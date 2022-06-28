using UnityEngine;
using System;
using System.Collections.Generic;
namespace Asset
{
    public class SceneAsset
    {
        public static IAssetRequest LoadScene(string path, bool add, TaskHandle complete)
        {
            DEBUG.Log("LoadScene(string name, bool add):" + path + ":" + add);
            SceneAssetItem tsk = new SceneAssetItem(path, add, complete);
            tsk.RootCompleteHandle += sceneRootComplete;
            tsk.CompleteHandle += sceneAssetComplete;
            loadList.Push(tsk);
            if (Current == null || Current.ProcessState == 0)
            {
                Current = loadList.Pop();
                Current.StartLoad();
            }
            return tsk;
        }
        private static void sceneRootComplete()
        {
            if (Last != null && !Last.IsAdd) Last.Distory();
        }
        private static void sceneAssetComplete()
        {
            Last = Current;
            if (loadList.Count > 0)
            {
                Current = loadList.Pop();
                Current.StartLoad();
            }
            else
            {
                Current = null;
            }
        }
        public static void DontDestroyOnLoad(GameObject g)
        {
            int rootId = g.transform.root.gameObject.GetInstanceID();
            if (!RootList.ContainsKey(rootId))
            {
                RootList[rootId] = g.transform.root.gameObject;
            }
        }
        public static bool CheckDestroyOnLoad(GameObject g)
        {
            if (g == null) return true;
            int rootId = g.transform.root.gameObject.GetInstanceID();
            return RootList.ContainsKey(rootId);
        }
        public static void AddTask(AssetRecordRoot assetHandle)
        {
            if (Current != null) Current.AddTask(assetHandle);
        }

        public static float Process
        {
            get
            {
                if (Current != null) return Current.Process;
                return 0;
            }
        }
        public static Dictionary<int, GameObject> RootList = new Dictionary<int, GameObject>();
        private static SceneAssetItem Current = null, Last = null;
        private static Stack<SceneAssetItem> loadList = new Stack<SceneAssetItem>();
    }
    public class SceneAssetItem : IAssetRequest
    {
        public SceneAssetItem(string path, bool add, TaskHandle complete)
        {
            IsAdd = add;
            Path = path;
            sceneTaskList.Clear();
            taskIndex = 0;
            ProcessState = 1;
            CompleteHandle = complete;
        }
        public void StartLoad()
        {
            sceneRequest = LoadHelp.LoadObject(Path, loadSceneComplete);
            startTime = Time.realtimeSinceStartup;
            //DEBUG.Log("LoadScene(string name, bool add):" + Path + ":" + IsAdd);
        }
        private void loadSceneComplete(AssetRequest req)
        {
            ProcessState = 2;
            if (RootCompleteHandle != null) RootCompleteHandle();
            foreach (KeyValuePair<int, GameObject> i in SceneAsset.RootList)
            {
                GameObject.DontDestroyOnLoad(i.Value);
            }
            string sceneName = Path.Substring(Path.IndexOf("/") + 1);
            sceneName = sceneName.Substring(0, sceneName.Length - 2); 
            //DEBUG.Log(">>>>>>>>>>>>>>>LoadScene(string name, bool add):" + sceneName + ":" + IsAdd);
            req.LoadScene(sceneName, IsAdd);
        }
        public void AddTask(AssetRecordRoot assetHandle)
        {
            if (assetHandle.Init == 1)
            {
                if (assetHandle.Objects != null && assetHandle.Objects.Length > 0)
                {
                    for (int i = 0; i < assetHandle.Objects.Length; i++)
                    {
                        AssetItem item = new AssetItem(assetHandle.Objects[i], assetHandle.Records[i]);
                        item.CompleteHandle += sceneAssetItemComplete;
                        sceneTaskList.Add(item);
                    }
                }
                else
                {
                    sceneAssetItemComplete();
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
        private void sceneAssetItemComplete()
        {
            taskIndex++;
            if (taskIndex >= sceneTaskList.Count)
            {
                ProcessState = 3;
                AssetItem item = null;
                List<GameObject> combineList = new List<GameObject>();
                for (int i = 0; i < sceneTaskList.Count; i++)
                {
                    item = sceneTaskList[i];
                    item.SetDurationScripts();
                    if (item.CanCombine)
                    {
                        combineList.Add(item.Handle.gameObject);
                    }
                }
                if (AssetManager.MeshCombine)
                {
                    if (combineList.Count > 1)
                    {
                        GameObject g = new GameObject("Scene");
                        StaticBatchingUtility.Combine(combineList.ToArray(), g);
                        for (int i = 0; i < sceneTaskList.Count; i++)
                        {
                            sceneTaskList[i].DeleteMesh();
                        }
                    }
                }
                string skyboxName = Path.Substring(0, Path.IndexOf(".")) + "_skybox";
                skyboxName = skyboxName.Substring(skyboxName.IndexOf("/") + 1);
                GameObject skybox = GameObject.Find(skyboxName);

                if (skybox != null)
                {
                    if (skybox.renderer != null) RenderSettings.skybox = skybox.renderer.sharedMaterials[0];
                    if (AssetManager.DeleteAssetRecord) GameObject.DestroyImmediate(skybox);
                    else skybox.SetActive(false);
                }
                startTime = Time.realtimeSinceStartup - startTime;
                DEBUG.Log("load:" + Path + ",time:" + startTime); 
                if (CompleteHandle != null) CompleteHandle();
            }
        }
        public float Process
        {
            get
            {
                process = 0;
                if (ProcessState == 1)
                {
                    if (sceneRequest != null)
                    {
                        process = sceneRequest.Process;
                    }
                    process = process * loadSceneRate;
                }
                else if (ProcessState == 2)
                {
                    if (sceneTaskList.Count != 0)
                    {
                        for (int i = 0; i < sceneTaskList.Count; i++)
                        {
                            process += sceneTaskList[i].Process;
                        }
                        process = process / sceneTaskList.Count;
                    }
                    //process = process * (1 - loadSceneRate) + loadSceneRate;
                    process = process * (1 - loadSceneRate) + loadSceneRate - .05f;
                }
                else if (ProcessState == 3)
                {
                    process = 1;
                }
                return process;
            }
        }
        public void Distory()
        {
            //if (!IsAdd)
            {
                if (RenderSettings.skybox != null) AssetItem.DeleteMaterial(RenderSettings.skybox.name);
                AssetItem tmp = null;
                for (int i = 0; i < sceneTaskList.Count; i++)
                {
                    tmp = sceneTaskList[i];
                    if (tmp.Handle == null)
                    {
                        tmp.Dispose();
                        //DEBUG.Log("--------------------------------------- private static void distory()>>>" + tmp.RootName);
                        sceneTaskList.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        int rootId = tmp.Handle.transform.root.gameObject.GetInstanceID();
                        if (!SceneAsset.RootList.ContainsKey(rootId))
                        {
                            tmp.Dispose();
                            sceneTaskList.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            DEBUG.Log("--------------------------------------- private static void distory():" + tmp.Handle.transform.root.name);
                        }
                    }
                }
                LoadHelp.DeleteObject(sceneRequest.Path);
            }
            /*        
   AssetItem tmp = null;
   for (int i = 0; i < sceneTaskList.Count; i++)
   {
       tmp = sceneTaskList[i];
       tmp.Dispose();
       sceneTaskList.RemoveAt(i);
   }
            
             */
            //if (ScenePath == "Scenes/Titles_export.s") LoadHelp.LogState();
            taskIndex = 0;
            ProcessState = 0;
        }
        public string Path
        {
            set { path = value; }
            get { return path; }
        }
        private string path = null;
        public byte ProcessState = 0;
        public bool IsAdd = false;
        public TaskHandle RootCompleteHandle = null;
        public TaskHandle CompleteHandle = null;
        private int taskIndex = 0;
        private List<AssetItem> sceneTaskList = new List<AssetItem>();
        private float process = 0, loadSceneRate = .3f;
        private IAssetRequest sceneRequest = null;
        private float startTime = 0;


        public UnityEngine.Object mainAsset
        {
            get { return null; }
        }
    }
}