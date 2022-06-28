using System;
using System.Collections;
using UnityEngine;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Common;

namespace RManager
{
    
    public class BundleLoader
    {
#if UNITY_EDITOR
        string mBundleBase = U3DFileIO.Combine(Application.dataPath, "/../ExportedResources/");
#elif UNITY_IPHONE
        string mBundleBase = U3DFileIO.Combine(Application.persistentDataPath, "/Resource/");
#elif UNITY_ANDROID
        string mBundleBase = U3DFileIO.Combine(Application.persistentDataPath, "/Resource/");
#else
        string mBundleBase = U3DFileIO.Combine(Application.dataPath, "/../ExportedResources/");
#endif

        Dictionary<string, PackageAndBundleInfo>    mAllPackageInfo;

        List<PackageAndBundleInfo>                  mCurrentLoadedAssetObj;

        public BundleLoader()
        {
            mCurrentLoadedAssetObj = new List<PackageAndBundleInfo>();
        }

        static void  Log(string log)
        {
#if UNITY_EDITOR
            //DEBUG.Log(log);
#endif
        }

        static void  LogError(string log)
        {
#if UNITY_EDITOR
            DEBUG.LogError(log);
#endif
        }

        static void  LogWarn(string log)
        {
#if UNITY_EDITOR
            DEBUG.LogWarning(log);
#endif
        }

        bool mProtocolDetected = false;
        bool mNeedAddProtocol = false;

        public string BundleBase 
        { 
            get{ return mBundleBase; } 
            set 
            { 
                mBundleBase = value;
                mProtocolDetected = false;
            } 
        }

        public Dictionary<string, PackageAndBundleInfo> AllPAckages {get { return mAllPackageInfo;} set{ mAllPackageInfo = value;} }

        public string GetFullPathWithProtocol(string path)
        {
            if(!mProtocolDetected)
            {
                mNeedAddProtocol = mBundleBase.IndexOf("://") < 0;
                mProtocolDetected = true;
            }

            if(!mNeedAddProtocol)
                return U3DFileIO.Combine(mBundleBase, path);

            return string.Format("file://{0}", U3DFileIO.Combine(mBundleBase, path));
        }

        static IEnumerator  LoadBundle_(string bundlePath, CoroutineResult<AssetBundle> result)
        {
            var URL = bundlePath;

            //DEBUG.Log(string.Format("Loading bundle from {0} By WWW", URL));

            var www = WWW.LoadFromCacheOrDownload(URL, 1);

            yield return www;

            if (www.error != null)
            {
                result.HasError = true;
                result.ErrorMessage = "www: " + www.error;
                yield break;
            }

            result.Reuslt = www.assetBundle;
            result.HasError = false;
        }
        /// <summary>
        /// bundlePath : related path
        /// </summary>
        /// <param name="bundlePath"></param>
        /// <returns></returns>
        public CoroutineResult<AssetBundle>  LoadBundle(string bundlePath)
        {
            var result = new CoroutineResult<AssetBundle>();
            result.Coroutine = LoadBundle_(GetFullPathWithProtocol(bundlePath), result);
            return result;
        }

        public static Dictionary<string, PackageAndBundleInfo> LoadAllAssetsClassXML(string path)
        {
            var result = new Dictionary<string, PackageAndBundleInfo>();
            var deffered = new Dictionary<string, PackageAndBundleInfo>();

            var xml = XElement.Load(path);
            var allPackage = xml.Elements("package");

            foreach (var pack in allPackage)
            {
                var packageName = pack.Element("name").Value;
                if (packageName == "external")
                    continue;

                var allDepends = pack.Element("dependencies");
                if (allDepends == null)
                    allDepends = pack.Element("depends");

                var isOneAsset = pack.Element("One") != null;

                var bundlePath = packageName + ".bdl";

                if (!isOneAsset)
                {
                    bundlePath = string.Format("{0}_{1}.bdl", packageName, pack.Element("Bundle").Value);
                }
                else
                {
                    bundlePath = string.Format("{0}_{1}.bdl", packageName, pack.Element("One").Value);
                }

                PackageAndBundleInfo padInfo = null;
                if (deffered.TryGetValue(packageName, out padInfo))
                {
                    padInfo.BundlePath = bundlePath;
                    deffered.Remove(packageName);
                }
                else
                {
                    padInfo = new PackageAndBundleInfo
                    {
                        Name = packageName,
                        BundlePath = bundlePath
                    };
                }

                padInfo.IsOneAsset = isOneAsset;
                if (!isOneAsset)
                {
                    var allAssets = pack.Element("assets").Elements("Info");
                    if (padInfo.AllAssets == null)
                        padInfo.AllAssets = new List<string>();
                    foreach (var asset in allAssets)
                    {
                        padInfo.AllAssets.Add(asset.Element("Name").Value);
                    }
                }

                if (allDepends != null)
                {
                    if (padInfo.Dependens == null)
                        padInfo.Dependens = new List<PackageAndBundleInfo>();

                    foreach (var dependen in allDepends.Elements("package"))
                    {
                        var pakageName = dependen.Value;
                        if (pakageName == "external")
                            continue;
                        PackageAndBundleInfo dependenPADInfo = null;
                        if (!result.TryGetValue(pakageName, out dependenPADInfo))
                        {
                            if (!deffered.TryGetValue(pakageName, out dependenPADInfo))
                            {
                                dependenPADInfo = new PackageAndBundleInfo { Name = pakageName };
                                deffered.Add(pakageName, dependenPADInfo);
                            }
                        }

                        if (dependenPADInfo.Dependers == null)
                            dependenPADInfo.Dependers = new Dictionary<string, PackageAndBundleInfo>();

                        padInfo.Dependens.Add(dependenPADInfo);
                        dependenPADInfo.Dependers.Add(packageName, padInfo);
                    }
                }

                result.Add(packageName, padInfo);
            }

            xml = null;
            return result;
        }

        class BatchLoadEntry
        {
            public WWW www;
            public PackageAndBundleInfo padInfo;
        }

        static void AddBatchItem(
            PackageAndBundleInfo padInfo, 
            int batchLevel, 
            Dictionary<int, Dictionary<string, BatchLoadEntry>> allBatches)
        {
            Dictionary<string, BatchLoadEntry> batch = null;
            if(!allBatches.TryGetValue(batchLevel, out batch))
            {
                batch = new Dictionary<string, BatchLoadEntry>();
                allBatches.Add(batchLevel, batch);
            }

            if(!batch.ContainsKey(padInfo.Name))
                batch.Add(padInfo.Name, new BatchLoadEntry{padInfo = padInfo});
        }

        /// <summary>
        /// 将可以同时加载的资源分别打包在一起，并且按依赖关系建立层级
        /// </summary>
        /// <param name="padInfo"></param>
        /// <param name="allBatches"></param>
        /// <returns></returns>
        static int BatchAllLoadOperation(
            PackageAndBundleInfo padInfo,
            Dictionary<int, Dictionary<string, BatchLoadEntry>> allBatches)
        {
            var batchLevel = 0;
            if(padInfo.IsAssetValid || padInfo.IsBundleInPlace || padInfo.IsBundleInLoading)
            {
                AddBatchItem(padInfo, batchLevel, allBatches);
                return batchLevel;
            }

            if(padInfo.Dependens != null && padInfo.Dependens.Count > 0)
            {
                foreach(var d in padInfo.Dependens)
                {
                    batchLevel = Math.Max(batchLevel, BatchAllLoadOperation(d, allBatches) + 1);
                }
            }

            AddBatchItem(padInfo, batchLevel, allBatches);

            return batchLevel;
        }

        static IEnumerator LoadBundleByBatchAsync(
            string bundleBaseFolder,
            PackageAndBundleInfo padInfo,
            CoroutineResult<AssetBundle> result)
        {
            var allBatches = new Dictionary<int, Dictionary<string, BatchLoadEntry>>();

            var level = BatchAllLoadOperation(padInfo, allBatches);

            long totalSize = 0;
            for(var i = 0; i < (level + 1); i++)
            {
                var batch = allBatches[i];

                var waitList = new LinkedList<BatchLoadEntry>();
                foreach (var kv in batch)
                {
                    var pi = kv.Value;
                    pi.padInfo.BundleRefCount++;

                    if (pi.padInfo.IsAssetValid)
                    {
                        totalSize += pi.padInfo.AssetSize;
                        continue;
                    }

                    if (pi.padInfo.IsBundleInLoading)
                    {
                        waitList.AddFirst(pi);
                        continue;
                    }

                    pi.padInfo.IsBundleInLoading = true;
                    var bundlePath = U3DFileIO.Combine(bundleBaseFolder, pi.padInfo.BundlePath);
                    var URL = string.Format("file://{0}", bundlePath);

                    //Log(string.Format("Loading bundle from {0} By WWW", URL));

                    pi.www  = new WWW(URL);
                    //yield return pi.www;

                    waitList.AddFirst(pi);
                }

                batch.Clear();
                yield return null;

                var hasWait     = (waitList.Count > 0);

                while(hasWait)
                {
                    hasWait     = false;

                    var current = waitList.First;
                    var magicCount = 0;
                    while (current != null)
                    {
                        var pi = current.Value;
                        if (!pi.www.isDone)
                        {
                            hasWait = true;
                            if(magicCount++ > 7)
                            {
                                magicCount = 0;
                                yield return null;
                            }

                            current = current.Next;
                            continue;
                        }

                        if (string.IsNullOrEmpty(pi.www.error))
                        {
                            pi.padInfo.AssetSize = pi.www.size;
                            totalSize += pi.padInfo.AssetSize;
                            pi.padInfo.Bundle = pi.www.assetBundle;
                        }
                        else
                        {
                            LogError("error loading <" + pi.padInfo.BundlePath + "> : " + pi.www.error);
                        }
                        pi.padInfo.IsBundleInLoading = false;
                        pi.www.Dispose();
                        pi.www = null;

                        var next = current.Next;
                        waitList.Remove(current);
                        current = next;
                    }

                    yield return null;
                }
            }
            LogError(string.Format("All bundle loaded in size : {0:N}", totalSize));
            allBatches.Clear();

            result.Completed(padInfo.Bundle);
            if(result.Reuslt == null)
                result.HasError = true;
/** */
        }

        static void LoadBundleByBatchSync(
            string bundleBaseFolder,
            PackageAndBundleInfo padInfo)
        {
            var allBatches = new Dictionary<int, Dictionary<string, BatchLoadEntry>>();

            var level = BatchAllLoadOperation(padInfo, allBatches) + 1;

            long totalSize = 0;
            for(var i = 0; i < level; i++)
            {
                var batch = allBatches[i];

                var waitList    = new LinkedList<BatchLoadEntry>();
                foreach (var kv in batch)
                {
                    var pi = kv.Value;
                    pi.padInfo.BundleRefCount++;

                    if (pi.padInfo.IsAssetValid)
                    {
                        totalSize += pi.padInfo.AssetSize;
                        continue;
                    }

                    if (pi.padInfo.IsBundleInLoading)
                    {
                        waitList.AddFirst(pi);
                        continue;
                    }

                    pi.padInfo.IsBundleInLoading = true;
                    var bundlePath = U3DFileIO.Combine(bundleBaseFolder, pi.padInfo.BundlePath);
                    var URL = string.Format("file://{0}", bundlePath);

                    //Log(string.Format("Loading bundle from {0} By WWW Sync", URL));

                    pi.www  = new WWW(URL);
                    //yield return pi.www;

                    waitList.AddFirst(pi);
                }

                batch.Clear();

                var hasWait     = (waitList.Count > 0);
                
                while (hasWait)
                {
                    hasWait     = false;
                    var current = waitList.First;
                    while (current != null)
                    {
                        var pi = current.Value;
                        pi.www.ForceLoadSync();
                        if (!pi.www.isDone)
                        {
                            hasWait = true;
                            current = current.Next;
                            continue;
                        }

                        if (string.IsNullOrEmpty(pi.www.error))
                        {
                            pi.padInfo.AssetSize = pi.www.size;
                            totalSize += pi.padInfo.AssetSize;
                            pi.padInfo.Bundle = pi.www.assetBundle;
                        }
                        else
                        {
                            LogError("error loading <" + pi.padInfo.BundlePath + "> : " + pi.www.error);
                        }
                        pi.padInfo.IsBundleInLoading = false;
                        pi.www.Dispose();
                        pi.www = null;

                        var next = current.Next;
                        waitList.Remove(current);
                        current = next;
                    }
                }
            }

            //LogError(string.Format("All bundle loaded in size : {0:N}", totalSize));
            allBatches.Clear();
            /** */
        }

        static IEnumerator  LoadBundleEx (
            string bundleFullPath, 
            string bundleBaseFolder, 
            PackageAndBundleInfo padInfo, 
            Dictionary<string, PackageAndBundleInfo> allPackageInfo,
            MonoBehaviour monoBehaviour,
            CoroutineResult<AssetBundle> result)
        {
            if(padInfo.Dependens != null && padInfo.Dependens.Count > 0)
            {
                foreach(var p in padInfo.Dependens)
                {
                    var load = LoadBundledResourceAsync(p.Name, bundleBaseFolder, allPackageInfo, monoBehaviour);
                    if(load.Coroutine != null)
                        yield return monoBehaviour.StartCoroutine(load.Coroutine);

                    if (load.HasError)
                    {
                        result.DoneByError(load.ErrorMessage, load.ErrorCode);
                        yield break;
                    }
                }
            }

            while (padInfo.IsBundleInLoading)
                yield return null;

            if(padInfo.Bundle != null)
            {
                result.Completed(padInfo.Bundle);
                yield break;
            }

            padInfo.IsBundleInLoading = true;

            var URL = string.Format("file://{0}", bundleFullPath);

            Log(string.Format("Loading bundle from {0} By WWW", URL));

            var www = new WWW(URL);

            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                result.DoneByError("www: " + www.error);
                padInfo.IsBundleInLoading = false;
                www.Dispose();
                yield break;
            }
/*
            var MD5 = Helper.GetMD5OfStream(new MemoryStream(www.bytes));
            if (check.ContainsKey(MD5))
            {
                padInfo.Bundle = check[MD5].Bundle;
            }
            else*/
            {
                //padInfo.BytesMD5 = MD5;
                padInfo.Bundle = www.assetBundle;
                //check.Add(MD5, padInfo);
            }

            padInfo.IsBundleInLoading = false;

            result.Completed(padInfo.Bundle);
            www.Dispose();
        }

        public static CoroutineResult<AssetBundle> LoadBundledResourceAsync(
            string resourcePath,
            string bundleBaseFolder, 
            Dictionary<string, PackageAndBundleInfo> allPackageInfo,
            MonoBehaviour monoBehaviour)
        {
            PackageAndBundleInfo padInfo = null;
            var result = new CoroutineResult<AssetBundle>();

            var packageName = resourcePath.ToLower();
            var packageNameB = packageName;
            if(packageName[0] != '/')
                packageNameB = "/" + packageName;

            if (!allPackageInfo.TryGetValue(packageName, out padInfo) && !allPackageInfo.TryGetValue(packageNameB, out padInfo))
            {
                result.DoneByError("there is no such bundle :" + resourcePath);
                //result.Coroutine = LoadBundle_(U3DFileIO.Combine("file://" + bundleBaseFolder, packageName  + ".bdl"), result);
                return result;
            }

            if (padInfo.Bundle != null)
            {
                result.Completed(padInfo.Bundle);
                return result;
            }

            var bundlePath = U3DFileIO.Combine(bundleBaseFolder, padInfo.BundlePath);
            //result.Coroutine = LoadBundleEx(bundlePath, bundleBaseFolder, padInfo, allPackageInfo, monoBehaviour, result);
            result.Coroutine = LoadBundleByBatchAsync(bundleBaseFolder, padInfo, result);
            return result;
        }

        static PackageAndBundleInfo LoadPackageSync(
            PackageAndBundleInfo padInfo, 
            string bundleBaseFolder, 
            Dictionary<string, PackageAndBundleInfo> allPackageInfo)
        {
            if (padInfo.IsBundleInPlace)
            {
                padInfo.BundleRefCount++;
                padInfo.IncAllDependsBundleRef();
                return padInfo;
            }

            //DEBUG.LogError(">>>>>>>>>>>>>>>>>>>> Bundle Load start >>>>>>>>>>>>>>>>>>>>");
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();

            //var bundlePath = U3DFileIO.Combine(bundleBaseFolder, padInfo.BundlePath);

            LoadBundleByBatchSync(bundleBaseFolder, padInfo);

            //watch.Stop();
            //DEBUG.LogError("<<<<<<<<<<<<<<<<<<<< Load end <<<<<<<<<<<<<<<<<<<<<");

            //DEBUG.LogError(string.Format("Resource bundle loaded in {0} ms", watch.Elapsed.TotalMilliseconds));

            return padInfo;
        }

        public static PackageAndBundleInfo LoadPackageSync(
            string resourcePath,
            string bundleBaseFolder, 
            Dictionary<string, PackageAndBundleInfo> allPackageInfo)
        {
            PackageAndBundleInfo padInfo = null;

            var packageName = resourcePath.ToLower();
            var packageNameB = packageName;
            if(packageName[0] != '/')
                packageNameB = "/" + packageName;

            if (!allPackageInfo.TryGetValue(packageName, out padInfo) && !allPackageInfo.TryGetValue(packageNameB, out padInfo))
            {
                LogError("there is no such bundle :" + resourcePath);
                return null;
            }

            return LoadPackageSync(padInfo, bundleBaseFolder, allPackageInfo);
        }

        public static void UnloadPackage(PackageAndBundleInfo padInfo)
        {
            if (padInfo.Dependens != null)
            {
                foreach (var d in padInfo.Dependens)
                {
                    UnloadPackage(d);
                }
            }

            padInfo.BundleRefCount--;
            if(padInfo.BundleRefCount > 0)
                return;

            Log(string.Format("Unloading {0} with {1} : {2}", padInfo.BundlePath, padInfo.Bundle, padInfo.BundleRefCount));
            if(padInfo.Bundle != null)
            {
                padInfo.Bundle.Unload(false);
                padInfo.Bundle = null;
                padInfo.IsBundleInLoading = false;  //TODO: fix this Bug
            }
        }

        public static IEnumerator UnloadPackageAsync(PackageAndBundleInfo padInfo, bool isFirst)
        {
            if(isFirst)
                yield return null;

            if (padInfo.Dependens != null)
            {
                foreach (var d in padInfo.Dependens)
                {
                    var x = UnloadPackageAsync(d, false);
                    while(x.MoveNext())
                        yield return null;
                }
            }

            padInfo.BundleRefCount--;
            if(padInfo.BundleRefCount > 0)
                yield break;

            Log(string.Format("Unloading {0} with {1} : {2}", padInfo.BundlePath, padInfo.Bundle, padInfo.BundleRefCount));
            if(padInfo.Bundle != null)
            {
                padInfo.Bundle.Unload(false);
                padInfo.Bundle = null;
                padInfo.IsBundleInLoading = false;  //TODO: fix this Bug
            }
        }

        public static void UnloadPackage(string resourcePath, Dictionary<string, PackageAndBundleInfo> allPackageInfo)
        {
            PackageAndBundleInfo padInfo = null;
            //var result = new CoroutineResult<AssetBundle>();

            var packageName = resourcePath.ToLower();
            var packageNameB = packageName;
            if(packageName[0] != '/')
                packageNameB = "/" + packageName;

            if (!allPackageInfo.TryGetValue(packageName, out padInfo) && !allPackageInfo.TryGetValue(packageNameB, out padInfo))
            {
                return;
            }

            UnloadPackage(padInfo);
        }

        public static void UnloadAssetAllObject()
        {}

        public static T GetAssetObjInBundleInfo<T>(string assetNameEx, PackageAndBundleInfo pInfo) where T : UnityEngine.Object
        {
            var assetName = assetNameEx.ToLower();
            assetName = U3DFileIO.Combine(Path.GetDirectoryName(assetName), Path.GetFileNameWithoutExtension(assetName));
            var result = pInfo.Bundle.Load(assetName, typeof(T)) as T;
            if (result == null)
            {
                LogError(string.Format("Can't find asset of {0}, now try to search", assetNameEx));
                var all = pInfo.Bundle.LoadAll();
                for (int i = 0; i < all.Length; i++)
                {
                    result = all[i] as T;
                    if (result != null && assetName.IndexOf(result.name) >= 0)
                        break;

                    all[i] = null;
                }
                all = null;
                if (result == null)
                    LogError(string.Format("Can't find asset of {0} !!!", assetNameEx));
            }

            return result;
        }

        public static IEnumerator GetAssetObjInBundleInfoAsync<T>(
            string                  assetNameEx,
            PackageAndBundleInfo    pInfo,
            CoroutineResult<T>      result
            ) where T : UnityEngine.Object
        {
            var assetName   = assetNameEx.ToLower();
            assetName   = U3DFileIO.Combine(Path.GetDirectoryName(assetName), Path.GetFileNameWithoutExtension(assetName));
            var load    = pInfo.Bundle.LoadAsync(assetName, typeof(T));
            yield return load;

            var assetObj = load.asset as T;
            if (assetObj == null)
            {
                LogError(string.Format("Can't find asset of {0}, now try to search", assetNameEx));
                var all = pInfo.Bundle.LoadAll();
                for (int i = 0; i < all.Length; i++)
                {
                    assetObj = all[i] as T;
                    if (assetObj != null && assetName.IndexOf(assetObj.name) >= 0)
                        break;

                    all[i] = null;
                    yield return null;
                }
                all = null;
                if (assetObj == null)
                {
                    var err = string.Format("Can't find asset of {0} !!!", assetNameEx);
                    LogError(err);
                    result.DoneByError(err);
                }
                else
                {
                    result.Completed(assetObj);
                }
            }
        }

        public T GetAssetObjSync<T>(
            string resourcePath,
            MonoBehaviour mono) where T : UnityEngine.Object
        {
            PackageAndBundleInfo padInfo = null;

            var allPackageInfo = mAllPackageInfo;
            var bundleBaseFolder = BundleBase;

            var packageName = resourcePath.ToLower();
            var packageNameB = packageName;
            if(packageName[0] != '/')
                packageNameB = "/" + packageName;

            if (!allPackageInfo.TryGetValue(packageName, out padInfo) && !allPackageInfo.TryGetValue(packageNameB, out padInfo))
            {
                LogError("there is no such asset :" + resourcePath);
                return null;
            }

            T result = null;
            if(!padInfo.IsAssetValid)
            {
                var needLoadBundle = !padInfo.IsBundleInPlace;
                if(needLoadBundle)
                    LoadPackageSync(padInfo, bundleBaseFolder, allPackageInfo);

                if(!padInfo.IsBundleInPlace)
                {
                    mono.StartCoroutine(UnloadPackageAsync(padInfo, true));
                    return result;
                }

                result = padInfo.Bundle.mainAsset as T;
                if(result == null)
                {
                    result = GetAssetObjInBundleInfo<T>(resourcePath, padInfo);
                }

                if(needLoadBundle)
                {
                    mono.StartCoroutine(UnloadPackageAsync(padInfo, true));
                }

                padInfo.MainAsset = result;

                mCurrentLoadedAssetObj.Add(padInfo);
            }
            else
            {
                result = padInfo.GetMainAsset() as T;
            }

            return result;
        }

        public IEnumerator GetAssetObjAsync<T>(
            string resourcePath,
            MonoBehaviour mono,
            CoroutineResult<T> result) where T : UnityEngine.Object
        {
            PackageAndBundleInfo padInfo = null;

            var allPackageInfo = mAllPackageInfo;
            var bundleBaseFolder = BundleBase;

            var packageName = resourcePath.ToLower();
            var packageNameB = packageName;
            if(packageName[0] != '/')
                packageNameB = "/" + packageName;

            if (!allPackageInfo.TryGetValue(packageName, out padInfo) && !allPackageInfo.TryGetValue(packageNameB, out padInfo))
            {
                LogError("there is no such asset :" + resourcePath);
                result.DoneByError("there is no such asset :" + resourcePath);
                yield break;
            }

            if(!padInfo.IsAssetValid)
            {
                var needLoadBundle = !padInfo.IsBundleInPlace;
                if(needLoadBundle)
                {
                    var result_ = new CoroutineResult<AssetBundle>();
                    var bundlePath = U3DFileIO.Combine(bundleBaseFolder, padInfo.BundlePath);

                    DEBUG.LogError(">>>>>>>>>>>>>>>>>>>> Bundle Load start >>>>>>>>>>>>>>>>>>>>");
                    var watch = new System.Diagnostics.Stopwatch();
                    watch.Start();

                    //result_.Coroutine = LoadBundleEx(bundlePath, bundleBaseFolder, padInfo, allPackageInfo, monoBehaviour, result_);
                    result_.Coroutine = LoadBundleByBatchAsync(bundleBaseFolder, padInfo, result_);
                    yield return mono.StartCoroutine(result_.Coroutine);

                    watch.Stop();
                    DEBUG.LogError("<<<<<<<<<<<<<<<<<<<< Load end <<<<<<<<<<<<<<<<<<<<<");

                    DEBUG.LogError(string.Format("Resource bundle loaded in {0} ms", watch.Elapsed.TotalMilliseconds));
                }

                if(!padInfo.IsBundleInPlace)
                {
                    mono.StartCoroutine(UnloadPackageAsync(padInfo, true));
                    yield break;
                }
                
                var assetObj = padInfo.Bundle.mainAsset as T;

                if(assetObj == null)
                {
                    var load = new CoroutineResult<T>();
                    load.Coroutine = GetAssetObjInBundleInfoAsync<T>(resourcePath, padInfo, load);
                    yield return mono.StartCoroutine(load.Coroutine);

                    assetObj = load.Reuslt;
                }

                if(needLoadBundle)
                    mono.StartCoroutine(UnloadPackageAsync(padInfo, true));

                padInfo.MainAsset = assetObj;

                result.Completed(assetObj);

                mCurrentLoadedAssetObj.Add(padInfo);
            }
            else
            {
                var assetObj = padInfo.GetMainAsset() as T;
                result.Completed(assetObj);
            }
        }

        public IEnumerator DestroyAllAssetObjects()
        {
            var copy = new List<PackageAndBundleInfo>(mCurrentLoadedAssetObj);
            mCurrentLoadedAssetObj.Clear();

            for(int i = 0; i < copy.Count; i++)
            {
                var pInfo = copy[i];
                var go = pInfo.MainAsset;
                if(go != null)
                    GameObject.Destroy(go);
                yield return null;
            }
            copy.Clear();
        }
    }
}
