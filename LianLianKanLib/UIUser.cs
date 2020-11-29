using LianLianKanLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LianLianKanLib.SecurityTCP;

namespace LianLianKanLib
{
    public class UIUser : BindableObject
    {
        public UIUser(int id, string account, string password, string name)
        {
            _user = new User(id, account, password, name);
        }
        public UIUser(string account, string name) : this(-1, account, "", name)
        {
        }
        ~UIUser()
        {
            _user.HeadStream?.Dispose();
        }
        private User _user;
        public string Account => _user.Account;
        public string Name => _user.Name;
        public int ID => _user.ID;
        public string Password => _user.Password;
        public string Address => _user.Address;
        public string HeadImageName => _user.HeadImageName;
        public Client Client
        {
            get
            {
                return _user.Client;
            }
            set
            {
                if (!EqualityComparer<Client>.ReferenceEquals(_user.Client, value))
                {
                    _user.Client = value;
                    OnPropertyChanged(nameof(Client));
                    OnPropertyChanged(nameof(Address));
                }
            }
        }
        public string HeadImagePath
        {
            get
            {
                return @"HeadImage\" + _user.HeadImageName;
            }
            set
            {
                var info = new FileInfo(@"HeadImage\" + value);
                if (info.Exists)
                    SetProperty(ref _user.HeadImageName, new FileInfo(value).Name);
                else
                    SetProperty(ref _user.HeadImageName, "Logo.ico");
            }
        }
        public string Introduce
        {
            get
            {
                return _user.Introduce;
            }
            set
            {
                SetProperty(ref _user.Introduce, value);
            }
        }
        public event PropertyChangedEventHandler HeadStreamPropertyChanged;
        public MemoryStream HeadStream => _user.HeadStream;
        public void ChangeHeadStream(MemoryStream newStream)
        {
            var old = _user.HeadStream;
            _user.HeadStream = newStream;
            HeadStreamPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HeadStream)));
            old?.Dispose();
        }
        public void ChangeHead(string imagePaht)
        {
            if (File.Exists(imagePaht) == false)
                return;
            using (var file = File.OpenRead(imagePaht))
            {
                var stream= new MemoryStream();
                file.CopyTo(stream);
                this.ChangeHeadStream(stream);
            }
        }

        public ObservableCollection<GameRecord> GameRecords => _user.GameRecords;
        public GameRecord LastGameRecord()
        {
            if (GameRecords != null)
                if (GameRecords.Count > 0)
                    return GameRecords.ElementAt(0);
            return null;
        }
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
    }
}
