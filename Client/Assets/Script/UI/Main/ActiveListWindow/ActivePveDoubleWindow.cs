using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

public class ActivePveDoubleWindow : ActiveTotalWindow
{
	public override void Init ()
	{
		EventCenter.Register ("Button_go_double_drop_level_button", new DefineFactory<Button_go_double_drop_level_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		mDesLabelName = "double_drop_describe_label";
		mCountdownLabelName = "double_drop_rest_time";
	}
}

class Button_go_double_drop_level_button : CEvent
{
	public override bool _DoEvent ()
	{
		return true;
	}
}

