using RimworldRendererMod.AppConnection;
using System;
using System.Threading;

namespace RimworldRendererMod.RemoteRenderer
{
    public static class Program
    {
        private static bool IsRendering = false;

        public static void Main(string[] args)
        {
            Run();
        }

        private static void Run()
        {
            const int SLEEP = 100;
            Console.WriteLine($"Hello world! Waiting {SLEEP}ms to allow for program to register and set up.");
            Thread.Sleep(SLEEP);

            ThreadedConnection conn = new ThreadedConnection(new ClientConnection());
            int timeout = 5000;
            Console.WriteLine($"Attempting to connect to server. Timeout: {timeout}ms.");
            conn.ClientConnection.Connect(timeout);

            if (!conn.ClientConnection.Pipe.IsConnected)
            {
                Console.WriteLine("Failed to establish connection. Exiting.");
                return;
            }

            Console.WriteLine("Established connection. Notifying server and starting read.");
            conn.Write(DataID.GoodToGo, "Ready.");

            conn.UponRecieve = UponMessage;
            conn.StartRead();
        }

        private static void UponMessage(ConnectionData data)
        {
            switch (data.ID)
            {
                case DataID.Start:

                    Console.WriteLine("Server has told client to start. Running...");
                    IsRendering = true;
                    // TODO start render.

                    break;

                case DataID.Stop:

                    Console.WriteLine("Server has requested a cancel. Stopping...");
                    // TODO stop render.

                    break;

                default:
                    Console.WriteLine($"[ERROR] Unexpected data ID sent to client: {data.ID}.");
                    break;
            }
        }
    }
}
