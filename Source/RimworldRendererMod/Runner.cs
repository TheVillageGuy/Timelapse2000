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

        private static Process RunRenderer()
        {
            string executable = Path.Combine(Path.Combine(RimworldRendererMod.BaseFolder, "Executables"), "RemoteRenderer.exe");

            Log.Message($"Running {executable}...");
            UI_Dialog.Status = $"Running {executable}...";
            string source = @"E:\RimworldRenders\baseball cap";
            string dest = @"E:\RimworldRenders\Mod Output.mp4";
            var prc = Process.Start(executable, $"\"{source}\" \"{dest}\" 1920 1080 5000000 Default 10 5 HighQualityBicubic");
            return prc;
        }

        private static void RunThread()
        {
            try
            {
                UI_Dialog.Status = "Launching renderer executable...";
                RunRenderer();

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
