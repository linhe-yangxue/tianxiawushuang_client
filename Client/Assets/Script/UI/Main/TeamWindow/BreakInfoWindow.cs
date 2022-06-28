using UnityEngine;
using System.Collections;
using Logic ;
using DataTable ;
using System.Collections.Generic;
using System.Linq ;
using System;

public enum BASE_ATTRITUBE_TYPE
{
	NONE,
	MAGIC_DEFENCE,      // 法防
	PHYSICAL_DEFENCE,   // 物防
	ATTACK,             // 攻击
	HP,                 // 生命
	MAX,
}

public class BreakInfoWindow : tWindow 
{
	public ActiveData curRoleData;
    public List<ItemDataBase> costItemDataList = new List<ItemDataBase>();
	RoleLogicData roleLogicData;
	ConsumeItemLogicData consumeItemLogicData;
	public ConsumeItemData curConsumeItemData;
	int iBodyTid ;
	public int breakStuffNeed;
	public int breakNeedCoin;
	int iMaxBreakLevel = 0;
    GameObject uiObject;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_break_button", new DefineFactory<Button_break_button>());
		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mBreakLevelConfig.GetAllRecord ())
		{
			if(v.Key != null)
				iMaxBreakLevel++;
		}
        iMaxBreakLevel--;
	}

	public override void Open (object param)
	{
		base.Open (param);
        curRoleData = TeamPosInfoWindow.mCurActiveData;
        iBodyTid = curRoleData.tid;

		consumeItemLogicData = DataCenter.GetData ("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
		curConsumeItemData = consumeItemLogicData.GetDataByTid ((int)ITEM_TYPE.BREAK_STONE);

		roleLogicData = RoleLogicData.Self ;
		Refresh (null);
	}

	public override bool Refresh(object param)
	{
		if(curRoleData.breakLevel < iMaxBreakLevel)
		{
			GameCommon.FindObject (mGameObjUI, "pet_break_info").gameObject.SetActive (true);
			GameCommon.FindObject (mGameObjUI, "last_pet_info").gameObject.SetActive (false);
			GameCommon.FindObject(mGameObjUI, "need_gold").gameObject.SetActive (true);
			GameCommon.FindObject (mGameObjUI, "break_button").GetComponent<UIImageButton>().isEnabled = true;
		}else 
		{
			GameCommon.FindObject (mGameObjUI, "pet_break_info").gameObject.SetActive (false);
			GameCommon.FindObject (mGameObjUI, "last_pet_info").gameObject.SetActive (true);
			GameCommon.FindObject(mGameObjUI, "need_gold").gameObject.SetActive (false);
			GameCommon.FindObject (mGameObjUI, "break_button").GetComponent<UIImageButton>().isEnabled = false;
		}
		UpdaeUI();
		return base.Refresh (param);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch (keyIndex)
		{
		case "SEND_BREAK_MESSAGE":
			SendBreakMessage();
			break;
		case "BREAK_UPGRAGE_SUCCESS":
			object tempItem;
			getData(keyIndex, out tempItem);
			BreakUpgradeSuccess(tempItem as ItemDataBase);
			break;
		}
	}
	public bool BreakCondition()
	{
		bool bIsCan = true;
		breakStuffNeed = TableCommon.GetNumberFromBreakLevelConfig(curRoleData.breakLevel, "NEED_GEM_NUM");
		breakNeedCoin = TableCommon.GetNumberFromBreakLevelConfig(curRoleData.breakLevel, "NEED_COIN_NUM");
		int breakSelfIconNum = GetBreakSelfIconByTid (iBodyTid);

		int breakNeedLevel = TableCommon.GetNumberFromBreakLevelConfig (curRoleData.breakLevel, "ACTIVE_LEVEL");

        int iCount = PetLogicData.Self.GetBreakStuffPetCount(curRoleData);

		if(null == curConsumeItemData)
		{
			//DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_BREAK_NEED_BREAK_STONE);
			DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_PET_BREAK_NEED_BREAK_STONE);
			bIsCan = false;
		}
		else if( breakStuffNeed > curConsumeItemData.itemNum )
		{
			//DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_BREAK_NEED_BREAK_STONE);
			DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_PET_BREAK_NEED_BREAK_STONE);
			bIsCan = false;
		}
		else if(breakNeedCoin > roleLogicData.gold)
		{
            DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
			bIsCan = false;
		}
        else if (breakSelfIconNum > iCount)
		{
			//DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_BREAK_NEED_SELF_ICON);
			DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_PET_BREAK_NEED_SELF_ICON);
			bIsCan = false;
		}
		else if(breakNeedLevel > curRoleData.level)
		{
            //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_UPGRADE_PET_LEVEL_BREAK, breakNeedLevel);
			DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_UPGRADE_PET_LEVEL_BREAK,breakNeedLevel.ToString());
			bIsCan = false;
		}
		
		return bIsCan;
	}

	void SendBreakMessage()
	{
		if(BreakCondition())
		{
            NetManager.RequestBreakUpgrade(curRoleData, costItemDataList);
		}
	}

	void BreakUpgradeSuccess(ItemDataBase itemData)
	{
        MResources mResources = new MResources();
        float breakTime = 1.65f;
        if (uiObject == null)
        {
            uiObject = GameCommon.LoadAndIntanciateUIPrefabs("tupo_animation_window", "CenterAnchor");// mResources.LoadPrefab("Prefabs/UI/tupo_animation_window", "UI");

            GameObject ui = GameCommon.FindUI("CenterAnchor");
            if (uiObject != null)
            {
                uiObject.transform.parent = ui.transform;
                uiObject.transform.localPosition = new Vector3(0, 0, -2000);
                uiObject.transform.localScale = new Vector3(1, 1, 1);
            }

            ActiveBirthForUI birthUI = GameCommon.FindComponent<ActiveBirthForUI>(uiObject, "UIPoint");
            BaseObject character = GameCommon.ShowCharactorModel(birthUI.gameObject, curRoleData.tid, 1.5f);
            character.mMainObject.transform.localPosition = new Vector3(0, -100, 100);
            GlobalModule.DoLater(() => GameObject.Destroy(character.mMainObject), breakTime);
            if (character != null) 
            {
                if (character.mAureoleObj != null) 
                {
                    character.mAureoleObj.SetActive(false);
                }
            }
        }
        else
        {
            uiObject.SetActive(true);
            ActiveBirthForUI birthUI = GameCommon.FindComponent<ActiveBirthForUI>(uiObject, "UIPoint");
            BaseObject character = GameCommon.ShowCharactorModel(birthUI.gameObject, curRoleData.tid, 1.5f);
            character.mMainObject.transform.localPosition = new Vector3(0, -100, 100);
            GlobalModule.DoLater(() => GameObject.Destroy(character.mMainObject), breakTime);
            if (character != null)
            {
                if (character.mAureoleObj != null)
                {
                    character.mAureoleObj.SetActive(false);
                }
            }
        }
        GlobalModule.DoLater(() => { GameObject.Destroy(uiObject); uiObject = null; }, breakTime);

        GlobalModule.DoLater(() => DataCenter.OpenWindow("BREAK_RESULT_WINDOW", curRoleData), breakTime);
        PackageManager.RemoveItem(costItemDataList);
        PackageManager.RemoveItem(itemData);
		PackageManager.RemoveItem((int)ITEM_TYPE.GOLD, -1, breakNeedCoin);

//		breakStuffNeed = TableCommon.GetNumberFromBreakLevelConfig(curRoleData.breakLevel, "NEED_GEM_NUM");
		curRoleData.breakLevel += 1;
		if(curRoleData.breakLevel >= iMaxBreakLevel)
		{
			curRoleData.breakLevel = iMaxBreakLevel;
			GameCommon.FindObject (mGameObjUI, "pet_break_info").gameObject.SetActive (false);
			GameCommon.FindObject (mGameObjUI, "last_pet_info").gameObject.SetActive (true);
			GameCommon.FindObject(mGameObjUI, "need_gold").gameObject.SetActive (false);
			GameCommon.FindObject (mGameObjUI, "break_button").GetComponent<UIImageButton>().isEnabled = false;
            AddBreakChangeTip(curRoleData.breakLevel,ChangeTipValueType.PET_BREAK_FINAL_LEVEL);
		}
        else
        {
            AddBreakChangeTip(curRoleData.breakLevel, ChangeTipValueType.PET_BREAK_LEVEL);
        }
        DataCenter.Set("CHANGE_TIP_PANEL_TYPE",ChangeTipPanelType.BREAK_INFO_WINDOW);
        CommonParam.mIsRefreshPetBreakInfo = false;
        
		UpdaeUI ();

        GlobalModule.DoOnNextLateUpdate(() => 
        {
            TeamInfoWindow _teamInfoWin = DataCenter.GetData("TEAM_INFO_WINDOW") as TeamInfoWindow;
            if (_teamInfoWin != null && _teamInfoWin.IsOpen())
            {
                GameObject _curPetObj = _teamInfoWin.mCardList[(int)TeamManager.mCurTeamPos];
                if (_curPetObj != null)
                {
                    GameObject _aureoleObj = GameCommon.FindObject(_curPetObj, "pet_orange(Clone)");
                    if (_aureoleObj != null)
                    {
                        _aureoleObj.SetActive(false);
                    }
                }
            }
        });



        //added by xuke
        //test
        TeamNewMarkManager.Self.CheckCoin();
        TeamManager.CheckTeamPetBagTab_NewMark();
        TeamNewMarkManager.Self.RefreshTeamNewMark();
        //DataCenter.SetData("TEAM_INFO_WINDOW", "UPDATE_BREAK_NEWMARK", curRoleData);
        //end
	}
    private void AddBreakChangeTip(int kTargetValue,ChangeTipValueType kValueType) 
    {
        string _addLevelInfo = "突破等级 +1";
        ChangeTip _levelTip = new ChangeTip() {Content = _addLevelInfo, TargetValue = kTargetValue,ShowType = LabelShowType.HAS_ADD};
        _levelTip.SetTargetObj(ChangeTipPanelType.BREAK_INFO_WINDOW, kValueType);
        ChangeTipManager.Self.Enqueue(_levelTip, (int)ChangeTipPriority.PET_BREAK_LEVEL);
    }

    public void UpdateNextBreakLevelInfo() 
    {
        GameObject roleObj = GameCommon.FindObject(mGameObjUI, "label_info").gameObject;
        if (roleObj == null)
            return;
        UILabel aimBreakLevel = roleObj.transform.Find("aim_name_label/break_number").GetComponent<UILabel>();
        if (aimBreakLevel == null)
            return; 
        aimBreakLevel.text = "+" + (curRoleData.breakLevel + 1).ToString();
    }
	//set role basic Info
	public void SetRoleBasicInfo()
	{
		GameObject roleObj = GameCommon.FindObject (mGameObjUI, "label_info").gameObject;
		UILabel curNameLabel =  roleObj.transform.Find("cur_name_label").GetComponent<UILabel >();
		curNameLabel.text = GameCommon.GetItemName (iBodyTid);
		UILabel curBreakLevel =  roleObj.transform.Find("cur_name_label/break_number").GetComponent<UILabel >();
        UILabel aimBreakLevel = roleObj.transform.Find("aim_name_label/break_number").GetComponent<UILabel>();
		UILabel aimNameLabel =  roleObj.transform.Find("aim_name_label").GetComponent<UILabel >();
       
        aimNameLabel.text = GameCommon.GetItemName(iBodyTid);
       
        if (CommonParam.mIsRefreshPetBreakInfo)
        {
            if (curRoleData.breakLevel == 0)
            {
                curBreakLevel.text = "";
            }
            else
            {
                curBreakLevel.text = "+" + curRoleData.breakLevel.ToString();
            }
            aimBreakLevel.text = "+" + (curRoleData.breakLevel + 1).ToString();
        }
        else
        {
            if (curRoleData.breakLevel == 1)
            {
                curBreakLevel.text = "";
            }
            else
            {
                curBreakLevel.text = "+" + (curRoleData.breakLevel - 1).ToString();
            }
            aimBreakLevel.text = "+" + (curRoleData.breakLevel).ToString();
        }
		
		UILabel roleLevelLabel =  roleObj.transform.Find("level_label/level_label_num").GetComponent<UILabel >();
		int breakNeedLevel = TableCommon.GetNumberFromBreakLevelConfig (curRoleData.breakLevel, "ACTIVE_LEVEL");
		int chooseColor = curRoleData.level / breakNeedLevel;
		if (chooseColor > 0)
			roleLevelLabel.color = Color.green;
		else {
			roleLevelLabel.color = Color.red;
		}
		roleLevelLabel.text = curRoleData.level + "/" + breakNeedLevel;
	}
	void SetLastInfo()
	{
		GameObject obj = GameCommon.FindObject (mGameObjUI, "last_pet_info").gameObject;
		GameCommon.SetPetIconWithElementAndStar(obj, "item_icon", "", "", iBodyTid);

		UILabel curNameLabel = GameCommon.FindObject (obj, "name_label").GetComponent<UILabel >();
		curNameLabel.text = GameCommon.GetItemName (iBodyTid);
		UILabel curBreakLevel = GameCommon.FindObject (obj, "break_number").GetComponent<UILabel >();
        if (CommonParam.mIsRefreshPetBreakInfo)
        {
            if (curRoleData.breakLevel == 0)
            {
                curBreakLevel.text = "";
            }
            else
            {
                curBreakLevel.text = "+" + curRoleData.breakLevel.ToString();
            }
        }
        else 
        {
            curBreakLevel.text = "+" + (curRoleData.breakLevel - 1).ToString();
        }
	}

	//set role Icon
	public void SetRoleIcon()
	{
		GameObject roleIconObj = GameCommon.FindObject (mGameObjUI, "pet_break_info").gameObject;
		GameObject curRoleIconObj = roleIconObj.transform.Find ("cur_pet_icon").gameObject;
		GameObject aimRoleIconObj = roleIconObj.transform.Find ("aim_pet_icon").gameObject;
		GameCommon.SetPetIconWithElementAndStar(curRoleIconObj, "Background", "", "", iBodyTid);
		GameCommon.SetPetIconWithElementAndStar(aimRoleIconObj, "Background", "", "", iBodyTid);
	}

	public int GetBreakSelfIconByTid(int tid)
	{
		int iItemType = tid / 1000;
		int breakSelfIcon = 0;

		switch ((ITEM_TYPE)iItemType)
		{
		case ITEM_TYPE.CHARACTER:
			breakSelfIcon = 0;
			return breakSelfIcon;
		case ITEM_TYPE.PET:
			breakSelfIcon = TableCommon.GetNumberFromBreakLevelConfig (curRoleData.breakLevel, "ACTIVE_NUM");
			return breakSelfIcon;
		}
		return breakSelfIcon;
	}

	//update need break number
	public void UpdateBreakNeedNum()
	{
        GameObject _stoneIcon = GameCommon.FindObject(mGameObjUI, "stuff_stone_icon");
		GameObject stuffPetIcon = GameCommon.FindObject (mGameObjUI, "stuff_pet_icon").gameObject;
		int breakSelfIconNum = GetBreakSelfIconByTid (iBodyTid);
		List<PetData> petDataList;
        PetLogicData.Self.GetBreakStuffPetList(out petDataList, curRoleData);
		// set need 
		GameObject needGoldNum =  GameCommon.FindObject(mGameObjUI, "need_gold").gameObject;
		UILabel needGlodNumLabel = needGoldNum.transform.Find("need_gold_num").GetComponent<UILabel>();
		UILabel needStuffNumLabel = GameCommon.FindComponent<UILabel> (mGameObjUI, "need_stone_number");
		UILabel needSelfNumLabel = GameCommon.FindComponent<UILabel>(mGameObjUI, "need_self_num");

		if(curRoleData.breakLevel < iMaxBreakLevel)
		{
			breakStuffNeed = TableCommon.GetNumberFromBreakLevelConfig(curRoleData.breakLevel, "NEED_GEM_NUM");
			breakNeedCoin = TableCommon.GetNumberFromBreakLevelConfig(curRoleData.breakLevel, "NEED_COIN_NUM");
		}else
		{
			breakStuffNeed = 0;
			breakNeedCoin = 0;
		}

		if(needGoldNum != null)
		{
			string strText = breakNeedCoin.ToString();
			if(breakNeedCoin > roleLogicData.gold)
			{
				strText = "[ff0000]" + strText;
			}	
			if(needGlodNumLabel != null)
			{
				needGlodNumLabel.text = strText;
			}	
		}

		if(breakSelfIconNum == 0 || curRoleData.breakLevel > iMaxBreakLevel)
		{
			stuffPetIcon.SetActive (false);
		}else 
		{
			stuffPetIcon.SetActive (true);
		}

        if (needSelfNumLabel == null)
            return;

        needSelfNumLabel.text = petDataList.Count + "/" + breakSelfIconNum.ToString();
		if(breakSelfIconNum > petDataList.Count)
		{
            needSelfNumLabel.text = "[ff0000]" + petDataList.Count + "[ffffff]" + "/" + breakSelfIconNum.ToString();
		}

		if(needStuffNumLabel != null)
		{
			if(curRoleData.breakLevel < iMaxBreakLevel)
			{
				if(null == curConsumeItemData)
				{
					needStuffNumLabel.text = "0/" + "[ff0000]" + breakStuffNeed.ToString ();
				}else 
				{
					if(breakStuffNeed > curConsumeItemData.itemNum )
					{
						needStuffNumLabel.text = curConsumeItemData.itemNum.ToString() + "/" + "[ff0000]" + breakStuffNeed.ToString ();
					}else 
					{
						needStuffNumLabel.text = curConsumeItemData.itemNum.ToString() + "/" + breakStuffNeed.ToString ();
					}
				}
			}else
			{
				if(null == curConsumeItemData)
				{
					needStuffNumLabel.text = "0/∞";
				}else 
				{
					needStuffNumLabel.text = curConsumeItemData.itemNum.ToString() + "/∞";
				}
			}
		}

		GameCommon.SetPetIconWithElementAndStar(stuffPetIcon, "Background", "", "", iBodyTid);
		GameCommon.SetUIText (mGameObjUI, "pet_name_stuff_label", GameCommon.GetItemName (iBodyTid) );

        // 增加点击图标显示描述
        GameCommon.BindItemDescriptionEvent(_stoneIcon, (int)ITEM_TYPE.BREAK_STONE);
        GameCommon.BindItemDescriptionEvent(stuffPetIcon, iBodyTid);
	}

    private void BindItemDescriptionEvent(GameObject kTargetObj,int kTid) 
    {
        AddButtonAction(kTargetObj, () => GameCommon.SetAccountItemDetailsWindow(kTid));
    }

	public void SetBreakIcon()
	{
		if(null == curConsumeItemData)
		{
			GameCommon.SetUIText (mGameObjUI,"stone_number_label", "0");
			return;
		}
		GameCommon.SetUIText (mGameObjUI,"stone_number_label", curConsumeItemData.itemNum.ToString ());
	}

	public void SetCurExpInfo()
	{
		GameObject curBaseObj = GameCommon.FindObject(mGameObjUI , "basic_attribute_grid");
		UIGridContainer attuibuteGrid = curBaseObj.GetComponent<UIGridContainer>();
		
		for(int i = 0; i < 4; i++)
		{
			GameObject obj = attuibuteGrid.controlList[i];
			UILabel curAttributeName =  obj.transform.Find("basic_attribute_label1").GetComponent<UILabel >();
						
			AFFECT_TYPE affectType = BreakInfoWindow.GetAttributeType((BASE_ATTRITUBE_TYPE)(i + 1));
			curAttributeName.text = TableCommon.GetStringFromEquipAttributeIconConfig ((int)affectType, "NAME") + ": ";

		}

		setAttackValue(curBaseObj);
		setPhysicalValue (curBaseObj);
		setHPValue (curBaseObj);
		setMagicValue (curBaseObj);
	}
	//攻击
	public void setAttackValue(GameObject obj)
	{
		UILabel attributeValue =  obj.transform.Find("basic_attribute(Clone)_2/basic_attribute_label1/basic_attribute_number_label1").GetComponent<UILabel >();

        // add by LC
        // begin
        int curExpNum = GameCommon.GetTotalAttack(curRoleData);
        curRoleData.breakLevel++;
        int AimExpNum = GameCommon.GetTotalAttack(curRoleData);
        curRoleData.breakLevel--;
        int iAddNum = AimExpNum - curExpNum;

        if (CommonParam.mIsRefreshPetBreakInfo)
        {
            if (curRoleData.breakLevel < iMaxBreakLevel)
            {
                attributeValue.text = curExpNum.ToString() + "  [00FF00]+" + iAddNum.ToString();
            }
            else
            {
                attributeValue.text = curExpNum.ToString();
            }
        }
        else 
        {
            string _suffixInfo = "";
            if (curRoleData.breakLevel < iMaxBreakLevel)
            {
                _suffixInfo = "  [00FF00]+" + iAddNum.ToString();         
            }
            curRoleData.breakLevel--;
            int _lastExpNum = GameCommon.GetTotalAttack(curRoleData);
            attributeValue.text = _lastExpNum.ToString() + _suffixInfo;
            curRoleData.breakLevel++;         
        }
        // end
	}
	//物防
	public void setPhysicalValue(GameObject obj)
	{
		UILabel attributeValue =  obj.transform.Find("basic_attribute(Clone)_1/basic_attribute_label1/basic_attribute_number_label1").GetComponent<UILabel >();

        // add by LC
        // begin

        int curExpNum = GameCommon.GetTotalPhysicalDefence(curRoleData);
        curRoleData.breakLevel++;
        int AimExpNum = GameCommon.GetTotalPhysicalDefence(curRoleData);
        curRoleData.breakLevel--;
        int iAddNum = AimExpNum - curExpNum;

        if (CommonParam.mIsRefreshPetBreakInfo)
        {
            if (curRoleData.breakLevel < iMaxBreakLevel)
            {

                attributeValue.text = curExpNum.ToString() + "  [00FF00]+" + iAddNum.ToString();
            }
            else
            {
                attributeValue.text = curExpNum.ToString();
            }
        }
        else 
        {
            string _suffixInfo = "";
            if (curRoleData.breakLevel < iMaxBreakLevel)
            {
                _suffixInfo = "  [00FF00]+" + iAddNum.ToString();
            }
            curRoleData.breakLevel--;
            int _lastExpNum = GameCommon.GetTotalPhysicalDefence(curRoleData);
            attributeValue.text = _lastExpNum.ToString() + _suffixInfo;
            curRoleData.breakLevel++;   
        }

        // end
	}
	//生命
	public void setHPValue(GameObject obj)
	{
		UILabel attributeValue =  obj.transform.Find("basic_attribute(Clone)_3/basic_attribute_label1/basic_attribute_number_label1").GetComponent<UILabel >();

        // add by LC
        // begin

        int curExpNum = GameCommon.GetTotalMaxHP(curRoleData);
        curRoleData.breakLevel++;
        int AimExpNum = GameCommon.GetTotalMaxHP(curRoleData);
        curRoleData.breakLevel--;
        int iAddNum = AimExpNum - curExpNum;

        if (CommonParam.mIsRefreshPetBreakInfo)
        {
            if (curRoleData.breakLevel < iMaxBreakLevel)
            {

                attributeValue.text = curExpNum.ToString() + "  [00FF00]+" + iAddNum.ToString();
            }
            else
            {
                attributeValue.text = curExpNum.ToString();
            }
        }
        else 
        {
            string _suffixInfo = "";
            if (curRoleData.breakLevel < iMaxBreakLevel)
            {
                _suffixInfo = "  [00FF00]+" + iAddNum.ToString();
            }
            curRoleData.breakLevel--;
            int _lastExpNum = GameCommon.GetTotalMaxHP(curRoleData);
            attributeValue.text = _lastExpNum.ToString() + _suffixInfo;
            curRoleData.breakLevel++;   
        }
      
        // end
	}
	//法防
	public void setMagicValue(GameObject obj)
	{
		UILabel attributeValue =  obj.transform.Find("basic_attribute(Clone)_0/basic_attribute_label1/basic_attribute_number_label1").GetComponent<UILabel >();

        // add by LC
        // begin

        int curExpNum = GameCommon.GetTotalMagicDefence(curRoleData);
        curRoleData.breakLevel++;
        int AimExpNum = GameCommon.GetTotalMagicDefence(curRoleData);
        curRoleData.breakLevel--;
        int iAddNum = AimExpNum - curExpNum;

        if (CommonParam.mIsRefreshPetBreakInfo)
        {
            if (curRoleData.breakLevel < iMaxBreakLevel)
            {

                attributeValue.text = curExpNum.ToString() + "  [00FF00]+" + iAddNum.ToString();
            }
            else
            {
                attributeValue.text = curExpNum.ToString();
            }
        }
        else
        {
            string _suffixInfo = "";
            if (curRoleData.breakLevel < iMaxBreakLevel)
            {
                _suffixInfo = "  [00FF00]+" + iAddNum.ToString();
            }
            curRoleData.breakLevel--;
            int _lastExpNum = GameCommon.GetTotalMagicDefence(curRoleData);
            attributeValue.text = _lastExpNum.ToString() + _suffixInfo;
            curRoleData.breakLevel++;   
        }
        
        // end
	}

	//新增天赋
	public void AddNewTalent()
	{
		GameObject talentNameObj = GameCommon.FindObject (mGameObjUI, "new_talent_root").gameObject;
		//UILabel newTalentName = talentNameObj.transform.Find ("new_talent_name_label").GetComponent<UILabel>();
		UILabel talentNameLabel =talentNameObj.transform.Find ("new_talent_value_label").GetComponent<UILabel>();
		if(curRoleData.breakLevel < iMaxBreakLevel)
		{
			int iBuffTid = TableCommon.GetNumberFromBreakBuffConfig (curRoleData.tid, "BREAK_" + (curRoleData.breakLevel + 1).ToString ());
			string _breadBuffName = String.Empty;
			string _buffInfo = TableCommon.GetStringFromAffectBuffer (iBuffTid, "INFO");
			if (iBuffTid != 0) {
				_breadBuffName = TableCommon.GetStringFromAffectBuffer (iBuffTid, "NAME");
				int _indexOfNum = -1;
				//newTalentName.text = TableCommon.GetStringFromAffectBuffer (iBuffTid, "TIP_TITLE");
				if(!string.IsNullOrEmpty(_buffInfo)){
					_indexOfNum = _buffInfo.IndexOfAny(new char[]{'0','1','2','3','4','5','6','7','8','9'});
				}
				
				if(_indexOfNum != -1)
					talentNameLabel.text = _breadBuffName + _buffInfo.Substring(0,_indexOfNum) + "[00ff00]" + _buffInfo.Substring(_indexOfNum) + "[-]";
			} else {
				talentNameLabel.text = string.Empty;
			}
		}else
		{
			talentNameLabel.text = "【敬请期待】";
		}
	}

	public void UpdaeUI()
	{
		if(curRoleData.breakLevel < iMaxBreakLevel)
		{
			SetRoleBasicInfo();
			SetRoleIcon();
		}else
		{
			SetLastInfo();
		}
		AddNewTalent();
		SetCurExpInfo();
//		SetBreakIcon();
		UpdateBreakNeedNum();

        CommonParam.mIsRefreshPetBreakInfo = true;
	}

	public static AFFECT_TYPE GetAttributeType(BASE_ATTRITUBE_TYPE type)
	{
		if (type >= BASE_ATTRITUBE_TYPE.MAGIC_DEFENCE && type <= BASE_ATTRITUBE_TYPE.HP)
		{
			string name = type.ToString();
			return GameCommon.GetEnumFromString<AFFECT_TYPE>(name, AFFECT_TYPE.NONE);
		}
		return AFFECT_TYPE.NONE;
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

//突破button
class Button_break_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData("BREAK_INFO_WINDOW", "SEND_BREAK_MESSAGE",true);
		return true ;
	}
}