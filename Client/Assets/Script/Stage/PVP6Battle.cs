using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

public class PVPPetData : PetData
{
    public bool mbUsed = false;
}

public class PVP6Battle : StageBattle 
{
    public bool mbIsAttributeWar = false;
    public bool mbIsRevenge = false;
    public UInt64 mOpponentDBID = 0;
    public string mOpponentName = "";

    public PVPPetData[]  mOpponentPet = new PVPPetData[6];
    public BaseObject[] mSelfPet = new BaseObject[6];
    public BaseObject[] mOtherPet = new BaseObject[6];

    public Vector3[] mBirthPos = new Vector3[4];
    public Vector3[] mOpponentBirth = new Vector3[4];

    public int mCurrentUsedPos = 0;

    static public void RegisterNetEvent()
    {
        Logic.EventCenter center = Net.gNetEventCenter;
        center.RegisterEvent("CS_RequestPVP6", new DefineFactoryLog<CS_RequestPVP6>());
        center.RegisterEvent("CS_RequestPVP6Result", new DefineFactoryLog<CS_RequestPVP6Result>());
        center.RegisterEvent("CS_RequestPVP6Score", new DefineFactoryLog<CS_RequestPVP6Score>());
        
        EventCenter.Self.RegisterEvent("TM_WaitShowFinishResult", new DefineFactory<TM_WaitShowFinishResult>());
        EventCenter.Self.RegisterEvent("TM_PVPWaitStartAttack", new DefineFactory<TM_PVPWaitStartAttack>());
    }

    public override void InitBattleUI()
	{ 
		BaseUI.OpenWindow("PVP_WINDOW", null, false);
	}

	override public void OnStart()
	{
		DataCenter.SetData("PVP_BATTLE_LOADING_WINDOW", "CONTINUE", true);
	}

    public override void InitSetBirthPoint(MonoBehaviour birthPointCom)
    {
        pvp_active_point birthPoint = birthPointCom as pvp_active_point;
        if (birthPoint != null)
        {
            if (birthPoint.mIndex >= 10)
            {
                int p = birthPoint.mIndex - 10;
                if (p < 4)
                    mOpponentBirth[p] = birthPoint.transform.position;
            }
            else
            {
                if (birthPoint.mIndex < 4)
                    mBirthPos[birthPoint.mIndex] = birthPoint.transform.position;
            }
        }
        else
        {
            DEBUG.LogError("Muse is pvp_active_point in PVP6Battle scene");
        }
    }

    public void InitOpponent(NiceTable usePetData)
    {
        int i = 0;
        foreach (KeyValuePair<int, DataRecord> r in usePetData.GetAllRecord())
        {
            if (i >= 6)
                break;

            DataRecord re = r.Value;
            PVPPetData pet = new PVPPetData();
            pet.itemId = re.getData("ID");
            pet.level = re.getData("PET_LEVEL");
            pet.exp = re.getData("PET_EXP");
            pet.tid = re.getData("PET_ID");
            pet.mFailPoint = re.getData("FAIL_POINT");
            pet.strengthenLevel = re.getData("PET_ENHANCE");
            pet.starLevel = TableCommon.GetNumberFromActiveCongfig(pet.tid, "STAR_LEVEL");
            mOpponentPet[i++] = pet;
        }
    }

    public virtual PetData GetPVPBattlePet(int teamPos)
    {
        PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;
        if (mbIsAttributeWar)
            return petLogic.GetAttributePVPPet(teamPos);
        else
            return petLogic.GetPVPPet(teamPos);
    }

    public PetData UseSelfPet(bool bNext)
    {               
        for (int i = mCurrentUsedPos; i < 6; ++i)
        {
            PetData pet = GetPVPBattlePet(i);
            if (!bNext)
                mCurrentUsedPos = i + 1;
            if (pet != null)
            {
                return pet;
            }
        }
        return null;
    }

    public PVPPetData UseOpponentPet(bool bNext)
    {
        for (int i = 0; i < 6; ++i )
        {
            if (mOpponentPet[i]!=null && !mOpponentPet[i].mbUsed)
            {
                if (!bNext)
                    mOpponentPet[i].mbUsed = true;

                return mOpponentPet[i];
            }
        }
        return null;
    }

    // birthPointIndex 0 ~ 3
    bool BirthSelfPet(int birthPointIndex, bool bStart)
    {
        if (birthPointIndex < 0 || birthPointIndex > 3)
        {
            DEBUG.LogError("Now pvp birth point num is 4, more than >"+birthPointIndex.ToString());
            return false;
        }

        PetData pet = UseSelfPet(false);
        if (pet != null)
        {
            BaseObject obj = ObjectManager.Self.CreateObject(OBJECT_TYPE.PVP_ACTIVE, pet.tid);
            if (obj != null)
            {
                Vector3 pos = mBirthPos[birthPointIndex];
                obj.SetLevel(pet.level);
                obj.InitPosition(pos);
                obj.SetVisible(true);
                obj.SetCamp(CommonParam.PlayerCamp);
                //obj.Start();                    
				obj.SetDirection(mOpponentBirth[birthPointIndex]);
                for (int n = 0; n < 6; ++n)
                {
                    if (mSelfPet[n] == null)
                    {
                        mSelfPet[n] = obj;
                        break;
                    }
                }

                if (bStart)
                    obj.Start();

                return true;
            }
        }
        return false;
    }

    // birthPointIndex 0 ~ 3
    bool BirthOpponentPet(int birthPointIndex, bool bStart)
    {
        if (birthPointIndex < 0 || birthPointIndex > 3)
        {
            DEBUG.LogError("Now pvp birth point num is 4, more than >" + birthPointIndex.ToString());
            return false;
        }

        PVPPetData pet = UseOpponentPet(false);
        if (pet == null)
            return false;

        Vector3 pos = mOpponentBirth[birthPointIndex];

        BaseObject obj = ObjectManager.Self.CreateObject(OBJECT_TYPE.PVP_ACTIVE, pet.tid);
        if (obj != null)
        {
			obj.SetLevel(pet.level);
            obj.InitPosition(pos);
            obj.SetVisible(true);

            obj.SetCamp(CommonParam.MonsterCamp);
            //obj.Start();
			obj.SetDirection(mBirthPos[birthPointIndex]);
            for (int n = 0; n < 6; ++n)
            {
                if (mOtherPet[n] == null)
                {
                    mOtherPet[n] = obj;
                    break;
                }
            }

            if (bStart)
                obj.Start();

            return true;
        }
        return false;
    }

	// Use this for initialization
	public override void Start () 
    {
        base.Start();

        //string key2 = "POINT_0";
        //Vector3 pos2= (Vector3)(DataCenter.Get(key2).mObj);
        MainProcess.mCameraMoveTool.MoveTo(mBirthPos[0]);

        for (int i = 1; i < 4; ++i)
        {
            if (!BirthSelfPet(i, false))            
                break;
        }

        for (int i = 1; i < 4; ++i)
        {
            if (!BirthOpponentPet(i, false))
                break;
        }

        DataCenter.SetData("PVP_PLAYER_WINDOW", "UPDATE_PET", null);
        DataCenter.SetData("PVP_OTHER_PLAYER_WINDOW", "UPDATE_PET", null);
		DataCenter.SetData("PVP_PLAYER_WINDOW", "PET_DEAD", this);
		DataCenter.SetData("PVP_OTHER_PLAYER_WINDOW", "PET_DEAD", this);

        tEvent evt = EventCenter.Start("TM_PVPWaitStartAttack");
        evt.WaitTime(2);
	}

    //public override void StartWar()
    //{
    //    for (int n = 0; n < 6; ++n)
    //    {
    //        if (mSelfPet[n]!=null)
    //        {
    //            mSelfPet[n].Start();               
    //        }
    //    }

    //    for (int n = 0; n < 6; ++n)
    //    {
    //        if (mOtherPet[n] != null)
    //        {
    //            mOtherPet[n].Start();               
    //        }
    //    }
    //}
	

	public void OnPetDead(BaseObject pet)
    {
		if (pet.GetCamp()==CommonParam.PlayerCamp)
		{
            if (BirthSelfPet(UnityEngine.Random.Range(0, 4), true))
            {
                DataCenter.SetData("PVP_PLAYER_WINDOW", "UPDATE_PET", null);
                return;
            }
			bool bAllDead = true;
			foreach (BaseObject obj in mSelfPet)
			{
				if (obj!=null && !obj.IsDead())
				{
					bAllDead = false;
					break;
				}
			}
            DataCenter.SetData("PVP_PLAYER_WINDOW", "UPDATE_PET", null);
			if (bAllDead)
				OnFinish(false);
		}
		else
		{
            if (BirthOpponentPet(UnityEngine.Random.Range(0, 4), true))
            {
                DataCenter.SetData("PVP_OTHER_PLAYER_WINDOW", "UPDATE_PET", null);
                return;
            }
			bool bAllDead = true;
			foreach (BaseObject obj in mOtherPet)
			{
				if (obj!=null && !obj.IsDead())
				{
					bAllDead = false;
                    break;
                }
            }
            DataCenter.SetData("PVP_OTHER_PLAYER_WINDOW", "UPDATE_PET", null);
            if (bAllDead)
                OnFinish(true);
		}
    }

	public override void OnFinish(bool bWin)
	{        
		//ClearBattle(bWin);
       
        tEvent requestFinish = Net.StartEvent("CS_RequestPVP6Result");
        requestFinish.set("IS_REVENGE", mbIsRevenge);
        requestFinish.set("IS_ATTRIBUTE_WAR", mbIsAttributeWar);
        requestFinish.set("WIN", bWin);
        requestFinish.set("OPPONENT_DBID", mOpponentDBID);
        requestFinish.set("OPPONENT_NAME", mOpponentName);
        requestFinish.DoEvent();
    }
}
    
public class Button_PVPBtn : CEvent
{
    public override bool _DoEvent()
    {
        //MainUIScript.Self.OpenMainUI();
		//DataCenter.OpenWindow("PVP_SELECT_ENTER", "FOUR_VS_FOUR");
        //GlobalModule.DoCoroutine(DoPVPBtn());

		//神装shop
		//DataCenter.OpenWindow("SHOP_RENOWN_WINDOW");
		//声望shop
		//DataCenter.OpenWindow("PVP_PRESTIGE_SHOP_WINDOW");
		//战功商店
		//DataCenter.OpenWindow("FEATS_SHOP_WINDOW");
		//神秘商店
		DataCenter.OpenWindow("NEW_MYSTERIOUS_SHOP_WINDOW");


        /*
        if (!GameCommon.bIsLogicDataExist("RAMMBOCK_WINDOW"))
        {
            GlobalModule.ClearAllWindow();
//            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RammbockWindow);
            DataCenter.OpenWindow("RAMMBOCK_WINDOW");
            DataCenter.OpenWindow("INFO_GROUP_WINDOW");
        }*/
//        GlobalModule.ClearAllWindow();
        //DataCenter.OpenWindow("RAMMBOCK_WINDOW");

        return true;
    }
}

public class Button_LilianBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("TRIAL_WINDOW");
        DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
		DataCenter.SetData ("INFO_GROUP_WINDOW","PRE_WIN",1);
        MainUIScript.Self.HideMainBGUI();
        return true;
    }
}

public class Button_TrialWindowBackBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("TRIAL_WINDOW");
        DataCenter.CloseWindow("TRIAL_WINDOW_BACK");
        //MainUIScript.Self.ShowMainBGUI();
        return true;
    }
}

public class CS_RequestPVP6 : BaseNetEvent 
{
    public override void _OnResp(tEvent respEvt)
    {
        respEvt.Dump();

        NiceTable usePetData = respEvt["USE_PET"].mObj as NiceTable;
        if (usePetData != null)
        {
            GlobalModule.ClearAllWindow();

            DataCenter.CloseWindow("PVP_SELECT_ENTER");
            DataCenter.CloseWindow("PVP_TEAM_ADJUST");
            DataCenter.CloseWindow("PVP_PET_TEAM_WINDOW");
            DataCenter.CloseWindow("PVP_PLAYER_RANK_WINDOW");
            DataCenter.CloseWindow("PVP_RANK_WINDOW");
            DataCenter.CloseWindow("PVP_SEARCH_OPPONENT");
            DataCenter.CloseWindow("PVP_PET_BAG_WINDOW");
            //DataCenter.OpenWindow("PVP_SEARCH_OPPONENT", respEvt);
            tEvent evt = EventCenter.Start("TM_WaitShowVictoryWindow");
            if (evt != null)
            {
                evt.set("SEARCH_RESULT", respEvt);
                evt.DoEvent();
            }
            else
                EventCenter.Log(LOG_LEVEL.ERROR, "No register TM_WaitShowVictoryWindow");
        }
        else
        {
			eRequestPVPResult result = (eRequestPVPResult)(int)respEvt["RESULT"];
            if (result == eRequestPVPResult.PVP_UsePetEmpty)
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PVP_EQUIP_PETS_FIRST);
            else if (result == eRequestPVPResult.PVP_TicketEmpty)
                DataCenter.OpenWindow("PVP_BUY_TICKET");
			else if (result == eRequestPVPResult.PVP_NotOpen)
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PVP_NOT_OPEN);
			else
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PVP_SEARCH_AGAIN);
        }
    }
}

//-------------------------------------------------------------------------
public class CS_RequestPVP6Result : BaseNetEvent
{
	static public tEvent evt;
    public override void _OnResp(tEvent respEvt)
    {
        DataCenter.OpenWindow("PVP_FINISH_WINDOW", respEvt);
		evt = EventCenter.Start("TM_WaitShowFinishResult");
        evt.WaitTime(4);
        return;
    }

//	static public void FinishEvent()
//	{
//		if(evt == null)
//			return;
//
//		evt.Finish ();
//		evt.DoOverTime ();
//	}
}

//-------------------------------------------------------------------------
class TM_WaitShowFinishResult : CEvent
{
    public override void _OnOverTime()
    {
		if(MainProcess.mStage is PVP4Battle)
		{
			MainProcess.ClearBattle();
			MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.PVPFourVsFour);
			return ;
		}

        PVP6Battle battle = MainProcess.mStage as PVP6Battle;
		MainProcess.ClearBattle();
        if (battle != null && battle.mbIsRevenge)
        {
            MainProcess.LoadRoleSelScene();
            DataCenter.OpenWindow("PVP_RECORD_WINDOW");
        }
        else if (battle != null && battle.mbIsAttributeWar)
			MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.PVPAttributeWindow);
        else
			MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.PVPRankWindow);
    }
}

//-------------------------------------------------------------------------
class TM_PVPWaitStartAttack : CEvent
{
    public override void _OnOverTime()
    {
        //if(MainProcess.mStage is PVP6Battle)
        //    MainProcess.mStage.StartWar();
    }
}
//-------------------------------------------------------------------------