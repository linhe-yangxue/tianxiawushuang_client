using UnityEngine;
using System.Collections;
using Logic ;
using DataTable ;
using System.Collections.Generic;
using System.Linq ;

public class FateSuccessWindow : tWindow 
{
	public ActiveData curRoleData;
//	int curRoleId = 0;
	int iBodyTid ;
	GameObject card;

	public override void Init ()
	{
		EventCenter.Register ("Button_fate_result_close_button", new DefineFactory<Button_fate_result_close_button>());
	}

	public override void Open (object param)
	{
		base.Open (param);

		GameCommon.FindObject (mGameObjUI, "fate_sucess_title").SetActive (true );
		GameCommon.FindObject (mGameObjUI, "aim_fate_label").SetActive (true );
		GameCommon.FindObject (mGameObjUI, "cur_fate_label").SetActive (true );
		GameCommon.FindObject (mGameObjUI, "break_sucess_title").SetActive (false );
		GameCommon.FindObject (mGameObjUI, "new_talent_info").SetActive (false);
		GameCommon.FindObject (mGameObjUI, "break_result_close_button").SetActive (false);
		GameCommon.FindObject (mGameObjUI, "fate_result_close_button").SetActive (true);
//
//		curRoleId = (int)TeamManager.mCurTeamPos;
//		iBodyTid = TeamManager.GetBodyTidByTeamPos(curRoleId);
//		curRoleData = TeamManager.GetActiveDataByTeamPos (curRoleId );
		curRoleData = (ActiveData)param;
		iBodyTid = curRoleData.tid;
//		curRoleId = curRoleData.itemId;

		InitWindow ();
		Refresh (null );
	}

	public virtual void InitWindow()
	{
		card = GameCommon.FindObject(mGameObjUI, "breakAndFateCard");
		GameCommon.InitCardWithoutBackground(card, 1.0f, mGameObjUI);
	}

	public override bool Refresh(object param)
	{
		UpdaeUI();
		return base.Refresh (param);
	}

	public void UpdaeUI()
	{
		SetCurExpInfo();
		GameCommon.SetCardInfo(card.name, iBodyTid, 0, 0, mGameObjUI);
	}

	public void SetCurExpInfo()
	{
		SetBaseAttribute("attuibute_add(Clone)_", curRoleData.fateLevel);

		GameCommon.SetUIText (mGameObjUI, "cur_level_label","[32cd32]" + curRoleData.fateLevel.ToString () + "[ffffff]级");
		GameCommon.SetUIText (mGameObjUI, "aim_level_label","[32cd32]" + (curRoleData.fateLevel + 1).ToString () + "[ffffff]级");
	}

	public void SetBaseAttribute(string baseObjName, int iFateLevel)
	{
		for (int i = 0; i < 4; i++)
		{
			SetBaseAttribute(i, baseObjName, iFateLevel);
		}
	}
	//set Base attribute infos
	public void SetBaseAttribute(int iIndex, string baseObjName, int iFateLevel)
	{
		if (curRoleData == null)
			return;
		
		GameObject baseObj = GameCommon.FindObject(mGameObjUI , baseObjName + iIndex.ToString());
		// set base attribute number
		SetBaseAttributeValue(iIndex, baseObj, iFateLevel);
	}
	// set base attribute number
	public void SetBaseAttributeValue(int iIndex, GameObject obj, int iStrengthenLevel)
	{
		UILabel curAttributeNameLabel =  obj.transform.Find("attuibute_name_label/cur_attuibute_sprite/label").GetComponent<UILabel >();
		UILabel aimAttributeNameLabel =  obj.transform.Find("attuibute_name_label/aim_attuibute_sprite/label").GetComponent<UILabel >();
		UILabel baseNumLabel =  obj.transform.Find("attuibute_name_label/cur_attuibute_sprite/cur_attuibute_label").GetComponent<UILabel >();
		UILabel addNumLabel =  obj.transform.Find("attuibute_name_label/aim_attuibute_sprite/aim_attuibute_label").GetComponent<UILabel >();
		float fBaseValue = (float)TableCommon.GetNumberFromFateConfig(curRoleData.fateLevel, "ADD_RATE" );

		AFFECT_TYPE affectType = BreakInfoWindow.GetAttributeType((BASE_ATTRITUBE_TYPE)(iIndex + 1));
		curAttributeNameLabel.text = TableCommon.GetStringFromEquipAttributeIconConfig ((int)affectType, "NAME");
		aimAttributeNameLabel.text = TableCommon.GetStringFromEquipAttributeIconConfig ((int)affectType, "NAME");

		if (baseNumLabel != null)
		{
			baseNumLabel.text = "+" + fBaseValue * (curRoleData.fateLevel)+ "%";
			addNumLabel.text = "+" + fBaseValue * (curRoleData.fateLevel + 1) + "%";
		}
	}

	public override void OnClose ()
	{
		base.OnClose ();
		GameObject.Destroy (mGameObjUI);
//		card = GameCommon.FindObject(mGameObjUI, "breakAndFateCard");
//		foreach(Transform child in card.transform)
//		{
//			GameObject.Destroy(child.gameObject);
//		}
	}

}

class Button_fate_result_close_button : CEvent
{
	public override bool _DoEvent ()
	{
		bool isCanFate = true;
		DataCenter.CloseWindow ("FATE_RESULT_WINDOW");
		DataCenter.SetData ("FATE_INFO_WINDOW", "FATE_GO_ON", isCanFate);

		return true;
	}
}