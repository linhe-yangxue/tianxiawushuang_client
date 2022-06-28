using UnityEngine;
using System;
using System.Collections;
using DataTable;
using Logic;

public class DailySignWindow : tWindow
{
	public override void Init()
	{
		EventCenter.Self.RegisterEvent ("Button_supplementary_sign_window_close_button", new DefineFactory<Button_supplementary_sign_window_close_button>());
		EventCenter.Self.RegisterEvent ("Button_supplementary_sign_window_ok_button", new DefineFactory<Button_supplementary_sign_window_ok_button>());
		EventCenter.Self.RegisterEvent ("Button_supplementary_sign_button", new DefineFactory<Button_supplementary_sign_button>());
		EventCenter.Self.RegisterEvent ("Button_close_supplementary_sign_window_button", new DefineFactory<Button_close_supplementary_sign_window_button>());
	}

	public override void Open(object param)
	{
		base.Open (param);

		Refresh (param);
	}

	public override bool Refresh(object param)
	{
		SetVisible ("supplementary_sign_window", false);

		UILabel time_limit_label = GameCommon.FindObject (mGameObjUI, "time_limit_label").GetComponent<UILabel>();
		DateTime now = GameCommon.NowDateTime ();
		int iYear = now.Year;
		string strYear = iYear.ToString ();
		string strMonth = now.Month.ToString ();
		string strYearAndMonth = strYear + "/" + strMonth + "/";
		string strLimitDay = "31";
		if(strMonth == "2")
		{
			if(iYear%400 == 0 || (iYear%4 == 0 && iYear%100 != 0)) strLimitDay = "29";
			else strLimitDay = "28";
		}
		if(strMonth == "4" || strMonth == "6" || strMonth == "9" || strMonth == "11") strLimitDay = "30";
		string strStarDate = strYearAndMonth + "1";
		string strEndDate = strYearAndMonth + strLimitDay;
		time_limit_label.text = strStarDate + "-" + strEndDate;

		DailySignLogicData logicData = DataCenter.GetData ("DAILY_SIGN_DATA") as DailySignLogicData;
		int count = logicData.mDailSignDataList.Length;
		UIGridContainer mGridLeft = GameCommon.FindObject (mGameObjUI, "grid_left").GetComponent<UIGridContainer>();
		mGridLeft.MaxCount = 24;

		UIGridContainer mGridRight = GameCommon.FindObject (mGameObjUI, "grid_right").GetComponent<UIGridContainer>();
		mGridRight.MaxCount = 4;

		int iCanReceiveMaxNum = logicData.get ("MAX_NUM");

		for(int i = 1; i < (count + 1); i++)
		{
			DailySignData dailySignData = logicData.mDailSignDataList[i - 1];
			int iDailySignIndex = dailySignData.mID;
			int iDailySignStatus = dailySignData.mStatus;

			int iItemType = TableCommon.GetNumberFromDailySign (iDailySignIndex, "ITEM_TYPE");
			int iItemID = TableCommon.GetNumberFromDailySign (iDailySignIndex, "ITEM_ID");
			string strItemNum = TableCommon.GetStringFromDailySign (iDailySignIndex, "COUNT");

			GameObject subcell;
			if(i%7 == 0) 
			{
				subcell = mGridRight.controlList[i/7 - 1];
				GameCommon.SetIconData (subcell, new ItemData {mType = iItemType, mID = iItemID});
				GameCommon.SetUIVisiable (subcell, "item_description", true);
				DataRecord d = DataCenter.mItemIcon.GetRecord (iItemType);
				string n = d["NAME"];
				if(iItemType != 0) GameCommon.SetUIText (subcell, "item_description", strItemNum + DataCenter.mItemIcon.GetRecord (iItemType)["NAME"]);
				else GameCommon.SetUIText (subcell, "item_description", TableCommon.GetStringFromActiveCongfig (iItemID, "NAME"));
			}
			else 
			{
				subcell = mGridLeft.controlList[i - 1 - i/7];
				GameCommon.SetIconData (subcell, new ItemData {mType = iItemType, mID = iItemID});
				GameCommon.SetUIVisiable (subcell, "item_description", false);
			}

			if(iItemType == (int)ITEM_TYPE.PET)
			{
				int iStarNum = TableCommon.GetNumberFromActiveCongfig (iItemID, "STAR_LEVEL");
				GameCommon.SetStarLevelLabel (subcell, iStarNum );
			}
			else if(iItemType == (int)ITEM_TYPE.EQUIP)
			{
				int iStarNum = TableCommon.GetNumberFromRoleEquipConfig (iItemID, "STAR_LEVEL");
				GameCommon.SetStarLevelLabel (subcell, iStarNum );
			}
			else 
				GameCommon.SetUIVisiable (subcell, "star_label", false);

			GameCommon.SetUIText (subcell, "title_num", i.ToString ());
			GameCommon.SetUIText (subcell, "item_num", "x" + strItemNum);

			string strSpriteName = TableCommon.GetStringFromDailySign (iDailySignIndex, "ITEM_SPRITES_NAME");
			string strAtlasName = TableCommon.GetStringFromDailySign (iDailySignIndex, "ITEM_ATLAS_NAME");
			GameCommon.SetUISprite (subcell, "item_icon", strAtlasName, strSpriteName);

			GameCommon.SetUIVisiable (subcell, "item_get_icon", false);
			GameCommon.SetUIVisiable (subcell, "item_get_chose", false);
			GameCommon.SetUIVisiable (subcell, "item_not_chose", true);
			if(iDailySignStatus == 1) 
			{
				GameCommon.SetUIVisiable (subcell, "item_get_icon", true);
				GameCommon.SetUIVisiable (subcell, "item_get_chose", true);
				GameCommon.SetUIVisiable (subcell, "item_not_chose", false);
				int currentIndex = DataCenter.Get ("FIRST_LANDING");
				if(currentIndex == iDailySignIndex) 
				{
					GameObject itemGetIconObj = GameCommon.FindObject (subcell, "item_get_icon");
					itemGetIconObj.SetActive (true);
					TweenScale[] ts = itemGetIconObj.GetComponents<TweenScale>();
					foreach(TweenScale t in ts)
					{
						t.ResetToBeginning ();
						t.PlayForward ();
					}
				}
			}
		}

		return true;
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);

		switch(keyIndex)
		{
		case "OPEN_AND_CLOSE_SUPPLEMENTARY_SIGN_WINDOW":
			SetVisible ("supplementary_sign_window", Convert.ToBoolean(objVal));
			if(Convert.ToBoolean (objVal))
			{
				NiceData button_data = GameCommon.GetButtonData (mGameObjUI, "supplementary_sign_window_ok_button");
				button_data.set ("INDEX", (int)objVal);

				SetText ("cost_num", TableCommon.GetStringFromDailySign ((int)objVal, "COST"));
			}
			break;
		case "RESULT_SUPPLEMENTARY_SIGN":
			GameCommon.SetUIVisiable (mGameObjUI, "supplementary_sign_window", false);
			GameObject obj = getObject ("PARENT_OBJ") as GameObject;
			GameCommon.SetUIVisiable (obj, "supplementary_sign_button", false);
			GameCommon.SetUIVisiable (obj, "item_get_icon", true);

			DailySignLogicData logicData = DataCenter.GetData ("DAILY_SIGN_DATA") as DailySignLogicData;
			for(int i=0; i<logicData.mDailSignDataList.Length; i++)
			{
				if(logicData.mDailSignDataList[i].mID == (int)objVal)
				{
					logicData.mDailSignDataList[i].mStatus = 1;
					break;
				}
			}
			break;
		}
	}
}


public class Button_close_supplementary_sign_window_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("DAILY_SIGN_WINDOW");
		return true;
	}
}

public class Button_supplementary_sign_button : CEvent
{
	public override bool _DoEvent()
	{
		GameObject obj = getObject ("BUTTON") as GameObject;
		GameObject parentObj = obj.transform.parent.gameObject;
		DataCenter.SetData ("DAILY_SIGN_WINDOW", "PARENT_OBJ", parentObj);

		int iIndex = (int)getObject ("INDEX");
		DataCenter.SetData ("DAILY_SIGN_WINDOW", "OPEN_AND_CLOSE_SUPPLEMENTARY_SIGN_WINDOW", iIndex);
		return true;
	}
}


public class Button_supplementary_sign_window_close_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("DAILY_SIGN_WINDOW", "OPEN_AND_CLOSE_SUPPLEMENTARY_SIGN_WINDOW", false);
		return true;
	}
}

public class Button_supplementary_sign_window_ok_button : CEvent
{
	public override bool _DoEvent()
	{	
		int iIndex = (int)getObject ("INDEX");
		int iCostNum = TableCommon.GetNumberFromDailySign (iIndex, "COST");

		RoleLogicData roleData = RoleLogicData.Self;
		if(roleData.diamond < iCostNum) DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_SHOP_NO_ENOUGH_DIAMOND);
		else
		{
			tEvent evt = Net.StartEvent ("CS_DiamondDailySign");
			evt.set ("COST", -iCostNum);
			evt.set ("DAILY_ID", iIndex);
			evt.DoEvent ();
		}
		return true;
	}
}


