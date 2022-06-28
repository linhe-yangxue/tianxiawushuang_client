using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable;
using System.Collections.Generic;
using Utilities;

public class ActivityFlashSaleWindow:tWindow
{
    //added by xuke
    private int[] mCanBuyCountArr = { 0, 0, 0 }; //> 剩余购买次数
    private bool mHasNotBuy = false;             //> 还没开始购买 
    //end
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_show_inside_button1", new DefineFactory<Button_show_inside_button1>());
        EventCenter.Self.RegisterEvent("Button_show_inside_button2", new DefineFactory<Button_show_inside_button2>());
        EventCenter.Self.RegisterEvent("Button_show_inside_button3", new DefineFactory<Button_show_inside_button3>());
        EventCenter.Self.RegisterEvent("Button_buy_now_button1", new DefineFactory<Button_buy_now_button1>());
        EventCenter.Self.RegisterEvent("Button_buy_now_button2", new DefineFactory<Button_buy_now_button2>());
        EventCenter.Self.RegisterEvent("Button_buy_now_button3", new DefineFactory<Button_buy_now_button3>());
    }

    public override void Open(object param)
    {
        base.Open(param);
        NetManager.RequestLimitTimeSale();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REFRESH_UI":
                refreshUi(objVal.ToString());
                break;
            case "BUY1":
                buySuccess(objVal,1);
                break;
            case "BUY2":
                buySuccess(objVal, 2);
                break;
            case "BUY3":
                buySuccess(objVal, 3);
                break;
            
            }
    }
    private void RefreshNewMark()
    {
        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_FLASH_SALE);
    }
    void buySuccess(object objVal,int n)
    {
        SC_BuyCheapWares info = JCode.Decode<SC_BuyCheapWares>(objVal.ToString());      
        if (GameObject.Find("activity_flash_sale_result") != null)
        {
            MonoBehaviour.Destroy(GameObject.Find("activity_flash_sale_result"));
        }
        GameObject mresult_window = MonoBehaviour.Instantiate(GameCommon.FindObject(mGameObjUI, "activity_flash_sale_result"), Vector3.zero, Quaternion.identity) as GameObject;  
        mresult_window.SetActive(true);
        mresult_window.transform.parent = mGameObjUI.transform;
        mresult_window.transform.localScale = Vector3.one;
        GameCommon.SetItemIconNew(mresult_window, "result_icon", info.randWareID.tid);
        GameCommon.SetUIText(mresult_window, "result_tips", GameCommon.GetItemName(info.randWareID.tid) + "X" + info.randWareID.itemNum.ToString());
        mresult_window.name = "activity_flash_sale_result";
        //刷新剩余可购买次数，次数为0，按钮灰化，按协议加物品       
        GameObject box = GameCommon.FindObject(mGameObjUI, "box_" + n.ToString());
        int leftBuytimes = DataCenter.mLimitTimeSale.GetData(n, "BUY_NUM") - info.haveBuyNum;
        GameCommon.SetUIText(box, "times_Label", leftBuytimes.ToString());
        if (leftBuytimes > 0)
        {
            GameCommon.FindObject(box, "buy_now_button" + n.ToString()).GetComponent<UIImageButton>().isEnabled = PackageManager.IsHasEnoughItemByTid(DataCenter.mLimitTimeSale.GetData(n, "NEW_COST_TYPE"), DataCenter.mLimitTimeSale.GetData(n, "NEW_COST_NUM"));
        }
        else
        {
            GameCommon.FindObject(box, "buy_now_button" + n.ToString()).GetComponent<UIImageButton>().isEnabled = false;
        }       
        PackageManager.UpdateItem(info.randWareID);
        PackageManager.RemoveItem(DataCenter.mLimitTimeSale.GetData(n, "NEW_COST_TYPE"), -1, DataCenter.mLimitTimeSale.GetData(n, "NEW_COST_NUM"));
        for (int i = 1; i < 4; i++)
        {
            GameObject mbox = GameCommon.FindObject(mGameObjUI, "box_" + i.ToString());
            GameCommon.FindObject(mbox, "buy_now_button" + i.ToString()).GetComponent<UIImageButton>().isEnabled = PackageManager.IsHasEnoughItemByTid(DataCenter.mLimitTimeSale.GetData(i, "NEW_COST_TYPE"), DataCenter.mLimitTimeSale.GetData(i, "NEW_COST_NUM"));
        }
        GlobalModule.DoCoroutine(DestroyResult(mresult_window));
        //added by xuke       
        if (!mHasNotBuy) 
        {
            mCanBuyCountArr[n - 1]--;
            int _canBuyCountTotal = 0;
            for (int i = 0; i < 3; i++)
            {
                _canBuyCountTotal += mCanBuyCountArr[i];
            }
            if (_canBuyCountTotal <= 0)
            {
                SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_FLASH_SALE, false);
                RefreshNewMark();
            }
        }
        mHasNotBuy = false;
        //end

    }

    IEnumerator DestroyResult(GameObject mresultWindow)
    {
        yield return new WaitForSeconds(1.5f);
        if (mresultWindow != null)
        {
            MonoBehaviour.Destroy(mresultWindow);
        }
    }

    void refreshUi(string text)
    {
        SC_LimitTimeSale reply = JCode.Decode<SC_LimitTimeSale>(text);
        

        int leftBuytimes = 0;
        for (int i = 1; i < 4; i++)
        {
            GameObject box = GameCommon.FindObject(mGameObjUI, "box_" + i.ToString());
            GameCommon.FindObject(box, "buy_now_button" + i.ToString()).GetComponent<UIImageButton>().isEnabled =PackageManager.IsHasEnoughItemByTid(DataCenter.mLimitTimeSale.GetData(i, "NEW_COST_TYPE"), DataCenter.mLimitTimeSale.GetData(i, "NEW_COST_NUM"));
            GameCommon.SetItemIconNew(box, "pre_cost_icon", DataCenter.mLimitTimeSale.GetData(i, "OLD_COST_TYPE"), false);
            GameCommon.SetUIText(box, "pre_cost_num", DataCenter.mLimitTimeSale.GetData(i, "OLD_COST_NUM").ToString());
            GameCommon.SetItemIconNew(box, "now_cost_icon", DataCenter.mLimitTimeSale.GetData(i, "NEW_COST_TYPE"), false);
            GameCommon.SetUIText(box, "now_cost_num", DataCenter.mLimitTimeSale.GetData(i, "NEW_COST_NUM").ToString());
            leftBuytimes = DataCenter.mLimitTimeSale.GetData(i, "BUY_NUM");
            GameCommon.SetUIText(box, "times_Label", leftBuytimes.ToString());
        }
        mHasNotBuy = true;
        if (reply.haveBuyNum != null)
        {
            int _index = 0;
            foreach (HaveBuyNum temp in reply.haveBuyNum)
            {
                if (temp != null)
                {
                    leftBuytimes = DataCenter.mLimitTimeSale.GetData(temp.goodsIndex, "BUY_NUM") - temp.hadBuyNum;
                    //added by xuke
                    mCanBuyCountArr[_index++] = leftBuytimes;
                    //end
                    GameObject box = GameCommon.FindObject(mGameObjUI, "box_" + temp.goodsIndex.ToString());
                    GameCommon.SetUIText(box, "times_Label", leftBuytimes.ToString());
                    if (leftBuytimes <= 0)
                    {
                        GameCommon.FindObject(box, "buy_now_button" + temp.goodsIndex.ToString()).GetComponent<UIImageButton>().isEnabled = false;
                    }
                    else
                    {
                        GameCommon.FindObject(box, "buy_now_button" + temp.goodsIndex.ToString()).GetComponent<UIImageButton>().isEnabled =
                        PackageManager.IsHasEnoughItemByTid(DataCenter.mLimitTimeSale.GetData(temp.goodsIndex, "NEW_COST_TYPE"), DataCenter.mLimitTimeSale.GetData(temp.goodsIndex, "NEW_COST_NUM"));
                    }
                }
            }
        }
        else 
        {
            mHasNotBuy = true;
        }       
        string[] mtimes = DataCenter.mOpenTime.GetData(111, "Condition").ToString().Split('|');
        if (CommonParam.NowServerTime() > Int64.Parse(mtimes[0]))
        {
            if (Int64.Parse(mtimes[1]) >= CommonParam.NowServerTime())
            {
                SetCountdownTime(mGameObjUI, "activity_left_time", Int64.Parse(mtimes[1]));
            }
            else
            {
                GameCommon.SetUIText(mGameObjUI, "activity_left_time", "已结束");
            }
        }
        else
        {
            GameCommon.SetUIText(mGameObjUI, "activity_left_time", "尚未开启");
        }
    }
}

public class Button_show_inside_button1 : CEvent
{
    public override bool _DoEvent() 
    {
        GameObject tips=GameCommon.FindObject(GameObject.Find("box_1"), "tips_bg");
        tips.SetActive(!tips.activeSelf);
        if (tips.activeSelf)
        {
            GameCommon.SetUIText(tips, "tips_title", DataCenter.mLimitTimeSale.GetData(1, "NAME"));
            GameCommon.SetUIText(tips, "tips_label", DataCenter.mLimitTimeSale.GetData(1, "DESCRIBE"));
        }
        return true;
    }
}

public class Button_show_inside_button2 : CEvent
{
    public override bool _DoEvent()
    {
        GameObject tips = GameCommon.FindObject(GameObject.Find("box_2"), "tips_bg");
        tips.SetActive(!tips.activeSelf);
        if (tips.activeSelf)
        {
            GameCommon.SetUIText(tips, "tips_title", DataCenter.mLimitTimeSale.GetData(2, "NAME"));
            GameCommon.SetUIText(tips, "tips_label", DataCenter.mLimitTimeSale.GetData(2, "DESCRIBE"));
        }
        return true;
    }
}

public class Button_show_inside_button3 : CEvent
{
    public override bool _DoEvent()
    {
        GameObject tips = GameCommon.FindObject(GameObject.Find("box_3"), "tips_bg");
        tips.SetActive(!tips.activeSelf);
        if (tips.activeSelf)
        {
            GameCommon.SetUIText(tips, "tips_title", DataCenter.mLimitTimeSale.GetData(3, "NAME"));
            GameCommon.SetUIText(tips, "tips_label", DataCenter.mLimitTimeSale.GetData(3, "DESCRIBE"));
        }
        return true;
    }
}

public class Button_buy_now_button1 : CEvent
{
    public override bool _DoEvent()
    {
        NetManager.RequestBuyCheapWares(1);
        return true;
    }
}

public class Button_buy_now_button2 : CEvent
{
    public override bool _DoEvent()
    {
        NetManager.RequestBuyCheapWares(2);
        return true;
    }
}

public class Button_buy_now_button3 : CEvent
{
    public override bool _DoEvent()
    {
        NetManager.RequestBuyCheapWares(3);
        return true;
    }
}
