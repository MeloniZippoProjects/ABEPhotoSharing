using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace KPClient
{
    public class SymmetricKey
    {
        public byte[] Key { get; set; }
        public byte[] Iv { get; set; }

        public SymmetricKey GetNextKey()
        {
            if (Key == null || Iv == null)
                throw new InvalidOperationException("Undefined Key or IV, cannot compute next");

            SHA256Cng sha = new SHA256Cng();
            SymmetricKey next = new SymmetricKey
            {
                Key = sha.ComputeHash(Key),
                Iv = sha.ComputeHash(Iv).Take(128 / 8).ToArray()
            };
            return next;
        }

        public void Encrypt(Stream inputStream, Stream outputStream)
        {
            Aes aes = new AesCng();
            aes.KeySize = 256;
            aes.Key = Key;
            aes.IV = Iv; //always 128 bits

            ICryptoTransform encryptor = aes.CreateEncryptor();

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
            aes.IV = Iv; //always 128 bits

            ICryptoTransform decryptor = aes.CreateDecryptor();
            using (CryptoStream decryptCryptoStream = new CryptoStream(outputStream, decryptor, CryptoStreamMode.Write))
            {
                inputStream.CopyTo(decryptCryptoStream);
            }
        }
    }
}