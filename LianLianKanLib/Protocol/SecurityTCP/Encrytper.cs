using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol.SecurityTCP
{
    public class Encrytper : IDisposable
    {
        #region 属性与变量

        private CngKey _crypKey;

        public byte[] CrypPubKey { get; set; }

        #endregion

        #region 方法

        public Encrytper()
        {
            //创建私钥
            this._crypKey = CngKey.Create(CngAlgorithm.ECDiffieHellmanP521);
            //通过私钥，生成公钥
            this.CrypPubKey = this._crypKey.Export(CngKeyBlobFormat.EccPublicBlob);
        }

        /// <summary>
        /// 加密数据
        /// </summary>
        public int EncrytpData(byte[] pubKey, byte[] input, int offset, int inputSize, out byte[] output)
        {
            //通过私钥，生成算法，然后导入公钥。
            using (var keyAlg = new ECDiffieHellmanCng(this._crypKey))
            using (var otherKey = CngKey.Import(pubKey, CngKeyBlobFormat.EccPublicBlob))
            {
                //根据私钥算法和公钥，生成新的密钥
                var symmKey = keyAlg.DeriveKeyMaterial(otherKey);
                //AES算法提供者
                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.Key = symmKey;
                    aes.GenerateIV();
                    int ivSize = aes.IV.Count();
                    //生成加密者
                    using (var encryptor = aes.CreateEncryptor())
                    using (var memStream = new MemoryStream())
                    { 
                        using (var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                        {
                            //先写IV向量
                            memStream.Write(aes.IV, 0, ivSize);
                            //再写数据并加密
                            cryptoStream.Write(input, offset, inputSize);
                        }
                        output = memStream.ToArray();
                        return output.Count();
                    }
                }
            }
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        public int DecrytpData(byte[] pubKey, byte[] input, int offset, int inputSize, out byte[] output)
        {
            output = null;
            //通过私钥，生成算法，然后导入公钥。
            using (var KeyAlg = new ECDiffieHellmanCng(this._crypKey))
            using (var otherKey = CngKey.Import(pubKey, CngKeyBlobFormat.EccPublicBlob))
            {
                //根据私钥算法和公钥，生成新的密钥
                var sysmmKey = KeyAlg.DeriveKeyMaterial(otherKey);
                //AES算法提供者
                using (var aes = new AesCryptoServiceProvider())
                {
                    int ivSize = aes.BlockSize >> 3;
                    if (inputSize < ivSize)
                        return 0;
                    int rawDataSize = inputSize - ivSize;
                    ArraySegment<byte> seg = new ArraySegment<byte>(input, offset, ivSize);
                    aes.IV = seg.ToArray();
                    aes.Key = sysmmKey;
                    //生成加密者
                    using (var decryptor = aes.CreateDecryptor())
                    using (var memStream = new MemoryStream())
                    {
                        //重新加密就是解密
                        using (var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(input, ivSize + offset, rawDataSize);
                        }
                        output = memStream.ToArray();
                        return output.Length;
                    }
                }
            }
        }

        public void Dispose()
        {
            this._crypKey.Dispose();
        }

        #endregion

    }
}
