using UnityEngine;
using System.Collections;
using Logic;

public class ObjectEvent : Logic.CEvent
{
    public BaseObject mOwner;

    public void SetOwner(BaseObject obj) { mOwner = obj; }
    public BaseObject GetOwner() { return mOwner; }

    public override bool _DoEvent() { return false; }
}

public class Object_FadeInOrOut : ObjectEvent
{
    protected ValueAnimation mAlpha = new ValueAnimation();

    struct RenderInfo
    {
        public Renderer    mRenderer;
        public Material    mUseMaterial;
        public Material    mNowMaterial;
    }

    RenderInfo[] mRenderList;
    bool mbVisibleOnFinish = true;

    //-------------------------------------------------------------------------
    public virtual GameObject GetMainObject() { return GetOwner().mMainObject; }

    public virtual Renderer[] GetRenderers() { return GetOwner().mBodyObject.GetComponentsInChildren<Renderer>(); }

    public void Start(float useTime, bool bIn)
    {
        mbVisibleOnFinish = bIn;

        if (bIn)
            mAlpha.Start(0, 1, useTime);
        else
            mAlpha.Start(1, 0, useTime);

        if (GetOwner() != null && GetOwner().mMainObject != null)
        {
            Renderer[] render = GetRenderers();
            if (render == null || render.Length <= 0)
            {
                Finish();
                return;
            }
            mRenderList = new RenderInfo[render.Length];
            
            for (int i = 0; i < render.Length; ++i)
            {
                mRenderList[i] = new RenderInfo();

                mRenderList[i].mRenderer = render[i];
                mRenderList[i].mUseMaterial = render[i].material;

				mRenderList[i].mNowMaterial = new Material(render[i].material);
                mRenderList[i].mNowMaterial.shader = GameCommon.FindShader("Easy/EasyTransparent");

                render[i].material = mRenderList[i].mNowMaterial;
            }
        }
        StartUpdate();
    }

    public override bool _DoEvent()
    {
        StartUpdate();

        return true;
    }

    public override bool Update(float dT)
    {
        float nowAlpha;
        bool bFinish = mAlpha.Update(dT, out nowAlpha);
        if (mRenderList != null)
        {
            for (int i = 0; i < mRenderList.Length; ++i)
            {
                Color temp = mRenderList[i].mNowMaterial.color;
                temp.a = nowAlpha;
                mRenderList[i].mNowMaterial.color = temp;
            }
        }
        if (bFinish)
        {
            Finish();
        }
        return !bFinish;
    }

    public override bool _OnFinish()
    {
        if (GetOwner() != null && GetOwner().mMainObject != null)
        {
            if (mRenderList != null)
            {
                for (int i = 0; i < mRenderList.Length; ++i)
                {
					if (mRenderList[i].mRenderer!=null)
                    	mRenderList[i].mRenderer.material = mRenderList[i].mUseMaterial;
                }
                mRenderList = null;
            }
            GetOwner().SetVisible(mbVisibleOnFinish);
        }

        return true;
    }
}


//public class Object_Transparent : BaseEvent
//{
//    static public float msOutTime = 0.2f;
//    static public float msUseTime = 0.8f;

//    protected ValueAnimation mAlpha = new ValueAnimation();
//    Material[] mUseMaterial;
//    Material[] mNowMaterial;

//    public Renderer[] mRenders;
//    public bool mbExist = true;

//    public bool mbNeedFinish = false;

//    public virtual Renderer[] GetRenderers() { return mRenders; }

//    public override bool _DoEvent()
//    {
//        InitReady();
//        mAlpha.Start(1, 0.2f, msOutTime);

//        StartUpdate();

//        return true;
//    }

//    public override void setData(tNiceData data) { }
//    public override tNiceData getData() { return null; }


//    public void InitReady()
//    {
//        Renderer[] render = GetRenderers();

//        mUseMaterial = new Material[render.Length];
//        mNowMaterial = new Material[render.Length];
//        for (int i = 0; i < render.Length; ++i)
//        {
//            mUseMaterial[i] = render[i].material;

//            mNowMaterial[i] = new Material(GameCommon.FindShader("Transparent/Diffuse"));
//            mNowMaterial[i].mainTexture = mUseMaterial[i].mainTexture;
//            render[i].material = mNowMaterial[i];
//        }
//    }

//    public override bool Update(float dT)
//    {
//        float nowAlpha;
//        bool bFinish = mAlpha.Update(dT, out nowAlpha);

//        for (int i = 0; i < mNowMaterial.Length; ++i)
//        {
//            Color temp = mNowMaterial[i].color;
//            temp.a = nowAlpha;
//            mNowMaterial[i].color = temp;
//        }

//        if (bFinish && mbNeedFinish)
//            Finish();

//        return !bFinish;
//    }

//    public override bool _OnFinish()
//    {
//        if (mbNeedFinish)
//        {
//            Renderer[] render = GetRenderers();
//            for (int i = 0; i < render.Length; ++i)
//            {
//                render[i].material = mUseMaterial[i];
//            }
//        }
//        else
//        {
//            SetFinished(false);
//            mbNeedFinish = true;
//            mAlpha.Start(mAlpha.NowValue(), 1, msUseTime);
//            StartUpdate();
//        }

//        return true;
//    }


//    public void ContinueTran()
//    {
//        if (mbNeedFinish)
//        {
//            mbNeedFinish = false;
//            if (GetFinished())
//            {
//                SetFinished(false);
//                _DoEvent();
//            }
//            else
//            {
//                mAlpha.Start(mAlpha.NowValue(), 0.2f, msOutTime);
//                StartUpdate();
//            }
//        }

//    }

//}

public class Object_Transparent : BaseEvent
{
    static public float msOutTime = 0.2f;
    static public float msUseTime = 0.8f;

    protected ValueAnimation mAlpha = new ValueAnimation();

    public Renderer[] mRenders;
    public bool mbExist = true;

    public bool mbNeedFinish = false;


    struct RenderInfo
    {
        public Renderer mRenderer;
        public Shader[] mUseShader;
    }

    RenderInfo[] mRenderList;

    public virtual Renderer[] GetRenderers() { return mRenders; }

    public override bool _DoEvent()
    {
        InitReady();
        mAlpha.Start(1, 0.2f, msOutTime);

        StartUpdate();

        return true;
    }

    public override void setData(tNiceData data) { }
    public override tNiceData getData() { return null; }


    public void InitReady()
    {
        Renderer[] render = GetRenderers();
        mRenderList = new RenderInfo[render.Length];
        for (int i = 0; i < render.Length; ++i)
        {
            mRenderList[i] = new RenderInfo();
            mRenderList[i].mRenderer = render[i];
            mRenderList[i].mUseShader = new Shader[render[i].materials.Length];

            for (int n = 0; n < render[i].materials.Length; ++n)
            {
                Material mat = render[i].materials[n];
                mRenderList[i].mUseShader[n] = mat.shader;

                mat.shader = GameCommon.FindShader("Transparent/Diffuse");
            }
        }
    }

    public override bool Update(float dT)
    {
        float nowAlpha;
        bool bFinish = mAlpha.Update(dT, out nowAlpha);

        foreach (RenderInfo info in mRenderList)
        {
            foreach (Material mat in info.mRenderer.materials)
            {
                Color temp = mat.color;
                temp.a = nowAlpha;
                mat.color = temp;
            }
        }

        if (bFinish && mbNeedFinish)
            Finish();

        return !bFinish;
    }

    public override bool _OnFinish()
    {
        if (mbNeedFinish)
        {
            foreach (RenderInfo info in mRenderList)
            {
                for (int i = 0; i < info.mRenderer.materials.Length; ++i)
                {
                    info.mRenderer.materials[i].shader = info.mUseShader[i];
                }
            }
        }
        else
        {
            SetFinished(false);
            mbNeedFinish = true;
            mAlpha.Start(mAlpha.NowValue(), 1, msUseTime);
            StartUpdate();
        }

        return true;
    }


    public void ContinueTran()
    {
        if (mbNeedFinish)
        {
            mbNeedFinish = false;
            if (GetFinished())
            {
                SetFinished(false);
                _DoEvent();
            }
            else
            {
                mAlpha.Start(mAlpha.NowValue(), 0.2f, msOutTime);
                StartUpdate();
            }
        }

    }

}

public class Object_OutLine : ObjectEvent
{
	struct RenderInfo
	{
		public Renderer mRenderer;
		public Material mUseMaterail;        
	}
	
	RenderInfo[] mRenderList;

    public virtual Renderer[] GetRenderers() { return mOwner.mBodyObject.GetComponentsInChildren<Renderer>(); }

    public override bool _DoEvent()
    {
        InitReady();

        return true;
    }


    public void InitReady()
    {
		Renderer[] render = GetRenderers();
		mRenderList = new RenderInfo[render.Length];
		for (int i = 0; i < render.Length; ++i)
		{
			mRenderList[i] = new RenderInfo();
			mRenderList[i].mRenderer = render[i];
			mRenderList[i].mUseMaterail = render[i].material;
            render[i].material = new Material(render[i].material);
			
			//for (int n = 0; n < render[i].materials.Length; ++n )
			{
				//Material mat = render[i].materials[n];				
                Material mat = render[i].material;
				if(mat.name != "_niceSprite_")
				{
					mat.shader = GameCommon.FindShader("NewOutLine");
					mat.SetColor("_OutlineColor", Color.red);
				}
			}
		}

    }


    public override bool _OnFinish()
    {		
		foreach (RenderInfo info in mRenderList)
		{
            if (info.mRenderer == null)
                continue;
            //for (int i = 0; i < info.mRenderer.materials.Length; ++i)
            //{
            //    info.mRenderer.materials[i].shader = info.mUseShader[i];
            //}
            info.mRenderer.material = info.mUseMaterail;
		}       
		return true;
    }
}


public class Object_HeightLight : ObjectEvent
{
    struct RenderInfo
    {
        public Renderer mRenderer;
        public Shader[] mUseShader;        
    }

    RenderInfo[] mRenderList;

    public virtual Renderer[] GetRenderers() 
    {
        return GetOwner().mBodyObject.GetComponentsInChildren<Renderer>(); 
    }

    public override bool _DoEvent()
    {
        InitReady();       
        return true;
    }

    public override void setData(tNiceData data) { }
    public override tNiceData getData() { return null; }


    public void InitReady()
    {
        Renderer[] render = GetRenderers();
        mRenderList = new RenderInfo[render.Length];
        for (int i = 0; i < render.Length; ++i)
        {
            mRenderList[i] = new RenderInfo();
            mRenderList[i].mRenderer = render[i];
            mRenderList[i].mUseShader = new Shader[render[i].materials.Length];
            
            for (int n = 0; n < render[i].materials.Length; ++n )
            {
                Material mat = render[i].materials[n];
                mRenderList[i].mUseShader[n] = mat.shader;
                
				if(mat.name != "_niceSprite_")
                {
					mat.shader = GameCommon.FindShader("Easy/WhiteColor");
                    mat.SetColor("_TintColor", new Color(66.0f/255, 66.0f/255, 66.0f/255));
                }
            }
        }
    }

    public override void _OnOverTime()
    {
        Finish();
    }

    public override bool _OnFinish()
    {
      	//if (GetOwner()==null || GetOwner().IsDead())
			//return false;

            foreach (RenderInfo info in mRenderList)
            {
                if (info.mRenderer == null)
                    continue;
                for (int i = 0; i < info.mRenderer.materials.Length; ++i)
                {
                    info.mRenderer.materials[i].shader = info.mUseShader[i];
                }
            }       
        return true;
    }
}