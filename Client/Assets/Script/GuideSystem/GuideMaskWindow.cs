using Logic;
using DataTable;
using System;
using System.Collections;
using UnityEngine;
using Utilities.Routines;


public class GuideMaskWindow : tWindow
{
    private static readonly int MAX_WIDTH = 1500;
    private static readonly int MAX_HEIGHT = 1000;

    public static bool isOpened { get; private set; }
    public static bool isLocked { get; private set; }

    private UIWidget operateWidget;
    private Action onClick;
    private IRoutine bindButtonRoutine;
    private float buttonScale = 1f;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_operate_region", new DefineFactory<Button_operate_region>());
    }

    public override void OnOpen()
    {
        isOpened = true;
        isLocked = true;
        GlobalModule.sbIsNeedJudge = false;
        operateWidget = GetComponent<UIWidget>("operate_region");
        operateWidget.transform.localPosition = Vector3.zero;
        operateWidget.width = MAX_WIDTH;
        operateWidget.height = MAX_HEIGHT;
        onClick = null;  
    }

    public override void OnClose()
    {
        GlobalModule.sbIsNeedJudge = true;
        isLocked = false;
        isOpened = false;

        if (bindButtonRoutine != null)
        {
            bindButtonRoutine.Break();
            bindButtonRoutine = null;
        }
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "SET_CENTER":
                if (bindButtonRoutine != null)
                {
                    bindButtonRoutine.Break();
                    bindButtonRoutine = null;
                }

                Vector2 center = (Vector2)objVal;
                operateWidget.transform.localPosition = new Vector3(center.x, center.y, 0f);
                break;

            case "SET_SIZE":
                if (bindButtonRoutine != null)
                {
                    bindButtonRoutine.Break();
                    bindButtonRoutine = null;
                }

                Vector2 size = (Vector2)objVal;
                operateWidget.width = (int)size.x;
                operateWidget.height = (int)size.y;
                break;

            case "SET_BUTTON":
                if (bindButtonRoutine != null)
                {
                    bindButtonRoutine.Break();
                    bindButtonRoutine = null;
                }

                GameObject btn = objVal as GameObject;
                BoxCollider collider = btn == null ? null : btn.GetComponent<BoxCollider>();

                if (collider == null)
                {
                    operateWidget.transform.localPosition = Vector3.zero;
                    operateWidget.width = 0;
                    operateWidget.height = 0;
                }
                else
                {
                    operateWidget.transform.position = collider.transform.TransformPoint(collider.center);
                    operateWidget.width = (int)(collider.size.x * buttonScale);
                    operateWidget.height = (int)(collider.size.y * buttonScale);
                    Vector3 localPos = operateWidget.transform.localPosition;
                    operateWidget.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);

                    bindButtonRoutine = btn.StartRoutine(DoBindButton(collider));
                }

                break;

            case "SET_ACTION":
                onClick = (Action)objVal;
                break;

            case "SET_BUTTON_SCALE":
                buttonScale = (float)objVal;
                break;

            case "ON_CLICK":
                if (onClick != null)
                {
                    onClick();
                }
                break;
        }

        isLocked = isOpened && 
            (operateWidget.width == 0 
            || operateWidget.height == 0 
            || onClick == null 
            || Mathf.Abs(operateWidget.transform.localPosition.x) > (GuideKit.GetLocalWidth() + operateWidget.width) /2f
            || Mathf.Abs(operateWidget.transform.localPosition.y) > (GuideKit.GetLocalHeight() + operateWidget.height) / 2f);
    }

    private IEnumerator DoBindButton(BoxCollider collider)
    {
        while (collider != null)
        {
            yield return new Delay(0.5f);
            operateWidget.transform.position = collider.transform.TransformPoint(collider.center);
            operateWidget.width = (int)(collider.size.x * buttonScale);
            operateWidget.height = (int)(collider.size.y * buttonScale);
            Vector3 localPos = operateWidget.transform.localPosition;
            operateWidget.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);

            isLocked = isOpened &&
                (operateWidget.width == 0
                || operateWidget.height == 0
                || onClick == null
                || Mathf.Abs(operateWidget.transform.localPosition.x) > (GuideKit.GetLocalWidth() + operateWidget.width) / 2f
                || Mathf.Abs(operateWidget.transform.localPosition.y) > (GuideKit.GetLocalHeight() + operateWidget.height) / 2f);
        }
    }
}

public class Button_operate_region : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("GUIDE_MASK_WINDOW", "ON_CLICK", null);
        return true;
    }
}