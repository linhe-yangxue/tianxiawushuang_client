using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;

public class MissionUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		DataCenter.RegisterData("MissionWindow", new MissionWindow(gameObject));
		DataCenter.OpenWindow ("BACK_GROUP_MISSION_WINDOW");

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDestroy()
	{
		DataCenter.CloseWindow ("BACK_GROUP_MISSION_WINDOW");
		DataCenter.Remove("MissionWindow");
	}
}


public class MissionWindow : tWindow
{
    private const int TASK_ICON_WIDTH = 85;
    private const int TASK_ICON_HEIGHT = 85;
    private const int AWARD_ICON_WIDTH = 58;
    private const int AWARD_ICON_HEIGHT = 48;
    private const int AWARD_ICON_WIDTH_S = 40;
    private const int AWARD_ICON_HEIGHT_S = 40;

    GameObject mAchievmentTaskBtn;
    GameObject mWeeklyTaskBtn;
	GameObject mDailyTaskBtn;
    GameObject mActivityTasksBtn;

    Dictionary<TASK_PAGE_TYPE, GameObject> mDicTaskTabBtn = new Dictionary<TASK_PAGE_TYPE, GameObject>();

    TASK_PAGE_TYPE mCurPageType = TASK_PAGE_TYPE.DAILY;
    GameObject mCurPageContext;
    UIGridContainer mCurPageGrid;
    GameObject mCurGridItemObj;
    private bool onInit = true;

	public MissionWindow(GameObject obj)
	{
		mGameObjUI = obj;
	}

	public override void Init()
	{
        mAchievmentTaskBtn = mGameObjUI.transform.Find("TabBtnPanel/TabBtn/achievment_tasks_btn").gameObject;
        mWeeklyTaskBtn = mGameObjUI.transform.Find("TabBtnPanel/TabBtn/weekly_tasks_btn").gameObject;
        mDailyTaskBtn = mGameObjUI.transform.Find("TabBtnPanel/TabBtn/daily_tasks_btn").gameObject;
        mActivityTasksBtn = mGameObjUI.transform.Find("TabBtnPanel/TabBtn/activity_tasks_btn").gameObject;

        mDicTaskTabBtn.Add(TASK_PAGE_TYPE.DAILY, mDailyTaskBtn);
        mDicTaskTabBtn.Add(TASK_PAGE_TYPE.ACHIEVEMENT, mAchievmentTaskBtn);
        mDicTaskTabBtn.Add(TASK_PAGE_TYPE.WEEKLY, mWeeklyTaskBtn);
        mDicTaskTabBtn.Add(TASK_PAGE_TYPE.ACTIVITY, mActivityTasksBtn);

        tEvent evt = Net.StartEvent("CS_RequestMissionData");
        evt.DoEvent();
	}

	public override void Open (object param)
	{
		base.Open (param);
	}

    public override bool Refresh(object param)
	{
        RefreshPriorPage();
        GuideManager.Notify(GuideIndex.EnterTaskWindow);
        GuideManager.Notify(GuideIndex.AcceptTaskAward);
		return true;
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
		switch (keyIndex)
		{
		case "SET_SEL_PAGE":
			SetSelectTaskBtn((TASK_PAGE_TYPE)objVal);
			break;
        case "ACCEPT_TASK_AWARD":
			AcceptTaskAward((int)objVal);
			break;
		}
	}

	public void SetSelectTaskBtn(TASK_PAGE_TYPE taskPageType)
	{
        GameCommon.ToggleTrue(mDicTaskTabBtn[taskPageType]);
        SetUIInfo();
	}

    public TASK_PAGE_TYPE GetCurPageType()
    {
        foreach (KeyValuePair<TASK_PAGE_TYPE, GameObject> iter in mDicTaskTabBtn)
        {
            UIToggle uiToggle = iter.Value.GetComponent<UIToggle>();
            if (uiToggle.value)
                return iter.Key;
        }

        return TASK_PAGE_TYPE.DAILY;
    }

    public void SetUIInfo()
    {
        TaskLogicData taskLogicData = TaskLogicData.Instance;
		if(taskLogicData == null)
			return;

        SetAllCanDeliverTaskNum(taskLogicData);
        mCurPageType = GetCurPageType();
        Dictionary<int, TaskData> dicTask = taskLogicData.m_dicTask[mCurPageType];
        RemoveOvertimeTask(dicTask);
        //TaskSystemMgr.Self.PrintTaskByPage(mCurPageType);
        GameObject btnObj = GetTaskPageBtn(mCurPageType);
        UIToggledObjects uiToggledObjects = btnObj.GetComponent<UIToggledObjects>();
        mCurPageContext = uiToggledObjects.activate[0];
        //mCurPageContext = mGameObjUI.transform.Find("TabContextPanel/ScrollviewContext_" + ((int)mCurPageType).ToString()).gameObject;

        GameObject scrollBarObj = GameCommon.FindObject(mCurPageContext, "Scroll Bar");
        UIScrollBar scrollBar = scrollBarObj.GetComponent<UIScrollBar>();
        float scrollBarValue = scrollBar.value;

		mCurPageGrid = mCurPageContext.transform.Find("ScrollviewContext/ScrollView/Grid").GetComponent<UIGridContainer>();

		//int iCount = 0;
		//mCurPageGrid.MaxCount = dicTask.Count;
        List<TaskData> taskDatas = new List<TaskData>();

        foreach (KeyValuePair<int, TaskData> iter in dicTask)
        {
            if (iter.Value.m_TaskState == TASK_STATE.Finished)
            {
                //InitContextUI(iCount, iter.Value);
                //iCount++;
                taskDatas.Add(iter.Value);
            }
        }
        foreach (KeyValuePair<int, TaskData> iter in dicTask)
        {
            if (iter.Value.m_TaskState != TASK_STATE.Finished)
            {
                //InitContextUI(iCount, iter.Value);
                //iCount++;
                taskDatas.Add(iter.Value);
            }
        }

        if (mCurPageGrid.MaxCount == 0)
        {
            mCurPageGrid.SetMaxCountAsync(taskDatas.Count, 1, i => InitContextUI(i, taskDatas[i]), null);
        }
        else 
        {
            mCurPageGrid.MaxCount = taskDatas.Count;

            for (int i = 0; i < taskDatas.Count; ++i)
            {
                InitContextUI(i, taskDatas[i]);
            }

            scrollBar.value = scrollBarValue;
        }       

		GameObject npcObj = GameCommon.FindObject (mGameObjUI, "npc");
		npcObj.GetComponent<TweenPosition>().delay = (float)dicTask.Count/(100);
		npcObj.GetComponent<TweenPosition>().enabled = true;
    }

    public void SetAllCanDeliverTaskNum(TaskLogicData taskLogicData)
    {
        SetCanDeliverTaskNum(taskLogicData, TASK_PAGE_TYPE.DAILY);
        SetCanDeliverTaskNum(taskLogicData, TASK_PAGE_TYPE.WEEKLY);
        SetCanDeliverTaskNum(taskLogicData, TASK_PAGE_TYPE.ACHIEVEMENT);
        SetCanDeliverTaskNum(taskLogicData, TASK_PAGE_TYPE.ACTIVITY);
    }

    public void SetCanDeliverTaskNum(TaskLogicData taskLogicData, TASK_PAGE_TYPE page)
    {
        Dictionary<int, TaskData> dicTask = taskLogicData.m_dicTask[page];
        if (dicTask != null)
        {
            int count = 0;
            foreach (KeyValuePair<int, TaskData> pair in dicTask)
            {
                if (pair.Value.m_TaskState == TASK_STATE.Finished)
                {
                    ++count;
                }
            }
            SetCanDeliverTaskNum(page, count);
            return;
        }
        SetCanDeliverTaskNum(page, 0);
    }

    public void SetCanDeliverTaskNum(TASK_PAGE_TYPE page, int num)
    {
        GameObject btnObj = GetTaskPageBtn(page);
        if (btnObj != null)
        {
            GameObject logoObj = GameCommon.FindObject(btnObj, "achievment_tasks_num_icon");
            if (num == 0)
            {
                logoObj.SetActive(false);
            }
            else 
            {
                logoObj.SetActive(true);
                GameCommon.SetUIText(logoObj, "achievment_tasks_num", num.ToString());
            }
        }
    }

	public void InitContextUI(int iIndex, TaskData taskData)
    {
		if (iIndex < mCurPageGrid.MaxCount && taskData != null)
        {
            mCurGridItemObj = mCurPageGrid.controlList[iIndex];

            // task state
            SetTaskState(taskData);
           
            // award
            SetTaskAward(taskData.m_iTaskID);

            // task icon
            SetTaskIcon(taskData.m_iTaskID);

            // discription
            SetTaskDiscription(taskData);

			// name
            SetTaskName(taskData.m_iTaskID);
            //UILabel lab_name = mCurGridItemObj.transform.Find("lab_name").gameObject.GetComponent<UILabel>();
            //lab_name.text = TableCommon.GetStringFromTaskConfig(taskData.m_iTaskID, "TASK_NAME");
        }
    }

    /// <summary>
    /// Sets the state of the task.
    /// </summary>
    /// <param name="taskData">Task data.</param>
    public void SetTaskState(TaskData taskData)
    {       
        if (mCurGridItemObj == null)
        {
            DEBUG.LogError("SetTaskState : mCurGridItemObj == null");
            return;
        }

		GameObject but_get = mCurGridItemObj.transform.Find("but_get_task_award").gameObject;
		UILabel label = but_get.GetComponent<UILabel>();
		label.text = taskData.m_iTaskID.ToString();
		TASK_STATE taskType = taskData.m_TaskState;
        GameObject complete = mCurGridItemObj.transform.Find("complete").gameObject;
        UIImageButton imageBtn = but_get.GetComponent<UIImageButton>();

		GameObject not_complete = mCurGridItemObj.transform.Find("no_complete_icon").gameObject;
		GameObject btn_go_task = mCurGridItemObj.transform.Find("go_task_btn").gameObject;
		UILabel btn_go_task_label = btn_go_task.GetComponent<UILabel>();
		btn_go_task_label.text = TableCommon.GetNumberFromTaskConfig (taskData.m_iTaskID,"TASK_TYPE").ToString ();

        switch (taskType)
        {
            case TASK_STATE.Deliver:
                but_get.SetActive(false);
                not_complete.SetActive(false);
                complete.SetActive(true);
				btn_go_task.SetActive(false);
                break;
            case TASK_STATE.Finished:
                but_get.SetActive(true);
                not_complete.SetActive(false);
                complete.SetActive(false);
				btn_go_task.SetActive(false);
                break;
            case TASK_STATE.Had_Accept:
                but_get.SetActive(false);
                not_complete.SetActive(false );
                complete.SetActive(false);
				btn_go_task.SetActive(true );
                break;
            case TASK_STATE.Can_Not_Accept:
            default:
                but_get.SetActive(false);
                not_complete.SetActive(true);
                complete.SetActive(false);
				btn_go_task.SetActive(false);
                break;
        }
        //if (taskType == TASK_STATE.Deliver)
        //{
        //    but_get.SetActive(false);
        //    complete.SetActive(true);
        //}
        //else if(taskType == TASK_STATE.Finished)
        //{
        //    but_get.SetActive(true);
        //    complete.SetActive(false);
        //    UIImageButton imageBtn = but_get.GetComponent<UIImageButton>();
        //    imageBtn.isEnabled = false;
        //    if (taskType == TASK_STATE.Finished)
        //    {
        //        imageBtn.isEnabled = true;
        //    }
        //}
    }

    /// <summary>
    /// task award
    /// </summary>
    /// <param name="iTaskID"></param>
    public void SetTaskAward(int iTaskID)
    {
        if (mCurGridItemObj == null)
        {
            DEBUG.LogError("SetAward : mCurGridItemObj == null");
            return;
        }
       
        GameObject gp_award = mCurGridItemObj.transform.Find("gp_award").gameObject;
        // award icon
        UISprite award_icon = gp_award.transform.Find("icon").gameObject.GetComponent<UISprite>();
        string strAtlasName = TableCommon.GetStringFromTaskConfig(iTaskID, "AWARD_ICON_ATLAS_NAME");
        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
		string strSpriteName = TableCommon.GetStringFromTaskConfig(iTaskID, "AWARD_ICON_SPRITE_NAME");
        award_icon.atlas = tu;
        award_icon.spriteName = strSpriteName;
        //award_icon.MakePixelPerfect();        
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
		GameCommon.SetUIVisiable (mCurGridItemObj, "icon_background_sprite", bIsRoleEquip);
		UISprite award_icon_background = gp_award.transform.Find("icon_background_sprite").gameObject.GetComponent<UISprite>();

        award_icon.width = AWARD_ICON_WIDTH_S;
        award_icon.height = AWARD_ICON_HEIGHT_S;
		award_icon_background.width = AWARD_ICON_WIDTH_S;
		award_icon_background.height = AWARD_ICON_WIDTH_S;

        //if (iTaskAwardType == (int)ITEM_TYPE.PET || iTaskAwardType == (int)ITEM_TYPE.ROLE_EQUIP || iTaskAwardType == (int)ITEM_TYPE.GEM)
        //{
        //    award_icon.width = AWARD_ICON_WIDTH_S;
        //    award_icon.height = AWARD_ICON_HEIGHT_S;
        //}
        //else
        //{
        //    award_icon.width = AWARD_ICON_WIDTH;
        //    award_icon.height = AWARD_ICON_HEIGHT;
        //}

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

    /// <summary>
    /// task icon
    /// </summary>
    /// <param name="iTaskID"></param>
    public void SetTaskIcon(int iTaskID)
    {
        UISprite mission_icon = mCurGridItemObj.transform.Find("mission_icon").gameObject.GetComponent<UISprite>();
		string strAtlasName = TableCommon.GetStringFromTaskConfig(iTaskID, "ICON_ATLAS_NAME");
        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
		string strSpriteName = TableCommon.GetStringFromTaskConfig(iTaskID, "ICON_SPRITE_NAME");
        mission_icon.atlas = tu;
        mission_icon.spriteName = strSpriteName;
        //mission_icon.MakePixelPerfect();
        mission_icon.width = TASK_ICON_WIDTH;
        mission_icon.height = TASK_ICON_HEIGHT;

		int iTaskAwardType = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_AWARD_TYPE");
		bool bIsFragment = iTaskAwardType == (int)ITEM_TYPE.PET_FRAGMENT ? true : false;
		UISprite award_icon_fragment = mCurGridItemObj.transform.Find("mission_icon_fragment").gameObject.GetComponent<UISprite>();
		award_icon_fragment.gameObject.SetActive (bIsFragment);
		if(bIsFragment)
		{
			int iFragmentID = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_AWARD_ID");
			int iItemID = TableCommon.GetNumberFromFragment(iFragmentID, "ITEM_ID");
			int elementIndex = TableCommon.GetNumberFromActiveCongfig(iItemID, "ELEMENT_INDEX");
			GameCommon.SetElementFragmentIcon (mCurGridItemObj, "mission_icon_fragment", elementIndex);
		}

		bool bIsRoleEquip = iTaskAwardType == (int)ITEM_TYPE.EQUIP ? true : false;
		GameCommon.SetUIVisiable (mCurGridItemObj, "mission_icon_background_sprite", bIsRoleEquip);
    }

    /// <summary>
    /// set task discription
    /// </summary>
    /// <param name="taskData"></param>
    public void SetTaskDiscription(TaskData taskData)
    {
        UILabel lab_info = mCurGridItemObj.transform.Find("lab_info").gameObject.GetComponent<UILabel>();
        string strInfo = TableCommon.GetStringFromTaskConfig(taskData.m_iTaskID, "TASK_TIP");
        int iNeedNum = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "TASK_NUM");
        int iCurrentNum = taskData.m_iTaskAimCount > iNeedNum ? iNeedNum : taskData.m_iTaskAimCount;
        lab_info.text = string.Format(strInfo, iCurrentNum, iNeedNum);
    }

    public void SetTaskName(int iTaskID)
    {
        UILabel lab_name = mCurGridItemObj.transform.Find("lab_name").gameObject.GetComponent<UILabel>();
        lab_name.text = TableCommon.GetStringFromTaskConfig(iTaskID, "TASK_NAME");
        int remainDays = GetRemainDays(iTaskID);

        if (remainDays > 0)
            lab_name.text += "（剩余" + remainDays + "天）";
    }

    public void AcceptTaskAward(int iTaskID)
    {
        int iTaskAwardType = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_AWARD_TYPE");
        int iNum = TableCommon.GetNumberFromTaskConfig(iTaskID, "TASK_AWARD_NUM");

        switch (iTaskAwardType)
        {
            case (int)ITEM_TYPE.GOLD:
                GameCommon.RoleChangeGold(iNum);
                break;

            case (int)ITEM_TYPE.POWER:
                GameCommon.RoleChangeStamina(iNum);
                break;

            case (int)ITEM_TYPE.YUANBAO:
                GameCommon.RoleChangeDiamond(iNum);
                break;

            case (int)ITEM_TYPE.SPIRIT:
                GameCommon.RoleChangeSpirit(iNum);
                break;
        }
    }

    private GameObject GetTaskPageBtn(TASK_PAGE_TYPE page)
    {
        string btnName = "";
        switch (page)
        {
            case TASK_PAGE_TYPE.DAILY:
                btnName = "daily_tasks_btn";
                break;
            case TASK_PAGE_TYPE.WEEKLY:
                btnName = "weekly_tasks_btn";
                break;
            case TASK_PAGE_TYPE.ACHIEVEMENT:
                btnName = "achievment_tasks_btn";
                break;
            case TASK_PAGE_TYPE.ACTIVITY:
                btnName = "activity_tasks_btn";
                break;
        }
        if (!string.IsNullOrEmpty(btnName))
        {
            return GameCommon.FindObject(mGameObjUI, btnName);
        }
        return null;
    }

    private void RefreshPriorPage()
    {
        if (onInit)
        {
            SetSelectTaskBtn(GetPriorPageType());
            onInit = false;
        }
        SetUIInfo(); 
    }

    private TASK_PAGE_TYPE GetPriorPageType()
    {
        TaskLogicData taskLogicData = TaskLogicData.Instance;
        if (taskLogicData.CheckWhetherHadFinishTask(TASK_PAGE_TYPE.DAILY))
        {
            return TASK_PAGE_TYPE.DAILY;
        }
        else if (taskLogicData.CheckWhetherHadFinishTask(TASK_PAGE_TYPE.WEEKLY))
        {
            return TASK_PAGE_TYPE.WEEKLY;
        }
        else if (taskLogicData.CheckWhetherHadFinishTask(TASK_PAGE_TYPE.ACHIEVEMENT))
        {
            return TASK_PAGE_TYPE.ACHIEVEMENT;
        }
        else if (taskLogicData.CheckWhetherHadFinishTask(TASK_PAGE_TYPE.ACTIVITY))
        {
            return TASK_PAGE_TYPE.ACTIVITY;
        }
        else if (taskLogicData.CheckWhetherHadAcceptTask(TASK_PAGE_TYPE.DAILY))
        {
            return TASK_PAGE_TYPE.DAILY;
        }
        else if (taskLogicData.CheckWhetherHadAcceptTask(TASK_PAGE_TYPE.WEEKLY))
        {
            return TASK_PAGE_TYPE.WEEKLY;
        }
        else if (taskLogicData.CheckWhetherHadAcceptTask(TASK_PAGE_TYPE.ACHIEVEMENT))
        {
            return TASK_PAGE_TYPE.ACHIEVEMENT;
        }
        else if (taskLogicData.CheckWhetherHadAcceptTask(TASK_PAGE_TYPE.ACTIVITY))
        {
            return TASK_PAGE_TYPE.ACTIVITY;
        }
        return TASK_PAGE_TYPE.DAILY;
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

    private void RemoveOvertimeTask(Dictionary<int, TaskData> dic)
    {
        GameCommon.RemoveFromDictionary<TaskData>(dic, x => GetRemainDays(x.m_iTaskID) == 0);
    }
}
