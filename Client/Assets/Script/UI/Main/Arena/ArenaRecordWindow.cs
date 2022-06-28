using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class ArenaRecordWindow : ArenaBase
{
	
	public override void Init ()
	{
        EventCenter.Self.RegisterEvent("Button_arena_record_window_close_button", new DefineFactoryLog<Button_arena_record_window_close_button>());
        EventCenter.Self.RegisterEvent("Button_arena_record_window_black_btn", new DefineFactoryLog<Button_arena_record_window_close_button>());
	
    }
	
	public override void Open (object param)
	{
		base.Open (param);

        ArenaNetManager.RequestArenaBattleRecord();
	}

    public void InitUI(SC_ArenaBattleRecord scRecord)
    {
        ArenaBattleRecord[] recordArr = scRecord.arenaBattleRecord;
        UIGridContainer grid = GetUIGridContainer("arena_record_list_grid");
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

    public void RefreshBoard(GameObject board, ArenaBattleRecord record)
    {
        AddButtonAction(GameCommon.FindObject(board, "record_team_btn"), () =>
        {
            //查看阵容 
            GameCommon.VisitGameFriend(record.rivalUid, record.name, UIWindowString.arena_record_window);
        });

        //排名变化
        setRankInfo(board, record);

        //玩家信息
        setRoleInfo(board, record);
    }

    public void setRankInfo(GameObject board, ArenaBattleRecord record)
    {
        if (record.rankChange >= 0)
        {
            GameCommon.SetUIVisiable(board, "win_sprite", true);
            GameCommon.SetUIVisiable(board, "fail_sprite", false);
            GameCommon.SetUIVisiable(board, "down_sprite", false);
        }
        else
        {
            int change = Math.Abs(record.rankChange);
            GameCommon.SetUIVisiable(board, "win_sprite", false);
            GameCommon.SetUIVisiable(board, "fail_sprite", true);
            GameCommon.SetUIVisiable(board, "down_sprite", true);
            GameCommon.SetUIText(board, "down_label", change.ToString());
        }
    }

        //玩家信息
    public void setRoleInfo(GameObject board, ArenaBattleRecord record)
    {
        //name
        GameCommon.SetUIText(board, "role_name_label", record.name.ToString());
        Color color = GameCommon.GetNameColor(record.tid);
        GameCommon.FindObject(board, "role_name_label").GetComponent<UILabel>().color = color;

        //lelve
        GameCommon.SetUIText(board, "role_level", record.level.ToString());

        //剩余时间
        GameCommon.SetUIText(board, "left_time", GameCommon.GetStringByOffTime(record.attackTime)); 

        //icon
        GameCommon.SetItemIconNew(board, "player_icon", record.tid);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "ARENA_RECORD_INITUI":
                if (objVal is SC_ArenaBattleRecord)
                {
                    InitUI((SC_ArenaBattleRecord)objVal);
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

public class Button_arena_record_window_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.arena_record_window);
        return true;
    }
}

