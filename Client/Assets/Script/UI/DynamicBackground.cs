using UnityEngine;
using System.Collections.Generic;


public class DynamicBackground : MonoBehaviour, IAdjustable
{
    public enum Arrangement
    {
        Horizontal,
        Vertical,
    }

    public int offset = 10;
    public Arrangement arrangement = Arrangement.Vertical;
    public int manualPriority = 0;
    public Transform[] content;

    private UIWidget background;

    public int priority { get; private set; }

    public void Adjust()
    {
        List<Transform> children;

        if (content == null || content.Length == 0)
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
        }
        else 
        {
            children = new List<Transform>(content);
            children.RemoveAll(x => x == null || !x.gameObject.activeSelf);
        }
        
        if (background == null)
        {
            background = GetComponent<UIWidget>();
        }

        if (background == null || children.Count <= 0)
        {
            return;
        }

        Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(transform, children[0]);

        for (int i = 1; i < children.Count; ++i)
        {
            bounds.Encapsulate(NGUIMath.CalculateRelativeWidgetBounds(transform, children[i]));
        }

        if (arrangement == Arrangement.Vertical)
        {
            background.height = (int)bounds.size.y + offset;
        }
        else 
        {
            background.width = (int)bounds.size.x + offset;
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