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
/*
 * 资源名称中不能包含 * & 关键符号
 */
public class ExportNGUIHelp : EditorWindow
{
    /*
    [MenuItem("AssetBundle/NGUI/Export Scene")]
    public static void ExportNGUIScene()
    {
        GameObject[] objs = GetAllGameObject();//GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject o in objs)
        {
            CheckAssets(o);
        }
    }
    [MenuItem("AssetBundle/NGUI/Export Scene1")]
    public static void ExportNGUIScene1()
    {
        GameObject g = Selection.activeObject as GameObject;
        g.SetActive(true);
    }
     * */
    static IEnumerable<Object> SceneRoots()
    {
        HierarchyProperty hp = new HierarchyProperty(HierarchyType.GameObjects);
        int[] expanded = new int[0];
        while (hp.Next(expanded))
        {
            yield return hp.pptrValue;
        }
    }
    public static GameObject[] GetAllGameObject()
    {
        List<GameObject> objs = new List<GameObject>();
        foreach (Object o in SceneRoots())
        {
            GetChild(o as GameObject, objs);
        }
        return objs.ToArray();
    }
    static void GetChild(GameObject root, List<GameObject> objs)
    {
        foreach (Transform child in root.transform)
        {
            GetChild(child.gameObject, objs);
        }
        objs.Add(root);
    }
    static void CheckAssets(Object root)
    {
        GameObject g = root as GameObject;
        Object[] assets = UnityEditor.EditorUtility.CollectDependencies(new Object[] { root });
        foreach (Object o in assets)
        {
            if (o.GetType() == typeof(Texture2D))
            {
                DEBUG.Log(g.name + ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" + o.GetType() + ":" + o.name);
            }
            /*
            if (o.GetType() == typeof(Material))
            {

                DEBUG.Log(o.GetType() + ":" + o.name);
            }
            else if (o.GetType() == typeof(AudioClip))
            {

                DEBUG.Log(o.GetType() + ":" + o.name);
            }
            else if (o.GetType() == typeof(Mesh))
            {

                DEBUG.Log(o.GetType() + ":" + o.name);
            }
            else if (o.GetType() == typeof(UIAtlas))
            {
                DEBUG.Log(o.GetType() + ":" + o.name);
            }
            else if (o.GetType() == typeof(Font))
            {

                DEBUG.Log(o.GetType() + ":" + o.name);
            }*/
        }
    }
}
