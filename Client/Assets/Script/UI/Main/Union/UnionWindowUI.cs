using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class UnionWindow : tWindow
{
	
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_union_window_info_back", new DefineFactory<Button_union_window_info_back>());
		EventCenter.Self.RegisterEvent("Button_union_lobby_button", new DefineFactory<Button_union_lobby_button>());
		EventCenter.Self.RegisterEvent("Button_union_donate_button", new DefineFactory<Button_union_donate_button>());
		EventCenter.Self.RegisterEvent("Button_union_tech_button", new DefineFactory<Button_union_tech_button>());
		EventCenter.Self.RegisterEvent("Button_union_war_button", new DefineFactory<Button_union_war_button>());
		EventCenter.Self.RegisterEvent("Button_union_shop_button", new DefineFactory<Button_union_shop_button>());

	}
	
	public override void Open (object param)
	{
		base.Open (param);
		GameCommon.ToggleTrue  (GameCommon.FindUI( "union_lobby_button"));
		DataCenter.OpenWindow ("UNION_LOBBY_WINDOW");

//		GameCommon.ToggleTrue  (GetSub ( "union_lobby_button"));
		DataCenter.OpenWindow ("BACK_GROUP_UNION_WINDOW");

	}
	
	public override void Close ()
	{
		base.Close ();

//		DataCenter.CloseWindow ("BACK_GROUP_UNION_WINDOW");

	}
	
	
	public static void CloseAllWindow()
	{
		DataCenter.CloseWindow ("UNION_LOBBY_WINDOW");
		DataCenter.CloseWindow ("UNION_DONATE_WINDOW");
		DataCenter.CloseWindow ("UNION_TECH_WINDOW");
		DataCenter.CloseWindow ("UNION_WAR_WINDOW");
		DataCenter.CloseWindow ("UNION_SHOP_WINDOW");
	}
	
	public override bool Refresh(object param)
	{
		base.Refresh (param);

		DataCenter.OpenWindow (param.ToString (), true);
		
		return true;
	}
	
}

//----------------------------------------------------------------------------------
// UnionWindow
public class Button_union_window_info_back : CEvent
{
	public override bool _DoEvent()
	{
		UnionWindow.CloseAllWindow();
		//UnionWarInfoWindow.CloseAllWindow ();
		DataCenter.CloseWindow("UNION_WINDOW");
		DataCenter.CloseWindow ("BACK_GROUP_UNION_WINDOW");
		DataCenter.CloseWindow("UNION_WAR_INFO_WINDOW");
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
		return true;
	}	
}

public class Button_union_donate_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("UNION_DONATE_WINDOW");
		return true;
	}
}

public class Button_union_lobby_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("UNION_LOBBY_WINDOW");
		return true;
	}
}

public class Button_union_tech_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("UNION_TECH_WINDOW");
		return true;
	}
}

public class Button_union_war_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow ("UNION_WAR_WINDOW");
		return true;
	}
}
public class Button_union_shop_button : CEvent
{
	public override bool _DoEvent()
	{
		
		DataCenter.OpenWindow ("UNION_SHOP_WINDOW");
		return true;
	}
}

public class Button_UnionBtn : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainUI();
		DataCenter.OpenWindow("UNION_WINDOW");
		return true;
	}
}
