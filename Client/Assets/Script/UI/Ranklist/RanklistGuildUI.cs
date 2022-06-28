using UnityEngine;
using System.Collections;
using DataTable;
using Logic;

//宗门界面
public class Button_union_rank_window_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RANKLIST_GUILD_WINDOW");
        return true;
    }
}
public class RanklistGuildWindow : RanklistSubUI
{
    private SC_Ranklist_GuildRanklist mGuildRanklist;
    protected override void OpenInit()
    {
        base.OpenInit();

        EventCenter.Self.RegisterEvent("Button_union_rank_window_btn", new DefineFactory<Button_union_rank_window_btn>());
    }
    public override void Open(object param)
    {
        base.Open(param);

        //TODO Test
        if (!RanklistNetManager.IsSend)
        {
            SC_Ranklist_GuildRanklist tmpRank = new SC_Ranklist_GuildRanklist();
            tmpRank.myRanking = 10;
            tmpRank.ranklist = new GuildRanklist_ItemData[] {
                new GuildRanklist_ItemData(){ guildName = "GuildTest123", hierarchName = "www", currMemberCount = 12, ranking = 1, guildLv = 2 },
                new GuildRanklist_ItemData(){ guildName = "GuildTest1233", hierarchName = "www1", currMemberCount = 10, ranking = 2, guildLv = 1 }
            };
            Refresh(tmpRank);
        }
    }

    public override bool Refresh(object param)
    {
        mGuildRanklist = param as SC_Ranklist_GuildRanklist;

        return base.Refresh(param);
    }

    protected override string _OnGetWindowName()
    {
        return "RANKLIST_GUILD_WINDOW";
    }

    protected override string _GetCloseBtnName()
    {
        return "guild_rank_list_close_button";
    }

    protected override void _OnRequestRanklist()
    {
        GlobalModule.DoCoroutine(RanklistNetManager.RequestGuildRanklist());
    }

    protected override string _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE itemType)
    {
        string tmpName = "";
        switch (itemType)
        {
            case RANKLIST_ITEM_COMPONENT_TYPE.ITEM_GRID: tmpName = "rank_list_grid"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.RANKING: tmpName = "union_rank_num"; break;
        }
        return tmpName;
    }

    protected override void _OnRefreshRanklistItem(GameObject goItem, IRanklistItemData itemData)
    {
        GuildRanklist_ItemData tmpItemData = itemData as GuildRanklist_ItemData;

        //排名图标
        GameObject tmpGORank = GameCommon.FindObject(goItem, "union_rank");
        for (int i = 0; i < 3; i++)
            GameCommon.SetUIVisiable(tmpGORank, "no" + (i + 1).ToString(), tmpItemData.Ranking == (i + 1));
        GameCommon.SetUIVisiable(tmpGORank, "union_rank_num", tmpItemData.Ranking != 1 && tmpItemData.Ranking != 2 && tmpItemData.Ranking != 3);

        DataRecord tmpGuildConfig = DataCenter.mGuildCreated.GetRecord(tmpItemData.guildLv);

        //宗门名称
        GameCommon.SetUIText(goItem, "union_name_label", tmpItemData.guildName);
        //掌门
        GameCommon.SetUIText(goItem, "union_creator_name_label", tmpItemData.hierarchName);
        //成员数量
        GameCommon.SetUIText(goItem, "union_member_num_label", tmpItemData.currMemberCount.ToString() + "/" + (tmpGuildConfig != null ? tmpGuildConfig.getData("NUMBRE_LIMIT") : "0"));
        //等级
        GameCommon.SetUIText(goItem, "union_level_label", tmpItemData.guildLv.ToString());
    }

    protected override void _OnRefreshMyRanking()
    {
        //排名
        GameCommon.SetUIVisiable(mGameObjUI, "union_rank_number_label", mGuildRanklist.myRanking > 0);
        GameCommon.SetUIText(mGameObjUI, "union_rank_number_label", mGuildRanklist.myRanking.ToString());
        GameCommon.SetUIVisiable(mGameObjUI, "union_cur_label", mGuildRanklist.myRanking <= 0);
    }

    protected override IRanklistItemData[] _OnGetRanklist()
    {
        if (mGuildRanklist == null)
            return null;
        return mGuildRanklist.ranklist;
    }
}
