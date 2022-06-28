using UnityEngine;
using System.Collections;
using Logic;


public class ArenaResultWindow : tWindow
{
    private ArenaNetManager.ArenaAccountInfo info;

    private UIGridContainer mGridContainer;
    public override void Open(object param)
    {
        base.Open(param);

        info = param as ArenaNetManager.ArenaAccountInfo;
        AddButtonAction("arena_result_close", () => OnClickClose(info));

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

    public int getShowRank(int rank)
    {
        return rank + 1;
    }

    private void PerformWin()
    {
        SetVisible("win_result", true);
        SetVisible("fail_result", false);

        SetText("player_name", info.mChallengeName);
        SetText("old_rank_number", getShowRank(info.mPreRank).ToString());
        SetText("new_rank_number", getShowRank(info.mPreRank).ToString());

        SetVisible("rank_label", info.mPreRank > info.mPostRank ? true : false);
        SetVisible("rank_tips", info.mPreRank > info.mPostRank ? false : true);
        if (Guide.isActive)
        {
            SetText("rank_tips", TableCommon.getStringFromStringList(STRING_INDEX.ARENA_RANK_TIPS_FOR_GUIDE));
        }
        else
        {
            SetText("rank_tips", TableCommon.getStringFromStringList(STRING_INDEX.ARENA_RANK_CHANGE_TIPS));
        }
    }

    private void PerformLose()
    {
        SetVisible("win_result", false);
        SetVisible("fail_result", true);

        mGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI,"Grid");
        if (mGridContainer == null)
            return;
        BattleFailManager.Self.InitFailGuideUI(mGridContainer, () => { Close(); });
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
        percentLabel.text = (int)(preRoleRate * 100) + "%";
        bar.value = preRoleRate;

        if (info.mIsWin)
        {
            UILabel rankLabel = GetComponent<UILabel>("new_rank_number");
            yield return DoCoroutine(UIKIt.PushNumberLabel(rankLabel, getShowRank(info.mPreRank), getShowRank(info.mPostRank)));
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

    private void OnClickClose(ArenaNetManager.ArenaAccountInfo info)
    {
        Close();

        if (info.mIsWin)
        {
            if (info.mHasBreak)
            {
                DataCenter.OpenWindow("ARENA_BREAK_WINDOW", info);
            }
            else
            {
                DataCenter.OpenWindow("ARENA_TREASURE_WINDOW", info);
            }
        }
        else 
        {
            PVP4Battle.GoBack();
        }
    }
}