using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlphaTweenAnim : MonoBehaviour 
{
    [SerializeField]
    public float mDelayStart = 0f;   //> 延迟开始播放时间
    [SerializeField]
    public float mStepDelay = 0f;    //> 每项延迟播放时间
    [SerializeField]
    public float mStepDuration = 1f; //> 每项持续时间
    [SerializeField]
    public float mFromAlpha = 0f;
    [SerializeField]
    public float mToAlpha = 1f;
    [SerializeField]
    public AnimationCurve mAnimCurve;
    private bool mPlayFlag = true;
    private List<Transform> mTweenChildren = new List<Transform>();
    private List<TweenAlpha> mTweenAlpha = new List<TweenAlpha>();
    private Vector3 mFinalPos = Vector3.zero;

    void Awake() 
    {
        mFinalPos = transform.localPosition;
    }
    public void ResetPlayFlag()
    {
        mPlayFlag = true;
    }
    public void PlayAlphaTweenAnim() 
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
        mTweenAlpha.Clear();
        for (int i = 0, count = this.transform.childCount; i < count; i++)
        {
            mTweenChildren.Add(transform.GetChild(i));
        }
        for (int i = 0, count = mTweenChildren.Count; i < count; i++)
        {
            mTweenAlpha.Add(InitCellInfo(i));
        }
    }
    private TweenAlpha InitCellInfo(int kCellIndex)
    {
        TweenAlpha _tweenAlpha = null;
        _tweenAlpha = mTweenChildren[kCellIndex].GetComponent<TweenAlpha>();
        if (_tweenAlpha == null) 
        {
            _tweenAlpha = mTweenChildren[kCellIndex].gameObject.AddComponent<TweenAlpha>();
            _tweenAlpha.enabled = false;        
        }
        
        _tweenAlpha.from = mFromAlpha;
        _tweenAlpha.to = mToAlpha;
        _tweenAlpha.duration = mStepDuration;
        _tweenAlpha.animationCurve = mAnimCurve;
        if (mStepDelay != 0)
        {
            _tweenAlpha.delay = kCellIndex * mStepDelay;
        }
        return _tweenAlpha;
    }
    private void InitOriginPos()
    {
        for (int i = 0, count = mTweenAlpha.Count; i < count; i++)
        {
            mTweenAlpha[i].ResetToBeginning();
            mTweenAlpha[i].enabled = false;
        }
        //added by xuke
        this.transform.localPosition = mFinalPos;
        //end
    }
    private void PlayOpenTweenAnim()
    {
        GlobalModule.DoLater(() =>
        {
            for (int i = 0, count = mTweenChildren.Count; i < count; i++)
            {
                if (mTweenAlpha[i] != null)
                    mTweenAlpha[i].PlayForward();
            }
        }, mDelayStart);
    }
}
