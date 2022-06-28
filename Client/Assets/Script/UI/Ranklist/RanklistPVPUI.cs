using UnityEngine;
using System.Collections;
using Logic;
using System;
using System.Collections.Generic;
using DataTable;

//竞技场排行

public class RanklistPVPWindow : RanklistSubUI
{
    private List<DataRecord> mListConfig = new List<DataRecord>();

    private SC_Ranklist_GetArenaRankList mPVPRanklist;

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_peak_rank_btn", new DefineFactoryLog<Button_peak_rank_btn>());
        EventCenter.Self.RegisterEvent("Button_peak_rank_check_team_button", new DefineFactory<Button_pvp_visit_friend_button>());
//        EventCenter.Self.RegisterEvent("Button_peak_rank_check_team_button", new DefineFactoryLog<Button_Check_Target_TeamInfo>());

        mListConfig.Clear();
        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mPvpRankAwardConfig.GetAllRecord())
            mListConfig.Add(pair.Value);
    }

    public override void Open(object param)
    {
        base.Open(param);

        //TODO Test
        if (!RanklistNetManager.IsSend)
        {
            SC_Ranklist_GetArenaRankList tmpRank = new SC_Ranklist_GetArenaRankList();
            tmpRank.myRanking = 14;
            tmpRank.ranklist = new GetArenaRankList_ItemData[] {
                new GetArenaRankList_ItemData(){ nickname = "www1", playerId = "7", power = 123, headIconId = 1, ranking = 1 }
            };
            Refresh(tmpRank);
        }
    }

    public override bool Refresh(object param)
    {
        mPVPRanklist = param as SC_Ranklist_GetArenaRankList;

        return base.Refresh(param);
    }

    protected override string _OnGetWindowName()
    {
        return "RANKLIST_PVP_WINDOW";
    }

    protected override string _GetCloseBtnName()
    {
        return "peak_rank_close_button";
    }

    protected override void _OnRequestRanklist()
    {
        GlobalModule.DoCoroutine(RanklistNetManager.RequestPVPRanklist());
    }

    protected override string _GetComponentNameByType(RANKLIST_ITEM_COMPONENT_TYPE itemType)
    {
        string tmpName = "";
        switch (itemType)
        {
            case RANKLIST_ITEM_COMPONENT_TYPE.ITEM_GRID: tmpName = "rank_list_grid"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.NICKNAME: tmpName = "role_name_label"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.POWER: tmpName = "fight_strength_number"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.HEAD_ICON_ID: tmpName = "item_icon"; break;
            case RANKLIST_ITEM_COMPONENT_TYPE.RANKING: tmpName = "peak_rank_num"; break;
        }
        return tmpName;
    }

    protected override void _OnRefreshRanklistItem(GameObject goItem, IRanklistItemData itemData)
    {
        GetArenaRankList_ItemData tmpItemData = itemData as GetArenaRankList_ItemData;

        //排名
        GameObject tmpRankParent = GameCommon.FindObject(goItem, "peak_rank");
        for (int i = 0; i < 3; i++)
            GameCommon.SetUIVisiable(tmpRankParent, "no" + (i + 1).ToString(), tmpItemData.Ranking == (i + 1));
        GameCommon.SetUIVisiable(tmpRankParent, "peak_rank_num", tmpItemData.Ranking != 1 && tmpItemData.Ranking != 2 && tmpItemData.Ranking != 3);

        //奖励
        GameObject tmpAwardsParent = GameCommon.FindObject(goItem, "peak_rank_btn");
        DataRecord tmpAwardConfig = __GetConfigByRanking(tmpItemData.Ranking);
        List<ItemDataBase> tmpListAwards = (tmpAwardConfig != null) ? GameCommon.ParseItemList(tmpAwardConfig.getObject("AWARD_GROUPID").ToString()) : new List<ItemDataBase>();
        int tmpCount = tmpListAwards.Count;
        for (int i = 0; i < 3; i++)
        {
            GameObject tmpAwardItemParent = GameCommon.FindObject(tmpAwardsParent, "Sprite0" + (i + 1).ToString());
            tmpAwardItemParent.SetActive(i < tmpCount);
            if (i < tmpCount)
            {
                string tmpAtlas, tmpSprite;
                GameCommon.GetItemAtlasSpriteName(tmpListAwards[i].tid, out tmpAtlas, out tmpSprite);
                GameCommon.SetIcon(tmpAwardItemParent.GetComponent<UISprite>(), tmpAtlas, tmpSprite);
                GameCommon.SetUIText(tmpAwardItemParent, "num_label", tmpListAwards[i].itemNum.ToString());
            }
        }

        //按钮数据
        NiceData tmpCheckData = GameCommon.GetButtonData(goItem, "peak_rank_check_team_button");
        if (tmpCheckData != null)
        {
            tmpCheckData.set("WINDOW_NAME", "PEAK_PVP_WINDOW");
            tmpCheckData.set("FRIEND_ID", tmpItemData.playerId);
            tmpCheckData.set("FRIEND_NAME", tmpItemData.Nickname);
        }
    }

    private DataRecord __GetConfigByRanking(int ranking)
    {
        DataRecord tmpConfig = null;
        for (int i = 0, count = mListConfig.Count; i < count; i++)
        {
            int tmpMin = (int)mListConfig[i].getObject("RANK_MIN");
            int tmpMax = (int)mListConfig[i].getObject("RANK_MAX");
            if (ranking >= tmpMin && ranking <= tmpMax)
            {
                tmpConfig = mListConfig[i];
                break;
            }
        }
        return tmpConfig;
    }

    /// <summary>
    /// 根据当前等级，返回当前等级资源数据、目标等级资源数据
    /// </summary>
    /// <param name="myRanking"></param>
    /// <param name="myRankingIdx"></param>
    /// <param name="awardAtlas"></param>
    /// <param name="awardSprite"></param>
    /// <param name="awardNum"></param>
    /// <param name="dstRanking"></param>
    /// <param name="dstRankingIdx"></param>
    /// <param name="dstAwardAtlas"></param>
    /// <param name="dstAwardSprite"></param>
    /// <param name="dstAwardNum"></param>
    private void _GetAward(
        int myRanking, out int myRankingIdx, out List<string> awardAtlas, out List<string> awardSprite, out List<int> awardNum,
        out int dstRankingIdx, out int dstRanking, out List<string> dstAwardAtlas, out List<string> dstAwardSprite, out List<int> dstAwardNum)
    {
        myRankingIdx = 0;
        awardAtlas = new List<string>();
        awardSprite = new List<string>();
        awardNum = new List<int>();
        dstRankingIdx = 0;
        dstRanking = 0;
        dstAwardAtlas = new List<string>();
        dstAwardSprite = new List<string>();
        dstAwardNum = new List<int>();

        string tmpAtlas, tmpSprite;
        List<ItemDataBase> tmpItems;

        if (myRanking == 0 || myRanking == -1)
        {
            int tmpCount = mListConfig.Count;
            if (tmpCount <= 0)
                return;
            DataRecord tmpDstConfig = mListConfig[tmpCount - 1];
            dstRankingIdx = tmpCount;
            dstRanking = (int)tmpDstConfig.getObject("RANK_MAX");
            tmpItems = GameCommon.ParseItemList(tmpDstConfig.getObject("AWARD_GROUPID").ToString());
            for (int i = 0, count = tmpItems.Count; i < count; i++)
            {
                GameCommon.GetItemAtlasSpriteName(tmpItems[i].tid, out tmpAtlas, out tmpSprite);
                dstAwardAtlas.Add(tmpAtlas);
                dstAwardSprite.Add(tmpSprite);
                dstAwardNum.Add(tmpItems[i].itemNum);
            }
            return;
        }

        for (int i = mListConfig.Count - 1; i >= 0; i--)
        {
            DataRecord tmpConfig = mListConfig[i];
            int tmpMin = (int)tmpConfig.getObject("RANK_MIN");
            int tmpMax = (int)tmpConfig.getObject("RANK_MAX");
            if (i == mListConfig.Count - 1)
            {
                if (myRanking > tmpMax)
                {
                    dstRankingIdx = i;
                    dstRanking = tmpMax;
                    tmpItems = GameCommon.ParseItemList(tmpConfig.getObject("AWARD_GROUPID").ToString());
                    for (int k = 0, count = tmpItems.Count; k < count; k++)
                    {
                        GameCommon.GetItemAtlasSpriteName(tmpItems[k].tid, out tmpAtlas, out tmpSprite);
                        dstAwardAtlas.Add(tmpAtlas);
                        dstAwardSprite.Add(tmpSprite);
                        dstAwardNum.Add(tmpItems[k].itemNum);
                    }
                    break;
                }
            }
            if (myRanking >= tmpMin && myRanking <= tmpMax)
            {
                myRankingIdx = i + 1;
                tmpItems = GameCommon.ParseItemList(tmpConfig.getObject("AWARD_GROUPID").ToString());
                for (int j = 0, count = tmpItems.Count; j < count; j++)
                {
                    GameCommon.GetItemAtlasSpriteName(tmpItems[j].tid, out tmpAtlas, out tmpSprite);
                    awardAtlas.Add(tmpAtlas);
                    awardSprite.Add(tmpSprite);
                    awardNum.Add(tmpItems[j].itemNum);
                }
                if (i == 0)
                {
                    dstRankingIdx = 1;
                    dstRanking = 1;
                    dstAwardAtlas = awardAtlas;
                    dstAwardSprite = awardSprite;
                    dstAwardNum = awardNum;
                }
                else
                {
                    DataRecord tmpDstConfig = mListConfig[i - 1];
                    dstRankingIdx = i;
                    dstRanking = (int)tmpDstConfig.getObject("RANK_MAX");
                    tmpItems = GameCommon.ParseItemList(tmpDstConfig.getObject("AWARD_GROUPID").ToString());
                    for (int k = 0, count = tmpItems.Count; k < count; k++)
                    {
                        GameCommon.GetItemAtlasSpriteName(tmpItems[k].tid, out tmpAtlas, out tmpSprite);
                        dstAwardAtlas.Add(tmpAtlas);
                        dstAwardSprite.Add(tmpSprite);
                        dstAwardNum.Add(tmpItems[k].itemNum);
                    }
                }
                break;
            }
        }
    }

    protected override void _OnRefreshMyRanking()
    {
        if (mPVPRanklist == null)
            return;

        int tmpMyRanking = mPVPRanklist.myRanking;
        GameObject tmpParent = GetSub("peak_rank_info");

        List<string> awardAtlasName, awardSpriteName, dstAwardAtlasName, dstAwardSpriteName;
        List<int> awardNum, dstAwardNum;
        int myRankingIdx, dstRankingIdx, dstRanking;
        _GetAward(
            tmpMyRanking, out myRankingIdx, out awardAtlasName, out awardSpriteName, out awardNum,
            out dstRankingIdx, out dstRanking, out dstAwardAtlasName, out dstAwardSpriteName, out dstAwardNum);

        //是否上榜
        bool tmpIsInRanklist = (tmpMyRanking > 0);
        GameObject tmpGOMyRanking = GameCommon.FindObject(tmpParent, "peak_role_rank_number_label");
        if (tmpGOMyRanking != null)
        {
            tmpGOMyRanking.SetActive(tmpIsInRanklist);
            tmpGOMyRanking.GetComponent<UILabel>().text = tmpMyRanking.ToString();
        }
        GameCommon.SetUIVisiable(tmpParent, "peak_role_cur_label", !tmpIsInRanklist);

        //当前奖励
        GameObject tmpGOMyRankingNum = GameCommon.FindObject(tmpParent, "role_rank");
        if (tmpGOMyRankingNum != null)
        {
            tmpGOMyRankingNum.SetActive(tmpIsInRanklist);
            GameCommon.FindComponent<UILabel>(tmpGOMyRankingNum, "number").text = tmpMyRanking.ToString();
        }
        GameCommon.SetUIVisiable(tmpParent, "tips_rank_label", !tmpIsInRanklist);
        GameObject tmpGOMyAwardParent = GameCommon.FindObject(tmpParent, "reward_cur_info_group");
        tmpGOMyAwardParent.SetActive(tmpIsInRanklist);
        //当前奖励图标
        string tmpStrCurrAwardGroupID = "";
        if (tmpIsInRanklist)
        {
            DataRecord tmpAwardConfig = DataCenter.mPvpRankAwardConfig.GetRecord(myRankingIdx);
            tmpStrCurrAwardGroupID = (tmpAwardConfig != null) ? tmpAwardConfig.getObject("AWARD_GROUPID").ToString() : "";
            __SetAwardGridsInMyRanking(tmpGOMyAwardParent, tmpStrCurrAwardGroupID);
        }
        GameCommon.SetUIVisiable(tmpParent, "rank_reward01", tmpIsInRanklist && tmpStrCurrAwardGroupID != "");
        GameCommon.SetUIVisiable(tmpParent, "tips_rank_label", !tmpIsInRanklist);

        //目标奖励
        GameObject tmpGODstRankingNum = GameCommon.FindObject(tmpParent, "aim_rank");
        tmpGODstRankingNum.SetActive(tmpMyRanking != 1);
        GameCommon.SetUIText(tmpGODstRankingNum, "number", dstRanking.ToString());
        GameObject tmpGODstAwardParent = GameCommon.FindObject(tmpParent, "reward_aim_info_group");
        tmpGODstAwardParent.SetActive(tmpMyRanking != 1);
        //目标奖励图标
        string tmpStrDstAwardGroupID = "";
        DataRecord tmpDstAwardConfig = DataCenter.mPvpRankAwardConfig.GetRecord(dstRankingIdx);
        tmpStrDstAwardGroupID = (tmpDstAwardConfig != null) ? tmpDstAwardConfig.getObject("AWARD_GROUPID").ToString() : "";
        __SetAwardGridsInMyRanking(tmpGODstAwardParent, tmpStrDstAwardGroupID);
        GameCommon.SetUIVisiable(tmpParent, "rank_reward02", tmpMyRanking != 1 && tmpStrDstAwardGroupID != "");
    }
    /// <summary>
    /// 在我的排行信息中设置奖励信息
    /// </summary>
    /// <param name="goGridContainer"></param>
    /// <param name="listItems"></param>
    private void __SetAwardGridsInMyRanking(GameObject goGridContainer, string awardString)
    {
        if (goGridContainer == null)
            return;

        List<ItemDataBase> tmpListAward = (awardString != "") ? GameCommon.ParseItemList(awardString) : new List<ItemDataBase>();
        int tmpCount = tmpListAward.Count;
        UIGridContainer tmpGridContainer = goGridContainer.GetComponent<UIGridContainer>();
        tmpGridContainer.MaxCount = tmpCount;
        for (int i = 0; i < tmpCount; i++)
        {
            GameObject tmpGOItem = tmpGridContainer.controlList[i];
            ItemDataBase tmpItem = tmpListAward[i];

            //图标
            string tmpAtlas, tmpSprite;
            GameCommon.GetItemAtlasSpriteName(tmpItem.tid, out tmpAtlas, out tmpSprite);
            GameCommon.SetIcon(GameCommon.FindComponent<UISprite>(tmpGOItem, "prestige_icon"), tmpAtlas, tmpSprite);

            //数量
            GameCommon.SetUIText(tmpGOItem, "number_label", tmpItem.itemNum.ToString());
        }
    }

    protected override IRanklistItemData[] _OnGetRanklist()
    {
        if (mPVPRanklist == null)
            return null;
        return mPVPRanklist.ranklist;
    }
}

/// <summary>
/// PVP排行
/// </summary>
public class Button_peak_rank_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("RANKLIST_PVP_WINDOW");

        return true;
    }
}

class Button_pvp_visit_friend_button : CEvent
{
    public override bool _DoEvent()
    {
        string iFriendID = Convert.ToString(getObject("FRIEND_ID"));
//         if (iFriendID < 0)
//         {
//             DataCenter.OpenMessageWindow("无法查看阵容");
//             return true;
//         }
        string strName = getObject("FRIEND_NAME").ToString();
        string strWindowName = getObject("WINDOW_NAME").ToString();
        FriendNetEvent.RequestVisitPlayer(iFriendID, RequestVisitPlayerSuccess, ResponeVisitPlayerFail);
        /*
        tEvent evt = Net.StartEvent("CS_VisitFriend");
        evt.set("FRIEND_ID",  iFriendID);
        evt.set ("FRIEND_NAME", strName);
        evt.set ("WINDOW_NAME", strWindowName);
        evt.DoEvent();
        */

        return true;
    }

    void RequestVisitPlayerSuccess(string text)
    {
        SC_ResponeVisitPlayer vPlayer = JCode.Decode<SC_ResponeVisitPlayer>(text);
        if (vPlayer.ret == (int)STRING_INDEX.ERROR_NONE)
        {
            string strWindowName = get("WINDOW_NAME");
            //respEvt.set ("WINDOW_NAME", strWindowName);
            string iFriendID = get("FRIEND_ID").ToString();
            //respEvt.set ("FRIEND_ID", iFriendID);

            if (strWindowName == "RANK_WINDOW")
                GlobalModule.ClearAllWindow();
            else if (strWindowName == "FRIEND_WINDOW")
                GlobalModule.ClearAllWindow();
            //by chenliang
            //begin

            else if (strWindowName == "BOSS_RAID_WINDOW" ||
                    strWindowName == "RAMMBOCK_WINDOW" ||
                    strWindowName == "PEAK_PVP_WINDOW" ||
                    strWindowName == "UNION_WINDOW" ||
                    strWindowName == "RANKLIST_MAINUI_WINDOW")
            {
                tWindow tmpWin = DataCenter.GetData(strWindowName) as tWindow;
                if (tmpWin != null)
                    tmpWin.mGameObjUI.SetActive(false);
                GlobalModule.ClearAllWindow();
                DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "CLEAR_GRID", true);
            }

            //end
            else
                DataCenter.CloseWindow(strWindowName);

            DEBUG.Log("visit player...");
            DataCenter.OpenWindow("FRIEND_VISIT_WINDOW", strWindowName);
            //DataCenter.OpenWindow ("BACK_GROUP_FRIEND_VISIT_WINDOW");
            //DataCenter.OpenWindow ("INFO_GROUP_WINDOW");

            DataCenter.SetData("FRIEND_VISIT_WINDOW", "REFRESH", vPlayer);
        }
    }

    void ResponeVisitPlayerFail(string text)
    {
        switch (text)
        {
            case "103":
                {
                    DataCenter.OpenMessageWindow("无法查看阵容");
                } break;
        }
    }
}
