using UnityEngine;
using System.Collections;
using Logic ;
using DataTable ;
using System.Collections.Generic;
using System.Linq ;
using Utilities;

public class EquipStrengthenInfoWindow :tWindow 
{
	RoleEquipLogicData roleEquipLogicData;
	public EquipData curSelEquipData;
	RoleLogicData logicData;

	public Dictionary<int, StrengthenEquipData> mDicSelEquip = new Dictionary<int, StrengthenEquipData>();

	public int strengthNeedCoin = 0;
	GameObject equipStrengthenInfo;

	List<EquipData> equipList = new List<EquipData>();
	int curMasterLevel = 0;

	public override void Init ()
	{
		EventCenter.Register ("Button_equip_strengthen_ok_button", new DefineFactory<Button_equip_strengthen_ok_button>());
		EventCenter.Register ("Button_equip_strengthen_five_ok_button", new DefineFactory<Button_equip_strengthen_five_ok_button>());
//		EventCenter.Register ("Button_close_equip_strengthen_info_window", new DefineFactory<Button_close_equip_strengthen_info_window>());
	}

	public override void Open (object param)
	{
        base.Open(param);
        mGameObjUI.StopAllCoroutines();
        if (GetComponent<TweenAlpha>("strengthen_result") != null && GetComponent<TweenScale>("strengthen_result") != null && GetComponent<TweenRotation>("chui") != null)
        {
            GetComponent<TweenAlpha>("strengthen_result").ResetToBeginning();
            GetComponent<TweenScale>("strengthen_result").ResetToBeginning();
            GetComponent<TweenRotation>("chui").ResetToBeginning();
        }
		equipList.Clear();
		for (int i = 0; i < 4; i++) 
		{
			var equip = TeamManager.GetRoleEquipDataByCurTeamPos(i);
			if (equip != null) equipList.Add(equip);
		}
		curMasterLevel =GameCommon.GetMinMosterLevel("EQUIP_STERNG_LEVEL", equipList, GetMinStrengthLevel(equipList));
        mGameObjUI.StopAllCoroutines();
        curSelEquipData = param as EquipData;//TeamManager.GetRoleEquipDataByTeamPos (curEquipId, curEquipType);
		equipStrengthenInfo = GameCommon.FindObject(mGameObjUI, "EquipStrengthenInfoWindow").gameObject;
        GameObject chui = equipStrengthenInfo.transform.Find("chui").gameObject;
        chui.SetActive(false);
        GameObject equipStrengthen = equipStrengthenInfo.transform.Find("strengthen_result").gameObject;
        equipStrengthen.SetActive(false);
        GameObject texiao = equipStrengthenInfo.transform.Find("ui_weapon_qianghua").gameObject;
        texiao.SetActive(false);
		logicData =  RoleLogicData.Self;
		Refresh (param);
		//刷新强化按钮
		UpdateStrengthButton ();
		SetVisible ("equip_strengthen_tips", false );
	}
	int GetMinStrengthLevel(List<EquipData> _equipList)
	{
		int iMinLevel = 100;
		foreach(var v in _equipList)
		{
			if(v.strengthenLevel < iMinLevel)
				iMinLevel = v.strengthenLevel;
		}
		return iMinLevel;
	}

	public override bool Refresh(object param)
	{
		UpdateUI();
		return base.Refresh (param);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch (keyIndex)
		{
		case "SEND_EQUIP_STRENGTHEN_MESSAGE":
			SendEquipStrengthenMessage((int)objVal);
			break;
		case "EQUIP_STRENGTHEN_SUCCESS":
			EquipStrengthenSuccess((string)objVal);
			break ;
		}
	}
	
	public bool EquipStrengthenCondition()
	{
		bool bIsCan = true;
		if(curSelEquipData.strengthenLevel >= RoleLogicData.Self.character.level * 2  ) 
		{
            DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_ROLE_EQUIP_STRENGTHEN_LEVEL_MAX);			
			bIsCan = false;
		}
		else if(strengthNeedCoin > logicData.gold)
		{
            DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
			bIsCan = false;
			UpdateStrengthButton ();
		}
		return bIsCan;
	}
	void SendEquipStrengthenMessage(int iCount)
	{
        GameCommon.FindObject(mGameObjUI, "equip_strengthen_ok_button").GetComponent<UIImageButton>().isEnabled = false;
        GameCommon.FindObject(mGameObjUI, "equip_strengthen_five_ok_button").GetComponent<UIImageButton>().isEnabled = false;
		if(EquipStrengthenCondition())
		{
			int iItemId = curSelEquipData.itemId;
			int iTid = curSelEquipData.tid;
			NetManager.RequestEquipStrengthen(iItemId, iTid, iCount);
		}
	}
    private int mStrengthenAddLevel = 0;    //> 强化增加的等级
	void EquipStrengthenSuccess(string text)
	{
		SC_StrengthenEquip item = JCode.Decode<SC_StrengthenEquip>(text);
		if( null == item )
			return ;

		int iAddLevel = item.upgradeLevel;
		int iCostCoin = item.costGold;
		int iCritVipTimes = TableCommon.GetNumberFromVipList (RoleLogicData.Self.vipLevel, "STR_CRI_LV");
		int iCritCount = (item.upgradeLevel - item.succTimes) / ( iCritVipTimes - 1);
		PackageManager.RemoveItem((int)ITEM_TYPE.GOLD, -1, iCostCoin);
//		logicData.gold -= iCostCoin;

		curSelEquipData.strengthenLevel += iAddLevel;
        //added by xuke  本地维护装备强化费用
        curSelEquipData.strengCostGold += iCostCoin;
        DataCenter.SetData("PACKAGE_EQUIP_WINDOW", "REFRESH_EQUIP_BAG_GROUP", null);
        //end
        mStrengthenAddLevel = iAddLevel;
        CommonParam.mIsRefreshEquipStrengthenInfo = false;
        UpdateUI();
        if (GameObject.Find("team_info_window") != null)
        {
            GameObject mteamInfoWindow = GameCommon.FindObject(GameObject.Find("UI Root"), "team_info_window");
            UIGridContainer mTeamInfoGrid = GameCommon.FindComponent<UIGridContainer>(mteamInfoWindow, "team_info_grid");
            GameObject obj = mTeamInfoGrid.controlList[curSelEquipData.teamPos];
            UIGridContainer itemIconGrid = GameCommon.FindComponent<UIGridContainer>(obj, "equip_info_grid");
            int packageEquipType = TableCommon.GetNumberFromRoleEquipConfig(curSelEquipData.tid, "EQUIP_TYPE");
            GameObject itemObj = itemIconGrid.controlList[packageEquipType - 1];

            GameObject mlevel = GameCommon.FindObject(itemObj, "level_bg");

            GameCommon.FindObject(mlevel, "level_label").SetActive(true);
            GameCommon.FindObject(mlevel, "level_label").GetComponent<UILabel>().text = curSelEquipData.strengthenLevel.ToString();
            GameCommon.FindObject(mlevel, "level_label").GetComponent<UILabel>().fontSize = 16;
        }
		UpdateEquipStrengthenTips(item.succTimes, iAddLevel);
        //added by xuke 装备强化成功后刷新红点提示
        TeamNewMarkManager.Self.CheckCoin();
        TeamNewMarkManager.Self.RefreshTeamNewMark();
        //end

		TeamInfoWindow _tWindow = DataCenter.GetData("TEAM_INFO_WINDOW") as TeamInfoWindow;
		if(equipList.Count >= 4)
		{
			int aa = GameCommon.GetMinMosterLevel("EQUIP_STERNG_LEVEL", equipList, GetMinStrengthLevel(equipList));
			if (aa > curMasterLevel)
			{
				if(_tWindow != null)
				{
					string str = "[ff9900]装备强化大师[-] [99ff66]{0}[-] 级达成！" ;
					_tWindow.SetTipsMaster(str, aa);
					curMasterLevel = aa;
				}
			}
		}
		if(_tWindow != null)
		{
			ActiveData _tmp = TeamManager.GetActiveDataByTeamPos (curSelEquipData.teamPos);
//			GlobalModule.DoCoroutine(_tWindow.SetTipsAttribute(_tmp));
			AddStrengthenChangeTip(curSelEquipData,iAddLevel, iCritCount);
			_tWindow.SetTipsAttribute(_tmp);
			_tWindow.ChangeFitting();

            ChangeTipManager.Self.PlayAnim(() => { SetAimExpInfo(); });
		}
	}
    /// <summary>
    /// 添加强化成功的文字反馈效果
    /// </summary>
    /// <param name="kCurEquipData"></param>
    /// <param name="kAddLevel"></param>
	private void AddStrengthenChangeTip(EquipData kCurEquipData,int kAddLevel, int iCritCount) 
    {
        //强化等级
		string _addLevelInfo ="暴击 [99ff66]" + iCritCount + " [ff9900]次，" + "强化等级 +" + kAddLevel;
		if(iCritCount == 0)
			_addLevelInfo ="强化等级 +" + kAddLevel;
        ChangeTip _levelTip = new ChangeTip() {Content = _addLevelInfo ,TargetValue = kCurEquipData.strengthenLevel};
        _levelTip.SetTargetObj(ChangeTipPanelType.EQUIP_STRENGTHEN_INFO_WINDOW,ChangeTipValueType.EQUIP_STRENGTHEN_LEVEL);
        ChangeTipManager.Self.Enqueue(_levelTip, (int)ChangeTipPriority.STRENGTHEN_LEVEL);
        //属性变化
        for (int i = 0; i < 1; i++) 
        {
            int iAttributeType = TableCommon.GetNumberFromRoleEquipConfig(kCurEquipData.tid, "ATTRIBUTE_TYPE_" + i.ToString());
            string _attrName = TableCommon.GetStringFromEquipAttributeIconConfig(iAttributeType, "NAME");
            float _targetValue = RoleEquipData.GetEquipBaseAttributeValue(i, kCurEquipData.tid, kCurEquipData.strengthenLevel);
            float _beforeValue = RoleEquipData.GetEquipBaseAttributeValue(i, kCurEquipData.tid, (kCurEquipData.strengthenLevel - kAddLevel));
            float _addAttrValue = _targetValue - _beforeValue;

            string strAttributeType = GameCommon.ToAffectTypeString((AFFECT_TYPE)iAttributeType);
            bool bIsRate = strAttributeType.LastIndexOf("_RATE") > 0;

            _addAttrValue = _addAttrValue / 10000;
            string strValue = "";
            if (bIsRate)
            {
                _addAttrValue *= 100;
                int iInteger = (int)_addAttrValue;
                int iDecimal = (int)((_addAttrValue - iInteger) * 100);

                string strDecimal = iDecimal.ToString();
                if (iDecimal < 10)
                    strDecimal = "0" + strDecimal;
                strValue = iInteger.ToString() + "." + strDecimal + "%";
            }
            else
            {
                _addAttrValue = (float)((int)_addAttrValue);
                strValue = _addAttrValue.ToString();
            }

            float _showTargetValue = bIsRate ? _targetValue / 100 : _targetValue / 10000;

            string _addAttrInfo = _attrName + " +" + strValue;
            ChangeTip _strengthAttrTip = new ChangeTip() { Content = _addAttrInfo, TargetValue = _showTargetValue };
            _strengthAttrTip.SetTargetObj(ChangeTipPanelType.EQUIP_STRENGTHEN_INFO_WINDOW, ChangeTipValueType.EQUIP_STRENGTHEN_ATTR_BASE + i + 1);
            ChangeTipManager.Self.Enqueue(_strengthAttrTip, (int)ChangeTipPriority.STRENGTHEN_ATTR);
        }
    }

	//Update equip strength window UI
	public void UpdateUI()
	{
		SetEquipName();
		SetRoleEquipIcon();
        SetCurExpInfo();
        SetAimExpInfo();     
        CommonParam.mIsRefreshEquipStrengthenInfo = true;
		UpdateNeedCoinNum();
	}

	public void UpdateEquipStrengthenTips(int iCount, int iLevel)
	{
        mGameObjUI.StopAllCoroutines();
        GetSub("chui").SetActive(false);
        GetSub("strengthen_result").SetActive(false);
        if (GetComponent<TweenAlpha>("strengthen_result") != null && GetComponent<TweenScale>("strengthen_result") != null && GetComponent<TweenRotation>("chui") != null)
        {
            GetComponent<TweenAlpha>("strengthen_result").ResetToBeginning();
            GetComponent<TweenScale>("strengthen_result").ResetToBeginning();
            GetComponent<TweenRotation>("chui").ResetToBeginning();
        }
        GameObject chui = equipStrengthenInfo.transform.Find("chui").gameObject;       
        chui.SetActive(true);
//		int critNum = 0;		 
       
        mGameObjUI.StartCoroutine(a());        
       // GlobalModule.DoCoroutine(a());
       // GameObject texiao = equipStrengthenInfo.transform.Find("ui_weapon_qianghua").gameObject;
        
            //TweenPosition.Begin(equipStrengthen, 1.0f, new Vector3(-60, -117, 0));
        // TweenScale twScale =
        mGameObjUI.StartCoroutine(ShowEquipStrengthenResult(iCount, iLevel));
        mGameObjUI.StartCoroutine(ShowTexiao());
        //GlobalModule.DoLater(() => ShowEquipStrengthenResult(iCount, iLevel), 0.45f);
        //GlobalModule.DoLater(() =>texiao.SetActive(true), 0.27f);
         //GlobalModule.DoLater(() =>texiao.SetActive(false),0.83f);
        
	}

    IEnumerator ShowTexiao()
    {
        yield return new WaitForSeconds(0.27f);
        GameObject texiao = equipStrengthenInfo.transform.Find("ui_weapon_qianghua").gameObject;
        texiao.SetActive(true);
        yield return new WaitForSeconds(0.56f);
        texiao.SetActive(false);
    }

    IEnumerator ShowEquipStrengthenResult(int iCount, int iLevel)
    {
        yield return new WaitForSeconds(0.45f);
        GameObject equipStrengthen = equipStrengthenInfo.transform.Find("strengthen_result").gameObject;
        equipStrengthen.SetActive(true);

        float fLevelAddValue = (float)TableCommon.GetNumberFromRoleEquipConfig(curSelEquipData.tid, "STRENGTHEN_0");
        int geweishu = iLevel % 10;
        int shiweishu = iLevel/10;
        if (iLevel <= iCount)
        {
//            equipStrengthen.transform.Find("strengthen_type").GetComponent<UISprite>().spriteName = "a_ui_texiaoqianghua";    
			equipStrengthen.transform.Find("strengthen_type").GetComponent<UISprite>().spriteName = "";  
        }
        else
        {
            equipStrengthen.transform.Find("strengthen_type").GetComponent<UISprite>().spriteName = "a_ui_texiaobaoji";           
        }
        if (shiweishu > 0)
        {
            equipStrengthen.transform.Find("dataone").gameObject.SetActive(true);
            equipStrengthen.transform.Find("dataten").GetComponent<UISprite>().spriteName = "a_ui_texiao_" + shiweishu.ToString();
            equipStrengthen.transform.Find("dataone").GetComponent<UISprite>().spriteName = "a_ui_texiao_" + geweishu.ToString();
        }
        else
        {
            equipStrengthen.transform.Find("dataone").gameObject.SetActive(false);
            equipStrengthen.transform.Find("dataten").GetComponent<UISprite>().spriteName = "a_ui_texiao_" + geweishu.ToString();

        }
        TweenScale tw1 = TweenScale.Begin(equipStrengthen,0.3f, new Vector3(1.2f, 1.2f, 0));
//        tw1.ResetToBeginning();
        mGameObjUI.StartCoroutine(b());
 
    }
    IEnumerator a()
    {
        GameObject chui = equipStrengthenInfo.transform.Find("chui").gameObject;
        TweenRotation twrotation1 = TweenRotation.Begin(chui, 0.2f, Quaternion.AngleAxis(-45, Vector3.forward));
        twrotation1.ResetToBeginning();
        yield return new WaitForSeconds(0.2f);        
        TweenRotation twrotation2 = TweenRotation.Begin(chui, 0.15f, Quaternion.AngleAxis(-45, Vector3.back));
        twrotation2.ResetToBeginning();
        twrotation2.onFinished.Add(new EventDelegate(ChuiTweenOver));

    }
    IEnumerator b()
    {
        yield return new WaitForSeconds(0.3f);
        GameObject equipStrengthen = equipStrengthenInfo.transform.Find("strengthen_result").gameObject;
        TweenScale tw2 = TweenScale.Begin(equipStrengthen, 0.15f, new Vector3(1f, 1f, 0));
//        tw2.ResetToBeginning();
        mGameObjUI.StartCoroutine(c());
        
    }
    IEnumerator c() 
    {
        yield return new WaitForSeconds(0.3f);
        GameObject equipStrengthen = equipStrengthenInfo.transform.Find("strengthen_result").gameObject;
        TweenAlpha twalpha = TweenAlpha.Begin(equipStrengthen, 0.2f,0);
//        twalpha.ResetToBeginning();
        twalpha.onFinished.Add(new EventDelegate(TweenOver));
    }
	void TweenOver()
	{
        GameObject equipStrengthen = equipStrengthenInfo.transform.Find("strengthen_result").gameObject;
        equipStrengthen.transform.localPosition = new Vector3(-14f, 10f, 0);
		equipStrengthen.transform.localScale = new Vector3(0,0,0);
        
		equipStrengthen.SetActive (false);
        equipStrengthen.transform.GetComponent<UISprite>().alpha = 1;
//        GameCommon.FindObject(mGameObjUI, "equip_strengthen_ok_button").GetComponent<UIImageButton>().isEnabled = true;
//        GameCommon.FindObject(mGameObjUI, "equip_strengthen_five_ok_button").GetComponent<UIImageButton>().isEnabled = true;
		//刷新强化按钮
		UpdateStrengthButton ();
	}
    void ChuiTweenOver()
    {
        GameObject chui = equipStrengthenInfo.transform.Find("chui").gameObject;
        chui.transform.localEulerAngles = Vector3.zero;
        chui.SetActive(false);

    }
	public void SetStrengthenLevel(GameObject obj, int iStrengthenLevel)
	{
		// set level
		GameObject level = obj.transform.Find("LevelLabel").gameObject;
		UILabel levelLabel = level.GetComponent<UILabel>();
		if (levelLabel != null)
		{
            string _colorFlag = "";
            _colorFlag = iStrengthenLevel > RoleLogicData.Self.character.level * 2 ? "[ff0000]" : "[00ff00]";
            levelLabel.text = _colorFlag + iStrengthenLevel.ToString() + " / " + (RoleLogicData.Self.character.level * 2).ToString() + "[-]";
		}
	}

	public UILabel GetAttributeNumLabel(GameObject obj, int iAttributeType)
	{
		if (curSelEquipData == null || obj == null
		    || iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
			return null;
		
		string strAttributeType = GameCommon.ToAffectTypeString((AFFECT_TYPE)iAttributeType);
		bool bIsRate = strAttributeType.LastIndexOf("_RATE") > 0;
		
		GameObject number = obj.transform.Find("num_label").gameObject;
		return number.GetComponent<UILabel>();
	}

	public void SetRoleEquipIcon()
	{
		GameObject obj = equipStrengthenInfo.transform.Find ("equip_icon_group").gameObject ;

		// set role equip icon
		GameCommon.SetEquipIcon(obj, curSelEquipData.tid );
	}

	public void SetEquipName()
	{
        string _colorPrefix = GameCommon.GetEquipTypeColor(TableCommon.GetNumberFromRoleEquipConfig(curSelEquipData.tid, "QUALITY"));
		GameObject equipNameObj = equipStrengthenInfo.transform.Find ( "equip_name_label").gameObject ;
		UILabel equipNameLabel = equipNameObj.GetComponent<UILabel>();
        equipNameLabel.text = _colorPrefix + TableCommon.GetStringFromRoleEquipConfig(curSelEquipData.tid, "NAME") + "[-]";

        GameCommon.FindComponent<UILabel>(equipNameObj, "type_label").text = _colorPrefix + GameCommon.GetEquipTypeDesc(TableCommon.GetNumberFromRoleEquipConfig(curSelEquipData.tid, "EQUIP_TYPE")) + "[-]";
        GameCommon.FindComponent<UILabel>(equipNameObj, "point_label").text = _colorPrefix + "·" + "[-]";
    }

	public int GetNeedSyntheticCoinNum()
	{
		int iNeedSyntheticCoin = 0;
		
		string costQualityType = "COST_MONEY_" + (int)curSelEquipData.mQualityType;
		int costMoney = TableCommon.GetNumberFromEquipStrengthCostConfig (curSelEquipData.strengthenLevel, "COST_MONEY");
		//计算品质额外花费
		int costMoneyQualityType = TableCommon.GetNumberFromEquipStrengthCostConfig(curSelEquipData.strengthenLevel , costQualityType);
		iNeedSyntheticCoin = costMoney + costMoneyQualityType;
		return iNeedSyntheticCoin;
	}

	public void UpdateStrengthButton()
	{
//		if (GetNeedSyntheticCoinNum () < RoleLogicData.Self.gold) {
			GameCommon.FindObject (mGameObjUI, "equip_strengthen_ok_button").GetComponent<UIImageButton> ().isEnabled = true;
			GameCommon.FindObject (mGameObjUI, "equip_strengthen_five_ok_button").GetComponent<UIImageButton> ().isEnabled = true;
//		} else {
//		GameCommon.FindObject (mGameObjUI, "equip_strengthen_ok_button").GetComponent<UIImageButton> ().isEnabled = false;
//		GameCommon.FindObject (mGameObjUI, "equip_strengthen_five_ok_button").GetComponent<UIImageButton> ().isEnabled = false;
//		}
	}

	//update need gold number
	public void UpdateNeedCoinNum()
	{
		strengthNeedCoin = GetNeedSyntheticCoinNum();

		// set need gold
		GameObject needGoldNum =  GameCommon.FindObject(mGameObjUI,"need_gold_num");
		if(needGoldNum != null)
		{
			string strText = strengthNeedCoin.ToString();
			if(strengthNeedCoin > logicData.gold)
			{
				strText = "[ff0000]" + strText;
			}
			
			UILabel needGlodNumLabel = needGoldNum.GetComponent<UILabel>();
			if(needGlodNumLabel != null)
			{
				needGlodNumLabel.text = strText;
			}	
		}
	}
	public void SetCurExpInfo()
	{
		// cur base attribute
		GameObject srcBaseObj = GameCommon.FindObject(mGameObjUI , "cur_attribute_group");
        if (!CommonParam.mIsRefreshEquipStrengthenInfo)
        {
            SetBaseAttribute("cur_attribute_", curSelEquipData.strengthenLevel - mStrengthenAddLevel);
            SetStrengthenLevel(srcBaseObj, curSelEquipData.strengthenLevel - mStrengthenAddLevel);
        }
        else 
        {
            SetBaseAttribute("cur_attribute_", curSelEquipData.strengthenLevel);
            SetStrengthenLevel(srcBaseObj, curSelEquipData.strengthenLevel);
        }
	}

	public void SetAimExpInfo()
	{
		int aimStrengthenLevel = 0;
		// aim base attribute
		GameObject aimBaseObj = GameCommon.FindObject(mGameObjUI , "aim_attribute_group");
        if (!CommonParam.mIsRefreshEquipStrengthenInfo)
        {
            aimStrengthenLevel = curSelEquipData.strengthenLevel + 1 - mStrengthenAddLevel;
        }
        else
        {
            aimStrengthenLevel = curSelEquipData.strengthenLevel + 1;
        }

		SetBaseAttribute("aim_attribute_", aimStrengthenLevel);
		SetStrengthenLevel(aimBaseObj, aimStrengthenLevel);
	}
	
	public void SetBaseAttribute(string baseObjName, int iStrengthenLevel)
	{
		for (int i = 0; i < 1; i++)
		{
			SetBaseAttribute(i, baseObjName, iStrengthenLevel);
		}
	}
	//set Base attribute infos
	public void SetBaseAttribute(int iIndex, string baseObjName, int iStrengthenLevel)
	{
		if (curSelEquipData == null)
			return;
		
		int iAttributeType = TableCommon.GetNumberFromRoleEquipConfig(curSelEquipData.tid, "ATTRIBUTE_TYPE_" + iIndex.ToString());
		if (iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
			return;
		
		GameObject baseObj = GameCommon.FindObject(mGameObjUI , baseObjName + iIndex.ToString());
		// set base attribute name
		SetAttributeName(baseObj, iAttributeType);
		// set base attribute number
		SetBaseAttributeValue(iIndex, baseObj, iAttributeType, iStrengthenLevel);
	}

	// set base attribute name
	public void SetAttributeName(GameObject obj, int iAttributeType)
	{
		if (obj == null || iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
			return;
		
		GameObject name = obj.transform.Find("name").gameObject;
		UILabel nameLabel = name.GetComponent<UILabel>();
		if (nameLabel != null)
			nameLabel.text = TableCommon.GetStringFromEquipAttributeIconConfig (iAttributeType, "NAME");
	}

	// set base attribute number
	public void SetBaseAttributeValue(int iIndex, GameObject obj, int iAttributeType, int iStrengthenLevel)
	{
		UILabel numLabel = GetAttributeNumLabel(obj, iAttributeType);
		float fValue = 0.0f;
		if (numLabel != null)
		{
			if (curSelEquipData != null)
			{
//				float fBaseValue = (float)TableCommon.GetNumberFromRoleEquipConfig(curSelEquipData.mModelIndex, "BASE_ATTRIBUTE_" + iIndex.ToString());
//				float fLevelAddValue = (float)TableCommon.GetNumberFromRoleEquipConfig(curSelEquipData.mModelIndex, "STRENGTHEN_" + iIndex.ToString());
//				// 基础属性数值 + (强化等级-1) * 强化属性加成值
//				fValue = fBaseValue + (iStrengthenLevel - 1.0f )* fLevelAddValue;
				fValue = RoleEquipData.GetEquipBaseAttributeValue(iIndex, curSelEquipData.tid, iStrengthenLevel);
				SetAttributeValueUI(numLabel, iAttributeType, fValue);
			}
		}
	}
	public void SetAttributeValueUI(UILabel numLabel, int iAttributeType, float fValue)
	{
		if (curSelEquipData == null || numLabel == null
		    || iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
			return;
		
		string strAttributeType = GameCommon.ToAffectTypeString((AFFECT_TYPE)iAttributeType);
		bool bIsRate = strAttributeType.LastIndexOf("_RATE") > 0;
		
		fValue = fValue / 10000;
		string strValue = "";
		if (bIsRate)
		{
			fValue *= 100;
			int iInteger = (int)fValue;
			int iDecimal = (int)((fValue - iInteger) * 100) ;
			
			string strDecimal = iDecimal.ToString();
			if(iDecimal < 10)
				strDecimal = "0" + strDecimal;
			strValue = iInteger.ToString() + "." + strDecimal + "%";
		}
		else
		{
			fValue = (float)((int)fValue);
			strValue = fValue.ToString();
		}
		numLabel.text = strValue;
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

//强化1次
class Button_equip_strengthen_ok_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("EQUIP_STRENGTHEN_INFO_WINDOW", "SEND_EQUIP_STRENGTHEN_MESSAGE", 1);
		return true ;
	}
}
//强化5次
class Button_equip_strengthen_five_ok_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("EQUIP_STRENGTHEN_INFO_WINDOW", "SEND_EQUIP_STRENGTHEN_MESSAGE", 5);
		return true ;
	}
}
////关闭强化界面
//class Button_close_equip_strengthen_info_window : CEvent
//{
//	public override bool _DoEvent ()
//	{
//		DataCenter.CloseWindow ("EQUIP_STRENGTHEN_INFO_WINDOW");
//		DataCenter.CloseWindow ("EQUIP_INFO_WINDOW");
////		DataCenter.OpenWindow ("TEAM_DATA_WINDOW");
//
//		return true ;
//	}
//}
