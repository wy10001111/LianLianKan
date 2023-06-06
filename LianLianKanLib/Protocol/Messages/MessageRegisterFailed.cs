using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageRegisterFailed : Message
    {
        public override string MsgName => nameof(MessageRegisterFailed);
        public override MessageID MessageID => MessageID.MI_REGISTER_FAILED;
        [XmlElement]
        public string Account { get; set; }
        [XmlElement]
        public string Error { get; set; }
    }
}
