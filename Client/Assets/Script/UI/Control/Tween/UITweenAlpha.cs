//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tween the object's alpha.
/// </summary>

public class UITweenAlpha : UITweenMultiTargets
{
#if UNITY_3_5
	public float from = 1f;
	public float to = 1f;
#else
    [Range(0f, 1f)]
    public float from = 1f;
    [Range(0f, 1f)]
    public float to = 1f;
#endif

    protected float mCurrValue = 0.0f;

    /// <summary>
    /// Tween's current value.
    /// </summary>

    public float value
    {
        get
        {
            return mCurrValue;
        }
        set
        {
            if (m_Targets == null)
                return;

            mCurrValue = value;
            for (int i = 0, count = m_Targets.Count; i < count; i++)
                m_Targets[i].alpha = mCurrValue;
        }
    }

    /// <summary>
    /// Tween the value.
    /// </summary>

    protected override void OnUpdate(float factor, bool isFinished) { value = Mathf.Lerp(from, to, factor); }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public UITweenAlpha Begin(GameObject go, float duration, float alpha)
    {
        UITweenAlpha comp = UITweener.Begin<UITweenAlpha>(go, duration);
        comp.from = comp.value;
        comp.to = alpha;

        if (duration <= 0f)
        {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }

    public override void SetStartToCurrentValue() { from = value; }
    public override void SetEndToCurrentValue() { to = value; }
}
