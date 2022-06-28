using UnityEngine;
using System.Collections;

// below ui info
public class NewSkillDescribeInfo : MonoBehaviour {
	//skill desc
	public UILabel skillName;
	public UILabel currentSkillLevel;
	public UILabel nextSkillLevel;
	public UILabel skillDesc;
	public string skillDescStr;
	// end desc

	public void SetSkillName(string skiName) {
		skillName.text = skiName;
	}

	public void SetCurrentSkillLevel(int level) {
		currentSkillLevel.text = level.ToString();
	}

	public void SetNextSkillLevel(int level) {
		nextSkillLevel.text = level.ToString();
	}

	public void SetSkillDescribe(string desc) {
		skillDesc.text = desc;
	}

	public void SetSkillDescribe() {
		SetSkillDescribe(skillDescStr);
	}

	// upgrade material
	public UISprite matIcon;
	public UILabel matName;
	public UILabel matNum;
	public UILabel coin;
	// end material
	public void SetMaterialIcon(string spriteName) {
		matIcon.spriteName = spriteName;
	}

	public void SetMaterialName(string name) {
		matName.text = name;
	}

	public void SetMaterialNum(string rateNum) {
		matNum.text = rateNum;
	}

	public void SetCastCoin(int coins) {
        string coincost = null;
        coincost = coins.ToString();
        if (coins >RoleLogicData.Self.gold)
        {
            coincost = "[ff0000]" + coincost;
	 
        }
        coin.text = coincost;
	}

	// upgrade btn
	public UIImageButton upgradeBtn;
	// end btn
	public void SetLevelUpBtnDisable() {
		upgradeBtn.isEnabled = false;
	}

	public void SetLevelUpBtnEnable() {
		upgradeBtn.isEnabled = true;
	}
}
