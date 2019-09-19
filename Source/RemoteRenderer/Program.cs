using Accord.Video.FFMPEG;
using RimworldRendererMod.Common;
using RimworldRendererMod.RemoteRenderer.IO;
using System;
using System.Drawing.Drawing2D;
using System.Threading;

namespace RimworldRendererMod.RemoteRenderer
{
    public static class Program
    {
        private static bool IsRendering = false;
        private static ClientConnection Connection;
        private static Renderer Renderer;

        private static string[] files;
        private static string images;
        private static string output;
        private static int resX;
        private static int resY;
        private static int bitRate;
        private static VideoCodec codec;
        private static int framerate;
        private static int framesPerImage;
        private static InterpolationMode interpolation;

        public static void Main(string[] args)
        {
            /*
             * 0. Images dir.
             * 1. Output path.
             * 2. ResX
             * 3. ResY
             * 4. Bitrate
             * 5. Codec string
             * 6. Framerate
             * 7. FramesPerImage
             * 8. Interpolation mode string
             */

            if(args.Length != 9)
            {
                Console.WriteLine($"Expected 9 args, got {args.Length}.");
                Console.ReadKey();
                return;
            }

            images = args[0];
            output = args[1];
            resX = int.Parse(args[2]);
            resY = int.Parse(args[3]);
            bitRate = int.Parse(args[4]);
            codec = (VideoCodec)Enum.Parse(typeof(VideoCodec), args[5]);
            framerate = int.Parse(args[6]);
            framesPerImage = int.Parse(args[7]);
            interpolation = (InterpolationMode)Enum.Parse(typeof(InterpolationMode), args[8]);

            Console.WriteLine($"Images dir: {images}");
            Console.WriteLine($"Output: {output}");
            Console.WriteLine($"ResX: {resX}");
            Console.WriteLine($"ResY: {resY}");
            Console.WriteLine($"Bitrate: {bitRate}");
            Console.WriteLine($"Codec: {codec}");
            Console.WriteLine($"Framerate: {framerate}");
            Console.WriteLine($"Frames per image: {framesPerImage}");
            Console.WriteLine($"Interpolation: {interpolation}");

            Run();

#if DEBUG
            Console.ReadKey();
#endif
        }

        private static void Run()
        {
            const int SLEEP = 100;
            Console.WriteLine($"Hello world! Waiting {SLEEP}ms to allow for program to register and set up.");
            Thread.Sleep(SLEEP);

            RunClient();

            // Loop and send updates when necessary.
            while (Connection != null && Connection.IsConnected)
            {
                if (IsRendering)
                {
                    SendUpdate();
                    Thread.Sleep(100);
                }
            }
        }

        private static void RunClient()
        {
            Connection = new ClientConnection("Rimworld_Renderer_Mod_Pipeline");
            Connection.UponMessage = UponMessage;
            Connection.Connect();

            if (!Connection.IsConnected)
            {
                Console.WriteLine("Failed to establish connection to process runner. Stopping...");
                Thread.Sleep(2000);
                Environment.Exit(1);
            }


            string error = CheckReady();
            if(error != null)
            {
                Console.WriteLine($"Error, cannot render: {error}");
                Connection.Write(DataID.Error, error);
                Shutdown();
                return;
            }
            else
            {
                Connection.StartRead();
                Connection.Write(DataID.Connected, "Ready.");

            }
        }

        private static string CheckReady()
        {
            files = FileIO.GetAllFilesSorted(images, "*.png", "*.jpg");
            if (files == null || files.Length < 2)
            {
                return $"Needs at least two images to render! Found {files.Length}.";
            }

            return null;
        }

        private static void UponMessage(ConnectionData data)
        {
            switch (data.ID)
            {
                case DataID.Start:

                    if (IsRendering)
                        break;

                    Console.WriteLine("Server has told client to start. Running...");
                    IsRendering = true;                    

                    Renderer = new Renderer(output, files);
                    Renderer.Width = resX;
                    Renderer.Height = resY;
                    Renderer.Bitrate = bitRate;
                    Renderer.Codec = codec;
                    Renderer.FrameRate = framerate;
                    Renderer.FramesPerImage = framesPerImage;
                    Renderer.InterpolationMode = interpolation;

                    Renderer.Done = () =>
                    {
                        IsRendering = false;
                        Console.WriteLine("Finished render!");
                        Connection.Write(DataID.Done, "All done! Render complete.");
                    };

                    Renderer.StartRender();

                    break;

                default:
                    Console.WriteLine($"[ERROR] Unexpected data ID sent to client: {data.ID}.");
                    break;
            }
        }

        private static void Shutdown()
        {
            if(Connection != null)
            {
                Connection.Dispose();
                Connection = null;
            }
        }

        private static string currentStatus;
        private static float currentPercentage;
        private static string currentETA;

        internal static void SetStatusSafe(string status)
        {
            currentStatus = status;
        }

        internal static void SetProgressSafe(float percentage)
        {
            currentPercentage = percentage;
        }

        internal static void SetEstimatedTimeSafe(string time)
        {
            currentETA = time;
        }

        private static void SendUpdate()
        {
            Connection.Write(DataID.Update, $"{currentStatus},{currentPercentage},{currentETA}");
        }
    }
}
