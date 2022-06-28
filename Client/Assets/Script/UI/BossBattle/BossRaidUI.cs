using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

/// <summary>
/// Boss列表界面
/// BOSS_RAID_WINDOW	boss_raid_window	BossRaidWindow
/// </summary>
public class BossRaidWindow : tWindow
{
    private static string msBeatDemonCardTimerName = "BOSS_TIMER";

	//计算时间
	private static DateTime sm_1970 = new DateTime(1970, 1, 1, 8, 0, 0);

    private static bool msHasNewBossAppear = false;     //新的天魔出现
    private static SC_Boss_GetDemonBossList msBossList;         //当前天魔列表

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_fight_button", new DefineFactoryLog<Button_fight_button>());
		EventCenter.Self.RegisterEvent("Button_refresh_btn", new DefineFactoryLog<Button_refresh_btn>());

		//界面按钮事件
		EventCenter.Self.RegisterEvent("Button_refresh_boss_list_button", new DefineFactoryLog<Button_refresh_boss_list_button>());
        EventCenter.Self.RegisterEvent("Button_boss_list_rank_btn", new DefineFactoryLog<Button_boss_list_rank_btn>());
		EventCenter.Self.RegisterEvent("Button_exploit_btn", new DefineFactoryLog<Button_exploit_btn>());
		EventCenter.Self.RegisterEvent("Button_feats_btn", new DefineFactoryLog<Button_feats_btn>());
        EventCenter.Self.RegisterEvent("Button_boss_world_map_back", new DefineFactoryLog<Button_boss_world_map_back>());

        DataCenter.SetData("BOSS_RAID_WINDOW", "SELF_MERIT", (long)0);
        DataCenter.SetData("BOSS_RAID_WINDOW", "SELF_DAMAGEOUTPUT", 0);
	}

    public override void OnOpen()
    {
        DataCenter.OpenWindow("BOSS_BACK_WINDOW");

        set("IS_RETURN_STAGE", false);
        //隐藏新的天魔快捷入口
        BossRaidWindow.HasNewBossAppear = false;

		__InitData();
		__InitUI();

		__RequestData();
    }

    public override void OnClose()
    {
//         GetCurUIGameObject("no_boss_tips", false);
//         GetCurUIGameObject("boss_list_group", false);
        DataCenter.CloseWindow("BOSS_BACK_WINDOW");
        base.OnClose();

        //刷新快捷入口界面
        DataCenter.SetData("TRIAL_EASY_JUMP_WINDOW", "REFRESH", null);
    }

	private void __InitData()
	{
	}
	private void __InitUI()
	{
		GameCommon.SetResIcon (mGameObjUI,"icon_feats",(int)ITEM_TYPE.BATTLEACHV,false);
        //GameCommon.SetOnlyItemIcon(GetSub("icon_feats"), (int)ITEM_TYPE.BATTLEACHV);
		GameCommon.SetUIText(GetSub("top_info"), "cur_number", "");
		GameCommon.SetUIText(GetSub("top_info"), "highest_number", "");
		GameCommon.SetUIText(GetSub("top_info"), "feats_num", "0");
		GameCommon.SetUIText(GetSub("boss_refresh_time_title_label"), "boss_refresh_time_label", "--");
	}

	private void __RequestData()
	{
		BossBattleNetManager.RequestGetDamageAndMerit();
	}

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REQUEST_DATA":
                {
                    __RequestData();
                } break;
            case "REFRESH_ROLE_DATA":
                {
                    __RefreshRoleData(objVal);
                } break;
            case "REFRESH_BOSS_LIST":
                {
                    __RefreshBossList(objVal);
                    if (objVal is SC_Boss_GetDemonBossList)
                        msBossList = objVal as SC_Boss_GetDemonBossList;
                } break;
            case "RESTORE_DAMAGE_MERIT":
                {
                    if (objVal is SC_Boss_GetDamageAndMerit)
                    {
                        SC_Boss_GetDamageAndMerit tmpGetDamageAndMerit = objVal as SC_Boss_GetDamageAndMerit;
                        DataCenter.SetData("BOSS_RAID_WINDOW", "SELF_MERIT", tmpGetDamageAndMerit.merit);
                        DataCenter.SetData("BOSS_RAID_WINDOW", "SELF_DAMAGEOUTPUT", tmpGetDamageAndMerit.damageOutput);
                    }
                }break;
            case "RESTORE_BOSS_LIST":
                {
                    msBossList = objVal as SC_Boss_GetDemonBossList;
                }break;
        }
    }

    public static string BeatDemonCardTimerName
    {
        get { return msBeatDemonCardTimerName; }
    }
    private void __OnBeatDemonCardRecoverOver(CountdownTimerData timerData)
    {
        RoleLogicData.Self.AddBeatDemonCard(1);
        if (RoleLogicData.Self.beatDemonCard < 10)
            GlobalModule.Instance.countdownTimer.StartTimer(msBeatDemonCardTimerName, true);
    }

    public void OnBossOverTime(object param)
    {
        GameObject item = param as GameObject;
		if (item != null)
        {
            UIImageButton but = GameCommon.GetUIButton(item, "damage_button");
            if (but != null)
                but.gameObject.SetActive(false);

            but = GameCommon.GetUIButton(item, "fight_button");
            if (but != null)
                but.gameObject.SetActive(false);
        }
    }

	private void __RefreshRoleData(object param)
	{
		SC_Boss_GetDamageAndMerit roleData = param as SC_Boss_GetDamageAndMerit;
		if(roleData != null)
		{
			//功勋
			string meritValue = GameCommon.ShowNumUI(roleData.merit);
			DataCenter.SetData("BOSS_RAID_WINDOW", "SELF_MERIT", roleData.merit);
			meritValue += "（";
			meritValue += (roleData.selfMeritRank != -1 ? ("第" + roleData.selfMeritRank.ToString() + "名") : "未上榜");
			meritValue += "）";
			GameCommon.SetUIText(GetSub("top_info"), "cur_number", meritValue);

			//伤害
			string damageOutputValue = GameCommon.ShowNumUI(roleData.damageOutput);
			DataCenter.SetData("BOSS_RAID_WINDOW", "SELF_DAMAGEOUTPUT", roleData.damageOutput);
			damageOutputValue += "（";
			damageOutputValue += (roleData.selfDamageRank != -1 ? ("第" + roleData.selfDamageRank.ToString() + "名") : "未上榜");
			damageOutputValue += "）";
			GameCommon.SetUIText(GetSub("top_info"), "highest_number", damageOutputValue);

			//战功
			GameCommon.SetUIText(GetSub("top_info"), "feats_num", GameCommon.ShowNumUI(RoleLogicData.Self.battleAchv));
		}
		else
		{
			//TODO
		}
	}

	private void __RefreshBossList(object param)
    {
		SC_Boss_GetDemonBossList bossList = param as SC_Boss_GetDemonBossList;
        if (bossList != null) {
            //设置重置时间
            long resetTime = __GetBossResetTime();
            SetCountdownTime(GetSub("boss_refresh_time_title_label"), "boss_refresh_time_label", resetTime, new CallBack(this, "BossRefresh", true));

            Boss_GetDemonBossList_BossData[] bossRecord = bossList.arr;

            if (bossRecord == null) {
                EventCenter.Log(LOG_LEVEL.ERROR, "Reply boss record is null");
                return;
            }

            int count = bossRecord.Length;
            //			UIGridContainer grid = mGameObjUI.GetComponentInChildren<UIGridContainer>();
            var haveBoss=count!=0;
            GetCurUIGameObject("no_boss_tips", !haveBoss);
            GetCurUIGameObject("boss_list_group", haveBoss);

            if (haveBoss) {
                UIGridContainer grid = GameCommon.FindObject(mGameObjUI, "grid").GetComponent<UIGridContainer>();
                if (grid != null)
                    grid.MaxCount = count;

                //无boss列表
                //			SetVisible ("no_boss_label", !Convert.ToBoolean(count));

                int i = 0;
                foreach (Boss_GetDemonBossList_BossData bossData in bossRecord) {
                    string finderDBID = bossData.finderId;
                    string finderName = bossData.finderName;

                    if (i >= count)
                        break;

                    int bossID = bossData.tid;
                    if (bossID <= 0) {
                        EventCenter.Log(LOG_LEVEL.ERROR, "ERROR: data no exist BOSS_ID");
                        continue;
                    }

                    if (grid != null) {
                        GameObject item = grid.controlList[i++];
                        DataRecord configRe = DataCenter.mBossConfig.GetRecord(bossID);
                        if (configRe == null) {
                            EventCenter.Log(LOG_LEVEL.ERROR, "ERROR: no exist boss config>" + bossID.ToString());
                            continue;
                        }

                        int maxHp = BigBoss.GetBossMaxHp(bossData.quality, bossData.bossLevel);//configRe.get("BASE_HP");
                        int nowHp = bossData.hpLeft;
                        string strHp = nowHp.ToString();
                        strHp += "/";
                        strHp += maxHp.ToString();
                        GameCommon.SetUIText(item, "boss_hp_info", strHp);
                        GameCommon.FindObject(item, "boss_name").GetComponent<UILabel>().color = GameCommon.GetNameColorByQuality(bossData.quality);
                        GameCommon.SetUIText(item, "boss_name", (string)configRe.get("NAME"));
                        GameCommon.SetUIText(item, "finder_name", finderName);
						GameCommon.FindObject (item, "finder_name").GetComponent<UILabel>().color = GameCommon.GetNameColor (bossData.finderTid);

                        //设置Boss逃脱时间
                        if (nowHp > 0) {
                            long bossEscapeTime = bossData.findTime + bossData.standingTime;//GetBossEscapeTime(bossData.findTime);
                            SetCountdownTime(item, "boss_escape_time", bossEscapeTime, new CallBack(this, "OnBossOverTime", item));
                        } else
                            GameCommon.SetUIVisiable(item, "boss_escape_time", false);

                        GameCommon.SetUIText(item, "boss_level_label", bossData.bossLevel.ToString()/*configRe.get("LV").ToString()*/);

                        int element = configRe["ELEMENT_INDEX"];
                        GameCommon.SetElementIcon(item, "element_icon", element);

                        //设置Boss模型
                        GameObject tmpBossModel = GameCommon.FindObject(item, "boss_model");
                        if (tmpBossModel != null)
                        {
                            ActiveBirthForUI tmpBirth = tmpBossModel.GetComponent<ActiveBirthForUI>();
                            __ShowModel(tmpBirth, bossID, true);
                        }
                        //GameCommon.SetUISprite(item, "boss_icon", (string)configRe["HEAD_ATLAS_NAME"], configRe["HEAD_SPRITE_NAME"]);

                        UISlider hpSlider = item.GetComponentInChildren<UISlider>();
                        if (hpSlider != null) {
                            hpSlider.value = ((float)nowHp / (float)maxHp);
                            GameCommon.SetUIVisiable(hpSlider.gameObject, "Foreground", Convert.ToBoolean(hpSlider.value));
                        }

                        //GameCommon.SetMonsterIcon(item, "boss_icon", bossID);
                        //GameCommon.SetUIText(item, "find_palyer_name", "FINDER: ID" + (string)bossData.get("FINDER"));

                        /*
                         NiceData damageUIData = GameCommon.GetButtonData(item, "damage_button");
                            if (damageUIData != null)
                            {
                                damageUIData.set("FINDER_DBID", finderDBID);
                                damageUIData.set("FINDER_NAME", finderName);
                                damageUIData.set("BOSS_DATA", bossData);
                            }
                            */

                        UIImageButton button = GameCommon.GetUIButton(item, "fight_button");
                        if (button != null)
                            button.gameObject.SetActive(nowHp > 0);
                        else
                            EventCenter.Log(LOG_LEVEL.ERROR, "Error: no exist > fight_button");

                        /*
                        UIImageButton awardButton = GameCommon.GetUIButton(item, "get_awards_button");
                        if (awardButton != null)
                            awardButton.gameObject.SetActive(nowHp<=0);
                        else
                            EventCenter.Log(LOG_LEVEL.ERROR, "Error: no exist > get_awards_button");
                            */

                        if (nowHp > 0) {
                            NiceData buttonData = GameCommon.GetButtonData(item, "fight_button");
                            if (buttonData != null)
                                buttonData.set("BOSS_DATA", bossData);
                        }
                        /*
                        else
                        {
                            GameCommon.SetUIText(item, "boss_escape_time", "");
                            NiceData buttonData = GameCommon.GetButtonData(item, "get_awards_button");
                            if (buttonData != null)
                                buttonData.set("BOSS_DATA", bossData);
                        }
                        */
                    }
                }
            }
            
            DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_RIGHT", "REFRESH", null);
        }
        else
		{
			//TODO
//			EventCenter.Log(true, "参数为空");
		}
    }

    private void __ShowModel(ActiveBirthForUI birthUI, int bossID, bool visible)
    {
        if (birthUI == null)
            return;

        if (birthUI.mActiveObject != null)
            birthUI.mActiveObject.OnDestroy();

        birthUI.mObjectType = (int)OBJECT_TYPE.BIG_BOSS;
        birthUI.mBirthConfigIndex = bossID;
        birthUI.Init();
        if (birthUI.mActiveObject != null)
        {
            //解决UI3DCamera.cs的CreateObject中的诡异操作，导致天魔列表中的天魔模型Y轴反向
            if(GameCommon.bIsLogicDataExist("SCROLL_WORLD_MAP_WINDOW") && (DataCenter.GetData("SCROLL_WORLD_MAP_WINDOW") as tWindow).IsOpen())
			{
                GameObject tmpActiveObj = birthUI.mActiveObject.mMainObject;
                Vector3 tmpPos = tmpActiveObj.transform.localPosition;
                tmpActiveObj.transform.localPosition = new Vector3(tmpPos.x, tmpPos.y - 80f, tmpPos.z);
                tmpActiveObj.transform.Rotate(new Vector3(0, -180f, 0));
			}

            birthUI.mActiveObject.SetScale(80f);
            birthUI.mActiveObject.PlayAnim("idle");

            BossBirthOnApearUI modelScript = birthUI.mActiveObject.mMainObject.AddComponent<BossBirthOnApearUI>();
            if (modelScript != null)
                modelScript.mActiveObject = birthUI.mActiveObject;
        }
    }

	public void BossRefresh(object obj)
	{
		UIGridContainer grid = GetSub ("grid").GetComponent<UIGridContainer>();
		grid.MaxCount = 0;
		grid.Reposition ();
//		SetVisible ("no_boss_label", true);
	}

	public static DateTime Get1970TimeFromServer(long time)
	{
		DateTime dateTime = new DateTime(time * TimeSpan.TicksPerSecond);
		dateTime = dateTime.Add(new TimeSpan(sm_1970.Ticks));
		return dateTime;
	}
    public static DateTime Get1970TimeFromServerMillisecond(long time)
    {
        DateTime dateTime = new DateTime(time * TimeSpan.TicksPerMillisecond);
        dateTime = dateTime.Add(new TimeSpan(sm_1970.Ticks));
        return dateTime;
    }
	private long __GetBossResetTime()
	{
		long nowServerTime = CommonParam.NowServerTime();
		DateTime nowDateTime = new DateTime(nowServerTime * TimeSpan.TicksPerSecond);
		nowDateTime = nowDateTime.Add(new TimeSpan(sm_1970.Ticks));
		DateTime dayBeginTime = new DateTime(nowServerTime * TimeSpan.TicksPerSecond);
		dayBeginTime = dayBeginTime.Add(new TimeSpan(sm_1970.Ticks));
		long nextSundayLeft = (7 - (int)nowDateTime.DayOfWeek) * 86400;
		dayBeginTime = dayBeginTime.AddSeconds(-(dayBeginTime.Hour * 3600 + dayBeginTime.Minute * 60 + dayBeginTime.Second));
		DateTime nextSundayDateTime = dayBeginTime.AddSeconds(nextSundayLeft);
		TimeSpan left = nextSundayDateTime - nowDateTime;
		return (nowServerTime + (long)left.TotalSeconds);
	}
    //public static long GetBossEscapeTime(long finderTime)
    //{
    //    //2小时逃脱时间
    //    return (finderTime + 7200);
    //}

	public static void InitBossBattle(/*Boss_GetDemonBossList_BossData bossData*/BossBattleStartData startData)
	{
        Boss_GetDemonBossList_BossData bossData = startData.bossData;
		DataRecord config = DataCenter.mBossConfig.GetRecord(bossData.tid);
		BossBattle battle = MainProcess.mStage as BossBattle;      
		battle.mBossID = bossData.tid;
		battle.mBossLevel = 1;
//		battle.mBossCode = respEvent.get("BOSS_CODE");
//		battle.mBossTime = respEvent.get("BOSS_TIME");
		battle.mBossHp = bossData.hpLeft;
		battle.mBossFinderDBID =bossData.finderId;
		battle.mBossFinderName = bossData.finderName;
        battle.mBattleDuration = startData.battleDuration;
        battle.mBossLeftHpAfterBattle = startData.leftHpAfterBattle;
        battle.mBossQuality = startData.bossData.quality;
        battle.mBossLevel = startData.bossData.bossLevel;
        battle.mIsPowerAttack = DataCenter.Self.get("IS_BOSS_POWER_ATTACK") > 0;
		battle.mIfShareWithFriend = bossData.ifShareWithFriend;
//		battle.mTotalResultDamage = (int)respEvent["TOTAL_DAMAGE"];
        //if (battle.mBoss != null)
        //    battle.mBoss.SetHp(battle.mBossHp);

        battle.mMaxDamageToBoss = BattleProver.GetMaxLimit(BATTLE_PROVE_TYPE.BOSS, GameCommon.GetTeamTotalAttack());
	}

    public static long GetCurrentMerit()
    {
        BossRaidWindow raidWindow = DataCenter.GetData("BOSS_RAID_WINDOW") as BossRaidWindow;
        if (raidWindow == null)
            return 0;

        long merit = (long)raidWindow.getObject("SELF_MERIT");
        return merit;
    }

    /// <summary>
    /// 是否有新的天魔出现
    /// </summary>
    public static bool HasNewBossAppear
    {
        set { msHasNewBossAppear = value; }
        get { return msHasNewBossAppear; }
    }
    /// <summary>
    /// 是否可以检查是否有天魔
    /// </summary>
    /// <returns></returns>
    public static bool CanCheckHasBoss()
    {
        return (msBossList != null);
    }
    /// <summary>
    /// 当前是否有天魔
    /// </summary>
    /// <returns></returns>
    public static bool HasBoss()
    {
        if (msBossList == null)
            return false;

        return (msBossList.arr.Length > 0);
    }
}

class Button_fight_button : CEvent
{
	public override bool _DoEvent()
	{
		Dump ();

        Boss_GetDemonBossList_BossData bossData = getObject("BOSS_DATA") as Boss_GetDemonBossList_BossData;
        if (bossData != null)
        {
            int bossID = bossData.tid;
            DataRecord config = DataCenter.mBossConfig.GetRecord(bossID);
            
            if (config == null)
            {
                Log("BOSS 配置不存在 >" + bossID.ToString());
                return true;
            }

            DataCenter.SetData("BOSS_STAGE_INFO_WINDOW", "BOSS_DATA", bossData);
//			DataCenter.SetData("BOSS_STAGE_INFO_WINDOW", "BOSS_FINDER_DBID", (int)get("FINDER_DBID"));
			DataCenter.SetData("BOSS_STAGE_INFO_WINDOW", "OPEN", (int)config.get("SCENE_ID"));
        }
		else
		{
			Log ("boss data is not set");
		}
		return true;
    }
}

class Button_damage_button : CEvent
{
    public override bool _DoEvent()
    {
        Dump();
//		DataCenter.SetData("SELECT_LEVEL_TEAM_INFO_WINDOW", "OPEN_AND_CLOSE_PLAYER", false);
        DataCenter.OpenWindow("BOSS_DAMAGE_WINDOW", getData());

		return true;
    }
}

/// <summary>
/// 刷新界面数据
/// </summary>
class Button_refresh_boss_list_button : CEvent
{
	public override bool _DoEvent ()
	{
		//刷新
		BossBattleNetManager.RequestGetDamageAndMerit();
		BossBattleNetManager.RequestGetDemonBossList();

		return true;
	}
}

/// <summary>
/// 排行榜界面按钮
/// </summary>
class Button_boss_list_rank_btn : CEvent
{
	public override bool _DoEvent ()
	{
		//打开排行榜界面
        DataCenter.OpenWindow("RANKLIST_BOSSBATTLE_WINDOW", null);
        
		return true;
	}
}

/// <summary>
/// 功勋奖励界面按钮
/// </summary>
class Button_exploit_btn : CEvent
{
	public override bool _DoEvent ()
	{
		//打开功勋奖励界面

        DataCenter.OpenWindow("BOSS_AWARD_WINDOW", null);

		return true;
	}
}

/// <summary>
/// 战功商店界面按钮
/// </summary>
class Button_feats_btn : CEvent
{
	public override bool _DoEvent ()
	{
		//TODO 打开战功商店按钮
		DataCenter.OpenWindow("FEATS_SHOP_WINDOW");
        DataCenter.CloseWindow("BOSS_RAID_WINDOW");
        DataCenter.OpenWindow("SHOP_FEATS_WINDOW_BACK");

		return true;
	}
}

//-------------------------------------------------------------------------
class Button_refresh_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("BOSS_RAID_WINDOW", "REQUEST_DATA", null);
		
		return true;
	}
}


//-------------------------------------------------------------------------

/// <summary>
/// 退出按钮
/// </summary>
class Button_boss_world_map_back : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("BOSS_RAID_WINDOW");

        return true;
    }
}
