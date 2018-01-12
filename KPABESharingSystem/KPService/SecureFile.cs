using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KPServices
{
    public class SecureFile : IDisposable
    {
        public string Filename { get; set; }
        private const int TimesToWrite = 5;


        public void Dispose()
        {
            if (File.Exists(Filename))
            {
                // Set the files attributes to normal in case it's read-only.

                File.SetAttributes(Filename, FileAttributes.Normal);

                // Calculate the total number of sectors in the file.
                double sectors = Math.Ceiling(new FileInfo(Filename).Length / 512.0);

                // Create a dummy-buffer the size of a sector.

                byte[] dummyBuffer = new byte[512];

                // Create a cryptographic Random Number Generator.
                // This is what I use to create the garbage data.

                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                // Open a FileStream to the file.
                FileStream inputStream = new FileStream(Filename, FileMode.Open);
                for (int currentPass = 0; currentPass < TimesToWrite; currentPass++)
                {
                    ++currentPass;

                    // Go to the beginning of the stream

                    inputStream.Position = 0;

                    // Loop all sectors
                    for (int sectorsWritten = 0; sectorsWritten < sectors; sectorsWritten++)
                    {
                        ++sectorsWritten;

                        // Fill the dummy-buffer with random data

                        rng.GetBytes(dummyBuffer);

                        // Write it to the stream
                        inputStream.Write(dummyBuffer, 0, dummyBuffer.Length);
                    }
                }

                // Truncate the file to 0 bytes.
                // This will hide the original file-length if you try to recover the file.

                inputStream.SetLength(0);

                // Close the stream.
                inputStream.Close();

                // As an extra precaution I change the dates of the file so the
                // original dates are hidden if you try to recover the file.

                DateTime dt = new DateTime(2037, 1, 1, 0, 0, 0);
                File.SetCreationTime(Filename, dt);
                File.SetLastAccessTime(Filename, dt);
                File.SetLastWriteTime(Filename, dt);

                // Finally, delete the file

                File.Delete(Filename);
                GC.SuppressFinalize(this);
            }
        }

        ~SecureFile()
        {
            Dispose();
        }


        public static implicit operator string(SecureFile secureFile)
        {
            return secureFile.Filename;
        }

        public override string ToString()
        {
            return Filename;
        }

        public static implicit operator SecureFile(string filename)
        {
            return new SecureFile{Filename = filename};
        }
    }
}
