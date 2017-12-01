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
            if(Key == null || IV == null)
                throw new InvalidOperationException("Undefined Key or IV, cannot compute next");

            var sha = new SHA256Cng();
            var next = new SymmetricKey
            {
                Key = sha.ComputeHash(Key),
                IV = sha.ComputeHash(IV).Take(128 / 8).ToArray()
            };
            return next;
        }

        public void Encrypt(Stream inputStream, Stream outputStream)
        {
            Aes aes = new AesCng();
            aes.KeySize = 256;
            aes.Key = Key;
            aes.IV = IV; //always 128 bits

            var encryptor = aes.CreateEncryptor();

            using (CryptoStream encryptStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
            {
                inputStream.CopyTo(encryptStream);
            }
        }

        public void Decrypt(Stream inputStream, Stream outputStream)
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
