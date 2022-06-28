using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

//符灵探险操作主界面

public enum FAIRYLAND_OP_WINDOW_TYPE
{
    NONE = -1,
    CONQUER_READY,              //征服准备界面
    SEARCH_SELECT,              //寻仙选择界面
    SEARCH_READY,               //寻仙准备界面
    SEARCH_RECORD,              //寻仙记录界面
    REPRESS_RIOT                //镇压界面
}

/// <summary>
/// 打开仙境操作界面时许传入的参数
/// </summary>
public class FairylandOPWindowData
{
    public int FairylandTid { set; get; }                       //仙境tid
    public FAIRYLAND_ELEMENT_STATE State { set; get; }          //状态
    public FAIRYLAND_OP_WINDOW_TYPE WindowType { set; get; }    //类型
    public bool IsMyselfFairyland { set; get; }                 //是否为自己的仙境
    public long ExploreEndTime { set; get; }                    //寻仙结束时间
    public object Data { set; get; }                            //数据
}

public class FairylandOPWindow : tWindow
{
    private string mLastLeftWindow = "";
    private string mLastRightWindow = "";
    private FAIRYLAND_OP_WINDOW_TYPE mWinType = FAIRYLAND_OP_WINDOW_TYPE.NONE;
    private FairylandOPWindowData mOPData;

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_explore_bg_close_button", new DefineFactoryLog<Button_explore_bg_close_button>());
        EventCenter.Self.RegisterEvent("Button_explore_bg_window", new DefineFactoryLog<Button_explore_bg_close_button>());

    }

    public override void Open(object param)
    {
        if(IsOpen())
            __CloseOPWindow();

        base.Open(param);

        Refresh(param);
    }

    public override void OnClose()
    {
        __CloseOPWindow();

        base.OnClose();

        switch (mWinType)
        {
            case FAIRYLAND_OP_WINDOW_TYPE.SEARCH_READY:
                {
                    mOPData.WindowType = FAIRYLAND_OP_WINDOW_TYPE.SEARCH_SELECT;
                    DataCenter.OpenWindow("FAIRYLAND_OP_WINDOW", mOPData);
                }break;
        }
    }

    public override bool Refresh(object param)
    {
        mOPData = param as FairylandOPWindowData;

        mWinType = mOPData.WindowType;

        //标题
        DataRecord tmpFairylandConfig = DataCenter.mFairylandConfig.GetRecord(mOPData.FairylandTid);
        SetText("title_label", tmpFairylandConfig.getData("NAME"));

        __OpenOPWindow(mOPData);

        switch (mOPData.State)
        {
            case FAIRYLAND_ELEMENT_STATE.EXPLORING:
            case FAIRYLAND_ELEMENT_STATE.RIOTING:
            case FAIRYLAND_ELEMENT_STATE.TO_HARVEST: GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandEvents(mOPData.FairylandTid)); break;
        }

        return true;
    }

    /// <summary>
    /// 打开指定窗口
    /// </summary>
    /// <param name="opWindow"></param>
    private void __OpenOPWindow(FairylandOPWindowData opData)
    {
        if (opData.WindowType == FAIRYLAND_OP_WINDOW_TYPE.NONE)
            return;

        FairylandWindow.GetOPWindowNameByWindowType(opData.WindowType, out mLastLeftWindow, out mLastRightWindow);
        DataCenter.OpenWindow(mLastLeftWindow, opData);
        DataCenter.OpenWindow(mLastRightWindow, opData);
    }

    /// <summary>
    /// 将上次打开的窗口关闭
    /// </summary>
    private void __CloseOPWindow()
    {
        DataCenter.CloseWindow(mLastLeftWindow);
        DataCenter.CloseWindow(mLastRightWindow);
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_explore_bg_close_button : CEvent
{
    public override bool _DoEvent()
    {
        // By XiaoWen
        // Bug #13543【寻仙】同时点击仙境+号和拜访好友按钮时会造成图中问题
        // Begin
        DataCenter.SetData("FAIRYLAND_OP_WINDOW", "isOpen", false);
        // End
        DataCenter.CloseWindow("FAIRYLAND_OP_WINDOW");

        return true;
    }
}
