Heeey this is the readme for SassGame!
This project was the team effort of Oli Jung, Mark Borokowski and Arthur Hertz.
I (Arthur) wrote and integrated all of the level generation code, putting together the 3D noise, marching cubes, and engineering a chunking/streaming solution from scratch.
Mark has been working on shaders for the procedural terrain that work without UV maps at all, and Oli has been integrating the project with the WWise middleware and diving deep
into procedural sound design.

This project should just boot up and play, but if it's a little laggy I recommend you either build it or check out one of the release builds I already made in the repository.
They are much smoother and use incremental garbage collection to help with the world loading, although you won't get to see the chunk visibility gizmo I wrote.

If you have any questions on getting the project to run smoothly please email me at arthertz@iu.edu

Best,
Arthur


Attributions:
Scrawk's marching cubes implementation (MIT license)
https://github.com/Scrawk/Marching-Cubes

The LibNoise package w/ bindings for Unity (LGPL license)
https://github.com/ricardojmendez/LibNoise.Unity

Link to download WWise middleware (you probably won't need it):
https://www.audiokinetic.com/download/