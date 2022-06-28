using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class DailyStageMainWindow : DailyStageBase
{
    private StageProperty iProperty;
    public Dictionary<int, DataRecord> iDicConfig = DataCenter.mDailyStageConfig.GetAllRecord();

    public UIGridContainer iDailyPveDiffList;
    public UIGridContainer iDailyPveTypeList;

    public int iTypeIndex = 0;          //类型索引
    public int iTmpI = 0;          //难度索引---

    public int iIndex = 0;

	public override void Init ()
	{
        //入口按钮
        EventCenter.Self.RegisterEvent("Button_daily_pve_btn", new DefineFactory<Button_daily_pve_btn>());

        //出口按钮
        EventCenter.Self.RegisterEvent("Button_daily_pve_prepare_window_back_btn", new DefineFactory<Button_daily_pve_prepare_window_back_btn>());
        
        //本界面的按钮
        EventCenter.Self.RegisterEvent("Button_daily_pve_go_lineup_button", new DefineFactory<Button_daily_pve_go_lineup_button>());
        EventCenter.Self.RegisterEvent("Button_daily_pve_challenge_button", new DefineFactory<Button_daily_pve_challenge_button>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);

        //init
        iDailyPveDiffList = GetUIGridContainer("daily_pve_difficulty_list_grid");
        iDailyPveDiffList.MaxCount = 0;

        DataCenter.OpenWindow("INFO_GROUP_WINDOW");
        DataCenter.OpenWindow(UIWindowString.daily_stage_main_window_back);
        DataCenter.SetData(UIWindowString.daily_stage_main_window, "REQUEST_DAILYSTAGE_INFO", null);
	}

    //model
    public void setBossModel(int index, string point, bool pos = true)
    {
        //BOSS模型
        Debug.Log("boss model--index ==" + index);
        int stageIndex = TableCommon.GetNumberFromConfig(index, "STAGE_ID", DataCenter.mDailyStageConfig);
        DataRecord stageConfig = DataCenter.mStageTable.GetRecord(stageIndex);
        RammbockModelState tmpState = RammbockModelState.NotFight;
        string bossID = stageConfig.getData("HEADICON");

        ActiveBirthForUI birthUI_left = GameCommon.FindObject(mGameObjUI, point).GetComponent<ActiveBirthForUI>();
        if (birthUI_left != null)
            ShowModel(birthUI_left, System.Convert.ToInt32(bossID), tmpState, true);;
        var preModelL = GetSub("_role_");
        preModelL.name = "_role_" + point;

        //重命名一下
        if (pos)
        {
            if (point == "UIPoint_left")
            {
                preModelL.transform.localPosition = mShowPos;
            }
            else
            {
                preModelL.transform.localPosition = new Vector3(770.0f, -184.71f, 510.0f);
            }
        }
    }

    //refresh model
    public void RefreshModel(int index)
    {
        setBossModel(index, "UIPoint_left");
        //setBossModel(index, "UIPoint_left", false);

        //ModelTweenPosDis("UIPoint_left", index);

        //DoLater(() =>
        //{
        //    ModelTweenPosStart("UIPoint_right", index);
        //}, 0.01f);
    }

    //model tween pos消失动画
    public void ModelTweenPosDis(string point, int index)
    {
        GameObject tarGet = GetSub("_role_" + point);
        if (tarGet != null)
        {
            TweenPosition.Begin(tarGet, 0.0655f, new Vector3(-770.0f, -184.71f, 510.0f));
        }
        DoLater(() =>
        {
            if (tarGet != null)
            {
                tarGet.transform.localPosition = new Vector3(770.0f, -184.71f, 510.0f);
                tarGet.name = "_role_UIPoint_right";
            }
        }, 0.07f);
    }

    //model tween pos出现动画
    public void ModelTweenPosStart(string point, int index)
    {
        GameObject tarGet = GetSub("_role_" + point);
        if (tarGet != null)
        {
            TweenPosition.Begin(tarGet, 0.1f, mShowPos);
            
        }
        DoLater(() =>
        {
            if(tarGet != null)
            tarGet.name = "_role_UIPoint_left";
        }, 0.1f);
    }

    public static float mScaleFactor = 100f;
    private void ShowModel(ActiveBirthForUI birthUI, int bossID, RammbockModelState state, bool visible)
    {
        if (birthUI == null)
            return;

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
           
            ChangePos(birthUI, bossID);
        }

    }

    private Vector3 mShowPos = new Vector3(-200f, -122f, 510f);
    private void ChangePos(ActiveBirthForUI kBirthUI, int kBossID)
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
        kBirthUI.mActiveObject.SetScale(mScaleFactor * _uiScaleFactor);

    }
    public void initUI()
    {
        //set左边的scroll view
        setLeftScrollView();

        //初始化界面UI
        int type = GetTypeByOrderIndex(iTypeIndex);
		int index = 0;
		if (DataCenter.Get("STAGE_MAIN_SELECT_INDEX") != 0)
        {
            index = DataCenter.Get("STAGE_MAIN_SELECT_INDEX");
        }
        else 
		{ 
			index = GetTopIndexByType(type);
        }
        updateUI(index);
        DataCenter.Set("STAGE_MAIN_SELECT_INDEX", index);

        //showmodel
        setBossModel(index, "UIPoint_left");
        //setBossModel(index, "UIPoint_right");
    }

    public void InitData()
    {
        iTypeIndex = 0;
        iTmpI = 0;
        SetTypeOrderList();
    }

    public void updateUI(int index)
    {
        //初始化
        setLeftInfo(index);
        setRightInfo(index);

        //set右边的scroll view
        setRightScrollView(index);

        //added by xuke 设置队伍调整红点
        GameObject _teamAdjustBtnObj = GameCommon.FindObject(mGameObjUI, "daily_pve_go_lineup_button");
        GameCommon.SetNewMarkVisible(_teamAdjustBtnObj,TeamManager.CheckTeamHasNewMark());
        //end
    }

    public void setLeftInfo(int index)
    {
        //推荐战斗力
        int battlePoint = GetBattlePoint(index);
        if (battlePoint >= 1000000)
        {
            battlePoint = battlePoint / 10000;
            string battlePointStr = battlePoint.InsertToString(TableCommon.getStringFromStringList(STRING_INDEX.DAILY_STAGE_MAIN_BATTLE_POINT));
            GameCommon.SetUIText(GameCommon.FindObject(mGameObjUI, "left_infos"), "recommend_fighting_num", battlePointStr);
        }
        else 
        {
            string battlePointStr = battlePoint.InsertToString("{0}");
            GameCommon.SetUIText(GameCommon.FindObject(mGameObjUI, "left_infos"), "recommend_fighting_num", battlePointStr);
        }

        //难度
        int difficulty = TableCommon.GetNumberFromConfig(index, "DIFFICULTY", DataCenter.mDailyStageConfig);
        string diffiStr = GetDifficultyString(difficulty);
        SetText("daily_pve_difficulity_label", diffiStr);
        
    }

    public void setRightInfo(int index)
    {
        //体力消耗
        int cost = TableCommon.GetNumberFromConfig(index, "COST", DataCenter.mDailyStageConfig);
        string costStr = "";
        if (RoleLogicData.Self.stamina >= cost)
        {
            costStr = cost.InsertToString("[99ff66]{0}[ffffff]");
        }
        else
        {
            costStr = cost.InsertToString("[ff3333]{0}[ffffff]");
        }


        SetText("need_vitality_num", costStr);

        //文字说明--已通关xx副本
        int type = GetTypeByIndex(index);
        string typeStr = GetNameByIndex(index);
        string typeStrRet = string.Format(TableCommon.getStringFromStringList(STRING_INDEX.DAILY_STAGE_MAIN_BATTLE_CROSS), typeStr);
        SetText("daily_pve_name_label", typeStrRet);
        SetVisible("daily_pve_name_label", IsTiaoZhanEdByIndex(index) ? true : false);

        //还未挑战--次数没用完-算没挑战
        SetVisible("not_challenge_label", IsTiaoZhanEdByIndex(index) ? false : true);

        //挑战按钮是否变灰 --通关变灰-次数用完变灰
        setButtonGrey(IsTiaoZhanEdByIndex(index) ? true : false);
    }

    public void setButtonGrey(bool grey)
    {
        GameObject obj = GameCommon.FindObject(mGameObjUI, "daily_pve_challenge_button");
        UIImageButton button = obj.GetComponent<UIImageButton>();
        button.isEnabled = !grey;
    }

    public void setRightScrollView(int index)
    {
        //get data
        int type = GetTypeByIndex(index);
        int diff = GetDailyStageDiffByIndex(index);
        List<int> list = GetRightScrollViewData(type);

        //container
        iDailyPveDiffList = GetUIGridContainer("daily_pve_difficulty_list_grid");
        iDailyPveDiffList.MaxCount = list.Count;

        var pDailyPveDiff = iDailyPveDiffList.controlList;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject board = pDailyPveDiff[i];
            if (board != null)
            {
                refreshRightBoard(board, list[i], i);
            }
        }

        //reset pos 
        if (diff > 3)
        {
            GameCommon.SetScrollViewAmount(mGameObjUI, "daily_pve_difficulty_list_scrollView", 1.0f);
        }
        else
        {
            GameCommon.ResetScrollViewPosiTion(mGameObjUI, "daily_pve_difficulty_list_scrollView");
        }

        //设置默认的uitoogle
        initITmpI();
        RefreshToogel(iDailyPveDiffList, iTmpI, "item_icon_info_Btn", "Checkmark");
    }

    public void initITmpI()
    {
        int tmpI = DataCenter.Get("STAGE_MAIN_SELECT_ITMPI");
        int temp = tmpI - 1;  //赋值的时候加1了，这里要减1 -- 这是索引

        int type = GetTypeByOrderIndex(iTypeIndex);
        int index = GetTopIndexByType(type) + temp;

        if (temp > 0 && IsOpenEdByIndex(index))
        {
            iTmpI = temp;
        }
        else
        {
            iTmpI = 0;
            DataCenter.Set("STAGE_MAIN_SELECT_ITMPI", iTmpI + 1);
        }
    }

    public void refreshRightBoard(GameObject board, int index, int i)
    {
        //button
        AddButtonAction(GameCommon.FindObject(board, "item_icon_info_Btn"), () =>
        {
            int temp = index;
            int tempI = i;
            if (!IsOpenEdLevelByIndex(temp))
            {
                RefreshToogel(iDailyPveDiffList, iTmpI, "item_icon_info_Btn", "Checkmark");
                DataCenter.OnlyTipsLabelMessage(STRING_INDEX.DAILY_STAGE_MAIN_LEVEL_NOT_ENOUGH);
                return;
            }
            //设定选择的关卡
            DataCenter.Set("STAGE_MAIN_SELECT_INDEX", temp);
            iTmpI = tempI;
            DataCenter.Set("STAGE_MAIN_SELECT_ITMPI", iTmpI + 1);

            //刷新界面
            RefreshToogel(iDailyPveDiffList, tempI, "item_icon_info_Btn", "Checkmark");
            setLeftInfo(temp);
            setRightInfo(temp);
        });

        //难度
        GameCommon.SetUISprite(board, "daily_pve_difficulity_type_sprite", GetDiffIcon(index));

        //icon-掉落
        List<string> list = GetDropItem(index);
        if (list.Count >= 2)
        {
            GameCommon.SetItemIconNew(board, "item_icon", int.Parse(list[0]));
            GameCommon.SetUIText(board, "item_num", ("X" + list[1]));
        }
        

        //战斗力
        int battlePoint = GetBattlePoint(index);
        GameCommon.SetUIText(board, "recommend_fighting_num", battlePoint.ToString());
        GameCommon.FindObject(board, "recommend_fighting_num").SetActive(IsOpenEdLevelByIndex(index) ? true : false);

        //等级解锁
        GameCommon.SetUIText(board, "open_level", GetLockStrByIndex(index));
        GameCommon.FindObject(board, "open_level").SetActive(IsOpenEdLevelByIndex(index) ? false : true);
    }

    public void setLeftScrollView()
    {
        int maxTypeNum = GetMaxTypeNums();
        iDailyPveTypeList = GetUIGridContainer("daily_pve_type_grid");
        iDailyPveTypeList.MaxCount = maxTypeNum;

        var pDailyPveType = iDailyPveTypeList.controlList;
        for (int i = 0; i < maxTypeNum; i++ )
        {
            if (pDailyPveType[i] != null)
            {
                GameObject board = pDailyPveType[i];
                int type = GetTypeByOrderIndex(i);
                refreshLeftBoard(board, type);
            }
        }

        //初始化checkMark
        //设置默认的uitoogle
        initItype();
        RefreshToogel(iDailyPveTypeList, iTypeIndex, "daily_pve_type_btn", "Checkmark");
    }

    public void initItype()
    {
        int typeIndex = DataCenter.Get("STAGE_MAIN_SELECT_TYPE_INDEX");
        int temp = typeIndex - 1;       //索引 因为赋值的时候加1了 这里减1
        int type = GetTypeByOrderIndex(temp); 
        if (temp > 0 && IsOpenEdByType(type))
        {
            iTypeIndex = temp;
        }
        else
        {
            iTypeIndex = GetOpenEdType();
            DataCenter.Set("STAGE_MAIN_SELECT_TYPE_INDEX", iTypeIndex + 1);
        }
    }

    public bool IsTheSameReturn(int i)
    {
        bool ret = false;
        if (iTypeIndex == GetOrderIndexByType(i))
        {
            ret = true;
        }
        return ret;
    }

    public void refreshLeftBoard(GameObject board, int i)
    {
        //button
        AddButtonAction(GameCommon.FindObject(board, "daily_pve_type_btn"), () =>
        {
            int tmep = i;
            if (IsTheSameReturn(tmep))
            {
                return;
            }
            
            //先时间后等级
            if (!IsOpenEdTimeByType(tmep))
            {
                //保持老状态
                GlobalModule.DoLater(() =>
                {
                    RefreshToogel(iDailyPveTypeList, iTypeIndex, "daily_pve_type_btn", "Checkmark", true);
                }, 0.2f);

                DataCenter.OnlyTipsLabelMessage(STRING_INDEX.DAILY_STAGE_MAIN_TIME_NOT_REACH);
                return;
            }
            if (!IsOpenEdLevelByType(tmep))
            {
                //保持老状态
                GlobalModule.DoLater(() =>
                {
                    RefreshToogel(iDailyPveTypeList, iTypeIndex, "daily_pve_type_btn", "Checkmark", true);
                }, 0.2f);

                DataCenter.OnlyTipsLabelMessage(STRING_INDEX.DAILY_STAGE_MAIN_LEVEL_NOT_ENOUGH);
                return;
            }

            //数据刷新
            int index = GetTopIndexByType(tmep) + iTmpI;
            iIndex = 0;
            index = GetExitIndex(index, tmep);
            
            DataCenter.Set("STAGE_MAIN_SELECT_INDEX", index);
            iTypeIndex = GetOrderIndexByType(i);
            DataCenter.Set("STAGE_MAIN_SELECT_TYPE_INDEX", iTypeIndex + 1);

            //界面刷新
            RefreshToogel(iDailyPveTypeList, iTypeIndex, "daily_pve_type_btn", "Checkmark"); 
            updateUI(index);

            //refreshModel
            RefreshModel(index);
        });

        //setcheckmark
        GameCommon.FindObject(board, "Checkmark").SetActive(true);
        GameCommon.SetUISprite(board, "Checkmark", GetIconByNum(i, DAILY_STAGE_STATE.DAILY_STAGE_DOWN));
        GameCommon.FindObject(board, "Checkmark").SetActive(false);

        //开放等级-副本名字-开放时间描述
        GameCommon.SetUIText(board, "daily_pve_type_name", GetNameByType(i));

        // --包含了排序-时间优先-等级也有排序
        SetButtonAndText(board, i);
    }

    public int GetExitIndex(int index, int type)
    {
        if (GetTypeByIndex(index) == type && IsOpenEdByIndex(index) )
        {
            iTmpI = iTmpI - iIndex;
            DataCenter.Set("STAGE_MAIN_SELECT_ITMPI", iTmpI + 1);
            return index;
        }
        iIndex++;
        if (iIndex == 5)
        {
            return index;
        }
        return GetExitIndex(index-1, type);
    }

    public void SetButtonAndText(GameObject board, int i) 
    {
        //按钮三态 开启条件-OPEN_LEVEL-OPEN_DAY
        if (!IsOpenEdTimeByType(i))
        {
            //等级限制隐藏
            GameCommon.FindObject(board, "daily_pve_type_lock").SetActive(false);

            //时间未到
            GameCommon.SetUIText(board, "daily_pve_type_open_time", GetTimeDesc(i));
            GameCommon.FindObject(board, "daily_pve_type_open_time").SetActive(true);

            //按钮状态
            GameCommon.SetUISprite(board, "item_icon", GetIconByNum(i, DAILY_STAGE_STATE.DAILY_STAGE_DIS));
        }
        else if (!IsOpenEdLevelByType(i))
        {
            //时间隐藏
            GameCommon.FindObject(board, "daily_pve_type_open_time").SetActive(false);

            //等级限制提示
            GameCommon.SetUIText(board, "daily_pve_type_lock", GetLockStrByType(i));
            GameCommon.FindObject(board, "daily_pve_type_lock").SetActive(true);

            //按钮状态
            GameCommon.SetUISprite(board, "item_icon", GetIconByNum(i, DAILY_STAGE_STATE.DAILY_STAGE_DIS));
        }
        else
        {
            //等级限制提示
            GameCommon.FindObject(board, "daily_pve_type_lock").SetActive(false);

            //时间未到
            GameCommon.FindObject(board, "daily_pve_type_open_time").SetActive(false);

            //按钮状态
            GameCommon.SetUISprite(board, "item_icon", GetIconByNum(i, DAILY_STAGE_STATE.DAILY_STAGE_NORMAL));
        }
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "REQUEST_DAILYSTAGE_INFO":
                {
                    DailyStageNetManager.RequestDailyStageInfo();
                }
                break;
            case "DAILYSTAGE_INIT":
                {
                    //init ui
                    InitData();
                    initUI();
                }
                break;
            case "REQUEST_BATTLE":
                {
                    //stageIndex
                    int index = DataCenter.Get("STAGE_MAIN_SELECT_INDEX");
                    int stageIndex = TableCommon.GetNumberFromConfig(index, "STAGE_ID", DataCenter.mDailyStageConfig);

                    DataCenter.Set("CURRENT_STAGE", stageIndex);
                    iProperty = StageProperty.Create(stageIndex);
                    RequestBattle();
                }
                break;
        }
    }

    private void RequestBattle()
    {
        if (iProperty.stageType == STAGE_TYPE.CHAOS)
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

        if (StageProperty.IsMainStage(iProperty.stageType))
        {
            req = new BattleMainStartRequester();
        }
        else if (StageProperty.IsActiveStage(iProperty.stageType))
        {
            req = new DailyStageNetManager.DailyStageBattleStartRequester();
        }
        else
        {
            yield break;
        }

        yield return req.Start();

        if (req.success)
        {
            DataCenter.CloseWindow("STAGE_INFO_WINDOW");
            MainProcess.LoadBattleLoadingScene();
        }
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
/// 布阵
/// </summary>
public class Button_daily_pve_go_lineup_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        //close prepare
        DataCenter.Set("DAILAY_STAGE_GOTO_LINEUP", true);
        DataCenter.CloseWindow(UIWindowString.daily_stage_main_window);
        DataCenter.CloseWindow(UIWindowString.daily_stage_main_window_back);

        tWindow activeStageWindow = DataCenter.GetData("ACTIVE_STAGE_WINDOW") as tWindow;
        bool isActiveStageWindowOpen = activeStageWindow != null && activeStageWindow.mGameObjUI != null && activeStageWindow.mGameObjUI.activeInHierarchy;

        if (isActiveStageWindowOpen)
        {
            activeStageWindow.mGameObjUI.SetActive(false);
        }

        int currentWorldPage = ScrollWorldMapWindow.mPage;

        DataCenter.Set("NO_PAOPAO_MAINUI", false);//set no paopao

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
        DataCenter.OpenWindow(UIWindowString.daily_stage_main_window, stageIndex);
        DataCenter.OpenWindow(UIWindowString.daily_stage_main_window_back);
    }
}

/// <summary>
/// 挑战
/// </summary>
public class Button_daily_pve_challenge_button : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        int index = DataCenter.Get("STAGE_MAIN_SELECT_INDEX");
        if (!DailyStageBase.IsOpenEdTimeByIndex(index))
        {
            DataCenter.OnlyTipsLabelMessage(STRING_INDEX.DAILY_STAGE_MAIN_TIME_NOT_REACH);
            return true;
        }
        if (!DailyStageBase.IsOpenEdLevelByIndex(index))
        {
            DataCenter.OnlyTipsLabelMessage(STRING_INDEX.DAILY_STAGE_MAIN_LEVEL_NOT_ENOUGH);
            return true;
        }
        if (DailyStageBase.IsTiaoZhanEdByIndex(index))
        {
            DataCenter.OnlyTipsLabelMessage(STRING_INDEX.DAILY_STAGE_MAIN_TIMES_NOT_ENOUGH);
            return true;
        }

        string reward = DataCenter.mDailyStageConfig.GetData(index, "DROP_ITEM").ToString();
        List<ItemDataBase> tmpItems = GameCommon.ParseItemList(reward);
        List<PACKAGE_TYPE> tmpTypes = PackageManager.GetPackageTypes(tmpItems);
        if (!CheckPackage.Instance.CanEnterBattle(tmpTypes))
        {
            return true;
        }

        DataCenter.SetData(UIWindowString.daily_stage_main_window, "REQUEST_BATTLE", null);
        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.DAILYSTAGE);
        return true;
    }
}

public class Button_daily_pve_btn : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_WINDOW");
        DataCenter.OpenWindow(UIWindowString.daily_stage_main_window);
        return true;
    }
}

/// <summary>
/// 上一级
/// </summary>
public class Button_daily_pve_prepare_window_back_btn : CEvent
{
    public override bool _DoEvent()
    {
        //TODO
        DataCenter.CloseWindow(UIWindowString.daily_stage_main_window);
        DataCenter.CloseWindow(UIWindowString.daily_stage_main_window_back);

        //open冒险界面
        if (DataCenter.Get("DAILAY_STAGE_GOTO_LINEUP"))
        {
            DataCenter.Set("DAILAY_STAGE_GOTO_LINEUP", false);
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
            
            DataCenter.SetData("INFO_GROUP_WINDOW", "PRE_WIN", 2);
        }
        else
        {
            ScrollWorldMapWindow.mPointIndex = 0;

            if (GameCommon.bIsLogicDataExist("SCROLL_WORLD_MAP_WINDOW"))
                DataCenter.OpenWindow("SCROLL_WORLD_MAP_WINDOW");
            else
                MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
        }
        return true;
    }
}

