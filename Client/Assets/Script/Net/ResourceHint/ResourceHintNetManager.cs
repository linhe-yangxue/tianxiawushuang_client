using UnityEngine;
using System.Collections;

public class ResourceHintNetManager
{
    private static RESOURCE_HINT_TYPE mResType;
#region 使用资源
	/// <summary>
	/// 使用资源
	/// </summary>
	/// <param name="truceItemData"></param>
	/// <param name="peaceTime"></param>
	public static Coroutine RequestUseItemToken(ItemDataBase kItemData,RESOURCE_HINT_TYPE kResType)
	{
        mResType = kResType;
		UsePropRequester _req = new UsePropRequester (kItemData);
        return GlobalModule.DoCoroutine(DoStart(_req));
	}

	public static IEnumerator DoStart(UsePropRequester kReq)
	{
		yield return kReq.Start ();
        if (kReq.success)
        {
			__OnRequestUseBeatDemonCardTokenSuccess(kReq.respMsg.items);
        }
	}
	private static void __OnRequestUseBeatDemonCardTokenSuccess(ItemDataBase[] kItemDatas)
	{
        switch (mResType) 
        {
            case RESOURCE_HINT_TYPE.BEATDEMONCARD:          
                DataCenter.SetData("BOSS_STAGE_INFO_WINDOW", "REFRESH", null);           
                break;
            case RESOURCE_HINT_TYPE.STAMINA_ITEM:
                break;
            case RESOURCE_HINT_TYPE.SPIRIT_ITEM:
                break;
        }
        //减少背包道具
        //PackageManager.RemoveItem(kItemDatas[0].tid, kItemDatas[0].itemId, 1);
        //刷新数量显示
        DataCenter.SetData("ADD_VITALITY_UP_WINDOW", "REFRESH_BEATDEMON_COUNT", null);
		
	}
	private static void __OnRequestUseBeatDemonCardTokenFailed(string text)
	{
		//TODO
	}
#endregion
	
	

#region 购买资源
	/// <summary>
	/// 买资源
	/// </summary>
	/// <param name="sIndex"></param>
	/// <param name="num"></param>
	public static void RequestBuyResourceToken(int cost, int tid, int sIndex, int num)
	{
		CS_ResourceHint_BuyResourceToken buyResToken = new CS_ResourceHint_BuyResourceToken();
		buyResToken.sIndex = sIndex;
		buyResToken.num = num;
		HttpModule.Instace.SendGameServerMessage(buyResToken, (x) => { __OnRequestBuyResourceTokenSuccess(cost, tid, x); }, __OnRequestBuyResourceTokenFailed);
	}

	private static void __OnRequestBuyResourceTokenSuccess(int cost, int tid, string text)
	{
		SC_ResourceHint_BuyResourceToken retBuyResToken = JCode.Decode<SC_ResourceHint_BuyResourceToken>(text);
		if(retBuyResToken.isBuySuccess == 1)
		{
			//减元宝
			RoleLogicData.Self.AddDiamond(-cost);
			//加物品
			for (int i = 0, count = retBuyResToken.buyItem.Length; i < count; i++)
			{
				if(retBuyResToken.buyItem[i].itemId == -1)
					PackageManager.AddItem(retBuyResToken.buyItem);
				else
					PackageManager.UpdateItem(retBuyResToken.buyItem);
			}

			//刷新资源数量显示
			DataCenter.SetData("ADD_VITALITY_UP_WINDOW","REFRESH",null);
		}
		else
			DataCenter.OpenMessageWindow("资源购买失败");
	}
	private static void __OnRequestBuyResourceTokenFailed(string text)
	{
		//TODO
	}
#endregion


#region 获得资源还可购买次数
	public static void RequestBuyResourceNumToken(int sIndex)
	{
		CS_PropShopItemQueryToken buyResToken = new CS_PropShopItemQueryToken();
		buyResToken.index = sIndex;
		HttpModule.Instace.SendGameServerMessage(buyResToken, (x) => { __OnRequestBuyResourceNumTokenSuccess(x); }, __OnRequestBuyResourceNumTokenFailed);
	}

	private static void __OnRequestBuyResourceNumTokenSuccess(string text)
	{

		SC_PropShopItemQueryToken retBuyResToken = JCode.Decode<SC_PropShopItemQueryToken>(text);
		if (retBuyResToken.prop == null) 
		{
			DEBUG.LogError("NULL");
		}

		DataCenter.SetData ("ADD_VITALITY_UP_WINDOW","ALREADY_BUY_NUM",retBuyResToken.prop);
	}

	private static void __OnRequestBuyResourceNumTokenFailed(string text)
	{

	}
#endregion


}
