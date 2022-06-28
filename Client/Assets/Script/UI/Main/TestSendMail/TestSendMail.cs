using UnityEngine;
using System.Collections;
using Logic ;
using DataTable ;
using System.Collections.Generic;
using System.Linq ;
using System;

public class TestSendMail : tWindow 
{
	public ActiveData curRoleData;
	public ActiveData curEquipData;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_close_send_mail_window_button", new DefineFactory<Button_close_send_mail_window_button>());
		EventCenter.Self.RegisterEvent("Button_send_mail_button", new DefineFactory<Button_send_mail_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);
	
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch (keyIndex)
		{
		case "SEND_MAIL_MESSAGE":
			SendMailMessage();
			break;
		case "SET_BUTTON_STATE":
			if(mGameObjUI != null)
			{
				UIImageButton btn = mGameObjUI.transform.Find("send_mail_button").GetComponent<UIImageButton>();
				if(btn != null)
					btn.isEnabled = (bool)objVal;
			}
			break;
		}
	}

	void SendMailMessage()
	{
		GameObject obj = GameCommon.FindObject (mGameObjUI ,"input_label");
		UILabel objLabel = obj.GetComponent<UILabel >();
		string strText = objLabel.text;
		string[] strTexts = strText.Split (',');

//		int[] iTidNum = new int[strText.Length];
		string titleName = "测试发邮件";

//		int iTid =Convert.ToInt32(objLabel.text);
//		string titleName = System.Convert.ToString (GameCommon.GetItemField(iTid, GET_ITEM_FIELD_TYPE.NAME));
		string[] _textStr = strTexts[0].Split ('*');
		if(_textStr[0] == "12345")
		{
			if(_textStr.Length == 2)
			{
				NetManager.RequestSendGuildExp(Convert.ToInt32(_textStr[1]));
			}else
			{
				NetManager.RequestSendGuildExp(1000);
			}
		}else
		{
			NetManager.RequestSendMail(strTexts, titleName);
		}

		DataCenter.SetData("SEND_MAIL_WINDOW", "SET_BUTTON_STATE", false);
	}
}
public class Button_SendMailBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("SEND_MAIL_WINDOW");
		MainUIScript.Self.HideMainBGUI ();
		return true;
	}
}
public class Button_close_send_mail_window_button : CEvent
{
	public override bool _DoEvent()
	{
        //MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
		DataCenter.CloseWindow ("SEND_MAIL_WINDOW");
		MainUIScript.Self.ShowMainBGUI ();
		return true;
	}
}
public class Button_send_mail_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("SEND_MAIL_WINDOW", "SEND_MAIL_MESSAGE", true);
		return true;
	}
}








