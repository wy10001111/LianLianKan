using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol.SecurityTCP
{
    public class Error
    {
        public Error(Client client, ref string message)
        {
            this.Client = client;
            this.Message = message;
        }
        public Client Client { get; }
        public string Message { get; set; }
    }
}
