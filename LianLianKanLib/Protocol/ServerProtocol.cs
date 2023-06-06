using LianLianKanLib.Protocol.SecurityTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol
{
    public class ServerProtocol : Protocol
    {
        public ServerProtocol()
        {
            var server = new SecurityTCPServer();
            server.AcceptClientEventHandler += AcceptUser;
            server.MissClientEventHandler += MissUser;
            this.TCP = server;
        }

        public void AcceptUser(object sender, SecurityTCP.Client client)
        {
            this.AddEndpoint(new Endpoint(client));
        }

        public void MissUser(object sender, SecurityTCP.Client client)
        {
            EndpointList.TryGetValue(client, out Endpoint endpoint);
            this.RemoveEndpoint(endpoint);
            OfflineEventHandler?.Invoke(endpoint);
        }

        public bool StartServing(string ip, int port)
        {
            var server = this.TCP as SecurityTCPServer;
            return server.StartServe(ip, port);
        }

        public bool StopServing()
        {
            this.StopDelivering();
            var server = this.TCP as SecurityTCPServer;
            return server.StopServe();
        }

    }
}
