//using UnityEngine;
//using System.Collections.Generic;


//public class MovementPathBuider : MonoBehaviour
//{
//    private class CornerData
//    {
//        public int index;
//        public MovementCorner corner;
//    }

//    public static MovementPathBuider instance
//    {
//        get 
//        {
//            if (_instance == null)
//            {
//                GameObject obj = new GameObject("MovementPathBuider");
//                _instance = obj.AddComponent<MovementPathBuider>();
//            }

//            return _instance;
//        }
//    }

//    private static MovementPathBuider _instance;

//    public MovementPath path
//    {
//        get 
//        {
//            if (isDirty)
//            {
//                BuildPath();
//                isDirty = false;
//            }

//            return _path;
//        }
//    }

//    private List<CornerData> pathCorners = new List<CornerData>();
//    private bool isDirty = false;
//    private MovementPath _path = new MovementPath();

//    public void RegisterCorner(int index, Vector3 position, Quaternion rotation, float speed)
//    {
//        isDirty = true;

//        MovementCorner corner = new MovementCorner();
//        corner.position = position;
//        corner.rotation = rotation;
//        corner.speed = speed;

//        CornerData data = new CornerData();
//        data.index = index;
//        data.corner = corner;


//        for (int i = 0; i < pathCorners.Count; ++i)
//        {
//            if (data.index <= pathCorners[i].index)
//            {
//                pathCorners.Insert(i, data);
//                return;
//            }
//        }

//        pathCorners.Insert(pathCorners.Count, data);
//    }

//    public void ClearCorners()
//    {
//        isDirty = true;
//        pathCorners.Clear();
//    }

//    private void BuildPath()
//    {
//        _path.corners = new MovementCorner[pathCorners.Count];

//        for (int i = 0; i < pathCorners.Count; ++i)
//        {
//            _path.corners[i] = pathCorners[i].corner;
//        }
//    }
//}