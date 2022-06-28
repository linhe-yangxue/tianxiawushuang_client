using UnityEngine;
using System.Collections;
using DataTable;
using System.Collections.Generic;
using Logic;
using System.Linq;

public class PetUpgradeUI : MonoBehaviour {

	public GameObject mChooseMaterial;
	public GameObject mChoosePet;
	public GameObject mSelPetGroup;
	public GameObject mUnSelPetGroup;
	public GameObject mUpgradeButton;
	public GameObject mUpgradeBtnDisabled;

	public UILabel mCurLevelLabel;
	public UILabel mAimLevelLabel;
	public UILabel mSelPetNumLabel;
	public UILabel mMaxLevelLabel;
	public UILabel mNeedCoinNumLabel;

	public UILabel mPetCurExpPercentage;
	public UIProgressBar mPetCurExpBar;
	public UILabel mPetAimExpPercentage;
	public UIProgressBar mPetAimExpBar;

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
				DataCenter.SetData("PetUpgradeWindow", "UPGRADE_PET_OK", true);
			}
		}
	}

	void Init()
	{
		PetUpgradeWindow petUpgradeWindow = DataCenter.GetData("PetUpgradeWindow") as PetUpgradeWindow;

		petUpgradeWindow.mChooseMaterial		= 	  mChooseMaterial;		
		petUpgradeWindow.mChoosePet				=	  mChoosePet;

		petUpgradeWindow.mSelPetGroup			= 	  mSelPetGroup;		
		petUpgradeWindow.mUnSelPetGroup			=	  mUnSelPetGroup;

		petUpgradeWindow.mUpgradeButton			=	  mUpgradeButton;
		petUpgradeWindow.mUpgradeBtnDisabled	= 	  mUpgradeBtnDisabled;

		petUpgradeWindow.mCurLevelLabel			=	  mCurLevelLabel;
		petUpgradeWindow.mAimLevelLabel			=	  mAimLevelLabel;

		petUpgradeWindow.mSelPetNumLabel		=	  mSelPetNumLabel;

		petUpgradeWindow.mMaxLevelLabel			=	  mMaxLevelLabel;

		petUpgradeWindow.mNeedCoinNumLabel		=	  mNeedCoinNumLabel;

		petUpgradeWindow.mPetCurExpPercentage 	= 	  mPetCurExpPercentage;
		petUpgradeWindow.mPetCurExpBar			=	  mPetCurExpBar;
		petUpgradeWindow.mPetAimExpPercentage 	=	  mPetAimExpPercentage;
		petUpgradeWindow.mPetAimExpBar			=	  mPetAimExpBar;

		petUpgradeWindow.set ("INIT_UI", true);
		petUpgradeWindow.set ("CLOSE", true);


        if (GameCommon.bIsLogicDataExist("SELECT_UPGRADE_PET_ID"))
        {
            int iSelUpgradePetID = (int)(DataCenter.Get("SELECT_UPGRADE_PET_ID"));
            if (iSelUpgradePetID > 0)
            {
                DataCenter.SetData("AllPetAttributeInfoWindow", "SHOW_WINDOW", ALL_PET_ATTRIBUTE_INFO_INDEX.PetUpgrade);
                DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_UPGRADE_PET_ICONS", iSelUpgradePetID);
            }
        }
	}
}

public class UpgradePetData
{
	public PetData mPetData;
	public int mIndex;
}

public class PetUpgradeWindow : tWindow{

	public GameObject mChooseMaterial;
	public GameObject mChoosePet;
	public GameObject mSelPetGroup;
	public GameObject mUnSelPetGroup;
	public GameObject mUpgradeButton;
	public GameObject mUpgradeBtnDisabled;
	
	public UILabel mCurLevelLabel;
	public UILabel mAimLevelLabel;
	public UILabel mSelPetNumLabel;
	public UILabel mMaxLevelLabel;
	public UILabel mNeedCoinNumLabel;

	public UILabel mPetCurExpPercentage;
	public UIProgressBar mPetCurExpBar;
	public UILabel mPetAimExpPercentage;
	public UIProgressBar mPetAimExpBar;

    private UIGridContainer mAddPetGrid;

	public PetData mPetData = null;
	public Dictionary<int, UpgradePetData> mDicSelPet = new Dictionary<int, UpgradePetData>();

	public int miAimExp = 0;
	public int miAimLevel = 1;
	public int miAimLevelBreak = 0;
	public int miNeedSyntheticCoin = 0;



	public override void Init()
	{
		mGameObjUI = GameCommon.FindUI("PetUpgradeWindow");

        mAddPetGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "add_pet_grid");
//		mChooseMaterial = GameCommon.FindUI("ChooseMaterial");
//		mChoosePet = GameCommon.FindUI("ChooseEvolutionPet");

		mDicSelPet.Clear();
	
//		Close();
	}

	public bool SetBtnSpriteIcon(GameObject obj, PetData petData)
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
			int iElementIndex = TableCommon.GetNumberFromActiveCongfig(iModelIndex, "ELEMENT_INDEX");
            GameCommon.SetElementIcon(obj, iElementIndex);
			
			// set level
            GameCommon.SetLevelLabel(obj, petData.level);

			// set star level
			GameCommon.SetStarLevelLabel(obj, petData.starLevel);

			// set strengthen level text
			GameCommon.SetStrengthenLevelLabel(obj, petData.strengthenLevel);

			if(bg != null)
				bg.SetActive(true);

            mAddPetGrid.MaxCount = mDicSelPet.Count - 1;
            mAddPetGrid.transform.localPosition = new Vector3(0 - mAddPetGrid.MaxCount * mAddPetGrid.CellWidth, 0, 0);
		}
		else
		{
			if(bg != null)
				bg.SetActive(false);

            mAddPetGrid.MaxCount = 0;
		}

		SetChooseBtnEffect(obj, petData);

		return true;
	}

    public void SetChooseBtnEffect(GameObject obj, PetData petData)
    {
        if (obj == null)
            return;

        if (obj.name == "ChoosePet")
        {
            GameObject effect = GameCommon.FindObject(obj, "effect");
            GameObject chooseMaterialEffect = obj.transform.parent.Find("ChooseMaterial/effect").gameObject;
            if (effect == null || chooseMaterialEffect == null)
                return;

            effect.SetActive(true);

            if (petData != null)
            {
                effect.SetActive(false);
                chooseMaterialEffect.SetActive(true);
            }
            else
            {
                effect.SetActive(true);
                chooseMaterialEffect.SetActive(false);
            }
        }
    }

	public override void Open (object param)
	{
		base.Open (param);
        DataCenter.CloseWindow("PET_SKILL_WINDOW");
		DataCenter.CloseWindow("PET_DECOMPOSE_WINDOW");

		//DataCenter.OpenWindow("BAG_INFO_WINDOW");
		DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.Bag_Pet_Window_SpriteTitle);

		DataCenter.SetData("PetUpgradeWindow", "REMOVE_UPGRADE_PET", true);
		DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_PET_ICONS", true);
	}
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);

		if(keyIndex == "SELECT_UPGRADE_PET")
		{			
			SetUpgradePetByID((int)objVal);
		}
		else if(keyIndex == "REMOVE_UPGRADE_PET")
		{			
			RemoveUpgradePet();
		}
		else if(keyIndex == "SET_SELECT_PET_BY_ID")
		{
			SetPetByID((int)objVal);
		}
		else if(keyIndex == "INIT_UI")
		{
			UIButtonEvent buttonEvent = mChoosePet.GetComponent<UIButtonEvent>();
			buttonEvent.mData.set("ID", -1);

			ClearChoosePetUI();
			ClearChooseMaterialUI();

			UpdateUI();
		}
		else if(keyIndex == "UPGRADE_PET_OK")
		{
			GameObject black = mGameObjUI.transform.parent.Find("black").gameObject;
			GameObject upgradeEffect = GameCommon.FindObject(mChooseMaterial, "ec_ui_strengthen-002");
			GameObject effect = GameCommon.FindObject(mChooseMaterial, "effect");
			if(upgradeEffect != null && effect != null)
			{
				black.SetActive(false);
				upgradeEffect.SetActive(false);
				effect.SetActive(true);
			}

			UpgradePetOK();
		}
		else if(keyIndex == "SEND_SERVER_PET_UPGRADE")
		{
            bool bIsHadHighStarLevelPet = false;
            foreach (KeyValuePair<int, UpgradePetData> pair in mDicSelPet)
            {
                if (pair.Value.mPetData.starLevel >= 3)
                {
                    bIsHadHighStarLevelPet = true;
                    break;
                }
            }

            if(bIsHadHighStarLevelPet)
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_UPGRADE_PET_HIGH_STAR_LEVEL, "", () => SendServerPetUpgrade());
            else
                SendServerPetUpgrade();
		}
		else if(keyIndex == "UPGRADE_PET_PLAY_EFFECT")
		{
			PlayUpgradeEffect();
		}
		else if(keyIndex == "AUTO_ADD_PET")
		{
			AutoAddPet();
		}
	}

	public void SetUpgradePetByID(int iID)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mPetData = petLogicData.GetPetDataByItemId(iID);
		if(mPetData != null)
		{
			SetBtnSpriteIcon(mChoosePet, mPetData);

			UIButtonEvent buttonEvent = mChoosePet.GetComponent<UIButtonEvent>();
            buttonEvent.mData.set("ID", mPetData.itemId);

			miAimLevelBreak = mPetData.breakLevel;
		}

		SetPetUpgradeAutoAddPetBtnVisible(true);
	}

	public void SetPetUpgradeAutoAddPetBtnVisible(bool bIsVisible)
	{
		Transform objTrans = mGameObjUI.transform.Find("pet_upgrade_auto_add_pet_btn");
		if(objTrans != null)
			objTrans.gameObject.SetActive(bIsVisible);
	}

	public void ClearChoosePetUI()
	{
		if(mChoosePet != null)
		{
			SetBtnSpriteIcon(mChoosePet, null);

			UIButtonEvent buttonEvent = mChoosePet.GetComponent<UIButtonEvent>();
			buttonEvent.mData.set("ID", -1);

			SetPetUpgradeAutoAddPetBtnVisible(false);
		}
	}

	public void ClearChooseMaterialUI()
	{
		if(mChooseMaterial != null)
		{
			SetBtnSpriteIcon(mChooseMaterial, null);
			mSelPetGroup.SetActive(false);
			mUnSelPetGroup.SetActive(true);
		}

		ClearUpgradeButton();
	}

	public void ClearUpgradeButton()
	{
		if(mUpgradeBtnDisabled != null)
			mUpgradeBtnDisabled.SetActive(true);
		
		if(mNeedCoinNumLabel != null)
			mNeedCoinNumLabel.text = "0";
	}


	public void AddSelPetByID(int iID)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		PetData petData = petLogicData.GetPetDataByItemId(iID);
		if(petData != null)
		{
			UpgradePetData upgradePetData = new UpgradePetData();
			upgradePetData.mIndex = GetSelMaxIndex() + 1;
			upgradePetData.mPetData = petData;
			mDicSelPet.Add(iID, upgradePetData);

			SetBtnSpriteIcon(mChooseMaterial, GetSelMaxIndexPetData());
		}

		UpdateUI();
	}

	public void RemoveSelPet(int iID)
	{
		if(mDicSelPet.ContainsKey(iID))
		{
			mDicSelPet.Remove(iID);
			SetBtnSpriteIcon(mChooseMaterial, GetSelMaxIndexPetData());
			UpdateUI();
		}

	}

	public void RemoveUpgradePet()
	{
		mDicSelPet.Clear();
		ClearChoosePetUI();
		ClearChooseMaterialUI();

		miAimLevel = 1;
		miAimExp = 0;
		miAimLevelBreak = 0;
	}

	public int GetSelMaxIndex()
	{
		UpgradePetData upgradePetData = GetSelMaxIndexUpgradePetData();
		if(upgradePetData != null)
		{
			return upgradePetData.mIndex;
		}
		return 0;
	}
	
	public PetData GetSelMaxIndexPetData()
	{
		UpgradePetData upgradePetData = GetSelMaxIndexUpgradePetData();
		if(upgradePetData != null)
		{
			return upgradePetData.mPetData;
		}
		return null;
	}

	public UpgradePetData GetSelMaxIndexUpgradePetData()
	{
		List<UpgradePetData> petList = mDicSelPet.Values.ToList();
		petList = SortList(petList);
		if(mDicSelPet.Count > 0)
		{
			return petList[0];
		}
		return null;
	}

	public List<UpgradePetData> SortList(List<UpgradePetData> list)
	{
		if(list != null && list.Count > 0)
		{
//			list = list.OrderByDescending(p => p.mIndex).ToList();
			list = GameCommon.SortList (list, SortListByIndex);
		}
		return list;
	}

	int SortListByIndex(UpgradePetData a, UpgradePetData b)
	{
		return GameCommon.Sort (a.mIndex, b.mIndex, true);
	}

	public PetData GetSelPetDataByID(int iID)
	{
		UpgradePetData upgradePetData = null;		
		mDicSelPet.TryGetValue(iID, out upgradePetData);

		if(upgradePetData != null)
			return upgradePetData.mPetData;

		return null;
	}

	public bool IsSelPetByID(int iID)
	{
		PetData tempPetData = GetSelPetDataByID(iID);
		return tempPetData != null;
	}

	public bool GetUpgradePetCurLevelIsMax(int iAimLevelBreak)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;

		PetData petData = petLogicData.GetPetDataByItemId(mPetData.itemId);

		if(petData != null)
		{
			if(petData.level < mPetData.mMaxLevelNum + iAimLevelBreak)
			{
				return false;
			}
		}
		return true;
	}

	public bool GetUpgradePetCurLevelBreakIsMax()
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		
		PetData petData = petLogicData.GetPetDataByItemId(mPetData.itemId);
		
		if(petData != null)
		{
			if(petData.level < mPetData.mMaxLevelNum + mPetData.mMaxLevelBreakNum)
			{
				return false;
			}
		}
		return true;
	}

	public bool GetUpgradePetAimLevelIsMax(int iAimLevelBreak)
	{
		if(miAimLevel < mPetData.mMaxLevelNum + iAimLevelBreak)
		{
			return false;
		}
		
		return true;
	}

	public bool GetUpgradePetAimLevelBreakIsMax()
	{
		if(miAimLevel < mPetData.mMaxLevelNum + mPetData.mMaxLevelBreakNum)
		{
			return false;
		}
		
		return true;
	}

	public bool IsPetCanUpgrade(int iID, bool bIsNeedMessageBox)
	{
		if(GetSelPetDataByID(iID) == null)
		{
			int iCurLevelBreak = mPetData.breakLevel;
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			foreach(KeyValuePair<int, UpgradePetData> pair in mDicSelPet)
			{
				if(pair.Value.mPetData.tid == mPetData.tid)
					iCurLevelBreak++;
			}

			int iAimLevelBreak = 0;
			PetData pet = petLogicData.GetPetDataByItemId(iID);
			if(pet != null && mPetData.tid == pet.tid)
				iAimLevelBreak++;

			iAimLevelBreak += iCurLevelBreak;
			if(GetUpgradePetCurLevelIsMax(iAimLevelBreak))
			{
				if(bIsNeedMessageBox)
					DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_UPGRADE_PET_LEVEL_MAX);
				return false;
			}

			if(GetUpgradePetCurLevelBreakIsMax())
			{
				if(bIsNeedMessageBox)
					DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_UPGRADE_PET_LEVEL_MAX);
				return false;
			}

			if(GetUpgradePetAimLevelIsMax(iAimLevelBreak))
			{
				if(bIsNeedMessageBox)
					DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_UPGRADE_PET_AIM_LEVEL_MAX);
				return false;
			}

			if(GetUpgradePetAimLevelBreakIsMax())
			{
				if(bIsNeedMessageBox)
					DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_UPGRADE_PET_AIM_LEVEL_MAX);
				return false;
			}

			if(mDicSelPet.Count >= 10)
			{
				if(bIsNeedMessageBox)
					DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_PET_ADD_EXP);
				return false;
			}
		}
		return true;
	}
	//------------------------------------------------------------------------
	public void UpdateUI()
	{
		if(mDicSelPet.Count > 0)
		{
			mSelPetGroup.SetActive(true);
			mUnSelPetGroup.SetActive(false);
			
			SetAimLevelBreak();
			SetCurExpInfo();
			SetAimExpInfo();
			UpdateNeedCoinNum();
			
			mSelPetNumLabel.text = mDicSelPet.Count.ToString() + "/10";
			mMaxLevelLabel.text = mPetData.mMaxLevelNum.ToString();
			// TODO:
		}
		else
		{
			miAimLevel = 1;
			miAimExp = 0;
			ClearChooseMaterialUI();
		}
	}

	public void SetPetByID(int iID)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		PetData petData = petLogicData.GetPetDataByItemId(iID);
		if(petData != null)
		{
			UpgradePetData tempPetData = null;
			mDicSelPet.TryGetValue(iID, out tempPetData);
			if(tempPetData != null)
			{
				// remove
				RemoveSelPet(iID);
			}
			else
			{
				// add
				AddSelPetByID(iID);
			}
		}
		else
		{
			Logic.EventCenter.Log(LOG_LEVEL.ERROR, "petData is null");
		}
	}

	public int GetNeedSyntheticCoinNum()
	{
		int iNeedSyntheticCoin = 0;
		foreach(KeyValuePair<int, UpgradePetData> kvp in mDicSelPet)
		{
			PetData petData = kvp.Value.mPetData;
			iNeedSyntheticCoin += TableCommon.GetSyntheticCoin(petData.starLevel, petData.level);
		}
		return iNeedSyntheticCoin;
	}
	
	public void DeductCoin()
	{
		RoleLogicData logicData = RoleLogicData.Self;
		if(miNeedSyntheticCoin <= logicData.gold)
		{
			GameCommon.RoleChangeGold((-1)*miNeedSyntheticCoin);
		}
	}

	public void UpdateNeedCoinNum()
	{
		miNeedSyntheticCoin = GetNeedSyntheticCoinNum();
		mUpgradeBtnDisabled.SetActive(true);

		RoleLogicData logicData = RoleLogicData.Self;
		if(miNeedSyntheticCoin <= logicData.gold)
		{
			mUpgradeBtnDisabled.SetActive(false);
		}

		mNeedCoinNumLabel.text = miNeedSyntheticCoin.ToString();

		if(miNeedSyntheticCoin > logicData.gold)
		{
			mNeedCoinNumLabel.text = "[ff0000]" + mNeedCoinNumLabel.text;
		}
	}

	public void SetCurExpInfo()
	{
		 mCurLevelLabel.text = "Lv." + mPetData.level.ToString();

		int curExp = mPetData.exp;
		int maxExp = TableCommon.GetMaxExp(mPetData.starLevel, mPetData.level);
		float percentage = curExp / (float)maxExp;
		
		int iPercentage = (int)(percentage * 100);
		
		mPetCurExpPercentage.text = iPercentage.ToString() + "%";
		mPetCurExpBar.value = percentage;
	}

	public int GetSyntheticExp(PetData pet)
	{
		float fSyntheticExp = 0;
		if(pet != null)
		{
			int iBaseSyntheticExp = TableCommon.GetSyntheticExp(pet.starLevel, pet.level);

			float fMulriple = 1f;
			DataRecord record = DataCenter.mActiveConfigTable.GetRecord(pet.tid);
			if(record != null)
				fMulriple = (float)record.get ("EXP_MULRIPLE");


			int iSrcElementType = TableCommon.GetNumberFromActiveCongfig(mPetData.tid, "ELEMENT_INDEX");
			int iAimElementType = TableCommon.GetNumberFromActiveCongfig(pet.tid, "ELEMENT_INDEX");

			float fElementMulriple = 1f;
			if(record != null && iSrcElementType == iAimElementType)
				fElementMulriple = (float)record.get ("EXP_ELEMENT_MULRIPLE");

			fSyntheticExp = iBaseSyntheticExp * fMulriple * fElementMulriple;
		}

		return (int)fSyntheticExp;
	}
	public void SetAimExpInfo()
	{
		int iAddExp = 0;
		foreach(KeyValuePair<int, UpgradePetData> kvp in mDicSelPet)
		{
			PetData petData = kvp.Value.mPetData;
			iAddExp += GetSyntheticExp(petData);
		}

		miAimExp = mPetData.exp;
		miAimLevel = mPetData.level;

		AddExp(iAddExp);

		if(miAimLevel >= mPetData.mMaxLevelNum + miAimLevelBreak)
		{
			miAimExp = 0;
		}

		SetAimExpInfoUI();

	}

	public void SetAimExpInfoUI()
	{

		mAimLevelLabel.text = "Lv." + miAimLevel.ToString();

		int iAimMaxExp = TableCommon.GetMaxExp(mPetData.starLevel, miAimLevel);

		float percentage = miAimExp / (float)iAimMaxExp;
		
		int iPercentage = (int)(percentage * 100);
		
		mPetAimExpPercentage.text = iPercentage.ToString() + "%";
		mPetAimExpBar.value = percentage;
	}

	public virtual void AddExp(int dExp)
	{
		if(miAimLevel >= mPetData.mMaxLevelNum + miAimLevelBreak)
		{
			miAimLevel = mPetData.mMaxLevelNum + miAimLevelBreak;
			return;
		}

		miAimExp += dExp;

		int iMaxExp = TableCommon.GetMaxExp(mPetData.starLevel, miAimLevel);
		if(miAimExp >= iMaxExp)
		{
			LevelUp();

			int iDExp = miAimExp - iMaxExp;

			miAimExp = 0;
			AddExp(iDExp);
		}
	}

	public virtual void LevelUp()
	{
		if(miAimLevel >= mPetData.mMaxLevelNum + miAimLevelBreak)
			return;
		miAimLevel += 1;
	}

	public virtual void SetAimLevelBreak()
	{
		miAimLevelBreak = mPetData.breakLevel;
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		foreach(KeyValuePair<int, UpgradePetData> pair in mDicSelPet)
		{
			if(pair.Value.mPetData.tid == mPetData.tid)
				miAimLevelBreak++;
		}

		if(miAimLevelBreak > mPetData.mMaxLevelBreakNum)
		{
			miAimLevelBreak = mPetData.mMaxLevelBreakNum;
		}
		SetLevelBreakLabel();
	}

	public void SetLevelBreakLabel()
	{
		if(mGameObjUI != null)
		{
			GameObject levelBreakObj = GameCommon.FindObject(mGameObjUI, "add_level_num_label");
			if(levelBreakObj != null)
			{
				levelBreakObj.SetActive(miAimLevelBreak > 0);
				UILabel levelBreakLable = levelBreakObj.GetComponent<UILabel>();
				if(levelBreakLable != null)
				{
					levelBreakLable.text = "+" + miAimLevelBreak.ToString();
				}
			}

		}
	}

	public void UpgradePetOK()
	{
        BagInfoWindow.isNeedRefresh = true;
        
        // upgrade
		UIButtonEvent buttonEvent = mChoosePet.GetComponent<UIButtonEvent>();
		int iID = (int)buttonEvent.mData.get("ID");

		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		PetData petData = petLogicData.GetPetDataByItemId(iID);

		if(petData != null)
		{
			int iOldExp = petData.exp;
			int iOldMaxExp = TableCommon.GetMaxExp(petData.starLevel, petData.level);
			DataCenter.SetData("UpGradeAndStrengthenResultWindow", "SRC_LEVEL", petData.level);
			DataCenter.SetData("UpGradeAndStrengthenResultWindow", "AIM_LEVEL", miAimLevel);
			DataCenter.SetData("UpGradeAndStrengthenResultWindow", "OLD_EXP_RATIO", iOldExp / iOldMaxExp);

			petData.level = miAimLevel;
			petData.exp = miAimExp;
			petData.breakLevel = miAimLevelBreak;
		}
		else
		{
			Logic.EventCenter.Log(LOG_LEVEL.ERROR, "petData is null");
		}

		// delete select pet
		DeletePetData();

		ClearChooseMaterialUI();

        buttonEvent.mData.set("ID", petData.itemId);
		// update ungrade pet icons
        DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_UPGRADE_PET_ICONS", petData.itemId);

		SetBtnSpriteIcon(mChoosePet, petData);

        DataCenter.SetData("UpGradeAndStrengthenResultWindow", "SHOW_LEVEL_UP", petData.itemId);

		// deduct coin
		DeductCoin();

        // task
        //tLogicData taskNeedData = DataCenter.GetData("TASK_NEED_DATA");
        //taskNeedData.set("NUM", 1);
        //TaskSystemMgr.Self.CheckFinishAllTaskSubentryByTaskType(TASK_TYPE.Pet_Upgrade_Num);
	}

    public void PlayUpgradeEffect()
    {
        GameObject black = mGameObjUI.transform.parent.Find("black").gameObject;
        GameObject upgradeEffect = GameCommon.FindObject(mChooseMaterial, "ec_ui_strengthen-002");
        GameObject effect = GameCommon.FindObject(mChooseMaterial, "effect");
        if (upgradeEffect != null && effect != null)
        {
            black.SetActive(true);
            upgradeEffect.SetActive(true);
            effect.SetActive(false);
            PetUpgradeUI.mfRemainEffectTime = PetUpgradeUI.mfMaxEffectTime;
        }
    }

	public void SendServerPetUpgrade()
	{
		tEvent quest = Net.StartEvent("CS_RequestPetUpgrade");
		DataBuffer dataBuffer = new DataBuffer(256);
		dataBuffer.write(mDicSelPet.Count);
		foreach(KeyValuePair<int, UpgradePetData> kvp in mDicSelPet)
		{
			PetData tempPetData = kvp.Value.mPetData;
			dataBuffer.write(tempPetData.itemId);
		}

		UIButtonEvent buttonEvent = mChoosePet.GetComponent<UIButtonEvent>();
		int iID = (int)buttonEvent.mData.get("ID");
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		PetData petData = petLogicData.GetPetDataByItemId(iID);

		quest.set("PET_ID", petData.itemId);
		quest.set("DATABUFFER", dataBuffer);
		quest.DoEvent();
	}

	public void DeletePetData()
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;

		foreach(KeyValuePair<int, UpgradePetData> iter in mDicSelPet)
		{
			petLogicData.RemoveItemData(iter.Value.mPetData);
		}

		mDicSelPet.Clear();
	}

	public void AutoAddPet()
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		List<PetData> petList = petLogicData.mDicPetData.Values.ToList();
		petList = SortAutoAddPetList(petList);

		for(int i = 0; i < petList.Count; i++)
		{
			PetData pet = petList[i];
			if(pet != null)
			{
				if(pet.itemId == mPetData.itemId 
				   || petLogicData.IsPetUsed(pet.itemId) 
				   || pet.starLevel > 3 
				   || pet.level > 1
				   || pet.strengthenLevel > 0)
					continue;

				bool isCan = IsPetCanUpgrade(pet.itemId, false);
                if (isCan && mDicSelPet.Count < 10 && !mDicSelPet.ContainsKey(pet.itemId))
                {
                    DataCenter.SetData("PetUpgradeWindow", "SET_SELECT_PET_BY_ID", pet.itemId);

                    DataCenter.SetData("BAG_INFO_WINDOW", "SET_UPGRATE_BTN_TOGGLE_STATE", pet.itemId);
                }
                //else
                //{
                //    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_UPGRADE_PET_LEVEL_MAX);
                //}
			}
		}
	}

	public List<PetData> SortAutoAddPetList(List<PetData> list)
	{
		if(list != null && list.Count > 0)
		{
//			list = list.OrderBy (p => p.mStarLevel).ToList();
			list = GameCommon.SortList (list, SortPetDataListByStarLevel);
		}
		
		return list;
	}

	int SortPetDataListByStarLevel(PetData a, PetData b)
	{
		return GameCommon.Sort (a.starLevel, b.starLevel, false);
	}
}