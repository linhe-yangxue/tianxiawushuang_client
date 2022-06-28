using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;

public enum ALL_PET_ATTRIBUTE_INFO_INDEX
{
	PetInfo,
	PetUpgrade,
    PetSkill,
	PetEvolution,
	PetDecompose
}

public class AllPetAttributeInfoUI : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		DataCenter.Self.registerData("AllPetAttributeInfoWindow", new AllPetAttributeInfoWindow(gameObject));	
		DataCenter.SetData("AllPetAttributeInfoWindow", "OPEN", true);
	}


	void OnDestroy()
	{
		DataCenter.CloseWindow ("BACK_GROUP_PET_WINDOW");
        DataCenter.Remove("AllPetAttributeInfoWindow");
        DataCenter.CloseWindow("PET_SKILL_WINDOW");
		DataCenter.CloseWindow("PET_DECOMPOSE_WINDOW");
	}

	// Update is called once per frame
	void Update () {
	
	}
}

public class AllPetAttributeInfoWindow : tWindow
{
	public ALL_PET_ATTRIBUTE_INFO_INDEX mLastIndex = ALL_PET_ATTRIBUTE_INFO_INDEX.PetInfo;
	public ALL_PET_ATTRIBUTE_INFO_INDEX mCurIndex = ALL_PET_ATTRIBUTE_INFO_INDEX.PetInfo;
	public bool mbIsInit = false;

	public AllPetAttributeInfoWindow(GameObject obj)
	{
		mGameObjUI = obj;
        VecAllPetAttributeInfoWindowInit();

		//Open(true);
	}

	public override void Open(object param)
	{
		base.Open(param);

        BagInfoWindow.isNeedRefresh = true;

		DataCenter.OpenWindow ("BACK_GROUP_PET_WINDOW");

        if (!GameCommon.bIsLogicDataExist("PET_INFO_WINDOW_PAGE_TYPE"))
        {
            DataCenter.Set("PET_INFO_WINDOW_PAGE_TYPE", (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetInfo);
        }
        mCurIndex = (ALL_PET_ATTRIBUTE_INFO_INDEX)((int)DataCenter.Get("PET_INFO_WINDOW_PAGE_TYPE"));
        mLastIndex = mCurIndex;

        // open PetInfoWindow first
        ShowWindow(mCurIndex);

		ChangePetAndRoleBtnState();

		InitButtonIsUnLockOrNot();

        DataCenter.Set("PET_INFO_WINDOW_PAGE_TYPE", (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetInfo);
	}

	void InitButtonIsUnLockOrNot()
	{
		GameCommon.ToggleCloseButCanClick (GetSub ("PetUpgradeBtn"), UNLOCK_FUNCTION_TYPE.UPGRADE_PET, "Checkmark");
        GameCommon.ToggleCloseButCanClick(GetSub("PetSkillBtn"), UNLOCK_FUNCTION_TYPE.SKILL_PET, "Checkmark");
		GameCommon.ToggleCloseButCanClick (GetSub ("PetEvolutionBtn"), UNLOCK_FUNCTION_TYPE.STRENGTHER_PET, "Checkmark");

		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade", UNLOCK_FUNCTION_TYPE.UPGRADE_PET);
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade_JinHua", UNLOCK_FUNCTION_TYPE.STRENGTHER_PET);
		//GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade_QiangHua", UNLOCK_FUNCTION_TYPE.STRENGTHER_PET);
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "Upgrade_Skill", UNLOCK_FUNCTION_TYPE.SKILL_PET);
	}

    public void VecAllPetAttributeInfoWindowInit()
	{
        DataCenter.Self.registerData("PetInfoWindow", new PetInfoWindow());
		
        DataCenter.Self.registerData("PetUpgradeWindow", new PetUpgradeWindow());
		DataCenter.Self.registerData("PetEvolutionWindow", new PetEvolutionWindow());
        DataCenter.OpenWindow("BAG_INFO_WINDOW");
        DataCenter.SetData("RoleEquipWindow", "CLOSE", true);
	}

	public override void Close()
	{
		base.Close ();
		DataCenter.CloseWindow ("BACK_GROUP_PET_WINDOW");
	}

    public void CloseAllWindow()
	{
		if(mbIsInit)
		{
			DataCenter.SetData("PetInfoWindow", "CLOSE", true);
	        DataCenter.SetData("PetUpgradeWindow", "CLOSE", true);
	        DataCenter.SetData("PetEvolutionWindow", "CLOSE", true);
		}
		else
		{
			mbIsInit = true;
		}
	}

	public void ChangePetAndRoleBtnState()
	{
        GameCommon.ToggleTrue(GameCommon.FindUI("PetInfoBtn"));


		// TODO
		// 按钮变化
		
//		GameObject obj = null;
//		mVecPetAndRoleBtn.TryGetValue(mLastIndex, out obj);
//		if(obj != null)
//		{
//			obj.SetActive(true);
//		}
//
//		mVecPetAndRoleBtn.TryGetValue(mCurIndex, out obj);
//		if(obj != null)
//		{
//			obj.SetActive(false);
//		}
	}

	public void onRemove()
	{
        DataCenter.Remove("PetInfoWindow");
        DataCenter.Remove("PetUpgradeWindow");
        DataCenter.Remove("PetEvolutionWindow");
		DataCenter.CloseWindow("BAG_INFO_WINDOW");
	}

	public void ShowWindow(object param)
	{
        CloseAllWindow();
		if((ALL_PET_ATTRIBUTE_INFO_INDEX)param == ALL_PET_ATTRIBUTE_INFO_INDEX.PetInfo)
		{
			DataCenter.SetData("PetInfoWindow", "OPEN", PET_INFO_WINDOW_TYPE.PET);

            GameCommon.ToggleTrue(GameCommon.FindUI("PetInfoBtn"));


			DataCenter.SetData("AllPetAttributeInfoWindow", "CUR_UI_INDEX", (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetInfo);
		}
		else if((ALL_PET_ATTRIBUTE_INFO_INDEX)param == ALL_PET_ATTRIBUTE_INFO_INDEX.PetUpgrade)
		{
            PetUpgradeWindow petUpgradeWindow = DataCenter.GetData("PetUpgradeWindow") as PetUpgradeWindow;
            if (petUpgradeWindow != null && petUpgradeWindow.mChoosePet)
            {
                DataCenter.SetData("PetUpgradeWindow", "OPEN", true);

                GameCommon.ToggleTrue(GameCommon.FindUI("PetUpgradeBtn"));

                DataCenter.SetData("AllPetAttributeInfoWindow", "CUR_UI_INDEX", (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetUpgrade);

                if (GameCommon.bIsLogicDataExist("SELECT_UPGRADE_PET_ID"))
                {
                    int iSelUpgradePetID = (int)(DataCenter.Get("SELECT_UPGRADE_PET_ID"));
                    if (iSelUpgradePetID > 0)
                    {
                        DataCenter.SetData("PetUpgradeWindow", "SELECT_UPGRADE_PET", iSelUpgradePetID);
                        DataCenter.Set("SELECT_UPGRADE_PET_ID", -1);
                    }
                }
            }
		}
        else if ((ALL_PET_ATTRIBUTE_INFO_INDEX)param == ALL_PET_ATTRIBUTE_INFO_INDEX.PetSkill)
        {
            DataCenter.OpenWindow("PET_SKILL_WINDOW");
            GameCommon.ToggleTrue(GameCommon.FindUI("PetSkillBtn"));
            DataCenter.SetData("AllPetAttributeInfoWindow", "CUR_UI_INDEX", (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetSkill);
        }
		else if ((ALL_PET_ATTRIBUTE_INFO_INDEX)param == ALL_PET_ATTRIBUTE_INFO_INDEX.PetDecompose)
		{
			DataCenter.OpenWindow("PET_DECOMPOSE_WINDOW");
			GameCommon.ToggleTrue(GameCommon.FindUI("PetDecomposeBtn"));
			DataCenter.SetData("AllPetAttributeInfoWindow", "CUR_UI_INDEX", (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetDecompose);
		}
		else if((ALL_PET_ATTRIBUTE_INFO_INDEX)param == ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution)
		{
            PetEvolutionWindow petEvolutionWindow = DataCenter.GetData("PetEvolutionWindow") as PetEvolutionWindow;
            if (petEvolutionWindow != null && petEvolutionWindow.mChoosePet != null)
            {
                DataCenter.SetData("PetEvolutionWindow", "OPEN", true);

                GameCommon.ToggleTrue(GameCommon.FindUI("PetEvolutionBtn"));

                DataCenter.SetData("AllPetAttributeInfoWindow", "CUR_UI_INDEX", (int)ALL_PET_ATTRIBUTE_INFO_INDEX.PetEvolution);

                if (GameCommon.bIsLogicDataExist("SELECT_EVOLUTION_PET_ID"))
                {
                    int iSelUpgradePetID = (int)(DataCenter.Get("SELECT_EVOLUTION_PET_ID"));
                    if (iSelUpgradePetID > 0)
                    {
                        DataCenter.SetData("PetEvolutionWindow", "SELECT_EVOLUTION_PET", iSelUpgradePetID);
                        DataCenter.Set("SELECT_EVOLUTION_PET_ID", -1);
                    }
                }
            }
		}
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);

		if(keyIndex == "SHOW_WINDOW")
		{
			ShowWindow(objVal);
		}
	}
}
//
//public class BackGroupPetWindow : tWindow
//{
//	public override void OnOpen ()
//	{
//		base.OnOpen ();
//
//		UIButtonEvent buttonEvent = mGameObjUI.transform.Find ("AllPetAttributeInfoBack").GetComponent<UIButtonEvent>();
//		if(buttonEvent != null)
//		{
//			buttonEvent.mData.set("ACTION", () => MainUIScript.Self.GoBack());
//		}
//	}
//}