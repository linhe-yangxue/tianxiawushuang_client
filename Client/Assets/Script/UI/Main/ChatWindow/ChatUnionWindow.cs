using UnityEngine;
using System.Collections;
using Utilities;

public class ChatUnionWindow : ChatWindow
{
    private string mLastGuildId = "";      //上一次公会Id

    public override void Open(object param)
    {
        base.Open(param);
        GameCommon.FindComponent<UIInput>(mGameObjUI, "chat_input").characterLimit = CommonParam.chatTextMaxNum;
    }

    public static string GetNextRecvChatIDKey()
    {
        return "CHAT_UNION_WINDOW_NEXT_RECV_CHAT_ID_" + RoleLogicData.Self.guildId + "_" + CommonParam.mUId + "_" + CommonParam.mZoneID + "_" + CommonParam.LoginIP + ":" + CommonParam.LoginPort;
    }
    public static void SaveNextRecvChatID()
    {
        ChatUnionWindow tmpUnionWin = DataCenter.GetData("CHAT_UNION_WINDOW") as ChatUnionWindow;
        if (tmpUnionWin != null)
            PlayerPrefs.SetInt(GetNextRecvChatIDKey(), tmpUnionWin.mNextRecvChatId);

        ChatNewMarkManager.Self.SetChatStateByType(ChatType.Union, false);
        ChatNewMarkManager.Self.RefreshAllChatNewMark();
    }

    public static void InitData()
    {
        ChatUnionWindow tmpUnionWin = DataCenter.GetData("CHAT_UNION_WINDOW") as ChatUnionWindow;
        if (tmpUnionWin != null)
        {
            string tmpIDKey = GetNextRecvChatIDKey();
            if (PlayerPrefs.HasKey(tmpIDKey))
                tmpUnionWin.mNextRecvChatId = PlayerPrefs.GetInt(tmpIDKey);
        }
    }

    protected override void __OnOpen(object param)
    {
		GameCommon.ToggleTrue (GameCommon.FindObject (mGameObjUI.transform.parent.gameObject, "union_channel_button"));
		mChatType = ChatType.Union;
		mTargetName = "UNION";

        if (mLastGuildId != RoleLogicData.Self.guildId)
        {
            CleanAllChat();
            mNextRecvChatId = 1;
            SaveNextRecvChatID();
        }
        mLastGuildId = RoleLogicData.Self.guildId;

		Refresh (null);
	}

    public override void OnRecieveAllChat(object param)
    {
        this.DoCoroutine(Chat_RcvGuildChat_Requester.StartRequest(mNextRecvChatId));
    }

    public bool CheckAndSetUnionChanged()
    {
        if (mLastGuildId == "")
        {
            mLastGuildId = RoleLogicData.Self.guildId;
            return false;
        }
        bool tmpIsChanged = (mLastGuildId != RoleLogicData.Self.guildId);
        if (tmpIsChanged)
        {
            CleanAllChat();
            mLastGuildId = RoleLogicData.Self.guildId;
            mNextRecvChatId = 1;
            SaveNextRecvChatID();
        }
        return tmpIsChanged;
    }
}
