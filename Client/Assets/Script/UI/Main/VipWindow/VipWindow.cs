using UnityEngine;
using System.Collections;
using System;
using Logic;
using System.Collections.Generic;
using DataTable;

public class VipWindow : tWindow
{
	int mShowVipLevel = 0;
	int mMinVipLevel = 1;
	int mMaxVipLevel = 1;
	int mAllVipExp = 0;
	public override void Init ()
	{
		base.Init ();
		EventCenter.Self.RegisterEvent ("Button_close_vip_window_button", new DefineFactory<Button_close_vip_window_button>());
		EventCenter.Self.RegisterEvent ("Button_vip_forward_button", new DefineFactory<Button_vip_forward_button>());
		EventCenter.Self.RegisterEvent ("Button_vip_back_button", new DefineFactory<Button_vip_back_button>());
		EventCenter.Self.RegisterEvent ("Button_vip_recharge_button", new DefineFactory<Button_vip_recharge_button>());
		EventCenter.Self.RegisterEvent ("Button_vip_player_icon", new DefineFactory<Button_vip_level_icon_choose>());
		Net.gNetEventCenter.RegisterEvent("CS_ChangeRoleIconIndex", new DefineFactory<CS_ChangeRoleIconIndex>());
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
		case"FORWARD_AND_BACK":
			mShowVipLevel += (int)objVal;
			if(mShowVipLevel < mMinVipLevel) mShowVipLevel = mMinVipLevel;
			if(mShowVipLevel > mMaxVipLevel) mShowVipLevel = mMaxVipLevel;
			Refresh (null);
			break;
		case"CHANGE_ICON":
			int iIndex = (int)objVal;
			UISprite s = GetSub ("now_player_icon").GetComponent<UISprite>();
			GameCommon.SetPalyerIcon (s, iIndex);
			break;
		}
	}

	public override void Open (object param)
	{
		base.Open (param);
		GetMaxVipLevel();
		mAllVipExp = 0;
		mShowVipLevel = RoleLogicData.Self.vipLevel + 1;
		if(mShowVipLevel > mMaxVipLevel ) mShowVipLevel = mMaxVipLevel;
		mMinVipLevel = 1;
		GetAllVipLevelExp ();
		VipLeftInfo ();
		SetVipIcon ();
		VipPresentProgress ();
		Refresh (param);
	}

	public override void Close ()
	{
		int iIndex = RoleLogicData.Self.iconIndex;
		if(iIndex < 1000) iIndex = 1000;
		UIGridContainer grid = GetSub ("vip_icon_grid").GetComponent<UIGridContainer>();
		GameObject obj = grid.controlList[iIndex - 1000];
		GameCommon.ToggleFalse (GameCommon.FindObject (obj, "vip_player_icon"));

//		tEvent evt = Net.StartEvent ("CS_ChangeRoleIconIndex");
//		evt.set ("ROLE_ICON_INDEX", iIndex);
//		evt.DoEvent ();

		base.Close ();
	}
	
	public override bool Refresh (object param)
	{
		base.Refresh (param);

		GameCommon.SetUIVisiable (mGameObjUI, "vip_forward_button", true);
		GameCommon.SetUIVisiable (mGameObjUI, "vip_back_button", true);
		if(mShowVipLevel == mMinVipLevel) GameCommon.SetUIVisiable (mGameObjUI, "vip_back_button", false);
		if(mShowVipLevel == mMaxVipLevel) GameCommon.SetUIVisiable (mGameObjUI, "vip_forward_button", false);

		int iIndex = 1000 + mShowVipLevel;
		int n = DataCenter.mVipListConfig.GetAllRecord ().Count;
		string strVipLevelDescription = TableCommon.GetStringFromVipList (iIndex, "VIPDESC");
		string nowVipLevel = TableCommon.GetStringFromVipList (iIndex, "name");

		int iInsetID = strVipLevelDescription.IndexOf ('\\');
		strVipLevelDescription = strVipLevelDescription.Insert (iInsetID, "[sub]");

		strVipLevelDescription = strVipLevelDescription.Replace ("\\n", "\n");
		GameCommon.SetUIText (mGameObjUI, "vip_description_label", strVipLevelDescription);
		GameCommon.SetUIText (mGameObjUI, "now_vip_level", nowVipLevel);

		int count ;
		GetIconNum(iIndex, out count);
		UIGridContainer mVipPrivilegeGrid = GameCommon.FindObject (mGameObjUI, "vip_privilege_grid").GetComponent<UIGridContainer>();
		mVipPrivilegeGrid.MaxCount = count;
		
		for(int i = 0; i < count ; i++)
		{
			GameObject obj = mVipPrivilegeGrid.controlList[i];
			UISprite sprite = GameCommon.FindObject (obj, "res").GetComponent<UISprite>();
			DataRecord record = DataCenter.mVipListConfig.GetRecord (iIndex);
			if(record != null)
			{
				int iItemType = record["TYPE" + (i+1).ToString ()];
				int iItemID = record["ITEMID" + (i+1).ToString ()];
				int iItemNum = record["NUMBER" + (i+1).ToString ()];

				GameCommon.SetUIText (obj, "number","X" + iItemNum.ToString ());
				GameCommon.SetItemIcon (sprite, iItemType, iItemID);

				GameCommon.SetIconData (obj, new ItemData {mType = iItemType, mID = iItemID});
				if(iItemType == (int)ITEM_TYPE.EQUIP)
				{
//					GameCommon.SetUIVisiable (obj, true, "star_level", "equipt_bg", "equipt_attribute_bg");
//					GameCommon.SetUIText (obj, "star_level", TableCommon.GetStringFromRoleEquipConfig (iItemID, "STAR_LEVEL"));
					GameCommon.SetUIVisiable (obj, true, "equipt_bg", "equipt_attribute_bg");
					GameCommon.SetStarLevelLabel (obj, TableCommon.GetNumberFromRoleEquipConfig (iItemID, "STAR_LEVEL") );
				}
				else if(iItemType == (int)ITEM_TYPE.PET)
				{
//					GameCommon.SetUIVisiable (obj, "star_level", true);
//					GameCommon.SetUIText (obj, "star_level", TableCommon.GetStringFromActiveCongfig(iItemID, "STAR_LEVEL"));
					GameCommon.SetStarLevelLabel (obj, TableCommon.GetNumberFromActiveCongfig (iItemID, "STAR_LEVEL"));
					GameCommon.SetUIVisiable (obj, "equipt_bg", false);
				}
				else GameCommon.SetUIVisiable (obj, false, "star_level", "equipt_bg");
			}
		}
		return true;
	}
	
	void GetIconNum(int index, out int num)
	{
		num = 0;
		DataRecord record = DataCenter.mVipListConfig.GetRecord (index);
		if(record != null)
		{
			for(int i = 1; i < 7; i++)
			{
				int groupID = record["NUMBER" + i.ToString ()];
				if(groupID != 0) num++;
			}
		}
	}

	void VipPresentProgress ()
	{
		int iIndex = 1000 + mShowVipLevel;
		int n = DataCenter.mVipListConfig.GetAllRecord ().Count;
		string strVipLevelDescription = TableCommon.GetStringFromVipList (iIndex, "VIPDESC");
		string nextVipLevel = TableCommon.GetStringFromVipList (iIndex, "name");

		GameCommon.SetUIText (mGameObjUI, "next_vip_level", nextVipLevel);
		
		RoleLogicData roleLgoicData = RoleLogicData.Self;
		int iNeedCost = TableCommon.GetNumberFromVipList (iIndex, "CASHPAID");
		int vip_max_label=TableCommon.GetNumberFromVipList (iIndex, "CASHPAID_CL");
		int iNeedMoney = 0;
		GameCommon.SetUIText (mGameObjUI, "vip_max_number", iNeedCost.ToString ());
        GameCommon.SetUIText(mGameObjUI, "now_role_vip_num", "当前 VIP " + RoleLogicData.Self.vipLevel.ToString());
		
		int iCurrentVipLevel = roleLgoicData.vipLevel + 1;
		int iCurrentVipExp = 0;
		
		if(mShowVipLevel < iCurrentVipLevel) iCurrentVipExp = iNeedCost;
		if(mShowVipLevel == iCurrentVipLevel )
		{
			if(roleLgoicData.mVIPExp >= iNeedCost) 
			{
				roleLgoicData.mVIPExp -= iNeedCost;
				roleLgoicData.vipLevel++;
				iCurrentVipExp = iNeedCost;
			}
			else iCurrentVipExp = roleLgoicData.mVIPExp;
		}

		UISlider vipProgressBar = GameCommon.FindObject (mGameObjUI, "vip_progress_bar").GetComponent<UISlider>();
		vipProgressBar.value = iCurrentVipExp/(float)iNeedCost;
		GameCommon.SetUIText (mGameObjUI, "vip_min_number", iCurrentVipExp.ToString ());
		iNeedMoney = iNeedCost  - iCurrentVipExp  ;

		GameCommon.SetUIText (mGameObjUI, "tips_label_info","[96795e]再充值 "+"[ffcc00]"+iNeedMoney.ToString ()+" [96795e]元宝可成为 "+nextVipLevel);
		if(iCurrentVipExp <= 0)
		{
			iNeedMoney = vip_max_label  - mAllVipExp ;
			GameCommon.SetUIText (mGameObjUI, "tips_label_info","[96795e]再充值 "+"[ffcc00]"+iNeedMoney.ToString ()+" [96795e]元宝可成为 "+nextVipLevel);
		}else 
		{
			iNeedMoney = iNeedCost  - iCurrentVipExp  ;
			GameCommon.SetUIText (mGameObjUI, "tips_label_info","[96795e]再充值 "+"[ffcc00]"+iNeedMoney.ToString ()+" [96795e]元宝可成为 "+nextVipLevel);
		}
		
		if(vipProgressBar.value == 0) GameCommon.SetUIVisiable (vipProgressBar.gameObject, "Foreground", false);
		else GameCommon.SetUIVisiable (vipProgressBar.gameObject, "Foreground", true);
		
		GetSub ("scroll_view").GetComponent<UIScrollView>().ResetPosition ();
	}

	void GetMaxVipLevel()
	{
		for(int i = 0; i < DataCenter.mVipListConfig.GetRecordCount (); i++)
		{
			if(TableCommon.GetStringFromVipList (i + 1000, "name") == "")
			{
				mMaxVipLevel = i - 1;
				return;
			}
		}
	}

	void GetAllVipLevelExp()
	{
		foreach(KeyValuePair<int, DataRecord> d in DataCenter.mVipListConfig.GetAllRecord ())
		{
			if(d.Key == 0) continue;
			string strVipLevel = d.Value["name"];
			string s = strVipLevel.Substring (3,(strVipLevel.Length - 3));
			int iViplevel = Convert.ToInt32(s);
			if(iViplevel < RoleLogicData.Self.vipLevel + 1)
			{
				mAllVipExp += d.Value["CASHPAID"];
			}
			else if(iViplevel == RoleLogicData.Self.vipLevel + 1)
			{
				mAllVipExp += RoleLogicData.Self.mVIPExp;
			}
		}
	}

	void SetVipIcon()
	{
		int iIndex = RoleLogicData.Self.iconIndex;
		UISprite sprite = GetSub ("now_player_icon").GetComponent<UISprite>();
		GameCommon.SetPalyerIcon (sprite, iIndex);
		
		UIGridContainer grid = GetSub ("vip_icon_grid").GetComponent<UIGridContainer>();
		GameObject obj = grid.controlList[iIndex - 1000];
		GameCommon.ToggleTrue (GameCommon.FindObject (obj, "vip_player_icon"));
	}

	// left  vip icon  info
	void VipLeftInfo()
	{
		UIGridContainer mVipGridLeft = GameCommon.FindObject (mGameObjUI, "vip_icon_grid").GetComponent<UIGridContainer>();
		mVipGridLeft.MaxCount = mMaxVipLevel+1;
		
		for(int i = 0; i < mVipGridLeft.MaxCount ; i++)
		{	
			string count = TableCommon.GetStringFromVipList (i+1000, "name");
			GameCommon.SetPalyerIcon (GameCommon.FindObject (mVipGridLeft.controlList[i], "vip_player_icon").GetComponent<UISprite>(), i+1000);

			GameCommon.SetUIVisiable (mVipGridLeft.controlList[i], "vip_icon_black_bg", true );
			UIToggle toggle = GameCommon.FindObject (mVipGridLeft.controlList[i], "vip_player_icon").GetComponent<UIToggle>();
			toggle.value = false;
			GameCommon.ToggleFalse (GameCommon.FindObject (mVipGridLeft.controlList[i], "vip_player_icon"));
			GameCommon.SetUIVisiable (mVipGridLeft.controlList[i],"vip_icon_bg",true );

			if(i<RoleLogicData.Self.vipLevel+1 )
			{
				GameCommon.SetUIVisiable (mVipGridLeft.controlList[i], "now_icon_insure", true );
				GameCommon.SetUIVisiable (mVipGridLeft.controlList[i], false, "vip_icon_black_bg", "vip_icon_bg");
				NiceData n = GameCommon.GetButtonData (mVipGridLeft.controlList[i], "vip_player_icon");
				n.set ("INDEX", i+1000);
			}

			GameCommon.SetUIText (mVipGridLeft.controlList[i], "vip_level", count);
			GameCommon.SetUIText (mVipGridLeft.controlList[i], "now_vip_number", count);
		}
	}
}


public class Button_close_vip_window_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("VIP_WINDOW");
		return true;
	}
}

public class Button_vip_forward_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("VIP_WINDOW", "FORWARD_AND_BACK", 1);
		return true;
	}
}

public class Button_vip_back_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("VIP_WINDOW", "FORWARD_AND_BACK", -1);
		return true;
	}
}

public class Button_vip_recharge_button : CEvent
{
	public override bool _DoEvent()
	{
		int i = Convert.ToInt32 (SHOP_PAGE_TYPE.DIAMOND);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
		
		GlobalModule.ClearAllWindow();
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
		return true;
	}
}

public class Button_vip_level_icon_choose : CEvent 
{
	public override bool _DoEvent()
	{
		int iIndex = (int)getObject ("INDEX");

		tEvent evt = Net.StartEvent ("CS_ChangeRoleIconIndex");
		evt.set ("ROLE_ICON_INDEX", iIndex);
		evt.DoEvent ();

		return true;
	}
}

public class CS_ChangeRoleIconIndex : tNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int bResult = respEvt.get("RESULT");
		int iIndex = get ("ROLE_ICON_INDEX");
		if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
		{
			RoleLogicData.Self.iconIndex = iIndex;
			DataCenter.SetData ("VIP_WINDOW","CHANGE_ICON", iIndex);
			DataCenter.SetData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_ROLE_NAME", true);
		}
	}
}
		
		
		