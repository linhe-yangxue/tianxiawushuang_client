using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;

/// <summary>
/// 首充礼包窗口
/// </summary>
public class ActivityFirstRechargeWindow : tWindow
{
    private enum BTN_STATE 
    {
        GO_RECHARGE = 0,    //> 去充值
        GET = 1,            //> 领取
        HAS_GOT = 2,        //> 已领取
    }

    public UIGridContainer mRewardGrid = null;
    private BTN_STATE mRechargeState = BTN_STATE.GO_RECHARGE;     //> 首充状态 首充礼包领取状态 0=>去充值 1=>领取 2=>已领取
    public override void Init()
    {
        base.Init();
        EventCenter.Self.RegisterEvent("Button_activity_first_charge_button",new DefineFactory<Button_activity_first_charge_button>());
        EventCenter.Self.RegisterEvent("Button_first_recharge_rewardsGetBtn", new DefineFactory<Button_first_recharge_rewardsGetBtn>());
    }

    protected override void OpenInit()
    {
        base.OpenInit();
        mRewardGrid = GetCurUIComponent<UIGridContainer>("today_reward_Grid");
    }

    public override void Open(object param)
    {
        base.Open(param);

        NetManager.RequestFirstCharge();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex) 
        {
            case "SET_FIRST_RECHARGE_INFO":
                SetFirstRechargeInfo(objVal);
                break;
            case "GET_REWARD":
                GetReward(objVal);
                RefershNewMark();
                break;
        }
    }
#region 数据获取
    private void SetFirstRechargeInfo(object param) 
    {
        SC_FirstChargeQuery _receive = (SC_FirstChargeQuery)param;
        mRechargeState = (BTN_STATE)_receive.code;

        RefreshGiftUI();
    }
    private void GetReward(object param) 
    {
        if (param != null && param is SC_GetFirstChargeReward) 
        {
            SC_GetFirstChargeReward _receive = (SC_GetFirstChargeReward)param;
            List<ItemDataBase> itemDataList = PackageManager.UpdateItem(_receive.rewards);
//            DataCenter.OpenWindow("GET_REWARDS_WINDOW", new ItemDataProvider(itemDataList));
			DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemDataList);
            SetBtnState(BTN_STATE.HAS_GOT);
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_FIRST_RECHARGE, false);
        }
    }
#endregion

#region 刷新UI
    private void RefreshGiftUI() 
    {
        List<ItemDataBase> rewardsList = GameCommon.ParseItemList(DataCenter.mFirstRechargeConfig.GetRecord(1)["REWARD"]);
        int _rewardCount = rewardsList.Count;
        mRewardGrid.MaxCount = _rewardCount;
        for (int i = 0; i < _rewardCount; i++) 
        {
            //刷新奖励UI
            GameObject _rewardItme = mRewardGrid.controlList[i];
			int _iTid = rewardsList[i].tid;
            GameCommon.SetOnlyItemIcon(_rewardItme, "item_icon",rewardsList[i].tid);
			AddButtonAction (GameCommon.FindObject (_rewardItme, "icon_tips_btn"), () => GameCommon.SetItemDetailsWindow (_iTid));
            GameCommon.SetOnlyItemCount(_rewardItme, "item_num",rewardsList[i].itemNum);
        }
        SetBtnState(mRechargeState);
    }

    private void RefershNewMark() 
    {
        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_FIRST_RECHARGE);
    }
#endregion
    private void SetBtnState(BTN_STATE kBtnState) 
    {

        GameObject _goRechargeBtn = GameCommon.FindObject(mGameObjUI, "activity_first_charge_button");
        GameObject _hasGotBtn = GameCommon.FindObject(mGameObjUI, "rewards_have_get_btn");
        GameObject _rewardsGetBtn = GameCommon.FindObject(mGameObjUI, "first_recharge_rewardsGetBtn");

        _goRechargeBtn.SetActive(false);
        _hasGotBtn.SetActive(false);
        _rewardsGetBtn.SetActive(false);

        switch (kBtnState) 
        {
            case BTN_STATE.GO_RECHARGE:
                _goRechargeBtn.SetActive(true);
                break;
            case BTN_STATE.GET:
                _rewardsGetBtn.SetActive(true);
                break;                     
            case BTN_STATE.HAS_GOT:
                _hasGotBtn.SetActive(true);
                break;
        }
    }
}

/// <summary>
/// 去充值按钮
/// </summary>
public class Button_activity_first_charge_button : CEvent
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

/// <summary>
/// 领取奖励按钮
/// </summary>
public class Button_first_recharge_rewardsGetBtn : CEvent 
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
        NetManager.RequestGetFirstRechargeGift();
        return true;
    }
}