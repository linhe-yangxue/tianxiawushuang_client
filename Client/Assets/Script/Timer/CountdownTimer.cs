using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//为全局提供倒计时

public class CountdownTimerData
{
    private string mTimerName = "";     //计时器名称，需唯一
    private bool mIsRunning = false;    //是否在计时
    private float mResetTime = 0.0f;    //重置时倒计时时间，以秒为单位
    private float mLeftTime = 0.0f;     //倒计时剩余时间，以秒为单位
    private Action<CountdownTimerData> mUpdateCallback;             //刷新时回调，参数：倒计时数据；是否为强制结束
    private Action<CountdownTimerData, bool> mCompleteCallback;     //结束时回调，参数：倒计时数据；是否为强制结束

    public string TimerName
    {
        set { mTimerName = value; }
        get { return mTimerName; }
    }
    public bool IsRunning
    {
        set { mIsRunning = value; }
        get { return mIsRunning; }
    }
    public float ResetTime
    {
        set { mResetTime = value; }
        get { return mResetTime; }
    }
    public float LeftTime
    {
        set { mLeftTime = value; }
        get { return mLeftTime; }
    }
    public Action<CountdownTimerData> UpdateCallback
    {
        set { mUpdateCallback = value; }
        get { return mUpdateCallback; }
    }
    public Action<CountdownTimerData, bool> CompleteCallback
    {
        set { mCompleteCallback = value; }
        get { return mCompleteCallback; }
    }
}

public class CountdownTimer : MonoBehaviour
{
    private Dictionary<string, CountdownTimerData> mDicTimer = new Dictionary<string, CountdownTimerData>();

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
        if (mDicTimer == null || mDicTimer.Count <= 0)
            return;

        foreach (KeyValuePair<string, CountdownTimerData> tmpPair in mDicTimer)
        {
            CountdownTimerData tmpData = tmpPair.Value;
            if (!tmpData.IsRunning)
                continue;
            tmpData.LeftTime -= Time.fixedDeltaTime;
            if (tmpData.UpdateCallback != null)
                tmpData.UpdateCallback(tmpData);

//            DEBUG.Log("Timer " + tmpData.TimerName + " left " + tmpData.LeftTime.ToString());

            if (tmpData.LeftTime <= 0.0f)
            {
                tmpData.IsRunning = false;
                if (tmpData.CompleteCallback != null)
                    tmpData.CompleteCallback(tmpData, false);
            }
        }
    }

    /// <summary>
    /// 每个计时器用之前需要注册
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <returns></returns>
    public CountdownTimerData RegisterTimer(string timerName)
    {
        if (mDicTimer == null)
            return null;

        CountdownTimerData tmpData = null;
        if (!mDicTimer.TryGetValue(timerName, out tmpData))
        {
            tmpData = new CountdownTimerData();
            mDicTimer[timerName] = tmpData;
        }
        tmpData.TimerName = timerName;
        return tmpData;
    }
    /// <summary>
    /// 根据计时器名称获取计时器
    /// </summary>
    /// <param name="timerName"></param>
    /// <returns></returns>
    public CountdownTimerData GetTimer(string timerName)
    {
        if (timerName == "" || mDicTimer == null)
            return null;

        CountdownTimerData tmpData = null;
        if (!mDicTimer.TryGetValue(timerName, out tmpData))
            return null;
        return tmpData;
    }

    /// <summary>
    /// 添加计时器刷新回调
    /// </summary>
    /// <param name="timerName"></param>
    /// <param name="callback"></param>
    public void AddTimerUpdateCallback(string timerName, Action<CountdownTimerData> updateCallback)
    {
        CountdownTimerData tmpData = GetTimer(timerName);
        if (tmpData == null)
            return;
        tmpData.UpdateCallback += updateCallback;
    }
    /// <summary>
    /// 移除计时器刷新回调
    /// </summary>
    /// <param name="timerName"></param>
    /// <param name="callback"></param>
    public void RemoveTimerUpdateCallback(string timerName, Action<CountdownTimerData> updateCallback)
    {
        CountdownTimerData tmpData = GetTimer(timerName);
        if (tmpData == null)
            return;
        tmpData.UpdateCallback -= updateCallback;
    }
    /// <summary>
    /// 添加计时器完成回调
    /// </summary>
    /// <param name="timerName"></param>
    /// <param name="callback"></param>
    public void AddTimerCompleteCallback(string timerName, Action<CountdownTimerData, bool> completeCallback)
    {
        CountdownTimerData tmpData = GetTimer(timerName);
        if (tmpData == null)
            return;
        tmpData.CompleteCallback += completeCallback;
    }
    /// <summary>
    /// 移除计时器完成回调
    /// </summary>
    /// <param name="timerName"></param>
    /// <param name="callback"></param>
    public void RemoveTimerCompleteCallback(string timerName, Action<CountdownTimerData, bool> completeCallback)
    {
        CountdownTimerData tmpData = GetTimer(timerName);
        if (tmpData == null)
            return;
        tmpData.CompleteCallback -= completeCallback;
    }

    /// <summary>
    /// 启动计时器
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    public void StartTimer(string timerName)
    {
        StartTimer(timerName, true);
    }
    /// <summary>
    /// 启动计时器
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <param name="isReset">是否重置计时器</param>
    public void StartTimer(string timerName, bool isReset)
    {
        StartTimer(timerName, -1.0f, isReset);
    }
    /// <summary>
    /// 启动计时器
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <param name="fResetTime">每次计时器重置时间，如果小于0，忽略该参数</param>
    /// <param name="isReset">是否重置计时器时间</param>
    public void StartTimer(string timerName, float fResetTime, bool isReset)
    {
        CountdownTimerData tmpData = GetTimer(timerName);
        if (tmpData == null)
            return;
        if (tmpData.IsRunning)
            return;

        if (fResetTime >= 0.0f)
            tmpData.ResetTime = fResetTime;
        if (isReset)
            tmpData.LeftTime = tmpData.ResetTime;
        tmpData.IsRunning = true;
    }
    /// <summary>
    /// 关闭计时器
    /// </summary>
    /// <param name="timerName"></param>
    public void StopTimer(string timerName)
    {
        CountdownTimerData tmpData = GetTimer(timerName);
        if (tmpData == null)
            return;

        tmpData.IsRunning = false;
        if (tmpData.CompleteCallback != null)
            tmpData.CompleteCallback(tmpData, true);
    }
    /// <summary>
    /// 判断计时器是否在运行
    /// </summary>
    /// <param name="timerName"></param>
    /// <returns></returns>
    public bool IsRunning(string timerName)
    {
        CountdownTimerData tmpData = GetTimer(timerName);
        if (tmpData == null)
            return false;
        return tmpData.IsRunning;
    }
}
