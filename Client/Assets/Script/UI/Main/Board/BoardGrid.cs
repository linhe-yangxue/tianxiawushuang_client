using UnityEngine;
using System;
using System.Collections;
using Logic;
using Utilities;


public class BoardGrid : DynamicGrid
{
    public float duration = 0.5f;
    public int maxCount = 0;
    public GameObject itemTemplate;
    public GameObject describeTemplate;
    public UIScrollView scrollView;

    public Func<GameObject, GameObject, bool> onInitDesc;

    private GameObject _selectedItem;
    private GameObject _describeLabel;
    private GameObject _fadeOut;

    public int count
    {
        get 
        { 
            return itemList.Count; 
        }
        set 
        {
            if (value != itemList.Count)
            {
                Clear();

                if (itemTemplate != null)
                {
                    for (int i = 0; i < value; ++i)
                    {
                        GameObject item = Create(itemTemplate);                       
                        InitItem(item);
                        itemList.Add(item);
                    }

                    Reposition();
                }
            }
        }
    }

    private void OnSelect(GameObject item)
    {
        if (item == null || describeTemplate == null)
            return;
      
        int index = itemList.IndexOf(item);

        if (index < 0)
            return;

        if (_selectedItem == item)
        {
            _selectedItem = null;
            StopReposition();
            StartCoroutine(StartReposiion(null, _describeLabel));           
        }
        else
        {
            _selectedItem = item;
            GameObject newDescribe = Create(describeTemplate);
            InitCollider(newDescribe);
            
            if (onInitDesc == null || onInitDesc(newDescribe, item))
            {
                itemList.Insert(index + 1, newDescribe);
                StopReposition();
                StartCoroutine(StartReposiion(newDescribe, _describeLabel));
            }
            else
            {
                DestroyImmediate(newDescribe);
                StopReposition();               
                StartCoroutine(StartReposiion(null, _describeLabel));
            }
        }
    }

    private GameObject Create(GameObject template)
    {
        if (template == null)
            return null;

        GameObject inst = Instantiate(template) as GameObject;
        inst.name = template.name;
        inst.transform.parent = this.transform;
        inst.transform.localScale = Vector3.one;
        inst.SetActive(true);
        return inst;
    }

    private void OnEnable()
    {
        count = maxCount;
    }

    private void OnDisable()
    {
        StopReposition();
        Clear();
    }

    private void StopReposition()
    {
        StopAllCoroutines();

        if (_describeLabel != null)
        {
            _describeLabel.transform.localPosition = Vector3.one;
        }
        if (_fadeOut != null)
        {
            itemList.Remove(_fadeOut);
            Destroy(_fadeOut);
        }

        Reposition();
    }

    private IEnumerator StartReposiion(GameObject fadeIn, GameObject fadeOut)
    {
        float time = 0f;
        _describeLabel = fadeIn;
        _fadeOut = fadeOut;

        while (time < duration)
        {
            float factor = time / duration;
            factor = 1f - (1f - factor) * (1f - factor);

            if(fadeIn != null)
                fadeIn.transform.localScale = vertical ? new Vector3(1f, factor, 1f) : new Vector3(factor, 1f, 1f);

            if(fadeOut != null)
                fadeOut.transform.localScale = vertical ? new Vector3(1f, 1f - factor, 1f) : new Vector3(1f - factor, 1f, 1f);

            Reposition();
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        if(fadeIn != null)
            fadeIn.transform.localScale = Vector3.one;

        if (fadeOut != null)
        {
            itemList.Remove(fadeOut);
            Destroy(fadeOut);
        }

        _fadeOut = null;
        Reposition();

        if (scrollView != null)
            scrollView.RestrictWithinBounds(false);
    }

    private void InitCollider(GameObject obj)
    {
        if (obj.GetComponent<Collider>() == null)
            NGUITools.AddWidgetCollider(obj);

        if (obj.GetComponent<UIDragScrollView>() == null)
            obj.AddComponent<UIDragScrollView>();
    }

    private void InitItem(GameObject item)
    {
        InitCollider(item);

        if (item.GetComponent<OnClickListener>() == null)
            item.AddComponent<OnClickListener>().onClick += () => OnSelect(item);
    }
}