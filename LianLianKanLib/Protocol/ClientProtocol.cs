using LianLianKanLib.Protocol.SecurityTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol
{
    public class ClientProtocol : Protocol
    {
        public ClientProtocol()
        {
            var client = new SecurityTCPClient();
            this.TCP = client;
        }

        #region 属性与变脸

        /// <summary>
        /// 通讯端
        /// </summary>
        public Endpoint Endpoint { get; private set; }

        #endregion

        #region 方法

        private void PostOffineEvent(object obj, SecurityTCP.Client client)
        {
            Task.Run(() =>
            {
                this.Disconnect();
                EndpointList.TryGetValue(client, out Endpoint endpoint);
                OfflineEventHandler?.Invoke(endpoint);
            });
        }

        public bool Connect(string host, int port)
        {
            var client = this.TCP as SecurityTCPClient;
            if (client.StartConnectAsync(host, port))
            {
                client.LoseConnectionEventHandler += PostOffineEvent;
                this.Endpoint = new Endpoint(client.Server);
                this.AddEndpoint(this.Endpoint);
            }
            return IsConnected;
        }

        public bool Disconnect()
        {
            this.StopDelivering();
            var client = this.TCP as SecurityTCPClient;
            client.LoseConnectionEventHandler -= PostOffineEvent;
            if (client.StopConnect())
            {
                this.Endpoint = null;
                this.ClearEndpoint();
            }
            return IsConnected != true;
        }

        #endregion
    }

}
