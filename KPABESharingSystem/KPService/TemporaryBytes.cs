using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPServices
{
    public class TemporaryBytes : IDisposable
    {
        private byte[] bytes = null;

        public byte[] Bytes
        {
            get => bytes;
            set
            {
                if(bytes != null)
                    Array.Clear(bytes, 0, bytes.Length);
                bytes = value;
            }
        }

        public static implicit operator byte[](TemporaryBytes tmpBytes)
        {
            return tmpBytes.bytes;
        }

        public void Dispose()
        {
            Array.Clear(bytes, 0, bytes.Length);
            GC.SuppressFinalize(this);
        }

        ~TemporaryBytes()
        {
            Dispose();
        }
    }
}
