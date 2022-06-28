using UnityEngine;
using System.Collections;
using Utilities;

public class ChatWorldWindow : ChatWindow
{
    public int mWorldChatFreeCount = 10;
    public int mCostCount = 5;
	private UIScrollView m_ChatWorldScrollView = null;

    public override void Open(object param)
    {
        base.Open(param);
        GameCommon.FindComponent<UIInput>(mGameObjUI, "chat_input").characterLimit = CommonParam.chatTextMaxNum;
    }

    public static string GetNextRecvChatIDKey()
    {
        return "CHAT_WORLD_WINDOW_NEXT_RECV_CHAT_ID_" + CommonParam.mUId + "_" + CommonParam.mZoneID + "_" + CommonParam.LoginIP + ":" + CommonParam.LoginPort;
    }
    public static void SaveNextRecvChatID()
    {
        ChatWorldWindow tmpWorldWin = DataCenter.GetData("CHAT_WORLD_WINDOW") as ChatWorldWindow;
        if (tmpWorldWin != null)
            PlayerPrefs.SetInt(GetNextRecvChatIDKey(), tmpWorldWin.mNextRecvChatId);

        ChatNewMarkManager.Self.SetChatStateByType(ChatType.World, false);
        ChatNewMarkManager.Self.RefreshAllChatNewMark();
    }

    public static void InitData()
    {
        ChatWorldWindow tmpWorldWin = DataCenter.GetData("CHAT_WORLD_WINDOW") as ChatWorldWindow;
        if (tmpWorldWin != null)
        {
            string tmpIDKey = GetNextRecvChatIDKey();
            if (PlayerPrefs.HasKey(tmpIDKey))
                tmpWorldWin.mNextRecvChatId = PlayerPrefs.GetInt(tmpIDKey);
        }
    }

    protected override void __OnOpen(object param)
    {
		m_ChatWorldScrollView = GameCommon.FindComponent<UIScrollView> (mGameObjUI,"scrollview");

        mCostCount = DataCenter.mGlobalConfig.GetData("CHAT_WORLD_NEED_COST_COUNT", "VALUE");
        GameCommon.ToggleTrue(GameCommon.FindObject(mGameObjUI.transform.parent.gameObject, "world_channel_button"));
        mChatType = ChatType.World;
        mTargetName = "WORLD";
        Refresh(null);
		__MoveToBottom ();
    }

    public override bool Refresh(object param)
    {
        int nChatTimes = 0;
        DataCenter.Self.get("WORLD_CHAT_TIMES", out nChatTimes);
        SetVisible("cost_count", (mWorldChatFreeCount - nChatTimes) > 0 ? false : true);
        SetVisible("free_count", (mWorldChatFreeCount - nChatTimes) > 0 ? true : false);
        SetText("free_count", "[99ff00]" + (mWorldChatFreeCount - nChatTimes).ToString());
        SetChatSendButtonData((int)mChatType);

		__MoveToBottom ();
        return base.Refresh(param);

    }

	private void __MoveToBottom()
	{
		m_ChatWorldScrollView.SetDragAmount(0.5f,1f,false);
	}

    public override void onChange(string keyIndex, object objVal)
    {
        if (keyIndex == "REDUCE_COUNT_THEN_MAY_COST")
        {
            int tmpReduceCount = (int)objVal;
            if (tmpReduceCount > 0)
            {
                int nChatTimes = 0;
                DataCenter.Self.get("WORLD_CHAT_TIMES", out nChatTimes);
                int tmpCostCount = 0;
                if (nChatTimes + tmpReduceCount > mWorldChatFreeCount)
                {
                    if (nChatTimes < mWorldChatFreeCount)
                        tmpCostCount = nChatTimes + tmpReduceCount - mWorldChatFreeCount;
                    else
                        tmpCostCount = tmpReduceCount;
                }
                nChatTimes += tmpReduceCount;
                DataCenter.Set("WORLD_CHAT_TIMES", nChatTimes);
                if (tmpCostCount > 0)
                    GameCommon.RoleChangeDiamond(-mCostCount * tmpCostCount);
            }
        }
        base.onChange(keyIndex, objVal);
    }

    public override void OnRecieveAllChat(object param)
    {
        this.DoCoroutine(Chat_RcvWorldChat_Requester.StartRequest(mNextRecvChatId));
    }

    public override void SetChatSendButtonData(int type)
    {
        int nChatTimes = 0;
        DataCenter.Self.get("WORLD_CHAT_TIMES", out nChatTimes);
        GameCommon.GetButtonData(GetSub("chat_send_button")).set("FREE_COUNT", (mWorldChatFreeCount - nChatTimes));
        GameCommon.GetButtonData(GetSub("chat_send_button")).set("COST_COUNT", mCostCount);
        base.SetChatSendButtonData(type);
    }
}
