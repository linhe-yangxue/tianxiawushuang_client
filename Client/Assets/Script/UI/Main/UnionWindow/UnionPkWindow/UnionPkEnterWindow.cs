using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class UnionPkEnterWindow : UnionBase
{
	public override void Init ()
	{
        EventCenter.Self.RegisterEvent("Button_union_pk_enter_window_close_button", new DefineFactory<Button_union_pk_enter_window_close_button>());
        EventCenter.Self.RegisterEvent("Button_go_rule_button", new DefineFactory<Button_go_rule_button>());
        EventCenter.Self.RegisterEvent("Button_chose_pk_level_button", new DefineFactory<Button_chose_pk_level_button>());
        EventCenter.Self.RegisterEvent("Button_union_pk_enter_window_btn", new DefineFactory<Button_union_pk_enter_window_close_button>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);
        DataCenter.OpenWindow(UIWindowString.union_pk_back_window);
        DataCenter.OpenWindow("INFO_GROUP_WINDOW");
        DataCenter.Set("UNIONPK_CURRENT_WINDOW", UIWindowString.union_pk_enter_window);
        //DataCenter.CloseWindow("INFO_GROUP_WINDOW");
        //GameCommon.FindComponent<UIScrollView>(mGameObjUI, "pk_level_list_scrollView").ResetPosition();
	}

    public void initUI()
    {
        Dictionary<int, DataRecord> recordDic = DataCenter.mGuildBoss.GetAllRecord();
        var pkLevelList = GetUIGridContainer("pk_level_list_grid");
        pkLevelList.MaxCount = recordDic.Count;

        var pkLevel = pkLevelList.controlList;
        int index = 6001;
        for (int i = 0; i < recordDic.Count; i++) 
        {
            GameObject board = pkLevel[i];
            int indexTemp = index + i;
            refreshBoard(indexTemp, i, board);
        }
    }

    public void refreshBoard(int index, int i, GameObject board)
    {
        //获取表数据
        string name = TableCommon.GetStringFromConfig(index, "NAME", DataCenter.mGuildBoss);
        int stageId = TableCommon.GetNumberFromConfig(index, "STAGE_ID", DataCenter.mGuildBoss);
        int exBossId = TableCommon.GetNumberFromConfig(index, "EX_BOSS_ID", DataCenter.mGuildBoss);
        string exName = TableCommon.GetStringFromConfig(exBossId, "NAME", DataCenter.mGuildBoss);

        //第几关-前置关卡boss名字
        int guan = i + 1;
        string strName = guan.InsertToString(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_NAME)) + name;
        GameCommon.SetUIText(board, "union_pk_level_name", strName);
        if(i > 0)
        {
            string strExName = i.InsertToString(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_NEED_CROSS_LEVEL) + exName);
            GameCommon.SetUIText(board, "no_chose_label", strExName);
        }

        //icon and icon button
        int monsterId = TableCommon.GetNumberFromStageConfig(stageId, "HEADICON");
        GameCommon.SetItemIconNew(board, "item_icon_tips_rewards_btn", monsterId);

        //item_icon_tips_rewards_btn
        AddButtonAction(GameCommon.FindObject(board, "item_icon_tips_rewards_btn"), () =>
        {
            DEBUG.Log("item_icon_tips_rewards_btn---" + stageId);
            DataCenter.OpenWindow(UIWindowString.union_pk_drop_preview_window, index);
        });

        //goto_prepare_button
        AddButtonAction(GameCommon.FindObject(board, "goto_prepare_button"), () =>
        {
            DataCenter.CloseWindow(UIWindowString.union_pk_enter_window);
            DataCenter.OpenWindow(UIWindowString.union_pk_prepare_window);
            DEBUG.Log("goto_prepare_button---" + stageId);
        });

        boardMainLogic(index, i, board);
    }

    public void boardMainLogic(int index, int i, GameObject board)
    {
        //open or no open
        if (index == guildBossObject.mid)//iIndex
        {
            //pk_open_infos
            GameCommon.FindObject(board, "pk_open_infos").SetActive(true);

            //是否已打死
            GameCommon.FindObject(board, "success_kill_sprite").SetActive(guildBossObject.monsterHealth <= 0 ? true: false);

            //奖励数量
            int contri = TableCommon.GetNumberFromConfig(index, "ATTACK_CONTRIBUTE", DataCenter.mGuildBoss);
            GameCommon.SetUIText(board, "rewards_num", contri.ToString());  //奖励数额

            //红点标志位 boss未被击杀 并且剩余挑战次数>0
            GameCommon.FindObject(board, "new_mark").SetActive((guildBossObject.monsterHealth > 0 && guildBossWarriorObject.leftBattleTimes > 0 && !IsYiJingGuoqi() && IsCanAttackBoss()) ? true : false); //奖励标志

            //progress bar
            SetProgressBar(index, board, "pk_progress_rate");

            //no_chose_pk
            GameCommon.FindObject(board, "no_chose_pk").SetActive(false);

            //buttons
            GameCommon.FindObject(board, "buttons").SetActive(true);

            //goto_prepare_button 前往战斗按钮
            ShowGotoPrepare(board);
        }
        else
        {
            //pk_open_infos
            GameCommon.FindObject(board, "pk_open_infos").SetActive(false);

            //no_chose_pk
            GameCommon.FindObject(board, "no_chose_pk").SetActive(true);
            int openLevel = TableCommon.GetNumberFromConfig(index, "OPEN_GUILD_LEVEL", DataCenter.mGuildBoss);
            int exBoosId = TableCommon.GetNumberFromConfig(index, "EX_BOSS_ID", DataCenter.mGuildBoss);
            if (guildBaseObject.level >= openLevel )
            {
                if (exBoosId == 0 || IsKilledExIndex(exBoosId))
                {
                    GameCommon.FindObject(board, "can_chose_premise").SetActive(false);
                    GameCommon.FindObject(board, "no_chose_tips").SetActive(true);
                    GameCommon.SetUIText(board, "no_chose_tips", "未选择");
                }
                else
                {
                    GameCommon.FindObject(board, "can_chose_premise").SetActive(false);
                    GameCommon.FindObject(board, "no_chose_tips").SetActive(true);
                    GameCommon.SetUIText(board, "no_chose_tips", "");
                }
            }
            else
            {
                string strOpenLevel = openLevel.InsertToString(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_NEED_LEVEL));
                GameCommon.SetUIText(board, "can_chose_premise", strOpenLevel);
                GameCommon.FindObject(board, "can_chose_premise").SetActive(true);
                GameCommon.FindObject(board, "no_chose_tips").SetActive(false);
            }

            //button
            GameCommon.FindObject(board, "goto_prepare_button").SetActive(false);
        }
    }

    public void ShowGotoPrepare(GameObject board)
    {
        //goto_prepare_button obj
        GameObject preBtnObj = GameCommon.FindObject(board, "goto_prepare_button");
        if (guildBossObject.monsterHealth > 0)
        {
            GameCommon.FindObject(board, "goto_prepare_button").SetActive(IsShiJianWeiDao() ? false : true);

            //new mark boss未死 并且还有剩余此数 时间未到
            GameCommon.FindObject(preBtnObj, "new_mark").SetActive(IsShiJianWeiDao() ? false : true);
            GameCommon.FindObject(preBtnObj, "new_mark").SetActive(IsHasRemainedTimes() ? true : false);

            //是否过期
            GameCommon.FindObject(board, "yijingguoqi").SetActive(IsYiJingGuoqi() ? true : false);
            GameCommon.FindObject(board, "shijianweidao").SetActive(IsShiJianWeiDao() ? true : false);

            //是否领取过奖励--
            GameCommon.FindObject(board, "get_rewards_label").SetActive(false);
            GameCommon.FindObject(board, "have_get_rewards_label").SetActive(false);
        }
        else
        {
            //是否过期 时间未到
            GameCommon.FindObject(board, "goto_prepare_button").SetActive(true);
            GameCommon.FindObject(board, "yijingguoqi").SetActive(IsYiJingGuoqi() ? true : false);
            GameCommon.FindObject(board, "shijianweidao").SetActive(false);

            //是否领取过奖励--
            GameCommon.FindObject(board, "get_rewards_label").SetActive(IsGettedReward() ? false : true);
            GameCommon.FindObject(board, "have_get_rewards_label").SetActive(IsGettedReward() ? true : false);

            //new mark 是否领取了奖励
            GameCommon.FindObject(preBtnObj, "new_mark").SetActive(IsGettedReward() ? false : true);
        }
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "UNION_PKENTER_FIRSTIN":
                {
                    GuildBossNetManager.RequestGuildBossInfoSimple(RoleLogicData.Self.guildId);
                }
                break;
            case "UNION_PKENTER_INIT":
                if (objVal is SC_GuildBossInfoSimple)
                {
                    SC_GuildBossInfoSimple scGuildBossInfoSimple = (SC_GuildBossInfoSimple)objVal;
                    getGuildBossSimpleData(scGuildBossInfoSimple);
                    initUI();
                }
                break;
            case "UNION_PKENTER_REFRESH":
                {
                    initUI();
                }
                break;

            default:
                break;
        }
    }

    public void getGuildBossSimpleData(SC_GuildBossInfoSimple simpleInfo)
    {
        GuildBossSimple simple = simpleInfo.guildBossSimple;
        GuildBoss boss = new GuildBoss();
        boss.gid = simple.gid;
        boss.mid = simple.mid;
        boss.nextSetTime = simple.nextSetTime;
        boss.monsterHealth = simple.monsterHealth;
        boss.criticalStrikeUID = simple.criticalStrikeUID;
        boss.criticalStrikeTime = simple.criticalStrikeTime;
        boss.killedGuildBossIndex = simple.killedGuildBossIndex;
        boss.nextMid = simple.nextMid;

        //boss
        guildBossObject = boss;
        guildBossWarriorObject = simpleInfo.guildBossWarrior;
    }

	public override void Close ()
	{
		base.Close ();
//		DataCenter.CloseWindow ("BACK_GROUP_UNION_WINDOW");
		
	}

	public static void CloseAllWindow()
	{
	}
	
	public override bool Refresh(object param)
	{
		base.Refresh (param);
		return true;
	}
}

/// <summary>
/// close
/// </summary>
public class Button_union_pk_enter_window_close_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        DataCenter.CloseWindow(UIWindowString.union_pk_back_window);
        DataCenter.CloseWindow(UIWindowString.union_pk_enter_window);
        DataCenter.OpenWindow(UIWindowString.union_main);
        DataCenter.OpenWindow("INFO_GROUP_WINDOW");
        return true;
    }
}

/// <summary>
/// rule
/// </summary>
public class Button_go_rule_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        DataCenter.OpenWindow("RULE_TIPS_WINDOW", HELP_INDEX.HELP_UNION_BOSS);
        return true;
    }
}

/// <summary>
/// pk
/// </summary>
public class Button_chose_pk_level_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        DataCenter.CloseWindow(UIWindowString.union_pk_enter_window);
        DataCenter.OpenWindow(UIWindowString.union_set_pk_aim_window);
        return true;
    }
}
