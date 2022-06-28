using RManager;
using RManager.AssetLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngineInternal;

namespace UnityEngine
{

    //public class BundleLoadingInfo
    //{
    //    public AssetBundle mBundle;
    //    public bool        mIsLoading;
    //    public string      mBundleName;
    //}

    public class MResources
    {
        BundleAssetManager                      mMainAssetManager = null;

        static bool msIsShowLog = false;
        static bool msIsLoadFromBundle = false;

        //BundleLoader        mBundleLoader = null;
        string mResourceBase = null;
        IU3dCroutineHelper  mCoroutineHelper = null;

        public bool IsShowLog { get {return msIsShowLog; } set { msIsShowLog = value;}}
        public bool IsLoadFromBundle {get {return msIsLoadFromBundle;} set {msIsLoadFromBundle = value;}}
        public string ResourceBase 
        { 
            get{ return mResourceBase; } 
            set 
            { 
                mResourceBase = value;
            } 
        }

        public MResources() 
        {
            msIsLoadFromBundle = StaticDefine.isDynamicResources;
            mCoroutineHelper = null;

            // 在加载资源时， 确保 BundleAssetManager.AssetRoot 已设置好
            mMainAssetManager = new BundleAssetManager();
			//BundleAssetManager.AssetsRoot = Application.streamingAssetsPath;
        }

        public void SetU3dInterface(IU3dCroutineHelper coroutineHelper)
        {
            mCoroutineHelper = coroutineHelper;
        }

        public void ClearCache()
        {
            Caching.CleanCache();
        }

        void LogDebug(string log)
        {
            if(msIsShowLog)
                DEBUG.Log(log);
        }
        
        void LogError(string error)
        {
            DEBUG.LogError(error);
        }

        void LogWarn(string warnning)
        {
            DEBUG.Log(warnning);
            DEBUG.LogWarning(warnning);
        }

        public Texture LoadTexture(string resourcePath)
        {
            if(!msIsLoadFromBundle)
                return Resources.Load(resourcePath, typeof(Texture)) as Texture;
            var result = mMainAssetManager.LoadTexture(resourcePath);
            if(result == null)
                result = Resources.Load(resourcePath, typeof(Texture)) as Texture;
			//DEBUG.Log("LoadTexture:" + resourcePath);
            return result;
        }

        public UnityEngine.Object LoadModel(string resourcePath)
        {
            if(!msIsLoadFromBundle)
                return Resources.Load(resourcePath, typeof(UnityEngine.Object));

            var result = mMainAssetManager.LoadPrefab(resourcePath);
			//DEBUG.Log("LoadModel:" + resourcePath);
            //LogDebug(string.Format("Load model {0}; result: {1}", resourcePath, result));
            return result;
        }

        public RuntimeAnimatorController LoadController(string resourcePath)
        {
            if(!msIsLoadFromBundle)
                return Resources.Load(resourcePath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
            var result = mMainAssetManager.LoadController(resourcePath);
            //LogDebug(string.Format("Load controller {0}; result: {1}", resourcePath, result));
            return result;
        }

        public GameObject LoadPrefab(string resourcePath, string prefabHint)
        {
            if(!msIsLoadFromBundle)
                return Resources.Load(resourcePath, typeof(GameObject)) as GameObject;
			//DEBUG.Log("LoadPrefab:" + resourcePath);
            var result = mMainAssetManager.LoadPrefab(resourcePath);
            //LogDebug(string.Format("Load prefab {0}; result: {1}", resourcePath, result));
            return result;
        }

        public UIAtlas LoadUIAtlas(string resourcePath, string prefabHint)
        {
            if(!msIsLoadFromBundle)
                return Resources.Load(resourcePath, typeof(UIAtlas)) as UIAtlas;
			//DEBUG.Log("LoadUIAtlas:" + resourcePath);
            var result = mMainAssetManager.LoadUIAtlas(resourcePath);
            //LogDebug(string.Format("Load UIAtlas {0}; result: {1}", resourcePath, result));
            return result;
        }

        public AudioClip LoadSound(string resourcePath)
        {
			//DEBUG.Log("LoadSound:" + resourcePath);
            if(!msIsLoadFromBundle)
                return Resources.Load(resourcePath, typeof(AudioClip)) as AudioClip;
            return mMainAssetManager.LoadSound(resourcePath);
        }

        public Shader LoadShader(string shaderName)
        {
			//DEBUG.Log("LoadShader:" + shaderName);
            return Shader.Find(shaderName);
        }

        public void LoadLevel(string levelName)
        {
			//DEBUG.Log("LoadLevel:" + levelName);
            if (!msIsLoadFromBundle) 
                Application.LoadLevel(levelName);
            else
                BundleAssetManager.LoadScene(levelName);
        }

        public void LoadLevelAdditive(string levelName)
        {
			//DEBUG.Log("LoadLevelAdditive:" + levelName);
            if (!msIsLoadFromBundle)
                Application.LoadLevelAdditive(levelName);
            else
                BundleAssetManager.LoadScene(levelName, true);
        }

        public TextAsset LoadTextAsset(string resourcePath)
        {
			//DEBUG.Log("LoadTextAsset:" + resourcePath);
            if(!msIsLoadFromBundle)
                return Resources.Load(resourcePath, typeof(TextAsset)) as TextAsset;
            return mMainAssetManager.LoadTextAsset(resourcePath);
        }
    }
}
