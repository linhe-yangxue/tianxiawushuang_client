using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable ;
using System.Collections.Generic;

public class ActiveLoginGiveGiftBagWindow : ActiveTotalWindow
{
	List<DataRecord> mRecords = new List<DataRecord>();
	int mMaxGifCount = 3;

	bool mbLog;
	bool mbOnLine;
	int mOnLineTime;
	int onlineTimeLimt =0;

	public override void Init ()
	{
		EventCenter.Register ("Button_log_gift_get_btn", new DefineFactory<Button_log_gift_get_btn>());
		EventCenter.Register ("Button_online_gift_get_btn", new DefineFactory<Button_online_gift_get_btn>());

		Net.gNetEventCenter.RegisterEvent ("CS_GetFreeBagActivityInfo", new DefineFactoryLog<CS_GetFreeBagActivityInfo>());
		Net.gNetEventCenter.RegisterEvent ("CS_ReceiveFreeBagInActivity", new DefineFactoryLog<CS_ReceiveFreeBagInActivity>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		mDesLabelName = "active_log_in_gift_describe_label";
		mCountdownLabelName = "active_log_in_gift_rest_time";

		mRecords = TableCommon.FindAllRecords (DataCenter.mAwardConfig, lhs => {return lhs["EVENT_ID"] == mConfigIndex;});
		DataRecord record = DataCenter.mOperateEventConfig.GetRecord(mConfigIndex );
		onlineTimeLimt = record ["ONLINE_TIME"];
	}

	public override void OnOpen ()
	{
		base.OnOpen ();
		tEvent evt = Net.StartEvent ("CS_GetFreeBagActivityInfo");
		evt.set ("BAG_INDEX",GetCurrentDate() );

		evt.DoEvent ();
	}

	int GetCurrentDate()
	{
		foreach(var v in mRecords)
		{
			string strEndTime = v["EVENT_DATE"];
			string[] strEndTimes = strEndTime.Split ('_');
			if(GameCommon.NowDateTime ().Day == Convert.ToInt32(strEndTimes[2]))
				return v["INDEX"];
		}

		return 0;
	}

	public override void onChange (string keyIndex, object objVal)
	{
		switch(keyIndex)
		{
		case"SHOW_LOG_ITEMS":
				ShowLogItems ((int)objVal);
			break;
		case"SHOW_ONLINE_ITEMS":
				ShowOnlineItems ((int)objVal);
			break;
		case"SEND_MESSAGE_RESULT":
				SendMessageResult ((int)objVal);
			break;
		}
		base.onChange (keyIndex, objVal);
	}
	
	public override bool Refresh (object param)
	{
		base.Refresh (param);
		tEvent evt = param as tEvent;
		mbLog = evt.get("RECEIVE_1");
		mbOnLine = evt.get ("RECEIVE_2");
		mOnLineTime = evt.get ("ONLINE_TIME");

		RefreshLogGift();
		return true;
	}

	void RefreshLogGift()
	{
		UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "grid").GetComponent<UIGridContainer>();
		grid.MaxCount = mRecords.Count;
		for(int i = 0; i <mRecords.Count ; i++)
		{
			GameObject obj = grid.controlList[i];
			DataRecord record = mRecords[i];
			 
			if(record == null) continue;

			int awardGroupID1 = record["AWARD_GROUP_1"];
			int awardGroupID2 = record["AWARD_GROUP_2"];
			int awardConfigIndex = record["INDEX"];
			int dayCount = 0;

			string gift_tips_label = record["EVENT_DATE"];

			if(i==GetCurrentDate()-1 )
			{
				GameCommon.SetUIText (obj,"gift_tips_label", "今日登陆大礼" );
				if(beforeBuy (awardConfigIndex))
				{
					GameCommon.SetUIText (obj,"log_gift_open_time_label","[cc0000]未开启" );
					GameCommon.SetUIText (obj,"online_gift_open_time_label","[cc0000]未开启" );
					GameCommon.SetUIVisiable (obj, "online_gift_open_time_text", false );
				}
				if(IsCanBuy (awardConfigIndex))
				{
					GameCommon.SetUIVisiable (obj, "log_gift_open_time_label", false );
					GameCommon.SetUIVisiable (obj, "log_gift_get_btn", !mbLog);
					GameCommon.SetUIVisiable (obj, "log_gift_get_btn_gray", mbLog);
					GameCommon.GetButtonData (GameCommon.FindObject(obj,"log_gift_get_btn")).set ("LOG_GIF_GIVE",1);

					GameCommon.SetUIVisiable (obj, "online_gift_get_btn", !mbOnLine);
					GameCommon.GetButtonData (GameCommon.FindObject(obj,"online_gift_get_btn")).set ("ONLINE_GIF_GIVE",2);
					GameCommon.SetUIVisiable (obj, "online_gift_get_btn_gray", mbOnLine);
					 if(mOnLineTime<onlineTimeLimt&&!mbOnLine)
					{
						GameCommon.SetUIVisiable (obj, "online_gift_open_time_label", true  );
						SetCountdownTime (obj, "online_gift_open_time_label",(Int64)(CommonParam.NowServerTime() + onlineTimeLimt - mOnLineTime ) ,new CallBack(this, "OnLineTimeIsOver", obj));
						GameCommon.SetUIVisiable (obj, "online_gift_open_time_text", true  );
					}else if(mOnLineTime>=onlineTimeLimt && !mbOnLine)
					{
						GameCommon.SetUIVisiable (obj, "online_gift_open_time_label", false );
					}else if(mbOnLine)
					{
						GameCommon.SetUIVisiable (obj, "online_gift_open_time_label", false );
					}
				}
				if(afterBuy (awardConfigIndex )&& mbLog==true )
				{
					GameCommon.SetUIVisiable (obj, "log_gift_open_time_label", false );
					GameCommon.SetUIVisiable (obj, "log_gift_get_btn", false );
					GameCommon.SetUIVisiable (obj, "log_gift_get_btn_gray", true);
				}
				if(afterBuy (awardConfigIndex )&& mbLog==false )
				{
					GameCommon.SetUIText (obj,"log_gift_open_time_label","[cc0000]领取时间已过" );
				}
				if(afterBuy (awardConfigIndex )&& mbOnLine == true )
				{
					GameCommon.SetUIVisiable (obj, "online_gift_open_time_label", false );
					GameCommon.SetUIVisiable (obj, "online_gift_get_btn", false );
					GameCommon.SetUIVisiable (obj, "online_gift_get_btn_gray", true);
				}
				if(afterBuy (awardConfigIndex )&& mbOnLine == false )
				{
					GameCommon.SetUIText (obj,"online_gift_open_time_label","[cc0000]领取时间已过" );
					GameCommon.SetUIVisiable (obj, "online_gift_open_time_text", false );
				}
			}else if(i==GetCurrentDate())
			{
				GameCommon.SetUIText (obj,"gift_tips_label", "明日登陆大礼" );
				GameCommon.SetUIText (obj,"log_gift_open_time_label","[cc0000]明日开启" );
				GameCommon.SetUIText (obj,"online_gift_open_time_label","[cc0000]明日开启" );
				GameCommon.SetUIVisiable (obj, "online_gift_open_time_text", false );
			}else 
			{
				GameCommon.SetUIText (obj,"gift_tips_label",gift_tips_label + "登陆大礼" );
				GameCommon.SetUIText (obj,"log_gift_open_time_label","[cc0000]" + gift_tips_label + "开启" );
				GameCommon.SetUIText (obj,"online_gift_open_time_label","[cc0000]" + gift_tips_label + "开启" );
				GameCommon.SetUIVisiable (obj, "online_gift_open_time_text", false );
			}

			List<DataRecord> groupIDRecords1 = TableCommon.FindAllRecords (DataCenter.mGroupIDConfig, lhs => {return lhs["GROUP_ID"] == awardGroupID1;});
			List<DataRecord> groupIDRecords2 = TableCommon.FindAllRecords (DataCenter.mGroupIDConfig, lhs => {return lhs["GROUP_ID"] == awardGroupID2;});
			UIGridContainer log_gift_grid = GameCommon.FindObject (obj, "log_gift_grid").GetComponent<UIGridContainer>();
			log_gift_grid.MaxCount = mMaxGifCount;
			for(int j = 0;j < mMaxGifCount; j++)
			{
				GameObject itemSubCell = log_gift_grid.controlList[j];
				DataRecord itemRecord = groupIDRecords1[j];
				GameCommon.SetItemIcon (itemSubCell, new ItemData{mID = itemRecord["ITEM_ID"], mType = itemRecord["ITEM_TYPE"], mNumber = itemRecord["ITEM_COUNT"]});
			}

			UIGridContainer online_gift_grid = GameCommon.FindObject (obj, "online_gift_grid").GetComponent<UIGridContainer>();
			online_gift_grid.MaxCount = mMaxGifCount;
			for(int j = 0; j< mMaxGifCount; j++)
			{
				GameObject itemSubCell = online_gift_grid.controlList[j];
				DataRecord itemRecord = groupIDRecords2[j];
				GameCommon.SetItemIcon (itemSubCell, new ItemData{mID = itemRecord["ITEM_ID"], mType = itemRecord["ITEM_TYPE"], mNumber = itemRecord["ITEM_COUNT"]});
			}

			if(IsCanGetDay (awardConfigIndex))
			{
				obj.SetActive (false  );
				dayCount ++ ;
			}
		}
	}
	public  void OnLineTimeIsOver(object obj)
	{
		OnOpen ();
	}

	bool IsCanGetDay(int bagIndex)
	{
		DataRecord record = DataCenter.mAwardConfig.GetRecord (bagIndex);
		string strEndTime = record["EVENT_DATE"];
		string[] strEndTimes = strEndTime.Split ('_');
		DateTime endData = new DateTime(Convert.ToInt32 (strEndTimes[0]), Convert.ToInt32 (strEndTimes[1]), Convert.ToInt32 (strEndTimes[2]), 23, 59, 59);
		
		Int64 endSeconds = GameCommon.DateTime2TotalSeconds(endData);
		Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime ());
		
		return nowSeconds > endSeconds;
	}

	bool TimeLimt (int bagIndex, int type)
	{
		DataRecord awrdRecord = DataCenter.mAwardConfig.GetRecord (bagIndex);
		int configIndex = awrdRecord ["EVENT_ID"];
		string strDayTime = awrdRecord ["EVENT_DATE"];
		DataRecord record = DataCenter.mOperateEventConfig.GetRecord (configIndex);
		string strOpenTime = record ["OPEN_TIME"];
		string strCloseTime = record ["CLOSED_TIME"];
		string[] strDayTimes = strDayTime.Split ('_');
		string[] strOpenTimes = strOpenTime.Split (':');
		string[] strCloseTimes = strCloseTime.Split (':');
		DateTime startData = new DateTime (Convert.ToInt32 (strDayTimes [0]), Convert.ToInt32 (strDayTimes [1]), Convert.ToInt32 (strDayTimes [2]),
		                                   Convert.ToInt32 (strOpenTimes [0]), Convert.ToInt32 (strOpenTimes [1]), 0);
		DateTime endData = new DateTime (Convert.ToInt32 (strDayTimes [0]), Convert.ToInt32 (strDayTimes [1]), Convert.ToInt32 (strDayTimes [2]), 
		                                 Convert.ToInt32 (strCloseTimes [0]), Convert.ToInt32 (strCloseTimes [1]), 0);
		Int64 startSeconds = GameCommon.DateTime2TotalSeconds (startData);
		Int64 endSeconds = GameCommon.DateTime2TotalSeconds (endData);
		Int64 nowSeconds = GameCommon.DateTime2TotalSeconds (GameCommon.NowDateTime ());
		if (type == 0) 
		{
			return (nowSeconds < startSeconds);
		}else if (type == 1) 
		{
			return (nowSeconds > startSeconds && nowSeconds < endSeconds);
		}else 
		{
			return (nowSeconds > endSeconds);
		}
	}
	bool beforeBuy(int bagIndex)
	{
		return TimeLimt (bagIndex, 0);
	}
	bool IsCanBuy(int bagIndex)
	{
		return TimeLimt (bagIndex, 1);
	}
	bool afterBuy(int bagIndex)
	{
		return TimeLimt (bagIndex, 2);
	}

	void ShowLogItems(int bagAwardIndex)
	{
		tEvent evt = Net.StartEvent ("CS_ReceiveFreeBagInActivity");
		int bagIndex = GetCurrentDate ();
		evt.set ("POS", 1);
		evt.set ("BAG_INDEX", bagIndex);
		evt.set ("AWARD_INDEX", bagAwardIndex);
		evt.DoEvent ();
	}
	void ShowOnlineItems(int awardIndex)
	{
		tEvent evt = Net.StartEvent ("CS_ReceiveFreeBagInActivity");
		int bagIndex = GetCurrentDate ();
		evt.set ("POS", 2);
		evt.set ("BAG_INDEX", bagIndex);
		evt.set ("AWARD_INDEX", awardIndex);
		evt.DoEvent ();
	}

	void SendMessageResult(int pos)
	{
		if(pos == 1) mbLog = true;
		else if(pos == 2) mbOnLine = true;
		RefreshLogGift ();
	}
}

class Button_log_gift_get_btn : CEvent
{
	public override bool _DoEvent ()
	{
		int bagAwardIndex = (int)getObject ("LOG_GIF_GIVE");

		DataCenter.SetData (ACTIVE_TYPE.ACTIVE_LOGIN_GIVE_GIFT_BAG.ToString () + "_WINDOW", "SHOW_LOG_ITEMS", bagAwardIndex);
		return true;
	}
}
class Button_online_gift_get_btn : CEvent
{
	public override bool _DoEvent ()
	{
		int awardIndex = (int)getObject ("ONLINE_GIF_GIVE");
		DataCenter.SetData (ACTIVE_TYPE.ACTIVE_LOGIN_GIVE_GIFT_BAG.ToString () + "_WINDOW", "SHOW_ONLINE_ITEMS", awardIndex);
		return true;
	}
}
//----------------------------------------------------------
class CS_GetFreeBagActivityInfo : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			DataCenter.SetData (ACTIVE_TYPE.ACTIVE_LOGIN_GIVE_GIFT_BAG.ToString () + "_WINDOW", "REFRESH", respEvt);
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}
class CS_ReceiveFreeBagInActivity : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			int awardIndex = get ("AWARD_INDEX");
			int pos = get ("POS");
			DataCenter.SetData (ACTIVE_TYPE.ACTIVE_LOGIN_GIVE_GIFT_BAG.ToString () + "_WINDOW", "SEND_MESSAGE_RESULT", pos);
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}


