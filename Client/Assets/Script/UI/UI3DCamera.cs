using UnityEngine;
using System.Collections;

public class UI3DCamera : MonoBehaviour 
{
	static int sCount = 0;


    RenderTexture m3DTexture;
    public int mModelIndex = 0;
    public int mDepth = 6;

    public GameObject mModel;

	public void CreateObject (int iModelIndex, BaseObject obj, int iDepth)
    {
//		if(mModel != null)
//			GameObject.Destroy(mModel);

		mModelIndex = iModelIndex;
		mDepth = iDepth;

		GameObject sceneObj = GameCommon.FindObject(gameObject, "3d_scene");
        sceneObj.transform.localPosition = Vector3.left * (10000 * (++sCount%10 + 1));

        bool bIsShopGainPetWinOpen = GameCommon.bIsLogicDataExist("shop_gain_pet_window") && (DataCenter.GetData("shop_gain_pet_window") as tWindow).IsOpen();

        int iWidth = 512;
        int iHeight = 512;
        float fScale = 1;
		if((DataCenter.GetData("BOSS_APPEAR_WINDOW") as tWindow).IsOpen())
		{
            iWidth = 1024;
            iHeight = 1024;
        }
        else if (MainUIScript.mCurIndex == MAIN_WINDOW_INDEX.ShopWindow && !bIsShopGainPetWinOpen)
        {
            iWidth = 200;
            iHeight = 256;
            fScale = 2;
        }

        m3DTexture = new RenderTexture(iWidth, iHeight, 24, RenderTextureFormat.ARGB32);
        Camera cam = transform.Find("3d_scene/3DCamera").GetComponent<Camera>();
        //        Camera cam = gameObject.GetComponentInChildren<Camera>();
        cam.targetTexture = m3DTexture;
        gameObject.layer = 29;

        cam.cullingMask = 1 << 30;
        cam.clearFlags = CameraClearFlags.SolidColor;

        cam.orthographic = true;

        cam.pixelRect = new Rect(0, 0, iWidth, iHeight);
        //cam.pixelWidth = iWidth;

        UITexture tex = transform.Find("Texture").GetComponent<UITexture>();
        //        UITexture tex = gameObject.GetComponentInChildren<UITexture>();
        tex.mainTexture = m3DTexture;
        tex.width = iWidth;
        tex.height = iHeight;
        tex.depth = mDepth;
		
		if (mModelIndex > 0)
        {
            GameObject modelObject = GameCommon.FindObject(gameObject, "model");
            foreach (Transform f in modelObject.transform)
            {
                Destroy(f.gameObject);
            }

//            BaseObject obj = ObjectManager.Self.CreateObject(mModelIndex);
            obj.SetVisible(true);

            modelObject.transform.localScale = Vector3.one * fScale;

			Vector3 localPosition = obj.mMainObject.transform.localPosition;
			Vector3 localScale = obj.mMainObject.transform.localScale;
			Quaternion localRotation = obj.mMainObject.transform.localRotation;

			Transform trans = obj.mMainObject.transform;
			obj.mMainObject.transform.parent = modelObject.transform;
			obj.mMainObject.transform.localPosition = localPosition;
			obj.mMainObject.transform.localScale = localScale;
			obj.mMainObject.transform.localRotation = localRotation;
			mModel = obj.mMainObject;
            GameCommon.SetLayer(mModel, 30);

            //by chenliang
            //begin

// 			if((DataCenter.GetData("STAGE_INFO_WINDOW") as tWindow).IsOpen())
// 			{
// 				obj.mMainObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y - 70f, localPosition.z);
// 			}
// 			else if((DataCenter.GetData("BOSS_APPEAR_WINDOW") as tWindow).IsOpen())
// 			{
// 				modelObject.transform.localScale = Vector3.one * 0.6f;
// 				obj.mMainObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y - 50f, localPosition.z);
// 			}
//---------------------
            if ((DataCenter.GetData("BOSS_APPEAR_WINDOW") as tWindow).IsOpen())
            {
                modelObject.transform.localScale = Vector3.one * 0.6f;
                obj.mMainObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y - 50f, localPosition.z);
            }
            else if ((DataCenter.GetData("STAGE_INFO_WINDOW") as tWindow).IsOpen())
            {
                obj.mMainObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y - 70f, localPosition.z);
            }

            //end
			else if((DataCenter.GetData("BOSS_STAGE_INFO_WINDOW") as tWindow).IsOpen())
			{
				modelObject.transform.localScale = Vector3.one;
				obj.mMainObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y - 50f, localPosition.z);
			}
			else if((DataCenter.GetData("PET_INFO_SINGLE_WINDOW") as tWindow).IsOpen()
			        || (DataCenter.GetData("PVP_PET_INFO_SINGLE_WINDOW") as tWindow).IsOpen())
			{
				obj.mMainObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y - 50f, localPosition.z);
			}
			else if(GameCommon.bIsLogicDataExist("SCROLL_WORLD_MAP_WINDOW") && (DataCenter.GetData("SCROLL_WORLD_MAP_WINDOW") as tWindow).IsOpen())
			{
				obj.mMainObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y + 80f, localPosition.z);
				obj.mMainObject.transform.Rotate(new Vector3(0, 180f, 0));
			}
            else if (MainUIScript.mCurIndex == MAIN_WINDOW_INDEX.ShopWindow && !bIsShopGainPetWinOpen)
            {
                obj.mMainObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y - 80f, localPosition.z);
                tex.transform.localPosition = new Vector3(tex.transform.localPosition.x, (-86.42999f + 60f), tex.transform.localPosition.z);
            }
       }

	}


    //// Update is called once per frame
    //void Update()
    //{
    //    if (mModel != null && mModel.layer != 1 << 30)
    //    {
    //        GameCommon.SetLayer(mModel, 1 << 30);
    //    }
    //}
}
