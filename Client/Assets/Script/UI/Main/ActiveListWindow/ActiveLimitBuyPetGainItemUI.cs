using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;

public class ActiveLimitBuyPetGainItemUI : MonoBehaviour {

	void Start () {
		Init();
	}
	
	public virtual void Init()
	{
		ActiveGainItemWindow itemInfoWindow = new ActiveGainItemWindow();
		DataCenter.Self.registerData("ACTIVE_GAIN_ITEM_WINDOW", itemInfoWindow);
		
	}
	
	public void OnDestroy()
	{		
		DataCenter.Remove("ACTIVE_GAIN_ITEM_WINDOW");
	}
}


public class ActiveGainItemWindow : tWindow
{
	public PetData mPetData = null;
	
	public string mStrWindowName = "active_gain_item_window";
	
	GameObject mItemInfoObj;
	UIGridContainer  mGrid;
	
	public override void Init ()
	{
		mGameObjUI = GameCommon.FindUI(mStrWindowName);
		
		mItemInfoObj = GameCommon.FindObject (mGameObjUI, "single_item_info");
		mGrid = GameCommon.FindObject (mGameObjUI, "grid").GetComponent<UIGridContainer>();
		
		Close();
	}
	
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "SET_SELECT_ITEM_BY_MODEL_INDEX")
		{
			mItemInfoObj.SetActive (true);
			mGrid.MaxCount = 0;
			SetItem((DataRecord)objVal, mItemInfoObj);
		}
		if(keyIndex == "SET_SELECT_ITEMS_BY_MODEL_INDEX")
		{
			mItemInfoObj.SetActive (false);
			SetItemsByModelIndex((NiceTable)objVal);
		}
	}
	
	public void SetItemsByModelIndex(NiceTable itemData)
	{
		int num = 0;
		mGrid.MaxCount = itemData.GetAllRecord ().Count;
		foreach (KeyValuePair<int, DataRecord> r in itemData.GetAllRecord())
		{
			DataRecord re = r.Value;
			SetItem (re, mGrid.controlList[num]);
			num++;
		}
	}
	
	public void SetItem(DataRecord dataRecord, GameObject obj)
	{
		int iItemType = dataRecord.getData("ITEM_TYPE");
		int iItemID = dataRecord.getData("ITEM_ID");
		int iItemCount = dataRecord.getData("ITEM_COUNT");
		int iRoleEquipElement = dataRecord.getData ("ELEMENT");
		
		GameCommon.SetUIVisiable (obj, "num", true);
		if(iItemCount > 1) GameCommon.SetUIText (obj, "num", "x" + iItemCount.ToString ());
		else GameCommon.SetUIText (obj, "num", "");
		UISprite itemSprite = GameCommon.FindObject (obj , "item_icon").GetComponent<UISprite>();
		GameCommon.SetUIVisiable (obj, "role_equip_element", false);
		GameCommon.SetUIVisiable (obj, "star_level_label", false);
		GameCommon.SetUIVisiable (obj, "fragment_sprite", false);
		GameCommon.SetStarLevelLabel(obj, 0);
		
		GameObject effectFiveObj = GameCommon.FindObject (obj, "effect_five");
		if(effectFiveObj != null) effectFiveObj.SetActive (false);
		GameObject elementObj = GameCommon.FindObject (obj, "element");
		if(effectFiveObj != null) elementObj.SetActive (false);

		switch(iItemType)
		{
		case (int)ITEM_TYPE.PET:
			GameCommon.SetPetIconWithElementAndStar (obj, "item_icon", "element", "star_level_label", iItemID);
			int iStarLevel = TableCommon.GetNumberFromActiveCongfig (iItemID, "STAR_LEVEL");
			if(iStarLevel == 5 && effectFiveObj != null)
			{
				effectFiveObj.SetActive (true);
			}
			break;
		case (int)ITEM_TYPE.PET_FRAGMENT:
			int iFragment = TableCommon.GetNumberFromFragment(iItemID,"ITEM_ID");
			GameCommon.SetPetIconWithElementAndStar (obj, "item_icon", "element", "star_level_label", iFragment);
			int iFragmentStarLevel = TableCommon.GetNumberFromActiveCongfig (iFragment, "STAR_LEVEL");
			if(iFragmentStarLevel == 5 && effectFiveObj != null)
			{
				effectFiveObj.SetActive (true);
			}
			
			GameCommon.SetUIVisiable (obj, "fragment_sprite", iItemType == (int)ITEM_TYPE.PET_FRAGMENT ? true : false);
			
			int elementIndex = TableCommon.GetNumberFromActiveCongfig(iFragment, "ELEMENT_INDEX");
			GameCommon.SetElementFragmentIcon (obj, "fragment_sprite", elementIndex);
			break;
		case (int)ITEM_TYPE.EQUIP:
			GameCommon.SetUIVisiable (obj, "num", false);
			GameCommon.SetUIVisiable (obj, "role_equip_element", true);
			GameCommon.SetEquipElementBgIcons(obj, "element", "role_equip_element", iItemID, iRoleEquipElement);
			GameCommon.SetEquipIcon (itemSprite, iItemID);
			iItemCount = TableCommon.GetNumberFromRoleEquipConfig (iItemID, "STAR_LEVEL");
			GameCommon.SetStarLevelLabel(obj, iItemCount, "star_level_label");
			break;
		default :
			GameCommon.SetItemIcon (itemSprite, iItemType, iItemID);
			break;
		}
	}
}
