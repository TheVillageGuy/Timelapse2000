using System;
using System.Threading;

namespace RimworldRendererMod.AppConnection
{
    public class ThreadedConnection : IDisposable
    {
        public Connection Connection { get; }
        public ServerConnection ServerConnection { get { return Connection as ServerConnection; } }
        public ClientConnection ClientConnection { get { return Connection as ClientConnection; } }
        public Action<ConnectionData> UponRecieve;

        public bool IsReading { get; private set; }

        private Thread readThread;

        public ThreadedConnection(Connection connection)
        {
            Connection = connection;
        }

        public void StartRead()
        {
            if (readThread != null)
                return;

            readThread = new Thread(RunRead);
            readThread.Name = "Connection read thread";
            IsReading = true;
            readThread.Start();
        }

        private void RunRead()
        {
            try
            {
                while (Connection.Pipe.IsConnected)
                {
                    var read = Connection.ReadData();
                    UponRecieve?.Invoke(read);
                }
                Console.WriteLine("Pipe connection closed, stopping read.");
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
            Connection.Dispose();
        }
    }
}
