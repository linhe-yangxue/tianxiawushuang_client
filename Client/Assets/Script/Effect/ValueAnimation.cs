using UnityEngine;
using System.Collections;
using System;

//---------------------------------------------------------------------------------------
public class AddSpeedMove
{
	public Vector3 mStartPos;
	public Vector3 mEndPos;
	
	public float aSpeed = 0.2f;
	
	float mPathLength = 0f;
	float mCurrentTime = 0f;
	Vector3 mDir;
	
	public bool Update(float time, out Vector3 nowPos)
	{
		mCurrentTime += time;
		bool bFinish = false;
		float currentMove = aSpeed * mCurrentTime * mCurrentTime * 0.5f;
		if (currentMove >= mPathLength)
		{
			bFinish = true;
			//return true;
			currentMove = mPathLength;
		}
		
		nowPos = (mStartPos + mDir * currentMove);           
		
		return bFinish;
	}
	
	public virtual void SetPos(Vector3 start, Vector3 end, float addspeed)
	{
		mStartPos = start;
		mEndPos = end;
		mDir = mEndPos - mStartPos;
		mDir.z = 0;
		mPathLength = mDir.magnitude;
		mDir.Normalize();
		aSpeed = addspeed;
	}
	
	public virtual void SetPos(Vector3 start, Vector3 dir, float movePath, float addspeed)
	{
		dir.z = 0;
		dir.Normalize();
		
		mStartPos = start;
		mDir = dir;
		mPathLength = movePath;
		mEndPos = mStartPos + dir * movePath;
		
		aSpeed = addspeed;
	}
	
	
	
}

//---------------------------------------------------------------------------------------

public class SubSpeedMove
{
	public Vector3 mStartPos;
	public Vector3 mEndPos;
	
	public float startSpeed = 0f;
	public float subSpeed = 0.2f;
    public float paopaolifetime;
	
	float mPathLength = 0f;
	float mCurrentTime = 0f;
	float mMoveTime = 0f;
	
	Vector3 mDir;
	
	//-----------------------------------------------------
	public bool Update(float time, out Vector3 nowPos)
	{
		mCurrentTime += time;
		bool bFinish = false;
		float nowmovetime = mMoveTime - mCurrentTime;
		float currentMove = subSpeed * nowmovetime * nowmovetime * 0.5f;
		if (mCurrentTime >= mMoveTime || currentMove <= 0)
		{
			bFinish = true;
			//return true;
			currentMove = 0;
		}
		
		
		nowPos = (mEndPos - mDir * currentMove);
		
		return bFinish;
	}
	
	private float SetSpeed(float aspeed)
	{
		mMoveTime = (float)Math.Sqrt(mPathLength * 2f / aspeed);
		subSpeed = aspeed;
        return mMoveTime;
	}
	
	public virtual float SetPos(Vector3 start, Vector3 end, float addspeed)
	{
		mStartPos = start;
		mEndPos = end;
		mDir = mEndPos - mStartPos;
		mDir.z = 0;
		mPathLength = mDir.magnitude;
		mDir.Normalize();
		paopaolifetime=SetSpeed(addspeed);
        return paopaolifetime;
	}
	
	public virtual void SetPos(Vector3 start, Vector3 dir, float movePath, float addspeed)
	{
		dir.z = 0;
		dir.Normalize();
		
		mStartPos = start;
		mDir = dir;
		mPathLength = movePath;
		mEndPos = mStartPos + dir * movePath;
		SetSpeed(addspeed);
	}
	
}
//---------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------

public class ValueAnimation
{
	public float mParamValue = 0;
	public float mDestValue = 0;
	public float mNowValue = 0;
	public float mLastTime = 0;
	public bool mFinish = false;
	
	//---------------------------------------------------
	//public virtual void Apply(float nowV) { }
	
	public void Start(float startV, float destV, float useTime)
	{
        if (useTime <= 0.0001f)
        {
            Logic.EventCenter.Log(LOG_LEVEL.ERROR, "Use time is zero for ValueAnimation Start ");
            return;
        }
		mLastTime = useTime;
		mParamValue = (destV - startV) / mLastTime;
		
		mDestValue = destV;
		mNowValue = startV;
		mFinish = false;
	}
	
	public bool Update(float time, out float nowValue)
	{
		if (mFinish)
		{
			nowValue = mNowValue;
			return true;
		}
		mLastTime -= time;
		if (mLastTime <= 0)
		{
			mNowValue = mDestValue;
			mFinish = true;
		}
		else
		{
			mNowValue += mParamValue * time;
		}
		
		nowValue = mNowValue;
		
		return mFinish;
	}
	
	public float NowValue()
	{
		return mNowValue;
	}
	
	public bool IsFinish()
	{
		return mFinish;
	}
}
//---------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------

public class WaveAnimation
{
    float mTotalTime = 0;
    float mWaveLength = 1;
    float mWaveHeight = 1;
    float mEndTime = 1;

    float mNowWaveValue = 0;

    public void Start(float startTime, float endTime, float waveLength, float waveHeight)
    {
        mEndTime = endTime;
        mTotalTime = 0;
        mWaveLength = waveLength;
        mWaveHeight = waveHeight;
        Update(0);
    }

    public bool Update(float dt)
    {
        bool bEnd = false;
        mTotalTime += dt;
        if (mTotalTime >= mEndTime)
        {
            bEnd = true;
            mTotalTime = mEndTime;
        }

        mNowWaveValue = (float)Math.Sin(mTotalTime / mWaveLength * ((float)Math.PI) * 0.5f) * mWaveHeight;
        return bEnd;
    }

    public float NowValue() { return mNowWaveValue; }
}
//-------------------------------------------------------------------------
//-------------------------------------------------------------------------

public class VectorAnimation : ValueAnimation
{
    Vector3 mBeginPos = Vector3.zero;
    Vector3 mEndPos = Vector3.zero;
    public Vector3 mDirection = Vector3.forward;
    Vector3 mNowPos = Vector3.zero;

    public void Start(Vector3 beginPos, Vector3 endPos, float speed)
    {
        mBeginPos = beginPos;
        mEndPos = endPos;
        float dis = Vector3.Distance(beginPos, endPos);

        if (dis > 0.0001f)
        {
            mDirection = endPos - beginPos;
            mDirection.Normalize();
            base.Start(0, dis, dis / speed);
        }
        else
        {
            mFinish = true;
            mNowPos = endPos;
        }
    }

    public void StartOnTime(Vector3 beginPos, Vector3 endPos, float useTime)
    {
        float dis = Vector3.Distance(beginPos, endPos);
        if (dis>0.0001f)
            Start(beginPos, endPos, dis / useTime);
        else
        {
            mFinish = true;
            mNowPos = endPos;
        }
    }

    public bool Update(float dt)
    {
        if (base.mFinish)
            return true;

		float nowValue = 0;
        bool bEnd = base.Update(dt, out nowValue);
		if (bEnd)
			mNowPos = mEndPos;
		else
        	mNowPos = mBeginPos + mDirection * nowValue;
        return bEnd;
    }

    public Vector3 NowValue()
    {
        return mNowPos;
    }
}

//---------------------------------------------------------------------------------------

public class TimeAnimation
{
	public float mNowTime = 0;
	public float mLastTime = 0;
	public bool mFinish = true;
	
	//---------------------------------------------------
	//public virtual void Apply(float nowV) { }
	
	public void Start(float useTime)
	{
		mLastTime = useTime;

		mNowTime = 0;
		mFinish = false;
	}
	
	public bool Update(float time)
	{
		if (mFinish)
		{
			//mNowTime = mLastTime;
			return true;
		}
		mNowTime += time;
		if (mNowTime>=mLastTime)
		{
			mNowTime = mLastTime;
			mFinish = true;
		}	
		
		return mFinish;
	}
	
	public float NowTime()
	{
		return mNowTime;
	}
	
	public bool IsFinish()
	{
		return mFinish;
	}
}
//---------------------------------------------------------------------------------------

public class TotalTime
{
    protected float mOverTime;

    public void Start(float overTime)
    {
        mOverTime = Time.time + overTime;
    }

    public bool Update()
    {
        return Time.time > mOverTime;
    }

	public float GetRestTime()
	{
		float f = mOverTime - Time.time;
		if(f < 0) f = 0;
		return f;
	}
}