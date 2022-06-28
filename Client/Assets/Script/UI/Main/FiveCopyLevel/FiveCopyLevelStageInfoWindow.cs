using UnityEngine;
using System.Collections;
using System;
using Logic;
using DataTable;

public class FiveCopyLevelStageInfoWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Register ("Button_close_five_copy_level_stage_info_window", new DefineFactory<Button_close_five_copy_level_stage_info_window>());
		EventCenter.Register ("Button_copy_level_clean", new DefineFactory<Button_close_five_copy_level_stage_info_window>());
		EventCenter.Register ("Button_copy_level_start", new DefineFactory<Button_close_five_copy_level_stage_info_window>());

		EventCenter.Register ("Button_select_copy_level_type_button", new DefineFactory<Button_select_copy_level_type_button>());
		EventCenter.Register ("Button_select_add_type_button", new DefineFactory<Button_select_add_type_button>());
		EventCenter.Register ("Button_select_player_level_type_button", new DefineFactory<Button_select_player_level_type_button>());

		EventCenter.Register ("Button_copy_level_type_novice_button", new DefineFactory<Button_copy_level_type_novice_button>());
		EventCenter.Register ("Button_copy_level_type_help_button", new DefineFactory<Button_copy_level_type_help_button>());
		EventCenter.Register ("Button_copy_level_type_oneself_button", new DefineFactory<Button_copy_level_type_oneself_button>());
		EventCenter.Register ("Button_copy_level_type_soldier_button", new DefineFactory<Button_copy_level_type_soldier_button>());

		EventCenter.Register ("Button_copy_level_enter_all_button", new DefineFactory<Button_copy_level_enter_all_button>());
		EventCenter.Register ("Button_copy_level_enter_just_friend_button", new DefineFactory<Button_copy_level_enter_just_friend_button>());
		EventCenter.Register ("Button_copy_level_enter_no_one_button", new DefineFactory<Button_copy_level_enter_no_one_button>());

		EventCenter.Register ("Button_copy_level_player_level_button1", new DefineFactory<Button_copy_level_player_level_button>());
		EventCenter.Register ("Button_copy_level_player_level_button2", new DefineFactory<Button_copy_level_player_level_button>());
		EventCenter.Register ("Button_copy_level_player_level_button3", new DefineFactory<Button_copy_level_player_level_button>());

		EventCenter.Register ("Button_select_type_parent_close_button", new DefineFactory<Button_select_type_parent_close_button>());
	}
	//FIVE_COPY_LEVEL_STAGE_INFO_WINDOW
	public override void Open (object param)
	{
		base.Open (param);
		Refresh (param);
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);

		switch(keyIndex)
		{
		case "SET_VISIBLE_COPY_LEVEL_STATE_INFO":
			SetVisibleCopyLevelStateInfo((bool)objVal);
			break;
		case "SET_VISIBLE_SELECT_TYPE_PARENT":
			SetVisibleSelectTypeParent((bool)objVal);
			break;
		case "SET_SELECT_TYPE_PARENT":
			SetSelectTypeParent((int)objVal);
			break;
		case "SET_COPY_LEVEL_ADD_TYPE":
			SetCopyLevelAddType ((int)objVal);
			break;
		case "SET_COPY_LEVEL_PLAYER_LEVEL_TYPE":
			SetCopyLevelPlayerLevelType ((int)objVal);
			break;
		case "SET_COPY_LEVEL_TYPE":
			SetCopyLevelType ((int)objVal);
			break;
		}
	}

	public override bool Refresh (object param)
	{
		RefreshPlayerInfo();
		RefreshTeamInfo();
		return true;
	}

	void SetVisibleCopyLevelStateInfo(bool bVisible)
	{
		SetVisible ("select_type_add", bVisible);

		if(!bVisible) return;

		SetVisibleSelectTypeParent (false);
		SetCopyLevelType(1);
		SetCopyLevelAddType(1);
		SetCopyLevelPlayerLevelType(1);
	}

	void SetVisibleSelectTypeParent(bool bVisible)
	{
		SetVisible ("select_type_parent", bVisible);
	}

	void SetSelectTypeParent(int index)
	{
		SetVisibleSelectTypeParent (true);
		SetVisible ("select_type_add", false);
		SetVisible ("select_type_copy_level", false);
		SetVisible ("select_type_player", false);

		if(index == 1) SetSelectTypeCopyLevel();
		else if(index == 2) SetSelectTypeAddState();
		else if(index == 3) SetSelecteTypePlayer();
	}

	void SetSelectTypeAddState()
	{
		SetVisible ("select_type_add", true);
		int iIndex = get ("CURRENT_INDEX");

		if(iIndex == 2) GameCommon.ToggleTrue (GetSub ("copy_level_enter_just_friend_button"));
		else if(iIndex == 3) GameCommon.ToggleTrue (GetSub ("copy_level_enter_no_one_button"));
		else GameCommon.ToggleTrue (GetSub ("copy_level_enter_all_button"));
	}

	void SetSelectTypeCopyLevel()
	{
		SetVisible ("select_type_copy_level", true);
		int iIndex = get ("CURRENT_INDEX");

		if(iIndex == 2) GameCommon.ToggleTrue (GetSub ("copy_level_type_soldier_button"));
		else if(iIndex == 3) GameCommon.ToggleTrue (GetSub ("copy_level_type_help_button"));
		else if(iIndex == 4) GameCommon.ToggleTrue (GetSub ("copy_level_type_oneself_button"));
		else GameCommon.ToggleTrue (GetSub ("copy_level_type_novice_button"));
	}

	void SetSelecteTypePlayer()
	{
		SetVisible ("select_type_player", true);
		int iIndex = get ("CURRENT_INDEX");

		if(iIndex == 2) GameCommon.ToggleTrue (GetSub ("copy_level_player_level_button2"));
		else if(iIndex == 3) GameCommon.ToggleTrue (GetSub ("copy_level_player_level_button3"));
		else GameCommon.ToggleTrue (GetSub ("copy_level_player_level_button1"));
	}

	void SetCopyLevelType(int index)
	{
		string strType = "欢迎新手";
		if(index == 1) strType = "欢迎新手";
		else if(index == 2) strType = "欢迎战友";
		else if(index == 3) strType = "求帮助";
		else if(index == 4) strType = "挑战者";
		else index = 1;

		GetSub ("select_copy_level_type_button").GetComponentInChildren<UILabel>().text = strType;

		GameCommon.GetButtonData (GetSub ("select_copy_level_type_button")).set ("CURRENT_INDEX", index);
	}

	void SetCopyLevelAddType(int index)
	{
		string strType = "所有人";
		if(index == 1) strType = "所有人";
		else if(index == 2) strType = "仅限好友";
		else if(index == 3) strType = "关闭";
		else index = 1;

		GetSub ("select_add_type_button").GetComponentInChildren<UILabel>().text = strType;

		GameCommon.GetButtonData (GetSub ("select_add_type_button")).set ("CURRENT_INDEX", index);
	}

	void SetCopyLevelPlayerLevelType(int index)
	{
		string strType = "封灵师以上";
		if(index == 1) strType = "封灵师以上";
		else if(index == 2) strType = "降妖师以上";
		else if(index == 3) strType = "驱鬼师以上";
		else index = 1;

		GetSub ("select_player_level_type_button").GetComponentInChildren<UILabel>().text = strType;

		GameCommon.GetButtonData (GetSub ("select_player_level_type_button")).set ("CURRENT_INDEX", index);
	}

	private void RefreshPlayerInfo()
	{
		GameCommon.GetButtonData (GetSub ("fa_bao_button")).set ("ACTION", "FIVE_COPY_LEVEL_ACTION");
		GameCommon.GetButtonData (GetSub ("change_team_button")).set ("ACTION", "FIVE_COPY_LEVEL_ACTION");

		GameObject playerInfoObj = GetSub("player_info");
		RoleData d = RoleLogicData.GetMainRole();
		
		string roleName = TableCommon.GetStringFromActiveCongfig(d.tid, "NAME");
		GameCommon.SetUIText(playerInfoObj, "player_name", roleName);
		
        DataRecord record = DataCenter.mRoleSkinConfig.GetRecord(d.tid);
		if (record != null)
		{
			string roleHeadAtlas = record.getData("ICON_ATLAS");
			string roleHeadSprite = record.getData("ICON_SPRITE");
			GameCommon.SetIcon(playerInfoObj, "player_head", roleHeadSprite, roleHeadAtlas);
		}
		
		int baseHp = GameCommon.GetBaseMaxHP(d.tid, d.level, 0);
		int baseMp = GameCommon.GetBaseMaxMP(d.tid, d.level, 0);
		int baseAttack = (int)GameCommon.GetBaseAttack(d.tid, d.level, 0);
		int addHp = GameCommon.GetBaseMaxHP(d.tid, d.level, 0) - baseHp;
		int addMp = GameCommon.GetBaseMaxMP(d.tid, d.level, 0) - baseMp;
		int addAttack = (int)GameCommon.GetBaseAttack(d.tid, d.level, 0) - baseAttack;
		GameCommon.SetUIText(playerInfoObj, "life_num", CombineAttributeText(baseHp, addHp));
		GameCommon.SetUIText(playerInfoObj, "magic_num", CombineAttributeText(baseMp, addMp));
		GameCommon.SetUIText(playerInfoObj, "damage_num", CombineAttributeText(baseAttack, addAttack));
		
		GameObject levelobj = GetSub("level_Info");
		GameCommon.SetUIText(levelobj, "player_level", "Lv." + d.level);
		int levelUpExp = TableManager.GetData("CharacterLevelExp", d.level, "LEVEL_EXP");
		float exp = d.exp;
		float expRate = exp / levelUpExp;
		GameCommon.FindComponent<UIProgressBar>(levelobj, "bar").value = expRate;
		
		RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		EquipData curUseEquip = roleEquipLogicData.GetUseEquip();
		
		if (curUseEquip != null)
		{
			GameCommon.SetUIVisiable(playerInfoObj, "attribute_icon", true);
			GameCommon.SetUIVisiable(playerInfoObj, "none_label", false);
			int iElementIndex = (int)curUseEquip.mElementType;
			GameCommon.SetElementIcon(playerInfoObj, "attribute_icon", iElementIndex);
		}
		else
		{
			GameCommon.SetUIVisiable(playerInfoObj, "attribute_icon", false);
			GameCommon.SetUIVisiable(playerInfoObj, "none_label", true);
		}

		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "fa_bao_button", UNLOCK_FUNCTION_TYPE.FA_BAO);
	}
	
	private string CombineAttributeText(int baseValue, int addValue)
	{
		if (addValue == 0)
			return baseValue.ToString();
		else
			return baseValue.ToString() + " [00FF00]+" + addValue.ToString() + "[-]";
	}
	
	private void RefreshTeamInfo()
	{
		DataCenter.SetData("SELECT_LEVEL_TEAM_INFO_WINDOW", "TEAM_REFRESH", null);
	}
	
}

public class Button_close_five_copy_level_stage_info_window : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW");
		return true;
	}
}

public class Button_select_copy_level_type_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "CURRENT_INDEX", getObject ("CURRENT_INDEX"));
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_SELECT_TYPE_PARENT", 1);
		return true;
	}
}

public class Button_select_add_type_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "CURRENT_INDEX", getObject ("CURRENT_INDEX"));
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_SELECT_TYPE_PARENT", 2);
		return true;
	}
}

public class Button_select_player_level_type_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "CURRENT_INDEX", getObject ("CURRENT_INDEX"));
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_SELECT_TYPE_PARENT", 3);
		return true;
	}
}

public class Button_copy_level_type_novice_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_COPY_LEVEL_TYPE", 1);
		return true;
	}
}

public class Button_copy_level_type_help_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_COPY_LEVEL_TYPE", 3);
		return true;
	}
}

public class Button_copy_level_type_oneself_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_COPY_LEVEL_TYPE", 4);
		return true;
	}
}

public class Button_copy_level_type_soldier_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_COPY_LEVEL_TYPE", 2);
		return true;
	}
}

public class Button_copy_level_enter_all_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_COPY_LEVEL_ADD_TYPE", 1);
		return true;
	}
}

public class Button_copy_level_enter_just_friend_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_COPY_LEVEL_ADD_TYPE", 2);
		return true;
	}
}

public class Button_copy_level_enter_no_one_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_COPY_LEVEL_ADD_TYPE", 3);
		return true;
	}
}

public class Button_copy_level_player_level_button : CEvent
{
	public override bool _DoEvent ()
	{
		string buttonName = getObject ("NAME").ToString();
		int iLevel = System.Convert.ToInt32 ( buttonName.Substring (buttonName.Length - 1, 1));
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_COPY_LEVEL_PLAYER_LEVEL_TYPE", iLevel);
		return true;
	}
}

public class Button_select_type_parent_close_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_VISIBLE_SELECT_TYPE_PARENT", false);
		return true;
	}
}
