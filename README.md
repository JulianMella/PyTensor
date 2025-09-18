# PyTensor - Unity based tool to create a 3 dimensional space with obstacles
<p align="center">
  <img src="https://github.com/JulianMella/PyTensor/blob/main/Scene.png?raw=true" width="200" height="200">
</p>

3 Dimensional Extension of [PyMatrix](https://github.com/julianmella/PyMatrix)

This tool gives you a user interface under Window -> UI Toolkit -> PyTensor, which when utilized, creates a 3D tensor.

The tensor consists of spheres. The spheres are clickable, if clicked it turns into a cube which marks it as an obstacle within the course.

Since the tensor can be very large, the scroll wheel has functionality to work on an individual layer basis such that only spheres for one layer are shown, whilst everything above is hidden and every layer below only shows the obstacles.

Once satisfied with the results, go back to the PyTensor UI, select the path where you want to store the Tensor data, give it a file name (without .py or any file extension at the end) and press the "Export to python set" button.

Keep in mind that the python file that is generated is only a set of all the cubes wthin the boundaries and not the boundaries themselves. This design choice was due to the use case for a University Exam I have.

Bugs are to be expected.

This tool has severe performance issues. When initializing a 100x100x100 space, all calls to the GPU are done from the CPU, which is very costly!

To fix this, I will create a new repo with a GPU instanced based solution called PyTensor-GPUInstanced soon.

---
###### note on git usage style
I am implementing things on an iterative way with git hunks, so only main branch exists, but features, fixes and such are atomically pushed, so it should be fine :D 
