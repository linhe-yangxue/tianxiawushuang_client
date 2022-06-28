using UnityEngine;
using System.Collections;
using Utilities.Routines;


[ExecuteInEditMode]
public class CGComponent : MonoBehaviour
{
    public GameObject target;

    [HideInInspector]
    public bool demonstrate = false;

    private void Awake()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            if (demonstrate)
            {
                CGTools.InitGameData();           
            }
#endif
            OnInit();
#if UNITY_EDITOR
        }
        else 
        {
            demonstrate = false;
        }
#endif
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (Application.isPlaying && demonstrate)
        {
            PlayDemonstrate();
        }
#endif
    }

    protected virtual void OnInit() { }    
    protected virtual void OnPlay() { }
    protected virtual void OnPlayDemonstrate() { OnPlay(); }
    protected virtual IEnumerator DoPlay() { yield break; }
    protected virtual IEnumerator DoPlayDemonstrate() { return DoPlay(); }

    public IRoutine Play()
    {
        OnPlay();

        if (target != null)
        {
            return target.StartRoutine(DoPlay());
        }

        return null;
    }

    public IRoutine PlayDemonstrate()
    {
        OnPlayDemonstrate();

        if (target != null)
        {
            return target.StartRoutine(DoPlayDemonstrate());
        }

        return null;
    }
}