using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 角色计时器类型
/// </summary>
public enum ROLE_INFO_TIMER_TYPE
{
    NONE = -1,
    STAMINA,                //体力
    SPIRIT,                 //精力
    BEAT_DEMON_CARD         //降魔令
}

public class RoleInfoTimerManager
{
    private static RoleInfoTimerManager msInstance;

    //计时器名称
    private string[] mTimerName = new string[] {
        "StaminaRecoverTimer",
        "SpiritRecoverTimer",
        "BeatDemonCardRecoverTimer"
    };
    //计时器恢复时间
    private int[] mRecoverDelta = new int[] {
        6 * 60, 30 * 60, 60 * 60
    };
    //恢复上限
    private int[] mRecoverMaxValue = new int[] {
        100, 30, 10
    };
    //每次恢复刷新的回调
    private Action<float>[] mUpdateCallback = new Action<float>[3];
    //每次恢复完成的回调
    private Action[] mCompleteCallback = new Action[3];

    private RoleInfoTimerManager()
    {
        __InitTimer();
    }

    public static RoleInfoTimerManager Instance
    {
        get
        {
            if (msInstance == null)
                msInstance = new RoleInfoTimerManager();
            return msInstance;
        }
    }

    private void __InitTimer()
    {
        for (int i = 0, count = mTimerName.Length; i < count; i++)
        {
            CountdownTimerData tmpTimerData = GlobalModule.Instance.countdownTimer.RegisterTimer(mTimerName[i]);
            if (tmpTimerData != null)
            {
                tmpTimerData.ResetTime = mRecoverDelta[i];
                GlobalModule.Instance.countdownTimer.AddTimerUpdateCallback(mTimerName[i], __OnUpdate);
                GlobalModule.Instance.countdownTimer.AddTimerCompleteCallback(mTimerName[i], __OnComplete);
            }
        }
    }

    public void Clear()
    {
        if (GlobalModule.Instance != null && GlobalModule.Instance.countdownTimer != null)
        {
            for (int i = 0, count = mTimerName.Length; i < count; i++)
            {
                GlobalModule.Instance.countdownTimer.StopTimer(mTimerName[i]);
                GlobalModule.Instance.countdownTimer.RemoveTimerUpdateCallback(mTimerName[i], __OnUpdate);
                GlobalModule.Instance.countdownTimer.RemoveTimerCompleteCallback(mTimerName[i], __OnComplete);
            }
        }
        msInstance = null;
    }

    public int GetRecoverMaxValue(ROLE_INFO_TIMER_TYPE type)
    {
        if (type == ROLE_INFO_TIMER_TYPE.NONE)
            return 0;
        return mRecoverMaxValue[(int)type];
    }
    public int GetRecoverDelta(ROLE_INFO_TIMER_TYPE type)
    {
        if (type == ROLE_INFO_TIMER_TYPE.NONE)
            return 0;
        return mRecoverDelta[(int)type];
    }

    /// <summary>
    /// 获取指定计时器名称
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private string __GetTimerName(ROLE_INFO_TIMER_TYPE type)
    {
        if (type == ROLE_INFO_TIMER_TYPE.NONE)
            return "";
        return mTimerName[(int)type];
    }
    /// <summary>
    /// 获取计时器类型
    /// </summary>
    /// <param name="timerName"></param>
    /// <returns></returns>
    public ROLE_INFO_TIMER_TYPE GetTimerType(string timerName)
    {
        ROLE_INFO_TIMER_TYPE tmpType = ROLE_INFO_TIMER_TYPE.NONE;
        for (int i = 0, count = mTimerName.Length; i < count; i++)
        {
            if (mTimerName[i] == timerName)
            {
                tmpType = (ROLE_INFO_TIMER_TYPE)i;
                break;
            }
        }
        return tmpType;
    }
    /// <summary>
    /// 获取计时器
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private CountdownTimerData __GetTimer(ROLE_INFO_TIMER_TYPE type)
    {
        string tmpTimerName = __GetTimerName(type);
        return GlobalModule.Instance.countdownTimer.GetTimer(tmpTimerName);
    }

    /// <summary>
    /// 开启恢复
    /// </summary>
    /// <param name="type"></param>
    public void StartRecover(ROLE_INFO_TIMER_TYPE type)
    {
        string tmpTimerName = __GetTimerName(type);
        CountdownTimerData tmpTimer = __GetTimer(type);
        if (tmpTimer != null)
        {
            switch (type)
            {
                case ROLE_INFO_TIMER_TYPE.STAMINA: tmpTimer.LeftTime = __GetLeftTime(type, RoleLogicData.Self.staminaStamp); break;
                case ROLE_INFO_TIMER_TYPE.SPIRIT: tmpTimer.LeftTime = __GetLeftTime(type, RoleLogicData.Self.spiritStamp); break;
                case ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD: tmpTimer.LeftTime = __GetLeftTime(type, RoleLogicData.Self.beatDemonCardStamp); break;
            }
        }
        GlobalModule.Instance.countdownTimer.StartTimer(tmpTimerName, false);
    }
    /// <summary>
    /// 检查并且开始恢复
    /// </summary>
    /// <param name="type"></param>
    /// <param name="isRefreshStartTime">是否自动重置当前刷新开始时间</param>
    public void CheckAndStartRecover(ROLE_INFO_TIMER_TYPE type, bool isRefreshStartTime)
    {
        if (type == ROLE_INFO_TIMER_TYPE.NONE)
            return;

        bool tmpIsStart = false;
        PUSH_TYPE pushiType = PUSH_TYPE.NONE;
        switch (type)
        {
            case ROLE_INFO_TIMER_TYPE.STAMINA: 
                tmpIsStart = RoleLogicData.Self.stamina < mRecoverMaxValue[0]; 
                pushiType = PUSH_TYPE.STAMINA; 
                break;
            case ROLE_INFO_TIMER_TYPE.SPIRIT: 
                tmpIsStart = RoleLogicData.Self.spirit < mRecoverMaxValue[1];
                pushiType = PUSH_TYPE.SPIRIT; 
                break;
            case ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD: 
                tmpIsStart = RoleLogicData.Self.beatDemonCard < mRecoverMaxValue[2];
                pushiType = PUSH_TYPE.BEAT_DEMON_CARD; 
                break;
        }
        if (tmpIsStart)
        {
            if (isRefreshStartTime)
            {
                switch (type)
                {
                    case ROLE_INFO_TIMER_TYPE.STAMINA: RoleLogicData.Self.staminaStamp = CommonParam.NowServerTime(); break;
                    case ROLE_INFO_TIMER_TYPE.SPIRIT: RoleLogicData.Self.spiritStamp = CommonParam.NowServerTime(); break;
                    case ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD: RoleLogicData.Self.beatDemonCardStamp = CommonParam.NowServerTime(); break;
                }
            }
            StartRecover(type);
            __AddLocalPush(pushiType, type);
        }
        else
            StopTimer(type);
    }

    /// <summary>
    /// 添加推送
    /// </summary>
    /// <param name="pushiType">推送类型</param>
    /// <param name="type"></param>
    private bool __AddLocalPush(PUSH_TYPE pushiType, ROLE_INFO_TIMER_TYPE type)
    {
        return PushMessageManager.Self.AddLocalPush(pushiType, __GetAllRecoverEndTime(type));
    }

    private long __GetAllRecoverEndTime(ROLE_INFO_TIMER_TYPE type)
    {
        int tmpMaxValue = 0;
        int tmpDelta = 0;
        int tmpLeftTime = 0;
        long tmpDstTime = 0;
        switch (type)
        {
            case ROLE_INFO_TIMER_TYPE.STAMINA:
                tmpMaxValue = RoleInfoTimerManager.Instance.GetRecoverMaxValue(ROLE_INFO_TIMER_TYPE.STAMINA);
                tmpDelta = RoleInfoTimerManager.Instance.GetRecoverDelta(ROLE_INFO_TIMER_TYPE.STAMINA);
                tmpLeftTime = (tmpMaxValue - RoleLogicData.Self.stamina) * tmpDelta;
                tmpDstTime = RoleLogicData.Self.staminaStamp + tmpLeftTime;
                break;
            case ROLE_INFO_TIMER_TYPE.SPIRIT:
                tmpMaxValue = RoleInfoTimerManager.Instance.GetRecoverMaxValue(ROLE_INFO_TIMER_TYPE.SPIRIT);
                tmpDelta = RoleInfoTimerManager.Instance.GetRecoverDelta(ROLE_INFO_TIMER_TYPE.SPIRIT);
                tmpLeftTime = (tmpMaxValue - RoleLogicData.Self.spirit) * tmpDelta;
                tmpDstTime = RoleLogicData.Self.spiritStamp + tmpLeftTime;
                break;
            case ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD:
                tmpMaxValue = RoleInfoTimerManager.Instance.GetRecoverMaxValue(ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD);
                tmpDelta = RoleInfoTimerManager.Instance.GetRecoverDelta(ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD);
                tmpLeftTime = (tmpMaxValue - RoleLogicData.Self.beatDemonCard) * tmpDelta;
                tmpDstTime = RoleLogicData.Self.beatDemonCardStamp + tmpLeftTime;
                break;
        }
        return tmpDstTime;
    }
    /// <summary>
    /// 停止计时器
    /// </summary>
    /// <param name="type"></param>
    public void StopTimer(ROLE_INFO_TIMER_TYPE type)
    {
        string tmpTimerName = __GetTimerName(type);
        GlobalModule.Instance.countdownTimer.StopTimer(tmpTimerName);
    }
    /// <summary>
    /// 添加每次恢复刷新回调
    /// </summary>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public void AddTimerUpdateCallback(ROLE_INFO_TIMER_TYPE type, Action<float> callback)
    {
        if (type == ROLE_INFO_TIMER_TYPE.NONE)
            return;
        mUpdateCallback[(int)type] += callback;
    }
    /// <summary>
    /// 移除每次恢复刷新回调
    /// </summary>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public void RemoveTimerUpdateCallback(ROLE_INFO_TIMER_TYPE type, Action<float> callback)
    {
        if (type == ROLE_INFO_TIMER_TYPE.NONE)
            return;
        if (mUpdateCallback[(int)type] != null)
            mUpdateCallback[(int)type] -= callback;
    }
    /// <summary>
    /// 添加每次恢复完成回调
    /// </summary>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public void AddTimerCompleteCallback(ROLE_INFO_TIMER_TYPE type, Action callback)
    {
        if (type == ROLE_INFO_TIMER_TYPE.NONE)
            return;
        mCompleteCallback[(int)type] += callback;
    }
    /// <summary>
    /// 移除每次恢复完成回调
    /// </summary>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public void RemoveTimerCompleteCallback(ROLE_INFO_TIMER_TYPE type, Action callback)
    {
        if (type == ROLE_INFO_TIMER_TYPE.NONE)
            return;
        if (mCompleteCallback[(int)type] != null)
            mCompleteCallback[(int)type] -= callback;
    }
    /// <summary>
    /// 获取当前剩余时间
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public long GetCurrentLeftTime(ROLE_INFO_TIMER_TYPE type)
    {
        CountdownTimerData tmpTimer = __GetTimer(type);
        if (tmpTimer == null)
            return -1;
        return (long)tmpTimer.LeftTime;
    }

    /// <summary>
    /// 获取剩余时间
    /// </summary>
    /// <param name="startTime"></param>
    /// <returns></returns>
    private long __GetLeftTime(ROLE_INFO_TIMER_TYPE type, long startTime)
    {
        if(type == ROLE_INFO_TIMER_TYPE.NONE)
            return 0;
        DateTime tmpStartDate = GameCommon.ConvertServerSecTimeTo1970(startTime);
        int tmpDelta = mRecoverDelta[(int)type];
        DateTime tmpStopDate = tmpStartDate.AddSeconds(tmpDelta);
        DateTime tmpNowDate = GameCommon.ConvertServerSecTimeTo1970(CommonParam.NowServerTime());
        TimeSpan tmpSpan = tmpStopDate - tmpNowDate;
//        DEBUG.Log("StartDate = " + tmpStartDate + ", stopDate = " + tmpStopDate + ", nowDate = " + tmpNowDate);
        return Convert.ToInt64(tmpSpan.TotalSeconds);
    }
    /// <summary>
    /// 恢复时间结束后添加相应恢复值
    /// </summary>
    /// <param name="type"></param>
    private void __AddRecoverValue(ROLE_INFO_TIMER_TYPE type)
    {
        switch (type)
        {
            case ROLE_INFO_TIMER_TYPE.STAMINA: RoleLogicData.Self.AddStamina(1); break;
            case ROLE_INFO_TIMER_TYPE.SPIRIT: RoleLogicData.Self.AddSpirit(1); break;
            case ROLE_INFO_TIMER_TYPE.BEAT_DEMON_CARD: RoleLogicData.Self.AddBeatDemonCard(1); break;
        }
    }
    /// <summary>
    /// 每次刷新结束调用
    /// </summary>
    /// <param name="timerData"></param>
    private void __OnUpdate(CountdownTimerData timerData)
    {
        ROLE_INFO_TIMER_TYPE tmpType = GetTimerType(timerData.TimerName);
        int tmpTypeValue = (int)tmpType;
        if (tmpTypeValue < 0 || tmpTypeValue >= mCompleteCallback.Length)
            return;
        if (mUpdateCallback[tmpTypeValue] != null)
            mUpdateCallback[tmpTypeValue](timerData.LeftTime);
    }
    /// <summary>
    /// 每次恢复结束调用
    /// </summary>
    /// <param name="timerData"></param>
    private void __OnComplete(CountdownTimerData timerData, bool isForceStop)
    {
        ROLE_INFO_TIMER_TYPE tmpType = GetTimerType(timerData.TimerName);
        int tmpTypeValue = (int)tmpType;
        if (tmpTypeValue < 0 || tmpTypeValue >= mCompleteCallback.Length)
            return;
        if (!isForceStop)
            __AddRecoverValue(tmpType);
        if (mCompleteCallback[tmpTypeValue] != null)
            mCompleteCallback[tmpTypeValue]();
    }
}
