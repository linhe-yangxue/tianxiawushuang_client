using UnityEngine;
using System.Collections;
using DataTable;
using Logic;

public class PetInfoSingleUI : MonoBehaviour 
{
	public UILabel mPetStarLevelLabel;
	public UILabel mPetTitleLabel;
	public UILabel mPetNameLabel;
	
	public UILabel mLevel;
	public UILabel mShopPetLevel;
	public UILabel mExpPercentage;
	public UIProgressBar mPetExpBar;
	
	public UILabel mStrengthenLevelLabel;
	public UILabel mAttackLabel;
	public UILabel mMaxHPLabel;
	public UILabel mFightingLabel;
	public UILabel mAttributeLabel;
	public UILabel mAttackTypeLabel;

	public UILabel mShopAttackLabel;
	public UILabel mShopMaxHPLabel;

	public GameObject mCard;

	public UISprite mElementIcon;
	
	public GameObject mSkillBtn1;
	public GameObject mSkillBtn2;
	public GameObject mSkillBtn3;

	public GameObject mRoleMastButtonUI;

	public GameObject mPetInfoGroup;
	public GameObject mShopPetInfoGroup;
	public bool mbIsShop = false;

	// Use this for initialization
	void Awake () {

        Init();
	}

    public virtual void Init()
    {
		return;
//        PetInfoSingleWindow petInfoData = new PetInfoSingleWindow();
//        DataCenter.Self.registerData("PET_INFO_SINGLE_WINDOW", petInfoData);

		PetInfoSingleWindow petInfoData = DataCenter.GetData("PET_INFO_SINGLE_WINDOW") as PetInfoSingleWindow;
        InitPetInfo(petInfoData);
    }

    public void InitPetInfo(PetInfoSingleWindow petInfoData)
	{
		petInfoData.mPetStarLevelLabel 	= 	  mPetStarLevelLabel;
		petInfoData.mPetTitleLabel 		= 	  mPetTitleLabel;
		petInfoData.mPetNameLabel 		= 	  mPetNameLabel;
		
		petInfoData.mLevel 				=	  mLevel;
		petInfoData.mShopPetLevel 				=	  mShopPetLevel;
		petInfoData.mExpPercentage 		=	  mExpPercentage;
		petInfoData.mPetExpBar 			=	  mPetExpBar;
		
		petInfoData.mStrengthenLevelLabel	=	  mStrengthenLevelLabel;
		petInfoData.mAttackLabel 		=	  mAttackLabel;
		petInfoData.mMaxHPLabel 		=	  mMaxHPLabel;
		petInfoData.mFightingLabel    =     mFightingLabel;
		petInfoData.mAttributeLabel    =     mAttributeLabel;
		petInfoData.mAttackTypeLabel    =     mAttackTypeLabel;

		petInfoData.mShopAttackLabel 		=	  mShopAttackLabel;
		petInfoData.mShopMaxHPLabel 		=	  mShopMaxHPLabel;

//		petInfoData.mPetInfoGroup 		=	  mPetInfoGroup;
//		petInfoData.mShopPetInfoGroup 		=	  mShopPetInfoGroup;

//		petInfoData.mCardStrengthenLevelLabel	=	  mCardStrengthenLevelLabel;
//		petInfoData.mCardAttackLabel 		=	  mCardAttackLabel;
//		petInfoData.mCardMaxHPLabel 		=	  mCardMaxHPLabel;
//		petInfoData.mSkillIcon 			=	  mSkillIcon;
//		petInfoData.mCard				=	  mCard;

		petInfoData.mElementIcon 		=	  mElementIcon;
		
		petInfoData.mSkillBtn1 			=	  mSkillBtn1;
		petInfoData.mSkillBtn2 			=	  mSkillBtn2;
		petInfoData.mSkillBtn3 			=	  mSkillBtn3;

		petInfoData.mGameObjUI			=	  gameObject;

		petInfoData.mRoleMastButtonUI	=	  mRoleMastButtonUI;
		petInfoData.set ("CLOSE", true);
	}
	
	public void OnDestroy()
	{

//        DataCenter.Remove("PET_INFO_SINGLE_WINDOW");
	}

}


public enum PET_INFO_WINDOW_TYPE
{
	PET,
	SHOP,
	TUJIAN,
	COMPOSE,
}

public class PetInfoSingleBaseWindow : tWindow
{
	public UILabel mPetStarLevelLabel;
	public UILabel mPetTitleLabel;
	public UILabel mPetNameLabel;
	
	public UILabel mLevel;
	public UILabel mShopPetLevel;
	public UILabel mExpPercentage;
	public UIProgressBar mPetExpBar;
	
	public UILabel mStrengthenLevelLabel;
	public UILabel mAttackLabel;
	public UILabel mMaxHPLabel;
	public UILabel mFightingLabel;
	public UILabel mAttributeLabel;
	public UILabel mAttackTypeLabel;

	public UILabel mShopAttackLabel;
	public UILabel mShopMaxHPLabel;
	public GameObject mShopTip;
    public UILabel mShopAttributeLabel;
    public UILabel mShopAttackTypeLabel;

	public UILabel mTujianAttackLabel;
	public UILabel mTujianMaxHPLabel;
	public GameObject mTujianAwardGroup;
	public GameObject mTujianTip;
	public UILabel mTujianTipLabel;
    public UILabel mTujianAttributeLabel;
    public UILabel mTujianAttackTypeLabel;

	public UILabel mComposeAttackLabel;
	public UILabel mComposeMaxHPLabel;

	public GameObject mPetInfoGroup;
	public GameObject mShopPetInfoGroup;
	public GameObject mTujianPetInfoGroup;
	public GameObject mComposePetInfoGroup;

	public GameObject mCard;

	public UISprite mElementIcon;
    public UISprite mShopElementIcon;
    public UISprite mTujianElementIcon;
	
	public GameObject mSkillBtn1;
	public GameObject mSkillBtn2;
	public GameObject mSkillBtn3;

	public PetData mPetData = null;

	public GameObject mRoleMastButtonUI;

	public bool mbIsShop = false;

	public string m_strWindowName = "PetInfoSingleWindow";

	public PET_INFO_WINDOW_TYPE mCurPetInfoWindowType = PET_INFO_WINDOW_TYPE.PET;

	public float mfCardScale = 1.0f;

	public int mCurAnimIndex = 0;

	public string[] mVecAnimName = 
	{
		"attack",
		"run",
		"win",
		"stun",
	};

	public float[] mVecPetInfoLocalPosX = 
	{
		-164.3f,
		-128.2f,
		-128.2f,
		-128.2f,
	};

	public override void Init()
	{

	}

	public virtual void InitWindow()
	{
		if(mGameObjUI != null)
		{
			mPetStarLevelLabel = mGameObjUI.transform.Find("BG/pet_info/Info/star_level").GetComponent<UILabel>();
			mPetTitleLabel = mGameObjUI.transform.Find("BG/pet_info/Info/title").GetComponent<UILabel>();
			mPetNameLabel = mGameObjUI.transform.Find("BG/pet_info/Info/name").GetComponent<UILabel>();

			mSkillBtn1 = mGameObjUI.transform.Find("BG/pet_info/skill/Button1").gameObject;
			mSkillBtn2 = mGameObjUI.transform.Find("BG/pet_info/skill/Button2").gameObject;
			mSkillBtn3 = mGameObjUI.transform.Find("BG/pet_info/skill/Button3").gameObject;
			
			mRoleMastButtonUI = mGameObjUI.transform.Find("BG/black").gameObject;

			mPetInfoGroup = mGameObjUI.transform.Find("BG/pet_info_group").gameObject;
			mShopPetInfoGroup = mGameObjUI.transform.Find("BG/shop_pet_info_group").gameObject;
			mTujianPetInfoGroup = mGameObjUI.transform.Find("BG/tujian_pet_info_group").gameObject;
			mComposePetInfoGroup = mGameObjUI.transform.Find("BG/compose_pet_info_group").gameObject;

			mLevel = mPetInfoGroup.transform.Find("level_info/label").GetComponent<UILabel>();
			mExpPercentage = mPetInfoGroup.transform.Find("level_info/Progress Bar/label").GetComponent<UILabel>();
			mPetExpBar = mPetInfoGroup.transform.Find("level_info/Progress Bar").GetComponent<UIProgressBar>();
			
			mStrengthenLevelLabel = mPetInfoGroup.transform.Find("strengthen_level_info/num").GetComponent<UILabel>();
			mAttackLabel = mPetInfoGroup.transform.Find("damage_info/num").GetComponent<UILabel>();
			mMaxHPLabel = mPetInfoGroup.transform.Find("life_info/num").GetComponent<UILabel>();
			mFightingLabel = mPetInfoGroup.transform.Find("fighting_info/num").GetComponent<UILabel>();
			mAttributeLabel = mPetInfoGroup.transform.Find("attribute_info/Label").GetComponent<UILabel>();
			mAttackTypeLabel = mPetInfoGroup.transform.Find("attribute_info/attack_type_label").GetComponent<UILabel>();
            mElementIcon = mPetInfoGroup.transform.Find("attribute_info/sprite").GetComponent<UISprite>();

			mShopPetLevel = mShopPetInfoGroup.transform.Find("level_label").GetComponent<UILabel>();
			mShopAttackLabel = mShopPetInfoGroup.transform.Find("damage_info/num").GetComponent<UILabel>();
			mShopMaxHPLabel = mShopPetInfoGroup.transform.Find("life_info/num").GetComponent<UILabel>();
            mShopAttributeLabel = mShopPetInfoGroup.transform.Find("attribute_info/Label").GetComponent<UILabel>();
            mShopAttackTypeLabel = mShopPetInfoGroup.transform.Find("attribute_info/attack_type_label").GetComponent<UILabel>();
            mShopElementIcon = mShopPetInfoGroup.transform.Find("attribute_info/sprite").GetComponent<UISprite>();

			mTujianAttackLabel = mTujianPetInfoGroup.transform.Find("damage_info/num").GetComponent<UILabel>();
			mTujianMaxHPLabel = mTujianPetInfoGroup.transform.Find("life_info/num").GetComponent<UILabel>();
            mTujianAwardGroup = mTujianPetInfoGroup.transform.Find("tujian_award_group").gameObject;
			mTujianTip = mTujianPetInfoGroup.transform.Find("sprite").gameObject;
			mTujianTipLabel = mTujianPetInfoGroup.transform.Find("sprite/label").GetComponent<UILabel>();
            mTujianAttributeLabel = mTujianPetInfoGroup.transform.Find("attribute_info/Label").GetComponent<UILabel>();
            mTujianAttackTypeLabel = mTujianPetInfoGroup.transform.Find("attribute_info/attack_type_label").GetComponent<UILabel>();
            mTujianElementIcon = mTujianPetInfoGroup.transform.Find("attribute_info/sprite").GetComponent<UISprite>();

			mComposeAttackLabel = mComposePetInfoGroup.transform.Find("damage_info/num").GetComponent<UILabel>();
			mComposeMaxHPLabel = mComposePetInfoGroup.transform.Find("life_info/num").GetComponent<UILabel>();

			InitCard();
		}
	}

	void InitCard()
	{
		if(mCard.transform.childCount == 0)
		{
			GameObject obj = GameCommon.LoadAndIntanciateUIPrefabs("card_group_window", mCard.name);
			
			if(obj != null)
			{
				CardGroupUI uiScript = obj.GetComponent<CardGroupUI>();
				uiScript.InitPetInfo(mCard.name, mfCardScale, mGameObjUI);
			}
			
			mCard.transform.localScale = mCard.transform.localScale * mfCardScale;
		}
	}

	public override void OnOpen ()
	{
		base.OnOpen ();


	}
	public override void Open(object param) 
	{
		base.Open(param);

		ShowWindow((PET_INFO_WINDOW_TYPE)param);
        //by chenliang
        //begin

        //初始化窗口
        InitWindow();

        //end
	}

	public void ShowWindow(PET_INFO_WINDOW_TYPE petInfoWindowType)
	{
		mCurPetInfoWindowType = petInfoWindowType;
		Transform petInfoTrans = mGameObjUI.transform.Find("BG/pet_info");
		if(petInfoTrans != null)
			petInfoTrans.localPosition = new Vector3(mVecPetInfoLocalPosX[(int)mCurPetInfoWindowType], petInfoTrans.localPosition.y, petInfoTrans.localPosition.z);

		if(mPetInfoGroup != null)
			mPetInfoGroup.SetActive(mCurPetInfoWindowType == PET_INFO_WINDOW_TYPE.PET);

		if(mShopPetInfoGroup != null)
			mShopPetInfoGroup.SetActive(mCurPetInfoWindowType == PET_INFO_WINDOW_TYPE.SHOP);

		if(mTujianPetInfoGroup != null)
			mTujianPetInfoGroup.SetActive(mCurPetInfoWindowType == PET_INFO_WINDOW_TYPE.TUJIAN);

		if(mComposePetInfoGroup != null)
			mComposePetInfoGroup.SetActive(mCurPetInfoWindowType == PET_INFO_WINDOW_TYPE.COMPOSE);

		Transform closeBtnTrans = mGameObjUI.transform.Find("BG/PetInfoSingleWindowCloseBtn");
		if(closeBtnTrans != null)
			closeBtnTrans.gameObject.SetActive(mCurPetInfoWindowType != PET_INFO_WINDOW_TYPE.COMPOSE);
	}
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		if(keyIndex == "INIT_WINDOW")
		{
			InitWindow();
		}
		else if(keyIndex == "SHOW_WINDOW")
		{
			ShowWindow((PET_INFO_WINDOW_TYPE)objVal);
		}
		else if(keyIndex == "SET_SELECT_PET_BY_ID")
		{
			SetPetByID((int)objVal);
		}
		else if (keyIndex == "SET_SELECT_PET_BY_MODEL_INDEX")
		{
			SetPetByModelIndex((int)objVal);
		}
		else if(keyIndex == "PLAY_ANIM")
		{
			PlayAnim((string)objVal);
		}
		else if(keyIndex == "PLAY_ANIM_SEQUENCE")
		{
			PlayAnimSequence();
		}
		else if(keyIndex == "SALE_PET")
		{
			RequestSalePet();
		}
		else if(keyIndex == "SALE_PET_RESULT")
		{
			RequestSalePetResult();
		}
	}

	public void PlayAnim(string strAnimate)
	{
		if(strAnimate == "attack")
		{
			int iIndex = Random.Range(0, 2) + 1;

			strAnimate = strAnimate + "_" +iIndex.ToString();
		}

		ActiveBirthForUI activeBirthForUI = GameCommon.FindObject(mCard, "UIPoint").GetComponent<ActiveBirthForUI>();
		if(activeBirthForUI != null)
		{
			Logic.tEvent idleEvent = Logic.EventCenter.Self.StartEvent("RoleSelUI_PlayIdleEvent");
			activeBirthForUI.mActiveObject.PlayMotion(strAnimate, idleEvent);
		}
	}

	public void PlayAnimSequence()
	{
		PlayAnim(mVecAnimName[mCurAnimIndex++%mVecAnimName.Length]);
	}

	public virtual void RequestSalePet()
	{
	}
	
	public virtual void RequestSalePetResult()
	{
        int iID = (int)(get("SET_SELECT_PET_ID"));
        SaleGetGold(iID);
        PetLogicData logic = DataCenter.GetData("PET_DATA") as PetLogicData;

        logic.RemoveItemData(iID);
        DataCenter.SetData("PetInfoWindow", "TEAM_REFRESH", true);

        GameCommon.PlaySound("Sound/new/key/sell", GameCommon.GetMainCamera().transform.position);
	}

	public virtual void SaleGetGold(int iItemId)
	{
		PetLogicData logic = DataCenter.GetData("PET_DATA") as PetLogicData;
		PetData pet = logic.GetPetDataByItemId(iItemId);
		if(pet != null)
		{
			int iModelIndex = pet.tid;
            string[] kind_value = TableCommon.GetStringFromActiveCongfig(iModelIndex, "SELL_PRICE").Split('#');
            int iPrice =int.Parse( kind_value[1]);
			RoleLogicData.Self.AddGold((int)(iPrice * (1 + 0.05 * pet.level)));
		}
	}

	public void SetPetInfoTipBtn()
	{
		Transform tipBtnTrans = mGameObjUI.transform.Find("BG/tip_btn");
		if(tipBtnTrans != null)
		{
			UIButtonEvent btnEvent =  tipBtnTrans.GetComponent<UIButtonEvent>();
			if(btnEvent != null)
			{
				int iModelIndex = mPetData.tid;
				btnEvent.mData.set ("INDEX", iModelIndex);
				btnEvent.mData.set ("TABLE_NAME", "ActiveObject");
			}
		}
	}

	public void SetPetByID(int iID)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mPetData = petLogicData.GetPetDataByItemId(iID);
		if(mPetData != null)
		{
            //by chenliang
            //begin

//			mPetStarLevelLabel.text = mPetData.starLevel.ToString();
//------------------
            mPetStarLevelLabel.text = "Lv." + mPetData.starLevel.ToString();

            //end
			mPetTitleLabel.text = TableCommon.GetStringFromActiveCongfig(mPetData.tid, "NAME");
			mPetNameLabel.text = TableCommon.GetStringFromActiveCongfig(mPetData.tid, "TXT");
			
			mLevel.text = "Lv." + mPetData.level.ToString();
			SetExpInfo();
			SetStrengthenLevel();
			SetAttack();
			SetMaxHP();
			SetFightingNum();
			SetAttributeLabel();
			SetAttackTypeLabel();
			SetElementInfo();
			SetSkillInfo();
			SetPetInfoTipBtn();

            GameCommon.SetCardInfo(mCard.name, mPetData.tid, mPetData.level, mPetData.strengthenLevel, mRoleMastButtonUI);

			DataCenter.SetData("PetInfoWindow", "SHOW_PET_FLAG", false);

			// set button
			SetBtn();
		}
	}

	public virtual void SetBtn()
	{

	}

    public virtual void SetPetByModelIndex(int iModelIndex)
    {
        mPetData = new PetData();
        mPetData.level = 1;

		if(mCurPetInfoWindowType == PET_INFO_WINDOW_TYPE.TUJIAN)
		{
			mPetData.mMaxLevelNum = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "MAX_LEVEL");
			mPetData.level = mPetData.mMaxLevelNum;
			
			TujianLogicData logic = DataCenter.GetData("TUJIAN_DATA") as TujianLogicData;
			TujianData tujianData = logic.GetTujianDataByModelIndex(iModelIndex);
			
			TUJIAN_STATUS status = TUJIAN_STATUS.TUJIAN_NOTHAD;
			if(tujianData != null)
			{
				status = (TUJIAN_STATUS)tujianData.mStatus;
			}
			mTujianAwardGroup.SetActive(status == TUJIAN_STATUS.TUJIAN_REWARD);
			mTujianTip.SetActive(status != TUJIAN_STATUS.TUJIAN_REWARD);
			mTujianTipLabel.text = TableCommon.GetStringFromActiveCongfig(iModelIndex, "DESCRIBE");
		}

		mPetData.starLevel = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "STAR_LEVEL");
		mPetData.exp = 0;
		mPetData.tid = iModelIndex;
		mPetData.strengthenLevel = 0;
        mPetData.SetSkillData();

        //by chenliang
        //begin

//        mPetStarLevelLabel.text = mPetData.starLevel.ToString();
//----------------
        mPetStarLevelLabel.text = "Lv." + mPetData.starLevel.ToString();

        //end
        mPetTitleLabel.text = TableCommon.GetStringFromActiveCongfig(iModelIndex, "NAME");
		mPetNameLabel.text = TableCommon.GetStringFromActiveCongfig(iModelIndex, "TXT");
		mShopPetLevel.text = "Lv." + mPetData.level;
        SetExpInfo();
        SetStrengthenLevel();
        SetAttack();
		SetFightingNum();
		SetAttributeLabel();
		SetAttackTypeLabel();
        SetMaxHP();
        SetElementInfo();
		SetSkillInfo();
		SetPetInfoTipBtn();

        GameCommon.SetCardInfo(mCard.name, mPetData.tid, mPetData.level, mPetData.strengthenLevel, mRoleMastButtonUI);

        if (mPetData != null)
        {
            mPetData = null;
            System.GC.Collect();
        }

    }

	public virtual void UpdateUI(){}

    public void SetExpInfo()
    {
        int curExp = mPetData.exp;
        int maxExp = TableCommon.GetMaxExp(mPetData.starLevel, mPetData.level);
        float percentage = curExp / (float)maxExp;

        int iPercentage = (int)(percentage * 100);

        mExpPercentage.text = iPercentage.ToString() + "%";
        mPetExpBar.value = percentage;
    }
	
	public void SetStrengthenLevel()
	{
		if(mPetData.strengthenLevel > 0)
		{
			mStrengthenLevelLabel.text = "+" + mPetData.strengthenLevel.ToString();
		}
		else
		{
			mStrengthenLevelLabel.text = "+" + "0";
		}
	}
	
	public void SetAttack()
	{
		int iAcctack = (int)GameCommon.GetBaseAttack(mPetData.tid, mPetData.level, mPetData.strengthenLevel);
		mAttackLabel.text = iAcctack.ToString();
		if(mShopAttackLabel != null)
			mShopAttackLabel.text = iAcctack.ToString();

		if(mTujianAttackLabel != null)
			mTujianAttackLabel.text = iAcctack.ToString();
	}
	
	public void SetMaxHP()
	{
		int iMaxHP = GameCommon.GetBaseMaxHP(mPetData.tid, mPetData.level, mPetData.strengthenLevel);
		mMaxHPLabel.text = iMaxHP.ToString();
		if(mShopMaxHPLabel != null)
			mShopMaxHPLabel.text = iMaxHP.ToString();

		if(mTujianMaxHPLabel != null)
			mTujianMaxHPLabel.text = iMaxHP.ToString();
	}

	public void SetFightingNum()
	{
		int iFightingNum = GameCommon.GetFightingStrength (mPetData.tid, mPetData.level, mPetData.strengthenLevel);
		mFightingLabel.text = iFightingNum.ToString ();
	}

	public void SetAttributeLabel()
	{
		int iElementIndex = TableCommon.GetNumberFromActiveCongfig (mPetData.tid, "ELEMENT_INDEX");
		if(iElementIndex > 4) return;

		string strElementColor = TableCommon.GetStringFromElement (iElementIndex, "ELEMENT_COLOR");
		string strElementName = TableCommon.GetStringFromElement (iElementIndex, "ELEMENT_NAME");
        if (mAttributeLabel != null)
        {
            mAttributeLabel.text = strElementColor + strElementName;

            UILabel attributeLabel = mAttributeLabel.transform.parent.GetComponent<UILabel>();
            if (attributeLabel != null) attributeLabel.text = strElementColor + attributeLabel.text.Substring(attributeLabel.text.Length - 2, 2);
        }
        if (mShopAttributeLabel != null)
        {
            mShopAttributeLabel.text = strElementColor + strElementName;

            UILabel attributeLabel = mShopAttributeLabel.transform.parent.GetComponent<UILabel>();
            if (attributeLabel != null) attributeLabel.text = strElementColor + attributeLabel.text.Substring(attributeLabel.text.Length - 2, 2);
        }
        if (mTujianAttributeLabel != null)
        {
            mTujianAttributeLabel.text = strElementColor + strElementName;

            UILabel attributeLabel = mTujianAttributeLabel.transform.parent.GetComponent<UILabel>();
            if (attributeLabel != null) attributeLabel.text = strElementColor + attributeLabel.text.Substring(attributeLabel.text.Length - 2, 2);
        }		
	}

	public void SetAttackTypeLabel()
    {
        if (mAttackTypeLabel != null)
            mAttackTypeLabel.text = GameCommon.GetAttackType(mPetData.tid);

        if (mShopAttackTypeLabel != null)
            mShopAttackTypeLabel.text = GameCommon.GetAttackType(mPetData.tid);

        if (mTujianAttackTypeLabel != null)
            mTujianAttackTypeLabel.text = GameCommon.GetAttackType(mPetData.tid);
	}

	public void SetElementInfo()
	{
		int iElementIndex = TableCommon.GetNumberFromActiveCongfig(mPetData.tid, "ELEMENT_INDEX");
				
		// set icon
		string strAtlasName = TableCommon.GetStringFromElement(iElementIndex, "ELEMENT_ATLAS_NAME");
        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
		
		string strSpriteName = TableCommon.GetStringFromElement(iElementIndex, "ELEMENT_SPRITE_NAME");

        if (mElementIcon != null)
        {
            mElementIcon.atlas = tu;
            mElementIcon.spriteName = strSpriteName;
            //mElementIcon.MakePixelPerfect();
        }
        if (mShopElementIcon != null)
        {
            mShopElementIcon.atlas = tu;
            mShopElementIcon.spriteName = strSpriteName;
            //mShopElementIcon.MakePixelPerfect();
        }
        if (mTujianElementIcon != null)
        {
            mTujianElementIcon.atlas = tu;
            mTujianElementIcon.spriteName = strSpriteName;
            //mTujianElementIcon.MakePixelPerfect();
        }
		
	}

	public void SetSkillInfo()
	{
		// 主动技能
		SetSkillInfo(mSkillBtn1, 0);
		
		// 被动技能
        //int iIndex = 1;
        //SetUnactiveSkillInfo(mSkillBtn2, ref iIndex);
        //SetUnactiveSkillInfo(mSkillBtn3, ref iIndex);

        SetSkillInfo(mSkillBtn2, 1);
        SetSkillInfo(mSkillBtn3, 2);
	}

	public void SetSkillInfo(GameObject objBtn, UIAtlas tu, string strSpriteName)
	{
		GameObject obj = GameCommon.FindObject(objBtn, "background");
		if(obj == null)
			return;
		
		UISprite sprite = obj.GetComponent<UISprite>();

		if(sprite != null)
		{
			sprite.atlas = tu;
			sprite.spriteName = strSpriteName;
		}
	}

	public void SetSkillInfo(GameObject objBtn, string strFieldName)
	{
		GameObject obj = GameCommon.FindObject(objBtn, "background");
		if(obj == null)
			return;
		
		UISprite sprite = obj.GetComponent<UISprite>();
		
		if(sprite != null)
		{
            int iSkillIndex = mPetData.GetSkillIndexByIndex(0);//TableCommon.GetNumberFromActiveCongfig(mPetData.mModelIndex, strFieldName);
			
			// 去技能表里取技能数据
			string strAtlasName = TableCommon.GetStringFromSkillConfig(iSkillIndex, "SKILL_ATLAS_NAME");
            UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
			
			string strSpriteName = TableCommon.GetStringFromSkillConfig(iSkillIndex, "SKILL_SPRITE_NAME");
			
			sprite.atlas = tu;
			sprite.spriteName = strSpriteName;
			//sprite.MakePixelPerfect();
			
			objBtn.SetActive(true);
			if(tu == null)
				objBtn.SetActive(false);

			UIButtonEvent btnEvent =  objBtn.GetComponent<UIButtonEvent>();
			if(btnEvent != null)
			{
				btnEvent.mData.set ("INDEX", iSkillIndex);
				btnEvent.mData.set ("TABLE_NAME", "SKILL");
			}
		}
	}

	public void SetActiveSkillInfo(GameObject objBtn, int iIndex)
	{
        return;
		SetSkillInfo(objBtn, "PET_SKILL_" + iIndex.ToString());
	}

	public void SetUnactiveSkillInfo(GameObject objBtn, ref int iIndex)
	{
        return;
		while(true)
		{
			UIAtlas tu = null;
			string strSpriteName = "";

			if(iIndex < 1 || iIndex > 6)
			{
                SetSkillInfo(objBtn, tu, strSpriteName);
                return;
			}

			int iCol = (iIndex - 1) % 3;

			int iSkillIndex = 0;
			string strTable = "";
			if(iIndex <= 3)
			{
				strTable = "ATTACK_STATE";
			}
			else if(iIndex <= 6)
			{
				strTable = "AFFECT_BUFFER";
			}

			iSkillIndex =  TableCommon.GetNumberFromActiveCongfig(mPetData.tid, strTable + "_" + (iCol + 1).ToString());

			if(iSkillIndex <= 0)
			{
				iIndex++;
				continue;
			}

			if(iIndex <= 3)
			{
                string strAtlasName = TableCommon.GetStringFromAttackState(iSkillIndex, "SKILL_ATLAS_NAME");
                tu = GameCommon.LoadUIAtlas(strAtlasName);

                strSpriteName = TableCommon.GetStringFromAttackState(iSkillIndex, "SKILL_SPRITE_NAME");
			}
			else
			{
				string strAtlasName = TableCommon.GetStringFromAffectBuffer(iSkillIndex, "SKILL_ATLAS_NAME");
                tu = GameCommon.LoadUIAtlas(strAtlasName);
				
				strSpriteName = TableCommon.GetStringFromAffectBuffer(iSkillIndex, "SKILL_SPRITE_NAME");
			}

			SetSkillInfo(objBtn, tu, strSpriteName);
			iIndex++;


			UIButtonEvent btnEvent =  objBtn.GetComponent<UIButtonEvent>();
			if(btnEvent != null)
			{
				btnEvent.mData.set ("INDEX", iSkillIndex);
				btnEvent.mData.set ("TABLE_NAME", strTable);
			}
			
			return;
		}
	}

    public void SetSkillInfo(GameObject objBtn, int iIndex)
    {
        UIAtlas tu = null;
        string strSpriteName = "";
        int iSkillIndex = mPetData.GetSkillIndexByIndex(iIndex);
        objBtn.SetActive(iSkillIndex > 0);
        if (iSkillIndex <= 0)
        {
            return;
        }

        string strAtlasName = GameCommon.GetStringFromSkill(iSkillIndex, "SKILL_ATLAS_NAME");
        tu = GameCommon.LoadUIAtlas(strAtlasName);

        strSpriteName = GameCommon.GetStringFromSkill(iSkillIndex, "SKILL_SPRITE_NAME");

        SetSkillInfo(objBtn, tu, strSpriteName);

        // name
        GameCommon.SetPetSkillName(objBtn, iSkillIndex, "skill_name");

        // level
        GameCommon.SetPetSkillLevel(objBtn, iSkillIndex, "skill_level", mPetData);

        UIButtonEvent btnEvent = objBtn.GetComponent<UIButtonEvent>();
        if (btnEvent != null)
        {
            string strTable = string.Empty;
            string str = iSkillIndex.ToString().Substring(0, 1);
            if (str == "8")
            {
                strTable = "SKILL";
            }
            else if (str == "9")
            {
                strTable = "AFFECT_BUFFER";
			}
            else
            {
                strTable = "ATTACK_STATE";
            }

            btnEvent.mData.set("INDEX", iSkillIndex);
            btnEvent.mData.set("TABLE_NAME", strTable);
        }

    }

    public override void OnClose()
    {
        base.OnClose();

        GameObject obj = GameCommon.FindObject(mCard, "ui_3d_model");
        if (obj != null)
            obj.SetActive(false);
    }
}

public class PetInfoSingleWindow : PetInfoSingleBaseWindow
{	
	public override void InitWindow()
	{
		if(mGameObjUI != null)
		{
            mCard = mGameObjUI.transform.Find("BG/pet_info/PetInfoSingleWindowCard").gameObject;

            mGameObjUI.transform.Find("BG/pet_info_group/PetInfoSingleOKBtn").gameObject.SetActive(true);
            mGameObjUI.transform.Find("BG/pet_info_group/PetInfoSingleWinUpgradeBtnGroup").gameObject.SetActive(true);
			mGameObjUI.transform.Find ("BG/PetInfoSingleWindowCloseBtn").gameObject.SetActive (true);

			if(mGameObjUI.transform.Find("BG/pet_info/pvp_pet_info_single_window_card") != null)
			{
				GameObject.Destroy (mGameObjUI.transform.Find("BG/pet_info/pvp_pet_info_single_window_card").gameObject);
                GameObject.Destroy(mGameObjUI.transform.Find("BG/pet_info_group/pvp_pet_info_single_ok_button").gameObject);
                GameObject.Destroy(mGameObjUI.transform.Find("BG/pet_info_group/PvpPetInfoSingleWinUpgradeBtnGroup").gameObject);
                GameObject.Destroy(mGameObjUI.transform.Find("BG/pvp_pet_info_single_window_close").gameObject);
			}
		}

		base.InitWindow();
	
        //by chenliang
        //begin

//		DataCenter.CloseWindow("PET_INFO_SINGLE_WINDOW");
//---------------
        //不能关闭

        //end
	}

	public override void OnOpen ()
	{
		base.OnOpen ();
		InitButtonIsUnLockOrNot ();
	}

	void InitButtonIsUnLockOrNot()
	{
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade", UNLOCK_FUNCTION_TYPE.UPGRADE_PET);
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade_JinHua", UNLOCK_FUNCTION_TYPE.STRENGTHER_PET);
		//GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade_QiangHua", UNLOCK_FUNCTION_TYPE.STRENGTHER_PET);
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade_Skill", UNLOCK_FUNCTION_TYPE.SKILL_PET);
	}


    public override void RequestSalePet()
    {
        int iID = (int)(get("SET_SELECT_PET_ID"));
        tEvent tempQuest = Net.StartEvent("CS_RequestSalePet");
        CS_RequestSalePet quest = tempQuest as CS_RequestSalePet;
        quest.set("PET_ID", iID);
        quest.mAction = () => DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SALE_PET_RESULT", true);
        quest.DoEvent();
    }

    public override void RequestSalePetResult()
    {
        base.RequestSalePetResult();
        Close();
    }

	public override void SetBtn()
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;

		Transform btnTrans = mGameObjUI.transform.Find("BG/pet_info_group/PetInfoSingleOKBtn");		
		UILabel OkBtnLabel1 = btnTrans.Find("label").GetComponent<UILabel>();
		UILabel OkBtnLabel2 = btnTrans.Find("label/label").GetComponent<UILabel>();
		
		string strLabel = "确定";

		if((DataCenter.GetData("PetInfoWindow") as tWindow).IsOpen())
		{
			if(petLogicData.IsPetUsedInCurTeam(mPetData.itemId))
			{
				strLabel = "下阵";
			}
			else
			{
				strLabel = "上阵";
			}
		}
		
		OkBtnLabel1.text = strLabel;
		OkBtnLabel2.text = strLabel;
	}

}

public class PVPPetInfoSingleWindow : PetInfoSingleBaseWindow
{	
	public override void InitWindow()
	{
		if(mGameObjUI != null)
		{
			mCard = mGameObjUI.transform.Find("BG/pet_info/pvp_pet_info_single_window_card").gameObject;

			mGameObjUI.transform.Find ("BG/pet_info_group/pvp_pet_info_single_ok_button").gameObject.SetActive (true);
            mGameObjUI.transform.Find("BG/pet_info_group/PvpPetInfoSingleWinUpgradeBtnGroup").gameObject.SetActive(true);
			mGameObjUI.transform.Find ("BG/pvp_pet_info_single_window_close").gameObject.SetActive (true);

			if(mGameObjUI.transform.Find("BG/pet_info/PetInfoSingleWindowCard") != null)
			{
				GameObject.Destroy (mGameObjUI.transform.Find("BG/pet_info/PetInfoSingleWindowCard").gameObject);
				GameObject.Destroy (mGameObjUI.transform.Find ("BG/pet_info_group/PetInfoSingleOKBtn").gameObject);
                GameObject.Destroy(mGameObjUI.transform.Find("BG/pet_info_group/PetInfoSingleWinUpgradeBtnGroup").gameObject);
				GameObject.Destroy (mGameObjUI.transform.Find ("BG/PetInfoSingleWindowCloseBtn").gameObject);
			}
		}
		base.InitWindow();

		DataCenter.CloseWindow("PVP_PET_INFO_SINGLE_WINDOW");
	}

	public override void OnOpen ()
	{
		base.OnOpen ();
		InitButtonIsUnLockOrNot ();
	}

	void InitButtonIsUnLockOrNot()
	{
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade", UNLOCK_FUNCTION_TYPE.UPGRADE_PET);
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade_JinHua", UNLOCK_FUNCTION_TYPE.STRENGTHER_PET);
		//GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade_QiangHua", UNLOCK_FUNCTION_TYPE.STRENGTHER_PET);
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade_Skill", UNLOCK_FUNCTION_TYPE.SKILL_PET);
	}

    public override void RequestSalePet()
    {
        int iID = (int)(get("SET_SELECT_PET_ID"));
        tEvent tempQuest = Net.StartEvent("CS_RequestSalePet");
        CS_RequestSalePet quest = tempQuest as CS_RequestSalePet;
        quest.set("PET_ID", iID);
        quest.mAction = () => DataCenter.SetData("PVP_PET_INFO_SINGLE_WINDOW", "SALE_PET_RESULT", true);
        quest.DoEvent();
    }

    public override void RequestSalePetResult()
    {
        int iID = (int)(get("SET_SELECT_PET_ID"));
        SaleGetGold(iID);
        PetLogicData logic = DataCenter.GetData("PET_DATA") as PetLogicData;

        logic.RemoveItemData(iID);
		if(GameCommon.bIsWindowOpen ("PVP_PET_BAG_WINDOW"))
        	DataCenter.SetData("PVP_PET_BAG_WINDOW", "TEAM_ADJUST_UNLOAD", true);
		else if(GameCommon.bIsWindowOpen ("PVP_FOUR_VS_FOUR_PET_BAG_WINDOW"))
			DataCenter.SetData("PVP_FOUR_VS_FOUR_PET_BAG_WINDOW", "TEAM_ADJUST_UNLOAD", true);

        GameCommon.PlaySound("Sound/new/key/sell", GameCommon.GetMainCamera().transform.position);

        Close();
    }

	public override void SetBtn()
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;

		Transform btnTrans = mGameObjUI.transform.Find("BG/pet_info_group/pvp_pet_info_single_ok_button");
		UILabel OkBtnLabel1 = btnTrans.Find("label").GetComponent<UILabel>();
		UILabel OkBtnLabel2 = btnTrans.Find("label/label").GetComponent<UILabel>();
		
		string strLabel = "";
		if((DataCenter.GetData ("PVP_PET_BAG_WINDOW") as tWindow).IsOpen ())
		{
			if(petLogicData.IsPVPUsePet(mPetData.itemId))
			{
				strLabel = "下阵";
			}
			else
			{
				strLabel = "上阵";
			}
		}
		else if((DataCenter.GetData ("PVP_ATTR_READY_PET_BAG") as tWindow).IsOpen ())
		{
			if(petLogicData.InAttributePVPTeam(mPetData))
			{
				strLabel = "下阵";
			}
			else
			{
				strLabel = "上阵";
			}
		}
		else if((DataCenter.GetData ("PVP_FOUR_VS_FOUR_PET_BAG_WINDOW") as tWindow).IsOpen ())
		{
			if(petLogicData.IsFourVsFourPVPUsePet(mPetData.itemId))
			{
				strLabel = "下阵";
			}
			else
			{
				strLabel = "上阵";
			}
		}

		OkBtnLabel1.text = strLabel;
		OkBtnLabel2.text = strLabel;
	}
}
