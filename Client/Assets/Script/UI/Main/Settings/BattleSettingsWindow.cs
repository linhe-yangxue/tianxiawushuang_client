using UnityEngine;
using Logic;

public class BattleSettingsWindow : tWindow
{
    public override void Init()
    {
        base.Init();

//        EventCenter.Self.RegisterEvent("Button_battle_setting_music", new DefineFactory<Button_battle_setting_music>());
		EventCenter.Self.RegisterEvent("Button_battle_on_music_button", new DefineFactory<Button_battle_on_music_button>());
		EventCenter.Self.RegisterEvent("Button_battle_off_music_button", new DefineFactory<Button_battle_off_music_button>());
		EventCenter.Self.RegisterEvent("Button_battle_on_sound_button", new DefineFactory<Button_battle_on_sound_button>());
		EventCenter.Self.RegisterEvent("Button_battle_off_sound_button", new DefineFactory<Button_battle_off_sound_button>());
//        EventCenter.Self.RegisterEvent("Button_battle_setting_sound", new DefineFactory<Button_battle_setting_sound>());
        EventCenter.Self.RegisterEvent("Button_battle_setting_resume", new DefineFactory<Button_battle_setting_resume>());
        EventCenter.Self.RegisterEvent("Button_battle_setting_quit", new DefineFactory<Button_battle_setting_quit>());
        EventCenter.Self.RegisterEvent("Button_battle_setting_bk", new DefineFactory<Button_battle_setting_bk>());
    }

    public override void OnOpen()
    {
        Refresh(null);
        Settings.SetGamePaused(true);
    }

    public override bool Refresh(object param)
    {
		string strMusic = Settings.IsMusicEnabled() ? "ON" : "OFF";
		string strSound = Settings.IsSoundEffectEnabled() ? "ON" : "OFF";
        GameCommon.SetUITextRecursively(mGameObjUI, "battle_setting_music", strMusic);
        GameCommon.SetUITextRecursively(mGameObjUI, "battle_setting_sound", strSound);
		if(strMusic=="ON")
		{
			BattleOnMusic();
		}else 
		{
			BattleOffMusic();
		}
		if(strSound == "ON")
		{
			BattleOnSound();
		}else 
		{
			BattleOffSound();
		}
        return true;
    }

    public override void Close()
    {
        Settings.SetGamePaused(false);
        base.Close();       
    }

	public void BattleOnMusic()
	{
		GameCommon.FindUI("battle_off_music_button").SetActive (false );
		GameCommon.FindUI("battle_on_music_button").SetActive (true );
		GameObject obj = null;
		obj =  GameCommon.FindUI ("battle_on_music_button");
		TweenScale.Begin(obj, 0.05f, new Vector3(1f, 1f, 1f));
		obj =  GameCommon.FindUI ("battle_move_music_button");
		TweenPosition.Begin(obj, 0.05f, new Vector3(50, 0, 0));
		obj =  GameCommon.FindUI("battle_move_music_label");
        TweenPosition.Begin(obj, 0.05f, new Vector3(-25, -1, 0));
	}
	public void BattleOffMusic()
	{
		GameCommon.FindUI("battle_off_music_button").SetActive (true);
		GameCommon.FindUI("battle_on_music_button").SetActive (false);
		GameObject obj = null;
		obj =  GameCommon.FindUI ("battle_on_music_button");
		TweenScale.Begin(obj, 0.05f, new Vector3(0f, 0f, 0f));
		obj =  GameCommon.FindUI ("battle_move_music_button");
        TweenPosition.Begin(obj, 0.05f, new Vector3(-42.5f, 0, 0));
		obj =  GameCommon.FindUI ("battle_move_music_label");
		TweenPosition.Begin(obj, 0.05f, new Vector3(29.51f, -1, 0));
	}
	public void BattleOnSound()
	{
		GameCommon.FindUI("battle_off_sound_button").SetActive (false );
		GameCommon.FindUI("battle_on_sound_button").SetActive (true );
		GameObject obj = null;
		obj =  GameCommon.FindUI ("battle_on_sound_button");
		TweenScale.Begin(obj, 0.05f, new Vector3(1f, 1f, 1f));
		obj =  GameCommon.FindUI ("battle_move_sound_button");
		TweenPosition.Begin(obj, 0.05f, new Vector3(50, 0, 0));
		obj =  GameCommon.FindUI("battle_move_sound_label");
        TweenPosition.Begin(obj, 0.05f, new Vector3(-25, -1, 0));
	}
	public void BattleOffSound()
	{
		GameCommon.FindUI("battle_off_sound_button").SetActive (true);
		GameCommon.FindUI("battle_on_sound_button").SetActive (false);
		GameObject obj = null;
		obj =  GameCommon.FindUI ("battle_on_sound_button");
		TweenScale.Begin(obj, 0.05f, new Vector3(0f, 0f, 0f));
		obj =  GameCommon.FindUI ("battle_move_sound_button");
        TweenPosition.Begin(obj, 0.05f, new Vector3(-42.5f, 0, 0));
		obj =  GameCommon.FindUI ("battle_move_sound_label");
		TweenPosition.Begin(obj, 0.05f, new Vector3(29.51f, -1, 0));
	}
	
}

//public class Button_battle_setting_music : CEvent 
//{
//    public override bool _DoEvent()
//    {
//        Settings.SetMusicEnabled(!Settings.IsMusicEnabled());
//        DataCenter.SetData("BATTLE_SETTINGS_WINDOW", "REFRESH", null);
//        return true;
//    }
//}
public class Button_battle_on_music_button : CEvent
{
	public override bool _DoEvent()
	{
		BattleSettingsWindow btw = new BattleSettingsWindow();
		btw.BattleOffMusic ();

		Settings.SetMusicEnabled(!Settings.IsMusicEnabled());
		DataCenter.SetData("BATTLE_SETTINGS_WINDOW", "REFRESH", null);
		return true;
	}
}
public class Button_battle_off_music_button : CEvent
{
	public override bool _DoEvent()
	{
		BattleSettingsWindow btw = new BattleSettingsWindow();
		btw.BattleOnMusic ();
		
		Settings.SetMusicEnabled(!Settings.IsMusicEnabled());
		DataCenter.SetData("BATTLE_SETTINGS_WINDOW", "REFRESH", null);
		return true;
	}
}
public class Button_battle_on_sound_button : CEvent
{
	public override bool _DoEvent()
	{
		BattleSettingsWindow btw = new BattleSettingsWindow();
		btw.BattleOffSound ();
		
		Settings.SetSoundEffectEnabled(!Settings.IsSoundEffectEnabled());
		DataCenter.SetData("BATTLE_SETTINGS_WINDOW", "REFRESH", null);
		return true;
	}
}
public class Button_battle_off_sound_button : CEvent
{
	public override bool _DoEvent()
	{
		BattleSettingsWindow btw = new BattleSettingsWindow();
		btw.BattleOnSound ();
		
		Settings.SetSoundEffectEnabled(!Settings.IsSoundEffectEnabled());
		DataCenter.SetData("BATTLE_SETTINGS_WINDOW", "REFRESH", null);
		return true;
	}
}
//public class Button_battle_setting_sound : CEvent
//{
//    public override bool _DoEvent()
//    {
//        Settings.SetSoundEffectEnabled(!Settings.IsSoundEffectEnabled());
//        DataCenter.SetData("BATTLE_SETTINGS_WINDOW", "REFRESH", null);
//        return true;
//    }
//}

public class Button_battle_setting_resume : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("BATTLE_SETTINGS_WINDOW");     
        return true;
    }
}

public class Button_battle_setting_quit : CEvent
{
    public override bool _DoEvent()
    {
        //活动副本中途推出设定
        DataCenter.Set("DAILAY_STAGE_GOTO_LINEUP", false);
        if (DataCenter.Get("IS_DAILYS_STAGE_BATTLE"))
        {
            DataCenter.Set("IS_DAILYS_STAGE_BATTLE", false);
            DataCenter.Set("IS_DAILY_STAGE_BACK", true);
        }
        else
        {
            DataCenter.Set("IS_DAILY_STAGE_BACK", false);
        }

        DataCenter.CloseWindow("BATTLE_SETTINGS_WINDOW");
		if(PetLogicData.mFreindPetData != null) PetLogicData.mFreindPetData = null;
		if(FriendLogicData.mSelectPlayerData != null) FriendLogicData.mSelectPlayerData = null;
        Settings.QuitBattle();
        return true;
    }
}

public class Button_battle_setting_bk : CEvent
{
    public override bool _DoEvent()
    {
        return true;
    }
}