using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageGetGameRecords : Message
    {
        public override string MsgName => nameof(MessageGetGameRecords);
        public override MessageID MessageID => MessageID.MI_GET_GAME_RECORDS;
    }
}
