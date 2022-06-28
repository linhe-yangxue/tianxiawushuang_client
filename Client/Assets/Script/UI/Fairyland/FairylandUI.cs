using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

//符灵探险主界面

//DataCenter.Self.getObject("FAIRYLAND_CURRENT_VISIT_TARGET");通过FAIRYLAND_CURRENT_VISIT_TARGET来判断当前访问目标的界面，-1为自己，否则为好友

enum FAIRYLAND_WINDOW_BTN_TYPE
{
    RETURN_HOME,
    RECEIVE_ONE_CLICK,
    VISIT_FRIENDS,
	FRIENDS_NAME_INFO
}

public enum FAIRYLAND_ELEMENT_STATE
{
    NONE = -1,
    NO_CONQUER,         //未征服
    NO_EXPLORE,         //未探索
    EXPLORING,          //探索中
    RIOTING,            //暴乱中，暴乱肯定出现在探索中
    TO_HARVEST          //等待收获
}

/// <summary>
/// 符灵探险log
/// </summary>
public class FairylandLog
{
    public static void Log(string log)
    {
        DEBUG.Log("Fairyland - " + log);
    }
    public static void LogError(string log)
    {
        DEBUG.LogError("Fairyland - " + log);
    }
    public static void LogWarn(string log)
    {
        DEBUG.LogWarning("Fairyland - " + log);
    }
}

/// <summary>
/// 用于FairylandWindow的onChange中REFRESH_FAIRYLAND_ELEMENT消息
/// </summary>
public class FairylandRefreshElementData
{
    public int FairylandId { set; get; }                    //仙境Id
    public FAIRYLAND_ELEMENT_STATE State { set; get; }      //仙境状态
    public long EndTime { set; get; }                       //结束时间

    public FairylandRefreshElementData()
    {
        FairylandId = -1;
        State = FAIRYLAND_ELEMENT_STATE.NONE;
        EndTime = -1;
    }
}

public class FairylandWindow : tWindow
{
    // 正在寻仙符灵列表
    public static List<int> explore_list = new List<int>();

    private static string[] smButtonsName = new string[] {
        "explore_return_button",
        "explore_get_button",
        "explore_see_button",
		"friend_name_infos"
    };
    private static string[][] smWindowsName = new string[][] {
        new string[] { "FAIRYLAND_LEFT_INFO_WINDOW", "FAIRYLAND_RIGHT_READY_WINDOW" },
        new string[] { "FAIRYLAND_LEFT_PET_SEL_WINDOW", "FAIRYLAND_RIGHT_AWARD_WINDOW" },
        new string[] { "FAIRYLAND_LEFT_PET_SEL_WINDOW", "FAIRYLAND_RIGHT_SEARCH_READY_WINDOW" },
        new string[] { "FAIRYLAND_LEFT_RECORD_WINDOW", "FAIRYLAND_RIGHT_SEARCH_RECORD_WINDOW" },
        new string[] { "FAIRYLAND_LEFT_RECORD_WINDOW", "FAIRYLAND_RIGHT_SEARCH_RECORD_WINDOW" }
    };

    private int mRepressLeftCount = 30;

    private SC_Fairyland_GetFairylandStates mMyFairylandStates;     //最近一次玩家自己的仙境状态

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_explore_window_back_btn", new DefineFactoryLog<Button_explore_window_back_btn>());
        EventCenter.Self.RegisterEvent("Button_explore_return_button", new DefineFactoryLog<Button_explore_return_button>());
        EventCenter.Self.RegisterEvent("Button_explore_get_button", new DefineFactoryLog<Button_explore_get_button>());
        EventCenter.Self.RegisterEvent("Button_explore_see_button", new DefineFactoryLog<Button_explore_see_button>());
        EventCenter.Self.RegisterEvent("Button_explore_main_common_btn", new DefineFactoryLog<Button_explore_main_common_btn>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        //关闭子窗口
        bool tmpIsPlayCloseTween = GameCommon.IsPlayCloseTween;
        GameCommon.IsPlayCloseTween = false;
        DataCenter.CloseWindow("FAIRYLAND_OP_WINDOW");
        GameCommon.IsPlayCloseTween = tmpIsPlayCloseTween;

        DataCenter.OpenWindow("FAIRYLAND_BACK_WINDOW");

        Refresh(param);
    }

    public override void OnClose()
    {
        //关闭子窗口
        DataCenter.CloseWindow("FAIRYLAND_OP_WINDOW");

        DataCenter.CloseWindow("FAIRYLAND_BACK_WINDOW");

        base.OnClose();

        //刷新快捷入口界面
        DataCenter.SetData("TRIAL_EASY_JUMP_WINDOW", "REFRESH", null);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REFRESH_FAIRYLAND_ELEMENT":
                {
                    FairylandRefreshElementData tmpData = objVal as FairylandRefreshElementData;
                    if (mMyFairylandStates != null)
                    {
                        //更新玩家仙境的最近状态
                        mMyFairylandStates.fairylandState[tmpData.FairylandId - 1] = (int)tmpData.State;
                    }
                    __RefreshFairylandElement(tmpData.FairylandId, (int)tmpData.State, tmpData.EndTime);
                } break;
            case "REFRESH_REPRESS_LEFT_COUNT":
                {
                    __RefreshRepressLeftCount();
                } break;
            case "RESTORE_STATES":
                {
                    mMyFairylandStates = objVal as SC_Fairyland_GetFairylandStates;
                } break;
        }
    }

    public override bool Refresh(object param)
    {
        string tmpCurrVisitTarget = (string)DataCenter.Self.getObject("FAIRYLAND_CURRENT_VISIT_TARGET");
        if (tmpCurrVisitTarget == "")
            __RefreshMyself();
        else
            __RefreshFriend();
        SC_Fairyland_GetFairylandStates tmpResp = param as SC_Fairyland_GetFairylandStates;
        __RefreshFairyland(tmpResp);
		DataRecord r = DataCenter.mVipListConfig.GetRecord (RoleLogicData.Self.vipLevel);
		int allPressCnt = 0;
		if(r != null)
			 allPressCnt =r["EVENT_NUM"];
		int num = allPressCnt-tmpResp.repressCnt;
		if (num > 0) {
			set ("REPRESS_LEFT_COUNT", num);
		}
		else set ("REPRESS_LEFT_COUNT", 0);
        __RefreshRepressLeftCount();

        NiceData tmpBtnOneKeyGet = GameCommon.GetButtonData(mGameObjUI, "explore_get_button");
        if (tmpBtnOneKeyGet != null)
            tmpBtnOneKeyGet.set("FAIRYLAND_ALL_ID", tmpResp.fairylandState);

        return true;
    }

    private void __RefreshMyself()
    {
        //隐藏显示按钮
        __ButtonOPVisible(FAIRYLAND_WINDOW_BTN_TYPE.RETURN_HOME, false);
		__ButtonOPVisible(FAIRYLAND_WINDOW_BTN_TYPE.FRIENDS_NAME_INFO, false);
//        __ButtonOPVisible(FAIRYLAND_WINDOW_BTN_TYPE.RECEIVE_ONE_CLICK, true);
    }
    private void __RefreshFriend()
    {
        //隐藏显示按钮
        __ButtonOPVisible(FAIRYLAND_WINDOW_BTN_TYPE.RETURN_HOME, true);
		__ButtonOPVisible(FAIRYLAND_WINDOW_BTN_TYPE.FRIENDS_NAME_INFO, true);
		__RefreshVistName();
//        __ButtonOPVisible(FAIRYLAND_WINDOW_BTN_TYPE.RECEIVE_ONE_CLICK, false);
    }
    private void __RefreshFairyland(SC_Fairyland_GetFairylandStates resp)
    {
        if (resp == null)
            return;

        //TODO 暂时代码指定个数
        UIGridContainer tmpGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "castle_group");
        if (tmpGridContainer == null)
        {
            FairylandLog.LogError("找不到castle_group");
            return;
        }
        int count = resp.fairylandState.Length;
        tmpGridContainer.MaxCount = count;
        for (int i = 0; i < count; i++)
            __RefreshFairylandElement(tmpGridContainer.controlList[i], i+1, resp.fairylandState[i], resp.endTime[i]);
    }
    private void __RefreshFairylandElement(GameObject elem, int flTid, int state, long endTime)
    {
        FAIRYLAND_ELEMENT_STATE tmpState = (FAIRYLAND_ELEMENT_STATE)state;

        DataRecord tmpFLConfig = DataCenter.mFairylandConfig.GetRecord(flTid);
        long tmpNowTime = CommonParam.NowServerTime();
        string tmpCurrVisitTarget = (string)DataCenter.Self.getObject("FAIRYLAND_CURRENT_VISIT_TARGET");
        bool isMyself = (tmpCurrVisitTarget == "");

        //描述
        GameCommon.SetUIText(GameCommon.FindObject(elem, "title_sprite"), "Label", tmpFLConfig.getData("NAME"));
		GameCommon.SetUIText(elem.transform.Find ("explore_main_common_btn/lock/title_sprite").gameObject, "Label", tmpFLConfig.getData("NAME"));

        GameObject tmpCommonBtn = GameCommon.FindObject(elem, "explore_main_common_btn");

        //设置按钮图片
        //TODO 图片资源未指定，暂不设置设置按钮图片
//        GameCommon.SetIcon(tmpCommonBtn.GetComponent<UISprite>(), tmpFLConfig.getData("ATLAS_NAME"), tmpFLConfig.getData("SPRITE_NAME"));

        //等级限制
        int tmpLevelLimit = (int)tmpFLConfig.getObject("LEVEL_LIMIT");
        int tmpTargetLevel = isMyself ? RoleLogicData.Self.character.level : ((int)DataCenter.Self.getObject("FAIRYLAND_CURRENT_VISIT_TARGET_LEVEL"));
        bool bCanEnter = tmpTargetLevel >= tmpLevelLimit;
        GameCommon.SetUIVisiable(tmpCommonBtn, "lock", !bCanEnter);
        if (!bCanEnter)
            GameCommon.SetUIText(GameCommon.FindObject(tmpCommonBtn, "lock"), "Label", tmpLevelLimit.ToString() + "级开放");

        //正常状态按钮是否可以点击
        tmpCommonBtn.GetComponent<UIButtonEvent>().enabled = bCanEnter;

        //设置当前状态
		//NO_CONQUER,         //未征服
		GameCommon.SetUIVisiable(tmpCommonBtn, "can_battle", bCanEnter && tmpState == FAIRYLAND_ELEMENT_STATE.NO_CONQUER);
		//探索中
        //未探索
        GameCommon.SetUIVisiable(tmpCommonBtn, "explore_main_add_button", bCanEnter && tmpState == FAIRYLAND_ELEMENT_STATE.NO_EXPLORE);
        //探索中
        GameObject tmpExploreProcess = GameCommon.FindObject(tmpCommonBtn, "explore_process");
        tmpExploreProcess.gameObject.SetActive(bCanEnter && (tmpState == FAIRYLAND_ELEMENT_STATE.EXPLORING || tmpState == FAIRYLAND_ELEMENT_STATE.RIOTING));
        //探索标志
        tmpExploreProcess.GetComponent<UISprite>().enabled = (bCanEnter && tmpState == FAIRYLAND_ELEMENT_STATE.EXPLORING);
        GameCommon.SetUIVisiable(tmpExploreProcess, "Label", bCanEnter && tmpState == FAIRYLAND_ELEMENT_STATE.EXPLORING);
        //暴乱中
        GameCommon.SetUIVisiable(elem, "riot_btn", bCanEnter && tmpState == FAIRYLAND_ELEMENT_STATE.RIOTING);
        //等待收获
        GameCommon.SetUIVisiable(tmpCommonBtn, "explore_over", bCanEnter && tmpState == FAIRYLAND_ELEMENT_STATE.TO_HARVEST);

        //设置剩余寻仙时间，为-2时不改变时间
        if (endTime != -2)
        {
            GameCommon.SetUIVisiable(tmpExploreProcess, "time", bCanEnter && (tmpState == FAIRYLAND_ELEMENT_STATE.EXPLORING || tmpState == FAIRYLAND_ELEMENT_STATE.RIOTING));
            if (bCanEnter   &&
                (tmpState == FAIRYLAND_ELEMENT_STATE.EXPLORING  ||
                tmpState == FAIRYLAND_ELEMENT_STATE.RIOTING)    &&
                tmpNowTime <= endTime)
                SetCountdown(tmpExploreProcess, "time", endTime, new CallBack(this, "__OnSearchOver", flTid));//elem));
            else
            {
                //停止时间
                CountdownUI tmpCountdown = GameCommon.FindComponent<CountdownUI>(tmpExploreProcess, "time");
                if(tmpCountdown != null)
                    tmpCountdown.enabled = false;
            }
        }

        //设置按钮数据
        NiceData tmpBtnData = GameCommon.GetButtonData(tmpCommonBtn);
        if (tmpBtnData != null)
        {
            tmpBtnData.set("FAIRYLAND_TID", flTid);
            tmpBtnData.set("FAIRYLAND_STATE", state);
            tmpBtnData.set("FAIRYLAND_ENDTIME", endTime);
        }
    }
    private void __RefreshFairylandElement(int flTid, int state, long endTime)
    {
        UIGridContainer tmpGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "castle_group");
        if (tmpGridContainer == null)
        {
            FairylandLog.LogError("找不到castle_group");
            return;
        }
        __RefreshFairylandElement(tmpGridContainer.controlList[flTid - 1], flTid, state, endTime);
    }

    /// <summary>
    /// 刷新剩余镇压次数
    /// </summary>
    private void __RefreshRepressLeftCount()
    {
        int tmpLeftCount = (int)getObject("REPRESS_LEFT_COUNT");
        GameCommon.SetUIText(mGameObjUI, "explore_left_repress_count", "剩余镇压次数：" + tmpLeftCount.ToString());
    }

    /// <summary>
    /// 寻仙结束
    /// </summary>
    private void __OnSearchOver(object param)
    {
        /*
        GameObject tmpElem = param as GameObject;

        //状态改为寻仙完成
        GameCommon.SetUIVisiable(tmpElem, "riot_btn", false);

        GameObject tmpCommonBtn = GameCommon.FindObject(tmpElem, "explore_main_common_btn");
        GameCommon.SetUIVisiable(tmpCommonBtn, "lock", false);
        GameCommon.SetUIVisiable(tmpCommonBtn, "explore_over", true);
        GameCommon.SetUIVisiable(tmpCommonBtn, "explore_process", false);
        GameCommon.SetUIVisiable(tmpCommonBtn, "explore_main_add_button", false);
         * */

        int fairylandId = (int)param;
        DataCenter.SetData("FAIRYLAND_WINDOW", "REFRESH_FAIRYLAND_ELEMENT", new FairylandRefreshElementData { FairylandId = fairylandId, State = FAIRYLAND_ELEMENT_STATE.TO_HARVEST, EndTime = -1 });
    }

    private void __ButtonOPVisible(FAIRYLAND_WINDOW_BTN_TYPE btnType, bool bVisible)
    {
        int index = (int)btnType;
        if (index < 0 || index >= smButtonsName.Length)
            return;
        SetVisible(smButtonsName[index], bVisible);
    }

    /// <summary>
    /// 根据仙境状态和是否为自己的仙境，返回打开的操作界面类型
    /// </summary>
    /// <param name="state"></param>
    /// <param name="isMyself"></param>
    /// <returns></returns>
    public static FAIRYLAND_OP_WINDOW_TYPE ConvertStateToWindowType(FAIRYLAND_ELEMENT_STATE state, bool isMyself)
    {
        FAIRYLAND_OP_WINDOW_TYPE tmpType = FAIRYLAND_OP_WINDOW_TYPE.NONE;
        switch (state)
        {
            case FAIRYLAND_ELEMENT_STATE.NO_CONQUER: tmpType = isMyself ? FAIRYLAND_OP_WINDOW_TYPE.CONQUER_READY : tmpType; break;
            case FAIRYLAND_ELEMENT_STATE.NO_EXPLORE: tmpType = isMyself ? FAIRYLAND_OP_WINDOW_TYPE.SEARCH_SELECT : tmpType; break;
            case FAIRYLAND_ELEMENT_STATE.EXPLORING: tmpType = FAIRYLAND_OP_WINDOW_TYPE.SEARCH_RECORD; break;
            case FAIRYLAND_ELEMENT_STATE.RIOTING: tmpType = FAIRYLAND_OP_WINDOW_TYPE.REPRESS_RIOT; break;
            case FAIRYLAND_ELEMENT_STATE.TO_HARVEST: tmpType = FAIRYLAND_OP_WINDOW_TYPE.SEARCH_RECORD; break;
        }
        return tmpType;
    }
    public static void GetOPWindowNameByWindowType(FAIRYLAND_OP_WINDOW_TYPE winType, out string leftWinName, out string rightWinName)
    {
        string[] tmpWinName = FairylandWindow.smWindowsName[(int)winType];
        leftWinName = tmpWinName[0];
        rightWinName = tmpWinName[1];
    }

    /// <summary>
    /// 是否可以获取到玩家的状态
    /// </summary>
    /// <returns></returns>
    public static bool CanGetMyStates()
    {
        FairylandWindow tmpWin = DataCenter.GetData("FAIRYLAND_WINDOW") as FairylandWindow;
        if (tmpWin == null)
            return false;
        SC_Fairyland_GetFairylandStates tmpStates = tmpWin.getObject("RESTORE_STATES") as SC_Fairyland_GetFairylandStates;
        return (tmpStates != null);
    }
    /// <summary>
    /// 玩家是否有指定的状态
    /// </summary>
    /// <param name="state">目标仙境状态</param>
    /// <returns></returns>
    public static bool HasMySpecifiedState(FAIRYLAND_ELEMENT_STATE state)
    {
        FairylandWindow tmpWin = DataCenter.GetData("FAIRYLAND_WINDOW") as FairylandWindow;
        if (tmpWin == null)
            return false;
        SC_Fairyland_GetFairylandStates tmpStates = tmpWin.getObject("RESTORE_STATES") as SC_Fairyland_GetFairylandStates;
        for (int i = 0, count = tmpStates.fairylandState.Length; i < count; i++)
        {
            if (tmpStates.fairylandState[i] == (int)state)
                return true;
        }
        return false;
    }

	/// <summary>
	/// visit friend name
	/// </summary>
	private void __RefreshVistName()
	{
		string tmpStrVisitName = (string)DataCenter.Self.getObject("VISIT_FRIEND_NAME");
		UILabel visitName = GameCommon.FindObject (mGameObjUI, "friend_name_label").GetComponent<UILabel>();
		visitName.text = tmpStrVisitName;
		int tmpIntVisitTid = (int)DataCenter.Self.getObject ("VISIT_FRIEND_TID");
		visitName.color = GameCommon.GetNameColor (tmpIntVisitTid);
//		GameCommon.SetUIText(mGameObjUI, "friend_name_label", tmpLeftCount.ToString());
	}
}

/// <summary>
/// 返回自己主界面按钮
/// </summary>
class Button_explore_return_button : CEvent
{
    public override bool _DoEvent()
    {
        GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates(""));

        return true;
    }
}

/// <summary>
/// 一键领取
/// </summary>
class Button_explore_get_button : CEvent
{
    public override bool _DoEvent()
    {
        int[] tmpFairylandStates = getObject("FAIRYLAND_ALL_ID") as int[];
        //依次领取
        GlobalModule.DoCoroutine(__GetAwards(tmpFairylandStates));

        return true;
    }

    private IEnumerator __GetAwards(int[] fairylandStates)
    {
        for (int i = 0, count = fairylandStates.Length; i < count; i++)
            yield return GlobalModule.DoCoroutine(FairylandNetManager.RequestOneKeyTakeFairylandAwards(i + 1));

        yield return GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates());
    }
}

/// <summary>
/// 拜访好友
/// </summary>
class Button_explore_see_button : CEvent
{
    public override bool _DoEvent()
    {
        // By XiaoWen
        // Bug #13543【寻仙】同时点击仙境+号和拜访好友按钮时会造成图中问题
        // Begin     
        Data dataFairy = DataCenter.GetData("FAIRYLAND_OP_WINDOW", "isOpen");
        bool isOpenFairy = dataFairy.Empty() ? false : (bool)dataFairy;
        if (!isOpenFairy)
        {
            DataCenter.SetData("FAIRYLAND_FRIEND_SEL_WINDOW", "isOpen", true);
            DataCenter.OpenWindow("FAIRYLAND_FRIEND_SEL_WINDOW");
        }
        // End

        return true;
    }
}

/// <summary>
/// 仙境按钮
/// </summary>
class Button_explore_main_common_btn : CEvent
{
    public override bool _DoEvent()
    {
        string tmpCurrVisitTarget = (string)DataCenter.Self.getObject("FAIRYLAND_CURRENT_VISIT_TARGET");
        FAIRYLAND_ELEMENT_STATE tmpElemState = (FAIRYLAND_ELEMENT_STATE)getObject("FAIRYLAND_STATE");
        bool isMyself = (tmpCurrVisitTarget == "");

        if(!isMyself    &&
            tmpElemState != FAIRYLAND_ELEMENT_STATE.EXPLORING    &&
            tmpElemState != FAIRYLAND_ELEMENT_STATE.RIOTING &&
            tmpElemState != FAIRYLAND_ELEMENT_STATE.TO_HARVEST)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_FAIRYLAND_CANNOT_ENTER_FRIEND);
            return true;
        }

        FairylandOPWindowData tmpWinData = new FairylandOPWindowData();
        tmpWinData.FairylandTid = (int)getObject("FAIRYLAND_TID");
        tmpWinData.State = (FAIRYLAND_ELEMENT_STATE)getObject("FAIRYLAND_STATE");
        tmpWinData.WindowType = FairylandWindow.ConvertStateToWindowType(tmpWinData.State, isMyself);
        tmpWinData.IsMyselfFairyland = isMyself;
        tmpWinData.ExploreEndTime = (long)getObject("FAIRYLAND_ENDTIME");

        // By XiaoWen
        // Bug #13543【寻仙】同时点击仙境+号和拜访好友按钮时会造成图中问题
        // Begin
        Data dataFriend = DataCenter.GetData("FAIRYLAND_FRIEND_SEL_WINDOW", "isOpen");
        bool isOpenFriend = dataFriend.Empty() ? false : (bool)dataFriend;
        if (!isOpenFriend)
        {
            DataCenter.SetData("FAIRYLAND_OP_WINDOW", "isOpen", true);
            DataCenter.OpenWindow("FAIRYLAND_OP_WINDOW", tmpWinData);
        }
        // End

        return true;
    }
}

/// <summary>
/// 符灵探险返回按钮
/// </summary>
class Button_explore_window_back_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("FAIRYLAND_WINDOW");
        DataCenter.OpenWindow("TRIAL_WINDOW");
        DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
        return true;
    }
}
