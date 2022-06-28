using UnityEditor;
using System.IO;
using System.Linq;
using Assets.Editor;
using BundleHelper;
using EditorHelp;
using System.Collections;
using Assets.Editor.Bundle;
using System.Collections.Generic;
using UnityEngine;
using Common;
//using PackDefine;

public class DuplicateAssetInfo
{
    public string AssetName;
    public List<ResourcesMapItemInfo> DuplicatedAssets;

    public DuplicateAssetInfo()
    {
        DuplicatedAssets = new List<ResourcesMapItemInfo>();
    }
}

class AssetsExamine
{
    static IEnumerator DependsCollector()
    {
        //收集所选目录下的所有场景文件信息
        var collector = new SelectedAssetsInfoCollector(); //ModelCollector();//TextureCollector(); //

        List<ResourcesMapItemInfo> allAssetsInfo = new List<ResourcesMapItemInfo>();
        yield return collector.StartCollection(allAssetsInfo);

        DEBUG.Log("there is [" + allAssetsInfo.Count + "] of assets collected; now analyzing depends ... ");

        var tempFilePath = Helper.CombinePath(Application.dataPath, "/./.Depends/analize.txt");
        if(!Directory.Exists(Path.GetDirectoryName(tempFilePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));

        var tempFile = File.Create(tempFilePath);
        foreach(var item in allAssetsInfo)
        {
            var temp = new DependencyCollector(item, tempFile);
            yield return temp.StartCollectDependencies();
        }

        tempFile.Close();

        DEBUG.Log("depends analyzing done.");
    }

    [MenuItem("Assets/AnalyzeDepends")]
    static void TestDepends()
    {
        EditorCoroutineRunner.StartEditorCoroutine(DependsCollector());
    }

    static IEnumerator ShowAssetsInfo()
    {
        var collector = new SelectedAssetsInfoCollector();

        List<ResourcesMapItemInfo> allAssetsInfo = new List<ResourcesMapItemInfo>();
        collector.IsCollectAny = true;
        yield return collector.StartCollection(allAssetsInfo);

        //DEBUG.Log("there is [" + allAssetsInfo.Count + "] of assets collected; now dumping information ... ");

        foreach(var item in allAssetsInfo)
        {
            var dumper = new DumpAssetInfo(item);
            yield return dumper.StartDumpInfo();
        }

        //DEBUG.Log("information dunping done.");
    }

    public static IEnumerator ScanDuplicatedAssets()
    {
        var allAsssets = new SelectedAssetsInfoCollector();
        allAsssets.SkipMonoScript = true;
        var allAssetsInfo = new List<ResourcesMapItemInfo>();
        yield return allAsssets.StartCollection(allAssetsInfo);

        var allDuplicated = new Dictionary<string, DuplicateAssetInfo>();
        foreach (var a in allAssetsInfo)
        {
            DuplicateAssetInfo dupInfo = null;
            if (!allDuplicated.TryGetValue(a.name, out dupInfo))
            {
                dupInfo = new DuplicateAssetInfo { AssetName = a.name };
                dupInfo.DuplicatedAssets.Add(a);
                allDuplicated.Add(a.name, dupInfo);
            }
            else
            {
                dupInfo.DuplicatedAssets.Add(a);
                DEBUG.LogError(string.Format("Duplicated assets found: {0} at {1}", a.name, a.assetPath));
            }
        }
    }

    static IEnumerator ShowAssetsMD5()
    {
        var collector = new SelectedAssetsInfoCollector();

        List<ResourcesMapItemInfo> allAssetsInfo = new List<ResourcesMapItemInfo>();
        collector.IsCollectAny = true;
        yield return collector.StartCollection(allAssetsInfo);

        //DEBUG.Log("there is [" + allAssetsInfo.Count + "] of assets collected; now dumping information ... ");

        foreach (var item in allAssetsInfo)
        {
            using(var assetFile = File.Open(item.assetPath, FileMode.Open))
            {
                var md5 = MD5.CalculateMD5(assetFile);
                DEBUG.Log(string.Format("{0} MD5 : {1}", item.assetPath, md5));
            }
            yield return null;
        }
    }

    [MenuItem("Assets/ShowInfo")]
    static void ExaminAssetsInfo()
    {
        EditorCoroutineRunner.StartEditorCoroutine(ShowAssetsInfo());
    }

    [MenuItem("Assets/ScanDuplicated")]
    static void ScanDuplicated()
    {
        EditorCoroutineRunner.StartEditorCoroutine(ScanDuplicatedAssets());
    }

    [MenuItem("Assets/ShowMD5")]
    static void ShowAssetFileMD5()
    {
        EditorCoroutineRunner.StartEditorCoroutine(ShowAssetsMD5());
    }
}