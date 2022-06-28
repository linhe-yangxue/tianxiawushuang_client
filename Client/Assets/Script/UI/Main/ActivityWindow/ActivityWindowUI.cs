using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;
using Utilities;

public enum ACTIVITY_TYPE
{
    MIN = 0,
    ACTIVITY_SIGN,						//每日签到    1
    ACTIVITY_SEVEN_DAY_LOGIN,			//七日登陆    2
    ACTIVITY_TREE,						//摇钱树      3
    ACTIVITY_WELFARE,					//VIP礼包     4
    ACTIVITY_FUND,						//开服基金    5
    ACTIVITY_FIRST_RECHARGE,            //> 首充礼包  6
    ACTIVITY_VITALITY,					//领仙桃      7
    ACTIVE_DAILY_REWARD_FOR_MONTH,      //月卡        8
    ACTIVITY_GIFT,						//礼品码      9
    ACTIVITY_SHOOTER_PARK,              //> 射手乐园 10
    ACTIVITY_LUCKY_CARD,                //> 幸运翻牌 11
    ACTIVITY_RECHARGE_GIFT,             //> 充值送礼 12
    ACTIVITY_CUMULATIVE,                //> 累计消费 13
    ACTIVITY_FLASH_SALE,                //限时抢购 14
    ACTIVITY_SINGLE_WELFARE,            //单冲福利 15
    MAX,
}

public class ActivityWindowUI : MonoBehaviour
{
    ActivityWindow mActivityWindow;

    void Start()
    {
        mActivityWindow = new ActivityWindow(gameObject) { mWinName = "ACTIVITY_WINDOW" };
        DataCenter.Self.registerData("ACTIVITY_WINDOW", mActivityWindow);

        DataCenter.OpenWindow("BACK_ACTIVE_LIST_WINDOW");
        DataCenter.OpenWindow("INFO_GROUP_WINDOW");

        DataCenter.OpenWindow("ACTIVITY_WINDOW", true);
    }

    public void FixedUpdate()
    {
        mActivityWindow.FixedUpdate();
    }

    void OnDestroy()
    {
        DataCenter.CloseWindow("BACK_ACTIVE_LIST_WINDOW");
        DataCenter.Remove("ACTIVITY_WINDOW");

        GameObject obj = GameObject.Find("create_scene");
        if (obj != null)
            GameObject.Destroy(obj);
    }
}

public class ActivityWindow : tWindow
{
    IEnumerable<DataRecord> mActivityList;
    ACTIVITY_TYPE mCurrentType = ACTIVITY_TYPE.MIN;
    public static Action mRefreshTabCallBack = null;

    UIGridContainer mGrid;
    UIScrollView mView;
    UIPanel mPanel;
    float mViewY;
	IEnumerable<DataRecord> mNewActivityList;

    public ActivityWindow(GameObject obj)
    {
        mGameObjUI = obj;
        mGameObjUI.transform.name = "activity_window";
    }

    public override void Init()
    {
        EventCenter.Register("Button_active_list_window_info_back", new DefineFactory<Button_active_list_window_info_back>());
        EventCenter.Register("Button_activity_window_button", new DefineFactory<Button_activity_window_button>());
    }

    public override void OnOpen()
    {
        DataCenter.OpenWindow("BACK_ACTIVE_LIST_WINDOW");

        mGrid = GetComponent<UIGridContainer>("activity_grid");
        mView = GetComponent<UIScrollView>("activity_scrollview");
        mPanel = GetComponent<UIPanel>("activity_scrollview");

        Refresh(null);
    }

    public override bool Refresh(object param)
    {
        RefreshTab(0);
//        ShowWindow(GetFirstActiveIndex());
        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "CHANGE_TAB_POS":
                ChangeTabPos((int)objVal);
                break;
            case "CHANGE_TAB_POS_2":
                ChangeTabsPos_2((int)objVal);
                break;
            case "SHOW_WINDOW":
                ShowWindow((int)objVal);
                break;
            case "REFRESH_TAB_NEWMARK":
                RefreshTabNewMark(objVal);
                break;
        }
    }

    /// <summary>
    /// 根据活动时间判断活动是否开启
    /// </summary>
    /// <returns></returns>
    public static bool CheckActivityIsOpenByTime(long kActivityOpenTime, long kActivityEndTime, long kGetGiftEndTime)
    {
        Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime());
        if (nowSeconds >= kGetGiftEndTime || nowSeconds < kActivityOpenTime)
        {
            return false;
        }
        return true;
    }

    // 获得开服的活动列表
    private IEnumerable<DataRecord> GetActivityOpenList(SC_GetActivitiesEndTime kOpenTime)
    {
        //all activity open
        //return DataCenter.mOperateEventConfig.Records();
        //
        List<DataRecord> _activityOpenList = new List<DataRecord>();
        _activityOpenList.Clear();
        _activityOpenList.AddRange(DataCenter.mOperateEventConfig.Records());
        // 判断射手乐园是否开启
        if (CommonParam.isOnLineVersion)
        {
            if (!ActivityShooterParkWindow.GetIsActivityOpen())
            {
                RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_SHOOTER_PARK);
            }
        }
        else
        {
            RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_SHOOTER_PARK);
        }
        if (!ActivityRechargeGiftWindow.GetIsActivityOpen(kOpenTime.rechargeTime.activityOpenTime, kOpenTime.rechargeTime.activityEndTime, kOpenTime.rechargeTime.rewardEndTime))
        {
            RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_RECHARGE_GIFT);
        }
//
//        //线上版本才显示幸运翻牌和礼品码、仙桃、vip礼包和首冲礼包 
        if (!CommonParam.isOnLineVersion)
        {
            RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_LUCKY_CARD);
            RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_GIFT);
            RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_VITALITY);
            RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_WELFARE);
            RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_FIRST_RECHARGE);
        }
//        //限时抢购
        string[] mtimes = DataCenter.mOpenTime.GetData(111, "Condition").ToString().Split('|');
        if (!CheckActivityIsOpenByTime(Int64.Parse(mtimes[0]), Int64.Parse(mtimes[1]), Int64.Parse(mtimes[1])))
        {
            RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_FLASH_SALE);
        }
//        //单充福利
        string[] mSingleTimes = DataCenter.mOpenTime.GetData(112, "Condition").ToString().Split('|');
        if (!CheckActivityIsOpenByTime(Int64.Parse(mSingleTimes[0]), Int64.Parse(mSingleTimes[1]), Int64.Parse(mSingleTimes[1])))
        {
            RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_SINGLE_WELFARE);
        }
        ActivitySevenDayLoginWindow tmpWin = DataCenter.GetData("ACTIVITY_SEVEN_DAY_LOGIN_WINDOW") as ActivitySevenDayLoginWindow;
        if (tmpWin != null)
        {
            if (!tmpWin.GetActivityIsOpen())
                RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_SEVEN_DAY_LOGIN);
        }
//        // 消费返利
        if (!ActivityCumulativeWindow.GetIsActivityOpen(kOpenTime.cumulativeTime.activityOpenTime, kOpenTime.cumulativeTime.activityEndTime, kOpenTime.cumulativeTime.rewardEndTime))
        {
            RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_CUMULATIVE);
        }

		//只保留礼包码
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_SHOOTER_PARK);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_RECHARGE_GIFT);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_LUCKY_CARD);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_VITALITY);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_WELFARE);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_FIRST_RECHARGE);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_FLASH_SALE);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_SINGLE_WELFARE);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_SEVEN_DAY_LOGIN);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_CUMULATIVE);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_FUND);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_SIGN);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVITY_TREE);
//		RemoveActivityByType(_activityOpenList, ACTIVITY_TYPE.ACTIVE_DAILY_REWARD_FOR_MONTH);

        return _activityOpenList;
    }

    /// <summary>
    /// 根据活动类型隐藏相应的活动   
    /// </summary>
    /// <param name="kActivityType"></param>
    private void RemoveActivityByType(List<DataRecord> kActivityList, ACTIVITY_TYPE kActivityType)
    {
        int _eventIndex = 0;
        foreach (var record in DataCenter.mOperateEventConfig.Records())
        {
            if ((int)record.getObject("EVENT_TYPE") == (int)kActivityType)
            {
                _eventIndex = (int)record.getObject("INDEX");
                break;
            }
        }
        for (int i = 0, count = kActivityList.Count; i < count; i++)
        {
            if ((int)kActivityList[i].getObject("INDEX") == _eventIndex)
            {
                kActivityList.RemoveAt(i);
                break;
            }
        }
    }
    #region 活动红点状态

    /// <summary>
    /// 根据活动提示状态得到活动类型
    /// </summary>
    /// <param name="kState"></param>
    /// <returns></returns>
    public static ACTIVITY_TYPE GetEventTypeByNotifState(SYSTEM_STATE kState)
    {
        switch (kState)
        {
            case SYSTEM_STATE.NOTIF_LUXURY_SIGN:        //> 豪华签到
            case SYSTEM_STATE.NOTIF_DAILY_SIGN:         //> 每日签到
                return ACTIVITY_TYPE.ACTIVITY_SIGN;
            case SYSTEM_STATE.NOTIF_SEVEN_DAY_LOGIN:    //> 七日登陆
                return ACTIVITY_TYPE.ACTIVITY_SEVEN_DAY_LOGIN;
            case SYSTEM_STATE.NOTIF_SHAKE_TREE:         //> 摇钱树
                return ACTIVITY_TYPE.ACTIVITY_TREE;
            case SYSTEM_STATE.NOTIF_WELFARE_DAY:        //> vip每日礼包
            case SYSTEM_STATE.NOTIF_WELFARE_WEEK:       //> vip每周礼包
                return ACTIVITY_TYPE.ACTIVITY_WELFARE;
            case SYSTEM_STATE.NOTIF_FIRST_RECHARGE:     //> 首充礼包
                return ACTIVITY_TYPE.ACTIVITY_FIRST_RECHARGE;
            case SYSTEM_STATE.NOTIF_VITALITY:           //> 领仙桃
                return ACTIVITY_TYPE.ACTIVITY_VITALITY;
            case SYSTEM_STATE.NOTIF_MONTH_CARD:         //> 月卡
                return ACTIVITY_TYPE.ACTIVE_DAILY_REWARD_FOR_MONTH;
            case SYSTEM_STATE.NOTIF_LUCKY_CARD:         //> 幸运翻牌
                return ACTIVITY_TYPE.ACTIVITY_LUCKY_CARD;
            case SYSTEM_STATE.NOTIF_CUMULATIVE:         //> 消费返利
                return ACTIVITY_TYPE.ACTIVITY_CUMULATIVE;
            case SYSTEM_STATE.NOTIF_FLASH_SALE:         //> 限时折扣
                return ACTIVITY_TYPE.ACTIVITY_FLASH_SALE;
            case SYSTEM_STATE.NOTIF_CHARGE_AWARD:       //> 充值送礼
                return ACTIVITY_TYPE.ACTIVITY_RECHARGE_GIFT;
            //case SYSTEM_STATE.NOTIF_SHOOTER_PARK:     //> 射手乐园
            //    return ACTIVITY_TYPE.ACTIVITY_SHOOTER_PARK;
            case SYSTEM_STATE.NOTIF_FUND_DIAMOND:       //> 开服基金
            case SYSTEM_STATE.NOTIF_FUND_WELFARE:       //> 全民福利
                return ACTIVITY_TYPE.ACTIVITY_FUND;
        }
        return ACTIVITY_TYPE.MAX;
    }
    /// <summary>
    /// 检测指定活动是否在配表中
    /// </summary>
    /// <param name="kActivityType"></param>
    /// <returns></returns>
    public static bool CheckActivityInConfig(ACTIVITY_TYPE kActivityType)
    {
        foreach (var record in DataCenter.mOperateEventConfig.GetAllRecord())
        {
            if ((int)record.Value["EVENT_TYPE"] == (int)kActivityType)
                return true;
        }
        return false;
    }
    /// <summary>
    /// 根据活动类型获得红点状态类型
    /// </summary>
    /// <param name="kActivityType"></param>
    /// <returns></returns>
    private bool GetSystemStateByActivityType(ACTIVITY_TYPE kActivityType)
    {
        switch (kActivityType)
        {
            case ACTIVITY_TYPE.ACTIVITY_SIGN:               //> 每日签到活动
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_DAILY_SIGN) || SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_LUXURY_SIGN);
            case ACTIVITY_TYPE.ACTIVITY_SEVEN_DAY_LOGIN:    //> 七日登录 
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_SEVEN_DAY_LOGIN);
            case ACTIVITY_TYPE.ACTIVITY_TREE:               //> 摇钱树
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_SHAKE_TREE);
            case ACTIVITY_TYPE.ACTIVITY_WELFARE:            //> VIP礼包
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_WELFARE_DAY) || SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_WELFARE_WEEK);
            case ACTIVITY_TYPE.ACTIVITY_FUND:               //> 开服基金
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FUND_DIAMOND) || SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FUND_WELFARE);
            case ACTIVITY_TYPE.ACTIVITY_FIRST_RECHARGE:     //> 首充礼包
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FIRST_RECHARGE);
            case ACTIVITY_TYPE.ACTIVITY_VITALITY:           //> 领仙桃
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_VITALITY);
            case ACTIVITY_TYPE.ACTIVE_DAILY_REWARD_FOR_MONTH://> 月卡
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_MONTH_CARD);
            case ACTIVITY_TYPE.ACTIVITY_GIFT:                //> 礼品码
                return false;
            case ACTIVITY_TYPE.ACTIVITY_LUCKY_CARD:          //> 幸运翻牌
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_LUCKY_CARD);
            case ACTIVITY_TYPE.ACTIVITY_RECHARGE_GIFT:       //> 充值送礼
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_CHARGE_AWARD);
            case ACTIVITY_TYPE.ACTIVITY_CUMULATIVE:          //> 消费返利
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_CUMULATIVE);
            case ACTIVITY_TYPE.ACTIVITY_FLASH_SALE:          //> 限时折扣
                return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FLASH_SALE);
            //case ACTIVITY_TYPE.ACTIVITY_SHOOTER_PARK:        //> 射手乐园
            //    return SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_SHOOTER_PARK);
        }
        return false;
    }
    /// <summary>
    /// 检测这个活动是否不需要红点提示
    /// </summary>
    /// <returns></returns>
    private bool CheckIsThisActivityExcludeNewMark(ACTIVITY_TYPE kActivityType)
    {
        switch (kActivityType)
        {
            case ACTIVITY_TYPE.ACTIVITY_GIFT:
                return true;
        }
        return false;
    }
    private void RefreshTabNewMark(object kObjVal)
    {
        if (!(kObjVal != null && kObjVal is ACTIVITY_TYPE))
            return;
        RefreshTabNewMark((ACTIVITY_TYPE)kObjVal);
    }
    private void RefreshTabNewMark(ACTIVITY_TYPE kActivity)
    {
        GameObject _subCell = GetTabCellByActivityType(kActivity);
        RefreshTabNewMark(_subCell, kActivity);
    }
    private void RefreshTabNewMark(ACTIVITY_TYPE kActivity, bool kVisible)
    {
        GameObject _subCell = GetTabCellByActivityType(kActivity);
        RefreshTabNewMark(_subCell, kActivity, kVisible);
    }
    private void RefreshTabNewMark(GameObject kSubCell, ACTIVITY_TYPE kActivity)
    {
        RefreshTabNewMark(kSubCell, kActivity, GetSystemStateByActivityType(kActivity));
    }
    /// <summary>
    /// 根据活动类型刷新活动红点状态
    /// </summary>
    /// <param name="kSubCell"></param>
    /// <param name="kActivity"></param>
    /// <param name="kVisible"></param>
    private void RefreshTabNewMark(GameObject kSubCell, ACTIVITY_TYPE kActivity, bool kVisible)
    {
        if (kSubCell == null)
            return;
        if (CheckIsThisActivityExcludeNewMark(kActivity))
        {
            GameCommon.SetNewMarkVisible(kSubCell, false);
        }
        else
        {
            GameCommon.SetNewMarkVisible(kSubCell, kVisible);
        }
    }

    /// <summary>
    /// 根据活动类型获得标签物体
    /// </summary>
    /// <param name="kActivity"></param>
    /// <returns></returns>
    private GameObject GetTabCellByActivityType(ACTIVITY_TYPE kActivityType)
    {
        if (mActivityList == null)
            return null;
        if (mGrid == null)
            return null;
        int _index = 0;
        foreach (var v in mActivityList)
        {
            if ((ACTIVITY_TYPE)v.getObject("EVENT_TYPE") == kActivityType)
            {
                if (_index < mGrid.MaxCount)
                {
                    DEBUG.Log("活动类型名称为:" + kActivityType);
                    return mGrid.controlList[_index];
                }
            }
            _index++;
        }
        DEBUG.Log("该活动未开启或者没有该活动");
        return null;
    }
    #endregion

	private IEnumerable<DataRecord> GetNewActivityList(IEnumerable<DataRecord> activityList)
	{
		List<DataRecord> _activityHaveMarkList = new List<DataRecord>();
		_activityHaveMarkList.Clear();
		List<DataRecord> _activityNoMarkList = new List<DataRecord>();
		_activityNoMarkList.Clear();
		List<DataRecord> _activityMarkList = new List<DataRecord>();
		_activityMarkList.Clear();

		foreach(var v in activityList)
		{
			if (v["INDEX"] == 0) continue;
			if((ACTIVITY_TYPE)((int)v["EVENT_TYPE"]) == ACTIVITY_TYPE.ACTIVITY_SIGN)
			{
				_activityMarkList.Add (v);
			}
			else if(GetSystemStateByActivityType((ACTIVITY_TYPE)((int)v["EVENT_TYPE"])))
			{
				_activityHaveMarkList.Add (v);
			}else
			{
				_activityNoMarkList.Add (v);
			}
		}

		_activityHaveMarkList = GameCommon.SortList(_activityHaveMarkList, SortActivityDataRecord);
		_activityNoMarkList = GameCommon.SortList(_activityNoMarkList, SortActivityDataRecord);
		_activityHaveMarkList.AddRange (_activityNoMarkList);
		//把每日签到永远放在第一个位置（即使“WEIGHT”不是）
		_activityMarkList.AddRange (_activityHaveMarkList);
		return _activityMarkList;
	}

	private int SortActivityDataRecord(DataRecord _list1, DataRecord _list2)
	{
		return _list1["WEIGHT"] - _list2["WEIGHT"];
	}
    public void RefreshTab(int iChoseId)
    {
        SetVisible("activity_window_button_back_btn", false);
        SetVisible("activity_window_button_forward_btn", true);

        object objVal = DataCenter.Self.getObject("OPEN_TIME");
        SC_GetActivitiesEndTime _receive = (SC_GetActivitiesEndTime)objVal;
		mNewActivityList = GetActivityOpenList(_receive);
		mActivityList = GetNewActivityList(mNewActivityList);

        mGrid.MaxCount = GetActiveCount(mActivityList);
        int index = 0;

        foreach (var v in mActivityList)
        {
            //			if(v["INDEX"] == 0 || !activeIsValid (v["INDEX"])) continue;
            if (v["INDEX"] == 0) continue;

            GameObject subCell = mGrid.controlList[index];
            subCell.transform.name = "activity_window_button";

            if (index == 0) GameCommon.ToggleTrue(subCell);

            if (iChoseId == (int)ACTIVITY_TYPE.ACTIVITY_FUND)
            {
                //开富基金
                if (v["EVENT_TYPE"] == (int)ACTIVITY_TYPE.ACTIVITY_FUND)
                {
                    GameCommon.ToggleTrue(subCell);
                    DataCenter.SetData("ACTIVITY_WINDOW", "CHANGE_TAB_POS", index);
                }
            }
            else if (iChoseId == (int)ACTIVITY_TYPE.ACTIVITY_FIRST_RECHARGE)
            {
                //首充礼包
                if (v["EVENT_TYPE"] == (int)ACTIVITY_TYPE.ACTIVITY_FIRST_RECHARGE)
                {
                    GameCommon.ToggleTrue(subCell);
                    DataCenter.SetData("ACTIVITY_WINDOW", "CHANGE_TAB_POS", index);
                }
            }

            string title = v["TITLE"];
            int configIndex = v["INDEX"];

            foreach (UILabel l in subCell.GetComponentsInChildren<UILabel>())
            {
                if (l != null) l.text = title;
            }
            string titleAtlas = v["RESOURCE_ATLAS"];
            string titleSprite = v["SPRITE_NAME"];
            foreach (UISprite _uiSprite in subCell.GetComponentsInChildren<UISprite>())
            {
                if (_uiSprite != null && _uiSprite.name == "Sprite")
                {
                    GameCommon.SetIcon(_uiSprite, titleAtlas, titleSprite);
                    _uiSprite.MakePixelPerfect();
                }
            }
            GameCommon.GetButtonData(subCell).set("POS", index);
            GameCommon.GetButtonData(subCell).set("INDEX", configIndex);

            index++;

            //added by xuke 刷新红点
            RefreshTabNewMark(subCell, (ACTIVITY_TYPE)v.getObject("EVENT_TYPE"));
            //end
        }

        UIScrollView view = GetComponent<UIScrollView>("activity_scrollview");
        view.ResetPosition();
        if (mRefreshTabCallBack != null)
        {
            GlobalModule.DoLater(() =>
            {
                if (mRefreshTabCallBack != null) 
                {
                    mRefreshTabCallBack();
                }
                mRefreshTabCallBack = null;
            }, 0.3f);
        }
		ShowWindow(GetFirstActiveIndex(mActivityList));
    }
	int GetFirstActiveIndex( IEnumerable<DataRecord> mActivityList)
	{
		foreach (var v in mActivityList)
		{
			int index = v["INDEX"];
			if (index != 0) return index;
		}
		
		return 0;
	}


    //	private void SetToggleSprite(GameObject kObj, string kAtlasName,string kSpriteName)
    //	{
    //		UISprite _BGIcon = GameCommon.FindComponent<UISprite>(GameCommon.FindObject(kObj,"Background"),"Sprite");
    //		UISprite _CheckmarkIcon = GameCommon.FindComponent<UISprite>(GameCommon.FindObject(kObj,"Checkmark"),"Sprite");
    //
    //		GameCommon.SetIcon (_BGIcon,kAtlasName,kSpriteName);
    //		GameCommon.SetIcon (_CheckmarkIcon,kAtlasName,kSpriteName);
    //
    //		_BGIcon.MakePixelPerfect ();
    //		_CheckmarkIcon.MakePixelPerfect ();
    //	}

    public void FixedUpdate()
    {
        if (mView.transform.position.y != mViewY && mGrid.MaxCount != 0)
        {
            mViewY = mView.transform.position.y;
            SetVisible("activity_window_button_back_btn", !mPanel.IsVisible(mGrid.controlList[0].transform.position));
            SetVisible("activity_window_button_forward_btn", !mPanel.IsVisible(mGrid.controlList[mGrid.MaxCount - 1].transform.position));
        }
    }

    void ChangeTabPos(int pos)
    {
        if (pos < 0 || pos > mGrid.MaxCount - 1) return;

        if (pos == 0 || pos == mGrid.MaxCount - 1)
        {
            //mView.SetDragAmount (0, pos/(mGrid.MaxCount - 1), false);
            return;
        }

        bool bBack = mPanel.IsVisible(mGrid.controlList[pos - 1].transform.position);        // panel.ConstrainTargetToBounds (backObj.transform, false);
        bool bBefore = mPanel.IsVisible(mGrid.controlList[pos + 1].transform.position);     // panel.ConstrainTargetToBounds (beforeObj.transform, false);

        if (!bBack)
        {
            mView.SetDragAmount(0, (float)(pos - 1) / (float)mGrid.MaxCount, false);
        }
        else if (!bBefore)
        {
            mView.SetDragAmount(0, (float)(pos + 1) / (float)mGrid.MaxCount, false);
        }
    }

    private void ChangeTabsPos_2(int kConfigIndex)
    {
        int _realPos = 0;
        //根据configIndex 得到该活动在活动列表中的位置
        foreach (var record in mActivityList)
        {
            if (record["INDEX"] == kConfigIndex)
            {
                break;
            }
            _realPos++;
        }

        mView.SetDragAmount(0, (float)(_realPos + 1) / (float)mGrid.MaxCount, false);
        mGrid.controlList[_realPos].GetComponent<UIToggle>().value = true;
    }

    void ShowWindow(int configIndex)
    {
        ACTIVITY_TYPE type = (ACTIVITY_TYPE)((int)DataCenter.mOperateEventConfig.GetData(configIndex, "EVENT_TYPE"));
        if (mCurrentType != type)
        {
            DataCenter.CloseWindow(mCurrentType.ToString() + "_WINDOW");
            DataCenter.OpenWindow(type.ToString() + "_WINDOW", configIndex);
        }
        mCurrentType = type;
    }

    int GetActiveCount(IEnumerable<DataRecord> kActivityList)
    {
        int count = 0;
        foreach (var v in kActivityList)
        {
            //			if(v["INDEX"] != 0  && activeIsValid (v["INDEX"])) count ++;
            if (v["INDEX"] != 0) count++;
        }

        return count;
    }

    int GetFirstActiveIndex()
    {
        foreach (var v in DataCenter.mOperateEventConfig.Records())
        {
            int index = v["INDEX"];
            //			if(index != 0 && activeIsValid (index)) return index;
            if (index != 0) return index;
        }

        return 0;
    }

    bool activeIsValid(int configIndex)
    {
        DataRecord record = DataCenter.mOperateEventConfig.GetRecord(configIndex);
        string strStartTime = record["START_TIME"];
        string strEndTime = record["AWARD_TIME"];
        string[] strStartTimes = strStartTime.Split('_');
        string[] strEndTimes = strEndTime.Split('_');
        DateTime startData = new DateTime(Convert.ToInt32(strStartTimes[0]), Convert.ToInt32(strStartTimes[1]), Convert.ToInt32(strStartTimes[2]), 0, 0, 0);
        DateTime endData = new DateTime(Convert.ToInt32(strEndTimes[0]), Convert.ToInt32(strEndTimes[1]), Convert.ToInt32(strEndTimes[2]), 23, 59, 59);

        Int64 startSeconds = GameCommon.DateTime2TotalSeconds(startData);
        Int64 endSeconds = GameCommon.DateTime2TotalSeconds(endData);
        Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime());

        if (nowSeconds < startSeconds || nowSeconds > endSeconds) return false;

        return true;
    }

}

class Button_activity_window_button : CEvent
{
    public override bool _DoEvent()
    {
        int pos = (int)getObject("POS");
        DataCenter.SetData("ACTIVITY_WINDOW", "CHANGE_TAB_POS", pos);

        int configIndex = (int)getObject("INDEX");
        DataCenter.SetData("ACTIVITY_WINDOW", "SHOW_WINDOW", configIndex);
        return true;
    }
}