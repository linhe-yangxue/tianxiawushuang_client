using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;
using System;

//符灵探险操作界面右边

public class FairylandRightWindow : tWindow
{
    protected static string[] smSearchWayName = new string[] {
        "普通",
        "中级",
        "高级"
    };
    protected static string[] smSubWindow = new string[] {
        "explore_right_title",
        "explore_award_list",
        "explore_change_window",
        "explore_fight_window",
        "explore_reward_window",
        "explore_start_button",
        "explore_riot_window",
        "explore_start_window"
    };

    public override void Open(object param)
    {
        base.Open(param);

        __HideAllWindow();
        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        FairylandOPWindowData tmpOPData = param as FairylandOPWindowData;
        _OnRefresh(tmpOPData);

        return true;
    }

    /// <summary>
    /// 隐藏有子窗口
    /// </summary>
    private void __HideAllWindow()
    {
        for (int i = 0, count = smSubWindow.Length; i < count; i++)
            SetVisible(smSubWindow[i], false);
    }

    protected virtual void _OnRefresh(FairylandOPWindowData opData)
    {
    }

    /// <summary>
    /// 设置奖励列表
    /// </summary>
    /// <param name="gridContainer"></param>
    /// <param name="listAward"></param>
    /// <param name="beginIdx"></param>
    /// <param name="endIdx"></param>
    protected void _RefreshAwardList(GameObject gridContainer, List<ItemDataBase> listAward, int beginIdx, int endIdx)
    {
        if (gridContainer == null)
            return;
        gridContainer.SetActive(true);
        if (beginIdx == -1)
        {
            beginIdx = 0;
            endIdx = listAward != null ? listAward.Count - 1 : 0;
        }
        if(beginIdx > endIdx)
            beginIdx = endIdx;

        UIGridContainer tmpGridContainer = GameCommon.FindComponent<UIGridContainer>(gridContainer, "item_group");
        int tmpCount = (endIdx != beginIdx) ? endIdx - beginIdx + 1 : 0;
        tmpGridContainer.MaxCount = tmpCount;

		Dictionary<int,int> _sameItemDic = CombineSameItem (listAward);
		tmpGridContainer.MaxCount = _sameItemDic.Count;
		int _itemIndex = 0;
		foreach(KeyValuePair<int,int> kItem in _sameItemDic)
		{
			int tmpIdx = beginIdx + _itemIndex;
			GameObject tmpItem = tmpGridContainer.controlList[_itemIndex];

            //碎片判断
            bool isFragment = GameCommon.CheckIsFragmentByTid(kItem.Key);
			int _iTid = kItem.Key;
            if (isFragment)
            {
                GameCommon.SetOnlyItemIcon(tmpItem, "item_icon", kItem.Key - 10000);
            }
            else
            {
                GameCommon.SetOnlyItemIcon(tmpItem, "item_icon", kItem.Key);
            }
			AddButtonAction (tmpItem, () => GameCommon.SetItemDetailsWindow (_iTid));
            GameCommon.SetUIVisiable(tmpItem, "item_suipian", isFragment);
//            int _itemID = kItem.Key;
//            UIButtonEvent _btnEvent = tmpItem.GetComponent<UIButtonEvent>();
//            if (_btnEvent != null)
//            {
//                _btnEvent.AddAction(() => 
//                {
//                    bool temp = isFragment;
//                    if (isFragment)
//                    {
//                        DataCenter.OpenWindow("GRABTREASURE_COMPOSE_RESULT_WINDOW", _itemID);
//                    }
//                });
//            }
			GameCommon.SetUIText(tmpItem,"item_num","x"+kItem.Value.ToString());
			_itemIndex++;
		}
        tmpGridContainer.transform.parent.GetComponent<UIScrollView>().ResetPosition();
    }

	// 将同类型的道具整合到一起
	private Dictionary<int,int> CombineSameItem(List<ItemDataBase> kListAward)
	{
		Dictionary<int,int> _sameItemDic = new Dictionary<int,int> ();
		for (int i = 0; i < kListAward.Count; i++) 
		{
			ItemDataBase _tmpItem = kListAward[i];
			if(!_sameItemDic.ContainsKey(_tmpItem.tid))
				_sameItemDic.Add(_tmpItem.tid,_tmpItem.itemNum);
			else
				_sameItemDic[_tmpItem.tid] = _sameItemDic[_tmpItem.tid] + _tmpItem.itemNum;
		}
		return _sameItemDic;
	}
    /// <summary>
    /// 设置奖励列表
    /// </summary>
    /// <param name="gridContainer"></param>
    /// <param name="listAward"></param>
    protected void _RefreshAwardList(GameObject gridContainer, List<ItemDataBase> listAward)
    {
        _RefreshAwardList(gridContainer, listAward, -1, -1);
    }

    /// <summary>
    /// 获取寻仙持续时间
    /// </summary>
    /// <param name="timeType"></param>
    /// <returns></returns>
    public static int GetFairylandSearchTime(FAIRYLAND_SEARCH_TIME_TYPE timeType)
    {
        int time = 0;
        switch (timeType)
        {
            case FAIRYLAND_SEARCH_TIME_TYPE.FOUR_HOUR: time = 4; break;
            case FAIRYLAND_SEARCH_TIME_TYPE.EIGHT_HOUR: time = 8; break;
            case FAIRYLAND_SEARCH_TIME_TYPE.TWELVE_HOUR: time = 12; break;
        }
        return time;
    }
    /// <summary>
    /// 获取寻仙持续时间
    /// </summary>
    /// <param name="timeType"></param>
    /// <returns></returns>
    public static string GetFairylandSearchTimeString(FAIRYLAND_SEARCH_TIME_TYPE timeType)
    {
        string strTime = "";
        switch (timeType)
        {
            case FAIRYLAND_SEARCH_TIME_TYPE.FOUR_HOUR: strTime = "04"; break;
            case FAIRYLAND_SEARCH_TIME_TYPE.EIGHT_HOUR: strTime = "08"; break;
            case FAIRYLAND_SEARCH_TIME_TYPE.TWELVE_HOUR: strTime = "12"; break;
        }
        return strTime;
    }
    /// <summary>
    /// 获取寻仙方式名称
    /// </summary>
    /// <param name="timeType"></param>
    /// <returns></returns>
    public static string GetFairylandSearchWayName(FAIRYLAND_SEARCH_WAY_TYPE wayType)
    {
        string strName = smSearchWayName[(int)wayType];
        return strName;
    }

    /// <summary>
    /// 创建仙境消耗Id
    /// </summary>
    /// <param name="wayType"></param>
    /// <param name="timeType"></param>
    /// <returns></returns>
    public static int MakeFairylandCostId(FAIRYLAND_SEARCH_WAY_TYPE wayType, FAIRYLAND_SEARCH_TIME_TYPE timeType)
    {
        int tmpWay = (int)wayType + 1;
        string tmpTime = GetFairylandSearchTimeString(timeType);
        string tmpStr = tmpWay.ToString() + tmpTime;
        return Convert.ToInt32(tmpStr);
    }
}

/// <summary>
/// 仙境挑战准备
/// </summary>
public class FairylandRightReadyWindow : FairylandRightWindow
{
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_explore_fight_adjust_button", new DefineFactoryLog<Button_explore_fight_adjust_button>());
        EventCenter.Self.RegisterEvent("Button_explore_fight_challenge_button", new DefineFactoryLog<Button_explore_fight_challenge_button>());
    }

    protected override void _OnRefresh(FairylandOPWindowData opData)
    {
        base._OnRefresh(opData);

        GameObject tmpObj = GetSub("explore_fight_window");
        tmpObj.gameObject.SetActive(true);

        DataRecord tmpFairylandConfig = DataCenter.mFairylandConfig.GetRecord(opData.FairylandTid);

        //我方战斗力
        //TODO
        //int tmpFightPoint = UnityEngine.Random.Range(1000, 100000);
        GameCommon.SetUIText(tmpObj, "fight_strength_number", GameCommon.GetPower().ToString("f0"));

        //战斗奖励
        int tmpGroupId = (int)tmpFairylandConfig.getObject("STATE_REARD");
        List<ItemDataBase> tmpListAward = GameCommon.GetItemGroup(tmpGroupId, true);
        _RefreshAwardList(GameCommon.FindObject(mGameObjUI, "explore_award_list"), tmpListAward);

        //推荐战力
        GameCommon.SetUIText(GameCommon.FindObject(tmpObj, "fight_tips_label"), "num_label", tmpFairylandConfig.getData("FIGHTING"));

        NiceData tmpBtnChangeTeamData = GameCommon.GetButtonData(tmpObj, "explore_fight_adjust_button");
        if (tmpBtnChangeTeamData != null)
            tmpBtnChangeTeamData.set("FAIRYLAND_ID", opData.FairylandTid);

        NiceData tmpBtnChallengeData = GameCommon.GetButtonData(tmpObj, "explore_fight_challenge_button");
        if (tmpBtnChallengeData != null)
        {
            tmpBtnChallengeData.set("AWARD_GROUP_ID", tmpGroupId);
            tmpBtnChallengeData.set("FAIRYLAND_ID", opData.FairylandTid);
        }
        //added by xuke
        GameObject _teamAdjustBtnObj = GameCommon.FindObject(mGameObjUI, "explore_fight_adjust_button");
        GameCommon.SetNewMarkVisible(_teamAdjustBtnObj, TeamManager.CheckTeamHasNewMark());
        //end
    }
}
/// <summary>
/// 调整队伍
/// </summary>
class Button_explore_fight_adjust_button : CEvent
{
    private GameObject mFairylandMainObj;

    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("FAIRYLAND_BACK_WINDOW");
		DataCenter.CloseWindow ("TRIAL_WINDOW_BACK");
		DataCenter.CloseWindow ("TRIAL_WINDOW");
        FairylandWindow tmpWin = DataCenter.Self.getData("FAIRYLAND_WINDOW") as FairylandWindow;
        mFairylandMainObj = tmpWin.mGameObjUI;
        if (mFairylandMainObj != null)
            mFairylandMainObj.SetActive(false);

        MainUIScript.Self.mWindowBackAction = __OnComplete;
        DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);
        MainUIScript.Self.HideMainBGUI();
        return true;
    }

    /// <summary>
    /// 调整队伍完成
    /// </summary>
    private void __OnComplete()
    {
//        GlobalModule.DoCoroutine(__DoAction());
        DataCenter.OpenWindow("FAIRYLAND_BACK_WINDOW");
        if (mFairylandMainObj != null)
            mFairylandMainObj.SetActive(true);
    }

    private IEnumerator __DoAction()
    {
        yield return GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates());
        tEvent tmpEvt = EventCenter.Self.StartEvent("Button_explore_main_common_btn");
        tmpEvt.DoEvent();
    }
}
/// <summary>
/// 挑战
/// </summary>
class Button_explore_fight_challenge_button : CEvent
{
    public override bool _DoEvent()
    {
        //检查背包是否已满
        int tmpAwardGroupID = (int)getObject("AWARD_GROUP_ID");
        List<ItemDataBase> tmpAwardItems = GameCommon.GetItemGroup(tmpAwardGroupID, false);
        List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(tmpAwardItems);
        if (!CheckPackage.Instance.CanExploreFairyland(tmpPackageTypes))
            return true;

        int tmpFairylandId = (int)getObject("FAIRYLAND_ID");
        DataCenter.Set("QUIT_BACK_SCENE", QUIT_BACK_SCENE_TYPE.FAIRYLAND);
        GlobalModule.DoCoroutine(FairylandNetManager.RequestConquerFairylandStart(tmpFairylandId));

        return true;
    }
}

/// <summary>
/// 仙境奖励
/// </summary>
public class FairylandRightAwardWindow : FairylandRightWindow
{
    protected override void _OnRefresh(FairylandOPWindowData opData)
    {
        base._OnRefresh(opData);

        DataRecord tmpFairylandConfig = DataCenter.mFairylandConfig.GetRecord(opData.FairylandTid);
        string tmpStrItems = tmpFairylandConfig.getData("REWARD");
        List<ItemDataBase> tmpListItems = GameCommon.ParseItemList(tmpStrItems);
        int tmpCount = tmpListItems.Count;

        GameObject tmpParent = GetSub("explore_change_window");

        List<ItemDataBase> tmpPetListItems = new List<ItemDataBase>();
        List<ItemDataBase> tmpNoPetListItems = new List<ItemDataBase>();
        int tmpFindPetCount = 0, tmpFindNoPetCount = 0;
        for (int i = 0; i < tmpCount; i++)
        {
            ItemDataBase tmpItemData = tmpListItems[i];
            ITEM_TYPE tmpType = PackageManager.GetItemTypeByTableID(tmpItemData.tid);
            if (tmpType == ITEM_TYPE.PET && tmpFindPetCount < 3)
            {
                tmpPetListItems.Add(tmpItemData);
                tmpFindPetCount += 1;
            }
            else if (tmpType != ITEM_TYPE.PET && tmpFindNoPetCount < 3)
            {
                tmpNoPetListItems.Add(tmpItemData);
                tmpFindNoPetCount += 1;
            }
            if (tmpFindPetCount >= 3 && tmpFindNoPetCount >= 3)
                break;
        }

        //获得符灵
        _RefreshAwardList(GameCommon.FindObject(tmpParent, "info_bg(Clone)_0"), tmpPetListItems);
        //获得资源
        _RefreshAwardList(GameCommon.FindObject(tmpParent, "info_bg(Clone)_1"), tmpNoPetListItems);

        GlobalModule.DoCoroutine(IEShowAwardWindow(tmpParent));
    }

    // By XiaoWen
    // Bug #13249【寻仙】蓬莱仙境点击进入显示是空的
    // 由于SetActive自身机制的问题，在显示主窗口时会导致一些子窗口虽然加载完图片但是有显示不全的问题，所以这里采用延后显示窗口以等待图片的加载完成
    // Begin
    IEnumerator IEShowAwardWindow(GameObject obj)
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForEndOfFrame();
        }

        obj.SetActive(true);
    }
    // End
}

/// <summary>
/// 寻仙时间类型
/// </summary>
public enum FAIRYLAND_SEARCH_TIME_TYPE
{
    FOUR_HOUR,
    EIGHT_HOUR,
    TWELVE_HOUR
}
/// <summary>
/// 寻仙方式类型
/// </summary>
public enum FAIRYLAND_SEARCH_WAY_TYPE
{
    NORMAL,
    MIDDLE,
    HIGH
}
/// <summary>
/// 准备寻仙
/// </summary>
public class FairylandRightSearchReadyWindow : FairylandRightWindow
{
    private int mSelPetId = -1;             //选择的符灵ItemId
    private int mSelPetTid = -1;             //选择的符灵Id
    private FAIRYLAND_SEARCH_TIME_TYPE mSelTime = FAIRYLAND_SEARCH_TIME_TYPE.FOUR_HOUR;     //寻仙时间
    private FAIRYLAND_SEARCH_WAY_TYPE mSelWay = FAIRYLAND_SEARCH_WAY_TYPE.NORMAL;           //寻仙方式
    private int mCostConfigId;      //消耗配置Id
    private int mCostId;            //消耗物品Id
    private int mCostNum;           //消耗物品数量
    private FairylandOPWindowData mOPData;

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_explore_change_btn(Clone)_0", new DefineFactoryLog<Button_explore_change_btn>());
        EventCenter.Self.RegisterEvent("Button_explore_change_btn(Clone)_1", new DefineFactoryLog<Button_explore_change_btn>());
        EventCenter.Self.RegisterEvent("Button_explore_change_btn(Clone)_2", new DefineFactoryLog<Button_explore_change_btn>());
        EventCenter.Self.RegisterEvent("Button_explore_group_sifting_button", new DefineFactoryLog<Button_explore_group_sifting_button>());
        EventCenter.Self.RegisterEvent("Button_explore_start_button", new DefineFactoryLog<Button_explore_start_button>());
    }

    public override void Open(object param)
    {
        mSelPetId = -1;
        mOPData = param as FairylandOPWindowData;
        mSelWay = FAIRYLAND_SEARCH_WAY_TYPE.NORMAL;

        base.Open(param);

        SetVisible("explore_start_button", true);

        NiceData tmpBtnStartData = GameCommon.GetButtonData(GetSub("explore_start_button"));
        if (tmpBtnStartData != null)
            tmpBtnStartData.set("FAIRYLAND_ID", mOPData.FairylandTid);

        __RefreshItemList();
        __SetSearchTime(FAIRYLAND_SEARCH_TIME_TYPE.FOUR_HOUR);
        __SetSearchWay(FAIRYLAND_SEARCH_WAY_TYPE.NORMAL);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "SELECT_PET":
                {
                    mSelPetId = (int)objVal;
                    NiceData tmpBtnStartData = GameCommon.GetButtonData(GetSub("explore_start_button"));
                    if (tmpBtnStartData != null)
                        tmpBtnStartData.set("PET_ID", mSelPetId);
                    _OnRefresh(mOPData);
                }break;
            case "SELECT_PET_TID":
                {
                    mSelPetTid = (int)objVal;
                    __RefreshItemList();
                }break;
            case "SELECT_TIME":
                {
                    __SetSearchTime((FAIRYLAND_SEARCH_TIME_TYPE)objVal);
                }break;
            case "SELECT_WAY":
                {
                    __SetSearchWay((FAIRYLAND_SEARCH_WAY_TYPE)objVal);
                }break;
        }
    }

    protected override void _OnRefresh(FairylandOPWindowData opData)
    {
        base._OnRefresh(opData);

        GameObject tmpParent = GetSub("explore_start_window");
        tmpParent.gameObject.SetActive(true);
    }

    private void __RefreshItemList()
    {
        DataRecord tmpFairylandConfig = DataCenter.mFairylandConfig.GetRecord(mOPData.FairylandTid);
        string tmpStrItems = tmpFairylandConfig.getData("REWARD");
        List<ItemDataBase> tmpListItems = GameCommon.ParseItemList(tmpStrItems);
        if (mSelPetTid != -1)
            tmpListItems.Insert(0, new ItemDataBase() { tid = mSelPetTid + 10000, itemNum = 1 }); 

        GameObject tmpParent = GetSub("explore_start_window");
        //获得奖励
        _RefreshAwardList(GameCommon.FindObject(tmpParent, "item_group_panel"), tmpListItems);

        //设置寻仙按钮数据
        NiceData tmpBtnStartExplore = GameCommon.GetButtonData(mGameObjUI, "explore_start_button");
        if (tmpBtnStartExplore != null)
            tmpBtnStartExplore.set("AWARD_STRING", tmpStrItems);
    }

    /// <summary>
    /// 设置寻仙时间
    /// </summary>
    /// <param name="timeType"></param>
    private void __SetSearchTime(FAIRYLAND_SEARCH_TIME_TYPE timeType)
    {
        mSelTime = timeType;

        GameObject tmpParent = GetSub("explore_start_window");
        GameObject tmpTimeSelParent = GameCommon.FindObject(tmpParent, "explore_change_group");

        //寻仙时间
        UIToggle tmpToggle = GameCommon.FindComponent<UIToggle>(tmpTimeSelParent, "explore_change_btn(Clone)_" + ((int)timeType).ToString());
        tmpToggle.value = true;

        NiceData tmpBtnData = GameCommon.GetButtonData(tmpParent, "explore_group_sifting_button");
        if (tmpBtnData != null)
            tmpBtnData.set("TIME_TYPE", timeType);

        __RefreshSearchCost();
    }
    /// <summary>
    /// 设置寻仙方式
    /// </summary>
    /// <param name="wayType"></param>
    private void __SetSearchWay(FAIRYLAND_SEARCH_WAY_TYPE wayType)
    {
        mSelWay = wayType;

        GameObject tmpParent = GetSub("explore_start_window");

        //寻仙方式
        GameObject tmpWaySelParent = GameCommon.FindObject(tmpParent, "explore_group_sifting_button");
        string tmpStrWay = GetFairylandSearchWayName(wayType) + "镇妖";
        GameCommon.SetUIText(tmpWaySelParent, "label", tmpStrWay);

        NiceData tmpBtnData = GameCommon.GetButtonData(tmpParent, "explore_group_sifting_button");
        if (tmpBtnData != null)
            tmpBtnData.set("SEL_WAY", mSelWay);

        __RefreshSearchCost();
    }
    /// <summary>
    /// 刷新寻仙消耗
    /// </summary>
    private void __RefreshSearchCost()
    {
        mCostConfigId = MakeFairylandCostId(mSelWay, mSelTime);
        GameObject tmpParent = GetSub("explore_start_window");
        GameObject tmpExpendParent = GameCommon.FindObject(tmpParent, "expend_label");
        DataRecord tmpCostConfig = DataCenter.mFairylandCostConfig.GetRecord(mCostConfigId);

        //文字
        string tmpStrWayCost = GetFairylandSearchWayName(mSelWay) + "消耗";
        tmpExpendParent.GetComponent<UILabel>().text = tmpStrWayCost;

        mCostId = (int)tmpCostConfig.getObject("EXPLOR_ITEM_ID");
        mCostNum = (int)tmpCostConfig.getObject("EXPLOR_ITEM_NUM");

        //图标
        GameCommon.SetItemIcon(tmpExpendParent, new ItemData() { mID = mCostId, mType = (int)PackageManager.GetItemTypeByTableID(mCostId) });

        //个数
        GameCommon.SetUIText(tmpExpendParent, "Label", "x" + mCostNum.ToString());

        NiceData tmpBtnStartData = GameCommon.GetButtonData(GetSub("explore_start_button"));
        if (tmpBtnStartData != null)
        {
            tmpBtnStartData.set("COST_CONFIG_ID", mCostConfigId);
            tmpBtnStartData.set("COST_ID", mCostId);
            tmpBtnStartData.set("COST_NUM", mCostNum);
            tmpBtnStartData.set("TIME_TYPE", mSelTime);
            tmpBtnStartData.set("OP_DATA", mOPData);
        }
    }
    /// <summary>
    /// 检查是否可以开始
    /// </summary>
    private bool __CheckStart()
    {
        DataRecord tmpConfig = DataCenter.mFairylandCostConfig.GetRecord(mCostConfigId);
        if (tmpConfig == null)
        {
            FairylandLog.LogError("找不到花费配置" + mCostConfigId);
            return false;
        }

        bool bCanStart = true;

        //消耗品是否够
        int tmpLeftCount = PackageManager.GetItemLeftCount(mCostId);
        bCanStart &= (tmpLeftCount >= mCostNum);

        /*
        UIImageButton tmpBtn = GameCommon.FindComponent<UIImageButton>(mGameObjUI, "explore_start_button");
        if (tmpBtn != null)
            tmpBtn.isEnabled = bCanStart;
         * */

        return bCanStart;
    }
}
/// <summary>
/// 选择寻仙时间
/// </summary>
class Button_explore_change_btn : CEvent
{
    public override bool _DoEvent()
    {
        GameObject tmpBtnObj = getObject("BUTTON") as GameObject;
        string tmpSub = tmpBtnObj.name.Substring(tmpBtnObj.name.Length - 1);
        int tmpSubIndex = System.Convert.ToInt32(tmpSub);
        DataCenter.SetData("FAIRYLAND_RIGHT_SEARCH_READY_WINDOW", "SELECT_TIME", (FAIRYLAND_SEARCH_TIME_TYPE)tmpSubIndex);

        return true;
    }
}
/// <summary>
/// 选择寻仙方式
/// </summary>
class Button_explore_group_sifting_button : CEvent
{
    public override bool _DoEvent()
    {
        FAIRYLAND_SEARCH_TIME_TYPE tmpTimeType = (FAIRYLAND_SEARCH_TIME_TYPE)getObject("TIME_TYPE");
        FAIRYLAND_SEARCH_WAY_TYPE tmpWayType = (FAIRYLAND_SEARCH_WAY_TYPE)getObject("SEL_WAY");

        DataCenter.OpenWindow("FAIRYLAND_SEARCH_WAY_WINDOW");
        DataCenter.SetData("FAIRYLAND_SEARCH_WAY_WINDOW", "TIME_TYPE", tmpTimeType);
        DataCenter.SetData("FAIRYLAND_SEARCH_WAY_WINDOW", "CLOSE_CALLBACK", (Action<FAIRYLAND_SEARCH_WAY_TYPE>)__OnSelectWayComplete);//new CallBack(this, "__OnSelectComplete", null));
        DataCenter.SetData("FAIRYLAND_SEARCH_WAY_WINDOW", "REFRESH", null);
        DataCenter.SetData("FAIRYLAND_SEARCH_WAY_WINDOW", "WAY_SELECTED", tmpWayType);

        return true;
    }

    private void __OnSelectWayComplete(FAIRYLAND_SEARCH_WAY_TYPE wayType)
    {
        DataCenter.SetData("FAIRYLAND_RIGHT_SEARCH_READY_WINDOW", "SELECT_WAY", wayType);
    }
}
/// <summary>
/// 开始寻仙按钮
/// </summary>
class Button_explore_start_button : CEvent
{
    public override bool _DoEvent()
    {
        int tmpCostId = (int)getObject("COST_ID");
        int tmpCostNum = (int)getObject("COST_NUM");

        //消耗品是否够
        int tmpLeftCount = PackageManager.GetItemLeftCount(tmpCostId);
        if (tmpLeftCount < tmpCostNum)
        {
            //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_FAIRYLAND_ITEM_NO_ENOUGH);
			if(PackageManager.GetItemTypeByTableID(tmpCostId) ==ITEM_TYPE.YUANBAO)
				GameCommon.ToGetDiamond();
			else
				GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.SPIRIT_ITEM);
            return true;
        }

        //检查背包是否已满
        string tmpStrAward = getObject("AWARD_STRING").ToString();
        List<ItemDataBase> tmpItems = GameCommon.ParseItemList(tmpStrAward);
        List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(tmpItems);
        if (!CheckPackage.Instance.CanExploreFairyland(tmpPackageTypes))
            return true;

        //确认寻仙
        // By XiaoWen
        // Begin
        FAIRYLAND_SEARCH_TIME_TYPE tmpTimeType = (FAIRYLAND_SEARCH_TIME_TYPE)getObject("TIME_TYPE");
        Data tmpWayType = DataCenter.GetData("FAIRYLAND_RIGHT_SEARCH_READY_WINDOW", "SELECT_WAY");
        FairylandSearchConfirmData tmpConfirmData = new FairylandSearchConfirmData()
        {
            ItemTid = tmpCostId,
            ItemCount = tmpCostNum,
            CostTime = FairylandRightWindow.GetFairylandSearchTime(tmpTimeType),
            FairylandWay = tmpWayType.mObj == null ? FAIRYLAND_SEARCH_WAY_TYPE.NORMAL : (FAIRYLAND_SEARCH_WAY_TYPE)tmpWayType.mObj
        };
        // End
        DataCenter.OpenWindow("FAIRYLAND_SEARCH_CONFIRM_WINDOW", tmpConfirmData);
		CallBack tmpOkCallback = new CallBack(this, "ConfirmExplore", null);
        DataCenter.SetData("FAIRYLAND_SEARCH_CONFIRM_WINDOW", "OK_CALLBACK", tmpOkCallback);

        return true;
    }

    public void ConfirmExplore(object param)
    {
        int tmpCostId = (int)getObject("COST_ID");
        int tmpCostNum = (int)getObject("COST_NUM");
        int tmpCostConfigId = (int)getObject("COST_CONFIG_ID");
        int tmpFairylandId = (int)getObject("FAIRYLAND_ID");
        int tmpPetId = (int)getObject("PET_ID");
        FairylandOPWindowData tmpOPData = getObject("OP_DATA") as FairylandOPWindowData;
        GlobalModule.DoCoroutine(FairylandNetManager.RequestExploreFairyland(tmpFairylandId, tmpPetId, tmpCostConfigId, tmpCostId, tmpCostNum, tmpOPData));
    }
}

/// <summary>
/// 寻仙记录
/// </summary>
public class FairylandRightSearchRecordWindow : FairylandRightWindow
{
    private FairylandOPWindowData mOPData;

    public override void Init()
    {
        base.Init();
        EventCenter.Self.RegisterEvent("Button_explore_over_get_button", new DefineFactoryLog<Button_explore_over_get_button>());
        EventCenter.Self.RegisterEvent("Button_explore_suppress_button", new DefineFactoryLog<Button_explore_suppress_button>());
    }

    public override void Open(object param)
    {
        mOPData = param as FairylandOPWindowData;

        base.Open(param);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REFRESH_RECORD":
                {
                    __RefreshRecord(objVal);
                }break;
        }
    }

    private void __RefreshRecord(object param)
    {
        SC_Fairyland_GetFairylandEvents resp = param as SC_Fairyland_GetFairylandEvents;

        long tmpNowTime = CommonParam.NowServerTime();

        //奖励列表
        List<ItemDataBase> tmpListItems = new List<ItemDataBase>();
        for (int i = 0, count = resp.events.Length; i < count; i++)
        {
            int tmpIndex = resp.events[i].index;
            string tmpStrIndex = tmpIndex.ToString();
            string tmpStrMark = tmpStrIndex.Substring(0, 1);
            if(tmpStrMark == "3")
                tmpListItems.Add(new ItemDataBase() { tid = resp.events[i].tid, itemNum = resp.events[i].itemNum });
        }
        //当寻仙结束后，在列表最后加入寻仙符灵
        if (mOPData.State == FAIRYLAND_ELEMENT_STATE.TO_HARVEST)
            tmpListItems.Add(new ItemDataBase() { tid = resp.petTid + 10000, itemNum = resp.events[resp.events.Length - 1].itemNum});
        _RefreshAwardList(GameCommon.FindObject(mGameObjUI, "explore_award_list"), tmpListItems);

        //寻仙
        GameObject tmpRewardObj = GetSub("explore_reward_window");
        //是否显示寻仙中
        bool tmpIsShowSearching = mOPData.IsMyselfFairyland ||
            (mOPData.State == FAIRYLAND_ELEMENT_STATE.EXPLORING) ||
            (mOPData.State == FAIRYLAND_ELEMENT_STATE.TO_HARVEST);
        tmpRewardObj.SetActive(tmpIsShowSearching);
        if (tmpIsShowSearching)
        {
            //是否显示领取按钮
            GameObject tmpGetButtonObj = GameCommon.FindObject(tmpRewardObj, "explore_over_get_button");
            tmpGetButtonObj.gameObject.SetActive(mOPData.IsMyselfFairyland);
            if (mOPData.IsMyselfFairyland)
                tmpGetButtonObj.GetComponent<UIImageButton>().isEnabled = (tmpNowTime > resp.endTime);
            __ChangeSearchStateText(resp.endTime);

            NiceData tmpBtnGetData = GameCommon.GetButtonData(tmpGetButtonObj);
            if (tmpBtnGetData != null)
            {
                tmpBtnGetData.set("FAIRYLAND_ID", mOPData.FairylandTid);
                PetLogicData tmpPetLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
                if (tmpPetLogicData != null)
                {
                    PetData tmpTargetPetData = null;
                    foreach (KeyValuePair<int, PetData> tmpPetPair in tmpPetLogicData.mDicPetData)
                    {
                        if (tmpPetPair.Value.tid == resp.petTid)
                        {
                            tmpTargetPetData = tmpPetPair.Value;
                            break;
                        }
                    }
                    if (tmpTargetPetData != null)
                        tmpBtnGetData.set("FAIRYLAND_PET_ITEM_ID", tmpTargetPetData.itemId);
                }
            }
        }

        //暴乱
        GameObject tmpRiotObj = GetSub("explore_riot_window");
        //是否显示暴乱
        bool tmpIsShowRiot = !mOPData.IsMyselfFairyland &&
            (mOPData.State == FAIRYLAND_ELEMENT_STATE.RIOTING);
        tmpRiotObj.SetActive(tmpIsShowRiot);
        if (tmpIsShowRiot)
        {
            NiceData tmpBtnRiot = GameCommon.GetButtonData(tmpRiotObj, "explore_suppress_button");
            if (tmpBtnRiot != null)
            {
                tmpBtnRiot.set("FAIRYLAND_ID", mOPData.FairylandTid);
                tmpBtnRiot.set("OP_DATA", mOPData);
            }
        }
    }

    /// <summary>
    /// 寻仙结束
    /// </summary>
    /// <param name="param"></param>
    private void __OnSearchOver(object param)
    {
        __ChangeSearchStateText((int)param);
    }

    /// <summary>
    /// 改变寻仙状态文字
    /// </summary>
    /// <param name="endTime"></param>
    private void __ChangeSearchStateText(long endTime)
    {
        long tmpNowTime = CommonParam.NowServerTime();
        GameObject tmpRewardObj = GetSub("explore_reward_window");
        GameObject tmpTimeCountParent = GameCommon.FindObject(tmpRewardObj, "explore_tips_label");
        if (tmpNowTime <= endTime)
        {
            tmpTimeCountParent.GetComponent<UILabel>().text = "镇妖中";
            GameCommon.SetUIVisiable(tmpTimeCountParent, "num_label", true);
            CallBack tmpCallback = mOPData.IsMyselfFairyland ? (new CallBack(this, "__OnSearchOver", endTime)) : null;
            SetCountdown(tmpTimeCountParent, "num_label", endTime, tmpCallback);
        }
        else
        {
            tmpTimeCountParent.GetComponent<UILabel>().text = "镇妖完成";
            GameCommon.SetUIVisiable(tmpTimeCountParent, "num_label", false);
        }
    }
}

/// <summary>
/// 领取按钮
/// </summary>
class Button_explore_over_get_button : CEvent
{
    public override bool _DoEvent()
    {
        int tmpFairylandId = (int)getObject("FAIRYLAND_ID");
        object obj = getObject ("FAIRYLAND_PET_ITEM_ID");
		int tmpPetItemId = 0;
		if (obj != null) {
			tmpPetItemId = (int)obj;
		}
        GlobalModule.DoCoroutine(FairylandNetManager.RequestTakeFairylandAwards(tmpFairylandId, tmpPetItemId));

        return true;
    }
}
/// <summary>
/// 镇压按钮
/// </summary>
class Button_explore_suppress_button : CEvent
{
    public override bool _DoEvent()
    {
        //检查镇压次数
        FairylandWindow tmpWin = DataCenter.GetData("FAIRYLAND_WINDOW") as FairylandWindow;
        if (tmpWin != null)
        {
            int tmpRepressLeftCount = (int)tmpWin.getObject("REPRESS_LEFT_COUNT");
            if (tmpRepressLeftCount <= 0)
            {
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_FAIRYLAND_REPRESS_COUNT_NO_MORE);
                return true;
            }
			else {
				DataCenter.ErrorTipsLabelMessage("镇压成功,获得5元宝！");
			}
        }

        int tmpFairylandId = (int)getObject("FAIRYLAND_ID");
        string tmpFriendId = (string)DataCenter.Self.getObject("FAIRYLAND_CURRENT_VISIT_TARGET");
        if (tmpFriendId == CommonParam.mUId)
        {
            //TODO
            FairylandLog.LogError("镇压目标位自己Id - " + tmpFriendId);
            return true;
        }

        FairylandOPWindowData tmpOPData = getObject("OP_DATA") as FairylandOPWindowData;
        GlobalModule.DoCoroutine(FairylandNetManager.RequestRepressRiot(tmpFriendId, tmpFairylandId, tmpOPData));

        return true;
    }
}
