using UnityEngine;
using System.Collections;

public class ShopNetEvent {

	public static void RequestLotteryQuery(HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestLotteryQuery lq = new CS_RequestLotteryQuery();

		HttpModule.Instace.SendGameServerMessage(lq, "CS_LotteryQuery", success, fail);
	}

	public static void RequestNormalLottery(ItemDataBase input, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestNormalLottery nl = new CS_RequestNormalLottery();
		nl.consume = input;
		HttpModule.Instace.SendGameServerMessage(nl, "CS_NormalLottery", success, fail);
	}

	public static void RequestTenNormalLottery(ItemDataBase input, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestTenNormalLottery tl = new CS_RequestTenNormalLottery();
		tl.consume = input;
		HttpModule.Instace.SendGameServerMessage(tl, "CS_TenNormalLottery", success, fail);
	}

	public static void RequestPreciousLottery(ItemDataBase input, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestPreciousLottery pl = new CS_RequestPreciousLottery();
		pl.consume = input;
		HttpModule.Instace.SendGameServerMessage(pl, "CS_PreciousLottery", success, fail);
	}

	public static void RequestTenPreciousLottery(ItemDataBase input, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestTenPreciousLottery tpl = new CS_RequestTenPreciousLottery();
		tpl.consume = input;
		HttpModule.Instace.SendGameServerMessage(tpl, "CS_TenPreciousLottery", success, fail);
	}

	public static void RequestPropShopQuery(HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestPropShopQuery query = new CS_RequestPropShopQuery();
		HttpModule.Instace.SendGameServerMessage(query, "CS_PropShopQuery", success, fail);
	}

	public static void RequestShopPurchase(int index, int num, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestShopPurchase purchase = new CS_RequestShopPurchase();
		purchase.sIndex = index;
		purchase.num = num;
		HttpModule.Instace.SendGameServerMessage(purchase, "CS_PropShopPurchase", success, fail);
	}

	public static void RequestVipShopPurchase(int index, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestVipShop vip = new CS_RequestVipShop();
		vip.index = index;
		HttpModule.Instace.SendGameServerMessage(vip, "CS_VIPShop", success, fail);
	}

}
