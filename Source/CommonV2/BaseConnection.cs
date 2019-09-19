using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace RimworldRendererMod.Common
{
    public abstract class BaseConnection : IDisposable
    {
        public bool IsConnected { get; protected set; }
        protected bool isServer { get; }

        protected NamedPipeServerStream serverPipe { get; private set; }
        protected NamedPipeClientStream clientPipe { get; private set; }
        protected ConnectionStream stream { get; private set; }

        protected string CS { get { return isServer ? "Server" : "Client"; } }

        public BaseConnection(string id, bool isServer)
        {
            this.isServer = isServer;
            if (isServer)
            {
                serverPipe = new NamedPipeServerStream(id, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            }
            else
            {
                clientPipe = new NamedPipeClientStream(".", id, PipeDirection.InOut, PipeOptions.Asynchronous);
            }

            stream = new ConnectionStream(isServer ? (Stream)serverPipe : (Stream)clientPipe);
        }

        public void Write(byte id, string info)
        {
            this.Write(new ConnectionData(id, info));
        }

        public void Write(ConnectionData data)
        {
            if (IsConnected)
            {
                stream.Write(data);
            }
            else
            {
                Console.WriteLine($"[{CS}] Cannot write data, not connected!");
            }
        }

        public bool Read(out ConnectionData data)
        {
            return stream.Read(out data);
        }

        public virtual void Dispose()
        {
            if(serverPipe != null)
            {
                serverPipe.Dispose();
                serverPipe = null;
            }

            if(clientPipe != null)
            {
                clientPipe.Dispose();
                clientPipe = null;
            }

            if(stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }
    }
}
