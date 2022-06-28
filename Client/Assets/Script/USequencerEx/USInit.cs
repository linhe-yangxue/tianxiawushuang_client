using UnityEngine;
using System.Collections;


public class USProcessor : MonoBehaviour
{
    private void Start()
    {
        if (GlobalModule.mInstance != null)
            return;

        GlobalModule.InitGameData();
        Net.Init();
        DataCenter.Self.InitResetGameData();
    }

    private void FixedUpdate()
    {
        Net.gNetEventCenter.Process(Time.fixedDeltaTime);
        Logic.EventCenter.Self.Process(Time.fixedDeltaTime);
        ObjectManager.Self.Update(Time.fixedDeltaTime);
    }
}