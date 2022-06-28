using UnityEngine;
using System;
using System.Runtime.InteropServices;


public static class JCode
{
    //by chenliang
    //begin

    //更换JsonFx库为LitJson
    //增加LitJson自定义Json数据转换
    private static bool mIsInitLitJson = false;
    public static void InitLitJson()
    {
        if (mIsInitLitJson)
            return;
        mIsInitLitJson = true;

        __InitValueToString();
        __InitStringToValue();
        __InitValueToValue();
    }
    private static void __InitValueToString()
    {
        LitJson.JsonMapper.RegisterImporter<Int32, String>((Int32 jsonValue) =>
        {
            return jsonValue.ToString();
        });
        LitJson.JsonMapper.RegisterImporter<Double, String>((Double jsonValue) =>
        {
            return jsonValue.ToString();
        });
        LitJson.JsonMapper.RegisterImporter<Int64, String>((Int64 jsonValue) =>
        {
            return jsonValue.ToString();
        });
        LitJson.JsonMapper.RegisterImporter<Boolean, String>((Boolean jsonValue) =>
        {
            return jsonValue.ToString();
        });
    }
    private static void __InitStringToValue()
    {
        LitJson.JsonMapper.RegisterImporter<String, Int32>((String jsonValue) =>
        {
            Int32 tmpValue = 0;
            if (!Int32.TryParse(jsonValue, out tmpValue))
                return 0;
            return tmpValue;
        });
        LitJson.JsonMapper.RegisterImporter<String, float>((String jsonValue) =>
        {
            float tmpValue = 0.0f;
            if (!float.TryParse(jsonValue, out tmpValue))
                return 0.0f;
            return tmpValue;
        });
        LitJson.JsonMapper.RegisterImporter<String, Double>((String jsonValue) =>
        {
            Double tmpValue = 0.0;
            if (!Double.TryParse(jsonValue, out tmpValue))
                return 0.0;
            return tmpValue;
        });
        LitJson.JsonMapper.RegisterImporter<String, long>((String jsonValue) =>
        {
            long tmpValue = 0;
            if (!long.TryParse(jsonValue, out tmpValue))
                return 0;
            return tmpValue;
        });
        LitJson.JsonMapper.RegisterImporter<String, Boolean>((String jsonValue) =>
        {
            if (jsonValue == "true")
                return true;
            return false;
        });
    }
    private static void __InitValueToValue()
    {
        LitJson.JsonMapper.RegisterImporter<Int32, Int64>((Int32 jsonValue) =>
        {
            Int64 tmpValue = (Int64)jsonValue;
            return tmpValue;
        });
        LitJson.JsonMapper.RegisterImporter<Double, Single>((Double jsonValue) =>
        {
            Single tmpValue = (Single)jsonValue;
            return tmpValue;
        });
        LitJson.JsonMapper.RegisterImporter<Int32, Boolean>((Int32 jsonValue) =>
        {
            Boolean tmpValue = (jsonValue != 0 ? true : false);
            return tmpValue;
        });
        LitJson.JsonMapper.RegisterImporter<Int64, Boolean>((Int64 jsonValue) =>
        {
            Boolean tmpValue = (jsonValue != 0 ? true : false);
            return tmpValue;
        });
        LitJson.JsonMapper.RegisterImporter<Double, Int64>((Double jsonValue) =>
        {
            Int64 tmpValue = (Int64)jsonValue;
            return tmpValue;
        });

        LitJson.JsonMapper.RegisterExporter<Single>((Single obj, LitJson.JsonWriter writer) =>
        {
            if (writer == null)
                return;

            writer.Write(Convert.ToDouble(obj));
        });
    }

    //end
    //--------------------------------------------------------------------------------
    public static string Encode(object value)
    {
        //by chenliang
        //beign

//        return JsonWriter.Serialize(value);
//---------------
        return LitJson.JsonMapper.ToJson(value);

        //end
    }
    //--------------------------------------------------------------------------------
    //public static T Decode<T>(string context) where T : ICode
    public static T Decode<T>(string context)
    {
        //by chenliang
        //begin

//        return JsonReader.Deserialize<T>(context);
//---------------------
        return LitJson.JsonMapper.ToObject<T>(context);

        //end
    }
    //--------------------------------------------------------------------------------
    public static byte[] GetBytes(string data)
    {
        //return System.Text.Encoding.ASCII.GetBytes(data);
        return System.Text.Encoding.UTF8.GetBytes(data);
    }
    //--------------------------------------------------------------------------------
    public static string GetString(byte[] data)
    {
        //return System.Text.Encoding.ASCII.GetString(data);
        return System.Text.Encoding.UTF8.GetString(data);
    }
}