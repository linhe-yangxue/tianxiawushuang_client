using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 分步加载时每步加载相关数据
/// </summary>
public class LoadingStepData
{
    public Vector2 RangeProgress { set; get; }      //加载进度范围，x：开始进度，y：结束进度
    public Action<LoadingProgressParam> ProgressCallback { set; get; }      //进度设置回调函数
    public object LoadingParam { set; get; }        //进度加载参数
    public Func<Vector2, Action<LoadingProgressParam>, object, IEnumerator> LoadingFunction { set; get; }     //进度加载函数，参数：RangeProgress、LoadingParam，返回：IEnumerator
}

/// <summary>
/// 加载进度参数
/// </summary>
public class LoadingProgressParam
{
    private long m_BytesLoaded = 0;     //已加载大小
    private long m_BytesTotal = 0;      //总大小
    private float m_Progress = 0.0f;    //进度

    /// <summary>
    /// 已加载大小
    /// </summary>
    public long BytesLoaded
    {
        set { m_BytesLoaded = value; }
        get { return m_BytesLoaded; }
    }
    /// <summary>
    /// 总大小
    /// </summary>
    public long BytesTotal
    {
        set { m_BytesTotal = value; }
        get { return m_BytesTotal; }
    }
    /// <summary>
    /// 进度
    /// </summary>
    public float Progress
    {
        set { m_Progress = value; }
        get { return m_Progress; }
    }

    public void Init()
    {
        m_Progress = 0.0f;
        m_BytesLoaded = 0;
        m_BytesTotal = 0;
    }

    public void CopyFrom(LoadingProgressParam target)
    {
        if (target == null)
            return;

        m_Progress = Mathf.Clamp01(target.m_Progress);
        m_BytesLoaded = target.m_BytesLoaded;
        m_BytesTotal = target.m_BytesTotal;
    }
    public void Add(LoadingProgressParam target)
    {
        if (target == null)
            return;

        m_Progress += target.m_Progress;
        m_BytesLoaded += target.m_BytesLoaded;
        m_BytesTotal += target.m_BytesTotal;
    }

    public static bool operator ==(LoadingProgressParam lhs, LoadingProgressParam rhs)
    {
        bool tmpIsLhsNull = (lhs as object == null);
        bool tmpIsRhsNull = (rhs as object == null);
        if (tmpIsLhsNull && tmpIsRhsNull)
            return true;
        if (tmpIsLhsNull || tmpIsRhsNull)
            return false;

        return (
            lhs.m_Progress == rhs.m_Progress &&
            lhs.m_BytesLoaded == rhs.m_BytesLoaded &&
            lhs.m_BytesTotal == rhs.m_BytesTotal);
    }
    public static bool operator !=(LoadingProgressParam lhs, LoadingProgressParam rhs)
    {
        return !(lhs == rhs);
    }
}

public class LoadingHelper
{
    private static LoadingHelper msInstance;

    public static LoadingHelper Instance
    {
        get
        {
            if (msInstance == null)
                msInstance = new LoadingHelper();
            return msInstance;
        }
    }

    /// <summary>
    /// 将B转换为KB
    /// </summary>
    /// <param name="byteCount"></param>
    /// <returns></returns>
    public float ConvertBToKB(float byteCount)
    {
        return (byteCount / 1024.0f);
    }
    /// <summary>
    /// 将B转换为MB
    /// </summary>
    /// <param name="byteCount"></param>
    /// <returns></returns>
    public float ConvertBToMB(float byteCount)
    {
        return (byteCount / (1024.0f * 1024.0f));
    }
}

/// <summary>
/// 所有加载基类
/// </summary>
public interface ILoading
{
    /// <summary>
    /// 协同操作
    /// </summary>
    ICoroutineOperation CoroutineOperation { set; get; }

    /// <summary>
    /// 开始加载
    /// </summary>
    /// <param name="path">加载路径</param>
    /// <param name="progressCallback">加载进度回调，参数：已加载大小，总大小，已加载进度</param>
    /// <param name="param">加载参数</param>
    /// <returns></returns>
    IEnumerator StartLoad(string path, Action<LoadingProgressParam> progressCallback, object param);
    /// <summary>
    /// 停止加载
    /// </summary>
    void StopLoad();
    /// <summary>
    /// 当前加载进度
    /// </summary>
    /// <returns></returns>
    LoadingProgressParam LoadProgress();
    /// <summary>
    /// 是否在加载中
    /// </summary>
    /// <returns></returns>
    bool IsLoading();
    /// <summary>
    /// 是否加载完成
    /// </summary>
    /// <returns></returns>
    bool IsComplete();
    /// <summary>
    /// 是否加载成功
    /// </summary>
    /// <returns></returns>
    bool IsSuccess();
    /// <summary>
    /// 错误信息
    /// </summary>
    /// <returns></returns>
    string ErrorInfo();
}
