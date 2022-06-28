using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
using Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class LuaInstance : IDisposable
{
    public static LuaState Lua { get; private set; }

    private static Dictionary<string, string> Globals;
    private static Dictionary<string, string> Scripts;
    private static LuaFunction NewInstanceFunc;
    
    static LuaInstance()
    {
#if UNITY_EDITOR
        if (Directory.Exists(Application.dataPath + "/Resources/Lua/Scripts"))
        {
            Directory.Delete(Application.dataPath + "/Resources/Lua/Scripts", true);
            File.Delete(Application.dataPath + "/Resources/Lua/Scripts.meta");
        }

        Build(Application.dataPath + "/LuaGlobals/", 
            Application.dataPath + "/Resources/Lua/luaglobals.bytes", 
            new byte[] { 0x97, 0x05, 0x22, 0x0F, 0x8A, 0x74, 0xF4, 0x32 });

        Build(Application.dataPath + "/LuaScripts/", 
            Application.dataPath + "/Resources/Lua/luascripts.bytes",
            new byte[] { 0x61, 0x60, 0x35, 0xBC, 0x26, 0x49, 0xC1, 0xDA });
#endif
        Globals = LuaScriptCrypto.Decrypt("Lua/luaglobals", new byte[] { 0x97, 0x05, 0x22, 0x0F, 0x8A, 0x74, 0xF4, 0x32 });
        Scripts = LuaScriptCrypto.Decrypt("Lua/luascripts", new byte[] { 0x61, 0x60, 0x35, 0xBC, 0x26, 0x49, 0xC1, 0xDA });
        LuaScriptMgr mgr = new LuaScriptMgr();
        Lua = mgr.lua;
        DoFile(Globals, "Math");
        DoFile(Globals, "Vector3");
        DoFile(Globals, "Quaternion");
        DoFile(Globals, "Predef");
        mgr.Start();     
        NewInstanceFunc = Lua["NewInstance"] as LuaFunction;
    }

    public static void Preload() { }

    public static object[] DoFile(Dictionary<string, string> pool, string script)
    {
        string text = null;
        script = Path.GetFileNameWithoutExtension(script);

        if (!pool.TryGetValue(script, out text))
        {
            DEBUG.LogError("Lua Script \"" + script + "\" does not exist!");
            return null;
        }

        object[] returnValue = null;

        try
        {
            returnValue = Lua.DoString(text);
        }
        catch (Exception e)
        {
            DEBUG.LogError(e.ToString() + "\n---------- In Script \"" + script + "\" ----------");
        }

        return returnValue;
    }

    public static object[] DoFile(string script)
    {
        return DoFile(Scripts, script);
    }

    public static bool CheckScriptExist(string fileName)
    {
        return Scripts.ContainsKey(fileName);
    }

    public static LuaTable NewTable()
    {
        return Lua.NewTable();
    }
    
    public static LuaTable NewTable(IEnumerable vals)
    {
        LuaTable t = Lua.NewTable();
        int i = 0;
    
        foreach (var v in vals)
        {
            t[++i] = v;
        }
    
        return t;
    }

#if UNITY_EDITOR
    private static void Build(string directory, string destFile, byte[] key)
    {
        if (File.Exists(destFile))
        {
            File.Delete(destFile);
        }

        LuaScriptCrypto.Build(directory, destFile, key);
        AssetDatabase.Refresh();
    }
#endif

    private bool _isDisposed = false;

    public string name { get; private set; }
    public LuaTable table { get; private set; }

    public LuaInstance()
    {
        this.name = null;
        this.table = NewInstanceFunc.Call()[0] as LuaTable;
    }

    public LuaInstance(LuaFunction func)
    {
        this.name = null;
        this.table = func.Call()[0] as LuaTable;
    }

    public LuaInstance(string script)
    {
        this.name = script;
        this.table = DoFile(script)[0] as LuaTable;
    }

    public virtual void Init() 
    { }

    public object[] Invoke(LuaFunction f, params object[] args)
    {
        if (f == null)
            return null;

        object[] result = null;

        try
        {
            result = f.Call(args);
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(name))
            {
                DEBUG.LogError(e.ToString());
            }               
            else
            {
                DEBUG.LogError(e.ToString() + "\n---------- In Script \"" + name + "\" ----------");
            }
#endif
        }

        return result;
    }

    public object[] Invoke(string fName, params object[] args)
    {
        return Invoke(GetFunction(fName));
    }

    public T Get<T>(string key)
    {
        return table.Get<T>(key);
    }

    public bool TryGet<T>(string key, out T value)
    {
        return table.TryGet<T>(key, out value);
    }

    public double GetNumber(string key)
    {
        return table.GetNumber(key);
    }

    public bool GetBoolean(string key)
    {
        return table.GetBoolean(key);
    }

    public string GetString(string key)
    {
        return table.GetString(key);
    }

    public LuaFunction GetFunction(string key)
    {
        return table.GetFunction(key);
    }

    public LuaTable GetTable(string key)
    {
        return table.GetTable(key);
    }

    public object GetObject(string key)
    {
        return table.GetObject(key);
    }

    public void Set(string key, object value)
    {
        table.Set(key, value);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            table.Dispose();
            table = null;
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}


public static partial class LuaExtensions
{
    public static T Get<T>(this LuaTable t, string key)
    {
        object obj = t.rawget(key);

        if (obj is T)
            return (T)obj;
        else
            return default(T);
    }

    public static bool TryGet<T>(this LuaTable t, string key, out T value)
    {
        object obj = t.rawget(key);

        if (obj is T)
        {
            value = (T)obj;
            return true;
        }
        else
        {
            value = default(T);
            return false;
        }
    }

    public static object GetObject(this LuaTable t, string key)
    {
        return t.rawget(key);
    }

    public static double GetNumber(this LuaTable t, string key)
    {
        return t.Get<double>(key);
    }

    public static bool GetBoolean(this LuaTable t, string key)
    {
        return t.Get<bool>(key);
    }

    public static string GetString(this LuaTable t, string key)
    {
        return t.Get<string>(key);
    }

    public static LuaFunction GetFunction(this LuaTable t, string key)
    {
        return t.Get<LuaFunction>(key);
    }

    public static LuaTable GetTable(this LuaTable t, string key)
    {
        return t.Get<LuaTable>(key);
    }

    public static Vector3 ToVector3(this LuaTable t)
    {
        float x = (float)t.GetNumber("x");
        float y = (float)t.GetNumber("y");
        float z = (float)t.GetNumber("z");
        return new Vector3(x, y, z);
    }
}


public class LuaScriptCrypto
{
    private static DESCrypto crypto = new DESCrypto();

    public static void Build(string directory, string destFile, byte[] key)
    {
        if (File.Exists(destFile))
        {
            File.Delete(destFile);
        }

        Dictionary<string, string> scripts = new Dictionary<string, string>();

        FileAssistant.ForEachFile(directory, "*.lua",
            path => scripts.Add(Path.GetFileNameWithoutExtension(path), File.ReadAllText(path)));

        Encrypt(scripts, destFile, key);
    }

    public static Dictionary<string, string> Decrypt(string resPath, byte[] key)
    {
        TextAsset asset = GameCommon.mResources.LoadTextAsset(resPath);

        if (asset == null)
        {
            DEBUG.LogError("Lua scripts asset file \"" + resPath + "\" in resources does not exist!");
            return new Dictionary<string, string>();
        }

        byte[] decripted = null;

        if (crypto.TryDecrypt(asset.bytes, key, out decripted))
        {
            Dictionary<string, string> dict = null;

            if (Serializer.TryDeserialize<Dictionary<string, string>>(decripted, out dict))
            {
                return dict;
            }
        }

        return new Dictionary<string, string>();
    }

    private static void Encrypt(Dictionary<string, string> scripts, string destFile, byte[] key)
    {
        byte[] buffer = null;

        if (Serializer.TrySerialize<Dictionary<string, string>>(scripts, out buffer))
        {
            byte[] encripted = null;

            if (crypto.TryEncrypt(buffer, key, out encripted))
            {
                File.WriteAllBytes(destFile, encripted);
            }
        }
    }
}