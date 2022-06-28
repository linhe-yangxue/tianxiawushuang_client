using UnityEngine;
using System.Collections;

/// <summary>
/// PVP4Vs4战斗界面加载
/// </summary>
public class PVPFourVsFourBattleLoadingUI : BattleLoadingUI
{
}

/// <summary>
/// PVP4Vs4战斗窗口加载
/// </summary>
public class PVPFourVsFourBattleLoadingWindow : BattleLoadingWindow
{
    public override void Open(object param)
    {
        base.Open(param);

        DataCenter.SetData("PVP_FOUR_VS_FOUR_VICTORY_PREDICT_WINDOW", "INIT_BATTLE", true);
    }
    public override void OnClose()
    {
        //和进度条一起消失
        DataCenter.CloseWindow("PVP_FOUR_VS_FOUR_VICTORY_PREDICT_WINDOW");

        base.OnClose();
    }

    protected override void _OnStartLoading(object param)
    {
//        DataCenter.SetData("PVP_FOUR_VS_FOUR_VICTORY_PREDICT_WINDOW", "INIT_BATTLE", true);

        base._OnStartLoading(param);
    }

    protected override void _OnStartBattle()
    {
        DataCenter.SetData("PVP_FOUR_VS_FOUR_VICTORY_PREDICT_WINDOW", "START_BATTLE", true);

//         if (MainProcess.mStage != null)
//             MainProcess.mStage.Start();
    }
}
