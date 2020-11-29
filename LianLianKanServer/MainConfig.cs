using LianLianKanLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanServer
{
    public class MainConfig
    {
        public SqlServer _sqlServer;
        public ServerProtocol _serverProtocol;
        public ObservableCollection<UIUser> UserList { get; } = new ObservableCollection<UIUser>();
    }
}
