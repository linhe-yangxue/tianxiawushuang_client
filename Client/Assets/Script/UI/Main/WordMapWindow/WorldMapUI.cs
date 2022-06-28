using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;
using System;
/*
public class WorldMapUI : MonoBehaviour {

	// Use this for initialization
	void Start () {

		EventCenter.Self.RegisterEvent("Button_world_map_back", new DefineFactory<Button_world_map_back>());
		EventCenter.Self.RegisterEvent("Button_world_map_shop", new DefineFactory<Button_world_map_shop>());
		EventCenter.Self.RegisterEvent("Button_world_map_blue", new DefineFactory<Button_world_map_blue>());
		EventCenter.Self.RegisterEvent("Button_world_map_green", new DefineFactory<Button_world_map_green>());
		EventCenter.Self.RegisterEvent("Button_world_map_gold", new DefineFactory<Button_world_map_gold>());
		EventCenter.Self.RegisterEvent("Button_world_map_shadow", new DefineFactory<Button_world_map_shadow>());
		EventCenter.Self.RegisterEvent("Button_world_map_red", new DefineFactory<Button_world_map_red>());
		EventCenter.Self.RegisterEvent("Button_world_map_active", new DefineFactory<Button_world_map_active>());
		EventCenter.Self.RegisterEvent("Button_world_map_boss", new DefineFactory<Button_world_map_boss>());
	
		DataCenter.Self.registerData ("WORLD_MAP_WINDOW", new WorldMapWindow(gameObject));
		DataCenter.SetData ("WORLD_MAP_WINDOW", "OPEN", true);
	}
}

public class WorldMapWindow : tWindow
{
	GameObject  mWorldMapBlue;
	GameObject  mWorldMapGold;
	GameObject  mWorldMapGreen;
	GameObject  mWorldMapRed;
	GameObject  mWorldMapShadow;
    GameObject  mWorldMapActive;

	public WorldMapWindow(GameObject obj)
	{
		mGameObjUI = obj;
	}

	public override void Open (object param)
	{
		tEvent t = Net.StartEvent("CS_RequestBossRecord");
		t.set ("WINDOW_NAME", "WORLD_MAP_WINDOW");
		t.DoEvent();
	}

	public override bool Refresh (object param)
	{
		tEvent evt = param as tEvent;
		int count = evt.get("COUNT");
		if(count != 0) GameCommon.SetUIVisiable (mGameObjUI, "ec_ui_wmkfp", true);
		return true;
	}

	public override void Init()
	{
		mWorldMapBlue = GameCommon.FindObject (mGameObjUI, "world_map_blue");
		mWorldMapGold = GameCommon.FindObject (mGameObjUI, "world_map_gold");
		mWorldMapGreen = GameCommon.FindObject (mGameObjUI, "world_map_green");
		mWorldMapRed = GameCommon.FindObject (mGameObjUI, "world_map_red");
		mWorldMapShadow = GameCommon.FindObject (mGameObjUI, "world_map_shadow");
		mWorldMapActive = GameCommon.FindObject(mGameObjUI, "world_map_active");
		
        ButtonIsValid(mWorldMapGreen, StagePropertyList.HasUnlocked(STAGE_TYPE.NORMAL, ELEMENT_TYPE.GREEN));
        ButtonIsValid(mWorldMapBlue, StagePropertyList.HasUnlocked(STAGE_TYPE.NORMAL, ELEMENT_TYPE.BLUE));
        ButtonIsValid(mWorldMapGold, StagePropertyList.HasUnlocked(STAGE_TYPE.NORMAL, ELEMENT_TYPE.GOLD));
        ButtonIsValid(mWorldMapRed, StagePropertyList.HasUnlocked(STAGE_TYPE.NORMAL, ELEMENT_TYPE.RED));
        ButtonIsValid(mWorldMapShadow, StagePropertyList.HasUnlocked(STAGE_TYPE.NORMAL, ELEMENT_TYPE.SHADOW));
        ButtonIsValid(mWorldMapActive, StagePropertyList.HasUnlocked(STAGE_TYPE.ACTIVE));
        GameCommon.SetUIVisiable(mGameObjUI, "ec_ui_bydxw", StagePropertyList.HasUnlocked(STAGE_TYPE.ACTIVE));

		GameCommon.SetUIVisiable (mGameObjUI, "ec_ui_wmkfp", false);
	}


	public void ButtonIsValid(GameObject obj, bool bIsValid)
	{
		GameObject mBackground = GameCommon.FindObject(obj, "Background");
		UISprite mSprite = mBackground.GetComponent<UISprite>();
		obj.GetComponent<SphereCollider>().enabled = bIsValid;
        if (bIsValid)
            mSprite.color = new Color(1.0f, 1.0f, 1.0f);
		else
            mSprite.color = new Color(0.4f, 0.4f, 0.4f);			
	}
}



public class Button_world_map_shop : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
		return true;
	}
}

public class Button_world_map_blue : CEvent
{
	public override bool _DoEvent()
	{
        CommonParam.mCurrentLevelType = STAGE_TYPE.NORMAL;
		CommonParam.mCurrentLevelElement = ELEMENT_TYPE.BLUE;
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
		return true;
	}
}

public class Button_world_map_green : CEvent
{
	public override bool _DoEvent()
	{
        CommonParam.mCurrentLevelType = STAGE_TYPE.NORMAL;
        CommonParam.mCurrentLevelElement = ELEMENT_TYPE.GREEN;
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
		return true;
	}
}

public class Button_world_map_gold : CEvent
{
	public override bool _DoEvent()
	{
        CommonParam.mCurrentLevelType = STAGE_TYPE.NORMAL;
        CommonParam.mCurrentLevelElement = ELEMENT_TYPE.GOLD;
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
		return true;
	}
}

public class Button_world_map_shadow : CEvent
{
	public override bool _DoEvent()
	{
        CommonParam.mCurrentLevelType = STAGE_TYPE.NORMAL;
        CommonParam.mCurrentLevelElement = ELEMENT_TYPE.SHADOW;
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
		return true;
	}
}

public class Button_world_map_red : CEvent
{
	public override bool _DoEvent()
	{
        CommonParam.mCurrentLevelType = STAGE_TYPE.NORMAL;
        CommonParam.mCurrentLevelElement = ELEMENT_TYPE.RED;
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
		return true;
	}
}

public class Button_world_map_active : CEvent
{
	public override bool _DoEvent()
	{
		//MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.BossRaidWindow);
        CommonParam.mCurrentLevelType = STAGE_TYPE.ACTIVE;
        CommonParam.mCurrentLevelElement = ELEMENT_TYPE.MAX;
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
		return true;
	}
}

public class Button_world_map_boss : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.BossRaidWindow);
		return true;
	}
}


public class Button_world_map_back : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
		return true;
	}
}
*/