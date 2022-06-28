using System.Collections;
using Logic;
using DataTable;
using UnityEngine;
using System;

public class BossStageInfo
{
	public int useBeatDemonCardCount;
	public int attackMulti;
}

/// <summary>
/// Boss战斗信息窗口
/// BOSS_STAGE_INFO_WINDOW	boss_ready_window	BossStageInfoWindow
/// </summary>
public class BossStageInfoWindow : tWindow
{
	StageProperty property;

	public override void Init()
	{
        EventCenter.Self.RegisterEvent("Button_boss_ready_adjust_button", new DefineFactoryLog<Button_boss_ready_adjust_button>());
		EventCenter.Self.RegisterEvent("Button_AddyieldaBtn", new DefineFactoryLog<Button_add_beatdeomoncard_button>());
//		EventCenter.Self.RegisterEvent("Button_adjust_button", new DefineFactoryLog<Button_ChangeTeamBtn>());
		EventCenter.Self.RegisterEvent("Button_common_attack_button", new DefineFactoryLog<Button_normal_attack_button>());
		EventCenter.Self.RegisterEvent("Button_max_attack_button", new DefineFactoryLog<Button_max_attack_button>());
		EventCenter.Self.RegisterEvent("Button_boss_list_close_button", new DefineFactoryLog<Button_boss_list_close_button>());
	}
	
	public override void Open(object param)
	{
		base.Open(param);
		
		int stageIndex = new Data(param);
		
		if (stageIndex > 0)
		{
			DataCenter.Set("CURRENT_STAGE", stageIndex);
			property = StageProperty.Create(stageIndex);
			Refresh(null);

			//调整队伍按钮
            UIImageButton imageBtn = GameCommon.FindObject(mGameObjUI, "boss_ready_adjust_button").GetComponent<UIImageButton>();
			if(imageBtn != null)
				imageBtn.isEnabled = true;
		}
	}

	public override bool Refresh (object param)
	{
		/*
		if (StageProperty.IsMainStage(property.stageType))
		{
			RefreshNormalInfo();
		}
		else if (StageProperty.IsActiveStage(property.stageType))
		{
			if (!GameCommon.bIsWindowOpen("ACTIVE_STAGE_WINDOW"))
				DataCenter.OpenWindow("ACTIVE_STAGE_WINDOW");
			
			RefreshActiveInfo();
		}
		else if (property.stageType == STAGE_TYPE.CHAOS)
		{
			if (!GameCommon.bIsWindowOpen("BOSS_RAID_WINDOW"))
				DataCenter.OpenWindow("BOSS_RAID_WINDOW");
			
			__RefreshBossInfo();
			__RefreshRoleInfo();
			__RefreshFeatEvent();
		}
		*/

		__RefreshBossInfo();
		__RefreshRoleInfo();
		__RefreshFeatEvent();
		
        //added by xuke 添加队伍调整红点逻辑
        GameObject _teamAdjustBtnObj = GameCommon.FindObject(mGameObjUI, "boss_ready_adjust_button");
        GameCommon.SetNewMarkVisible(_teamAdjustBtnObj,TeamManager.CheckTeamHasNewMark());
        //end
		return true;
	}

	private void __RefreshBossInfo()
	{
		__ShowBossModel(true);

		Boss_GetDemonBossList_BossData bossData = getObject("BOSS_DATA") as Boss_GetDemonBossList_BossData;
		NiceData normalButtonData = GameCommon.GetButtonData(mGameObjUI, "common_attack_button");
		if (normalButtonData != null)
			normalButtonData.set("BOSS_DATA", bossData);
		NiceData powerButtonData = GameCommon.GetButtonData(mGameObjUI, "max_attack_button");
		if (powerButtonData != null)
			powerButtonData.set("BOSS_DATA", bossData);
		DataRecord bossConfig = DataCenter.mBossConfig.GetRecord(bossData.tid);
		if (bossConfig == null)
		{
			EventCenter.Log(LOG_LEVEL.ERROR, "ERROR: no exist boss config>" + bossData.tid.ToString());
			return;
		}

		//名称
        GameCommon.FindObject(mGameObjUI, "boss_name").GetComponent<UILabel>().color = GameCommon.GetNameColorByQuality(bossData.quality);
		SetText("boss_name", bossConfig.get("NAME"));
		//等级
		SetText("boss_level_label", bossData.bossLevel.ToString()/*bossConfig.get("LV").ToString()*/);
		//逃走时间
		GameObject escapeItem = GetSub("boss_info_subcell");
		SetCountdownTime(escapeItem, "boss_escape_time", bossData.findTime + bossData.standingTime/*BossRaidWindow.GetBossEscapeTime(bossData.findTime)*/);

		//血量
        int maxHp = BigBoss.GetBossMaxHp(bossData.quality, bossData.bossLevel);//bossConfig.get("BASE_HP");
		int nowHp = bossData.hpLeft;
		string strHp = nowHp.ToString();
		strHp += "/";
		strHp += maxHp.ToString();
		SetText("boss_hp_info", strHp);
		UISlider hpSlider = GameCommon.FindComponent<UISlider>(mGameObjUI, "boss_hp_bar_red");
		if(hpSlider != null)
		{
			hpSlider.value = (float)nowHp / (float)maxHp;
			GameCommon.SetUIVisiable (hpSlider.gameObject, "Foreground", Convert.ToBoolean(hpSlider.value));
		}
	}
	private void __RefreshRoleInfo()
	{
		//战斗力
		SetText("fight_strength_number", GameCommon.GetPower().ToString("f0"));

		//降魔令
		GameCommon.SetUIText(GetSub("AddyieldaBtn"), "StaminaNum", RoleLogicData.Self.beatDemonCard.ToString() + "/10");

		//攻击倍数
		int maxBreakLevel = GetMaxBreakLevel();
		GameCommon.SetUIText(GetSub("tips_label"), "number", maxBreakLevel.ToString());

		//普通攻击降魔令
		GameCommon.SetUIText(GetSub("normal_consume_yield"), "StaminaNum", "1");

		//全力一击降魔令
		GameCommon.SetUIText(GetSub("consume_yield"), "StaminaNum", __IsReduceHalfBeatDemonCard() ? "1" : "2");

        //调整队伍
        NiceData tmpBtnData = GameCommon.GetButtonData(mGameObjUI, "boss_ready_adjust_button");
        if (tmpBtnData != null)
        {
            Boss_GetDemonBossList_BossData tmpBossData = getObject("BOSS_DATA") as Boss_GetDemonBossList_BossData;
            if (tmpBossData != null)
                tmpBtnData.set("BOSS_DATA", tmpBossData);
        }
	}
	private void __RefreshFeatEvent()
	{
        for (int i = 0; i < 2; i++)
        {
            DataRecord record = DataCenter.mFeatEventConfig.GetRecord(i + 1);
            int openHour = (int)record.getObject("OPEN_HOUR");
            string strOpenHour = openHour.ToString();
            if (strOpenHour.Length == 1)
                strOpenHour = "0" + strOpenHour;
            int openMinute = (int)record.getObject("OPEN_MINUTE");
            string strOpenMinute = openMinute.ToString();
            if (strOpenMinute.Length == 1)
                strOpenMinute = "0" + strOpenMinute;
            int closeHour = (int)record.getObject("CLOSE_HOUR");
            string strCloseHour = closeHour.ToString();
            if (strCloseHour.Length == 1)
                strCloseHour = "0" + strCloseHour;
            int closeMinute = (int)record.getObject("CLOSE_MINUTE");
            string strCloseMinute = closeMinute.ToString();
            if (strCloseMinute.Length == 1)
                strCloseMinute = "0" + strCloseMinute;
            UILabel tmpLabel = GameCommon.FindComponent<UILabel>(GetSub("info_activity_label"), "time_label0" + (i + 1).ToString());
            tmpLabel.text = strOpenHour + ":" + strOpenMinute + "-" + strCloseHour + ":" + strCloseMinute;
            UILabel tmpAffectLabel = GameCommon.FindComponent<UILabel>(tmpLabel.gameObject, "attect_label");
            tmpAffectLabel.color = IsFeatEventOccured(i + 1) ? Color.red : Color.white;
        }
	}

    public static int GetMaxBreakLevel()
	{
		int maxBreakLevel = -1;
		for(int i = (int)TEAM_POS.CHARACTER; i < (int)TEAM_POS.MAX; i++)
		{
			ActiveData data = TeamManager.GetActiveDataByTeamPos(i);
			if(data != null && data.breakLevel > maxBreakLevel)
				maxBreakLevel = data.breakLevel;
		}
		return maxBreakLevel;
	}

	public static bool IsFeatEventOccured(int eventIndex)
	{
		DataRecord record = DataCenter.mFeatEventConfig.GetRecord(eventIndex);
		int openHour;
		record.get("OPEN_HOUR", out openHour);
		int openMinute;
		record.get("OPEN_MINUTE", out openMinute);
		int closeHour;
		record.get("CLOSE_HOUR", out closeHour);
		int closeMinute;
		record.get("CLOSE_MINUTE", out closeMinute);
		DateTime currDateTime = BossRaidWindow.Get1970TimeFromServer(CommonParam.NowServerTime());
		int tmpCurrTime = currDateTime.Hour * 60 + currDateTime.Minute;
		int tmpOpenTime = openHour * 60 + openMinute;
		int tmpCloseTime = closeHour * 60 + closeMinute;
		if(tmpOpenTime <= tmpCurrTime && tmpCurrTime <= tmpCloseTime)
		//if (currDateTime.Hour >= openHour && currDateTime.Hour <= closeHour && currDateTime.Minute >= openMinute && currDateTime.Minute <= closeMinute) 
			return true;
		return false;
	}
	private bool __IsReduceHalfBeatDemonCard()
	{
		return IsFeatEventOccured(1);
	}

    private void __ShowBossModel(bool visible)
    {
		/*
        GameObject obj = GetSub("boss_model");

        if (!visible)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);
        */
        Boss_GetDemonBossList_BossData bossData = getObject("BOSS_DATA") as Boss_GetDemonBossList_BossData;

        if (bossData != null)
        {
            int bossID = bossData.tid;
            ActiveBirthForUI birthUI = GameCommon.FindComponent<ActiveBirthForUI>(mGameObjUI, "UIPoint");
            if (birthUI != null)
            {
                if (birthUI.mActiveObject != null)
                    birthUI.mActiveObject.OnDestroy();

                birthUI.mBirthConfigIndex = bossID;
                birthUI.mObjectType = (int)OBJECT_TYPE.BIG_BOSS;
                birthUI.Init();
                if (birthUI.mActiveObject != null)
                {
                    birthUI.mActiveObject.SetScale(80f);
                    birthUI.mActiveObject.PlayAnim("idle", true);

                    BossBirthOnApearUI modelScript = birthUI.mActiveObject.mMainObject.AddComponent<BossBirthOnApearUI>();
                    if (modelScript != null)
                        modelScript.mActiveObject = birthUI.mActiveObject;
                }
            }
        }
    }
}

/// <summary>
/// 调整队伍
/// </summary>
class Button_boss_ready_adjust_button : CEvent
{
    private GameObject mBossMainObj;
    private GameObject mBossBackMainObj;
    private GameObject mBossStageInfoObj;
    private GameObject mTrialObj;
    private GameObject mTrialBackObj;

    public override bool _DoEvent()
    {
        if (MainUIScript.mCurIndex == MAIN_WINDOW_INDEX.WorldMapWindow)
            DataCenter.CloseWindow("SCROLL_WORLD_MAP_WINDOW");
        DataCenter.CloseWindow("BOSS_STAGE_INFO_WINDOW");
        DataCenter.CloseWindow("BOSS_RAID_WINDOW");
        DataCenter.CloseWindow("TRIAL_WINDOW");
        DataCenter.CloseWindow("TRIAL_WINDOW_BACK");
        MainUIScript.Self.mWindowBackAction = __OnAdjustComplete;
        if (MainProcess.mStage != null)
        {
            MainProcess.ClearBattle();
            MainGameSceneLoadingWindow.FinishedCallback = () =>
            {
                DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);
            };
        }
        else
            DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);

        return true;
    }

    private void __OnAdjustComplete()
    {
        GetPathHandlerDic.HandlerDic[GET_PARTH_TYPE.TIAN_MO]();
        Boss_GetDemonBossList_BossData bossData = getObject("BOSS_DATA") as Boss_GetDemonBossList_BossData;
        if (bossData != null)
        {
            //   MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
            DataRecord config = DataCenter.mBossConfig.GetRecord(bossData.tid);
            DataCenter.SetData("BOSS_STAGE_INFO_WINDOW", "BOSS_DATA", bossData);
            DataCenter.OpenWindow("BOSS_STAGE_INFO_WINDOW", (int)config.get("SCENE_ID"));
        }
    }
}

/// <summary>
/// 增加降魔令
/// </summary>
class Button_add_beatdeomoncard_button : CEvent
{
	public override bool _DoEvent ()
	{
		//TODO

        //
        //GrabTreasurePeaceData tmpPeaceData = new GrabTreasurePeaceData();
        ////设置数据
        //tmpPeaceData.tid = GrabTreasurePeaceID.Instace[GRABTREASURE_PEACE_TYPE.ONE_HOUR];
        //tmpPeaceData.PeaceType = GRABTREASURE_PEACE_TYPE.ONE_HOUR;

        ResourceHintData _resourceHintData = new ResourceHintData();
		_resourceHintData.tid = (int)ITEM_TYPE.BEATDEMONCARD_ITEM;
		_resourceHintData.ResourceType = RESOURCE_HINT_TYPE.BEATDEMONCARD;

        DataCenter.OpenWindow("ADD_VITALITY_UP_WINDOW", _resourceHintData);

        //
		return true;
	}
}

/// <summary>
/// 普通攻击
/// </summary>
class Button_normal_attack_button : CEvent
{
	public override bool _DoEvent ()
	{
		Boss_GetDemonBossList_BossData bossData = getObject("BOSS_DATA") as Boss_GetDemonBossList_BossData;
		BossBattleNetManager.RequestDemonBossStart(bossData, 1, BossStageInfoWindow.IsFeatEventOccured(1) ? 1 : (BossStageInfoWindow.IsFeatEventOccured(2) ? 2 : -1));

		return true;
	}
}
/// <summary>
/// 全力攻击
/// </summary>
class Button_max_attack_button : CEvent
{
	public override bool _DoEvent ()
	{
		Boss_GetDemonBossList_BossData bossData = getObject("BOSS_DATA") as Boss_GetDemonBossList_BossData;
		BossBattleNetManager.RequestDemonBossStart(bossData, 2, BossStageInfoWindow.IsFeatEventOccured(1) ? 1 : (BossStageInfoWindow.IsFeatEventOccured(2) ? 2 : -1));
		
		return true;
	}
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_boss_list_close_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("BOSS_STAGE_INFO_WINDOW");
		GameObject _panelObj = GameObject .Find ("TopAnchor/info_group_window");
        if (_panelObj != null) 
        {
           UIPanel infor = _panelObj.GetComponent <UIPanel>();
           infor.depth = 4;
        }
		
        BossRaidWindow raidWin = DataCenter.GetData("BOSS_RAID_WINDOW") as BossRaidWindow;
        bool isReturnStage = (bool)raidWin.getObject("IS_RETURN_STAGE");
        if (isReturnStage)
        {
            BossAppearWindow bossWindow = DataCenter.GetData("BOSS_APPEAR_WINDOW") as BossAppearWindow;
            bossWindow.mRequestEvent.NextActive();
        }
        raidWin.set("IS_RETURN_STAGE", false);

		return true;
	}
}
