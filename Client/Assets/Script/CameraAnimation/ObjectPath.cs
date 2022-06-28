using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;
using Utilities.Math;


public class ObjectPath : CGComponent
{
    private static Dictionary<int, ObjectPath> paths = new Dictionary<int, ObjectPath>();

    public static ObjectPath GetObjectPathInCurrentScene(int group)
    {
        ObjectPath p;

        if (paths.TryGetValue(group, out p) && p != null)
        {
            return p;
        }

        return null;
    }

    [Serializable]
    public class PathNode
    {
        public Transform point;
        public float speed = 1f;
    }

    public int group = 1;
    public List<PathNode> pathNodes = new List<PathNode>();
    public MovementPath path { get; private set; }

#if UNITY_EDITOR
    public bool showPath = true;
    public Color pathColor = new Color(0f, 1f, 0f, 1f);
#endif

    private GameObject[] cornerObjects;

    private void Reset()
    {
        CameraPath[] allPath = FindObjectsOfType<CameraPath>();
        int maxGroup = 0;

        foreach (var p in allPath)
        {
            if (p != this && p.group > maxGroup)
            {
                maxGroup = p.group;
            }
        }

        this.group = maxGroup + 1;
    }

    protected override void OnInit()
    {
        path = GeneratePath();

        if (paths.ContainsKey(group))
        {
            paths[group] = this;
        }
        else
        {
            paths.Add(group, this);
        }
    }

    private void OnDestroy()
    {
        if (paths.ContainsKey(group) && paths[group] == this)
        {
            paths.Remove(group);
        }
    }

    public GameObject GetCornerObjectByCornerIndex(int cornerIndex)
    {
        return cornerObjects != null && cornerIndex < cornerObjects.Length ? cornerObjects[cornerIndex] : null;
    }

    protected override IEnumerator DoPlay()
    {
        var r = new MovementRoutine(target.transform, path);
        r.onCorner += OnReachCorner;
        return r;
    }

    private void OnReachCorner(int index)
    {
        if (cornerObjects != null && index < cornerObjects.Length && cornerObjects[index] != null)
        {
            OnReachCorner(cornerObjects[index]);
        }
    }

    protected virtual void OnReachCorner(GameObject cornerObject)
    { }

    protected virtual void OnCreateNewCornerObject(GameObject cornerObject)
    { }

    private MovementPath GeneratePath()
    {
        var p = new MovementPath();

        if (pathNodes != null)
        {
            List<PathNode> nodeList = new List<PathNode>(pathNodes);
            nodeList.RemoveAll(node => node.point == null);

            p.corners = new MovementCorner[nodeList.Count];
            cornerObjects = new GameObject[nodeList.Count];

            for (int i = 0; i < nodeList.Count; ++i)
            {
                var corner = new MovementCorner();
                var node = nodeList[i];
                corner.position = node.point.position;
                corner.rotation = node.point.rotation;
                corner.speed = node.speed;
                p.corners[i] = corner;
                cornerObjects[i] = node.point.gameObject;
            }
        }

        p.Refresh();
        return p;
    }

    public void DeleteAtIndex(int index)
    {
        if (pathNodes == null || index >= pathNodes.Count)
        {
            return;
        }

        if (pathNodes[index].point != null)
        {
            DestroyImmediate(pathNodes[index].point.gameObject);
        }

        pathNodes.RemoveAt(index);

        for (int i = index; i < pathNodes.Count; ++i)
        {
            if (pathNodes[i].point != null)
            {
                pathNodes[i].point.name = "Point_" + i;
            }
        }
    }

    public PathNode InsertAtIndex(int index)
    {
        if (pathNodes == null || index > pathNodes.Count)
        {
            return null;
        }

        var newPoint = CGTools.CreateObject("Point_" + index, transform).transform;
        OnCreateNewCornerObject(newPoint.gameObject);
        var newNode = new PathNode();
        newNode.point = newPoint;
        pathNodes.Insert(index, newNode);
        
        if (index >= 1 && pathNodes[index - 1].point != null)
        {
            newPoint.position = pathNodes[index - 1].point.position;
            newPoint.rotation = pathNodes[index - 1].point.rotation;
            newNode.speed = pathNodes[index - 1].speed;
        }
        else if (index + 1 < pathNodes.Count && pathNodes[index + 1].point != null)
        {
            newPoint.position = pathNodes[index + 1].point.position;
            newPoint.rotation = pathNodes[index + 1].point.rotation;
            newNode.speed = pathNodes[index + 1].speed;
        }
        else 
        {
            newPoint.localPosition = Vector3.zero;
            newPoint.localRotation = Quaternion.identity;
            newNode.speed = 1;
        }       

        for (int i = index + 1; i < pathNodes.Count; ++i)
        {
            if (pathNodes[i].point != null)
            {
                pathNodes[i].point.name = "Point_" + i;
            }
        }

        return newNode;
    }

    public PathNode Add(Vector3 position, Quaternion rotation)
    {
        if (pathNodes == null)
        {
            return null;
        }

        var newPoint = CGTools.CreateObject("Point_" + pathNodes.Count, transform).transform;
        newPoint.position = position;
        newPoint.rotation = rotation;
        OnCreateNewCornerObject(newPoint.gameObject);
        var newNode = new PathNode();
        newNode.point = newPoint;
        newNode.speed = 1f;
        pathNodes.Add(newNode);
        return newNode;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showPath)
        {
            return;
        }

        var p = GeneratePath();

        if (p.corners.Length > 1)
        {
            Gizmos.color = pathColor;

            for (int i = 0; i < p.corners.Length - 1; ++i)
            {
                Vector3 from = p.corners[i].position;
                Vector3 to = p.corners[i + 1].position;
                Vector3 left = i == 0 ? from : p.corners[i - 1].position;
                Vector3 right = i == p.corners.Length - 2 ? to : p.corners[i + 2].position;
                Vector3 pt1, pt2;
                BezierCurve.CalculateControlPoint(left, from, to, right, out pt1, out pt2);
                BezierCurve curve = new BezierCurve(from, pt1, pt2, to);
                float len = (to - from).magnitude;
                int count = Mathf.Clamp((int)len * 5, 10, 100);
                Vector3[] sampline = curve.SampleToArray(count);

                for (int j = 0; j < sampline.Length - 1; ++j)
                {
                    Gizmos.DrawLine(sampline[j], sampline[j + 1]);
                }
            }
        }
    }
#endif
}