using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class UnionSetPkAimWindow : UnionBase
{

    public bool isHuizhang = true;
	public override void Init ()
	{
        EventCenter.Self.RegisterEvent("Button_union_set_pk_aim_window_close_button", new DefineFactory<Button_union_set_pk_aim_window_close_button>());
        EventCenter.Self.RegisterEvent("Button_union_set_pk_aim_window_btn", new DefineFactory<Button_union_set_pk_aim_window_close_button>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);
        DataCenter.Set("UNIONPK_CURRENT_WINDOW", UIWindowString.union_set_pk_aim_window);
        initUI();
        refreshUI(GetSelectMid());
	}

    public void initUI()
    {
        GameCommon.FindComponent<UIScrollView>(mGameObjUI, "pk_level_list_scrollView").ResetPosition();
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

        //第几关
        int guan = i + 1;
        string strName = guan.InsertToString(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_NAME)) + name;
        GameCommon.SetUIText(board, "union_pk_level_name", strName);

        //icon and icon button
        int monsterId = TableCommon.GetNumberFromStageConfig(stageId, "HEADICON");
        if (monsterId == 0)
        {
            monsterId = 70014;
        }
        GameCommon.SetItemIconNew(board, "set_pk_item_icon_tips_rewards_btn", monsterId);

        //这是？？
        GameCommon.SetUIText(board, "times_label", "");
        GameCommon.FindObject(board, "times_label").SetActive(true);
        

        //buttons
        AddButtonAction(GameCommon.FindObject(board, "set_pk_item_icon_tips_rewards_btn"), () =>
        {
            DEBUG.Log("set_pk_item_icon_tips_rewards_btn---" + stageId);
            int temp = index;
            DataCenter.OpenWindow(UIWindowString.union_pk_drop_preview_window, temp);
        });
        int indexTemp = index;
        AddButtonAction(GameCommon.FindObject(board, "check_toggle_btn"), () =>
        {
            DEBUG.Log("check_toggle_btn---" + indexTemp);
            DataCenter.Set("CHECK_TOGGLE_BTN_INDEX", indexTemp);

            //公会BOSS的前置BOSS没有完成过击杀
            int exBossId = TableCommon.GetNumberFromConfig(index, "EX_BOSS_ID", DataCenter.mGuildBoss);
            if (exBossId != 0)
            {
                //判断前置boss是否被击杀
                if (!UnionBase.IsKilledExIndex(exBossId))
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.GUILD_BOSS_GUNAKA_EXT_NOT_KILLED);
                    refreshUI(GetSelectMid());
                    return;
                }
            }

            //设置相同的 不再请求
            if (indexTemp == GetSelectMid())
            {
                return;
            }

            //

            //设置boss
            GuildBossNetManager.RequestGuildBossInit(RoleLogicData.Self.guildId, indexTemp);
        });

        boardMainLogic(index, i, board);
    }

    public void refreshUI(int indexBTN)
    {
        Dictionary<int, DataRecord> recordDic = DataCenter.mGuildBoss.GetAllRecord();
        var pkLevelList = GetUIGridContainer("pk_level_list_grid");
        pkLevelList.MaxCount = recordDic.Count;

        var pkLevel = pkLevelList.controlList;
        int indexTmp = 6001;
        for (int i = 0; i < recordDic.Count; i++)
        {
            GameObject board = pkLevel[i];
            if (board != null)
            {
                if (indexTmp + i == indexBTN)
                {
                    GameCommon.FindObject(board, "checkmark_guanquan").SetActive(true);
                    GameCommon.FindObject(board, "check_toggle_btn").GetComponent<UIToggle>().value = true;
                }
                else
                {
                    GameCommon.FindObject(board, "checkmark_guanquan").SetActive(false);
                    GameCommon.FindObject(board, "check_toggle_btn").GetComponent<UIToggle>().value = false;
                }
            }
        }
    }

    public void boardMainLogic(int index, int i, GameObject board)
    {
        //奖励数量
        int contri = TableCommon.GetNumberFromConfig(index, "ATTACK_CONTRIBUTE", DataCenter.mGuildBoss);
        GameCommon.SetUIText(board, "rewards_num", contri.ToString());  //奖励数额

        //open or no open
        if (index == GetSelectMid())//iIndex
        {
            //check_toggle_btn-
            GameCommon.FindObject(board, "check_toggle_btn").SetActive(true);

            //no_chose_pk
            GameCommon.FindObject(board, "can_not_chose_infos").SetActive(false);
        }
        else
        {
            //no_chose_pk
            int openLevel = TableCommon.GetNumberFromConfig(index, "OPEN_GUILD_LEVEL", DataCenter.mGuildBoss);
            if (guildBaseObject.level >= openLevel)
            {
                //check_toggle_btn
                GameCommon.FindObject(board, "check_toggle_btn").SetActive((IsCanSetBoosIndex() && myTitle == UnionTitle.PRESIDENT) ? true : false);
                GameCommon.FindObject(board, "can_not_chose_infos").SetActive(false);
            }
            else
            {
                GameCommon.FindObject(board, "check_toggle_btn").SetActive(false);
                GameCommon.FindObject(board, "can_not_chose_infos").SetActive(true);
                string strOpenLevel = openLevel.InsertToString(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_NEED_LEVEL));
                GameCommon.SetUIText(board, "can_chose_premise", strOpenLevel);
            }
        }
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "SET_PK_REFRESHUI":
                int index = DataCenter.Get("CHECK_TOGGLE_BTN_INDEX");
                refreshUI(index);
                break;

            default:
                break;
        }
    }

	public override void Close ()
	{
		base.Close ();
		
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
/// pk
/// </summary>
public class Button_union_set_pk_aim_window_close_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        
        //else
        {
            DataCenter.CloseWindow(UIWindowString.union_set_pk_aim_window);
            DataCenter.OpenWindow(UIWindowString.union_pk_enter_window);
            DataCenter.SetData(UIWindowString.union_pk_enter_window, "UNION_PKENTER_REFRESH", null);
        }
        return true;
    }

}