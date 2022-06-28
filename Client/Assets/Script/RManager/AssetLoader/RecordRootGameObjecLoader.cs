using Asset;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RManager.AssetLoader
{
    class PrefabLikeGameObjectLoader
    {
        private AssetRecordRoot     mRecordRoot     = null;
        private BundleAssetManager  mAssetManager   = null;
        private List<AssetLoadInfo> mCurrentLoadingAsset = new List<AssetLoadInfo>();

        private Dictionary<string, AssetGameObjectInfo> mAllObjectsToBuild = null;
        private List<AssetGameObjectInfo>               mAllImediateBuild =  new List<AssetGameObjectInfo>();

        public PrefabLikeGameObjectLoader(AssetRecordRoot recordRoot, BundleAssetManager assetManager)
        {
            mRecordRoot = recordRoot;
            mAssetManager = assetManager;

            if(mRecordRoot == null)
                return;
            mRecordRoot.OnRecordRootState = OnRecordRootState;

            mRecordRoot.enabled = true;
        }

        public void ReleaseAllRefAssets(bool isUnload)
        {
            foreach(var load in mCurrentLoadingAsset)
            {
                load.ReleaseAsset(isUnload);
            }

            mCurrentLoadingAsset.Clear();
        }

        public void InformGObjectOK(string GObjName)
        {
            mAllObjectsToBuild.Remove(GObjName);
        }

        public void EnableAllScript()
        {
            if(mRecordRoot.DeferredExecScripts == null)
            {
                DEBUG.LogError("资源导出工具版本不一致！！！");
                return;
            }
            for(var i = 0; i < mRecordRoot.DeferredExecScripts.Length; i++)
            {
                if(mRecordRoot.DeferredExecScripts[i] == null)
                    continue;
                try
                {
                    mRecordRoot.DeferredExecScripts[i].enabled = true;
                }catch
                {}
            }
        }

        public void OnRecordRootState(AssetRecordRoot record)
        {
            var key = record.transform.root.name;

            if(mRecordRoot != null && record != mRecordRoot)
            {
                DEBUG.LogError("Unmatch Object!!! " + key);
                return;
            }
            if(mRecordRoot == null)
                mRecordRoot = record;

            if (record.Init == 1)
            {
                PrepareLoadObjects();
                return;
            }

            if (record.Init == 2)
            {
                DEBUG.Log("Loading Large object " + key);
                record.Init = 3;
                record.StartCoroutine(BuildObjectAsync());
            }
        }

        int PrepareLoadMesh(AssetGameObjectInfo gObjInfo)
        {
            var assetPath = BundleAssetManager.getMeshAssetBundlePath(gObjInfo.Record.Mesh);

            AssetLoadInfo load = mAssetManager.GetAssetLoadInfo(assetPath);
            //DEBUG.Log("Asset: Loading mesh " + assetPath);
            load.RequireAsset(gObjInfo, -1, GObjRequestType.GORQ_MESH);

            mCurrentLoadingAsset.Add(load);

            return 1;
        }

        int PrepareLoadTexture(AssetGameObjectInfo gObjInfo)
        {
            var assetPath = "";
            var totalRequire = 0;
            for (var i = 0; i < gObjInfo.Record.Textures.Length; i++)
            {
                var textureName = gObjInfo.Record.Textures[i];

                if (string.IsNullOrEmpty(textureName))
                    continue;

                string[] ts = textureName.Split(new char[] { '*' });
                if (string.IsNullOrEmpty(ts[1]))
                {
                    continue;
                }
                else
                {
                    assetPath = BundleAssetManager.getTextureAssetBundlePath(ts[1]);

                    //DEBUG.Log("Asset: Loading texture " + assetPath);
                    AssetLoadInfo load = mAssetManager.GetAssetLoadInfo(assetPath);
                    load.RequireAsset(gObjInfo, i, GObjRequestType.GORQ_TEXTUR);
                    mCurrentLoadingAsset.Add(load);
                    totalRequire++;
                }
            }

            return totalRequire;
        }

        int PrepareLoadAudio(AssetGameObjectInfo gObjInfo)
        {
            var assetPath = "";
            var totalRequire = 0;
            for (var i = 0; i < gObjInfo.Record.Audios.Length; i++)
            {
                var audioName = gObjInfo.Record.Audios[i];

                if (string.IsNullOrEmpty(audioName))
                    continue;

                string[] ts = audioName.Split(new char[] { '*' });
                if(ts.Length < 2)
                {
                    DEBUG.LogError("Asset: error  audio " + audioName);
                    continue;
                }
                if (string.IsNullOrEmpty(ts[1]))
                {
                    continue;
                }
                else
                {
                    assetPath = BundleAssetManager.getSoundAssetBundlePath(ts[1]);
                    //DEBUG.Log("Asset: Loading autdio " + assetPath);
                    AssetLoadInfo load = mAssetManager.GetAssetLoadInfo(assetPath);
                    load.RequireAsset(gObjInfo, i, GObjRequestType.GORQ_AUTIO);
                    mCurrentLoadingAsset.Add(load);
                    totalRequire++;
                }
            }

            return totalRequire;
        }

        int PrepareLoadSubObjects(AssetGameObjectInfo gObjInfo)
        {
            var assetPath = "";
            var totalRequire = 0;
            for (var i = 0; i < gObjInfo.Record.Objects.Length; i++)
            {
                var audioName = gObjInfo.Record.Objects[i];

                if (string.IsNullOrEmpty(audioName))
                    continue;

                string[] paras = audioName.Split(new char[] { '*' });
                if (string.IsNullOrEmpty(paras[1]))
                {
                    continue;
                }

                string[] paras1 = paras[1].Split(new char[] { '.' });
                assetPath = audioName;

                switch (paras1[1])
                {
                    case "m":
                        {
                            assetPath = BundleAssetManager.getMeshAssetBundlePath(paras[1]);
                            break;
                        }
                    case "p":
                        {
                            assetPath = BundleAssetManager.getPrefabAssetBundlePath(paras[1]);
                            break;
                        }
                    case "t":
                        {
                            assetPath = BundleAssetManager.getTextureAssetBundlePath(paras[1]);
                            break;
                        }
                    case "a":
                        {
                            assetPath = BundleAssetManager.getSoundAssetBundlePath(paras[1]);
                            break;
                        }
                    case "ani":
                        {
                            assetPath = BundleAssetManager.getAnimationAssetBundlePath(paras[1]);
                            break;
                        }
                }

                //DEBUG.Log("Asset: Loading object " + assetPath);
                AssetLoadInfo load = mAssetManager.GetAssetLoadInfo(assetPath);

                load.RequireAsset(gObjInfo, i, GObjRequestType.GORQ_OBJECT);
                mCurrentLoadingAsset.Add(load);
                totalRequire++;
            }

            return totalRequire;
        }

        void PrepareLoadObjects()
        {
            var needToBuild = (mRecordRoot.Objects != null && mRecordRoot.Objects.Length > 0);

            if (needToBuild)
            {
                if(mAllObjectsToBuild == null)
                    mAllObjectsToBuild = new Dictionary<string,AssetGameObjectInfo>();
                else
                    mAllObjectsToBuild.Clear();

                mAllImediateBuild.Clear();
                for (var i = 0; i < mRecordRoot.Objects.Length; i++)
                {
                    var GObjInfo = new AssetGameObjectInfo();
                    GObjInfo.RootLoader = this;
                    GObjInfo.GObject = mRecordRoot.Objects[i];
                    GObjInfo.Record = mRecordRoot.Records[i];
                    GObjInfo.GObjName = GObjInfo.GObject.name;

                    if (GObjInfo.GObject == null)
                        DEBUG.LogError("null object!!!!");

                    //DEBUG.LogError("Asset: >>> Loading gameObject " + GObjInfo.GObjName);
                    var requireNum = GObjInfo.PrepareForLoad();

                    if (requireNum <= 0)
                    {
                        mAllImediateBuild.Add(GObjInfo);
                        continue;
                    }

                    var isCheck = false;

                    if (!string.IsNullOrEmpty(GObjInfo.Record.Mesh))
                    {
                        PrepareLoadMesh(GObjInfo);
                    }

                    if (GObjInfo.Record.Textures != null && GObjInfo.Record.Textures.Length > 0)
                    {
                        PrepareLoadTexture(GObjInfo);
                    }

                    if (GObjInfo.Record.Audios != null && GObjInfo.Record.Audios.Length > 0)
                    {
                        PrepareLoadAudio(GObjInfo);
                    }

                    if (GObjInfo.Record.Objects != null && GObjInfo.Record.Objects.Length > 0)
                    {
                        PrepareLoadSubObjects(GObjInfo);
                    }

                    // TODO : if duplicated GObjName??
                    mAllObjectsToBuild[GObjInfo.GObjName] = GObjInfo;
                }
            }
        }

        IEnumerator BuildObjectAsync()
        {
            for(var i = 0; i < mAllImediateBuild.Count; i++)
                mAllImediateBuild[i].Build();

            var needToBuild = (mRecordRoot.Objects != null && mRecordRoot.Objects.Length > 0);

            if (needToBuild)
            {
                var checkList = mCurrentLoadingAsset.ToArray();
                var needWait = false;
                var lastWaitIndex = 0;
                var isFirstRound = true;

                do
                {
                    needWait = false;
                    for (var i = lastWaitIndex; i < checkList.Length; i++)
                    {
                        var loadInfo = checkList[i];
                        if (loadInfo == null)
                            continue;

                        if (!loadInfo.LookAssetState(false))
                        {
                            yield return null;
                            needWait = true;
                            if (!isFirstRound)
                            {
                                lastWaitIndex = i;
                                break;
                            }
                        }
                        else
                        {
                            checkList[i] = null;
                        }
                    }

                    isFirstRound = false;

                } while (needWait);

                if(mAllObjectsToBuild.Count > 0)
                {
                    foreach(var kv in mAllObjectsToBuild)
                    {
                        DEBUG.LogError(string.Format("There stil have GObject {0} left !!!!", kv.Key));
                    }
                }
                
                EnableAllScript();
            }

            mRecordRoot.enabled = false;
            mRecordRoot = null;
        }

        public void LoadAndBuildObject()
        {
            PrepareLoadObjects();

            for(var i = 0; i < mAllImediateBuild.Count; i++)
                mAllImediateBuild[i].Build();

            var needToBuild = (mRecordRoot.Objects != null && mRecordRoot.Objects.Length > 0);
            if (needToBuild)
            {
                var checkList = mCurrentLoadingAsset.ToArray();
                var needWait = false;
                var lastWaitIndex = 0;

                var isFirstRound = true;

                do
                {
                    needWait = false;
                    for (var i = lastWaitIndex; i < checkList.Length; i++)
                    {
                        var loadInfo = checkList[i];
                        if (loadInfo == null)
                            continue;

                        if (!loadInfo.LookAssetState(true))
                        {
                            needWait = true;
                            if (!isFirstRound)
                            {
                                lastWaitIndex = i;
                                break;
                            }
                        }
                        else
                        {
                            checkList[i] = null;
                        }
                    }

                    isFirstRound = false;

                } while (needWait);
            }

            EnableAllScript();
        }

        public IEnumerator LoadAndBuildObjectAsync()
        {
            PrepareLoadObjects();

            for(var i = 0; i < mAllImediateBuild.Count; i++)
                mAllImediateBuild[i].Build();

            var needToBuild = (mRecordRoot.Objects != null && mRecordRoot.Objects.Length > 0);
            if (needToBuild)
            {
                var checkList = mCurrentLoadingAsset.ToArray();
                var needWait = false;
                var isFirstRound = true;
                var lastWaitIndex = 0;

                do
                {
                    needWait = false;
                    for (var i = lastWaitIndex; i < checkList.Length; i++)
                    {
                        var loadInfo = checkList[i];
                        if (loadInfo == null)
                            continue;

                        if (!loadInfo.LookAssetState(false))
                        {
                            yield return null;
                            needWait = true;
                            if (!isFirstRound)
                            {
                                lastWaitIndex = i;
                                break;
                            }
                        }
                        else
                        {
                            checkList[i] = null;
                        }
                    }

                    isFirstRound = false;

                } while (needWait);
            }

            EnableAllScript();
        }
    }

    class AssetGameObjectInfo : RManager.AssetLoader.IAssetRequiring
    {
        public PrefabLikeGameObjectLoader  RootLoader;
        public GameObject GObject { get; set; }
        public string GObjName { get; set; }
        public AssetRecord Record { get; set; }

        private bool ActiveSelf = false;
        private bool CanCombine = false;
        private int texIndex = 0;
        private Texture[] texList = null;
        private AudioClip[] audioList = null;
        private UnityEngine.Object[] objectList = null;
        private Mesh mesh = null;

        private int totalCount = 0;

        public int PrepareForLoad()
        {
            if (!string.IsNullOrEmpty(Record.Mesh))
            {
                totalCount++;
            }

            if (Record.Textures != null && Record.Textures.Length > 0)
            {
                texList = new Texture[Record.Textures.Length];
                totalCount += Record.Textures.Length;
            }

            if (Record.Audios != null && Record.Audios.Length > 0)
            {
                audioList = new AudioClip[Record.Audios.Length];
                totalCount += Record.Audios.Length;
            }

            if (Record.Objects != null && Record.Objects.Length > 0)
            {
                objectList = new UnityEngine.Object[Record.Objects.Length];
                totalCount += Record.Objects.Length;
            }

            return totalCount;
        }

        public void CompleteMesh(UnityEngine.Object assetObj, string error, object requestCtx)
        {
            mesh = assetObj as Mesh;
            totalCount--;
            if (totalCount <= 0)
                Build();
        }

        public void CompleteTexture(UnityEngine.Object assetObj, string error, object requestCtx)
        {
            int index = (int)requestCtx;
            texList[index] = assetObj as Texture;
            totalCount--;
            if (totalCount <= 0)
                Build();
        }

        public void CompleteAudio(UnityEngine.Object assetObj, string error, object requestCtx)
        {
            int index = (int)requestCtx;
            audioList[index] = assetObj as AudioClip;
            totalCount--;
            if (totalCount <= 0)
                Build();
        }

        public void CompleteObject(UnityEngine.Object assetObj, string error, object requestCtx)
        {
            int index = (int)requestCtx;
            objectList[index] = assetObj;
            totalCount--;
            if (totalCount <= 0)
                Build();
        }

        static List<Texture> tmpTextureList = new List<Texture>();
        private List<Texture> getTexture(int mIndex)
        {
            tmpTextureList.Clear();
            string tmpStart = mIndex + "*";
            if (texList != null)
            {
                for (int i = texIndex; i < texList.Length; i++)
                {
                    if (Record.Textures[i].StartsWith(tmpStart))
                    {
                        tmpTextureList.Add(texList[i]);
                    }
                    else
                    {
                        texIndex = i;
                        break;
                    }
                }
            }
            return tmpTextureList;
        }

        private void setMaterial(Material[] mats, int index, string setting)
        {
            if (string.IsNullOrEmpty(setting))
                return;
            getTexture(index);
            bool creat = true;
            string[] matSetting = setting.Split(new char[] { ',' });
            Material mat = AssetItem.GetMaterial(setting, matSetting[0], out creat);

            if (creat)
            {
                try
                {
                    ExportSceneConfig.SetMaterial(mat, tmpTextureList, matSetting);
                }
                catch(Exception ex)
                {
                    DEBUG.LogError("Asset: setMat Failed: " + ex.Message);
                }
            }

            mats[index] = mat;
        }

        public void StructurePhaseBuild()
        {
            if (objectList != null)
            {
                UnityEngine.Object tmpObj = null;
                string[] para = null;
                for (int i = 0; i < objectList.Length; i++)
                {
                    tmpObj = objectList[i];
                    if (tmpObj != null)
                    {
                        para = Record.Objects[i].Split(new char[] { '*' });

                        try
                        {
                            switch (para[0])
                            {
                                case "m":
                                    {
                                        GObject.GetComponent<MeshCollider>().sharedMesh = tmpObj as Mesh;
                                        break;
                                    }
                                case "f":
                                    {
                                        GObject.GetComponent<UILabel>().ambigiousFont = tmpObj;
                                        break;
                                    }
                                case "t":
                                    {
                                        Texture tmpObject = tmpObj as Texture;
                                        if (tmpObject != null) GObject.GetComponent<UITexture>().mainTexture = tmpObj as Texture;
                                        else GObject.GetComponent<UITexture>().material = tmpObj as Material;
                                        break;
                                    }
                                case "a":
                                    {
                                        GObject.GetComponent<UISprite>().atlas = tmpObj as UIAtlas;
                                        break;
                                    }
                                case "pa":
                                    {
                                        GObject.GetComponent<UIPopupList>().atlas = tmpObj as UIAtlas;
                                        break;
                                    }
                                case "pf":
                                    {
                                        GObject.GetComponent<UIPopupList>().ambigiousFont = tmpObj;
                                        break;
                                    }
                                case "flare":
                                    {
                                        GObject.GetComponent<Light>().flare = tmpObj as Flare;
                                        break;
                                    }
                                case "ani":
                                    {
                                        GObject.animation.AddClip(tmpObj as AnimationClip, tmpObj.name);
                                        if (GObject.animation.GetClipCount() == 1)
                                        {
                                            GObject.animation.clip = tmpObj as AnimationClip;
                                            if (GObject.transform.root.GetComponent<AssetRecordRoot>() == null)
                                            {
                                                GObject.transform.root.gameObject.SetActive(false);
                                                GObject.transform.root.gameObject.SetActive(true);
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                        catch(Exception ex)
                        { 
                            DEBUG.LogError("Asset: error building object");
                        }
                    }
                }
            }
            if (GObject.audio != null && audioList != null)
            {
                GObject.audio.clip = audioList[0];
                GObject.audio.Play();
            }
        }

        public void RenderPhaseBuild()
        {
            Material[] ms = null;
            byte renderType = 0;// SkinnedMeshRenderer:1;MeshFilter:2;Particle:3;MeshCollider:4;UISprite:5;UILabel:6
            ActiveSelf = Record.Paras[2] > 0;
            renderType = Record.Paras[1];
            if (renderType > 0 && renderType < 4)
            {
                ms = new Material[Record.Materials.Length];
                for (int i = 0; i < ms.Length; i++)
                {
                    setMaterial(ms, i, Record.Materials[i]);
                }
            }

            switch (renderType)
            {
                case 1:
                    {
                        SkinnedMeshRenderer renderer = GObject.GetComponent<SkinnedMeshRenderer>();
                        if (renderer != null)
                        {
                            renderer.sharedMaterials = ms;
                            if (mesh != null) renderer.sharedMesh = mesh;
                        }
                        break;
                    }
                case 2:
                    {
                        //DEBUG.Log(RootName);
                        MeshFilter filter = GObject.GetComponent<MeshFilter>();
                        GObject.renderer.sharedMaterials = ms;
                        filter.sharedMesh = mesh;
                        CanCombine = Record.Paras[0] > 0;
                        break;
                    }
                case 3:
                    {
                        GObject.renderer.sharedMaterials = ms;
                        ParticleSystemRenderer pr = GObject.GetComponent<ParticleSystemRenderer>();
                        if (pr != null) pr.mesh = mesh;
                        break;
                    }
                case 4:
                    {
                        MeshCollider collider = GObject.GetComponent<MeshCollider>();
                        if (collider != null) renderType = 4;
                        collider.sharedMesh = mesh;
                        break;
                    }
            }
        }

        public void Build()
        {
            RootLoader.InformGObjectOK(GObjName);
            if (GObject != null)
            {
                try
                {
                    StructurePhaseBuild();
                    RenderPhaseBuild();
                    GObject.SetActive(ActiveSelf);
/*
                    var allMono = GObject.GetComponents<MonoBehaviour>();
                    for (var i = 0; i < allMono.Length; i++)
                    {
                        if(allMono[i] == null)
                            continue;
                        if (ExportSceneConfig.DefferedEnableMonoscripts.ContainsKey(allMono[i].GetType().Name))
                            RootLoader.AddDeferedScript(allMono[i]);   
                    }
 */
                }
                catch (Exception ex)
                {
                    DEBUG.LogError(string.Format("Asset : buidl GObject {0} failed: {1}", GObjName, ex.Message));
                    DEBUG.LogError(string.Format("      < {0} >", ex.StackTrace));
                }
            }
            else
            {
                DEBUG.LogWarning (string.Format("Asset : GObjet {0} == null!!", GObjName));
            }
        }
    }
}
