using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using JCode;

public class MessageBase
{
	public string vs;
	public string pt;
	public int pi;
    //by chenliang
    //begin

//    public MessageBase()
//    {
//        vs = CommonParam.ClientVer;
//    }
//-------------------
    private bool mIsEncode = false;       //是否加密
    public MessageBase()
    {
        vs = CommonParam.ClientVer;
    }
    public MessageBase(bool isEncode)
    {
        vs = CommonParam.ClientVer;
        mIsEncode = isEncode;
    }

    public void SetIsEncode(bool isEncode)
    {
        mIsEncode = isEncode;
    }
    public bool GetIsEncode()
    {
        return mIsEncode;
    }

    //end
};

public class LoginServerMessage : MessageBase
{
    public int registtype;
};

public class GameServerMessage : MessageBase
{
    public string zuid;
    public string zid;
    public string tk;
    public string acc=PlayerPrefs.GetString("USRNAME");
    public string channel=DeviceBaseData.channel;

    public GameServerMessage()
    {
        //by chenliang
        //begin

//        tk = CommonParam.mStrToken;
//----------------
        //根据令牌状态设置协议令牌
        ResetToken();

        //end
        zuid = CommonParam.mUId;
        zid=CommonParam.mZoneID;
    }
    //by chenliang
    //begin

    public static string GetCurrentToken()
    {
        string tmpToken = "";
        if (LoginData.Instance.IsGameTokenValid)
            tmpToken = CommonParam.mStrToken;
        else if (LoginData.Instance.IsLoginTokenValid)
            tmpToken = CommonParam.mStrLoginToken;
        return tmpToken;
    }
    //根据令牌状态设置协议令牌
    public void ResetToken()
    {
        if (LoginData.Instance.IsGameTokenValid)
            tk = CommonParam.mStrToken;
        else if (LoginData.Instance.IsLoginTokenValid)
            tk = CommonParam.mStrLoginToken;
        else
            tk = "";
    }
    /// <summary>
    /// 重置基础数据
    /// </summary>
    public void ResetBaseData()
    {
        vs = CommonParam.ClientVer;

        ResetToken();

        zuid = CommonParam.mUId;
        zid = CommonParam.mZoneID;
    }

    //end
};

public class ChatServerMessage : GameServerMessage
{
};

public class RespMessage : MessageBase
{
    public int ret;
};

public class HttpModule : Singleton<HttpModule> {

    //by chenliang
    //begin

    private const int msMaxSendIndex = 100000;  //最大包序号
    private static int msCurrentSendIndex = 0;  //当前发送序号

    private HttpCrypto m_HttpCrypto = new HttpCrypto();

    private static int msRetryCount = 6;
    public static int RetryCount
    {
        set { msRetryCount = value; }
        get { return msRetryCount; }
    }
    private static float msCanRetryTimeDelta = 5.0f;    //最短重连时间间隔

    //end
    private int mMessageCount = 0;
	// Use this for initialization
    private class WWWData
    {
        public WWW www;
        //public WWWForm form;
        public string url;
        public string context;
        public CallBack successMethod;
        public CallBack failMethod;
        //public int retryCount;
        public bool failed;
        //public bool isGamePacket;
        //public bool encoded;
        //by chenliang
        //begin

        public bool IsEncode { set; get; }

        public int SendIndex { set; get; }
        //重发
        public Action ReSend { set; get; }

        //end
    }

    public delegate void CallBack(string text);

    /// <summary>
    /// 监听网络回复的监听器，若为null则执行正常的回调，否则执行该监听器逻辑
    /// 参数依次为回复消息的名称，成功时执行的回调和回调参数
    /// 返回值表示是否重置监听器为null（true表示重置）
    /// </summary>
    public Func<string, CallBack, string, bool> mRespListener = null;

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SendLoginMessage(MessageBase message, string funName, CallBack successMethod, CallBack failMethod)
    {
        if (message != null)
        {
            message.pt = funName;
            SendLoginMessage(message, successMethod, failMethod);
        }
    }

    public void SendLoginMessage(MessageBase message, CallBack successMethod, CallBack failMethod)
    {
        if (message != null)
        {
            //by chenliang
            //begin

//            SendMessage(message, @"http://" + CommonParam.LoginIP + ":" + CommonParam.LoginPort + "/" + message.pt + "?", successMethod, failMethod);
//-------------------
            if (CommonParam.isUseHttps)
                SendMessage(message, @"https://" + CommonParam.LoginIP + ":" + CommonParam.LoginPort + "/" + message.pt + "?", successMethod, failMethod);
            else
                SendMessage(message, @"http://" + CommonParam.LoginIP + ":" + CommonParam.LoginPort + "/" + message.pt + "?", successMethod, failMethod);

            //end
        }
    }

    public void SendGameServerMessageT<T>(T message, CallBack successMethod, CallBack failMethod, bool isNeedWaitEffect = true) where T:MessageBase
    {
        SendGameServerMessage(message, message.GetType().Name, successMethod, failMethod, isNeedWaitEffect);
    }

    public void SendGameServerMessage(MessageBase message, string funName, CallBack successMethod, CallBack failMethod, bool isNeedWaitEffect = true)
    {
        if (message != null)
        {
            message.pt = funName;
            SendGameServerMessage(message, successMethod, failMethod, isNeedWaitEffect);
        }
    }

    public void SendGameServerMessage(MessageBase message, CallBack successMethod, CallBack failMethod, bool isNeedWaitEffect = true)
    {
        if (message != null)
        {
            //by chenliang
            //begin

//            SendMessage(message, @"http://" + CommonParam.LoginIP + ":" + CommonParam.LoginPort + "/" + message.pt + "?", successMethod, failMethod, isNeedWaitEffect);
//------------------
            if (CommonParam.isUseHttps)
                SendMessage(message, @"https://" + CommonParam.LoginIP + ":" + CommonParam.LoginPort + "/" + message.pt + "?", successMethod, failMethod, isNeedWaitEffect);
            else
                SendMessage(message, @"http://" + CommonParam.LoginIP + ":" + CommonParam.LoginPort + "/" + message.pt + "?", successMethod, failMethod, isNeedWaitEffect);

            //end
        }
    }

    //by chenliang
    //begin

    public void Init()
    {
        msCurrentSendIndex = 0;
    }

    /// <summary>
    /// 获取当前发送序号
    /// </summary>
    /// <returns></returns>
    private static int __CurrentSendIndex()
    {
        int tmpIndex = msCurrentSendIndex++;
        if (msCurrentSendIndex >= msMaxSendIndex)
            msCurrentSendIndex = 0;
        return tmpIndex;
    }

    //end
    public void SendMessage(MessageBase message, string url, CallBack successMethod, CallBack failMethod, bool isNeedWaitEffect = true)
	{
        //by chenliang
        //begin

        //重发函数
        Action tmpReSend = () =>
        {
            //延迟重发，让玩家能看到窗口关闭
            GlobalModule.DoLater(() =>
            {
                GameServerMessage tmpGameMsg = message as GameServerMessage;
                if (tmpGameMsg != null)
                    tmpGameMsg.ResetBaseData();
                SendMessage(message, url, successMethod, failMethod, isNeedWaitEffect);
            }, 0.5f);
        };
        if (!NetManager.HasAnyNetConnectable())
        {
//             //如果在游戏里，重新获取数据，如果还未进游戏，就重新发送请求
//             NetManager.OpenNoNetConnectableWindow(
//                 (LoginData.Instance.IsGameTokenValid && LoginData.Instance.IsInGameScene) ? null : tmpReSend);
            //改为每次都重新发送
            NetManager.OpenNoNetConnectableWindow(tmpReSend);
            return;
        }
        //设置检查网络标志位
        GlobalModule.NeedCheckNetConnectable = true;

        //end

        //DEBUG.Log ("here");
		//Dictionary<string, string> dic = new Dictionary<string, string> ();
		//dic.Add ("action", "1");
        //cs_login_en cmd = new cs_login_en();
        //cmd.id = "1";
        //cmd.pw = "fuck";
		//Jcode.
		//JCode.Encode (cmd);
        WWWData data = new WWWData();
        //by chenliang
        //begin

        //设置重发函数
        data.ReSend = tmpReSend;
        data.SendIndex = __CurrentSendIndex();
        message.pi = data.SendIndex;

        //end
        data.failed = false;
        data.url = url;
        data.context = JCode.Encode(message);
        data.successMethod = successMethod;
        data.failMethod = failMethod;
        data.IsEncode = message.GetIsEncode();

        if(isNeedWaitEffect)
            Net.StartWaitEffect();

        StartCoroutine("GetRoutine", data);
        //by chenliang
        //begin

//        StartCoroutine("TimeoutRoutine", data);
//--------------
        //重连一定次数后再进行超时处理

        //end
	}

    public static bool CheckConnectValidInfo(string info)
    {
        DEBUG.Log("CheckConnectValidInfo - " + info);
        if (info == null)
            return true;
        //包含下列信息的判断为连接错误
        List<string> tmpListContainsInfo = new List<string>()
        {
            "couldn't connect to host",
            "connect failed",
            "ENETUNREACH",
            "Network is unreachable",
            "failed to connect",
            "ETIMEDOUT",
            "java.io.FileNotFoundException"
        };
        for (int i = 0, count = tmpListContainsInfo.Count; i < count; i++)
        {
            if (info.IndexOf(tmpListContainsInfo[i]) != -1)
                return false;
        }
        //包含下列信息的排除在外
        List<string> tmpListExceptInfo = new List<string>()
        {
            "500 ",
            "400 ",
            "404 ",
            "900 "
        };
        for (int i = 0, count = tmpListExceptInfo.Count; i < count; i++)
        {
            if (info.IndexOf(tmpListExceptInfo[i]) != -1)
                return true;
        }
        //剩余的都为连接错误
        return false;
    }

    // get
    IEnumerator GetRoutine(WWWData wwwData)
	{       
        //by chenliang
        //begin

//         byte[] bytedata = System.Text.Encoding.UTF8.GetBytes(wwwData.context);
//         string strData = Convert.ToBase64String(bytedata, 0, bytedata.Length);
//
//         wwwData.www = new WWW(System.Uri.EscapeUriString(wwwData.url + strData));
//         mMessageCount++;
//         DEBUG.Log("send message succuss. wwwData.url = " + wwwData.url + "\n wwwData.context = " + wwwData.context + "\n messagecount = " + mMessageCount);
//         //GlobalModule.stopwatch.Reset();
//         //GlobalModule.stopwatch.Start();
//         //DEBUG.LogError("total start GetRoutine Ticks: " + GlobalModule.stopwatch.ElapsedTicks + " mS: " + GlobalModule.stopwatch.ElapsedMilliseconds);
//         yield return wwwData.www;
//         //DEBUG.LogError("total end GetRoutine Ticks: " + GlobalModule.stopwatch.ElapsedTicks + " mS: " + GlobalModule.stopwatch.ElapsedMilliseconds);
//         mMessageCount--;
//         DEBUG.Log("get message succuss. wwwData.www.text = " + wwwData.www.text + "\n messagecount = " + mMessageCount);
//         if (mMessageCount == 0)
//         {
//             StopCoroutine("TimeoutRoutine");
//             Net.StopWaitEffect();
//         }
        //-----------------------------
        if(m_HttpCrypto == null)
            yield break;
        //Http加密
        string tmpHttpEncode = "";
        if (wwwData.IsEncode)
        {
            Debug.Log("加密协议----");
            tmpHttpEncode = m_HttpCrypto.Encode(wwwData.url, wwwData.context);
//            DEBUG.Log("Http : " + tmpHttpEncode);
			Debug.Log("加密协议111----" + tmpHttpEncode);
            if (tmpHttpEncode == "")
            {
                GameCommon.OpenToLoginWindow(STRING_INDEX.ERROR_HTTP_PACKAGE_INDEX_REPEAT, null);
                yield break;
            }
        }
        else
        {
            //无Http无加密
            Debug.Log("没有加密协议----" + wwwData.url);
            byte[] bytedata = System.Text.Encoding.UTF8.GetBytes(wwwData.context);
            string strData = Convert.ToBase64String(bytedata, 0, bytedata.Length);
            tmpHttpEncode = wwwData.url + strData;

        }

        //增加重连机制
        int tmpRetryCount = 0;
        float tmpNextCanRetryTime = 0.0f;
        do
        {
            StopCoroutine("TimeoutRoutine");
            StartCoroutine("TimeoutRoutine", wwwData);
            Debug.Log("没有加密协议222----" + tmpHttpEncode);
            Debug.Log("转义字符串--" + System.Uri.EscapeUriString(tmpHttpEncode));

            wwwData.www = new WWW(System.Uri.EscapeUriString(tmpHttpEncode));
            mMessageCount++;
//            Debug.Log("send message succuss. wwwData.url = " + wwwData.url + "\n wwwData.context = " + wwwData.context + "\n messagecount = " + mMessageCount + (tmpRetryCount > 0 ? ("\n retryCount = " + tmpRetryCount) : ""));
            DEBUG.Log("send message succuss. wwwData.url = " + wwwData.url + "\n wwwData.context = " + wwwData.context + "\n messagecount = " + mMessageCount + (tmpRetryCount > 0 ? ("\n retryCount = " + tmpRetryCount) : ""));
            yield return wwwData.www;
            mMessageCount--;
            //DEBUG.Log("get message succuss. wwwData.www.text = " + wwwData.www.text + "\n messagecount = " + mMessageCount + (tmpRetryCount > 0 ? ("\n retryCount = " + tmpRetryCount) : ""));
            if (CheckConnectValidInfo(wwwData.www.error))
                break;
            if (tmpRetryCount == 0)
                tmpNextCanRetryTime = Time.realtimeSinceStartup + msCanRetryTimeDelta;
            tmpRetryCount += 1;
        } while (tmpRetryCount <= msRetryCount);
        if (tmpRetryCount > msRetryCount && tmpNextCanRetryTime > Time.realtimeSinceStartup)
            yield return new WaitForSeconds(tmpNextCanRetryTime - Time.realtimeSinceStartup);

        if (mMessageCount == 0)
        {
            StopCoroutine("TimeoutRoutine");
            Net.StopWaitEffect();
        }

        //end
        Debug.Log("没有加密协议111----" + wwwData.www.text);
        if (wwwData.www != null && wwwData.www.text != "")
        {
            //by chenliang
            //begin

//            RespMessage respMessage = JCode.Decode<RespMessage>(wwwData.www.text);
//-------------------
            Debug.Log("没有加密协议444----" + wwwData.www.text);
            RespMessage respMessage = null;
            //Http解密
//            string tmpHttpDecode = m_HttpCrypto.Decode(wwwData.www.text);
//            if (tmpHttpDecode == HttpCryptoError.CANOT_VERIFY_DATA)
//                tmpHttpDecode = wwwData.www.text;
//            else if (tmpHttpDecode == HttpCryptoError.VERIFY_FAILED)
//            {
//                GameCommon.OpenToLoginWindow(STRING_INDEX.ERROR_HTTP_PACKAGE_INDEX_REPEAT, null);
//                yield break;
//            }
            string tmpHttpDecode = wwwData.www.text;
			Debug.Log("没有加密协议555----" + tmpHttpDecode);
            DEBUG.Log("get message succuss(decode). wwwData.www.text = " + tmpHttpDecode + "\n messagecount = " + mMessageCount + (tmpRetryCount > 0 ? ("\n retryCount = " + tmpRetryCount) : ""));
            respMessage = JCode.Decode<RespMessage>(tmpHttpDecode);

            //end
            if (respMessage.ret != 1)
            {
                //by chenliang
                //begin

//                 if (-4 == respMessage.ret)
//                 {
//                     DataCenter.OpenWindow(UIWindowString.connectError);
//                 }
//------------------------
                if (tmpRetryCount > 0)
                {
                    StopCoroutine("TimeoutRoutine");
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_REQUEST_ERROR, () =>
                    {
#if !UNITY_EDITOR && !NO_USE_SDK
                        U3DSharkEventListener.LogoutCallback = GlobalModule.ChangeAccount;
                        U3DSharkSDK.Instance.Logout();
#else
                        GlobalModule.ChangeAccount();
#endif
                    });
                    yield break;
                }

                if (-4 == respMessage.ret)
                {
                    //                     DataCenter.OpenWindow(UIWindowString.connectError);
                    //                     DataCenter.SetData(UIWindowString.connectError, "WINDOW_CONTENT", STRING_INDEX.ERROR_NET_GAME_TOKEN_INVALID);
                    //此时为账号在别处登录，返回登录界面
//                    if (GameObject.Find("connect_error_tologin_window") == null)
                        DataCenter.OpenWindow("CONNECT_ERROR_TOLOGIN_WINDOW");
//                    DataCenter.SetData("CONNECT_ERROR_TOLOGIN_WINDOW", "WINDOW_CONTENT", STRING_INDEX.ERROR_NET_LOGIN_BY_OTHER);
                    DataCenter.SetData("CONNECT_ERROR_TOLOGIN_WINDOW", "WINDOW_CONTENT", STRING_INDEX.ERROR_NET_KICKOFF_BY_SERVER);
                    DataCenter.SetData("CONNECT_ERROR_TOLOGIN_WINDOW", "RE_LOGIN_BTN_TEXT", "确定");
                    Action tmpRelogin = () =>
                    {
#if !UNITY_EDITOR && !NO_USE_SDK
                        U3DSharkEventListener.LogoutCallback = HotUpdateLoading.ReloadHotUpdateScene;
                        U3DSharkSDK.Instance.Logout();
#else
                        HotUpdateLoading.ReloadHotUpdateScene();
#endif
                    };
                    DataCenter.SetData("CONNECT_ERROR_TOLOGIN_WINDOW", "RE_LOGIN_ACTION", tmpRelogin);
                }
                else if (respMessage.ret == -3)
                {
                    //登录服务器网络令牌过期
                    //                     DataCenter.OpenWindow("CONNECT_ERROR_TOLOGIN_WINDOW");
                    //                     DataCenter.SetData("CONNECT_ERROR_TOLOGIN_WINDOW", "WINDOW_CONTENT", STRING_INDEX.ERROR_NET_LOGIN_TOKEN_INVALID);
#if !UNITY_EDITOR && !NO_USE_SDK
                    if (CommonParam.isUseSDK)
                    {
//                        if (GameObject.Find("connect_error_tologin_window") == null)
                            DataCenter.OpenWindow("CONNECT_ERROR_TOLOGIN_WINDOW");
                        DataCenter.SetData("CONNECT_ERROR_TOLOGIN_WINDOW", "WINDOW_CONTENT", STRING_INDEX.ERROR_NET_LOGIN_TOKEN_INVALID);
                        //不自动登录
//                         GlobalModule.DoOnNextUpdate(() =>
//                         {
//                             GlobalModule.DoLater(
//                                 () =>
//                                 {
//                                     U3DSharkSDK.Instance.Login();
//                                 }, 0.1f);
//                         });
                    }
                    else
#endif
                    {
                        //自动登录，之后重新发送之前的协议
                        LoginNet.CheckAndRequestLoginNoUI(() =>
                        {
                            if (wwwData != null && wwwData.ReSend != null)
                                wwwData.ReSend();
                        }, null);
                        DEBUG.LogError("error is " + respMessage.ret.ToString() + ". Relogin no ui.");
                    }
                    //直接返回
                    yield break;
                }
                else if (respMessage.ret == -10000)
                {
                    //                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_HOTUPDATE_VERSION_TOO_LOW);
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_HOTUPDATE_VERSION_TOO_LOW, () =>
                    {
#if !UNITY_EDITOR && !NO_USE_SDK
                        U3DSharkEventListener.LogoutCallback = HotUpdateLoading.ReloadHotUpdateScene;
                        U3DSharkSDK.Instance.Logout();
#else
                        HotUpdateLoading.ReloadHotUpdateScene();
#endif
                    });
                    yield break;
                }
                else if (respMessage.ret == -10001)
                {
                    //包序号重复
                    StaticDefine.useHeartbeat = false;
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_HTTP_PACKAGE_INDEX_REPEAT, () =>
                    {
#if !UNITY_EDITOR && !NO_USE_SDK
                        U3DSharkEventListener.LogoutCallback = GlobalModule.ChangeAccount;
                        U3DSharkSDK.Instance.Logout();
#else
                        GlobalModule.ChangeAccount();
#endif
                    });
                    yield break;
                }
                /*Temp+++++++++++++++++++++++++++++++++++++++++*/
                else if (respMessage.ret == 1314)
                {
                    DataCenter.OpenMessageWindow("不能弹劾会长");
                }
                else if (respMessage.ret == -7)
                {
                    DataCenter.OpenMessageWindow("外挂被检测");
                }
                else if (respMessage.ret == -1)
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SERVER_INBUSY);
                }

                //end
                else
                {
                    if (DataCenter.mRetCode.GetData(respMessage.ret, "INFO")!=string.Empty)
                    {
                        DataCenter.OpenMessageWindow(DataCenter.mRetCode.GetData(respMessage.ret, "INFO"));
                    }
                    
                }
                DEBUG.LogError("error is " + respMessage.ret.ToString());
                
                wwwData.failMethod(respMessage.ret.ToString());
                wwwData.failed = true;
            }
            else if (respMessage.ret == 1)
            {
                //DEBUG.Log("request ok" + wwwData.www.text);
                //by chenliang
                //begin

//                 if (mRespListener != null)
//                 {
//                     bool reset = mRespListener(respMessage.pt, wwwData.successMethod, wwwData.www.text);
// 
//                     if (reset)
//                     {
//                         mRespListener = null;
//                     }
//                 }
//                 else
//                 {
//                     wwwData.successMethod(wwwData.www.text);
//                 }
//---------------------------
                if (mRespListener != null)
                {
                    bool reset = mRespListener(respMessage.pt, wwwData.successMethod, tmpHttpDecode);

                    if (reset)
                        mRespListener = null;
                }
                else
                    wwwData.successMethod(tmpHttpDecode);

                //end
            }
        }
        else
        {
            //by chenliang
            //begin

            if (wwwData.www != null && wwwData.www.error != null && wwwData.www.error != "")
            {
                string tmpWWWError = wwwData.www.error;
                if (tmpWWWError.IndexOf("500 ") != -1)          //显示500错误
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SERVER_500);
                else if (tmpWWWError.IndexOf("404 ") != -1)     //显示404错误
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SERVER_404);
                else if (tmpWWWError.IndexOf("900") != -1)
                    GameCommon.OpenToLoginWindow(STRING_INDEX.ERROR_HTTP_PACKAGE_INDEX_REPEAT, null);
                else if (!CheckConnectValidInfo(tmpWWWError))
                {
//                     DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SERVER_CANOT_CONNECT_TO_HOST, () =>
//                     {
//                         if (wwwData.ReSend != null)
//                             wwwData.ReSend();
//                     });
//                     DataCenter.SetData("MESSAGE", "SET_PANEL_DEPTH", 300);

                   DataCenter.OpenWindow("CONNECT_ERROR_TOLOGIN_WINDOW");
                   DataCenter.SetData("CONNECT_ERROR_TOLOGIN_WINDOW", "WINDOW_CONTENT", STRING_INDEX.ERROR_NET_KICKOFF_BY_SERVER);
                   DataCenter.SetData("CONNECT_ERROR_TOLOGIN_WINDOW", "RE_LOGIN_BTN_TEXT", "确定");
                   Action tmpRelogin = () =>
                   {
#if !UNITY_EDITOR && !NO_USE_SDK
                       U3DSharkEventListener.LogoutCallback = HotUpdateLoading.ReloadHotUpdateScene;
                       U3DSharkSDK.Instance.Logout();
#else
                       HotUpdateLoading.ReloadHotUpdateScene();
#endif
                   };
                   DataCenter.SetData("CONNECT_ERROR_TOLOGIN_WINDOW", "RE_LOGIN_ACTION", tmpRelogin);
                }
			}

            //end
            DEBUG.LogError("request error" + wwwData.www.text);
        }
	}

    private IEnumerator TimeoutRoutine(WWWData wwwData)
    {
        yield return new WaitForSeconds(150.0f);

        mMessageCount--;
        if (!wwwData.failed)
        {
            if (mMessageCount == 0)
            {
                StopCoroutine("GetRoutine");
            }
            Net.StopWaitEffect();
            wwwData.failed = true;

            wwwData.failMethod(string.Empty);
            //by chenliang
            //begin

//            DataCenter.OpenWindow(UIWindowString.connectError);
//------------------
            OpenTokenErrorWindow(() =>
            {
//                 if (wwwData.ReSend != null)
//                     wwwData.ReSend();
            });

            //end
            //StopCoroutine("GetRoutine");
        }
    }
    //by chenliang
    //begin

    public static void OpenTokenErrorWindow(Action closeCallback)
    {
        string tmpWinName = "";
        STRING_INDEX tmpStrIndex = STRING_INDEX.ERROR_NONE;
        if (LoginData.Instance.IsGameTokenValid && LoginData.Instance.IsInGameScene)
        {
            tmpWinName = UIWindowString.connectError;
            tmpStrIndex = STRING_INDEX.ERROR_NET_GAME_TOKEN_INVALID;
        }
        else
        {
            tmpWinName = "CONNECT_ERROR_TOLOGIN_WINDOW";
            tmpStrIndex = STRING_INDEX.ERROR_NET_LOGIN_TOKEN_INVALID;
        }
        if (tmpWinName == "")
            return;
        if (GameObject.Find(tmpWinName) == null)
        {
            DataCenter.OpenWindow(tmpWinName);
        }
        DataCenter.SetData(tmpWinName, "WINDOW_CONTENT", tmpStrIndex);
        DataCenter.SetData(tmpWinName, "CLOSE_CALLBACK", closeCallback);
    }

    //end

}
