using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;
using System;

//天魔排行

public enum BOSSBATTLE_RANKLIST_FEAT_TYPE
{
    DAMAGE = 2,
    FEAT
}

/// <summary>
/// 天魔排行主界面
/// </summary>
public class RanklistBossBattleWindow : RanklistMainUI
{
    private static Dictionary<int, List<DataRecord>> msDicFeatAward = new Dictionary<int, List<DataRecord>>();       //Key为类型，Value为记录

    public override void Init()
    {
        base.Init();
        
        msDicFeatAward.Clear();
        msDicFeatAward.Add((int)BOSSBATTLE_RANKLIST_FEAT_TYPE.DAMAGE, new List<DataRecord>());
        msDicFeatAward.Add((int)BOSSBATTLE_RANKLIST_FEAT_TYPE.FEAT, new List<DataRecord>());
        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mFeatAwardConfig.GetAllRecord())
        {
            if ((int)pair.Value.getObject("AWARD_TYPE") == (int)BOSSBATTLE_RANKLIST_FEAT_TYPE.DAMAGE)
                msDicFeatAward[(int)BOSSBATTLE_RANKLIST_FEAT_TYPE.DAMAGE].Add(pair.Value);
            else if ((int)pair.Value.getObject("AWARD_TYPE") == (int)BOSSBATTLE_RANKLIST_FEAT_TYPE.FEAT)
                msDicFeatAward[(int)BOSSBATTLE_RANKLIST_FEAT_TYPE.FEAT].Add(pair.Value);
        }
    }

    protected override string _GetCloseBtnName()
    {
        return "boss_rank_list_close_button";
    }
    protected override string _GetWindowName()
    {
        return "RANKLIST_BOSSBATTLE_WINDOW";
    }
    protected override List<string> _GetTabBtnName()
    {
        List<string> listTabsBtnName = new List<string>(){
            "boss_rank_hurt_button",
            "boss_rank_feats_button",
            "boss_rank_role_break_button"
        };
        return listTabsBtnName;
    }
    protected override List<string> _GetSubWindowName()
    {
        List<string> listSubWin = new List<string>(){
            "RANKLIST_BOSSBATTLE_DAMAGE_WINDOW",
            "RANKLIST_BOSSBATTLE_FEATS_WINDOW",
            "RANKLIST_BOSSBATTLE_AWARD_WINDOW"
        };
        return listSubWin;
    }

    /// <summary>
    /// 获取奖励列表
    /// </summary>
    /// <returns></returns>
    public static List<DataRecord> GetFeatTypeAward(BOSSBATTLE_RANKLIST_FEAT_TYPE featType)
    {
        if (featType != BOSSBATTLE_RANKLIST_FEAT_TYPE.DAMAGE && featType != BOSSBATTLE_RANKLIST_FEAT_TYPE.FEAT)
            return null;
        if (msDicFeatAward == null)
            return null;

        return msDicFeatAward[(int)featType];
    }
    /// <summary>
    /// 根据排行和类型获取奖励信息
    /// </summary>
    /// <param name="myRanking"></param>
    /// <param name="featType"></param>
    /// <param name="awardAtlas"></param>
    /// <param name="awardSprite"></param>
    /// <param name="awardNum"></param>
    /// <param name="dstRanking"></param>
    /// <param name="dstAwardAtlas"></param>
    /// <param name="dstAwardSprite"></param>
    /// <param name="dstAwardNum"></param>
    public static void GetFeatAward(
        int myRanking, BOSSBATTLE_RANKLIST_FEAT_TYPE featType,
        out string awardAtlas, out string awardSprite, out int awardNum,
        out int dstRanking,out string dstAwardAtlas, out string dstAwardSprite, out int dstAwardNum)
    {
        awardAtlas = "";
        awardSprite = "";
        awardNum = 0;
        dstRanking = 0;
        dstAwardAtlas = "";
        dstAwardSprite = "";
        dstAwardNum = 0;

        if (featType != BOSSBATTLE_RANKLIST_FEAT_TYPE.DAMAGE && featType != BOSSBATTLE_RANKLIST_FEAT_TYPE.FEAT)
            return;

        string tmpName = (featType == BOSSBATTLE_RANKLIST_FEAT_TYPE.DAMAGE) ? "DAMAGE_RANK_" : "FEAT_RANK_";
        if (myRanking == 0 || myRanking == -1)
        {
            List<DataRecord> tmpDstList = msDicFeatAward[(int)featType];
            int tmpCount = tmpDstList.Count;
            if (tmpCount <= 0)
                return;
            DataRecord tmpDstConfig = tmpDstList[tmpCount - 1];
            int tmpDstAwardId = (int)tmpDstConfig.getObject("AWARD_ID");
            dstRanking = (int)tmpDstConfig.getObject(tmpName + "MAX");
            dstAwardNum = (int)tmpDstConfig.getObject("AWARD_NUM");
            GameCommon.GetItemAtlasSpriteName(tmpDstAwardId, out dstAwardAtlas, out dstAwardSprite);
            return;
        }

        List<DataRecord> tmpList = msDicFeatAward[(int)featType];
        for (int i = tmpList.Count - 1; i >= 0; i--)
        {
            DataRecord tmpConfig = tmpList[i];
            int tmpMin = (int)tmpConfig.getObject(tmpName + "MIN");
            int tmpMax = (int)tmpConfig.getObject(tmpName + "MAX");
            if (i == tmpList.Count - 1)
            {
                if (myRanking > tmpMax)
                {
                    dstRanking = tmpMax;
                    int tmpDstAwardId = (int)tmpConfig.getObject("AWARD_ID");
                    dstRanking = (int)tmpConfig.getObject(tmpName + "MAX");
                    dstAwardNum = (int)tmpConfig.getObject("AWARD_NUM");
                    GameCommon.GetItemAtlasSpriteName(tmpDstAwardId, out dstAwardAtlas, out dstAwardSprite);
                    break;
                }
            }
            if (myRanking >= tmpMin && myRanking <= tmpMax)
            {
                int tmpAwardId = (int)tmpConfig.getObject("AWARD_ID");
                awardNum = (int)tmpConfig.getObject("AWARD_NUM");
                GameCommon.GetItemAtlasSpriteName(tmpAwardId, out awardAtlas, out awardSprite);
                if (i == 0)
                {
                    dstRanking = 1;
                    dstAwardAtlas = awardAtlas;
                    dstAwardSprite = awardSprite;
                    dstAwardNum = awardNum;
                }
                else
                {
                    DataRecord tmpDstConfig = tmpList[i - 1];
                    int tmpDstAwardId = (int)tmpDstConfig.getObject("AWARD_ID");
                    dstRanking = (int)tmpDstConfig.getObject(tmpName + "MAX");
                    dstAwardNum = (int)tmpDstConfig.getObject("AWARD_NUM");
                    GameCommon.GetItemAtlasSpriteName(tmpDstAwardId, out dstAwardAtlas, out dstAwardSprite);
                }
                break;
            }
        }
    }

    /// <summary>
    /// 获取剩余重置时间
    /// </summary>
    /// <returns></returns>
    public static long GetResetTime()
    {
        long tmpNowServerTime = CommonParam.NowServerTime();
        DateTime tmpNowDate = GameCommon.ConvertServerSecTimeTo1970(tmpNowServerTime);
        DateTime tmpDstDate = tmpNowDate.AddSeconds(-(tmpNowDate.Hour * 3600 + tmpNowDate.Minute * 60 + tmpNowDate.Second));
        tmpDstDate = tmpDstDate.AddHours(24);
        TimeSpan tmpLeft = tmpDstDate - tmpNowDate;
        return (tmpNowServerTime + (long)tmpLeft.TotalSeconds);
    }
}

/// <summary>
/// 天魔伤害排行
/// </summary>
public class RanklistBossBattleDamageWindow : RanklistSubUI
{
    private SC_Ranklist_BossBattleDamageRanklist mDamageRanklist;

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_boss_rank_damage_rank_button", new DefineFactory<Button_visit_friend_button>());
//        EventCenter.Self.RegisterEvent("Button_boss_rank_damage_rank_button", new DefineFactoryLog<Button_Check_Target_TeamInfo>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        //TODO Test
        if (!RanklistNetManager.IsSend)
        {
            SC_Ranklist_BossBattleDamageRanklist tmpRank = new SC_Ranklist_BossBattleDamageRanklist();
            tmpRank.myRanking = 5;
            tmpRank.ranklist = new BossBattleDamageRanklist_ItemData[] {
                new BossBattleDamageRanklist_ItemData(){ nickname = "迷人的端木蒲", playerId = "14", power = 123, headIconId = 1, ranking = 1, vipLv = -1, damage = 150000 }
            };
            Refresh(tmpRank);
        }
    }

    public override bool Refresh(object param)
    {
        mDamageRanklist = param as SC_Ranklist_BossBattleDamageRanklist;

        return base.Refresh(param);
    }

    protected override string _OnGetWindowName()
    {
        return "RANKLIST_BOSSBATTLE_DAMAGE_WINDOW";
    }

    protected override void _OnRequestRanklist()
    {
        GlobalModule.DoCoroutine(RanklistNetManager.RequestBossBattleDamageRanklist());
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
            case RANKLIST_ITEM_COMPONENT_TYPE.VIP_LEVEL: tmpName = "VIP"; break;
        }
        return tmpName;
    }

    protected override GameObject _OnGetResetTimeGO()
    {
        return GetSub("rank_reset_time");
    }

    protected override void _OnRefreshRanklistItem(GameObject goItem, IRanklistItemData itemData)
    {
        BossBattleDamageRanklist_ItemData tmpItemData = itemData as BossBattleDamageRanklist_ItemData;

        //伤害
        GameCommon.SetUIText(goItem, "hurt_number", tmpItemData.damage.ToString());

        //查看阵容
        NiceData tmpCheckData = GameCommon.GetButtonData(goItem, "boss_rank_damage_rank_button");
        if (tmpCheckData != null)
        {
            tmpCheckData.set("WINDOW_NAME", "BOSS_RAID_WINDOW");
            tmpCheckData.set("FRIEND_ID", tmpItemData.playerId);
            tmpCheckData.set("FRIEND_NAME", tmpItemData.Nickname);
        }

		// 设置排名奖杯图标
		for (int i = 0; i < 3; i++)
			GameCommon.SetUIVisiable(goItem, "no" + (i + 1).ToString(), tmpItemData.Ranking == (i + 1));
		GameCommon.SetUIVisiable(goItem, "ranking_name_label", tmpItemData.Ranking != 1 && tmpItemData.Ranking != 2 && tmpItemData.Ranking != 3);

    }

    protected virtual int _GetMyRanking()
    {
        if (mDamageRanklist == null)
            return 0;
        return mDamageRanklist.myRanking;
    }
    protected virtual BOSSBATTLE_RANKLIST_FEAT_TYPE _GetFeatType()
    {
        return BOSSBATTLE_RANKLIST_FEAT_TYPE.DAMAGE;
    }
    protected override void _OnRefreshMyRanking()
    {
        int tmpMyRanking = _GetMyRanking();
        GameObject tmpParent = GetSub("role_rank_info");

        string awardAtlasName, awardSpriteName, dstAwardAtlasName, dstAwardSpriteName;
        int awardNum, dstRanking, dstAwardNum;
        RanklistBossBattleWindow.GetFeatAward(
            tmpMyRanking, _GetFeatType(),
            out awardAtlasName, out awardSpriteName, out awardNum,
            out dstRanking, out dstAwardAtlasName, out dstAwardSpriteName, out dstAwardNum);

        //是否上榜
        bool tmpIsInRanklist = (tmpMyRanking > 0);
        GameObject tmpGOMyRanking = GameCommon.FindObject(tmpParent, "role_rank_number_label");
        if(tmpGOMyRanking != null)
        {
            tmpGOMyRanking.SetActive(tmpIsInRanklist);
            tmpGOMyRanking.GetComponent<UILabel>().text = tmpMyRanking.ToString();
        }
        GameCommon.SetUIVisiable(tmpParent, "role_cur_label", !tmpIsInRanklist);

        //当前奖励
        GameObject tmpGOMyRankingNum = GameCommon.FindObject(tmpParent, "my_ranking_number");
        if(tmpGOMyRankingNum != null)
        {
            tmpGOMyRankingNum.SetActive(tmpIsInRanklist);
            tmpGOMyRankingNum.GetComponent<UILabel>().text = tmpMyRanking.ToString();
        }
        GameCommon.SetUIVisiable(tmpParent, "role_rank_label", !tmpIsInRanklist);
        GameObject tmpGOMyAwardParent = GameCommon.FindObject(tmpParent, "reward_info02");
        tmpGOMyAwardParent.SetActive(tmpIsInRanklist && (awardAtlasName != "" && awardSpriteName != ""));
        GameCommon.SetIcon(tmpGOMyAwardParent, "prestige_icon", awardSpriteName, awardAtlasName);
        GameCommon.SetUIText(tmpGOMyAwardParent, "number_label", awardNum.ToString());
        GameCommon.SetUIVisiable(tmpParent, "tips_rank_label", !tmpIsInRanklist);

		GameObject _curRankRootObj = GameCommon.FindObject (tmpParent,"cur_rank_label");
		Vector3 _originPos = _curRankRootObj.transform.localPosition;
		Vector3 _originRewardPos = tmpGOMyAwardParent.transform.localPosition;
		float _originPosY = -1.3f;
		float _mediumPosY = -15f;
		// 调整位置 如果是第一名则居中显示
		if (tmpMyRanking == 1) {
			_curRankRootObj.transform.localPosition = new Vector3 (_originPos.x, _mediumPosY, _originPos.z);
			tmpGOMyAwardParent.transform.localPosition = new Vector3(_originRewardPos.x,_mediumPosY,_originRewardPos.z);
		} else {
			_curRankRootObj.transform.localPosition = new Vector3 (_originPos.x, _originPosY, _originPos.z);
			tmpGOMyAwardParent.transform.localPosition = new Vector3(_originRewardPos.x,_originPosY,_originRewardPos.z);
		}

        //目标奖励
        GameCommon.SetUIVisiable(tmpParent, "aim_rank_label", tmpMyRanking != 1);
        GameCommon.SetUIText(tmpParent, "dst_ranking_number", dstRanking.ToString());
        GameObject tmpGODstAwardParent = GameCommon.FindObject(tmpParent, "reward_info01");
        tmpGODstAwardParent.SetActive(tmpMyRanking != 1 && (dstAwardAtlasName != "" && dstAwardSpriteName != ""));
        GameCommon.SetIcon(tmpGODstAwardParent, "prestige_icon", dstAwardSpriteName, dstAwardAtlasName);
        GameCommon.SetUIText(tmpGODstAwardParent, "number_label", dstAwardNum.ToString());
    }

    protected override long _OnGetResetTime()
    {
        return RanklistBossBattleWindow.GetResetTime();
    }
    protected override IRanklistItemData[] _OnGetRanklist()
    {
        if (mDamageRanklist == null)
            return null;
        return mDamageRanklist.ranklist;
    }
}
/// <summary>
/// 天魔功勋排行
/// </summary>
public class RanklistBossBattleFeatsWindow : RanklistBossBattleDamageWindow
{
    private SC_Ranklist_BossBattleFeatsRanklist mFeatsRanklist;

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_boss_rank_feats_rank_button", new DefineFactory<Button_visit_friend_button>());
//        EventCenter.Self.RegisterEvent("Button_boss_rank_damage_rank_button", new DefineFactoryLog<Button_Check_Target_TeamInfo>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        //TODO Test
        if (!RanklistNetManager.IsSend)
        {
            SC_Ranklist_BossBattleFeatsRanklist tmpRank = new SC_Ranklist_BossBattleFeatsRanklist();
            tmpRank.myRanking = 2;
            tmpRank.ranklist = new BossBattleFeatsRanklist_ItemData[] {
                new BossBattleFeatsRanklist_ItemData(){ nickname = "www1", playerId = "7", power = 123, headIconId = 1, ranking = 1, vipLv = 123, feats = 1000 }
            };
            Refresh(tmpRank);
        }
    }

    public override bool Refresh(object param)
    {
        mFeatsRanklist = param as SC_Ranklist_BossBattleFeatsRanklist;
        return base.Refresh(param);
    }

    protected override string _OnGetWindowName()
    {
        return "RANKLIST_BOSSBATTLE_FEATS_WINDOW";
    }

    protected override void _OnRequestRanklist()
    {
        GlobalModule.DoCoroutine(RanklistNetManager.RequestBossBattleFeatsRanklist());
    }

    protected override void _OnRefreshRanklistItem(GameObject goItem, IRanklistItemData itemData)
    {
        BossBattleFeatsRanklist_ItemData tmpItemData = itemData as BossBattleFeatsRanklist_ItemData;

        //伤害
        GameCommon.SetUIText(goItem, "hurt_number", tmpItemData.feats.ToString());

        //查看阵容
        NiceData tmpCheckData = GameCommon.GetButtonData(goItem, "boss_rank_feats_rank_button");
        if (tmpCheckData != null)
        {
            tmpCheckData.set("WINDOW_NAME", "BOSS_RAID_WINDOW");
            tmpCheckData.set("FRIEND_ID", tmpItemData.playerId);
            tmpCheckData.set("FRIEND_NAME", tmpItemData.Nickname);
        }
		for (int i = 0; i < 3; i++)
			GameCommon.SetUIVisiable(goItem, "no" + (i + 1).ToString(), tmpItemData.Ranking == (i + 1));
		GameCommon.SetUIVisiable(goItem, "ranking_name_label", tmpItemData.Ranking != 1 && tmpItemData.Ranking != 2 && tmpItemData.Ranking != 3);
    }

    protected override int _GetMyRanking()
    {
        if (mFeatsRanklist == null)
            return 0;
        return mFeatsRanklist.myRanking;
    }
    protected override BOSSBATTLE_RANKLIST_FEAT_TYPE _GetFeatType()
    {
        return BOSSBATTLE_RANKLIST_FEAT_TYPE.FEAT;
    }

    protected override IRanklistItemData[] _OnGetRanklist()
    {
        if (mFeatsRanklist == null)
            return null;
        return mFeatsRanklist.ranklist;
    }
}
/// <summary>
/// 天魔排名奖励
/// </summary>
public class RanklistBossBattleAwardWindow : tWindow
{
    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        UIGridContainer tmpGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "rank_list_grid");
        List<DataRecord> tmpDamageListConfig = RanklistBossBattleWindow.GetFeatTypeAward(BOSSBATTLE_RANKLIST_FEAT_TYPE.DAMAGE);
        List<DataRecord> tmpFeatsListConfig = RanklistBossBattleWindow.GetFeatTypeAward(BOSSBATTLE_RANKLIST_FEAT_TYPE.FEAT);
        int tmpCount = tmpDamageListConfig.Count;
		int featCount = tmpFeatsListConfig.Count;
        tmpGridContainer.MaxCount = tmpCount;
        string tmpDamageRankMin = "DAMAGE_RANK_MIN";
        string tmpDamageRankMax = "DAMAGE_RANK_MAX";
        for (int i = 0; i < tmpCount; i++)
        {
            GameObject tmpItem = tmpGridContainer.controlList[i];
            DataRecord tmpDamageConfig = tmpDamageListConfig[i];
		
			DataRecord tmpFeatsConfig = null;
			if(i < featCount)
				tmpFeatsConfig = tmpFeatsListConfig[i];

            int tmpMin = (int)tmpDamageConfig.getObject(tmpDamageRankMin);
            int tmpMax = (int)tmpDamageConfig.getObject(tmpDamageRankMax);

            //排名
            string tmpRanking = "第";
            if (tmpMin == tmpMax)
                tmpRanking += tmpMin.ToString();
            else
                tmpRanking += (tmpMin.ToString() + "-" + tmpMax.ToString());
            tmpRanking += "名";
            GameCommon.SetUIText(tmpItem, "ranking_label", tmpRanking);




            //伤害奖励
            int tmpDamageId = (int)tmpDamageConfig.getObject("AWARD_ID");
            int tmpDamageNum = (int)tmpDamageConfig.getObject("AWARD_NUM");
            GameCommon.SetUIText(tmpItem, "hurt_reward_label", GameCommon.GetItemName(tmpDamageId) + "x" + tmpDamageNum.ToString());

            //功勋奖励
			if(tmpFeatsConfig != null){
				int tmpFeatsId = (int)tmpFeatsConfig.getObject("AWARD_ID");
				int tmpFeatsNum = (int)tmpFeatsConfig.getObject("AWARD_NUM");
				GameCommon.SetUIText(tmpItem, "feats_reward_label", GameCommon.GetItemName(tmpFeatsId) + "x" + tmpFeatsNum.ToString());
			}
			GameCommon.SetUIVisiable(tmpItem,"feats_reward_label",tmpFeatsConfig != null);
        }

        return true;
    }
}
