using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using DataTable;

public enum OBJECT_TYPE
{
    CHARATOR,
    MONSTER,
	PET,
    MONSTER_BOSS,
    BIG_BOSS,
	PVP_ACTIVE,
	OPPONENT_PET,
	OPPONENT_CHARACTER
}


public class ObjectManager
{	
	static public ObjectManager Self = new ObjectManager();
    static public int nameCode = 0;
		
    public ObjectManager()
    {
        
    }

    public void RegisterObjectFactory(ObjectFactory tFactory)
    {
        mObjectFactoryMap[tFactory.GetObjectType()] = tFactory;
        //tFactory.LoadConfigTable();
    }

    public BaseObject NewObject(OBJECT_TYPE type)
    {
        BaseObject newObj = null;
        ObjectFactory objectFactory;
        if (mObjectFactoryMap.TryGetValue(type, out objectFactory))
        {
            newObj = objectFactory.NewObject();
            newObj.SetFactory(objectFactory);            
        }
        else
        {
            Logic.EventCenter.Self.Log("Error: no exist object factory >>>" + type.ToString());            
        }
        return newObj;
    }
	public BaseObject CreateObject(OBJECT_TYPE type, int configIndex)
	{
		return CreateObject(type, configIndex, false, false);
    }
	public BaseObject CreateObject(OBJECT_TYPE type, int configIndex, bool bIsUI, bool bIsMainUI)
	{
		BaseObject newObj = NewObject(type);
		
		if (null != newObj)
		{
			newObj.mbIsMainUI = bIsMainUI;
			newObj.mbIsUI = bIsUI;
            AppendObject(newObj);

            if (newObj.Init(configIndex))
            {
                if (!bIsUI && !bIsMainUI)
                {
                    newObj.SetAttributeByLevel();
                }
                AppendAureole(newObj);
            }
            else 
            {
                newObj.OnDestroy();
            }

            return newObj;
		}

		return null;
	}
	public BaseObject CreateObject(int configIndex)
	{
		return CreateObject(configIndex, false, false);
	}
	
	public BaseObject CreateObject(int configIndex, bool bIsUI, bool bIsMainUI)
	{

		OBJECT_TYPE type = GetObjectType(configIndex);
		
		return CreateObject(type, configIndex, bIsUI, bIsMainUI);
	}

    /// <summary>
    /// 添加脚底光环
    /// </summary>
    /// <returns></returns>
    private void AppendAureole(BaseObject kBaseObj)
    {
        if (kBaseObj == null || kBaseObj.mAureoleObj != null || !(kBaseObj is Role))
            return;

        //获得符灵界面不出现光环
        object _petGainWinObj = DataCenter.Self.getObject("PET_GAIN_WINDOW");

        if (_petGainWinObj != null)
        {
            tWindow _tWin = _petGainWinObj as tWindow;
            if (_tWin != null && _tWin.mGameObjUI != null && _tWin.mGameObjUI.activeInHierarchy)
            {
                return;
            }
        }

        //冒险界面主角脚底下不生成
        if (OBJECT_TYPE.CHARATOR == GetObjectType(kBaseObj.mConfigIndex))
        {
            object _worldMapObj = DataCenter.GetData("SCROLL_WORLD_MAP_WINDOW");
            if (_worldMapObj != null)
            {
                tWindow _tWin = _worldMapObj as tWindow;
                if (_tWin.IsOpen())
                {
                    return;
                }
            }
        }

        int _petQuality = GameCommon.GetItemQuality(kBaseObj.mConfigIndex);
        DataRecord qualityConfig = DataCenter.mQualityConfig.GetRecord(_petQuality);

        if (qualityConfig == null)
            return;

        string _effectName = "";
        float _rotateX = 0.0f;

        // 在主界面
        if (kBaseObj.mbIsMainUI && kBaseObj.mbIsUI)
        {
            _effectName = qualityConfig["MAIN_UI_AUREOLE"];
            _rotateX = qualityConfig["ROTATE_MAIN_UI"];
        }
        // 非主界面UI
        else if (!kBaseObj.mbIsMainUI && kBaseObj.mbIsUI)
        {
            _effectName = qualityConfig["UI_AUREOLE"];
            _rotateX = qualityConfig["ROTATE_UI"];
        }
        // 战斗界面
        else if (!kBaseObj.mbIsMainUI && !kBaseObj.mbIsUI)
        {
            _effectName = qualityConfig["FIGHT_UI_AUREOLE"];
            _rotateX = qualityConfig["ROTATE_FIGHT_UI"];
        }

        if (string.IsNullOrEmpty(_effectName) || _effectName == "0")
            return;

        GameObject _aureoleObj = GameCommon.LoadAndIntanciateEffectPrefabs("EFFECT/UIEffect/" + _effectName);

        if (_aureoleObj == null)
            return;

        kBaseObj.mAureoleObj = _aureoleObj;
        _aureoleObj.transform.parent = kBaseObj.mBodyObject.transform;
        _aureoleObj.transform.localPosition = Vector3.zero;
        _aureoleObj.transform.parent = kBaseObj.mMainObject.transform;
        _aureoleObj.transform.eulerAngles = new Vector3(_rotateX, 0f, 0f);

        if (kBaseObj is ActiveObject)
            ((ActiveObject)kBaseObj).mQualityShadow = _aureoleObj;

        if (GlobalModule.Instance != null)
        {
            GlobalModule.DoCoroutine(__IE_SetAureolePos(kBaseObj, _aureoleObj/*, _scale*/));
        }
    }

    private IEnumerator __IE_SetAureolePos(BaseObject kBaseObj, GameObject kAureoleObj/*, float kScaleFactor*/) 
    {
        yield return null;
        if (kBaseObj.mbIsMainUI || kBaseObj.mbIsUI)
        {
            if (kAureoleObj != null) 
            {
                kAureoleObj.transform.parent = kBaseObj.mMainObject.transform.parent;
                Vector3 _originPos = kAureoleObj.transform.localPosition;
                Vector3 _bodyPos = kBaseObj.mBodyObject.transform.localPosition;
                float _scaleFactor = kAureoleObj.transform.localScale.x;
                kAureoleObj.transform.localPosition = new Vector3(_originPos.x + _bodyPos.x * _scaleFactor, _originPos.y + _bodyPos.y * _scaleFactor, _originPos.z + _bodyPos.z * _scaleFactor);
            }
        }
        //yield return null;
        //if (kAureoleObj == null)
        //    yield break;
        //ParticleScaler _pScaler = kAureoleObj.GetComponent<ParticleScaler>();
        //if (_pScaler != null) 
        //{
        //    _pScaler.particleScale = kScaleFactor;
        //}
    }

	public OBJECT_TYPE GetObjectType(int configIndex)
	{
		OBJECT_TYPE type = OBJECT_TYPE.MONSTER;
		string className = "";

        ITEM_TYPE itemType = PackageManager.GetItemTypeByTableID(configIndex);
        if (itemType == ITEM_TYPE.PET || itemType == ITEM_TYPE.CHARACTER)
        {
            className = TableManager.GetData("ActiveObject", configIndex, "CLASS");
        }
        else
        {
            className = TableManager.GetData("MonsterObject", configIndex, "CLASS");
        }
		
		switch (className)
		{
		case "CHAR":
			type = OBJECT_TYPE.CHARATOR;
			break;
			
		case "MONSTER":
			type = OBJECT_TYPE.MONSTER;
			break;
			
		case "PET":
			type = OBJECT_TYPE.PET;
			break;
			
		case "BOSS":
			type = OBJECT_TYPE.MONSTER_BOSS;
			break;
			
		case "BIG_BOSS":
			type = OBJECT_TYPE.BIG_BOSS;
			break;
			
		default:
			type = OBJECT_TYPE.MONSTER;
			break;
		}

		return type;
	}

    public DataRecord GetObjectConfig(int configIndex)
    {
        OBJECT_TYPE t = GetObjectType(configIndex);
        ObjectFactory objectFactory;

        if (mObjectFactoryMap.TryGetValue(t, out objectFactory))
        {
            return objectFactory.GetConfig(configIndex);
        }

        return null;
    }

	public void AppendObject(BaseObject newObj)
    {
        for( int i=0; i<mObjectMap.Length; ++i)
        {
            if (mObjectMap[i]==null)
            {
				newObj.SetID(i);
				mObjectMap[i] = newObj;
                return;
            }
        }
        int nowCount = mObjectMap.Length;
        Array.Resize<BaseObject>(ref mObjectMap, mObjectMap.Length * 2);
        newObj.SetID(nowCount);
        mObjectMap[nowCount] = newObj;
    }

    public BaseObject GetObject(int id)
    {
        if (id<mObjectMap.Length)
            return mObjectMap[id];

        return null;

        //BaseObject obj = null;
        //mObjectMap.TryGetValue(id, out obj);
        //return obj;
    }

    public bool DeleteObject(int id)
    {
        //return mObjectMap.Remove(id);
        if (id < mObjectMap.Length)
        {
            bool b = mObjectMap[id] != null;
            if (b)
                mObjectMap[id].Destroy();

            mObjectMap[id] = null;
            return b;
        }

        return false;
    }

    public bool DeleteObject(BaseObject obj)
    {
        if (obj!=null)
            return DeleteObject(obj.GetID());

        return false;
    }

    public void Update( float delyTime )
    {
        int len = mObjectMap.Length;

        for (int i = 0; i < len; ++i)
        {
            var obj = mObjectMap[i];

            if (obj == null)
                continue;

            obj.Update(delyTime);

            if (obj.NeedDestroy())
                mDestroyList.Add(obj);
        }

        len = mDestroyList.Count;

        if (len > 0)
        {
            for (int i = 0; i < len; ++i)
            {
                DeleteObject(mDestroyList[i]);
            }

            mDestroyList.Clear();
        }
    }

    public void ClearAll()
    {
        foreach (BaseObject kv in mObjectMap)
        {
            if (kv != null)
            {
                kv.Destroy();
                kv.OnDestroy();
            }
        }

        Array.Clear( mObjectMap, 0, mObjectMap.Length);
    }

    public BaseObject[] mObjectMap = new BaseObject[128];

    protected Dictionary<OBJECT_TYPE, ObjectFactory> mObjectFactoryMap = new Dictionary<OBJECT_TYPE, ObjectFactory>();
    protected List<BaseObject> mDestroyList = new List<BaseObject>();

    public BaseObject FindAlived(Predicate<BaseObject> match)
    {
        int len = mObjectMap.Length;

        for (int i = 0; i < len; ++i)
        {
            var obj = mObjectMap[i];

            if (obj != null && !obj.IsDead() && (match == null || match(obj)))
            {
                return obj;
            }
        }

        return null;
    }

    public BaseObject[] FindAllAlived(Predicate<BaseObject> match)
    {
        int len = mObjectMap.Length;
        List<BaseObject> result = new List<BaseObject>();

        for (int i = 0; i < len; ++i)
        {
            var obj = mObjectMap[i];

            if (obj != null && !obj.IsDead() && (match == null || match(obj)))
            {
                result.Add(obj);
            }
        }

        return result.ToArray();
    }

    public int CountAlived(Predicate<BaseObject> match)
    {
        int len = mObjectMap.Length;
        int count = 0;

        for (int i = 0; i < len; ++i)
        {
            var obj = mObjectMap[i];

            if (obj != null && !obj.IsDead() && (match == null || match(obj)))
            {
                ++count;
            }
        }

        return count;
    }

    public void ForEachAlived(Predicate<BaseObject> match, Action<BaseObject> action)
    {
        int len = mObjectMap.Length;

        for (int i = 0; i < len; ++i)
        {
            var obj = mObjectMap[i];

            if (obj != null && !obj.IsDead() && (match == null || match(obj)))
            {
                action(obj);
            }
        }
    }
}