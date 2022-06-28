using UnityEngine;
using System.Collections;
using DataTable;
using VersionControl;
using RManager.AssetLoader;
using Common;
using Asset;

class VerCtlContext : DefaultVerCtlCtx
{
    MonoBehaviour mMonoBehaviour;

    public VerCtlContext(MonoBehaviour mb)
    {
        mMonoBehaviour = mb;
    }


    public override Coroutine StartCoroutine(IEnumerator coroutine)
    {
        return mMonoBehaviour.StartCoroutine(coroutine);
    }
}

public class HotUpdateLoadingUI : MonoBehaviour, IU3dCroutineHelper, IHotUpdateUI
{
    public static HotUpdateLoadingUI Self = null;
    public float mfProgress = 0;
    private int mDisplayProgress = 0;

    private float mStartTime = 0;
    private float mDuringTime = 20.0f;

    public float mfValue = 1.0f;
    public float mfDValue = -0.02f;

    private int mMaxValue = 100;//500;

    private bool mbIsStart = false;

    private bool IsInCoreMode = false;
    private bool IsLoadResFromServer = false;

    private HotUpdate mHotUpdate;

    // Use this for initialization
    void Start()
    {
        Self = this;
        Init();

        IsInCoreMode = DataCenter.mStaticConfig.GetData("IS_IN_CORE_MODE", "VALUE") == "YES";
        IsLoadResFromServer = DataCenter.mStaticConfig.GetData("IS_LOAD_RES_FROM_SERVER", "VALUE") == "YES";
        if (IsInCoreMode || IsLoadResFromServer)
        {
            //进行热更新
            //by chenliang
            //begin

//            GlobalModule.isHotLoading = true;

            //end
            StartCoroutine(StartUpdateAndLoadResource());
        }
        else
            Self.End();

        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator StartUpdateAndLoadResource()
    {
        Self.Start(0.0f);
        if (IsLoadResFromServer)
        {
            mHotUpdate = new HotUpdate();
//            mHotUpdate.CoroutineHelper = this;
            mHotUpdate.View = this;
            DataRecord tmpRecord = DataCenter.mStaticConfig.GetRecord("HOT_UPDATE_IP");
            string tmpHotUpdateIP = tmpRecord.getObject("VALUE").ToString();
            tmpRecord = DataCenter.mStaticConfig.GetRecord("HOT_UPDATE_PORT");
            string tmpHotUpdatePort = tmpRecord.getObject("VALUE").ToString();
            yield return StartCoroutine(mHotUpdate.Check(tmpHotUpdateIP, tmpHotUpdatePort));
        }

        if (!IsInCoreMode)
        {
            yield return null;
            Self.End();
            yield break;
        }

        Self.End();
    }

    public virtual void Init()
    {
        AssetManager.Init(this);
        BundleAssetManager.AssetsRoot = Helper.CombinePath(BundleAssetManager.AssetsRoot, Application.streamingAssetsPath);
        GameCommon.mResources.IsLoadFromBundle = false;
        GameCommon.mResources.IsShowLog = true;

        // screen resolution        
        GlobalModule.SetResolution(GameCommon.FindUI("Camera").GetComponent<Camera>());

        GlobalModule.InitGameDataWithoutTable();
        Net.Init();
        DataCenter.Self.LoadConfigDataFromFile();
        InitData();
    }

    public void InitData()
    {
        mfValue = 1.0f;
        mfDValue = -0.03f;
        mStartTime = Time.time;
        mfProgress = 0;
        transform.Find("background").GetComponent<UIWidget>().alpha = 0;
        SetPercent(mfValue);
    }

    public virtual IEnumerator StartLoading()
    {
        Reset();
        int toProgress = 0;

        if (mfProgress >= 0)
        {
            while (mfProgress <= 1.0f)
            {
                toProgress = (int)(mfProgress * mMaxValue);
                while (mDisplayProgress < toProgress)
                {
                    ++mDisplayProgress;
                    UpdateUI(mDisplayProgress * 1.0f / mMaxValue);
                    yield return new WaitForEndOfFrame();
                }

                if (Time.time - mStartTime >= mDuringTime)
                {
                    FadeOut();
                    mStartTime = Time.time;
                }

                yield return null;
            }
        }
    }

    public void UpdateUI(float fPercent)
    {
        SetPercent(1 - fPercent);
    }

    public void SetPercent(float percentage)
    {
        UIProgressBar loadingBar = transform.Find("group/loading_bar").GetComponent<UIProgressBar>();
        UILabel percentUI = loadingBar.transform.Find("percent_label").GetComponent<UILabel>();
        loadingBar.value = percentage;

        int iPercentage = 0;
        if (mfDValue > 0)
            iPercentage = (int)(percentage * 100);
        else if (mfDValue < 0)
            iPercentage = (int)(100 - percentage * 100);

        percentUI.text = iPercentage.ToString() + "%";


        Transform movingRoleTran = loadingBar.transform.Find("moving_role");
        if (movingRoleTran != null)
        {
            float width = loadingBar.transform.Find("background").GetComponent<UISprite>().width;
            movingRoleTran.localPosition = new Vector3(iPercentage / 100.0f * width, movingRoleTran.localPosition.y, movingRoleTran.localPosition.z);
        }
    }

    public void UpdateDescriptionUI()
    {
        int iMaxNum = DataCenter.mTips.GetAllRecord().Count;
        int iIndex = Random.Range(0, iMaxNum) + 1000;
        DataRecord re = DataCenter.mTips.GetRecord(iIndex);
        if (re != null)
        {
            // set background
            UITexture texture = transform.Find("background").GetComponent<UITexture>();
            if (texture != null)
            {
                string strPicName = (string)(re["TIP_BACKGROUND_PATH"]) + (string)(re["TIP_BACKGROUND_NAME"]);
                texture.mainTexture = GameCommon.LoadTexture(strPicName, LOAD_MODE.RESOURCE);
                if (texture.mainTexture == null)
                {
                    //InitBackground();
                    DEBUG.LogError("texture.mainTexture == null");
                }
            }
        }
    }

    private void FadeIn()
    {
        TM_FadeIn evt = Logic.EventCenter.Self.StartEvent("TM_FadeIn") as TM_FadeIn;
        evt.mTarget = transform.Find("background").gameObject;
        evt.mBefore = 0.2f;
        evt.mDuration = 0.3f;
        evt.DoEvent();
    }

    private void FadeOut()
    {
        TM_FadeOut evt = Logic.EventCenter.Self.StartEvent("TM_FadeOut") as TM_FadeOut;
        evt.mTarget = transform.Find("background").gameObject;
        evt.mBefore = 0.2f;
        evt.mDuration = 0.3f;
        evt.onElapsed += x =>
        {
            UpdateDescriptionUI();
            FadeIn();
        };
        evt.DoEvent();
    }

    private void FixedUpdate()
    {
        Net.gNetEventCenter.Process(Time.fixedDeltaTime);
        Logic.EventCenter.Self.Process(Time.fixedDeltaTime);
        ObjectManager.Self.Update(Time.fixedDeltaTime);
    }

    public void Reset()
    {
        mDisplayProgress = 0;
    }

    //------------------------------------------------------------------------------------------
    // supply interface
    public void Start(float fProgress)
    {
        mbIsStart = true;
        UpdateDescriptionUI();
        FadeIn();

        StartCoroutine(StartLoading());

        SetProgress(fProgress);
    }

    public string testScene = "";
    public bool loadAll = true;
    public bool loadTexture = false;
    public bool loadModel = false;
    public bool loadController = false;
    public bool loadPrefab = false;
    public bool loadSound = false;
    public bool loadShader = false;

    IEnumerator LoadGameResourceAsync()
    {
        // load all bundels
        GameCommon.mResources.ClearCache();

        if (!string.IsNullOrEmpty(testScene))
        {
            DEBUG.Log("Loading test scene...");
            GameCommon.LoadLevel(testScene);
            yield break;
        }
    }

    public void End()
    {
        DEBUG.Log("Loading game start scene...");
        GameCommon.LoadLevel("StartGame");
    }

    public void SetLoadingName(string strLoadingName)
    {
        // set title
        UILabel titleLabel = GameCommon.FindComponent<UILabel>(gameObject, "tip_title_label");
        if (titleLabel != null)
            titleLabel.text = strLoadingName;
    }

    public void SetLoadingDescription(string strDescriptionLabel)
    {
        // set description
        UILabel descriptionLabel = GameCommon.FindComponent<UILabel>(gameObject, "tip_description_label");
        if (descriptionLabel != null)
            descriptionLabel.text = strDescriptionLabel;
    }

    public void SetProgress(float fProgress)
    {
        if (fProgress < mfProgress)
            Reset();

        mfProgress = fProgress;
        if (mfProgress >= 1.0f)
            mfProgress = 1.0f;
        else if (mfProgress <= 0.0f)
            mfProgress = 0;

        if (mfProgress <= 0.0f)
            Reset();
        //mStartTime = Time.time;
    }
}
