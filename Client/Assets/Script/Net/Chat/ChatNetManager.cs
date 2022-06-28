using UnityEngine;
using System.Collections;

/// <summary>
/// 发送私聊
/// </summary>
public class Chat_SendPrivateChat_Requester:
    NetRequester<CS_SendPrivateChat, SC_SendPrivateChat>
{
    private string mMsg;        //消息内容
    private string mTargetId;      //目标Id

    public static IEnumerator StartRequest(string msg, string targetId)
    {
        Chat_SendPrivateChat_Requester requester = new Chat_SendPrivateChat_Requester();
        requester.mMsg = msg;
        requester.mTargetId = targetId;
        yield return requester.Start();
    }

    protected override CS_SendPrivateChat GetRequest()
    {
        CS_SendPrivateChat req = new CS_SendPrivateChat();
        req.msg = mMsg;
        req.targetId = mTargetId;
        return req;
    }

    protected override void OnSuccess()
    {
        if(respCode == 1)
            DataCenter.SetData("CHAT_PRIVATE_WINDOW", "SEND_SUCCESS", respMsg);
    }
    protected override void OnFail()
    {
        //TODO
    }
}

/// <summary>
/// 获取私聊
/// </summary>
public class Chat_RcvPrivateChat_Requester:
    NetRequester<CS_RcvPrivateChat, SC_RcvPrivateChat>
{
    private int mRcdIndex;      //聊天记录序号

    public static IEnumerator StartRequest(int rcdIndex)
    {
        Chat_RcvPrivateChat_Requester requester = new Chat_RcvPrivateChat_Requester();
        requester.mRcdIndex = rcdIndex;
        yield return requester.Start();
    }

    protected override CS_RcvPrivateChat GetRequest()
    {
        CS_RcvPrivateChat req = new CS_RcvPrivateChat();
        req.rcdIndex = mRcdIndex;
        return req;
    }

    protected override void OnSuccess()
    {
        if (respCode == 1)
            DataCenter.SetData("CHAT_PRIVATE_WINDOW", "RECIEVE_CHAT", respMsg);
    }
    protected override void OnFail()
    {
        //TODO
    }
}

/// <summary>
/// 发送公会聊天
/// </summary>
public class Chat_SendGuildChat_Requester:
    NetRequester<CS_SendGuildChat, SC_SendGuildChat>
{
    private string mMsg;        //消息内容

    public static IEnumerator StartRequest(string msg)
    {
        Chat_SendGuildChat_Requester requester = new Chat_SendGuildChat_Requester();
        requester.mMsg = msg;
        yield return requester.Start();
    }

    protected override CS_SendGuildChat GetRequest()
    {
        CS_SendGuildChat req = new CS_SendGuildChat();
        req.msg = mMsg;
        return req;
    }

    protected override void OnSuccess()
    {
        if (respCode == 1)
        {
            DataCenter.SetData("CHAT_UNION_WINDOW", "SEND_SUCCESS", respMsg);
            if (respMsg.kicked == 1)
                DataCenter.OpenWindow("CHAT_WINDOW", null);
        }
    }
    protected override void OnFail()
    {
        //TODO
        switch (respCode)
        {
            case 1154: break;
        }
    }
}

/// <summary>
/// 获取公会聊天
/// </summary>
public class Chat_RcvGuildChat_Requester:
    NetRequester<CS_RcvGuildChat, SC_RcvGuildChat>
{
    private int mRcdIndex;      //聊天记录序号

    public static IEnumerator StartRequest(int rcdIndex)
    {
        Chat_RcvGuildChat_Requester requester = new Chat_RcvGuildChat_Requester();
        requester.mRcdIndex = rcdIndex;
        yield return requester.Start();
    }

    protected override CS_RcvGuildChat GetRequest()
    {
        CS_RcvGuildChat req = new CS_RcvGuildChat();
        req.rcdIndex = mRcdIndex;
        return req;
    }

    protected override void OnSuccess()
    {
        if (respCode == 1)
            DataCenter.SetData("CHAT_UNION_WINDOW", "RECIEVE_CHAT", respMsg);
        if (respMsg.kicked == 1)
        {
            RoleLogicData.Self.guildId = "";     //如果被踢，将公会Id改为0
            DataCenter.OpenWindow("CHAT_WINDOW", null);
        }
    }
    protected override void OnFail()
    {
        //TODO
        switch (respCode)
        {
            case 1154: break;
        }
    }
}

/// <summary>
/// 发送世界聊天
/// </summary>
public class Chat_SendWorldChat_Requester:
    NetRequester<CS_SendWorldChat, SC_SendWorldChat>
{
    private string mMsg;        //消息内容

    public static IEnumerator StartRequest(string msg)
    {
        Chat_SendWorldChat_Requester requester = new Chat_SendWorldChat_Requester();
        requester.mMsg = msg;
        yield return requester.Start();
    }

    protected override CS_SendWorldChat GetRequest()
    {
        CS_SendWorldChat req = new CS_SendWorldChat();
        req.msg = mMsg;
        return req;
    }

    protected override void OnSuccess()
    {
        if (respCode == 1)
        {
            if (respMsg.isDontChat == 1)
            {
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_CHAT_BANNED_TO_POST);
                return;
            }
            DataCenter.SetData("CHAT_WORLD_WINDOW", "SEND_SUCCESS", respMsg);
        }
    }
    protected override void OnFail()
    {
        //TODO
        switch (respCode)
        {
            case 1153: break;
        }
    }
}

/// <summary>
/// 获取世界聊天
/// </summary>
public class Chat_RcvWorldChat_Requester:
    NetRequester<CS_RcvWorldChat, SC_RcvWorldChat>
{
    private int mRcdIndex;      //聊天记录序号

    public static IEnumerator StartRequest(int rcdIndex)
    {
        Chat_RcvWorldChat_Requester requester = new Chat_RcvWorldChat_Requester();
        requester.mRcdIndex = rcdIndex;
        yield return requester.Start();
    }

    protected override CS_RcvWorldChat GetRequest()
    {
        CS_RcvWorldChat req = new CS_RcvWorldChat();
        req.rcdIndex = mRcdIndex;
        return req;
    }

    protected override void OnSuccess()
    {
        if (respCode == 1)
            DataCenter.SetData("CHAT_WORLD_WINDOW", "RECIEVE_CHAT", respMsg);
    }
    protected override void OnFail()
    {
        //TODO
    }
}

/// <summary>
/// 获取世界聊天次数
/// </summary>
public class Chat_GetWorldChatCnt_Requester:
    NetRequester<CS_GetWorldChatCnt, SC_GetWorldChatCnt>
{
    public static IEnumerator StartRequest()
    {
        Chat_GetWorldChatCnt_Requester requester = new Chat_GetWorldChatCnt_Requester();
        yield return requester.Start();
    }

    protected override CS_GetWorldChatCnt GetRequest()
    {
        CS_GetWorldChatCnt req = new CS_GetWorldChatCnt();
        return req;
    }

    protected override void OnSuccess()
    {
        if (respMsg.ret == 1)
        {
            DataCenter.Set("WORLD_CHAT_TIMES", respMsg.cnt);
        }
    }
    protected override void OnFail()
    {
        //TODO
    }
}
