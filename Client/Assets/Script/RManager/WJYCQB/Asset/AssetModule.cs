using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using RManager.AssetLoader;
using Common;
namespace Asset
{
    public class AssetModule:MonoBehaviour
    {

        public void Start()
        {
            AssetManager.Init(this);
            SceneAsset.DontDestroyOnLoad(this.gameObject);

            BundleAssetManager.AssetsRoot = Application.streamingAssetsPath; //"DResources";//
        }
        public void Update()
        {
            AssetManager.Update();
        }
        public string level = "level_0000", prefab = "np_npc01";
        public string controller = "controller";
        public string texture = "sssss", textasset = "textasset";
        private IAssetRequest item = null, item1 = null;
        private UnityEngine.GameObject oo;
        private GameObject g1, g2, source;

        List<GameObject> allGObjs = new List<GameObject>();

        public void OnGUI()
        {
            GUILayout.Label("测试加载场景");
            //level = GUILayout.TextField(level);
            if (GUILayout.Button("load Scene:" + level + ", process:" + (SceneAsset.Process * 100) + "%"))
            {
                //ChangeScene(level);
                BundleAssetManager.LoadScene(level, false);
                //Application.LoadLevelAdditive(level);
            }
            GUILayout.Label("测试加载Prefab");
            //prefab = GUILayout.TextField(prefab);
            if (GUILayout.Button(string.Format("load Prefab:{0},process: {1}", prefab, (item == null ? "0" : (item.Process * 100 + "")) + "%")))
            {
                var watch = new System.Diagnostics.Stopwatch();
                DEBUG.LogError(">>>>>>>>>>>>>>>>>>>> Load start >>>>>>>>>>>>>>>>>>>>");
                watch.Start();
                /**
                item = AssetManager.LoadObject(U3DFileIO.Combine("Prefabs", prefab), delegate(UnityEngine.Object g)
                {
                    watch.Stop();
                    DEBUG.LogError("<<<<<<<<<<<<<<<<<<<< Load end <<<<<<<<<<<<<<<<<<<<<");
                    DEBUG.LogError(string.Format("Resource bundle loaded in {0} ms", watch.Elapsed.TotalMilliseconds));
                    g1 = PrefabAsset.InitObject(g) as GameObject;
                    DEBUG.Log("load Prefab");
                });
                 /* */
                //var g = BundleAssetManager.Instance.LoadPrefab(prefab);
/*
                var atlas = loadTest.LoadAsset<UIAtlas>(prefab);
                if (atlas != null)
                {
                    var gobj = new GameObject(atlas.name);
                    //GameObject.Instantiate(gobj);
                }
                */
                //var c = BundleAssetManager.DefaultInstance.LoadController(controller);
                //var t = BundleAssetManager.DefaultInstance.LoadTexture(texture);
                //var ta = BundleAssetManager.DefaultInstance.LoadTextAsset(textasset);

                StartCoroutine(BundleAssetManager.DefaultInstance.LoadPrefabAsync(this, prefab, (g) =>
                    {
                        watch.Stop();
                        DEBUG.LogError(string.Format("Resource bundle loaded in {0} ms", watch.Elapsed.TotalMilliseconds));
                        DEBUG.LogError("<<<<<<<<<<<<<<<<<<<< Load end <<<<<<<<<<<<<<<<<<<<<");

                        var gobj = GameObject.Instantiate(g) as GameObject; //PrefabAsset.InitObject(g) as GameObject; //
                        if (gobj != null)
                        {
                            gobj.SetActive(true);
                            gobj.name = g.name;
                            allGObjs.Add(gobj);
                        }
                    }));
                //watch.Stop();
                //DEBUG.LogError(string.Format("Resource bundle loaded in {0} ms", watch.Elapsed.TotalMilliseconds));
                //DEBUG.LogError("<<<<<<<<<<<<<<<<<<<< Load end <<<<<<<<<<<<<<<<<<<<<");

                //var gobj = PrefabAsset.InitObject(g) as GameObject; //GameObject.Instantiate(g) as GameObject; //
                //if (gobj != null)
                //{
                //    gobj.SetActive(true);
                //    gobj.name = g.name;
                //    allGObjs.Add(gobj);
                //}
                /* */
            }
            if (GUILayout.Button(string.Format("delete Prefab:{0}", prefab)))
            {
                foreach(var gobj in allGObjs)
                    GameObject.DestroyImmediate(gobj);

                BundleAssetManager.DefaultInstance.UnloadAllUnuseAssets();
                Resources.UnloadUnusedAssets();
                AssetManager.DeleteObject(item.Path);
            }

            //texture = GUILayout.TextField(texture);
            if (GUILayout.Button("load Image"))
            {
                item1 = AssetManager.LoadObject(U3DFileIO.Combine("Images", texture), delegate(UnityEngine.Object o)
                {
                    oo = new GameObject("image test");
                    GUITexture gt = oo.AddComponent<GUITexture>();
                    gt.texture = o as Texture2D;
                    DEBUG.Log("load Image");
                });
            }
            if (GUILayout.Button("delete Image"))
            {
                GameObject.DestroyImmediate(oo, true);
                AssetManager.DeleteObject(item1.Path);
            }
        }
        public void ChangeScene(string name)
        {
            AssetManager.LoadScene(name, false, delegate()
            {
                DEBUG.LogError("load scene:" + name);
            });
        }
    }
}
