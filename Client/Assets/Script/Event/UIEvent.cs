using UnityEngine;
using System;
using Logic;

public class TM_WaitNetEvent : CEvent
{
	GameObject mWaitUIEffect;

	public int waiting {get; set;}

	public void StartWatch()
	{
		waiting++;
		if (!GetFinished())
			return;
		
		SetFinished(false);

        //立即打开遮罩层
        OpenUICoverLayer();

		WaitTime(StaticDefine.netDelayTollarence);
	}

    void OpenUICoverLayer()
    {
        if (mWaitUIEffect == null)
        {
            GameObject uiCamera = GameCommon.FindUI("CenterAnchor");
            mWaitUIEffect = GameCommon.LoadUIPrefabs("wait_time_effect", uiCamera);
            mWaitUIEffect.transform.localPosition = new Vector3(0, 0, -2000.0f);
            mWaitUIEffect.transform.localScale = Vector3.one;
        }

        mWaitUIEffect.SetActive(true);

        SpriteRenderer waitingSprit = mWaitUIEffect.GetComponentInChildren<SpriteRenderer>();

        if(waitingSprit != null)
            waitingSprit.enabled = false;
    }

    void CloseUICoverLayer()
    {
        if (mWaitUIEffect != null)
        {
			//GameObject.Destroy(mWaitUIEffect);
		    //mWaitUIEffect = null;
            mWaitUIEffect.SetActive(false);
        }
    }

	public void FinishWatch()
	{
        /*
		waiting -= 1;
		if(waiting <= 0)
		{
			waiting = 0;
			Finish();
		}
        **/
        waiting = 0;
        Finish();
	}

	public override void _OnOverTime ()
	{
		var waitingSprit = mWaitUIEffect.GetComponentInChildren<SpriteRenderer>();

        if(waitingSprit != null)
            waitingSprit.enabled = true;
	}

	public override bool _DoEvent()
	{
        //OpenUICoverLayer();

		return true;
	}

	public override bool _OnFinish()
	{
		CloseUICoverLayer();
		return true;
	}
}


public class TM_Tweener : CEvent
{
    public float mBefore = 0f;
    public float mDuration = 1f;
    public float mAfter = 0f;

    public GameObject mTarget;

    public Action<GameObject, float> tween;
    public Action<GameObject> onElapsed;
    public Func<float, float> curve;

    private float t = 0f;

    public override bool _DoEvent()
    {
        if (mTarget == null || !mTarget.activeSelf || tween == null)
            return false;

        t = 0f;
        tween(mTarget, Factor(t));
        StartUpdate();
        return true;
    }

    public override bool Update(float secondTime)
    {
        t += secondTime;

        if (mTarget == null || !mTarget.activeSelf)
        {
            return false;
        }

        if (t > mBefore + mDuration + mAfter)
        {
            tween(mTarget, Factor(1f));

            if (onElapsed != null)
                onElapsed(mTarget);

            return false;
        }
        else if (t > mBefore + mDuration)
        {
            tween(mTarget, Factor(1f));
        }
        else if (t > mBefore)
        {
            tween(mTarget, Factor((t - mBefore) / mDuration));
        }
        else // t <= mBefore
        {
            tween(mTarget, Factor(0f));
        }

        return true;
    }

    private float Factor(float t)
    {
        return curve == null ? t : curve(t);
    }
}


public abstract class TM_TweenAlpha : TM_Tweener
{
    private UIPanel panel;
    private UIWidget widget;

    protected abstract float Alpha(float factor);

    public TM_TweenAlpha()
    {
        tween += Tween;
    }

    public override bool _DoEvent()
    {
        if (mTarget != null)
        {
            panel = mTarget.GetComponent<UIPanel>();
            widget = mTarget.GetComponent<UIWidget>();
        }
        return base._DoEvent();
    }

    private void Tween(GameObject target, float factor)
    {
        if (panel != null)
            panel.alpha = Alpha(factor);
        else if (widget != null)
            widget.alpha = Alpha(factor);
    }
}


public class TM_FadeIn : TM_TweenAlpha
{
    protected override float Alpha(float factor)
    {
        return factor;
    }
}


public class TM_FadeOut : TM_TweenAlpha
{
    protected override float Alpha(float factor)
    {
        return 1 - factor;
    }
}


public class TM_FadeInOut : CEvent
{
    public float mBefore = 0f;
    public float mFadeIn = 1f;
    public float mKeep = 1f;
    public float mFadeOut = 1f;
    public float mAfter = 0f;

    public GameObject mTarget;

    public Action<GameObject> onFinish;

    public override bool _DoEvent()
    {
        if (mTarget == null)
            return false;

        TM_FadeIn fadeInEvent = EventCenter.Start("TM_FadeIn") as TM_FadeIn;
        fadeInEvent.mTarget = mTarget;
        fadeInEvent.mBefore = mBefore;
        fadeInEvent.mDuration = mFadeIn;
        fadeInEvent.mAfter = mKeep;
        fadeInEvent.onElapsed += FadeOut;
        fadeInEvent.DoEvent();
        return true;
    }

    private void FadeOut(GameObject target)
    {
        TM_FadeOut fadeOutEvent = EventCenter.Start("TM_FadeOut") as TM_FadeOut;
        fadeOutEvent.mTarget = mTarget;
        fadeOutEvent.mBefore = 0f;
        fadeOutEvent.mDuration = mFadeOut;
        fadeOutEvent.mAfter = mAfter;
        fadeOutEvent.onElapsed += onFinish;
        fadeOutEvent.DoEvent();
    }
}


public class TM_TweenPosition : TM_Tweener
{
    public Vector3 mFrom = Vector3.zero;
    public Vector3 mTo = Vector3.zero;

    public TM_TweenPosition()
    {
        this.tween += Tween;
    }

    private void Tween(GameObject target, float factor)
    {
        target.transform.localPosition = mFrom + (mTo - mFrom) * factor;
    }
}


public class TM_TweenScale : TM_Tweener
{
    public Vector3 mFrom = Vector3.one;
    public Vector3 mTo = Vector3.one;

    public TM_TweenScale()
    {
        this.tween += Tween;
    }

    private void Tween(GameObject target, float factor)
    {
        target.transform.localScale = mFrom + (mTo - mFrom) * factor;
    }
}