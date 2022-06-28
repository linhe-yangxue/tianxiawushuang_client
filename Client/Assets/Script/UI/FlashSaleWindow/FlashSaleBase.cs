using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using DataTable;


public abstract class FlashSaleBase : tWindow
{
    public static SC_GetFlashSaleList iSaleList;

    public enum FLASH_SALE_TYPE
    {
        FLASH_SALE_MULTI = 1,
        FLASH_SALE_SINGLE

    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
    }

    public static void InitSaleList(SC_GetFlashSaleList data)
    {
        iSaleList = data;
    }

    public static SC_GetFlashSaleList GetSaleList()
    {
        return iSaleList;
    }

    public static void RefreshSaleList(int index)
    {
        if (iSaleList != null)
        {
            for (int i = 0; i < iSaleList.flashSaleList.Length; i++)
            {
                if (iSaleList.flashSaleList[i] != null && iSaleList.flashSaleList[i].index == index)
                {
                    iSaleList.flashSaleList[i].state = 1;
                }
            }
        }
    }

    public static int GetIndex(FlashSaleItem item)
    {
        return item.index;
    }

    public static long GetPutAwayTime(FlashSaleItem item)
    {
        return item.putawayTime;
    }
    
    public static int GetState(FlashSaleItem item)
    {
        return item.state;
    }

    public static int GetPriceIconTid(FlashSaleItem item)
    {
        return 1000001;
    }

    public static string GetPriceNumStr(int index, int front)
    {
        string result = "";
        string ret = TableCommon.GetStringFromConfig(index, "PRICE", DataCenter.mFlashSaleEvent);
        string[] temp = ret.Split('#');
        if (temp.Length > 1)
        {
             result = "x" + temp[front];
        }
        return result;
    }

    public static int GetPriceNum(int index, int front)
    {
        string result = "";
        string ret = TableCommon.GetStringFromConfig(index, "PRICE", DataCenter.mFlashSaleEvent);
        string[] temp = ret.Split('#');
        if (temp.Length > 1)
        {
            result = temp[front];
        }
        return int.Parse(result);
    }

    public static string GetTitle(int index)
    {
        string ret = TableCommon.GetStringFromConfig(index, "TITLE", DataCenter.mFlashSaleEvent);
        return ret;
    }

    public static FLASH_SALE_TYPE GetType(int index)
    {
        int ret = TableCommon.GetNumberFromConfig(index, "TYPE", DataCenter.mFlashSaleEvent);
        return (FLASH_SALE_TYPE)ret;
    }

    public static int GetLast(int index)
    {
        int ret = TableCommon.GetNumberFromConfig(index, "LAST", DataCenter.mFlashSaleEvent);
        return ret;
    }

    public static string GetIconAtlasName(int index)
    {
        string ret = TableCommon.GetStringFromConfig(index, "ICON_ATLAS_NAME", DataCenter.mFlashSaleEvent);
        return ret;
    }

    public static string GetIconSpriteName(int index)
    {
        string ret = TableCommon.GetStringFromConfig(index, "ICON_SPRITE_NAME", DataCenter.mFlashSaleEvent);
        return ret;
    }

    public static List<ItemDataBase> GetItemList(int index)
    {
        List<ItemDataBase> list = new List<ItemDataBase>();
        string ret = TableCommon.GetStringFromConfig(index, "ITEM_LIST", DataCenter.mFlashSaleEvent);
        string[] temp = ret.Split('|');
        for (int i = 0; i < temp.Length; i++)
        {
            string[] str = temp[i].Split('#');
            if (str.Length > 1)
            {
                list.Add(new ItemDataBase() { tid = int.Parse(str[0]), itemNum = int.Parse(str[1]) });
            }
        }
        return list;
    }

    //根据等级和vip等级判断是否有活动
    public static bool CheckFlashSale(FLASH_SALE_TYPE type)
    {
        foreach (var record in DataCenter.mFlashSaleEvent.GetAllRecord())
        {
            string condition = record.Value["CONDITION"];
            int saleType = record.Value["TYPE"];
            string[] temp = condition.Split('#');
            if (temp.Length > 2)
            {
                int level = RoleLogicData.GetMainRole().level;
                int vipLevel = RoleLogicData.Self.vipLevel;

                int levelType = int.Parse(temp[0]);
                int frontNum = int.Parse(temp[1]);
                int afterNum = int.Parse(temp[2]);
                if (saleType == (int)type)
                {
                    if (levelType == 1 && level >= frontNum && level <= afterNum)
                    {
                        return true;
                    }
                    if (levelType == 2 && vipLevel >= frontNum && vipLevel <= afterNum)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    //时间到了 --
    public static bool IsTimeOver(FLASH_SALE_TYPE type)
    {
        //请教策划喽- -

        return true;
    }

    //消除红点
    public static void RemoveNewMarkState(FLASH_SALE_TYPE type)
    {
        bool result = false;
        if (iSaleList != null)
        {
            if (type == FLASH_SALE_TYPE.FLASH_SALE_MULTI)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (iSaleList.flashSaleList[i] != null && iSaleList.flashSaleList[i].state == 0)
                    {
                        result = true;
                    }
                }
                SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_FlASH_SALE_MULTI, result);
            }
            if (type == FLASH_SALE_TYPE.FLASH_SALE_SINGLE)
            {
                if (iSaleList.flashSaleList.Length > 0 && iSaleList.flashSaleList[0] != null && iSaleList.flashSaleList[0].state == 0)
                {
                    result = true;
                }
                SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_FLASH_SALE_SINGLE, result);
            }
        }
    }
}
