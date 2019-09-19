using System;
using System.Diagnostics;
using System.Threading;

namespace RimworldRendererMod.AppConnection
{
    public class ThreadedConnection : IDisposable
    {
        public Connection Connection { get; }
        public ServerConnection ServerConnection { get { return Connection as ServerConnection; } }
        public ClientConnection ClientConnection { get { return Connection as ClientConnection; } }
        public Action<ConnectionData> UponRecieve;
        public Action UponReadConnect;

        public bool IsReading { get; private set; }
        public bool IsShuttingDown { get; private set; }

        private string CS { get { return ServerConnection != null ? "Server" : "Client"; } }

        private Thread readThread;

        public ThreadedConnection(Connection connection)
        {
            Connection = connection;
        }

        public void StartRead(int timeout = 5000)
        {
            if (readThread != null)
                return;

            readThread = new Thread(new ParameterizedThreadStart(RunRead));
            readThread.Name = "Connection read thread";
            IsShuttingDown = false;
            IsReading = true;
            readThread.Start(timeout);
        }

        public void Shutdown()
        {
            IsShuttingDown = true;
        }

        private void RunRead(object vars)
        {
            int timeout = (int)vars;
            try
            {
                if(Connection.ServerPipe != null)
                {
                    Stopwatch s = new Stopwatch();
                    Console.WriteLine($"[{CS}] Waiting for connection to start read...");
                    s.Start();

                    bool done = false;
                    var result = Connection.ServerPipe.BeginWaitForConnection((data) =>
                    {
                        if (data?.IsCompleted ?? false)
                            done = true;
                    }, null);

                    while (!done)
                    {
                        if (IsShuttingDown)
                        {
                            Connection.ServerPipe.EndWaitForConnection(null);
                            return;
                        }
                        if (s.Elapsed.TotalMilliseconds > timeout)
                        {
                            return;
                        }

                        Thread.Sleep(5);
                    }

                    s.Stop();
                    Console.WriteLine($"[{CS}] Got connection! Took {s.Elapsed.TotalMilliseconds:F0}ms.");
                }
                else
                {
                    Stopwatch s = new Stopwatch();
                    Console.WriteLine($"[{CS}] Waiting for connection to start read...");
                    s.Start();

                    while (!Connection.ClientPipe.IsConnected)
                    {
                        if (IsShuttingDown)
                        {
                            return;
                        }
                        Thread.Sleep(5);
                        if (s.Elapsed.TotalMilliseconds > timeout)
                            return;
                    }

                    s.Stop();
                    Console.WriteLine($"[{CS}] Got connection! Took {s.Elapsed.TotalMilliseconds:F0}ms.");
                }

                UponReadConnect?.Invoke();

                while (Connection.Pipe.IsConnected)
                {
                    if (IsShuttingDown)
                    {
                        return;
                    }

                    var read = Connection.ReadData();
                    UponRecieve?.Invoke(read);
                }
                Console.WriteLine($"[{CS}] Pipe connection closed, stopping read.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            IsReading = false;
            readThread = null;
        }

        public void Write(byte id, string info)
        {
            this.Write(new ConnectionData(id, info));
        }

        public void Write(ConnectionData data)
        {
            Connection.WriteData(data);
        }

        public void Dispose()
        {
            Shutdown();
            Connection?.Dispose();
        }
    }
}
