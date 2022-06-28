using UnityEngine;
using System.Collections;
using DataTable;
using System.Collections.Generic;
using System;
using System.Linq;
using Logic;

public class TeamRightRoleInfoWindow : tWindow
{
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_role_level_up_button", new DefineFactory<Button_role_level_up_button>());
		EventCenter.Self.RegisterEvent("Button_role_break_button", new DefineFactory<Button_role_break_button>());
		EventCenter.Self.RegisterEvent("Button_role_fate_button", new DefineFactory<Button_role_fate_button>());
		EventCenter.Self.RegisterEvent("Button_role_skill_button", new DefineFactory<Button_role_skill_button>());

	}

	public override void Open(object param)
	{

		base.Open(param);
        CloseAllWindow();
        UIButton btn = GameCommon.FindComponent<UIButton>(mGameObjUI, "role_level_up_button");
        if (btn != null)
        {
			if(CommonParam.isOnLineVersion)
			{
				btn.isEnabled = PackageManager.GetItemTypeByTableID(TeamPosInfoWindow.mCurActiveData.tid) == ITEM_TYPE.PET;
			}
			else
			{
				btn.gameObject.SetActive(PackageManager.GetItemTypeByTableID(TeamPosInfoWindow.mCurActiveData.tid) == ITEM_TYPE.PET);
			}
        }
        UIButton skillBtn = GameCommon.FindComponent<UIButton>(mGameObjUI, "role_skill_button");
        if (skillBtn != null)
        {
			if(CommonParam.isOnLineVersion)
			{
				skillBtn.isEnabled = PackageManager.GetItemTypeByTableID(TeamPosInfoWindow.mCurActiveData.tid) != ITEM_TYPE.CHARACTER;
			}
			else
			{
				skillBtn.gameObject.SetActive(PackageManager.GetItemTypeByTableID(TeamPosInfoWindow.mCurActiveData.tid) != ITEM_TYPE.CHARACTER);
			}
            ;
        }
        if ((string)param == "ROLE_LEVEL_UP")
        {
            GameCommon.ToggleTrue(GetSub("role_level_up_button"));
            DataCenter.OpenWindow("PET_LEVEL_UP_INFO_WINDOW");
        }
        else if ((string)param == "ROLE_FATE")
		{
			GameCommon.ToggleTrue (GetSub ("role_fate_button"));
			
//			DataCenter.OpenWindow("FATE_INFO_WINDOW", "SEND_FATE_STRENGTHEN_VALUE_MESSAGE");
			DataCenter.OpenWindow("FATE_INFO_WINDOW");
			DataCenter.SetData ("FATE_INFO_WINDOW",  "SEND_FATE_STRENGTHEN_VALUE_MESSAGE", true);
		}
		else if((string)param == "ROLE_BREAK")
		{
			GameCommon.ToggleTrue (GetSub ("role_break_button"));
			DataCenter.OpenWindow("BREAK_INFO_WINDOW");
		}
		else if((string)param == "ROLE_SKILL") 
		{
           
			GameCommon.ToggleTrue (GetSub ("role_skill_button"));
			DataCenter.OpenWindow("SKILL_UPGRADE_WINDOW");
		}

	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);

//		if (keyIndex == "ROLE_FATE")
//		{
//			GameCommon.ToggleTrue (GetSub ("role_fate_button"));
//			
//			DataCenter.OpenWindow("FATE_INFO_WINDOW", "SEND_FATE_STRENGTHEN_MESSAGE");
//		}
//		else if(keyIndex == "ROLE_BREAK")
//		{
//			GameCommon.ToggleTrue (GetSub ("role_break_button"));
//			DataCenter.OpenWindow("BREAK_INFO_WINDOW");
//		}


	}

	public void CloseAllWindow()
	{
        DataCenter.CloseWindow("PET_LEVEL_UP_INFO_WINDOW");
		DataCenter.CloseWindow("FATE_INFO_WINDOW");
		DataCenter.CloseWindow("BREAK_INFO_WINDOW");
		DataCenter.CloseWindow("SKILL_UPGRADE_WINDOW");
	}

}

class Button_role_level_up_button : CEvent
{
	public override bool _DoEvent ()
	{
        DataCenter.OpenWindow("TEAM_RIGHT_ROLE_INFO_WINDOW", "ROLE_LEVEL_UP");
		return true;
	}
}
class Button_role_break_button : CEvent
{
	public override bool _DoEvent ()
	{
        //DataCenter.OpenWindow("BREAK_INFO_WINDOW");
        DataCenter.OpenWindow("TEAM_RIGHT_ROLE_INFO_WINDOW", "ROLE_BREAK");
        

		return true;
	}
}
class Button_role_fate_button : CEvent
{
	public override bool _DoEvent ()
    {
        //DataCenter.OpenWindow("FATE_INFO_WINDOW");
        //DataCenter.SetData("FATE_INFO_WINDOW", "SEND_FATE_STRENGTHEN_VALUE_MESSAGE", true);

        DataCenter.OpenWindow("TEAM_RIGHT_ROLE_INFO_WINDOW", "ROLE_FATE");
		return true;
	}
}
class Button_role_skill_button : CEvent
{
	public override bool _DoEvent ()
	{
        //DataCenter.OpenWindow("SKILL_UPGRADE_WINDOW");

        DataCenter.OpenWindow("TEAM_RIGHT_ROLE_INFO_WINDOW", "ROLE_SKILL");
		return true;
	}
}