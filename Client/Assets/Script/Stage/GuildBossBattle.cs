using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

//-------------------------------------------------------------------------
enum eGuildBOSS_STATE
{
    eBossActive,
    eBossDead,
    ebossTimeOver,
    eBossNoExist,
};
//-------------------------------------------------------------------------

public class GuildBossBattleAccountInfo : BattleAccountInfo
{
    public int battleAchv = 0;
    public int merit = 0;
}

//-------------------------------------------------------------------------
public class GuildBossBattle : StageBattle
{
    public static BATTLE_CONTROL mBattleControl = BATTLE_CONTROL.MANUAL;
    public static BATTLE_SPEED mBattleSpeed = BATTLE_SPEED.X1;

    public MonsterBoss mBoss;
    public int mBossID = -1;
    public int mBossCode = -1;
    public int mBossLevel = 0;
    public System.UInt64 mBossTime = 0;
    public int mBossHp = 1;
    public int mTotalResultDamage = 0;
    public int mMaxDamageToBoss = 10000;  // 单次战斗对Boss造成的伤害上限
    private bool mIsBattleTerminated = false;


    static public void RegisterNetEvent()
    {
        Logic.EventCenter center = Net.gNetEventCenter;

        EventCenter.Self.RegisterEvent("TM_WaitToMoveCameraToMainCharGuildBoss", new DefineFactoryLog<TM_WaitToMoveCameraToMainCharGuildBoss>());
        EventCenter.Self.RegisterEvent("TM_WaitToStartAttackGuildBoss", new DefineFactoryLog<TM_WaitToStartAttackGuildBoss>());
        EventCenter.Self.RegisterEvent("TM_WaitToBattleStartWindowEndGuildBoss", new DefineFactoryLog<TM_WaitToBattleStartWindowEndGuildBoss>());
        EventCenter.Self.RegisterEvent("Event_RequestGuildBossBattle", new DefineFactoryLog<RequestBossBattleEvent>());
        EventCenter.Self.RegisterEvent("TM_WaitToGuildBossBattleResult", new DefineFactoryLog<TM_WaitToGuildBossBattleResult>());
    }

    public override BATTLE_CONTROL GetBattleControl()
    {
        return GuildBossBattle.mBattleControl;
    }

    public override void SetBattleControl(BATTLE_CONTROL control)
    {
        GuildBossBattle.mBattleControl = control;
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

        if (GuideManager.Notify(GuideIndex.EnterBossBattle))
        {
            GuideManager.SetBossBattleSettingEnabled(false);
        }

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

        mBoss = ObjectManager.Self.CreateObject(OBJECT_TYPE.MONSTER_BOSS, mBossID) as MonsterBoss;
        mBoss.SetCamp(CommonParam.MonsterCamp);
        mBoss.mLevel = mBossLevel;
        float x = DataCenter.Get("BOSS_POSITION_X");
        float z = DataCenter.Get("BOSS_POSITION_Z");
        mBoss.InitPosition(new Vector3(x, 0, z));


        //mTotalTime.Start(msUpdateBossHpOnceTime);

        mBoss.Start();     
        mBoss.SetHp(mBossHp);

        DataCenter.SetData("BATTLE_BOSS_HP_WINDOW", "MAX_HP", mBoss.GetMaxHp());
        DataCenter.SetData("BATTLE_BOSS_HP_WINDOW", "HP", mBossHp);

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
            //tEvent evt = EventCenter.Start("TM_WaitToMoveCameraToMainCharGuildBoss");
            //MainProcess.mCameraMoveTool.MoveTo(mBoss.GetPosition(), evt);
            tEvent evt = EventCenter.Start("TM_WaitToStartAttackGuildBoss");
            evt.DoEvent();
        });
    }

    protected override void OnActive()
    {
        if (/*mBattleControl*/GetBattleControl() != BATTLE_CONTROL.MANUAL)
        {
            //Time.timeScale = AIParams.autoBattleTimeScale;
            mHasAutoFighted = true;
        }

        Time.timeScale = GetTimeScale(mBattleSpeed);
    }

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
    //}

    protected override void OnProcess(float delayTime)
    {
        base.OnProcess(delayTime);
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

    private void TerminateBattle()
    {
        if (Character.Self != null)
        {
            Character.Self.ChangeHp(-9999999);
        }
    }

    protected override void OnStageFinish(bool bWin)
    {
        GuideManager.SetBossBattleSettingEnabled(false);
    }

    //protected override void OnRequestResult(bool bWin)
    //{
    //    TM_WaitToGuildBossBattleResult e = EventCenter.Start("TM_WaitToGuildBossBattleResult") as TM_WaitToGuildBossBattleResult;
    //    e.mbWin = bWin;
    //    e.DoEvent();
    //}

    //public override void OnFinish(bool bWin)
    //{
    //    //LuaBehaviour.ClearObjects(false);
        
    //    mbBattleFinish = true;
    //    Time.timeScale = 1f;
    //    mbSucceed = bWin;
    //    mHPRateOnFinish = bWin ? (float)Character.Self.GetHp() / Character.Self.GetMaxHp() : 0f;

    //    DisableAI();
    //    ClearBattle(bWin);

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


    //    BaseObject.mModelAddColor = new Color(0, 0, 0, 1);
    //    float finishtime = Time.time;
    //    mBattleTime = finishtime - mActiveTime;

    //    if (mOnStageFinish != null)
    //    {
    //        mOnStageFinish();
    //    }

    //    if (bWin)
    //    {
    //        GameCommon.RandPlaySound(GetConfig("WIN_SOUND"), GameCommon.GetMainCamera().transform.position);
    //    }
    //    else
    //    {
    //        GameCommon.RandPlaySound(GetConfig("LOST_SOUND"), GameCommon.GetMainCamera().transform.position);
    //    }

    //    GameCommon.RemoveBackgroundSound();

    //    TM_WaitToGuildBossBattleResult e = EventCenter.Start("TM_WaitToGuildBossBattleResult") as TM_WaitToGuildBossBattleResult;
    //    e.mbWin = bWin;
    //    e.DoEvent();

    //    //int lostHP = 0;
    //    //BossBattleStartData startData = DataCenter.Self.getObject("START_BOSS_BATTLE") as BossBattleStartData;
    //    //Boss_GetDemonBossList_BossData bossData = startData.bossData;
    //    //BossBattle bossBattle = MainProcess.mStage as BossBattle;
    //    //if(bossBattle != null)
    //    //    lostHP = bossData.hpLeft - bossBattle.mBoss.GetHp();
    //    //BossBattleNetManager.RequestDemonBossResult(lostHP, (int battleAchv, int merit) =>
    //    //{
    //    //    GuildBossBattleAccountInfo info = new GuildBossBattleAccountInfo();
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

    //    TM_WaitToGuildBossBattleResult e = EventCenter.Start("TM_WaitToGuildBossBattleResult") as TM_WaitToGuildBossBattleResult;
    //    e.mbWin = bWin;
    //    e.DoEvent();
    //}

    protected override void _SendBattleResult(bool iswin)
    {
        GuildBossBattleAccountInfo info = new GuildBossBattleAccountInfo();
        info.battleId = mConfigIndex;
        info.isWin = iswin;
        info.isSweep = false;
        info.starMask = ~0;
        info.starRate = 0;
        info.battleTime = mBattleTime;
        info.battleAchv = 1;
        info.merit = 1;
        DataCenter.Set("ISFROMGUILDBOSS", true);
        int damage = mBoss.IsDead() ? mBossHp : mBossHp - mBoss.GetHp();
        GuildBossNetManager.RequesGuildBossBattleEnd(RoleLogicData.Self.guildId, damage);
        info.demage = damage;
        DataCenter.OpenWindow(UIWindowString.union_pk_win_window, info);
    }
}

//-------------------------------------------------------------------------

class TM_WaitToGuildBossBattleResult : TM_WaitToBattleResult
{
    //public override void _OnOverTime()
    //{
    //    Finish();
    //    //MainProcess.mStage.BattleResult(mbWin);

    //    DataCenter.SetData("BATTLE_UI", "OPEN", true);
    //}
}

//-------------------------------------------------------------------------
class TM_WaitToMoveCameraToMainCharGuildBoss : TM_WaitToBattleResult
{
    public override bool _DoEvent()
    {
        WaitTime(0.5f);
        return true;
    }

    public override void _OnOverTime()
    {
        tEvent evt = EventCenter.Start("TM_WaitToStartAttackGuildBoss");
        MainProcess.mCameraMoveTool.MoveTo(Character.Self.GetPosition(), evt);
    }
}
//-------------------------------------------------------------------------
class TM_WaitToStartAttackGuildBoss : TM_WaitToPlayPveBeginTextEffect
{
    public override bool _DoEvent()
    {
        base._DoEvent();
        Character.Self.Start();

        return true;
    }

    public override void DoOverTime()
    {
        DataCenter.OpenWindow("BATTLE_START_WINDOW");
        tWindow battleStartWindow = DataCenter.GetData("BATTLE_START_WINDOW") as tWindow;
        GlobalModule.DoLater(() => GameCommon.FindObject(battleStartWindow.mGameObjUI, "Texture2").SetActive(true), 0.7f);
        GlobalModule.DoLater(() => GameCommon.FindObject(battleStartWindow.mGameObjUI, "Texture1").SetActive(true), 1.2f);
        GlobalModule.DoLater(() => GameCommon.FindObject(battleStartWindow.mGameObjUI, "Texturestart").SetActive(true), 1.6f);

        if (battleStartWindow != null) MonoBehaviour.Destroy(battleStartWindow.mGameObjUI, 3.0f);

        tEvent evt = EventCenter.Start("TM_WaitToBattleStartWindowEndGuildBoss");
        GlobalModule.DoLater(() => evt.DoEvent(), 0);

        DataCenter.OpenWindow("MASK_OPERATE_WINDOW");
    }
}

//-------------------------------------------------------------------------
class TM_WaitToBattleStartWindowEndGuildBoss : TM_WaitToShowAutoBattleButton
{
    public override bool _DoEvent()
    {
        base._DoEvent();
        return true;
    }

    public override void DoOverTime()
    {
        if (MainProcess.mStage != null)
        {
            //if (MainProcess.mStage is PVP4Battle)
            //    MainProcess.mStage.StartWar();
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
