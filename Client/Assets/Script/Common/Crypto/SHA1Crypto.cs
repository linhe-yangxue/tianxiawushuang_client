using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System;

public class SHA1Crypto
{
    private SHA1CryptoServiceProvider m_SHA1Crypto;

    public SHA1Crypto()
    {
        m_SHA1Crypto = new SHA1CryptoServiceProvider();
    }

    public string Encode(string str)
    {
        if (m_SHA1Crypto == null)
            return "";

        string tmpStrEncode = "";
        try
        {
            byte[] tmpBuffer = Encoding.UTF8.GetBytes(str);
            tmpStrEncode = BitConverter.ToString(m_SHA1Crypto.ComputeHash(tmpBuffer)).Replace("-", "").ToLower();
        }
        catch (Exception excep)
        {
            return "";
        }
        return tmpStrEncode;
    }
}
