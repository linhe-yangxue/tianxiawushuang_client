using UnityEngine;
using System.Collections;

public class NewSkillBean : MonoBehaviour {

	public UISprite background;
	public UISprite skill_frame;
	public UISprite skill_flag;// 主|被动技能
	public UISprite skill_locked;
	[HideInInspector]
	public bool enableUpgrade;// 该技能是否可以升级
	public UILabel skill_level;
	public UILabel skill_desc;
	public UILabel skill_name;
	public UILabel skill_level_bottom_lbl;

	// pros
	public int skillId;
	public string icon;
	public int openLevel;
	public string skillDesc;
	public string skillName;
	//public UIAtlas atlasName;
	// end pros
	private int _skillLv = 0;

	public void UnlockSkill() {
		skill_locked.gameObject.SetActive(false);
		enableUpgrade = true;
	}

	public void LockSkill() {
		//modified by xuke
//		skill_locked.gameObject.SetActive(true);
		skill_locked.gameObject.SetActive(false);
		//end
		enableUpgrade = false;
	}

	public void SetSkillLevel(int val) {
		_skillLv = val;
		skill_level.gameObject.SetActive(true);
		skill_level.text = "Lv." + val.ToString();
		if(val == 0) {
			skill_level.gameObject.SetActive(false);
		}
	}

	public void SetSkillIcon(string atlas, string name) {
        background.atlas = GameCommon.LoadUIAtlas(atlas);
		background.spriteName = name;
	}

	public void SetSkillDesc(string desc) {
		skill_desc.text = desc;
	}

	// fun for desc skill, call by internal ngui
	public void OnClick() {
		SetSkillDesc(skillDesc);
		print("MyBean:" + gameObject.name);
		//设置当前选中技能的等级和名称
		skill_name.text = skillName;
		skill_level_bottom_lbl.text = _skillLv.ToString();
	}
}
