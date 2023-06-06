using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol.SecurityTCP
{
    public class TCPPackage : EventArgs
    {
        public TCPPackage(Client client, byte[] data, int size)
        {
            this.Client = client;
            this.Data = data;
            this.Size = size;
        }

        public Client Client { get; }
        public byte[] Data { get; }
        public int Size { get; }
    }
}
