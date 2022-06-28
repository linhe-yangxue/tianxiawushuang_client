using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System;
using System.Collections.Generic;

//寻仙征服仙境结算界面

/// <summary>
/// 寻仙征服仙境结算数据
/// </summary>
public class FairylandConquerResultData
{
    public SC_Fairyland_ConquerFairylandWin WinData { set; get; }
    public Action CloseCallback { set; get; }
}

public class FairylandConquerResultWindow : tWindow
{
    protected FairylandConquerResultData mResultData;

    public override void Init()
    {
        base.Init();

        EventCenter.Register("Button_explore_result_btn_close", new DefineFactoryLog<Button_explore_result_btn_close>());
        EventCenter.Self.RegisterEvent("Button_explore_update_shop_button", new DefineFactory<Button_update_shop_button>());       //战斗失败界面 3种提升战力的选择
        EventCenter.Self.RegisterEvent("Button_explore_update_equip_button", new DefineFactory<Button_update_equip_button>());
        EventCenter.Self.RegisterEvent("Button_explore_update_active_stage_button", new DefineFactory<Button_update_active_stage_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);
        mResultData = param as FairylandConquerResultData;
        NiceData tmpBtnData = GameCommon.GetButtonData(mGameObjUI, "explore_result_btn_close");
        if (tmpBtnData != null)
            tmpBtnData.set("WINDOW_NAME", _WindowName);
        _ShowAnim();
        _OnRefreshUI();
    }
    public override void OnClose()
    {
        if (mResultData != null && mResultData.CloseCallback != null)
            mResultData.CloseCallback();

        base.OnClose();
    }

    protected virtual string _WindowName
    {
        get { return ""; }
    }
    protected virtual string _AnimName
    {
        get { return ""; }
    }

    /// <summary>
    /// 刷新界面
    /// </summary>
    /// <param name="param"></param>
    protected virtual void _OnRefreshUI()
    {
    }

    private void _ShowAnim()
    {
        GameObject uiPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");
        BaseObject model = GameCommon.ShowCharactorModel(uiPoint, 1.6f);

        string animName = _AnimName;
        if (animName != "")
            model.PlayAnim(animName);
    }
}

class Button_explore_result_btn_close : CEvent
{
    public override bool _DoEvent()
    {
        string tmpWinName = getObject("WINDOW_NAME").ToString();
        DataCenter.CloseWindow(tmpWinName);

        return true;
    }
}

/// <summary>
/// 寻仙征服仙境胜利界面
/// </summary>
public class FairylandConquerResultWinWindow : FairylandConquerResultWindow
{
    private float mShowItemDelta = 0.2f;
    UIGridContainer mGridContainer;

    protected override string _WindowName
    {
        get { return "FAIRYLAND_CONQUER_RESULT_WIN_WINDOW"; }
    }
    protected override string _AnimName
    {
        get { return "win"; }
    }

    /// <summary>
    /// 刷新界面
    /// </summary>
    /// <param name="param"></param>
    protected override void _OnRefreshUI()
    {
        mGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "Grid");
        ItemDataBase[] tmpItems = mResultData.WinData.awardItems;
        int tmpCount = tmpItems.Length;
        //合并相同项
        Dictionary<int, int> tmpDic = new Dictionary<int, int>();
        for (int i = 0; i < tmpCount; i++)
        {
            ItemDataBase tmpItem = tmpItems[i];
            if (tmpDic.ContainsKey(tmpItem.tid))
                tmpDic[tmpItem.tid] += tmpItem.itemNum;
            else
                tmpDic[tmpItem.tid] = tmpItem.itemNum;
        }
        mGridContainer.MaxCount = tmpDic.Count;
        int tmpIdx = 0;
        foreach (KeyValuePair<int, int> tmpPair in tmpDic)
        {
            GameObject tmpItem = mGridContainer.controlList[tmpIdx];
            GameCommon.SetOnlyItemIcon(tmpItem, "item_icon", tmpPair.Key);
			int iItemId = tmpPair.Key;
			AddButtonAction (GameCommon.FindObject (tmpItem, "item_icon"), () => GameCommon.SetAccountItemDetailsWindow (iItemId));
            GameCommon.SetUIText(tmpItem, "count_label", "x" + tmpPair.Value);
            tmpIdx += 1;
        }

        this.DoCoroutine(__ShowItems());
    }

    private IEnumerator __ShowItems()
    {
        if(mGridContainer == null)
            yield break;

        foreach (GameObject tmpItem in mGridContainer.controlList)
            tmpItem.SetActive(false);

        foreach (GameObject tmpItem in mGridContainer.controlList)
        {
            yield return new WaitForSeconds(mShowItemDelta);
            tmpItem.SetActive(true);
            mGridContainer.Reposition();
        }
    }
}

/// <summary>
/// 寻仙征服仙境失败界面
/// </summary>
public class FairylandConquerResultLoseWindow : FairylandConquerResultWindow
{
    private UIGridContainer mGridContainer;
    protected override string _WindowName
    {
        get { return "FAIRYLAND_CONQUER_RESULT_LOSE_WINDOW"; }
    }
    protected override string _AnimName
    {
        get { return "lose"; }
    }

    protected override void _OnRefreshUI() 
    {
        mGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "Grid");
        BattleFailManager.Self.InitFailGuideUI(mGridContainer, () => { DataCenter.CloseWindow(_WindowName); });
    }
}
