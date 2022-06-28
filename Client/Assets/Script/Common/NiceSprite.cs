using UnityEngine;
using System.Collections;


public class NiceSprite
{
	//static GameObject msPrefabObject;
	
	public GameObject mMain;
	public GameObject mSprite;
	public Transform mMainTransform;
	
	static public GameObject CreateObject(string name, bool bCenter)
	{
		GameObject msPrefabObject = new GameObject(name);
		//GameCommon.InitSpriteRenderObject(ref msPrefabObject);
		MeshFilter  meshFilter = (MeshFilter)msPrefabObject.AddComponent("MeshFilter");
		
		Mesh mMesh = meshFilter.mesh; 
		GameCommon.ReadyMesh(ref mMesh, bCenter);
		
		MeshRenderer r = msPrefabObject.AddComponent("MeshRenderer") as MeshRenderer;
		
		msPrefabObject.renderer.castShadows = false;
		msPrefabObject.renderer.receiveShadows = false;
		
		//MeshRenderer r = msPrefabObject.GetComponent<MeshRenderer>();
		//r.material = new Material(GameCommon.FindShader.("Diffuse"));
		//r.enabled = false;
		//msPrefabObject.SetActive(false);
		
		return msPrefabObject;
	}
	
	static public NiceSprite Create(string name, GameObject prefab, bool bCenter)
	{
		NiceSprite s = new NiceSprite();
		
		if (prefab==null)
		{
            s.mSprite = CreateObject("_niceSprite_", bCenter);
		}
		else
			s.mSprite = (GameObject)GameObject.Instantiate(prefab);
		
		s.mMain = new GameObject(name);
		
		s.mMainTransform = s.mMain.transform;
		
		s.mSprite.transform.parent = s.mMainTransform;
		
		s.mSprite.transform.localPosition = Vector3.zero;
		s.mSprite.transform.localRotation = Quaternion.identity;
		s.mSprite.transform.localScale = Vector3.one;
		
		s.SetVisible(true);
		
		return s;
	}
	
	public virtual void SetSize(float x, float y)
	{
		mSprite.transform.localScale = new Vector3(x, y, 1);
	}
	
	public void SetVisible(bool bShow)
	{
		if (mSprite!= null)
			mSprite.SetActive(bShow);
		if(mMain != null)
			mMain.SetActive (bShow);
	}
	
	public void SetColor(Color co)
	{
		MeshRenderer r = mSprite.GetComponent<MeshRenderer>();
		r.material.color = co;
		r.material.SetColor("_Emission", co);
	}
	
	public void SetDirection(Quaternion quDir)
	{
		mSprite.transform.localRotation = quDir;
	}
	
	public void SetMaterail(string shaderName)
	{
		MeshRenderer r = mSprite.GetComponent<MeshRenderer>();
		r.material = new Material(GameCommon.FindShader(shaderName));
	}
	
	public void SetMaterail(Material mat)
	{
		MeshRenderer r = mSprite.GetComponent<MeshRenderer>();
		r.material = mat;
	}

    public Material GetMaterail()
    {
        MeshRenderer r = mSprite.GetComponent<MeshRenderer>();
		return r.material;	
    }
	
	public void SetTexture(string textureResName)
	{
		SetTexture(GameCommon.LoadTexture(textureResName, LOAD_MODE.RESOURCE));
	}
	
	public void SetTexture(string indexName, string textureResName)
	{
		MeshRenderer r = mSprite.GetComponent<MeshRenderer>();
		r.material.SetTexture(indexName, GameCommon.LoadTexture(textureResName, LOAD_MODE.RESOURCE));
	}
	
	public void SetTexture(Texture tex)
	{
		MeshRenderer r = mSprite.GetComponent<MeshRenderer>();
		r.material.mainTexture = tex;
	}
	
	public void Destroy()
	{
		if (mSprite!=null)
		{
			mSprite.SetActive(true);
			GameObject.DestroyImmediate(mSprite);
            GameObject.DestroyImmediate(mMain);
		}
	}
	
	public bool SetUV(int uvIndex, Vector2[] uv)
	{
		MeshFilter filter= mSprite.GetComponent<MeshFilter>();
		if (filter!=null)
		{
			if (uvIndex == 0)
				filter.mesh.uv = uv;
			else if (uvIndex == 1)
				filter.mesh.uv1 = uv;
			else if (uvIndex == 2)
				filter.mesh.uv2 = uv;
			else
				return false;
			
			return true;
		}
		
		return false;
	}

    public void SetUV(int uvIndex, float u1, float v2, float u2, float v1)
    {
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(u1, v1);
        uv[1] = new Vector2(u2, v1);
        uv[2] = new Vector2(u1, v2);
        uv[3] = new Vector2(u2, v2);
        SetUV(0, uv);
    }

	public void SetAtlasTexture(string atlasName, string atlasSpriteName)
	{
        UIAtlas atlas = GameCommon.LoadUIAtlas(atlasName);
        if (atlas != null)
        {
            UISpriteData spriteData = atlas.GetSprite(atlasSpriteName);
            if (spriteData != null)
            {
                SetTexture(atlas.texture);
                int w = atlas.texture.width;
                int h = atlas.texture.height;
                SetUV(0, (float)spriteData.x / w, (float)(h - spriteData.y) / h, (float)(spriteData.x + spriteData.width) / w, (float)(h - (spriteData.y + spriteData.height)) / h);
                SetSize(spriteData.width, spriteData.height);
            }
        }     
	}
	
	public void SetLayer(int layerIndex)
	{
		mMainTransform.gameObject.layer = layerIndex;
		mSprite.layer = layerIndex;
	}
	
	public virtual void SetSize(float x, float y, bool bCenter)
	{
		SetSize(x, y);
		//if (bCenter)
		//    mSprite.transform.localPosition = new Vector3(-x * 0.5f, -y * 0.5f, 1);
	}
}

public class CenterSprite : NiceSprite
{
	public override void SetSize(float x, float y)
	{
		base.SetSize(x, y);
		mSprite.transform.localPosition = new Vector3(-x*0.5f, -y*0.5f, 1);
	}
}


public class CircularSprite
{
    public NiceSprite mNiceSprite;

    public CircularSprite(bool bCenter)
    {
        mNiceSprite = NiceSprite.Create("nice_sprite", null, bCenter);
        mNiceSprite.SetMaterail("Game/MiniMap");
        mNiceSprite.SetTexture("_MaskTex", "textures/MiniMapMask");

        Vector2[] uv = new Vector2[4];

        float x1 = 0.0f;
        float y1 = 0.0f;

        float u2 = 1.0f;
        float v2 = 1.0f;

        uv[0] = new Vector2(x1, y1);
        uv[1] = new Vector2(u2, y1);
        uv[2] = new Vector2(x1, v2);
        uv[3] = new Vector2(u2, v2);
        mNiceSprite.SetUV(1, uv);
    }

    public NiceSprite Init(GameObject parentObject, float fScale)
    {
        //mNiceSprite.SetTexture(texName);
        mNiceSprite.mMainTransform.parent = parentObject.transform;
        mNiceSprite.mMainTransform.localPosition = new Vector3(0, 0, 0);
        mNiceSprite.mMainTransform.localScale = new Vector3(fScale, fScale, fScale);
        mNiceSprite.mMainTransform.localRotation = Quaternion.identity;
        mNiceSprite.SetLayer(CommonParam.UILayer);
        return mNiceSprite;
    }

    public void SetAtlasTexture(string atlasName, string atlasSpriteName)
    {
        mNiceSprite.SetAtlasTexture(atlasName, atlasSpriteName);
    }
}

public class BloodMarkSprite
{
	public NiceSprite mNiceSprite;
	
	public BloodMarkSprite(bool bCenter)
	{
		mNiceSprite = NiceSprite.Create("nice_sprite", null, bCenter);
		mNiceSprite.SetMaterail("Easy/EasyTransparent");
		mNiceSprite.SetTexture("_MaskTex", "textures/MiniMapMask");
		
		Vector2[] uv = new Vector2[4];
		
		float x1 = 0.0f;
		float y1 = 0.0f;
		
		float u2 = 1.0f;
		float v2 = 1.0f;
		
		uv[0] = new Vector2(x1, y1);
		uv[1] = new Vector2(u2, y1);
		uv[2] = new Vector2(x1, v2);
		uv[3] = new Vector2(u2, v2);
		mNiceSprite.SetUV(1, uv);
	}
	
	public void Init(GameObject parentObject, float fScale, float z)
	{
		//mNiceSprite.SetTexture(texName);
		mNiceSprite.mMainTransform.parent = parentObject.transform;
		mNiceSprite.mMainTransform.localPosition = new Vector3(-0.17f, 0, z);
		mNiceSprite.mMainTransform.localScale = new Vector3(fScale, fScale, fScale);
		mNiceSprite.mMainTransform.localRotation = Quaternion.identity;
		mNiceSprite.SetLayer(CommonParam.PlayerLayer);
	}
	
	public void SetAtlasTexture(string atlasName, string atlasSpriteName)
	{
		mNiceSprite.SetAtlasTexture(atlasName, atlasSpriteName);
	}
}

