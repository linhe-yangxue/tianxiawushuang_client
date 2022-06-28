using System;
using System.IO;

public static class MD5
{
    public static string CalculateMD5(Stream stream)
    {
        string result = "";

        var provider = new System.Security.Cryptography.MD5CryptoServiceProvider();
        result = BitConverter.ToString(provider.ComputeHash(stream)).Replace("-", "");

        return result;
    }
}