using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;

namespace Asset
{
    public enum AssetType
    {
        AssetBundle = 0,
        Texture,
        Audio,
        Text,
    }
    public class AssetRequest : IAssetRequest
    {
        public WWW Request = null;
        public string RealPath = string.Empty;
        public int UseCount = 0;
        public int UsedCount
        {
            get { return UseCount; }
            set { UseCount = value; }
        }
        public int RetryCount = 0;
        public bool WriteToLocal = false;
        public AssetRequestState State = AssetRequestState.None;
        public AssetRequestType RequestType = AssetRequestType.Default;
        private Stack<Action<AssetRequest>> callBackList = new Stack<Action<AssetRequest>>();
        private string path = "";
        public string Path
        {
            get { return path; }
        }
        public AssetRequest(string path)
        {
            RealPath = this.path = path;
        }
        private AssetType type = AssetType.AssetBundle;
        public AssetType Type
        {
            get { return type; }
            set { type = value; }
        }
        string text = null;
        public string Text
        {
            get
            {
                if (text == null)
                {
                    text = Request.text;
                    Request.Dispose();
                    Request = null;
                }
                return text;
            }
        }
        private UnityEngine.Object mainAssetObject;
        public UnityEngine.Object mainAsset
        {
            get
            {
                if (mainAssetObject == null)
                {
                    try
                    {
                        objs = Request.assetBundle.LoadAll();
                        mainAssetObject = Request.assetBundle.mainAsset;
                        Request.assetBundle.Unload(false);
                        Request.Dispose();
                        Request = null;
                    }
                    catch (Exception ex)
                    {
                        DEBUG.LogError(string.Format("AssetRequest({0}) load mainAsset fail:({1})", path, ex.ToString()));
                    }
                }
                return mainAssetObject;
            }
        }
        private UnityEngine.Object[] objs = null;
        public void AddTask(Action<AssetRequest> task)
        {
            //DEBUG.LogError("AddTask:  "+path + "   " + Time.realtimeSinceStartup);
            callBackList.Push(task);
            if (State == AssetRequestState.LoadComplete)
            {
                ClearTask();

                //PlayerHandle.InvokeHandle(delegate()
                //{
                //    ClearTask();

                //});
            }
            Update();
        }
        public void ClearTask()
        {
            while (callBackList.Count > 0)
            {
                Action<AssetRequest> callBack = callBackList.Pop();
                if (State == AssetRequestState.LoadComplete || State == AssetRequestState.LoadCompleted) { callBack(this); }
                else
                {
                    DEBUG.LogError(string.Format("AssetRequest url:{0} fail, state:{1}", RealPath, State));
                    callBack(null);
                }
            }
        }
        public float Process
        {
            get
            {
                if (Request == null)
                {
                    return 0;
                }
                else
                {
                    //if (State == AssetRequestState.Dispose || State == AssetRequestState.LoadFail || Request.isDone) return 1;
                    if (State == AssetRequestState.Dispose || State == AssetRequestState.LoadFail || State == AssetRequestState.LoadComplete) return 1;
                    else return Request.progress;
                }
            }
        }
        public void GetAsset()
        {
            LoadHelp.RequestOperation.StartHandle(this);
        }
        public void Update()
        {
            LoadHelp.RequestOperation.UpdateHandle(this);
        }
        public UnityEngine.Object Load(string n, Type t)
        {
            UnityEngine.Object result = null, tmp = null;
            if (objs != null && objs.Length > 0)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    tmp = objs[i];
                    if (string.Compare(tmp.name, n) == 0 && t == tmp.GetType())
                    {
                        result = tmp;
                        break;
                    }
                }
            }
            return result;
        }
        public void Dispose()
        {
            operateAssetUnload();
            State = AssetRequestState.Dispose;
        }
        private void operateAssetUnload()
        {
            if (State == AssetRequestState.LoadComplete && RequestType == AssetRequestType.Default)
            {
                switch (Type)
                {
                    case AssetType.AssetBundle:
                        {
                            if (Request == null)
                            {
                                if (objs != null)
                                {
                                   for(int i = 0; i < objs.Length; i++)
                                   {
                                       GameObject.DestroyImmediate(objs[i], true);
                                       //Resources.UnloadAsset(objs[i]);
                                   }
                                   objs = null;
                                }
                                mainAssetObject = null;
                            }
                            else
                            {
                                if (objs != null)
                                {
                                    for (int i = 0; i < objs.Length; i++)
                                    {
                                        //Resources.UnloadAsset(objs[i]);
                                        GameObject.DestroyImmediate(objs[i], true);
                                    }
                                    objs = null;
                                }
                                mainAssetObject = null;
                                Request.assetBundle.Unload(true);
                                Request.Dispose();
                                Request = null;
                            }
                            break;
                        }
                }
            }
        }
        private AssetBundle assetBundle = null;
        public void LoadScene(string name, bool add)
        {
            /*
           objs = Request.assetBundle.LoadAll();
           AssetBundle ab = Request.assetBundle;
            *  */

            assetBundle = AssetBundle.CreateFromMemoryImmediate(Request.bytes);
            Request.Dispose();
            Request = null;

            if (add) Application.LoadLevelAdditive(name);
            else Application.LoadLevel(name);
            AssetManager.Invoke(2, delegate()
            {
                assetBundle.Unload(false);
                assetBundle = null;
            });
        }
        /*
        public void LoadObjectFromAssetBundle(string name, Type type, Action<UnityEngine.Object> callBack)
        {
            //callBack(Request.assetBundle.Load(name, type));
            //Define.Mono.StartCoroutine(loadObjectFromAssetBundle(name, type, callBack));
            LoadHelp.AddObjectRequest(this, name, type, callBack);
        }
        private IEnumerator loadObjectFromAssetBundle(string name, Type type, Action<UnityEngine.Object> callBack)
        {
            yield return 0;
            //long t = DateTime.Now.Ticks;
            if (Request.assetBundle == null) DEBUG.LogError("loadObjectFromAssetBundle errror");
            AssetBundleRequest req = Request.assetBundle.LoadAsync(name, type);
            //DEBUG.Log(req.priority);
            //t = DateTime.Now.Ticks - t;
            //DEBUG.Log("loadObjectFromAssetBundle:" + t);
            while (!req.isDone)
            {
                yield return new WaitForSeconds(0.01f);
            }
            yield return new WaitForEndOfFrame();
            callBack(req.asset);
        }
         * */
    }
    public enum AssetRequestState
    {
        None = 0,
        BeginCheckFile,
        EndCheckFile,
        BeginLoad,
        Loading,
        LoadComplete,
        LoadCompleted,
        LoadFail,
        Dispose
    }
    public enum AssetRequestType
    {
        Default = 0,
        OtherDomain
    }
    public class AssetRequestOperation
    {
        public AssetRequestOperation(AssetRequestOperationHandle start, AssetRequestOperationHandle update)
        {
            StartHandle = start;
            UpdateHandle = update;
        }
        public AssetRequestOperationHandle StartHandle;
        public AssetRequestOperationHandle UpdateHandle;
    }
    public delegate void AssetRequestOperationHandle(AssetRequest request);
    public interface IAssetRequest
    {
        float Process { get; }
        string Path { get; }
        //byte[] Bytes { get; }
        //void Dispose();
        //AssetBundleRequest LoadAsyncState(string name, Type type);
        //UnityEngine.Object Load(string name);
        //UnityEngine.Object Load(string name, Type type);
        //int UsedCount { get; set; }
        //AssetType Type { get; set; }
        UnityEngine.Object mainAsset { get; }
        //void LoadScene(string name);
        //string Text { get; }
    }
}
