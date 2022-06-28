using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System;

public class BossBattleAccountWindow : BattleAccountWindow
{
	public BossBattleStartData startData;
	public Boss_GetDemonBossList_BossData bossData;
	public int lostHP;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_again_boss_battle", new DefineFactoryLog<Button_again_boss_battle>());
		EventCenter.Self.RegisterEvent("Button_quit_boss_battle", new DefineFactoryLog<Button_quit_boss_battle>());
	}
	
	protected override void Perform()
	{
        base.Perform();

        lostHP = 0;
        startData = DataCenter.Self.getObject("START_BOSS_BATTLE") as BossBattleStartData;
        bossData = startData.bossData;
        BossBattle bossBattle = MainProcess.mStage as BossBattle;
        if (bossBattle != null)
            lostHP = bossData.hpLeft - startData.leftHpAfterBattle;//bossBattle.mBoss.GetHp();

		__ShowBossInfo();
		__ShowBattleAccount();
	}

	private void __ShowBossInfo()
    {
        DataRecord bossConfig = DataCenter.mBossConfig.GetRecord(bossData.tid);
        //Boss名称
        GameCommon.FindObject(mGameObjUI, "boss_name").GetComponent<UILabel>().color = GameCommon.GetNameColorByQuality(bossData.quality);
		SetText("boss_name", bossConfig.getData("NAME"));
		//Boss等级
		SetText("boss_level", bossData.bossLevel.ToString());
		//发现者
		SetText("find_player_name", bossData.finderName);
		GameCommon.FindObject (mGameObjUI, "find_player_name").GetComponent<UILabel>().color = GameCommon.GetNameColor (bossData.finderTid);
		//Boss头像
		SetUISprite("boss_icon", bossConfig.getData("HEAD_ATLAS_NAME"), bossConfig.getData("HEAD_SPRITE_NAME"));

		//血量
        int currHP = startData.leftHpAfterBattle;
		int maxHP = BigBoss.GetBossMaxHp(bossData.quality, bossData.bossLevel);
		SetText("boss_hp", currHP.ToString() + "/" + maxHP.ToString());
		UISlider hpSlider = GameCommon.FindComponent<UISlider>(GetSub("info"), "Slider");
		if(hpSlider != null)
		{
			hpSlider.value = (float)currHP / (float)maxHP;
			GameCommon.SetUIVisiable(hpSlider.gameObject, "Foreground", Convert.ToBoolean(hpSlider.value));
		}
		GameCommon.GetButtonData (GameCommon.FindObject (mGameObjUI,"again_boss_battle")).set ("IS_SHARE_PK", bossData);
	}

	private void __ShowBattleAccount()
	{
        BossBattleAccountInfo tmpInfo = info as BossBattleAccountInfo;

		//伤害
        //by chenliang
        //begin

//		SetText("player_damage_info", lostHP.ToString());
//----------------
        //将伤害信息改为一个UILabel
        string tmpDamageInfo = string.Format("[FF3333]{0}", lostHP.ToString());
        SetText("player_damage_info", tmpDamageInfo);

        //end

		//功勋
		SetText("merit_label", tmpInfo.merit.ToString());

		//战功
		GameCommon.SetUIText(GetSub("reward_info01"), "number_label", tmpInfo.battleAchv.ToString());
        //设置魔鳞图标
        GameCommon.SetResIcon(GetSub("reward_info01"), "battleachv_icon", (int)ITEM_TYPE.BATTLEACHV, false, true);
	}
}
