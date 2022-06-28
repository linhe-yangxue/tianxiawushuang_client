using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Logic;
using Utilities;
using DataTable;

public class PetDecomposeWindow : tWindow
{
	int mSelcetPetMaxCount = 5;
	List<PetData> mSelectPetDatas = new List<PetData>();
	List<DecomposeResultItem> mDecomposeResultItems = new List<DecomposeResultItem>();

	int mCurrent = 0;
	int mTotalCostNum = 0;

	bool mbAdd;
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_choose_decompose_pet", new DefineFactory<Button_choose_decompose_pet>());
		EventCenter.Self.RegisterEvent("Button_pet_decompose_button", new DefineFactory<Button_pet_decompose_button>());
		EventCenter.Self.RegisterEvent("Button_decompose_result_forward_button", new DefineFactory<Button_decompose_result_forward_button>());
		EventCenter.Self.RegisterEvent("Button_decompose_result_back_button", new DefineFactory<Button_decompose_result_back_button>());
		EventCenter.Self.RegisterEvent("Button_pet_decompose_auto_add_pet_btn", new DefineFactory<Button_pet_decompose_auto_add_pet_btn>());

		Net.gNetEventCenter.RegisterEvent ("CS_DecomposePet", new DefineFactoryLog<CS_DecomposePet>());
	}
	
	public override void Open (object param)
	{
		base.Open (param);
	}

	public override void OnOpen()
	{
		DataCenter.CloseWindow("PET_SKILL_WINDOW");
		//DataCenter.OpenWindow("BAG_INFO_WINDOW");

		Refresh (null);
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
		case "SHOW_PET_BAG":
            DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_DECOMPOSE_PET_ICONS", new List<PetData> { new PetData { itemId = -1 } });
			DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.Bag_Pet_Window_SpriteTitle);
			break;
		case "IS_ADD":
			mbAdd = (bool)objVal;
			break;
		case "ADD_OR_REMOVE_SELECT_DECOMPOSE_PET":
			if(mbAdd && mSelectPetDatas.Count  >= mSelcetPetMaxCount)
				break ;

			AddOrRemoveSelectPetAndResultItem ((int)objVal);
			DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_DECOMPOSE_PET_ICONS", mSelectPetDatas);
			Refresh (null);
			break;
		case "FORWARD_OR_BACK":
			mCurrent += (int)objVal;
			ShowDecomposeResult ();
			break;
		case "DECOMPOSE_SELECT_PET_AUTO":
			if(mSelectPetDatas.Count  >= mSelcetPetMaxCount)
				break ;

			AddSelectPetAuto ();
			Refresh (null);
			break;
		case "DECOMPOSE_SELECT_PET":
			if(!JudgeMailWillFull())
			{
				tEvent evt = Net.StartEvent("CS_DecomposePet");
				DataBuffer dataBuffer = new DataBuffer(256);
				dataBuffer.write(mSelectPetDatas.Count);
				foreach(PetData d in mSelectPetDatas)
				{
					dataBuffer.write(d.itemId);
				}
				evt.set("DATABUFFER", dataBuffer);
				evt.DoEvent();
			}
			break;
		case "DECOMPOSE_SELECT_PET_RESULT":
			SetVisible ("mask", true);
			for(int i = 0; i < mSelectPetDatas.Count; i++)
			{
				GameObject obj = GetComponent<UIGridContainer>("pet_grid").controlList[i];
				GameCommon.SetUIVisiable (obj, "decompose_effect", true);
			}

			this.StartCoroutine (DecomposeResult());

			break;
		}
	}

	public override bool Refresh (object param)
	{
		ShowSelectPets ();
		ShowDecomposeResult();

		SetVisible ("mask", false);
		GameCommon.GetButtonData (GetSub ("pet_decompose_button")).set ("PET_COUNT", mSelectPetDatas.Count);
		SetText ("NeedCoinNumLabel", mTotalCostNum.ToString ());
		return true;
	}

	public override void Close ()
	{
		ResetData();
		base.Close ();
	}

	void ResetData()
	{
		mSelectPetDatas.Clear ();
		mDecomposeResultItems.Clear ();
		mCurrent = 0;
		mTotalCostNum = 0;
	}

	void ShowSelectPets()
	{
		UIGridContainer petGrid = GetComponent <UIGridContainer>("pet_grid");
		petGrid.MaxCount = mSelcetPetMaxCount;
		for(int i = 0; i < mSelcetPetMaxCount; i++)
		{
			GameObject petCell = petGrid.controlList[i];

			GameCommon.SetUIVisiable (petCell, "decompose_effect", false);
			GameCommon.SetUIVisiable (petCell, "item_icon", false);
			GameCommon.SetUIVisiable (petCell, "txt_label", i == mSelectPetDatas.Count);
			if(mSelectPetDatas.Count == 0 || i >= mSelectPetDatas.Count)
			{
				continue;
			}
			
			PetData data = mSelectPetDatas[i];
			if(data != null)
			{
				GameCommon.SetUIVisiable (petCell, "item_icon", true);
				GameCommon.SetItemIcon (petCell, new ItemData{mID = data.tid, mType = (int)ITEM_TYPE.PET, 
					mLevel = data.level, mStrengthen = data.strengthenLevel});
				
				GameCommon.GetButtonData (petCell, "choose_decompose_pet").set ("DBID", data.itemId);
			}
		}
	}

	void AddOrRemoveSelectPetAndResultItem(int petDBID)
	{
		PetData data = PetLogicData.Self.GetPetDataByItemId (petDBID);
		GetDecomposeResultItems (data.starLevel, data.level);
		if(mbAdd)
			mSelectPetDatas.Add (data);
		else
		{
			foreach(PetData d in mSelectPetDatas)
			{
				if(d.itemId == petDBID)
				{
					mSelectPetDatas.Remove (d);
					return;
				}
			}
		}
	}
	
	void ShowDecomposeResult()
	{
		SetVisible ("decompose_result_back_button", mCurrent != 0);
		SetVisible ("decompose_result_forward_button", (mDecomposeResultItems.Count - (mCurrent + 1) * mSelcetPetMaxCount ) > 0);

		UIGridContainer resultGrid = GetComponent <UIGridContainer>("result_grid");
		resultGrid.MaxCount = mSelcetPetMaxCount;
		for(int j = 0; j < mSelcetPetMaxCount; j++)
		{
			GameObject resultObj = resultGrid.controlList[j];
			GameCommon.SetUIVisiable (resultObj, "item_icon", false);
			
			if(mDecomposeResultItems.Count == 0 || j >= (mDecomposeResultItems.Count - mCurrent * mSelcetPetMaxCount))
			{
				continue;
			}
			
			DecomposeResultItem resultItemData = mDecomposeResultItems[j + mCurrent * mSelcetPetMaxCount];
			if(resultItemData != null)
			{
				GameCommon.SetUIVisiable (resultObj, "item_icon", true);
				
				GameCommon.SetItemIcon (resultObj, new ItemData{mID = resultItemData.mID, mType = resultItemData.mType, 
					mNumber = resultItemData.mNum});
			}
		}
	}

	void GetDecomposeResultItems(int starLevel, int level)
	{
		foreach(KeyValuePair<int, DataRecord> v in DataCenter.mPetDecompose.GetAllRecord ())
		{
			if(v.Value["STAR_LEVEL"] == starLevel && v.Value["LEVEL"] == level)
			{
				for(int i = 1; i < 5; i++)
				{
					string strNum = "ITEM_NUMBER" + i.ToString ();
					if(v.Value[strNum] != 0)
					{
						string strType = "ITEM_TYPE" + i.ToString ();
						string strID = "ITEM_ID" + i.ToString ();
						AddOrRemoveDecomposeResultItem (new DecomposeResultItem{mID = v.Value[strID], mType = v.Value[strType], mNum = v.Value[strNum]});
					}
				}

				AddOrRemoveTotalCostNum(v.Value["COST"]);
			}
		}
	}

	void AddOrRemoveDecomposeResultItem(DecomposeResultItem data)
	{
		if(data.mType == (int)ITEM_TYPE.PET || data.mType == (int)ITEM_TYPE.EQUIP || data.mType == (int)ITEM_TYPE.PET_EQUIP)
		{
			if(mbAdd)	mDecomposeResultItems.Add (data);
			else 
			{
				//mDecomposeResultItems.Remove (data);//error

				foreach(DecomposeResultItem d in mDecomposeResultItems)
				{
					if(d.mType == data.mType && d.mID == data.mID)
					{
						mDecomposeResultItems.Remove (d);
						return;
					}
				}
			}
			return;
		}

		foreach(DecomposeResultItem d in mDecomposeResultItems)
		{
			if(d.mType == data.mType && d.mID == data.mID)
			{
				if(mbAdd) d.mNum += data.mNum;
				else 
				{
					d.mNum -= data.mNum;
					if(d.mNum == 0) mDecomposeResultItems.Remove (d);
				}
				return;
			}
		}

		if(mbAdd)	mDecomposeResultItems.Add (data);
		else mDecomposeResultItems.Remove (data);
	}

	void AddOrRemoveTotalCostNum(int costNum)
	{
		if(mbAdd) mTotalCostNum += costNum;
		else mTotalCostNum -= costNum;
	}

	void AddSelectPetAuto()
	{
		List<PetData> petList = PetLogicData.Self.mDicPetData.Values.ToList();
		petList = SortAutoAddPetList(petList);

		for(int i = 0; i < petList.Count; i++)
		{
			PetData pet = petList[i];
			if(pet != null)
			{
				if(PetLogicData.Self.IsPetUsed(pet.itemId) 
				   || pet.starLevel > 2 
				   || pet.level > 1
				   || pet.strengthenLevel > 0)
					continue;

				if(mSelectPetDatas.Count < mSelcetPetMaxCount && !mSelectPetDatas.Contains (pet))
				{
					mbAdd = true;
					AddOrRemoveSelectPetAndResultItem (pet.itemId);
					DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_DECOMPOSE_PET_ICONS", mSelectPetDatas);
				}
			}
		}
	}

	List<PetData> SortAutoAddPetList(List<PetData> list)
	{
		if(list != null && list.Count > 0)
		{
            list = GameCommon.SortList(list, (a, b) => GameCommon.Sort(a.starLevel, b.starLevel, false));
		}
		
		return list;
	}

	IEnumerator DecomposeResult()
	{
		yield return new WaitForSeconds(0.3f); 
//		yield return new WaitForEndOfFrame();
		GameCommon.RoleChangeNumericalAboutRole ((int)ITEM_TYPE.GOLD, -mTotalCostNum);
		RemoveDecomposePetFromPetLogicData ();
		int addPetCount =0 ;
		int addRoleEquipCount = 0;
		GetCountDecomposeResultItem (ref addPetCount, ref addRoleEquipCount);
		GainNumericalItemFromDecomposeResultItem ();
		if(addPetCount != 0)
		{
			//get pet list from  server
			CS_RequestPetList evt = Net.StartEvent("CS_RequestPetList") as CS_RequestPetList;
            evt.mAction = () => { DataCenter.SetData("BAG_INFO_WINDOW", "UPDATE_DECOMPOSE_PET_ICONS", new List<PetData> { new PetData { itemId = -1 } }); };
			evt.set ("IS_PET_BAG", true);
			evt.DoEvent();
		}
		
		List<ItemData> datas = new List<ItemData>();
		foreach(DecomposeResultItem d in mDecomposeResultItems)
		{
			datas.Add (new ItemData{mID = d.mID, mNumber = d.mNum, mType = d.mType});
		}
		DataCenter.OpenWindow ("GET_DECOMPOSE_RESULT_WINDOW", new ItemDataProvider(datas));
		
		ResetData ();
		Refresh (null);
	}

	void RemoveDecomposePetFromPetLogicData()
	{
		foreach(PetData d in mSelectPetDatas)
		{
			PetLogicData.Self.RemoveItemData (d);
		}
	}

	bool JudgeMailWillFull()
	{
		int addPetCount = 0;
		int addRoleEquipCount = 0;
		GetCountDecomposeResultItem (ref addPetCount, ref addRoleEquipCount);
		int petCount = addPetCount - mSelectPetDatas.Count;
		int maxMailCount = DataCenter.mGlobalConfig.GetData ("MAX_MAIL_COUNT", "VALUE");
//		if(RoleLogicData.Self.mMailNum + (petCount - RoleLogicData.Self.GetFreeSpaceInPetBag ()) > maxMailCount 
//		   || RoleLogicData.Self.mMailNum + (addRoleEquipCount - RoleLogicData.Self.GetFreeSpaceInRoleEquipBag ()) > maxMailCount)
		//不考虑背包的剩余空间（背包数据不是实时同步）
		if(RoleLogicData.Self.mMailNum + addPetCount +addRoleEquipCount > maxMailCount) 
		{
			DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_SHOP_FULL_MAIL);
			return true;
		}

		return false;
	}

	void GetCountDecomposeResultItem(ref int petCount, ref int roleEquipCount)
	{
		foreach(DecomposeResultItem d in mDecomposeResultItems)
		{
			switch(d.mType)
			{
			case(int)ITEM_TYPE.PET:
				petCount += d.mNum;
				break;
			case(int)ITEM_TYPE.EQUIP:
				roleEquipCount += d.mNum;
				break;
			}
		}
	}

	void GainNumericalItemFromDecomposeResultItem()
	{
		foreach(DecomposeResultItem d in mDecomposeResultItems)
		{
			GameCommon.RoleChangeNumericalAboutRole (d.mType, d.mNum);
		}
	}
	
	class DecomposeResultItem
	{
		public int mType;
		public int mNum;
		public int mID;
	}

}


public class Button_choose_decompose_pet : CEvent
{
	public override bool _DoEvent()
	{
		int DBID = (int)getObject ("DBID");
		DataCenter.SetData ("PET_DECOMPOSE_WINDOW", "IS_ADD", false);
		DataCenter.SetData ("PET_DECOMPOSE_WINDOW", "ADD_OR_REMOVE_SELECT_DECOMPOSE_PET", DBID);
		return true;
	}
}

public class Button_pet_decompose_button : CEvent
{
	public override bool _DoEvent()
	{
		int count = (int)getObject ("PET_COUNT");
		if(count == 0) return false; 

		DataCenter.SetData ("PET_DECOMPOSE_WINDOW", "DECOMPOSE_SELECT_PET", true);
		return true;
	}
}

public class Button_pet_decompose_auto_add_pet_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("PET_DECOMPOSE_WINDOW", "DECOMPOSE_SELECT_PET_AUTO", true);
		return true;
	}
}

public class Button_decompose_result_forward_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("PET_DECOMPOSE_WINDOW", "FORWARD_OR_BACK", 1);
		return true;
	}
}

public class Button_decompose_result_back_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("PET_DECOMPOSE_WINDOW", "FORWARD_OR_BACK", -1);
		return true;
	}
}



class CS_DecomposePet : BaseNetEvent
{
	public override void _OnResp(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if (result == (int)STRING_INDEX.ERROR_NONE)
		{
			DataCenter.SetData ("PET_DECOMPOSE_WINDOW", "DECOMPOSE_SELECT_PET_RESULT", true);
		}
		else
			DataCenter.OpenMessageWindow ((STRING_INDEX)result);
	}
}


