using UnityEngine;
using System.Collections;
using Logic;
using System;
using System.IO;
using System.Text;

public enum RECHARGE_PAGE
{
    VIP_RIGHT,      //VIP特权
    RECHARGE        //充值
}

/// <summary>
/// 打开窗口时参数
/// </summary>
public class RechargeContainerOpenData
{
    private RECHARGE_PAGE mPage = RECHARGE_PAGE.VIP_RIGHT;
    private int mPanelDepth = 1;       //层级
    private Action mCloseCallback;// = () => MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
    private bool mIsUseDefaultCallback = true;          //是否用默认回调

    public RECHARGE_PAGE Page
    {
        set { mPage = value; }
        get { return mPage; }
    }
    public int PanelDepth
    {
        set { mPanelDepth = value; }
        get { return mPanelDepth; }
    }
    public Action CloseCallback
    {
        set
        {
            mCloseCallback = value;
            mIsUseDefaultCallback = false;
        }
        get { return mCloseCallback; }
    }
    public bool IsUseDefaultCallback
    {
        get { return mIsUseDefaultCallback; }
    }
}

/// <summary>
/// VIP特权、充值父窗口
/// </summary>
public class RechargeContainerWindow : tWindow
{
    private Action mCloseCallback;
    private bool mIsUseDefaultCallback = true;

    /// <summary>
    /// 打开自己
    /// </summary>
    /// <param name="openData"></param>
    public static void OpenMyself(RechargeContainerOpenData openData)
    {
        DataCenter.OpenWindow("RECHARGE_CONTAINER_WINDOW", openData);

        // add by LC
        // begin

        //MainUIScript.Self.HideMainBGUI();

        // end
    }

    public override void Init()
    {
        base.Init();

        EventCenter.Register("Button_recharge_button", new DefineFactoryLog<Button_Recharge_Open>());
        EventCenter.Register("Button_recharge_vip_window_back_btn", new DefineFactoryLog<Button_Recharge_Close>());
        EventCenter.Register("Button_recharge_close_btn", new DefineFactoryLog<Button_recharge_close_btn>());
        EventCenter.Register("Button_recharge_vip_window_bg_btn", new DefineFactoryLog<Button_recharge_close_btn>());
        
    }

    public override void Open(object param)
    {
		Debug.Log ("支付面板222---" + param);
		DEBUG.Log ("支付面板222---" + param);
        if (param == null || (param as RechargeContainerOpenData) == null)
            return;

        base.Open(param);

        //DataCenter.OpenWindow("RECHARGE_CONTAINER_BACK_WINDOW");
		Debug.Log ("支付面板111---" + param);
		DEBUG.Log ("支付面板111---" + param);
        RechargeContainerOpenData tmpOpenData = param as RechargeContainerOpenData;
        mCloseCallback = tmpOpenData.CloseCallback;
        mIsUseDefaultCallback = tmpOpenData.IsUseDefaultCallback;
        __CloseAllSubWindow();
		Debug.Log ("支付面板---" + tmpOpenData.Page);
		DEBUG.Log ("支付面板---" + tmpOpenData);
        switch (tmpOpenData.Page)
        {
            case RECHARGE_PAGE.VIP_RIGHT:
                {
                    DataCenter.OpenWindow("VIP_WINDOW", tmpOpenData);
                } break;
           	 case RECHARGE_PAGE.RECHARGE:
                {
                    DataCenter.OpenWindow("RECHARGE_WINDOW", tmpOpenData);
                } break;
        }
		DEBUG.Log ("支付面板333---" + tmpOpenData.PanelDepth);
        __SetPanelDepth(tmpOpenData.PanelDepth);
        __SetLocalPosZ(CommonParam.rechagePosZ);

    }
    public override void Close()
    {
        __CloseAllSubWindow();
        DataCenter.CloseWindow("RECHARGE_CONTAINER_BACK_WINDOW");

        base.Close();

        if (mCloseCallback != null)
            mCloseCallback();

        if (!mIsUseDefaultCallback)
        {
            //请求CS_GetNotification
            GlobalModule.DoCoroutine(GetNotification_Requester.StartRequester());
        }
    }

    private void __CloseAllSubWindow()
    {
        DataCenter.CloseWindow("VIP_WINDOW");
        DataCenter.CloseWindow("RECHARGE_WINDOW");
    }

    /// <summary>
    /// 设置Panel深度
    /// </summary>
    /// <param name="depth"></param>
    private void __SetPanelDepth(int depth)
    {
        UIPanel tmpPanel = mGameObjUI.GetComponent<UIPanel>();
        if (tmpPanel != null)
            tmpPanel.depth = depth;
		DEBUG.Log("支付页面深度111----" + tmpPanel.depth);
        //tWindow tmpBackWin = DataCenter.GetData("RECHARGE_CONTAINER_BACK_WINDOW") as tWindow;
        //if (tmpBackWin != null)
        //{
        //    UIPanel tmpBackPanel = tmpBackWin.mGameObjUI.GetComponent<UIPanel>();
        //    if (tmpBackPanel != null)
        //        tmpBackPanel.depth = depth + 4;
        //}
    }

    private void __SetLocalPosZ(int posZ)
    {
        if (mGameObjUI != null)
        {
			DEBUG.Log("支付页面深度222----" + posZ);
            mGameObjUI.transform.localPosition = new Vector3(0, 0, posZ);
        }
    }
}

/// <summary>
///打开充值窗口
/// </summary>
class Button_Recharge_Open : CEvent
{
    public override bool _DoEvent()
    {
//        RechargeContainerOpenData tmpOpenData = new RechargeContainerOpenData() { Page=RECHARGE_PAGE.RECHARGE };
//        RechargeContainerWindow.OpenMyself(tmpOpenData);
		DEBUG.Log ("打开充值窗口---");
        GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, () =>
        {
            //GlobalModule.DoCoroutine(GetNotification_Requester.StartRequester());
            DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "UPDATE_ROLE_SELECT", true);
        }, CommonParam.rechageDepth);
        return true;
    }
}
/// <summary>
/// VIP退出
/// </summary>
class Button_Recharge_Close : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RECHARGE_CONTAINER_WINDOW");

        return true;
    }
}

class Button_recharge_close_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RECHARGE_CONTAINER_WINDOW");

        return true;
    }
}