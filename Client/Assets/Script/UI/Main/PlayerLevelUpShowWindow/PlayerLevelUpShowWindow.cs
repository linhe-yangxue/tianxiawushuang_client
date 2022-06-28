using UnityEngine;
using System.Collections;
using Logic;

public class PlayerLevelUpShowWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_close_player_level_window_button", new DefineFactory<Button_close_player_level_window_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		Refresh(param);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "NEED_LEVEL")
		{
			int needLevel = GameCommon.FunctionUnlockNeedLevel ((UNLOCK_FUNCTION_TYPE)objVal);
			SetText ("need_level", needLevel.ToString ());
		}
	}

	public override bool Refresh (object param)
	{
		RoleLogicData roleData = RoleLogicData.Self;
		SetText ("old_player_level_label", (roleData.chaLevel - 1).ToString ());
		SetText ("old_player_vitality_label", roleData.stamina.ToString ());
		SetText ("player_level_label", roleData.chaLevel.ToString ());
		SetText ("player_vitality_label", roleData.stamina >= roleData.mMaxStamina ? roleData.stamina.ToString () : roleData.mMaxStamina.ToString ());
		string strUnlockFunction = TableCommon.GetStringFromPlayerLevelConfig (roleData.chaLevel, "UNLOCK_TITLE");
		if(strUnlockFunction == "")
			strUnlockFunction = "无";
		strUnlockFunction = strUnlockFunction.Replace ("\\n", "\n");
		SetText ("unlock_function", strUnlockFunction);

		bool bVisible = (bool)param;
		SetVisible ("unlock_background", bVisible);
		SetVisible ("lock_background", !bVisible);

		return true;
	}
	
}


public class Button_close_player_level_window_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("PLAYER_LEVEL_UP_SHOW_WINDOW");

        if (OptionalGuide.NotifyPlayerLevelUp(RoleLogicData.Self.chaLevel))
        {
            if (MainProcess.mStage != null)
            {
                MainProcess.QuitBattle();
                MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);
            }
            else
            {
                DataCenter.CloseWindow("PVE_ACCOUNT_CLEAN_WINDOW");
                MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
            }
        }

		return true;
	}
}
