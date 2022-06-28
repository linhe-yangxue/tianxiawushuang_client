using UnityEngine;
using System.Collections;
using Logic;


public class PeakResultWindow : tWindow
{
    private PeakAccountInfo info;

    public override void Open(object param)
    {
        base.Open(param);

        info = param as PeakAccountInfo;
        AddButtonAction("peak_result_close", () => OnClickClose(info));

        if (info != null)
        {
            ObjectManager.Self.ClearAll();
            ShowModel();

            if (info.mIsWin)
            {
                PerformWin();
            }
            else
            {
                PerformLose();
            }

            DoCoroutine(DoPerformAward());
        }
        else
        {
            DEBUG.LogError("Invalid parameter for open battle account window");
        }
    }

    private void PerformWin()
    {
        SetVisible("win_result", true);
        SetVisible("fail_result", false);

        SetText("player_name", info.mChallengeName);
        SetText("old_rank_number", info.mPreRank.ToString());
        SetText("new_rank_number", info.mPreRank.ToString());
    }

    private void PerformLose()
    {
        SetVisible("win_result", false);
        SetVisible("fail_result", true);
    }

    private IEnumerator DoPerformAward()
    {
        float preRoleRate = (float)info.mPreRoleExp / TableCommon.GetRoleMaxExp(info.mPreRoleLevel);
        float postRoleRate = (float)info.mPostRoleExp / TableCommon.GetRoleMaxExp(info.mPostRoleLevel);

        UILabel goldLabel = GetComponent<UILabel>("gold_num");
        UILabel prestigeLabel = GetComponent<UILabel>("prestige_num");
        UILabel levelLabel = GameCommon.FindComponent<UILabel>(GetSub("level_label"), "num");
        UILabel percentLabel = GetComponent<UILabel>("percent_label");
        UIProgressBar bar = GetComponent<UIProgressBar>("Progress Bar");

        goldLabel.text = "0";
        prestigeLabel.text = "0";
        levelLabel.text = info.mPreRoleLevel.ToString();
        percentLabel.text = (int)preRoleRate + "%";
        bar.value = preRoleRate;

        if (info.mIsWin)
        {
            UILabel rankLabel = GetComponent<UILabel>("new_rank_number");
            yield return DoCoroutine(UIKIt.PushNumberLabel(rankLabel, info.mPreRank, info.mPostRank));
            yield return new WaitForSeconds(0.1f);
        }

        yield return DoCoroutine(UIKIt.PushNumberLabel(goldLabel, 0, info.mAwardGold));
        yield return new WaitForSeconds(0.1f);
        yield return DoCoroutine(UIKIt.PushNumberLabel(prestigeLabel, 0, info.mAwardReputation));
        yield return new WaitForSeconds(0.1f);
        yield return DoCoroutine(UIKIt.PushLevelBar(this, bar, percentLabel, info.mPreRoleLevel, preRoleRate, info.mPostRoleLevel, postRoleRate, x => levelLabel.text = x.ToString()));
    }

    private void ShowModel()
    {
        GameObject uiPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");
        BaseObject model = GameCommon.ShowCharactorModel(uiPoint, 1.6f);

        if (info.mIsWin)
            model.PlayAnim("cute", false);
        else
            model.PlayAnim("lose");
    }

    private void OnClickClose(PeakAccountInfo info)
    {
        Close();

        if (info.mIsWin)
        {
            if (info.mHasBreak)
            {
                DataCenter.OpenWindow("PEAK_BREAK_WINDOW", info);
            }
            else
            {
                DataCenter.OpenWindow("PEAK_TREASURE_WINDOW", info);
            }
        }
        else 
        {
            PVP4Battle.GoBack();
        }
    }
}