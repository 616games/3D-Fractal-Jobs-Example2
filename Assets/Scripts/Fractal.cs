using UnityEngine;

public class Fractal : MonoBehaviour
{
    #region --Fields / Properties--
    
    /// <summary>
    /// The number of layers in the entire fractal.
    /// </summary>
    [SerializeField, Range(1, 8), Tooltip("Any value above 8 will crash Unity.")]
    private int _depth = 4;

    /// <summary>
    /// Specifies how many additional fractal parts per individual fractal part (children) should be created.
    /// </summary>
    [SerializeField]
    private int _numberOfChildFractals = 5;

    /// <summary>
    /// The mesh used for each fractal part.
    /// </summary>
    [SerializeField]
    private Mesh _mesh;

    /// <summary>
    /// The material used for each fractal part.
    /// </summary>
    [SerializeField]
    private Material _material;

    /// <summary>
    /// Controls the rotation speed of each fractal part.
    /// </summary>
    [SerializeField, Range(0.0f, 50.0f)]
    private float _rotationSpeed = 25f;

    /// <summary>
    /// Cached Transform component.
    /// </summary>
    private Transform _transform;

    /// <summary>
    /// Contains all the relevant directions used for the fractal parts.
    /// </summary>
    private Vector3[] _directions =
    {
        Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3. back
    };

    /// <summary>
    /// Contains all the relevant rotations used for the fractal parts.
    /// </summary>
    private Quaternion[] _rotations =
    {
        Quaternion.identity, Quaternion.Euler(0, 0, -90f), Quaternion.Euler(0, 0, 90f), Quaternion.Euler(90f, 0, 0),
        Quaternion.Euler(-90f, 0, 0)
    };

    /// <summary>
    /// Each depth layer gets its own array of fractal parts to keep the Hierarchy organized.
    /// </summary>
    private FractalPart[][] _fractalParts;
    
    #endregion
    
    #region --Structs--

    /// <summary>
    /// Used to keep track of each fractal part's relevant data without having to instantiate new instances of the Fractal class.
    /// Allows us to process Update callbacks per fractal part individually in just this class (Fractal).
    /// Helps improve performance compared to using separate instances of the Fractal class processing its own Update callback.
    /// </summary>
    private struct FractalPart
    {
        public Vector3 direction;
        public Quaternion rotation;
        public Transform transform;
    }
    
    #endregion
    
    #region --Unity Specific Methods--

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        UpdateFractalParts();
    }
    
    #endregion
    
    #region --Custom Methods--

    /// <summary>
    /// Initializes variables and caches components.
    /// </summary>
    private void Init()
    {
        if(transform != null && _transform != transform) _transform = transform;
        
        CreateFractalPartsArray();
        CreateFractalParts();
    }

    /// <summary>
    /// Assigns fractal parts to their appropriate depth layers in the _fractalParts array.
    /// </summary>
    private void CreateFractalPartsArray()
    {
        _fractalParts = new FractalPart[_depth][];
        
        //First layer (0) only has one fractal part and no children.
        _fractalParts[0] = new FractalPart[1];
        
        //Each remaining fractal part has _numberOfChildFractals spawned with it.
        //Therefore, each subsequent layer has _numberOfChildFractals times more fractal parts than the previous layer.
        for (int i = 0, _arrayLength = 1; i < _fractalParts.Length; i++, _arrayLength *= _numberOfChildFractals)
        {
            _fractalParts[i] = new FractalPart[_arrayLength];
        }
    }
    
    /// <summary>
    /// Creates a fractal part as a separate game object.
    /// </summary>
    private FractalPart CreateFractalPart(int _layerIndex, int _childIndex, float _scale)
    {
        GameObject _fractalPart = 
            new GameObject("Fractal Part - Layer " + _layerIndex + " - Child Index " + _childIndex);
        _fractalPart.transform.SetParent(_transform, false);
        _fractalPart.AddComponent<MeshFilter>().mesh = _mesh;
        _fractalPart.AddComponent<MeshRenderer>().material = _material;
        _fractalPart.transform.localScale = _scale * Vector3.one;

        return new FractalPart
               {
                   direction = _directions[_childIndex],
                   rotation = _rotations[_childIndex],
                   transform = _fractalPart.transform
               };
    }

    /// <summary>
    /// Create all the individual fractal parts and place them in their appropriate array indices.
    /// </summary>
    private void CreateFractalParts()
    {
        //Create only one fractal part for the first layer (0) with scale 1.
        float _scale = 1f;
        _fractalParts[0][0] = CreateFractalPart(0, 0, _scale);

        //Start on the second layer (1) and iterate through the rest, halving the scale of the fractal parts of each layer.
        for (int _layerIndex = 1; _layerIndex < _fractalParts.Length; _layerIndex++)
        {
            _scale *= .5f;
            FractalPart[] _layerParts = _fractalParts[_layerIndex];
            
            //Create the fractal parts at each layer.
            for (int _fractalPartIndex = 0; _fractalPartIndex < _layerParts.Length; _fractalPartIndex += _numberOfChildFractals)
            {
                //Create the additional child fractals per individual fractal part.
                for (int _childIndex = 0; _childIndex < _numberOfChildFractals; _childIndex++)
                {
                    _layerParts[_fractalPartIndex + _childIndex] =  CreateFractalPart(_layerIndex, _childIndex, _scale);
                }
            }
        }
    }

    /// <summary>
    /// Iterate through every layer starting with layer 2 (index 1) and every fractal part to update its FractalPart struct variables.
    /// </summary>
    private void UpdateFractalParts()
    {
        //Apply rotation over time.
        Quaternion _deltaRotation = Quaternion.Euler(0, _rotationSpeed * Time.deltaTime, 0);
        
        //Update the root fractal part's rotation first and apply that rotation to its transform.
        //Remember that since FractalPart is a struct, we have to get a copy of the existing FractalPart, modify it, then replace it.
        FractalPart _rootFractal = _fractalParts[0][0];
        _rootFractal.rotation *= _deltaRotation;
        _rootFractal.transform.localRotation = _rootFractal.rotation;
        _fractalParts[0][0] = _rootFractal;
        
        for (int _layerIndex = 1; _layerIndex < _fractalParts.Length; _layerIndex++)
        {
            //Get the parent fractal part of the current fractal part.
            FractalPart[] _parentParts = _fractalParts[_layerIndex - 1];
            
            FractalPart[] _layerParts = _fractalParts[_layerIndex];
            for (int _fractalPartIndex = 0; _fractalPartIndex < _layerParts.Length; _fractalPartIndex++)
            {
                Transform _parentTransform = _parentParts[_fractalPartIndex / _numberOfChildFractals].transform;
                FractalPart _part = _layerParts[_fractalPartIndex];

                //Update current fractal part's rotation to use the _deltaRotation.
                _part.rotation *= _deltaRotation;
                
                //Adjust the current fractal part's rotation taking into account the parent's rotation and the _deltaRotation.
                //The order of multiplying quaternions matters.
                //In our Hierarchy, the transformation would occur first by the child and then by the parent.
                //Therefore, we must multiply rotations in this order:  parent - child.
                _part.transform.localRotation = _parentTransform.localRotation * _part.rotation;
                
                //Set the current fractal parts position relative to its parent's transform.
                //We need to account for the following:
                //**Current fractal part's scaled size being halved
                //**The parent's position
                //**The parent's rotation
                //**The current fractal part's direction.
                _part.transform.localPosition = _parentTransform.localPosition + _parentTransform.localRotation *
                                                (1.5f * _part.transform.localScale.x * _part.direction);

                //Now replace the current FractalPart struct with the one we just modified.
                _layerParts[_fractalPartIndex] = _part;
            }
        }
    }
    
    #endregion
    
}