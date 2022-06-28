using UnityEngine;
using System.Collections;

public class BaseUI : MonoBehaviour {

	public static string mWindowName = "";
	public static object mOpenParam;

	public static BaseUI self;
	// Use this for initialization
    void Start()
    {
		self = this;
    }

    static public bool OpenWindow(string willOpenWindow, object param, bool bClearAll, bool isDestroy = false)
    {
        if (bClearAll)
        {
            GlobalModule.ClearAllWindow(isDestroy);//mInstance.LoadScene("base_ui", !bClearAll);
        }

        DataCenter.OpenWindow(willOpenWindow, param);
        return true;
    }
}
