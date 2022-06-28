using UnityEngine;
using System.Collections;
using DataTable;

public class PVP_PlayerInfoWindow : tWindow
{
    public int mAllPetCount = 1;
    public int mDeadCount = 0;

    public override void Open(object param)
    {
        base.Open(param);

		Refresh(param);
    }

    public override bool Refresh(object param)
    {
		RoleLogicData roleLogic = RoleLogicData.Self;
        SetText("my_name", roleLogic.name);

        PetLogicData petLogic = DataCenter.GetData("PET_DATA") as PetLogicData;

        PVP6Battle battle = MainProcess.mStage as PVP6Battle;

        bool bAttrPVP = battle!=null && battle.mbIsAttributeWar;

        mAllPetCount = 0;
        for (int i = 0; i < 6; ++i)
        {
            PetData pet;
            if (bAttrPVP)
                pet = petLogic.GetAttributePVPPet(i);
            else
                pet = petLogic.GetPVPPet(i);

            if (pet!=null)
            {
                ++mAllPetCount;
            }
        }

        if (mAllPetCount <= 0)
            mAllPetCount = 1;

        SetText("blood_max_label", mAllPetCount.ToString());
        SetText("blood_cur_label", mAllPetCount.ToString());
		return true;
    }


    public override void onChange(string keyIndex, object objVal)
    {
        if (keyIndex == "PET_DEAD")
        {
            mDeadCount = 0;

            PVP6Battle battle = MainProcess.mStage as PVP6Battle;
            foreach(BaseObject pet in battle.mSelfPet)
            {
				if (pet!=null && pet.IsDead())
                    ++mDeadCount;
            }

            if (mDeadCount > mAllPetCount)
                mDeadCount = mAllPetCount;
            SetText("blood_cur_label", (mAllPetCount - mDeadCount).ToString());

            GameObject obj = GetSub("my_blood_bar");
            if (obj != null)
            {
                UISlider slider = obj.GetComponent<UISlider>();
                slider.value = (float)(mAllPetCount - mDeadCount) / mAllPetCount;
            }
        }
        else if (keyIndex == "CHANGED_HP")
        {
            PVPActive pet = objVal as PVPActive;
            int nIndex = pet.mIconIndex;

            GameObject girdObj = GetSub("grid");
            if (girdObj != null)
            {
                 UIGridContainer grid = girdObj.GetComponent<UIGridContainer>();
                 if (grid != null)
                 {
                     if (nIndex >= 0 && nIndex < grid.controlList.Count)
                         UpdatePetHp(grid.controlList[nIndex], pet);
                 }
            }

        }
        else if (keyIndex == "UPDATE_PET")
        {           
            GameObject girdObj = GetSub("grid");
            if (girdObj != null)
            {
                UIGridContainer grid = girdObj.GetComponent<UIGridContainer>();
                if (grid != null)
                {
                    int nNowCount = 0;
                    PVP6Battle battle = MainProcess.mStage as PVP6Battle;
                    foreach (BaseObject pet in battle.mSelfPet)
                    {
                        if (pet != null && !pet.IsDead())
                            ++nNowCount;
                    }

                    PetData next = battle.UseSelfPet(true);
                    if (nNowCount >= 3 && next!=null)
                        nNowCount = 4;

                    grid.MaxCount = nNowCount;

                    int i=0;
                    foreach (BaseObject pet in battle.mSelfPet)
                    {
                        if (pet != null && !pet.IsDead())
                        {
                            PVPActive active = pet as PVPActive;
                            active.mIconIndex = i;
                            GameObject item = grid.controlList[i++];
                            GameCommon.SetSprite(item, pet.GetConfig("HEAD_ATLAS_NAME"), pet.GetConfig("HEAD_SPRITE_NAME"));
							UISlider slider = item.GetComponentInChildren<UISlider>();
							if (slider != null)
								slider.value = 1;

                            if (i >= 3)
                                break;
                            //UpdatePetHp(item, pet);
                        }
                    }

                    if (next != null)
                    {
                        DataRecord r = DataCenter.mActiveConfigTable.GetRecord(next.tid);
                        if (r != null)
                        {
                            GameObject item = grid.controlList[i++];
                            GameCommon.SetSprite(item, r.get("HEAD_ATLAS_NAME"), r.get("HEAD_SPRITE_NAME"));
                            GameCommon.SetUIVisiable(item, "blood_bar", false);
                            GameCommon.SetUIVisiable(item, "next_label", true);
                        }
                        else
                            DEBUG.LogError("No exist active config >" + next.tid.ToString());
                    }
                }
            }

        }
        else
            base.onChange(keyIndex, objVal);
    }

    static public void UpdatePetHp(GameObject item, BaseObject pet)
    {
        UISlider slider = item.GetComponentInChildren<UISlider>();
        if (slider != null)
            slider.value = (float)pet.GetHp() / pet.GetMaxHp();
    }
}

public class PVP_OtherPlayerWindow :  tWindow
{
    public int mAllPetCount = 1;
    public int mDeadCount = 0;

	public override void Open(object param)
	{
		base.Open(param);
		
		Refresh(param);
    }
    
    public override bool Refresh(object param)
    {
        //RoleLogicData roleLogic = DataCenter.GetData("ROLE_DATA");
        PVP6Battle battle = MainProcess.mStage as PVP6Battle;
        SetText("other_name", battle.mOpponentName);

        mAllPetCount = 0;
        foreach(PVPPetData p in battle.mOpponentPet)
        {
            if (p != null)
                ++mAllPetCount;
        }

        if (mAllPetCount <= 0)
            mAllPetCount = 1;

        SetText("blood_max_label", mAllPetCount.ToString());
        SetText("blood_cur_label", mAllPetCount.ToString());

		return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        if (keyIndex == "PET_DEAD")
        {
             mDeadCount = 0;

            PVP6Battle battle = MainProcess.mStage as PVP6Battle;
			foreach(BaseObject pet in battle.mOtherPet)
            {
				if (pet!=null && pet.IsDead())
                    ++mDeadCount;
            }

            if (mDeadCount > mAllPetCount)
                mDeadCount = mAllPetCount;
            SetText("blood_cur_label", (mAllPetCount - mDeadCount).ToString());

            GameObject obj = GetSub("other_blood_bar");
            if (obj != null)
            {
                UISlider slider = obj.GetComponent<UISlider>();
                slider.value = (float)(mAllPetCount - mDeadCount) / mAllPetCount;
            }
        }
        else if (keyIndex == "CHANGED_HP")
        {
            PVPActive pet = objVal as PVPActive;
            int nIndex = pet.mIconIndex;

            GameObject girdObj = GetSub("grid");
            if (girdObj != null)
            {
                UIGridContainer grid = girdObj.GetComponent<UIGridContainer>();
                if (grid != null)
                {
                    if (nIndex >= 0 && nIndex < grid.controlList.Count)
						PVP_PlayerInfoWindow.UpdatePetHp(grid.controlList[nIndex], pet);
                }
            }

        }
        else if (keyIndex == "UPDATE_PET")
        {
            GameObject girdObj = GetSub("grid");
            if (girdObj != null)
            {
                UIGridContainer grid = girdObj.GetComponent<UIGridContainer>();
                if (grid != null)
                {
                    int nNowCount = 0;
                    PVP6Battle battle = MainProcess.mStage as PVP6Battle;
                    foreach (BaseObject pet in battle.mOtherPet)
                    {
                        if (pet != null && !pet.IsDead())
                            ++nNowCount;
                    }

                    PetData next = battle.UseOpponentPet(true);
                    if (nNowCount >= 3 && next != null)
                        nNowCount = 4;

                    grid.MaxCount = nNowCount;

                    int i = 0;
                    foreach (BaseObject pet in battle.mOtherPet)
                    {
                        if (pet != null && !pet.IsDead())
                        {
                            PVPActive active = pet as PVPActive;
                            active.mIconIndex = i;
                            GameObject item = grid.controlList[i++];
                            GameCommon.SetSprite(item, pet.GetConfig("HEAD_ATLAS_NAME"), pet.GetConfig("HEAD_SPRITE_NAME"));
                            UISlider slider = item.GetComponentInChildren<UISlider>();
                            if (slider != null)
                                slider.value = 1;
                            //UpdatePetHp(item, pet);
                            if (i>=3)
                                break;
                        }
                    }

                    if (next != null)
                    {
                        DataRecord r = DataCenter.mActiveConfigTable.GetRecord(next.tid);
                        if (r != null)
                        {
                            GameObject item = grid.controlList[i++];
                            GameCommon.SetSprite(item, r.get("HEAD_ATLAS_NAME"), r.get("HEAD_SPRITE_NAME"));
                            GameCommon.SetUIVisiable(item, "blood_bar", false);
                            GameCommon.SetUIVisiable(item, "next_label", true);
                        }
                        else
                            DEBUG.LogError("No exist active config >" + next.tid.ToString());
                    }
                }
            }

        }
        else
            base.onChange(keyIndex, objVal);
    }
}


public class PVPWindow :  tWindow
{
	public override void Open(object param)
	{
//		GameObject obj = GameCommon.FindUI("back_ground");
//		if (obj != null)
//			obj.SetActive(false);

		DataCenter.OpenWindow("PVP_PLAYER_WINDOW");
		DataCenter.OpenWindow("PVP_OTHER_PLAYER_WINDOW");

//		MainProcess.mStage.Start();
	}
}
