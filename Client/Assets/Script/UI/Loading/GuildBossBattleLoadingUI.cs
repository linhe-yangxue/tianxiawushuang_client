using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

/// <summary>
/// 天魔战斗界面加载
/// </summary>
public class GuildBossBattleLoadingUI : BattleLoadingUI
{
}

/// <summary>
/// 天魔战斗窗口加载
/// </summary>
public class GuildBossBattleLoadingWindow : BattleLoadingWindow
{
    protected override void _OnStartBattle()
    {
        GuildBoss boss = UnionBase.guildBossObject;
        GuildBossBattle battle = MainProcess.mStage as GuildBossBattle;
        int stageId = DataCenter.Get("CURRENT_STAGE");
        DataRecord stageRecord = DataCenter.mStageTable.GetRecord(stageId);

        if (battle != null && stageRecord != null)
        {
            battle.mBossID = stageRecord["HEADICON"];
            battle.mBossLevel = 1;
            battle.mBossHp = boss.monsterHealth;
            battle.mMaxDamageToBoss = BattleProver.GetMaxLimit(BATTLE_PROVE_TYPE.GUILD, GameCommon.GetTeamTotalAttack());
//            battle.Start();
        }
    }
}