using UnityEngine;
using System.Collections;

public class PVPPrestigeShopNet {

	public static void RequestPrestigeShopQuery(HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestPrestigeShopQuery rpsq = new CS_RequestPrestigeShopQuery();
		HttpModule.Instace.SendGameServerMessage(rpsq, "CS_PrestigeShopQuery", success, fail);
	}

	public static void RequestPrestigeShopPurchase(int idx, int num, int[] itemIdArr,HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestPrestigeShopPurchase rpsp = new CS_RequestPrestigeShopPurchase();
		rpsp.pIndex = idx;
		rpsp.num = num;
		rpsp.itemIdArr = itemIdArr;
		HttpModule.Instace.SendGameServerMessage(rpsp, "CS_PrestigeShopPurchase", success, fail);
	}
}
