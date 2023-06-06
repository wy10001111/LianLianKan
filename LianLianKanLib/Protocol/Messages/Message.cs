using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol.Messages
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
        [XmlIgnore]
        public abstract string MsgName { get; }
        [XmlIgnore]
        public Endpoint Endpoint { get; set; }

        [XmlElement]
        public int CallID { get; set; }
        [XmlElement]
        public int UserID { get; set; }
        [XmlElement]
        public abstract MessageID MessageID { get; }

        /// <summary>
        /// 创建Message实例
        /// </summary>
        public static Message CreateInstance(string msgName)
        {
            var fullName = "LianLianKanLib.Protocol.Messages." + msgName;
            return MsgAssembly.CreateInstance(fullName) as Message;
        }

        /// <summary>
        /// 创建Message实例
        /// </summary>
        public static Type GetType(string msgName)
        {
            var fullName = "LianLianKanLib.Protocol.Messages." + msgName;
            return MsgAssembly.GetType(fullName);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public static Message DeserializeMessage(string msgName, byte[] data, int index, int size)
        {
            try
            {
                var type = Message.GetType(msgName);
                if (type == null)
                    throw new Exception("Can't find message.");
                
                var serializer = new XmlSerializer(type);
                using (var steam = new MemoryStream(data, index, size))
                    return serializer.Deserialize(steam) as Message;
            }
            catch (Exception ex)
            {
                ex.GetType();
                return null;
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        public static int SerializeMessage(Message msg, byte[] data, int index, int size)
        {
            try
            {
                var serializer = new XmlSerializer(msg.GetType());
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                using (var steam = new MemoryStream(data, index, size))
                {
                    serializer.Serialize(steam, msg, namespaces);
                    return (int)steam.Position;
                }
            }
            catch (Exception ex)
            {
                ex.GetType();
            }
            return 0;
        }

        /// <summary>
        /// 序列化
        /// </summary>
        public static byte[] SerializeMessage(Message msg)
        {
            try
            {
                var serializer = new XmlSerializer(msg.GetType());
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                using (var steam = new MemoryStream())
                {
                    serializer.Serialize(steam, msg, namespaces);
                    return steam.ToArray();
                }
            }
            catch (Exception ex)
            {
                ex.GetType();
            }
            return null;
        }

    }

}
