using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageGameRecords : Message
    {
        public override string MsgName => nameof(MessageGameRecords);
        public override MessageID MessageID => MessageID.MI_GAME_RECORDS;
        [XmlElement]
        public GameRecord[] Records { get; set; }
    }
}
