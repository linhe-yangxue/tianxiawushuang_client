using UnityEngine;
using System.Collections;


public class GuideBaseMaskWindow : tWindow
{
    public static bool isOpened { get; private set; }

    public override void OnOpen()
    {
        isOpened = true;
    }

    public override void OnClose()
    {
        isOpened = false;
    }
}