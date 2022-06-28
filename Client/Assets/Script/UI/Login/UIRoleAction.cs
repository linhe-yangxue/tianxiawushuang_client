using UnityEngine;
using System.Collections;

public class UIRoleAction : MonoBehaviour {

    // RoleUIConfig Index
    public int mModelIndexOfUIConfig = 0;
    public BaseObject mModel;
	// Use this for initialization
	void Start () {
	
	}

    void OnClick()
    {
        DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "SELECT_ROLE", mModelIndexOfUIConfig);
        foreach(BaseObject obj in ObjectManager.Self.mObjectMap)
        {
            if (obj!=null && obj!=mModel)
                obj.ShowOutline(false);
        }
        mModel.ShowOutline(true);
        Logic.tEvent evt = Logic.EventCenter.Self.StartEvent("RoleSelUI_PlayIdleEvent");
        mModel.PlayMotion("attack", evt);
		GameObject button = GameCommon.FindUI("SelectRoleOKButton");
		if (button!=null)
			button.SetActive(true);
    }

    void OnDrag(Vector2 delta)
    {
        transform.Rotate(new Vector3(0, -delta.x, 0));
    }
}
