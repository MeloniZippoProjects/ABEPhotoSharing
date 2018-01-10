using System;
using System.Security.Cryptography;
using KPServices;

public class SecureBytes
{
    private byte[] protectedBytes;
    public byte[] ProtectedBytes
    {
        get{
            byte[] unprotectedBytes = (byte[]) protectedBytes.Clone();
            ProtectedMemory.Unprotect( unprotectedBytes, MemoryProtectionScope.SameProcess);
            return unprotectedBytes;
        }
        set{
            protectedBytes = (byte[]) value.Clone();
            ProtectedMemory.Protect( protectedBytes, MemoryProtectionScope.SameProcess);
        }

    }

    public static implicit operator TemporaryBytes (SecureBytes secureBytes)
    {
        return new TemporaryBytes {Bytes = secureBytes.ProtectedBytes};
    }

    public static implicit operator SecureBytes(byte[] bytes)
    {
        return new SecureBytes {ProtectedBytes = bytes};
    }
}