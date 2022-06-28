using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[System.Serializable]
//public class LoadEntry
//{
//    public LoadEntry(bool isLoad, string strName)
//    {
//        mIsLoad = isLoad;
//        mStrName = strName;
//    }

//    public bool mIsLoad;
//    public string mStrName;
//}

//public enum Script_Name
//{
//    GlobalModule,
//    BaseUI,
//    JpushInit,
//}

public class LoadScripts : MonoBehaviour
{
    //public LoadEntry[] mScripts;

    public string[] mScripts;
	void Awake () {

        //by chenliang
        //begin

//         foreach (string k in mScripts)
//         {
//             if(!string.IsNullOrEmpty(k))
//                 gameObject.AddComponent(k);
//         }
//----------------
        //放在Start中处理

        //end

        //CS_AppLaunch cs=new CS_AppLaunch();
        //HttpModule.Instace.SendGameServerMessageT(cs,NetManager.RequestSuccess,NetManager.RequestFail);

        //gameObject.AddComponent("GlobalModule");
        //gameObject.AddComponent("BaseUI");
        //gameObject.AddComponent("JpushInit");
    }
    //by chenliang
    //begin

    void Start()
    {
//         //BI播放过场动画
//         BIHelper.SendAppLoadStep(LoadStepType.Animation);
// #if UNITY_ANDROID || UNITY_IOS
//         Handheld.PlayFullScreenMovie("game_mv.mp4", Color.black, FullScreenMovieControlMode.CancelOnInput);
// #endif
        JCode.InitLitJson();
        foreach (string k in mScripts)
        {
            if (!string.IsNullOrEmpty(k))
                gameObject.AddComponent(k);
        }

        //增加自动调整摄像机视口
        GameObject tmpGOCamera = GameCommon.FindObject(gameObject, "Camera");
        if (tmpGOCamera != null && tmpGOCamera.GetComponent<AutoCameraResolution>() == null)
            tmpGOCamera.AddComponent<AutoCameraResolution>();
    }

    //end
}
