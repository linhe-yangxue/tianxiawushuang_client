using UnityEngine;
using System.Collections;
using Logic;

public class sPlayerInfo
{
    public sPlayerInfo()
    {

    }

    public sPlayerInfo(string uid, string tid, string name)
    {
        this.uid = uid;
        this.tid = tid;
        this.name = name;
    }

    public string uid;
    public string tid;
    public string name;
}

public class PlayerInfoWindow : tWindow {

    public static string str_player_info_data = "player_info";
    public static string str_root_name = "root_name";

    private UIImageButton mAddFriendButton = null;

    public enum buttonState
    {
        disalbe,
        enable,
        addf,
        deletef
    }

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_player_info_close_button", new DefineFactory<Button_player_info_close_button>());
        EventCenter.Self.RegisterEvent("Button_player_info_check_array_button", new DefineFactory<Button_player_info_check_array_button>());
        EventCenter.Self.RegisterEvent("Button_player_info_add_friend_button", new DefineFactory<Button_player_info_add_friend_button>());
        EventCenter.Self.RegisterEvent("Button_player_info_msg_button", new DefineFactory<Button_player_info_msg_button>());
        EventCenter.Self.RegisterEvent("Button_player_info_email_button", new DefineFactory<Button_player_info_email_button>());
    }

    protected override void OpenInit()
    {
        base.OpenInit();

        tWindow pWindow = DataCenter.GetData("PLAYER_INFO_WINDOW") as tWindow;
        if (pWindow == null) return;
        GameObject obj = pWindow.mGameObjUI;
        GameObject button = GameCommon.FindObject(obj, "player_info_add_friend_button");
        mAddFriendButton = button.GetComponent<UIImageButton>();

        // 暂时无邮件系统
        GameObject mailButton = GameCommon.FindObject(obj, "player_info_email_button");
        if(mailButton)
            mailButton.SetActive(false);
    }

    public override void Open(object param)
    {
        base.Open(param);

        NiceData data = param as NiceData;
        if (data == null) return;

        SetWinData(str_player_info_data, data.get(PlayerHeadShotScript.str_player_info).mObj);
        SetWinData(str_root_name, data.get(PlayerHeadShotScript.str_root_name).mObj);

        CS_RequestVisitPlayer visitPalyer = new CS_RequestVisitPlayer();
        sPlayerInfo playerInfo = data.get(PlayerHeadShotScript.str_player_info).mObj as sPlayerInfo;
        visitPalyer.targetId = playerInfo.uid;
        HttpModule.Instace.SendGameServerMessage(visitPalyer, "CS_VisitPlayer", OnRequestSuccess, OnRequestFailed);

        ChangeAddFriendButtonState(buttonState.disalbe);
        FriendNetEvent.RequestFriendList(OnRequestFriendListSuccess, OnRequestFriendListFailed);
    }

    public static Data GetWinData(string index)
    {
        PlayerInfoWindow window = DataCenter.GetData(UIWindowString.player_info_window) as PlayerInfoWindow;
        if (window == null) return Data.NULL;
        return window.get(index);
    }

    public static void SetWinData(string index, object val)
    {
        PlayerInfoWindow window = DataCenter.GetData(UIWindowString.player_info_window) as PlayerInfoWindow;
        if (window == null) return;
        window.set(index, val);
    }

    public override void OnClose()
    {
        ClearUIData();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        if (keyIndex == "AttachData")
        {
            SC_ResponeVisitPlayer data = objVal as SC_ResponeVisitPlayer;
            SetUIData(data);
        }
        else if (keyIndex == "ChangeButtonState")
        {
            buttonState value = (buttonState)objVal;
            ChangeAddFriendButtonState(value);
        }
    }

    public void SetUIData(SC_ResponeVisitPlayer data)
    {
        if (data == null) return;

        tWindow pWindow = DataCenter.GetData("PLAYER_INFO_WINDOW") as tWindow;
        if (pWindow == null) return;
        GameObject obj = pWindow.mGameObjUI;

        GameObject infolables = GameCommon.FindObject(obj, "info_labels");
        GameCommon.SetUIText(infolables, "name_label", data.name);
        GameCommon.SetUIText(infolables, "number_label", data.power.ToString());
        GameCommon.SetUIText(infolables, "union_name", data.guildName);
        GameCommon.SetUIText(infolables, "label", data.arr[0].level.ToString());

        int tid = data.arr[0].tid;
        string atlasName, spriteName;
        GameObject iconContainer = GameCommon.FindObject(infolables, "item_icon");
        UISprite spriteIcon = iconContainer.GetComponent<UISprite>();
        GameCommon.GetItemAtlasSpriteName(tid, out atlasName, out spriteName);
        GameCommon.SetIcon(spriteIcon, atlasName, spriteName);
    }

    public void ClearUIData()
    {
        tWindow pWindow = DataCenter.GetData("PLAYER_INFO_WINDOW") as tWindow;
        if (pWindow == null) return;
        GameObject obj = pWindow.mGameObjUI;    
        GameObject infolables = GameCommon.FindObject(obj, "info_labels");
        GameCommon.SetUIText(infolables, "name_label", "");
        GameCommon.SetUIText(infolables, "number_label", "");
        GameCommon.SetUIText(infolables, "union_name", "");
        GameCommon.SetUIText(infolables, "label", "");

        GameObject iconContainer = GameCommon.FindObject(infolables, "item_icon");
        UISprite spriteIcon = iconContainer.GetComponent<UISprite>();
        GameCommon.SetIcon(spriteIcon, "CommonUIAtlas", "a_ui_tubiao_1");
    }

    private void OnRequestSuccess(string text)
    {
        SC_ResponeVisitPlayer data = JCode.Decode<SC_ResponeVisitPlayer>(text);
        if (data == null) return;
        DataCenter.SetData("PLAYER_INFO_WINDOW", "AttachData", data);
    }

    private void OnRequestFailed(string text)
    {
        Debug.LogError("PlayerInfoWindow.Request failed with return -- " + text);
        DataCenter.OpenMessageTipsWindow(STRING_INDEX.ERROR_AQUIRE_PLAYER_INFO);
    }

    private void OnRequestFriendListSuccess(string text)
    {
        FriendLogicData data = JCode.Decode<FriendLogicData>(text);
        if (data == null) return;
        Data niceData = PlayerInfoWindow.GetWinData(str_player_info_data);
        sPlayerInfo selectPlayer = PlayerInfoWindow.GetWinData(PlayerInfoWindow.str_player_info_data).mObj as sPlayerInfo;
        if(selectPlayer == null) return;
        for (int i = 0; i < data.arr.Length; ++i)
        {
            FriendData d = data.arr[i];
            if (d != null && d.friendId == selectPlayer.uid)
            {
                ChangeAddFriendButtonState(buttonState.deletef);
            }
        }
        ChangeAddFriendButtonState(buttonState.addf);
    }

    private void OnRequestFriendListFailed(string text)
    {
        Debug.LogError("PlayerInfoWindow.Request failed with return -- " + text);
        DataCenter.OpenMessageTipsWindow(STRING_INDEX.ERROR_AQUIRE_FRIEND_LIST);
    }

    private void ChangeAddFriendButtonState(buttonState state)
    {
        if (mAddFriendButton == null) return;

        switch (state)
        {
            case buttonState.enable:
                mAddFriendButton.isEnabled = true;
                break;
            case buttonState.disalbe:
                mAddFriendButton.isEnabled = false;
                break;
            case buttonState.addf:
                ChangeAddFriendButtonState(buttonState.enable);
                UILabel lable = mAddFriendButton.GetComponentInChildren<UILabel>();
                lable.text = "添加好友";
                break;
            case buttonState.deletef:
                ChangeAddFriendButtonState(buttonState.enable);
                lable = mAddFriendButton.GetComponentInChildren<UILabel>();
                lable.text = "删除好友";
                break;
        }
    }
}
