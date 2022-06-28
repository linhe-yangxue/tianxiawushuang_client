using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class UnionWarInfoWindow : tWindow
{
	
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_union_window_info_back", new DefineFactory<Button_union_window_info_back>());
		EventCenter.Self.RegisterEvent("Button_union_battle_prepare_button", new DefineFactory<Button_union_battle_prepare_button>());
		EventCenter.Self.RegisterEvent("Button_union_all_rank_button", new DefineFactory<Button_union_all_rank_button>());
		
	}
	
	public override void Open (object param)
	{
		base.Open (param);
		GameCommon.ToggleTrue  (GameCommon.FindUI( "union_battle_prepare_button"));
		DataCenter.OpenWindow ("UNION_BATTLE_PREPARE_WINDOW");
		DataCenter.OpenWindow ("BACK_GROUP_UNION_WINDOW");
		DataCenter.CloseWindow ("UNION_WINDOW");
		DataCenter.CloseWindow ("UNION_WAR_WINDOW");
		
	}
	
	public override void Close ()
	{
		base.Close ();
//		DataCenter.CloseWindow ("BACK_GROUP_UNION_WINDOW");
		
	}

	public static void CloseAllWindow()
	{
		DataCenter.CloseWindow ("UNION_ALL_RANK_WINDOW");
		DataCenter.CloseWindow ("UNION_BATTLE_PREPARE_WINDOW");
	}
	
	public override bool Refresh(object param)
	{
		base.Refresh (param);
		DataCenter.OpenWindow (param.ToString (), true);
		return true;
	}
}
//UnionWarInfo   Window
public class Button_union_battle_prepare_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("UNION_BATTLE_PREPARE_WINDOW");

		return true;
	}
}

public class Button_union_all_rank_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("UNION_ALL_RANK_WINDOW");
		return true;
	}
}

//工会战整体排名
public class UnionAllRankWindow : tWindow
{
	public override void OnOpen()
	{
		DataCenter.CloseWindow ("UNION_BATTLE_PREPARE_WINDOW");

	}
}

//工会战准备界面
public class UnionBattlePrepareWindow : tWindow
{
	public override void OnOpen()
	{
		DataCenter.CloseWindow ("UNION_ALL_RANK_WINDOW");
		
	}
}