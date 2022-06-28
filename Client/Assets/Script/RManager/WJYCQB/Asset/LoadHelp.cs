using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;

namespace Asset
{
    public delegate void TaskHandle();
    public class LoadHelp
    {
        public static string BundleBase = "./";
        public static string GetPath(string path)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            getPathResult.Length = 0;
            getPathResult.Append(Application.persistentDataPath);
            getPathResult.Append("/");
            getPathResult.Append(BundleBase);
            getPathResult.Append(path);
            tmpPath = getPathResult.ToString();
            if (File.Exists(tmpPath))
            {
                getPathResult.Length = 0;
                getPathResult.Append("file:///");
                getPathResult.Append(Application.persistentDataPath);
                getPathResult.Append("/");
                getPathResult.Append(BundleBase);
                getPathResult.Append(path);
                tmpPath = getPathResult.ToString();
                return tmpPath;
            }
            getPathResult.Length = 0;
            getPathResult.Append("file:///");
            getPathResult.Append(Application.dataPath);
            getPathResult.Append("/StreamingAssets/");
            getPathResult.Append(BundleBase);
            getPathResult.Append(path);
            tmpPath = getPathResult.ToString();
#elif UNITY_IPHONE
                getPathResult.Length = 0;
                getPathResult.Append(Application.persistentDataPath);
                getPathResult.Append("/");
                getPathResult.Append(BundleBase);
                getPathResult.Append(path);
                tmpPath = getPathResult.ToString();
                if (File.Exists(tmpPath))
                {
                    getPathResult.Length = 0;
                    getPathResult.Append("file:///");
                    getPathResult.Append(Application.persistentDataPath);
                    getPathResult.Append("/");
                    getPathResult.Append(BundleBase);
                    getPathResult.Append(path);
                    tmpPath = getPathResult.ToString();
                    return tmpPath;
                }
            getPathResult.Length = 0;
            getPathResult.Append("file:///");
            getPathResult.Append(Application.dataPath);
            getPathResult.Append("/Raw/");
            getPathResult.Append(BundleBase);
            getPathResult.Append(path);
            tmpPath = getPathResult.ToString();
#elif UNITY_ANDROID
                getPathResult.Length = 0;
                getPathResult.Append(Application.persistentDataPath);
                getPathResult.Append("/");
                getPathResult.Append(BundleBase);
                getPathResult.Append(path);
                tmpPath = getPathResult.ToString();
                if (File.Exists(tmpPath))
                {
                    getPathResult.Length = 0;
                    getPathResult.Append("file:///");
                    getPathResult.Append(Application.persistentDataPath);
                    getPathResult.Append("/");
                    getPathResult.Append(BundleBase);
                    getPathResult.Append(path);
                    tmpPath = getPathResult.ToString();
                    return tmpPath;
                }
            getPathResult.Length = 0;
            getPathResult.Append("jar:file://");
            getPathResult.Append(Application.dataPath);
            getPathResult.Append("!/assets/");
            getPathResult.Append(BundleBase);
            getPathResult.Append(path);
            tmpPath = getPathResult.ToString();
#endif
            return tmpPath;
        }
        private static StringBuilder getPathResult = new StringBuilder();
        public const int RequestRetryCount = 3;
        public static AssetRequestOperation RequestOperation;
        private static string tmpPath = string.Empty;
        public static Dictionary<string, AssetRequest> RequestCache = new Dictionary<string, AssetRequest>();
        private static List<string> tmpCompleteList = new List<string>();
        private static Dictionary<string, AssetRequest> tmpUpdateList = new Dictionary<string, AssetRequest>();
        public static Dictionary<string, AssetRequest> TmpUpdateList
        {
            get { return tmpUpdateList; }
        }

        private static MonoBehaviour thread;
        public static void Init(MonoBehaviour _thread)
        {
            thread = _thread;
            RequestOperation = new AssetRequestOperation(delegate(AssetRequest request)
            {
                if (request.RequestType == AssetRequestType.Default) 
                    request.RealPath = GetPath(request.Path);
                else
                    request.RealPath = request.Path;
                request.State = AssetRequestState.BeginLoad;
            }, delegate(AssetRequest request)
            {
                switch (request.State)
                {
                    case AssetRequestState.BeginLoad:
                        {
                            request.Request = new WWW(request.RealPath);
                            request.Request.threadPriority = ThreadPriority.Low;
                            request.State = AssetRequestState.Loading;

                            //DEBUG.Log(string.Format("request of [{0}] started", request.RealPath));
                            //DEBUG.LogError("BeginLoad :  "+request.RealPath + "   " + Time.realtimeSinceStartup);

                            break;
                        }
                    case AssetRequestState.Loading:
                        {
                            if (string.IsNullOrEmpty(request.Request.error))
                            {
                                if (request.Request.isDone)
                                {
                                    request.State = AssetRequestState.LoadComplete;
                                    //DEBUG.LogError("LoadComplete:   "+request.RealPath + "   " + Time.realtimeSinceStartup);
                                }
                            }
                            else
                            {
                                request.RetryCount++;
                                DEBUG.Log(string.Format("LoadHelp load {0} fail:{1},retry {2}", request.RealPath, request.Request.error, request.RetryCount));
                                if (request.RetryCount <= LoadHelp.RequestRetryCount)
                                {
                                    request.Dispose();
                                    request.State = AssetRequestState.BeginLoad;
                                }
                                else
                                {
                                    request.State = AssetRequestState.LoadFail;
                                    RequestCache.Remove(request.Path);
                                    tmpCompleteList.Add(request.Path);
                                    request.Dispose();
                                }
                            }
                            break;
                        }
                    case AssetRequestState.LoadComplete:
                        {
                            //DEBUG.Log(string.Format("request of [{0}] completed; result: {1}", request.RealPath, request.Request == null ? "NAN" : request.Request.error));
                            //request.State = AssetRequestState.LoadCompleted;
                            break;
                        }
                }
            });
        }
        public static IAssetRequest LoadObject(string path, Action<AssetRequest> callBack)
        {
            return LoadObject(path, AssetRequestType.Default, false, AssetType.AssetBundle, callBack);
        }
        public static IAssetRequest LoadObject(string path, Action<UnityEngine.Object> callBack)
        {
            return LoadObject(path, AssetRequestType.Default, false, AssetType.AssetBundle, delegate(AssetRequest req) { callBack(req.mainAsset); });
        }
        public static IAssetRequest LoadObject(string path, AssetType type, Action<AssetRequest> callBack)
        {
            return LoadObject(path, AssetRequestType.Default, false, type, callBack);
        }
        public static IAssetRequest LoadObject(string path, AssetRequestType requestType, Action<AssetRequest> callBack)
        {
            return LoadObject(path, requestType, false, AssetType.AssetBundle, callBack);
        }
        public static IAssetRequest LoadObject(string path, AssetRequestType requestType, bool preLoad, AssetType type, Action<AssetRequest> callBack)
        {
            AssetRequest request = null;
            if (RequestCache.ContainsKey(path))
            {
                request = RequestCache[path];
            }
            else
            {
                request = new AssetRequest(path);
                request.Type = type;
                request.RequestType = requestType;
                RequestCache[path] = request;
                tmpUpdateList[path] = request;
                request.GetAsset();
            }
            if (!preLoad) request.UseCount++;
            request.AddTask(callBack);
            return request;
        }

        public static void DeleteObject(string path)
        {
            if (RequestCache.ContainsKey(path))
            {
                AssetRequest request = RequestCache[path];
                request.UseCount--;
                if (request.UseCount <= 0 && (int)request.State > 4)
                {
                    request.Dispose();
                    RequestCache.Remove(path);
                }
            }
        }

        public static void LogState()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("RequestCache count:" + RequestCache.Count + "\r\n");
            foreach (KeyValuePair<string, AssetRequest> tmp in RequestCache)
            {
                sb.Append(tmp.Key + ":" + tmp.Value.State + ":" + tmp.Value.UsedCount + "\r\n");
            }
            DEBUG.Log(sb.ToString());
        }
        public static void Update()
        {
            assetRequestUpdate();
        }
        public static void LoadTexture(string path, Action<AssetRequest> callback)
        {
            LoadObject(path, AssetRequestType.Default, false, AssetType.Texture, delegate(AssetRequest req)
            {
                if (req == null)
                {
                    callback(null);
                }
                else
                {
                    callback(req);
                }
            });
        }
        private static void assetRequestUpdate()
        {
            foreach (KeyValuePair<string, AssetRequest> tmp in tmpUpdateList)
            {
                tmp.Value.Update();
                if (tmp.Value.State == AssetRequestState.LoadComplete || tmp.Value.State == AssetRequestState.LoadFail)
                {
                    tmpCompleteList.Add(tmp.Key);
                }
            }
            for (int i = 0; i < tmpCompleteList.Count; i++)
            {
                string key = tmpCompleteList[i];
                if (tmpUpdateList.ContainsKey(key))
                {
                    AssetRequest req = tmpUpdateList[key];
                    if (req.UsedCount <= 0)
                    {
                        RequestCache.Remove(key);
                        req.Dispose();
                    }
                    else
                    {
                        if (req.Type == AssetType.Text)
                        {
                            RequestCache.Remove(key);
                        }
                        req.ClearTask();
                    }
                    tmpUpdateList.Remove(key);
                }
            }
            if (tmpCompleteList.Count > 0) tmpCompleteList.Clear();
        }
        public static Transform FindChildObject(string partName, Transform mod)
        {
            Transform Result = null;
            foreach (Transform child in mod)
            {
                if (child.name == partName)
                {
                    Result = child;
                    break;
                }
                else
                {
                    Result = FindChildObject(partName, child);
                    if (Result != null)
                    {
                        break;
                    }
                }
            }
            return Result;
        }
        public static T FindChildObject<T>(Transform mod, bool checkRoot) where T : Component
        {
            T Result = null;
            if (checkRoot)
            {
                Result = mod.GetComponent<T>();
                if (Result != null)
                {
                    return Result;
                }
            }
            foreach (Transform child in mod)
            {
                Result = child.GetComponent<T>();
                if (Result != null)
                {
                    break;
                }
                else
                {
                    Result = FindChildObject<T>(child, false);
                    if (Result != null)
                    {
                        break;
                    }
                }
            }
            return Result;
        }
        public static void Invoke(float delay, TaskHandle t)
        {
            thread.StartCoroutine(invoke(delay, t));
        }
        private static IEnumerator invoke(float delay, TaskHandle t)
        {
            yield return new WaitForSeconds(delay);
            yield return new WaitForEndOfFrame();
            t();
        }
    }
}
