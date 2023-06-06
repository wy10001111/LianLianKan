using LianLianKanLib;
using LianLianKanLib.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanServer.MessageTriggers
{
    public class ActionForMessageTakeOneGameRecord : MessageTrigger
    {
        protected override Message GetRespondsMsg(MainManager manager, User localUser, Message msg)
        {
            var realMsg = msg as MessageTakeOneGameRecord;
            manager._sqlServer.InsertGameRecord(localUser, new LianLianKanLib.GameRecord(realMsg.Record.PlayTime, realMsg.Record.SpentTime));
            return null;
        }
    }

}
