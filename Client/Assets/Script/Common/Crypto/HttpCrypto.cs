using UnityEngine;
using System.Collections;
using DataTable;
using System.Text;
using System.IO;
using System;

/// <summary>
/// Http加密错误
/// </summary>
public class HttpCryptoError
{
    public static string CANOT_VERIFY_DATA = "Can't verify data.";
    public static string VERIFY_FAILED = "Verify failed.";
}

public class HttpCrypto
{
    private const string m_CSStartMark = "CS_";
    private const string m_CSSendMark = "PHO";
    private const string m_SCReceiveMark = "UHE";

    private TripleDESCrypto m_TripleDESCrypto;
    private SHA1Crypto m_SHA1Crypto;

    public HttpCrypto()
    {
        m_TripleDESCrypto = new TripleDESCrypto();
        m_SHA1Crypto = new SHA1Crypto();
    }

    public string Encode(string strUrl, string strQuery)
    {
        if (strUrl == null || strQuery == null)
            return "";
        if(m_TripleDESCrypto == null || m_SHA1Crypto == null)
            return "";

		Debug.Log("加密协议222----");
        string tmpStr = strUrl + Convert.ToBase64String(Encoding.UTF8.GetBytes(strQuery));
        int tmpStartIdx = tmpStr.IndexOf(m_CSStartMark);
        if (tmpStartIdx == -1)
            return "";
        string tmpStrFront = tmpStr.Substring(0, tmpStartIdx);
        string tmpStrBack = tmpStr.Substring(tmpStartIdx);
		Debug.Log("加密协议333----");
        byte[] tmpTripleDESValue = m_TripleDESCrypto.Encode(tmpStrBack);
        if (tmpTripleDESValue == null)
            return "";

        string tmpSHA1Value = m_SHA1Crypto.Encode(tmpStrBack);
        if (tmpSHA1Value == "")
            return "";
		Debug.Log("加密协议444----");
        byte[] tmpSHA1Data = Encoding.UTF8.GetBytes(tmpSHA1Value);
        MemoryStream tmpMS = new MemoryStream();
        tmpMS.Write(tmpSHA1Data, 15, 25);
        tmpMS.Write(tmpTripleDESValue, 0, tmpTripleDESValue.Length);
        tmpMS.Write(tmpSHA1Data, 0, 15);
        tmpMS.Seek(0, SeekOrigin.Begin);
        byte[] tmpEncodeData = tmpMS.ToArray();
        tmpMS.Close();
        string tmpStrEncodeBack = Convert.ToBase64String(tmpEncodeData);

        return (tmpStrFront + m_CSSendMark + tmpStrEncodeBack);
    }
    public string Decode(string data)
    {
        if (data == null)
            return HttpCryptoError.CANOT_VERIFY_DATA;
        if (m_TripleDESCrypto == null || m_SHA1Crypto == null)
            return HttpCryptoError.CANOT_VERIFY_DATA;

        int tmpStartIdx = data.IndexOf(m_SCReceiveMark);
        if (tmpStartIdx == -1)
            return HttpCryptoError.CANOT_VERIFY_DATA;
        tmpStartIdx += m_SCReceiveMark.Length;
        if (tmpStartIdx >= data.Length)
            return HttpCryptoError.CANOT_VERIFY_DATA;
        string tmpStrFront = data.Substring(0, tmpStartIdx - m_SCReceiveMark.Length);
        string tmpStrBack = data.Substring(tmpStartIdx);

        byte[] tmpDecodeData = Convert.FromBase64String(tmpStrBack);
        MemoryStream tmpMS = new MemoryStream(tmpDecodeData);
        //SHA1
        byte[] tmpSHA1Data = new byte[40];
        tmpMS.Seek(-15, SeekOrigin.End);
        tmpMS.Read(tmpSHA1Data, 0, 15);
        tmpMS.Seek(0, SeekOrigin.Begin);
        tmpMS.Read(tmpSHA1Data, 15, 25);
        string tmpSHA1Value = Encoding.UTF8.GetString(tmpSHA1Data);
        //3DES
        byte[] tmpTripleDESValue = new byte[tmpDecodeData.Length - 40];
        tmpMS.Read(tmpTripleDESValue, 0, tmpTripleDESValue.Length);
        string tmpStrDecodeBack = m_TripleDESCrypto.Decode(tmpTripleDESValue);
        if (tmpStrDecodeBack == "")
            return HttpCryptoError.VERIFY_FAILED;
        //Verify
        string tmpCurrSHA1Value = m_SHA1Crypto.Encode(tmpStrDecodeBack);
        if (tmpSHA1Value != tmpCurrSHA1Value)
            return HttpCryptoError.VERIFY_FAILED;

        return (tmpStrFront + tmpStrDecodeBack);
    }
}
