using UnityEngine;
using System.Collections;


public class PeakBreakWindow : tWindow
{
    public override void Open(object param)
    {
        base.Open(param);

        PeakAccountInfo info = param as PeakAccountInfo;
        AddButtonAction("peak_break_close", () => OnClickClose(info));

        int raise = info.mPreBestRank - info.mPostBestRank;

        SetText("break_rank_num", info.mPostBestRank.ToString());
        SetText("break_raise_num", (info.mPreBestRank - info.mPostBestRank).ToString());
        SetText("break_award_num", info.mBreakAward.ToString());
    }

    private void OnClickClose(PeakAccountInfo info)
    {
        Close();
        DataCenter.OpenWindow("PEAK_TREASURE_WINDOW", info);
    }
}