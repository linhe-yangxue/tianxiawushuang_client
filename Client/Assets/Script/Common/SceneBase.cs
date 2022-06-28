using UnityEngine;
using System.Collections;

public class SceneBase : MonoBehaviour {

	public virtual void Awake()
	{
        //by chenliang
        //begin

//		Camera camera = GetComponent<Camera>();
//		GlobalModule.SetResolution(camera);
//--------------
        //增加自动调整摄像机视口
        if (gameObject.GetComponent<AutoCameraResolution>() == null)
            gameObject.AddComponent<AutoCameraResolution>();

        //end
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
