using UnityEngine;
using System.Collections.Generic;


public interface IAdjustable
{
    int priority { get; }
    void Adjust();
}


public class DynamicArrangment : Singleton<DynamicArrangment>
{
    public static void Adjust(IAdjustable a)
    {
        Instace.WillAdjust(a);
    }

    public static void AdjustImmediate(GameObject root)
    {
        List<IAdjustable> targetList = new List<IAdjustable>();

        var arrangments = root.GetComponentsInChildren<DynamicGridContainer>();
        
        for (int i = 0; i < arrangments.Length; ++i)
        {
            SortedInsert(arrangments[i], targetList);
        }

        var backgrounds = root.GetComponentsInChildren<DynamicBackground>();

        for (int i = 0; i < backgrounds.Length; ++i)
        {
            SortedInsert(backgrounds[i], targetList);
        }

        for (int i = 0; i < targetList.Count; ++i)
        {
            targetList[i].Adjust();
        }
    }

    private static void SortedInsert(IAdjustable a, List<IAdjustable> sortedList)
    {
        if (sortedList.Contains(a))
        {
            return;
        }

        for (int i = 0; i < sortedList.Count; ++i)
        {
            if (a.priority >= sortedList[i].priority)
            {
                sortedList.Insert(i, a);
                return;
            }
        }

        sortedList.Insert(sortedList.Count, a);
    }

    private List<IAdjustable> adjustList = new List<IAdjustable>();

    private void WillAdjust(IAdjustable a)
    {
        SortedInsert(a, adjustList);
    }

    private void LateUpdate()
    {
        if (adjustList.Count > 0)
        {
            foreach (var a in adjustList)
            {
                a.Adjust();
            }

            adjustList.Clear();
        }
    }
}