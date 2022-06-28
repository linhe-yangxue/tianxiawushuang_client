using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using System.Linq;

public class MaterialFragmentPackageWindow : ItemPackageBaseWindow
{
    MaterialFragmentData mCurSelMaterialFragment = null;
    SORT_TYPE mSortType = SORT_TYPE.NUMBER;
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_package_material_fragment_icon_btn", new DefineFactory<Button_PackageMaterialFragmentIconBtn>());
        EventCenter.Self.RegisterEvent("Button_material_fragment_package_use_button", new DefineFactory<Button_MaterialFragmentPackageUseButton>());
        EventCenter.Self.RegisterEvent("Button_material_fragment_package_control_button", new DefineFactory<Button_MaterialFragmentPackageControlBtn>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "USE":                
			    UseOK();
                break;
            case "USE_RESULT":
			    UseResult();
                break;
        }
    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);
        mCurSelMaterialFragment = MaterialFragmentLogicData.Self.GetMaterialFragmentDataFromGridIndex(mCurSelGridIndex);

        if (mGrid.MaxCount > 0)
            GameCommon.ToggleTrue(GameCommon.FindObject(mGrid.controlList[mCurSelGridIndex], "package_material_fragment_icon_btn"));

        RefreshInfoWindow();
        return true;
    }

    public override void InitVariable()
    {
        mSortType = SORT_TYPE.NUMBER;
        InitPackageIcons();
        RefreshBagNum();
        Refresh(0);
    }

    public override void InitPackageIcons()
    {
        mGrid.MaxCount = MaterialFragmentLogicData.Self.mDicMaterialFragment.Count;

        InitFragmentIcons();
    }

    public void InitFragmentIcons()
    {
        int iIndex = 0;

        List<MaterialFragmentData> fragmentList = MaterialFragmentLogicData.Self.mDicMaterialFragment.Values.ToList();
        //fragmentList = SortList(fragmentList);consume_item_package_use_button
        for (int i = 0; i < fragmentList.Count; i++)
        {
            MaterialFragmentData fragment = fragmentList[i];
            if (fragment != null)
            {
                fragment.mGridIndex = iIndex;
                GameObject tempObj = mGrid.controlList[i];
                GameObject obj = tempObj.transform.Find("package_material_fragment_icon_btn").gameObject;
                if (obj != null)
                {
                    UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
                    buttonEvent.mData.set("GRID_INDEX", fragment.mGridIndex);
                    // set icon
                    SetFragmentIcon(obj, fragment, false);

                    iIndex++;
                }
            }
        }
    }

    public List<MaterialFragmentData> SortList(List<MaterialFragmentData> list)
    {
        if (list != null && list.Count > 0)
        {
            switch (mSortType)
            {
                case SORT_TYPE.NUMBER:
                    list = GameCommon.SortList(list, GameCommon.SortMaterialFragmentDataByNumber);
                    //				list = list.OrderByDescending(p => p.mCount).
                    //						ThenByDescending(p => TableCommon.GetNumberFromActiveCongfig(p.mComposeItemTid, "STAR_LEVEL")).
                    //						ThenBy(p => p.mModelIndex).
                    //						ThenBy(p => TableCommon.GetNumberFromActiveCongfig(p.mComposeItemTid, "ELEMENT_INDEX")).
                    //						ToList();
                    break;
            }
        }

        return list;
    }

    public bool SetFragmentIcon(GameObject obj, MaterialFragmentData fragmentData, bool bIsUsed)
    {
        if (obj == null)
            return false;

        GameObject background = GameCommon.FindObject(obj, "Background");
        GameObject emptyBG = GameCommon.FindObject(obj, "empty_bg");

        if (fragmentData == null)
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

            SetIcon(obj, fragmentData);
        }

        return true;
    }

    public void SetIcon(GameObject parentObj, MaterialFragmentData fragmentData)
    {
        if (parentObj != null && fragmentData != null)
        {
            // set icon
            GameObject obj = GameCommon.FindObject(parentObj, "Background");
            GameCommon.SetItemIcon(obj.GetComponent<UISprite>(), ITEM_TYPE.MATERIAL_FRAGMENT, fragmentData.mFragmentIndex);

            // set num text
            GameCommon.SetUIText(parentObj, "num_label", fragmentData.mCount.ToString());
        }
    }

    public override void RefreshBagNum()
    {
        UILabel fragmentNumLabel = mGameObjUI.transform.Find("num_label").GetComponent<UILabel>();
        if (fragmentNumLabel != null)
        {
            fragmentNumLabel.text = MaterialFragmentLogicData.Self.mDicMaterialFragment.Count.ToString();
        }
    }

    public override void RefreshInfoWindow()
    {
        GameObject fragmentGroup = mGameObjUI.transform.Find("info_window/group").gameObject;
        GameObject fragmentEmptyGroup = mGameObjUI.transform.Find("info_window/empty_bg").gameObject;
        if (mCurSelMaterialFragment == null)
        {
            fragmentGroup.SetActive(false);
            fragmentEmptyGroup.SetActive(true);
        }
        else
        {
            fragmentGroup.SetActive(true);
            fragmentEmptyGroup.SetActive(false);

            SetIcon(fragmentGroup, mCurSelMaterialFragment);           

            // set name
            UILabel name = GameCommon.FindObject(fragmentGroup, "props_name").GetComponent<UILabel>();
            name.text = TableCommon.GetStringFromMaterialFragment(mCurSelMaterialFragment.mFragmentIndex, "NAME");

            //set fragment_consume_number 
            UILabel fragment_consume_number = GameCommon.FindObject(fragmentGroup, "fragment_consume_number").GetComponent<UILabel>();
            fragment_consume_number.text = TableCommon.GetStringFromMaterialFragment(mCurSelMaterialFragment.mFragmentIndex, "COST_NUM");

            // set description
            UILabel descrition = GameCommon.FindObject(fragmentGroup, "introduce_label").GetComponent<UILabel>();
            descrition.text = TableCommon.GetStringFromMaterialFragment(mCurSelMaterialFragment.mFragmentIndex, "DESCRIPTION");
        }
    }

    public void SortFragmentIcons(SORT_TYPE sortType)
    {
        mSortType = sortType;

        InitPackageIcons();
    }

    public void UseOK()
    {
        if (mCurSelMaterialFragment == null)
            return;

        if (UseCondition())
        {
            if (CommonParam.bIsNetworkGame)
            {
                CS_ComposeMaterial quest = Net.StartEvent("CS_ComposeMaterial") as CS_ComposeMaterial;
                quest.set("ID", mCurSelMaterialFragment.mFragmentIndex);
                quest.mAction = () => DataCenter.SetData("MATERIAL_FRAGMENT_PACKAGE_WINDOW", "USE_RESULT", true);
                quest.DoEvent();
            }
            else
            {
                UseResult();
            }
        }
    }

    public void UseResult()
    {
        // set pet fragment data
        if (mCurSelMaterialFragment != null)
        {
            int iDNum = TableCommon.GetNumberFromMaterialFragment(mCurSelMaterialFragment.mFragmentIndex, "COST_NUM");
            bool isSuccess = MaterialFragmentLogicData.Self.ChangeMaterialFragmentDataNum(mCurSelMaterialFragment.mFragmentIndex, iDNum * (-1));

            // refresh pet fragment icons
            InitPackageIcons();

            if (isSuccess)
            {
                MaterialFragmentData materialFragmentData = MaterialFragmentLogicData.Self.GetMaterialFragmentDataFromIndex(mCurSelMaterialFragment.mFragmentIndex);
                if (materialFragmentData != null)
                    Refresh(materialFragmentData.mGridIndex);
            }
            else
            {
                Refresh(0);
            }

            // cost gold
            int iCostGoldNum = TableCommon.GetNumberFromMaterialFragment(mCurSelMaterialFragment.mFragmentIndex, "PRICE");
            GameCommon.RoleChangeGold(iCostGoldNum * (-1));

            int iModelIndex = TableCommon.GetNumberFromMaterialFragment(mCurSelMaterialFragment.mFragmentIndex, "MATERIAL_INDEX");
            //DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "OPEN", PET_INFO_WINDOW_TYPE.COMPOSE);
            //DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_BY_MODEL_INDEX", iModelIndex);
        }
    }

    public bool UseCondition()
    {
        if (mCurSelMaterialFragment != null)
        {
            if (TableCommon.GetNumberFromMaterialFragment(mCurSelMaterialFragment.mFragmentIndex, "COST_NUM") > mCurSelMaterialFragment.mCount)
            {
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MATERIAL_FRAGMENT_COUNT);
                return false;
            }
            if (TableCommon.GetNumberFromMaterialFragment(mCurSelMaterialFragment.mFragmentIndex, "PRICE") > RoleLogicData.Self.gold)
            {
                DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
                return false;
            }

            return true;
        }
        return false;
    }
}


public class Button_PackageMaterialFragmentIconBtn : CEvent
{
    public override bool _DoEvent()
    {
        object val;
        bool b = getData("BUTTON", out val);
        GameObject obj = val as GameObject;
        UIToggle toggle = obj.GetComponent<UIToggle>();
        toggle.value = !toggle.value;

        MaterialFragmentLogicData logicData = DataCenter.GetData("MATERIAL_FRAGMENT_DATA") as MaterialFragmentLogicData;
        int iGridIndex = (int)get("GRID_INDEX");
        MaterialFragmentData data = logicData.GetMaterialFragmentDataFromGridIndex(iGridIndex);

        if (data != null)
        {
            DataCenter.SetData("MATERIAL_FRAGMENT_PACKAGE_WINDOW", "REFRESH", iGridIndex);
        }

        return true;
    }
}

public class Button_MaterialFragmentPackageUseButton : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("MATERIAL_FRAGMENT_PACKAGE_WINDOW", "USE", true);

        return true;
    }
}

public class Button_MaterialFragmentPackageControlBtn : CEvent
{
    public override bool _DoEvent()
    {
        if (CommonParam.bIsNetworkGame)
        {
            CS_RequestMaterialFragment quest = Net.StartEvent("CS_RequestMaterialFragment") as CS_RequestMaterialFragment;
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
        }
        else
        {
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
            MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "ROLE_EQUIP_CULTIVATE_WINDOW";
        }
        return true;
    }
}