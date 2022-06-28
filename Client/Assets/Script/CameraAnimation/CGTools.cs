using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities.Routines;


public class CGTools : MonoBehaviour
{
    private static bool hasInit = false;
    private static List<BaseObject> createdList = new List<BaseObject>();
    private static List<BaseObject> movedList = new List<BaseObject>();
    private static List<IRoutine> routineList = new List<IRoutine>();

    public static void InitGameData()
    {
        if (Application.isPlaying && !hasInit)
        {
            hasInit = true;
            GlobalModule.InitGameData();

            var worldCenter = GameCommon.GetMainCameraObj();

            if (worldCenter == null)
            {
                worldCenter = new GameObject("world_center");
            }

            GameObject obj = GameCommon.LoadAndIntanciatePrefabs("Prefabs/Camera_Z", worldCenter);
            string name = obj.name.Replace("(Clone)", "");
            obj.name = name;
            MainProcess.mMainCamera = GameCommon.GetMainCamera();

            worldCenter.StartRoutine(DoProcess());
        }
    }

    public static GameObject CreateObject(string name, Transform parent)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Component.DestroyImmediate(obj.GetComponent<Collider>());
        obj.AddComponent<HideOnAwake>();
        obj.name = name;
        obj.transform.parent = parent;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one * 0.5f;
        return obj;
    }

    public static BaseObject CreateBaseObject(int index)
    {
        if (index > 0)
        {
            var obj = ObjectManager.Self.CreateObject(index);

            if (obj != null)
            {
                obj.SetVisible(true);
                obj.mbSmoothRotation = false;
                (obj as ActiveObject).mWarnEffect.SetVisible(false);
                createdList.Add(obj);
            }

            return obj;
        }

        return null;
    }

    public static void TagAsPathMoved(BaseObject target)
    {
        if (!createdList.Contains(target) && !movedList.Contains(target))
        {
            target.mbSmoothRotation = false;
            movedList.Add(target);
        }
    }

    public static void TagAsCGRoutine(IRoutine routine)
    {
        if (routine != null && !routineList.Contains(routine))
        {
            routineList.Add(routine);
        }
    }

    public static void ClearCG()
    {
        foreach (var r in routineList)
        {
            r.Break();
        }

        routineList.Clear();

        foreach (var obj in createdList)
        {
            obj.Destroy();
            obj.OnDestroy();
        }

        createdList.Clear();

        foreach (var obj in movedList)
        {
            if (obj != null && obj.mMainObject != null)
            {
                RaycastHit hit;

                if (GuideKit.RaycastToObstruct(obj.GetPosition(), out hit))
                {
                    obj.SetPosition(hit.point);
                    obj.SetDirection(obj.GetPosition() + obj.mMainObject.transform.forward);
                    obj.mbSmoothRotation = true;
                    NavMeshAgent agent = obj.mMainObject.GetComponent<NavMeshAgent>();

                    if (agent != null)
                        agent.enabled = true;
                }
            }
        }

        movedList.Clear();
    }

    private static IEnumerator DoProcess()
    {
        while (true)
        {
            Logic.EventCenter.Self.Process(Time.deltaTime);
            yield return null;
        }
    }
}