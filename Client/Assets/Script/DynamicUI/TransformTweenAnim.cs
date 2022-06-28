using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 移动方向
/// </summary>
public enum MoveDirection 
{
    TO_LEFT = 0,    
    TO_RIGHT,
}
/// <summary>
/// 移动Tween动画
/// </summary>
public class TransformTweenAnim : MonoBehaviour
{
    [SerializeField]
    public float mDelayStart = 0f;   //> 延迟开始播放时间
    [SerializeField]
    public float mStepDelay = 0f;    //> 每项延迟播放时间
    [SerializeField]
    public float mStepDuration = 1f; //> 每项持续时间
    [SerializeField]
    public Vector3 mOriginPos = Vector3.zero;   //> 起始位置
    [SerializeField]
    public AnimationCurve mAnimCurve;
    private List<Transform> mTweenChildren = new List<Transform>();
    private List<TweenPosition> mTweenPos = new List<TweenPosition>();

    private Vector3 mFinalyPos = Vector3.zero;
    void Awake() 
    {
        mFinalyPos = transform.localPosition;
    }
    public void PlayTweenAnim() 
    {
        if (!CommonParam.mOpenDynamicUI)
            return;
        InitScriptInfo();
        InitOriginPos();
        PlayOpenTweenAnim();
    }

    private bool mHasInited = false;
    /// <summary>
    /// Step1: 初始化Tween动画信息
    /// </summary>
    public void PreInitTweenAnimInfo() 
    {
        if (!CommonParam.mOpenDynamicUI)
            return;
        InitScriptInfo();
        InitOriginPos();

        mHasInited = true;
    }

    /// <summary>
    /// Step2: 播放动画
    /// </summary>
    public void PlayAfterInit() 
    {
        if (mHasInited)
        {
            PlayOpenTweenAnim();        
        }
    }

    [ContextMenu("Set Origin Position")]
    private void SetOriginPos() 
    {
        mOriginPos = this.transform.localPosition;
    }
    private void InitScriptInfo() 
    {
        mTweenChildren.Clear();
        mTweenPos.Clear();
        for (int i = 0, count = this.transform.childCount; i < count; i++) 
        {
            mTweenChildren.Add(transform.GetChild(i));
        }    
        for (int i = 0, count = mTweenChildren.Count; i < count; i++) 
        {
            mTweenPos.Add( InitCellInfo(i));
        }
    }
    private TweenPosition InitCellInfo(int kCellIndex)
    {
        TweenPosition _tweenPos = null; 
        _tweenPos = mTweenChildren[kCellIndex].GetComponent<TweenPosition>();
        if (_tweenPos == null) 
        {
            _tweenPos = mTweenChildren[kCellIndex].gameObject.AddComponent<TweenPosition>();
            _tweenPos.enabled = false;            
        }
           
        _tweenPos.SetFirstToPos(mTweenChildren[kCellIndex].localPosition);
        _tweenPos.from = mOriginPos;
        _tweenPos.duration = mStepDuration;
        _tweenPos.animationCurve = mAnimCurve;
        if (mStepDelay != 0) 
        {
            _tweenPos.delay = kCellIndex * mStepDelay;
        }
        return _tweenPos;
    }

    private void InitOriginPos() 
    {
        for (int i = 0, count = mTweenPos.Count; i < count; i++) 
        {
            mTweenPos[i].ResetToBeginning();
            mTweenPos[i].enabled = false;
        }
        this.transform.localPosition = mFinalyPos;
    }

    private void PlayOpenTweenAnim() 
    {
        GlobalModule.DoLater(() =>
        {
            for (int i = 0, count = mTweenChildren.Count; i < count; i++) 
            {
                if (mTweenPos[i] != null) 
                {
                    //mTweenPos[i].ResetToBeginning();
                    mTweenPos[i].PlayForward();                
                }
            }
        }, mDelayStart);
    }
}
