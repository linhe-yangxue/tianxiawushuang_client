using UnityEngine;
using System.Collections;

public class SelectScene : SceneBase {

	public override void Awake()
	{
		base.Awake();
		SelectSceneWindow selectSceneWindow = new SelectSceneWindow();
		selectSceneWindow.mGameObjUI = gameObject;
		DataCenter.RegisterData("SELECT_SCENE_WINDOW", selectSceneWindow);
	}

	// Use this for initialization
	void Start () {
		DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "INIT_ROLE_SELECT", true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDestroy()
	{
		DataCenter.Remove("SELECT_SCENE_WINDOW");
	}
}


public class SelectSceneWindow : tWindow
{
	public override void Init()
	{

	}
	
	public override void OnOpen()
	{		

	}
	
	public override void Close ()
	{
	}
	
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);

	}

}
