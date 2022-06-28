using UnityEngine;
using System.Collections;
using Logic;
using System;
using DataTable;
using System.Collections.Generic;

public class TeamPosInfoWindow : tWindow
{
    private GameObject mBaseAtributeGroup;
    private GameObject mFateGroup;
    private GameObject mRelateGroup;
    private GameObject mTalentGroup;
	private GameObject mIntroduceGroup;
    static public ActiveData mCurActiveData;
    //added by xuke 反馈文字相关
    static public bool mShowZeroInfo = false;
    //end
    //public const int RELATE_HEIGHT = 60;
    public const int MAX_TALENT_NUM = 15;

    public override void Init()
    {
        base.Init();
        
        //EventCenter.Self.RegisterEvent("Button_team_window_back_btn", new DefineFactory<Button_TeamWindowBack>());
		EventCenter.Self.RegisterEvent("Button_go_fate_button", new DefineFactory<Button_go_fate_button>());
		EventCenter.Self.RegisterEvent("Button_Upgrade_Skill", new DefineFactory<Button_Upgrade_Skill>());
		EventCenter.Self.RegisterEvent("Button_go_team_break_button", new DefineFactory<Button_go_team_break_button>());
        EventCenter.Self.RegisterEvent("Button_go_team_upgrade", new DefineFactory<Button_GoTeamUpgrade>());
    }


    public override void Open(object param)
    {
        base.Open(param);
		GameCommon.FindObject (mGameObjUI, "go_fate_button_gray").SetActive (false );

        DataCenter.CloseWindow("TEAM_DATA_WINDOW");

        Refresh(param);
    }
    public override void OnOpen()
    {
        base.OnOpen();
        GameObject _scrollViewObj = GameCommon.FindObject(mGameObjUI, "Scroll View");
        UIScrollView _scrollView = GameCommon.FindComponent<UIScrollView>(_scrollViewObj);
        if (_scrollView != null)
        {			
            if (_scrollView.enabled == false) 
            {
                _scrollView.enabled = true;
            }
			_scrollView.ResetPosition ();
			DataCenter.SetData ("TEAM_INFO_WINDOW", "SET_BASE_INFOS", true);
			GameObject mobj=GameObject .Find ("Label_no_pet_fragment_label");
			if(mobj != null)
				GameObject .Find ("Label_no_pet_fragment_label").GetComponent <UILabel >().text ="";
		}
    }

    private void UpdateFateBtnState()
    {
        object _relateWinObj = DataCenter.Self.getObject("TEAM_RELATE_INFO_WINDOW");
        if (_relateWinObj == null)
            return;
        tWindow _relateWin = _relateWinObj as tWindow;
        GameObject _fateBtnObj = GameCommon.FindObject(mGameObjUI, "go_fate_button");
        if(_fateBtnObj != null)
            _fateBtnObj.SetActive(!_relateWin.IsVisible());
    }

	public int GetRelateGroupOffsetY()
	{
		int ret = 0;
		int pos = mCurActiveData.teamPos;
		int level = mCurActiveData.starLevel;
		if (pos == 0) {
			ret = 56;
		} else {
			switch (level) {
				case 2:
				{ 
					ret = -98;
				}
				break;
			case 3:
				{ 
					ret = -83;
				}
				break;
			case 4:
				{
					ret = -36;
				}
				break;
			case 5:
				{
					ret = -22;
				}
				break;
			case 6:
				{
					ret = -14;
				}
				break;
			}
		}
		return ret;
	}

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if ("SHOW_WINDOW" == keyIndex)
        {
		}
        else if ("REFRESH_TEAM_POS_INFO_NEWMARK" == keyIndex) 
        {
            RefreshNewMark();
        }
        else if ("REFRESH_BAGPET_POSINFO_NEWMARK" == keyIndex) 
        {
            RefreshBagPetPosInfoNewMark(objVal);
        }
        else if ("UPFATE_BASE_ATTRIBUTE_INFO" == keyIndex)
        {
            UpdatePetAttributeInfo();
        }
		else if("SET_RELATE_POSITION" == keyIndex)
		{
			GameObject _scrollViewObj = GameObject.Find ("team_pos_info_window/Scroll View").gameObject;
			UIScrollView _scrollView = _scrollViewObj.GetComponent<UIScrollView> ();
			float topPos = GameCommon.FindObject (_scrollViewObj,"base_attribute_group").gameObject.transform.localPosition.y + _scrollViewObj.transform.Find ("base_attribute_group/bg").GetComponent<UISprite>().height/ 2.0f;
			GameObject relateGroupObj = GameCommon.FindObject (_scrollViewObj,"relate_group");
			float relatePos = GameCommon.FindObject (_scrollViewObj,"relate_group").gameObject.transform.localPosition.y - topPos + GetRelateGroupOffsetY();
			float roleIntroPos = GameCommon.FindObject (_scrollViewObj,"role_introduce_group").gameObject.transform.localPosition.y - topPos;
			float fPos = relatePos / roleIntroPos;
			_scrollView.SetDragAmount(0, fPos, false);
		}
		else if("SET_BASE_POSITION" == keyIndex)
		{
			UIScrollView _scrollView = GameObject.Find ("team_pos_info_window/Scroll View").GetComponent<UIScrollView> ();
			_scrollView.ResetPosition ();
		}
    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        UpdateSkill();
        ActiveData data = param as ActiveData;
        if (data != null)
        {
            mCurActiveData = data;
        }
        //by chenliang
        //beign

//        UpdateUI();
//-------------
        this.DoCoroutine(UpdateUIAsync());

        //end
        return true;
    }
	private void UpdateIntroduceInfo()
	{
		if (mCurActiveData != null)
		{
			GameObject introduceObj = GameCommon.FindObject (mIntroduceGroup, "role_introduce_label").gameObject;
			var r = DataCenter.mActiveConfigTable.GetRecord(mCurActiveData.tid);
			string strDesc = "";
			if (r != null)
			{
				strDesc = r["DESCRIBE"];
			}
			introduceObj.GetComponent<UILabel>().text = strDesc;		
		}
	}

    /// <summary>
    /// 更新符灵的基础属性信息
    /// </summary>
    private void UpdatePetAttributeInfo() 
    {
        UpdateAttack();
        UpdateHp();
        UpdatePhysicalDefence();
        UpdateMagicDefence();
    }

    public virtual void UpdateUI()
    {
        mBaseAtributeGroup = GameCommon.FindObject(mGameObjUI, "base_attribute_group");
        mFateGroup = GameCommon.FindObject(mGameObjUI, "fate_group");
        mRelateGroup = GameCommon.FindObject(mGameObjUI, "relate_group");
        mTalentGroup = GameCommon.FindObject(mGameObjUI, "talent_group");
		mIntroduceGroup = GameCommon.FindObject (mGameObjUI, "role_introduce_group");

        // 缘分状态隐藏
        GameObject go_team_upgrade = GetSub("go_team_upgrade");
        GameObject go_team_break_button = GetSub("go_team_break_button");
        GameObject Upgrade_Skill = GetSub("Upgrade_Skill");
        if (go_team_upgrade != null)
            go_team_upgrade.SetActive(mCurActiveData.teamPos >= (int)TEAM_POS.CHARACTER && mCurActiveData.teamPos <= (int)TEAM_POS.PET_3);
        if (go_team_break_button != null)
            go_team_break_button.SetActive(mCurActiveData.teamPos >= (int)TEAM_POS.CHARACTER && mCurActiveData.teamPos <= (int)TEAM_POS.PET_3);
        if (Upgrade_Skill != null)
            Upgrade_Skill.SetActive(mCurActiveData.teamPos >= (int)TEAM_POS.CHARACTER && mCurActiveData.teamPos <= (int)TEAM_POS.PET_3);

		UpdateSkill();

        UpdateUpgrageBtnState();
        //added by xuke 反馈文字相关
        if (CommonParam.mIsRefreshPetInfo)
        {
            UpdatePetAttributeInfo();
            TeamPosInfoWindow.mShowZeroInfo = false;
        }
        CommonParam.mIsRefreshPetInfo = true;
        //end
        UpdateFateInfo();
        UpdateRelateInfo();
        UpdateTalentInfo();
		UpdateIntroduceInfo();

            //             mPetStarLevelLabel = mGameObjUI.transform.Find("BG/pet_info/Info/star_level").GetComponent<UILabel>();
            //             mPetTitleLabel = mGameObjUI.transform.Find("BG/pet_info/Info/title").GetComponent<UILabel>();
            //             mPetNameLabel = mGameObjUI.transform.Find("BG/pet_info/Info/name").GetComponent<UILabel>();
            // 
            //             mSkillBtn1 = mGameObjUI.transform.Find("BG/pet_info/skill/Button1").gameObject;
            //             mSkillBtn2 = mGameObjUI.transform.Find("BG/pet_info/skill/Button2").gameObject;
            //             mSkillBtn3 = mGameObjUI.transform.Find("BG/pet_info/skill/Button3").gameObject;
            // 
            //             mRoleMastButtonUI = mGameObjUI.transform.Find("BG/black").gameObject;
            // 
            //             mPetInfoGroup = mGameObjUI.transform.Find("BG/pet_info_group").gameObject;
            //             mShopPetInfoGroup = mGameObjUI.transform.Find("BG/shop_pet_info_group").gameObject;
            //             mTujianPetInfoGroup = mGameObjUI.transform.Find("BG/tujian_pet_info_group").gameObject;
            //             mComposePetInfoGroup = mGameObjUI.transform.Find("BG/compose_pet_info_group").gameObject;
            // 
            //             mLevel = mPetInfoGroup.transform.Find("level_info/label").GetComponent<UILabel>();
            //             mExpPercentage = mPetInfoGroup.transform.Find("level_info/Progress Bar/label").GetComponent<UILabel>();
            //             mPetExpBar = mPetInfoGroup.transform.Find("level_info/Progress Bar").GetComponent<UIProgressBar>();
            // 
            //             mStrengthenLevelLabel = mPetInfoGroup.transform.Find("strengthen_level_info/num").GetComponent<UILabel>();
            //             mAttackLabel = mPetInfoGroup.transform.Find("damage_info/num").GetComponent<UILabel>();
            //             mMaxHPLabel = mPetInfoGroup.transform.Find("life_info/num").GetComponent<UILabel>();
            //             mFightingLabel = mPetInfoGroup.transform.Find("fighting_info/num").GetComponent<UILabel>();
            //             mAttributeLabel = mPetInfoGroup.transform.Find("attribute_info/Label").GetComponent<UILabel>();
            //             mAttackTypeLabel = mPetInfoGroup.transform.Find("attribute_info/attack_type_label").GetComponent<UILabel>();
            //             mElementIcon = mPetInfoGroup.transform.Find("attribute_info/sprite").GetComponent<UISprite>();
            // 
            //             mShopPetLevel = mShopPetInfoGroup.transform.Find("level_label").GetComponent<UILabel>();
            //             mShopAttackLabel = mShopPetInfoGroup.transform.Find("damage_info/num").GetComponent<UILabel>();
            //             mShopMaxHPLabel = mShopPetInfoGroup.transform.Find("life_info/num").GetComponent<UILabel>();
            //             mShopAttributeLabel = mShopPetInfoGroup.transform.Find("attribute_info/Label").GetComponent<UILabel>();
            //             mShopAttackTypeLabel = mShopPetInfoGroup.transform.Find("attribute_info/attack_type_label").GetComponent<UILabel>();
            //             mShopElementIcon = mShopPetInfoGroup.transform.Find("attribute_info/sprite").GetComponent<UISprite>();
            // 
            //             mTujianAttackLabel = mTujianPetInfoGroup.transform.Find("damage_info/num").GetComponent<UILabel>();
            //             mTujianMaxHPLabel = mTujianPetInfoGroup.transform.Find("life_info/num").GetComponent<UILabel>();
            //             mTujianAwardGroup = mTujianPetInfoGroup.transform.Find("tujian_award_group").gameObject;
            //             mTujianTip = mTujianPetInfoGroup.transform.Find("sprite").gameObject;
            //             mTujianTipLabel = mTujianPetInfoGroup.transform.Find("sprite/label").GetComponent<UILabel>();
            //             mTujianAttributeLabel = mTujianPetInfoGroup.transform.Find("attribute_info/Label").GetComponent<UILabel>();
            //             mTujianAttackTypeLabel = mTujianPetInfoGroup.transform.Find("attribute_info/attack_type_label").GetComponent<UILabel>();
            //             mTujianElementIcon = mTujianPetInfoGroup.transform.Find("attribute_info/sprite").GetComponent<UISprite>();
            // 
            //             mComposeAttackLabel = mComposePetInfoGroup.transform.Find("damage_info/num").GetComponent<UILabel>();
            //             mComposeMaxHPLabel = mComposePetInfoGroup.transform.Find("life_info/num").GetComponent<UILabel>();
            // 
            //             mPetStarLevelLabel.text = activeData.starLevel.ToString();
            //             mPetTitleLabel.text = TableCommon.GetStringFromActiveCongfig(activeData.tid, "NAME");
            //             mPetNameLabel.text = TableCommon.GetStringFromActiveCongfig(activeData.tid, "TXT");
            // 
            //             mLevel.text = "Lv." + activeData.level.ToString();
            //             SetStrengthenLevel();
            //             SetAttack();
            //             SetMaxHP();
            //             SetFightingNum();
            //             SetAttributeLabel();
            //             SetAttackTypeLabel();
            //             SetElementInfo();
            //             SetSkillInfo();
            //             SetPetInfoTipBtn();
            // 
            //             DataCenter.SetData("PetInfoWindow", "SHOW_PET_FLAG", false);
            // 
            //             // set button
            //             SetBtn();
    }
    //by chenliang
    //begin

    public virtual IEnumerator UpdateUIAsync()
    {
        mBaseAtributeGroup = GameCommon.FindObject(mGameObjUI, "base_attribute_group");
        mFateGroup = GameCommon.FindObject(mGameObjUI, "fate_group");
        mRelateGroup = GameCommon.FindObject(mGameObjUI, "relate_group");
        mTalentGroup = GameCommon.FindObject(mGameObjUI, "talent_group");
        mIntroduceGroup = GameCommon.FindObject(mGameObjUI, "role_introduce_group");

        // 缘分状态隐藏
        GameObject go_team_upgrade = GetSub("go_team_upgrade");
        GameObject go_team_break_button = GetSub("go_team_break_button");
        GameObject Upgrade_Skill = GetSub("Upgrade_Skill");
        if (go_team_upgrade != null)
            go_team_upgrade.SetActive(mCurActiveData.teamPos >= (int)TEAM_POS.CHARACTER && mCurActiveData.teamPos <= (int)TEAM_POS.PET_3);
        if (go_team_break_button != null)
            go_team_break_button.SetActive(mCurActiveData.teamPos >= (int)TEAM_POS.CHARACTER && mCurActiveData.teamPos <= (int)TEAM_POS.PET_3);
        if (Upgrade_Skill != null)
            Upgrade_Skill.SetActive(mCurActiveData.teamPos >= (int)TEAM_POS.CHARACTER && mCurActiveData.teamPos <= (int)TEAM_POS.PET_3);

        UpdateSkill();

        UpdateUpgrageBtnState();
        //added by xuke 反馈文字相关
        if (CommonParam.mIsRefreshPetInfo)
        {
            UpdatePetAttributeInfo();
            TeamPosInfoWindow.mShowZeroInfo = false;
        }
        CommonParam.mIsRefreshPetInfo = true;

        yield return null;
        UpdateFateInfo();
        UpdateFateBtnState();
        UpdateRelateInfo();
        yield return null;
        UpdateTalentInfo();
        UpdateIntroduceInfo();
    }

    //end

#region 红点逻辑
    private void RefreshNewMark()
    {
        // 突破
        GameObject _breakBtnObj = GetSub("go_team_break_button");
        GameCommon.SetNewMarkVisible(_breakBtnObj, TeamNewMarkManager.Self.GetPetNewMarkData((int)TeamManager.mCurTeamPos).CheckBreak);
        // 技能
        GameObject _skillBtnObj = GetSub("Upgrade_Skill");
        GameCommon.SetNewMarkVisible(_skillBtnObj, TeamNewMarkManager.Self.GetPetNewMarkData((int)TeamManager.mCurTeamPos).CheckSkillUp);
        // 升级
        GameObject _levelUpBtnObj = GetSub("go_team_upgrade");
        GameCommon.SetNewMarkVisible(_levelUpBtnObj, TeamNewMarkManager.Self.GetPetNewMarkData((int)TeamManager.mCurTeamPos).CheckLevelUp);
    }
    private void RefreshBagPetPosInfoNewMark(object kObjVal) 
    {
        if (kObjVal == null && !(kObjVal is PetData))
            return;
        PetData _petData = kObjVal as PetData;
        if (_petData == null)
            return;

        GameObject _breakBtnObj = GetSub("go_team_break_button");
        GameCommon.SetNewMarkVisible(_breakBtnObj, TeamNewMarkManager.Self.GetPetNewMarkData(_petData.teamPos).CheckBreak);
        GameObject _skillBtnObj = GetSub("Upgrade_Skill");
        GameCommon.SetNewMarkVisible(_skillBtnObj, TeamNewMarkManager.Self.GetPetNewMarkData(_petData.teamPos).CheckSkillUp);
        GameObject _levelUpBtnObj = GetSub("go_team_upgrade");
        GameCommon.SetNewMarkVisible(_levelUpBtnObj, TeamNewMarkManager.Self.GetPetNewMarkData(_petData.teamPos).CheckLevelUp);        
    }
#endregion
    // 根据技能ID来设置技能UI信息
    private void SetSkillUIInfo(NewSkillBean kSkillBean ,int kSkillPosID) 
    {
        string _dataReadIndex = (kSkillPosID + 1).ToString();
        int _SkillID = TableCommon.GetNumberFromActiveCongfig(mCurActiveData.tid, "PET_SKILL_" + _dataReadIndex);
        DataRecord _skillRecord = GameCommon.GetSkillDataRecord(_SkillID);
        if (kSkillBean == null)
        {
            DEBUG.LogError("请设置需要初始化的技能");
            return;
        }
        // 如果没有这个技能则隐藏
        if (_skillRecord == null) 
        {
            kSkillBean.gameObject.SetActive(false);
            return;
        }
        kSkillBean.gameObject.SetActive(true);

        int playLevel = mCurActiveData.breakLevel;
        kSkillBean.skillId = _SkillID;
        kSkillBean.SetSkillLevel(mCurActiveData.skillLevel[kSkillPosID]);
        kSkillBean.skillDesc = GameCommon.InitSkillDescription(mCurActiveData.skillLevel[kSkillPosID], _SkillID);     //> 技能描述 
        kSkillBean.skillName = _skillRecord.getObject("NAME").ToString();                   //> 技能名称
        kSkillBean.icon = _skillRecord.getObject("SKILL_SPRITE_NAME").ToString();           //> 技能图标图片名称
        string _atlasName = _skillRecord.getObject("SKILL_ATLAS_NAME").ToString();          //> 图集名称
        // 设置技能图标
        kSkillBean.SetSkillIcon(_atlasName,kSkillBean.icon);

        int skillOpenLevel = TableCommon.GetNumberFromActiveCongfig(mCurActiveData.tid, "SKILL_ACTIVE_LEVEL_" + _dataReadIndex);
        if (playLevel >= skillOpenLevel)
        {
            kSkillBean.UnlockSkill();
            kSkillBean.SetSkillLevel(mCurActiveData.skillLevel[kSkillPosID]);

            SkillEnableLevelUp.usrOpenSkills++;
        }
        else
        {
            kSkillBean.LockSkill();
            kSkillBean.SetSkillLevel(mCurActiveData.skillLevel[kSkillPosID]);
            string str = DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_SKILL_OPEN_TIPS, "STRING_CN");
            string strTips = skillOpenLevel.InsertToString(str);//STRING_INDEX
            kSkillBean.skillDesc += "\n[ff0000]" + strTips;

        }

    }




    private void UpdateSkill()
    {
        // clear skill open count
        SkillEnableLevelUp.usrOpenSkills = 0;
        if (mCurActiveData != null)
        {
            NewSkillBean[] skills = mGameObjUI.GetComponentsInChildren<NewSkillBean>(true);

            if (skills.Length <= 0)
                return;

            int playLevel = mCurActiveData.level;
            for (int i = 0; i < 4; i++) 
            {
                SetSkillUIInfo(skills[i],i);
            }
            GameObject _uiGridObj = GameCommon.FindObject(skills[0].transform.parent.parent.gameObject, "Grid_Pos");
            _uiGridObj.GetComponent<UIGrid>().Reposition();
            GameObject _mSKILL = _uiGridObj.transform.parent.gameObject;
            if (mCurActiveData.teamPos != (int)TEAM_POS.CHARACTER)
            {

                //DEBUG.Log(_mSKILL.name);
                _mSKILL.SetActive(true);
            }
            else
            {
                //DEBUG.Log(_mSKILL.name);
                _mSKILL.SetActive(false);
            }
            skills[0].SetSkillDesc(skills[0].skillDesc);
            // 默认显示第一个技能的名称和当前等级
            string skillName = TableCommon.GetStringFromSkillConfig(skills[0].skillId , "NAME");
            GameObject _skillRootObj = GameCommon.FindObject(mGameObjUI, "Skill");
            UILabel _skillNameLbl = GameCommon.FindObject(_skillRootObj, "skill_name_value_label").GetComponent<UILabel>();
			_skillNameLbl.text = skillName;
            UILabel _curSkillLbl = GameCommon.FindObject(_skillRootObj, "level_label").GetComponent<UILabel>();
            _curSkillLbl.text = mCurActiveData.skillLevel[0].ToString();
            // 默认选中第一个技能
            UIToggle _toggle = GameCommon.FindComponent<UIToggle>(_skillRootObj, "Button1");
            _toggle.value = true;
            UIToggle _toggle2 = GameCommon.FindComponent<UIToggle>(_skillRootObj, "Button2");
            _toggle2.value = false;
            UIToggle _toggle3 = GameCommon.FindComponent<UIToggle>(_skillRootObj, "Button3");
            _toggle3.value = false;
            UIToggle _toggle4 = GameCommon.FindComponent<UIToggle>(_skillRootObj, "Button4");
            _toggle4.value = false;

            mGameObjUI.BroadcastMessage("ReceiverMessage", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void UpdateUpgrageBtnState()
    {
        UIImageButton btn = GameCommon.FindComponent<UIImageButton>(mBaseAtributeGroup, "go_team_upgrade");
        if (btn != null)
        {
			if(CommonParam.isOnLineVersion)
			{
				btn.isEnabled = mCurActiveData.teamPos != (int)TEAM_POS.CHARACTER;
			}
			else
			{
				btn.gameObject.SetActive(mCurActiveData.teamPos != (int)TEAM_POS.CHARACTER);
			}
        }
    }

    private void UpdateAttack()
    {
        GameObject obj = GameCommon.FindObject(mBaseAtributeGroup, "attack");
        GameCommon.SetAttributeName(obj, (int)AFFECT_TYPE.ATTACK, "title");

        UILabel label = mBaseAtributeGroup.transform.Find("attack/num").GetComponent<UILabel>();
        if (mCurActiveData != null && label != null)
        {
			//by chenliang
			//begin

//			label.text = Convert.ToInt32(mCurActiveData.GetAttribute(AFFECT_TYPE.ATTACK)).ToString();
//------------------
            ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(mCurActiveData.teamPos);
            float tmpATK = GameCommon.GetBaseAttack(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);
            float tmpFinalATK = 0.0f;
            if (tmpData == null)
                tmpFinalATK = tmpATK;
            else
            {
                //Affect tmpAffect = AffectFunc.GetPetAllAffect(mCurActiveData.teamPos);
                //Affect relAffect = Relationship.GetTotalAffect(mCurActiveData.teamPos);
                //tmpFinalATK = tmpAffect.Final(AFFECT_TYPE.ATTACK, tmpATK);
                //tmpFinalATK = relAffect.Final(AFFECT_TYPE.ATTACK, tmpFinalATK);
                tmpFinalATK = AffectFunc.Final(mCurActiveData.teamPos, AFFECT_TYPE.ATTACK, tmpATK);
            }
            label.text = tmpFinalATK.ToString("f0");

            if (TeamPosInfoWindow.mShowZeroInfo) 
            {
                label.text = "0";
            }
			//end
        }
    }

    private void UpdateHp()
    {
        GameObject obj = GameCommon.FindObject(mBaseAtributeGroup, "hp");
        GameCommon.SetAttributeName(obj, (int)AFFECT_TYPE.HP, "title");

        UILabel label = mBaseAtributeGroup.transform.Find("hp/num").GetComponent<UILabel>();
        if (mCurActiveData != null && label != null)
        {
			//by chenliang
			//begin

//			label.text = Convert.ToInt32(mCurActiveData.GetAttribute(AFFECT_TYPE.HP)).ToString();
//-----------------
            ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(mCurActiveData.teamPos);
            float tmpHP = GameCommon.GetBaseMaxHP(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);
            float tmpFinalHP = 0.0f;
            if (tmpData == null)
                tmpFinalHP = tmpHP;
            else
            {
                //Affect tmpAffect = AffectFunc.GetPetAllAffect(mCurActiveData.teamPos);
                //Affect relAffect = Relationship.GetTotalAffect(mCurActiveData.teamPos);
                //tmpFinalHP = tmpAffect.Final(AFFECT_TYPE.HP_MAX, tmpHP);
                //tmpFinalHP = relAffect.Final(AFFECT_TYPE.HP_MAX, tmpFinalHP);
                tmpFinalHP = AffectFunc.Final(mCurActiveData.teamPos, AFFECT_TYPE.HP_MAX, tmpHP);
            }
            label.text = tmpFinalHP.ToString("f0");

			//end
            if (TeamPosInfoWindow.mShowZeroInfo)
            {
                label.text = "0";
            }
        }
    }
    private void UpdatePhysicalDefence()
    {
        GameObject obj = GameCommon.FindObject(mBaseAtributeGroup, "physical_defense");
        GameCommon.SetAttributeName(obj, (int)AFFECT_TYPE.PHYSICAL_DEFENCE, "title");

        UILabel label = mBaseAtributeGroup.transform.Find("physical_defense/num").GetComponent<UILabel>();
        if (mCurActiveData != null && label != null)
        {
			//by chenliang
			//begin

//			label.text = Convert.ToInt32(mCurActiveData.GetAttribute(AFFECT_TYPE.PHYSICAL_DEFENCE)).ToString();
//--------------------
            ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(mCurActiveData.teamPos);
            float tmpPD = GameCommon.GetBasePhysicalDefence(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);
            float tmpFinalPD = 0.0f;
            if (tmpData == null)
                tmpFinalPD = tmpPD;
            else
            {
                //Affect tmpAffect = AffectFunc.GetPetAllAffect(mCurActiveData.teamPos);
                //Affect relAffect = Relationship.GetTotalAffect(mCurActiveData.teamPos);
                //tmpFinalPD = tmpAffect.Final(AFFECT_TYPE.PHYSICAL_DEFENCE, tmpPD);
                //tmpFinalPD = relAffect.Final(AFFECT_TYPE.PHYSICAL_DEFENCE, tmpFinalPD);
                tmpFinalPD = AffectFunc.Final(mCurActiveData.teamPos, AFFECT_TYPE.PHYSICAL_DEFENCE, tmpPD);
            }
            label.text = tmpFinalPD.ToString("f0");

			//end
            if (TeamPosInfoWindow.mShowZeroInfo)
            {
                label.text = "0";
            }
        }
    }

    private void UpdateMagicDefence()
    {
        GameObject obj = GameCommon.FindObject(mBaseAtributeGroup, "magic_defense");
        GameCommon.SetAttributeName(obj, (int)AFFECT_TYPE.MAGIC_DEFENCE, "title");

        UILabel label = mBaseAtributeGroup.transform.Find("magic_defense/num").GetComponent<UILabel>();
        if (mCurActiveData != null && label != null)
        {
			//by chenliang
			//begin

//			label.text = Convert.ToInt32(mCurActiveData.GetAttribute(AFFECT_TYPE.MAGIC_DEFENCE)).ToString();
//--------------------
            ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(mCurActiveData.teamPos);
            float tmpMD = GameCommon.GetBaseMagicDefence(mCurActiveData.tid, mCurActiveData.level, mCurActiveData.breakLevel);
            float tmpFinalMD = 0.0f;
            if (tmpData == null)
                tmpFinalMD = tmpMD;
            else
            {
                //Affect tmpAffect = AffectFunc.GetPetAllAffect(mCurActiveData.teamPos);
                //Affect relAffect = Relationship.GetTotalAffect(mCurActiveData.teamPos);
                //tmpFinalMD = tmpAffect.Final(AFFECT_TYPE.MAGIC_DEFENCE, tmpMD);
                //tmpFinalMD = relAffect.Final(AFFECT_TYPE.MAGIC_DEFENCE, tmpFinalMD);
                tmpFinalMD = AffectFunc.Final(mCurActiveData.teamPos, AFFECT_TYPE.MAGIC_DEFENCE, tmpMD);
            }
            label.text = tmpFinalMD.ToString("f0");

			//end
            if (TeamPosInfoWindow.mShowZeroInfo)
            {
                label.text = "0";
            }
        }
    }

    private void UpdateFateInfo()
    {
        if (mCurActiveData != null)
        {
            UILabel fateLevel = mFateGroup.transform.Find("fate_level").GetComponent<UILabel>();
            if (fateLevel != null)
            {
                fateLevel.text = mCurActiveData.fateLevel.ToString();
            }

            UILabel fateTipsLabel = mFateGroup.transform.Find("fate_label_tips").GetComponent<UILabel>();
            if (fateTipsLabel != null)
            {
				string showText = "";
				showText = TableCommon.getStringFromStringList (STRING_INDEX.ERROR_FATE_DESCRIBE_TEXT);
				fateTipsLabel.text = showText;
            }
        }
    }

	public void AddIconList(DataRecord r, GameObject cell)
	{
		//add iconlist
		List<int> tidList = new List<int>();
		tidList.Clear ();
		for(int j = 1; j <= 6; j++)
		{
			int tempTid = r["NEED_CONTENT_" + j.ToString()];
			if(tempTid !=0)
			{
				tidList.Add(tempTid);
			}
		}
		var iconGrid = GameCommon.FindObject(cell, "icon_grid").GetComponent<UIGridContainer>();
		iconGrid.MaxCount = tidList.Count;
		var iconGridList = iconGrid.controlList;
		for(int k = 0; k < tidList.Count; k++)
		{
			GameObject board = iconGridList[k];
			if(board != null)
			{
				int tid = tidList[k];
				GameCommon.SetOnlyItemIcon(GameCommon.FindObject(board, "iconBtn"), tid);
				GameCommon.FindObject(board, "iconBtn").GetComponent<UISprite>().color = Relationship.IsActiveByTid(tid, mCurActiveData.teamPos) ? new Color(1, 1, 1) : new Color(.5f, .5f, .5f);
				GameCommon.SetUIText(board, "name", GameCommon.GetItemName(tid));
				GameCommon.FindObject(board, "name").GetComponent<UILabel>().color = GameCommon.GetNameColor(tid);

				//btn 
				GameCommon.AddButtonAction(GameCommon.FindObject(board, "iconBtn"), ()=>{
					GameCommon.SetItemDetailsWindow(tid);
				});
			}
		}
	}

    private void UpdateRelateInfo()
    {
        if (mCurActiveData != null)
        {
            //added by xuke 绑定缘分跳转功能
            AddButtonAction("relate_label_bg", () => 
            {
                DataCenter.OpenWindow(UIWindowString.petDetail, mCurActiveData.tid);
                DataCenter.SetData(UIWindowString.petDetail, "RELATE_INFO",mCurActiveData.tid);
                DEBUG.Log("符灵tid是:" + mCurActiveData.tid);
            });
            //end
            var rels = Relationship.GetCachedRelationshipList(mCurActiveData.teamPos);//Relationship.AllRelationships(mCurActiveData.teamPos);
            UIGridContainer grid = mRelateGroup.transform.Find("grid").GetComponent<UIGridContainer>();
            //UISprite bgSprite = mRelateGroup.transform.Find ("bg").GetComponent<UISprite>();
            //UISprite labelBgSprite = mRelateGroup.transform.Find ("bg/label_bg").GetComponent<UISprite>();
            //bgSprite.height = 120;
            //labelBgSprite.height = 80;
           
            if (grid != null && rels != null)
            {
                grid.MaxCount = rels.Count;

				//set bg height
				GameObject objBg = GameCommon.FindObject (mRelateGroup, "relate_bg");
				objBg.GetComponent<UISprite> ().height = rels.Count * (int)grid.CellHeight + 25;

                //bgSprite.height = bgSprite.height + RELATE_HEIGHT * (rels == null ? 0 : rels.Count - 1);
                //labelBgSprite.height = labelBgSprite.height + RELATE_HEIGHT * (rels == null ? 0 : rels.Count - 1);

                for (int i = 0; i < grid.MaxCount; ++i)
                {
                    var r = DataCenter.mRelateConfig.GetRecord(rels[i].tid);

                    if (r != null)
                    {
                        string relName = r["RELATE_NAME"];
                        string relDesc = r["RELATE_DWSCRIBE"];
                        var cell = grid.controlList[i];
                        UILabel nameLab = GameCommon.FindComponent<UILabel>(cell, "relate_name");
                        UILabel descLab = GameCommon.FindComponent<UILabel>(cell, "relate_tips");
                        nameLab.text ="【" +relName+"】";
                        descLab.text = relDesc;

						nameLab.color = Color.white;
						nameLab.effectStyle = UILabel.Effect.Outline;
						nameLab.effectColor = new Color(53f/255f, 40f/255f, 32f/255f);
						descLab.color = Color.white;
						descLab.effectStyle = UILabel.Effect.Outline;
						descLab.effectColor = new Color(53f/255f, 40f/255f, 32f/255f);

                        if (mCurActiveData.teamPos < (int)TEAM_POS.MAX && rels[i].active)
                        {
							nameLab.text = "[99ff66]" + nameLab.text;
							descLab.text = "[99ff66]" + descLab.text+"[-]";
                        }

						//add list
						AddIconList(r, cell);
                    }
                }
			}else if(grid != null && rels == null)
			{
				List<PetRelation> result = new List<PetRelation>();
				DataRecord record = DataCenter.mActiveConfigTable.GetRecord(mCurActiveData.tid);
				
				for (int i = 1; i <= 15; ++i)
				{
					string fieldName = "RELATE_ID_" + i.ToString();
					int relTid = record[fieldName];
					if (relTid > 0)
					{
						PetRelation rel = new PetRelation();
						rel.tid = relTid;
						result.Add(rel);
					}
				}
				grid.MaxCount = result.Count;
                //bgSprite.height = bgSprite.height + RELATE_HEIGHT * (result.Count - 1);
                //labelBgSprite.height = labelBgSprite.height + RELATE_HEIGHT * (result.Count - 1);
				for (int i = 0; i < grid.MaxCount; ++i)
				{
					var r = DataCenter.mRelateConfig.GetRecord(result[i].tid);
					if (r != null)
					{
						string relName = r["RELATE_NAME"];
						string relDesc = r["RELATE_DWSCRIBE"];
						var cell = grid.controlList[i];
						UILabel nameLab = GameCommon.FindComponent<UILabel>(cell, "relate_name");
						UILabel descLab = GameCommon.FindComponent<UILabel>(cell, "relate_tips");
                        nameLab.text = "【" + relName + "】";
						descLab.text = relDesc;
                        //added by xuke
                        nameLab.color = Color.white;
						nameLab.text = nameLab.text;
						nameLab.effectStyle = UILabel.Effect.Outline;
						nameLab.effectColor = new Color(53f/255f, 40f/255f, 32f/255f);

                        //descLab.text = GameCommon.ClearRelateStrColorPrefix(descLab.text);
						descLab.text = descLab.text + "[-]";
						descLab.effectStyle = UILabel.Effect.Outline;
						descLab.effectColor = new Color(53f/255f, 40f/255f, 32f/255f);
                        //end
					}
				}
			}
            //if (mCurActiveData.tid >= 50000 && mCurActiveData.tid < 60000)
            //{
            //    grid.CellHeight = 40;
            //    grid.transform.parent.localPosition = new Vector3(0, 12, 0);
            //}
            //else
            //{
            //    grid.CellHeight = 60;
            //    grid.transform.parent.localPosition = new Vector3(0, -227, 0);
            //}
			// 天赋Grid位置动态调整
            //var p = mRelateGroup.transform.localPosition;
            //mTalentGroup.transform.localPosition = new Vector3(p.x, p.y - bgSprite.height - 12.0f, p.z);
            grid.Reposition();
            grid.repositionNow = false;
            //by chenliang
            //begin

            //动态调整
            GameObject tmpGOGridParent = grid.transform.parent.gameObject;
//             if (tmpGOGridParent != null)
//             {
//                 tmpGOGridParent.SetActive(false);
//                 tmpGOGridParent.SetActive(true);
//             }
//            DynamicArrangment.AdjustImmediate(tmpGOGridParent);

            //end
        }
    }
    private void UpdateTalentInfo()
    {
        if (mCurActiveData != null)
        {
            UIGridContainer grid = mTalentGroup.transform.Find("grid").GetComponent<UIGridContainer>();
            //UISprite bgSprite = mTalentGroup.transform.Find ("bg").GetComponent<UISprite>();
            //UISprite labelBgSprite = mTalentGroup.transform.Find ("bg/label_bg").GetComponent<UISprite>();
            //bgSprite.height = 120;
            //grid.CellHeight = 40;
            //labelBgSprite.height = 80;

			int iMaxTalentNum = 0;
			for(int i = 0; i < MAX_TALENT_NUM; i++)
			{
				int talentId = TableCommon.GetNumberFromBreakBuffConfig (mCurActiveData.tid, "BREAK_" + (i + 1).ToString());
				if(talentId != 0)
				{
					iMaxTalentNum++;
				}
			}

            if (grid != null)
            {
//                grid.MaxCount = mCurActiveData.breakLevel;
				grid.MaxCount = iMaxTalentNum;

                //bgSprite.height = bgSprite.height + RELATE_HEIGHT * (grid.MaxCount - 1);
                //labelBgSprite.height = labelBgSprite.height + RELATE_HEIGHT * (grid.MaxCount - 1);

                for (int i = 0; i < grid.MaxCount; i++)
                {
                    int bufferId = TableCommon.GetNumberFromBreakBuffConfig(mCurActiveData.tid, "BREAK_" + (i + 1).ToString());
                    if (bufferId > 0)
                    {
                        DataRecord record = DataCenter.mAffectBuffer.GetRecord(bufferId);
                        if (record != null)
                        {
                            GameObject obj = grid.controlList[i];
                            UILabel name = GameCommon.FindComponent<UILabel>(obj, "talent_name");
                            if (name != null)
                            {
								name.text = record["NAME"];
								name.color = Color.white;
								name.effectStyle = UILabel.Effect.Outline;
								name.effectColor = new Color(53f/255f, 40f/255f, 32f/255f);
								if(i <= mCurActiveData.breakLevel - 1)
								{
									name.text = "[99ff66]" + record["NAME"];
								}
                            }

                            UILabel info = GameCommon.FindComponent<UILabel>(obj, "talent_tips");
                            if (info != null)
                            {
								info.text = record["TIP_TITLE"];
								info.color = Color.white;
								info.effectStyle = UILabel.Effect.Outline;
								info.effectColor = new Color(53f/255f, 40f/255f, 32f/255f);
								if(i <= mCurActiveData.breakLevel - 1)
								{
									info.text = "[99ff66]" + record["TIP_TITLE"];
								}
                            }

                            //UILabel add_rate = GameCommon.FindComponent<UILabel>(obj, "talent_add_rate");
                            //if (add_rate != null)
                            //{
                            //    add_rate.text = record["AFFECT_VALUE"] + "%";
                            //    if(i <= mCurActiveData.breakLevel - 1)
                            //    {
                            //        add_rate.text ="[ff3333]" + record["AFFECT_VALUE"];
                            //    }
                            //}
                        }
                    }
                }

                grid.Reposition();
                grid.repositionNow = false;
                //by chenliang
                //begin

                GameObject tmpGOGridParent = grid.transform.parent.gameObject;
//             if (tmpGOGridParent != null)
//             {
//                 tmpGOGridParent.SetActive(false);
//                 tmpGOGridParent.SetActive(true);
//             }
//                DynamicArrangment.AdjustImmediate(tmpGOGridParent);
                GlobalModule.DoOnNextUpdate(() =>
                {
                    DynamicArrangment.AdjustImmediate(mGameObjUI);
                });

                //end
            }
        }
    }
    public override void OnClose()
    {
        if (ChangeTipManager.Self != null)
        {
            ChangeTipManager.Self.CheckChangeTipShowState(mGameObjUI);
        }
        base.OnClose();
    }
}


public class Button_GoTeamUpgrade : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("TEAM_POS_INFO_WINDOW");
        DataCenter.OpenWindow("TEAM_RIGHT_ROLE_INFO_WINDOW", "ROLE_LEVEL_UP");
        return true;
    }
}

class Button_go_fate_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow ("TEAM_POS_INFO_WINDOW");
		DataCenter.OpenWindow ("TEAM_RIGHT_ROLE_INFO_WINDOW", "ROLE_FATE");
		return true;

	}
}
class Button_go_team_break_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow ("TEAM_POS_INFO_WINDOW");
		DataCenter.OpenWindow ("TEAM_RIGHT_ROLE_INFO_WINDOW", "ROLE_BREAK");
		return true;
	}
}

// 升级技能
public class Button_Upgrade_Skill : CEvent
{

    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("TEAM_POS_INFO_WINDOW");
        DataCenter.OpenWindow("TEAM_RIGHT_ROLE_INFO_WINDOW", "ROLE_SKILL");
        /*
        DataCenter.OpenWindow("SKILL_UPGRADE_WINDOW");
        DataCenter.CloseWindow ("TEAM_POS_INFO_WINDOW");
        DEBUG.Log("ok.");
        */
        return true;
    }
}

