using System.Collections;
using System.Text;
using System.Security.Cryptography;
using System;

public class TripleDESCrypto
{
    private byte[] m_TripleDESKey;
    private byte[] m_TripleDESIV = new byte[] { 199, 45, 249, 137, 145, 11, 155, 107 };
    private TripleDESCryptoServiceProvider m_TripleDESCypto;

    public TripleDESCrypto()
    {
        m_TripleDESCypto = new TripleDESCryptoServiceProvider();
        m_TripleDESCypto.Mode = CipherMode.ECB;

        m_TripleDESKey = __ParseKey();
    }

    public byte[] Encode(string str)
    {
        if (m_TripleDESKey == null)
            return null;

        byte[] tmpBuffer = Encoding.UTF8.GetBytes(str);
        try
        {
            ICryptoTransform tmpTripleDESEncrypt = m_TripleDESCypto.CreateEncryptor(m_TripleDESKey, m_TripleDESIV);
            tmpBuffer = tmpTripleDESEncrypt.TransformFinalBlock(tmpBuffer, 0, tmpBuffer.Length);
        }
        catch (Exception excep)
        {
            return null;
        }
        return tmpBuffer;
    }
    public string Decode(byte[] data)
    {
        if (m_TripleDESKey == null)
            return "";

        byte[] tmpBuffer = null;
        try
        {
            ICryptoTransform tmpTripleDESDecrypt = m_TripleDESCypto.CreateDecryptor(m_TripleDESKey, m_TripleDESIV);
            tmpBuffer = tmpTripleDESDecrypt.TransformFinalBlock(data, 0, data.Length);
        }
        catch (Exception excep)
        {
            return "";
        }
        string tmpStr = Encoding.UTF8.GetString(tmpBuffer);
        return tmpStr;
    }

    private byte[] __ParseKey()
    {
        int[] tmpIndex = null;
        tmpIndex = CryptoKeyHelper.Instance.GetIntData(CryptoKeyType.HttpTripleDES);
        byte[] tmpKey = new byte[tmpIndex.Length];
//        DllKeyAccessor.GetGameBytesValueByIndex(tmpIndex, tmpKey, tmpIndex.Length);
        return tmpKey;
    }
}
