using UnityEngine;
using System.Collections;
using Logic;

public class InviteCodeWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_invite_code_close_button", new DefineFactory<Button_invite_code_close_button>());
//		EventCenter.Self.RegisterEvent("Button_know_button", new DefineFactory<Button_know_button>());
		EventCenter.Self.RegisterEvent("Button_validation_button", new DefineFactory<Button_validation_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);

		UIInput inputInviteCode = GameCommon.FindObject (mGameObjUI, "input_invite_code").transform.GetComponent<UIInput>();
		inputInviteCode.value = "";

		SetText ("validation_result_label", null);
		SetText ("invite_code_lable", RoleLogicData.Self.mStrID);
		SetText ("label_player_name", RoleLogicData.Self.name + " :");
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch(keyIndex)
		{
		case "VALIDATION_INVITE_CODE":
			UIInput inputInviteCode = GameCommon.FindObject (mGameObjUI, "input_invite_code").transform.GetComponent<UIInput>();
			if(inputInviteCode.value != "")
			{
				if(inputInviteCode.value == RoleLogicData.Self.mStrID)
				DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_INVITE_CODE_INPUT_SELF);
				else
				{
					tEvent evt = Net.StartEvent("CS_AddFriend");
					evt.set ("WINDOW_NAME", "INVITE_CODE_WINDOW");
					evt.set("FRIEND_STR", inputInviteCode.value);
					evt.DoEvent();
				}
			}
			else
				DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_INVITE_CODE_NEED_INPUT);
			break;
//		case "VALIDATION_RESULT":
//			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_INVITE_CODE_INPUT_ERROR);
//			break;
//	    case "VALIDATION_RESULT_USED":
//			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_INVITE_CODE_INPUTED);
//			break;
//		case "VALIDATION_RESULT_SUCCESS":
//			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_INVITE_CODE_INPUT_SUCCESS);
//			break;
		}
	}

}


public class Button_invite_code_close_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("INVITE_CODE_WINDOW");
		return true;
	}
}

public class Button_know_button : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.GoBack ();
		return true;
	}
}

public class Button_validation_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("INVITE_CODE_WINDOW", "VALIDATION_INVITE_CODE", true);
		return true;
	}
}


