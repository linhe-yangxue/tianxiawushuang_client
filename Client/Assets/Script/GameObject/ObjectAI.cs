using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;

public enum AI_LEVEL
{
	Low,
	Normal,
	High,
}

public class ObjectAI : CEvent
{
	public BaseObject	mOwner;

    public AI_LEVEL mAILevel = AI_LEVEL.Normal;
    //----------------------------------------
    public virtual void Stop() {  }
    public virtual bool HoldControl() { return false;  }
    public virtual bool CanDoOnDead() { return false; }
    public virtual bool NeedFinishOnDead() { return true; }
    public virtual bool NeedFinishOnChangeAI() { return true; }
    public virtual float AffectSpeed() { return 0; }

    public virtual bool AllowPlayAnimation(string animName) { return true; }

    public override bool needLowUpdate() { return true; }

	public override bool DoEvent()
	{
		if (GetOwner()==null || GetOwner().IsDead() && !CanDoOnDead() )
		{
            Log("owner is not exist or already dead");
            return false;
		}
		return base.DoEvent();
	}
		
    public void SetOwner(BaseObject baseObject){ mOwner = baseObject; }
	public BaseObject GetOwner(){ return mOwner; }

    public bool IsNowAI()
    {
        return GetOwner().mCurrentAI == this;
    }
			
    public ObjectAI StartNextAI(string nextAIName)
	{
		ObjectAI ai = (ObjectAI)GetOwner().StartAI(nextAIName);

		return ai;
	}

	public virtual void RestartAI(float waitTime)
	{
        if (GetOwner() == null)
            return;       

        if (GetOwner().IsDead())
        {
            if (GetOwner() is Monster)
            {                
                ObjectAI wait = GetOwner().ForceStartAI("TM_WaitDestory");
                wait.WaitTime(6);
            }
            return;
        }

        if (waitTime < -1)
        {
            GetOwner().StartAI();
            return;
        }

        TM_RestartAI ai = GetOwner().StartRestartAI() as TM_RestartAI; //StartNextAI("TM_RestartAI") as TM_RestartAI;
        if (ai != null)
        {
            ai.mWaitTime = waitTime;
            ai.DoEvent();
        }
	}

    public override void Log(string info)
    {
        if (GetOwner() is Friend)
            base.Log(info);
    }

	public override bool _DoEvent(){ return false; }

	public virtual string _NextAIName(){ return ""; }

	public virtual void SetTarget(BaseObject obj){}

}

public class TM_RestartAI : ObjectAI
{
    public float mWaitTime = 0;

    public override bool CanDoOnDead() { return true; }

    public override bool _DoEvent()
	{        
        //if (GetOwner().IsAnimStoped())
        //    GetOwner().ResetIdle();
        //else
        //    GetOwner().mMotionFinishCallBack = this;
        //GetOwner().StopMove();

        if (mWaitTime <= 0.0001f)
        {
            mWaitTime = GetOwner().GetConfig("AI_TIME");
			if (mWaitTime < 0.0001f)
				mWaitTime = 1;
        }
        //float realWaitTime = GetOwner().GetFinalAttackTimeRatio() * mWaitTime;
        //WaitTime(realWaitTime);
        WaitTime(mWaitTime);

        StartUpdate();

		return true;
	}

    public override bool Update(float time)
    {
        if (!GetFinished() && IsNowAI())
        {
            Log(" to idle state ...");
            GetOwner().ResetIdle();
            // ResetIdle的同时停止运动以避免出现滑步现象
            GetOwner()._StopMove();
        }
        return false;
    }


    public override void _OnOverTime()
    {
		if (GetOwner().mCurrentAI!=this)
			Finish();
		else
		{
			Log ("KKK, Now restart ai");
        	GetOwner().StartAI();
		}        
	}

	public override void CallBack(object obj)
	{
		if (!GetFinished())
        {
			GetOwner().ResetIdle();
        }
	}

    public override bool _OnFinish()
    {
        mWaitTime = 0;
        return true;
    }
}

//public class AI_ReadyIdle : ObjectAI
//{
//    //public override string _NextAIName(){ return "AI_Attack"; }

//    public override bool _DoEvent()
//    {
//        StartUpdate();
//        return true;
//    }

//    public override bool Update(float secondTime)
//    {
//        BaseObject obj = GetOwner().FindNearestEnemy();
//        if (obj!=null)
//        {
//            AI_Attack ai = (AI_Attack)StartNextAI(_NextAIName());
//            if (ai!=null)
//            {
//                ai.SetTarget(obj);
//                ai.DoEvent ();
//            }
//            return false;
//        }
//        return true;
//    }
//}



// monster ai
public class AI_MonsterReadyIdle : ObjectAI
{
    public AI_MonsterReadyIdle()
    {
        mAILevel = AI_LEVEL.Low;
    }

	public override string _NextAIName(){ return "AI_MoveToTargetThenAttack"; }

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
        BaseObject obj = GetOwner().GetCampEnemy();
        
        if (obj == null)
            obj = GetOwner().FindNearestEnemy();

        if (obj != null)
        {
            GetOwner().SetCurrentEnemy(obj);
            float dis = Vector3.Distance(GetOwner().GetPosition(), obj.GetPosition());
            int skill = GetOwner().mAttackSkillIndex;
            if (dis <= GetOwner().SkillBound(skill))
            {
                GetOwner().OnStartAttackEnemy(obj);
                //obj.SetCurrentEnemy( GetOwner() );
                GetOwner().Attack(obj);
            }
            else
            {
                BaseObject mTarget = obj;

                AI_MoveTo mto = StartNextAI("AI_MoveToTargetThenAttack") as AI_MoveTo;
                if (mto != null)
                {
                    mto.SetTarget(obj);
                                     
                    mto.DoEvent();
                }

			}
			return false;
		}
		return true;
	}
}

public class AI_CoolDownAttack : ObjectAI
{
    public BaseObject mTarget;

    public override bool _DoEvent()
    {
        float t = GetOwner().GetConfig("SKILL_COOLDOWN");
        if (t > 0.01f)
        {
            GetOwner().OnIdle();
            float realWaitTime = GetOwner().GetFinalAttackTimeRatio() * t;
            WaitTime(realWaitTime);
            //WaitTime(t);
            StartUpdate();
        }
        else
            DoOverTime();
        return true;
    }

    public override bool Update(float secondTime)
    {
        BaseObject enemy = mTarget;
        if (enemy == null || enemy.IsDead())
        {            
            DoOverTime();
            return false;
        }
        return true;
    }

    public override void _OnOverTime()
    {
        BaseObject enemy = mTarget;
        if (enemy != null && !enemy.IsDead())
            GetOwner().PursueAttack(enemy);
        else
            RestartAI(-2);

        Finish();
    }
}

public class AI_MoveTo : ObjectAI
{
    static public string msRunAnimName = "run";
	public Vector3 mTargetPos;

	public ObjectAI mFinishEvent;

    public AI_MoveTo()
    {
        mAILevel = AI_LEVEL.High;
    }

    public override bool AllowPlayAnimation(string animName) { return animName==msRunAnimName; }

    public override bool _DoEvent()
    {
        if (GetOwner().MoveTo(mTargetPos, this))
        {
            Friend f = GetOwner() as Friend;
            if (f!=null)
            {
                if (f.IsFollowInBossBattle())
                    return true;
            }
            StartUpdate();

			return true;
        }
        else if (IsNowAI())
        {
            GetOwner().OnIdle();
        }
        return false;
    }


    public override bool Update(float dt)
    {
        BaseObject obj = GetOwner().FindAttackBoundEnemy();
        if (obj != null)
        {
            GetOwner().SetCurrentEnemy(obj);
            if (GetOwner().Attack(obj))
            {
                Finish();
                return false;
            }
        }

        return true;
    }


    public override bool _OnFinish()
    {
        ObjectAI finishEvt = mFinishEvent;
        mFinishEvent = null;
        if (finishEvt != null)
			mFinishEvent.CallBack(this);

        if (IsNowAI())
        {
            if (finishEvt == null)
            {
                //GetOwner().StopMove();
                ActiveObject obj = GetOwner() as ActiveObject;

                // 当移动中丢失目标时立即RestartAI重新寻找目标以保证移动的连贯性
                if (obj != null && obj.mMoveComponent.mNeedMove)
                    RestartAI(0.01f);
                else
                    RestartAI(0);
            }
        }

        return true;
    }

    public override void CallBack(object caller)
    {
        Finish();
    }
}




public class AI_MoveToTargetThenAttack : AI_MoveTo
{
	public BaseObject mTarget;
    

    public override float AffectSpeed() 
	{ 
		return 3; 
	}

	public override void SetTarget(BaseObject obj){ mTarget = obj;}

    protected Vector3 MakeTargetPos()
    {        
        ///!!!
        //if (mTarget != null && !mTarget.IsDead())
        //{
        //    ObjectLocationForPet l = mTarget.GetAttackPointLocation(GetOwner());
        //    if (l != null)
        //    {
        //        return l.Update(mTarget, null);
        //    }
        //}
        return mTarget.GetPosition();
    }

    public override bool _DoEvent()
    {
        if (mTarget == null)
        {
            Finish();
            return false;
        }

        mTargetPos = MakeTargetPos();       

        StartUpdate();

        //if ( !base._DoEvent() )
        //{
        //    mTargetPos = mTarget.GetPosition();
        //    base._DoEvent();
        //}

        return base._DoEvent();
    }

    public virtual bool _CheckAttackTarget()
    {
        if (mTarget != null && !mTarget.IsDead())
        {
            float dis = Vector3.Distance(GetOwner().GetPosition(), mTarget.GetPosition());

            if (dis < GetOwner().AttackBound())
            {
                GetOwner().SetCurrentEnemy(mTarget);
                mTarget.SetCurrentEnemy(GetOwner());
                GetOwner().Attack(mTarget);
                return true;
            }
        }
        
        return false;
    }

    public override bool Update(float secondTime)
    {
        if (mTarget == null || mTarget.IsDead())
        {
            Finish();
            return false;
        }

        if (_CheckAttackTarget())
            return false;

        if (!base.Update(secondTime))
            return false;

        if (Vector3.Distance(mTarget.GetPosition(), mTargetPos) > 1)
        {

            mTargetPos = MakeTargetPos();

			if ( !base._DoEvent() )
			{
				mTargetPos = mTarget.GetPosition();
				base._DoEvent();
			}
        }

        return true;
    }
}

public class AI_MoveToTargetThenSkill : AI_MoveToTargetThenAttack
{
    public int mWaitSkillIndex = 0;
    float mCheckDistance = 1;
    public tLogicData mSkillData;

    public override bool _DoEvent()
    {
        mCheckDistance = GetOwner().SkillBound(mWaitSkillIndex)-0.2f;

        return base._DoEvent();
    }

    public override bool _CheckAttackTarget()
    {
        if ( Vector3.Distance(GetOwner().GetPosition(), mTarget.GetPosition()) < mCheckDistance )
        {
            GetOwner().SetCurrentEnemy(mTarget);
            mTarget.SetCurrentEnemy(GetOwner());
            Skill s = GetOwner().DoSkill(mWaitSkillIndex, mTarget, mSkillData);
            if (s == null)
            {
                Log("info > skill fail > " + mWaitSkillIndex.ToString());
            }
            return true;
        }

        return false;
    } 

	public override bool Update(float secondTime)
	{
		if (mTarget==null || mTarget.IsDead())
		{
			Finish();
			return false;
		}
		
		if (_CheckAttackTarget())
			return false;
		
		//if (!base.Update(secondTime))
		//	return false;
		
		if (Vector3.Distance(mTarget.GetPosition(), mTargetPos) > 1)
		{

            //mTargetPos = MakeTargetPos();
			
			base._DoEvent();
		}
		
		return true;
	}
}



public class AI_MonsterRelive : ObjectAI
{
	public override bool CanDoOnDead(){ return true; }

	public override bool _DoEvent()
	{
		WaitTime(6);
		
		return true;
	}

	public override void _OnOverTime()
	{
		GetOwner().Relive();
		Vector3 pos = GetOwner().GetPosition();

		pos.x = pos.x + Random.Range(-4, 4);
		pos.z = pos.z + Random.Range(-4, 4);
		GetOwner().SetPosition(pos);
	}
}

public class TM_WaitToBeginOutShow : ObjectAI
{
    public float mOutTime = 3;

    public override bool CanDoOnDead() { return true; }
    public override bool _DoEvent()
    {
        WaitTime(3);
        return true;
    }

    public override void _OnOverTime()
    {
        GetOwner().StartFadeInOrOut(mOutTime, false);
    }
}


public class TM_WaitDestory : ObjectAI
{
    public override bool CanDoOnDead() { return true; }
    public override bool _DoEvent()
    {
        _OnOverTime();

		return true;
    }

    public override void _OnOverTime()
    {
        GetOwner().Destroy();
    }
}



