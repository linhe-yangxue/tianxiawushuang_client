using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class UnionPkRankWindow : UnionBase
{
	
	public override void Init ()
	{
        EventCenter.Self.RegisterEvent("Button_union_pk_rank_window_close_button", new DefineFactory<Button_union_pk_rank_window_close_button>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);

        GameCommon.FindComponent<UIScrollView>(mGameObjUI, "rank_list_scrollView").ResetPosition();
        initUI();
	}

    public GuildBossWarrior getMyWarrior(GuildBossWarrior[] warriorArr)
    {
        for(int i = 0; i < warriorArr.Length; i++)
        {
            if (CommonParam.mUId == warriorArr[i].uid.ToString())
            {
                GuildBossWarrior warrior = warriorArr[i];
                warrior.rank = i + 1;
                return warrior;
            }
        }
        return null;
    }

    public void initUI()
    {
        //玩家数据
        GuildBossWarrior[] warriorArr = guildBossObject.warriors;

        //set my rank
        GuildBossWarrior myWarrior = getMyWarrior(warriorArr);
        if (myWarrior != null)
        {
            //rank-left
            if (myWarrior.rank != 0)
            {
                SetText("cur_rank_rank", myWarrior.rank.ToString());
                SetVisible("no_rank_label", false);
            }

            //right
            GameCommon.SetUIText(mGameObjUI, "today_attack_times_down", myWarrior.totalChallengeTimes.ToString());
            GameCommon.SetUIText(mGameObjUI, "max_attack_damage_down", myWarrior.maxDamage.ToString());

            //blood
            SetText("cur_hurt_number", myWarrior.damage.ToString());
        }
        else
        {
            //没有排名
            SetVisible("cur_rank_label", false);
            SetVisible("no_rank_label", true);
            GameCommon.SetUIText(mGameObjUI, "today_attack_times_down", 0.ToString());
            GameCommon.SetUIText(mGameObjUI, "max_attack_damage_down", 0.ToString());
        }

        //scroll view
        var rankGridList = GetUIGridContainer("rank_list_grid");
        rankGridList.MaxCount = warriorArr.Length;

        var rankGrid = rankGridList.controlList;
        for (int i = 0; i < warriorArr.Length; i++)
        {
            GameObject board = rankGrid[i];
            if (warriorArr[i] != null)
                refreshBoard(board, warriorArr[i], i + 1);
        }
    }

    public void refreshBoard(GameObject board, GuildBossWarrior warrior, int rank)
    {
        //rank
        if (rank == 1 || rank == 2 || rank == 3)
        {
            string rankSprite = rank.InsertToString("0{0}_sprite");
            GameCommon.FindObject(board, rankSprite).SetActive(true);
            GameCommon.SetUIText(board, "cur_rank_number", "");
        }
        else
        {
            GameCommon.SetUIText(board, "cur_rank_number", rank.ToString());
        }

        //player_icon
        GameCommon.SetItemIconNew(board, "player_icon", warrior.tid);

        //get pet data petinfo
        int[] pets = warrior.pets;
        for (int i = 0; i < 3; i++)
        {
            string boardName = "pet_info(Clone)_" + i.ToString();
            GameObject boardPet = GameCommon.FindObject(board, boardName);
            if (pets.Length > i)
            {
                int tid = pets[i];
                GameCommon.SetItemIconNew(boardPet, "item_icon", tid);
                boardPet.SetActive(true);
            }
            else
            {
                boardPet.SetActive(false);
            }
        }

        //name
        GameCommon.SetUIText(board, "role_name_label", warrior.name);
        GameCommon.FindObject(board, "role_name_label").GetComponent<UILabel>().color = GameCommon.GetNameColor(warrior.tid);

        //damage
        GameCommon.SetUIText(board, "today_attack_times", warrior.totalChallengeTimes.ToString());
        GameCommon.SetUIText(board, "max_attack_damage", warrior.maxDamage.ToString());

        //vip level
        GameCommon.SetUIText(board, "vip_num", "VIP " + warrior.vipLevel);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "update":
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
public class Button_union_pk_rank_window_close_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        DataCenter.CloseWindow(UIWindowString.union_pk_rank_window);
        return true;
    }
}
