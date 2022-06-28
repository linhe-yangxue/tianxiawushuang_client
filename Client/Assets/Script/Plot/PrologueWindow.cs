using UnityEngine;
using System;
using System.Collections;
using Utilities;
using Logic;
using Utilities.Routines;


public class PrologueWindow : tWindow
{
    public static float fadeTime = 0.5f;

    public static bool isOpened { get; private set; }

    private static readonly int FRONT_DEPTH = 10;
    private static readonly int BACK_DEPTH = 0;

    private UITexture backTex;
    private UITexture frontTex;
    private Action onClick = null;
    private bool locked = false;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_prologue_mask", new DefineFactory<Button_prologue_mask>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        frontTex = GetComponent<UITexture>("front");
        backTex = GetComponent<UITexture>("back");

        PrologueWindowParam p = param as PrologueWindowParam;

        if (isOpened)
        {
            this.StartCoroutine(DoFade(p, fadeTime));
        }
        else 
        {
            this.StartCoroutine(DoFadeIn(p, fadeTime));
        }

        isOpened = true;
    }

    public override void OnClose()
    {
        locked = false;
        isOpened = false;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "SET_ACTION":
                onClick = (Action)objVal;
                break;

            case "ON_CLICK":
                if (!locked && onClick != null)
                {
                    onClick();
                }
                break;

            case "FADE_OUT":
                if (frontTex != null && backTex != null)
                {
                    this.StartCoroutine(DoFadeOut(fadeTime));
                }
                break;
        }
    }

    private IEnumerator DoFade(PrologueWindowParam param, float duration)
    {
        locked = true;
        backTex.mainTexture = GameCommon.LoadTexture(param.path, LOAD_MODE.RESOURCE);
        backTex.alpha = 1f;
        frontTex.alpha = 1f;

        backTex.gameObject.StopAllCoroutines();
        UILabel label = GameCommon.FindComponent<UILabel>(backTex.gameObject, "label");
        label.text = "";
        label.fontSize = param.textSize;
        label.transform.localPosition = new Vector3(param.textOffset.x, param.textOffset.y, 0f);

        if (!string.IsNullOrEmpty(param.text))
        {
            backTex.gameObject.StartCoroutine(DoShowText(label, param.text, param.textSpeed, param.textDelay));
        }

        if (duration > 0.001f)
        {
            while (frontTex.alpha > 0.001f)
            {
                yield return null;
                frontTex.alpha -= Time.deltaTime / duration;
            }
        }

        frontTex.alpha = 0f;

        var temp = frontTex;
        frontTex = backTex;
        backTex = temp;

        frontTex.name = "front";
        frontTex.depth = FRONT_DEPTH;
        GameCommon.FindComponent<UILabel>(frontTex.gameObject, "label").depth = FRONT_DEPTH + 1;


        backTex.name = "back";
        backTex.depth = BACK_DEPTH;
        GameCommon.FindComponent<UILabel>(backTex.gameObject, "label").depth = BACK_DEPTH + 1;

        locked = false;
    }

    private IEnumerator DoFadeIn(PrologueWindowParam param, float duration)
    {
        locked = true;
        frontTex.mainTexture = GameCommon.LoadTexture(param.path, LOAD_MODE.RESOURCE);
        backTex.alpha = 0f;
        frontTex.alpha = 0f;

        frontTex.gameObject.StopAllCoroutines();
        UILabel label = GameCommon.FindComponent<UILabel>(frontTex.gameObject, "label");
        label.text = "";
        label.fontSize = param.textSize;
        label.transform.localPosition = new Vector3(param.textOffset.x, param.textOffset.y, 0f);

        if (!string.IsNullOrEmpty(param.text))
        {
            frontTex.gameObject.StartCoroutine(DoShowText(label, param.text, param.textSpeed, param.textDelay));
        }

        if (duration > 0.001f)
        {
            while (frontTex.alpha < 0.999f)
            {
                yield return null;
                frontTex.alpha += Time.deltaTime / duration;              
            }
        }

        frontTex.alpha = 1f;
        locked = false;
    }

    private IEnumerator DoFadeOut(float duration)
    {
        locked = true;
        backTex.alpha = 0f;

        if (duration > 0.001f)
        {
            while (frontTex.alpha > 0.001f)
            {
                yield return null;
                frontTex.alpha -= Time.deltaTime / duration;               
            }
        }

        frontTex.alpha = 0f;
        locked = false;
        Close();
    }

    private IEnumerator DoShowText(UILabel label, string text, float speed, float delay)
    {
        label.text = "";
        int n = text.Length;
        float value = 0;
        int current = 0;

        yield return new WaitForSeconds(delay);

        while (current < n)
        {
            yield return null;
            value += Time.deltaTime * speed;

            if(current < (int)value)
            {
                current = (int)value;
                label.text = text.Substring(0, current);
            }            
        }
    }
}


public class Button_prologue_mask : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PROLOGUE_WINDOW", "ON_CLICK", true);
        return true;
    }
}


public class PrologueWindowParam
{
    public string path = "";
    public string text = "";
    public float textSpeed = 5f;
    public int textSize = 30;
    public Vector2 textOffset = Vector2.zero;
    public float textDelay = 0f;
}