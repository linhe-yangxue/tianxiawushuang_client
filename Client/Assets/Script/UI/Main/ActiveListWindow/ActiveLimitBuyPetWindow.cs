using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Logic;
using DataTable;
using Utilities;


public class ActiveLimitBuyPetWindow : ActiveTotalWindow
{
	int mCountDown = 0;
	int mTotalCount = 10;

	int mPetStar = 0;

	public override void Init ()
	{
		EventCenter.Register ("Button_go_mysterious_shop_button", new DefineFactory<Button_go_mysterious_shop_button>());
		EventCenter.Register ("Button_lucy_pet_one_button", new DefineFactory<Button_lucy_pet_button>());
		EventCenter.Register ("Button_lucy_pet_ten_button", new DefineFactory<Button_lucy_pet_button>());

		Net.gNetEventCenter.RegisterEvent ("CS_GetBuyPetActivityInfo", new DefineFactoryLog<CS_GetBuyPetActivityInfo>());
		Net.gNetEventCenter.RegisterEvent ("CS_BuyPetInActivity", new DefineFactoryLog<CS_BuyPetInActivity>());

		EventCenter.Register ("Button_extract_again", new DefineFactory<Button_extract_again>());
		EventCenter.Register ("Button_active_gain_item_window_close_btn", new DefineFactory<Button_active_gain_item_window_close_btn>());
		EventCenter.Register ("Button_active_gain_pet_window_close_btn", new DefineFactory<Button_active_gain_pet_window_close_btn>());
		EventCenter.Register ("Button_active_gain_pet_window_details_btn", new DefineFactory<Button_active_gain_pet_window_details_btn>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		mDesLabelName = "active_house_pet_tips_label";
		mCountdownLabelName = "active_house_pet_gift_rest_time";

		if(GameObject.Find ("create_scene") == null)
		{
			this.StartCoroutine (LoadChouKaScene ());
		}
	}

	public override void OnClose ()
	{
		GameObject obj = GameObject.Find ("create_scene");
		if(obj != null)
			GameObject.Destroy (obj);

		base.OnClose ();
	}

	public IEnumerator LoadChouKaScene()
	{
		yield return new WaitForSeconds(0.3f);
		GlobalModule.Instance.LoadScene("chouka", true);
	}

	public override void OnOpen ()
	{
		base.OnOpen ();
		tEvent evt = Net.StartEvent ("CS_GetBuyPetActivityInfo");
		evt.set ("ACTIVITY_INDEX", mConfigIndex);
		evt.set ("IS_FREE", isFree ());
		evt.set ("IS_DISCOUNT", false);
		evt.DoEvent ();
	}
	
	public override bool Refresh (object param)
	{
		SetVisible ("jump_animator_button", false);

		tEvent respEvt = param as tEvent;
		mCountDown = respEvt.get ("COUNT_DOWN");
		mTotalCount = respEvt.get ("TOTAL_TIMES");
		if(mCountDown < 1) mCountDown = 1;
		if(mTotalCount > 10) mTotalCount = 10;

		int shopSlotIndex = DataCenter.mOperateEventConfig.GetData (mConfigIndex, "SHOPID_1");
		int costType = TableCommon.GetNumberFromShopSlotBase (shopSlotIndex, "COST_ITEM_TYPE");
		int costCount = TableCommon.GetNumberFromShopSlotBase (shopSlotIndex, "COST_ITEM_COUNT");

		string atlasName = DataCenter.mItemIcon.GetData(costType, "MYSTERIOUS_SHOP_ATLAS");
		string spriteName = DataCenter.mItemIcon.GetData(costType, "MYSTERIOUS_SHOP_SPRITE");
		GameCommon.SetIcon (GetSub ("lucy_pet_ten_infos"), "honor_icon", spriteName, atlasName);
		GameCommon.SetIcon (GetSub ("lucy_pet_one_infos"), "honor_icon", spriteName, atlasName);

		GameCommon.SetUIVisiable (GetSub ("lucy_pet_ten_infos"), "this_time_free", false);
		SetText ("lucy_pet_one_noney", costCount.ToString());
		SetText ("lucy_pet_ten_noney", (10 * costCount).ToString ());
		SetButtonData ("lucy_pet_one_button", costType, costCount, 1);
		SetButtonData ("lucy_pet_ten_button", costType, 10 *costCount, 10);

		RefreshChangeComponent();
		return true;
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
		case "SEND_MESSAGE":
			SendMessage ((int)objVal);
			break;
		case"SEND_MESSAGE_RESULT":
			SendMessageResult (objVal as tEvent);
			break;
		case"FU_ANIMATOR_END":
			FuAnimatorEnd ((tEvent)objVal);
			break;
		case"UI_HIDDEN":
			UIHidden ((bool)objVal);
			ShowMainScene (true);
			break;
		case "SHOW_GAIN_PET_WINDOW_CARD":
			SetVisible ("gain_pet_window_card", (bool)objVal);
			break;
		}
	}

	void SetButtonData(string buttonName, int costType, int costCount, int times)
	{
		GameCommon.GetButtonData (GetSub (buttonName)).set ("COST_TYPE", costType);
		GameCommon.GetButtonData (GetSub (buttonName)).set ("COST_COUNT", costCount);
		GameCommon.GetButtonData (GetSub (buttonName)).set ("TIMES", times);
	}

	void RefreshChangeComponent()
	{
		SetText ("my_honor_point_number", RoleLogicData.Self.mHonorPoint.ToString ());
		SetText ("tips_num", mCountDown.ToString ());

		GameCommon.SetUIVisiable (GetSub ("lucy_pet_one_infos"), "this_time_free", isFree ());
		GameCommon.SetUIVisiable (GetSub ("lucy_pet_one_infos"), "lucy_pet_one_noney", !isFree ());
		SetVisible ("lucy_pet_ten_infos", !isFree ());
	}
	
	bool isFree()
	{
		int shopSlotIndex = DataCenter.mOperateEventConfig.GetData (mConfigIndex, "SHOPID_1");
		bool isFree = Convert.ToBoolean(TableCommon.GetNumberFromShopSlotBase (shopSlotIndex, "IS_FREE"));

		if(isFree)
		{
			ShopLogicData shopLogicData = DataCenter.GetData ("SHOP_DATA") as ShopLogicData;
			if(shopLogicData != null)
				isFree = shopLogicData.IsFreeByIndex (shopSlotIndex);
			else 
				DEBUG.LogError ("ShopLogicData is null");
		}

		return isFree;
	}

	void SetFreeShopData()
	{
		int shopSlotIndex = DataCenter.mOperateEventConfig.GetData (mConfigIndex, "SHOPID_1");
		ShopLogicData shopLogicData = DataCenter.GetData ("SHOP_DATA") as ShopLogicData;
		shopLogicData.SetShopDataByIndex (shopSlotIndex, 1, CommonParam.NowServerTime ());
	}

	void SendMessage(int count)
	{
		tEvent evt = Net.StartEvent ("CS_BuyPetInActivity");
		evt.set ("ACTIVITY_INDEX", mConfigIndex);
		evt.set ("DRAW_TIMES", count);
		evt.DoEvent ();
	}

	void SendMessageResult(tEvent respEvt)
	{
		object resultData;
		if (!respEvt.getData("RESULT_TABLE", out resultData)) return;
		NiceTable itemData = resultData as NiceTable;

		int times = itemData.GetRecordCount ();

		GainHonorPoint(times);
		ChangeCountDown(times);
		CostCount (times);
		SetFreeShopData ();
		RefreshChangeComponent ();

		ShowGainItemsUI(respEvt);
	}

	void GainHonorPoint(int times)
	{
		int count = DataCenter.mOperateEventConfig.GetData (mConfigIndex, "BONUS");
		GameCommon.RoleChangeNumericalAboutRole ((int)ITEM_TYPE.HONOR_POINT, count * times);
	}

	void CostCount(int times)
	{
		int shopSlotIndex = DataCenter.mOperateEventConfig.GetData (mConfigIndex, "SHOPID_1");
		int costType = TableCommon.GetNumberFromShopSlotBase (shopSlotIndex, "COST_ITEM_TYPE");
		int costCount = TableCommon.GetNumberFromShopSlotBase (shopSlotIndex, "COST_ITEM_COUNT");

		GameCommon.RoleChangeNumericalAboutRole (costType, -times * costCount);
	}
	
	void ChangeCountDown(int times)
	{
		mCountDown -= times;

		if(mCountDown <= 0) mCountDown += mTotalCount;
	}

	void UIHidden(bool bVisible)
	{
		SetVisible ("parents", bVisible);
		SetVisible ("black_background", !bVisible);
	}

	void ShowMainScene(bool bVisible)
	{
		GameObject obj = GameObject.Find ("Mainmenu_bg");
		if(obj != null)
		{
			RenderSettings.fogColor = bVisible ? new Color(65f/255f, 251f/255f, 213f/255f) : new Color(76f/255f, 141f/255f, 195f/255f);
			
			foreach(Transform t in obj.transform)
				t.gameObject.SetActive (bVisible);
		}
	}

	public virtual void ShowGainItemsUI(tEvent respEvt)
	{
		object resultData;
		if (!respEvt.getData("RESULT_TABLE", out resultData))
		{
			return;
		}
		NiceTable itemData = resultData as NiceTable;
		
		bool bIsPet = false;
		foreach (KeyValuePair<int, DataRecord> r in itemData.GetAllRecord())
		{
			if(r.Value.getData ("ITEM_TYPE") == (int)ITEM_TYPE.PET)
				bIsPet = true;
		}
		
		DataCenter.SetData ("ACTIVE_LIST_WINDOW", "UI_HIDDEN", false);
		UIHidden (false);
		ShowMainScene (!bIsPet);
		SetVisible ("black_background", !bIsPet);
		if(itemData.GetAllRecord().Count == 1)
		{
			ShowGainSingleItemUI(respEvt);
			if(bIsPet)
			{
				ShowGainItemAnimator(() => RoleAnimatorEnd (respEvt), respEvt, 90012, 1.5f);
				ShowShopGainPetWindow (false);
			}
		}
		else
		{
			if(bIsPet)
				ShowGainItemAnimator(() => RoleAnimatorEnd (respEvt), respEvt, 90012, 1.5f);
			else
				ShowGainGroupAllItemsUI(respEvt);
		}
	}

	public virtual void ShowGainSingleItemUI(tEvent respEvt)
	{
		object resultData;
		if (!respEvt.getData("RESULT_TABLE", out resultData))
		{
			return;
		}
		NiceTable itemData = resultData as NiceTable;
		
		foreach (KeyValuePair<int, DataRecord> r in itemData.GetAllRecord())
		{
			DataRecord re = r.Value;
			int iItemType = re.getData("ITEM_TYPE");
			int iItemID = re.getData("ITEM_ID");
			if (iItemType == (int)ITEM_TYPE.PET)
			{
//				SetShopPetBuyAgainButton(respEvt);
				SetAgainButtonData (1, GetSub ("active_gain_pet_window"));

				SetPetStar (iItemID);
				DataCenter.SetData("ACTIVE_GAIN_PET_WINDOW", "OPEN", true);
				DataCenter.SetData("ACTIVE_GAIN_PET_WINDOW", "SET_SELECT_PET_BY_MODEL_INDEX", iItemID);
				
				return;
			}
			
//			SetShopItemBuyAgainButton(respEvt);
			SetAgainButtonData (1, GetSub ("active_gain_item_window"));
			
			DataCenter.SetData("ACTIVE_GAIN_ITEM_WINDOW", "SET_SELECT_ITEM_BY_MODEL_INDEX", re);
			DataCenter.SetData("ACTIVE_GAIN_ITEM_WINDOW", "OPEN", true);
		}
	}

	void SetPetStar(int petIndex)
	{
		mPetStar = TableCommon.GetNumberFromActiveCongfig (petIndex, "STAR_LEVEL");
	}

	public void ShowGainItemAnimator(Action callBack, tEvent respEvt, int modelIndex, float fScale)
	{
		bool bIsRole = modelIndex == 90012 ? true : false;
		GameObject obj = GameObject.Find ("create_scene");
		if(obj == null)
			return;
		
		float fAniSpeed = 1.0f;
		fAniSpeed = bIsRole ? 1.0f : 1.5f;
		string strPoint = "";
		strPoint = bIsRole ? "role_point" : "fu_point";
		
		GameCommon.SetUIVisiable (obj, "item", !bIsRole);
		GameCommon.SetUIVisiable (obj, "effect_background", bIsRole);
		
		ActiveObject mChouKaAnimatorObj = new ActiveObject();
		mChouKaAnimatorObj.mMainObject = new GameObject("_Role_");
		mChouKaAnimatorObj.CreateMainObject (modelIndex);
		GameCommon.SetLayer (mChouKaAnimatorObj.mMainObject, CommonParam.PlayerLayer);
		mChouKaAnimatorObj.mMainObject.transform.parent = GameCommon.FindObject (obj, strPoint).transform;
		mChouKaAnimatorObj.mMainObject.transform.localScale *= fScale; 
		mChouKaAnimatorObj.SetVisible (true);
		mChouKaAnimatorObj.mMainObject.transform.localPosition = Vector3.zero;
		mChouKaAnimatorObj.mMainObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		
		if(bIsRole)
		{
			GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UI/ec_ui_futuowei", GameCommon.FindObject(mChouKaAnimatorObj.mMainObject, "Bone056"));
		}
		else
		{
			GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UI/ec_ui_fufly2", GameCommon.FindObject(mChouKaAnimatorObj.mMainObject, "Bone056"));
			GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UI/ec_ui_fubagua", GameCommon.FindObject(obj, "effect_bagua"));
		}
		
		mChouKaAnimatorObj.PlayMotion ("chouka", null);
		mChouKaAnimatorObj.SetAnimSpeed (fAniSpeed);
		ChouKaAnimitorEnd mAniEvt = EventCenter.Start ("ChouKaAnimitorEnd") as ChouKaAnimitorEnd;
		mAniEvt.set ("CHOU_KA_OBJ", mChouKaAnimatorObj);
		mAniEvt.set ("GAIN_ITEM_DATA", respEvt);
		mAniEvt.mAction = callBack;
		mAniEvt.DoEvent ();
		
		SetVisible ("jump_animator_button", true);
		GameCommon.GetButtonData (mGameObjUI, "jump_animator_button").set ("ANI_EVT", mAniEvt);
		GameCommon.GetButtonData (mGameObjUI, "jump_animator_button").set ("WINDOW_NAME", mWinName);
	}

	void RoleAnimatorEnd(tEvent respEvt)
	{
		ShowGainItemAnimator (() => FuAnimatorEnd (respEvt), respEvt, 90013, 1f);
	}
	
	void FuAnimatorEnd(tEvent respEvt)
	{
		SetVisible ("jump_animator_button", false);
		object resultData;
		if (!respEvt.getData("RESULT_TABLE", out resultData))
			return;
		
		NiceTable itemData = resultData as NiceTable;
		if(itemData.GetRecordCount() == 1)
			ShowShopGainPetWindow (true);
		else
			ShowGainGroupAllItemsUI(respEvt);
	}

	void ShowShopGainPetWindow(bool bVisible)
	{
		GameObject shopGainPetWindowObj = GameCommon.FindObject (mGameObjUI, "active_gain_pet_window");
		shopGainPetWindowObj.SetActive (bVisible);
		
		if(!bVisible) return;
		
		GameCommon.SetUIVisiable (shopGainPetWindowObj, "moreThanThreeStarLevelEffect", mPetStar > 3);
		GameCommon.SetUIVisiable (shopGainPetWindowObj, "background_effect", mPetStar > 2);
		ShowStar ();
		
		PlayTweenScale(GameCommon.FindObject (shopGainPetWindowObj, "gain_pet_window_card"));
		PlayTweenScale(GameCommon.FindObject (shopGainPetWindowObj, "extract_again"));
		PlayTweenScale(GameCommon.FindObject (shopGainPetWindowObj, "active_gain_pet_window_close_btn"));
		PlayTweenScale(GameCommon.FindObject (shopGainPetWindowObj, "active_gain_pet_window_details_btn"));
	}

	public virtual void ShowGainGroupAllItemsUI(tEvent respEvt)
	{
		object resultData;
		if (!respEvt.getData("RESULT_TABLE", out resultData)) return;

		NiceTable itemData = resultData as NiceTable;

		SetAgainButtonData (itemData.GetRecordCount(), GetSub ("active_gain_item_window"));
		
		DataCenter.SetData("ACTIVE_GAIN_ITEM_WINDOW", "SET_SELECT_ITEMS_BY_MODEL_INDEX", itemData);
		DataCenter.SetData("ACTIVE_GAIN_ITEM_WINDOW", "OPEN", true);
	}

	void ShowStar()
	{
		if(mPetStar == 0) return;
		
		for(int i = 1; i < 6; i++)
		{
			SetVisible ("star_" + i.ToString (), false);
			
			if(i > 2) SetVisible ("effect_font_star_" + i.ToString (), false);
		}
		
		SetVisible ("star_" + mPetStar.ToString (), true);
		GameObject starInfo = GetSub ("star_" + mPetStar.ToString ());
		for(int i = 1; i < mPetStar + 1; i ++)
		{
			GameCommon.SetUIVisiable (starInfo, "star" + i.ToString (), false);
		}
		
		this.StartCoroutine (ShowStar (starInfo));
	}
	
	IEnumerator ShowStar(GameObject parentObj)
	{
		for(int i = 1; i < mPetStar + 1; i ++)
		{
			yield return new WaitForSeconds(0.3f);
			GameCommon.SetUIVisiable (parentObj, "star" + i.ToString (), true);
		}
		
		if(mPetStar > 2) SetVisible ("effect_font_star_" + mPetStar.ToString (), true);
	}
	
	public void PlayTweenScale(GameObject obj)
	{
		TweenScale [] tweens;
		tweens = obj.GetComponents<TweenScale>();
		foreach(TweenScale t in tweens)
		{
			t.ResetToBeginning ();
			t.PlayForward ();
		}
	}


	public void SetAgainButtonData(int times, GameObject parentObj)
	{
		int shopSlotIndex = DataCenter.mOperateEventConfig.GetData (mConfigIndex, "SHOPID_1");
		int costType = TableCommon.GetNumberFromShopSlotBase (shopSlotIndex, "COST_ITEM_TYPE");
		int costCount = TableCommon.GetNumberFromShopSlotBase (shopSlotIndex, "COST_ITEM_COUNT");

		GameCommon.GetButtonData (GameCommon.FindObject (parentObj, "extract_again")).set ("TIMES", times);
		GameCommon.GetButtonData (GameCommon.FindObject (parentObj, "extract_again")).set ("COST_TYPE", costType);
		GameCommon.GetButtonData (GameCommon.FindObject (parentObj, "extract_again")).set ("COST_COUNT", costCount);

		// number
		UILabel iconLabel = parentObj.transform.Find("extract_again/icon_label").gameObject.GetComponent<UILabel>();
		iconLabel.text = (times  * costCount).ToString();
		
		// icon
		DataRecord itemDataRecord = DataCenter.mItemIcon.GetRecord(costType);
		if (itemDataRecord != null)
		{
			UISprite iconSprite = parentObj.transform.Find("extract_again/icon_sprite").gameObject.GetComponent<UISprite>();
			string strAtlasName = itemDataRecord.get("ITEM_ATLAS_NAME").ToString ();
			string strSpriteName = itemDataRecord.get("ITEM_SPRITE_NAME").ToString ();
			GameCommon.SetIcon (iconSprite, strAtlasName, strSpriteName);
		}

	}
}


class Button_go_mysterious_shop_button : CEvent
{
	public override bool _DoEvent ()
	{
		int i = Convert.ToInt32 (SHOP_PAGE_TYPE.MYSTERIOUS);
		DataCenter.Set ("WHICH_SHOP_PAGE", i);
		
		GlobalModule.ClearAllWindow();
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);

		return true;
	}
}


class Button_lucy_pet_button : CEvent
{
	public override bool _DoEvent ()
	{
		int costType = (int)getObject ("COST_TYPE");
		int costCount = (int)getObject ("COST_COUNT");
		int times = (int)getObject ("TIMES");
		if(GameCommon.HaveEnoughCurrency ((ITEM_TYPE)costType, costCount))
		{
			DataCenter.SetData ("ACTIVE_LIMIT_BUY_PET_WINDOW", "SEND_MESSAGE", times);
		}
		return true;
	}
}


class Button_extract_again : Button_lucy_pet_button
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("ACTIVE_GAIN_ITEM_WINDOW", "CLOSE", true);
		DataCenter.SetData ("ACTIVE_GAIN_PET_WINDOW", "CLOSE", true);

		base._DoEvent ();
		return true;
	}
}


class Button_active_gain_item_window_close_btn : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData("ACTIVE_GAIN_ITEM_WINDOW", "CLOSE", true);

		DataCenter.SetData ("ACTIVE_LIST_WINDOW", "UI_HIDDEN", true);
		DataCenter.SetData ("ACTIVE_LIMIT_BUY_PET_WINDOW", "UI_HIDDEN", true);

		return true;
	}
}


class Button_active_gain_pet_window_close_btn : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData("ACTIVE_GAIN_PET_WINDOW", "CLOSE", true);
		
		DataCenter.SetData ("ACTIVE_LIST_WINDOW", "UI_HIDDEN", true);
		DataCenter.SetData ("ACTIVE_LIMIT_BUY_PET_WINDOW", "UI_HIDDEN", true);
		return true;
	}
}

class Button_active_gain_pet_window_details_btn : CEvent
{
	public override bool _DoEvent ()
	{
		ActiveGainPetWindow petInfoWindow = DataCenter.GetData("ACTIVE_GAIN_PET_WINDOW") as ActiveGainPetWindow;
		if(petInfoWindow != null)
		{
			int iModelIndex = petInfoWindow.get ("SET_SELECT_PET_BY_MODEL_INDEX");
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_BY_MODEL_INDEX", iModelIndex);
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "OPEN", PET_INFO_WINDOW_TYPE.SHOP);
			
			DataCenter.SetData ("ACTIVE_LIMIT_BUY_PET_WINDOW", "SHOW_GAIN_PET_WINDOW_CARD", false);
		}
		return true;
	}
}


//----------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------
class CS_GetBuyPetActivityInfo : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			DataCenter.SetData ("ACTIVE_LIMIT_BUY_PET_WINDOW", "REFRESH", respEvt);
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}

class CS_BuyPetInActivity : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			DataCenter.SetData ("ACTIVE_LIMIT_BUY_PET_WINDOW", "SEND_MESSAGE_RESULT", respEvt);
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}