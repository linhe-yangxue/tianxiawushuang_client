using System.Collections;
using System.Security.Cryptography;
using System.IO;
using System.Text;

public class AESCrypto
{
    private byte[] m_Key;
    private byte[] m_IV = new byte[]
    {
        33,98,60,117,91,170,20,253,
        115,108,87,32,160,106,143,250
    };
    private Rijndael m_AESCrypto;

    public AESCrypto()
    {
        m_AESCrypto = Rijndael.Create();

        m_Key = __ParseKey();
    }

    public byte[] Encode(byte[] data)
    {
        if (data == null)
            return null;
        if (m_Key == null || m_AESCrypto == null)
            return null;

        MemoryStream tmpMS = new MemoryStream();
        CryptoStream tmpCS = new CryptoStream(tmpMS, m_AESCrypto.CreateEncryptor(m_Key, m_IV), CryptoStreamMode.Write);
        tmpCS.Write(data, 0, data.Length);
        tmpCS.FlushFinalBlock();
        byte[] tmpEncode = tmpMS.ToArray();
        tmpCS.Close();
        tmpMS.Close();

        return tmpEncode;
    }
    public byte[] Decode(byte[] data)
    {
        if (data == null)
            return null;
        if (m_Key == null || m_AESCrypto == null)
            return null;

        MemoryStream tmpMS = new MemoryStream();
        CryptoStream tmpCS = new CryptoStream(tmpMS, m_AESCrypto.CreateDecryptor(m_Key, m_IV), CryptoStreamMode.Write);
        tmpCS.Write(data, 0, data.Length);
        tmpCS.FlushFinalBlock();
        byte[] tmpBytesDecode = tmpMS.ToArray();
        tmpCS.Close();
        tmpMS.Close();

        return tmpBytesDecode;
    }

    private byte[] __ParseKey()
    {
        string tmpMD5 = "";
        tmpMD5 = CryptoKeyHelper.Instance.GetMD5(CryptoKeyType.ConfigAES);
        byte[] tmpKey = new byte[32];
//        DllKeyAccessor.GetGameBytesValueBySign(tmpMD5, tmpKey, tmpMD5.Length);
        return tmpKey;
    }
}
