using UnityEngine;
using System;
using Utilities;


public interface IDecorator
{
    void Decorate(GameObject target);
    void Cancle();
}

public abstract class Decorator : IDecorator
{
    private GameObject _decorator;

    protected abstract GameObject OnDecorator(GameObject target);

    public void Decorate(GameObject target)
    {
        Cancle();

        if (target != null)
        {
            _decorator = OnDecorator(target);
        }
    }

    public void Cancle()
    {
        if (_decorator != null)
        {
            GameObject.DestroyImmediate(_decorator);
        }
    }
}

public class FingerDecorator : Decorator
{
    private Vector3 _offset;

    public FingerDecorator(Vector3 offset)
    {
        this._offset = offset;
    }

    protected override GameObject OnDecorator(GameObject target)
    {
        int depth = NGUITools.CalculateNextDepth(target);
        BoxCollider c = target.GetComponent<BoxCollider>();
        Vector3 center = c == null ? Vector3.zero : c.center;

        GameObject pref = GameCommon.mResources.LoadPrefab("Prefabs/GuideFinger", "PREFAB");
        GameObject inst = GameObject.Instantiate(pref) as GameObject;
        inst.name = "finger_decorator";
        inst.transform.parent = target.transform;
        inst.transform.localScale = Vector3.one;
        inst.transform.localPosition = center + _offset;
        inst.GetComponent<UIWidget>().depth = depth;
        return inst;
    }
}


public class InverseFingerDecorator : Decorator
{
    private Vector3 _offset;

    public InverseFingerDecorator(Vector3 offset)
    {
        this._offset = offset;
    }

    protected override GameObject OnDecorator(GameObject target)
    {
        int depth = NGUITools.CalculateNextDepth(target);
        BoxCollider c = target.GetComponent<BoxCollider>();
        Vector3 center = c == null ? Vector3.zero : c.center;

        GameObject pref = GameCommon.mResources.LoadPrefab("Prefabs/GuideFinger", "PREFAB");
        GameObject inst = GameObject.Instantiate(pref) as GameObject;
        inst.name = "finger_decorator";
        inst.transform.parent = target.transform;
        inst.transform.localScale = Vector3.one;
        inst.transform.localRotation = Quaternion.AngleAxis(180, Vector3.forward);
        inst.transform.localPosition = center + _offset;
        inst.GetComponent<UIWidget>().depth = depth;
        return inst;
    }
}


public class SpotDecorator : Decorator
{
    private Vector3 _offset;

    public SpotDecorator(Vector3 offset)
    {
        this._offset = offset;
    }

    protected override GameObject OnDecorator(GameObject target)
    {
        int depth = NGUITools.CalculateNextDepth(target);
        BoxCollider c = target.GetComponent<BoxCollider>();
        Vector3 center = c == null ? Vector3.zero : c.center;

        GameObject pref = GameCommon.mResources.LoadPrefab("Prefabs/GuideSpot", "PREFAB");
        GameObject inst = GameObject.Instantiate(pref) as GameObject;
        inst.name = "spot_decorator";
        inst.transform.parent = target.transform;
        inst.transform.localScale = Vector3.one;
        inst.transform.localPosition = center + _offset;
        inst.GetComponent<UIWidget>().depth = depth;
        return inst;
    }
}


public class TipDecorator : Decorator
{
    private Vector3 _offset;
    private string _text = "";

    public TipDecorator(Vector3 offset, string text)
    {
        this._offset = offset;
        this._text = text;
    }

    protected override GameObject OnDecorator(GameObject target)
    {
        int depth = NGUITools.CalculateNextDepth(target);
        BoxCollider c = target.GetComponent<BoxCollider>();
        Vector3 center = c == null ? Vector3.zero : c.center;

        GameObject pref = GameCommon.mResources.LoadPrefab("Prefabs/GuideTip", "PREFAB");
        GameObject inst = GameObject.Instantiate(pref) as GameObject;
        inst.name = "tip_decorator";
        inst.transform.parent = target.transform;
        inst.transform.localScale = Vector3.one;
        inst.transform.localPosition = center + _offset;

        UIWidget widget = inst.GetComponent<UIWidget>();
        widget.depth = depth;
        UILabel label = GameCommon.FindComponent<UILabel>(inst, "label");
        label.text = _text;
        label.depth = depth + 1;
        widget.width = label.width + 20;

        TweenPosition t = UITweener.Begin<TweenPosition>(inst, 0.5f);
        t.from = center + _offset + new Vector3(0, -5, 0);
        t.to = center + _offset + new Vector3(0, 5, 0);
        t.style = UITweener.Style.PingPong;
        return inst;
    }
}


public class CompositeDecorator : IDecorator
{
    private IDecorator[] _decorators;

    public CompositeDecorator(params IDecorator[] decorators)
    {
        this._decorators = decorators;
    }

    public void Decorate(GameObject target)
    {
        foreach (var de in _decorators)
        {
            de.Decorate(target);
        }
    }

    public void Cancle()
    {
        foreach (var de in _decorators)
        {
            de.Cancle();
        }
    }
}


public class FingerTipDecorator : CompositeDecorator
{
    public FingerTipDecorator(string tipText)
        : this(new Vector3(0, -50, 0), tipText)
    { }

    public FingerTipDecorator(Vector3 tipOffset, string tipText)
        : this(Vector3.zero, tipOffset, tipText)
    { }

    public FingerTipDecorator(Vector3 fingerOffset, Vector3 tipOffset, string tipText)
        : base(new FingerDecorator(fingerOffset), new TipDecorator(tipOffset, tipText))
    { }
}


public class InverseFingerTipDecorator : CompositeDecorator
{
    public InverseFingerTipDecorator(string tipText)
        : this(new Vector3(0, -80, 0), tipText)
    { }

    public InverseFingerTipDecorator(Vector3 tipOffset, string tipText)
        : this(Vector3.zero, tipOffset, tipText)
    { }

    public InverseFingerTipDecorator(Vector3 fingerOffset, Vector3 tipOffset, string tipText)
        : base(new InverseFingerDecorator(fingerOffset), new TipDecorator(tipOffset, tipText))
    { }
}