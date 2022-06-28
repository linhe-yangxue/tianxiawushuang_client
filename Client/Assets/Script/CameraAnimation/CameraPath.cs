using UnityEngine;


public class CameraPath : ObjectPath
{
    protected override void OnPlay()
    {
        target = MainProcess.mMainCamera.gameObject;
    }
}