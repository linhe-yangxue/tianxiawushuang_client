using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class RammbockListWindow : RammbockBase
{
    public UIGridContainer iGrid = null;
    public List<ItemDataBase> iList = new List<ItemDataBase>();
    public int iNum = 0;
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_rammbock_list_complete_button", new DefineFactoryLog<Button_rammbock_list_complete_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        GameCommon.SetWindowZ(mGameObjUI, -1000);

        iList.Clear();
        UIScrollView mgrid = GameCommon.FindObject(mGameObjUI, "rewards_scrollview").GetComponent<UIScrollView>();
        if (mgrid != null)
            mgrid.ResetPosition();

        List<RammbockAttriBuffData> tmpBuffData = RammbockWindow.GetCurrentValidBuffData();
        ClimbTowerBuffInfo[] tmpBuffInfo = new ClimbTowerBuffInfo[tmpBuffData != null ? tmpBuffData.Count : 0];
        for (int i = 0, count = tmpBuffData.Count; i < count; i++)
        {
            tmpBuffInfo[i] = new ClimbTowerBuffInfo();
            tmpBuffInfo[i].buffType = tmpBuffData[i].m_affectType.ToString();
            tmpBuffInfo[i].buffValue =
                tmpBuffData[i].m_isRate ?
                Mathf.FloorToInt(tmpBuffData[i].m_affectValue * 100.0f) :
                Convert.ToInt32(tmpBuffData[i].m_affectValue);
        }
        RammbockNetManager.RequestClimbTowerSweep(tmpBuffInfo);
    }

    public void InitUI()
    {
        SetScrollView();
        SetBotton();
        SetBottonActive(false);
    }

    public void SetBottonActive(bool active)
    {
        GetSub("bottom").SetActive(active);
    }

    public void SetBotton()
    {
        //pos
        GetSub("bottom").transform.localPosition = GetRewardPos();

        //data
        ItemDataBase[] itemArr = GettierPassRewards();
        GameObject objBottom = GetSub("bottom");

        //tips
        string str = TableCommon.getStringFromStringList(STRING_INDEX.RAMMBOCK_GET_STAR_TIPS);
        int remainedGuanka = GetRemainedGuan();
        int guanka = GetGuanka();
        string strShow = string.Format(str, guanka.ToString(), (guanka + remainedGuanka - 1).ToString(), (remainedGuanka * 3).ToString());
        GameCommon.SetUIText(objBottom, "tip_label", strShow);

        //item
        SetUIGridReward(itemArr);
    }

    public void SetUIGridReward(ItemDataBase[] itemArr)
    {
        if (itemArr != null)
        {
            var grid = GetUIGridContainer("Grid_rewards");
            grid.MaxCount = itemArr.Length;
            var gridList = grid.controlList;
            for (int i = 0; i < itemArr.Length; i++)
            {
                if (itemArr[i] != null)
                {
                    RefreshBoardReward(gridList[i], itemArr[i]);
                }
            }
        }
    }

    public void RefreshBoardReward(GameObject board, ItemDataBase item)
    {
        setItemIcon(board, item);
    }

    public void SetScrollView()
    {
        //getdata
        int[] awardIndexes = GetAwardIndexes();

        //refresh
        if (awardIndexes != null)
        {
            iGrid = GetUIGridContainer("Grid");
            iGrid.MaxCount = awardIndexes.Length;
            var gridList = iGrid.controlList;
            for (int i = 0; i < awardIndexes.Length; i++)
            {
                gridList[i].SetActive(false);
            }
            for (int i = 0; i < awardIndexes.Length; i++)
            {
                if (awardIndexes[i] != null)
                {
                    int temp = i;
                    gridList[temp].SetActive(true);
                    RefreshBoard(gridList[temp], awardIndexes[temp], temp);
                }
            }
        }

        //一个一个出来
        DoCoroutine(ShowItemsInTurn(iGrid));
    }

    private IEnumerator ShowItemsInTurn(UIGridContainer container)
    {
        foreach (var item in container.controlList)
        {
            item.SetActive(false);
        }

        int index = 0;
        foreach (var item in container.controlList)
        {
            item.SetActive(true);
            container.Reposition();
            index++;
            yield return new WaitForSeconds(1.0f);
        }

        if (index == container.MaxCount)
        {
            SetBottonActive(true);
            UIScrollView uiScrollView = GameCommon.FindComponent<UIScrollView>(mGameObjUI, "rewards_scrollview");
            if (uiScrollView != null)
            {
                yield return new WaitForSeconds(1.0f);
                uiScrollView.SetDragAmount(0, 1.0f, false);
            }
        }
    }

    public void setItemIcon(GameObject board, ItemDataBase item)
    {
        GameObject obj = GameCommon.FindObject(board, "item_icon");
        GameCommon.SetItemIconNew(board, "item_icon", item.tid);
        GameCommon.SetUIText(obj, "count_label", item.itemNum.ToString());
        GameCommon.SetUIText(obj, "name_label", GameCommon.GetItemName(item.tid));
		GameObject suipian = GameCommon.FindObject (board, "FragmentMask");
		if (suipian != null) {
			GameCommon.SetUIVisiable (obj, "FragmentMask", GameCommon.CheckIsFragmentByTid (item.tid) || GameCommon.CheckIsExpMagicEquip (item.tid));
		}
    }

    public void setItemNum(GameObject obj,ItemDataBase item)
    {
        GameCommon.SetUIText(obj, "num_label", item.itemNum.ToString());
    }

    public void RefreshBoard(GameObject board, int awardIndex, int i)
    {
        //第几关
        string str = TableCommon.getStringFromStringList(STRING_INDEX.RAMMBOCK_GUANKA_NUM);
        string strShow = string.Format(str, (i + GetGuanka()).ToString());
        GameCommon.SetUIText(board, "time_label", strShow);

        //获取奖励显示
        GameCommon.SetUIText(board, "token_num_label", GetEquipToken(i + GetGuanka(), awardIndex).ToString());
        GameCommon.SetUIText(board, "token_name_label", GetCrikeText(awardIndex).ToString());
        GameCommon.SetUIText(board, "coin_num_label", GetPrice(i + GetGuanka(), awardIndex).ToString());
        GameCommon.SetUIText(board, "coin_name_label", GetCrikeText(awardIndex).ToString());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "INIT_UI":
                {
                    InitUI();
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

    public static void CloseAllWindow()
    {

    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        return true;
    }
}

public class Button_rammbock_list_complete_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.rammbock_list_window);

        SC_ClimbTowerSweep sweepData = RammbockBase.GetClimbData();
        if (sweepData.chooseBuff != null && sweepData.chooseBuff.Length > 0)
        {
            //直接打开加属性界面
            DataCenter.OpenWindow("RAMMBOCK_ATTRI_ADD_WINDOW", sweepData.chooseBuff);
        }
        return true;
    }
}

