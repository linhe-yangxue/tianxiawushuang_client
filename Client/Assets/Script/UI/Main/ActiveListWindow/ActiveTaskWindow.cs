using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

public class ActiveTaskWindow: ActiveTotalWindow
{
	private const int TASK_ICON_WIDTH = 85;
	private const int TASK_ICON_HEIGHT = 85;
	private const int AWARD_ICON_WIDTH = 58;
	private const int AWARD_ICON_HEIGHT = 48;
	private const int AWARD_ICON_WIDTH_S = 40;
	private const int AWARD_ICON_HEIGHT_S = 40;

	List<DataRecord> mRecords = new List<DataRecord>();
	public override void Init ()
	{
		EventCenter.Register ("Button_active_task_child_window_button", new DefineFactory<Button_active_task_child_window_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);
		mDesLabelName = "tips_label";
		mCountdownLabelName = "active_achievement_task_rest_time";

		mRecords = TableCommon.FindAllRecords (DataCenter.mActiveTaskConfig, lhs => {return lhs["EVENT_ID"] == mConfigIndex;});
	}

	public override void OnOpen ()
	{
		base.OnOpen ();
		RefreshTab();

		tEvent evt = Net.StartEvent("CS_RequestMissionData");
		evt.set ("ACTIVE_INDEX", GetFirstActiveIndex ());
		evt.DoEvent();
//		RequestTaskChildActiveData (GetFirstActiveIndex());
	}
	
	void RefreshTab()
	{
		UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(GetSub ("button_scrollview_context"), "grid");
		grid.MaxCount  = GetChildActive ();
		int index = 0;
		foreach(var v in mRecords)
		{
			GameObject subCell = grid.controlList[index];
			subCell.transform.name = "active_task_child_window_button";

			if(index == 0) GameCommon.ToggleTrue (subCell);

			string title = v["TITLE"];
			foreach (UILabel l in subCell.GetComponentsInChildren<UILabel>())
			{
				if(l != null) l.text = title;
			}
			GameCommon.GetButtonData (subCell).set ("ACTIVE_TASK_INDEX", (int)v["INDEX"]);
			
			index ++;
		}
	}

	int GetChildActive()
	{
		int count = 0;
		foreach(var v in DataCenter.mActiveTaskConfig.Records ())
		{
			if(v["INDEX"] != 0) count ++;
		}
		
		return count;
	}

	int GetFirstActiveIndex()
	{
		foreach(var v in DataCenter.mOperateEventConfig.Records ())
		{
			int index = v["INDEX"];
			if(index != 0) return index;
		}
		return 0;
	}

	void RequestTaskChildActiveData(int activeTaskConfigIndex)
	{
		int taskStart = DataCenter.mActiveTaskConfig.GetData (activeTaskConfigIndex, "TASK_START");
		int taskEnd = DataCenter.mActiveTaskConfig.GetData (activeTaskConfigIndex, "TASK_END");

        TaskLogicData taskLogicData = TaskLogicData.Instance;
		if(taskLogicData == null) return;

		Dictionary<int, TaskData> dicTask = taskLogicData.m_dicTask[TASK_PAGE_TYPE.ACTIVITY];
		RemoveOvertimeTask(dicTask);
		List<TaskData> taskDatas = new List<TaskData>();
		foreach (KeyValuePair<int, TaskData> iter in dicTask)
		{
			if (iter.Value.m_TaskState == TASK_STATE.Finished)
			{
				for(int i = taskStart; i < taskEnd; i++)
				{
					if(iter.Value.m_iTaskID == i && !taskDatas.Contains(iter.Value))
						taskDatas.Add(iter.Value);
				}
			}
		}
		foreach (KeyValuePair<int, TaskData> iter in dicTask)
		{
			if (iter.Value.m_TaskState != TASK_STATE.Finished)
			{
				for(int i = taskStart; i < taskEnd; i++)
				{
					if(iter.Value.m_iTaskID == i && !taskDatas.Contains(iter.Value))
						taskDatas.Add(iter.Value);
				}
			}
		}

		UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(GetSub ("ScrollviewContext"), "grid");
		if (grid.MaxCount == 0)
		{
			grid.SetMaxCountAsync(taskDatas.Count, 1, i => InitContextUI(grid.controlList[i], taskDatas[i]), null);
		}
		else 
		{
			grid.MaxCount = taskDatas.Count;
			for (int i = 0; i < taskDatas.Count; ++i)
			{
				InitContextUI(grid.controlList[i], taskDatas[i]);
			}
		}       
	}

	private void RemoveOvertimeTask(Dictionary<int, TaskData> dic)
	{
		GameCommon.RemoveFromDictionary<TaskData>(dic, x => GetRemainDays(x.m_iTaskID) == 0);
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
		case"REQUSET_CHILD_ACTIVE_DATA":
				RequestTaskChildActiveData((int)objVal);
				break;
		}
	}

	public void InitContextUI(GameObject subCell, TaskData taskData)
	{		
		SetTaskState(subCell, taskData);
		SetTaskAward(subCell, taskData.m_iTaskID);
		SetTaskIcon(subCell, taskData.m_iTaskID);
		SetTaskDiscription(subCell, taskData);
		SetTaskName(subCell, taskData.m_iTaskID);
	}

	public void SetTaskState(GameObject subCell,TaskData taskData)
	{       
		GameObject but_get = subCell.transform.Find("but_get_task_award").gameObject;
		UILabel label = but_get.GetComponent<UILabel>();
		label.text = taskData.m_iTaskID.ToString();
		TASK_STATE taskType = taskData.m_TaskState;
		GameObject complete = subCell.transform.Find("complete").gameObject;
		GameObject not_complete = subCell.transform.Find("no_complete_icon").gameObject;
		UIImageButton imageBtn = but_get.GetComponent<UIImageButton>();
		
		switch (taskType)
		{
		case TASK_STATE.Deliver:
			but_get.SetActive(false);
			not_complete.SetActive(false);
			complete.SetActive(true);
			break;
		case TASK_STATE.Finished:
			but_get.SetActive(true);
			not_complete.SetActive(false);
			complete.SetActive(false);
			break;
		case TASK_STATE.Had_Accept:
			but_get.SetActive(false);
			not_complete.SetActive(true);
			complete.SetActive(false);
			break;
		case TASK_STATE.Can_Not_Accept:
		default:
			but_get.SetActive(false);
			not_complete.SetActive(false);
			complete.SetActive(false);
			break;
		}
	}

	public void SetTaskAward(GameObject subCell, int iTaskID)
	{
		GameObject gp_award = subCell.transform.Find("gp_award").gameObject;
		UISprite award_icon = gp_award.transform.Find("icon").gameObject.GetComponent<UISprite>();
		string strAtlasName = TableCommon.GetStringFromTaskConfig(iTaskID, "AWARD_ICON_ATLAS_NAME");
		UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
		string strSpriteName = TableCommon.GetStringFromTaskConfig(iTaskID, "AWARD_ICON_SPRITE_NAME");
		award_icon.atlas = tu;
		award_icon.spriteName = strSpriteName;  
		int iTaskAwardType = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_AWARD_TYPE");
		
		bool bIsFragment = iTaskAwardType == (int)ITEM_TYPE.PET_FRAGMENT ? true : false;
		UISprite award_icon_fragment = gp_award.transform.Find("icon_fragment").gameObject.GetComponent<UISprite>();
		award_icon_fragment.gameObject.SetActive (bIsFragment);
		if(bIsFragment)
		{
			int iFragmentID = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_AWARD_ID");
			int iItemID = TableCommon.GetNumberFromFragment(iFragmentID, "ITEM_ID");
			int elementIndex = TableCommon.GetNumberFromActiveCongfig(iItemID, "ELEMENT_INDEX");
			GameCommon.SetElementFragmentIcon (gp_award, "icon_fragment", elementIndex);
		}
		
		bool bIsRoleEquip = iTaskAwardType == (int)ITEM_TYPE.EQUIP ? true : false;
		GameCommon.SetUIVisiable (subCell, "icon_background_sprite", bIsRoleEquip);
		UISprite award_icon_background = gp_award.transform.Find("icon_background_sprite").gameObject.GetComponent<UISprite>();
		
		award_icon.width = AWARD_ICON_WIDTH_S;
		award_icon.height = AWARD_ICON_HEIGHT_S;
		award_icon_background.width = AWARD_ICON_WIDTH_S;
		award_icon_background.height = AWARD_ICON_WIDTH_S;

		UILabel lab_award = gp_award.transform.Find("lab_award").gameObject.GetComponent<UILabel>();
		
		if (iTaskAwardType == (int)ITEM_TYPE.PET)
		{
			int petIndex = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_AWARD_ID");
			string petName = TableCommon.GetStringFromActiveCongfig(petIndex, "NAME");
			lab_award.text = petName;
		}
		else if (iTaskAwardType == (int)ITEM_TYPE.EQUIP)
		{
			int equipIndex = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_AWARD_ID");
			string equipName = TableCommon.GetStringFromRoleEquipConfig(equipIndex, "NAME");
			lab_award.text = equipName;
		}
		else
		{
			int awardNumber = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_AWARD_NUM");
			lab_award.text = awardNumber.ToString();           
		}
	}

	public void SetTaskIcon(GameObject subCell, int iTaskID)
	{
		UISprite mission_icon = subCell.transform.Find("mission_icon").gameObject.GetComponent<UISprite>();
		string strAtlasName = TableCommon.GetStringFromTaskConfig(iTaskID, "ICON_ATLAS_NAME");
		UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
		string strSpriteName = TableCommon.GetStringFromTaskConfig(iTaskID, "ICON_SPRITE_NAME");
		mission_icon.atlas = tu;
		mission_icon.spriteName = strSpriteName;
		mission_icon.width = TASK_ICON_WIDTH;
		mission_icon.height = TASK_ICON_HEIGHT;
		
		int iTaskAwardType = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_AWARD_TYPE");
		bool bIsFragment = iTaskAwardType == (int)ITEM_TYPE.PET_FRAGMENT ? true : false;
		UISprite award_icon_fragment = subCell.transform.Find("mission_icon_fragment").gameObject.GetComponent<UISprite>();
		award_icon_fragment.gameObject.SetActive (bIsFragment);
		if(bIsFragment)
		{
			int iFragmentID = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_AWARD_ID");
			int iItemID = TableCommon.GetNumberFromFragment(iFragmentID, "ITEM_ID");
			int elementIndex = TableCommon.GetNumberFromActiveCongfig(iItemID, "ELEMENT_INDEX");
			GameCommon.SetElementFragmentIcon (subCell, "mission_icon_fragment", elementIndex);
		}
		
		bool bIsRoleEquip = iTaskAwardType == (int)ITEM_TYPE.EQUIP ? true : false;
		GameCommon.SetUIVisiable (subCell, "mission_icon_background_sprite", bIsRoleEquip);
	}

	public void SetTaskDiscription(GameObject subCell, TaskData taskData)
	{
		UILabel lab_info = subCell.transform.Find("lab_info").gameObject.GetComponent<UILabel>();
		string strInfo = TableCommon.GetStringFromTaskConfig(taskData.m_iTaskID, "TASK_TIP");
		int iNeedNum = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "TASK_NUM");
		int iCurrentNum = taskData.m_iTaskAimCount > iNeedNum ? iNeedNum : taskData.m_iTaskAimCount;
		lab_info.text = string.Format(strInfo, iCurrentNum, iNeedNum);
	}
	
	public void SetTaskName(GameObject subCell, int iTaskID)
	{
		UILabel lab_name = subCell.transform.Find("lab_name").gameObject.GetComponent<UILabel>();
		lab_name.text = TableCommon.GetStringFromTaskConfig(iTaskID, "TASK_NAME");
		int remainDays = GetRemainDays(iTaskID);
		
		if (remainDays > 0)
			lab_name.text += "（剩余" + remainDays + "天）";
	}

	// No time limit : -1; 
	// Not in active duration : 0; 
	// In active duration : 1, 2, ......
	private int GetRemainDays(int iTaskID)
	{
		string strStartTime = TableCommon.GetStringFromTaskConfig(iTaskID, "TASK_START_TIME");
		string strEndTime = TableCommon.GetStringFromTaskConfig(iTaskID, "TASK_END_TIME");
		
		if (strEndTime == "")
			return -1;
		
		DateTime startTime = GetDateTimeFromString(strStartTime);
		DateTime endTime = GetDateTimeFromString(strEndTime);
		DateTime now = GameCommon.NowDateTime();
		
		if (now.CompareTo(startTime) < 0 || now.CompareTo(endTime) > 0)
			return 0;
		
		TimeSpan span = endTime - now;
		return span.Days + 1;
	}

	private DateTime GetDateTimeFromString(string time)
	{
		string[] result = time.Split('_');
		
		if (result.Length != 3)
			return new DateTime();
		else
			return new DateTime(int.Parse(result[0]), int.Parse(result[1]), int.Parse(result[2]));
	}

}


public class Button_active_task_child_window_button : CEvent
{
	public override bool _DoEvent()
	{
		int activeTaskIndex = (int)getObject ("ACTIVE_TASK_INDEX");
		DataCenter.SetData ("ACTIVE_TASK_WINDOW", "REQUSET_CHILD_ACTIVE_DATA", activeTaskIndex);
		return true;
	}
}