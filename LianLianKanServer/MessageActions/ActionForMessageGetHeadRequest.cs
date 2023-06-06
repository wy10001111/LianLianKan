using LianLianKanLib;
using LianLianKanLib.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanServer.MessageTriggers
{
    public class ActionForMessageGetHeadRequest : MessageTrigger
    {
        public override void StartWork(MainManager manager, Message msg)
        {
            User realUser = manager.FindUser(msg.UserID);
            Message respondsMsg = null;
            if (null != realUser)
                if (realUser.HeadStream != null)
                {
                    respondsMsg = new MessageGetHeadResponds()
                    {
                        CallID = msg.CallID,
                        Endpoint = realUser.Endpoint,
                        UserID = realUser.ID,
                        Source = realUser.HeadStream?.GetBuffer(),
                    };
                }
                else
                {
                    respondsMsg = new MessageError()
                    {
                        Endpoint = msg.Endpoint,
                        CallID = msg.CallID,
                        UserID = msg.UserID,
                        Error = "未找到头像",
                    };
                }
            else
            {
                respondsMsg = new MessageError()
                {
                    Endpoint = msg.Endpoint,
                    CallID = msg.CallID,
                    UserID = msg.UserID,
                    Error = "账号不存在",
                };
            }
            if (respondsMsg != null)
                manager._serverProtocol.SendBigMsg(respondsMsg);
        }

    }

}
