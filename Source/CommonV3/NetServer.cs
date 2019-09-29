using System;
using System.Net.Sockets;

namespace RimworldRendererMod.CommonV3
{
    public class NetServer : NetBase
    {
        public override bool Connected
        {
            get
            {
                return client != null && client.Connected;
            }
        }

        private readonly byte[] inBuffer = new byte[2048];
        private TcpListener listener;
        private TcpClient client;
        private NetworkStream stream;

        public override void Run(int port)
        {
            if (listener != null)
                return;

            Running = true;

            try
            {
                listener = new TcpListener(base.GetLocalAddress(), port);
                listener.Start(1);

                Log("Waiting for client connection...");
                client = listener.AcceptTcpClient();
                stream = client.GetStream();

                Log("Got client connection!");
                UponConnected?.Invoke();

                int count;
                while (Running && client.Connected)
                {
                    count = stream.Read(inBuffer, 0, inBuffer.Length);

                    if(count == 0)
                    {
                        Log("Read message of length 0, quitting read.");
                        break;
                    }

                    NetData inData = new NetData(inBuffer, count);
                    UponMessageIn?.Invoke(inData);
                }                
            }
            catch(Exception e)
            {
                Log(e.ToString());
            }
            finally
            {
                Log("Shutting down...");

                if(client != null)
                {
                    client.Close();
                    client = null;
                }

                if(listener != null)
                {
                    listener.Stop();
                    listener = null;
                }

                if(stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }

                Running = false;
            }
        }

        public override void Write(NetData data)
        {
            if (stream == null)
                return;

            stream.Write(data.ToArray(), 0, data.DataLength);
        }

        public override void Shutdown()
        {
            base.Shutdown();
            listener.Stop();
        }
    }
}
