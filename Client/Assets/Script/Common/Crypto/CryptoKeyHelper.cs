using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CryptoKeyType
{
    ConfigAES,
    HttpTripleDES
}

public class CryptoKeyHelper
{
    private static CryptoKeyHelper msInstance = new CryptoKeyHelper();

    private Dictionary<CryptoKeyType, GameMD5> mDicMD5 = new Dictionary<CryptoKeyType, GameMD5>();
    private Dictionary<CryptoKeyType, int[]> mDicIntData = new Dictionary<CryptoKeyType, int[]>();

    public CryptoKeyHelper()
    {
        __DicMD5Init();
        __DicIntData();
    } 

    public static CryptoKeyHelper Instance
    {
        get { return msInstance; }
    }

    private void __DicMD5Init()
    {
        mDicMD5.Add(CryptoKeyType.ConfigAES, new GameMD5(false, true) { md5 = "6C8B466CBE0D53EC1C861516232971EC" });
    }
    private void __DicIntData()
    {
        mDicIntData.Add(CryptoKeyType.HttpTripleDES, new int[] {
            66, 39, 102, 80, 252, 191, 74, 108,
            102, 176, 198, 140, 42, 48, 187, 203,
            39, 222, 230, 224, 157, 245, 81, 173
        });
    }

    public string GetMD5(CryptoKeyType md5Type)
    {
        if (mDicMD5 == null || mDicMD5.Count <= 0)
            return "";

        GameMD5 tmpMD5 = null;
        if (!mDicMD5.TryGetValue(md5Type, out tmpMD5))
            return "";
        return tmpMD5.md5;
    }
    public int[] GetIntData(CryptoKeyType intDataType)
    {
        if (mDicIntData == null || mDicIntData.Count <= 0)
            return null;

        int[] tmpIntData = null;
        if (!mDicIntData.TryGetValue(intDataType, out tmpIntData))
            return null;
        return tmpIntData;
    }
}
