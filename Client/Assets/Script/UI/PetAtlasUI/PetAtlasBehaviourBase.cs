using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PetAtlasBehaviourBase : MonoBehaviour
{
    [HideInInspector]
    public List<string> RegisDataName = new List<string>();
    public Action<string> RegisDelegate = null;


    public virtual void OnDestroy()
    {
        if (RegisDataName.Count != 0)
        {
            for (int i = 0; i < RegisDataName.Count; i++)
            {
                if (RegisDelegate != null)
                    RegisDelegate(RegisDataName[i]);
                else
                    DataCenter.Remove(RegisDataName[i]);
            }
        }
    }

}
