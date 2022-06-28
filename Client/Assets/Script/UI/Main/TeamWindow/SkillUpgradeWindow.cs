using UnityEngine;
using System.Collections.Generic;
using Logic;
using DataTable;

public class Button_skill_up_button : CEvent {
	
	public override bool _DoEvent()
	{
		// send level up request
		int selId = NewSKillUpgradeBean.soleSkillIndex;
		int itemId = TeamPosInfoWindow.mCurActiveData.itemId;

		int _skillLv = TeamPosInfoWindow.mCurActiveData.skillLevel[selId];

        //技能等级不能高于符灵等级
        if (TeamPosInfoWindow.mCurActiveData.level <= _skillLv)
        {
            DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_SKILL_LARGER_THAN_PET_LV);
            return true;
        }

        int moneyCost = TableCommon.GetNumberFromSkillCost(_skillLv, "MONEY_COST");
        Debug.Log("moneyCost = " + moneyCost + "gold = " + RoleLogicData.Self.gold);
        if(RoleLogicData.Self.gold < moneyCost)
        {
            DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
            return true;
        }

		int _costBookNum = TableCommon.GetNumberFromSkillCost (_skillLv, "SKILL_BOOK_COST");
		ConsumeItemLogicData itemData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
		int _hasBookNum = itemData.GetDataByTid((int)ITEM_TYPE.SKILL_BOOK).itemNum;
		if (_hasBookNum < _costBookNum) 
		{
			DataCenter.ErrorTipsLabelMessage("技能秘籍不足");
			return true;
		}


		SkillNetHandle.RequestSkillLevelUp(itemId, selId, RequestLevelUpSuccess, RequestLevelUpFail);
		DEBUG.Log("SoleId:" + selId);

		return true;
	}

	void RequestLevelUpSuccess(string text) {
		SC_RequestSkillLevelUp lv = JCode.Decode<SC_RequestSkillLevelUp>(text);
		if(lv.ret == (int)STRING_INDEX.ERROR_NONE) {
			// 提示框，升级成功 , refresh
			// - coin skillbooks update level
            DataCenter.ErrorTipsLabelMessage("升级成功！");
			DataCenter.SetData ("SKILL_UPGRADE_WINDOW", "UPDATA_ITEM_NUM", true);
			DEBUG.Log("level up sucess, " + text);
            //added by xuke
            TeamNewMarkManager.Self.CheckCoin();
            TeamNewMarkManager.Self.RefreshTeamNewMark();
            //end
		}

	}

	void RequestLevelUpFail(string text) {
		int errCode = 0;
		string errText = "";

		int.TryParse(text, out errCode);

		if (errCode == 1501) 
			errText = "技能等级已达上限";
		else if (errCode == 1502) 
			errText = "没有技能秘籍";
		else if (errCode == 1101)
			errText = "用户银币不够";
		else if (errCode == 1111)
			errText = "技能秘籍不足";
		else
			errText = "errCode:" + text;

		DataCenter.ErrorTipsLabelMessage(errText);
		DataCenter.CloseMessageWindow();
		//SC_RequestSkillLevelUp
		DEBUG.Log("level up err, " + text);
	}
}

/*
 * public UISprite matIcon;
	public UILabel matName;
	public UILabel matNum;
	public UILabel coin;

public UILabel skillName;
	public UILabel currentSkillLevel;
	public UILabel nextSkillLevel;
	public UILabel skillDesc;
	public string skillDescStr;
 */

public class SkillUpgradePanelInfoData {
	public string skillName;
	public int currentSkillLevel;
	public int nextSkillLevel;
	public string skillDesc;
	public string matIcon;
	public string matName;
	public string matRateNum;
	public int bookCost;
	public int coin;
    public int openLevel;
    public int petBreakLevel;
}

public class SkillUpgradeWindow : tWindow {

	NewSKillUpgradeBean [] skills;
	public override void Init() {
		base.Init();

		EventCenter.Self.RegisterEvent("Button_skill_up_button", new DefineFactory<Button_skill_up_button>());
	}

	public override void Open(object param) {

        base.Open(param);

        // 关闭窗口时默认选择第一个技能
        NewSKillUpgradeBean.soleSkillIndex = 0;
        if(skills != null && skills.Length > 0)    skills[NewSKillUpgradeBean.soleSkillIndex].OnClick();
        GameObject obj = GameCommon.FindObject(mGameObjUI, "Button1(Clone)_0");
        if (obj)
        {
            UIToggle toggle = obj.GetComponent<UIToggle>();
            toggle.value = true;
        }

		Refresh(null);
        GameObject _skillIconObj = GetSub("stuff_stone_icon");
        // By XiaoWen
        // Begin
        GameCommon.SetUIText(_skillIconObj, "stone_stuff_label", GameCommon.GetItemName((int)ITEM_TYPE.SKILL_BOOK));
        // End
        GameCommon.BindItemDescriptionEvent(_skillIconObj,(int)ITEM_TYPE.SKILL_BOOK);
    }

	public override void Close() {
		base.Close();
	}

    public override void OnClose()
    {
        base.OnClose();
    }

	public override void onChange(string keyIndex, object objVal) {
		base.onChange(keyIndex, objVal);

		switch(keyIndex) {
			case"UPDATA_ITEM_NUM":
				ItemDataBase coinCost = new ItemDataBase();
				coinCost.tid = (int)ITEM_TYPE.GOLD;
				coinCost.itemNum = skills[NewSKillUpgradeBean.soleSkillIndex].supi.coin;
				
				ItemDataBase skillBookCost = new ItemDataBase();
				skillBookCost.tid = (int)ITEM_TYPE.SKILL_BOOK;
				skillBookCost.itemNum = skills[NewSKillUpgradeBean.soleSkillIndex].supi.bookCost;
				
				PackageManager.RemoveItem(coinCost);
				PackageManager.RemoveItem(skillBookCost);

				ActiveData ad = TeamPosInfoWindow.mCurActiveData;
				ad.skillLevel[NewSKillUpgradeBean.soleSkillIndex] += 1; 

				Refresh(null);
				break;
		}


	}

	public override bool Refresh(object param) { 

		UpdateUI();

		return true; 
	}

    // 根据技能ID来设置技能UI信息
    private void SetSkillUIInfo(NewSKillUpgradeBean kSkillBean,int kSkillPosID) 
    {
        int playLevel = TeamPosInfoWindow.mCurActiveData.breakLevel;    //已修改为获取符灵突破等级
        string _dataReadIndex = (kSkillPosID + 1).ToString();
        int _SkillID = TableCommon.GetNumberFromActiveCongfig(TeamPosInfoWindow.mCurActiveData.tid, "PET_SKILL_" + _dataReadIndex);
        DataRecord _skillRecord = GameCommon.GetSkillDataRecord(_SkillID);
        
        if(_skillRecord == null)
        {
            kSkillBean.gameObject.SetActive(false);
            return ;
        }
        kSkillBean.gameObject.SetActive(true);

        int _skillLevel = TeamPosInfoWindow.mCurActiveData.skillLevel[kSkillPosID];
        string _skillAtlasName = _skillRecord.getObject("SKILL_ATLAS_NAME").ToString();
        string _skillSpriteName = _skillRecord.getObject("SKILL_SPRITE_NAME").ToString();
        string _skillInfo = GameCommon.InitSkillDescription(_skillLevel, _SkillID);
        string _skillName = _skillRecord.getObject("NAME").ToString();

        SkillUpgradePanelInfoData supi = new SkillUpgradePanelInfoData();
        supi.skillName = _skillName;
        // added by xuke begin
        supi.skillDesc = _skillInfo;
        supi.currentSkillLevel = _skillLevel;
        //end
        kSkillBean.SetSkillIcon(_skillAtlasName, _skillSpriteName);
                          
        ConsumeItemLogicData itemData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
        int _usrBookNum = itemData.GetDataByTid((int)ITEM_TYPE.SKILL_BOOK).itemNum;

        supi.coin = TableCommon.GetNumberFromSkillCost(_skillLevel, "MONEY_COST");
        int _bookCost = TableCommon.GetNumberFromSkillCost(_skillLevel, "SKILL_BOOK_COST");
        supi.matRateNum = _usrBookNum + " / " + _bookCost;
        supi.bookCost = _bookCost;
        kSkillBean.supi = supi;
        kSkillBean.skillIndex = kSkillPosID;

        int skillOpenLevel = TableCommon.GetNumberFromActiveCongfig(TeamPosInfoWindow.mCurActiveData.tid, "SKILL_ACTIVE_LEVEL_" + _dataReadIndex);
        if (playLevel >= skillOpenLevel)
        {
            kSkillBean.UnlockSkill();
            kSkillBean.SetSkillLevel(_skillLevel);
            
        }
        else
        {
            kSkillBean.LockSkill();
            kSkillBean.SetSkillLevel(_skillLevel);
            string str = DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_SKILL_OPEN_TIPS, "STRING_CN");
            string strTips = skillOpenLevel.InsertToString(str);
            supi.skillDesc+= "\n[ff0000]" + strTips;
            
        }
        supi.openLevel = skillOpenLevel;
        supi.petBreakLevel = playLevel;
    }
	
	private void UpdateUI()
	{
		ActiveData activeData = TeamPosInfoWindow.mCurActiveData;
		if (activeData != null) {
			skills = mGameObjUI.GetComponentsInChildren<NewSKillUpgradeBean>(true);
			
			if (skills.Length <= 0)
				return;

            for (int i = 0; i < 4; i++) 
            {
                SetSkillUIInfo(skills[i], i);
            }
            GameCommon.FindComponent<UIGridContainer>(skills[0].transform.parent.parent.gameObject, "skill_icon_grid").Reposition();

            skills[NewSKillUpgradeBean.soleSkillIndex].OnClick();
		}
	}

}
