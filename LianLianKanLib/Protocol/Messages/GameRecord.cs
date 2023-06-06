using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib.Protocol.Messages
{
    [XmlRoot]
    public class GameRecord : BindableObject
    {
        [XmlAttribute]
        public string PlayTime { get; set; }

        [XmlAttribute]
        public string SpentTime { get; set; }
    }

}
