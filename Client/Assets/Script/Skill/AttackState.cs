using UnityEngine;
using System.Collections;
using DataTable;
using Logic;



public class AttackState 
{

    //enum TARGET_TYPE
    //{
    //    TARGET,
    //    SELF,
    //    TWO,
    //    FRIEND,
    //    ENEMY,
    //}

    public const float AUREOLA_RATE = 2f; // Buff check rate (seconds)

    private int mAureolaLifeSeconds = 0;   // Aureola life time (seconds)
    private DataRecord mStateConfig;
    
    public AttackState(int stateConfig)
    {
        mStateConfig = DataCenter.mAttackState.GetRecord(stateConfig);

        if (mStateConfig == null)
            EventCenter.Log(LOG_LEVEL.ERROR, "At AttackStage no exist state index record > " + stateConfig.ToString());        
    }

    public Data GetConfig(string strConfigKey)
    {
        if (mStateConfig != null)
        {
            Data result = mStateConfig.getData(strConfigKey);

            if (result.Empty())
                EventCenter.Log(LOG_LEVEL.ERROR, "At AttackStage table no exist config >" + strConfigKey);

            return result;
        }
        EventCenter.Log(LOG_LEVEL.ERROR, "At AttackStage no exist state index record");
        return Data.NULL;
    }

    public AffectBuffer ApplyBuffer(BaseObject owner, BaseObject targetObject, AFFECT_TIME affectTime)
    {
        if (mStateConfig != null)
        {
            int affectIndex = 0;
            string targetType = "";
            float targetRange = 0;
            int rate = 0;

            if (affectTime == AFFECT_TIME.ON_HIT)
            {
                affectIndex = GetConfig("HIT_AFFECT");
                targetType = GetConfig("HIT_AFFECT_OBJECT");
                targetRange = GetConfig("HIT_TARGET_RANGE");
                rate = GetConfig("HIT_AFFECT_RATE");

                if(affectIndex > 0)
                    return _generateBuffer(owner, targetObject, affectIndex, targetType, targetRange, rate);
            }
            else if (affectTime == AFFECT_TIME.ON_ATTACK)
            {
                affectIndex = GetConfig("ATTACK_AFFECT");
                targetType = GetConfig("ATTACK_AFFECT_OBJECT");
                targetRange = GetConfig("ATTACK_TARGET_RANGE");
                rate = GetConfig("ATTACK_AFFECT_RATE");

                if(affectIndex > 0)
                    return _generateBuffer(owner, targetObject, affectIndex, targetType, targetRange, rate);
            }
            else if (affectTime == AFFECT_TIME.AUREOLA)
            {
                affectIndex = GetConfig("AUREOLA_AFFECT");
                targetType = GetConfig("AUREOLA_AFFECT_OBJECT");
                targetRange = GetConfig("AUREOLA_AFFECT_RANGE");
                rate = GetConfig("AUREOLA_AFFECT_RATE");
                mAureolaLifeSeconds = GetConfig("AUREOLA_AFFECT_TIME");

                if(affectIndex > 0)
                    return _generateBuffer(owner, targetObject, affectIndex, targetType, targetRange, rate);
            }
        }

        return null;
    }

    public static AffectBuffer _generateBuffer(BaseObject owner, BaseObject targetObject, int bufferIndex, string targetType, float targetRange, int rate)
    {
		if (owner.IsDead())
			return null;

        switch (targetType)
        {
            case "TARGET":
                if (Trigger(rate))
                {
                    if (targetObject != null)
                        return targetObject.StartAffect(owner, bufferIndex);
                }
                break;

            case "SELF":
                if (Trigger(rate))
                {
                    if (owner != null)
                        return owner.StartAffect(owner, bufferIndex);
                }
                break;

            case "TWO":
                if (Trigger(rate))
                {
                    if (owner != null)
                        return owner.StartAffect(owner, bufferIndex);

                    if (targetObject != null)
                        return targetObject.StartAffect(owner, bufferIndex);
                }
                break;

            case "FRIEND":
			{
                AffectBuffer buffer = null;
                foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
                {
                    if (obj != null
                        && obj != owner
                        && obj.GetCamp() == owner.GetCamp()
                        && InRange(owner, obj, targetRange)
                        && Trigger(rate)
                        )
                    {
                        buffer = obj.StartAffect(owner, bufferIndex);
                    }
                }
                return buffer;
			}

            case "SELF_ALL":
                {
                    AffectBuffer buffer = null;
                    foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
                    {
                        if (obj != null
                            //&& obj != owner
                            && obj.GetCamp() == owner.GetCamp()
                            && InRange(owner, obj, targetRange)
                            && Trigger(rate)
                            )
                        {
                            buffer = obj.StartAffect(owner, bufferIndex);
                        }
                    }
                    return buffer;
                }

            case "ENEMY":
			{
                AffectBuffer buffer = null;
                foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
                {
                    if (obj != null
                        && obj != owner
                        && obj.GetCamp() != owner.GetCamp()
                        && InRange(owner, obj, targetRange)
                        && Trigger(rate)
                        )
                    {
                        buffer = obj.StartAffect(owner, bufferIndex);
                    }
                }
				return buffer;
			}             
        }

		return null;
    }

    public bool AureolaOverTime()
    {
        if (MainProcess.mStage == null)
            return false;
        else
            return mAureolaLifeSeconds > 0 && Time.time - MainProcess.mStage.mStartTime > mAureolaLifeSeconds;
    }

    private static bool Trigger(int rate)
    {
        return Random.Range(0, 101) <= rate;
    }

    private static bool InRange(BaseObject owner, BaseObject target, float radius)
    {
        return (owner.GetPosition() - target.GetPosition()).sqrMagnitude < radius * radius;
    }
}


// 施加时机
public enum AFFECT_TIME
{
    ON_ATTACK,      // 攻击时
    ON_HIT,         // 被击时
    AUREOLA,        // 光环
}
