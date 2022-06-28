using UnityEngine;
using System.Collections;

/// <summary>
/// 协同接口，提供基本的协同操作
/// </summary>
public interface ICoroutineOperation
{
    /// <summary>
    /// 开启协同
    /// </summary>
    /// <param name="methodName"></param>
    /// <returns></returns>
    Coroutine StartCoroutine(string methodName);
    /// <summary>
    /// 开启协同
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    Coroutine StartCoroutine(string methodName, object value);
    /// <summary>
    /// 开启协同
    /// </summary>
    /// <param name="coroutine"></param>
    /// <returns></returns>
    Coroutine StartCoroutine(IEnumerator coroutine);
    /// <summary>
    /// 停止所有协同
    /// </summary>
    void StopAllCoroutines();
    /// <summary>
    /// 停止指定协同
    /// </summary>
    /// <param name="routine"></param>
    void StopCoroutine(Coroutine routine);
    /// <summary>
    /// 停止指定协同
    /// </summary>
    /// <param name="routine"></param>
    void StopCoroutine(IEnumerator routine);
    /// <summary>
    /// 停止指定协同
    /// </summary>
    /// <param name="methodName"></param>
    void StopCoroutine(string methodName);
}
