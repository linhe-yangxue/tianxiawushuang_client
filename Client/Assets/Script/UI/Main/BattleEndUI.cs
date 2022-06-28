using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;
using Utilities;


public class BattleAccountWindow : tWindow
{
    protected BattleAccountInfo info;
    public override void Open(object param)
    {
        base.Open(param);
        info = param as BattleAccountInfo;

        if (info != null)
        {
            ObjectManager.Self.ClearAll();
            Perform();
        }
        else
        {
            DEBUG.LogError("Invalid parameter for open battle account window");
        }
    }

    public override void OnClose()
    {
        ObjectManager.Self.ClearAll();
    }

    protected virtual void Perform()
    {
        ShowModel();
        ShowBattleName();
        ShowBattleTime();
    }

    private void ShowBattleName()
    {
        string battleName = TableCommon.GetStringFromStageConfig(info.battleId, "NAME");
        SetText("stage_name", battleName);
    }

    private void ShowBattleTime()
    {
        //TimeSpan span = new TimeSpan((long)(info.battleTime * 10000000f));
        SetText("lab_time", vp_TimeUtility.TimeToString(info.battleTime)); 
    }

    private void ShowModel()
    {
        GameObject uiPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");
        BaseObject model = GameCommon.ShowCharactorModel(uiPoint, 1.6f);

        if (info.isWin)
            model.PlayAnim("cute", false);
        else
            model.PlayAnim("lose");
    }
}


public class BattleAccountWinWindow : BattleAccountWindow
{
	private List<ItemDataBase> _itemList = null;
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex,objVal);
		switch (keyIndex) 
		{
		case "SET_DROP_LIST_ITEM":
			_itemList = (List<ItemDataBase>)objVal;
			break;
		}
	}
    protected override void Perform()
    {
        base.Perform();

        // 初始化掉落物品列表
        UIGridContainer grid = GetSub("Grid").GetComponent<UIGridContainer>();
        grid.MaxCount = 0;

        // 初始化评级
        GameObject starInfo = GetSub("star_info");

        GameObject[] stars = new GameObject[3] {
            GameCommon.FindObject(starInfo, "star1"), 
            GameCommon.FindObject(starInfo, "star2"), 
            GameCommon.FindObject(starInfo, "star3") };

        foreach (var star in stars)
            star.SetActive(false);

        // 初始化金币
        UILabel goldLab = GetComponent<UILabel>("lab_coin");

        if (goldLab != null)
            goldLab.text = "0";

        // 初始化角色经验
       var roleExpBar = GetComponent<UIProgressBar>("progressbar_exp");
        float preRoleRate = (float)info.preRoleExp / TableCommon.GetRoleMaxExp(info.preRoleLevel);
        float postRoleRate = (float)info.postRoleExp / TableCommon.GetRoleMaxExp(info.postRoleLevel);
        UILabel percent = GameCommon.FindComponent<UILabel>(roleExpBar.gameObject, "player_lab_exp_pct");

        if (percent != null)
        {
            percent.text = (int)(preRoleRate * 100) + "%";
        }

        if (roleExpBar != null)
            roleExpBar.value = preRoleRate;

        UILabel levelLabel = GetComponent<UILabel>("lab_level");

        if (levelLabel != null)
            levelLabel.text = "Lv." + info.preRoleLevel;

        //计算经验是否播放升级效果
        //if (info.postRoleLevel > info.preRoleLevel)
        //{
        //    DataCenter.OpenWindow("UPGRADE_WINDOW");
        //    tWindow t = DataCenter.GetData("UPGRADE_WINDOW") as tWindow;

        //    if (t != null && t.mGameObjUI != null)
        //    {
        //        MonoBehaviour.Destroy(t.mGameObjUI, 2f);
        //    }
        //}
        //Character nomean = new Character();
        //nomean.AddExp(info.postRoleExp +TableCommon.GetRoleMaxExp(info.preRoleLevel)-info.preRoleExp);
        // 动态展示结算效果
        this.ExecuteQueued(

            ShowStars(stars),

            this.GroupCoroutine(
                UIKIt.PushNumberLabel(goldLab, 0, info.gold),
                UIKIt.PushLevelBar(this, roleExpBar, percent, info.preRoleLevel, preRoleRate, info.postRoleLevel, postRoleRate, x => { levelLabel.text = "Lv." + x; OnLevelUp(); })              
            ),

            ShowItems(),
            this.ActCoroutine(OnPerformCompleted)
            );
    }

    public void OnLevelUp()
    {
        DataCenter.OpenWindow("UPGRADE_WINDOW");
        tWindow t = DataCenter.GetData("UPGRADE_WINDOW") as tWindow;

        if (t != null && t.mGameObjUI != null)
        {
            MonoBehaviour.Destroy(t.mGameObjUI, 2f);
        }
        GlobalModule.DoLater(() =>
        {
            if (NewFuncOpenWindow.mbNeedBreak) 
            {
                NewFuncOpenWindow.mbNeedBreak = false;
                return;                
            }
            NewFuncOpenWindow.ShowGoToNewFuncWin(RoleLogicData.Self.character.level);
        }, NewFuncOpenWindow.mWaitTime);
    }

    protected virtual void OnPerformCompleted()
    { }

    private IEnumerator ShowStars(GameObject[] stars)
    {
        int starRate = info.starRate;

        for (int i = 0; i < starRate; ++i)
        {
            yield return new WaitForSeconds(0.3f);
            ShowStar(stars[i]);
        }

        yield return new WaitForSeconds(0.4f);
    }

    private void ShowStar(GameObject star)
    {
        GameObject sprite = GameCommon.FindObject(star, "sprite");
        sprite.SetActive(false);
        star.SetActive(true);
        this.ExecuteDelayed(() => sprite.SetActive(true), 0.4f);
    }

    private IEnumerator ShowItems()
    {
        //List<ItemDataBase> drops = MergedByTid(info.dropList);
		List<ItemDataBase> drops = MergedByTid(_itemList.ToArray());
		
        UIGridContainer grid = GetSub("Grid").GetComponent<UIGridContainer>();
        //grid.transform.localPosition = new Vector3(166 - 46 * drops.Count, -27, 0);

        ItemDataProvider provider = new ItemDataProvider(drops);
        ItemGrid itemGrid = new ItemGrid(grid);
        itemGrid.Reset();
        itemGrid.Set(provider);

        yield return this.StartCoroutine(ShowItemsInTurn(grid));
    }

    private IEnumerator ShowItemsInTurn(UIGridContainer container)
    {
        foreach (var item in container.controlList)
        {
            item.SetActive(false);
        }

        foreach (var item in container.controlList)
        {
            yield return new WaitForSeconds(0.2f);
            item.SetActive(true);
            container.Reposition();
        }
    }


    private List<ItemDataBase> MergedByTid(IEnumerable<ItemDataBase> items)
    {
        List<ItemDataBase> results = new List<ItemDataBase>();

        foreach (ItemDataBase it in items)
        {
            ItemDataBase item = results.Find(x => x.tid == it.tid);

            if (item == null)
            {
                results.Add(new ItemDataBase() { itemId = it.itemId, itemNum = it.itemNum, tid = it.tid });
            }
            else 
            {
                item.itemNum += it.itemNum;
            }
        }

        return results;
    }
}


public class BattleAccountLoseWindow : BattleAccountWindow
{
    private UIGridContainer mGridContainer;
    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "SHOW_NEWMARK":
                SetNewMarkVisible(true);
                break;
        }
    }
    protected virtual void SetNewMarkVisible(bool kVisible)
    {
        GameObject _backHomePageBtnObj = GameCommon.FindObject(mGameObjUI, "but_back_home_page");
        GameCommon.SetNewMarkVisible(_backHomePageBtnObj, kVisible);
    }

    protected override void Perform()
    {
        base.Perform();

        // 初始化金币
        UILabel goldLab = GetComponent<UILabel>("lab_coin");

        if (goldLab != null)
            goldLab.text = "0";

        // 初始化角色经验
        UILabel expLabel = GetComponent<UILabel>("lab_exp");

        if (expLabel != null)
            expLabel.text = "0";

        var roleExpBar = GetComponent<UIProgressBar>("progressbar_exp");
        float preRoleRate = (float)info.preRoleExp / TableCommon.GetRoleMaxExp(info.preRoleLevel);
        //float postRoleRate = (float)info.postRoleExp / TableCommon.GetRoleMaxExp(info.postRoleLevel);
        UILabel percent = GameCommon.FindComponent<UILabel>(roleExpBar.gameObject, "player_lab_exp_pct");

        if (percent != null)
        {
            percent.text = (int)(preRoleRate * 100) + "%";
        }

        if (roleExpBar != null)
            roleExpBar.value = preRoleRate;

        UILabel levelLabel = GetComponent<UILabel>("lab_level");

        if (levelLabel != null)
            levelLabel.text = "Lv." + info.preRoleLevel;

        //// 动态展示结算效果
        //this.ExecuteQueued(

        //    this.GroupCoroutine(
        //        UIKIt.PushNumberLabel(goldLab, 0, info.gold),
        //        UIKIt.PushNumberLabel(expLabel, 0, info.roleExp),
        //        UIKIt.PushLevelBar(this, roleExpBar, percent, info.preRoleLevel, preRoleRate, info.postRoleLevel, postRoleRate, x => levelLabel.text = "Lv." + x)
        //    ),

        //    this.ActCoroutine(OnPerformCompleted)
        //    );

        //活动副本设置
        DataCenter.Set("DAILAY_STAGE_GOTO_LINEUP", false);
        if (DataCenter.Get("IS_DAILYS_STAGE_BATTLE"))
        {
            GameCommon.FindObject(mGameObjUI, "but_back_home_page").SetActive(false);
            GameCommon.FindObject(mGameObjUI, "but_again").SetActive(false);
            GameObject objSelect = GameCommon.FindObject(mGameObjUI, "but_select");
            GameCommon.SetUIText(objSelect, "Label", TableCommon.getStringFromStringList(STRING_INDEX.DAILY_STAGE_MAIN_CLOSE));
        }
        else
        {
            //added by xuke
            SetNewMarkVisible(false);
            TeamManager.SetBackToHomePage_NewMark();
            //end
        }

        mGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI,"Grid");
        BattleFailManager.Self.InitFailGuideUI(mGridContainer, () => { Close(); });
    }

    //protected virtual void OnPerformCompleted()
    //{ }

}


public class PVEAccountWinWindow : BattleAccountWinWindow
{
    protected override void Perform()
    {
        base.Perform();

        ShowStarInfo();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "SHOW_NEWMARK":
                SetNewMarkVisible(true);
                break;
        }
    }
    protected virtual void SetNewMarkVisible(bool kVisible)
    {
        GameObject _backHomePageBtnObj = GameCommon.FindObject(mGameObjUI, "but_back_home_page");
        GameCommon.SetNewMarkVisible(_backHomePageBtnObj, kVisible);
    }

    private void ShowStarInfo()
    {
        DataRecord record = DataCenter.mStageTable.GetRecord(info.battleId);

        if (record != null)
        {
            GameObject infos = GetSub("star_level_infos");
            GameObject info1 = GameCommon.FindObject(infos, "spoil(Clone)_1");
            GameObject info2 = GameCommon.FindObject(infos, "spoil(Clone)_2");
            SetStarInfo(info1, record["ADDSTAR_1"], (info.starMask & (1 << 1)) > 0);
            SetStarInfo(info2, record["ADDSTAR_2"], (info.starMask & (1 << 2)) > 0);
        }

        //活动副本设置
        DataCenter.Set("DAILAY_STAGE_GOTO_LINEUP", false);
        if (DataCenter.Get("IS_DAILYS_STAGE_BATTLE"))
        {
            GameCommon.FindObject(mGameObjUI, "but_back_home_page").SetActive(false);
            GameCommon.FindObject(mGameObjUI, "but_next").SetActive(false);
            GameObject objSelect = GameCommon.FindObject(mGameObjUI, "but_select");
            GameCommon.SetUIText(objSelect, "Label", TableCommon.getStringFromStringList(STRING_INDEX.DAILY_STAGE_MAIN_CLOSE));
        }
        else 
        {
            //added by xuke
            SetNewMarkVisible(false);
            TeamManager.SetBackToHomePage_NewMark();
            //end
        }
        
    }

    private void SetStarInfo(GameObject info, int starRecordIndex, bool achieved)
    {
        string desc = StageStar.GetDescription(starRecordIndex);
        GameCommon.SetUIText(info, "star_level_info_label", desc);
        GameCommon.SetUIVisiable(info, "yes_star", achieved);
    }
}


public class PVEAccountLoseWindow : BattleAccountLoseWindow
{ }


public class PVEAccountCleanWindow : PVEAccountWinWindow
{
    protected override void Perform()
    {
        base.Perform();
        GameCommon.SetUIButtonEnabled(mGameObjUI, "but_clean", false);
    }

    protected override void OnPerformCompleted()
    {
        GameCommon.SetUIButtonEnabled(mGameObjUI, "but_clean", true);
    }
}


public class UIKIt
{
    public static IEnumerator PushBar(UIProgressBar bar, float from, float to, float speed, UILabel percentLabel)
    {
        if (bar == null)
            yield break;

        float rate = from;

        if (to > from)
        {
            while (rate < to)
            {
                bar.value = rate;

                if (percentLabel != null)
                {
                    percentLabel.text = (int)(rate * 100f) + "%";
                }

                yield return null;
                rate += speed * Time.deltaTime;
            }
        }
        else
        {
            while (rate > to)
            {
                bar.value = rate;

                if (percentLabel != null)
                {
                    percentLabel.text = (int)(rate * 100f) + "%";
                }

                yield return null;
                rate -= speed * Time.deltaTime;
            }
        }

        bar.value = to;
		rate = to;
        if (percentLabel != null)
        {

            percentLabel.text = (int)(rate * 100f) + "%";
        }
    }

    public static IEnumerator PushLevelBar(tWindow win, UIProgressBar bar, UILabel percentLab, int fromLv, float fromRate, int toLv, float toRate, Action<int> onLevelChanged)
    {
        if (bar == null)
            yield break;

        float speed = toLv + toRate - fromLv - fromRate;
        speed = Mathf.Clamp(speed, 0.5f, 3f);

        if (fromLv == toLv)
        {
            yield return win.DoCoroutine(PushBar(bar, bar.value, toRate, speed, percentLab));
        }
        else
        {
            yield return win.DoCoroutine(PushBar(bar, bar.value, 1f, speed, percentLab));
            int curLv = fromLv + 1;

           
            while (curLv < toLv)
            {
                if (onLevelChanged != null)
                {
                    onLevelChanged(curLv);
                }

                yield return win.DoCoroutine(PushBar(bar, 0f, 1f, speed, percentLab));
                ++curLv;
            }

            if (onLevelChanged != null)
            {
                onLevelChanged(curLv);
            }

            yield return win.DoCoroutine(PushBar(bar, 0f, toRate, speed, percentLab));
        }
    }

    public static IEnumerator PushNumberLabel(UILabel label, int from, int to)
    {
        if (label == null)
            yield break;

        label.text = from.ToString();
        float current = from;

        if (from < to)
        {
            while (current < to - 0.2f)
            {
                yield return null;
                current = Mathf.Lerp(current, to, Time.deltaTime * 10f);
                label.text = Mathf.FloorToInt(current).ToString();
            }
        }
        else 
        {
            while (current > to + 0.2f)
            {
                yield return null;
                current = Mathf.Lerp(current, to, Time.deltaTime * 10f);
                label.text = Mathf.CeilToInt(current).ToString();
            }
        }

        label.text = to.ToString();
    }
}


public class Button_but_clean : CEvent
{
    public override bool _DoEvent()
    {
        //判断是否有剩余次数
        int _stageID = DataCenter.Get("CURRETN_SAODANG_STAGE_ID");
        //判断是否有挑战次数
        if (!GameCommon.HasLeftChallengeTimes(_stageID))
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.GUILD_BOSS_GUNAKA_TIMES_OVER);
            return true;
        }
        //MainProcess.RequestAppearBoss(eACTIVE_AFTER_APPEAR.CLEAN_LEVEL);
        //NetManager.RequestBattleMainSweep();

		//stamia is enough
		if (RoleLogicData.Self.stamina < StageProperty.GetCurrentStageStaminaCost()/*CommonParam.battleMainStaminaCost*/)
        {
			//DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SELECT_LEVEL_NO_ENOUGH_STAMINA);
            GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.STAMINA_ITEM);
			return true;
		}

        //package is enough
        int stageId = DataCenter.Get("CURRENT_STAGE");
        int tmpDropGroupID = TableCommon.GetNumberFromStageConfig(stageId, "DROPGROUPID");
        List<ItemDataBase> tmpItems = GameCommon.GetmStageLootGroup(tmpDropGroupID);
        List<PACKAGE_TYPE> tmpTypes = PackageManager.GetPackageTypes(tmpItems);
        if (!CheckPackage.Instance.CanEnterBattle(tmpTypes))
            return true;

        GlobalModule.DoCoroutine(DoRequestClean());
        return true;
    }

    private IEnumerator DoRequestClean()
    {
        var req = new BattleMainSweepRequester();

//        if (req.canSweep)
//        {
            yield return req.Start();

            if (req.success)
            {
                DataCenter.CloseWindow("STAGE_INFO_WINDOW");
                DataCenter.CloseWindow("PVE_ACCOUNT_CLEAN_WINDOW");
                DataCenter.OpenWindow("PVE_ACCOUNT_CLEAN_WINDOW", req.accountInfo);

                //TODO Boss出现
                var sweepResult = req.respMsg;

                if (/*sweepResult.bossIndex != -1*/sweepResult.demonBoss != null && sweepResult.demonBoss.tid > 0)
                {
                    //Boss_GetDemonBossList_BossData bossData = new Boss_GetDemonBossList_BossData();
                    ////bossData.bossIndex = sweepResult.demonBoss.bossIndex;//sweepResult.bossIndex;
                    //bossData.tid = sweepResult.demonBoss.tid;//sweepResult.tid;
                    //bossData.finderId = System.Convert.ToString(CommonParam.mUId);
                    //bossData.finderName = RoleLogicData.Self.name;
                    //bossData.findTime = CommonParam.NowServerTime();
                    //DataRecord bossRecord = DataCenter.mBossConfig.GetRecord(bossData.tid);
                    //bossRecord.get("BASE_HP", out bossData.hpLeft);
                    RequestBossBattleEvent bossEvent = EventCenter.Start("Event_RequestBossBattle") as RequestBossBattleEvent;//new RequestBossBattleEvent();
                    bossEvent.mNextActive = eACTIVE_AFTER_APPEAR.QUIT_CLEAN_LEVEL;
                    bossEvent.mBossData = sweepResult.demonBoss;//bossData;
                    bossEvent.DoEvent();
                }
         
            }
        //}
//        else
//        {
//            DataCenter.OpenMessageWindow("扫荡券不足");
//        }
    }
}


public class Button_but_close : CEvent
{
    public override bool _DoEvent()
    {
        //MainProcess.RequestAppearBoss(eACTIVE_AFTER_APPEAR.QUIT_CLEAN_LEVEL);
        this.CloseOwnerWindow();
        DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "REFRESH", null);
        return true;
    }
}


public class Button_quit_boss_battle : CEvent
{
    public override bool _DoEvent()
    {
        this.CloseOwnerWindow();

        if (MainProcess.mStage != null)
        {
            MainProcess.ClearBattle();
            MainProcess.LoadRoleSelScene();
        }
        else
        {
            DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "REFRESH", null);
        }

        return true;
    }
}

public class Button_again_boss_battle : CEvent
{
	public override bool _DoEvent()
	{
		this.CloseOwnerWindow();
		
		BossBattle battle = MainProcess.mStage as BossBattle;
		Boss_GetDemonBossList_BossData _bossDate = (Boss_GetDemonBossList_BossData)getObject("IS_SHARE_PK");

		if (battle != null)
		{
			if (battle.mBoss != null && battle.mBoss.GetHp() > 0)
			{
				if(battle.mBossFinderDBID == CommonParam.mUId)
				{	
					if(battle.mIfShareWithFriend == 0)
					{
						Action action=()=>
						{
							HttpModule.CallBack requestSuccess_changeDemonBossShareState = (text) => 
							{
								DEBUG.Log("RequestSuccess:text = " + text);
								battle.mIfShareWithFriend = 1;
								_bossDate.ifShareWithFriend = 1;
								SetBossInfo (battle);
							};
							HttpModule.CallBack RequestFail_changeDemonBossShareState = (text) => 
							{
								if (string.IsNullOrEmpty(text))
								return;		
								SetBossInfo (battle);
								return;
							};
							CS_ChangeDemonBossShareState changeDemonBossShareState = new CS_ChangeDemonBossShareState(1);
							HttpModule.Instace.SendGameServerMessage(changeDemonBossShareState,"CS_ChangeDemonBossShareState",requestSuccess_changeDemonBossShareState,RequestFail_changeDemonBossShareState);
						};
						DataCenter.OpenMessageOkWindow("要求好友一起攻打天魔吗？", action, () => SetBossInfo (battle));
					}
					else 
						SetBossInfo (battle);
				}else
					SetBossInfo (battle);
			}else
			{
				MainProcess.ClearBattle();
				MainProcess.LoadRoleSelScene();
			}
		}
		return true;
	}
	void SetBossInfo(BossBattle battle)
	{
		if (battle != null)
		{
			MainProcess.ClearBattle();
			MainProcess.LoadRoleSelScene();
			
			if (MainUIScript.mCurIndex == MAIN_WINDOW_INDEX.WorldMapWindow)
				MainUIScript.Self.mStrWorldMapSubWindowName = "BOSS_RAID_WINDOW";
			else
				DataCenter.OpenWindow("BOSS_RAID_WINDOW");
		}
	}
}

public class Button_update_pet_team_button : CEvent
{
    public override bool _DoEvent()
    {
        this.CloseOwnerWindow();
        MainProcess.QuitBattle();
        MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow);
        return true;
    }
}

public class Button_update_active_stage_button : CEvent
{
    public override bool _DoEvent()
    {
        this.CloseOwnerWindow();
        DataCenter.Set("FROM_BATTLE_FAIL", "stage");
        MainProcess.QuitBattle();
        MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);
        
        return true;
    }
}

public class Button_update_equip_button : CEvent
{
    public override bool _DoEvent()
    {
        this.CloseOwnerWindow();
        DataCenter.Set("FROM_BATTLE_FAIL", "equip");
        MainProcess.QuitBattle();
        MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);
        return true;
    }
}

public class Button_update_shop_button : CEvent
{
    public override bool _DoEvent()
    {
        this.CloseOwnerWindow();

        DataCenter.Set("FROM_BATTLE_FAIL", "shop");
        MainProcess.QuitBattle();
        MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);
        return true;
    }
}
