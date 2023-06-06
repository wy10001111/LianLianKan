using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol
{
    public class ProtocolError
    {
        public ProtocolError(Endpoint endpoint, string content)
        {
            this.Endpoint = endpoint;
            this.Content = content;
        }
        public Endpoint Endpoint { get; }
        public string Content { get; }
    }
}
