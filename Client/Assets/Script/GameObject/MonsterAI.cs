using UnityEngine;
using System.Collections;



public class AI_PatrolReadyIdle : ObjectAI
{
    Vector3 mStartPosition;
	float mRandRange = 1;
    int mWaitTimeMin = 1;
    int mWaitTimeMax = 1;

    public override bool _DoEvent()
    {
		mRandRange = GetOwner().GetConfig("PATROL_RANGE");
        string waitString = GetOwner().GetConfig("PATROL_WAIT_TIME");

		
        int[] val;
        int l = GameCommon.SplitToInt(waitString, ' ', out val);
        
        if (val.Length>=2)
        {
            mWaitTimeMin = val[0];
            mWaitTimeMax = val[1];
        }
        else if(val.Length>0)
            mWaitTimeMax = mWaitTimeMax = val[0];
        else
        {
            Log("ERROR: PATROL_WAIT_TIME config error, must int value");
            return false;
        }

        mStartPosition = GetOwner().GetPosition();
        _StartPatrol();
        StartUpdate();
		return true;
    }

    public bool Update(float t)
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
                obj.SetCurrentEnemy(GetOwner());
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

    public override bool _OnFinish()
    {
		return true;
    }

    public override void _OnOverTime()
    {
        _StartPatrol();

    }

    public override void CallBack(object caller)
    {
        if (!GetFinished())
        {
            float waitTime = UnityEngine.Random.Range(mWaitTimeMin, mWaitTimeMax + 1);
            GetOwner().ResetIdle();
            WaitTime(waitTime);
        }
    }

    protected void _StartPatrol()
    {
        Vector3 randPos = GameCommon.RandInCircularGround(mStartPosition, mRandRange);

        GetOwner().MoveTo(randPos, this);
    }


}