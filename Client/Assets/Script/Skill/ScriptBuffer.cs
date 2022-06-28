using UnityEngine;
using System.Collections.Generic;
using DataTable;
using LuaInterface;


public class ScriptBuffer : AffectBuffer
{
    public LuaBuff buff;

    public override bool ApplyAffect()
    {
        if(GetOwner() != null)
        {
            buff = LuaBuff.New(this);
        }

        if (buff != null && buff.state == BehaviourState.Inactive)
        {
            buff.Activate();
        }
        
        return base.ApplyAffect();
    }

    protected override void StopAffect()
    {
        base.StopAffect();

        if (buff != null && buff.state == BehaviourState.Active)
        {
            if (mTimeOut)
            {
                buff.OnTimeOut();
            }
            else
            {
                buff.OnRemove();
            }
        }
    }

    public override void OnOwnerDead()
    {
        base.OnOwnerDead();

        if (buff != null && buff.state == BehaviourState.Active)
        {
            buff.OnDead();
        }
    }

    public override void OnAttack(BaseObject target) 
    {
        base.OnAttack(target);

        if (buff != null && buff.state == BehaviourState.Active)
        {
            buff.OnAttack(target);
        }
    }

    public override void OnHit(BaseObject target, int damage, SKILL_DAMAGE_FLAGS flags) 
    {
        base.OnHit(target, damage, flags);

        if (buff != null && buff.state == BehaviourState.Active)
        {
            buff.OnHit(target, damage);
        }
    }

    public override void OnDamage(BaseObject attacker, int damage, SKILL_DAMAGE_FLAGS flags) 
    {
        base.OnDamage(attacker, damage, flags);

        if (buff != null && buff.state == BehaviourState.Active)
        {
            buff.OnDamage(attacker, damage);
        }
    }

    public override void OnSkill(BaseObject target, int skillIndex) 
    {
        base.OnSkill(target, skillIndex);

        if (buff != null && buff.state == BehaviourState.Active)
        {
            buff.OnSkill(target, skillIndex);
        }
    }

    //public override int FinalHit(BaseObject target, int damage)
    //{
    //    if (buff != null && buff.state == BehaviourState.Active)
    //    {
    //        return (int)buff.FinalHit(target, damage);
    //    }

    //    return 0;
    //}

    //public override int FinalDamage(BaseObject attacker, int damage)
    //{
    //    if (buff != null && buff.state == BehaviourState.Active)
    //    {
    //        return (int)buff.FinalDamage(attacker, damage);
    //    }

    //    return 0;
    //}
}