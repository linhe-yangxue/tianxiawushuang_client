using UnityEngine;
using System.Collections;
using Logic;
using System;
using Utilities;
using System.Text.RegularExpressions;

//----------------------------------------------------------------------------------
// RoleSelWindow
public class Button_AddStaminaBtn : CEvent
{
	public override bool _DoEvent()
	{
		if (DataCenter.GetData ("SHOP_WINDOW") != null) {
            GlobalModule.ClearAllWindow(new string[] { "shop_window_back_group", "info_group_window" });
			DataCenter.OpenWindow("SHOP_WINDOW", null);
			GlobalModule.DoCoroutine(__GoToShop());
		}
		else
		{
            GlobalModule.ClearAllWindow(new string[] { "shop_window_back_group", "info_group_window" });
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
            //by chenliang
            //begin

            GlobalModule.DoCoroutine(__GoToShop());

            //end
		}
		return true;
	}
    //by chenliang
    //begin

    private IEnumerator __GoToShop()
    {
        yield return new WaitForEndOfFrame();
        DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.TOOL);
    }

    //end
}
//by chenliang
//begin

public class Button_add_refine_btn : CEvent
{
    public override bool _DoEvent()
    {
		if (DataCenter.GetData ("SHOP_WINDOW") != null) 
		{
            GlobalModule.ClearAllWindow(new string[] { "shop_window_back_group", "info_group_window" });
			DataCenter.OpenWindow("SHOP_WINDOW", null);
			GlobalModule.DoCoroutine(__GoToShop());
		}
        else
        {
            GlobalModule.ClearAllWindow(new string[] { "shop_window_back_group", "info_group_window" });
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
            GlobalModule.DoCoroutine(__GoToShop());
        }
        return true;
    }

    private IEnumerator __GoToShop()
    {
        yield return new WaitForEndOfFrame();
		DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.TOOL);
    }
}

//end

public class Button_AddGoldBtn : CEvent
{
	public override bool _DoEvent()
	{
		if (DataCenter.GetData ("SHOP_WINDOW") != null) 
		{
			GlobalModule.ClearAllWindow(new string[]{"shop_window_back_group","info_group_window"});
			DataCenter.OpenWindow("SHOP_WINDOW", null);
			GlobalModule.DoCoroutine(__GoToShop());
		}
			
		else
		{
            GlobalModule.ClearAllWindow(new string[] { "shop_window_back_group", "info_group_window" });
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
		}
		return true;
	}

	private IEnumerator __GoToShop()
	{
		yield return new WaitForEndOfFrame();
		DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.TOOL);
	}
}

public class Button_AddDiamondBtn : CEvent
{
	public override bool _DoEvent()
	{
        //go to recharge
        GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, null, CommonParam.rechageDepth);
		return true;
	}
}	

public class Button_TalkBtn : CEvent
{
	public override bool _DoEvent()
	{        
        //MainProcess.RequestAppearBoss();



		//Character.Self.AddExp(10000);
		//Character.Self.LevelUp();
		//Character.Self.AddExp(10000);
		return true;
	}
}

public class Button_SettingBtn : CEvent
{
	public override bool _DoEvent()
	{
        DataCenter.OpenWindow("GAME_SETTINGS_WINDOW");
		return true;
	}
}

public class Button_battle_settings : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("BATTLE_SETTINGS_WINDOW");
        return true;
    }
}

public class Button_on_hook_btn : CEvent
{
	public override bool _DoEvent()
	{
		tEvent evt = Net.StartEvent("CS_RequestIdleBottingStatus");
		evt.set ("WINDOW_NAME", "ON_HOOK_WINDOW");
		evt.DoEvent();
//		DataCenter.OpenWindow("ON_HOOK_WINDOW");
		return true;
	}
}

public class Button_NoticeBtn : CEvent
{
	public override bool _DoEvent()
	{
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.BoardWindow);
        //DataCenter.OpenWindow("BOARD_WINDOW");
		//MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.MailWindow);
		return true;
	}
}

public class Button_MailBtn : CEvent
{
	public override bool _DoEvent()
	{
		//		DataCenter.OpenWindow ("MAIL_WINDOW");
		if(!GameCommon.bIsLogicDataExist ("MAIL_WINDOW"))
		{
			GlobalModule.ClearAllWindow();
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.MailWindow);
			DataCenter.OpenWindow ("INFO_GROUP_WINDOW");
            DataCenter.Set("FUNC_ENTER_INDEX",FUNC_ENTER_INDEX.MAIL);
		}
		return true;
	}
}

public class Button_DailySignBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow("DAILY_SIGN_WINDOW");
		return true;
	}
}

public class Button_first_pay_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow("FIRST_PAY_GIFT_BAG");
		return true;
	}
}
//ToDo
public class Button_PointStarBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("ASTROLOGY_UI_WINDOW");
//		DataCenter.OpenWindow ("ASTROLOGY_WINDOW");
		MainUIScript.Self.HideMainBGUI ();
//		DataCenter.SetData ("ASTROLOGY_WINDOW", "REQUST_POINT_STAR_INDEX", true);
		return true;
	}
}

public class Button_morrow_land_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("MORROW_LAND_WINDOW");
		MainUIScript.Self.HideMainBGUI ();
		return true;
	}
}

public class Button_seven_days_carnival_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("SEVEN_DAYS_CARNIVAL_WINDOW");
		NetManager.RequestGetRevelryList();
		MainUIScript.Self.HideMainBGUI ();
		return true;
	}
}

public class Button_TaskBtn : CEvent
{
	public override bool _DoEvent()
	{
        GlobalModule.DoCoroutine(DoTaskBtn());
        return true;
	}

    private IEnumerator DoTaskBtn()
    {
        if (TaskState.shouldDailyRefresh)
        {
            yield return new GetDailyTaskDataRequester().Start();
        }

        if (TaskState.shouldAchieveRefresh)
        {
            yield return new GetAchievementDataRequester().Start();
        }

        DataCenter.OpenWindow("TASK_WINDOW");
		MainUIScript.Self.HideMainBGUI ();
        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.TASK);
    }
}

public class Button_ShopBtn : CEvent
{
	public override bool _DoEvent()
	{
		GlobalModule.ClearAllWindow ();
        //by chenliang
        //begin

//		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
//---------------
        DataCenter.OpenWindow("SHOP_WINDOW");

        //end
		return true;
	}
}

public class Button_PVEBtn : CEvent
{
	public override bool _DoEvent()
	{
        // 将记录的地图点索引重置为0，这样进入地图后就会自动跳转到最新的关卡
        ScrollWorldMapWindow.mPointIndex = 0;

        if (GameCommon.bIsLogicDataExist("SCROLL_WORLD_MAP_WINDOW"))
            DataCenter.OpenWindow("SCROLL_WORLD_MAP_WINDOW");
        else
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
		//MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
        //DataCenter.OpenWindow("SCROLL_WORLD_MAP_WINDOW");
		DataCenter.SetData ("INFO_GROUP_WINDOW","PRE_WIN",2);
		return true;
	}
}

public class Button_FriendBtn : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.FriendWindow);
		return true;
	}
}

public class Button_PetBtn : CEvent
{
	public override bool _DoEvent()
	{
        if (CommonParam.bIsNetworkGame)
        {
			tEvent gemQuest = Net.StartEvent("CS_RequestGem");
			gemQuest.DoEvent();

			CS_RequestPetList petListQuest = Net.StartEvent("CS_RequestPetList") as CS_RequestPetList;
            petListQuest.mAction = () => MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
			SetBackAction(petListQuest);
			petListQuest.set ("IS_PET_BAG", true);
			petListQuest.DoEvent();

        }
        else
        {
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
//			MainUIScript.Self.mStrAllPetAttInfoPageWindowName = "PetInfoWindow";
        }
		return true;
	}

	public virtual void SetBackAction(CS_RequestPetList request)
	{
		if(request != null)
			request.mBackAction = () => MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
	}
}

public class Button_CharacterBtn : CEvent
{
	public override bool _DoEvent()
	{
		if (CommonParam.bIsNetworkGame)
		{
            DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);

			MainUIScript.Self.HideMainBGUI();
            return true;
		}
		else
		{
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
			MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "ROLE_INFO_WINDOW";
		}
		return true;
	}

    public virtual void SetBackAction(CS_RequestRoleEquip request)
    {
        if (request != null)
            request.mBackAction = () => MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
    }
}
public class Button_ActivityRankBtn : CEvent
{
    public override bool _DoEvent()
    {
        MainUIScript.Self.OpenMainUI();
        DataCenter.OpenWindow("RANKLIST_ACTIVITY_UI_WINDOW");
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_RANK_ACTIVITY,false);
        return true;
    }
}
public class Button_flash_sale_multi_button : CEvent
{
    public override bool _DoEvent()
    {
        FlashSaleWindowNetManager.RequestGetFlashSaleList((int)FlashSaleBase.FLASH_SALE_TYPE.FLASH_SALE_MULTI);
        return true;
    }
}
public class Button_flash_sale_single_button : CEvent
{
    public override bool _DoEvent()
    {
        FlashSaleWindowNetManager.RequestGetFlashSaleList((int)FlashSaleBase.FLASH_SALE_TYPE.FLASH_SALE_SINGLE);
        return true;
    }
}



public class Button_RankBtn : CEvent 
{
    public override bool _DoEvent()
	{
        //by chenliang
        //begin

//        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RankWindow);
//--------------------
		MainUIScript.Self.OpenMainUI ();
        DataCenter.OpenWindow("RANKLIST_MAIN_UI_WINDOW");

        //end
		return true;
	}  
}

public class Button_ChangeTeamBtn : Button_PetBtn
{
	public override void SetBackAction(CS_RequestPetList request)
	{
		if(request != null)
		{
			if(get ("ACTION") == "FIVE_COPY_LEVEL_ACTION")
			{
				request.set ("ACTION", "FIVE_COPY_LEVEL_ACTION");
				request.mBackAction = () => {
					DataCenter.OpenWindow ("FIVE_COPY_LEVEL_SELECT_WINDOW");
					DataCenter.OpenWindow ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW");
					MainUIScript.Self.GoBack();
				};
			}
			else if(get ("ACTION") == "MAIL_ACTION")
			{
				request.mBackAction = () => {
					MainUIScript.Self.GoBack();
				};
			}
			else
			{
				GameObject obj = (GameObject) getObject("BUTTON");
				UIImageButton imageBtn = obj.GetComponent<UIImageButton>();
				imageBtn.isEnabled = false;

				request.mBackAction = () => {
				MainUIScript.Self.GoBack();
				DataCenter.Set ("IS_WINDOW_BACK", true);
				};
			}
		}
	}
}

public class Button_ChangeRoleBtn : Button_CharacterBtn
{
    public override void SetBackAction(CS_RequestRoleEquip request)
    {
        if (request != null)
        {
            request.mBackAction = () =>
            {
                MainUIScript.Self.GoBack();
                DataCenter.Set("IS_WINDOW_BACK", true);
            };
        }
    }
}

public class Button_vip_details_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("VIP_WINDOW");
		return true;
	}
}

public class Button_close_menu_button : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.mbAlwaysOpenMenu = false ;
		GameCommon.FindUI ("close_menu_button").SetActive (false);
		GameCommon.FindUI("open_menu_button").SetActive (true);

		UIPlayTween uiPt=null,uiPt1=null ;
		uiPt=GameObject.Find ("move_menu_left_bottom").GetComponent<UIPlayTween >();
		uiPt.Play (true);
		uiPt1=GameObject.Find ("move_menu_left_top").GetComponent<UIPlayTween >();
		uiPt1.Play (true);
		return true;
	}
}

public class Button_open_menu_button : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.mbAlwaysOpenMenu = false ;
		GameCommon.FindUI("close_menu_button").SetActive (true);
		GameCommon.FindUI("open_menu_button").SetActive (false);
	
		UIPlayTween uiPt=null,uiPt1=null ;
		uiPt=GameObject.Find ("move_menu_left_bottom").GetComponent<UIPlayTween >();
		uiPt.Play (true);
		uiPt1=GameObject.Find ("move_menu_left_top").GetComponent<UIPlayTween >();
		uiPt1.Play (true);
		return true;
	}
}
//----------------------------------------------------------------------------------
// AllRoleAttributeInfoUI
public class Button_AllRoleAttributeInfoBack : CEvent
{
	public override bool _DoEvent()
	{
		if(MainUIScript.Self.mWindowBackAction != null)
		{
			MainUIScript.Self.mWindowBackAction();
			MainUIScript.Self.mWindowBackAction = null;
		}
		else
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
		return true;

	}
}

public class Button_RoleInfoBtn : CEvent
{
	public override bool _DoEvent()
	{
        //DataCenter.SetData("AllRoleAttributeInfoWindow", "SET_SEL_PAGE", ALL_ROLE_ATTRIBUTE_PAGE_TYPE.RoleInfo);
        //DataCenter.CloseWindow("ALL_ROLE_ATTRIBUTE_INFO_WINDOW");
        //DataCenter.CloseWindow("STAR_LEVEL_UP_WINDOW");
        //DataCenter.CloseWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
        //DataCenter.CloseWindow("ROLE_EQUIP_COMPOSITION_WINDOW");
        DataCenter.OpenWindow("ROLE_INFO_WINDOW");
		return true;
	}
}

public class Button_RoleEvolutionBtn : CEvent
{
	public override bool _DoEvent()
	{
		UNLOCK_FUNCTION_TYPE  type = (UNLOCK_FUNCTION_TYPE)getObject ("TYPE");
		if(GameCommon.ButtonEnbleShowInfo(type))
			return true;

        //DataCenter.CloseWindow("ALL_ROLE_ATTRIBUTE_INFO_WINDOW");
        //DataCenter.OpenWindow("ALL_ROLE_ATTRIBUTE_INFO_WINDOW");
        //DataCenter.SetData("AllRoleAttributeInfoWindow", "SET_SEL_PAGE", ALL_ROLE_ATTRIBUTE_PAGE_TYPE.RoleEvolution);
        //DataCenter.CloseWindow("ROLE_INFO_WINDOW");
        //DataCenter.CloseWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
        //DataCenter.CloseWindow("ROLE_EQUIP_COMPOSITION_WINDOW");
        DataCenter.OpenWindow("STAR_LEVEL_UP_WINDOW");
		return true;
	}
}

public class Button_RoleEquipCultivateBtn : CEvent
{
	public override bool _DoEvent()
	{
		object obj = getObject ("TYPE");
		UNLOCK_FUNCTION_TYPE  type = UNLOCK_FUNCTION_TYPE.NONE;
		if(obj != null)
			type = (UNLOCK_FUNCTION_TYPE)getObject ("TYPE");

		if(GameCommon.ButtonEnbleShowInfo(type))
			return true;

		// is not open
		tWindow widnow = DataCenter.GetData("ROLE_EQUIP_CULTIVATE_WINDOW") as tWindow;
		if(widnow != null && widnow.mGameObjUI != null && widnow.mGameObjUI.activeSelf)
			return false;

        DataCenter.OpenWindow("ROLE_EQUIP_CULTIVATE_WINDOW");

		return true;
	}
}

public class Button_RoleEquipCompositionBtn : CEvent
{
    public override bool _DoEvent()
    {
        object obj = getObject("TYPE");
        UNLOCK_FUNCTION_TYPE type = UNLOCK_FUNCTION_TYPE.NONE;
        if (obj != null)
            type = (UNLOCK_FUNCTION_TYPE)getObject("TYPE");

        if (GameCommon.ButtonEnbleShowInfo(type))
            return true;

        //DataCenter.CloseWindow("ALL_ROLE_ATTRIBUTE_INFO_WINDOW");
        //DataCenter.OpenWindow("ALL_ROLE_ATTRIBUTE_INFO_WINDOW");
        //DataCenter.CloseWindow("ROLE_INFO_WINDOW");
        //DataCenter.SetData("AllRoleAttributeInfoWindow", "SET_SEL_PAGE", ALL_ROLE_ATTRIBUTE_PAGE_TYPE.RoleEquipComposition);    
        //DataCenter.CloseWindow("ROLE_INFO_WINDOW");
        //DataCenter.CloseWindow("STAR_LEVEL_UP_WINDOW");
        //DataCenter.CloseWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
        DataCenter.OpenWindow("ROLE_EQUIP_COMPOSITION_WINDOW");
        return true;
    }
}

public class Button_RoleEquipStrengthenBtn : CEvent
{
    public override bool _DoEvent()
    {
//		DataCenter.SetData("RoleEquipWindow", "SHOW_EQUIP_ICONS", false);

        DataCenter.SetData("ExpansionInfoWindow", "SHOW_WINDOW", ExpansionInfo_Page_Type.Strengthen);
        return true;
    }
}

//public class Button_RoleEquipEvolutionBtn : CEvent
//{
//    public override bool _DoEvent()
//    {
//        DataCenter.SetData("ExpansionInfoWindow", "SHOW_WINDOW", ExpansionInfo_Page_Type.Evolution);
//        return true;
//    }
//}

public class Button_RoleEquipResetBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("ExpansionInfoWindow", "SHOW_WINDOW", ExpansionInfo_Page_Type.Reset);
        return true;
    }
}

public class Button_RoleEquipStrengthenAutoAddBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("ExpansionInfoWindow", "AUTO_ADD_EQUIP", true);
        return true;
    }
}

public class Button_RoleEquipExpansionInfoWindowCloseBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("ExpansionInfoWindow", "CLOSE_WINDOW", true);
        return true;
    }
}

public class Button_RoleEquipStrengthenOKBtn : CEvent
{
	public override bool _DoEvent()
	{
        DataCenter.SetData("ExpansionInfoWindow", "STRENGTHEN_OK", true);
		return true;
	}
}

//public class Button_RoleEquipEvolutionOKBtn : CEvent
//{
//    public override bool _DoEvent()
//    {
//        DataCenter.SetData("ExpansionInfoWindow", "EVOLUTION_OK", true);
//        return true;
//    }
//}

public class Button_RoleEquipResetOKBtn : CEvent
{
	public override bool _DoEvent()
	{
        DataCenter.SetData("ExpansionInfoWindow", "RESET_OK", true);
		return true;
	}
}

public class Button_RoleEquipUseBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "USE", true);
		return true;
	}
}

public class Button_RoleEquipUnUseBtn : CEvent
{
	public override bool _DoEvent()
	{
        DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "UNUSE", true);
		return true;
	}
}

public class Button_RoleEquipSaleBtn : CEvent
{
	public override bool _DoEvent()
	{
        DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "SALE", true);
		return true;
	}
}

public class Button_UnlockButton : CEvent
{
	public override bool _DoEvent()
	{
		if(RoleLogicData.Self.mLockNum <= 0)
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_CONSUME_NOT_ENOUGH_LOCK_POINT);
			return false;
		}

		DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "UNLOCK_ATTRIBUTE", (int)get ("GRID_INDEX"));


		return true;
	}
}

public class Button_LockButton : CEvent
{
	public override bool _DoEvent()
	{
		if(RoleLogicData.Self.mLockNum <= 0)
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_CONSUME_NOT_ENOUGH_LOCK_POINT);
			return false;
		}

		DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "LOCK_ATTRIBUTE", (int)get ("GRID_INDEX"));


		return true;
	}
}

//----------------------------------------------------------------------------------
// AllPetAttributeInfoUI
public class Button_AllPetAttributeInfoBack : CEvent
{
	public override bool _DoEvent()
	{
        DataCenter.CloseWindow("PET_SKILL_WINDOW");
		DataCenter.CloseWindow("PET_DECOMPOSE_WINDOW");
		if(MainUIScript.Self.mWindowBackAction != null)
		{
			MainUIScript.Self.mWindowBackAction();
			MainUIScript.Self.mWindowBackAction = null;
		}
		else
        	MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
		//MainUIScript.Self.GoBack();
		return true;
	}
}

public class Button_PetInfoBtn : CEvent
{
	public override bool _DoEvent()
	{
        if (!GameCommon.bIsWindowOpen("PetInfoWindow"))
		    DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetInfo);
		return true;
	}
}

public class Button_PetUpgradeBtn : CEvent
{
	public override bool _DoEvent()
	{
		UNLOCK_FUNCTION_TYPE  type = (UNLOCK_FUNCTION_TYPE)getObject ("TYPE");
		if(GameCommon.ButtonEnbleShowInfo(type))
			return true;
        if (!GameCommon.bIsWindowOpen("PetUpgradeWindow"))
		    DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetUpgrade);
		return true;
	}
}

public class Button_PetSkillBtn : CEvent
{
    public override bool _DoEvent()
    {
        UNLOCK_FUNCTION_TYPE type = (UNLOCK_FUNCTION_TYPE)getObject("TYPE");
        if (GameCommon.ButtonEnbleShowInfo(type))
            return true;
        if (!GameCommon.bIsWindowOpen("PET_SKILL_WINDOW"))
            DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetSkill);
        return true;
    }
}

public class Button_PetDecomposeBtn : CEvent
{
	public override bool _DoEvent()
	{
        if (!GameCommon.bIsWindowOpen("PET_DECOMPOSE_WINDOW"))
		    DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetDecompose);
		DataCenter.SetData("PET_DECOMPOSE_WINDOW", "SHOW_PET_BAG", true);

        BagInfoWindow.isNeedRefresh = true;

		return true;
	}
}

public class Button_PetEvolutionBtn : CEvent
{
	
	public override bool _DoEvent()
	{
		UNLOCK_FUNCTION_TYPE  type = (UNLOCK_FUNCTION_TYPE)getObject ("TYPE");
		if(GameCommon.ButtonEnbleShowInfo(type))
			return true;

        if (!GameCommon.bIsWindowOpen("PetEvolutionWindow"))
		    DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution);
		return true;
	}
}

public class Button_PetSaleButton : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_PET_SALE, "", () => DataCenter.SetData("PetInfoWindow", "SALE_PET", true));
		return true;
	}
}

public class Button_AutoJoinButton : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData("PetInfoWindow", "SET_AUTO_JOIN", true);
		return true;
	}
}

public class TacticalFormationOpenButton : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.OpenWindow("TACTICAL_FORMATION_WINDOW");
		return true;
	}
}

public class Button_pet_play_check_box_btn : CEvent
{
	public override bool _DoEvent()
	{
		int iUsePos = get ("POS");
		DataCenter.SetData("PetInfoWindow", "SET_SELECT_PET", iUsePos);

		return true;
	}
}

public class Button_pet_play_flag_btn : CEvent
{
	public override bool _DoEvent()
	{
		int iUsePos = get ("POS");
        int iItemId = (int)(DataCenter.GetData("PetInfoWindow").get("SET_SELECT_USE_PET_ID"));
		SetChangePetUsePos(iUsePos, iItemId);
		return true;
	}

	public void SetChangePetUsePos(int usePos, int iItemId)
	{
		if(CommonParam.bIsNetworkGame)
		{
			SendServerChangePetUsePos(usePos, iItemId);
		}
		else
		{
			SetLogic(usePos, iItemId);
		}
	}
	public void SendServerChangePetUsePos(int iUsePos, int iItemId)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		PetData petData = petLogicData.GetPetDataByItemId(iItemId);

		int iTeam = petLogicData.mCurrentTeam;
		PetData teamPet = petLogicData.GetPetDataByPos(iTeam, iUsePos);
		bool bIsEquip = teamPet != null && teamPet.itemId == iItemId;
		if(petData != null)
		{
			CS_RequestChangePetUsePos quest = Net.StartEvent("CS_RequestChangePetUsePos") as CS_RequestChangePetUsePos;
            quest.set("WINDOW", "PET_BAG");
			quest.set("PET_ID", iItemId);
			quest.set("TEAM_ID", iTeam);
			quest.set("EQUIP", !bIsEquip);
            quest.set("USE_POS", iUsePos);
            quest.DoEvent();
		}
	}

	public static void ChangePetUsePos(bool bIsSuccess, int usePos, int iItemId)
	{
		if(bIsSuccess)
		{
			SetLogic(usePos, iItemId);

            int count = 0;
            PetData[] datas = PetLogicData.Self.GetPetDatasByTeam(PetLogicData.Self.mCurrentTeam);

            foreach (var data in datas)
            {
                if (data != null)
                    ++count;
            }

            if (count == 2)
            {
                GuideManager.Notify(GuideIndex.ChangeTeamOK);
            }
            else if(count == 3)
            {
                GuideManager.Notify(GuideIndex.ChangeTeamOK2);
            }
		}
		else
		{
            DataCenter.SetData("PetInfoWindow", "SET_SELECT_USE_PET_ID", -1);
		}
	}

	public static void SetSelectState(int iUsePos)
	{
		PetPlayCheckBoxData petPlayCheckBoxData = DataCenter.GetData("PetPlayCheckBoxData" + iUsePos.ToString()) as PetPlayCheckBoxData;

        GameCommon.ToggleTrue(petPlayCheckBoxData.mPetPlayCheckBox);
	}

	public static void SetLogic(int iUsePos, int iItemId)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		PetData petData = petLogicData.GetPetDataByItemId(iItemId);
        if (petData != null)
        {
			int iTeam = petLogicData.mCurrentTeam;
			petLogicData.SetPosInTeam(iTeam, iUsePos, iItemId);
        }

		DataCenter.SetData("PetInfoWindow", "SHOW_PET_FLAG", false);
		DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_PET_ICONS", true);
		DataCenter.SetData("PetInfoWindow", "SET_SELECT_PET", iUsePos);
//		DataCenter.SetData("PetInfoWindow", "SET_SELECT_PET_ID", -1);
	}
}

public class Button_Upgrade : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetUpgrade);

		object val;		
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		string strParentName = obj.transform.parent.gameObject.name;

		if(strParentName == "PetInfoUpgradeBtnGroup")
		{
			SetSelUpgradePetID("PetInfoWindow");
		}
		else if(strParentName == "PetInfoSingleWinUpgradeBtnGroup")
		{

			SetSelUpgradePetID("PET_INFO_SINGLE_WINDOW");
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "CLOSE", true);
		}
        else if (strParentName == "PvpPetInfoSingleWinUpgradeBtnGroup")
		{
            BaseUI.OpenWindow("MAIN_UI_WIDNOW", null, true);

            tEvent gemQuest = Net.StartEvent("CS_RequestGem");
            gemQuest.DoEvent();

            CS_RequestPetList petListQuest = Net.StartEvent("CS_RequestPetList") as CS_RequestPetList;
            petListQuest.mAction = () =>
            {
                DataCenter.Set("PET_INFO_WINDOW_PAGE_TYPE", (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetUpgrade);
                DataCenter.Set("SELECT_UPGRADE_PET_ID", (int)(DataCenter.GetData("PVP_PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID")));
                DataCenter.SetData("PVP_PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", -1);
                MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
            };

            petListQuest.mBackAction = () => MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
            petListQuest.set("IS_PET_BAG", true);
            petListQuest.DoEvent();
		}
		return true;
	}


	public void SetSelUpgradePetID(string strWindowName)
	{
		int iID = (int)(DataCenter.GetData(strWindowName).get ("SET_SELECT_PET_ID"));
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		PetData petData = petLogicData.GetPetDataByItemId(iID);
		
		if(petData != null)
		{
			DataCenter.SetData("PetUpgradeWindow", "SELECT_UPGRADE_PET", iID);
		}
		else
		{
            DEBUG.LogError("there is not exist petData");
		}
		
		DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_UPGRADE_PET_ICONS", iID);
		DataCenter.SetData(strWindowName, "SET_SELECT_PET_ID", -1);
	}
}

public class Button_Upgrade_JinHua : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution);

		object val;		
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		string strParentName = obj.transform.parent.gameObject.name;
		
		if(strParentName == "PetInfoUpgradeBtnGroup")
		{
			SetSelEvolutionPetID("PetInfoWindow");
		}
		else if(strParentName == "PetInfoSingleWinUpgradeBtnGroup")
		{
			SetSelEvolutionPetID("PET_INFO_SINGLE_WINDOW");
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "CLOSE", true);
		}
        else if (strParentName == "PvpPetInfoSingleWinUpgradeBtnGroup")
        {
            BaseUI.OpenWindow("MAIN_UI_WIDNOW", null, true);

            tEvent gemQuest = Net.StartEvent("CS_RequestGem");
            gemQuest.DoEvent();

            CS_RequestPetList petListQuest = Net.StartEvent("CS_RequestPetList") as CS_RequestPetList;
            petListQuest.mAction = () =>
            {
                DataCenter.Set("PET_INFO_WINDOW_PAGE_TYPE", (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution);
                DataCenter.Set("SELECT_EVOLUTION_PET_ID", (int)(DataCenter.GetData("PVP_PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID")));
                DataCenter.SetData("PVP_PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", -1);
                MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
            };

            petListQuest.mBackAction = () => MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
            petListQuest.set("IS_PET_BAG", true);
            petListQuest.DoEvent();
        }
		return true;
	}
	
	
	public void SetSelEvolutionPetID(string strWindowName)
	{
		int iID = (int)(DataCenter.GetData(strWindowName).get ("SET_SELECT_PET_ID"));
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		PetData petData = petLogicData.GetPetDataByItemId(iID);
		
		if(petData != null)
		{
			// evolution type
			int iEvolutionType = TableCommon.GetNumberFromEvolutionConsumeConfig(petData.starLevel, "TYPE");
			
			if(iEvolutionType == (int)Evolution_Type.NONE)
			{
				// can not evolution
				int i = 1;
			}
			else
			{
				if(iEvolutionType == (int)Evolution_Type.GEM)
				{
					// need gem
					int i = 3;
				}
				else if(iEvolutionType == (int)Evolution_Type.CARD)
				{
					// need card
					int i = 4;
				}

                DataCenter.SetData("PetEvolutionWindow", "SELECT_EVOLUTION_PET", petData.itemId);
			}
        }
        DataCenter.SetData(strWindowName, "SET_SELECT_PET_ID", -1);
	}
}

public class Button_Upgrade_QiangHua : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution);
		object val;		
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		string strParentName = obj.transform.parent.gameObject.name;
		
		if(strParentName == "PetInfoUpgradeBtnGroup")
		{
			SetSelEvolutionPetID("PetInfoWindow");
		}
		else if(strParentName == "PetInfoSingleWinUpgradeBtnGroup")
		{
			SetSelEvolutionPetID("PET_INFO_SINGLE_WINDOW");
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "CLOSE", true);
		}
        else if (strParentName == "PvpPetInfoSingleWinUpgradeBtnGroup")
        {
            BaseUI.OpenWindow("MAIN_UI_WIDNOW", null, true);

            tEvent gemQuest = Net.StartEvent("CS_RequestGem");
            gemQuest.DoEvent();

            CS_RequestPetList petListQuest = Net.StartEvent("CS_RequestPetList") as CS_RequestPetList;
            petListQuest.mAction = () =>
            {
                DataCenter.Set("PET_INFO_WINDOW_PAGE_TYPE", (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution);
                DataCenter.Set("SELECT_EVOLUTION_PET_ID", (int)(DataCenter.GetData("PVP_PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID")));
                DataCenter.SetData("PVP_PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", -1);
                MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
            };

            petListQuest.mBackAction = () => MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
            petListQuest.set("IS_PET_BAG", true);
            petListQuest.DoEvent();
        }
		return true;
	}
	
	
	public void SetSelEvolutionPetID(string strWindowName)
	{
		int iID = (int)(DataCenter.GetData(strWindowName).get ("SET_SELECT_PET_ID"));
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		PetData petData = petLogicData.GetPetDataByItemId(iID);
		
		if(petData != null)
		{
			// evolution type
			int iEvolutionType = TableCommon.GetNumberFromEvolutionConsumeConfig(petData.starLevel, "TYPE");
			
			if(iEvolutionType == (int)Evolution_Type.NONE)
			{
				// can not evolution
				int i = 1;
			}
			else
			{
				if(iEvolutionType == (int)Evolution_Type.GEM)
				{
					// need gem
					int i = 3;
				}
				else if(iEvolutionType == (int)Evolution_Type.CARD)
				{
					// need card
					int i = 4;
				}

                DataCenter.SetData("PetEvolutionWindow", "SELECT_EVOLUTION_PET", petData.itemId);
			}
		}
		DataCenter.SetData(strWindowName, "SET_SELECT_PET_ID", -1);
	}
}

public class Button_Upgrade_Skill1 : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetSkill);
        object val;
        bool b = getData("BUTTON", out val);
        GameObject obj = val as GameObject;
        string strParentName = obj.transform.parent.gameObject.name;
		
		if(strParentName == "PetInfoUpgradeBtnGroup")
		{
			//SetSelUpgradeSkillPetID("PetInfoWindow");
		}
		else if(strParentName == "PetInfoSingleWinUpgradeBtnGroup")
		{
            SetSelUpgradeSkillPetID("PET_INFO_SINGLE_WINDOW");
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "CLOSE", true);
		}
        else if (strParentName == "PvpPetInfoSingleWinUpgradeBtnGroup")
        {
            //BaseUI.OpenWindow("MAIN_UI_WIDNOW", null, true);

            //tEvent gemQuest = Net.StartEvent("CS_RequestGem");
            //gemQuest.DoEvent();

            //CS_RequestPetList petListQuest = Net.StartEvent("CS_RequestPetList") as CS_RequestPetList;
            //petListQuest.mAction = () =>
            //{
            //    DataCenter.Set("PET_INFO_WINDOW_PAGE_TYPE", (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution);
            //    DataCenter.Set("SELECT_EVOLUTION_PET_ID", (int)(DataCenter.GetData("PVP_PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID")));
            //    DataCenter.SetData("PVP_PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", -1);
            //    MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
            //};

            //petListQuest.mBackAction = () => MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
            //petListQuest.set("IS_PET_BAG", true);
            //petListQuest.DoEvent();
        }
		return true;
	}


    public void SetSelUpgradeSkillPetID(string strWindowName)
	{
        int iID = (int)(DataCenter.GetData(strWindowName).get("SET_SELECT_PET_ID"));
        PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        PetData petData = petLogicData.GetPetDataByItemId(iID);

        DataCenter.SetData("PET_SKILL_WINDOW", "SET_SELECT_UPGRADE_SKILL_PET", petData.itemId);
		DataCenter.SetData(strWindowName, "SET_SELECT_PET_ID", -1);
	}
}
public class Button_Upgrade_Unuse : CEvent
{
	public override bool _DoEvent()
	{
		int iID = (int)(DataCenter.GetData("PetInfoWindow").get ("SET_SELECT_PET_ID"));
		DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", iID);
		EventCenter.Start("Button_PetInfoSingleOKBtn").DoEvent();
		return true;
	}
}

public class Button_PetAndStoneSelBtn : CEvent
{
	public override bool _DoEvent()
	{
		object val;		
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		BAG_INFO_TITLE_TYPE type = BAG_INFO_TITLE_TYPE.Bag_Pet_Window_ButtonTitle;
		if(toggle.value)
		{
			type = BAG_INFO_TITLE_TYPE.Bag_Stone_Window_ButtonTitle;
		}

		DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", type);
		return true;
	}
}

public class Button_ChoosePet : CEvent
{
	public override bool _DoEvent()
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		int iID = (int)get("ID");
		PetData petData = petLogicData.GetPetDataByItemId(iID);
		
		if(petData != null)
		{
			int index = (int)DataCenter.GetData("AllPetAttributeInfoWindow").get ("CUR_UI_INDEX");
			//string strType = a.GetType().ToString();

			if(index == (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetUpgrade)
			{
				DataCenter.SetData("PetUpgradeWindow", "REMOVE_UPGRADE_PET", true);
				DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_PET_ICONS", true);
			}
			else if(index == (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution)
			{
				DataCenter.SetData("PetEvolutionWindow", "SET_EVOLUTION_STATE", (int)PetEvolutionWindow.Evolution_State.Clear);
				DataCenter.SetData("BAG_INFO_WINDOW", "CLEAR_STONE_SELECT_STATE", true);
				DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_PET_ICONS", true);
			}
		}

		return true;
	}
}

public class Button_PetUpgradeBtnOK : CEvent
{
	public override bool _DoEvent()
	{
		if(CommonParam.bIsNetworkGame)
			DataCenter.SetData("PetUpgradeWindow", "SEND_SERVER_PET_UPGRADE", true);
		else
			DataCenter.SetData("PetUpgradeWindow", "UPGRADE_PET_PLAY_EFFECT", true);
		return true;
	}
}

public class Button_PetStrengthenOK : CEvent
{
	public override bool _DoEvent()
	{
		if(CommonParam.bIsNetworkGame)
			DataCenter.SetData("PetEvolutionWindow", "SEND_SERVER_PET_STRENGTHEN_OR_EVOLUTION", true);
		else
			DataCenter.SetData("PetEvolutionWindow", "STRENGTHEN_OR_EVOLUTION_PET_PLAY_EFFECT", true);
		return true;
	}
}

public class Button_PetEvolutionBtnOK : CEvent
{
	public override bool _DoEvent()
	{
		if(CommonParam.bIsNetworkGame)
			DataCenter.SetData("PetEvolutionWindow", "SEND_SERVER_PET_STRENGTHEN_OR_EVOLUTION", true);
		else
			DataCenter.SetData("PetEvolutionWindow", "STRENGTHEN_OR_EVOLUTION_PET_PLAY_EFFECT", true);
		return true;
	}
}


public class Button_evolution_gain_pet_info_window_close_btn : CEvent
{
	public override bool _DoEvent()
    {
		DataCenter.SetData("evolution_gain_pet_info_window", "CLOSE", true);

		return true;		
	}
}

public class Button_UpGradeAndStrengthenWindowCloseBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("UpGradeAndStrengthenResultWindow", "CLOSE", true);
//		DataCenter.SetData("PetEvolutionWindow", "UPDATE_UI", true);

		return true;		
	}
}

public class Button_PetUpgradeAutoAddPetBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("PetUpgradeWindow", "AUTO_ADD_PET", true);
		return true;
	}
}

//public class Button_ShopBtn : CEvent
//{
//    void SendBattleResult(bool iswin)
//    {
//        int stageIndex = DataCenter.Get("CURRENT_STAGE");
//        tEvent quest = Net.StartEvent("CS_BattleResult");
//        quest.set("MAP_ID", stageIndex);
//        quest.set("ISWIN", iswin);

//        float battletime = Time.time;

//        quest.set("BATTLETIME", battletime);
//        quest.DoEvent();
//    }

//    public override bool _DoEvent()
//    {
//        SendBattleResult(true);

//        return true;
//    }
//}


//----------------------------------------------------------------------------------
// bag
public class Button_pet_icon_check_btn : CEvent
{
	public override bool _DoEvent()
	{
		object val;		
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		toggle.value = !toggle.value;
		
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		int iID = (int)get("ID");

		if(iID == 0)
			return false;

		PetData petData = petLogicData.GetPetDataByItemId(iID);
		
		if(petData != null)
		{
			DataCenter.OpenWindow("PET_INFO_SINGLE_WINDOW", PET_INFO_WINDOW_TYPE.PET);
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_BY_ID", iID);
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", iID);
			//DataCenter.SetData("PetInfoWindow", "SET_SELECT_PET_BUTTON", obj);
		}
		else
		{
//            DEBUG.LogError("there is not exist petData");
		}
		return true;
	}
}

public class Button_pet_icon_upgrade_btn : CEvent
{
	public override bool _DoEvent()
	{
		PetUpgradeWindow petUpgradeWindow = DataCenter.GetData("PetUpgradeWindow") as PetUpgradeWindow;
		if(petUpgradeWindow != null)
		{
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			int iID = (int)get("ID");

			bool bIsCan = false;
			if(iID != 0)
			{
				bIsCan = IsPetCanUpgrade(iID);
			}
			
			if(bIsCan)
			{
				SetSelPet();
			}
			else
			{
				object val;
				bool b = getData("BUTTON", out val);
				GameObject obj = val as GameObject;
				UIToggle toggle = obj.GetComponent<UIToggle>();
				toggle.value = false;
			}
			
			
		}
		else
		{
			DEBUG.LogError("petUpgradeWindow is null");
		}
		
		return true;
	}
	
	public void SetSelPet()
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		int iID = (int)get("ID");
		PetData petData = petLogicData.GetPetDataByItemId(iID);
		
		if (petData != null)
		{
			DataCenter.SetData("PetUpgradeWindow", "SET_SELECT_PET_BY_ID", iID);
		}
		else
		{
            DEBUG.LogError("there is not exist petData");
		}
	}

	public bool IsPetCanUpgrade(int iID)
	{
		PetUpgradeWindow petUpgradeWindow = DataCenter.GetData("PetUpgradeWindow") as PetUpgradeWindow;
		if(petUpgradeWindow != null)
		{
			return petUpgradeWindow.IsPetCanUpgrade(iID, true);
		}
		return false;
	}
}

public class Button_stone_icon_sel_btn : CEvent
{
	public override bool _DoEvent()
	{
		object val;
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		
		PetEvolutionWindow petEvolutionWindow = DataCenter.GetData("PetEvolutionWindow") as PetEvolutionWindow;
		if(petEvolutionWindow.mEvolutionState == (int)PetEvolutionWindow.Evolution_State.Clear)
		{
			if(toggle.value)
			{
				toggle.value = false;
				return true;
			}
		}
		
		
		if(!toggle.value)
		{
			toggle.value = true;
			return true;
		}
		
		DataCenter.SetData("BAG_INFO_WINDOW", "CLEAR_STONE_SELECT_STATE", true);
		
		toggle.value = true;
		
		GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
		int iIndex = (int)get("INDEX");
		GemData gemData = gemLogicData.GetGemDataByIndex(iIndex);
		
		if(gemData != null)
		{
			DataCenter.SetData("PetEvolutionWindow", "SET_SELECT_STONE_BY_INDEX", iIndex);
		}
		
		return true;
	}
}

public class ButtonRoleEquipIconBtn : CEvent
{
	public override bool _DoEvent()
	{
		//DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "OPEN", false);
		
		object val;		
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		toggle.value = !toggle.value;

		RoleEquipWindow window = DataCenter.GetData("RoleEquipWindow") as RoleEquipWindow;
		if(window != null)
		{
			RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
			int iGridIndex = (int)get("GRID_INDEX");

			if(window.mEquipType == EQUIP_TYPE.MAX)
			{
				EquipData equipData = logicData.GetEquipDataByGridIndex(iGridIndex);
				
				if(equipData != null)
				{
					DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "REFRESH", iGridIndex);
				}
			}
			else
			{
				EquipData equipData = logicData.GetEquipDataByGridIndex(iGridIndex, (int)window.mEquipType);
				if(equipData != null)
				{
                    DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "WINDOW_TYPE", (int)RoleEquipInfoWindow.WINDOW_TYPE.COMMON);
					DataCenter.OpenWindow("ROLE_EQUIP_INFO_WINDOW", equipData.itemId);
				}
			}
		}

		return true;
	}
}

public class ButtonRoleEquipStrengthenIconBtn : CEvent
{
	public override bool _DoEvent()
	{
		RoleEquipWindow window = DataCenter.GetData("RoleEquipWindow") as RoleEquipWindow;
		if(window != null)
		{
			RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
			int iItemId = (int)get("DBID");

			bool bIsCan = IsEquipCanStrengthen(iItemId);
			
			if(bIsCan)
			{
				SetSelEquip();
			}
			else
			{
				object val;
				bool b = getData("BUTTON", out val);
				GameObject obj = val as GameObject;
				UIToggle toggle = obj.GetComponent<UIToggle>();
				toggle.value = false;
			}

			EquipData equipData = logicData.GetEquipDataByItemId(iItemId);
			
			if(equipData != null)
			{
				DataCenter.SetData("ExpansionInfoWindow", "REFRESH", true);
			}
		}
		
		return true;
	}
	
	public void SetSelEquip()
	{
		RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		int iItemId = (int)get("DBID");
		EquipData equipData = logicData.GetEquipDataByItemId(iItemId);
		
		if (equipData != null)
		{
			DataCenter.SetData("ExpansionInfoWindow", "SET_SELECT_EQUIP_BY_ID", equipData.itemId);
		}
		else
		{
			DEBUG.LogError("there is not exist equipData");
		}
	}
	
	public bool IsEquipCanStrengthen(int iItemId)
	{
		ExpansionInfoWindow window = DataCenter.GetData("ExpansionInfoWindow") as ExpansionInfoWindow;
		if(window != null)
		{
			return window.IsEquipCanStrengthen(iItemId, true);
		}
		return false;
	}
}

public class Button_pet_group_sifting_button : CEvent
{
	public override bool _DoEvent()
	{
		GameCommon.PlayUIPlayTween (null);
		return true;
	}
}

public class Button_PetStarBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_INFO_WINDOW", "SORT_PET_ICONS", SORT_TYPE.STAR_LEVEL);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

public class Button_PetLevelBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_INFO_WINDOW", "SORT_PET_ICONS", SORT_TYPE.LEVEL);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

public class Button_PetAttributeBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BAG_INFO_WINDOW", "SORT_PET_ICONS", SORT_TYPE.ELEMENT_INDEX);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

public class Button_RoleEquipStarBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("RoleEquipWindow", "SORT_ROLE_EQUIP_ICONS", SORT_TYPE.STAR_LEVEL);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

public class Button_RoleEquipStrengthenLevelBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("RoleEquipWindow", "SORT_ROLE_EQUIP_ICONS", SORT_TYPE.STRENGTHEN_LEVEL);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

public class Button_RoleEquipAttributeBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("RoleEquipWindow", "SORT_ROLE_EQUIP_ICONS", SORT_TYPE.ELEMENT_INDEX);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

//----------------------------------------------------------------------------------
// common ui
public class Button_PetInfoSingleCloseBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "CLOSE", true);
		DataCenter.CloseWindow("COMMON_TIP_WINDOW");

		if(GameCommon.bIsWindowOpen ("SHOP_WINDOW"))
			DataCenter.SetData ("SHOP_WINDOW", "SHOW_GAIN_PET_WINDOW_CARD", true);
		else if(GameCommon.bIsWindowOpen ("ACTIVE_LIST_WINDOW"))
			DataCenter.SetData ("ACTIVE_LIMIT_BUY_PET_WINDOW", "SHOW_GAIN_PET_WINDOW_CARD", true);
		return true;
	}
}

public class Button_PetInfoSingleOKBtn : CEvent
{
	public void SetChangePetUsePos()
	{
        int iID = (int)(DataCenter.GetData("PET_INFO_SINGLE_WINDOW").get("SET_SELECT_PET_ID"));
        if (CommonParam.bIsNetworkGame)
        {
            RequestChangePetUsePos(iID);
        }
		else
		{
			ChangePetUsePos(true);
		}
	}
	
	public void RequestChangePetUsePos(int iID)
	{
        PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        PetData petData = petLogicData.GetPetDataByItemId(iID);
        
		int iTeam = petLogicData.mCurrentTeam;

		if(petData != null)
		{
			CS_RequestChangePetUsePos quest = Net.StartEvent("CS_RequestChangePetUsePos") as CS_RequestChangePetUsePos;
            quest.set("WINDOW", "SINGKE_PET_BAG");
            quest.set("PET_ID", petData.itemId);
            quest.set("TEAM_ID", iTeam);
            quest.set("EQUIP", false);
            quest.set("USE_POS", petLogicData.GetPosInTeam(iTeam, iID));
			quest.DoEvent();
		}
	}
	
	public static void ChangePetUsePos(bool bIsSuccess)
	{
		if(bIsSuccess)
		{
			int iID = (int)(DataCenter.GetData("PET_INFO_SINGLE_WINDOW").get ("SET_SELECT_PET_ID"));
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			PetData petData = petLogicData.GetPetDataByItemId(iID);
			int iTeam = petLogicData.mCurrentTeam;
			
			if(petData != null)
			{
                if (petLogicData.IsPetUsedInTeam(iTeam, iID))
				{
					// has use
                    int usePos = petLogicData.GetPosInTeam(iTeam, iID);

                    petLogicData.SetPosInTeam(iTeam, usePos, 0);

					DataCenter.SetData("PetPlayCheckBoxData" + usePos.ToString(), "SET_USE_POS", usePos);
					
					DataCenter.SetData("PetInfoWindow", "SET_SELECT_PET", usePos);
				}
				DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", -1);
			}
			else
			{
                DEBUG.LogError("there is not exist petData");
			}
			
			DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_PET_ICONS", true);
		}
		else
		{
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", -1);
		}
		DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "CLOSE", true);
	}
	
	public override bool _DoEvent()
	{		
		//string str = objVal.GetType().ToString();
		AllPetAttributeInfoWindow objVal = DataCenter.GetData("AllPetAttributeInfoWindow") as AllPetAttributeInfoWindow;
		
		int index = (int)(DataCenter.GetData("AllPetAttributeInfoWindow").get ("CUR_UI_INDEX"));
		//string strType = a.GetType().ToString();
		if(index == (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetInfo)
		{
			int iID = (int)(DataCenter.GetData("PET_INFO_SINGLE_WINDOW").get ("SET_SELECT_PET_ID"));
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			PetData petData = petLogicData.GetPetDataByItemId(iID);
			int iTeam = petLogicData.mCurrentTeam;
			//string str = objVal.GetType().ToString();
			if(petData != null)
			{
				DataCenter.SetData("PetInfoWindow", "SET_SELECT_USE_PET_ID", iID);
				if (petLogicData.IsPetUsedInTeam(iTeam, iID))
				{
					// has use
					SetChangePetUsePos();
				}
				else
				{
					// has not use
					bool bIsFull = true;
					PetData[] petDatas = petLogicData.GetPetDatasByTeam(iTeam);
					for(int i = 0; i < petDatas.Length; i++)
					{
						if(petDatas[i] == null)
						{
							Button_pet_play_flag_btn btnEvent = EventCenter.Start("Button_pet_play_flag_btn") as Button_pet_play_flag_btn;
							btnEvent.set("POS", i+1);
							btnEvent.DoEvent();
							bIsFull = false;
							break;
						}
					}

					if(bIsFull)
					{
						// is not full
						DataCenter.SetData("PetInfoWindow", "SHOW_PET_FLAG", true);
						DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_PET_ICONS", true);
					}
				}
			}
			else
			{
                DEBUG.LogError("there is not exist petData");
			}
		}
		else if(index == (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetUpgrade)
		{
			int iID = (int)(DataCenter.GetData("PET_INFO_SINGLE_WINDOW").get ("SET_SELECT_PET_ID"));
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			PetData petData = petLogicData.GetPetDataByItemId(iID);
			
			if(petData != null)
			{
				DataCenter.SetData("PetUpgradeWindow", "SELECT_UPGRADE_PET", petData.itemId);
			}
			else
			{
                DEBUG.LogError("there is not exist petData");
			}
			
			DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_UPGRADE_PET_ICONS", iID);
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", -1);
		}
        else if (index == (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetSkill)
        {
            int iID = (int)(DataCenter.GetData("PET_INFO_SINGLE_WINDOW").get("SET_SELECT_PET_ID"));
            PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
            PetData petData = petLogicData.GetPetDataByItemId(iID);

            if (petData != null)
            {
                DataCenter.SetData("PET_SKILL_WINDOW", "SET_SELECT_UPGRADE_SKILL_PET", petData.itemId);
            }
            else
            {
                DEBUG.LogError("there is not exist petData");
            }
            DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", -1);
        }
		else if (index == (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetDecompose)
		{
			int iID = (int)(DataCenter.GetData("PET_INFO_SINGLE_WINDOW").get("SET_SELECT_PET_ID"));
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			PetData petData = petLogicData.GetPetDataByItemId(iID);
			
			if (petData != null)
			{
				DataCenter.SetData("PET_DECOMPOSE_WINDOW", "IS_ADD", true);
				DataCenter.SetData("PET_DECOMPOSE_WINDOW", "ADD_OR_REMOVE_SELECT_DECOMPOSE_PET", petData.itemId);
			}
			else
			{
				DEBUG.LogError("there is not exist petData");
			}
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", -1);
		}
		else if(index == (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution)
		{
			DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution);

			int iID = (int)(DataCenter.GetData("PET_INFO_SINGLE_WINDOW").get ("SET_SELECT_PET_ID"));
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			PetData petData = petLogicData.GetPetDataByItemId(iID);
			
			if(petData != null)
			{
				// evolution type
				int iEvolutionType = TableCommon.GetNumberFromEvolutionConsumeConfig(petData.starLevel, "TYPE");
				
				if(iEvolutionType == (int)Evolution_Type.NONE)
				{
					// can not evolution
					int i = 1;
				}
				else
				{
					if(iEvolutionType == (int)Evolution_Type.GEM)
					{
						// need gem
						int i = 3;
					}
					else if(iEvolutionType == (int)Evolution_Type.CARD)
					{
						// need card
						int i = 4;
					}

                    DataCenter.SetData("PetEvolutionWindow", "SELECT_EVOLUTION_PET", petData.itemId);
				}
			}
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_ID", -1);
		}
		
		DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "CLOSE", true);
		return true;
		
	}
}


public class Button_PetInfoSingleSaleBtn : CEvent
{
	public override bool _DoEvent ()
	{
        object val;
        bool b = getData("BUTTON", out val);
        GameObject obj = val as GameObject;
        string strParentName = obj.transform.parent.gameObject.name;
        string strWindowName = "";
        if (strParentName == "PetInfoSingleWinUpgradeBtnGroup")
        {
            strWindowName = "PET_INFO_SINGLE_WINDOW";
        }
        else if (strParentName == "PvpPetInfoSingleWinUpgradeBtnGroup")
        {
            strWindowName = "PVP_PET_INFO_SINGLE_WINDOW";
        }

        DoSale(strWindowName);

		return true;
	}

    private void DoSale(string strWindowName)
    {
        PetInfoSingleBaseWindow window = DataCenter.GetData(strWindowName) as PetInfoSingleBaseWindow;
        int iItemId = (int)(window.get("SET_SELECT_PET_ID"));
        PetLogicData logic = DataCenter.GetData("PET_DATA") as PetLogicData;
        PetData pet = logic.GetPetDataByItemId(iItemId);
        if (pet != null)
        {
            string[] kind_value = TableCommon.GetStringFromActiveCongfig(pet.tid, "SELL_PRICE").Split('#');
            int iPrice = int.Parse(kind_value[1]);
            iPrice = (int)(iPrice * (1 + 0.05 * pet.level));
            DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_PET_SALE, iPrice.ToString(), () => DataCenter.SetData(strWindowName, "SALE_PET", true));
        }
    }
}

public class Button_PetInfoDescriptionWindowCloseBtn : CEvent
{
	public override bool _DoEvent()
	{
		//DataCenter.SetData("PetInfoDescriptionWindow", "CLOSE", true);        
		PetInfoDescriptionWindow window = DataCenter.GetData("PetInfoDescriptionWindow") as PetInfoDescriptionWindow;
		//window.CloseSelf();
		return true;
	}
}

//public class Button_Skill_Button : CEvent
//{
//	public override bool _DoEvent()
//	{
//		EventCenter.Start("Button_tip_btn").DoEvent();
//
//		return true;
//	}
//}

public class Button_TipBackground : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow("COMMON_TIP_WINDOW");
		return true;
	}
}

public class Button_SkillTip : CEvent
{
	public override bool _DoEvent()
	{
		return true;
	}
}

//----------------------------------------------------------------------------------
// MissionWindow
public class Button_MissionWindowBack : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
		return true;
	}
}

public class Button_daily_tasks_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("MissionWindow", "SET_SEL_PAGE", TASK_PAGE_TYPE.DAILY);
		return true;
	}
}

public class Button_achievment_tasks_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("MissionWindow", "SET_SEL_PAGE", TASK_PAGE_TYPE.ACHIEVEMENT);
		return true;
	}
}

public class Button_weekly_tasks_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("MissionWindow", "SET_SEL_PAGE", TASK_PAGE_TYPE.WEEKLY);
		return true;
	}
}

public class Button_activity_tasks_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("MissionWindow", "SET_SEL_PAGE", TASK_PAGE_TYPE.ACTIVITY);
		return true;
	}
}

public class Button_but_get_task_award : CEvent
{
	public override bool _DoEvent()
	{
		object obj;
		getData("BUTTON", out obj);
		GameObject btn = obj as GameObject;
		if(btn != null)
		{
			UILabel label = btn.GetComponent<UILabel>();
            int iTaskID = Convert.ToInt32(label.text);

			if(CommonParam.bIsNetworkGame)
			{
                //TaskSystemMgr.Self.DeliverTask(iTaskID);
				tEvent quest = Net.StartEvent("CS_TaskAcceptAwardResult");		
				quest.set("TASK_ID", iTaskID);
				quest.DoEvent();
                //DataCenter.SetData("MissionWindow", "DELIVER_TASK_ID", iTaskID);
			}
			else
			{
				DataCenter.SetData("MissionWindow", "ACCEPT_TASK_AWARD", iTaskID);
			}
		}
		return true;
	}
}

public class Button_go_task_btn : CEvent
{
	public override bool _DoEvent()
	{
		object obj;
		getData("BUTTON", out obj);
		GameObject btn = obj as GameObject;
		if(btn != null)
		{
			UILabel label = btn.GetComponent<UILabel>();
			int iTaskTypeID = Convert.ToInt32(label.text);

			if(iTaskTypeID ==1 || iTaskTypeID == 2 || iTaskTypeID == 4 || iTaskTypeID == 16 ||iTaskTypeID ==26 )
			{
				//跳转到当前最后的通关关卡列表界面
				MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);

			}else if(iTaskTypeID == 3 || iTaskTypeID == 5)
			{
				//跳转到斗法列表界面
				MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
				MainUIScript.Self.OpenMainUI();
				DataCenter.OpenWindow("PVP_SELECT_ENTER", "FOUR_VS_FOUR");

			}else if(iTaskTypeID == 6 || iTaskTypeID == 7)
			{
				//跳转到金钱蛋副本列表
				MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
				MainUIScript.Self.mStrWorldMapSubWindowName = "ACTIVE_STAGE_WINDOW";
			}else if(iTaskTypeID == 8 || iTaskTypeID == 9 || iTaskTypeID == 11)
			{
				//跳转到好友列表
				MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.FriendWindow);
			}else if(iTaskTypeID == 10 || iTaskTypeID == 25 )
			{
				//进入商店抽符灵页面
				int i = Convert.ToInt32 (SHOP_PAGE_TYPE.PET);
				DataCenter.Set ("WHICH_SHOP_PAGE", i);
				
				if(DataCenter.GetData ("SHOP_WINDOW") != null)
                    //by chenliang
                    //begin

//					DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.DIAMOND);
//------------------------------
                    DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.PET);

                    //end
				else
				{
					GlobalModule.ClearAllWindow();
					MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
				}
			}else if(iTaskTypeID == 12 || iTaskTypeID == 13 ||iTaskTypeID == 14 || iTaskTypeID == 15)
			{
				//进入宠物主界面
				MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
				DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution);
				DataCenter.CloseWindow ("BECOME_STRONG_WINDOW");
			}else if(iTaskTypeID == 19 )
			{
				//进入商店抽法宝页面
				int i = Convert.ToInt32 (SHOP_PAGE_TYPE.TOOL);
				DataCenter.Set ("WHICH_SHOP_PAGE", i);
				
				if(DataCenter.GetData ("SHOP_WINDOW") != null)
                    //by chenliang
                    //begin

//					DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.DIAMOND);
//---------------------------
                    DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.PET);

                    //end
				else
				{
					GlobalModule.ClearAllWindow();
					MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
				}
			}else if(iTaskTypeID == 20 || iTaskTypeID == 21)
			{
				//进入法宝管理界面（强化功能需要修改为升级）
				MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
				MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "ROLE_EQUIP_CULTIVATE_WINDOW";
			}else if(iTaskTypeID == 23)
			{
				//打开挂机功能界面
				tEvent evt = Net.StartEvent("CS_RequestIdleBottingStatus");
				evt.set ("WINDOW_NAME", "ON_HOOK_WINDOW");
				evt.DoEvent();
			}else if(iTaskTypeID == 24 )
			{
				//跳转到签到页面
				DataCenter.OpenWindow("DAILY_SIGN_WINDOW");

			}else 
			{
				//

			}

		}
		return true;
	}
}

public class Button_go_to_get_btn : CEvent
{
	public override bool _DoEvent()
    {
        //去掉队伍界面的返回函数
        MainUIScript.Self.mWindowBackAction = null;

        int iIndexNum = get("FUNCTION_INDEX");
        DataCenter.CloseWindow("TEAM_WINDOW");
        DataCenter.CloseWindow("PET_DETAILS_WINDOW");
        DataCenter.CloseWindow("PET_ALBUM_WINDOW");
        DataCenter.CloseWindow("PACKAGE_EQUIP_WINDOW");
		DataCenter.CloseWindow("BAG_PET_WINDOW");
		DataCenter.CloseWindow ("RECOVER_WINDOW");
		DataCenter.CloseWindow ("NEW_MYSTERIOUS_SHOP_WINDOW");
        DataCenter.CloseWindow(UIWindowString.access_to_res_window);
        DataCenter.CloseWindow(UIWindowString.union_worship);
//		MainUIScript.Self.OpenMainWindowByIndex (MAIN_WINDOW_INDEX.RoleSelWindow);
        //在抽卡界面
        tWindow _jishiWin = DataCenter.GetData("SHOP_WINDOW") as tWindow;
        if (_jishiWin != null && _jishiWin.IsOpen())
        {
            DataCenter.SetData("shop_gain_pet_window", "CLOSE", true);
            DataCenter.SetData("SHOP_WINDOW", "UI_HIDDEN", true);
            DataCenter.CloseWindow("SHOP_WINDOW");
            DataCenter.CloseWindow("SHOP_ALBUM_WINDOW");
        }
        //by chenliang
        //begin

        //开服狂欢
        tWindow tmpSevenDaysWin = DataCenter.GetData("SEVEN_DAYS_CARNIVAL_WINDOW") as tWindow;
        if(tmpSevenDaysWin != null && tmpSevenDaysWin.IsOpen())
        {
            DataCenter.CloseWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW");
            DataCenter.CloseWindow("SEVEN_DAYS_CARNIVAL_WINDOW");
        }

        //end

        tWindow _grabTreasureWin = DataCenter.GetData("GRABTREASURE_WINDOW") as tWindow;
        if (_grabTreasureWin != null && _grabTreasureWin.IsOpen()) 
        {
            DataCenter.CloseWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW");
            DataCenter.CloseWindow("GRABTREASURE_WINDOW");
        }

        tWindow _equipDetailWin = DataCenter.GetData("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW") as tWindow;
        if (_equipDetailWin != null && _equipDetailWin.IsOpen()) 
        {
            DataCenter.CloseWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW");
        }
        //MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);

		tWindow _trialWindow = DataCenter.GetData ("TRIAL_WINDOW") as tWindow;
		if(_trialWindow != null && _trialWindow.IsOpen())
		{
			DataCenter.CloseWindow ("PVP_PRESTIGE_SHOP_WINDOW");
			DataCenter.CloseWindow ("ARENA_MAIN_WINDOW");
			DataCenter.CloseWindow ("TRIAL_WINDOW");
			DataCenter.CloseWindow("TRIAL_WINDOW_BACK");
			DataCenter.CloseWindow(UIWindowString.arena_window_back);
			DataCenter.CloseWindow("GRABTREASURE_WINDOW");
			DataCenter.CloseWindow("FEATS_SHOP_WINDOW");
			DataCenter.CloseWindow("SHOP_FEATS_WINDOW_BACK");
			DataCenter.CloseWindow("BOSS_RAID_WINDOW");
		}

		tWindow _activityWindow = DataCenter.GetData ("ACTIVITY_WINDOW") as tWindow;
		if(_activityWindow != null && _activityWindow.IsOpen())
		{
			MainUIScript.Self.OpenMainWindowByIndex (MAIN_WINDOW_INDEX.RoleSelWindow);
		}

		tWindow _unionWindow = DataCenter.GetData ("UNION_SHOP_CONTAINER_WINDOW") as tWindow;
		if(_unionWindow != null && _unionWindow.IsOpen())
		{
			DataCenter.CloseWindow("UNION_SHOP_CONTAINER_WINDOW");
			DataCenter.CloseWindow("UNION_SHOP_LIMIT_WINDOW");
			DataCenter.CloseWindow("UNION_SHOP_AWARD_WINDOW");
			DataCenter.CloseWindow("UNION_SHOP_PROP_WINDOW");
			DataCenter.CloseWindow("UNION_SHOP_CLOTH_WINDOW");
		}

        tWindow _vipWindow = DataCenter.GetData("RECHARGE_CONTAINER_WINDOW") as tWindow;
        if (_vipWindow != null && _vipWindow.IsOpen())
        {
            DataCenter.CloseWindow("RECHARGE_CONTAINER_WINDOW");
        }

        if (iIndexNum < 0)
        {
            //跳转到当前最后的通关关卡列表界面
            GlobalModule.DoOnNextUpdate(2, () => 
            {
                MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
                GlobalModule.DoLater(() => Select(), 0f);
            });     
        }
        else
        {
            System.Action _goToGetFragmentWinHandler = null;
            int iTypeId = TableCommon.GetNumberFromGainFunctionConfig(iIndexNum, "Type");
            GET_PARTH_TYPE parthType = (GET_PARTH_TYPE)iTypeId;
            if (GetPathHandlerDic.HandlerDic.TryGetValue(parthType, out _goToGetFragmentWinHandler))
            {
                _goToGetFragmentWinHandler();
            }
            if (_goToGetFragmentWinHandler == null)
                DEBUG.LogError("GetPathHandlerDic中不存在该iIndexNum:" + iIndexNum.ToString());
        }
        return true;
    }
	private void Select()
	{
		int index = get("LEVEL_INDEX");
		int difficultyIndex = get ("DIFFICULTY_INDEX");
		DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_LEFT", "INIT_POPUP_LIST", difficultyIndex);
		DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_DIFFICULTY", difficultyIndex);
		DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_POINT", index);
	}
}

//----------------------------------------------------------------------------------
// ShopWindow
public class Button_shop_window_back : CEvent
{
	public override bool _DoEvent()
	{
//		MainProcess.ClearBattle();
//		MainProcess.LoadRoleSelScene();

//		DataCenter.CloseWindow("PVP_SELECT_ENTER");

        //by chenliang

        DataCenter.CloseWindow("SHOP_WINDOW");

        //end
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);

        if (MainUIScript.Self.mShopBackAction != null) 
        {
            MainUIScript.Self.mShopBackAction();
            MainUIScript.Self.mShopBackAction = null;
        }
        MainUIScript.Self.mWindowBackAction = null;
		return true;
	}
}

public class Button_pet_shop_btn1 : CEvent
{
    public override bool _DoEvent()
    {
		int i = Convert.ToInt32 (SHOP_PAGE_TYPE.PET);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
        //by chenliang
        //begin

//		DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.PET);
//----------------
        DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.PET);

        //end
        return true;
    }
}

public class Button_tool_shop_btn1 : CEvent
{
    public override bool _DoEvent()
    {
		int i = Convert.ToInt32 (SHOP_PAGE_TYPE.TOOL);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
        //by chenliang
        //begin

//		DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.TOOL);
//-----------------
        DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.TOOL);

        //end
        return true;
    }
}


public class Button_character_skin_shop_btn1 : CEvent
{
    public override bool _DoEvent()
    {
		int i = Convert.ToInt32 (SHOP_PAGE_TYPE.CHARACTER);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
        //by chenliang
        //begin

//		DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.CHARACTER);
//-----------------
        DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.CHARACTER);

        //end
        return true;
    }
}

public class Button_gold_shop_btn : CEvent
{
    public override bool _DoEvent()
    {
		int i = Convert.ToInt32 (SHOP_PAGE_TYPE.GOLD);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
		DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.GOLD);
        return true;
    }
}

public class Button_diamond_shop_btn : CEvent
{
    public override bool _DoEvent()
    {
		int i = Convert.ToInt32 (SHOP_PAGE_TYPE.DIAMOND);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
		DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.DIAMOND);
        return true;
    }
}
public class Button_mysterious_shop_btn : CEvent
{
	public override bool _DoEvent()
	{
		if(GameCommon.bIsWindowOpen("MYSTERIOUS_SHOP_WINDOW"))
			return true;

		int i = Convert.ToInt32 (SHOP_PAGE_TYPE.MYSTERIOUS);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
		DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.MYSTERIOUS);
		return true;
	}
}

public class Button_shop_gain_pet_window_close_btn : CEvent
{
    public override bool _DoEvent()
    {
		DataCenter.SetData("shop_gain_pet_window", "CLOSE", true);

		DataCenter.SetData ("SHOP_WINDOW", "UI_HIDDEN", true);
        return true;
    }
}

public class Button_shop_gain_pet_window_details_btn : CEvent
{
	public override bool _DoEvent()
	{
		shop_gain_pet_window petInfoWindow = DataCenter.GetData("shop_gain_pet_window") as shop_gain_pet_window;
		if(petInfoWindow != null)
		{
			int iModelIndex = petInfoWindow.get ("SET_SELECT_PET_BY_MODEL_INDEX");
            //by chenliang
            //begin

// 			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_BY_MODEL_INDEX", iModelIndex);
// 			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "OPEN", PET_INFO_WINDOW_TYPE.SHOP);
// 
// 			DataCenter.SetData ("SHOP_WINDOW", "SHOW_GAIN_PET_WINDOW_CARD", false);
//--------------------
            DataCenter.OpenWindow(UIWindowString.petDetail, iModelIndex);

            //end
		}
		return true;
	}
}

public class Button_shop_gain_item_window_close_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("shop_gain_item_window", "CLOSE", true);

		DataCenter.SetData ("SHOP_WINDOW", "UI_HIDDEN", true);
		return true;
	}
}

public class Button_shop_gain_pet_and_info_window_close_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("shop_gain_pet_and_info_window", "CLOSE", true);
		return true;
	}
}

public class Button_but_shop_buy : CEvent
{
	public bool isDiscount = false;
	GameObject button;
	public override bool _DoEvent()
	{
		button = (GameObject)getObject ("BUTTON");
		button.GetComponent<BoxCollider>().enabled = false;
		WaitTime (0.1f);
		int iGridIndex = get ("GRID_INDEX");
		int iShopSlotIndex = get ("SHOP_SLOT_INDEX");

		int getItemCount = TableCommon.GetNumberFromShopSlotBase (iShopSlotIndex, "EXTRACT_TIMES");
		if(!JudgeMailWillFull(getItemCount))
		{
			DataCenter.SetData ("SHOP_WINDOW", "IS_DISCOUNT", isDiscount);
			DataCenter.SetData("SHOP_WINDOW", "SHOP_SLOT_INDEX", iShopSlotIndex);
			DataCenter.SetData("SHOP_WINDOW", "BUY_ITEM_BY_GRID_INDEX", iGridIndex);
		}
		return true;
	}

	bool JudgeMailWillFull(int addCount)
	{
		int maxMailCount = DataCenter.mGlobalConfig.GetData ("MAX_MAIL_COUNT", "VALUE");
//		if(RoleLogicData.Self.mMailNum + (addCount - RoleLogicData.Self.GetFreeSpaceInPetBag ()) > maxMailCount 
//		   || RoleLogicData.Self.mMailNum + (addCount - RoleLogicData.Self.GetFreeSpaceInRoleEquipBag ()) > maxMailCount)
		//不考虑背包的剩余空间（背包数据不是实时同步）
		if(RoleLogicData.Self.mMailNum + addCount> maxMailCount) 
		{
			DataCenter.SetData("shop_gain_item_window", "CLOSE", true);
			DataCenter.SetData("shop_gain_pet_window", "CLOSE", true);
			DataCenter.SetData ("SHOP_WINDOW", "UI_HIDDEN", true);
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_SHOP_FULL_MAIL);
			return true;
		}

		return false;
	}

	public override void _OnOverTime ()
	{
		button.GetComponent<BoxCollider>().enabled = true;
	}
}

public class Button_but_shop_buy_disable : CEvent
{
	public bool isDiscount = false;
	public override bool _DoEvent()
	{
        DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NEED_BUY_CHARACET);
		return true;
	}
}

public class Button_but_shop_buy_again : Button_but_shop_buy
{
	public override bool _DoEvent()
	{
		isDiscount = (bool)getObject ("IS_DISCOUNT");
		DataCenter.SetData("shop_gain_pet_window", "CLOSE", true);
		DataCenter.SetData("shop_gain_item_window", "CLOSE", true);
		base._DoEvent();
		return true;
	}
}


public class Button_fa_bao_button : CEvent
{
	public override bool _DoEvent()
	{
		string strAction = get ("ACTION");
		if (CommonParam.bIsNetworkGame)
		{
			CS_RequestRoleEquip quest = Net.StartEvent("CS_RequestRoleEquip") as CS_RequestRoleEquip;
			quest.set ("WINDOW_NAME", "ROLE_EQUIP_CULTIVATE_WINDOW");

			quest.set ("ACTION", strAction);
			quest.mAction = () =>MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
			if(strAction == "FIVE_COPY_LEVEL_ACTION")
			{
				quest.mBackAction = () =>{
					DataCenter.OpenWindow ("FIVE_COPY_LEVEL_SELECT_WINDOW");
					DataCenter.OpenWindow ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW");
					MainUIScript.Self.GoBack();
				};
			}
			else
			{
//				quest.mAction = () =>MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
				quest.mBackAction = () => {
					MainUIScript.Self.GoBack();
					DataCenter.Set ("IS_WINDOW_BACK", true);
				};
			}
			quest.DoEvent();

			tEvent gemQuest = Net.StartEvent("CS_RequestGem");
			gemQuest.DoEvent();
		}
		else
		{
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
			MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "ROLE_EQUIP_CULTIVATE_WINDOW";
		}
		return true;
	}
}


// Help Tips

public class Button_help_tip : CEvent
{
    public override bool _DoEvent()
    {
        string name = get("NAME");

        if (name.Length > 9)
        {
            string strIndex = name.Substring(9);
            int index = Convert.ToInt32(strIndex);
            DataCenter.OpenHelpMessageWindow(index);
        }

        return true;
    }
}

public class Button_gary : CEvent
{
	public override bool _DoEvent()
	{
		UNLOCK_FUNCTION_TYPE type = (UNLOCK_FUNCTION_TYPE)getObject ("TYPE");
		DataCenter.OpenWindow ("PLAYER_LEVEL_UP_SHOW_WINDOW", false);
		DataCenter.SetData ("PLAYER_LEVEL_UP_SHOW_WINDOW", "NEED_LEVEL", type);
		return true;
	}
}

public class Button_order_button :CEvent
{
	public override bool _DoEvent()
	{
		bool bDescendingOrder = DataCenter.Get ("DESCENDING_ORDER");
		DataCenter.Set ("DESCENDING_ORDER", !bDescendingOrder);

		(getObject ("BUTTON") as GameObject).GetComponentInChildren<UISprite>().spriteName = bDescendingOrder ? "ui_s" : "ui_j";

		string windowName = getObject ("WINDOW_NAME").ToString ();
		DataCenter.SetData(windowName, "ORDER_RULE", true);

		return true;
	}
}

class Button_active_list_button : CEvent
{
	public override bool _DoEvent ()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ActivityWindow);
		NetManager.RequstActivitySevenDayLoginQuery();
		GlobalModule.DoCoroutine(_DoAction());
		return true;
	}

	private IEnumerator _DoAction()
	{
		yield return NetManager.StartWaitDailySignQuery();
		DataCenter.SetData("ACTIVITY_SIGN_WINDOW", "REFRESH", null);
	}
}

class Button_open_fund_button : CEvent
{
	public override bool _DoEvent ()
	{
		EventCenter.Start ("Button_active_list_button").DoEvent ();
		int configIndex = (int)ACTIVITY_TYPE.ACTIVITY_FUND;
		GlobalModule.DoLater (() => LaterAction (configIndex), 0);
		return true;
	}
	void LaterAction(int iIndex)
	{
		ActivityWindow _tWindow = DataCenter.GetData("ACTIVITY_WINDOW") as ActivityWindow;
		_tWindow.RefreshTab (iIndex);
		DataCenter.SetData ("ACTIVITY_WINDOW", "SHOW_WINDOW", iIndex);
//		DataCenter.SetData ("ACTIVITY_WINDOW", "CHANGE_TAB_POS", iIndex - 1);
	}
}

class Button_first_recharge_button : CEvent
{
	public override bool _DoEvent ()
	{
        EventCenter.Start("Button_active_list_button").DoEvent();
        int configIndex = (int)ACTIVITY_TYPE.ACTIVITY_FIRST_RECHARGE;
        GlobalModule.DoLater(() => LaterAction(configIndex), 0);
		return true;
	}
	void LaterAction(int iIndex)
	{
		ActivityWindow _tWindow = DataCenter.GetData("ACTIVITY_WINDOW") as ActivityWindow;
		_tWindow.RefreshTab (iIndex);
		DataCenter.SetData ("ACTIVITY_WINDOW", "SHOW_WINDOW", iIndex);
	}
}

public class Button_PackageEquipBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("PACKAGE_EQUIP_WINDOW");
		MainUIScript.Self.HideMainBGUI ();
        return true;
    }
}

public class Button_PackageConsumeBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenBackWindow("PACKAGE_CONSUME_WINDOW", "a_ui_baoguo_logo", () => MainUIScript.Self.ShowMainBGUI(), 120);
        DataCenter.OpenWindow("PACKAGE_CONSUME_WINDOW");
		MainUIScript.Self.HideMainBGUI();
        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.PACKAGE);
        return true;
    }
}

/// <summary>
/// 关闭按钮所在的窗口
/// 公用事件，请勿修改，如有附加操作请单独定义按钮事件
/// </summary>
public class Button_close_owner_window : CEvent
{
    public override bool _DoEvent()
    {
        this.CloseOwnerWindow();
		GameObject .Find ("task_back_window").GetComponent <UIPanel  > ().depth=16 ;
        return true;
    }
}

// TODO
public class Button_SentGuildExpNumBtn : CEvent
{
	public override bool _DoEvent()
	{
		UILabel objLabel = GameObject.Find ("input_label").GetComponent<UILabel>();
		string strNum = objLabel.text;

		Regex reg2 = new Regex(@"[0-9]");

		if(reg2.Replace (strNum, "").Length == 0)
		{
			NetManager.RequestSendGuildExp(Convert.ToInt32(strNum));
		}else
		{
			NetManager.RequestSendGuildExp(1000);
		}
		return true;
	}
}

public class Button_ItemIconBtn : CEvent
{
	public override bool _DoEvent()
	{
		int iTid = get("INFO_ITEM_ICON_TID");
		if(iTid/1000 == 2000 || iTid/1000 == 1000)
		{
			//其它物品
			DataCenter.OpenWindow("CONSUMBLES_DETAILS_WINDOW", iTid);
		}else if(iTid/1000 == 30 || iTid/1000 == 40)
		{
			//宠物及碎片
			DataCenter.OpenWindow(UIWindowString.petDetail, iTid);

		}else if(iTid/1000 == 14 || iTid/1000 == 16 || iTid/1000 == 15 || iTid/1000 == 17)
		{
			// 装备及碎片、 法器及碎片
			DataCenter.OpenWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW", iTid);
			DataCenter.SetData("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW","RELATE_TO_GET_PATH", iTid);
		}
		return true;
	}
}
