using System;

namespace RimworldRendererMod.Common
{
    public class ServerConnection : ThreadedConnection
    {
        public Action<ConnectionData> UponMessage;
        public Action UponShutdown;

        public ServerConnection(string id) : base(id, true)
        {

        }

        protected override void UponDisconnected()
        {
            UponShutdown?.Invoke();
        }

        protected override void UponMessageIn(ConnectionData data)
        {
            UponMessage?.Invoke(data);
        }
    }
}
