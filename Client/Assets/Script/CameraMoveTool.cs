using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using Utilities.Routines;
using DataTable;


public class CameraMoveEvent : CEvent
{
	static public float msCameraMaxHeight = 20;
	static public float msTargetMaxDistance = 20;
    static public ActiveObject mMainTarget;

    public static Vector3 mCameraOffset = new Vector3(0f, 11f, 12.75f); // 摄像机偏移值
    public static Vector3 mCameraAngles = new Vector3(40f, 180f, 0f);   // 摄像机倾角，一般只需要调x坐标
    public static float mMaxAccelerate = 20f;   // 最大加速度，主要用于控制摄像机的加速
    public static float mDamping = 10f;          // 平滑阻尼系数，主要用于控制摄像机的减速
    public static float mForwardOffset = 2.5f;  // 前向偏移，摄像机注视的点在目标对象正前方多少米

    // 最大冻结时间，当跟随目标丢失攻击目标后，摄像机会有一段冻结时间以避免再次找到攻击目标引发镜头抖动现象
    // 跟随目标移动或重新找寻到攻击目标或超过最大冻结时间会取消冻结状态
    public static float mMaxFrozenTime = 2f;     
  
    public static float mCheckBounds = 1f;

    private Vector3 mLastCheckPos = Vector3.zero;
    private Vector3 mLastSpeed = Vector3.zero;
    
    private bool mLocked = false;
    private float mMoveElapsed = 0f;
    private float mMoveDuration = 1f;
    private Vector3 mMoveFromPos = Vector3.zero;
    private Vector3 mMoveToPos = Vector3.zero;
    private Action mMoveCallback = null;
    private bool mReached = false;
    private bool mTargetMoved = false;
    private Vector3 mTargetPos = Vector3.zero;
    private float mRemainedFrozenTime = 0f;

    VectorAnimation mVectorMove = new VectorAnimation();

    public List<Object_Transparent> mTranObjectList = new List<Object_Transparent>();
    //public Vector3 mCameraOffsetPos;
    //public Vector3 mCameraDirection;
    //float mLookLength;
    //float mNowLookLength;

    public Rect mScreenRange = new Rect(0.1f, 0.1f, 0.8f, 0.8f);
    float mHeightSpeed = 20;
    public bool mShakeEnabled = true;

    //CameraRestoreHeightEvent mRestoreEvent;
    tEvent mMoveFinishEvent;

    public CameraMoveEvent()
    {
        //MainProcess.mMainCamera.transform.localPosition = mCameraOffset;

        //mLookLength = Vector3.Distance(mCameraOffset, Vector3.zero);
        //mNowLookLength = mLookLength;

        //MainProcess.mMainCamera.transform.localRotation = Quaternion.Euler(mCameraAngles);

        //mCameraDirection = mCameraOffset;
        //mCameraDirection.Normalize();
		mScreenRange = new Rect ( mScreenRange.xMin * Screen.width,
		                         mScreenRange.yMin * Screen.height,
        							mScreenRange.width * Screen.width,
		                         mScreenRange.height * Screen.height );

        //mRestoreEvent = EventCenter.Start("CameraRestoreHeightEvent") as CameraRestoreHeightEvent;
    }

    static public void OnTargetObjectMove(ActiveObject obj)
    {
        //if ( MainProcess.mCameraMoveTool==null)
        //    return;
        
        //if (CameraMoveEvent.mMainTarget==obj && obj!=null && MainProcess.mCameraMoveTool.CanMoveCamera())
        //{
        //    MainProcess.mCameraMoveTool._MoveByObject(obj);
        //}
    }

	static public void BindMainObject(ActiveObject mainObject)
    {
        MainProcess.mCameraMoveTool._MoveByObject(mainObject);
    }

    public bool InitConfig(int index)
    {
        DataRecord r = DataCenter.mCamera.GetRecord(index);

        if (r != null)
        {
            mCameraOffset.x = (float)r["COORDINATE_X"];
            mCameraOffset.y = (float)r["COORDINATE_Y"];
            mCameraOffset.z = (float)r["COORDINATE_Z"];
            mCameraAngles.x = (float)r["ANGLE_X"];
            mCameraAngles.y = (float)r["ANGLE_Y"];
            mCameraAngles.z = (float)r["ANGLE_Z"];
            mMaxAccelerate = (float)r["MAX_ACCELERATE"];
            mForwardOffset = (float)r["DISTANCE"];
            mDamping = (float)r["DAMPING"];
            return true;
        }

        return false;
    }

    public void Reset()
    {
        MainProcess.mMainCamera.transform.localPosition = mCameraOffset;
        //mLookLength = Vector3.Distance(mCameraOffset, Vector3.zero);
        //mNowLookLength = mLookLength;
        MainProcess.mMainCamera.transform.localRotation = Quaternion.Euler(mCameraAngles);
        //mCameraDirection = mCameraOffset;
        //mCameraDirection.Normalize();
    }

    public bool CanMoveCamera()
    {
        return !AutoBattleAI.InAutoBattle()
            || GetFinished()
            || (mMainTarget!=null && (mMainTarget.GetCurrentEnemy() == null || mMainTarget.GetCurrentEnemy().IsDead()) )
            ;
    }

    public void MoveTo(Vector3 targetPos)
    {
        MoveTo(targetPos, 0.5f);
    }

    public void MoveTo(Vector3 targetPos, float duration)
    {
        ////mVectorMove.SetPos(MainProcess.mCameraObject.transform.position, targetPos, 2);
        //mVectorMove.StartOnTime(MainProcess.mCameraObject.transform.position, targetPos, useTime);
        //SetFinished(false);
        //StartUpdate();        

        //mCameraOffsetPos = MainProcess.mMainCamera.transform.localPosition;
        //mNowLookLength = Vector3.Distance(mCameraOffsetPos, Vector3.zero);

        //mRestoreEvent.Finish();
        MoveTo(targetPos, duration, null);
    }

    public void MoveTo(Vector3 targetPos, tEvent finishEvent)
    {
        //mMoveFinishEvent = finishEvent;
        //MoveTo(targetPos);
        if (finishEvent == null)
        {
            MoveTo(targetPos, 0.5f, null);
        }
        else 
        {
            MoveTo(targetPos, 0.5f, () => finishEvent.DoEvent());
        }
    }

    public void MoveTo(Vector3 targetPos, float duration, Action callback)
    {
        mLocked = false;
        mMoveFromPos = MainProcess.mCameraObject.transform.position;
        mMoveToPos = targetPos;
        mMoveElapsed = 0f;
        mMoveDuration = duration;
        mMoveCallback = callback;
        mReached = false;
    }

    public override bool _DoEvent()
    {
       SetFinished(false);
       if (mMainTarget == null)
            return false;

       _MoveByObject(mMainTarget);
		return true;
    }

    public void _MoveByObject(ActiveObject targetObject)
    {
        _MoveByObject(targetObject, 0.5f);
    }

    public void _MoveByObject(ActiveObject targetObject, float useTime)
    {
        mMainTarget = targetObject;

        if (mMainTarget == null)
        {
            return;
        }

        mLocked = true;
        mTargetMoved = false;
        mRemainedFrozenTime = 0f;

        //Vector3 mainCharPos = mMainTarget.GetPosition();
        //BaseObject nowEnemy = mMainTarget.GetCurrentEnemy();
       
        //if (nowEnemy != null && !nowEnemy.IsDead())
        //{
        //    if (Vector3.Distance(mainCharPos, nowEnemy.GetPosition()) < msTargetMaxDistance)
        //    {
        //        mainCharPos += mMainTarget.mMoveComponent.mDirection * 1.5f;
        //        mainCharPos.z += 2;

        //            Vector3 enemyPos = nowEnemy.GetPosition();
        //            enemyPos.z += 2;
        //            MoveTo((mainCharPos + enemyPos) * 0.5f, useTime);
        //            return;
        //    }
        //}
       
        //MoveTo(mainCharPos, useTime);        
        //return;
    }

    public void SetCameraShakeEnabled(bool enabled)
    {
        if (!enabled)
        {
            if (Effect_ShakeCamera.Self != null && !Effect_ShakeCamera.Self.GetFinished())
            {
                Effect_ShakeCamera.Self.Finish();
            }
        }

        mShakeEnabled = enabled;
    }

    // 具有平滑阻尼和加速度限制的插值
    private Vector3 LerpPosition(Vector3 current, Vector3 target, float secondTime, Vector3 targetVelocity)
    {
        Vector3 dir = target - current;
        float targetSpeed = targetVelocity.magnitude;
        float factor = (1f + Vector3.Dot(targetVelocity.normalized, dir.normalized)) / 2f;
        Vector3 next = Vector3.Lerp(current, target, mDamping * secondTime * factor);

        if (secondTime > 0.0001f)
        {
            Vector3 speed = (next - current) / secondTime;
            Vector3 accelerate = (speed - mLastSpeed) / secondTime;

            if (speed.sqrMagnitude > mLastSpeed.sqrMagnitude)
            {
                accelerate = Vector3.ClampMagnitude(accelerate, mMaxAccelerate);
            }

            speed = mLastSpeed + accelerate * secondTime;
            speed = Vector3.ClampMagnitude(speed, targetSpeed + 3f);
            next = current + speed * secondTime;
        }

        return next;
    }

    public override bool Update(float secondTime)
    {
        if (MainProcess.mCameraObject == null)
        {
            return false;
        }

        if (mLocked)
        {
            if (mMainTarget != null && mMainTarget.mMainObject != null)
            {
                float targetSqrVelocity = mMainTarget.GetVelocity().sqrMagnitude;

                if (!mTargetMoved)
                {
                    mTargetMoved = targetSqrVelocity > 0.01f;
                }

                Vector3 currentPos = MainProcess.mCameraObject.transform.position;
                //Vector3 targetPos;
                BaseObject fixEnemy = mMainTarget.aiMachine.fixEnemy;
                BaseObject farTarget = mMainTarget.aiMachine.currentTarget;

                if (farTarget != null && AIKit.InBounds(mMainTarget, farTarget, 3f))
                {
                    farTarget = null;
                }

                // 确定跟随目标点
                if (fixEnemy != null || farTarget != null)
                {
                    Vector3 p;

                    if (fixEnemy == null)
                    {
                        p = farTarget.GetPosition();
                    }
                    else if (farTarget == null)
                    {
                        p = fixEnemy.GetPosition();
                    }
                    else
                    {
                        p = (farTarget.GetPosition() + fixEnemy.GetPosition()) / 2f;
                    }

                    float rate = AIKit.Distance(mMainTarget.GetPosition(), p) / mMainTarget.LookBounds();
                    mTargetPos = Vector3.Lerp(p, mMainTarget.GetPosition(), rate);//(mMainTarget.GetPosition() + mMainTarget.aiMachine.fixEnemy.GetPosition()) / 2f;
                    mRemainedFrozenTime = mMaxFrozenTime;
                }
                else if (mTargetMoved && targetSqrVelocity < 0.01f)
                {
                    if (mMainTarget.aiMachine.onBeatBack)
                        mRemainedFrozenTime = 0f;
                    else
                        mRemainedFrozenTime -= Time.deltaTime;

                    if (mRemainedFrozenTime < 0.001f)
                        mTargetPos = mMainTarget.GetPosition() + mMainTarget.GetDirection() * Vector3.forward * mForwardOffset;                                
                }
                else
                {
                    mTargetPos = mMainTarget.GetPosition();
                    mRemainedFrozenTime = 0f;
                }

                Vector3 nextPos;

                // 插值
                if (mMainTarget.aiMachine.onBeatBack)
                {
                    mRemainedFrozenTime = 0f;
                    nextPos = Vector3.Lerp(currentPos, mTargetPos, 10f * secondTime);
                }
                else
                {
                    nextPos = LerpPosition(currentPos, mTargetPos, secondTime, mMainTarget.GetVelocity());
                }

                MainProcess.mCameraObject.transform.position = nextPos;

                if ((nextPos - mLastCheckPos).sqrMagnitude > mCheckBounds * mCheckBounds)
                {
                    mLastCheckPos = nextPos;
                    OnCameraPositionChanged();
                }

                if (secondTime > 0.001f)
                {
                    mLastSpeed = (nextPos - currentPos) / secondTime;
                }
            }
            else 
            {
                mRemainedFrozenTime = 0f;
            }
        }
        else 
        {
            MainProcess.mCameraObject.transform.position = Vector3.Lerp(mMoveFromPos, mMoveToPos, mMoveElapsed / mMoveDuration);

            if (mMoveElapsed >= mMoveDuration && !mReached)
            {
                mReached = true;
                OnCameraPositionChanged();

                if (mMoveCallback != null)
                {
                    mMoveCallback();
                    mMoveCallback = null;
                }
            }

            mMoveElapsed += secondTime;
            mRemainedFrozenTime = 0f;
        }

        return true;
  
        //bool bEnd = mVectorMove.Update(secondTime);

        //Vector3 nowPos = mVectorMove.NowValue();
        ////DEBUG.Log (x.ToString());
        ////if (x.ToString()=="(NaN, NaN, NaN)")
        ////{
        ////	return false;
        ////}

        //MainProcess.mCameraObject.transform.position = nowPos;
        ////if (mMainTarget != null)
        ////{
        ////    BaseObject obj = mMainTarget.GetCurrentEnemy();
        ////    if (mNowLookLength < msCameraMaxHeight && obj != null && !obj.IsDead())
        ////    {
        ////        Vector3 screenPos = MainProcess.mMainCamera.WorldToScreenPoint(obj.GetPosition());
        ////        Vector3 charScreenPos = MainProcess.mMainCamera.WorldToScreenPoint(mMainTarget.GetPosition());

        ////        if (!mScreenRange.Contains(screenPos) || !mScreenRange.Contains(charScreenPos))
        ////        {
        ////            mNowLookLength += secondTime * mHeightSpeed;
        ////            MainProcess.mMainCamera.transform.localPosition = mCameraDirection * mNowLookLength;
        ////            UpdateOverLook();
        ////        }
        ////    }
        ////}
        ////else
        //if (CheckNeedAddHeight())
        //{
        //    mNowLookLength += secondTime * mHeightSpeed;
        //    MainProcess.mMainCamera.transform.localPosition = mCameraDirection * mNowLookLength;
        //    UpdateOverLook();
        //}

        //if (bEnd)
        //{
        //    mRestoreEvent.DoEvent();
        //    if (mMoveFinishEvent != null)
        //    {
        //        mMoveFinishEvent.DoEvent();
        //        mMoveFinishEvent = null;
        //    }
        //}
        
        //OnCameraPositionChanged();

        //return !bEnd;
    }

    public void UpdateOverLook()
    {
        //float an = mRotateValue.x + (mNowLookLength-mLookLength);
        // MainProcess.mMainCamera.transform.localRotation = Quaternion.Euler(an, mRotateValue.y, mRotateValue.z);
    }

    //public bool CheckNeedAddHeight()
    //{
    //    if (mMainTarget == null)
    //        return false;

    //    if (!mMainTarget.AllowCameraUpdateHeight())
    //        return false;

    //    if (mNowLookLength > msCameraMaxHeight)
    //        return false;

    //    BaseObject obj = mMainTarget.GetCurrentEnemy();
    //    if (obj != null && !obj.IsDead())
    //    {
    //        Vector3 screenPos = MainProcess.mMainCamera.WorldToScreenPoint(obj.GetPosition());
    //        Vector3 charScreenPos = MainProcess.mMainCamera.WorldToScreenPoint(mMainTarget.GetPosition());
    //        if (!mScreenRange.Contains(screenPos) || !mScreenRange.Contains(charScreenPos))
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

    //public bool RestoreCameraHeight(float secondTime)
    //{
    //    if (mNowLookLength > mLookLength)
    //    {   
    //        mNowLookLength -= secondTime * mHeightSpeed;
    //        if (mNowLookLength <= mLookLength)
    //        {

    //            mNowLookLength = mLookLength;
    //        }
    //        MainProcess.mMainCamera.transform.localPosition = mCameraDirection * mNowLookLength;
    //        UpdateOverLook();
    //        return true;
    //    }
    //    else
    //        return false;
    //}

    public void OnCameraPositionChanged()
    {
		Vector3 cameraPos = MainProcess.mMainCamera.gameObject.transform.position;
        int mask = 1<<CommonParam.ObstructLayer;     
        //mask |= 1 << 12;
       
		Ray ray = new Ray(MainProcess.mCameraObject.transform.position, cameraPos - MainProcess.mCameraObject.transform.position);

        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, 600, mask);

        if (hits.Length<=0)
        {
            if (mTranObjectList.Count<=0)
                return;

            foreach (Object_Transparent t in mTranObjectList)
            {
                if (!t.mbNeedFinish)
                    t.Finish();
            }  
            //mTranObjectList.Clear();

            return;
        }

        foreach (Object_Transparent t in mTranObjectList)
        {
             t.mbExist =false;
        }  

        for (int index=0; index<hits.Length; ++index)
        {
            RaycastHit hit = hits[index];
            Renderer r = hit.collider.renderer;

            foreach (Object_Transparent t in mTranObjectList)
            {
                if (t.mRenders[0]==r)
                {
                    t.mbExist = true;
                    hits[index].distance = -1;
                    t.ContinueTran();
                    break;
                }
            }            
        }

        for (int index=0; index<mTranObjectList.Count; )
        {
             Object_Transparent t = mTranObjectList[index];
            if ( !t.mbExist )
            {
                if (!t.mbNeedFinish)
                    t.Finish();
                else if (t.GetFinished())
				{
                    mTranObjectList.RemoveAt(index);
					continue;
				}
            }
            
            ++index;
        }  

         for (int index=0; index<hits.Length; ++index)
        {
            RaycastHit hit = hits[index];
             if (hit.distance<0)
                 continue;

            Renderer r = hit.collider.renderer;
             Object_Transparent t = EventCenter.Start("Object_Transparent") as Object_Transparent;
             t.mRenders = new Renderer[1];
             t.mRenders[0] = r;

             t.DoEvent();
             mTranObjectList.Add(t);
        }
    }
}

//class CameraRestoreHeightEvent : CEvent
//{
//    public override bool _DoEvent()
//    {
//        SetFinished(false);
//        if (MainProcess.mCameraMoveTool.RestoreCameraHeight(Time.deltaTime))
//            StartUpdate();
//        else
//            MainProcess.mCameraMoveTool.Finish();

//        return true;
//    }

//    public override bool Update(float dt)
//    {
//        if (MainProcess.mCameraMoveTool.CheckNeedAddHeight())
//        {
//            MainProcess.mCameraMoveTool.Finish();
//            return false;
//        }
//        if (!MainProcess.mCameraMoveTool.RestoreCameraHeight(dt))
//        {
//            MainProcess.mCameraMoveTool.Finish();
//            return false;
//        }
//        return true;
//    }
//}