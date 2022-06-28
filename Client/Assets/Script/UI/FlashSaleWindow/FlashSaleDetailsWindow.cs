using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class FlashSaleDetailsWindow : FlashSaleBase
{
    public int iIndex = 0;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_flash_sale_details_close_Btn", new DefineFactoryLog<Button_flash_sale_details_close_Btn>());
        EventCenter.Self.RegisterEvent("Button_flash_sale_detail_ok_Btn", new DefineFactoryLog<Button_flash_sale_detail_ok_Btn>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        if(param is int)
        {
            iIndex = (int)param;
            InitUI();
        }
    }

    public void InitUI()
    {
        //uicontainer
        List<ItemDataBase> list = GetItemList(iIndex);
        UIGridContainer grid = GetUIGridContainer("grid");
        grid.MaxCount = list.Count;
        var gridList = grid.controlList;
        for (int i = 0; i < list.Count; i++)
        {
            RefreshBoard(gridList[i], list[i]);
        }
    }

    public void RefreshBoard(GameObject board, ItemDataBase itemDataBase)
    {
        GameCommon.SetOnlyItemIcon(board, "icon", itemDataBase.tid);
        GameCommon.SetUIText(board, "num", "x" + itemDataBase.itemNum.ToString());
        GameCommon.SetUIText(board, "name", GameCommon.GetItemName(itemDataBase.tid));
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "INITUI":
                {

                }
                break;

            default:
                break;
        }
    }

    public override void Close()
    {
        base.Close();

    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        return true;
    }
}

public class Button_flash_sale_details_close_Btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.flash_sale_details_window);
        return true;
    }
}

public class Button_flash_sale_detail_ok_Btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.flash_sale_details_window);
        return true;
    }
}

