using LianLianKanLib;
using LianLianKanLib.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanServer.MessageTriggers
{
    public abstract class MessageTrigger
    {

        public static MainManager Manager;
        private readonly static Assembly _msgAssembly = Assembly.Load("LianLianKanServer");

        public virtual void StartWork(MainManager member, Message msg)
        {
            string error = "账号不存在";
            User realUser = member.FindUser(msg.UserID);
            Message respondsMsg = null;
            if (null != realUser)
                respondsMsg = this.GetRespondsMsg(member, realUser, msg);
            else
                respondsMsg = new MessageError()
                {
                    Endpoint = msg.Endpoint,
                    CallID = msg.CallID,
                    UserID = msg.UserID,
                    Error = error,
                };
            if (respondsMsg != null)
                member._serverProtocol.SendMsg(respondsMsg);
        }

        protected virtual Message GetRespondsMsg(MainManager manager, User localUser, Message msg)
        {
            return null;
        }

        public static void ReceiveMessage(object sender, Message msg)
        {
            App.Current.Dispatcher.Invoke(() => {
                var action = _msgAssembly.CreateInstance("LianLianKanServer.MessageTriggers.ActionFor" + msg.MsgName) as MessageTrigger;
                action?.StartWork(Manager, msg);
            });
        }

    }

}
