using UnityEngine;
using System.Collections;
using DataTable;

public class StartMain : MonoBehaviour 
{
    //by chenliang
    //begin

//    void Start()
//    {
//        MessageReceiver.Init();
//        StartCoroutine(waitTime(3));
//    }
//
//    IEnumerator waitTime(float time)
//    {
//        yield return new WaitForSeconds(time);
//
//        Application.LoadLevel("HotUpdateLoading");
//        //Application.LoadLevel("StartGame");
//    }
//------------------------
    [SerializeField]
    private UITexture m_UnityLogo;          //UnityLogo

    private AsyncOperation m_LoadSceneOP;   //场景异步操作

    void Start()
    {
        MessageReceiver.Init();
        StartCoroutine(__InitGame());
    }

    /// <summary>
    /// 初始化游戏
    /// </summary>
    /// <returns></returns>
    private IEnumerator __InitGame()
    {
        JCode.InitLitJson();
        __ShowUnityLogo();
        StartCoroutine(__LoadScene());
        //由于场景的allowSceneActivation为false，所以只有设置成true，才能继续加载场景
        while (m_LoadSceneOP.progress < 0.9f)
            yield return null;
        //暂时不隐藏
        __HideUnityLogo();
        yield return StartCoroutine(__PlayMovie());
        GlobalModule.stopwatch.Reset();
        GlobalModule.stopwatch.Start();
        DEBUG.Log("InitGame - ticks : " + GlobalModule.stopwatch.ElapsedTicks + ", milliseconds : " + GlobalModule.stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// 显示Unity启动Logo
    /// </summary>
    private void __ShowUnityLogo()
    {
        if (m_UnityLogo == null)
            return;
        m_UnityLogo.enabled = true;
    }
    /// <summary>
    /// 隐藏Unity启动Logo
    /// </summary>
    private void __HideUnityLogo()
    {
        if (m_UnityLogo == null)
            return;
        m_UnityLogo.enabled = false;
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    /// <returns></returns>
    private IEnumerator __LoadScene()
    {
        //这里检查是否有热更新
        if (false)       //选择改为只加载热更新
        {
            HotUpdateLog.Log("Loading HotUpdateLoading scene.");
            m_LoadSceneOP = Application.LoadLevelAsync("HotUpdateLoading");
        }
        else
        {
            HotUpdateLog.Log("Loading StartGame scene.");
            m_LoadSceneOP = Application.LoadLevelAsync("StartGame");
        }
        m_LoadSceneOP.allowSceneActivation = false;
        while (!m_LoadSceneOP.isDone)
        {
            HotUpdateLog.Log("Progress = " + m_LoadSceneOP.progress);
            yield return null;
        }
    }
    /// <summary>
    /// 播放片头动画
    /// </summary>
    /// <returns></returns>
    private IEnumerator __PlayMovie()
    {
        yield return null;
#if UNITY_ANDROID || UNITY_IOS
//        yield return new WaitForSeconds(0.3f);
        Handheld.PlayFullScreenMovie("company_logo.mp4", Color.black, FullScreenMovieControlMode.Hidden);
#endif

        if (m_LoadSceneOP != null)
            m_LoadSceneOP.allowSceneActivation = true;
    }

    //end
	
}
