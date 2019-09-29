using System;
using System.Net.Sockets;

namespace RimworldRendererMod.CommonV3
{
    public class NetClient : NetBase
    {
        public override bool Connected
        {
            get
            {
                return client != null && client.Connected;
            }
        }

        private readonly byte[] inBuffer = new byte[2048];
        private TcpClient client;
        private NetworkStream stream;

        public override void Run(int port)
        {
            if (client != null)
                return;

            Running = true;

            try
            {
                Log("Connecting client...");
                client = new TcpClient("127.0.0.1", port);

                Log("Connected!");
                stream = client.GetStream();

                UponConnected?.Invoke();

                int count;
                while (Running && client.Connected)
                {
                    count = stream.Read(inBuffer, 0, inBuffer.Length);

                    if (count == 0)
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
            {
                Log("ERROR: Cannot send data, client is not connected!");
                return;
            }

            stream.Write(data.ToArray(), 0, data.DataLength);
        }

        public override void Shutdown()
        {
            base.Shutdown();
            client.Close();
        }
    }
}
