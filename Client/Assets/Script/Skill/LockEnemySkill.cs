using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using Utilities.Routines;

public class tDamageSkill : Skill
{
    public int mDamageCount = 1;

    public TotalTime mTimeTool = new TotalTime();

    public override bool _DoEvent()
    {
        mDamageCount = GetConfig("DAMAGE_COUNT");
        if (mDamageCount < 1)
            mDamageCount = 1;

		mNowStage = EStateStage.START;
		_DoSkill();
		return true;
    }

    public override void _OnOverTime()
    {
        _DoSkill();
    }

    public virtual bool _DoSkill()
    {
        if (GetFinished())
            return false;

        switch (mNowStage)
        {
            case EStateStage.START:
                mNowStage = EStateStage.ATTACK;
                if (!_StartStage())
                {
                    //_DoSkill();
                }
                ExtraOnStart();
                break;

            case EStateStage.ATTACK:
                {
                    mNowStage = EStateStage.DAMAGE;
                    if (!_AttackStage())
                    {
                        //_DoSkill();
                    }
                    ExtraOnAttack();
                    float t = GetConfig("DO_HIT_TIME");
                    if (t < 0.0001f)
                        _OnOverTime();
                    else
                        WaitTime(t);
                }
                break;

            case EStateStage.DAMAGE:
                {
                    mNowStage = EStateStage.FINISH;

                    //if (!_DamageStage())
                    //{
                    //    //Finish();
                    //}
                    float t = GetConfig("DAMAGE_TIME");
                    if (t < 0.0001f)
                        _DamageStage();
                    else
                    {
                        mTimeTool.Start(t);
                        StartUpdate();
                    }

                    t = GetConfig("ATTACK_TIME");
                    t -= (float)GetConfig("DO_HIT_TIME");
                    if (t < 0.0001f)
                        _OnOverTime();
                    else
                        WaitTime(t);

                    break;
                }
            default:
                _ApplyFinish();
                break;
        }
        return true;
    }

    public override bool Update(float secondTime)
    {
        if (mTimeTool.Update())
        {
            if (mDamageCount-- <= 0)
            {
                Finish();
                return false;
            }
            else
            {
                _DamageStage();
                float t = GetConfig("DAMAGE_TIME");
                {
                    mTimeTool.Start(t);
                }
            }
        }
        return true;
    }
}


public class AOESkill : tDamageSkill
{
    Vector3 mTargetPos;
    BaseEffect mBoundEffect;

    public override bool _DoEvent()
    {
        mTargetPos = mTarget.GetPosition();
        return base._DoEvent();
    }

    public override void _PlayStartEffect()
    {
        int effectIndex = GetConfig("START_EFFECT");
        if (effectIndex > 0)
        {
            mBoundEffect = BaseEffect.CreateEffect(mOwner, mTarget, effectIndex);
            if (mBoundEffect != null)
                mBoundEffect.SetPosition(mTargetPos);
        }
    }

    public override void _PlayAttackEffect()
    {
        if (mBoundEffect != null)
            mBoundEffect.Finish();

        int effectIndex = GetConfig("ATTACK_EFFECT");
        if (effectIndex > 0)
        {
            mBoundEffect = BaseEffect.CreateEffect(mOwner, mTarget, effectIndex);
            if (mBoundEffect != null)
            {
                mBoundEffect.SetPosition(mTargetPos);
                //mBoundEffect.mGraphObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            }
        }
    }

    public override bool _DamageStage()
    {
        string motionName = GetConfig("HIT_MOTION");
        float range = GetConfig("DAMAGE_RANGE");
        int effectIndex = GetConfig("DAMAGE_EFFECT");

        ObjectManager.Self.ForEachAlived(x => (mOwner.IsSameCamp(x) ^ !isFriendSkill) && AIKit.InBounds(x, mTargetPos, range), x => _Affect(x));

        // target pos circle bound play
        if (effectIndex > 0)
        {
            BaseEffect effect = BaseEffect.CreateEffect(mOwner, mTarget, effectIndex);
            if (effect != null)
            {
                Vector3 pos = MathTool.RandPosInCircle(mTargetPos, range);
                effect.SetPosition(pos);
            }
            //target.PlayEffect(effectIndex, mOwner);
        }

        return true;

    }

    public override bool _OnFinish()
    {
        if (mBoundEffect != null)
            mBoundEffect.Finish();

        return base._OnFinish();
    }


    protected override void ExtraOnStart()
    {
        base.ExtraOnStart();

        int extraCount = mExtraParam.GetInt("extra_count", 0);

        if (extraCount > 0)
        {
            float extraRate = mExtraParam.GetFloat("extra_rate", 0f);

            for (int i = 0; i < extraCount; ++i)
            {
                mOwner.StartRoutine(DoExtraDamage((i + 1) * extraRate));
            }
        }
    }

    private IEnumerator DoExtraDamage(float delay)
    {
        yield return new Delay(delay);

        Vector3 pos = mTarget.GetPosition();
        float startTime = GetConfig("START_TIME");
        BaseEffect eff = OnExtraStart(pos);
        yield return new Delay(startTime);

        if (eff != null)
            eff.Finish();

        eff = OnExtraAttack(pos);

        float doHitTime = GetConfig("DO_HIT_TIME");
        yield return new Delay(doHitTime);

        float damageTime = GetConfig("DAMAGE_TIME");
        float damageCount = Mathf.Max(1, (int)GetConfig("DAMAGE_COUNT"));

        while (damageCount > 0)
        {
            yield return new Delay(damageTime);
            OnExtraDamage(pos);
            --damageCount;
        }

        float finishTime = GetConfig("ATTACK_TIME") - doHitTime - damageCount * damageTime;
        yield return new Delay(finishTime);

        if (eff != null)
            eff.Finish();
    }

    private BaseEffect OnExtraStart(Vector3 pos)
    {
        int effectIndex = GetConfig("START_EFFECT");

        if (effectIndex > 0)
        {
            var eff = BaseEffect.CreateEffect(mOwner, mTarget, effectIndex);

            if (eff != null)
                eff.SetPosition(pos);

            return eff;
        }

        return null;
    }

    private BaseEffect OnExtraAttack(Vector3 pos)
    {
        int effectIndex = GetConfig("ATTACK_EFFECT");

        if (effectIndex > 0)
        {
            var eff = BaseEffect.CreateEffect(mOwner, mTarget, effectIndex);

            if (eff != null)
                eff.SetPosition(pos);

            return eff;
        }

        return null;
    }

    private BaseEffect OnExtraDamage(Vector3 pos)
    {
        string motionName = GetConfig("HIT_MOTION");
        float range = GetConfig("DAMAGE_RANGE");
        int effectIndex = GetConfig("DAMAGE_EFFECT");

        ObjectManager.Self.ForEachAlived(x => (mOwner.IsSameCamp(x) ^ !isFriendSkill) && AIKit.InBounds(x, pos, range), x => _Affect(x));

        if (effectIndex > 0)
        {
            BaseEffect effect = BaseEffect.CreateEffect(mOwner, mTarget, effectIndex);

            if (effect != null)
            {
                effect.SetPosition(MathTool.RandPosInCircle(pos, range));
            }

            return effect;
        }

        return null;
    }
}
