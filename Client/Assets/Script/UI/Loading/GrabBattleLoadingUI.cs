using UnityEngine;
using System.Collections;

/// <summary>
/// 夺宝战斗界面加载
/// </summary>
public class GrabBattleLoadingUI : BattleLoadingUI
{
}

/// <summary>
/// 夺宝战斗窗口加载
/// </summary>
public class GrabBattleLoadingWindow : BattleLoadingWindow
{
    protected override void _OnStartBattle()
    {
        GrabTreasureWindow.StartGrabBattle();
//         if (MainProcess.mStage != null)
//             MainProcess.mStage.Start();
    }
}
