//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18444
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------
using System;

public class NewMysteriousShopNet
{
	public static void RequestMysteriousShopQuery(HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_MysteryShopQuery msq = new CS_MysteryShopQuery();
		HttpModule.Instace.SendGameServerMessage(msq, "CS_MysteryShopQuery", success, fail);
	}

	public static void RequestMysteriousShopRefresh(ItemDataBase sxl, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_MysteryShopRefresh msr = new CS_MysteryShopRefresh();
		msr.consume = sxl;
		HttpModule.Instace.SendGameServerMessage(msr, "CS_MysteryShopRefresh", success, fail);
	}

	public static void RequestMysterousShopPurchase(int idx, int num, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_MysteryShopPurchase msp = new CS_MysteryShopPurchase();
		msp.indexNum = idx;
		msp.num = num;
		HttpModule.Instace.SendGameServerMessage(msp, "CS_MysteryShopPurchase", success, fail);
	}

}

