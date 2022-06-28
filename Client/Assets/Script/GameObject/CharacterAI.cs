using UnityEngine;
using System.Collections;

public class AI_CharReadyIdle : ObjectAI
{
    public AI_CharReadyIdle()
    {
        mAILevel = AI_LEVEL.Low;
    }

    public override string _NextAIName() { return "AI_CharMoveToEnemy"; }

    public override bool _DoEvent()
    {
        if (!Update(0))
            return true;

        GetOwner().ResetIdle();
        StartUpdate();
        return true;
    }

    public override bool Update(float secondTime)
    {
        Character mainChar = GetOwner() as Character;
        BaseObject obj = GetOwner().GetCampEnemy();

        if (obj == null)
            obj = GetOwner().FindNearestEnemy();

        if (obj != null)
        {
            //if (mainChar != null)
            //    GuideManager.Notify(GuideIndex.EncounterMonster);

            float dis = Vector3.Distance(GetOwner().GetPosition(), obj.GetPosition());
            if (AutoBattleAI.InAutoBattle() || dis <= GetOwner().LookBounds())
            {
                GetOwner().SetCurrentEnemy(obj);

                if (dis <= GetOwner().AttackBound())
                {
                    obj.SetCurrentEnemy(GetOwner());
                    GetOwner().Attack(obj);
                }
                else
                {
                    BaseObject mTarget = obj;

                    AI_MoveTo mto;
                    if (AutoBattleAI.InAutoBattle() && dis >= GetOwner().LookBounds())
                        mto = StartNextAI("AI_CharMoveToEnemy") as AI_MoveTo;
                    else
                        mto = StartNextAI("AI_MoveToTargetThenAttack") as AI_MoveTo;
                    if (mto != null)
                    {
                        mto.SetTarget(obj);
                        mto.DoEvent();
                    }

                }
                return false;
            }
        }
        return true;
    }
}

public class AI_MainCharMove : ObjectAI
{
    public Vector3 mTargetPos;

    public override bool AllowPlayAnimation(string animName) { return animName == AI_MoveTo.msRunAnimName; }

    public override bool _DoEvent()
    {        
        Character.Self.TryMoveTo(mTargetPos, this);
        return true;
    }

    public override bool _OnFinish()
    {
        if (IsNowAI())
        {
            //GetOwner().StopMove();
            RestartAI(0);
        }
        return true;
    }

    public override void CallBack(object caller)
    {
        Finish();
    }

}

public class AI_CharMoveToEnemy : AI_MoveToTargetThenAttack
{

    public override bool AllowPlayAnimation(string animName) { return animName == "run" || animName == "idle"; }

    public override bool Update(float secondTime)
    {
        if (mTarget == null || mTarget.IsDead())
        {
            Finish();
            return false;
        }

        //BaseObject obj = GetOwner().FindNearestEnemy();
        if (mTarget != null && Vector3.Distance(mTarget.GetPosition(), GetOwner().GetPosition()) < Character.Self.LookBounds())
        {
            bool bIsSummonPet = false;
            Character mainChar = GetOwner() as Character;
            if (mainChar != null)
            {
                foreach (BaseObject obj in mainChar.mFriends)
                {
                    if(obj != null && !obj.IsDead())
                    {
                        bIsSummonPet = true;
                        break;
                    }
                }
            }
            if (bIsSummonPet)
            {
                GetOwner()._StopMove();
                GetOwner().PlayAnim("idle");
                WaitTime(0.45f);
            }
            else
            {
                WaitTime(0);
            }
            
            return false;
        }

        //if (_CheckAttackTarget())
        //    return false;

        //if (!base.Update(secondTime))
        //    return false;

        //if (Vector3.Distance(mTarget.GetPosition(), mTargetPos) > 1)
        //{

        //    mTargetPos = mTarget.GetPosition();

        //    base._DoEvent();
        //}

        return true;
    }

    public override void _OnOverTime()
    {
        if (mTarget != null && !mTarget.IsDead())
        {
            float dis = Vector3.Distance(GetOwner().GetPosition(), mTarget.GetPosition());


            GetOwner().SetCurrentEnemy(mTarget);

            if (dis <= GetOwner().AttackBound())
            {
                mTarget.SetCurrentEnemy(GetOwner());
                if (GetOwner().Attack(mTarget))
                    return;
            }
            else
            {
                AI_MoveTo mto = StartNextAI("AI_MoveToTargetThenAttack") as AI_MoveTo;
                if (mto != null)
                {
                    mto.SetTarget(mTarget);
                    mto.DoEvent();
                    return;
                }
            }
        }

        Finish();
    }
}
