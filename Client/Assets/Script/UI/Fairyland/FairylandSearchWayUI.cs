using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System;

//符灵探险寻仙方式确认界面

public class FairylandSearchWayWindow : tWindow
{
    private FAIRYLAND_SEARCH_TIME_TYPE mTimeType;           //花费配置Id
    private Action<FAIRYLAND_SEARCH_WAY_TYPE> mCallback;    //确认后回调

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_explore_select_way_bg", new DefineFactoryLog<Button_explore_select_way_bg>());
        EventCenter.Self.RegisterEvent("Button_explore_way_close_button", new DefineFactoryLog<Button_explore_way_close_button>());
        EventCenter.Self.RegisterEvent("Button_explore_way_affirm_button", new DefineFactoryLog<Button_explore_way_affirm_button>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "TIME_TYPE": mTimeType = (FAIRYLAND_SEARCH_TIME_TYPE)objVal; break;
            case "CLOSE_CALLBACK": mCallback = (Action<FAIRYLAND_SEARCH_WAY_TYPE>)objVal; break;
            case "WAY_SELECTED":
                {
                    FAIRYLAND_SEARCH_WAY_TYPE tmpWayType = (FAIRYLAND_SEARCH_WAY_TYPE)objVal;
                    __OnSelected(tmpWayType);
                }break;
        }
    }

    public override bool Refresh(object param)
    {
        GameObject tmpWayGroup = GetSub("way_group");
        for (int i = 0; i < 3; i++)
        {
            GameObject tmpObj = GameCommon.FindObject(tmpWayGroup, "way_info(Clone)_" + i.ToString());
            __RefreshElement(tmpObj, (FAIRYLAND_SEARCH_WAY_TYPE)i);
        }

        return true;
    }

    private void __RefreshElement(GameObject elem, FAIRYLAND_SEARCH_WAY_TYPE wayType)
    {
        int tmpCostId = FairylandRightWindow.MakeFairylandCostId(wayType, mTimeType);
        DataRecord tmpConfig = DataCenter.mFairylandCostConfig.GetRecord(tmpCostId);
        if (tmpConfig == null)
        {
            FairylandLog.LogError("找不到消耗配置" + tmpCostId.ToString());
            return;
        }

        //标题
        string tmpTitle = FairylandRightWindow.GetFairylandSearchWayName(wayType) + "镇妖";
        GameCommon.SetUIText(elem, "Label", tmpTitle);

        //描述
        GameCommon.SetUIText(elem, "info_label", tmpConfig.getData("DESC"));

        //查看VIP等级
        int tmpVIPLimit = (int)tmpConfig.getObject("VIP_LEVEL");
        GameObject tmpSelectBG = GameCommon.FindObject(elem, "explore_select_way_bg");
        bool tmpReachVIPStandard = VIPHelper.IsReachStandard(tmpVIPLimit);
        tmpSelectBG.GetComponent<UIButtonEvent>().enabled = (tmpReachVIPStandard);
        tmpSelectBG.GetComponent<UIToggle>().enabled = (tmpReachVIPStandard);
        tmpSelectBG.GetComponent<BoxCollider>().enabled = (tmpReachVIPStandard);
        tmpSelectBG.GetComponent<UIButton>().isEnabled = (tmpReachVIPStandard);
        GameCommon.SetUIVisiable(elem, "vip_info_label", !tmpReachVIPStandard);
        if (!tmpReachVIPStandard)
            GameCommon.SetUIText(elem, "vip_info_label", "VIP" + tmpVIPLimit + "开放");

        NiceData tmpToggleData = GameCommon.GetButtonData(elem, "explore_select_way_bg");
        if (tmpToggleData != null)
        {
            tmpToggleData.set("SEARCH_WAY", wayType);
            tmpToggleData.set("CALLBACK", mCallback);
        }
    }

    private void __OnSelected(FAIRYLAND_SEARCH_WAY_TYPE wayType)
    {
        NiceData tmpBtnData = GameCommon.GetButtonData(mGameObjUI, "explore_way_affirm_button");
        if (tmpBtnData != null)
        {
            tmpBtnData.set("SEARCH_WAY", wayType);
            tmpBtnData.set("CALLBACK", mCallback);
        }
    }
}

/// <summary>
/// Toggle按钮
/// </summary>
class Button_explore_select_way_bg : CEvent
{
    public override bool _DoEvent()
    {
        FAIRYLAND_SEARCH_WAY_TYPE tmpWayType = (FAIRYLAND_SEARCH_WAY_TYPE)getObject("SEARCH_WAY");
        DataCenter.SetData("FAIRYLAND_SEARCH_WAY_WINDOW", "WAY_SELECTED", tmpWayType);

        return true;
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_explore_way_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("FAIRYLAND_SEARCH_WAY_WINDOW");

        return true;
    }
}

/// <summary>
/// 确认
/// </summary>
class Button_explore_way_affirm_button : CEvent
{
    public override bool _DoEvent()
    {
        FAIRYLAND_SEARCH_WAY_TYPE tmpWayType = (FAIRYLAND_SEARCH_WAY_TYPE)getObject("SEARCH_WAY");
        Action<FAIRYLAND_SEARCH_WAY_TYPE> tmpCallback = (Action<FAIRYLAND_SEARCH_WAY_TYPE>)getObject("CALLBACK");

        DataCenter.CloseWindow("FAIRYLAND_SEARCH_WAY_WINDOW");

        if (tmpCallback != null)
            tmpCallback(tmpWayType);

        return true;
    }
}
