using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

public class GrabTreasureBattleResultWindow : tWindow
{
    GrabTreasureResultData mResultData;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_grab_result_close_button", new DefineFactoryLog<Button_grab_result_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        mResultData = param as GrabTreasureResultData;

        NiceData tmpBtnCloseData = GameCommon.GetButtonData(mGameObjUI, "grab_result_close_button");
        if (tmpBtnCloseData != null)
        {
            tmpBtnCloseData.set("IS_WIN", _IsWin);
            tmpBtnCloseData.set("WINDOW_NAME", _WindowName);
            tmpBtnCloseData.set("SELECT_ITEM", mResultData.mResult.awardItem);
        }

        Refresh(param);

		//隐藏泡泡文字闪避
		HidePaoPaoText ();
    }

	private void HidePaoPaoText()
	{
		GameObject _textPanel = GameObject.Find ("text_panel");
        if (_textPanel != null)
            _textPanel.SetActive(false);
	}

    public override bool Refresh(object param)
    {
        //播放主角动画
        __ShowModel();

        _RefreshUI(param);

        DoCoroutine(__DoPerformAward());

        return true;
    }

    private IEnumerator __DoPerformAward()
    {
        __CloseButtonEnable(false);

        float preRoleRate = (float)mResultData.mPreRoleExp / TableCommon.GetRoleMaxExp(mResultData.mPreRoleLevel);
        float postRoleRate = (float)mResultData.mPostRoleExp / TableCommon.GetRoleMaxExp(mResultData.mPostRoleLevel);

        UILabel expLabel = GameCommon.FindComponent<UILabel>(GetSub("get_exp_num"), "num");
        UILabel goldLabel = GameCommon.FindComponent<UILabel>(GetSub("get_coin_num"), "num");
        UILabel levelLabel = GameCommon.FindComponent<UILabel>(GetSub("Level"), "level_label");
        UILabel percentLabel = GameCommon.FindComponent<UILabel>(GetSub("Level"), "Label");
        UIProgressBar bar = GetComponent<UIProgressBar>("Progress Bar");

        expLabel.text = "0";
        goldLabel.text = "0";
        levelLabel.text = "Lv" + mResultData.mPreRoleLevel;
        percentLabel.text = (int)(preRoleRate * 100.0f) + "%";
        bar.value = preRoleRate;

        yield return DoCoroutine(UIKIt.PushNumberLabel(expLabel, 0, mResultData.mGetExp));
        yield return new WaitForSeconds(0.1f);
        yield return DoCoroutine(UIKIt.PushNumberLabel(goldLabel, 0, mResultData.mGetGold));
        yield return new WaitForSeconds(0.1f);
        yield return DoCoroutine(UIKIt.PushLevelBar(this, bar, percentLabel, mResultData.mPreRoleLevel, preRoleRate, mResultData.mPostRoleLevel, postRoleRate, x => levelLabel.text = "Lv" + x));

        __CloseButtonEnable(true);
    }

    private void __CloseButtonEnable(bool visible)
    {
        UIImageButton tmpBtn = GameCommon.FindComponent<UIImageButton>(mGameObjUI, "grab_result_close_button");
        tmpBtn.isEnabled = visible;
    }

    protected virtual void _RefreshUI(object param)
    {
    }

    protected virtual string _WindowName
    {
        get
        {
            return "";
        }
    }
    protected virtual int _IsWin
    {
        get
        {
            return 0;
        }
    }
    protected virtual string _AnimName
    {
        get
        {
            return "";
        }
    }

    private void __ShowModel()
    {
        GameObject uiPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");
        BaseObject model = GameCommon.ShowCharactorModel(uiPoint, 1.6f);

        string animName = _AnimName;
        if (animName != "")
            model.PlayAnim(animName, false);
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_grab_result_close_button : CEvent
{
    public override bool _DoEvent()
    {
        string winName = get("WINDOW_NAME");
        int isWin = (int)getObject("IS_WIN");
        ItemDataBase tmpSelItem = getObject("SELECT_ITEM") as ItemDataBase;

        DataCenter.Set("NO_PAOPAO_MAINUI", false);//去气泡
        if (isWin == 1)
        {
            //关闭结算窗口
            DataCenter.CloseWindow("GRABTREASURE_BATTLE_RESULT_WIN_WINDOW");

            //打开选择奖励物品窗口
            DataCenter.OpenWindow("GRABTREASURE_BATTLE_WIN_AWARD_WINDOW", tmpSelItem);
        }
        else
        {
            DataCenter.CloseWindow(winName);
            MainProcess.ClearBattle();
            MainUIScript.mLoadingFinishAction = () => DataCenter.OpenWindow("GRABTREASURE_WINDOW");
            MainProcess.LoadRoleSelScene();
        }

        return true;
    }
}

public class GrabTreasureBattleResultWinWindow : GrabTreasureBattleResultWindow
{
    protected override string _WindowName
    {
        get
        {
            return "GRABTREASURE_BATTLE_RESULT_WIN_WINDOW";
        }
    }
    protected override int _IsWin
    {
        get
        {
            return 1;
        }
    }
    protected override string _AnimName
    {
        get
        {
            return "cute";
        }
    }

    protected override void _RefreshUI(object param)
    {
        GameCommon.SetUIVisiable(mGameObjUI, "win_result", true);
        GameCommon.SetUIVisiable(mGameObjUI, "fail_result", false);

        GrabTreasureResultData tmpResultData = param as GrabTreasureResultData;

        GameCommon.SetUIVisiable(mGameObjUI, "grab_result_win_sorry_label", tmpResultData.mResult.succeed == 0);
        GameCommon.SetUIVisiable(mGameObjUI, "grab_result_win_tips_label", tmpResultData.mResult.succeed == 1);

        if (tmpResultData.mResult.succeed == 1)
        {
            //碎片名
            GameCommon.SetUIText(mGameObjUI, "grab_result_win_treasure_name", GameCommon.GetItemName(tmpResultData.mFragId));

            //碎片Icon
            GameCommon.SetOnlyItemIcon(GameCommon.FindObject(mGameObjUI, "grab_result_win_tips_label"), "item_icon", tmpResultData.mFragId);
        }
    }
}

public class GrabTreasureBattleResultLoseWindow : GrabTreasureBattleResultWindow
{
    private UIGridContainer mGridContainer;
    protected override string _WindowName
    {
        get
        {
            return "GRABTREASURE_BATTLE_RESULT_LOSE_WINDOW";
        }
    }
    protected override int _IsWin
    {
        get
        {
            return 0;
        }
    }
    protected override string _AnimName
    {
        get
        {
            return "lose";
        }
    }

    protected override void _RefreshUI(object param)
    {
        GameCommon.SetUIVisiable(mGameObjUI, "win_result", false);
        GameCommon.SetUIVisiable(mGameObjUI, "fail_result", true);

        mGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI,"Grid");
        BattleFailManager.Self.InitFailGuideUI(mGridContainer, () => { Close(); });
    }
}
