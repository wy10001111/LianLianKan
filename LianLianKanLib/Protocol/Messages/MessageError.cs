using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageError : Message
    {
        public override string MsgName => nameof(MessageError);
        public override MessageID MessageID => MessageID.MI_ERROR;
        [XmlElement]
        public string Error { get; set; }
    }

}
