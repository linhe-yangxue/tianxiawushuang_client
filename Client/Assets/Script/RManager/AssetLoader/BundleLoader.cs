using Common;
using System;
using System.IO;
using UnityEngine;

namespace RManager.AssetLoader
{
    public enum BundleFileLoader
    {
        USE_WWW = 0,
        USE_FILEIO = 1,
        USE_U3DRES = 2
    }
    public class BundleLoader : IDisposable
    {
        public string BundlePath { get; set; }

        public string AssetsRoot  { get; set; }
        public BundleFileLoader LoaderUse { get; private set; }
        public string error
        {
            get
            {
                if (LoaderUse == BundleFileLoader.USE_WWW && www != null)
                    return www.error;
                else
                    return errorMessage;
            }
        }

        public AssetBundle assetBundle
        {
            get
            {
                if (LoaderUse == BundleFileLoader.USE_WWW && www != null)
                    return www.assetBundle;
                else
                    return constructedBundle;
            }
        }
        public byte[] bytes
        {
            get
            {
                if (LoaderUse == BundleFileLoader.USE_WWW && www != null)
                    return www.bytes;
                else
                    return readBuffer;
            }
        }
        public int size
        {
            get
            {
                if (LoaderUse == BundleFileLoader.USE_WWW && www != null)
                    return www.size;
                else
                    return fileSize;
            }
        }

        public bool isDone
        {
            get
            {
                if (LoaderUse == BundleFileLoader.USE_WWW && www != null)
                    return www.isDone;
                else
                    return isFileReadDone;
            }
        }

        public int downloadedLength { get; private set; }

        FileStream fileStream;
        WWW www;
        ResourceRequest resourceRquest;
        bool isDownLoadTriggered;
        bool isFileReadDone;
        bool isFileReadStarted;
        int fileSize;
        byte[] readBuffer;
        string errorMessage;
        AssetBundle constructedBundle;

        string bundleFullPath;

        public BundleLoader(string assetRoot, string bundlePath, BundleFileLoader loaderUse)
        {
            AssetsRoot = assetRoot;
            BundlePath = bundlePath;
            constructedBundle = null;
            errorMessage = null;
            isFileReadStarted = false;
            isFileReadDone = false;
            isDownLoadTriggered = false;

            LoaderUse = loaderUse;

            bundleFullPath = Helper.CombinePath(AssetsRoot, BundlePath);

            if (LoaderUse == BundleFileLoader.USE_WWW)
            {
                www = new WWW(bundleFullPath);
            }
            else if(LoaderUse == BundleFileLoader.USE_U3DRES)
            {
                readBuffer = new byte[0];
                //resourceRquest = Resources.LoadAsync<TextAsset>(bundleFullPath);
            }
            else
            {
                readBuffer = new byte[0];

                try
                {
                    fileStream = new FileStream(bundleFullPath, FileMode.Open);
                    if (fileStream == null)
                    {
                        throw new Exception(string.Format("Bundle: Open file {0} failed!!", bundleFullPath));
                    }

                    var fInfo = new FileInfo(bundleFullPath);
                    fileSize = (int)fInfo.Length;

                    if (size <= 0)
                    {
                        isFileReadDone = true;
                        fileSize = 0;
                    }
                    else
                    {
                        readBuffer = new byte[fileSize];
                    }

                    StartFileRead();
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    isFileReadDone = true;
                    return;
                }
            }
        }

        void StartFileRead()
        {
            if (isFileReadStarted)
                return;

            isFileReadStarted = true;

            fileStream.BeginRead(bytes, 0, size, new AsyncCallback(iAsync =>
            {
                var read = fileStream.EndRead(iAsync);
                if (read != fileSize)
                {
                    errorMessage = string.Format("Bundle: file {0} read Failed!", bundleFullPath);
                }
                else
                {
                    try
                    {
                        constructedBundle = AssetBundle.CreateFromMemoryImmediate(bytes);//rq.assetBundle;
                        if(constructedBundle == null)
                            errorMessage = string.Format("Bundle: Failed to construct bundle from {0}", bundleFullPath);
                    }
                    catch (Exception ex)
                    {
                        errorMessage = string.Format("Bundle: bundle {0} create Failed! {1}", bundleFullPath, ex.Message);
                    }
                }

                isFileReadDone = true;
            }), this);
        }

        UnityEngine.Object LookResourceLoad()
        {
            return resourceRquest.asset;
        }

        void LookResourceReading()
        {
            /*
            var assetObj = LookResourceLoad();
            if(assetObj == null)
            {
                if(resourceRquest.isDone)
                {
                    errorMessage = string.Format("Bundle: bundle asset {0} cannot load from resource ", bundleFullPath);
                    isFileReadDone = true;
                }
                return;
            }
             */

            var assetObj = Resources.Load<TextAsset>(bundleFullPath);
            if (assetObj == null)
            {
                errorMessage = string.Format("Bundle: bundle asset {0} cannot load from resource ", bundleFullPath);
                isFileReadDone = true;
                return;
            }

            var textAssets = assetObj as TextAsset;

            if(textAssets == null)
            {
                errorMessage = string.Format("Bundle: bundle asset {0} cannot load from resource, invalid asset type ", bundleFullPath);
                isFileReadDone = true;
                return;
            }

            readBuffer = textAssets.bytes;
            var bundle = AssetBundle.CreateFromMemoryImmediate(readBuffer);
            textAssets = null;

            //Resources.UnloadUnusedAssets();

            if(bundle == null)
            {
                errorMessage = string.Format("Bundle: bundle asset {0} cannot be constructed ", bundleFullPath);
                isFileReadDone = true;
                return;
            }

            fileSize = readBuffer.Length;
            constructedBundle = bundle;
            isFileReadDone = true;            
        }

        public void PeekState(bool isFoceWWWSync)
        {
            if (isDone)
                return;

            if (!isDownLoadTriggered)
            {
                isDownLoadTriggered = true;

                if (isFoceWWWSync && www != null)
                {
                    ForceLoadSync();
                }
                else if (LoaderUse == BundleFileLoader.USE_FILEIO && fileStream != null)
                {
                    StartFileRead();
                }
                else //if(resourceRquest != null)
                {
                    LookResourceReading();
                }
            }
        }

        public int ForceLoadSync()
        {
            try
            {
                if (www != null)
                    return www.ForceLoadSync();
            }
            catch { }

            return -1;
        }

        public void Dispose()
        {
            if (www != null)
                www.Dispose();

            www = null;

            if (fileStream != null)
                fileStream.Dispose();

            fileStream = null;

            if (resourceRquest != null)
                resourceRquest = null;
        }
    }
}
