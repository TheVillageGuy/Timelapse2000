namespace RimworldRendererMod.Common
{
    public static class DataID
    {
        /// <summary>
        /// Should never be sent, indicates that the data recieved is invalid.
        /// </summary>
        public const byte Unknown = 0;
        /// <summary>
        /// Sent from the client to server to indicate connection is established.
        /// </summary>
        public const byte Connected = 1;
        /// <summary>
        /// Sent from server to client to tell the client to start rendering.
        /// </summary>
        public const byte Start = 2;
        /// <summary>
        /// Sent from client to server if there is an error with the 
        /// rendering process or the data supplied to the process.
        /// </summary>
        public const byte Error = 3;
        /// <summary>
        /// Sent from client to server to give a status update.
        /// </summary>
        public const byte Update = 4;
        /// <summary>
        /// Sent from client to server when rendering is completed.
        /// </summary>
        public const byte Done = 5;
    }
}
