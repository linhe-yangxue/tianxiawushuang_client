using UnityEngine;
using System.Collections;
using Utilities.Routines;


public class AbsorbEffectRoutine : Routine
{    
    public float absorbRadius = 1f;
    public float maxStayTime = 9999f;
    public float speed = 8f;

    private BaseEffect effect;

    public AbsorbEffectRoutine(BaseEffect effect)
    {
        this.effect = effect;
        Bind(DoAbsorb());
    }

    private IEnumerator DoAbsorb()
    {
        if (effect == null || effect.mGraphObject == null || Character.Self == null)
        {
            yield break;
        }

        GameObject obj = effect.mGraphObject;
        float elapsed = 0f;

        // 第一阶段： 等待主角移动到吸附范围内
        while (!AIKit.InBounds(Character.Self, obj.transform.position, absorbRadius))
        {
            yield return null;

            elapsed += Time.deltaTime;

            if (elapsed > maxStayTime)
            {
                effect.FinishImmediate();
                yield break;
            }
        }

        float impactRadius = Mathf.Max(Character.Self.mImpactRadius, 0.5f);

        // 第二阶段：被吸附后飞向主角
        while (!AIKit.InBounds(Character.Self, obj.transform.position, impactRadius))
        {
            yield return null;

            Vector3 dir = Character.Self.GetPosition() - obj.transform.position;
            dir.Normalize();
            obj.transform.Translate(dir * speed * Time.deltaTime);
        }

        // 第三阶段：吸附后产生效果
        OnAbsorb();
        effect.FinishImmediate();
    }

    protected virtual void OnAbsorb()
    { }
}



public class AbsorbBuffRoutine : AbsorbEffectRoutine
{
    private int buff;
    private int effectOnAbsorb;

    public AbsorbBuffRoutine(BaseEffect effect, int buff, int effectOnAbsorb)
        : base(effect)
    {
        this.buff = buff;
        this.effectOnAbsorb = effectOnAbsorb;
    }

    protected override void OnAbsorb()
    {
        if (MainProcess.mStage != null && !MainProcess.mStage.mbBattleFinish && Character.Self != null && !Character.Self.IsDead())
        {
            if (effectOnAbsorb > 0)
            {
                Character.Self.PlayEffect(effectOnAbsorb, null);
            }

            if (buff > 0)
            {
                Character.Self.StartAffect(Character.Self, buff);
            }
        }
    }
}