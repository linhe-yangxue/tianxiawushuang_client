using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataTable;
using Logic;

public class SC_RequestUpdateNotice : DefaultNetEvent
{
    public override bool _DoEvent()
    {
		NiceTable table = getTable("UPDATE_NOTICE");
//		DataCenter.mBoardVersionConfig = table;

		if(!DataCenter.Get ("FIRST_ENTER_GAME"))
		{
			if(table != null && table.GetRecordCount() > 0)
			{
				DataCenter.OpenWindow("BOARD_VERSION_WINDOW", table);
				DataCenter.Set ("FIRST_ENTER_GAME", true);
				//GameCommon.SetBackgroundSound("Sound/Opening", 0.5f);
			}
		}

		Finish();
		return true;
    }
}
