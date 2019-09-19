using RimworldRendererMod.Common;
using System;
using System.Diagnostics;
using System.Threading;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerConnection server = new ServerConnection("Rimworld_Renderer_Mod_Pipeline");

            server.UponMessage = (data) =>
            {
                if(data.ID == DataID.Connected)
                {
                    Console.WriteLine("Client connected! Telling them to start.");
                    server.Write(DataID.Start, "Go!");
                }
                if(data.ID == DataID.Error)
                {
                    Console.WriteLine("Client had an error and cannot start: " + data.Info);
                    server.Dispose();
                }
                if(data.ID == DataID.Done)
                {
                    Console.WriteLine("Client has finished rendering!");
                    server.Dispose();
                }
                if(data.ID == DataID.Update)
                {
                    Console.WriteLine("UPD: " + data.Info);
                }
            };

            server.StartRead();

            Stopwatch timeout = new Stopwatch();
            timeout.Start();
            while(server.IsReading && !server.IsConnected)
            {
                Thread.Sleep(5);
                if(timeout.Elapsed.TotalMilliseconds > 6000)
                {
                    Console.WriteLine("Client never connected to us. Timed out. Exiting...");
                    server.Dispose();
                    break;
                }
            }
            timeout.Stop();

            while (server.IsReading)
            {
                Thread.Sleep(5);
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
