using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol.Messages
{
    public class MessageRegisterSuccessfully : Message
    {
        public override string MsgName => nameof(MessageRegisterSuccessfully);
        public override MessageID MessageID => MessageID.MI_REGISTER_SUCCESSFULLY;
    }
}
