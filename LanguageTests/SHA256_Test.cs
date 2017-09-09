using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            string text = "Calcola il valore hash della matrice di byte specificata.(Ereditato da HashAlgorithm.)";
            byte[] byteText = Encoding.UTF8.GetBytes(text);
            MemoryStream msHash = new MemoryStream(byteText);

            SHA256 sha = new SHA256Cng();

            byte[] hash = sha.ComputeHash(msHash);

            Console.WriteLine(BitConverter.ToString(hash).Replace("-", string.Empty));

            Console.ReadLine();
            return;
        }
    }
}
