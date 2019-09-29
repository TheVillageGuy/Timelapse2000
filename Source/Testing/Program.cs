using RimworldRendererMod.CommonV3;
using System;
using System.Threading;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            Test1();
        }

        private static void Test1()
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
    }
}
