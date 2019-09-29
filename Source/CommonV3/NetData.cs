using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RimworldRendererMod.CommonV3
{
    public class NetData
    {
        public const byte UNKNOWN = 0;
        public const byte END = 1;

        public int DataLength { get { return rawData.Count; } }

        private List<byte> rawData = new List<byte>();
        private int readIndex = 0;

        public NetData()
        {

        }

        public NetData(byte[] data, int length)
        {
            for (int i = 0; i < length; i++)
            {
                rawData.Add(data[i]);
            }
            readIndex = 0;
        }

        private bool ReadCheck(int length)
        {
            if (length <= 0)
                return false;

            if(readIndex + length > rawData.Count)
            {
                return false;
            }
            return true;
        }

        public NetData Write(byte b)
        {
            rawData.Add(b);
            return this;
        }

        public NetData Write(float f)
        {
            rawData.AddRange(BitConverter.GetBytes(f));
            return this;
        }

        public NetData Write(int x)
        {
            rawData.AddRange(BitConverter.GetBytes(x));
            return this;
        }

        public NetData Write(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            Write(bytes.Length);
            rawData.AddRange(bytes);
            return this;
        }

        public byte ReadByte()
        {
            if (ReadCheck(1))
            {
                readIndex += 1;
                return rawData[readIndex - 1];
            }
            else
            {
                return 0;
            }
        }

        public float ReadFloat()
        {
            if (ReadCheck(4))
            {
                readIndex += 4;
                return BitConverter.ToSingle(rawData.ToArray(), readIndex - 4);
            }
            else
            {
                return 0f;
            }
        }

        public int ReadInt32()
        {
            if (ReadCheck(4))
            {
                readIndex += 4;
                return BitConverter.ToInt32(rawData.ToArray(), readIndex - 4);
            }
            else
            {
                return 0;
            }
        }

        public string ReadString()
        {
            int length = ReadInt32();
            if (ReadCheck(length))
            {

                readIndex += length;
                return Encoding.UTF8.GetString(rawData.ToArray(), readIndex - length, length);
            }
            else
            {
                return null;
            }
        }

        public byte[] ToArray()
        {
            return rawData.ToArray();
        }
    }
}
