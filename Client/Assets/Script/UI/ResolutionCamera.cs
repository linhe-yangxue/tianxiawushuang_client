using UnityEngine;
using System.Collections;

public class ResolutionCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject tempObj = GameCommon.FindObject(gameObject, "Camera");

        if (tempObj != null)
        {
            GlobalModule.SetResolution(tempObj.GetComponent<Camera>());
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
