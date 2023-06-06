using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageLoginResponds : Message
    {
        public override string MsgName => nameof(MessageLoginResponds);
        public override MessageID MessageID => MessageID.MI_LOGIN_RESPONSE;
        [XmlElement]
        public string Account { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Introduce { get; set; }
    }
}
