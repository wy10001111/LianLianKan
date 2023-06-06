using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageGetHeadRequest : Message
    {
        public override string MsgName => nameof(MessageGetHeadRequest);
        public override MessageID MessageID => MessageID.MI_GET_HEAD_REQUEST;
    }
}
