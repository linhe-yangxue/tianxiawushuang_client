using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;


public enum GUIDE_TRIGGER_TYPE
{
    UI_VISIBLE = 1,         // 控件可见
    // "ui_name"    string[]    控件路径
    // "sort_type"  int         如果有多个同名控件在同一父节点下，则启用排序查找，1表示从左到右，2表示从上到下
    // "sort_index" int         启用排序查找时的位次，从0开始

    BATTLE_ACTIVE = 2,      // 战斗开始
    
    CLEAR_STAGE = 3,        // 第一次通过某个关卡（用于触发式引导）
    // "stage_id"   int         关卡ID
    // "star_limit" int         最小星数限制，低于此星数不会触发

    LEVEL_UP = 4,           // 主角升级到某个等级（用于触发式引导）
    // "level"      int         主角等级

    ENEMY_VISIBLE = 5,      // 主角视野内有敌人
    // "enemy"      int         怪物ID，0表示当前最近的敌人

    BATTLE_FINISH = 6,      // 战斗结束，若启用此触发条件则结算表现延迟到此项引导结束

    PROLOGUE_BATTLE = 7,    // 假战斗入口
    // "stage"      int         关卡ID
    // "role"       int         主角ID
    // "pet"        int[]       宠物ID
    // "dir"        int         队伍初始角度，默认0，即面向摄像机，正角度表示从上往下看顺时针旋转

    PET_EXIST = 8,          // 是否存在某个宠物
    // "exist"      int         是否存在
    // "pet"        int         宠物id

    CLEAR_STAGE_LEVEL_UP = 9,   // 第一次通过某个主线关卡且同时升级（用于触发式引导）
    // "stage_id"   int         关卡ID
    // "level"      int         主角等级
    // "star_limit" int         最小星数限制，低于此星数不会触发

    ENEMY_DO_SKILL = 10,        // 当怪物释放技能时，若启用此触发条件则技能生效延迟到后续引导要求继续为止
    // "enemy"      int         怪物ID，0表示当前最近的敌人
    // "skill"      int         技能ID，0表示任意非普攻技能

    STAGE_TASK_REWARD = 11,     // 当领取完关卡奖励后（用于触发式引导）
    // "task_id"    int         奖励ID

    EQUIP_LEVEL_FULL = 12,      // 指定装备是否到达当前最大等级
    // "team_pos"   int         阵位 0 主角 1 ~ 3 宠物
    // "equip_pos"  int         装备位 0 ~ 5
    // "level_full" int         0 未满或没穿装备 1 已满
}

public class GuideTrigger : Routine
{
    public GuideData data { get; private set; }
    public GUIDE_TRIGGER_TYPE type { get; private set; }
    public bool terminate { get; private set; }
    public GameObject uiTarget { get; private set; }    // 目标控件，当触发类型是UI_VISIBLE时有效，缓存此控件用于避免引导行为重复查找目标

    private List<ManualResetGuideOperate> resetOps = null;

    public GuideTrigger(GuideData data)
    {
        this.data = data;
        this.type = (GUIDE_TRIGGER_TYPE)data.triggerType;
        this.terminate = false;
        Bind(DoGuideTrigger());
    }

    private IEnumerator DoGuideTrigger()
    {
        switch (type)
        {
            case GUIDE_TRIGGER_TYPE.UI_VISIBLE:
                yield return DoWaitUIVisible();
                break;

            case GUIDE_TRIGGER_TYPE.BATTLE_ACTIVE:
                yield return DoWaitBattleActive();
                break;

            case GUIDE_TRIGGER_TYPE.ENEMY_VISIBLE:
                yield return DoWaitEnemyVisible();
                break;

            case GUIDE_TRIGGER_TYPE.BATTLE_FINISH:
                yield return DoWaitBattleFinish();
                break;

            case GUIDE_TRIGGER_TYPE.PET_EXIST:
                yield return DoWaitPetExist();
                break;

            case GUIDE_TRIGGER_TYPE.CLEAR_STAGE:
                CheckStarLimit();
                break;

            case GUIDE_TRIGGER_TYPE.ENEMY_DO_SKILL:
                yield return DoWaitEnemyDoSkill();
                break;

            case GUIDE_TRIGGER_TYPE.EQUIP_LEVEL_FULL:
                yield return DoWaitEquipLevelFull();
                break;
        }
    }

    private IEnumerator DoWaitUIVisible()
    {
        string[] path = data.triggerParam.GetStringArray("ui_name");
        int sortArrangement = data.triggerParam.GetInt("sort_type", 0);

        if (path.Length > 0)
        {
            WaitUIVisible w;

            if (sortArrangement == 0)
            {
                w = new WaitUIVisible(path);
            }
            else
            {
                UIArrangement arrangement = sortArrangement == 1 ? UIArrangement.Horizontal : UIArrangement.Vertical;
                int index = data.actionParam.GetInt("sort_index", 0);
                w = new WaitUIVisible(arrangement, index, path);
            }

            yield return w;
            uiTarget = w.target;
        }
    }

    private IEnumerator DoWaitBattleActive()
    {
        yield return new WaitUntil(() => MainProcess.mStage != null && MainProcess.mStage.mbBattleActive);
    }

    private IEnumerator DoWaitEnemyVisible()
    {
        yield return new WaitEnemyVisible(data.triggerParam.GetInt("enemy"));
    }

    private IEnumerator DoWaitMainUILoadingDone()
    {
        bool loadOk = false;
        MainUIScript.mLoadingFinishAction = () => loadOk = true;
        yield return new WaitUntil(() => loadOk);
    }

    private IEnumerator DoWaitBattleFinish()
    {
        Guide.manualAccountOnGuideStageFinish = true;
        yield return new WaitUntil(() => MainProcess.mStage != null && MainProcess.mStage.mbBattleFinish);
        AddResetOperate(new ManualPVEBattleAccountOperate());
    }

    private IEnumerator DoWaitPetExist()
    {
        bool exist = data.triggerParam.GetInt("exist") > 0;
        int pet = data.triggerParam.GetInt("pet");
        yield return new WaitUntil(() => (PetLogicData.Self.GetPetDataByModelIndex(pet) == null) ^ exist);
    }

    private IEnumerator DoWaitEquipLevelFull()
    {       
        int teamPos = data.triggerParam.GetInt("team_pos");
        int equipPos = data.triggerParam.GetInt("equip_pos");
        bool levelFull = data.triggerParam.GetInt("level_full") > 0;
        EquipData equipData = TeamManager.GetRoleEquipDataByTeamPos(teamPos, equipPos);
        yield return new WaitUntil(() => (equipData == null || equipData.strengthenLevel < Mathf.Min(equipData.mMaxStrengthenLevel, RoleLogicData.Self.character.level * 2)) ^ levelFull);
    }

    private void CheckStarLimit()
    {
        int limit = data.triggerParam.GetInt("star_limit");

        if (limit > 0 && GuideKit.GetCurrentMapPageStar() < limit)
        {
            terminate = true;
        }
    }

    private IEnumerator DoWaitEnemyDoSkill()
    {
        int enemyIndex = data.triggerParam.GetInt("enemy");
        int skillIndex = data.triggerParam.GetInt("skill");
        var wait = new WaitEnemyDoSkill(enemyIndex, skillIndex);
        yield return wait;
        wait.skill.WaitTime(99999f);
        AddResetOperate(new ManualResumeSkillOperate(wait.enemy, wait.skill));
    }

    private void AddResetOperate(ManualResetGuideOperate op)
    {
        if (resetOps == null)
        {
            resetOps = new List<ManualResetGuideOperate>();
        }

        resetOps.Add(op);
    }

    public ManualResetGuideOperate[] GetResetOperateOnTrigger()
    {
        return resetOps == null ? null : resetOps.ToArray();
    }
}