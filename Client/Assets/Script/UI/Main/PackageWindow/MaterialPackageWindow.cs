using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using System.Linq;

public class MaterialPackageWindow : ItemPackageBaseWindow {

    MaterialData mCurSelMaterialData = null;
    public SORT_TYPE mSortType;

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_package_material_icon_btn", new DefineFactory<Button_PackageMaterialIconBtn>());
        EventCenter.Self.RegisterEvent("Button_material_package_sale_button", new DefineFactory<Button_MaterialPackageSaleBtn>());
        EventCenter.Self.RegisterEvent("Button_material_package_control_button", new DefineFactory<Button_MaterialPackageControlBtn>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        mCurSelMaterialData = MaterialLogicData.Self.GetMaterialDataFromGridIndex(mCurSelGridIndex);

        if (mGrid.MaxCount > 0)
            GameCommon.ToggleTrue(GameCommon.FindObject(mGrid.controlList[mCurSelGridIndex], "package_material_icon_btn"));

        RefreshInfoWindow();
        return true;
    }

    public override void InitVariable()
    {
        base.InitVariable();
        mSortType = SORT_TYPE.NUMBER;
        InitPackageIcons();
        RefreshBagNum();
        Refresh(0);
    }

    public override void InitPackageIcons()
    {
        base.InitPackageIcons();
        mGrid.MaxCount = MaterialLogicData.Self.mDicMaterial.Count;

        RefreshMaterialIcons();
    }

    public void RefreshMaterialIcons()
    {
        int iIndex = 0;

        List<MaterialData> materialDataList = MaterialLogicData.Self.mDicMaterial.Values.ToList();
        materialDataList = SortList(materialDataList);
        for (int i = 0; i < materialDataList.Count; i++)
        {
            MaterialData materialData = materialDataList[i];
            if (materialData != null)
            {
                materialData.mGridIndex = iIndex;
                GameObject tempObj = mGrid.controlList[i];
                GameObject obj = tempObj.transform.Find("package_material_icon_btn").gameObject;
                if (obj != null)
                {
                    UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
                    buttonEvent.mData.set("GRID_INDEX", materialData.mGridIndex);
                    // set icon
                    SetMaterialDataIcon(obj, materialData, false);

                    iIndex++;
                }
            }
        }
    }

    public List<MaterialData> SortList(List<MaterialData> list)
    {
        if (list != null && list.Count > 0)
        {
            switch (mSortType)
            {
                case SORT_TYPE.NUMBER:
                    list = GameCommon.SortList(list, GameCommon.SortMaterialDataByNumber);
                    break;
            }
        }

        return list;
    }

    public bool SetMaterialDataIcon(GameObject obj, MaterialData materialData, bool bIsUsed)
    {
        if (obj == null)
            return false;

        GameObject background = GameCommon.FindObject(obj, "Background");
        GameObject emptyBG = GameCommon.FindObject(obj, "empty_bg");

        if (materialData == null)
        {
            background.SetActive(false);
            emptyBG.SetActive(true);

            UIToggle toggle = obj.GetComponent<UIToggle>();
            if (toggle != null)
                toggle.value = false;
        }
        else
        {
            background.SetActive(true);
            emptyBG.SetActive(false);

            UIToggle toggle = obj.GetComponent<UIToggle>();
            if (toggle != null)
                toggle.value = bIsUsed;

            SetIcon(obj, materialData);
        }

        return true;
    }

    public void SetIcon(GameObject obj, MaterialData materialData)
    {
        if (obj != null && materialData != null)
        {
            // set  icon
            SetMaterialIcon(obj, materialData);

            // set number text
            GameCommon.SetUIText(obj, "num_label", materialData.mCount.ToString());
        }
    }

    public void SetMaterialIcon(GameObject obj, MaterialData materialData)
    {
        string strAtlasName = TableCommon.GetStringFromMaterialConfig(materialData.mIndex, "ICON_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromMaterialConfig(materialData.mIndex, "ICON_SPRITE_NAME");

        GameCommon.SetIcon(obj, "Background", strSpriteName, strAtlasName);
    }

    public override void RefreshBagNum()
    {
        UILabel numLabel = mGameObjUI.transform.Find("num_label").GetComponent<UILabel>();
        if (numLabel != null)
        {
            numLabel.text = MaterialLogicData.Self.mDicMaterial.Count.ToString();
        }
    }

    public override void RefreshInfoWindow()
    {
        GameObject group = mGameObjUI.transform.Find("info_window/group").gameObject;
        GameObject emptyGroup = mGameObjUI.transform.Find("info_window/empty_bg").gameObject;
        if (mCurSelMaterialData == null)
        {
            group.SetActive(false);
            emptyGroup.SetActive(true);
        }
        else
        {
            group.SetActive(true);
            emptyGroup.SetActive(false);

            SetIcon(group, mCurSelMaterialData);
            
            // set name
            UILabel name = GameCommon.FindObject(group, "props_name").GetComponent<UILabel>();
            name.text = TableCommon.GetStringFromMaterialConfig(mCurSelMaterialData.mIndex, "NAME");

            // set description
            UILabel descrition = GameCommon.FindObject(group, "introduce_label").GetComponent<UILabel>();
            descrition.text = TableCommon.GetStringFromMaterialConfig(mCurSelMaterialData.mIndex, "DESCRIPTION");
        }
    }

    public void SortMaterialIcons(SORT_TYPE sortType)
    {
        mSortType = sortType;

        InitPackageIcons();
    }
}


public class Button_PackageMaterialIconBtn : CEvent
{
    public override bool _DoEvent()
    {
        object val;
        bool b = getData("BUTTON", out val);
        GameObject obj = val as GameObject;
        UIToggle toggle = obj.GetComponent<UIToggle>();
        toggle.value = !toggle.value;

        MaterialLogicData logicData = DataCenter.GetData("MATERIAL_DATA") as MaterialLogicData;
        int iGridIndex = (int)get("GRID_INDEX");
        MaterialData data = logicData.GetMaterialDataFromGridIndex(iGridIndex);

        if (data != null)
        {
            DataCenter.SetData("MATERIAL_PACKAGE_WINDOW", "REFRESH", iGridIndex);
        }

        return true;
    }
}

public class Button_MaterialPackageSaleBtn : CEvent
{
    public override bool _DoEvent()
    {
        return true;
        DataCenter.SetData("MATERIAL_PACKAGE_WINDOW", "SALE", true);

        return true;
    }
}

public class Button_MaterialPackageControlBtn : CEvent
{
    public override bool _DoEvent()
    {
        if (CommonParam.bIsNetworkGame)
        {
            CS_RequestMaterial quest = Net.StartEvent("CS_RequestMaterial") as CS_RequestMaterial;
            //quest.set("WINDOW_NAME", "ROLE_EQUIP_CULTIVATE_WINDOW");
            //quest.mAction = () =>
            //{
            //    DataCenter.CloseWindow("PACKAGE_WINDOW");
            //    MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
            //};
            //quest.mBackAction = () =>
            //{
            //    EventCenter.Start("Button_package_btn").DoEvent();
            //};
            //quest.DoEvent();

            //tEvent gemQuest = Net.StartEvent("CS_RequestGem");
            //gemQuest.DoEvent();
        }
        else
        {
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
            MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "ROLE_EQUIP_CULTIVATE_WINDOW";
        }
        return true;
    }
}