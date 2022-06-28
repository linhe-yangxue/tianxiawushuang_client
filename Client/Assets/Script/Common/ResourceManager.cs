using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Game
{
    public abstract class tResourceManager
    {
        // resource config file, game start load from run path, then 
        public abstract bool LoadConfig(string configName);

        public abstract Resource GetResource(string resourceName);

        public abstract Resource GetResource(string resourceName, string type);
    }


    public class ResourceManager : tResourceManager
    {
        static public ResourceManager Self = new ResourceManager();

        public Dictionary<string, AssetBundle> mModelsBundlesList;

        ResourceManager()
        {

        }
        // resource config file, game start load from run path, then 
        public override bool LoadConfig(string configName) { return true; }

        public override Resource GetResource(string resourceName)
        {
            return (Resource)null;
        }

        public override Resource GetResource(string resourceName, string type)
        {
            Resource resource = null;
            switch (type)
            {
                case "file":
                case "File":
                case "FILE":
                    {
                        resource = new FileResource();
                        resource.SetResourceName(resourceName);
                    }
                    break;

                case "unity":
                case "Unity":
                case "UNITY":
                {
                    resource = new UnityResource();
                    resource.SetResourceName(resourceName);
                }
                break;
            }

            return resource;
        }

        bool LoadBundle(string bundleFileName)
        {
//            string path = "window/";
//#if UNITY_IPHONE
//            path = "ios/";
//#elif UNITY_ANDROID
//            path = "android";
//#endif           
            string bundleName = GameCommon.MakeGamePathFileName(bundleFileName);
            AssetBundle bundle = AssetBundle.CreateFromFile(bundleName);
            if (bundle != null)
			{
                mModelsBundlesList[bundleFileName] = bundle;
				return true;
			}
			return false;
        }

        AssetBundle GetBundleAndTryLoad(string bundleName)
        {
            AssetBundle bundle = null;

            if (mModelsBundlesList.TryGetValue(bundleName, out bundle))
            {
                return bundle;
            }

            if (LoadBundle(bundleName))
            {
                if (mModelsBundlesList.TryGetValue(bundleName, out bundle))
                {
                    return bundle;
                }
            }

            return null;
        }

        UnityEngine.Object LoadFromBudle(string bundleName, string resName)
        {
            string fileName = Path.GetFileNameWithoutExtension(resName);
            AssetBundle bundle = GetBundleAndTryLoad(bundleName);

            if (bundle != null)
                return bundle.Load(fileName);

            return null;
        }

        bool LoadLevelFromBundle(string bundleName, string levelname)
        {
            AssetBundle bundle = GetBundleAndTryLoad(bundleName);

            if (bundle != null)
            {
                Application.LoadLevel(levelname);
                return true;

            }

            return false;
        }

    }

    
}
