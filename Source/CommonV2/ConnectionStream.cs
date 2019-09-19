using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RimworldRendererMod.Common
{
    public class ConnectionStream : IDisposable
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
            try
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
            catch(Exception e)
            {
                Console.WriteLine($"Exception writing data: {e}");
            }
            
        }

        /// <summary>
        /// Blocks the calling thread until a new message arrives.
        /// </summary>
        /// <param name="data">The read data.</param>
        /// <returns>True if the data read was successful, false if the read data is invalid.</returns>
        public bool Read(out ConnectionData data)
        {
            int rawByte = stream.ReadByte();
            if(rawByte < 0)
            {
                data = default(ConnectionData);
                return false;
            }
            byte id = (byte)rawByte;
            int length = stream.ReadByte();
            byte[] bytes = new byte[length];
            stream.Read(bytes, 0, length);

            string str = encoding.GetString(bytes);

            data = new ConnectionData() { ID = id, Info = str };
            return true;
        }

        public void Dispose()
        {
            if(stream != null)
            {
                stream = null;
                encoding = null;
            }
        }
    }
}
