using System;

/*
 * 限时抢购协议
 */

#region  限时抢购协议

public class FlashSaleItem
{
    public int index = -1;
    public long putawayTime;
    public int state;
}

/// <summary>
/// GetFlashSaleList
/// </summary>
/// 请求
/// 
public class CS_GetFlashSaleList : GameServerMessage
{
    public int type = -1;
    public CS_GetFlashSaleList() :
        base()
    {
        pt = "CS_GetFlashSaleList";
    }
}
//回复
public class SC_GetFlashSaleList : RespMessage
{
    public FlashSaleItem[] flashSaleList;
}

/// <summary>
/// FlashPurchase
/// </summary>
/// 请求
/// 
public class CS_FlashPurchase : GameServerMessage
{
    public int index = -1;
    public CS_FlashPurchase() :
        base()
    {
        pt = "CS_FlashPurchase";
    }
}
//回复
public class SC_FlashPurchase : RespMessage
{
    public ItemDataBase[] rewards;
}

#endregion