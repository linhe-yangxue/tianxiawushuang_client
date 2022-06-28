using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DynamicGrid : MonoBehaviour
{
    public bool vertical = true;
    public float space = 10f;
    public bool initOnStart = false;

    [HideInInspector]
    public List<GameObject> itemList = new List<GameObject>();

    public virtual Bounds GetItemRelativeBounds(GameObject item)
    {
        return NGUIMath.CalculateRelativeWidgetBounds(item.transform);
    }

    public void Reposition()
    {
        itemList.RemoveAll(x => x == null);

        float currentPos = 0f;

        for (int i = 0; i < itemList.Count; ++i)
        {
            GameObject item = itemList[i];

            if (item != null && item.activeSelf)
            {
                if (item.transform.parent != this.transform)
                {
                    item.transform.parent = this.transform;
                }

                Bounds bounds = GetItemRelativeBounds(item);
                Vector3 pos = item.transform.localPosition;

                if (vertical)
                {
                    float y = -currentPos - bounds.max.y * item.transform.localScale.y;
                    item.transform.localPosition = new Vector3(pos.x, y, pos.z);
                    currentPos += (bounds.size.y + space) * item.transform.localScale.y;
                }
                else
                {
                    float x = currentPos - bounds.min.x * item.transform.localScale.x;
                    item.transform.localPosition = new Vector3(x, pos.y, pos.z);
                    currentPos += (bounds.size.x + space) * item.transform.localScale.x;
                }
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < itemList.Count; ++i)
        {
            GameObject item = itemList[i];

            if (item != null)
                Destroy(item);
        }

        itemList.Clear();
    }

    public void ResetPosition()
    {
        itemList.RemoveAll(x => x == null);

        foreach (var item in itemList)
        {
            item.transform.parent = this.transform;
            item.transform.localScale = Vector3.one;
            item.SetActive(true);
        }

        Reposition();
    }

    protected void Start()
    {
        if (initOnStart)
        {
            itemList.Clear();

            foreach (Transform child in this.transform)
            {
                itemList.Add(child.gameObject);
            }

            Reposition();
        }
    }
}