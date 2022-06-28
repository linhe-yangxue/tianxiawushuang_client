using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Reflection;
using System;


[ExecuteInEditMode]
public class Modelhandle : MonoBehaviour
{

    public static List<string> FbxFileList = new List<string>();



    //[MenuItem("EffortForETC1/FBXAnimFilesCompressedStyle")]
     public static void FindAllFBXAnimFiles(ModelImporterAnimationCompression AnimForamt)
    {
        string[] paths = Directory.GetFiles(Application.dataPath, "*.FBX", SearchOption.AllDirectories);
        foreach (string path in paths)
        {
            if (!string.IsNullOrEmpty(path) && IsFBXAnimFile(path))   //full name
            {
                HandleFBXAnimFileZipStyle(path);
            }
        }

        Debug.Log("fbxfile num :" + FbxFileList.Count.ToString());
        for (int i = 0; i < FbxFileList.Count; i++)
        {
            //Debug.Log(name);
            ModelImporter ai = AssetImporter.GetAtPath(FbxFileList[i]) as ModelImporter;
            ai.animationCompression = AnimForamt;
            if (i < FbxFileList.Count)
            {
                double t = Math.Round( (i + 1) * 100.0/ FbxFileList.Count,1);
				EditorUtility.DisplayProgressBar("修改模型为高缩模式","进度：" + t.ToString() + "%",(float)t*0.01f);
                Debug.Log("修改模型为高缩模式 进度：" + t.ToString() + "%");
            }
            AssetDatabase.WriteImportSettingsIfDirty(FbxFileList[i]);
        }
		EditorUtility.DisplayProgressBar("修改模型为高缩模式","进度：100%",1f);
		EditorUtility.ClearProgressBar ();
        Debug.Log("修改结束！");
    }

    static bool IsFBXAnimFile(string _path)
    {
        string path = _path.ToLower();
        if (path.EndsWith(".fbx") && path.Contains("@"))
        {
            return true;
        }
        else
            return false;
    }
    static string GetRelativeAssetPath(string _fullPath)
    {
        _fullPath = GetRightFormatPath(_fullPath);
        int idx = _fullPath.IndexOf("Assets");
        string assetRelativePath = _fullPath.Substring(idx);
        return assetRelativePath;
    }
    static string GetRightFormatPath(string _path)
    {
        return _path.Replace("\\", "/");
    }

    static void HandleFBXAnimFileZipStyle(string _path)
    {
        string path = GetRelativeAssetPath(_path);
        FbxFileList.Add(path);
        return;
    }
}
 