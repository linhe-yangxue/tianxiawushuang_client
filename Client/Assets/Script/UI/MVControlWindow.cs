using UnityEngine;
using System.Collections;
using Logic;
using System;

//by chenliang
//begin

public class MVControlParam
{
    private string mAtlasName = "";
    private string mSpriteName = "";
    private string[] mPath;

    public string AtlasName
    {
        set { mAtlasName = value; }
        get { return mAtlasName; }
    }
    public string SpriteName
    {
        set { mSpriteName = value; }
        get { return mSpriteName; }
    }
    public string[] Path
    {
        set { mPath = value; }
        get { return mPath; }
    }
}

//end

[RequireComponent(typeof(MMT.MobileMovieManager))]
public class MVControlWindow : tWindow {

    public GameObject moviePlane; // 作为播放 game_mv.ogv 的幕布
    private string nextWindow; // 视频结束后打开的下一个窗口
    private GameObject mGOSkipTip;

    private MMT.MobileMovieTexture m_movieTexture;
    private Plane tmpPlane;
    private GameObject AV;
    private GameObject cameraObj;
    private Camera camera;
    private AudioSource audio;
    private AudioSource otherAudio;

    private float distance;
    private int clickTimes = 0;
    private Vector3 mInitScale = Vector3.zero;

    private static bool mIsClick = false;
    public static bool IsClick
    {
        set { mIsClick = value; }
        get { return mIsClick; }
    }

    private Action mCompleteCallback;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_mv_skip_button", new DefineFactory<Button_MV_Skip>());
    }
    protected override void OpenInit()
    {
        base.OpenInit();

        AV = GameObject.Find("Audio_Vedio_Player");
        cameraObj = GameCommon.FindObject(AV, "camera");
        //by chenliang
        //beign

        GameObject.DontDestroyOnLoad(AV);

        if (cameraObj != null)
            cameraObj.SetActive(true);

        //end
        camera = cameraObj.GetComponent<Camera>();
        moviePlane = GameCommon.FindObject(cameraObj, "mv_plane");
        audio = GameCommon.FindComponent<AudioSource>(cameraObj);
        
        m_movieTexture = cameraObj.GetComponent<MMT.MobileMovieTexture>();
        m_movieTexture.PlayAutomatically = false;
        m_movieTexture.LoopCount = 0;
        m_movieTexture.onFinished -= OnFinished;
        m_movieTexture.onFinished += OnFinished;

        mInitScale = moviePlane.transform.localScale;
    }

    public override void Open(object param)
    {
        base.Open(param);
        
        //by chenliang
        //beign

//        string[] path = (string[])param;
//---------------
        string[] path = new string[] { "", "" };
        if (param is MVControlParam)
            path = (param as MVControlParam).Path;

        //end
        m_movieTexture.Path = path[0]; // 视频路径
        AudioClip clip = GameCommon.LoadAudioClip(path[1]); // 音频路径
        audio.clip = clip;
        
        Ray ray1 = camera.ViewportPointToRay(new Vector3(0, 0, 0));
        Ray ray2 = camera.ViewportPointToRay(new Vector3(1.0f, 0, 0));

        tmpPlane = new Plane(moviePlane.transform.up, moviePlane.transform.position);

        tmpPlane.Raycast(ray1, out distance);
        Vector3 p1 = ray1.GetPoint(distance);

        tmpPlane.Raycast(ray2, out distance);
        Vector3 p2 = ray2.GetPoint(distance);

        moviePlane.transform.localScale = mInitScale * (Vector3.Distance(p1, p2) / 10.0f);

        //by chenliang
        //begin

//        mGOSkipTip = GameCommon.FindObject(mGameObjUI, "Label");
//----------------
        mGOSkipTip = GameCommon.FindObject(mGameObjUI, "skip_tip");
        MVControlParam tmpParam = param as MVControlParam;
        if (tmpParam != null)
            GameCommon.SetUISprite(mGOSkipTip, "bg", tmpParam.AtlasName, tmpParam.SpriteName);

        //end
        mGOSkipTip.transform.localPosition = new Vector3(0, -5000, 0);
        otherAudio = GameCommon.GetMainCamera().transform.GetComponent<AudioSource>();
        otherAudio.Stop();
        cameraObj.SetActive(true);
        if (m_movieTexture != null)
        {
            m_movieTexture.Play(); // 播放mv
        }
        if (audio != null)
            audio.Play(); // 同步播放音频

        mCompleteCallback = null;
	}

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "FINISH":
                {
                    OnFinished(m_movieTexture);
                    mGOSkipTip.transform.localPosition = new Vector3(0, -5000f, 0);
                }
                break;
            case "LABEL":
                {
                    //by chenliang
                    //begin

//                    mGOSkipTip.transform.localPosition = new Vector3(0, 25.0f, 0);
//----------------------------
                    mGOSkipTip.transform.localPosition = new Vector3(-110.0f, -50.0f, 0);

                    //end
                }
                break;
            case "COMPLETE_CALLBACK":
                {
                    mCompleteCallback = (Action)objVal;
                }
                break;
        }
    }

    void OnFinished(MMT.MobileMovieTexture sender)
    {
        m_movieTexture.Stop();
        moviePlane.transform.localScale = mInitScale;

        cameraObj.SetActive(false);
        //by chenliang
        //begin

//         if (m_movieTexture.Path != "op_2.ogv")
//         {
//             GameObject.DontDestroyOnLoad(AV);
//             //m_movieTexture = null;
//         }
//         else
//         {
//             GameObject.Destroy(AV);
//             m_movieTexture = null;
//         }
//-----------------
        //去除多余逻辑

        //end

        if (mCompleteCallback != null)
        {
            mCompleteCallback();
            mCompleteCallback = null;
        }
        MVControlWindow.IsClick = false;
        otherAudio.Play();
        DataCenter.CloseWindow("MV_CONTROL_WINDOW");
    }
    //by chenliang
    //begin

    public static void EnableAudioVedioPlayer(bool isEnable)
    {
        GameObject tmpGOAVP = GameObject.Find("Audio_Vedio_Player");
        if (tmpGOAVP == null)
            return;

        foreach (Transform tmpTrans in tmpGOAVP.transform)
            tmpTrans.gameObject.SetActive(isEnable);
    }

    //end
}

public class Button_MV_Skip : CEvent
{
    public override bool _DoEvent()
    {
        if (!MVControlWindow.IsClick)
        {
            MVControlWindow.IsClick = true;
            DataCenter.SetData("MV_CONTROL_WINDOW", "LABEL", null);
            return true;
        }
        if (MVControlWindow.IsClick)
        {
            DataCenter.SetData("MV_CONTROL_WINDOW", "FINISH", null);
            MVControlWindow.IsClick = false;
            return true;
        }
        return false;
    }
}
