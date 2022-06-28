using UnityEngine;
using System;
using System.Collections;
using Utilities;

public enum QUIT_BACK_SCENE_TYPE
{
	NONE = -1,
	WORLD_MAP,	//> 冒险
	RAMMBOCK,	//> 群魔乱舞（封灵塔）
    FAIRYLAND,   //寻仙
    GUILDBOSS,   //寻仙
}


public class Settings
{
    private const string KEY = "SETTINGS";

    private static bool mIsSoundEffectEnabled = true;
    private static bool mIsMusicEnabled = true;
    private static bool mbIsGamePaused = false;
    private static float mReserveTimeScale = 1f;

    public static void Load()
    {
        SettingData data = null;

        if (GamePrefs.TryGet<SettingData>(KEY, out data))
        {
            SetMusicEnabled(data.mIsMusicEnabled);
            SetSoundEffectEnabled(data.mIsSoundEffectEnabled);
        }
    }

    public static void Save()
    {
        SettingData data = CreateData();
        GamePrefs.Set<SettingData>(KEY, data);
    }

    public static bool IsGamePaused()
    {
        return mbIsGamePaused;//TimeSetting.Self.currentState == TimeState.StrictPause;
    }

    public static void SetGamePaused(bool pause)
    {
        if (pause && !mbIsGamePaused)
        {
            //if (TimeSetting.Self.currentState != TimeState.StrictPause)
            //    TimeSetting.Self.Apply(TimeState.StrictPause);
            mbIsGamePaused = true;
            mReserveTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else if(!pause && mbIsGamePaused)
        {
            //if (TimeSetting.Self.currentState == TimeState.StrictPause)
            //    TimeSetting.Self.Revert();
            mbIsGamePaused = false;
            Time.timeScale = mReserveTimeScale;
            mReserveTimeScale = 1f;
        }
    }

    public static bool IsSoundEffectEnabled()
    {
        return mIsSoundEffectEnabled;
    }

    public static void SetSoundEffectEnabled(bool enable)
    {
        mIsSoundEffectEnabled = enable;
        //by chenliang
        //begin

        //实时记录设置，防止强制退出游戏时，设置不能保存
        Save();
        PlayerPrefs.Save();

        //end
    }

    public static bool IsMusicEnabled()
    {
        return mIsMusicEnabled;
    }

    public static void SetMusicEnabled(bool enable)
    {
        mIsMusicEnabled = enable;
        AudioSource music = GetMusicSource();

        if (music != null)
        {
            //music.enabled = enable;

            if (enable && !music.isPlaying)
                music.Play();
            else if (!enable && music.isPlaying)
                music.Stop();
        }
        //by chenliang
        //begin

        //实时记录设置，防止强制退出游戏时，设置不能保存
        Save();
        PlayerPrefs.Save();

        //end
    }

    public static void QuitBattle()
    {
        if (MainProcess.mStage != null && MainProcess.mStage.IsMainPVE())
        {
            var req = new ExitBattleMainRequester();
            req.Start();
        }
		QuitBackToCertainScene ();
    }

	// 从战斗场景返回到指定场景
	private static void QuitBackToCertainScene()
	{
		var _quitInfoObj = DataCenter.Self.getObject ("QUIT_BACK_SCENE");
		QUIT_BACK_SCENE_TYPE _backSceneType = QUIT_BACK_SCENE_TYPE.NONE;
		if(_quitInfoObj == null)
			 _backSceneType = QUIT_BACK_SCENE_TYPE.WORLD_MAP;
		else
			 _backSceneType = (QUIT_BACK_SCENE_TYPE)_quitInfoObj;

		switch (_backSceneType) 
		{
		case QUIT_BACK_SCENE_TYPE.NONE:
			MainProcess.OpenWordMapWindow();
			break;
		case QUIT_BACK_SCENE_TYPE.WORLD_MAP:
			MainProcess.OpenWordMapWindow();
			break;
		case QUIT_BACK_SCENE_TYPE.RAMMBOCK:
			MainProcess.ClearBattle();
			MainProcess.LoadRoleSelScene();
			
			DataCenter.OpenWindow("RAMMBOCK_WINDOW");
			break;
        case QUIT_BACK_SCENE_TYPE.FAIRYLAND:
            MainProcess.ClearBattle();
			MainProcess.LoadRoleSelScene();

            //改为场景加载完之后打开历练，进入寻仙界面
            MainGameSceneLoadingWindow.FinishedCallback = () =>
            {
                DataCenter.OpenWindow("TRIAL_WINDOW");
                DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
                GlobalModule.DoCoroutine(FairylandNetManager.RequestGetFairylandStates(""));
            };
            break;
        case QUIT_BACK_SCENE_TYPE.GUILDBOSS:
            MainProcess.OpenUnionPkPrepareWindow();
            break;
		}
		DataCenter.Set ("QUIT_BACK_SCENE",QUIT_BACK_SCENE_TYPE.WORLD_MAP);
	}

    public static void Logout()
    {
#if !UNITY_EDITOR && !NO_USE_SDK
        if(CommonParam.isUseSDK)
        {
            U3DSharkEventListener.LogoutCallback = GlobalModule.ChangeAccount;
            U3DSharkSDK.Instance.Logout();
        }
        else
#endif
        {
//            DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_NEED_QUIT_THE_GAME, "", () => Application.Quit());
            GlobalModule.ChangeAccount();
        }
    }

    //end

    public static void QuitGame()
    {
#if !UNITY_EDITOR && !NO_USE_SDK

        if (CommonParam.isUseSDK)
        {
            bool is_SdkExit = U3DSharkSDK.Instance.IsHasRequest(U3DSharkAttName.IS_USE_SDK_EXIT_WINDOW);
            if (is_SdkExit)
            {
                U3DSharkSDK.Instance.ExitGame();
            }
            else
            {
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_NEED_QUIT_THE_GAME, "", () => Application.Quit());
            }
            //U3DSharkSDK.Instance.ExitGame();
        }
        else
            DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_NEED_QUIT_THE_GAME, "", () => Application.Quit());
#else
        if (LoginData.Instance.IsInGameScene)
            DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_NEED_QUIT_THE_GAME, "", () => Application.Quit());
        else
            Application.Quit();
#endif
    }

    public static void SwitchAccount()
    {
//        string filePath = GameCommon.MakeGamePathFileName("Account.info");
//        System.IO.File.Delete(filePath);
//        DEBUG.LogWarning("SwitchAccount, then restart game >");
        //by chenliang
        //beign

//		GlobalModule.ChangeAccount();
//---------------
#if !UNITY_EDITOR && !NO_USE_SDK
        U3DSharkEventListener.LogoutCallback = GlobalModule.ChangeAccount;
        U3DSharkSDK.Instance.Logout();
#else
        GlobalModule.ChangeAccount();
#endif

        //end
    }

    private static AudioSource GetMusicSource()
    {
        Camera mainCamera = MainProcess.mMainCamera;

        if (mainCamera != null)
        {
            AudioSource musicSource = mainCamera.GetComponent<AudioSource>();
            return musicSource;
        }

        return null;
    }

    private static SettingData CreateData()
    {
        SettingData data = new SettingData();
        data.mIsMusicEnabled = mIsMusicEnabled;
        data.mIsSoundEffectEnabled = mIsSoundEffectEnabled;
        return data;
    }


    [Serializable]
    private class SettingData
    {
        public bool mIsSoundEffectEnabled = true;
        public bool mIsMusicEnabled = true;
    }
}


public class TimeState : IStatus
{
    public static readonly TimeState Default = new TimeState(1f, 1f);
    public static readonly TimeState StrictPause = new TimeState(0f, 0f);
    public static readonly TimeState NonStrictPause = new TimeState(0f, 1f);

    private float timeScale = 1f;
    private float realTimeScale = 1f;

    public TimeState()
        : this(1f, 1f)
    { }

    public TimeState(float timeScale, float realTimeScale)
    {
        this.timeScale = timeScale;
        this.realTimeScale = realTimeScale;
    }

    public void Apply()
    {
        TimeSetting.timeScale = timeScale;
        TimeSetting.realTimeScale = realTimeScale;
    }
}


public class TimeSetting : StateSetting<TimeState>
{
    private static float _realTimeScale = 1f;

    public static float timeScale
    {
        get { return Time.timeScale; }
        set { Time.timeScale = value; }
    }

    public static float realTimeScale 
    {
        get { return _realTimeScale; }
        set { _realTimeScale = Mathf.Max(0f, value); }
    }

    public static float realDeltaTime { get { return RealTime.deltaTime * _realTimeScale; } }

    public static TimeSetting Self = new TimeSetting();

    public override TimeState defaultState
    {
        get { return TimeState.Default; }
    }
}