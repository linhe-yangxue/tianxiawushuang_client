using System.Collections.Generic;
using UnityEngine;
using DataTable;


public class SkillInfo
{  
    public float skillCD = 0f;
    public int needMP = 0;
    public float distance = 999f;
    public string title = "";
    public string describe = "";
}


public class SkillGlobal
{
    private static Dictionary<int, SkillInfo> skillInfos = new Dictionary<int, SkillInfo>();

    static SkillGlobal()
    {
        Dictionary<int, SkillInfo> loadedScriptBuffer = new Dictionary<int, SkillInfo>();

        foreach (var pair in DataCenter.mSkillConfigTable.GetAllRecord())
        {
            if (pair.Key > 0)
            {
                DataRecord r = pair.Value;
                SkillInfo info = new SkillInfo();

                if (r["TYPE"] == "ScriptSkill")
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
                                skillInfos.Add(pair.Key, info);
                                continue;
                            }

                            LuaScript script = new LuaScript(config);
                            script.Set("index", pair.Key);
                            script.Init();
                            info.skillCD = (float)script.GetNumber("skillCD");
                            info.needMP = (int)script.GetNumber("needMP");
                            info.distance = (float)script.GetNumber("distance");
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
                    info.skillCD = r["CD_TIME"];
                    info.needMP = r["NEED_MP"];
                    info.distance = r["ATTACK_DISTANCE"];
                    info.title = r["TIP_TITLE"];
                    info.describe = r["INFO"];
                }

                skillInfos.Add(pair.Key, info);
            }
        }
    }

    public static void Init() { }

    public static SkillInfo GetInfo(int index)
    {
        SkillInfo info = null;
        skillInfos.TryGetValue(index, out info);
        return info;
    }

    public static SkillInfo GetInfo(DataRecord record)
    {
        return GetInfo((int)record["INDEX"]);
    }
}