using RimworldRendererMod.Common;
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
        public static ServerConnection Connection;

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
            UI_Dialog.Status = "Running {executable}...";
            var prc = Process.Start(executable, @"C:\Users\James\Pictures\ToRender C:\Users\James\Pictures\ToRender\Output.mp4 1920 1080 5000000 Default 10 5 HighQualityBicubic");
            return prc;
        }

        private static void RunThread()
        {
            try
            {
                UI_Dialog.Status = "Launching renderer executable...";
                RunRenderer();

                UI_Dialog.Status = "Creating connection pipeline...";
                Connection = new ServerConnection("Rimworld_Renderer_Mod_Pipeline");
                Connection.UponMessage = (data) =>
                {
                    switch (data.ID)
                    {
                        case DataID.Connected:
                            UI_Dialog.Status = "Established connection! Starting render...";
                            Log.Message("Established renderer connection, starting render...");
                            Connection.Write(DataID.Start, "Go!");
                            break;

                        case DataID.Error:
                            UI_Dialog.Status = "Error! " + data.Info;
                            Log.Message($"Renderer error: {data.Info}.");
                            Connection.Dispose();
                            break;

                        case DataID.Update:
                            string[] split = data.Info.Split(',');
                            if(split != null && split.Length == 3)
                            {
                                UI_Dialog.Status = split[0];
                                UI_Dialog.ProgressBarPercentage = float.Parse(split[1]);
                                UI_Dialog.ETA = split[2];
                            }
                            break;

                        case DataID.Done:
                            UI_Dialog.Status = "Done! " + data.Info.Trim();
                            break;

                        default:
                            Log.Message($"Unhandled data type from pipeline {data.ID}. Info: {data.Info}");
                            break;
                    }
                };

                UI_Dialog.Status = "Waiting to establish connection...";
                Connection.StartRead();

                Stopwatch timeout = new Stopwatch();
                timeout.Start();
                while (Connection.IsReading && !Connection.IsConnected)
                {
                    Thread.Sleep(5);
                    if (timeout.Elapsed.TotalMilliseconds > 6000)
                    {
                        UI_Dialog.Status = "Renderer failed to connect. Are you running 64 bit windows?";
                        Log.Message("Client never connected to us. Timed out. Exiting...");
                        Connection.Dispose();
                        break;
                    }
                }
                timeout.Stop();

                while (Connection.IsReading)
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
                if(Connection != null)
                {
                    Connection.Dispose();
                    Connection = null;
                    thread = null;
                }
            }
        }
    }
}
