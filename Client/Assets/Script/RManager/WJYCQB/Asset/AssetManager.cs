using UnityEngine;
using System;
using System.Collections.Generic;
using RManager.AssetLoader;
namespace Asset
{
    public class AssetManager
    {
        /* config */
        public const bool MeshCombine = true;
        public const bool DeleteAssetRecord = true;

        public static void Init(MonoBehaviour mono)
        {
            LoadHelp.Init(mono);
            AssetRecordRoot.AddTaskHandle = addTask;
            TextAsset sourceList = Resources.Load("Material/Static") as TextAsset;
            if (sourceList != null)
            {
                List<string> strList = AssetItem.GetStringList(sourceList.text);
                List<UnityEngine.Object> list = new List<UnityEngine.Object>();
                if (strList.Count > 0)
                {
                    UnityEngine.Object tmp = null;
                    for (int i = 0; i < strList.Count; i++)
                    {
                        tmp = Resources.Load(string.Concat("Material/", strList[i]));
                        if (tmp != null) list.Add(tmp);
                    }
                }
                AssetItem.AddStatic(list.ToArray());
            }
        }

        public static void Invoke(int frame, TaskHandle task)
        {
            invokeList.Add(new InvokeItem(1, frame, task));
        }
        public class InvokeItem
        {
            public InvokeItem(byte _type, float _frameAll, TaskHandle _delayTask)
            {
                type = _type;
                frameAll = _frameAll;
                delayTask = _delayTask;
                IsDone = false;
                frameIndex = 0;
            }
            float frameIndex, frameAll;
            byte type;
            TaskHandle delayTask;
            public bool IsDone;
            public void Update()
            {
                switch (type)
                {
                    case 1:
                        {
                            frameIndex ++;
                            break;
                        }
                    case 2:
                        {
                            frameIndex += Time.deltaTime;
                            break;
                        }
                }

                if (frameIndex >= frameAll)
                {
                    if (delayTask != null) delayTask();
                    IsDone = true;
                }
            }
        }
        public static void Invoke(float delay, TaskHandle task)
        {
            invokeList.Add(new InvokeItem(2, delay, task));
        }
        static List<InvokeItem> invokeList = new List<InvokeItem>();
        static InvokeItem item;
        static void invokeUpdate()
        {
            for(int i = 0; i < invokeList.Count; i++)
            {
                item = invokeList[i];
                item.Update();
                if (item.IsDone)
                {
                    invokeList.RemoveAt(i);
                    i--;
                }

            }
        }
        public static void Update()
        {
            invokeUpdate();
            LoadHelp.Update();
        }
        public static IAssetRequest LoadScene(string name, bool add, TaskHandle complete)
        {
            return SceneAsset.LoadScene("Scenes/" + name + "_export.s", add, complete); 
        }
        public static IAssetRequest LoadObject(string path, Action<UnityEngine.Object> complete)
        {
            string[] paras = path.Split(new char[]{'/'});
            if (paras == null || paras.Length < 2) return null;                    
            switch (paras[0])
            {
                case "Icon":
                case "Images":
                    {
                        path = string.Concat("Images/", paras[1], ".t"); 
                        return LoadHelp.LoadObject(path, complete);
                    }
                case "Meshs":
                    {
                        path = string.Concat(path, ".m"); 
                        break;
                    }
                case "Sounds":
                    {
                        path = string.Concat(path, ".a"); 
                        return LoadHelp.LoadObject(path, complete);
                    }
                case "db":
                case "Config":
                    {
                        path = string.Concat("Config/", paras[1], ".xml"); 
                        break;
                    }
                case "Atlas":
                case "Skill":
                case "CutScene":
                case "Prefabs":
                    {
                        path = string.Concat("Prefabs/", paras[1], ".p"); 
                        return PrefabAsset.LoadPrefab(path, complete);
                    }
            }
            return null;
        }
        public static void DestroyAllPrefabs()
        {
            PrefabAsset.DestroyAllPrefabs();
        }
        public static void DontDestroyOnLoad(GameObject g)
        {
            SceneAsset.DontDestroyOnLoad(g);
        }
        public static void DeleteObject(string path)
        {
            string[] paras = path.Split(new char[] { '/' });
            if (paras == null || paras.Length < 2) return;
            switch (paras[0])
            {
                case "Images":
                    {
                        LoadHelp.DeleteObject(string.Concat("Images/", paras[1]));
                        break;
                    }
                case "Meshs":
                    {
                        break;
                    }
                case "Sounds":
                    {
                        LoadHelp.DeleteObject(string.Concat("Sounds/", paras[1]));
                        break;
                    }
                case "db":
                case "Config":
                    {
                        path = string.Concat("Config/", paras[1]);
                        break;
                    }
                case "Atlas":
                case "Skill":
                case "CutScene":
                case "Prefabs":
                    {
                        path = string.Concat("Prefabs/", paras[1]);
                        PrefabAsset.DeletePrefab(path);
                        break;
                    }
            }
        }
        private static void addTask(AssetRecordRoot assetHandle)
        {
            if (string.Compare(assetHandle.Type, "s") == 0)
            {
                SceneAsset.AddTask(assetHandle);
                BundleAssetManager.OnSceneRecordRootState(assetHandle);
            }
            else
            {
                PrefabAsset.AddTask(assetHandle);
            }
        }
    } 
}