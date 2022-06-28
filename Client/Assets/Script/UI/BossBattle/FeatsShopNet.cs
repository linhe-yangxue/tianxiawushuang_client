using UnityEngine;
using System.Collections;


public class FeatsShopNet
{
	public static void RequestFeatsShopQuery(HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestFeatsShopQuery rfsq = new CS_RequestFeatsShopQuery();
		HttpModule.Instace.SendGameServerMessage(rfsq, "CS_DeamonShopQuery", success, fail);
	}

	public static void RequestFeatsShopPurchase(int idx, int num,int[] itemIdArr, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestFeatsShopPurchase rfsp = new CS_RequestFeatsShopPurchase();
		rfsp.indexNum = idx;
		rfsp.num = num;
		rfsp.itemIdArr = itemIdArr;
		HttpModule.Instace.SendGameServerMessage(rfsp, "CS_DeamonShopPurchase", success, fail);
	}

}

