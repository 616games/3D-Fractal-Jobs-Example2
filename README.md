# 3D-Fractal-Jobs-Example2
 Continues from example1 to further optimize the existing code.

We removed the individual Update, Start, and CreateChildFractal methods to have only one game object at the root control all of the initialization and updating for the fractal game objects.  This helps cut down significantly on the number of calls to Update.

We also change the terminology from "fractal child" to "fractal part" since these are now treated as individual parts of the whole fractal.

Created in Unity 2020.3.11f1
