using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataTable;
using System;

/// <summary>
/// 充值送礼相关信息
/// </summary>
public class RechargeQueryInfo
{
    public int rmbNum;            //> 需要达到的进度
    public int hasAward;          //> 是否已经领取奖励
    public ItemDataBase[] items;  //> 奖励道具数组
}

/// <summary>
/// 充值送礼窗口
/// </summary>
public class ActivityRechargeGiftWindow : tWindow
{
    private static long mActivityEndTime = 0;            //> 活动结束时间
    private static long mGetGiftEndTime = 0;             //> 领奖结束时间
    private UIGridContainer mGiftGrid = null;
    //private List<int> mGiftHasGotList = new List<int>(); //> 已经获得的奖励索引列表
    private int mCurGetGiftIndex = -1;                   //> 当前领取的奖励索引   
    private static bool mIsActivityOn = false;           //> 当前活动是否开启
    private static bool mCanGetGift = false;             //> 是否可以领取礼物
    SC_RechargeGiftQuery _rechargeQueryInfo = null;      //> 充值送礼活动相关信息
    private List<RechargeQueryInfo> _sequenceRechargeQueryInfo = null;
    private const int MAX_REWARD_ITEM_SHOW_COUNT = 5;    //> 最多显示奖励数量
    //added by xuke 红点相关
    private int mCanGetCount = 0;   //> 可以领取的奖励数量
    //end
    /// <summary>
    /// 判断当前活动是否开启
    /// </summary>
    /// <returns></returns>
    public static bool GetIsActivityOpen(long kActivityOpenTime,long kActivityEndTime, long kGetGiftEndTime)
    {
        mActivityEndTime = kActivityEndTime;
        mGetGiftEndTime = kGetGiftEndTime;

        Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime());
        if (nowSeconds >= mGetGiftEndTime || nowSeconds < kActivityOpenTime)
        {
            mIsActivityOn = false;
            mCanGetGift = false;
            return false;
        }
        else 
        {
            mCanGetGift = true;
            if (nowSeconds < mActivityEndTime)
                mIsActivityOn = true;
        }         
        return true;    
    }

    protected override void OpenInit()
    {
        base.OpenInit();
        mGiftGrid = GetCurUIComponent<UIGridContainer>("task_grid");
    }
    public override void Open(object param)
    {
        base.Open(param);

        NetManager.RequestRechargeGift();
    }
    public override bool Refresh(object param)
    {
        return base.Refresh(param);     
    }
    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex) 
        {
            case "SET_RECHARGE_GIFT_INFO":
                SetRechargeGiftInfo(objVal);
                break;
            case "GET_RECHARGE_GIFT":
                GetRechargeGift(objVal);
                break;
        }
    }

    private void RefreshNewMark() 
    {
        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_RECHARGE_GIFT);        
    }
#region 数据获取
    /// <summary>
    /// 设置当前充值送礼奖励的领取情况
    /// </summary>
    /// <param name="kGiftInfo"></param>
    private void SetRechargeGiftInfo(object kGiftInfo) 
    {
        SC_RechargeGiftQuery _receive = (SC_RechargeGiftQuery)kGiftInfo;
        _rechargeQueryInfo = _receive;

        RefreshGiftUI();
    }
    private void GetRechargeGift(object kGiftInfo) 
    {
        SC_GetRechargeGift _receive = (SC_GetRechargeGift)kGiftInfo;
        //刷新当前领取奖励的UI
        SetBtnState(mCurGetGiftIndex,Btn_StateType.HAS_GOT);
        //领取奖励
        List<ItemDataBase> itemDataList = PackageManager.UpdateItem(_receive.awardArr); 
//        DataCenter.OpenWindow("GET_REWARDS_WINDOW", new ItemDataProvider(itemDataList));
		DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemDataList);
        if (mCanGetCount - 1 <= 0) 
        {
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_CHARGE_AWARD,false);
            RefreshNewMark();
        }
    }
#endregion

#region 回调
    public void ActivityEndCallBack(object param)
    {
        mIsActivityOn = false;
        NetManager.RequestRechargeGift();
        SetEndLabel(GameCommon.FindObject(mGameObjUI, "activity_left_time"));
    }
    public void GetGiftEndCallBack(object param)
    {
        mCanGetGift = false;
        DataCenter.OpenMessageOkWindow("活动已经结束", () => { MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow); });
        SetEndLabel(GameCommon.FindObject(mGameObjUI, "get_rewards_left_time"));
    }
#endregion

#region UI刷新
    private void SetEndLabel(GameObject kTimeLblObj, string kLblName = "Label") 
    {
        if(kTimeLblObj == null)
            return;
        UILabel _timeLbl = GameCommon.FindComponent<UILabel>(kTimeLblObj, kLblName);
        if (_timeLbl != null) 
        {
            _timeLbl.text = "已结束";
        }
    }

    private int ConvertMoney(int kMoney) 
    {
        return kMoney / 100;
    }

    private List<RechargeQueryInfo> GetSequenceItemDataArray(RechargeQueryInfo[] kItemArray)
    {
        if (kItemArray == null || kItemArray.Length == 0)
            return null;
        List<RechargeQueryInfo> _arr = new List<RechargeQueryInfo>(kItemArray);
        RechargeQueryInfo _tmpItem;
        for (int i = 0, count_i = kItemArray.Length - 1; i < count_i; i++)
        {
            for (int j = i + 1, count_j = kItemArray.Length; j < count_j; j++) 
            {
                if (_arr[i].rmbNum > _arr[j].rmbNum) 
                {
                    _tmpItem = _arr[i];
                    _arr[i] = _arr[j];
                    _arr[j] = _tmpItem;
                }
            }
        }
        return _arr;
    }
    private void RefreshGiftUI() 
    {
        if (_rechargeQueryInfo == null)
            return;
        //1.设置时间
        string _endInfo = "已结束";
        SetCountdownTimeWithEndInfo(mIsActivityOn, "activity_left_time", mActivityEndTime, new CallBack(this, "ActivityEndCallBack", null), _endInfo);
        SetCountdownTimeWithEndInfo(mCanGetGift, "get_rewards_left_time", mGetGiftEndTime, new CallBack(this, "GetGiftEndCallBack", null), _endInfo);
        //2.刷新奖励
        int _maxGiftCount = _rechargeQueryInfo.indexArr.Length;
        mGiftGrid.MaxCount = _maxGiftCount;
        _sequenceRechargeQueryInfo = GetSequenceItemDataArray(_rechargeQueryInfo.indexArr);
        if (_sequenceRechargeQueryInfo == null)
            return;
        ItemDataBase[] _itemArr;    //> 奖励
        int _rmbNum;                //> 进度
        bool _hasGot;               //> 是否领取
        int _totalRechargeMoney = _rechargeQueryInfo.money;    //> 充值的总金额

        mCanGetCount = 0;
        for(int i = 0;i < _maxGiftCount;i++)
        {
            GameObject _giftItem = mGiftGrid.controlList[i];
            _itemArr = _sequenceRechargeQueryInfo[i].items;
            _rmbNum = _sequenceRechargeQueryInfo[i].rmbNum;
            _hasGot = _sequenceRechargeQueryInfo[i].hasAward == 1;
            //DataRecord _record = DataCenter.mRechargeEventConfig.GetRecord(i+1);
                      
            // 1.设置进度，按钮状态
            int _needCostNum = _rmbNum;// _record["COST"];

            UILabel _progressLbl = GameCommon.FindComponent<UILabel>(_giftItem, "task_value_label");
            if (_totalRechargeMoney < _needCostNum)
            {
                _progressLbl.text = ConvertMoney(_totalRechargeMoney) + "/" + ConvertMoney(_needCostNum);
                _progressLbl.SetProgressColor(CommonColorType.HINT_RED);
                // 设置按钮状态
                SetBtnState(i, mIsActivityOn ? Btn_StateType.GO_FORWARD : Btn_StateType.GO_FORWARD_GREY);
            }
            else 
            {
                _progressLbl.text = ConvertMoney(_needCostNum) + "/" + ConvertMoney(_needCostNum);
                _progressLbl.SetProgressColor(CommonColorType.GREEN);

                SetBtnState(i, _hasGot ? Btn_StateType.HAS_GOT : Btn_StateType.CAN_GET);
                if (!_hasGot) 
                {
                    mCanGetCount++;
                }
            }          
            // 2.设置奖励
            List<ItemDataBase> _itemList = _itemArr.ToList<ItemDataBase>();// GameCommon.ParseItemList(_record["REWARD"]);
            UIGridContainer _rewardsGrid = GameCommon.FindComponent<UIGridContainer>(_giftItem, "rewards_grid");
            int _itemShowCount = Mathf.Min(MAX_REWARD_ITEM_SHOW_COUNT, _itemList.Count);
            _rewardsGrid.MaxCount = _itemShowCount;
            for (int k = 0, count = _itemShowCount; k < count; k++) 
            {
                GameObject _tmpRewardItem = _rewardsGrid.controlList[k];
                GameCommon.SetOnlyItemIcon(GameCommon.FindObject(_tmpRewardItem, "item_icon"), _itemList[k].tid);
                GameCommon.FindComponent<UILabel>(_tmpRewardItem, "rewards_num_label").SetNumLabel(_itemList[k].itemNum);

                int tmpItemID = _itemList[k].tid;
                _tmpRewardItem.GetComponent<UIButtonEvent>().AddAction(() =>
                {
                    DataCenter.OpenWindow("CONSUMBLES_DETAILS_WINDOW", tmpItemID);
                });        
            }
            // 3.如果奖励物品是0则隐藏
            _giftItem.SetActive(_itemShowCount != 0);        
        }
        mGiftGrid.repositionNow = true;
    }
    /// <summary>
    /// 设置按钮状态和绑定按钮事件
    /// </summary>
    /// <param name="kGiftIndex"></param>
    /// <param name="kHasGot"></param>
    private void SetBtnState(int kGiftIndex,Btn_StateType kBtnType) 
    {
        List<GameObject> _btnList = new List<GameObject>();
        GameObject _goForwardObj = GameCommon.FindObject(mGiftGrid.controlList[kGiftIndex], "go_task_btn");
        GameObject _canGetObj = GameCommon.FindObject(mGiftGrid.controlList[kGiftIndex], "rewards_get_btn");
        GameObject _hasGotObj = GameCommon.FindObject(mGiftGrid.controlList[kGiftIndex], "rewards_have_get_btn");
        GameObject _goForwardGreyObj = GameCommon.FindObject(mGiftGrid.controlList[kGiftIndex], "go_task_grey_btn");
        _btnList.Add(_goForwardObj);
        _btnList.Add(_canGetObj);
        _btnList.Add(_hasGotObj);
        _btnList.Add(_goForwardGreyObj);

        for (int i = 0, count = _btnList.Count; i < count; i++) 
        {
            _btnList[i].SetActive(false);
        }
        _btnList[(int)kBtnType].SetActive(true);

        //绑定按钮事件
        AddButtonAction(_goForwardObj,()=>
        {
            //前往充值界面
            GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, () =>
                {
                    NetManager.RequestRechargeGift();
                },
            CommonParam.rechageDepth);
        });

        AddButtonAction(_canGetObj, () =>
        {
            mCurGetGiftIndex = kGiftIndex;
            //预判背包是否已满
            List<ItemDataBase> tmpAwardItems = _rechargeQueryInfo.indexArr[mCurGetGiftIndex].items.ToList<ItemDataBase>();//GameCommon.ParseItemList(DataCenter.mRechargeEventConfig.GetRecord(mCurGetGiftIndex)["REWARD"]);
            List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(tmpAwardItems);
            if (!CheckPackage.Instance.CanAddItems(tmpPackageTypes)) 
            {
                DataCenter.OpenMessageWindow("背包已满");
                return;
            }
            if (_sequenceRechargeQueryInfo == null)
                return;
            NetManager.RequestGetRechargeGift(_sequenceRechargeQueryInfo[mCurGetGiftIndex].rmbNum);
        });
    }
#endregion


}
