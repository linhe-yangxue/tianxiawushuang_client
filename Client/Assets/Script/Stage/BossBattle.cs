using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

//-------------------------------------------------------------------------
enum eBOSS_STATE
{
    eBossActive,
    eBossDead,
    ebossTimeOver,
    eBossNoExist,
};
//-------------------------------------------------------------------------

public class BossBattleAccountInfo : BattleAccountInfo
{
    public int battleAchv = 0;
    public int merit = 0;
	public int finderTid;
}

//-------------------------------------------------------------------------
public class BossBattle : StageBattle 
{
    //public static BATTLE_CONTROL mBattleControl = BATTLE_CONTROL.MANUAL;
    public static BATTLE_SPEED mBattleSpeed = BATTLE_SPEED.X1;
    public float mMaxWinBattleTime = DataCenter.mGlobalConfig.GetData("FOUR_VS_FOUR_MAX_BATTLE_TIME", "VALUE");
    //static float msUpdateBossHpOnceTime = 2;

    public BigBoss mBoss;
    public int mBossID = -1;
    public int mBossCode = -1;
    public int mBossLevel = 0;
    public System.UInt64 mBossTime = 0;
    public int mBossHp = 1;
    public int mBossLeftHpAfterBattle = 0;
    public int mBattleDuration = 0;
    public int mBossQuality = 0;
    public bool mIsPowerAttack = false; // 是否全力一击

    //TotalTime mTotalTime = new TotalTime();

    public string mBossFinderDBID = "";
    public string mBossFinderName = "";
    public int mTotalResultDamage = 0;

	public int mIfShareWithFriend ;

	//public int mLastHp = 0;

    //public int mTotalNetTimeHp = 0;
    public int mMaxDamageToBoss = 10000;  // 单次战斗对Boss造成的伤害上限
    private bool mIsBattleTerminated = false;
    private float mRemainTime = 0f;

    static public void RegisterNetEvent()
    {
        Logic.EventCenter center = Net.gNetEventCenter;
        //center.RegisterEvent("CS_RequestDamageBoss", new DefineFactoryLog<CS_RequestDamageBoss>());
        //center.RegisterEvent("CS_RequestUpdateBoss", new DefineFactoryLog<CS_RequestUpdateBoss>());

        EventCenter.Self.RegisterEvent("TM_WaitToMoveCameraToMainChar", new DefineFactoryLog<TM_WaitToMoveCameraToMainChar>());
        EventCenter.Self.RegisterEvent("TM_WaitToStartAttackBoss", new DefineFactoryLog<TM_WaitToStartAttackBoss>());
		EventCenter.Self.RegisterEvent ("TM_WaitToBattleStartWindowEnd", new DefineFactoryLog<TM_WaitToBattleStartWindowEnd>());

        EventCenter.Self.RegisterEvent("Event_RequestBossBattle", new DefineFactoryLog<RequestBossBattleEvent>());
    }

    public override void InitBattleUI() 
	{
		BaseUI.OpenWindow("BOSS_BATTLE_WINDOW", null, false);
		ShowGuankaName();
	}

    public override void InitSetBirthPoint(MonoBehaviour birthPoint) 
	{
		if (Character.Self == null)
		{
            Vector3 pos = GetMainRoleBirthPoint(birthPoint.transform.position);
            CreateMainRole(pos, false);
		}
	}

    override public void OnStart()
    {
        if (MainUIScript.Self != null)
            MainUIScript.Self.DestroyAllMainWindow();

        DataCenter.CloseWindow("MAIN_CENTER_GROUP");

        DataCenter.SetData("BOSS_BATTLE_LOADING_WINDOW", "CONTINUE", true);

        //if (GuideManager.Notify(GuideIndex.EnterBossBattle))
        //{
        //    GuideManager.SetBossBattleSettingEnabled(false);
        //}

        //增加属性
        //ActiveData tmpData = TeamManager.GetActiveDataByTeamPos((int)TEAM_POS.CHARACTER);
        //Character.Self.mStaticAttack *= tmpData.breakLevel;
        //for (int i = (int)TEAM_POS.PET_1, count = (int)TEAM_POS.MAX; i < count; i++)
        //{
        //    tmpData = TeamManager.GetActiveDataByTeamPos(i);
        //    ActiveObject tmpActiveObj = Character.Self.mFriends[i - 1] as ActiveObject;
        //    if (tmpActiveObj == null || tmpActiveObj.IsDead())
        //        continue;
        //    tmpActiveObj.mStaticAttack *= tmpData.breakLevel;
        //}
    }

    public override void Start()
    {
        base.Start();
        if (mBossID < 0)
        {
            EventCenter.Log(LOG_LEVEL.ERROR, "No set boss id");
            return;
        }
       
        mBoss = ObjectManager.Self.CreateObject(OBJECT_TYPE.BIG_BOSS, mBossID) as BigBoss;
        mBoss.SetCamp(CommonParam.MonsterCamp);
		//mBoss.mLevel = mBossLevel;
        mBoss.SetQualityAndLevel(mBossQuality, mBossLevel);
        mBoss.mImmortal = mBossLeftHpAfterBattle > 0;
		float x = DataCenter.Get ("BOSS_POSITION_X");
		float z = DataCenter.Get ("BOSS_POSITION_Z");
        mBoss.InitPosition(new Vector3(x, 0, z));
        

        //mTotalTime.Start(msUpdateBossHpOnceTime);

        mBoss.Start();
		mBoss.SetHp(mBossHp);

		//mLastHp = mBossHp;

        AutoBattleAI.AddMonster(0, mBoss);
        //DataCenter.SetData("BOSS_HP", "MAX_HP", mBoss.GetMaxHp());
        //DataCenter.SetData("BOSS_HP", "HP", mBossHp);

        // Initiate Character's Atrribute for UI      
        System.Action initAttributeForUI = () =>
        {
            Character character = Character.Self;
            if (character != null)
            {
                character.SetLevel(RoleLogicData.GetMainRole().level);
                character.SetExp(RoleLogicData.GetMainRole().exp);
                character.SetAttributeByLevel();
            }
        };

        // Delay one frame wait for Character.Self initiation
        GuideManager.ExecuteDelayed(initAttributeForUI, 0f);

        CastCGThenDo(() =>
        {
            // Move camera to boss
            //tEvent evt = EventCenter.Start("TM_WaitToMoveCameraToMainChar");
            //MainProcess.mCameraMoveTool.MoveTo(mBoss.GetPosition(), evt);
            if (!mbBattleFinish)
            {
                tEvent evt = EventCenter.Start("TM_WaitToStartAttackBoss");
                evt.DoEvent();
            }
        });

        DataCenter.OpenWindow("BATTLE_SKIP_WINDOW");
    }

    protected override void OnActive()
    {
        if (/*mBattleControl*/GetBattleControl() != BATTLE_CONTROL.MANUAL)
        {
            //Time.timeScale = AIParams.autoBattleTimeScale;
            mHasAutoFighted = true;
        }

        Time.timeScale = GetTimeScale(mBattleSpeed);
        var intervencer = new BossBattleIntervencer();
        intervencer.bossBattle = this;
        intervencer.bossStartHp = mBossHp;
        intervencer.bossFinalHp = mBossLeftHpAfterBattle;
        intervencer.battleFinalTime = mBattleDuration;
        MainProcess.battleIntervencer = intervencer;
    }

    //public void SetServerBossHp(int bossHp)
    //{
    //    mTotalNetTimeHp += mLastHp - mBoss.GetHp();

    //    mBoss.SetHp(bossHp);
    //    mLastHp = mBoss.GetHp();
    //}

    //public override void Process(float delayTime)
    //{
    //    base.Process(delayTime);

    //    if (mBoss != null && mBossHp - mBoss.GetHp() > mMaxDamageToBoss && !mIsBattleTerminated)
    //    {
    //        mBoss.SetHp(mBossHp - mMaxDamageToBoss);

    //        if (!mIsBattleTerminated)
    //        {
    //            mIsBattleTerminated = true;
    //            TerminateBattle();
    //        }
    //    }
    //    if (mbBattleActive && !mbBattleFinish)
    //    {
    //        mBattleTime = mMaxWinBattleTime - (Time.time - mActiveTime);
    //        if (mBattleTime <= 0)
    //        {
    //            mBattleTime = 0;
    //            OnFinish(false);
    //        }
    //        else
    //            DataCenter.SetData("BATTLE_PLAYER_EXP_WINDOW", "TIME", mBattleTime);
    //    }
    //}

    protected override void OnProcess(float delayTime)
    {
		base.OnProcess (delayTime);

        if (mBoss != null && mBossHp - mBoss.GetHp() > mMaxDamageToBoss && !mIsBattleTerminated)
        {
            mBoss.SetHp(mBossHp - mMaxDamageToBoss);

            if (!mIsBattleTerminated)
            {
                mIsBattleTerminated = true;
                TerminateBattle();
            }
        }
    }

    protected override void OnBattleUI(float delayTime)
    {
		base.OnBattleUI(delayTime);
    }
    
    private void TerminateBattle()
    {
        if (Character.Self != null)
        {
            Character.Self.ChangeHp(-9999999);
        }
    }
    
    //public override void Process(float delayTime)
    //{
    //    base.Process(delayTime);

    //    if (!mbBattleFinish && mBoss != null && mTotalTime.Update())
    //    {
    //        mTotalTime.Start(msUpdateBossHpOnceTime);

    //        int damage = mLastHp - mBoss.GetHp() + mTotalNetTimeHp;
    //        mLastHp = mBoss.GetHp();
    //        mTotalNetTimeHp = 0;
    //        /*
    //        Logic.tEvent requestEvent;
    //        if (damage>0)
    //            requestEvent = Net.StartEvent("CS_RequestDamageBoss");
    //        else
    //            requestEvent = Net.StartEvent("CS_RequestUpdateBoss");
    //        requestEvent.set("BOSS_FINDER", mBossFinderDBID);
    //        requestEvent.set("BOSS_CODE", mBossCode);
    //        requestEvent.set("DAMAGE", damage);
            
    //        //requestEvent.set("BOSS_LEVEL", mBossLevel);

    //        requestEvent.DoEvent();
    //        */
    //    }
    //}

    protected override void OnStageFinish(bool bWin)
    {
        GuideManager.SetBossBattleSettingEnabled(false);
    }

    public override void SkipBattle()
    {
        if (mbBattleFinish)
        {
            SendBattleResult(mBossLeftHpAfterBattle <= 0);
        }
        else
        {
            mbSkipFinishAnim = true;

            if (mbBattleActive)
            {
                if (mBossLeftHpAfterBattle > 0)
                {
                    if (Character.Self != null)
                    {
                        Character.Self.ChangeHp(-Character.Self.GetHp() - 999999);
                    }
                }
                else
                {
                    if (mBoss != null)
                    {
                        mBoss.ChangeHp(-mBoss.GetHp() - 999999);
                    }
                }
            }

            OnFinish(mBossLeftHpAfterBattle <= 0);
        }
    }

    //protected override void OnRequestResult(bool bWin)
    //{
    //    TM_WaitToBossBattleResult e = EventCenter.Start("TM_WaitToBossBattleResult") as TM_WaitToBossBattleResult;
    //    e.mbWin = bWin;
    //    e.DoEvent();
    //}


    //public override void OnFinish(bool bWin)
    //{
    //    if (mbBattleFinish)
    //    {
    //        return;
    //    }

    //    OnStageFinish(bWin);

        

    //    //TODO
    //    /*
    //    if (!bWin)
    //    {
    //        DataCenter.OpenWindow("BOSS_BATTLE_LOSE_WINDOW");
    //    }
    //    else
    //        base.OnFinish(bWin);
    //        */
    //    //base.OnFinish(bWin);


    //    if (!MainProcess.isOnCheating)
    //    {
    //        TM_WaitToBossBattleResult e = EventCenter.Start("TM_WaitToBossBattleResult") as TM_WaitToBossBattleResult;
    //        e.mbWin = bWin;
    //        e.DoEvent();
    //    }
		
    //    //int lostHP = 0;
    //    //BossBattleStartData startData = DataCenter.Self.getObject("START_BOSS_BATTLE") as BossBattleStartData;
    //    //Boss_GetDemonBossList_BossData bossData = startData.bossData;
    //    //BossBattle bossBattle = MainProcess.mStage as BossBattle;
    //    //if(bossBattle != null)
    //    //    lostHP = bossData.hpLeft - bossBattle.mBoss.GetHp();
    //    //BossBattleNetManager.RequestDemonBossResult(lostHP, (int battleAchv, int merit) =>
    //    //{
    //    //    BossBattleAccountInfo info = new BossBattleAccountInfo();
    //    //    info.battleId = DataCenter.Get("CURRENT_STAGE");
    //    //    info.isWin = bWin;
    //    //    info.isSweep = false;
    //    //    info.starMask = ~0;
    //    //    info.starRate = 0;
    //    //    info.battleTime = mBattleTime;
    //    //    info.battleAchv = battleAchv;
    //    //    info.merit = merit;
    //    //    DataCenter.OpenWindow("BOSS_BATTLE_ACCOUNT_WINDOW", info);
    //    //});
    //}
    //    mbFinish = true;

    //    foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
    //    {
    //        if (obj != null)
    //        {
    //            if (bWin)
    //            {
    //                if (obj is Monster)
    //                    obj.Die();
    //            }
    //            else
    //            {
    //                obj.StopAI();
    //                obj.OnIdle();
    //            }
    //        }
    //    }
    //    StopAllSkill();

    //    BaseObject.mModelAddColor = new Color(0, 0, 0, 1);
    //    mbBattleFinish = true;
    //    float finishtime = Time.time;
    //    battletime = finishtime - starttime;
    //    if (bWin)
    //    {
    //        GameCommon.RandPlaySound(GetConfig("WIN_SOUND"), GameCommon.GetMainCamera().transform.position);
    //    }
    //    else
    //    {
    //        GameCommon.RandPlaySound(GetConfig("LOST_SOUND"), GameCommon.GetMainCamera().transform.position);
    //    }

    //    TM_WaitToBossBattleResult e = EventCenter.Start("TM_WaitToBossBattleResult") as TM_WaitToBossBattleResult;
    //    e.mbWin = bWin;
    //    e.DoEvent();
    //}

    protected override void _SendBattleResult(bool iswin)
    {
        int lostHP = 0;
        BossBattleStartData startData = DataCenter.Self.getObject("START_BOSS_BATTLE") as BossBattleStartData;
        Boss_GetDemonBossList_BossData bossData = startData.bossData;
        BossBattle bossBattle = MainProcess.mStage as BossBattle;

        if (bossBattle != null)
        {
            lostHP = bossData.hpLeft - startData.leftHpAfterBattle;//bossBattle.mBoss.GetHp();
            lostHP = Mathf.Min(lostHP, mMaxDamageToBoss);
        }

        BossBattleNetManager.RequestDemonBossResult(lostHP, (int battleAchv, int merit, int palyerTid) =>
        {
            BossBattleAccountInfo info = new BossBattleAccountInfo();
            info.battleId = DataCenter.Get("CURRENT_STAGE");
            info.isWin = iswin;
            if (iswin)
            {
                string lenth = GameCommon.GetDataByZoneUid("BOSS_LIST_LENTH");
                DEBUG.Log("BOSS_LIST_LENTH = " + lenth);
                if (lenth == "1")
                {
                    SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_BOSS, false);
                }
            }
            info.isSweep = false;
            info.starMask = ~0;
            info.starRate = 0;
            info.battleTime = mBattleTime;
            info.battleAchv = battleAchv;
            info.merit = merit;
			info.finderTid = palyerTid;
            DataCenter.OpenWindow("BOSS_BATTLE_ACCOUNT_WINDOW", info);
        });
    }

    protected override void OnCharacterStart(Character character)
    {
        base.OnCharacterStart(character);

        if (mIsPowerAttack/*DataCenter.Self.get("IS_BOSS_POWER_ATTACK") > 0*/)
        {
            character.mDamageScale = 2.5f * (character.mBreakLevel + 1);
        }
        else 
        {
            character.mDamageScale = 1f * (character.mBreakLevel + 1);
        }

        character.onPetsCreateDone += OnTeamReady;
    }

    protected override void OnPetStart(Pet pet)
    {
        base.OnPetStart(pet);

        if (mIsPowerAttack/*DataCenter.Self.get("IS_BOSS_POWER_ATTACK") > 0*/)
        {
            pet.mDamageScale = 2.5f * (pet.mBreakLevel + 1);
        }
        else
        {
            pet.mDamageScale = 1f * (pet.mBreakLevel + 1);
        }
    }

    private void OnTeamReady()
    {
        ShowBuffTip(Character.Self);

        for (int i = 0; i < Character.msFriendsCount; ++i)
        {
            var f = Character.Self.mFriends[i];

            if (f != null && !f.IsDead())
            {
                ShowBuffTip(f);
            }
        }
    }

    private void ShowBuffTip(BaseObject target)
    {
        var panel = UI_PaoPaoText.GetTextPanel();

        if (panel == null)
            return;

        var tip = GameCommon.LoadAndIntanciatePrefabs("Prefabs/battle_buff_effect_tip");

        if (tip != null)
        {
            ObjectMonitor.MakeDependency(tip, MainProcess.Self.gameObject);

            tip.transform.parent = panel.transform;
            tip.transform.localScale = Vector3.one * 2.5f;
            tip.transform.position = MainProcess.WorldToUIPosition(target.GetPosition());

            float powerScale = mIsPowerAttack ? 2.5f : 1f;
            float breakScale = (target is Role) ? (target as Role).mBreakLevel + 1f : 1f;
            float totalScale = powerScale * breakScale;

            var powerScaleLabel = GameCommon.FindComponent<UILabel>(tip, "power_attack");

            if (powerScaleLabel != null)
                powerScaleLabel.text = "全力一击 x " + powerScale.ToString();

            var breakScaleLabel = GameCommon.FindComponent<UILabel>(tip, "break_attack");

            if (breakScaleLabel != null)
                breakScaleLabel.text = "突破提升 x " + breakScale.ToString();

            var totalScaleLabel = GameCommon.FindComponent<UILabel>(tip, "total_attack");

            if (totalScaleLabel != null)
                totalScaleLabel.text = "伤害 x " + totalScale.ToString();
        }
    }
}

//-----------------------------------------------------------------------
//class CS_RequestDamageBoss : tServerEvent
//{
//    public override void _OnResp(tEvent respEvent)
//    {
//        respEvent.Dump();

//        int result = respEvent.get("RESULT");
//        if (result == (int)eBOSS_STATE.eBossActive)
//        {
//            int damage = respEvent.get("DAMAGE_HP");
//            int hp = respEvent.get("BOSS_HP");
//            BossBattle battle = MainProcess.mStage as BossBattle;
//            if (battle != null && !battle.mbBattleFinish)
//            {
//                battle.mTotalResultDamage = (int)respEvent["TOTAL_DAMAGE"];
//                if (battle.mBoss != null)                                   
//                    battle.SetServerBossHp(hp);
//                if (hp <= 0)
//                {
//                    MainProcess.mStage.OnFinish(true);
//                }
//                NiceTable damageList = respEvent.getObject("ATTACK_LIST") as NiceTable;
//                if (damageList != null)
//                    DataCenter.SetData("BOSS_ATTACK_INFO_WINDOW", "REFRESH", damageList);
//            }
//        }
//        else
//        {
//            // boss gone alway
//            MainProcess.mStage.OnFinish(false);
//            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_BOSS_RAIN_RUN);
//        }
//    }
//}
////-------------------------------------------------------------------------
//class CS_RequestUpdateBoss : tServerEvent
//{
//    public override void _OnResp(tEvent respEvent)
//    {
//        respEvent.Dump();

//        if ((int)eBOSS_STATE.eBossActive == (int)respEvent.get("RESULT"))
//        {
//            int hp = respEvent.get("BOSS_HP");
//            BossBattle battle = MainProcess.mStage as BossBattle;
//            if (battle != null && !battle.mbBattleFinish)
//            {
//                battle.SetServerBossHp(hp);
//                if (hp <= 0)
//                {
//                    MainProcess.mStage.OnFinish(true);
//                }
//                NiceTable damageList = respEvent.getObject("ATTACK_LIST") as NiceTable;
//                if (damageList!=null)
//                    DataCenter.SetData("BOSS_ATTACK_INFO_WINDOW", "REFRESH", damageList);
//            }
//        }
//        else
//        {
//            // boss gone alway
//            MainProcess.mStage.OnFinish(false);
//            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_BOSS_RAIN_RUN);
//        }
//    }
//}

//-------------------------------------------------------------------------

class TM_WaitToBossBattleResult : TM_WaitToBattleResult
{
    //public override void _OnOverTime()
    //{
    //    Finish();
    //    //MainProcess.mStage.BattleResult(mbWin);

    //    DataCenter.SetData("BATTLE_UI", "OPEN", true);
    //}
}

//-------------------------------------------------------------------------
class TM_WaitToMoveCameraToMainChar : TM_WaitToBattleResult
{
    public override bool _DoEvent()
    {
        WaitTime(0.5f);        
		return true;
    }

    public override void _OnOverTime()
    {
        tEvent evt = EventCenter.Start("TM_WaitToStartAttackBoss");
        MainProcess.mCameraMoveTool.MoveTo(Character.Self.GetPosition(), evt);
    }
}
//-------------------------------------------------------------------------
class TM_WaitToStartAttackBoss : TM_WaitToPlayPveBeginTextEffect
{
    public override bool _DoEvent()
    {
		base._DoEvent ();
        Character.Self.Start();

		return true;
    }

	public override void DoOverTime()
    {
        bool inBossBattle = MainProcess.mStage != null && MainProcess.mStage.IsBOSS();

        // 天魔战由于要显示加成效果，故不再显示倒计时
        if (!inBossBattle)
        {
            DataCenter.OpenWindow("BATTLE_START_WINDOW");
            tWindow battleStartWindow = DataCenter.GetData("BATTLE_START_WINDOW") as tWindow;
            GlobalModule.DoLater(() => GameCommon.FindObject(battleStartWindow.mGameObjUI, "Texture2").SetActive(true), 0.7f);
            GlobalModule.DoLater(() => GameCommon.FindObject(battleStartWindow.mGameObjUI, "Texture1").SetActive(true), 1.2f);
            GlobalModule.DoLater(() => GameCommon.FindObject(battleStartWindow.mGameObjUI, "Texturestart").SetActive(true), 1.6f);

            if (battleStartWindow != null) MonoBehaviour.Destroy(battleStartWindow.mGameObjUI, 3.0f);
        }

		tEvent evt = EventCenter.Start("TM_WaitToBattleStartWindowEnd");

        // 天魔战要等加成效果显示完，故延迟时间较长
        GlobalModule.DoLater(() => evt.DoEvent(), inBossBattle ? 1f : 0f);

        DataCenter.OpenWindow("MASK_OPERATE_WINDOW");
    }
}

//-------------------------------------------------------------------------
class TM_WaitToBattleStartWindowEnd : TM_WaitToShowAutoBattleButton
{
	public override bool _DoEvent()
	{
		base._DoEvent ();
		return true;
	}
	
	public override void DoOverTime()
	{        
        if (MainProcess.mStage != null)
        {
            //if(MainProcess.mStage is PVP4Battle)
            //    MainProcess.mStage.StartWar ();
            MainProcess.mStage.ActivateBattle();
        }

        //tLogicData skillData = DataCenter.Self.getData("SKILL_UI");

        //if (skillData != null)
        //{
        //    for (int i = 1; i <= 4; ++i)
        //    {
        //        tLogicData d = skillData.getData("do_skill_" + i);

        //        if (d != null)
        //        {
        //            //d.set("START_SUMMON_CD", i == 4 ? 0f : 0.2f);
        //        }
        //    }
        //}
	}
}
//-------------------------------------------------------------------------
