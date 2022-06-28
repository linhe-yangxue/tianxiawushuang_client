using UnityEngine;
using System.Collections;

public class NewSKillUpgradeBean : MonoBehaviour {

	public NewSkillDescribeInfo infoPanel;
	public UISprite background;
	public UISprite skill_frame;
	public UISprite skill_flag;// 主|被动技能
	public UISprite skill_locked;
	[HideInInspector]
	public bool enableUpgrade;// 该技能是否可以升级
	public UILabel skill_level;

	// props
	public SkillUpgradePanelInfoData supi;
	public int skillIndex;
	public static int soleSkillIndex;
	// end props

	public void UnlockSkill() {
		skill_locked.gameObject.SetActive(false);
		enableUpgrade = true;
	}

	/// <summary>
	/// 这个方法不会使技能变灰
	/// </summary>
	public void LockSkill() 
	{
		// modified by xuke
		//skill_locked.gameObject.SetActive(true);
		skill_locked.gameObject.SetActive(false);
		// end
		enableUpgrade = false;
	}
	
	public void SetSkillLevel(int val) {
		if(val == 0) {
			skill_level.gameObject.SetActive(false);
			return;
		}
		skill_level.gameObject.SetActive(true);
		skill_level.text = "Lv." + val.ToString();

	}
	
	public void SetSkillIcon(string atlas, string name) {
        background.atlas = GameCommon.LoadUIAtlas(atlas);
		background.spriteName = name;
	}

    private static int m_nSkillLevelLimit = int.MinValue;
    public static int SKILL_LEVEL_LIMIT 
    {
        get 
        {
            if (m_nSkillLevelLimit == int.MinValue) 
            {
                m_nSkillLevelLimit = DataCenter.mSkillCost.GetRecordCount();
            }
            return m_nSkillLevelLimit;
        }
    }
	// fun for desc skill, call by internal ngui
	public void OnClick() {
		infoPanel.SetSkillName(supi.skillName);
		infoPanel.SetCurrentSkillLevel(supi.currentSkillLevel);
		infoPanel.SetNextSkillLevel(supi.currentSkillLevel + 1);
		infoPanel.SetCastCoin(supi.coin);
		infoPanel.SetMaterialNum(supi.matRateNum);
		infoPanel.SetSkillDescribe(supi.skillDesc);
        UIImageButton mbutton = GameObject.Find("skill_up_button").GetComponent<UIImageButton>();
        if (supi.currentSkillLevel >= SKILL_LEVEL_LIMIT)
        {
             mbutton.isEnabled = false;
        }
        else
        {
            if (supi.petBreakLevel >= supi.openLevel)
            {
                mbutton.isEnabled = true;
            }
            else
            {
                mbutton.isEnabled = false;
            }
        }
		soleSkillIndex = skillIndex;
        //print("MyBean:" + gameObject.name);
	}
}
