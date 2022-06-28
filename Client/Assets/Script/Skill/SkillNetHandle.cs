using UnityEngine;
using System.Collections;

public class SkillNetHandle {

	public static void RequestSkillLevelUp(int itemId, int skillIndex, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestSkillLevelUp lv = new CS_RequestSkillLevelUp();
		lv.itemId = itemId;
		lv.skillIndex = skillIndex;
		HttpModule.Instace.SendGameServerMessage(lv, "CS_UpgradeSkill", success, fail);
	}
}
