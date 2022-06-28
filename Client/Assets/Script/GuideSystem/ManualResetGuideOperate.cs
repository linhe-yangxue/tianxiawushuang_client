using UnityEngine;
using System;
using System.Collections;
using Utilities.Routines;


public abstract class ManualResetGuideOperate
{
    public int currentCycle { get; set; }
    public int maxCycle { get; protected set; }

    public virtual bool NeedReset(GuideData data)
    {
        return true;
    }

    public abstract void Set();
    public abstract void Reset();
}


public class ManualPVEBattleAccountOperate : ManualResetGuideOperate
{
    public ManualPVEBattleAccountOperate()
    {
        maxCycle = 0;
    }

    public override void Set() { }

    public override void Reset()
    {
        PVEStageBattle pve = MainProcess.mStage as PVEStageBattle;

        if(!Guide.inPrologue && pve != null && pve.mbBattleFinish)
        {
            pve.ManualAccount();
        }
    }
}


public class ManualResumeSkillOperate : ManualResetGuideOperate
{
    private BaseObject owner;
    private Skill skill;

    public ManualResumeSkillOperate(BaseObject owner, Skill skill)
    {
        this.owner = owner;
        this.skill = skill;
        this.maxCycle = 5;
    }

    public override bool NeedReset(GuideData data)
    {
        return data.actionParam.GetInt("resume_skill", 0) == 1;
    }

    public override void Set()
    {
        skill.WaitTime(9999f);
        owner.PlayAnim("idle");
        skill.dontStopOnAIBreak = true;
    }

    public override void Reset()
    {
        if (owner != null && !owner.IsDead())
        {
            skill.dontStopOnAIBreak = false;
            owner.aiMachine.TrySwitchAI(DoResumeSkill(owner, skill), AI_LAYER.HIGHEST + 100, null);
        }
    }

    private IEnumerator DoResumeSkill(BaseObject target, Skill skill)
    {
        bool finish = false;
        skill.mOnApplyFinish += () => finish = true;
        skill.WaitTime(0.5f);
        yield return new Any(new WaitUntil(() => finish), new Delay(5f));
    }
}