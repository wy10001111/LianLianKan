using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol.SecurityTCP
{
    public class Client : IComparable<Client>
    {
        #region 属性与变量

        public string IP { get; protected set; }

        public int Port { get; protected set; }

        #endregion

        #region 方法

        public static bool operator ==(Client client1, Client client2)
        {
            if (Object.ReferenceEquals(client1, client2))
                return true;
            if (Object.ReferenceEquals(client1, null) || Object.ReferenceEquals(client2, null))
                return false;
            return client1.IP == client2.IP && client1.Port == client2.Port;
        }
        
        public static bool operator !=(Client client1, Client client2) => !(client1 == client2);
        
        public override bool Equals(object obj)
        {
            return this == (obj as Client);
        }
        
        public override int GetHashCode()
        {
            return (IP.GetHashCode()) | Port;
        }

        public override string ToString()
        {
            return $"{IP}:{Port}";
        }

        public int CompareTo(Client other)
        {
            if (this == null)
                return -1;
            if (other == null)
                return 1;
            return this.GetHashCode() - other.GetHashCode();
        }

        #endregion
    }
}
