using UnityEngine;
using System;


public class DeployGridItem : MonoBehaviour
{
    public GameObject deployed;

    private void Start()
    {
        NGUITools.AddWidgetCollider(this.gameObject);
    }

    private void OnClick()
    {
        if (this.transform.parent != null)
        {
            DeployGrid grid = this.transform.parent.GetComponent<DeployGrid>();

            if (grid != null)
            {
                grid.Select(this.gameObject);
            }
        }
    }
}