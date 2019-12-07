# Rimworld Renderer Mod
Here is the source code for my Timelapse 2000 Mod, which is used to create timelapse videos from a sequence of images.
Integrates seamlessly into the Rimworld game. See 

## Settings Help
#### Resolution
The horizontal and vertical size, in pixels, that the output video file will have. If all of your input images are the same resolution, it sometimes makes sense to set the resolution to the same as the image's resolution. This makes rendering faster.
#### Sampling
This is the algorithm used to resize the input images so that they fit into the video frame. **I highly recommend leaving it on HighQualityBicubic**.
#### Codec
This is the algorithm used to encode the video file, and can be thought of as similar to file format. Again, **I highly recommend leaving it on MPEG4**.
*NOTE: You can rename the ouput video file from Output.avi to Output.mp4 and it will work just fine.*
#### Bitrate
The bitrate that the output video uses. A full explaination of this is beyond the scope of this documentation, but **if the ouput video ever looks choppy, or has compression artifacts increase this value**. In general, higher resolutions and framerates will require a higher bitrate. 20 mbps is a good value for everything from 1080p video to 4k video.
#### Images per second
The number of input images that will be played per second. This is completely up to you, and is not a technical setting. For example, if you stored one image per Rimworld day then you could have one second of video equal 10 Rimworld days by setting this value to 10. Setting this lower than 10 may result in a video that does not work very well in all media players.
