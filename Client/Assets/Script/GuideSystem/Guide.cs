using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;
using DataTable;
using Logic;


// 各层级设置
// message_window           155
// GuideTip                 157
// Finger                   158
// guide_base_mask_window   160
// guide_mask_window        165
// prologue_window          168
// guide_dialog_window      170
// guide_skip_window        172
// loading_window           180
// message_help_window      300

public class Guide
{
    public static readonly int NEW_START_INDEX = 1001;
    public static readonly int MAX_INDEX = 10000;
    public static readonly float DEFAULT_PROTECT_TIME = 6f;
    public static float protectTime = 6f;

    private static GameObject guideHelperObject = null;
    public static GuideProcess currentGuide = null;
    public static int serverIndex = 0;
    public static int serverReloadIndex = 0;

    private static Dictionary<int, int> levelTriggerMap = new Dictionary<int, int>();
    private static Dictionary<int, int> stageTriggerMap = new Dictionary<int, int>();
    private static Dictionary<int, int> stageLevelTriggerMap = new Dictionary<int, int>();
    private static Dictionary<int, int> stageTaskTriggerMap = new Dictionary<int, int>();
    //by chenliang
    //begin

    private static bool mEnableGuide = true;        //是否开启新手引导
    private static bool mEnablePrologue = true;     //是否开启序章假战斗

    public static bool EnableGuide
    {
        set { mEnableGuide = value; }
        get { return mEnableGuide; }
    }
    public static bool EnablePrologue
    {
        set { mEnablePrologue = value; }
        get { return mEnablePrologue; }
    }

    //end

    /// <summary>
    /// 引导辅助GameObject，用于开启协程
    /// </summary>
    public static GameObject helper
    {
        get 
        {
            if (guideHelperObject == null)
            {
                guideHelperObject = new GameObject("guide_helper");
                GameObject.DontDestroyOnLoad(guideHelperObject);
            }

            return guideHelperObject;
        }
    }

    /// <summary>
    /// 是否处于引导流程中
    /// </summary>
    public static bool isActive
    {
        get 
        {
            return Routine.IsActive(currentGuide);
        }
    }

    /// <summary>
    /// 判定引导是否被锁定
    /// 如果引导长时间被锁定，则意味着引导流程出现异常，此时需要强制结束引导
    /// 被锁定的条件是：
    /// 1 引导基础遮罩已打开但操作遮罩未打开 或者 操作遮罩打开但无响应区域
    /// 2 引导对话窗口关闭 （不处于对话剧情状态下）
    /// 3 没有网络遮罩（不处于等待网络回复的状态下）
    /// 4 不处于等待关卡战斗开始阶段
    /// 5 掉线重连或网络错误窗口等高层级窗口未打开
    /// </summary>
    /// <returns> 引导是否被锁定 </returns>
    public static bool isLocked
    {
        get
        {
            return ((GuideBaseMaskWindow.isOpened && !GuideMaskWindow.isOpened) || GuideMaskWindow.isLocked)
                && !GuideDialogWindow.isOpened 
                && (Net.msUIWaitEffect == null || Net.msUIWaitEffect.waiting == 0)
                && (MainProcess.mStage == null || MainProcess.mStage.mbBattleActive)
                && (!MessageWindow.isOpened || MessageWindow.depth < 180)
                && (!MessageOkWindow.isOpened || MessageOkWindow.depth < 180)
                && !AstrologyTipWindow.isOpened
                && !NetErrorWindow.isOpened
                && !ConnectErrorWindow.isOpened;
        }
    }

    public static bool resetLockedTimeFlag = false;

    /// <summary>
    /// PVE战斗结束后是否手动结算，该标识位在战斗结束后会被重置为false
    /// </summary>
    public static bool manualAccountOnGuideStageFinish
    {
        get;
        set;
    }

    /// <summary>
    /// 是否开启新手引导
    /// </summary>
    public static bool isOpened
    {
        get 
        {
            //by chenliang
            //begin

//             string val = ((string)DataCenter.mGlobalConfig.GetData("ENABLE_GUIDE", "VALUE")).ToLower();
//             return val != "no";
//--------------------
            if (HotUpdateLoading.IsUseHotUpdate)
                return mEnableGuide;
            else
            {
                string val = ((string)DataCenter.mGlobalConfig.GetData("ENABLE_GUIDE", "VALUE")).ToLower();
                return val != "no";
            }

            //end
        }
    }

    /// <summary>
    /// 是否开启序章假战斗
    /// </summary>
    public static bool isPrologueOpened
    {
        get 
        {
            if (isOpened)
            {
                //by chenliang
                //begin

//                 string val = ((string)DataCenter.mGlobalConfig.GetData("ENABLE_PROLOGUE", "VALUE")).ToLower();
//                 return val != "no";
//--------------------
                if (HotUpdateLoading.IsUseHotUpdate)
                    return mEnablePrologue;
                else
                {
                    string val = ((string)DataCenter.mGlobalConfig.GetData("ENABLE_PROLOGUE", "VALUE")).ToLower();
                    return val != "no";
                }

                //end
            }

            return false;
        }
    }


    /// <summary>
    /// 是否位于序章中
    /// </summary>
    public static bool inPrologue
    {
        get;
        private set;
    }

    /// <summary>
    /// 开启引导，登陆时调用
    /// </summary>
    public static void Start()
    {
        serverIndex = 0;
        serverReloadIndex = 0;
        Terminate(false);

        levelTriggerMap = GroupConfigByTrigger(GUIDE_TRIGGER_TYPE.LEVEL_UP, "level");
        RoleLogicData.mOnMainRoleLevelUp -= OnLevelUp;
        RoleLogicData.mOnMainRoleLevelUp += OnLevelUp;

        stageTriggerMap = GroupConfigByTrigger(GUIDE_TRIGGER_TYPE.CLEAR_STAGE, "stage_id");
        MapLogicData.mOnFirstClearStage -= OnFirstClearStage;
        MapLogicData.mOnFirstClearStage += OnFirstClearStage;

        stageLevelTriggerMap = GroupConfigByTrigger(GUIDE_TRIGGER_TYPE.CLEAR_STAGE_LEVEL_UP, "stage_id");
        BattleMainAccountRequester.onAccount -= OnFirstClearStageAndLevelUp;
        BattleMainAccountRequester.onAccount += OnFirstClearStageAndLevelUp;

        stageTaskTriggerMap = GroupConfigByTrigger(GUIDE_TRIGGER_TYPE.STAGE_TASK_REWARD, "task_id");
        TotalTaskWindow.onRewardReceived -= OnStageTaskReward;
        TotalTaskWindow.onRewardReceived += OnStageTaskReward;

        helper.StartRoutine(DoSyncAndGuide());
    }

    /// <summary>
    /// 开启新用户假战斗引导
    /// </summary>
    public static void StartPrologue()
    {
        helper.StartRoutine(DoPrologue());
    }

    /// <summary>
    /// 强行终止引导
    /// </summary>
    /// <param name="commit"> 是否提交终结索引 </param>
    public static void Terminate(bool commit)
    {
        inPrologue = false;

        helper.BreakAllRoutines();

        GuideKit.CloseBaseMask();
        GuideKit.CloseMask();
        GuideKit.CloseDialog();
        GuideKit.CloseCursor();
        GuideKit.CloseTip();
        GuideKit.ClosePrologueWindowImmediate();
        //GuideKit.CloseSkipPrologueWindowImmediate();

        if (MainProcess.mStage != null && MainProcess.mStage.mbBattleActive && !MainProcess.mStage.mbBattleFinish)
        {
            MainProcess.mStage.DisableAI();
            MainProcess.mStage.EnableAI();
            GuideKit.SetPVEBattleSettingEnabled(true);
            GuideKit.SetBossBattleSettingEnabled(true);
        }

        if (commit)
        {
            CommitTerminateIndex();
        }
    }

    /// <summary>
    /// 提交终结索引
    /// </summary>
    public static void CommitTerminateIndex()
    {
        GuideKit.SaveProgressByLocal(MAX_INDEX);
        helper.StartRoutine(new SaveProgressByServerRoutine(MAX_INDEX));
    }

    /// <summary>
    /// 初始化假数据，用于假战斗
    /// </summary>
    public static void InitPrologueData(int roleIndex, int[] petIndex)
    {
        RoleLogicData r = new RoleLogicData();
        r.mRoleList = new RoleData[(int)CREATE_ROLE_TYPE.max];
        DataCenter.RegisterData("ROLE_DATA", r);

        r.character = new RoleData();
        r.character.tid = roleIndex;
        r.character.teamPos = 0;
        r.character.level = 1;

        PetLogicData p = new PetLogicData();
        DataCenter.RegisterData("PET_DATA", p);
        
        if (petIndex != null)
        {
            for (int i = 0; i < petIndex.Length; ++i)
            {
                if (petIndex[i] > 0)
                {                
                    PetData d = new PetData();
                    d.tid = petIndex[i];
                    d.itemNum = 1;
                    d.itemId = i + 1;
                    d.teamPos = i + 1;
                    PetLogicData.Self.AddItemData(d);
                }
            }
        }   

        RoleEquipLogicData e = new RoleEquipLogicData();
        DataCenter.RegisterData("EQUIP_DATA", e);
        e.mDicEquip.Clear();

        MagicLogicData m = new MagicLogicData();       
        DataCenter.RegisterData("MAGIC_DATA", m);

        TeamManager.InitTeamListData();

        Relationship.Init();
    }

    /// <summary>
    /// 跳过序章漫画，只有当前处于序章漫画时有效
    /// </summary>
    /// <returns> 当前是否处于序章漫画阶段 </returns>
    public static bool SkipPrologue()
    {
        if (Routine.IsActive(currentGuide))
        {
            return currentGuide.SkipPrologue();
        }

        return false;
    }

    private static IEnumerator DoSyncAndGuide()
    {
        if (!string.IsNullOrEmpty(GmWindow.UsedGM) && GmWindow.UsedGM == CommonParam.mUId)
        {
            // 如果是GM账号，不进行强制引导
            yield return new SaveProgressRoutine(Guide.MAX_INDEX);
            GmWindow.UsedGM = null;
            yield break;
        }

        int reloadIndex = 0;
        var sync = new SyncGuideProgress();
        yield return sync;

        if (sync.isNewPlayer)
        {
            reloadIndex = NEW_START_INDEX;
        }
        else
        {
            DataRecord r = DataCenter.mBeginnerConfig.GetRecord(sync.finalIndex);
            reloadIndex = r == null ? MAX_INDEX : r["RELOAD_INDEX"];
        }

        DataRecord loadConfig = DataCenter.mBeginnerConfig.GetRecord(reloadIndex);

        if (loadConfig != null)
        {
            yield return DoGuide(reloadIndex);
        }
    }

    private static void OnLevelUp(int level)
    {
        int triggered = 0;

        if (levelTriggerMap != null && levelTriggerMap.TryGetValue(level, out triggered))
        {
            helper.StartRoutine(DoGuide(triggered));
        }
    }

    private static void OnFirstClearStage(int stageIndex)
    {
        int triggered = 0;

        if (stageTriggerMap != null && stageTriggerMap.TryGetValue(stageIndex, out triggered))
        {
            helper.StartRoutine(DoGuide(triggered));
        }
    }

    private static void OnFirstClearStageAndLevelUp(BattleAccountInfo info)
    {
        int triggered = 0;

        if (info.isWin && stageLevelTriggerMap != null && stageLevelTriggerMap.TryGetValue(info.battleId, out triggered))
        {
            StageProperty prop = StageProperty.Create(info.battleId);

            if (prop != null && !prop.passed && info.preRoleExp + info.roleExp >= RoleLogicData.GetMainRole().GetMaxExp())
            {
                var record = DataCenter.mBeginnerConfig.GetRecord(triggered);
                var param = new ConfigParam((string)record["TRIGGER_PARAM"]);
                int triggerLevel = param.GetInt("level");

                if (info.preRoleLevel + 1 == triggerLevel)
                {
                    helper.StartRoutine(DoGuide(triggered));
                }
            }
        }
    }

    private static void OnStageTaskReward(int taskId)
    {
        int triggered = 0;

        if (stageTaskTriggerMap != null && stageTaskTriggerMap.TryGetValue(taskId, out triggered))
        {
            helper.StartRoutine(DoGuide(triggered));
        }
    }

    private static IEnumerator DoGuide(params int[] startIndex)
    {
        if (!Routine.IsActive(currentGuide))
        {
            //DataCenter.CloseWindow("NEW_FUNC_OPEN_WINDOW");
            GuideKit.OpenBaseMask();
            currentGuide = new GuideProcess(startIndex);
            yield return currentGuide.AppendWith(new GuideProtectRoutine());
            currentGuide = null;
            Terminate(true);
        }
    }

    private static Dictionary<int, int> GroupConfigByTrigger(GUIDE_TRIGGER_TYPE triggerType, string conditionName)
    {
        Dictionary<int, int> result = new Dictionary<int, int>();

        foreach (var pair in DataCenter.mBeginnerConfig.GetAllRecord())
        {
            if (pair.Value["TRIGGER_TYPE"] == (int)triggerType)
            {
                var param = new ConfigParam((string)pair.Value["TRIGGER_PARAM"]);
                int conditionValue = param.GetInt(conditionName);

                if (!result.ContainsKey(conditionValue))
                {
                    result.Add(conditionValue, pair.Key);
                }
            }
        }

        return result;
    }

    private static IEnumerator DoPrologue()
    {
        inPrologue = true;
        yield return new PrologueRoutine();
        inPrologue = false;
    }
}


/// <summary>
/// 保护协程，当检测出引导流程异常时终止引导
/// </summary>
public class GuideProtectRoutine : RoutineBlock
{
    private float elapsed = 0f;
    private float ignored = 0f;
    private bool isLastLocked = false;

    protected override bool OnBlock()
    {
        if (Guide.resetLockedTimeFlag)
        {
            Guide.resetLockedTimeFlag = false;

            if (isLastLocked)
            {
                isLastLocked = false;
                DEBUG.Log("Locked Time = " + elapsed + ", Ignore Time = " + ignored);
            }

            elapsed = 0f;
            ignored = 0f;
        }
        else if (Guide.isLocked)
        {
            elapsed += Mathf.Min(Time.deltaTime, 0.1f);
            ignored += Mathf.Max(Time.deltaTime - 0.1f, 0f);

            if (elapsed > Guide.protectTime)
            {
                DEBUG.Log("Guide Terminated! Locked Time = " + elapsed + ", Ignore Time = " + ignored);
                Guide.Terminate(true);
            }

            isLastLocked = true;
        }
        else
        {
            if (isLastLocked)
            {
                isLastLocked = false;
                DEBUG.Log("Locked Time = " + elapsed + ", Ignore Time = " + ignored);
            }

            elapsed = 0f;
            ignored = 0f;
        }

        return true;
    }
}


public class Button_skip_prologue : CEvent
{
    public override bool _DoEvent()
    {
        Guide.SkipPrologue();
        GuideKit.CloseSkipPrologueWindow();
        return true;
    }
}