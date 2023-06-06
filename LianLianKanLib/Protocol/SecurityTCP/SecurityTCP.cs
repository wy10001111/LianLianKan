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
    public abstract class SecurityTCP
    {
        #region 属性与变量

        /// <summary>
        /// 是否链接
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        /// 加密解密者
        /// </summary>
        protected Encrytper _encrytper;

        #endregion

        #region 事件

        /// <summary>
        /// 数据包事件
        /// </summary>
        public event EventHandler<TCPPackage> PackageEventHandler;

        /// <summary>
        /// 错误事件
        /// </summary>
        public event EventHandler<Error> ErrorEventHandler;

        /// <summary>
        /// 标准Buff长度
        /// </summary>
        public static int StandardBuffLen = 1024 * 1024 * 3; //

        /// <summary>
        /// Buff最大长度
        /// </summary>
        public static int StandardBuffLenMax = StandardBuffLen - 1024 * 69;

        #endregion

        #region 方法

        /// <summary>
        /// 上报数据包
        /// </summary>
        protected void PushPackageEvent(Client client, byte[] data, int size) 
            => this.PackageEventHandler?.Invoke(this, new TCPPackage(client, data, size));

        /// <summary>
        /// 上报错误
        /// </summary>
        protected void PushErrorEvent(Client client, string error) 
            => this.ErrorEventHandler?.Invoke(this, new Error(client, ref error));

        /// <summary>
        /// 发送数据
        /// </summary>
        public bool Send(Client client, byte[] data, int dataSize)
        {
            //将对象赋值给ClientEx变量
            ClientEx clientEx = client as ClientEx;
            //判断clientEx是否为空
            if (clientEx == null)
                return false;
            //判断client是否取消
            if (clientEx.TokenSource.Token.IsCancellationRequested)
                return false;
            //判断client是否断开
            if (clientEx.Client.Connected == false)
                return false;
            int offset = 0, encrytpSize = 0; ;
            while (dataSize > 0)
            {
                //限制一次加密的数据长度
                encrytpSize = dataSize > StandardBuffLenMax ? StandardBuffLenMax : dataSize;
                byte[] sendData;
                //加密
                int sendSize = this._encrytper.EncrytpData(clientEx.PubKey, data, offset, encrytpSize, out sendData);
                //发送
                clientEx.Stream.WriteAsync(sendData, 0, sendSize, clientEx.TokenSource.Token);
                //计算偏移量和已发送量
                offset += encrytpSize;
                dataSize -= encrytpSize;
            }
            //刷新一下流，让所有数据及时发送
            clientEx.Stream.FlushAsync(clientEx.TokenSource.Token).Wait();
            return true;
        }

        /// <summary>
        /// 接收数据中...
        /// </summary>
        protected async Task Receiving(Client client)
        {
            try
            {
                //将对象赋值给ClientEx变量
                ClientEx clientEx = client as ClientEx;
                //Start receiving message
                int readSize;
                byte[] readBuff = new byte[StandardBuffLen];

                do
                {
                    //接收数据
                    readSize = await clientEx.Stream.ReadAsync(readBuff, 0, readBuff.Length, clientEx.TokenSource.Token);
                    if (readSize > 0)
                    {
                        byte[] data;
                        //解密
                        int dataSize = this._encrytper.DecrytpData(clientEx.PubKey, readBuff, 0, readSize, out data);
                        //上传
                        this.PushPackageEvent(clientEx, data, dataSize);
                    }
                } while (clientEx.TokenSource.Token.IsCancellationRequested == false && readSize > 0);

            }
            catch (Exception) 
            { 
            }
        }

        /// <summary>
        /// 等待对方密钥
        /// </summary>
        protected async Task<byte[]> WaitForKey(NetworkStream stream, int keySize, CancellationToken cancellationToken)
        {
            //Wait for key
            byte[] readBuff = new byte[StandardBuffLen];
            int readSize = await stream.ReadAsync(readBuff, 0, readBuff.Length, cancellationToken);
            if (readSize != keySize)
                throw new Exception("Failed to exchange key.");
            var result = new byte[readSize];
            Array.Copy(readBuff, result, readSize);
            return result;
        }

        /// <summary>
        /// 发送密钥
        /// </summary>
        protected async Task SendKey(NetworkStream stream, byte[] key, CancellationToken cancellationToken)
        {
            //Send my key
            await stream.WriteAsync(key, 0, key.Length, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        #endregion

    }
}
