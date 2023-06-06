using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageGetHeadResponds : Message
    {
        public override string MsgName => nameof(MessageGetHeadResponds);
        public override MessageID MessageID => MessageID.MI_GET_HEAD_RESPONSE;
        [XmlElement]
        public byte[] Source { get; set; }
    }
}
