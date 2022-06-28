using DataTable;
using Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.Routines;

public enum EObjectState
{
	IDLE,
	MOVING,
	ATTACK,
	HIT,
	DEAD,
}

public enum SKILL_POS
{
    SPECIAL_ATTACK = -1,    // 特攻（第一次攻击目标时不会释放）
    CHAR_MAIN = 0,          // 主角主技能
    CHAR_PET_1 = 1,         // 主角宠物技能1
    CHAR_PET_2 = 2,         // 主角宠物技能2
    CHAR_PET_3 = 3,         // 主角宠物技能3
    CHAR_FRIEND = 4,        // 主角好友助战技能
}

public class BaseObject : ILocation
{
	static public int msAttackStageNum = 3;
	static public float msHpBarWidth = 0.9f;
	static public float msHpBarHeight = 0.1f;
	static public Color mModelAddColor = new Color(0, 0, 0, 1);

    public int              mConfigIndex = 0;
	public ObjectFactory 	mFactory;
	public DataRecord		mConfigRecord; 

	public GameObject		mMainObject;
    public GameObject       mBodyObject;
    public bool             mbVisible;

	public ObjectAI			mCurrentAI;
	public Logic.tEvent		mFadeInOrOutEvent;
    public ObjectEvent      mOutlineEvt;

	public int 				mID = -1;
    public int              mCamp = -1;
	public bool				mDied = false;
	public bool				mNeedDestroy = false;

    public int              mServerID = -1;
    
	public EObjectState		mState = EObjectState.IDLE;


	private int				_mHp = 1;
    //private byte[]          _mHpMD5;
    private GameMD5         _mHpMD5 = new GameMD5();

	public ProgressBar 		mShowHpBar;
    public Logic.tEvent     mMotionFinishCallBack;
    public ObjectAI         mRestartAI;

	protected BaseObject	mEnemy;

	public int 				mLevel = 1;

	//public float			mAttack = 1.0f;

    public int              mAttackSkillIndex = 0;
    public AttackState[] mAttackStage = new AttackState[msAttackStageNum];

	public bool				mbIsCriticalStrike = false;
	//public bool				mbIsStrickenCriticalStrike = false;

	public bool				mbIsMainUI = false;
	public bool				mbIsUI = false;

	public GameObject		mEffect;

    public GameObject       mAureoleObj;   //> 橙色符灵脚底下的光环 

    ObjectLocationForPet[] mAttackPoint = new ObjectLocationForPet[6];
    BaseObject[]        mEnemyList = new BaseObject[6];

    public BuffAffectPool mAffectPool = new BuffAffectPool();
    public BaseObjectWrapper mWrapper;
    public BaseObject mOwnerObject;
    public PaoPaoTextQueue mPaoPaoQueue;

	public bool mbIsPvpOpponent = false;
    public bool mbHasStarted = false;

    public float mBaseImpactRadius = 0.5f;  // 基础碰撞半径（不考虑模型缩放）
    public float mBaseSelectRadius = 0.5f;  // 基础有效点选半径（不考虑模型缩放）
    public float mModelScale = 1f;          // 模型缩放
    public float mImpactRadius = 0.5f;  // 最终碰撞半径 = 基础碰撞半径 X 模型缩放
    public float mSelectRadius = 0.5f;  // 最终有效点选半径 = 基础有效点选半径 X 模型缩放

    public Affect mAffect = new Affect();

    public int mBaseMaxHp = 1;                          // 基础最大HP值
    public float mBaseCriticalStrikeRate = 2f;          // 基础暴击概率
    public int mBaseAttack = 1;                         // 基础攻击力
    public float mBaseHitRate = 1f;                     // 基础命中率
    public float mBaseDodgeRate = 0f;                   // 基础闪避率
    public float mBaseAttackSpeed = 1f;                 // 基础攻击速率
    public float mBaseMoveSpeed = 4f;                   // 基础移动速度
    public float mBaseDamageMitigationRate = 0f;        // 基础伤害减免率
    public float mBaseDefenceIceRate = 0f;              // 基础冰冻抵抗
    public float mBaseDefenceWoozyRate = 0f;            // 基础眩晕抵抗
    public int mBasePhysicalDefence = 0;                // 基础物理防御
    public int mBaseMagicDefence = 0;                   // 基础魔法防御
    public float mBaseDefenceCriticalStrikeRate = 0f;   // 基础暴击抵抗
    public float mBaseDamageEnhanceRate = 0f;           // 基础伤害加深
    public float mBaseBloodSuckingRate = 0f;            // 基础吸血率
    public float mBaseDamageReboundRate = 0f;           // 基础反伤率

    public int mStaticMaxHp = 1;                        // 静态最大HP值
    public float mStaticCriticalStrikeRate = 2f;        // 静态暴击概率
    public int mStaticAttack = 1;                       // 静态攻击力
    public float mStaticHitRate = 1f;                   // 静态命中率
    public float mStaticDodgeRate = 1f;                 // 静态闪避率
    public float mStaticAttackSpeed = 1f;               // 静态攻击速率
    public float mStaticMoveSpeed = 4f;                 // 静态移动速度
    public float mStaticDamageMitigationRate = 0f;      // 静态伤害减免率
    public float mStaticDefenceIceRate = 0f;            // 静态冰冻抵抗
    public float mStaticDefenceWoozyRate = 0f;          // 静态眩晕抵抗
    public int mStaticPhysicalDefence = 0;              // 静态魔法防御
    public int mStaticMagicDefence = 0;                 // 静态魔法防御
    public float mStaticDefenceCriticalStrikeRate = 0f; // 静态暴击抵抗
    public float mStaticDamageEnhanceRate = 0f;         // 静态伤害加深
    public float mStaticBloodSuckingRate = 0f;          // 静态吸血率
    public float mStaticDamageReboundRate = 0f;         // 静态反伤率

    public float mDamageScale = 1f;                     // 伤害系数，用于特殊战斗场合中
    public bool mImmortal = false;                      // 特殊状态：不死，当血量降至1后仍可对其造成伤害，但血量不再下降

    protected bool mNeedVerifyMD5 = false;    // 是否需要动态属性MD5验证
    public Dictionary<int, int> mSkillCounter = new Dictionary<int, int>(); // 技能计数器，用于作弊检测
    public OBJECT_FIGHT_TYPE fightintType { get; private set; }
    private float mLookBounds = 8f;

    public static OBJECT_FIGHT_TYPE GetFightingType(BaseObject owner)
    {
        if (owner is Role)
        {
            int t = owner.GetConfig("PET_TYPE");

            switch (t)
            {
                //case 0: return OBJECT_FIGHT_TYPE.OFFENSIVE;
                //case 1: return OBJECT_FIGHT_TYPE.DEFENSIVE;
                //case 2: return OBJECT_FIGHT_TYPE.ASSISTANT;
                //default: return OBJECT_FIGHT_TYPE.UNDEFINED;
                case 0: return OBJECT_FIGHT_TYPE.DEFENSIVE;     // 防御
                case 1:                                         // 物攻
                case 2: return OBJECT_FIGHT_TYPE.OFFENSIVE;     // 法攻
                case 3: return OBJECT_FIGHT_TYPE.ASSISTANT;     // 辅助
                default: return OBJECT_FIGHT_TYPE.UNDEFINED;
            }
        }

        return OBJECT_FIGHT_TYPE.UNDEFINED;
    }

    public int mHp
    {
        get 
        { 
            return _mHp; 
        }
        
        set 
        {
            if (_mHp != value)
            {
                if (mNeedVerifyMD5 && CommonParam.IsVerifyNumberValue)
                {
                    //byte[] hash = MD5Tools.GetMD5(_mHp);

                    //if (_mHpMD5 == null || !hash.SequenceEqual(_mHpMD5))
                    //{
                    //    MainProcess.OnCheatVerifyFailed();
                    //}
                   
                    //_mHpMD5 = MD5Tools.GetMD5(value);
                    if (_mHpMD5.CalcMD5(_mHp) != _mHpMD5.md5)
                    {
                        MainProcess.OnCheatVerifyFailed();
                    }

                    _mHpMD5.md5 = value.ToString();
                }

                _mHp = value;
            }        
        }
    }

    // 属性是否被锁定
    // 若属性未被锁定，则在以下三个时机基础属性和静态属性会被自动计算
    // 1 通过ObjectManager创建该对象时
    // 2 使用ActiveData初始化时
    // 3 战斗中升级时
    // 若属性被锁定，则基础属性和静态属性不会被自动计算，
    // 只能通过手动对各属性赋值或调用InitBaseAttribute()和InitStaticAttribute()方法计算各属性
    public bool mIsAttributeLocked = false;         

	public BaseObject()
	{
        mWrapper = new BaseObjectWrapper(this);
        mPaoPaoQueue = new PaoPaoTextQueue(this);
        //_mHpMD5 = MD5Tools.GetMD5(_mHp);
        _mHpMD5.md5 = _mHp.ToString();
        isCampChanged = false;
	}

	//--------------------------------------------
	public OBJECT_TYPE GetObjectType() { return mFactory.GetObjectType(); }

	public Data GetConfig(string keyIndex)
	{ 
		Data d; 
		d = mConfigRecord.get (keyIndex); 
		if (d.mObj!=null) 
			return d; 

		Log(LOG_LEVEL.ERROR, "no exist config > " + keyIndex);

        return Data.NULL; 
	}

	public void Log(LOG_LEVEL level, string info)
	{
		EventCenter.Log(level, info);
	}

    public virtual void InitBaseAttribute()
    {
        mBaseMaxHp                      = GetBaseMaxHp()                        ;
        mBaseCriticalStrikeRate         = GetBaseCriticalStrikeRate()           ;
        mBaseAttack                     = GetBaseAttack()                       ;
        mBaseHitRate                    = GetBaseHitRate()                      ;
        mBaseDodgeRate                  = GetBaseDodgeRate()                    ;
        mBaseAttackSpeed                = GetBaseAttackSpeed()                  ;
        mBaseMoveSpeed                  = GetBaseMoveSpeed()                    ;
        mBaseDamageMitigationRate       = GetBaseDamageMitigationRate()         ;
        mBaseDefenceIceRate             = GetBaseDefenceIceRate()               ;
        mBaseDefenceWoozyRate           = GetBaseDefenceWoozyRate()             ;
        mBasePhysicalDefence            = GetBasePhysicalDefence()              ;
        mBaseMagicDefence               = GetBaseMagicDefence()                 ;
        mBaseDefenceCriticalStrikeRate  = GetBaseDefenceCriticalStrikeRate()    ;
        mBaseDamageEnhanceRate          = GetBaseDamageEnhanceRate()            ;
        mBaseBloodSuckingRate           = GetBaseBloodSuckingRate()             ;
        mBaseDamageReboundRate          = GetBaseDamageReboundRate()            ;
    }

    public virtual void InitStaticAttribute()
    {
        mStaticMaxHp                        = GetStaticMaxHp()                      ;
        mStaticCriticalStrikeRate           = GetStaticCriticalStrikeRate()         ;
        mStaticAttack                       = GetStaticAttack()                     ;
        mStaticHitRate                      = GetStaticHitRate()                    ;
        mStaticDodgeRate                    = GetStaticDodgeRate()                  ;
        mStaticAttackSpeed                  = GetStaticAttackSpeed()                ;
        mStaticMoveSpeed                    = GetStaticMoveSpeed()                  ;
        mStaticDamageMitigationRate         = GetStaticDamageMitigationRate()       ;
        mStaticDefenceIceRate               = GetStaticDefenceIceRate()             ;
        mStaticDefenceWoozyRate             = GetStaticDefenceWoozyRate()           ;
        mStaticPhysicalDefence              = GetStaticPhysicalDefence()            ;
        mStaticMagicDefence                 = GetStaticMagicDefence()               ;
        mStaticDefenceCriticalStrikeRate    = GetStaticDefenceCriticalStrikeRate()  ;
        mStaticDamageEnhanceRate            = GetStaticDamageEnhanceRate()          ;
        mStaticBloodSuckingRate             = GetStaticBloodSuckingRate()           ;
        mStaticDamageReboundRate            = GetStaticDamageReboundRate()          ;
    }

    public virtual void CloneAttributeFrom(BaseObject cloneFrom)
    {
        mBaseMaxHp                      = cloneFrom.mBaseMaxHp                      ;
        mBaseCriticalStrikeRate         = cloneFrom.mBaseCriticalStrikeRate         ;
        mBaseAttack                     = cloneFrom.mBaseAttack                     ;
        mBaseHitRate                    = cloneFrom.mBaseHitRate                    ;
        mBaseDodgeRate                  = cloneFrom.mBaseDodgeRate                  ;
        mBaseAttackSpeed                = cloneFrom.mBaseAttackSpeed                ;
        mBaseMoveSpeed                  = cloneFrom.mBaseMoveSpeed                  ;
        mBaseDamageMitigationRate       = cloneFrom.mBaseDamageMitigationRate       ;
        mBaseDefenceIceRate             = cloneFrom.mBaseDefenceIceRate             ;
        mBaseDefenceWoozyRate           = cloneFrom.mBaseDefenceWoozyRate           ;
        mBasePhysicalDefence            = cloneFrom.mBasePhysicalDefence            ;
        mBaseMagicDefence               = cloneFrom.mBaseMagicDefence               ;
        mBaseDefenceCriticalStrikeRate  = cloneFrom.mBaseDefenceCriticalStrikeRate  ;
        mBaseDamageEnhanceRate          = cloneFrom.mBaseDamageEnhanceRate          ;
        mBaseBloodSuckingRate           = cloneFrom.mBaseBloodSuckingRate           ;
        mBaseDamageReboundRate          = cloneFrom.mBaseDamageReboundRate          ;

        mStaticMaxHp                      = cloneFrom.mStaticMaxHp                      ;
        mStaticCriticalStrikeRate         = cloneFrom.mStaticCriticalStrikeRate         ;
        mStaticAttack                     = cloneFrom.mStaticAttack                     ;
        mStaticHitRate                    = cloneFrom.mStaticHitRate                    ;
        mStaticDodgeRate                  = cloneFrom.mStaticDodgeRate                  ;
        mStaticAttackSpeed                = cloneFrom.mStaticAttackSpeed                ;
        mStaticMoveSpeed                  = cloneFrom.mStaticMoveSpeed                  ;
        mStaticDamageMitigationRate       = cloneFrom.mStaticDamageMitigationRate       ;
        mStaticDefenceIceRate             = cloneFrom.mStaticDefenceIceRate             ;
        mStaticDefenceWoozyRate           = cloneFrom.mStaticDefenceWoozyRate           ;
        mStaticPhysicalDefence            = cloneFrom.mStaticPhysicalDefence            ;
        mStaticMagicDefence               = cloneFrom.mStaticMagicDefence               ;
        mStaticDefenceCriticalStrikeRate  = cloneFrom.mStaticDefenceCriticalStrikeRate  ;
        mStaticDamageEnhanceRate          = cloneFrom.mStaticDamageEnhanceRate          ;
        mStaticBloodSuckingRate           = cloneFrom.mStaticBloodSuckingRate           ;
        mStaticDamageReboundRate          = cloneFrom.mStaticDamageReboundRate          ;
    }

    public float GetStaticAttribute(AFFECT_TYPE t, float baseValue)
    {
        return AffectFunc.Final(mAffect, t, baseValue);//mRelateAffect.Final(t, mAffect.Final(t, baseValue));
    }

	public void SetFactory(ObjectFactory fact){ mFactory = fact; }

    public virtual bool Init(int configIndex)
    {
        mConfigIndex = configIndex;
        mConfigRecord = mFactory.GetConfig(configIndex);
        if (mConfigRecord == null)
            return false;

        mAttackSkillIndex = GetConfig("ATTACK_SKILL");
        fightintType = GetFightingType(this);
        mLookBounds = (float)mConfigRecord.get("LOOKBOUND");

        mMainObject = new GameObject("_role_");
        //lhcworldmap
		NavMeshAgent nav = mMainObject.AddComponent<NavMeshAgent>();
		nav.height = 2;
		//nav.radius = 0.5f;
        nav.radius = mImpactRadius * CommonParam.navRadiusScale;//AIParams.defaultNavRadius;
        nav.avoidancePriority = 50; 
		nav.enabled = false;
        CreateMainObject((int)mConfigRecord.getData("MODEL"));

        SetMainObject(mMainObject);

        mRestartAI = Logic.EventCenter.Start("TM_RestartAI") as ObjectAI;
		mRestartAI.SetOwner(this);		

        for (int i=0; i<6; ++i)
        {
            mAttackPoint[i] = new ObjectLocationForPet();
            mAttackPoint[i].mPosition = Vector3.zero;
            mAttackPoint[i].mPosition.z = 1.2f;      
        }

        mAttackPoint[0].SetParam(-60 * 0); 
        mAttackPoint[1].SetParam(-60 * 1); 
        mAttackPoint[2].SetParam(60 * 1); 
        mAttackPoint[3].SetParam(-60 * 2); 
        mAttackPoint[4].SetParam(60 * 2); 
        mAttackPoint[5].SetParam(60 * 3); 
	
        //SetAttributeByLevel();
		return true;
    }

	public void SetID(int id){ mID = id; }
    public int GetID() { return mID; }

    public void SetCamp(int campID) { mCamp = campID; }
    public int GetCamp() { return mCamp; }
    public bool IsSameCamp(BaseObject other) { if (other != null) return mCamp == other.GetCamp(); return false; }
    public bool isCampChanged { get; set; } // 阵营是否已改变
    public void SetOwnerObject(BaseObject obj) { mOwnerObject = obj; }
    public BaseObject GetOwnerObject() { return mOwnerObject; }

	public virtual void Destroy()
    { 
        mNeedDestroy = true;
        //LuaBehaviour.RemoveObject(this);
    }

	public bool NeedDestroy(){ return mNeedDestroy; }

	public virtual void OnDestroy()
	{       
		StopAI();
        mEnemy = null;

		if (mFadeInOrOutEvent!=null)
        	mFadeInOrOutEvent.Finish();

        if (mShowHpBar != null)
        {
            mShowHpBar.Destroy();
            mShowHpBar = null;
        }

        if (mMainObject != null)
        {
            GameObject.Destroy(mMainObject);
            mMainObject = null;
        }
        if (mAureoleObj != null) 
        {
            GameObject.Destroy(mAureoleObj);
            mAureoleObj = null;
        }
		mDied = true;
	}
	//-------------------------------------------
    public virtual void OnWillMoveTo(ref Vector3 targetPos) { }
	public bool IsDead(){ return mDied; }

	public virtual void OnDead() 
	{
        stopMoveFrame = -1;

        if (Routine.IsActive(mAIMachine))
        {
            mAIMachine.Break();
        }
        //StopAI();

        //if (mCurrentAI != null && mCurrentAI.NeedFinishOnDead())
        //{
        //    StopAI();
        //}
		mState = EObjectState.DEAD;

        TM_WaitToBeginOutShow waitEvt = StartAI("TM_WaitToBeginOutShow") as TM_WaitToBeginOutShow;
        waitEvt.mOutTime = 2.4f;
        waitEvt.WaitTime(0.8f);
		//StartFadeInOrOut(3, false);
        //LuaBehaviour.RemoveObject(this);

        if (GetOwnerObject() != null)
        {
            GetOwnerObject().SetFollowPosLocked(mFollowPos, false);
        }

        mAliveStartTime = -1f;

        if (MainProcess.mStage != null)
        {
            MainProcess.mStage.OnObjectDead(this);
        }
	}

    public void ShowPaoPao(string strInfo, PAOTEXT_TYPE showFont)
    {
        //DataCenter.SetData("BATTLE_UI", "DEMAGE", new Vector3Data(GetPosition()));

        //UI_PaoPaoText t = EventCenter.Self.StartEvent("UI_PaoPaoText") as UI_PaoPaoText;
        //
        //if (t != null)
        //{
        //    t.InitText(showFont);
        //    t.Start(this, strInfo);
        //}
        //PaoPaoTextPool.Start(showFont, this, strInfo);
        mPaoPaoQueue.Push(strInfo, showFont);
    }

    public void ShowHPPaoPao(int hpChange)
    {
        if (hpChange > 0)
        {
            ShowPaoPao(hpChange.ToString(), PAOTEXT_TYPE.GREEN);
        }
        else if (hpChange < 0)
        {
            if (GetCamp() == CommonParam.PlayerCamp)
                ShowPaoPao((-hpChange).ToString(), PAOTEXT_TYPE.RED);
            else
                ShowPaoPao((-hpChange).ToString(), PAOTEXT_TYPE.RED);
        }
    }

    public virtual void ApplyDamage(BaseObject attacker, Skill skill, int damage)
    {
        ChangeHp(-damage);

        if (attacker == null || skill == null)
            return;

        // 当怪物受到技能伤害时
        if (!skill.isNormalAttack && this.GetCamp() != CommonParam.PlayerCamp)
        {
            ShowPaoPao(damage.ToString(), PAOTEXT_TYPE.BLUE);
        }
        else
        {
            string damageStr = damage.ToString();
            //ELEMENT_RELATION relation = GameCommon.GetElmentRaletion(attacker.mConfigRecord, attacker.mbIsPvpOpponent, this.mConfigRecord, mbIsPvpOpponent);

            //// 当属性被克时受到普通攻击
            //if (skill.IsPhysicalAttackSkill() && relation == ELEMENT_RELATION.ADVANTAGEOUS)
            //    damageStr += "#";

            // 暴击时
            if (attacker.mbIsCriticalStrike)
            {
                ShowPaoPao(damageStr, PAOTEXT_TYPE.YELLOW);
            }
            // 非暴击时
            else
            {
                // 当己方受到伤害时
                if (GetCamp() == CommonParam.PlayerCamp)
                {
                    ShowPaoPao(damageStr, PAOTEXT_TYPE.RED);
                }
                // 当怪物受到伤害时
                else
                {
                    ShowPaoPao(damageStr, PAOTEXT_TYPE.RED);
                }
            }
        }
        //PAOTEXT_TYPE type = PAOTEXT_TYPE.RED;
        //if(mbIsStrickenCriticalStrike)
        //{
        //	type = PAOTEXT_TYPE.YELLOW;
        //}
        //else if (GetCamp()==CommonParam.PlayerCamp)
        //{
        //    type = PAOTEXT_TYPE.WHITE;
        //}
        //
        //ShowPaoPao((-damage).ToString(), type);
    }

	public void ShowHp()
	{
		if (mShowHpBar!=null)
			mShowHpBar.SetValue(GetHp(), GetMaxHp());
	}

	public virtual void BeatBack(Vector3 dir, float backLength){}

    public virtual BaseEffect PlayEffect(int effectIndex, BaseObject targetObject) 
    {
        return PlayEffect(effectIndex, targetObject, false);
    }

    public virtual BaseEffect PlayEffect(int effectIndex, BaseObject targetObject, bool bIsChild)
    {
        return BaseEffect.CreateEffect(this, targetObject, effectIndex, bIsChild);
        //NiceTable table = TableManager.GetTable( "Effect" );
        //if (table==null)
        //    return null;

        //DataRecord config = table.GetRecord( effectIndex );
        //if (config==null)
        //    return null;

        //string effectTypeName = config.getData("TYPE");
        //BaseEffect effect = EventCenter.Self.StartEvent(effectTypeName) as BaseEffect;
        //if (effect != null)
        //    if (effect.InitCreate(this, targetObject, effectIndex))
        //    {
        //        effect.DoEvent();
        //        return effect;
        //    }
        //return null;
    }

    public virtual void OnAttack(BaseObject target) 
    {
        foreach (AttackState state in mAttackStage)
        {
            if (state != null)
                state.ApplyBuffer(this, target, AFFECT_TIME.ON_ATTACK);
        }
    }

    public virtual void OnDoSkill(BaseObject target, int index) { }

	public virtual bool CanDoSkill(DataRecord config)
	{
        return config["ATTACK_SKILL"] == 1 || !IsSilence();
	}

    public bool CanDoSkill(int index)
    {
        DataRecord r = DataCenter.mSkillConfigTable.GetRecord(index);

        if (r == null)
        {
            DEBUG.LogError("Skill " + index + " has no config (Object ID = " + mConfigIndex + ")");
            return false;
        }

        return CanDoSkill(DataCenter.mSkillConfigTable.GetRecord(index));
    }

    public virtual Skill DoSkill(int skillIndex, BaseObject target, tLogicData extData)
    {
        DataRecord config = DataCenter.mSkillConfigTable.GetRecord(skillIndex);
        if (config == null)
            return null;

        if (!CanDoSkill(config) || (mCurrentAI != null && mCurrentAI.HoldControl()))
            return null;

        string skillTypeName = config.getData("TYPE");
        Skill skill = ForceStartAI(skillTypeName) as Skill;

        if (skill != null)
        {
            if (skill.Init(this, target, skillIndex))
            {
                skill.mLogicData = extData;
                skill.DoEvent();

                if (skillIndex == mAttackSkillIndex)
                {
                    OnAttack(target);
                }
                else 
                {
                    OnDoSkill(target, skillIndex);
                }

                return skill;
            }
        }

        return null;
    }

    public virtual bool SkillAttack(int skillIndex, BaseObject enemy, tLogicData extData)
    {
        // First check can use skill
        DataRecord config = DataCenter.mSkillConfigTable.GetRecord(skillIndex);
        if (!CanDoSkill(config))
            return false;

        if (enemy!=null && Vector3.Distance(enemy.GetPosition(), GetPosition()) > SkillBound(skillIndex))
        {
            AI_MoveToTargetThenSkill ai = StartAI("AI_MoveToTargetThenSkill") as AI_MoveToTargetThenSkill;
            ai.SetTarget(enemy);
            ai.mWaitSkillIndex = skillIndex;
            ai.mSkillData = extData;
            ai.DoEvent();
            return true;
        }
        return DoSkill(skillIndex, enemy, extData)!=null;
    }

    public AffectBuffer StartAffect(BaseObject startObject, int affectConfig) 
    {
       return StartAffect(startObject, affectConfig, 1);
    }

    public virtual AffectBuffer StartAffect(BaseObject startObject, int affectConfig, int level) { return null; }
    public virtual void StopAffect(int affectConfig) { }

	public virtual void StopMove()
    {
        stopMoveFrame = -1;
    }

	public virtual void _StopMove(){}

	public void SetVisibleHp(bool bShow)
	{
		if (mShowHpBar!=null)
			mShowHpBar.SetVisible(bShow);
	}

	public virtual void _UpdateHpBarPos() {}


	public void StartFadeInOrOut(float fUseTime, bool bIn)
	{
		if (mFadeInOrOutEvent!=null)
			mFadeInOrOutEvent.Finish ();

		mFadeInOrOutEvent = _StartEvent("Object_FadeInOrOut");

		Object_FadeInOrOut evt = mFadeInOrOutEvent as Object_FadeInOrOut;

		if (evt!=null)
			evt.Start(fUseTime, bIn);
	}
	//--------------------------------------------
	
	public bool SetMainObject(GameObject obj)
	{
		mMainObject = obj;

		if (mMainObject!=null)
			OnInitModelObject();

		return mMainObject!=null;
	}

	public virtual void OnInitModelObject()
	{
		float s = GetConfig("SCALE");
		if (s>0.00001f)
			SetScale(s);

//		SetLightColor(mModelAddColor);        

        // add effect
        int[] effectList;
		string strEffectIndex = "";
		if(mbIsUI && !mbIsMainUI)
		{
			ConsumeItemLogicStatus logic = DataCenter.GetData ("CONSUME_ITEM_STATUS") as ConsumeItemLogicStatus;
			if(logic != null && logic.GetChangeModelIndex () != 0)
				strEffectIndex = "";
			else
				strEffectIndex = GetConfig("UI_EFFECT");
		}
		else
		{
			strEffectIndex = GetConfig("EFFECT");
		}

		if (GameCommon.SplitToInt(strEffectIndex, ' ', out effectList) > 0)
		{
			foreach (int effectIndex in effectList)
			{
				PlayEffect(effectIndex, null, true);
			}
		}
	}

    public virtual void SetLightColor(Color lightColor)
    {
        Renderer[] rendlist = mBodyObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer render in rendlist)
        {
            if (render != null)
            {
                render.castShadows = false;
                render.receiveShadows = false;
                render.material.SetColor("_AddColor", lightColor);
            }
        }

    }

    public virtual void ShowOutline(bool bShow)
    {
        if (mOutlineEvt != null)
            mOutlineEvt.Finish();
        if (bShow)
        {
            bool bVisible = mbVisible;
            if (!bVisible)
                SetVisible(true);

            mOutlineEvt = EventCenter.Start("Object_OutLine") as ObjectEvent;
            if (mOutlineEvt != null)
            {
                mOutlineEvt.SetOwner(this);
                mOutlineEvt.DoEvent();
            }

            if (!bVisible)
                SetVisible(false);
        }

        //Renderer[] r = mBodyObject.GetComponentsInChildren<Renderer>();
        //foreach (Renderer mesh in r)
        //{
        //    //ShowSelected show = mesh.gameObject.GetComponent<ShowSelected>();
        //    //if (show==null)
        //    //{
        //    //    show = mesh.gameObject.AddComponent<ShowSelected>();
        //    //    show.outterColor = Color.red;
        //    //}

        //    mesh.gameObject.layer = bShow ? GameCommon.OutlineLayer : GameCommon.PlayerLayer;
        //}
    }
	
	public bool CreateMainObject(string objName)
	{
		objName = "models/"+objName;
        GameObject o = GameCommon.LoadAndIntanciatePrefabs(objName);
		if (o!=null)
		{
            return SetMainObject(o);
		}
		return false;
	}

    public bool CreateMainObject(int modelIndex)
    {
        if (mMainObject == null)
            return false;

        if (mFadeInOrOutEvent != null)
        {
            mFadeInOrOutEvent.Finish();
            mFadeInOrOutEvent = null;
        }

        foreach (Transform form in mMainObject.transform)
        {
            form.gameObject.SetActive(false);
            GameObject.Destroy(form.gameObject);
        }
		 
        DataRecord modelRecord = DataCenter.mModelTable.GetRecord(modelIndex);
        if (modelRecord!=null)
        {
            string anim = modelRecord.getData("ANIMATION");         
            string body = modelRecord.getData("BODY");

            if (anim != "" && body != "")
            {
                RuntimeAnimatorController control = GameCommon.LoadController(anim); //Resources.Load(anim, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                if (control == null)
                    EventCenter.Log(LOG_LEVEL.ERROR, "Anim control no exist > " + anim);

                string tex1 = modelRecord.getData("BODY_TEX_1");
                mBodyObject = _CreatePart(body, mMainObject, control, tex1);
                if (mBodyObject != null)
                {                    
                    //Renderer[] rendList = bodyObject.GetComponentsInChildren<Renderer>();
                    //foreach (Renderer rend in rendList)
                    //{
                    //    rend.castShadows = false;
                    //    rend.receiveShadows = false;
                    //}
                    
                    //tex1 = modelRecord.getData("HEAD_TEX_1");
                    //_CreatePart((string)modelRecord.getData("HEAD"), mMainObject, control, tex1);
                    //tex1 = modelRecord.getData("WEAPON_TEX_1");
                    //_CreatePart((string)modelRecord.getData("WEAPON"), mMainObject, control, tex1);  
                    //by chenliang
                    //begin

//                     mImpactRadius = (float)modelRecord["IMPACT_RADIUS"] * (float)GetConfig("SCALE");
//                     mSelectRadius = (float)modelRecord["SELECT_RADIUS"] * (float)GetConfig("SCALE");
//----------------------------
                    if(modelRecord != null && mConfigRecord != null)
                    {
                        mBaseImpactRadius = (float)modelRecord["IMPACT_RADIUS"];
                        mBaseSelectRadius = (float)modelRecord["SELECT_RADIUS"];
                        mModelScale = (float)GetConfig("SCALE");
                        mImpactRadius = mBaseImpactRadius * mModelScale;
                        mSelectRadius = mBaseSelectRadius * mModelScale;
                    }

                    //end

                    OnModelChanged();

                    SetVisible(mbVisible);

                    //SetMainObject(mMainObject);
                    return true;
                }
            }
        }
        foreach (Transform form in mMainObject.transform)
        {
            form.gameObject.SetActive(false);
            GameObject.Destroy(form.gameObject);
        }               
        return false;
    }

    public virtual void OnModelChanged() { }

    static public bool _AppendPart(string partModelResName, GameObject bodyObj, GameObject parentObj, string textureResName)
    {
        if (partModelResName == "")
            return false;

        GameObject obj = GameCommon.CreateObject(partModelResName) as GameObject;
        if (obj != null)
        {
            GameCommon.SetLayer(obj, parentObj.layer);
            SkinnedMeshRenderer rend = obj.GetComponentInChildren<SkinnedMeshRenderer>();
            if (rend != null)
            {
                GameObject bone = GameCommon.FindObject(parentObj, rend.rootBone.name);
                if (bone != null)
                    rend.rootBone = bone.transform;

                obj.transform.parent = parentObj.transform;

                obj.transform.localScale = Vector3.one;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localPosition = Vector3.zero;

                if (textureResName != "")
                {
                    Texture tex = GameCommon.LoadTexture(textureResName, LOAD_MODE.RESOURCE);
                    if (tex != null)
                    {
                        GameCommon.SetObjectTexture(obj, tex);
                    }
                }

                return true;
            }
       }
        return false;
    }


    static public GameObject _CreatePart(string partModelResName, GameObject parentObj, RuntimeAnimatorController animControl, string textureResName)
    {
        if (partModelResName=="")
            return null;

		GameObject obj = GameCommon.LoadModel(partModelResName); //GameCommon.CreateObject(partModelResName);
        if (obj != null)
        {
            GameCommon.SetLayer(obj, parentObj.layer);
            Animator anim = obj.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.applyRootMotion = false;
                anim.runtimeAnimatorController = animControl;
            }
            if (textureResName != "")
            {
                Texture tex = GameCommon.LoadTexture(textureResName, LOAD_MODE.RESOURCE);
                if (tex != null)
                {
                    GameCommon.SetObjectTexture(obj, tex);
                }
            }

            obj.transform.parent = parentObj.transform;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localPosition = Vector3.zero;

            return obj;
        }
        EventCenter.Log(LOG_LEVEL.WARN, "Load fail > " + partModelResName);
        return null;
    }

	public virtual void SetScale(float s)
	{
		if (mMainObject!=null)
			mMainObject.transform.localScale = new Vector3(s, s, s);
	}

	public virtual void InitPosition(Vector3 pos)
	{
		SetPosition(pos);
        prePosition = pos;
	}

    public bool   PlayMotion(string motionName, Logic.tEvent callBack)
    {
        if (mMainObject != null && !IsDead())
        {
            PlayAnim(motionName);
            mMotionFinishCallBack = callBack;
        }
        return false;
    }

    public virtual void _PlayAnim(string animName) { }
    public virtual void _PlayAnim(string animName, bool bLoop) { }

    private string willAnimName = "";
    private int willAnimMode = -1;

    public void PlayAnim(string animName) 
    {
        if (!mbHasStarted)
        {
            _PlayAnim(animName);
        }
        else 
        {
            if (animName != "" && animName != "0")
            {
                willAnimName = animName;
                willAnimMode = 0;
            }
        }
    }

    public void PlayAnim(string animName, bool bLoop) 
    {
        if (!mbHasStarted)
        {
            _PlayAnim(animName, bLoop);
        }
        else 
        {
            if (animName != "" && animName != "0")
            {
                willAnimName = animName;
                willAnimMode = bLoop ? 1 : 2;
            }
        }
    }

    public virtual void StopAnim() { }
    public virtual void SetAnimSpeed(float speed) { }

    public virtual string GetCurrentAnimName() { return ""; }
	
	public virtual void SetPosition(Vector3 pos)
	{
		if(mMainObject != null)
			mMainObject.transform.position = pos;

		//_UpdateHpBarPos();
	}

    public virtual void OnPositionChanged() { }
    public virtual void OnMoveEnd() { }

    public virtual void OnLookEnemy() { }

    private Quaternion mWillRotation = Quaternion.identity;
    private static readonly float mRotationDamping = 15f;
    public bool mbSmoothRotation = true;
	
	public void SetDirection(Vector3 lookPos)
	{
        if (mMainObject != null)
        {
            Vector3 dir = lookPos - mMainObject.transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.0001f)
                return;

            mWillRotation = Quaternion.LookRotation(dir, Vector3.up);

            if (!mbHasStarted || !mbSmoothRotation)
            {
                mMainObject.transform.rotation = mWillRotation;
            }
        }
	}

    public void SetRealDirection(Vector3 lookPos)
    {
        if (mMainObject != null)
        {
            Vector3 dir = lookPos - mMainObject.transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.0001f)
                return;

            mWillRotation = Quaternion.LookRotation(dir, Vector3.up);
            mMainObject.transform.rotation = mWillRotation;
        }
    }

	public Quaternion GetDirection()
	{
        return mbSmoothRotation ? mWillRotation : (mMainObject != null ? mMainObject.transform.rotation : Quaternion.identity);
	}

    public Quaternion GetRealDirection()
    {
        return mMainObject != null ? mMainObject.transform.rotation : Quaternion.identity;
    }

	public Vector3 GetPosition()
	{
		if(mMainObject != null)
		return mMainObject.transform.position;
		return Vector3.zero;
	}
	
	public virtual ObjectAI StartAI(string aiName)
	{		
		ObjectAI ai = (ObjectAI)Logic.EventCenter.Self.StartEvent(aiName);
        if (ai!=null)
		{
            //if (mCurrentAI != null && mCurrentAI.mAILevel > ai.mAILevel)
            //    return null;
            ObjectAI nowAI = mCurrentAI;
            //StopAI();
            mCurrentAI = ai;
			ai.SetOwner(this);
            if (nowAI != null)
                nowAI.Finish();
		}
		return ai;
	}

	public virtual ObjectAI StartDoAI(string aiName)
	{
		ObjectAI ai = StartAI(aiName);
		if (ai!=null)
			ai.DoEvent();

		return ai;
	}

    public virtual void OnStartAI() { }

	public virtual ObjectAI StartAI()
    {
        OnStartAI();
        aiMachine.Start();
        return null; 
    }

	public virtual ObjectAI ForceStartAI(string aiName)
	{
        StopAI();

        return StartAI(aiName);
	}


	
	public void StopAI()
	{
        if (mCurrentAI != null)
        {
            mCurrentAI.Finish();
        }

        if (Routine.IsActive(mAIMachine))
        {
            mAIMachine.Stop();
        }
        //if (Routine.IsActive(aiRoutine))
        //{
        //    aiRoutine.Break();
        //}
        //ObjectAI temp = mCurrentAI;
        //mCurrentAI = null;
        //if (temp != null)
        //{
        //    temp.Stop();
        //    temp.Finish();
        //}
        //if (mCurrentAI!=null)
        //{
        //    if (mCurrentAI.NeedFinishOnChangeAI())
        //        mCurrentAI.Finish();			
        //}
	}


	public virtual Logic.tEvent _StartEvent(string evtName)
	{
		Logic.tEvent evt = Logic.EventCenter.Self.StartEvent(evtName);

		ObjectEvent ai = evt as ObjectEvent;
		if (ai!=null)
		{
			ai.SetOwner(this);						
		}

		return evt;
	}
	//-------------------------------------------------
	public virtual void Start () 
	{
        if (mShowHpBar == null && GameCommon.FindObject(mMainObject, "Bip01 Head") != null)
        {
            mShowHpBar = new HpBar();
            mShowHpBar.Init(msHpBarWidth, msHpBarHeight, GetCamp()==CommonParam.EnemyCamp ? Color.red:Color.green, Color.blue);
			mShowHpBar.InitValue(GetMaxHp(), GetMaxHp());
            ShowHp();

			mShowHpBar.InitBloodMark (GameCommon.GetActiveObjectElement(mConfigRecord, mbIsPvpOpponent));
			if(this is MonsterBoss || this is BigBoss)
				mShowHpBar.InitBossBloodMark ();
        }
        _UpdateHpBarPos();
        //LuaBehaviour.AddObject(this);

        OnStart();
        mbHasStarted = true;

        if (MainProcess.mStage.aiEnabled)
        {
            StartAI();
        }

        mAliveStartTime = Time.time;

        if (MainProcess.mStage != null)
        {
            MainProcess.mStage.OnObjectStart(this);
        }
	}

	public virtual void OnStart(){}

	// Update is called once per frame
	public virtual void Update (float onceTime) 
	{
        if (mbHasStarted)
        {
            if (Time.frameCount == stopMoveFrame)
            {
                StopMove();
            }

            if (willAnimMode != -1)
            {
                if (willAnimMode == 0)
                {
                    _PlayAnim(willAnimName);
                }
                else
                {
                    _PlayAnim(willAnimName, willAnimMode == 1);
                }

                willAnimMode = -1;
            }

            if (onceTime > 0.0001f)
            {
                velocity = (GetPosition() - prePosition) / onceTime;
                velocity.y = 0f;
            }

            prePosition = GetPosition();

            if (mMainObject != null && mbSmoothRotation)
            {
                mMainObject.transform.rotation = Quaternion.Lerp(mMainObject.transform.rotation, mWillRotation, onceTime * mRotationDamping);
            }
        }

        if (mNeedDestroy)
            return;

        if (mMotionFinishCallBack != null && !mMotionFinishCallBack.GetFinished())
        {
            if (IsAnimStoped())
            {
				tEvent tempEvt = mMotionFinishCallBack;
				mMotionFinishCallBack = null;
				tempEvt.CallBack(this);
            }
        }

        if (mbHasStarted)
        {
            mAffectPool.Update();

            if (aliveTime >= lifeTime && !IsDead())
            {
                OnLifeTimeOver();
                ChangeHp(-99999999);
            }

            mPaoPaoQueue.Update(onceTime);
        }
	}

    public virtual bool IsAnimStoped() { return true; }

    public BaseObject FindNearestEnemy(bool global = false)
    {
        if (global)
            return FindNearestAlived(x => !IsSameCamp(x));
        else
            return FindNearestAlived(x => !IsSameCamp(x) && InLookBounds(x));
    }
	
    //public BaseObject FindNearestEnemyInLookBounds()
    //{
    //    return FindNearestAlived(x => !IsSameCamp(x) && InLookBounds(x));
    //    //float dis = -1;
    //    //BaseObject resultObj = null;

    //    //foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
    //    //{
    //    //    if (obj!=null && IsEnemy(obj))
    //    //    {
    //    //        float d = Vector3.Distance(GetPosition(), obj.GetPosition());
    //    //        if ( d<=LookBounds()
    //    //            && (dis < 0 ||  d < dis) )
    //    //        {
    //    //            dis = d;
    //    //            resultObj = obj;
    //    //        }
    //    //    }
    //    //}        

    //    //return resultObj;
    //}

    public virtual BaseObject FindAttackBoundEnemy()
    {
        BaseObject obj = FindNearestEnemy();

        if (obj != null && Vector3.Distance(obj.GetPosition(), GetPosition()) < AttackBound())
            return obj;

        return null;
    }

    //public virtual BaseObject FindLookBoundEnemy()
    //{
    //    BaseObject obj = FindNearestEnemy();

    //    if (obj != null && Vector3.Distance(obj.GetPosition(), GetPosition()) < LookBounds())
    //        return obj;

    //    return null;
    //}

    public virtual void OnStartAttackEnemy(BaseObject enemy) { }

    public virtual bool SetCurrentEnemy(BaseObject enemy)
    {
        if (enemy==null || mEnemy != enemy && mEnemy == null || mEnemy.IsDead())
        {
            mEnemy = enemy;
            if (mEnemy != null && !mEnemy.IsDead() && mEnemy.mCurrentAI == null && !(mEnemy is Character))
            {
                mEnemy.SetVisible(true);
                mEnemy.Start();
            }
            return mEnemy!=null;
       }
        return false;
    }

    public BaseObject GetCurrentEnemy() { return mEnemy;  }

    public virtual BaseObject GetCampEnemy()
    {
        //if (mEnemy != null && !mEnemy.IsDead())
        //{
        //    return mEnemy;
        //}
        //foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
        //{
        //    if (obj != null && obj != this && obj.GetOwner() == GetOwner() && !obj.IsDead()
        //        && obj.mEnemy != null && !obj.mEnemy.IsDead())
        //        return obj.mEnemy;
        //}

        return null;
    }

    // 攻击者占位, 以前方向两边 0~5 共6个位置, 一旦被占用, 直到占位对象死掉, 才可以再次被使用
    public virtual ObjectLocationForPet GetAttackPointLocation(BaseObject targetEnemy)
    {
        for (int i=0; i<mEnemyList.Length; ++i)
        {
            if (mEnemyList[i]==null || mEnemyList[i]==targetEnemy || mEnemyList[i].IsDead())
            {
				mEnemyList[i] = targetEnemy;
                return mAttackPoint[i];
            }
        }

        return null;
    }


    public virtual bool MoveTo(Vector3 targetPos, ObjectAI finishAI) 
    {
        stopMoveFrame = -1;
        return false; 
    }

    public virtual bool MoveToThenCallback(Vector3 targetPos, Action<bool> onReach, float clampSpeed) 
    {
        stopMoveFrame = -1;
        return false; 
    }

    public virtual bool AllowCameraUpdateHeight() { return true;  }

    public virtual ObjectAI StartMoveAI()
    {
        return ForceStartAI("AI_MoveTo");
    }

    public virtual ObjectAI StartRestartAI()
    {
        StopAI();
        mCurrentAI = mRestartAI;
        mRestartAI.SetFinished(false);
        return mRestartAI;
    }

    // 治疗基础值 = 施法者攻击 * 施法者技能系数 + 施法者技能固定值
    // 治疗最终值 = 治疗基础值 * [95%-105%随机值]
    public int TotalCure(BaseObject targetObject, int baseValue)
    {
        return (int)(baseValue * UnityEngine.Random.Range(0.95f, 1.05f));
    }

    // 技能伤害基础值 = (施法者攻击 - 目标防御) * 施法者技能系数 + 施法者技能固定值
    // 溅射buff伤害基础值 = (施法者攻击 - 目标防御) * 溅射系数
    // 伤害最终值 = 伤害基础值 * [95%-105%随机值] * (1 + 施法者伤害加成 - 目标伤害减免）* [暴击/闪避]
	public int TotalDamage(BaseObject targetObject, int baseValue)
	{
        float fCriticalStrikeDamageRate = 1.0f;
		SetCriticalStrikeRate(targetObject);
		
		if(mbIsCriticalStrike)
		{
            fCriticalStrikeDamageRate = GetCriticalStrikeDamageRate();
		}

        float fDamageEnhanceRate = GetDamageEnhanceRate();
		float fDamageMitigationRate = targetObject.GetDamageMitigationRate();
        float fDamageRate = fCriticalStrikeDamageRate * Mathf.Max(0f, 1f + fDamageEnhanceRate - fDamageMitigationRate);
        float fDamage = baseValue * fDamageRate * UnityEngine.Random.Range(0.95f, 1.05f);
		return (int)fDamage;
	}

    public virtual void PursueAttack(BaseObject enmey) { }
    public virtual void Attack(Vector3 attackPos) { }
    public virtual bool Attack(BaseObject target) 
    {
        int skillIndex = mAttackSkillIndex;
        if (skillIndex > 0)
        {
            return SkillAttack(skillIndex, target, null);
            //float dis = Vector3.Distance(GetPosition(), target.GetPosition());
            //if (dis < SkillBound(skillIndex))
            //{
            //    return DoSkill(skillIndex, target, null)!=null;
            //}
            //else
            //{
            //    AI_MoveToTargetThenSkill ai = StartAI("AI_MoveToTargetThenSkill") as AI_MoveToTargetThenSkill;
            //    if (ai != null)
            //    {
            //        ai.SetTarget(target);
            //        ai.mWaitSkillIndex = skillIndex;
            //        ai.DoEvent();
            //    }
            //}
        }
        EventCenter.Log(LOG_LEVEL.ERROR, "No config attack skill >" + mConfigIndex.ToString());
        return false; 
    }
	public virtual void OnAttackFinish(){ mState = EObjectState.IDLE; }
	public virtual void OnHit(string hitAnim, BaseObject attacker){ mState = EObjectState.HIT; }
    public virtual void ApplyBufferOnHit(BaseObject attacker) { }
    public virtual void OnHitBuff(BaseObject target, int damage, SKILL_DAMAGE_FLAGS flags) { }
    public virtual void OnDamageBuff(BaseObject attacker, int damage, SKILL_DAMAGE_FLAGS flags) { }
    //public virtual int FinalHit(BaseObject target, int damage) { return 0; }
    //public virtual int FinalDamage(BaseObject attacker, int damage) { return 0; }
	public virtual float HitTime(){ return 2; }

    public virtual bool IsEnemy(BaseObject other) { return other!=this && !other.IsDead() && !IsSameCamp(other); }

    public virtual float LookBounds() { return mLookBounds; }

    public bool InLookBounds(Vector3 pos) 
    {
        return AIKit.InBounds(GetPosition(), pos, mLookBounds);
    }

    public bool InLookBounds(ILocation location)
    {
        return InLookBounds(location.GetPosition());
    }

	public float SkillBound(int skillIndex)
    {
        if (skillIndex > 0)
        {
            float dis = SkillGlobal.GetInfo(skillIndex).distance;//TableManager.GetData("Skill", skillIndex, "ATTACK_DISTANCE");
            if (dis > 0.0001f)
                return dis;
        }

        return 1.0f; 
    }

    public float AttackBound()
    {
        return SkillBound(mAttackSkillIndex);
    }

	public virtual float AttackTime(){ return 2; }

	public virtual bool IsNeedMove(){ return false; }

	public virtual void ResetIdle()
	{
        if (!IsDead())
        {
            mState = EObjectState.IDLE;
            OnIdle();
        }
	}


	public virtual void OnIdle(){}

	public virtual void Relive()
	{
		mState = EObjectState.IDLE;
		mDied = false;
		mHp = GetMaxHp();
		ResetIdle();

		SetVisible(true);
		StartFadeInOrOut(1, true);

		StartAI();
		ShowHp();

        //LuaBehaviour.AddObject(this);
	}

	public virtual void SetVisible(bool bV)
	{
        mbVisible = bV;       

        foreach (Transform form in mMainObject.transform)
        {
            form.gameObject.SetActive(bV);
        }

        //Renderer[] rend = mBodyObject.GetComponentsInChildren<Renderer>();
        //foreach (Renderer mR in rend)
        //{
        //    mR.enabled = bV;
        //}
        
		SetVisibleHp(bV);
	}

	public virtual void SetCastShadow(bool bCast)
	{
        Renderer[] rend = mBodyObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer mR in rend)
		{
			mR.castShadows = bCast;
			mR.receiveShadows = bCast;
		}
	}

    //public virtual float ApplyAffect(AFFECT_TYPE affectType, float fValue) { return fValue;  }
    //public virtual int ApplyAffect(AFFECT_TYPE affectType, int nValue) { return nValue; }


	public virtual Vector3 GetTopPosition()
	{
		return GetPosition();
	}

    // get can move position to target pos at line 
    public bool _GetLineMovePostion(Vector3 targetPos, out Vector3 resultMovePos)
    {
        NavMeshAgent navAgent = mMainObject.GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            NavMeshPath path = new NavMeshPath();
			if (navAgent.CalculatePath(targetPos, path) && path.status != NavMeshPathStatus.PathPartial && path.corners.Length == 2)
            {
				resultMovePos = targetPos;
                return true;
            }
            else
            {               
				int checkCount = (int)( (Vector3.Distance(targetPos, GetPosition())+0.25f)/0.5f - 1);
                if (checkCount > 1)
                {
					Vector3 dir = GetPosition() - targetPos;
                    dir.Normalize();
					float len = 0.0f;
                    for (int i = 0; i < checkCount; ++i)
                    {
						len += 0.5f;
						Vector3 checkPos = targetPos + dir * len;
                        if (navAgent.CalculatePath(checkPos, path) && path.status != NavMeshPathStatus.PathPartial && path.corners.Length == 2)
                        {
                            resultMovePos = checkPos;
                            return true;
                        }
                    }
                }            
				resultMovePos = Vector3.zero;
                return false;
            }
        }
        else
        {
            resultMovePos = targetPos;
            return true;
        }

        //return false;
    }
	//----------------------------------------------
	// Level
	public virtual void SetLevel(int nLevel) { mLevel = nLevel; }
	public virtual int GetLevel() { return mLevel; }
	
	// Star Level
	public int GetStarLevel(){ return TableCommon.GetNumberFromActiveCongfig(mConfigIndex, "STAR_LEVEL");}

	// critical strike rate
	public virtual void SetCriticalStrikeRate(BaseObject target)
	{
		mbIsCriticalStrike = false;
        int iCriticalStrike = (int)((GetCriticalStrikeRate() - target.GetDefenceCriticalStrikeRate()) * 100);
		mbIsCriticalStrike = UnityEngine.Random.Range(0, 100) <= iCriticalStrike;
        //target.mbIsStrickenCriticalStrike = mbIsCriticalStrike;
	}

	// critical strike damage rate
	public float GetCriticalStrikeDamageRate()
	{
		float fCriticalStrikeDamageRate = 1.0f;

		if(mbIsCriticalStrike)
		{
            fCriticalStrikeDamageRate = mConfigRecord.get("CRITICAL_STRIKE_DAMAGE") / 10000f;//GetCriticalStrikeDamage();
		}
		
		return fCriticalStrikeDamageRate;
	}

	// critical strike damage rate
    //public virtual float GetCriticalStrikeDamageDerateRate()
    //{
    //    float fCriticalStrikeDamageDerateRate = 1.0f;
    //    if(mbIsStrickenCriticalStrike)
    //    {
    //        fCriticalStrikeDamageDerateRate = GameCommon.GetCriticalStrikeDamageDerateRate(mConfigRecord, mbIsPvpOpponent);
    //    }

    //    return fCriticalStrikeDamageDerateRate;
    //}

    public virtual float GetCriticalStrikeRate()
    {
        //float value = GetStaticCriticalStrikeRate();
        //float baseValue = GetBaseCriticalStrikeRate();
        return mStaticCriticalStrikeRate + GetBufferAddValue(AFFECT_TYPE.CRITICAL_STRIKE_RATE) / 10000f;
        //return value + GetBufferAddValue(AFFECT_TYPE.CRITICAL_STRIKE) * 0.01f + GetBufferAddValueByRate(baseValue, AFFECT_TYPE.CRITICAL_STRIKE_RATE);
    }

    public virtual float GetBaseCriticalStrikeRate()
    {
        return GameCommon.GetBaseCriticalStrikeRate(mConfigRecord);
    }

    public virtual float GetStaticCriticalStrikeRate()
    {
        //return GetBaseCriticalStrikeRate();
        return GetStaticAttribute(AFFECT_TYPE.CRITICAL_STRIKE_RATE, mBaseCriticalStrikeRate);
    }

    //public virtual float GetCriticalStrikeDamage()
    //{
    //    float value = GetStaticCriticalStrikeDamage();
    //    return value + GetBufferAddValue(AFFECT_TYPE.HIT_CRITICAL_STRIKE_RATE) * 0.01f;
    //}

    //public virtual float GetBaseCriticalStrikeDamage()
    //{
    //    return GameCommon.GetBaseCriticalStrikeDamageRate(mConfigRecord);
    //}

    //public virtual float GetStaticCriticalStrikeDamage()
    //{
    //    return GameCommon.GetCriticalStrikeDamageRate(mConfigRecord, mbIsPvpOpponent);
    //}

	// Attack
    public virtual int GetBaseAttack() 
    { 
        return GameCommon.GetBaseAttack(mConfigRecord, mLevel, 0);
    }

    public virtual int GetStaticAttack() 
    {
        //return GetBaseAttack();
        return (int)GetStaticAttribute(AFFECT_TYPE.ATTACK, mBaseAttack);
    }

    public virtual int GetAttack() 
    {
        //float value = GetStaticAttack();
        //float baseValue = GetBaseAttack();
        return mStaticAttack + (int)GetBufferAddValue(AFFECT_TYPE.ATTACK) + (int)GetBufferAddValueByRate(mStaticAttack, AFFECT_TYPE.ATTACK_RATE);
    }

    //public virtual float GetBaseDefence()
    //{
    //    return GameCommon.GetBaseDefence(mConfigRecord);
    //}

    //public virtual float GetStaticDefence()
    //{
    //    return GetBaseDefence();
    //}

    //// defence
    //public virtual float GetDefence()
    //{
    //    float value = GetStaticDefence();
    //    float baseValue = GetBaseDefence();
    //    return value + GetBufferAddValue(AFFECT_TYPE.DEFENCE) + GetBufferAddValueByRate(baseValue, AFFECT_TYPE.DEFENCE_RATE);
    //}

    // physical defence
    public virtual int GetPhysicalDefence()
    {
        return mStaticPhysicalDefence
            + (int)GetBufferAddValue(AFFECT_TYPE.PHYSICAL_DEFENCE)
            + (int)GetBufferAddValue(AFFECT_TYPE.DEFENCE)
            + (int)GetBufferAddValueByRate(mStaticPhysicalDefence, AFFECT_TYPE.PHYSICAL_DEFENCE_RATE)
            + (int)GetBufferAddValueByRate(mStaticPhysicalDefence, AFFECT_TYPE.DEFENCE_RATE);
    }

    public virtual int GetBasePhysicalDefence()
    {
        return GameCommon.GetBasePhysicalDefence(mConfigRecord, mLevel, 0);
    }

    public virtual int GetStaticPhysicalDefence()
    {
        //return GetBasePhysicalDefence();
        return (int)GetStaticAttribute(AFFECT_TYPE.PHYSICAL_DEFENCE, mBasePhysicalDefence);
    }

    // magic defence
    public virtual int GetMagicDefence()
    {
        return mStaticMagicDefence
            + (int)GetBufferAddValue(AFFECT_TYPE.MAGIC_DEFENCE)
            + (int)GetBufferAddValue(AFFECT_TYPE.DEFENCE)
            + (int)GetBufferAddValueByRate(mStaticMagicDefence, AFFECT_TYPE.MAGIC_DEFENCE_RATE)
            + (int)GetBufferAddValueByRate(mStaticMagicDefence, AFFECT_TYPE.DEFENCE_RATE);
    }

    public virtual int GetBaseMagicDefence()
    {
        return GameCommon.GetBaseMagicDefence(mConfigRecord, mLevel, 0);
    }

    public virtual int GetStaticMagicDefence()
    {
        //return GetBaseMagicDefence();
        return (int)GetStaticAttribute(AFFECT_TYPE.MAGIC_DEFENCE, mBaseMagicDefence);
    }

    public int GetDefenceByDamageType(SKILL_DAMAGE_TYPE damageType)
    {
        if (damageType == SKILL_DAMAGE_TYPE.PHYSICAL)
            return GetPhysicalDefence();
        else if (damageType == SKILL_DAMAGE_TYPE.MAGIC)
            return GetMagicDefence();
        else
            return (GetPhysicalDefence() + GetMagicDefence()) / 2;
    }

    public int GetDefenceByDamageFlags(SKILL_DAMAGE_FLAGS flags)
    {
        return GetDefenceByDamageType(Skill.GetDamageType(flags));
    }

	// final hit rate
	public virtual float GetFinalHitRate(BaseObject targetObject)
	{
		if(targetObject == null)
			return 0f;

		return GetHitRate() - targetObject.GetDodgeRate();
	}

	// hit rate
	public virtual float GetHitRate()
	{
        //float value = GetStaticHitRate();
        //float baseValue = GetBaseHitRate();
        return mStaticHitRate + GetBufferAddValue(AFFECT_TYPE.HIT_TARGET_RATE) / 10000f;
        //return value + GetBufferAddValue(AFFECT_TYPE.HIT_TARGET) * 0.01f + GetBufferAddValueByRate(baseValue, AFFECT_TYPE.HIT_TARGET_RATE);
	}

    public virtual float GetBaseHitRate()
    {
        return GameCommon.GetBaseHitRate(mConfigRecord);
    }

    public virtual float GetStaticHitRate()
    {
        //return GetBaseHitRate();
        return GetStaticAttribute(AFFECT_TYPE.HIT_TARGET_RATE, mBaseHitRate);
    }

	// dodge rate
	public virtual float GetDodgeRate()
	{
        //float value = GetStaticDodgeRate();
        //float baseValue = GetBaseDodgeRate();
        return mStaticDodgeRate + GetBufferAddValue(AFFECT_TYPE.DODGE_RATE) / 10000f;
        //return value + GetBufferAddValue(AFFECT_TYPE.DODGE) * 0.01f + GetBufferAddValueByRate(baseValue, AFFECT_TYPE.DODGE_RATE);
	}

    public virtual float GetBaseDodgeRate()
    {
        return GameCommon.GetBaseDodgeRate(mConfigRecord);
    }

    public virtual float GetStaticDodgeRate()
    {
        //return GetBaseDodgeRate();
        return GetStaticAttribute(AFFECT_TYPE.DODGE_RATE, mBaseDodgeRate);
    }

    public float GetFinalAttackTimeRatio()
    {
        float speed = GetAttackSpeed();
        return 1f / speed;
    }

	// attack speed	
	public virtual float GetAttackSpeed()
	{
        //float value = GetStaticAttackSpeed();
        //float baseValue = GetBaseAttackSpeed();
        float value = mStaticAttackSpeed + GetBufferAddValue(AFFECT_TYPE.ATTACK_SPEED_RATE) / 10000f;
        return Mathf.Max(0.001f, value);
        //return value + GetBufferAddValue(AFFECT_TYPE.ATTACK_SPEED) * 0.01f + GetBufferAddValueByRate(baseValue, AFFECT_TYPE.ATTACK_SPEED_RATE);
	}

    public virtual float GetBaseAttackSpeed()
    {
        return GameCommon.GetBaseAttackSpeed(mConfigRecord);
    }

    public virtual float GetStaticAttackSpeed()
    {
        //return GetBaseAttackSpeed();
        return GetStaticAttribute(AFFECT_TYPE.ATTACK_SPEED_RATE, mBaseAttackSpeed);
    }

    public virtual float GetMoveSpeed()
    {
        //float value = GetStaticMoveSpeed();
        //float baseValue = GetBaseMoveSpeed();
        //float aiSpeed = 0;
        //if (mCurrentAI != null && !mCurrentAI.GetFinished())
        //    aiSpeed = mCurrentAI.AffectSpeed();

        float value = mStaticMoveSpeed + GetBufferAddValue(AFFECT_TYPE.MOVE_SPEED) / 10000f + mAdditionalMoveSpeed;
        return Mathf.Max(0f, value);
        //return value + GetBufferAddValue(AFFECT_TYPE.MOVE_SPEED) * 0.01f + GetBufferAddValueByRate(baseValue, AFFECT_TYPE.MOVE_SPEED_RATE) + mAdditionalMoveSpeed;
    }

    public virtual float GetBaseMoveSpeed()
    {
        return GameCommon.GetBaseMoveSpeed(mConfigRecord);
    }

    public virtual float GetStaticMoveSpeed()
    {
        //return GetBaseMoveSpeed();
        return GetStaticAttribute(AFFECT_TYPE.MOVE_SPEED, mBaseMoveSpeed);
    }

    //public float GetAddMoveSpeedByBuffer()
    //{
    //    return GetMoveSpeed() - GameCommon.GetMoveSpeed(mConfigRecord, mbIsPvpOpponent);
    //}

    private float mAdditionalMoveSpeed = 0f;
 
    // 用于设定在特殊状态下的附加速度
    // 回到正常状态后请不要忘记还原
    // 不支持叠加态
    public void SetAdditionalMoveSpeed(float deltaSpeed)
    {
        mAdditionalMoveSpeed = deltaSpeed;
    }

    public float GetAdditionalMoveSpeed()
    {
        return mAdditionalMoveSpeed;
    }

	// damege mitigation rate
	public virtual float GetDamageMitigationRate()
	{
        //float value = GetStaticDamegeMitigationRate();
        return mStaticDamageMitigationRate + GetBufferAddValue(AFFECT_TYPE.DEFENCE_HIT_RATE) / 10000f;
	}

    public virtual float GetBaseDamageMitigationRate()
    {
        return GameCommon.GetBaseDamageMitigationRate(mConfigRecord);
    }

    public virtual float GetStaticDamageMitigationRate()
    {
        //return GetBaseDamageMitigationRate();
        return GetStaticAttribute(AFFECT_TYPE.DEFENCE_HIT_RATE, mBaseDamageMitigationRate);
    }

    // damage enhance rate
    public virtual float GetDamageEnhanceRate()
    {
        return mStaticDamageEnhanceRate + GetBufferAddValue(AFFECT_TYPE.HIT_RATE) / 10000f;
    }

    public virtual float GetBaseDamageEnhanceRate()
    {
        return GameCommon.GetBaseDamageEnhanceRate(mConfigRecord);
    }

    public virtual float GetStaticDamageEnhanceRate()
    {
        //return GetBaseDamageEnhanceRate();
        return GetStaticAttribute(AFFECT_TYPE.HIT_RATE, mBaseDamageEnhanceRate);
    }


    public virtual float GetDefenceCriticalStrikeRate()
    {
        return mStaticDefenceCriticalStrikeRate + GetBufferAddValue(AFFECT_TYPE.DEFENCE_CRITICAL_STRIKE_RATE) / 10000f;
    }

    public virtual float GetBaseDefenceCriticalStrikeRate()
    {
        return GameCommon.GetBaseDefenceCriticalStrikeRate(mConfigRecord);
    }

    public virtual float GetStaticDefenceCriticalStrikeRate()
    {
        //return GetBaseDefenceCriticalStrikeRate();
        return GetStaticAttribute(AFFECT_TYPE.DEFENCE_CRITICAL_STRIKE_RATE, mBaseDefenceCriticalStrikeRate);
    }

	//element reduction rate
    //public virtual float GetElementReductionRate(int elementType)
    //{
    //    if(elementType >= (int)ELEMENT_TYPE.MAX)
    //        return 0;

    //    float value = GetStaticElementReductionRate(elementType);
    //    return value + GetBufferAddValue((AFFECT_TYPE)(elementType + (int)AFFECT_TYPE.RED_REDUCTION_RATE)) * 0.01f;
    //}
	
    //public virtual float GetBaseElementReductionRate(int elementType)
    //{
    //    return GameCommon.GetBaseElementReductionRate(mConfigRecord, elementType);
    //}
	
    //public virtual float GetStaticElementReductionRate(int elementType)
    //{
    //    return GameCommon.GetElementReductionRate(mConfigRecord, elementType, mbIsPvpOpponent);
    //}

    //public virtual float GetDefenceCharmRate()
    //{
    //    float value = GetStaticDefenceCharmRate();
    //    return value + GetBufferAddValue(AFFECT_TYPE.DEFENCE_CHARM_RATE) * 0.01f;
    //}

    //public virtual float GetBaseDefenceCharmRate()
    //{
    //    return GameCommon.GetBaseDefenceCharmRate(mConfigRecord);
    //}

    //public virtual float GetStaticDefenceCharmRate()
    //{
    //    return GameCommon.GetDefenceCharmRate(mConfigRecord, mbIsPvpOpponent);
    //}

    //public virtual float GetDefenceFearRate()
    //{
    //    float value = GetStaticDefenceFearRate();
    //    return value + GetBufferAddValue(AFFECT_TYPE.DEFENCE_FEAR_RATE) * 0.01f;
    //}

    //public virtual float GetBaseDefenceFearRate()
    //{
    //    return GameCommon.GetBaseDefenceFearRate(mConfigRecord);
    //}

    //public virtual float GetStaticDefenceFearRate()
    //{
    //    return GameCommon.GetDefenceFearRate(mConfigRecord, mbIsPvpOpponent);
    //}

    //public virtual float GetDefenceBeatBackRate()
    //{
    //    float value = GetStaticDefenceBeatBackRate();
    //    return value + GetBufferAddValue(AFFECT_TYPE.DEFENCE_BEATBACK_RATE) * 0.01f;
    //}

    //public virtual float GetBaseDefenceBeatBackRate()
    //{
    //    return GameCommon.GetBaseDefenceBeatBackRate(mConfigRecord);
    //}

    //public virtual float GetStaticDefenceBeatBackRate()
    //{
    //    return GameCommon.GetDefenceBeatBackRate(mConfigRecord, mbIsPvpOpponent);
    //}

    public virtual float GetDefenceIceRate()
    {
        //float value = GetStaticDefenceIceRate();
        return mStaticDefenceIceRate + GetBufferAddValue(AFFECT_TYPE.DEFENCE_ICE_RATE) / 10000f;
    }

    public virtual float GetBaseDefenceIceRate()
    {
        return GameCommon.GetBaseDefenceIceRate(mConfigRecord);
    }

    public virtual float GetStaticDefenceIceRate()
    {
        //return GetBaseDefenceIceRate();
        return GetStaticAttribute(AFFECT_TYPE.DEFENCE_ICE_RATE, mBaseDefenceIceRate);
    }

    //public virtual float GetDefenceDownRate()
    //{
    //    float value = GetStaticDefenceDownRate();
    //    return value + GetBufferAddValue(AFFECT_TYPE.DEFENCE_DOWN_RATE) * 0.01f;
    //}

    //public virtual float GetBaseDefenceDownRate()
    //{
    //    return GameCommon.GetBaseDefenceDownRate(mConfigRecord);
    //}

    //public virtual float GetStaticDefenceDownRate()
    //{
    //    return GameCommon.GetDefenceDownRate(mConfigRecord, mbIsPvpOpponent);
    //}

    public virtual float GetDefenceWoozyRate()
    {
        //float value = GetStaticDefenceWoozyRate();
        return mStaticDefenceWoozyRate + GetBufferAddValue(AFFECT_TYPE.DEFENCE_WOOZY_RATE) / 10000f;
    }

    public virtual float GetBaseDefenceWoozyRate()
    {
        return GameCommon.GetBaseDefenceWoozyRate(mConfigRecord);
    }

    public virtual float GetStaticDefenceWoozyRate()
    {
        //return GetBaseDefenceWoozyRate();
        return GetStaticAttribute(AFFECT_TYPE.DEFENCE_WOOZY_RATE, mBaseDefenceWoozyRate);
    }

    public virtual float GetBloodSuckingRate()
    {
        return mStaticBloodSuckingRate + GetBufferAddValue(AFFECT_TYPE.BLOOD_SUCKING) / 10000f;
    }

    public virtual float GetBaseBloodSuckingRate()
    {
        return GameCommon.GetBaseBloodSuckingRate(mConfigRecord);
    }

    public virtual float GetStaticBloodSuckingRate()
    {
        //return GetBaseBloodSuckingRate();
        return GetStaticAttribute(AFFECT_TYPE.BLOOD_SUCKING, mBaseBloodSuckingRate);
    }

    public virtual float GetDamageReboundRate()
    {
        return mStaticDamageReboundRate + GetBufferAddValue(AFFECT_TYPE.DAMAGE_REBOUND_RATE) / 10000f;
    }

    public virtual float GetBaseDamageReboundRate()
    {
        return GameCommon.GetBaseDamageReboundRate(mConfigRecord);
    }

    public virtual float GetStaticDamageReboundRate()
    {
        //return GetBaseDamageReboundRate();
        return GetStaticAttribute(AFFECT_TYPE.DAMAGE_REBOUND_RATE, mBaseDamageReboundRate);
    }

    //public virtual float GetAutoHp()
    //{
    //    float value = GetStaticAutoHp();
    //    return value + GetBufferAddValue(AFFECT_TYPE.AUTO_HP);
    //}

    //public virtual float GetBaseAutoHp()
    //{
    //    return GameCommon.GetBaseAutoHp(mConfigIndex);
    //}

    //public virtual float GetStaticAutoHp()
    //{
    //    return GameCommon.GetAutoHp(mConfigIndex, mbIsPvpOpponent);
    //}

    //public virtual float GetAutoMp()
    //{
    //    float value = GetStaticAutoMp();
    //    return value + GetBufferAddValue(AFFECT_TYPE.AUTO_MP);
    //}

    //public virtual float GetBaseAutoMp()
    //{
    //    return GameCommon.GetBaseAutoMp(mConfigIndex);
    //}

    //public virtual float GetStaticAutoMp()
    //{
    //    return GameCommon.GetAutoMp(mConfigIndex, mbIsPvpOpponent);
    //}

    public virtual bool IsSilence()
    {
        return mAffectPool.GetValue("SILENCE") > 0.001d;
    }

	// HP
	public void SetHp(int nHp){ ChangeHp(nHp-mHp); }
	public int GetHp(){ return mHp; }
    public float GetHpRate() { return (float)mHp / GetMaxHp(); }

	public virtual void ChangeHp(int nHp)
	{
		if(!mDied && !mbIsUI)
		{
			mHp += nHp;

            if (mImmortal)
            {
                mHp = Mathf.Max(mHp, 1);
            }

			if (mHp<=0)
			{
				mHp = 0;
				if (!mDied)
				{
					mDied = true;
					OnDead();
				}
			}
			else if(mHp>GetMaxHp())
				mHp = GetMaxHp();
			
			ShowHp();
		}
	}

    public virtual void Die()
    {
        if (mDied)
            return;

        mHp = 0;
        mDied = true;
        OnDead();
        ShowHp();
    }

	public virtual int GetMaxHp()
    {
        int value = mStaticMaxHp + (int)GetBufferAddValue(AFFECT_TYPE.HP_MAX) + (int)GetBufferAddValueByRate(mStaticMaxHp, AFFECT_TYPE.HP_MAX_RATE);
        return Mathf.Max(1, value);
    }

    public virtual int GetBaseMaxHp() 
    {
        return GameCommon.GetBaseMaxHP(mConfigRecord, mLevel, 0); 
    }

    public virtual int GetStaticMaxHp() 
    {
        //return GetBaseMaxHp();
        return (int)GetStaticAttribute(AFFECT_TYPE.HP_MAX, mBaseMaxHp);
    }

	public virtual void SetAttributeByLevel()
	{
        if (!mIsAttributeLocked)
        {
            InitBaseAttribute();
            InitStaticAttribute();
        }

		SetHp(GetMaxHp());
	}

    public virtual float GetBufferAddValue(AFFECT_TYPE type) { return 0f; }

    public virtual AffectBuffer GetBufferByIndex(int index) { return null; }

    //public virtual void NotifyBufferFinished(AffectBuffer buffer) { }

	public virtual Renderer[] GetRenderers()
	{ 
		if(!IsDead () && mMainObject != null )
            return mBodyObject.GetComponentsInChildren<Renderer>();
		else 
			return null;
	}

    public float GetBufferAddValueByRate(float baseValue, AFFECT_TYPE rateType)
    {
        float rate = GetBufferAddValue(rateType);
        return baseValue * (1f + rate / 10000f) - baseValue;
    }

    //public int GetBufferAddValueByRate(int baseValue, AFFECT_TYPE rateType)
    //{
    //    float rate = GetBufferAddValue(rateType);
    //    return (int)(baseValue * (1f + rate * 0.01f) - baseValue);
    //}
    //
    //public float GetBufferAddValue(float baseValue, AFFECT_TYPE addType, AFFECT_TYPE rateType)
    //{
    //    float addValue = GetBufferAddValue(addType);
    //    float rateValue = GetBufferAddValue(rateType);
    //    return baseValue * (1f + rateValue * 0.01f) + addValue - baseValue;
    //}
    //
    //public int GetBufferAddValue(int baseValue, AFFECT_TYPE addType, AFFECT_TYPE rateType)
    //{
    //    float addValue = GetBufferAddValue(addType);
    //    float rateValue = GetBufferAddValue(rateType);
    //    return (int)(baseValue * (1f + rateValue * 0.01f) + addValue) - baseValue;
    //}

    public float GetDefenceRate(BUFFER_TYPE bufferType)
    {
        switch (bufferType)
        {
            case BUFFER_TYPE.HoldBuffer:
                return GetDefenceWoozyRate();

            case BUFFER_TYPE.IceBuffer:
                return GetDefenceIceRate();

            default:
                return 0f;
        }
    }

    public bool CheckCanBeApplyAffect(BUFFER_TYPE bufferType)
    {
        if (bufferType == BUFFER_TYPE.CharmBuffer)
        {
            return !isCampChanged;
        }
        else
        {
            return UnityEngine.Random.value >= GetDefenceRate(bufferType);
        }
    }

    public bool CheckCanBeApplyAffect(int configIndex)
    {
        BUFFER_TYPE bufferType = AffectBuffer.GetBufferType(configIndex);
        return CheckCanBeApplyAffect(bufferType);
    }

    public bool CheckCanHitTarget(BaseObject target)
    {
        float rate = GetFinalHitRate(target);
        return UnityEngine.Random.value < rate;
    }

    //public bool CheckCanBeBeatBack()
    //{
    //    return UnityEngine.Random.value >= GetDefenceBeatBackRate();
    //}

    public virtual void StopAllBuffer()
    { }

#region Methods for AI routines

    public Skill DoSkillIndependent(int skillIndex, BaseObject target, int skillLevel, tLogicData extData, Action onApplyFinish)
    {
        DataRecord config = DataCenter.mSkillConfigTable.GetRecord(skillIndex);

        if (config == null || !CanDoSkill(config))
        {
            DEBUG.LogError("Skill " + skillIndex + " has no config or can't do!");
            return null;
        }

        //if (!CanDoSkill(config) || (mCurrentAI != null && mCurrentAI.HoldControl()))
        //    return null;

        string skillTypeName = config.getData("TYPE");
        Skill skill = (ObjectAI)Logic.EventCenter.Self.StartEvent(skillTypeName) as Skill;
        skill.SetOwner(this);
        skill.mIndependent = true;
        skill.mLevel = skillLevel;
        skill.mOnApplyFinish += onApplyFinish;

        if (skill != null)
        {
            if (skill.Init(this, target, skillIndex))
            {
                skill.mLogicData = extData;
                skill.DoEvent();

                if (skillIndex == mAttackSkillIndex)
                {
                    OnAttack(target);
                }
                else
                {
                    OnDoSkill(target, skillIndex);
                }

                return skill;
            }
        }

        return null;
    }

    // 找寻优先级最高的敌人
    public virtual BaseObject SearchEnemyByDefaultStrategy()
    {
        return aiMachine.searchTargetCenter.FindNearestEnemy(aiMachine.searchTargetInGlobal);
    }

    public IEnumerable<SkillCDLooper> CoolDownedSkillCDLoopers()
    {
        if (cdLoopers != null)
        {
            foreach (var cd in cdLoopers)
            {
                if (cd.isCoolDown)
                {
                    yield return cd;
                }
            }
        }
    }

    public IEnumerable<SkillCDLooper> AllSkillCDLoopers()
    {
        if (cdLoopers != null)
        {
            foreach (var cd in cdLoopers)
            {
                yield return cd;
            }
        }
    }

    //public IRoutine aiRoutine = null;
    private AIMachine mAIMachine = null;

    public virtual AIMachine CreateAIMachine()
    {
        return new AIMachine(this);
    }

    public AIMachine aiMachine
    {
        get 
        {
            if (mAIMachine == null)
            {
                mAIMachine = CreateAIMachine();
            }

            return mAIMachine;
        }
    }

    //public void StartAIRoutine(IRoutine ai)
    //{
    //    StopAIRoutine();
    //    aiRoutine = ai;
    //    StartRoutine(aiRoutine);
    //}

    //public void StopAIRoutine()
    //{
    //    if (Routine.IsActive(aiRoutine))
    //    {
    //        aiRoutine.Break();
    //    }
    //}

    public IRoutine StartRoutine(IEnumerator routine)
    {
        if (mMainObject != null)
        {
            return mMainObject.StartRoutine(routine);
        }

        return null;
    }

    public void BreakAllRoutines()
    {
        if (mMainObject != null)
        {
            mMainObject.BreakAllRoutines();
        }
    }

    //public void BroadcastRoutines(string msg, object arg)
    //{
    //    if (mMainObject != null)
    //    {
    //        mMainObject.BroadcastRoutines(msg, arg);
    //    }
    //}

    private int stopMoveFrame = -1;

    public void WillStopMove()
    {
        stopMoveFrame = Time.frameCount + 1;
    }

    public bool standStill { get; private set; }

    public void SetStandStill(bool standStill)
    {
        this.standStill = standStill;

        if (standStill)
        {
            SetAnimSpeed(0f);
            StopMove();
        }
        else 
        {
            SetAnimSpeed(1f);
        }
    }

    private Vector3 prePosition = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    //public BaseObject mLockedTarget;

    private List<SkillCDLooper> cdLoopers = null;

    public void ActivateCDLooper(int skillIndex, SKILL_POS skillPos, int skillLevel, float initCD)
    {
        if (cdLoopers == null)
        {
            cdLoopers = new List<SkillCDLooper>();
        }

        SkillCDLooper looper = new SkillCDLooper(skillIndex);
        looper.skillPos = skillPos;
        looper.skillLevel = skillLevel;
        StartRoutine(looper);
        cdLoopers.Add(looper);

        if (initCD > 0.001f)
        {
            looper.Restart();
            looper.rate = 1f - initCD / looper.cdTime;
        }

        looper.paused = this is Monster && MainProcess.mStage != null && MainProcess.mStage.isMonsterSkillCDPaused;
        aiMachine.skillIntentQueue.resetFlag = true;
    }

    public bool IsSkillCooldown(int skillIndex)
    {
        if (cdLoopers == null)
            return true;

        var looper = cdLoopers.Find(x => Routine.IsActive(x) && x.skillIndex == skillIndex && x.isCoolDown);
        return looper == null;
    }

    public void RestartSkillCD(int skillIndex)
    {
        if (cdLoopers == null)
            return;

        var looper = cdLoopers.Find(x => Routine.IsActive(x) && x.skillIndex == skillIndex);

        if (looper != null)
        {
            looper.Restart();
        }
    }

    public void RestartSkillCDByUsePos(int usePos)
    {
        if (cdLoopers == null)
            return;

        var looper = cdLoopers.Find(x => Routine.IsActive(x) && (int)x.skillPos == usePos);

        if (looper != null)
        {
            looper.Restart();
        }
    }

    // 暂停技能CD，如果该技能CD有UI表现，需要手动暂停UI的CD转圈
    public void PauseSkillCD()
    {
        if (cdLoopers != null)
        {
            cdLoopers.ForEach(x => x.paused = true);
        }
    }

    public void ResumeSkillCD()
    {
        if (cdLoopers != null)
        {
            cdLoopers.ForEach(x => x.paused = false);
        }
    }

    public virtual void SetClampMoveSpeed(float clampSpeed) { }

    public int mFollowPos = -1;
    public Vector3 mFollowOffset = Vector3.back * AIParams.followRadius;

    private HashSet<int> mLockedFollowPosPool = new HashSet<int>();

    //public IEnumerable<int> allFreeFollowPos 
    //{
    //    get 
    //    {
    //        foreach (int i in mFreeFollowPosPool)
    //        {
    //            yield return i;
    //        }
    //    }
    //}
    public int GetFreeFollowPos(int minPos)
    {
        int i = minPos + 1;

        while (mLockedFollowPosPool.Contains(i))
        {
            ++i;
        }

        return i;
    }

    public void SetFollowPosLocked(int pos, bool locked)
    {
        if (locked)
        {
            mLockedFollowPosPool.Add(pos);
        }
        else
        {
            mLockedFollowPosPool.Remove(pos);
        }
    }

    private float mAliveStartTime = -1f;
    private float mLifeTime = Mathf.Infinity;

    public float aliveTime
    {
        get 
        {
            return mAliveStartTime < 0 ? 0f : Time.time - mAliveStartTime;
        }
    }

    public float lifeTime
    {
        get 
        {
            return mLifeTime;
        }
        set 
        {
            mLifeTime = value;

            if (aliveTime >= mLifeTime)
            {
                ChangeHp(-99999999);
                OnLifeTimeOver();
            }
        }
    }

    protected virtual void OnLifeTimeOver() { }


    /// <summary>
    /// 当目标被锁定或解锁时的回调
    /// </summary>
    /// <param name="enemy"> 被锁定的目标，若为null表示解除锁定 </param>
    public virtual void OnLockEnemy(BaseObject enemy) { }

    // fieldIndex = 0, 1, 2, 3
    public virtual int GetConfigSkillLevel(int fieldIndex) { return 1; }

    public virtual int GetAttackSkillLevel() { return 1; }

#endregion

    public void IncreaseSkillCount(int skillIndex)
    {
        int current = 0;

        if (mSkillCounter.TryGetValue(skillIndex, out current))
        {
            mSkillCounter[skillIndex] = current + 1;
        }
        else 
        {
            mSkillCounter.Add(skillIndex, 1);
        }
    }

    public BaseObject FindNearestAlived(Predicate<BaseObject> match)
    {
        var objMap = ObjectManager.Self.mObjectMap;
        float minSqrDist = Mathf.Infinity;
        BaseObject target = null;
        float tempSqrDist;

        for (int i = objMap.Length - 1; i >= 0; --i)
        {
            var obj = objMap[i];

            if (obj != null && obj != this && !obj.IsDead() && (match == null || match(obj)))
            {
                tempSqrDist = AIKit.SqrDistance(GetPosition(), obj.GetPosition());

                if (tempSqrDist < minSqrDist)
                {
                    minSqrDist = tempSqrDist;
                    target = obj;
                }
            }
        }

        return target;
    }
}


//-----------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------

public abstract class ObjectFactory
{    
	public abstract OBJECT_TYPE GetObjectType();
	public abstract BaseObject NewObject();
	
	public virtual string GetConfigTableFile() { return null; }

    public virtual NiceTable GetConfigTable() { return DataCenter.mActiveConfigTable; }
	//-----------------------------------------------------------------------------------
    //public NiceTable mConfigTable;

    //public virtual void LoadConfigTable()
    //{
    //    string tableFile = GetConfigTableFile();
    //    if (null != tableFile)
    //    {
    //        //string configFile = GameCommon.MakeGamePathFileName(tableFile);
    //        //if (!mConfigTable.LoadTable(configFile))
    //        //if (!GameCommon.LoadTableOnUnity(tableFile, out mConfigTable))
    //        mConfigTable = GameCommon.LoadTable(tableFile, LOAD_MODE.RESOURCE);
    //        if (mConfigTable==null)
    //        {
    //            string configFile = GameCommon.MakeGamePathFileName(tableFile);
    //            EventCenter.Self.Log("Fail >>> " + configFile);
    //            throw new Exception("Error: load config table fail. >>>" + configFile);
    //        }
    //    }
    //}
	
	public virtual DataRecord GetConfig(int configIndex)
	{
        //lhcworldmap
        DataRecord record=GetConfigTable().GetRecord(configIndex);

        //DataRecord record=DataCenter.mBossConfig.GetRecord(configIndex);
		if (null == record)
		{
			EventCenter.Self.Log(" Error: check table [" + GetConfigTableFile() + "], no exist crop config >>>" + configIndex.ToString());
		}
		return record;
	}
	
}

//---------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------
public class ResourcesManger
{
	static public Texture FindTexture(string name)
	{
		Texture[] tex = Resources.FindObjectsOfTypeAll<Texture>();

		foreach(Texture t in tex)
		{
			if (t.name==name)
				return t;
		}
		return null;
	}
}

//---------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------

