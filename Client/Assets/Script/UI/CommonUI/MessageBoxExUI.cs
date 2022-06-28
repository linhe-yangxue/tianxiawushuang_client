using UnityEngine;
using System.Collections;
using System;
using Logic;

public enum MESSAGE_BOX_EX_BUTTON_TYPE
{
    OK_CANCEL,      //确定、取消按钮
    OK              //只有确定按钮
}

public class MessageBoxExOpenData
{
    private string mTitle = "提示";         //标题
    private string mContent = "";           //内容

    private MESSAGE_BOX_EX_BUTTON_TYPE mBtnType = MESSAGE_BOX_EX_BUTTON_TYPE.OK_CANCEL;

    private string mBtnOkInfo = "确定";         //确定按钮标签内容
    private Action mBtnOkCallback;              //确定按钮回调
    private string mBtnCancelInfo = "取消";     //取消按钮标签内容
    private Action mBtnCancelCallback;          //取消按钮回调

    public string Title
    {
        set { mTitle = value; }
        get { return mTitle; }
    }
    public string Content
    {
        set { mContent = value; }
        get { return mContent; }
    }

    public MESSAGE_BOX_EX_BUTTON_TYPE ButtonType
    {
        set { mBtnType = value; }
        get { return mBtnType; }
    }

    public string BtnOkInfo
    {
        set { mBtnOkInfo = value; }
        get { return mBtnOkInfo; }
    }
    public Action BtnOkCallback
    {
        set { mBtnOkCallback = value; }
        get { return mBtnOkCallback; }
    }
    public string BtnCancelInfo
    {
        set { mBtnCancelInfo = value; }
        get { return mBtnCancelInfo; }
    }
    public Action BtnCancelCallback
    {
        set { mBtnCancelCallback = value; }
        get { return mBtnCancelCallback; }
    }
}

public class MessageBoxExUI : tWindow
{
    private Action mBtnOkCallback;
    private Action mBtnCancelCallback;

    public override void Init()
    {
        base.Init();

        EventCenter.Register("Button_button_ok_message_ex_window", new DefineFactoryLog<Button_button_ok_message_ex_window>());
        EventCenter.Register("Button_button_middle_ok_message_ex_window", new DefineFactoryLog<Button_button_ok_message_ex_window>());
        EventCenter.Register("Button_button_close_message_ex_window", new DefineFactoryLog<Button_button_close_message_ex_window>());
        EventCenter.Register("Button_message_ok_ex_window_background", new DefineFactoryLog<Button_button_close_message_ex_window>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        MessageBoxExOpenData tmpOpenData = param as MessageBoxExOpenData;
        if (tmpOpenData == null)
            return;
        __Refresh(tmpOpenData);
    }

    private void __Refresh(MessageBoxExOpenData openData)
    {
        if (openData == null)
            return;

        //标题
        GameCommon.SetUIText(mGameObjUI, "title_label", openData.Title);

        //内容
        GameCommon.SetUIText(mGameObjUI, "text_label", openData.Content);

        __VisibleButton(openData.ButtonType);
        switch (openData.ButtonType)
        {
            case MESSAGE_BOX_EX_BUTTON_TYPE.OK_CANCEL:
                {
                    //确定按钮
                    GameObject tmpGOOk = GetSub("button_ok_message_ex_window");
                    if (tmpGOOk != null)
                    {
                        GameCommon.SetUIText(tmpGOOk, "Label", openData.BtnOkInfo);
                        NiceData tmpBtnOkData = tmpGOOk.GetComponent<UIButtonEvent>().mData;
                        if (tmpBtnOkData != null)
                            tmpBtnOkData.set("OPEN_DATA", openData);
                    }

                    //取消按钮
                    GameObject tmpGOCancel = GetSub("button_close_message_ex_window");
                    if (tmpGOCancel != null)
                    {
                        GameCommon.SetUIText(tmpGOCancel, "Label", openData.BtnOkInfo);
                        NiceData tmpBtnCancelData = tmpGOCancel.GetComponent<UIButtonEvent>().mData;
                        if (tmpBtnCancelData != null)
                            tmpBtnCancelData.set("OPEN_DATA", openData);
                    }
                } break;
            case MESSAGE_BOX_EX_BUTTON_TYPE.OK:
                {
                    //中间确定按钮
                    GameObject tmpGOMiddleOk = GetSub("button_middle_ok_message_ex_window");
                    if (tmpGOMiddleOk != null)
                    {
                        GameCommon.SetUIText(tmpGOMiddleOk, "Label", openData.BtnOkInfo);
                        NiceData tmpBtnMiddleOkData = tmpGOMiddleOk.GetComponent<UIButtonEvent>().mData;
                        if (tmpBtnMiddleOkData != null)
                            tmpBtnMiddleOkData.set("OPEN_DATA", openData);
                    }
                } break;
        }
    }

    private void __VisibleButton(MESSAGE_BOX_EX_BUTTON_TYPE btnType)
    {
        switch (btnType)
        {
            case MESSAGE_BOX_EX_BUTTON_TYPE.OK_CANCEL:
                {
                    GameCommon.SetUIVisiable(mGameObjUI, "button_ok_message_ex_window", true);
                    GameCommon.SetUIVisiable(mGameObjUI, "button_close_message_ex_window", true);
                    GameCommon.SetUIVisiable(mGameObjUI, "button_middle_ok_message_ex_window", false);
                }break;
            case MESSAGE_BOX_EX_BUTTON_TYPE.OK:
                {
                    GameCommon.SetUIVisiable(mGameObjUI, "button_ok_message_ex_window", false);
                    GameCommon.SetUIVisiable(mGameObjUI, "button_close_message_ex_window", false);
                    GameCommon.SetUIVisiable(mGameObjUI, "button_middle_ok_message_ex_window", true);
                } break;
        }
    }
}

/// <summary>
/// 确定按钮
/// </summary>
class Button_button_ok_message_ex_window : CEvent
{
    public override bool _DoEvent()
    {
        MessageBoxExOpenData tmpOpenData = getObject("OPEN_DATA") as MessageBoxExOpenData;
        if(tmpOpenData == null)
            return false;

        DataCenter.CloseWindow("MESSAGE_BOX_EX_WINDOW");

        if (tmpOpenData.BtnOkCallback != null)
            tmpOpenData.BtnOkCallback();

        return true;
    }
}
/// <summary>
/// 取消按钮
/// </summary>
class Button_button_close_message_ex_window : CEvent
{
    public override bool _DoEvent()
    {
        MessageBoxExOpenData tmpOpenData = getObject("OPEN_DATA") as MessageBoxExOpenData;
        if (tmpOpenData == null)
            return false;

        DataCenter.CloseWindow("MESSAGE_BOX_EX_WINDOW");

        if (tmpOpenData.BtnCancelCallback != null)
            tmpOpenData.BtnCancelCallback();

        return true;
    }
}
