using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using DataTable;

public class AstrologyInfoWindow : tWindow
{
	public ConsumeItemData curConsumeItemData;
	public int mStarMapCount = 0;
	public static int mPage = 0;

	private const int MAP_WIDTH = 762;
	private const int MAP_CHOSE = 5;
	private const int MAP_STARS = 5;
	int curStarIndex = 0;
	public int iAttackValue = 0;
	public int iPhysicalValue = 0;
	public int iHPValue = 0;
	public int iMagicValue = 0;
	UIGridContainer  starsGroupGrid;

	public override void Init()
	{
//		EventCenter.Self.RegisterEvent("Button_astrology_go_back_btn", new DefineFactory<Button_astrology_go_back_btn>());
//		EventCenter.Self.RegisterEvent("Button_astrology_go_forward_btn", new DefineFactory<Button_astrology_go_forward_btn>());
//		EventCenter.Self.RegisterEvent ("Button_stars_btn", new DefineFactory<Button_stars_btn>());
//		EventCenter.Self.RegisterEvent("Button_lighten_Button", new DefineFactory<Button_lighten_Button>());

		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mPointStarConfig.GetAllRecord ())
		{
			if(v.Key != null)
			{
				mStarMapCount ++;
			}
		}
	}

    protected override void OpenInit()
    {
        base.OpenInit();
        ConsumeItemLogicData consumeItemLogicData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
        curConsumeItemData = consumeItemLogicData.GetDataByTid((int)ITEM_TYPE.POINT_STAR_TONE);

        starsGroupGrid = GameCommon.FindObject(mGameObjUI, "stars_group_grid").GetComponent<UIGridContainer>();
        starsGroupGrid.MaxCount = mStarMapCount / MAP_STARS;

        for (int i = 0; i < starsGroupGrid.MaxCount; i++)
        {
            GameObject map = starsGroupGrid.controlList[i];
            for (int j = 0; j < MAP_CHOSE; j++)
            {
                GameObject mapChoseObj = GameCommon.FindObject(map, "stars_info_window_" + j).gameObject;

                if (i % MAP_STARS == j)
                {
                    mapChoseObj.SetActive(true);
                }
                else
                {
                    mapChoseObj.SetActive(false);
                }
            }
            map.transform.localScale = Vector3.one;
            map.transform.localPosition = new Vector3(MAP_WIDTH * (i - mPage), 0, 0);
            map.name = "stars_info_group(Clone)_" + i.ToString();
        }
        if ((curStarIndex - 1) / MAP_STARS > mPage)
        {
            GameCommon.FindObject(mGameObjUI, "tips03").SetActive(true);
            GameCommon.FindObject(mGameObjUI, "tips04").SetActive(false);
            GameCommon.FindObject(mGameObjUI, "lighten_infos").SetActive(false);
        }
        else if ((curStarIndex - 1) / MAP_STARS == mPage)
        {
            GameCommon.FindObject(mGameObjUI, "tips03").SetActive(false);
            GameCommon.FindObject(mGameObjUI, "tips04").SetActive(false);
            GameCommon.FindObject(mGameObjUI, "lighten_infos").SetActive(true);
        }
        else
        {
            GameCommon.FindObject(mGameObjUI, "tips03").SetActive(false);
            GameCommon.FindObject(mGameObjUI, "tips04").SetActive(true);
            GameCommon.FindObject(mGameObjUI, "lighten_infos").SetActive(false);
        }
        UpdateStarMaps();
        SetBackAndForwardButton();
        SetAllAttributeValue();
    }
	public override void Open (object param)
	{
		base.Open (param);
		curStarIndex = (int)param;
		
		SetPointStarsLight(curStarIndex);
	}

	public void UpdateStarMaps()
	{
		int starIndex = 0;

		for (int i = 0; i < starsGroupGrid.MaxCount; i++)
		{
			GameObject starMapObj = starsGroupGrid.controlList[i];
			for(int j = 0; j < MAP_CHOSE; j++)
			{
				GameObject mapChoseObj = GameCommon.FindObject (starMapObj, "stars_info_window_" + j).gameObject;
			
				if(i % MAP_CHOSE == j)
				{
					mapChoseObj.SetActive (true);
					for (int iStar = 0; iStar < MAP_STARS; iStar++)
					{
						starIndex ++ ;
						GameObject starPointObj = mapChoseObj.transform.Find ("stars_info_group/stars_point(Clone)_" + iStar.ToString ()).gameObject;
						GameObject starTipsObj = starPointObj.transform.Find ("star_tips").gameObject;
						UILabel starTipsLabel = starTipsObj.transform.Find ("star_tips_label").GetComponent<UILabel>();
						GameObject starLightenObj = starPointObj.transform.Find ("stars_btn/star_lighten_icon").gameObject;
						starLightenObj.SetActive (false);
						
						int rewardType = TableCommon.GetNumberFromPointStarConfig (starIndex, "REWARD_TYPE");
						string tipsInfos = TableCommon.GetStringFromPointStarConfig (starIndex, "TIPS");
						string rewardNum = TableCommon.GetStringFromPointStarConfig (starIndex, "REWARD_NUMERICAL");
						
						starTipsLabel.text = tipsInfos;
						
						starTipsObj.SetActive (false);
//						TweenAlpha twA = TweenAlpha.Begin (starTipsObj, 0.001f, 0f);
						if(iStar == MAP_STARS - 1)
						{
							starTipsObj.SetActive (true);
//							twA = TweenAlpha.Begin (starTipsObj, 0.001f, 1f);
						}
						GameCommon.GetButtonData(GameCommon.FindObject(starPointObj, "stars_btn")).set("CUR_STAR_INDEX", starIndex);
					}
				}else
				{
					mapChoseObj.SetActive (false);
				}
			}
		}
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
		switch (keyIndex)
		{
		case "FORWARD_AND_BACK":
			mPage += (int)objVal;
			SetBackAndForwardButton();
			DataCenter.SetData ("ASTROLOGY_WINDOW", "UPDATE_ASTROLOGYR_INDEX", mPage);
			DataCenter.SetData ("ASTROLOGY_WINDOW", "CHANGE_TAB_POS", mPage);
			UpdateStarWindow(mPage);    
			break;
		case "UPDATE_TIPS":
			UpdateStarTips((int)objVal);
			break;
		case "UPDATE_STAR_WINDOW":
			UpdateStarWindow((int)objVal);
			break;
		case "UPDATE_BUTTON_INFO":
			ClickLightButton((int)objVal);
			break;
		case "SET_POINT_STAR_INDEX":
			curStarIndex = (int)objVal;
			SetPointStarsLight(curStarIndex);
			break;
		case "SUCCESS_POINT_STAR":
			SuccessPointStar((string)objVal);
			break;
		case "SUCCESS_GET_REWARDS":
			SuccessGetRewards((string)objVal);
			break;
		}
	}
	public void SuccessGetRewards(string text)
	{
		SC_PointLightenClick item = JCode.Decode<SC_PointLightenClick>(text);
		if( null == item )
			return ;

        PointStarLogicData.Self.mCurIndex = item.currentIndex;
        SetPointStarsLight(PointStarLogicData.Self.mCurIndex);
        PackageManager.UpdateItem(item.arr);

	}
	void SuccessPointStar(string text)
	{
		SC_PointLightenClick item = JCode.Decode<SC_PointLightenClick>(text);
		if( null == item )
			return ;

        curStarIndex = item.currentIndex;
        PointStarLogicData.Self.mCurIndex = item.currentIndex;
        int iPointStarStoneNeed = TableCommon.GetNumberFromPointStarConfig(curStarIndex - 1, "COST");
        curConsumeItemData.itemNum -= iPointStarStoneNeed;
        int iType = TableCommon.GetNumberFromPointStarConfig(curStarIndex - 1, "REWARD_TYPE");
        string rewardNum = TableCommon.GetStringFromPointStarConfig(curStarIndex - 1, "REWARD_NUMERICAL");

        SetPointStarsLight(curStarIndex);
        if (iType >= (int)POINT_STAR_TYPE.ATTACK && iType <= (int)POINT_STAR_TYPE.HP)
        {
            SetAttributeValue(curStarIndex - 1);
        }
        else if (iType == (int)POINT_STAR_TYPE.STAR_LEVEL_UP)
        {
            string[] strReNum = rewardNum.Split('|');
            int roleType = GameCommon.GetRoleType(RoleLogicData.Self.character.tid);
            for (int i = 0; i < strReNum.Length; i++)
            {
                if (GameCommon.GetRoleType(Convert.ToInt32(strReNum[i])) == roleType)
                {
                    RoleLogicData.Self.character.tid = Convert.ToInt32(strReNum[i]);
                }
            }
        }
	}

	public void SetPointStarsLight(int iCurStarIndex)
	{
		GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "lighten_Button")).set("CUR_STAR_INDEX", iCurStarIndex);
		mPage = (iCurStarIndex - 1) / 5;
		DataCenter.SetData ("ASTROLOGY_WINDOW", "UPDATE_ASTROLOGYR_INDEX", mPage);
		DataCenter.SetData ("ASTROLOGY_WINDOW", "CHANGE_TAB_POS", mPage);

		for(int i = 0; i < starsGroupGrid.MaxCount; i++)
		{
			GameObject starMapObj = starsGroupGrid.controlList[i];
			for(int j = 0; j < MAP_CHOSE; j++)
			{
				GameObject mapChoseObj = GameCommon.FindObject (starMapObj, "stars_info_window_" + j).gameObject;

				if(i % MAP_CHOSE == j)
				{
					mapChoseObj.SetActive (true);

					for (int iStar = 0; iStar < MAP_STARS; iStar++)
					{
						GameObject starPointObj = mapChoseObj.transform.Find ("stars_info_group/stars_point(Clone)_" + iStar).gameObject;
						GameObject starLightenObj = starPointObj.transform.Find ("stars_btn/star_lighten_icon").gameObject;
						GameObject curCheckmark = starPointObj.transform.Find ("stars_btn/Checkmark").gameObject;
						GameObject needPointStarStoneObj = GameCommon.FindObject (mGameObjUI, "need_gold");
						
						UILabel needPointStarStoneLabel = needPointStarStoneObj.transform.Find ("need_gold_num").GetComponent<UILabel>();
						
						GameCommon.SetUIText (mGameObjUI, "tips02", TableCommon.GetStringFromPointStarConfig (iCurStarIndex, "TIPS"));
						
						int isNeedPointStarStone = TableCommon.GetNumberFromPointStarConfig(curStarIndex, "COST");
						if(null == curConsumeItemData)
						{
							needPointStarStoneLabel.text = "0" + "/" + isNeedPointStarStone;
							return;
						}
						needPointStarStoneLabel.text = curConsumeItemData.itemNum + "/" + isNeedPointStarStone;
						if(i * 5 + iStar + 1 < iCurStarIndex)
						{
							starLightenObj.SetActive (true);
						}
						
						if(i * 5 + iStar + 1 <= mStarMapCount * 5)
						{
							if(i * 5 + iStar + 1 == iCurStarIndex)
							{
								curCheckmark.SetActive (true);
							}else 
							{
								curCheckmark.SetActive (false);
							}
						}else 
						{
							curCheckmark.SetActive (false);
						}
					}
				}else
					mapChoseObj.SetActive (false);
			}
		}
	}

	public void SetAllAttributeValue()
	{
		UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "attuibute_group");
		if(grid != null)
		{
			for(int i = (int)POINT_STAR_TYPE.ATTACK; i <= grid.MaxCount; i++)
			{
				GameObject obj = grid.controlList[i - 1];
				UpdateAttributeUI(obj, i);
			}
		}
	}

	public void SetAttributeValue(int iCurStarIndex)
	{
		PointStarLogicData.Self.AddItem (iCurStarIndex);
		int iType = TableCommon.GetNumberFromPointStarConfig (iCurStarIndex, "REWARD_TYPE");
		UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "attuibute_group");
		if(grid != null)
		{
			GameObject obj = grid.controlList[iType - 1];
			UpdateAttributeUI(obj, iType);
		}
	}

	public void UpdateAttributeUI(GameObject obj, int iType)
	{
		if(obj != null)
		{
			UILabel attributeNameLabel =  obj.transform.Find("attuibute").GetComponent<UILabel >();
			AFFECT_TYPE affectType = PointStarLogicData.Self.GetAttributeType ((POINT_STAR_TYPE)iType);
			attributeNameLabel.text = TableCommon.GetStringFromEquipAttributeIconConfig ((int)affectType, "NAME");
			
			UILabel attributeNumLabel =  obj.transform.Find("number").GetComponent<UILabel >();
			attributeNumLabel.text = PointStarLogicData.Self.GetAttributeValue ((POINT_STAR_TYPE)iType).ToString ();
		}
	}

	public bool LightCondition()
	{
		bool bIsCan = true;
		int pointStarStoneNeed = TableCommon.GetNumberFromPointStarConfig(curStarIndex, "COST");
		
		if(null == curConsumeItemData)
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_POINT_STAR_NEED_STONE);
			bIsCan = false;
		}else if(pointStarStoneNeed > curConsumeItemData.itemNum)
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_POINT_STAR_NEED_STONE);
			bIsCan = false;
		}

		return bIsCan;
	}

	public void ClickLightButton(int curPointStarIndex)
	{
		if(LightCondition())
		{
			NetManager.RequestPointLightClick(curPointStarIndex);
		}
	}

	public void UpdateStarWindow(int iStarWindow)
	{
		curStarIndex = PointStarLogicData.Self.mCurIndex;

		for(int i = 0; i < starsGroupGrid.MaxCount; i++)
		{
			GameObject map = starsGroupGrid.controlList[i];
			for(int j = 0; j < MAP_CHOSE; j++)
			{
				GameObject mapChoseObj = GameCommon.FindObject (map, "stars_info_window_" + j).gameObject;

				if(i % MAP_STARS == j)
				{
					mapChoseObj.SetActive (true);
				}else
					mapChoseObj.SetActive (false);
			}
			map.transform.localScale = Vector3.one;
			map.transform.localPosition = new Vector3(MAP_WIDTH * (i - iStarWindow), 0, 0);
			map.name = "stars_info_group(Clone)_" + i.ToString();
			mPage = iStarWindow;
			
			SetBackAndForwardButton();
		}

		if(curStarIndex <= mStarMapCount * MAP_STARS)
		{
			if((curStarIndex - 1) / MAP_STARS > iStarWindow)
			{
				GameCommon.FindObject (mGameObjUI, "tips03").SetActive (true);
				GameCommon.FindObject (mGameObjUI, "tips04").SetActive (false);
				GameCommon.FindObject (mGameObjUI, "lighten_infos").SetActive (false);
			}else if((curStarIndex - 1) / MAP_STARS == iStarWindow)
			{
				GameCommon.FindObject (mGameObjUI, "tips03").SetActive (false);
				GameCommon.FindObject (mGameObjUI, "tips04").SetActive (false);
				GameCommon.FindObject (mGameObjUI, "lighten_infos").SetActive (true);
			}else 
			{
				GameCommon.FindObject (mGameObjUI, "tips03").SetActive (false);
				GameCommon.FindObject (mGameObjUI, "tips04").SetActive (true);
				GameCommon.FindObject (mGameObjUI, "lighten_infos").SetActive (false);
			}
		}else 
		{
			GameCommon.FindObject (mGameObjUI, "tips03").SetActive (true);
			GameCommon.FindObject (mGameObjUI, "tips04").SetActive (false);
			GameCommon.FindObject (mGameObjUI, "lighten_infos").SetActive (false);
		}
	}
	
	private void SetBackAndForwardButton()
	{
		GameCommon.FindObject (mGameObjUI, "astrology_go_forward_btn").SetActive (true);
		GameCommon.FindObject (mGameObjUI, "astrology_go_back_btn").SetActive (true);

		if (mPage == 0)
			GameCommon.FindObject (mGameObjUI, "astrology_go_back_btn").SetActive (false);
		if (mPage == mStarMapCount / MAP_STARS - 1)
			GameCommon.FindObject (mGameObjUI, "astrology_go_forward_btn").SetActive (false);
	}

	public void UpdateStarTips(int iStarsIndex)
	{
		for(int i = 0; i < starsGroupGrid.MaxCount; i++)
		{
			GameObject starMapObj = starsGroupGrid.controlList[i];
			for(int j = 0; j < MAP_CHOSE; j++)
			{
				GameObject mapChoseObj = GameCommon.FindObject (starMapObj, "stars_info_window_" + j).gameObject;

				if(i % MAP_CHOSE == j)
				{
					mapChoseObj.SetActive (true);

					for (int iStar = 0; iStar < MAP_STARS; iStar++)
					{
						GameObject starPointObj = mapChoseObj.transform.Find ("stars_info_group/stars_point(Clone)_" + iStar).gameObject;
						GameObject starTipsObj = starPointObj.transform.Find ("star_tips").gameObject;
						
						if(iStarsIndex == i * MAP_STARS + iStar + 1)
						{
							if(iStarsIndex % MAP_STARS == 0)
							{
//								TweenScale twS = TweenScale.Begin (starTipsObj, 0.2f, Vector3.zero);
//								TweenScale twScaleLater = starTipsObj.gameObject.GetComponent<TweenScale>();
//								twScaleLater.from = Vector3.zero;
//								twScaleLater.to = new Vector3 (1.0f, 1.0f, 1.0f);
//								twScaleLater.duration = 0.5f;
//								twScaleLater.delay = 0.5f;
								TweenAlpha twA = TweenAlpha.Begin (starTipsObj, 0.1f, 0f);
								TweenAlpha twB = starTipsObj.gameObject.GetComponent<TweenAlpha>();
								twB.from = 0;
								twB.to = 1;
								twB.duration = 1f;
								twB.delay = 0.2f;
								
							}else
							{
								TweenAlpha twA = TweenAlpha.Begin (starTipsObj, 0.1f, 1f);
								starTipsObj.SetActive (true);
								TweenAlpha twB = starTipsObj.gameObject.GetComponent<TweenAlpha>();
								twB.from = 1;
								twB.to = 0;
								twB.duration = 1.0f;
								twB.delay = 0.5f;
							}
						}
					}
				}else
					mapChoseObj.SetActive (false);
			}
		}
	}

}

public class Button_astrology_go_back_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("ASTROLOGY_INFO_WINDOW", "FORWARD_AND_BACK", -1);
		return true;
	}
}

public class Button_astrology_go_forward_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("ASTROLOGY_INFO_WINDOW", "FORWARD_AND_BACK", 1);
		return true;
	}
}

public class Button_stars_btn : CEvent
{
	public override bool _DoEvent()
	{
		int starIndex = get ("CUR_STAR_INDEX");
		DataCenter.SetData ("ASTROLOGY_INFO_WINDOW", "UPDATE_TIPS", starIndex);
		return true;
	}
}
public class Button_lighten_Button : CEvent
{
	public override bool _DoEvent()
	{
		int starIndex = get ("CUR_STAR_INDEX");

		int iType = TableCommon.GetNumberFromPointStarConfig (starIndex, "REWARD_TYPE");
		if(iType == 6)
		{
			string iGroupId = TableCommon.GetStringFromPointStarConfig (starIndex, "REWARD_NUMERICAL");
			DataCenter.OpenWindow ("ASTROLOGY_REWARD_WINDOW", System.Convert.ToInt32(iGroupId));
		}else
		{
			DataCenter.SetData ("ASTROLOGY_INFO_WINDOW", "UPDATE_BUTTON_INFO", starIndex);
		}
		return true;
	}
}
