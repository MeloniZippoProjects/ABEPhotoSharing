using System;
using System.Security.Cryptography;

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
}