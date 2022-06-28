using UnityEngine;
using System.Collections;

public class ProgressBar
{
	protected NiceSprite mShow;
	protected NiceSprite mBack;
	
	protected float mWidth = 1;
	protected float mHeight = 0.1f;
	
	static protected float msBackOff = -0.0008f;
	static public GameObject mWorldCenter;
	
	public ProgressBar()
	{
		mShow = NiceSprite.Create("test_hp", null, false);
		//mShow.mSprite.transform.localRotation = Quaternion.Euler(48.1f, 150, 0);
		
		//mShowHpBar.mMainTransform.parent = mMainObject.transform;
		mBack = NiceSprite.Create("_hp_back", null, false);

        mBack.mMainTransform.parent = mWorldCenter.transform;
        mBack.mMainTransform.localRotation = Quaternion.Euler(CameraMoveEvent.mCameraAngles.x, 0f, 0f);//Quaternion.Euler(48.1f, 0, 0);

        mShow.mMainTransform.parent = mBack.mMainTransform;
        mShow.mMainTransform.localPosition = new Vector3(0, 0, -0.01f);
        mShow.mMainTransform.localScale = Vector3.one;
        mShow.mMainTransform.localRotation = Quaternion.identity;


	}
	
    public virtual void Destroy()
    {
        if (mShow!=null)
            mShow.Destroy();
        if (mBack!=null)
            mBack.Destroy();
    }

	public virtual  void Init(float width, float height, Color showColor, Color backColor)
	{	
		mWidth = width;
		mHeight = height;
		
		mShow.SetSize(mWidth, mHeight);
		mBack.SetSize(mWidth, mHeight);
		
		mShow.SetMaterail("VertexLit");
		mShow.SetColor(showColor);
		
		mBack.SetMaterail("Transparent/VertexLit");
		mBack.SetColor(backColor);
		
		
		mShow.mSprite.transform.localPosition = new Vector3(-mWidth*0.5f, 0, 0);
		//mBack.mSprite.transform.localPosition = new Vector3(-mWidth*0.5f, msBackOff, 0);
        
	}

	
	public void SetPos(Vector3 pos)
	{
		//mShow.mMainTransform.localPosition = pos;
		mBack.mMainTransform.localPosition = pos;
	}
	
	public virtual void SetValue(float nowValue, float maxValue)
	{
        float u2 = Mathf.Clamp01(nowValue / maxValue) * mWidth;
		mShow.SetSize(u2, mHeight);

        Vector2[] uv = new Vector2[4];
        float v2 = 1.0f;

        float zF = 0.0f;
        uv[0] = new Vector2(zF, zF);
        uv[1] = new Vector2(u2, zF);
        uv[2] = new Vector2(zF, v2);
        uv[3] = new Vector2(u2, v2);
        MeshFilter r = mShow.mSprite.GetComponent<MeshFilter>();
        r.mesh.uv = uv;
	}
	
	public virtual void SetVisible(bool bShow)
	{
		mShow.SetVisible(bShow);
		mBack.SetVisible(bShow);
	}
	
	public virtual void Update(float dt){}
	
	public virtual void InitValue(float n, float max){ SetValue(n, max); }

	public virtual void InitBloodMark (int elementIndex){}
	public virtual void InitBossBloodMark (){}
}