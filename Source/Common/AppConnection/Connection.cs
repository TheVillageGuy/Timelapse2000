using System;
using System.IO.Pipes;

namespace RimworldRendererMod.AppConnection
{
    public abstract class Connection : IDisposable
    {
        public PipeStream Pipe { get; }
        public NamedPipeServerStream ServerPipe { get { return Pipe as NamedPipeServerStream; } }
        public PipeStream ClientPipe { get { return Pipe as NamedPipeClientStream; } }

        private ConnectionStream stream;

        public Connection()
        {
            Pipe = CreatePipe("RimworldRenderConnectionPipe", PipeDirection.InOut, 1);
            stream = new ConnectionStream(Pipe);
        }

        protected abstract PipeStream CreatePipe(string id, PipeDirection dir, int count);

        public ConnectionData ReadData()
        {
            return stream.Read();
        }

        public void WriteData(ConnectionData data)
        {
            stream.Write(data);
        }

        public abstract void Dispose();
    }

    public class ServerConnection : Connection
    {
        protected override PipeStream CreatePipe(string id, PipeDirection dir, int count)
        {
            return new NamedPipeServerStream(id, dir, count, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        }

        public void WaitForConnection()
        {
             ServerPipe.WaitForConnection();
        }

        public void Disconnect()
        {
            if(ServerPipe.IsConnected)
                ServerPipe.Disconnect();
        }

        public override void Dispose()
        {
            Disconnect();
            Pipe.Dispose();
        }
    }

    public class ClientConnection : Connection
    {
        protected override PipeStream CreatePipe(string id, PipeDirection dir, int count)
        {
            return new NamedPipeClientStream(".", id, dir, PipeOptions.Asynchronous);
        }

        public void Connect(int timeout = 5000)
        {
            try
            {
                (Pipe as NamedPipeClientStream).Connect(timeout);
            }
            catch(Exception e)
            {
                // Huh. That sucks.
                Console.WriteLine(e);
            }
        }

        public override void Dispose()
        {
            Pipe.Close();
            Pipe.Dispose();
        }
    }
}
