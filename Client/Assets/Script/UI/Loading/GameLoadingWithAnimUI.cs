using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;

/// <summary>
/// 进游戏后加载界面基类
/// </summary>
public class GameLoadingWithAnimUI : GameLoading
{
    [SerializeField]
    protected UILabel m_LBProgress;      //进度数字

    protected Action<object> mStartCallback;
    protected Action mStopCallback;
    protected Action<LoadingProgressParam> mProgressChangedCallback;
    protected Action mFinishedCallback;

    protected override void _OnAwake()
    {
        ChangeBackground();
    }

    protected override void _OnStart()
    {
        if (m_LBProgress == null)
            m_LBProgress = _GetProgressLabel();
    }

    public Action<object> StartCallback
    {
        set { mStartCallback = value; }
        get { return mStartCallback; }
    }
    public Action StopCallback
    {
        set { mStopCallback = value; }
        get { return mStopCallback; }
    }
    public Action<LoadingProgressParam> ProgressChagnedCallback
    {
        set { mProgressChangedCallback = value; }
        get { return mProgressChangedCallback; }
    }
    public Action FinishedCallback
    {
        set { mFinishedCallback = value; }
        get { return mFinishedCallback; }
    }

    /// <summary>
    /// 改变背景
    /// </summary>
    public virtual void ChangeBackground()
    {
        if (DataCenter.mGlobalConfig == null)
            return;

        int iMaxNum = DataCenter.mGlobalConfig.GetData("LOADING_BACKGROUND_NUMBER", "VALUE");
        int iIndex = UnityEngine.Random.Range(0, iMaxNum);
        Transform tmpTrans = transform.Find("group");
        if (tmpTrans == null)
            return;
        UITexture texture = tmpTrans.GetComponent<UITexture>();
        if (texture != null)
        {
            string strPicName = "StaticResources/textures/UItextures/Loading/" + "battle_loading_" + iIndex.ToString();
            texture.mainTexture = Resources.Load(strPicName, typeof(Texture)) as Texture;//GameCommon.LoadTexture(strPicName, LOAD_MODE.RESOURCE);//

            if (texture.mainTexture == null)
            {
                DEBUG.LogError("GameLoadingWithAnimUI - ChangeBackground : " + strPicName + " doesn't exist.");
                DEBUG.LogError("texture.mainTexture == null");
            }
            else 
            {
                GameCommon.ForceUpdateUI(texture.gameObject);
            }
        }
    }

    /// <summary>
    /// 获取进度条数值标签
    /// </summary>
    /// <returns></returns>
    protected virtual UILabel _GetProgressLabel()
    {
        if (m_ProgressSlider == null)
            return null;
        UILabel tmpLB = GameCommon.FindComponent<UILabel>(m_ProgressSlider.gameObject, "progress_value");
        return tmpLB;
    }

    /// <summary>
    /// 开始加载时调用
    /// </summary>
    /// <param name="param"></param>
    protected override void _OnStartLoad(object param)
    {
        StartCoroutine(__StartLoad(param));
    }
    private IEnumerator __StartLoad(object param)
    {
        //延迟使背景图加载
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (mStartCallback != null)
            mStartCallback(param);
    }
    /// <summary>
    /// 当停止加载时调用
    /// </summary>
    protected override void _OnStopLoad()
    {
        if (mStopCallback != null)
            mStopCallback();
    }
    /// <summary>
    /// 加载完成
    /// </summary>
    protected override void _OnLoadFinished()
    {
        //modified by xuke
        //GlobalModule.DoOnNextUpdate(() =>
        //{
        //    if (mFinishedCallback != null)
        //        mFinishedCallback();
        //});
        //-----
        StartCoroutine(IE_FinishdedCallback());
    }
    private IEnumerator IE_FinishdedCallback()
    {
        yield return null;
        if (mFinishedCallback != null)
            mFinishedCallback();
    }

    /// <summary>
    /// 当进度有改变时调用，可在此实现进度改变时效果
    /// </summary>
    /// <param name="delta"></param>
    protected override void _OnProgressChanged(LoadingProgressParam delta)
    {
        float tmpProgress = LoadProgress().Progress;
        if (m_LBProgress != null)
            m_LBProgress.text = Mathf.FloorToInt(tmpProgress * 100.0f).ToString() + "%";

        if (mProgressChangedCallback != null)
            mProgressChangedCallback(delta);
    }

    /// <summary>
    /// 设置进度条值
    /// </summary>
    /// <param name="progress"></param>
    protected override void _SetProgressUIValue(LoadingProgressParam progress)
    {
        if (m_ProgressSlider == null)
            return;
        m_ProgressSlider.value = 1.0f - progress.Progress;
    }
}

/// <summary>
/// 进游戏后加载界面窗口基类
/// </summary>
public class GameLoadingWithAnimWindow : tWindow
{
    protected GameLoadingWithAnimUI mGameLoading;
    private Action mFinishedCallback;               //加载完成时回调
    protected bool mIsAutoClose = true;             //加载完成后是否自动关闭窗口

    protected bool mIsRecordTimer = false;          //是否记录加载时间
    protected Stopwatch mTimer;

    private LoadingProgressParam mTmpProgressParam = new LoadingProgressParam();        //防止每次刷新都new一个对象

    public override void Open(object param)
    {
        base.Open(param);

        if (mGameLoading == null)
        {
            if (_Timer != null)
            {
                _Timer.Reset();
                _Timer.Start();
            }
            mGameLoading = mGameObjUI.GetComponent<GameLoadingWithAnimUI>();
            if (mGameLoading != null)
            {
                mGameLoading.StartCallback = _OnStartLoading;
                mGameLoading.StopCallback = _OnStopLoading;
                mGameLoading.ProgressChagnedCallback = _OnProgressChanged;
                mGameLoading.FinishedCallback = () =>
                {
                    _OnLoadFinished();
                    if (mFinishedCallback != null)
                        mFinishedCallback();
                    if (mIsAutoClose && IsOpen())
                        this.set("CLOSE", true);
                };
            }
        }

        if (mGameLoading != null)
            this.DoCoroutine(mGameLoading.StartLoad("", null, null));
    }

    public override void Close()
    {
        if (_Timer != null && _Timer.IsRunning)
        {
            _Timer.Stop();
            DEBUG.Log(this.GetType().ToString() + " load time is " + _Timer.ElapsedMilliseconds + "ms");
        }

        base.Close();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "CHANGE_BACKGROUND":
                {
                    if (mGameLoading != null)
                        mGameLoading.ChangeBackground();
                } break;
        }
    }

    protected Stopwatch _Timer
    {
        get
        {
            if (!mIsRecordTimer)
                return null;

            if (mTimer == null)
                mTimer = new Stopwatch();
            return mTimer;
        }
    }

    public Action FinishedCallback
    {
        set { mFinishedCallback = value; }
        get { return mFinishedCallback; }
    }

    protected LoadingProgressParam _LoadingProgressParam
    {
        get
        {
            if (mTmpProgressParam == null)
                mTmpProgressParam = new LoadingProgressParam();
            return mTmpProgressParam;
        }
    }
    /// <summary>
    /// 设置当前加载进度
    /// </summary>
    /// <param name="progress"></param>
    protected void _SetLoadingProgress(float progress)
    {
        if (mGameLoading == null)
            return;
        _LoadingProgressParam.Progress = progress;
        mGameLoading.SetLoadingProgress(_LoadingProgressParam);
    }

    /// <summary>
    /// 在开始加载时被调用
    /// </summary>
    protected virtual void _OnStartLoading(object param)
    {
    }
    /// <summary>
    /// 在主动停止加载时被调用
    /// </summary>
    protected virtual void _OnStopLoading()
    {
    }
    /// <summary>
    /// 进度改变时被调用
    /// </summary>
    /// <param name="delta"></param>
    protected virtual void _OnProgressChanged(LoadingProgressParam delta)
    {
    }
    /// <summary>
    /// 在加载完成时被调用
    /// </summary>
    protected virtual void _OnLoadFinished()
    {
    }
}
