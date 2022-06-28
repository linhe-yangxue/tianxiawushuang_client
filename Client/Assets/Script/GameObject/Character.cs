//#define DEBUG_SKILL
//#define  SHOW_FOLLOW_POSITION
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataTable;
using Logic;
using Utilities;

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
public class Role : ActiveObject
{
    public ObjectAI mMoveAI;
	public string mBattlePlayerWindow = "BATTLE_PLAYER_WINDOW";
    //public int mSmallSkillIndex = 0;

    public ActiveData mActiveData = null;
    public int mBreakLevel = 0;

	public Role()
	{
		mLevel = 1;

        mMoveAI = Logic.EventCenter.Start("AI_MoveTo") as ObjectAI;
        mMoveAI.SetOwner(this);
        mNeedVerifyMD5 = true;
	}

    //public override bool Init(int configIndex)
    //{
    //    if (base.Init(configIndex))
    //    {
    //        mSmallSkillIndex = GetConfig("PET_SKILL_2");
    //        return true;
    //    }
    //    else 
    //    {
    //        return false;
    //    }
    //}

    public override void Start()
    {       
        base.Start();

        if (MainProcess.mStage != null && (GetObjectType() == OBJECT_TYPE.CHARATOR || GetObjectType() == OBJECT_TYPE.PET))
        {
            int teamBuffer = MainProcess.mStage.mTeamBuffer;
            int teamExtraBuffer = MainProcess.mStage.mTeamExtraBuffer;
            int equipSetBuffer = MainProcess.mStage.mEquipSetBuffer;
			int consumeItemBuffer = MainProcess.mStage.mConsumeItemBuffer;

            if (teamBuffer > 0)
                StartAffect(this, teamBuffer);

            if (teamExtraBuffer > 0)
                StartAffect(this, teamExtraBuffer);

            if (equipSetBuffer > 0)
                StartAffect(this, equipSetBuffer);

			if (consumeItemBuffer > 0)
				StartAffect(this, consumeItemBuffer);

			//BOSS战Buff
//            int bossNormalAttackBuff = 0;
//            int bossPowerAttackBuff = 0;

//            DataCenter.Self.get("IS_BOSS_NORMAL_ATTACK", out bossNormalAttackBuff);
//            DataCenter.Self.get("IS_BOSS_POWER_ATTACK", out bossPowerAttackBuff);

////			bossNormalAttackBuff = MainProcess.mStage.mBossNormalAttack;
////			bossPowerAttackBuff = MainProcess.mStage.mBossPowerAttack;

//            if(bossNormalAttackBuff > 0)
//                StartAffect(this, bossNormalAttackBuff);
//            if(bossPowerAttackBuff > 0)
//                StartAffect(this, bossPowerAttackBuff);

            //by chenliang
            //begin

            //群魔乱舞Buff只在爬塔战斗有用
            object tmpObjRammbockStartClimb = DataCenter.Self.getObject("RAMMBOCK_START_CLIMB");
            if(tmpObjRammbockStartClimb != null && (bool)tmpObjRammbockStartClimb == true)
            {
                //群魔乱舞战斗Buff
                RammbockWindow winRammbock = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
                if (winRammbock != null && winRammbock.m_climbingInfo != null)
                {
                    List<int> listRammbockBuff = winRammbock.m_climbingInfo.buffList;
                    if (listRammbockBuff != null && listRammbockBuff.Count > 0)
                    {
                        for (int i = 0, count = listRammbockBuff.Count; i < count; i++)
                            StartAffect(this, listRammbockBuff[i]);
                    }
                }
            }

            //end
        }

        //if (mSmallSkillIndex > 0)
        //{
        //    ActivateCDLooper(mSmallSkillIndex, -1);
        //}
    }

    public override int GetConfigSkillLevel(int fieldIndex)
    {
        if (fieldIndex >= 0 && mActiveData != null && mActiveData.skillLevel != null && mActiveData.skillLevel.Length > fieldIndex)
        {
            return mActiveData.skillLevel[fieldIndex];
        }

        return 1;
    }

    protected override void InitSkillOnStart(int fieldIndex, int skillIndex)
    {
        if (fieldIndex > 0 && IsSKillOpened(fieldIndex))
        {
            ActivateCDLooper(skillIndex, SKILL_POS.SPECIAL_ATTACK, GetConfigSkillLevel(fieldIndex), 0f);
        }
    }

    protected override void InitBuffOnStart(int fieldIndex, int buffIndex)
    {
        if (IsSKillOpened(fieldIndex))
        {
            StartAffect(this, buffIndex, GetConfigSkillLevel(fieldIndex));
        }
    }

    // 检测技能是否开启
    // 技能按照配置的突破等级开启
    protected bool IsSKillOpened(int fieldIndex)
    {
        if (mActiveData != null)
        {
            return mActiveData.breakLevel >= (int)mConfigRecord["SKILL_ACTIVE_LEVEL_" + (fieldIndex + 1)];
        }

        return true;
    }

    public virtual ObjectAI StartMoveAI()
    {
        if (mCurrentAI!=mMoveAI)
            StopAI();
        mMoveAI.SetFinished(false);
        mCurrentAI = mMoveAI;
        return mMoveAI;
    }

    //public override float GetMoveSpeed()
    //{
    //    return (AIKit.InPVE() && PVEStageBattle.mBattleControl != BATTLE_CONTROL.MANUAL) ? base.GetMoveSpeed() * 1.5f : base.GetMoveSpeed();
    //}

    public virtual void SetActiveData(ActiveData activeData)
    {
        mActiveData = activeData;

        if (activeData != null && activeData.teamPos >= 0 && activeData.teamPos <= 3 && (GetObjectType() == OBJECT_TYPE.CHARATOR || GetObjectType() == OBJECT_TYPE.PET))
        {
            mAffect = AffectFunc.GetPetAllAffect(activeData.teamPos);

            // AffectFunc.GetPetAllAffect接口中包含了光环效果
            // mAffect.Append(BreakAffect.GetAuraBreakAffect());         
        }

        SetLevel(activeData.level);
        SetBreakLevel(activeData.breakLevel);

        SetAttributeByLevel();

        //if (MainProcess.mStage != null && MainProcess.mStage.IsBOSS())
        //{
        //    mStaticAttack *= activeData.breakLevel + 1;
        //}
    }

    public void SetBreakLevel(int breakLevel)
    {
        mBreakLevel = breakLevel;
    }

	//public override BaseObject FindNearestEnemy ()
	//{
	//	if(MainProcess.mStage is PVP4Battle)
	//	{
	//		foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
	//		{
	//			if (obj!=null && IsEnemy(obj) && obj is OpponentCharacter)
	//			{
	//				return obj;
	//			}
	//		}        
	//	}
    //
	//	return base.FindNearestEnemy ();
	//}

    public override int GetBaseMaxHp()
    {
        return GameCommon.GetBaseMaxHP(mConfigRecord, mLevel, mBreakLevel);
    }

    public override int GetBaseAttack()
    {
        return GameCommon.GetBaseAttack(mConfigRecord, mLevel, mBreakLevel);
    }

    public override int GetBasePhysicalDefence()
    {
        return GameCommon.GetBasePhysicalDefence(mConfigRecord, mLevel, mBreakLevel);
    }

    public override int GetBaseMagicDefence()
    {
        return GameCommon.GetBaseMagicDefence(mConfigRecord, mLevel, mBreakLevel);
    }

    public override AIMachine CreateAIMachine()
    {
        return new RoleAIMachine(this);
    }
}

public class Character : Role
{
	static public Character Self;

	static public int msFriendsCount = 4;

	public Transform mHandBoneForm;

	public BaseObject[] mFriends = new BaseObject[msFriendsCount];

    public AutoBattleAI mCurrentAutoTargetAI;

    private int _mMp = 0;
    //private byte[] _mMpMD5;
    private GameMD5 _mMpMD5 = new GameMD5();

	public int mExp = 0;
	public int mTotalAddExp = 0;

	public int mMaxLevel = 40;

	public int mGold = 0;
    public int mUIGold = 0;

    public int mUIItem = 0;

    public int mKillMonster = 0;

	public float mDTime = 0;

    public AutoBattleAI mAutoBattleAI;

    public BaseEffect mBoundEffect;

	public ObjectAI mCharMoveAI;

    //public float mLookRange = 8;

    public BaseEffect mRunEffect;

    public ActiveObject mLockEnemy = null;

    //public int mMainSkillIndex = 0;

    public int mBaseMaxMp = 0;                      // 基础最大MP值

    public int mStaticMaxMp = 0;                    // 静态最大MP值

    public event Action onPetsCreateDone = null;
    public bool petsCreateDone { get; private set; }
    public int createdPetCount { get; private set; }
	
#if SHOW_FOLLOW_POSITION
    GameObject mFollowPosFlag;
#endif

    public int mMp
    {
        get 
        {
            return _mMp;
        }

        set
        {
            if (_mMp != value)
            {
                if (mNeedVerifyMD5 && CommonParam.IsVerifyNumberValue)
                {
                    //byte[] hash = MD5Tools.GetMD5(_mMp);

                    //if (_mMpMD5 == null || !hash.SequenceEqual(_mMpMD5))
                    //{
                    //    MainProcess.OnCheatVerifyFailed();
                    //}
                  
                    //_mMpMD5 = MD5Tools.GetMD5(value);
                    if (_mMpMD5.CalcMD5(_mMp) != _mMpMD5.md5)
                    {                      
                        MainProcess.OnCheatVerifyFailed();
                    }

                    _mMpMD5.md5 = value.ToString();
                }

                _mMp = value;
            }
        }
    }
	
	public Character()
	{
		Self = this;
#if SHOW_FOLLOW_POSITION
        GameObject m = GameCommon.LoadPrefabs("prefabs/MouseCoord");
#endif
        //SetAttributeByLevel();
        //_mMpMD5 = MD5Tools.GetMD5(_mMp);
        _mMpMD5.md5 = _mMp.ToString();
	}

	public Character(bool isOpponent)
    {
        //_mMpMD5 = MD5Tools.GetMD5(_mMp);
        _mMpMD5.md5 = _mMp.ToString();
    }

    //public override bool Init (int configIndex)
    //{		
    //    //GameCommon.JudgeRestoreCharacterConfigRecord();
        
    //    if (base.Init(configIndex))
    //    {
    //        mMainSkillIndex = GetConfig("PET_SKILL_1");
    //        return true;
    //    }
    //    else 
    //    {
    //        return false;
    //    }
    //}

    public override void InitBaseAttribute()
    {
        base.InitBaseAttribute();
        mBaseMaxMp = GetBaseMaxMp();
    }

    public override void InitStaticAttribute()
    {
        base.InitStaticAttribute();
        mStaticMaxMp = GetStaticMaxMp();
    }

    public override void CloneAttributeFrom(BaseObject cloneFrom)
    {
        base.CloneAttributeFrom(cloneFrom);

        var ch = cloneFrom as Character;

        if (ch != null)
        {
            mBaseMaxMp = ch.mBaseMaxMp;
            mStaticMaxMp = ch.mStaticMaxMp;
        }        
    }

	public override void OnInitModelObject()
	{
		base.OnInitModelObject();  

        NavMeshAgent nav = mMainObject.GetComponent<NavMeshAgent>();
        nav.avoidancePriority = 48;
        //nav.radius = 0.01f;
        //nav.radius = AIParams.characterNavRadius;

		GameObject wep = GameCommon.FindObject(mMainObject, "Bone_wap");
		if (wep!=null)
			mHandBoneForm = wep.transform;

        if (mBoundEffect != null)
            mBoundEffect.Finish();

        if (mMainObject != null)
            mMainObject.name = "_MainCharacter_";

		mCharMoveAI = Logic.EventCenter.Start("AI_MainCharMove") as ObjectAI;
		mCharMoveAI.SetOwner(this);

	}

    public override void OnDestroy()
    {
		DataCenter.SetData(mBattlePlayerWindow, "CLEAR_ALL_BUFF", true);
        if (mBoundEffect != null)
        {
            mBoundEffect.Finish();
            mBoundEffect = null;
        }
        base.OnDestroy();
    }

    public override void InitShadow()
    {
        //if (!GameCommon.CheckNeedShowAureole(mConfigIndex)) 
        //{
        //    CreateShadow("textures/char_shadow", 1.5f/*mBaseSelectRadius * CommonParam.shadowRadiusScale*/, 0.85f);        
        //}
        base.InitShadow();

        if (GetObjectType() == OBJECT_TYPE.CHARATOR && mBoundEffect == null)
            mBoundEffect = BaseEffect.CreateEffect( this, null, 3005 );

    }
#if DEBUG_SKILL
    public override ObjectAI StartAI(string aiName)
    {
        string info = "NONE";
        if (mCurrentAI != null)
            info = mCurrentAI.GetEventName();
        ObjectAI ai = base.StartAI(aiName);
        if (ai != null)
        {
            info += " -> " + ai.GetEventName();
        }
        else
            info += " ->O (stop)";
        Logic.EventCenter.Log(LOG_LEVEL.GENERAL, info);
        return ai;
    }
#endif
	// Use this for initialization
	public override void Start () 
	{
        mRunEffect = PlayEffect(9025, null);
        mRunEffect.Show(false);

        //mLookRange = GetConfig("LOOKBOUND");

        CameraMoveEvent.BindMainObject(this);

        if (mShowHpBar != null)
            mShowHpBar.Init(msHpBarWidth, msHpBarHeight, Color.green, Color.yellow);

        mAutoBattleAI = Logic.EventCenter.Start("AutoBattleAI") as AutoBattleAI;

		base.Start ();

        //if (mMainSkillIndex > 0)
        //{
        //    ActivateCDLooper(mMainSkillIndex, 0);
        //}

		//SetMp(GetMaxMp());
		CommonParam.StartBirth = true;

        MainProcess.mCameraObject.transform.position = GetPosition();

		OnPositionChanged();

#if DEBUG_SKILL
        return;
#endif
        MainProcess.mLockCharacterPos = true;
		
        //float waitTime01 = 0f;
        //float waitTime02 = 0f;
        //float waitTime03 = 0f;
        //ChenckPet(ref waitTime01, ref waitTime02, ref waitTime03 );
        //
        //Logic.EventCenter.WaitAction(this, "InitFriendPet", 0, waitTime01);
        //Logic.EventCenter.WaitAction(this, "InitFriendPet", 1, waitTime02);
        //Logic.EventCenter.WaitAction(this, "InitFriendPet", 2, waitTime03);
        //
        GameCommon.SetLayer(mMainObject, CommonParam.CharacterLayer);
        mMainObject.StartCoroutine(CreatePets());

        // 缘分Buff
        //if (GetObjectType() == OBJECT_TYPE.CHARATOR)
        //{
        //    List<int> buffs = Relationship.AllActiveRelateBuffs(0);

        //    foreach (var b in buffs)
        //    {
        //        StartAffect(this, b);
        //    }
        //}
	}

    protected override void InitSkillOnStart(int fieldIndex, int skillIndex)
    {
        if (fieldIndex == 0)
        {
            ActivateCDLooper(skillIndex, SKILL_POS.CHAR_MAIN, GetConfigSkillLevel(fieldIndex), 0f);
        }
        else if(IsSKillOpened(fieldIndex))
        {
            ActivateCDLooper(skillIndex, SKILL_POS.SPECIAL_ATTACK, GetConfigSkillLevel(fieldIndex), 0f);
        }
    }

	//base.base
	public void _Start(){base.Start ();}

    //public override float LookBound() { return mLookRange; }

    //public override float GetMoveSpeed()
    //{
    //    if (mAutoBattleAI != null)
    //        return base.GetMoveSpeed() * 1.5f;
    //    else
    //        return base.GetMoveSpeed();
    //}

    private IEnumerator CreatePets()
    {
        if (petsCreateDone)
        {
            yield break;
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < 4; ++i)
        {
            if (InitFriendPet(i) == null)
            {
                yield return null;
            }
            else
            {
                ++createdPetCount;
                yield return new WaitForSeconds(0.2f);
            }
        }

        petsCreateDone = true;

        if (onPetsCreateDone != null)
        {
            onPetsCreateDone();
        }
    }

    public void CreatePetsImmediate()
    {
        if (mbHasStarted || petsCreateDone)
        {
            return;
        }

        for (int i = 0; i < 4; ++i)
        {
            if (InitFriendPet(i) != null)
            {
                ++createdPetCount;
            }
        }

        petsCreateDone = true;

        if (onPetsCreateDone != null)
        {
            onPetsCreateDone();
        }
    }

	public void ChenckPet(ref float waitTime01, ref float waitTime02, ref float waitTime03)
	{
		float startTime = 0.1f;
		float intervlTime = 0.2f;
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;

		PetData petData01 = petLogicData.GetPetDataByPos(petLogicData.mCurrentTeam, 1);
		PetData petData02 = petLogicData.GetPetDataByPos(petLogicData.mCurrentTeam, 2);
		PetData petData03 = petLogicData.GetPetDataByPos(petLogicData.mCurrentTeam, 3);
		if(petData01 != null)
		{
			waitTime01 = startTime;
			if(petData02 != null)
			{
				waitTime02 = waitTime01 + intervlTime ;
				if(petData03 != null) waitTime03 = waitTime02 + intervlTime;
				else waitTime03 = 0f;
			}
			else
			{
				waitTime02 = 0f;
				if(petData03 != null) waitTime03 = waitTime01 + intervlTime;
				else waitTime03 = 0f;
			}
		}
		else
		{
			waitTime01 = 0f;
			if(petData02 != null)
			{
				waitTime02 = startTime;
				if(petData03 != null) waitTime03 = waitTime02 + intervlTime;
				else waitTime03 = 0f;
			}
			else
			{
				waitTime02 = 0f;
				if(petData03 != null) waitTime03 = startTime;
				else waitTime03 = 0f;
			}
		}
	}

    public Friend InitFriendPet(object param)
    {
        //PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        //PetData petData = null;
        //for (int usePos = 1; usePos <= msFriendsCount - 1; usePos++)
        int usePos = (int)param + 1;
        {
            //if (MainProcess.mStage is PVP4Battle)
            //    petData = petLogicData.GetFourVsFourPetDataByPos(usePos);
            //else
            var petData = MainProcess.mStage.GetPetDataAtTeamPos(usePos);//TeamManager.GetPetDataByTeamPos(usePos);//petLogicData.GetPetDataByPos(petLogicData.mCurrentTeam, usePos);
            
            if (petData != null)
            {
                Friend obj = ObjectManager.Self.CreateObject(OBJECT_TYPE.PET, petData.tid) as Friend;
                obj.SetCamp(GetCamp());
                obj.SetOwnerObject(this);

                obj.SetActiveData(petData);
                obj.mUsePos = usePos;

                int petSkillIndex = obj.GetConfig("PET_SKILL_1");

                if(petSkillIndex > 0)
                {
                    ActivateCDLooper(petSkillIndex, (SKILL_POS)usePos, obj.GetConfigSkillLevel(0), 0f);
                }
                //obj.mFollowLocation.SetParam(-45.0f - 90 * usePos); //(float)System.Math.PI * 0.25f);
                //
                //obj.InitPosition(obj.GetFollowPosition());
                //obj.Start();               
                mFriends[usePos - 1] = obj;
                InitFriendFollowPosition(obj, usePos);       
                InitFriendValidPosition(obj, 10);
                obj.SetDirection(obj.GetPosition() + this.GetDirection() * Vector3.forward);

                obj.Start();
                DataCenter.SetData("BATTLE_PLAYER_WINDOW", "PET_ALIVE", usePos);
                return obj;
            }
        }
//
//        if (usePos >= 3)
//        {
//            MainProcess.mLockCharacterPos = false;

//            if (MainProcess.mStage.IsPVE())
//            {
//                MainProcess.mStage.mbEnableAI = true;
//
//                if (MainProcess.mStage.IsPVE() && PVEStageBattle.mbAutoBattle)
//                {
//                    GuideManager.ExecuteDelayed(() => EventCenter.Start("AutoBattleAI").DoEvent(), 0f);
//                }
//            }
//        }
        return null;
    }

    public void InitFriendFollowPosition(Friend friend, int usePos)
    {
        int standPos = 0;

        if (AIKit.GetNormalAttackDistance(friend) < 5f)
        {
            standPos = 1;

            for (int i = 0; i < usePos - 1; ++i)
            {
                if (mFriends[i] != null && AIKit.GetNormalAttackDistance(mFriends[i]) < 5f)
                {
                    ++standPos;
                }
            }
        }
        else
        {
            standPos = 5;

            for (int i = 0; i < usePos - 1; ++i)
            {
                if (mFriends[i] != null && AIKit.GetNormalAttackDistance(mFriends[i]) >= 5f)
                {
                    ++standPos;
                }
            }
        }

        friend.mFollowOffset = AIKit.GetFollowOffset(standPos);
        friend.mFollowPos = standPos;
        SetFollowPosLocked(standPos, true);
    }

    public bool InitFriendValidPosition(Friend friend, int maxAttemptCount)
    {
        int attemptCount = maxAttemptCount;
        var loc = new FollowLocation(friend.GetOwnerObject(), friend.mFollowOffset);
        friend.InitPosition(loc.GetPosition());

        while (!CheckPositionValid(friend) && attemptCount-- > 0)
        {
            float angle = UnityEngine.Random.Range(0f, 360f);
            friend.mFollowLocation.SetParam(angle);
            friend.InitPosition(friend.GetFollowPosition());
        }

        return attemptCount < 0;
    }

    public bool CheckPositionValid(BaseObject obj)
    {
        NavMeshAgent agent = obj.mMainObject.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(mMainObject.transform.position, path);
            return path.status == NavMeshPathStatus.PathComplete;
        }
        else
        {
            return true;
        }
    }

    //public void RefreshFiendAI(bool bFollow)
    //{
    //    foreach (BaseObject f in mFriends)
    //    {
    //        Friend friend = f as Friend;
    //        if (friend!=null)
    //        {
    //            friend.StopAI();
    //            if (bFollow)
    //            {
    //                friend.SetCurrentEnemy(null);
    //                friend.FollowOwner();
    //            }
    //            else
    //                friend.StartAI();
    //        }
    //    }
    //}

    public Friend CreateFriendPet(PetData petData)
    {
        int usePos = 4;
        Friend obj = ObjectManager.Self.CreateObject(OBJECT_TYPE.PET, petData.tid) as Friend;
        obj.SetCamp(GetCamp());
        obj.SetOwnerObject(this);

        obj.SetActiveData(petData);
        obj.mUsePos = usePos;

        int petSkillIndex = obj.GetConfig("PET_SKILL_1");

        if (petSkillIndex > 0)
        {
            ActivateCDLooper(petSkillIndex, SKILL_POS.CHAR_FRIEND, obj.GetConfigSkillLevel(0), 0f);
        }

        InitFriendFollowPosition(obj, 4);   
        InitFriendValidPosition(obj, 10);

        //obj.mFollowLocation.SetParam(-45.0f - 90 * usePos); //(float)System.Math.PI * 0.25f);
        //
        //obj.InitPosition(GetPosition());
        obj.Start();

        mFriends[3] = obj;

		return obj;
    }

    //public virtual void SetCurCharactor()
    //{
    //    RoleData roleData = RoleLogicData.GetMainRole();

    //    if(roleData != null)
    //    {
    //        SetLevel(roleData.level);
    //        SetBreakLevel(roleData.breakLevel);
    //        SetExp(roleData.exp);
    //        SetTotalAddExp(0);
    //        SetGold(0);
    //        AddGold(0);
    //    }

    //    SetAttributeByLevel();
    //}

    public override void SetActiveData(ActiveData activeData)
    {
        SetExp(activeData.exp);
        SetTotalAddExp(0);
        SetGold(0);
        AddGold(0);

        base.SetActiveData(activeData);
    }

	public override void OnPositionChanged()
	{
		base.OnPositionChanged();

		if(mbIsPvpOpponent)
			return;

        if (mEnemy != null)
        {
            if (!AutoBattleAI.InAutoBattle() && Vector3.Distance(mEnemy.GetPosition(), GetPosition()) > LookBounds())
                mEnemy = null;
        }

        CameraMoveEvent.OnTargetObjectMove(this);
		
        if (MainProcess.mbPress)
            MainProcess.UpdateMouseCoord();

        DataCenter.SetData("MINI_MAP", "POSITION", 0);
	}

//	//base.base
//	public void _OnPositionChanged(){base.OnPositionChanged ();}

    public override bool AllowCameraUpdateHeight() { return !(mCurrentAI is AI_CharMoveToEnemy); }

    //public override ObjectAI StartAI()
    //{
    //    OnStartAI();

    //    //ObjectAI ai = StartAI("AI_CharReadyIdle");
    //    //if (ai!=null)
    //    //	ai.DoEvent();
    //    //return ai; 
    //    if (MainProcess.mStage.IsPVP())
    //    {
    //        //StartAIRoutine(new FightRoutine(this));
    //        StartAIRoutine(new AIMachine(this));
    //    }
    //    else
    //    {
    //        //StartAIRoutine(new CustomerAIRoutine(this));
    //        StartAIRoutine(new CustomerAIMachine(this));
    //    }

    //    return null;
    //}

    //public override AIMachine CreateAIMachine()
    //{
    //    if (MainProcess.mStage.IsPVP())
    //    {
    //        return new AIMachine(this);
    //    }
    //    else 
    //    {
    //        return new RoleAIMachine(this);
    //    }
    //}

	public virtual ObjectAI StartCharMoveAI()
	{
        if (mCurrentAI != mCharMoveAI)
		    StopAI();
		mCurrentAI = mCharMoveAI;
        mCharMoveAI.SetFinished(false);
		return mCharMoveAI;
	}

	// Update is called once per frame
	public override void Update (float onceTime) 
	{
		base.Update (onceTime);

        if (mbHasStarted)
        {
            GameObject effect = GameObject.Find("w_effect");

            if (effect != null)
            {
                TrailRenderer r = effect.GetComponentInChildren<TrailRenderer>();

                if (mState != EObjectState.ATTACK)
                    r.enabled = false;
                else
                {
                    r.enabled = true;

                    if (mHandBoneForm != null)
                    {
                        effect.transform.position = mHandBoneForm.position;
                        effect.transform.rotation = mHandBoneForm.rotation;
                    }
                }
            }

            // battleing
            if (MainProcess.mStage != null && !MainProcess.mStage.mbBattleFinish)
            {
                mDTime += onceTime;

                if (mDTime > 0.5f && !mDied)
                {
                    ChangeHp(GetConfig("AUTO_HP"));
                    ChangeMp(GetConfig("AUTO_MP"));
                    //ChangeHp((int)GetAutoHp());
                    //ChangeMp((int)GetAutoMp());
                    mDTime -= 0.5f;
                }
            }
        }
    }


    public override void OnMoveEnd()
    {
        base.OnMoveEnd();
#if SHOW_FOLLOW_POSITION
        mFollowPosFlag.transform.position = GetPosition() + GetDirection() * (Vector3.forward * 2f);
#endif
        if (MainProcess.mMouseCoord!=null)
            MainProcess.mMouseCoord.SetActive(false);

        if (mRunEffect!=null)
            mRunEffect.Show(false);
    }

    public override void OnWillMoveTo(ref Vector3 targetPos) 
    {
        //if (mCurrentAutoTargetAI==null)
        //	FriendsToFollow(targetPos);
        //
        if(mRunEffect != null)
            mRunEffect.Show(true);
    }

    //public void FriendsToFollow(Vector3 targetPos)
    //{
    //    Vector3 dir = targetPos - GetPosition();
    //    dir.Normalize();
    //    foreach (BaseObject obj in mFriends)
    //    {
    //        Friend f = obj as Friend;
    //        if (f != null)
    //            f.FollowOwner(targetPos, dir);
    //    }  
    //}

	public override void OnDead()
	{
		base.OnDead();
		//StartDoAI("AI_MonsterRelive");

		DataCenter.SetData(mBattlePlayerWindow, "CHAR_DEAD", true);

        //if (MainProcess.mStage is PVP4Battle)
        //{
        //    if (CheckAllDead())
        //    {
        //        MainProcess.mStage.OnFinish(mbIsPvpOpponent);
        //    }
        //    else if(!mbIsPvpOpponent)
        //    {
        //        CameraMoveEvent.BindMainObject(GetAlivePet());
        //    }
        //}
        //else
        //{
        //    MainProcess.mStage.OnFinish(false);
        //}
	}

    public bool TeamAllDead()
    {
        if (IsDead())
        {
            return GetAlivedPet() == null;
        }

        return false;
    }

    public Pet GetAlivedPet()
    {
        for (int i = 0; i < msFriendsCount; ++i)
        {
            var f = mFriends[i];

            if (f != null && !f.IsDead())
            {
                return f as Pet;
            }
        }

        return null;
    }

    public float GetTeamHpRate()
    {
        int hp = GetHp();
        int total = GetMaxHp();

        for (int i = 0; i < msFriendsCount; ++i)
        {
            var f = mFriends[i];

            if (f != null)
            {
                hp += f.GetHp();
                total += f.GetMaxHp();
            }
        }

        return total <= 0 ? 0f : hp / (float)total;
    }

	public override float AttackTime(){ return 1.5f; }
	
	public override float HitTime(){ return 0.6f; }

    public override BaseObject GetCampEnemy()
    {
        if (mCurrentAutoTargetAI != null
            && !mCurrentAutoTargetAI.GetFinished()
            && mCurrentAutoTargetAI.mTargetMonster != null
            && !mCurrentAutoTargetAI.mTargetMonster.IsDead()
            )
            return mCurrentAutoTargetAI.mTargetMonster;

        if (mEnemy != null && !mEnemy.IsDead())
        {
            return mEnemy;
        }

        //foreach (BaseObject obj in mFriends)
        //{
        //    if (obj != null && obj.GetCurrentEnemy() != null && !obj.GetCurrentEnemy().IsDead())
        //    {
        //        BaseObject enemy = obj.GetCurrentEnemy();
        //        if (Vector3.Distance(enemy.GetPosition(), GetPosition()) < LookBound())
        //            return enemy;
        //    }
        //}


        return null;
    }

     public override bool SetCurrentEnemy(BaseObject enemy)
     {
		if(MainProcess.mStage is PVP4Battle && !(enemy is Character))
			return false;

         if (base.SetCurrentEnemy(enemy))
         {            
             ActiveObject a = mEnemy as ActiveObject;
             if (a != null)
             {
                 //a.ShowLockFlag();
                 LockEnemy(a);
                 //if (mWarnEffect!=null)
                 //   mWarnEffect.SetVisible(true);
             }
             return true;
         }
         return false;
     }

#if DEBUG_SKILL
    public override Skill Attack(BaseObject target)
    {
        return null;
    }
#endif
	public override bool CanDoSkill(DataRecord config)
	{
		if(config != null)
		{
            int iNeedMp = SkillGlobal.GetInfo(config).needMP;//config.getData("NEED_MP");
			if(mMp < iNeedMp)
			{
				//DEBUG.Log("你的MP应大于" + iNeedMp);
				return false;
			}			
		}
		return base.CanDoSkill(config);
	}

    public override void Relive()
    {
        base.Relive();

        //??? DataCenter.SetData("BATTLE_UI", "CLOSE", 0);
    }

	public BaseObject SelectMonster()
	{
		Ray mouseRay = GameCommon.GetMainCamera().ScreenPointToRay(Input.mousePosition);
		RaycastHit rayInfo;        
		int mask = 1<<CommonParam.MonsterLayer;
		
		if (Physics.Raycast(mouseRay, out rayInfo, 600, mask))
		{
            //string name = rayInfo.collider.gameObject.name;
            //char[] s = {'_'};
            //string[] slist = name.Split(s, 10);
            //if (slist.Length >= 1)
            //{
            //    try
            //    {
            //        BaseObject hitObject = ObjectManager.Self.GetObject(int.Parse(slist[0]));

            MonsterFlag m = rayInfo.collider.gameObject.GetComponent<MonsterFlag>();
            if (m!=null)
            {
                ActiveObject hitObject = m.mMonster;
                    if (hitObject != null && !hitObject.IsDead() && IsEnemy(hitObject))
                    {
                        ////hitObject.ShowLockFlag();
                        //LockEnemy(hitObject);
                        //
                        //if(!(mCurrentAI is AttackSkill))
                        //    PursueAttack(hitObject);
                        //
                        //foreach (BaseObject f in mFriends)
                        //{
                        //	ActiveObject friend = f as Friend;
                        //    if (friend!=null)
                        //    {
                        //        if (friend.GetCurrentEnemy()!=hitObject)
                        //            friend.PursueAttack(hitObject);                                
                        //    }
                        //}

                        //hitObject.InitShadow();
                        //hitObject.ShowLockFlag();

                        if (MainProcess.Self.mController != null)
                        {
                            MainProcess.Self.mController.OnSelectObject(hitObject);
                        }
                        //OnBattleCustomerActionObserver.Instance.Notify("ClickObject", hitObject);
                        return hitObject;
                    }
                //}
                //catch
                //{
                //}
            }
        }

		return null;
	}

    private void LockEnemy(ActiveObject target)
    {
        if (mLockEnemy == target)
            return;

        if (mLockEnemy != null && !mLockEnemy.IsDead())
        {
            mLockEnemy.InitShadow();
        }

        if (target != null && !target.IsDead())
        {
            LockFlag.Lock(target);
        }

        mLockEnemy = target;
    }

    public override void OnLockEnemy(BaseObject enemy)
    {
        LockFlag.Lock(enemy as ActiveObject);
    }

	//----------------------------------------------
	// HP
    //public override int GetMaxHp()
    //{
    //    int value = GetStaticMaxHp();
    //    int baseValue = GetBaseMaxHp();
    //    return value + (int)GetBufferAddValue(AFFECT_TYPE.HP_MAX) +(int)GetBufferAddValueByRate(baseValue, AFFECT_TYPE.HP_MAX_RATE);
    //}

    //public override int GetStaticMaxHp()
    //{
    //    return GetBaseMaxHp();
    //}

	public override void ChangeHp(int nHp)
	{
		if(!mDied && !mbIsUI)
		{
			base.ChangeHp(nHp);
			tLogicData d = DataCenter.Self.getData (mBattlePlayerWindow);
			if (d!=null)
			{
				d.set ("MAX_HP", GetMaxHp());
				d.set ("HP", GetHp());
			}
		}
	}

	// Exp
	public virtual void AddExp(int dExp)
	{
		if(!mDied)
		{
			if(mLevel >= GetMaxLevel())
			{
				mExp = 0;
				return;
			}

			mExp += dExp;
			int iMaxExp = GetMaxExp();
			if(mExp >= iMaxExp)
			{
	            LevelUp();
	            int iDExp = mExp - iMaxExp;
	            mExp = 0;
	            AddExp(iDExp);
			}
			else
			{
				// set UI
				ShowExp();
			}
		}
	}

	public virtual int GetMaxExp()
	{
		return TableCommon.GetRoleMaxExp(mLevel);
	}

	public void ShowExp()
	{
		tLogicData d = DataCenter.Self.getData ("BATTLE_PLAYER_EXP_WINDOW");
		if (d!=null)
		{
			d.set ("MAX_EXP", GetMaxExp());
			d.set ("EXP", GetExp());
		}
	}

	public void SetExp( int nExp ){ mExp = nExp; }
	public int GetExp(){ return mExp;}

	public void SetTotalAddExp( int nTotalAddExp ){ mTotalAddExp = nTotalAddExp; }
	public int GetTotalAddExp(){ return mTotalAddExp;}

	public virtual void AddTotalExp(int dExp)
	{
		if(!mDied)
		{
			mTotalAddExp += dExp;
		}
	}

	// Gold
	public virtual void AddGold(int dGold)
	{
		mGold += dGold;       
	}

    public virtual void AddUIGold(int dGold)
    {
        mUIGold += dGold;

        if (dGold > 0)
            ShowPaoPao(dGold.ToString(), PAOTEXT_TYPE.GOLD);

        ShowGold();
    }

	public void SetGold(int iGold){mGold = iGold;}
	public int GetGold(){ return mGold;}

	public void ShowGold()
	{
        tLogicData d = DataCenter.Self.getData("PVE_TOP_RIGHT_WINDOW");
		if (d!=null)
		{
			d.set ("GOLD", mUIGold);
		}
	}

    public void AddUIItem(int dItem)
    {
        mUIItem += dItem;

        if (dItem > 0)
        {
            // Effect
        }

        ShowItem();
    }

    public void ShowItem()
    {
        tLogicData d = DataCenter.Self.getData("PVE_TOP_RIGHT_WINDOW");
        if (d != null)
        {
            d.set("ITEM", mUIItem);
        }
    }

	public override void SetAttributeByLevel()
	{
		base.SetAttributeByLevel();
		ShowExp();
		SetMp(GetMaxMp());
		
		tLogicData d = DataCenter.Self.getData (mBattlePlayerWindow);
		if (d!=null)
			d.set ("LEVEL", GetLevel());
	}
	
	public void SetMp(int iMp){ChangeMp(iMp - mMp);}
	public int GetMp(){ return mMp;}

    //public virtual int GetMaxMp()
    //{
    //    int value = GetStaticMaxMp();
    //    int baseValue = GetBaseMaxMp();
    //    return value + (int)GetBufferAddValue(AFFECT_TYPE.MP_MAX) + (int)GetBufferAddValueByRate(baseValue, AFFECT_TYPE.MP_MAX_RATE);
    //}

    public int GetMaxMp()
    {
        int value = mStaticMaxMp + (int)GetBufferAddValue(AFFECT_TYPE.MP_MAX) + (int)GetBufferAddValueByRate(mStaticMaxMp, AFFECT_TYPE.MP_MAX_RATE);
        return Mathf.Max(1, value);
    }

    public virtual int GetBaseMaxMp()
    {
        return GameCommon.GetBaseMaxMP(mConfigRecord, mLevel, mBreakLevel);
    }

    public virtual int GetStaticMaxMp()
    {
        //return GetBaseMaxMp();
        return (int)GetStaticAttribute(AFFECT_TYPE.MP_MAX, mBaseMaxMp);//(int)mAffect.Final(AFFECT_TYPE.MP_MAX, mBaseMaxMp);
    }
	
	public void ChangeMp(int nMp)
	{
		if(!mDied && !mbIsUI)
		{
			mMp += nMp;
			if (mMp<=0)
			{
				mMp = 0;
				// TODO：技能显示是灰色，不可用
			}
			else if(mMp>GetMaxMp())
				mMp = GetMaxMp();
			
			ShowMp();
			
			tLogicData d = DataCenter.Self.getData (mBattlePlayerWindow);
			if (d!=null)
			{
				d.set ("MAX_MP", GetMaxMp());
				d.set ("MP", GetMp());
			}
		}
	}
	
	public void ShowMp()
	{
		//		if (mShowMpBar!=null)
		//			mShowMpBar.SetValue(GetMp(), GetMaxMp());
	}

	// Attack
    //public override int GetBaseAttack()
    //{
    //    return GameCommon.GetBaseAttack(mConfigRecord, mLevel, mBreakLevel);
    //}

    //public override float GetStaticAttack()
    //{
    //    return GetBaseAttack();
    //}

    //public override float GetAttack()
    //{
    //    float value = GetStaticAttack();
    //    float baseValue = GetBaseAttack();
    //    return value + GetBufferAddValue(AFFECT_TYPE.ATTACK) + GetBufferAddValueByRate(baseValue, AFFECT_TYPE.ATTACK_RATE);
    //}

    public override float GetDamageMitigationRate()
    {
        return (MainProcess.mStage != null && MainProcess.mStage.mbSucceed) ? 1f : base.GetDamageMitigationRate();
    }

	// Level
	public virtual void LevelUp()
	{
		if(!mDied)
		{
			if(mLevel >= GetMaxLevel())
			{
				return;
			}

			mLevel += 1;
			DataCenter.OpenWindow ("UPGRADE_WINDOW");
			tWindow t = DataCenter.GetData ("UPGRADE_WINDOW") as tWindow;
			if(t != null) MonoBehaviour.Destroy (t.mGameObjUI, 2f);
            GlobalModule.DoLater(() =>
            {
                if (NewFuncOpenWindow.mbNeedBreak) 
                {
                    NewFuncOpenWindow.mbNeedBreak = false;
                    return;
                }
                NewFuncOpenWindow.ShowGoToNewFuncWin(mLevel);
            }, NewFuncOpenWindow.mWaitTime);
			if(mLevel >= GetMaxLevel())
			{
				mExp = 0;
			}

			// setting
			SetAttributeByLevel();

	        // player level up action
	        PlayAnim("win");
		}
	}
	public virtual int GetMaxLevel(){return mMaxLevel;}

    public override void InitBufferUI(int iAffectConfig)
    {
		DataCenter.SetData(mBattlePlayerWindow, "START_CHAR_BUFF", iAffectConfig);
    }

    public override void RemoveBufferUI(int iAffectConfig)
    {
		DataCenter.SetData(mBattlePlayerWindow, "REMOVE_CHAR_BUFF", iAffectConfig);
    }

    protected override void OnPoolAffect()
    {
        base.OnPoolAffect();

        int mp = (int)mAffectPool.GetValue("MP");

        if (mp != 0)
        {
            ChangeMp(mp);
            mAffectPool.ResetValue("MP");
        }
    }

    public float GetSkillCD(float baseCD)
    {
        double delta = mAffectPool.GetValue("SKILL_CD");
        double rate = mAffectPool.GetValue("SKILL_CD_RATE") / 100d;
        return (float)(baseCD + delta + baseCD * rate);
    }

    public float GetSummonCD(float baseCD)
    {
        double delta = mAffectPool.GetValue("SUMMON_CD");
        double rate = mAffectPool.GetValue("SUMMON_CD_RATE") / 100d;
        return (float)(baseCD + delta + baseCD * rate);
    }

    public float GetSummonTime(float baseCD)
    {
        double delta = mAffectPool.GetValue("SUMMON_TIME");
        double rate = mAffectPool.GetValue("SUMMON_TIME_RATE") / 100d;
        return (float)(baseCD + delta + baseCD * rate);
    }

    virtual public void RemoveAllSkillCD()
    {
        tLogicData skillData = DataCenter.GetData("SKILL_UI");

        if (skillData == null)
            return;

        for (int i = 1; i <= 4; ++i)
        {
            tLogicData d = skillData.getData("do_skill_" + i);

            if (d != null)
                d.set("REMOVE_SKILL_CD", true);         
        }
    }

    virtual public void RemoveAllSummonCD()
    {
        tLogicData skillData = DataCenter.GetData("SKILL_UI");

        if (skillData == null)
            return;

        for (int i = 1; i <= 4; ++i)
        {
            tLogicData d = skillData.getData("do_skill_" + i);

            if (d != null)
                d.set("REMOVE_SUMMON_CD", true);
        }
    }

    //public override BaseObject FindPriEnemy(SEARCH_RANGE range)
    //{
    //    if (range == SEARCH_RANGE.AUTO && AIKit.InAutoBattle())
    //    {
    //        return AIKit.FindNearestEnemy(this, false);
    //    }

    //    return base.FindPriEnemy(range);
    //}
}
//------------------------------------------------------------------------------

public class CharatorerFactory : ObjectFactory
{
    public override OBJECT_TYPE GetObjectType(){ return OBJECT_TYPE.CHARATOR; }
    public override BaseObject NewObject()
    {
        return new Character();
    }

    public override NiceTable GetConfigTable() { return DataCenter.mActiveConfigTable; }
}

//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
public class Friend : Role
{
    public tLocation mFollowLocation = new ObjectLocationForPet();

	public tLogicData mMiniMapFlagData;

	//public PetData mPetData;

    public int mUsePos;

    public Logic.tEvent[] mSkillList;

    public bool mbLifeTimeOver = false;


#if SHOW_FOLLOW_POSITION
    GameObject mFollowPosFlag;
#endif

    public Friend()
    {
        mFollowLocation.mPosition = Vector3.zero;
        mFollowLocation.mPosition.z = 1.5f;        

#if SHOW_FOLLOW_POSITION
        GameObject m = GameCommon.LoadPrefabs("prefabs/MouseCoord");
#endif
    }

    //public override void InitShadow()
    //{
    //    if (GameCommon.CheckNeedShowAureole(mConfigIndex))
    //    {
    //        return;
    //    }
    //    CreateShadow("textures/shadow_friend", 1.5f/*mBaseSelectRadius * CommonParam.shadowRadiusScale*/, 0.5f);

    //    //mShadow = new Shadow();
    //    //mShadow.Init("Prefabs/CharShadow", new Vector3(0, 2, 0) );
    //    ////GameObject o = Resources.Load("Prefabs/Shadow_Friend", typeof(GameObject)) as GameObject;
    //    ////if (o != null)
    //    ////{
    //    ////    GameObject s = GameObject.Instantiate(o) as GameObject;
    //    ////    s.transform.parent = mMainObject.transform;
    //    ////    s.transform.localPosition = new Vector3(0, 2, 0);
    //    ////}
    //}

    // 宠物重启时让宠物快速找到视野敌人, 避免被主角被动跟随 (锁定敌人后, 就不会再跟随)
    public override ObjectAI StartRestartAI()
    {
        mEnemy = FindNearestEnemy();

        return base.StartRestartAI();
    }

    public override void OnStart()
    {
        base.OnStart();

        if (mActiveData == null || mActiveData.starLevel < 4)
        {
            PlayEffect(3026, null);
        }
        else 
        {
            PlayEffect(3030, null);
        }
    }

    //public override void Start()
    //{
    //    base.Start();

    //    // 缘分Buff
    //    if (GetObjectType() == OBJECT_TYPE.PET && mActiveData != null && mActiveData.teamPos >= 1 && mActiveData.teamPos <= 3)
    //    {
    //        List<int> buffs = Relationship.AllActiveRelateBuffs(mActiveData.teamPos);

    //        foreach (var b in buffs)
    //        {
    //            StartAffect(this, b);
    //        }
    //    }
    //}

    public override void OnInitModelObject()
    {
        base.OnInitModelObject();

        NavMeshAgent nav = mMainObject.GetComponent<NavMeshAgent>();
        nav.avoidancePriority = 51;
        //nav.radius = 1;
        //nav.radius = AIParams.friendNavRadius;

        string skillConfig = GetConfig("SKILL");
        if (skillConfig != "")
        {            
            int[] skillList;
            GameCommon.SplitToInt(skillConfig, ' ', out skillList);
            mSkillList = new Logic.tEvent[skillList.Length];
            for (int i = 0; i < skillList.Length; ++i)
            {
                int skillIndex = skillList[i];
                if (skillIndex > 0)
                {
                    float cdTime = SkillGlobal.GetInfo(skillIndex).skillCD;//TableManager.GetData("Skill", skillIndex, "CD_TIME");
                    mSkillList[i] = Logic.EventCenter.Start("TM_SkillCD");
                    mSkillList[i].WaitTime(cdTime);
                    mSkillList[i].set("SKILL_INDEX", skillIndex);
                }
            }
        }
    }

    //public override float GetMoveSpeed()
    //{
    //    if (Character.Self.mAutoBattleAI != null)
    //        return Character.Self.GetMoveSpeed();
    //    else
    //        return base.GetMoveSpeed();
    //}

	public override void Update (float onceTime) 
	{
		base.Update (onceTime);
		if(mMiniMapFlagData == null)
			return;
		mMiniMapFlagData.set("POS", GetPosition());
		mMiniMapFlagData.set("SHOW", GetPosition());
	}

    //public override int TotalDamage(BaseObject targetObject, int damageValue, Skill skill)
    //{
    //    return damageValue + GetConfig("ATTACK_DAMAGE");
    //}

    //public override void Start()
    //{
    //    base.Start();
    //
    //    if (MainProcess.mbStartPetAIOnCreate)
    //    {
    //        ObjectAI ai = StartAI("AI_FriendCheckMainRaleEnemy");
    //        ai.DoEvent();
    //    }
    //}

    public override void OnStartAI()
    {
        if (mMiniMapFlagData == null)
        {
            object obj;
            bool bisExist = DataCenter.Self.getData("MINI_MAP", out obj);
            if (bisExist)
            {
                tLogicData d = obj as tLogicData;
                mMiniMapFlagData = d.getData("FRIEND");
            }
        }
    }

    //public override AIMachine CreateAIMachine()
    //{
    //    return new RoleAIMachine(this);
    //}

    //public override ObjectAI StartAI()
    //{
    //    OnStartAI();

    //    //ObjectAI ai;
    //    ////if (AutoBattleAI.InAutoBattle())
    //    ////    ai = StartAI("AI_FriendReadyIdleInAutoBattle");
    //    ////else
    //    //    ai = StartAI("AI_FriendReadyIdle");
    //    //
    //    //if (ai!=null)
    //    //    ai.DoEvent();
    //    //
    //    //return ai;
    //    //StartAIRoutine(new PetFightRoutine(this));
    //    StartAIRoutine(new AIMachine(this));
    //    return null;
    //}

    public override bool Attack(BaseObject target)
    {
		if (mEnemy!=target)
			return false;

        if (mSkillList != null && mSkillList.Length > 0)
        {
            for (int i = 0; i < mSkillList.Length; ++i)
            {
                if (mSkillList[i] != null && mSkillList[i].GetFinished())
                {
                    int skillIndex = mSkillList[i].get("SKILL_INDEX");
                    if (skillIndex > 0)
                    {
                        mSkillList[i].SetFinished(false);

                        //Skill s = DoSkill(skillIndex, target, null);
                        if (SkillAttack(skillIndex, target, null)) // s != null)
                        {
                            float t = 0;
                            DataRecord configRe = DataCenter.mSkillConfigTable.GetRecord(skillIndex);
                            if (configRe != null)
                                t = SkillGlobal.GetInfo(configRe).skillCD;//configRe.get("CD_TIME");

                            if (t < 0.0001f)
                                mSkillList[i] = null;
                            else
                                mSkillList[i].WaitTime(t);

                            return true;
                        }
                    }
                }
            }
        }
        return base.Attack(target);
    }

    public override void OnIdle()
    {
        base.OnIdle();

        //FollowOwner();
    }

    public Vector3 GetFollowPosition()
    {
        return mFollowLocation.Update(mOwnerObject, null);
    }

    public bool FollowOwner()
    {
        if (IsDead() || mEnemy!=null&&!mEnemy.IsDead() )
            return false;

        //StartAIRoutine(new FollowRoutine(this, GetOwnerObject(), this.mFollowOffset));
        aiMachine.StartTerminateAI(new FollowRoutine(this, GetOwnerObject(), mFollowOffset, mFollowPos <= 0 || mFollowPos > 8));
        return true;
        //if (!IsFollowInBossBattle())
        //{
        //    BaseObject obj = FindAttackBoundEnemy();
        //    if (obj != null)
        //    {
        //        SetCurrentEnemy(obj);
        //        if (Attack(obj))
        //        {
        //            return false;
        //        }
        //    }
        //}
        /*
        if (mOwnerObject == null)
            return false;

        if (AutoBattleAI.InAutoBattle())
        {
            BaseObject obj = mOwnerObject.GetCampEnemy();
            if (obj != null && !obj.IsDead())
            {
                AI_FriendWaitAttack waitAI = StartAI("AI_FriendWaitAttack") as AI_FriendWaitAttack;
				waitAI.mTarget = obj;
				waitAI.DoEvent();
                return true;
            }
        }

        Vector3 pos;
        //if (Vector3.Distance(mOwnerObject.GetPosition(), GetPosition()) > 2)
        //    pos = mOwnerObject.GetPosition();
        //else
        {
            pos = GetFollowPosition();           
            Vector3 p = GetPosition();

            float dis = Distance(pos.x, pos.z, p.x, p.z);
            if (dis < 0.2f)
                return false;
        }
        //if (Character.Self.IsNeedMove())
        //{
        //    Vector3 toDir = Character.Self.GetPosition()-GetPosition();
        //    toDir.Normalize();
        //    float last = Vector3.Distance(Character.Self.GetPosition(), GetPosition()) * Vector3.Dot(toDir, Character.Self.mMoveValue.mDirection);
        //    if (last < 0.5f)
        //        return false;
        //}

		AI_MoveTo ai;
        if (IsFollowInBossBattle())
            ai = StartMoveAI() as AI_MoveTo;
        else
            ai = StartAI("AI_FriendFollowMove") as AI_MoveTo; //ForceStartAI("AI_MoveTo") as AI_MoveTo;
        if (ai != null)
        {
#if SHOW_FOLLOW_POSITION
            mFollowPosFlag.transform.position = pos;
#endif
            ai.mTargetPos = pos; 
            ai.DoEvent();

            return true;
        }
        return false;*/
    }

    static public float Distance(float x, float z, float targetX, float targetZ)
    {
        float lx = (targetX - x);
        float lz = (targetZ - z);
        return (float)System.Math.Sqrt(lx*lx+lz*lz);
    }

    public bool FollowOwner(Vector3 charTargetPos, Vector3 charMoveDirection)
    {
        if (!IsFollowInBossBattle())
        {
            if (mCurrentAI is Skill || IsDead() || (mEnemy != null && !mEnemy.IsDead()))
                return false;

            //BaseObject obj = FindAttackBoundEnemy();
            //if (obj != null)
            //{
            //    SetCurrentEnemy(obj);
            //    if (Attack(obj) != null)
            //    {
            //        return false;
            //    }
            //}

            if (Character.Self.IsNeedMove())
            {
                Vector3 toDir = Character.Self.GetPosition() - GetPosition();
                toDir.Normalize();
                float last = Vector3.Dot(toDir, Character.Self.mMoveComponent.mDirection);
                // Vector3.Distance(Character.Self.GetPosition(), GetPosition()) * Vector3.Dot(toDir, Character.Self.mMoveValue.mDirection);
                if (last <= 0)
                {
                    ObjectAI evt = StartAI("AI_FriendCheckFollow");
                    if (evt != null)
                        evt.DoEvent();

                    return false;
                }
            }
        }
        ObjectLocationForPet l = mFollowLocation as ObjectLocationForPet;

        Vector3 pos = l.Update(charTargetPos, charMoveDirection);       
        Vector3 p = GetPosition();
        
        float dis = Distance(pos.x, pos.z, p.x, p.z);
        if (dis < 0.2f)
            return false;

        //StopAI();

        //MoveTo(pos);
        //return true;

        AI_MoveTo ai;
        if (IsFollowInBossBattle())
            ai = StartMoveAI() as AI_MoveTo;
        else
            ai = StartAI("AI_FriendFollowMove") as AI_MoveTo; 
        if (ai != null)
        {
#if SHOW_FOLLOW_POSITION
                    mFollowPosFlag.transform.position = pos;
#endif
            ai.mTargetPos = pos;
            ai.DoEvent();

            return true;
        }
        return false;
    }

#if DEBUG_SKILL
    public override Skill Attack(BaseObject target)
    {
        return null;
    }
#endif

    public bool IsFollowInBossBattle()
    {
        BossBattle battle = MainProcess.mStage as BossBattle;
        if (battle != null)
            if (battle.mbForcePetFollow)
                return true;

        return false;
    }

    public override BaseObject FindAttackBoundEnemy()
    {
        BaseObject enemy = mOwnerObject.GetCurrentEnemy();
        if (enemy != null && !enemy.IsDead())
            return enemy;

        if (IsFollowInBossBattle())
            return null;

        return base.FindAttackBoundEnemy();
    }

    public override bool SetCurrentEnemy(BaseObject enemy)
    {
        if (enemy!=null && IsFollowInBossBattle())
            return false;

        return base.SetCurrentEnemy(enemy);
    }

    public override BaseObject GetCampEnemy()
    {
        // 张钟鸣确认, 宠物不考虑队友的敌人, 只有按自己还是主角的视野范围考虑
        return null;

        //if (mEnemy != null && !mEnemy.IsDead())
        //{
        //    return mEnemy;
        //}

        //BossBattle battle = MainProcess.mStage as BossBattle;
        //if (battle != null)
        //    if (!battle.mbAttackBoss)
        //        return null;

        //BaseObject campEnemy = null;

        //if (mOwnerObject != null)
        //{
        //    campEnemy = mOwnerObject.GetCampEnemy();
        //    if (campEnemy != null)
        //        return campEnemy;
        //    else
        //    {
        //        Character main = mOwnerObject as Character;
        //        foreach (BaseObject obj in main.mFriends)
        //        {
        //            if (obj != null && obj.GetCurrentEnemy() != null && !obj.GetCurrentEnemy().IsDead())
        //            {
        //                return obj.GetCurrentEnemy();
        //            }
        //        }
        //    }            
        //}

        //return null;
    }

	public override void OnDead()
	{
		base.OnDead();

        PlayEffect(3027, null);

        if (mMiniMapFlagData != null)
        {
            //mMiniMapFlagData.set ("DEAD", null);
            mMiniMapFlagData.onRemove();
            mMiniMapFlagData = null;
        }

		if(!mbIsPvpOpponent)
		{
	        tLogicData skillData = DataCenter.GetData("SKILL_UI");

	        if (mUsePos >= 1 && mUsePos <= 4)
	        {
	            if (skillData != null)
	            {
	                string key = "do_skill_" + mUsePos.ToString();
	                skillData.setData(key, "PET_LIFE_TIME_OVER", mbLifeTimeOver);
	                skillData.setData(key, "PET_DEAD", true);
	            }

	            if (mUsePos <= 3 && !mbLifeTimeOver)
	            {
	                ++MainProcess.mStage.mPetDeadCount;
	            }
	        }
		}

		DataCenter.SetData(mBattlePlayerWindow, "PET_DEAD", mUsePos);

        //PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        //int iPos = petLogicData.GetPosInCurTeam(mPetData.mDBID);
        //
        //if (iPos >= 1 && iPos <= 3)
        //{
        //    if (skillData != null)
        //    {
        //        string key = "do_skill_" + iPos.ToString();
        //        skillData.setData(key, "PET_DEAD", true);
        //    }
        //
		//    DataCenter.SetData(mBattlePlayerWindow, "PET_DEAD", iPos);
        //}
        //else
        //{
		//    DataCenter.SetData(mBattlePlayerWindow, "FRIEND_DEAD", iPos);
        //}        
	}


    //public void SetPetData(PetData petData)
    //{
    //    mPetData = petData;

    //    SetLevel(mPetData.level);
    //    SetBreakLevel(mPetData.breakLevel);

    //    SetAttributeByLevel();
    //}

	//----------------------------------------------
	// HP
    //public override int GetMaxHp()
    //{
    //    if(mPetData == null)
    //        return 1;
    //    int value = GetStaticMaxHp();
    //    int baseValue = GetBaseMaxHp();
    //    return value + (int)GetBufferAddValue(AFFECT_TYPE.HP_MAX) + (int)GetBufferAddValueByRate(baseValue, AFFECT_TYPE.HP_MAX_RATE);
    //}

    //public override int GetStaticMaxHp()
    //{
    //    return GetBaseMaxHp();
    //}

	public override void ChangeHp(int nHp)
	{
		if(!mDied && !mbIsUI)
		{
			base.ChangeHp(nHp);
			//PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			//int iNum = petLogicData.GetPetUseNumByPos(mPetData.mUsePos);
            //
			DataCenter.SetData(mBattlePlayerWindow, "PET_HP", this);
            //tLogicData d = DataCenter.Self.getData ("FRIEND_INFO_" + iNum.ToString());
            //if (d!=null)
            //{
            //    d.set ("MAX_HP", GetMaxHp());
            //    d.set ("HP", GetHp());
            //}
		}
	}


	// Attack
    //public override int GetBaseAttack()
    //{
    //    return GameCommon.GetBaseAttack(mConfigRecord, mLevel, mBreakLevel);
    //}

    //public override float GetStaticAttack()
    //{
    //    return GetBaseAttack();
    //}

    //public override float GetAttack()
    //{
    //    float value = GetStaticAttack();
    //    float baseValue = GetBaseAttack();
    //    return value + GetBufferAddValue(AFFECT_TYPE.ATTACK) + GetBufferAddValueByRate(baseValue, AFFECT_TYPE.ATTACK_RATE);
    //}

    public override void InitBufferUI(int iAffectConfig)
    {
		DataCenter.SetData(mBattlePlayerWindow, "START_PET_BUFF_" + mUsePos.ToString(), iAffectConfig);
    }

    public override void RemoveBufferUI(int iAffectConfig)
    {
		DataCenter.SetData(mBattlePlayerWindow, "REMOVE_PET_BUFF_" + mUsePos.ToString(), iAffectConfig);
    }

    public float GetLifeTimeSpeed(float baseTime)
    {
        double delta = mAffectPool.GetValue("LIFE_TIME");
        double rate = mAffectPool.GetValue("LIFE_TIME_RATE") / 100d;
        float realTime = Mathf.Max(0.001f, (float)(baseTime + delta + baseTime * rate));
        return baseTime / realTime;
    }

    //public override BaseObject FindPriEnemy(bool global)
    //{
    //    BaseObject ownerObj = GetOwnerObject();

    //    if (ownerObj == null || ownerObj.IsDead() || !this.IsSameCamp(ownerObj))
    //    {
    //        return FindPriEnemy(this, global);
    //    }
    //    else
    //    {
    //        return FindPriEnemy(ownerObj, global);
    //    }    
    //}
    public override BaseObject SearchEnemyByDefaultStrategy()
    {
        BaseObject priEnemy = null;
        float priSqrDis = Mathf.Infinity;
        float lookBounds = aiMachine.searchTargetCenter.LookBounds();
        var objMap = ObjectManager.Self.mObjectMap;

        for (int i = objMap.Length - 1; i >= 0; --i)
        {
            var obj = objMap[i];

            if (obj != null && IsEnemy(obj))
            {
                if (!aiMachine.searchTargetInGlobal && !aiMachine.searchTargetCenter.InLookBounds(obj))
                    continue;

                float d2 = AIKit.SqrDistance(GetPosition(), obj.GetPosition());

                if (priEnemy == null)
                {
                    priEnemy = obj;
                    priSqrDis = d2;
                }
                else
                {
                    int priEnemyPri = GetEnemyTypePri(priEnemy);
                    int currentPri = GetEnemyTypePri(obj);

                    if (currentPri > priEnemyPri || (currentPri == priEnemyPri && d2 < priSqrDis))
                    {
                        priEnemy = obj;
                        priSqrDis = d2;
                    }
                }
            }
        }

        return priEnemy;
    }

    //private BaseObject FindPriEnemy(BaseObject searchCenter, bool global)
    //{
    //    BaseObject priEnemy = null;
    //    float priSqrDis = Mathf.Infinity;
    //    float lookBounds = searchCenter.LookBound();

    //    foreach (var e in AIKit.AllEnemies(this))
    //    {
    //        if (!global && !AIKit.InBounds(searchCenter.GetPosition(), e.GetPosition(), lookBounds))
    //            continue;

    //        float d2 = AIKit.SqrDistance(GetPosition(), e.GetPosition());

    //        if (priEnemy == null)
    //        {
    //            priEnemy = e;
    //            priSqrDis = d2;
    //        }
    //        else 
    //        {
    //            int cmp = CompareEnemyType(priEnemy, e);

    //            if (cmp < 0 || (cmp == 0 && d2 < priSqrDis))
    //            {
    //                priEnemy = e;
    //                priSqrDis = d2;
    //            }
    //        }
    //    }

    //    return priEnemy;
    //}

    // 防御型 > 进攻型 > 主角 > 辅助型
    private int GetEnemyTypePri(BaseObject e)
    {
        if (e is Character)
            return 200;

        switch (e.fightintType)
        {
            case OBJECT_FIGHT_TYPE.DEFENSIVE:
                return 1000;
            case OBJECT_FIGHT_TYPE.OFFENSIVE:
                return 500;
            case OBJECT_FIGHT_TYPE.ASSISTANT:
                return 100;
            default:
                return 10;
        }
    }
}
//------------------------------------------------------------------------------


