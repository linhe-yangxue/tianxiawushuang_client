using UnityEngine;
using System.Collections;
using Utilities.Routines;


public class ActivateLater : MonoBehaviour
{
    public float delay = 1f;
    public GameObject target = null;
    public bool ignoreTimeScale = true;

    private void OnEnable()
    {
        if (ignoreTimeScale)
            RealTimeDelay.Start(gameObject, delay, Activate);
        else
            Delay.Start(gameObject, delay, Activate);
    }

    private void Activate()
    {
        if (target != null)
            target.SetActive(true);
    }
}