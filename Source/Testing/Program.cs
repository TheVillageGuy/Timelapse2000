using RimworldRendererMod.Common;
using RimworldRendererMod.CommonV3;
using System;
using System.Diagnostics;
using System.Threading;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            Test2();
        }

        private static void Test2()
        {
            NetClient client = null;
            NetServer server = null;

            Thread serverThread = new Thread(() =>
            {
                server = new NetServer();
                server.Run(7777);
            });
            serverThread.Name = "Server thread";
            Thread clientThread = new Thread(() =>
            {
                client = new NetClient();
                Thread runThread = new Thread(() =>
                {
                    client.Run(7777);
                });
                runThread.Name = "Client run thread.";
                runThread.Start();
                Thread.Sleep(1500);
                client.Write(new NetData().Write("Hey there server! I am the client."));
            });
            clientThread.Name = "Client thread";

            serverThread.Start();
            clientThread.Start();

            Console.WriteLine("Press any key to shutdown...");
            Console.ReadKey();

            server.Shutdown();
            client.Shutdown();
            Console.ReadKey();
        }

        private static void Test1()
        {
            ServerConnection server = new ServerConnection("Rimworld_Renderer_Mod_Pipeline");

            server.UponMessage = (data) =>
            {
                if (data.ID == DataID.Connected)
                {
                    Console.WriteLine("Client connected! Telling them to start.");
                    server.Write(DataID.Start, "Go!");
                }
                if (data.ID == DataID.Error)
                {
                    Console.WriteLine("Client had an error and cannot start: " + data.Info);
                    server.Dispose();
                }
                if (data.ID == DataID.Done)
                {
                    Console.WriteLine("Client has finished rendering!");
                    server.Dispose();
                }
                if (data.ID == DataID.Update)
                {
                    string[] split = data.Info.Split(',');
                    string status = split[0];
                    float percentage = float.Parse(split[1]);
                    string timeLeft = split[2];

                    Console.WriteLine($"UPD:\nStatus: {status}\nProgress: {percentage * 100f:F0}%\nETA: {timeLeft}");
                }
            };

            server.StartRead();

            Stopwatch timeout = new Stopwatch();
            timeout.Start();
            while (server.IsReading && !server.IsConnected)
            {
                Thread.Sleep(5);
                if (timeout.Elapsed.TotalMilliseconds > 6000)
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
