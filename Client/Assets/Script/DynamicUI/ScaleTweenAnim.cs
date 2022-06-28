using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 缩放Tween动画
/// </summary>
public class ScaleTweenAnim : MonoBehaviour 
{
    [SerializeField]
    public float mDelayStart = 0f;   //> 延迟开始播放时间
    [SerializeField]
    public float mStepDelay = 0f;    //> 每项延迟播放时间
    [SerializeField]
    public float mStepDuration = 1f; //> 每项持续时间
    [SerializeField]
    public Vector3 mFrom = Vector3.zero;
    [SerializeField]
    public Vector3 mTo = Vector3.zero;
    [SerializeField]
    public AnimationCurve mAnimCurve;
    private bool mPlayFlag = true;
    private List<Transform> mTweenChildren = new List<Transform>();
    private List<TweenScale> mTweenScript = new List<TweenScale>();
    public void ResetPlayFlag()
    {
        mPlayFlag = true;
    }
    public void PlayScaleTweenAnim() 
    {
        if (!CommonParam.mOpenDynamicUI)
            return;
        InitScriptInfo();
        InitOriginPos();
        PlayOpenTweenAnim();
    }
    private void InitScriptInfo() 
    {
        mTweenChildren.Clear();
        mTweenScript.Clear();
        for (int i = 0, count = this.transform.childCount; i < count; i++)
        {
            mTweenChildren.Add(transform.GetChild(i));
        }
        for (int i = 0, count = mTweenChildren.Count; i < count; i++)
        {
            mTweenScript.Add(InitCellInfo(i));
        }
    }
    private TweenScale InitCellInfo(int kCellIndex)
    {
        TweenScale _tweenScale = null;
        _tweenScale = mTweenChildren[kCellIndex].GetComponent<TweenScale>();
        if (_tweenScale == null) 
        {
            _tweenScale = mTweenChildren[kCellIndex].gameObject.AddComponent<TweenScale>();
            _tweenScale.enabled = false;        
        }
        _tweenScale.from = mFrom;
        _tweenScale.to = mTo;
        _tweenScale.duration = mStepDuration;
        _tweenScale.animationCurve = mAnimCurve;
        if (mStepDelay != 0)
        {
            _tweenScale.delay = kCellIndex * mStepDelay;
        }
        return _tweenScale;
    }
    private void InitOriginPos()
    {
        for (int i = 0, count = mTweenScript.Count; i < count; i++)
        {
            mTweenScript[i].ResetToBeginning();
            mTweenScript[i].enabled = false;
        }
        //added by xuke
        this.gameObject.SetActive(true);
        //end
    } 
    private void PlayOpenTweenAnim()
    {
        GlobalModule.DoLater(() =>
        {
            for (int i = 0, count = mTweenChildren.Count; i < count; i++)
            {
                if (mTweenScript[i] != null)
                    mTweenScript[i].PlayForward();
            }
        }, mDelayStart);
    }
}



