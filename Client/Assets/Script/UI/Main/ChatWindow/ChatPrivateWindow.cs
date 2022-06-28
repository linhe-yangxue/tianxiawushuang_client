using UnityEngine;
using System.Collections;
using Logic;
using Utilities;

public class ChatPrivateWindow : ChatWindow
{
    public override void Open(object param)
    {
        base.Open(param);
        GameCommon.FindComponent<UIInput>(mGameObjUI, "chat_input").characterLimit = CommonParam.chatTextMaxNum;
    }
    public static string GetNextRecvChatIDKey()
    {
        return "CHAT_PRIVATE_WINDOW_NEXT_RECV_CHAT_ID_" + CommonParam.mUId + "_" + CommonParam.mZoneID + "_" + CommonParam.LoginIP + ":" + CommonParam.LoginPort;
    }
    public static void SaveNextRecvChatID()
    {
        ChatPrivateWindow tmpPrivateWin = DataCenter.GetData("CHAT_PRIVATE_WINDOW") as ChatPrivateWindow;
        if (tmpPrivateWin != null)
            PlayerPrefs.SetInt(GetNextRecvChatIDKey(), tmpPrivateWin.mNextRecvChatId);

        ChatNewMarkManager.Self.SetChatStateByType(ChatType.Private, false);
        ChatNewMarkManager.Self.RefreshAllChatNewMark();
    }

    public static void InitData()
    {
        ChatPrivateWindow tmpPrivateWin = DataCenter.GetData("CHAT_PRIVATE_WINDOW") as ChatPrivateWindow;
        if (tmpPrivateWin != null)
        {
            string tmpIDKey = GetNextRecvChatIDKey();
            if (PlayerPrefs.HasKey(tmpIDKey))
                tmpPrivateWin.mNextRecvChatId = PlayerPrefs.GetInt(tmpIDKey);
        }
    }

    public override void Init()
    {
        EventCenter.Register("Button_select_chat_target_button", new DefineFactory<Button_select_chat_target_button>());
        EventCenter.Register("Button_replace_chat_target_button", new DefineFactory<Button_replace_chat_target_button>());
    }

    public override void Close()
    {
        DataCenter.CloseWindow("PRIVATE_CHAT_FRIEND_WINDOW");
        base.Close();
    }

    protected override void __OnOpen(object param)
    {
        GameCommon.ToggleTrue(GameCommon.FindObject(mGameObjUI.transform.parent.gameObject, "private_channel_button"));
        mChatType = ChatType.Private;
        if (param != null && param is ChatWindowOpenData)
        {
            ChatWindowOpenData tmpData = param as ChatWindowOpenData;
            mTargetUid = tmpData.Target.ToString();
            mTargetName = tmpData.TargetName;
        }
        else
        {
            mTargetUid = get("TARGET_ID").ToString();
            mTargetName = get("TARGET_NAME");
        }
        Refresh(null);
    }

    public override bool Refresh(object param)
    {
        SetTargetName();
        ShowButton(mTargetName == "" ? true : false);
        return base.Refresh(param);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if (keyIndex == "SELECT_OR_REPLACE_TARGET")
        {
            DataCenter.OpenWindow("PRIVATE_CHAT_FRIEND_WINDOW", mTargetName);
            ShowButton((bool)objVal);
        }
        else if (keyIndex == "SELECT_BY_ICON")
        {
            ChatIconData iconData = objVal as ChatIconData;
            mTargetName = iconData.targetName;
            mTargetUid = iconData.targetId.ToString();
            SetTargetName();
            ShowButton(mTargetName == "" ? true : false);
        }
    }

    public override void OnRecieveAllChat(object param)
    {
        this.DoCoroutine(Chat_RcvPrivateChat_Requester.StartRequest(mNextRecvChatId));
    }

    public override void SendInputValueChat(string strInputValue)
    {
        if (mTargetName == "")
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_CHAT_NEED_SELECT_FRIEND, true);
        else
            base.SendInputValueChat(strInputValue);
    }

    void SetTargetName()
    {
        string strColor = mTargetName == "" ? "" : "[96795e]";
        SetText("target_name", "[96795e]" + mTargetName);
    }

    void ShowButton(bool bVisible)
    {
        SetVisible("select_chat_target_button", bVisible);
        SetVisible("replace_chat_target_button", !bVisible);
    }
}

public class Button_select_chat_target_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("CHAT_PRIVATE_WINDOW", "SELECT_OR_REPLACE_TARGET", false);
        return true;
    }
}

public class Button_replace_chat_target_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("CHAT_PRIVATE_WINDOW", "SELECT_OR_REPLACE_TARGET", false);
        return true;
    }
}
