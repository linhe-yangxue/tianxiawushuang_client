using UnityEngine;
using System.Collections;
using System;

public class U3DSharkEventListener : MonoBehaviour {

    private bool mIsLogin = false;      //是否登陆

    void NotifyInitFinish(U3DSharkEvent evt)
    {
         
    }
    //登录操作完成后的回调函数
    void NotifyLogin(U3DSharkEvent evt)
    {
        mIsLogin = true;

        //by chenliang
        //begin

        DEBUG.Log("U3DSharkEventListener - NotifyLogin " + evt.evtType.ToString() + ", data = " + evt.evtData.DataToString());

        //关闭新手引导
        Guide.Terminate(false);
        //关闭游戏中SDK登录按钮界面
        DataCenter.CloseWindow("FIRST_LOAD_SDK_WINDOW");

        //end
        var baseData=evt.evtData;
        DEBUG.LogError("StartLogin");
        var id = baseData.GetData(U3DSharkAttName.USER_ID);
        var token=baseData.GetData(U3DSharkAttName.USER_TOKEN);
        var data=baseData.GetData(U3DSharkAttName.EXTRA);
        var channelId=U3DSharkSDK.Instance.GetPlatformData().GetData(U3DSharkAttName.CHANNEL_ID);
        PlayerPrefs.SetString("USRNAME",baseData.GetData(U3DSharkAttName.USER_NAME));
        //by chenliang
        //begin

        CommonParam.SDKUserID = id;
        CommonParam.SDKToken = token;
        DEBUG.Log("U3DSharkEventListener - Client id = " + CommonParam.SDKUserID + ", token = " + CommonParam.SDKToken);

        //end
#if !UNITY_EDITOR && !NO_USE_SDK
        LoginNet.sdkData=baseData;
#endif
        var cs=new CS_SDKLogin(id,token,data,channelId);
        HttpModule.Instace.SendGameServerMessageT(cs,text => {
            //by chenliang
            //begin

//            DEBUG.LogError("SendGameServerMessageT");
//------------------
            DEBUG.Log("U3DSharkEventListener - SendGameServerMessageT");

            //end
            SC_SDKLogin sc=JCode.Decode<SC_SDKLogin>(text);
            if(sc.sdkRet==0) {
                //by chenliang
                //begin

//                 CommonParam.mUId = sc.uid;
//                 CommonParam.mStrLoginToken = sc.token;
//                DEBUG.LogError("token:"+sc.token);
//---------------------------
                //如果此时返回有channelUid、channelToken，替换之前的channelUid、channelToken
                if (sc.channelUid != "" && sc.channelUid != null)
                    CommonParam.SDKUserID = sc.channelUid;
                if (sc.channelToken != "" && sc.channelToken != null)
                    CommonParam.SDKToken = sc.channelToken;

                CommonParam.mUId = sc.uid;
                CommonParam.mStrLoginToken = sc.token;
                DEBUG.Log("U3DSharkEventListener - Server id = " + CommonParam.SDKUserID + ", token = " + CommonParam.SDKToken);

                if (PlayerPrefs.HasKey("QUICK_LOGIN"))
                {
                    PlayerPrefs.DeleteKey("QUICK_LOGIN");
                    PlayerPrefs.Save();
                }

                DataCenter.Set("ENTER_GS", true);

                DataCenter.OpenWindow("LANDING_WINDOW", false);
                DataCenter.CloseWindow("SELECT_SERVER_WINDOW");
                DataCenter.SetData("LANDING_WINDOW", "REFRESH", null);

                //设置登录令牌标志
                LoginData.Instance.IsLoginTokenValid = true;
                LoginData.Instance.IsGameTokenValid = false;
                LoginData.Instance.IsInGameScene = false;

                //end
                DataCenter.Set("TOKEN",sc.token);
                CS_ZoneList cs2=new CS_ZoneList(CommonParam.mStrLoginToken);
                HttpModule.Instace.SendLoginMessage(cs2,"CS_ZoneList",text2 => {

                    LoginNet.RequestLoginHistorySuccess(text2);
                    DEBUG.LogError("LoginOK");

                },NetManager.RequestFail);
            } else {
                //by chenliang
                //begin

//                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_SDK,"",() => U3DSharkSDK.Instance.Login());
//----------------------
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_SDK, "",
                    () =>
                    {
                        DEBUG.Log("NotifyLogin - FirstLoadSDKWindow");
                        DataCenter.OpenWindow("FIRST_LOAD_SDK_WINDOW");
                        //停止心跳包
                        StaticDefine.useHeartbeat = false;
                        //不自动登录
//                         GlobalModule.DoOnNextUpdate(() =>
//                         {
//                             GlobalModule.DoLater(
//                                 () =>
//                                 {
//                                     U3DSharkSDK.Instance.Login();
//                                 }, 0.1f);
//                         });
                    });

                //end
            }


           
        },
        NetManager.RequestFail);

       
        //解析渠道登录成功返回的信息，一般有user_token,user_id...
        //CP方需要将信息解析为CP服务器约定的数据格式转发给游戏服务器以完成游戏的登录验证

    }
    //更新用户信息完成后回调
    void NotifyUpdateFinish(U3DSharkEvent evt)
    {

    }
    //支付结果通知回调，CP需根据支付返回结果完成相应逻辑
    void NotifyPayResult(U3DSharkEvent evt)
    {
        //by chenliang
        //begin

//         if (evt.evtData.GetData(U3DSharkAttName.PAY_RESULT).Equals("1"))
//         { //支付成功
//             
//         }
//         else
//         {//支付失败,
//             
//         }
//--------------------
        string tmpPayReason = "";
        string tmpPayData = "";
        string tmpPayResult = evt.evtData.GetData(U3DSharkAttName.PAY_RESULT);
        int tmpPayCode = -1;
        if (!int.TryParse(tmpPayResult, out tmpPayCode))
        {
            DEBUG.Log("NotifyPayRestul convert PAY_RESULT(" + tmpPayResult + ") failed");
            tmpPayCode = 0;
        }
        tmpPayReason = evt.evtData.GetData(U3DSharkAttName.PAY_RESULT_REASON);
        tmpPayData = evt.evtData.GetData(U3DSharkAttName.PAY_RESULT_DATA);
        SDKPayHelper.Instance.PayComplete(tmpPayCode, tmpPayReason, tmpPayData);

        //end

    }
    //by chenliang
    //begin

    private static Action msLogoutCallback;     //登出回调，为空时默认行为切换账号
    public static Action LogoutCallback
    {
        set { msLogoutCallback = value; }
        get { return msLogoutCallback; }
    }

    //end
    //登出结果通知回调
    void NotifyLogout(U3DSharkEvent evt)
    {
        //by chenliang
        //beign

        if (!mIsLogin)
            return;
        mIsLogin = false;

        DEBUG.Log("U3DSharkEventListener - NotifyLogout " + evt.evtType.ToString() + ", data = " + evt.evtData.DataToString());

        Guide.Terminate(false);

        if (msLogoutCallback != null)
        {
            msLogoutCallback();
            msLogoutCallback = null;
        }
        else
        {
            //切换账号
            GlobalModule.ChangeAccount();
        }

        //end
    }
    //重登录结果通知回调
    void NotifyRelogin(U3DSharkEvent evt)
    {
  
        //by chenliang
        //begin

        DEBUG.Log("U3DSharkEventListener - NotifyRelogin " + evt.evtType.ToString() + ", data = " + evt.evtData.DataToString());

        //清除当前账号信息
        GlobalModule.ClearAccountUIAndData();

        __LoginFromSDKData(evt);

        //end
    }
    //取消退出游戏通知回调
    void NotifyCancelExit(U3DSharkEvent evt)
    {

    }
    //本地推送通知回调，游戏需根据收到的数据实现相应的游戏逻辑
    void NotifyReceiveLocalPush(U3DSharkEvent evt)
    {

    }
    //获取好友列表通知回调
    void NotifyUserFriends(U3DSharkEvent evt)
    {

    }
    //分享结果通知回调
    void NotifyShareResult(U3DSharkEvent evt)
    {

    }
    //by chenliang
    //begin

    private void __LoginFromSDKData(U3DSharkEvent evt)
    {
        //关闭游戏中SDK登录按钮界面
        DataCenter.CloseWindow("FIRST_LOAD_SDK_WINDOW");

        var baseData = evt.evtData;
        DEBUG.LogError("StartLogin");
        var id = baseData.GetData(U3DSharkAttName.USER_ID);
        var token = baseData.GetData(U3DSharkAttName.USER_TOKEN);
        var data = baseData.GetData(U3DSharkAttName.EXTRA);
        var channelId = U3DSharkSDK.Instance.GetPlatformData().GetData(U3DSharkAttName.CHANNEL_ID);
        PlayerPrefs.SetString("USRNAME", baseData.GetData(U3DSharkAttName.USER_NAME));
#if !UNITY_EDITOR && !NO_USE_SDK
        LoginNet.sdkData=baseData;
#endif
        CommonParam.SDKUserID = id;
        CommonParam.SDKToken = token;
        DEBUG.Log("U3DSharkEventListener - Client id = " + CommonParam.SDKUserID + ", token = " + CommonParam.SDKToken);
        var cs = new CS_SDKLogin(id, token, data, channelId);
        HttpModule.Instace.SendGameServerMessageT(cs, text =>
        {
            DEBUG.Log("U3DSharkEventListener - SendGameServerMessageT");
            SC_SDKLogin sc = JCode.Decode<SC_SDKLogin>(text);
            if (sc.sdkRet == 0)
            {
                //如果此时返回有channelUid、channelToken，替换之前的channelUid、channelToken
                if (sc.channelUid != "" && sc.channelUid != null)
                    CommonParam.SDKUserID = sc.channelUid;
                if (sc.channelToken != "" && sc.channelToken != null)
                    CommonParam.SDKToken = sc.channelToken;

                CommonParam.mUId = sc.uid;
                CommonParam.mStrLoginToken = sc.token;
                DEBUG.Log("U3DSharkEventListener - Server id = " + CommonParam.SDKUserID + ", token = " + CommonParam.SDKToken);

                if (PlayerPrefs.HasKey("QUICK_LOGIN"))
                {
                    PlayerPrefs.DeleteKey("QUICK_LOGIN");
                    PlayerPrefs.Save();
                }
                DataCenter.Set("ENTER_GS", true);

                DataCenter.OpenWindow("LANDING_WINDOW", false);
                DataCenter.CloseWindow("SELECT_SERVER_WINDOW");
                DataCenter.SetData("LANDING_WINDOW", "REFRESH", null);

                //设置登录令牌标志
                LoginData.Instance.IsLoginTokenValid = true;
                LoginData.Instance.IsGameTokenValid = false;
                LoginData.Instance.IsInGameScene = false;
                DataCenter.Set("TOKEN", sc.token);
                CS_ZoneList cs2 = new CS_ZoneList(CommonParam.mStrLoginToken);
                HttpModule.Instace.SendLoginMessage(cs2, "CS_ZoneList", text2 =>
                {

                    LoginNet.RequestLoginHistorySuccess(text2);
                    DEBUG.LogError("LoginOK");

                }, NetManager.RequestFail);
            }
            else
            {
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_SDK, "",
                    () =>
                    {
                        DEBUG.Log("NotifyLogin - FirstLoadSDKWindow");
                        DataCenter.OpenWindow("FIRST_LOAD_SDK_WINDOW");
                        //停止心跳包
                        StaticDefine.useHeartbeat = false;
//                         //不自动登录
//                         GlobalModule.DoOnNextUpdate(() =>
//                         {
//                             GlobalModule.DoLater(
//                                 () =>
//                                 {
//                                     U3DSharkSDK.Instance.Login();
//                                 }, 0.1f);
//                         });
                    });
            }
        },
        NetManager.RequestFail);
    }

    //end





    /*
       void RequestLoginSuccess(bool isFromLoadAccount,bool isQuickLogin,string text) {

        //end
        SC_Login login=JCode.Decode<SC_Login>(text);

        if((STRING_INDEX)login.ret==STRING_INDEX.ERROR_NONE) {
            //by chenliang
            //begin

            if(login.accNotExists==1) {
                //账号不存在
                if(isFromLoadAccount)
                    DataCenter.OpenWindow("LOGIN_WINDOW",false);
                else
                    DataCenter.OpenMessageWindow("该账号不存在");
                return;
            } else if(login.wrongPw==1) {
                //密码错误
                if(isFromLoadAccount)
                    DataCenter.OpenWindow("LOGIN_WINDOW",false);
                else
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NEED_PASSWORD);
                return;
            }

            //检测是否为快速登录
            if(isQuickLogin) {
                PlayerPrefs.SetString("QUICK_LOGIN","quick_login");
                PlayerPrefs.Save();
            } else {
                if(PlayerPrefs.HasKey("QUICK_LOGIN")) {
                    PlayerPrefs.DeleteKey("QUICK_LOGIN");
                    PlayerPrefs.Save();
                }
            }

            DataCenter.CloseWindow("TOURISTS_LOAD_WINDOW");
            DataCenter.CloseWindow("FIRST_LOAD_WINDOW");

            //end
            DataCenter.CloseWindow("LOGIN_WINDOW");

            {
                DataCenter.Set("ENTER_GS",true);

                DataCenter.OpenWindow("LANDING_WINDOW",false);
                DataCenter.CloseWindow("SELECT_SERVER_WINDOW");
                DataCenter.SetData("LANDING_WINDOW","REFRESH",accounts[0]);
            }

            // Save account and password md5
            string account=accounts[0];
            string password=accounts[1];
            GameCommon.SaveUsrLoginDataFromUnity(account,password);
            // add token for game controller
            DataCenter.Set("TOKEN",login.tk);

            CommonParam.mAccount=account;

            // RequestLoginHistory(login.tk);
            //by chenliang
            //begin

            //             CommonParam.mStrToken = login.tk;
            //             CommonParam.mUId = login.uid;
            //---------------------
            CommonParam.mStrLoginToken=login.tk;
            CommonParam.mUId=login.uid;

            //设置登录令牌标志
            LoginData.Instance.IsLoginTokenValid=true;

            //end

            RequestLoginHistory(login.tk);
        }
    }
	
     
 
     
     */
    private void __NotifyExtraFunction(U3DSharkEvent evt)
    {
        //乐变
        string tmpLBRet = evt.evtData.GetData("lb_update_result");
        if (tmpLBRet != "" && tmpLBRet != null)
        {
            //返回值：
            //-1：请求失败；1：未知错误；2：没有更新；
            //3：有非强更版本；4：有强更版本
            int tmpLBValueRet = -2;
            if (int.TryParse(tmpLBRet, out tmpLBValueRet))
                HotUpdateLoading.LeBianUpdateState = tmpLBValueRet;
        }
    }



    //以下部分不建议修改
    private static U3DSharkEventListener instance;
    private static object syncRoot = new object();
    private static GameObject _container;
    private static int createCount = 0;
    void Awake()
    {
        U3DSharkSDK.Instance.AddEventDelegate(SharkEventType.EVENT_INIT_FINISH, NotifyInitFinish);
        U3DSharkSDK.Instance.AddEventDelegate(SharkEventType.EVENT_UPDATE_FINISH, NotifyUpdateFinish);
        U3DSharkSDK.Instance.AddEventDelegate(SharkEventType.EVENT_LOGIN_SUCCESS, NotifyLogin);
        U3DSharkSDK.Instance.AddEventDelegate(SharkEventType.EVENT_PAY_RESULT, NotifyPayResult);
        U3DSharkSDK.Instance.AddEventDelegate(SharkEventType.EVENT_LOGOUT, NotifyLogout);
        U3DSharkSDK.Instance.AddEventDelegate(SharkEventType.EVENT_RELOGIN, NotifyRelogin);
        U3DSharkSDK.Instance.AddEventDelegate(SharkEventType.EVENT_CANCEL_EXIT_GAME, NotifyCancelExit);
        U3DSharkSDK.Instance.AddEventDelegate(SharkEventType.EVENT_SHARE_RESULT, NotifyCancelExit);
        U3DSharkSDK.Instance.AddEventDelegate(SharkEventType.EVENT_RECEIVE_LOCAL_PUSH, NotifyCancelExit);
        U3DSharkSDK.Instance.AddEventDelegate(SharkEventType.EVENT_GET_FRIEND_RESULT, NotifyUserFriends);
        U3DSharkSDK.Instance.AddEventDelegate(SharkEventType.EVENT_EXTRA_FUNCTION, __NotifyExtraFunction);
        DontDestroyOnLoad(this.gameObject);
    }
    public static U3DSharkEventListener Instance
    {
        get
        {
            if (null == instance)
            {
                _container = new GameObject();
                _container.name = "SDKController";
                UnityEngine.Object.DontDestroyOnLoad(_container);
                lock (syncRoot)
                {
                    if (null == instance)
                    {
                        createCount++;
                        DEBUG.Log("createCount::::" + createCount);
                        instance = _container.AddComponent(typeof(U3DSharkEventListener)) as U3DSharkEventListener;
                    }
                }
            }
            return instance;
        }
    }
    public void InitSelf()
    {
        DEBUG.Log("Init U3DSharkEventListener Finished !");
    }
}
