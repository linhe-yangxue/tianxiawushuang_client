using UnityEngine;
using System.Collections;
using DataTable;


public class USEffect : MonoBehaviour
{
    public Effect effect;
    public float speed;
    public float range = 1f;

    private void Update()
    {
        this.transform.position += this.transform.forward * speed * Time.deltaTime;

        if (effect.mTargetObject != null 
            && AIKit.InBounds(this.transform.position, effect.mTargetObject.mMainObject.transform.position, range))
        {
            effect.Finish();
        }
    }
}