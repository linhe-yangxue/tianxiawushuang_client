using Common;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetHelp
{
    /// <summary>
    /// 返回指定路径下的所有U3D Assets 
    /// </summary>
    /// <param name="SubFolder">U3D 资源路径 Assets 下的子路径</param>
    /// <returns></returns>
    public static IEnumerable<UnityEngine.Object> AllU3DAssets(string SubFolder)
    {
        var resourceBase = string.IsNullOrEmpty(SubFolder) ? Application.dataPath : Helper.CombinePath(Application.dataPath, SubFolder);
        var assetFiles = Directory.GetFiles(resourceBase, "*", SearchOption.AllDirectories);
        var directories = new Dictionary<string, string>();

        foreach (var filePath in assetFiles)
        {
            UnityEngine.Object asset = null;
            var assetPath = Helper.CombinePath("Assets", filePath.Substring(filePath.IndexOf(Application.dataPath) + Application.dataPath.Length));
            var assetFolder = Path.GetDirectoryName(assetPath);
            if (!directories.ContainsKey(assetFolder))
            {
                directories.Add(assetFolder, assetFolder);
                asset = AssetDatabase.LoadAssetAtPath(assetFolder, typeof(UnityEngine.Object));
                if (asset != null)
                    yield return asset;
            }

            asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
            if (asset == null)
                continue;
            if (asset is MonoScript)
                continue;

            yield return asset;
        }

        directories.Clear();
    }

    public static IEnumerable<UnityEngine.Object> AllSelectedU3DAssets()
    {
        return Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
    }

    /// <summary>
    /// 获取被选择的目录或者是 默认的 Resources目录 下的所有 U3D Assets 
    /// </summary>
    /// <param name="isEnumAllAssets"></param>
    /// <returns></returns>
    public static IEnumerable<UnityEngine.Object> GetAllAssetsToDefine(bool isEnumAllAssets)
    {
        IEnumerable<UnityEngine.Object> allAssets = null;

        if (isEnumAllAssets)
        {
            allAssets = AllU3DAssets("/Resources");
        }
        else
        {
            allAssets = AllSelectedU3DAssets();
        }

        return allAssets;
    }
}