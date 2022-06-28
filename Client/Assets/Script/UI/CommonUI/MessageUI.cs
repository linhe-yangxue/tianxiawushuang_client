using UnityEngine;
using System.Collections;

public class MessageWindow : tWindow
{
    public static bool isOpened { get; private set; }
    public static int depth { get; private set; }

	TM_FadeOut evt = null;
    public override void Init()
    {
        Logic.EventCenter.Self.RegisterEvent("Button_message_window", new Logic.DefineFactoryLog<Button_message_window>());
		Logic.EventCenter.Self.RegisterEvent("Button_message_window_button", new Logic.DefineFactoryLog<Button_message_window>());
    }

    public override void Open(object param) 
    {
        base.Open(param);
        //EventListenerCenter.RegisterListener("MESSAGE_CLICK");
        GameCommon.SetUIText(mGameObjUI, "text", param.ToString());
        //by chenliang
        //begin

        UIPanel tmpPanel = mGameObjUI.GetComponent<UIPanel>();
        if (tmpPanel != null)
        {
            tmpPanel.depth = 155;
            tmpPanel.startingRenderQueue = 4000;
        }

        //end
        isOpened = true;
        depth = 155;
    }

    public override void Close()
    {
        base.Close();
        ObserverCenter.Clear("MESSAGE_CLICK");

		if(evt != null && !evt.GetFinished ())
		{
			evt.Finish ();
			evt = null;
			GameObject.Destroy(mGameObjUI);
		}

        isOpened = false;
    }

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "FADE_OUT_TIME")
		{
			SetVisible ("message_window_button", false);

			evt = Logic.EventCenter.Self.StartEvent("TM_FadeOut") as TM_FadeOut;
			evt.mTarget = GetSub("message_background");
			evt.mBefore = 0.7f;
			evt.mDuration = 0.3f;
			evt.onElapsed += x => Close();
			evt.DoEvent();
		}
        //by chenliang
        //begin

        switch (keyIndex)
        {
            case "SET_PANEL_DEPTH":
                {
                    UIPanel tmpPanel = mGameObjUI.GetComponent<UIPanel>();
                    if (tmpPanel != null)
                        tmpPanel.depth = (int)objVal;

                    depth = (int)objVal;
                } break;
            case "SET_PANEL_START_RENDER_QUEUE":
                {
                    UIPanel tmpPanel = mGameObjUI.GetComponent<UIPanel>();
                    if (tmpPanel != null)
                        tmpPanel.startingRenderQueue = (int)objVal;
                }break;
        }

        //end
	}
}

class Button_message_window : Logic.CEvent
{
    public override bool _DoEvent()
    {
        ObserverCenter.Notify("MESSAGE_CLICK");
        DataCenter.CloseMessageWindow();
		return true;
    }
}


//-------------------------------------------------------------------------
public class MessageHelpWindow : tWindow
{
	public override void Init()
	{
		Logic.EventCenter.Self.RegisterEvent("Button_close_message_help_window", new Logic.DefineFactoryLog<Button_close_message_help_window>());
        Logic.EventCenter.Self.RegisterEvent("Button_message_help_window_bg", new Logic.DefineFactoryLog<Button_close_message_help_window>());
	}
	
	public override void Open(object param) 
	{
		string strText = param.ToString ();
		strText = strText.Replace ("\\n", "\n");
		base.Open(param);
		//EventListenerCenter.RegisterListener("MESSAGE_CLICK");
		GameCommon.SetUIText(mGameObjUI, "text", strText);
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "TITLE")
		{
			SetText ("title_name", objVal.ToString ());
		}
	}

	public override void Close()
	{
		base.Close();
        ObserverCenter.Clear("MESSAGE_CLICK");
	}
}

class Button_close_message_help_window : Logic.CEvent
{
	public override bool _DoEvent()
	{
        ObserverCenter.Notify("MESSAGE_CLICK");
		DataCenter.CloseHelpMessageWindow();
		return true;
	}
}

