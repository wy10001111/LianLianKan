using LianLianKanLib;
using LianLianKanLib.Protocol.Messages;
using LianLianKanLib.ValidationRules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LianLianKanServer.MessageTriggers
{
    class ActionForMessageRegisterRequest : MessageTrigger
    {

        private bool CheckMsg(MainManager manager, Message msg, out string error)
        {
            try
            {
                var realMsg = msg as MessageRegisterRequest;
                var realUser = manager.FindUser(realMsg.Account);
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

        public override void StartWork(MainManager manager, Message msg)
        {
            string error;
            var realMsg = msg as MessageRegisterRequest;
            Message respondsMsg = null;
            if (CheckMsg(manager, msg, out error))
            {
                var user = new User(0, realMsg.Account, realMsg.Password, realMsg.Name);
                user.Introduce = realMsg.Introduce;
                //头像
                var fileName = $"User{realMsg.Account.GetHashCode()}.png";
                var directory = Path.Combine(Environment.CurrentDirectory, $@"HeadImages");
                Directory.CreateDirectory(directory);
                var filePath = Path.Combine(directory, fileName);
                using (var stream = new MemoryStream(realMsg.HeadsculptStream))
                {
                    var encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(stream));
                    using (var fileStream = File.OpenWrite(filePath))
                    {
                        encoder.Save(fileStream);
                    }
                }
                user.ChangeHead(filePath);
                //插入数据库
                var realUser = manager._sqlServer.InsertUser(user.Account, user.Password, user.Name, user.Introduce, user.HeadImagePath);
                //
                realUser.Endpoint = msg.Endpoint;
                manager.UserList.Add(realUser);
                respondsMsg = new MessageRegisterSuccessfully()
                {
                    CallID = msg.CallID,
                    UserID = realUser.ID,
                    Endpoint = realUser.Endpoint
                };
            }
            else
                respondsMsg = new MessageRegisterFailed()
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
