using UnityEngine;
using System.Collections;
using DataTable;
using Utilities.Routines;

//-------------------------------------------------------------------------
public class MonsterFlag : MonoBehaviour
{
    public ActiveObject mMonster;
}
//------------------------------------------------------------------------------
public class Monster : ActiveObject
{

    public tLogicData mMiniMapFlagData;
    public int mGroup = 0;      // 组别，用于实现怪物的分批创建

    // 覆盖属性，用于被召唤怪物对召唤者的属性继承，0表示不覆盖
    //public int mOverrideAttack = 0;
    //public int mOverrideMaxhp = 0;

    //ObjectEvent mOutlineEvt;

    public Monster()
    {
    }

    //public override bool Init(int configIndex)
    //{
    //    if (base.Init(configIndex))
    //    {
    //        CapsuleCollider monsterColl = mMainObject.AddComponent<CapsuleCollider>();
    //        monsterColl.center = new Vector3(0, 1, 0);
    //        monsterColl.radius = 1d.7f;
    //        monsterColl.height = 2;
    //        monsterColl.isTrigger = true;

    //        return true;
    //    }
    //    return false;
    //}

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

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        StartFadeInOrOut(1, true);

        mMainObject.name = GetID().ToString() + "_Monster";

        //if (GetOwner() != Character.Self.GetOwner())
        mMainObject.layer = CommonParam.MonsterLayer;

    }

    public override void OnStartAI()
    {
        if (mMiniMapFlagData == null)
        {
            object obj;
            bool bisExist = DataCenter.Self.getData("MINI_MAP", out obj);
            if (bisExist)
            {
                tLogicData d = obj as tLogicData;
                mMiniMapFlagData = d.getData("CREATE");
            }
        }
    }

    //public override ObjectAI StartAI()
    //{
    //    OnStartAI();

    //    //ObjectAI ai;
    //    //ai = StartAI("AI_MonsterReadyIdle");        
    //    //
    //    //ai.DoEvent();
    //    //return ai;
    //    //StartAIRoutine(new FightRoutine(this));
    //    StartAIRoutine(new AIMachine(this));
    //    return null;
    //}

    // Update is called once per frame
    public override void Update(float onceTime)
    {
        base.Update(onceTime);
        if (mMiniMapFlagData == null)
            return;
        mMiniMapFlagData.set("POS", GetPosition());
        mMiniMapFlagData.set("SHOW", GetPosition());

		if(Character.Self != null && Character.Self.mAutoBattleAI != null && Character.Self.mAutoBattleAI.GetNextAttackMonster () != null && mMainObject == Character.Self.mAutoBattleAI.GetNextAttackMonster ().mMainObject)
			mMiniMapFlagData.set("ROTATION_ROUND", GetPosition());
    }

    public override void OnPositionChanged()
    {
        base.OnPositionChanged();
	
        if (Character.Self != null && Character.Self.GetCurrentEnemy() == this)
			CameraMoveEvent.OnTargetObjectMove(Character.Self);

        if (mMiniMapFlagData != null)
            mMiniMapFlagData.set("POS", GetPosition());
    }

    //public override float LookBound() { return 7; }

    public override float AttackTime() { return 3; }

    public override float HitTime() { return 1.5f; }

    //public override int TotalDamage(BaseObject targetObject, int damageValue, Skill skill)
    //{
    //    //return damageValue + GetConfig("ATTACK_DAMAGE"); 
    //    return GetConfig("ATTACK_DAMAGE");
    //}

    public override void OnDead()
    {
        if (mOutlineEvt != null)
        {
            mOutlineEvt.Finish();
            mOutlineEvt = null;
        }

        base.OnDead();

        //PlayEffect(3027, null);

        if (mMiniMapFlagData != null)
        {
            //mMiniMapFlagData.set ("DEAD", null);
            mMiniMapFlagData.onRemove();
            mMiniMapFlagData = null;
        }

        Drop();

		AddCharactorAttribute();

        if (AutoBattleAI.InAutoBattle())
            Character.Self.mCurrentAutoTargetAI.CheckOnMonsterDead(this);

        //tLogicData battleData = DataCenter.GetData("BATTLE");

        //int monsterCount = battleData.get("MONSTER_COUNT");
        //--monsterCount;
        //battleData.set("MONSTER_COUNT", monsterCount);
        //if (monsterCount <= 0)
        //{
        //    tLogicData uiBattle = DataCenter.GetData("BATTLE_UI");
        //    uiBattle.set("OPEN", true);
        //}
    }

#if DEBUG_SKILL
    public override Skill Attack(BaseObject target)
    {
        return null;
    }
#endif


    public virtual void Drop()
    {
        int dropHp = GetConfig("DROP_HP");
        if (dropHp > 0)
        {
            int dropHpMax = GetConfig("DROP_HP_MAX_NUM");
            for (int i = 0; i < dropHpMax; i++)
            {
                BaseEffect effect = BaseEffect.CreateEffect(this, null, 3002);
                effect.SetPosition(MathTool.RandPosInCircle(GetPosition(), 0.5f));
                effect.set("DROP_HP", dropHp);
            }
        }

        int dropMp = GetConfig("DROP_MP");
        if (dropMp > 0)
        {
            int dropHpMax = GetConfig("DROP_MP_MAX_NUM");
            for (int i = 0; i < dropHpMax; i++)
            {
                BaseEffect effect = BaseEffect.CreateEffect(this, null, 3003);
                effect.SetPosition(MathTool.RandPosInCircle(GetPosition(), 0.5f));
                effect.set("DROP_MP", dropMp);
            }
        }

        //DropGold();
        //DropItem();
    }

    //public virtual void DropGold()
    //{
    //    int stageIndex = DataCenter.Get("CURRENT_STAGE");
    //    int dropGold = TableCommon.GetNumberFromStageConfig(stageIndex, "MONSTER_MONEY");

    //    if (dropGold > 0)
    //    {
    //        Character.Self.AddGold(dropGold);
    //        BaseEffect effect = BaseEffect.CreateEffect(this, null, 3029);
    //        effect.SetPosition(MathTool.RandPosInCircle(GetPosition(), 1f));
    //        effect.set("DROP_GOLD", dropGold);
    //    }
    //}

    //public virtual void DropItem()
    //{
    //    if (MainProcess.mStage != null && MainProcess.mStage.CheckCanDropItem(mConfigRecord))
    //    {
    //        BaseEffect effect = BaseEffect.CreateEffect(this, null, 3028);
    //        effect.SetPosition(MathTool.RandPosInCircle(GetPosition(), 1f));
    //        effect.set("DROP_ITEM", 1);
    //    }
    //}

	public virtual void AddCharactorAttribute()
	{
		int stageIndex = DataCenter.Get("CURRENT_STAGE");

		if(Character.Self != null)
		{
			//Character.Self.AddTotalExp(TableCommon.GetNumberFromStageConfig(stageIndex, "MONSTER_EXP"));
			//Character.Self.AddExp(TableCommon.GetNumberFromStageConfig(stageIndex, "MONSTER_EXP"));

	        if (GetObjectType() == OBJECT_TYPE.MONSTER)
	            Character.Self.mKillMonster++;
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


    public override void OnStartAttackEnemy(BaseObject enemy) 
    {
        if (enemy != null)
            enemy.SetCurrentEnemy(this);
    }
    

	//----------------------------------------------
	// HP
    //public override int GetMaxHp()
    //{
    //    if (mOverrideMaxhp > 0)
    //    {
    //        return mOverrideMaxhp + (int)GetBufferAddValue(AFFECT_TYPE.HP_MAX) + (int)GetBufferAddValueByRate(mOverrideMaxhp, AFFECT_TYPE.HP_MAX_RATE);
    //    }
    //    else
    //    {
    //        int value = GetStaticMaxHp();
    //        int baseValue = GetBaseMaxHp();
    //        return value + (int)GetBufferAddValue(AFFECT_TYPE.HP_MAX) + (int)GetBufferAddValueByRate(baseValue, AFFECT_TYPE.HP_MAX_RATE);
    //    }
    //}

    //public override int GetBaseMaxHp()
    //{
    //    if (mOverrideMaxhp > 0)
    //        return mOverrideMaxhp;
    //    else
    //        return GameCommon.GetBaseMaxHP(mConfigRecord, mLevel, 0);
    //}

    //public override int GetStaticMaxHp()
    //{
    //    if (mOverrideMaxhp > 0)
    //        return mOverrideMaxhp;
    //    else
    //        return GetBaseMaxHp();
    //}

	// Attack
    //public override int GetBaseAttack()
    //{
    //    if (mOverrideAttack > 0)
    //        return mOverrideAttack;
    //    else
    //        return GameCommon.GetBaseAttack(mConfigRecord, mLevel, 0);
    //}

    //public override float GetStaticAttack()
    //{
    //    if (mOverrideAttack > 0)
    //    {
    //        return mOverrideAttack;
    //    }
    //    else
    //    {
    //        return GameCommon.GetBaseAttack(mConfigRecord, mLevel, 0);
    //    }
    //}

    //public override float GetAttack()
    //{
    //    if (mOverrideAttack > 0)
    //    {
    //        return mOverrideAttack + GetBufferAddValue(AFFECT_TYPE.ATTACK) + GetBufferAddValueByRate(mOverrideAttack, AFFECT_TYPE.ATTACK_RATE);
    //    }
    //    else
    //    {
    //        float value = GetStaticAttack();
    //        float baseValue = GetBaseAttack();
    //        return value + GetBufferAddValue(AFFECT_TYPE.ATTACK) + GetBufferAddValueByRate(baseValue, AFFECT_TYPE.ATTACK_RATE);
    //    }
    //}

	public override void OnHit(string hitAnim, BaseObject attacker)
	{
//		SetLightColor (Color.white);
        if(!IsDead())
		    SetWhiteColorOnHit();

		base.OnHit(hitAnim, attacker);
	}

    static float msHeightLightTime = 0.2f;
	public virtual void SetWhiteColorOnHit()
	{
        if (mOutlineEvt == null || !(mOutlineEvt is Object_HeightLight))
		{
			if (mFadeInOrOutEvent != null)
				mFadeInOrOutEvent.Finish();

            mOutlineEvt = Logic.EventCenter.Start("Object_HeightLight") as ObjectEvent;
			mOutlineEvt.SetOwner(this);

			mOutlineEvt.DoEvent();
            mOutlineEvt.WaitTime(msHeightLightTime);
			return;
		}

        if (mOutlineEvt.GetFinished())
        {
            mOutlineEvt.SetFinished(false);
            mOutlineEvt.DoEvent();
            mOutlineEvt.WaitTime(msHeightLightTime);
        }
        else
            mOutlineEvt.WaitTime(msHeightLightTime);
	}

    protected override void InitSkillOnStart(int fieldIndex, int skillIndex)
    {
        float initCD = mConfigRecord["PET_CD_" + (fieldIndex + 1)];
        ActivateCDLooper(skillIndex, SKILL_POS.SPECIAL_ATTACK, GetConfigSkillLevel(fieldIndex), initCD);
    }
}


//------------------------------------------------------------------------------
public class MonsterFactory : ObjectFactory
{
    public override OBJECT_TYPE GetObjectType() { return OBJECT_TYPE.MONSTER; }
    public override BaseObject NewObject()
    {
        return new Monster();
    }
    public override NiceTable GetConfigTable() { return DataCenter.mMonsterObject; }
}


public class MonsterBoss : Monster
{
    public Logic.tEvent[] mSkillList;

    public MonsterBoss()
    {
        mNeedVerifyMD5 = true;
    }

    public override void OnInitModelObject()
    {
        base.OnInitModelObject();

        ShowOutline(true);

        string skillConfig = GetConfig("SKILL");
        if (skillConfig != "")
        {
            char[] pa = { ' ' };
            string[] skillList = skillConfig.Split(pa);

            mSkillList = new Logic.tEvent[skillList.Length];
            for (int i = 0; i < skillList.Length; ++i)
            {
                int skillIndex = int.Parse(skillList[i]);
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

    public override bool Attack(BaseObject target)
    {
#if DEBUG_SKILL
        return null;
#else
		if (mSkillList!=null && mSkillList.Length>0)
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
#endif
    }

    //public override void OnHit(string hitAnim, BaseObject attacker)
    //{
    //    if(!IsDead())
    //        SetWhiteColorOnHit();

    //    if (mCurrentAI != null && !mCurrentAI.GetFinished() && mCurrentAI is Skill)
    //        return;

    //    if (Random.Range(0, 3) == 1)
    //    {
    //        SetDirection(attacker.GetPosition());
    //        PlayAnim(hitAnim);
    //    }
    //}

    public override void ApplyDamage(BaseObject attacker, Skill skill, int damage)
    {
        int nowHp = GetHp();
        base.ApplyDamage(attacker, skill, damage);

        if (!IsDead())
        {
            int hpSkill = GetConfig("HP_SKILL");
            if (hpSkill > 0)
            {
                int rate = GetConfig("BELOW_HP_RATE");
                int limit = (int)((float)rate / 100 * GetMaxHp());
                if (nowHp >= limit)
                {
                    if (GetHp() < limit)
                    {
                        SkillAttack(hpSkill, GetCurrentEnemy(), null);
                    }
                }
            }
        }
    }


    //public override void OnDead()
    //{
    //    base.OnDead();

    //    MainProcess.mStage.OnFinish(true);
    //    //StartRoutine(new SlowTime(2f));
    //}

    private class SlowTime : Routine
    {
        public SlowTime(float realTime)
        {
            Bind(DoSlow(realTime));
        }

        private IEnumerator DoSlow(float realTime)
        {
            Time.timeScale = 0.5f;
            yield return new Delay(realTime * Time.timeScale);
            Time.timeScale = 1f;
        }

        protected override void OnBreak()
        {
            Time.timeScale = 1f;
        }
    }

    public override void ChangeHp(int nHp)
    {
        base.ChangeHp(nHp);

        DataCenter.SetData("BATTLE_BOSS_HP_WINDOW", "HP", GetHp());
    }

    //public override void AddCharactorAttribute()
    //{
    //    int stageIndex = DataCenter.Get("CURRENT_STAGE");
    //    Character.Self.AddTotalExp(TableCommon.GetNumberFromStageConfig(stageIndex, "BOSS_EXP"));
    //    Character.Self.AddExp(TableCommon.GetNumberFromStageConfig(stageIndex, "BOSS_EXP"));
    //}

    // Use this for initialization
//    public override void Start()
//    {
//        base.Start();

//        //GuideManager.Notify(GuideIndex.EncounterBoss); 

////		ShowInfoUI();
//    }

	public void ShowInfoUI()
	{
		DataCenter.OpenWindow("BATTLE_BOSS_HP_WINDOW");

        if (!Guide.inPrologue)
        {
            DataCenter.OpenWindow("PVE_BOSS_APPEAR_WINDOW");
            tWindow t = DataCenter.GetData("PVE_BOSS_APPEAR_WINDOW") as tWindow;
            if (t != null) MonoBehaviour.Destroy(t.mGameObjUI, 2f);
        }
		
		DataCenter.SetData("BATTLE_BOSS_HP_WINDOW", "MAX_HP", GetMaxHp());
		DataCenter.SetData("BATTLE_BOSS_HP_WINDOW", "HP", GetMaxHp());

        PVEStageBattle stage = MainProcess.mStage as PVEStageBattle;

        if (stage != null)
        {
            stage.OnBossActive();
        }
	}

    public override void InitShadow()
    {
        mShadow = CreateShadow("textures/shadow_boss", 3f/*mBaseSelectRadius * CommonParam.shadowRadiusScale*/, 0.6f);
    }

    //public override void DropGold()
    //{
    //    int stageIndex = DataCenter.Get("CURRENT_STAGE");
    //    int dropGold = TableCommon.GetNumberFromStageConfig(stageIndex, "BOSS_MONEY");

    //    if (dropGold > 0)
    //    {
    //        Character.Self.AddGold(dropGold);
    //        BaseEffect effect = BaseEffect.CreateEffect(this, null, 3003);
    //        effect.SetPosition(MathTool.RandPosInCircle(GetPosition(), 0.5f));
    //        effect.set("DROP_GOLD", dropGold);
    //    }
    //}

}
//------------------------------------------------------------------------------
public class MonsterBossFactory : MonsterFactory
{
    public override OBJECT_TYPE GetObjectType() { return OBJECT_TYPE.MONSTER_BOSS; }
    public override BaseObject NewObject()
    {
        return new MonsterBoss();
    }
}
//-------------------------------------------------------------------------
