using UnityEngine;
using System.Collections;
using DataTable;
using Utilities;

public class OpponentCharacter : Character
{
	static public OpponentCharacter mInstance;

	private PetData[] mPetData = new PetData[3];
    private PetFightDetail[] mPetDetails = new PetFightDetail[3];
	//private TotalTime[] mSkillCDTime = new TotalTime[4];
	//private TotalTime[] mSummonPetCDTime = new TotalTime[3];
	//private TotalTime[] mPetActiveCDTime = new TotalTime[3];
	//private TotalTime mSummonCDTime = new TotalTime();

	public OpponentCharacter() :base(true)
	{
		mInstance = this;
	}

	public override bool Init(int configIndex)
	{
		mBattlePlayerWindow = "OPPONENT_PLAYER_WINDOW";
		mbIsPvpOpponent = true;
		//GameCommon.RestoreCharacterConfigRecord ();
		base.Init(configIndex);
		//GameCommon.ChangeCharacterConfigRecord ();

		SetDirection(GetPosition() - Vector3.forward);

		return true;
	}

    public override void OnModelChanged()
    {
        base.OnModelChanged();

        GameObject mainBone = GameCommon.FindObject(mMainObject, "Bip01");
        if (mainBone == null)
            mainBone = mMainObject;

        if (mainBone != null)
        {
            mainBone.layer = CommonParam.MonsterLayer;
            MonsterFlag m = mainBone.AddComponent<MonsterFlag>();
            m.mMonster = this;
            SphereCollider s = mainBone.AddComponent<SphereCollider>();
            //s.radius = 1f;
            s.radius = mSelectRadius;
            s.isTrigger = true;
        }
    }

    //public override void InitShadow()
    //{
    //    if (GameCommon.CheckNeedShowAureole(mConfigIndex))
    //    {
    //        return;
    //    }
    //    CreateShadow("textures/shadow_enemy", 1.5f/*mBaseSelectRadius * CommonParam.shadowRadiusScale*/, 0.4f);
    //}

	public override void Start()
	{
		mRunEffect = PlayEffect(9025, null);
		mRunEffect.Show(false);
		
		//mLookRange = GetConfig("LOOKBOUND");
		
        //if (mShowHpBar != null)
        //    mShowHpBar.Init(msHpBarWidth, msHpBarHeight, Color.green, Color.yellow);
		
		base._Start ();
		
		//GameCommon.SetLayer(mMainObject, CommonParam.CharacterLayer);
        GameCommon.SetLayer(mMainObject, CommonParam.MonsterLayer);
        mMainObject.StartCoroutine(CreatePets());
	}

    public override void OnLockEnemy(BaseObject enemy) { }

    public override AIMachine CreateAIMachine()
    {
        return new AIMachine(this);
    }
    //public override ObjectAI StartAI()
    //{
    //    OnStartAI();
    //    //StartAIRoutine(new FightRoutine(this));
    //    StartAIRoutine(new AIMachine(this));
    //    return null;
    //}

	//public override void Update (float onceTime)
	//{
	//	base.Update (onceTime);
    //
	//	//CanInitFriendPet();
	//	
	//	for(int i = 0; i < 3; i++)
	//	{
	//		//SetPetLifeTimeUI(i);
	//		//JudgePeiIsDeadAndInitSummonPetCD (i);
	//	}
	//}

    private IEnumerator CreatePets()
    {
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < 3; ++i)
        {
            if (InitFriendPet(i) == null)
            {
                yield return null;
            }
            else 
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

	public void StartWar()
	{
		//for(int i = 0; i < 3; i++)
		//{
		//	if(mSummonPetCDTime[i] == null) 
		//	{
		//		mSummonPetCDTime[i] = new TotalTime();
		//		PetData data = mPetData[i];
		//		if(data != null)
		//			mSummonPetCDTime[i].Start (GetSummonPetCDByIndex (data.mModelIndex) / 5000);
		//	}
		//	if(mSkillCDTime[i] == null) mSkillCDTime[i] = new TotalTime();
		//	if(mPetActiveCDTime[i] == null) mPetActiveCDTime[i] = new TotalTime();
		//}
		//if(mSkillCDTime[3] == null) mSkillCDTime[3] = new TotalTime();
	}

	//private void SetPetLifeTimeUI(int pos)
	//{
	//	if(mFriends[pos] != null && mPetActiveCDTime[pos] != null)
	//	{
	//		float fRestTime = mPetActiveCDTime[pos].GetRestTime() / (GetPetActiveCDByIndex (mPetData[pos].mModelIndex) / 1000);
	//		DataCenter.SetData (mBattlePlayerWindow, "SET_PET_LIFETIME_" + (pos + 1).ToString (), fRestTime);
	//	}
	//}

	//private void JudgePeiIsDeadAndInitSummonPetCD(int pos)
	//{
	//	if(mFriends[pos] != null)
	//	{
	//		if(mPetActiveCDTime[pos].Update())
	//		{
	//			mSummonPetCDTime[pos].Start (GetSummonPetCDByIndex (mPetData[pos].mModelIndex) / 1000);
	//			PetIsDead(pos);
	//		}
	//		else if(!(mPetActiveCDTime[pos].Update()) && mFriends[pos].IsDead())
	//		{
	//			mSummonPetCDTime[pos].Start ((GetSummonPetCDByIndex (mPetData[pos].mModelIndex) + GetPunishTimeByIndex (mPetData[pos].mModelIndex)) / 1000);
	//			PetIsDead(pos);
	//		}
	//	}
	//}

	//private void PetIsDead(int pos)
	//{
	//	mFriends[pos].ChangeHp (-1000000000);
	//	mFriends[pos] = null;
	//}
    //
	public void SetPetData(PetData data, int pos)
	{
		if(pos > 2)
			return;

		mPetData[pos] = data;
	}

	public PetData GetPetData(int pos)
	{
		if(pos > 2)
			return null;
		
		return mPetData[pos];
	}

    public void SetPetDetail(PetFightDetail detail, int pos)
    {
        if (pos > 2)
            return;

        mPetDetails[pos] = detail;
    }

	//private void CanInitFriendPet()
	//{
	//	if(GetDistanceIsLessThanLookBound())
	//	{
	//		for(int i = 0; i < 3; i++)
	//		{
	//			PetData data = mPetData[i];
	//			if(data != null && mSummonPetCDTime[i].Update () && mFriends[i] == null && mSummonCDTime.Update ())
	//			{
	//				mSummonCDTime.Start (1.0f);
	//				mPetActiveCDTime[i].Start (GetPetActiveCDByIndex (data.mModelIndex) / 1000);
	//				InitFriendPet (i);
	//			}
	//		}
	//	}
	//}
    //
	private OpponentPet InitFriendPet(int pos)
	{
		if(pos > 2) 
			return null;

		PetData petData = mPetData[pos];

		if(petData != null)
		{
			OpponentPet obj = ObjectManager.Self.CreateObject(OBJECT_TYPE.OPPONENT_PET, petData.tid) as OpponentPet;
			obj.SetCamp(GetCamp ());
			obj.mUsePos = pos + 1;
			obj.SetActiveData (petData);
			obj.SetOwnerObject(this);
            InitFriendFollowPosition(obj, obj.mUsePos);      
			InitFriendValidPosition(obj, 10);
            obj.SetDirection(obj.GetPosition() + this.GetDirection() * Vector3.forward);

            int petSkillIndex = obj.GetConfig("PET_SKILL_1");

            if (petSkillIndex > 0)
            {
                ActivateCDLooper(petSkillIndex, (SKILL_POS)(pos + 1), obj.GetConfigSkillLevel(0), 0f);
            }
			
			mFriends[pos] = obj;
            var detail = mPetDetails[pos];

            obj.InitBaseAttribute();
            obj.InitStaticAttribute();

            obj.mStaticMaxHp = detail.hp;
            obj.mStaticAttack = detail.attack;
            obj.mStaticPhysicalDefence = detail.phyDef;
            obj.mStaticMagicDefence = detail.mgcDef;
            obj.mStaticCriticalStrikeRate = detail.criRt / 10000f;
            obj.mStaticDefenceCriticalStrikeRate = detail.dfCriRt / 10000f;
            obj.mStaticHitRate = detail.hitTrRt / 10000f;
            obj.mStaticDodgeRate = detail.gdRt / 10000f;
            obj.mStaticDamageEnhanceRate = detail.hitRt / 10000f;
            obj.mStaticDamageMitigationRate = detail.dfHitRt / 10000f;
            obj.mIsAttributeLocked = true;
            obj.SetAttributeByLevel();

            obj.Start();

			DataCenter.SetData(mBattlePlayerWindow, "PET_ALIVE", pos + 1);
			return obj;
		}
		return null;
	}
	
	//public override BaseObject FindNearestEnemy()
	//{       
//	//	if(GetDistanceIsLessThanLookBound())
	//	if(Character.Self != null)
	//		return Character.Self;
    //
	//	return base.FindNearestEnemy();
	//}

    //public override void SetCurCharactor()
    //{
    //    int opponentLevel = DataCenter.Get ("OPPONENT_CHARATER_LEVEL");
    //    if(opponentLevel <= 0)
    //        return;
		
    //    SetLevel(opponentLevel);
    //}

	//public override bool Attack(BaseObject target)
	//{
//	//	for(int i = 0; i < 3; i++)
//	//	{
//	//		PetData data = mPetData[i];
//	//		if(data != null && mSummonPetCDTime[i].Update () && mFriends[i] == null)
//	//		{
//	//			mPetActiveCDTime[i].Start (GetPetActiveCDByIndex (data.mModelIndex) / 1000);
//	//			InitFriendPet (i);
//	//			break;
//	//		}
//	//	}
    //
	//	for(int i = 0; i < 3; i++)
	//	{
	//		PetData data = mPetData[i];
    //
	//		if(data != null && mFriends[i] != null && !mFriends[i].IsDead () && mSkillCDTime[i].Update () && CanDoSkill (GetSkillRecord (data.mModelIndex)))
	//		{
	//			mSkillCDTime[i].Start (GetSkillCDByIndex(data.mModelIndex));
	//			return SkillAttack(GetSkilIndexByIndex(data.mModelIndex), target, null);
	//		}
	//	}
    //
	//	//role skill
	//	if(CanDoSkill (GetSkillRecord (mConfigIndex)) && mSkillCDTime[3].Update ())
	//	{
	//		mSkillCDTime[3].Start (GetSkillCDByIndex(mConfigIndex));
	//		return SkillAttack(GetSkilIndexByIndex(mConfigIndex), target, null);
	//	}
    //
	//	return base.Attack(target);
	//}
    //
	//public override void ApplyDamage(BaseObject attacker, Skill skill, int damage)
	//{
	//	int nowHp = GetHp();
	//	base.ApplyDamage(attacker, skill, damage);
	//}
	//
	//public override void RemoveAllSkillCD()
	//{
	//	for(int i= 0; i < 3; i++)
	//	{
	//		if(mSkillCDTime[i]!= null)
	//			mSkillCDTime[i].Start (0);
	//	}
	//}
	//public override void RemoveAllSummonCD()
	//{
	//	for(int i= 0; i < 3; i++)
	//	{
	//		if(mSummonPetCDTime[i]!= null)
	//			mSkillCDTime[i].Start (0);
	//	}
	//}

    //private bool GetDistanceIsLessThanLookBound()
    //{
    //	if(Character.Self != null)
    //	{
    //		float d = Vector3.Distance(GetPosition(), Character.Self.GetPosition());
    //		if ( d<= LookBound()) return true;
    //	}
    //
    //	return false;
    //}
    //
    //private DataRecord GetSkillRecord(int modelIndex)
    //{
    //	return DataCenter.mSkillConfigTable.GetRecord (GetSkilIndexByIndex(modelIndex));
    //}
    //
    //private float GetSkillCDByIndex(int iModelIndex)
    //{
    //	return (float)TableCommon.GetNumberFromSkillConfig (GetSkilIndexByIndex (iModelIndex), "CD_TIME");
    //}
    //
    //private float GetSummonPetCDByIndex(int iModelIndex)
    //{
    //	return (float)TableCommon.GetNumberFromActiveCongfig (iModelIndex, "SUMMON_TIME");
    //}
    //
    //private float GetPunishTimeByIndex(int iModelIndex)
    //{
    //	return (float)TableCommon.GetNumberFromActiveCongfig (iModelIndex, "PUNISH_TIME");
    //}
    //
    //private float GetPetActiveCDByIndex(int iModelIndex)
    //{
    //	return (float)TableCommon.GetNumberFromActiveCongfig (iModelIndex, "ACTIVE_TIME");
    //}
    //
    //private int GetSkilIndexByIndex(int iModelIndex)
    //{
    //	return TableCommon.GetNumberFromActiveCongfig (iModelIndex, "PET_SKILL_1");
    //}
}

//--------------------------------------------------------------------------------------------------
public class OpponentCharacterFactory : ObjectFactory
{
	public override OBJECT_TYPE GetObjectType() { return OBJECT_TYPE.OPPONENT_CHARACTER; }
	public override BaseObject NewObject()
	{
		return new OpponentCharacter();
	}
}
//--------------------------------------------------------------------------------------------------

public class OpponentPet : Pet 
{
	public override bool Init(int configIndex)
	{
		mBattlePlayerWindow = "OPPONENT_PLAYER_WINDOW";
		mbIsPvpOpponent = true;
		base.Init(configIndex);
		return true;
	}

    public override void OnModelChanged()
    {
        base.OnModelChanged();

        GameObject mainBone = GameCommon.FindObject(mMainObject, "Bip01");
        if (mainBone == null)
            mainBone = mMainObject;

        if (mainBone != null)
        {
            mainBone.layer = CommonParam.MonsterLayer;
            MonsterFlag m = mainBone.AddComponent<MonsterFlag>();
            m.mMonster = this;
            SphereCollider s = mainBone.AddComponent<SphereCollider>();
            //s.radius = 1f;
            s.radius = mSelectRadius;
            s.isTrigger = true;
        }
    }
	
	public override bool SetCurrentEnemy(BaseObject enemy)
	{
		return false;
	}
	
    //public override void InitShadow()
    //{
    //    if (GameCommon.CheckNeedShowAureole(mConfigIndex)) 
    //    {
    //        return;
    //    }
    //    CreateShadow("textures/shadow_enemy", 1.5f/*mBaseSelectRadius * CommonParam.shadowRadiusScale*/, 0.4f);
    //}

    public override AIMachine CreateAIMachine()
    {
        return new AIMachine(this);
    }
	
	//public override BaseObject FindNearestEnemy()
	//{       
	//	if(Character.Self != null)
	//	{
	//		return Character.Self ;
	//		
	//		//			float d = Vector3.Distance(GetPosition(), Character.Self.GetPosition());
	//		//			if ( d<= LookBound()) return Character.Self;
	//	}
	//	
	//	return base.FindNearestEnemy();
	//}
	
}

public class OpponentPetFactory : ObjectFactory
{
	public override OBJECT_TYPE GetObjectType() { return OBJECT_TYPE.OPPONENT_PET; }
	public override BaseObject NewObject()
	{
		return new OpponentPet();
	}
}
