using UnityEngine;
using System.Collections;
using Logic;

public class SelectPetWindow : SelectModelBaseWindow {

	public SelectPetWindow()
	{
		mStrSceneName = "create_scene";
	}

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_is_he_button", new DefineFactory<Button_is_he_button>());
		EventCenter.Self.RegisterEvent("Button_select_pet_back_button", new DefineFactory<Button_SelectPetBackButton>());

		Net.gNetEventCenter.RegisterEvent("CS_RequestSelPet", new DefineFactory<CS_RequestSelPet>());
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);

	}

	public override bool Refresh (object param)
	{
        GuideManager.Notify(GuideIndex.SelectPet);
		mCurModelIndex = TableCommon.GetNumberFromSelPetUIConfig(mCurSelIndex, "MODEL");

		SetModelBackEffect();
		UpdateModel();
		UpdateSelPetInfo();
		return base.Refresh (param);
	}

	public override void SetModelBackEffect()
	{
		GameObject bgScene = GameObject.Find(mStrSceneName);
		bgScene.transform.Find("Camera/effect_group/ec_ui_status_" + mCurSelIndex).gameObject.SetActive(true);
	}

	public void UpdateModel()
	{
		SetElementUIEffect();
	}

	public void SetElementUIEffect()
	{
		if(mGameObjUI != null)
		{
			Transform effectGroupTran = mGameObjUI.transform.Find("group/effect_group");
			if(effectGroupTran != null)
			{
				for(int i = (int)ELEMENT_TYPE.RED; i <(int)ELEMENT_TYPE.MAX; i++)
				{
					effectGroupTran.Find("effect_group/ec_ui_main_" + i.ToString()).gameObject.SetActive(i == mCurSelIndex);
				}
			}
		}
	}

	public override void HideAllUIEffect()
	{
		if(mGameObjUI != null)
		{
			Transform effectGroupTran = mGameObjUI.transform.Find("group/effect_group");
			if(effectGroupTran != null)
			{
				for(int i = (int)ELEMENT_TYPE.RED; i <(int)ELEMENT_TYPE.MAX; i++)
				{
					effectGroupTran.Find("effect_group/ec_ui_main_" + i.ToString()).gameObject.SetActive(false);
				}
			}

			GameObject bgScene = GameObject.Find(mStrSceneName);
			if(bgScene != null)
			{
				for(int i = (int)ELEMENT_TYPE.RED; i <(int)ELEMENT_TYPE.MAX; i++)
				{
					bgScene.transform.Find("Camera/effect_group/ec_ui_status_" + i.ToString()).gameObject.SetActive(false);
				}
			}
		}
	}


	public void UpdateSelPetInfo()
	{
		SetSelPetInfoUI();
		SetElementUI();

//		SetButtonVisible();
	}

	public void SetSelPetInfoUI()
	{
		SetName();
		SetRecommend();
		SetElementDiscription();
		SetPetDiscription();
	}

	public void SetElementUI()
	{
		SetCurElementUI();
		SetElementSamllUIEffect();
	}

//	public void SetButtonVisible()
//	{
//		bool bIsVisible = (mCurSelIndex >= (int)ELEMENT_TYPE.RED && mCurSelIndex <= (int)ELEMENT_TYPE.GREEN);
//
//		GameObject tip = mGameObjUI.transform.Find("info_group/introduce_label").gameObject;
//		GameObject okBtn = mGameObjUI.transform.Find("info_group/is_he_button").gameObject;
//
//		tip.SetActive(!bIsVisible);
//		okBtn.SetActive(bIsVisible);
//	}

	public void SetName()
	{
		UILabel petNameLabel = mGameObjUI.transform.Find("group/pet_name").GetComponent<UILabel>();
		if(petNameLabel != null)
		{
			int curSelPetModelIndex = TableCommon.GetNumberFromSelPetUIConfig(mCurSelIndex, "MODEL");
			petNameLabel.text = TableCommon.GetStringFromActiveCongfig(curSelPetModelIndex, "NAME");

//			mTuijianSprite.transform.localPosition = new Vector3(mPetNameRight.transform.localPosition.x + mPetNameRight.width, mTuijianSprite.transform.localPosition.y, mTuijianSprite.transform.localPosition.z);
		}
	}

	public void SetRecommend()
	{
		if(mGameObjUI != null)
		{
			GameObject tuijianSprite = mGameObjUI.transform.Find("info_group/tuijian_sprite").gameObject;
			if(tuijianSprite != null)
			{
				int iRecommendFlag = TableCommon.GetNumberFromSelPetUIConfig(mCurSelIndex, "RECOMMEND");
				tuijianSprite.SetActive(iRecommendFlag == 1);
			}
		}
	}

	public void SetElementDiscription()
	{
		if(mGameObjUI != null)
		{
			UILabel forbornElementLabel = mGameObjUI.transform.Find("info_group/forborn/forborn_element_label").GetComponent<UILabel>();
			UILabel forbornPropertyLabel = mGameObjUI.transform.Find("info_group/forborn/forborn_property_label").GetComponent<UILabel>();
			UILabel restrainElementLabel = mGameObjUI.transform.Find("info_group/restrain/restrain_element_label").GetComponent<UILabel>();
			UILabel restrainPropertyLabel = mGameObjUI.transform.Find("info_group/restrain/restrain_property_label").GetComponent<UILabel>();
			
			int iPromitingIndex = TableCommon.GetNumberFromElement(mCurSelIndex, "CONSTRAINING_INDEX");
			string iPromitingText = TableCommon.GetStringFromElement(iPromitingIndex, "ELEMENT_NAME");
			string iPromitingColor = TableCommon.GetStringFromElement(iPromitingIndex, "ELEMENT_COLOR");
			
			int iConstrainingIndex = TableCommon.GetNumberFromElement(mCurSelIndex, "PROMOTING_INDEX");
			string iConstrainingText = TableCommon.GetStringFromElement(iConstrainingIndex, "ELEMENT_NAME");
			string iConstrainingColor = TableCommon.GetStringFromElement(iConstrainingIndex, "ELEMENT_COLOR");
			
			string[] ForbornProperty = forbornPropertyLabel.text.Split(']');
			string[] RestrainProperty = restrainPropertyLabel.text.Split(']');
			forbornElementLabel.text = iPromitingColor + iPromitingText;
			forbornPropertyLabel.text = iPromitingColor + ForbornProperty[ForbornProperty.Length - 1];
			restrainElementLabel.text = iConstrainingColor + iConstrainingText;
			restrainPropertyLabel.text = iConstrainingColor + RestrainProperty[RestrainProperty.Length - 1];
		}
	}

	public void SetPetDiscription()
	{
		if(mGameObjUI != null)
		{
			int curSelPetModelIndex = TableCommon.GetNumberFromSelPetUIConfig(mCurSelIndex, "MODEL");

			UILabel petIntroduceLabel = mGameObjUI.transform.Find("info_group/pet_introduce_label").GetComponent<UILabel>();
			UILabel petTypeLabel = mGameObjUI.transform.Find("info_group/pet_attribute/pet_attribute_label").GetComponent<UILabel>();
			UISprite petTypeSprite = mGameObjUI.transform.Find("info_group/pet_attribute").GetComponent<UISprite>();


			petIntroduceLabel.text = TableCommon.GetStringFromActiveCongfig(curSelPetModelIndex, "DESCRIBE");

			int iPetType = TableCommon.GetNumberFromActiveCongfig(curSelPetModelIndex, "PET_TYPE");
			string strTypeName = TableCommon.GetStringFromPetType(iPetType, "NAME");
			petTypeLabel.text = strTypeName;

			string strAtlasName = TableCommon.GetStringFromPetType(iPetType, "TPYE_ATLAS_NAME");
			string strSpriteName = TableCommon.GetStringFromPetType(iPetType, "TYPE_SPRITE_NAME");
			GameCommon.SetIcon(petTypeSprite, strAtlasName, strSpriteName);
		}
	}

	public void SetCurElementUI()
	{
		if(mGameObjUI != null)
		{
			UISprite elementIcon = mGameObjUI.transform.Find("info_group/element_icon").GetComponent<UISprite>();			

			string strAtlasName = TableCommon.GetStringFromElement(mCurSelIndex, "SELECT_PET_ELEMENT_ICON_ATLAS_NAME");
			string strSpriteName = TableCommon.GetStringFromElement(mCurSelIndex, "SELECT_PET_ELEMENT_ICON_SPRITE_NAME");
			GameCommon.SetIcon(elementIcon, strAtlasName, strSpriteName);
		}
	}

	public void SetElementSamllUIEffect()
	{
		if(mGameObjUI != null)
		{
			Transform effectGroupTran = mGameObjUI.transform.Find("group/effect_group");
			for(int i = (int)ELEMENT_TYPE.RED; i <(int)ELEMENT_TYPE.MAX; i++)
			{
				effectGroupTran.Find("small_effect_group/ec_ui_mainsmall_" + i.ToString()).gameObject.SetActive(i == mCurSelIndex);
			}
		}
	}
}


public class Button_is_he_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow("SELECT_PET_WINDOW");
		DataCenter.SetData("SELECT_PET_WINDOW", "CLEAR_GRID", true);
		DataCenter.OpenWindow("SELECT_CREATE_NAME_WINDOW");
		
		return true;
	}
}

public class Button_SelectPetBackButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow("SELECT_PET_WINDOW");
		DataCenter.SetData("SELECT_PET_WINDOW", "CLEAR_GRID", true);
		DataCenter.OpenWindow("SELECT_CREATE_ROLE_WINDOW");
		
		return true;
	}
}