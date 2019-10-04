using RimworldRendererMod.CommonV3;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Verse;

namespace RimworldRendererMod
{
    public static class Runner
    {
        public static bool IsRendering { get; private set; }
        public static NetServer Server;
        public static string SavePath { get; private set; }

        private static Thread thread;

        public static void StartRender()
        {
            if (IsRendering)            
                return;

            IsRendering = true;
            thread = new Thread(RunThread);
            thread.Name = "Renderer connection thread";
            thread.Start();
        }

        private static Process RunRenderer(string source, string dest, int w, int h, int bitrate, VideoCodec codec, int imagesPerSecond, int framesPerImage, InterpolationMode interpolation)
        {
            string executable = Path.Combine(Path.Combine(RimworldRendererMod.BaseFolder, "Executables"), "RemoteRenderer.exe");

            Log.Message($"Running {executable}...");
            UI_Dialog.Status = $"Running {executable}...";
            var prc = Process.Start(executable, $"\"{source}\" \"{dest}\" {w} {h} {bitrate} {codec} {imagesPerSecond} {framesPerImage} {interpolation}");
            return prc;
        }

        private static string GetDefaultOutputFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RimWorld Renders");
        }

        private static string GetOutputVideoName(string name, string extension)
        {
            // name = "large";
            // extension = ".mp4";

            string basePath = RimworldRendererMod.Settings.DefaultSaveLocation;

            int counter = 0;
            while(true)
            {
                string output = Path.Combine(basePath, name + (counter == 0 ? "" : $" ({counter})") + extension);
                counter++;

                if (!File.Exists(output))
                    return output;
                if (counter == 10000)
                {
                    Log.Error($"Reached file {counter}, but all of these files exist!?! How many videos did you render, you animal?!?");
                    return null;
                }
            }
        }

        private static void SetSavePath(string sourceDir)
        {
            SavePath = GetOutputVideoName(new DirectoryInfo(sourceDir).Name, RimworldRendererMod.Settings.FileExtension);
        }

        private static int WorkOutFramesPerImage(int imagesPerSecond, out int newImagesPerSecond)
        {
            if (imagesPerSecond >= 10)
            {
                newImagesPerSecond = imagesPerSecond;
                return 1;
            }
            // From now on, try to keep the framerate at 10 fps.
            if(imagesPerSecond == 5)
            {
                newImagesPerSecond = 10;
                return 2;
            }
            if (imagesPerSecond == 2)
            {
                newImagesPerSecond = 10;
                return 5;
            }

            // Could't optimize without changing video length...
            newImagesPerSecond = imagesPerSecond;
            return 1;
        }

        private static void RunThread()
        {
            try
            {
                UI_Dialog.Status = "Launching renderer executable...";
                int imagesPerSecond;
                int framesPerImage = WorkOutFramesPerImage(UI_Dialog.ImagesPerSecond, out imagesPerSecond);

                if (framesPerImage * imagesPerSecond != UI_Dialog.ImagesPerSecond)
                {
                    float oldTime = UI_Dialog.ImagesPerSecond;
                    float newTime = imagesPerSecond / framesPerImage;
                    float ratio = newTime / oldTime;
                    float change = ratio - 1f;

                    string changeStr = $"{change * 100f:F0}%";
                    string msg = $"Due to the low images per second ({UI_Dialog.ImagesPerSecond}), {framesPerImage} frames will be rendered per image, at a framerate of {imagesPerSecond} (the goal is at least 10fps). Due to this, the video file will be {changeStr} {(change >= 0f ? "longer" : "shorter")} than expected.";
                    Log.Warning(msg);
                }

                Log.Message($"Running renderer:\nSource: {UI_Dialog.SourceFolder}\nOutput: {GetOutputVideoName(UI_Dialog.SourceFolder, ".mp4")}\nResolution: {UI_Dialog.ResX}x{UI_Dialog.ResY}\nBitrate: {UI_Dialog.Bitrate}\nCodec: {UI_Dialog.CurrentCodec}\nInterpolation: {UI_Dialog.CurrentInterpolationMode}\nFramerate: {imagesPerSecond} ({framesPerImage} frames per image for {(1f / imagesPerSecond) * framesPerImage:F2} seconds per image)");

                RunRenderer(UI_Dialog.SourceFolder, GetOutputVideoName(UI_Dialog.SourceFolder, ".mp4"), UI_Dialog.ResX, UI_Dialog.ResY, UI_Dialog.Bitrate, UI_Dialog.CurrentCodec, imagesPerSecond, framesPerImage, UI_Dialog.CurrentInterpolationMode);

                UI_Dialog.Status = "Creating connection...";
                Server = new NetServer();
                Server.UponMessage = (data) =>
                {
                    byte ID = data.ReadByte();
                    switch (ID)
                    {
                        case NetData.READY:
                            UI_Dialog.Status = "Established connection! Starting render...";
                            Log.Message("Established renderer connection, starting render...");
                            Server.Write(new NetData().Write(NetData.START).Write("Go!"));
                            break;

                        case NetData.ERROR:
                            string error = data.ReadString();
                            UI_Dialog.Status = "Error! " + error;
                            Log.Message($"Renderer error: {error}.");
                            Server.Shutdown();
                            break;

                        case NetData.UPDATE:
                            string[] split = data.ReadString().Split(',');
                            if (split != null && split.Length == 3)
                            {
                                UI_Dialog.Status = split[0];
                                UI_Dialog.ProgressBarPercentage = float.Parse(split[1]);
                                UI_Dialog.ETA = split[2];
                            }
                            break;

                        case NetData.DONE:
                            UI_Dialog.Status = "Done! " + data.ReadString().Trim();
                            Server.Shutdown();
                            break;

                        default:
                            Log.Message($"Unhandled data type from pipeline {ID}. Info: {data.ReadString() ?? "null"}");
                            break;
                    }
                };

                // Inner thread that runs the read loop.
                UI_Dialog.Status = "Waiting to establish connection...";
                Thread innerThread = new Thread(() =>
                {
                    Server.Run(7171);
                });
                innerThread.Name = "Renderer connection inner thread";
                innerThread.Start();

                Thread.Sleep(10);

                // Timeout...
                Stopwatch timeout = new Stopwatch();
                timeout.Start();
                while (!Server.Connected)
                {
                    Thread.Sleep(5);
                    if (timeout.Elapsed.TotalMilliseconds > 6000)
                    {
                        UI_Dialog.Status = "Renderer failed to connect. Are you running 64 bit windows?";
                        Log.Message("Client never connected to us. Timed out. Exiting...");
                        Server.Shutdown();
                        break;
                    }
                }
                timeout.Stop();

                // Wait until connnection closes.
                while (Server.Connected)
                {
                    Thread.Sleep(5);
                }

                Log.Message("Runner connection thread is now exiting...");
            }
            catch(Exception e)
            {
                UI_Dialog.Status = $"Renderering failed! Exception: {e.GetType().Name}. More info in log.";
                Log.Warning(e.ToString());                
            }
            finally
            {
                UI_Dialog.ETA = "---";
                UI_Dialog.ProgressBarPercentage = 0f;
                IsRendering = false;
                if(Server != null)
                {
                    Server.Shutdown();
                    Server = null;
                    thread = null;
                }
            }
        }
    }
}
