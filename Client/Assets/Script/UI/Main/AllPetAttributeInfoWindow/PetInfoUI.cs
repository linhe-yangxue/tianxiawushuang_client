using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System;

public class PetInfoUI : MonoBehaviour {

	public GameObject mPetPlayCheckBox1;
	public GameObject mPetPlayCheckBox2;
	public GameObject mPetPlayCheckBox3;

	public GameObject mPetPlayBtn;

	public UILabel mPetStarLevelLabel;
	public UILabel mPetTitleLabel;
	public UILabel mPetNameLabel;

	public UILabel mLevel;
	public UILabel mExpPercentage;
	public UIProgressBar mPetExpBar;

	public UILabel mStrengthenLevelLabel;
	public UILabel mAttackLabel;
	public UILabel mMaxHPLabel;
	public UILabel mFightingLabel;
	public UILabel mAttributeLabel;
	public UILabel mAttackTypeLabel;

	public UISprite mElementIcon;

	public GameObject mSkillBtn1;
	public GameObject mSkillBtn2;
	public GameObject mSkillBtn3;

	public int mCurrentIndex;

	public GameObject mCard;
	public float mfCardScale = 1.0f;

	// Use this for initialization
	void Start () {

		mCurrentIndex = 1;

//		PetPlayCheckBoxData petPlayCheckBoxData1 = new PetPlayCheckBoxData();
//		petPlayCheckBoxData1.mPetPlayCheckBox = mPetPlayCheckBox1;
//		int usePos = 1;
//		petPlayCheckBoxData1.set("SET_USE_POS", usePos);
//		DataCenter.RegisterData("PetPlayCheckBoxData1", petPlayCheckBoxData1);
//
//		PetPlayCheckBoxData petPlayCheckBoxData2 = new PetPlayCheckBoxData();
//		petPlayCheckBoxData2.mPetPlayCheckBox = mPetPlayCheckBox2;
//		usePos = 2;
//		petPlayCheckBoxData2.set("SET_USE_POS", usePos);
//		DataCenter.RegisterData("PetPlayCheckBoxData2", petPlayCheckBoxData2);
//
//		PetPlayCheckBoxData petPlayCheckBoxData3 = new PetPlayCheckBoxData();
//		petPlayCheckBoxData3.mPetPlayCheckBox = mPetPlayCheckBox3;
//		usePos = 3;
//		petPlayCheckBoxData3.set("SET_USE_POS", usePos);
//		DataCenter.RegisterData("PetPlayCheckBoxData3", petPlayCheckBoxData3);

		InitCard();

		InitPetInfo();
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

	void InitPetInfo()
	{
		PetInfoWindow petInfoData = DataCenter.GetData("PetInfoWindow") as PetInfoWindow;
		
		petInfoData.mPetStarLevelLabel 	= 	  mPetStarLevelLabel;
		petInfoData.mPetTitleLabel 		= 	  mPetTitleLabel;
		petInfoData.mPetNameLabel 		= 	  mPetNameLabel;
		
		petInfoData.mLevel 				=	  mLevel;
		petInfoData.mExpPercentage 		=	  mExpPercentage;
		petInfoData.mPetExpBar 			=	  mPetExpBar;
		
		petInfoData.mStrengthenLevelLabel	=	  mStrengthenLevelLabel;
		petInfoData.mAttackLabel 		=	  mAttackLabel;
		petInfoData.mMaxHPLabel 		=	  mMaxHPLabel;
		petInfoData.mFightingLabel      =  mFightingLabel;
		petInfoData.mAttributeLabel      =  mAttributeLabel;
		petInfoData.mAttackTypeLabel      =  mAttackTypeLabel;

		petInfoData.mCard				=	  mCard;

		petInfoData.mElementIcon 		=	  mElementIcon;
		
		petInfoData.mSkillBtn1 			=	  mSkillBtn1;
		petInfoData.mSkillBtn2 			=	  mSkillBtn2;
		petInfoData.mSkillBtn3 			=	  mSkillBtn3;

		petInfoData.mPetPlayBtn    		= 	  mPetPlayBtn;

		petInfoData.mbIsInit			= 	  true;

		petInfoData.set("SET_SELECT_PET", mCurrentIndex);
		petInfoData.set("SHOW_PET_FLAG", false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnDestroy()
	{
//		DataCenter.Remove("PetPlayCheckBoxData1");
//		DataCenter.Remove("PetPlayCheckBoxData2");
//		DataCenter.Remove("PetPlayCheckBoxData3");
	}
}

public class PetPlayCheckBoxData : tLogicData
{
	public GameObject mPetPlayCheckBox;
	public int miID = -1;
	public override void onChange(string keyIndex, object objVal)
	{
		if(keyIndex == "SET_USE_POS")
		{
			string str = objVal.GetType().ToString();

			SetPetByUsePos((int)objVal);
		}
		if(keyIndex == "SET_SELECT")
		{
			SetSelect();
		}
	}

	public void SetPetByUsePos(int iUsePos)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;

        PetInfoWindow petInfoData = DataCenter.GetData("PetInfoWindow") as PetInfoWindow;
        PetData petData = petLogicData.GetPetDataByPos(petInfoData.miCurTeam, iUsePos);
		int iModelIndex = -1;

		SetSpriteIcon(petData);
	}

	void SetSpriteIcon(PetData petData)
	{
		// set pet icon
        int iModelIndex = -1;
        if (petData != null)
        {
            iModelIndex = petData.tid;
        }

        GameCommon.SetPetIcon(mPetPlayCheckBox, iModelIndex);

		GameObject bg =  GameCommon.FindObject(mPetPlayCheckBox, "BG");
		if(petData != null)
		{
			// set element icon
			int iElementIndex = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "ELEMENT_INDEX");
            GameCommon.SetElementIcon(mPetPlayCheckBox, iElementIndex);
			
			// set level
            GameCommon.SetLevelLabel(mPetPlayCheckBox, petData.level);

			// set star level
            GameCommon.SetStarLevelLabel(mPetPlayCheckBox, petData.starLevel);

			// set strengthen level text
			GameCommon.SetStrengthenLevelLabel(mPetPlayCheckBox, petData.strengthenLevel);

			bg.SetActive(true);
		}
		else
		{
			bg.SetActive(false);
		}
	}

	public void SetSelect()
	{
        GameCommon.ToggleTrue(mPetPlayCheckBox);
	}
}


public class PetInfoWindow : PetInfoSingleBaseWindow
{
	public GameObject mPetPlayBtn;
	public GameObject mPetInfoGroup;
	public GameObject mPetInfoEmptyBG;
	public bool mbIsInit = false;

    public int miCurTeam = 0;

	public PetInfoTeamInfoUI mTeamInfoUI;

	public override void Init()
	{
        mGameObjUI = GameCommon.FindUI("PetInfoWindow");
		mPetInfoGroup = GameCommon.FindObject(mGameObjUI, "pet_info");
		mPetInfoEmptyBG = GameCommon.FindObject(mGameObjUI, "pet_info_empty_bg");

		mTeamInfoUI = new PetInfoTeamInfoUI();
		mTeamInfoUI.Init(mGameObjUI, "PetInfoWindow");
		Close();
	}

	public override void Open (object param)
	{
		base.Open (param);
		Refresh(param);
        //DataCenter.SetData("PetUpgradeWindow", "CLOSE", true);
        //DataCenter.SetData("PetEvolutionWindow", "CLOSE", true);
        DataCenter.CloseWindow("PET_SKILL_WINDOW");
		DataCenter.CloseWindow("PET_DECOMPOSE_WINDOW");

		//DataCenter.OpenWindow("BAG_INFO_WINDOW");
		DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.Bag_Pet_Window_SpriteTitle);

		DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_PET_ICONS", true);

        GuideManager.Notify(GuideIndex.EnterTeamWindow);
        GuideManager.Notify(GuideIndex.EnterTeamWindow2);
        GuideManager.Notify(GuideIndex.EnterTeamWindow3);
	}

	public void SetAutoJoinButtonState()
	{
		return;
		if(mGameObjUI != null)
		{
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			bool isFull = true;
			for(int i = 1; i <= 3; i++)
			{
				PetData pet = petLogicData.GetPetDataByPos(mTeamInfoUI.mWhichTeam, i);
				if(pet == null)
				{
					isFull = false;
					break;
				}
			}

			GameObject autoJoinObj = GameCommon.FindObject(mGameObjUI, "auto_join_button");
			if(autoJoinObj != null)
			{
				UIImageButton autoJoinBtn = autoJoinObj.GetComponent<UIImageButton>();
				if(autoJoinBtn != null)
					autoJoinBtn.isEnabled = (isFull != true);
			}
		}


	}

    public override bool Refresh(object param)
	{
		if(mbIsInit)
		{
//			int usePos = 1;
//			DataCenter.SetData("PetPlayCheckBoxData1", "SET_USE_POS", usePos);
//			usePos = 2;
//			DataCenter.SetData("PetPlayCheckBoxData2", "SET_USE_POS", usePos);
//			usePos = 3;
//			DataCenter.SetData("PetPlayCheckBoxData3", "SET_USE_POS", usePos);

			SetPetByUsePos(1);
		}
		return true;
	}
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		if(keyIndex == "SET_SELECT_PET")
		{
			SetPetByUsePos((int)objVal);
		}
		else if(keyIndex == "SHOW_PET_FLAG")
		{
			mTeamInfoUI.ShowPetFlag((bool)objVal);
		}
		else if(keyIndex == "BACK_OR_FORWARD")
		{
			mTeamInfoUI.BackOrForward ((int)objVal);
		}
		else if(keyIndex == "TEAM_REFRESH")
		{
			SetPetByUsePos(1);

			DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_PET_ICONS", true);
		}
		else if(keyIndex == "SET_AUTO_JOIN")
		{
			SetAutoJoin();
		}
	}

	public override void RequestSalePet()
	{
		int iID = (int)(get("SET_SELECT_PET_ID"));
		tEvent tempQuest = Net.StartEvent("CS_RequestSalePet");
		CS_RequestSalePet quest = tempQuest as CS_RequestSalePet;
		quest.set("PET_ID", iID);
		quest.mAction = () => DataCenter.SetData("PetInfoWindow", "SALE_PET_RESULT", true);
		quest.DoEvent();
	}

	public void SetPetByUsePos(int iUsePos)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mPetData = petLogicData.GetPetDataByPos(mTeamInfoUI.mWhichTeam, iUsePos);
		if(mPetData != null)
		{
			mPetInfoGroup.SetActive(true);
			mPetInfoEmptyBG.SetActive(false);

			mPetStarLevelLabel.text = mPetData.starLevel.ToString();
			mPetTitleLabel.text = TableCommon.GetStringFromActiveCongfig(mPetData.tid, "NAME");
			mPetNameLabel.text = TableCommon.GetStringFromActiveCongfig(mPetData.tid, "NAME");
			
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

			GameCommon.SetCardInfo(mCard.name, mPetData.tid, mPetData.level, mPetData.strengthenLevel, false);

            DataCenter.SetData("PetInfoWindow", "SET_SELECT_PET_ID", mPetData.itemId);
		}
		else
		{
			mPetInfoGroup.SetActive(false);
			mPetInfoEmptyBG.SetActive(true);
		}

		mTeamInfoUI.Refresh(iUsePos);
		SetAutoJoinButtonState();
	}

	public bool SetAutoJoin()
	{
		DataCenter.SetData("BAG_INFO_WINDOW", "SET_AUTO_JOIN", 3);
		return true;
	}
}
