using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//夺宝之战协议

/// <summary>
/// 夺宝目标
/// </summary>
public class RobAim
{
    public string uid;         //目标Id
    public string name;     //目标名字
    public int level;       //主角等级
    public int charTid;     //主角Tid
    public int[] petTid;    //符灵Tid数组
}

/// <summary>
/// 被夺历史
/// </summary>
public class HistoryOfRobbed
{
    public string uid;         //抢夺者id
    public string name;     //抢夺者名字
    public int tid;         //被抢物品tid
    public int robTime;     //被抢时间
}

/// <summary>
/// 夺宝目标列表
/// </summary>
/// 请求
public class CS_GetRobAimList : GameServerMessage
{
    public int tid;         //法宝碎片tid

    public CS_GetRobAimList():
        base()
    {
        pt = "CS_GetRobAimList";
    }
}
//返回
public class SC_GetRobAimList : RespMessage
{
    public RobAim[] arr;        //夺宝信息列表
}

/// <summary>
/// 开始夺宝
/// </summary>
/// 请求
public class CS_RobMagicStart : GameServerMessage
{
    public string aimUid;       //被抢玩家uid
    public int tid;             //法宝碎片tid

    public CS_RobMagicStart():
        base()
    {
        pt = "CS_RobMagicStart";
    }
}
//回复
public class SC_RobMagicStart : RespMessage
{
    public int noEnoughFrag;        //目标玩家碎片不足
    public int InTruce;             //目标玩家处于免战状态
    public ChallengePlayer opponent;     //玩家战斗数据
}

/// <summary>
/// 结束夺宝
/// </summary>
/// 请求
public class CS_RobMagicResult : GameServerMessage
{
    public string aimUid;       //被抢玩家
    public int tid;             //法宝碎片tid
    public int isWin;           //是否胜利

    public CS_RobMagicResult():
        base()
    {
        pt = "CS_RobMagicResult";
    }
}
//回复
public class SC_RobMagicResult : RespMessage
{
    public int succeed;         //是否成功抢夺
    public int gold;            //硬币数量
    public int exp;             //经验数量
    public ItemDataBase awardItem;      //奖励物品
    public ItemDataBase[] allAddItems;  //所有添加物品
}

/// <summary>
/// 夺五次
/// </summary>
/// 请求
public class CS_Rob5Times : GameServerMessage
{
    public string aimUid;       //被抢玩家
    public int tid;             //法强碎片tid

    public CS_Rob5Times():
        base()
    {
        pt = "CS_Rob5Times";
    }
}
//回复
public class SC_Rob5Times : RespMessage
{
    public int succeed;      //是否成功抢夺
    public int[] golds;     //硬币数量
    public int[] exps;      //经验数量
    public ItemDataBase[] awardItems;   //奖励物品
    public ItemDataBase[] allAddItems;  //所有添加物品

    public int MaxGrabCount
    {
        get
        {
            return golds.Length;
        }
    }

    /// <summary>
    /// 抢夺成功的次数，未成功返回-1
    /// </summary>
    public int SuccessGrabCount
    {
        get
        {
            if (succeed == 0)
                return -1;
            return golds.Length;
        }
    }
}
/// <summary>
/// 一键夺宝
/// </summary>
/// 请求
public class CS_RobOneKey : GameServerMessage
{
	public int equipTid;          //法器tid
	public int fragmentTid;				//碎片tip
	public CS_RobOneKey():
		base()
	{
		pt = "CS_RobOneKey";
	}
}
//回复
public class SC_RobOneKey : RespMessage
{
	public int succeed;      //是否成功抢夺
	public int gold;     //硬币数量
	public int exp;      //经验数量
	public ItemDataBase awardItem;   //奖励物品
	public ItemDataBase[] allAddItems;  //所有添加物品
}
/// <summary>
/// 被抢夺记录
/// </summary>
/// 请求
public class CS_GetRobbedHistoryList : GameServerMessage
{
    public CS_GetRobbedHistoryList():
        base()
    {
        pt = "CS_GetRobbedHistoryList";
    }
}
//回复
public class SC_GetRobbedHistoryList : RespMessage
{
    public HistoryOfRobbed[] arr;       //抢夺成功历史队列
}

/// <summary>
/// 法器合成
/// </summary>
/// 请求
public class CS_MagicCompose : GameServerMessage
{
    public int tid;                     //法器tid
    public List<ItemDataBase> frags;    //合成所需碎片

    public CS_MagicCompose():
        base()
    {
        pt = "CS_MagicCompose";
    }
}
//回复
public class SC_MagicCompose : RespMessage
{
    public ItemDataBase ItemMagic;        //合成法器
    public List<ItemDataBase> magicFragmentsInfo;         
}

/// <summary>
/// 使用免战令牌
/// </summary>
/// 请求
public class CS_UseTruceCard : GameServerMessage
{
    public ItemDataBase truceCard;      //免战令

    public CS_UseTruceCard():
        base()
    {
        pt = "CS_UseTruceCard";
    }
}
//回复
public class SC_UseTruceCard : RespMessage
{
}

/// <summary>
/// 获取免战时间
/// </summary>
/// 请求
public class CS_GetTruceTime : GameServerMessage
{
    public CS_GetTruceTime():
        base()
    {
        pt = "CS_GetTruceTime";
    }
}
//回复
public class SC_GetTruceTime : RespMessage
{
    public long time;       //免战时间
}
