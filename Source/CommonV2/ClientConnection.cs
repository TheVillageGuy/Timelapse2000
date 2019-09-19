using System;

namespace RimworldRendererMod.Common
{
    public class ClientConnection : ThreadedConnection
    {
        public Action<ConnectionData> UponMessage;
        public Action UponClientDisconnected;

        public ClientConnection(string id) : base(id, false)
        {

        }

        /// <summary>
        /// Blocks the calling thread until this client establishes a connection, or times out.
        /// See <see cref="BaseConnection.IsConnected"/> to check if the connection was successful.
        /// </summary>
        /// <param name="timeout">The timeout time in milliseconds. Defaults to 5000 (5 seconds).</param>
        public void Connect(int timeout = 5000)
        {
            if (IsConnecting)
            {
                Console.WriteLine("[Client] Already connecting.");
                return;
            }
            if (IsConnected)
            {
                Console.WriteLine("[Client] Already connected.");
            }

            IsConnecting = true;

            try
            {
                Console.WriteLine($"[Client] Starting connect, timeout {timeout}ms.");
                clientPipe.Connect(timeout);
                IsConnected = true;
                Console.WriteLine("[Client] Connected!");
            }
            catch (Exception e)
            {
                IsConnected = false;
                IsConnecting = false;
                Console.WriteLine("[Client] Failed to connect: " + e);
            }
        }

        protected override void UponDisconnected()
        {
            UponClientDisconnected?.Invoke();
        }

        protected override void UponMessageIn(ConnectionData data)
        {
            UponMessage?.Invoke(data);
        }
    }
}
