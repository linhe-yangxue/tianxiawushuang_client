using UnityEngine;
using System.Collections;
using DataTable;
using Logic;

public class RoleInfoWindow : tWindow
{
    private const string BASE_COLOR = "[FFFFFF]";
    private const string ADD_COLOR = "[00FF00]";

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_EnterRoleEvolutionPageBtn", new DefineFactory<Button_EnterRoleEvolutionPageBtn>());

        EventCenter.Self.RegisterEvent("Button_role_equip_icon_sprite_0", new DefineFactory<Button_RoleEquipIconBtn>());
        EventCenter.Self.RegisterEvent("Button_role_equip_icon_sprite_1", new DefineFactory<Button_RoleEquipIconBtn>());
        EventCenter.Self.RegisterEvent("Button_role_equip_icon_sprite_2", new DefineFactory<Button_RoleEquipIconBtn>());
        EventCenter.Self.RegisterEvent("Button_role_equip_icon_sprite_3", new DefineFactory<Button_RoleEquipIconBtn>());

        EventCenter.Self.RegisterEvent("Button_role_skin_button", new DefineFactory<Button_RoleSkinBtn>());
        
    }

    public override void OnOpen()
    {
        DataCenter.CloseWindow("STAR_LEVEL_UP_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_COMPOSITION_WINDOW");

        DataCenter.OpenWindow("ROLE_INFO_CHANGE_ROLE_WINDOW");

		InitButtonIsUnLockOrNot();

        Refresh(null);
    }

	void InitButtonIsUnLockOrNot()
	{
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "EnterRoleEvolutionPageBtn", UNLOCK_FUNCTION_TYPE.UPGRADE_ROLE);
	}

    public override void Close()
    {
        DataCenter.CloseWindow("ROLE_INFO_CHANGE_ROLE_WINDOW");

        base.Close();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        if (keyIndex == "REFRESH")
            Refresh(objVal);
		else if(keyIndex == "SET_ALL_EQUIP_ICON")
		{
			SetAllEquipIcon();
		}
        else if(keyIndex == "SET_TITLE_VISIBLE")
            SetVisible("role_title_name", (bool)objVal);
        else
            base.onChange(keyIndex, objVal);           
    }

    public override bool Refresh(object param)
    {
        ObjectManager.Self.ClearAll();
        RoleLogicData data = RoleLogicData.Self;
        if (data != null)
        {
            RoleData d = RoleLogicData.GetMainRole();
            GameObject uiPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");
            Character role = GameCommon.ShowCharactorModel(uiPoint, 1f) as Character;

            if (role == null)
                return false;

            string roleName = TableCommon.GetStringFromActiveCongfig(d.tid, "NAME");
            GameCommon.SetUIText(mGameObjUI, "RoleName", roleName);

            GameObject levelInfo = GameCommon.FindObject(mGameObjUI, "LevelInfo");
            RefreshExpBar(levelInfo, d);

            RoleInfoValue baseInfoValue = GetBaseInfoValue();
            RoleInfoValue totalInfoValue = GetTotalInfoValue();

            SetLabelValue("Damage", baseInfoValue.mAttack, totalInfoValue.mAttack);
            SetLabelValue("Life", baseInfoValue.mHP, totalInfoValue.mHP);
            SetLabelValue("MP", baseInfoValue.mMP, totalInfoValue.mMP);
            SetText("UpgradeName", RoleStarLevel2String(RoleLogicData.GetMainRole().starLevel));

            SetAllEquipIcon();

            GameObject card = GameCommon.FindObject(mGameObjUI, "Card");
//            GameCommon.SetUIText(card, "role_name", GetVerticalString(roleName));
			GameCommon.SetUIText(card, "role_name", roleName);
            GameObject gplab = GameCommon.FindObject(card, "gp_lab");
            GameCommon.SetUIText(gplab, "lab_atk", totalInfoValue.mAttack.ToString());
            GameCommon.SetUIText(gplab, "lab_mp", totalInfoValue.mMP.ToString());
            GameCommon.SetUIText(gplab, "lab_hp", totalInfoValue.mHP.ToString());

			string strTitleName = TableCommon.GetStringFromRoleSkinConfig (RoleLogicData.GetMainRole().tid, "ROLE_TITLE");
			GameCommon.SetUIText (card, "role_title_name", strTitleName);
			GameCommon.SetUIText (card, "role_title_name_back", strTitleName);

			GameCommon.SetUIText (card, "role_level", RoleLogicData.GetMainRole().level.ToString ());

            int iIndex = TableCommon.GetNumberFromActiveCongfig(RoleLogicData.GetMainRole().tid, "PET_SKILL_1");
            if (iIndex > 0)
            {
                string strAtlasName = TableCommon.GetStringFromSkillConfig(iIndex, "SKILL_ATLAS_NAME");
                string strSpriteName = TableCommon.GetStringFromSkillConfig(iIndex, "SKILL_SPRITE_NAME");
                SetUISprite("skill_icon", strAtlasName, strSpriteName);
            }

//            GameCommon.SetRoleTitle(GameCommon.FindComponent<UISprite>(card, "role_title"));

            SetVisible("role_title_name", true);
            //for(int i = 1; i < 4; i++)
            //{
            //    if(i == RoleLogicData.GetMainRole().mStarLevel) SetVisible ("ec_ui_ghostball_" + i.ToString () , true);
            //    else SetVisible ("ec_ui_ghostball_" + i.ToString () , false);
            //}


            int iAttackStateIndex = TableCommon.GetNumberFromActiveCongfig(d.tid, "ATTACK_STATE_1");
            GameCommon.SetUIVisiable(mGameObjUI, "skin_tips", iAttackStateIndex > 0);
            GameCommon.SetUIVisiable(mGameObjUI, "skin_tips_null", iAttackStateIndex <= 0);
            if (iAttackStateIndex > 0)
            {
                string strAtlasName = TableCommon.GetStringFromAttackState(iAttackStateIndex, "SKILL_ATLAS_NAME");
                string strSpriteName = TableCommon.GetStringFromAttackState(iAttackStateIndex, "SKILL_SPRITE_NAME");
                GameCommon.SetUISprite(mGameObjUI, "skin_attribute_icon", strAtlasName, strSpriteName);
            }

            string strAttackStateName = TableCommon.GetStringFromAttackState(iAttackStateIndex, "INFO");
            if (strAttackStateName != "")
            {
                strAttackStateName = strAttackStateName.Replace("\\n", "");
            }
            GameCommon.SetUIText(mGameObjUI, "skin_tip_label", strAttackStateName);

            return true;
        }
        return false;
    }

	private void SetAllEquipIcon()
	{
		SetEquipInfoVisiable(false);
		for(int i = (int)EQUIP_TYPE.ARM_EQUIP; i < (int)EQUIP_TYPE.MAX; i++)
		{
			SetEquipIcon(i);
		}

		SetRoleEquipSetDescription();
	}

	private void SetRoleEquipSetDescription()
	{
		if(mGameObjUI != null)
		{
			GameObject desObj = GameCommon.FindObject(mGameObjUI, "tip_label");
			int iRoleEquipSetIndex = GameCommon.GetRoleEquipSetIndex();
			if(iRoleEquipSetIndex != 0)
			{
				desObj.SetActive(true);
				UILabel desLabel = desObj.GetComponent<UILabel>();
				DataRecord record = DataCenter.mSetEquipConfig.GetRecord(iRoleEquipSetIndex);
				if(record != null)
				{
					desLabel.text = "[" + record.get ("SET_NAME") + "]" + record.get("SET_DES");
				}
			}
			else
			{
				desObj.SetActive(false);
			}
		}
	}

    private void SetEquipIcon(int iType)
    {
        RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		EquipData curUseEquip = roleEquipLogicData.GetUseEquip(iType);
        if (curUseEquip != null)
        {
			SetEquipIconVisiable(iType, true);

			GameObject obj = GameCommon.FindObject(mGameObjUI, "role_equip_icon_sprite_" + iType.ToString());
			GameCommon.SetEquipIcon(obj, curUseEquip.tid);

			// set star level
			GameCommon.SetStarLevelLabel(obj, curUseEquip.mStarLevel);
			
			// set strengthen level text
			GameCommon.SetStrengthenLevelLabel(obj, curUseEquip.strengthenLevel);

			// set element icon and set equip element background icon
			int iElementIndex = (int)curUseEquip.mElementType;
			GameCommon.SetEquipElementBgIcons(obj, curUseEquip.tid, iElementIndex);
			obj = GameCommon.FindObject(mGameObjUI, "RoleEquipIconInfo");
			GameCommon.SetEquipElementIcon(obj, "equip_element", curUseEquip.tid, iElementIndex);

			if(iType == (int)EQUIP_TYPE.ELEMENT_EQUIP)
			{
				SetEquipInfoVisiable(false);

				string equipName = GetEquipQualityColor(curUseEquip.mQualityType) + TableCommon.GetStringFromRoleEquipConfig(curUseEquip.tid, "NAME");
				GameCommon.SetUIText(obj, "equip_name_label", equipName);
			}
        }
        else 
        {
			SetEquipIconVisiable(iType, false);
        }
    }

	private void SetEquipInfoVisiable(bool visiable)
	{
		GameObject obj = GameCommon.FindObject(mGameObjUI, "RoleEquipIconInfo");
		GameCommon.SetUIVisiable(obj, "equip_element", visiable);
		GameCommon.SetUIVisiable(obj, "equip_name_label", visiable);
	}

    private void SetEquipIconVisiable(int iType, bool visiable)
    {
		GameObject obj = GameCommon.FindObject(mGameObjUI, "RoleEquipIconInfo");
		GameObject equipIconGroup = GameCommon.FindObject(obj, "role_equip_icon_sprite_" + iType.ToString());
		GameCommon.SetUIVisiable(equipIconGroup, "icon_sprite", visiable);
    }

    private string GetVerticalString(string str)
    {
        string verticalStr = "";
        foreach (char c in str)
        {
            verticalStr += c + "\n";
        }
        return verticalStr;
    }

	public string GetEquipQualityColor(EQUIP_QUALITY_TYPE type)
	{
		string strQualityColor = "";
		
		switch (type)
		{
		case EQUIP_QUALITY_TYPE.LOW:
			break;
		case EQUIP_QUALITY_TYPE.MIDDLE:
			strQualityColor = "[6bc235]";
			break;
		case EQUIP_QUALITY_TYPE.GOOD:
			strQualityColor = "[005aab]";
			break;
		case EQUIP_QUALITY_TYPE.BETTER:
			strQualityColor = "[5a0d43]";
			break;
		case EQUIP_QUALITY_TYPE.BEST:
			strQualityColor = "[f8d61d]";
			break;
		}

		return strQualityColor;
	}

    private string EquitQualityType2String(EQUIP_QUALITY_TYPE type)
    {
        switch (type)
        {
            case EQUIP_QUALITY_TYPE.LOW:
                return "凡品";
            case EQUIP_QUALITY_TYPE.MIDDLE:
                return "珍品";
            case EQUIP_QUALITY_TYPE.GOOD:
                return "绝品";
            case EQUIP_QUALITY_TYPE.BEST:
                return "仙品";
            default:
                return "";
        }
    }

    private string RoleStarLevel2String(int starLevel)
    {
        switch (starLevel)
        {
            case 1:
                return "驱鬼师";
            case 2:
                return "御妖师";
            case 3:
                return "伏魔师";
            case 4:
                return "天师";
            case 5:
                return "封神";
            default:
                return "";
        }
    }

    private void RefreshExpBar(GameObject obj, RoleData d)
    {
        int levelUpExp = TableManager.GetData("CharacterLevelExp", d.level, "LEVEL_EXP");
        float exp = d.exp;
        float expRate = exp / levelUpExp;
        GameCommon.SetUIText(obj, "Label3", "Lv" + d.level);
        GameObject bar = GameCommon.FindObject(obj, "Progress Bar");
        bar.GetComponent<UIProgressBar>().value = expRate;
        GameCommon.SetUIText(bar, "Label", Float2Percent(expRate));
    }

    private string Float2Percent(float value)
    {
        return (int)(value * 100f) + "%";
    }

    private void SetLabelValue(string objName, int baseValue, int totalValue)
    {
        GameObject obj = GameCommon.FindObject(mGameObjUI, objName);
        string str = BASE_COLOR + baseValue.ToString();
        if (totalValue > baseValue)
        {
            str += ADD_COLOR + "+" + (totalValue - baseValue).ToString();
        }
        GameCommon.SetUIText(obj, "Num", str);
    }

    private RoleInfoValue GetBaseInfoValue()
    {
        RoleInfoValue infoValue = new RoleInfoValue();
        RoleData d = RoleLogicData.GetMainRole();
        infoValue.mHP = (int)GameCommon.GetBaseMaxHP(d.tid, d.level, 0);
        infoValue.mMP = (int)GameCommon.GetBaseMaxMP(d.tid, d.level, 0);
        infoValue.mAttack = (int)GameCommon.GetBaseAttack(d.tid, d.level, 0);
        return infoValue;
    }

    private RoleInfoValue GetTotalInfoValue()
    {
        RoleInfoValue infoValue = new RoleInfoValue();
        //RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
        //EquipData curUseEquip = roleEquipLogicData.GetUseEquip();
        //if (curUseEquip == null)
        //{
        //    infoValue.mHP = baseInfoValue.mHP;
        //    infoValue.mMP = baseInfoValue.mMP;
        //    infoValue.mAttack = baseInfoValue.mAttack;           
        //}
        //else 
        //{
        //    infoValue.mHP = curUseEquip.ApplyAffect(AFFECT_TYPE.HP_MAX, baseInfoValue.mHP);
        //    infoValue.mMP = curUseEquip.ApplyAffect(AFFECT_TYPE.MP_MAX, baseInfoValue.mMP);
        //    infoValue.mAttack = curUseEquip.ApplyAffect(AFFECT_TYPE.ATTACK, baseInfoValue.mAttack);
        //}
        RoleData d = RoleLogicData.GetMainRole();
        infoValue.mHP = GameCommon.GetBaseMaxHP(d.tid, d.level, 0);
        infoValue.mMP = GameCommon.GetBaseMaxMP(d.tid, d.level, 0);
        infoValue.mAttack = (int)GameCommon.GetBaseAttack(d.tid, d.level, 0);
        return infoValue;
    }

    private class RoleInfoValue
    {
        public int mHP = 0;
        public int mMP = 0;
        public int mAttack = 0;
    }
}


public class Button_EnterRoleEvolutionPageBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("ROLE_INFO_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_COMPOSITION_WINDOW");
        DataCenter.OpenWindow("STAR_LEVEL_UP_WINDOW");
        DataCenter.SetData("ALL_ROLE_ATTRIBUTE_INFO_WINDOW", "SET_SEL_PAGE", "STAR_LEVEL_UP_WINDOW");
        return true;
    }
}

public class Button_RoleEquipBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("ROLE_INFO_WINDOW");
        DataCenter.CloseWindow("STAR_LEVEL_UP_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_COMPOSITION_WINDOW");
        DataCenter.OpenWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
        DataCenter.SetData("ALL_ROLE_ATTRIBUTE_INFO_WINDOW", "SET_SEL_PAGE", "ROLE_EQUIP_CULTIVATE_WINDOW");
        return true;
    }
}

public class Button_RoleEquipIconBtn : CEvent
{
    public override bool _DoEvent()
    {
        string[] names = GetEventName().Split('_');

        DataCenter.OpenWindow("BAG_INFO_WINDOW");
        DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.RoleEquipWindow);
        DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS_FOR_TYPE", int.Parse(names[names.Length - 1]));
        return true;
    }
}

public class Button_RoleSkinBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("ROLE_SKIN_CHANGE_WINDOW");
        return true;
    }
}

//---------------------------------------------------------------------------
public class RoleAttributeWindow : tWindow
{
	private const string ADD_COLOR = "[99ff00]";
	private const string BASE_MP_COLOR = "[3399ff]";
	private const string BASE_HP_COLOR = "[cc0000]";
	private const string BASE_ATTACK_COLOR = "[ff9900]";

	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_check_role_info_button", new DefineFactory<Button_check_role_info_button>());
		EventCenter.Self.RegisterEvent("Button_role_attribute_window_close_button", new DefineFactory<Button_role_attribute_window_close_button>());
	}

	public override void OnOpen ()
	{
		int iModelIndex = RoleLogicData.GetMainRole().tid;
		SetRoleSkinSkill(iModelIndex);
		SetRoleInfo (iModelIndex);

		DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);   
		RoleInfoValue baseInfoValue = GetBaseInfoValue();
		RoleInfoValue totalInfoValue = GetTotalInfoValue();
	
		string strMp = BASE_MP_COLOR + baseInfoValue.mMP.ToString ();
		string strHp = BASE_HP_COLOR + baseInfoValue.mHP.ToString ();
		string strAttack = BASE_ATTACK_COLOR + baseInfoValue.mAttack.ToString ();
		if((totalInfoValue.mMP - baseInfoValue.mMP) != 0)
			strMp += ADD_COLOR + "+" + (totalInfoValue.mMP - baseInfoValue.mMP).ToString ();
		if((totalInfoValue.mHP - baseInfoValue.mHP) != 0)
			strHp += ADD_COLOR + "+" + (totalInfoValue.mHP - baseInfoValue.mHP).ToString ();
		if((totalInfoValue.mAttack - baseInfoValue.mAttack) != 0)
			strAttack += ADD_COLOR + "+" + (totalInfoValue.mAttack - baseInfoValue.mAttack).ToString ();

		System.Collections.Generic.Dictionary<string, string> mRoleInfos = new System.Collections.Generic.Dictionary<string, string>();
		mRoleInfos.Add ("mp_num", strMp);
		mRoleInfos.Add ("hp_num", strHp);
		mRoleInfos.Add ("damage_num", strAttack);
		mRoleInfos.Add ("defense_num", ADD_COLOR + GameCommon.GetBaseDefence (dataRecord).ToString () + "%");
		mRoleInfos.Add ("hit_num", ADD_COLOR + (GameCommon.GetBaseHitRate (dataRecord) * 100).ToString () + "%");
		mRoleInfos.Add ("dodge_num", ADD_COLOR + (GameCommon.GetBaseDodgeRate (dataRecord) * 100).ToString () + "%");
		mRoleInfos.Add ("crit_num", ADD_COLOR + (GameCommon.GetBaseCriticalStrikeRate (dataRecord) * 100).ToString () + "%");
		mRoleInfos.Add ("reduce_injury_num", ADD_COLOR + (GameCommon.GetBaseDamageMitigationRate (dataRecord) * 100).ToString () + "%");
        //mRoleInfos.Add ("reduce_shui", ADD_COLOR + (GameCommon.GetElementReductionRate (dataRecord, (int)ELEMENT_TYPE.BLUE) * 100).ToString () + "%");
        //mRoleInfos.Add ("reduce_mu", ADD_COLOR + (GameCommon.GetElementReductionRate (dataRecord, (int)ELEMENT_TYPE.GREEN) * 100).ToString () + "%");
        //mRoleInfos.Add ("reduce_huo", ADD_COLOR + (GameCommon.GetElementReductionRate (dataRecord, (int)ELEMENT_TYPE.RED) * 100).ToString () + "%");
        //mRoleInfos.Add ("reduce_yin", ADD_COLOR + (GameCommon.GetElementReductionRate (dataRecord, (int)ELEMENT_TYPE.SHADOW) * 100).ToString () + "%");
        //mRoleInfos.Add ("reduce_yang", ADD_COLOR + (GameCommon.GetElementReductionRate (dataRecord, (int)ELEMENT_TYPE.GOLD) * 100).ToString () + "%");

		foreach(System.Collections.Generic.KeyValuePair<string, string> d in mRoleInfos)
		{
			SetText (d.Key, d.Value);
		}
	}

	private void SetRoleInfo(int iModelIndex)
	{
		string strAtlasName = TableCommon.GetStringFromActiveCongfig (iModelIndex, "HEAD_ATLAS_NAME");
		string strSpriteName = TableCommon.GetStringFromActiveCongfig (iModelIndex, "HEAD_SPRITE_NAME");
		GameCommon.SetIcon (GetComponent<UISprite>("role_icon"), strAtlasName, strSpriteName);

		SetText ("role_attribute_infomation", TableCommon.GetStringFromActiveCongfig (iModelIndex, "DESCRIBE"));
	}

	private void SetRoleSkinSkill(int iModelIndex)
	{
		int iIndex = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "PET_SKILL_1");
		GameCommon.SetUIVisiable(mGameObjUI, "main_skill_infomations", iIndex > 0);
		if (iIndex > 0)
		{
			string strAtlasName = TableCommon.GetStringFromSkillConfig(iIndex, "SKILL_ATLAS_NAME");
			string strSpriteName = TableCommon.GetStringFromSkillConfig(iIndex, "SKILL_SPRITE_NAME");
			GameCommon.SetUISprite(mGameObjUI, "main_skill_icon", strAtlasName, strSpriteName);
		}
		
		string strAttackStateName = TableCommon.GetStringFromSkillConfig(iIndex, "INFO");
		if (strAttackStateName != "")
		{
			strAttackStateName = strAttackStateName.Replace("\\n", "");
		}
		GameCommon.SetUIText(mGameObjUI, "main_skill_label", strAttackStateName);
	}

	private RoleInfoValue GetBaseInfoValue()
	{
		RoleInfoValue infoValue = new RoleInfoValue();
		RoleData d = RoleLogicData.GetMainRole();
		infoValue.mHP = GameCommon.GetBaseMaxHP(d.tid, d.level, 0);
		infoValue.mMP = GameCommon.GetBaseMaxMP(d.tid, d.level, 0);
		infoValue.mAttack = (int)GameCommon.GetBaseAttack(d.tid, d.level, 0);
		return infoValue;
	}

	private RoleInfoValue GetTotalInfoValue()
	{
		RoleInfoValue infoValue = new RoleInfoValue();
		RoleData d = RoleLogicData.GetMainRole();
		infoValue.mHP = GameCommon.GetBaseMaxHP(d.tid, d.level, 0);
		infoValue.mMP = GameCommon.GetBaseMaxMP(d.tid, d.level, 0);
		infoValue.mAttack = (int)GameCommon.GetBaseAttack(d.tid, d.level, 0);
		return infoValue;
	}

	private class RoleInfoValue
	{
		public int mHP = 0;
		public int mMP = 0;
		public int mAttack = 0;
	}
}


class Button_check_role_info_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow("ROLE_ATTRIBUTE_WINDOW");
		return true;
	}
}

class Button_role_attribute_window_close_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow("ROLE_ATTRIBUTE_WINDOW");
		return true;
	}
}


