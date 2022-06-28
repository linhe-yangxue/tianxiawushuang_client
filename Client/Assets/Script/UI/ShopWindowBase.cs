using UnityEngine;
using System.Collections;
using System;

abstract class ShopWindowBase<T,K> : tWindow where T:CS_GuildShopBuy,new() where K:SC_GuildShopBuy
{

    //protected void Buy(int gIndex, int num, Action refresh) {
    //    T cs = new T();
    //    cs.gIndex = gIndex;
    //    cs.num = num;

    //    HttpModule.CallBack requestSuccess = (text) => {
    //        K item = JCode.Decode<K>(text);
    //        PackageManager.AddItem(item.arr);

    //        refresh();
    //    };
    //    HttpModule.Instace.SendGameServerMessageT<T>(cs, requestSuccess, NetManager.RequestFail);
    //}

    //protected Action GetBuyAction(int gIndex,int num,Action refresh) {
    //    return () => {
    //        T cs = new T();
    //        cs.gIndex = gIndex;
    //        cs.num = num;

    //        HttpModule.CallBack requestSuccess = (text) => {
    //            K item = JCode.Decode<K>(text);
    //            refresh();
    //        };
    //        HttpModule.Instace.SendGameServerMessageT<T>(cs, requestSuccess, NetManager.RequestFail);
    //    };
    //}
}


public class BuyObject
{
    public readonly int index;
    public readonly int buyNum;

    public BuyObject(int index, int buyNum) {
        this.index = index;
        this.buyNum = buyNum;
    }

    public BuyObject() { }
}


