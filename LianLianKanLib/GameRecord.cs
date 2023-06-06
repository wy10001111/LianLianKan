using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib
{
    public class GameRecord : BindableObject
    {
        public GameRecord(string playTime, string spentTime)
        {
            this.PlayTime = playTime;
            this.SpentTime = spentTime;
        }
        public GameRecord()
        {
        }

        public static string PlayTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public static string SpentTimeFormat = @"mm\:ss\:ffffff";

        private string _playTime;
        public string PlayTime
        {
            get => _playTime;
            set => Set(ref _playTime, value);
        }

        private string _spentTime;
        public string SpentTime
        {
            get => _spentTime;
            set => Set(ref _spentTime, value);
        }

    }
}
