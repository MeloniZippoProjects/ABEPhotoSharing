using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KPClient
{
    public class SymmetricKey
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }

        public SymmetricKey GetNextKey()
        {
            var sha = new SHA256Cng();
            var nextSymmetricKey = new SymmetricKey();
            nextSymmetricKey.Key = sha.ComputeHash(nextSymmetricKey.IV);
            nextSymmetricKey.IV = sha.ComputeHash(nextSymmetricKey.IV).Take(128 / 8).ToArray();
            return nextSymmetricKey;
        }

        public void EncryptFile(Stream inputStream, Stream outputStream)
        {
            Aes aes = new AesCng();
            aes.KeySize = 256;
            aes.Key = Key;
            aes.IV = IV; //always 128 bits

            var encryptor = aes.CreateEncryptor();

            var decryptor = aes.CreateDecryptor();
            using (CryptoStream decryptCryptoStream = new CryptoStream(outputStream, decryptor, CryptoStreamMode.Write))
            {
                inputStream.CopyTo(decryptCryptoStream);
            }
        }

        public void DecryptFile(Stream inputStream, Stream outputStream)
        {
            Aes aes = new AesCng();
            aes.KeySize = 256;
            aes.Key = Key;
            aes.IV = IV; //always 128 bits

            var decryptor = aes.CreateDecryptor();
            using (CryptoStream decryptCryptoStream = new CryptoStream(outputStream, decryptor, CryptoStreamMode.Write))
            {
                inputStream.CopyTo(decryptCryptoStream);
            }
        }
    }
}
