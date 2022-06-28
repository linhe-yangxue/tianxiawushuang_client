using UnityEngine;
using System.Collections;
using DataTable;

public class PetInfoDescriptionUI : MonoBehaviour {

	public UILabel mPetStarLevelLabel;
	public UILabel mPetTitleLabel;
	public UILabel mPetNameLabel;
	
	public UILabel mDescriptionLabel;

	public UILabel mAttackLabel;
	public UILabel mMaxHPLabel;
	
	public GameObject mCard;
	public float mfCardScale = 1.0f;

	public UISprite mElementIcon;
	
	public GameObject mSkillBtn1;
	public GameObject mSkillBtn2;
	public GameObject mSkillBtn3;

	public GameObject mRoleMastButtonUI;
	
	// Use this for initialization
	void Start () {
		
		InitCard();
		
		PetInfoDescriptionWindow petInfoData = new PetInfoDescriptionWindow();
		DataCenter.Self.registerData("PetInfoDescriptionWindow", petInfoData);
		InitPetInfo(petInfoData);
	}
	
	void InitCard()
	{
		GameObject obj = GameCommon.LoadAndIntanciateUIPrefabs("card_group_window", mCard.name);		
		if(obj != null)
		{
			CardGroupUI uiScript = obj.GetComponent<CardGroupUI>();
			uiScript.InitPetInfo(mCard.name, mfCardScale, gameObject);
		}		
		mCard.transform.localScale = mCard.transform.localScale * mfCardScale;
	}
		
	void InitPetInfo(PetInfoDescriptionWindow petInfoData)
	{
		petInfoData.mPetStarLevelLabel 	= 	  mPetStarLevelLabel;
		petInfoData.mPetTitleLabel 		= 	  mPetTitleLabel;
		petInfoData.mPetNameLabel 		= 	  mPetNameLabel;
		
		petInfoData.mDescriptionLabel 	=	  mDescriptionLabel;

		petInfoData.mAttackLabel 		=	  mAttackLabel;
		petInfoData.mMaxHPLabel 		=	  mMaxHPLabel;

		petInfoData.mCard				=	  mCard;

		petInfoData.mElementIcon 		=	  mElementIcon;
		
		petInfoData.mSkillBtn1 			=	  mSkillBtn1;
		petInfoData.mSkillBtn2 			=	  mSkillBtn2;
		petInfoData.mSkillBtn3 			=	  mSkillBtn3;
		
		petInfoData.mGameObjUI			=	  gameObject;
		petInfoData.mRoleMastButtonUI	=	  mRoleMastButtonUI;
		//petInfoData.set ("CLOSE", true);
	}
	
	public void OnDestroy()
	{
		DataCenter.Remove("PetInfoDescriptionWindow");
	}
}

public class PetInfoDescriptionWindow : tWindow
{
	public UILabel mPetStarLevelLabel;
	public UILabel mPetTitleLabel;
	public UILabel mPetNameLabel;
	
	public UILabel mDescriptionLabel;

	public UILabel mAttackLabel;
	public UILabel mMaxHPLabel;

	public GameObject mCard;

	public UISprite mElementIcon;
	
	public GameObject mSkillBtn1;
	public GameObject mSkillBtn2;
	public GameObject mSkillBtn3;		

	public int mModelIndex = -1;
	public int mLevel = 30;
	public int mStrengthenLevel = 0;

	public GameObject mRoleMastButtonUI;

	public override void Init()
	{
		if(mGameObjUI != null)
			mGameObjUI = GameCommon.FindUI("PetInfoDescriptionWindow");
		
		//Close();
	}
	
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		if(keyIndex == "SET_SELECT_PET_BY_MODEL_INDEX")
		{
			SetPetByModelIndex((int)objVal);
		}
	}
	
	public void SetPetByModelIndex(int iModelIndex)
	{
		mModelIndex = iModelIndex;
		if(mModelIndex == -1)
			return;
		mPetStarLevelLabel.text = (TableCommon.GetNumberFromActiveCongfig(mModelIndex, "STAR_LEVEL")).ToString();
		mPetTitleLabel.text = TableCommon.GetStringFromActiveCongfig(mModelIndex, "NAME");
		mPetNameLabel.text = TableCommon.GetStringFromActiveCongfig(mModelIndex, "NAME");
		
		mDescriptionLabel.text = "请叫我描述";
		SetAttack();
		SetMaxHP();
		SetElementInfo();
		// 主动技能
		SetSkillInfo(mSkillBtn1, 1);
		
		// 被动技能
		//			SetSkillInfo(mSkillBtn2, 2);
		//			SetSkillInfo(mSkillBtn3, 3);
		
		
		GameCommon.SetCardInfo(mCard.name, mModelIndex, mLevel, mStrengthenLevel, mRoleMastButtonUI);
	}
	
	public void SetAttack()
	{
		int iAcctack = (int)GameCommon.GetBaseAttack(mModelIndex, mLevel, mStrengthenLevel);
		mAttackLabel.text = iAcctack.ToString();
	}
	
	public void SetMaxHP()
	{
		int iMaxHP = GameCommon.GetBaseMaxHP(mModelIndex, mLevel, mStrengthenLevel);
		mMaxHPLabel.text = iMaxHP.ToString();
	}
	
	public void SetElementInfo()
	{
		int iElementIndex = TableCommon.GetNumberFromActiveCongfig(mModelIndex, "ELEMENT_INDEX");
		
		// set icon
		string strAtlasName = TableCommon.GetStringFromElement(iElementIndex, "ELEMENT_ATLAS_NAME");
        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
		
		string strSpriteName = TableCommon.GetStringFromElement(iElementIndex, "ELEMENT_SPRITE_NAME");
		
		mElementIcon.atlas = tu;
		mElementIcon.spriteName = strSpriteName;
		mElementIcon.MakePixelPerfect();
	}
	
	public void SetSkillInfo(GameObject objBtn, int iIndex)
	{
		GameObject obj = GameCommon.FindObject(objBtn, "Background");
		if(obj == null)
			return;
		
		UISprite sprite = obj.GetComponent<UISprite>();
		
		if(sprite != null)
		{
			int iSkillIndex =  TableCommon.GetNumberFromActiveCongfig(mModelIndex, "PET_SKILL_" + iIndex.ToString());
			
			// 去技能表里取技能数据
			string strAtlasName = TableCommon.GetStringFromSkillConfig(iSkillIndex, "SKILL_ATLAS_NAME");
            UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
			
			string strSpriteName = TableCommon.GetStringFromSkillConfig(iSkillIndex, "SKILL_SPRITE_NAME");
			
			sprite.atlas = tu;
			sprite.spriteName = strSpriteName;
			sprite.MakePixelPerfect();
		}
	}
}