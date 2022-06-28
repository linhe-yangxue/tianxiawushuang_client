using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// auto battle
public class AutoBattleAI : ObjectEvent
{
    static public Dictionary<int, BaseObject> mMonsterList = new Dictionary<int, BaseObject>();
    static public int mMaxIndex = 0;
    static public int mCurrentIndex = 0;

    public BaseObject mTargetMonster;

    static public void InitReset()
    {
        mMaxIndex = 0;
        mCurrentIndex = 0;
        mMonsterList.Clear();
    }

    static public void AddMonster(int index, BaseObject monster)
    {
        if (index > mMaxIndex)
            mMaxIndex = index;
        mMonsterList.Add(index, monster);
    }

	static public bool InAutoBattle() { return Character.Self!=null && Character.Self.mCurrentAutoTargetAI != null && !Character.Self.mCurrentAutoTargetAI.GetFinished(); }

    public override bool _DoEvent()
    {
        if (Character.Self.mCurrentAutoTargetAI == null)
        {
            SetFinished(false);
            mTargetMonster = GetNextAttackMonster();
            Character.Self.mCurrentAutoTargetAI = this;
            Character.Self.SetCurrentEnemy(mTargetMonster);
            // ??? Auto use skill, Now can not use
            //??? WaitTime(1);
        }
        else
        {
            Character.Self.mCurrentAutoTargetAI.Finish();
            Character.Self.mCurrentAutoTargetAI = null;
        }

        return true;
    }


    public BaseObject GetNextAttackMonster()
    {
        while (true)
        {
            BaseObject target;
            if (mMonsterList.TryGetValue(mCurrentIndex, out target))
            {
                if (target != null && !target.IsDead())
                {
                    //DEBUG.Log("**** > " + mCurrentIndex);
                    return target;
                }
            }
            ++mCurrentIndex;
            if (mCurrentIndex > mMaxIndex)
                break;
        }
		
        return null;
    }

    public void CheckOnMonsterDead(BaseObject deadMonster)
    {
        if (!GetFinished() && deadMonster.IsDead() && deadMonster == mTargetMonster)
        {
            mTargetMonster = GetNextAttackMonster();
            if (mTargetMonster == null)
            {
                DEBUG.Log("not find next monster, may be all monster dead?");
                Finish();
            }
            else
            {
                Character.Self.SetCurrentEnemy(mTargetMonster);     
				CameraMoveEvent.OnTargetObjectMove(Character.Self);
            }
        }
    }

	public override void _OnOverTime()
    {
		if (Character.Self.IsDead() || !InAutoBattle())
        {
            Finish();
            return;
        }

        BaseObject enemy = Character.Self.GetCurrentEnemy();
        if (enemy != null && !enemy.IsDead())
        {
            tLogicData skillData = DataCenter.GetData("SKILL_UI");
            //for (int i = 1; i < 3; ++i)
            int i = Random.Range(1, 4);
            tLogicData data = skillData.getData("DoSkill_" + i.ToString());
            if (data != null)
            {
                if (data.get("CD_FINISH"))
                {
                    int skillIndex = data.get("SKILL_INDEX");
                    if (skillIndex > 0)
                    {
                        if (Vector3.Distance(enemy.GetPosition(), Character.Self.GetPosition()) <= Character.Self.SkillBound(skillIndex))
                        {
                            Skill skill = Character.Self.DoSkill(skillIndex, enemy, data);
                            if (skill != null)
                            {                   
                                //WaitTime((float)skill.GetConfig("CD_TIME") + 2);
                                //return false;
                            }
                        }
                        else
                        {
                            AI_MoveToTargetThenSkill ai = Character.Self.StartAI("AI_MoveToTargetThenSkill") as AI_MoveToTargetThenSkill;
                            ai.SetTarget(enemy);
                            ai.mWaitSkillIndex = skillIndex;
                            ai.mSkillData = data;
                            ai.DoEvent();
                        }
                    }
                }
            }
        }
		WaitTime(1);
    }


}
