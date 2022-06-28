using UnityEngine;
using System.Collections;

public class ActiveLimitBuyPetGainPetUI : GainPetInfoUI
{
	public override void Init()
	{
		ActiveGainPetWindow petInfoWindow = new ActiveGainPetWindow();
		DataCenter.Self.registerData("ACTIVE_GAIN_PET_WINDOW", petInfoWindow);
		
		petInfoWindow.mPetStarLevelLabel = mPetStarLevelLabel;
		petInfoWindow.mPetTitleLabel = mPetTitleLabel;
		petInfoWindow.mPetNameLabel = mPetNameLabel;
		petInfoWindow.mCard = mCard;
		petInfoWindow.mRoleMastButtonUI = mRoleMastButtonUI;
		petInfoWindow.Close();
	}

	public void OnDestroy()
	{		
		DataCenter.Remove("ACTIVE_GAIN_PET_WINDOW");
	}
}


public class ActiveGainPetWindow : GainPetInfoWindow
{
	public ActiveGainPetWindow()
	{
		m_strWindowName = "active_gain_pet_window";
	}
}
