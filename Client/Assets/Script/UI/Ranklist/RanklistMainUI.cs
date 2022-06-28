using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;

//主界面排行

/// <summary>
/// 主界面排行主界面
/// </summary>
public class RanklistMainUIWindow : RanklistMainUI
{
    public override void Open(object param)
    {
        base.Open(param);

        DataCenter.OpenWindow("RANKLIST_MAIN_UI_BACK_WINDOW");

        GameObject tmpBackWin = GameObject.Find("main_rank_window_back");
        if (tmpBackWin != null)
        {
            NiceData tmpCloseBtnData = GameCommon.GetButtonData(tmpBackWin, mCloseBtnName);
            if (tmpCloseBtnData != null)
                _SetCloseButtonData(tmpCloseBtnData);
        }
    }

    public override void OnClose()
    {
        DataCenter.CloseWindow("RANKLIST_MAIN_UI_BACK_WINDOW");
		MainUIScript.Self.ShowMainBGUI ();
        base.OnClose();
    }

    protected override string _GetCloseBtnName()
    {
        return "main_rank_back_button";
    }

    protected override string _GetWindowName()
    {
        return "RANKLIST_MAIN_UI_WINDOW";
    }
    protected override List<string> _GetTabBtnName()
    {
        List<string> listTabsBtnName = new List<string>() {
            "main_rank_title_group(Clone)_0",
            "main_rank_title_group(Clone)_1"
        };
        return listTabsBtnName;
    }
    protected override List<string> _GetSubWindowName()
    {
        List<string> listSubWin = new List<string>() {
            "RANKLIST_MAIN_UI_POWER_WINDOW",
            "RANKLIST_MAIN_UI_LEVEL_WINDOW"
        };
        return listSubWin;
    }
}

/// <summary>
/// 主界面战力排行
/// </summary>
public class RanklistMainUIPowerWindow : RanklistSubBuildInUI
{
    private SC_Ranklist_MainUIPowerRanklist mPowerList;

    public override void Open(object param)
    {
        base.Open(param);

        //TODO Test
        if (!RanklistNetManager.IsSend)
        {
            SC_Ranklist_MainUIPowerRanklist tmpRank = new SC_Ranklist_MainUIPowerRanklist();
            tmpRank.myRanking = 33;
            tmpRank.ranklist = new MainUIRanklist_ItemData[] {
                new MainUIRanklist_ItemData(){ nickname = "wwwdsfg234", power = 123, headIconId = 1, ranking = 1, vipLv = 123, level = 12, guildName = "GuildTest1547567" },
                new MainUIRanklist_ItemData(){ nickname = "wwwxcbv", power = 123, headIconId = 1, ranking = 2, vipLv = 123, level = 12, guildName = "GuildTest1ghfnjfg" },
                new MainUIRanklist_ItemData(){ nickname = "wwwdsfg", power = 123, headIconId = 1, ranking = 3, vipLv = 123, level = 12, guildName = "GuildTest15475478" },
                new MainUIRanklist_ItemData(){ nickname = "www3454", power = 123, headIconId = 1, ranking = 4, vipLv = 123, level = 12, guildName = "GuildTest1546756" },
                new MainUIRanklist_ItemData(){ nickname = "www2354", power = 123, headIconId = 1, ranking = 5, vipLv = 123, level = 12, guildName = "GuildTest1678gfhj" },
                new MainUIRanklist_ItemData(){ nickname = "wwwsdfgdsfg", power = 123, headIconId = 1, ranking = 6, vipLv = 123, level = 12, guildName = "GuildTest1dfxcvb" }
            };
            Refresh(tmpRank);
        }
    }

    public override bool Refresh(object param)
    {
        mPowerList = param as SC_Ranklist_MainUIPowerRanklist;

        base.Refresh(param);
        _ResetScrollViewPos("rank_list_scrollView");
        return true;
    }

    protected override string _GetWindowGameObjectName()
    {
        return "rank_role_group";
    }

    protected override string _OnGetWindowName()
    {
        return "RANKLIST_MAIN_UI_POWER_WINDOW";
    }

    protected override void _OnRequestRanklist()
    {
        GlobalModule.DoCoroutine(RanklistNetManager.RequestMainUIPowerRanklist());
    }

    protected override string _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE itemType)
    {
        string tmpName = "";
        switch (itemType)
        {
            case RANKLIST_ITEM_COMPONENT_TYPE.ITEM_GRID: tmpName = "rank_list_grid"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.NICKNAME: tmpName = "role_name_label"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.HEAD_ICON_ID: tmpName = "item_icon"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.RANKING: tmpName = "peak_rank_num"; break;
        }
        return tmpName;
    }

    protected override void _OnRefreshRanklistItem(GameObject goItem, IRanklistItemData itemData)
    {
        MainUIRanklist_ItemData tmpItemData = itemData as MainUIRanklist_ItemData;

        //排名
        GameObject tmpRankParent = GameCommon.FindObject(goItem, "peak_rank");
        for (int i = 0; i < 3; i++)
            GameCommon.SetUIVisiable(tmpRankParent, "no" + (i + 1).ToString(), tmpItemData.Ranking == (i + 1));
        GameCommon.SetUIVisiable(tmpRankParent, "peak_rank_num", tmpItemData.Ranking != 1 && tmpItemData.Ranking != 2 && tmpItemData.Ranking != 3);

        //等级
        GameCommon.SetUIVisiable(goItem, "fighting_label_level", false);
        GameCommon.SetUIVisiable(goItem, "level_label_fighting", true);
        GameCommon.SetUIText(goItem, "level_label_fighting_num", tmpItemData.level.ToString());

        //公会
        GameCommon.SetUIText(goItem, "union_label_name", tmpItemData.guildName);

        //战力
        GameCommon.SetUIVisiable(goItem, "level_label_level", false);
        GameCommon.SetUIVisiable(goItem, "fighting_label_fighting", true);
        GameCommon.SetUIText(goItem, "fighting_label_fighting_num", GameCommon.ShowNumUI(tmpItemData.Power));
    }

    protected override void _OnRefreshMyRanking()
    {
        bool tmpIsMyInRanking = (mPowerList != null) ? (mPowerList.myRanking > 0) : false;

        GameObject tmpBottomParent = GameCommon.FindObject(mGameObjUI.transform.parent.gameObject, "bottom_info_group");

        //自己排行
        GameCommon.SetUIVisiable(tmpBottomParent, "main_rank_cur_label", !tmpIsMyInRanking);
        GameCommon.SetUIVisiable(tmpBottomParent, "main_rank_number_label", tmpIsMyInRanking);
        if (tmpIsMyInRanking)
            GameCommon.SetUIText(tmpBottomParent, "main_rank_number_label", mPowerList.myRanking.ToString());

        //战力
        GameCommon.SetUIVisiable(tmpBottomParent, "level_label", false);
        GameCommon.SetUIVisiable(tmpBottomParent, "fighting_label", true);
        //int tmpMyPower = (int)GameCommon.GetPower();     //TODO
		GameCommon.SetUIText(tmpBottomParent, "fighting_label_num", GameCommon.ShowNumUI(System.Convert.ToInt32(GameCommon.GetPower().ToString("f0"))));
    }

    protected override IRanklistItemData[] _OnGetRanklist()
    {
        if (mPowerList == null)
            return null;
        return mPowerList.ranklist;
    }
}

/// <summary>
/// 主界面等级排行
/// </summary>
public class RanklistMainUILevelWindow : RanklistSubBuildInUI
{
    private SC_Ranklist_MainUILevelRanklist mLevelList;

    public override void Open(object param)
    {
        base.Open(param);

        //TODO Test
        if (!RanklistNetManager.IsSend)
        {
            SC_Ranklist_MainUILevelRanklist tmpRank = new SC_Ranklist_MainUILevelRanklist();
            tmpRank.myRanking = 23;
            tmpRank.ranklist = new MainUIRanklist_ItemData[] {
                new MainUIRanklist_ItemData(){ nickname = "www1", power = 123, headIconId = 1, ranking = 1, vipLv = 123, level = 12, guildName = "GuildTest1" },
                new MainUIRanklist_ItemData(){ nickname = "www1234", power = 123, headIconId = 1, ranking = 2, vipLv = 123, level = 12, guildName = "GuildTest1dsf" },
                new MainUIRanklist_ItemData(){ nickname = "www1dfsg", power = 123, headIconId = 1, ranking = 3, vipLv = 123, level = 12, guildName = "GuildTest1345" },
                new MainUIRanklist_ItemData(){ nickname = "www123443", power = 123, headIconId = 1, ranking = 4, vipLv = 123, level = 12, guildName = "GuildTest1ghfj" },
                new MainUIRanklist_ItemData(){ nickname = "www1534", power = 123, headIconId = 1, ranking = 5, vipLv = 123, level = 12, guildName = "GuildTest16789" },
                new MainUIRanklist_ItemData(){ nickname = "www12134", power = 123, headIconId = 1, ranking = 6, vipLv = 123, level = 12, guildName = "GuildTest1gfhj" }
            };
            Refresh(tmpRank);
        }
        UIScrollView _rankListView = GameCommon.FindComponent<UIScrollView>(mGameObjUI, "rank_list_scrollView");
        if (_rankListView != null)
            _rankListView.ResetPosition();
    }

    public override bool Refresh(object param)
    {
        mLevelList = param as SC_Ranklist_MainUILevelRanklist;

        base.Refresh(param);
        _ResetScrollViewPos("rank_list_scrollView");
        return true;
    }

    protected override string _GetWindowGameObjectName()
    {
        return "rank_role_group";
    }

    protected override string _OnGetWindowName()
    {
        return "RANKLIST_MAIN_UI_LEVEL_WINDOW";
    }

    protected override void _OnRequestRanklist()
    {
        GlobalModule.DoCoroutine(RanklistNetManager.RequestMainUILevelRanklist());
    }

    protected override string _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE itemType)
    {
        string tmpName = "";
        switch (itemType)
        {
            case RANKLIST_ITEM_COMPONENT_TYPE.ITEM_GRID: tmpName = "rank_list_grid"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.NICKNAME: tmpName = "role_name_label"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.HEAD_ICON_ID: tmpName = "item_icon"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.RANKING: tmpName = "peak_rank_num"; break;
        }
        return tmpName;
    }

    protected override void _OnRefreshRanklistItem(GameObject goItem, IRanklistItemData itemData)
    {
        MainUIRanklist_ItemData tmpItemData = itemData as MainUIRanklist_ItemData;

        //排名
        GameObject tmpRankParent = GameCommon.FindObject(goItem, "peak_rank");
        for (int i = 0; i < 3; i++)
            GameCommon.SetUIVisiable(tmpRankParent, "no" + (i + 1).ToString(), tmpItemData.Ranking == (i + 1));
        GameCommon.SetUIVisiable(tmpRankParent, "peak_rank_num", tmpItemData.Ranking != 1 && tmpItemData.Ranking != 2 && tmpItemData.Ranking != 3);

        //战力
        GameCommon.SetUIVisiable(goItem, "level_label_fighting", false);
        GameCommon.SetUIVisiable(goItem, "fighting_label_level", true);
        GameCommon.SetUIText(goItem, "fighting_label_level_num", tmpItemData.Power.ToString());

        //公会
        GameCommon.SetUIText(goItem, "union_label_name", tmpItemData.guildName);

        //等级
        GameCommon.SetUIVisiable(goItem, "fighting_label_fighting", false);
        GameCommon.SetUIVisiable(goItem, "level_label_level", true);
        GameCommon.SetUIText(goItem, "level_label_level_num", tmpItemData.level.ToString());
    }

    protected override void _OnRefreshMyRanking()
    {
        bool tmpIsMyInRanking = (mLevelList != null) ? (mLevelList.myRanking > 0) : false;

        GameObject tmpBottomParent = GameCommon.FindObject(mGameObjUI.transform.parent.gameObject, "bottom_info_group");

        //自己排行
        GameCommon.SetUIVisiable(tmpBottomParent, "main_rank_cur_label", !tmpIsMyInRanking);
        GameCommon.SetUIVisiable(tmpBottomParent, "main_rank_number_label", tmpIsMyInRanking);
        if (tmpIsMyInRanking)
            GameCommon.SetUIText(tmpBottomParent, "main_rank_number_label", mLevelList.myRanking.ToString());

        //等级
        GameCommon.SetUIVisiable(tmpBottomParent, "fighting_label", false);
        GameCommon.SetUIVisiable(tmpBottomParent, "level_label", true);
        int tmpMyLevel = RoleLogicData.Self.character.level;
        GameCommon.SetUIText(tmpBottomParent, "level_label_num", tmpMyLevel.ToString());
    }

    protected override IRanklistItemData[] _OnGetRanklist()
    {
        if (mLevelList == null)
            return null;
        return mLevelList.ranklist;
    }
}
