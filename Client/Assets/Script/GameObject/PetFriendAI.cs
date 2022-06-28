using UnityEngine;
using System.Collections;
using Logic;



// 每次攻击完成之后, 先搜索自己视野的敌人, 再搜索主角视野的敌人
public class AI_FriendReadyIdle : ObjectAI
{
    public AI_FriendReadyIdle()
    {
        mAILevel = AI_LEVEL.Low;
    }

    public override bool _DoEvent()
    {        
        Friend p = GetOwner() as Friend;
        if (p != null)
        {
            BaseObject obj = GetOwner().FindNearestEnemy();

            if (obj == null)
                obj = p.GetOwnerObject().FindNearestEnemy();

            if (obj != null)
            {
                GetOwner().PursueAttack(obj);
                return false;
            }

            // 如果未存在敌人直接跟随
            if (!p.FollowOwner())
                StartUpdate();
        }


        return true;
    }

    public override bool Update(float secondTime)
    {
        Friend p = GetOwner() as Friend;
        BaseObject obj = p.GetOwnerObject().FindNearestEnemy();

        if (obj != null)
        {
            GetOwner().PursueAttack(obj);
            return false;
        }
        else
            if (p.FollowOwner())
                return false;

        return true;

        //Friend p = GetOwner() as Friend;
        //if (p!=null && GetOwner().GetCampEnemy()==null)
        //{
        //    BaseObject followTarget = p.GetOwnerObject();
        //    if (followTarget != null)
        //    {
        //        if (p.FollowOwner())
        //             return false;
        //    }
        //}

        //return base.Update(secondTime);
    }


}

public class AI_FriendReadyIdleInAutoBattle : ObjectAI
{
    public override bool _DoEvent()
    {
        if (Update(0))
            StartUpdate();

        return true;
    }

    public override bool Update(float secondTime)
    {
        BaseObject obj = Character.Self.GetCurrentEnemy();
        if (obj != null && !obj.IsDead())
        {
            GetOwner().PursueAttack(obj);
            return false;
        }
        return true;
    }

}

// Follow move, 跟随中只检查主角视野的敌人
public class AI_FriendFollowMove : AI_MoveTo
{
    public override bool _DoEvent()
    {
        base._DoEvent();
        StartUpdate();
        return true;
    }

    public override bool Update(float secondTime)
    {
        Friend p = GetOwner() as Friend;
        if (p != null)
        {
            BaseObject obj = p.GetOwnerObject().FindNearestEnemy();
            if (obj != null)
            {
                GetOwner().PursueAttack(obj);
                return false;
            }
        }

        return true;
    }

    public override void CallBack(object caller)
    {
        WaitTime(0.5f);
    }

    public override void _OnOverTime()
    {
        Finish();
    }

    public override bool _OnFinish()
    {
        if (IsNowAI())
        {
            ObjectAI ai = StartNextAI("AI_FriendCheckMainRaleEnemy");
            ai.DoEvent();  
        }
		return true;
    }
}

// 刚开始或, 每次跟随完后, 就只判断主角的目标, 根据张钟鸣流程图
public class AI_FriendCheckMainRaleEnemy : ObjectAI
{
    public override bool _DoEvent()
    {        
        
        if (Update(0))
        {
            GetOwner().OnIdle();
			WaitTime(0.2f);
        }
        
        return true;
    }

    public override void _OnOverTime()
    {
        if (Update(0))
        {
            GetOwner().OnIdle();
            StartUpdate();
        }
    }

    public override bool Update(float secondTime)
    {
        Friend p = GetOwner() as Friend;
        BaseObject obj = p.GetOwnerObject().FindNearestEnemy();
        if (obj != null)
        {
            GetOwner().PursueAttack(obj);
            return false;
        }
		if (p.FollowOwner())
			return false;

        return true;
    }
}


public class AI_FriendCheckFollow : ObjectAI
{
    public override bool _DoEvent()
    {
        StartUpdate();
        return true;
    }

    public override bool Update(float dt)
    {
        Vector3 toDir = Character.Self.GetPosition() - GetOwner().GetPosition();
        toDir.Normalize();
        //float last = Vector3.Distance(Character.Self.GetPosition(), GetOwner().GetPosition()) * Vector3.Dot(toDir, Character.Self.mMoveValue.mDirection);
        float last = Vector3.Dot(toDir, Character.Self.mMoveComponent.mDirection);
        if (last < 0)
        {
            Friend f = GetOwner() as Friend;
            f.FollowOwner();
            Finish();
            return false;
        }

        BaseObject obj = GetOwner().FindAttackBoundEnemy();
        if (obj != null)
        {
            GetOwner().SetCurrentEnemy(obj);
            if (GetOwner().Attack(obj))
            {
                return false;
            }
        }

        return true;
    }

    public override bool _OnFinish()
    {
        if (IsNowAI())
            RestartAI(0);

        return true;
    }
}

public class AI_FriendFollowIdle : ObjectAI
{
    public AI_FriendFollowIdle()
    {
        mAILevel = AI_LEVEL.Low;
    }

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
        Friend p = GetOwner() as Friend;

        BaseObject followTarget = p.GetOwnerObject();
        if (followTarget != null)
        {

            if (p.FollowOwner())
                return false;
        }


        return true;
    }


}

public class AI_FriendWaitAttack : ObjectAI
{
    public BaseObject mTarget;

    public override bool _DoEvent()
    {
        WaitTime(0.5f);
        return true;
    }

    public override void _OnOverTime()
    {
        if (mTarget != null && !mTarget.IsDead())
        {
            GetOwner().PursueAttack(mTarget);           
        }
        Finish();
    }
}