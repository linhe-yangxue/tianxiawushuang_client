using UnityEngine;
using System.Collections;
using Logic;

public class MessageOkUI : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		DataCenter.Self.registerData("MESSAGE_WINDOW", new MessageOkWindow(gameObject));

		EventCenter.Self.RegisterEvent("Button_button_close_message_window", new DefineFactory<Button_button_close_message_window>());
		EventCenter.Self.RegisterEvent("Button_button_ok_message_window", new DefineFactory<Button_button_ok_message_window>());
        EventCenter.Self.RegisterEvent("Button_message_ok_window_bg", new DefineFactory<Button_button_message_ok_window_bg>());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDestroy()
	{
		DataCenter.Remove("MESSAGE_WINDOW");
	}
}


public class MessageOkWindow : tWindow
{
//	GameObject button_ok;
    public static bool isOpened { get; private set; }
    public static int depth { get; private set; }

    public bool iNotClose = false;

	public MessageOkWindow(GameObject obj)
	{
		mGameObjUI = obj;
        //EventListenerCenter.RegisterListener("MESSAGE_OK");
//		button_ok = GameCommon.FindObject (mGameObjUI, "button_ok_message_window");
	}

	public override void Open(object param) 
	{
		base.Open(param);

        // 临时添加代码，保持原设置
        UIPanel tmpPanel = mGameObjUI.GetComponent<UIPanel>();
        if (tmpPanel != null)
            tmpPanel.depth = 300;

        depth = 300;
        isOpened = true;

        //设置默认文字
        SetButtonText("取消", "确定");
        GameCommon.SetUIText(mGameObjUI, "text", param.ToString());
        GameCommon.SetUIText(mGameObjUI, "state_text", param.ToString());

        //初始化
        InitData();
	}

    public void InitData()
    {
        iNotClose = false;

		bool bIsShowState = (bool)DataCenter.Get ("IS_SHOW_STATE");
        if (GetSub("common_group") != null)
		    GetSub ("common_group").SetActive (!bIsShowState);
		GetSub ("state_group").SetActive (bIsShowState);
    }

    public void SetButtonText(string left, string right)
    {
        GameObject objClose = GameCommon.FindObject(mGameObjUI, "button_close_message_window");
        GameObject objOk = GameCommon.FindObject(mGameObjUI, "button_ok_message_window");
        GameCommon.SetUIText(objClose, "Label", left);
        GameCommon.SetUIText(objOk, "Label", right);
    }

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
		case "WINDOW_SEND":
			NiceData data = GameCommon.GetButtonData (mGameObjUI, "button_ok_message_window");
			if(data != null) data.set ("WINDOW_NAME", objVal.ToString ());
			else DEBUG.Log ("NiceData of button_ok_message_window is null");
			break;
        case "WINDOW_TEXT_UPDATE":
            {
                GameCommon.SetUIText(mGameObjUI, "text", objVal.ToString());
            }
            break;
        case "WINDOW_BUTTON_TEXT_UPDATE":
            {
                if (objVal is string)
                {
                    string strText = (string)objVal;
                    string[] strArr = strText.Split('|');
                    if(strArr.Length > 1)
                    {
                        SetButtonText(strArr[0], strArr[1]);
                    }
                }
            }
            break;
        case "WINDOW_NOT_CLOSE":
            {
                bool notClose = (bool)objVal;
                DataCenter.Set("MESSAGE_OK_NOT_CLOSE", notClose);
            }
            break;
        case "SET_PANEL_DEPTH":
            {
                UIPanel tmpPanel = mGameObjUI.GetComponent<UIPanel>();
                if (tmpPanel != null)
                    tmpPanel.depth = (int)objVal;

                depth = (int)objVal;
            }
            break;
        case "SET_PANEL_START_RENDER_QUEUE":
            {
                UIPanel tmpPanel = mGameObjUI.GetComponent<UIPanel>();
                if (tmpPanel != null)
                    tmpPanel.startingRenderQueue = (int)objVal;
            } break;
		case "WINDOW_CLICK_CLOSE":
			NiceData _data = GameCommon.GetButtonData (mGameObjUI, "button_close_message_window");
			if(_data != null) _data.set ("WINDOW_BTN_NAME", objVal.ToString ());
			else DEBUG.Log ("NiceData of button_ok_message_window is null");
			break;
		}
	}

    public override void Close()
    {
        base.Close();
        ObserverCenter.Clear("MESSAGE_OK");
        ObserverCenter.Clear("MESSAGE_CANCEL");
        isOpened = false;
    }
}

public class Button_button_message_ok_window_bg : CEvent
{
    public override bool _DoEvent()
    {
        if (!DataCenter.Get("MESSAGE_OK_NOT_CLOSE"))
        {
            DataCenter.CloseMessageOkWindow();
        }
        return true;
    }
}

public class Button_button_close_message_window : CEvent
{
	public override bool _DoEvent()
	{
//        ObserverCenter.Notify("MESSAGE_CANCEL");
//		DataCenter.CloseMessageOkWindow ();
		object val;
		bool b = getData ("BUTTON", out val);
		GameObject obj = val as GameObject;
		
		string window_name = getObject ("WINDOW_BTN_NAME").ToString();
		
		if (window_name != "")
			DataCenter.SetData(window_name, "SEND", true);
		else
			ObserverCenter.Notify("MESSAGE_CANCEL");
		
		if (!DataCenter.Get("MESSAGE_OK_NOT_CLOSE"))
		{
			DataCenter.CloseMessageOkWindow();
		}
		return true;
	}
}

public class Button_button_ok_message_window : CEvent
{
	public override bool _DoEvent()
	{
		object val;
		bool b = getData ("BUTTON", out val);
		GameObject obj = val as GameObject;

		string window_name = getObject ("WINDOW_NAME").ToString();

        if (window_name != "")
            DataCenter.SetData(window_name, "SEND", true);
        else
            //DEBUG.LogError ("data of button_ok_message_window is null");
            ObserverCenter.Notify("MESSAGE_OK");

        if (!DataCenter.Get("MESSAGE_OK_NOT_CLOSE"))
        {
            DataCenter.CloseMessageOkWindow();
        }
		return true;
	}
}

