using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;
using System.Collections.Generic;

/// <summary>
/// 进度条方向
/// </summary>
public enum GAMELOADING_PROGRESS_DIRECTION
{
    FORWARD,    //向前
    BACK        //向后
}

/// <summary>
/// 游戏加载界面
/// </summary>
public partial class GameLoading : MonoBehaviour, ILoading, ICoroutineOperation
{
    protected bool m_IsLoading = false;            //是否在下载
    protected bool m_HasLoadError = false;         //是否有下载错误
    protected string m_LoadError = "";             //下载错误信息

    protected bool m_IsSimulateProgress = false;    //是否模拟加载进度

    protected LoadingProgressParam m_RealProgress = new LoadingProgressParam(); //当前真实进度，当前进度以此值为准
    private LoadingProgressParam m_Progress = new LoadingProgressParam();       //当前进度，在Update中mRealProgress会根据mProgress改变，最终和mProgress值相同
	[SerializeField]
    protected float m_TimeDeltaLimit = 0.3f;        //当Time.deltaTime大于此值时，不更新进度
    [SerializeField]
    protected float m_ProgressSpeed = 1.0f; //进度速度，以秒为单位
    [SerializeField]
    protected UISlider m_ProgressSlider;    //进度条
    [SerializeField]
    protected GameObject m_ProgressAnimParent;         //进度动画父节点
    [SerializeField]
    protected List<GameObject> m_ListPrefabProgressAnim = new List<GameObject>();     //进度动画
    protected GAMELOADING_PROGRESS_DIRECTION m_ProgressDirection;       //进度条方向

    private LoadingProgressParam m_ProgressDelta = new LoadingProgressParam();      //进度改变量

    void Awake()
    {
        CoroutineOperation = this;
        m_ProgressDirection = GAMELOADING_PROGRESS_DIRECTION.FORWARD;

        _OnAwake();
    }

    // Use this for initialization
    void Start()
    {
        if (m_ProgressSlider == null)
            m_ProgressSlider = _GetProgressSlider();
        if (m_ProgressAnimParent != null)
            _CreateProgressAnim();

        _OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        _OnUpdate();
    }

    void LateUpdate()
    {
        if (m_IsLoading)
        {
            m_ProgressDelta.Init();
            float tmpDeltaTime = Time.deltaTime;
            if (m_IsSimulateProgress)
            {
                if (m_RealProgress.Progress != m_Progress.Progress && tmpDeltaTime <= m_TimeDeltaLimit)
                {
                    m_RealProgress.BytesTotal = m_Progress.BytesTotal;
                    m_ProgressDelta.Progress = m_ProgressSpeed * tmpDeltaTime;
                    if ((m_ProgressDirection == GAMELOADING_PROGRESS_DIRECTION.FORWARD && m_RealProgress.Progress + m_ProgressDelta.Progress > m_Progress.Progress) ||
                        (m_ProgressDirection == GAMELOADING_PROGRESS_DIRECTION.BACK && m_RealProgress.Progress - m_ProgressDelta.Progress < m_Progress.Progress))
                    {
                        //如果增加进度后超过目标进度，将当前进度设置为目标进度，并更新进度变化量
                        m_ProgressDelta.Progress = m_Progress.Progress - m_RealProgress.Progress;
                        m_ProgressDelta.BytesLoaded = m_Progress.BytesLoaded - m_RealProgress.BytesLoaded;
                        m_ProgressDelta.BytesTotal = m_Progress.BytesTotal - m_RealProgress.BytesTotal;
                        m_RealProgress.CopyFrom(m_Progress);
                    }
                    else
                    {
                        int tmpMultiSign = (m_ProgressDirection == GAMELOADING_PROGRESS_DIRECTION.FORWARD) ? 1 : -1;
                        m_RealProgress.Progress += (m_ProgressDelta.Progress * tmpMultiSign);
                        m_ProgressDelta.BytesLoaded = (long)((float)m_RealProgress.BytesTotal * m_ProgressDelta.Progress);
                        m_RealProgress.BytesLoaded += m_ProgressDelta.BytesLoaded;
                    }
                }
            }
            else
            {
                m_ProgressDelta.Progress = m_Progress.Progress - m_RealProgress.Progress;
                //真实加载
                m_RealProgress.CopyFrom(m_Progress);
            }
            _SetProgressUIValue(m_RealProgress);
            if (m_ProgressDelta.Progress != 0.0f)
            {
                if (m_ProgressAnimParent != null)
                {
                    Vector3 tmpLocalPos = m_ProgressAnimParent.transform.localPosition;
                    tmpLocalPos.x = _GetSliderLength() * m_RealProgress.Progress;
                    m_ProgressAnimParent.transform.localPosition = tmpLocalPos;
                }
                //进度有变化
                _OnProgressChanged(m_ProgressDelta);
            }
        }

        _OnLateUpdate();
    }

    protected virtual void _OnAwake()
    {
    }
    protected virtual void _OnStart()
    {
    }
    protected virtual void _OnUpdate()
    {
    }
    protected virtual void _OnLateUpdate()
    {
    }

    /// <summary>
    /// 用于加载计时
    /// </summary>
    private static Stopwatch m_Stopwatch = new Stopwatch();
    protected static void _ResetWatch()
    {
        if (m_Stopwatch == null)
            return;

        m_Stopwatch.Reset();
    }
    protected static void _StartWatch()
    {
        if (m_Stopwatch == null)
            return;

        m_Stopwatch.Start();
    }
    protected static void _StopWatch()
    {
        if (m_Stopwatch == null)
            return;

        m_Stopwatch.Stop();
    }
    protected static long _WatchElapsedMilliseconds()
    {
        if (m_Stopwatch == null)
            return 0;

        return m_Stopwatch.ElapsedMilliseconds;
    }
    private static bool m_IsLogLoadTime = true;        //是否记录加载时间

    public float ProgressSpeed
    {
        set { m_ProgressSpeed = value; }
        get { return m_ProgressSpeed; }
    }

    /// <summary>
    /// 协同操作
    /// </summary>
    public ICoroutineOperation CoroutineOperation { set; get; }

    /// <summary>
    /// 开始异步加载
    /// </summary>
    /// <param name="path">加载路径</param>
    /// <param name="progressCallback">加载进度回调</param>
    /// <param name="param">加载参数</param>
    /// <returns></returns>
    public IEnumerator StartLoad(string path, Action<LoadingProgressParam> progressCallback, object param)
    {
        if (m_IsLoading)
            yield break;

        m_IsLoading = true;
        m_HasLoadError = false;
        m_LoadError = "";

        LoadingProgressParam tmpLastRealProgress = new LoadingProgressParam();
        m_RealProgress.Init();
        m_Progress.Init();
        _SetProgressUIValue(m_RealProgress);
        _OnStartLoad(param);
        _OnProgressChanged(new LoadingProgressParam());

        if (m_IsLogLoadTime)
        {
            //开始计时
            _ResetWatch();
            _StartWatch();
        }

        while (
            (m_ProgressDirection == GAMELOADING_PROGRESS_DIRECTION.FORWARD && m_RealProgress.Progress < 1.0f) ||
            (m_ProgressDirection == GAMELOADING_PROGRESS_DIRECTION.BACK && m_RealProgress.Progress > 0.0f))
        {
            //只有当进度有变化时调用
            if (tmpLastRealProgress != m_RealProgress && progressCallback != null)
                progressCallback(m_RealProgress);
            tmpLastRealProgress.CopyFrom(m_RealProgress);
            yield return null;
        }

        //加载完成
        m_IsLoading = false;
        _OnLoadFinished();

        if (m_IsLogLoadTime)
        {
            _StopWatch();
//            DEBUG.LogError(this.GetType().ToString() + " - Loading time = " + GameLoading._WatchElapsedMilliseconds());
        }
    }
    /// <summary>
    /// 开始加载时调用
    /// </summary>
    /// <param name="param"></param>
    protected virtual void _OnStartLoad(object param)
    {
    }
    /// <summary>
    /// 停止加载
    /// </summary>
    public void StopLoad()
    {
        if (!m_IsLoading)
            return;
        m_IsLoading = false;
        _OnStopLoad();
    }
    /// <summary>
    /// 当停止加载时调用
    /// </summary>
    protected virtual void _OnStopLoad()
    {
    }
    /// <summary>
    /// 加载完成
    /// </summary>
    protected virtual void _OnLoadFinished()
    {
    }
    /// <summary>
    /// 当前加载进度
    /// </summary>
    /// <returns></returns>
    public LoadingProgressParam LoadProgress()
    {
        return m_RealProgress;
    }
    /// <summary>
    /// 是否在加载中
    /// </summary>
    /// <returns></returns>
    public bool IsLoading()
    {
        return m_IsLoading;
    }
    /// <summary>
    /// 是否加载完成
    /// </summary>
    /// <returns></returns>
    public bool IsComplete()
    {
        return (
            (m_ProgressDirection == GAMELOADING_PROGRESS_DIRECTION.FORWARD && m_RealProgress.Progress >= 1.0f)  ||
            (m_ProgressDirection == GAMELOADING_PROGRESS_DIRECTION.BACK && m_RealProgress.Progress <= 0.0f));
    }
    /// <summary>
    /// 是否加载成功
    /// </summary>
    /// <returns></returns>
    public bool IsSuccess()
    {
        return (IsComplete() && !m_HasLoadError);
    }
    /// <summary>
    /// 错误信息
    /// </summary>
    /// <returns></returns>
    public string ErrorInfo()
    {
        return m_LoadError;
    }

    /// <summary>
    /// 设置进度
    /// </summary>
    public void SetLoadingProgress(LoadingProgressParam progress)
    {
        m_Progress.CopyFrom(progress);
    }

    /// <summary>
    /// 设置进度条值
    /// </summary>
    /// <param name="progress"></param>
    protected virtual void _SetProgressUIValue(LoadingProgressParam progress)
    {
        if (m_ProgressSlider == null)
            return;
        m_ProgressSlider.value = progress.Progress;
    }

    /// <summary>
    /// 获取进度条控件
    /// </summary>
    /// <returns></returns>
    protected virtual UISlider _GetProgressSlider()
    {
        UISlider tmpSlider = transform.GetComponentInChildren<UISlider>();
        return tmpSlider;
    }
    /// <summary>
    /// 获取进度条长度
    /// </summary>
    /// <returns></returns>
    protected virtual float _GetSliderLength()
    {
        if (m_ProgressSlider == null)
            return 0.0f;

        return (m_ProgressSlider.backgroundWidget.width - 10.0f);
    }
    /// <summary>
    /// 创建进度动画
    /// </summary>
    protected virtual void _CreateProgressAnim()
    {
        if (m_ProgressAnimParent == null ||
            m_ListPrefabProgressAnim == null || m_ListPrefabProgressAnim.Count <= 0)
            return;

        for (int i = 0, count = m_ListPrefabProgressAnim.Count; i < count; i++)
        {
            GameObject tmpPrefabAnim = m_ListPrefabProgressAnim[i];
            if(tmpPrefabAnim == null)
                continue;
            GameObject tmpGOAnim = Instantiate(tmpPrefabAnim) as GameObject;
            if (tmpGOAnim == null)
                continue;
            Vector3 tmpLocalPos = tmpGOAnim.transform.localPosition;
            Quaternion tmpLocalRot = tmpGOAnim.transform.localRotation;
            Vector3 tmpLocalScale = tmpGOAnim.transform.localScale;
            tmpGOAnim.transform.parent = m_ProgressAnimParent.transform;
            tmpGOAnim.transform.localPosition = tmpLocalPos;
            tmpGOAnim.transform.localRotation = tmpLocalRot;
            tmpGOAnim.transform.localScale = tmpLocalScale;
        }
    }
    /// <summary>
    /// 当进度有改变时调用，可在此实现进度改变时效果
    /// </summary>
    /// <param name="delta"></param>
    protected virtual void _OnProgressChanged(LoadingProgressParam delta)
    {
    }

    /// <summary>
    /// 执行延迟操作
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="callback"></param>
    protected void _DelayAction(float delay, Action callback)
    {
        StartCoroutine(__DoDelayAction(delay, callback));
    }
    private IEnumerator __DoDelayAction(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        if (callback != null)
            callback();
    }
}
