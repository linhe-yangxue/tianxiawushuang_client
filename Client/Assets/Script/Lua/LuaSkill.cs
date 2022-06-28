using DataTable;


public class LuaSkill : LuaTimer
{
    private ScriptSkill skill;

    public static LuaSkill New(ScriptSkill skill)
    {
        int scriptIndex = skill.mConfig["SCRIPT"];

        if (scriptIndex == 0)
            return null;

        DataRecord r = DataCenter.mScriptBase.GetRecord(scriptIndex);

        if (r == null)
            return null;

        LuaSkill s = new LuaSkill(r, skill); 
        s.Set("owner", skill.GetOwner().mWrapper);
        s.Set("index", (int)skill.mConfig["INDEX"]);
        s.Set("target", skill.mTarget == null ? null : skill.mTarget.mWrapper);
        s.Init();
        return s;
    }

    public LuaSkill(DataRecord record, ScriptSkill skill)
        : base(record)
    {
        this.skill = skill;
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        if (skill.GetOwner() == Character.Self)
        {
            MainProcess.mLockCharacterPos = true;
        }
    }

    protected override void _OnDestroy(object[] param)
    {
        if (skill.GetOwner() == Character.Self)
        {
            MainProcess.mLockCharacterPos = false;
        }

        skill._ApplyFinish();
    }
}