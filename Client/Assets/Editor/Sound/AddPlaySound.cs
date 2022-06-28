using UnityEditor;
using UnityEngine;
using System.Collections;
using DataTable;

public class AddPlaySound : MonoBehaviour
{    
    [MenuItem("Edit/AddPlaySound")]
    public static void AddPlaySoundScript()
	{
        NiceTable soundTable = GameCommon.LoadTable("Config/EffectSound.csv", LOAD_MODE.UNICODE);
        if (soundTable == null)
            return;

        Object[] objs = Selection.GetFiltered(typeof(object),SelectionMode.DeepAssets);
        foreach (Object o in objs)
        {
            GameObject obj = o as GameObject;
            if (obj != null)
            {
                if (soundTable.GetRecord(obj.name) != null)
                {
                    CPlaySound playSound = obj.GetComponent<CPlaySound>();
                    if (playSound == null)
                    {
                        obj.AddComponent<CPlaySound>();
                    }
                }
            }
        }
	}

}
