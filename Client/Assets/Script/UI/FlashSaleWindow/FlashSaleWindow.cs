using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class FlashSaleWindow : FlashSaleBase
{
    public int iCurType = 1;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_flash_sale_close_Btn", new DefineFactoryLog<Button_flash_sale_close_Btn>());
        EventCenter.Self.RegisterEvent("Button_flash_sale_window_bg_btn", new DefineFactoryLog<Button_flash_sale_close_Btn>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        if(param is int)
        {
            iCurType = (int)param;
        }
    }

    public void InitUI(int type)
    {
        RefreshCurrentSale(type);
    }

    public void ShowCurrentSale(int type)
    {
        if (type == (int)FLASH_SALE_TYPE.FLASH_SALE_SINGLE)
        {
            GetSub("multi_sale").SetActive(false);
            GetSub("single_sale").SetActive(true);
        }
        else if (type == (int)FLASH_SALE_TYPE.FLASH_SALE_MULTI)
        {
            GetSub("multi_sale").SetActive(true);
            GetSub("single_sale").SetActive(false);
        }
    }

    public void RefreshCurrentSale(int type)
    {
        //show
        ShowCurrentSale(type);

        //refresh
        if (type == (int)FLASH_SALE_TYPE.FLASH_SALE_SINGLE)
        {
            UpdateSingleInfo(GetSaleList());
        }
        else if (type == (int)FLASH_SALE_TYPE.FLASH_SALE_MULTI)
        {
            UpdateMultiInfo(GetSaleList());
        }
    }

    public void UpdateSingleInfo(SC_GetFlashSaleList data)
    {
        if (data.flashSaleList.Length > 0 && data.flashSaleList[0] != null)
        {
            //get single obj and data
            GameObject singleObj = GameCommon.FindObject(mGameObjUI, "single_sale");
            FlashSaleItem item = data.flashSaleList[0];

            //purchase-button
            GameObject purchaseObj = GameCommon.FindObject(singleObj, "single_flash_sale_Btn");
            AddButtonAction(purchaseObj, () =>
            {
                //--加判断-消耗品是否足够
                if (RoleLogicData.Self.diamond <= GetPriceNum(GetIndex(item), 1))
                {
                    GameCommon.ToGetDiamond();
                    return;
                }
                FlashSaleWindowNetManager.RequestFlashPurchase(GetIndex(item));
            });

            //complete
            GameCommon.FindObject(singleObj, "single_flash_sale_Btn").SetActive(GetState(item) == 0);
            GameCommon.FindObject(singleObj, "completed").SetActive(GetState(item) == 1);
            GameCommon.FindObject(singleObj, "times").SetActive(GetState(item) == 0);
            GameCommon.FindObject(singleObj, "single_flash_sale_Btn").GetComponent<UIImageButton>().isEnabled = (GetState(item) == 0);

            //price-old-now
            ShowPrice(GameCommon.FindObject(singleObj, "old_price"), data.flashSaleList[0], 0);
            ShowPrice(GameCommon.FindObject(singleObj, "now_price"), data.flashSaleList[0], 1);

            //remained time
            SetCountdown(singleObj, "times", GetPutAwayTime(item) + GetLast(GetIndex(item)), new CallBack(this, "RefreshSingleUI", singleObj));

            //uicontainer
            List<ItemDataBase> list = GetItemList(GetIndex(item));
            UIGridContainer grid = GameCommon.FindObject(singleObj, "grid_single").GetComponent<UIGridContainer>();
            grid.MaxCount = list.Count;
            var gridList = grid.controlList;
            for (int i = 0; i < list.Count; i++)
            {
                RefreshSingleBoard(gridList[i], list[i]);
            }
        }
    }

    public void RefreshSingleUI(object obj)
    {
        if (obj is GameObject)
        {
            GameObject item = (GameObject)obj;
            if (item != null)
            {
                GameCommon.FindObject(item, "single_flash_sale_Btn").GetComponent<UIImageButton>().isEnabled = false;
            }
        }

        //时间到
        Debug.Log("time is over!");
    }

    public void RefreshSingleBoard(GameObject board, ItemDataBase itemDataBase)
    {
        GameCommon.SetOnlyItemIcon(board, "icon", itemDataBase.tid);
        GameCommon.SetUIText(board, "num", "x" + itemDataBase.itemNum.ToString());
        GameCommon.SetUIText(board, "name", GameCommon.GetItemName(itemDataBase.tid));
    }


    public void UpdateMultiInfo(SC_GetFlashSaleList data)
    {
        //get obj
        GameObject obj = GameCommon.FindObject(mGameObjUI, "multi_sale");

        //desc_body
        GameCommon.SetUIText(obj, "desc_body", TableCommon.getStringFromStringList(STRING_INDEX.FLASH_SALE_DESC_BODY));

        //grid
        for (int i = 0; i < 3; i++)
        {
            string gridName = "group(Clone)_" + i.ToString();
            GameObject board = GameCommon.FindObject(obj, gridName);
            if (board != null && data.flashSaleList[i] != null)
            {
                RefreshBoard(board, data.flashSaleList[i]);
            }
        }
        
    }

    public void RefreshBoard(GameObject board, FlashSaleItem item)
    {
        //index
        int index = GetIndex(item);

        //check-button
        GameObject checkObj = GameCommon.FindObject(board, "flash_sale_click_tocheck_btn");
        AddButtonAction(checkObj, () =>
        {
            DataCenter.OpenWindow(UIWindowString.flash_sale_details_window, index);
        });

        //pruchase-button
        GameObject purchaseObj = GameCommon.FindObject(board, "multi_flash_sale_Btn");
        AddButtonAction(purchaseObj, () =>
        {
            //--加判断-消耗品是否足够
            if (RoleLogicData.Self.diamond <= GetPriceNum(GetIndex(item), 1))
            {
                GameCommon.ToGetDiamond();
                return;
            }
            FlashSaleWindowNetManager.RequestFlashPurchase(index);
        });

        //complete
        GameCommon.FindObject(board, "multi_flash_sale_Btn").SetActive(GetState(item) == 0);
        GameCommon.FindObject(board, "completed").SetActive(GetState(item) == 1);
        GameCommon.FindObject(board, "times").SetActive(GetState(item) == 0);
        GameCommon.FindObject(board, "multi_flash_sale_Btn").GetComponent<UIImageButton>().isEnabled = (GetState(item) == 0);
        
        //name
        GameCommon.SetUIText(board, "name", GetTitle(index));

        //icon
        GameObject iconObj = GameCommon.FindObject(board, "flash_sale_click_tocheck_btn");
        GameCommon.SetIcon(iconObj.GetComponent<UISprite>(), GetIconAtlasName(index), GetIconSpriteName(index));

        //remained time
        SetCountdown(board, "times", GetPutAwayTime(item)+ GetLast(index), new CallBack(this, "RefreshUI", board));

        //price-old-now
        ShowPrice(GameCommon.FindObject(board, "old_price"), item, 0);
        ShowPrice(GameCommon.FindObject(board, "now_price"), item, 1);
    }

    public void ShowPrice(GameObject price, FlashSaleItem item, int front)
    {
        GameCommon.SetItemIconNew(price, "sprite", GetPriceIconTid(item), false);
        GameCommon.SetUIText(price, "label", GetPriceNumStr(GetIndex(item), front));
    }

    public void RefreshUI(object obj)
    {
        if (obj is GameObject)
        {
            GameObject item = (GameObject)obj;
            if (item != null)
            {
                GameCommon.FindObject(item, "multi_flash_sale_Btn").GetComponent<UIImageButton>().isEnabled = false;
            }
        }

        //时间到
        Debug.Log("time is over!");
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "FLASH_SALE_INIT":
                {
                    InitUI(iCurType);
                }
                break;
            case "FLASH_SALE_RERESH":
                {
                    if (objVal is SC_FlashPurchase)
                    {
                        SC_FlashPurchase flashPurchase = (SC_FlashPurchase)objVal;
                        DataCenter.CloseWindow("AWARDS_TIPS_WINDOW");
                        List<ItemDataBase> itemList = PackageManager.UpdateItem(flashPurchase.rewards);
                        DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemList.ToArray());
                    }
                    InitUI(iCurType);
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

public class Button_flash_sale_close_Btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.flash_sale_window);
        DataCenter.SetData("ROLE_SEL_TOP_RIGHT_GROUP", "UPDATE_FLASH_SALE", null);
        return true;
    }
}





