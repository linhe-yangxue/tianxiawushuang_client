using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ContainerWindowBase:tWindow
{
    protected string[] windowNameArr;
    protected string[] toggleNameArr;

    protected void ContainerInit() {
        for (int i = 0; i < windowNameArr.Length; i++) {
            var toggleName = toggleNameArr[i];
            var winName=windowNameArr[i];
            AddButtonAction(toggleName, () => {
                CloseAllTabWindow();
                DataCenter.OpenWindow(winName);
            });
        }
    }

    protected void CloseAllTabWindow() {
        windowNameArr.Foreach(winName => DataCenter.CloseWindow(winName));
    }

    public override void OnOpen() {
        base.OnOpen();
        GetUIToggle(toggleNameArr[0]).value = true;
    }

    public override void Close() {
        base.Close();
        if (windowNameArr!=null) windowNameArr.Foreach(winName => DataCenter.CloseWindow(winName));
    }

}

