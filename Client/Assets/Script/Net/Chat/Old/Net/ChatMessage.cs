using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;



/// <summary>
///  登录聊天服务器（请求）
/// </summary>
class CS_LoginChat : MessageBase
{

    public CS_LoginChat()
    {
        base.pt = "CS_LoginChat";
    }

    public string tk { get; set; }

    public string zid { get; set; }

    public string uid { get; set; }

    public string charid { get; set; }

    public string nickName { get; set; }

}

/// <summary>
/// 登录聊天服务器（返回）
/// </summary>
class SC_LoginChat : MessageBase
{
    public string ret { get; set; }

}


/// <summary>
/// 世界聊天（请求）
/// </summary>
class CS_ChatWorld : MessageBase
{

    public CS_ChatWorld()
    {
        base.pt = "CS_ChatWorld";
    }

    public string tk { get; set; }

    public string zid { get; set; }

    public string uid { get; set; }

    public string charid { get; set; }

	public string nickName { get; set; }

    public string message { get; set; }

}

/// <summary>
/// 世界聊天（返回）
/// </summary>
class SC_ChatWorld : MessageBase
{

    public string ret { get; set; }

    public string fromUid { get; set; }

    public string fromCharid { get; set; }

    public string fromNickName { get; set; }

    public string message { get; set; }


}


/// <summary>
/// 公会聊天（请求）
/// </summary>
class CS_ChatGuild : MessageBase
{

    public CS_ChatGuild() {
        base.pt = "CS_ChatGuild";
    }

    public string tk { get; set; }

    public string zid { get; set; }

    public string uid { get; set; }

    public string charid { get; set; }

    public string nickName { get; set; }

    public string guildid { get; set; }

    public string message { get; set; }

}

/// <summary>
/// 工会聊天（返回）
/// </summary>
class SC_ChatGuild : MessageBase
{

    public string ret { get; set; }

    public string fromUid { get; set; }

    public string fromCharid { get; set; }

    public string fromNickName { get; set; }

    public string message { get; set; }


}


/// <summary>
/// 私聊（请求）
/// </summary>
class CS_ChatPrivate : MessageBase
{
    public CS_ChatPrivate() {
        base.pt = "CS_ChatPrivate";
    }


    public string tk { get; set; }

    public string zid { get; set; }

    public string uid { get; set; }

    public string charid { get; set; }

    public string nickName { get; set; }

    public string toUid { get; set; }

    public string toCharid { get; set; }

    public string message { get; set; }

}



/// <summary>
/// 私聊（返回）
/// </summary>
class SC_ChatPrivate : MessageBase
{

    public string ret { get; set; }

    public string fromUid { get; set; }

    public string fromCharid { get; set; }

    public string fromNickName { get; set; }

    public string message { get; set; }


}