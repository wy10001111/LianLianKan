using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol
{
    public enum CommandSymbol
    {
        App,   //Apply sending Permit
        All,     //Allow sending Permit
        Ent,    // Message Entity
        Flu,    // Flush Connection
    }

    [XmlRoot]
    public class Command
    {
        [XmlElement]
        public CommandSymbol Symbol { get; set; }

        [XmlElement]
        public byte[] Data { get; set; }

        /// <summary>
        /// 序列化
        /// </summary>
        public static int SerializeCommand(Command cmd, byte[] data, int index, int size)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Command));
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                using (var steam = new MemoryStream(data, index, size))
                {
                    serializer.Serialize(steam, cmd, namespaces);
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
        /// 反序列化
        /// </summary>
        public static Command DeserializeCommand(byte[] data, int index, int size)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Command));
                using (var steam = new MemoryStream(data, index, size))
                    return serializer.Deserialize(steam) as Command;
            }
            catch (Exception ex)
            {
                ex.GetType();
                return null;
            }
        }

    }
}
