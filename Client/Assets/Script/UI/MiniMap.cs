using UnityEngine;
using System.Collections;
using DataTable;

public class MiniMap : MonoBehaviour 
{
    static public NiceSprite MapSprite;

	public GameObject mSprite;
	public UIAtlas mUIAtls;
	static public MiniMap Self;

    public float mStart_x;
    public float mStart_z;
    public float mEnd_x;
    public float mEnd_z;
    public float mMiniMapWidth;
    public float mMiniMapHeight;

	public GameObject mMiniMapRound;

	// Use this for initialization
	void Start () 
    {
		Self = this;       

        //by chenliang
        //begin

//		InitData();
//-----------
        mIsInited = false;

        //end
	}

//	void FixedUpdate()
//	{
//		BaseObject nextMonster;
//		nextMonster = Character.Self.mAutoBattleAI.GetNextAttackMonster ();
//		if(nextMonster != null)
//		{
//			Vector3 v3MonsterPosition = nextMonster.mMainObject.transform.position;
//			v3MonsterPosition = new Vector3(v3MonsterPosition.x, 0, v3MonsterPosition.z);
//
//			Vector3 v3CharacterPosition = Character.Self.GetPosition ();
//			v3CharacterPosition = new Vector3(v3CharacterPosition.x, 0, v3CharacterPosition.z);
//
//			Vector3 v3Direction = (v3MonsterPosition - v3CharacterPosition);
//			float fDot = Vector3.Dot (v3Direction.normalized, Vector3.right);
//			float fAngle = Vector3.Angle (v3Direction.normalized, Vector3.forward);
//
////			float fAngleZ = mMiniMapRound.transform.localEulerAngles.z;
//			Quaternion v3Target = Quaternion.Euler (0f, 0f, 0f);
//			float fSmooth = 1.0f;
//
////			if(fDot > 0) mMiniMapRound.transform.localEulerAngles = new Vector3(0f, 0f, 360.0f - fAngle);
////			else if(fDot < 0) mMiniMapRound.transform.localEulerAngles = new Vector3(0f, 0f, fAngle);
//			if(fDot > 0) 
//			{
//				v3Target = Quaternion.Euler (0f, 0f, 360.0f - fAngle);
//			}
//			else if(fDot < 0) 
//			{
//				v3Target = Quaternion.Euler (0f, 0f, fAngle);
//			}
//			else if(fDot == 0 && Vector3.Dot (v3Direction.normalized, Vector3.forward) < 0)
//			{
//				v3Target = Quaternion.Euler (0f, 0f, 180.0f);
//			}
//
//			mMiniMapRound.transform.localRotation = Quaternion.Lerp (mMiniMapRound.transform.localRotation, v3Target, Time.deltaTime * fSmooth);
//		}
//	}

	public void InitData()
	{
		if(!gameObject.activeSelf) gameObject.SetActive (true);
		mMiniMapRound.transform.rotation = Quaternion.identity;

		int stageIndex = DataCenter.Get("CURRENT_STAGE");
		DataRecord configRecord = DataCenter.mStageTable.GetRecord(stageIndex);
		string texName = configRecord.getData("MINIMAP_TEXTURE");
		
		mStart_x = configRecord.getData("START_X");
		mStart_z = configRecord.getData("START_Z"); 
		mEnd_x = configRecord.getData("END_X"); 
		mEnd_z = configRecord.getData("END_Z");
		mMiniMapWidth = configRecord.getData("MINIMAP_TEXTURE_W"); 
		mMiniMapHeight = configRecord.getData("MINIMAP_TEXTURE_H");

		UISprite [] sprArray = mSprite.GetComponentsInChildren<UISprite>();
		foreach(UISprite t in sprArray)
		{
			if(t != null) t.gameObject.SetActive (false);
		}
		GameObject miniMapObj = GameCommon.FindObject (gameObject, "miniMap");
		if(miniMapObj != null) Destroy (miniMapObj);

		MapSprite = NiceSprite.Create("miniMap", null, false);
		MapSprite.SetMaterail("Game/MiniMap");
		MapSprite.SetTexture("_MaskTex", "textures/MiniMapMask");
		MapSprite.SetTexture(texName);
		MapSprite.GetMaterail().color = new Color(1, 1, 1, 0.7f);
		MapSprite.mMainTransform.parent = gameObject.transform;
		MapSprite.mMainTransform.localPosition = Vector3.zero;
		MapSprite.mMainTransform.localScale *= (0.85f *0.6f * GlobalModule.GetResolutionRatio());
		MapSprite.mMainTransform.localRotation = Quaternion.identity;
		MapSprite.SetLayer(CommonParam.UILayer);
//		MapSprite.mMainTransform.parent.localScale *= 0.8f;
		
		Vector2[] uv = new Vector2[4];
		
		float x1 = 0.0f;
		float y1 = 0.0f;
		
		float u2 = 1.0f;
		float v2 = 1.0f;
		
		uv[0] = new Vector2(x1, y1);
		uv[1] = new Vector2(u2, y1);
		uv[2] = new Vector2(x1, v2);
		uv[3] = new Vector2(u2, v2);
		MapSprite.SetUV(1, uv);
		
		DataCenter.Self.registerData("MINI_MAP", new MiniMapData());

        if (Character.Self != null)
            DataCenter.SetData("MINI_MAP", "POSITION", 0);

		//by chenliang
        //begin

//		MainProcess.mStage.Start();
//-----------

        //end
	}
    //by chenliang
    //begin

    private bool mIsInited = false;     //是否已经初始化
    
    void Update()
    {
        if (GlobalModule.IsSceneLoadComplete)
        {
            if (!mIsInited)
            {
                mIsInited = true;
                InitData();
                DataCenter.SetData("MINI_MAP", "POSITION", 0);      //刷新小地图
            }
        }
    }

    //end


    static public void UpdatePosition(float u1, float v1, float u2, float v2)
    {
		if (MapSprite==null)
			return;

        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(u1, v1);
        uv[1] = new Vector2(u2, v1);
        uv[2] = new Vector2(u1, v2);
        uv[3] = new Vector2(u2, v2);
        MapSprite.SetUV(0, uv);
    }

	void OnDestroy()
	{
		MapSprite = null;
	}

    static public bool IsOpen() { return MapSprite != null;  }

    public Vector3 UpdateFlagPosition(Vector3 activePosition)
    {
        if (Character.Self == null)
            return Vector3.zero;

        float minimapSize = CommonParam.CreateMiniMapSize;
        float monsterPoint = CommonParam.MonsterPointTextureSize;
        float miniMapMaskSize = CommonParam.MiniMapMaskSize;

        float ratioWidth = miniMapMaskSize / mMiniMapWidth;
        float ratioHeight = miniMapMaskSize / mMiniMapHeight;

        Vector3 v3 = activePosition;
        float u = (activePosition.x - mStart_x) / (mEnd_x - mStart_x);
        float v = (activePosition.z - mStart_z) / (mEnd_z - mStart_z);



        Vector3 v3Ch = Character.Self.GetPosition();
        float u1 = (v3Ch.x - MiniMap.Self.mStart_x) / (MiniMap.Self.mEnd_x - MiniMap.Self.mStart_x);
        float v1 = (v3Ch.z - MiniMap.Self.mStart_z) / (MiniMap.Self.mEnd_z - MiniMap.Self.mStart_z);


        float f1 = (minimapSize - monsterPoint) / ratioWidth;
        float f2 = (minimapSize - monsterPoint) / ratioHeight;

        return new Vector3((u - u1) * f1, (v - v1) * f2, 0);
    }

}

public class MiniMapData : tLogicData
{	
    public override void onChange(string key, object val)
    {
        if (!MiniMap.IsOpen())
            return;

        if (key == "POSITION")
        {
            Vector3 v3 = Character.Self.GetPosition();
			float miniMapMaskSize = CommonParam.MiniMapMaskSize;
			float ratioWidth = miniMapMaskSize/MiniMap.Self.mMiniMapWidth;
			float ratioHeight = miniMapMaskSize/MiniMap.Self.mMiniMapHeight;

			float u = (v3.x - MiniMap.Self.mStart_x)/(MiniMap.Self.mEnd_x - MiniMap.Self.mStart_x);
			float v = (v3.z - MiniMap.Self.mStart_z)/(MiniMap.Self.mEnd_z - MiniMap.Self.mStart_z);

			float u1 = u - ratioWidth/2;
			float v1 = v - ratioHeight/2;
			float u2 = u + ratioWidth/2;
			float v2 = v + ratioHeight/2;
			MiniMap.UpdatePosition(u1, v1,u2,v2);
        }
        //else if(key == "DEAD")
        //{
        //    MiniMap.Destroy (MiniMap.self.mSprite.gameObject);
        //}
    }

    public override tLogicData getData(string index)
    {
        if (MiniMap.MapSprite == null)
            return null;		

        if (index == "CREATE")
        {
            MonsterFlagData p = new MonsterFlagData();
            p.InitSprite();
			return p;
        }

        else if (index == "FRIEND")
        {
            MonsterFlagData p = new FriendFlagData();
            p.InitSprite();
            return p;
        }

        return null;
    }
}

public class MonsterFlagData : tLogicData
{
	protected UISprite mSprite = null;

	float minimapSize = CommonParam.CreateMiniMapSize;
	float monsterPoint = CommonParam.MonsterPointTextureSize;
	float miniMapMaskSize = CommonParam.MiniMapMaskSize;

    public  MonsterFlagData()
    {
		
    }

    public virtual void InitSprite()
    {
        mSprite = NGUITools.AddSprite (MiniMap.Self.mSprite, MiniMap.Self.mUIAtls, "Monster_point");
		mSprite.MakePixelPerfect ();
		mSprite.transform.localPosition = Vector3.zero;
		mSprite.transform.name = "Monster_point";
		mSprite.depth = 1;
        mSprite.gameObject.layer = CommonParam.UILayer;
    }

    public override void onChange(string keyIndex, object objVal)
    {
		if(keyIndex == "ROTATION_ROUND")
		{
			Vector3 v3MonsterPosition = (Vector3)objVal;
			v3MonsterPosition = new Vector3(v3MonsterPosition.x, 0, v3MonsterPosition.z);
			
			Vector3 v3CharacterPosition = Character.Self.GetPosition ();
			v3CharacterPosition = new Vector3(v3CharacterPosition.x, 0, v3CharacterPosition.z);
			
			Vector3 v3Direction = (v3MonsterPosition - v3CharacterPosition);
			float fDot = Vector3.Dot (v3Direction.normalized, Vector3.right);
			float fAngle = Vector3.Angle (v3Direction.normalized, Vector3.forward);

			Quaternion v3Target = Quaternion.Euler (0f, 0f, 0f);
			float fSmooth = 1.0f;

			if(fDot > 0) 
			{
				v3Target = Quaternion.Euler (0f, 0f, 360.0f - fAngle);
			}
			else if(fDot < 0) 
			{
				v3Target = Quaternion.Euler (0f, 0f, fAngle);
			}
			else if(fDot == 0 && Vector3.Dot (v3Direction.normalized, Vector3.forward) < 0)
			{
				v3Target = Quaternion.Euler (0f, 0f, 180.0f);
			}
			
			MiniMap.Self.mMiniMapRound.transform.localRotation = Quaternion.Lerp ( MiniMap.Self.mMiniMapRound.transform.localRotation, v3Target, Time.deltaTime * fSmooth);
		}

        if (keyIndex == "POS")
        {
			Vector3 v3 = (Vector3)objVal;

            mSprite.transform.localPosition = MiniMap.Self.UpdateFlagPosition(v3); // new Vector3((u - u1) * f1, (v - v1) * f2, 0); 
        }

		if(keyIndex == "NAME")
		{
			if(mSprite == null)
				return;
			GameObject g = (GameObject)objVal;
			mSprite.name = g.transform.name;
		}

		if (keyIndex == "SHOW")
		{
			float ratioWidth = miniMapMaskSize/MiniMap.Self.mMiniMapWidth;

			Vector3 v3 = (Vector3)objVal;
			float distance = (MiniMap.Self.mEnd_x - MiniMap.Self.mStart_x) * ratioWidth/2;
			distance *= 0.85f;
			if(mSprite == null || Character.Self == null)
				return;
			if((Character.Self.GetPosition() - v3 ).sqrMagnitude > distance*distance)
			{
				mSprite.gameObject.SetActive (false);
			}
			else 
			{
				mSprite.gameObject.SetActive (true);
			}
		}

		else if(keyIndex == "DEAD")
		{
//			GameObject g = (GameObject )objVal;
//			DEBUG.Log (g.transform.name);
			if (mSprite!=null && mSprite.gameObject!=null)
			{
				MiniMap.Destroy (mSprite.gameObject);
				mSprite = null;
			}
		}
    }

    public override void onRemove()
    {
        if (mSprite != null && mSprite.gameObject != null)
        {
            GameObject.Destroy(mSprite.gameObject);
            mSprite = null;
        }
    }
	
}

public class FriendFlagData : MonsterFlagData
{
    public override void InitSprite()
    {
        mSprite = NGUITools.AddSprite(MiniMap.Self.mSprite, MiniMap.Self.mUIAtls, "Friend_point");
        mSprite.MakePixelPerfect();
        mSprite.transform.localPosition = Vector3.zero;
        mSprite.transform.name = "Friend_point";
        mSprite.depth = 1;
        mSprite.gameObject.layer = CommonParam.UILayer;
    }
}

