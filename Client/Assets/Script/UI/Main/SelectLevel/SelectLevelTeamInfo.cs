using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

public class SelectLevelTeamInfo : MonoBehaviour {

//	public GameObject mPlayer;
//	public float mfScaleRatio = 1.0f;

	void OnEnable () {
		DataCenter.Self.registerData("SELECT_LEVEL_TEAM_INFO_WINDOW", new SelectLevelTeamInfoWindow(gameObject));

//		mfScaleRatio = 2.0f;
//		InitActiveBirthForUI();
	}
	
//	public void InitActiveBirthForUI()
//	{
//		if(mPlayer != null)
//		{
//			ActiveBirthForUI activeBirthForUI = mPlayer.GetComponent<ActiveBirthForUI>();
//			if (activeBirthForUI != null)
//			{
//				if (activeBirthForUI.mIndex == 0)
//				{
//					RoleLogicData logicData = RoleLogicData.Self;
//					activeBirthForUI.mBirthConfigIndex = RoleLogicData.GetMainRole().mModelIndex;
//				}
//				activeBirthForUI.Init(false, mfScaleRatio);
//			}
//		}
//	}
}

public class SelectLevelTeamInfoWindow : tWindow
{
	TeamInfoCommonUI mTeamInfoCommonUI;
	float mfScaleRatio = 2.0f;
	public SelectLevelTeamInfoWindow(GameObject obj)
	{
		mGameObjUI = obj;
	}

	public override void Init()
	{
		base.Init ();
		InitActiveBirthForUI();

		mTeamInfoCommonUI = new TeamInfoCommonUI();
		mTeamInfoCommonUI.Init(mGameObjUI, "SELECT_LEVEL_TEAM_INFO_WINDOW");

		ShowRoleInfo();
	}

	public void InitActiveBirthForUI()
	{
        GameObject uiPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");

        if (uiPoint == null)
            return;

		ActiveBirthForUI activeBirthForUI = uiPoint.GetComponent<ActiveBirthForUI>();
		if (activeBirthForUI != null)
		{
			if (activeBirthForUI.mIndex == 0)
			{
				RoleLogicData logicData = RoleLogicData.Self;
				activeBirthForUI.mBirthConfigIndex = RoleLogicData.GetMainRole().tid;
			}
			activeBirthForUI.Init(false, mfScaleRatio);
		}
	}

	public void ShowRoleInfo()
	{
		RoleLogicData roleLogicData = RoleLogicData.Self;
		RoleData roleData = RoleLogicData.GetMainRole();
		string playerName = roleLogicData.name;
		int roleLevel = roleData.level;
		int roleModelIndex = roleData.tid;
		int roleHp = GameCommon.GetBaseMaxHP(roleModelIndex, roleLevel, 0);
		int roleMp = GameCommon.GetBaseMaxMP(roleModelIndex, roleLevel, 0);
		int roleAttack = (int)GameCommon.GetBaseAttack(roleModelIndex, roleLevel, 0);
		int roleElement = TableCommon.GetNumberFromActiveCongfig(roleModelIndex, "ELEMENT_INDEX");
		string strElementName = TableCommon.GetStringFromElement (roleElement, "ELEMENT_NAME");

		GameCommon.SetUIText(mGameObjUI, "player_name", playerName);
		GameCommon.SetUIText(mGameObjUI, "player_level", "LV" + roleLevel.ToString());
		GameCommon.SetUIText(mGameObjUI, "life_num", roleHp.ToString());
		GameCommon.SetUIText(mGameObjUI, "damage_num", roleAttack.ToString());
		GameCommon.SetUIText(mGameObjUI, "magic_num", roleMp.ToString());
		GameCommon.SetUIText(mGameObjUI, "attribute", strElementName);
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
		case "BACK_OR_FORWARD":
			mTeamInfoCommonUI.BackOrForward ((int)objVal);
			break;
//		case "OPEN_AND_CLOSE_PLAYER":
//			SetVisible ("player", (bool)objVal);
//			break;
		case "TEAM_REFRESH":
			mTeamInfoCommonUI.Refresh (null);
			break;
		}
	}
}
	
	