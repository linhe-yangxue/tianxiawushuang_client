using Asset;
using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RManager.AssetLoader
{
    public enum GObjRequestType
    {
        GORQ_MESH = 0,
        GORQ_TEXTUR = 1,
        GORQ_AUTIO = 2,
        GORQ_OBJECT = 3
    }

    public interface IAssetRequiring
    {
        void CompleteMesh(UnityEngine.Object assetObj, string error, object rquestCtx);
        void CompleteTexture(UnityEngine.Object assetObj, string error, object rquestCtx);
        void CompleteAudio(UnityEngine.Object assetObj, string error, object rquestCtx);
        void CompleteObject(UnityEngine.Object assetObj, string error, object rquestCtx);
    }

    

    public class AssetLoadInfo
    {
        private List<Action<AssetLoadInfo, string>> requests;
        public string   assetPath   { get; set; }
        public BundleLoader      www { get; set; }
        public string   errorMsg    { get; private set;}

        public UnityEngine.Object mainAsset     {get; private set;}
        public byte[]             assetBytes    {get; private set;}
        public long               assetLenth    {get; private set;}

        public int                assetRefCount {get; private set;}

        public AssetLoadInfo()
        {
            requests = new List<Action<AssetLoadInfo, string>>();
            assetRefCount = 0;
            errorMsg = null;
        }

        /// <summary>
        /// 释放资源的引用
        /// </summary>
        /// <param name="isUnload">如果为true, 则当引用为零时，同时真正释放资源</param>
        public void ReleaseAsset(bool isUnload)
        {
            assetRefCount--;
            if(assetRefCount <= 0)
            {
                assetRefCount = 0;
                if(isUnload)
                    UnloadAsset();
            }
        }

        public void UnloadAsset()
        {
            if(mainAsset != null)
                GameObject.DestroyImmediate(mainAsset, true);
            assetLenth = 0;
            mainAsset  = null;
            assetBytes = null;
            assetRefCount = 0;
        }

        public void RequireAsset(IAssetRequiring rquest, object rquestCtx, GObjRequestType rqType)
        {
            switch (rqType)
            {
                case GObjRequestType.GORQ_MESH:
                    requests.Add((loadInfo, error) =>
                    {
                        rquest.CompleteMesh(mainAsset, error, rquestCtx);
                    });
                    break;
                case GObjRequestType.GORQ_TEXTUR:
                    requests.Add((loadInfo, error) =>
                    {
                        rquest.CompleteTexture(mainAsset, error, rquestCtx);
                    });
                    break;
                case GObjRequestType.GORQ_AUTIO:
                    requests.Add((loadInfo, error) =>
                    {
                        rquest.CompleteAudio(mainAsset, error, rquestCtx);
                    });
                    break;
                case GObjRequestType.GORQ_OBJECT:
                    requests.Add((loadInfo, error) =>
                    {
                        rquest.CompleteObject(mainAsset, error, rquestCtx);
                    });
                    break;
            }

            if (assetLenth == 0 && www == null)
            {
                //DEBUG.LogError(string.Format("Assets: load  asset bundle from {0} by www", assetPath));
                //www = new WWW(assetPath);
                www = new BundleLoader(BundleAssetManager.AssetsRootForWWW, assetPath, BundleFileLoader.USE_WWW);
            }

            assetRefCount++;
        }

        public void RequireAsset(Action<AssetLoadInfo, string> onComplete)
        {
            requests.Add(onComplete);
            if (assetLenth == 0 && www == null)
            {
                //DEBUG.LogError(string.Format("Assets: load  asset bundle from {0} by www", assetPath)); 
                www = new BundleLoader(BundleAssetManager.AssetsRootForWWW, assetPath, BundleFileLoader.USE_WWW);
            }

            assetRefCount++;
        }

        public bool LookAssetState(bool isForceWWWSync)
        {
            if(assetLenth > 0)
            {
                comleteLoad(null);
                return true;
            }

            if(errorMsg != null)
            {
                comleteLoad(errorMsg);
                return true;
            }

            if(www != null && www.isDone)
            {
                if(!string.IsNullOrEmpty(www.error))
                {
                    errorMsg = www.error;
                }
                else
                {
                    mainAsset = www.assetBundle.mainAsset;
                    assetBytes = www.bytes;
                    assetLenth = www.bytes.Length;
                    www.assetBundle.Unload(false);
                    errorMsg = null;
                }

                www.Dispose();
                www = null;

                comleteLoad(errorMsg);
                return true;
            }

            if(www != null)
                www.PeekState(isForceWWWSync);

            return false;
        }

        private void comleteLoad(string error)
        {
            foreach (var action in requests)
                action(this, error);

            requests.Clear();
        }
    }

    /// <summary>
    /// 在加载资源时， 确保 BundleAssetManager.AssetRoot 已设置好
    /// </summary>
    public class BundleAssetManager
    {
        public static BundleAssetManager DefaultInstance {get; private set;}

        static PrefabLikeGameObjectLoader SceneStaticLoader {get; set;}
        static Dictionary<string, AssetBundle>  StaticSceneBundle {get; set;}

        static List<PrefabLikeGameObjectLoader> AdditiveSceneStaticLoader {get; set;}
        
        static string                           msAssetsRoot;
        static string                           msAssetsRootForWWW;

        public static void OnSceneRecordRootState(AssetRecordRoot record)
        {
            SceneStaticLoader.OnRecordRootState(record);

            if(record.Init == 2 && StaticSceneBundle.Count > 0)
            {
                foreach(var assetBundle in StaticSceneBundle.Values)
                {
                    try
                    {
                        assetBundle.Unload(false);
                    }catch
                    {}
                }

                StaticSceneBundle.Clear();
            }
        }

        static BundleAssetManager()
        {
            msAssetsRoot = "";
            if(DefaultInstance == null)
            {
                DefaultInstance = new BundleAssetManager();
                SceneStaticLoader = new PrefabLikeGameObjectLoader(null, DefaultInstance);
                AdditiveSceneStaticLoader = new List<PrefabLikeGameObjectLoader>();
            }

            StaticSceneBundle = new Dictionary<string, AssetBundle>();
        }


        Dictionary<string, AssetLoadInfo>    mAllLoadedAsset;

        public BundleAssetManager()
        {
            mAllLoadedAsset = new Dictionary<string, AssetLoadInfo>();
        }

#region Static helpers
		 
        internal static string EnsurePathUsableForWWW(string path)
        {
            if(path == null || path.IndexOf("file://") >= 0)
                return path;

            return "file://" + path;
        }

        public static string AssetsRoot 
        { 
            get 
            { 
                return msAssetsRoot; 
            } 
            internal set 
            { 
                msAssetsRoot = value;
                if(string.IsNullOrEmpty(msAssetsRootForWWW))
                    msAssetsRootForWWW = EnsurePathUsableForWWW(msAssetsRoot);
            }
        }

        public static string AssetsRootForWWW
        {
            get { return msAssetsRootForWWW; }
            internal set { msAssetsRootForWWW = EnsurePathUsableForWWW(value); }
        }

        internal static string getPrefabAssetBundlePath(string assetName)
        {
            var result = assetName.IndexOf(".p") > 0 ? string.Format("Prefabs/{0}", assetName) : string.Format("Prefabs/{0}.p", assetName);

            return result.Replace(' ', '_');
        }

        internal static string getUIAtlasAssetBundlePath(string assetName)
        {
            return getPrefabAssetBundlePath(assetName);
        }

        internal static string getTextureAssetBundlePath(string assetName)
        {
            var result = assetName.IndexOf(".t") > 0 ? string.Format("Images/{0}", assetName) : string.Format("Images/{0}.t", assetName);
            return result.Replace(' ', '_');
        }

        internal static string getSoundAssetBundlePath(string assetName)
        {
            var result = assetName.IndexOf(".a") > 0 ? string.Format("Sounds/{0}", assetName) : string.Format("Sounds/{0}.a", assetName);
            return result.Replace(' ', '_');
        }

        internal static string getAnimationAssetBundlePath(string assetName)
        {
            var result = assetName.IndexOf(".ani") > 0 ? string.Format("Animations/{0}", assetName) : string.Format("Animations/{0}.ani", assetName);
            return result.Replace(' ', '_');
        }

        internal static string getMeshAssetBundlePath(string assetName)
        {
            var result = assetName.IndexOf(".m") > 0 ? string.Format("Meshs/{0}", assetName) : string.Format("Meshs/{0}.m", assetName);
            return result.Replace(' ', '_');
        }

        public static string GetPrefabAssetBundlePath(string resourcePath)
        {
            var assetName = Path.GetFileNameWithoutExtension(resourcePath);
            var result = string.Format("Prefabs/{0}.p", assetName);

            if (resourcePath.IndexOf('/') >= 0 || resourcePath.IndexOf('\\') >= 0)
            {
                var unityPath = AssetFileName.UniformResourceAssetPath(resourcePath);
                result = string.Format("Prefabs/{0}_{1}.p", assetName, unityPath.GetMD5());
            }

            return result.Replace(' ', '_');
        }

        public static string GetUIAtlasAssetBundlePath(string resourcePath)
        {
            return GetPrefabAssetBundlePath(resourcePath);
        }

        public static string GetTextureAssetBundlePath(string resourcePath)
        {
            var assetName = Path.GetFileNameWithoutExtension(resourcePath);
            var result = string.Format("Images/{0}.t", assetName);

            if (resourcePath.IndexOf('/') >= 0 || resourcePath.IndexOf('\\') >= 0)
            {
                var unityPath = AssetFileName.UniformResourceAssetPath(resourcePath);
                result = string.Format("Images/{0}_{1}.t", assetName, unityPath.GetMD5());
            }

            return result.Replace(' ', '_');
        }

        public static string GetTextAssetBundlePath(string resourcePath)
        {
            var assetName = Path.GetFileNameWithoutExtension(resourcePath);
            var result = string.Format("TextAssets/{0}.ta", assetName);

            if (resourcePath.IndexOf('/') >= 0 || resourcePath.IndexOf('\\') >= 0)
            {
                var unityPath = AssetFileName.UniformResourceAssetPath(resourcePath);
                result = string.Format("TextAssets/{0}_{1}.ta", assetName, unityPath.GetMD5());
            }

            return result.Replace(' ', '_');
        }

        public static string GetSoundAssetBundlePath(string resourcePath)
        {
            var assetName = Path.GetFileNameWithoutExtension(resourcePath);
            var result = string.Format("Sounds/{0}.a", assetName);

            if (resourcePath.IndexOf('/') >= 0 || resourcePath.IndexOf('\\') >= 0)
            {
                var unityPath = AssetFileName.UniformResourceAssetPath(resourcePath);
                result = string.Format("Sounds/{0}_{1}.a", assetName, unityPath.GetMD5());
            }

            return result.Replace(' ', '_');
        }

        public static string GetAnimationAssetBundlePath(string resourcePath)
        {
            var assetName = Path.GetFileNameWithoutExtension(resourcePath);
            var result = string.Format("Animations/{0}.ani", assetName);

            if (resourcePath.IndexOf('/') >= 0 || resourcePath.IndexOf('\\') >= 0)
            {
                var unityPath = AssetFileName.UniformResourceAssetPath(resourcePath);
                result = string.Format("Animations/{0}_{1}.ani", assetName, unityPath.GetMD5());
            }

            return result.Replace(' ', '_');
        }

        public static string GetControllerAssetBundlePath(string resourcePath)
        {
            var assetName = Path.GetFileNameWithoutExtension(resourcePath);
            var result = string.Format("Controllers/{0}.ctl", assetName);

            if (resourcePath.IndexOf('/') >= 0 || resourcePath.IndexOf('\\') >= 0)
            {
                var unityPath = AssetFileName.UniformResourceAssetPath(resourcePath);
                result = string.Format("Controllers/{0}_{1}.ctl", assetName, unityPath.GetMD5());
            }

            return result.Replace(' ', '_');
        }

        public static string GetSceneAssetBundlePath(string resourcePath)
        {
            var assetName = Path.GetFileNameWithoutExtension(resourcePath);
            var result = string.Format("Scenes/{0}_export.s", assetName);

            return result.Replace(' ', '_');
        }

        public static string GetMeshAssetBundlePath(string resourcePath)
        {
            var assetName = Path.GetFileNameWithoutExtension(resourcePath);
            var result = string.Format("Meshs/{0}.m", assetName);

            return result.Replace(' ', '_');
        }
#endregion

        public AssetLoadInfo GetAssetLoadInfo(string assetPath)
        {
            AssetLoadInfo loadInfo = null;
            if(mAllLoadedAsset.TryGetValue(assetPath, out loadInfo))
            {
                return loadInfo;
            }
            else
            {
                loadInfo = new AssetLoadInfo
                {
                    assetPath = assetPath
                };

                mAllLoadedAsset[assetPath] = loadInfo;
            }
            return loadInfo;
        }

        T LoadSimpleAsset<T>(string assetPath, string resourcePath)
            where T : UnityEngine.Object
        {
            T result = null;

            AssetLoadInfo load = GetAssetLoadInfo(assetPath);

            load.RequireAsset((loadInfo, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    DEBUG.LogError(string.Format("Asset: error loading asset resource {0} : {1}", resourcePath, error));
                    return;
                }

                result = loadInfo.mainAsset as T;
            });

            while (!load.LookAssetState(true))
                ;

            load.ReleaseAsset(false);

            return result;
        }

        public RuntimeAnimatorController LoadController(string namePath)
        {
            var path = GetControllerAssetBundlePath(namePath);
            return LoadSimpleAsset<RuntimeAnimatorController>(path, namePath);
        }

        public AudioClip LoadSound(string namePath)
        {
            var path = GetSoundAssetBundlePath(namePath);
            return LoadSimpleAsset<AudioClip>(path, namePath);
        }

        public AnimationClip LoadAnimation(string namePath)
        {
            var path = GetAnimationAssetBundlePath(namePath);
            return LoadSimpleAsset<AnimationClip>(path, namePath);
        }

        public Texture LoadTexture(string namePath)
        {
            var path = GetTextureAssetBundlePath(namePath);
            return LoadSimpleAsset<Texture>(path, namePath);
        }

        public UIAtlas LoadUIAtlas(string namePath)
        {
            var path = GetUIAtlasAssetBundlePath(namePath);

            return LoadSimpleAsset<UIAtlas>(path, namePath);
        }

        public TextAsset LoadTextAsset(string namePath)
        {
            var path = GetTextAssetBundlePath(namePath);
            return LoadSimpleAsset<TextAsset>(path, namePath);
        }

        public GameObject LoadPrefab(string namePath)
        {
            var path = GetPrefabAssetBundlePath(namePath);

            AssetLoadInfo load = GetAssetLoadInfo(path);

            GameObject result = null;

            load.RequireAsset((loadInfo, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    DEBUG.LogError(string.Format("Asset: error loading prefab {0} : {1}", namePath, error));
                    return;
                }

                result = loadInfo.mainAsset as GameObject;

                var record = result.GetComponent<AssetRecordRoot>();

                if (record != null)
                {
                    result.SetActive(true); //TODO
                    var prefabLoader = new PrefabLikeGameObjectLoader(record, this);
                    prefabLoader.LoadAndBuildObject();

                    prefabLoader.ReleaseAllRefAssets(false);
                }
            });

            while (!load.LookAssetState(true))
                ;
            load.ReleaseAsset(false);
            return result;
        }

        public IEnumerator LoadPrefabAsync(MonoBehaviour monoInterface, string namePath, Action<GameObject> callback)
        {
            var path = GetPrefabAssetBundlePath(namePath);

            AssetLoadInfo load = GetAssetLoadInfo(path);

            GameObject result = null;

            load.RequireAsset((loadInfo, error) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    DEBUG.LogError(string.Format("Asset: error loading prefab {0} : {1}", namePath, error));
                    return;
                }

                result = loadInfo.mainAsset as GameObject;
            });

            while (!load.LookAssetState(false))
                yield return null;

            if(result == null)
            {
                callback(null);
                load.ReleaseAsset(false);
                yield break;
            }

            var record = result.GetComponent<AssetRecordRoot>();

            if (record != null)
            {
                var prefabLoader = new PrefabLikeGameObjectLoader(record, this);
                yield return monoInterface.StartCoroutine(prefabLoader.LoadAndBuildObjectAsync());

                prefabLoader.ReleaseAllRefAssets(false);
            }

            callback(result);
            load.ReleaseAsset(false);
        }

        public static void LoadScene(string name)
        {
            LoadScene(name, false);
        }

        public static void LoadScene(string name, bool add)
        {
            var path = GetSceneAssetBundlePath(name);

            AssetLoadInfo load = DefaultInstance.GetAssetLoadInfo(path);

            if (!add)
            {
                // 释放上一个场景对资源的引用
                SceneStaticLoader.ReleaseAllRefAssets(false);
                if (AdditiveSceneStaticLoader.Count > 0)
                {
                    foreach (var item in AdditiveSceneStaticLoader)
                    {
                        try
                        {
                            item.ReleaseAllRefAssets(false);
                        }
                        catch
                        {
                            DEBUG.LogError("Asset: error releasing additive scene resource!!");
                        }
                    }

                    AdditiveSceneStaticLoader.Clear();
                }
            }
            else
            {
                AdditiveSceneStaticLoader.Add(SceneStaticLoader);
                SceneStaticLoader = new PrefabLikeGameObjectLoader(null, DefaultInstance);
            }

            AssetBundle assetBundle = null;
            if (!StaticSceneBundle.TryGetValue(name, out assetBundle))
            {
                load.RequireAsset((loadInfo, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        DEBUG.LogError(string.Format("Asset: error loading scene {0} : {1}", name, error));
                        return;
                    }

                    try
                    {
                        assetBundle = AssetBundle.CreateFromMemoryImmediate(loadInfo.assetBytes);
                    }
                    catch (Exception ex)
                    {
                        DEBUG.LogError(string.Format("Asset: error loading scene {0} : {1}", name, ex.Message));
                        return;
                    }

                    StaticSceneBundle.Add(name, assetBundle);
                });

                while (!load.LookAssetState(true))
                    ;

                load.ReleaseAsset(false);
            }

            if (!add)
                Application.LoadLevel(Path.GetFileNameWithoutExtension(path));
            else
                Application.LoadLevelAdditive(Path.GetFileNameWithoutExtension(path));
        }

        public static IEnumerator LoadSceneAsync(string name, bool add)
        {
            var path = GetSceneAssetBundlePath(name);

            AssetLoadInfo load = DefaultInstance.GetAssetLoadInfo(path);

            if (!add)
            {
                // 释放上一个场景对资源的引用
                SceneStaticLoader.ReleaseAllRefAssets(false);
                if (AdditiveSceneStaticLoader.Count > 0)
                {
                    foreach (var item in AdditiveSceneStaticLoader)
                    {
                        try
                        {
                            item.ReleaseAllRefAssets(false);
                        }
                        catch
                        {
                            DEBUG.LogError("Asset: error releasing additive scene resource!!");
                        }
                    }

                    AdditiveSceneStaticLoader.Clear();
                }
            }
            else
            {
                AdditiveSceneStaticLoader.Add(SceneStaticLoader);
                SceneStaticLoader = new PrefabLikeGameObjectLoader(null, DefaultInstance);
            }

            AssetBundle assetBundle = null;
            if (!StaticSceneBundle.TryGetValue(name, out assetBundle))
            {
                load.RequireAsset((loadInfo, error) =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        DEBUG.LogError(string.Format("Asset: error loading scene {0} : {1}", name, error));
                        return;
                    }

                    try
                    {
                        assetBundle = AssetBundle.CreateFromMemoryImmediate(loadInfo.assetBytes);
                    }
                    catch (Exception ex)
                    {
                        DEBUG.LogError(string.Format("Asset: error loading scene {0} : {1}", name, ex.Message));
                        return;
                    }

                    StaticSceneBundle.Add(name, assetBundle);
                });

                while (!load.LookAssetState(false))
                    yield return null;

                load.ReleaseAsset(false);
            }

            if (!add)
                Application.LoadLevel(Path.GetFileNameWithoutExtension(path));
            else
                Application.LoadLevelAdditive(Path.GetFileNameWithoutExtension(path));
        }

        /// <summary>
        /// 释放当前 BundleAssetManager 实例的所有资源
        /// 注意！！：A. 必须等待所有的 异步操作完成之后，再调用此函数，
        ///           否则，会造成资源泄漏 或者 异步加载的资源不完整
        ///          B. 调用此函数，会强制卸载底层资源，如果上层有引用，会出现问题 
        /// </summary>
        public void UnloadAllAssets()
        {
            foreach(var kv in mAllLoadedAsset)
            {
                kv.Value.UnloadAsset();
            }
        }

        public void UnloadAllUnuseAssets()
        {
            foreach(var kv in mAllLoadedAsset)
            {
                if(kv.Value.assetRefCount <= 0)
                    kv.Value.UnloadAsset();
            }
        }
    }
}
