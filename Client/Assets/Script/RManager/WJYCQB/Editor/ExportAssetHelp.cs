using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using Object = UnityEngine.Object;
using Asset;
using System.IO;
/*
 * 资源名称中不能包含 * & 关键符号
 */
public class ExportAssetHelp : EditorWindow
{
    [MenuItem("AssetBundle/Prefab/Export")]
    static void ExportAssets()
    {
        exportAssets();
    }
    [MenuItem("AssetBundle/Prefab/Export Select")]
    static void ExportAssetsSelect()
    {
        UnityEditor.EditorApplication.SaveAssets();
        UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        if (objs.Length > 0)
        {
            ExportSceneHelp.ReadMd5List();
            ExportItem.CollectStaticAssets(true);
            exportObjects(objs);
            ExportItem.CollectStaticAssets(false);
            ExportSceneHelp.WriteMd5List();
        }
        else
        {
            UnityEditor.EditorUtility.DisplayDialog("提示", "没有需要打包的文件！", "嗯");
        }
    }
    [MenuItem("AssetBundle/Prefab/Export Force")]
    static void ExportAssetsForce()
    {
        ExportSceneHelp.ForceExport = 0;
        exportAssets();
    }
    static void exportAssets()
    {
        UnityEditor.EditorApplication.SaveAssets();
        UnityEngine.Object select = Selection.activeObject;
        if (select.GetType() == typeof(UnityEngine.Object))
        {
            UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            if (objs.Length > 1)
            {
                ExportSceneHelp.ReadMd5List();
                ExportItem.CollectStaticAssets(true);
                exportObjects(objs);
                ExportItem.CollectStaticAssets(false);
                ExportSceneHelp.WriteMd5List();
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog("提示", "没有需要打包的文件！", "嗯");
            }
        }
        else
        {
            UnityEditor.EditorUtility.DisplayDialog("提示", "没有选中文件夹！", "嗯");
        }
    }
    static void exportObjects(UnityEngine.Object[] objs)
    {
        UnityEditor.EditorUtility.DisplayProgressBar("稍等", "正在分析任务...", 0.5f);
        List<UnityEngine.Object> oList = new List<Object>();
        foreach (UnityEngine.Object o in objs)
        {
            if (/*o.GetType() != typeof(UnityEngine.Object) 
                && o.GetType() != typeof(UnityEngine.TextAsset) 
                &&*/ o.GetType() != typeof(UnityEngine.Material)
                && o.GetType() != typeof(UnityEngine.Shader))
            {
                oList.Add(o);
            }
        }
        UnityEngine.Object obj = null;
        for (int i = 0; i < oList.Count; i++)
        {
            obj = oList[i];
            UnityEditor.EditorUtility.DisplayProgressBar("导出进度", "正在导出 " + (i + 1) +"/" + oList.Count, 0);
            exportObject(obj);
        }
        ExportSceneHelp.ClearTmpFile();
        UnityEditor.EditorUtility.ClearProgressBar();
    }
    public static void exportObject(UnityEngine.Object obj)
    {
        Type objType = obj.GetType();
        if (objType == typeof(UnityEngine.GameObject))
        {
            if (obj.name.ToLower().IndexOf("atlas") > 0)
            {
                ExportItem item = new ExportItem(obj, "Prefabs", AssetDatabase.GetAssetPath(obj));
                DEBUG.Log(item.ExportPath);
                if (ExportSceneHelp.CheckMd5State(item))
                {
                    BuildPipeline.BuildAssetBundle(
                        item.Obj,
                        null,
                        ExportSceneHelp.ExportPath + "/" + item.ExportPath,
                        BuildAssetBundleOptions.CollectDependencies,
                        EditorUserBuildSettings.activeBuildTarget);
                }

                return;
            }
            GameObject g = GameObject.Instantiate(obj) as GameObject;
            g.name = obj.name;
            Dictionary<string, ExportItem> list = new Dictionary<string, ExportItem>();
            List<Transform> tList = new List<Transform>();
            List<GameObject> gList = new List<GameObject>();
            List<AssetRecord> rList = new List<AssetRecord>();
            List<MonoBehaviour> sList = new List<MonoBehaviour>();
            AssetRecordRoot root = g.AddComponent<AssetRecordRoot>();
            findObjectsInRoot(g.transform, tList);
            for (int i = 0; i < tList.Count; i++)
            {
                ExportItem.ExportObject2Item(tList[i].gameObject, list, gList, rList, sList);
            }
            root.Objects = gList.ToArray();
            root.Records = rList.ToArray();
            root.DeferredExecScripts = sList.ToArray();
            root.Type = "p";
            root.SetVersion();

            foreach (KeyValuePair<string, ExportItem> v in list)
            {
                if (ExportSceneHelp.CheckMd5State(v.Value) || ExportSceneHelp.ForceExport > -1) 
                {
                    //var exportPath = ExportSceneHelp.ExportPath + "/" + v.Value.ExportPath;
                    //var tempObj = PrefabUtility.CreateEmptyPrefab(exportPath + ".prefab");
                    //GameObject tempPrefab = PrefabUtility.ReplacePrefab(g, o);
                    BuildPipeline.BuildAssetBundle(v.Value.Obj, null, ExportSceneHelp.ExportPath + "/" + v.Value.ExportPath, BuildAssetBundleOptions.CollectDependencies, EditorUserBuildSettings.activeBuildTarget);
                }
            }
            string folder = System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + "Export" + Path.DirectorySeparatorChar;
            if (!System.IO.Directory.Exists(folder)) AssetDatabase.CreateFolder("Assets", "Export");
            UnityEngine.Object o = PrefabUtility.CreateEmptyPrefab("Assets/Export/" + g.name + ".prefab");
            GameObject rootPrefab = PrefabUtility.ReplacePrefab(g, o);
            ExportItem rootItem = new ExportItem(rootPrefab, "Prefabs", AssetDatabase.GetAssetPath(obj));
            if (ExportSceneHelp.CheckMd5State(rootItem) || ExportSceneHelp.ForceExport > -1) 
            {
                BuildPipeline.BuildAssetBundle(rootPrefab, null, ExportSceneHelp.ExportPath + "/" + rootItem.ExportPath, BuildAssetBundleOptions.CollectDependencies, EditorUserBuildSettings.activeBuildTarget);
            }

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(rootPrefab));
            GameObject.DestroyImmediate(g);
        }
        else if (objType == typeof(UnityEngine.Texture2D))
        {
            ExportItem item = new ExportItem(obj, "Images", AssetDatabase.GetAssetPath(obj));
            if (ExportSceneHelp.CheckMd5State(item)) BuildPipeline.BuildAssetBundle(item.Obj, null, ExportSceneHelp.ExportPath + "/" + item.ExportPath, BuildAssetBundleOptions.CollectDependencies, EditorUserBuildSettings.activeBuildTarget);
        }
        else if (objType == typeof(UnityEngine.AudioClip))
        {
            ExportItem item = new ExportItem(obj, "Sounds", AssetDatabase.GetAssetPath(obj));
            DEBUG.Log(item.ExportPath);
            if (ExportSceneHelp.CheckMd5State(item)) BuildPipeline.BuildAssetBundle(item.Obj, null, ExportSceneHelp.ExportPath + "/" + item.ExportPath, BuildAssetBundleOptions.CollectDependencies, EditorUserBuildSettings.activeBuildTarget);
        }
        else if (objType == typeof(UnityEngine.Object))
        {
            var scenePath = AssetDatabase.GetAssetPath(obj);
            if (scenePath.ToLower().Contains(".unity"))
                ExportSceneHelp.ExportSceneByName(scenePath);
        }
        else if (objType == typeof(UnityEngine.RuntimeAnimatorController) || obj is UnityEngine.RuntimeAnimatorController)
        {
            ExportItem item = new ExportItem(obj, "Controllers", AssetDatabase.GetAssetPath(obj));
            DEBUG.Log(item.ExportPath);
            if (ExportSceneHelp.CheckMd5State(item)) 
                BuildPipeline.BuildAssetBundle(item.Obj, null, ExportSceneHelp.ExportPath + "/" + item.ExportPath, BuildAssetBundleOptions.CollectDependencies, EditorUserBuildSettings.activeBuildTarget);
        }
        else if(objType == typeof(UnityEngine.TextAsset))
        {
            ExportItem item = new ExportItem(obj, "TextAssets", AssetDatabase.GetAssetPath(obj));
            DEBUG.Log(item.ExportPath);
            if (ExportSceneHelp.CheckMd5State(item)) 
                BuildPipeline.BuildAssetBundle(item.Obj, null, ExportSceneHelp.ExportPath + "/" + item.ExportPath, BuildAssetBundleOptions.CollectDependencies, EditorUserBuildSettings.activeBuildTarget);
        }
    }
    static void findObjectsInRoot(Transform g, List<Transform> gList)
    {
        foreach (Transform t in g)
        {
            findObjectsInRoot(t, gList);
        }
        //if (g.GetComponent("UISprite") != null || g.GetComponent("UILabel") != null)
        {
            //g.gameObject.SetActive(false);
        }
        gList.Add(g);
    }
}
