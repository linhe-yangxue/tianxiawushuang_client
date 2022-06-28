using UnityEngine;
using System.Collections;
using DataTable;
using Utilities.Routines;


public class AbsorbEffectBirth : MonoBehaviour
{
    public int mIndex = 0;

    [HideInInspector]
    public float mDelay = 0f;
    [HideInInspector]
    public float mInterval = 0f;
    [HideInInspector]
    public float mBuffTime = 9999f;
    [HideInInspector]
    public int mEffect = 0;
    [HideInInspector]
    public int mEffectOnAbsorb = 0;
    [HideInInspector]
    public int mBuffId = 0;
    [HideInInspector]
    public float mRandRadius = 0f;

    private void Start()
    {
        if (MainProcess.mStage == null)
        {
            Destroy(gameObject);
            return;
        }

        int key = MainProcess.mStage.mConfigIndex * 1000 + mIndex;
        DataRecord record = DataCenter.mSceneBuff.GetRecord(key);

        if (record != null)
        {
            mBuffId = record["BUFF"];
            mDelay = record["BUFF_DELAY"];
            mInterval = record["BUFF_CD"];
            mBuffTime = record["BUFF_TIME"];
            mEffect = record["BUFF_EFFECT"];
            mEffectOnAbsorb = record["ABSORB_EFFECT"];
            mRandRadius = record["BUFF_RANDOM"];
        }

        if (mBuffId <= 0 || mEffect <= 0)
        {
            Destroy(gameObject);
            return;
        }

        MeshRenderer renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        renderer.enabled = false;

        gameObject.StartRoutine(DoGenerate());
    }

    private IEnumerator DoGenerate()
    {
        yield return new WaitUntil(() => MainProcess.mStage != null && MainProcess.mStage.mbBattleActive);

        yield return new Delay(mDelay);

        while (true)
        {
            if (MainProcess.mStage == null || MainProcess.mStage.mbBattleFinish)
            {
                yield break;
            }

            var effect = BaseEffect.CreateEffect(null, null, mEffect);

            if (effect != null && effect.mGraphObject != null)
            {
                if (mRandRadius < 0.001f)
                {
                    effect.SetPosition(transform.position);
                }
                else
                {
                    effect.SetPosition(MathTool.RandPosInCircle(transform.position, mRandRadius));
                }

                var r = new AbsorbBuffRoutine(effect, mBuffId, mEffectOnAbsorb);
                r.maxStayTime = mBuffTime;
                effect.mGraphObject.StartRoutine(r);
            }

            yield return new Delay(mInterval);
        }
    }
}