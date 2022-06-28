using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;
using System;

/// <summary>
/// 获取仙境状态列表
/// </summary>
public class Fairyland_GetFairylandStates_Requester:
    NetRequester<CS_Fairyland_GetFairylandStates, SC_Fairyland_GetFairylandStates>
{
    private string mOwnerId;           //所有者Id

    public Fairyland_GetFairylandStates_Requester(string ownerId)
    {
        mOwnerId = ownerId;
    }

    protected override CS_Fairyland_GetFairylandStates GetRequest()
    {
        CS_Fairyland_GetFairylandStates req = new CS_Fairyland_GetFairylandStates();
        req.ownerId = mOwnerId;
        return req;
    }

    protected override void OnSuccess()
    {
        //TODO
    }
    protected override void OnFail()
    {
        //TODO
    }
}

/// <summary>
/// 开始征服仙境
/// </summary>
public class Fairyland_ConquerFairylandStart_Requester:
    NetRequester<CS_Fairyland_ConquerFairylandStart, SC_Fairyland_ConquerFairylandStart>
{
    private int mFairylandId;           //仙境Id

    public Fairyland_ConquerFairylandStart_Requester(int fairylandId)
    {
        mFairylandId = fairylandId;
    }

    protected override CS_Fairyland_ConquerFairylandStart GetRequest()
    {
        CS_Fairyland_ConquerFairylandStart req = new CS_Fairyland_ConquerFairylandStart();
        req.fairylandId = mFairylandId;
        return req;
    }

    protected override void OnSuccess()
    {
        //进入战斗

        //设置关卡信息
        DataCenter.CloseWindow("FAIRYLAND_OP_WINDOW");
        GlobalModule.ClearAllWindow();
        DataRecord tmpFairylandConfig = DataCenter.mFairylandConfig.GetRecord(mFairylandId);
        int stageIndex = (int)tmpFairylandConfig.getObject("STATE_ID");
        DataCenter.Set("CURRENT_STAGE", stageIndex);
        MainProcess.LoadBattleLoadingScene();

        //TODO 测试，随机一个战斗结果
//         int isWin = 1;// (Random.Range(1, 2) == 1) ? 1 : 0;
//         if (isWin == 1)
//             GlobalModule.DoCoroutine(FairylandNetManager.RequestConquerFairylandResult());
//         else
//           GlobalModule.DoCoroutine(__DoAction());
    }
    protected override void OnFail()
    {
        //TODO
    }

    private IEnumerator __DoAction()
    {
        MainProcess.ClearBattle();
        MainProcess.LoadRoleSelScene();

        yield return GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates());
    }
}

/// <summary>
/// 结束征服仙境，只有成功了才能请求
/// </summary>
public class Fairyland_ConquerFairylandWin_Requester:
    NetRequester<CS_Fairyland_ConquerFairylandWin, SC_Fairyland_ConquerFairylandWin>
{
    protected override CS_Fairyland_ConquerFairylandWin GetRequest()
    {
        CS_Fairyland_ConquerFairylandWin req = new CS_Fairyland_ConquerFairylandWin();
        return req;
    }

    protected override void OnSuccess()
    {
        //加物品
        for (int i = 0, count = respMsg.awardItems.Length; i < count; i++)
        {
            if (respMsg.awardItems[i].itemId == -1)
                PackageManager.AddItem(respMsg.awardItems[i]);
            else
                respMsg.awardItems[i] = PackageManager.UpdateItem(respMsg.awardItems[i]);
        }

        FairylandConquerResultData tmpResultData = new FairylandConquerResultData()
        {
            WinData = respMsg,
            CloseCallback = () =>
            {
                //结算窗口关闭
                GlobalModule.DoCoroutine(__DoAction());
            }
        };
        DataCenter.OpenWindow("FAIRYLAND_CONQUER_RESULT_WIN_WINDOW", tmpResultData);
    }
    protected override void OnFail()
    {
        switch (respCode)
        {
            case 2053: break;       //战斗时间错误
        }
    }

    private IEnumerator __DoAction()
    {
        MainProcess.ClearBattle();
        MainProcess.LoadRoleSelScene();
        DataCenter.Set("QUIT_BACK_SCENE", QUIT_BACK_SCENE_TYPE.WORLD_MAP);

        yield return GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates());
    }
}

/// <summary>
/// 探索仙境
/// </summary>
public class Fairyland_ExploreFairyland_Requester:
    NetRequester<CS_Fairyland_ExploreFairyland, SC_Fairyland_ExploreFairyland>
{
    private int mFairylandId;           //仙境Id
    private int mPetItemId;             //寻仙符灵Id
    private int mExploreType;           //探索方式

    private int mCostId;            //消耗品Id
    private int mCostNum;           //消耗品数量
    private FairylandOPWindowData mOPData;      //操作数据

    public Fairyland_ExploreFairyland_Requester(int fairylandId, int petItemId, int exploreType, int costId, int costNum, FairylandOPWindowData opData)
    {
        mFairylandId = fairylandId;
        mPetItemId = petItemId;
        mExploreType = exploreType;

        mCostId = costId;
        mCostNum = costNum;
        mOPData = opData;
    }

    protected override CS_Fairyland_ExploreFairyland GetRequest()
    {
        CS_Fairyland_ExploreFairyland req = new CS_Fairyland_ExploreFairyland();
        req.fairylandId = mFairylandId;
        req.petItemId = mPetItemId;
        req.exploreType = mExploreType;
        return req;
    }

    protected override void OnSuccess()
    {
        //减物品
        ITEM_TYPE tmpItemType = PackageManager.GetItemTypeByTableID(mCostId);
        switch (tmpItemType)
        {
            case ITEM_TYPE.YUANBAO: RoleLogicData.Self.AddDiamond(-mCostNum); break;
            case ITEM_TYPE.GOLD: RoleLogicData.Self.AddGold(-mCostNum); break;
            case ITEM_TYPE.POWER: break;
            case ITEM_TYPE.PET_SOUL: break;
            case ITEM_TYPE.SPIRIT: RoleLogicData.Self.AddSpirit(-mCostNum); break;
            case ITEM_TYPE.REPUTATION: RoleLogicData.Self.AddReputation(-mCostNum); break;
            case ITEM_TYPE.PRESTIGE: RoleLogicData.Self.AddPrestige(-mCostNum); break;
            case ITEM_TYPE.BATTLEACHV: RoleLogicData.Self.AddBattleAchv(-mCostNum); break;
            case ITEM_TYPE.UNIONCONTR: RoleLogicData.Self.AddUnionContr(-mCostNum); break;
            case ITEM_TYPE.BEATDEMONCARD: RoleLogicData.Self.AddBeatDemonCard(-mCostNum); break;
            case ITEM_TYPE.CHARACTER_EXP: break;
        }

        mOPData.State = FAIRYLAND_ELEMENT_STATE.EXPLORING;
        mOPData.WindowType = FAIRYLAND_OP_WINDOW_TYPE.SEARCH_RECORD;
        DataCenter.SetData("FAIRYLAND_WINDOW", "REFRESH_FAIRYLAND_ELEMENT", new FairylandRefreshElementData() { FairylandId = mFairylandId, State = FAIRYLAND_ELEMENT_STATE.EXPLORING, EndTime = respMsg.endTime });
        DataCenter.OpenWindow("FAIRYLAND_OP_WINDOW", mOPData);
        GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandEvents(mOPData.FairylandTid));

        // 添加推送
        __AddLocalPush(PUSH_TYPE.FAIRYLAND_FINISH, respMsg.endTime);

        //设置相应符灵寻仙状态
        PetLogicData tmpPetLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        if (tmpPetLogicData != null)
        {
            PetData tmpPetData = tmpPetLogicData.GetPetDataByItemId(mPetItemId);
            if (tmpPetData != null)
            {
                tmpPetData.inFairyland = mFairylandId;
                if (!FairylandWindow.explore_list.Contains(tmpPetData.tid))
                    FairylandWindow.explore_list.Add(tmpPetData.tid);
            }
        }
    }
    protected override void OnFail()
    {
        switch (respCode)
        {
            case 2054:
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_FAIRYLAND_ALREADY_EXPLORING);
                }break;
        }
    }

    /// <summary>
    /// 添加推送
    /// </summary>
    /// <param name="pushiType">推送类型</param>
    /// <returns>是否添加成功</returns>
    private bool __AddLocalPush(PUSH_TYPE pushiType, long endTime)
    {
        return PushMessageManager.Self.AddLocalPush(pushiType, endTime);
    }
}

/// <summary>
/// 获取仙境信息
/// </summary>
public class Fairyland_GetFairylandEvents_Requester:
    NetRequester<CS_Fairyland_GetFairylandEvents, SC_Fairyland_GetFairylandEvents>
{
    private int mFairylandId;           //仙境Id
    private string mTargetId;           //目标用户Id

    public Fairyland_GetFairylandEvents_Requester(int fairylandId, string targetId)
    {
        mFairylandId = fairylandId;
        mTargetId = targetId;
    }

    protected override CS_Fairyland_GetFairylandEvents GetRequest()
    {
        CS_Fairyland_GetFairylandEvents req = new CS_Fairyland_GetFairylandEvents();
        req.fairylandId = mFairylandId;
        req.targetId = mTargetId;
        return req;
    }

    protected override void OnSuccess()
    {
        DataCenter.SetData("FAIRYLAND_LEFT_RECORD_WINDOW", "REFRESH_RECORD", respMsg);
        DataCenter.SetData("FAIRYLAND_RIGHT_SEARCH_RECORD_WINDOW", "REFRESH_RECORD", respMsg);
    }
    protected override void OnFail()
    {
        //TODO
    }
}

/// <summary>
/// 领取探索奖励
/// </summary>
public class Fairyland_TakeFairylandAwards_Requester:
    NetRequester<CS_Fairyland_TakeFairylandAwards, SC_Fairyland_TakeFairylandAwards>
{
    private int mFairylandId;           //仙境Id

    private int mPetItemId;         //寻仙的符灵Id

    public Fairyland_TakeFairylandAwards_Requester(int fairylandId, int petItemId)
    {
        mFairylandId = fairylandId;

        mPetItemId = petItemId;
    }

    protected override CS_Fairyland_TakeFairylandAwards GetRequest()
    {
        CS_Fairyland_TakeFairylandAwards req = new CS_Fairyland_TakeFairylandAwards();
        req.fairylandId = mFairylandId;
        return req;
    }

    protected override void OnSuccess()
    {
        //加物品
        List<ItemDataBase> itemTemp = PackageManager.UpdateItem(respMsg.awardItems);
        GlobalModule.DoLater(() => { DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", itemTemp); }, 0.2f);
        
        DataCenter.CloseWindow("FAIRYLAND_OP_WINDOW");
 
        //改变符灵状态
        PetLogicData tmpPetLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        if (tmpPetLogicData != null)
        {
            PetData tmpPetData = tmpPetLogicData.GetPetDataByItemId(mPetItemId);
            if (tmpPetData != null)
            {
                tmpPetData.inFairyland = 0;
                if (FairylandWindow.explore_list.Contains(tmpPetData.tid))
                    FairylandWindow.explore_list.Remove(tmpPetData.tid);
            }
        }
        DataCenter.SetData("FAIRYLAND_WINDOW", "REFRESH_FAIRYLAND_ELEMENT", new FairylandRefreshElementData { FairylandId = mFairylandId, State = FAIRYLAND_ELEMENT_STATE.NO_EXPLORE, EndTime = -1 });
    }
    protected override void OnFail()
    {
        //TODO
    }
}
/// <summary>
/// 一键领取探索奖励
/// </summary>
public class Fairyland_OneKeyTakeFairylandAwards_Requester :
    NetRequester<CS_Fairyland_TakeFairylandAwards, SC_Fairyland_TakeFairylandAwards>
{
    private int mFairylandId;           //仙境Id

    public Fairyland_OneKeyTakeFairylandAwards_Requester(int fairylandId)
    {
        mFairylandId = fairylandId;
    }

    protected override CS_Fairyland_TakeFairylandAwards GetRequest()
    {
        CS_Fairyland_TakeFairylandAwards req = new CS_Fairyland_TakeFairylandAwards();
        req.fairylandId = mFairylandId;
        return req;
    }

    protected override void OnSuccess()
    {
        //加物品
        for (int i = 0, count = respMsg.awardItems.Length; i < count; i++)
        {
            if (respMsg.awardItems[i].itemId == -1)
                PackageManager.AddItem(respMsg.awardItems[i]);
            else
                PackageManager.UpdateItem(respMsg.awardItems[i]);
        }
    }
    protected override void OnFail()
    {
        //TODO
    }
}

/// <summary>
/// 镇压暴乱
/// </summary>
public class Fairyland_RepressRiot_Requester:
    NetRequester<CS_Fairyland_RepressRiot, SC_Fairyland_RepressRiot>
{
    private string mFriendId;       //好友Id
    private int mFairylandId;       //仙境Id

    private FairylandOPWindowData mOPData;      //操作数据

    public Fairyland_RepressRiot_Requester(string friendId, int fairylandId, FairylandOPWindowData opData)
    {
        mFriendId = friendId;
        mFairylandId = fairylandId;

        mOPData = opData;
    }

    protected override CS_Fairyland_RepressRiot GetRequest()
    {
        CS_Fairyland_RepressRiot req = new CS_Fairyland_RepressRiot();
        req.friendId = mFriendId;
        req.fairylandId = mFairylandId;
        return req;
    }

    protected override void OnSuccess()
    {
        //加奖励
        int awardDiamond = 5;
        RoleLogicData.Self.AddDiamond(awardDiamond);

        //减少镇压次数
        FairylandWindow tmpWin = DataCenter.GetData("FAIRYLAND_WINDOW") as FairylandWindow;
        if (tmpWin != null)
        {
            int tmpRepressLeftCount = (int)tmpWin.getObject("REPRESS_LEFT_COUNT");
            tmpWin.set("REPRESS_LEFT_COUNT", tmpRepressLeftCount - 1);
            DataCenter.SetData("FAIRYLAND_WINDOW", "REFRESH_REPRESS_LEFT_COUNT", null);
        }

        //重新设置类型，之后再次打开窗口
        mOPData.State = FAIRYLAND_ELEMENT_STATE.EXPLORING;
        mOPData.WindowType = FAIRYLAND_OP_WINDOW_TYPE.SEARCH_RECORD;
        DataCenter.SetData("FAIRYLAND_WINDOW", "REFRESH_FAIRYLAND_ELEMENT", new FairylandRefreshElementData { FairylandId = mFairylandId, State = FAIRYLAND_ELEMENT_STATE.EXPLORING, EndTime = mOPData.ExploreEndTime });
        DataCenter.OpenWindow("FAIRYLAND_OP_WINDOW", mOPData);
    }
    protected override void OnFail()
    {
        switch (respCode)
        {
            case 2052:
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_FAIRYLAND_REPRESS_COUNT_NO_MORE);
                }break;
        }
    }
}

/// <summary>
/// 获取好友仙境信息
/// </summary>
public class Fairyland_GetFairylandFriendList_Requester:
    NetRequester<CS_Fairyland_GetFairylandFriendList, SC_Fairyland_GetFairylandFriendList>
{
    protected override CS_Fairyland_GetFairylandFriendList GetRequest()
    {
        CS_Fairyland_GetFairylandFriendList req = new CS_Fairyland_GetFairylandFriendList();
        return req;
    }

    protected override void OnSuccess()
    {
        DataCenter.SetData("FAIRYLAND_FRIEND_SEL_WINDOW", "REFRESH", respMsg);
    }
    protected override void OnFail()
    {
        //TODO
    }
}

public class FairylandNetManager
{
    /// <summary>
    /// 获取仙境状态列表
    /// </summary>
    /// <param name="currVisitTarget">当前要访问的仙境</param>
    /// <param name="isJustRestoreStates">是否仅存储仙境状态</param>
    public static IEnumerator RequestGetFairylandStates(string currVisitTarget, bool isJustRestoreStates = false, Action successCallback = null)
    {
        if (!isJustRestoreStates)
            DataCenter.Self.set("FAIRYLAND_CURRENT_VISIT_TARGET", currVisitTarget);
        string tmpCurrVisitTarget = currVisitTarget == "" ? CommonParam.mUId : currVisitTarget;
        Fairyland_GetFairylandStates_Requester tmpRequester = new Fairyland_GetFairylandStates_Requester(tmpCurrVisitTarget);
        yield return (tmpRequester.Start());
        if (tmpRequester.respCode == 1)
        {
            if (!isJustRestoreStates)
                DataCenter.OpenWindow("FAIRYLAND_WINDOW", tmpRequester.respMsg);
            if (tmpCurrVisitTarget == CommonParam.mUId)
                DataCenter.SetData("FAIRYLAND_WINDOW", "RESTORE_STATES", tmpRequester.respMsg);
            if (successCallback != null)
                successCallback();
        }

//        EventCenter.Start("Button_trial_window_back_btn").DoEvent();
    }
    /// <summary>
    /// 获取当前仙境状态列表
    /// </summary>
    public static IEnumerator RequestGetFairylandStates()
    {
        string tmpCurrVisitTarget = (string)DataCenter.Self.getObject("FAIRYLAND_CURRENT_VISIT_TARGET");
        yield return GlobalModule.DoCoroutine(RequestGetFairylandStates(tmpCurrVisitTarget));
    }

    /// <summary>
    /// 开始征服仙境
    /// </summary>
    /// <param name="fairylandId"></param>
    /// <returns></returns>
    public static IEnumerator RequestConquerFairylandStart(int fairylandId)
    {
        yield return (new Fairyland_ConquerFairylandStart_Requester(fairylandId)).Start();
    }

    /// <summary>
    /// 结束征服仙境
    /// </summary>
    /// <returns></returns>
    public static IEnumerator RequestConquerFairylandWin()
    {
        yield return (new Fairyland_ConquerFairylandWin_Requester()).Start();
    }

    /// <summary>
    /// 探索仙境
    /// </summary>
    /// <param name="fairylandId"></param>
    /// <param name="petItemId"></param>
    /// <returns></returns>
    public static IEnumerator RequestExploreFairyland(int fairylandId, int petItemId, int exploreType, int costId, int costNum, FairylandOPWindowData opData)
    {
        yield return (new Fairyland_ExploreFairyland_Requester(fairylandId, petItemId, exploreType, costId, costNum, opData)).Start();
    }

    /// <summary>
    /// 获取仙境信息
    /// </summary>
    /// <param name="fairylandId"></param>
    /// <returns></returns>
    public static IEnumerator RequestGetFairylandEvents(int fairylandId)
    {
        string tmpCurrVisit = (string)DataCenter.Self.getObject("FAIRYLAND_CURRENT_VISIT_TARGET");
        tmpCurrVisit = (tmpCurrVisit == "") ? CommonParam.mUId : tmpCurrVisit;
        yield return (new Fairyland_GetFairylandEvents_Requester(fairylandId, tmpCurrVisit)).Start();
    }

    /// <summary>
    /// 领取探索奖励
    /// </summary>
    /// <param name="fairylandId"></param>
    /// <returns></returns>
    public static IEnumerator RequestTakeFairylandAwards(int fairylandId, int petItemId)
    {
        yield return (new Fairyland_TakeFairylandAwards_Requester(fairylandId, petItemId)).Start();
    }

    /// <summary>
    /// 一键领取探索奖励
    /// </summary>
    /// <param name="fairylandId"></param>
    /// <returns></returns>
    public static IEnumerator RequestOneKeyTakeFairylandAwards(int fairylandId)
    {
        yield return (new Fairyland_OneKeyTakeFairylandAwards_Requester(fairylandId)).Start();
    }

    /// <summary>
    /// 镇压暴乱
    /// </summary>
    /// <param name="friendId"></param>
    /// <param name="fairylandId"></param>
    /// <returns></returns>
    public static IEnumerator RequestRepressRiot(string friendId, int fairylandId, FairylandOPWindowData opData)
    {
        yield return (new Fairyland_RepressRiot_Requester(friendId, fairylandId, opData)).Start();
    }

    /// <summary>
    /// 获取好友仙境信息
    /// </summary>
    /// <returns></returns>
    public static IEnumerator RequestGetFairylandFriendList()
    {
        yield return (new Fairyland_GetFairylandFriendList_Requester()).Start();
    }
}
