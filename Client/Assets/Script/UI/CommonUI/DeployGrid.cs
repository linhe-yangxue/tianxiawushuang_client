using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class DeployGrid : DynamicGrid
{
    public float duration = 0.5f;
    public bool singleDeploy = true;
    public bool animated = true;
    public GameObject defaultSelected;

    public Action<GameObject, GameObject> onDeploy;
    public Action onDoRepositionFinish;

    private bool locked = false;


    public bool Select(GameObject item)
    {
        if (item == null)
            return false;

        int index = itemList.IndexOf(item);
        DeployGridItem it = item.GetComponent<DeployGridItem>();

        if (index < 0 || locked || it == null)
            return false;

        if (it.deployed == null || !it.deployed.activeSelf)
        {
            GameObject fold = singleDeploy ? GetDeployed() : null;

            if (it.deployed != null)
            {
                if (!itemList.Contains(it.deployed))
                {
                    itemList.Insert(index + 1, it.deployed);
                }

                it.deployed.SetActive(true);

                if (onDeploy != null)
                {
                    onDeploy(item, it.deployed);
                }
            }

            StartCoroutine(DoReposition(it.deployed, fold));
        }
        else
        {
            StartCoroutine(DoReposition(null, it.deployed));
        }

        return true;
    }

    protected void OnEnable()
    {
        locked = false;
        Reposition();
        Select(defaultSelected);
    }

    protected void Start()
    {
        base.Start();
        locked = false;
        Init();
        HideAllDeployed();
        Select(defaultSelected);
    }

    protected void OnDisable()
    {
        HideAllDeployed();
    }

    private IEnumerator DoReposition(GameObject deploy, GameObject fold)
    {
        locked = true;
        float time = animated ? 0f : duration + 1f;

        SetScale(deploy, fold, 0f);

        while (time < duration)
        {
            SetScale(deploy, fold, time / duration);
            Reposition();
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        SetScale(deploy, fold, 1f);
        Reposition();

        if (fold != null)
        {
            fold.SetActive(false);
        }

        locked = false;

        if (onDoRepositionFinish != null)
        {
            onDoRepositionFinish();
        }
    }

    private void SetScale(GameObject deploy, GameObject fold, float scale)
    {
        if (deploy != null)
            deploy.transform.localScale = GetScale(scale);

        if (fold != null)
            fold.transform.localScale = GetScale(1 - scale);
    }

    private Vector3 GetScale(float factor)
    {
        return vertical ? new Vector3(1f, factor, 1f) : new Vector3(factor, 1f, 1f);
    }

    private void Init()
    {
        HashSet<GameObject> set = new HashSet<GameObject>();

        foreach (var item in itemList)
        {
            DeployGridItem it = item.GetComponent<DeployGridItem>();

            if (it != null && it.deployed != null)
            {
                if (set.Contains(it.deployed))
                {
                    GameObject clone = Instantiate(it.deployed) as GameObject;
                    clone.name = it.deployed.name;
                    clone.transform.parent = item.transform;
                    it.deployed = clone;
                }
                else
                {
                    set.Add(it.deployed);
                }
            }
        }
    }

    private void HideAllDeployed()
    {
        foreach (var item in itemList)
        {
            if (item != null)
            {
                DeployGridItem it = item.GetComponent<DeployGridItem>();

                if (it != null && it.deployed != null)
                {
                    it.deployed.SetActive(false);
                }
            }
        }

        Reposition();
    }

    private GameObject GetDeployed()
    {
        foreach (var item in itemList)
        {
            if (item != null)
            {
                DeployGridItem it = item.GetComponent<DeployGridItem>();

                if (it != null && it.deployed != null && it.deployed.activeSelf)
                {
                    return it.deployed;
                }
            }
        }

        return null;
    }
}