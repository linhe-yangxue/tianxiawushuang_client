using UnityEngine;
using System.Collections;

public class HpBar : ProgressBar
{
    public static readonly float mSpeed = 2f;

	static Material msShowHpMat;
	static Material msBackHpMat;
	
	static Material msShowGreen;
	static Material msGreenBack;    
	
	float mMaxValue = 1;
	ValueAnimation mChangeValue = new ValueAnimation();

	static BloodMarkSprite msBloodMark;
	static BloodMarkSprite msBossBloodMark;

	static public void Init()
	{
		msShowHpMat = new Material(GameCommon.FindShader("VertexLit"));
		msShowHpMat.color = Color.white;
		msShowHpMat.mainTexture = GameCommon.LoadTexture("textures/bar_red", LOAD_MODE.RESOURCE);
		msShowHpMat.SetColor("_Emission", Color.white);
		
		msShowGreen = new Material(GameCommon.FindShader("VertexLit"));
		msShowGreen.color = Color.white;
		msShowGreen.mainTexture = GameCommon.LoadTexture("textures/bar_green", LOAD_MODE.RESOURCE);
		msShowGreen.SetColor("_Emission", Color.white);
		
		msBackHpMat = new Material(GameCommon.FindShader("Transparent/VertexLit"));
		Color backColor = Color.black;
		backColor.a = 200.0f/255;
		msBackHpMat.color = backColor;
		msBackHpMat.mainTexture = GameCommon.LoadTexture("textures/bar_back", LOAD_MODE.RESOURCE);
		msBackHpMat.SetColor("_Emission", backColor);
		
		msGreenBack = new Material(GameCommon.FindShader("Transparent/VertexLit"));
		backColor = Color.black;
		backColor.a = 200.0f/255;
		msGreenBack.color = backColor;
		msGreenBack.mainTexture = GameCommon.LoadTexture("textures/bar_back", LOAD_MODE.RESOURCE);
		msGreenBack.SetColor("_Emission", backColor);

		mWorldCenter = GameCommon.GetMainCameraObj();
		if(mWorldCenter == null)
			mWorldCenter = new GameObject("world_center");
		GameObject obj = GameCommon.LoadAndIntanciatePrefabs("Prefabs/Camera_Z", mWorldCenter);
		string name = obj.name.Replace("(Clone)", "");
		obj.name = name;
        //by chenliang
        //begin

        //--------------
        //增加自动调整摄像机视口
        GameObject tmpWCCamera = GameCommon.FindObject(obj, "Camera");
        if (tmpWCCamera != null && tmpWCCamera.GetComponent<AutoCameraResolution>() == null)
            tmpWCCamera.AddComponent<AutoCameraResolution>();

        //end

        MainProcess.mMainCamera = GameCommon.GetMainCamera();
        GlobalModule.SetResolution(MainProcess.mMainCamera);

		GameCommon.SetMainCameraEnable(false);

		MainProcess script = obj.GetComponent<MainProcess>();
		if(script != null)
			script.enabled = false;

        GameObject.DontDestroyOnLoad(mWorldCenter);

	}
	
	public override  void Init(float width, float height, Color showColor, Color backColor)
	{	
		mWidth = BaseObject.msHpBarWidth;
		mHeight = BaseObject.msHpBarHeight;       

		mShow.SetSize(width, height);
		mBack.SetSize(width + 0.04f, height + 0.04f);
		
		mBack.mSprite.transform.localPosition = new Vector3(-mWidth*0.5f, 0, 0);
		mShow.mMainTransform.localPosition = new Vector3(-mWidth*0.5f + 0.02f, 0 + 0.02f,  msBackOff);

		mShow.mSprite.layer = CommonParam.PlayerLayer;
		mBack.mSprite.layer = CommonParam.PlayerLayer;

		if (showColor == Color.red)
		{
			mShow.SetMaterail(msShowHpMat);
			mBack.SetMaterail(msBackHpMat);
		}
		else
		{
			mShow.SetMaterail(msShowGreen);
			mBack.SetMaterail(msGreenBack);
		}
		
	}

	public override void InitBloodMark(int elementIndex)
	{
		if(elementIndex > 5) 
			return;

		string strBloodMarkName = "ui_bloodmark_blue";
		if(elementIndex == (int)ELEMENT_TYPE.RED) strBloodMarkName = "ui_bloodmark_red";
		else if(elementIndex == (int)ELEMENT_TYPE.BLUE) strBloodMarkName = "ui_bloodmark_blue";
		else if(elementIndex == (int)ELEMENT_TYPE.GREEN) strBloodMarkName = "ui_bloodmark_green";
		else if(elementIndex == (int)ELEMENT_TYPE.GOLD) strBloodMarkName = "ui_bloodmark_gold";
		else if(elementIndex == (int)ELEMENT_TYPE.SHADOW) strBloodMarkName = "ui_bloodmark_shadow";
		else  strBloodMarkName = "ui_bloodmark_no";

		msBloodMark = new BloodMarkSprite(true);
		msBloodMark.Init (mShow.mMain, 1.0f/64, -0.1f);
		msBloodMark.SetAtlasTexture("Battle Atlas", strBloodMarkName);
        //added by xuke
        MeshRenderer r = msBloodMark.mNiceSprite.mSprite.GetComponent<MeshRenderer>();
        GameCommon.SetMat_ETC1(r.material);
        //end
	}

	public override void InitBossBloodMark()
	{
		msBossBloodMark = new BloodMarkSprite(true);
		msBossBloodMark.Init (mShow.mMain, 1.0f/64, -0.11f);
		msBossBloodMark.SetAtlasTexture("Battle Atlas", "ui_bloodmark_boss");
        //added by xuke
        MeshRenderer r = msBossBloodMark.mNiceSprite.mSprite.GetComponent<MeshRenderer>();
        GameCommon.SetMat_ETC1(r.material);
        //end
	}

	public override void SetValue(float nowValue, float maxValue)
	{
        float current = Mathf.Clamp(mChangeValue.NowValue(), 0f, maxValue);
        nowValue = Mathf.Clamp(nowValue, 0f, maxValue);
        float delta = Mathf.Abs(current - nowValue);
        float deltaRate = delta / maxValue;
        float duration = deltaRate / mSpeed;    
        mMaxValue = maxValue;
        mChangeValue.mFinish = true;

        if (duration > 0.001f)
        {
            mChangeValue.Start(current, nowValue, duration);
        }
        else 
        {
            base.SetValue(nowValue, maxValue);
        }	
	}

	public override void Update(float dt)
	{
        if (!mChangeValue.mFinish)
        {
            float nowV;
            mChangeValue.Update(dt, out nowV);
            base.SetValue(nowV, mMaxValue);
        }
	}
	
	public override void InitValue(float n, float max)
	{
		mChangeValue.Start(n, max, 0.1f);
		mMaxValue = max;
		SetValue(n, max); 
	}

//	public override void Destroy()
//	{
//		if(msBloodMark != null && msBloodMark.mNiceSprite != null)
//			msBloodMark.mNiceSprite.Destroy ();
//		if(msBossBloodMark != null && msBossBloodMark.mNiceSprite != null)
//			msBossBloodMark.mNiceSprite.Destroy ();
//
//		base.Destroy ();
//	}

//	public override void SetVisible(bool bShow)
//	{
//		if(msBloodMark != null && msBloodMark.mNiceSprite != null && msBloodMark.mNiceSprite.mMain != null)
//		{
//			msBloodMark.mNiceSprite.SetVisible (bShow);
//			msBloodMark.mNiceSprite.mMain.SetActive (bShow);
//		}
//		if(msBossBloodMark != null && msBossBloodMark.mNiceSprite != null && msBossBloodMark.mNiceSprite.mMain != null)
//		{
//			msBossBloodMark.mNiceSprite.SetVisible (bShow);
//			msBossBloodMark.mNiceSprite.mMain.SetActive (bShow);
//		}
//
//		base.SetVisible (bShow);
//	}
}
//-----------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------