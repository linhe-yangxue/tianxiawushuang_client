using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class ArenaMainWindow : ArenaBase
{
	public float iIntervalTime = 0;
	public class IndexAreaWarrior
	{
		public int iIndex;
		public ArenaWarrior _ArenaWarrior;
	}

	public override void Init ()
	{
        EventCenter.Self.RegisterEvent("Button_arena_rank_list_btn", new DefineFactoryLog<Button_arena_rank_list_btn>());
        EventCenter.Self.RegisterEvent("Button_arena_adjust_btn", new DefineFactoryLog<Button_arena_adjust_btn>());
        EventCenter.Self.RegisterEvent("Button_arena_prestige_btn", new DefineFactoryLog<Button_arena_prestige_btn>());
        EventCenter.Self.RegisterEvent("Button_arena_window_back_btn", new DefineFactoryLog<Button_arena_window_back_btn>());
        EventCenter.Self.RegisterEvent("Button_arena_record_btn", new DefineFactoryLog<Button_arena_record_btn>());
        EventCenter.Self.RegisterEvent("Button_arena_rule_btn", new DefineFactoryLog<Button_arena_rule_btn>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);
		iIntervalTime = (float)TableCommon.GetNumberFromFightUiNotice(1, "TIME");

        DataCenter.OpenWindow(UIWindowString.arena_window_back);

        ArenaNetManager.RequestArenaChallengeList();
        //added by xuke 声望商店红点
        PVPNewMarkManager.Self.RequestPrestigeShopInfoAndCheckReward(() => { DataCenter.SetData("ARENA_MAIN_WINDOW", "REFRESH_NEWMARK", null); });
        //end
	}

    public void InitUI()
    {
        RefreshMyInfo();

        InitLeftWindow();

        //added by xuke 添加调整队伍红点提示
        GameObject _teamAdjustBtnObj = GameCommon.FindObject(mGameObjUI, "arena_adjust_btn");
        GameCommon.SetNewMarkVisible(_teamAdjustBtnObj,TeamManager.CheckTeamHasNewMark());
        //end
    }

    public void InitLeftWindow()
    {
        ArenaWarrior[] arenaArr = arenaWarriorArrObject;
        var pArenaList = GetUIGridContainer("arena_grid");
        pArenaList.MaxCount = arenaArr.Length;

        var pArena = pArenaList.controlList;
        for (int i = 0; i < arenaArr.Length; i++) 
        {
            GameObject board = pArena[i];
            if (arenaArr[i] != null)
            {
                RefreshBoard(arenaArr[i], board);
            }  
        }

        //移动到最下面
        if (arenaArr !=null && arenaArr.Length >= 4)
        {
            ScrollMaxDown();
        }
		ShowPVPPaoPaoTips();
//		GlobalModule.DoLater ( () => ShowPVPPaoPaoTips(), 1f);
    }
	public void ShowPVPPaoPaoTips()
	{
		List<IndexAreaWarrior> _list = ShowPVPPaoPaoList();
        //by chenliang
        //begin

        if (_list == null)
            return;

        //end
		int iCount = _list.Count;
		int _iIndex = UnityEngine.Random.Range(0,iCount);

		GameObject board = GetUIGridContainer("arena_grid").controlList[_list[_iIndex].iIndex];
		int iRank = GetShowRank(_list[_iIndex]._ArenaWarrior);
		SetPVPPaoPaoTips(board, iRank);
	}
	List<IndexAreaWarrior> ShowPVPPaoPaoList()
	{
        //by chenliang
        //begin

        if (arenaWarriorArrObject == null)
            return null;

        //end
		ArenaWarrior[] arenaArr = arenaWarriorArrObject;
		var pArenaList = GetUIGridContainer("arena_grid");
		pArenaList.MaxCount = arenaArr.Length;
		
		var pArena = pArenaList.controlList;
		List<IndexAreaWarrior> result = new List<IndexAreaWarrior>();
		for (int i = 0; i < arenaArr.Length; i++) 
		{
			GameObject board = pArena[i];
			if (arenaArr[i] != null)
			{
				if(IsInView(board))
				{
					result.Add (new IndexAreaWarrior(){iIndex = i, _ArenaWarrior = arenaArr[i]});
				}
			}  
		}
		return result;
	}
	
	bool IsInView(GameObject _obj)
	{
		UIPanel RestrictPanel = GameCommon.FindComponent<UIPanel>(mGameObjUI, "arena_scrollView"); 

		Bounds _relativeBounds = NGUIMath.CalculateRelativeWidgetBounds(RestrictPanel.transform, _obj.transform);
		Vector2 _relativePos_1 = new Vector2(_relativeBounds.min.x, _relativeBounds.min.y);
		Vector3 _offset = RestrictPanel.CalculateConstrainOffset(_relativePos_1, _relativeBounds.max);
		float iMax = 60.0f;
		return _offset.y < iMax && _offset.y > -iMax;
	}
	List<int> GetKeyGroup(int iValue)
	{
		List<int> result = new List<int>();
		
		foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mFightUiNotice.GetAllRecord())
		{
			DataRecord r = pair.Value;
			
			if (r["WIN"] == iValue)
			{
				result.Add(pair.Key);
			}
		}		
		return result;
	}
	void SetPVPPaoPaoTips(GameObject infoObj, int iPVPRank)
	{
		GameObject RoleTipsObj = GameCommon.FindObject (mGameObjUI, "pvp_paopao_tips").gameObject;
		List<int> iTipsIndexList = new List<int>();
		if(iPVPRank < GetShowRank(arenaWarriorObject) && iPVPRank != 0)
		{
			iTipsIndexList = GetKeyGroup(1);
		}else if(iPVPRank > GetShowRank(arenaWarriorObject) || iPVPRank == 0)
		{
			iTipsIndexList = GetKeyGroup(0);
		}
		if(iTipsIndexList.Count == 0)
			return;

		string str = TableCommon.GetStringFromFightUiNotice(iTipsIndexList[0], "NOTICE");
		int iMaxRate = TableCommon.GetNumberFromFightUiNotice(iTipsIndexList[0], "RATE");
		int iMinRate = 0;

		for (int i = 0; i < iTipsIndexList.Count; i++)
		{
			string tempStrLists = TableCommon.GetStringFromFightUiNotice(iTipsIndexList[i], "CONDITION");
			List<TipsInfoBase> tmpList = GameCommon.ParseTipsList(tempStrLists);
			if(GameCommon.SetIsCountion(tmpList))
			{
				int iTipsNum = UnityEngine.Random.Range(0,101);
				if(iTipsNum <= iMaxRate && iTipsNum >= iMinRate)
				{
					str = TableCommon.GetStringFromFightUiNotice(iTipsIndexList[i], "NOTICE");
					ClonePVPTips(infoObj, RoleTipsObj, str);
					break;
				}else 
				{
					if(i < iTipsIndexList.Count - 1)
					{
						iMaxRate = iMaxRate + TableCommon.GetNumberFromFightUiNotice(iTipsIndexList[i + 1], "RATE");
						iMinRate = iMinRate + TableCommon.GetNumberFromFightUiNotice(iTipsIndexList[i], "RATE");
					}else 
						break;
				}
			}else
				break;
		}
	}
	void ClonePVPTips(GameObject infoObj,GameObject RoleTipsObj, string str)
	{
		GameCommon.FindComponent<UILabel>(RoleTipsObj, "paopao_tips_label").text = str;
		GameObject obj = GameObject.Instantiate (RoleTipsObj) as GameObject;
		obj.transform.parent = infoObj.transform;
		obj.transform.localPosition = new Vector3(-15f, 0f,0f);
		obj.transform.localScale = Vector3.one;
		obj.SetActive (true);
		GlobalModule.DoLater (() => GameObject.DestroyImmediate (obj), 2.0f);
	}
    public void ScrollMaxDown()
    {
        //滚动到最下面 -！～！----滚动到自己的位置呀
        if (GameCommon.GetDataByZoneUid("ArenaMainNotSetDragAmount") == "")
        {
            float index = (GetIndexByArenaWarrior(arenaWarriorObject) + 1) * 1.0f;
            float pos = index / arenaWarriorArrObject.Length;
            UIScrollView uiScrollView = GameCommon.FindComponent<UIScrollView>(mGameObjUI, "arena_scrollView");

            DoLater(() =>
            {
                uiScrollView.SetDragAmount(0, 1.0f, false);
            }, 0.8f);
        }
        else 
        {
            GameCommon.SetDataByZoneUid("ArenaMainNotSetDragAmount", "");
        }
    }

    public void RefreshBoard(ArenaWarrior arenaWarrior, GameObject board)
    {
        //button
        setButtons(arenaWarrior, board);

        //info 名字 排名 战斗力 等级
        SetInfo(arenaWarrior, board);

        //icon 
        SetIcon(arenaWarrior, board);
    }

    private IEnumerator DoChallengeStart(ArenaWarrior player)
    {
        ArenaNetManager.ArenaBattleStartRequester req = new ArenaNetManager.ArenaBattleStartRequester(player);

        if (req.notEnoughSpirit)
        {
            //DataCenter.OpenMessageWindow("精力值不足");
            GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.SPIRIT_ITEM);
            yield break;
        }

        yield return req.Start();

        if (req.success)
        {
            DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.PVP); //设置升级跳转

            SC_ArenaBattleStart resp = req.respMsg;

            DataCenter.OpenWindow("PVP_FOUR_VS_FOUR_VICTORY_PREDICT_WINDOW");
            DataCenter.OpenWindow("PVP_FOUR_VS_FOUR_BATTLE_LOADING_WINDOW");
            yield return null;
            GlobalModule.ClearAllWindow(new List<string>()
            {
                "pvp_four_vs_four_victory_predict",
                "pvp_four_vs_four_battle_loading_window"
            });
        }
    }

    public void setButtons(ArenaWarrior arenaWarrior, GameObject board)
    {
        //是否能挑战
        GameCommon.FindObject(board, "arena_chanllenge_btn").SetActive(CanTiaoZhan(arenaWarrior) ? true : false);

        //战5次按钮显示
        if (IsFiveTimesPlayer(GetUid(arenaWarrior)) && GameCommon.IsFuncCanShow(board, "arena_five_chanllenge_btn"))
        {
            GameCommon.FindObject(board, "arena_five_chanllenge_btn").SetActive(true);
            GameCommon.SetUIPosition(board, "arena_chanllenge_btn", new Vector3(305.1f, arenaButtonY, 0));
        }

        //button
        Action action = () =>
        {
            //goto battle
            setChallengeUid(GetUid(arenaWarrior));
            setChallengeRank(GetTrueRank(arenaWarrior));
            setChallengeVipLevel(GetVipLevel(arenaWarrior));
            DoCoroutine(DoChallengeStart(arenaWarrior));
        };
        AddButtonAction(GameCommon.FindObject(board, "arena_chanllenge_btn"), action);

        //点角色头像-跳转阵容
        Action actionRole = () =>
        {
            //goto battle
            if (!IsFiveTimesPlayer(GetUid(arenaWarrior)))
            {
                GameCommon.SetDataByZoneUid("ArenaMainNotSetDragAmount", "NotSet");
                GameCommon.VisitGameFriend(GetUid(arenaWarrior), GetName(arenaWarrior), UIWindowString.arena_main_window); ;
            }
        };
        AddButtonAction(GameCommon.FindObject(board, "icon_role_btn"), actionRole);

        //button
        Action actionFive = () =>
        {
            //goto battle--//先注释-以防再用
            //if (RoleLogicData.Self.vipLevel < viplevel)
            //{
            //    string str = TableCommon.getStringFromStringList(STRING_INDEX.ARENA_NEET_VIP_LEVEL);
            //    string strGet = string.Format(str, viplevel);
            //    DataCenter.OpenMessageOkWindow(strGet, () =>
            //    {
            //        GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, CommonParam.rechageDepth);
            //    });
            //    DataCenter.RefreshMessageOkText(STRING_INDEX.ARENA_BUY_TO_THINK, STRING_INDEX.ARENA_TO_RECHAGRE);
            //    return;
            //}
            if (RoleLogicData.Self.spirit >= limitSpirit)
            {
                DataCenter.OpenWindow(UIWindowString.arena_list_window);
            }
            else
            {
                GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.SPIRIT_ITEM);
            }
        };
        AddButtonAction(GameCommon.FindObject(board, "arena_five_chanllenge_btn"), actionFive);
        
    }

    public void SetIcon(ArenaWarrior arenaWarrior, GameObject board)
    {
        //主角头像-机器人也一样
        GameCommon.SetOnlyItemIcon(board, "icon_role_btn", int.Parse(GetTid(arenaWarrior)));
        //GameCommon.SetItemIconNew(board, "icon_role_btn", int.Parse(GetTid(arenaWarrior)));
        
        //pet头像
        int[] pets = GetPets(arenaWarrior);
        for (int i = 0; i < pets.Length; i++)
        {
            GameCommon.FindObject(board, "pet_item_icon_" + i.ToString()).SetActive(true);
            GameCommon.SetOnlyItemIcon(board, "pet_item_icon_" + i.ToString(), pets[i]);
            //GameCommon.SetItemIconNew(board, "pet_item_icon_" + i.ToString(), pets[i]);
        }
        
    }

    public void SetInfo(ArenaWarrior arenaWarrior, GameObject board)
    {
        //亮的条
        GameCommon.SetUIVisiable(board, "Sprite_select", GetUid(arenaWarrior) == CommonParam.mUId ? true : false);

        //name
        GameCommon.SetUIText(board, "role_name_label", GetName(arenaWarrior));
        Color color = GameCommon.GetNameColor(int.Parse(GetTid(arenaWarrior)));
        GameCommon.FindObject(board, "role_name_label").GetComponent<UILabel>().color = color;

        //等级---
        GameObject objLevel = GameCommon.FindObject(board, "goods_number_label");
        GameCommon.SetUIText(objLevel, "num", GetLevel(arenaWarrior).ToString());
        
        //排名
        int rank = GetShowRank(arenaWarrior);
        if(rank == 1 || rank == 2 || rank == 3)
        {
            GameCommon.SetUIText(board, "cur_rank_number", "");
            GameCommon.FindObject(board, "0" + rank.ToString() + "_sprite").SetActive(true);
        }
        else if(rank == 0)
        {
            GameCommon.SetUIText(board, "cur_rank_number", "");
            GameCommon.FindObject(board, "jiqiren_sprite").SetActive(true);
        }
        else
        {
            GameCommon.SetUIText(board, "cur_rank_number", GetShowRank(arenaWarrior).ToString());
        }
        
        //战斗力 
        GameObject objFight = GameCommon.FindObject(board, "fight_strength");
        int power = GetPower(arenaWarrior);
        if(power < 1000000)
        {
            GameCommon.SetUIText(objFight, "fight_strength_number", power.ToString());
            GameCommon.FindObject(objFight, "label").SetActive(false);
        }
        else
        {
            int powerTemp = power / 10000;
            GameCommon.SetUIText(objFight, "fight_strength_number", powerTemp.ToString());
            GameCommon.FindObject(objFight, "label").SetActive(true);
        }
    }

    private void ShowRankAwardTips()
    {

    }

    private void RefreshMyReputation()
    {
        SetText("reputation_num", RoleLogicData.Self.reputation.ToString());
    }

    private void RefreshMyInfo()
    {
        //排名 声望 最高排名
        if (Guide.isActive && PlayerPrefs.GetInt(CommonParam.mUId + "FirstInArena") == 0)
        {
            PlayerPrefs.SetInt(CommonParam.mUId + "FirstInArena", 1);
            PlayerPrefs.Save();
            SetText("highest_number", (GetBestRank(arenaWarriorObject) + 1).ToString());
            SetText("cur_number_labels", (GetShowRank(arenaWarriorObject) + 1).ToString());
        }
        else
        {
            SetText("highest_number", GetBestRank(arenaWarriorObject).ToString());
            SetText("cur_number_labels", GetShowRank(arenaWarriorObject).ToString());
        }

        if (!PlayerPrefs.HasKey(CommonParam.mZoneID + RoleLogicData.Self.name + "HASINTOPEAKREWARDRANK"))
        {
            int showRank = GetMaxRankGetRankAward(); ;
            if(GetShowRank(arenaWarriorObject)<=showRank&&GetShowRank(arenaWarriorObject)!=0)
            {
                 PlayerPrefs.SetString(CommonParam.mZoneID + RoleLogicData.Self.name + "HASINTOPEAKREWARDRANK","YES");
                 PlayerPrefs.Save();
                 GetSub("reward_group").SetActive(true);
                 GetSub("reward_labels").SetActive(true);
                 GetSub("rank_reward_tips_label").SetActive(false);   
            }
            else
            {
              GetSub("reward_group").SetActive(false);
              GetSub("reward_labels").SetActive(false);
              GetSub("rank_reward_tips_label").SetActive(true);
              string mtip= DataCenter.mStringList.GetData((int)STRING_INDEX.PVP_SHOW_REWARD_MAX_RANK, "STRING_CN");
              SetText("rank_reward_tips_label",mtip);
            }

        }
        else
        {
           GetSub("reward_group").SetActive(true);
           GetSub("reward_labels").SetActive(true);
           GetSub("rank_reward_tips_label").SetActive(false);            
        }

        //声望
        RefreshMyReputation();

        //排名奖励
        setRankReward();
              
        GameObject _rightWindow = GetSub("right_window");
        GameCommon.SetUIText(_rightWindow, "fight_strength_number", GameCommon.ShowNumUI(System.Convert.ToInt32(GameCommon.GetPower().ToString("f0"))));
    }

    public void setRankReward()
    {
        string[] awardArr = GetRewardByRank(GetShowRank(arenaWarriorObject));
        if (awardArr != null)
        {
            for (int i = 0; i < awardArr.Length; i++)
            {
                if (awardArr[i] != null)
                {
                    string[] award = awardArr[i].Split('#');
                    GameObject obj = GameCommon.FindObject(mGameObjUI, "reward_info(Clone)_" + i.ToString());
                    GameCommon.SetItemIconNew(obj, "icon", int.Parse(award[0]), false);
                    GameCommon.SetUIText(obj, "need_num", award[1]);
                }
            }
        }
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "INIT_UI":
                InitUI();
                break;
            case "REFRESH_MY_REPUTATION":
                {
                    RefreshMyReputation();
                }
                break;
            case "REFRESH_NEWMARK":
                RefreshNewMark();
                break;
            default:
                break;
        }
    }

    private void RefreshNewMark() 
    {
        if (mGameObjUI == null)
            return;
        GameObject _prestigeShopBtnObj = GameCommon.FindObject(mGameObjUI, "arena_prestige_btn");
        GameCommon.SetNewMarkVisible(_prestigeShopBtnObj, PVPNewMarkManager.Self.PrestigeShopBtnVisible);
    }
	public override void Close ()
	{
		base.Close ();
		//by chenliang
        //begin

        //刷新快捷入口界面
        DataCenter.SetData("TRIAL_EASY_JUMP_WINDOW", "REFRESH", null);

        //end
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

public class Button_arena_window_back_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.arena_window_back);
        DataCenter.CloseWindow(UIWindowString.arena_main_window);
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
        DataCenter.OpenWindow("TRIAL_WINDOW");
        DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
        return true;
    }
}

public class Button_arena_prestige_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("PVP_PRESTIGE_SHOP_WINDOW");
        return true;
    }
}

public class Button_arena_record_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow(UIWindowString.arena_record_window);
        return true;
    }
}

public class Button_arena_rule_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("RULE_TIPS_WINDOW", HELP_INDEX.HELP_ATHLETICS);
        return true;
    }
}

public class Button_arena_rank_list_btn : CEvent
{
    public override bool _DoEvent()
    {
        ArenaNetManager.RequestArenaRankList();
        return true;
    }
}

public class Button_arena_adjust_btn : CEvent
{
    public override bool _DoEvent()
    {
        ArenaBase.openFourWindow(false);
        DataCenter.CloseWindow("TRIAL_WINDOW");
        DataCenter.CloseWindow("TRIAL_WINDOW_BACK");

        tWindow activeStageWindow = DataCenter.GetData("ACTIVE_STAGE_WINDOW") as tWindow;
        bool isActiveStageWindowOpen = activeStageWindow != null && activeStageWindow.mGameObjUI != null && activeStageWindow.mGameObjUI.activeInHierarchy;

        if (isActiveStageWindowOpen)
        {
            activeStageWindow.mGameObjUI.SetActive(false);
        }

        //close TRIAL_WINDOW 的时候会打开主界面
        //MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);

        DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);
        MainUIScript.Self.HideMainBGUI();
        MainUIScript.Self.mWindowBackAction = () => OnReturn();
        return true;
    }

    private void OnReturn()
    {

        DataCenter.OpenWindow("TRIAL_WINDOW");
        DataCenter.OpenWindow("TRIAL_WINDOW_BACK");

        DoButton();
    }

    private void DoButton()
    {
        MainUIScript.Self.OpenMainUI();
        DataCenter.OpenWindow(UIWindowString.arena_main_window);
        DataCenter.OpenWindow(UIWindowString.arena_window_back);
    }
}

public class Button_player_headshot : CEvent
{
    public override bool _DoEvent()
    {
        return true;
    }
}

