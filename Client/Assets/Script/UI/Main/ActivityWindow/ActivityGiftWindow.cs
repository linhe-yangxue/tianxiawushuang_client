using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable ;
using System.Collections.Generic;

public class ActivityGiftWindow : tWindow 
{
	public override void Init()
	{
		EventCenter.Self.RegisterEvent ("Button_activity_gift_search_btn", new DefineFactory<Button_activity_gift_search_btn>());
	}
	
	public override void Open(object param)
	{
		base.Open (param);
		GameObject obj = GameCommon.FindObject (mGameObjUI ,"code_input");
		UILabel objLabel = obj.transform.Find ("Label").GetComponent<UILabel >();
		objLabel.text = "";
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch (keyIndex)
		{
		case "SEND_GIFT_CODE_MESSAGE":
			SendGiftCodeMessage();
			break;
		case "GET_GIFT_SUCCESS":
			GetGiftSuccess ((string)objVal);
			break;
		case "SET_BUTTON_STATE":
			if(mGameObjUI != null)
			{
				UIImageButton btn = mGameObjUI.transform.Find("activity_gift_search_btn").GetComponent<UIImageButton>();
				if(btn != null)
					btn.isEnabled = (bool)objVal;
			}
			break;
		}
	}
	void GetGiftSuccess(string text)
	{
		SC_GiftCode giftCode = JCode.Decode<SC_GiftCode>(text);
		if( null == giftCode )
			return ;
		if(giftCode.errorIndex == 0)
		{
//			DataCenter.OpenMessageTipsWindow (STRING_INDEX.ERROR_GIFT_CODE_NOT_EXIST);
			DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_GIFT_CODE_NOT_EXIST);
		}else if(giftCode.errorIndex == 1)
		{
//			DataCenter.OpenMessageTipsWindow (STRING_INDEX.ERROR_GIFT_CODE_OVERDUE);
			DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_GIFT_CODE_OVERDUE);
		}else if(giftCode.errorIndex == 2)
		{
//			DataCenter.OpenMessageTipsWindow(STRING_INDEX.ERROR_GIFT_CODE_INVALID);
			DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_GIFT_CODE_INVALID);
		}else if(giftCode.exchangeItem != null)
		{
			List<ItemDataBase> itemDataList = PackageManager.UpdateItem (giftCode.exchangeItem);

			DataCenter.OpenWindow ("AWARDS_TIPS_WINDOW", itemDataList);

		}
	}

	void SendGiftCodeMessage()
	{
		GameObject obj = GameCommon.FindObject (mGameObjUI ,"code_input");
		UILabel objLabel = obj.transform.Find ("Label").GetComponent<UILabel >();
		string strText = objLabel.text;
		NetManager.RequestGiftCode (strText);
		DataCenter.SetData("ACTIVITY_GIFT_WINDOW", "SET_BUTTON_STATE", false);
	}

}
public class Button_activity_gift_search_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ACTIVITY_GIFT_WINDOW", "SEND_GIFT_CODE_MESSAGE", true);
		return true;
	}
}
