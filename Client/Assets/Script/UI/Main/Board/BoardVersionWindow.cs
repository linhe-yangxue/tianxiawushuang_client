using UnityEngine;
using System.Collections;
using DataTable;
using Logic;

public class BoardVersionWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Register ("Button_board_version_window_close_button", new DefineFactory<Button_board_version_window_close_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);

		NiceTable table = param as NiceTable;
		if(table == null)
			return;

		string strDescribe = table.GetRecord (0)["DESC"];
//		int iInsetID = strDescribe.IndexOf ('\\');
//		strDescribe = strDescribe.Insert (iInsetID, "[sub]");
		
		strDescribe = strDescribe.Replace ("\\n", "\n");
		SetText ("vip_description_label", strDescribe);
	}

}

class Button_board_version_window_close_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow ("BOARD_VERSION_WINDOW");
		return true;
	}
}
