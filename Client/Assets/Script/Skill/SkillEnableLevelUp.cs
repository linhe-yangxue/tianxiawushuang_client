using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillEnableLevelUp : MonoBehaviour {

	// only one case, skill can level up
	// 1. coin > min skill level up cost coin
	// 2. skill boos > min skill level up cost books
	// 3. user level > min skill level

	/// <summary>
	/// val = 2 <=> open skill index 0, 1
	/// </summary>
	public static int usrOpenSkills = 0;
	[HideInInspector]
	public bool enableOpenLevelUp = false;

	void Start() {
		gameObject.SetActive(false);
	}

	void ComputeSkillEnable() {
		if(usrOpenSkills > 0) {
            ActiveData activeData = TeamPosInfoWindow.mCurActiveData;
			int playLevel = activeData.level;

			for(int i = 0; i < usrOpenSkills; i ++) {
				if(activeData.skillLevel[i] < playLevel) {
					int bookCost = TableCommon.GetNumberFromSkillCost(activeData.skillLevel[i], "SKILL_BOOK_COST");

					ConsumeItemLogicData itemData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
					int usrBookNum = itemData.GetDataByTid((int)ITEM_TYPE.SKILL_BOOK).itemNum;

					if(usrBookNum >= bookCost) {
						int coinCost = TableCommon.GetNumberFromSkillCost(activeData.skillLevel[i], "MONEY_COST");
						int usrCoinCost = RoleLogicData.Self.gold;
						if(usrCoinCost >= coinCost) {
							enableOpenLevelUp = true;
							return;
						} else {
							enableOpenLevelUp = false;
						}
					} else {
						enableOpenLevelUp = false;
					}
				} else {
					enableOpenLevelUp = false;
				}
			}

		}

	}

	void ReceiverMessage() {
		ComputeSkillEnable();
		if(enableOpenLevelUp) {
			gameObject.SetActive(false);
		}else {
			gameObject.SetActive(true);
		}
	}
}
