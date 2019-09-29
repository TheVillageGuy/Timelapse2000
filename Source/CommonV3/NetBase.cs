using System;
using System.Net;

namespace RimworldRendererMod.CommonV3
{
    public abstract class NetBase
    {
        public abstract bool Connected { get; }
        public bool Running { get; protected set; }
        public Action UponConnected;
        public Action<NetData> UponMessageIn;

        protected internal IPAddress GetLocalAddress()
        {
            IPAddress address = IPAddress.Parse("127.0.0.1");

            return address;
        }

        protected void Log(string s)
        {
            Console.WriteLine(s);
        }

        public abstract void Run(int port);

        public abstract void Write(NetData data);

        public virtual void Shutdown()
        {
            Running = false;
        }
    }
}
