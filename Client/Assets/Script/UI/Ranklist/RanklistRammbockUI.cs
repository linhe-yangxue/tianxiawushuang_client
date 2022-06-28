using UnityEngine;
using System.Collections;
using System;
using Logic;

//群魔排行

public class RanklistRammbockWindow : RanklistSubUI
{
    private SC_Ranklist_GetClimbTowerStarsRank mRammbockRanklist;

    public override void Open(object param)
    {
        base.Open(param);

        GameCommon.SetUIVisiable(mGameObjUI, "reset_bg", false);

        //TODO Test
        if (!RanklistNetManager.IsSend)
        {
            SC_Ranklist_GetClimbTowerStarsRank tmpRank = new SC_Ranklist_GetClimbTowerStarsRank();
            tmpRank.myRanking = 1;
            tmpRank.myMaxStarCount = 321;
            tmpRank.ranklist = new RammbockRanklist_ItemData[] {
                new RammbockRanklist_ItemData(){ nickname = "www", power = 123, headIconId = 1, ranking = 1, vipLv = 123, starCount = 100 }
            };
            Refresh(tmpRank);
        }
    }

    public override bool Refresh(object param)
    {
        mRammbockRanklist = param as SC_Ranklist_GetClimbTowerStarsRank;

        return base.Refresh(param);
    }

    protected override string _OnGetWindowName()
    {
        return "RANKLIST_RAMMBOCK_WINDOW";
    }

    protected override string _GetCloseBtnName()
    {
        return "rammbock_rank_list_close_button";
    }

    protected override void _OnRequestRanklist()
    {
        GlobalModule.DoCoroutine(RanklistNetManager.RequestRammbockRanklist());
    }

    protected override string _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE itemType)
    {
        string tmpName = "";
        switch (itemType)
        {
            case RANKLIST_ITEM_COMPONENT_TYPE.ITEM_GRID: tmpName = "rank_list_grid"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.NICKNAME: tmpName = "role_name_label"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.POWER: tmpName = "fight_strength_number"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.HEAD_ICON_ID: tmpName = "icon_role"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.RANKING: tmpName = "ranking_name_label"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.VIP_LEVEL: tmpName = "vip_sprite"; break;
        }
        return tmpName;
    }

    /*
    protected override GameObject _OnGetResetTimeGO()
    {
        return GetSub("rank_reset_time");
    }
     * */

    protected override void _OnRefreshRanklistItem(GameObject goItem, IRanklistItemData itemData)
    {
        RammbockRanklist_ItemData tmpItemData = itemData as RammbockRanklist_ItemData;
		//设置VIP
		GameCommon.SetUIText (goItem,"num",tmpItemData.VIPLv.ToString());

        //星星
        GameCommon.SetUIText(goItem, "player_rank_reward", tmpItemData.starCount.ToString());

		// 设置前3名奖杯图标
		for (int i = 0; i < 3; i++)
			GameCommon.SetUIVisiable(goItem, "0" + (i + 1)+"_sprite".ToString(), tmpItemData.Ranking == (i + 1));
		GameCommon.SetUIVisiable(goItem, "peak_rank_num", tmpItemData.Ranking != 1 && tmpItemData.Ranking != 2 && tmpItemData.Ranking != 3);
    }

    protected override void _OnRefreshMyRanking()
    {
        //排名
//        GameCommon.SetUIVisiable(mGameObjUI, "cur_rank_number", mRammbockRanklist.myRanking > 0);
        GameCommon.SetUIText(mGameObjUI, "cur_rank_number", (mRammbockRanklist.myRanking > 0) ? mRammbockRanklist.myRanking.ToString() : "未上榜");

        //历史最高
        GameCommon.SetUIText(mGameObjUI, "highest_number", mRammbockRanklist.myMaxStarCount.ToString());
    }

    protected override long _OnGetResetTime()
    {
        long tmpNowServerTime = CommonParam.NowServerTime();
        DateTime tmpNowDate = GameCommon.ConvertServerSecTimeTo1970(tmpNowServerTime);
        DateTime tmpDstDate = tmpNowDate.AddSeconds(-(tmpNowDate.Hour * 3600 + tmpNowDate.Minute * 60 + tmpNowDate.Second));
        tmpDstDate.AddHours(24);
        TimeSpan tmpLeft = tmpDstDate - tmpNowDate;
        return (tmpNowServerTime + (long)tmpLeft.TotalSeconds);
    }
    protected override IRanklistItemData[] _OnGetRanklist()
    {
        if (mRammbockRanklist == null)
            return null;
        return mRammbockRanklist.ranklist;
    }
}
