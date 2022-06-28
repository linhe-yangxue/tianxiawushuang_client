using UnityEngine;
using System;
using System.Collections.Generic;
namespace Asset
{
    public class PrefabAsset
    {
        public static IAssetRequest LoadPrefab(string path, Action<UnityEngine.Object> complete)
        {
            PrefabAssetItem item = null;
            if (objectList.ContainsKey(path))
            {
                item = objectList[path];
                item.AddCallBack(complete);
            }
            else
            {
                item = new PrefabAssetItem(path);
                item.AddCallBack(complete);
                item.RootRequest = LoadHelp.LoadObject(item.Path, delegate(AssetRequest req)
                {
                    item.ProcessState = 1;
                    if (req.mainAsset == null)
                    {
                        complete(null);
                    }
                    else
                    {
                        item.SetObject(GameObject.Instantiate(req.mainAsset) as GameObject);
                    }
                });
                objectList[path] = item;
            }
            item.UsedCount++;
            return item;
        }
        public static void DeletePrefab(string path)
        {
            PrefabAssetItem item = null;
            if (objectList.ContainsKey(path))
            {
                item = objectList[path];
                item.UsedCount--;
                if (item.UsedCount <= 0)
                {
                    objectList.Remove(path);
                    item.Dispose();
                }
            }
        }
        public static void AddTask(AssetRecordRoot assetHandle)
        {
            if(assetHandle == null)  return;
            string key = assetHandle.transform.root.name;
            if (objectList.ContainsKey(key))
            {
                PrefabAssetItem item = objectList[key];
                item.AddTask(assetHandle);
            }
        }
        public static GameObject InitObject(UnityEngine.Object o)
        {
            GameObject mod = GameObject.Instantiate(o) as GameObject;
            if (objectList.ContainsKey(o.name))
            {
                mod.SetActive(objectList[o.name].ActiveSelf);
            }
            return mod;
        }
        public static void SetObject(Action<Transform> set, Transform mod)
        {
            foreach (Transform child in mod)
            {
                SetObject(set, child);
            }
            set(mod);
        }
        public static void DestroyAllPrefabs()
        {
            List<string> deList = new List<string>();
            foreach (KeyValuePair<string, PrefabAssetItem> k in objectList)
            {
                if (!SceneAsset.CheckDestroyOnLoad(k.Value.Obj))
                {
                    k.Value.Dispose();
                    deList.Add(k.Key);
                }
            }
            foreach (string k in deList)
            {
                objectList.Remove(k);
            }
        }
        private static Dictionary<string, PrefabAssetItem> objectList = new Dictionary<string, PrefabAssetItem>();
    }
    public class PrefabAssetItem : IAssetRequest
    {
        public PrefabAssetItem(string p)
        {
            path = p;
        }
        public GameObject Obj = null;
        public IAssetRequest RootRequest = null;
        public int ProcessState = 0;
        private string path = null;
        private byte countState = 0;
        private int taskIndex = 0;
        private bool enable = false;
        private List<AssetItem> sceneTaskList = new List<AssetItem>();
        private static float process = 0, loadSceneRate = .3f;
        public void AddTask(AssetRecordRoot assetHandle)
        {
            //prefab only OnStart is enable, Awake is exc on GameObject.Instantiate;
            if (assetHandle.Init == 1)
            {
                if (assetHandle.Objects != null && assetHandle.Objects.Length > 0)
                {
                    for (int i = 0; i < assetHandle.Objects.Length; i++)
                    {
                        AssetItem item = new AssetItem(assetHandle.Objects[i], assetHandle.Records[i]);
                        item.CompleteHandle += itemComplete;
                        item.StateHandle += GameObjectState;
                        sceneTaskList.Add(item);
                    }
                }
                else
                {
                    itemComplete();
                }
            }
            else if (assetHandle.Init == 2)
            {
                for (int i = 0; i < sceneTaskList.Count; i++)
                {
                    sceneTaskList[i].StartLoad();
                }
            }
        }
        public float Process
        {
            get
            {
                process = 0;
                if (ProcessState == 0)
                {
                    if (RootRequest != null)
                    {
                        process = RootRequest.Process;
                    }
                    process = process * loadSceneRate;
                }
                else if (ProcessState == 1)
                {
                    if (sceneTaskList.Count != 0)
                    {
                        for (int i = 0; i < sceneTaskList.Count; i++)
                        {
                            process += sceneTaskList[i].Process;
                        }
                        process = process / sceneTaskList.Count;
                    }
                    process = process * (1 - loadSceneRate) + loadSceneRate;
                }
                else
                {
                    process = 1;
                }
                return process;
            }
        }
        private void itemComplete()
        {
            taskIndex++;
            if (taskIndex >= sceneTaskList.Count)
            {
                ProcessState = 2;
                AssetItem item = null;
                for (int i = 0; i < sceneTaskList.Count; i++)
                {
                    item = sceneTaskList[i];
                    item.SetDurationScripts();
                }
                if (AssetManager.DeleteAssetRecord) GameObject.DestroyImmediate(Obj.GetComponent<AssetRecordRoot>());
                while (callBackList.Count > 0)
                {
                    Action<UnityEngine.Object> callBack = callBackList.Pop();
                    callBack(Obj);
                }
            }
        }
        public void SetObject(GameObject g)
        {
            Obj = g;
            Obj.name = path;
            if (!Obj.activeSelf)
            {
                AssetRecordRoot record = Obj.GetComponent<AssetRecordRoot>();
                record.Init = 1;
                AddTask(record);
                record.Init = 2;
                AddTask(record);
            }
        }
        public void GameObjectState(GameObject g, bool s)
        {
            if (Obj != null && g != null && g.GetInstanceID() == Obj.GetInstanceID())
            {
                ActiveSelf = s;
            }
        }
        public string Path
        {
            get { return path; }
        }
        public void Dispose()
        {
            for (int i = 0; i < sceneTaskList.Count; i++)
            {
                sceneTaskList[i].Dispose();
            }
            sceneTaskList.Clear();
            GameObject.DestroyImmediate(Obj);
            LoadHelp.DeleteObject(RootRequest.Path);
            //Resources.UnloadUnusedAssets();
        }
        private Stack<Action<UnityEngine.Object>> callBackList = new Stack<Action<UnityEngine.Object>>();
        public void AddCallBack(Action<UnityEngine.Object> callBack)
        {
            if (ProcessState > 1)
            {
                callBack(Obj);
            }
            else
            {
                callBackList.Push(callBack);
            }
        }
        public bool ActiveSelf = false;
        private int useCount = 0;
        public int UsedCount
        {
            get
            {
                return useCount;
            }
            set
            {
                useCount = value;
            }
        }

        public UnityEngine.Object mainAsset
        {
            get { return Obj; }
        }
    }
}