using System.Collections.Generic;
using UnityEngine;
using DataTable;


public class BuffInfo
{
    public float time = 0f;
    public bool hideCD = false;
    public string title = "";
    public string describe = "";
}


public class BuffGlobal
{
    private static Dictionary<int, BuffInfo> buffInfos = new Dictionary<int, BuffInfo>();

    static BuffGlobal()
    {
        Dictionary<int, BuffInfo> loadedScriptBuffer = new Dictionary<int, BuffInfo>();

        foreach (var pair in DataCenter.mAffectBuffer.GetAllRecord())
        {
            if (pair.Key > 0)
            {
                DataRecord r = pair.Value;
                BuffInfo info = new BuffInfo();

                if (r["BUFFER_TYPE"] == "ScriptBuffer")
                {
                    int id = r["SCRIPT"];

                    if (id > 0)
                    {
                        if (!loadedScriptBuffer.ContainsKey(id))
                        {
                            DataRecord config = DataCenter.mScriptBase.GetRecord(id);

                            if (config == null || !LuaInstance.CheckScriptExist(config["SCRIPT_NAME"]))
                            {
#if UNITY_EDITOR
                                if (config == null)
                                {
                                    DEBUG.LogError("ID \"" + id + "\" in \"ScriptBase.csv\" does not exist!");
                                }
                                else 
                                {
                                    DEBUG.LogError("Lua Script \"" + config["SCRIPT_NAME"] + "\" does not exist!");
                                }
#endif
                                buffInfos.Add(pair.Key, info);
                                continue;
                            }

                            LuaScript script = new LuaScript(config);
                            script.Set("index", pair.Key);
                            script.Init();
                            info.time = (float)script.GetNumber("totalTime");
                            info.hideCD = script.GetBoolean("hideCD");
                            info.title = script.GetString("title");
                            info.describe = script.GetString("describe");
                            loadedScriptBuffer.Add(id, info);
                            script.Dispose();
                        }
                        else
                        {
                            info = loadedScriptBuffer[id];
                        }
                    }
                }
                else
                {
                    info.time = r["TIME"];
                    info.hideCD = false;
                    info.title = r["TIP_TITLE"];
                    info.describe = r["INFO"];
                }

                buffInfos.Add(pair.Key, info);
            }
        }
    }

    public static void Init() { }

    public static BuffInfo GetInfo(int index)
    {
        BuffInfo info = null;
        buffInfos.TryGetValue(index, out info);
        return info;
    }

    public static BuffInfo GetInfo(DataRecord record)
    {
        return GetInfo((int)record["INDEX"]);
    }
}