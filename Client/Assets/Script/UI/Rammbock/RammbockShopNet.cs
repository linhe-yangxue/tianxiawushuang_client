using UnityEngine;
using System.Collections;

public class RammbockShopNet {

	public static void RequestClothShopPurchase(int idx, int num,int[] itemIdArr, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_ClothShopPurchase csp = new CS_ClothShopPurchase();
		csp.indexNum = idx;
		csp.num = num;
		csp.itemIdArr = itemIdArr;
		HttpModule.Instace.SendGameServerMessage(csp, "CS_ClothShopPurchase", success, fail);
	}

	public static void RequestClothShopInfo(HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestClothShopQuery rcsq = new CS_RequestClothShopQuery();

		HttpModule.Instace.SendGameServerMessage(rcsq, "CS_ClothShopQuery", success, fail);
	}
}
