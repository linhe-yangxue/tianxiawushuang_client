using UnityEngine;
using System.Collections;
using DataTable;
using System;

public class UpGradeAndStrengthenResultUI : MonoBehaviour {
	
	public UILabel mPetStarLevelLabel;
	public UILabel mPetTitleLabel;
	public UILabel mPetNameLabel;

	public UILabel mLevelUpSuccessSrcLevelLabel;
	public UILabel mLevelUpSuccessAimLevelLabel;
	public UILabel mLevelUpSuccessSrcAttackLabel;
	public UILabel mLevelUpSuccessAimAttackLabel;
	public UILabel mLevelUpSuccessSrcHPLabel;
	public UILabel mLevelUpSuccessAimHPLabel;

	public GameObject mCard;
	public float mfCardScale = 1.0f;
	
	// Use this for initialization
	void Start () {
        GameCommon.InitCardWithoutBackground(mCard, mfCardScale, gameObject);

		UpGradeAndStrengthenResultWindow window = new UpGradeAndStrengthenResultWindow();
		DataCenter.Self.registerData("UpGradeAndStrengthenResultWindow", window);
		Init(window);
    }

//    void InitCard()
//    {
//        GameObject obj = GameCommon.LoadAndIntanciateUIPrefabs("card_group_window", mCard.name);
		
//        if(obj != null)
//        {
//            CardGroupUI uiScript = obj.GetComponent<CardGroupUI>();
//            uiScript.InitPetInfo(mCard.name, mfCardScale, gameObject);
//        }
		
//        mCard.transform.localScale = mCard.transform.localScale * mfCardScale;

//        // hide
//        HideUI();
//    }

//    public void HideUI()
//    {
//        GameObject info = GameCommon.FindObject(mCard, "info");
////		GameObject infoCard = GameCommon.FindObject(mCard, "info_card");
//        GameObject infoCardBackground = GameCommon.FindObject(mCard, "info_card_background");

//        if(info != null)
//            info.SetActive(false);
		
////		if(infoCard != null)
////		{
////			UISprite sprite = infoCard.GetComponent<UISprite>();
////			sprite.alpha = 0;
////		}
		
//        if(infoCardBackground != null)
//            infoCardBackground.SetActive(false);
//    }
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void Init(UpGradeAndStrengthenResultWindow window)
	{
		window.mPetStarLevelLabel = mPetStarLevelLabel;
		window.mPetTitleLabel = mPetTitleLabel;
		window.mPetNameLabel = mPetNameLabel;
														  
		window.mLevelUpSuccessSrcLevelLabel				  = mLevelUpSuccessSrcLevelLabel;
		window.mLevelUpSuccessAimLevelLabel				  = mLevelUpSuccessAimLevelLabel;
		window.mLevelUpSuccessSrcAttackLabel			  = mLevelUpSuccessSrcAttackLabel;
		window.mLevelUpSuccessAimAttackLabel			  = mLevelUpSuccessAimAttackLabel;
		window.mLevelUpSuccessSrcHPLabel				  = mLevelUpSuccessSrcHPLabel;
		window.mLevelUpSuccessAimHPLabel				  = mLevelUpSuccessAimHPLabel;

		window.mCard = mCard;
		window.mfCardScale = mfCardScale;
	}
	
	public void OnDestroy()
	{
		DataCenter.Remove("UpGradeAndStrengthenResultWindow");
	}
}


public class UpGradeAndStrengthenResultWindow : tWindow{

	
	public UILabel mPetStarLevelLabel;
	public UILabel mPetTitleLabel;
	public UILabel mPetNameLabel;

	public UILabel mStrengthenFailStrengthenPointLabel;
	public UILabel mStrengthenFailStrengthenLevelLabel;
	public UILabel mStrengthenFailHpLabel;
	public UILabel mStrengthenFailAttackLabel;
	public UILabel mStrengthenFailCurStrengthenPointLabel;
	
	public UILabel mStrengthenSuccessSrcStrengthenLevelLabel;
	public UILabel mStrengthenSuccessAimStrengthenLevelLabel;
	public UILabel mStrengthenSuccessSrcAttackLabel;
	public UILabel mStrengthenSuccessAimAttackLabel;
	public UILabel mStrengthenSuccessSrcHPLabel;
	public UILabel mStrengthenSuccessAimHPLabel;
	
	public UILabel mLevelUpSuccessSrcLevelLabel;
	public UILabel mLevelUpSuccessAimLevelLabel;
	public UILabel mLevelUpSuccessSrcAttackLabel;
	public UILabel mLevelUpSuccessAimAttackLabel;
	public UILabel mLevelUpSuccessSrcHPLabel;
	public UILabel mLevelUpSuccessAimHPLabel;
	public UILabel mLevelUpSuccessAddLevelNumLabel;
	public GameObject mLevelUpSuccessMaxLevel;
	
	public GameObject mStrengthenFailGroup;
	public GameObject mStrengthenSuccessGroup;
	public GameObject mLevelUpSuccessGroup;

	public GameObject mUpgradeSprite;
	public GameObject mUnUpgradeSprite;
	public UIProgressBar mLevelUpExpBar;
	public UIProgressBar mStrengthenFailExpBar;
	public UIProgressBar mStrengthenSuccessExpBar;
	public UILabel mMaxStrengthenLevelLabel;
	
	public PetData mPetData = null;
	public DataRecord mDataRecord = null;

	public GameObject mCard;
	public float mfCardScale = 1.0f;

	public override void Init ()
	{
		mGameObjUI = GameCommon.FindUI("UpGradeAndStrengthenResultWindow");

		mLevelUpSuccessGroup = GameCommon.FindObject(mGameObjUI, "level_up_success_group");
		mStrengthenFailGroup = GameCommon.FindObject(mGameObjUI, "strengthen_fail_group");
		mStrengthenSuccessGroup = GameCommon.FindObject(mGameObjUI, "strengthen_success_group");

		mUpgradeSprite = GameCommon.FindObject(mLevelUpSuccessGroup, "upgrade_sprite");
		mUnUpgradeSprite = GameCommon.FindObject(mLevelUpSuccessGroup, "un_upgrade_sprite");
		mLevelUpSuccessSrcLevelLabel = GameCommon.FindObject(mLevelUpSuccessGroup, "src_level_label").GetComponent<UILabel>();
		mLevelUpSuccessAimLevelLabel = GameCommon.FindObject(mLevelUpSuccessGroup, "aim_level_label").GetComponent<UILabel>();
		mLevelUpSuccessSrcAttackLabel = GameCommon.FindObject(mLevelUpSuccessGroup, "src_attack_label").GetComponent<UILabel>();
		mLevelUpSuccessAimAttackLabel = GameCommon.FindObject(mLevelUpSuccessGroup, "aim_attack_label").GetComponent<UILabel>();
		mLevelUpSuccessSrcHPLabel = GameCommon.FindObject(mLevelUpSuccessGroup, "src_hp_label").GetComponent<UILabel>();
		mLevelUpSuccessAimHPLabel = GameCommon.FindObject(mLevelUpSuccessGroup, "aim_hp_label").GetComponent<UILabel>();
		mLevelUpExpBar = GameCommon.FindObject(mLevelUpSuccessGroup, "exp_bar").GetComponent<UIProgressBar>();
		mLevelUpSuccessAddLevelNumLabel = GameCommon.FindObject(mLevelUpSuccessGroup, "add_level_num_label").GetComponent<UILabel>();
		mLevelUpSuccessMaxLevel = GameCommon.FindObject(mLevelUpSuccessGroup, "max_level_group");

		mStrengthenSuccessSrcStrengthenLevelLabel = GameCommon.FindObject(mStrengthenSuccessGroup, "src_strengthen_level_label").GetComponent<UILabel>();
		mStrengthenSuccessAimStrengthenLevelLabel = GameCommon.FindObject(mStrengthenSuccessGroup, "aim_strengthen_level_label").GetComponent<UILabel>();
		mStrengthenSuccessSrcAttackLabel = GameCommon.FindObject(mStrengthenSuccessGroup, "src_attack_label").GetComponent<UILabel>();
		mStrengthenSuccessAimAttackLabel = GameCommon.FindObject(mStrengthenSuccessGroup, "aim_attack_label").GetComponent<UILabel>();
		mStrengthenSuccessSrcHPLabel = GameCommon.FindObject(mStrengthenSuccessGroup, "src_hp_label").GetComponent<UILabel>();
		mStrengthenSuccessAimHPLabel = GameCommon.FindObject(mStrengthenSuccessGroup, "aim_hp_label").GetComponent<UILabel>();
		mStrengthenSuccessExpBar = GameCommon.FindObject(mStrengthenSuccessGroup, "exp_bar").GetComponent<UIProgressBar>();
		mMaxStrengthenLevelLabel = GameCommon.FindObject(mStrengthenSuccessGroup, "max_strengthen_level_label").GetComponent<UILabel>();

		mStrengthenFailStrengthenPointLabel = GameCommon.FindObject(mStrengthenFailGroup, "strengthen_point_label").GetComponent<UILabel>();
		mStrengthenFailStrengthenLevelLabel = GameCommon.FindObject(mStrengthenFailGroup, "strengthen_level_label").GetComponent<UILabel>();
		mStrengthenFailHpLabel = GameCommon.FindObject(mStrengthenFailGroup, "hp_label").GetComponent<UILabel>();
		mStrengthenFailAttackLabel = GameCommon.FindObject(mStrengthenFailGroup, "attack_label").GetComponent<UILabel>();
		mStrengthenFailCurStrengthenPointLabel = GameCommon.FindObject(mStrengthenFailGroup, "cur_strengthen_point_label").GetComponent<UILabel>();
		mStrengthenFailExpBar = GameCommon.FindObject(mStrengthenFailGroup, "exp_bar").GetComponent<UIProgressBar>();
		
		Close();
	}
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "SET_SELECT_PET_BY_ID")
		{
			SetPetByID((int)objVal);
		}
		else if(keyIndex == "SHOW_LEVEL_UP")
		{
			ShowLevelUpGroup((int)objVal);
		}
		else if(keyIndex == "SHOW_STRENGTHEN_SUCCESS")
		{
			ShowStrengthenSuccessGroup((int)objVal);
		}
		else if(keyIndex == "SHOW_STRENGTHEN_FAILED")
		{
			ShowStrengthenFaildGroup((int)objVal);
		}
	}
	
	public void SetPetByID(int iID)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mPetData = petLogicData.GetPetDataByItemId(iID);
		if(mPetData != null)
		{
			GameCommon.SetCardInfo(mCard.name, mPetData.tid, mPetData.level, mPetData.strengthenLevel, false);
		}
	}

	public void ShowLevelUpGroup(int iID)
	{
		Open(true);
		SetPetByID(iID);

		mLevelUpSuccessGroup.SetActive(true);
		mStrengthenSuccessGroup.SetActive(false);
		mStrengthenFailGroup.SetActive(false);

		int iSrcLevel = (int)get ("SRC_LEVEL");
		int iAimLevel = (int)get ("AIM_LEVEL");

		mLevelUpSuccessSrcLevelLabel.text = iSrcLevel.ToString();
		mLevelUpSuccessSrcAttackLabel.text = ((int)GameCommon.GetBaseAttack(mPetData.tid, iSrcLevel, mPetData.strengthenLevel)).ToString();
		mLevelUpSuccessSrcHPLabel.text = ((int)GameCommon.GetBaseMaxHP(mPetData.tid, iSrcLevel, mPetData.strengthenLevel)).ToString();

		mLevelUpSuccessAimLevelLabel.text = iAimLevel.ToString();
		mLevelUpSuccessAimAttackLabel.text = ((int)GameCommon.GetBaseAttack(mPetData.tid, iAimLevel, mPetData.strengthenLevel)).ToString();
		mLevelUpSuccessAimHPLabel.text = ((int)GameCommon.GetBaseMaxHP(mPetData.tid, iAimLevel, mPetData.strengthenLevel)).ToString();

		UILabel LevelUpSuccessSrcAttackPLabel = mLevelUpSuccessGroup.transform.Find("description_info/label3/add_attack_label").GetComponent<UILabel>();
		UILabel LevelUpSuccessSrcDHPLabel = mLevelUpSuccessGroup.transform.Find("description_info/label4/add_hp_label").GetComponent<UILabel>();
		LevelUpSuccessSrcAttackPLabel.text = "(+" + (Convert.ToInt32(mLevelUpSuccessAimAttackLabel.text) - Convert.ToInt32(mLevelUpSuccessSrcAttackLabel.text)).ToString() + ")";
		LevelUpSuccessSrcDHPLabel.text = "(+" + (Convert.ToInt32(mLevelUpSuccessAimHPLabel.text) - Convert.ToInt32(mLevelUpSuccessSrcHPLabel.text)).ToString() + ")";

		// level break
		mLevelUpSuccessAddLevelNumLabel.text = "+" + mPetData.breakLevel.ToString();
		mLevelUpSuccessAddLevelNumLabel.gameObject.SetActive(mPetData.breakLevel > 0);
		mLevelUpSuccessMaxLevel.SetActive(mPetData.breakLevel <= 0);

		GameObject successEffect = GameCommon.FindObject(mGameObjUI, "success_effect_group");
		successEffect.SetActive(iSrcLevel != iAimLevel);
		mUpgradeSprite.SetActive(iSrcLevel != iAimLevel);
		mUnUpgradeSprite.SetActive(iSrcLevel == iAimLevel);

		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mPetData = petLogicData.GetPetDataByItemId(iID);
		if(mPetData != null)
		{
			int curExp = mPetData.exp;
			int maxExp = TableCommon.GetMaxExp(mPetData.starLevel, mPetData.level);
			float percentage = curExp / (float)maxExp;

			SetSmoothAddValue(mLevelUpExpBar, get ("OLD_EXP_RATIO"), percentage + iAimLevel - iSrcLevel, 0.05f);
			mLevelUpExpBar.value = percentage;
		}
	}

	public void ShowStrengthenSuccessGroup(int iID)
	{
		Open(true);
		SetPetByID(iID);
		
		mLevelUpSuccessGroup.SetActive(false);
		mStrengthenSuccessGroup.SetActive(true);
		mStrengthenFailGroup.SetActive(false);

		int iAimStrengthenLevel = (int)get ("AIM_STRENGTHEN_LEVEL");
		int iSrcStrengthenLevel = iAimStrengthenLevel - 1;

		mStrengthenSuccessSrcStrengthenLevelLabel.text = iSrcStrengthenLevel.ToString();
		mStrengthenSuccessSrcAttackLabel.text = ((int)GameCommon.GetBaseAttack(mPetData.tid, mPetData.level, iSrcStrengthenLevel)).ToString();
		mStrengthenSuccessSrcHPLabel.text = ((int)GameCommon.GetBaseMaxHP(mPetData.tid, mPetData.level, iSrcStrengthenLevel)).ToString();

		mStrengthenSuccessAimStrengthenLevelLabel.text = iAimStrengthenLevel.ToString();
		mStrengthenSuccessAimAttackLabel.text = ((int)GameCommon.GetBaseAttack(mPetData.tid, mPetData.level, iAimStrengthenLevel)).ToString();
		mStrengthenSuccessAimHPLabel.text = ((int)GameCommon.GetBaseMaxHP(mPetData.tid, mPetData.level, iAimStrengthenLevel)).ToString();

		UILabel strengthenSuccessSrcAttackPLabel = mStrengthenSuccessGroup.transform.Find("description_info/label3/add_attack_label").GetComponent<UILabel>();
		UILabel strengthenSuccessSrcDHPLabel = mStrengthenSuccessGroup.transform.Find("description_info/label4/add_hp_label").GetComponent<UILabel>();
		strengthenSuccessSrcAttackPLabel.text = "(+" + (Convert.ToInt32(mStrengthenSuccessAimAttackLabel.text) - Convert.ToInt32(mStrengthenSuccessSrcAttackLabel.text)).ToString() + ")";
		strengthenSuccessSrcDHPLabel.text = "(+" + (Convert.ToInt32(mStrengthenSuccessAimHPLabel.text) - Convert.ToInt32(mStrengthenSuccessSrcHPLabel.text)).ToString() + ")";

		mMaxStrengthenLevelLabel.text = (TableCommon.GetNumberFromActiveCongfig(mPetData.tid, "MAX_GROW_LEVEL")).ToString();

		mStrengthenSuccessExpBar.gameObject.SetActive(false);

		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mPetData = petLogicData.GetPetDataByItemId(iID);

//		if(mPetData != null)
//		{
//			int curExp = mPetData.mExp;
//			int maxExp = TableCommon.GetMaxExp(mPetData.mStarLevel, mPetData.mLevel);
//			float percentage = curExp / (float)maxExp;
//			
//			mStrengthenSuccessExpBar.value = percentage;
//
//			UILabel curLabel = mStrengthenSuccessExpBar.transform.Find("cur_label").GetComponent<UILabel>();
//			UILabel addLabel = mStrengthenSuccessExpBar.transform.Find("add_label").GetComponent<UILabel>();
//		}

		GameObject successEffect = GameCommon.FindObject(mGameObjUI, "success_effect_group");
		successEffect.SetActive(true);
	}

	public void ShowStrengthenFaildGroup(int iID)
	{
		Open(true);
		SetPetByID(iID);
		
		mLevelUpSuccessGroup.SetActive(false);
		mStrengthenSuccessGroup.SetActive(false);
		mStrengthenFailGroup.SetActive(true);

		int iAddFailedPoint = get("STRENGTHEN_FAILED_POINT");
		mStrengthenFailStrengthenPointLabel.text = "+" + iAddFailedPoint.ToString() + "%";
//		mStrengthenFailCurStrengthenPointLabel.text = mPetData.mFailPoint.ToString() + "/100";

		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mPetData = petLogicData.GetPetDataByItemId(iID);
		if(mPetData != null)
		{
			SetSmoothAddValue(mStrengthenFailExpBar, (mPetData.mFailPoint - iAddFailedPoint)/100.0f, mPetData.mFailPoint/100.0f, 0.011f);
//			float percentage = mPetData.mFailPoint / 100.0f;
//			mStrengthenFailExpBar.value = percentage;
			UILabel curLabel = mStrengthenFailExpBar.transform.Find("cur_label").GetComponent<UILabel>();
			UILabel oldLabel = mStrengthenFailExpBar.transform.Find("old_label").GetComponent<UILabel>();
			curLabel.text = mPetData.mFailPoint.ToString() + "%";
			oldLabel.text = (mPetData.mFailPoint - iAddFailedPoint).ToString() + "%";

			mStrengthenFailStrengthenLevelLabel.text = mPetData.strengthenLevel.ToString();
			mStrengthenFailHpLabel.text = ((int)GameCommon.GetBaseMaxHP(mPetData.tid, mPetData.level, mPetData.strengthenLevel)).ToString();
			mStrengthenFailAttackLabel.text = ((int)GameCommon.GetBaseAttack(mPetData.tid, mPetData.level, mPetData.strengthenLevel)).ToString();
		}

		GameObject successEffect = GameCommon.FindObject(mGameObjUI, "success_effect_group");
		successEffect.SetActive(false);
	}

	public void SetSmoothAddValue(UIProgressBar bar, float fStartValue, float fEndValue, float fStep)
	{
		UpGradeAndStrengthenResultUI uiScript = mGameObjUI.GetComponent<UpGradeAndStrengthenResultUI>();
		SmoothAddValue(uiScript, bar, fStartValue, fEndValue, fStep);
	}

	public void SmoothAddValue(MonoBehaviour uiScript, UIProgressBar bar, float fStartValue, float fEndValue, float fStep)
	{
		if(mGameObjUI != null && uiScript != null)
		{
			uiScript.StartCoroutine(AddValue(bar, fStartValue, fEndValue, fStep));
		}
	}
	
	private IEnumerator AddValue(UIProgressBar bar, float fStartValue, float fEndValue, float fStep)
	{
		if(bar != null)
		{
			int iCutValue = (int)(fStartValue * 100);
			int iEndValue = (int)(fEndValue * 100);
			int iStep = (int)(fStep * 100);
			while(iCutValue <= iEndValue)
			{
				bar.value = (iCutValue%100) * 0.01f;
				iCutValue += iStep;
				yield return new WaitForSeconds(0.0001f);
			}

			if(bar.value >= 1.0f)
				bar.value = 0;
		}

        GuideManager.Notify(GuideIndex.PetUpgradeOK);
	}
}