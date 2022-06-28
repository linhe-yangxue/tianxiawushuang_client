using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.IO;
using DataTable;
using Logic;
using System.Collections.Generic;
using System.Linq;

//enum LoginMsgType
//{
//    eLoginOk,
//    eLoginVerOld,
//    eLoginVerNew,
//    eNeedUpdateResources,
//    eNoExistResourcesServer,
//};

public enum LOGIN_INFO
{
    eError_NONE,
    eLogin_Succeed,
    eLogin_Version_Too_Old,
    eLogin_Version_NoSet,
    eLogin_SQL_function_Error,
    eLogin_CreateAccount_Succeed,
    eLogin_CreateDBData_Succeed,
    eLogin_CreateDBData_Fail,
    eLogin_Account_NoSet,
    eLogin_Account_NoExist,
    eLogin_Account_Repeat,
    eLogin_DBData_NoExist,
    eLogin_PassWord_Error,
    eLogin_Need_PassWord,
    eLogin_NoExist_GameServer,
    eLogin_World_NoResponse,
    eLogin_World_NoExist,
    eLogin_Resources_Too_Old,
    eLogin_Server_Logic_Error,
    eLogin_NoExist_ResourcesServer,

    eLogin_Login_Fail,
    eLogin_NoResponse_LoginData,
    eLogin_ConnectLoginServer_Fail,
    eLogin_ConnectLoginServer_OverTime,
    eLogin_ConnectGameServer_Fail,
    eLogin_ChangePsw,
    eLogin_ChangePsw_Success,
    eLogin_ChangePsw_Fail,
    eLogin_Other_Login,
    eLogin_Need_Restart_App,
    eLogin_Update_Resources_Succeed,
    eLogin_NETWORK_DISCONNECT,
    eLogin_NETWORK_DISCONNECT_TITLE,
};

public class LoginNet 
{
    static public string msServerName = "";
    static public int msServerState = 0;
    static public int msLevel=0;

    static public string msGameServerIP = "";
    static public int msGameServerPort = 0;
    static public CS_QuestEnterGame msEnterGSEvent;
    static public tEvent msUDPConnectEvent;
	static private string[] accounts = new string[2];


#if !UNITY_EDITOR && !NO_USE_SDK
    public static U3DSharkBaseData sdkData;
#endif

    //-------------------------------------------------------------------------
    // login server regist
	// exports.REGIST_TYPE_QUICK = 0;           // 快速登录方式
	// exports.REGIST_TYPE_CHECK = 1;           // 账号密码验证方式
	// exports.REGIST_TYPE_THIRD = 2;           // 账号密码验证方式
	static public void RequestRegistAccount(string acc, string pw, int registtype, string url) {
		RequestRegistAccount(acc, pw, registtype, url, false);
	}

	static public void RequestRegistAccount(string acc, string pw, int registtype, string url, bool quickLogin) {
		CS_Register cmd = new CS_Register();
		//cmd.channel = "galaxy";
		cmd.registtype = registtype;
		cmd.acc = acc;
		cmd.pw = pw; 
		
		accounts[0] = acc;
		accounts[1] = pw;
		
        //by chenliang
        //begin

//		HttpModule.Instace.SendLoginMessage(cmd, "CS_Register", RequestRegistAccountSuccess, RequestRegistAccountFail);
//------------------
        //统一改用SendGameServerMessage
        HttpModule.Instace.SendGameServerMessage(cmd, "CS_Register", RequestRegistAccountSuccess, RequestRegistAccountFail);

        //end

		// 快速登陆保存登陆tag
		if(quickLogin) {
			PlayerPrefs.SetString("QUICK_LOGIN", "quick_login");
			PlayerPrefs.Save();
		} else {
			if(PlayerPrefs.HasKey("QUICK_LOGIN"))
                PlayerPrefs.DeleteKey("QUICK_LOGIN");
		}
	}

	static public void RequestRegistAccountSuccess(string text) {
		SC_Register scr = JCode.Decode<SC_Register>(text);

		if(scr.ret == (int)STRING_INDEX.ERROR_NONE) {

			// 账号已经存在，则不进入游戏
			if(scr.accExists == 1) {
				DataCenter.OpenMessageWindow("该账号已经存在");
				return;
			}

			// Save account and password md5
			string account = accounts[0];
			string password = accounts[1];
			string md5 = GameCommon.MakeMD5(password);
			GameCommon.SaveUsrLoginDataFromUnity(account, password);
						
			DataCenter.SetData("REGISTRATION_WINDOW", "CREAT_ACCOUNT_SUCCEED", account);

			PlayerPrefs.SetString("LOGIN_TOKEN", scr.regtoken);
			PlayerPrefs.Save();
            //added by xuke
            CommonParam.mIsNewAccount = true;
            //end
			// 注册成功以后登陆服务器
            //by chenliang
            //begin

//			LoginNet.RequestLogin(scr.registtype, scr.regtoken, "channel", account, password, "ip", "mac", "url");
//----------------------
            LoginNet.RequestLogin(scr.registtype, scr.regtoken, "channel", account, password, "ip", "mac", "url", false);
            //DataCenter.OpenWindow(UIWindowString.announce_info);

            //end
		}
		else {
			string error = "err";
			DataCenter.SetData("REGISTRATION_WINDOW", "INFO", error);
		}
	}
	
	static public void RequestRegistAccountFail(string text) {
	}

    //-------------------------------------------------------------------------
    // login server login
    //by chenliang
    //begin

    /// <summary>
    /// 无界面登录，仅数据获取
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="failCallback"></param>
    /// <returns>可以登录返回true，否则返回false</returns>
    public static bool CheckAndRequestLoginNoUI(Action successCallback, Action failCallback)
    {
        string[] r = GameCommon.GetSavedLoginDataFromUnity();
        if (r == null)
            return false;

        string account = r[0];
        string password = r[1];
        if (account == "" || password == "")
            return false;
        if (GlobalModule.usePeerNetwork)
        {
            tEvent evt = Net.StartEvent("CS_QuestEnterGame");
            evt.set("ACCOUNT", account);
            evt.set("PSWORDMD5", password);
            LoginNet.LoginGameServer(evt);
        }
        else
        {
            int loginType = 1;
            string loginToken = "";
            if (PlayerPrefs.HasKey("QUICK_LOGIN"))
            {
                loginType = 0;
                loginToken = PlayerPrefs.GetString("LOGIN_TOKEN");
            }
            RequestLoginNoUI(
                loginType, loginToken, "channel", account, password, "ip", "mac", "url", true,
                successCallback, failCallback);
        }
        return true;
    }
    /// <summary>
    /// 无界面请求登陆
    /// </summary>
    /// <param name="loginType"></param>
    /// <param name="regtoken"></param>
    /// <param name="channel"></param>
    /// <param name="acc"></param>
    /// <param name="pw"></param>
    /// <param name="ip"></param>
    /// <param name="mac"></param>
    /// <param name="url"></param>
    /// <param name="isFromLoadAccount">是否加载本地账号信息后登陆</param>
    public static void RequestLoginNoUI(
        int loginType, string regtoken, string channel, string acc, string pw, string ip, string mac, string url, bool isFromLoadAccount,
        Action successCallback, Action failCallback)
    {
        CS_Login login = new CS_Login();
        login.registtype = loginType;
        login.regtoken = regtoken;
        login.acc = acc;
        login.pw = pw;
        accounts[0] = acc;
        accounts[1] = pw;
        HttpModule.Instace.SendGameServerMessage(
            login, "CS_Login", (x) =>
            {
                __RequestLoginNoUISuccess(isFromLoadAccount, loginType == 0, x);
                if (successCallback != null)
                    successCallback();
            }, (x) =>
            {
                if (failCallback != null)
                    failCallback();
            });
    }
    private static void __RequestLoginNoUISuccess(bool isFromLoadAccount, bool isQuickLogin, string text)
    {
        SC_Login login = JCode.Decode<SC_Login>(text);

        if ((STRING_INDEX)login.ret == STRING_INDEX.ERROR_NONE)
        {
            if (login.accNotExists == 1)
            {
                //账号不存在
                if (isFromLoadAccount)
                    DataCenter.OpenWindow("LOGIN_WINDOW", false);
                else
                    DataCenter.OpenMessageWindow("该账号不存在");
                return;
            }
            else if (login.wrongPw == 1)
            {
                //密码错误
                if (isFromLoadAccount)
                    DataCenter.OpenWindow("LOGIN_WINDOW", false);
                else
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NEED_PASSWORD);
                return;
            }

            //检测是否为快速登录
            if (isQuickLogin)
            {
                PlayerPrefs.SetString("QUICK_LOGIN", "quick_login");
                PlayerPrefs.Save();
            }
            else
            {
                if (PlayerPrefs.HasKey("QUICK_LOGIN"))
                {
                    PlayerPrefs.DeleteKey("QUICK_LOGIN");
                    PlayerPrefs.Save();
                }
            }

            // Save account and password md5
            string account = accounts[0];
            string password = accounts[1];
            GameCommon.SaveUsrLoginDataFromUnity(account, password);
            // add token for game controller
            DataCenter.Set("TOKEN", login.tk);

            CommonParam.mAccount = account;

            CommonParam.mStrLoginToken = login.tk;
            CommonParam.mUId = login.uid;

            //设置登录令牌标志
            LoginData.Instance.IsLoginTokenValid = true;
        }
    }

//	static public void RequestLogin(int loginType, string regtoken, string channel, string acc, string pw, string ip, string mac, string url) {
//------------------------
    /// <summary>
    /// 请求登陆
    /// </summary>
    /// <param name="loginType"></param>
    /// <param name="regtoken"></param>
    /// <param name="channel"></param>
    /// <param name="acc"></param>
    /// <param name="pw"></param>
    /// <param name="ip"></param>
    /// <param name="mac"></param>
    /// <param name="url"></param>
    /// <param name="isFromLoadAccount">是否加载本地账号信息后登陆</param>
    static public void RequestLogin(int loginType, string regtoken, string channel, string acc, string pw, string ip, string mac, string url, bool isFromLoadAccount)
    {

    //end
		CS_Login login = new CS_Login();
		login.registtype = loginType;
        login.regtoken = regtoken;
		//login.channel = "galaxy";
		login.acc = acc;
		login.pw = pw; 
		//login.ip = "10.10.10.10";
		//login.mac = "AABBCCDDEEFF0077";
		accounts[0] = acc;
		accounts[1] = pw;
        //by chenliang
        //begin

//        HttpModule.Instace.SendLoginMessage(login, "CS_Login", RequestLoginSuccess, RequestLoginFail);
//---------------------------
        HttpModule.Instace.SendGameServerMessage(login, "CS_Login", (x) => { RequestLoginSuccess(isFromLoadAccount, loginType == 0, x); }, RequestLoginFail);

        //end
	}

    //by chenliang
    //begin

//	static public void RequestLoginSuccess(string text) {
//---------------------------
    static public void RequestLoginSuccess(bool isFromLoadAccount, bool isQuickLogin, string text)
    {

    //end
		SC_Login login = JCode.Decode<SC_Login>(text);

        if ((STRING_INDEX)login.ret == STRING_INDEX.ERROR_NONE)
        {
            //by chenliang
            //begin

            if(login.accNotExists == 1)
            {
                //账号不存在
                if (isFromLoadAccount)
                    DataCenter.OpenWindow("LOGIN_WINDOW", false);
                else
                    DataCenter.OpenMessageWindow("该账号不存在");
                return;
            }
            else if (login.wrongPw == 1)
            {
                //密码错误
                if (isFromLoadAccount)
                    DataCenter.OpenWindow("LOGIN_WINDOW", false);
                else
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NEED_PASSWORD);
                return;
            }

            //检测是否为快速登录
            if (isQuickLogin)
            {
                PlayerPrefs.SetString("QUICK_LOGIN", "quick_login");
                PlayerPrefs.Save();
            }
            else
            {
                if (PlayerPrefs.HasKey("QUICK_LOGIN"))
                {
                    PlayerPrefs.DeleteKey("QUICK_LOGIN");
                    PlayerPrefs.Save();
                }
            }

            DataCenter.CloseWindow("TOURISTS_LOAD_WINDOW");
            DataCenter.CloseWindow("FIRST_LOAD_WINDOW");

            //end
			DataCenter.CloseWindow("LOGIN_WINDOW");

			{
				DataCenter.Set("ENTER_GS", true);
				
				DataCenter.OpenWindow ("LANDING_WINDOW", false);
				DataCenter.CloseWindow ("SELECT_SERVER_WINDOW");
				DataCenter.SetData("LANDING_WINDOW", "REFRESH", accounts[0]);
			}
			
			// Save account and password md5
			string account = accounts[0];
			string password = accounts[1];
			GameCommon.SaveUsrLoginDataFromUnity(account, password);
			// add token for game controller
			DataCenter.Set("TOKEN", login.tk);

			CommonParam.mAccount = account;

			// RequestLoginHistory(login.tk);
            //by chenliang
            //begin

//             CommonParam.mStrToken = login.tk;
//             CommonParam.mUId = login.uid;
//---------------------
            CommonParam.mStrLoginToken = login.tk;
            CommonParam.mUId = login.uid;
			if (!PlayerPrefs.HasKey ("USER_HAD_AGREE_ID_" + CommonParam.mUId)) {
				//if (!CommonParam.isUserAgree) {
				//CommonParam.isUserAgree = false; //默认不勾选
				DataCenter.OpenWindow ("USER_AGREEMENT_WINDOW");
				//}
				UIImageButton btn = GameObject.Find("LandingPushGoinButton").GetComponent<UIImageButton> ();
				if(btn !=null)
					btn.isEnabled = CommonParam.isUserAgree;
			}
			else{CommonParam.isUserAgree=true;}

            //设置登录令牌标志
            LoginData.Instance.IsLoginTokenValid = true;

            //end
			RequestLoginHistory(login.tk);
		}
	}
	
 	static public void RequestLoginFail(string text) {
 	}
	 
	// login history
	static public void RequestLoginHistory(string token) {

        CS_ZoneList cs=new CS_ZoneList(token);

        //by chenliang
        //begin

//        HttpModule.Instace.SendLoginMessage(cs,"CS_ZoneList",RequestLoginHistorySuccess,RequestLoginHistoryFail);
//-------------------
        //统一改为SendGameServerMessage
        HttpModule.Instace.SendGameServerMessage(cs, "CS_ZoneList", RequestLoginHistorySuccess, RequestLoginHistoryFail);

        //end
	}

    //added by xuke 获得推荐服务器
    private static InfoOfZone GetRecommendZone(InfoOfZone[] kZoneInfos) 
    {
        if (kZoneInfos == null || kZoneInfos.Length == 0)
            return null;
        InfoOfZone _zoneInfo = kZoneInfos[0];
        for (int k = 0, length = kZoneInfos.Length; k < length; k++) 
        {
            if (kZoneInfos[k].zstate != 4) 
            {
                _zoneInfo = kZoneInfos[k];
                break;
            }
        }
        if (_zoneInfo == null) 
        {
            Logic.EventCenter.Log(LOG_LEVEL.GENERAL,"所有服务器都是隐藏状态");
            return null;
        }
        //1.如果是新建账号，则获取推荐服务器,推荐规则=>先推荐新服,没有则推荐正常,再没有则选择一个火爆
        if (CommonParam.mIsNewAccount)
        {
            for (int i = 1, count = kZoneInfos.Length; i < count; i++)
            {
                // 如果是新区
                if (kZoneInfos[i].zstate == 1)
                {
                    if (_zoneInfo.zstate != 1)
                    {
                        _zoneInfo = kZoneInfos[i];
                    }
                    else if (int.Parse(kZoneInfos[i].zid) > int.Parse(_zoneInfo.zid))
                    {
                        _zoneInfo = kZoneInfos[i];
                    }
                }
                // 如果是正常区
                else if (kZoneInfos[i].zstate == 3 && _zoneInfo.zstate != 1)
                {
                    if (_zoneInfo.zstate != 3)
                    {
                       _zoneInfo = kZoneInfos[i];
                    }
                    else if (int.Parse(kZoneInfos[i].zid) > int.Parse(_zoneInfo.zid))
                    {
                        _zoneInfo = kZoneInfos[i];
                    }
                }
            }
        }
        else
        {
            for (int i = 1; i < kZoneInfos.Length; i++)
            {
                if (kZoneInfos[i].lastTime > _zoneInfo.lastTime)
                    _zoneInfo = kZoneInfos[i];
            }
        }
        return _zoneInfo;
    }
    //end

	public static void RequestLoginHistorySuccess(string text) {
        SC_ZoneList getZoneList=JCode.Decode<SC_ZoneList>(text);

		if(getZoneList.ret == (int)STRING_INDEX.ERROR_NONE) {
			InfoOfZone[] arr = getZoneList.zoneInfos;
            // modified by xuke
            CommonParam.mIsNewAccount = true;
            var hisArr = arr.Where(info => info.pname != "").ToArray();
            if (arr.Length > 0)
            {
                DataCenter.Set("ZONE_LIST", arr);          
            }
            if (hisArr.Length > 0) 
            {
                CommonParam.mIsNewAccount = false;
            }
            InfoOfZone lastZone = GetRecommendZone(arr);        
            if (lastZone == null) 
            {
                DEBUG.LogError("没有服务器");
                return;
            }
            //InfoOfZone lastZone=arr[0];
            //for(int i=1;i<arr.Length;i++) {
            //    if(arr[i].lastTime>lastZone.lastTime)
            //        lastZone=arr[i];
            //}
            //end
          

            msServerName=lastZone.zname;
            msServerState=lastZone.zstate;
            msLevel=int.Parse(lastZone.plevel);
            CommonParam.mZoneID=lastZone.zid;
            //by chenliang
            //begin

            CommonParam.mZoneName = lastZone.zname;
            CommonParam.mZoneState = lastZone.zstate;

            //end
			if (!PlayerPrefs.HasKey ("USER_HAD_AGREE_ID_" + CommonParam.mUId)) {
				//if (!CommonParam.isUserAgree) {
				//CommonParam.isUserAgree = false; //默认不勾选
				DataCenter.OpenWindow ("USER_AGREEMENT_WINDOW");
				//}
				UIImageButton btn = GameObject.Find("LandingPushGoinButton").GetComponent<UIImageButton> ();
				if(btn !=null)
					btn.isEnabled = CommonParam.isUserAgree;
			}
			else{CommonParam.isUserAgree=true;}
            tWindow landingWindow=DataCenter.GetData("LANDING_WINDOW") as tWindow;
            var go=landingWindow.mGameObjUI;
            GameCommon.SetUIText(go,"server_name",msServerName);
            SetServerStateText(GameCommon.FindObject(go,"server_state"),msServerState);

            DataCenter.SetData("LANDING_WINDOW", "REFRESH_ZONE_STATE", false);
		}
	}

	static void RequestLoginHistoryFail(string text) {
		// request err
		DEBUG.Log("history:"+text);
	}

	

    //-------------------------------------------------------------------------
    // game server login
    static public void RequestGameServerLogin()
    {
        CS_GameServerLogin gameServerLogin = new CS_GameServerLogin();
        //gameServerLogin.zid=CommonParam.mZoneID;
        gameServerLogin.pt = "CS_GameServerLogin";
        NetManager.StartWaitResp(gameServerLogin, RequestGameServerLoginSuccess, RequestGameServerLoginFail);
        //HttpModule.Instace.SendGameServerMessage(gameServerLogin, RequestGameServerLoginSuccess, RequestGameServerLoginFail);
    }

    static public void RequestGameServerLoginFail(string text)
    {
        DEBUG.Log("RequestGameServerLoginFail:" + text);
    }

    static public void RequestGameServerLoginSuccess(string text)
    {
        DEBUG.Log("RequestGameServerLoginSuccess:text = " + text);
        SC_GameServerLogin gameServerLogin = JCode.Decode<SC_GameServerLogin>(text);
		CommonParam.mOpenDate = gameServerLogin.openDate;
        if (PlayerPrefs.HasKey("MYLASTLOGINID"))
        {
            if ((gameServerLogin.zid+gameServerLogin.zuid)!= PlayerPrefs.GetString("MYLASTLOGINID"))
            {
                PVEStageBattle.mBattleSpeed = BATTLE_SPEED.X1;
            }
        }
        PlayerPrefs.SetString("MYLASTLOGINID", gameServerLogin.zid + gameServerLogin.zuid);
        PlayerPrefs.Save();
		string tmp = gameServerLogin.zuid;
		string[] stmp =tmp.Split(':');
		string mtmp = stmp[1];
		PlayerPrefs.SetString ("USER_HAD_AGREE_ID_" + mtmp,"whetherIsAgreeUser");
        //by chenliang
        //begin

//        CommonParam.mStrToken = gameServerLogin.gtk;
//
//        StaticDefine.useHeartbeat = true;
//
//-----------------
        if (gameServerLogin.isSeal == 1)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_DENY_ACCOUNT_LOGIN);
            return;
        }
        CommonParam.mStrToken = gameServerLogin.gtk;

        // add by LC
        CommonParam.mUId = gameServerLogin.zuid;
        CommonParam.mZoneID = gameServerLogin.zid;
        
        StaticDefine.useHeartbeat = true;

        //设置游戏令牌标志
        LoginData.Instance.IsGameTokenValid = true;

        //读取本地存的各系统状态
        SystemStateManager.ReadAllStates();

		//Chat.
//		ChatManager.Instace.Init();
//		ChatManager.Instace.StartConnect(CommonParam.ChatServerIP, int.Parse(CommonParam.ChatServerPort));
        GlobalModule.DoCoroutine(Chat_GetWorldChatCnt_Requester.StartRequest());

        //end
		
        // request role data
        RequestRoleData();
        TaskStarManager.TaskStarInit();

        //added by xuke
        NetManager.RequestActivitiesEndTime();
        //end

    }   
    //-------------------------------------------------------------------------
    // get server data
    /// <summary>
    /// 获得指定服务器的状态
    /// </summary>
    /// <param name="kZoneID"></param>
    static public void RequestServerState() 
    {
        CS_GetZoneState _request = new CS_GetZoneState() { };
        if(!string.IsNullOrEmpty(_request.tk))
            HttpModule.Instace.SendGameServerMessage(_request,RequestServerStateSuccess,RequestServerStateFail);
    }
    static public void RequestServerStateFail(string text)
    {
        EnterGameWindow.mIsNeedStop = true;
        if (string.IsNullOrEmpty(text))
            return;
    }

    static public void RequestServerStateSuccess(string text) 
    {
        SC_GetZoneState _receive = JCode.Decode<SC_GetZoneState>(text);
        LoginNet.msServerState = _receive.state;
        DataCenter.SetData("LANDING_WINDOW","REFRESH", null);
        //added by xuke 请求成功后再继续发请求进行刷新
        DataCenter.SetData("LANDING_WINDOW", "REFRESH_ZONE_STATE_AFTER_SUCCESS", null);
        //end
    }
    //-------------------------------------------------------------------------
    // 分页获得服务器信息列表
    static public void RequestPageZoneList(int kPageIndex,int kUnitCount) 
    {
        CS_PageZoneList _request = new CS_PageZoneList();
        _request.page = kPageIndex;
        _request.cnt = kUnitCount;
        HttpModule.Instace.SendGameServerMessage(_request, RequestGetPageListSuccess, RequestGetPageListFail);
    }
    static public void RequestGetPageListFail(string text) 
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestGetPageListSuccess(string text) 
    {
        SC_PageZoneList _receive = JCode.Decode<SC_PageZoneList>(text);
        DataCenter.SetData("SELECT_SERVER_WINDOW", "REFRESH_SERVER_LIST",_receive);
    }


    //-------------------------------------------------------------------------
    // get role data
    static public void RequestRoleData()
    {
        CS_GetPlayerData getPlayerData = new CS_GetPlayerData();
        getPlayerData.pt = "CS_GetPlayerData";
        NetManager.StartWaitResp(getPlayerData, RequestRoleDataSuccess, RequestRoleDataFail);
    }

    static public void RequestRoleDataFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (Convert.ToInt32(text)== (int)STRING_INDEX.ERROR_PLAYER_NOT_EXIST)
        {
        }
        return;
    }

    static public void RequestRoleDataSuccess(string text)
    {
        DEBUG.Log("RequestRoleDataSuccess:text = " + text);
		SC_GetPlayerData getPlayerData = JCode.Decode<SC_GetPlayerData>(text);
		CommonParam.mCreateDate = getPlayerData.createDate;
        if (CS_RequestRoleData.ReadRoleFromData(text))
        {
            CommonParam.isTeamManagerInit = false;

            //by chenliang
            //begin

            //初始化聊天数据
            ChatWorldWindow.InitData();
            ChatUnionWindow.InitData();
            ChatPrivateWindow.InitData();

            //检查体力、精力、降魔令恢复
            RoleInfoTimerManager.Instance.CheckAndStartRecover(ROLE_INFO_TIMER_TYPE.STAMINA, false);
            RoleInfoTimerManager.Instance.CheckAndStartRecover(ROLE_INFO_TIMER_TYPE.SPIRIT, false);
            RoleInfoTimerManager.Instance.CheckAndStartRecover(ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD, false);

            //end
            //int index = RoleLogicData.Self.guideProgress;// respEvent.get("GUIDE_STATE");
            //GuideManager.LoadGuideProcess(index);
            MainProcess.LoadRoleSelScene();

            TotalTaskManager.Init();

            // 战斗失败引导数据初始化
            //BattleFailGuide.Reset();

            // 新手引导初始化
            if (Guide.isOpened)
            {
                Guide.Start();
            }
            else 
            {
                Guide.CommitTerminateIndex();
            }
        }
    }

    //-------------------------------------------------------------------------
    // create role
    static public void RequestCreateRole(int iPlayerModelIndex, string strPlayerName)
    {
        CS_CreatePlayer createPlayer = new CS_CreatePlayer();
        createPlayer.chaModelIndex = iPlayerModelIndex;
        createPlayer.playerName = strPlayerName;
        createPlayer.pt = "CS_CreatePlayer";
        HttpModule.Instace.SendGameServerMessage(createPlayer, RequestCreateRoleSuccess, RequestCreateRoleFail);
    }

    static public void RequestCreateRoleFail(string text)
    {
        //by chenliang
        //begin

//         if (!string.IsNullOrEmpty(text))
//         {
//             STRING_INDEX error = (STRING_INDEX)Convert.ToInt32(text);
//             DataCenter.OpenMessageWindow(error);
//         }
//----------------
        switch (text)
        {
            case "101": DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NAME_ILLEGAL); break;
            case "102": DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NAME_EXIST); break;
            case "103": DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PLAYER_NOT_EXIST); break;
            case "104": DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NAME_TOO_LONG); break;
            case "105": DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PLAYER_ALREADY_EXIST); break;
        }

        //end

		DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "SET_BUTTON_STATE", true);
    }

    static public void RequestCreateRoleSuccess(string text)
    {
        GmWindow.hasOpen = false;
        RequestRoleData();
    }

	// Request Game Server IP PORT
	static public void RequestGameServerInfo(tEvent succeedEvent, string failEvent)
	{
        if (msEnterGSEvent!=null)
        {
            if (msEnterGSEvent.mWaitSendEvent != null)
                msEnterGSEvent.mWaitSendEvent.Finish();

            msEnterGSEvent.Finish();
            msEnterGSEvent = null;
        }
        msGameServerIP = "";
        msGameServerPort = 0;
        
		DataCenter.CloseWindow("TRY_REQUEST_SERVER_IP_WINDOWS");
		GlobalModule.ClearAllWindow();

        DEBUG.Log("RequestGameServerInfo:"+CommonParam.isUseSDK);
#if !UNITY_EDITOR && !NO_USE_SDK
        //弹出公告
        DataCenter.OpenWindow(UIWindowString.announce_info);
        if(CommonParam.isUseSDK) {
            //by chenliang
            //begin

//             U3DSharkSDK.Instance.Login();
//             //停止心跳包
//             StaticDefine.useHeartbeat = false;
//--------------------
//             DEBUG.Log("RequestGameServerInfo - FirstLoadSDKWindow");
//             DataCenter.OpenWindow("FIRST_LOAD_SDK_WINDOW");
            //停止心跳包
            StaticDefine.useHeartbeat = false;
            //不自动登录
//            GlobalModule.DoOnNextUpdate(() =>
//            {
//                GlobalModule.DoLater(
//                    () =>
//                    {
//                        U3DSharkSDK.Instance.Login();
//                    }, 0.1f);
//            });

            //end
        }
//         else
//         {
//             DataCenter.OpenWindow("FIRST_LOAD_WINDOW");
//         }
#else
        //弹出公告
        DataCenter.OpenWindow(UIWindowString.announce_info);
//        DataCenter.OpenWindow("FIRST_LOAD_WINDOW");
#endif
        
		return;

         if (msUDPConnectEvent!=null)
            msUDPConnectEvent.Finish();
        UDP_RequestGameServerInfo evt = Net.StartEvent("UDP_RequestGameServerInfo") as UDP_RequestGameServerInfo;
        msUDPConnectEvent = evt;
       
        evt.mSucceedEvent = succeedEvent;
        evt.mFailEventName = failEvent;
        evt.DoEvent();
    }

    // Request game server ip and port, to download resources need update
    //static public void StartLoginSDK()
    //{
    //    DEBUG.Log("Now Login SDK");
    //    Schedule.Default.StartCoroutine(TryLoginSDKAfterResgisterAPNS(), "TryLoginSDKAfterResgisterAPNS");
    //}

    static public void StartLogin()
    {
        RequestGameServerInfo(null, "Net_ConnectFailEvent");
    }

    static private IEnumerator TryLoginSDKAfterResgisterAPNS()
    {
        while (MyPlaySDK.Variables.registerAPNSResult == 0)
        {
            yield return new WaitForEndOfFrame();
        }

        if (MyPlaySDK.Variables.registerAPNSResult == 1)
        {
            MyPlaySDK.Login();
        }
        else
        {
            DEBUG.LogError("Register APNS failed");
        }

        MyPlaySDK.Variables.registerAPNSResult = 0;
        MyPlaySDK.Variables.initIAPYet = false;
    }

    static public void OnLoginSDKResult(bool successful, string gid, string token)
    {
        if (successful)
        {
            DEBUG.Log("Login succeed: GID = " + gid + ", Token = " + token);
            StartLogin();
        }
        else 
        {
            DEBUG.LogError("Login Failed");
        }      
    }

    //static public void ConnectGameServer(string strIP, int port, tEvent connectFinishEvent)
    //{
    //    msGameServerIP = strIP;
    //    msGameServerPort = port;
    //    msEnterGSEvent = connectFinishEvent as CS_QuestEnterGame;

    //    Net.StartWaitEffect();
    //    Net.StartConnect(strIP, port, false, connectFinishEvent, "Net_CloseEvent", "Net_ConnectGSFailEvent", 3, 6);
    //}

    static public void LoginGameServer(tEvent connectFinishEvent)
    {
        if (msGameServerIP == "" || msGameServerPort <= 0)
        {
            DEBUG.LogWarning("LoginGameServer server ip is null or port is zero > restart game");
            GlobalModule.RestartGame();
            return;
        }

        msEnterGSEvent = connectFinishEvent as CS_QuestEnterGame;

        //Net.StartWaitEffect();
        Net.StartConnect(msGameServerIP, msGameServerPort, false, connectFinishEvent, "Net_CloseEvent", "Net_ConnectGSFailEvent", 5, 6);
 
       // ConnectGameServer(Net.msServerIP, Net.msServerPort, connectFinishEvt);
    }

    static public void OnConnectLoginFail()
    {
        EventCenter.Log(LOG_LEVEL.ERROR, "Net reconnect login fail> " + msGameServerIP + ":" + msGameServerPort.ToString());

        msGameServerIP = "";
        msGameServerPort = 0;

        DEBUG.LogError("Net connect fail, then restart game");
        GlobalModule.RestartGame();
    }
    
    static public void ReconnectGameServer(tEvent waitSendEvent)
    {
        if (IsReconnecting())
            return;

        waitSendEvent.Log("WARN: NET try connect game server > " + msGameServerIP + ":" + msGameServerPort.ToString());
		if (msEnterGSEvent == null)
		{
			GlobalModule.RestartGame();
			return;
		}

        msEnterGSEvent.mWaitSendEvent = waitSendEvent;
        msEnterGSEvent.SetFinished(false);      

        LoginGameServer(msEnterGSEvent);
    }

    static public void OnConnectFail()
    {
        if (IsReconnecting())
            OnReconnectFail();
        else
        {
            EventCenter.Log(LOG_LEVEL.ERROR, "Net connect game server fail > "+msGameServerIP+":"+msGameServerPort.ToString());
        }
    }

    static public void OnReconnectFail()
    {
        EventCenter.Log(LOG_LEVEL.ERROR, "Net reconnect game server fail> " + msGameServerIP + ":" + msGameServerPort.ToString());
        GameCommon.ShowDebugInfo(0.1f, 0.3f, "ERROR: NET Try reconnect fail > " + msGameServerIP + ":" + msGameServerPort.ToString());
    }



    static public bool IsReconnecting()
    {
		return msEnterGSEvent!=null && msEnterGSEvent.mWaitSendEvent != null;
    }

    static public bool IsWaitSendEvent(tEvent checkEvent)
    {
		return msEnterGSEvent!=null && msEnterGSEvent.mWaitSendEvent == checkEvent;
    }

    static public bool ReadLoginServerInfo(out string serverIP, out int serverPort)
    {
        string strFileName = GameCommon.MakeGamePathFileName("server_id.red");
        if (File.Exists(strFileName))
        {
            FileStream f = new FileStream(strFileName, FileMode.Open);
            DataBuffer data = new DataBuffer(4);
            f.Read(data.mData, 0, 4);

            int serverID = -1;
            data.read(out serverID);

            NiceTable t = TableManager.GetTable("ServerList");
			/*
			WWWRequester.Instace.GetSvrList((o)=>{
				string result = (string)o;
				NiceTable t = new NiceTable();
				byte [] br = Encoding.Unicode.GetBytes(result);
				MemoryStream ms = new MemoryStream(br);
				t.LoadTable(ms, Encoding.Unicode);
			});
			*/
			if (t != null)
			{
				DataRecord r = t.GetRecord(serverID);
				if (r != null)
				{
					LoginNet.msServerName = r["NAME"];
					LoginNet.msServerState = r["STATE"];
					serverIP = r["SERVER_IP"];
					serverPort = r["PORT"];
					f.Close();
					return true;
				}
			}
            f.Close();
        }

        //LoginNet.msServerName = "";
        serverIP = "";
        serverPort = 0;
        return false;
    }

	void OnSvrListRequsetFinish(object o) {
		
		string result = (string)o;
		NiceTable t = new NiceTable();
		byte [] br = Encoding.Unicode.GetBytes(result);
		MemoryStream ms = new MemoryStream(br);
		t.LoadTable(ms, Encoding.Unicode);


	}

    static public void WriteServerID(int nServerID)
    {
        string strFileName = GameCommon.MakeGamePathFileName("server_id.red");
        FileStream f = new FileStream(strFileName, FileMode.Create);
        DataBuffer data = new DataBuffer(4);
        data.write(nServerID);
        f.Write(data.mData, 0, 4);
        f.Close();
    }

    static public void SetServerStateText(GameObject stateTextObj)
    {
        SetServerStateText(stateTextObj, msServerState);
    }

    static public void SetServerStateText(GameObject stateTextObj, int serverState)
    {
        if (stateTextObj == null)
        {
            DEBUG.LogError("server state gameobject is null");
            return;
        }
        UILabel lable = stateTextObj.GetComponent<UILabel>();
        if (lable != null)
        {
            switch (serverState)
            {
                case 1:
                    //by chenliang
                    //begin

                   // lable.text = "新服";
//-------------------------
                    lable.text = "推荐";

                    //end
                    lable.color = Color.green;
                    break;
                case 2:
                    lable.text="火爆";
                    lable.color=Color.green;
                    break;
                case 3:
                    lable.text="正常";
                    lable.color=Color.green;
                    break;
                case 4:
                    lable.text = "维护";  //> 隐藏
                    lable.color=Color.green;
                    break;
                case 5:
                    lable.text = "维护";
                    lable.color = Color.green;
                    break;
                case 6:
                    lable.text = "爆满";
                    lable.color = Color.red;
                    break;
            }            
        }
    }
}
// Frist get server connect info, then start connect to server
public class UDP_RequestGameServerInfo : CEvent
{
    UDPNet mUDPLoginNet;
    DataBuffer mRevBuffer = new DataBuffer(128);
    public tEvent mSucceedEvent;
    public tEvent mFailEvent;
    public string mFailEventName;

    DataBuffer mUDPRequestLoginData;
    int mTryCount = 3;

    ///  now resources md5
    static public string GetNowResourcesMD5()
    {
        string fileName = GameCommon.MakeGamePathFileName("resources_md5.txt");
        string resMD5 = "";
        if (File.Exists(fileName))
        {
            try
            {
                FileStream f = new FileStream(fileName, FileMode.Open);
                StreamReader reader = new StreamReader(f, Encoding.Unicode);
                resMD5 = reader.ReadLine();
                f.Close();
            }
            catch { }
        }
        return resMD5;
    }



    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("TOP_MESSAGE_WINDOW", "网络登陆连接...");

        LoginNet.msGameServerIP = "";
        LoginNet.msGameServerPort = 0;

        //string debugIp = TableManager.GetData("Debug", 0, "SERVER_IP");
        //int port = TableManager.GetData("Debug", 0, "PORT");

        string mUDPServerIp = "";
        int mUDPServerPort = 0;

		if (!LoginNet.ReadLoginServerInfo(out mUDPServerIp, out mUDPServerPort))
        {
            DataCenter.CloseWindow("TRY_REQUEST_SERVER_IP_WINDOWS");
            GlobalModule.ClearAllWindow();
//            DataCenter.OpenWindow("SELECT_SERVER_WINDOW");
//			DataCenter.OpenWindow ("READY_REGISTER_WINDOW");
			DataCenter.OpenWindow ("FIRST_LOAD_WINDOW");

            if(Debug.isDebugBuild)
                DataCenter.OpenWindow("TOP_MESSAGE_WINDOW", "无法找到服务器配置信息！！");
            return false;
        }

        var prompMsg = "正在尝试连接到服务器...";


		Log("UDP Start connect game server, then get server IP PORT> [" + mUDPServerIp + ":" + mUDPServerPort.ToString() + "]");

        DataCenter.OpenWindow("TOP_MESSAGE_WINDOW", prompMsg);
       
        Net.StartWaitEffect();
        WaitTime(10);

        mUDPLoginNet = new UDPNet();
		mUDPLoginNet.InitNet(mUDPServerIp, mUDPServerPort);
        //byte[] requestData = BitConverter.GetBytes(StaticDefine.StringID("DJXGame"));
        mUDPRequestLoginData = new DataBuffer(32);
		mUDPRequestLoginData.write(StaticDefine.StringID("DJXGame"));
		mUDPRequestLoginData.writeTwo(CommonParam.ClientVer);
		mUDPRequestLoginData.writeTwo(GetNowResourcesMD5());

		mUDPLoginNet.SendData(mUDPRequestLoginData.mData, 0, mUDPRequestLoginData.tell());
        EventCenter.WaitUpdate(this, "TrySendOnceRequest", null, 2);

        StartUpdate();
        return true;
    }

    public bool TrySendOnceRequest(object p)
    {
        if (GetFinished())
            return false;

        if (--mTryCount < 0)
            return false;

		mUDPLoginNet.SendData(mUDPRequestLoginData.mData, 0, mUDPRequestLoginData.tell());
        DEBUG.LogWarning("===> Try send once request login");

        return true;
    }

    public override bool Update(float t)
    {
        int revSize = mUDPLoginNet._AsynReceiveData(ref mRevBuffer.mData, 0, 128);

        if (revSize < 0)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_LOGIN_CONNECT_SERVER_FAIL);
            WaitTime(2);
            return false;
        }

        if (revSize > 0)
        {
            Finish();
            LoginNet.msUDPConnectEvent = null;
            mRevBuffer.seek(0);

            int loginMsgType = (int)LOGIN_INFO.eLogin_Succeed;
            mRevBuffer.read(out loginMsgType);
            switch ((LOGIN_INFO)loginMsgType)
            {
       		 case LOGIN_INFO.eLogin_Version_Too_Old:
				DataCenter.OpenWindow("MESSAGE", "版本已更新，请至研发部获取新版本");
				GameCommon.ShowDebugInfo(0.1f, 0.3f, "版本已更新，请至研发部获取新版本");
           		 return false;

       		 case LOGIN_INFO.eLogin_Resources_Too_Old:
	            {
	                string resIp;
	                int resPort;
	                if (mRevBuffer.readOne(out resIp))
	                    if (mRevBuffer.read(out resPort))
	                    {
	                        Log("Succeed get Resources Server IP [" + resIp + ":" + resPort.ToString() + "]");
	                        DEBUG.Log("Need update resources, Now download from [" + resIp + ":" + resPort.ToString() + "]");

	                        CS_RequestResourcesList requestConfig = Net.StartEvent("CS_RequestResourcesList") as CS_RequestResourcesList;
	                        //requestConfig.set("RES_NAME", "t_resources");
	                        //requestConfig.mReceiveControl = new ResourcesUpdate();
	                        requestConfig.WaitTime(10);
	                        LoginNet.msGameServerIP = resIp;
	                        LoginNet.msGameServerPort = resPort;
	                        //LoginNet.ConnectGameServer(resIp, resPort, requestConfig);
	                        LoginNet.LoginGameServer(requestConfig);
	                        Finish();
	                        return false;
	                    }

	                OnFail("Rev data error");
	                Finish();

	                return false;
	            }
        	case LOGIN_INFO.eLogin_NoExist_ResourcesServer:
	            {
					DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_LOGIN_NOT_RESOURCES_UPDATE_SERVER);
	                SetFinished(false);
	                WaitTime(6);
	                return false;
	            }
            }

            string ip;
            int port;
            if (mRevBuffer.readOne(out ip))
			{
                if (mRevBuffer.read(out port))
                {
                    Log("Succeed get Game Server IP [" + ip + ":" + port.ToString() + "]");

                    LoginNet.msGameServerIP = ip;
                    LoginNet.msGameServerPort = port;

                    DataCenter.CloseWindow("TOP_MESSAGE_WINDOW");
					
					if(!File.Exists (GameCommon.MakeGamePathFileName ("Account.info")) && !DataCenter.Get ("IS_CHANGE_SERVER"))
					{
						if(DataCenter.Get ("IS_ENTER_REGISTER"))
						{
							DataCenter.OpenWindow ("REGISTRATION_WINDOW", "FIRST_LOAD_WINDOW");
							DataCenter.CloseWindow ("FIRST_LOAD_WINDOW");
							DataCenter.Set ("IS_ENTER_REGISTER", false);
						}
						else if(DataCenter.Get ("ENTER_GAME_QUICKLY"))
						{
							DataCenter.SetData ("FIRST_LOAD_WINDOW", "ENTER_GAME_QUICKLY", true);
							DataCenter.Set ("ENTER_GAME_QUICKLY", false);
						}
						else
						{
							DataCenter.SetData ("LOGIN_WINDOW", "BACK_WINDOW_NAME", "FIRST_LOAD_WINDOW");
							DataCenter.OpenWindow ("LOGIN_WINDOW", false);
							DataCenter.CloseWindow ("FIRST_LOAD_WINDOW");
							DataCenter.CloseWindow("START_GAME_LOADING_WINDOW");
						}
					
						Finish();
						return false;
					}
				
					if(DataCenter.Get ("IS_CHANGE_SERVER"))
					{
						string filePath = GameCommon.MakeGamePathFileName("Account.info");
						NiceTable table = new NiceTable();
						table.LoadBinary(filePath);
						DataRecord r = table.GetRecord(0);
						if (r != null)
						{
							string inputAccout = r["VALUE_1"];
							string psWord = r["VALUE_2"];
							tEvent evt = Net.StartEvent("CS_QuestEnterGame");
							evt.set("ACCOUNT", inputAccout);
							evt.set("PSWORDMD5", psWord);
							LoginNet.LoginGameServer(evt);
						}
	
//						DataCenter.CloseWindow ("SELECT_SERVER_WINDOW");
						DataCenter.Set ("IS_CHANGE_SERVER", false);
					}
					else
	                    MainProcess.LoadLoginScene();
                    //LoginNet.ConnectGameServer(ip, port, mSucceedEvent);
                    Finish();
                    return false;
                }
			}

            OnFail("Rev data error");
            Finish();
            return false;
        }
        return true;
    }

    public override void _OnOverTime()
    {
        OnFail("Over time 20 sec");

        Finish();
        DataCenter.OpenWindow("TOP_MESSAGE_WINDOW", "网络不给力, 尝试切换服务器");
        //DataCenter.OpenMessageWindow("网络不给力, 尝试切换服务器");
        DEBUG.LogWarning("UDP_RequestGameServerInfo over time, then restart game");

//      	GlobalModule.RestartGame();
        GlobalModule.ClearAllWindow();
		DataCenter.OpenWindow("LANDING_WINDOW", false);
        //by chenliang
        //begin

        //设置游戏令牌标志
        LoginData.Instance.IsGameTokenValid = false;
        LoginData.Instance.IsInGameScene = false;

        //停止心跳包
        StaticDefine.useHeartbeat = false;

        //停止走马灯
        //RoleSelBottomLeftWindow.StopRollPlaying();

        //end
    }

    public void OnFail(string info)
    {
        Log("X Fail> get Game Server IP > " + info);

        Finish();
        DataCenter.OpenWindow("TOP_MESSAGE_WINDOW", "连接登陆失败");
        DEBUG.LogWarning("UDP_RequestGameServerInfo fail, then restart game >" + info);

        GlobalModule.RestartGame();

        //if (mFailEvent != null)
        //    mFailEvent.DoEvent();

        //if (mFailEventName != null)
        //{
        //    tEvent evt = Net.StartEvent(mFailEventName);
        //    if (evt != null)
        //        evt.DoEvent();
        //}
    }

    public override bool _OnFinish()
    {
        Net.StopWaitEffect();
        return true;
    }
}

public class Net_ConnectFailEvent : CEvent
{
    public override bool _DoEvent()
    {
        string mIp = get("IP");
        int mPort = get("PORT");
        GameCommon.ShowDebugInfo(0.1f, 0.3f, "ERROR: NET connect fail > "+mIp + ":" + mPort.ToString());
        LoginNet.OnConnectLoginFail();
        return true;
    }
}

public class Net_ConnectGSFailEvent : CEvent
{
    public override bool _DoEvent()
    {
        string mIp = get("IP");
        int mPort = get("PORT");
        GameCommon.ShowDebugInfo(0.1f, 0.3f, "ERROR: NET connect game server fail > " + mIp + ":" + mPort.ToString());

        LoginNet.OnConnectFail();
        WaitTime(3);
        return true;
    }

    public override void _OnOverTime()
    {
        DEBUG.LogWarning("Net_ConnectGSFailEvent over time, then restart game");
        GlobalModule.RestartGame();
    }
}

public class Net_CloseEvent : CEvent
{
    public override bool _DoEvent()
    {
        string mIp = get("IP");
        int mPort = get("PORT");
        GameCommon.ShowDebugInfo(0.1f, 0.3f, "WARN: NET close > " + mIp + ":" + mPort.ToString());

        LoginNet.ReconnectGameServer(Net.mLastSendEvent);
        //WaitTime(3);

        Finish();

        return true;
    }

    //public override void _OnOverTime()
    //{
    //    if (Net.mLastSendEvent != null)
    //    {
    //        Net.mLastSendEvent.DoOverTime();
    //    }
    //    else
    //    {
    //        DEBUG.LogWarning("Net_CloseEvent over time, then restart game");
    //        GlobalModule.RestartGame();
    //    }
    //}
}

public class CS_RequestCreateAccount : tNetEvent
{
    public override bool _DoEvent()
    {
        set("VER", "1.00");
        //set("ACCOUNT", "xxx");
        //set("PASSWORD", "xxx");
        Dump();
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {        
        respEvent.Dump();

        int errorType = respEvent.get("INFOTYPE");
        if (errorType == (int)LOGIN_INFO.eLogin_CreateAccount_Succeed)
        {
//            DataCenter.Set("ENTER_GS", false);
//
//            DataCenter.CloseWindow("LOGIN_WINDOW");
//
//            DataCenter.SetData("LANDING_WINDOW", "REFRESH", true);
//			DataCenter.CloseWindow("LANDING_WINDOW");

//            DataCenter.SetData("SelectRoleUI", "CLOSE", true);

//            DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "OPEN", true);

            // Save account and password md5
            string account = get("ACCOUNT");
            string password = get("PASSWORD");
			GameCommon.SaveUsrLoginDataFromUnity(account, password);

			if(PlayerPrefs.HasKey ("IS_TOURISTS"))
				PlayerPrefs.DeleteKey ("IS_TOURISTS");

			DataCenter.SetData("REGISTRATION_WINDOW", "CREAT_ACCOUNT_SUCCEED", account);
        }
        else
        {
            string error = respEvent.get("ERROR");
			switch ((LOGIN_INFO)errorType)
			{
			case LOGIN_INFO.eLogin_Account_Repeat:
				error = "帐号已经存在";
				break;

			case LOGIN_INFO.eLogin_Need_PassWord:
				error = "需要正确密码";
				break;
			}
			DataCenter.SetData("REGISTRATION_WINDOW", "INFO", error);
        }

        Finish();
    }

    public override bool _OnFinish()
    {
        return true;
    }
}

public class CS_BindGuestUser : tNetEvent
{
	public override void _OnResp(tEvent respEvent)
	{        
		respEvent.Dump();
		
		int errorType = respEvent.get("INFOTYPE");
		if (errorType == (int)LOGIN_INFO.eLogin_CreateAccount_Succeed)
		{
			// Save account and password md5
			string account = get("ACCOUNT");
			string password = get("PASSWORD");
			string md5 = GameCommon.MakeMD5(password);
			string accountFilePath = GameCommon.MakeGamePathFileName("Account.info");
			NiceTable t = new NiceTable();
			t.SetField("INDEX", FIELD_TYPE.FIELD_INT, 0);
			t.SetField("VALUE_1", FIELD_TYPE.FIELD_STRING, 1);
			t.SetField("VALUE_2", FIELD_TYPE.FIELD_STRING, 2);
			DataRecord r = t.CreateRecord(0);
			r.set("VALUE_1", account);
			r.set("VALUE_2", md5);
			t.SaveBinary(accountFilePath);
			
			if(PlayerPrefs.HasKey ("IS_TOURISTS"))
				PlayerPrefs.DeleteKey ("IS_TOURISTS");
			
			DataCenter.SetData("REGISTRATION_WINDOW", "BIND_ACCOUNT_SUCCEED", account);
		}
		else
		{
			string error = respEvent.get("ERROR");
			switch ((LOGIN_INFO)errorType)
			{
			case LOGIN_INFO.eLogin_Account_Repeat:
				error = "帐号已经存在";
				break;
				
			case LOGIN_INFO.eLogin_Need_PassWord:
				error = "需要正确密码";
				break;
			}
			DataCenter.SetData("REGISTRATION_WINDOW", "INFO", error);
		}
	}
}

// Load from record account to enter game
public class LOGIN_TryLoadAccountThenEnterGame : CEvent
{
	bool mbTry = false;
	
	public override bool _DoEvent()
	{
		if (mbTry)
		{
			return false;
        }
        mbTry = true;

		string[] r = GameCommon.GetSavedLoginDataFromUnity();
        if (r != null)
        {
            string account = r[0];
            string password = r[1];
            if (account != "" && password != "")
            {
				if(GlobalModule.usePeerNetwork) {				
	                tEvent evt = Net.StartEvent("CS_QuestEnterGame");
	                evt.set("ACCOUNT", account);
	                evt.set("PSWORDMD5", password);
	                LoginNet.LoginGameServer(evt);
				} else {
					int loginType = 1;
					string loginToken = "";
					if( PlayerPrefs.HasKey("QUICK_LOGIN") ) { 
						loginType = 0;
						loginToken = PlayerPrefs.GetString("LOGIN_TOKEN");
					}
                    //by chenliang
                    //begin

//					LoginNet.RequestLogin(loginType, loginToken, "channel", account, password, "ip", "mac", "url");
//---------------------------
                    LoginNet.RequestLogin(loginType, loginToken, "channel", account, password, "ip", "mac", "url", true);

                    //end
				}
                return true;
            }
        }     

        return false;
    }
}

//public class CS_HeartBeat : BaseNetEvent
//{
//    public override bool _DoEvent()
//    {
//        WaitTime(60);
//        return true;
//    }

//    public override void _OnResp(tEvent respEvt)
//    {
//        return;
//        //Finish();
//    }
//}

public class TM_TickTestNet : CEvent
{
    //tEvent mNetHeartEvent = null;

    public TM_TickTestNet()
    {
        
    }

    public override bool _DoEvent()
    {
        WaitTime(3);
		return true;
    }

    public override void _OnOverTime()
    {
		tEvent mNetHeartEvent = Net.StartEvent("CS_HeartBeat");
        mNetHeartEvent.Send();

        SetFinished(false);
        DoEvent();
    }
}

//login event
public class CS_QuestEnterGame : tNetEvent
{
    static public tEvent mTestTickNetkEvent;

    public tEvent mWaitSendEvent;

    public int INFOTYPE = (int)LOGIN_INFO.eError_NONE;

    public override bool _DoEvent()
    {
        if (mTestTickNetkEvent != null)
        {
            mTestTickNetkEvent.Finish();
            mTestTickNetkEvent = null;
        }
        //set("VER", "1.00");
        this["VER"] = new Data(CommonParam.ClientVer);
        set("RES_MD5", UDP_RequestGameServerInfo.GetNowResourcesMD5());

        Dump();
       
        WaitTime(10);
        return true;
    }

    public override void _OnResp(tEvent respEvent)
    {
        respEvent.Dump();

        LOGIN_INFO result = (LOGIN_INFO)(int)respEvent.get("INFOTYPE");

		int iIsToursist = respEvent.get("GUEST");
		if(iIsToursist != 1 && PlayerPrefs.HasKey ("IS_TOURISTS"))
			PlayerPrefs.DeleteKey ("IS_TOURISTS");

        switch (result)
        {

            case LOGIN_INFO.eLogin_DBData_NoExist:
                {
//                	DataCenter.CloseWindow("LOGIN_WINDOW");
//                  DataCenter.Set("ENTER_GS", false);
//
//                  DataCenter.SetData("LANDING_WINDOW", "REFRESH", true);
//					DataCenter.CloseWindow("LANDING_WINDOW");
//
//                  DataCenter.SetData("SelectRoleUI", "CLOSE", true);
					
					// Save account and password md5
					string account = get("ACCOUNT");
					string password = get("PSWORDMD5");
					string accountFilePath = GameCommon.MakeGamePathFileName("Account.info");
					NiceTable t = new NiceTable();
					t.SetField("INDEX", FIELD_TYPE.FIELD_INT, 0);
					t.SetField("VALUE_1", FIELD_TYPE.FIELD_STRING, 1);
					t.SetField("VALUE_2", FIELD_TYPE.FIELD_STRING, 2);
					DataRecord r = t.CreateRecord(0);
					r.set("VALUE_1", account);
					r.set("VALUE_2", password);
					t.SaveBinary(accountFilePath);

                    CommonParam.mAccount = account;
                    //GuideManager.StartGuide();

					//need first open landing window, then creat role
					if(!DataCenter.Get ("ENTER_GAME_IMMEDIATELY"))
					{
						DataCenter.CloseWindow ("LOGIN_WINDOW");
						DataCenter.CloseWindow ("SELECT_SERVER_WINDOW");
						DataCenter.Set("ENTER_GS", false);
						DataCenter.OpenWindow ("LANDING_WINDOW", false);
//						DataCenter.Set ("ENTER_GAME_IMMEDIATELY", false);
						return;
					}
                  
                    DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "OPEN", true);

                    WaitTime(6);
                    return;
                }
                break;

            case LOGIN_INFO.eLogin_Succeed:
                {
                    /// ??? 心跳包发送
                    //mTestTickNetkEvent = Net.StartEvent("TM_TickTestNet");
                    //mTestTickNetkEvent.DoEvent();

                    DataCenter.CloseWindow("LOGIN_WINDOW");					

                    if (mWaitSendEvent != null)
                    {
                        mWaitSendEvent.DoEvent();
                        mWaitSendEvent = null;
                        return;
                    }

                    bool bEnterImmediately = get("ENTER_GAME");
					bool bEnterGameImmediately = DataCenter.Get ("ENTER_GAME_IMMEDIATELY");
					if (bEnterImmediately || bEnterGameImmediately)
                    {
						DataCenter.Set ("ENTER_GAME_IMMEDIATELY", false);

                        //CS_Game_Server_Login 
                        ///////////////////////////////////////////////////////////////////
                        tEvent questData = Net.StartEvent("CS_RequestRoleData");
                        questData.DoEvent();
                    }
                    else
                    {
                        DataCenter.Set("ENTER_GS", true);

						DataCenter.OpenWindow ("LANDING_WINDOW", false);
						DataCenter.CloseWindow ("SELECT_SERVER_WINDOW");
						DataCenter.SetData("LANDING_WINDOW", "REFRESH", get("ACCOUNT"));
                    }
				
                    UInt64 serverTime = respEvent.get("SERVER_TIME");
                    Log("Now set server time >" + serverTime.ToString());
                    CommonParam.SetServerTime(serverTime);

                    // Save account and password md5
                    string account = get("ACCOUNT");
                    string password = get("PSWORDMD5");
					GameCommon.SaveUsrLoginDataFromUnity(account, password);

                    CommonParam.mAccount = account;
                    //GuideManager.LoadGuideProcess();

                    return;
                }

            case LOGIN_INFO.eLogin_Resources_Too_Old:
                {
                    string resIp = respEvent["RES_IP"];
                    int resPort = respEvent["RES_PORT"];
                    if (resIp != "" && resPort > 0)
                    {
                        Log("Succeed get Resources Server IP [" + resIp + ":" + resPort.ToString() + "]");
                        DEBUG.Log("Need update resources, Now download from [" + resIp + ":" + resPort.ToString() + "]");

                        CS_RequestResourcesList requestConfig = Net.StartEvent("CS_RequestResourcesList") as CS_RequestResourcesList;
                        //requestConfig.set("RES_NAME", "t_resources");
                        //requestConfig.mReceiveControl = new ResourcesUpdate();
                        requestConfig.WaitTime(10);
                        LoginNet.msGameServerIP = resIp;
                        LoginNet.msGameServerPort = resPort;
                        //LoginNet.ConnectGameServer(resIp, resPort, requestConfig);
                        LoginNet.LoginGameServer(requestConfig);
                        Finish();
                        return;
                    }
					DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_LOGIN_NOT_PROVIDE_RESOURCES_SERVER_ADDRESS);
                    WaitTime(6);
                    return;
                }
                break;
            case LOGIN_INFO.eLogin_NoExist_ResourcesServer:
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_LOGIN_NOT_RESOURCES_UPDATE_SERVER);
                    SetFinished(false);
                    WaitTime(6);
                    return;
                }
                break;
        }

//        string filePath = GameCommon.MakeGamePathFileName("Account.info");
//        File.Delete(filePath);

		GlobalModule.ClearAllWindow ();
        DataCenter.OpenWindow("LOGIN_WINDOW", false);
        string error = respEvent.get("ERROR");
        DataCenter.SetData("LOGIN_WINDOW", "INFO", error);

		if(result == LOGIN_INFO.eLogin_PassWord_Error)
		{
			DataCenter.Set("ENTER_GS", false);
			string accountInfo = get("ACCOUNT");
			DataCenter.SetData("LOGIN_WINDOW", "KEEP_ACCOUNT", accountInfo);
		}

    }

    public override void _OnOverTime()
    {
        DEBUG.LogWarning("CS_QuestEnterGame over time, then restart game");
        DataCenter.OpenWindow("TOP_MESSAGE_WINDOW", "无法登陆服务器，即将重新开始游戏...");
        GlobalModule.RestartGame();
    }
}

public class SC_NotifyLoginAgain : DefaultNetEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("LOGIN_WINDOW");
        return true;
    }
}

public class TestServerEvent : tClientEvent
{
    public override bool _DoEvent()
    {
        //return base._DoEvent();
        set("dddd", 8883.3);
        Finish();
        return true;
    }

    public override void SetRespData(ref tEvent evt)
	{

	}
}

public class CS_ZoneList:MessageBase { 
    public readonly string tk;
    public readonly string mac=DeviceBaseData.mac;

    public CS_ZoneList(string tk) {
        this.tk=tk;
    }
}

public class SC_ZoneList:RespMessage {
    public readonly InfoOfZone[] zoneInfos;
}

public class InfoOfZone{
    public readonly string zid;
    public readonly string zname;
    public readonly int zstate;
    public readonly string pname;
    public readonly string plevel;
    public readonly int lastTime;
}




