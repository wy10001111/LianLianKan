using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageLoginRequest : Message
    {
        public override string MsgName => nameof(MessageLoginRequest);
        public override MessageID MessageID => MessageID.MI_LOGIN_REQUEST;
        [XmlElement]
        public string Account { get; set; }
        [XmlElement]
        public string Password { get; set; }
    }
}
