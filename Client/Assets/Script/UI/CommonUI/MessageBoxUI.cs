using UnityEngine;
using System.Collections;
using Logic;
public class MessageBoxUI : MonoBehaviour 
{
	void Start () 
    {
        DataCenter.Self.registerData("MessageBoxWindow", new MessageBoxWindow(gameObject));
     
	}


    void OnDestroy()
    {
        DataCenter.Remove("MessageBoxWindow");
    }
}

public class MessageBoxWindow : tWindow
{
    public MessageBoxWindow(GameObject obj)
	{
		mGameObjUI = obj;

		Close();
	}

    UIImageButton but_ok;
    UIImageButton but_cancel;
    UILabel lab;

    public override void Init()
    {
        but_ok = mGameObjUI.transform.FindChild("but_ok").GetComponent<UIImageButton>();
        lab = mGameObjUI.transform.FindChild("Label").GetComponent<UILabel>();
        but_ok.gameObject.SetActive(false);
        lab.gameObject.SetActive(false);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        MessageBox m = objVal as MessageBox;

        if(m!=null)
        {
            ShowWin(m);
        }
    }

    public override void Open(object param)
    {
        
    }

    void ShowWin(MessageBox m)
    {
        switch (m.type)
        {
            case MessageBoxType.close:
                but_ok.gameObject.SetActive(true);
                lab.gameObject.SetActive(true);
                SetMessageBoxType(m);
                break;
            case MessageBoxType.click:
                but_ok.gameObject.SetActive(true);
                lab.gameObject.SetActive(true);
                SetMessageBoxType(m);
                break;
        }
    }

    void SetMessageBoxType(MessageBox m)
    {
        switch (m.type)
        {
            case MessageBoxType.close:
                lab.text = m.content;
                onclickok += CloseWindow;
                break;
            case MessageBoxType.click:
                lab.text = m.content;
                onclickok = m.onclickok;
                break;
        }
    }

    void CloseWindow()
    {
        onclickok = null;
        but_ok.gameObject.SetActive(false);
        lab.gameObject.SetActive(false);
        DataCenter.SetData("MessageBoxWindow", "CLOSE", true);
    }

    public class Button_Ok : CEvent
    {
        public override bool _DoEvent()
        {
            if (onclickok != null)
                onclickok();
            return true;
        }
    }

    public static OnClickOk onclickok;
}
public delegate void OnClickOk();
public class MessageBox
{
    public string content;
    public MessageBoxType type;
    public OnClickOk onclickok;
}
public enum MessageBoxType
{
    close,
    click,
}


public class TopMessageWindow : tWindow
{
    tEvent mWaitCloseEvent;

    public override void Open(object text)
    {
        base.Open(text);
        if (mWaitCloseEvent == null)
            mWaitCloseEvent = EventCenter.WaitAction(this, "CloseMessage", null, 6);
        else
        {
            mWaitCloseEvent.SetFinished(false);
            mWaitCloseEvent.WaitTime(6);
        }

        SetText("Label", (string)text);
    }

    public void CloseMessage(object param)
    {
        Close();
        if (mWaitCloseEvent == null)
            mWaitCloseEvent.Finish();
    }
}