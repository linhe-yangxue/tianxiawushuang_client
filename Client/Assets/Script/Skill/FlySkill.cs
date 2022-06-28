using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using Utilities.Routines;


public class BulletScript : MonoBehaviour
{
    public Skill mSkill;

    //void OnDestroy()
    //{
    //    if (mSkill != null)
    //    {
    //        mSkill._ApplyFinish();
    //        mSkill.Finish();
    //    }
    //}
}


// 子弹技能，非穿透攻击
public class BulletSkill : Skill
{
	protected Vector3 mBeginPosition;
	protected Vector3 mFlyDirection = Vector3.zero;
	protected BaseEffect mBulletEffect = null;
	
	protected ValueAnimation mMoveValue = new ValueAnimation();

    protected Vector3 mTargetPosition;

    public override bool needLowUpdate() { return false; }

    protected override void OnSkillInit(BaseObject owner, BaseObject target)
    {
        mTargetPosition = target.GetPosition();
    }

    public override void Stop()
    {
        //LOG.log("Now stop, then finish >" + GetEventInfo());
        if (mBulletEffect==null)
            Finish();
    }

    public override bool _DoEvent()
    {
        //LOG.log("Bullet start > " + GetEventInfo());      
        mNowStage = EStateStage.START;
        _StartStage();
        ExtraOnStart();
        return true;
    }

    //public virtual void _Damage(BaseObject target)
    //{
    //    int damageValue = mOwner.TotalDamage(target, (int)GetConfig("DAMAGE"), this);
    //    if (damageValue > 0)
    //    {
    //        target.ApplyDamage(damageValue);
    //    }
    //}
	
    //public override bool Update(float t)
    //{
    //    if (mBulletEffect.mGraphObject == null)
    //    {
    //        mBulletEffect = null;
    //        _ApplyFinish();
    //        Finish();
    //        return false;
    //    }
    //    float nowValue;
    //    bool bEnd = mMoveValue.Update(t, out nowValue);


    //    Vector3 nowPos = mBeginPosition + mFlyDirection * nowValue;
    //    string v = nowPos.ToString();

    //    if (v.LastIndexOf("NaN") >= 0)
    //    {
    //        Logic.EventCenter.Log(LOG_LEVEL.ERROR, "Use time is zero for ValueAnimation Start " + (string)GetConfig("INDEX"));
    //        _ApplyFinish();
    //        Finish();
    //        return false;
    //    }

    //    mBulletEffect.mGraphObject.transform.position = nowPos;

    //    float range = GetConfig("DAMAGE_RANGE");

    //    // off body height
    //    nowPos.y -= 1.5f;

    //    //Vector2 p = new Vector2(nowPos.x, nowPos.z);
    //    foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
    //    {
    //        if (obj != mOwner
    //            && obj != null
    //            //&& !obj.IsDead()
    //            && obj is ActiveObject
    //            && obj == mTarget//mOwner.IsEnemy(obj)
    //            )
    //        {
    //            if (Skill.InRange(obj.GetPosition(), nowPos, range + obj.mImpactRadius))
    //            {
    //                if (obj.IsDead())
    //                {
    //                    DestoryBullet();
    //                    return false;
    //                }
                    
    //                int damageEffect = GetConfig("DAMAGE_EFFECT");
    //                if (damageEffect > 0)
    //                    obj.PlayEffect(damageEffect, mOwner);

    //                float beatBack = GetConfig("BEAT_BACK");
    //                if (beatBack > 0.0001f)
    //                {
    //                    Vector3 dir = obj.GetPosition() - GetOwner().GetPosition();
    //                    dir.Normalize();
    //                    obj.BeatBack(dir, beatBack);
    //                }

    //                string motionName = GetConfig("HIT_MOTION");
    //                if (motionName != "")
    //                {
    //                    obj.OnHit(motionName, mOwner);
    //                }

    //                float damageDelay = GetConfig("DO_HIT_TIME");

    //                if (damageDelay > 0.001f)
    //                {
    //                    _DamageLater(obj, damageDelay);
    //                }
    //                else
    //                {
    //                    _Damage(obj);
    //                }

    //                DestoryBullet();
    //                return false;
    //            }
    //        }
    //    }

    //    if (bEnd)
    //    {
    //        DestoryBullet();
    //    }
    //    return !bEnd;
    //}
	
    //public override bool _AttackStage()
    //{
    //    //DEBUG.LogWarning(GetEventInfo()+">attack bullte owner now ai>" + GetOwner().mCurrentAI.GetEventInfo());

    //    string motionName = GetConfig("ATTACK_MOTION");
		
    //    if (motionName != "")
    //    {
    //        mOwner.SetDirection(mTarget.GetPosition());
    //        mOwner.PlayMotion(motionName, null);
    //    }

    //    if ((bool)GetConfig("CAMERA_SHAKE"))
    //    {
    //        Effect_ShakeCamera.Shake(0.9f, 0.4f);
    //    }

    //    int effectIndex = GetConfig("ATTACK_EFFECT");
		
    //    if (effectIndex > 0)
    //    {
    //        mBulletEffect = BaseEffect.CreateEffect(null, null, effectIndex); //mOwner.PlayEffect(effectIndex, mTarget);				

    //        if (mBulletEffect==null || mBulletEffect.mGraphObject==null)
    //        {
    //            DEBUG.LogError("No exist bullet effect at skill >"+(string)GetConfig("INDEX"));
    //            _ApplyFinish();
    //            Finish ();
    //            return false;
    //        }

    //        mBeginPosition = mBulletEffect.mGraphObject.transform.position + GetOwner().GetPosition();
    //        mFlyDirection = mTargetPosition - GetOwner().GetPosition();
    //        mFlyDirection.Normalize();

    //        mBulletEffect.SetPosition(GetOwner().GetPosition());
    //        mBulletEffect.mGraphObject.transform.LookAt(GetOwner().GetPosition() + mFlyDirection * 100);
          
    //        BulletScript sp = mBulletEffect.mGraphObject.AddComponent<BulletScript>();
    //        sp.mSkill = this;

    //        float dis = (float)GetConfig("FLY_LENGTH");
    //        float speed = (float)GetConfig("FLY_SPEED");

    //        if (dis <= 0.0001f || speed <= 0.0001f)
    //        {
    //            Logic.EventCenter.Log(LOG_LEVEL.ERROR, "Use time is zero for ValueAnimation Start>" + (string)GetConfig("INDEX"));
    //            _ApplyFinish();
    //            Finish();
    //            return false;
    //        }

    //        mMoveValue.Start(0, dis, dis / speed);
		
    //        StartUpdate();
    //    }
    //    else
    //    {
    //        mBeginPosition = GetOwner().GetPosition();
    //        mFlyDirection = mTarget.GetPosition() - GetOwner().GetPosition();
    //        mFlyDirection.Normalize();
    //        EventCenter.Log(LOG_LEVEL.ERROR, "Bullet skill not config bullet effect >"+(string)GetConfig("INDEX"));
    //        _ApplyFinish();
    //        Finish();
    //    }
    //    return true;
    //}

    protected override void OnAttackStage()
    {
        if (mTarget == null || mTarget.IsDead())
        {
            _ApplyFinish();
            Finish();
            return;
        }

        int effectIndex = GetConfig("ATTACK_EFFECT");

        if (effectIndex > 0)
        {
            //mBulletEffect = BaseEffect.CreateEffect(null, null, effectIndex); //mOwner.PlayEffect(effectIndex, mTarget);				

            //if (mBulletEffect == null || mBulletEffect.mGraphObject == null)
            //{
            //    DEBUG.LogError("No exist bullet effect at skill >" + (string)GetConfig("INDEX"));
            //    _ApplyFinish();
            //    Finish();
            //    return;
            //}

            //mBeginPosition = mBulletEffect.mGraphObject.transform.position + GetOwner().GetPosition();
            //mFlyDirection = mTargetPosition - GetOwner().GetPosition();
            //mFlyDirection.Normalize();

            //mBulletEffect.SetPosition(GetOwner().GetPosition());
            //mBulletEffect.mGraphObject.transform.LookAt(GetOwner().GetPosition() + mFlyDirection * 100);

            //BulletScript sp = mBulletEffect.mGraphObject.AddComponent<BulletScript>();
            //sp.mSkill = this;

            //float dis = (float)GetConfig("FLY_LENGTH");
            //float speed = (float)GetConfig("FLY_SPEED");

            //if (dis <= 0.0001f || speed <= 0.0001f)
            //{
            //    Logic.EventCenter.Log(LOG_LEVEL.ERROR, "Use time is zero for ValueAnimation Start>" + (string)GetConfig("INDEX"));
            //    _ApplyFinish();
            //    Finish();
            //    return;
            //}

            //mMoveValue.Start(0, dis, dis / speed);
            //StartUpdate();

            Bullet bullet = CreateBullet(effectIndex);
            bullet.Start();
        }
        else
        {
            //mBeginPosition = GetOwner().GetPosition();
            //mFlyDirection = mTarget.GetPosition() - GetOwner().GetPosition();
            //mFlyDirection.Normalize();
            EventCenter.Log(LOG_LEVEL.ERROR, "Bullet skill not config bullet effect >" + (string)GetConfig("INDEX"));
            _ApplyFinish();
            Finish();
        }
    }

    protected virtual Bullet CreateBullet(int effectIndex)
    {
        float dis = (float)GetConfig("FLY_LENGTH");
        float speed = (float)GetConfig("FLY_SPEED");
        float range = GetConfig("DAMAGE_RANGE");

        Bullet bullet = new Bullet(GetOwner(), effectIndex);
        bullet.hitRange = range;
        bullet.maxTime = dis / speed;
        bullet.position = GetOwner().GetPosition();
        Vector3 velocity = mTargetPosition - GetOwner().GetPosition();

        if (velocity.sqrMagnitude < 0.001f)
        {
            velocity = GetOwner().GetDirection() * Vector3.forward;
        }

        velocity.Normalize();
        bullet.velocity = speed * velocity;
        bullet.targetType = BULLET_TARGET_TYPE.SINGLE;
        bullet.target = mTarget;
        bullet.overlapHit = false;
        bullet.oneshotHit = true;
        bullet.onHit = OnBulletHit;
        return bullet;
    }

    protected virtual void OnBulletHit(BaseObject target)
    {
        if (target.IsDead())
        {
            return;
        }

        int damageEffect = GetConfig("DAMAGE_EFFECT");

        if (damageEffect > 0)
        {
            target.PlayEffect(damageEffect, mOwner);
        }

        float beatBack = GetConfig("BEAT_BACK");

        if (beatBack > 0.0001f)
        {
            Vector3 dir = target.GetPosition() - GetOwner().GetPosition();
            dir.Normalize();
            target.BeatBack(dir, beatBack);
        }

        string motionName = GetConfig("HIT_MOTION");

        if (motionName != "")
        {
            target.OnHit(motionName, mOwner);
        }

        float damageDelay = GetConfig("DO_HIT_TIME");

        if (damageDelay > 0.001f)
        {
            _AffectLater(target, damageDelay);
        }
        else
        {
            _Affect(target);
        }
    }

    public override void _OnOverTime()
    {
        if (GetFinished())
            return;

        if (mNowStage == EStateStage.START)
        {
            _AttackStage();
            mNowStage = EStateStage.ATTACK;
            ExtraOnAttack();

            float t = GetConfig("ATTACK_TIME");
            t -= (float)GetConfig("START_TIME");
            if (t < 0.0001f)
                _OnOverTime();
            else
                WaitTime(t);
        }
        else
            _ApplyFinish();
    }

    public override bool _ApplyFinish()
    {
        base._ApplyFinish();

        //DEBUG.LogWarning("Bullet apply finish>" + GetEventInfo());

		return true;
    }

    public virtual void DestoryBullet()
	{
        if (mBulletEffect != null)
        {            
            //mBulletEffect.Finish();
            mBulletEffect.FinishImmediate();
            mBulletEffect = null;                                   
        }
	}

    public override bool _OnFinish()
    {
        if (mBulletEffect != null)
            mBulletEffect.FinishImmediate();

        return base._OnFinish();
    }

    protected override void ExtraOnAttack()
    {
        // 附加攻击特效，仅用于表现，不可用于AOE技能
        int attackEffectID = mExtraParam.GetInt("attack_effect", 0);

        if (attackEffectID > 0)
        {
            var eff = mOwner.PlayEffect(attackEffectID, mTarget);

            if (eff != null && eff.mGraphObject != null)
            {
                eff.mGraphObject.transform.forward = mTargetPosition - GetOwner().GetPosition();
            }           
        }
    }
}

// 子弹技能，穿透攻击
public class FlySkill : BulletSkill
{
    //List<BaseObject> mDamageList = new List<BaseObject>();
	
    //public virtual void UpdateEffect(Vector3 nowPos)
    //{
    //    if (mBulletEffect!=null && mBulletEffect.mGraphObject!=null)
    //        mBulletEffect.mGraphObject.transform.position = nowPos;
    //}

    protected override Bullet CreateBullet(int effectIndex)
    {
        Bullet bullet = base.CreateBullet(effectIndex);
        bullet.targetType = isFriendSkill ? BULLET_TARGET_TYPE.FRIEND : BULLET_TARGET_TYPE.ENEMY;
        bullet.oneshotHit = false;
        return bullet;
    }

    //public override bool Update(float t)
    //{
    //    float nowValue;
    //    bool bEnd = mMoveValue.Update(t, out nowValue);
		
    //    Vector3 nowPos = mBeginPosition + mFlyDirection * nowValue;
    //    UpdateEffect(nowPos);
		
    //    float range = GetConfig("DAMAGE_RANGE");

    //    foreach (BaseObject obj in ObjectManager.Self.mObjectMap)
    //    {
    //        if (obj != mOwner
    //            && obj != null
    //            && !obj.IsDead()
    //            && obj is ActiveObject
    //            && mOwner.IsEnemy(obj)
    //            )
    //        {
    //            if (Skill.InRange(obj.GetPosition(), nowPos, range + obj.mImpactRadius))
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

    //                int damageEffect = GetConfig("DAMAGE_EFFECT");

    //                if (damageEffect > 0)
    //                    obj.PlayEffect(damageEffect, mOwner);

    //                _Damage(obj);
    //                mDamageList.Add(obj);
    //            }
    //        }
    //    }
		
    //    if (bEnd)
    //    {			
    //        DestoryBullet();
    //    }

    //    return !bEnd;
    //}
}

// 龙卷风 附加特效自身向前移动
public class TwisterSkill : FlySkill
{
    protected float mEffectSpace = 1;
    //protected float mNextEffectDis = 1;

    public override bool _DoEvent()
    {
        mEffectSpace = GetConfig("EFFECT_SPACE");
        //mNextEffectDis = mEffectSpace;
        return base._DoEvent();
    }

    protected override Bullet CreateBullet(int effectIndex)
    {
        Bullet bullet = base.CreateBullet(effectIndex);
        bullet.targetType = isFriendSkill ? BULLET_TARGET_TYPE.FRIEND : BULLET_TARGET_TYPE.ENEMY;
        bullet.oneshotHit = false;
        bullet.Append(OnBulletMove(bullet));
        return bullet;
    }

    protected virtual IEnumerator OnBulletMove(Bullet bullet)
    {
        float speed = GetConfig("FLY_SPEED");

        if (speed > 0.001f && mEffectSpace > 0.001f)
        {
            float rate = mEffectSpace / speed;

            while (true)
            {
                yield return new Delay(rate);
                OnUpdateEffect(bullet);
            }
        }
    }

    protected virtual void OnUpdateEffect(Bullet bullet)
    {
        float range = GetConfig("DAMAGE_RANGE");
        float nextDis = mEffectSpace + Random.Range(0, range);
        int effectIndex = GetConfig("ATTACK_EFFECT");

        if (effectIndex > 0)
        {
            BaseEffect mBoundEffect = BaseEffect.CreateEffect(mOwner, mTarget, effectIndex);

            if (mBoundEffect != null)
            {
                Vector3 pos = MathTool.RandPosInCircle(bullet.position, range);
                mBoundEffect.SetPosition(pos);
                EffectMove move = mBoundEffect.mGraphObject.AddComponent<EffectMove>();
                move.mMovePath = new VectorAnimation();
                move.mMovePath.Start(pos, pos + mFlyDirection * nextDis, mMoveValue.mParamValue * 0.1f);
            }
        }
    }
    //public override bool _AttackStage()
    //{
    //    string motionName = GetConfig("START_MOTION");
		
    //    if (motionName != "")
    //    {
    //        mOwner.SetDirection(mTarget.GetPosition());
    //        mOwner.PlayMotion(motionName, null);
    //    }
		
    //    //int effectIndex = GetConfig("ATTACK_EFFECT");
		
    //    //if (effectIndex > 0)
    //    //    mBulletEffect = mOwner.PlayEffect(effectIndex, mTarget);
		
    //    if ((bool)GetConfig("CAMERA_SHAKE"))
    //    {
    //        Effect_ShakeCamera.Shake(0.9f, 0.4f);
    //    }

    //    mBeginPosition = mOwner.GetPosition();
    //    mFlyDirection = mTarget.GetPosition() - GetOwner().GetPosition();
    //    mFlyDirection.Normalize();
		
    //    float dis = (float)GetConfig("FLY_LENGTH");
    //    float speed = (float)GetConfig("FLY_SPEED");
    //    mMoveValue.Start(0, dis, dis / speed);
        
    //    StartUpdate();
        
    //    return true;
    //}

    //protected override void OnAttackStage()
    //{
    //    mBeginPosition = mOwner.GetPosition();
    //    mFlyDirection = mTarget.GetPosition() - GetOwner().GetPosition();
    //    mFlyDirection.Normalize();

    //    float dis = (float)GetConfig("FLY_LENGTH");
    //    float speed = (float)GetConfig("FLY_SPEED");
    //    mMoveValue.Start(0, dis, dis / speed);

    //    StartUpdate();
    //}

    //public override void UpdateEffect(Vector3 nowPos)
    //{
    //    if (mMoveValue.mNowValue > mNextEffectDis)
    //    {
    //        float range = GetConfig("DAMAGE_RANGE");
    //        float nextDis = mEffectSpace + Random.Range(0, range);
    //        mNextEffectDis += nextDis;

    //        int effectIndex = GetConfig("ATTACK_EFFECT");
    //        if (effectIndex > 0)
    //        {
    //            BaseEffect mBoundEffect = BaseEffect.CreateEffect(mOwner, mTarget, effectIndex);
    //            if (mBoundEffect != null)
    //            {
    //                Vector3 pos = MathTool.RandPosInCircle(nowPos, range);

    //                mBoundEffect.SetPosition(pos);
    //                EffectMove move = mBoundEffect.mGraphObject.AddComponent<EffectMove>();
    //                move.mMovePath = new VectorAnimation();

    //                move.mMovePath.Start(pos, pos + mFlyDirection * nextDis, mMoveValue.mParamValue * 0.1f);
    //            }
    //        }
    //    }
    //}
}

// 地刺技能 特效出现后 特效不再移动
public class LurkerSkill : TwisterSkill
{
    //public override void UpdateEffect(Vector3 nowPos)
    //{
    //    if (mMoveValue.mNowValue > mNextEffectDis)
    //    {
    //        float range = GetConfig("DAMAGE_RANGE");
    //        float nextDis = mEffectSpace + Random.Range(0, range);
    //        mNextEffectDis += nextDis;
			
    //        int effectIndex = GetConfig("ATTACK_EFFECT");
    //        if (effectIndex > 0)
    //        {
    //            BaseEffect mBoundEffect = BaseEffect.CreateEffect(mOwner, mTarget, effectIndex);
    //            if (mBoundEffect != null)
    //            {
    //                Vector3 pos = nowPos;// MathTool.RandPosInCircle(nowPos, range);
					
    //                mBoundEffect.SetPosition(pos);
    //            }
    //        }
    //    }
    //}
    protected override void OnUpdateEffect(Bullet bullet)
    {
        float range = GetConfig("DAMAGE_RANGE");
        float nextDis = mEffectSpace + Random.Range(0, range);
        int effectIndex = GetConfig("ATTACK_EFFECT");

        if (effectIndex > 0)
        {
            BaseEffect mBoundEffect = BaseEffect.CreateEffect(mOwner, mTarget, effectIndex);

            if (mBoundEffect != null)
            {
                mBoundEffect.SetPosition(bullet.position);
            }
        }
    }
}