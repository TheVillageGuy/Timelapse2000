using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;

namespace RimworldRendererMod.Common
{
    public abstract class ThreadedConnection : BaseConnection
    {
        public bool IsConnecting { get; protected set; }
        public bool IsReading { get; private set; }

        private Thread thread;

        public ThreadedConnection(string id, bool isServer) : base(id, isServer)
        {

        }

        public void StartRead()
        {
            if (IsReading)
                return;

            IsReading = true;
            thread = new Thread(RunThread);
            thread.Name = CS + " data read thread";
            thread.Start();
        }

        protected virtual void RunThread()
        {
            PipeStream pipe = isServer ? (PipeStream)serverPipe : (PipeStream)clientPipe;

            if (!pipe.IsConnected)
            {
                Console.WriteLine($"[{CS}] Waiting for connection to start read...");
                if (isServer)
                {
                    serverPipe.BeginWaitForConnection((state) =>
                    {
                        if (state.IsCompleted && serverPipe != null)
                        {
                            IsConnected = true;
                            serverPipe.EndWaitForConnection(state);
                        }
                    }, null);

                    Stopwatch s = new Stopwatch();
                    s.Start();
                    while (!IsConnected)
                    {
                        Thread.Sleep(5);
                        if (!IsReading)
                        {
                            Console.WriteLine($"[{CS}] Cancelled read before connection was even established.");
                            return;
                        }
                    }
                    s.Stop();
                    Console.WriteLine($"[{CS}] Wait over, had to wait {s.Elapsed.TotalMilliseconds:F0}ms.");
                }
                else
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    while (!pipe.IsConnected)
                    {
                        if (!IsReading)
                        {
                            Console.WriteLine($"[{CS}] Cancelled read before connection was even established.");
                            return;
                        }
                        Thread.Sleep(5);
                    }
                    IsConnected = true;
                    s.Stop();
                    Console.WriteLine($"[{CS}] Wait over, had to wait {s.Elapsed.TotalMilliseconds:F0}ms.");
                }
                
                Console.WriteLine($"[{CS}] Found connection, starting read!");
            }

            while (IsConnected && (isServer ? serverPipe.IsConnected : clientPipe.IsConnected))
            {
                if (!IsReading)
                    return;

                bool worked = stream.Read(out ConnectionData data);
                if (!worked)
                {
                    // We have been disconnected.
                    Console.WriteLine($"[{CS}] Reading data failed, connection assumed lost.");
                    IsConnected = false;
                    continue;
                }

                UponMessageIn(data);
            }

            UponDisconnected();
            IsConnected = false;
        }

        protected virtual void UponDisconnected()
        {

        }

        protected abstract void UponMessageIn(ConnectionData data);

        public override void Dispose()
        {
            IsReading = false;
            IsConnected = false;
            IsConnecting = false;
            thread = null;
            base.Dispose();
        }
    }
}
