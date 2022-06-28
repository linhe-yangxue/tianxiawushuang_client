using UnityEngine;
using System.Collections;
using Logic;

public class TeamInfoCommonBaseUI{

	public GameObject mTeamRoot;

//	public GameObject mBackBuuton;
//	public GameObject mForwardButton;
	
	public UIGridContainer mGridPet;
	public UIGridContainer mGridTeam;
//	public UILabel mTeamNumLabel;
	
	public int mWhichTeam = 0;

	public string mStrParentWinName = "";

	public void SetParentWinName(string strParentWinName)
	{
		mStrParentWinName = strParentWinName;
	}
	
	public virtual void Init(GameObject parentObj, string strParentWinName)
	{
		mTeamRoot = GameCommon.FindObject (parentObj, "team_info_common_window");
		SetParentWinName(strParentWinName);

		SetButtonData ("team_back_button");
		SetButtonData ("team_forward_button");

		mGridPet = GameCommon.FindObject (mTeamRoot, "grid_pet").GetComponent<UIGridContainer>();
		mGridTeam = GameCommon.FindObject (mTeamRoot, "grid_team").GetComponent<UIGridContainer>();
//		mTeamNumLabel = GameCommon.FindObject (mTeamRoot, "team_num").GetComponent<UILabel>();

		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mWhichTeam = petLogicData.mCurrentTeam;


		Refresh(null);
	}

	void SetButtonData(string strName)
	{
		UIButtonEvent buttonData = GameCommon.FindObject (mTeamRoot, strName).GetComponent<UIButtonEvent>();
		if(buttonData != null) buttonData.mData.set ("PARENT_WINDOW_NAME", mStrParentWinName);
	}

	public void BackOrForward(int i)
	{
		mWhichTeam += i;

		tEvent evt = Net.StartEvent ("CS_SetTeam");
		evt.set ("TEAM_ID", mWhichTeam);
		evt.set ("WINDOW_NAME", mStrParentWinName);
		evt.DoEvent ();

//		Refresh (null);
	}

	public virtual bool Refresh(object param)
	{
		RefreshTeamIcons();
		RefreshPetIcons();
		SetElementState();
		return true;
	}

	public void RefreshTeamIcons()
	{
		if(mWhichTeam == 0) GameCommon.SetUIVisiable (mTeamRoot, "team_back_button", false);
		else GameCommon.SetUIVisiable (mTeamRoot, "team_back_button", true);
		if(mWhichTeam == mGridTeam.MaxCount - 1) GameCommon.SetUIVisiable (mTeamRoot, "team_forward_button", false);
		else GameCommon.SetUIVisiable (mTeamRoot, "team_forward_button", true);
		
//		mTeamNumLabel.text = (mWhichTeam + 1).ToString ();
		GameCommon.SetUIText (mTeamRoot, "team_num", (mWhichTeam + 1).ToString ());
		for(int t = 0; t < mGridTeam.MaxCount; t++)
		{
			if(t == mWhichTeam) mGridTeam.controlList[t].GetComponent<UISprite>().spriteName = "ui_point_on";
			else mGridTeam.controlList[t].GetComponent<UISprite>().spriteName = "ui_point_off";
		}
	}

	public void RefreshPetIcons()
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mGridPet.MaxCount = 3;
		for(int i = 0; i < 3; i++)
		{
			GameObject obj = mGridPet.controlList[i];
			GameObject petInfoParent = GameCommon.FindObject (obj, "pet_info_parent");
			PetData pet = petLogicData.GetPetDataByPos(mWhichTeam, i + 1);
			if(pet != null)
			{
				int iPetID = pet.tid;
				GameCommon.SetPetIconWithElementAndStarAndLevel (obj, "pet_icon", "element", "star_level_label", "level_label", pet.level, iPetID);
//				SetElementEffect (obj, iPetID);//add element effect
				GameCommon.SetStrengthenLevelLabel (obj, pet.strengthenLevel, "strengthen_level_label");
//				string strStrengthenLevel = "+" + pet.mStrengthenLevel.ToString ();
//				if(pet.mStrengthenLevel < 1) strStrengthenLevel = "";
//				GameCommon.SetUIText (obj, "strengthen_level_label", strStrengthenLevel);

				petInfoParent.SetActive (true);
			}
			else 
			{
				petInfoParent.SetActive (false);
			}
		}
	}

	public void SetElementState()
	{
		int iElementIndex = -1;
		
		PetLogicData petLogicData = DataCenter.GetData ("PET_DATA") as PetLogicData;
		int flag = 1;
		for(int i = 0; i < 3; i++)
		{
			PetData pet = petLogicData.GetPetDataByPos(mWhichTeam, i + 1);
			if(pet != null)
			{
				int iElement = TableCommon.GetNumberFromActiveCongfig(pet.tid, "ELEMENT_INDEX");

				if(iElementIndex == -1)
				{
					iElementIndex = iElement;
				}

				if(iElement == iElementIndex)
					flag = flag << 1 ;
			}
		}
		flag = flag >> 3;
		GameObject effectGroup = GameCommon.FindObject (mTeamRoot, "effect_group").gameObject;
		if(effectGroup != null)
		{
			effectGroup.SetActive(flag == 1);

			if(iElementIndex != -1)
			{
				for(int i = (int)ELEMENT_TYPE.RED; i < (int)ELEMENT_TYPE.MAX; i++)
				{
					GameObject effect = effectGroup.transform.Find("ec_ui_team_" + i.ToString()).gameObject;
					effect.SetActive(i == iElementIndex);
				}
			}
		}
	}

	public void SetElementEffect(GameObject obj, int iPetID)
	{
		GameCommon.SetUIVisiable (obj, "ec_ui_blue", false);
		GameCommon.SetUIVisiable (obj, "ec_ui_gold", false);
		GameCommon.SetUIVisiable (obj, "ec_ui_green", false);
		GameCommon.SetUIVisiable (obj, "ec_ui_red", false);
		GameCommon.SetUIVisiable (obj, "ec_ui_shadow", false);
		int iElementIndex = TableCommon.GetNumberFromActiveCongfig(iPetID, "ELEMENT_INDEX");

		if(iElementIndex == 0) GameCommon.SetUIVisiable (obj, "ec_ui_red", true);
		else if(iElementIndex == 1) GameCommon.SetUIVisiable (obj, "ec_ui_blue", true);
		else if(iElementIndex == 2) GameCommon.SetUIVisiable (obj, "ec_ui_green", true);
		else if(iElementIndex == 3) GameCommon.SetUIVisiable (obj, "ec_ui_gold", true);
		else if(iElementIndex == 4) GameCommon.SetUIVisiable (obj, "ec_ui_shadow", true);
	}
}

public class TeamInfoCommonUI : TeamInfoCommonBaseUI{

	public override bool Refresh(object param)
	{
		base.Refresh(null);

		RefreshSkillIcons();
		
		return true;
	}

	public void RefreshSkillIcons()
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		mGridPet.MaxCount = 3;
		for(int i = 0; i < 3; i++)
		{
			GameObject obj = mGridPet.controlList[i];
			PetData pet = petLogicData.GetPetDataByPos(i + 1);
			if(pet != null)
			{
				int iPetID = pet.tid;

                int iSkillID = pet.GetSkillIndexByIndex(0);
				//int iSkillID = TableCommon.GetNumberFromActiveCongfig (iPetID, "PET_SKILL_1");
				string strSkillIconName = TableCommon.GetStringFromSkillConfig (iSkillID, "SKILL_SPRITE_NAME");
				GameCommon.SetUISprite (obj, "skill_icon", strSkillIconName);

                // name
                GameCommon.SetPetSkillName(obj, iSkillID, "skill_name");

                // level
                GameCommon.SetPetSkillLevel(obj, iSkillID, "skill_level", pet);

				NiceData buttonData = GameCommon.GetButtonData (obj, "Button1");
				buttonData.set ("INDEX", iSkillID);
				buttonData.set ("TABLE_NAME", "SKILL");
			}
		}
	}
}

public class PetInfoTeamInfoUI : TeamInfoCommonBaseUI{

	public override bool Refresh (object param)
	{
		base.Refresh (param);

		ShowPetFlag(false);

		int iGridIndex = 0;
		if(param != null)
		{
			iGridIndex = (int)param - 1;
		}
		SetSelect(iGridIndex);

		SetPetBtnEvent();
		return true;
	}


	public void SetPetBtnEvent()
	{
		for(int i = 0; i < mGridPet.MaxCount; i++)
		{
			GameObject obj = mGridPet.controlList[i];
			UIButtonEvent uiPetCheckBoxButtonEvent = obj.transform.Find("pet_play_check_box_btn").GetComponent<UIButtonEvent>();
			UIButtonEvent uiPetFlagButtonEvent = obj.transform.Find("pet_play_flag_btn").GetComponent<UIButtonEvent>();
			uiPetCheckBoxButtonEvent.mData.set ("POS", i + 1);
			uiPetFlagButtonEvent.mData.set ("POS", i + 1);
		}
	}

	public void SetSelect(int iGridIndex)
	{
		if(iGridIndex >= 0 && iGridIndex < 3)
			GameCommon.ToggleTrue(mGridPet.controlList[iGridIndex].transform.Find("pet_play_check_box_btn").gameObject);
	}

	public void ShowPetFlag(bool bisVisible)
	{
        if (mGridPet == null)
            return;

		for(int i = 0; i < mGridPet.MaxCount; i++)
		{
			mGridPet.controlList[i].transform.Find("pet_play_flag_btn").gameObject.SetActive(bisVisible);
		}
	}

//	public override bool Refresh(object param)
//	{
//		base.Refresh(null);
//		
//		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
//		mGridPet.MaxCount = 3;
//		for(int i = 0; i < 3; i++)
//		{
//			GameObject obj = mGridPet.controlList[i];
//			PetData pet = petLogicData.GetPetDataByPos(i + 1);
//			if(pet != null)
//			{
//				int iPetID = pet.mModelIndex;
//				
//				int iSkillID = TableCommon.GetNumberFromActiveCongfig (iPetID, "PET_SKILL_1");
//				string strSkillIconName = TableCommon.GetStringFromSkillConfig (iSkillID, "ICON");
//				GameCommon.SetUISprite (obj, "skill_icon", strSkillIconName);
//			}
//			else 
//			{
//				GameObject petInfoParent = GameCommon.FindObject (obj, "pet_info_parent");
//				petInfoParent.SetActive (false);
//			}
//		}
//		
//		return true;
//	}
}

public class Button_team_forward_button : CEvent
{
	public override bool _DoEvent()
	{
		string strParentWinName = get ("PARENT_WINDOW_NAME");
		DataCenter.SetData (strParentWinName, "BACK_OR_FORWARD", 1);
		return true;
	}
}

public class Button_team_back_button : CEvent
{
	public override bool _DoEvent()
	{
		string strParentWinName = get ("PARENT_WINDOW_NAME");
		DataCenter.SetData (strParentWinName, "BACK_OR_FORWARD", -1);
		return true;
	}
}
