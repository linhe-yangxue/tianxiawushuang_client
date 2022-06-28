using UnityEngine;
using System.Collections;
using Logic;
using Utilities.Routines;


public class AstrologyTipWindow : tWindow
{
    private const float CAST_TIME = 1.0f;
    private const float ROTATE_SPEED = 1440f;

    public static bool isOpened { get; private set; }
 
    private ConfigParam configParam;
    private bool canClose = false;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_astrology_tip_mask", new DefineFactory<Button_astrology_tip_mask>());
    }
    
    public override void Open(object param)
    {
        base.Open(param);
        SetVisible("delay_open", false);
        configParam = param as ConfigParam;
        canClose = false;
        isOpened = true;

        if (configParam != null)
        {
            mGameObjUI.StartRoutine(DoCast());
            SetText("tip_text", configParam.GetString("astrology_tip"));
            SetText("num_text", configParam.GetString("astrology_num"));
        }
        else 
        {
            Close();
        }
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "CLICK_MASK":
                if (canClose)
                    Close();
                break;
        }
    }

    public override void OnClose()
    {
        isOpened = false;
    }

    private IEnumerator DoCast()
    {
        float time = 0f;
        Transform target = GetSub("astrology_stone").transform;

        while (time < CAST_TIME)
        {
            yield return null;
            target.localRotation = Quaternion.Euler(0f, 0f, time * ROTATE_SPEED);
            target.localScale = Vector3.one * time / CAST_TIME;
            time += Time.deltaTime;
        }

        target.localRotation = Quaternion.identity;
        target.localScale = Vector3.one;
        canClose = true;
        SetVisible("delay_open", true);
    }
}


public class Button_astrology_tip_mask : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("ASTROLOGY_TIP_WINDOW", "CLICK_MASK", true);
        return true;
    }
}