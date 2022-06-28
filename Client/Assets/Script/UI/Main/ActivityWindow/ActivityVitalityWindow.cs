using UnityEngine;
using System.Collections;
using DataTable;
using System;
using Logic;
using System.Collections.Generic;

public class ActivityVitalityWindow : tWindow 
{
	bool mbTime1;
	bool mbTime2;

	public override void Init ()
	{
		EventCenter.Register ("Button_activity_vitality_get_button", new DefineFactory<Button_activity_vitality_get_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		NetManager.RequstVitalityQuery();
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);

		switch(keyIndex)
		{
		case "GET_VITALITY_MESSAGE":
			RefreshTakeBreak((string)objVal);
			break;
		case "SNED_MESSAGER":
			NetManager.RequstVitalityRewards((int)objVal);
			break;
		case "GET_VITALITY_REWARDS":
			GetVitalityRewards((int)objVal);
            RefreshNewMark();
            break;
        case "SET_BUTTON_GRAY":
        {
            GameObject activityVitalityButtonObj = GameCommon.FindObject(mGameObjUI, "activity_vitality_get_button").gameObject;
            UIImageButton button = activityVitalityButtonObj.GetComponent<UIImageButton>();
            if (button != null)
                button.isEnabled = false;
        }
        break;
		}

	}

    private void RefreshNewMark() 
    {
        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_VITALITY);
    }
	void GetVitalityRewards(int iIndex)
	{
		ItemDataBase itemData= new ItemDataBase();

		itemData.itemNum = DataCenter.mEnergyEvent.GetRecord (iIndex)["NUMBER"];
		itemData.tid = DataCenter.mEnergyEvent.GetRecord (iIndex)["ITEM"];
		itemData.itemId = -1;
		PackageManager.UpdateItem (itemData);

		DataCenter.OpenWindow ("AWARDS_TIPS_WINDOW", itemData);
//		ItemDataBase[] itemDatas = new ItemDataBase[1];
//		itemDatas[0] = itemData;
//		DataCenter.OpenWindow ("GET_REWARDS_WINDOW", new ItemDataProvider(itemDatas));

		SetButtonGrey(GameCommon.FindObject(mGameObjUI, "activity_vitality_get_button").gameObject, true);
        //added by xuke
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_VITALITY, false);
        //end
	}

	void RefreshTakeBreak(string text)
	{
		SC_PowerQuery item = JCode.Decode<SC_PowerQuery>(text);
		if( null == item )
			return ;

		mbTime1 = item.isPower_1;
		mbTime2 = item.isPower_2;

		GameObject activityVitalityButtonObj = GameCommon.FindObject(mGameObjUI, "activity_vitality_get_button").gameObject;
		
		DateTime nowTime = GameCommon.NowDateTime ();
		int secTemp = nowTime.Hour * 3600 + nowTime.Minute * 60 + nowTime.Second;

        int iTemp = 1;
		foreach(var r in DataCenter.mEnergyEvent.GetAllRecord ())
		{
			DataRecord record = r.Value;
			int startTime = record["START_TIME"] * 3600;
			int endTime = record["END_TIME"] * 3600;

            if ((iTemp == 1 && secTemp >= startTime && secTemp < endTime && !mbTime1) ||
                (iTemp == 2 && secTemp >= startTime && secTemp < endTime && !mbTime2))
            {
                GameCommon.GetButtonData(activityVitalityButtonObj).set("WHICH_COUNT", iTemp);
                SetButtonGrey(activityVitalityButtonObj, false);
                break;
            }
            else
            {
                GameCommon.GetButtonData(activityVitalityButtonObj).set("WHICH_COUNT", iTemp);
                SetButtonGrey(activityVitalityButtonObj, true);
            }
            iTemp++;
		}
	}
	
	void SetButtonGrey(GameObject obj , bool bIsGrey)
	{
		UIImageButton button = obj.GetComponent<UIImageButton>();
		if(button != null)
			button.isEnabled = !bIsGrey;
	}
}

class Button_activity_vitality_get_button : CEvent
{
	public override bool _DoEvent ()
	{
        

		int strWhichCount = get("WHICH_COUNT");

        //--判断时间一下 
        DateTime nowTime = GameCommon.NowDateTime();
        int secTemp = nowTime.Hour * 3600 + nowTime.Minute * 60 + nowTime.Second;
        int startTime = DataCenter.mEnergyEvent.GetRecord(strWhichCount)["START_TIME"] * 3600;
        int endTime = DataCenter.mEnergyEvent.GetRecord(strWhichCount)["END_TIME"] * 3600;
        if (!(secTemp >= startTime && secTemp < endTime))
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_CAN_NOT_REWARD_POWER);
            DataCenter.SetData("ACTIVITY_VITALITY_WINDOW", "SET_BUTTON_GRAY", strWhichCount);
            return true;
        }

		DataCenter.SetData ("ACTIVITY_VITALITY_WINDOW", "SNED_MESSAGER", strWhichCount);
		return true;
	}
}