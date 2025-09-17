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


---
#### Reflections on how programming this has been
---

I have been watching vlogs of people complaining how long it takes to implement different stuff in the game. I get it, it takes time to some extent, but I think there are many ways to solve that feeling.

This task has been one which has required me to consistently pour code from my fingertips, deep thinking, research, understanding, visualization. Such a work methodology requires to work fast, many thoughts are occurring, you need to research different branches of knowledge and understanding, go back and forth between them, so you need to do it quick to keep up. Luckily, I have 135wpm average (thanks 2007 varrock trading). On the downside, messy ideas can sometimes be put in place and long nested restructuring, refactoring, and such is required. At times, you just need to take a decision and go with it. There are many edge cases to think about which can hinder you strongly if you sometimes don't decide to skip them! Refactoring is a good skill to learn anyways. 
