using System;
using System.Security.Cryptography;
using KPServices;

public class SecureBytes
{
    private byte[] protectedBytes;
    public byte[] ProtectedBytes
    {
        get
        {
            using (TemporaryBytes tb = (byte[]) protectedBytes.Clone())
            {
                ProtectedMemory.Unprotect(tb.Bytes, MemoryProtectionScope.SameProcess);
                int paddingLength = tb.Bytes[tb.Bytes.Length - 1];
                int contentLength = tb.Bytes.Length - paddingLength;
                byte[] unprotectedBytes = new byte[contentLength];
                Array.Copy(sourceArray: tb, destinationArray:unprotectedBytes, length: contentLength);
                return unprotectedBytes;
            }
        }
        set
        {
            int paddedLength =  ((int) Math.Ceiling((decimal) (value.Length + 1) / 16)) * 16;
            protectedBytes = new byte[paddedLength];
            Array.Copy(sourceArray: value, destinationArray: protectedBytes, length: value.Length);
            for (int idx = value.Length; idx < protectedBytes.Length; idx++)
                protectedBytes[idx] = (byte) (paddedLength - value.Length);
            ProtectedMemory.Protect(protectedBytes, MemoryProtectionScope.SameProcess);
        }
    }

    public static implicit operator TemporaryBytes (SecureBytes secureBytes)
    {
        return new TemporaryBytes {Bytes = secureBytes.ProtectedBytes};
    }

    public static implicit operator SecureBytes (byte[] bytes)
    {
        return new SecureBytes {ProtectedBytes = bytes};
    }

    public static implicit operator SecureBytes(TemporaryBytes temporaryBytes)
    {
        return new SecureBytes {ProtectedBytes = temporaryBytes};
    }
}