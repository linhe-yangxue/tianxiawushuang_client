using UnityEngine;
using System.Collections;
using Logic;

//public class TaskConditionType : MonoBehaviour
//{
//    public static string[] strTaskConditionTypeName;
//
//    // Use this for initialization
//	void Start () {
//	}
//}
//
//public class TaskConditionBase : CEvent
//{
//
//}
//
//
////////////////////////////////////////////////////////////////////////////
///// <summary>
///// 接受任务条件
///// </summary>
//public class AcceptTaskCondition : TaskConditionBase
//{
//    public delegate bool AcceptTaskConditionHandler(int iTaskID, int iTaskAcceptConditionNum);
//    public static AcceptTaskConditionHandler[] acceptTaskConditions = new AcceptTaskConditionHandler[]
//    {
//        NoAcceptTaskCondition,
//        GetDesignatedPetAcceptTaskCondition,
//        LevelAcceptTaskCondition,
//        TaskAcceptTaskCondition
//    };
//
//    /// <summary>
//    /// 无前置条件
//    /// </summary>
//    public static bool NoAcceptTaskCondition(int iTaskID, int iTaskAcceptConditionNum)
//    {
//        return true;
//    }
//
//    /// <summary>
//    /// 获得指定侠客
//    /// </summary>
//    public static bool GetDesignatedPetAcceptTaskCondition(int iTaskID, int iTaskAcceptConditionNum)
//    {
//        PetLogicData logic = DataCenter.GetData("PET_DATA") as PetLogicData;
//        PetData pet = logic.GetPetDataByModelIndex(iTaskAcceptConditionNum);
//        return (pet != null);
//    }
//
//    /// <summary>
//    /// 玩家等级
//    /// </summary>
//    public static bool LevelAcceptTaskCondition(int iTaskID, int iTaskAcceptConditionNum)
//    {
//        //RoleLogicData logic = RoleLogicData.Self;
//
//		return (RoleLogicData.GetMainRole().level >= iTaskAcceptConditionNum);
//    }
//
//    /// <summary>
//    /// 任务
//    /// </summary>
//    public static bool TaskAcceptTaskCondition(int iTaskID, int iTaskAcceptConditionNum)
//    {
//        TaskLogicData logic = DataCenter.GetData("TASK_DATA") as TaskLogicData;
//		TaskData taskData = logic.GetTaskData(iTaskAcceptConditionNum);
//
//        return (taskData != null && taskData.m_TaskState == TASK_STATE.Deliver);
//    }
//}
//
////////////////////////////////////////////////////////////////////////////
///// <summary>
///// 完成任务条件
///// </summary>
//public class FinishTaskCondition : TaskConditionBase
//{
//    public static void FinishTaskItem(TaskData taskData, bool bIsFinish)
//    {
//        if (taskData != null && bIsFinish)
//        {
//            int iNeedNum = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "TASK_NUM");
//            if (taskData.m_iTaskAimCount >= iNeedNum)
//            {
//                taskData.m_iTaskAimCount = iNeedNum;
//                taskData.m_TaskState = TASK_STATE.Finished;
//            }
//        }
//    }
//
//	public delegate bool FinishTaskConditionHandler(TaskData taskData);
//	public static FinishTaskConditionHandler[] finishTaskConditions = new FinishTaskConditionHandler[]
//	{
//		GetProficiencyFinishTaskCondition,
//		PVEWinSatgeFinishTaskCondition,
//		PVEEnterSatgeFinishTaskCondition,
//		PVPFinishStageFinishTaskCondition,
//		KillMonsterFinishTaskCondition,
//		PVPRankingFinishTaskCondition,
//		GetGlodFinishTaskCondition,
//		GetGemFinishTaskCondition,
//		GameFriendNumFinishTaskCondition,
//		PlanFriendNumFinishTaskCondition,
//		PetNumFinishTaskCondition,
//		SendFriendlyNumFinishTaskCondition,
//		PetUpgradeNumFinishTaskCondition,
//		PetStrengthenNumFinishTaskCondition,
//		PetEvolutionNumFinishTaskCondition,
//		SalePetNumFinishTaskCondition,
//		RoleLevelUpFinishTaskCondition,
//		RoleGetStarNumFinishTaskCondition
//	};
//
//	/// <summary>
//	/// 获得熟练度
//	/// </summary>
//	public static bool GetProficiencyFinishTaskCondition(TaskData taskData)
//	{
//        bool bIsFinish = false;
//        if (taskData != null)
//        {
//            tLogicData logicData = DataCenter.GetData("TASK_NEED_DATA");
//            bool bIsWin = logicData.get("ISWIN");
//
//            if (bIsWin)
//            {
//                // 任意关卡熟练度到顶级
//                bIsFinish = true;
//
//                int iStageID = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "STAGE_ID");
//                if (iStageID != 0)
//                {
//                    // 指定关卡熟练度到顶级
//                    int stageIndex = logicData.get("MAP_ID");
//                    if (iStageID != stageIndex)
//                    {
//                        bIsFinish = false;
//                    }
//                }
//            }            
//
//            if (bIsFinish)
//                taskData.m_iTaskAimCount++;
//        }
//
//        return bIsFinish;
//	}
//
//	/// <summary>
//	/// 通关关卡
//	/// </summary>
//    public static bool PVEWinSatgeFinishTaskCondition(TaskData taskData)
//    {
//        bool bIsFinish = false;
//        if (taskData != null)
//        {
//            tLogicData logicData = DataCenter.GetData("TASK_NEED_DATA");
//            bool bIsWin = logicData.get("ISWIN");
//            if (bIsWin)
//            {
//                // 通关任意关卡
//                bIsFinish = true;
//
//                int iStageID = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "STAGE_ID");
//                if (iStageID != 0)
//                {
//                    // 通关指定关卡
//                    int stageIndex = logicData.get("MAP_ID");
//                    if (iStageID != stageIndex)
//                    {
//                        bIsFinish = false;
//                    }
//                }
//            }
//
//            if (bIsFinish)
//                taskData.m_iTaskAimCount++;
//        }
//        return bIsFinish;
//    }
//
//	/// <summary>
//	/// 进入关卡
//	/// </summary>
//	public static bool PVEEnterSatgeFinishTaskCondition(TaskData taskData)
//	{
//        bool bIsFinish = false;
//        if (taskData != null)
//        {
//            // 进入任意关卡
//            bIsFinish = true;
//            tLogicData logicData = DataCenter.GetData("TASK_NEED_DATA");
//            
//            int iStageID = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "STAGE_ID");
//            if (iStageID != 0)
//            {
//                // 进入指定关卡
//                int stageIndex = logicData.get("MAP_ID");
//                if (iStageID != stageIndex)
//                {
//                    bIsFinish = false;
//                }
//            }
//
//            if (bIsFinish)
//                taskData.m_iTaskAimCount++;
//        }
//        return bIsFinish;
//	}
//
//	/// <summary>
//	/// 完成PVP
//	/// </summary>
//	public static bool PVPFinishStageFinishTaskCondition(TaskData taskData)
//	{
//		return false;
//	}
//
//	/// <summary>
//	/// 杀死怪物
//	/// </summary>
//	public static bool KillMonsterFinishTaskCondition(TaskData taskData)
//	{
//        bool bIsFinish = false;
//        if (taskData != null)
//        {
//            tLogicData logicData = DataCenter.GetData("TASK_NEED_DATA");
//            int iStageID = logicData.get("MAP_ID");
//            string strMonsterType = TableCommon.GetStringFromTaskConfig(taskData.m_iTaskID, "MONSTER_TYPE");
//
//            int iMonsterNum = TableCommon.GetNumberFromStageConfig(iStageID, "MONSTER_NUM");
//            bool bIsWin = logicData.get("ISWIN");
//
//            bIsFinish = true;
//
//            if (strMonsterType == "MONSTER")
//            {
//                // 杀死小怪数量
//                if (bIsWin)
//                {
//                    taskData.m_iTaskAimCount += iMonsterNum;
//                }
//                else
//                {
//                    taskData.m_iTaskAimCount += 3;
//                }
//            }
//            else if (strMonsterType == "MONSTER_BOSS")
//            {
//                if (bIsWin)
//                {
//                    // 杀死BOSS数量
//                    taskData.m_iTaskAimCount += 1;
//                }
//                else
//                {
//                    bIsFinish = false;
//                }
//            }
//			else
//			{
//				// 杀死(小怪 + BOSS)数量
//				if (bIsWin)
//				{
//					taskData.m_iTaskAimCount += (iMonsterNum + 1);
//				}
//				else
//				{
//					taskData.m_iTaskAimCount += 3;
//				}
//			}
//        }
//        return bIsFinish;
//	}
//
//	/// <summary>
//	/// PVP排名
//	/// </summary>
//	public static bool PVPRankingFinishTaskCondition(TaskData taskData)
//	{
//		return false;
//	}
//
//	/// <summary>
//	/// 收集金币
//	/// </summary>
//	public static bool GetGlodFinishTaskCondition(TaskData taskData)
//	{
//        bool bIsFinish = false;
//        if (taskData != null)
//        {
//            tLogicData logicData = DataCenter.GetData("TASK_NEED_DATA");
//
//            int iGold = logicData.get("GOLD");
//            if (iGold > 0)
//            {
//                taskData.m_iTaskAimCount += iGold;
//                bIsFinish = true;
//            }
//        }
//        return bIsFinish;
//	}
//
//	/// <summary>
//	/// 收集灵石
//	/// </summary>
//	public static bool GetGemFinishTaskCondition(TaskData taskData)
//	{
//        bool bIsFinish = false;
//        if (taskData != null)
//        {
//            tLogicData logicData = DataCenter.GetData("TASK_NEED_DATA");            
//            int iNum = logicData.get("NUM");
//			if(iNum > 0)
//			{
//				bIsFinish = true;
//
//				int iIndex = logicData.get("ITEM_ID");
//				int iGemType = iIndex - 1000;
//				
//				int iConditionGenIndex = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "GEM_ID");
//				if(iConditionGenIndex >= 0)
//				{
//					GemLogicData gemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
//					if(iGemType >= 0
//					   && iGemType < gemLogicData.mGemList.Length
//					   && iConditionGenIndex != iGemType)
//					{
//						bIsFinish = false;
//					}
//				}
//				
//				int iConditionElementType = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "ELEMENT_TYPE");
//
//				if (iConditionElementType >= 0)
//				{
//					int iElementType = iGemType/3;
//					if (iConditionElementType != iElementType)
//						bIsFinish = false;
//				}
//			}
//
//			if(bIsFinish)
//				taskData.m_iTaskAimCount += iNum;
//            
//        }
//        return bIsFinish;
//    }
//
//	/// <summary>
//	/// 游戏好友数量
//	/// </summary>
//	public static bool GameFriendNumFinishTaskCondition(TaskData taskData)
//	{
//		return false;
//	}
//	/// <summary>
//	/// 平台好友数量
//	/// </summary>
//	public static bool PlanFriendNumFinishTaskCondition(TaskData taskData)
//	{
//		return false;
//	}
//	/// <summary>
//	/// 累计符灵数量
//	/// </summary>
//	public static bool PetNumFinishTaskCondition(TaskData taskData)
//	{
//        bool bIsFinish = false;
//        if (taskData != null)
//        {
//            bIsFinish = true;
//            tLogicData logicData = DataCenter.GetData("TASK_NEED_DATA");
//            int iIndex = logicData.get("ITEM_ID");
//            int iNum = logicData.get("NUM");
//
//            int iConditionStarLevel = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "STAR_NUM");
//            int iConditionElementType = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "ELEMENT_TYPE");
//
//            int iStarLevel = TableCommon.GetNumberFromActiveCongfig(iIndex, "STAR_LEVEL");
//            int iElementType = TableCommon.GetNumberFromActiveCongfig(iIndex, "ELEMENT_INDEX");
//
//            if (iConditionStarLevel != 0)
//            {
//                if (iConditionStarLevel != iStarLevel)
//                    bIsFinish = false;
//            }
//
//            if (iConditionElementType >= 0)
//            {
//                if (iConditionElementType != iElementType)
//                    bIsFinish = false;
//            }
//
//            if (bIsFinish)
//            {
//                taskData.m_iTaskAimCount += iNum;
//            }
//
//        }
//        return bIsFinish;
//	}
//	/// <summary>
//	/// 赠送好友度次数
//	/// </summary>
//    public static bool SendFriendlyNumFinishTaskCondition(TaskData taskData)
//	{
//		return false;
//	}
//	/// <summary>
//	/// 符灵升级次数
//	/// </summary>
//	public static bool PetUpgradeNumFinishTaskCondition(TaskData taskData)
//    {
//        bool bIsFinish = false;
//        if (taskData != null)
//        {
//            bIsFinish = true;
//            tLogicData logicData = DataCenter.GetData("TASK_NEED_DATA");
//            int iNum = logicData.get("NUM");
//            
//            if (bIsFinish)
//            {
//                taskData.m_iTaskAimCount += iNum;
//            }
//        }
//        return bIsFinish;
//	}
//
//	/// <summary>
//	/// 符灵强化次数
//	/// </summary>
//	public static bool PetStrengthenNumFinishTaskCondition(TaskData taskData)
//    {
//        bool bIsFinish = false;
//        if (taskData != null)
//        {
//            bIsFinish = true;
//            tLogicData logicData = DataCenter.GetData("TASK_NEED_DATA");
//            int iNum = logicData.get("NUM");
//
//            int iConditionStrengthenLevel = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "STRENGTHEN_LEVEL");
//            if (iConditionStrengthenLevel != 0)
//            {
//                int iStrengthenLevel = logicData.get("PET_STRENGTHEN_LEVEL");
//                if (iConditionStrengthenLevel != iStrengthenLevel)
//                    bIsFinish = false;
//            }
//            else
//            {
//				int iStrengthenResult = (int)logicData.get("PET_STRENGTHEN_RESULT");
//				int iConditionStrengthenResult = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "STRAENGTHEN_SUCCEED");
//				if (iStrengthenResult != iConditionStrengthenResult)
//					bIsFinish = false;
//            }
//
//            if (bIsFinish)
//            {
//                taskData.m_iTaskAimCount += iNum;
//            }
//        }
//        return bIsFinish;
//    }
//
//	/// <summary>
//	/// 符灵进化次数
//	/// </summary>
//	public static bool PetEvolutionNumFinishTaskCondition(TaskData taskData)
//	{
//        bool bIsFinish = false;
//        if (taskData != null)
//        {
//            bIsFinish = true;
//            tLogicData logicData = DataCenter.GetData("TASK_NEED_DATA");            
//            int iNum = logicData.get("NUM");
//
//            int iConditionStarLevel = TableCommon.GetNumberFromTaskConfig(taskData.m_iTaskID, "STAR_NUM");
//
//            if (iConditionStarLevel != 0)
//            {
//                int iIndex = logicData.get("ITEM_ID");
//                int iStarLevel = TableCommon.GetNumberFromActiveCongfig(iIndex, "STAR_LEVEL");
//                if (iConditionStarLevel != iStarLevel)
//                    bIsFinish = false;
//            }
//
//            if (bIsFinish)
//            {
//                taskData.m_iTaskAimCount += iNum;
//            }
//        }
//        return bIsFinish;
//	}
//
//	/// <summary>
//	/// 出售符灵次数
//	/// </summary>
//	public static bool SalePetNumFinishTaskCondition(TaskData taskData)
//	{
//		return false;
//	}
//
//	/// <summary>
//	/// 符灵强化成功次数
//	/// </summary>
//	public static bool PetStrengthenVictoryNumFinishTaskCondition(TaskData taskData)
//	{
//		return false;
//	}
//
//	/// <summary>
//	/// 玩家升级
//	/// </summary>
//	public static bool RoleLevelUpFinishTaskCondition(TaskData taskData)
//	{
//        bool bIsFinish = false;
//        if (taskData != null)
//        {
//            tLogicData logicData = DataCenter.GetData("TASK_NEED_DATA");
//
//            int iDLevel = logicData.get("ROLE_LEVEL_UP");
//            if (iDLevel > 0)
//            {
//				taskData.m_iTaskAimCount += iDLevel;
//                bIsFinish = true;
//            }
//        }
//        return bIsFinish;
//	}
//
//	/// <summary>
//	/// 玩家获得指定星级数量
//	/// </summary>	
//	public static bool RoleGetStarNumFinishTaskCondition(TaskData taskData)
//	{
//		return false;
//	}
//
//
//}