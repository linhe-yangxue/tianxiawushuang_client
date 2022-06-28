using UnityEngine;
using System.Collections;

//--------------------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------------------
public class BazierPath
{
    enum PATH_MODE
    {
        PATH_END,
        PATH_FOUR,
        PATH_FIVE,
    }

    float mMoveTime = 1.0f;
    float mCurrentTime = 0.0f;
    Vector2[] mPathPoint;
    PATH_MODE mMode = PATH_MODE.PATH_END;

    public void ResetBegin()
    {
        SetPath(mPathPoint, mMoveTime);
    }

    public bool SetPath(Vector2[] pathPoint, float pathTime)
    {
        if (pathPoint.Length > 4)
            mMode = PATH_MODE.PATH_FIVE;
        else if (pathPoint.Length > 3)
            mMode = PATH_MODE.PATH_FOUR;
        else
        {
            mMode = PATH_MODE.PATH_END;
            return false;
        }

        mPathPoint = pathPoint;
        mMoveTime = pathTime;
        mCurrentTime = 0.0f;
        return true;
    }

    public Vector2 Update(float time, out Vector2 dir)
    {
        mCurrentTime += time;
        if (mCurrentTime > mMoveTime)
        {
            mMode = PATH_MODE.PATH_END;
            mCurrentTime = mMoveTime;
        }
        float t = mCurrentTime / mMoveTime;
        switch (mMode)
        {
            case PATH_MODE.PATH_FOUR:
                return BazierPath4(mPathPoint, t, out dir);
            case PATH_MODE.PATH_FIVE:
                return BazierPath5(mPathPoint, t, out dir);
        }
        dir = new Vector2();
        return new Vector2();
    }

    public bool IsEnd() { return mMode == PATH_MODE.PATH_END; }


    Vector2 BaseBezier(Vector2 scrPoint1, Vector2 scrPoint2, float t)
    {
        Vector2 destPoint = new Vector2();
        destPoint.x = scrPoint1.x + (scrPoint2.x - scrPoint1.x) * t;
        destPoint.y = scrPoint1.y + (scrPoint2.y - scrPoint1.y) * t;
        return destPoint;
    }


    public Vector2 BazierPath4(Vector2[] scrPoint, float t, out Vector2 dir)
    {
        if (scrPoint == null || scrPoint.Length < 4)
        {
            dir = new Vector2();
            return new Vector2();
        }

        Vector2 q0 = BaseBezier(scrPoint[0], scrPoint[1], t);
        Vector2 q1 = BaseBezier(scrPoint[1], scrPoint[2], t);
        Vector2 q2 = BaseBezier(scrPoint[2], scrPoint[3], t);

        Vector2 r0 = BaseBezier(q0, q1, t);
        Vector2 r1 = BaseBezier(q1, q2, t);

        dir = new Vector2();
        dir.x = r1.x - r0.x;
        dir.y = r1.y - r0.y;

        return BaseBezier(r0, r1, t);
    }


    public Vector2 BazierPath5(Vector2[] scrPoint, float t, out Vector2 dir)
    {
        if (scrPoint == null || scrPoint.Length < 5)
        {
            dir = new Vector2();
            return new Vector2();
        }

        Vector2 p0 = BaseBezier(scrPoint[0], scrPoint[1], t);
        Vector2 p1 = BaseBezier(scrPoint[1], scrPoint[2], t);
        Vector2 p2 = BaseBezier(scrPoint[2], scrPoint[3], t);
        Vector2 p3 = BaseBezier(scrPoint[3], scrPoint[4], t);

        Vector2 q0 = BaseBezier(p0, p1, t);
        Vector2 q1 = BaseBezier(p1, p2, t);
        Vector2 q2 = BaseBezier(p2, p3, t);

        Vector2 r0 = BaseBezier(q0, q1, t);
        Vector2 r1 = BaseBezier(q1, q2, t);

        dir = new Vector2();
        dir.x = r1.x - r0.x;
        dir.y = r1.y - r0.y;

        return BaseBezier(r0, r1, t);
    }

    public void MainEdit(Vector2[] point, float t)
    {
        Vector2 dir;
        Vector2 curr;
        if (point.Length > 4)
        {
            curr = BazierPath5(point, t, out dir);
        }
        else if (point.Length > 3)
        {
            curr = BazierPath4(point, t, out dir);
        }
        else
        {
            //Console.WriteLine("");
            return;
        }
        string info = " x = " + curr.x.ToString();
        info += ", y = " + curr.y.ToString();
        info += ", dir -> x = " + dir.x.ToString();
        info += ", y = " + dir.y.ToString();
        //Console.WriteLine(info);
    }

}


