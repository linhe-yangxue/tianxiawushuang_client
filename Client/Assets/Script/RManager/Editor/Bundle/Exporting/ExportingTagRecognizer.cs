using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class ExportingTagRecognizer
{
    private Dictionary<string, string> mTagedFolders;
    private Dictionary<string, string> mExcludedTagedFolders;

    private UnityEngine.Object rootAsset;

    public string ExportAssetsTag { get; set; }
    public string ExcludedAssetsTag { get; set; }

    public ExportingTagRecognizer()
    {
        ExportAssetsTag = "*";
        ExcludedAssetsTag = null;
    }

    public ExportingTagRecognizer(string exportTag)
    {
        ExportAssetsTag = exportTag;
        ExcludedAssetsTag = null;
    }

    public ExportingTagRecognizer(string exportTag, string excludeTag)
    {
        ExportAssetsTag = exportTag;
        ExcludedAssetsTag = excludeTag;
    }

    public bool IsFolderHasTag(string folderPath)
    {
        if (mTagedFolders == null)
            mTagedFolders = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(ExcludedAssetsTag) && mExcludedTagedFolders == null)
            mExcludedTagedFolders = new Dictionary<string, string>();

        if (rootAsset == null)
            rootAsset = AssetDatabase.LoadAssetAtPath("/assets", typeof(UnityEngine.Object));

        var assetFolder = folderPath.ToLower();

        if (mTagedFolders.ContainsKey(assetFolder))
            return true;

        if (!string.IsNullOrEmpty(ExcludedAssetsTag) && mExcludedTagedFolders.ContainsKey(assetFolder))
            return false;

        var folderAsset = AssetDatabase.LoadAssetAtPath(assetFolder, typeof(UnityEngine.Object));
        while (folderAsset != null && rootAsset != folderAsset)
        {
            var tag = AssetDatabase.GetLabels(folderAsset);
            if (tag.Length > 0)
            {
                var result = tag.Where(str => str.IndexOf(ExportAssetsTag) >= 0).Select(str => str).Count() > 0;
                if (result)
                {
                    mTagedFolders.Add(assetFolder, assetFolder);
                    return true;
                }

                if (!string.IsNullOrEmpty(ExcludedAssetsTag))
                {
                    result = tag.Where(str => str.IndexOf(ExcludedAssetsTag) >= 0).Select(str => str).Count() > 0;
                    if (result)
                    {
                        mExcludedTagedFolders.Add(assetFolder, assetFolder);
                        return false;
                    }
                }
            }

            assetFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(folderAsset));

            if (mTagedFolders.ContainsKey(assetFolder))
                return true;

            if (!string.IsNullOrEmpty(ExcludedAssetsTag) && mExcludedTagedFolders.ContainsKey(assetFolder))
                return false;

            folderAsset = AssetDatabase.LoadAssetAtPath(assetFolder, typeof(UnityEngine.Object));
        }

        if (ExportAssetsTag == "*")
            return true;

        return false;
    }

    public bool IsAssetHasTag(UnityEngine.Object assetObj)
    {
        if (string.IsNullOrEmpty(ExportAssetsTag))
            return false;

        var needCheck = !string.IsNullOrEmpty(ExcludedAssetsTag);

        if (ExportAssetsTag == "*" && !needCheck)
            return true;

        var result = false;
        var tag = AssetDatabase.GetLabels(assetObj);
        if (tag.Length > 0)
        {
            if (needCheck)
            {
                result = tag.Where(str => str.IndexOf(ExcludedAssetsTag) >= 0).Select(str => str).Count() > 0;
                if (result)
                    return false;
            }

            if (ExportAssetsTag == "*")
                return true;

            result = tag.Where(str => str.IndexOf(ExportAssetsTag) >= 0).Select(str => str).Count() > 0;
            if (result)
                return true;
        }

        return IsFolderHasTag(Path.GetDirectoryName(AssetDatabase.GetAssetPath(assetObj)));
    }
}