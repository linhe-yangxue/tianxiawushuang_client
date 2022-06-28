using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class ArenaListWindow : ArenaBase
{
    public List<List<ItemDataBase> > iItemListList = new List<List<ItemDataBase>>();
    UIGridContainer iGrid = null;
    public int iNum = 0; 

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_arena_list_close_button", new DefineFactoryLog<Button_arena_list_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        //initdata
        iNum = 0;
        iItemListList.Clear();
		UIScrollView mgrid = GameCommon.FindObject (mGameObjUI, "grab_fight_scrollview").GetComponent<UIScrollView>();
		if (mgrid != null)
			mgrid.ResetPosition ();
        ArenaNetManager.RequestArenaSweep();
    }

    public void InitData(SC_ArenaSweep arenaSweep)
    {
        List<ItemDataBase> itemList = new List<ItemDataBase>();
        //是物品这里刷新
        PackageManager.UpdateItem(arenaSweep.rewards);

        //刷新arenamain
        RefreshArenaMainReputation();

        for (int i = 0; i < arenaSweep.addItems.Length; i++)
        {
			itemList.Add(arenaSweep.addItems[i]);
        }
        iItemListList.Add(itemList);
    }

    public void RefreshArenaMainReputation()
    {
        DataCenter.SetData(UIWindowString.arena_main_window, "REFRESH_MY_REPUTATION", null);
    }

    public void InitUI(SC_ArenaSweep arenaSweep)
    {
        //scrollview
        iGrid = GetUIGridContainer("grab_fight_grid");
        iGrid.MaxCount = iItemListList.Count;
        var gridList = iGrid.controlList;
       
        for (int i = 0; i < iItemListList.Count; i++)
        {
            if (iItemListList[i] != null)
            {
                RefreshBoard(gridList[i], iItemListList[i]);
                //第几次
                GameCommon.SetUIVisiable(gridList[i], "time_label0" + (i + 1).ToString(), true);
            }
        }

        //处理自动滚动
        if (iNum >= 3)
        {
            UIScrollView uiScrollView = GameCommon.FindComponent<UIScrollView>(mGameObjUI, "grab_fight_scrollview");
            DoLater(() =>
            {
                uiScrollView.SetDragAmount(0, 1.0f, false);
            }, 0.1f);
        }
    }

    public void setItemIcon(GameObject board, ItemDataBase item)
    {
        GameObject obj = GameCommon.FindObject(board, "item_icon");
        GameCommon.SetItemIconNew(board, "item_icon", item.tid);
        GameCommon.SetUIText(obj, "num_label", item.itemNum.ToString());
    }

    public void setItemNum(GameObject obj,ItemDataBase item)
    {
        GameCommon.SetUIText(obj, "num_label", item.itemNum.ToString());
    }

    public void RefreshBoard(GameObject board, List<ItemDataBase> itemList)
    {
        //set奖励吧 - -
        for (int i = 0; i < itemList.Count; i++)
        {
            if(itemList[i] != null && i != 3)
            {
                ITEM_TYPE type = PackageManager.GetItemTypeByTableID(itemList[i].tid);
                switch(type)
                {
                    case ITEM_TYPE.REPUTATION:
                        {
                            GameObject objPres = GameCommon.FindObject(board, "get_pres");
                            setItemNum(objPres, itemList[i]);
                            break;
                        }
                    case ITEM_TYPE.CHARACTER_EXP:
                        {
                            GameObject objPres = GameCommon.FindObject(board, "get_exp");
                            setItemNum(objPres, itemList[i]);
                            break;
                        }
                    case ITEM_TYPE.GOLD:
                        {
                            GameObject objPres = GameCommon.FindObject(board, "get_coin");
                            setItemNum(objPres, itemList[i]);
                            break;
                        }
                }
            }
        }
        if (itemList.Count > 3)
        {
            setItemIcon(board, itemList[3]);
        }
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "ARENA_LIST_INITUI":
                if(objVal is SC_ArenaSweep)
                {
                    InitData((SC_ArenaSweep)objVal);
                    InitUI((SC_ArenaSweep)objVal);

                    DoLater(() =>
                    {
                        iNum++;
                        if (iNum < 5)
                        {
                            ArenaNetManager.RequestArenaSweep();
                        }
                    }, 0);

                }
                break;

            default:
                break;
        }
    }

    public IEnumerator _DoLater(Action action, float delay)
    {
        if (action != null)
        {
            yield return new WaitForSeconds(delay);
            action();
        }
    }

    public override void Close()
    {
        base.Close();

        //iCoroutineOperation.StopCoroutine();
    }

    public static void CloseAllWindow()
    {

    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        return true;
    }
}

public class Button_arena_list_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.arena_list_window);
        return true;
    }
}

