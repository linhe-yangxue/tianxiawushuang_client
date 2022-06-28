using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

//public class InfoGroupUI : MonoBehaviour {

//    // Use this for initialization
//    void Start () {
	
//    }
	
//    // Update is called once per frame
//    void Update () {
	
//    }
//}
//public class InfoGroupsWindow:tWindow 
//{
//    
//    public override void Open(object param)
//    {
//        DataCenter.OpenWindow("INFO_GROUP_WINDOW");
//    }
//}

public class InfoGroupWindow:tWindow
{
    private UILabel mStaminaNum;
    private UILabel mGoldNum;
    private UILabel mDiamondNum;

    //public override void Init()

    //{
    //    mGameObjUI = GameObject.Find(CommonParam.UIRootName);
    //}

    public override void OnOpen()
    {
		base.OnOpen();

        Refresh(null);
    }

    public override bool Refresh(object param)
    {
        InitInfoGroup();

        return true;
    }

    private void InitInfoGroup()
    {
		if(mGameObjUI == null)
			return;

//        GameObject playerInfoObj = GameCommon.FindObject(mGameObjUI, "info_group_window");

		GameObject vnObj = GameCommon.FindObject(mGameObjUI, "AddStaminaBtn");
        mStaminaNum = GameCommon.FindObject(vnObj, "StaminaNum").GetComponent<UILabel >();

		GameObject gnObj = GameCommon.FindObject(mGameObjUI, "AddGoldBtn");
        mGoldNum = GameCommon.FindObject(gnObj, "GoldNum").GetComponent<UILabel>();
		GameObject dnObj = GameCommon.FindObject(mGameObjUI, "AddDiamondBtn");
        mDiamondNum = GameCommon.FindObject(dnObj, "DiamondNum").GetComponent<UILabel>();

        UpdateStaminaUI();
        UpdateGoldUI();
        UpdateDiamondUI();
		UpdataSpirit();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if (keyIndex == "UPDATE_VITAKITY")
        {
            UpdateStaminaUI();
        }
        if (keyIndex == "UPDATE_GOLD")
        {
            UpdateGoldUI();
        }
        if (keyIndex == "UPDATE_DIAMOND")
        {
            UpdateDiamondUI();
        }
		if (keyIndex == "UPDATE_FRIEND_POINT")
		{
			UpdataSpirit();
		}
//		if(keyIndex == "UPDATE_ROLE_SELECT_SCENE")
//		{
//			CheckMailMark();
//		}
		if(keyIndex == "UPDATE_CHAT_MARK")
		{
			CheckChatMark((bool)objVal);
		}
    }

	public void CheckChatMark(bool bVisible)
	{
        //by chenliang
        //begin

//		GameObject newMarkObj = mGameObjUI.transform.Find("InfoGroup/ChatBtn/NewMark").gameObject;
//---------------
        GameObject newMarkObj = mGameObjUI.transform.Find("transform_tween_root/transform_root/InfoGroup/ChatBtn/NewMark").gameObject;

        //end
		newMarkObj.SetActive (bVisible);
	}

	public void CheckMailMark()
	{
		if(mGameObjUI == null)
			return;

        GameObject mailNum = mGameObjUI.transform.Find("transform_tween_root/transform_root/InfoGroup/MailBtn/mail_num_label").gameObject;
		RoleLogicData RoleData = RoleLogicData.Self;
		if(RoleData.mMailNum != 0)
		{
//			mailNum.SetActive (true);
			mailNum.GetComponent<UILabel>().text = RoleData.mMailNum.ToString ();
		}
		else
			mailNum.SetActive (false);

        GameObject newMarkObj = mGameObjUI.transform.Find("transform_tween_root/transform_root/InfoGroup/MailBtn/NewMark").gameObject;
		newMarkObj.SetActive (System.Convert.ToBoolean (RoleData.mMailNum));
	}

    public void UpdateStaminaUI()
    {
		if(mStaminaNum != null)
			mStaminaNum.text = RoleLogicData.Self.GetUpdateStaminaString();
	}
	
	public void UpdateGoldUI()
    {
		if(mGoldNum != null)
		{
			if(RoleLogicData.Self.gold / 100000000 > 0)
			{
				mGoldNum.text = (RoleLogicData.Self.gold / 100000000).ToString() + "亿";
			}
			else if(RoleLogicData .Self.gold/1000000>0)
			{
				mGoldNum.text = (RoleLogicData.Self.gold / 10000).ToString() + "万";
			}
			else
			{
				mGoldNum.text = GameCommon.ShowNumUI(RoleLogicData.Self.gold);
			}
		}
			
    }

    public void UpdateDiamondUI()
    {
		if(mDiamondNum != null)
		{
			//mDiamondNum.text = GameCommon.ShowNumUI(RoleLogicData.Self.diamond);
			mDiamondNum.text = RoleLogicData.Self.diamond.ToString();
			if(RoleLogicData.Self.diamond / 100000000 > 0)
			{
				mDiamondNum.text = (RoleLogicData.Self.diamond / 100000000).ToString() + "亿";
			}
			else if(RoleLogicData.Self.diamond / 1000000 > 0)
			{
				mDiamondNum.text=(RoleLogicData.Self.diamond / 10000).ToString ()+"万";
			}
			else
			{
				mDiamondNum.text = GameCommon.ShowNumUI(RoleLogicData.Self.diamond);
			}
		}
			
    }

	public void UpdataSpirit()
	{
        SetText("friendly_number", RoleLogicData.Self.GetSpiritString());
	}
}
