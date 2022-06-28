using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

public class AstrologyWindow : tWindow
{
	int curStarIndex = 0;
	int iIndexNum = 0;
	UIGridContainer mLeftGrid;
	UIScrollView mLeftView;
	UIPanel mLeftPanel;

	public override void Init()
	{
//		EventCenter.Self.RegisterEvent("Button_astrology_back_btn", new DefineFactory<Button_astrology_back_btn>());
//		EventCenter.Self.RegisterEvent ("Button_constellation_piont_button", new DefineFactory<Button_constellation_piont_button>());

		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mPointStarConfig.GetAllRecord ())
		{
			if(v.Key != null)
			{
				iIndexNum ++;
			}
		}
	}

	public override void Open(object param)
	{
		base.Open (param);
		curStarIndex = PointStarLogicData.Self.mCurIndex;

		mLeftGrid = GetComponent<UIGridContainer>("constellation_group_grid");
		mLeftView = GetComponent<UIScrollView>("Scroll View");
		mLeftPanel = GetComponent<UIPanel>("Scroll View");

		DataCenter.OpenWindow ("BACK_GROUP_ASTROLOGY_WINDOW");
		DataCenter.OpenWindow ("ASTROLOGY_INFO_WINDOW", curStarIndex);
		Refresh(param);
	}

	public override bool Refresh (object param)
	{
		UpDateButtons();
		UpdateLightConstellation(curStarIndex - 1);
		return true;
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
		switch (keyIndex)
		{
		case "SUCCESS_POINT_STAR":
			SuccessPointStar((string)objVal);
			break;
		case "UPDATE_ASTROLOGYR_INDEX":
			SetAstrologyIndex((int)objVal);
			break;
		case"CHANGE_TAB_POS":
			ChangeTabPos ((int)objVal);
			break;
		}
	}
	void ChangeTabPos(int pos)
	{
		if(pos < 0 || pos > mLeftGrid.MaxCount - 1) return;

		if(pos == 0 || pos == mLeftGrid.MaxCount - 1) 
		{
			mLeftView.SetDragAmount (0, pos/(mLeftGrid.MaxCount - 1), false);
			return;
		}

		bool bBack = mLeftPanel.IsVisible (mLeftGrid.controlList[pos - 1].transform.position);        // panel.ConstrainTargetToBounds (backObj.transform, false);
		bool bBefore = mLeftPanel.IsVisible (mLeftGrid.controlList[pos + 1].transform.position);     // panel.ConstrainTargetToBounds (beforeObj.transform, false);

		if(!bBack) 
		{
			mLeftView.SetDragAmount (0, (float)(pos - 1)/(float)mLeftGrid.MaxCount, false);
		}else if(!bBefore)
		{
			mLeftView.SetDragAmount (0, (float)(pos + 1)/(float)mLeftGrid.MaxCount, false);
		}
	}

	public void SetAstrologyIndex(int iIndex)
	{
		GameObject constellationObj = GameCommon.FindObject (mGameObjUI, "constellation_info_window").gameObject;
		UIGridContainer constellationGrid = constellationObj.transform.Find ("Scroll View/constellation_group_grid").GetComponent<UIGridContainer>();
		constellationGrid.MaxCount = iIndexNum / 5;
		
		for(int i = 0; i < constellationGrid.MaxCount; i++)
		{
			GameObject obj = constellationGrid.controlList[i];
			if(i == iIndex)
			{
				GameCommon.ToggleTrue (GameCommon.FindObject (obj, "constellation_piont_button"));
			}
		}
	}
	void SuccessPointStar(string text)
	{
		SC_PointLightenClick item = JCode.Decode<SC_PointLightenClick>(text);
		if( null == item )
			return ;

        curStarIndex = item.currentIndex;

		if((curStarIndex - 1) % 5 == 0)
		{
			if(curStarIndex < iIndexNum )
			{
				DataCenter.SetData ("ASTROLOGY_INFO_WINDOW" , "UPDATE_STAR_WINDOW", curStarIndex / 5);
			}else 
			{
				DataCenter.SetData ("ASTROLOGY_INFO_WINDOW" , "UPDATE_STAR_WINDOW", (iIndexNum - 1) / 5);
			}
		}

		UpdateLightConstellation(curStarIndex - 1);
	}

	public void UpdateLightConstellation(int iConstellationIndex)
	{
		GameObject constellationObj = GameCommon.FindObject (mGameObjUI, "constellation_info_window").gameObject;
		UIGridContainer constellationGrid = constellationObj.transform.Find ("Scroll View/constellation_group_grid").GetComponent<UIGridContainer>();
		constellationGrid.MaxCount = iIndexNum / 5;
		
		for(int i = 0; i < constellationGrid.MaxCount; i++)
		{
			GameObject obj = constellationGrid.controlList[i];
			GameObject constellationLightenObj =  obj.transform.Find ("constellation_piont_button/constellation_lighten_icon").gameObject;
			constellationLightenObj.SetActive(false);
			if(i <= iConstellationIndex / 5)
			{
				constellationLightenObj.SetActive(true);
			}
		}
	}

	public void UpDateButtons()
	{
		GameObject constellationObj = GameCommon.FindObject (mGameObjUI, "constellation_info_window").gameObject;

		UIGridContainer constellationGrid = constellationObj.transform.Find ("Scroll View/constellation_group_grid").GetComponent<UIGridContainer>();
		constellationGrid.MaxCount = iIndexNum / 5;

		for(int i = 0; i < constellationGrid.MaxCount; i++)
		{
			GameObject obj = constellationGrid.controlList[i];
			int iIndex = 5 * i + 1;

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

			GameCommon.GetButtonData (GameCommon.FindObject(obj, "constellation_piont_button")).set ("POS", i);
			GameCommon.GetButtonData(GameCommon.FindObject(obj, "constellation_piont_button")).set("CONSTELLATION_INDEX", i);
		}
	}

	public override void Close()
	{
		base.Close();
		DataCenter.CloseWindow("BACK_GROUP_ASTROLOGY_WINDOW");
	}
}
public class Button_constellation_piont_button : CEvent
{
	public override bool _DoEvent()
	{
		int pos = (int)getObject ("POS");
		DataCenter.SetData ("ASTROLOGY_WINDOW", "CHANGE_TAB_POS", pos);

		int starIndex = get ("CONSTELLATION_INDEX");
		DataCenter.SetData ("ASTROLOGY_INFO_WINDOW" , "UPDATE_STAR_WINDOW", starIndex);
		return true;
	}
}

//----------------------------------------------------------------------------------
// astrology window back
public class Button_astrology_back_btn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow("ASTROLOGY_WINDOW");
		MainUIScript.Self.OpenMainWindowByIndex (MAIN_WINDOW_INDEX.RoleSelWindow);
		return true;
	}
}