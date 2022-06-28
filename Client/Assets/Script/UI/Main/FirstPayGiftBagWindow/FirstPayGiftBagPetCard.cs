using UnityEngine;
using System.Collections;

public class FirstPayGiftBagPetCard : GainPetInfoUI
{
	public override void Init()
	{
		FirstPayGiftBagPetCardWindow petInfoWindow = new FirstPayGiftBagPetCardWindow();
		DataCenter.Self.registerData("FIRST_PAY_PET_CARD_WINDOW", petInfoWindow);
		
		petInfoWindow.mPetStarLevelLabel = mPetStarLevelLabel;
		petInfoWindow.mPetTitleLabel = mPetTitleLabel;
		petInfoWindow.mPetNameLabel = mPetNameLabel;
		petInfoWindow.mCard = mCard;
		petInfoWindow.mRoleMastButtonUI = mRoleMastButtonUI;
		petInfoWindow.Close();

		int iPetID = DataCenter.GetData ("FIRST_PAY_GIFT_BAG").get ("PET_ID");
		DataCenter.SetData("FIRST_PAY_PET_CARD_WINDOW", "OPEN", true);
		DataCenter.SetData("FIRST_PAY_PET_CARD_WINDOW", "SET_SELECT_PET_BY_MODEL_INDEX", iPetID);
	}
	

	
	public void OnDestroy()
	{		
		DataCenter.Remove("FIRST_PAY_PET_CARD_WINDOW");
	}
}


public class FirstPayGiftBagPetCardWindow : GainPetInfoWindow
{
	public FirstPayGiftBagPetCardWindow()
	{
		m_strWindowName = "first_pay_pet_info_card";
	}
}
