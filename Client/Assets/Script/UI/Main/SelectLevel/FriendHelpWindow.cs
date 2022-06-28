using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;

public class FriendHelpWindow : tWindow
{
	UIGridContainer mSelectFriendGrid;
	private int mFriendHelpCDTime; //2 hour
	SORT_TYPE mSortType = SORT_TYPE.STAR_LEVEL;
	
	public override void Init()
	{
		mFriendHelpCDTime = (int)DataCenter.mGlobalConfig.GetData("FRIEND_HELP_CD_TIME", "VALUE");
	}

	public override void Open (object param)
	{
		base.Open (param);
		if(!(bool)param)
		{
			mSelectFriendGrid = GetComponent<UIGridContainer>("grid");
			mSelectFriendGrid.MaxCount = 0;
		}
		GameCommon.CloseUIPlayTween (mSortType);
	}

	public override void OnOpen ()
	{
		base.OnOpen ();

		/*
		if(!get ("OPEN"))
		{
			tEvent evt = Net.StartEvent("CS_RequestFriendList");
			evt.set("IS_GAME_FRIEND_LIST", false);
			evt.DoEvent();
		}
		else
			Refresh (SORT_TYPE.STAR_LEVEL);
			*/
	}

	public override bool Refresh (object param)
	{
		SelectFriend(param);
		return true;
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
		switch (keyIndex)
		{
		case "SORTING":
			SelectFriend(objVal);
			break;	
		case "SELECT_FRIEND":
			GameObject selectObj = (GameObject)objVal;
			Selected(selectObj, true);
			break;
		case "CANCEL_SELECT_FRIEND":
			GameObject cancelSelectObj = (GameObject)objVal;
			Selected(cancelSelectObj, false);
			break;
		case "IS_SELECTED":
			object isSelectedObj;
			getData("SELECT_FRIEND", out isSelectedObj);
			
			GameObject selectedObj = (GameObject)isSelectedObj;
			if (selectedObj != null)
			{
				Selected(selectedObj, false);
			}
			break;
		}
	}

	void GetSortListParamFromFriendaData(FriendData a, FriendData b, out SortListParam sStarLevel, out SortListParam sLevel, out SortListParam sElement)
	{
		sStarLevel = new SortListParam{mParam1 = TableCommon.GetNumberFromActiveCongfig(a.mModel, "STAR_LEVEL"), 
			mParam2 = TableCommon.GetNumberFromActiveCongfig(b.mModel, "STAR_LEVEL"), bIsDescending = true};
		sLevel = new SortListParam{mParam1 = a.mLevel, mParam2 = b.mLevel, bIsDescending = true};
		sElement = new SortListParam{mParam1 = TableCommon.GetNumberFromActiveCongfig(a.mModel, "ELEMENT_INDEX"), 
			mParam2 = TableCommon.GetNumberFromActiveCongfig(b.mModel, "ELEMENT_INDEX"), bIsDescending = false};
	}
	
	int SortMethodsByStarLevel(FriendData a, FriendData b)
	{
		SortListParam sStarLevel, sLevel, sElement;
		GetSortListParamFromFriendaData(a, b, out sStarLevel, out sLevel, out sElement);
		return GameCommon.Sort (a.mIsFriend, b.mIsFriend, true, sStarLevel, sLevel, sElement);
	} 
	
	int SortMethodsByPlayerLevel(FriendData a, FriendData b)
	{
		SortListParam sStarLevel, sLevel, sElement;
		GetSortListParamFromFriendaData(a, b, out sStarLevel, out sLevel, out sElement);
		return GameCommon.Sort (a.mIsFriend, b.mIsFriend, true, sLevel, sStarLevel, sElement);
	} 
	
	int SortMethodsByElement(FriendData a, FriendData b)
	{
		SortListParam sStarLevel, sLevel, sElement;
		GetSortListParamFromFriendaData(a, b, out sStarLevel, out sLevel, out sElement);
		return GameCommon.Sort (a.mIsFriend, b.mIsFriend, true, sElement, sStarLevel, sLevel);
	} 

	void SelectFriend(object iWhichSort)
	{
		FriendLogicData friendLogicData = DataCenter.GetData("HELP_FRIEND_LIST") as FriendLogicData;
		if (friendLogicData != null)
		{
			List<FriendData> friendDataList = friendLogicData.arr.ToList();

			if ((SORT_TYPE)iWhichSort == SORT_TYPE.LEVEL)
			{
				friendDataList = GameCommon.SortList (friendDataList, SortMethodsByPlayerLevel);
			}
			else if ((SORT_TYPE)iWhichSort == SORT_TYPE.STAR_LEVEL)
			{
				friendDataList = GameCommon.SortList (friendDataList, SortMethodsByStarLevel);
			}
			else if ((SORT_TYPE)iWhichSort == SORT_TYPE.ELEMENT_INDEX)
			{
				friendDataList = GameCommon.SortList (friendDataList, SortMethodsByElement);
			}

			int iCount = friendDataList.Count;
			int iMaxCount = friendDataList.Count;
			FriendData friendIsCDData = null;
			for (int j = 0; j < iCount; j++)
			{
				String name = friendDataList[j].name;
				if (friendDataList[j].mIsFriend && (friendDataList[j].mCombatTime + mFriendHelpCDTime - CommonParam.NowServerTime()) > 0)
				{
					friendIsCDData = friendDataList[j];
					friendDataList.Remove(friendDataList[j]);
					friendDataList.Insert(iMaxCount - 1, friendIsCDData);
					iCount--;
					j--;
				}
			}
			
			mSelectFriendGrid.MaxCount = iMaxCount;
			for (int i = 0; i < iMaxCount; i++)
			{
				GameObject subcell = mSelectFriendGrid.controlList[i];
				FriendData friendData = friendDataList[i];
				
				int iLimitTime = friendData.mCombatTime + mFriendHelpCDTime;
				int lastTime = (int)(iLimitTime - (Int64)CommonParam.NowServerTime());
				if (lastTime > 0)
				{
					GameCommon.SetUIVisiable(subcell, "wait_time", true);
					SetCountdownTime(subcell, "wait_time", (Int64)iLimitTime, new CallBack(this, "FriendHelpCDIsOver", subcell));
				}
				else
					GameCommon.SetUIVisiable(subcell, "wait_time", false);

				PetData friendPetData = new PetData();
				SetPetData (friendPetData, friendData);
				SetSelectButtonData (subcell, friendData, friendPetData);
			
				int iPetID = friendData.mModel;
				GameCommon.SetPetIconWithElementAndStarAndLevel(subcell, "friend_icon", "element", "star_level_label", "level_label", friendData.mLevel, iPetID);
				
				UIGridContainer skillGrid = GameCommon.FindObject(subcell, "skill_grid").GetComponent<UIGridContainer>();
				skillGrid.MaxCount = 3;
				
				if (InitActiveSkillIcon(skillGrid, iPetID))
				{
					int iIndex = 1;
					InitPassiveSkillIcon(iPetID, skillGrid.controlList[1], ref iIndex);
					InitPassiveSkillIcon(iPetID, skillGrid.controlList[2], ref iIndex);
				}
				else skillGrid.gameObject.SetActive(false);
				
				GameCommon.SetStrengthenLevelLabel(subcell, friendData.mStrengthenLevel, "strengthen_level_label");
				GameCommon.SetUIText(subcell, "friend_name", friendData.name);
				
				GameCommon.SetUIVisiable(subcell, "is_game_friend_label", friendData.mIsFriend);
				SetButtonsVisible (subcell, false);
				string iCurrentSelectFriendID = get("FRIEND_ID").ToString ();
//				if (iCurrentSelectFriendID == friendData.mID)
//				{
//					SetButtonsVisible (subcell, true);
//					SetSelectedButtonInVisualInterface ((float)i, (float)iMaxCount);
//					set ("SELECT_FRIEND", subcell);
//				}
				if((iCurrentSelectFriendID == "" && i == 0 && lastTime <= 0) || iCurrentSelectFriendID == friendData.friendId)
				{
					SetButtonsVisible (subcell, true);
					SetSelectedButtonInVisualInterface ((float)i, (float)iMaxCount);
					set ("SELECT_FRIEND", subcell);
					FriendLogicData.mSelectPlayerData = friendData;
					PetLogicData.mFreindPetData = friendPetData;
				}
			}
		}
		else
		{
			mSelectFriendGrid.MaxCount = 0;
		}
	}
	
	public void FriendHelpCDIsOver(object param)
	{
		GameObject obj = param as GameObject;
		GameCommon.SetUIVisiable(obj, "wait_time", false);
	}

	void SetPetData(PetData petData, FriendData friendData)
	{
		petData.tid = friendData.mModel;
		petData.level = friendData.mLevel;
		petData.strengthenLevel = friendData.mStrengthenLevel;
		petData.starLevel = TableCommon.GetNumberFromActiveCongfig(friendData.mModel, "STAR_LEVEL");
        petData.teamPos = -1;
        petData.SetSkillData();
	}

	void SetSelectButtonData(GameObject parentObj, FriendData friendData, PetData petData)
	{
		NiceData buttonData = GameCommon.GetButtonData(parentObj, "friend_help_select_button");
		buttonData.set("PLAYER_DATA", friendData);
		buttonData.set("FRIEND_PET_DATA", petData);
	}

	void SetButtonsVisible(GameObject parentObj, bool bVisible)
	{
		GameCommon.SetUIVisiable (parentObj, bVisible, "friend_help_cancel_button", "selected_background");
		GameCommon.SetUIVisiable (parentObj, !bVisible, "friend_help_select_button");
	}

	void SetSelectedButtonInVisualInterface(float curCount, float max)
	{
		UIScrollBar bar = GetComponent<UIScrollBar>("scroll_bar");
		bar.value = curCount / max;
	}

	void Selected(GameObject parentObj, bool b)
	{
		GameObject friendSelect = GameCommon.FindObject(parentObj, "friend_help_select_button");
		GameObject cancelFriendSelect = GameCommon.FindObject(parentObj, "friend_help_cancel_button");
		GameObject selectedBackground = GameCommon.FindObject(parentObj, "selected_background");
		friendSelect.SetActive(!b);
		cancelFriendSelect.SetActive(b);
		selectedBackground.SetActive(b);
	}
	
	bool InitActiveSkillIcon(UIGridContainer skillGrid, int iPetID)
	{
		for (int i = 1; i < 4; i++)
		{

			int iActiveSkillIndex = TableCommon.GetNumberFromActiveCongfig(iPetID, "PET_SKILL_" + i.ToString());
			if (iActiveSkillIndex != 0)
			{
				UISprite skillSprite = skillGrid.controlList[0].GetComponent<UISprite>();
				string strSkillIconName = TableCommon.GetStringFromSkillConfig(iActiveSkillIndex, "SKILL_SPRITE_NAME");
				string strSkillIconAtlas = TableCommon.GetStringFromSkillConfig(iActiveSkillIndex, "SKILL_ATLAS_NAME");
				GameCommon.SetIcon(skillSprite, strSkillIconAtlas, strSkillIconName);
				
				NiceData buttonData = GameCommon.GetButtonData(skillSprite.gameObject, "Button1");
				buttonData.set("INDEX", iActiveSkillIndex);
				buttonData.set("TABLE_NAME", "SKILL");
				return true;
			}
		}
		return false;
	}
	
	bool InitPassiveSkillIcon(int iPetID, GameObject obj, ref int index)
	{
		UISprite skillFlagIcon = GameCommon.FindObject(obj, "skill_flag").GetComponent<UISprite>();
		UISprite skillFrameIcon = GameCommon.FindObject(obj, "skill_frame").GetComponent<UISprite>();
		if (skillFlagIcon != null) skillFlagIcon.spriteName = "ui_f";
		if (skillFrameIcon != null) skillFrameIcon.spriteName = "ui_f_light";
		
		int iPassoveSkillIndex = 0;
		for (int i = index; i < 7; i++)
		{
			if (i <= 3)
			{
				iPassoveSkillIndex = TableCommon.GetNumberFromActiveCongfig(iPetID, "ATTACK_STATE_" + i.ToString());
				if (iPassoveSkillIndex != 0)
				{
					UISprite skillSprite = obj.GetComponent<UISprite>();
					string strSkillIconName = TableCommon.GetStringFromAttackState(iPassoveSkillIndex, "SKILL_SPRITE_NAME");
					string strSkillIconAtlas = TableCommon.GetStringFromAttackState(iPassoveSkillIndex, "SKILL_ATLAS_NAME");
					GameCommon.SetIcon(skillSprite, strSkillIconAtlas, strSkillIconName);
					
					NiceData buttonData = GameCommon.GetButtonData(skillSprite.gameObject, "Button1");
					buttonData.set("INDEX", iPassoveSkillIndex);
					buttonData.set("TABLE_NAME", "ATTACK_STATE");
					index = i + 1;
					return true;
				}
			}
			else if (i <= 6)
			{
				iPassoveSkillIndex = TableCommon.GetNumberFromActiveCongfig(iPetID, "AFFECT_BUFFER_" + (i - 3).ToString());
				if (iPassoveSkillIndex != 0)
				{
					UISprite skillSprite = obj.GetComponent<UISprite>();
					string strSkillIconName = TableCommon.GetStringFromAffectBuffer(iPassoveSkillIndex, "SKILL_SPRITE_NAME");
					string strSkillIconAtlas = TableCommon.GetStringFromAffectBuffer(iPassoveSkillIndex, "SKILL_ATLAS_NAME");
					GameCommon.SetIcon(skillSprite, strSkillIconAtlas, strSkillIconName);
					
					NiceData buttonData = GameCommon.GetButtonData(skillSprite.gameObject, "Button1");
					buttonData.set("INDEX", iPassoveSkillIndex);
					buttonData.set("TABLE_NAME", "AFFECT_BUFFER");
					index = i + 1;
					return true;
				}
			}
		}
		
		obj.SetActive(false);
		return false;
	}
}


public class Button_friend_help_select_button : CEvent
{
	public override bool _DoEvent()
	{
		GameObject obj = (GameObject)getObject("BUTTON");
		GameObject parentObj = obj.transform.parent.gameObject;
		
		DataCenter.SetData("FRIEND_HELP_WINDOW", "IS_SELECTED", true);
		DataCenter.SetData("FRIEND_HELP_WINDOW", "SELECT_FRIEND", parentObj);
		
		FriendData selectPlayerData = (FriendData)getObject("PLAYER_DATA");
		FriendLogicData.mSelectPlayerData = selectPlayerData;
		DataCenter.SetData("FRIEND_HELP_WINDOW", "FRIEND_ID", selectPlayerData.mID);
		
		PetData friendPetData = (PetData)getObject("FRIEND_PET_DATA");
		PetLogicData.mFreindPetData = friendPetData;
		
		return true;
	}
}

public class Button_friend_help_cancel_button : CEvent
{
	public override bool _DoEvent()
	{
		GameObject obj = (GameObject)getObject("BUTTON");
		GameObject parentObj = obj.transform.parent.gameObject;
		
		DataCenter.SetData("FRIEND_HELP_WINDOW", "CANCEL_SELECT_FRIEND", parentObj);
		DataCenter.SetData("FRIEND_HELP_WINDOW", "FRIEND_ID", 0);

		if (FriendLogicData.mSelectPlayerData != null) FriendLogicData.mSelectPlayerData = null;
		if (PetLogicData.mFreindPetData != null) PetLogicData.mFreindPetData = null;
		return true;
	}
}

public class Button_friend_help_start_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("FRIEND_HELP_WINDOW", "FRIEND_ID", 0);
		MainProcess.StartBattle(null, false);
		
		return true;
	}
}

public class Button_friend_help_element_ranking_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("FRIEND_HELP_WINDOW", "SORTING", SORT_TYPE.ELEMENT_INDEX);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

public class Button_friend_help_level_ranking_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("FRIEND_HELP_WINDOW", "SORTING", SORT_TYPE.LEVEL);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

public class Button_friend_help_star_ranking_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("FRIEND_HELP_WINDOW", "SORTING", SORT_TYPE.STAR_LEVEL);
		UILabel siftingButtonItemLabel = (getObject ("BUTTON") as GameObject).GetComponentInChildren<UILabel>();
		GameCommon.PlayUIPlayTween (siftingButtonItemLabel.text);
		return true;
	}
}

public class Button_friend_help_back_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow ("FRIEND_HELP_WINDOW");
		DataCenter.SetData("FRIEND_HELP_WINDOW", "FRIEND_ID", 0);
		if(FriendLogicData.mSelectPlayerData != null) FriendLogicData.mSelectPlayerData = null;
		if(PetLogicData.mFreindPetData != null) PetLogicData.mFreindPetData = null;
		return true;
	}
}
public class Button_friend_help_sifting_button : CEvent
{
	public override bool _DoEvent()
	{
		UIPlayTween uiPt = null;
		uiPt = GameObject.Find ("friend_help_sifting_box").GetComponent<UIPlayTween >();
		uiPt.Play (true);

		return true;
	}
}
