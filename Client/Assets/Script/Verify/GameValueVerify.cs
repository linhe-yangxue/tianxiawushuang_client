using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

class GameMD5Element
{
    private int m_Index = -1;
    private string m_MD5Fragment = "";

    public int Index
    {
        set { m_Index = value; }
        get { return m_Index; }
    }
    public string MD5Fragment
    {
        set { m_MD5Fragment = value; }
        get { return m_MD5Fragment; }
    }
}

public class GameMD5
{
    private string m_Salt = "chenliang";
    private bool m_IsUseSalt = true;
    private bool m_IsJustRestore = false;       //是否仅存储传入的MD5值
    private int m_ChangeDelta = 1;
    private int m_MaxMD5Fragment = 3;
    private List<GameMD5Element> m_ListElements;

    public GameMD5()
    {
        m_ChangeDelta = Math.Min(2, GameValueVerify.IntRandom);
    }
    public GameMD5(bool isUseSalt, bool isJustRestore)
    {
        m_IsUseSalt = isUseSalt;
        m_IsJustRestore = isJustRestore;
        m_ChangeDelta = Math.Min(2, GameValueVerify.IntRandom);
    }

    public string Salt
    {
        set
        {
            m_Salt = value;
            if (m_Salt == "")
                m_Salt = "chenliang";
        }
        get { return m_Salt; }
    }
    public bool IsUseSalt
    {
        get { return m_IsUseSalt; }
    }
    /// <summary>
    /// 是否仅存储传入的MD5值
    /// </summary>
    public bool IsJustRestore
    {
        get { return m_IsJustRestore; }
    }
    public string md5
    {
        set
        {
            string tmpMD5 = "";
            if (m_IsJustRestore)
                tmpMD5 = value;
            else
                tmpMD5 = CalcMD5(value);
            m_ListElements = __Split(tmpMD5);
        }
        get { return __Merge(m_ListElements); }
    }

    /// <summary>
    /// 获取指定值得MD5
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string CalcMD5(object value)
    {
        if (value == null)
            return "";

        string tmpSrcMD5 = value.ToString() + (m_IsUseSalt ? m_Salt : "");
        MemoryStream tmpMS = new MemoryStream(Encoding.UTF8.GetBytes(tmpSrcMD5));
        string tmpStrMD5 = MD5.CalculateMD5(tmpMS);
        return tmpStrMD5;
    }
    /// <summary>
    /// 验证指定值得MD5是否和当前MD5值相等
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool CheckMD5(object value)
    {
        string tmpMD5 = CalcMD5(value);
        return (md5 == tmpMD5);
    }

    private List<GameMD5Element> __Split(string value)
    {
        List<GameMD5Element> tmpListElements = new List<GameMD5Element>();
        string tmpValue = value.Clone().ToString();
        while (tmpValue.Length > 0)
        {
            int tmpRandomIndex = GameValueVerify.IntRandom % Math.Min(tmpValue.Length, m_MaxMD5Fragment);
            string tmpSubValue = tmpValue.Substring(0, tmpRandomIndex + 1);
            int tmpSubIndex = tmpRandomIndex + 1;
            if (tmpSubIndex >= tmpValue.Length)
                tmpValue = "";
            else
                tmpValue = tmpValue.Substring(tmpSubIndex);
            GameMD5Element tmpMD5Elem = new GameMD5Element()
            {
                Index = tmpListElements.Count,
                MD5Fragment = tmpSubValue
            };
            tmpListElements.Add(tmpMD5Elem);
        }
        int tmpListCount = tmpListElements.Count;
        if (tmpListCount > 1)
            m_ChangeDelta = GameValueVerify.InitRandomRange(1, tmpListCount - 1);
        else
            m_ChangeDelta = 1;
        __Encode(tmpListElements);
        return tmpListElements;
    }
    private string __Merge(List<GameMD5Element> listElements)
    {
        if (listElements == null)
            return "";

        List<GameMD5Element> tmpListElements = new List<GameMD5Element>();
        tmpListElements.AddRange(listElements);
        __Decode(tmpListElements);
        string tmpMD5 = "";
        for (int i = 0, count = tmpListElements.Count; i < count; i++)
        {
            GameMD5Element tmpMD5Elem = tmpListElements[i];
            tmpMD5 += tmpMD5Elem.MD5Fragment;
        }
        return tmpMD5;
    }

    private void __Encode(List<GameMD5Element> listElements)
    {
        if (listElements == null)
            return;

        for (int i = 0, count = listElements.Count; i < count; i++)
        {
            int tmpDstI = (i + m_ChangeDelta) % count;
            GameMD5Element tmpMD5Elem = listElements[i];
            listElements[i] = listElements[tmpDstI];
            listElements[tmpDstI] = tmpMD5Elem;
        }
    }
    private void __Decode(List<GameMD5Element> listElements)
    {
        if (listElements == null)
            return;

        for (int i = listElements.Count - 1, count = listElements.Count; i >= 0; i--)
        {
            int tmpSrcI = (i + m_ChangeDelta) % count;
            GameMD5Element tmpMD5Elem = listElements[tmpSrcI];
            listElements[tmpSrcI] = listElements[i];
            listElements[i] = tmpMD5Elem;
        }
    }
}

/// <summary>
/// 游戏数值验证
/// </summary>
public class GameValueVerify
{
    private static GameValueVerify ms_Instance;

    private Dictionary<string, GameMD5> m_DicMD5 = new Dictionary<string, GameMD5>();       //存储MD5值

    private GameValueVerify()
    {
    }

    public static GameValueVerify Instance
    {
        get
        {
            if (ms_Instance == null)
                ms_Instance = new GameValueVerify();
            return ms_Instance;
        }
    }

    public void ClearAllData()
    {
        if (m_DicMD5 == null)
            return;

        m_DicMD5.Clear();
    }

    public static double DoubleRandom
    {
        get
        {
            DateTime tmpCurrDate = new DateTime();
            int tmpSeed = Convert.ToInt32(tmpCurrDate.Ticks / TimeSpan.TicksPerMinute);
            Random tmpRandom = new Random(tmpSeed);

            double tmpRandomSalt = 0.0;
            tmpRandomSalt = tmpRandom.NextDouble();
            return tmpRandomSalt;
        }
    }
    public static int IntRandom
    {
        get
        {
            DateTime tmpCurrDate = DateTime.Now;
            int tmpSeed = Convert.ToInt32(tmpCurrDate.Ticks % int.MaxValue);
            Random tmpRandom = new Random(tmpSeed);

            int tmpRandomSalt = 0;
            tmpRandomSalt = tmpRandom.Next();
            return tmpRandomSalt;
        }
    }
    public static int InitRandomRange(int min, int max)
    {
        DateTime tmpCurrDate = new DateTime();
        int tmpSeed = Convert.ToInt32(tmpCurrDate.Ticks / TimeSpan.TicksPerMinute);
        Random tmpRandom = new Random(tmpSeed);

        int tmpRandomSalt = 0;
        tmpRandomSalt = tmpRandom.Next(min, max);
        return tmpRandomSalt;
    }

    public string CreateNewKey()
    {
        string tmpKey = "";
        DateTime tmpCurrDate = new DateTime();
        double tmpRandom = DoubleRandom;
        tmpKey = __GetMD5(tmpCurrDate.ToLongTimeString() + tmpRandom);
        return tmpKey;
    }

    /// <summary>
    /// 更新指定键值的MD5
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value">为null删除键值</param>
    public void Update(string key, object value)
    {
        if (key == null)
            return;
        if (m_DicMD5 == null)
            return;

        if (value == null)
        {
            m_DicMD5.Remove(key);
            return;
        }
        GameMD5 tmpGameMD5 = new GameMD5();
        tmpGameMD5.md5 = value.ToString();
        m_DicMD5[key] = tmpGameMD5;
    }
    /// <summary>
    /// 验证指定值得MD5
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Verify(string key, object value)
    {
        if (key == null || value == null)
            return false;
        if (m_DicMD5 == null)
            return false;

        string tmpSrcMD5 = "";
        GameMD5 tmpSrcGameMD5 = null;
        if (!m_DicMD5.TryGetValue(key, out tmpSrcGameMD5))
            return false;
        tmpSrcMD5 = tmpSrcGameMD5.md5;
        string tmpStrMD5 = tmpSrcGameMD5.CalcMD5(value);
        if (tmpStrMD5 == "")
            return false;
        return (tmpSrcMD5 == tmpStrMD5);
    }
    public bool HasKey(string key)
    {
        if (m_DicMD5 == null)
            return false;

        return m_DicMD5.ContainsKey(key);
    }

    /// <summary>
    /// 获取指定值得MD5
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private string __GetMD5(object value)
    {
        if (value == null)
            return "";

        MemoryStream tmpMS = new MemoryStream(Encoding.UTF8.GetBytes(value.ToString()));
        string tmpStrMD5 = MD5.CalculateMD5(tmpMS);
        return tmpStrMD5;
    }
}
