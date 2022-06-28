using UnityEngine;
using System.Collections.Generic;
using Logic;
using System;
using DataTable;

public class InvitationFriendWindow : tWindow
{
	public InvitationFriendWindow(GameObject objParent)
	{
		mGameObjUI = GameCommon.FindObject (objParent,"invitation_friend_window");
	}
}
