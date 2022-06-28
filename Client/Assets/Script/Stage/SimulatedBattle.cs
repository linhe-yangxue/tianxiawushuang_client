using UnityEngine;
using System.Collections.Generic;


public class SimulatedStage : PVEStageBattle
{
    public BattleSimulator simulator;

    public override STAGE_TYPE GetStageType()
    {
        return simulator.isPVE ? STAGE_TYPE.MAIN_COMMON : STAGE_TYPE.PVP4;
    }

    public override void InitBattleUI()
    {
        DataCenter.OpenWindow("MASK_OPERATE_WINDOW");
        DataCenter.OpenWindow("BATTLE_PLAYER_WINDOW");
        DataCenter.OpenWindow("BATTLE_SKILL_WINDOW");
        //DataCenter.OpenWindow("PET_ATTACK_BOSS_WINDOW");
        //DataCenter.SetData("PET_ATTACK_BOSS_WINDOW", "PET_FOLLOW", false);

        for (int i = 1; i <= 3; ++i)
        {
            var parent = GameCommon.FindUI("do_skill_father_" + i);

            if (parent != null)
            {
                GameCommon.SetUIVisiable(parent, "skill_open_description_root", false);
            }
        }
    }

    public override void OnObjectDead(BaseObject obj)
    {
        MainProcess.OnObjectDead(obj);
        OBJECT_TYPE objType = obj.GetObjectType();

        if (objType == OBJECT_TYPE.CHARATOR || objType == OBJECT_TYPE.PET)
        {
            if (Character.Self.TeamAllDead())
            {
                OnFinish(false);
            }
            else if (CameraMoveEvent.mMainTarget == obj)
            {
                CameraMoveEvent.BindMainObject(Character.Self.GetAlivedPet());
            }
        }
        else
        {
            if (simulator.isPVE)
            {
                bool allMonsterDead = true;

                if (simulator.monsters != null)
                {
                    for (int i = 0; i < simulator.monsters.Length; ++i)
                    {
                        allMonsterDead &= simulator.monsters[i] == null || simulator.monsters[i].IsDead();
                    }
                }

                if (allMonsterDead)
                {
                    OnFinish(true);
                }
            }
            else 
            {
                if (objType == OBJECT_TYPE.OPPONENT_CHARACTER || objType == OBJECT_TYPE.OPPONENT_PET)
                {
                    if (OpponentCharacter.mInstance.TeamAllDead())
                    {
                        OnFinish(true);
                    }
                }
            }
        }
    }

    protected override void OnActive()
    { }

    protected override void OnProcess(float delayTime)
    { }

    public override void OnFinish(bool bWin)
    {
        if (mbBattleFinish)
            return;

        mbBattleFinish = true;
        mbSucceed = bWin;
        mBattleTime = Time.time - mActiveTime;
        DisableAI();
        MainProcess.OnBattleFinish(bWin);
        simulator.BattleFinish();
    }
}


public class SimilatedBattleMonitor : BattleMonitor
{
    public SimulatorLog simulatorLog;

    public override void OnSkillDamage(Skill skill, BaseObject target, int damageValue, SKILL_DAMAGE_FLAGS flags)
    {
        SkillDamageInfo info = new SkillDamageInfo();
        info.timeSinceBattleStart = MainProcess.mStage == null ? 0f : MainProcess.mStage.mBattleTime;
        info.skillIndex = skill.mConfigIndex;
        info.skillUniqueID = skill.mUniqueID;
        info.attackerID = skill.mOwner.mID;
        info.damagerID = target.mID;
        info.damageValue = damageValue;
        info.flags = flags;
        simulatorLog.LogSkillDamage(info);
    }
}