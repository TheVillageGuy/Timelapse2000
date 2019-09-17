using Accord.Video.FFMPEG;
using RimworldRendererMod.AppConnection;
using RimworldRendererMod.RemoteRenderer.IO;
using System;
using System.Drawing.Drawing2D;
using System.Threading;

namespace RimworldRendererMod.RemoteRenderer
{
    public static class Program
    {
        private static bool IsRendering = false;
        private static ThreadedConnection Connection;
        private static ThreadedConnection ServerConnection;
        private static Renderer Renderer;

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

            Console.ReadKey();
        }

        private static void Run()
        {
            const int SLEEP = 100;
            Console.WriteLine($"Hello world! Waiting {SLEEP}ms to allow for program to register and set up.");
            Thread.Sleep(SLEEP);


            RunServer();

            Thread.Sleep(50);

            RunClient();

            // TODO put me somewhere better.
            while (Connection.IsReading && !Connection.IsShuttingDown)
            {
                if (IsRendering)
                {
                    SendUpdate();
                    Thread.Sleep(100);
                }
            }
        }

        private static void RunServer()
        {
            ServerConnection = new ThreadedConnection(new ServerConnection());
            ServerConnection.UponReadConnect = () =>
            {
                Console.WriteLine("Upon connect.");
                ServerConnection.Write(DataID.Start, "Go!");

            };
            ServerConnection.UponRecieve = (data) =>
            {
                Console.WriteLine($"Server: [{data.ID}] {data.Info}");
            };
            ServerConnection.StartRead();
        }

        private static void RunClient()
        {
            Connection = new ThreadedConnection(new ClientConnection());
            Connection.UponRecieve = UponMessage;
            Connection.StartRead();

            int timeout = 5000;
            Console.WriteLine($"Attempting to connect to server. Timeout: {timeout}ms.");
            Connection.ClientConnection.Connect(timeout);

            if (!Connection.ClientConnection.Pipe.IsConnected)
            {
                Console.WriteLine("Failed to establish connection. Exiting.");
                return;
            }

            Console.WriteLine("Established connection. Notifying server and starting read.");
            Connection.Write(DataID.GoodToGo, "Ready.");
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

                    string[] files = FileIO.GetAllFilesSorted(images, "*.png", "*.jpg");
                    if(files == null || files.Length < 2)
                    {
                        Connection.Write(DataID.Error, $"Needs at least two images to render! Found {files.Length}.");
                        Shutdown();
                        break;
                    }

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

                case DataID.Stop:

                    if (!IsRendering)
                        break;

                    Console.WriteLine("Server has requested a cancel. Stopping...");
                    // TODO stop render.

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
