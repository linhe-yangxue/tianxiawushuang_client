using System.Collections;
using LuaInterface;
using DataTable;


public sealed class LuaBuff : LuaBehaviour
{
    public static LuaBuff New(ScriptBuffer b)
    {
        if (b.mConfigIndex > 0)
        {
            int scriptIndex = b.mAffectConfig["SCRIPT"];

            if (scriptIndex == 0)
                return null;

            DataRecord r = DataCenter.mScriptBase.GetRecord(scriptIndex);

            if (r == null || !LuaInstance.CheckScriptExist(r["SCRIPT_NAME"]))
                return null;

            LuaBuff buff = new LuaBuff(r, b.GetOwner().mAffectPool, b);           
            buff.Set("owner", b.GetOwner().mWrapper);
            buff.Set("index", (int)b.mAffectConfig["INDEX"]);
            buff.Set("stageIndex", (int)DataCenter.Get("CURRENT_STAGE"));
            buff.Init();
            return buff;
        }
        else 
        {
            LuaBuff buff = new LuaBuff(b.GetOwner().mAffectPool, b);        
            buff.Set("owner", b.GetOwner().mWrapper);
            buff.Set("index", 0);
            buff.Set("stageIndex", (int)DataCenter.Get("CURRENT_STAGE"));
            buff.Init();
            return buff;
        }
    }

    private LuaFunction _onRemove;
    private LuaFunction _onDead;
    private LuaFunction _onTimeOut;
    private LuaFunction _onAttack;
    private LuaFunction _onHit;
    private LuaFunction _onDamage;
    private LuaFunction _onSkill;
    private LuaFunction _finalHit;
    private LuaFunction _finalDamage;

    private BuffAffect _affect;
    private ScriptBuffer _buff;

    protected LuaBuff(BuffAffectPool affectPool, ScriptBuffer buff)
        : base()
    {
        _affect = new BuffAffect(affectPool);
        _buff = buff;
    }

    protected LuaBuff(DataRecord record, BuffAffectPool affectPool, ScriptBuffer buff)
        : base(record)
    {
        _affect = new BuffAffect(affectPool);
        _buff = buff;
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        _onRemove = GetFunction("OnRemove");
        _onDead = GetFunction("OnDead");
        _onTimeOut = GetFunction("OnTimeOut");
        _onAttack = GetFunction("OnAttack");
        _onHit = GetFunction("OnHit");
        _onDamage = GetFunction("OnDamage");
        _onSkill = GetFunction("OnSkill");
        _finalHit = GetFunction("FinalHit");
        _finalDamage = GetFunction("FinalDamage");
    }

    public void OnDead()
    {
        Invoke(_onDead);
        OnRemove();
    }

    public void OnTimeOut()
    {
        Invoke(_onTimeOut);
        OnRemove();
    }

    public void OnRemove()
    {
        Invoke(_onRemove);
        _affect.Invalidate();
        Destroy();
    }

    public void OnAttack(BaseObject target)
    {
        OnReturnValue(Invoke(_onAttack, target == null ? null : target.mWrapper));
    }

    public void OnHit(BaseObject target, int damage)
    {
        OnReturnValue(Invoke(_onHit, target == null ? null : target.mWrapper, damage));
    }

    public void OnDamage(BaseObject attacker, int damage)
    {
        OnReturnValue(Invoke(_onDamage, attacker == null ? null : attacker.mWrapper, damage));
    }

    public void OnSkill(BaseObject target, int index)
    {
        OnReturnValue(Invoke(_onSkill, target == null ? null : target.mWrapper, index));
    }

    public double FinalHit(BaseObject target, int damage)
    {
        object[] val = Invoke(_finalHit, target == null ? null : target.mWrapper, damage);
        
        if (val != null && val.Length > 0 && val[0] is double)
        {
            return (double)val[0];
        }

        return 0d;
    }

    public double FinalDamage(BaseObject attacker, int damage)
    {
        object[] val = Invoke(_finalDamage, attacker == null ? null : attacker.mWrapper, damage);

        if (val != null && val.Length > 0 && val[0] is double)
        {
            return (double)val[0];
        }

        return 0d;
    }

    protected override void _OnAwake(object[] param)
    {
        OnReturnValue(param);
    }

    protected override void _OnStart(object[] param)
    {
        OnReturnValue(param);
    }

    protected override void _OnUpdate(object[] param)
    {
        OnReturnValue(param);
    }

    protected override void _OnDestroy(object[] param)
    {
        if (_buff != null)
        {
            if (_buff.mConfigIndex > 0)
            {
                _buff.GetOwner().StopAffect(_buff.mConfigIndex);
            }
            else
            {
                _buff.Finish();
            }
        }
    }

    private LuaTable CallFunction(LuaFunction func, params object[] args)
    {
        if (func == null)
            return null;

        object[] result = Invoke(func, args);

        if (result != null && result.Length > 0)
            return result[0] as LuaTable;

        return null;
    }

    private void OnAffect(LuaTable t)
    {
        _affect.ResetAffect();

        foreach (DictionaryEntry entry in t)
        {
            if (entry.Key is string && entry.Value is double)
            {
                string key = (string)entry.Key;
                double val = (double)entry.Value;
                _affect.SetAffect(key, val);
            }
        }
    }

    private void OnReturnValue(object[] param)
    {
        if (param != null && param.Length > 0)
        {
            LuaTable t = param[0] as LuaTable;

            if (t != null)
            {
                OnAffect(t);
            }
        }
    }
}