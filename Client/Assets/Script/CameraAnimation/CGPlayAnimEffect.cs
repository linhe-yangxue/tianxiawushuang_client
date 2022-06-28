using UnityEngine;
using System.Collections;
using Utilities.Routines;


public class CGPlayAnimEffect : MonoBehaviour
{
    public int objectIndex;
    public string animName = "idle";
    public float animSpeed = 1f;
    public int effect1 = 0;
    public float effectTime1 = 0f;
    public int effect2 = 0;
    public float effectTime2 = 0f;
    public int effect3 = 0;
    public float effectTime3 = 0f;
    public string sound2D = "";

    public void Play(BaseObject owner)
    {
        PlayAnim(owner);
        PlayEffect(owner, effect1, effectTime1);
        PlayEffect(owner, effect2, effectTime2);
        PlayEffect(owner, effect3, effectTime3);
        Play2DSound();
    }

    private void PlayAnim(BaseObject owner)
    {
        if (owner == null && objectIndex > 0)
        {
            var objMap = ObjectManager.Self.mObjectMap;

            for (int i = objMap.Length - 1; i >= 0; --i)
            {
                if (objMap[i] != null && objMap[i].mConfigIndex == objectIndex)
                {
                    owner = objMap[i];
                    break;
                }
            }
        }

        if (owner != null)
        {
            if (!string.IsNullOrEmpty(animName))
                owner.PlayAnim(animName);

            owner.SetAnimSpeed(animSpeed);
        }   
    }

    private void PlayEffect(BaseObject owner, int effectIndex, float effectTime)
    {
        if (effectIndex > 0)
        {
            var effect = BaseEffect.CreateEffect(owner, null, effectIndex);

            if (effectTime > 0.001f && effect.mGraphObject != null)
            {
                effect.mGraphObject.StartRoutine(new Series(new Delay(effectTime), new Act(() => effect.Finish())));
            }

            if (owner == null && effect != null)
            {
                effect.SetPosition(transform.position);
            }
        }
    }

    private void Play2DSound()
    {
        if (!string.IsNullOrEmpty(sound2D))
        {
            GameCommon.PlaySound(sound2D, GameCommon.GetMainCamera().transform.position);
        }
    }
}