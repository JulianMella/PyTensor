# PyTensor - Unity based tool to create a 3 dimensional space with obstacles
<p align="center">
  <img src="https://github.com/JulianMella/PyTensor/blob/main/Scene.png?raw=true" width="200" height="200">
  <img src="https://github.com/JulianMella/PyTensor/blob/main/Scene2.png?raw=true" width="200" height="200">
</p>

3 Dimensional Extension of [PyMatrix](https://github.com/julianmella/PyMatrix)

## How to use this tool

You need [Unity](https://unity.com/products/unity-engine)

Once downloaded, open the project.

Start Play Mode

Go to Window -> UI Toolkit -> PyTensor.

Select the dimensions you want (Note: This generates a cube where the outer walls are invisible.)

Generate it.

## Adding obstacles

Scroll to view different Y-level slices.

Left click to add or remove obstacle (Can hold too).

## Adding start and end points
Right click to select radius mode

Select origo by left clicking any sphere. (Start point)

Scroll to generate a hollow voxel sphere.

Left click any to mark an end point.

### Why select end points this way?
Such that all end points are equidistant from origo and results in some fairness for path finding algorithms


## Storing data
Select a save path in UI

Write name (without .py extension)

Export.



## Notes
Keep in mind that the python file that is generated is only a set of all the cubes within the boundaries and not the boundaries themselves. This design choice was due to the use case for a University Exam I have.

Bugs are to be expected.

This tool has severe performance issues. When initializing a 100x100x100 space, all calls to the GPU are done from the CPU, which is very costly!

To fix this, I will create a new repo with a GPU instanced based solution called PyTensor-GPUInstanced soon.

### About Assets/SphereCoordinates
---
A slight drawback that is immediately noticeable is the size of this project. The SphereCoordinates directory is the main reason for that. The Radius mode tool required a precise calculation of a hollow sphere, and my weak maths didn’t let me visualize the solution. I circumvented the issue by quickly vibe coding the [VoxelSphereHollow](https://github.com/JulianMella/VoxelSphereHollow) tool. It generates the outer coordinates of a perfect voxel sphere with values normalized to origo.

Either way, I found out that WorldEdit is open source and I extracted the algorithm they utilize. If I ever implement it, it would be interesting to profile the performance of both solutions.

---
###### git usage style
I am implementing things on an iterative way with git hunks, so only main branch exists, but features, fixes and such are atomically pushed, so it should be fine :D 
