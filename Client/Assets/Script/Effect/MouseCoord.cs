using UnityEngine;
using System.Collections;

public class MouseCoord : BaseEffect
{
    static float msWaveTime = 0.5f;
    float mWaveHeight = 0.3f;
    float mMinSize = 0.3f;
    WaveAnimation mWaveValue;

    NiceSprite  mSprite;

    bool mbShow = false;

    // Use this for initialization
    public override bool  InitCreate(BaseObject owner, BaseObject target, int configIndex)
    {
 	    //base.InitCreate(owner, target, configIndex);
        mSprite = NiceSprite.Create("mouse_coord", null, true);

        mWaveValue = new WaveAnimation();
        mWaveValue.Start(msWaveTime * 0.5f, 100000000, msWaveTime, mWaveHeight);

        Material mat = new Material(GameCommon.FindShader("Transparent/VertexLit"));
		mat.color = Color.white;
		mat.mainTexture = GameCommon.LoadTexture("textures/MouseCoord", LOAD_MODE.RESOURCE);
		mat.SetColor("_Emission", Color.white);

        mSprite.SetMaterail(mat);
        mSprite.SetSize(1, 1);
        Vector3 pos = mSprite.mSprite.transform.localPosition;
        //pos.y = 0.5f;
        //pos.z = -0.5f;
        mSprite.mSprite.transform.localPosition = pos;
        mSprite.mSprite.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

        SetActive(false);

        return true;
    }

    // Update is called once per frame
    public override bool Update(float dt)
    {
        if (!mbShow)
            return false;

        if (mWaveValue.Update(Time.deltaTime))
            mWaveValue.Start(msWaveTime * 0.5f, 100000000, msWaveTime, mWaveHeight);

        float x = mMinSize + System.Math.Abs(mWaveValue.NowValue());

        mSprite.mMainTransform.localScale = new Vector3(x, x, x);

		return true;
    }

    public override bool _OnFinish()
    {
        if (mSprite != null)
        {
            mSprite.Destroy();
            mSprite = null;
        }
        return base._OnFinish();
    }

    public override void SetPosition(Vector3 pos)
    {
        pos.y += 0.1f;
        mSprite.mMainTransform.transform.position = pos;
    }

    public void SetActive(bool bShow)
    {
        mSprite.SetVisible(bShow);

        mbShow = bShow;

        if (mbShow)
            StartUpdate();
    }
}


public class WarnEffect : Effect
{

    NiceSprite mSprite;

    ValueAnimation mSizeValue = new ValueAnimation();
	TimeAnimation	mTimeValue = new TimeAnimation();

	float mMaxScale = 1.2f;

	bool mbSizeFinish = false;

    // Use this for initialization
    public override bool InitCreate(BaseObject owner, BaseObject target, int configIndex)
    {
        return InitCreate(owner, target, configIndex);
    }

    public override bool InitCreate(BaseObject owner, BaseObject target, int configIndex, bool bIsChild)
    {
        base.InitCreate(owner, target, configIndex, bIsChild);
        mSprite = NiceSprite.Create("warn_flag", null, true);

        Material mat = new Material(GameCommon.FindShader("Transparent/VertexLit"));
        mat.color = Color.white;
        mat.mainTexture = GameCommon.LoadTexture("textures/warn", LOAD_MODE.RESOURCE);
        mat.SetColor("_Emission", Color.white);

        mSprite.SetMaterail(mat);
        mSprite.SetSize(1, 1, true);

        mSprite.mMainTransform.parent = owner.mMainObject.transform;
        owner.mEffect = mSprite.mMain;

        return true;
    }

    // Update is called once per frame
     public override bool Update(float dt)
     {
		if (mSprite.mMainTransform==null)
			return false;

		if (!mbSizeFinish)
		{
			float nowValue;
			bool bEnd = mSizeValue.Update(dt, out nowValue);
			if (bEnd)
			{
				mTimeValue.Start (0.6f);
                mbSizeFinish = true;
            }

			mSprite.mMainTransform.localScale = new Vector3(nowValue, nowValue, nowValue);
        }        
        else
		{
			if (mTimeValue.Update(dt))
			{
            	SetVisible(false);
				return false;
			}
		}

        mSprite.mMainTransform.position = mShowPosition.Update(mOwner, null);

        if (ProgressBar.mWorldCenter != null)
        {
            SetRotation(Quaternion.Euler(CameraMoveEvent.mCameraAngles.x, 0f, 0f) * ProgressBar.mWorldCenter.transform.rotation);
        }

        return true;
     }


    public override bool _OnFinish()
    {
        if (mSprite != null)
        {
            mSprite.Destroy();
            mSprite = null;
        }
        return base._OnFinish();
    }

    public override void SetPosition(Vector3 pos)
    {
        mSprite.mMainTransform.transform.position = pos;
    }

    public void SetRotation(Quaternion rot)
    {
        mSprite.mMainTransform.transform.rotation = rot;
    }

    public void SetVisible(bool bShow)
    {       
        mSprite.SetVisible(bShow);
        if (bShow)
        {
			mbSizeFinish = false;
			mSizeValue.Start(0, mMaxScale, 0.5f);
			StartUpdate();
        }
    }

    public bool IsVisible()
    {
        return mSprite.mSprite.activeSelf;
    }
}
