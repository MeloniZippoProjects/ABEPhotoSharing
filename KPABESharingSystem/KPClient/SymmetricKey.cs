using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using KPServices;

namespace KPClient
{
    public class SymmetricKey
    {
        public SecureBytes Key { get; set; }
        public SecureBytes Iv { get; set; }

        public void GenerateKey()
        {
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            using (TemporaryBytes key = new byte[256 / 8], iv = new byte[128 / 8])
            {
                rngCsp.GetBytes(key);
                rngCsp.GetBytes(iv);
                Key = key.Bytes;
                Iv = iv.Bytes;
            }
        }

        public SymmetricKey GetNextKey()
        {
            if (Key == null || Iv == null)
                throw new InvalidOperationException("Undefined Key or IV, cannot compute next");

            using (SHA256Cng sha = new SHA256Cng())
            {
                using (TemporaryBytes key = Key, iv = Iv)
                {
                    using (TemporaryBytes nextKey = sha.ComputeHash(key),
                        nextIv = sha.ComputeHash(iv).Take(128 / 8).ToArray())
                    {
                        SymmetricKey next = new SymmetricKey
                        {
                            Key = nextKey.Bytes,
                            Iv = nextIv.Bytes
                        };
                        return next;
                    }
                }
            }
        }

        public async Task Encrypt(Stream inputStream, Stream outputStream)
        {
            using (TemporaryBytes key = Key, iv = Iv)
            {
                Aes aes = new AesCng
                {
                    KeySize = 256,
                    Key = key,
                    IV = iv
                };

                ICryptoTransform encryptor = aes.CreateEncryptor();
                using (CryptoStream encryptStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                {
                    await inputStream.CopyToAsync(encryptStream);
                }
            }
        }

        public async Task Decrypt(Stream inputStream, Stream outputStream)
        {
            using (TemporaryBytes key = Key, iv = Iv)
            {
                Aes aes = new AesCng
                {
                    KeySize = 256,
                    Key = key,
                    IV = iv
                };

                ICryptoTransform decryptor = aes.CreateDecryptor();
                using (CryptoStream decryptCryptoStream = new CryptoStream(outputStream, decryptor, CryptoStreamMode.Write))
                {
                    await inputStream.CopyToAsync(decryptCryptoStream);
                }
            }
        }
    }
}