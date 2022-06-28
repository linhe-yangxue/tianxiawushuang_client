using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Logic;
using Utilities;
using DataTable;

/// <summary>
/// 聊天类型
/// </summary>
public enum ChatType
{
    World,
    Union,
    Private
}

/// <summary>
/// 打开聊天窗口时传的数据
/// </summary>
public class ChatWindowOpenData
{
    private ChatType mType = ChatType.World;
    private string mTarget;
    private string mTargetName;
    private Action mCalback;

    public ChatType chatType
    {
        set { mType = value; }
        get { return mType; }
    }
    public string Target
    {
        set { mTarget = value; }
        get { return mTarget; }
    }
    public string TargetName
    {
        set { mTargetName = value; }
        get { return mTargetName; }
    }
    public Action Callback
    {
        set { mCalback = value; }
        get { return mCalback; }
    }

    public ChatWindowOpenData(ChatType chatType, string targetUid, string targetName, Action callBack)
    {
        mType = chatType;
        mTarget = targetUid;
        TargetName = targetName;
        mCalback = callBack;
    }

    public ChatWindowOpenData() { }
}

public class ChatWindow : tWindow
{
    public int mCurrentServerID = -1;
    public int mSpace = 10;
    public int mParagraphHistory = 50;

    public ChatType mChatType;
    public List<ChatRecord> mChatRecordList = new List<ChatRecord>();
    protected int mNextRecvChatId = 1;          //下次获取聊天信息时的开始索引
    protected long mRecieveChatDelta = 2;       //获取聊天信息的间隔，以秒为间隔
    protected bool mScrollBottom = false;       //是否滑到最底部

    public List<string> mAllKeyList = new List<string>();

    public string mTargetUid = null;
    public string mTargetName = "";

    private Action mCloseCallback;      //关闭窗口回调

    public override void Init()
    {
        EventCenter.Register("Button_chat_window_black_background_btn", new DefineFactory<Button_close_chat_window_button>());
        EventCenter.Register("Button_close_chat_window_button", new DefineFactory<Button_close_chat_window_button>());
        EventCenter.Register("Button_union_channel_button", new DefineFactory<Button_union_channel_button>());
        EventCenter.Register("Button_world_channel_button", new DefineFactory<Button_world_channel_button>());
        EventCenter.Register("Button_private_channel_button", new DefineFactory<Button_private_channel_button>());
        EventCenter.Register("Button_chat_send_button", new DefineFactory<Button_chat_send_button>());
        EventCenter.Register("Button_chat_icon", new DefineFactory<Button_chat_icon>());

    }

    public override void Open(object param)
    {
        mCurrentServerID = GetCurrentSeverID();
        base.Open(param);
        __OnOpen(param);
        __RecieveAllChat(false);
        //GameObject.Find("chat_window").transform.localPosition = new Vector3(0, 0, -100);
        if (mGameObjUI != null)
        {
            mGameObjUI.transform.localPosition = new Vector3(0, 0, -100);
        }
    }

    protected virtual void __OnOpen(object param)
    {
        ChatGrid grid = GetComponent<ChatGrid>("grid");
        if (grid != null)
            grid.ResetPosition();

        GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_CHAT_MARK", false);
        ChatNewMarkManager.Self.RefreshAllChatNewMark();

        HttpModule.CallBack requestSuccess = text =>
        {
            var item = JCode.Decode<SC_GetGuildId>(text);
            RoleLogicData.Self.guildId = item.guildId;
            bool tmpHasGuild = RoleLogicData.Self.guildId != "";
            if (CommonParam.isOnLineVersion)
            {
                _EnableToggle(GetSub("union_channel_button"), tmpHasGuild);
                if (!tmpHasGuild)
                {
                    GameObject _unionBtn = GameCommon.FindObject(mGameObjUI, "union_channel_button");
                    GameCommon.SetNewMarkVisible(_unionBtn, false);

                    ChatWindowOpenData _tmpOpenData = param as ChatWindowOpenData;
                    if (_tmpOpenData != null) 
                    {
                        if (ChatType.Union == _tmpOpenData.chatType) 
                        {
                            DataCenter.OpenWindow("CHAT_UNION_WINDOW", param);
                            GameCommon.ToggleTrue(GetSub("union_channel_button"));
                        }
                    }                 
                }
            }
            else
            {
                GetSub("union_channel_button").SetActive(tmpHasGuild);
            }
        };
        CS_GetGuildId cs = new CS_GetGuildId();
        HttpModule.Instace.SendGameServerMessageT<CS_GetGuildId>(cs, requestSuccess, NetManager.RequestFail);




        __SetScrollBottomMark(ChatType.World);
        __SetScrollBottomMark(ChatType.Union);
        __SetScrollBottomMark(ChatType.Private);

        mChatType = ChatType.Private;
        CloseAllWindow();
        //added by xuke
        //        mCloseCallback = null;
        //end
        string tmpToggleName = "world_channel_button";
        string tmpWinName = "CHAT_WORLD_WINDOW";
        ChatWindowOpenData tmpOpenData = param as ChatWindowOpenData;
        if (tmpOpenData != null)
        {
            mCloseCallback = tmpOpenData.Callback;
            switch (tmpOpenData.chatType)
            {
                case ChatType.World:
                    {
                        tmpToggleName = "world_channel_button";
                        tmpWinName = "CHAT_WORLD_WINDOW";
                    } break;
                case ChatType.Union:
                    {
                        tmpToggleName = "union_channel_button";
                        tmpWinName = "CHAT_UNION_WINDOW";
                    } break;
                case ChatType.Private:
                    {
                        tmpToggleName = "private_channel_button";
                        tmpWinName = "CHAT_PRIVATE_WINDOW";
                    } break;
            }
        }

        DataCenter.OpenWindow(tmpWinName, param);
        GameCommon.ToggleTrue(GetSub(tmpToggleName));

    }

    public void CloseAllWindow()
    {
        DataCenter.CloseWindow("CHAT_WORLD_WINDOW");
        DataCenter.CloseWindow("CHAT_UNION_WINDOW");
        DataCenter.CloseWindow("CHAT_PRIVATE_WINDOW");
    }

    /// <summary>
    /// 设置指定聊天窗口开启滑到最底部的标志
    /// </summary>
    /// <param name="chatType"></param>
    private void __SetScrollBottomMark(ChatType chatType)
    {
        ChatWindow tmpChatWin = null;
        switch (chatType)
        {
            case ChatType.World: tmpChatWin = DataCenter.GetData("CHAT_WORLD_WINDOW") as ChatWindow; break;
            case ChatType.Union: tmpChatWin = DataCenter.GetData("CHAT_UNION_WINDOW") as ChatWindow; break;
            case ChatType.Private: tmpChatWin = DataCenter.GetData("CHAT_PRIVATE_WINDOW") as ChatWindow; break;
        }
        if (tmpChatWin != null)
            tmpChatWin.mScrollBottom = true;
    }

    public override bool Refresh(object param)
    {
        SetChatSendButtonData((int)mChatType);

        ChatGrid grid = GetComponent<ChatGrid>("grid");
        int tmpCount = mChatRecordList.Count;
        grid.MaxCount = tmpCount;
        for (int i = 0; i < tmpCount; i++)
        {
            ChatRecord chatInfo = mChatRecordList[i];
            bool bIsMySelf = (chatInfo.SrcName == RoleLogicData.Self.name);
            GameCommon.SetUIVisiable(grid.itemList[i], "left", !bIsMySelf);
            GameCommon.SetUIVisiable(grid.itemList[i], "right", bIsMySelf);
            string str = bIsMySelf ? "right" : "left";

            GameObject parentObj = GameCommon.FindObject(grid.itemList[i], str);
            UILabel chatInfoLabel = GameCommon.FindObject(parentObj, "chat_info").GetComponent<UILabel>();
            UILabel nameLabel = GameCommon.FindObject(parentObj, "name").GetComponent<UILabel>();
            UISprite background = GameCommon.FindObject(parentObj, "background").GetComponent<UISprite>();
            UISprite icon = GameCommon.FindObject(parentObj, "chat_icon").GetComponent<UISprite>();
            SetChatIconButtonData(parentObj, (int)mChatType, chatInfo);

            chatInfo.ChatInfo = BanedText(chatInfo.ChatInfo);
            int originalHeight = chatInfoLabel.height;
            chatInfoLabel.text = chatInfo.ChatInfo;
            if (chatInfoLabel.width > 600)
            {
                chatInfoLabel.overflowMethod = UILabel.Overflow.ResizeHeight;
                chatInfoLabel.width = 600;
            }

            if (mChatType == ChatType.Private)
            {
                nameLabel.text = bIsMySelf ?
                    "[9900ff]你对[-] " + "[99ff00]" + chatInfo.TargetName + "[-]" + "[9900ff]说:[-]" : "[99ff00]" + chatInfo.SrcName + "[-]" + " [9900ff]对你说:[-]";
            }
            else
            {
                string strColor = bIsMySelf ? "[00ccff]" : "[99ff00]";
                nameLabel.text = strColor + chatInfo.SrcName;
            }

            if (bIsMySelf)
                chatInfoLabel.pivot = chatInfoLabel.localSize.y >= 2 * chatInfoLabel.fontSize ? UIWidget.Pivot.TopLeft : UIWidget.Pivot.TopRight;

            background.height += ((int)chatInfoLabel.localSize.y - originalHeight);
            background.width = (chatInfoLabel.width > nameLabel.width ? chatInfoLabel.width : nameLabel.width) + 50;

            GameCommon.SetPalyerIcon(icon, chatInfo.SrcHeadIconId);
        }

        grid.ResetPosition();
        if (IsOpen() && mScrollBottom)
        {
            mScrollBottom = false;
            this.StartCoroutine(ChangeScrollValue());
        }

        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "SHOW_WINDOW":
                CloseAllWindow();
                DataCenter.OpenWindow(objVal.ToString());
                break;
            case "SEND_INPUT_CHAT":
                string strInputValue = objVal.ToString();
                SendInputValueChat(strInputValue);
                break;
            case "SEND_SUCCESS":
                {
                    //如果是世界聊天，减去相应剩余聊天次数
                    if (mChatType == ChatType.World)
                        DataCenter.SetData("CHAT_WORLD_WINDOW", "REDUCE_COUNT_THEN_MAY_COST", 1);
                    CleanInputValue();
                    __RecieveAllChat(false);
                } break;
            case "RECIEVE_CHAT":
                {
                    _RecvChat(objVal);
                    __RecieveAllChat(true);
                } break;
            case "DELET_ALL_CHAT_LOG":
                DeletAllChatLog();
                break;
            case "CLEAN_ALL_CHAT":
                CleanAllChat();
                break;
            case "CLOSE_CALLBACK":
                {
                    if (mCloseCallback != null)
                    {
                        mCloseCallback();
                        mCloseCallback = null;
                    }

                } break;
            case "REFRESH_ALL_CHAT_NEWMARK":
                RefreshAllChatNewMark();
                break;
        }
    }

    private void RefreshAllChatNewMark() 
    {
        //世界
        GameObject _worldBtn = GameCommon.FindObject(mGameObjUI, "world_channel_button");
        GameCommon.SetNewMarkVisible(_worldBtn,ChatNewMarkManager.Self.WorldChatVisible);
        //私聊
        GameObject _privateBtn = GameCommon.FindObject(mGameObjUI, "private_channel_button");
        GameCommon.SetNewMarkVisible(_privateBtn,ChatNewMarkManager.Self.PrivateChatVisible);
        //宗门
        GameObject _unionBtn = GameCommon.FindObject(mGameObjUI, "union_channel_button");
        GameCommon.SetNewMarkVisible(_unionBtn,ChatNewMarkManager.Self.UnionChatVisible);
    }

    public static bool HasNewMessage(int currRecvChatId, ChatType type)
    {
        bool bHasNew = false;
        switch (type)
        {
            case ChatType.World:
                {
                    //世界聊天
                    ChatWorldWindow tmpWorldWin = DataCenter.GetData("CHAT_WORLD_WINDOW") as ChatWorldWindow;
                    bHasNew = (tmpWorldWin.mNextRecvChatId != currRecvChatId);
                } break;
            case ChatType.Union:
                {
                    ChatUnionWindow tmpUnionWin = DataCenter.GetData("CHAT_UNION_WINDOW") as ChatUnionWindow;
                    tmpUnionWin.CheckAndSetUnionChanged();
                    bHasNew = (tmpUnionWin.mNextRecvChatId != currRecvChatId);
                } break;
            case ChatType.Private:
                {
                    ChatPrivateWindow tmpPrivateWin = DataCenter.GetData("CHAT_PRIVATE_WINDOW") as ChatPrivateWindow;
                    bHasNew = (tmpPrivateWin.mNextRecvChatId != currRecvChatId);
                } break;
        }
        return bHasNew;
    }

    string BanedText(string originalText)
    {
        foreach (DataRecord r in DataCenter.mBanedTextConfig.Records())
        {
            string strBanedText = r["BANEDLIST"];
            if (strBanedText != "" && originalText.Contains(strBanedText))
            {
                originalText = originalText.Replace(strBanedText, "***");
            }
        }
        return originalText;
    }

    /// <summary>
    /// 因为改变滚动条时，聊天记录可能没加如列表中，所以延迟滚动到底部
    /// </summary>
    /// <returns></returns>
    IEnumerator ChangeScrollValue()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.2f);
        GetSub("scroll_bars").GetComponent<UIScrollBar>().value = 1.0f;
    }

    public virtual void SendInputValueChat(string strInputValue)
    {
        switch (mChatType)
        {
            case ChatType.World:
                {
                    GlobalModule.DoCoroutine(Chat_SendWorldChat_Requester.StartRequest(strInputValue));
                } break;
            case ChatType.Union:
                {
                    GlobalModule.DoCoroutine(Chat_SendGuildChat_Requester.StartRequest(strInputValue));
                } break;
            case ChatType.Private:
                {
                    if (mTargetUid == null)
                        DataCenter.OpenMessageWindow("无效的聊天目标");
                    else
                        GlobalModule.DoCoroutine(Chat_SendPrivateChat_Requester.StartRequest(strInputValue, mTargetUid));
                } break;
        }
    }

    protected void _RecvChat(object param)
    {
        RespMessage tmpResp = param as RespMessage;
        ChatRecord[] tmpChatRecords = null;

        switch (tmpResp.pt)
        {
            case "SC_RcvPrivateChat":
                {
                    SC_RcvPrivateChat tmpPrivateChat = tmpResp as SC_RcvPrivateChat;
                    mNextRecvChatId = tmpPrivateChat.rcdIndex;
                    ChatPrivateWindow.SaveNextRecvChatID();
                    tmpChatRecords = tmpPrivateChat.arr;
                } break;
            case "SC_RcvGuildChat":
                {
                    SC_RcvGuildChat tmpGuildChat = tmpResp as SC_RcvGuildChat;
                    mNextRecvChatId = tmpGuildChat.rcdIndex;
                    ChatUnionWindow.SaveNextRecvChatID();
                    tmpChatRecords = tmpGuildChat.arr;
                } break;
            case "SC_RcvWorldChat":
                {
                    SC_RcvWorldChat tmpWorldChat = tmpResp as SC_RcvWorldChat;
                    mNextRecvChatId = tmpWorldChat.rcdIndex;
                    ChatWorldWindow.SaveNextRecvChatID();
                    tmpChatRecords = tmpWorldChat.arr;
                } break;
        }
        if (tmpChatRecords != null)
        {
            bool tmpIsSendFromMyself = false;
            int tmpCount = tmpChatRecords.Length;
            for (int i = 0; i < tmpCount; i++)
            {
                AddChatRecord(tmpChatRecords[i]);
                if (!tmpIsSendFromMyself)
                    tmpIsSendFromMyself = (tmpChatRecords[i].SrcName == RoleLogicData.Self.name);
            }
            if (tmpCount > 0)
            {
                //检查是否为自己发的消息
                if (!tmpIsSendFromMyself)
                {
                    if (GameCommon.bIsWindowOpen("CHAT_WINDOW"))
                        Refresh(null);
                    else
                        GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_CHAT_MARK", true);
                }
                else
                {
                    CleanInputValue();
                    Refresh(null);
                }
            }
        }
    }

    /// <summary>
    /// 请求聊天数据
    /// </summary>
    private void __RecieveAllChat(bool isDelay)
    {
        CountdownNoUI tmpCountdown = mGameObjUI.GetComponent<CountdownNoUI>();
        if (tmpCountdown == null)
            tmpCountdown = __SetCountdown(mGameObjUI, 0, new CallBack(this, "OnRecieveAllChat", tmpCountdown));
        long tmpNowTime = CommonParam.NowServerTime();
        if (isDelay)
            tmpNowTime += mRecieveChatDelta;
        tmpCountdown.mServerOverTime = tmpNowTime;
        tmpCountdown.enabled = true;
    }
    public virtual void OnRecieveAllChat(object param)
    {
    }

    public virtual void SetChatSendButtonData(int type)
    {
        GameCommon.GetButtonData(GetSub("chat_send_button")).set("INPUT_OBJ", GetSub("chat_input"));
        GameCommon.GetButtonData(GetSub("chat_send_button")).set("INPUT_LABEL_OBJ", GetSub("input_label"));
        GameCommon.GetButtonData(GetSub("chat_send_button")).set("CHAT_TYPE", type);
    }

    public virtual void SetChatIconButtonData(GameObject parentObj, int type, ChatRecord chatInfo)
    {
        NiceData buttonData = GameCommon.GetButtonData(GameCommon.FindObject(parentObj, "chat_icon"));
        if (buttonData == null)
            return;

        buttonData.set("SRC_NAME", chatInfo.SrcName);
        buttonData.set("SRC_ID", chatInfo.SrcUid);
        buttonData.set("TYPE", type);
    }

    public void AddChatRecord(ChatRecord info)
    {
        //有新消息
        mScrollBottom = true;
        mChatRecordList.Add(info);
        if (mChatRecordList.Count > mParagraphHistory)
            mChatRecordList.RemoveAt(0);
    }

    public void CleanInputValue()
    {
        GetSub("chat_input").GetComponent<UIInput>().value = "";
    }

    public bool SaveChatLog()
    {
        string key = mTargetName + "_CHAT_LOG" + mCurrentServerID.ToString() + RoleLogicData.Self.name;
        if (GamePrefs.Set<List<ChatRecord>>(key, mChatRecordList) && !mAllKeyList.Contains(key))
        {
            mAllKeyList.Add(key);
            return GamePrefs.Set<List<string>>("ALL_CHAT_LOG_KEY", mAllKeyList);
        }

        return false;
    }

    public bool GetChatLog()
    {
        string key = mTargetName + "_CHAT_LOG" + mCurrentServerID.ToString() + RoleLogicData.Self.name;
        if (!GamePrefs.TryGet<List<ChatRecord>>(key, out mChatRecordList))
            mChatRecordList = new List<ChatRecord>();

        if (!GamePrefs.TryGet<List<string>>("ALL_CHAT_LOG_KEY", out mAllKeyList))
            mAllKeyList = new List<string>();

        return true;
    }

    public void DeletChatLog()
    {
        string key = mTargetName + "_CHAT_LOG" + mCurrentServerID.ToString() + RoleLogicData.Self.name;
        if (GamePrefs.HasKey(key))
            GamePrefs.DeleteKey(key);
    }

    public void DeletAllChatLog()
    {
        foreach (string s in mAllKeyList)
        {
            if (GamePrefs.HasKey(s))
                GamePrefs.DeleteKey(s);
        }
    }

    public void CleanAllChat()
    {
        mChatRecordList.Clear();
        if (mChatType == ChatType.World)
            mTargetName = "WORLD";
        else if (mChatType == ChatType.Union)
            mTargetName = "UNION";
        else if (mChatType == ChatType.Private)
            mTargetName = "";
    }

    public int GetCurrentSeverID()
    {
        string strFileName = GameCommon.MakeGamePathFileName("server_id.red");
        int serverID = -1;
        if (System.IO.File.Exists(strFileName))
        {
            System.IO.FileStream f = new System.IO.FileStream(strFileName, System.IO.FileMode.Open);
            DataBuffer data = new DataBuffer(4);
            f.Read(data.mData, 0, 4);
            data.read(out serverID);
            f.Close();
        }

        return serverID;
    }

    private CountdownNoUI __SetCountdown(GameObject subObject, long serverOverTime, CallBack callBack)
    {
        CountdownNoUI countDown = subObject.AddComponent<CountdownNoUI>();
        countDown.mServerOverTime = serverOverTime;
        countDown.mFinishCallBack = callBack;
        return countDown;
    }

    protected void _EnableToggle(GameObject goToggle, bool isEnable, string activeName = "CheckMark", string inActiveName = "bg")
    {
//        goToggle.GetComponent<BoxCollider>().enabled = isEnable;
        UIButtonEvent tmpEvt = goToggle.GetComponent<UIButtonEvent>();
        if(tmpEvt != null)
        {
            if (isEnable)
                tmpEvt.AddAction(null);
            else
                tmpEvt.AddAction(() => { });
        }
        UIToggle tmpToggle = goToggle.GetComponent<UIToggle>();
        if (tmpToggle != null)
            tmpToggle.enabled = isEnable;

        Color tmpColor = isEnable ? Color.white : Color.grey;

        GameObject _inActiveObj = GameCommon.FindObject(goToggle,inActiveName);
        UISprite _inActiveSprite = _inActiveObj.GetComponent<UISprite>();
        if (_inActiveSprite != null) 
        {
            _inActiveSprite.color = tmpColor;
        }
        foreach (Transform tmpTrans in _inActiveObj.transform)
        {
            GameObject tmpGO = tmpTrans.gameObject;

            UISprite tmpSprite = tmpGO.GetComponent<UISprite>();
            if (tmpSprite != null)
                tmpSprite.color = tmpColor;

            UILabel tmpLabel = tmpGO.GetComponent<UILabel>();
            if (tmpLabel != null)
                tmpLabel.color = tmpColor;
        }

        GameObject _activeObj = GameCommon.FindObject(goToggle, activeName);
        if (_activeObj != null) 
        {
            UISprite _activeSprite = _activeObj.GetComponent<UISprite>();
            if (_activeSprite != null) 
            {
                Color _tColor = Color.white;
                _tColor.a = 0;
                _activeSprite.color = _tColor;
            }
        }
    }
}


public class Button_world_channel_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("CHAT_WINDOW", "SHOW_WINDOW", "CHAT_WORLD_WINDOW");
        return true;
    }
}

public class Button_union_channel_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("CHAT_WINDOW", "SHOW_WINDOW", "CHAT_UNION_WINDOW");
        return true;
    }
}

public class Button_private_channel_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("CHAT_WINDOW", "SHOW_WINDOW", "CHAT_PRIVATE_WINDOW");
        return true;
    }
}

public class Button_chat_send_button : CEvent
{
    protected List<string> mListBannedText = new List<string>()
    {
        "\'",
        "\""
    };

    GameObject selfObj;
    public override bool _DoEvent()
    {
        GameObject inputObj = getObject("INPUT_OBJ") as GameObject;
        string inputValue = inputObj.GetComponent<UIInput>().value;
        if (inputValue == "")
        {
            DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_CHAT_NEED_INPUT_VALUE);
            return false;
        }

        string[] strs = inputValue.Split('\n');
        if (strs.Length >= 3)
            inputValue = inputValue.Substring(0, strs[0].Length + strs[1].Length + strs[2].Length + 2);

        int type = (int)getObject("CHAT_TYPE");
        string windowName = "CHAT_WINDOW";
        switch (type)
        {
            case (int)ChatType.World:
                int freeCount = (int)getObject("FREE_COUNT");
                if (freeCount <= 0)
                {
                    int costCount = (int)getObject("COST_COUNT");
                    if (RoleLogicData.Self.diamond < costCount)
                    {
                        DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_NO_ENOUGH_DIAMOND);
                        return false;
                    }
                    //                     else
                    //                         GameCommon.RoleChangeDiamond(-costCount);
                }
                windowName = "CHAT_WORLD_WINDOW";
                break;
            case (int)ChatType.Union:
                windowName = "CHAT_UNION_WINDOW";
                break;
            case (int)ChatType.Private:
                ChatPrivateWindow priWin = DataCenter.GetData("CHAT_PRIVATE_WINDOW") as ChatPrivateWindow;
                if (priWin == null || priWin.mTargetName == "")
                {
                    DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_CHAT_NEED_SELECT_FRIEND);
                    return false;
                }
                windowName = "CHAT_PRIVATE_WINDOW";
                break;
        }
        //去除特定字符
        for (int i = 0, coutn = mListBannedText.Count; i < coutn; i++)
            inputValue = inputValue.Replace(mListBannedText[i], "");
        DataCenter.SetData(windowName, "SEND_INPUT_CHAT", inputValue);

        ClickButtonCD(type);
        return true;
    }

    void ClickButtonCD(int type)
    {
        selfObj = getObject("BUTTON") as GameObject;
        selfObj.GetComponent<UIImageButton>().isEnabled = false;
        selfObj.GetComponent<BoxCollider>().enabled = false;
        GuideManager.ExecuteDelayed(() => ButtonCanClick(), 3.0f);
    }

    void ButtonCanClick()
    {
        selfObj.GetComponent<UIImageButton>().isEnabled = true;
        selfObj.GetComponent<BoxCollider>().enabled = true;
    }
}

public class Button_ChatBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("CHAT_WINDOW");
        return true;
    }
}

public class Button_close_chat_window_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("CHAT_WINDOW");
        DataCenter.SetData("CHAT_WINDOW", "CLOSE_CALLBACK", null);
        return true;
    }
}

public class ChatIconData
{
    public string targetName;
    public int targetId;
}

public class Button_chat_icon : CEvent
{
    public override bool _DoEvent()
    {
        if (getObject("SRC_NAME").ToString() == RoleLogicData.Self.name)
            return false;

        int type = (int)getObject("TYPE");
        ChatIconData iconData = new ChatIconData();
        iconData.targetName = getObject("SRC_NAME").ToString();
        iconData.targetId = (int)getObject("SRC_ID");
        switch (type)
        {
            case (int)ChatType.World:
                break;
            case (int)ChatType.Private:
                DataCenter.SetData("CHAT_PRIVATE_WINDOW", "SELECT_BY_ICON", iconData);
                break;
            case (int)ChatType.Union:
                break;
        }
        return true;
    }
}
