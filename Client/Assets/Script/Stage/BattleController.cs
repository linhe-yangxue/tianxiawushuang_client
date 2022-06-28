using UnityEngine;
using DataTable;


public class BattleController
{
    public virtual void OnPressGroud(Vector3 pos)
    {
        NavMeshPath path = null;
        NavMeshPathStatus pathStatus = AIKit.CalculatePath(Character.Self, pos, out path);

        if (pathStatus == NavMeshPathStatus.PathComplete)
        {
            Character.Self.aiMachine.RequestSwitchAI("MOVE", pos, AI_LAYER.MEDIUM);
        }
    }

    public virtual void OnClickSkill(tLogicData skillData)
    {
        if (skillData == null)
            return;

        int skillIndex = skillData.get("SKILL_INDEX");

        if (Character.Self.aiMachine.fixSkill != skillIndex)
        {
            DataRecord config = DataCenter.mSkillConfigTable.GetRecord(skillIndex);

            if (config != null && Character.Self.CanDoSkill(config))
            {
                Character.Self.aiMachine.RequestSwitchAI("SKILL", skillData, AI_LAYER.MEDIUM);
            }
        }    
    }

    public virtual void OnSelectObject(BaseObject target)
    {
        if (target != null && target.IsEnemy(Character.Self)/* && target != Character.Self.aiMachine.lockedTarget*/)
        {
            if (target != Character.Self.aiMachine.fixEnemy/*lockedTarget*/)
            {
                if (Character.Self.aiMachine.RequestSwitchAI("ATTACK", target, AI_LAYER.MEDIUM))
                {
                    foreach (BaseObject obj in Character.Self.mFriends)
                    {
                        if (obj != null && !obj.IsDead())
                        {
                            obj.aiMachine.RequestSwitchAI("ATTACK", target, AI_LAYER.MEDIUM);
                        }
                    }
                }
            }
            else 
            {
                Character.Self.aiMachine.fixEnemy = target;
            }
        }
    }
}