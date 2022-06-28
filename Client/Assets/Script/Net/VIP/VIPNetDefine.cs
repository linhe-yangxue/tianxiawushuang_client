using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;

//VIP协议

/// <summary>
/// VIP商店
/// </summary>
/// 请求
public class CS_VIPShop : GameServerMessage
{
    public int index;

    public CS_VIPShop():
        base()
    {
        pt = "CS_VIPShop";
    }
}
//回复
public class SC_VIPShop : RespMessage
{
    public int isBuySuccess;
    public ItemDataBase[] buyItem;
}

/// <summary>
/// VIP商店请求
/// </summary>
/// 请求
public class CS_VIPShopQuery : GameServerMessage
{
    public CS_VIPShopQuery():
        base()
    {
        pt = "CS_VIPShopQuery";
    }
}
//回复
public class SC_VIPShopQuery : RespMessage
{
    public List<ShopPropData> buyInfo;
}

/// <summary>
/// 改变VIP等级
/// </summary>
/// 请求
public class CS_ChangeVipLevel : GameServerMessage
{
    public int vipLevel;        //VIP等级

    public CS_ChangeVipLevel():
        base()
    {
        pt = "CS_ChangeVipLevel";
    }
}
//回复
public class SC_ChangeVipLevel : RespMessage
{
}
