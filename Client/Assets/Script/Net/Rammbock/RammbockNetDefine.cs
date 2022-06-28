using System.Collections;
using System.Collections.Generic;

//爬塔协议

/// <summary>
/// Buff信息
/// </summary>
public class ClimbTowerBuffInfo
{
    public string buffType;
    public int buffValue;
}

/// <summary>
/// 获取爬塔信息
/// </summary>
/// 请求
public class CS_Rammbock_GetTowerClimbingInfo : GameServerMessage
{
    public CS_Rammbock_GetTowerClimbingInfo():
        base()
    {
        pt = "CS_GetTowerClimbingInfo";
    }
}
//回复
public class SC_Rammbock_GetTowerClimbingInfo : RespMessage
{
    public int rankStars;
    public int currentStars;
    public int remainStars;
    public int nextTier;
    public int tierState;
    public List<int> buffList;
    public int[] chooseBuff;
    public int[] starList;
    public int resetTimes;
    public ItemDataBase itemOnSale;
    public int saleItemState;           //0为可以买，1为不可以买
	public int previousTierStarNum;  			//当前层的星星总数
    public int saleItemPrice;  //打折商品价格
    public int originalItemPrice;  //打折商品原价
}

/// <summary>
/// 开始爬塔
/// </summary>
/// 请求
public class CS_Rammbock_ClimbTowerStart : GameServerMessage
{
    public int crtTierStars;        //当前挑战星数

    public CS_Rammbock_ClimbTowerStart():
        base()
    {
        pt = "CS_ClimbTowerStart";
    }
}
//回复
public class SC_Rammbock_ClimbTowerStart : RespMessage
{
}

/// <summary>
/// 结束爬塔
/// </summary>
/// 请求
public class CS_Rammbock_ClimbTowerResult : GameServerMessage
{
    public int isWin;                           //是否胜利
    public ClimbTowerBuffInfo[] buffEffect;     //当前拥有的爬塔Buff

    public CS_Rammbock_ClimbTowerResult():
        base()
    {
        pt = "CS_ClimbTowerResult";
    }
}
//回复
public class SC_Rammbock_ClimbTowerResult : RespMessage
{
    public ItemDataBase itemOnSale;         //打折商品信息
    public int[] chooseBuff;                //备选的buff
    public int awardIndex;                  //奖励序号
    public ItemDataBase[] arr;              //三关奖励
    public int saleItemPrice;  //打折商品价格
    public int originalItemPrice;  //打折商品原价
}

/// <summary>
/// 选buff
/// </summary>
/// 请求
public class CS_Rammbock_ClimbTowerChooseBuff : GameServerMessage
{
    public int buffIndex;

    public CS_Rammbock_ClimbTowerChooseBuff():
        base()
    {
        pt = "CS_ClimbTowerChooseBuff";
    }
}
//回复
public class SC_Rammbock_ClimbTowerChooseBuff : RespMessage
{
}

/// <summary>
/// 买商品
/// </summary>
/// 请求
public class CS_Rammbock_ClimbTowerBuyCommodity : GameServerMessage
{
    public CS_Rammbock_ClimbTowerBuyCommodity():
        base()
    {
        pt = "CS_ClimbTowerBuyCommodity";
    }
}
//回复
public class SC_Rammbock_ClimbTowerBuyCommodiy : RespMessage
{
    public ItemDataBase[] arr;
}

/// <summary>
/// 重置关卡
/// </summary>
/// 请求
public class CS_Rammbock_ResetClimbTower : GameServerMessage
{
    public CS_Rammbock_ResetClimbTower():
        base()
    {
        pt = "CS_ResetClimbTower";
    }
}
//回复
public class SC_Rammbock_ResetClimbTower : RespMessage
{
}

/// <summary>
/// 一键3星
/// </summary>
/// 请求
public class CS_ClimbTowerSweep : GameServerMessage
{
    public ClimbTowerBuffInfo[] buffEffect;     //当前拥有的爬塔Buff

    public CS_ClimbTowerSweep() :
        base()
    {
        pt = "CS_ClimbTowerSweep";
    }
}
//回复
public class SC_ClimbTowerSweep : RespMessage
{
    public int[] chooseBuff;
    public int[] awardIndexes;
    public ItemDataBase[] tierPassRewards;
}