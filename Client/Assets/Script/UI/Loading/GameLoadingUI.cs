using UnityEngine;
using System;
using System.Collections;

public enum RUN_STATE
{
	RUNNING,
	PAUSE,
	PAUSEED,
	FINISH,
}

public class GameLoadingUI : MonoBehaviour {

	public float mfValue = 1;
	public float mfDValue = -0.02f;

	public string mStrRegisterName = "";

	public int mStartPauseValue = 100;



	public RUN_STATE mRunState = RUN_STATE.RUNNING;

    private static bool mLockLoading = false;
    private static Action mOnUnlock = null;

    public static bool lockLoading 
    {
        get { return mLockLoading; }
        
        set 
        {
            if (!value && mOnUnlock != null)
            {
                mOnUnlock();
                mOnUnlock = null;
            }

            mLockLoading = value;
        }
    }

	// Use this for initialization
	void Start () 
	{
        if (CommonParam.ShowLoadingTime)
        {
            DEBUG.LogError(mStrRegisterName + " loading start Time.time = " + Time.time);
        }
		mStartPauseValue = UnityEngine.Random.Range(80, 95);
	}
	
	// Update is called once per frame
	public virtual void Update () {
		if(mRunState != RUN_STATE.PAUSE && mRunState != RUN_STATE.FINISH)
		{
			ChangeLoadingPercent(mfDValue);
		}

	}

	public virtual void Init()
	{
		mRunState = RUN_STATE.RUNNING;

		InitData();

		DataCenter.SetData(mStrRegisterName, "DVALUE", mfDValue);
		DataCenter.SetData(mStrRegisterName, "PERCENTAGE", mfValue);


		LoadScene();
	}


	public virtual void InitBackground()
	{
//		string fileName = "";
//		string strFolderName = "Resources/textures/UItextures/Loading";
//		#if UNITY_IPHONE			
//		string fileNameBase = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
//		fileName += fileNameBase.Substring(0, fileNameBase.LastIndexOf('/')) + "/Documents/" + strFolderName;
//		#elif UNITY_ANDROID
//		fileName += Application.persistentDataPath + "/" + strFolderName ;
//		#else
//		fileName += Application.dataPath + "/" + strFolderName;
//		#endif
//
//		string[] strFileNames = System.IO.Directory.GetFiles(fileName, "*.jpg");
//		DEBUG.LogError("ALL = " + strFileNames.Length);
//		int iIndex = Random.Range(0, strFileNames.Length);

		int iMaxNum = DataCenter.mGlobalConfig.GetData ("LOADING_BACKGROUND_NUMBER", "VALUE");
        int iIndex = UnityEngine.Random.Range(0, iMaxNum);
		UITexture texture = transform.Find("group").GetComponent<UITexture>();
		if(texture != null)
		{
            string strPicName = "StaticResources/textures/UItextures/Loading/" + "battle_loading_" + iIndex.ToString();
            texture.mainTexture = Resources.Load(strPicName, typeof(Texture)) as Texture;//GameCommon.LoadTexture(strPicName, LOAD_MODE.RESOURCE);//
			//texture.MakePixelPerfect();

			if(texture.mainTexture == null)
			{
				//InitBackground();
				DEBUG.LogError("texture.mainTexture == null");
			}
		}
	}

	public void ChangeLoadingPercent(float fDPercent)
	{
		float fPercent = GetPercent();
//		DEBUG.LogError("..................fPercent = " + fPercent.ToString());
		if (fPercent != -1)
		{
			fPercent += fDPercent;
			if(fPercent < 0)
			{
				fPercent = 0;
				mRunState = RUN_STATE.FINISH;
			}
			else if(fPercent > 1)
			{
				fPercent = 1;
				mRunState = RUN_STATE.FINISH;
			}

			if(mRunState == RUN_STATE.RUNNING)
			{
				if((fDPercent > 0 && (fPercent * 100) > mStartPauseValue) || (fDPercent < 0 && (100 - fPercent * 100) > mStartPauseValue))
				{
					mRunState = RUN_STATE.PAUSE;
				}
			}

			DataCenter.SetData(mStrRegisterName, "PERCENTAGE", fPercent);

//			DEBUG.LogError("mRunState = " + mRunState.ToString());
			if(RUN_STATE.PAUSE == mRunState)
				DoPause();

            if (RUN_STATE.FINISH == mRunState)
            {
                if (mLockLoading)
                {
                    mOnUnlock = DoFinish;
                }
                else
                {
                    DoFinish();
                }
            }
		}
		else
		{
			DEBUG.Log("fPercent is -1");
		}
	}

	public void SetLoadingPercentage(float fPercent)
	{
		DataCenter.SetData(mStrRegisterName, "PERCENTAGE", 1 - fPercent);
	}

	public virtual void InitData()
	{
		InitBackground();
	}

	public virtual void DoPause()
	{
        if (CommonParam.ShowLoadingTime)
        {
            DEBUG.LogError(mStrRegisterName + " loading pause Time.time = " + Time.time);
        }
		//mRunState = RUN_STATE.PAUSEED;
	}

	public virtual void DoFinish()
	{
        if (CommonParam.ShowLoadingTime)
        {
            DEBUG.LogError(mStrRegisterName + " loading end Time.time = " + Time.time);
        }

		DataCenter.CloseWindow(mStrRegisterName);
		mRunState = RUN_STATE.RUNNING;
	}
	
	public tLogicData GetLoadingData()
	{
		return DataCenter.Self.getData(mStrRegisterName);
	}

//	public void SetPercent(float fPercent)
//	{
//		LoadingWindow logic = GetLoadingData() as LoadingWindow;
//		if(logic != null)
//			logic.set("PERCENTAGE", fPercent);
//	}

	public float GetPercent()
	{
		LoadingWindow logic = GetLoadingData() as LoadingWindow;
		if(logic != null && logic.mGameObjUI != null)
		{
			UIProgressBar loadingBar = logic.mGameObjUI.transform.Find("group/loading_bar").GetComponent<UIProgressBar>();
			return loadingBar.value;
		}
		return -1;
	}











	public void SetLoadingPercentage(int iPercent)
	{
		DataCenter.SetData(mStrRegisterName, "PERCENTAGE", 1 - iPercent * 0.01f);
	}

	public virtual void LoadScene()
	{
		
	}

	public virtual IEnumerator StartLoading(string strSceneName) {
		int displayProgress = 0;
		int toProgress = 0;
		
		AsyncOperation op  = Application.LoadLevelAsync(strSceneName);		
		
		op.allowSceneActivation = false;
		if(op.progress > 0)
		{
			while(op.progress < 0.9f)
			{
				toProgress = (int)(op.progress * 100);
				while(displayProgress < toProgress)
				{
					++displayProgress;
					SetLoadingPercentage(displayProgress);
					yield return new WaitForEndOfFrame();
				}
			}
			
			toProgress = 100;
			while(displayProgress < toProgress)
			{
				++displayProgress;
				SetLoadingPercentage(displayProgress);
				if(displayProgress + 10 == toProgress)
				{				
					op.allowSceneActivation = true;
				}
				
				if(displayProgress + 3 == toProgress)
				{
					DoFinish();
				}
				
				yield return new WaitForEndOfFrame();
			}

		}
		else if(op.progress <= 0)
		{
			toProgress = 100;
			while(displayProgress < toProgress)
			{
				++displayProgress;
				SetLoadingPercentage(displayProgress);
				if(displayProgress + 10 == toProgress)
				{				
					op.allowSceneActivation = true;
				}
				
				if(displayProgress + 3 == toProgress)
				{
					DoFinish();
				}
				
				yield return new WaitForEndOfFrame();
			}
//			op.allowSceneActivation = true;
//			DoFinish();
		}

	}
	

}

public class LoadingWindow : tWindow
{
	public float mfDValue = 0;

	public override void Open (object param)
	{
		base.Open (param);
	}
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		if("DVALUE" == keyIndex)
		{
			mfDValue = (float)objVal;
		}
		if("PERCENTAGE" == keyIndex)
		{
			SetPercent((float)objVal);
		}
		if("CONTINUE" == keyIndex)
		{
			LoadingContinue();
		}
	}

	public void SetPercent(float percentage)
	{
		if(mGameObjUI == null)
			return;

		UIProgressBar loadingBar = mGameObjUI.transform.Find("group/loading_bar").GetComponent<UIProgressBar>();
		UILabel percentUI = loadingBar.transform.Find("percent_label").GetComponent<UILabel>();
		loadingBar.value = percentage;
		
		int iPercentage = 0;
		if(mfDValue > 0)
			iPercentage = (int)(percentage*100);
		else if(mfDValue < 0)
			iPercentage = (int)(100 - percentage*100);
		
		percentUI.text = iPercentage.ToString () + "%";
		
		
		Transform movingRoleTran = loadingBar.transform.Find("moving_role");
		if(movingRoleTran != null)
		{
			float width = loadingBar.transform.Find("background").GetComponent<UISprite>().width;
			movingRoleTran.localPosition = new Vector3(iPercentage / 100.0f * width, movingRoleTran.localPosition.y, movingRoleTran.localPosition.z);
		}
	}

	public override void OnOpen ()
	{
		base.OnOpen ();

		if(mGameObjUI != null)
		{
//			GameLoadingUI uiObj = mGameObjUI.GetComponent<GameLoadingUI>();
//			if(uiObj != null)
//			{
//				uiObj.mIsFinish = false;
//			}

			Reset();
		}
	}

	public void Reset()
	{
		GameLoadingUI ui = mGameObjUI.GetComponent<GameLoadingUI>();
		ui.Init();
	}

	public override void Close ()
	{
		base.Close ();
		GameObject.Destroy(mGameObjUI);
	}

	public void LoadingContinue()
	{
		if(mGameObjUI != null)
		{
			GameLoadingUI ui = mGameObjUI.GetComponent<GameLoadingUI>();
			ui.mRunState = RUN_STATE.PAUSEED;
		}
	}
}