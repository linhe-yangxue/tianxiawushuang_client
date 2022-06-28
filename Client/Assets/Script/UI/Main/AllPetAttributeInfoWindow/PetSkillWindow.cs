using UnityEngine;
using System.Collections;
using Logic;

public class PetSkillWindow : tWindow {

    public int mCurPetID = 0;
    public PetData mCurSelPetData = null;
    public int mCurSelSkillBtnIndex = 0;

    public override void Init()
    {
        Net.gNetEventCenter.RegisterEvent("CS_RequestPetSkillLevelUp", new DefineFactoryLog<CS_RequestPetSkillLevelUp>());

        EventCenter.Self.RegisterEvent("Button_pet_skill_btn_0", new DefineFactory<Button_PetSkillTypeEvent>());
        EventCenter.Self.RegisterEvent("Button_pet_skill_btn_1", new DefineFactory<Button_PetSkillTypeEvent>());
        EventCenter.Self.RegisterEvent("Button_pet_skill_btn_2", new DefineFactory<Button_PetSkillTypeEvent>());
        EventCenter.Self.RegisterEvent("Button_choose_upgrade_skill_pet", new DefineFactory<Button_UnChooseUpgradeSkillPet>());
        EventCenter.Self.RegisterEvent("Button_skill_upgrade_button", new DefineFactory<Button_SkillUpgradeButton>());
    }
    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "SET_SELECT_UPGRADE_SKILL_PET":
                mCurSelSkillBtnIndex = 0;
                Refresh((int)objVal);
                break;
            case "SELECT_PET_SKILL":
                mCurSelSkillBtnIndex = (int)objVal;
                UpdataSelSkillInfo();
                break;
            case "UPGRADE_PET_SKILL":
                UpgradePetSkill();
                break;
            case "UPGRADE_PET_SKILL_RESULT":
                UpgradePetSkillResult();
                break;
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();

		DataCenter.CloseWindow("PET_DECOMPOSE_WINDOW");
        //DataCenter.OpenWindow("BAG_INFO_WINDOW");
        DataCenter.SetData("PET_SKILL_WINDOW", "SET_SELECT_UPGRADE_SKILL_PET", 0);
    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);

        mCurPetID = (int)param;
        SetCurSelPetData();

        UpdatePetSkillUI();
        return true;
    }

    private void SetCurSelPetData()
    {
        PetLogicData logicData = DataCenter.GetData("PET_DATA") as PetLogicData;
        if (logicData != null)
        {
            mCurSelPetData = logicData.GetPetDataByItemId(mCurPetID);
        }
    }

    private void UpdatePetSkillUI()
    {
        UpdateChoosePetUI();

        UpdateChoosePetSkillInfoUI();
    }

    private void UpdateChoosePetUI()
    {
        UpdateChoosePetIcon();

        if (mCurSelPetData == null)
        {
            DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_PET_ICONS", true);
            DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.Bag_Pet_Window_SpriteTitle);
        }
        else
        {
            DataCenter.SetData("BAG_INFO_WINDOW", "CLEAR_STONE_SELECT_STATE", true);
            DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.Bag_Stone_Window);
        }
    }

    private void UpdateChoosePetSkillInfoUI()
    {
        GameObject skill = GameCommon.FindObject(mGameObjUI, "skill");
        skill.SetActive(mCurSelPetData != null);
        GameObject chooseSkill = mGameObjUI.transform.Find("choose_skill/background").gameObject;
        chooseSkill.SetActive(mCurSelPetData != null);
        GameObject chooseGem = mGameObjUI.transform.Find("choose_stone/background").gameObject;
        chooseGem.SetActive(mCurSelPetData != null);

        GameObject unselPetLabel = GameCommon.FindObject(mGameObjUI, "unsel_pet_label");
        unselPetLabel.SetActive(mCurSelPetData == null);
        GameObject chooseStoneEffect = GameCommon.FindObject(mGameObjUI, "choose_stone_effect");
        chooseStoneEffect.SetActive(mCurSelPetData == null);

        GameObject skillUpgradeEffect = GameCommon.FindObject(mGameObjUI, "skill_upgrade_effect");
        skillUpgradeEffect.SetActive(false);

        if (mCurSelPetData != null)
        {
            UpdateForwardlySkillinfo();
            UpdatePassiveSkillInfo();

            UpdataSelSkillInfo();
        }
    }

    private void UpdateChoosePetIcon()
    {
        GameObject obj = GameCommon.FindObject(mGameObjUI, "choose_upgrade_skill_pet");
        GameObject background = GameCommon.FindObject(obj, "Background");
        background.SetActive(mCurSelPetData != null);

        GameObject effect = GameCommon.FindObject(obj, "choose_pet_effect");
        effect.SetActive(mCurSelPetData == null);

        if (mCurSelPetData != null)
        {
            // set pet icon
            GameCommon.SetPetIcon(obj, mCurSelPetData.tid);

            // set element icon
            int iElementIndex = TableCommon.GetNumberFromActiveCongfig(mCurSelPetData.tid, "ELEMENT_INDEX");
            GameCommon.SetElementIcon(obj, iElementIndex);

            // set level
            GameCommon.SetLevelLabel(obj, mCurSelPetData.level);

            // set star level
            GameCommon.SetStarLevelLabel(obj, mCurSelPetData.starLevel);

            // set strengthen level text
            GameCommon.SetStrengthenLevelLabel(obj, mCurSelPetData.strengthenLevel);
        }
    }

    private void UpdateForwardlySkillinfo()
    {
        if (mCurSelPetData != null)
        {
            UpdatePetSkillInfo(0);
        }
    }

    private void UpdatePassiveSkillInfo()
    {
        if (mCurSelPetData != null)
        {
            for (int i = 1; i <= 2; i++)
            {
                UpdatePetSkillInfo(i);
            }
        }
    }

    //private int GetPassiveSkillNum()
    //{
    //    int iNum = 0;
    //    int iMaxNum = GameCommon.GetPassiveSkillMaxNum(mCurSelPetData);
    //    for (int i = 1; i <= 3 && iNum <= iMaxNum; i++ )
    //    {
    //        int skillIndex = TableCommon.GetNumberFromActiveCongfig(mCurSelPetData.mModelIndex, "ATTACK_STATE_" + i.ToString());
    //        if (skillIndex <= 0)
    //            continue;

    //        iNum++;
    //    }

    //    for (int j = 1; j <= 3 && iNum <= iMaxNum; j++)
    //    {
    //        int skillIndex = TableCommon.GetNumberFromActiveCongfig(mCurSelPetData.mModelIndex, "AFFECT_BUFFER_" + j.ToString());
    //        if (skillIndex <= 0)
    //            continue;

    //        iNum++;
    //    }

    //    return iNum;
    //}

    private void UpdatePetSkillInfo(int iBtnIndex)
    {
        GameObject obj = GameCommon.FindObject(mGameObjUI, "pet_skill_btn_" + iBtnIndex.ToString());
        int skillIndex = mCurSelPetData.GetSkillIndexByIndex(iBtnIndex);
        if (obj == null)
            return;

        obj.SetActive(skillIndex > 0);
        if (skillIndex > 0)
        {
            PetSkillData petSkillData = mCurSelPetData.GetPetSkillDataByIndex(iBtnIndex);
            if (petSkillData != null)
            {
                // name
                string strName = GameCommon.GetSkillName(skillIndex, "NAME");
                UILabel nameAndLevelLabel = GameCommon.FindComponent<UILabel>(obj, "skill_name_and_level");
                nameAndLevelLabel.text = strName;

                // description
                string strDescription = GameCommon.GetSkillDescription(skillIndex, "INFO");
                if (strDescription != "")
                {
                    strDescription = strDescription.Replace("\\n", "");
                    strDescription = strDescription.Replace("\n", "");
                }
                UILabel skillIntroduceLabel = GameCommon.FindComponent<UILabel>(obj, "skill_introduce_label");
                skillIntroduceLabel.text = strDescription;
                
                // icon
                string strAtlasName = GameCommon.GetStringFromSkill(skillIndex, "SKILL_ATLAS_NAME");
                string strSpriteName = GameCommon.GetStringFromSkill(skillIndex, "SKILL_SPRITE_NAME");
                GameCommon.SetUISprite(obj, "background", strAtlasName, strSpriteName);
            }
        }
    }

    private void UpdataSelSkillInfo()
    {
        UIToggle toggle = GameCommon.FindComponent<UIToggle>(mGameObjUI, "pet_skill_btn_" + mCurSelSkillBtnIndex.ToString());
        if (toggle != null)
        {
            toggle.value = true;

            UpdateChoosePetSkillIcon();
            UpdateChooseGemIcon();
        }
    }

    private void UpdateChoosePetSkillIcon()
    {
        GameObject obj = GameCommon.FindObject(mGameObjUI, "choose_skill");
        int skillIndex = mCurSelPetData.GetSkillIndexByIndex(mCurSelSkillBtnIndex);
        string strAtlasName = GameCommon.GetStringFromSkill(skillIndex, "SKILL_ATLAS_NAME");
        string strSpriteName = GameCommon.GetStringFromSkill(skillIndex, "SKILL_SPRITE_NAME");
        GameCommon.SetUISprite(obj, "background", strAtlasName, strSpriteName);

        GameObject skillForwardlyFrame = GameCommon.FindObject(obj, "skill_forwardly_frame");
        GameObject skillForwardlyFlag = GameCommon.FindObject(obj, "skill_forwardly_flag");
        GameObject skillPassiveFrame = GameCommon.FindObject(obj, "skill_passive_frame");
        GameObject skillPassiveFlag = GameCommon.FindObject(obj, "skill_passive_flag");
        skillForwardlyFrame.SetActive(mCurSelSkillBtnIndex == 0);
        skillForwardlyFlag.SetActive(mCurSelSkillBtnIndex == 0);
        skillPassiveFrame.SetActive(mCurSelSkillBtnIndex != 0);
        skillPassiveFlag.SetActive(mCurSelSkillBtnIndex != 0);
    }

    private void UpdateChooseGemIcon()
    {
        // icon
        GameObject obj = GameCommon.FindObject(mGameObjUI, "choose_stone");
        int skillIndex = mCurSelPetData.GetSkillIndexByIndex(mCurSelSkillBtnIndex);
        int iIndex = GameCommon.GetNumberFromSkill(skillIndex, "ITEM_ID");
        string strAtlasName = TableCommon.GetStringFromStoneTypeIconConfig(iIndex, "STONE_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromStoneTypeIconConfig(iIndex, "STONE_SPRITE_NAME");
        GameCommon.SetUISprite(obj, "background", strAtlasName, strSpriteName);

        // num
        int iNeedGemNum = GameCommon.GetNumberFromSkill(skillIndex, "ITEM_NUM");
        UILabel numLabel = GameCommon.FindComponent<UILabel>(obj, "StoneNumLabel");
        GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
        GemData gemData = gemLogicData.GetGemDataByIndex(iIndex - 1000);
        int iHadGemCount = 0;
        if (gemData != null)
        {
            iHadGemCount = gemData.mCount;
            string strCountInfo = iHadGemCount.ToString() + "[ffffff]/" + iNeedGemNum.ToString();

            if (iHadGemCount < iNeedGemNum)
            {
                strCountInfo = "[ff0000]" + strCountInfo;
            }

            numLabel.text = strCountInfo;
        }

        int iSkillIndex = mCurSelPetData.GetSkillIndexByIndex(mCurSelSkillBtnIndex);
        // set icon
        int iNeedCoinType = GameCommon.GetNumberFromSkill(iSkillIndex, "COST_TYPE");
        int iNeedCoinID = GameCommon.GetNumberFromSkill(iSkillIndex, "COST_ID");
        int iNeedCoinNum = GameCommon.GetNumberFromSkill(iSkillIndex, "COST_NUM");
        //UISprite needCoinIcon = GameCommon.FindComponent<UISprite>(mGameObjUI, "coin_sprite");
        //GameCommon.SetItemIcon(needCoinIcon, (ITEM_TYPE)iNeedCoinType, iNeedCoinID);
        // set number
        UILabel needCoinNumLabel = GameCommon.FindComponent<UILabel>(mGameObjUI, "NeedCoinNumLabel");
        needCoinNumLabel.text = iNeedCoinNum.ToString();

        int iSkillLevel = mCurSelPetData.GetSkillLevelByIndex(mCurSelSkillBtnIndex);
        if (iSkillLevel < mCurSelPetData.level && iSkillLevel < 10 && iHadGemCount >= iNeedGemNum && RoleLogicData.Self.gold >= iNeedCoinNum)
        {
            GameCommon.FindObject(mGameObjUI, "skill_upgrade_button").SetActive(true);
            GameCommon.FindObject(mGameObjUI, "skill_upgrade_button_gray").SetActive(false);
        }
        else
        {
            GameCommon.FindObject(mGameObjUI, "skill_upgrade_button").SetActive(false);
            GameCommon.FindObject(mGameObjUI, "skill_upgrade_button_gray").SetActive(true);
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        mCurPetID = 0;
        mCurSelPetData = null;
    }

    public void UpgradePetSkill()
    {
        tEvent quest = Net.StartEvent("CS_RequestPetSkillLevelUp");
        quest.set("PET_ID", mCurPetID);
        quest.set("SKILL_ID", mCurSelPetData.GetPetSkillDataByIndex(mCurSelSkillBtnIndex).index);
        quest.set("SKILL_INDEX", mCurSelSkillBtnIndex + 1);
        quest.DoEvent();
    }

    public void UpgradePetSkillResult()
    {
        GameCommon.FindObject(mGameObjUI, "skill_upgrade_effect").SetActive(true);

        // update gem data
        int skillIndex = mCurSelPetData.GetSkillIndexByIndex(mCurSelSkillBtnIndex);
        int iIndex = GameCommon.GetNumberFromSkill(skillIndex, "ITEM_ID");
        int iNeedNum = GameCommon.GetNumberFromSkill(skillIndex, "ITEM_NUM");
        GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
        GemData gemData = gemLogicData.GetGemDataByIndex(iIndex - 1000);
        if (gemData != null)
        {
            gemData.mCount -= iNeedNum;
            if (gemData.mCount < 0)
                gemData.mCount = 0;
        }

        // update gold
        // set icon
        int iNeedCoinType = GameCommon.GetNumberFromSkill(skillIndex, "COST_TYPE");
        int iNeedCoinID = GameCommon.GetNumberFromSkill(skillIndex, "COST_ID");
        int iNeedCoinNum = GameCommon.GetNumberFromSkill(skillIndex, "COST_NUM");
        RoleLogicData.Self.AddGold((-1) * iNeedCoinNum);

        // update pet data
        mCurSelPetData.mDicPetSkill[mCurSelSkillBtnIndex].level++;

        DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_GEM_ICONS", true);
        DataCenter.SetData("BAG_INFO_WINDOW", "CLEAR_STONE_SELECT_STATE", true);

        Refresh(mCurPetID);
    }

}

public class Button_PetSkillTypeEvent : CEvent
{
    public override bool _DoEvent()
    {
        string[] names = GetEventName().Split('_');

        DataCenter.SetData("PET_SKILL_WINDOW", "SELECT_PET_SKILL", int.Parse(names[names.Length - 1]));
        return true;
    }
}

public class Button_UnChooseUpgradeSkillPet : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PET_SKILL_WINDOW", "SET_SELECT_UPGRADE_SKILL_PET", 0);
        return true;
    }
}

public class Button_SkillUpgradeButton : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PET_SKILL_WINDOW", "UPGRADE_PET_SKILL", true);
        return true;
    }
}

class CS_RequestPetSkillLevelUp : BaseNetEvent
{
    public override void _OnResp(tEvent respEvt)
    {
        int bResult = respEvt.get("RESULT");
        if ((STRING_INDEX)bResult == STRING_INDEX.ERROR_NONE)
            DataCenter.SetData("PET_SKILL_WINDOW", "UPGRADE_PET_SKILL_RESULT", true);
    }
}