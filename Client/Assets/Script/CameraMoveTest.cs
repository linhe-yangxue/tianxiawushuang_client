using UnityEngine;
using System.Collections;


public class CameraMoveTest : MonoBehaviour
{
    public Vector3 cameraOffset = new Vector3(0f, 11f, 12.75f); // 摄像机偏移值
    public Vector3 cameraAngles = new Vector3(40f, 180f, 0f);   // 摄像机倾角，一般只需要调x坐标
    public float maxAccelerate = 20f;   // 最大加速度，主要用于控制摄像机的加速
    public float damping = 10f;          // 平滑阻尼系数，主要用于控制摄像机的减速
    public float forwardOffset = 2.5f;  // 前向偏移，摄像机注视的点在目标对象正前方多少米
    

    private void Update()
    {
        if (MainProcess.mMainCamera != null && MainProcess.mStage != null && !MainProcess.mStage.mbBattleFinish)
        {
            MainProcess.mMainCamera.transform.localPosition = cameraOffset;
            MainProcess.mMainCamera.transform.localRotation = Quaternion.Euler(cameraAngles);

            CameraMoveEvent.mCameraOffset = cameraOffset;
            CameraMoveEvent.mCameraAngles = cameraAngles;
            CameraMoveEvent.mDamping = damping;
            CameraMoveEvent.mMaxAccelerate = maxAccelerate;
            CameraMoveEvent.mForwardOffset = forwardOffset;
        }
    }
}