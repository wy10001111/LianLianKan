using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageTakeOneGameRecord : Message
    {
        public override string MsgName => nameof(MessageTakeOneGameRecord);
        public override MessageID MessageID => MessageID.MI_TAKE_ONE_GAME_RECORD;
        [XmlElement]
        public GameRecord Record { get; set; }
    }
}
