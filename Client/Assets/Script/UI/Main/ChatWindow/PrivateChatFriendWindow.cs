using UnityEngine;
using System.Collections;
using Logic;

public class PrivateChatFriendWindow : tWindow
{
	public string mTargetName;
	public string mTargetUid;
	public override void Init ()
	{
		EventCenter.Register ("Button_friend_chat_select_button", new DefineFactory<Button_friend_chat_select_button>());
		EventCenter.Register ("Button_friend_chat_cancel_button", new DefineFactory<Button_friend_chat_cancel_button>());
		EventCenter.Register ("Button_friend_chat_sure_button", new DefineFactory<Button_friend_chat_sure_button>());
        EventCenter.Register("Button_private_chat_friend_back_button", new DefineFactory<Button_friend_chat_x>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		mTargetName = param.ToString ();
		DataCenter.CloseWindow ("CHAT_WINDOW");
	}
	
	public override void OnOpen ()
	{
		FriendNetEvent.RequestFriendList(FriendListSuccess, FriendListFail);
	}

	void FriendListSuccess(string text)
	{
		/*
		GuideManager.Notify(GuideIndex.EnterWorldMap);
		GuideManager.Notify(GuideIndex.EnterWorldMap2);
		GuideManager.Notify(GuideIndex.EnterWorldMap3);
		
		FriendLogicData logicData = new FriendLogicData();
		logicData = JCode.Decode<FriendLogicData>(text);
		DataCenter.RegisterData("GAME_FRIEND_LIST", logicData);
		
		DataCenter.SetData ("GAME_FRIEND_WINDOW", "CLEAN_ALL_UI", true);
		DataCenter.SetData ("GAME_FRIEND_WINDOW", "REFRESH", true);
		
		FriendNetEvent.RequestGetSpiritSendList(GetSpiritSendListSuccess, GetSpiritSendListFail);
		*/
		FriendLogicData logicData = new FriendLogicData();
		logicData = JCode.Decode<FriendLogicData>(text);
		DataCenter.RegisterData("GAME_FRIEND_LIST", logicData);

		Refresh(true);

		DEBUG.Log("Friend:" + text);
	}
	
	void FriendListFail(string text) {
		DEBUG.Log("FriendErr:" + text);
	}

	public override void onChange (string keyIndex, object objVal)
	{
		if(keyIndex == "SELECT_OR_CANCEL_FRIEND")
		{
			FriendData data = objVal as FriendData;
			if(data != null)
			{
				mTargetName = data.name;
				mTargetUid = data.friendId;
			}
			else
			{
				mTargetName = "";
				mTargetUid = null;
			}
			Refresh (null);
		}
		else if(keyIndex == "SURE_SELECT")
		{
			DataCenter.SetData ("CHAT_PRIVATE_WINDOW", "TARGET_NAME", mTargetName);
			DataCenter.SetData ("CHAT_PRIVATE_WINDOW", "TARGET_ID", mTargetUid);
			DataCenter.OpenWindow ("CHAT_WINDOW");
			DataCenter.SetData ("CHAT_WINDOW", "SHOW_WINDOW", "CHAT_PRIVATE_WINDOW");
		}
        else if(keyIndex == "CANCLE_SELECT")
        {
			DataCenter.OpenWindow ("CHAT_WINDOW");
			DataCenter.SetData ("CHAT_WINDOW", "SHOW_WINDOW", "CHAT_PRIVATE_WINDOW");
        }
		base.onChange (keyIndex, objVal);
	}

	public override bool Refresh (object param)
	{
		//获取存于DataCenter的GAME_FRIEND_LIST数据，更新好友选择界面
		FriendLogicData friendLogicData = DataCenter.GetData("GAME_FRIEND_LIST") as FriendLogicData;
		if(friendLogicData != null)
		{
			FriendData[] friendList = friendLogicData.arr;
			int count = friendList.Length;

			UIGridContainer grid = GetSub ("grid").GetComponent<UIGridContainer>();
			grid.MaxCount = count;
			for(int i=0; i < count; i++)
			{
				GameObject subcell = grid.controlList[i];
				FriendData tmpFriendData = friendList[i];

                //玩家信息
                GameObject tmpGOFriendInfo = GameCommon.FindObject(subcell, "friend_info");
                //名称
                GameCommon.SetUIText(tmpGOFriendInfo, "friend_name", tmpFriendData.name);
                //头像
               //GameCommon.SetPalyerIcon(GameCommon.FindComponent<UISprite>(tmpGOFriendInfo, "friend_icon"), tmpFriendData.icon);
                GameCommon.SetItemIconNew(tmpGOFriendInfo, "friend_icon", tmpFriendData.icon);

                //上阵宠物
                GameObject tmpGOPet = GameCommon.FindObject(subcell, "pet_icon");
                //TODO 上阵宠物
				
				//检查是否为主动打开
				bool bOpen = false;
				if(param != null)
					bOpen = (bool)param;
				if((bOpen && mTargetName == "" && i == 0) || (mTargetName == tmpFriendData.name))
				{
					mTargetName = tmpFriendData.name;
					mTargetUid = tmpFriendData.friendId;
				}

                //选择按钮数据
                NiceData tmpBtnSelectData = GameCommon.GetButtonData(subcell, "friend_chat_select_button");
                if (tmpBtnSelectData != null)
                    tmpBtnSelectData.set("FRIEND_DATA", tmpFriendData);
			}

		}
		return true;
	}

	void ShowSelectButton(GameObject obj, bool bVisible)
	{
		GameCommon.SetUIVisiable (obj, "friend_chat_select_button", bVisible);
		GameCommon.SetUIVisiable (obj, "friend_chat_cancel_button", !bVisible);
		GameCommon.SetUIVisiable(obj, "selected_background", false);
	}
}

public class Button_friend_chat_select_button : CEvent
{
    public override bool _DoEvent()
    {
        FriendData tmpFriendData = getObject("FRIEND_DATA") as FriendData;
        DataCenter.SetData("PRIVATE_CHAT_FRIEND_WINDOW", "SELECT_OR_CANCEL_FRIEND", tmpFriendData);
        DataCenter.SetData("PRIVATE_CHAT_FRIEND_WINDOW", "SURE_SELECT", true);
        return true;
    }
}

public class Button_friend_chat_cancel_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("PRIVATE_CHAT_FRIEND_WINDOW", "SELECT_OR_CANCEL_FRIEND", null);
		return true;
	}
}

public class Button_friend_chat_sure_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("PRIVATE_CHAT_FRIEND_WINDOW", "SURE_SELECT", true);
		return true;
	}
}

public class Button_friend_chat_x : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PRIVATE_CHAT_FRIEND_WINDOW", "CANCLE_SELECT", true);
        return true;
    }
}
