using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

//排行窗口基类

/// <summary>
/// 排行元素数据
/// </summary>
public interface IRanklistItemData
{
    /// <summary>
    /// 昵称
    /// </summary>
    string Nickname { set; get; }
    /// <summary>
    /// 战斗力
    /// </summary>
    int Power { set; get; }
    /// <summary>
    /// 头像Id
    /// </summary>
    int HeadIconId { set; get; }
    /// <summary>
    /// 排名，0为未上榜，-1为错误状态无排行信息
    /// </summary>
    int Ranking { set; get; }
    /// <summary>
    /// VIP等级，-1为无VIP
    /// </summary>
    int VIPLv { set; get; }

    /// <summary>
    /// 根据是不是自己，获取显示颜色
    /// </summary>
    /// <param name="isMyself"></param>
    /// <returns></returns>
    Color GetShowColor();
}
/// <summary>
/// 排行元素数据默认实现
/// </summary>
public class RankingItemData : IRanklistItemData
{
    public virtual string Nickname { set { } get { return ""; } }
    public virtual int Power { set { } get { return 0; } }
    public virtual int HeadIconId { set { } get { return 0; } }
    public virtual int Ranking { set { } get { return 0; } }
    public virtual int VIPLv { set { } get { return -1; } }

    public virtual Color GetShowColor() { return Color.white; }
}

/// <summary>
/// 刷新子排行界面时传入数据
/// </summary>
public class RanklistSubWinData
{
    public int TabIndex { set; get; }
    public object Data { set; get; }
}

/// <summary>
/// 排行榜主界面基类，可包含1个或多个RanklistSubUI
/// </summary>
public class RanklistMainUI : tWindow
{
    protected string mCloseBtnName = "";
    protected List<string> mTabsBtnName = null;
    protected List<string> mSubWinName = null;
    protected int mCurrTabIdx = -1;             //当前标签索引

    public override void Init()
    {
        base.Init();

        mCloseBtnName = _GetCloseBtnName();
        mTabsBtnName = _GetTabBtnName();
        mSubWinName = _GetSubWindowName();
        EventCenter.Self.RegisterEvent("Button_boss_rank_window", new DefineFactoryLog<Button_Ranklist_Close_1>());
        if (mCloseBtnName != "")
        {
            EventCenter.Self.RegisterEvent("Button_" + mCloseBtnName, new DefineFactoryLog<Button_Ranklist_Close>());
        }
        for (int i = 0, count = mTabsBtnName.Count; i < count; i++)
        {
            if (mTabsBtnName[i] != "")
                EventCenter.Self.RegisterEvent("Button_" + mTabsBtnName[i], new DefineFactoryLog<Button_Ranklist_Tab>());
        }
    }
    public override void Open(object param)
    {
        base.Open(param);

        for (int i = 0, count = mSubWinName.Count; i < count; i++)
            DataCenter.SetData(mSubWinName[i], "MAIN_UI_GAMEOBJECT", mGameObjUI);

        NiceData tmpCloseBtnData = GameCommon.GetButtonData(mGameObjUI, mCloseBtnName);
        if (tmpCloseBtnData != null)
            _SetCloseButtonData(tmpCloseBtnData);

        //设置标签按钮数据
        List<NiceData> tmpTabsBtnData = new List<NiceData>();
        for (int i = 0, count = mTabsBtnName.Count; i < count; i++)
        {
            NiceData tmpData = GameCommon.GetButtonData(mGameObjUI, mTabsBtnName[i]);
            if (tmpData != null)
                tmpTabsBtnData.Add(tmpData);
        }
        _SetTabsButtonData(tmpTabsBtnData);

        int tmpTabIdx = 0;
        if (param != null)
        {
            if (!int.TryParse(param.ToString(), out tmpTabIdx))
                tmpTabIdx = 0;
        }

        __CloseAllSubWindow();
        for (int i = 0, count = mSubWinName.Count; i < count; i++)
            DataCenter.SetData(mSubWinName[i], "NEED_REQUEST_DATA", true);
        if (mSubWinName != null && mSubWinName.Count > 0)
            __OpenSubWindow(tmpTabIdx);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "VISIBLE":
                {
                    bool tmpIsVisible = (bool)objVal;
                    mGameObjUI.SetActive(tmpIsVisible);
                }break;
            case "CHANGE_TO_TAB":
                {
                    int tmpSubIdx = (int)objVal;
                    __CloseAllSubWindow();
                    __OpenSubWindow(tmpSubIdx);
                } break;
            case "REFRESH_TAB":
                {
                    RanklistSubWinData tmpWinData = objVal as RanklistSubWinData;
                    __RefreshTabData(tmpWinData);
                } break;
        }
    }

    public override void OnClose()
    {
        __CloseAllSubWindow();

        base.OnClose();
    }

    /// <summary>
    /// 获取主窗口名称
    /// </summary>
    /// <returns></returns>
    protected virtual string _GetWindowName()
    {
        return "";
    }

    /// <summary>
    /// 获取所有标签名称
    /// </summary>
    /// <returns></returns>
    protected virtual List<string> _GetTabBtnName()
    {
        return null;
    }
    /// <summary>
    /// 获取所有子排行窗口名称
    /// </summary>
    /// <returns></returns>
    protected virtual List<string> _GetSubWindowName()
    {
        return null;
    }
    /// <summary>
    /// 获取子窗口名称
    /// </summary>
    /// <param name="subIdx"></param>
    /// <returns></returns>
    public string GetSubWindowName(int subIdx)
    {
        if (mSubWinName == null)
            return "";
        if (subIdx < 0 || subIdx >= mSubWinName.Count)
            return "";

        return mSubWinName[subIdx];
    }
    /// <summary>
    /// 设置标签按钮数据
    /// </summary>
    /// <param name="listTabsBtnData"></param>
    protected void _SetTabsButtonData(List<NiceData> listTabsBtnData)
    {
        if (listTabsBtnData == null)
            return;

        string tmpWinName = _GetWindowName();
        for (int i = 0, count = listTabsBtnData.Count; i < count; i++)
        {
            NiceData tmpData = listTabsBtnData[i];
            tmpData.set("WINDOW_NAME", tmpWinName);
            tmpData.set("TAB_INDEX", i);
        }
    }
    /// <summary>
    /// 获取关闭按钮名称
    /// </summary>
    /// <returns></returns>
    protected virtual string _GetCloseBtnName()
    {
        return "";
    }
    /// <summary>
    /// 设置关闭按钮数据
    /// </summary>
    /// <param name="closeBtnData"></param>
    protected void _SetCloseButtonData(NiceData closeBtnData)
    {
        if (closeBtnData == null)
            return;

        closeBtnData.set("WINDOW_NAME", _GetWindowName());
    }

    /// <summary>
    /// 打开指定索引的子排行
    /// </summary>
    /// <param name="subIdx"></param>
    private void __OpenSubWindow(int subIdx)
    {
        if (mSubWinName == null)
            return;
        if (subIdx < 0 || subIdx >= mSubWinName.Count)
            return;

        mCurrTabIdx = subIdx;
        string tmpTabName = mTabsBtnName[subIdx];
        UIToggle tmpToggle = GameCommon.FindComponent<UIToggle>(mGameObjUI, tmpTabName);
        if (tmpToggle != null)
            tmpToggle.value = true;
        DataCenter.OpenWindow(mSubWinName[subIdx]);
    }
    /// <summary>
    /// 刷新子排行数据
    /// </summary>
    /// <param name="subIdx"></param>
    /// <param name="data"></param>
    private void __RefreshTabData(RanklistSubWinData data)
    {
        if (data == null || mSubWinName == null)
            return;
        int tmpSubIdx = data.TabIndex;
        if (tmpSubIdx < 0 || tmpSubIdx >= mSubWinName.Count)
            return;

        DataCenter.SetData(mSubWinName[tmpSubIdx], "REFRESH_RANKLIST", data.Data);
    }
    /// <summary>
    /// 关闭所有子排行窗口
    /// </summary>
    private void __CloseAllSubWindow()
    {
        if (mSubWinName != null)
        {
            for (int i = 0, count = mSubWinName.Count; i < count; i++)
                DataCenter.CloseWindow(mSubWinName[i]);
        }
    }
}

/// <summary>
/// 排行主界面标签按钮点击
/// </summary>
public class Button_Ranklist_Tab : CEvent
{
    public override bool _DoEvent()
    {
        string tmpWinName = getObject("WINDOW_NAME").ToString();
        int tmpTabIdx = (int)getObject("TAB_INDEX");

        DataCenter.SetData(tmpWinName, "CHANGE_TO_TAB", tmpTabIdx);

        return true;
    }
}
/// <summary>
/// 排行主界面关闭按钮
/// </summary>
public class Button_Ranklist_Close : CEvent
{
    public override bool _DoEvent()
    {
        string tmpWinName = getObject("WINDOW_NAME").ToString();

        DataCenter.CloseWindow(tmpWinName);

        return true;
    }
}
public class Button_Ranklist_Close_1 : CEvent
{
    public override bool _DoEvent()
    {
        string tmpWinName = "RANKLIST_BOSSBATTLE_WINDOW";

        DataCenter.CloseWindow(tmpWinName);

        return true;
    }
}

public enum RANKLIST_ITEM_COMPONENT_TYPE
{
    ITEM_GRID,          //网格
    NICKNAME,           //昵称
    POWER,              //战斗力
    HEAD_ICON_ID,       //头像
    RANKING,            //排行
    VIP_LEVEL           //VIP等级
}
/// <summary>
/// 排行子界面基类，可单独或包含在RanklistMainUI中
/// </summary>
public class RanklistSubUI : tWindow
{
    protected GameObject mGOMainUI;

    protected string mCloseBtnName = "";

    protected bool mNeedRequestData = true;     //是否需要请求数据
    protected RespMessage mLastRequestData;     //上一次请求的数据

    public override void Init()
    {
        base.Init();

        mCloseBtnName = _GetCloseBtnName();
        EventCenter.Self.RegisterEvent("Button_rammbock_rank_window", new DefineFactoryLog<Button_Ranklist_Sub_Close_1>());
        if (mCloseBtnName != "")
            EventCenter.Self.RegisterEvent("Button_" + mCloseBtnName, new DefineFactoryLog<Button_Ranklist_Sub_Close>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        NiceData tmpCloseBtnData = GameCommon.GetButtonData(mGameObjUI, mCloseBtnName);
        if (tmpCloseBtnData != null)
            _SetCloseButtonData(tmpCloseBtnData);

        if (mNeedRequestData)
        {
            mNeedRequestData = false;
            _OnRequestRanklist();
        }
        else
            Refresh(mLastRequestData);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "MAIN_UI_GAMEOBJECT":
                {
                    mGOMainUI = objVal as GameObject;
                } break;
            case "NEED_REQUEST_DATA":
                {
                    mNeedRequestData = (bool)objVal;
                }break;
            case "VISIBLE":
                {
                    bool tmpIsVisible = (bool)objVal;
                    mGameObjUI.SetActive(tmpIsVisible);
                } break;
            case "REFRESH_RANKLIST":
                {
                    _OnRequestRanklist();
                } break;
        }
    }

    public override bool Refresh(object param)
    {
        mLastRequestData = param as RespMessage;

        //刷新重置时间
        long tmpResetTime = _OnGetResetTime();
        __RefreshResetTime(tmpResetTime);

        //刷新排行榜
        _RefreshRanklist();

        //刷新我的排名
        _OnRefreshMyRanking();

        return true;
    }

    /// <summary>
    /// 获取窗口名称
    /// </summary>
    /// <returns></returns>
    protected virtual string _OnGetWindowName()
    {
        return "";
    }

    /// <summary>
    /// 获取关闭按钮名称
    /// </summary>
    /// <returns></returns>
    protected virtual string _GetCloseBtnName()
    {
        return "";
    }
    /// <summary>
    /// 设置关闭按钮数据
    /// </summary>
    /// <param name="closeBtnData"></param>
    protected void _SetCloseButtonData(NiceData closeBtnData)
    {
        if (closeBtnData == null)
            return;

        closeBtnData.set("WINDOW_NAME", _OnGetWindowName());
    }

    /// <summary>
    /// 请求排行榜
    /// </summary>
    protected virtual void _OnRequestRanklist()
    {
    }

    /// <summary>
    /// 根据类型获取控件名称
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns></returns>
    protected virtual string _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE itemType)
    {
        return "";
    }

    /// <summary>
    /// 获取重置时间显示GameObject
    /// </summary>
    /// <returns></returns>
    protected virtual GameObject _OnGetResetTimeGO()
    {
        return null;
    }
    /// <summary>
    /// 刷新重置时间
    /// </summary>
    /// <param name="resetTime"></param>
    private void __RefreshResetTime(long resetTime)
    {
        GameObject tmpGO = _OnGetResetTimeGO();
        if (tmpGO == null)
            return;

        CountdownUI tmpCount = tmpGO.GetComponent<CountdownUI>();
        if (tmpCount != null)
        {
            tmpCount.mServerOverTime = resetTime;
            tmpCount.enabled = (resetTime > 0);
        }
        else
            SetCountdown(tmpGO.transform.parent.gameObject, tmpGO.name, resetTime, new CallBack(this, "OnResetTimeOver", null));
    }
    /// <summary>
    /// 重置时间结束
    /// </summary>
    /// <param name="param"></param>
    public void OnResetTimeOver(object param)
    {
        _OnRequestRanklist();
    }

    /// <summary>
    /// 刷新排行榜数据
    /// </summary>
    /// <param name="listItems"></param>
    protected void _RefreshRanklist()
    {
        IRanklistItemData[] tmpListItems = _OnGetRanklist();

        UIGridContainer tmpGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE.ITEM_GRID));
        if (tmpGridContainer == null)
            return;

        int count = tmpListItems != null ? tmpListItems.Length : 0;
        tmpGridContainer.MaxCount = count;
        string tmpComNickname = _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE.NICKNAME);
        string tmpComHead = _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE.HEAD_ICON_ID);
        string tmpComVIP = _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE.VIP_LEVEL);
        string tmpComPower = _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE.POWER);
        string tmpComRanking = _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE.RANKING);
        for (int i = 0; i < count; i++)
        {
            GameObject tmpGOItem = tmpGridContainer.controlList[i];
            IRanklistItemData tmpItemData = tmpListItems[i];

            //昵称
            if (tmpComNickname != "")
                GameCommon.SetUIText(tmpGOItem, tmpComNickname, tmpItemData.Nickname);

            //头像
            if (tmpComHead != "")
            {
                GameCommon.SetPalyerIcon(GameCommon.FindComponent<UISprite>(tmpGOItem, tmpComHead), tmpItemData.HeadIconId);
//                 string tmpHeadAtlasName, tmpHeadSpriteName;
//                 GameCommon.GetItemAtlasSpriteName(tmpItemData.HeadIconId, out tmpHeadAtlasName, out tmpHeadSpriteName);
//                 GameCommon.SetIcon(GameCommon.FindComponent<UISprite>(tmpGOItem, tmpComHead), tmpHeadAtlasName, tmpHeadSpriteName);
            }

            //VIP
            if (tmpComVIP != "")
                GameCommon.SetUIVisiable(tmpGOItem, tmpComVIP, tmpItemData.VIPLv > -1);

            //战斗力
            if (tmpComPower != "")
                GameCommon.SetUIText(tmpGOItem, tmpComPower, tmpItemData.Power.ToString());

            //排名
            if(tmpComRanking != "")
                GameCommon.SetUIText(tmpGOItem, tmpComRanking, tmpItemData.Ranking.ToString());

            _OnRefreshRanklistItem(tmpGOItem, tmpItemData);
            //added by xuke
            if (tmpGridContainer.transform.parent.GetComponent<UIScrollView>() != null)
                tmpGridContainer.transform.parent.GetComponent<UIScrollView>().ResetPosition();
            else if (tmpGridContainer.transform.parent.parent.GetComponent<UIScrollView>() != null)
                tmpGridContainer.transform.parent.parent.GetComponent<UIScrollView>().ResetPosition();
            //end
           
        }
    }
    /// <summary>
    /// 刷新单个排行数据
    /// </summary>
    /// <param name="item"></param>
    protected virtual void _OnRefreshRanklistItem(GameObject goItem, IRanklistItemData itemData)
    {
    }

    /// <summary>
    /// 刷新自己的排行
    /// </summary>
    /// <param name="myRanking"></param>
    protected virtual void _OnRefreshMyRanking()
    {
    }

    /// <summary>
    /// 获取重置时间
    /// </summary>
    /// <returns></returns>
    protected virtual long _OnGetResetTime()
    {
        return 0;
    }
    /// <summary>
    /// 获取排行榜列表数据
    /// </summary>
    /// <returns></returns>
    protected virtual IRanklistItemData[] _OnGetRanklist()
    {
        return null;
    }
}

/// <summary>
/// 排行子界面基类，包含在RanklistMainUI中
/// </summary>
public class RanklistSubBuildInUI : RanklistSubUI
{
    public override void Open(object param)
    {
        mGameObjUI = GameCommon.FindObject(mGOMainUI, _GetWindowGameObjectName());

        base.Open(param);
    }

    /// <summary>
    /// 获取子窗口在主窗口中的资源名
    /// </summary>
    /// <returns></returns>
    protected virtual string _GetWindowGameObjectName()
    {
        return "";
    }

    protected virtual void _ResetScrollViewPos(string kScrollViewName) 
    {
        UIScrollView _rankListView = GameCommon.FindComponent<UIScrollView>(mGameObjUI, kScrollViewName);
        if (_rankListView != null)
        {
            GlobalModule.DoLater(() => 
            {
                _rankListView.ResetPosition();
            },0f);
        }
    }
}

/// <summary>
/// 查看目标阵容
/// </summary>
public class Button_Check_Target_TeamInfo : CEvent
{
    public override bool _DoEvent()
    {
        int tmpTargetId = (int)getObject("TARGET_ID");
        //TODO

        return true;
    }
}

/// <summary>
/// 子排行窗口关闭
/// </summary>
public class Button_Ranklist_Sub_Close : CEvent
{
    public override bool _DoEvent()
    {
        string tmpWinName = getObject("WINDOW_NAME").ToString();

        DataCenter.SetData(tmpWinName, "NEED_REQUEST_DATA", true);
        DataCenter.CloseWindow(tmpWinName);

        return true;
    }
}
public class Button_Ranklist_Sub_Close_1 : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RANKLIST_RAMMBOCK_WINDOW");
        return true;
    }
}