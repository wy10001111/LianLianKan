using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageRegisterRequest : Message
    {
        public override string MsgName => nameof(MessageRegisterRequest);
        public override MessageID MessageID => MessageID.MI_REGISTER_REQUEST;
        [XmlElement]
        public string Account { get; set; }
        [XmlElement]
        public string Password { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Introduce { get; set; }
        [XmlElement]
        public byte[] HeadsculptStream { get; set; }
    }
}
