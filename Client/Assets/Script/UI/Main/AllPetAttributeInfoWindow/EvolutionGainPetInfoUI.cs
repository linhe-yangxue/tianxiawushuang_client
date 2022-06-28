using UnityEngine;
using System.Collections;

public class EvolutionGainPetInfoUI : GainPetInfoUI {

	public override void Init()
	{
		EvolutionGainPetInfoWindow petInfoWindow = new EvolutionGainPetInfoWindow();
		DataCenter.Self.registerData("evolution_gain_pet_info_window", petInfoWindow);
		
		petInfoWindow.mPetStarLevelLabel = mPetStarLevelLabel;
		petInfoWindow.mPetTitleLabel = mPetTitleLabel;
		petInfoWindow.mPetNameLabel = mPetNameLabel;
		petInfoWindow.mCard = mCard;
		petInfoWindow.mRoleMastButtonUI = mRoleMastButtonUI;
		petInfoWindow.Close();
	}
	
	public void OnDestroy()
	{		
		DataCenter.Remove("evolution_gain_pet_info_window");
	}

}


public class EvolutionGainPetInfoWindow : GainPetInfoWindow
{
	public EvolutionGainPetInfoWindow()
	{
		m_strWindowName = "evolution_gain_pet_info_window";
	}
	
	public override void Init ()
	{
		base.Init();

		mPetStarLevelLabel = mGameObjUI.transform.Find("PetInfo/info/star_level_label").GetComponent<UILabel>();
		mPetTitleLabel = mGameObjUI.transform.Find("PetInfo/info/title_label").GetComponent<UILabel>();
		mPetNameLabel = mGameObjUI.transform.Find("PetInfo/info/name_label").GetComponent<UILabel>();
		mCard = GameCommon.FindObject(mGameObjUI, "gain_pet_info_window_card");
		mRoleMastButtonUI = GameCommon.FindObject(mGameObjUI, "black");

		HideUI();
	}
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "HIDEUI")
		{
			HideUI();
		}
	}
	
	public void HideUI()
	{
		GameObject info = GameCommon.FindObject(mCard, "info");
//		GameObject infoCard = GameCommon.FindObject(mCard, "info_card");
		GameObject infoCardBackground = GameCommon.FindObject(mCard, "info_card_background");
		
		if(info != null)
			info.SetActive(false);
		
//		if(infoCard != null)
//		{
//			UISprite sprite = infoCard.GetComponent<UISprite>();
//			sprite.alpha = 0;
//		}
		
		if(infoCardBackground != null)
			infoCardBackground.SetActive(false);
	}

	public override void SetPetByDBID(int iItemId)
	{
		base.SetPetByDBID(iItemId);

		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mPetData = petLogicData.GetPetDataByItemId(iItemId);
		if(mPetData != null)
		{
			UILabel petStrengthenLevelLabel = mGameObjUI.transform.Find("evolve_success_group/description_info/label2/strengthen_level_label").GetComponent<UILabel>();
			UILabel petLevelLabel = mGameObjUI.transform.Find("evolve_success_group/description_info/label5/level_label").GetComponent<UILabel>();
			UILabel petHPLabel = mGameObjUI.transform.Find("evolve_success_group/description_info/label3/hp_label").GetComponent<UILabel>();
			UILabel petAttackLabel = mGameObjUI.transform.Find("evolve_success_group/description_info/label4/attack_label").GetComponent<UILabel>();
			UIGridContainer srcStarGrid = mGameObjUI.transform.Find("evolve_success_group/description_info/label1/src_evolve_level_grid").GetComponent<UIGridContainer>();
			UIGridContainer aimStarGrid = mGameObjUI.transform.Find("evolve_success_group/description_info/label1/aim_evolve_level_grid").GetComponent<UIGridContainer>();

			petStrengthenLevelLabel.text = mPetData.strengthenLevel.ToString();
			petLevelLabel.text = mPetData.level.ToString();
			petHPLabel.text = (GameCommon.GetBaseMaxHP(mPetData.tid, mPetData.level, mPetData.strengthenLevel)).ToString();
			petAttackLabel.text = ((int)GameCommon.GetBaseAttack(mPetData.tid, mPetData.level, mPetData.strengthenLevel)).ToString();
			srcStarGrid.MaxCount = mPetData.starLevel - 1;
			aimStarGrid.MaxCount = mPetData.starLevel;
		}
	}
}
