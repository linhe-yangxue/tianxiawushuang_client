using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class BecomeStrongWindow : tWindow
{
	int MaxBecomeStrongActive = 10;

	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_close_become_strong_window_button", new DefineFactory<Button_close_become_strong_window_button>());
		EventCenter.Self.RegisterEvent ("Button_bg(Clone)_0", new DefineFactory<Button_stronger_action_icon_1>());
		EventCenter.Self.RegisterEvent ("Button_bg(Clone)_1", new DefineFactory<Button_stronger_action_icon_6>());
//		EventCenter.Self.RegisterEvent ("Button_stronger_action_icon_2", new DefineFactory<Button_update_equip_button>());
		EventCenter.Self.RegisterEvent ("Button_bg(Clone)_2", new DefineFactory<Button_stronger_action_icon_7>());
		EventCenter.Self.RegisterEvent ("Button_bg(Clone)_3", new DefineFactory<Button_stronger_action_icon_3>());
		EventCenter.Self.RegisterEvent ("Button_bg(Clone)_4", new DefineFactory<Button_stronger_action_icon_4>());
		EventCenter.Self.RegisterEvent ("Button_bg(Clone)_5", new DefineFactory<Button_stronger_action_icon_5>());
		EventCenter.Self.RegisterEvent ("Button_bg(Clone)_6", new DefineFactory<Button_stronger_action_icon_2>());
		EventCenter.Self.RegisterEvent ("Button_bg(Clone)_7", new DefineFactory<Button_stronger_action_icon_8>());
		EventCenter.Self.RegisterEvent ("Button_bg(Clone)_8", new DefineFactory<Button_stronger_action_icon_8>());
		EventCenter.Self.RegisterEvent ("Button_bg(Clone)_9", new DefineFactory<Button_stronger_action_icon_8>());

		
		
	}

	public override void OnOpen()
	{
		BecomeStrongActive();
	}


	void BecomeStrongActive()
	{
		UIGridContainer mStrongerActive = GameCommon.FindObject (mGameObjUI, "stronger_action_grid").GetComponent<UIGridContainer>();
		mStrongerActive.MaxCount = MaxBecomeStrongActive;
		
		int iIndex = 1001 ;
		
		for(int i = 0; i < mStrongerActive.MaxCount ; i++)
		{	
			int mStrongerOpenCondition = TableCommon.GetNumberBeStronger (i + iIndex, "LEVEL");

			string strAtlasName = TableCommon.GetStringFromBeStronger (i + iIndex, "ICONATLAS");
			string strSpriteName = TableCommon.GetStringFromBeStronger (i + iIndex, "PICTURE");
			GameCommon.SetUISprite (mStrongerActive.controlList[i], "stronger_action_icon", strAtlasName, strSpriteName);

			GameCommon.SetUIText (mStrongerActive.controlList[i], "stronger_open_condition", mStrongerOpenCondition.ToString ());
			GameCommon.SetUIVisiable (mStrongerActive.controlList[i], "stronger_action_icon_ensure", false );
			GameCommon.SetUIVisiable (mStrongerActive.controlList[i], "stronger_action_black_bg", true );
			GameCommon.SetUIVisiable (mStrongerActive.controlList[i], "stronger_action_name", true );
			GameCommon.SetUIVisiable (mStrongerActive.controlList[i], "stronger_open_condition", true );

			if( mStrongerOpenCondition <= RoleLogicData.Self.chaLevel )
			{
				GameCommon.SetUIVisiable (mStrongerActive.controlList[i], "stronger_action_black_bg", false );
				GameCommon.SetUIVisiable (mStrongerActive.controlList[i], "stronger_open_condition", false );
				GameCommon.SetUIVisiable (mStrongerActive.controlList[i], "stronger_action_name", false );
				GameCommon.SetUIVisiable (mStrongerActive.controlList[i], "stronger_action_icon_ensure", true );
			}

		}
	}

}

public class Button_BecomeStrongButton : CEvent 
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("BECOME_STRONG_WINDOW");
		return true;
	}
}

public class Button_close_become_strong_window_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("BECOME_STRONG_WINDOW");
		return true;
	}
}

//提升等级——与主界面“降妖”按钮功能一致，打开关卡地图
public class Button_stronger_action_icon_1 : CEvent 
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
		DataCenter.CloseWindow ("BECOME_STRONG_WINDOW");
		return true;
	}
}

//强化法宝——打开法宝养成界面
public class Button_stronger_action_icon_2 : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
		MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "ROLE_EQUIP_CULTIVATE_WINDOW";
		DataCenter.CloseWindow ("BECOME_STRONG_WINDOW");
		return true;
	}
}

//符灵升级——打开到符灵升级界面
public class Button_stronger_action_icon_3 : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
		DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetUpgrade);
		DataCenter.CloseWindow ("BECOME_STRONG_WINDOW");
		return true;
	}
}

//强化符灵——打开到符灵强化/进化界面
public class Button_stronger_action_icon_4 : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
		DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution);
		DataCenter.CloseWindow ("BECOME_STRONG_WINDOW");
		return true;
	}
}

//符人探险——打开到寻宝界面
public class Button_stronger_action_icon_5 : CEvent
{
	public override bool _DoEvent()
	{
		tEvent evt = Net.StartEvent("CS_RequestIdleBottingStatus");
		evt.set ("WINDOW_NAME", "ON_HOOK_WINDOW");
		evt.DoEvent();
		DataCenter.CloseWindow ("BECOME_STRONG_WINDOW");
		return true;
	}
}

//召唤符灵——打开到商店第一页符灵界面
public class Button_stronger_action_icon_6 : CEvent 
{
	public override bool _DoEvent()
	{
		int i = Convert.ToInt32 (SHOP_PAGE_TYPE.PET);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
		
		if(DataCenter.GetData ("SHOP_WINDOW") != null)
            //by chenliang
            //begin

//			DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.DIAMOND);
//------------------
            DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.PET);

            //end
		else
		{
			GlobalModule.ClearAllWindow();
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
		}
		return true;
	}
}

//抽取法宝——打开到商店第二页道具界面
public class Button_stronger_action_icon_7 : CEvent 
{
	public override bool _DoEvent()
	{
		int i = Convert.ToInt32 (SHOP_PAGE_TYPE.TOOL);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
		
		if(DataCenter.GetData ("SHOP_WINDOW") != null)
            //by chenliang
            //begin

//			DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.DIAMOND);
//----------------------
            DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.TOOL);

            //end
		else
		{
			GlobalModule.ClearAllWindow();
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
		}
		return true;
	}
}

//金钱活动、经验活动、挑战密境——均打开到“降妖→活动列表”中
public class Button_stronger_action_icon_8 : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
		MainUIScript.Self.mStrWorldMapSubWindowName = "ACTIVE_STAGE_WINDOW";
//		MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.WorldMapWindow);
		DataCenter.CloseWindow ("BECOME_STRONG_WINDOW");
		return true;
	}
}





