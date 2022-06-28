using System.Collections;

//-------------------------------------------------------------------------
// 聊天协议
//-------------------------------------------------------------------------

/// <summary>
/// 登陆聊天服务器
/// </summary>
public class CS_Chat_Login : ChatServerMessage
{
	public CS_Chat_Login():
		base()
	{
		pt = "CS_LoginChat";
	}
}
/// <summary>
/// 登陆聊天服务器（回复）
/// </summary>
public class SC_Chat_Login : RespMessage
{
	public int worldChatTimes;
}

/// <summary>
/// 世界聊天
/// </summary>
public class CS_Chat_World : ChatServerMessage
{
	public string nickName;
	public string message;

	public CS_Chat_World():
		base()
	{
		pt = "CS_ChatWorld";
	}
}
/// <summary>
/// 世界聊天（回复）
/// </summary>
public class SC_Chat_World : RespMessage
{
	public string fromNickName;
	public string fromUid;
	public int fromIconIndex;
	public string message;
}

/// <summary>
/// 公会聊天
/// </summary>
public class CS_Chat_Guild : ChatServerMessage
{
	public string nickName;
	public string guildid;
	public string message;

	public CS_Chat_Guild():
		base()
	{
		pt = "CS_ChatGuild";
	}
}
/// <summary>
/// 公会聊天（回复）
/// </summary>
public class SC_Chat_Guild : RespMessage
{
	public string fromNickName;
	public string fromUid;
	public int fromIconIndex;
	public string message;
}

/// <summary>
/// 私聊
/// </summary>
public class CS_Chat_Private : ChatServerMessage
{
	public string nickName;
	public string toNickName;
	public string toUid;
	public string message;

	public CS_Chat_Private():
		base()
	{
		pt = "CS_ChatPrivate";
	}
}
public class SC_Chat_Private : RespMessage
{
	public string fromNickName;
	public string fromUid;
	public int fromIconIdex;
	public string toNickName;
	public string toUid;
	public string message;
}
