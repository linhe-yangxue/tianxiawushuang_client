using UnityEngine;
using System.Collections;
using System;
using DataTable;
using System.Collections.Generic;
using Logic;
using System.Linq;

public class RevelryObject
{
    public int revelryId;
    public int progress;
    public bool accepted;
}

public class RevelryId
{
    public int revelryId;
    public int iIndex;
}

public enum SEVENDAY_PAGE
{
    PAGE_ONE = 1,
    PAGE_TWO,
    PAGE_THREE,
    PAGE_FOUR
}

public class SevenDaysCarnivalWindow : tWindow
{
    public List<RevelryObject> revelryObject = new List<RevelryObject>();

    public const int MAX_DAY = 7;
    public const int MAX_GET_DAY = 10;
    const int DAY_TASK_TYPE_NUM = 4;//3;
    UIGridContainer mLeftGrid;
    UIScrollView mLeftView;
    UIPanel mLeftPanel;
    float mLeftViewY;
    UIGridContainer titleNameGrid;
    UIScrollView mRightView;
    Int64 closeGoTaskDate = 0;
    Int64 closeGetRewardsDate = 0;
    long iDaysIndex = 1;		//第几天
    public SC_HalfPriceQuery scHalfPriceQuery = null;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_carnival_window_back_btn", new DefineFactory<Button_carnival_window_back_btn>());
        EventCenter.Self.RegisterEvent("Button_revelry_button", new DefineFactory<Button_revelry_button>());
        EventCenter.Self.RegisterEvent("Button_no_open_button", new DefineFactory<Button_no_open_button>());
        EventCenter.Self.RegisterEvent("Button_revelry_day_task_button", new DefineFactory<Button_revelry_day_task_button>());
        EventCenter.Self.RegisterEvent("Button_seven_days_rewards_get_btn", new DefineFactory<Button_seven_days_rewards_get_btn>());

        //half price
        EventCenter.Self.RegisterEvent("Button_carnival_half_buy_button", new DefineFactory<Button_carnival_half_buy_button>());


    }

    public override void Open(object param)
    {
        base.Open(param);

        DateTime startData = DateTime.Parse(CommonParam.mOpenDate);
        Int64 startSeconds = GameCommon.DateTime2TotalSeconds(startData);
        Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime());
        closeGoTaskDate = startSeconds + 24 * 60 * 60 * MAX_DAY;
        closeGetRewardsDate = startSeconds + 24 * 60 * 60 * MAX_GET_DAY;
        if ((nowSeconds - startSeconds) % (24 * 60 * 60) == 0)
        {
            iDaysIndex = (nowSeconds - startSeconds) / (24 * 60 * 60);
        }
        else
        {
            iDaysIndex = (nowSeconds - startSeconds) / (24 * 60 * 60) + 1;
        }
        SetCountdownTime("preform_task_rest_time", closeGoTaskDate, null);
        SetCountdownTime("get_rewards_rest_time", closeGetRewardsDate, null);

        DataCenter.OpenWindow("SEVEN_DAYS_CARNIVAL_WINDOW_BACK");
        mLeftGrid = GetComponent<UIGridContainer>("seven_days_carnival_grid");
        mLeftPanel = GetComponent<UIPanel>("seven_days_carnival_scrollview");
        mLeftView = GetComponent<UIScrollView>("seven_days_carnival_scrollview");
        titleNameGrid = GetComponent<UIGridContainer>("carnival_day_task_grid");
        mRightView = GetComponent<UIScrollView>("preform_task_rewards_scrollView");
        titleNameGrid.MaxCount = DAY_TASK_TYPE_NUM;
        mLeftGrid.MaxCount = MAX_DAY;
        InitHalfPriceData();

        Refresh(null);
    }

    public override bool Refresh(object param)
    {
        RefreshTab();
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
            case "SHOW_WINDOW":
                ShowWindow((int)objVal);
                break;
            case "SHOW_TASK_WINDOW":
                ShowTaskWindow((string)objVal);
                break;
            case "SHOW_HALF_PRICE_WINDOW":
                ShowHalfPriceWindow((string)objVal);
                break;
            case "REFRESH_HALF_PRICE_WINDOW":
                RefreshHalfPriceWindow((string)objVal);
                break;
            case "REFRESH_HALF_PRICE_BUY_WINDOW":
                RefreshHalfPriceBuyWindow((string)objVal);
                break;
            case "GET_REVELRY_LIST":
                SetRevelryList((SC_GetRevelryList)objVal);
                break;
            case "SET_GET_REWARDS":
                RequstGetRewards((int)objVal);
                break;
            case "GET_REVELRY_REWARDS_SUCCESS":
                GetRevelryRewards((string)objVal);
                break;
        }
    }
    void GetRevelryRewards(string text)
    {
        SC_TakeRevelryAward item = JCode.Decode<SC_TakeRevelryAward>(text);

        List<ItemDataBase> itemDataList = PackageManager.UpdateItem(item.awards);
        DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemDataList);
        foreach (var v in revelryObject)
        {
            if (v.revelryId == item.revelryId)
                v.accepted = true;
        }

        string dayTaskIndex = TableCommon.GetNumberFromHDrevelry(item.revelryId, "DAY") + "|" + TableCommon.GetNumberFromHDrevelry(item.revelryId, "PAGE");
        ShowTaskWindow(dayTaskIndex);
        SetMark(TableCommon.GetNumberFromHDrevelry(item.revelryId, "DAY"));
        SetRevelryMark();
    }

    void RequstGetRewards(int iRewardsIndex)
    {
        NetManager.RequestTakeRevelryAward(iRewardsIndex);
    }

    void SetRevelryList(SC_GetRevelryList text)
    {
        SC_GetRevelryList item = text;
        List<RevelryObject> revelryArrList = item.revelryArr.ToList();

        revelryObject = revelryArrList;
        for (int i = 0; i < revelryObject.Count; i++)
        {
            revelryObject[i].revelryId = item.revelryArr[i].revelryId;
            revelryObject[i].progress = item.revelryArr[i].progress;
            revelryObject[i].accepted = item.revelryArr[i].accepted;
        }
        Open(null);
    }

    public void ShowHalfPriceWindow(string dayTaskIndex)
    {
        //打开半价抢购
        SetHalfPriceVisible(true);

        //半价抢购-请求
        if (scHalfPriceQuery == null)
        {
            NetManager.RequestHalfPriceQuery();
        }
        else
        {
            RefreshHalfPriceWindow("");
        }
    }

    //初始化-半价抢购
    public void RefreshHalfPriceWindow(string text)
    {
        //data
        int[] buyArr;
        int[] useArr;
        if (scHalfPriceQuery == null)
        {
            SC_HalfPriceQuery item = JCode.Decode<SC_HalfPriceQuery>(text);
            scHalfPriceQuery = item;
            buyArr = item.buyArr;
            useArr = item.useArr;
        }
        else
        {
            buyArr = scHalfPriceQuery.buyArr;
            useArr = scHalfPriceQuery.useArr;
        }

        //拿到half price 对象
        GameObject halfObject = GameCommon.FindObject(mGameObjUI, "half_buy_group");

        //buttons
        int curDay = GetCurrentDay();
        int curDayIndex = curDay - 1; //索引
        SetButtonGrey(IsBuyedHalfPrice(curDay, buyArr) ? true : false);

        //tips
        int serverLimit = GetHalfPriceServerLimit();
        if (useArr.Length > curDayIndex)
        {
            int leftNum = serverLimit - useArr[curDayIndex];
            string strTips = string.Format(TableCommon.getStringFromStringList(STRING_INDEX.HALF_PRICE_TIPS), serverLimit, leftNum);
            GameCommon.SetUIText(halfObject, "buy_tips_label", strTips);
        }
        else
        {
            string strTips = string.Format(TableCommon.getStringFromStringList(STRING_INDEX.HALF_PRICE_TIPS), serverLimit, serverLimit);
            GameCommon.SetUIText(halfObject, "buy_tips_label", strTips);
        }

        //reward
        string[] reward = GetHalfPriceReward();
        if (reward.Length > 0)
        {
            string[] rewardList = reward[0].Split('#');
            int iRewardsTid = int.Parse(rewardList[0]);
            GameCommon.SetOnlyItemIcon(halfObject, "item_icon", iRewardsTid);
            AddButtonAction(GameCommon.FindObject(halfObject, "item_icon").gameObject, () => GameCommon.SetItemDetailsWindow(iRewardsTid));
            GameCommon.SetUIText(halfObject, "item_num", "x" + rewardList[1]);
            GameCommon.SetUIText(halfObject, "Describe_label", GameCommon.GetItemName(int.Parse(rewardList[0])));
        }

        //花费
        string[] price = GetHalfPricePrice();
        if (price.Length > 1)
        {
            //原价
            string[] priceListCur = price[0].Split('#');
            GameObject objCurLable = GameCommon.FindObject(halfObject, "cur_label");
            ShowCurAimPrice(objCurLable, priceListCur);

            //现价
            string[] priceListAim = price[1].Split('#');
            GameObject objaimLabel = GameCommon.FindObject(halfObject, "aim_label");
            ShowCurAimPrice(objaimLabel, priceListAim);

            GameObject objDiscountLabel = GameCommon.FindObject(halfObject, "discount_sprite");

            int curPriceNum = Convert.ToInt32(priceListCur[1]);
            int aimPriceNum = Convert.ToInt32(priceListAim[1]);
            if (aimPriceNum < curPriceNum)
            {
                objDiscountLabel.SetActive(true);
                int discountNum = aimPriceNum * 10 / curPriceNum;
                string ret = "a_ui_smsddazhe_0" + discountNum;
                GameCommon.SetUISprite(halfObject, "discount_sprite", ret);
            }
            else
            {
                objDiscountLabel.SetActive(false);
            }
        }
    }

    public static bool IsBuyedHalfPrice(int day, int[] arr)
    {
        bool ret = false;
        if (arr == null)
        {
            return ret;
        }
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == day)
            {
                ret = true;
                break;
            }
        }
        return ret;
    }

    public void InitHalfPriceData()
    {
        scHalfPriceQuery = null;
        GameCommon.SetDataByZoneUid("DAY_TASK_INDEX", "1|1");
    }

    public void ShowCurAimPrice(GameObject obj, string[] rewardList)
    {
        if (rewardList.Length > 1)
        {
            GameCommon.SetItemIconNew(obj, "ingot", int.Parse(rewardList[0]), false);
            GameCommon.SetUIText(obj, "price", rewardList[1]);
        }
    }

    public void SetButtonGrey(bool grey)
    {
        GameObject button = GameCommon.FindObject(mGameObjUI, "carnival_half_buy_button");
        button.GetComponent<UIImageButton>().isEnabled = !grey;
    }

    public static int GetHalfPriceIndex()
    {
        string dayTaskIndex = GameCommon.GetDataByZoneUid("DAY_TASK_INDEX");
        int index = TableCommon.GetIndexFromMHdrevelry(dayTaskIndex);
        return index;
    }

    public int GetCurrentDay()
    {
        string dayTaskIndex = GameCommon.GetDataByZoneUid("DAY_TASK_INDEX");
        string[] dayAndTask = dayTaskIndex.Split('|');
        int dayIndex = 1;
        if (dayAndTask.Length > 0)
        {
            dayIndex = System.Convert.ToInt32(dayAndTask[0]);
        }
        return dayIndex;
    }

    public int GetCurrentPage()
    {
        string dayTaskIndex = GameCommon.GetDataByZoneUid("DAY_TASK_INDEX");

        int index = TableCommon.GetIndexFromMHdrevelry(dayTaskIndex);
        string[] dayAndTask = dayTaskIndex.Split('|');
        int taskIndex = 1;
        if (dayAndTask.Length > 1)
        {
            taskIndex = System.Convert.ToInt32(dayAndTask[1]);
        }

        int ret = 1;
        if (IsHalfPricePage())
        {
            ret = taskIndex;
        }
        else
        {
            ret = 1;
        }
        return ret;
    }

    public string[] GetHalfPriceReward()
    {
        int index = GetHalfPriceIndex();
        string reward = TableCommon.GetStringFromConfig(index, "REWARD", DataCenter.mHdrevelry);
        string[] rewardTemp = reward.Split('|');
        return rewardTemp;
    }

    public static string[] GetHalfPricePrice()
    {
        int index = GetHalfPriceIndex();
        string reward = TableCommon.GetStringFromConfig(index, "PRICE", DataCenter.mHdrevelry);
        string[] rewardTemp = reward.Split('|');
        return rewardTemp;
    }

    public static int GetHalfPriceServerLimit()
    {
        int index = GetHalfPriceIndex();
        int temp = TableCommon.GetNumberFromConfig(index, "SERVER_LIMIT", DataCenter.mHdrevelry);
        return temp;
    }

    public static bool IsOverHalfPriceServelimit(int index, int[] useArr)
    {
        int remained = 0;
        if (useArr == null)
        {
            return false;
        }
        int serverLimit = TableCommon.GetNumberFromConfig(index, "SERVER_LIMIT", DataCenter.mHdrevelry);
        int day = TableCommon.GetNumberFromConfig(index, "DAY", DataCenter.mHdrevelry);
        if (useArr.Length > day)
        {
            remained = serverLimit - useArr[day];
        }
        else
        {
            remained = serverLimit;
        }
        return remained > 0 ? false : true;
    }

    public static SC_GetRevelryList UpdateRevelryListByHalfQuery(SC_HalfPriceQuery halfQuery, SC_GetRevelryList itemList)
    {
        foreach (var item in itemList.revelryArr)
        {
            int page = TableCommon.GetNumberFromConfig(item.revelryId, "PAGE", DataCenter.mHdrevelry);
            int day = TableCommon.GetNumberFromConfig(item.revelryId, "DAY", DataCenter.mHdrevelry);
            if (page == (int)SEVENDAY_PAGE.PAGE_FOUR && (IsBuyedHalfPrice(day, halfQuery.buyArr) || IsOverHalfPriceServelimit(item.revelryId, halfQuery.useArr)))
            {
                item.accepted = true;
            }
        }
        return itemList;
    }

    //购买刷新-初始化-半价抢购
    public void RefreshHalfPriceBuyWindow(string text)
    {
        SC_HalfPrice item = JCode.Decode<SC_HalfPrice>(text);

        //get reward
        ItemDataBase[] itemList = item.buyItem;
        List<ItemDataBase> list = PackageManager.UpdateItem(itemList, true);
        DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", list);

        //刷新界面
        UpdateBuyHalfPriceData();
        UpdateNewMark(GetHalfPriceIndex());
    }

    void UpdateNewMark(int index)
    {
        //更新data
        foreach (var v in revelryObject)
        {
            if (v.revelryId == index)
                v.accepted = true;
        }

        //刷新红点
        SetMark(GetCurrentDay());
        SetRevelryMark();
    }

    public void UpdateBuyHalfPriceData()
    {
        //扣花费
        string[] price = GetHalfPricePrice();
        if (price.Length > 1)
        {
            //原价
            string[] priceListCur = price[1].Split('#');
            if (priceListCur.Length > 1)
            {
                PackageManager.RemoveItem(new ItemDataBase() { tid = int.Parse(priceListCur[0]), itemNum = int.Parse(priceListCur[1]) });
            }
        }
        scHalfPriceQuery = null;
        ShowHalfPriceWindow(GameCommon.GetDataByZoneUid("DAY_TASK_INDEX"));
    }

    void SetHalfPriceVisible(bool visible)
    {
        GameCommon.FindObject(mGameObjUI, "preform_task_rewards_group").SetActive(!visible);
        GameCommon.FindObject(mGameObjUI, "half_buy_group").SetActive(visible);
    }

    bool IsHalfPricePage()
    {
        string dayTaskIndex = GameCommon.GetDataByZoneUid("DAY_TASK_INDEX");
        int index = TableCommon.GetIndexFromMHdrevelry(dayTaskIndex);
        int page = TableCommon.GetNumberFromConfig(index, "PAGE", DataCenter.mHdrevelry);
        if (page == (int)SEVENDAY_PAGE.PAGE_FOUR)
        {
            return true;
        }
        return false;
    }

    void ResultSort(List<RevelryId> result)
    {
        result.Sort((RevelryId a, RevelryId b) =>
        {
            RevelryObject tempA = new RevelryObject();
			RevelryObject tempB = new RevelryObject();

            foreach (RevelryObject revelry in revelryObject)
            {
                if (a.revelryId == revelry.revelryId)
                {
                    tempA = revelry;
                }
				else if (b.revelryId == revelry.revelryId)
				{
					tempB = revelry;
				}
            }

			int NeedNumA = TableCommon.GetNumberFromHDrevelry(tempA.revelryId, "NUMBER");
			int NeedNumB = TableCommon.GetNumberFromHDrevelry(tempB.revelryId, "NUMBER");

			if (!tempA.accepted)
            {
				if(tempB.accepted)
				{
                	return -1;
				}
				else
				{
					if(tempA.progress >= NeedNumA)
					{
						if(tempB.progress >= NeedNumB)
						{
							if(tempA.revelryId > tempB.revelryId)
							{
								return 1;
							}
							else
							{
								return -1;
							}
						}
						else
						{
							return -1;
						}
					}
					else
					{
						if(tempB.progress >= NeedNumB)
						{
							return 1;
						}
						else
						{
							if(tempA.revelryId > tempB.revelryId)
							{
								return 1;
							}
							else
							{
								return -1;
							}
						}
					}
				}
            }
            else
            {
				if(tempB.accepted)
				{

					if(tempA.revelryId > tempB.revelryId)
					{
						return 1;
					}
					else
					{
						return -1;
					}

				}
				else
				{
					return 1;
				}
            }
        });

    }

    void ShowTaskWindow(string dayTaskIndex)
    {
        //关闭半价抢购
        SetHalfPriceVisible(false);

        mRightView.ResetPosition();
        string[] dayAndTask = dayTaskIndex.Split('|');

        int dayIndex = System.Convert.ToInt32(dayAndTask[0]);
        int taskIndex = System.Convert.ToInt32(dayAndTask[1]);

        int taskNum = 0;
        List<RevelryId> result = new List<RevelryId>();

        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mHdrevelry.GetAllRecord())
        {
            DataRecord r = pair.Value;
            if (r["DAY"] == dayIndex && r["PAGE"] == taskIndex)
            {
                taskNum++;
                int iId = r["INDEX"];
                result.Add(new RevelryId() { revelryId = iId, iIndex = taskNum });
            }
        }

        UIGridContainer preformTaskRewardsGrid = GetComponent<UIGridContainer>("preform_task_rewards_grid");
        preformTaskRewardsGrid.MaxCount = taskNum;

        ResultSort(result);

        for (int i = 0; i < preformTaskRewardsGrid.MaxCount; i++)
        {
            GameObject obj = preformTaskRewardsGrid.controlList[i];

            GameObject goPreformTaskBtnObj = GameCommon.FindObject(obj, "go_preform_task_btn").gameObject;
            GameObject sevenDaysRewardsGetBtnObj = GameCommon.FindObject(obj, "seven_days_rewards_get_btn").gameObject;
            GameObject rewardsHaveGetBtnObj = GameCommon.FindObject(obj, "rewards_have_get_btn").gameObject;
            UILabel preformTaskProgressNumObj = GameCommon.FindObject(obj, "preform_task_progress_num").GetComponent<UILabel>();
            UILabel progressTitleObj = GameCommon.FindComponent<UILabel>(obj, "preform_task_progress_label");
            UILabel preformTaskName = GameCommon.FindObject(obj, "preform_task_name").GetComponent<UILabel>();

            RevelryObject revelryObjs = new RevelryObject();
            foreach (RevelryObject ro in revelryObject)
            {
                if (ro.revelryId == result[i].revelryId)
                {
                    revelryObjs = ro;
                    revelryObjs.revelryId = ro.revelryId;
                    revelryObjs.progress = ro.progress;
                    revelryObjs.accepted = ro.accepted;
                }
            }
            goPreformTaskBtnObj.SetActive(false);
            sevenDaysRewardsGetBtnObj.SetActive(!revelryObjs.accepted);
            rewardsHaveGetBtnObj.SetActive(revelryObjs.accepted);
            int iNeedNum = TableCommon.GetNumberFromHDrevelry(result[i].revelryId, "NUMBER");
            int iTaskType = TableCommon.GetNumberFromHDrevelry(result[i].revelryId, "TYPE");
            //added by xuke  竞技场进度如果没完成则显示0/1，完成则显示1/1
            if(preformTaskProgressNumObj != null)
                preformTaskProgressNumObj.gameObject.SetActive(iTaskType != 1);
            if (progressTitleObj != null)
                progressTitleObj.gameObject.SetActive(iTaskType != 1);
            if (iTaskType == 6 || iTaskType == 21)
            {
                preformTaskProgressNumObj.text = (revelryObjs.progress <= iNeedNum && revelryObjs.progress != 0) ? "1/1" : "0/1";
            }
            //by chenliang
            //begin

            else if (iTaskType == 11)
            {
                preformTaskProgressNumObj.text = revelryObjs.progress / 100 + "/" + iNeedNum / 100;
            }

            //end
            else
            {
                preformTaskProgressNumObj.text = revelryObjs.progress + "/" + iNeedNum;
            }

            if (iTaskType != null)
            {
                if (iTaskType == 6 || iTaskType == 21)
                {
                    if (revelryObjs.progress <= iNeedNum && revelryObjs.progress != 0)
                        sevenDaysRewardsGetBtnObj.GetComponent<UIImageButton>().isEnabled = true;
                    else
                        sevenDaysRewardsGetBtnObj.GetComponent<UIImageButton>().isEnabled = false;
                }
                else
                {
                    if (revelryObjs.progress < iNeedNum)      //为什么此处不加revelryObjs.progress != 0
                        sevenDaysRewardsGetBtnObj.GetComponent<UIImageButton>().isEnabled = false;
                    else
                        sevenDaysRewardsGetBtnObj.GetComponent<UIImageButton>().isEnabled = true;
                }
            }

            GameCommon.GetButtonData(sevenDaysRewardsGetBtnObj).set("DAY_TASK_REWARDS_INDEX", result[i].revelryId);
            //			GameCommon.GetButtonData(goPreformTaskBtnObj).set ("DAY_TASK_INDEX", result[i].revelryId);

            preformTaskName.text = TableCommon.GetStringFromHDrevelry(result[i].revelryId, "TASK_TIP");
            string rewards = TableCommon.GetStringFromHDrevelry(result[i].revelryId, "REWARD");
            string[] rewardsInfo = rewards.Split('|');
            int rewardsTypeNum = rewardsInfo.Length;
            for (int j = 0; j < rewardsTypeNum; j++)
            {
                if (rewardsInfo[j] == "0" || rewardsInfo[j] == "")
                {
                    rewardsTypeNum--;
                }
            }


            UIGridContainer rewardsGrid = GameCommon.FindComponent<UIGridContainer>(obj, "rewards_grid");
            rewardsGrid.MaxCount = rewardsTypeNum;
            if (rewardsGrid != null)
            {
                for (int num = 0; num < rewardsGrid.MaxCount; num++)
                {
                    string[] rewardIdAndNum = rewardsInfo[num].Split('#');
                    int iRewardsTid = System.Convert.ToInt32(rewardIdAndNum[0]);
                    string iRewardsNumStr = rewardIdAndNum[1];
                    GameObject rewardsObj = rewardsGrid.controlList[num];
                    GameCommon.SetOnlyItemIcon(rewardsObj, "item_icon", iRewardsTid);
                    AddButtonAction(GameCommon.FindObject(rewardsObj, "item_icon").gameObject, () => GameCommon.SetItemDetailsWindow(iRewardsTid));
                    UILabel itemNameLabel = GameCommon.FindObject(rewardsObj, "rewards_name_label").GetComponent<UILabel>();
                    UILabel itemNumLabel = GameCommon.FindObject(rewardsObj, "rewards_num_label").GetComponent<UILabel>();
                    itemNameLabel.text = GameCommon.GetItemName(iRewardsTid);
                    itemNameLabel.color = GameCommon.GetNameColor(iRewardsTid);
                    itemNameLabel.effectColor = GameCommon.GetNameEffectColor();
                    itemNumLabel.text = "x" + iRewardsNumStr;
                }
                if (rewardsGrid.MaxCount == 3)
                {
                    rewardsGrid.CellWidth = 190;
                }
            }
        }
    }
    void SetRevelryMark()
    {
        int iCanGetRewardsNum = 0;
        foreach (var v in revelryObject)
        {
            int iDay = TableCommon.GetNumberFromHDrevelry(v.revelryId, "DAY");
            int iNeedNum = TableCommon.GetNumberFromHDrevelry(v.revelryId, "NUMBER");
            int iDayTask = TableCommon.GetNumberFromHDrevelry(v.revelryId, "PAGE");
            int iTaskType = TableCommon.GetNumberFromHDrevelry(v.revelryId, "TYPE");

            if (iTaskType != null)
            {
                if (iTaskType != 6 && iTaskType != 21)
                {
                    if (v.progress >= iNeedNum && v.accepted == false && CheckToadayWithRevelryDay(iDay))
                    {
                        GameCommon.FindObject(mLeftGrid.controlList[iDay - 1], "NewMark").gameObject.SetActive(true);
                        iCanGetRewardsNum++;
                    }
                }
                else
                {
                    if (v.progress <= iNeedNum && v.accepted == false && v.progress != 0 && CheckToadayWithRevelryDay(iDay))
                    {
                        GameCommon.FindObject(mLeftGrid.controlList[iDay - 1], "NewMark").gameObject.SetActive(true);
                        iCanGetRewardsNum++;
                    }
                }
            }
        }
        SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_REVELRY, iCanGetRewardsNum > 0);
    }

    void SetMark(int dayIndex)
    {
        for (int i = 0; i < mLeftGrid.MaxCount; i++)
        {
            GameCommon.FindObject(mLeftGrid.controlList[i], "NewMark").gameObject.SetActive(false);
        }
        for (int i = 0; i < titleNameGrid.MaxCount; i++)
        {
            GameCommon.FindObject(titleNameGrid.controlList[i], "NewMark").gameObject.SetActive(false);
        }

        foreach (var v in revelryObject)
        {
            int iDay = TableCommon.GetNumberFromHDrevelry(v.revelryId, "DAY");
            int iNeedNum = TableCommon.GetNumberFromHDrevelry(v.revelryId, "NUMBER");
            int iDayTask = TableCommon.GetNumberFromHDrevelry(v.revelryId, "PAGE");
            int iTaskType = TableCommon.GetNumberFromHDrevelry(v.revelryId, "TYPE");
            //added by xuke 半价抢购活动暂时没做 -- 
            if (iDayTask > DAY_TASK_TYPE_NUM)
                continue;
            //end
            if (iTaskType != null)
            {
                if (iTaskType != 6 && iTaskType != 21)
                {
                    if (v.progress >= iNeedNum && v.accepted == false && CheckToadayWithRevelryDay(iDay))
                    {
                        GameCommon.FindObject(mLeftGrid.controlList[iDay - 1], "NewMark").gameObject.SetActive(true);
                    }
                    if (iDay == dayIndex && v.progress >= iNeedNum && v.accepted == false && CheckToadayWithRevelryDay(iDay))
                    {
                        GameCommon.FindObject(titleNameGrid.controlList[iDayTask - 1], "NewMark").gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (v.progress <= iNeedNum && v.accepted == false && v.progress != 0 && CheckToadayWithRevelryDay(iDay))
                    {
                        GameCommon.FindObject(mLeftGrid.controlList[iDay - 1], "NewMark").gameObject.SetActive(true);
                    }
                    if (iDay == dayIndex && v.progress <= iNeedNum && v.accepted == false && v.progress != 0 && CheckToadayWithRevelryDay(iDay))
                    {
                        GameCommon.FindObject(titleNameGrid.controlList[iDayTask - 1], "NewMark").gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 判断今天是否已经达到或超过任务的天数
    /// </summary>
    /// <returns></returns>
    private bool CheckToadayWithRevelryDay(int kRevelryDay)
    {
        return iDaysIndex >= kRevelryDay;
    }

    void ShowWindow(int dayIndex)
    {
        for (int i = 0; i < titleNameGrid.MaxCount; i++)
        {
            GameObject obj = titleNameGrid.controlList[i];
            obj.transform.name = "revelry_day_task_button";
            if (i + 1 == getShowPage()) GameCommon.ToggleTrue(obj); ;
            string title = "";
            foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mHdrevelry.GetAllRecord())
            {
                DataRecord r = pair.Value;
                if (r["DAY"] == dayIndex)
                {
                    if (r["PAGE"] == (i + 1))
                    {
                        title = TableCommon.GetStringFromHDrevelry(r["INDEX"], "PAGENAME");
                    }
                }
            }
            foreach (UILabel label in obj.GetComponentsInChildren<UILabel>())
            {
                label.text = title;
            }
            string dayAndTaskIndex = dayIndex + "|" + (i + 1);
            GameCommon.GetButtonData(obj).set("DAY_TASK_INDEX", dayAndTaskIndex);
        }

        if (IsHalfPricePage())
        {
            string dayTaskIndex = dayIndex.ToString() + "|" + getShowPage();
            GameCommon.SetDataByZoneUid("DAY_TASK_INDEX", dayTaskIndex);
            ShowHalfPriceWindow(GameCommon.GetDataByZoneUid("DAY_TASK_INDEX"));
        }
        else
        {
            ShowTaskWindow(dayIndex + "|" + getShowPage());
        }
        SetMark(dayIndex);
    }

    void ChangeTabPos(int pos)
    {
        if (pos < 0 || pos > mLeftGrid.MaxCount - 1) return;

        if (pos == 0 || pos == mLeftGrid.MaxCount - 1)
        {
            mLeftView.SetDragAmount(0, pos / (mLeftGrid.MaxCount - 1), false);
            return;
        }

        bool bBack = mLeftPanel.IsVisible(mLeftGrid.controlList[pos - 1].transform.position);
        bool bBefore = mLeftPanel.IsVisible(mLeftGrid.controlList[pos + 1].transform.position);

        if (!bBack)
        {
            mLeftView.SetDragAmount(0, (float)(pos - 1) / (float)mLeftGrid.MaxCount, false);
        }
        else if (!bBefore)
        {
            mLeftView.SetDragAmount(0, (float)(pos + 1) / (float)mLeftGrid.MaxCount, false);
        }
    }

    void RefreshTab()
    {
        for (int i = 0; i < MAX_DAY; i++)
        {
            string title = "第" + (i + 1) + "天";
            GameObject titleObj = mLeftGrid.controlList[i];
            GameObject titleGroupObj = titleObj.transform.Find("revelry_button").gameObject;
            GameObject titleNoOpenObj = titleObj.transform.Find("no_open_button").gameObject;

            if (i == 0) GameCommon.ToggleTrue(titleGroupObj);
            foreach (UILabel l in titleGroupObj.GetComponentsInChildren<UILabel>())
            {
                if (l != null)
                    l.text = title;
            }
            GameCommon.GetButtonData(titleGroupObj).set("POS", i);
            GameCommon.GetButtonData(titleNoOpenObj).set("POS", i);
            GameCommon.GetButtonData(titleGroupObj).set("INDEX", i + 1);
            if (i + 1 <= iDaysIndex)
            {
                titleNoOpenObj.SetActive(false);
            }
            else
                titleNoOpenObj.SetActive(true);
        }
        mLeftView.ResetPosition();

        ShowWindow(1);
        //SetMark(1);
    }

    int getShowPage()
    {
        int ret = 1;  //默认第一页
        ret = GetCurrentPage();
        if (ret == 0)
        {
            ret = 1;
        }
        return ret;
    }

    public override void Close()
    {
        base.Close();
        DataCenter.CloseWindow("SEVEN_DAYS_CARNIVAL_WINDOW_BACK");
    }
}

public class Button_carnival_window_back_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("SEVEN_DAYS_CARNIVAL_WINDOW");
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
        return true;
    }
}
public class Button_revelry_button : CEvent
{
    public override bool _DoEvent()
    {
        int pos = (int)getObject("POS");
        DataCenter.SetData("SEVEN_DAYS_CARNIVAL_WINDOW", "CHANGE_TAB_POS", pos);

        int configIndex = (int)getObject("INDEX");
        DataCenter.SetData("SEVEN_DAYS_CARNIVAL_WINDOW", "SHOW_WINDOW", configIndex);
        return true;
    }
}
public class Button_no_open_button : CEvent
{
    public override bool _DoEvent()
    {
        int pos = (int)getObject("POS");
        DataCenter.SetData("SEVEN_DAYS_CARNIVAL_WINDOW", "CHANGE_TAB_POS", pos);
        DataCenter.ErrorTipsLabelMessage("尚未开启哟！！");
        return true;
    }
}
public class Button_revelry_day_task_button : CEvent
{
    public override bool _DoEvent()
    {
        string dayTaskIndex = (string)getObject("DAY_TASK_INDEX");
        GameCommon.SetDataByZoneUid("DAY_TASK_INDEX", dayTaskIndex);

        int index = TableCommon.GetIndexFromMHdrevelry(dayTaskIndex);
        int page = TableCommon.GetNumberFromConfig(index, "PAGE", DataCenter.mHdrevelry);
        if (page == (int)SEVENDAY_PAGE.PAGE_FOUR)
        {
            DataCenter.SetData("SEVEN_DAYS_CARNIVAL_WINDOW", "SHOW_HALF_PRICE_WINDOW", dayTaskIndex);
        }
        else
        {
            DataCenter.SetData("SEVEN_DAYS_CARNIVAL_WINDOW", "SHOW_TASK_WINDOW", dayTaskIndex);
        }
        return true;
    }
}
public class Button_seven_days_rewards_get_btn : CEvent
{
    public override bool _DoEvent()
    {
        int dayTaskRewardsIndex = (int)getObject("DAY_TASK_REWARDS_INDEX");
        DataCenter.SetData("SEVEN_DAYS_CARNIVAL_WINDOW", "SET_GET_REWARDS", dayTaskRewardsIndex);
        return true;
    }
}

public class Button_carnival_half_buy_button : CEvent
{
    private string dayTaskIndex = "";
    private int tid = 0;
    public override bool _DoEvent()
    {
        dayTaskIndex = GameCommon.GetDataByZoneUid("DAY_TASK_INDEX");
        string[] dayAndTask = dayTaskIndex.Split('|');
        int dayIndex = 1;
        if (dayAndTask.Length > 0)
        {
            dayIndex = System.Convert.ToInt32(dayAndTask[0]);
        }

        if (isEnoughNum())
        {
            NetManager.RequestHalfPrice(dayIndex);
        }
        else
        {
            //string tmpCostName = GameCommon.GetItemName(tid);
            //DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_MYSTERIOUS_NO_ENGOUGH_COST, tmpCostName);
            GameCommon.ToGetDiamond();
        }
        return true;
    }
    public bool isEnoughNum()
    {
        int index = TableCommon.GetIndexFromMHdrevelry(dayTaskIndex);
        string reward = TableCommon.GetStringFromConfig(index, "PRICE", DataCenter.mHdrevelry);
        string[] price = reward.Split('|');
        string[] priceHalfCur;
        int num = 0;
        if (price.Length > 1)
        {
            //原价
            priceHalfCur = price[1].Split('#');
            if (priceHalfCur.Length > 1)
            {
                tid = int.Parse(priceHalfCur[0]);
                num = int.Parse(priceHalfCur[1]);
            }
        }
        if (PackageManager.GetItemLeftCount(tid) >= num)
        {
            return true;
        }
        return false;
    }
}

