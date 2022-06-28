using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

/// <summary>
/// 进入主场景加载界面
/// </summary>
public class MainGameSceneLoadingUI : GameLoadingWithAnimUI
{
}

/// <summary>
/// 进入主场景加载界面窗口
/// </summary>
public class MainGameSceneLoadingWindow : GameLoadingWithAnimWindow
{
    protected List<LoadingStepData> mListLoadingStep = new List<LoadingStepData>();
    protected static Action mFinishCallback = null;
    protected float mSceneLoadedProgress = 0.0f;        //场景加载进度

    public override void Open(object param)
    {
        if (mListLoadingStep.Count <= 0)
        {
            mListLoadingStep.Add(new LoadingStepData()
            {
                RangeProgress = new Vector2(0.0f, 0.6f),
                LoadingFunction = __RequestAllData
            });
            mListLoadingStep.Add(new LoadingStepData()
            {
                RangeProgress = new Vector2(0.6f, 1.0f),
                LoadingFunction = __LoadAllResources
            });
        }

        base.Open(param);
    }

    public static Action FinishedCallback
    {
        set { mFinishCallback = value; }
        get { return mFinishCallback; }
    }

    /// <summary>
    /// 在开始加载时被调用
    /// </summary>
    protected override void _OnStartLoading(object param)
    {
        GlobalModule.DoCoroutine(__LoadAllData());
    }
    /// <summary>
    /// 在加载完成时被调用
    /// </summary>
    protected override void _OnLoadFinished()
    {
        if (CommonParam.UseSDK && !MyPlaySDK.Variables.initIAPYet)
        {
            MyPlaySDK.InitIAP(MyPlaySDK.Variables.ServerID, MyPlaySDK.Variables.RoleID);
            MyPlaySDK.Variables.initIAPYet = true;
        }

        if (!GuideManager.IsGuideFinished())
        {
            DataCenter.Set("FIRST_LANDING", false);
        }

        if (DataCenter.Get("FIRST_LANDING"))
        {
            DataCenter.OpenWindow("DAILY_SIGN_WINDOW");
            DataCenter.Set("FIRST_LANDING", false);
        }
        
        //从战斗失败界面跳到抽卡or装备or队伍界面
        GotoWindowFromBattleFail();

        if (mFinishCallback != null)
        {
            mFinishCallback();
            mFinishCallback = null;
        }
    }

    public void GotoWindowFromBattleFail()
    {
        object _valueFromBattle = DataCenter.Self.getObject ("FROM_BATTLE_FAIL");
        if (_valueFromBattle != null && _valueFromBattle is int) 
        {
            int _gainFuncID = (int)_valueFromBattle;
            //by chenliang
            //begin

//            GlobalModule.DoCoroutine(IE_GotoGainFunc(_gainFuncID));
//--------------------
            //使用委托执行逻辑
            Action tmpComplete = null;
            tmpComplete = () =>
            {
                TeamWindow.RemoveWindowInstanceComoplete(tmpComplete);
                GetPathHandlerDic.ExecuteDelegate(_gainFuncID);
            };
            TeamWindow.AddWinInstanceComplete(tmpComplete);

            //end
            DataCenter.Set("FROM_BATTLE_FAIL", null);
            return;
        }

        string strFromBattle = DataCenter.Get("FROM_BATTLE_FAIL");

            if (strFromBattle == "shop"){
                GlobalModule.ClearAllWindow();
                DataCenter.OpenWindow("SHOP_WINDOW");
            }
            else if (strFromBattle == "stage"){
                if (CommonParam.bIsNetworkGame)
                {
                    GlobalModule.DoCoroutine(IE_GoToStageWinWhenFail());             
                }
                else
                {
                    MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
                    MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "ROLE_INFO_WINDOW";
                }
            }
            else if(strFromBattle == "equip"){
                DataCenter.OpenWindow("PACKAGE_EQUIP_WINDOW");
                MainUIScript.Self.HideMainBGUI();
            }
            DataCenter.Set("FROM_BATTLE_FAIL", null);
    }

    private IEnumerator IE_GotoGainFunc(int kGainFuncID) 
    {
        for (int i = 0; i < 6; i++) 
        {
            yield return null;
        }
        GetPathHandlerDic.ExecuteDelegate(kGainFuncID);
    }

    private IEnumerator IE_GoToStageWinWhenFail() 
    {
        for (int i = 0; i < 6; i++) 
        {
            yield return new WaitForEndOfFrame();
        }
        DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);
        MainUIScript.Self.HideMainBGUI();
    }
    private void __SetLoadingProgress(float progress)
    {
        _SetLoadingProgress(progress);
    }

    public static IEnumerator LoadMainScene(Action<float> funcProgress)
    {
        int iIndex = UnityEngine.Random.Range((int)ELEMENT_TYPE.RED, (int)ELEMENT_TYPE.MAX);
        iIndex = 4;

        AsyncOperation tmpAsyncOP = GlobalModule.Instance.LoadSceneAsync("Mainmenu_bg_" + iIndex.ToString(), false);
        while (!tmpAsyncOP.isDone)
        {
            yield return null;
            if (funcProgress != null)
                funcProgress(tmpAsyncOP.progress);
        }

        GameCommon.SetBackgroundSound("Sound/Opening", 0.7f);

        GameCommon.SetMainCameraEnable(false);
    }

    /// <summary>
    /// 加载所有数据
    /// </summary>
    /// <returns></returns>
    private IEnumerator __LoadAllData()
    {
        if (mListLoadingStep == null || mListLoadingStep.Count <= 0)
            yield break;

        mSceneLoadedProgress = 0.0f;
        RoleSetting.IsInited = false;
        GlobalModule.DoCoroutine(LoadMainScene((float progress) =>
        {
            mSceneLoadedProgress = progress;
        }));

        for (int i = 0, count = mListLoadingStep.Count; i < count; i++)
        {
            LoadingStepData tmpStepData = mListLoadingStep[i];
            if (tmpStepData.LoadingFunction != null)
            {
                yield return GlobalModule.DoCoroutine(tmpStepData.LoadingFunction(
                    tmpStepData.RangeProgress,
                    (LoadingProgressParam progress) =>
                    {
                        __SetLoadingProgress(progress.Progress);
                    },
                    tmpStepData.LoadingParam));
            }
        }
    }
    /// <summary>
    /// 请求所有数据
    /// </summary>
    /// <param name="rangeProgress"></param>
    /// <returns></returns>
    private IEnumerator __RequestAllData(Vector2 rangeProgress, Action<LoadingProgressParam> progressCallback, object param)
    {
        LoadingProgressParam tmpCurrProgress = new LoadingProgressParam();
        float tmpProgressStart = rangeProgress.x;
        float tmpProgressRange = rangeProgress.y - rangeProgress.x;
        float tmpProgress = 0.0f;
        float tmpProgressStepCount = 1.0f;          //如果新加了请求步骤，需要设置此变量为新的请求步骤数量
        float tmpProgressStep = 1.0f / tmpProgressStepCount;

        if (!CommonParam.isDataInit)
        {
            tmpProgressStepCount += 6.0f;
            tmpProgressStep = 1.0f / tmpProgressStepCount;
            // 获取包裹数据
            for (int i = (int)PACKAGE_TYPE.PET, count = (int)PACKAGE_TYPE.MAX; i < count; ++i)
            {
                yield return NetManager.StartWaitPackageData((PACKAGE_TYPE)i);
//                 NetManager.StartWaitPackageData((PACKAGE_TYPE)i);
//                 yield return null;
                tmpCurrProgress.Progress = tmpProgress + (((float)i / (float)(count - 1.0f)) * tmpProgressStep);
                tmpCurrProgress.Progress = tmpProgressStart + tmpCurrProgress.Progress * tmpProgressRange;
                if (progressCallback != null)
                    progressCallback(tmpCurrProgress);
            }
            tmpProgress += tmpProgressStep;

            // 清空副本数据
            MapLogicData.Reset();

            // 获取主线副本数据
            yield return new BattleMainMapRequester().Start();
            tmpCurrProgress.Progress = tmpProgressStart + (tmpProgress + tmpProgressStep) * tmpProgressRange;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
            tmpProgress += tmpProgressStep;

            //added by xuke 得到主线副本数据后进行红点数据初始化
            if (AdventureNewMarkManager.Self != null)
            {
                AdventureNewMarkManager.Self.CheckReward_NewMark();
            }
            //end

            // 获取活动副本数据
            //yield return new BattleActiveMapRequester().Start();
            //tmpCurrProgress.Progress = tmpProgressStart + (tmpProgress + tmpProgressStep) * tmpProgressRange;
            //if (progressCallback != null)
            //    progressCallback(tmpCurrProgress);
            //tmpProgress += tmpProgressStep;

            // 获得点星数据
            //        yield return NetManager.StartWaitPointStarValue();
            NetManager.StartWaitPointStarValue();
            yield return null;
            tmpCurrProgress.Progress = tmpProgressStart + (tmpProgress + tmpProgressStep) * tmpProgressRange;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
            tmpProgress += tmpProgressStep;

            //         // 获得每日签到数据
            //         yield return NetManager.StartWaitDailySignQuery();
            //         __SetLoadingProgress(tmpProgressStart + (tmpProgress + tmpProgressStep) * tmpProgressRange);
            //         tmpProgress += tmpProgressStep;

            // 获取日常任务数据
            //        yield return new GetDailyTaskDataRequester().Start();
            new GetDailyTaskDataRequester().Start();
            yield return null;
            tmpCurrProgress.Progress = tmpProgressStart + (tmpProgress + tmpProgressStep) * tmpProgressRange;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
            tmpProgress += tmpProgressStep;

            // 获取成就数据
            //        yield return new GetAchievementDataRequester().Start();
            new GetAchievementDataRequester().Start();
            yield return null;
            tmpCurrProgress.Progress = tmpProgressStart + (tmpProgress + tmpProgressStep) * tmpProgressRange;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
            tmpProgress += tmpProgressStep;

            // 获取次日登陆请求数据
            //        yield return NetManager.StartWaitMorrowLandQuery();
            NetManager.StartWaitMorrowLandQuery();
            yield return null;
            tmpCurrProgress.Progress = tmpProgressStart + (tmpProgress + tmpProgressStep) * tmpProgressRange;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
            tmpProgress += tmpProgressStep;


            CommonParam.isDataInit = true;
        }

        yield return null;
        tmpCurrProgress.Progress = tmpProgressStart + (tmpProgress + tmpProgressStep) * tmpProgressRange;
        if (progressCallback != null)
            progressCallback(tmpCurrProgress);
        tmpProgress += tmpProgressStep;

#if !UNITY_EDITOR && !NO_USE_SDK
        if (CommonParam.isUseSDK)
        {
            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.USER_NAME, LoginNet.sdkData.GetData(U3DSharkAttName.USER_NAME));
            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.USER_TOKEN, CommonParam.SDKToken);

            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.USER_ID, CommonParam.SDKUserID);
            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.USER_HEAD_ID, "");
            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.USER_HEAD_URL, "");
            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.ROLE_ID, CommonParam.mUId);
            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.ROLE_NAME, RoleLogicData.Self.name);
            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.ROLE_LEVEL, RoleLogicData.Self.character.level);
            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.ZONE_ID, CommonParam.mZoneID);
            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.ZONE_NAME, CommonParam.mZoneName);
            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.SERVER_ID, CommonParam.mZoneID);
            U3DSharkSDK.Instance.GetUserData().SetData(U3DSharkAttName.SERVER_NAME, CommonParam.mZoneName);
            U3DSharkSDK.Instance.UpdatePlayerInfo();
        }
#endif
    }
    /// <summary>
    /// 加载所有资源
    /// </summary>
    /// <param name="rangeProgress"></param>
    /// <returns></returns>
    private IEnumerator __LoadAllResources(Vector2 rangeProgress, Action<LoadingProgressParam> progressCallback, object param)
    {
        LoadingProgressParam tmpCurrProgress = new LoadingProgressParam();
        float tmpProgressStart = rangeProgress.x;
        float tmpProgressRange = rangeProgress.y - rangeProgress.x;

        TeamManager.InitTeamListData();
        Relationship.Init();
//         yield return GlobalModule.DoCoroutine(LoadMainScene((float progress) =>
//         {
//             tmpCurrProgress.Progress = tmpProgressStart + progress * tmpProgressRange;
//             if (progressCallback != null)
//                 progressCallback(tmpCurrProgress);
//         }));
        //只有场景加载完后才能进行后面的操作
        if (mSceneLoadedProgress >= 1.0f)
        {
            tmpCurrProgress.Progress = rangeProgress.y;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
        }
        else
        {
            tmpCurrProgress.Progress = Mathf.Max(rangeProgress.y - 0.01f, rangeProgress.x);
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
            while (mSceneLoadedProgress < 1.0f)
                yield return null;
            tmpCurrProgress.Progress = rangeProgress.y;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
        }
        RoleSetting.IsInited = true;

        //刷新左上角玩家信息
        DataCenter.SetData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_ROLE_NAME", null);
        DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "UPDATE_MARK", true);
        //刷新顶部数据
        GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_VITAKITY", true);
        GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_GOLD", true);
        GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_DIAMOND", true);
        GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_FRIEND_POINT", true);

        yield return null;

        //DataCenter.SetData("ROLE_SEL_BOTTOM_LEFT_GROUP", "UPDATE_TEAM_RED_POINT", TeamManager.HasFreeTeamPos() && PetLogicData.Self.HasFreePet() || RoleEquipLogicData.Self.HasFreeEquipAndFreeEquipPos());

        GlobalModule.DoOnNextLateUpdate(() =>
        {
            DataCenter.OpenWindow("TEAM_WINDOW");
        });
    }
}
