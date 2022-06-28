using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using System;
using System.Linq;
using System.IO;
using System.Text;

/// <summary>
/// 累计消费相关信息
/// </summary>
public class CumulativeQueryInfo : IComparable
{
    public int rmbNum;            //> 需要达到的进度
    public ItemDataBase[] items;  //> 奖励道具数组

    #region IComparable Member
    public int CompareTo(object obj) 
    {
        CumulativeQueryInfo _info = (CumulativeQueryInfo)obj;
        return rmbNum.CompareTo(_info.rmbNum);
    }
    #endregion
}
/// <summary>
/// 累计消费窗口
/// </summary>
public class ActivityCumulativeWindow : tWindow
{
    /// <summary>
    /// 累计消费元宝
    /// </summary>
    private int mCumulativeDiamond = 0;
    private string mCumulativeDiamondMD5 = "";
    private const int MAX_REWARD_ITEM_SHOW_COUNT = 5;    //> 最多显示奖励数量
    //added by xuke
    private int mCanGetCount = 0;
    //end
    public int CumulativeDiamond 
    {
        set 
        {
            mCumulativeDiamond = value;
            MemoryStream tmpMS = new MemoryStream(Encoding.UTF8.GetBytes(mCumulativeDiamond.ToString()));
            mCumulativeDiamondMD5 = MD5.CalculateMD5(tmpMS);
            tmpMS.Close();
        }
        get 
        {
            MemoryStream tmpMS = new MemoryStream(Encoding.UTF8.GetBytes(mCumulativeDiamond.ToString()));
            string tmpCumulativeDiamondMD5 = MD5.CalculateMD5(tmpMS);
            tmpMS.Close();
            if(mCumulativeDiamondMD5 != tmpCumulativeDiamondMD5)
            {
                //TODO
                return 0;
            }
            return mCumulativeDiamond;
        }
    }
    public List<int> mGiftHasGotList = new List<int>();  //> 奖励领取状态列表
    private UIGridContainer mGiftGrid = null;
    private int mCurGetGiftIndex = -1;                  //> 当前领取奖励的索引
    private static long mActivityEndTime = 0;           //> 活动结束时间
    private static long mGetGiftEndTime = 0;            //> 领奖结束时间
    private static bool mIsActivityOn = false;          //> 判断当前活动是否正在进行
    private static bool mCanGetGift = false;            //> 是否可以领取礼物
    public const int ACTIVITY_OPEN_LEVEL = 10;         //> 该活动10级开放
    SC_CumulativeQuery _cumulativeQueryReceive = null;      //> 充值送礼活动相关信息
    private List<CumulativeQueryInfo> _sequenceCumulativeQueryInfo = null;
    public static bool GetIsActivityOpen(long kActivityOpenTime,long kActivityEndTime,long kGetGiftEndTime) 
    {
        if (RoleLogicData.Self.character.level < ACTIVITY_OPEN_LEVEL)
            return false;
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
        //test
        //CumulativeDiamond = 150;
        //end
        base.OpenInit();
        mGiftGrid = GetCurUIComponent<UIGridContainer>("task_grid");
    }
    public override void Open(object param)
    {
        base.Open(param);

        if (mGiftGrid == null)
        {
            mGiftGrid = GetCurUIComponent<UIGridContainer>("task_grid");
        }
        NetManager.RequestCumulative();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex) 
        {
            case "SET_CUMULATIVE_GIFT_INFO":
                SetCumulativeGiftInfo(objVal);
                break;
            case "GET_CUMULATIVE_GIFT":
                GetCumulativeGift(objVal);
                break;
        }
    }

    #region 数据获取
    private void SetCumulativeGiftInfo(object param) 
    {
        SC_CumulativeQuery _receive = (SC_CumulativeQuery)param;
        _cumulativeQueryReceive = _receive;
        //刷新UI
        RefreshGiftUI();
    }
    private void GetCumulativeGift(object param) 
    {
        SC_GetCumulativeGift _receive = (SC_GetCumulativeGift)param;
        //刷新当前领取奖励的UI
        SetBtnState(mCurGetGiftIndex, Btn_StateType.HAS_GOT);
        //领取奖励
        List<ItemDataBase> itemDataList = PackageManager.UpdateItem(_receive.prizeContent);
//        DataCenter.OpenWindow("GET_REWARDS_WINDOW", new ItemDataProvider(itemDataList));
		DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemDataList);
        if (mCanGetCount - 1 <= 0) 
        {
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_CUMULATIVE,false);
            RefreshNewMark();
        }
    }

    #endregion

    #region UI刷新
    private void SetEndLabel(GameObject kTimeLblObj, string kLblName = "Label")
    {
        if (kTimeLblObj == null)
            return;
        UILabel _timeLbl = GameCommon.FindComponent<UILabel>(kTimeLblObj, kLblName);
        if (_timeLbl != null)
        {
            _timeLbl.text = "已结束";
        }
    }
    private void RefreshNewMark() 
    {
        DataCenter.SetData("ACTIVITY_WINDOW", "REFRESH_TAB_NEWMARK", ACTIVITY_TYPE.ACTIVITY_CUMULATIVE);
    }

    private List<CumulativeQueryInfo> GetSequenceItemDataArray(CumulativeQueryInfo[] kItemArray)
    {
        if (kItemArray == null || kItemArray.Length == 0)
            return null;
        List<CumulativeQueryInfo> _arr = new List<CumulativeQueryInfo>(kItemArray);
        _arr.Sort();
        return _arr;
    }

    private void RefreshGiftUI()
    {
        if (_cumulativeQueryReceive == null)
            return;
        //1.设置时间
        string _endInfo = "已结束";
        SetCountdownTimeWithEndInfo(mIsActivityOn,"activity_left_time", mActivityEndTime, new CallBack(this, "ActivityEndCallBack",null), _endInfo);
        SetCountdownTimeWithEndInfo(mCanGetGift, "get_rewards_left_time", mGetGiftEndTime, new CallBack(this, "GetGiftEndCallBack", null), _endInfo);
        //2.刷新奖励
        _sequenceCumulativeQueryInfo = GetSequenceItemDataArray(_cumulativeQueryReceive.chargeAward);
        if (_sequenceCumulativeQueryInfo == null)
            return;
        int _maxGiftCount = _sequenceCumulativeQueryInfo.Count;//DataCenter.mCostEventConfig.GetRecordCount();

        mGiftGrid.MaxCount = _maxGiftCount;
        ItemDataBase[] _itemArr;    //> 奖励
        int _rmbNum;                //> 进度
        bool _hasGot = false ;      //> 是否领取
        int _totalConsumeDiamond = _cumulativeQueryReceive.costNums;    //> 消费的总金额

        mCanGetCount = 0;
        for (int i = 0; i < _maxGiftCount; i++)
        {
            _hasGot = false;
            _itemArr = _sequenceCumulativeQueryInfo[i].items;
            _rmbNum = _sequenceCumulativeQueryInfo[i].rmbNum;
            // 判断该条消费奖励是否已经领取过
            if (_cumulativeQueryReceive.prizeStatus == null)
            {
                _hasGot = false;
            }
            else 
            {
                for (int statusIndex = 0, statusCount = _cumulativeQueryReceive.prizeStatus.Length; statusIndex < statusCount; statusIndex++) 
                {
                    if (_cumulativeQueryReceive.prizeStatus[statusIndex] == _sequenceCumulativeQueryInfo[i].rmbNum) 
                    {
                        _hasGot = true;
                        break;
                    }
                }
            }
            GameObject _giftItem = mGiftGrid.controlList[i];
            //DataRecord _record = DataCenter.mCostEventConfig.GetRecord(i + 1);

            // 1.设置进度，按钮状态
            int _needCostNum = _rmbNum;//_record["NUMBER"];
            UILabel _progressLbl = GameCommon.FindComponent<UILabel>(_giftItem, "task_value_label");
            if (_totalConsumeDiamond < _needCostNum)
            {
                _progressLbl.text = _totalConsumeDiamond + "/" + _needCostNum;
                _progressLbl.SetProgressColor(CommonColorType.HINT_RED);
                // 设置按钮状态
                SetBtnState(i, mIsActivityOn ? Btn_StateType.GO_FORWARD : Btn_StateType.GO_FORWARD_GREY);
            }
            else
            {
                _progressLbl.text = _needCostNum + "/" + _needCostNum;
                _progressLbl.SetProgressColor(CommonColorType.GREEN);
                // 设置按钮状态 
                //bool kHasGot = false;
                //for (int j = 0, count = mGiftHasGotList.Count; j < count; j++)
                //{
                //    if (i == mGiftHasGotList[j]-1)
                //    {
                //        kHasGot = true;
                //        break;
                //    }
                //}
                SetBtnState(i, _hasGot ? Btn_StateType.HAS_GOT : Btn_StateType.CAN_GET);
                if (!_hasGot)
                {
                    mCanGetCount++;
                }
            }
            // 2.设置奖励
            //List<ItemDataBase> _itemList = GameCommon.ParseItemList(_record["REWARD"]);
            UIGridContainer _rewardsGrid = GameCommon.FindComponent<UIGridContainer>(_giftItem, "rewards_grid");
            int _itemShowCount = Mathf.Min(MAX_REWARD_ITEM_SHOW_COUNT, _itemArr.Length);
            _rewardsGrid.MaxCount = _itemShowCount;//_itemList.Count;
            for (int k = 0, count = _itemShowCount; k < count; k++)
            {
                GameObject _tmpRewardItem = _rewardsGrid.controlList[k];
                GameCommon.SetOnlyItemIcon(GameCommon.FindObject(_tmpRewardItem, "item_icon"), _itemArr[k].tid);
                GameCommon.FindComponent<UILabel>(_tmpRewardItem, "rewards_num_label").SetNumLabel(_itemArr[k].itemNum);

                int tmpItemID = _itemArr[k].tid;
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
    #endregion

#region 回调
    public void ActivityEndCallBack(object param) 
    {
        mIsActivityOn = false;
        NetManager.RequestCumulative();
        SetEndLabel(GameCommon.FindObject(mGameObjUI, "activity_left_time"));
    }
    public void GetGiftEndCallBack(object param)
    {
        mCanGetGift = false;
        DataCenter.OpenMessageOkWindow("活动已经结束", () => { MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow); });
        SetEndLabel(GameCommon.FindObject(mGameObjUI, "activity_left_time"));
    }
#endregion

    private void SetBtnState(int kGiftIndex, Btn_StateType kBtnType) 
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
        AddButtonAction(_goForwardObj, () =>
        {
            //关闭活动界面
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
            //前往集市抽卡
            GetPathHandlerDic.HandlerDic[GET_PARTH_TYPE.JI_SHI_CHOUKA]();
            //绑定返回委托
            MainUIScript.Self.mShopBackAction = () =>
            {
                Logic.EventCenter.Start("Button_active_list_button")._DoEvent();
                //判断当前活动是否开启
                if (mCanGetGift) 
                { 
                    int configIndex = 0;
                    foreach (DataRecord record in DataCenter.mOperateEventConfig.Records()) 
                    {
                        if (record["EVENT_TYPE"] == (int)ACTIVITY_TYPE.ACTIVITY_CUMULATIVE) 
                        {
                            configIndex = record["INDEX"];
                            break;
                        }
                    }
                    ActivityWindow.mRefreshTabCallBack = () => { DataCenter.SetData("ACTIVITY_WINDOW", "CHANGE_TAB_POS_2", configIndex); };
                    GlobalModule.DoOnNextUpdate(2,() =>
                    {
                        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ActivityWindow);
                        
                        GlobalModule.DoCoroutine(IE_ShowCumulativeWin(configIndex));
                    });
                }
            };
        });
        AddButtonAction(_canGetObj, () =>
        {
            mCurGetGiftIndex = kGiftIndex;
            //预判背包是否已满
            List<ItemDataBase> tmpAwardItems = _sequenceCumulativeQueryInfo[mCurGetGiftIndex].items.ToList<ItemDataBase>();//GameCommon.ParseItemList(DataCenter.mCostEventConfig.GetRecord(mCurGetGiftIndex)["REWARD"]);
            List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(tmpAwardItems);
            if (!CheckPackage.Instance.CanAddItems(tmpPackageTypes))
            {
                return;
            }
            NetManager.RequestGetCumulativeGift(_sequenceCumulativeQueryInfo[mCurGetGiftIndex].rmbNum);
        });
    }

    private IEnumerator IE_ShowCumulativeWin(int kConfigIndex) 
    {
        yield return null;
        DataCenter.SetData("ACTIVITY_WINDOW", "SHOW_WINDOW", kConfigIndex);        
    }
}
