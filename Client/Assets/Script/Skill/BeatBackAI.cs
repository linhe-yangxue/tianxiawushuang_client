using UnityEngine;
using System.Collections;

public class AI_BeatBackMove : ObjectAI
{
    static public float mCheckOnceStepLenght = 0.5f;
    public Vector3 mTargetPos;
    public float mSpeed = 0.8f;

    public SubSpeedMove mPosMoveValue;

    public override bool CanDoOnDead() { return true; }
    public override bool NeedFinishOnDead() { return false; }

    public override bool _DoEvent()
    {
        Vector3 targetPos = mTargetPos;
        NavMeshAgent navAgent = GetOwner().mMainObject.GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            NavMeshPath path = new NavMeshPath();
            try
            {
                if (navAgent.CalculatePath(mTargetPos, path) && path.status != NavMeshPathStatus.PathPartial && path.corners.Length == 2)
                {
                    targetPos = mTargetPos;
                }
                else
                {
                    bool bHave = false;
                    int checkCount = (int)(Vector3.Distance(mTargetPos, GetOwner().GetPosition()) / mCheckOnceStepLenght - 1);
                    if (checkCount > 1)
                    {
                        Vector3 dir = GetOwner().GetPosition() - mTargetPos;
                        dir.Normalize();
                        for (int i = 0; i < checkCount; ++i)
                        {
                            Vector3 checkPos = mTargetPos + dir * mCheckOnceStepLenght;
                            if (navAgent.CalculatePath(checkPos, path) && path.status != NavMeshPathStatus.PathPartial && path.corners.Length == 2)
                            {
                                targetPos = checkPos;
                                bHave = true;
                                break;
                            }
                        }
                    }
                    if (!bHave)
                    {
                        Finish();
                        return false;
                    }
                }
            }
            catch
            {
                Finish();
                return false;
            }
        }

        mPosMoveValue = new SubSpeedMove();
        mPosMoveValue.SetPos(GetOwner().GetPosition(), targetPos, mSpeed);

        StartUpdate();

        return true;
    }

    public bool _GetCanMovePos(NavMeshAgent navAgent, Vector3 targetPos, int count, ref Vector3 result)
    {
        NavMeshPath path = new NavMeshPath();
        if (navAgent.CalculatePath(targetPos, path) && path.status != NavMeshPathStatus.PathPartial)
        {
            result = targetPos;
            return true;
        }

        //Vector3 pos = targetPos;
        //pos.y += 9000;
        //Ray ray = new Ray(pos, Vector3.down);
        //RaycastHit rayInfo;
        //int mask = 1 << CommonParam.ObstructLayer;
        //mask = ~mask;

        //NavMeshHit hit;
        //Vector3 t = targetPos;
        //t.y -= 1000;
        //if (NavMesh.Raycast(pos, t, out hit, -1))
        //{
        //    result = targetPos;
        //    return true;
        //}

        if (--count <= 0)
        {
            result = Vector3.zero;
            return false;
        }
        return _GetCanMovePos(navAgent, (navAgent.transform.position + targetPos) / 2, count, ref result);
    }

    public override bool Update(float dt)
    {
        //if (bCheckNav)
        //{
        //    NavMeshAgent navAgent = GetOwner().mMainObject.GetComponent<NavMeshAgent>();
        //    if (navAgent != null)
        //    {
        //        if (navAgent.pathStatus == NavMeshPathStatus.PathInvalid || navAgent.pathStatus == NavMeshPathStatus.PathPartial)
        //        {
        //            Finish();
        //            return false;
        //        }
        //        return true;
        //    }
        //    return false;
        //}
        //else
        {
            Vector3 nowPos;
            bool bEnd = mPosMoveValue.Update(dt, out nowPos);
            GetOwner().SetPosition(nowPos);
            GetOwner().OnPositionChanged();
            if (bEnd)
                Finish();
            return !bEnd;
        }
        return true;
    }

    public override bool _OnFinish()
    {
        RestartAI(0);

        return true;
    }
}