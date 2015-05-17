using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Tesis_WebService
{
    public static class Logic
    {
        public static bool PasswordHash(string Password, string PasswordHash)
        {
            if (PasswordHash == null) { return false; }
            if (Password == null)  { throw new ArgumentNullException("password"); }

            byte[] buffer4;
            byte[] src = Convert.FromBase64String(PasswordHash);

            if ((src.Length != 0x31) || (src[0] != 0)) { return false; }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);

            Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(Password, dst, 0x3e8);
            buffer4 = bytes.GetBytes(0x20);

            return ByteArraysEqual(buffer3, buffer4);
        }
        private static bool ByteArraysEqual(byte[] b0, byte[] b1)
        {
            if (b0 == b1) { return true; }
            if (b0 == null || b1 == null) { return false; }
            if (b0.Length != b1.Length) { return false; }

            for (int i = 0; i < b0.Length; i++)
            {
                if (b0[i] != b1[i]) { return false; }
            }

            return true;
        }
    }
}