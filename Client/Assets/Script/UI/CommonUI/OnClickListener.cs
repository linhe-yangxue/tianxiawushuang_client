using UnityEngine;
using System;


public class OnClickListener : MonoBehaviour
{
    public event Action onClick = null;

    private void OnClick()
    {
        if (onClick != null)
            onClick();
    }
}