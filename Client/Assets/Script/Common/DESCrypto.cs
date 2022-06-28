using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using UnityEngine;


namespace Utilities
{
    public class DESCrypto
    {
        private byte[] rgbIV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        public DESCrypto()
        { }

        public DESCrypto(byte[] rgbIV)
        {
            Array.Resize<byte>(ref rgbIV, 8);
            this.rgbIV = rgbIV;
        }

        public bool TryEncrypt(byte[] origin, byte[] rgbKey, out byte[] encrypted)
        {
            try
            {
                Array.Resize<byte>(ref rgbKey, 8);
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, provider.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(origin, 0, origin.Length);
                cStream.FlushFinalBlock();
                encrypted = mStream.ToArray();
                cStream.Close();
                mStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                DEBUG.LogError("Encryption Failed : " + ex.Message);
                encrypted = null;
                return false;
            }
        }

        public bool TryDecrypt(byte[] encrypted, byte[] rgbKey, out byte[] origin)
        {
            try
            {
                Array.Resize<byte>(ref rgbKey, 8);
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, provider.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(encrypted, 0, encrypted.Length);
                cStream.FlushFinalBlock();
                origin = mStream.ToArray();
                cStream.Close();
                mStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                DEBUG.LogError("Decryption Failed : " + ex.Message);
                origin = null;
                return false;
            }
        }
    }
}