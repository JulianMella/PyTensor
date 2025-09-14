# PyTensor

3 Dimensional Extension of PyMatrix

With this tool, you can select a height, width and length which in turn spawns in a h x w x l tensor with spheres. Each sphere is clickable and turned into a cube if clicked.

Once clicked, that cube is considered a hindrance, such that when you export the tensor to a python file, all hindrances will be stored in the form of a tuple (x, y, z) within a set.

This tool has severe performance issues. When initializing a 100x100x100 space, all calls to the GPU are done from the CPU, which is very costly!

To fix this, I will create a new repo with a GPU instanced based solution called PyTensor-GPUInstanced soon.
