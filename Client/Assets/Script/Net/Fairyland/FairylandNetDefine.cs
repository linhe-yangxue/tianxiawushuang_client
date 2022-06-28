using UnityEngine;
using System.Collections;

//符灵探险协议文件

public class FairylandEvent
{
    public int index;           //表格索引
    public int tid;             //物品表格索引
    public int itemNum;         //物品数量
    public string friendName;   //好友名称
}

public class FairylandFriend
{
    public string zuid;             //好友Id
    public string name;             //名字
    public int level;               //等级
    public int conquerdNum;         //征服数量
    public int iconIndex;           //头像
    public int exploringNum;        //探索中数量
    public int riotingNum;          //暴乱中数量
}

/// <summary>
/// 获取仙境状态列表
/// </summary>
/// 请求
public class CS_Fairyland_GetFairylandStates : GameServerMessage
{
    public string ownerId;         //所有者Id

    public CS_Fairyland_GetFairylandStates():
        base()
    {
        pt = "CS_GetFairylandStates";
    }
}
//回复
public class SC_Fairyland_GetFairylandStates : RespMessage
{
    public int[] fairylandState;        //仙境状态数组
    public long[] endTime;              //结束时间
    public int repressCnt;        //已镇压次数
}

/// <summary>
/// 开始征服仙境
/// </summary>
/// 请求
public class CS_Fairyland_ConquerFairylandStart : GameServerMessage
{
    public int fairylandId;         //仙境Id

    public CS_Fairyland_ConquerFairylandStart():
        base()
    {
        pt = "CS_ConquerFairylandStart";
    }
}
//回复
public class SC_Fairyland_ConquerFairylandStart : RespMessage
{
}

/// <summary>
/// 结束征服仙境
/// </summary>
/// 请求
public class CS_Fairyland_ConquerFairylandWin : GameServerMessage
{
    public CS_Fairyland_ConquerFairylandWin():
        base()
    {
        pt = "CS_ConquerFairylandWin";
    }
}
//回复
public class SC_Fairyland_ConquerFairylandWin : RespMessage
{
    public ItemDataBase[] awardItems;           //奖励物品数量
}

/// <summary>
/// 探索仙境
/// </summary>
/// 请求
public class CS_Fairyland_ExploreFairyland : GameServerMessage
{
    public int fairylandId;         //仙境Id
    public int petItemId;           //寻仙符灵Id
    public int exploreType;         //探索方式

    public CS_Fairyland_ExploreFairyland():
        base()
    {
        pt = "CS_ExploreFairyland";
    }
}
//回复
public class SC_Fairyland_ExploreFairyland : RespMessage
{
    public long endTime;            //寻仙结束时间
}

/// <summary>
/// 获取仙境信息
/// </summary>
/// 请求
public class CS_Fairyland_GetFairylandEvents : GameServerMessage
{
    public int fairylandId;         //仙境Id
    public string targetId;         //目标用户Id

    public CS_Fairyland_GetFairylandEvents():
        base()
    {
        pt = "CS_GetFairylandEvents";
    }
}
//回复
public class SC_Fairyland_GetFairylandEvents : RespMessage
{
    public FairylandEvent[] events;         //事件列表
    public long endTime;                    //结束时间
    public long refreshTime;                //刷新时间
    public int petTid;                      //寻仙符灵Id
}

/// <summary>
/// 领取探索奖励
/// </summary>
/// 请求
public class CS_Fairyland_TakeFairylandAwards : GameServerMessage
{
    public int fairylandId;         //仙境Id

    public CS_Fairyland_TakeFairylandAwards():
        base()
    {
        pt = "CS_TakeFairylandAwards";
    }
}
//回复
public class SC_Fairyland_TakeFairylandAwards : RespMessage
{
    public ItemDataBase[] awardItems;
}

/// <summary>
/// 镇压暴乱
/// </summary>
/// 请求
public class CS_Fairyland_RepressRiot : GameServerMessage
{
    public string friendId;         //好友Id
    public int fairylandId;         //仙境Id

    public CS_Fairyland_RepressRiot():
        base()
    {
        pt = "CS_RepressRiot";
    }
}
//回复
public class SC_Fairyland_RepressRiot : RespMessage
{
}

/// <summary>
/// 获取好友仙境信息
/// </summary>
/// 请求
public class CS_Fairyland_GetFairylandFriendList : GameServerMessage
{
    public CS_Fairyland_GetFairylandFriendList():
        base()
    {
        pt = "CS_GetFairylandFriendList";
    }
}
//回复
public class SC_Fairyland_GetFairylandFriendList : RespMessage
{
    public FairylandFriend[] ffList;            //仙境好友对象数组
}
