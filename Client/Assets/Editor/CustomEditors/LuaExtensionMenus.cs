using UnityEngine;
using UnityEditor;
using System.IO;


public class LuaExtensionMenus
{
    public static readonly string LuaScriptsPath = Application.dataPath + "/Resources/Lua/Scripts";

    [MenuItem("Lua/Create Lua in LuaScripts")]
    public static void CreateLuaInLuaScripts()
    {
        string file = NewLuaInPath(Application.dataPath + "/LuaScripts", "NewLuaScript");
        AssetDatabase.Refresh();
        Debug.Log(file + " Created!");
    }

    //[MenuItem("Lua/Change Lua to Txt in Resources")]
    public static void ChangeLuaToTxtInResources()
    {
        int count = Lua2TxtInPath(LuaScriptsPath);
        AssetDatabase.Refresh();
        Debug.Log("Change Complete: " + count + " files changed!");
    }
    
    //[MenuItem("Lua/Change Txt to Lua in Resources")]
    public static void ChangeTxtToLuaInResources()
    {
        int count = Txt2LuaInPath(LuaScriptsPath);
        AssetDatabase.Refresh();
        Debug.Log("Change Complete: " + count + " files changed!");
    }

    //[MenuItem("Assets/Lua to Txt", priority = 200)]
    public static void ChangeLuaToTxt()
    {
        int count = 0;
        Object[] selected = Selection.GetFiltered(typeof(Object), SelectionMode.Unfiltered);

        foreach (var s in selected)
        {
            string path = AssetDatabase.GetAssetOrScenePath(s);

            if (!string.IsNullOrEmpty(Lua2Txt(path)))
            {
                ++count;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Change Complete: " + count + " files changed!");
    }

    //[MenuItem("Assets/Txt to Lua", priority = 200)]
    public static void ChangeTxtToLua()
    {
        int count = 0;
        Object[] selected = Selection.GetFiltered(typeof(Object), SelectionMode.Unfiltered);

        foreach (var s in selected)
        {
            string path = AssetDatabase.GetAssetOrScenePath(s);

            if (!string.IsNullOrEmpty(Txt2Lua(path)))
            {
                ++count;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Change Complete: " + count + " files changed!");
    }


    private static string NewLuaInPath(string path, string defaultName)
    {
        string fileName = defaultName;
        int index = 0;

        while (File.Exists(path + "/" + fileName + ".lua"))
        {
            fileName = defaultName + (++index);
        }

        string file = path + "/" + fileName + ".lua";
        FileStream s = new FileStream(file, FileMode.Create);
        s.Close();
        return file;
    }

    private static int Lua2TxtInPath(string path)
    {
        int count = 0;
        string[] files = Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if(!string.IsNullOrEmpty(Lua2Txt(file)))
            {
                ++count;
            }
        }

        return count;
    }

    private static int Txt2LuaInPath(string path)
    {
        int count = 0;
        string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if (!string.IsNullOrEmpty(Txt2Lua(file)))
            {
                ++count;
            }
        }

        return count;
    }

    private static string Lua2Txt(string lua)
    {
        if (!lua.EndsWith(".lua", true, null))
            return "";

        string txt = lua + ".txt";
        File.Move(lua, txt);
        return txt;
    }

    private static string Txt2Lua(string txt)
    {
        if (!txt.EndsWith(".txt", true, null))
            return "";

        string lua = txt.Replace(".txt", "");

        if (!lua.EndsWith(".lua", true, null))
        {
            lua += ".lua";
        }

        File.Move(txt, lua);
        return lua;
    }
}