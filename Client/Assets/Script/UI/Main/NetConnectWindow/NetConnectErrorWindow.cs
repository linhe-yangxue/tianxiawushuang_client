using UnityEngine;
using System.Collections;
using DataTable;
using System;

class NetErrorWindow:tWindow 
{
    public static bool isOpened { get; private set; }

    protected override void OpenInit() {
        base.OpenInit();
        AddButtonAction("reLogin", () => {
            StaticDefine.useHeartbeat = false;
            //by chenliang
            //beign

//            GlobalModule.ChangeAccount();
//-----------------------
#if !UNITY_EDITOR && !NO_USE_SDK
            U3DSharkEventListener.LogoutCallback = GlobalModule.ChangeAccount;
            U3DSharkSDK.Instance.Logout();
#else
            GlobalModule.ChangeAccount();
#endif

            //end
            DataCenter.CloseWindow(UIWindowString.netError);
        });

        isOpened = true;
    }

    public override void OnClose()
    {
        base.OnClose();
        isOpened = false;
    }
}

class ConnectErrorWindow : tWindow 
{
    public static bool isOpened { get; private set; }

    protected override void OpenInit() {
        base.OpenInit();
        AddButtonAction("reLogin", () => {
            StaticDefine.useHeartbeat = false;
            //by chenliang
            //begin

//            GlobalModule.ChangeAccount();
//-------------------
            if (mReloginCallback != null)
            {
                mReloginCallback();
                mReloginCallback = null;
            }
            else
            {
#if !UNITY_EDITOR && !NO_USE_SDK
                U3DSharkEventListener.LogoutCallback = GlobalModule.ChangeAccount;
                U3DSharkSDK.Instance.Logout();
#else
                GlobalModule.ChangeAccount();
#endif
            }

            //end
            DataCenter.CloseWindow(UIWindowString.netError);
        });

        AddButtonAction("reConnect", () => {
            //by chenliang
            //begin

//            NetManager.ReConnect();
//------------------
            if (mReconnectCallback != null)
            {
                mReconnectCallback();
                mReconnectCallback = null;
            }
            else
                NetManager.ReConnect();

            //end
            DataCenter.CloseWindow(UIWindowString.connectError);
        });

        isOpened = true;
    }
    //by chenliang
    //begin

    private Action mReloginCallback;
    private Action mReconnectCallback;
    private Action mCloseCallback;

    public override void Open(object param)
    {
        base.Open(param);

        mReloginCallback = null;
        mReconnectCallback = null;

        GameObject tmpReLoginGO = GameCommon.FindObject(mGameObjUI, "reLogin");
        if(tmpReLoginGO != null)
            GameCommon.SetUIText(tmpReLoginGO, "Label", "返回登陆");
        GameObject tmpReConnectGO = GameCommon.FindObject(mGameObjUI, "reConnect");
        if (tmpReConnectGO != null)
            GameCommon.SetUIText(tmpReConnectGO, "Label", "重新连接");
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "WINDOW_CONTENT":
                {
                    string tmpContent = "";
                    if (objVal is STRING_INDEX)
                    {
                        DataRecord tmpStrConfig = DataCenter.mStringList.GetRecord((int)objVal);
                        if (tmpStrConfig != null)
                            tmpContent = tmpStrConfig.get("STRING_CN");
                    }
                    else if (objVal is string)
                        tmpContent = (string)objVal;
                    UILabel tmpLBContent = GameCommon.FindComponent<UILabel>(mGameObjUI, "inform_label");
                    if (tmpLBContent != null)
                        tmpLBContent.text = tmpContent;
                } break;
            case "RE_LOGIN_BTN_TEXT":
                {
                    GameObject tmpGO = GameCommon.FindObject(mGameObjUI, "reLogin");
                    if (tmpGO != null)
                        GameCommon.SetUIText(tmpGO, "Label", (string)objVal);
                } break;
            case "RE_LOGIN_ACTION":
                {
                    mReloginCallback = (Action)objVal;
                } break;
            case "RE_CONNECT_BTN_TEXT":
                {
                    GameObject tmpGO = GameCommon.FindObject(mGameObjUI, "reConnect");
                    if (tmpGO != null)
                        GameCommon.SetUIText(tmpGO, "Label", (string)objVal);
                } break;
            case "RE_CONNECT_ACTION":
                {
                    mReconnectCallback = (Action)objVal;
                }break;
            case "CLOSE_CALLBACK": mCloseCallback = (Action)objVal; break;
        }
    }
    public override void OnClose()
    {
        if (mCloseCallback != null)
            mCloseCallback();

        mReloginCallback = null;
        mReconnectCallback = null;

        base.OnClose();
        isOpened = false;
    }

    //end
}
