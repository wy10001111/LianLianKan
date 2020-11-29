using LianLianKanLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LianLianKanServer
{
    public abstract class MessageAction
    {
        public virtual void StartWork(MainConfig config, Message msg)
        {
            string error = "账号不存在";
            UIUser realUser = this.FindUser(config, msg.UserID);
            Message respondsMsg = null;
            if (null != realUser)
                respondsMsg = this.GetRespondsMsg(config, realUser, msg);
            else
                respondsMsg = new MessageError()
                {
                    User = msg.User,
                    CallID = msg.CallID,
                    UserID = msg.UserID,
                    Error = error,
                };
            if (respondsMsg != null)
                config._serverProtocol.SendMsg(respondsMsg);
        }
        protected UIUser FindUser(MainConfig config, int id)
        {
            UIUser realUser = null;
            for (int index = 0; index < config.UserList.Count; index++)
            {
                var user = config.UserList[index];
                if (user.ID == id)
                {
                    realUser = user;
                    break;
                }
            }
            return realUser;
        }
        protected UIUser FindUser(MainConfig config, string account)
        {
            for (int index = 0; index < config.UserList.Count; index++)
            {
                var user = config.UserList[index];
                if (user.Account == account)
                    return user;
            }
            return null;
        }
        protected virtual Message GetRespondsMsg(MainConfig config, UIUser localUser, Message msg)
        {
            return null;
        }
    }

    class ActionForMessageLoginRequest : MessageAction
    {
        public override void StartWork(MainConfig config, Message msg)
        {
            var realMsg = msg as MessageLoginRequest;
            string error = null;
            UIUser realUser = FindUser(config, realMsg.Account);
            if (realUser == null)
                error = "账号不存在";
            else if (realUser.Password != realMsg.Password)
            {
                realUser = null;
                error = "密码错误";
            }
            else if (realUser.Client != null)
            {
                realUser = null;
                error = "账号已登录";
            }
            Message respondsMsg = null;
            if (null != realUser)
            {
                respondsMsg = new MessageLoginResponds()
                {
                    CallID = msg.CallID,
                    User = realUser,
                    UserID = realUser.ID,
                    Account = realUser.Account,
                    Name = realUser.Name,
                    Introduce = realUser.Introduce
                };
                realUser.Client = msg.User.Client;
                config._serverProtocol.UserList[msg.User.Client] = realUser;
            }
            else
                respondsMsg = new MessageLoginFailed()
                {
                    User = msg.User,
                    CallID = msg.CallID,
                    UserID = msg.UserID,
                    Account = realMsg.
                    Account, Error = error,
                };
            config._serverProtocol.SendMsg(respondsMsg);
        }
    }

    class ActionForMessageGetHeadRequest : MessageAction
    {
        public override void StartWork(MainConfig config, Message msg)
        {
            string error = "账号不存在";
            UIUser realUser = this.FindUser(config, msg.UserID);
            Message respondsMsg = null;
            if (null != realUser)
                respondsMsg = this.GetRespondsMsg(config, realUser, msg);
            else
                respondsMsg = new MessageError()
                {
                    User = msg.User,
                    CallID = msg.CallID,
                    UserID = msg.UserID,
                    Error = error,
                };
            if (respondsMsg != null)
                config._serverProtocol.SendBigMsg(respondsMsg);
        }
        protected override Message GetRespondsMsg(MainConfig config, UIUser localUser, Message msg)
        {
            localUser.ChangeHead(localUser.HeadImagePath);
            return new MessageGetHeadResponds()
            {  
                CallID = msg.CallID,
                User = localUser,
                UserID = localUser.ID,
                Source = localUser.HeadStream.GetBuffer(),
            };
        }
    }

    class ActionForMessageGetGameRecords : MessageAction
    {
        protected override Message GetRespondsMsg(MainConfig config, UIUser localUser, Message msg)
        {
            config._sqlServer.GetGameRecords(localUser);
            return new MessageGameRecords()
            {  
                CallID = msg.CallID,
                User = localUser,
                UserID = localUser.ID,
                Records = localUser.GameRecords.ToArray()
            };
        }
    }

    class ActionForMessageTakeOneGameRecord : MessageAction
    {
        protected override Message GetRespondsMsg(MainConfig config, UIUser localUser, Message msg)
        {
            var realMsg = msg as MessageTakeOneGameRecord;
            config._sqlServer.InsertGameRecord(localUser, realMsg.Record);
            return null;
        }
    }

    class ActionForMessageRegisterRequest : MessageAction
    {
        private bool CheckMsg(MainConfig config, Message msg, out string error)
        {
            try
            {
                var realMsg = msg as MessageRegisterRequest;
                UIUser realUser = FindUser(config, realMsg.Account);
                if (null != realUser)
                    throw new Exception("账号已存在");
                var validateResult = new AccountValidationRule().Validate(realMsg.Account, null);
                if (validateResult.IsValid == false)
                    throw new Exception(validateResult.ErrorContent.ToString());
                
                validateResult = new PasswordValidationRule().Validate(realMsg.Password, null);
                if (validateResult.IsValid == false)
                    throw new Exception(validateResult.ErrorContent.ToString());

                validateResult = new NameValidationRule().Validate(realMsg.Name, null);
                if (validateResult.IsValid == false)
                    throw new Exception(validateResult.ErrorContent.ToString());

                validateResult = new IntroduceValidationRule().Validate(realMsg.Introduce, null);
                if (validateResult.IsValid == false)
                    throw new Exception(validateResult.ErrorContent.ToString());

                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }

        }
        public override void StartWork(MainConfig config, Message msg)
        {
            string error;
            var realMsg = msg as MessageRegisterRequest;
            Message respondsMsg = null;
            if (CheckMsg(config, msg, out error))
            {
                UIUser user = new UIUser(0, realMsg.Account, realMsg.Password, realMsg.Name);
                user.Introduce = realMsg.Introduce;
                var fileName = $"User{realMsg.Account.GetHashCode()}.png";
                using (var stream = new MemoryStream(realMsg.HeadsculptStream))
                {
                    var encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(stream));
                    using (var fileStream = File.OpenWrite($@"HeadImage\{fileName}"))
                        encoder.Save(fileStream);
                }
                user.HeadImagePath = fileName;
                var realUser = config._sqlServer.InsertUser(user);
                realUser.Client = msg.User.Client;
                config._serverProtocol.UserList[realUser.Client] = realUser;
                config.UserList.Add(realUser);
                respondsMsg = new MessageRegisterSuccessfully()
                {  
                    CallID = msg.CallID,
                    UserID = realUser.ID,
                    User = realUser
                };
            }
            else
                respondsMsg = new MessageRegisterFailed()
                {
                    User = msg.User,
                    CallID = msg.CallID,
                    UserID = msg.UserID,
                    Account = realMsg.Account,
                    Error = error,
                };
            config._serverProtocol.SendMsg(respondsMsg);
        }
    }

}
