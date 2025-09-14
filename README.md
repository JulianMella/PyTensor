# PyTensor - Unity based tool to create a 3 dimensional space with obstacles

3 Dimensional Extension of [PyMatrix](https://github.com/julianmella/PyMatrix)

This tool gives you a user interface under Window -> UI Toolkit -> PyTensor, which when utilized, creates a 3D tensor.

The tensor consists of spheres. The spheres are clickable, if clicked it turns into a cube which marks it as an obstacle within the course.

Since the tensor can be very large, the scroll wheel has functionality to work on an individual layer basis such that only spheres for one layer are shown, whilst everything above is hidden and every layer below only shows the obstacles.

Once satisfied with the results, go back to the PyTensor UI, select the path where you want to store the Tensor data, give it a file name (without .py or any file extension at the end) and press the "Export to python set" button.

Keep in mind that the python file that is generated is only a set of all the cubes wthin the boundaries and not the boundaries themselves. This design choice was due to the use case for a University Exam I have.

Bugs are to be expected.

This tool has severe performance issues. When initializing a 100x100x100 space, all calls to the GPU are done from the CPU, which is very costly!

To fix this, I will create a new repo with a GPU instanced based solution called PyTensor-GPUInstanced soon.

TODO: Change naming from Matrix to Tensor where appropriate within the scripting code. I did an oopsie and did not think about proper terminology within the code before it was too late. So now I have to go and refactor a bunch, which honestly i cant be bothered with right now since this is not the final solution anyways.
