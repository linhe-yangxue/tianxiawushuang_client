using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class ArenaRankWindow : ArenaBase
{
    ArenaWarrior myArena = null;
	public override void Init ()
	{
        EventCenter.Self.RegisterEvent("Button_arena_rank_close_button", new DefineFactoryLog<Button_arena_rank_close_button>());
        EventCenter.Self.RegisterEvent("Button_arena_rank_window_black_btn", new DefineFactoryLog<Button_arena_rank_close_button>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);

        //GameCommon.FindComponent<UIScrollView>(mGameObjUI, "rank_list_scrollView").ResetPosition();
        //ArenaNetManager.RequestArenaRankList();
	}

    public void InitUI(SC_ArenaRankList scRecord)
    {
        //自己的排名数据 down
        setMyRankInfo(scRecord.arenaPrincipal, scRecord.arenaRankList);

        //scrollview up
        setScroolView(scRecord.arenaRankList);
    }

    public void setScroolView(ArenaWarrior[] arenaWarriorArr)
    {
        ArenaWarrior[] recordArr = arenaWarriorArr;
        UIGridContainer grid = GetUIGridContainer("rank_list_grid");
        grid.MaxCount = recordArr.Length;
        var gridList = grid.controlList;
        for (int i = 0; i < recordArr.Length; i++)
        {
            if (recordArr[i] != null)
            {
                RefreshBoard(gridList[i], recordArr[i]);
            }
        }
    }

    public void setMyRankInfo(ArenaWarrior arenaWarrior, ArenaWarrior[] arenaWarriorArr)
    {
        //排名
        bool isIntheList = IsInTheList(arenaWarrior, arenaWarriorArr);
        SetVisible("arena_role_cur_label", !isIntheList);
        SetVisible("arena_role_rank_number_label", isIntheList);
        SetText("arena_role_rank_number_label", GetShowRank(arenaWarrior).ToString());

        //前多名能拿奖励
        string str = string.Format(TableCommon.getStringFromStringList(STRING_INDEX.ARENA_NEED_TOP_RANK), GetMaxRankGetRankAward());
        SetText("tips_rank_label", str);
        int targetRank = GetTargetRank(arenaWarrior, arenaWarriorArr);
        if (targetRank == 0)
        {
            SetText("aim_number", TableCommon.getStringFromStringList(STRING_INDEX.ARENA_RANK_LIMIT));
        }
        else
        {
            SetText("aim_number", GetTargetRank(arenaWarrior, arenaWarriorArr).ToString());
        }

        //排名奖励
        string[] awardArr = GetRewardByRank(GetShowRank(arenaWarrior));
        if (awardArr != null)
        {
            for (int i = 0; i < awardArr.Length; i++)
            {
                GameObject obj = GameCommon.FindObject(mGameObjUI, "reward_info(Clone)_" + i.ToString());
                string[] award = awardArr[i].Split('#');
                string spriteStr = "Sprite0" + (i + 1).ToString();
                string labelStr = "num_label0" + (i + 1).ToString();
                if (award.Length > 1)
                {
                    obj.SetActive(true);
                    GameCommon.SetOnlyItemIcon(obj, "coin_icon", int.Parse(award[0]));
                    GameCommon.SetUIText(obj, "number_label", award[1]);
                }
            }
        }
    }

    public void RefreshBoard(GameObject board, ArenaWarrior record)
    {
        //非真实玩家，查看不了 隐藏了- -
        string uid = GetUid(record);
        if (!IsTruePlayer(uid))
        {
            //GameCommon.FindObject(board, "arena_rank_check_team_button").SetActive(false);
        }

        AddButtonAction(GameCommon.FindObject(board, "arena_rank_check_team_button"), () =>
        {
            //查看阵容 
            GameCommon.VisitGameFriend(GetUid(record), GetName(record), UIWindowString.arena_rank_window);
        });

        //排名变化
        SetRankInfo(board, record);

        //玩家信息-包括按钮
        SetRoleInfo(board, record);

        //排名奖励
        SetRewardInfo(board, record);
    }

    public void SetRewardInfo(GameObject board, ArenaWarrior record)
    {
        string[] awardArr = GetRewardByRank(GetShowRank(record));
        for (int i = 0; i < 3; i++)
        {
            string spriteStr = "Sprite0" + (i + 1).ToString();
            GameCommon.FindObject(board, spriteStr).SetActive(false);
        }
        if (awardArr != null)
        {
            for (int i = 0; i < awardArr.Length; i++)
            {
                string[] award = awardArr[i].Split('#');
                string spriteStr = "Sprite0" + (i + 1).ToString();
                string labelStr = "num_label0" + (i + 1).ToString();
                if (award.Length > 1)
                {
                    GameCommon.FindObject(board, spriteStr).SetActive(true);
                    GameCommon.SetOnlyItemIcon(board, spriteStr, int.Parse(award[0]));
                    GameCommon.SetUIText(board, labelStr, award[1]);
                }
            }
        }
    }

    public void SetRankInfo(GameObject board, ArenaWarrior record)
    {
        //排名
        int rank = GetShowRank(record);
        if (rank == 1 || rank == 2 || rank == 3)
        {
            GameCommon.SetUIText(board, "arena_rank_num", "");
            GameCommon.FindObject(board, "0" + rank.ToString() + "_sprite").SetActive(true);
        }
        else if (rank == 0)
        {
            GameCommon.SetUIText(board, "arena_rank_num", "");
        }
        else
        {
            GameCommon.SetUIText(board, "arena_rank_num", GetShowRank(record).ToString());
        }
    }

    public void SetRoleInfo(GameObject board, ArenaWarrior record)
    {
        //亮的条
        GameCommon.SetUIVisiable(board, "rank_bg_select", GetUid(record) == CommonParam.mUId ? true : false);

        //主角头像
        GameCommon.SetOnlyItemIcon(board, "icon_role", int.Parse(GetTid(record)));
        Color color = GameCommon.GetNameColor(int.Parse(GetTid(record)));
        GameCommon.FindObject(board, "role_name_label").GetComponent<UILabel>().color = color;
   
        //name
        GameCommon.SetUIText(board, "role_name_label", GetName(record));

        //战斗力
        GameCommon.SetUIText(board, "fight_strength_number", record.power.ToString());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "ARENA_RANK_INITUI":
                if (objVal is SC_ArenaRankList)
                {
                    InitUI((SC_ArenaRankList)objVal);
                }
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

public class Button_arena_rank_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.arena_rank_window);
        return true;
    }
}

