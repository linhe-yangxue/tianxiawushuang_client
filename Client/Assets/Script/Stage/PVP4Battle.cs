using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;
using Utilities.Routines;

public class PVP4Battle : StageBattle
{
	private OpponentCharacter mOpponentCharacter;
    private PetFightDetail mOpponentCharacterDetail;
	private PetData[] mOpponentPets = new PetData[3];
    private PetFightDetail[] mOpponentPetDetails = new PetFightDetail[3];
	private Vector3 mBirthPos = new Vector3();
	private Vector3 mOpponentBirth = new Vector3();
    private int mOpponentPower = 10000;
    private bool mIsOpponentRobot = false;

	private float mMaxBattleTime;
    private float mRemainTime = 0f;
	//public NiceData mOpponentData = new NiceData();

    static public void RegisterNetEvent()
    {
        Logic.EventCenter center = Net.gNetEventCenter;
        center.RegisterEvent("CS_Request4v4State", new DefineFactoryLog<CS_Request4v4State>());
		center.RegisterEvent("CS_Search4v4Competitor", new DefineFactoryLog<CS_Search4v4Competitor>());
		center.RegisterEvent("CS_Request4v4GlobalRanks", new DefineFactoryLog<CS_Request4v4GlobalRanks>());
		center.RegisterEvent("CS_Request4v4FriendRanks", new DefineFactoryLog<CS_Request4v4FriendRanks>());
		center.RegisterEvent("CS_Request4v4Result", new DefineFactoryLog<CS_Request4v4Result>());
		center.RegisterEvent("CS_Request4v4Competition", new DefineFactoryLog<CS_Request4v4Competition>());
    }

	public override void InitBattleUI()
	{ 
        //by chenliang
        //begin

        //将InitBattleUI实现放在Start里
// 		mMaxBattleTime = DataCenter.mGlobalConfig.GetData ("FOUR_VS_FOUR_MAX_BATTLE_TIME", "VALUE");
// 		DataCenter.OpenWindow("BATTLE_SKILL_WINDOW");
// 		DataCenter.OpenWindow("BATTLE_PLAYER_WINDOW");
// 		DataCenter.OpenWindow("BATTLE_PLAYER_EXP_WINDOW");

        //end
//		DataCenter.OpenWindow("OPPONENT_PLAYER_WINDOW");
        //by chenliang
        //begin

//		MainProcess.mStage.Start();
//---------------
        //场景改为手动开始

        //end
	}

    //public override void Process (float delayTime)
    //{
    //    if(mbBattleActive && !mbBattleFinish)
    //    {
    //        mBattleTime = mMaxBattleTime - (Time.time - mActiveTime);
    //        if(mBattleTime <= 0)
    //        {
    //            mBattleTime = 0;
    //            OnFinish (false);
    //        }
    //        else
    //            DataCenter.SetData("BATTLE_PLAYER_EXP_WINDOW", "TIME", mBattleTime);
    //    }
    //}

    protected override void OnProcess(float delayTime)
    {
		base.OnProcess(delayTime);
    }

    protected override void OnBattleUI(float delayTime)
    {
		base.OnBattleUI(delayTime);
    }

	public override void Start ()
	{
        //by chenliang
        //begin

        mMaxBattleTime = DataCenter.mGlobalConfig.GetData("FOUR_VS_FOUR_MAX_BATTLE_TIME", "VALUE");
        DataCenter.OpenWindow("BATTLE_SKILL_WINDOW");
        GlobalModule.DoOnNextUpdate(() =>
        {
            DataCenter.OpenWindow("BATTLE_PLAYER_WINDOW");
            DataCenter.OpenWindow("BATTLE_PLAYER_EXP_WINDOW");
        });

        int selfTeamPower = (int)GameCommon.GetPower();
        int maxSelfPower = BattleProver.GetMaxLimit(BATTLE_PROVE_TYPE.PVP, mOpponentPower);

        if (selfTeamPower >= maxSelfPower)
        {
            DataCenter.OpenWindow("BATTLE_SKIP_WINDOW");
        }

        //end
		base.Start ();

		MainProcess.mCameraMoveTool.MoveTo (mBirthPos);

		BirthSelfRoleObj ();
		BirthOppinentRoleObj ();		

		DataCenter.OpenWindow("OPPONENT_PLAYER_WINDOW");

        tEvent evt = EventCenter.Start("TM_WaitToStartAttackBoss");
        evt.DoEvent();

        MainProcess.mCameraMoveTool._MoveByObject(null);

        CastCGThenDo(null);

        //CastCameraAnimationThenDo(() =>
        //{
        //    //tEvent evt = EventCenter.Start("TM_WaitToMoveCameraToMainChar");
        //    //MainProcess.mCameraMoveTool.MoveTo(mOpponentCharacter.GetPosition(), evt);
        //    MainProcess.mCameraMoveTool.MoveTo(Character.Self.GetPosition(), 0.5f, () => MainProcess.mCameraMoveTool._MoveByObject(Character.Self));
        //});
	}

	public override void OnStart()
	{
		DataCenter.SetData("PVP_FOUR_VS_FOUR_BATTLE_LOADING_WINDOW", "CONTINUE", true);
	}

    protected override void OnActive()
    {
        InitIntervencerOnActive();
    }

    private void InitIntervencerOnActive()
    {
        if (mIsOpponentRobot)
        {
            return;
        }

        int selfTeamPower = (int)GameCommon.GetPower();
        int maxOpponentPower = BattleProver.GetMaxLimit(BATTLE_PROVE_TYPE.PVP, selfTeamPower);
        int maxSelfPower = BattleProver.GetMaxLimit(BATTLE_PROVE_TYPE.PVP, mOpponentPower);

        if (mOpponentPower > maxOpponentPower)
        {
            var intervencer = new PvpBattleIntervencer();
            intervencer.SetPvpResult(false);
            MainProcess.battleIntervencer = intervencer;
        }
        else if (selfTeamPower > maxSelfPower)
        {
            var intervencer = new PvpBattleIntervencer();
            intervencer.SetPvpResult(true);
            MainProcess.battleIntervencer = intervencer;
        }
    }

	//public override void StartWar ()
	//{
	//	mOpponentCharacter.StartWar ();
	//}

	public override void InitSetBirthPoint(MonoBehaviour birthPoint)
	{
		pvp_active_point point = birthPoint as pvp_active_point;
		if (point != null)
		{
			if (point.mIndex == 1)
				mOpponentBirth = point.transform.position;
			else
				mBirthPos = point.transform.position;
		}
		else
			DEBUG.LogError("Muse is pvp_active_point in PVP4Battle scene");
	}

	public void BirthSelfRoleObj()
	{
        if (Character.Self == null)
        {
            Vector3 pos = GetMainRoleBirthPoint(mBirthPos);
            CreateMainRole(pos, false/*, new Vector3(-0.5f, 0, -0.5f)*/);
        }
	}

	public void BirthOppinentRoleObj()
	{
		DataCenter.Set ("OPPONENT_CHARATER_LEVEL", /*(int)mOpponentData["LEVEL"]*/ArenaBase.challengeTarget.level);
//		DataCenter.Set ("OPPONENT_CHARATER_LEVEL", RoleLogicData.GetMainRole().mLevel);
        mOpponentCharacter = ObjectManager.Self.CreateObject(OBJECT_TYPE.OPPONENT_CHARACTER, /*(int)mOpponentData["OBID"]*/mOpponentCharacterDetail.tid) as OpponentCharacter;
		mOpponentCharacter.SetCamp(CommonParam.MonsterCamp);
        var data = new ActiveData();
        data.tid = mOpponentCharacterDetail.tid;
        data.level = ArenaBase.challengeTarget.level;
        data.teamPos = -1;
        mOpponentCharacter.SetActiveData(data);
		mOpponentCharacter.InitPosition(new Vector3(mOpponentBirth.x , 0, mOpponentBirth.z));
        mOpponentCharacter.SetRealDirection(mOpponentCharacter.GetPosition() + Quaternion.Euler(0f, mTeamInitAngle, 0f) * Vector3.forward);

//        public class PetFightDetail
//{
//    public int tid = 0; /* 符灵Tid */
//    public int attack = 0; /* 攻击力 */
//    public int hp = 0; /* 生命值 */
//    public int phyDef = 0; /* 物防值 */
//    public int mgcDef = 0; /* 法防值 */
//    public int criRt = 0; /* 暴击率 */
//    public int dfCriRt = 0; /* 抗暴率 */
//    public int hitTrRt = 0; /* 命中率 */
//    public int gdRt = 0; /* 闪避率 */
//    public int hitRt = 0; /* 伤害率 */
//    public int dfHitRt = 0; /* 减伤率 */
//}
        mOpponentCharacter.InitBaseAttribute();
        mOpponentCharacter.InitStaticAttribute();

        mOpponentCharacter.mStaticMaxHp = mOpponentCharacterDetail.hp;
        mOpponentCharacter.mStaticAttack = mOpponentCharacterDetail.attack;
        mOpponentCharacter.mStaticPhysicalDefence = mOpponentCharacterDetail.phyDef;
        mOpponentCharacter.mStaticMagicDefence = mOpponentCharacterDetail.mgcDef;
        mOpponentCharacter.mStaticCriticalStrikeRate = mOpponentCharacterDetail.criRt / 10000f;
        mOpponentCharacter.mStaticDefenceCriticalStrikeRate = mOpponentCharacterDetail.dfCriRt / 10000f;
        mOpponentCharacter.mStaticHitRate = mOpponentCharacterDetail.hitTrRt / 10000f;
        mOpponentCharacter.mStaticDodgeRate = mOpponentCharacterDetail.gdRt / 10000f;
        mOpponentCharacter.mStaticDamageEnhanceRate = mOpponentCharacterDetail.hitRt / 10000f;
        mOpponentCharacter.mStaticDamageMitigationRate = mOpponentCharacterDetail.dfHitRt / 10000f;
        mOpponentCharacter.mIsAttributeLocked = true;
        mOpponentCharacter.SetAttributeByLevel();
	
		for(int i = 0; i < 3; i++)
		{
            if (mOpponentPets[i] != null)
            {
                mOpponentCharacter.SetPetData(mOpponentPets[i], i);
                mOpponentCharacter.SetPetDetail(mOpponentPetDetails[i], i);
            }
		}

		mOpponentCharacter.Start();
	}

    public void SetOpponentCharacterDetail(PetFightDetail detail, int teamPower, bool isRobot)
    {
        mOpponentCharacterDetail = detail;
        mOpponentPower = teamPower;
        mIsOpponentRobot = isRobot;
    }

	public void SetOpponentPetAndDetail(int pos, PetData data, PetFightDetail detail)
	{
		if(pos > mOpponentPets.Length)
			return;

		mOpponentPets[pos] = data;
        mOpponentPetDetails[pos] = detail;
	}

    public override void Preload()
    {
        base.Preload();

        for (int i = 0; i < mOpponentPets.Length; ++i)
        {
            if (mOpponentPets[i] != null && mOpponentPets[i].tid > 0)
            {
                AsyncLoadPool.EnqueueBattleObject(mOpponentPets[i].tid);
            }
        }
    }

    //protected override void OnRequestResult(bool bWin)
    //{
    //    GlobalModule.DoCoroutine(DoSendResult());
    //}

    protected override void _SendBattleResult(bool iswin)
    {
        GlobalModule.DoCoroutine(DoSendResult());
    }

    //public override void OnFinish (bool bWin)
    //{
    //    if (mbBattleFinish)
    //    {
    //        return;
    //    }

    //    OnStageFinish(bWin);

    //    //tEvent evt = Net.StartEvent("CS_Request4v4Result");
    //    //evt.set ("WIN", bWin);
    //    //evt.set ("4v4ARENA", (int)mOpponentData["4v4ARENA"]);
    //    //evt.set ("4v4MASTER", (int)mOpponentData["4v4MASTER"]);
    //    //evt.DoEvent ();
    //    if (!MainProcess.isOnCheating)
    //    {
    //        GlobalModule.DoCoroutine(DoSendResult());
    //    }
    //}

    private IEnumerator DoSendResult()
    {
        // 发送结算请求
        ArenaNetManager.ArenaBattleEndRequester req = new ArenaNetManager.ArenaBattleEndRequester();
        yield return req.Start();

        if (req.success)
        {
            ArenaNetManager.ArenaAccountInfo info = req.accountInfo;
            DataCenter.OpenWindow("ARENA_RESULT_WINDOW", info);
        }
    }

    public static void GoBack()
    {
        GameLoadingUI.lockLoading = true;
        MainUIScript.mLoadingFinishAction = () => {
            MainUIScript.Self.OpenMainUI();
            MainUIScript.Self.HideMainBGUI();
            DataCenter.OpenWindow("ARENA_MAIN_WINDOW");
            GameLoadingUI.lockLoading = false;
        };
        MainProcess.ClearBattle();
        MainProcess.LoadRoleSelScene();
    }

    private static IEnumerator ReturnToPeakPvpWindow()
    {
        GetArenaRankRequester rankReq = new GetArenaRankRequester();
        yield return rankReq.Start();

        RefreshChallengeListRequester listReq = new RefreshChallengeListRequester();
        yield return listReq.Start();

        MainUIScript.Self.OpenMainUI();
        DataCenter.OpenWindow("PEAK_PVP_WINDOW");
        GameLoadingUI.lockLoading = false;
    }

    /// <summary>
    /// 跳过战斗（直接胜利）
    /// </summary>
    public override void SkipBattle()
    {
        if (mbBattleFinish)
        {
            SendBattleResult(true);
        }
        else
        {
            mbSkipFinishAnim = true;

            if (mbBattleActive)
            {
                ObjectManager.Self.ForEachAlived(x => !x.IsSameCamp(Character.Self), x => x.ChangeHp(-x.GetHp() - 999999));
            }

            OnFinish(true);
        }
    }

    public override void OnObjectDead(BaseObject obj)
    {
        MainProcess.OnObjectDead(obj);
        OBJECT_TYPE objType = obj.GetObjectType();

        if (objType == OBJECT_TYPE.CHARATOR || objType == OBJECT_TYPE.PET)
        {
            if (Character.Self.TeamAllDead())
            {
                OnFinish(false);
            }
            else if (CameraMoveEvent.mMainTarget == obj)
            {
                CameraMoveEvent.BindMainObject(Character.Self.GetAlivedPet());
            }
        }
        else if (objType == OBJECT_TYPE.OPPONENT_CHARACTER || objType == OBJECT_TYPE.OPPONENT_PET)
        {
            if (OpponentCharacter.mInstance.TeamAllDead())
            {
                OnFinish(true);
            }
        }
    }
}

public class CS_Request4v4State : BaseNetEvent 
{
    public override void _OnResp(tEvent respEvt)
    {
        respEvt.Dump();

		STRING_INDEX result = (STRING_INDEX)(int)respEvt.get("RESULT");
		if(result == STRING_INDEX.ERROR_NONE)
		{
			if(get ("JUST_GET_RNAK"))
				DataCenter.SetData("PVP_FOUR_VS_FOUR_ENTER", "REFRESH", respEvt);
			else
				DataCenter.SetData("PVP_PLAYER_FOUR_VS_FOUR_RANK_WINDOW", "REFRESH", respEvt);
		}
		else 
			DataCenter.OpenMessageWindow(result);
    }
}

public class CS_Search4v4Competitor : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		respEvt.Dump();

		STRING_INDEX result = (STRING_INDEX)(int)respEvt.get("RESULT");

		if(result == STRING_INDEX.ERROR_NONE)
		{
			respEvt.set ("RANK_TYPE", (int)getObject ("RANK_TYPE"));
			DataCenter.SetData("PVP_FOUR_VS_FOUR_WINDOW", "REFRESH_RANK", respEvt);
		}
		else
			DataCenter.OpenMessageWindow(result);
	}
}

public class CS_Request4v4GlobalRanks : CS_Search4v4Competitor
{
	public override void _OnResp(tEvent respEvt)
	{
		base._OnResp(respEvt);
	}
}

public class CS_Request4v4FriendRanks : CS_Search4v4Competitor
{
	public override void _OnResp(tEvent respEvt)
	{
		base._OnResp(respEvt);
	}
}

public class CS_Request4v4Competition : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		respEvt.Dump();

		STRING_INDEX result = (STRING_INDEX)(int)respEvt.get("RESULT");
		if(result == STRING_INDEX.ERROR_NONE)
		{
			object d;
			if (!respEvt.getData("DATA", out d))
			{
				Log("Erro: No response role data");
				return;
			}
			NiceData data = d as NiceData;
			GlobalModule.ClearAllWindow ();
			DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_VICTORY_PREDICT_WINDOW", data);
			DataCenter.OpenWindow ("PVP_FOUR_VS_FOUR_BATTLE_LOADING_WINDOW");
		}
		else
			DataCenter.OpenMessageWindow (result, true);
	}
}


public class CS_Request4v4Result : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		respEvt.Dump();
		STRING_INDEX result = (STRING_INDEX)(int)respEvt.get("RESULT");
		if(result == STRING_INDEX.ERROR_NONE)
		{
			respEvt.set ("WIN", (bool)get ("WIN"));
			DataCenter.OpenWindow("PVP_FOUR_VS_FOUR_FINISH_WINDOW", respEvt);
			tEvent e = EventCenter.Start("TM_WaitShowFinishResult");
			e.WaitTime(4);
		}
		else
			DataCenter.OpenMessageWindow (result);
	}
}
