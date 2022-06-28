using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 加载场景物体
/// </summary>
public class SceneObjLoader
{
    private static readonly string msSceneRoot = "Scenes";

    private static SceneObjLoader msInstance = new SceneObjLoader();

    private bool mIsDynamicLoadSceneObj = true;         //是否动态加载场景物体
    private ICoroutineOperation mCoroutineOperation;

    private SceneObjLoader()
    {
    }

    public static SceneObjLoader Instance
    {
        get { return msInstance; }
    }

    public ICoroutineOperation coroutineOperation
    {
        set { mCoroutineOperation = value; }
        get { return mCoroutineOperation; }
    }

    /// <summary>
    /// 加载指定场景物体
    /// </summary>
    /// <param name="sceneID"></param>
    public void Load(int sceneID)
    {
        List<string> tmpListSceneObjName = __GetSceneObjNameList(sceneID);
        float tmpObjLoadDelay = __GetSceneObjLoadDelay(sceneID);
        if (mCoroutineOperation != null &&
            tmpListSceneObjName != null && tmpListSceneObjName.Count > 0)
            mCoroutineOperation.StartCoroutine(__Load(tmpListSceneObjName, tmpObjLoadDelay));
    }

    /// <summary>
    /// 获取指定场景物体数据
    /// </summary>
    /// <param name="sceneID"></param>
    /// <returns></returns>
    private List<string> __GetSceneObjNameList(int sceneID)
    {
        return null;
        //TODO now just for test
        if (sceneID != 20001)
            return null;

        return (new List<string>()
        {
            "loyalismwood_1/loyalismwood_1_part1",
            "loyalismwood_1/loyalismwood_1_part2",
            "loyalismwood_1/loyalismwood_1_part3",
            "loyalismwood_1/loyalismwood_1_part4",
        });
    }
    /// <summary>
    /// 获取指定场景物体之间加载间隔
    /// </summary>
    /// <param name="sceneID"></param>
    /// <returns></returns>
    private float __GetSceneObjLoadDelay(int sceneID)
    {
        //TODO
        return 0.1f;
    }

    /// <summary>
    /// 根据名字列表加载场景物体
    /// </summary>
    /// <param name="listSceneObjName"></param>
    /// <param name="objDelay">物体之间加载间隔</param>
    /// <returns></returns>
    private IEnumerator __Load(List<string> listSceneObjName, float objLoadDelay)
    {
        if (listSceneObjName == null)
            yield break;

        for (int i = 0, count = listSceneObjName.Count; i < count; i++)
        {
            if (mCoroutineOperation == null)
                yield break;
            string tmpSceneObj = listSceneObjName[i];
            yield return mCoroutineOperation.StartCoroutine(
                __LoadSceneObj(tmpSceneObj));
            yield return (new WaitForSeconds(objLoadDelay));
        }
    }
    /// <summary>
    /// 根据名字加载场景物体
    /// </summary>
    /// <param name="sceneObjName"></param>
    /// <returns></returns>
    private IEnumerator __LoadSceneObj(string sceneObjName)
    {
        if (sceneObjName == null || sceneObjName == "")
            yield break;

        string tmpSceneObjPath = msSceneRoot + "/" + sceneObjName;
        GameObject tmpScenObj = GameCommon.LoadAndIntanciatePrefabs(tmpSceneObjPath);
        if (tmpScenObj == null)
            yield break;
        if (mCoroutineOperation == null)
            yield break;
        yield return mCoroutineOperation.StartCoroutine(__TweenHandle(tmpScenObj));
    }
    /// <summary>
    /// 场景物体动画处理
    /// </summary>
    /// <param name="sceneObj"></param>
    private IEnumerator __TweenHandle(GameObject sceneObj)
    {
        if (sceneObj == null)
            yield break;

        UITweener tmpTween = __TweenPosition(
            sceneObj, new Vector3(0.0f, -20.0f, 0.0f), 0.5f);
        if (tmpTween == null)
            yield break;
        tmpTween.PlayForward();
    }

    private UITweener __TweenScale(GameObject sceneObj, Vector3 from, float duration)
    {
        TweenScale tmpTweenScale = sceneObj.GetComponent<TweenScale>();
        if (tmpTweenScale == null)
            tmpTweenScale = sceneObj.AddComponent<TweenScale>();
        tmpTweenScale.duration = duration;
        tmpTweenScale.from = from;
        tmpTweenScale.to = sceneObj.transform.localScale;
        return tmpTweenScale;
    }
    private UITweener __TweenPosition(GameObject sceneObj, Vector3 from, float duration)
    {
        TweenPosition tmpTweenPos = sceneObj.GetComponent<TweenPosition>();
        if (tmpTweenPos == null)
            tmpTweenPos = sceneObj.AddComponent<TweenPosition>();
        tmpTweenPos.animationCurve.AddKey(0.75f, 1.2f);     //回跳效果
        tmpTweenPos.duration = duration;
        tmpTweenPos.from = from;
        tmpTweenPos.to = sceneObj.transform.localPosition;
        return tmpTweenPos;
    }
}
