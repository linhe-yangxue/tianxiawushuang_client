using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using Object = UnityEngine.Object;
using Asset;
using System.IO;
using System.Reflection;
using System.Diagnostics;

public class ExportImageHelp
{
    [MenuItem("AssetBundle/Image/Create Alpha")]
    static void ExportAssets()
    {
        string[] assets = AssetDatabase.GetAllAssetPaths();
        List<string> source = new List<string>();
        string fromPath = null;
        for (int i = 0; i < assets.Length; i++)
        {
            fromPath = assets[i];
            if (fromPath.ToLower().IndexOf(".png") > -1)
            {
                fromPath = ExportSceneHelp.unityPath2PhysicsPath(fromPath);
                source.Add(fromPath);
            }
        }
        createAlphaByList(source);
    }
    [MenuItem("AssetBundle/Image/Create Alpha Select")]
    static void ExportAssetsSelect()
    {
        UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        string fromPath = null;
        List<string> source = new List<string>();
        foreach (UnityEngine.Object o in objs)
        {
            if (o.GetType() == typeof(UnityEngine.Texture2D))
            {
                fromPath = AssetDatabase.GetAssetPath(o);
                fromPath = ExportSceneHelp.unityPath2PhysicsPath(fromPath);
                if (fromPath.ToLower().IndexOf(".png") > -1)
                {
                    source.Add(fromPath);
                }
            }
        }
        createAlphaByList(source);
    }
    static void createAlphaByList(List<string> source)
    {
        string fromPath = null, toPath = null;
        setFileUpdateConfig(true);
        for (int i = 0; i < source.Count; i++)
        {
            fromPath = source[i];
            if (checkFileUpdate(fromPath))
            {
                toPath = string.Concat(fromPath.Substring(0, fromPath.IndexOf(".")), "_a.jpg");
                createAlpha(fromPath, toPath);
            }
        }
        setFileUpdateConfig(false);
    }
    static Dictionary<string, string> staticList = new Dictionary<string, string>();
    static bool checkFileUpdate(string fileName)
    {
        if (staticList.ContainsKey(fileName))
        {
            string md5 = MD5Help.GetMD5(fileName);
            if (string.Compare(staticList[fileName], md5) != 0)
            {
                staticList[fileName] = md5;
                return true;
            }
        }
        else
        {
            staticList[fileName] = MD5Help.GetMD5(fileName);
            return true;
        }
        return false;
    }
    static void setFileUpdateConfig(bool b)
    {
        string checkList = string.Concat(ExportSceneHelp.CreateExportFolder(), "CreateAlpha.dat");
        if (b)
        {
            if (File.Exists(checkList))
            {
                List<string> tmpList = new List<string>();
                using (StreamReader sr = new StreamReader(checkList, System.Text.Encoding.UTF8))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        tmpList.Add(line);
                    }
                }
                for (int i = 0; i < tmpList.Count; i++)
                {
                    staticList[tmpList[i]] = tmpList[i + 1];
                    i++;
                }
            }
        }
        else
        {
            if (staticList.Count != 0)
            {
                using (StreamWriter sr = new StreamWriter(checkList, false, System.Text.Encoding.UTF8))
                {
                    foreach (KeyValuePair<string, string> m in staticList)
                    {
                        sr.WriteLine(m.Key);
                        sr.WriteLine(m.Value);
                    }
                }
            }
        }
    }
    static void createAlpha(string from, string to)
    {
        string caName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "ca.exe");
        if (File.Exists(caName))
        {
            Process p = new Process();
            p.StartInfo.FileName = string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "ca.exe");
            p.StartInfo.Arguments = string.Concat("\"", from, "\" \"", to, "\"");
            p.Start();
            p.WaitForExit();
        }
    }
}
