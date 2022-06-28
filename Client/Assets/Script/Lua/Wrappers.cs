using UnityEngine;
using LuaInterface;

public abstract class BaseWrapper
{
    public abstract Vector3 GetPosition();

    public abstract void SetPosition(Vector3 v);

    public abstract Vector3 GetForward();

    public abstract void SetForward(Vector3 v);

    public bool InSphere(BaseWrapper center, double radius)
    {
        return (this.GetPosition() - center.GetPosition()).sqrMagnitude <= radius * radius;
    }

    public bool InRange(BaseWrapper center, double radius)
    {
        Vector3 v = this.GetPosition() - center.GetPosition();
        return v.x * v.x + v.z * v.z < radius * radius;
    }

    public void Translate(Vector3 dir)
    {
        SetPosition(GetPosition() + dir);
    }

    public void RotateAround(Vector3 axis, double angle)
    {
        Vector3 forward = GetForward();
        forward = Quaternion.AngleAxis((float)angle, axis) * forward;
        SetForward(forward);
    }

    public void Rotate(double angle)
    {
        RotateAround(Vector3.up, angle);
    }
}

public class BaseObjectWrapper : BaseWrapper
{
    private BaseObject obj;

    public BaseObjectWrapper(BaseObject obj)
    {
        this.obj = obj;
    }

    public Data GetConfig(string key)
    {
        return obj.mConfigRecord[key];
    }

    public void ChangeHP(double delta)
    {
        if (!obj.IsDead())
        {
            obj.ChangeHp((int)delta);
            obj.ShowHPPaoPao((int)delta);
        }
    }

    public void ChangeMP(double delta)
    {
        Character c = obj as Character;

        if (c != null && !c.IsDead())
        {
            c.ChangeMp((int)delta);
        }
    }

    public double GetHP()
    {
        return obj.GetHp();
    }

    public double GetMP()
    {
        Character c = obj as Character;
        return c == null ? 0 : c.GetMp();
    }

    public void PlayAnim(string anim)
    {
        obj.PlayAnim(anim);
    }

    public double GetBase(string key)
    {
        switch (key)
        {
            case "ATTACK":
                return obj.GetBaseAttack();
            case "PHYSICAL_DEFENCE":
                return obj.GetBasePhysicalDefence();
            case "MAGIC_DEFENCE":
                return obj.GetBaseMagicDefence();
            case "HP_MAX":
                return obj.GetBaseMaxHp();
            case "MP_MAX":
                {
                    Character c = obj as Character;
                    return c == null ? 0 : c.GetBaseMaxMp();
                }
            case "HIT_TARGET":
                return obj.GetBaseHitRate() * 100;
            case "CRITICAL_STRIKE_RATE":
                return obj.GetBaseCriticalStrikeRate() * 100;
            //case "HIT_CRITICAL_STRIKE_RATE":
            //    return obj.GetBaseCriticalStrikeDamage() * 100;
            //case "DEFENCE_CHARM_RATE":
            //    return obj.GetBaseDefenceCharmRate() * 100;
            //case "DEFENCE_FEAR_RATE":
            //    return obj.GetBaseDefenceFearRate() * 100;
            case "DEFENCE_WOOZY_RATE":
                return obj.GetBaseDefenceWoozyRate() * 100;
            case "DEFENCE_ICE_RATE":
                return obj.GetBaseDefenceIceRate() * 100;
            //case "DEFENCE_BEATBACK_RATE":
            //    return obj.GetBaseDefenceBeatBackRate() * 100;
            //case "DEFENCE_DOWN_RATE":
            //    return obj.GetBaseDefenceDownRate() * 100;
            case "DEFENCE_HIT_RATE":
                return obj.GetBaseDamageMitigationRate() * 100;
            case "MOVE_SPEED":
                return obj.GetBaseMoveSpeed() * 100;
            case "ATTACK_SPEED":
                return obj.GetBaseAttackSpeed() * 100;
            case "DODGE_RATE":
                return obj.GetBaseDodgeRate() * 100;
            //case "AUTO_HP":
            //    return obj.GetBaseAutoHp();
            //case "AUTO_MP":
            //    return obj.GetBaseAutoMp();
            case "SILENCE":
                return obj.IsSilence() ? 1d : 0d;
        }

        return 0;
    }

    public double GetStatic(string key)
    {
        switch (key)
        {
            case "ATTACK":
                return obj.GetStaticAttack();
            case "PHYSICAL_DEFENCE":
                return obj.GetStaticPhysicalDefence();
            case "MAGIC_DEFENCE":
                return obj.GetStaticMagicDefence();
            case "HP_MAX":
                return obj.GetStaticMaxHp();
            case "MP_MAX":
                {
                    Character c = obj as Character;
                    return c == null ? 0 : c.GetStaticMaxMp();
                }
            case "HIT_TARGET":
                return obj.GetStaticHitRate() * 100;
            case "CRITICAL_STRIKE_RATE":
                return obj.GetStaticCriticalStrikeRate() * 100;
            //case "HIT_CRITICAL_STRIKE_RATE":
            //    return obj.GetStaticCriticalStrikeDamage() * 100;
            //case "DEFENCE_CHARM_RATE":
            //    return obj.GetStaticDefenceCharmRate() * 100;
            //case "DEFENCE_FEAR_RATE":
            //    return obj.GetStaticDefenceFearRate() * 100;
            case "DEFENCE_WOOZY_RATE":
                return obj.GetStaticDefenceWoozyRate() * 100;
            case "DEFENCE_ICE_RATE":
                return obj.GetStaticDefenceIceRate() * 100;
            //case "DEFENCE_BEATBACK_RATE":
            //    return obj.GetStaticDefenceBeatBackRate() * 100;
            //case "DEFENCE_DOWN_RATE":
            //    return obj.GetStaticDefenceDownRate() * 100;
            case "DEFENCE_HIT_RATE":
                return obj.GetStaticDamageMitigationRate() * 100;
            case "MOVE_SPEED":
                return obj.GetStaticMoveSpeed() * 100;
            case "ATTACK_SPEED":
                return obj.GetStaticAttackSpeed() * 100;
            case "DODGE_RATE":
                return obj.GetStaticDodgeRate() * 100;
            //case "AUTO_HP":
            //    return obj.GetStaticAutoHp();
            //case "AUTO_MP":
            //    return obj.GetStaticAutoMp();
            case "SILENCE":
                return obj.IsSilence() ? 1d : 0d;
        }

        return 0;
    }

    public double GetFinal(string key)
    {
        switch (key)
        {
            case "ATTACK":
                return obj.GetAttack();
            case "PHYSICAL_DEFENCE":
                return obj.GetPhysicalDefence();
            case "MAGIC_DEFENCE":
                return obj.GetMagicDefence();
            case "HP_MAX":
                return obj.GetMaxHp();
            case "MP_MAX":
                {
                    Character c = obj as Character;
                    return c == null ? 0 : c.GetMaxMp();
                }
            case "HIT_TARGET":
                return obj.GetHitRate() * 100;
            case "CRITICAL_STRIKE_RATE":
                return obj.GetCriticalStrikeRate() * 100;
            //case "HIT_CRITICAL_STRIKE_RATE":
            //    return obj.GetCriticalStrikeDamage() * 100;
            //case "DEFENCE_CHARM_RATE":
            //    return obj.GetDefenceCharmRate() * 100;
            //case "DEFENCE_FEAR_RATE":
            //    return obj.GetDefenceFearRate() * 100;
            case "DEFENCE_WOOZY_RATE":
                return obj.GetDefenceWoozyRate() * 100;
            case "DEFENCE_ICE_RATE":
                return obj.GetDefenceIceRate() * 100;
            //case "DEFENCE_BEATBACK_RATE":
            //    return obj.GetDefenceBeatBackRate() * 100;
            //case "DEFENCE_DOWN_RATE":
            //    return obj.GetDefenceDownRate() * 100;
            case "DEFENCE_HIT_RATE":
                return obj.GetDamageMitigationRate() * 100;
            case "MOVE_SPEED":
                return obj.GetMoveSpeed() * 100;
            case "ATTACK_SPEED":
                return obj.GetAttackSpeed() * 100;
            case "DODGE_RATE":
                return obj.GetDodgeRate() * 100;
            //case "AUTO_HP":
            //    return obj.GetAutoHp();
            //case "AUTO_MP":
            //    return obj.GetAutoMp();
            case "SILENCE":
                return obj.IsSilence() ? 1d : 0d;
        }

        return 0;
    }

    public LuaBehaviourWrapper StartAffect(BaseObjectWrapper start, int index)
    {
        ScriptBuffer b = obj.StartAffect(start.obj, index) as ScriptBuffer;
        return b == null ? null : b.buff.wrapper;
    }

    public void StopAffect(int index)
    {
        obj.StopAffect(index);
    }

    public LuaBehaviourWrapper CreateBuff()
    {
        ActiveObject o = obj as ActiveObject;

        if (o != null && !o.IsDead())
        {
            return o.CreateScriptBuffer().buff.wrapper;
        }

        return null;
    }

    public bool IsDead()
    {
        return this.obj.IsDead();
    }

    public bool IsEnemy(BaseObjectWrapper target)
    {
        return this.obj.IsEnemy(target.obj);
    }

    public bool IsPet()
    {
        return this.obj.GetObjectType() == OBJECT_TYPE.PET;
    }

    public bool IsCharactor()
    {
        return this.obj.GetObjectType() == OBJECT_TYPE.CHARATOR;
    }

    public bool IsMonster()
    {
        return this.obj.GetObjectType() == OBJECT_TYPE.MONSTER;
    }

    public bool IsMonsterBoss()
    {
        return this.obj.GetObjectType() == OBJECT_TYPE.MONSTER_BOSS;
    }

    public bool IsBigBoss()
    {
        return this.obj.GetObjectType() == OBJECT_TYPE.BIG_BOSS;
    }

    public BaseObjectWrapper GetOwner()
    {
        BaseObject o = this.obj.GetOwnerObject();
        return o == null ? null : o.mWrapper;
    }

    public BaseEffectWrapper PlayEffect(int index)
    {
        BaseEffect eff = obj.PlayEffect(index, null);
        return eff == null ? null : eff.mWrapper;
    }

    public override Vector3 GetPosition()
    {
        return this.obj.GetPosition();
    }

    public override void SetPosition(Vector3 v)
    {
        this.obj.SetPosition(v);
    }

    public override Vector3 GetForward()
    {
        if (this.obj.mMainObject != null)
        {
            return this.obj.mMainObject.transform.forward;
        }

        return Vector3.forward;
    }

    public override void SetForward(Vector3 v)
    {
        if (this.obj.mMainObject != null)
        {
            this.obj.mMainObject.transform.forward = v.normalized;
        }
    }

    public void SetTarget(BaseObjectWrapper target)
    {
        this.obj.PursueAttack(target.obj);
    }

    public BaseObjectWrapper GetTarget()
    {
        BaseObject o = this.obj.GetCurrentEnemy();
        return o == null ? null : o.mWrapper;
    }

    public void DoSkill(int index, BaseObjectWrapper target)
    {
        this.obj.SkillAttack(index, target.obj, null);
    }

    public void RemoveSkillCD()
    {
        Character ch = this.obj as Character;

        if (ch != null)
        {
            ch.RemoveAllSkillCD();
        }
    }

    public void RemoveSummonCD()
    {
        Character ch = this.obj as Character;

        if (ch != null)
        {
            ch.RemoveAllSummonCD();
        }
    }

    public LuaBehaviourWrapper GetScriptBuff(int index)
    {
        ScriptBuffer b = this.obj.GetBufferByIndex(index) as ScriptBuffer;
        return b == null ? null : b.buff.wrapper;
    }

    public LuaTable GetAllScriptBuffs()
    {
        ActiveObject active = obj as ActiveObject;

        if (active == null)
        {
            return LuaInstance.NewTable();
        }
        else 
        {
            LuaTable t = LuaInstance.NewTable();
            int i = 0;

            foreach (var b in active.mAffectList)
            {
                if (b != null && !b.GetFinished())
                {
                    ScriptBuffer s = b as ScriptBuffer;

                    if (s != null)
                    {
                        t[++i] = s.buff.wrapper;
                    }
                }
            }

            return t;
        }
    }

    public LuaTable GetAllBuffIndices()
    {
        ActiveObject active = obj as ActiveObject;

        if (active == null)
        {
            return LuaInstance.NewTable();
        }
        else
        {
            LuaTable t = LuaInstance.NewTable();
            int i = 0;

            foreach (var b in active.mAffectList)
            {
                if (b != null && !b.GetFinished())
                {
                    t[++i] = b.mConfigIndex;
                }
            }

            return t;
        }
    }
}


public class BaseEffectWrapper : BaseWrapper
{
    private BaseEffect eff;

    public BaseEffectWrapper(BaseEffect eff)
    {
        this.eff = eff;
    }

    public void Finish()
    {
        eff.Finish();
    }

    public override Vector3 GetPosition()
    {
        return eff.GetPosition();
    }

    public override void SetPosition(Vector3 v)
    {
        if (eff.mGraphObject != null)
        {
            eff.mGraphObject.transform.position = v;
        }
    }

    public override Vector3 GetForward()
    {
        if (this.eff.mGraphObject != null)
        {
            return this.eff.mGraphObject.transform.forward;
        }

        return Vector3.forward;
    }

    public override void SetForward(Vector3 v)
    {
        if (this.eff.mGraphObject != null)
        {
            this.eff.mGraphObject.transform.forward = v.normalized;
        }
    }
}


public class CommonWrapper
{
    public static string FormatString(string format, params object[] args)
    {
        return string.Format(format, args);
    }
}