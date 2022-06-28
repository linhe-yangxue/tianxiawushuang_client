using UnityEngine;
using System.Collections.Generic;


public class ConfigParam
{
    private const char MAIN_SPERATOR = '|';
    private const char EQ_SPERATOR = '=';
    private const char SUB_SPERATOR = '#';

    public static int[] ParseIntArray(string text, char sperator)
    {
        string[] strArray = text.Split(sperator);
        int[] array = new int[strArray.Length];

        for (int i = 0; i < strArray.Length; ++i)
        {
            int.TryParse(strArray[i], out array[i]);
        }

        return array;
    }

    public static float[] ParseFloatArray(string text, char sperator)
    {
        string[] strArray = text.Split(sperator);
        float[] array = new float[strArray.Length];

        for (int i = 0; i < strArray.Length; ++i)
        {
            float.TryParse(strArray[i], out array[i]);
        }

        return array;
    }

    public static Vector2 ParseVector2(string text, char sperator)
    {
        Vector2 value = new Vector2();
        float[] array = ParseFloatArray(text, sperator);
        value.x = array.Length > 0 ? array[0] : 0f;
        value.y = array.Length > 1 ? array[1] : 0f;
        return value;
    }

    public static Vector3 ParseVector3(string text, char sperator)
    {
        Vector3 value = new Vector3();
        float[] array = ParseFloatArray(text, sperator);
        value.x = array.Length > 0 ? array[0] : 0f;
        value.y = array.Length > 1 ? array[1] : 0f;
        value.z = array.Length > 2 ? array[2] : 0f;
        return value;
    }

    public static Vector4 ParseVector4(string text, char sperator)
    {
        Vector4 value = new Vector4();
        float[] array = ParseFloatArray(text, sperator);
        value.x = array.Length > 0 ? array[0] : 0f;
        value.y = array.Length > 1 ? array[1] : 0f;
        value.z = array.Length > 2 ? array[2] : 0f;
        value.w = array.Length > 3 ? array[3] : 0f;
        return value;
    }

    public string rawText { get; private set; }

    private Dictionary<string, string> textDict = new Dictionary<string, string>();

    public ConfigParam()
        : this("")
    { }

    public ConfigParam(string str)
    {
        this.rawText = str;
        string[] split = str.Split(MAIN_SPERATOR);

        foreach (var s in split)
        {
            int eq = s.IndexOf(EQ_SPERATOR);

            if (eq > 0 && eq < s.Length - 1)
            {
                string left = s.Substring(0, eq).Trim().ToLower();
                string right = s.Substring(eq + 1, s.Length - eq - 1).Trim();
                textDict.Add(left, right);
            }
        }
    }

    public string GetString(string key, string defaultValue)
    {
        string str;

        if (textDict.TryGetValue(key.ToLower(), out str))
        {
            return str;
        }

        return defaultValue;
    }

    public string GetString(string key)
    {
        return GetString(key, "");
    }

    public int GetInt(string key, int defaultValue)
    {
        string str;

        if (textDict.TryGetValue(key.ToLower(), out str))
        {
            int value;
            int.TryParse(str, out value);
            return value;
        }

        return defaultValue;
    }

    public int GetInt(string key)
    {
        return GetInt(key, 0);
    }

    public float GetFloat(string key, float defaultValue)
    {
        string str;

        if (textDict.TryGetValue(key.ToLower(), out str))
        {
            float value;
            float.TryParse(str, out value);
            return value;
        }

        return defaultValue;
    }

    public float GetFloat(string key)
    {
        return GetFloat(key, 0f);
    }

    public Vector2 GetVector2(string key, Vector2 defaultValue)
    {
        string str;

        if (textDict.TryGetValue(key.ToLower(), out str))
        {
            return ParseVector2(str, SUB_SPERATOR);
        }

        return defaultValue;
    }

    public Vector2 GetVector2(string key)
    {
        return GetVector2(key, Vector2.zero);
    }

    public Vector3 GetVector3(string key, Vector3 defaultValue)
    {
        string str;

        if (textDict.TryGetValue(key.ToLower(), out str))
        {
            return ParseVector3(str, SUB_SPERATOR);
        }

        return defaultValue;
    }

    public Vector3 GetVector3(string key)
    {
        return GetVector3(key, Vector3.zero);
    }

    public Vector4 GetVector4(string key, Vector4 defaultValue)
    {
        string str;

        if (textDict.TryGetValue(key.ToLower(), out str))
        {
            return ParseVector4(str, SUB_SPERATOR);
        }

        return defaultValue;
    }

    public Vector4 GetVector4(string key)
    {
        return GetVector4(key, Vector4.zero);
    }

    public string[] GetStringArray(string key)
    {
        string str;

        if (textDict.TryGetValue(key.ToLower(), out str))
        {
            return str.Split(SUB_SPERATOR);
        }

        return new string[0];
    }

    public int[] GetIntArray(string key)
    {
        string str;

        if (textDict.TryGetValue(key.ToLower(), out str))
        {
            return ParseIntArray(str, SUB_SPERATOR);
        }

        return new int[0];
    }

    public float[] GetFloatArray(string key)
    {
        string str;

        if (textDict.TryGetValue(key.ToLower(), out str))
        {
            return ParseFloatArray(str, SUB_SPERATOR);
        }

        return new float[0];
    }
}