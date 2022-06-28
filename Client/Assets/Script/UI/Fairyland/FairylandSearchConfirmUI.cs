using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System;

//符灵探险寻仙确认界面

public class FairylandSearchConfirmData
{
    public int ItemTid { set; get; }        //物品Id
    public int ItemCount { set; get; }      //物品数量
    public int CostTime { set; get; }       //消耗时间

    // By XiaoWen
    // Begin
    public FAIRYLAND_SEARCH_WAY_TYPE FairylandWay { set; get; }   //寻仙方式
    // End
}

public class FairylandSearchConfirmWindow : tWindow
{
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_explore_tip_ok_button", new DefineFactoryLog<Button_explore_tip_confirm_button>());
        EventCenter.Self.RegisterEvent("Button_explore_tip_cancel_button", new DefineFactoryLog<Button_explore_tip_close_button>());
		EventCenter.Self.RegisterEvent ("Button_explore_tip_close_button",new DefineFactoryLog<Button_explore_tip_close_button>() );
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "OK_CALLBACK":
                {
                    CallBack tmpCallback = objVal as CallBack;
                    NiceData tmpBtnData = GameCommon.GetButtonData(mGameObjUI, "explore_tip_ok_button");
                    if (tmpBtnData != null)
                        tmpBtnData.set("CALLBACK", tmpCallback);
                } break;
            case "CANCEL_CALLBACK":
                {
                    CallBack tmpCallback = objVal as CallBack;
                    NiceData tmpBtnData = GameCommon.GetButtonData(mGameObjUI, "explore_tip_cancel_button");
                    if (tmpBtnData != null)
                        tmpBtnData.set("CALLBACK", tmpCallback);
                } break;
        }
    }

    public override bool Refresh(object param)
    {
        FairylandSearchConfirmData tmpData = param as FairylandSearchConfirmData;

		GameCommon.SetResIcon (mGameObjUI,"item_icon",tmpData.ItemTid,false);
        //图标
        //GameCommon.SetOnlyItemIcon(GetSub("info_bg"), "item_icon", tmpData.ItemTid);
		GameCommon.SetResIcon (mGameObjUI,"item_icon",tmpData.ItemTid,false);
        //提示文字
        // By XiaoWen
        // Begin
        string labelWay = "";
        switch (tmpData.FairylandWay)
        {
            case FAIRYLAND_SEARCH_WAY_TYPE.NORMAL:
                labelWay = "普通";
                break;
            case FAIRYLAND_SEARCH_WAY_TYPE.MIDDLE:
                labelWay = "中级";
                break;
            case FAIRYLAND_SEARCH_WAY_TYPE.HIGH:
                labelWay = "高级";
                break;
        }
        string tmpStr = "x" + tmpData.ItemCount.ToString() + "进行" + tmpData.CostTime.ToString() + "小时的" + labelWay + "镇妖吗？";
        GameCommon.SetUIText(GetSub("info_bg"), "tips_label04", tmpStr);
        // End

        return true;
    }
}

/// <summary>
/// 操作按钮
/// </summary>
class Button_explore_tip_confirm_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("FAIRYLAND_SEARCH_CONFIRM_WINDOW");

        CallBack tmpCallback = getObject("CALLBACK") as CallBack;
        if (tmpCallback != null)
            tmpCallback.Run();

        return true;
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_explore_tip_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("FAIRYLAND_SEARCH_CONFIRM_WINDOW");

        CallBack tmpCallback = getObject("CALLBACK") as CallBack;
        if (tmpCallback != null)
            tmpCallback.Run();

        return true;
    }
}
