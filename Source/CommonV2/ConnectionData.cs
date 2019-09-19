using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RimworldRendererMod.Common
{
    public struct ConnectionData
    {
        public byte ID;
        public string Info;

        public ConnectionData(byte id, string info)
        {
            this.ID = id;
            this.Info = info;
        }
    }
}
