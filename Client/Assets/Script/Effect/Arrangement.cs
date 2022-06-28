using UnityEngine;
using System.Collections.Generic;


public class Arrangement
{
    public static bool AddThenPlaceSmoothly(List<GameObject> objs, GameObject go, Vector3 offset, float duration)
    {
        if ((objs.Count > 0 && objs[objs.Count - 1] == null) || go == null || objs.Contains(go))
            return false;

        go.transform.localPosition = objs.Count == 0 ? Vector3.zero : objs[objs.Count - 1].transform.localPosition + offset;
        objs.Add(go);
        PlaceSmoothly(objs, offset, duration);
        return true;
    }

    public static bool RemoveThenPlaceSmoothly(List<GameObject> objs, GameObject go, Vector3 offset, float duration)
    {
        if (go == null || !objs.Contains(go))
            return false;

        objs.Remove(go);
        PlaceSmoothly(objs, offset, duration);
        return true;
    }

    public static void Place(IEnumerable<GameObject> objs, Vector3 offset)
    {
        Place(objs, GeneratePositions(objs, offset));
    }

    public static void PlaceSmoothly(IEnumerable<GameObject> objs, Vector3 offset, float duration)
    {
        PlaceSmoothly(objs, GeneratePositions(objs, offset), duration);
    }

    public static void Place(IEnumerable<GameObject> objs, List<Vector3> pos)
    {
        int i = 0;

        foreach (var obj in objs)
        {
            if (obj != null)
            {
                obj.transform.localPosition = pos[i];
            }

            if (++i >= pos.Count)
            {
                break;
            }
        }
    }

    public static void PlaceSmoothly(IEnumerable<GameObject> objs, List<Vector3> pos, float duration)
    {
        int i = 0;

        foreach (var obj in objs)
        {
            if (obj != null)
            {
                TweenPosition.Begin(obj, duration, pos[i]);
            }

            if (++i >= pos.Count)
            {
                break;
            }
        }
    }

    private static List<Vector3> GeneratePositions(IEnumerable<GameObject> objs, Vector3 offset)
    {
        List<Vector3> pos = new List<Vector3>();
        int i = 0;

        foreach (var obj in objs)
        {
            pos.Add(offset * i);
            ++i;
        }

        return pos;
    }
}