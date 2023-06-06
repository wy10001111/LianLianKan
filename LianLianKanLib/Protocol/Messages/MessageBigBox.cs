using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageBigBox : Message
    {
        public override string MsgName => nameof(MessageBigBox);
        public override MessageID MessageID => MessageID.MI_BIG_BOX;
        [XmlElement]
        public int TotalSize { get; set; }
        [XmlElement]
        public int Offset { get; set; }
        [XmlElement]
        public byte[] Data { get; set; }
        [XmlElement]
        public string InnnerMsgName { get; set; }
    }
}
