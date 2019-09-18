using RimworldRendererMod.AppConnection;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Verse;

namespace RimworldRendererMod
{
    public static class Runner
    {
        public static bool IsRendering { get; private set; }
        public static ThreadedConnection Connection;

        public static void StartRender()
        {
            if (IsRendering)            
                return;

            try
            {
                IsRendering = true;
                string executable = Path.Combine(Path.Combine(RimworldRendererMod.BaseFolder, "Executables"), "RemoteRenderer.exe");

                Log.Message($"Running {executable}...");
                UI_Dialog.Status = "Running {executable}...";
                var prc = Process.Start(executable, @"E:\RimworldRenders\small E:\RimworldRenders\Output.mp4 1920 1080 20000000 Default 10 1 HighQualityBicubic");

                UI_Dialog.Status = "Waiting to establish connection from renderer...";
                Connection = new ThreadedConnection(new ServerConnection());
                Connection.UponReadConnect = () =>
                {
                    UI_Dialog.Status = "Established renderer connection!";
                    Connection.Write(DataID.Start, "Go");
                };
                Connection.UponRecieve = (data) =>
                {
                    switch (data.ID)
                    {
                        case DataID.Error:
                            Log.Message("Renderer reported error: " + data.Info);
                            UI_Dialog.Status = "[ERROR] " + data.Info;

                            Connection.Dispose();
                            Connection = null;
                            IsRendering = false;

                            break;

                        case DataID.Done:

                            Log.Message("Renderer remote is now closing, render complete!");
                            UI_Dialog.Status = "Done! Render complete.";

                            Connection.Dispose();
                            Connection = null;
                            IsRendering = false;

                            break;

                        case DataID.Update:

                            UI_Dialog.Status = data.Info;

                            break;

                        default:
                            Log.Message($"Unknown data ID reached renderer server: {data.ID}.");
                            break;
                    }
                };

                Connection.StartRead();
            }
            catch (Exception e)
            {
                UI_Dialog.Status = "Error: " + e;
                File.WriteAllText(@"C:\Users\spain\Desktop\ErrorLog.txt", e.GetType().FullName + "\n" + e.Message + "\n" + e.StackTrace);
                IsRendering = false;
            }
        }
    }
}
