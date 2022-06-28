using UnityEngine;
using System.Collections;
using Logic;

//角色信息协议

/// <summary>
/// 仅用于获取角色信息
/// </summary>
/// 请求
public class CS_GetPlayerData2 : GameServerMessage
{
    public CS_GetPlayerData2():
        base()
    {
        pt = "CS_GetPlayerData2";
    }
}
//回复
public class SC_GetPlayerData2 : RespMessage
{
}
