using UnityEngine;
using System.Collections;
using DataTable;
using Logic;
using System;
using System.Collections.Generic;
using System.Linq;


public class FlashSaleWindowNetManager
{
    public static int iCompleteIndex = 0;
    public static int iCurType = 0;
    /// <summary>
    /// GetFlashSaleList
    /// </summary>
    public static void RequestGetFlashSaleList(int type)
    {
        CS_GetFlashSaleList data = new CS_GetFlashSaleList();
        data.type = type;
        iCurType = type;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestGetFlashSaleListSuccess, __OnRequestFailed);
    }
    private static void __OnRequestGetFlashSaleListSuccess(string text)
    {
        SC_GetFlashSaleList retRoleData = JCode.Decode<SC_GetFlashSaleList>(text);
        if (retRoleData != null)
        {
            FlashSaleBase.InitSaleList(retRoleData);
            DataCenter.OpenWindow(UIWindowString.flash_sale_window, iCurType);
            DataCenter.SetData(UIWindowString.flash_sale_window, "FLASH_SALE_INIT", retRoleData);
        }
    }

    /// <summary>
    /// FlashPurchase
    /// </summary>
    public static void RequestFlashPurchase(int index)
    {
        CS_FlashPurchase data = new CS_FlashPurchase();
        data.index = index;
        iCompleteIndex = index;
        HttpModule.Instace.SendGameServerMessage(data, __OnRequestFlashPurchaseSuccess, __OnRequestFailed);
    }
    private static void __OnRequestFlashPurchaseSuccess(string text)
    {
        SC_FlashPurchase retRoleData = JCode.Decode<SC_FlashPurchase>(text);
        if (retRoleData != null)
        {
            FlashSaleBase.RefreshSaleList(iCompleteIndex);
            FlashSaleBase.RemoveNewMarkState(FlashSaleBase.GetType(iCompleteIndex));
            DataCenter.SetData(UIWindowString.flash_sale_window, "FLASH_SALE_RERESH", retRoleData);
        }
    }


    //统一处理失败报错 客户端能避免，尽量避免
    private static void __OnRequestFailed(string text)
    {
        //TODO
        int ret = int.Parse(text);
        if (ret == 2102)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_BOSS_RAIN_DEAD);
            return;
        }
    }
}
