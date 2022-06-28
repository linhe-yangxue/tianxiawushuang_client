using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using Logic;

//public class FlyBullet
//{
//    protected Vector3 mBeginPosition;
//    protected Vector3 mFlyDirection;
//    protected BaseEffect mBulletEffect;

//    protected ValueAnimation mMoveValue = new ValueAnimation();


//    public FlyBullet(BaseEffect bulletEffect, Vector3 flyDirection, float dis, float speed)
//    {
//        mBulletEffect = bulletEffect;

//        mFlyDirection = flyDirection;

//        mBeginPosition = mBulletEffect.mGraphObject.transform.position; // GetOwner().GetPosition();
//        mBulletEffect.mGraphObject.transform.LookAt(mBeginPosition+mFlyDirection * 100);

//        mMoveValue.Start(0, dis, dis / speed);
//    }




//    public virtual bool Update(float t, ThreeBulletSkill skill, out BaseObject damageObject)
//    {
//        if (mBulletEffect == null || mBulletEffect.mGraphObject == null)
//        {
//            damageObject = null;
//            return true;
//        }
//        float nowValue;
//        bool bEnd = mMoveValue.Update(t, out nowValue);

//        Vector3 nowPos = mBeginPosition + mFlyDirection * nowValue;
//        mBulletEffect.mGraphObject.transform.position = nowPos;

//        damageObject = skill.CheckDamage(nowPos);

//        return bEnd;
//    }

//    public virtual void DestoryBullet()
//    {
//        if (mBulletEffect != null)
//        {
//            mBulletEffect.Finish();
//            mBulletEffect = null;
//        }
//    }
//}

// 多重子弹技能，穿透攻击
public class ThreeBulletSkill : FlySkill//Skill
{
    protected Vector3 mTargetPosition;

    //List<BaseObject> mDamageList = new List<BaseObject>();
    //protected FlyBullet[] mBullet;
    //bool mbStart = false;

    //public override bool needLowUpdate() { return false; }

    //// 修改为穿透攻击
    //public virtual bool CheckOneEnemy() { return false; }

    protected override void OnSkillInit(BaseObject owner, BaseObject target)
    {
        mTargetPosition = target.GetPosition();
    }

    //public override void Stop()
    //{
    //    //LOG.log("Now stop, then finish >" + GetEventInfo());
    //    if (mBullet == null)
    //        Finish();
    //}

    public override bool _DoEvent()
    {       
        mNowStage = EStateStage.START;
        _StartStage();
        ExtraOnStart();
        return true;
    }

    public override void _PlayStartEffect()
    {
        int effectIndex = GetConfig("START_EFFECT");

        if (effectIndex > 0)
        {
            // 如果启用多重预警特效，则根据多重子弹的数量和角度生成多个特效，特效角度同子弹角度一致
            // 注意：如果启用多重特效，则此特效不能实时更新位置，否则角度会被重置
            if (GetConfig("MULT_EFFECT") == 1)
            {
                int bulletCount = GetConfig("DAMAGE_COUNT");
                int deltaAngle = GetConfig("DAMAGE_TYPE_PARAM");
                int beginAngle = (int)(deltaAngle * (bulletCount - 1) * 0.5f);
                Quaternion beginRotation = Quaternion.Euler(0, -beginAngle, 0) * Quaternion.LookRotation(mTargetPosition - GetOwner().GetPosition());

                for (int i = 0; i < bulletCount; ++i)
                {
                    BaseEffect eff = mOwner.PlayEffect(effectIndex, mTarget);

                    if (eff != null && eff.mGraphObject != null)
                    {
                        eff.mGraphObject.transform.rotation = Quaternion.Euler(0, deltaAngle * i, 0) * beginRotation;
                    }
                }
            }
            else
            {
                mOwner.PlayEffect(effectIndex, mTarget);
            }
        }
    }

    //public override bool Update(float secondTime)
    //{
    //    bool bEnd = true;
    //    foreach (FlyBullet bullet in mBullet)
    //    {
    //        if (bullet!=null)
    //        {
    //            BaseObject damageObject;
    //            if (!bullet.Update(secondTime, this, out damageObject))
    //            {
    //                if (bEnd)
    //                    bEnd = false;
    //            }
    //            else
    //                bullet.DestoryBullet();

    //            if (damageObject != null)
    //            {
    //                _Damage(damageObject);
    //                if (CheckOneEnemy())
    //                {
    //                    bullet.DestoryBullet();
    //                }
    //            }
    //        }
    //    }

    //    if (bEnd)
    //        Finish();

    //    return !bEnd;
    //}

    //public virtual BaseObject CheckDamage(Vector3 bulletPos)
    //{
    //    float range = GetConfig("DAMAGE_RANGE");

    //    // off body height
    //    bulletPos.y -= 1.5f;

    //    foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
    //    {
    //        if (obj != mOwner
    //            && obj != null
    //            && !obj.IsDead()
    //            && mOwner.IsEnemy(obj)
    //            )
    //        {
    //            if (Skill.InRange(obj.GetPosition(), bulletPos, range + obj.mImpactRadius))
    //            {
    //                bool bAlreadyDamage = false;

    //                foreach (BaseObject o in mDamageList)
    //                {
    //                    if (o == obj)
    //                    {
    //                        bAlreadyDamage = true;
    //                        break;
    //                    }
    //                }
    //                if (bAlreadyDamage)
    //                    continue;

    //                mDamageList.Add(obj);
    //                return obj;
    //            }
    //        }
    //    }

    //    return null;
    //}

    //public virtual void _Damage(BaseObject target)
    //{
    //    int damageEffect = GetConfig("DAMAGE_EFFECT");
    //    if (damageEffect > 0)
    //        target.PlayEffect(damageEffect, mOwner);

    //    float beatBack = GetConfig("BEAT_BACK");
    //    if (beatBack > 0.0001f)
    //    {
    //        Vector3 dir = target.GetPosition() - GetOwner().GetPosition();
    //        dir.Normalize();
    //        target.BeatBack(dir, beatBack);
    //    }

    //    string motionName = GetConfig("HIT_MOTION");
    //    if (motionName != "")
    //    {
    //        target.OnHit(motionName, mOwner);
    //    }


    //    int damageValue = mOwner.TotalDamage(target, (int)GetConfig("DAMAGE"), this);
    //    if (damageValue > 0)
    //    {
    //        target.ApplyDamage(damageValue);
    //    }
    //}

    //public override bool _AttackStage()
    //{
    //    string motionName = GetConfig("ATTACK_MOTION");

    //    if (motionName != "")
    //    {
    //        mOwner.SetDirection(mTarget.GetPosition());
    //        mOwner.PlayMotion(motionName, null);
    //    }

    //    int effectIndex = GetConfig("ATTACK_EFFECT");


    //    if ((bool)GetConfig("CAMERA_SHAKE"))
    //    {
    //        Effect_ShakeCamera.Shake(0.9f, 0.4f);
    //    }

    //    if (effectIndex > 0)
    //    {
    //        float dis = (float)GetConfig("FLY_LENGTH");
    //        float speed = (float)GetConfig("FLY_SPEED");
    //        int bulletCount = GetConfig("DAMAGE_COUNT");
    //        int oneAngle = GetConfig("DAMAGE_TYPE_PARAM");
            
    //        int beginAngle = (int)(oneAngle * (bulletCount-1) *0.5f);
    //        Quaternion qu = Quaternion.Euler(0, -beginAngle, 0);
    //        Vector3 flyDirection = qu * (mTargetPosition - GetOwner().GetPosition());
    //        flyDirection.Normalize();

    //        mBullet = new FlyBullet[bulletCount];

    //        for (int i = 0; i < bulletCount; ++i)
    //        {
    //            BaseEffect bulletEffect = BaseEffect.CreateEffect(null, null, effectIndex);// mOwner.PlayEffect(effectIndex, mTarget);
    //            if (bulletEffect != null)
    //            {
    //                bulletEffect.SetPosition(mOwner.GetPosition());
    //                Quaternion ro = Quaternion.Euler(0, oneAngle * i, 0);
    //                Vector3 dir = ro * flyDirection;
    //                mBullet[i] = new FlyBullet(bulletEffect, dir, dis, speed);
    //            }
    //            else
    //            {
    //                Log("ERROR: create bullet effect fail>" + effectIndex.ToString());
    //            }
    //        }

    //    }

    //    StartUpdate();

    //    return true;
    //}

    protected override void OnAttackStage()
    {
        int effectIndex = GetConfig("ATTACK_EFFECT");

        //if (effectIndex > 0)
        //{
        //    float dis = (float)GetConfig("FLY_LENGTH");
        //    float speed = (float)GetConfig("FLY_SPEED");
        //    int bulletCount = GetConfig("DAMAGE_COUNT");
        //    int oneAngle = GetConfig("DAMAGE_TYPE_PARAM");

        //    int beginAngle = (int)(oneAngle * (bulletCount - 1) * 0.5f);
        //    Quaternion qu = Quaternion.Euler(0, -beginAngle, 0);
        //    Vector3 flyDirection = qu * (mTargetPosition - GetOwner().GetPosition());
        //    flyDirection.Normalize();

        //    mBullet = new FlyBullet[bulletCount];

        //    for (int i = 0; i < bulletCount; ++i)
        //    {
        //        BaseEffect bulletEffect = BaseEffect.CreateEffect(null, null, effectIndex);// mOwner.PlayEffect(effectIndex, mTarget);
        //        if (bulletEffect != null)
        //        {
        //            bulletEffect.SetPosition(mOwner.GetPosition());
        //            Quaternion ro = Quaternion.Euler(0, oneAngle * i, 0);
        //            Vector3 dir = ro * flyDirection;
        //            mBullet[i] = new FlyBullet(bulletEffect, dir, dis, speed);
        //        }
        //        else
        //        {
        //            Log("ERROR: create bullet effect fail>" + effectIndex.ToString());
        //        }
        //    }
        //}

        //StartUpdate();
        if (effectIndex > 0)
        {
            float speed = (float)GetConfig("FLY_SPEED");
            int bulletCount = GetConfig("DAMAGE_COUNT");
            int deltaAngle = GetConfig("DAMAGE_TYPE_PARAM");
            int beginAngle = (int)(deltaAngle * (1 - bulletCount) * 0.5f);
            Vector3 dir = mTargetPosition - GetOwner().GetPosition();

            if (dir.magnitude < 0.001f)
            {
                dir = GetOwner().GetDirection() * Vector3.forward;
            }

            dir.Normalize();

            Bullet[] bullets = new Bullet[bulletCount];

            for (int i = 0; i < bulletCount; ++i)
            {
                Quaternion rotation = Quaternion.Euler(0, beginAngle + i * deltaAngle, 0);
                Vector3 velocity = rotation * dir * speed;
                bullets[i] = CreateBullet(effectIndex);
                bullets[i].velocity = velocity;
            }

            Bullet.ShareHitList(bullets);

            foreach (var bullet in bullets)
            {
                bullet.Start();
            }
        }
    }

    //public override void _OnOverTime()
    //{
    //    if (mNowStage == EStateStage.START)
    //    {
    //        _AttackStage();
    //        mNowStage = EStateStage.ATTACK;
      
    //        float t = GetConfig("ATTACK_TIME");
    //        t -= (float)GetConfig("START_TIME");
    //        if (t < 0.0001f)
    //            _OnOverTime();
    //        else
    //            WaitTime(t);
    //    }
    //    else
    //        _ApplyFinish();
    //}

    //public override bool _OnFinish()
    //{
    //    if (mBullet!=null)
    //    {
    //        foreach (FlyBullet bullet in mBullet)
    //        {
    //            if (bullet != null)
    //                bullet.DestoryBullet();
    //        }
    //    }
    //    return true;
    //}

}

