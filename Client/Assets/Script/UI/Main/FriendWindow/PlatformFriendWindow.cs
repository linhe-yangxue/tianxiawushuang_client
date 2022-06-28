using UnityEngine;
using System.Collections.Generic;
using Logic;
using System;
using DataTable;

public class  PlatformFriendWindow : tWindow
{
	public PlatformFriendWindow(GameObject objParent)
	{
		mGameObjUI = GameCommon.FindObject (objParent,"platform_friend_window");
	}
	
	public override void Init() 
	{
		base.Init ();

		UIGridContainer grid = GameCommon.FindObject (mGameObjUI, "grid").GetComponent<UIGridContainer>();
		
		int iMaxCount = 8;
		grid.MaxCount = iMaxCount;
		for(int i = 0; i < iMaxCount; i++)
		{
			GameObject subcell = grid.controlList[i];		
			GameCommon.SetUIText (subcell, "off_time_label", "10:10:10");
		}
	}
}
