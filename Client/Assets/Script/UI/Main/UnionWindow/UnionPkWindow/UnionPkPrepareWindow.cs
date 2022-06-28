using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class UnionPkPrepareWindow : UnionBase
{

    private StageProperty property;

	public override void Init ()
	{
        EventCenter.Self.RegisterEvent("Button_go_pk_rank_button", new DefineFactory<Button_go_pk_rank_button>());
        EventCenter.Self.RegisterEvent("Button_go_to_pk_button", new DefineFactory<Button_go_to_pk_button>());
        EventCenter.Self.RegisterEvent("Button_go_lineup_button", new DefineFactory<Button_go_lineup_button>());
        EventCenter.Self.RegisterEvent("Button_go_to_get_pk_reward_button", new DefineFactory<Button_go_to_get_pk_reward_button>());
        EventCenter.Self.RegisterEvent("Button_rewards_infos_close_button", new DefineFactory<Button_rewards_infos_close_button>());
        EventCenter.Self.RegisterEvent("Button_union_pk_back_window_Btn", new DefineFactory<Button_union_pk_back_window_Btn>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);

        DataCenter.Set("UNIONPK_CURRENT_WINDOW", UIWindowString.union_pk_prepare_window);
        DataCenter.OpenWindow(UIWindowString.union_pk_back_window);
        DataCenter.OpenWindow("INFO_GROUP_WINDOW");

        GuildBossNetManager.RequesGuildBossInfo(RoleLogicData.Self.guildId);
    
	}

    public void initUI(int index)
    {
        setRightInfos(index);
        setLeftInfos(index);
    }

    public void setBossModel(int index)
    {
        //BOSS模型
        int stageIndex = TableCommon.GetNumberFromConfig(index, "STAGE_ID", DataCenter.mGuildBoss);
        DataRecord stageConfig = DataCenter.mStageTable.GetRecord(stageIndex);
        RammbockModelState tmpState = RammbockModelState.NotFight;
        string bossID = stageConfig.getData("HEADICON");
        ActiveBirthForUI birthUI = GameCommon.FindObject(mGameObjUI, "UIPoint").GetComponent<ActiveBirthForUI>();
        if (birthUI != null)
            ShowModel(birthUI, System.Convert.ToInt32(bossID), tmpState, true);
    }

    private void ShowModel(ActiveBirthForUI birthUI, int bossID, RammbockModelState state, bool visible)
    {
        if (birthUI == null)
            return;

        GameObject obj = GetSub("boss_model");
        if (!visible)
        {
            if (obj != null)
                obj.SetActive(false);
            return;
        }
        if (obj != null)
            obj.SetActive(true);

        if (birthUI.mActiveObject != null)
            birthUI.mActiveObject.OnDestroy();

        birthUI.mBirthConfigIndex = bossID;
        //            birthUI.mObjectType = (int)OBJECT_TYPE.MONSTER_BOSS;
        birthUI.Init();
        if (birthUI.mActiveObject != null)
        {
            birthUI.mActiveObject.SetScale(80f);
            if (state == RammbockModelState.Win)
                birthUI.mActiveObject.PlayAnim("idle");
            else
                birthUI.mActiveObject.PlayAnim("idle");
            //__ChangeModelColor(birthUI.mActiveObject.mBodyObject, RammbockModelColor.mColor[(int)state]);

            BossBirthOnApearUI modelScript = birthUI.mActiveObject.mMainObject.AddComponent<BossBirthOnApearUI>();
            if (modelScript != null)
                modelScript.mActiveObject = birthUI.mActiveObject;

            ChangePosAndScale(birthUI, bossID);
        }
    }

    public static float mUnionScaleFactor = 100f;
    private void ChangePosAndScale(ActiveBirthForUI kBirthUI, int kBossID)
    {
        if (kBirthUI == null)
            return;
        float _uiScaleFactor = 1f;
        DataRecord _monsterConfig = DataCenter.mMonsterObject.GetRecord(kBossID);
        if (_monsterConfig != null)
        {
            _uiScaleFactor = _monsterConfig["UI_SCALE"];
        }
        kBirthUI.mActiveObject.mMainObject.transform.localEulerAngles = new Vector3(12, 155, 0);
        kBirthUI.mActiveObject.SetScale(mUnionScaleFactor * _uiScaleFactor);

    }

    public void setRightInfos(int index)
    {
        //name
        string name = TableCommon.GetStringFromConfig(index, "NAME", DataCenter.mGuildBoss);
        int guan = index % 1000;
        string strName = guan.InsertToString(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_NAME)) + name;
        SetText("union_pk_level_name", strName);

        //contri
        int contri = TableCommon.GetNumberFromConfig(index, "ATTACK_CONTRIBUTE", DataCenter.mGuildBoss);
		string strContri = contri.InsertToString(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_CONTRI));
        SetText("get_union_contribute_label", strContri);

        //scroll view
        //get data
        int attackGroupId = TableCommon.GetNumberFromConfig(index, "ATTACK_GROUP_ID", DataCenter.mGuildBoss);
        List<ItemDataBase> itemDataBaseList = GameCommon.GetItemGroup(attackGroupId, true);
        var pkRewardList = GetUIGridContainer("pk_rewards_list_grid");
        pkRewardList.MaxCount = itemDataBaseList.Count;

        var pkReward = pkRewardList.controlList;
        int indexTemp = 0;
        foreach (ItemDataBase item in itemDataBaseList)
        {
            GameObject board = pkReward[indexTemp];
            refreshBoard(item, board);
            indexTemp++;
        }

        //get挑战此数
        updatePkUI();

        //set挑战和领取按钮
        setGetAndChanllegeButtons();

    }

    public void setGetAndChanllegeButtons()
    {
        GameCommon.FindObject(mGameObjUI, "go_to_pk_button").SetActive(guildBossObject.monsterHealth <= 0 ? false : true);
        
        GameObject obj = GameCommon.FindObject(mGameObjUI, "go_to_get_pk_reward_button");
        obj.SetActive(guildBossObject.monsterHealth <= 0 ? true : false);
        UIImageButton button = obj.GetComponent<UIImageButton>();
        if (button != null)
            button.isEnabled = !IsGettedReward();
    }

    public void refreshBoard(ItemDataBase item, GameObject board)
    {
        GameCommon.SetItemIconNew(board, "item_icon", item.tid);
		AddButtonAction (GameCommon.FindObject (board, "item_icon"), () => GameCommon.SetItemDetailsWindow (item.tid));
        GameCommon.SetUIText(board, "rewards_name", GameCommon.GetItemName(item.tid));
        GameCommon.SetUIText(board, "item_num", item.itemNum.ToString());
    }

    public void setLeftInfos(int index)
    {
        //name
        string name = TableCommon.GetStringFromConfig(index, "NAME", DataCenter.mGuildBoss);
        SetText("union_pk_level_name_left", name);

        //exp
        int exp = TableCommon.GetNumberFromConfig(index, "GUILD_EXP", DataCenter.mGuildBoss);
		string expStr = exp.InsertToString(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_EXP));
        SetText("kill_can_get_union_exp", expStr);
        
        //progress bar
        SetProgressBar(index, mGameObjUI, "boss_hurt_rate");

        //boss model
        setBossModel(index);
    }

    public void initFirstUI(object param)
    {
        if (param is GuildBoss)
        {
            guildBossObject = (GuildBoss)param;
            initUI(guildBossObject.mid);
        }

        //scrollview resetpos
        GameCommon.FindComponent<UIScrollView>(mGameObjUI, "pk_rewards_list_scrollView").ResetPosition();


        //init stage
        int stageId = TableCommon.GetNumberFromConfig(guildBossObject.mid, "STAGE_ID", DataCenter.mGuildBoss);
        DataCenter.Set("CURRENT_STAGE", stageId);
        property = StageProperty.Create(stageId);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "PKPREPARE_INIT":
                {
                    initFirstUI(objVal);
                }
                break;
            case "REQUESTBATTLE":
                MainProcess.LoadGuildBossBattleLoadingScene();
                //RequestBattle();
                break;
            case "UPDATAUI":
                {
                    //刷新元宝
                    DataCenter.SetData("INFO_GROUP_WINDOW", "UPDATE_DIAMOND", null);

                    //刷新挑战次数
                    updatePkUI();   
                }
                break;
            case "UPDATE_BUTTONS":
                {
                    setGetAndChanllegeButtons();
                }
                break;
            default:
                break;
        }
    }

    public void updatePkUI()
    {
        //get挑战此数
        int leftNum = guildBossWarriorObject.leftBattleTimes;
        string leftNumStr = leftNum.InsertToString("{0}/5");
        SetText("leftover_times_num", leftNumStr); //
    }

    private void RequestBossBattle()
    {
        DataRecord bossData = getObject("BOSS_DATA") as DataRecord;

        if (bossData != null)
        {
            tEvent evt = Net.StartEvent("CS_RequestStartAttackBoss");
            evt.set("BOSS_ID", (int)bossData.get("BOSS_ID"));
            evt.set("BOSS_CODE", (int)bossData.get("BOSS_CODE"));
            evt.set("BOSS_FINDER", (int)get("BOSS_FINDER_DBID"));
            evt.set("BOSS_TIME", (UInt64)bossData.get("BOSS_TIME"));
            evt.DoEvent();
        }
    }

    private void RequestBattle()
    {
        if (property.stageType == STAGE_TYPE.CHAOS || property.stageType == STAGE_TYPE.GUILDBOSS)
        {
            RequestBossBattle();
        }
        else
        {
            MainProcess.RequestBattle(() => GlobalModule.DoCoroutine(DoRequestBattle()));
        }
    }

    private IEnumerator DoRequestBattle()
    {
        INetRequester req;

        if (StageProperty.IsMainStage(property.stageType))
        {
            req = new BattleMainStartRequester();
        }
        //else if (StageProperty.IsActiveStage(property.stageType))
        //{
        //    req = new BattleActiveStartRequester();
        //}
        else 
        {
            yield break;
        }

        yield return req.Start();

        if (req.success)
        {
            DataCenter.CloseWindow(UIWindowString.union_pk_prepare_window);
            MainProcess.LoadBattleLoadingScene();
        }
    }

	public override void Close ()
	{
		base.Close ();
        DataCenter.CloseWindow("INFO_GROUP_WINDOW");
        DataCenter.CloseWindow(UIWindowString.union_pk_back_window);
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
/// go_pk_rank_button
/// </summary>
public class Button_go_pk_rank_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        DataCenter.OpenWindow(UIWindowString.union_pk_rank_window);
        return true;
    }
}

/// <summary>
/// go_to_pk_button
/// </summary>
public class Button_go_to_pk_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        //挑战此数不足
        if (UnionBase.guildBossWarriorObject.leftBattleTimes <= 0)
        {
            int totalBuyTimes = UnionBase.guildBossWarriorObject.totalBuyChanllengeTimes;
            int totalBuyTimesMax = UnionBase.GetMaxPkTimes();
            if (UnionBase.guildBossWarriorObject.totalBuyChanllengeTimes < totalBuyTimesMax)
            {
                //priceStr 
                int price = UnionBase.GetBuyTimesPrice();
                string strPrice = price.ToString();//
                string strMes = TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_TIMES);
                string strBuy = string.Format(strMes, strPrice, totalBuyTimes, totalBuyTimesMax);
                //action
                Action action = () =>
                {
                    //元宝不足
                    if (RoleLogicData.Self.diamond < price)
                    {
                        GlobalModule.DoLater(() =>
                        {
                            goToRechargeWindow();
                        }, 0.01f);
                        return;
                    }
                    GuildBossNetManager.RequesGuildBossBuyBattleTimes(RoleLogicData.Self.guildId);
                };
                if (RoleLogicData.Self.diamond < price)
                {
                    DataCenter.OpenMessageOkWindow(strBuy, action, true);
                }
                else
                {
                    DataCenter.OpenMessageOkWindow(strBuy, action);
                }
            }
            else
            {
                string str = TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_NO_TIMES);
                string strMes = string.Format(str, totalBuyTimes, totalBuyTimesMax);
                if (UnionBase.guildBossWarriorObject.totalBuyChanllengeTimes == 0)
                {
                    strMes = TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_TIMES_OVER);

                }
                DataCenter.OpenMessageWindow(strMes);
            }
        }
        else
        {
            //挑战次数OK
            GuildBossNetManager.RequesGuildBossBattleStart(RoleLogicData.Self.guildId);
            DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.GUILDBOSS);
        }
        return true;
    }

    public void goToRechargeWindow()
    {
        Action action = () =>
        {
            DataCenter.CloseWindow(UIWindowString.union_pk_back_window);
            DataCenter.CloseWindow(UIWindowString.union_pk_prepare_window);
            DataCenter.OpenWindow("INFO_GROUP_WINDOW");
            GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, ()=>{
                DataCenter.OpenWindow(UIWindowString.union_pk_prepare_window);
            });
        };
        DataCenter.RefreshMessageOK(TableCommon.getStringFromStringList(STRING_INDEX.GUILD_BOSS_GUNAKA_RECHARGE), action);
    }
}

/// <summary>
/// 领取奖励
/// </summary>
public class Button_go_to_get_pk_reward_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        GuildBossNetManager.RequesGuildBossGetReward(RoleLogicData.Self.guildId);
        return true;
    }
}

/// <summary>
/// go_lineup_button
/// </summary>
public class Button_go_lineup_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        //close prepare
        DataCenter.CloseWindow(UIWindowString.union_pk_prepare_window);

        tWindow activeStageWindow = DataCenter.GetData("ACTIVE_STAGE_WINDOW") as tWindow;
        bool isActiveStageWindowOpen = activeStageWindow != null && activeStageWindow.mGameObjUI != null && activeStageWindow.mGameObjUI.activeInHierarchy;

        if (isActiveStageWindowOpen)
        {
            activeStageWindow.mGameObjUI.SetActive(false);
        }

        int currentWorldPage = ScrollWorldMapWindow.mPage;

        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);

        DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);

        MainUIScript.Self.mWindowBackAction = () => OnReturn(currentWorldPage, isActiveStageWindowOpen);
        return true;
    }

    private void OnReturn(int worldPage, bool isActiveStageWindowOpen)
    {
        if (isActiveStageWindowOpen)
        {
            tWindow activeStageWindow = DataCenter.GetData("ACTIVE_STAGE_WINDOW") as tWindow;

            if (activeStageWindow != null && activeStageWindow.mGameObjUI != null)
            {
                activeStageWindow.mGameObjUI.SetActive(true);
            }
        }

        int stageIndex = DataCenter.Get("CURRENT_STAGE");
        DataCenter.OpenWindow(UIWindowString.union_pk_prepare_window, stageIndex);
        DataCenter.OpenWindow(UIWindowString.union_pk_back_window);
    }

}

/// <summary>
/// close
/// </summary>
public class Button_rewards_infos_close_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        DataCenter.CloseWindow(UIWindowString.union_pk_prepare_window);
        DataCenter.OpenWindow(UIWindowString.union_pk_enter_window);
        DataCenter.SetData(UIWindowString.union_pk_enter_window, "UNION_PKENTER_REFRESH", null);
        return true;
    }
}

/// <summary>
/// 退出按钮
/// </summary>
class Button_union_pk_back_window_Btn : CEvent
{
    public override bool _DoEvent()
    {
        //- -
        string windowStr = DataCenter.Get("UNIONPK_CURRENT_WINDOW");
        switch (windowStr)
        {
            case UIWindowString.union_pk_enter_window:
                {
                    DataCenter.CloseWindow(UIWindowString.union_pk_enter_window);
                    DataCenter.CloseWindow(UIWindowString.union_pk_back_window);
                    DataCenter.OpenWindow(UIWindowString.union_main);
                }
                break;
            case UIWindowString.union_set_pk_aim_window:
                {
                    DataCenter.CloseWindow(UIWindowString.union_set_pk_aim_window);
                    DataCenter.OpenWindow(UIWindowString.union_pk_enter_window);
                    DataCenter.SetData(UIWindowString.union_pk_enter_window, "UNION_PKENTER_REFRESH", null);
                }
                break;
            case UIWindowString.union_pk_prepare_window:
                {
                    DataCenter.CloseWindow(UIWindowString.union_pk_prepare_window);
                    DataCenter.OpenWindow(UIWindowString.union_pk_enter_window);
                    if (DataCenter.Get("ISFROMGUILDBOSS"))
                    {
                        DataCenter.Set("ISFROMGUILDBOSS", false);
                        DataCenter.SetData(UIWindowString.union_pk_enter_window, "UNION_PKENTER_FIRSTIN", null);
                    }
                    else
                    {
                        DataCenter.SetData(UIWindowString.union_pk_enter_window, "UNION_PKENTER_REFRESH", null);
                    }
                    
                }
                break;
        }
        
        return true;
    }
}

        