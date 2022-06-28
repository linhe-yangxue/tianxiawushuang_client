using UnityEngine;
using System.Collections;
using Logic;

/// <summary>
/// 天魔战斗界面加载
/// </summary>
public class BossBattleLoadingUI : BattleLoadingUI
{
}

/// <summary>
/// 天魔战斗窗口加载
/// </summary>
public class BossBattleLoadingWindow : BattleLoadingWindow
{
    protected override void _OnStartBattle()
    {
        object obj;
        if (DataCenter.Self.getData("START_BOSS_BATTLE", out obj))
        {
            BossBattleStartData startData = obj as BossBattleStartData;
            //Boss_GetDemonBossList_BossData bossData = startData.bossData;
            BossRaidWindow.InitBossBattle(startData);
//             if (MainProcess.mStage != null)
//                 MainProcess.mStage.Start();
        }
    }
}
