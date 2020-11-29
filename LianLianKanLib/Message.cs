using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace LianLianKanLib
{

    public enum MessageID
    {
        MI_BIG_BOX,
        MI_ERROR,
        MI_LOGIN_REQUEST,
        MI_LOGIN_FAILED,
        MI_LOGIN_RESPONSE,
        MI_GET_HEAD_REQUEST,
        MI_GET_HEAD_RESPONSE,
        MI_GET_GAME_RECORDS,
        MI_GAME_RECORDS,
        MI_TAKE_ONE_GAME_RECORD,
        MI_REGISTER_REQUEST,
        MI_REGISTER_FAILED,
        MI_REGISTER_SUCCESSFULLY,
    }

    [XmlRoot]
    public abstract class Message
    {
        [XmlIgnore]
        private static Assembly MsgAssembly = Assembly.Load("LianLianKanLib");
        public static Message CreateInstance(string msgName)
        {
            var fullName = "LianLianKanLib." + msgName;
            return MsgAssembly.CreateInstance(fullName) as Message;
        }
        [XmlIgnore]
        public UIUser User { get; set; }
        [XmlIgnore]
        public abstract string MsgName { get; }
        [XmlElement]
        public int CallID { get; set; }
        [XmlElement]
        public int UserID { get; set; }
        [XmlElement]
        public abstract MessageID MessageID { get; }
        public static Message SetMessageEntity(string msgName, byte[] data, int index, int size)
        {
            try
            {
                Message msgClass = Message.CreateInstance(msgName);
                if (msgClass == null)
                    throw new Exception("Can't Create Insatance.");

                var serializer = new XmlSerializer(msgClass.GetType());
                using (var steam = new MemoryStream(data, index, size))
                    return serializer.Deserialize(steam) as Message;
            }
            catch (Exception ex)
            {
                var type = ex.GetType();
                return null;
            }
        }
        public int GetMessgeEntity(byte[] data, int index, int size)
        {
            try
            {
                var serializer = new XmlSerializer(this.GetType());
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
#if false
                using (var file = File.OpenWrite($@"C:\Users\Playe\Documents\Visual Studio 2015\Projects\LianLianKan\MessageXml\{this.MsgName}.xml"))
                {
                    serializer.Serialize(file, this, namespaces);
                }
#endif
                using (var steam = new MemoryStream(data, index, size))
                {
                    serializer.Serialize(steam, this, namespaces);
                    return (int)steam.Position;
                }
            }
            catch (Exception ex)
            {
                ex.GetType();
            }
            return 0;
        }
    }

    public class MessageBigBox : Message
    {
        public override string MsgName => nameof(MessageBigBox);
        public override MessageID MessageID => MessageID.MI_BIG_BOX;
        [XmlElement]
        public int TotalSize { get; set; }
        [XmlElement]
        public int Offset { get; set; }
        [XmlElement]
        public byte[] Data { get; set; }
        [XmlElement]
        public string InnnerMsgName { get; set; }
    }

    public class MessageError : Message
    {
        public override string MsgName => nameof(MessageError);
        public override MessageID MessageID => MessageID.MI_ERROR;
        [XmlElement]
        public string Error { get; set; }
    }

    public class MessageLoginRequest : Message
    {
        public override string MsgName => nameof(MessageLoginRequest);
        public override MessageID MessageID => MessageID.MI_LOGIN_REQUEST;
        [XmlElement]
        public string Account { get; set; }
        [XmlElement]
        public string Password { get; set; }
    }
    public class MessageLoginFailed : Message
    {
        public override string MsgName => nameof(MessageLoginFailed);
        public override MessageID MessageID => MessageID.MI_LOGIN_FAILED;
        [XmlElement]
        public string Account { get; set; }
        [XmlElement]
        public string Error { get; set; }

    }
    public class MessageLoginResponds : Message
    {
        public override string MsgName => nameof(MessageLoginResponds);
        public override MessageID MessageID => MessageID.MI_LOGIN_RESPONSE;
        [XmlElement]
        public string Account { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Introduce { get; set; }
    }

    public class MessageGetHeadRequest : Message
    {
        public override string MsgName => nameof(MessageGetHeadRequest);
        public override MessageID MessageID => MessageID.MI_GET_HEAD_REQUEST;
    }
    public class MessageGetHeadResponds : Message
    {
        public override string MsgName => nameof(MessageGetHeadResponds);
        public override MessageID MessageID => MessageID.MI_GET_HEAD_RESPONSE;
        [XmlElement]
        public byte[] Source { get; set; }
    }

    public class MessageGetGameRecords : Message
    {
        public override string MsgName => nameof(MessageGetGameRecords);
        public override MessageID MessageID => MessageID.MI_GET_GAME_RECORDS;
    }
    public class MessageGameRecords : Message
    {
        public override string MsgName => nameof(MessageGameRecords);
        public override MessageID MessageID => MessageID.MI_GAME_RECORDS;
        [XmlArray]
        public GameRecord[] Records { get; set; }
    }

    public class MessageTakeOneGameRecord : Message
    {
        public override string MsgName => nameof(MessageTakeOneGameRecord);
        public override MessageID MessageID => MessageID.MI_TAKE_ONE_GAME_RECORD;
        [XmlElement]
        public GameRecord Record { get; set; }
    }

    public class MessageRegisterRequest : Message
    {
        public override string MsgName => nameof(MessageRegisterRequest);
        public override MessageID MessageID => MessageID.MI_REGISTER_REQUEST;
        [XmlElement]
        public string Account { get; set; }
        [XmlElement]
        public string Password { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Introduce { get; set; }
        [XmlElement]
        public byte[] HeadsculptStream { get; set; }
    }
    public class MessageRegisterFailed : Message
    {
        public override string MsgName => nameof(MessageRegisterFailed);
        public override MessageID MessageID => MessageID.MI_REGISTER_FAILED;
        [XmlElement]
        public string Account { get; set; }
        [XmlElement]
        public string Error { get; set; }

    }
    public class MessageRegisterSuccessfully : Message
    {
        public override string MsgName => nameof(MessageRegisterSuccessfully);
        public override MessageID MessageID => MessageID.MI_REGISTER_SUCCESSFULLY;
    }


}
