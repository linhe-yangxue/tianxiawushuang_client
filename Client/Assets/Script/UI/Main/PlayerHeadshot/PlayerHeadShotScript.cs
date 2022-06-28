using UnityEngine;
using System.Collections;

public class PlayerHeadShotScript : MonoBehaviour {

    public NiceData mData = new NiceData();
    string mWinName = string.Empty;
    private BoxCollider mCollider = null;   

    public static string str_root_name = "root_name";
    public static string str_player_info = "player_info";

	// Use this for initialization
	void Start () {
	}

    void OnEnable(){
    }

	// Update is called once per frame
	void Update () {	
	}

    public void OnClick()
    {
        if (IsAttachedData)
        {
            DataCenter.OpenWindow(UIWindowString.player_info_window, mData);
        }
    }

    public void AttachData(sPlayerInfo data)
    {
        GameObject headShot = GameCommon.FindObject(gameObject, "headshot");
        if (headShot)
        {
            mData.set(str_player_info, data);
            GameCommon.SetRoleIcon(headShot.GetComponent<UISprite>(), int.Parse(data.tid), GameCommon.ROLE_ICON_TYPE.PHOTO);
        }
    }

    public void ClearData()
    {
        mData = new NiceData();
    }

    private bool IsAttachedData
    {
        get { return !mData.get(str_player_info).Empty(); }
    }
}
