using UnityEngine;
using System.Collections;


public class ArenaBreakWindow : tWindow
{
    public override void Open(object param)
    {
        base.Open(param);

        ArenaNetManager.ArenaAccountInfo info = param as ArenaNetManager.ArenaAccountInfo;
        AddButtonAction("peak_break_close", () => OnClickClose(info));

        int raise = info.mPreBestRank - info.mPostBestRank;

        SetText("break_rank_num", getShowRank(info.mPostBestRank).ToString());
        SetText("break_raise_num", (info.mPreBestRank - info.mPostBestRank).ToString());
        SetText("break_award_num", info.mBreakAward.ToString());
    }

    public int getShowRank(int rank)
    {
        return rank + 1;
    }

    private void OnClickClose(ArenaNetManager.ArenaAccountInfo info)
    {
        Close();
        DataCenter.OpenWindow("ARENA_TREASURE_WINDOW", info);
    }
}