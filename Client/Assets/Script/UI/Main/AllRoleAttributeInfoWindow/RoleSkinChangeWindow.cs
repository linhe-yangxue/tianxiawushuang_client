using UnityEngine;
using System.Collections;
using DataTable;
using System.Collections.Generic;
using Logic;
using System;

public class RoleSkinChangeWindow : tWindow
{
    private const string BASE_COLOR = "[FFFFFF]";
    private const string ADD_COLOR = "[00FF00]";

    public int mCurIndex = 0;
    public int mSelIndex = 0;

    public List<ShopData> mShopDataList = new List<ShopData>();
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_go_buy_role_skin_btn", new DefineFactory<Button_GoBuyRoleSkinBtn>());
        EventCenter.Self.RegisterEvent("Button_use_role_skin_btn", new DefineFactory<Button_UseRoleSkinBtn>());
        EventCenter.Self.RegisterEvent("Button_forward_role_skin_btn", new DefineFactory<Button_ForwardRoleSkinBtn>());
        EventCenter.Self.RegisterEvent("Button_back_role_skin_btn", new DefineFactory<Button_BackRoleSkinBtn>());
        EventCenter.Self.RegisterEvent("Button_role_skin_close_button", new DefineFactory<Button_RoleSkinCloseButton>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "FORWARD":
                mCurIndex = (mCurIndex + 1) % mShopDataList.Count;
                Refresh(null);
                break;
            case "BACK":
                mCurIndex = (mCurIndex + mShopDataList.Count - 1) % mShopDataList.Count;
                Refresh(null);
                break;
        }
    }
    public override void OnOpen()
    {
        SetShopDataList();
        mCurIndex = mSelIndex;
        Refresh(null);

        DataCenter.SetData("ROLE_INFO_WINDOW", "SET_TITLE_VISIBLE", false);
    }

    private void SetShopDataList()
    {
        mShopDataList.Clear();
        ShopLogicData shopLogicData = DataCenter.GetData("SHOP_DATA") as ShopLogicData;
        if (shopLogicData != null)
        {
            RoleData d = RoleLogicData.GetMainRole();
            int iCurModeIndex = d.tid;
            foreach (ShopData shopData in shopLogicData.mShopDataList)
            {
                if (shopData.mIndex / 1000 == 3)
                {
                    int iModelIndex = TableCommon.GetNumberFromShopSlotBase(shopData.mIndex, "ROLE_MODEL_INDEX");
                    if (iCurModeIndex / 1000 == iModelIndex / 1000 /*&& shopData.mUsedCount > 0*/)
                    {
                        if (iCurModeIndex == iModelIndex)
                            mSelIndex = mShopDataList.Count;
                        mShopDataList.Add(shopData);
                    }
                }
            }
        }
    }

    public override bool Refresh(object param)
    {
        RoleLogicData data = RoleLogicData.Self;
        if (data != null)
        {
            ShopData shopData = mShopDataList[mCurIndex];
            int iModelIndex = TableCommon.GetNumberFromShopSlotBase(shopData.mIndex, "ROLE_MODEL_INDEX");
            RoleData d = RoleLogicData.GetMainRole();
            GameObject uiPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");
            Character role = GameCommon.ShowCharactorModel(uiPoint, iModelIndex, 1f) as Character;

            if (role == null)
                return false;

            string roleName = TableCommon.GetStringFromActiveCongfig(iModelIndex, "NAME");
            GameCommon.SetUIText(mGameObjUI, "RoleName", roleName);

            UIGridContainer grid = GameCommon.FindObject(mGameObjUI, "grid").GetComponent<UIGridContainer>();
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
            
            GameObject card = GameCommon.FindObject(mGameObjUI, "Card");
            GameCommon.SetUIText(card, "role_name", roleName);
            //GameObject gplab = GameCommon.FindObject(card, "gp_lab");
            //GameCommon.SetUIText(gplab, "lab_atk", totalInfoValue.mAttack.ToString());
            //GameCommon.SetUIText(gplab, "lab_mp", totalInfoValue.mMP.ToString());
            //GameCommon.SetUIText(gplab, "lab_hp", totalInfoValue.mHP.ToString());

            string strTitleName = TableCommon.GetStringFromRoleSkinConfig(iModelIndex, "ROLE_TITLE");
            GameCommon.SetUIText(card, "role_title_name", strTitleName);
            GameCommon.SetUIText(card, "role_title_name_back", strTitleName);

            GameCommon.SetUIText(card, "role_level", d.level.ToString());

            int iStarLevel = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "STAR_LEVEL");
            //for (int i = 1; i < 4; i++)
            //{
            //    if (i == iStarLevel) SetVisible("ec_ui_ghostball_" + i.ToString(), true);
            //    else SetVisible("ec_ui_ghostball_" + i.ToString(), false);
            //}

            SetRoleSkinAttackState(iModelIndex);
            SetRoleSkinSkill(iModelIndex);

            GameCommon.FindObject(card, "use").SetActive(mSelIndex == mCurIndex);

            GameCommon.FindObject(mGameObjUI, "go_buy_role_skin_btn").SetActive(shopData.mUsedCount <= 0);
            GameCommon.FindObject(mGameObjUI, "use_role_skin_btn").SetActive(shopData.mUsedCount > 0 && mSelIndex != mCurIndex);
            
            return true;
        }
        return false;
    }

    private void SetRoleSkinAttackState(int iModelIndex)
    {
        int iAttackStateIndex = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "ATTACK_STATE_1");
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
            strAttackStateName = strAttackStateName.Replace("\\n", "  ");
        }
        GameCommon.SetUIText(mGameObjUI, "skin_tip_label", strAttackStateName);
    }

    private void SetRoleSkinSkill(int iModelIndex)
    {
        int iIndex = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "PET_SKILL_1");
        GameCommon.SetUIVisiable(mGameObjUI, "main_skill_infomations", iIndex > 0);
        if (iIndex > 0)
        {
            string strAtlasName = TableCommon.GetStringFromSkillConfig(iIndex, "SKILL_ATLAS_NAME");
            string strSpriteName = TableCommon.GetStringFromSkillConfig(iIndex, "SKILL_SPRITE_NAME");
            SetUISprite("main_skill_icon", strAtlasName, strSpriteName);
            SetUISprite("skill_icon", strAtlasName, strSpriteName);
        }

        string strAttackStateName = TableCommon.GetStringFromSkillConfig(iIndex, "INFO");
        if (strAttackStateName != "")
        {
            strAttackStateName = strAttackStateName.Replace("\\n", "");
        }
        GameCommon.SetUIText(mGameObjUI, "main_skill_label", strAttackStateName);
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

    // set attribute icon
    private void SetAttributeIcon(GameObject obj, int iIndex)
    {
        string strAtlasName = TableCommon.GetStringFromEquipAttributeIconConfig(iIndex, "ICON_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromEquipAttributeIconConfig(iIndex, "ICON_SPRITE_NAME");

        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
        UISprite sprite = GameCommon.FindObject(obj, "icon").GetComponent<UISprite>();
        sprite.atlas = tu;
        sprite.spriteName = strSpriteName;
        //sprite.MakePixelPerfect();
    }

    private class AttributeObj
    {
        public int value = 0;
        public int attributeType = 0;
    }
}

public class Button_GoBuyRoleSkinBtn : CEvent
{
    public override bool _DoEvent()
    {
        int i = Convert.ToInt32(SHOP_PAGE_TYPE.CHARACTER);
        DataCenter.Set("WHICH_SHOP_PAGE", i);

        if (DataCenter.GetData("SHOP_WINDOW") != null)
            DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.CHARACTER);
        else
        {
            GlobalModule.ClearAllWindow();
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
        }
        return true;
    }
}

public class Button_UseRoleSkinBtn : CEvent
{
    public override bool _DoEvent()
    {
        RoleSkinChangeWindow window = DataCenter.GetData("ROLE_SKIN_CHANGE_WINDOW") as RoleSkinChangeWindow;
        RoleLogicData roleLogicData = RoleLogicData.Self;
        int roleIndex = window.mCurIndex;
        ShopData shopData = window.mShopDataList[roleIndex];
        int iSelModelIndex = RoleLogicData.GetMainRole().tid;
        int iModelIndex = TableCommon.GetNumberFromShopSlotBase(shopData.mIndex, "ROLE_MODEL_INDEX");
        if (GameCommon.GetCharacterTypeByModelIndex(iModelIndex) == GameCommon.GetCharacterTypeByModelIndex(iSelModelIndex))
        {
            ChangeRole(iModelIndex);
        }

        return true;
    }

    private void ChangeRole(int iModelIndex)
    {
        int roleIndex = 0;
        for (int i = 0; i < RoleLogicData.Self.mRoleList.Length; i++)
        {
            if (RoleLogicData.Self.GetRole(i).tid == RoleLogicData.GetMainRole().tid)
            {
                roleIndex = i;
                break;
            }
        }

        CS_RequestChangeMainChar evt = Net.StartEvent("CS_RequestChangeMainChar") as CS_RequestChangeMainChar;
        evt.set("CHAR_ID", RoleLogicData.GetMainRole().mIndex);
        evt.set("INDEX", roleIndex);
        evt.set("SKIN", GetShopIndex(iModelIndex));
        evt.set("MODEL_INDEX", iModelIndex);
        evt.mAction = () =>
        {
            int index = evt.get("INDEX");
            int iAimModelIndex = evt.get("MODEL_INDEX");
            RoleLogicData.Self.mRoleList[index].tid = iAimModelIndex;
            RoleLogicData.Self.mRoleList[index].starLevel = TableCommon.GetNumberFromActiveCongfig(iAimModelIndex, "STAR_LEVEL");
            RoleLogicData.Self.mRoleList[index].mMaxLevelNum = TableCommon.GetNumberFromActiveCongfig(iAimModelIndex, "MAX_LEVEL");
            //GameCommon.RestoreCharacterConfigRecord ();
            //GameCommon.ChangeCharacterConfigRecord ();
			DataCenter.SetData("ROLE_INFO_WINDOW", "REFRESH", null);
            DataCenter.CloseWindow("ROLE_SKIN_CHANGE_WINDOW");
        };
        evt.DoEvent();
    }

    private int GetShopIndex(int iModelIndex)
    {
        foreach (KeyValuePair<int, DataRecord> iter in DataCenter.mShopSlotBase.GetAllRecord())
        {
            if (iter.Value.get("ROLE_MODEL_INDEX") == iModelIndex)
                return iter.Key;
        }
        return -1;
    }
}

public class Button_ForwardRoleSkinBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("ROLE_SKIN_CHANGE_WINDOW", "FORWARD", true);
        return true;
    }
}

public class Button_BackRoleSkinBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("ROLE_SKIN_CHANGE_WINDOW", "BACK", true);
        return true;
    }
}

public class Button_RoleSkinCloseButton : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("ROLE_SKIN_CHANGE_WINDOW");
        DataCenter.SetData("ROLE_INFO_WINDOW", "SET_TITLE_VISIBLE", true);
        return true;
    }
}