using UnityEngine;
using System.Collections;

public class ActiveBirthForUI : MonoBehaviour {

    public byte mIndex = 0;
	public int mBirthConfigIndex = 0;
	public int mObjectType = -1;
	public int mOwner = 1000;

    public BaseObject mActiveObject = null;
	public bool mbIsCanOperate = true;
	public float mfScaleRatio = 1.0f;
	public GameObject mCollider = null;
	public bool mbIsMainUI = false;

	// float mIdleSecond = 0;

	void Start()
	{
//		Logic.tEvent idleEvent = Logic.EventCenter.Self.StartEvent("RoleSelUI_PlayIdleEvent");
//		mActiveObject.PlayMotion("cute", idleEvent);
	}
	void Update()
	{
//		if(mActiveObject.mIdleSecond > Random.Range(5, 10))
//		{
//			Logic.tEvent idleEvent = Logic.EventCenter.Self.StartEvent("RoleSelUI_PlayIdleEvent");
//			mActiveObject.PlayMotion("cute", idleEvent);
//			mActiveObject.mIdleSecond = 0;
//		}
//		else
//		{
//			mActiveObject.mIdleSecond += Time.deltaTime;
//		}
	}

	public void Init()
	{
		if(mBirthConfigIndex <= 0)
			return;

		BaseObject obj;

		if (mObjectType>=0)
			obj = ObjectManager.Self.CreateObject((OBJECT_TYPE)mObjectType, mBirthConfigIndex, true, mbIsMainUI);
		else
			obj = ObjectManager.Self.CreateObject(mBirthConfigIndex, true, mbIsMainUI);

		if (obj != null)
		{
			obj.SetCamp(mOwner);
			obj.mMainObject.transform.parent = transform.parent;

		    Vector3 pos = gameObject.transform.position;
            obj.SetPosition(pos);//new Vector3( 0,-93,-500)

			float fUIScaleRatio = 1.0f;
			if(mbIsMainUI)
			{
				fUIScaleRatio = obj.mConfigRecord.get ("SCALE");

				obj.mMainObject.transform.localScale = gameObject.transform.localScale * mfScaleRatio * fUIScaleRatio;

				obj.mMainObject.transform.localRotation = gameObject.transform.localRotation;
				SetGameObjectAndChildernLayer(obj.mMainObject, "Player");
			}
			else
			{
				fUIScaleRatio = obj.mConfigRecord.get ("UI_SCALE");
				SetScale(obj, mfScaleRatio * fUIScaleRatio);

                bool bIsShopGainPetWinOpen = GameCommon.bIsLogicDataExist("shop_gain_pet_window") && (DataCenter.GetData("shop_gain_pet_window") as tWindow).IsOpen();

				if((DataCenter.GetData("BOSS_APPEAR_WINDOW") as tWindow).IsOpen()
				   || (DataCenter.GetData("BOSS_STAGE_INFO_WINDOW") as tWindow).IsOpen()
				   || (DataCenter.GetData("STAGE_INFO_WINDOW") as tWindow).IsOpen()
                   || (DataCenter.GetData("BOSS_RAID_WINDOW") as tWindow).IsOpen()
                   || (MainUIScript.mCurIndex == MAIN_WINDOW_INDEX.ShopWindow && !bIsShopGainPetWinOpen)
				   || (OBJECT_TYPE.PET == ObjectManager.Self.GetObjectType(mBirthConfigIndex)
				       && TableCommon.GetNumberFromActiveCongfig(mBirthConfigIndex, "RENDER_TYPE") == 1))
				{
					obj.mMainObject.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

					GameObject UI_3D_Obj = GameCommon.FindObject(transform.parent.gameObject, "ui_3d_model");
                    if (UI_3D_Obj == null)
                        UI_3D_Obj = GameCommon.LoadUIPrefabs("ui_3d_model", transform.parent.gameObject);
                    else
                        UI_3D_Obj.SetActive(true);
					
					UI_3D_Obj.transform.parent = transform.parent;
					UI3DCamera ui_3dCamera = UI_3D_Obj.GetComponent<UI3DCamera>();
					
					if(ui_3dCamera != null)
					{
						ui_3dCamera.CreateObject(mBirthConfigIndex, obj, 125);
						ui_3dCamera.mModel.name = "fuck";

                        //by cheliang
                        //begin

                        //为了解决RenderTexture贴图过度曝光
                        Camera cam = ui_3dCamera.transform.Find("3d_scene/3DCamera").GetComponent<Camera>();
                        if (cam != null)
                            cam.isOrthoGraphic = true;

                        //end
					}
				}
				else
				{
					obj.mMainObject.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
					SetGameObjectAndChildernLayer(obj.mMainObject, CommonParam.UILayer);
				}

                // 设置初始位置
                float posY = obj.mConfigRecord.get("POS_Y");
                obj.mBodyObject.transform.localPosition = new Vector3(0, (-1)*posY, 0);
			}

            // 设置旋转角度
            float rotationY = obj.mConfigRecord.get ("ROTATION_Y");
            obj.mBodyObject.transform.localRotation = Quaternion.Euler(new Vector3(0, rotationY, 0) + obj.mBodyObject.transform.localEulerAngles);

			SetCollider(obj);
			SetCollider(mCollider, obj);
			//obj.mMainObject.transform.rotation = new Vector3(0, 180f, 0);
			//obj.InitPosition(transform.position);
			obj.SetVisible(true);
			//obj.mShowHpBar.SetVisible(false);
			obj.OnIdle();

//			obj.SetLightColor(new Color(0, 0, 0, 1));
			
			mActiveObject = obj;
			
			ActiveObjData.AddObj(obj);

			if(!obj.mbIsMainUI)
			{
				Transform parent = obj.mMainObject.transform.parent.parent.parent;
				UIPanel panel = parent.GetComponent<UIPanel>();
				if(panel != null && obj.mEffect != null)
				{
					int iLevel = panel.startingRenderQueue;

					ChangeParticleRenderQueue scrip = obj.mEffect.AddComponent<ChangeParticleRenderQueue>();
					scrip.renderQueue = iLevel + 7;
				}
			}
		}

		if(!mbIsMainUI && ObjectManager.Self.GetObjectType(mBirthConfigIndex) == OBJECT_TYPE.CHARATOR)
		{
			obj.SetLightColor(new Color(0, 0, 0, 1));
            Renderer[] rendlist = obj.mBodyObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer render in rendlist)
			{
				if (render != null)
				{
					//render.castShadows = false;
					//render.receiveShadows = false;
					//render.material.shader = GameCommon.FindShader("Unlit/Unlight");
				}
			}
		}
        else
        {
            int iModel = obj.mConfigRecord["MODEL"];
            string rimColor = TableManager.GetData("ModelConfig", iModel, "RIM_COLOR");
            char[] space = {' '};
            string[] colorVal = rimColor.Split(space, 4);
            if (colorVal.Length >= 4)
            {
                Color lightColor = new Color(float.Parse(colorVal[0]) / 255, float.Parse(colorVal[1]) / 255, float.Parse(colorVal[2]) / 255, float.Parse(colorVal[3]) / 255);
                Renderer[] rendlist = obj.mBodyObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer render in rendlist)
                {
                    if (render != null)
                    {
                        render.castShadows = false;
                        render.receiveShadows = false;
                        render.material.SetColor("_RimColor", lightColor);
                    }
                }
            }
        }
		gameObject.SetActive(false);
	}

	public void SetScale(BaseObject obj, float fScaleRatio)
	{
		if(obj != null)
			obj.SetScale(100 * fScaleRatio);
	}

	public void Init(bool bIsCanOperate, float fScaleRatio, GameObject obj)
	{
		mCollider = obj;
		Init (bIsCanOperate, fScaleRatio);
	}

	public void Init(bool bIsCanOperate, float fScaleRatio)
	{
		mbIsCanOperate = bIsCanOperate;
		mfScaleRatio = fScaleRatio;
		Init ();
	}

	public void Init(bool bIsCanOperate, float fScaleRatio, bool bIsMainUI)
	{
		mbIsCanOperate = bIsCanOperate;
		mfScaleRatio = fScaleRatio;
		mbIsMainUI = bIsMainUI;
		Init ();
	}

	void SetGameObjectAndChildernLayer(GameObject obj, int iLayer)
	{
		obj.layer = iLayer;
		for(int i=0; i<obj.transform.childCount; i++)
		{
			SetGameObjectAndChildernLayer(obj.transform.GetChild(i).gameObject, iLayer);
		}
	}

	void SetGameObjectAndChildernLayer(GameObject obj, string layerName)
	{
		obj.layer = LayerMask.NameToLayer(layerName);
		for(int i=0; i<obj.transform.childCount; i++)
		{
			SetGameObjectAndChildernLayer(obj.transform.GetChild(i).gameObject, layerName);
		}
	}

	public virtual void SetCollider(BaseObject obj)
	{
		if(obj != null && mbIsCanOperate)
		{
			CapsuleCollider coll = obj.mMainObject.GetComponent<CapsuleCollider>();
			if(coll == null)
			{
				coll = obj.mMainObject.AddComponent<CapsuleCollider>();
				coll.center = new Vector3(0, 1, 0);
                coll.radius = 1;
				coll.height = 2;
				coll.isTrigger = true;
			}
			UIRoleMastButton mastBtn = obj.mMainObject.GetComponent<UIRoleMastButton>();
			if(mastBtn == null)
			{
				mastBtn = obj.mMainObject.AddComponent<UIRoleMastButton>();
                mastBtn.mObject = obj;

			}
		}
	}

	public virtual void SetCollider(GameObject gameobj, BaseObject obj)
	{
		if(gameobj != null && obj != null && mbIsCanOperate)
		{
			UIRoleMastButton mastBtn = gameobj.GetComponent<UIRoleMastButton>();
			if(mastBtn == null)
			{
				mastBtn = gameobj.AddComponent<UIRoleMastButton>();
			}
			mastBtn.mObject = obj;
		}
	}

	public void OnDestroy()
	{
		if(mActiveObject != null)
			mActiveObject.OnDestroy();
	}
}

public class BossBirthOnApearUI : MonoBehaviour
{
    public BaseObject mActiveObject = null;

    void Update()
    {
        if (mActiveObject != null)
        {
            if (mActiveObject.IsAnimStoped())
            {
                ActiveObject o = mActiveObject as ActiveObject;
                if (o.mAnim.NowAnim() != "")
                    mActiveObject.PlayAnim(o.mAnim.NowAnim());
            }
        }
    }
}