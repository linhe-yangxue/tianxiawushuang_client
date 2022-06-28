using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;
using Utilities;
using Utilities.Routines;


// 技能在准备阶段 执行攻击阶段后 从角色身上移除 自由执行 伤害进度
public abstract class Skill : ObjectAI
{
    static public string msHitAnimName = "hit";

    private static int UniqueID() { return ++uniqueID; }
    private static int uniqueID = 0;

    protected EStateStage mNowStage = EStateStage.START;

    public enum EStateStage
    {
        START,
        ATTACK,
        DAMAGE,
        FINISH,
    }

    public int mUniqueID;   // 唯一ID，用于区分是否是同一次释放的技能
    public int mConfigIndex;
    public DataRecord mConfig;
    public BaseObject mTarget;
    public tLogicData mLogicData;
    public Skill mCallBackOnFinishControl;
    public bool mIndependent = false;
    public int mLevel = 1;
    public event Action mOnApplyFinish;
    public bool mHasAttacked = false;
    public ConfigParam mExtraParam;     // 附加参数，用于技能扩展
    public bool dontStopOnAIBreak = false;
    public bool isFriendSkill { get; private set; }
    public bool isNormalAttack { get; private set; }
    public SKILL_DAMAGE_TYPE damageType { get; private set; }

    //public static bool InRange(Vector3 from, Vector3 to, float range)
    //{
    //    return range >= 0
    //        && Mathf.Abs(to.y - from.y) <= Mathf.Max(range, 2f) 
    //        && (to.x - from.x) * (to.x - from.x) + (to.z - from.z) * (to.z - from.z) <= range * range;
    //}

    public static SEARCH_STRATEGY GetSearchStrategy(int skillIndex)
    {
        return (SEARCH_STRATEGY)TableCommon.GetNumberFromSkillConfig(skillIndex, "TARGET_CONDITION");
    }

    public static SEARCH_STRATEGY GetSearchStrategy(DataRecord config)
    {
        return (SEARCH_STRATEGY)(int)config["TARGET_CONDITION"];
    }

    public static bool IsNormalAttack(int skillIndex)
    {
        return TableCommon.GetNumberFromSkillConfig(skillIndex, "ATTACK_SKILL") == 1;
    }

    public static bool IsNormalAttack(DataRecord config)
    {
        return config["ATTACK_SKILL"] == 1;
    }

    public static SKILL_DAMAGE_TYPE GetDamageType(int skillIndex)
    {
        return (SKILL_DAMAGE_TYPE)TableCommon.GetNumberFromSkillConfig(skillIndex, "ATTACK_TYPE");
    }

    public static SKILL_DAMAGE_TYPE GetDamageType(DataRecord config)
    {
        return (SKILL_DAMAGE_TYPE)(int)config["ATTACK_SKILL"];
    }

    public static SKILL_DAMAGE_TYPE GetDamageType(SKILL_DAMAGE_FLAGS flags)
    {
        if ((flags & SKILL_DAMAGE_FLAGS.IS_PHYSICAL) > 0)
            return SKILL_DAMAGE_TYPE.PHYSICAL;
        else if ((flags & SKILL_DAMAGE_FLAGS.IS_MAGIC) > 0)
            return SKILL_DAMAGE_TYPE.MAGIC;
        else
            return SKILL_DAMAGE_TYPE.MEDIUM;
    }

    public static bool IsFriendSkill(int skillIndex)
    {
        return IsFriendSkill(GetSearchStrategy(skillIndex));
    }

    public static bool IsFriendSkill(DataRecord config)
    {
        return IsFriendSkill(GetSearchStrategy(config));
    }

    public static bool IsFriendSkill(SEARCH_STRATEGY searchStrategy)
    {
        return searchStrategy == SEARCH_STRATEGY.SELF
            || searchStrategy == SEARCH_STRATEGY.FRIEND_NEAREST 
            || searchStrategy == SEARCH_STRATEGY.SELF_LOWEST_HP 
            || searchStrategy == SEARCH_STRATEGY.SELF_LOWEST_HP_RATE
            || searchStrategy == SEARCH_STRATEGY.SELF_CHARACTER
            || searchStrategy == SEARCH_STRATEGY.SELF_DEFENSIVE
            || searchStrategy == SEARCH_STRATEGY.SELF_OFFENSIVE
            || searchStrategy == SEARCH_STRATEGY.SELF_ASSISTANT;
    }

    public static float GetSkillBuffProb(DataRecord config, int level)
    {
        int[] levelUpArray = ConfigParam.ParseIntArray(config["BUFF_PROBABILITY_ADD"], '/');
        return ((int)config["BUFF_PROBABILITY_BASE"] + GameCommon.LevelUpValue(levelUpArray, level - 1)) / 10000f;
    }

    private static Dictionary<int, ConfigParam> skillConditionPool = new Dictionary<int, ConfigParam>();

    public static ConfigParam GetSkillConditionParam(int skillIndex)
    {
        ConfigParam param = null;

        if (!skillConditionPool.TryGetValue(skillIndex, out param))
        {
            DataRecord r = DataCenter.mSkillConfigTable.GetRecord(skillIndex);

            if (r != null)
            {
                string text = r["SKILL_CONDITION"];

                if (text != "" && text != "0")
                {
                    param = new ConfigParam(text);
                }

                skillConditionPool.Add(skillIndex, param);
            }
        }

        return param;
    }

    public override void Stop() { Finish();  }

    public virtual bool Init(BaseObject owner, BaseObject target, int config)
    {
        mUniqueID = UniqueID();
        mOwner = owner;
        mTarget = target;

        mConfigIndex = config;
        mConfig = DataCenter.mSkillConfigTable.GetRecord(config);
        string strExtraParam = mConfig["EXTRA_PARAM"];
        mExtraParam = new ConfigParam(strExtraParam);
        isFriendSkill = IsFriendSkill(mConfig);
        isNormalAttack = IsNormalAttack(mConfig);
        damageType = GetDamageType(mConfig);

        OnSkillInit(owner, target);

        return mConfig != null;
    }

    protected virtual void OnSkillInit(BaseObject owner, BaseObject target) { }

    public Data GetConfig(string configIndex)
    {
        if (mConfig != null)
            return mConfig.getData(configIndex);

        return Data.NULL;
    }
    
    public override bool NeedFinishOnChangeAI() { return false; }
    public override bool AllowPlayAnimation(string animName) { return animName != msHitAnimName; }

    public override bool DoEvent()
    {
        GetOwner().StopMove();

        //if (mLogicData != null && mLogicData.get("PET_DEAD"))
        //    return _ApplyFinish();

        OnStartDoSkill();
        return base.DoEvent();
    }
    
    public override bool _DoEvent()
    {      
        bool result = _AttackStage();
        ExtraOnAttack();
        return result;
    }

    public virtual bool _AttackStage()
    {
        string motionName = GetConfig("ATTACK_MOTION");

        if (motionName != "")
        {
            if (mTarget != mOwner)
            {
                mOwner.SetDirection(mTarget.GetPosition());
            }

            mOwner.PlayMotion(motionName, null);
        }      

        if ((bool)GetConfig("CAMERA_SHAKE"))
        {
            Effect_ShakeCamera.Shake(0.9f, 0.4f);
        }

        // Skill Buff
        int state = GetConfig("ATTACK_STATE");

        if (state > 0)
        {
            AttackState s = new AttackState(state);
            s.ApplyBuffer(GetOwner(), mTarget, AFFECT_TIME.ON_ATTACK);
        }

        int buffId = GetConfig("BUFF_ADDITION");
        int buffTargetType = GetConfig("BUFF_TARGET_TYPE");
        float buffRange = GetConfig("BUFF_RANGE");

        //string[] addvalue = ((string)mConfig["BUFF_PROBABILITY_ADD"]).Split('/');
        //int sum = 0;
        //foreach (string s in addvalue)
        //{
        //    int e;
        //    int.TryParse(s,out e);
        //    sum += e;
        //}
        //int n = addvalue.Length;
        //int x = (mLevel - 1) / n;
        //int y = (mLevel - 1) % n - 1;
        //int z = 0;
        //while (y >= 0)
        //{
        //    int e;
        //    int.TryParse(addvalue[y], out e);
        //    z += e;
        //    y--;
        //}
        //int totaladd = x * sum + z;
        //float buffProb = ((int)GetConfig("BUFF_PROBABILITY_BASE") + totaladd) / 10000f;

        float buffProb = GetSkillBuffProb(mConfig, mLevel);

        if (buffId > 0)
        {
            ApplyBuff(buffId, buffRange, buffTargetType, buffProb);
        }

        OnAttackStage();
        mHasAttacked = true;

        return true;
    }


    private void ApplyBuff(int buffId, float buffRange, int buffTargetType, float buffProb)
    {
        switch ((BUFF_TARGET_TYPE)buffTargetType)
        {
            case BUFF_TARGET_TYPE.DEFAULT:
                {
                    if (AIKit.Triggered(buffProb))
                    {
                        mTarget.StartAffect(GetOwner(), buffId, mLevel);
                    }
                }
                break;

            case BUFF_TARGET_TYPE.SELF:
                {
                    if (AIKit.Triggered(buffProb))
                    {
                        GetOwner().StartAffect(GetOwner(), buffId, mLevel);
                    }
                }              
                break;

            case BUFF_TARGET_TYPE.ENEMY_NEAREST:
                {
                    var o = GetOwner().FindNearestEnemy(false);

                    if (o != null && AIKit.InBounds(GetOwner(), o, buffRange) && AIKit.Triggered(buffProb))
                    {
                        o.StartAffect(GetOwner(), buffId, mLevel);
                    }
                }
                break;

            case BUFF_TARGET_TYPE.FRIEND_NEAREST:
                {
                    var o = GetOwner().FindNearestAlived(x => GetOwner().IsSameCamp(x));

                    if (o != null && AIKit.InBounds(GetOwner(), o, buffRange) && AIKit.Triggered(buffProb))
                    {
                        o.StartAffect(GetOwner(), buffId, mLevel);
                    }
                }
                break;

            case BUFF_TARGET_TYPE.SELF_ALL:
                {
                    ObjectManager.Self.ForEachAlived(x => GetOwner().IsSameCamp(x) && AIKit.InBounds(GetOwner(), x, buffRange) && AIKit.Triggered(buffProb), x => x.StartAffect(GetOwner(), buffId, mLevel));
                }
                break;

            case BUFF_TARGET_TYPE.ENEMY_ALL:
                {
                    ObjectManager.Self.ForEachAlived(x => !GetOwner().IsSameCamp(x) && AIKit.InBounds(GetOwner(), x, buffRange) && AIKit.Triggered(buffProb), x => x.StartAffect(GetOwner(), buffId, mLevel));
                }
                break;

            case BUFF_TARGET_TYPE.SELF_LOWEST_HP:
                {
                    var target = AIKit.FindLowestHpTarget(GetOwner(), buffRange, false);

                    if (target != null && AIKit.Triggered(buffProb))
                    {
                        target.StartAffect(GetOwner(), buffId, mLevel);
                    }
                }
                break;

            case BUFF_TARGET_TYPE.ENEMY_LOWEST_HP:
                {
                    var target = AIKit.FindLowestHpTarget(GetOwner(), buffRange, true);

                    if (target != null && AIKit.Triggered(buffProb))
                    {
                        target.StartAffect(GetOwner(), buffId, mLevel);
                    }
                }
                break;

            case BUFF_TARGET_TYPE.SELF_RAND:
                {
                    var target = AIKit.FindRandTarget(GetOwner(), buffRange, false);

                    if (target != null && AIKit.Triggered(buffProb))
                    {
                        target.StartAffect(GetOwner(), buffId, mLevel);
                    }
                }
                break;

            case BUFF_TARGET_TYPE.ENEMY_RAND:
                {
                    var target = AIKit.FindRandTarget(GetOwner(), buffRange, true);

                    if (target != null && AIKit.Triggered(buffProb))
                    {
                        target.StartAffect(GetOwner(), buffId, mLevel);
                    }
                }
                break;
        }
    }

    protected virtual void OnAttackStage()
    {
        _PlayAttackEffect();
    }

    public virtual bool _ApplyFinish()
    {
        if (mOnApplyFinish != null)
        {
            mOnApplyFinish.Invoke();
        }

        if (mIndependent)
        {
            return true;
        }

        if (mCallBackOnFinishControl != null
            && (mOwner.mCurrentAI == mCallBackOnFinishControl || mCallBackOnFinishControl.mIndependent)
            && !mCallBackOnFinishControl.GetFinished()
            )
        {
            mCallBackOnFinishControl.CallBack(this);
        }

        if (mOwner.mCurrentAI == this)
        {
            //if (mOwner == Character.Self)
            //{
            //    int iNeedMp = GetConfig("NEED_MP");
            //    Character.Self.ChangeMp(-1 * iNeedMp);
            //}
            //
            //
            //if (mLogicData != null)
            //    mLogicData.set("START_CD", true);

            GetOwner().mCurrentAI = null;

            BaseObject enemy = mTarget;
            if (enemy != null && !enemy.IsDead())
            {
                AI_CoolDownAttack ai = StartNextAI("AI_CoolDownAttack") as AI_CoolDownAttack;
                ai.mTarget = enemy;
                ai.DoEvent();
            }
            else
            {
                // NOTE: 如果cd 时间为0, 以默认AI_TIME时间等待            
                RestartAI(0);
            }
            //int stage = GetConfig("ATTACK_STATE");
            //if (stage > 0)
            //{
            //    AttackState s = new AttackState(stage);
            //    s.ApplyBuffer(GetOwner(), mTarget, AFFECT_TIME.ON_ATTACK);
            //}
        }

        return true;
    }

    public virtual void _PlayStartEffect()
    {
        int effectIndex = GetConfig("START_EFFECT");

        if (effectIndex > 0)
            mOwner.PlayEffect(effectIndex, mTarget);
    }

    public virtual void _PlayAttackEffect()
    {
        int effectIndex = GetConfig("ATTACK_EFFECT");
        if (effectIndex > 0)
            mOwner.PlayEffect(effectIndex, mTarget);
    }

    public virtual void _PlayDamageEffect(BaseObject target)
    {
        int effectIndex = GetConfig("DAMAGE_EFFECT");
        if (effectIndex > 0)
            target.PlayEffect(effectIndex, mOwner);
    }

    public virtual bool _StartStage()
    {
        string motionName = GetConfig("START_MOTION");

        bool bResult = false;

        if (motionName != "")
        {
            if (mOwner != mTarget)
            {
                mOwner.SetDirection(mTarget.GetPosition());
            }

            mOwner.PlayMotion(motionName, null);
            bResult = true;
            //return true;
        }

        _PlayStartEffect();

        float t = GetConfig("START_TIME");
        if (t < 0.0001f)
            _OnOverTime();
        else
            WaitTime(t);

        return bResult;
    }

	public virtual bool _DamageStage(){ return false; }

    public void _AffectLater(BaseObject target, float delay)
    {
        if (target != null && !target.IsDead())
        {
            target.StartRoutine(DoAffectLater(target, delay));
        }
    }

    public IEnumerator DoAffectLater(BaseObject target, float delay)
    {
        yield return new Delay(delay);

        if (target != null && !target.IsDead())
        {
            _Affect(target);
        }
    }

    protected int GetAffectCoeff()
    {
        //string[] addvalue = ((string)mConfig["ADD_DAMAGE"]).Split('/');
        //int sum = 0;
        //int e = 0;

        //for (int i = 0; i < addvalue.Length; ++i)
        //{
        //    int.TryParse(addvalue[i], out e);
        //    sum += e;
        //}

        //int n = addvalue.Length;
        //int x = (mLevel - 1) / n;
        //int y = (mLevel - 1) % n - 1;
        //int z = 0;

        //while (y >= 0)
        //{
        //    int.TryParse(addvalue[y], out e);
        //    z += e;
        //    y--;
        //}

        //int totaladd = x * sum + z;
        //return (int)GetConfig("DAMAGE") + totaladd;
        int[] levelUpArray = ConfigParam.ParseIntArray(mConfig["ADD_DAMAGE"], '/');
        return (int)GetConfig("DAMAGE") + GameCommon.LevelUpValue(levelUpArray, mLevel - 1);
    }

    protected int GetAffectValue()
    {
        //string[] addvalue = ((string)mConfig["ADD_DAMAGE_NUM"]).Split('/');
        //int sum = 0;
        //int e = 0;

        //for (int i = 0; i < addvalue.Length; ++i)
        //{
        //    int.TryParse(addvalue[i], out e);
        //    sum += e;
        //}

        //int n = addvalue.Length;
        //int x = (mLevel - 1) / n;
        //int y = (mLevel - 1) % n - 1;
        //int z = 0;

        //while (y >= 0)
        //{
        //    int.TryParse(addvalue[y], out e);
        //    z += e;
        //    y--;
        //}

        //int totaladd = x * sum + z;
        //return (int)GetConfig("DAMAGE_NUM") + totaladd;
        int[] levelUpArray = ConfigParam.ParseIntArray(mConfig["ADD_DAMAGE_NUM"], '/');
        return (int)GetConfig("DAMAGE_NUM") + GameCommon.LevelUpValue(levelUpArray, mLevel - 1);
    }

    public void _Affect(BaseObject target)
    {
        if (isFriendSkill)
        {
            _Cure(target);            
        }
        else 
        {
            _Damage(target);
        }
    }

    public virtual void _Cure(BaseObject target)
    {
        int cureValue = mOwner.GetAttack() * GetAffectCoeff() / 10000 + GetAffectValue();
        cureValue = mOwner.TotalCure(target, cureValue);

        if (cureValue <= 0)
            return;

        target.ChangeHp(cureValue);
        ShowPaoPao(target, cureValue);

        string motionName = GetConfig("HIT_MOTION");

        if (motionName != "" && !mTarget.IsNeedMove())
        {
            target.OnHit(motionName, mOwner);
        }

        if (!(this is BulletSkill))
        {
            _PlayDamageEffect(target);
        }
    }
    
    public virtual void _Damage(BaseObject damageObject)
	{
        SKILL_DAMAGE_FLAGS flags = SKILL_DAMAGE_FLAGS.NONE;

        if (isNormalAttack)
            flags |= SKILL_DAMAGE_FLAGS.IS_NORMAL;

        if (damageType == SKILL_DAMAGE_TYPE.PHYSICAL)
            flags |= SKILL_DAMAGE_FLAGS.IS_PHYSICAL;
        else if (damageType == SKILL_DAMAGE_TYPE.MAGIC)
            flags |= SKILL_DAMAGE_FLAGS.IS_MAGIC;
        
        if (!mOwner.CheckCanHitTarget(damageObject))
        {
            if (MainProcess.mStage != null)
            {
                flags |= SKILL_DAMAGE_FLAGS.IS_MISSING;
                MainProcess.mStage.OnSkillDamage(this, damageObject, 0, flags);
            }

            damageObject.ShowPaoPao("", PAOTEXT_TYPE.DODGE);
            return;
        }

        int defence = damageObject.GetDefenceByDamageType(damageType);
        int damageValue = Mathf.Max(0, mOwner.GetAttack() - defence) * GetAffectCoeff() / 10000 + GetAffectValue();
        damageValue = mOwner.TotalDamage(damageObject, damageValue);
        //int iSkillIndex = (int)GetConfig("INDEX");
        //int finalHit = mOwner.FinalHit(damageObject, damageValue);
        //int finalDamage = damageObject.FinalDamage(mOwner, damageValue);
        //damageValue = damageValue + finalHit + finalDamage;
        damageValue = (int)(damageValue * mOwner.mDamageScale);

        if (damageValue < 0)
            damageValue = 0;

        if (MainProcess.battleIntervencer != null)
            damageValue = MainProcess.battleIntervencer.FinalDamage(this, damageObject, damageValue);

        if (damageValue <= 0)
            return;

        float beatBack = GetConfig("BEAT_BACK");

        if (beatBack > 0.0001f)
        {
            Vector3 dir = damageObject.GetPosition() - GetOwner().GetPosition();
            dir.Normalize();
            var beatbackAI = new BeatBackRoutine(damageObject, damageObject.GetPosition() + dir * beatBack, 1f, 30f);
            damageObject.aiMachine.TrySwitchAI(beatbackAI, AI_LAYER.HIGHEST + 10, new InteruptIntent("BeatBack"));
        }

        damageObject.ApplyDamage(mOwner, this, damageValue);

        if (MainProcess.mStage != null)
        {
            if (mOwner.mbIsCriticalStrike)
            {
                flags |= SKILL_DAMAGE_FLAGS.IS_CRITICAL;
            }

            MainProcess.mStage.OnSkillDamage(this, damageObject, damageValue, flags);
        }

        string motionName = GetConfig("HIT_MOTION");

        if (motionName != "" && !mTarget.IsNeedMove())
        {
            damageObject.OnHit(motionName, mOwner);            
        }

        damageObject.ApplyBufferOnHit(mOwner);

        if (isNormalAttack)
        {
            mOwner.OnHitBuff(damageObject, damageValue, flags);
            damageObject.OnDamageBuff(mOwner, damageValue, flags);      
        }

        // 吸血效果对普攻和非普攻均有效
        int suckingBlood = (int)(mOwner.GetBloodSuckingRate() * damageValue);
        mOwner.ChangeHp(suckingBlood);
        ShowPaoPao(mOwner, suckingBlood);

        if (damageObject.GetObjectType() != OBJECT_TYPE.MONSTER_BOSS && damageObject.GetObjectType() != OBJECT_TYPE.BIG_BOSS)
        {
            int reboundDamage = (int)(damageObject.GetDamageReboundRate() * damageValue);
            mOwner.ChangeHp(-reboundDamage);
            ShowPaoPao(mOwner, reboundDamage);
        }

        // 如果是BulletSkill（或其子类），则在子弹碰撞目标时播放受击特效，受到伤害时不再播放
        if (!(this is BulletSkill))
        {
            _PlayDamageEffect(damageObject);
        }
    }

    private void ShowPaoPao(BaseObject owner, int hpChange)
    {
        if (owner != null)
        {
            if (hpChange > 0)
            {
                owner.ShowPaoPao(hpChange.ToString(), PAOTEXT_TYPE.GREEN);
            }
            else if (hpChange < 0)
            {
                if (owner.GetCamp() == CommonParam.PlayerCamp)
                    owner.ShowPaoPao((-hpChange).ToString(), PAOTEXT_TYPE.RED);
                else
                    owner.ShowPaoPao((-hpChange).ToString(), PAOTEXT_TYPE.WHITE);
            }
        }
    }

    protected virtual void OnStartDoSkill()
    {
        //if (mOwner.mCurrentAI == this)
        //{
        // Change MP
        if (MainProcess.mStage != null)
        {
            MainProcess.mStage.OnDoSkill(this);
        }

        if (mOwner == Character.Self)
        {
            int iNeedMp = SkillGlobal.GetInfo(mConfig).needMP;//GetConfig("NEED_MP");
            Character.Self.ChangeMp(-1 * iNeedMp);
        }

        if (mOwner == OpponentCharacter.mInstance)
        {
            int iNeedMp = GetConfig("NEED_MP");
            OpponentCharacter.mInstance.ChangeMp(-1 * iNeedMp);
        }
        // Start Skill CD
        if (mLogicData != null && mLogicData is SkillButtonData)
        {
            var d = mLogicData as SkillButtonData;
            d.set("START_CD", true);

            // Skill Effect
            //if (GetConfig("IS_ANIME"))
            //{
            //    EffectOnDoSkill.Activate(this, 0.05f);
            //}

            mOwner.RestartSkillCDByUsePos(d.mPetUsePos);
        }
        else
        {
            mOwner.RestartSkillCD(mConfig["INDEX"]);
        }

        // Skill Buff
        //int stage = GetConfig("ATTACK_STATE");
        //if (stage > 0)
        //{
        //    AttackState s = new AttackState(stage);
        //    s.ApplyBuffer(GetOwner(), mTarget, AFFECT_TIME.ON_ATTACK);
        //}
        //}
    }

    //public bool IsAttackSkill()
    //{
    //    int index = mConfig.get("INDEX");

    //    if(index / 1000 == 3)
    //        return true;

    //    int attackSkill = mOwner.mConfigRecord.get("ATTACK_SKILL");
    //    return index == attackSkill;
    //}
    protected virtual void ExtraOnStart()
    {
        //// 附加预警特效，当START_EFFECT被占用时可以使用此配置
        //int warnEffectID = mExtraParam.GetInt("warn_effect", 0);
        //float warnEffectTime = mExtraParam.GetFloat("warn_effect_time", -1f);

        //if (warnEffectID > 0)
        //{
        //    var eff = mOwner.PlayEffect(warnEffectID, mTarget);

        //    if (warnEffectTime > 0f)
        //    {
        //        MainProcess.Self.gameObject.ExecuteDelayed(() => eff.Finish(), warnEffectTime);
        //    }
        //}
    }

    protected virtual void ExtraOnAttack()
    {
        // 附加攻击特效，仅用于表现，不可用于AOE技能
        int attackEffectID = mExtraParam.GetInt("attack_effect", 0);

        if (attackEffectID > 0)
        {
            mOwner.PlayEffect(attackEffectID, mTarget);
        }
    }
}

public class BaseSkill : Skill
{
    protected EStateStage mNowStage = EStateStage.START;    

    public enum EStateStage
    {
        START,
        ATTACK,
        DAMAGE,
        FINISH,
    }

    public override bool _DoEvent()
    {       
        mNowStage = EStateStage.START;
        _DoSkill();
        return true;
    }


    public override bool _DamageStage()
    {
        int damageAngle = GetConfig("DAMAGE_TYPE_PARAM");

        if (damageAngle > 0)
        {
            // apply damage area
            float range = GetConfig("DAMAGE_RANGE");
            double cosRange = System.Math.Cos(damageAngle * 0.5f * System.Math.PI / 180);

            Vector3 dir = mTarget.GetPosition() - GetOwner().GetPosition();
            dir.Normalize();
            var objMap = ObjectManager.Self.mObjectMap;

            for (int i = objMap.Length - 1; i >= 0; --i)
            {
                var obj = objMap[i];

                if (obj != null
                    && !obj.IsDead()
                    && (mOwner.IsSameCamp(obj) ^ !isFriendSkill)
                    && AIKit.InBounds(obj, mOwner, range + obj.mImpactRadius)
                    )
                {
                    Vector3 objDir = obj.GetPosition() - mOwner.GetPosition();
                    objDir.Normalize();

                    if (Vector3.Dot(dir, objDir) > cosRange)
                    {
                        _Affect(obj);
                    }
                }
            }
        }
        else
        {
            float range = GetConfig("DAMAGE_RANGE");

            if ((mOwner.IsSameCamp(mTarget) ^ !isFriendSkill) && AIKit.InBounds(mTarget, mOwner, range + mTarget.mImpactRadius))
            {
                _Affect(mTarget);
            }
        }

		return true;
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

                    if (!_DamageStage())
                    {
                        //Finish();
                    }
                    float t = GetConfig("ATTACK_TIME");
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


    public override void _OnOverTime()
    {
		_DoSkill();
    }
}

// 召唤怪物
public class SummonSkill : Skill
{
    public override bool _DoEvent()
    {
        _StartStage();
        ExtraOnStart();
        return true;
    }

    public override void _OnOverTime()
    {
        // appear monster
        Vector3 pos = GetOwner().GetPosition();

        int count = GetConfig("OBJECT_COUNT");
        int type = GetConfig("OBJECT_ID");
        //float range = GetConfig("DAMAGE_RANGE");
        float time = GetConfig("OBJECT_TIME");
        int percent = GetConfig("DAMAGE");
        string[] addvalue = ((string)mConfig["ADD_DAMAGE"]).Split('/');
        int sum = 0;
        foreach (string s in addvalue)
        {
            int e;
            int.TryParse(s,out e);
            sum += e;
        }
        int n = addvalue.Length;
        int x = (mLevel - 1) / n;
        int y = (mLevel - 1) % n - 1;
        int z = 0;
        while (y >= 0)
        {
            int e;
            int.TryParse(addvalue[y],out e);
            z += e;
            y--;
        }
        int addPercent = x * sum + z;

        for (int i = 0; i < count; ++i )
        {
            BaseObject obj = ObjectManager.Self.CreateObject(OBJECT_TYPE.MONSTER, type);

            if (obj != null)
            {
                obj.SetCamp(GetOwner().GetCamp());
                //obj.InitPosition(GameCommon.RandInCircularGround(pos, range));
                //obj.SetOwnerObject(GetOwner());
                TryInitFollowPosition(GetOwner(), obj);

                Monster monster = obj as Monster;

                if (monster != null)
                {
                    monster.mIsAttributeLocked = true;

                    int hp = (int)(monster.GetBaseMaxHp() * percent / 100f);
                    int attack = (int)(GetOwner().GetBaseAttack() * (percent + addPercent) / 10000f);

                    monster.mBaseMaxHp = hp;
                    monster.mBaseAttack = attack;
                    monster.mStaticMaxHp = hp;
                    monster.mStaticAttack = attack;

                    monster.ChangeHp(0);
                    monster.lifeTime = time < 0.001f ? Mathf.Infinity : time;
                }

                //AutoBattleAI.AddMonster(AutoBattleAI.mMaxIndex+1, obj);

                obj.Start();
            }
        }

        _ApplyFinish();

        Finish();
    }

    private bool TryInitFollowPosition(BaseObject owner, BaseObject target)
    {
        NavMeshPath path = new NavMeshPath();
        target.SetOwnerObject(owner);
        target.mFollowPos = owner.GetFreeFollowPos(8);
        target.mFollowOffset = AIKit.GetFollowOffset(target.mFollowPos);
        owner.SetFollowPosLocked(target.mFollowPos, true);
        Vector3 pos = owner.GetPosition() + target.mFollowOffset;

        if (AIKit.CalculatePath(owner, pos, out path) == NavMeshPathStatus.PathComplete)
        {
            target.InitPosition(pos);
            return true;
        }

        int tryCount = 0;

        while (tryCount < 10)
        {
            pos = GameCommon.RandInCircularGround(owner.GetPosition(), AIParams.followRadius + 1f);

            if (AIKit.CalculatePath(owner, pos, out path) == NavMeshPathStatus.PathComplete)
            {
                target.InitPosition(pos);
                return true;
            }
        }

        target.InitPosition(pos);
        return false;
    }
}


public enum SKILL_DAMAGE_TYPE
{
    MEDIUM = 0,         // 中间伤害，对应  (物防 + 法防) / 2
    PHYSICAL = 1,       // 物理伤害，对应  物防
    MAGIC = 2,          // 魔法伤害，对应  魔防
}

[Flags]
public enum SKILL_DAMAGE_FLAGS
{
    NONE = 0,
    IS_NORMAL = 1,      // 是否普攻
    IS_CRITICAL = 2,    // 是否暴击
    IS_MISSING = 4,     // 是否miss
    IS_PHYSICAL = 8,    // 是否物攻
    IS_MAGIC = 16,      // 是否法攻
}