using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 用资源
/// </summary>
/// 请求
public class CS_ResourceHint_UseResourceToken : GameServerMessage
{
	public ItemDataBase itemData;
	
	public CS_ResourceHint_UseResourceToken():
		base()
	{
		pt = "CS_UseTruceToken";
	}
}
//回复
public class SC_ResourceHint_UseResourceToken : RespMessage
{
}

/// <summary>
/// 买资源
/// </summary>
/// 请求
public class CS_ResourceHint_BuyResourceToken : GameServerMessage
{
	public int sIndex;
	public int num;
	
	public CS_ResourceHint_BuyResourceToken():
		base()
	{
		pt = "CS_PropShopPurchase";
	}
}
//购买回复
public class SC_ResourceHint_BuyResourceToken : RespMessage
{
	public int isBuySuccess;        //是否购买成功
	public ItemDataBase[] buyItem;  //购买的物品
}


/// <summary>
/// 获得资源还可购买次数
/// </summary>
public class CS_PropShopItemQueryToken : GameServerMessage 
{
	public int index;

	public CS_PropShopItemQueryToken() : base()
	{
		pt = "CS_PropShopItemQuery";
	}
}

//还可购买次数回复
public class SC_PropShopItemQueryToken : RespMessage
{
	public List<ShopPropData> prop;

}
