using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Logic;
using DataTable;
public class ActivitySingleWelfareWindow : tWindow
{
    int count;
    int index;
    long time1;
    long time2;

    UIGridContainer singleWelfareGrid;
    SingleRecharge[] singleRecharges;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_activity_single_welfare_rewards_get_btn", new DefineFactory<Button_activity_single_welfare_rewards_get_btn>());
        EventCenter.Self.RegisterEvent("Button_activity_single_welfare_go_task_btn", new DefineFactory<Button_activity_single_welfare_go_task_btn>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        NetManager.RequestSingleWelfare();

        singleWelfareGrid = GetUIGridContainer("activity_single_welfare_grid");

        GetItemCount();
        count = index;
        singleWelfareGrid.MaxCount = count;
        SetItemInfo();


    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "SINGLE_WELFARE":
                SC_GetSingleRechargeInfo item = JCode.Decode<SC_GetSingleRechargeInfo>((string)objVal);
                time1 = item.endTime;
                time2 = time1 + 24 * 60 * 60;
                Refresh(null);
                DEBUG.Log(item.singleRecharges.Length.ToString());
                singleRecharges = item.singleRecharges;

                for (int i = 0; i < singleWelfareGrid.MaxCount; i++)
                {
                    SetBtnState(i);
                }
                break;
            case "GET_REWARD":
                SC_RevSingleRechargeReward itemReward = JCode.Decode<SC_RevSingleRechargeReward>((string)objVal);
                List<ItemDataBase> itemDataList = PackageManager.UpdateItem(itemReward.rewards);
                DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemDataList);

                NetManager.RequestSingleWelfare();

                break;
        }

    }
    public override bool Refresh(object param)
    {
        UpdateTimeUI();
        return true;
    }

    void SetItemInfo()
    {
        for (int i = 0; i < singleWelfareGrid.MaxCount; i++)
        {
            UILabel descLabel = GameCommon.FindComponent<UILabel>(singleWelfareGrid.controlList[i], "desc");
            UIGridContainer rewardGrid = GameCommon.FindComponent<UIGridContainer>(singleWelfareGrid.controlList[i], "rewards_grid");

            string desc = DataCenter.mSingleRechargeEvent.GetData((i + 1).ToString(), "DESCRIBE");
            List<ItemDataBase> rewardList = GameCommon.ParseItemList(DataCenter.mSingleRechargeEvent.GetData((i + 1).ToString(), "REWARDE"));

            descLabel.text = desc;
            rewardGrid.MaxCount = rewardList.Count;
            for (int j = 0; j < rewardList.Count; j++)
            {
                var obj = rewardGrid.controlList[j];
                UILabel itemNumLabel = obj.transform.Find("item_icon/item_num").GetComponent<UILabel>();
                itemNumLabel.text = "x" + rewardList[j].itemNum.ToString();
                int tempTid = rewardList[j].tid;
                GameCommon.SetOnlyItemIcon(GameCommon.FindObject(obj, "item_icon"), tempTid);
                AddButtonAction(GameCommon.FindObject(obj, "item_icon").gameObject, () => GameCommon.SetItemDetailsWindow(tempTid));
            }
        }
    }

    void SetBtnState(int index)
    {
        GameObject btnHaveGeted = GameCommon.FindObject(singleWelfareGrid.controlList[index], "rewards_have_get_btn");
        GameObject btnGet = GameCommon.FindObject(singleWelfareGrid.controlList[index], "activity_single_welfare_rewards_get_btn");
        GameObject btnCharge = GameCommon.FindObject(singleWelfareGrid.controlList[index], "activity_single_welfare_go_task_btn");
        UILabel canBuyTimeLabel = GameCommon.FindComponent<UILabel>(singleWelfareGrid.controlList[index], "can_buy_times");

        int totalTime = DataCenter.mSingleRechargeEvent.GetData((index + 1), "REWARDE_TIME_MAX");

        btnHaveGeted.SetActive(false);
        btnGet.SetActive(false);
        btnCharge.SetActive(false);
        canBuyTimeLabel.gameObject.SetActive(false);

        bool IschangeState = false;

        if (singleRecharges.Length > 0)
        {
            int mIndex = DataCenter.mSingleRechargeEvent.GetData((index + 1), "INDEX");
            for (int i = 0; i < singleRecharges.Length; i++)
            {
                if (singleRecharges[i].index == mIndex)
                {
                    IschangeState = true;
                    if (singleRecharges[i].revCnt == singleRecharges[i].rechargeCnt)
                    {
                        if (totalTime > singleRecharges[i].rechargeCnt)
                        {
                            btnCharge.SetActive(true);
                        }
                        else
                        {
                            btnHaveGeted.SetActive(true);
                        }
                    }
                    else if (singleRecharges[i].rechargeCnt > singleRecharges[i].revCnt && singleRecharges[i].revCnt < totalTime)
                    {
                        btnGet.GetComponent<UIButtonEvent>().mData.set("REWARDINDEX", index);
                        btnGet.SetActive(true);
                    }
                    if (totalTime <= singleRecharges[i].rechargeCnt)
                    {
                        canBuyTimeLabel.gameObject.SetActive(false);
                    }
                    else
                    {
                        canBuyTimeLabel.gameObject.SetActive(true);
                        canBuyTimeLabel.text = string.Format("可以购买{0}次", (totalTime - singleRecharges[i].rechargeCnt).ToString());
                    }
                }
            }
        }
        if (!IschangeState)
        {
            btnCharge.SetActive(true);
            canBuyTimeLabel.gameObject.SetActive(true);
            canBuyTimeLabel.text = string.Format("可以购买{0}次", totalTime.ToString());
        }
    }

    void GetItemCount()
    {
        while (DataCenter.mSingleRechargeEvent.GetRecord(index + 1) != null)
        {
            index++;
        }
        DEBUG.Log("ActivitySimple index ==" + index);
    }

    void UpdateTimeUI()
    {
        SetCountdownTime("activity_left_time", time1);
        SetCountdownTime("get_rewards_left_time", time2);
    }
}
public class Button_activity_single_welfare_rewards_get_btn : CEvent
{
    public override bool _DoEvent()
    {
        List<ItemDataBase> tmpAwardItems = GameCommon.ParseItemList(DataCenter.mFirstRechargeConfig.GetRecord(1)["REWARD"]);
        List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(tmpAwardItems);
        if (!CheckPackage.Instance.CanAddItems(tmpPackageTypes))
        {
            DataCenter.OpenMessageWindow("背包已满");
            return true;
        }
        int getIndex = (int)get("REWARDINDEX");
        NetManager.RequestRevSingleRechargeReward(getIndex);
        return true;
    }
}
public class Button_activity_single_welfare_go_task_btn : CEvent
{
    public override bool _DoEvent()
    {
        //关闭活动界面
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
        //前往充值界面
        GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, () =>
        {
            Logic.EventCenter.Start("Button_active_list_button")._DoEvent();

            int configIndex = 1;
            foreach (DataRecord record in DataCenter.mOperateEventConfig.Records())
            {
                if (record["EVENT_TYPE"] == (int)ACTIVITY_TYPE.ACTIVITY_FIRST_RECHARGE)
                {
                    configIndex = record["INDEX"];
                    break;
                }
            }
            ActivityWindow.mRefreshTabCallBack = () => { DataCenter.SetData("ACTIVITY_WINDOW", "CHANGE_TAB_POS_2", configIndex); };
            GlobalModule.DoLater(() =>
            {
                DataCenter.SetData("ACTIVITY_WINDOW", "SHOW_WINDOW", configIndex);
            }, 0.1f);
        },
        CommonParam.rechageDepth);
        return true;
    }
}