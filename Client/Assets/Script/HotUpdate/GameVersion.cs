using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum GAME_VERSION_FIELD
{
    EXPANSION_PACK,         //资料片
    FUNCITON,               //功能
    EMERGENCY,              //紧急
    PATCH,                  //Patch
    SVN_TAG,                //SVN tag号
    FIELD_MAX               //最大字段
}
public enum GAME_VERSION_FIELD_DIFF
{
    HIGH,           //版本号高
    EQUAL,          //版本号相同
    LOW             //版本号低
}
/// <summary>
/// 客户端版本
/// </summary>
public class GameVersion
{
    private int[] mFields = new int[(int)GAME_VERSION_FIELD.FIELD_MAX];   //版本号字段数据

    public GameVersion()
    {
        for (int i = 0, count = mFields.Length; i < count; i++)
            mFields[i] = 0;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strVer">每个版本字段以"."分开</param>
    public GameVersion(string strVer)
    {
        ReadFrom(strVer);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ver">版本字段数组</param>
    public GameVersion(IEnumerable<int> ver)
    {
        ReadFrom(ver);
    }

    /// <summary>
    /// 获取两个版本的字段值差异
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    /// <returns>只返回有差异的字段名</returns>
    public static List<GAME_VERSION_FIELD> GetDifferentVersionField(GameVersion src, GameVersion dst)
    {
        int tmpMaxFieldCount = (int)GAME_VERSION_FIELD.FIELD_MAX;
        List<GAME_VERSION_FIELD> tmpFields = new List<GAME_VERSION_FIELD>();
        for (int i = 0; i < tmpMaxFieldCount; i++)
        {
            GAME_VERSION_FIELD tmpField = (GAME_VERSION_FIELD)i;
            if (src[tmpField] != dst[tmpField])
                tmpFields.Add(tmpField);
        }
        return tmpFields;
    }
    /// <summary>
    /// 获取两个版本之间每个字段的差异，src作为比较的左值
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    /// <returns></returns>
    public static List<GAME_VERSION_FIELD_DIFF> GetFullDifferentInfo(GameVersion src, GameVersion dst)
    {
        int tmpMaxFieldCount = (int)GAME_VERSION_FIELD.FIELD_MAX;
        List<GAME_VERSION_FIELD_DIFF> tmpFieldsDiff = new List<GAME_VERSION_FIELD_DIFF>();
        for (int i = 0; i < tmpMaxFieldCount; i++)
        {
            GAME_VERSION_FIELD tmpField = (GAME_VERSION_FIELD)i;
            int tmpSrcFieldValue = src[tmpField];
            int tmpDstFieldValue = dst[tmpField];
            if (tmpSrcFieldValue > tmpDstFieldValue)
                tmpFieldsDiff.Add(GAME_VERSION_FIELD_DIFF.HIGH);
            else if (tmpSrcFieldValue == tmpDstFieldValue)
                tmpFieldsDiff.Add(GAME_VERSION_FIELD_DIFF.EQUAL);
            else
                tmpFieldsDiff.Add(GAME_VERSION_FIELD_DIFF.LOW);
        }
        return tmpFieldsDiff;
    }

    /// <summary>
    /// 从指定字符串读取版本数据
    /// </summary>
    /// <param name="strVer">每个版本字段以"."分开</param>
    public void ReadFrom(string strVer)
    {
        string[] tmpVerFields = strVer.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

        int tmpMaxFieldCount = (int)GAME_VERSION_FIELD.FIELD_MAX;
        int[] tmpFields = new int[tmpMaxFieldCount];
        int tmpVerFieldsCount = tmpVerFields.Length;
        for (int i = 0; i < tmpMaxFieldCount; i++)
        {
            if (i < tmpVerFieldsCount)
                int.TryParse(tmpVerFields[i], out tmpFields[i]);
            else
                tmpFields[i] = 0;
        }
        for (int i = 0; i < tmpMaxFieldCount; i++)
            mFields[i] = tmpFields[i];
    }
    /// <summary>
    /// 从指定版本字段数组中读取版本数据
    /// </summary>
    /// <param name="ver">版本字段数组</param>
    public void ReadFrom(IEnumerable<int> ver)
    {
        List<int> tmpVer = new List<int>();
        foreach (int tmpVerFieldValue in ver)
            tmpVer.Add(tmpVerFieldValue);
        int tmpMaxFieldCount = (int)GAME_VERSION_FIELD.FIELD_MAX;
        int tmpVerCount = tmpVer.Count;
        for (int i = 0; i < tmpMaxFieldCount; i++)
        {
            if (i < tmpVerCount)
                mFields[i] = tmpVer[i];
            else
                mFields[i] = 0;
        }
    }

    public bool IsBigVersionEquals(GameVersion target)
    {
        if (target == null)
            return false;

        bool tmpIsEquals = true;
        for (int i = 0, count = 3; i < count; i++)
        {
            if (mFields[i] != target.mFields[i])
            {
                tmpIsEquals = false;
                break;
            }
        }
        return tmpIsEquals;
    }

    public int this[GAME_VERSION_FIELD field]
    {
        set
        {
            mFields[(int)field] = value;
        }
        get
        {
            return mFields[(int)field];
        }
    }

    public static bool operator ==(GameVersion lhs, GameVersion rhs)
    {
        bool tmpIsLhsNull = (lhs as object == null);
        bool tmpIsRhsNull = (rhs as object == null);
        if (tmpIsLhsNull && tmpIsRhsNull)
            return true;
        if (tmpIsLhsNull || tmpIsRhsNull)
            return false;

        bool tmpIsEquals = true;
        for (int i = 0, count = (int)GAME_VERSION_FIELD.FIELD_MAX; i < count; i++)
        {
            if (lhs.mFields[i] != rhs.mFields[i])
            {
                tmpIsEquals = false;
                break;
            }
        }
        return tmpIsEquals;
    }
    public static bool operator !=(GameVersion lhs, GameVersion rhs)
    {
        return !(lhs == rhs);
    }

    public static bool operator >(GameVersion lhs, GameVersion rhs)
    {
        if (lhs == null)
            return false;
        else if (rhs == null)
            return true;

        bool tmpIsBigger = false;
        int count = 3;//只需判断前3位
        for (int i = 0; i < count; i++)
        {
            if (lhs.mFields[i] > rhs.mFields[i])
            {
                tmpIsBigger = true;
                break;
            }
        }
        return tmpIsBigger;
    }
    public static bool operator <(GameVersion lhs, GameVersion rhs)
    {
        if (lhs == null)
            return false;
        else if (rhs == null)
            return true;

        bool tmpIsBigger = false;
        int count = 3;//只需判断前3位
        for (int i = 0; i < count; i++)
        {
            if (lhs.mFields[i] < rhs.mFields[i])
            {
                tmpIsBigger = true;
                break;
            }
        }
        return tmpIsBigger;
    }
}
