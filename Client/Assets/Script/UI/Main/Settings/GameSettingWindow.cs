using UnityEngine;
using Logic;

public class GameSettingsWindow : tWindow
{
    public override void Init()
    {
        base.Init();

//        EventCenter.Self.RegisterEvent("Button_game_setting_music", new DefineFactory<Button_game_setting_music>());
		EventCenter.Self.RegisterEvent("Button_on_music_button", new DefineFactory<Button_on_music_button>());
		EventCenter.Self.RegisterEvent("Button_off_music_button", new DefineFactory<Button_off_music_button>());
		EventCenter.Self.RegisterEvent("Button_on_sound_button", new DefineFactory<Button_on_sound_button>());
		EventCenter.Self.RegisterEvent("Button_off_sound_button", new DefineFactory<Button_off_sound_button>());
//        EventCenter.Self.RegisterEvent("Button_game_setting_sound", new DefineFactory<Button_game_setting_sound>());
        EventCenter.Self.RegisterEvent("Button_game_setting_switch", new DefineFactory<Button_game_setting_switch>());
        EventCenter.Self.RegisterEvent("Button_game_setting_quit", new DefineFactory<Button_game_setting_quit>());
        EventCenter.Self.RegisterEvent("Button_game_setting_bk", new DefineFactory<Button_game_setting_bk>());
        EventCenter.Self.RegisterEvent("Button_game_settings_window_bg_btn", new DefineFactory<Button_game_setting_bk>());

        //by chenliang
        //begin

        EventCenter.Self.RegisterEvent("Button_game_setting_quit_middle", new DefineFactory<Button_game_setting_quit>());

        //end
    }

    public override void OnOpen()
    {
        //by chenliang
        //begin

        __CheckSDKUI();
        __GetButtons();

        //end
        Refresh(null);
    }
    //by chenliang
    //begin

    /// <summary>
    /// 根据SDK设置UI
    /// </summary>
    private void __CheckSDKUI()
    {
        GameObject tmpGOFullBtnParent = GameCommon.FindObject(mGameObjUI, "full_btn_parent");
        GameObject tmpGOMiddelQuitBtn = GameCommon.FindObject(mGameObjUI, "game_setting_quit_middle");
        if (tmpGOFullBtnParent != null)
            tmpGOFullBtnParent.SetActive(!CommonParam.isUseSDK);
        if (tmpGOMiddelQuitBtn != null)
            tmpGOMiddelQuitBtn.SetActive(CommonParam.isUseSDK);
    }

    //优化刷新
    private GameObject mGOOnMusicButton;
    private GameObject mGOOffMusicButton;
    private GameObject mGOMoveMusicButton;
    private GameObject mGOMoveMusicLabel;
    private GameObject mGOOnSoundButton;
    private GameObject mGOOffSoundButton;
    private GameObject mGOMoveSoundButton;
    private GameObject mGOMoveSoundLabel;

    private void __GetButtons()
    {
        mGOOnMusicButton = GameCommon.FindObject(mGameObjUI, "on_music_button");
        mGOOffMusicButton = GameCommon.FindObject(mGameObjUI, "off_music_button");
        mGOMoveMusicButton = GameCommon.FindObject(mGameObjUI, "move_music_button");
        mGOMoveMusicLabel = GameCommon.FindObject(mGameObjUI, "move_music_label");

        mGOOnSoundButton = GameCommon.FindObject(mGameObjUI, "on_sound_button");
        mGOOffSoundButton = GameCommon.FindObject(mGameObjUI, "off_sound_button");
        mGOMoveSoundButton = GameCommon.FindObject(mGameObjUI, "move_sound_button");
        mGOMoveSoundLabel = GameCommon.FindObject(mGameObjUI, "move_sound_label");
    }

    //end

    public override bool Refresh(object param)
    {
		string strMusic = Settings.IsMusicEnabled() ? "ON" : "OFF";
		string strSound = Settings.IsSoundEffectEnabled() ? "ON" : "OFF";
        GameCommon.SetUITextRecursively(mGameObjUI, "game_setting_music", strMusic);
        GameCommon.SetUITextRecursively(mGameObjUI, "game_setting_sound", strSound);
		if(strMusic=="ON")
		{
			GameOnMusic();
		}else 
		{
			GameOffMusic();
		}
		if(strSound == "ON")
		{
			GameOnSound();		
		}else 
		{
			GameOffSound();
		}
        return true;
    }

    //by chenliang
    //begin

// 	public void GameOnMusic()
// 	{
// 		GameCommon.FindUI("off_music_button").SetActive (false );
// 		GameCommon.FindUI("on_music_button").SetActive (true );
// 		GameObject obj = null;
// 		obj =  GameCommon.FindUI ("on_music_button");
// 		TweenScale.Begin(obj, 0.05f, new Vector3(1f, 1f, 1f));
// 		obj =  GameCommon.FindUI ("move_music_button");
// 		TweenPosition.Begin(obj, 0.05f, new Vector3(50, 0, 0));
// 		obj =  GameCommon.FindUI("move_music_label");
//         TweenPosition.Begin(obj, 0.05f, new Vector3(-25, -1, 0));
// 	}
// 	public void GameOffMusic()
// 	{
// 		GameCommon.FindUI("off_music_button").SetActive (true);
// 		GameCommon.FindUI("on_music_button").SetActive (false);
// 		GameObject obj = null;
// 		obj =  GameCommon.FindUI ("on_music_button");
// 		TweenScale.Begin(obj, 0.05f, new Vector3(0f, 0f, 0f));
// 		obj =  GameCommon.FindUI ("move_music_button");
//         TweenPosition.Begin(obj, 0.05f, new Vector3(-42.5f, 0, 0));
// 		obj =  GameCommon.FindUI ("move_music_label");
// 		TweenPosition.Begin(obj, 0.05f, new Vector3(29.51f, -1, 0));
// 	}
// 	public void GameOnSound()
// 	{
// 		GameCommon.FindUI("off_sound_button").SetActive (false );
// 		GameCommon.FindUI("on_sound_button").SetActive (true );
// 		GameObject obj = null;
// 		obj =  GameCommon.FindUI ("on_sound_button");
// 		TweenScale.Begin(obj, 0.05f, new Vector3(1f, 1f, 1f));
// 		obj =  GameCommon.FindUI ("move_sound_button");
// 		TweenPosition.Begin(obj, 0.05f, new Vector3(50, 0, 0));
// 		obj =  GameCommon.FindUI("move_sound_label");
//         TweenPosition.Begin(obj, 0.05f, new Vector3(-25, -1, 0));
// 	}
// 	public void GameOffSound()
// 	{
// 		GameCommon.FindUI("off_sound_button").SetActive (true);
// 		GameCommon.FindUI("on_sound_button").SetActive (false);
// 		GameObject obj = null;
// 		obj =  GameCommon.FindUI ("on_sound_button");
// 		TweenScale.Begin(obj, 0.05f, new Vector3(0f, 0f, 0f));
// 		obj =  GameCommon.FindUI ("move_sound_button");
//         TweenPosition.Begin(obj, 0.05f, new Vector3(-42.5f, 0, 0));
// 		obj =  GameCommon.FindUI ("move_sound_label");
// 		TweenPosition.Begin(obj, 0.05f, new Vector3(29.51f, -1, 0));
// 	}
//---------------------
    //优化刷新
    public void GameOnMusic()
    {
        if (mGOOffMusicButton != null)
            mGOOffMusicButton.SetActive(false);
        if (mGOOnMusicButton != null)
        {
            mGOOnMusicButton.SetActive(true);
            TweenScale.Begin(mGOOnMusicButton, 0.05f, new Vector3(1.0f, 1.0f, 1.0f));
        }
        if (mGOMoveMusicButton != null)
            TweenPosition.Begin(mGOMoveMusicButton, 0.05f, new Vector3(50.0f, 0.0f, 0.0f));
        if (mGOMoveMusicLabel != null)
            TweenPosition.Begin(mGOMoveMusicLabel, 0.05f, new Vector3(-25.0f, -1.0f, 0.0f));
    }
    public void GameOffMusic()
    {
        if (mGOOffMusicButton != null)
            mGOOffMusicButton.SetActive(true);
        if (mGOOnMusicButton != null)
        {
            mGOOnMusicButton.SetActive(false);
            TweenScale.Begin(mGOOnMusicButton, 0.05f, new Vector3(0.0f, 0.0f, 0.0f));
        }
        if (mGOMoveMusicButton != null)
            TweenPosition.Begin(mGOMoveMusicButton, 0.05f, new Vector3(-42.5f, 0.0f, 0.0f));
        if (mGOMoveMusicLabel != null)
            TweenPosition.Begin(mGOMoveMusicLabel, 0.05f, new Vector3(29.51f, -1.0f, 0.0f));
    }
    public void GameOnSound()
    {
        if (mGOOffSoundButton != null)
            mGOOffSoundButton.SetActive(false);
        if (mGOOnSoundButton != null)
        {
            mGOOnSoundButton.SetActive(true);
            TweenScale.Begin(mGOOnSoundButton, 0.05f, new Vector3(1.0f, 1.0f, 1.0f));
        }
        if (mGOMoveSoundButton != null)
            TweenPosition.Begin(mGOMoveSoundButton, 0.05f, new Vector3(50.0f, 0.0f, 0.0f));
        if (mGOMoveSoundLabel != null)
            TweenPosition.Begin(mGOMoveSoundLabel, 0.05f, new Vector3(-25.0f, -1.0f, 0.0f));
    }
    public void GameOffSound()
    {
        if (mGOOffSoundButton != null)
            mGOOffSoundButton.SetActive(true);
        if (mGOOnSoundButton != null)
        {
            mGOOnSoundButton.SetActive(false);
            TweenScale.Begin(mGOOnSoundButton, 0.05f, new Vector3(0.0f, 0.0f, 0.0f));
        }
        if (mGOMoveSoundButton != null)
            TweenPosition.Begin(mGOMoveSoundButton, 0.05f, new Vector3(-42.0f, 0.0f, 0.0f));
        if (mGOMoveSoundLabel != null)
            TweenPosition.Begin(mGOMoveSoundLabel, 0.05f, new Vector3(29.51f, -1.0f, 0.0f));
    }

    //end

}

//public class Button_game_setting_music : CEvent
//{
//    public override bool _DoEvent()
//    {
//        Settings.SetMusicEnabled(!Settings.IsMusicEnabled());
//        DataCenter.SetData("GAME_SETTINGS_WINDOW", "REFRESH", null);
//        return true;
//    }
//}
public class Button_on_music_button : CEvent
{
	public override bool _DoEvent()
	{
		GameSettingsWindow gsw = new GameSettingsWindow();
		gsw.GameOffMusic ();
		
		Settings.SetMusicEnabled(!Settings.IsMusicEnabled());
		DataCenter.SetData("GAME_SETTINGS_WINDOW", "REFRESH", null);
		return true;
	}
}
public class Button_off_music_button : CEvent
{
	public override bool _DoEvent()
	{
		GameSettingsWindow gsw = new GameSettingsWindow();
		gsw.GameOnMusic ();
		
		Settings.SetMusicEnabled(!Settings.IsMusicEnabled());
		DataCenter.SetData("GAME_SETTINGS_WINDOW", "REFRESH", null);
		return true;
	}
}
public class Button_on_sound_button : CEvent
{
	public override bool _DoEvent()
	{
		GameSettingsWindow gsw = new GameSettingsWindow();
		gsw.GameOffSound ();

		Settings.SetSoundEffectEnabled(!Settings.IsSoundEffectEnabled());
		DataCenter.SetData("GAME_SETTINGS_WINDOW", "REFRESH", null);
		return true;
	}
}
public class Button_off_sound_button : CEvent
{
	public override bool _DoEvent()
	{
		GameSettingsWindow gsw = new GameSettingsWindow();
		gsw.GameOnSound ();
		
		Settings.SetSoundEffectEnabled(!Settings.IsSoundEffectEnabled());
		DataCenter.SetData("GAME_SETTINGS_WINDOW", "REFRESH", null);
		return true;
	}
}
//public class Button_game_setting_sound : CEvent
//{
//    public override bool _DoEvent()
//    {
//        Settings.SetSoundEffectEnabled(!Settings.IsSoundEffectEnabled());
//        DataCenter.SetData("GAME_SETTINGS_WINDOW", "REFRESH", null);
//        return true;
//    }
//}

public class Button_game_setting_switch : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GAME_SETTINGS_WINDOW");
        Settings.SwitchAccount();
        return true;
    }
}

public class Button_game_setting_quit : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GAME_SETTINGS_WINDOW");
        //by chenliang
        //begin

//        Settings.QuitGame();
//---------------
        //改为登出游戏
        Settings.Logout();

        //end
        return true;
    }
}

public class Button_game_setting_bk : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GAME_SETTINGS_WINDOW");
        return true;
    }
}