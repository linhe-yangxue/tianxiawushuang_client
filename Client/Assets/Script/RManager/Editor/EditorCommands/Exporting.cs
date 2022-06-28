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
using Assets.Editor.Bundle;
using Common;
using Asset;

class ExportingCommmand
{
    static string ExportTag = "NeedBind";
    static string ExcludeTag = "ExceptBind";
    static IEnumerator ExportAllScenes_(bool isSearchAll)
    {
        DEBUG.Log("Exporting scene : gathering assets ...");
        yield return null;
        var allAssets = AssetHelp.GetAllAssetsToDefine(isSearchAll);
        var tagRecognizer = new ExportingTagRecognizer();
        tagRecognizer.ExportAssetsTag = ExportTag;
        tagRecognizer.ExcludedAssetsTag = ExcludeTag;

        if (EditorUtility.DisplayCancelableProgressBar("导出场景资源", "正在搜索场景资源文件...", 0.0f))
        {
            EditorUtility.ClearProgressBar();
            yield break;
        }

        var countf = allAssets.Count();
        var fi = 0.0f;

        ExportItem.InitExportDirs();

        ExportSceneHelp.ReadMd5List();
        ExportItem.CollectStaticAssets(true);

        foreach (var asset in allAssets)
        {
            fi += 1.0f;
            if (EditorUtility.DisplayCancelableProgressBar("正在搜索场景资源文件", string.Format("{0}...", asset.name), fi / countf))
            {
                EditorUtility.ClearProgressBar();
                yield break;
            }

            yield return null;
            if (asset.GetType() != typeof(UnityEngine.Object))
                continue;

            if (!tagRecognizer.IsAssetHasTag(asset))
                continue;

            var resInfo = U3DAssetsFileInfo.GetAssetFileInfo(asset, false);
            if (resInfo.resourceType != ResourcesType.SCENE)
                continue;

            DEBUG.Log("Exporting scene " + resInfo.assetPath);
            yield return null;

            ExportAssetHelp.exportObject(asset);
        }

        ExportItem.CollectStaticAssets(false);
        ExportSceneHelp.WriteMd5List();
        ExportSceneHelp.ClearTmpFile();

        EditorUtility.ClearProgressBar();
    }

    static IEnumerator ExportAllPrefabs_(bool isSearchAll)
    {
        DEBUG.Log("Exporting prefabs : gathering assets ...");
        yield return null;

        var allAssets = AssetHelp.GetAllAssetsToDefine(isSearchAll);
        var tagRecognizer = new ExportingTagRecognizer();
        tagRecognizer.ExportAssetsTag = ExportTag;
        tagRecognizer.ExcludedAssetsTag = ExcludeTag;

        if (EditorUtility.DisplayCancelableProgressBar("导出Prefab资源", "正在搜索Prefab资源文件...", 0.0f))
        {
            EditorUtility.ClearProgressBar();
            yield break;
        }

        var countf = allAssets.Count();
        var fi = 0.0f;

        ExportItem.InitExportDirs();

        ExportSceneHelp.ReadMd5List();
        ExportItem.CollectStaticAssets(true);

        foreach (var asset in allAssets)
        {
            fi += 1.0f;
            if (EditorUtility.DisplayCancelableProgressBar("正在搜索Prefab资源文件", string.Format("{0}...", asset.name), fi / countf))
            {
                EditorUtility.ClearProgressBar();
                yield break;
            }

            yield return null;
            if (asset.GetType() != typeof(UnityEngine.GameObject))
                continue;

            if (!tagRecognizer.IsAssetHasTag(asset))
                continue;

            var resInfo = U3DAssetsFileInfo.GetAssetFileInfo(asset, false);
            if (resInfo.resourceType != ResourcesType.PREFAB 
                && resInfo.resourceType != ResourcesType.UIATLAS
                && resInfo.resourceType != ResourcesType.MODEL)
            {
                continue;
            }

            DEBUG.Log("Exporting prefab " + resInfo.assetPath);
            yield return null;

            ExportAssetHelp.exportObject(asset);
        }

        ExportItem.CollectStaticAssets(false);
        ExportSceneHelp.WriteMd5List();
        ExportSceneHelp.ClearTmpFile();

        EditorUtility.ClearProgressBar();
    }

    static IEnumerator ExportAllTextures_(bool isSearchAll)
    {
        DEBUG.Log("Exporting textures : gathering assets ...");
        yield return null;
        var allAssets = AssetHelp.GetAllAssetsToDefine(isSearchAll);
        var tagRecognizer = new ExportingTagRecognizer();
        tagRecognizer.ExportAssetsTag = ExportTag;
        tagRecognizer.ExcludedAssetsTag = ExcludeTag;

        if (EditorUtility.DisplayCancelableProgressBar("导出贴图资源", "正在搜索贴图资源文件...", 0.0f))
        {
            EditorUtility.ClearProgressBar();
            yield break;
        }

        var countf = allAssets.Count();
        var fi = 0.0f;

        ExportItem.InitExportDirs();
        ExportSceneHelp.ReadMd5List();
        ExportItem.CollectStaticAssets(true);

        foreach (var asset in allAssets)
        {
            fi += 1.0f;
            if (EditorUtility.DisplayCancelableProgressBar("正在搜索贴图资源文件", string.Format("{0}...", asset.name), fi / countf))
            {
                EditorUtility.ClearProgressBar();
                yield break;
            }

            yield return null;
            var resInfo = U3DAssetsFileInfo.GetAssetFileInfo(asset, false);
            if (resInfo.resourceType != ResourcesType.TEXTRUE)
                continue;

            if (!tagRecognizer.IsAssetHasTag(asset))
                continue;

            DEBUG.Log("Exporting texture " + resInfo.assetPath);
            yield return null;

            ExportAssetHelp.exportObject(asset);
        }

        ExportItem.CollectStaticAssets(false);
        ExportSceneHelp.WriteMd5List();
        ExportSceneHelp.ClearTmpFile();

        EditorUtility.ClearProgressBar();
    }

    static IEnumerator ExportAllSounds_(bool isSearchAll)
    {
        DEBUG.Log("Exporting sounds : gathering assets ...");
        yield return null;

        var allAssets = AssetHelp.GetAllAssetsToDefine(isSearchAll);
        var tagRecognizer = new ExportingTagRecognizer();
        tagRecognizer.ExportAssetsTag = ExportTag;
        tagRecognizer.ExcludedAssetsTag = ExcludeTag;

        if (EditorUtility.DisplayCancelableProgressBar("导出音频资源", "正在搜索音频资源文件...", 0.0f))
        {
            EditorUtility.ClearProgressBar();
            yield break;
        }

        var countf = allAssets.Count();
        var fi = 0.0f;

        ExportItem.InitExportDirs();
        ExportSceneHelp.ReadMd5List();
        ExportItem.CollectStaticAssets(true);

        foreach (var asset in allAssets)
        {
            fi += 1.0f;
            if (EditorUtility.DisplayCancelableProgressBar("正在搜索音频资源文件", string.Format("{0}...", asset.name), fi / countf))
            {
                EditorUtility.ClearProgressBar();
                yield break;
            }

            yield return null;
            var resInfo = U3DAssetsFileInfo.GetAssetFileInfo(asset, false);
            if (resInfo.resourceType != ResourcesType.SOUND)
                continue;

            if (!tagRecognizer.IsAssetHasTag(asset))
                continue;

            DEBUG.Log("Exporting texture " + resInfo.assetPath);
            yield return null;

            ExportAssetHelp.exportObject(asset);
        }

        ExportItem.CollectStaticAssets(false);
        ExportSceneHelp.WriteMd5List();
        ExportSceneHelp.ClearTmpFile();

        EditorUtility.ClearProgressBar();
    }

    static IEnumerator ExportAllTextAssets_(bool isSearchAll)
    {
        DEBUG.Log("Exporting TextAsset : gathering assets ...");
        yield return null;

        var allAssets = AssetHelp.GetAllAssetsToDefine(isSearchAll);
        var tagRecognizer = new ExportingTagRecognizer();
        tagRecognizer.ExportAssetsTag = ExportTag;
        tagRecognizer.ExcludedAssetsTag = ExcludeTag;

        if (EditorUtility.DisplayCancelableProgressBar("导出TextAsset资源", "正在搜索TextAsset资源文件...", 0.0f))
        {
            EditorUtility.ClearProgressBar();
            yield break;
        }

        var countf = allAssets.Count();
        var fi = 0.0f;

        ExportItem.InitExportDirs();
        ExportSceneHelp.ReadMd5List();
        ExportItem.CollectStaticAssets(true);

        foreach (var asset in allAssets)
        {
            fi += 1.0f;
            if (EditorUtility.DisplayCancelableProgressBar("正在搜索TextAsset资源文件", string.Format("{0}...", asset.name), fi / countf))
            {
                EditorUtility.ClearProgressBar();
                yield break;
            }

            yield return null;
            var resInfo = U3DAssetsFileInfo.GetAssetFileInfo(asset, false);
            if (resInfo.resourceType != ResourcesType.TEXTASSET)
                continue;

            if (!tagRecognizer.IsAssetHasTag(asset))
                continue;

            DEBUG.Log("Exporting TextAsset " + resInfo.assetPath);
            yield return null;

            ExportAssetHelp.exportObject(asset);
        }

        ExportItem.CollectStaticAssets(false);
        ExportSceneHelp.WriteMd5List();
        ExportSceneHelp.ClearTmpFile();

        EditorUtility.ClearProgressBar();
    }

    static IEnumerator ExportAllControllers_(bool isSearchAll)
    {
        DEBUG.Log("Exporting Controller : gathering assets ...");
        yield return null;

        var allAssets = AssetHelp.GetAllAssetsToDefine(isSearchAll);
        var tagRecognizer = new ExportingTagRecognizer();
        tagRecognizer.ExportAssetsTag = ExportTag;
        tagRecognizer.ExcludedAssetsTag = ExcludeTag;

        if (EditorUtility.DisplayCancelableProgressBar("导出Controller资源", "正在搜索Controller资源文件...", 0.0f))
        {
            EditorUtility.ClearProgressBar();
            yield break;
        }

        var countf = allAssets.Count();
        var fi = 0.0f;

        ExportItem.InitExportDirs();
        ExportSceneHelp.ReadMd5List();
        ExportItem.CollectStaticAssets(true);

        foreach (var asset in allAssets)
        {
            fi += 1.0f;
            if (EditorUtility.DisplayCancelableProgressBar("正在搜索Controller资源文件", string.Format("{0}...", asset.name), fi / countf))
            {
                EditorUtility.ClearProgressBar();
                yield break;
            }

            yield return null;
            var resInfo = U3DAssetsFileInfo.GetAssetFileInfo(asset, false);
            if (resInfo.resourceType != ResourcesType.CONTROLLER)
                continue;

            if (!tagRecognizer.IsAssetHasTag(asset))
                continue;

            DEBUG.Log("Exporting Controller " + resInfo.assetPath);
            yield return null;

            ExportAssetHelp.exportObject(asset);
        }

        ExportItem.CollectStaticAssets(false);
        ExportSceneHelp.WriteMd5List();
        ExportSceneHelp.ClearTmpFile();

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("AssetExport/ExportSelectedScenes")]
    static void ExportAllScenes()
    {
        EditorCoroutineRunner.StartEditorCoroutine(ExportAllScenes_(false));
    }

    [MenuItem("AssetExport/ExportSelectedPrefabs")]
    static void ExportAllPrefabs()
    {
        EditorCoroutineRunner.StartEditorCoroutine(ExportAllPrefabs_(false));
    }

    [MenuItem("AssetExport/ExportSelectedTextures")]
    static void ExportAllTexture()
    {
        EditorCoroutineRunner.StartEditorCoroutine(ExportAllTextures_(false));
    }

    [MenuItem("AssetExport/ExportSelectedSounds")]
    static void ExportAllSounds()
    {
        EditorCoroutineRunner.StartEditorCoroutine(ExportAllSounds_(false));
    }

    [MenuItem("AssetExport/ExportSelectedTextAssets")]
    static void ExportAllTextAssets()
    {
        EditorCoroutineRunner.StartEditorCoroutine(ExportAllTextAssets_(false));
    }

    [MenuItem("AssetExport/ExportSelectedControllers")]
    static void ExportAllControllers()
    {
        EditorCoroutineRunner.StartEditorCoroutine(ExportAllControllers_(false));
    }
}