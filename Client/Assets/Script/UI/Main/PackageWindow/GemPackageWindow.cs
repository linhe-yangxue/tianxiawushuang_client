using UnityEngine;
using System.Collections;
using Logic;

public class GemPackageWindow : ItemPackageBaseWindow {
	// gem
	public GemLogicData mGemLogicData;
	public SORT_TYPE mSortType;
	
	GemData mCurSelGemData = null;
	
	public override void Init ()
	{
		base.Init ();
		
		EventCenter.Self.RegisterEvent("Button_package_gem_icon_btn", new DefineFactory<Button_PackageGemIconBtn>());
		EventCenter.Self.RegisterEvent("Button_gem_package_sale_button", new DefineFactory<Button_GemPackageSaleButton>());
		EventCenter.Self.RegisterEvent("Button_gem_package_control_button", new DefineFactory<Button_GemPackageControlButton>());
		
	}
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "SALE")
		{
			SaleOK();
		}
		else if(keyIndex == "SALE_RESULT")
		{
			SaleResult();
		}
	}
	
	public override void OnOpen ()
	{
		base.OnOpen ();
	}
	
	public override void InitVariable()
	{
		mGemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
		if(mGemLogicData != null)
		{
			mSortType = SORT_TYPE.STAR_LEVEL;
			InitPackageIcons();
			RefreshBagNum();
		}
		
		Refresh(0);
	}
	
	public override bool Refresh (object param)
	{
		base.Refresh (param);
		
		mCurSelGemData = mGemLogicData.mGemList[mCurSelGridIndex];
		
		RefreshInfoWindow();
		
		return true;
	}
	
	
	public override void  InitPackageIcons()
	{
		mGemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
		mGrid.MaxCount = mGemLogicData.mGemList.Length;
		
		for(int i = 0; i < mGemLogicData.mGemList.Length; i++)
		{
			GameObject tempObj = mGrid.controlList[i];
			GameObject obj = tempObj.transform.Find("package_gem_icon_btn").gameObject;
			UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
			if (buttonEvent != null)                                          
				buttonEvent.mData.set("GRID_INDEX", i);

			SetGemIcon(i);
		}
	}

	public bool SetGemIcon(int iIndex)
	{
		if(iIndex < 0 || iIndex >= mGemLogicData.mGemList.Length)
			return false;
		
		UISprite sprite = GameCommon.FindObject(mGrid.controlList[iIndex], "Background").GetComponent<UISprite>();
		if(sprite != null)
		{
			string strAtlasName = TableCommon.GetStringFromStoneTypeIconConfig(iIndex + 1000, "STONE_ATLAS_NAME");
			string strSpriteName = TableCommon.GetStringFromStoneTypeIconConfig(iIndex + 1000, "STONE_SPRITE_NAME");
			
			UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
			sprite.atlas = tu;
			sprite.spriteName = strSpriteName;
			sprite.MakePixelPerfect();
		}
		
		SetStoneNum(iIndex);

		GameCommon.ToggleFalse(mGrid.controlList[iIndex].transform.Find("package_gem_icon_btn").gameObject);
		
		return true;
	}
	
	public void SetStoneNum(int iIndex)
	{
		if(iIndex >= 0 && iIndex < mGemLogicData.mGemList.Length)
		{
			GameObject stoneNumLabelObj = GameCommon.FindObject(mGrid.controlList[iIndex], "StoneNumLabel");			
			GameObject StoneIconCoverBtnObj = GameCommon.FindObject(mGrid.controlList[iIndex], "StoneIconCoverBtn");
			GameObject StoneIconCoverNumLabelObj = GameCommon.FindObject(StoneIconCoverBtnObj, "Label");
			
			UILabel label = stoneNumLabelObj.GetComponent<UILabel>();
			
			int iCount = mGemLogicData.mGemList[iIndex].mCount;
			if(iCount > 0)
			{
				label.text = iCount.ToString();
				stoneNumLabelObj.SetActive(true);
				StoneIconCoverBtnObj.SetActive(false);
			}
			else
			{
				label.text = "0";
				stoneNumLabelObj.SetActive(false);
				StoneIconCoverBtnObj.SetActive(true);
				StoneIconCoverNumLabelObj.SetActive(true);
			}
		}
	}
	
	public override void RefreshInfoWindow()
	{
		GameObject group = mGameObjUI.transform.Find("info_window/group").gameObject;
		GameObject emptyGroup = mGameObjUI.transform.Find("info_window/empty_bg").gameObject;
		if (mCurSelGemData == null)
		{
			group.SetActive(false);
			emptyGroup.SetActive(true);
		}
		else
		{
			group.SetActive(true);
			emptyGroup.SetActive(false);

			// set sprite
			UISprite sprite = GameCommon.FindObject(group, "icon_sprite").GetComponent<UISprite>();
			GameCommon.SetGemIcon(sprite, mCurSelGridIndex + 1000);

			// set name
			UILabel name = GameCommon.FindObject(group, "props_name").GetComponent<UILabel>();
			name.text = TableCommon.GetStringFromStoneTypeIconConfig(mCurSelGridIndex + 1000, "NAME");
			
			// set description
			UILabel descrition = GameCommon.FindObject(group, "introduce_label").GetComponent<UILabel>();
			descrition.text = TableCommon.GetStringFromStoneTypeIconConfig(mCurSelGridIndex + 1000, "DESCRIPTION");
		}
	}

	public void SaleOK()
	{
//		if(mCurSelGemData == null)
//			return;
//		
//		if(CommonParam.bIsNetworkGame)
//		{
//			CS_RequestGemSale quest = Net.StartEvent("CS_RequestGemSale") as CS_RequestGemSale;
//			quest.set("DBID", mCurSelGemData.mDBID);
//			quest.mAction = () => DataCenter.SetData("GEM_PACKAGE_WINDOW", "SALE_RESULT", true);
//			quest.DoEvent();
//		}
//		else
//		{
//			SaleResult();
//		}
	}
	
	public void SaleResult()
	{
		// set role equip data		
//		GemLogicData roleGemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
//		if(roleGemLogicData != null)
//		{
//			roleGemLogicData.RemoveGem(mCurSelGemData);
//		}
//		
//		// refresh bag role equip icons
//		DataCenter.OpenWindow("GEM_PACKAGE_WINDOW");
//		
//		// gain gold
//		int iBaseGainGoldNum = TableCommon.GetNumberFromGemEvolutionConfig(mCurSelGemData.mStarLevel, "PRICE");
//		int iDGainGoldNum = TableCommon.GetNumberFromGemEvolutionConfig(mCurSelGemData.mStarLevel, "PRICE_ADD");
//		
//		int iGainGlod = iBaseGainGoldNum + mCurSelGemData.mStrengthenLevel * iDGainGoldNum;
//		GameCommon.RoleChangeGold(iGainGlod);
	}
}


public class Button_PackageGemStarBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("GEM_PACKAGE_WINDOW", "SORT_GEM_ICONS", SORT_TYPE.STAR_LEVEL);
		return true;
	}
}

public class Button_PackageGemStrengthenLevelBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("GEM_PACKAGE_WINDOW", "SORT_GEM_ICONS", SORT_TYPE.STRENGTHEN_LEVEL);
		return true;
	}
}

public class Button_PackageGemAttributeBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("GEM_PACKAGE_WINDOW", "SORT_GEM_ICONS", SORT_TYPE.ELEMENT_INDEX);
		return true;
	}
}

public class Button_PackageGemIconBtn : CEvent
{
	public override bool _DoEvent()
	{
		object val;		
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		toggle.value = !toggle.value;
		
		GemLogicData logicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
		int iGridIndex = (int)get("GRID_INDEX");
		DataCenter.SetData("GEM_PACKAGE_WINDOW", "REFRESH", iGridIndex);
		
		return true;
	}
}

public class Button_GemPackageSaleButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("GEM_PACKAGE_WINDOW", "SALE", true);
		
		return true;
	}
}

public class Button_GemPackageControlButton : CEvent
{
	public override bool _DoEvent()
	{
//		if (CommonParam.bIsNetworkGame)
//		{
//			CS_RequestGem quest = Net.StartEvent("CS_RequestGem") as CS_RequestGem;
//			quest.set ("WINDOW_NAME", "GEM_CULTIVATE_WINDOW");
//			quest.mAction = () => {
//				DataCenter.CloseWindow("PACKAGE_WINDOW");
//				MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
//			};
//			quest.mBackAction = () => {
//				EventCenter.Start("Button_package_btn").DoEvent();
//			};
//			quest.DoEvent();
//			
//			tEvent gemQuest = Net.StartEvent("CS_RequestGem");
//			gemQuest.DoEvent();
//		}
//		else
//		{
//			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
//			MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "GEM_CULTIVATE_WINDOW";
//		}
		return true;
	}
}
