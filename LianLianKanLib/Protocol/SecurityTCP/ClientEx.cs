using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol.SecurityTCP
{
    class ClientEx : Client
    {
        public ClientEx(TcpClient client)
        {
            Client = client;
            client.ReceiveTimeout = 3000;
            client.SendTimeout = 3000;
            IPEndPoint ipep = Client.Client.RemoteEndPoint as IPEndPoint;
            if (null != ipep)
            {
                this.IP = ipep.Address.ToString();
                this.Port = ipep.Port;
            }

            //控制Token
            this.TokenSource = new CancellationTokenSource();

            //心跳包
            uint open = 1;    //开启/关闭 标志
            uint beginTime = 3000;  //闲时启动时间
            uint keepSpan = 1000;  //发送时间间隔
            uint dummy = 0;
            byte[] inValue = new byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes(open).CopyTo(inValue, 0); //
            BitConverter.GetBytes(beginTime).CopyTo(inValue, Marshal.SizeOf(open));
            BitConverter.GetBytes(keepSpan).CopyTo(inValue, Marshal.SizeOf(open) + Marshal.SizeOf(beginTime));
            Client.Client.IOControl(IOControlCode.KeepAliveValues, inValue, null);
        }

        public TcpClient Client { get; set; }
        public Task Task { get; set; }

        public NetworkStream Stream;
        public CancellationTokenSource TokenSource { get; set; }
        public byte[] PubKey { get; set; }

        public void SetAddress(string ip, int port)
        {
            this.IP = ip;
            this.Port = port;
        }

    }
}
