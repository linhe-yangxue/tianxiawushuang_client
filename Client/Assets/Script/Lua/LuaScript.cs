using LuaInterface;
using DataTable;


public class LuaScript : LuaInstance
{
    static LuaScript()
    {
        LuaTable Configs = Lua["Configs"] as LuaTable;
        Configs.Set("ActiveObject", DataCenter.mActiveConfigTable);
        Configs.Set("AffectBuffer", DataCenter.mAffectBuffer);
        Configs.Set("BossConfig", DataCenter.mBossConfig);
        Configs.Set("MonsterObject", DataCenter.mMonsterObject);
        Configs.Set("ScriptBase", DataCenter.mScriptBase);
        Configs.Set("Skill", DataCenter.mSkillConfigTable);
        Configs.Set("StageConfig", DataCenter.mStageTable);
    }

    public LuaScript()
        : base()
    { }

    public LuaScript(LuaFunction func)
        : base(func)
    { }

    public LuaScript(string script)
        : base(script)
    { }

    public LuaScript(DataRecord record)
        : base((string)record["SCRIPT_NAME"])
    {
        Set("var1", (int)record["VAR1"]);
        Set("var2", (int)record["VAR2"]);
        Set("var3", (int)record["VAR3"]);
        Set("var4", (int)record["VAR4"]);
        Set("var5", (int)record["VAR5"]);
        Set("var6", (int)record["VAR6"]);
        Set("var7", (int)record["VAR7"]);
        Set("var8", (int)record["VAR8"]);
        Set("var9", (int)record["VAR9"]);
        Set("var10", (int)record["VAR10"]);
    }

    public override void Init()
    {
        Invoke("Init");
    }
}