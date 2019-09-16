using System;
using System.IO;
using System.Text;

namespace RimworldRendererMod.AppConnection
{
    public class ConnectionStream
    {
        private Stream stream;
        private UnicodeEncoding encoding;

        public ConnectionStream(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException("stream", "Stream cannot be null.");
            encoding = new UnicodeEncoding();
        }

        public void Write(ConnectionData data)
        {
            byte[] stringBytes = data.Info == null ? new byte[0] : encoding.GetBytes(data.Info);
            if (stringBytes.Length > 255)
                throw new Exception("Data string bytes array too long! Send a shorter string.");

            byte[] final = new byte[stringBytes.Length + 2];
            final[0] = data.ID;
            final[1] = (byte)stringBytes.Length;
            Array.Copy(stringBytes, 0, final, 2, stringBytes.Length);

            stream.Write(final, 0, final.Length);
            stream.Flush();
        }

        public ConnectionData Read()
        {
            byte id = (byte)stream.ReadByte();
            int length = stream.ReadByte();
            byte[] bytes = new byte[length];
            stream.Read(bytes, 0, length);

            string str = encoding.GetString(bytes);

            return new ConnectionData() { ID = id, Info = str };
        }
    }
}
