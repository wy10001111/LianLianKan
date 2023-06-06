using LianLianKanLib;
using LianLianKanLib.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanServer.MessageTriggers
{

    public class ActionForMessageGetGameRecords : MessageTrigger
    {
        protected override Message GetRespondsMsg(MainManager manager, User localUser, Message msg)
        {
            manager._sqlServer.GetGameRecords(localUser);
            return new MessageGameRecords()
            {
                CallID = msg.CallID,
                Endpoint = localUser.Endpoint,
                UserID = localUser.ID,
                Records = localUser.GameRecords.Select((record) 
                => new LianLianKanLib.Protocol.Messages.GameRecord() 
                {  
                    PlayTime = record.PlayTime, 
                    SpentTime = record.SpentTime 
                }).ToArray()
            };
        }
    }

}
