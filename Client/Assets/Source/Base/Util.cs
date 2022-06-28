using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Reflection;

public class Util {
    /// <summary>
    /// 取得Lua路径
    /// </summary>
    public static string LuaPath(string name) {
        string path = Application.dataPath + "/";
        string lowerName = name.ToLower();
        if (lowerName.EndsWith(".lua")) {
            return path + "lua/" + name;
        }
        return path + "lua/" + name + ".lua";
    }

    public static void Log(string str) {
        Debug.Log(str);
    }

    public static void LogWarning(string str) {
        Debug.LogWarning(str);
    }

    public static void LogError(string str) {
        Debug.LogError(str); 
    }

    public static Component AddComponent(GameObject go, string assembly, string classname) {
        Assembly asmb = Assembly.Load(assembly);
        Type t = asmb.GetType(assembly + "." + classname);
        return go.AddComponent(t); 
    }
}