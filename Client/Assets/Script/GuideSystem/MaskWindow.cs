using Logic;
using DataTable;
using System;
using System.Collections;
using UnityEngine;
using Utilities;


public class MaskWindow : tWindow
{
    private static readonly int MAX_WIDTH = 1500;
    private static readonly int MAX_HEIGHT = 1000;
    private static readonly float FADE_TIME = 0.25f;
    private static readonly float ALPHA = 0.6f;

    public static int leftOffset = 0;
    public static int rightOffset = 0;
    public static int topOffset = 0;
    public static int bottomOffset = 0;

    private bool oneShot = true;
    private GameObject operateRegion;
    private GameObject shadow1;
    private GameObject shadow2;
    private GameObject shadow3;
    private GameObject shadow4;

    public static void ResetOffset()
    {
        leftOffset = 0;
        rightOffset = 0;
        topOffset = 0;
        bottomOffset = 0;
    }

    public override void Init()
    {
        //EventCenter.Self.RegisterEvent("Button_operate_region", new DefineFactory<Button_operate_region>());
        //EventListenerCenter.RegisterListener(GuideEvent.OnMaskMouseClick);
    }

    public override void Open(object param)
    {
        base.Open(param);

        GlobalModule.sbIsNeedJudge = false;
        operateRegion = GetSub("operate_region");
        shadow1 = GetSub("shadow_1");
        shadow2 = GetSub("shadow_2");
        shadow3 = GetSub("shadow_3");
        shadow4 = GetSub("shadow_4");

        UIWidget widget = operateRegion.GetComponent<UIWidget>();
        oneShot = true;

        if (param is Rect)
        {           
            Rect rect = (Rect)param;
            widget.transform.localPosition = new Vector3(rect.x, rect.y, 0f);
            widget.width = (int)rect.width;
            widget.height = (int)rect.height;
            SetShadowActive(false);
        }
        else if (param is GameObject)
        {
            BoxCollider collider = (param as GameObject).GetComponentInChildren<BoxCollider>();

            if (collider != null)
            {
                widget.transform.parent = collider.transform;
                widget.transform.localPosition = collider.center;
                widget.transform.localPosition += new Vector3((rightOffset - leftOffset) / 2f, (topOffset - bottomOffset) / 2f, 0f);
                widget.width = (int)collider.size.x + leftOffset + rightOffset;
                widget.height = (int)collider.size.y + topOffset + bottomOffset;
                widget.transform.parent = mGameObjUI.transform;
                Vector3 localPos = widget.transform.localPosition;
                widget.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
                SetShadowActive(true);
            }
            else
            {
                widget.transform.localPosition = Vector3.zero;
                widget.width = MAX_WIDTH;
                widget.height = MAX_HEIGHT;
                SetShadowActive(false);
            }
        }
        else
        {
            widget.transform.localPosition = Vector3.zero;
            widget.width = MAX_WIDTH;
            widget.height = MAX_HEIGHT;
            SetShadowActive(false);
        }
    }

    public override void OnClose()
    {
        GlobalModule.sbIsNeedJudge = true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "ONE_SHOT":
                oneShot = (bool)objVal;
                break;

            case "ON_MOUSE_CLICK":
                if (oneShot)
                    Close();
                break;

            case "SHOW_FINGER":
                GuideManager.FingerLocation location = objVal as GuideManager.FingerLocation;
                ShowFinger(location);
                break;

            case "SHADOW":
                if(objVal is bool)
                    SetShadowActive((bool)objVal);
                break;
        }
    }

    private void ShowFinger(GuideManager.FingerLocation location)
    {
        ObserverCenter.Add(GuideEvent.OnMaskMouseClick, () => GuideManager.DestroyFinger());

        if (location == null)
            GuideManager.ShowFinger(operateRegion, 1f);
        else
            GuideManager.ShowFinger(operateRegion, location.mScale, location.mOffset);
    }

    private void SetShadowActive(bool active)
    {
        shadow1.SetActive(active);
        shadow2.SetActive(active);
        shadow3.SetActive(active);
        shadow4.SetActive(active);

        if (active)
        {
            UIWidget widget = operateRegion.GetComponent<UIWidget>();
            float x = widget.width / 2f;
            float y = widget.height / 2f;
            float z = -2000f;

            shadow1.transform.localPosition = new Vector3(x, y, z);
            shadow2.transform.localPosition = new Vector3(-x, y, z);
            shadow3.transform.localPosition = new Vector3(-x, -y, z);
            shadow4.transform.localPosition = new Vector3(x, -y, z);

            this.StartCoroutine(FadeIn(shadow1, shadow2, shadow3, shadow4));
        }
    }

    private IEnumerator FadeIn(params GameObject[] objs)
    {
        float time = 0f;

        while (time < FADE_TIME)
        {
            foreach (var obj in objs)
            {
                obj.GetComponent<UIWidget>().alpha = time / FADE_TIME * ALPHA;
            }

            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        foreach (var obj in objs)
        {
            obj.GetComponent<UIWidget>().alpha = ALPHA;
        }
    }
}