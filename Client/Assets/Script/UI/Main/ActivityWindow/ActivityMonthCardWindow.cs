using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable;
using System.Collections.Generic;
using Utilities;

public class ActivityMonthCardWindow:tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_cheap_get_button", new DefineFactory<Button_cheap_get_button>());
        EventCenter.Self.RegisterEvent("Button_cheap_buy_button", new DefineFactory<Button_cheap_buy_button>());
        EventCenter.Self.RegisterEvent("Button_expensive_get_button", new DefineFactory<Button_expensive_get_button>());
        EventCenter.Self.RegisterEvent("Button_expensive_buy_button", new DefineFactory<Button_expensive_buy_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);
        NetManager.RequestMonthCardQuery();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "HAVE_CARD":
                haveCard((string)objVal);
                RefreshNewMark();
                break;
            case "GET_REWARD":
                getReward((string)objVal);
                NetManager.RequestMonthCardQuery();
                break;
            case "BUY_CARD":
                break;
        }
    }

    private void RefreshNewMark() 
    {
        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVE_DAILY_REWARD_FOR_MONTH);
    }
	void haveCard(string text)
    {
        bool mhaveCard = false;
        GameObject monthCardWindow = GameObject.Find("activity_month_card_window");
        GameObject cheap_get=GameCommon.FindObject(monthCardWindow,"cheap_get_button");
        UIImageButton cheap_get_button=cheap_get.GetComponent<UIImageButton>();
        UILabel cheap_label = GameCommon.FindObject(cheap_get, "get_label").GetComponent<UILabel>();
        GameObject cheap_buy=GameCommon.FindObject(monthCardWindow,"cheap_buy_button");
        GameObject expensive_get=GameCommon.FindObject(monthCardWindow,"expensive_get_button");
        UIImageButton expensive_get_button=expensive_get.GetComponent<UIImageButton>();
        UILabel expensive_label = GameCommon.FindObject(expensive_get, "get_label").GetComponent<UILabel>();
        GameObject expensive_buy=GameCommon.FindObject(monthCardWindow,"expensive_buy_button");
        UILabel cheap_day = GameCommon.FindObject(monthCardWindow, "CheapDay").GetComponent<UILabel>();
        UILabel expensive_day = GameCommon.FindObject(monthCardWindow, "ExpensiveDay").GetComponent<UILabel>();
        SC_MonthCardQuery ihave = JCode.Decode<SC_MonthCardQuery>(text);
        int _doNotBuyCount = 0;
        if (ihave.haveCheapCard)
        {
            cheap_buy.SetActive(false);
            cheap_get.SetActive(true);
            mhaveCard = true;
            cheap_day.fontSize = 24;
            cheap_day.text = "剩余" + ihave.cheapLeft + "天";
        }
        else
        {
            cheap_buy.SetActive(true);
            cheap_get.SetActive(false);
            _doNotBuyCount++;
        }

        if (ihave.haveExpensiveCard)
        {
            expensive_buy.SetActive(false);
            expensive_get.SetActive(true);
            mhaveCard = true;
            expensive_day.fontSize = 24;
            expensive_day.text = "剩余" + ihave.expensiveLeft + "天";
        }
        else
        {
            expensive_buy.SetActive(true);
            expensive_get.SetActive(false);
            _doNotBuyCount++;
        }

        if (mhaveCard)
        {
            bool canGetCheapCardReward=true;
            bool canGetExpensiveCardReward = true;
            foreach (int canget in ihave.cardArr)
            {
                if (canget == 1)
                {
                    canGetCheapCardReward = false;
                }
                else if (canget == 2)
                {
                    canGetExpensiveCardReward = false;
                }
            }
            //added by xuke 刷新红点
            int _hasGotCount = 0;
            //end
            if (canGetCheapCardReward)
            {
               cheap_label.text= "领取";
               cheap_get_button.isEnabled=true;
            }
            else
            {
               cheap_label.text= "已领取";
               cheap_get_button.isEnabled=false;
               _hasGotCount++;
            }

            if (canGetExpensiveCardReward)
            {
               expensive_label.text= "领取";
               expensive_get_button.isEnabled=true;
            }
            else
            {
               expensive_label.text = "已领取";
               expensive_get_button.isEnabled=false;
               _hasGotCount++;
            }
            //added by xuke
            if (_hasGotCount + _doNotBuyCount == 2) 
            {
                SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_MONTH_CARD,false);
            }
            //end
        }

    }

    void getReward(string text)
    {
        SC_MonthCardReward mreward = JCode.Decode<SC_MonthCardReward>(text);
        string tips = "成功领取" + mreward.diamond.ToString()+"元宝";
        ItemDataBase mCardReward=new ItemDataBase();
        mCardReward.tid =(int)ITEM_TYPE.YUANBAO;
        mCardReward.itemNum=mreward.diamond;
        PackageManager.AddItem(mCardReward);
        DataCenter.OpenMessageWindow(tips);
    }
}

public class Button_cheap_get_button : CEvent
{
    public override bool _DoEvent()
    {
        NetManager.RequestMonthCardReward(0);
        return true;
    }
}

public class Button_cheap_buy_button : CEvent
{
    public override bool _DoEvent()
    {
        GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, () =>
        {
            NetManager.RequestMonthCardQuery();
        }, CommonParam.rechageDepth);
        return true;
    }
}

public class Button_expensive_get_button : CEvent
{
    public override bool _DoEvent()
    {
        NetManager.RequestMonthCardReward(1);
        return true;
    }
}

public class Button_expensive_buy_button : CEvent
{
    public override bool _DoEvent()
    {
        GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, () => {
            NetManager.RequestMonthCardQuery();
        }, CommonParam.rechageDepth);
        return true;
    }
}
