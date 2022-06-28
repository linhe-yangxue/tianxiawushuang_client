using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using DataTable;


public class PeakPvpWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_peak_window_back_btn", new DefineFactory<Button_peak_window_back_btn>());
		EventCenter.Self.RegisterEvent("Button_prestige_btn", new DefineFactory<Button_prestige_btn>());
		EventCenter.Self.RegisterEvent ("Button_adjust_btn",new DefineFactory<Button_adjust_pvp_team_button>());
    }

    public override void OnOpen()
    {
        DataCenter.OpenWindow("PEAK_WINDOW_BACK");

        // 显示玩家排名信息
        RefreshMyInfo();
        
        // 显示挑战列表
        RefreshChallengeList();

        // 显示奖励
        RefreshAward();

        // 初始化按钮
        AddButtonAction("refresh_button", () => DoCoroutine(DoRefreshChallengeList()));

        float duration = 5f - (Time.time - PeakPvpData.lastResfreshTime);

        if (duration < 0f)
        {
            SetRefreshButtonEnabled(true);
        }
        else 
        {
            DoCoroutine(DoDisableRefreshButton(duration));
        }
    }

    public override void OnClose()
    {
        DataCenter.CloseWindow("PEAK_WINDOW_BACK");
    }

    private void RefreshMyInfo()
    {
        SetText("highest_number", PeakPvpData.bestRank.ToString());
        SetText("cur_number_labels", PeakPvpData.curRank.ToString());
        SetText("reputation_num", RoleLogicData.Self.reputation.ToString());
		GameObject _rightWindow = GetSub ("right_window");
		GameCommon.SetUIText (_rightWindow,"fight_strength_number", GameCommon.ShowNumUI(System.Convert.ToInt32(GameCommon.GetPower().ToString("f0"))));
    }

    private void RefreshAward()
    {
        int index = PeakPvpData.GetAwardIndexByRank(PeakPvpData.curRank);
        string awardStr = DataCenter.mPvpRankAwardConfig.GetData(index, "AWARD_GROUPID");

        if (!string.IsNullOrEmpty(awardStr))
        {
            List<ItemDataBase> awards = GameCommon.ParseItemList(awardStr);
            int count = Mathf.Min(3, awards.Count);
            UIGridContainer container = GetComponent<UIGridContainer>("reward_group");

            for (int i = 0; i < count; ++i)
            {
                GameObject item = container.controlList[i];
                ItemDataBase data = awards[i];
//                DataRecord itemRec = DataCenter.mItemIcon.GetRecord(data.tid);
				//设置不带边框的icon
				DataRecord _iconRecord = DataCenter.mItemIcon.GetRecord(data.tid);
				string _atlasName = _iconRecord.getObject("ITEM_ICON_ATLAS").ToString();
				string _spriteName = _iconRecord.getObject("ITEM_ICON_SPRITE").ToString();
				UISprite _icon = GameCommon.FindObject(item,"title_prestige").GetComponent<UISprite>();
				GameCommon.SetIcon(_icon,_atlasName,_spriteName);
				_icon.MakePixelPerfect();
				GameCommon.SetUIText(item, "need_prestige_num", data.itemNum.ToString());
//                if (itemRec != null)
//                {
//                    string atlas = itemRec["ITEM_ATLAS_NAME"];
//                    string sprite = itemRec["ITEM_SPRITE_NAME"];
//
//                    GameCommon.SetIcon(item, "title_prestige", sprite, atlas);
//                    GameCommon.SetUIText(item, "need_prestige_num", data.itemNum.ToString());
//                }
            }
        }
    }

    private void RefreshChallengeList()
    {
        UIGridContainer grid = GetComponent<UIGridContainer>("peak_grid");
        grid.MaxCount = PeakPvpData.challengeList.Count;

        for (int i = 0; i < grid.MaxCount; ++i)
        {
            RefreshCell(grid.controlList[i], PeakPvpData.challengeList[i]);
        }
    }

	private const int THRESHOLD_TO_SHOW_WAN = 1000000;	//> 显示"万"的临界值
	private const int NUM_WAN = 10000;	//> 万
	private void RefreshCell(GameObject target, ArenaPlayer player)
    {
        GameCommon.SetUIText(target, "goods_name_label", player.rank.ToString());
        GameCommon.SetUIText(target, "role_name_label", player.name);

		// 显示战斗力数值
		int _power_to_show = player.power;
		GameObject _fightObj = GameCommon.FindObject (target,"fight_strength");
		GameCommon.SetUIVisiable (_fightObj,"label",_power_to_show >= THRESHOLD_TO_SHOW_WAN);
		if(_power_to_show >= THRESHOLD_TO_SHOW_WAN)
			_power_to_show = _power_to_show / NUM_WAN;
		GameCommon.SetUIText(target, "fight_strength_number", GameCommon.ShowNumUI (_power_to_show));
        GameCommon.SetUIText(target, "goods_number_label", "LV." + player.level);

        UIButtonEvent evt = GameCommon.FindComponent<UIButtonEvent>(target, "players_rank");
        evt.AddAction(() => DoCoroutine(DoChallengeStart(player)));
    }

    private IEnumerator DoRefreshChallengeList()
    {
        DoCoroutine(DoDisableRefreshButton(5f));
        RefreshChallengeListRequester req = new RefreshChallengeListRequester();
        yield return req.Start();

        if (req.success)
        {
            RefreshChallengeList();
        }
    }

    private IEnumerator DoChallengeStart(ArenaPlayer player)
    {
        ChallengeStartRequester req = new ChallengeStartRequester(player.uid, player.rank);

        if (req.notEnoughSpirit)
        {
            //DataCenter.OpenMessageWindow("精力值不足");
            GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.SPIRIT_ITEM);
            yield break;
        }

        yield return req.Start();

        if (req.success)
        {
            SC_ChallengeStart resp = req.respMsg;

            if (resp.isFighting != 0)
            {
                DataCenter.OpenMessageWindow("选择的对手正在战斗中，请选择其他对手或刷新列表");
            }
            else if (resp.rankChanged != 0)
            {
                DataCenter.OpenMessageWindow("选择的对手排名已变化，请选择其他对手或刷新列表");
            }
            else
            {
                PeakPvpData.challengePower = player.power;
                GlobalModule.ClearAllWindow();
                DataCenter.OpenWindow("PVP_FOUR_VS_FOUR_VICTORY_PREDICT_WINDOW");
                DataCenter.OpenWindow("PVP_FOUR_VS_FOUR_BATTLE_LOADING_WINDOW");
            }
        }
    }

    private IEnumerator DoDisableRefreshButton(float duration)
    {
        SetRefreshButtonEnabled(false);
        //by chenliang
        //begin

//        yield return new WaitForSeconds(duration);
//-----------------
        //持续更新按钮剩余可点击时间
        GameObject tmpGOBtnRefresh = GetSub("refresh_button");
        UILabel tmpLB = GameCommon.FindComponent<UILabel>(tmpGOBtnRefresh, "CDLabel");
        while (duration > 0.0f)
        {
            if (tmpLB != null)
                tmpLB.text = "(" + ((int)duration) + ")";
            float tmpDuration = Mathf.Min(duration, 1.0f);
            yield return new WaitForSeconds(tmpDuration);
            duration -= 1.0f;
        }
        if (tmpLB != null)
            tmpLB.text = "";

        //end
        SetRefreshButtonEnabled(true);
    }

    private void SetRefreshButtonEnabled(bool enabled)
    {
        GameCommon.SetUIButtonEnabled(mGameObjUI, "refresh_button", enabled);
    }
}


public class Button_peak_window_back_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("PEAK_PVP_WINDOW");
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
        return true;
    }
}

public class Button_prestige_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow("PVP_PRESTIGE_SHOP_WINDOW");
		return true;
	}
}

public class Button_adjust_pvp_team_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow("PEAK_PVP_WINDOW");
		DataCenter.CloseWindow("TRIAL_WINDOW");
		DataCenter.CloseWindow("TRIAL_WINDOW_BACK");

		MainUIScript.Self.HideMainBGUI ();
		DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);
		
		MainUIScript.Self.mWindowBackAction = () => OnReturn();
		return true;
	}
	
	private void OnReturn()
	{
	
		DataCenter.OpenWindow("TRIAL_WINDOW");
		DataCenter.OpenWindow("TRIAL_WINDOW_BACK");

		GlobalModule.DoCoroutine(DoButton());
		//return true;
	}

	private IEnumerator DoButton()
	{
		if (Time.time - PeakPvpData.lastResfreshTime > 5f)
		{
			GetArenaRankRequester rankReq = new GetArenaRankRequester();
			yield return rankReq.Start();
			
			if (!rankReq.success)
				yield break;
			
			RefreshChallengeListRequester listReq = new RefreshChallengeListRequester();
			yield return listReq.Start();
			
			if (!listReq.success)
				yield break;
		}
		
		MainUIScript.Self.OpenMainUI();
		DataCenter.OpenWindow("PEAK_PVP_WINDOW");
		
		//by chenliang
		//begin
		
		//        EventCenter.Start("Button_trial_window_back_btn").DoEvent();
		
		//end
	}
}


public class PeakPvpData
{
    public static int bestRank = 0;
    public static int curRank = 0;
    public static float lastResfreshTime = -10000f;
    public static List<ArenaPlayer> challengeList = new List<ArenaPlayer>();
    public static ChallengePlayer challengeTarget;
    public static int challengeUid = 0;
    public static int challengeRank = 0;
    public static int challengePower = 0;

    /// <summary>
    /// 通过排名获取奖励tid
    /// </summary>
    /// <param name="rank"> 玩家当前竞技场排名 </param>
    /// <returns> PvpRankAwardConfig.csv中的tid，若排名不在配置范围内，返回-1 </returns>
    public static int GetAwardIndexByRank(int rank)
    {
        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mPvpRankAwardConfig.GetAllRecord())
        {
            DataRecord r = pair.Value;
            int min = r["RANK_MIN"];
            int max = r["RANK_MAX"];

            if (min <= rank && rank <= max)
            {
                return pair.Key;
            }
        }

        return -1;
    }

    //public static PetData GetOpponentPetDataByTeamPos(int teamPos)
    //{
    //    if (challengeTarget == null || challengeTarget.pets == null)
    //        return null;

    //    return challengeTarget.pets.FirstOrDefault<PetData>(x => x.teamPos == teamPos);
    //}
}


public class PeakAccountInfo
{
    public bool mIsWin = false;
    public string mChallengeName = "";

    public ItemDataBase mAwardItem;
    public int mAwardGold = 0;
    public int mAwardRoleExp = 0;
    public int mAwardReputation = 0;
    public int mAwardGroup = 0;

    public int mPreRoleLevel = 0;
    public int mPreRoleExp = 0;
    public int mPreRank = 0;
    public int mPreBestRank = 0;

    public int mPostRoleLevel = 0;
    public int mPostRoleExp = 0;
    public int mPostRank = 0;
    public int mPostBestRank = 0;

    public bool mHasBreak = false;
    public int mBreakAward = 0;
}