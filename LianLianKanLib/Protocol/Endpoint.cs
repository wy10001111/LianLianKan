using LianLianKanLib.Protocol.SecurityTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol
{
    public class Endpoint
    {
        public Endpoint(Client client)
        {
            this.Client = client;
        }

        public Client Client { get; }

        public string IP => Client.IP;

        public int Port => Client.Port;

        public string FullAddress => $"{IP}:{Port}";

    }
}
