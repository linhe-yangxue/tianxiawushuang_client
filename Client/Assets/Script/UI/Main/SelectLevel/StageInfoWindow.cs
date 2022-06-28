using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logic;
using DataTable;


public class StageInfoWindow : tWindow
{
    private StageProperty property;
    public static int multi_sweep_times=0;
	private TM_TweenScale scaleTweener;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_stage_info_back", new DefineFactory<Button_stage_info_back>());
        EventCenter.Self.RegisterEvent("Button_stage_info_start", new DefineFactory<Button_stage_info_start>());
        EventCenter.Self.RegisterEvent("Button_stage_info_clean", new DefineFactory<Button_stage_info_clean>());
        EventCenter.Self.RegisterEvent("Button_adjust_team_button", new DefineFactory<Button_adjust_team_button>());
		//added by xuke begin
		EventCenter.Self.RegisterEvent("Button_stage_info_start_new_1", new DefineFactory<Button_stage_info_start>());
		EventCenter.Self.RegisterEvent ("Button_saodang_one_time",new DefineFactory<Button_stage_info_clean>());
        EventCenter.Self.RegisterEvent("Button_saodang_multi_times", new DefineFactory<Button_stage_info_clean_multi>());
        EventCenter.Self.RegisterEvent("Button_reset_times_btn",new DefineFactory<Button_reset_times_btn>());
		//end
    }

    public override void Open(object param)
    {
        base.Open(param);

        int stageIndex = new Data(param);

        if (stageIndex > 0)
        {
            DataCenter.Set("CURRENT_STAGE", stageIndex);
            property = StageProperty.Create(stageIndex);
            Refresh(null);
        }
    }

	private void StartOpenTween()
	{
		StopOpenTween();
		
		scaleTweener = EventCenter.Start("TM_TweenScale") as TM_TweenScale;
		scaleTweener.mTarget = mGameObjUI;
		scaleTweener.mBefore = ScrollWorldMapWindow.openEffDelay;
		scaleTweener.mDuration = ScrollWorldMapWindow.openEffDuration;
		scaleTweener.mFrom = Vector3.one * 1.2f;
		scaleTweener.mTo = Vector3.one;
		scaleTweener.DoEvent();
	}

	private void StopOpenTween()
	{
		if (scaleTweener != null && !scaleTweener.GetFinished())
		{
			scaleTweener.Finish();
			scaleTweener = null;
			mGameObjUI.transform.localScale = Vector3.one;
		}
	}

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REQUEST_BATTLE":
                RequestBattle();
                break;
            case "REFRESH_RESET_INFO":
                RefreshNormalInfo();
                break;
        }
    }

    public override bool Refresh(object param)
    {
        RefreshLevelInfo();
        //RefreshPlayerInfo();
        //RefreshTeamInfo();
        RefreshDropInfo();
		//SetCleanNum ();
		RefreshPowerInfo ();
        if (StageProperty.IsMainStage(property.stageType))
        {
            RefreshNormalInfo();
        }
        else if (StageProperty.IsActiveStage(property.stageType))
        {
            if (!GameCommon.bIsWindowOpen("ACTIVE_STAGE_WINDOW"))
                DataCenter.OpenWindow("ACTIVE_STAGE_WINDOW");

            RefreshActiveInfo();
        }
        else if (property.stageType == STAGE_TYPE.CHAOS)
        {
            if (!GameCommon.bIsWindowOpen("BOSS_RAID_WINDOW"))
                DataCenter.OpenWindow("BOSS_RAID_WINDOW");

            RefreshBossInfo();
        }

        return true;
    }

    private void RefreshLevelInfo()
    {
        GameObject titleObj = GetSub("stage_title");

        string stageNumber = "";
        string difficulty = "";

        if (StageProperty.IsMainStage(property.stageType))
        {
            //GameCommon.SetUIVisiable(titleObj, "element", true);
            //GameCommon.SetElementIcon(titleObj, "element", (int)property.stageElement);
            stageNumber = TableCommon.GetStringFromStageConfig(property.mIndex, "STAGENUMBER");

            switch (ScrollWorldMapWindow.mDifficulty)
            {
                case 1:
                    difficulty = "[FFFFFF]轻松难度[-]";
                    break;
                case 2:
                    difficulty = "[FFA92E]普通难度[-]";
                    break;
                case 3:
                    difficulty = "[FF0000]困难难度[-]";
                    break;
            }
        }
        //else
        //{
        //    GameCommon.SetUIVisiable(titleObj, "element", false);
        //}
                    
        string stageName = TableCommon.GetStringFromStageConfig(property.mIndex, "NAME");
		//int _numStrIndex = stageName.IndexOfAny (new char[]{'0','1','2','3','4','5','6','7','8','9'});

        //if (_numStrIndex >= 0)
        //{
            //string _stageNameWithoutNumber = stageName.Substring(0, _numStrIndex);
            string title = stageNumber + "  " + stageName + "  " + difficulty;
            UILabel nameLabel = GameCommon.FindComponent<UILabel>(titleObj, "stage_name");
            nameLabel.text = title;
            int width = nameLabel.width;

            if (StageProperty.IsMainStage(property.stageType))
            {
                GameObject elementObj = GameCommon.FindObject(titleObj, "element");
                nameLabel.transform.localPosition = new Vector3(10, 0, 0);
                elementObj.transform.localPosition = new Vector3(-width / 2 - 10, 0, 0);
                titleObj.GetComponent<UISprite>().width = width + 100;
            }
            else
            {
                nameLabel.transform.localPosition = new Vector3(0, 0, 0);
                titleObj.GetComponent<UISprite>().width = width + 40;
            }
        //}
        //SetText("my_fight_strength_number", GameCommon.GetTotalFightingStrength().ToString());
    }

	// 刷新我方战斗力信息
	private void RefreshPowerInfo()
	{
		GameObject strengthObj = GetSub ("my_fight_strength");
		GameCommon.SetUIText (strengthObj,"name_label",GameCommon.ShowNumUI(System.Convert.ToInt32(GameCommon.GetPower().ToString("f0"))));
	}


    private void RefreshPlayerInfo()
    {
        GameObject playerInfoObj = GetSub("player_info");
        RoleData d = RoleLogicData.GetMainRole();

        string roleName = TableCommon.GetStringFromActiveCongfig(d.tid, "NAME");
        GameCommon.SetUIText(playerInfoObj, "player_name", roleName);

        DataRecord record = DataCenter.mRoleSkinConfig.GetRecord(d.tid);
        if (record != null)
        {
            string roleHeadAtlas = record.getData("ICON_ATLAS");
            string roleHeadSprite = record.getData("ICON_SPRITE");
            GameCommon.SetIcon(playerInfoObj, "player_head", roleHeadSprite, roleHeadAtlas);
        }

        int baseHp = GameCommon.GetBaseMaxHP(d.tid, d.level, d.breakLevel);
        int baseMp = GameCommon.GetBaseMaxMP(d.tid, d.level, d.breakLevel);
        int baseAttack = (int)GameCommon.GetBaseAttack(d.tid, d.level, d.breakLevel);
        int addHp = GameCommon.GetBaseMaxHP(d.tid, d.level, d.breakLevel) - baseHp;
        int addMp = GameCommon.GetBaseMaxMP(d.tid, d.level, d.breakLevel) - baseMp;
        int addAttack = (int)GameCommon.GetBaseAttack(d.tid, d.level, d.breakLevel) - baseAttack;
        GameCommon.SetUIText(playerInfoObj, "life_num", CombineAttributeText(baseHp, addHp));
        GameCommon.SetUIText(playerInfoObj, "magic_num", CombineAttributeText(baseMp, addMp));
        GameCommon.SetUIText(playerInfoObj, "damage_num", CombineAttributeText(baseAttack, addAttack));

        GameObject levelobj = GetSub("level_Info");
        GameCommon.SetUIText(levelobj, "player_level", "Lv." + d.level);
        int levelUpExp = TableManager.GetData("CharacterLevelExp", d.level, "LEVEL_EXP");
        float exp = d.exp;
        float expRate = exp / levelUpExp;
        GameCommon.FindComponent<UIProgressBar>(levelobj, "bar").value = expRate;

        RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
        EquipData curUseEquip = roleEquipLogicData.GetUseEquip();

        if (curUseEquip != null)
        {
            GameCommon.SetUIVisiable(playerInfoObj, "attribute_icon", true);
            GameCommon.SetUIVisiable(playerInfoObj, "none_label", false);
            int iElementIndex = (int)curUseEquip.mElementType;
            GameCommon.SetElementIcon(playerInfoObj, "attribute_icon", iElementIndex);
        }
        else
        {
            GameCommon.SetUIVisiable(playerInfoObj, "attribute_icon", false);
            GameCommon.SetUIVisiable(playerInfoObj, "none_label", true);
        }

		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "fa_bao_button", UNLOCK_FUNCTION_TYPE.FA_BAO);
    }

    private void RefreshTeamInfo()
    {
        DataCenter.SetData("SELECT_LEVEL_TEAM_INFO_WINDOW", "TEAM_REFRESH", null);
    }

    private void RefreshNormalInfo()
    {
        ShowMonsterInfo(true);
        ShowDropInfo(true);
        ShowStageInfo(true);
        ShowDropDesc(false);
		//added by xuke
        ShowStarInfo(false);
		ShowNormalStarNumInfo ();
		ShowNormalDifficultyInfo ();
		ShowLevelReward ();
		ShowSaoDao ();
        RefreshLeftTimesInfo();
		//end
        ShowStageImage(false);
        ShowBossInfo(false);
        ShowBossDrop(false);
        ShowBossModel(false);

		//added by xuke begin
        if (property.passed && property.mBestStar == 3)
        {

			//SetVisible("clean_number", true);
			SetVisible("clean_number", false);
			SetCleanNum();
            //SetVisible("stage_info_clean", true);
			SetVisible("stage_info_clean", false);
			//GameCommon.ButtonEnbleButCanClick (mGameObjUI, "stage_info_clean", UNLOCK_FUNCTION_TYPE.SWEEP);
            SetVisible("clean_tip", false);
        }
        else
        {
            SetVisible("clean_number", false);
            SetVisible("stage_info_clean", false);
			SetVisible("stage_info_clean_gray", false);
            //SetVisible("clean_tip", true);
			SetVisible("clean_tip", false);
        }

		//end     

        SetStaminaCost(StageProperty.GetCurrentStageStaminaCost()/*CommonParam.battleMainStaminaCost*/);
        //added by xuke 调整队伍红点相关
        CheckTeamAdjust_NewMark();
        //end
    }

    /// <summary>
    /// 检测调整队伍红点
    /// </summary>
    private void CheckTeamAdjust_NewMark()
    {
        GameObject _adjustTeamBtnObj = GameCommon.FindObject(mGameObjUI, "adjust_team_button");
        GameCommon.SetNewMarkVisible(_adjustTeamBtnObj, TeamManager.CheckTeamHasNewMark());
    }

	// 设置是否显示扫荡
	private void ShowSaoDao()
	{
		GameObject _saoDaoRoot = GameCommon.FindObject (mGameObjUI,"saodao_info");
		_saoDaoRoot.SetActive (property.mBestStar == 3);
	}

    // 刷新关卡剩余挑战次数
    private void RefreshLeftTimesInfo()
    {
        MapData _mapData = MapLogicData.Instance.GetMapDataByStageIndex(property.mIndex);
        //最大挑战次数
        int _maxChallengeNum = TableCommon.GetNumberFromStageConfig(property.mIndex, "CHALLENGE_NUM");
        //今日剩余挑战次数
        int _todayLeftTimes = _maxChallengeNum - _mapData.todaySuccess;
        GameObject _leftInfoRoot = GameCommon.FindObject(mGameObjUI, "left_times_info_root");
        UILabel _leftLbl = GameCommon.FindComponent<UILabel>(_leftInfoRoot, "left_num");
        string _colorPrefix = "[ffffff]";//_leftLbl.GetColorPrefixByColor();
        int left_sweep_times=0;
        if (RoleLogicData.Self.stamina / 5 != 0)
        {
            left_sweep_times = Math.Min(RoleLogicData.Self.stamina / 5, _todayLeftTimes);
        }
        else
        {
            left_sweep_times = Math.Min(10, _todayLeftTimes);
        }
        if (left_sweep_times > 10) { left_sweep_times = 10; }
        multi_sweep_times = left_sweep_times;
        GameObject multibutton=GameCommon.FindObject(mGameObjUI, "saodang_multi_times");
        GameObject onebutton = GameCommon.FindObject(mGameObjUI, "saodang_one_time");
        GameCommon.FindObject(multibutton, "label").GetComponent<UILabel>().text = "扫荡" + multi_sweep_times.ToString() + "次";

        GameObject _resetBtnObj = GameCommon.FindObject(_leftInfoRoot, "reset_times_btn");
        UIImageButton _resetImageBtn = _resetBtnObj.GetComponent<UIImageButton>();

        NiceData _btnData = GameCommon.GetButtonData(_leftInfoRoot, "reset_times_btn");
        _btnData.set("RESET_TIMES",_mapData.todayReset);
        if (_todayLeftTimes <= 0)
        {
            _colorPrefix = "[ff3333]";
            //_resetImageBtn.isEnabled = true;
            _resetImageBtn.gameObject.SetActive(true);
             multibutton.gameObject.SetActive(false);
             onebutton.gameObject.SetActive(false);
             DataCenter.Set("CURRENT_STAGE_ID",property.mIndex);
        }
        else 
        {
            //_resetImageBtn.isEnabled = false;
            _resetImageBtn.gameObject.SetActive(false);
            multibutton.gameObject.SetActive(true);
            onebutton.gameObject.SetActive(true);
        }
        _leftLbl.text = _colorPrefix + _todayLeftTimes.ToString() + "[-]/" + _maxChallengeNum.ToString();

        //给扫荡按钮和开始降妖按钮设置当前关卡ID
        NiceData _saoDangOneTimeBtnData = GameCommon.GetButtonData(mGameObjUI, "saodang_one_time");
        _saoDangOneTimeBtnData.set("STAGE_ID",property.mIndex);
        NiceData _saoDangTenTimesBtnData = GameCommon.GetButtonData(mGameObjUI, "saodang_multi_times");
        _saoDangTenTimesBtnData.set("STAGE_ID",property.mIndex);
        NiceData _startXiangYaoBtnData = GameCommon.GetButtonData(mGameObjUI, "stage_info_start_new_1");
        _startXiangYaoBtnData.set("STAGE_ID",property.mIndex);
    }

    // 设置体力消耗
    private void SetStaminaCost(int cost)
    {
        GameObject startBtn = GameCommon.FindObject(mGameObjUI, "stage_info_start_new_1");

        if (startBtn != null)
        {
            GameCommon.SetUIText(startBtn, "consume_num", cost.ToString());
        }
    }

	// 显示关卡奖励
	private void ShowLevelReward()
	{
		DataRecord _stageRecord = DataCenter.mStageTable.GetRecord (property.mIndex);
		//1.经验奖励
        int _exp_reward = GameCommon.GetMainStageExp(RoleLogicData.GetMainRole().level); //StageProperty.GetStageStaminaCost(property.mIndex) * 10;
		//2.金币奖励
        int _gold_reward = GameCommon.GetMainStageGold(RoleLogicData.GetMainRole().level); //_stageRecord["BOSS_MONEY"];

		GameObject _dropInfoRoot = GameCommon.FindObject (mGameObjUI,"drop_info");
		UILabel _exp_reward_lbl = GameCommon.FindObject (_dropInfoRoot,"reward_exp_num").GetComponent<UILabel>();
		UILabel _gold_reward_lbl = GameCommon.FindObject (_dropInfoRoot,"reward_gold_num").GetComponent<UILabel>();

		_exp_reward_lbl.text = _exp_reward.ToString();
		_gold_reward_lbl.text = _gold_reward.ToString();
	}

	// 显示普通关卡当前获得的星星数量
	private void ShowNormalStarNumInfo()
	{
		GameObject _starInfoRoot = GameCommon.FindObject (mGameObjUI,"star_info");
		UIGridContainer _gridContainer = GameCommon.FindObject(_starInfoRoot,"grid").GetComponent<UIGridContainer>();
		_gridContainer.MaxCount = 3;

		//得到当前关卡已经获得的星星数量
		for (int i = 0; i < 3; i++) 
		{
			GameCommon.FindObject(_gridContainer.controlList[i],"star").SetActive(i < property.mBestStar);
		}
	}

	// 设置关卡的难度信息
	private void ShowNormalDifficultyInfo()
	{
		GameObject _difficultyInfoRoot = GameCommon.FindObject (mGameObjUI,"difficulty_info");
		float _myPower = GameCommon.GetPower ();
		DataRecord _stageRecord = DataCenter.mStageTable.GetRecord (property.mIndex);
		int _recommendPower = _stageRecord["RENFERENCE_BATTLE"];
//		DEBUG.LogError ("推荐战斗力是: " + _recommendPower);

		GameObject _recommendPowerRoot = GameCommon.FindObject (mGameObjUI,"recommend_fight_strength");
		UILabel _recommendPowerLbl = GameCommon.FindObject (_recommendPowerRoot,"name_label").GetComponent<UILabel>();
		_recommendPowerLbl.text = GameCommon.ShowNumUI(_recommendPower);
		UISlider _slider = GameCommon.FindObject (_difficultyInfoRoot,"Slider").GetComponent<UISlider>();
		float _powerRate = 0f;
		if (_recommendPower != 0) {
			_powerRate = _myPower / _recommendPower;
		}else {
			_powerRate = 1f;
		}
		//SetSliderState (_slider,_powerRate);
		SetSliderState (_slider,_powerRate);
	}

	private class SliderStateInfo
	{
		public string mForePicName;		//> 前景图片名称
		public string mDifficultyName;	//> 难度字段
		public Color mColor;			//> 字段显示颜色
	}

	// 设置滑动条状态
	private void SetSliderState(UISlider kSlider,float kPowerRate)
	{
		kSlider.value = kPowerRate;
		SliderStateInfo _sliderStateInfo = GetSliderStateInfo (kPowerRate);
		UISprite _foreGroundSprite = GameCommon.FindObject (kSlider.gameObject,"slider_foreground").GetComponent<UISprite>();
		_foreGroundSprite.spriteName = _sliderStateInfo.mForePicName;
		UILabel _difficultyLbl = GameCommon.FindObject (kSlider.gameObject,"difficulty_label").GetComponent<UILabel>();
		_difficultyLbl.text = _sliderStateInfo.mDifficultyName;
		_difficultyLbl.color = _sliderStateInfo.mColor;

	}

	private SliderStateInfo GetSliderStateInfo(float kPowerRate)
	{
		Dictionary<float,SliderStateInfo> _sliderStateDic = new Dictionary<float, SliderStateInfo> ();
		_sliderStateDic.Add (0.6f,new SliderStateInfo(){mForePicName = "a_ui_nandutiao_1",mDifficultyName = "极度凶险",mColor = new Color(255f/255f,51f/255f,51f/255f,1f)});
		_sliderStateDic.Add (0.8f,new SliderStateInfo(){mForePicName = "a_ui_nandutiao_2",mDifficultyName = "凶险",mColor = new Color(255f/255f,152f/255f,76f/255f,1f)});
		_sliderStateDic.Add (1.0f,new SliderStateInfo(){mForePicName = "a_ui_nandutiao_3",mDifficultyName = "略难",mColor = new Color(242f/255f,236f/255f,65f/255f,1f)});
		_sliderStateDic.Add (2f,new SliderStateInfo(){mForePicName = "a_ui_nandutiao_4",mDifficultyName = "简单",mColor = new Color(45f/255f,227f/255f,45f/255f,1f)});
		foreach (KeyValuePair<float,SliderStateInfo> pair in _sliderStateDic) 
		{
			if(kPowerRate <= pair.Key)
			{
				return pair.Value;
			}
		}
		return _sliderStateDic[2f];
	}

	private void SetCleanNum()
	{
        int sweepNum = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.SAODANG_POINT).itemNum;
        SetText("clean_num", sweepNum.ToString());
	}

    private void RefreshActiveInfo()
    {       
        DataRecord record = GetActiveStageRecord(property.mIndex);

        if (record == null || record.get("STAGETYPE") == 2)
        {
            ShowMonsterInfo(true);
            ShowDropInfo(true);
            ShowStageInfo(true);
            ShowDropDesc(true);
            ShowStarInfo(false);
            ShowStageImage(false);
        }
        else
        {
            ShowMonsterInfo(false);
            ShowStageInfo(false);
            ShowDropDesc(false);
            ShowStarInfo(false);
            ShowDropInfo(true);
            ShowStageImage(true);
        }

        ShowBossInfo(false);
        ShowBossDrop(false);
        ShowBossModel(false);
        SetVisible("clean_number", false);
        SetVisible("stage_info_clean", false);
		SetVisible("stage_info_clean_gray", false);
        SetVisible("clean_tip", false);

        SetStaminaCost(CommonParam.battleActiveStaminaCost);
    }

    private void RefreshBossInfo()
    {
        ShowMonsterInfo(false);
        ShowDropInfo(false);
        ShowStageInfo(false);
        ShowDropDesc(false);
        ShowStarInfo(false);
        ShowStageImage(false);
        ShowBossInfo(true);
        ShowBossDrop(true);
        ShowBossModel(true);
        SetVisible("clean_number", false);
        SetVisible("stage_info_clean", false);
		SetVisible("stage_info_clean_gray", false);
        SetVisible("clean_tip", false);
    }

    private void RefreshDropInfo()
    {
        GameObject obj = GetSub("drop_info");
        obj.SetActive(true);
        GameObject dropObj = GameCommon.FindObject(obj, "grid_drop");

        //if (property.stageType == STAGE_TYPE.CHAOS)
        //{
        //    DataRecord bossData = getObject("BOSS_DATA") as DataRecord;

        //    if (bossData != null)
        //    {
        //        int bossID = bossData.get("BOSS_ID");
        //        int groupID = TableCommon.GetNumberFromBossConfig(bossID, "DROP_GROUP");
        //        FillDropGrid(dropObj, groupID, 8);
        //    }
        //}
        //else
        //{
        //    int groupId = TableCommon.GetNumberFromStageConfig(property.mIndex, "DROPGROUPID");
        //    FillDropGrid(dropObj, groupId, 8);
        //}
        FillDropGridNew(dropObj,property.mIndex);
    }

    private void ShowDropItemInfo(GameObject kGridDrop,int kStageIndex) 
    {
        if(kGridDrop == null)
            return;
        UIGridContainer _gridContainer = kGridDrop.GetComponent<UIGridContainer>();
        if (_gridContainer == null)
            return;
        string _dropInfo = TableCommon.GetStringFromStageConfig(kStageIndex, "LOOT_PREVEIW");
        List<ItemDataBase> _itemList = GameCommon.ParseItemList(_dropInfo);
        if (_itemList == null)
            return;
        _gridContainer.MaxCount = _itemList.Count;
        for (int i = 0, count = _itemList.Count; i < count; ++i) 
        {
            GameObject _curItem = _gridContainer.controlList[i];
            GameCommon.SetItemIconWithNum(_curItem, _itemList[i], "item_icon", "count_label");
            int _tid = _itemList[i].tid;
            AddButtonAction(GameCommon.FindObject(_curItem, "item_icon"), () => GameCommon.SetAccountItemDetailsWindow(_tid));
        }
    }


    private string CombineAttributeText(int baseValue, int addValue)
    {
        if (addValue == 0)
            return baseValue.ToString();
        else
            return baseValue.ToString() + " [00FF00]+" + addValue.ToString() + "[-]";
    }

    private void ShowMonsterInfo(bool visible)
    {
        GameObject obj = GetSub("monster_info");

        if (!visible)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);
        GameObject gridObj = GameCommon.FindObject(obj, "grid_monster");
        UIGridContainer grid = gridObj.GetComponent<UIGridContainer>();
        List<DataRecord> monsterList = GetMonsterList(property.mIndex);
        GameObject bossObj = GameCommon.FindObject(obj, "boss_pet_info");
        List<DataRecord> monsterListTemp = new List<DataRecord>();
        if (monsterList.Count >= 1)
        {
            grid.MaxCount = monsterList.Count - 1;

            for (int i = 0; i < monsterList.Count - 1; ++i)
            {
                monsterListTemp.Add(monsterList[i]);
            }
            monsterListTemp.Sort((DataRecord a, DataRecord b) =>
            {
                int ta=(int)a.getData("STAR_LEVEL");
                int tb=(int)b.getData("STAR_LEVEL");
                if (ta != tb)
                {
                    return tb - ta;
                }
                else
                    return 0;
            });
            for (int i = 0; i < monsterListTemp.Count; i++)
            {
                GameCommon.SetMonsterIcon(grid.controlList[i], (int)monsterListTemp[i].getData("INDEX"));
                int _tid = monsterListTemp[i].getData("FULING_ID");
                AddButtonAction(GameCommon.FindObject(grid.controlList[i], "item_icon"), () => GameCommon.SetAccountItemDetailsWindow(_tid));
            }
                bossObj.SetActive(true);
            GameCommon.SetMonsterIcon(bossObj, (int)monsterList[monsterList.Count - 1].getData("INDEX"));
            AddButtonAction(GameCommon.FindObject(bossObj, "item_icon"), () => GameCommon.SetAccountItemDetailsWindow(monsterList[monsterList.Count - 1].getData("FULING_ID")));
        }
        else
        {
            grid.MaxCount = 0;
            bossObj.SetActive(false);
        }
    }

    private void ShowStageInfo(bool visible)
    {
        GameObject obj = GetSub("level_introduces");

        if (!visible)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);
        string info = TableCommon.GetStringFromStageConfig(property.mIndex, "DESC");
        GameCommon.SetUIText(obj, "level_introduces_label", info);
    }

    private void ShowDropDesc(bool visible)
    {
        GameObject obj = GetSub("drop_introduces");

        if (!visible)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);
        GameCommon.SetUIText(obj, "drop_introduces_label", TableCommon.GetStringFromStageConfig(property.mIndex, "DESC_MJ"));
    }

    private void ShowStarInfo(bool visible)
    {
        GameObject obj = GetSub("three_star_game_condition");

        if (!visible)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);
        DataRecord record = DataCenter.mStageTable.GetRecord(property.mIndex);

        if (record != null)
        {
            GameCommon.SetUIText(obj, "spoil(Clone)_1", StageStar.GetDescription(record["ADDSTAR_1"]));
            GameCommon.SetUIText(obj, "spoil(Clone)_2", StageStar.GetDescription(record["ADDSTAR_2"]));
        }
    }

    private void ShowDropInfo(bool visible)
    {
        GameObject obj = GetSub("drop_info");

        if (!visible)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);
        GameObject dropObj = GameCommon.FindObject(obj, "grid_drop");
        int groupId = TableCommon.GetNumberFromStageConfig(property.mIndex, "DROPGROUPID");
        FillDropGridNew(dropObj,property.mIndex);
        //FillDropGrid(dropObj, groupId, 8);
    }

    private void ShowBossDrop(bool visible)
    {
        GameObject obj = GetSub("boss_drop");

        if (!visible)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);
        DataRecord bossData = getObject("BOSS_DATA") as DataRecord;

        if (bossData != null)
        {
            int bossID = bossData.get("BOSS_ID");
            int groupID = TableCommon.GetNumberFromBossConfig(bossID, "DROP_GROUP");
            GameObject dropObj = GameCommon.FindObject(obj, "grid_drop");
            //FillDropGrid(dropObj, groupID, 2);
            FillDropGridNew(dropObj, property.mIndex);
        }
    }

    private void ShowBossModel(bool visible)
    {
        GameObject obj = GetSub("boss_model");

        if (!visible)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);
        DataRecord bossData = getObject("BOSS_DATA") as DataRecord;

        if (bossData != null)
        {
            int bossID = bossData.get("BOSS_ID");
            ActiveBirthForUI birthUI = GameCommon.FindComponent<ActiveBirthForUI>(mGameObjUI, "ui_point");
            if (birthUI != null)
            {
                if (birthUI.mActiveObject != null)
                    birthUI.mActiveObject.OnDestroy();

                birthUI.mBirthConfigIndex = bossID;
                birthUI.mObjectType = (int)OBJECT_TYPE.BIG_BOSS;
                birthUI.Init();
                if (birthUI.mActiveObject != null)
                {
                    birthUI.mActiveObject.SetScale(80f);
                    birthUI.mActiveObject.PlayAnim("cute");

                    BossBirthOnApearUI modelScript = birthUI.mActiveObject.mMainObject.AddComponent<BossBirthOnApearUI>();
                    if (modelScript != null)
                        modelScript.mActiveObject = birthUI.mActiveObject;
                }
            }
        }
    }

    private void ShowStageImage(bool visible)
    {
        GameObject obj = GetSub("stage_image");

        if (!visible)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);
        DataRecord record = GetActiveStageRecord(property.mIndex);

        if (record != null)
        {
            string imageName = record.get("EVENTIMG");
            UITexture tex = obj.GetComponent<UITexture>();
            tex.mainTexture = GameCommon.LoadTexture(imageName, LOAD_MODE.RESOURCE);//Resources.Load(imageName, typeof(Texture)) as Texture;   
            //tex.MakePixelPerfect();
        }          
    }

    private void ShowBossInfo(bool visible)
    {
        GameObject obj = GetSub("boss_info");

        if (!visible)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);
        DataRecord bossData = getObject("BOSS_DATA") as DataRecord;

        if (bossData != null)
        {
            int bossID = bossData.get("BOSS_ID");
            DataRecord bossRecord = DataCenter.mBossConfig.GetRecord(bossID);

            if (bossRecord != null)
            {
                string name = bossRecord.get("NAME");
                int level = bossRecord.get("LV");
                int hp = bossRecord.get("BASE_HP");
                int attack = bossRecord.get("BASE_ATTACK");

                GameCommon.SetUIText(obj, "boss_name", name);
                GameCommon.SetUIText(obj, "boss_level", "Lv " + level);
                GameCommon.SetUIText(obj, "life_num", hp.ToString());
                GameCommon.SetUIText(obj, "damage_num", attack.ToString());
            }
        }
    }


    private void SetBtnStageIndex(int kStageIndex) 
    {
        NiceData tmpBtnBattleData = GameCommon.GetButtonData(mGameObjUI, "stage_info_start_new_1");

        if (tmpBtnBattleData != null)
            tmpBtnBattleData.set("STAGE_INDEX", kStageIndex);

        NiceData tmpBtnCleanData = GameCommon.GetButtonData(mGameObjUI, "saodang_one_time");

        if (tmpBtnCleanData != null)
            tmpBtnCleanData.set("STAGE_INDEX", kStageIndex);

        NiceData tmpBtnSweepData = GameCommon.GetButtonData(mGameObjUI, "saodang_multi_times");
        if (tmpBtnSweepData != null)
            tmpBtnSweepData.set("STAGE_INDEX", kStageIndex);
    }

    private void FillDropGridNew(GameObject kGridObj,int kStageIndex)
    {
        SetBtnStageIndex(kStageIndex);
        ShowDropItemInfo(kGridObj,kStageIndex);
    }

    private void FillDropGrid(GameObject container, int groupID, int maxCount)
    {
        //by chenliang
        //begin

        //设置当前掉落组ID
        //战斗按钮

		// added by xuke
        //NiceData tmpBtnBattleData = GameCommon.GetButtonData(mGameObjUI, "stage_info_start");
		NiceData tmpBtnBattleData = GameCommon.GetButtonData(mGameObjUI, "stage_info_start_new_1");
		//end

		if (tmpBtnBattleData != null)
            tmpBtnBattleData.set("DROP_GROUP_ID", groupID);
        //扫荡按钮

		//added by xuke
        //NiceData tmpBtnCleanData = GameCommon.GetButtonData(mGameObjUI, "stage_info_clean");
		NiceData tmpBtnCleanData = GameCommon.GetButtonData(mGameObjUI, "saodang_one_time");
		//end
		if (tmpBtnCleanData != null)
            tmpBtnCleanData.set("DROP_GROUP_ID", groupID);

        NiceData tmpBtnSweepData = GameCommon.GetButtonData(mGameObjUI, "saodang_multi_times");
        if (tmpBtnSweepData != null)
            tmpBtnSweepData.set("DROP_GROUP_ID", groupID);

        //end
        //UIGridContainer grid = container.GetComponent<UIGridContainer>();
        //List<DataRecord> dropList = GetDropList(groupID, maxCount);
        //ItemDataProvider provider = new ItemDataProvider();

        ////移除掉itemid为0的drop- -
        //List<DataRecord> list = dropList;
        //for (int i = 0; i < list.Count; i++)
        //{
        //    if (dropList[i] != null)
        //    {
        //        DataRecord record = list[i];
        //        if (record["ITEM_ID"] == 0)
        //        {
        //            list.RemoveAt(i);
        //        }
        //    }
        //}
        //dropList = list;

        //foreach (var drop in dropList)
        //{
        //    ItemData item = new ItemData();
        //    //item.mType = drop.get("ITEM_TYPE");
        //    item.mID = drop.get("ITEM_ID");
        //    item.mType = (int)PackageManager.GetItemTypeByTableID(item.mID);
        //    item.mNumber = 1;
        //    //item.mEquipElement = (int)property.stageElement;
        //    provider.Append(item);
        //}

        //UIGridContainer con = container.GetComponent<UIGridContainer>();
        //int count = provider.GetCount();
        //con.MaxCount = count;
		
        //for (int i = 0; i < count; ++i)
        //{
        //    ItemData data = provider.GetItem(i);
        //    GameObject item = con.controlList[i];
        //    ItemIcon icon = new ItemIcon(item);
        //    icon.Reset();
        //    //icon.Set(data);
        //    //added by xuke begin
        //    GameCommon.SetUIText(item, "count_label", "x" + data.mNumber.ToString());
        //    GameCommon.SetOnlyItemIcon(GameCommon.FindObject(item, "item_icon"), data.mID);
        //    AddButtonAction (GameCommon.FindObject(item, "item_icon"), () => GameCommon.SetAccountItemDetailsWindow (data.mID));
        //    //end
        //    GameCommon.SetIconData(item, data);
        //}
    }

    private List<DataRecord> GetMonsterList(int stageIndex)
    {
        DataRecord boss = null;
        List<DataRecord> list = GetMonsterGroup(stageIndex, out boss);
        List<DataRecord> result = new List<DataRecord>();

        foreach (var record in list)
        {
            if (!result.Exists(r => (int)r.get("MODEL") == (int)record.get("MODEL")))
                result.Add(record);

            if (result.Count >= 3)
                break;
        }
        //by chenliang
        //begin

//        result.Add(boss);
//-----------------
        if (boss != null)
            result.Add(boss);

        //end
        return result;
    }

    public static List<DataRecord> GetMonsterGroup(int stageIndex, out DataRecord boss)
    {
        List<DataRecord> monsterList = new List<DataRecord>();
        boss = null;
        int maxWayPoint = 0;

        foreach (var pair in DataCenter.mStageBirth.GetAllRecord())
        {
            if (pair.Key / 1000 == stageIndex)
            {
                DataRecord monster = DataCenter.mMonsterObject.GetRecord((int)pair.Value.get("MONSTER"));
                monsterList.Add(monster);
                int wayPoint = pair.Value.get("WAYPOINT");               

                if (wayPoint > maxWayPoint)
                {
                    boss = monster;
                    maxWayPoint = wayPoint;
                }
            }
        }

        monsterList.Remove(boss);
        return monsterList;
    }

    private List<DataRecord> GetDropList(int groupId, int maxCount)
    {
        List<DataRecord> list = GetItemGroup(groupId);
        list.Sort((lhs, rhs) => lhs.get("ITEM_DROP_WEIGHT") - rhs.get("ITEM_DROP_WEIGHT"));

        if (list.Count > maxCount)
            list = list.GetRange(0, maxCount);

        return list;
    }

    private List<DataRecord> GetItemGroup(int groupId)
    {
        List<DataRecord> result = new List<DataRecord>();

        foreach (var pair in DataCenter.mStageLootGroupIDConfig.GetAllRecord())
        {
            if (pair.Value.get("GROUP_ID") == groupId && pair.Value.get("ITEM_TYPE") != (int)ITEM_TYPE.AIR && pair.Value.get("ITEM_DROP_WEIGHT") > 0)
                result.Add(pair.Value);
        }

        return result;
    }

    private DataRecord GetActiveStageRecord(int stageId)
    {
        foreach (var pair in DataCenter.mEventConfig.GetAllRecord())
        {
            if (pair.Key > 0 && pair.Value.get("STAGE_ID") == stageId)
            {
                return pair.Value;
            }
        }

        return null;
    }

    private void RequestBattle()
    {
        if (property.stageType == STAGE_TYPE.CHAOS)
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
}


public class Button_stage_info_back : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("STAGE_INFO_WINDOW");
        return true;
    }
}

public class Button_stage_info_start : CEvent
{
    public override bool _DoEvent()
    {
        int _stageID = (int)getObject("STAGE_ID");
        //判断是否有挑战次数
        if (!GameCommon.HasLeftChallengeTimes(_stageID))
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.GUILD_BOSS_GUNAKA_TIMES_OVER);
            return true;
        }

		//判断体力是否足够
		if (RoleLogicData.Self.stamina < StageProperty.GetCurrentStageStaminaCost()/*CommonParam.battleMainStaminaCost*/) {
            //添加体力不足时的弹窗
            GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.STAMINA_ITEM);
			//DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SELECT_LEVEL_NO_ENOUGH_STAMINA);
				return true;
		}
        //by chenliang
        //begin

        //判断掉落物品的背包是否足够
        //int tmpDropGroupID = (int)getObject("DROP_GROUP_ID");
        //List<ItemDataBase> tmpItems = GameCommon.GetmStageLootGroup(tmpDropGroupID);
        //List<PACKAGE_TYPE> tmpTypes = PackageManager.GetPackageTypes(tmpItems);
        //if (!CheckPackage.Instance.CanEnterBattle(tmpTypes))
        //    return true;

        if (!GameCommon.CheckCanEnterBattle(this))
            return true;

        //end
        DataCenter.SetData("STAGE_INFO_WINDOW", "REQUEST_BATTLE", null);
        DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.ADVENTURE);
        return true;
    }
}

public class Button_stage_info_clean : CEvent
{
	GameObject obj;
    public override bool _DoEvent()
    {
        int _stageID = (int)getObject("STAGE_ID");
        //判断是否有挑战次数
        if (!GameCommon.HasLeftChallengeTimes(_stageID)) 
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.GUILD_BOSS_GUNAKA_TIMES_OVER);
            return true;
        }
        DataCenter.Set("CURRETN_SAODANG_STAGE_ID", _stageID);
		//判断体力是否足够
		if (RoleLogicData.Self.stamina < StageProperty.GetCurrentStageStaminaCost()/*CommonParam.battleMainStaminaCost*/) {
			//DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SELECT_LEVEL_NO_ENOUGH_STAMINA);
            GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.STAMINA_ITEM);
			return true;
		}

        //by chenliang
        //begin

        //判断掉落物品的背包是否足够
        //int tmpDropGroupID = (int)getObject("DROP_GROUP_ID");
        //List<ItemDataBase> tmpItems = GameCommon.GetmStageLootGroup(tmpDropGroupID);
        //List<PACKAGE_TYPE> tmpTypes = PackageManager.GetPackageTypes(tmpItems);
        //if (!CheckPackage.Instance.CanEnterBattle(tmpTypes))
        //    return true;

        if (!GameCommon.CheckCanEnterBattle(this))
            return true;

        //end
        obj = getObject("BUTTON") as GameObject;
        obj.GetComponent<BoxCollider>().enabled = false;
        WaitTime(0.3f);

        GlobalModule.DoCoroutine(DoRequestClean());
        return true;
    }

	public override void _OnOverTime ()
	{
		obj.GetComponent<BoxCollider>().enabled = true;
	}

    private IEnumerator DoRequestClean()
    {
        var req = new BattleMainSweepRequester();

		//去掉扫荡券
        //if (req.canSweep)
        //{
            yield return req.Start();

            if (req.success)
            {
                //DataCenter.CloseWindow("STAGE_INFO_WINDOW");
                DataCenter.OpenWindow("SWEEP_LIST_WINDOW", "ONE");

                DataCenter.SetData("SWEEP_LIST_WINDOW", 0.ToString() + "_ONE", req.accountInfo);     //判定是单次扫荡
                DataCenter.SetData("SWEEP_LIST_WINDOW", "STOP", "ONE");                            //判定是单次扫荡的结束
				//TODO Boss出现
                var sweepResult = req.respMsg;

                if (/*sweepResult.bossIndex != -1*/sweepResult.demonBoss != null && sweepResult.demonBoss.tid > 0)
				{
                    SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_BOSS, true);
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

                //added by xuke 刷新队伍按钮状态
                DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_RIGHT", "REFRESH_TEAM_BTN_NEW_MARK", null);
                //end

                //refresh stageinfo
                GlobalModule.DoLater(()=>{DataCenter.SetData("STAGE_INFO_WINDOW", "REFRESH_RESET_INFO", null);}, 0.2f);
            }
            else 
            {
                DataCenter.OpenMessageWindow("请求被拒绝");
                DataCenter.SetData("SWEEP_LIST_WINDOW", "STOP", "ONE");         
            }
        //}
//        else
//        {
//            DataCenter.OpenMessageWindow("扫荡券不足");
//        }
    }
}

public class Button_stage_info_clean_multi : CEvent
{
    public static bool stop = false;
    public static bool forcedStop = false;
    bool next = false;    
    int i = 0;
    public override bool _DoEvent()
    {
        if (RoleLogicData.Self.stamina < 5 && StageInfoWindow.multi_sweep_times == 0)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SELECT_LEVEL_NO_ENOUGH_STAMINA);
            return true;
        }

        if (StageInfoWindow.multi_sweep_times == 0)
        {
            DataCenter.OpenMessageWindow("当前没有剩余挑战次数");
            return true;
        }
        GlobalModule.DoCoroutine(doItAgain());
        return true;
    }

    IEnumerator doItAgain()
    {
        stop = false;
        forcedStop = false;
        for (i = 0; i < StageInfoWindow.multi_sweep_times; i++)
        {
            int _stageID = (int)getObject("STAGE_ID");          
            //判断是否有挑战次数
            if (!GameCommon.HasLeftChallengeTimes(_stageID))
            {
                if (!Guide.isActive) // 如果触发了新手引导，则不再打开提示窗口
                {
                    DataCenter.OpenMessageWindow("当前没有剩余挑战次数");
                }

                if (i != 0)
                {
                    DataCenter.SetData("SWEEP_LIST_WINDOW", "STOP", true);
                }
                break;
            }
            DataCenter.Set("CURRETN_SAODANG_STAGE_ID", _stageID);
            //判断体力是否足够
            if (RoleLogicData.Self.stamina < StageProperty.GetCurrentStageStaminaCost()/*CommonParam.battleMainStaminaCost*/)
            {
                //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SELECT_LEVEL_NO_ENOUGH_STAMINA);
                if (!Guide.isActive)  // 如果触发了新手引导，则不再打开提示窗口
                {
                    GameCommon.ShowResNotEnoughWin(RESOURCE_HINT_TYPE.STAMINA_ITEM);
                }

                if (i != 0)
                {
                    DataCenter.SetData("SWEEP_LIST_WINDOW", "STOP", true);
                }
                break;
            }
            if (i == 0)
            {
                stop = false;
            }
            //判断掉落物品的背包是否足够
            //int tmpDropGroupID = (int)getObject("DROP_GROUP_ID");
            //List<ItemDataBase> tmpItems = GameCommon.GetmStageLootGroup(tmpDropGroupID);
            //List<PACKAGE_TYPE> tmpTypes = PackageManager.GetPackageTypes(tmpItems);
            //if (!CheckPackage.Instance.CanEnterBattle(tmpTypes))
            if (!GameCommon.CheckCanEnterBattle(this))
            {
                if (i != 0)
                {
                    DataCenter.SetData("SWEEP_LIST_WINDOW", "STOP", true);
                }
                break;
            }
            if (stop)
            {
                if (i != 0)
                {
                    DataCenter.SetData("SWEEP_LIST_WINDOW", "STOP", true);
                }
                break;
            }          
            yield return GlobalModule.DoCoroutine(DoRequestClean(i));
            //DEBUG.LogError("request" + i);
        }
        yield return new WaitForSeconds(0.5f);
        if (GameObject.Find("sweep_list_window") != null)
        {
            DataCenter.SetData("SWEEP_LIST_WINDOW", "STOP", true);
        }

        //refresh stageinfo
        DataCenter.SetData("STAGE_INFO_WINDOW", "REFRESH_RESET_INFO", null);
    }

    void shouldStop(bool mvalue)
    {
        stop = mvalue;
    }

    IEnumerator DoRequestClean(int i)
    {
        var req = new BattleMainSweepRequester();
        yield return req.Start();

        if (req.success)
        {
            shouldStop(false);

            if (i == 0)
            {
                //DataCenter.CloseWindow("STAGE_INFO_WINDOW");
                DataCenter.OpenWindow("SWEEP_LIST_WINDOW");
            }
           
            DataCenter.SetData("SWEEP_LIST_WINDOW", i.ToString(), req.accountInfo);
            //TODO Boss出现
            var sweepResult = req.respMsg;

            if (/*sweepResult.bossIndex != -1*/sweepResult.demonBoss != null && sweepResult.demonBoss.tid > 0)
            {
                if (!(Button_stage_info_clean_multi.stop && Button_stage_info_clean_multi.forcedStop))
                {
                    SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_BOSS, true);
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
            //added by xuke 添加队伍按钮红点提示
            DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_RIGHT", "REFRESH_TEAM_BTN_NEW_MARK", null);
            //end
        }
        else
        {
            DataCenter.OpenMessageWindow("请求被拒绝");
            Button_stage_info_clean_multi.stop = true;
            Button_stage_info_clean_multi.forcedStop = true;
            DataCenter.SetData("SWEEP_LIST_WINDOW", "STOP", true);            
            if (GameObject.Find("add_vitality_up_window") != null)
            {
                MonoBehaviour.Destroy(GameObject.Find("add_vitality_up_window"));
            }
        }
        //}
        //        else
        //        {
        //            DataCenter.OpenMessageWindow("扫荡券不足");
        //        }
    }
}



public class Button_adjust_team_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("STAGE_INFO_WINDOW");
		//关闭左上角的冒险窗口
		DataCenter.CloseWindow ("SCROLL_WORLD_MAP_TOP_LEFT");
		DataCenter.CloseWindow ("TASK_REWARD_BOX");
		DataCenter.CloseWindow ("SCROLL_WORLD_MAP_BOTTOM_LEFT");
		DataCenter.CloseWindow ("SCROLL_WORLD_MAP_BOTTOM_RIGHT");
        DataCenter.CloseWindow("SCROLL_WORLD_MAP_CHAPTER_NAME_WINDOW");


        tWindow activeStageWindow = DataCenter.GetData("ACTIVE_STAGE_WINDOW") as tWindow;
        bool isActiveStageWindowOpen = activeStageWindow != null && activeStageWindow.mGameObjUI != null && activeStageWindow.mGameObjUI.activeInHierarchy;

        if (isActiveStageWindowOpen)
        {
            activeStageWindow.mGameObjUI.SetActive(false);
        }

        int currentWorldPage = ScrollWorldMapWindow.mPage;
        
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);

        DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);
        MainUIScript.Self.HideMainBGUI();
        MainUIScript.Self.mWindowBackAction = () => OnReturn(currentWorldPage, isActiveStageWindowOpen);
        return true;
    }

    private void OnReturn(int worldPage, bool isActiveStageWindowOpen)
    {
        MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
        DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SKIP_TO_PAGE", worldPage);

        if (isActiveStageWindowOpen)
        {
            tWindow activeStageWindow = DataCenter.GetData("ACTIVE_STAGE_WINDOW") as tWindow;

            if (activeStageWindow != null && activeStageWindow.mGameObjUI != null)
            {
                activeStageWindow.mGameObjUI.SetActive(true);
            }            
        }

        int stageIndex = DataCenter.Get("CURRENT_STAGE");
        DataCenter.OpenWindow("STAGE_INFO_WINDOW", stageIndex);
    }


}
/// <summary>
/// 冒险重置按钮
/// </summary>
public class Button_reset_times_btn : CEvent
{
    public override bool DoEvent()
    {
        //当前关卡今日已经重置次数
        int _todayResetTimes = (int)getObject("RESET_TIMES");
        //当前vip的最大重置次数
        int _maxResetTimes = TableCommon.GetNumberFromVipList(RoleLogicData.Self.vipLevel, "COPYRESET_NUM");
        if (_todayResetTimes >= _maxResetTimes)
        {
            //显示VIP等级不足提示框
            if (GameCommon.GetNextVipLevel() == -1)
            {
                DataCenter.OpenWindow("ADVENTURE_RESET_WINDOW", ADVENTURE_RESET_WIN_TYPE.MAX_RESET_TIMES);
            }
            else 
            {
                VipRechargeWindowOpenData _rechargeData = new VipRechargeWindowOpenData ();
		        _rechargeData.RechargeForType = RechargeForType.RESET_ADVENTURE;
                DataCenter.OpenWindow("VIP_RECHARGE_UP_WINDOW", _rechargeData);
            }
        }
        else 
        {
            //显示是否确认重置弹框
            DataCenter.OpenWindow("ADVENTURE_RESET_WINDOW", _todayResetTimes);
        }
        return base.DoEvent();
    }
}

public class Button_player_info_close_button : CEvent
{
    public override bool DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.player_info_window);
        DataCenter.Remove(PlayerInfoWindow.str_player_info_data);
        return true;
    }
}

public class Button_player_info_check_array_button : CEvent
{
    public override bool DoEvent()
    {
        GameCommon.SetDataByZoneUid("ArenaMainNotSetDragAmount", "NotSet");
        sPlayerInfo roleData = PlayerInfoWindow.GetWinData(PlayerInfoWindow.str_player_info_data).mObj as sPlayerInfo;
        if (roleData == null) return false;
        string rootName = PlayerInfoWindow.GetWinData(PlayerInfoWindow.str_root_name);
        GameCommon.VisitGameFriend( roleData.uid.ToString(), roleData.name, rootName); ;
        return true;
    }
}

public class Button_player_info_add_friend_button : CEvent
{
    public override bool DoEvent()
    {
        DataCenter.SetData(UIWindowString.player_info_window, "ChangeButtonState", PlayerInfoWindow.buttonState.disalbe);
        sPlayerInfo info = PlayerInfoWindow.GetWinData(PlayerInfoWindow.str_player_info_data).mObj as sPlayerInfo;
        if (info == null) return false;
        string iFriendID = info.uid;        
        if (iFriendID == Convert.ToString(CommonParam.mUId))
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_FRIEND_RECOMMEND_SELF, true);
        }
        else
        {
            FriendNetEvent.RequestAddFriendRequest(iFriendID, OnAddFriendSuccess, OnAddFriendFailed);
        }
        return true;
    }
    
	void OnAddFriendSuccess(string text) {
		SC_SendFriendRequest result = JCode.Decode<SC_SendFriendRequest>(text);

        STRING_INDEX ret = STRING_INDEX.ERROR_NONE;

		if (result.friendReqFull == 1) 
            ret = STRING_INDEX.ERROR_FRIEND_REQUEST_FULL;
        else if (result.friendsAlready == 1)
            ret = STRING_INDEX.ERROR_FRIEND_IS_FRIEND;
        else if (result.ret == (int)STRING_INDEX.ERROR_NONE)
            ret = STRING_INDEX.ERROR_FRIEND_RECOMMEND_SUCCESS;

        if (ret != STRING_INDEX.ERROR_NONE)
            DataCenter.OnlyTipsLabelMessage(ret);
	}

	void OnAddFriendFailed(string text) {        
	}
}

public class Button_player_info_msg_button : CEvent
{
    public override bool DoEvent()
    {
        sPlayerInfo info = PlayerInfoWindow.GetWinData(PlayerInfoWindow.str_player_info_data).mObj as sPlayerInfo;
        if (info == null) return false;
        ChatWindowOpenData openData = new ChatWindowOpenData(ChatType.Private, info.uid, info.name, null);
        DataCenter.SetData("CHAT_WINDOW", "OPEN", openData);
        DataCenter.CloseWindow("PLAYER_INFO_WINDOW");
        return true;
    }
}

public class Button_player_info_email_button : CEvent
{
    public override bool DoEvent()
    {

        return true;
    }
}