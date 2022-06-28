using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;
using Utilities.Routines;
using System.Linq;

// 待内侧版本之后修改的BUG
// 将抗暴率和伤害反弹率的枚举名称尾部加上"_RATE"，以统一计算公式，策划配表需要做相应修改
public enum AFFECT_TYPE
{
    NONE,                       //
    ATTACK = 1,                 // 攻击
	ATTACK_RATE = 2,            // 攻击%
    DEFENCE = 3,                // 防御
    DEFENCE_RATE = 4,           // 防御%
    HP = 5,                     // 生命
    HP_RATE = 6,                // 生命%
	HP_MAX = 7,                 // 生命上限
	HP_MAX_RATE = 8,            // 生命上限%
    MP = 9,                     // 魔法
    MP_RATE = 10,               // 魔法%
    MP_MAX = 11,                // 魔符上限
    MP_MAX_RATE = 12,           // 魔法上限%
    //HIT_TARGET = 13,            // 命中概率##
    HIT_TARGET_RATE = 14,       // 命中概率%
    //CRITICAL_STRIKE = 15,       // 暴击概率#
    CRITICAL_STRIKE_RATE = 16,  // 暴击概率%
    //HIT_CRITICAL_STRIKE_RATE = 17,  // 暴击伤害%#
    //DEFENCE_CHARM_RATE = 18,    // 魅惑抵抗#
    //DEFENCE_FEAR_RATE = 19,     // 恐惧抵抗#
    DEFENCE_WOOZY_RATE = 20,    // 眩晕抵抗
    DEFENCE_ICE_RATE = 21,      // 冰冻抵抗
    //DEFENCE_BEATBACK_RATE = 22, // 击退抵抗#
    //DEFENCE_DOWN_RATE = 23,     // 击倒抵抗#
	DEFENCE_HIT_RATE = 24,      // 伤害减免
	MOVE_SPEED = 25,            // 移动速度
	//MOVE_SPEED_RATE = 26,       // 移动速度%#
	//ATTACK_SPEED = 27,          // 攻击速度#
	ATTACK_SPEED_RATE = 28,     // 攻击速度%
	//DODGE = 29,                 // 闪避#
	DODGE_RATE = 30,            // 闪避%
	//DEFENCE_CRITICAL_DAMAGE_RATE = 31,  // 暴击伤害减免%#
    //AUTO_HP = 32,               // 自动生命回复#
    //AUTO_MP = 33,               // 自动魔法回复#
    BLOOD_SUCKING = 34,         // 吸血率（仅普攻，以最终伤害值为基数）
    //RED_REDUCTION_RATE = 35,   //火伤减免#
    //BLUE_REDUCTION_RATE = 36,   //水伤减免#
    //GREEjh_REDUCTION_RATE = 37,   //木伤减免#
    //GOLD_REDUCTION_RATE = 38,   //阳伤减免#
    //SHADOW_REDUCTION_RATE = 39,   //阴伤减免#
    PHYSICAL_DEFENCE = 40,   //物防
    MAGIC_DEFENCE = 41,   //法防
    PHYSICAL_DEFENCE_RATE = 42,   // 物防率
    MAGIC_DEFENCE_RATE = 43,   //法防率
    DEFENCE_CRITICAL_STRIKE_RATE = 44, // 抗暴率
    HIT_RATE  = 45,     // 伤害加深
    DAMAGE_REBOUND_RATE = 46, // 伤害反弹率（对普攻和技能有效，以最终伤害值为基数，对大小Boss无效）
    MAX,
}

public enum BUFFER_TYPE
{
    None = 0,
    ChangeValueBuffer = 1,
    CharmBuffer = 18,
    FearBuffer = 19,
    HoldBuffer = 20,
    IceBuffer = 21,
    BeatBackBuffer = 22,
    DownBuffer = 23,
}

// buff的附加效果，对任意类型buff适用
public enum EXTRA_BUFF_EFFECT
{
    NONE = 0, // 无
    SHOCK_SCERRN = 1, // 普攻暴击震屏
    SPUTTERING = 2, // 普攻溅射
    AURA_BUFF = 3,  // 光环效果(Buff持续期间额外对周围触发Buff)
    TEAM_BUFF=4,//只用于属性计算，非战斗Buff
    EXTRA_BUFF = 5, // Buff结束后以拥有者为圆心额外触发Buff
}


// buff附加效果影响对象类型
public enum EXTRA_TARGET_TYPE
{
    SELF_ALL = 1,   // 指定范围内全体友方
    ENEMY_ALL = 2,  // 指定范围内全体敌方
}


// AffectBuffer表添加字段VALUE_COEFF，为复合字段，内部字段定义如下
// base_coeff add_coeff base_coeff_1 add_coeff_1 均为float类型
// 
// 定义buff释放者：
// 	被动技能 —— 自身
// 	技能附加Buff —— 技能施法者
// 	buff次生buff —— 原buff的释放者
// 
// 定义buff基础数值为buff释放者的静态攻击力（基础攻击力 + 各系统攻击力加成，不含战斗中的动态buff加成）
// 则buff最终数值计算公式为
// 最终数值 = AFFECT_VALUE + ADD_AFFECT_VALUE * buff等级 + 基础数值 * (base_coeff + add_coeff * buff等级)
// 最终数值1 = AFFECT_VALUE_1 + ADD_AFFECT_VALUE_1 * buff等级 + 基础数值 * (base_coeff_1 + add_coeff_1 * buff等级)

public class AffectBuffer : ObjectAI
{
    public DataRecord mAffectConfig;
    public BaseEffect mEffect;
    public BaseObject mStartObject;
    public int mConfigIndex = 0;
    public EXTRA_BUFF_EFFECT mExtraEffect = EXTRA_BUFF_EFFECT.NONE;
    public int mExtraBuff = 0;
    public float mExtraRange = 1f;
    public float mExtraAffectTime = 0f;
    public EXTRA_TARGET_TYPE mExtraTargetType = EXTRA_TARGET_TYPE.ENEMY_ALL;
    public float mElapsed = 0f;

    protected bool mTimeOut = false;
    public int mLevel = 0;
    public int mBaseValue = 0;  // 用于数值buff的基础数值

    static public AFFECT_TYPE GetAffectType(int configIndex)
    {
        string affectTypeStr = TableCommon.GetStringFromAffectBuffer(configIndex, "AFFECT_TYPE");
        return GameCommon.GetEnumFromString<AFFECT_TYPE>(affectTypeStr, AFFECT_TYPE.NONE);
    }

    static public BUFFER_TYPE GetBufferType(int configIndex)
    {
        string bufferTypeStr = TableCommon.GetStringFromAffectBuffer(configIndex, "BUFFER_TYPE");
        return GameCommon.GetEnumFromString<BUFFER_TYPE>(bufferTypeStr, BUFFER_TYPE.None);
    }

    static public string GetAffectName(int index) {
        var record=DataCenter.mAffectBuffer.GetRecord(index);
        return record["TIP_TITLE"];
    }

    static public int GetAffectValue(int index) {
        var record=DataCenter.mAffectBuffer.GetRecord(index);
        return record["AFFECT_VALUE"];
    }

    static public Affect GetAffect(int configIndex)
    {
        var r = DataCenter.mAffectBuffer.GetRecord(configIndex);

        if (r == null)
            return new Affect();

        //by chenliang
        //begin

//        string bufferTypeStr = r["BUFFER_TYPE"];
        //BUFFER_TYPE应该为AFFECT_TYPE
        string bufferTypeStr = r["AFFECT_TYPE"];

        //end
        int bufferValue = r["AFFECT_VALUE"];

        var t = GameCommon.GetEnumFromString<AFFECT_TYPE>(bufferTypeStr, AFFECT_TYPE.NONE);

        string bufferTypeStr1 = r["AFFECT_TYPE_1"];

        //end
        int bufferValue1 = r["AFFECT_VALUE_1"];

        var t1 = GameCommon.GetEnumFromString<AFFECT_TYPE>(bufferTypeStr1, AFFECT_TYPE.NONE);
        Affect temp = new Affect();
        if (t == AFFECT_TYPE.NONE && t1 == AFFECT_TYPE.NONE)
            return temp;

        if (bufferTypeStr.EndsWith("_RATE"))
            temp.Append(new Affect(t, bufferValue / 10000f));
        else 
            temp.Append(new Affect(t, bufferValue));

        if (bufferTypeStr1.EndsWith("_RATE"))
            temp.Append(new Affect(t1, bufferValue1 / 10000f));
        else
            temp.Append(new Affect(t1, bufferValue1));

        return temp;

    }

    //static public AFFECT_TYPE GetAffectTypeOfDefenceByBufferType(BUFFER_TYPE bufferType)
    //{
    //    if (bufferType == BUFFER_TYPE.None && bufferType == BUFFER_TYPE.ChangeValueBuffer)
    //        return AFFECT_TYPE.NONE;
    //    else
    //        return (AFFECT_TYPE)bufferType;
    //}

    static public AffectBuffer StartBuffer(BaseObject startObject, BaseObject applyObject, int bufferIndex, int level)
    {
		DataRecord affectConfig = DataCenter.mAffectBuffer.GetRecord(bufferIndex);

        if (affectConfig != null)
        {
            string strType = affectConfig.getData("BUFFER_TYPE");

            if (strType!="")
            {
                AffectBuffer buffer = EventCenter.Start(strType) as AffectBuffer;
                if (buffer!=null)
                {
                    buffer.mConfigIndex = bufferIndex;
                    buffer.mAffectConfig = affectConfig;
                    buffer.mStartObject = startObject;
                    buffer.mLevel = level;
                    buffer.mBaseValue = startObject == null ? 0 : startObject.mStaticAttack;
                    buffer.Start(applyObject);
                    return buffer;
                }
                else
                {
                    EventCenter.Log(LOG_LEVEL.ERROR, "No register buffer >" + strType);
                }
            }
        }
        return null;
    }

    //public virtual float GetTotalAddByAllLevel(DataRecord mconfig,string field, int mlevel)
    //{
    //    string[] addvalue = ((string)mconfig[field]).Split('/');
    //    float sum = 0;
    //    float e;

    //    for (int i = 0; i < addvalue.Length; ++i)
    //    {
    //        float.TryParse(addvalue[i], out e);
    //        sum += e;
    //    }

    //    int n = addvalue.Length;
    //    int x = (mLevel - 1) / n;
    //    int y = (mLevel - 1) % n - 1;
    //    float z = 0;

    //    while (y >= 0)
    //    {
    //        float.TryParse(addvalue[y], out e);
    //        z += e;
    //        y--;
    //    }
    //    float totaladd = x * sum + z;
    //    return totaladd;
    //}

    public virtual bool HoldAnimation() { return true; }
    public virtual int AffectAttackDamage(int scrDamage) { return 0; }

	public virtual bool ApplyAffect() 
    {
        if (mExtraEffect == EXTRA_BUFF_EFFECT.AURA_BUFF && mExtraAffectTime > 0f)
        {
            StartUpdate();
        }

        return true; 
    }

    protected virtual void StopAffect() 
    {
        if (mExtraEffect == EXTRA_BUFF_EFFECT.EXTRA_BUFF)
        {
            AffectExtraBuff();
        }
    }

    private void AffectExtraBuff()
    {
        bool isEnemy = mExtraTargetType == EXTRA_TARGET_TYPE.ENEMY_ALL;
        ObjectManager.Self.ForEachAlived(x => (GetOwner().IsSameCamp(x) ^ isEnemy) && AIKit.InBounds(GetOwner(), x, mExtraRange), x => x.StartAffect(GetOwner(), mExtraBuff, mLevel));
    }

    public override bool CanDoOnDead() { return true; }
	public override bool NeedFinishOnDead() { return false; }

    public bool Start(BaseObject ownerObject)
    {
        SetOwner(ownerObject);

        if (mAffectConfig == null)
        {
            Log("ERROR: Skill affect config is not exist");
            return false;
        }

        // 动作
        string anim = GetConfig("ANIMATION");

        if (anim != "")
        {
            ownerObject.PlayMotion(anim, null);
        }

        // 特效
        int effectIndex = GetConfig("EFFECT");

        if (effectIndex > 0)
        {
            mEffect = ownerObject.PlayEffect(effectIndex, null);
        }
     
        // 附加效果
        int extraCondition = GetConfig("EXTRA_CONDITION");

        if (System.Enum.IsDefined(typeof(EXTRA_BUFF_EFFECT), extraCondition))
        {
            mExtraEffect = (EXTRA_BUFF_EFFECT)extraCondition;
        }

        int extraTargetType = (int)GetConfig("TARGET");

        if (System.Enum.IsDefined(typeof(EXTRA_TARGET_TYPE), extraTargetType))
        {
            mExtraTargetType = (EXTRA_TARGET_TYPE)extraTargetType;
        }

        mExtraAffectTime = (float)GetConfig("AFFECT_TIME");
        mExtraBuff = (int)GetConfig("EXTRA_BUFFID");
        mExtraRange = (float)GetConfig("RANGE");

        // 持续时间
        float affectTime = BuffGlobal.GetInfo(mConfigIndex).time;//GetConfig("TIME");

        if (affectTime > 0.0001f)
        {
            WaitTime(affectTime);
        }

        return DoEvent();
    }

    public virtual bool Restart()
    {
        if (GetOwner() == null)
        {
            return false;
        }
        float affectTime = BuffGlobal.GetInfo(mConfigIndex).time; //GetConfig("TIME");
        if (affectTime > 0.0001f)
        {
            WaitTime(affectTime);
        }
        return true;
    }

    public virtual bool ChangeStartObject(BaseObject newStartObject)
    {
        if (mStartObject != newStartObject)
        {          
            mStartObject = newStartObject;
            mBaseValue = mStartObject == null ? 0 : mStartObject.mStaticAttack;
            OnStartObjectChanged();
            return true;
        }
        return false;
    }


    public virtual void OnStartObjectChanged() { }

    public override bool _DoEvent()
    {
        return true;
    }

    public override bool Update(float secondTime)
    {
        mElapsed += secondTime;

        if (mElapsed >= mExtraAffectTime)
        {
            mElapsed -= mExtraAffectTime;
            AffectExtraBuff();
        }

        return true;
    }

    public override void _OnOverTime()
    {
        mTimeOut = true;
        Finish();
    }

    public override bool _OnFinish()
    {       
        if (mEffect != null)
            mEffect.Finish();

        if (!GetOwner().mDied)
            StopAffect();
        else
            OnOwnerDead();
        //GetOwner().NotifyBufferFinished(this);

        return true;
    }

    // Callback Methods
    public virtual void OnOwnerDead() { }

    public virtual void OnAttack(BaseObject target) { }

    public virtual void OnHit(BaseObject target, int damage, SKILL_DAMAGE_FLAGS flags) 
    {
        if (mExtraEffect == EXTRA_BUFF_EFFECT.SPUTTERING)
        {
            int factor = GetConfig("SPUTTERING_DAMAGE");
            int damageBaseValue = Mathf.Max(0, GetOwner().GetAttack() - target.GetDefenceByDamageFlags(flags)) * factor / 10000;
            var objMap = ObjectManager.Self.mObjectMap;

            for (int i = objMap.Length - 1; i >= 0; --i)
            {
                var obj = objMap[i];

                if (obj != null
                    && obj != target
                    && !obj.IsDead()
                    && !GetOwner().IsSameCamp(obj)
                    && AIKit.InBounds(target, obj, mExtraRange))
                {
                    int d = obj.TotalDamage(GetOwner(), damageBaseValue);

                    if (d > 0)
                    {
                        obj.ChangeHp(-d);
                        obj.ShowHPPaoPao(-d);
                    }
                }
            }
        }
        else if (mExtraEffect == EXTRA_BUFF_EFFECT.SHOCK_SCERRN)
        {
            if (GetOwner().mbIsCriticalStrike)
            {
                float duration = GetConfig("SHOCKSCERRN_TIME");
                Effect_ShakeCamera.Shake(0.9f, duration);
            }
        }
    }

    public virtual void OnDamage(BaseObject attacker, int damage, SKILL_DAMAGE_FLAGS flags) { }

    public virtual void OnSkill(BaseObject target, int skillIndex) { }

    //public virtual int FinalHit(BaseObject target, int damage) { return 0; }

    //public virtual int FinalDamage(BaseObject attacker, int damage) { return 0; }

    //public virtual int Affect(AFFECT_TYPE affectType) { return 0; }
    //public virtual int AffectRate(AFFECT_TYPE affectType) { return 0; }
   
    //public virtual BUFFER_TYPE GetBufferType() { return GetBufferType(mConfigIndex); }
    //public virtual AFFECT_TYPE GetAffectType() { return GetAffectType(mConfigIndex); }
    public virtual int GetAffectValue(AFFECT_TYPE affectType) { return 0; }

    public Data GetConfig(string configIndex)
    {
        if (mAffectConfig != null)
            return mAffectConfig.getData(configIndex);
        Log("ERROR: no exist config at affect table >"+configIndex.ToString());
        return Data.NULL;
    }
}


public class AI_HoldAI : ObjectAI
{
    public string mAnimationName;

    public override bool HoldControl() { return true; }

    public override bool _DoEvent()
    {
        ActiveObject obj = GetOwner() as ActiveObject;

        if (obj != null)
            obj.mMoveComponent.Stop(null);

        if (string.IsNullOrEmpty(mAnimationName))
            GetOwner().SetAnimSpeed(0f);
        else
            GetOwner().PlayAnim(mAnimationName);

        StartUpdate();
        return true;
    }

    public override bool AllowPlayAnimation(string animName)
    {
        return animName == mAnimationName;
    }

    public override bool Update(float secondTime)
    {
        if (string.IsNullOrEmpty(mAnimationName))
            GetOwner().SetAnimSpeed(0f);
        else
            GetOwner().PlayAnim(mAnimationName);

        return true;
    }

    public override bool _OnFinish()
    {
        GetOwner().SetAnimSpeed(1f);
        return true;
    }
}

public abstract class SpecialAIBuffer : AffectBuffer
{
    //private int mAIHandlerID = 0;
    private AIState state;

    public override bool ApplyAffect()
    {
        //mAIHandlerID = GetOwner().aiMachine.PushBuffAI(GetLayer(), () => Routine.Wrap(GetAI()));
        state = GetState();
        GetOwner().aiMachine.PushState(state);
        return base.ApplyAffect();
    }

    protected override void StopAffect()
    {
        base.StopAffect();

        //GetOwner().aiMachine.PopBuffAI(mAIHandlerID);
        GetOwner().aiMachine.PopState(state);
    }

    //protected virtual BuffAILayer GetLayer()
    //{
    //    return BuffAILayer.Medium;
    //}

    //protected abstract IEnumerator GetAI();
    protected abstract AIState GetState();
}


public class HoldBuffer : SpecialAIBuffer
{

    //public override bool ApplyAffect() 
    //{
    //    GetOwner().StopAI();
        
    //    AI_HoldAI tAI = GetOwner().StartAI("AI_HoldAI") as AI_HoldAI;
    //    tAI.mAnimationName = mAffectConfig.get("ANIMATION");
    //    tAI.DoEvent();

    //    return true;
    //}

    //public override void StopAffect() 
    //{
    //    GetOwner().SetAnimSpeed(1f);
    //    RestartAI(0);
    //}
    //protected override BuffAILayer GetLayer()
    //{
    //    return BuffAILayer.High;
    //}

    //protected override IEnumerator GetAI()
    //{
    //    string anim = mAffectConfig.get("ANIMATION");
    //    return new StayRoutine(GetOwner(), anim);
    //}
    protected override AIState GetState()
    {
        string anim = mAffectConfig.get("ANIMATION");
        return new StayState(GetOwner(), anim);
    }
}


public class CharmBuffer : AffectBuffer
{
    public int mOriginalCampOwner = 0;

    public override bool ApplyAffect()
    {
        //GetOwner().StopAI();
        //mOriginalCampOwner = GetOwner().GetCamp();
        ////!!! Now by common config
        //if (mOriginalCampOwner == CommonParam.SelfCamp)
        //    GetOwner().SetCamp(CommonParam.EnemyCamp);
        //else
        //    GetOwner().SetCamp(CommonParam.SelfCamp);

        //RestartAI(0.01f);
        //return true;
        //base.StopAffect();

        mOriginalCampOwner = GetOwner().GetCamp();
        bool hasStarted = GetOwner().aiMachine.acceptable;

        if (hasStarted)
        {
            GetOwner().aiMachine.Stop();
        }

        if (mOriginalCampOwner == CommonParam.SelfCamp)
        {
            GetOwner().SetCamp(CommonParam.EnemyCamp);
        }
        else
        {
            GetOwner().SetCamp(CommonParam.SelfCamp);
        }

        GetOwner().isCampChanged = true;

        if (hasStarted)
        {
            GetOwner().aiMachine.Start();
        }

        return base.ApplyAffect();
    }

    protected override void StopAffect()
    {
        //GetOwner().StopAI();
        //GetOwner().SetCamp(mOriginalCampOwner);
        //RestartAI(0.01f);
        bool hasStarted = GetOwner().aiMachine.acceptable;

        if (hasStarted)
        {
            GetOwner().aiMachine.Stop();
        }

        GetOwner().SetCamp(mOriginalCampOwner);
        GetOwner().isCampChanged = false;

        if (hasStarted)
        {
            GetOwner().aiMachine.Start();
        }
    }
}


public class FearBuffer : SpecialAIBuffer
{
    //public override bool ApplyAffect()
    //{
    //    bool b = base.ApplyAffect();
        
    //    int anger = Random.Range(0, 360);
    //    Quaternion rotation = Quaternion.Euler(0, (float)anger, 0);
       
    //    Vector3 targetPos = GetOwner().GetPosition() + rotation * Vector3.forward * 10;
    //    Vector3 movePos;
    //    if (GetOwner()._GetLineMovePostion(targetPos, out movePos))
    //    {
    //        GetOwner().MoveTo(movePos, this);
    //        //GetOwner().PlayMotion("fear_run", null);
    //    }
    //    else
    //        Finish();

    //    return true;
    //}

    //public override void StopAffect()
    //{
    //    GetOwner().StopMove();
    //    RestartAI(0.01f);
    //}
    //protected override BuffAILayer GetLayer()
    //{
    //    return BuffAILayer.Medium;
    //}

    //protected override IEnumerator GetAI()
    //{
    //    return new FearRoutine(GetOwner());
    //}
    protected override AIState GetState()
    {
        return new FearState(GetOwner());
    }
}


public class BaseValueBuffer : AffectBuffer
{
    public AFFECT_TYPE mAffectType = AFFECT_TYPE.NONE;
    public int mAffectValue = 0;
    public int mTotalAffectValue = 0;

    public AFFECT_TYPE mAffectType1 = AFFECT_TYPE.NONE;
    public int mAffectValue1 = 0;
    public int mTotalAffectValue1 = 0;

    public override bool ApplyAffect()
    {
        CalculateAffectValues();
        return base.ApplyAffect();
    }

    public override void OnStartObjectChanged()
    {
        base.OnStartObjectChanged();
        CalculateAffectValues();
    }

    private void CalculateAffectValues()
    {
        string coeffParamText = GetConfig("VALUE_COEFF");
        ConfigParam coeffParam = new ConfigParam(coeffParamText);
        //float coeff = GetCoeff(coeffParam.GetFloat("base_coeff"), coeffParam.GetFloatArray("add_coeff"));
        //float coeff1 = GetCoeff(coeffParam.GetFloat("base_coeff_1"), coeffParam.GetFloatArray("add_coeff_1"));

        //string mfield = "ADD_AFFECT_VALUE";      
        //int totaladd = (int)GetTotalAddByAllLevel(mAffectConfig, mfield, mLevel);
        int[] addArray = ConfigParam.ParseIntArray(mAffectConfig["ADD_AFFECT_VALUE"], '/');
        int totaladd = GameCommon.LevelUpValue(addArray, mLevel - 1);
        float coeff = coeffParam.GetFloat("base_coeff") + GameCommon.LevelUpValue(coeffParam.GetFloatArray("add_coeff"), mLevel - 1);
        mAffectValue = GetConfig("AFFECT_VALUE") + totaladd + (int)(mBaseValue * coeff);
        string strType = GetConfig("AFFECT_TYPE");
        mAffectType = GameCommon.GetEnumFromString<AFFECT_TYPE>(strType, AFFECT_TYPE.NONE);

        //string mfield1 = "ADD_AFFECT_VALUE_1";
        //int totaladd1 = (int)GetTotalAddByAllLevel(mAffectConfig, mfield1, mLevel);
        int[] addArray1 = ConfigParam.ParseIntArray(mAffectConfig["ADD_AFFECT_VALUE_1"], '/');
        int totaladd1 = GameCommon.LevelUpValue(addArray1, mLevel - 1);
        float coeff1 = coeffParam.GetFloat("base_coeff_1") + GameCommon.LevelUpValue(coeffParam.GetFloatArray("add_coeff_1"), mLevel - 1);
        mAffectValue1 = GetConfig("AFFECT_VALUE_1") + totaladd1 + (int)(mBaseValue * coeff1);
        string strType1 = GetConfig("AFFECT_TYPE_1");
        mAffectType1 = GameCommon.GetEnumFromString<AFFECT_TYPE>(strType1, AFFECT_TYPE.NONE);

        if (mAffectType1 == mAffectType)
        {
            mAffectValue += mAffectValue1;
            mAffectType1 = AFFECT_TYPE.NONE;
        }
    }

    //private float GetCoeff(float baseCoeff, float[] addCoeffs)
    //{
    //    if(addCoeffs.Length == 0)
    //        return baseCoeff;

    //    int n = addCoeffs.Length;
    //    float sum = 0;

    //    for (int i = 0; i < n; ++i)
    //    {
    //        sum += addCoeffs[i];
    //    }
        
    //    int x = (mLevel - 1) / n;
    //    int y = (mLevel - 1) % n - 1;
    //    float z = 0;

    //    while (y >= 0)
    //    {
    //        z += addCoeffs[y];
    //        y--;
    //    }

    //    return x * sum + z;
    //}

    public override int GetAffectValue(AFFECT_TYPE affectType)
    {
        if (affectType == mAffectType)
        {
            return mTotalAffectValue;
        }
        else if (affectType == mAffectType1)
        {
            return mTotalAffectValue1;
        }

        return 0;
    }

    //public override int Affect(AFFECT_TYPE affectType)
    //{
    //    if (affectType == mAffectType)
    //    {
    //        if (!mIsRate)
    //            return mAffectValue;
    //    }
    //    return 0;
    //}

    //public override int AffectRate(AFFECT_TYPE affectType)
    //{
    //    if (affectType == mAffectType)
    //    {
    //        if (mIsRate)
    //            return mAffectValue;
    //    }
    //    return 0;
    //}

    protected override void StopAffect()
    {
        base.StopAffect();

        if (mAffectType == AFFECT_TYPE.HP_MAX || mAffectType == AFFECT_TYPE.HP_MAX_RATE
            || mAffectType1 == AFFECT_TYPE.HP_MAX || mAffectType1 == AFFECT_TYPE.HP_MAX_RATE)
        {
            GetOwner().ChangeHp(0);
        }
        else if (mAffectType == AFFECT_TYPE.MP_MAX || mAffectType == AFFECT_TYPE.MP_MAX_RATE
            || mAffectType1 == AFFECT_TYPE.MP_MAX || mAffectType1 == AFFECT_TYPE.MP_MAX_RATE)
        {
            Character mainRole = GetOwner() as Character;

            if (mainRole != null)
            {
                mainRole.ChangeMp(0);
            }
        }
    }

    protected void ChangeValue()
    {
        ChangeValue(mAffectType, mAffectValue, ref mTotalAffectValue);
        ChangeValue(mAffectType1, mAffectValue1, ref mTotalAffectValue1);
    }

    private void ChangeValue(AFFECT_TYPE type, int value, ref int totalValue)
    {
        switch (type)
        {
            case AFFECT_TYPE.NONE:
                break;

            case AFFECT_TYPE.HP:
                {
                    mOwner.ChangeHp(value);
                    mOwner.ShowHPPaoPao(value);
                }
                break;

            case AFFECT_TYPE.HP_RATE:
                {
                    int hpChange = (int)(mOwner.GetMaxHp() * value / 10000f);
                    mOwner.ChangeHp(hpChange);
                    mOwner.ShowHPPaoPao(hpChange);
                }
                break;

            case AFFECT_TYPE.MP:
				if (mOwner.GetObjectType() == OBJECT_TYPE.CHARATOR || mOwner.GetObjectType() == OBJECT_TYPE.OPPONENT_CHARACTER)
                {
                    Character character = mOwner as Character;
                    character.ChangeMp(value);
                }
                break;

            case AFFECT_TYPE.MP_RATE:
				if (mOwner.GetObjectType() == OBJECT_TYPE.CHARATOR || mOwner.GetObjectType() == OBJECT_TYPE.OPPONENT_CHARACTER)
                {
                    Character character = mOwner as Character;
                    int mpChange = (int)(character.GetMaxMp() * value / 10000f);
                    character.ChangeMp(mpChange);
                }
                break;

            case AFFECT_TYPE.HP_MAX:
                {
                    totalValue += value;
                    mOwner.ChangeHp(value);                
                }
                break;

            case AFFECT_TYPE.HP_MAX_RATE:
                {
                    totalValue += value;
                    int hpChange = (int)(mOwner.GetBaseMaxHp() * value / 10000f);
                    mOwner.ChangeHp(hpChange);
                }
                break;

            case AFFECT_TYPE.MP_MAX:
                {
                    Character character = mOwner as Character;

                    if (character != null)
                    {
                        totalValue += value;
                        character.ChangeMp(value);
                    }
                }
                break;

            case AFFECT_TYPE.MP_MAX_RATE:
                {
                    Character character = mOwner as Character;

                    if (character != null)
                    {
                        totalValue += value;
                        int mpChange = (int)(character.GetBaseMaxMp() * value / 10000f);
                        character.ChangeMp(mpChange);
                    }
                }
                break;

            //case AFFECT_TYPE.BLOOD_SUCKING:
            //    {
            //        int hpChange = (int)(mOwner.GetAttack() * mAffectValue / 10000f);
            //        mOwner.ChangeHp(hpChange);
            //        ShowPaoPao(mOwner, hpChange);
            //        break;
            //    }

            default:
                totalValue += value;
                break;
        }
    }
}


public class AffectValueBuffer : BaseValueBuffer
{
    public override bool ApplyAffect()
    {
        bool result = base.ApplyAffect();

        if (result)
        {
            ChangeValue();
        }

        return result;
    }
}


public class ChangeValueBuffer : BaseValueBuffer
{
    public string m_strAffectType = "";
    public float mCurSecondTime = 0;
    public int mBufferTimes = 1;
    public float mTotalSecondTime = 0;


    public override bool ApplyAffect()
    {
        mBufferTimes = GetConfig("TIMES");
        mTotalSecondTime = GetConfig("TIME");
        m_strAffectType = GetConfig("AFFECT_TYPE");
        mCurSecondTime = 0;

        StartUpdate();

        return base.ApplyAffect();
    }

    public override bool Update(float secondTime)
    {
        mCurSecondTime += secondTime;
        float interval = mTotalSecondTime / mBufferTimes;

        if (mCurSecondTime >= interval)
        {
            mCurSecondTime -= interval;
            ChangeValue();
        }

        return true;
    }
}


public class ChangeModelBuffer : FearBuffer
{
    public override bool ApplyAffect() 
    {
        int nTargetModel = GetConfig("EFFECT_PARAM");
        GetOwner().CreateMainObject(nTargetModel);
		return base.ApplyAffect(); 
    }

    protected override void StopAffect()
    {
        GetOwner().CreateMainObject((int)GetOwner().GetConfig("MODEL"));

        base.StopAffect();
    }

    //public override int AffectRate(AFFECT_TYPE affectType) 
    //{ 
    //    if (affectType==AFFECT_TYPE.MOVE_SPEED)
    //        return -50;

    //    return 0;
    //}
}


public class Affect : IEnumerable, IEnumerable<KeyValuePair<AFFECT_TYPE, float>>
{
    private Dictionary<AFFECT_TYPE, float> dict = new Dictionary<AFFECT_TYPE, float>();
    
    /*TODO by lhc*/
    public void DebugAffect() {
        dict.Foreach(info => DEBUG.Log(info.Key+"____"+info.Value));
    }

    public Affect()
    { }

    public Affect(AFFECT_TYPE t, float v)
    {
        dict.Add(t, v);
    }

    public Affect(IEnumerable<KeyValuePair<AFFECT_TYPE, float>> i)
    {
        using (var iter = i.GetEnumerator())
        {
            while (iter.MoveNext())
            {
                this[iter.Current.Key] = iter.Current.Value;
            }
        }
    }

    public float this[AFFECT_TYPE t]
    {
        get
        {
            float v = 0f;
            dict.TryGetValue(t, out v);
            return v;
        }
        set
        {
            if (dict.ContainsKey(t))
            {
                dict[t] = value;
            }
            else
            {
                dict.Add(t, value);
            }
        }
    }

    public IEnumerator<KeyValuePair<AFFECT_TYPE, float>> GetEnumerator()
    {
        using (var iter = dict.GetEnumerator())
        {
            while (iter.MoveNext())
            {
                yield return iter.Current;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public void Append(Affect i)
    {
        using (var iter = i.GetEnumerator())
        {
            while (iter.MoveNext())
            {
                this[iter.Current.Key] += iter.Current.Value;
            }
        }
    }

    public void Reverse()
    {
        using (var iter = dict.GetEnumerator())
        {
            while (iter.MoveNext())
            {
                this[iter.Current.Key] = -iter.Current.Value;
            }
        }
    }

    public void Reset()
    {
        dict.Clear();
    }

    public float Final(AFFECT_TYPE t, float baseValue)
    {
        string s = t.ToString();

        if (t == AFFECT_TYPE.PHYSICAL_DEFENCE || t == AFFECT_TYPE.PHYSICAL_DEFENCE_RATE)
        {
            baseValue = baseValue + this[AFFECT_TYPE.PHYSICAL_DEFENCE] + this[AFFECT_TYPE.DEFENCE];
            return baseValue + baseValue * this[AFFECT_TYPE.PHYSICAL_DEFENCE_RATE] + baseValue * this[AFFECT_TYPE.DEFENCE_RATE];
        }
        else if (t == AFFECT_TYPE.MAGIC_DEFENCE || t == AFFECT_TYPE.MAGIC_DEFENCE_RATE)
        {
            baseValue = baseValue + this[AFFECT_TYPE.MAGIC_DEFENCE] + this[AFFECT_TYPE.DEFENCE];
            return baseValue + baseValue * this[AFFECT_TYPE.MAGIC_DEFENCE_RATE] + baseValue * this[AFFECT_TYPE.DEFENCE_RATE];
        }
        else if (t == AFFECT_TYPE.HP || t == AFFECT_TYPE.HP_MAX || t == AFFECT_TYPE.HP_RATE || t == AFFECT_TYPE.HP_MAX_RATE)
        {
            baseValue = baseValue + this[AFFECT_TYPE.HP] + this[AFFECT_TYPE.HP_MAX];
            return baseValue + baseValue * this[AFFECT_TYPE.HP_RATE] + baseValue * this[AFFECT_TYPE.HP_MAX_RATE];
        }
        else if (t == AFFECT_TYPE.MP || t == AFFECT_TYPE.MP_MAX || t == AFFECT_TYPE.MP_RATE || t == AFFECT_TYPE.MP_MAX_RATE)
        {
            baseValue = baseValue + this[AFFECT_TYPE.MP] + this[AFFECT_TYPE.MP_MAX];
            return baseValue + baseValue * this[AFFECT_TYPE.MP_RATE] + baseValue * this[AFFECT_TYPE.MP_MAX_RATE];
        }
        else if (s.EndsWith("_RATE"))
        {
            AFFECT_TYPE abs = GameCommon.GetEnumFromString<AFFECT_TYPE>(s.Substring(0, s.Length - 5), AFFECT_TYPE.NONE);

            if (abs == AFFECT_TYPE.NONE)
            {
                return baseValue + this[t];
            }
            else
            {
                baseValue = baseValue + this[abs];
                return baseValue + baseValue * this[t];
            }
        }
        else
        {
            AFFECT_TYPE rate = GameCommon.GetEnumFromString<AFFECT_TYPE>(s + "_RATE", AFFECT_TYPE.NONE);

            if (rate == AFFECT_TYPE.NONE)
            {
                return baseValue + this[t];
            }
            else
            {
                baseValue = baseValue + this[t];
                return baseValue + baseValue * this[rate];
            }
        }
    }
}