using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 战斗界面加载基类
/// </summary>
public class BattleLoadingUI : GameLoadingWithAnimUI
{
}

/// <summary>
/// 战斗窗口加载基类
/// </summary>
public class BattleLoadingWindow : GameLoadingWithAnimWindow
{
    protected bool mIsBattleAutoClose = true;

    public BattleLoadingWindow()
        : base()
    {
        mIsRecordTimer = true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "IS_BATTLE_AUTO_CLOSE":
                {
                    mIsBattleAutoClose = (bool)objVal;
                } break;
        }
    }

    protected override void _OnStartLoading(object param)
    {
        mIsAutoClose = false;
        GlobalModule.IsSceneLoadComplete = false;
        GlobalModule.DoCoroutine(__LoadAllData());
    }

    /// <summary>
    /// 加载所有数据
    /// </summary>
    /// <returns></returns>
    private IEnumerator __LoadAllData()
    {
//         //为了防止场景过小，加载时出现在0%时卡顿现象，设置随机进度和间隔
//         float tmpRandomProgress = UnityEngine.Random.Range(0.7f, 0.9f);
//         float tmpRandomProgressTime = UnityEngine.Random.Range(0.2f, 0.3f);
//         _SetLoadingProgress(tmpRandomProgress);
//         yield return new WaitForSeconds(tmpRandomProgressTime);

        //隔一帧，让背景显示出来
        yield return null;
        Type tmpType = this.GetType();
        string tmpStrBattleType = tmpType.ToString();
        DEBUG.Log("Start load battle " + tmpStrBattleType);
        BattleLoadingUI tmpLoadingUI = null;
        if (mGameObjUI != null)
        {
            tmpLoadingUI = mGameObjUI.GetComponent<BattleLoadingUI>();
            if (tmpLoadingUI != null && !tmpLoadingUI.enabled)
            {
                DEBUG.Log("Battle loading " + tmpStrBattleType + " is disabled before");
                tmpLoadingUI.enabled = true;
            }
        }
        else
            DEBUG.Log("Battle loading" + tmpStrBattleType + " ui gameobject is null");
        yield return GlobalModule.DoCoroutine(MainProcess.LoadBattleSceneAsync((float progress) =>
        {
            DEBUG.Log("battle " + tmpStrBattleType + " load progress = " + progress);
            if (tmpLoadingUI != null && !tmpLoadingUI.enabled)
            {
                DEBUG.Log("Battle loading " + tmpStrBattleType + " is disabled duration");
                tmpLoadingUI.enabled = true;
            }
            _SetLoadingProgress(progress);
        }));
    }

    protected override void _OnLoadFinished()
    {
        Type tmpType = this.GetType();
        string tmpStrBattleType = tmpType.ToString();
        GlobalModule.IsSceneLoadComplete = true;
        DEBUG.Log("Start battle " + tmpStrBattleType);
        _OnStartBattle();
        GlobalModule.DoOnNextLateUpdate(() =>
        {
            DEBUG.Log(tmpStrBattleType + " battle stage start");
            MainProcess.mStage.Start();
            GlobalModule.DoOnNextUpdate(() =>
            {
                GlobalModule.DoOnNextLateUpdate(() =>
                {
                    DEBUG.Log(tmpStrBattleType + " UI closed");
                    this.set("CLOSE", true);
                    //开始加载战斗场景物体
                    int tmpStageIndex = DataCenter.Get("CURRENT_STAGE");
                    if (tmpStageIndex > 0)
                        SceneObjLoader.Instance.Load(tmpStageIndex);
                });
            });
        });
    }

    /// <summary>
    /// 战斗开始
    /// </summary>
    protected virtual void _OnStartBattle()
    {
    }
}
