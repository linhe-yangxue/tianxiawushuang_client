using UnityEngine;
using System.Collections;

/// <summary>
/// 开始游戏加载界面
/// </summary>
public class StartGameLoadingUI : GameLoadingWithAnimUI
{
}

/// <summary>
/// 开始游戏加载窗口
/// </summary>
public class StartGameLoadingWindow : GameLoadingWithAnimWindow
{
    protected override void _OnStartLoading(object param)
    {
        this.DoCoroutine(__LoadAllData());
    }

    /// <summary>
    /// 加载所有数据
    /// </summary>
    /// <returns></returns>
    private IEnumerator __LoadAllData()
    {
        //随机获取一个进度点
        float tmpRandomStep = UnityEngine.Random.Range(0.3f, 0.7f);
        _SetLoadingProgress(tmpRandomStep);
        yield return new WaitForSeconds(1.0f);

        LoginNet.StartLogin();

        _SetLoadingProgress(1.0f);
    }

    protected override void _OnLoadFinished()
    {
        DataCenter.OpenWindow("LANDING_WINDOW");

        //设置游戏令牌标志
        LoginData.Instance.IsGameTokenValid = false;
        LoginData.Instance.IsInGameScene = false;

        //停止心跳包
        StaticDefine.useHeartbeat = false;

        //停止走马灯
        //RoleSelBottomLeftWindow.StopRollPlaying();

        GameCommon.SetBackgroundSound("Sound/Opening", 0.7f);
    }
}
