using UnityEngine;
using System;
using System.Collections.Generic;
using DataTable;


public enum TASK_PAGE
{
    DAILY = 1,      // 每日任务
    ACHIEVEMENT = 2 // 成就任务
}


public class TaskProperty
{
    public static TASK_PAGE GetTaskPage(int taskId)
    {
        int t = taskId / 1000000;
        return t == 3 ? TASK_PAGE.DAILY : TASK_PAGE.ACHIEVEMENT;
    }

    public static List<TaskProperty> GetPropertyList(TASK_PAGE page, Predicate<TaskProperty> match)
    {
        List<TaskProperty> list = new List<TaskProperty>();
        NiceTable table = page == TASK_PAGE.DAILY ? DataCenter.mTaskConfig : DataCenter.mAchieveConfig;

        foreach (KeyValuePair<int, DataRecord> pair in table.GetAllRecord())
        {
            int key = pair.Key;

            if (key > 0)
            {
                TaskProperty p = new TaskProperty(key);

                if (match(p))
                {
                    list.Add(p);
                }
            }
        }

        return list;
    }

    public static int DefaultComparison(TaskProperty lhs, TaskProperty rhs)
    {
        if (lhs == rhs)
        {
            return 0;
        }
        else if (lhs.accepted)
        {
            return 1;
        }
        else if (rhs.accepted)
        {
            return -1;
        }
        else
        {
            float lhsValue = lhs.progress / (float)lhs.aim;
            float rhsValue = rhs.progress / (float)rhs.aim;

            if (lhsValue > rhsValue)
            {
                return -1;
            }
            else if (lhsValue < rhsValue)
            {
                return 1;
            }
            else 
            {
                return lhs.taskId - rhs.taskId;
            }
        }
    }

    public static int GetDailyTaskAwardNeedScore(int index)
    {
        DataRecord r = DataCenter.mScoreConfig.GetRecord(index);
        return r["NUM"];
    }

    public static List<ItemDataBase> GetDailyTaskScoreAwardItems(int index)
    {
        DataRecord r = DataCenter.mScoreConfig.GetRecord(index);
        string str = r["AWARD_ITEM"];
        return GameCommon.ParseItemList(str);
    }

    public static bool HasAwardAcceptable(TASK_PAGE p)
    {
        if (p == TASK_PAGE.DAILY)
        {
            for (int i = 1; i <= 4; ++i)
            {
                if (TaskLogicData.Instance.curScore >= GetDailyTaskAwardNeedScore(i) && !TaskLogicData.Instance.IsScoreAwardAccepted(i))
                {
                    return true;
                }
            }

            List<TaskProperty> daily = GetPropertyList(TASK_PAGE.DAILY, x => x.visible && x.completed && !x.accepted);
            return daily.Count > 0;
        }
        else
        {
            List<TaskProperty> achieve = GetPropertyList(TASK_PAGE.ACHIEVEMENT, x => x.visible && x.completed && !x.accepted);
            return achieve.Count > 0;
        }
    }

    public TASK_PAGE page { get; private set; }
    public int taskId { get; private set; }
    public bool visible { get; private set; }
    public int scoreAward { get; private set; }
    public int progress { get; private set; }
    public int aim { get; private set; }
    public bool completed { get; private set; }
    public bool accepted { get; private set; }
    public int gotoindex { get; private set; }  //> 前往索引 added by xuke

    private DataRecord mRecord;

    public TaskProperty(int taskId)
    {
        this.taskId = taskId;
        page = GetTaskPage(taskId);
        TaskObject taskObj;

        if (page == TASK_PAGE.DAILY)
        {
            mRecord = DataCenter.mTaskConfig.GetRecord(taskId);
            taskObj = TaskLogicData.Instance.GetDailyTaskObject(taskId);
            scoreAward = mRecord["SCORE_NUM"];
        }
        else
        {
            mRecord = DataCenter.mAchieveConfig.GetRecord(taskId);
            taskObj = TaskLogicData.Instance.GetAchievementTaskObject(taskId);
            scoreAward = 0;
        }

        int curLevel = RoleLogicData.GetMainRole().level;
        int minLevel = mRecord["TASK_SHOW_LVMIN"];
        int maxLevel = mRecord["TASK_SHOW_LVMAX"];
        visible = minLevel <= curLevel && curLevel <= maxLevel && taskObj != null;

        aim = (int)mRecord["NUM"];

        if (aim == 0)
        {
            aim = (int)mRecord["LEVEL"];
        }

        if (aim == 0)
        {
            aim = 1;
        }

        //if (page == TASK_PAGE.DAILY)
        //{
        //    int curLevel = RoleLogicData.GetMainRole().level;
        //    int minLevel = mRecord["TASK_SHOW_LVMIN"];
        //    int maxLevel = mRecord["TASK_SHOW_LVMAX"];
        //    visible = minLevel <= curLevel && curLevel <= maxLevel && taskObj != null;
        //}
        //else 
        //{
        //    int curLevel = RoleLogicData.GetMainRole().level;
        //    int minLevel = mRecord["TASK_SHOW_LVMIN"];
        //    int maxLevel = mRecord["TASK_SHOW_LVMAX"];
        //    int preTask = mRecord["EX_TASK"];

        //    if (minLevel <= curLevel && curLevel <= maxLevel)
        //    {
        //        if (preTask == 0 || taskObj != null)
        //        {
        //            visible = true;
        //        }
        //        else
        //        {
        //            TaskObject preObj = TaskLogicData.Instance.GetAchievementTaskObject(preTask);

        //            if (preObj == null)
        //            {
        //                visible = false;
        //            }
        //            else
        //            {
        //                visible = preObj.accepted == 1;

        //                if (visible)
        //                {
        //                    taskObj = new TaskObject() { taskId = taskId, progress = preObj.progress, accepted = 0 };
        //                    TaskLogicData.Instance.AddOrUpdateAchievementTaskObject(taskObj);
        //                }
        //            }
        //        }
        //    }
        //    else 
        //    {
        //        visible = false;
        //    }
        //}

        if (taskObj != null)
        {
            progress = Mathf.Min(taskObj.progress, aim);
            completed = progress >= aim;
            accepted = taskObj.accepted == 1;
        }
        else 
        {
            progress = 0;
            completed = false;
            accepted = false;
        }

        //added by xuke begin
        //test
        gotoindex = (int)mRecord["ID"];
        //end
    }

    public List<ItemDataBase> awardItems
    {
        get
        {
            string itemStr = mRecord["AWARD_ITEM"];
            return GameCommon.ParseItemList(itemStr);
        }
    }

    public string taskDesc
    {
        get { return mRecord["TASK_NAME"]; }
    }

    public string awardDesc
    {
        get { return mRecord["AWARD_TIP"]; }
    }

    public string taskIconAtlas
    {
        get { return mRecord["ICON_ATLAS_NAME"]; }
    }

    public string taskIconSprite
    {
        get { return mRecord["ICON_SPRITE_NAME"]; }
    }

	public int taskIconTid
	{
		get { return mRecord["Item_ID"];}
	}
}


public class TaskState
{
    public static bool hasDailyAwardAcceptable { get; set; }
    public static bool hasAchieveAwardAcceptable { get; set; }
    public static bool shouldDailyRefresh { get; set; }
    public static bool shouldAchieveRefresh { get; set; }

    public static bool hasAwardAcceptable
    {
        get { return hasDailyAwardAcceptable || hasAchieveAwardAcceptable; }
        set { hasDailyAwardAcceptable = value; hasAchieveAwardAcceptable = value; }
    }

    public static bool shouldRefresh
    {
        get { return shouldDailyRefresh || shouldAchieveRefresh; }
        set { shouldDailyRefresh = value; shouldAchieveRefresh = value; }
    }
}