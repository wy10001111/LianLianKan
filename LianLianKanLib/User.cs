using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using static LianLianKanLib.SecurityTCP;

namespace LianLianKanLib
{
    public class User
    {
        public User(int id, string account, string password, string name)
        {
            this.ID = id;
            this.Password = password;
            this.Account = account;
            this.Name = name;
            HeadStream = null;
        }
        public readonly int ID;
        public readonly string Password;
        public readonly string Account;
        public readonly string Name;
        public string Address => Client?.ToString() ?? "No connection";
        public Client Client;
        public string Introduce;
        public string HeadImageName;
        public MemoryStream HeadStream;

        public ObservableCollection<GameRecord> GameRecords { get; } = new ObservableCollection<GameRecord>();

    }
}
