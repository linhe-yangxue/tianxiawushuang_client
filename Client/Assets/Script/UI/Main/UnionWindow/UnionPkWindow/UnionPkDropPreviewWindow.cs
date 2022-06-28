using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class UnionPkDropPreviewWindow : tWindow
{
	public int iIndex = 0;
	public override void Init ()
	{
        
	}
	
	public override void Open (object param)
	{
		base.Open (param);
        EventCenter.Self.RegisterEvent("Button_union_pk_drop_preview_window_close_button", new DefineFactory<Button_union_pk_drop_preview_window_close_button>());

        //scrollview resetpos
        GameCommon.FindComponent<UIScrollView>(mGameObjUI, "drop_preview_scroll_view").ResetPosition();

        if (param is int)
        {
            iIndex = (int)param;
            int deadGroupId = TableCommon.GetNumberFromConfig(iIndex, "DEAD_GROUP_ID", DataCenter.mGuildBoss);
            initUI(deadGroupId);
        }
	}

    public void initUI(int groupId)
    {
        //title_label
        int guan = iIndex - 6000;
        string strTitle = guan.InsertToString(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_REWARD));
        SetText("title_label", strTitle);

        //scroll view
        List<ItemDataBase> itemDataBaseList = GameCommon.GetItemGroup(groupId, true);
        var dropGridList = GetUIGridContainer("drop_preview_grid");
        dropGridList.MaxCount = itemDataBaseList.Count;//recordDic.Count;

        var dropGrid = dropGridList.controlList;
        int index = 0;
        foreach (ItemDataBase item in itemDataBaseList)
        {
            GameObject board = dropGrid[index];
            if (board != null)
            {
                refreshBoard(item, board);
                index++;
            }
        }
    }

    public void refreshBoard(ItemDataBase item, GameObject board)
    {
        GameCommon.SetItemIconNew(board, "item_icon", item.tid);
		AddButtonAction (GameCommon.FindObject(board, "item_icon"), () => GameCommon.SetAccountItemDetailsWindow (item.tid));
        //GameCommon.FindObject(board, "item_icon_info"")
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
public class Button_union_pk_drop_preview_window_close_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        DataCenter.CloseWindow(UIWindowString.union_pk_drop_preview_window);
        return true;
    }
}
