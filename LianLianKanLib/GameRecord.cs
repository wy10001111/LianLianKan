using LianLianKanLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LianLianKanLib
{
    [XmlRoot]
    public class GameRecord : BindableObject
    {
        public static string PlayTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public static string SpentTimeFormat = @"mm\:ss\:ffffff";
        public GameRecord(string playTime, string spentTime)
        {
            this.PlayTime = playTime;
            this.SpentTime = spentTime;
        }
        public GameRecord()
        {
        }
        private string _playTime;
        [XmlAttribute]
        public string PlayTime
        {
            get
            {
                return _playTime;
            }
            set
            {
                SetProperty(ref _playTime, value);
            }
        }
        private string _spentTime;
        [XmlAttribute]
        public string SpentTime
        {
            get
            {
                return _spentTime;
            }
            set
            {
                SetProperty(ref _spentTime, value);
            }
        }
    }
}
