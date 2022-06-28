using UnityEngine;
using System.Collections;

public class StandardTweenAnim : MonoBehaviour 
{
    void Awake() 
    {
        if (!CommonParam.mOpenDynamicUI)
            return;
        UITweener[] tweens = gameObject.GetComponents<UITweener>();
        GameCommon.InitTweenPrefab(tweens[0],tweens[1],gameObject);

        //DontDestroyOnLoad(this.gameObject);
        transform.parent = GameObject.Find("TweenPrefab_Root").transform;
    }
}
