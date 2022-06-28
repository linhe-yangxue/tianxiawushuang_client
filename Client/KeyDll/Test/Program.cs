using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Test
{
    class Program
    {
        [DllImport("KeyDll", EntryPoint = "GetGameStringValueBySign")]
        public extern static void GetGameStringValueBySign(string sz, StringBuilder sz2, int count);
        [DllImport("KeyDll", EntryPoint = "GetGameBytesValueBySign")]
        public extern static void GetGameBytesValueBySign(string sz, byte[] bytes, int count);
        [DllImport("KeyDll", EntryPoint = "GetGameBytesValueByIndex")]
        public extern static void GetGameBytesValueByIndex(int[] sz, byte[] bytes, int count);

        static void Main(string[] args)
        {
            string tmpTestMD5 = "6C8B466CBE0D53EC1C861516232971EC";
            byte[] tmpTestBytes = new byte[32];
            GetGameBytesValueBySign(tmpTestMD5, tmpTestBytes, tmpTestMD5.Length);
        }
    }
}
