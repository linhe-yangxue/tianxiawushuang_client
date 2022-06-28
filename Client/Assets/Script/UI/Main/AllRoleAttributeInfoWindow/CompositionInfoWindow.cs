using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Linq;
using System.Collections.Generic;

public class CompositionInfoWindow : tWindow {

    private GameObject[] mCompositionInfoGroups = new GameObject[5];
    private GameObject mSiftingBox;
    private GameObject mSiftingButton;
    private CompositionInfoPageType mCurPageType = CompositionInfoPageType.EQUIP_ARM;
	public SORT_TYPE mSortType = SORT_TYPE.STAR_LEVEL;
    
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_sifting_button_0", new DefineFactory<Button_SiftingButtonItemBtn>());
        EventCenter.Self.RegisterEvent("Button_sifting_button_1", new DefineFactory<Button_SiftingButtonItemBtn>());
        EventCenter.Self.RegisterEvent("Button_sifting_button_2", new DefineFactory<Button_SiftingButtonItemBtn>());
        EventCenter.Self.RegisterEvent("Button_sifting_button_3", new DefineFactory<Button_SiftingButtonItemBtn>());
        EventCenter.Self.RegisterEvent("Button_sifting_button_4", new DefineFactory<Button_SiftingButtonItemBtn>());

        EventCenter.Self.RegisterEvent("Button_sifting_button", new DefineFactory<Button_SiftingButtonBtn>());

        EventCenter.Self.RegisterEvent("Button_item_icon_check_btn", new DefineFactory<Button_ItemIconCheckBtn>());
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "SIFTING_ITEM_BTN_VISIBLE":
                SetSiftingItemBtnVisible();
                break;
            case "SHOW_SIFTING_ITEM_BTN":
                ShowSiftingItemBtn();
                break;
            case "HIDE_SIFTING_ITEM_BTN":
                HideSiftingItemBtn();
                break;
			case "ORDER_RULE":
				InitInfoIcons();
				break;
		}
	}
	
	public override void OnOpen()
    {
        base.OnOpen();

        mSiftingBox = GameCommon.FindObject(mGameObjUI, "sifting_box");
        mSiftingButton = GameCommon.FindObject(mGameObjUI, "sifting_button");
		DataCenter.Set ("DESCENDING_ORDER", true);
		if(GetSub ("order_button") != null && GetSub ("order_button").GetComponentInChildren<UISprite>() != null)
		{
			GetSub ("order_button").GetComponentInChildren<UISprite>().spriteName = "ui_j";
			GameCommon.GetButtonData (GetSub ("order_button")).set ("WINDOW_NAME", mWinName);
		}
        InitInfoIcons();
        
        Refresh((int)CompositionInfoPageType.EQUIP_ARM);
    }

    private void InitInfoIcons()
    {
        for (int i = 0; i < mCompositionInfoGroups.Length; i++ )
        {
            GameObject obj = GameCommon.FindObject(mGameObjUI, "info_group_" + i.ToString());
            mCompositionInfoGroups[i] = obj;

            InitIcons(i);
        }
    }

    public override bool Refresh(object param)
    {
        base.Refresh(param);

        mCurPageType = (CompositionInfoPageType)((int)param);

        DataCenter.SetData("ROLE_EQUIP_COMPOSITION_WINDOW", "SET_CUR_SEL_ITEM_FROM_INFO_WINDOW", 0);
        HideSiftingItemBtn();
        RefreshSiftingTitle();
        ShowWindow();

        UIGridContainer grid = GameCommon.FindObject(mCompositionInfoGroups[(int)mCurPageType], "grid").GetComponent<UIGridContainer>();
        if (grid != null)
        {
            GameCommon.ToggleTrue(GameCommon.FindObject(grid.controlList[0], "item_icon_check_btn"));
        }

        return true;
    }

    private void ShowWindow()
    {
        for (int i = 0; i < mCompositionInfoGroups.Length; i++)
        {
            mCompositionInfoGroups[i].SetActive(i == (int)mCurPageType);
        }
    }

    private void SetSiftingItemBtnVisible()
    {
        if (mSiftingBox != null)
        {
            mSiftingBox.SetActive(!mSiftingBox.activeSelf);
        }
    }

    private void ShowSiftingItemBtn()
    {
        if (mSiftingBox != null)
        {
            mSiftingBox.SetActive(true);
        }
    }

    private void HideSiftingItemBtn()
    {
        if (mSiftingBox != null)
        {
            mSiftingBox.SetActive(false);
        }
    }

    private void RefreshSiftingTitle()
    {
        if (mSiftingButton != null)
        {
            UILabel siftingButtonLabel = mSiftingButton.transform.Find("label").GetComponent<UILabel>();
            UILabel siftingButtonItemLabel = mSiftingBox.transform.Find("sifting_button_" + ((int)mCurPageType).ToString()).Find("label").GetComponent<UILabel>();

            siftingButtonLabel.text = siftingButtonItemLabel.text;
        }
    }

    private void InitIcons(int iPageType)
    {
        switch ((CompositionInfoPageType)iPageType)
        {
            case CompositionInfoPageType.EQUIP_ARM:
            case CompositionInfoPageType.EQUIP_DEFENCE:
            case CompositionInfoPageType.EQUIP_ORNAMENT:
            case CompositionInfoPageType.EQUIP_ELEMENT:
                InitRoleEquipIcons(iPageType);
                break;
            case CompositionInfoPageType.MATERIAL:
                InitMaterialIcons();
                break;
        }
    }

    //------------------------------------------------------------------------------------------
    // role equip
    //------------------------------------------------------------------------------------------
    private void InitRoleEquipIcons(int iType)
    {
        UIGridContainer grid = GameCommon.FindObject(mCompositionInfoGroups[iType], "grid").GetComponent<UIGridContainer>();
        if (grid != null)
        {
            grid.MaxCount = GetRoleEquipNumForType(iType);
            int i = 0;

			List<DataRecord> dataRecordList = DataCenter.mRoleEquipConfig.GetAllRecord().Values.ToList();
			dataRecordList = SortList (dataRecordList);

			foreach (DataRecord data in dataRecordList )
            {
				if (data["INDEX"] !=0 && data["ROLEEQUIP_TYPE"] == iType)
                {
					InitItemIcons(ITEM_TYPE.EQUIP, i++, grid, data);
                }
            }
        }
    }    

	public List<DataRecord> SortList(List<DataRecord> list)
	{
		if(list != null && list.Count > 0)
		{
			switch(mSortType)
			{
			case SORT_TYPE.STAR_LEVEL:
				list = GameCommon.SortList (list, GameCommon.SortEquipsDataByStarLevel);
	
				break;
			}
		}
		return list;
	}

    public int GetRoleEquipNumForType(int iType)
    {
        int i = 0;
        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mRoleEquipConfig.GetAllRecord())
        {
            if (pair.Key != 0 && pair.Value["ROLEEQUIP_TYPE"] == iType)
            {
                i++;
            }
        }
        return i;
    }

    public void InitItemIcons(ITEM_TYPE itemType, int iGridIndex, UIGridContainer grid, DataRecord record)
    {
        if (grid != null && record != null)
        {
            GameObject itemObj = grid.controlList[iGridIndex];
            GameObject obj = itemObj.transform.Find("item_icon_check_btn").gameObject;
            UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
            if (buttonEvent != null)
            {
                buttonEvent.mData.set("GRID_INDEX", iGridIndex);
                buttonEvent.mData.set("ITEM_INDEX", (int)record["INDEX"]);
                buttonEvent.mData.set("ITEM_TYPE", (int)itemType);
            }

            //GameCommon.FindObject(obj, "star_bg").SetActive(itemType == ITEM_TYPE.ROLE_EQUIP);
            //GameCommon.FindObject(obj, "star_level_label").SetActive(itemType == ITEM_TYPE.ROLE_EQUIP);

            GameObject stargrid = GameCommon.FindObject(obj, "stars_grid");

            if (stargrid != null)
                stargrid.SetActive(itemType == ITEM_TYPE.EQUIP);

            if (itemType == ITEM_TYPE.EQUIP)
            {
                SetEquipIcon(obj, record);
            }
            else if (itemType == ITEM_TYPE.MATERIAL)
            {
                SetMaterialIcon(obj, record);
            }
        }
    }

    public bool SetEquipIcon(GameObject obj, DataRecord record)
    {
        if (obj == null || record == null)
            return false;

        GameObject background = GameCommon.FindObject(obj, "icon_sprite");
        GameObject emptyBG = GameCommon.FindObject(obj, "empty_bg");

        if (record == null)
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
//            emptyBG.SetActive(false);

            // set equip icon
            GameCommon.SetEquipIcon(obj, record["INDEX"]);

            // set element icon and set equip element background icon
            GameCommon.SetIcon(obj, "background_sprite", UICommonDefine.strEquipIconBackgroundSprite, UICommonDefine.strEquipIconBackgroundAtlas);

            // set star level
            GameCommon.SetStarLevelLabel(obj, record["STAR_LEVEL"]);
        }

        return true;
    }

    //------------------------------------------------------------------------------------------
    // material
	//------------------------------------------------------------------------------------------
    private void InitMaterialIcons()
    {
        int iType = (int)CompositionInfoPageType.MATERIAL;
        UIGridContainer grid = GameCommon.FindObject(mCompositionInfoGroups[iType], "grid").GetComponent<UIGridContainer>();
        if (grid != null)
        {
            grid.MaxCount = DataCenter.mMaterialConfig.GetAllRecord().Count - 1;
            int i = 0;
			List<DataRecord> materialList = DataCenter.mMaterialConfig.GetAllRecord ().Values.ToList ();
			materialList = SortMaterialList (materialList );

			foreach (DataRecord data in materialList )
			{
				if (data["INDEX"] != 0 )
				{
					InitItemIcons(ITEM_TYPE.MATERIAL, i++, grid, data);
				}
			}
        }
    }
	public List<DataRecord> SortMaterialList(List<DataRecord> list)
	{
		if(list != null && list.Count > 0)
		{
			switch(mSortType)
			{
			case SORT_TYPE.STAR_LEVEL:
				list = GameCommon.SortList (list, GameCommon.SortMaterialDataByStarLevel);
				
				break;
			}
		}
		return list;
	}

    public bool SetMaterialIcon(GameObject obj, DataRecord record)
    {
        if (obj == null || record == null)
            return false;

        GameObject background = GameCommon.FindObject(obj, "icon_sprite");
        GameObject emptyBG = GameCommon.FindObject(obj, "empty_bg");

        if (record == null)
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
//            emptyBG.SetActive(false);

            // set icon
            SetMaterialIcon(obj, record["INDEX"]);

            GameCommon.SetIcon(obj, "background_sprite", UICommonDefine.strEquipIconBackgroundSprite, UICommonDefine.strEquipIconBackgroundAtlas);
        }

        return true;
    }

    private void SetMaterialIcon(GameObject obj, int iIndex)
    {
        string strAtlasName = TableCommon.GetStringFromMaterialConfig(iIndex, "ICON_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromMaterialConfig(iIndex, "ICON_SPRITE_NAME");
        GameCommon.SetIcon(obj, "icon_sprite", strSpriteName, strAtlasName);
    }

    public bool GetInfo(int iCurGridIndex, ref int iCurSelItemIndex, ref int iCurSelItemType)
    {
        UIGridContainer grid = GameCommon.FindObject(mCompositionInfoGroups[(int)mCurPageType], "grid").GetComponent<UIGridContainer>();
        if (grid != null)
        {
            GameObject obj = grid.controlList[iCurGridIndex];
            UIButtonEvent btnEvent = GameCommon.FindObject(obj, "item_icon_check_btn").GetComponent<UIButtonEvent>();
            iCurSelItemIndex = btnEvent.mData.get("ITEM_INDEX");
            iCurSelItemType = btnEvent.mData.get("ITEM_TYPE");
            return true;
        }
        return false;
    }
}


public class Button_SiftingButtonBtn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("COMPOSITION_INFO_WINDOW", "SIFTING_ITEM_BTN_VISIBLE", null);
        return true;
    }
}

public class Button_SiftingButtonItemBtn : CEvent
{
    public override bool _DoEvent()
    {
        string[] names = GetEventName().Split('_');

        DataCenter.SetData("COMPOSITION_INFO_WINDOW", "REFRESH", int.Parse(names[names.Length - 1]));
        return true;
    }
}

public class Button_ItemIconCheckBtn : CEvent
{
    public override bool _DoEvent()
    {
        int iGridIndex = get("GRID_INDEX");
        DataCenter.SetData("ROLE_EQUIP_COMPOSITION_WINDOW", "SET_CUR_SEL_ITEM_FROM_INFO_WINDOW", iGridIndex);
        DataCenter.SetData("COMPOSITION_INFO_WINDOW", "CUR_GRID_INDEX", iGridIndex);
        return true;
    }
}