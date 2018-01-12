using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

            String plainText = "Prova testo, non so cosa altro scrivere per allungare questo testo boh boh boh";
            byte[] cipherText;
            string plainDecrypted;

            Aes aes = new AesCng();
            aes.KeySize = 256;
            aes.GenerateIV();
            aes.GenerateKey();

            ICryptoTransform encryptor = aes.CreateEncryptor();
            ICryptoTransform decryptor = aes.CreateDecryptor();

            using (MemoryStream encryptMemStream = new MemoryStream())
            {
                using (CryptoStream encryptCryptoStream = new CryptoStream(encryptMemStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(encryptCryptoStream))
                    {
                        swEncrypt.Write(plainText);
                    }

                    byte[] encrypted = encryptMemStream.ToArray();

                    cipherText = encrypted;

                    Console.WriteLine(Encoding.UTF8.GetString(encrypted));
                }
            }


            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plainDecrypted = srDecrypt.ReadToEnd();
                    }
                }
            }

            Console.WriteLine(plainDecrypted);

            return;
        }
    }
}
