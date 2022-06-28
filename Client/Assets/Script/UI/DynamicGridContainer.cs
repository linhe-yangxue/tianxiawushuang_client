using UnityEngine;
using System.Collections.Generic;


public class DynamicGridContainer : MonoBehaviour, IAdjustable
{
    public enum Arrangement
    {
        Horizontal,
        Vertical,
    }

    public Arrangement arrangement = Arrangement.Vertical;
    public float space = 0f;
    public int manualPriority = 0;
    public Transform[] controlList;

    public int priority { get; private set; }

    private int CompareHorizontal(Transform lhs, Transform rhs)
    {
        if (lhs == rhs)
            return 0;

        Bounds lBounds = NGUIMath.CalculateAbsoluteWidgetBounds(lhs);
        Bounds rBounds = NGUIMath.CalculateAbsoluteWidgetBounds(rhs);
        return lBounds.min.x < rBounds.min.x ? -1 : 1;
    }

    private int CompareVertical(Transform lhs, Transform rhs)
    {
        if (lhs == rhs)
            return 0;

        Bounds lBounds = NGUIMath.CalculateAbsoluteWidgetBounds(lhs);
        Bounds rBounds = NGUIMath.CalculateAbsoluteWidgetBounds(rhs);
        return lBounds.max.y > rBounds.max.y ? -1 : 1;
    }

    public void Adjust()
    {
        List<Transform> children;

        if (controlList == null || controlList.Length == 0)
        {
            children = new List<Transform>();

            for (int i = 0; i < transform.childCount; ++i)
            {
                var t = transform.GetChild(i);

                if (t.gameObject.activeSelf)
                {
                    children.Add(t);
                }
            }

            if (arrangement == Arrangement.Horizontal)
            {
                children.Sort(CompareHorizontal);
            }
            else 
            {
                children.Sort(CompareVertical);
            }
        }
        else 
        {
            children = new List<Transform>(controlList);
            children.RemoveAll(x => x == null || !x.gameObject.activeSelf);
        }

        if (children.Count <= 1)
            return;

        if (arrangement == Arrangement.Horizontal)
        {
            Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(children[0]);
            float pos = bounds.min.x;

            for (int i = 1; i < children.Count; ++i)
            {
                pos += bounds.size.x + space * transform.lossyScale.x;
                bounds = NGUIMath.CalculateAbsoluteWidgetBounds(children[i]);
                float offset = pos - bounds.min.x;
                children[i].transform.position += new Vector3(offset, 0f, 0f);
            }
        }
        else
        {
            Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(children[0]);
            float pos = bounds.max.y;

            for (int i = 1; i < children.Count; ++i)
            {
                pos -= bounds.size.y + space * transform.lossyScale.y;
                bounds = NGUIMath.CalculateAbsoluteWidgetBounds(children[i]);
                float offset = pos - bounds.max.y;
                children[i].transform.position += new Vector3(0f, offset, 0f);
            }
        }
    }

    private void OnEnable()
    {
        if (manualPriority > 0)
        {
            priority = manualPriority;
        }
        else
        {
            priority = 0;
            Transform t = transform.parent;

            while (t != null)
            {
                ++priority;
                t = t.parent;
            }
        }

        DynamicArrangment.Adjust(this);
    }
}