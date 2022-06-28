using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;

//public class TaskObj{
//
//}
//
//
//public class TaskSystemMgr
//{
//    public static TaskSystemMgr Self = new TaskSystemMgr();
//    
//    //private List<int> m_waitForNofityAchieveTaskID = new List<int>();
//
//    public bool CheckCanAcceptTasksByTaskType(Task_Accept_Condition taskAcceptConditionType)
//    {
//        TaskLogicData logic = DataCenter.GetData("TASK_DATA") as TaskLogicData;
//        Dictionary<int, DataRecord> dicAllTask = DataCenter.mTaskConfig.GetAllRecord();
//        foreach (KeyValuePair<int, DataRecord> iter in dicAllTask)
//        {
//			if(iter.Key == 0)
//				continue;
//            if (logic.GetTaskData(iter.Key) == null)
//            {
//                CheckCanAcceptTask(iter.Key, taskAcceptConditionType);
//            }
//        }
//        return true;
//    }
//
//    public void CheckCanAcceptTask(int iTaskID, Task_Accept_Condition taskAcceptConditionType)
//    {
//		TaskLogicData logic = DataCenter.GetData("TASK_DATA") as TaskLogicData;
//        int iTaskAcceptConditionType = TableCommon.GetNumberFromTaskConfig(iTaskID, "EX_TASK_TYPE");
//		TASK_PAGE_TYPE taskPageType = logic.GetTaskPageType(iTaskID);
//        if (iTaskAcceptConditionType == (int)taskAcceptConditionType
//		    && taskPageType == TASK_PAGE_TYPE.ACHIEVEMENT)
//        {
//            int iTaskAcceptConditionNum = TableCommon.GetNumberFromTaskConfig(iTaskID, "EX_TASK_NUM");
//            bool bIsCan = AcceptTaskCondition.acceptTaskConditions[iTaskAcceptConditionType](iTaskID, iTaskAcceptConditionNum);
//            if (bIsCan)
//            {
//                
//                logic.CreateTaskData(iTaskID);
//            }
//        }
//    }
//
//    public bool CheckFinishAllTaskSubentryByTaskType(TASK_TYPE taskType)
//    {
//        TaskLogicData logic = DataCenter.GetData("TASK_DATA") as TaskLogicData;
//        foreach (KeyValuePair<TASK_PAGE_TYPE, Dictionary<int, TaskData>> tempIter in logic.m_dicTask)
//        {
//            // 不是成就已交付任务
//            if (tempIter.Key != TASK_PAGE_TYPE.ACHIEVEMENT_DILIVER)
//            {
//                foreach (KeyValuePair<int, TaskData> iter in tempIter.Value)
//                {
//                    TaskData taskData = iter.Value;
//                    if (taskData.m_TaskState == TASK_STATE.Had_Accept)
//                    {
//                        CheckFinishTaskSubentryByTaskType(taskType, taskData);
//                    }
//                }
//            }
//        }
//        return true;
//    }
//
//    public void CheckFinishTaskSubentryByTaskType(TASK_TYPE taskType, TaskData taskData)
//    {
//        if (taskData == null)
//            return;
//
//        int iTaskType = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "TASK_TYPE");
//        if (iTaskType == (int)taskType)
//        {
//            bool bIsFinish = FinishTaskCondition.finishTaskConditions[iTaskType](taskData);
//            FinishTaskCondition.FinishTaskItem(taskData, bIsFinish);
//        }
//    }
//
//	public void DeliverTask(int iTaskID)
//	{
//		TaskLogicData logic = DataCenter.GetData("TASK_DATA") as TaskLogicData;
//		TaskData taskData = logic.GetTaskData(iTaskID);
//		if(taskData != null)
//		{
//			taskData.m_TaskState = TASK_STATE.Deliver;
//			logic.UpdateTaskInfo(taskData);
//		}
//	}
////     public void CheckAllTasks()
////     {
////         Dictionary<int, DataRecord> dicAllTask = DataCenter.mTaskConfig.GetAllRecord();
////         foreach (KeyValuePair<int, DataRecord> iter in dicAllTask)
////         {
////             CheckCanAcceptTask(iter.Key);
////             //CheckFinishTaskSubentry(iter.Value);
////         }
////     }
//}