using UnityEngine;
using System.Collections;
using Logic;

public class FirstLoadSDKWindow : tWindow
{
    public override void Init()
    {
        base.Init();

        EventCenter.Register("Button_enter_load_window_sdk_button", new DefineFactory<Button_enter_load_window_sdk_button>());
    }

    public override void Open(object param)
    {
        DEBUG.Log("FirstLoadSDKWindow - Open");
        base.Open(param);

        GameCommon.ResetWorldCameraColor();
        DataCenter.OpenWindow("VERSION_NUMBER_WINDOW");

        //停止心跳包
        StaticDefine.useHeartbeat = false;

        //自动登录
        GlobalModule.DoOnNextUpdate(() =>
        {
            GlobalModule.DoLater(
                () =>
                {
#if !UNITY_EDITOR && !NO_USE_SDK
                    U3DSharkSDK.Instance.Login();
#endif
                }, 0.1f);
        });
    }

    public override void OnClose()
    {
        DEBUG.Log("FirstLoadSDKWindow - OnClose");

        base.OnClose();
    }
}

/// <summary>
/// 登录SDK
/// </summary>
class Button_enter_load_window_sdk_button : CEvent
{
    public override bool _DoEvent()
    {
#if !UNITY_EDITOR && !NO_USE_SDK
        U3DSharkSDK.Instance.Login();
#endif

        return true;
    }
}
