using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;
using System.Linq;

//-------------------------------------------------------------------------
using System.Text;
using System.IO;


public class Button_change_id_button : CEvent
{
    public override bool _DoEvent()
    {
        Settings.SwitchAccount();
        return true;
    }
}
//-------------------------------------------------------------------------
public class Button_login_window_back : CEvent
{
	public override bool _DoEvent()
	{
		string backWindowName = get ("BACK_WINDOW_NAME").ToString ();
        //by chenliang
        //begin

// 		DataCenter.CloseWindow ("LOGIN_WINDOW");
// 		if(backWindowName == "LANDING_WINDOW")
//  			DataCenter.OpenWindow (backWindowName, false);
// 		else if(backWindowName == "FIRST_LOAD_WINDOW" || backWindowName == "HAD_SELECT_SERVER_WINDOW")
// 			DataCenter.OpenWindow (backWindowName);
// 		else 
// 			DataCenter.OpenWindow ("FIRST_LOAD_WINDOW");
//----------------
        if (backWindowName == "LANDING_WINDOW")
        {
            //此时应该自动登录当前账号
            tEvent tmpEvt = Net.StartEvent("LOGIN_TryLoadAccountThenEnterGame");
            if (tmpEvt != null && tmpEvt.DoEvent())
                return true;
            else
            {
                DataCenter.CloseWindow("LOGIN_WINDOW");
                DataCenter.OpenWindow(backWindowName, false);
            }
        }
        else if (backWindowName == "FIRST_LOAD_WINDOW" || backWindowName == "HAD_SELECT_SERVER_WINDOW")
        {
            DataCenter.CloseWindow("LOGIN_WINDOW");
            DataCenter.OpenWindow(backWindowName);
        }
        else
        {
            DataCenter.CloseWindow("LOGIN_WINDOW");
            DataCenter.OpenWindow("FIRST_LOAD_WINDOW");
        }

        //end
		return true;
	}
}
//-------------------------------------------------------------------------
public class LoginWindow : tWindow
{
	public override void Init()
	{
		//mGameObjUI = GameCommon.FindUI("loginwindow");
        EventCenter.Self.RegisterEvent("Button_button_login", new DefineFactory<Button_button_login>());
        EventCenter.Self.RegisterEvent("Button_button_registration", new DefineFactory<Button_button_registration>());
		EventCenter.Self.RegisterEvent("Button_login_window_back", new DefineFactory<Button_login_window_back>());
	}
	
	public override bool Refresh(object param)
	{
		return true;
	}
	
	public override Data get(string key)
	{
		if (key == "INPUT_ACCOUNT")
		{
			GameObject input = GameCommon.FindObject(mGameObjUI, "input_name");
			UILabel lable = input.GetComponentInChildren<UILabel>();
			return new Data(lable.text);
		}
		else if (key == "INPUT_PS")
		{
			GameObject input = GameCommon.FindObject(mGameObjUI, "input_password");
			UIInput uiInput = input.GetComponentInChildren<UIInput>();
			return new Data(uiInput.value);
		}
		
		return base.get(key);
	}
	
	public override void onChange(string key, object value)
	{
		if (key=="INFO")
		{
			GameObject input = GameCommon.FindObject(mGameObjUI, "error_password");
			UILabel lable = input.GetComponentInChildren<UILabel>();
			if(lable != null)
				lable.text = (string) value;
		}
		if(key == "KEEP_ACCOUNT")
		{
			SetText("input_name_lable", value.ToString ());
		}

		base.onChange(key, value);
	}

	public override void Open(object param)
	{
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
		base.Open (param);
		object obj;
		string backWindowName = "";
		getData ("BACK_WINDOW_NAME", out obj);
		if(obj != null)
			backWindowName = obj.ToString ();
		GameCommon.GetButtonData (GetSub ("login_window_back")).set ("BACK_WINDOW_NAME", backWindowName);

		// load account info
		if((bool)param)
		{
			tEvent evt = Net.StartEvent("LOGIN_TryLoadAccountThenEnterGame");
			if (evt!=null && evt.DoEvent())
			{
				Close();
				return;
			}
		}

		if (mGameObjUI != null)
		{
			TweenScale scale = mGameObjUI.GetComponent<TweenScale>();
			if(scale != null)
			{
				scale.duration = 0.2f;
				scale.method = UITweener.Method.BounceIn;
				scale.PlayForward();
				
				EventDelegate.Add(scale.onFinished, OpenFinish);
			}
		}

        SetText("error_password_label", "");
        SetText("input_name_lable", "");
        SetText("input_password_label", "");
		DataCenter.OpenWindow ("VERSION_NUMBER_WINDOW");
//		SetText ("version_number", CommonParam.ClientVer);

        GetCurUIComponent<UIImageButton>("button_registration").isEnabled=!CommonParam.isCloseBtn;

	}

	public override void Close()
	{
		if (mGameObjUI != null)
		{
			TweenScale scale = mGameObjUI.GetComponent<TweenScale>();
			if(scale != null)
			{
				scale.duration = 0.2f;
				scale.method = UITweener.Method.BounceIn;
				scale.PlayReverse();
				
				EventDelegate.Add(scale.onFinished, CloseFinish);
			}
		}
		base.Close();
	}
	
	public void OpenFinish()
	{
		if (mGameObjUI != null)
		{
			TweenScale scale = mGameObjUI.GetComponent<TweenScale>();
			if(scale != null)
			{
				mGameObjUI.SetActive(true);
				
				scale.RemoveOnFinished(new EventDelegate(OpenFinish));
				scale.ResetToBeginning();
			}
		}
	}
	
	public void CloseFinish()
	{
		if (mGameObjUI != null)
		{
			TweenScale scale = mGameObjUI.GetComponent<TweenScale>();
			if(scale != null)
			{
				mGameObjUI.SetActive(false);
				
				scale.RemoveOnFinished(new EventDelegate(CloseFinish));
				scale.ResetToBeginning();
			}
		}
	}
}

#region loginbutton
public class Button_button_login : CEvent
{
	public override bool _DoEvent()
	{
		GameCommon.CleanALlChatLog ();
		set("NET", false);
		// 发送
		tLogicData loginWin = DataCenter.GetData("LOGIN_WINDOW");
		string inputAccout = loginWin.get("INPUT_ACCOUNT");
		string psWord = loginWin.get("INPUT_PS");
		
		if (inputAccout == "" || psWord == "")
			loginWin.set("INFO", "请输入账号或密码");
		else
		{
			if(GlobalModule.usePeerNetwork) {
				tEvent evt = Net.StartEvent("CS_QuestEnterGame");
				evt.set("ACCOUNT", inputAccout);
				evt.set("PSWORDMD5", GameCommon.MakeMD5(psWord));
				evt.DoEvent();

	            LoginNet.LoginGameServer(evt);
			} else {
                //by chenliang
                //begin

//				LoginNet.RequestLogin(1, "token", "channel", inputAccout, psWord, "ip", "mac", "url");
//--------------------------
                LoginNet.RequestLogin(1, "token", "channel", inputAccout, psWord, "ip", "mac", "url", false);

                //end
			}
		}
		return true;
	}
}
#endregion

public class Button_button_registration : CEvent
{
    public override bool _DoEvent()
    {
		DataCenter.OpenWindow ("REGISTRATION_WINDOW", "LOGIN_WINDOW");
		DataCenter.CloseWindow ("LOGIN_WINDOW");
        return true;
    }
}

//-------------------------------------------------------------------------

//-------------------------------------------------------------------------
public class TryRequestLoginServerWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_try_login_button", new DefineFactory<Button_try_login_button>());
    }

    public override void OnOpen()
    {
        base.OnOpen();

		NiceData buttonData = GameCommon.GetButtonData (GetSub ("change_server_area"));
		buttonData.set ("BACK_WINDOW", "TRY_REQUEST_SERVER_IP_WINDOWS");

        SetVisible("current_server_info", true);
        SetVisible("try_login_button", true);
        SetVisible("LandingPushGoinButton", false);
        SetVisible("change_id_button", false);
		SetVisible("landing_label", false);

        SetText("server_name", LoginNet.msServerName);

        LoginNet.SetServerStateText(GetSub("server_state"));

		DataCenter.OpenWindow ("VERSION_NUMBER_WINDOW");
		string[] r = GameCommon.GetSavedLoginDataFromUnity();
		if (r != null)
		{
			string account = r[0];
			string password = r[1];
			SetText ("id_name", account);
		}
    }
}

public class Button_try_login_button : CEvent
{
    public override bool _DoEvent()
    {
        GlobalModule.ClearAllWindow();
        DataCenter.OpenWindow("START_GAME_LOADING_WINDOW");
        LoginNet.StartLogin();
        return true;
    }
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class SelectServerListWindow : tWindow
{
    UIGridContainer svrHistoryList = null;
    UIGridContainer svrList = null;
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_server_id_info", new DefineFactory<Button_server_id_info>());
        EventCenter.Self.RegisterEvent("Button_close_server_list_button", new DefineFactory<Button_close_server_list_button>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex) 
        {
            case "REFRESH_SERVER_LIST":
                RefreshServerList(objVal);
                break;
        }
    }

    private int mShowCount = 0;
    private void RefreshServerList(object objVal) 
    {
        if (!(objVal != null && objVal is SC_PageZoneList))
            return;
        InfoOfZone[] _zoneArrInfo = ((SC_PageZoneList)objVal).zoneInfos;
        //UIGridContainer svrList = GetSub("server_info_group").GetComponent<UIGridContainer>();
        int _preMaxCount = svrList.MaxCount;
        svrList.MaxCount = _preMaxCount + _zoneArrInfo.Length;
        mShowCount += _zoneArrInfo.Length;
        if (_zoneArrInfo == null || _zoneArrInfo.Length < svrList.unitLoadCount)
        {
            svrList.mIsAllLoaded = true;
        }

        //历史服务器
        InfoOfZone[] zoneArr = (InfoOfZone[])DataCenter.Get("ZONE_LIST").mObj;
        var list = zoneArr.Where(info => info.lastTime != 0).ToList();
        list.Sort((l, r) =>
        {
            return (l.lastTime > r.lastTime) ? -1 : 1;
        });
        InfoOfZone[] historyZoneArr = list.ToArray();
        //所有服务器
        for (int i = 0, count = _zoneArrInfo.Length; i < count; i++)
        {
            var info = _zoneArrInfo[i];
            var grid = svrList.controlList[_preMaxCount + i];
            grid.name = "server_id_info";
            GameCommon.SetUIText(grid, "server_name", info.zname);
            LoginNet.SetServerStateText(GameCommon.FindObject(grid, "server_state"), info.zstate);
            NiceData butData = GameCommon.GetButtonData(grid);
            if (butData != null)
            {
                butData.set("SERVER_ID", info.zid);
                butData.set("SERVER_NAME", info.zname);
                butData.set("STATE", info.zstate);
                butData.set("MAIN_WINDOW", this);
            }
            else DEBUG.LogError("No add button event");
            //更新历史服务器状态
            __UpdateHistoryUI(historyZoneArr,info);
            if (info.zstate == 4) 
            {
                mShowCount--;
                grid.SetActive(false);
            }
        }
        svrList.Reposition();
        //设置服务器列表背景图的高度
        GameObject _serverRoot = GetSub("server_root");
        UISprite _bg_server_sprite = GameCommon.FindObject(_serverRoot, "bg_server").GetComponent<UISprite>();
        int _bg_begin_height = 36;
        int _server_row_count = (mShowCount + 1) / 2;
        _bg_server_sprite.height = _server_row_count * (int)svrList.CellHeight + _bg_begin_height;
    }

    /// <summary>
    /// 根据最新的服务器状态刷新历史服务器状态
    /// </summary>
    /// <param name="kHistoryZoneArr">历史服务器</param>
    /// <param name="kZoneInfo">服务器状态</param>
    private void __UpdateHistoryUI(InfoOfZone[] kHistoryZoneArr,InfoOfZone kZoneInfo) 
    {
        if (kHistoryZoneArr == null)
            return;
        if (kZoneInfo.lastTime == 0)
            return;
        for (int i = 0, count = kHistoryZoneArr.Length; i < count; i++) 
        {
            InfoOfZone _historyZoneInfo = kHistoryZoneArr[i];
            if (_historyZoneInfo.zid == kZoneInfo.zid) 
            {
                GameObject item = svrHistoryList.controlList [i];

				//GameCommon.SetUIText (item, "server_name", _historyZoneInfo.zname);
				//GameCommon.SetUIText (item, "my_name_in_this_server", _historyZoneInfo.pname);
				//GameCommon.SetUIText (item, "my_level_in_this_server", "Lv."+_historyZoneInfo.plevel);
				LoginNet.SetServerStateText (GameCommon.FindObject (item, "server_state"), _historyZoneInfo.zstate);
				item.name = "server_id_info";

				NiceData butData = GameCommon.GetButtonData (item);
                if (butData != null)
                {
                   // butData.set("SERVER_ID", _historyZoneInfo.zid);
                   // butData.set("SERVER_NAME", _historyZoneInfo.zname);
                    butData.set("STATE", _historyZoneInfo.zstate);
                   // butData.set("MAIN_WINDOW", this);
                }
            }
        }
    }
    public override void OnOpen()
    {
		base.OnOpen ();

//		SetText ("version_number", CommonParam.ClientVer);
		DataCenter.OpenWindow ("VERSION_NUMBER_WINDOW");
		string strBackWindow = get ("BACK_WINDOW");
		NiceData buttonData = GameCommon.GetButtonData (GetSub ("close_server_list_button"));
		buttonData.set ("BACK_WINDOW", strBackWindow);

		SetText ("select_server_name", LoginNet.msServerName);
		//SetText("select_server_state", LoginNet.msServerState);
		LoginNet.SetServerStateText (GetSub ("select_server_state"));

		InfoOfZone[] zoneArr = (InfoOfZone[])DataCenter.Get ("ZONE_LIST").mObj;
        var list=zoneArr.Where(info => info.lastTime!=0).ToList();
        list.Sort((l,r) => {
            return (l.lastTime>r.lastTime)?-1:1;
        });

        InfoOfZone[] historyZoneArr=list.ToArray();

        //historyZoneArr.ToList().Sort
	
		GameObject svrHistoryName = GetSub ("used_server_label");
		svrHistoryList = GetSub ("have_used_server_info_group").GetComponent<UIGridContainer> ();
		GameObject svrListName = GetSub ("server_label");
	    svrList = GetSub ("server_info_group").GetComponent<UIGridContainer> ();
        
        //added by xuke
        UIScrollView _scrollView = GameCommon.FindComponent<UIScrollView>(mGameObjUI, "server_info_group_scroll_view");
        if (_scrollView != null)
            _scrollView.ResetPosition();
        svrList.RegisterDynamicLoadHandler(() => 
        {
            //根据服务器总数和当前加载数量去请求服务器信息
            LoginNet.RequestPageZoneList(svrList.MaxCount / svrList.unitLoadCount, svrList.unitLoadCount);
        });
        //end

		if (historyZoneArr != null) {
			svrHistoryList.MaxCount = historyZoneArr.Length;

			for (int i=0; i<historyZoneArr.Length; i++) {
				var zoneInfo = historyZoneArr [i];
				GameObject item = svrHistoryList.controlList [i];

				GameCommon.SetUIText (item, "server_name", zoneInfo.zname);
				GameCommon.SetUIText (item, "my_name_in_this_server", zoneInfo.pname);
				GameCommon.SetUIText (item, "my_level_in_this_server", "Lv."+zoneInfo.plevel);
				LoginNet.SetServerStateText (GameCommon.FindObject (item, "server_state"), zoneInfo.zstate);
				item.name = "server_id_info";

				NiceData butData = GameCommon.GetButtonData (item);
				if (butData != null) {
					butData.set ("SERVER_ID", zoneInfo.zid);
					butData.set ("SERVER_NAME", zoneInfo.zname);
					butData.set ("STATE", zoneInfo.zstate);
					butData.set ("MAIN_WINDOW", this);
				} else
					DEBUG.LogError ("No add button event");
			}
		}

		//added by xuke begin
		GameObject _usedServerRoot = GetSub ("used_server_root");
		GameObject _serverRoot = GetSub ("server_root");

		UISprite _bg_used_server_sprite = GameCommon.FindObject (_usedServerRoot, "bg_server_history").GetComponent<UISprite> ();
		UISprite _bg_server_sprite = GameCommon.FindObject (_serverRoot, "bg_server").GetComponent<UISprite> ();

		float _used_Server_Cell_Height = svrHistoryList.CellHeight;
		float _server_Cell_Height = svrList.CellHeight;
		int _bg_begin_height = 36;
		int _used_row_count = (historyZoneArr.Length + 1) / 2;
		int _server_row_count = (zoneArr.Length + 1) / 2;
		float _server_root_begin_posY = 140f;
		// 如果没有登录过的服务器
		if (svrHistoryList.MaxCount == 0) {
			_usedServerRoot.SetActive (false);
			_serverRoot.transform.localPosition = new Vector3 (0f, _server_root_begin_posY, 0f);
		} 
		else 
		{
			//设置已登录服务器列表背景图高度
			_bg_used_server_sprite.height =  _used_row_count * (int)svrHistoryList.CellHeight + _bg_begin_height;
			//设置服务器列表的位置
			float _used_bg_bottom_posY = _bg_used_server_sprite.transform.localPosition.y + _bg_used_server_sprite.height;
			_serverRoot.transform.localPosition = new Vector3(0f,-1 * (_used_row_count - 1) * _used_Server_Cell_Height ,0f);
		}
		//设置服务器列表背景图的高度
		_bg_server_sprite.height = _server_row_count * (int)svrList.CellHeight + _bg_begin_height;
		//end

        svrList.MaxCount = 0;
        svrList.mIsAllLoaded = false;
        mShowCount = 0;
        if (svrList.unitLoadCount == 0)
        {
            DEBUG.LogError("unitLoadCount是0，0不能作为除数");
        }
        else 
        {
            LoginNet.RequestPageZoneList(svrList.MaxCount / svrList.unitLoadCount, svrList.unitLoadCount);
        }
       
        //svrList.MaxCount=zoneArr.Length;

        //for(int i=0;i<zoneArr.Length;i++) {
        //    var info=zoneArr[i];
        //    var grid=svrList.controlList[i];
        //    grid.name="server_id_info";
        //    GameCommon.SetUIText(grid,"server_name",info.zname);
        //    LoginNet.SetServerStateText(GameCommon.FindObject(grid,"server_state"),info.zstate);
        //    NiceData butData=GameCommon.GetButtonData(grid);
        //    if(butData!=null) {
        //        butData.set("SERVER_ID",info.zid);
        //        butData.set("SERVER_NAME",info.zname);
        //        butData.set("STATE",info.zstate);
        //        butData.set("MAIN_WINDOW",this);
        //    } else DEBUG.LogError("No add button event");
        //}

        //if (DataCenter.mGlobalConfig.GetData("ENABLE_UPDATE_SERVER_LIST", "VALUE") == "YES")
        //    WWWRequester.Instace.GetSvrList(OnSvrListRequsetFinish);
        //else
        //{
        //    NiceTable t = TableManager.GetTable("ServerList");
        //    if (t != null && mGameObjUI)
        //    {
        //        UIGridContainer grid = svrList;
        //        if (grid != null)
        //        {
        //            grid.MaxCount = t.GetRecordCount();
        //            int i = 0;
        //            foreach (KeyValuePair<int, DataRecord> vRe in t.GetAllRecord())
        //            {
        //                DataRecord r = vRe.Value;
        //                GameObject item = grid.controlList[i++];
        //                item.name = "server_id_info";
        //                GameCommon.SetUIText(item, "server_name", r["NAME"]);
        //                LoginNet.SetServerStateText(GameCommon.FindObject(item, "server_state"), r["STATE"]);

        //                NiceData butData = GameCommon.GetButtonData(item);
        //                if (butData != null)
        //                {
        //                    butData.set("SERVER_ID", (int)r["INDEX"]);
        //                    butData.set("SERVER_NAME", (string)r["NAME"]);
        //                    butData.set("STATE", (int)r["STATE"]);
        //                    butData.set("MAIN_WINDOW", this);
        //                }
        //                else
        //                    DEBUG.LogError("No add button event");
        //            }
        //        }
        //    }
        //}
    }

    //void OnSvrListRequsetFinish(object o) {

    //    string result = (string)o;
    //    NiceTable t = new NiceTable();
    //    byte [] br = Encoding.Unicode.GetBytes(result);
    //    MemoryStream ms = new MemoryStream(br);
    //    t.LoadTable(ms, Encoding.Unicode);

    //    if (t != null && mGameObjUI)
    //    {
    //        UIGridContainer grid = mGameObjUI.GetComponentInChildren<UIGridContainer>();
    //        if (grid != null)
    //        {
    //            grid.MaxCount = t.GetRecordCount();
    //            int i = 0;
    //            foreach (KeyValuePair<int, DataRecord> vRe in t.GetAllRecord())
    //            {
    //                DataRecord r = vRe.Value;
    //                GameObject item = grid.controlList[i++];
    //                item.name = "server_id_info";
    //                GameCommon.SetUIText(item, "server_name", r["NAME"]);
    //                LoginNet.SetServerStateText( GameCommon.FindObject(item, "server_state"), r["STATE"] );
					
    //                NiceData butData = GameCommon.GetButtonData(item);
    //                if (butData != null)
    //                {
    //                    butData.set("SERVER_ID", (int)r["INDEX"]);
    //                    /*Temp!!!!!!!!!!!!!!!!!!!!!!*/
    //                    butData.set("SERVER_NAME", (string)r["NAME"]);
    //                    //butData.set("SERVER_NAME", "绝世无双(内网)");
    //                    butData.set("STATE", (int)r["STATE"]);
    //                    butData.set("MAIN_WINDOW", this);
    //                }
    //                else
    //                    DEBUG.LogError("No add button event");
    //            }
    //        }
    //    }
    //}	
}

public class Button_server_id_info : CEvent
{
    public override bool _DoEvent()
    {
        int nServerID = get("SERVER_ID");

        LoginNet.WriteServerID(nServerID);

        tWindow mainWin = getObject("MAIN_WINDOW") as tWindow;
        if (mainWin != null)
        {
            if(get("STATE")==4||get("STATE")==5) return false;
            
            mainWin.SetText("select_server_name", get("SERVER_NAME"));
            //mainWin.SetText("select_server_state", get("STATE"));
			LoginNet.msServerName = get("SERVER_NAME");
			LoginNet.msServerState = get("STATE");
            CommonParam.mZoneID =get("SERVER_ID");
            LoginNet.SetServerStateText(mainWin.GetSub("select_server_state"));
//			EventCenter.Start("Button_close_server_list_button").DoEvent();
			DataCenter.Set ("IS_CHANGE_SERVER", true);
            RoleSelTopLeftWindow.FightStrengthNum = 0;

			DataCenter.CloseWindow ("SELECT_SERVER_WINDOW");
			DataCenter.OpenWindow("LANDING_WINDOW",false);

            //by chenliang
            //begin

            CommonParam.mZoneName = get("SERVER_NAME");
            CommonParam.mZoneState = (int)getObject("STATE");

            //设置游戏令牌标志
            LoginData.Instance.IsGameTokenValid = false;
            LoginData.Instance.IsInGameScene = false;

            //停止心跳包
            StaticDefine.useHeartbeat = false;

            //停止走马灯
            //RoleSelBottomLeftWindow.StopRollPlaying();

            //end
            //added by xuke
            DataCenter.SetData("LANDING_WINDOW", "REFRESH_ZONE_STATE", false);
            //end
            return true;
        }
        return false;
    }
}

public class Button_close_server_list_button : CEvent
{
    public override bool _DoEvent()
    {
//        LOG.log("Server select, then restart game");
//        //GlobalModule.RestartGame();
//
////        Settings.SwitchAccount();
//
////		string filePath = GameCommon.MakeGamePathFileName("Account.info");
////		System.IO.File.Delete(filePath);
//
//		GlobalModule.RestartGame();

		string strBackWindow = getObject ("BACK_WINDOW").ToString ();
		if(strBackWindow != "SELECT_SERVER_WINDOW")
		{
			DataCenter.CloseWindow ("SELECT_SERVER_WINDOW");
			DataCenter.OpenWindow (strBackWindow, false);
            //added by xuke
            DataCenter.SetData("LANDING_WINDOW", "REFRESH_ZONE_STATE", false);
            //end
		}
        return true;
    }
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class RegistrationWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_registration_account_button", new DefineFactory<Button_registration_account_button>());
		EventCenter.Self.RegisterEvent("Button_registration_window_back_button", new DefineFactory<Button_registration_window_back_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);
	
		Refresh(param);
	}

	public override bool Refresh (object param)
	{
		string backWindowName = param.ToString ();
		GameObject obj = GetSub ("registration_window_back_button");
		GameCommon.GetButtonData (obj).set ("BACK_WINDOW_NAME", backWindowName);

//		SetText ("version_number", CommonParam.ClientVer);
		DataCenter.OpenWindow ("VERSION_NUMBER_WINDOW");
		SetText ("input_name_lable", "");
		SetText ("input_password_label", "");
		SetText ("input_password_label_again", "");
		SetText ("error_password_label", "");
		return base.Refresh (param);
	}

	public override void onChange (string keyIndex, object objVal)
	{
		if(keyIndex == "INPUT_PASSWORD_IS_NOT_SAME")
		{
			SetText ("input_password_label_again", "");
			SetText ("error_password_label", "两次密码不一样，请重新输入");
//			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_REGISTRATION_ACCOUNT_NOT_SAME);
		}
		else if (keyIndex=="INFO")
		{
			GameObject input = GameCommon.FindObject(mGameObjUI, "error_password");
			UILabel lable = input.GetComponentInChildren<UILabel>();
			if(lable != null)
				lable.text = (string) objVal;
		}
		else if(keyIndex == "CREAT_ACCOUNT_SUCCEED")
		{
			SetText ("error_password_label", "注册成功");
			//DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_REGISTRATION_ACCOUNT_SUCCEED, () => set ("SEND", true));
		}
		else if(keyIndex == "BIND_ACCOUNT_SUCCEED")
		{
			SetText ("error_password_label", "绑定成功");
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_BIND_ACCOUNT_SUCCEED, () => set ("SEND", true));
		}
		else if(keyIndex == "SEND")
		{
			Close ();
			//DataCenter.Set ("ENTER_GS", false);
			DataCenter.OpenWindow ("LANDING_WINDOW" ,false);
		}
		base.onChange (keyIndex, objVal);
	}

	public override Data get(string key)
	{
		if (key == "INPUT_ACCOUNT")
		{
			GameObject input = GameCommon.FindObject(mGameObjUI, "input_name");
			UILabel lable = input.GetComponentInChildren<UILabel>();
			return new Data(lable.text);
		}
		else if (key == "INPUT_PS")
		{
			GameObject input = GameCommon.FindObject(mGameObjUI, "input_password");
			UIInput uiInput = input.GetComponentInChildren<UIInput>();
			return new Data(uiInput.value);
		}
		else if (key == "INPUT_PS_AGIAN")
		{
			GameObject input = GameCommon.FindObject(mGameObjUI, "input_password_again");
			UIInput uiInput = input.GetComponentInChildren<UIInput>();
			return new Data(uiInput.value);
		}

		return base.get(key);
	}
}

public class Button_registration_account_button : CEvent
{
	public override bool _DoEvent()
	{
		tLogicData loginWin = DataCenter.GetData("REGISTRATION_WINDOW");
		string inputAccout = loginWin.get("INPUT_ACCOUNT");
		string psWord = loginWin.get("INPUT_PS");
		string psWordAgain = loginWin.get("INPUT_PS_AGIAN");
		if(psWord != psWordAgain)
		{
			DataCenter.SetData ("REGISTRATION_WINDOW", "INPUT_PASSWORD_IS_NOT_SAME", true);
			return true;
		}
		
		if (inputAccout == "" || psWord == "")
			loginWin.set("INFO", "请输入账号或密码");
		else
		{
			string[] r = GameCommon.GetSavedLoginDataFromUnity();
            if(PlayerPrefs.HasKey ("IS_TOURISTS") && r != null)
            {
				if(PlayerPrefs.HasKey("IS_TOURISTS"))
					PlayerPrefs.DeleteKey("IS_TOURISTS");
				
            }
            else
			{
				if(GlobalModule.usePeerNetwork) {
					tEvent evt = Net.StartEvent("CS_RequestCreateAccount");
					evt.set("ACCOUNT", inputAccout);
					evt.set("PASSWORD", psWord);
					LoginNet.LoginGameServer(evt);
				} else {
					LoginNet.RequestRegistAccount(inputAccout, psWord, 1, null);
				}
                //DataCenter.OpenWindow(UIWindowString.announce_info);
			}
		}

		return true;
	}
}



public class Button_registration_window_back_button : CEvent
{
	public override bool _DoEvent()
	{
		/*
		string backWindowName = get ("BACK_WINDOW_NAME").ToString ();

		if(backWindowName == "LOGIN_WINDOW" || backWindowName == "LANDING_WINDOW")
		{
			DataCenter.OpenWindow (backWindowName, false);
		}
		else if(backWindowName == "FIRST_LOAD_WINDOW")
			DataCenter.OpenWindow (backWindowName);
		*/

		DataCenter.OpenWindow ("FIRST_LOAD_WINDOW");
		DataCenter.CloseWindow ("REGISTRATION_WINDOW");
		return true;
	}
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class ReadyRegistrationWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_register_or_landing_button", new DefineFactory<Button_register_or_landing_button>());
		EventCenter.Self.RegisterEvent("Button_select_server_list_button", new DefineFactory<Button_change_server_area>());
//		EventCenter.Self.RegisterEvent("Button_ready_change_server_area", new DefineFactory<Button_select_server_list_button>());
	}

	public override void OnOpen()
	{
		Refresh(null);
	}

	public override bool Refresh (object param)
	{
		SetVisible ("change_server_area", false);
		SetVisible ("server_name", false);
		SetVisible ("server_state", false);
		SetVisible ("info_name", true);

		NiceData button_data = GameCommon.GetButtonData (GetSub ("select_server_list_button"));
		button_data.set ("BACK_WINDOW", "READY_REGISTER_WINDOW");

		DataCenter.OpenWindow ("VERSION_NUMBER_WINDOW");

		return base.Refresh (param);
	}
}

public class Button_register_or_landing_button : CEvent
{
	public override bool _DoEvent()
	{
		if(LoginNet.msServerName != "")
		{
			DataCenter.SetData ("LOGIN_WINDOW", "BACK_WINDOW_NAME", "HAD_SELECT_SERVER_WINDOW");
			DataCenter.OpenWindow ("LOGIN_WINDOW", false);
			DataCenter.CloseWindow ("HAD_SELECT_SERVER_WINDOW");
		}
		else
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_REGISTRATION_ACCOUNT_SELECT_SERVER_FIRST);
		return true;
	}
}

//public class Button_select_server_list_button : CEvent
//{
//	public override bool _DoEvent()
//	{
//		DataCenter.OpenWindow ("SELECT_SERVER_WINDOW");
//		DataCenter.CloseWindow ("REGISTRATION_WINDOW");
//		return true;
//	}
//}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class HadSelectServerWindow : tWindow
{
	public override void OnOpen ()
	{
		if(System.IO.File.Exists (GameCommon.MakeGamePathFileName ("Account.info")))
		{
			System.IO.File.Delete (GameCommon.MakeGamePathFileName ("Account.info"));
			if(PlayerPrefs.HasKey("IS_TOURISTS"))
				PlayerPrefs.DeleteKey("IS_TOURISTS");
		}

		Refresh(null);
	}
	public override bool Refresh (object param)
	{
		SetVisible ("change_server_area", true);
		SetVisible ("server_name", true);
		SetVisible ("server_state", true);
		SetVisible ("info_name", false);
		GetSub ("select_server_list_button").GetComponent<BoxCollider>().enabled = false;

		NiceData button_data = GameCommon.GetButtonData (GetSub ("change_server_area"));
		button_data.set ("BACK_WINDOW", "HAD_SELECT_SERVER_WINDOW");

		SetText("server_name", LoginNet.msServerName);
		LoginNet.SetServerStateText(GetSub("server_state"));

		DataCenter.OpenWindow ("VERSION_NUMBER_WINDOW");

		return base.Refresh (param);
	}
}

public class VersionNumberWindow : tWindow
{
	public override void OnOpen ()
	{
        //by chenliang
        //begin

//		SetText("version_number", "版本号: " + CommonParam.ClientVer);
//---------------
        SetText("version_number", "版本号: " + CommonParam.RealClientVer);

        //end
	}
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class FirstLoadWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Register ("Button_enter_game_quickly_window_button", new DefineFactory<Button_enter_game_quickly_window_button>());
		EventCenter.Register ("Button_enter_game_quickly_button", new DefineFactory<Button_enter_game_quickly_button>());
		EventCenter.Register ("Button_close_enter_game_quickly_button", new DefineFactory<Button_close_enter_game_quickly_button>());
		EventCenter.Register ("Button_enter_register_window_button", new DefineFactory<Button_enter_register_window_button>());
		EventCenter.Register ("Button_enter_load_window_button", new DefineFactory<Button_enter_load_window_button>());
	}
	bool isFirstOpen = true;
	public override void OnOpen ()
	{

		//added by xuke begin
		if (isFirstOpen) 
		{
            //by chenliang
            //begin

//			DataCenter.OpenWindow (UIWindowString.announce_info);
//------------------

            //end
			isFirstOpen = false;
		}

		//end

		DataCenter.OpenWindow ("VERSION_NUMBER_WINDOW");
        //by chenliang
        //begin

        GameCommon.ResetWorldCameraColor();

        //设置登录、游戏令牌
        LoginData.Instance.IsLoginTokenValid = false;
        LoginData.Instance.IsGameTokenValid = false;
        LoginData.Instance.IsInGameScene = false;

        //停止心跳包
        StaticDefine.useHeartbeat = false;

        //停止走马灯
        //RoleSelBottomLeftWindow.StopRollPlaying();

        //end

        GetCurUIComponent<UIImageButton>("enter_register_window_button").isEnabled=!CommonParam.isCloseBtn;
        GetCurUIComponent<UIImageButton>("enter_game_quickly_window_button").isEnabled=!CommonParam.isCloseBtn;
	
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "IS_TOURISTS")
		{
			DataCenter.OpenWindow ("LANDING_WINDOW", false);
			DataCenter.CloseWindow ("START_GAME_LOADING_WINDOW");
			DataCenter.CloseWindow ("FIRST_LOAD_WINDOW");
			DataCenter.CloseWindow ("TOURISTS_LOAD_WINDOW");
//			GameObject.Destroy(GameObject.Find ("tourists_load_window"));
		}
		else if(keyIndex == "ENTER_GAME_QUICKLY")
		{
			tEvent evt = Net.StartEvent ("CS_CreateGuestUser");
			LoginNet.LoginGameServer(evt);
		}
	}

}

public class Button_enter_game_quickly_window_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.OpenWindow ("TOURISTS_LOAD_WINDOW");
		return true;
	}
}

public class Button_enter_load_window_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow("LOGIN_WINDOW");
        //DataCenter.OpenWindow(UIWindowString.announce_info);
        return true;
	}

    
}

public class Button_enter_register_window_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.Set ("IS_ENTER_REGISTER", true);
		DataCenter.OpenWindow ("REGISTRATION_WINDOW", "LANDING_WINDOW");
		DataCenter.CloseWindow ("LANDING_WINDOW");
		return true;
	}
}

public class Button_enter_game_quickly_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.Set ("ENTER_GAME_QUICKLY", true);
		// 随机生成的用户名字
		string randomAcc = "player" + Random.Range(1000, 10000);

		LoginNet.RequestRegistAccount(randomAcc, "quick", 0, null, true);
        //DataCenter.OpenWindow(UIWindowString.announce_info);
		return true;
	}
}

public class Button_close_enter_game_quickly_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow ("TOURISTS_LOAD_WINDOW");
		//GameObject.Destroy(GameObject.Find ("tourists_load_window"));
		return true;
	}
}


//---------------------------------------------------------------------
//---------------------------------------------------------------------
public class CS_CreateGuestUser : BaseNetEvent
{
	public override void _OnResp (tEvent respEvent)
	{
		respEvent.Dump();
		
		int errorType = respEvent.get("INFOTYPE");
		if (errorType == (int)LOGIN_INFO.eLogin_CreateAccount_Succeed)
		{
			// Save account and password md5
			string account = respEvent.get("ACCOUNT");
//			string password = get("PASSWORD");
//			string md5 = GameCommon.MakeMD5(password);
			string md5 = respEvent.get ("PASSWORD_MD5");
			string accountFilePath = GameCommon.MakeGamePathFileName("Account.info");
			NiceTable t = new NiceTable();
			t.SetField("INDEX", FIELD_TYPE.FIELD_INT, 0);
			t.SetField("VALUE_1", FIELD_TYPE.FIELD_STRING, 1);
			t.SetField("VALUE_2", FIELD_TYPE.FIELD_STRING, 2);
			DataRecord r = t.CreateRecord(0);
			r.set("VALUE_1", account);
			r.set("VALUE_2", md5);
			t.SaveBinary(accountFilePath);

			PlayerPrefs.SetString ("IS_TOURISTS", "isTourists");

			DataCenter.SetData ("FIRST_LOAD_WINDOW", "IS_TOURISTS", true);
		}
		else
		{
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_CREATE_GUEST_USER_ERROR);
		}
		
		Finish();
	}
}

