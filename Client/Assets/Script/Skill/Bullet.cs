using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;
using DataTable;


public enum BULLET_TARGET_TYPE
{
    ENEMY = 0,
    SINGLE,
    FRIEND,
    ALL
}


public class Bullet : Routine
{
    public BaseObject owner { get; private set; }   // 拥有者
    public int bulletEffectID { get; private set; }       // 子弹特效ID
    public float elapsed { get; private set; }      // 已存活时间

    public bool oneshotHit { get; set; } // 是否碰撞后即销毁
    public bool overlapHit { get; set; } // 是否可对同一目标造成多次碰撞
    public BULLET_TARGET_TYPE targetType { get; set; }    // 可碰撞目标类型
    public BaseObject target { get; set; }    // 当可碰撞目标类型为SINGLE时的目标
    public Vector3 position { get; set; }  // 位置 
    public Vector3 velocity { get; set; }  // 速度
    public float hitRange { get; set; }    // 碰撞范围
    public float hitRate { get; set; }     // 碰撞检测间隔
    public float maxTime { get; set; }     // 最大存活时间
    public Action<BaseObject> onHit { get; set; }    // 当碰撞时
    public Action onDestroy { get; set; }   // 当销毁时

    private BaseEffect bulletEffect = null;
    private bool needDestroy = false;
    private Transform bulletTrans = null;
    private List<BaseObject> hitObjects = new List<BaseObject>();
    private Vector3 offset = Vector3.zero;
    private bool firstFrame = true;

    // 共享已碰撞列表
    public static void ShareHitList(params Bullet[] bullets)
    {
        List<BaseObject> combined = new List<BaseObject>();

        foreach (var bullet in bullets)
        {
            foreach (var obj in bullet.hitObjects)
            {
                if (!combined.Contains(obj))
                {
                    combined.Add(obj);
                }
            }

            bullet.hitObjects = combined;
        }
    }

    public Bullet(BaseObject owner, int bulletEffectID)
    {
        this.owner = owner;
        this.bulletEffectID = bulletEffectID;
        this.oneshotHit = true;
        this.overlapHit = false;
        this.targetType = BULLET_TARGET_TYPE.ENEMY;
        this.maxTime = 10f;

        Bind(DoBullet());
    }

    public void Start()
    {
        if (owner == null || bulletEffectID == 0)
        {
            return;
        }

        bulletEffect = BaseEffect.CreateEffect(null, null, bulletEffectID);

        if (bulletEffect == null || bulletEffect.mGraphObject == null)
        {
            return;
        }

        bulletTrans = bulletEffect.mGraphObject.transform;
        offset = bulletTrans.position;

        if (!Routine.IsActive(this))
        {
            bulletTrans.gameObject.StartRoutine(this);
        }
    }

    private IEnumerator DoBullet()
    {
        position += offset;
        bulletTrans.position = position;
        bulletTrans.forward = velocity;
        firstFrame = true;

        Append(CollideDetect());

        yield return null;

        while (!needDestroy && elapsed < maxTime && bulletTrans != null)
        {
            elapsed += Time.deltaTime;
            position += velocity * Time.deltaTime;
            bulletTrans.position = position;
            bulletTrans.forward = velocity;
            firstFrame = false;
            yield return null;
        }

        needDestroy = true;

        if (onDestroy != null)
        {
            onDestroy();
        }

        bulletEffect.FinishImmediate();
    }

    private IEnumerator CollideDetect()
    {
        ErrorCompensationTimer timer = new ErrorCompensationTimer();

        while (true)
        {
            var objMap = ObjectManager.Self.mObjectMap;

            for (int i = objMap.Length - 1; i >= 0; --i)
            {
                var obj = objMap[i];

                if (obj != null 
                    && obj != owner 
                    && obj is ActiveObject 
                    && IsObjectTarget(obj)
                    && CheckCanCollide(obj))
                {
                    if (hitObjects.Contains(obj))
                    {
                        if (overlapHit && onHit != null)
                        {
                            onHit(obj);
                        }
                    }
                    else
                    {
                        hitObjects.Add(obj);

                        if (onHit != null)
                        {
                            onHit(obj);
                        }
                    }

                    PlayBulletHitSound(bulletEffectID);

                    if (oneshotHit)
                    {
                        needDestroy = true;
                    }
                }
            }

            yield return hitRate < 0.05f ? null : timer.DoDelay(hitRate);
        }
    }

    // 为避免当帧间隔过大或速度过快时出现穿透现象，在算法上进行特殊处理
    private bool CheckCanCollide(BaseObject obj)
    {      
        Vector3 nowPos = position - offset;
        Vector3 deltaPos = velocity * Time.deltaTime;
        deltaPos.y = 0f;

        if (firstFrame || hitRate >= 0.05f || deltaPos.sqrMagnitude < 0.01f)
        {
            return AIKit.InBounds(obj, nowPos, hitRange + obj.mImpactRadius);
        }
        else 
        {
            if (AIKit.InBounds(obj, nowPos, hitRange + obj.mImpactRadius))
            {
                return true;
            }
            else
            {
                Vector3 targetPos = obj.GetPosition();
                targetPos.y = 0;
                nowPos.y = 0;
                Vector3 prePos = nowPos - deltaPos;
                Vector3 preToTarget = targetPos - prePos;
                Vector3 nowToTarget = targetPos - nowPos;
                float preReflected = Vector3.Dot(preToTarget, deltaPos);
                float nowReflected = Vector3.Dot(nowToTarget, deltaPos);

                if (preReflected * nowReflected >= 0f)
                {
                    return false;
                }
                else 
                {
                    float minSqrDistance = preToTarget.sqrMagnitude - preReflected * preReflected / deltaPos.sqrMagnitude;
                    float bounds = hitRange + obj.mImpactRadius;
                    return minSqrDistance <= bounds * bounds;
                }
            }
        }
    }

    private bool IsObjectTarget(BaseObject obj)
    {
        switch (targetType)
        {
            case BULLET_TARGET_TYPE.ENEMY:
                return !obj.IsDead() && owner.IsEnemy(obj);

            case BULLET_TARGET_TYPE.FRIEND:
                return !obj.IsDead() && !owner.IsEnemy(obj);

            case BULLET_TARGET_TYPE.SINGLE:
                return obj == target;

            default:
                return !obj.IsDead();
        }
    }

    public void PlayBulletHitSound(int effectID)
    {
        NiceTable effect = DataCenter.mEffect;
        DataRecord effectRecord = effect.GetRecord(effectID);
        if (effectRecord == null) return;
        string modelPath = effectRecord.getData("MODEL");
        string[] str = modelPath.Split('/');
        if (str.Length == 0) return;
        string strEffect = str[str.Length - 1];
        NiceTable effectSoundTable = DataCenter.mEffectSound;
        DataRecord effectSoundRecord = effectSoundTable.GetRecord(strEffect);
        if (effectSoundRecord == null) return;
        GameCommon.PlaySound(effectSoundRecord.getData("HIT_SOUND_FILE"), GameCommon.GetMainCamera().transform.position, effectSoundRecord.getData("SOUND_TYPE"));
    }
}