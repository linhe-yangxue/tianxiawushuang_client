using UnityEngine;
using UnityEditor;


public class CGMenus
{
    [MenuItem("CG/New Time Line", priority = 0)]
    public static void CreateNewCGTimeLine()
    {
        GameObject obj = CreateEmptyObject();
        var p = obj.AddComponent<CGTimeLine>();
        obj.name = "time line " + p.group;
        Selection.activeGameObject = obj;
    }

    [MenuItem("CG/New Camera Path", priority = 1000)]
    public static void CreateNewCameraPath()
    {
        GameObject obj = CreateEmptyObject();
        var p = obj.AddComponent<CameraPath>();
        obj.name = "camera path " + p.group;
        Selection.activeGameObject = obj;
    }

    [MenuItem("CG/New Object Path", priority = 1010)]
    public static void CreateNewObjectPath()
    {
        GameObject obj = CreateEmptyObject();
        var p = obj.AddComponent<CGObjectPath>();
        obj.name = "object path " + p.group;
        Selection.activeGameObject = obj;
    }

    [MenuItem("CG/New Play Effect", priority = 1020)]
    public static void CreateNewPlayEffect()
    {
        GameObject obj = CGTools.CreateObject("play effect", GetRoot().transform);
        obj.AddComponent<CGPlayAnimEffect>();
        obj.transform.position = SceneView.lastActiveSceneView.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        Selection.activeGameObject = obj;
    }

    public static GameObject CreateEmptyObject()
    {
        GameObject obj = new GameObject();
        obj.transform.parent = GetRoot().transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        return obj;
    }

    public static GameObject GetRoot()
    {
        GameObject root = GameObject.Find("CG Root");

        if (root == null)
        {
            root = new GameObject("CG Root");
        }

        return root;
    }
}