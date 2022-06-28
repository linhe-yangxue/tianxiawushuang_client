using UnityEngine;
using System.Collections.Generic;
using Logic;
using DataTable;
using Utilities;


public class PlotTools
{
    public static Dictionary<int, GameObject> pictures = new Dictionary<int, GameObject>();

    private static Vector3 localScale = Vector3.zero;
    private static float manualHeight = 0f;

    public static GameObject CreateTexture(string path)
    {
        GameObject obj = new GameObject("pic");
        UITexture texture = obj.AddComponent<UITexture>();
        texture.mainTexture = GameCommon.LoadTexture(path, LOAD_MODE.RESOURCE);//Resources.Load(path, typeof(Texture)) as Texture;
        return obj;
    }

    public static GameObject CreateSprite(string atlasName, string spriteName)
    {
        GameObject obj = new GameObject("pic");
        UISprite sprite = obj.AddComponent<UISprite>();
        GameCommon.SetIcon(sprite, atlasName, spriteName);
        return obj;
    }

    public static GameObject PicLoad(int id, GameObject panel, int depth)
    {
        GameObject pic = null;

        if (pictures.TryGetValue(id, out pic))
        {
            PicInit(pic, panel, depth);
            return pic;
        }

        DataRecord record = DataCenter.mEventShow.GetRecord(id);

        if (record == null)
            return null;

        if (record["IMAGE_TYPE"] == 0)
            pic = CreateTexture(record["IMAGE_PATH"]);
        else
            pic = CreateSprite(record["IMAGE_ATLAS"], record["IMAGE_SPRITE"]);


        if (pic != null)
        {
            pic.name = "pic_" + id;
            PicInit(pic, panel, depth);
            pictures.Add(id, pic);           
        }

        return pic;
    }

    public static void PicInit(GameObject pic, GameObject panel, int depth)
    {
        pic.transform.parent = panel.transform;
        pic.transform.localScale = Vector3.one;
        pic.transform.localPosition = Vector3.zero;

        UIWidget widget = pic.GetComponent<UIWidget>();
        widget.depth = depth;
        widget.alpha = 0f;
        widget.MakePixelPerfect();
    }

    public static bool PicDel(int id)
    {
        GameObject pic = null;

        if (pictures.TryGetValue(id, out pic))
        {
            if (pic != null)
            {
                GameObject.Destroy(pic);
                pictures.Remove(id);
                return true;
            }
        }

        return false;
    }

    public static void Clear()
    {
        foreach (var pair in pictures)
        {
            if (pair.Value != null)
            {
                GameObject.Destroy(pair.Value);
            }
        }

        pictures.Clear();
    }

    public static GameObject GetPic(int id)
    {
        GameObject pic = null;
        pictures.TryGetValue(id, out pic);
        return pic;
    }

    public static void PicMove(GameObject pic, float xPercent, float yPercent, float delay, float duration)
    {
        if (pic == null)
            return;
        
        TweenPosition tween = pic.GetOrAddComponent<TweenPosition>();
        tween.from = pic.transform.localPosition;
        Vector3 offset = new Vector3(GetLocalWidth() * xPercent, GetLocalHeight() * yPercent, 0f);
        tween.to = pic.transform.localPosition + offset;
        tween.duration = duration;
        tween.delay = delay;
        tween.ResetToBeginning();
        tween.PlayForward();
    }

    public static void PicMove(int id, float xPercent, float yPercent, float delay, float duration)
    {
        PicMove(GetPic(id), xPercent, yPercent, delay, duration);
    }

    public static void PicZoom(GameObject pic, float scale, float delay, float duration)
    {
        if (pic == null)
            return;

        TweenScale tween = pic.GetOrAddComponent<TweenScale>();
        tween.from = pic.transform.localScale;
        tween.to = new Vector3(scale, scale, pic.transform.localScale.z);
        tween.duration = duration;
        tween.delay = delay;
        tween.ResetToBeginning();
        tween.PlayForward();
    }

    public static void PicZoom(int id, float scale, float delay, float duration)
    {
        PicZoom(GetPic(id), scale, delay, duration);
    }

    public static void PicFade(GameObject pic, float alpha, float delay, float duration)
    {
        if (pic == null)
            return;

        UIWidget widget = pic.GetComponentInChildren<UIWidget>();

        if (widget == null)
            return;

        TweenAlpha tween = widget.gameObject.GetOrAddComponent<TweenAlpha>();
        tween.from = widget.alpha;
        tween.to = alpha;
        tween.duration = duration;
        tween.delay = delay;
        tween.ResetToBeginning();
        tween.PlayForward();
    }

    public static void PicFade(int id, float alpha, float delay, float duration)
    {
        PicFade(GetPic(id), alpha, delay, duration);
    }

    public static float GetLocalWidth()
    {
        return (float)Screen.width / Screen.height * GetManualHeight() / GetLocalScale().x;
    }

    public static float GetLocalHeight()
    {
        return 640f / GetLocalScale().y;
    }

    private static Vector3 GetLocalScale()
    {
        if (localScale.sqrMagnitude < 1e-6)
        {
            GameObject center = GameCommon.FindUI("CenterAnchor");

            if (center != null)
            {
                localScale = center.transform.localScale;
                return localScale;
            }
            return Vector3.one;
        }

        return localScale;
    }

    private static float GetManualHeight()
    {
        if (manualHeight < 0.001f)
        {
            GameObject root = GameObject.Find(CommonParam.UIRootName);

            if (root != null)
            {
                UIRoot uiRoot = root.GetComponent<UIRoot>();

                if (uiRoot != null)
                {
                    manualHeight = uiRoot.manualHeight;
                    return manualHeight;
                }
            }

            return 640f;
        }

        return manualHeight;
    }

    public static BaseObject CreateModel(int index)
    {
        ActiveObject obj = new ActiveObject();
        obj.mConfigIndex = index;
        obj.mConfigRecord = DataCenter.mActiveConfigTable.GetRecord(index);
        obj.mbIsMainUI = true;
        obj.mbIsUI = true;
        obj.mMainObject = new GameObject("_role_");
        obj.CreateMainObject((int)obj.mConfigRecord.get("MODEL"));
        obj.SetMainObject(obj.mMainObject);
        return obj;
    }

    public static BaseObject ShowModel(int index, GameObject uiPoint, float scale)
    {
        if(uiPoint == null)
            return null;

        uiPoint.SetActive(false);

        BaseObject obj = CreateModel(index);

        if (obj == null)
            return null;

        float originScale = obj.mConfigRecord.get("UI_SCALE") * 100f;
        obj.mMainObject.transform.parent = uiPoint.transform.parent;
        obj.SetPosition(uiPoint.transform.position);
        obj.mMainObject.transform.localScale = Vector3.one * originScale * scale;
        obj.mMainObject.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        obj.SetVisible(true);
        obj.OnIdle();
        GameCommon.SetLayer(obj.mMainObject, CommonParam.UILayer);
        return obj;
    }

    public static void SetModelScale(BaseObject model, float scale)
    {
        if (model != null && model.mMainObject != null && model.mConfigRecord != null)
        {
            float originScale = model.mConfigRecord.get("UI_SCALE") * 100f;
            model.mMainObject.transform.localScale = Vector3.one * originScale * scale;
        }
    }

    public static void DestroyModel(BaseObject obj)
    {
        if (obj != null && obj.mMainObject != null)
        {
            GameObject.Destroy(obj.mMainObject);
            obj.mMainObject = null;
        }
    }
}