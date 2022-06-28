using UnityEngine;
using System.Collections.Generic;

public class BoardButton : MonoBehaviour
{
    public string window { get; set; }

    public void OnClick()
    {
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
    }
}