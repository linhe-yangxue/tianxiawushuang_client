using UnityEngine;
using System.Collections;
using Logic ;
using DataTable ;
using System.Collections.Generic;
using System.Linq ;
using System;

public class FateInfoWindow : tWindow 
{
	public ActiveData curRoleData;
	ConsumeItemLogicData consumeItemLogicData;
	public ConsumeItemData curConsumeItemData;
	public int fateNeedCoin = 0;
	int iBodyTid ;
	int curExp = 0;
	int iMaxFateLevel = -1;  //天命等级从0开始， 所以为初始值为“-1”。
	bool isCanFate = true;

	public override void Init ()
	{
//		EventCenter.Register ("Button_fate_stuff_button", new DefineFactory<Button_fate_stuff_button>());
		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mFateConfig.GetAllRecord ())
		{
			if(v.Key != null)
			{
				iMaxFateLevel ++;
			}
		}
	}

	public override void Open (object param)
	{
		base.Open (param);
        curRoleData = TeamPosInfoWindow.mCurActiveData;
        iBodyTid = curRoleData.tid;

		consumeItemLogicData = DataCenter.GetData ("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
		curConsumeItemData = consumeItemLogicData.GetDataByTid ((int)ITEM_TYPE.FATE_STONE);

		InitButtons();
		Refresh (null);
	}

	public override bool Refresh(object param)
	{
		UpdaeUI();
		return base.Refresh (param);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch (keyIndex)
		{
		case "SEND_FATE_STRENGTHEN_VALUE_MESSAGE":
			SendFateValueMessage( );
			break;
		case "SET_FATE_VALUE":
			curExp = (int)objVal ;
			UpdaeUI();
			break;
		case "SEND_FATE_STRENGTHEN_MESSAGE":
			SendFateMessage( );
			break;
		case "FATE_STRENGTHEN_SUCCESS":
			FateenSuccess((string)objVal );
			break;
		case "FATE_STRENGTHEN_FAILE":
			isCanFate = true;
			break;
		case "FATE_GO_ON":
			isCanFate = (bool)objVal;
			UpdaeUI();
			break;
		}
	}
	
	public bool FateenCondition()
	{
		bool bIsCan = true;
		isCanFate = false ;

		if(null == curConsumeItemData)
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_FATE_NEED_FATE_STONE);
			bIsCan = false;
		}else if(curRoleData.fateLevel >= iMaxFateLevel) 
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_FATE_UPGRADE_LEVEL_MAX);
			bIsCan = false;
		}else if( fateNeedCoin > curConsumeItemData.itemNum )
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_FATE_NEED_FATE_STONE);
			bIsCan = false;
		}
		return bIsCan;
	}

	void SendFateMessage()
	{
		if(FateenCondition ())
		{
			NetManager.RequestFateUpgrade(curRoleData.itemId, curRoleData.tid);
		}else
		{
			isCanFate = true;
		}
	}
	void SendFateValueMessage()
	{
		NetManager.RequestFateValue(curRoleData.itemId, curRoleData.tid);
	}
	
	void FateenSuccess(string text)
	{
	    SC_FateUpgrade item = JCode.Decode<SC_FateUpgrade>(text);
		if(null == item)
			return;

		int isFateSuccess = item.isFateSuccess;
		fateNeedCoin = TableCommon.GetNumberFromFateConfig(curRoleData.fateLevel, "COST_NUM");
		curConsumeItemData.itemNum -= fateNeedCoin;
		CreateFateEffect();

		if(isFateSuccess == 1)
		{
			DataCenter.OpenWindow ("FATE_RESULT_WINDOW", curRoleData);
			curRoleData.fateLevel += 1;
			curExp = 0;
//			UpdaeUI ();
		}else if(isFateSuccess == 0)
		{
			curExp = item.fateValue;
			SetCurFateValue();
			UpdateTips();
			GlobalModule.DoLater(() => isCanFate = true, 0f);
		}
	}

	void CreateFateEffect()
	{
		GameObject fateGetExpTips = GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UIEffect/ui_tianming_effect", GameCommon.FindObject (mGameObjUI, "fate_stuff_button"));
	//	fateGetExpTips.transform.localScale = Vector3.one;
	//	fateGetExpTips.transform.position = GameCommon.FindObject (mGameObjUI, "fate_stuff_button").gameObject.transform.position;
        GameObject upgradeEffectObj = NGUITools.AddChild(mGameObjUI, (GameObject)Resources.Load("Effect/UIEffect/ui_tianmingonly_tuowei"));
        upgradeEffectObj.transform.parent = GameCommon.FindObject(mGameObjUI, "fate_stuff_button").transform;
        TweenPosition[] tweenPosArr = upgradeEffectObj.GetComponents<TweenPosition>();

        if (tweenPosArr != null)
        {
            GameObject sliderObj = GameCommon.FindObject(mGameObjUI, "cur_fate_up_bar");
            GameObject sliderRightPos = GameCommon.FindObject(sliderObj, "rightpos");
            GameObject button = GameCommon.FindObject(mGameObjUI, "fate_stuff_button");
            Vector3 toPos = new Vector3(0, 0, 0);
            if (sliderObj != null && sliderRightPos != null)
            {
                UISlider slider = sliderObj.GetComponent<UISlider>();
                if (slider != null)
                {
                    toPos.x = sliderObj.transform.position.x + 0.05f + (sliderRightPos.transform.position.x - sliderObj.transform.position.x) * slider.value;
                    toPos.y = sliderObj.transform.position.y - 0.01f;
                    toPos.z = sliderObj.transform.position.z;
                }
            }
            if (button != null)
            {
                tweenPosArr[0].from = button.transform.position;
                tweenPosArr[0].to = toPos;
            }
            tweenPosArr[0].enabled = true;
            tweenPosArr[0].ResetToBeginning();
            tweenPosArr[0].PlayForward();
        }
		GlobalModule.DoLater (() => GameObject.DestroyImmediate(fateGetExpTips), 1f);
        GlobalModule.DoLater(() => GameObject.DestroyImmediate(upgradeEffectObj), 0.3f);
	}

	public void UpdateTips()
	{
		UILabel objLabel = GameCommon.FindObject (mGameObjUI ,"tips_1_label").GetComponent<UILabel >();
//		int rateLostNum = TableCommon.GetNumberFromFateConfig (curRoleData.fateLevel, "RATE_LOST");
		int costNum = TableCommon.GetNumberFromFateConfig (curRoleData.fateLevel, "NEED_NUM");

		float rateLost = 0.0f;
		GameCommon.SetUIText (mGameObjUI, "fate_stuff_stone_number_label", curConsumeItemData.itemNum.ToString () );

		if(curExp <= 0)
		{
			objLabel.text = "极小概率";
			objLabel.color = Color.white;
		}else 
		{
			rateLost = (float)curExp / (float)costNum;
			if(rateLost <= 0.3)
			{
				objLabel.text = "极小概率";
				objLabel.color = Color.white;
			}else if(rateLost <= 0.6)
			{
				objLabel.text = "较小概率";
				objLabel.color = Color.green;
			}else if(rateLost > 0.6)
			{
				objLabel.text = "较高概率";
				objLabel.color = Color.blue;
			}
		}

		UpdateNeedFateStuffNum();
	}

	public void UpdaeUI()
	{
		SetRoleBasicInfo();
		SetRoleIcon();
		SetCurExpInfo();
		UpdateNeedFateStuffNum();
		InitButtons();
		UpdateTips();
	}
	//update need fate stuff number
	public void UpdateNeedFateStuffNum()
	{
		fateNeedCoin = TableCommon.GetNumberFromFateConfig(curRoleData.fateLevel, "COST_NUM");
		// set need stuff
		GameObject needGoldNum =  GameCommon.FindObject(mGameObjUI, "need_gold_num");
		UILabel needGlodNumLabel = needGoldNum.GetComponent<UILabel>();
		string strText = fateNeedCoin.ToString();

		if(null == curConsumeItemData)
		{
			GameCommon.SetUIText (mGameObjUI,"fate_stuff_stone_number_label", "0");
			needGlodNumLabel.text = "[ff0000]" + strText;
		}
		else 
		{
			GameCommon.SetUIText (mGameObjUI, "fate_stuff_stone_number_label", curConsumeItemData.itemNum.ToString () );
			
			if(needGoldNum != null)
			{
				needGlodNumLabel.text = strText;
				if(fateNeedCoin > curConsumeItemData.itemNum)
				{
					strText = "[ff0000]" + strText;
				}	
				if(curRoleData.fateLevel >= iMaxFateLevel)
				{
					needGlodNumLabel.text = "∞";
				}
			}
		}

	}

	//set role basic Info
	public void SetRoleBasicInfo()
	{
		GameObject roleNameObj =GameCommon.FindObject (mGameObjUI, "pet_name_label").gameObject;
		UILabel rollNameObj = roleNameObj.GetComponent<UILabel >();
		rollNameObj.text = GameCommon.GetItemName(iBodyTid);

		GameCommon.SetUIText (mGameObjUI ,"pet_break_number_label", "+" + curRoleData.breakLevel.ToString ());
		GameCommon.SetUIText (mGameObjUI,"fate_level_label", "天命 " + curRoleData.fateLevel.ToString () + " 级");
	}
	//set role Icon
	public void SetRoleIcon()
	{
		GameObject roleIconObj = GameCommon.FindObject(mGameObjUI, "aim_pet_icon").gameObject;
		GameCommon.SetPetIconWithElementAndStar(roleIconObj, "Background", "", "", iBodyTid);
	}

	public void SetCurExpInfo()
	{
		GameObject srcBaseObj = GameCommon.FindObject(mGameObjUI, "fate_exp_bar");

		SetBaseAttribute("attribute_label(Clone)_", curRoleData.fateLevel);
		SetFateLevel(srcBaseObj, curRoleData.fateLevel );

		SetCurFateValue();
//		int maxExp = TableCommon.GetNumberFromFateConfig(curRoleData.fateLevel, "NEED_NUM");
//		float percentage = curExp / (float)(maxExp);
//		
//		UIProgressBar fateCurExpBar = GameCommon.FindObject(mGameObjUI, "cur_fate_up_bar").GetComponent<UIProgressBar>();
//		fateCurExpBar.value = percentage;
//		
//		UILabel fateCurExpPercentage = GameCommon.FindObject(mGameObjUI, "get_exp_label").GetComponent<UILabel>();
//		fateCurExpPercentage.text ="天命值  " + curExp.ToString() + "/" + maxExp.ToString ();
	}
	public void SetCurFateValue()
	{
		int maxExp = TableCommon.GetNumberFromFateConfig(curRoleData.fateLevel, "NEED_NUM");
		float percentage = curExp / (float)(maxExp);
		
		UIProgressBar fateCurExpBar = GameCommon.FindObject (mGameObjUI, "cur_fate_up_bar").GetComponent<UIProgressBar>();
		fateCurExpBar.value = percentage;
		
		UILabel fateCurExpPercentage = GameCommon.FindObject(mGameObjUI, "get_exp_label").GetComponent<UILabel>();
		fateCurExpPercentage.text ="天命值  " + curExp.ToString() + "/" + maxExp.ToString ();
		if(curRoleData.fateLevel >= iMaxFateLevel)
		{
			fateCurExpBar.value = 1;
			fateCurExpPercentage.text ="天命等级已达满级";
		}

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

		GameObject baseObj = GameCommon.FindObject(mGameObjUI, baseObjName + iIndex.ToString());
		// set base attribute number
		SetBaseAttributeValue(iIndex, baseObj, iFateLevel);
	}
	// set base attribute number
	public void SetBaseAttributeValue(int iIndex, GameObject obj, int iStrengthenLevel)
	{
		UILabel attributeNameLabel = obj.transform.Find ("attribute_name_label").GetComponent<UILabel>();
		UILabel baseNumLabel =  obj.transform.Find("attribute_name_label/pet_base_number_label").GetComponent<UILabel >();
		UILabel addNumLabel =  obj.transform.Find("attribute_name_label/pet_add_number_label").GetComponent<UILabel >();
//		int fLevelAddValues = 0; 
		int fValue = 0;

		AFFECT_TYPE affectType = BreakInfoWindow.GetAttributeType((BASE_ATTRITUBE_TYPE)(iIndex + 1));
		attributeNameLabel.text = TableCommon.GetStringFromEquipAttributeIconConfig ((int)affectType, "NAME");

		if (baseNumLabel != null)
		{
			int fBaseValue = TableCommon.GetNumberFromFateConfig(curRoleData.fateLevel, "ADD_RATE" );
			if (curRoleData != null && curRoleData.fateLevel < iMaxFateLevel)
			{
				int fLevelAddValue = TableCommon.GetNumberFromFateConfig(curRoleData.fateLevel + 1, "ADD_RATE" );

				for(int i = 0; i < curRoleData.fateLevel; i++)
				{
					fValue += fBaseValue;
				}

				baseNumLabel.text = "+" + fValue + "%";
				addNumLabel.text = "+" + fLevelAddValue + "%";
			}
			else if(curRoleData != null && curRoleData.fateLevel >= iMaxFateLevel)
			{
				for(int i = 0; i < curRoleData.fateLevel; i++)
				{
					fValue += fBaseValue;
				}
				baseNumLabel.text = "+" + fValue + "%";
				addNumLabel.text = "顶级天命";
			}
		}
	}

	public void SetFateLevel(GameObject obj, int iFateLevel)
	{
		// set level
		GameObject curFateLevel = obj.transform.Find("fate_name_label/cur_level_label").gameObject;
		UILabel curFateLevelLabel = curFateLevel.GetComponent<UILabel>();
		curFateLevelLabel.text = curRoleData.fateLevel.ToString ();
		GameCommon.SetUIText(mGameObjUI, "add_level_label", "+1");
		if(curRoleData.fateLevel >= iMaxFateLevel)
		{
			GameCommon.SetUIText(mGameObjUI, "add_level_label", "顶级");
		}
	}

	private void InitButtons()
	{
		UIButtonEvent evt = GameCommon.FindComponent<UIButtonEvent>(mGameObjUI, "fate_stuff_button");
		evt.SetOnPressingAction (()=>OnPressStone ());
	}
	private void OnPressStone()
	{
		if (isCanFate)
		{
			DataCenter.SetData("FATE_INFO_WINDOW", "SEND_FATE_STRENGTHEN_MESSAGE",true);
		}
	}
}
//天命升级button
class Button_fate_stuff_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData("FATE_INFO_WINDOW", "SEND_FATE_STRENGTHEN_MESSAGE",true);
		return true;
	}
}