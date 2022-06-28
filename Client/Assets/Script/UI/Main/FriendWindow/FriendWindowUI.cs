using UnityEngine;
using System.Collections.Generic;
using Logic;
using System;
using DataTable;

public class FriendWindowUI : MonoBehaviour {

	void Start () 
	{
		DataCenter.Self.registerData ("FRIEND_WINDOW", new FriendWindow(gameObject));

        EventCenter.Self.RegisterEvent("Button_friend_back", new DefineFactory<Button_friend_back>());
        EventCenter.Self.RegisterEvent("Button_add_friend_button", new DefineFactory<Button_add_friend_button>());
        EventCenter.Self.RegisterEvent("Button_aquire_spirit_button", new DefineFactory<Button_aquire_spirit_button>());
		EventCenter.Self.RegisterEvent("Button_game_friend_button", new DefineFactory<Button_game_friend_button>());
		EventCenter.Self.RegisterEvent("Button_invitation_friend_button", new DefineFactory<Button_invitation_friend_button>());
		EventCenter.Self.RegisterEvent("Button_platform_friend_button", new DefineFactory<Button_platform_friend_button>());
		
		EventCenter.Self.RegisterEvent("Button_find_friend_confirm_button", new DefineFactory<Button_find_friend_confirm_button>());
		EventCenter.Self.RegisterEvent("Button_agreed_button", new DefineFactory<Button_agreed_button>());
		EventCenter.Self.RegisterEvent("Button_refused_button", new DefineFactory<Button_refused_button>());
		EventCenter.Self.RegisterEvent("Button_request_friend_button", new DefineFactory<Button_request_friend_button>());
		EventCenter.Self.RegisterEvent("Button_visit_friend_button", new DefineFactory<Button_visit_friend_button>());
		EventCenter.Self.RegisterEvent("Button_praise_friend_button", new DefineFactory<Button_praise_friend_button>());
		EventCenter.Self.RegisterEvent("Button_delete_friend_button", new DefineFactory<Button_delete_friend_button>());
		EventCenter.Self.RegisterEvent("Button_praise_all_friend_button", new DefineFactory<Button_praise_all_friend_button>());

		DataCenter.OpenWindow ("FRIEND_WINDOW", true);
	}

	void OnDestroy()
	{
		DataCenter.CloseWindow ("BACK_GROUP_FRIEND_WINDOW");
		DataCenter.Remove("FRIEND_WINDOW");
	}
}

public class FriendWindow : tWindow
{
	public FriendWindow(GameObject obj)
	{
		mGameObjUI = obj;
	}

	public override void Init ()
	{
		DataCenter.Self.registerData ("ADD_FRIEND_WINDOW",  new AddFriendWindow(mGameObjUI));
        DataCenter.Self.registerData("GAME_FRIEND_WINDOW", new GameFriendWindow(mGameObjUI));
        DataCenter.Self.registerData("AQUIRE_SPIRIT_WINDOW", new AquireSpiritWindow(mGameObjUI));
		DataCenter.Self.registerData ("INVITATION_FRIEND_WINDOW",  new InvitationFriendWindow(mGameObjUI));
        DataCenter.Self.registerData("PLATFORM_FRIEND_WINDOW", new PlatformFriendWindow(mGameObjUI));
	}

	public override void Open (object param)
	{
		base.Open (param);

		CloseAllWindow ();
		DataCenter.OpenWindow ("GAME_FRIEND_WINDOW");
		IsShowNewRequestFriendIcon();
		DataCenter.OpenWindow ("BACK_GROUP_FRIEND_WINDOW");

        // 精力红点
        RoleLogicData RoleData = RoleLogicData.Self;
        GameObject buttonSpirit = GameCommon.FindObject(mGameObjUI, "aquire_spirit_button");
        GameObject markSpirit = GameCommon.FindObject(buttonSpirit, "new_request_friend_icon");
        markSpirit.SetActive(SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FRIEND_SPIRIT));
        // 添加好友红点
        GameObject buttonAddFriend = GameCommon.FindObject(mGameObjUI, "add_friend_button");
        GameObject markAddFriend = GameCommon.FindObject(buttonAddFriend, "new_request_friend_icon");
        markAddFriend.SetActive(SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FRIEND_REQUEST));
	}

	public override void Close ()
	{
		base.Close ();
		DataCenter.CloseWindow ("BACK_GROUP_FRIEND_WINDOW");
	}

	public void IsShowNewRequestFriendIcon()
	{
		int iRequestFriendNum = RoleLogicData.Self.mInviteNum;
		if(iRequestFriendNum == 0) GameCommon.SetUIVisiable (mGameObjUI, "new_request_friend_icon", false);
		else GameCommon.SetUIVisiable (mGameObjUI, "new_request_friend_icon", true);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		if(keyIndex == "NEW_REQUEST_FRIEND_ICON") IsShowNewRequestFriendIcon ();
	}
	
	void CloseAllWindow()
	{
		DataCenter.CloseWindow ("ADD_FRIEND_WINDOW");
		DataCenter.CloseWindow ("GAME_FRIEND_WINDOW");
		DataCenter.CloseWindow ("INVITATION_FRIEND_WINDOW");
		DataCenter.CloseWindow ("PLATFORM_FRIEND_WINDOW");
	}

	public override bool Refresh(object param)
	{
		base.Refresh (param);

		CloseAllWindow ();
		DataCenter.OpenWindow (param.ToString (), true);

		return true;
	}

	public override void onRemove()
	{
		base.onRemove ();
		DataCenter.Remove ("ADD_FRIEND_WINDOW");
		DataCenter.Remove ("GAME_FRIEND_WINDOW");
		DataCenter.Remove ("INVITATION_FRIEND_WINDOW");
		DataCenter.Remove ("PLATFORM_FRIEND_WINDOW");
	}
}

//----------------------------------------------------------------------------------
// FriendWindow
public class Button_friend_back : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
		return true;
	}
	
}

public class Button_add_friend_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("FRIEND_WINDOW","REFRESH","ADD_FRIEND_WINDOW");
		return true;
	}
}

public class Button_aquire_spirit_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("FRIEND_WINDOW", "REFRESH", "AQUIRE_SPIRIT_WINDOW");
        return true;
    }
}

public class Button_game_friend_button : CEvent
{
	public override bool _DoEvent()
	{
		tWindow gameFriendWindow = DataCenter.GetData ("GAME_FRIEND_WINDOW") as tWindow;
		if(gameFriendWindow.mGameObjUI.activeInHierarchy)
			DataCenter.SetData ("FRIEND_WINDOW","REFRESH","GAME_FRIEND_WINDOW");

		return true;
	}
}

public class Button_invitation_friend_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("FRIEND_WINDOW","REFRESH","INVITATION_FRIEND_WINDOW");
		return true;
	}
}

public class Button_platform_friend_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("FRIEND_WINDOW","REFRESH","PLATFORM_FRIEND_WINDOW");
		return true;
	}
}



