using UnityEngine;
using System.Collections;
using System;

public static class FunctionHelper
{
    public static Action GetButtonAction(string text, Action action) {
        return () => DataCenter.OpenMessageOkWindow(text, action);
    }

  
}
