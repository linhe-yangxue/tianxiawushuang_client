using UnityEngine;
using System.Collections;
using Logic ;
using DataTable ;
using System.Collections.Generic;
using System.Linq ;
using System;

public class BreakSuccessWindow : tWindow
{
	public ActiveData curRoleData;
//	int curRoleId = 0;
//	int iBodyTid ;
	GameObject card;
	
	public override void Init ()
	{
		EventCenter.Register ("Button_break_result_close_button", new DefineFactory<Button_break_result_close_button>());
	}
	
	public override void Open (object param)
	{
		base.Open ((ActiveData)param);
		curRoleData = (ActiveData)param;

		GameCommon.FindObject (mGameObjUI, "fate_sucess_title").SetActive (false );
		GameCommon.FindObject (mGameObjUI, "aim_fate_label").SetActive (false );
		GameCommon.FindObject (mGameObjUI, "cur_fate_label").SetActive (false );
		GameCommon.FindObject (mGameObjUI, "break_sucess_title").SetActive (true );
		GameCommon.FindObject (mGameObjUI, "new_talent_info").SetActive (true);
		GameCommon.FindObject (mGameObjUI, "break_result_close_button").SetActive (true);
		GameCommon.FindObject (mGameObjUI, "fate_result_close_button").SetActive (false);

//		curRoleId = (int)TeamManager.mCurTeamPos;
//		iBodyTid = TeamManager.GetBodyTidByTeamPos(curRoleId);
//		curRoleData = TeamManager.GetActiveDataByTeamPos (curRoleId );

		InitWindow();
		Refresh (null );
	}

	public virtual void InitWindow()
	{
		card = GameCommon.FindObject(mGameObjUI, "breakAndFateCard");
		GameCommon.InitCardWithoutBackground(card, 1.0f, mGameObjUI);
	}

	public override bool Refresh(object param)
	{
		UpdateUI();
		return base.Refresh (param);
	}
	
	public void UpdateUI()
	{
		AddNewTalent();
		SetCurExpInfo();
		UpdateBodyInfo();
	}

	public void UpdateBodyInfo()
	{
		// card
		GameCommon.SetCardInfo(card.name, curRoleData.tid, 0, 0, mGameObjUI);
	}
	
	//新增天赋
	public void AddNewTalent()
	{
		GameObject talentNameObj = GameCommon.FindObject (mGameObjUI, "new_talent_info").gameObject;
		UILabel newTalentName = talentNameObj.transform.Find ("new_talent_name_label").GetComponent<UILabel>();
		UILabel talentNameLabel =talentNameObj.transform.Find ("talent_attribute_label").GetComponent<UILabel>();
		int iBuffTid = TableCommon.GetNumberFromBreakBuffConfig (curRoleData.tid, "BREAK_" + (curRoleData.breakLevel).ToString ());
		
		newTalentName.text = TableCommon.GetStringFromAffectBuffer (iBuffTid, "NAME");
		talentNameLabel.text = TableCommon.GetStringFromAffectBuffer (iBuffTid, "INFO");
	}
	public void SetCurExpInfo()
	{
		GameObject curBaseObj = GameCommon.FindObject(mGameObjUI , "attuibute_change_grid");
		UIGridContainer attuibuteGrid = curBaseObj.GetComponent<UIGridContainer>();

		for(int i = 0; i < 4; i++)
		{
			GameObject obj = attuibuteGrid.controlList[i];
			UILabel curAttributeName =  obj.transform.Find("attuibute_name_label/cur_attuibute_sprite/label").GetComponent<UILabel >();
			UILabel aimAttributeName =  obj.transform.Find("attuibute_name_label/aim_attuibute_sprite/label").GetComponent<UILabel >();

			AFFECT_TYPE affectType = BreakInfoWindow.GetAttributeType((BASE_ATTRITUBE_TYPE)(i + 1));
			curAttributeName.text = TableCommon.GetStringFromEquipAttributeIconConfig ((int)affectType, "NAME");
			aimAttributeName.text = TableCommon.GetStringFromEquipAttributeIconConfig ((int)affectType, "NAME");
		}

		setAttackValue(curBaseObj);
		setPhysicalValue (curBaseObj);
		setHPValue (curBaseObj);
		setMagicValue (curBaseObj);
	}
	//攻击
	public void setAttackValue(GameObject obj)
	{
//		UILabel attributeName=  obj.transform.Find("attuibute_add(Clone)_0/attuibute_name_label/cur_attuibute_sprite/label").GetComponent<UILabel >();
//		attributeName.text = "攻击：";
		UILabel srcAttributeValue =  obj.transform.Find("attuibute_add(Clone)_2/attuibute_name_label/cur_attuibute_sprite/cur_attuibute_label").GetComponent<UILabel >();
		UILabel curAttributeValue =  obj.transform.Find("attuibute_add(Clone)_2/attuibute_name_label/aim_attuibute_sprite/aim_attuibute_label").GetComponent<UILabel >();

        curRoleData.breakLevel--;
        int srcExpNum = GameCommon.GetTotalAttack(curRoleData);

        curRoleData.breakLevel++;
        int curExpNum = GameCommon.GetTotalAttack(curRoleData);

        srcAttributeValue.text = srcExpNum.ToString();
        curAttributeValue.text = curExpNum.ToString();
	}
	//物防
	public void setPhysicalValue(GameObject obj)
	{
//		UILabel attributeName=  obj.transform.Find("attuibute_add(Clone)_1/attuibute_name_label/cur_attuibute_sprite/label").GetComponent<UILabel >();
//		attributeName.text = "物防：";

		UILabel srcAttributeValue =  obj.transform.Find("attuibute_add(Clone)_1/attuibute_name_label/cur_attuibute_sprite/cur_attuibute_label").GetComponent<UILabel >();
		UILabel curAttributeValue =  obj.transform.Find("attuibute_add(Clone)_1/attuibute_name_label/aim_attuibute_sprite/aim_attuibute_label").GetComponent<UILabel >();
        
        curRoleData.breakLevel--;
        int srcExpNum = GameCommon.GetTotalPhysicalDefence(curRoleData);

        curRoleData.breakLevel++;
        int curExpNum = GameCommon.GetTotalPhysicalDefence(curRoleData);

        srcAttributeValue.text = srcExpNum.ToString();
        curAttributeValue.text = curExpNum.ToString();
	}
	//生命
	public void setHPValue(GameObject obj)
	{
//		UILabel attributeName=  obj.transform.Find("attuibute_add(Clone)_2/attuibute_name_label/cur_attuibute_sprite/label").GetComponent<UILabel >();
//		attributeName.text = "生命：";

		UILabel srcAttributeValue =  obj.transform.Find("attuibute_add(Clone)_3/attuibute_name_label/cur_attuibute_sprite/cur_attuibute_label").GetComponent<UILabel >();
		UILabel curAttributeValue =  obj.transform.Find("attuibute_add(Clone)_3/attuibute_name_label/aim_attuibute_sprite/aim_attuibute_label").GetComponent<UILabel >();

        curRoleData.breakLevel--;
        int srcExpNum = GameCommon.GetTotalMaxHP(curRoleData);

        curRoleData.breakLevel++;
        int curExpNum = GameCommon.GetTotalMaxHP(curRoleData);

        srcAttributeValue.text = srcExpNum.ToString();
        curAttributeValue.text = curExpNum.ToString();
	}
	//法防
	public void setMagicValue(GameObject obj)
	{
//		UILabel attributeName=  obj.transform.Find("attuibute_add(Clone)_3/attuibute_name_label/cur_attuibute_sprite/label").GetComponent<UILabel >();
//		attributeName.text = "法防：";

		UILabel srcAttributeValue =  obj.transform.Find("attuibute_add(Clone)_0/attuibute_name_label/cur_attuibute_sprite/cur_attuibute_label").GetComponent<UILabel >();
		UILabel curAttributeValue =  obj.transform.Find("attuibute_add(Clone)_0/attuibute_name_label/aim_attuibute_sprite/aim_attuibute_label").GetComponent<UILabel >();

        curRoleData.breakLevel--;
        int srcExpNum = GameCommon.GetTotalMagicDefence(curRoleData);

        curRoleData.breakLevel++;
        int curExpNum = GameCommon.GetTotalMagicDefence(curRoleData);

        srcAttributeValue.text = srcExpNum.ToString();
        curAttributeValue.text = curExpNum.ToString();
	}
	public int SetExpNum(int iBasic, int iAddBasic, int iAddBreakNum, int iBreakLevel)
	{
		int breakBaseValue = iBasic + iAddBasic * iBreakLevel;
		int breakUpgradeValue = breakBaseValue + iAddBreakNum * curRoleData.level * iBreakLevel;
		
		return breakUpgradeValue;
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

class Button_break_result_close_button : CEvent
{
	public override bool _DoEvent ()
	{
		TeamInfoWindow _tWindow = DataCenter.GetData("TEAM_INFO_WINDOW") as TeamInfoWindow;
		BreakSuccessWindow _breTwin = DataCenter.GetData ("BREAK_RESULT_WINDOW") as BreakSuccessWindow;
		if(_tWindow != null)
		{
//			GlobalModule.DoCoroutine(_tWindow.SetTipsAttribute(_breTwin.curRoleData));
            DataCenter.Set("CHANGE_TIP_PANEL_TYPE", ChangeTipPanelType.BREAK_INFO_WINDOW);
			_tWindow.SetTipsAttribute(_breTwin.curRoleData);
			_tWindow.ChangeFitting();

            BreakInfoWindow _tBreakWin = DataCenter.GetData("BREAK_INFO_WINDOW") as BreakInfoWindow;
            ChangeTipManager.Self.PlayAnim(() => 
            {
                if (_tBreakWin != null)
                {
                    _tBreakWin.UpdateNextBreakLevelInfo();
                }
            });

            GameObject _curPetObj = _tWindow.mCardList[(int)TeamManager.mCurTeamPos];
            if (_curPetObj != null)
            {
                GameObject _aureoleObj = GameCommon.FindObject(_curPetObj, "pet_orange(Clone)");
                if (_aureoleObj != null)
                {
                    _aureoleObj.SetActive(true);
                }
            }
         
		}

		DataCenter.CloseWindow ("BREAK_RESULT_WINDOW");
		
		return true;
	}
}
