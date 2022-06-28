using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable ;
using System.Collections.Generic;

public class ActiveConsumeAwardWindow : ActiveTotalWindow
{
	int mExtractCount = 0;

	public override void Init ()
	{
		EventCenter.Register ("Button_active_cousume_lucky_guy_recharge_button",new DefineFactory<Button_ShopBtn>());
		EventCenter.Register ("Button_cousume_lucky_guy_one_button",new DefineFactory<Button_cousume_lucky_guy_one_button>());
		EventCenter.Register ("Button_cousume_lucky_guy_ten_button",new DefineFactory<Button_cousume_lucky_guy_ten_button>());

		Net.gNetEventCenter.RegisterEvent ("CS_GetConsumptionExchangeInfo", new DefineFactoryLog<CS_GetConsumptionExchangeInfo>());
		Net.gNetEventCenter.RegisterEvent ("CS_ConsumptionExchange", new DefineFactoryLog<CS_ConsumptionExchange>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		mDesLabelName = "cousume_lucky_guy_describe_label";
		mCountdownLabelName = "active_cousume_lucky_guy_rest_time";
	}

	public override void OnOpen()
	{
		base.OnOpen();
		tEvent evt = Net.StartEvent ("CS_GetConsumptionExchangeInfo");
		evt.set ("ACTIVITY_INDEX", mConfigIndex );
		evt.DoEvent ();
	}

	public override void onChange (string keyIndex, object objVal)
	{
		switch(keyIndex)
		{
		case "SEND_MESSAGE":
			SendMessage((int)objVal);
			break;
		case "SEND_MESSAGE_RESULT":
			SendMessageResult(objVal);
			break;
		}
		base.onChange (keyIndex, objVal);
	}

	public override bool Refresh (object param)
	{
		tEvent respEvt = param as tEvent;
		
		DataRecord record = DataCenter.mOperateEventConfig.GetRecord (mConfigIndex);

		int totalRechargeDiamond = respEvt.get ("CONSUMPTION_AMT");
		mExtractCount = respEvt.get ("REMAIN_TIMES");
		SetText ("cousume_number_label", totalRechargeDiamond.ToString ());
		
		int groudID = record["GROUP_ID"];
		List<DataRecord> records = TableCommon.FindAllRecords (DataCenter.mGroupIDConfig, lhs => {return lhs["GROUP_ID"] == groudID;});
		for(int i = 0; i < Math.Min (records.Count, 10); i++)
		{
			int itemType = records[i]["ITEM_TYPE"];
			int itemID = records[i]["ITEM_ID"];
			int itemCount = records[i]["ITEM_COUNT"];
			GameObject obj = GetSub ("award_icon_" + (i + 1).ToString ());
			GameCommon.SetItemIcon (obj, new ItemData{mID = itemID, mType =itemType, mNumber = itemCount});
		}
		
		RefreshExtractTime ();

		return true;
	}

	void RefreshExtractTime()
	{
		SetVisible ("go_cousume_infos", mExtractCount == 0);
		SetVisible ("cousume_can_draw_infos", mExtractCount != 0);
		if(mExtractCount != 0) SetText ("cousume_lucky_guy_num", mExtractCount.ToString ()+"次");
		
		SetButtonGrey (GetSub ("cousume_lucky_guy_ten_button"), mExtractCount < 10);
		SetButtonGrey (GetSub ("cousume_lucky_guy_one_button"), mExtractCount < 1);
	}

	void SendMessage(int count)
	{
		tEvent evt = Net.StartEvent("CS_ConsumptionExchange");
		evt.set ("ACTIVITY_INDEX", mConfigIndex);
		evt.set ("DRAW_TIMES", count);
		evt.DoEvent();
	}
	
	void SendMessageResult(object obj)
	{
		tEvent respEvt = obj as tEvent;
		object resultData;
		if (!respEvt.getData("RESULT_TABLE", out resultData)) return;
		NiceTable itemData = resultData as NiceTable;
		
		int pos = 0;
		if(itemData.GetRecordCount() == 1)
		{
			foreach(var v in itemData.GetAllRecord ())
			{
				pos = GetItemPos (v.Value["ITEM_TYPE"], v.Value["ITEM_ID"]);
			}
		}
		else pos = UnityEngine.Random.Range (0, 10);
		
		RotatePointer (new Vector3(0, 0, pos * 36),  respEvt);
	}

	//0~9
	int GetItemPos(int itemType, int itemID)
	{
		int groupID = DataCenter.mOperateEventConfig.GetData (mConfigIndex, "GROUP_ID");
		List<DataRecord> records = TableCommon.FindAllRecords (DataCenter.mGroupIDConfig, lhs => {return lhs["GROUP_ID"] == groupID;});
		for(int i = 0; i < Math.Min (records.Count, 10); i++)
		{
			int type = records[i]["ITEM_TYPE"];
			int ID = records[i]["ITEM_ID"];
			
			if(itemType == type && itemID == ID) return i; 
		}
		
		return 0;
	}
	void RotatePointer(Vector3 targetVector, object param)
	{
		SetButtonGrey (GetSub ("cousume_lucky_guy_one_button"), true);
		SetButtonGrey (GetSub ("cousume_lucky_guy_ten_button"), true);
		GameObject pointerObj = GetSub ("dial_parent");
		
		RotateGameObject evt = EventCenter.Start ("RotateGameObject") as RotateGameObject;
		evt.mRotateObj = pointerObj;
		evt.mStartVector = new Vector3(0, 0, pointerObj.transform.localEulerAngles.z);
		evt.mfInitSpeedRate = 0.01f;
		evt.mfAddSpeedRate = 0.002f;
		evt.mfReduceSpeedRate = evt.mfAddSpeedRate;
		evt.mEneVector = targetVector;
		evt.mCallBack = () =>RotateIsOver (param);
		evt.StartUpdate ();
	}

	void RotateIsOver(object param)
	{
		tEvent respEvt = param as tEvent;
		object resultData;
		if (!respEvt.getData("RESULT_TABLE", out resultData)) return;
		NiceTable itemData = resultData as NiceTable;
		
		mExtractCount -= itemData.GetRecordCount ();
		if(mExtractCount < 0) mExtractCount = 0;
		RefreshExtractTime ();
		
		List<ItemData> datas = new List<ItemData>();
		foreach(var v in itemData.Records())
		{
			datas.Add (new ItemData{mID = v["ITEM_ID"], mNumber = v["ITEM_COUNT"], mType = v["ITEM_TYPE"]});
		}
		DataCenter.OpenWindow ("GET_REWARDS_WINDOW", new ItemDataProvider(datas));
	}
	
}

class Button_cousume_lucky_guy_one_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData (ACTIVE_TYPE.ACTIVE_CONSUME_AWARD.ToString () + "_WINDOW", "SEND_MESSAGE", 1);
		return true;
	}
}

class Button_cousume_lucky_guy_ten_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData (ACTIVE_TYPE.ACTIVE_CONSUME_AWARD.ToString () + "_WINDOW", "SEND_MESSAGE", 10);
		return true;
	}
}

class CS_ConsumptionExchange : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			DataCenter.SetData (ACTIVE_TYPE.ACTIVE_CONSUME_AWARD.ToString () + "_WINDOW", "SEND_MESSAGE_RESULT", respEvt);
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}

class CS_GetConsumptionExchangeInfo : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			int configIndex = (int)get ("ACTIVITY_INDEX");
			ACTIVE_TYPE type = (ACTIVE_TYPE)((int)DataCenter.mOperateEventConfig.GetData (configIndex, "EVENT_TYPE"));
			DataCenter.SetData (type.ToString () + "_WINDOW", "REFRESH", respEvt);
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}
