using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

public class AstrologyUIWindow : tWindow
{
	int curStarIndex = 0;
	int iIndexNum = 0;
	int mPage = 0;
	public ConsumeItemData curConsumeItemData;
	UIGridContainer mLeftGridContainer;
	UIScrollView mLeftGridScrollView;
	UIPanel mLeftPanel;
	UIGridContainer mRightGridContainer;
	private float mLeftScrollSpeed = 0.1f;          //左侧列表滚动速度
	private bool mIsAddFinishedAction = false;      //是否添加了滑动结束回调
	private int mCurrSelIndex = -1;                

	private const int MAP_MODEL = 5;
	private const int MAP_STARS = 5;
    //by chenliang
    //begin

    private UICenterOnChild mStarsGroupGridCenter;

    //end

	bool isFirstOpen = true;
	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_astrology_back_btn", new DefineFactory<Button_AstrologyBackBtn>());
		EventCenter.Self.RegisterEvent ("Button_constellation_piont_button", new DefineFactory<Button_ConstellationPiontButton>());
		EventCenter.Self.RegisterEvent("Button_arrow_up_btn", new DefineFactory<Button_arrow_up_btn>());
		EventCenter.Self.RegisterEvent("Button_arrow_down_btn", new DefineFactory<Button_arrow_down_btn>());
		EventCenter.Self.RegisterEvent("Button_astrology_go_back_btn", new DefineFactory<Button_AastrologyGoBackBtn>());
		EventCenter.Self.RegisterEvent("Button_astrology_go_forward_btn", new DefineFactory<Button_AstrologyGoForwardBtn>());
		EventCenter.Self.RegisterEvent ("Button_stars_btn", new DefineFactory<Button_StarsBtn>());
		EventCenter.Self.RegisterEvent("Button_lighten_Button", new DefineFactory<Button_LightenButton>());

		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mPointStarConfig.GetAllRecord ())
		{
			if(v.Key != null)
				iIndexNum ++;
		}
	}

	public override void Open(object param)
	{
		base.Open (param);
		curStarIndex = PointStarLogicData.Self.mCurIndex;

		ConsumeItemLogicData consumeItemLogicData = DataCenter.GetData ("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
		curConsumeItemData = consumeItemLogicData.GetDataByTid ((int)ITEM_TYPE.POINT_STAR_TONE);
		
		mLeftGridContainer = GetComponent<UIGridContainer> ("constellation_group_grid");
		mLeftGridScrollView = GetComponent<UIScrollView> ("constellation_scrollview");
		mLeftPanel = GetComponent<UIPanel> ("constellation_scrollview");
		mRightGridContainer = GetComponent<UIGridContainer> ("stars_group_grid");

		//添加居中后回调
		//by chenliang
		//begin

//		UICenterOnChild tmpCenter = GameCommon.FindComponent<UICenterOnChild>(mGameObjUI, "stars_group_grid");
//--------------------
		if (mStarsGroupGridCenter == null)
			mStarsGroupGridCenter = GameCommon.FindComponent<UICenterOnChild> (mGameObjUI, "stars_group_grid");
		UICenterOnChild tmpCenter = mStarsGroupGridCenter;
		//end
		if (mRightGridContainer != null && tmpCenter != null) {
			if (tmpCenter.onFinished != null)
				tmpCenter.onFinished -= OnSweepStarMapFinished;
			tmpCenter.onFinished += OnSweepStarMapFinished;
		}

		mLeftGridContainer.MaxCount = iIndexNum / MAP_STARS;
		mRightGridContainer.MaxCount = iIndexNum / MAP_STARS;
		
		DataCenter.OpenWindow ("BACK_GROUP_ASTROLOGY_WINDOW");
		if (mCurrSelIndex == -1 || mCurrSelIndex >= iIndexNum / MAP_STARS) {
			ToggleConstellation ((curStarIndex - 1)/MAP_STARS);
			ChangeToMap ((curStarIndex - 1)/MAP_STARS , true);
			UpdateTips(curStarIndex, curStarIndex/MAP_STARS);
		}
		Refresh(param);
	}
	/// <summary>
	/// 滑动完成时回调
	/// </summary>
	private void OnSweepStarMapFinished()
	{
        //by chenliang
        //begin

//		UICenterOnChild tmpCenter = GameCommon.FindComponent<UICenterOnChild>(mGameObjUI, "stars_group_grid");
//-------------
        UICenterOnChild tmpCenter = mStarsGroupGridCenter;

        //end
		if (tmpCenter != null)
		{
			string centerGOName = tmpCenter.centeredObject.name;
			int index = -1;
			if (centerGOName == "stars_info_group")
				index = 0;
			else
			{
				string preName = "stars_info_group(Clone)_";
				string centerGOIndex = centerGOName.Substring(preName.Length);
				if (!int.TryParse(centerGOIndex, out index))
					index = -1;
			}
			if (index != -1 && isFirstOpen == true)
			{
				ToggleConstellation(index);
				ChangeToMap(index, true);
				UpdateTips(curStarIndex, index);
				isFirstOpen = false;
			}else if(index != -1 && isFirstOpen == false)
			{
				ToggleConstellation(index);
				ChangeToMap(index, false);
				UpdateTips(curStarIndex, index);
			}
		}
	}
	
	public override bool Refresh (object param)
	{
		UpDateButtons();
		SetStarEffactTips();
		UpdateTips(curStarIndex, mCurrSelIndex);
		UpdateLightConstellation(curStarIndex - 1);
		UpdateStarMaps();
		SetAllAttributeValue();
		ChangeToMap (mCurrSelIndex, true);
		SetPointStarsLight(curStarIndex);
		return true;
	}
	void SetStarEffactTips()
	{
        //by chenliang
        //begin

//		for (int i = 0; i < mRightGridContainer.MaxCount; i++)
//----------------
        for (int i = 0, count = mRightGridContainer.MaxCount; i < count; i++)

        //end
		{
			GameObject starMapObj = mRightGridContainer.controlList[i];
			for(int j = 0; j < MAP_MODEL; j++)
			{
				GameObject mapChoseObj = GameCommon.FindObject (starMapObj, "stars_info_window_" + j).gameObject;
				mapChoseObj.SetActive (false);
				if(i % MAP_MODEL == j)
				{
					mapChoseObj.SetActive (true);
					for (int iStar = 0; iStar < MAP_STARS; iStar++)
					{
						GameObject starPointObj = mapChoseObj.transform.Find ("stars_info_group/stars_point(Clone)_" + iStar.ToString ()).gameObject;
						if( GameCommon.FindObject (starPointObj, "Effect/UIEffect/ui_star_jihuo") == null)
						{
							GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UIEffect/ui_star_jihuo", GameCommon.FindObject (starPointObj, "Checkmark"));
						}else
							return;
					}
				}
			}
		}
	}
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
		switch (keyIndex)
		{
		case"CHANGE_CONSTELLATTON":
			ChangeToMap ((int)objVal, true);
			UpdateTips(curStarIndex, (int)objVal);
			break;
		case "UP_AND_DOWN":
			ToggleConstellation(mCurrSelIndex + (int)objVal);
			ChangeToMap(mCurrSelIndex + (int)objVal, true);
			UpdateTips(curStarIndex, mCurrSelIndex);
			break;
		case "UPDATE_TIPS":
			UpdateStarTips((int)objVal);
			break;
		case "UPDATE_BUTTON_INFO":
			ClickLightButton((int)objVal);
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
        PackageManager.UpdateItem(item.arr);
        curStarIndex = item.currentIndex;
        // add by LC
        // begin
        // 扣除点星石
        int iPointStarStoneNeed = TableCommon.GetNumberFromPointStarConfig(PointStarLogicData.Self.mCurIndex - 1, "COST");
        curConsumeItemData.itemNum -= iPointStarStoneNeed;
        // end

		SetEffect(curStarIndex);
		GlobalModule.DoLater (() => SetGetRewards(), 0.9f);       
	}
	void SetGetRewards()
	{
		UIImageButton btn = GameCommon.FindObject(mGameObjUI, "lighten_Button").GetComponent<UIImageButton>();
		if(btn != null)
			btn.isEnabled = true;

		SetPointStarsLight(PointStarLogicData.Self.mCurIndex);		
		ChangeToMap (curStarIndex / MAP_STARS, true);
		UpdateTips(curStarIndex, curStarIndex / MAP_STARS);
		UpdateLightConstellation(curStarIndex - 1);
		ToggleConstellation(mCurrSelIndex );
	}
	void SetEffect(int iEffectStarIndex)
	{
		for (int i = 0, count = mRightGridContainer.MaxCount; i < count; i++)
		{
			GameObject starMapObj = mRightGridContainer.controlList[i];
			for(int j = 0; j < MAP_MODEL; j++)
			{
				GameObject mapChoseObj = GameCommon.FindObject (starMapObj, "stars_info_window_" + j).gameObject;
				for (int iStar = 0; iStar < MAP_STARS; iStar++)
				{
					GameObject starPointObj = mapChoseObj.transform.Find ("stars_info_group/stars_point(Clone)_" + iStar.ToString ()).gameObject;
					if( GameCommon.FindObject (starPointObj, "Effect/UIEffect/ui_weapon_qianghua") == null)
					{		
						if(i * 5 + iStar + 1 == iEffectStarIndex - 1)
						{
							GameObject objEffect = GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UIEffect/ui_weapon_qianghua", GameCommon.FindObject (starPointObj, "stars_btn"));
							GlobalModule.DoLater (() => GameObject.DestroyImmediate(objEffect), 0.7f);
						}						
					}else
						return;
				}
			}
		}
	}

	void SuccessPointStar(string text)
	{
		SC_PointLightenClick item = JCode.Decode<SC_PointLightenClick>(text);
		if( null == item )
			return ;

        curStarIndex = item.currentIndex;
		SetEffect(curStarIndex);
		GlobalModule.DoLater (() => SetRefresh(), 0.9f);       
	}
	void SetRefresh()
	{
		UIImageButton btn = GameCommon.FindObject(mGameObjUI, "lighten_Button").GetComponent<UIImageButton>();
		if(btn != null)
			btn.isEnabled = true;
		if ((curStarIndex - 1) % 5 == 0)
		{
			if (curStarIndex < iIndexNum)
			{
				UpdateTips(curStarIndex, curStarIndex / 5);
			}
			else
			{
				UpdateTips(curStarIndex, (iIndexNum - 1) / 5);
			}
		}
		
		UpdateLightConstellation(curStarIndex - 1);
		PointStarLogicData.Self.mCurIndex = curStarIndex;
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
				if (GameCommon.GetRoleType(System.Convert.ToInt32(strReNum[i])) == roleType)
				{
					RoleLogicData.Self.character.tid = System.Convert.ToInt32(strReNum[i]);
				}
			}
			ChangeToMap(curStarIndex / MAP_STARS, true);
			ToggleConstellation(mCurrSelIndex);
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
			int iType = TableCommon.GetNumberFromPointStarConfig (curPointStarIndex, "REWARD_TYPE");
			if(iType == 6)
			{
				string iGroupId = TableCommon.GetStringFromPointStarConfig (curPointStarIndex, "REWARD_NUMERICAL");
				DataCenter.OpenWindow ("ASTROLOGY_REWARD_WINDOW", System.Convert.ToInt32(iGroupId));

                // 设置astrology_reward_ok_button的标志位，表明由谁调用（PackageConsumeWindow处也有调用）
                tWindow _tWin = DataCenter.GetData("ASTROLOGY_REWARD_WINDOW") as tWindow;
                string str = "AstrologyUIWindow";
                if (_tWin != null) 
                    GameCommon.GetButtonData(GameCommon.FindObject(_tWin.mGameObjUI, "astrology_reward_ok_button")).set("CALL_BY", str);
			}else
			{
				UIImageButton btn = GameCommon.FindObject(mGameObjUI, "lighten_Button").GetComponent<UIImageButton>();
				if(btn != null)
					btn.isEnabled = false;
				NetManager.RequestPointLightClick(curPointStarIndex);
			}
		}
	}
	public void SetPointStarsLight(int iCurStarIndex)
	{
		GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "lighten_Button")).set("CUR_LIGHTEN_INDEX", iCurStarIndex);
        //by chenliang
        //begin

        GameObject needPointStarStoneObj = mGameObjUI.transform.Find("astrology_info_window/lighten_infos/need_gold").gameObject;//GameCommon.FindObject(mGameObjUI, "need_gold");

        UILabel needPointStarStoneLabel = needPointStarStoneObj.transform.Find("need_gold_num").GetComponent<UILabel>();

//        GameCommon.SetUIText(mGameObjUI, "tips02", TableCommon.GetStringFromPointStarConfig(iCurStarIndex, "TIPS"));
        Transform tmpTransTips = mGameObjUI.transform.Find("astrology_info_window/lighten_infos/tips02");
        UILabel tmpLBTips = tmpTransTips.GetComponent<UILabel>();
        tmpLBTips.text = TableCommon.GetStringFromPointStarConfig(iCurStarIndex, "TIPS");

        int isNeedPointStarStone = TableCommon.GetNumberFromPointStarConfig(curStarIndex, "COST");
        if (null == curConsumeItemData)
        {
            needPointStarStoneLabel.text = "0" + "/" + isNeedPointStarStone;
            return;
        }
        needPointStarStoneLabel.text = curConsumeItemData.itemNum + "/" + isNeedPointStarStone;

        //end
		
		for(int i = 0; i < mRightGridContainer.MaxCount; i++)
		{
			GameObject starMapObj = mRightGridContainer.controlList[i];
			for(int j = 0; j < MAP_MODEL; j++)
			{
				GameObject mapChoseObj = GameCommon.FindObject (starMapObj, "stars_info_window_" + j).gameObject;

				if(i % MAP_MODEL == j)
				{
					mapChoseObj.SetActive (true);
					
					for (int iStar = 0; iStar < MAP_STARS; iStar++)
					{
						GameObject starPointObj = mapChoseObj.transform.Find ("stars_info_group/stars_point(Clone)_" + iStar).gameObject;
						GameObject starLightenObj = starPointObj.transform.Find ("stars_btn/star_lighten_icon").gameObject;
						GameObject curCheckmark = starPointObj.transform.Find ("stars_btn/Checkmark").gameObject;

						if(iStar < MAP_STARS - 1)
						{
							GameObject starLineLightObj = mapChoseObj.transform.Find ("line_group/line(Clone)_" + iStar).gameObject;
							if(i * 5 + iStar + 1 < iCurStarIndex)
							{
								TweenAlpha.Begin (starLineLightObj, 0.5f, 1f);
							}
						}


                        //by chenliang
                        //begin

// 						GameObject needPointStarStoneObj = GameCommon.FindObject (mGameObjUI, "need_gold");
// 						
// 						UILabel needPointStarStoneLabel = needPointStarStoneObj.transform.Find ("need_gold_num").GetComponent<UILabel>();
// 						
// 						GameCommon.SetUIText (mGameObjUI, "tips02", TableCommon.GetStringFromPointStarConfig (iCurStarIndex, "TIPS"));
// 						
// 						int isNeedPointStarStone = TableCommon.GetNumberFromPointStarConfig(curStarIndex, "COST");
//                        if (null == curConsumeItemData)
//                        {
//                            needPointStarStoneLabel.text = "0" + "/" + isNeedPointStarStone;
//                            return;
//                        }
//                        needPointStarStoneLabel.text = curConsumeItemData.itemNum + "/" + isNeedPointStarStone;
//----------------------------------
                        //公用部分移到外面

                        //end
						if(i * 5 + iStar + 1 < iCurStarIndex)
						{
							starLightenObj.SetActive (true);
						}
						
						if(i * 5 + iStar + 1 <= iIndexNum)
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

	void UpdateTips(int iIndex, int mPage)
	{
		if(iIndex <= iIndexNum)
		{
			if( (iIndex - 1)  / MAP_STARS > mPage)
			{
				GameCommon.FindObject (mGameObjUI, "tips03").SetActive (true);
				GameCommon.FindObject (mGameObjUI, "tips04").SetActive (false);
				GameCommon.FindObject (mGameObjUI, "lighten_infos").SetActive (false);
			}else if((iIndex - 1) / MAP_STARS == mPage)
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

	public void UpdateStarTips(int iStarsIndex)
	{
		for(int i = 0; i < mRightGridContainer.MaxCount; i++)
		{
			GameObject starMapObj = mRightGridContainer.controlList[i];
			for(int j = 0; j < MAP_MODEL; j++)
			{
                // By XiaoWen
                // Bug #13052【星相】连续点击不同的星相石放入框 响应速度过慢 TIPS介绍会同时出现
                // Note 尽量避免使用SetActive来进行显示和隐藏的转换，SetActive比较耗时
                // Begin

				GameObject mapChoseObj = GameCommon.FindObject (starMapObj, "stars_info_window_" + j).gameObject;
				//mapChoseObj.SetActive (false);
				if(i % MAP_MODEL == j)
				{
					//mapChoseObj.SetActive (true);
					
					for (int iStar = 0; iStar < MAP_STARS; iStar++)
					{
						GameObject starPointObj = mapChoseObj.transform.Find ("stars_info_group/stars_point(Clone)_" + iStar).gameObject;
						GameObject starTipsObj = starPointObj.transform.Find ("star_tips").gameObject;
						
						if(iStarsIndex == i * MAP_STARS + iStar + 1)
						{
							if(iStarsIndex % MAP_STARS == 0)
							{
                                TweenAlpha.Begin(starTipsObj, 1f, 1f);
                                TweenAlpha twB = starTipsObj.gameObject.GetComponent<TweenAlpha>();
                                twB.from = 0;
                                //twB.to = 1;
                                //twB.duration = 1;
                                //twB.delay = 0.2f;
							}
                            else
							{
                                //starTipsObj.SetActive(true);
								TweenAlpha.Begin (starTipsObj, 1f, 0f);
                                TweenAlpha twB = starTipsObj.gameObject.GetComponent<TweenAlpha>();
                                twB.from = 1;
                                //twB.to = 1;
                                //twB.duration = 1;
                                //twB.delay = 0.2f;
							}
						}
					}
				}

                // End
			}
		}
	}
	public void UpdateStarMaps()
	{
		int starIndex = 0;
		
        //by chenliang
        //begin

//		for (int i = 0; i < mRightGridContainer.MaxCount; i++)
//---------------
        for (int i = 0, count = mRightGridContainer.MaxCount; i < count; i++)

        //end
		{
			GameObject starMapObj = mRightGridContainer.controlList[i];
			for(int j = 0; j < MAP_MODEL; j++)
			{
				GameObject mapChoseObj = GameCommon.FindObject (starMapObj, "stars_info_window_" + j).gameObject;
				mapChoseObj.SetActive (false);
				if(i % MAP_MODEL == j)
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
						GameObject starQuenchObj = starPointObj.transform.Find ("stars_btn/star_quench_icon").gameObject;
						if(iStar < MAP_STARS - 1)
						{
							GameObject starLineLightObj = mapChoseObj.transform.Find ("line_group/line(Clone)_" + iStar).gameObject;
							TweenAlpha.Begin (starLineLightObj, 0.01f, 0.35f);
						}
						
						int rewardType = TableCommon.GetNumberFromPointStarConfig (starIndex, "REWARD_TYPE");
						string tipsInfos = TableCommon.GetStringFromPointStarConfig (starIndex, "TIPS");
						string rewardNum = TableCommon.GetStringFromPointStarConfig (starIndex, "REWARD_NUMERICAL");
						
						starTipsLabel.text = tipsInfos;
						
                        //by chenliang
                        //begin

// 						starTipsObj.SetActive (false);
// 						if(iStar == MAP_STARS - 1)
// 						{
// 							starTipsObj.SetActive (true);
// 						}
//------------------------------
                        // By XiaoWen
                        // Bug #13052【星相】连续点击不同的星相石放入框 响应速度过慢 TIPS介绍会同时出现
                        // Note 尽量避免使用SetActive来进行显示和隐藏的转换，SetActive比较耗时
                        // Begin

                        //starTipsObj.SetActive(iStar == MAP_STARS - 1);
                        if (iStar < MAP_STARS - 1)
                        {
                            TweenAlpha.Begin(starTipsObj, 0f, 0f);
                        }
                        else
                        {
                            TweenAlpha.Begin(starTipsObj, 0f, 1f);
                        }

                        // End
						if(iStar == MAP_STARS - 1)
						{
							starLightenObj.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
							starQuenchObj.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
						}

                        //end
						GameCommon.GetButtonData(GameCommon.FindObject(starPointObj, "stars_btn")).set("CUR_STAR_INDEX", starIndex);
					}
				}
			}
		}
	}
	//激活当前星象图
	private void ChangeToMap(int iIndex, bool changeFrag)
	{
		if (mLeftGridContainer == null || mLeftGridContainer.MaxCount <= 0)
			return;
		if (iIndex < 0 || iIndex >= mLeftGridContainer.MaxCount)
			return;
		
		int indexDelta = 0;
		if (mCurrSelIndex != -1)
			indexDelta = Mathf.Abs(iIndex - mCurrSelIndex);
		mCurrSelIndex = iIndex;
		if (changeFrag)
		{
            //by chenliang
            //begin

//			UICenterOnChild tmpCenter = GameCommon.FindComponent<UICenterOnChild>(mGameObjUI, "stars_group_grid");
//----------------
            UICenterOnChild tmpCenter = mStarsGroupGridCenter;

            //end
			if (mRightGridContainer != null && tmpCenter != null && iIndex >= 0 && iIndex < mRightGridContainer.controlList.Count)
			{
				GameObject go = mRightGridContainer.controlList[iIndex];
				tmpCenter.springStrength = (indexDelta == 1) ? 8 : 9999999;
				tmpCenter.CenterOn(go.transform);
			}
		}
		MoveCurMapToShow(iIndex);
	}

	/// <summary>
	/// 将当前选择的星象地图移动到显示区域
	/// </summary>
	private void MoveCurMapToShow(int iIndex)
	{
		if (mLeftGridScrollView == null || mLeftGridContainer == null || mLeftGridContainer.MaxCount <= 0)
			return;
		
		int tmpMaxCount = mLeftGridContainer.MaxCount;
		mCurrSelIndex = iIndex;
		if (mCurrSelIndex < 0 || mCurrSelIndex >= tmpMaxCount)
			return;

		GameObject tmpGoCurrItem = mLeftGridContainer.controlList[mCurrSelIndex];
		if(mCurrSelIndex == 0 || mCurrSelIndex == mLeftGridContainer.MaxCount - 1)
		{
			return;
		}
		//为保证所选物品能完全显示，现让上下相连物品中心点也显示
		bool isUpShow = mLeftPanel.IsVisible(mLeftGridContainer.controlList[mCurrSelIndex - 1].transform.position);
		bool isDownShow = mLeftPanel.IsVisible(mLeftGridContainer.controlList[mCurrSelIndex + 1].transform.position);
		if (!isUpShow)
			mLeftGridScrollView.SetDragAmount(0.0f, (float)(mCurrSelIndex - 1) / (float)(tmpMaxCount - 1), false);
		else if (!isDownShow)
			mLeftGridScrollView.SetDragAmount(0.0f, (float)(mCurrSelIndex + 1) / (float)(tmpMaxCount - 1), false);
		else if (!mLeftPanel.IsVisible(tmpGoCurrItem.transform.position))
			mLeftGridScrollView.SetDragAmount(0.0f, (float)mCurrSelIndex / (float)(tmpMaxCount - 1), false);
	}

	public void UpdateLightConstellation(int iConstellationIndex)
	{
		
        //by chenliang
        //begin

//		for(int i = 0; i < mLeftGridContainer.MaxCount; i++)
//---------------
        for (int i = 0, count = mLeftGridContainer.MaxCount; i < count; i++)

        //end
		{
			GameObject obj = mLeftGridContainer.controlList[i];
			GameObject constellationLightenObj =  obj.transform.Find ("constellation_piont_button/constellation_lighten_icon").gameObject;
            //by chenliang
            //begin

// 			constellationLightenObj.SetActive(false);
// 			if(i <= iConstellationIndex / MAP_STARS)
// 			{
// 				constellationLightenObj.SetActive(true);
// 			}
//-------------------
            constellationLightenObj.SetActive(i <= iConstellationIndex / MAP_STARS);

            //end
		}
	}
	public void UpDateButtons()
	{	
	    //by chenliang
        //begin

//		for(int i = 0; i < mLeftGridContainer.MaxCount; i++)
//--------------
        for (int i = 0, count = mLeftGridContainer.MaxCount; i < count; i++)

        //end
		{
			GameObject obj = mLeftGridContainer.controlList[i];
			int iIndex = MAP_STARS * i + 1;
			
			string constellationName = TableCommon.GetStringFromPointStarConfig(iIndex, "POINT_STAR_NAME");
			UILabel constellationNameLabel = obj.transform.Find ("constellation_piont_button/constellation_name").GetComponent<UILabel>();
			UISprite constellationLightenIcon =  obj.transform.Find ("constellation_piont_button/constellation_lighten_icon").GetComponent<UISprite>();
			UISprite constellationQuenchIcon =  obj.transform.Find ("constellation_piont_button/constellation_quench_icon").GetComponent<UISprite>();
			
			string strAtlasName = TableCommon.GetStringFromPointStarConfig(iIndex, "POINT_ATLAS");
			string strLightenSpriteName = TableCommon.GetStringFromPointStarConfig(iIndex, "POINT_ICON");
			string strQuenchSpriteName = TableCommon.GetStringFromPointStarConfig(iIndex, "DARK_ICON");
			GameCommon.SetIcon (constellationLightenIcon, strAtlasName, strLightenSpriteName);
			GameCommon.SetIcon (constellationQuenchIcon, strAtlasName, strQuenchSpriteName);
			constellationNameLabel.text = constellationName;

			GameCommon.GetButtonData(GameCommon.FindObject(obj, "constellation_piont_button")).set("CONSTELLATION_INDEX", i);
		}
	}
	/// <summary>
	/// 激活指定索引的地图列表元素
	/// </summary>
	/// <param name="dstIndex"></param>
	private void ToggleConstellation(int dstIndex)
	{
		if (mLeftGridContainer == null)
			return;
		if (dstIndex < 0 || dstIndex >= mLeftGridContainer.controlList.Count)
			return;
		GameObject tmpGoConstellationItem = mLeftGridContainer.controlList[dstIndex];
		UIToggle tmpGoToggle = GameCommon.FindComponent<UIToggle>(tmpGoConstellationItem, "constellation_piont_button");
		if (tmpGoToggle == null)
			return;
		tmpGoToggle.value = true;
	}

	public override void Close()
	{
		base.Close();
		DataCenter.CloseWindow("BACK_GROUP_ASTROLOGY_WINDOW");
	}
}
//点击左边星象按钮
public class Button_ConstellationPiontButton : CEvent
{
	public override bool _DoEvent()
	{		
		int starIndex = get ("CONSTELLATION_INDEX");
		DataCenter.SetData ("ASTROLOGY_UI_WINDOW" , "CHANGE_CONSTELLATTON", starIndex);
		return true;
	}
}
public class Button_AstrologyBackBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow("ASTROLOGY_UI_WINDOW");
		MainUIScript.Self.OpenMainWindowByIndex (MAIN_WINDOW_INDEX.RoleSelWindow);
		return true;
	}
}
class Button_arrow_up_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ASTROLOGY_UI_WINDOW" , "UP_AND_DOWN", -1);
		return true;
	}
}
class Button_arrow_down_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ASTROLOGY_UI_WINDOW" , "UP_AND_DOWN", 1);
		return true;
	}
}
public class Button_StarsBtn : CEvent
{
	public override bool _DoEvent()
	{
		int starIndex = get ("CUR_STAR_INDEX");
		DataCenter.SetData ("ASTROLOGY_UI_WINDOW", "UPDATE_TIPS", starIndex);
		return true;
	}
}
public class Button_LightenButton : CEvent
{
	public override bool _DoEvent()
	{
		int starIndex = get ("CUR_LIGHTEN_INDEX");
		DataCenter.SetData ("ASTROLOGY_UI_WINDOW", "UPDATE_BUTTON_INFO", starIndex);
		return true;
	}
}
public class Button_AastrologyGoBackBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ASTROLOGY_UI_WINDOW" , "UP_AND_DOWN", -1);
		return true;
	}
}

public class Button_AstrologyGoForwardBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("ASTROLOGY_UI_WINDOW" , "UP_AND_DOWN", 1);
		return true;
	}
}