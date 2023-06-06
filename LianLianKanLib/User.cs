using LianLianKanLib.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib
{
    public class User : BindableObject
    {
        public User(int id, string account, string password, string name)
        {
            this.ID = id;
            this.Password = password;
            this.Account = account;
            this.Name = name;
            HeadStream = null;
        }

        ~User()
        {
            HeadStream?.Dispose();
        }

        #region 属性与变量

        public int ID { get; protected set; }
        public string Password { get; protected set; }
        public string Account { get; protected set; }
        public string Name { get; protected set; }
        public string Address => Endpoint?.FullAddress ?? "No connection";

        /// <summary>
        /// 个人简介
        /// </summary>
        private string _introduce;
        public string Introduce
        {
            get => _introduce;
            set => Set(ref _introduce, value);
        }

        /// <summary>
        /// 端点
        /// </summary>
        private Endpoint _endpoint;
        public Endpoint Endpoint
        {
            get => _endpoint;
            set
            {
                if (Set(ref _endpoint, value))
                {
                    OnPropertyChanged(nameof(Endpoint));
                    OnPropertyChanged(nameof(Address));
                }
            }
        }

        /// <summary>
        /// 头像地址
        /// </summary>
        public string _eadImagePath;
        public string HeadImagePath
        {
            get => _eadImagePath;
            private set => Set(ref _eadImagePath, value);
        }

        /// <summary>
        /// 头像流
        /// </summary>
        public MemoryStream _headStream;
        public MemoryStream HeadStream
        {
            get => _headStream;
            private set => Set(ref _headStream, value);
        }

        /// <summary>
        /// 游戏记录
        /// </summary>
        public ObservableCollection<GameRecord> GameRecords { get; } 
            = new ObservableCollection<GameRecord>();

        #endregion

        #region 方法

        /// <summary>
        /// 修改头像流
        /// </summary>
        public void ChangeHeadStream(MemoryStream newStream)
        {
            var old = HeadStream;
            HeadStream = newStream;
            old?.Dispose();
        }

        /// <summary>
        /// 修改头像
        /// </summary>
        public void ChangeHead(string imagePath)
        {
            if (File.Exists(imagePath) == false)
                return;
            using (var file = File.OpenRead(imagePath))
            {
                var stream = new MemoryStream();
                file.CopyTo(stream);
                this.ChangeHeadStream(stream);
            }
            HeadImagePath = imagePath;
        }

        /// <summary>
        /// 最后一个游戏记录
        /// </summary>
        public GameRecord LastGameRecord()
        {
            return GameRecords?.ElementAtOrDefault(0);
        }

        /// <summary>
        /// 添加游戏记录
        /// </summary>
        public void AddGameRecord(DateTime gameTime, string spentTime)
        {
            var record = new GameRecord(gameTime.ToString(GameRecord.PlayTimeFormat), spentTime);
            GameRecords.Insert(0, record);
        }
        public void AddGameRecord(DateTime gameTime, TimeSpan spentTime)
        {
            AddGameRecord(gameTime, spentTime.ToString(GameRecord.SpentTimeFormat));
        }
        public void AddGameRecord(TimeSpan spentTime)
        {
            AddGameRecord(DateTime.Now, spentTime);
        }

        #endregion

    }
}
