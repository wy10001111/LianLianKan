using LianLianKanLib;
using LianLianKanLib.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanServer.MessageTriggers
{
    public class ActionForMessageLoginRequest : MessageTrigger
    {
        private bool VerifyUser(User user, Message msg, ref string error)
        {
            error = null;
            if (user == null)
                error = "账号不存在";
            else if (user.Password != user.Password)
            {
                error = "密码错误";
            }
            else if (user.Endpoint != null)
            {
                error = "账号已登录";
            }
            return error == null;
        }

        public override void StartWork(MainManager manager, Message msg)
        {
            var realMsg = msg as MessageLoginRequest;
            string error = null;
            User realUser = manager.FindUser(realMsg.Account);
            Message respondsMsg = null;
            if (VerifyUser(realUser, msg, ref error))
            {
                respondsMsg = new MessageLoginResponds()
                {
                    CallID = msg.CallID,
                    Endpoint = msg.Endpoint,
                    UserID = realUser.ID,
                    Account = realUser.Account,
                    Name = realUser.Name,
                    Introduce = realUser.Introduce
                };
                realUser.Endpoint = msg.Endpoint;
            }
            else
                respondsMsg = new MessageLoginFailed()
                {
                    Endpoint = msg.Endpoint,
                    CallID = msg.CallID,
                    UserID = msg.UserID,
                    Account = realMsg.Account,
                    Error = error,
                };
            manager._serverProtocol.SendMsg(respondsMsg);
        }

    }

}
