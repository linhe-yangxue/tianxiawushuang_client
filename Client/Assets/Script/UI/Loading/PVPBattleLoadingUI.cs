using UnityEngine;
using System.Collections;
using Logic;

/// <summary>
/// PVP界面加载
/// </summary>
public class PVPBattleLoadingUI : BattleLoadingUI
{
}

/// <summary>
/// PVP窗口加载
/// </summary>
public class PVPBattleLoadingWindow : BattleLoadingWindow
{
    protected override void _OnStartLoading(object param)
    {
        _SetLoadingProgress(1.0f);
    }

    protected override void _OnLoadFinished()
    {
        object obj;
        if (getData("OPEN", out obj))
        {
            TM_WaitShowVictoryWindow tempEvent = obj as TM_WaitShowVictoryWindow;
            tempEvent._OnOverTime();
        }
    }
}
