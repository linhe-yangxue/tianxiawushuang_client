using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;
using System.Linq;


public enum TACTICAL_FORMATION_STATE
{
	UNACTIVE,
	CAN_ACTIVE,
	ACTIVE,
}

public enum TACTICAL_FORMATION_TYPE
{
	ELEMENT = 1,
	PET,
}

public class TacticalFormationWindow : tWindow {

	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_close_tactical_formation_window_button", new DefineFactory<TacticalFormationWindowCloseBtn>());
		EventCenter.Self.RegisterEvent("Button_activate_button", new DefineFactory<TacticalFormationActivateBtn>());
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
//		if(keyIndex == "")
//		{
//
//		}
	}
	public override void OnOpen()
	{
		base.OnOpen();

		Refresh(null);
	}

	public override bool Refresh (object param)
	{
		UpdateTacticalFormationUI();
		return true;
	}

	public void UpdateTacticalFormationUI()
	{
		UIGridContainer grid = mGameObjUI.transform.Find("bg/scroll_wiew/grid").GetComponent<UIGridContainer>();
		if(grid != null)
		{
			int iIndex = 0;
			grid.MaxCount = DataCenter.mTacticalFormation.GetAllRecord().Count - 1;
			
			// update from server data
			UpdateTacticalFormationUIFromServerData(grid, ref iIndex);
			
			// update from table
			UpdateTacticalFormationUIFromTable(grid, ref iIndex);
		}

	}

	public void UpdateTacticalFormationUIFromServerData(UIGridContainer grid, ref int iIndex)
	{

	}

	public void UpdateTacticalFormationUIFromTable(UIGridContainer grid, ref int iIndex)
	{
		foreach(KeyValuePair<int, DataRecord> pair in DataCenter.mTacticalFormation.GetAllRecord())
		{
			if(pair.Key > 0)
			{
				GameObject obj = grid.controlList[iIndex];
				DataRecord record = pair.Value;
				if(obj != null && record != null)
				{
					// if not in server data
					if(false)
					{
						return;
					}
					else
					{
						UpdateUI(obj, record, TACTICAL_FORMATION_STATE.UNACTIVE);
						iIndex++;
					}
				}
			}
		}

	}

	public void UpdateUI(GameObject obj, DataRecord record, TACTICAL_FORMATION_STATE state)
	{
		SetActiveState(obj, record, ref state);
		SetInfo(obj, record, state);
	}

	public void SetActiveState(GameObject obj, DataRecord record, ref TACTICAL_FORMATION_STATE state)
	{
		int iType = (int)record["TEAM_TYPE"];
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		if(state != TACTICAL_FORMATION_STATE.ACTIVE)
		{
			if(iType == (int)TACTICAL_FORMATION_TYPE.ELEMENT)
			{
				int iElementIndex = (int)record["ELEMENT_ID"];
				int iCount = 0;
				foreach(KeyValuePair<int, PetData> iter in petLogicData.mDicPetData)
				{
					PetData pet = iter.Value;
					if(pet != null && TableCommon.GetNumberFromActiveCongfig(pet.tid, "ELEMENT_INDEX") == iElementIndex)
					{
						iCount++;
					}

					if(iCount >= 3)
					{
						state = TACTICAL_FORMATION_STATE.CAN_ACTIVE;
						break;
					}
				}
			}
			else if(iType == (int)TACTICAL_FORMATION_TYPE.PET)
			{
				
			}
		}

		obj.transform.Find("active_group").gameObject.SetActive(state == TACTICAL_FORMATION_STATE.ACTIVE);
		obj.transform.Find("can_active_group").gameObject.SetActive(state == TACTICAL_FORMATION_STATE.CAN_ACTIVE);
		obj.transform.Find("unactive_group").gameObject.SetActive(state == TACTICAL_FORMATION_STATE.UNACTIVE);

		obj.transform.Find("activate_button").GetComponent<UIImageButton>().isEnabled = (state != TACTICAL_FORMATION_STATE.UNACTIVE);
	}

	public void SetInfo(GameObject obj, DataRecord record, TACTICAL_FORMATION_STATE state)
	{
		SetCoin(obj, record, state);
		SetName(obj, record);
		SetDescription(obj, record);
	}

	public void SetCoin(GameObject obj, DataRecord record, TACTICAL_FORMATION_STATE state)
	{
		int iType = record["TEAM_TYPE"];
		UIGridContainer grid = obj.transform.Find("icon_grid").GetComponent<UIGridContainer>();
		for(int i = 0; i < grid.MaxCount; i++)
		{
			GameObject iconObj = grid.controlList[i];
			if(iType == (int)TACTICAL_FORMATION_TYPE.ELEMENT)
			{
				int iELEMENT = record["ELEMENT_ID"];
				
				GameCommon.SetElementIcon(iconObj, "tactical_formation_need_icon", iELEMENT);
			}
			else if(iType == (int)TACTICAL_FORMATION_TYPE.PET)
			{
				GameCommon.SetPetIcon(iconObj, record["TEAM_PET_" + i.ToString()], "tactical_formation_need_icon");
			}

			iconObj.transform.Find("tactical_formation_need_icon_black").gameObject.SetActive(state == TACTICAL_FORMATION_STATE.UNACTIVE);
		}
	}

	public void SetName(GameObject obj, DataRecord record)
	{
		if(record != null)
		{
			UILabel label = obj.transform.Find("name_label").GetComponent<UILabel>();
			string str = record["TEAM_NAME"];
			label.text = str.Replace("\\n", "\n");
		}
		else
		{
			DEBUG.LogError("record is null");
		}
	}

	public void SetDescription(GameObject obj, DataRecord record)
	{
		if(record != null)
		{
			UILabel label = obj.transform.Find("info_label").GetComponent<UILabel>();
			string str = record["DESCRIPTION"];
			label.text = str.Replace("\\n", "\n");
		}
		else
		{
			DEBUG.LogError("record is null");
		}
	}
}

public class TacticalFormationWindowCloseBtn : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("TACTICAL_FORMATION_WINDOW");
		return true;
	}
}

public class TacticalFormationActivateBtn : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.CloseWindow("TACTICAL_FORMATION_WINDOW");
		EventCenter.Start("Button_pet_info_auto_join_button").DoEvent ();
		return true;
	}
}