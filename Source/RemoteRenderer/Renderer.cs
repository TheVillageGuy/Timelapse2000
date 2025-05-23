﻿using Accord.Video.FFMPEG;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;

namespace RimworldRendererMod.RemoteRenderer
{
    public class Renderer
    {
        public string[] Images { get; }
        public string OutputDir { get; }

        public int FrameRate = 24;
        public int FramesPerImage = 24 / 4;
        public int Width = 3840, Height = 2160;
        public int Bitrate = 1000 * 1000;
        public VideoCodec Codec = VideoCodec.MPEG4;
        public InterpolationMode InterpolationMode = InterpolationMode.Bicubic;

        public Action Done;

        private Thread thread;

        public Renderer(string output, string[] images)
        {
            this.Images = images ?? throw new ArgumentNullException("images", "Images array cannot be null.");
            this.OutputDir = output ?? throw new ArgumentNullException("output", "Output path cannot be null.");
        }

        public void StartRender()
        {
            if (thread != null)
                return;

            thread = new Thread(RunRender);
            thread.Name = "Render Thread";
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
        }

        private void RunRender()
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                using (var writer = new VideoFileWriter())
                {
                    string toCreate = new FileInfo(OutputDir).Directory.FullName;
                    if (!Directory.Exists(toCreate))
                        Directory.CreateDirectory(toCreate);

                    writer.Open(OutputDir, Width, Height, FrameRate, Codec, Bitrate);

                    for (int i = 0; i < Images.Length; i++)
                    {
                        watch.Stop();
                        watch.Reset();
                        watch.Start();

                        string path = Images[i];
                        string name = new FileInfo(path).Name;

                        Program.SetStatusSafe($"Loading {name}...");
                        Bitmap bitmap = new Bitmap(new FileStream(path, FileMode.Open));

                        Program.SetStatusSafe($"Resizing {name}...");
                        Bitmap frame;
                        if (bitmap.Width == Width && bitmap.Height == Height)
                        {
                            frame = bitmap;
                        }
                        else
                        {
                            frame = ResizeAndCenter(bitmap, out bool resized);
                            if (resized)
                                bitmap.Dispose();
                        }

                        for (int j = 0; j < FramesPerImage; j++)
                        {
                            Program.SetStatusSafe($"Writing {name}: Frame {j + 1} of {FramesPerImage}.");
                            writer.WriteVideoFrame(frame);
                        }

                        frame.Dispose();

                        Program.SetProgressSafe((float)(i + 1) / Images.Length);

                        System.GC.Collect();

                        watch.Stop();
                        var elapsed = watch.Elapsed;
                        int remaing = Images.Length - (i + 1);
                        TimeSpan sum = TimeSpan.Zero;
                        for (int j = 0; j < remaing; j++)
                        {
                            sum = sum.Add(elapsed);
                        }

                        Program.SetEstimatedTimeSafe(sum.ToString(@"hh\:mm\:ss"));
                    }

                    writer.Close();
                }

                Done?.Invoke();
            }
            catch (Exception e)
            {
                Program.ErrorShutdown("Unhandled rendering algorithm exception: " + e.ToString());
            }
        }

        private Bitmap ResizeAndCenter(Bitmap original, out bool didResize)
        {
            // Don't resize if we don't need to!
            if (original.Width == Width && original.Height == Height)
            {
                didResize = false;
                return original;
            }

            Bitmap resized = new Bitmap(Width, Height);

            var frame = ScaleToFit(original.Width, original.Height, Width, Height);

            using (Graphics g = Graphics.FromImage(resized))
            {
                g.Clear(Color.Black);
                g.InterpolationMode = InterpolationMode;
                int sw = original.Width;
                int sh = original.Height;
                g.DrawImage(original, new Rectangle(frame.OffsetX, frame.OffsetY, frame.Width, frame.Height), new Rectangle(0, 0, sw, sh), GraphicsUnit.Pixel);
            }


            didResize = true;
            return resized;
        }

        private FrameBounds ScaleToFit(int width, int height, int containerWidth, int containerHeight)
        {
            int fw = 0;
            int fh = 0;
            int ox = 0;
            int oy = 0;

            bool containerWide = containerWidth >= containerHeight;
            bool imageWide = width >= height;
            int containerMin = containerWide ? containerWidth : containerHeight;

            float wScale = ScaleToFit(width, containerWidth);
            float hScale = ScaleToFit(height, containerHeight);

            // First fit x.
            fw = containerWidth;
            fh = round(height * wScale);
            ox = 0;
            oy = (containerHeight - fh) / 2;

            // Check if y goes out of bounds, and scale to y if it does.
            if (oy < 0)
            {
                fw = round(width * hScale);
                fh = containerHeight;
                ox = (containerWidth - fw) / 2;
                oy = 0;
            }

            return new FrameBounds()
            {
                Width = fw,
                Height = fh,
                OffsetX = ox,
                OffsetY = oy
            };

            int round(float x)
            {
                return (int)Math.Ceiling(x);
            }
        }

        private struct FrameBounds
        {
            public int Width, Height, OffsetX, OffsetY;
        }

        private float ScaleToFit(int start, int end)
        {
            float scale = (float)end / start;
            return scale;
        }
    }
}
