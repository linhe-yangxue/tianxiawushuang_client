using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;
using DataTable;


public class GuideData
{
    public int index { get; private set; }
    public bool valid { get; private set; }

    public int triggerType { get; private set; }
    public ConfigParam triggerParam { get; private set; }
    public float actionDelay { get; private set; }
    public int actionType { get; private set; }
    public ConfigParam actionParam { get; private set; }
    public int reloadIndex { get; private set; }
    public int[] nextIndex { get; private set; }
    public ConfigParam cursorParam { get; private set; }
    public ConfigParam tipParam { get; private set; }
    public float protectTime { get; private set; }

    public GuideData(int index)
    {
        this.index = index;
        DataRecord r = DataCenter.mBeginnerConfig.GetRecord(index);

        if (r == null)
        {
            this.valid = false;
        }
        else
        {
            int msec = (int)r["PROTECT_TIME"];
            this.valid = true;
            this.triggerType = r["TRIGGER_TYPE"];
            this.triggerParam = new ConfigParam((string)r["TRIGGER_PARAM"]);
            this.actionDelay = (int)r["TRIGGER_DELAY"] / 1000f;
            this.actionType = r["GUIDE_TYPE"];
            this.actionParam = new ConfigParam((string)r["GUIDE_PARAM"]);
            this.reloadIndex = r["RELOAD_INDEX"];
            this.cursorParam = new ConfigParam((string)r["GUIDE_HAND_SET"]);
            this.tipParam = new ConfigParam((string)r["GUIDE_TXT_SET"]);
            this.nextIndex = ConfigParam.ParseIntArray((string)r["FOLLOW_GUIDE"], '|');
            this.protectTime = msec == 0 ? Guide.DEFAULT_PROTECT_TIME : msec / 1000f;
        }
    }
}


public class GuideProcess : Routine
{
    private bool skipPrologue = false;
    private GuideAction currentAction = null;
    private List<ManualResetGuideOperate> resetOps = new List<ManualResetGuideOperate>();

    public GuideProcess(params int[] startIndex)
    {
        Bind(DoGuideProcess(startIndex));
    }

    private IEnumerator DoGuideProcess(int[] startIndex)
    {
        if (startIndex == null || startIndex.Length == 0 || (startIndex.Length == 1 && startIndex[0] <= 0))
        {
            yield break;
        }

        StageBattle.mOnStageActive -= OnGuideStageActive;
        StageBattle.mOnStageActive += OnGuideStageActive; 
        StageBattle.mOnStageFinish -= OnGuideStageFinish;
        StageBattle.mOnStageFinish += OnGuideStageFinish;

        int[] next = startIndex;

        while (true)
        {
            int length = next.Length;
            GuideData[] datas = new GuideData[length];
            GuideTrigger[] triggers = new GuideTrigger[length];
            string nextStr = "";

            for (int i = 0; i < length; ++i)
            {
                nextStr += next[i] + " ";              
                datas[i] = new GuideData(next[i]);
                triggers[i] = new GuideTrigger(datas[i]);
            }

            DEBUG.Log("next = " + nextStr);
           
            yield return new Any(triggers);

            int triggeredIndex = 0;

            for (int i = 0; i < length; ++i)
            {
                if (triggers[i].status == RoutineStatus.Done)
                {
                    triggeredIndex = i;
                    break;
                }
            }

            GuideTrigger triggered = triggers[triggeredIndex];
            GuideData triggeredData = datas[triggeredIndex];

            Guide.resetLockedTimeFlag = true;
            Guide.protectTime = triggeredData.protectTime;

            if (!triggeredData.valid || triggered.terminate)
            {
                break;
            }

            ManualResetGuideOperate[] ops = triggered.GetResetOperateOnTrigger();

            if (ops != null)
            {
                resetOps.AddRange(ops);

                foreach (var op in ops)
                {
                    op.Set();
                }
            }

            DEBUG.Log("guide = " + triggeredData.index);
            float timeOnTrigger = Time.time;

            if (triggeredData.reloadIndex > 0)
            {
                //yield return null;

                // 不需要等待保存协议回复的条件（满足其一即可）
                // 1 在序章中
                // 2 在关卡战斗过程中
                // 3 待保存的进度的reloadIndex与已保存的进度的reloadIndex一致
                bool dontNeedResp = Guide.inPrologue 
                    || (MainProcess.mStage != null && MainProcess.mStage.mbBattleActive && !MainProcess.mStage.mbBattleFinish)
                    || (Guide.serverReloadIndex == triggeredData.reloadIndex);

                yield return new SaveProgressRoutine(triggeredData.index, !dontNeedResp);
            }

            if (!skipPrologue || triggeredData.actionType != (int)GUIDE_ACTION_TYPE.PROLOGUE_PAGE)
            {
                skipPrologue = false;
                float delayOnTrigger = triggeredData.actionDelay - Time.time + timeOnTrigger;

                if (delayOnTrigger > 0.0001f)
                    yield return new Delay(delayOnTrigger);

                yield return new WaitUntil(() => Net.msUIWaitEffect == null || Net.msUIWaitEffect.waiting == 0);

                currentAction = new GuideAction(triggeredData, triggered);              
                yield return currentAction;
            }

            for (int i = 0; i < resetOps.Count; ++i)
            {
                var op = resetOps[i];

                if (op.NeedReset(triggeredData))
                {
                    op.Reset();
                    resetOps.RemoveAt(i);
                    --i;
                }
                else if (op.currentCycle >= op.maxCycle)
                {
                    resetOps.RemoveAt(i);
                    --i;
                }
                else
                {
                    ++op.currentCycle;
                }
            }

            next = triggeredData.nextIndex;

            if (next.Length == 1 && next[0] == 0)
            {
                break;
            }
        }

        StageBattle.mOnStageActive -= OnGuideStageActive;
        StageBattle.mOnStageFinish -= OnGuideStageFinish;
        skipPrologue = false;
        Guide.protectTime = Guide.DEFAULT_PROTECT_TIME;
    }

    protected override void OnBreak()
    {
        StageBattle.mOnStageActive -= OnGuideStageActive;
        StageBattle.mOnStageFinish -= OnGuideStageFinish;
        skipPrologue = false;
        Guide.protectTime = Guide.DEFAULT_PROTECT_TIME;
    }

    private static void OnGuideStageActive()
    {
        GuideKit.SetPVEBattleSettingEnabled(false);
        GuideKit.SetBossBattleSettingEnabled(false);
        GuideKit.CloseBaseMask();
    }

    private static void OnGuideStageFinish()
    {
        GuideKit.OpenBaseMask();

        PVEStageBattle pve = MainProcess.mStage as PVEStageBattle;

        if (pve != null && Guide.manualAccountOnGuideStageFinish)
        {
            pve.mManualAccountOnFinish = true;
            Guide.manualAccountOnGuideStageFinish = false;
        }
    }

    public bool SkipPrologue()
    {
        if (Routine.IsActive(currentAction) && currentAction.type == GUIDE_ACTION_TYPE.PROLOGUE_PAGE)
        {
            currentAction.Break();
            skipPrologue = true;
            GlobalModule.DoOnNextLateUpdate(GuideKit.ClosePrologueWindowImmediate);
            return true;
        }

        return false;
    }
}