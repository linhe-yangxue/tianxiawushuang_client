using UnityEngine;
using System.Collections;
using System;
using DataTable;
using Logic;

public class ActiveUpgradeLevelWindow : tWindow
{
	NiceTable mUpgradeLevelListData;
	int mCurConfigIndex;

	public override void Init ()
	{
		EventCenter.Register ("Button_upgrade_levle_get_award_button", new DefineFactory<Button_upgrade_levle_get_award_button>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);
		mCurConfigIndex = (int)param;
		
		tEvent evt = Net.StartEvent("CS_RequestHDData");
		evt.set ("CONFIG_INDEX", mCurConfigIndex);
		evt.DoEvent();
	}

	public override bool Refresh (object param)
	{
		tEvent evt = param as tEvent;
		object UpgradeLevelListData;
		bool b = evt.getData ("HD_DATA", out UpgradeLevelListData);
		mUpgradeLevelListData = UpgradeLevelListData as NiceTable;

		RefreshUpgradeLevel();
		return true;
	}

	public override void onChange (string keyIndex, object objVal)
	{
		switch(keyIndex)
		{
		case "SNED_MESSAGER":
			SendMessage((int)objVal);
			break;
		case "SEND_MESSAGE_RESULT":
			SendMessageResult(objVal);
			break;
		}
		base.onChange (keyIndex, objVal);
	}

	void SendMessage(int UpgradeLevelIndex)
	{
		tEvent evt = Net.StartEvent("CS_UpdateHDData");
		evt.set ("TIME_1", false);
		evt.set ("TIME_2", false);
		evt.set ("CASH_EXTRA", false);
		evt.set ("HD_ID", UpgradeLevelIndex);
		evt.set("CONFIG_INDEX", mCurConfigIndex);
		evt.DoEvent();
	}
	

	void SendMessageResult(object obj)
	{
		tEvent evt = obj as tEvent;

		int iUpgradeLevelIndex = evt.get ("HD_ID");

		foreach(var r in mUpgradeLevelListData.GetAllRecord ())
		{
			if(r.Value.getData ("HD_ID") == iUpgradeLevelIndex)
			{
				r.Value.set ("HD_STATUS", 1);
				break;
			}
		}
		RefreshUpgradeLevel ();
		
		int iType = DataCenter.mActiveLevelListConfig.GetRecord (iUpgradeLevelIndex)["TYPE"];
		int iNum = DataCenter.mActiveLevelListConfig.GetRecord (iUpgradeLevelIndex)["NUMBER"];
		GameCommon.RoleChangeNumericalAboutRole (iType, iNum);
	}

	void RefreshUpgradeLevel()
	{
		int count;
		GetUpgradeLevelNum(out  count);
		UIGridContainer grid = GetComponent<UIGridContainer>("grid");
		if(grid != null)
		{
			grid.MaxCount = count;
			for(int i = 0; i < count; i++)
			{
				GameObject obj = grid.controlList[i];
				bool bIsContinue = false;
				foreach(var r in mUpgradeLevelListData.GetAllRecord ())
				{
					if(r.Value.getData ("HD_ID") == i + 1001 && r.Value.getData ("HD_STATUS") == 1)
					{
						obj.SetActive (false);
						bIsContinue = true;
					}
				}
				
				if(bIsContinue) continue;
				
				DataRecord record = DataCenter.mActiveLevelListConfig.GetRecord (i + 1001);
				if(record != null)
				{
					int iTargetLevel = record["LEVEL"];
					string strName = record["NAME"];
					string strDescribe = record["DESCRIPTION"];
					int iType = record["TYPE"];
					int iItemID = record["GOODS"];
					int iItemNum = record["NUMBER"];
					
					int iRoleLevel = RoleLogicData.Self.character.level;
					bool bIsComplete = iRoleLevel >= iTargetLevel ? true : false;
					if(bIsComplete) GameCommon.GetButtonData (GameCommon.FindObject (obj, "upgrade_levle_get_award_button")).set ("INDEX", i+1001);
					GameCommon.SetUIVisiable (obj, "upgrade_levle_get_award_button", bIsComplete);
					GameCommon.SetUIVisiable (obj, "no_complete_icon", !bIsComplete);
					
					string strColor = bIsComplete ? "[00FF00]" : "[FF0000]";
					strDescribe = string.Format (strDescribe, iTargetLevel, iRoleLevel);
					strDescribe = strDescribe.Insert (strDescribe.IndexOf ("("), strColor);
					GameCommon.SetUIText (obj, "upgrade_levle_award_label", "X" + iItemNum.ToString ());
					GameCommon.SetUIText (obj, "upgrade_levle_name_label", strName);
					GameCommon.SetUIText (obj, "upgrade_levle_describe_label", strDescribe);
					
					UISprite icon = GameCommon.FindObject (obj, "upgrade_levle_icon").GetComponent<UISprite>();
					UISprite smallIcon = GameCommon.FindObject (obj, "upgrade_levle_award_icon").GetComponent<UISprite>();
					GameCommon.SetItemIcon (icon, iType, iItemID);
					GameCommon.SetItemIcon (smallIcon, iType, iItemID);
					
					GameCommon.SetUIVisiable (obj, "star_icon", false);
					if(iType == (int)ITEM_TYPE.PET)
					{
						GameCommon.SetStarLevelLabel (obj, TableCommon.GetNumberFromActiveCongfig (iItemID, "STAR_LEVEL"));
					}
					else if(iType == (int)ITEM_TYPE.EQUIP)
					{
						GameCommon.SetStarLevelLabel (obj, TableCommon.GetNumberFromRoleEquipConfig (iItemID, "STAR_LEVEL"));
					}
				}
				else DEBUG.Log ("DataRecord can not find  in HDLevelinglist.csv >>>>" + (i + 1001).ToString ());
			}
		}
		else DEBUG.Log ("can not find UIGridContainer in ActiveUpgradeLevelWindow");
		
		grid.Reposition ();
	}

	void GetUpgradeLevelNum(out int count)
	{
		count = 0;
		foreach(var r in DataCenter.mActiveLevelListConfig.GetAllRecord () )
		{
			if(r.Key >= 1000) count++;
		}
	}
}


class Button_upgrade_levle_get_award_button : CEvent
{
	public override bool _DoEvent ()
	{
		int iIndex = (int)getObject ("INDEX");
		DataCenter.SetData ("ACTIVE_UPGRADE_LEVEL_WINDOW", "SNED_MESSAGER", iIndex);
		return true;
	}
}


