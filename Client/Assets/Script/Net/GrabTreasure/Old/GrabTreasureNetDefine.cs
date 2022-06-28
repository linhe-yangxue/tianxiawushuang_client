using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

//夺宝协议

/// <summary>
/// 夺宝玩家信息
/// </summary>
public class GrabTreasure_IndianaObject
{
    public int type;            //玩家类型，1为玩家，0为机器人
    public int indexId;         //玩家索引
    public int iconId;          //玩家头像Id，如果为玩家，头像Id为iconId，如果为机器人，根据indexId查表
    public string name;         //玩家名字
    public int level;           //等级
    public int[] petTid;         //符灵ID数组
}

/// <summary>
/// 夺宝历史信息
/// </summary>
public class GrabTreasure_IndianaHistoryObject
{
    public string playerName;       //玩家姓名
    public int itemTid;              //物品Id
    public long time;               //被抢夺距离现在多少秒
}

/// <summary>
/// 夺宝玩家列表
/// </summary>
/// 请求
public class CS_GrabTreasure_IndianaPlayerList : GameServerMessage
{
    public int tid;          //碎片Id

    public CS_GrabTreasure_IndianaPlayerList():
        base()
    {
        pt = "CS_IndianaPlayerList";
    }
}
//回复
public class SC_GrabTreasure_IndianPlayerList : RespMessage
{
    public GrabTreasure_IndianaObject[] arr;        //夺宝信息列表
}

/// <summary>
/// 点击夺宝按钮
/// </summary>
/// 发送
public class CS_GrabTreasure_PointIndianaButton : GameServerMessage
{
    public int tid;             //目标Id
    public int itemTid;         //碎片Id
    public int type;            //玩家类型
    public int robotLevel;      //等级

    public CS_GrabTreasure_PointIndianaButton():
        base()
    {
        pt = "CS_PointIndianaButton";
    }
}
//回复
public class SC_GrabTreasure_PointIndianaButton : RespMessage
{
    public int flag;                            //是否能满足夺宝条件
    public ChallengePlayer challengePlayer;     //战斗所需数据
}

/// <summary>
/// 单次夺宝结算
/// </summary>
/// 请求
public class CS_GrabTreasure_ResultIndiana : GameServerMessage
{
    public int resultFightFlag;     //战斗是否成功
    public int tid;                 //法宝Id
    public int type;                //是否为玩家
    public int robedUid;            //被抢的玩家
    public int robotLevel;          //机器人等级

    public CS_GrabTreasure_ResultIndiana():
        base()
    {
        pt = "CS_ResultIndiana";
    }
}
//回复
public class SC_GrabTreasure_ResultIndiana : RespMessage
{
    public int robFlag;         //是否成功抢夺，1为成功
    public ItemDataBase[] arr;  //掉落队列
}

/// <summary>
/// 点击夺宝结算关闭按钮
/// </summary>
/// 请求
public class CS_GrabTreasure_ResultCloseClick : GameServerMessage
{
    public int resultFightFlag;             //战斗是否成功

    public CS_GrabTreasure_ResultCloseClick():
        base()
    {
        pt = "CS_ResultCloseClick";
    }
}
//回复
public class SC_GrabTreasure_ResultCloseClick : RespMessage
{
    public ItemDataBase[] arr;      //选择物品队列
}

/// <summary>
/// 点击选择奖励物品
/// </summary>
/// 请求
public class CS_GrabTreasure_SelectAwardClick : GameServerMessage
{
    public ItemDataBase awardItem;      //奖励物品

    public CS_GrabTreasure_SelectAwardClick():
        base()
    {
        pt = "CS_SelectAwardClick";
    }
}
//回复
public class SC_GrabTreasure_SelectAwardClick : RespMessage
{
}

/// <summary>
/// 点击夺宝5次按钮
/// </summary>
/// 请求
public class CS_GrabTreasure_IndianaFiveButton : GameServerMessage
{
    public int tid;         //法宝Id
    public int robedUid;    //抢夺对象Id
    public int robotLevel;  //等级

    public CS_GrabTreasure_IndianaFiveButton():
        base()
    {
        pt = "CS_IndianaFiveButton";
    }
}
//回复
public class SC_GrabTreasure_IndianaFiveButton : RespMessage
{
    public int flag;            //是否能满足5次夺宝条件
    public int successTime;     //第几次夺宝成功
    public ItemDataBase[] arr;  //奖励物品
}
/// <summary>
/// 一键夺宝按钮
/// </summary>
/// 请求
public class CS_GrabTreasure_IndianaRobOneButton : GameServerMessage
{
	public int MagicTid;        //法宝Id
	public int Tid;    			//抢夺对象Id
	public CS_GrabTreasure_IndianaRobOneButton():
		base()
	{
		pt = "CS_IndianaRobOneButton";
	}
}
//回复
public class SC_GrabTreasure_IndianaRobOneButton : RespMessage
{
	public int flag;            //是否能满足5次夺宝条件
	public int successTime;     //第几次夺宝成功
	public ItemDataBase[] arr;  //奖励物品
}
/// <summary>
/// 夺宝历史
/// </summary>
/// 请求
public class CS_GrabTreasure_IndianaHistory : GameServerMessage
{
    public CS_GrabTreasure_IndianaHistory():
        base()
    {
        pt = "CS_IndianaHistory";
    }
}
//回复
public class SC_GrabTreasure_IndianaHistory
{
    public GrabTreasure_IndianaHistoryObject[] arr;     //抢夺成功历史队列
}

/// <summary>
/// 法宝合成
/// </summary>
/// 请求
public class CS_GrabTreasure_IndianaCompose : GameServerMessage
{
    public int tid;
    public List<ItemDataBase> arr = new List<ItemDataBase>();

    public CS_GrabTreasure_IndianaCompose() :
        base()
    {
        pt = "CS_IndianaCompose";
    }
}
//回复
public class SC_GrabTreasure_IndianaCompose : RespMessage
{
    public int composeFlag;         //合成结果标志位
    public ItemDataBase[] arr;
}

/// <summary>
/// 获取免战时间
/// </summary>
/// 请求
public class CS_GrabTreasure_GetTruceTime : GameServerMessage
{
    public CS_GrabTreasure_GetTruceTime() :
        base()
    {
        pt = "CS_GetTruceTime";
    }
}
//回复
public class SC_GrabTreasure_GetTruceTime : RespMessage
{
    public long time;       //免战时间
}

/// <summary>
/// 用免战牌
/// </summary>
/// 请求
public class CS_GrabTreasure_UseTruceToken : GameServerMessage
{
	public ItemDataBase truceCard;

    public CS_GrabTreasure_UseTruceToken():
        base()
    {
		pt = "CS_UseTruceCard";
    }
}
//回复
public class SC_GrabTreasure_UseTruceToken : RespMessage
{
}

/// <summary>
/// 买免战牌
/// </summary>
/// 请求
public class CS_GrabTreasure_BuyTruceToken : GameServerMessage
{
    public int sIndex;
    public int num;

    public CS_GrabTreasure_BuyTruceToken():
        base()
    {
        pt = "CS_PropShopPurchase";
    }
}
//回复
public class SC_GrabTreasure_BuyTruceToken : RespMessage
{
    public int isBuySuccess;        //是否购买成功
    public ItemDataBase[] buyItem;  //购买的物品
}
