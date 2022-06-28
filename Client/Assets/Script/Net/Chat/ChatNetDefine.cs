using UnityEngine;
using System.Collections;
using System;

//聊天协议

/// <summary>
/// 聊天记录数据
/// </summary>
[Serializable]
public class ChatRecord
{
    public string zuid;             //来源Uid
    public string name;         //来源名称
    public int charTid;         //头像序号
    public string msg;          //聊天内容
    public string targetName;   //目标名称

    public string ChatInfo
    {
        set { msg = value; }
        get { return msg; }
    }

    public string SrcUid
    {
        get { return zuid; }
    }
    public string SrcName
    {
        get { return name; }
    }
    public int SrcHeadIconId
    {
        get { return charTid; }
    }

    public string TargetName
    {
        get { return targetName; }
    }
}

/// <summary>
/// 发送私聊
/// </summary>
/// 请求
public class CS_SendPrivateChat : GameServerMessage
{
    public string msg;      //消息内容
    public string targetId;    //目标Id

    public CS_SendPrivateChat():
        base()
    {
        pt = "CS_SendPrivateChat";
    }
}
//回复
public class SC_SendPrivateChat : RespMessage
{
}

/// <summary>
/// 获取私聊
/// </summary>
/// 请求
public class CS_RcvPrivateChat : GameServerMessage
{
    public int rcdIndex;        //聊天记录序号

    public CS_RcvPrivateChat():
        base()
    {
        pt = "CS_RcvPrivateChat";
    }
}
//回复
public class SC_RcvPrivateChat : RespMessage
{
    public ChatRecord[] arr;
    public int rcdIndex;
}

/// <summary>
/// 发送公会聊天
/// </summary>
/// 请求
public class CS_SendGuildChat : GameServerMessage
{
    public string msg;          //消息内容

    public CS_SendGuildChat():
        base()
    {
        pt = "CS_SendGuildChat";
    }
}
//回复
public class SC_SendGuildChat : RespMessage
{
    public int kicked;      //被踢
}

/// <summary>
/// 获取公会聊天
/// </summary>
/// 请求
public class CS_RcvGuildChat : GameServerMessage
{
    public int rcdIndex;        //聊天记录序号

    public CS_RcvGuildChat():
        base()
    {
        pt = "CS_RcvGuildChat";
    }
}
//回复
public class SC_RcvGuildChat : RespMessage
{
    public ChatRecord[] arr;        //聊天记录数据
    public int rcdIndex;            //聊天记录序号
    public int kicked;              //被踢
}

/// <summary>
/// 发送世界聊天
/// </summary>
/// 请求
public class CS_SendWorldChat : GameServerMessage
{
    public string msg;      //消息内容

    public CS_SendWorldChat():
        base()
    {
        pt = "CS_SendWorldChat";
    }
}
//回复
public class SC_SendWorldChat : RespMessage
{
    public int isDontChat;      //是否禁言
}

/// <summary>
/// 获取世界聊天
/// </summary>
/// 请求
public class CS_RcvWorldChat : GameServerMessage
{
    public int rcdIndex;        //聊天记录序号

    public CS_RcvWorldChat():
        base()
    {
        pt = "CS_RcvWorldChat";
    }
}
//回复
public class SC_RcvWorldChat : RespMessage
{
    public ChatRecord[] arr;        //聊天记录数组
    public int rcdIndex;            //聊天记录序号
}

/// <summary>
/// 获取世界聊天次数
/// </summary>
/// 请求
public class CS_GetWorldChatCnt : GameServerMessage
{
    public CS_GetWorldChatCnt():
        base()
    {
        pt = "CS_GetWorldChatCnt";
    }
}
//回复
public class SC_GetWorldChatCnt : RespMessage
{
    public int cnt;         //世界聊天次数
}
