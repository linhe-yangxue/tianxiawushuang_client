using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;

public class RoleSkinDetailsWindow : tWindow
{
	private const string BASE_COLOR = "[FFFFFF]";
	private const string ADD_COLOR = "[00FF00]";

	public List<ShopData> mShopDataList = new List<ShopData>();

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_role_skin_details_button", new DefineFactory<Button_role_skin_details_button>());
		EventCenter.Self.RegisterEvent("Button_role_skin_details_close_button", new DefineFactory<Button_role_skin_details_close_button>());
		EventCenter.Self.RegisterEvent("Button_role_skin_details_sure_button", new DefineFactory<Button_role_skin_details_close_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		Refresh (param);
	}

	public override bool Refresh (object param)
	{
		int iModelIndex = (int)param;
		RoleData curRoleData = RoleLogicData.GetMainRole ();

		GameObject uiPoint =GetSub ("UIPoint");
		Character role = GameCommon.ShowCharactorModel(uiPoint, iModelIndex, 1f) as Character;

		int iStarLevel = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "STAR_LEVEL");
        //for (int i = 1; i < 4; i++)
        //{
        //    if (i == iStarLevel) SetVisible("ec_ui_ghostball_" + i.ToString(), true);
        //    else SetVisible("ec_ui_ghostball_" + i.ToString(), false);
        //}

		string roleName = TableCommon.GetStringFromActiveCongfig(iModelIndex, "NAME");
		SetText ("role_name", roleName);
		SetText ("role_level", curRoleData.level.ToString ());

		string strTitleName = TableCommon.GetStringFromRoleSkinConfig(iModelIndex, "ROLE_TITLE");
		SetText ("role_title_name", strTitleName);
		SetText ("role_title_name_back", strTitleName);

		SetVisible ("use", iModelIndex == curRoleData.tid);

		UIGridContainer grid = GetComponent<UIGridContainer>("grid");
		DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
		int iHp = GameCommon.GetRoleSkinHP(dataRecord);
		int iMp = GameCommon.GetRoleSkinMP(dataRecord);
		int iAttack = GameCommon.GetRoleSkinAttack(dataRecord);
		
		List<AttributeObj> attributeList = new List<AttributeObj>();
		if (iHp > 0)
		{
			AttributeObj attributeObj = new AttributeObj();
			attributeObj.value = iHp;
            attributeObj.attributeType = (int)AFFECT_TYPE.HP;
			attributeList.Add(attributeObj);
		}
		if (iMp > 0)
		{
			AttributeObj attributeObj = new AttributeObj();
			attributeObj.value = iMp;
            attributeObj.attributeType = (int)AFFECT_TYPE.MP;
			attributeList.Add(attributeObj);
		}
		if (iAttack > 0)
		{
			AttributeObj attributeObj = new AttributeObj();
			attributeObj.value = iAttack;
            attributeObj.attributeType = (int)AFFECT_TYPE.ATTACK;
			attributeList.Add(attributeObj);
		}
		
		grid.MaxCount = attributeList.Count;
		for (int i = 0; i < grid.MaxCount; i++ )
		{
			GameObject obj = grid.controlList[i];
			SetNameLabel(obj, attributeList[i].attributeType);
			SetLabelValue(obj, attributeList[i].value);
			SetAttributeIcon(obj, attributeList[i].attributeType);
		}

		SetRoleSkinAttackState(iModelIndex);
		SetRoleSkinSkill(iModelIndex);

		return true;
	}

	private void SetNameLabel(GameObject obj, int iIndex)
	{
		string str = TableCommon.GetStringFromEquipAttributeIconConfig(iIndex, "NAME");
		GameCommon.SetUIText(obj, "Title", str);
	}

	private void SetLabelValue(GameObject obj, int value)
	{
		string str = ADD_COLOR + value.ToString();
		GameCommon.SetUIText(obj, "Num", str);
	}
	
	private void SetAttributeIcon(GameObject obj, int iIndex)
	{
		string strAtlasName = TableCommon.GetStringFromEquipAttributeIconConfig(iIndex, "ICON_ATLAS_NAME");
		string strSpriteName = TableCommon.GetStringFromEquipAttributeIconConfig(iIndex, "ICON_SPRITE_NAME");
		UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
		UISprite sprite = GameCommon.FindObject(obj, "icon").GetComponent<UISprite>();
		sprite.atlas = tu;
		sprite.spriteName = strSpriteName;
	}
	
	private class AttributeObj
	{
		public int value = 0;
		public int attributeType = 0;
	}

	private void SetRoleSkinAttackState(int iModelIndex)
	{
		int iAttackStateIndex = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "ATTACK_STATE_1");
		SetVisible ("skin_tips", iAttackStateIndex > 0);
		SetVisible ("skin_tips_null", iAttackStateIndex <= 0);

		if (iAttackStateIndex > 0)
		{
			string strAtlasName = TableCommon.GetStringFromAttackState(iAttackStateIndex, "SKILL_ATLAS_NAME");
			string strSpriteName = TableCommon.GetStringFromAttackState(iAttackStateIndex, "SKILL_SPRITE_NAME");
			SetUISprite ("skin_attribute_icon", strAtlasName, strSpriteName);
		}
		
		string strAttackStateName = TableCommon.GetStringFromAttackState(iAttackStateIndex, "INFO");
		if (strAttackStateName != "")
			strAttackStateName = strAttackStateName.Replace("\\n", "  ");

		SetText ("skin_tip_label", strAttackStateName);
	}
	
	private void SetRoleSkinSkill(int iModelIndex)
	{
		int iIndex = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "PET_SKILL_1");
		SetVisible ("main_skill_infomations", iIndex > 0);
		if (iIndex > 0)
		{
			string strAtlasName = TableCommon.GetStringFromSkillConfig(iIndex, "SKILL_ATLAS_NAME");
			string strSpriteName = TableCommon.GetStringFromSkillConfig(iIndex, "SKILL_SPRITE_NAME");
            SetUISprite("main_skill_icon", strAtlasName, strSpriteName);
            SetUISprite("skill_icon", strAtlasName, strSpriteName);
        }
		
		string strAttackStateName = TableCommon.GetStringFromSkillConfig(iIndex, "INFO");
		if (strAttackStateName != "")
			strAttackStateName = strAttackStateName.Replace("\\n", "");

		SetText("main_skill_label", strAttackStateName);
	}
}


public class Button_role_skin_details_button : CEvent
{
	public override bool _DoEvent()
	{
		int iRoleSkinIndex = (int)getObject ("ROLE_SKIN_INDEX");
		DataCenter.OpenWindow ("ROLE_SKIN_DETAILS_WINDOW", iRoleSkinIndex);
		return true;
	}
}

public class Button_role_skin_details_close_button : CEvent
{
	public override bool _DoEvent()
	{
		return DataCenter.CloseWindow ("ROLE_SKIN_DETAILS_WINDOW");
	}
}