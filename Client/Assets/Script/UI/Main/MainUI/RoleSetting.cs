using UnityEngine;
using System;
using System.Collections;

public class RoleSetting : SceneBase {

	public int miIndex = 0;
    //by chenliang
    //begin

    private static bool mIsInited = false;        //是否已经初始化
    public static bool IsInited
    {
        set { mIsInited = value; }
        get { return mIsInited; }
    }

    //end

	public override void Awake()
	{
		base.Awake();

		MainRoleSceneWindow window = new MainRoleSceneWindow();
		DataCenter.RegisterData("MAIN_ROLE_SCENE_WINDOW", window);
		DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "CLEAR_GRID", true);
	}
	// Use this for initialization
    //by chenliang
    //begin

// 	void Start () {
// 		if(MainUIScript.mCurIndex != MAIN_WINDOW_INDEX.RoleSelWindow)
// 		{
// 			MainUIScript.Self.OpenMainWindowByIndex(MainUIScript.mCurIndex);
// 		}
// 		else
// 		{
// 			DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "INIT_ROLE_SELECT", miIndex);
// 		}
// 
//         //Preloader.PreloadWorldMap();
// 		DataCenter.SetData("MAIN_SCENE_LOADING_WINDOW", "CONTINUE", true);
// 
//         if (MainUIScript.mLoadingFinishAction != null)
//         {
//             MainUIScript.mLoadingFinishAction();
//             MainUIScript.mLoadingFinishAction = null;
//         }
// 	}
//--------------
    void Update()
    {
        if (!mIsInited)
            return;
        mIsInited = false;

        if (MainUIScript.mCurIndex != MAIN_WINDOW_INDEX.RoleSelWindow)
        {
            MainUIScript.Self.OpenMainWindowByIndex(MainUIScript.mCurIndex);
        }
        else
        {
            DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "INIT_ROLE_SELECT", miIndex);
        }

        //Preloader.PreloadWorldMap();
        DataCenter.SetData("MAIN_SCENE_LOADING_WINDOW", "CONTINUE", true);

        if (MainUIScript.mLoadingFinishAction != null)
        {
            MainUIScript.mLoadingFinishAction();
            MainUIScript.mLoadingFinishAction = null;
        }
    }

    //end

	void OnDestroy()
	{
		DataCenter.Remove("MAIN_ROLE_SCENE_WINDOW");
	}
}

public class MainRoleSceneWindow : tWindow {
	

}

