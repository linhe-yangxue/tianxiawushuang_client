using UnityEngine;
using System.Collections;
using System;


public class MsgBoxBtnInfo
{
    public readonly Action btnAction;
    public readonly string btnName;

    public MsgBoxBtnInfo(string btnName,Action btnAction) {
        this.btnAction = btnAction;
        this.btnName = btnName;
    }
}

public class MsgBoxInfo
{
    public readonly MsgBoxBtnInfo[] btnInfoArr;
    public readonly string msgBoxName;
    public readonly string title;
    public readonly string content;
	public readonly int iconIndex;

    public MsgBoxInfo(MsgBoxBtnInfo[] btnInfoArr, string msgBoxName, string title, string content, int iconIndex) {
        this.btnInfoArr = btnInfoArr;
        this.msgBoxName = msgBoxName;
        this.title = title;
        this.content = content;
		this.iconIndex = iconIndex;
    }
}

abstract class DynamicMsgBoxBase : tWindow
{
    UILabel[] btnLabelArr;
    string windowName;
    sealed public override void Open(object param) {
        base.Open(param);

        var info = param as MsgBoxInfo;
        if (info == null) DEBUG.LogWarning("YOU ARE WRONG!!!!!!!");
        if (info.btnInfoArr.Length != btnLabelArr.Length) DEBUG.LogWarning("BUTTON ACTION COUNT WRONG!!!!");
        GetUILabel("title").text = info.title;
        GetUILabel("msgBoxName").text = info.msgBoxName;
        GetUILabel("content").text = info.content;

		//set icon
		GameObject obj = GameCommon.FindObject (mGameObjUI, "item_icon");
		UISprite sprite = GameCommon.FindComponent<UISprite> (obj, "background_sprite");
		if (sprite != null) {
			GameCommon.SetPalyerIcon (sprite, info.iconIndex);
		}

        DEBUG.Log(windowName);
        AddButtonAction("close", () => {
            DEBUG.Log("close");
            DataCenter.CloseWindow(windowName);
        });
        for (int i = 0; i < info.btnInfoArr.Length; i++) {
            var btnInfo = info.btnInfoArr[i];
            btnLabelArr[i].text = btnInfo.btnName;
            ObserverCenter.Add(windowName + "_BUTTON_" + i, btnInfo.btnAction);
        }
    }

    sealed public override void OnOpen() {
        base.OnOpen();
    }
   
    protected void MessageBoxInit(int btnCount, string _windowName) {
        windowName = _windowName;
        Action<int> setBtnAction = (btnID) => {
            AddButtonAction("button_" + btnID, () => {
                ObserverCenter.Notify(windowName + "_BUTTON_" + btnID);
                DataCenter.CloseWindow(windowName);
            });
        };

        btnLabelArr = new UILabel[btnCount];
        for (int i = 0; i < btnLabelArr.Length; i++) {
            btnLabelArr[i] = GetUILabel("btn_label_" + (i + 1));
            setBtnAction(i);
        }
    }

    public override void Close() {
        base.Close();
        for (int i = 0; i < btnLabelArr.Length; i++) {
            ObserverCenter.Clear(windowName + "_BUTTON_" + i);
        }
    }
}

class DynamicMsgBox4 : DynamicMsgBoxBase
{
    protected override void OpenInit() {
        base.OpenInit();
        MessageBoxInit(4, UIWindowString.dynamic_MsgBox_4);
    }
}

class DynamicMsgBox3 : DynamicMsgBoxBase
{
    protected override void OpenInit() {
        base.OpenInit();
        MessageBoxInit(3, UIWindowString.dynamic_MsgBox_3);
    }

}

class DynamicMsgBox2 : DynamicMsgBoxBase
{
    protected override void OpenInit() {
        base.OpenInit();
        MessageBoxInit(2, UIWindowString.dynamic_MsgBox_2);
    }

}
