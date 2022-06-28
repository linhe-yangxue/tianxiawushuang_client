using UnityEngine;
using System.Collections;
using DataTable;
using Logic;

public class PetEvolutionUI : MonoBehaviour {

	public GameObject mChooseStone;
	public GameObject mChoosePet;

	public GameObject mUnSelPetGroup;
	public GameObject mUnSelStoneGroup;
	public GameObject mStrengthenGroup;
	public GameObject mEvolutionGroup;

    public GameObject mStrengthenButton;
    public GameObject mEvolutionButton;
    public GameObject mEvolutionBtnDisabled;

	public UILabel mNeedCoinNumLabel;
	public UILabel mStrengthenLevelLabel;
	public UILabel mStoneNumLabel;

	public static float mfRemainEffectTime = 0;
	public static float mfMaxEffectTime = 1.3f;
	
	// Use this for initialization
	void Start () {
		
		Init();
	}
	
	// Update is called once per frame
	void Update () {	
		if(mfRemainEffectTime > 0)
		{
			mfRemainEffectTime -= Time.deltaTime;
			
			if(mfRemainEffectTime <= 0)
			{
				DataCenter.SetData("PetEvolutionWindow", "EVOLUTION_PET_OK", true);
			}
		}
	}
	
	void Init()
	{
		PetEvolutionWindow petEvolutionWindow = DataCenter.GetData("PetEvolutionWindow") as PetEvolutionWindow;

		petEvolutionWindow.mChooseStone				= 	  mChooseStone;		
		petEvolutionWindow.mChoosePet				=	  mChoosePet;

		petEvolutionWindow.mUnSelPetGroup			= 	  mUnSelPetGroup;		
		petEvolutionWindow.mUnSelStoneGroup			=	  mUnSelStoneGroup;
		petEvolutionWindow.mStrengthenGroup			=	  mStrengthenGroup;
		petEvolutionWindow.mEvolutionGroup			=	  mEvolutionGroup;
		
		petEvolutionWindow.mStrengthenButton		=	  mStrengthenButton;
		petEvolutionWindow.mEvolutionButton			=	  mEvolutionButton;
		petEvolutionWindow.mEvolutionBtnDisabled	=	  mEvolutionBtnDisabled;
			
		petEvolutionWindow.mNeedCoinNumLabel		=	  mNeedCoinNumLabel;

		petEvolutionWindow.mStrengthenLevelLabel	=	  mStrengthenLevelLabel;
		petEvolutionWindow.mStoneNumLabel			=	  mStoneNumLabel;

		petEvolutionWindow.set ("UI_INIT", true);
//		petEvolutionWindow.set ("SET_EVOLUTION_STATE", (int)PetEvolutionWindow.Evolution_State.Clear);
		petEvolutionWindow.set ("CLOSE", true);

        if (GameCommon.bIsLogicDataExist("SELECT_EVOLUTION_PET_ID"))
        {
            int iSelUpgradePetID = (int)(DataCenter.Get("SELECT_EVOLUTION_PET_ID"));
            if (iSelUpgradePetID > 0)
            {
                DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution);
            }
        }
	}
}

public class PetEvolutionWindow : tWindow{

	public GameObject mChooseStone;
	public GameObject mChoosePet;

	public GameObject mUnSelPetGroup;
	public GameObject mUnSelStoneGroup;
	public GameObject mStrengthenGroup;
	public GameObject mEvolutionGroup;

	public GameObject mStrengthenButton;
	public GameObject mEvolutionButton;
	public GameObject mEvolutionBtnDisabled;

	public UILabel mNeedCoinNumLabel;

	public UILabel mStrengthenLevelLabel;
	public UILabel mStoneNumLabel;

	public int mEvolutionState;
	public PetData mPetData = null;
	public bool mbIsEvolutionSuccess = false;
	public bool mbIsUIInit = false;
	public int miNeedCoin = 0;

	const int mMaxStrengthenLevel = 5;

	public enum Evolution_State
	{
		Clear,
		SelEvolutionPet,
		SelStone,
		Evolution
	}

	public override void Init()
	{
        mGameObjUI = GameCommon.FindUI("PetEvolutionWindow");

//		mChooseStone = GameCommon.FindUI("ChooseStone");
//		mChoosePet = GameCommon.FindUI("ChooseEvolutionPet");

//		Close();
	}

	public override void Open (object param)
	{
		base.Open (param);
        DataCenter.CloseWindow("PET_SKILL_WINDOW");
		DataCenter.CloseWindow("PET_DECOMPOSE_WINDOW");
        //DataCenter.OpenWindow("BAG_INFO_WINDOW");
		//DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.Bag_Pet_Window_ButtonTitle);
		if(mbIsUIInit)
			SetEvolutionState((int)Evolution_State.Clear);
		DataCenter.SetData("BAG_INFO_WINDOW", "CLEAR_STONE_SELECT_STATE", true);
		DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_PET_ICONS", true);
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "SET_EVOLUTION_STATE")
		{
			SetEvolutionState((int)objVal);
		}
		else if(keyIndex == "EVOLUTION_PET_OK")
		{
			GameObject black = mGameObjUI.transform.parent.Find("black").gameObject;
			GameObject StrengthenEffect = GameCommon.FindObject(mChooseStone, "ec_ui_strengthen-002");
			GameObject effect = GameCommon.FindObject(mChooseStone, "effect");
			if(StrengthenEffect != null && effect != null)
			{
				black.SetActive(false);
				StrengthenEffect.SetActive(false);
				effect.SetActive(true);
			}

			if(StrengthenCondition())
				EvolutionPetOK((bool)get ("STRENGTHEN_OR_EVOLUTION_PET_PLAY_EFFECT"));
		}
		else if(keyIndex == "SEND_SERVER_PET_STRENGTHEN_OR_EVOLUTION")
		{
			SendServerPetStrengthenOrEvolution();
		}
		else if(keyIndex == "STRENGTHEN_OR_EVOLUTION_PET_PLAY_EFFECT")
		{
			PlayEffect();
		}
		else if(keyIndex == "SELECT_EVOLUTION_PET")
		{			
			SetEvolutionPetByID((int)objVal);
		}
		else if(keyIndex == "SET_SELECT_STONE_BY_INDEX")
		{
			SetSelStoneByIndex((int)objVal);

			SetEvolutionState((int)Evolution_State.SelStone);
		}
		else if(keyIndex == "UI_INIT")
		{
			mbIsUIInit = true;
		}
		else if(keyIndex == "UPDATE_UI")
		{
			UpdateUI();
		}
	}

	//------------------------------------------------------------------------
	public void PlayEffect()
	{
		GameObject black = mGameObjUI.transform.parent.Find("black").gameObject;
		GameObject upgradeEffect = GameCommon.FindObject(mChooseStone, "ec_ui_strengthen-002");
		GameObject effect = GameCommon.FindObject(mChooseStone, "effect");
		if(upgradeEffect != null && effect != null)
		{
			black.SetActive(true);
			upgradeEffect.SetActive(true);
			effect.SetActive(false);
			PetEvolutionUI.mfRemainEffectTime = PetEvolutionUI.mfMaxEffectTime;
		}
	}

	public void SetEvolutionState(int state)
	{
		mEvolutionState = state;

		UpdateUI();
	}

	public void UpdateUI()
	{
		if(mEvolutionState == (int)Evolution_State.Clear)
		{
			ClearChoosePetUI();
			ClearChooseStoneUI();

			DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.Bag_Pet_Window_ButtonTitle);

			mUnSelPetGroup.SetActive(true);
			mUnSelStoneGroup.SetActive(false);
			mStrengthenGroup.SetActive(false);
			mEvolutionGroup.SetActive(false);

			mStoneNumLabel.text = "";
		}
		else if(mEvolutionState == (int)Evolution_State.SelEvolutionPet)
		{
			ClearChooseStoneUI();

			DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.Bag_Stone_Window_ButtonTitle);
			DataCenter.SetData("BAG_INFO_WINDOW", "CLEAR_STONE_SELECT_STATE", true);

			mUnSelPetGroup.SetActive(false);
			mUnSelStoneGroup.SetActive(true);
			mEvolutionGroup.SetActive(false);

			mStrengthenGroup.SetActive(true);
			mStrengthenGroup.transform.Find("Label1").gameObject.SetActive(false);
			mStrengthenGroup.transform.Find("Label2").gameObject.SetActive(true);
			mStrengthenGroup.transform.Find("Label3").gameObject.SetActive(false);
			mStrengthenGroup.transform.Find("Label2/fail_point_num_slider/add_label").gameObject.SetActive(false);

			mStoneNumLabel.text = "";
			UpdateCurFailPointUI();
		}
		else if(mEvolutionState == (int)Evolution_State.SelStone)
		{
			mStoneNumLabel.text = "1";
			mUnSelPetGroup.SetActive(false);
			mUnSelStoneGroup.SetActive(false);
			mEvolutionGroup.SetActive(false);

			mStrengthenGroup.SetActive(true);
			mStrengthenGroup.transform.Find("Label1").gameObject.SetActive(true);
			mStrengthenGroup.transform.Find("Label2").gameObject.SetActive(true);
			mStrengthenGroup.transform.Find("Label3").gameObject.SetActive(true);
			mStrengthenGroup.transform.Find("Label2/fail_point_num_slider/add_label").gameObject.SetActive(true);

			UpdateenProbabilityText();
			UpdateCurFailPointUI();
			UpdateAimFailPointUI();
		}
		else if(mEvolutionState == (int)Evolution_State.Evolution)
		{
			SetChooseStoneQianghua();

			int iStarLevel = mPetData.starLevel;
			int iCount = TableCommon.GetNumberFromEvolutionConsumeConfig(iStarLevel, "NEED_GEM_NUM");

			mStoneNumLabel.text = iCount.ToString();
			mUnSelPetGroup.SetActive(false);
			mUnSelStoneGroup.SetActive(false);
			mStrengthenGroup.SetActive(false);
			mEvolutionGroup.SetActive(true);

			DataCenter.SetData("BAG_INFO_WINDOW", "SET_ALL_STONE_DISABLED", true);
		}

		SetEvolutionButton();
	}

	public void UpdateenProbabilityText()
	{
		if(mPetData != null)
		{
			int iStarLevel = mPetData.starLevel;
			int iElementType = TableCommon.GetNumberFromActiveCongfig(mPetData.tid, "ELEMENT_INDEX");

			int iGemIndex = (int)get ("INDEX");
			int iGemElementType = iGemIndex/3;

			int iBaseProbability = GetBaseProbability();
			int iAddProbability = 0;
			UILabel baseNumLabel = GameCommon.FindComponent<UILabel>(mStrengthenGroup, "base_num_label");
			UILabel totalNumLabel = GameCommon.FindComponent<UILabel>(mStrengthenGroup, "total_num_label");
//			UILabel failPointNumLabel = GameCommon.FindComponent<UILabel>(mStrengthenGroup, "fail_point_num_label");

			GameObject group1 = GameCommon.FindObject(mStrengthenGroup, "group1");
			GameObject group2 = GameCommon.FindObject(mStrengthenGroup, "group2");
			if(baseNumLabel != null)
			{
				baseNumLabel.text = iBaseProbability.ToString() + "%";
			}

			Transform labelTrans = mStrengthenGroup.transform.Find("Label1");
			if(iElementType == iGemElementType)
			{
				labelTrans.localPosition = new Vector3(-177.0f, labelTrans.localPosition.y, labelTrans.localPosition.z);
				group1.SetActive(false);
				group2.SetActive(true);
				iAddProbability = TableCommon.GetNumberFromStrengthenConsumeConfig(iStarLevel, "ADD_PROBABILITY");
				UILabel addNumLabel = GameCommon.FindComponent<UILabel>(mStrengthenGroup, "add_num_label");
				if(addNumLabel != null)
				{
					addNumLabel.text = iAddProbability.ToString() + "%";
				}
			}
			else
			{
				labelTrans.localPosition = new Vector3(-82.6f, labelTrans.localPosition.y, labelTrans.localPosition.z);
				group1.SetActive(true);
				group2.SetActive(false);
			}

			int iTotalProbability = iBaseProbability + iAddProbability;

			iTotalProbability = iTotalProbability > 100 ? 100 : iTotalProbability;
			if(totalNumLabel != null)
			{
				totalNumLabel.text = iTotalProbability.ToString() + "%";
			}
		}
	}

	public void UpdateCurFailPointUI()
	{
		SetPercent(mPetData.mFailPoint/100.0f);

		//			if(failPointNumLabel != null)
		//			{
		//				failPointNumLabel.text = mPetData.mFailPoint.ToString();
		//			}
	}

	public void UpdateAimFailPointUI()
	{
		GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
		GemData gemData = gemLogicData.GetGemDataByIndex(get ("INDEX"));
		if(gemData != null)
		{
			string strName = "FAIL_POINT_" + mPetData.starLevel.ToString();
			int iAddFailPoint = TableCommon.GetNumberFromStoneTypeIconConfig(gemData.mType, strName); 
			UILabel addLabel = mStrengthenGroup.transform.Find("Label2/fail_point_num_slider/add_label").GetComponent<UILabel>();
			addLabel.text = "(+" + iAddFailPoint.ToString() + "%)";
		}
	}

	public int GetBaseProbability()
	{
		int iStarLevel = mPetData.starLevel;
		int iStrengthenLevel = mPetData.strengthenLevel;
		int iElementType = TableCommon.GetNumberFromActiveCongfig(mPetData.tid, "ELEMENT_INDEX");
		
		int iGemIndex = (int)get ("INDEX");
		int iGemSizeType = iGemIndex%3;
		int iGemElementType = iGemIndex/3;
		string strSize = GetSizeString(iGemSizeType);
		
		int iBaseProbability = TableCommon.GetNumberFromStrengthenConsumeConfig(iStarLevel, strSize + "_GEM_PROBABILITY_" + (mPetData.strengthenLevel + 1).ToString());

		return iBaseProbability;
	}

	public string GetSizeString(int iGemSizeType)
	{
		string strSize = "";
		switch(iGemSizeType)
		{
		case 0:
			strSize = "SMALL";
			break;
		case 1:
			strSize = "MIDDLE";
			break;
		case 2:
			strSize = "LARGE";
			break;
		}

		return strSize;
	}

	public void EvolutionPetOK(bool bIsSuccess)
	{
        BagInfoWindow.isNeedRefresh = true;
		
        // qianghua	
		UpdateStoneNum();

		DeductCoin();

		if(mEvolutionState == (int)Evolution_State.SelStone)
		{
			UpStrengthenLevel(bIsSuccess);
		}
		else if(mEvolutionState == (int)Evolution_State.Evolution)
		{
			Evolution(bIsSuccess);
		}
	}

	public void Evolution(bool bIsSuccess)
	{
		if (bIsSuccess)
		{
			// remove pet from team
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			if(petLogicData.IsPetUsed(mPetData.itemId))
			{
				int iPos = petLogicData.GetPosInCurTeam(mPetData.itemId);
				petLogicData.RemoveDBIDByPos(iPos);
			}

			// get new pet
			int iNewModelIndex = get("NEW_PET_MODEL_INDEX");
			
			mPetData.level = 1;
			mPetData.starLevel = TableCommon.GetNumberFromActiveCongfig(iNewModelIndex, "STAR_LEVEL");
			mPetData.exp = 0;
			mPetData.tid = iNewModelIndex;
			mPetData.strengthenLevel = 0;

			DataCenter.SetData("evolution_gain_pet_info_window", "SET_SELECT_PET_BY_DBID", mPetData.itemId);
			DataCenter.SetData("evolution_gain_pet_info_window", "OPEN", true);
			
			// task
			//tLogicData taskNeedData = DataCenter.GetData("TASK_NEED_DATA");
			//
			//taskNeedData.set("NUM", 1);
			//taskNeedData.set("ITEM_ID", iNewModelIndex);
			//TaskSystemMgr.Self.CheckFinishAllTaskSubentryByTaskType(TASK_TYPE.Pet_Evolution_Num);
			
			mbIsEvolutionSuccess = true;

			SetEvolutionState((int)Evolution_State.Clear);
			DataCenter.SetData("BAG_INFO_WINDOW", "CLEAR_STONE_SELECT_STATE", true);
			DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_PET_ICONS", true);
		}
	}
	public void SendServerPetStrengthenOrEvolution()
	{
		if(mEvolutionState == (int)Evolution_State.SelStone)
		{
			if(StrengthenCondition())
				SendServerPetStrengthen();
		}
		else if(mEvolutionState == (int)Evolution_State.Evolution)
		{
			SendServerPetEvolution();
		}
	}

	public bool StrengthenCondition()
	{
		if(mEvolutionState == (int)Evolution_State.SelStone)
		{
			if( GetBaseProbability() <= 0)
			{
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_STRENGTHEN_NO_CHANCE);
				return false;
			}

			if(mPetData.strengthenLevel >= mPetData.mMaxStrengthenLevel)
			{
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MAX_ENHANCE);
				return false;
			}

		}
		else if(mEvolutionState == (int)Evolution_State.Evolution)
		{
			if(mPetData.level < mPetData.mMaxLevelNum)
			{
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_EVOLUTION_LEVEL);
				return false;
			}
		}

		return true;
	}
	
	public void SendServerPetStrengthen()
	{
		GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
		GemData gemData = gemLogicData.GetGemDataByIndex(get ("INDEX"));
		if(gemData != null)
		{
			tEvent quest = Net.StartEvent("CS_RequestPetStrengthen");		
			quest.set("PET_ID", mPetData.itemId);
			quest.set("GEM_INDEX", gemData.mType);
			quest.DoEvent();
		}

	}

	public void SendServerPetEvolution()
	{
		if(mPetData.level < mPetData.mMaxLevelNum)
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_EVOLUTION_LEVEL);

			return;
		}
		if(mPetData.starLevel >= 5)
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_EVOLUTION_STAR_LEVEL);
			
			return;
		}

		RoleLogicData logicData = RoleLogicData.Self;
		if(GetNeedEvolutionCoinNum() > logicData.gold)
		{
            DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
			//DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_OPERATE_NEED_COIN);

			return;
		}

		tEvent quest = Net.StartEvent("CS_RequestPetEvolution");		
		quest.set("PET_ID", mPetData.itemId);
		quest.DoEvent();
	}

	public void UpStrengthenLevel(bool bIsSuccess)
	{
		if(mPetData != null)
		{
			if(bIsSuccess)
			{
				mPetData.mFailPoint = 0;
				if(mEvolutionState == (int)Evolution_State.SelStone)
				{
					DataCenter.SetData("UpGradeAndStrengthenResultWindow", "AIM_STRENGTHEN_LEVEL", mPetData.strengthenLevel + 1);

					mPetData.strengthenLevel += 1;

					if (mPetData.strengthenLevel >= mPetData.mMaxStrengthenLevel)
                    {
						mPetData.strengthenLevel = mPetData.mMaxStrengthenLevel;
                    }

					UpdateenLevelAndState();

					DataCenter.SetData("UpGradeAndStrengthenResultWindow", "SHOW_STRENGTHEN_SUCCESS", mPetData.itemId);

                    // task
                    //tLogicData taskNeedData = DataCenter.GetData("TASK_NEED_DATA");
                    //taskNeedData.set("NUM", 1);
                    //taskNeedData.set("PET_STRENGTHEN_LEVEL", mPetData.mStrengthenLevel);
                    //taskNeedData.set("PET_STRENGTHEN_RESULT", bIsSuccess);
                    //TaskSystemMgr.Self.CheckFinishAllTaskSubentryByTaskType(TASK_TYPE.Pet_Strengthen_Num);
                }                
			}
			else
			{
				if(mEvolutionState == (int)Evolution_State.SelStone)
				{
					GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
					GemData gemData = gemLogicData.GetGemDataByIndex(get ("INDEX"));
					if(gemData != null)
					{
						string strName = "FAIL_POINT_" + mPetData.starLevel.ToString();
						int iStrengthenFailPoint = TableCommon.GetNumberFromStoneTypeIconConfig(gemData.mType, strName);
						mPetData.mFailPoint += iStrengthenFailPoint;

						DataCenter.SetData("UpGradeAndStrengthenResultWindow", "STRENGTHEN_FAILED_POINT", iStrengthenFailPoint);
						DataCenter.SetData("UpGradeAndStrengthenResultWindow", "SHOW_STRENGTHEN_FAILED", mPetData.itemId);
						UpdateenLevelAndState();
					}
				}
			}
		}
	}

	public void UpdateStoneNum()
	{
		int iIndex = get("INDEX");
		
		GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
		GemData gemData = gemLogicData.GetGemDataByIndex(iIndex);
		
		if(gemData != null)
		{
			int iDCount = 1;
			if(mEvolutionState == (int)Evolution_State.Evolution)
			{
				iDCount = TableCommon.GetNumberFromEvolutionConsumeConfig(mPetData.starLevel, "NEED_GEM_NUM");
			}

			gemData.mCount -= iDCount;
			if(gemData.mCount < 0)
				gemData.mCount = 0;
		}
		else
		{
			Logic.EventCenter.Log(LOG_LEVEL.ERROR, "gemData is null");
		}

		DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_STONE_CION", iIndex);

		if(mEvolutionState == (int) Evolution_State.Evolution)
		{
			int iGemType = TableCommon.GetNumberFromEvolutionConsumeConfig(mPetData.starLevel, "NEED_GEM_TYPE");
			iIndex = TableCommon.GetNumberFromActiveCongfig(mPetData.tid, "ELEMENT_INDEX")*3 + iGemType;
			DataCenter.SetData("BAG_INFO_WINDOW", "ELEMENT_INDEX", iIndex);
			DataCenter.SetData("BAG_INFO_WINDOW", "SET_ALL_STONE_DISABLED", true);
		}
	}
	
	public void SetSelStoneByIndex(int iIndex)
	{
		GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
		GemData gemData = gemLogicData.GetGemDataByIndex(iIndex);
		if(gemData != null)
		{
			SetStoneBtnSpriteIcon(mChooseStone, gemData);
			set ("INDEX", iIndex);
		}
	}

	public void SetEvolutionPetByID(int iID)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mPetData = petLogicData.GetPetDataByItemId(iID);
		if(mPetData != null)
		{
			SetPetBtnSpriteIcon(mChoosePet, mPetData);

			UIButtonEvent buttonEvent = mChoosePet.GetComponent<UIButtonEvent>();
			buttonEvent.mData.set("ID", mPetData.itemId);

			UpdateenLevelAndState();

			if(mEvolutionState == (int) Evolution_State.Evolution)
			{
				int iGemType = TableCommon.GetNumberFromEvolutionConsumeConfig(mPetData.starLevel, "NEED_GEM_TYPE");
				int iIndex = TableCommon.GetNumberFromActiveCongfig(mPetData.tid, "ELEMENT_INDEX")*3 + iGemType;
				DataCenter.SetData("BAG_INFO_WINDOW", "SET_STONE_SELECT", iIndex);
				DataCenter.SetData("BAG_INFO_WINDOW", "SET_ALL_STONE_DISABLED", true);
			}
		}
	}
	public void UpdateenLevelAndState()
	{
		SetStrengthenLevelLabel();
		
		if(mPetData.strengthenLevel >= mMaxStrengthenLevel/* && mPetData.mLevel >= mPetData.mMaxLevelNum*/)
		{
			// jinhua
			SetEvolutionState((int)Evolution_State.Evolution);
		}
		else
		{			
			// qianghua
			if((int)get ("INDEX") >= 0)
			{
				SetEvolutionState((int)Evolution_State.SelStone);
			}
			else
			{
				SetEvolutionState((int)Evolution_State.SelEvolutionPet);
			}
		}
	}

	public void SetChooseStoneQianghua()
	{
		DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.Bag_Stone_Window_ButtonTitle);
		DataCenter.SetData("BAG_INFO_WINDOW", "CLEAR_STONE_SELECT_STATE", true);

		int iGemType = TableCommon.GetNumberFromEvolutionConsumeConfig(mPetData.starLevel, "NEED_GEM_TYPE");
		int iIndex = TableCommon.GetNumberFromActiveCongfig(mPetData.tid, "ELEMENT_INDEX")*3 + iGemType;
		DataCenter.SetData("BAG_INFO_WINDOW", "SET_STONE_SELECT", iIndex);
		
		GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
		GemData gemData = gemLogicData.GetGemDataByIndex(iIndex);
		
		if(gemData != null)
		{
			SetSelStoneByIndex(iIndex);
		}
	}
	public void ClearChoosePetUI()
	{
		SetPetBtnSpriteIcon(mChoosePet, null);
		mPetData = null;

		UIButtonEvent buttonEvent = mChoosePet.GetComponent<UIButtonEvent>();
		buttonEvent.mData.set("ID", -1);

		SetStrengthenLevelLabel();
	}
	
	public void ClearChooseStoneUI()
	{
		if(mChooseStone == null)
			return;
		SetStoneBtnSpriteIcon(mChooseStone, null);

		set ("INDEX", -1);
	}

	public int GetNeedEvolutionCoinNum()
	{
		if(mPetData == null)
			return 0;
		
		int iIndex = mPetData.starLevel;
		int iNeedCoin = 0;

		if(mEvolutionState == (int)Evolution_State.SelStone)
		{
			iNeedCoin = TableCommon.GetNumberFromStrengthenConsumeConfig(iIndex, "NEED_COIN_" + (mPetData.strengthenLevel + 1).ToString());
		}
		else if(mEvolutionState == (int)Evolution_State.Evolution)
		{
			iNeedCoin = TableCommon.GetNumberFromEvolutionConsumeConfig(iIndex, "NEED_COIN_NUM");
		}
		return iNeedCoin;
	}
	
	public void DeductCoin()
	{
		RoleLogicData logicData = RoleLogicData.Self;
		if(miNeedCoin <= logicData.gold)
		{
			GameCommon.RoleChangeGold((-1)*miNeedCoin);
		}
	}

	public void SetEvolutionButton()
	{
		if(mEvolutionState == (int)Evolution_State.Clear 
		   || mEvolutionState == (int)Evolution_State.SelEvolutionPet)
		{
			ClearEvolutionButton();
			return;
		}

		miNeedCoin = GetNeedEvolutionCoinNum();

		RoleLogicData logicData = RoleLogicData.Self;

		int iIndex = get ("INDEX");
		GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
		GemData gemData = gemLogicData.GetGemDataByIndex(iIndex);

		bool bIsActive = true;
		if(mEvolutionState == (int)Evolution_State.SelStone)
		{
			if((miNeedCoin <= logicData.gold) && gemData != null && gemData.mCount > 0)
			{
				bIsActive = false;
			}

			mStrengthenButton.SetActive(true);
			mEvolutionButton.SetActive(false);
		}
		else if(mEvolutionState == (int)Evolution_State.Evolution)
		{
			int iStarLevel = mPetData.starLevel;
			int iCount = TableCommon.GetNumberFromEvolutionConsumeConfig(iStarLevel, "NEED_GEM_NUM");

			if((miNeedCoin <= logicData.gold)
			   && gemData != null && gemData.mCount >= iCount 
			   && mPetData != null && mPetData.level >= mPetData.mMaxLevelNum)
			{
				bIsActive = false;
			}

			mStrengthenButton.SetActive(false);
			mEvolutionButton.SetActive(true);
		}

		mEvolutionBtnDisabled.SetActive(bIsActive);

		mNeedCoinNumLabel.text = miNeedCoin.ToString();


		if(miNeedCoin > logicData.gold)
		{
			mNeedCoinNumLabel.text = "[ff0000]" + mNeedCoinNumLabel.text;
		}
	}

	public void ClearEvolutionButton()
	{
		mStrengthenButton.SetActive(true);
		mEvolutionButton.SetActive(false);

		if(mPetData != null && mPetData.strengthenLevel >= mMaxStrengthenLevel)
		{
			mStrengthenButton.SetActive(false);
			mEvolutionButton.SetActive(true);
		}

		mEvolutionBtnDisabled.SetActive(true);

		if(mNeedCoinNumLabel != null)
			mNeedCoinNumLabel.text = "0";
	}

	public void SetStrengthenLevelLabel()
	{
		if(mPetData != null && mPetData.strengthenLevel > 0)
			mStrengthenLevelLabel.text = "+" + mPetData.strengthenLevel.ToString();
		else
			mStrengthenLevelLabel.text = "";
	}

	public bool SetPetBtnSpriteIcon(GameObject obj, PetData petData)
	{
		if(obj == null)
			return false;

		int iModelIndex = -1;
		if(petData != null)
		{
			iModelIndex = petData.tid;
		}

		// set pet icon
		GameCommon.SetPetIcon(obj, iModelIndex);

		GameObject bg =  GameCommon.FindObject(obj, "Background");
		if(petData != null)
		{
			// set element icon
			//int iElementIndex = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "ELEMENT_INDEX");
            //GameCommon.SetElementIcon(obj, iElementIndex);

            // set level
            GameCommon.SetLevelLabel(obj, petData.level);

			// set star level
			GameCommon.SetStarLevelLabel(obj, petData.starLevel);

			// set strengthen level text
			GameCommon.SetStrengthenLevelLabel(obj, petData.strengthenLevel);
			
			bg.SetActive(true);
		}
		else
		{
			bg.SetActive(false);
		}

		SetChooseBtnEffect(obj, petData);
		
		return true;
	}
	
	public void SetChooseBtnEffect(GameObject obj, PetData petData)
	{
		if(obj == null)
			return;
		
		GameObject effect =  GameCommon.FindObject(obj, "effect");
		GameObject chooseStoneEffect = obj.transform.parent.Find("ChooseStone/effect").gameObject;
		if(effect == null || chooseStoneEffect == null)
			return;
		
		effect.SetActive(true);
		
		if(petData != null)
		{
			effect.SetActive(false);
			chooseStoneEffect.SetActive(true);
		}
		else
		{
			effect.SetActive(true);
			chooseStoneEffect.SetActive(false);
		}
	}

	public bool SetStoneBtnSpriteIcon(GameObject obj, GemData gemData)
	{
		if(obj == null)
			return false;
		
		UISprite sprite = GameCommon.FindObject(obj, "Background").GetComponent<UISprite>();
		if(sprite != null)
		{
			if(gemData != null)
			{
				string strAtlasName = TableCommon.GetStringFromStoneTypeIconConfig(gemData.mType, "STONE_ATLAS_NAME");
				string strSpriteName = TableCommon.GetStringFromStoneTypeIconConfig(gemData.mType, "STONE_SPRITE_NAME");

                UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
				sprite.atlas = tu;
				sprite.spriteName = strSpriteName;
				sprite.MakePixelPerfect();
			}
			else
			{
				sprite.atlas = null;
				sprite.spriteName = "";
				sprite.MakePixelPerfect();
			}
			
			return true;
		}
		return false;
	}

	public void SetPercent(float percentage)
	{
		if(mGameObjUI == null)
			return;
		
		UIProgressBar loadingBar = mStrengthenGroup.transform.Find("Label2/fail_point_num_slider").GetComponent<UIProgressBar>();
		loadingBar.value = percentage;

		UILabel curLabel = loadingBar.transform.Find("cur_label").GetComponent<UILabel>();
		curLabel.text = mPetData.mFailPoint.ToString() + "%";
	}

}
