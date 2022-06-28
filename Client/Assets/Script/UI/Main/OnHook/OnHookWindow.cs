using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

public class OnHookWindow : tWindow {

	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_on_hook_close_button", new DefineFactory<ButtonOnHookCloseButton>());
		EventCenter.Self.RegisterEvent("Button_hook_adjust_troops_button", new DefineFactory<ButtonhookAdjustTroopsButton>());
		EventCenter.Self.RegisterEvent("Button_begin_treasure_nunt_button", new DefineFactory<ButtonBeginTreasureNuntButton>());
		EventCenter.Self.RegisterEvent("Button_on_hook_award_button", new DefineFactory<ButtonOnHookAwardButton>());
	}
	
	public override void Close ()
	{
		base.Close ();
		GameObject.Destroy(mGameObjUI);
	}

	public override void OnOpen ()
	{
		base.OnOpen ();

		Refresh(null);
	}
	public override bool Refresh (object param)
	{
		SetLevel();
		SetRemainTime();
		SetRoleNum();
		SetRateNum();
		SetOnHookState();
		return base.Refresh (param);
	}

	public void SetLevel()
	{
		OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
		UILabel label = mGameObjUI.transform.Find("group/level_label/on_hook_level_label").GetComponent<UILabel>();
		label.text = logicData.mOnHook.mLevel.ToString() + "/30";
	}

	public void SetRemainTime()
	{
		OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
		UILabel label = mGameObjUI.transform.Find("group/time_label/on_hook_time_label").GetComponent<UILabel>();
		CountdownUI countdownUI = label.GetComponent<CountdownUI>();
		if(logicData.mOnHook.mState == ON_HOOK_STATE.DOING)
		{
			if(countdownUI != null) 
				MonoBehaviour.Destroy (countdownUI);

			SetCountdownTime(label.name, (System.Int64)(logicData.mOnHook.mRemainingTime + CommonParam.NowServerTime()));
		}
		else
		{
			if(countdownUI != null)
				countdownUI.enabled = false;
			label.text = "00:00:00";
		}

		GameObject speedupBtn = mGameObjUI.transform.Find("group/hook_adjust_troops_button").gameObject;
		speedupBtn.SetActive(logicData.mOnHook.mState == ON_HOOK_STATE.DOING);
	}

	public void SetRoleNum()
	{
		OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
		UILabel label = mGameObjUI.transform.Find("group/num_label/on_hook_num_label").GetComponent<UILabel>();

		float roleNum = (logicData.mOnHook.mRateNum - 100)/5.0f + 1;
		label.text = ((int)roleNum).ToString();
	}

	public void SetRateNum()
	{
		OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
		UILabel label = mGameObjUI.transform.Find("group/num_label/gain_num").GetComponent<UILabel>();

		label.text = logicData.mOnHook.mRateNum.ToString();
	}

	public void SetOnHookState()
	{
		OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
		GameObject startBtn = mGameObjUI.transform.Find("group/on_hook_state_group/begin_treasure_nunt_button").gameObject;
		GameObject awardBtn = mGameObjUI.transform.Find("group/on_hook_state_group/on_hook_award_button").gameObject;
		GameObject doing = mGameObjUI.transform.Find("group/on_hook_state_group/on_hook_doing").gameObject;

		startBtn.SetActive(logicData.mOnHook.mState == ON_HOOK_STATE.UN_START);
		awardBtn.SetActive(logicData.mOnHook.mState == ON_HOOK_STATE.UN_GET_AWARD);
		doing.SetActive(logicData.mOnHook.mState == ON_HOOK_STATE.DOING);
	}
}

public class OnHookTipWindow : tWindow {

	int iCount = 0;
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_on_hook_tip_ok_button", new DefineFactory<ButtonOnHookTipOkButton>());
		EventCenter.Self.RegisterEvent("Button_on_hook_tip_close_button", new DefineFactory<ButtonOnHookTipCloseButton>());
		EventCenter.Self.RegisterEvent("Button_on_hook_tip_change_button", new DefineFactory<ButtonOnHookTipChangeButton>());
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
	}
	public override void Close ()
	{
		base.Close ();
		GameObject.Destroy(mGameObjUI);
	}

	public override void Open (object param)
	{
		OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
		if(logicData.mOnHook.mState == ON_HOOK_STATE.LOCKED && iCount < 2)
		{
			iCount++;
			tEvent evt = Net.StartEvent("CS_RequestIdleBottingStatus");
			evt.set ("WINDOW_NAME", "ON_HOOK_TIP_WINDOW");
			evt.DoEvent();
		}
		else
		{
			iCount = 0;
			base.Open (param);
		}

	}
}


public class OnHookSpeedupWindow : tWindow {
	
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_on_hook_speedup_ok_button", new DefineFactory<ButtonOnHookSpeedupOkButton>());
		EventCenter.Self.RegisterEvent("Button_on_hook_speedup_cancle_button", new DefineFactory<ButtonOnHookSpeedupCancleButton>());
	}
	
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
	}
	public override void Close ()
	{
		base.Close ();
		GameObject.Destroy(mGameObjUI);
	}

	public override void OnOpen ()
	{
		base.OnOpen ();
		
		Refresh(null);
	}

	public override bool Refresh (object param)
	{
		SetRemainSpeedupToday();
		SetSpeedupNeedMoney();
		return base.Refresh (param);
	}

	public void SetRemainSpeedupToday()
	{
		OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
		UILabel label = mGameObjUI.transform.Find("group/remain_speedup_group/remain_speedup_num").GetComponent<UILabel>();
		label.text = logicData.mOnHook.mRemainSpeedupToday.ToString();
	}

	public void SetSpeedupNeedMoney()
	{
		OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
		UILabel label = mGameObjUI.transform.Find("group/ingot_cousume_number").GetComponent<UILabel>();
		label.text = (1 * logicData.mOnHook.mRemainingTime / 180 + 1).ToString();
	}
}


public class OnHookAwardWindow : tWindow {

	float mfPosX = -75;
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_on_hook_award_ok_button", new DefineFactory<ButtonOnHookAwardOkButton>());
	}
	
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
	}
	public override void Close ()
	{
		base.Close ();
		GameObject.Destroy(mGameObjUI);
	}

	public override void OnOpen ()
	{
		base.OnOpen ();
	}
	
	public override bool Refresh (object param)
	{
		tEvent temEvent = param as tEvent;
		object awardTable;
		temEvent.getData("AWARD_TABLE", out awardTable);

		SetGrid(awardTable as NiceTable);
		SetGridPosition();
		return base.Refresh (param);
	}

	public void SetGrid(NiceTable awardTable)
	{
		UIGridContainer grid = mGameObjUI.transform.Find("group/hooking_info/gain_grid").GetComponent<UIGridContainer>();

		int iCount = 0;
		foreach(KeyValuePair<int, DataRecord> r in awardTable.GetAllRecord ())
		{
			iCount++;
		}
		grid.MaxCount = iCount;

		iCount = 0;
		foreach(KeyValuePair<int, DataRecord> r in awardTable.GetAllRecord ())
		{
			SetIcon(grid.controlList[iCount], r.Value);
			iCount++;
		}
	}

	public void SetIcon(GameObject obj, DataRecord record)
	{
		if(obj != null && record != null)
		{
			int type = record.get("ITEM_TYPE");
			int id = record.get("ITEM_ID");
			int iCount = record.get("ITEM_COUNT");

			bool bIsFragment = type == (int)ITEM_TYPE.PET_FRAGMENT ? true : false;
			GameCommon.SetUIVisiable (obj, "fragment_sprite", bIsFragment);
			if(bIsFragment)
			{
				id = TableCommon.GetNumberFromFragment(id, "ITEM_ID");
				int elementIndex = TableCommon.GetNumberFromActiveCongfig(id, "ELEMENT_INDEX");
				GameCommon.SetElementFragmentIcon (obj, "fragment_sprite", elementIndex);
			}

			bool bIsRoleEquip = type == (int)ITEM_TYPE.EQUIP ? true : false;
			GameCommon.SetUIVisiable (obj, "Element", bIsRoleEquip);
			GameCommon.SetUIVisiable (obj, "background_sprite", bIsRoleEquip);

			UISprite icon = GameCommon.FindComponent<UISprite>(obj, "pet_icon");
			GameCommon.SetItemIcon(icon, type, id);

			if (type == (int)ITEM_TYPE.PET || type == (int)ITEM_TYPE.PET_FRAGMENT)
			{
				iCount = 1;
				int starLevel = TableCommon.GetNumberFromActiveCongfig(id, "STAR_LEVEL");
				string strName = TableCommon.GetStringFromActiveCongfig(id, "NAME");
				GameCommon.SetUIVisiable(obj, "star_level_label", true);
				GameCommon.SetUIVisiable(obj, "background", false);
				GameCommon.SetUIText(obj, "star_level_label", starLevel.ToString());
				GameCommon.SetUIText(obj, "gain_name", strName);
				GameCommon.SetUIText(obj, "gain_number", iCount.ToString());
			}
			else if (type == (int)ITEM_TYPE.EQUIP)
			{
				int starLevel = record.get("ITEM_COUNT");
				starLevel = TableCommon.GetNumberFromRoleEquipConfig (id, "STAR_LEVEL");
				iCount = 1;
				string strName = TableCommon.GetStringFromRoleEquipConfig(id, "NAME");
				GameCommon.SetUIVisiable(obj, "star_level_label", true);
				GameCommon.SetUIVisiable(obj, "background", true);
				GameCommon.SetUIText(obj, "star_level_label", starLevel.ToString());
				GameCommon.SetElementBackground(obj, "background", (int)record.get("ITEM_ELEMENT"));
				GameCommon.SetUIText(obj, "gain_name", strName);
				GameCommon.SetUIText(obj, "gain_number", iCount.ToString());

				// set element icon and set equip element background icon
				int iElementIndex = (int)record.get("ITEM_ELEMENT");
				GameCommon.SetEquipElementBgIcons(obj, id, iElementIndex);
			}
			else
			{
				string strName = DataCenter.mItemIcon.GetData(type, "NAME");
				GameCommon.SetUIVisiable(obj, "star_level_label", false);
				GameCommon.SetUIVisiable(obj, "background", false);
				GameCommon.SetUIText(obj, "gain_name", strName);
				GameCommon.SetUIText(obj, "gain_number", iCount.ToString());
			}

			if(type == (int)ITEM_TYPE.GOLD)
			{
				GameCommon.RoleChangeGold(iCount);
			}
			else if(type == (int)ITEM_TYPE.POWER)
			{
				GameCommon.RoleChangeStamina(iCount);
			}
			else if(type == (int)ITEM_TYPE.YUANBAO)
			{
				GameCommon.RoleChangeDiamond(iCount);
			}
			else if(type == (int)ITEM_TYPE.SPIRIT)
			{
				GameCommon.RoleChangeSpirit(iCount);
			}
		}
	}

	public void SetGridPosition()
	{
		UIGridContainer grid = mGameObjUI.transform.Find("group/hooking_info/gain_grid").GetComponent<UIGridContainer>();
		if(grid != null)
		{
			grid.transform.localPosition = new Vector3(mfPosX * (grid.MaxCount - 1), grid.transform.localPosition.y, grid.transform.localPosition.z);
		}
	}
}

//--------------------------------------------------------------------------------------------
public class ButtonOnHookCloseButton : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("ON_HOOK_WINDOW");

		return true;
	}
}

public class ButtonhookAdjustTroopsButton : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.OpenWindow("ON_HOOK_SPEEDUP_WINDOW");
		return true;
	}
}

public class ButtonBeginTreasureNuntButton : CEvent
{
	public override bool _DoEvent ()
	{
		tEvent evt = Net.StartEvent("CS_RequestIdleBottingBegin");
		evt.DoEvent();
		return true;
	}
}

public class ButtonOnHookAwardButton : CEvent
{
	public override bool _DoEvent ()
	{
		tEvent evt = Net.StartEvent("CS_RequestIdleBottingAward");
		evt.DoEvent();
		return true;
	}
}

//--------------------------------------------------------------------------------------------
public class ButtonOnHookTipOkButton : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("ON_HOOK_TIP_WINDOW");
		return true;
	}
}

public class ButtonOnHookTipCloseButton : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("ON_HOOK_TIP_WINDOW");
		return true;
	}
}

public class ButtonOnHookTipChangeButton : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("ON_HOOK_TIP_WINDOW");

		Logic.EventCenter.Start("Button_on_hook_btn").DoEvent();
		return true;
	}
}

//--------------------------------------------------------------------------------------------
public class ButtonOnHookSpeedupOkButton : CEvent
{
	public override bool _DoEvent ()
	{
		OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
		if(logicData.mOnHook.mRemainSpeedupToday > 0)
		{
			tEvent evt = Net.StartEvent("CS_RequestIdleBottingSpeedUp");
			evt.DoEvent();
		}
		else
		{
			DEBUG.LogError("logicData.mOnHook.mRemainSpeedupToday <= 0");
		}
		return true;
	}
}

public class ButtonOnHookSpeedupCancleButton : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("ON_HOOK_SPEEDUP_WINDOW");
		return true;
	}
}

//--------------------------------------------------------------------------------------------
public class ButtonOnHookAwardOkButton : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("ON_HOOK_AWARD_WINDOW");

		Logic.EventCenter.Start("Button_on_hook_btn").DoEvent();
		return true;
	}
}