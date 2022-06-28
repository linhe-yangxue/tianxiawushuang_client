using UnityEngine;
using System.Collections;
using System;
using Logic;
using DataTable;

public class FiveCopyLevelWindow : tWindow
{

}

public class Button_five_copy_level_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.OpenWindow ("FIVE_COPY_LEVEL_START_WINDOW");
		return true;
	}
}

//------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------
public class FiveCopyLevelStartWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Register ("Button_close_five_copy_level_start_window", new DefineFactory<Button_close_five_copy_level_start_window>());
		EventCenter.Register ("Button_creat_copy_level_button", new DefineFactory<Button_creat_copy_level_button>());
		EventCenter.Register ("Button_search_copy_level_button", new DefineFactory<Button_search_copy_level_button>());
	}
}

public class Button_close_five_copy_level_start_window : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow ("FIVE_COPY_LEVEL_START_WINDOW");
		return true;
	}
}

public class Button_creat_copy_level_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.OpenWindow ("FIVE_COPY_LEVEL_SELECT_WINDOW");
		DataCenter.CloseWindow ("FIVE_COPY_LEVEL_START_WINDOW");
		return true;
	}
}

public class Button_search_copy_level_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.OpenWindow ("FIVE_COPY_LEVEL_SEARCH_WINDOW");
		DataCenter.CloseWindow ("FIVE_COPY_LEVEL_START_WINDOW");
		return true;
	}
}

//------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------
public class FiveCopyLevelSelectWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Register ("Button_close_five_copy_level_select_window", new DefineFactory<Button_close_five_copy_level_select_window>());
		EventCenter.Register ("Button_begin_copy_level", new DefineFactory<Button_begin_copy_level>());
		EventCenter.Register ("Button_select_copy_level_enter01", new DefineFactory<Button_select_copy_level_enter>());
		EventCenter.Register ("Button_select_copy_level_enter02", new DefineFactory<Button_select_copy_level_enter>());
		EventCenter.Register ("Button_select_copy_level_enter03", new DefineFactory<Button_select_copy_level_enter>());
	}

	public override void Open(object param)
	{
		base.Open(param);
		Refresh(param);
	}

	public override bool Refresh(object param) 
	{
		UIGridContainer  grid = GetSub ("grid").GetComponent<UIGridContainer>();
		if(grid != null)
		{
			grid.MaxCount = 8;
		}

		return true;
	}
		
}

public class Button_close_five_copy_level_select_window : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow ("FIVE_COPY_LEVEL_SELECT_WINDOW");
		return true;
	}
}

public class Button_begin_copy_level : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.OpenWindow ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW");
		DataCenter.SetData ("FIVE_COPY_LEVEL_STAGE_INFO_WINDOW", "SET_VISIBLE_COPY_LEVEL_STATE_INFO", true);
//		DataCenter.CloseWindow ("FIVE_COPY_LEVEL_SELECT_WINDOW");
		return true;
	}
}

public class Button_select_copy_level_enter : CEvent
{
	public override bool _DoEvent ()
	{
//		DataCenter.CloseWindow ("FIVE_COPY_LEVEL_SELECT_WINDOW");
		return true;
	}
}

//------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------
public class FiveCopyLevelSearchWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Register ("Button_join_copy_level", new DefineFactory<Button_join_copy_level>());
		EventCenter.Register ("Button_close_five_copy_level_search_window", new DefineFactory<Button_close_five_copy_level_search_window>());
	}
	
	public override void Open(object param)
	{
		base.Open(param);
		Refresh(param);
	}
	
	public override bool Refresh(object param) 
	{
		int count = 8;
		UIGridContainer grid = GetSub ("grid").GetComponent<UIGridContainer>();
		if(grid != null)
		{
			grid.MaxCount = count;
			for(int i = 0; i < count; i++)
			{
				GameObject subcell = grid.controlList[i];

				int progress = UnityEngine.Random.Range (10, 90);
				int difficute = UnityEngine.Random.Range (0, 10);

				GameCommon.SetUIText (subcell, "creater_player_name", RoleLogicData.Self.name);
				GameCommon.SetUIText (subcell, "difficute_label", difficute.ToString ());
				GameCommon.SetUIText (subcell, "limit_label", "所有人");
				GameCommon.SetUIText (subcell, "progress_label", progress.ToString () + "%");
				GameCommon.SetUIText (subcell, "type_label", "欢迎新手");

				Int64 countdownTime =  CommonParam.NowServerTime() + UnityEngine.Random.Range (300, 7200);
				GameObject countdownTimeLabelObj = GameCommon.FindObject (subcell, "time_countdown");
				if(countdownTimeLabelObj != null && countdownTimeLabelObj.GetComponent<CountdownUI>() != null)
					MonoBehaviour.Destroy (countdownTimeLabelObj.GetComponent<CountdownUI>());
				SetCountdown (subcell, "time_countdown", countdownTime, new CallBack(this, "CountdownTimeOver", subcell));

				UIGridContainer gridIcon = GameCommon.FindObject (subcell, "grid_icon").GetComponent<UIGridContainer>();
				int iconCount = 5;
				gridIcon.MaxCount = 5;
				for(int j = 0; j < iconCount; j++)
				{
					UISprite icon = gridIcon.controlList[j].GetComponent<UISprite>();
					if(icon != null)
					{
						int iconIndex = UnityEngine.Random.Range (102011, 102022);
						icon.spriteName = TableCommon.GetStringFromActiveCongfig (iconIndex, "HEAD_SPRITE_NAME");
					}
				}
			}
		}
		
		return true;
	}

	public void CountdownTimeOver(object obj)
	{
		GameObject o = obj as GameObject;
		o.SetActive (false);
		UIGridContainer grid = GetSub ("grid").GetComponent<UIGridContainer>();
		grid.Reposition ();
	}
	
}

class Button_close_five_copy_level_search_window : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow ("FIVE_COPY_LEVEL_SEARCH_WINDOW");
		DataCenter.OpenWindow ("FIVE_COPY_LEVEL_START_WINDOW");
		return true;
	}
}

class Button_join_copy_level : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow ("FIVE_COPY_LEVEL_SEARCH_WINDOW");
		DataCenter.OpenWindow ("FIVE_COPY_LEVEL_START_WINDOW");
		return true;
	}
}

//------------------------------------------------------------------------------------
//------------------------------------------------------------------------------------