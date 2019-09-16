using System.IO.Pipes;

namespace RimworldRendererMod.AppConnection
{
    public abstract class Connection
    {
        public PipeStream Pipe { get; }

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
    }

    public class ServerConnection : Connection
    {
        protected override PipeStream CreatePipe(string id, PipeDirection dir, int count)
        {
            return new NamedPipeServerStream(id, dir, count);
        }

        public void WaitForConnection()
        {
            (Pipe as NamedPipeServerStream).WaitForConnection();
        }
    }

    public class ClientConnection : Connection
    {
        protected override PipeStream CreatePipe(string id, PipeDirection dir, int count)
        {
            return new NamedPipeClientStream(".", id, dir);
        }

        public void Connect(int timeout = 5000)
        {
            (Pipe as NamedPipeClientStream).Connect(timeout);
        }
    }
}
