using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

public class CompositionItem
{
    public ITEM_TYPE mItemType;
    public int mIndex;
}

public class RoleEquipCompositionWindow : tWindow {

    int mCurSelGridIndex = 0;
    int mCurSelItemType = 0;
    int mCurSelItemIndex = 0;

    List<CompositionItem> mSelCompositionItemList = new List<CompositionItem>();
    UIGridContainer mSelCompositionItemsGrid;
    UIGridContainer mCurCompositionNeedItemGrid;

    GameObject mTopGroup;
    GameObject mMiddleGroup;
    GameObject mBottomGroup;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_composition_items_icon_btn", new DefineFactory<Button_CompositionItemsIconBtn>());
        EventCenter.Self.RegisterEvent("Button_composition_need_item_icon_btn", new DefineFactory<Button_CompositionNeedItemIconBtn>());
        EventCenter.Self.RegisterEvent("Button_item_composition_button", new DefineFactory<Button_ItemCompositionButton>());
		EventCenter.Self.RegisterEvent("Button_equip_composition_go_level_button", new DefineFactory<Button_equip_composition_go_level_button>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "SET_CUR_SEL_ITEM_FROM_INFO_WINDOW":
                SetCurSelItemFromInfoWindow((int)objVal);
                break;
            case "SET_CUR_SEL_ITEM_FROM_LIST":
                SetCurSelItemFromList((int)objVal);
                break;
            case "SET_CUR_SEL_ITEM_FROM_NEED":
                SetCurSelItemFromNeed((int)objVal);
                break;
            case "COMPISITON_OK":
                CompositionOk();
                break;
            case "COMPISITON_RESULT":
                CompositionResult((int)objVal);
                break;
        }
    }    

    public override void OnOpen()
    {
        DataCenter.CloseWindow("ROLE_INFO_WINDOW");
        DataCenter.CloseWindow("STAR_LEVEL_UP_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_CULTIVATE_WINDOW");

        mTopGroup = GameCommon.FindObject(mGameObjUI, "top_infos");
        mMiddleGroup = GameCommon.FindObject(mGameObjUI, "middle_infos");
        mBottomGroup = GameCommon.FindObject(mGameObjUI, "bottom_infos");

        mSelCompositionItemsGrid = GameCommon.FindComponent<UIGridContainer>(mTopGroup, "grid");
        mCurCompositionNeedItemGrid = GameCommon.FindComponent<UIGridContainer>(mBottomGroup, "grid");

        DataCenter.OpenWindow("COMPOSITION_INFO_WINDOW");

        //Refresh(0);

        InitButtonIsUnLockOrNot();
    }

    public override bool Refresh(object param)
    {
        RefeshSelCompositionItemsInfo();
        RefeshCurSelCompositionItemInfo();
        RefeshCurCompositionNeedItemInfo();
        
        return true;
    }

    void InitButtonIsUnLockOrNot()
    {
    }

    public override void Close()
    {
        DataCenter.CloseWindow("COMPOSITION_INFO_WINDOW");

        base.Close();
    }

    private void SetCurSelItemFromInfoWindow(int iGridIndex)
    {
        mCurSelGridIndex = iGridIndex;
        CompositionInfoWindow window = DataCenter.GetData("COMPOSITION_INFO_WINDOW") as CompositionInfoWindow;
        window.GetInfo(mCurSelGridIndex, ref mCurSelItemIndex, ref mCurSelItemType);
        mSelCompositionItemList.Clear();
        CompositionItem curItem = new CompositionItem();
        curItem.mItemType = (ITEM_TYPE)mCurSelItemType;
        curItem.mIndex = mCurSelItemIndex;

        mSelCompositionItemList.Add(curItem);

        Refresh(null);
    }

    private void SetCurSelItemFromList(int iGridIndex)
    {
        if (iGridIndex >= mSelCompositionItemList.Count - 1)
            return;

        CompositionItem compositionItem = mSelCompositionItemList[iGridIndex];

        mCurSelItemIndex = compositionItem.mIndex;
        mCurSelItemType = (int)compositionItem.mItemType;

        for (int i = iGridIndex + 1; i < mSelCompositionItemList.Count; )
        {
            mSelCompositionItemList.RemoveAt(i);
        }

        Refresh(null);
    }

    private void SetCurSelItemFromNeed(int iGridIndex)
    {
        if (iGridIndex >= mCurCompositionNeedItemGrid.MaxCount)
            return;

        GameObject obj = mCurCompositionNeedItemGrid.controlList[iGridIndex];
        NiceData data = GameCommon.GetButtonData(obj, "composition_need_item_icon_btn");
        if (data != null)
        {
            mCurSelItemIndex = data.get("ITEM_INDEX");
            mCurSelItemType = data.get("ITEM_TYPE");
            CompositionItem curItem = new CompositionItem();
            curItem.mItemType = (ITEM_TYPE)mCurSelItemType;
            curItem.mIndex = mCurSelItemIndex;

            mSelCompositionItemList.Add(curItem);

            Refresh(null);
        }
    }

    private void CompositionOk()
    {
        CS_ComposeFabao quest = Net.StartEvent("CS_ComposeFabao") as CS_ComposeFabao;
        quest.set("ID", (int)(GetMaterialSynthesisRecord()["INDEX"]));
        quest.set("TYPE", mCurSelItemType);
        quest.mAction = () =>
        {
            int iItemId = quest.get("ROLE_EQUIP_DB_ID");
            DataCenter.SetData("ROLE_EQUIP_COMPOSITION_WINDOW", "COMPISITON_RESULT", iItemId);
        };
        quest.DoEvent();
    }

    private void CompositionResult(int iItemId)
    {
        RefreshNeedItemData((int)(GetMaterialSynthesisRecord()["INDEX"]));
        
        RefeshCurCompositionNeedItemInfo();

        if (mCurSelItemType == (int)ITEM_TYPE.EQUIP)
            ShowInfoWindow(iItemId);
    }

    private void ShowInfoWindow(int iItemId)
    {
        DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "WINDOW_TYPE", (int)RoleEquipInfoWindow.WINDOW_TYPE.COMPOSITION);
        DataCenter.OpenWindow("ROLE_EQUIP_INFO_WINDOW", iItemId);
    }

    private void RefreshNeedItemData(int iIndex)
    {
        DataRecord record = DataCenter.mMaterialSynthesis.GetRecord(iIndex);
        if (record != null)
        {
            RoleLogicData.Self.AddGold(((int)record["PRICE"]) * (-1));

            mCurCompositionNeedItemGrid.MaxCount = GetCompositionNeedItemNumber(record);
            if (mCurCompositionNeedItemGrid.MaxCount > 0)
            {
                for (int i = 1; i <= 4; i++)
                {
                    if (!IsHadMaterial(record, i))
                        continue;

                    switch((ITEM_TYPE)((int)record["TYPE_" + i.ToString()]))
                    {
                        case ITEM_TYPE.EQUIP:
                            RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
                            roleEquipLogicData.RemoveRoleEquipByModelIndex((int)record["MATERIAL_" + i.ToString()]);
                            break;
                        case ITEM_TYPE.MATERIAL:                            
                            MaterialLogicData materialLogicData = DataCenter.GetData("MATERIAL_DATA") as MaterialLogicData;
                            materialLogicData.ChangeMaterialDataNum((int)record["MATERIAL_" + i.ToString()], ((int)record["NUMBER_" + i.ToString()]) * (-1));
                            break;
                        case ITEM_TYPE.MATERIAL_FRAGMENT:
                            MaterialFragmentLogicData materialFragmentLogicData = DataCenter.GetData("MATERIAL_FRAGMENT_DATA") as MaterialFragmentLogicData;
                            materialFragmentLogicData.ChangeMaterialFragmentDataNum((int)record["MATERIAL_" + i.ToString()], ((int)record["NUMBER_" + i.ToString()]) * (-1));
                            break;
                    }
                }
            }
        }
    }

    private void RefeshSelCompositionItemsInfo()
    {
        mSelCompositionItemsGrid.MaxCount = mSelCompositionItemList.Count;
        for (int i = 0; i < mSelCompositionItemsGrid.MaxCount; i++ )
        {
            GameObject obj = mSelCompositionItemsGrid.controlList[i];
            CompositionItem compositionItem = mSelCompositionItemList[i];

            NiceData data = GameCommon.GetButtonData(obj, "composition_items_icon_btn");
            if (data != null)
            {
                data.set("GRID_INDEX", i);
            }

            SetItemIcons(obj, compositionItem.mItemType, compositionItem.mIndex);
            GameCommon.FindObject(obj, "next_sprite").SetActive(i != 0);
            GameCommon.FindObject(obj, "select_mark_sprite").SetActive(i == mSelCompositionItemsGrid.MaxCount - 1);
        }

        // TODO
        //UIScrollView scrollView = GameCommon.FindComponent<UIScrollView>(mTopGroup, "Scroll View");
        //if (mSelCompositionItemsGrid.MaxCount >= 6)
        //    mSelCompositionItemsGrid.uiScrollBar.value = 1.0f;
        //else
        //    mSelCompositionItemsGrid.uiScrollBar.value = 0;
    }

    private void RefeshCurSelCompositionItemInfo()
    {
        // set name
        UILabel nameLabel = GameCommon.FindComponent<UILabel>(mMiddleGroup, "name_label");
        if (mCurSelItemType == (int)ITEM_TYPE.EQUIP)
        {
            nameLabel.text = TableCommon.GetStringFromRoleEquipConfig(mCurSelItemIndex, "NAME");
            UILabel attachAttributeNumLabel = GameCommon.FindComponent<UILabel>(mMiddleGroup, "add_attribute_nember");
            attachAttributeNumLabel.text = TableCommon.GetNumberFromRoleEquipConfig(mCurSelItemIndex, "ATTACHATTRIBUTE_NUM").ToString();

            SetBaseAttribute();
        }
        else
        {
            UILabel descriptionLabel = GameCommon.FindComponent<UILabel>(mMiddleGroup, "description_label");
            if (mCurSelItemType == (int)ITEM_TYPE.MATERIAL)
            {
                nameLabel.text = TableCommon.GetStringFromMaterialConfig(mCurSelItemIndex, "NAME");
                descriptionLabel.text = TableCommon.GetStringFromMaterialConfig(mCurSelItemIndex, "DESCRIPTION");
            }
            else if (mCurSelItemType == (int)ITEM_TYPE.MATERIAL_FRAGMENT)
            {
                nameLabel.text = TableCommon.GetStringFromMaterialFragment(mCurSelItemIndex, "NAME");
                descriptionLabel.text = TableCommon.GetStringFromMaterialFragment(mCurSelItemIndex, "DESCRIPTION");
            }
        }
        SetItemIcons(mMiddleGroup, (ITEM_TYPE)mCurSelItemType, mCurSelItemIndex);
    }

    private void RefeshCurCompositionNeedItemInfo()
    {
        DataRecord record = GetMaterialSynthesisRecord();
        if (record != null)
        {
            mCurCompositionNeedItemGrid.MaxCount = GetCompositionNeedItemNumber(record);

            int iGridIndex = 0;
            if (mCurCompositionNeedItemGrid.MaxCount > 0)
            {
                for (int i = 1; i <= 4; i++)
                {
                    if (IsHadMaterial(record, i))
                    {
                        SetNeedMaterialIcons(record, i, iGridIndex);
                        iGridIndex++;
                    }
                }
                RefreshNeedGoldNum((int)record["PRICE"]);
            }
        }

        GameCommon.FindObject(mBottomGroup, "bottom_equip_infos").SetActive(record != null && mCurCompositionNeedItemGrid.MaxCount > 0);
        GameCommon.FindObject(mBottomGroup, "no_compound_equip_infos").SetActive(record == null || mCurCompositionNeedItemGrid.MaxCount <= 0);

        if (record == null || mCurCompositionNeedItemGrid.MaxCount <= 0)
        {
            RefreshGetWayUI();
        }
    }

    private void RefreshGetWayUI()
    {
        int iIndex = 0;
        if (mCurSelItemType == (int)ITEM_TYPE.EQUIP)
            iIndex = TableCommon.GetNumberFromRoleEquipConfig(mCurSelItemIndex, "GET_WAY_INDEX");
        else if (mCurSelItemType == (int)ITEM_TYPE.MATERIAL)
            iIndex = TableCommon.GetNumberFromMaterialConfig(mCurSelItemIndex, "GET_WAY_INDEX");
        else if (mCurSelItemType == (int)ITEM_TYPE.MATERIAL_FRAGMENT)
            iIndex = TableCommon.GetNumberFromMaterialFragment(mCurSelItemIndex, "GET_WAY_INDEX");

        int iCount = GetStageCount(iIndex);
        GameCommon.FindObject(mBottomGroup, "buy_in_shop").SetActive(iCount <= 0);
        GameCommon.FindObject(mBottomGroup, "stage_scroll_view").SetActive(iCount > 0);

        if (iCount <= 0)
        {
            string strDecs = TableCommon.GetStringFromMaterialPrompt(iIndex, "GET_WAY_DESC");
            if (strDecs != string.Empty)
            {
                GameCommon.FindComponent<UILabel>(mBottomGroup, "buy_in_shop").text = strDecs;
            }
        }
        else
        {
            UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mBottomGroup, "Grid");
            if (grid != null)
            {
                grid.MaxCount = iCount;
                for (int i = 0; i < grid.MaxCount; i++ )
                {
                    int iStageIndex = TableCommon.GetNumberFromMaterialPrompt(iIndex, "GET_STATE_" + (i+1).ToString());
                    if (iStageIndex > 0)
                    {
						GameObject obj = grid.controlList[i];
                        int iDifficulty = (iStageIndex - (iStageIndex / 1000 * 1000)) / 100;
                        string strDifficultyName = string.Empty;
                        string strPassColor = string.Empty;
                        bool bIsPassed = MapLogicData.Instance.GetMapDataByStageIndex(iStageIndex).successCount > 0;
                        if(bIsPassed)
                            strPassColor = "[999999]";
                        
                        switch ((STAGE_DIFFICULTY)iDifficulty)
                        {
                            case STAGE_DIFFICULTY.COMMON:
                                strDifficultyName = "[FFFFFF]" + strPassColor + "普通难度[-]";
							GameCommon.GetButtonData (GameCommon.FindObject (obj, "equip_composition_go_level_button")).set ("DIFFICULTY_INDEX",1 );
                                break;
                            case STAGE_DIFFICULTY.DIFFICUL:
                                strDifficultyName = "[FFA92E]" + strPassColor + "高手难度[-]";
							GameCommon.GetButtonData (GameCommon.FindObject (obj, "equip_composition_go_level_button")).set ("DIFFICULTY_INDEX",2 );
                                break;
                            case STAGE_DIFFICULTY.MASTER:
                                strDifficultyName = "[FF0000]" + strPassColor + "大师难度[-]";
							GameCommon.GetButtonData (GameCommon.FindObject (obj, "equip_composition_go_level_button")).set ("DIFFICULTY_INDEX",3);
                                break;

                        }
                        string strStageNum = "[99FF00]" + strPassColor + TableCommon.GetStringFromStageConfig(iStageIndex, "STAGENUMBER") + "[-]";
                        string strStageName = strPassColor + TableCommon.GetStringFromStageConfig(iStageIndex, "NAME") + "[-]";

                        GameCommon.FindComponent<UILabel>(obj, "difficulty_label").text = strDifficultyName;
                        GameCommon.FindComponent<UILabel>(obj, "level_number_label").text = strStageNum;
                        GameCommon.FindComponent<UILabel>(obj, "level_name_label").text = strStageName;
						GameCommon.SetUIVisiable (obj ,"equip_composition_go_level_button",true   );
						GameCommon.SetUIVisiable (obj ,"equip_composition_not_go_level_button",true   );

						List<DataRecord> stageID = TableCommon.FindAllRecords (DataCenter.mStagePoint, lhs => {return lhs["STAGEID"] == iStageIndex;});

						for(int j = 0;j < 1; j++)
						{
							DataRecord itemRecord = stageID[j];
							int levelIndex = itemRecord.get ("INDEX");
							StageProperty property = StageProperty.Create(itemRecord.get ("STAGEID"));
							if(property.unlocked )
							{
								GameCommon.SetUIVisiable (obj ,"equip_composition_not_go_level_button",false );
							}else 
							{
								GameCommon.SetUIVisiable (obj ,"equip_composition_go_level_button",false );
							}
							GameCommon.GetButtonData (GameCommon.FindObject (obj, "equip_composition_go_level_button")).set ("LEVEL_INDEX",levelIndex );
						}

                    }
                }
            }
        }

    }

    private int GetStageCount(int iIndex)
    {
        int iCount = 0;
        for (int i = 1; i <= 3; i++ )
        {
            if (TableCommon.GetNumberFromMaterialPrompt(iIndex, "GET_STATE_" + i.ToString()) > 0)
                iCount++;
        }

        return iCount;
    }

    private void RefreshNeedGoldNum(int iPrice)
    {
        UILabel label = GameCommon.FindObject(mBottomGroup, "need_gold_num").GetComponent<UILabel>();

        string strColor = "[ffffff]";
        if (iPrice > RoleLogicData.Self.gold)
        {
            strColor = "[ff0000]";
        }

        label.text = strColor + iPrice.ToString();
    }

    private DataRecord GetMaterialSynthesisRecord()
    {
        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mMaterialSynthesis.GetAllRecord())
        {
            if (pair.Value["ITEM_TYPE"] == mCurSelItemType && pair.Value["ITEM_INDEX"] == mCurSelItemIndex)
                return pair.Value;
        }
        return null;
    }

    private int GetCompositionNeedItemNumber(DataRecord record)
    {
        int iCount = 0;
        if (record != null)
        {
            for (int i = 1; i <= 4; i++)
            {
                if (IsHadMaterial(record, i))
                    iCount++;
            }
        }
        return iCount;        
    }

    private bool IsHadMaterial(DataRecord record, int iIndex)
    {
        if(record != null && record["MATERIAL_" + iIndex.ToString()] != 0)
        {
            return true;
        }
        return false;
    }

    private void SetNeedMaterialIcons(DataRecord record, int i, int iGridIndex)
    {
        if (record != null && iGridIndex < mCurCompositionNeedItemGrid.MaxCount)
        {
            GameObject obj = mCurCompositionNeedItemGrid.controlList[iGridIndex];
            int iItemType = (int)record["TYPE_" + i.ToString()];
            int iIndex = (int)record["MATERIAL_" + i.ToString()];
            NiceData data = GameCommon.GetButtonData(obj, "composition_need_item_icon_btn");
            if (data != null)
            {
                data.set("ITEM_TYPE", iItemType);
                data.set("ITEM_INDEX", iIndex);
                data.set("GRID_INDEX", iGridIndex);
            }

            SetItemIcons(obj, (ITEM_TYPE)iItemType, iIndex);
            SetNeedItemNum(obj, (ITEM_TYPE)iItemType, iIndex, (int)record["NUMBER_" + i.ToString()]);
            iGridIndex++;
        }
    }

    private void SetItemIcons(GameObject obj, ITEM_TYPE itemType, int iIndex)
    {
        GameObject roleEquipGroup = GameCommon.FindObject(obj, "composition_role_equip_icon");
        GameObject materialGroup = GameCommon.FindObject(obj, "composition_material_icon");
        GameObject materialFragment = GameCommon.FindObject(obj, "material_fragment_sprite");
        roleEquipGroup.SetActive(itemType == ITEM_TYPE.EQUIP);
        materialGroup.SetActive(itemType == ITEM_TYPE.MATERIAL || itemType == ITEM_TYPE.MATERIAL_FRAGMENT);
        materialFragment.SetActive(itemType == ITEM_TYPE.MATERIAL_FRAGMENT);

        if (itemType == ITEM_TYPE.EQUIP)
        {
            SetEquipIcon(roleEquipGroup, DataCenter.mRoleEquipConfig.GetRecord(iIndex));

        }
        else
        {
            if (itemType == ITEM_TYPE.MATERIAL)
            {
                SetMaterialIcon(materialGroup, DataCenter.mMaterialConfig.GetRecord(iIndex));
            }
            else if (itemType == ITEM_TYPE.MATERIAL_FRAGMENT)
            {
                SetMaterialFragmentIcon(materialGroup, DataCenter.mMaterialFragment.GetRecord(iIndex));
            }
        }
    }

    private void SetNeedItemNum(GameObject obj, ITEM_TYPE itemType, int iIndex, int iNeedNum)
    {
        UILabel label = GameCommon.FindObject(obj, "need_number").GetComponent<UILabel>();
        if (label == null)
            return;

        int iHadItemNum = 0;
        if (itemType == ITEM_TYPE.EQUIP)
        {
            iHadItemNum = GetHadEquipNumByIndex(iIndex);
        }
        else
        {
            if (itemType == ITEM_TYPE.MATERIAL)
            {
                iHadItemNum = GetHadMaterialNumByIndex(iIndex);
            }
            else if (itemType == ITEM_TYPE.MATERIAL_FRAGMENT)
            {
                iHadItemNum = GetHadMaterialFragmentNumByIndex(iIndex);
            }
        }

        string strColor = "[ffffff]";
        if (iHadItemNum < iNeedNum)
        {
            strColor = "[ff0000]";
        }

        label.text = strColor + iHadItemNum.ToString() + "[ffffff]/" + iNeedNum.ToString();
    }

    //------------------------------------------------------------------------------------------
    // role equip
    //------------------------------------------------------------------------------------------
    private bool SetEquipIcon(GameObject obj, DataRecord record)
    {
        if (obj == null || record == null)
            return false;

        // set equip icon
        GameCommon.SetEquipIcon(obj, record["INDEX"]);

        // set element icon and set equip element background icon
        GameCommon.SetIcon(obj, "background_sprite", UICommonDefine.strEquipIconBackgroundSprite, UICommonDefine.strEquipIconBackgroundAtlas);

        // set star level
        GameCommon.SetStarLevelLabel(obj, record["STAR_LEVEL"]);

        return true;
    }

    private int GetHadEquipNumByIndex(int iIndex)
    {
        RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
        return logicData.GetNumByIndex(iIndex);
    }

    private void SetBaseAttribute()
    {
        for (int i = 0; i < 2; i++)
        {
            int iAttributeType = TableCommon.GetNumberFromRoleEquipConfig(mCurSelItemIndex, "ATTRIBUTE_TYPE_" + i.ToString());
            if (iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
                return;

            GameObject baseObj = GameCommon.FindObject(mMiddleGroup, "base_attribute_" + i.ToString());
            // set icon
            SetAttributeIcon(baseObj, iAttributeType);

            // set name
            SetAttributeName(baseObj, iAttributeType);

            // set base number
            SetBaseAttributeValue(i, baseObj, iAttributeType, 0);
        }
    }

    // set equip attribute icon
    public void SetAttributeIcon(GameObject obj, int iIndex)
    {
        string strAtlasName = TableCommon.GetStringFromEquipAttributeIconConfig(iIndex, "ICON_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromEquipAttributeIconConfig(iIndex, "ICON_SPRITE_NAME");

        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
        UISprite sprite = GameCommon.FindObject(obj, "icon").GetComponent<UISprite>();
        sprite.atlas = tu;
        sprite.spriteName = strSpriteName;
        //sprite.MakePixelPerfect();
    }

    // set attribute name
    public void SetAttributeName(GameObject obj, int iAttributeType)
    {
        if (obj == null || iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
            return;

        GameObject name = obj.transform.Find("name").gameObject;
        UILabel nameLabel = name.GetComponent<UILabel>();
        if (nameLabel != null)
            nameLabel.text = TableCommon.GetStringFromEquipAttributeIconConfig(iAttributeType, "NAME");
    }

    public void SetBaseAttributeValue(int iIndex, GameObject obj, int iAttributeType, int iStrengthenLevel)
    {
        UILabel numLabel = GetAttributeNumLabel(obj, iAttributeType);
        float fValue = 0.0f;
        if (numLabel != null)
        {
            fValue = RoleEquipData.GetBaseAttributeValue(iIndex, mCurSelItemIndex, iStrengthenLevel);
            SetAttributeValueUI(numLabel, iAttributeType, fValue);
        }
    }

    public UILabel GetAttributeNumLabel(GameObject obj, int iAttributeType)
    {
        if (obj == null || iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
            return null;

        string strAttributeType = GameCommon.ToAffectTypeString((AFFECT_TYPE)iAttributeType);
        bool bIsRate = strAttributeType.LastIndexOf("_RATE") > 0;

        GameObject number = obj.transform.Find("num_label").gameObject;
        return number.GetComponent<UILabel>();
    }

    public void SetAttributeValueUI(UILabel numLabel, int iAttributeType, float fValue)
    {
        if (numLabel == null || iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
            return;

        string strAttributeType = GameCommon.ToAffectTypeString((AFFECT_TYPE)iAttributeType);
        bool bIsRate = strAttributeType.LastIndexOf("_RATE") > 0;

        fValue = fValue / 10000;
        string strValue = "";
        if (bIsRate)
        {
            fValue *= 100;
            int iInteger = (int)fValue;
            int iDecimal = (int)((fValue - iInteger) * 100);

            string strDecimal = iDecimal.ToString();
            if (iDecimal < 10)
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

    //------------------------------------------------------------------------------------------
    // material
    //------------------------------------------------------------------------------------------ 
    public bool SetMaterialIcon(GameObject obj, DataRecord record)
    {
        if (obj == null || record == null)
            return false;

        // set icon
        SetMaterialIcon(obj, record["INDEX"]);

        return true;
    }

    private void SetMaterialIcon(GameObject obj, int iIndex)
    {
        string strAtlasName = TableCommon.GetStringFromMaterialConfig(iIndex, "ICON_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromMaterialConfig(iIndex, "ICON_SPRITE_NAME");
        GameCommon.SetIcon(obj, "icon_sprite", strSpriteName, strAtlasName);
    }

    private int GetHadMaterialNumByIndex(int iIndex)
    {
        MaterialLogicData logicData = DataCenter.GetData("MATERIAL_DATA") as MaterialLogicData;
        return logicData.GetNumByIndex(iIndex);
    }

    //------------------------------------------------------------------------------------------
    // material fragment
    //------------------------------------------------------------------------------------------
    public bool SetMaterialFragmentIcon(GameObject parentObj, DataRecord record)
    {
        if (parentObj == null || record == null)
            return false;
        
        // set icon
        GameObject obj = GameCommon.FindObject(parentObj, "icon_sprite");        
        GameCommon.SetItemIcon(obj.GetComponent<UISprite>(), ITEM_TYPE.MATERIAL_FRAGMENT, record["INDEX"]);

        return true;
    }

    private int GetHadMaterialFragmentNumByIndex(int iIndex)
    {
        MaterialFragmentLogicData logicData = DataCenter.GetData("MATERIAL_FRAGMENT_DATA") as MaterialFragmentLogicData;
        return logicData.GetNumByIndex(iIndex);
    }
}


public class Button_CompositionItemsIconBtn : CEvent
{
    public override bool _DoEvent()
    {
        int iGridIndex = get("GRID_INDEX");
        DataCenter.SetData("ROLE_EQUIP_COMPOSITION_WINDOW", "SET_CUR_SEL_ITEM_FROM_LIST", iGridIndex);
        return true;
    }
}

public class Button_CompositionNeedItemIconBtn : CEvent
{
    public override bool _DoEvent()
    {
        int iGridIndex = get("GRID_INDEX");
        DataCenter.SetData("ROLE_EQUIP_COMPOSITION_WINDOW", "SET_CUR_SEL_ITEM_FROM_NEED", iGridIndex);
        return true;
    }
}

public class Button_ItemCompositionButton : CEvent
{
    public override bool _DoEvent()
    {
        int iGridIndex = get("GRID_INDEX");
        DataCenter.SetData("ROLE_EQUIP_COMPOSITION_WINDOW", "COMPISITON_OK", true);
        return true;
    }
}
public class Button_equip_composition_go_level_button : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
		GlobalModule.DoLater(Select,0f);
		return base._DoEvent();
	}

	private void Select()
	{
		int index = get("LEVEL_INDEX");
		int difficultyIndex = get ("DIFFICULTY_INDEX");
		DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_LEFT", "INIT_POPUP_LIST", difficultyIndex);
		DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_DIFFICULTY", difficultyIndex);
		DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_POINT", index);
	}
}