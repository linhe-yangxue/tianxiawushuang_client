using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

/// <summary>
/// 每关战斗结束界面
/// </summary>


/// <summary>
/// 群魔乱舞每关战斗结束所需数据
/// </summary>
public class RammbockBattleResultData
{
    public int currTier = 0;            //当前关卡索引
    public int selStarsNum = 0;         //进关卡时选择的星数
    public SC_Rammbock_ClimbTowerResult retClimb = null;        //爬塔结束协议返回值
}

/// <summary>
/// 群魔乱舞每关战斗结束界面基类
/// </summary>
public class RammbockBattleResultWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_rammbock_battle_result_close", new DefineFactoryLog<Button_Rammbock_Battle_close_button>());

        _OnInitEvent();
    }

    protected virtual void _OnInitEvent()
    {
    }

    public override void Open(object param)
    {
        base.Open(param);

        NiceData btnData = GameCommon.GetButtonData(mGameObjUI, "rammbock_battle_result_close");
        if (btnData != null)
        {
            btnData.set("WINDOW_KEY", _WindowKey);
            btnData.set("IS_WIN", _IsWin);
            btnData.set("IS_CHAPTER_WIN", 0);
        }

        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        RammbockBattleResultData retData = param as RammbockBattleResultData;

        __RefreshUI(retData);

        __ShowModel();

        return true;
    }

    protected virtual string _WindowKey
    {
        get
        {
            return "";
        }
    }
    protected virtual string _AnimName
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
            return -1;
        }
    }

    protected virtual void __RefreshUI(RammbockBattleResultData data)
    {
    }

    private void __ShowModel()
    {
        GameObject uiPoint = GameCommon.FindObject(mGameObjUI, "UIPoint");
        BaseObject model = GameCommon.ShowCharactorModel(uiPoint, 1.6f);

        string animName = _AnimName;
        if(animName != "")
            model.PlayAnim(animName);
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_Rammbock_Battle_close_button : CEvent
{
    public override bool _DoEvent()
    {
        //added by xuke
        DataCenter.Set("QUIT_BACK_SCENE", QUIT_BACK_SCENE_TYPE.WORLD_MAP);
        //end
        string windowKey = get("WINDOW_KEY");
        DataCenter.CloseWindow(windowKey);

        int isWin = (int)getObject("IS_WIN");
        int isChapterWin = (int)getObject("IS_CHAPTER_WIN");

        MainProcess.ClearBattle();
        MainProcess.LoadRoleSelScene();

        //DataCenter.OpenWindow("RAMMBOCK_WINDOW");
        GetPathHandlerDic.HandlerDic[GET_PARTH_TYPE.FENG_LING_TA]();
        return true;
    }
}

/// <summary>
/// 赢界面
/// </summary>
public class RammbockBattleResultWinWindow : RammbockBattleResultWindow
{
    protected override void __RefreshUI(RammbockBattleResultData data)
    {
        base.__RefreshUI(data);

        SC_Rammbock_ClimbTowerResult retClimbTower = data.retClimb;

        GameCommon.SetUIVisiable(mGameObjUI, "fail_result", false);
        GameCommon.SetUIVisiable(mGameObjUI, "label_info", false);
        GameCommon.SetUIVisiable(mGameObjUI, "rank_label", false);
        GameCommon.SetUIVisiable(mGameObjUI, "Level", false);

        if ((object)DataCenter.Self.get("NOW_RAMMBOCK_ID") != null && (object)DataCenter.Self.get("NOW_RAMMBOCK_STAR") != null)
        {
            DataRecord r = DataCenter.mStageStar.GetRecord(int.Parse(DataCenter.mStageTable.GetData(int.Parse(DataCenter.mClimbingTowerConfig.GetData(int.Parse(DataCenter.Self.get("NOW_RAMMBOCK_ID")), "STAR_" + int.Parse(DataCenter.Self.get("NOW_RAMMBOCK_STAR")).ToString())), "ADDSTAR_0")));
            string win_condition = r.getData("STARNAME_WIN").ToString().Replace("{0}", r.getData("STARVAR").ToString());
            GameCommon.SetUIText(mGameObjUI, "win_condition_label", win_condition);
        }
        DataCenter.Self.set("NOW_RAMMBOCK_ID", null);
        DataCenter.Self.set("NOW_RAMMBOCK_STAR", null);


        RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;

        DataRecord climbConfig = DataCenter.mClimbingTowerConfig.GetRecord(win.m_climbingInfo.nextTier);

        int _difficulty = DataCenter.Get("CURRETN_DIFFICULTY");
        //钱
        GameCommon.SetUIVisiable(GetSub("reward_info(Clone)_0"), "title_prestige", false);
        int moneyValue = int.Parse(RammbockSelectDifficultyWindow.GetSubString(climbConfig.getData("BASE_MONEY"), retClimbTower.awardIndex - 1));
        moneyValue *= data.selStarsNum;
        int _moneyValue = int.Parse(RammbockSelectDifficultyWindow.GetSubString(climbConfig.getData("BASE_MONEY"), 0)) * _difficulty;
        if (moneyValue > _moneyValue)
        {
            GameCommon.FindObject(mGameObjUI, "doubleAward").SetActive(true);
        }
        else
        {
            GameCommon.FindObject(mGameObjUI, "doubleAward").SetActive(false);
        }
        GameCommon.SetUIText(GetSub("award_gold"), "gold_num", moneyValue.ToString());

        //威名
        GameCommon.SetUIVisiable(GetSub("reward_info(Clone)_1"), "coin_icon", false);
        int prestige = int.Parse(RammbockSelectDifficultyWindow.GetSubString(climbConfig.getData("BASE_EQUIP_TOKEN"), retClimbTower.awardIndex - 1));
        prestige *= data.selStarsNum;
        GameCommon.SetUIText(GetSub("award_prestige"), "prestige_num", prestige.ToString());

        //设置奖励标志
        if (data.currTier % 3 == 0)
        {
            DataCenter.SetData("RAMMBOCK_WINDOW", "RAMMBOCK_AWARD", data);

            NiceData btnData = GameCommon.GetButtonData(mGameObjUI, "rammbock_battle_result_close");
            if (btnData != null)
                btnData.set("IS_CHAPTER_WIN", 1);
        }
    }

    protected override string _WindowKey
    {
        get
        {
            return "RAMMBOCK_BATTLE_RESULT_WIN_WINDOW";
        }
    }
    protected override string _AnimName
    {
        get
        {
            return "cute";
        }
    }
    protected override int _IsWin
    {
        get
        {
            return 1;
        }
    }
}

/// <summary>
/// 输界面
/// </summary>
public class RammbockBattleResultLoseWindow : RammbockBattleResultWindow
{
    private UIGridContainer mGridContainer;
    protected override void _OnInitEvent()
    {
        base._OnInitEvent();
    }

    protected override void __RefreshUI(RammbockBattleResultData data)
    {
        base.__RefreshUI(data);

        GameCommon.SetUIVisiable(mGameObjUI, "win_result", false);
        GameCommon.SetUIVisiable(mGameObjUI, "reward_info_group", false);
        GameCommon.SetUIVisiable(mGameObjUI, "Level", false);

        if ((object)DataCenter.Self.get("NOW_RAMMBOCK_ID") != null && (object)DataCenter.Self.get("NOW_RAMMBOCK_STAR") != null)
        {
            DataRecord r = DataCenter.mStageStar.GetRecord(int.Parse(DataCenter.mStageTable.GetData(int.Parse(DataCenter.mClimbingTowerConfig.GetData(int.Parse(DataCenter.Self.get("NOW_RAMMBOCK_ID")), "STAR_" + int.Parse(DataCenter.Self.get("NOW_RAMMBOCK_STAR")).ToString())), "ADDSTAR_0")));
            string win_condition = r.getData("LOST_STARNAME").ToString().Replace("{0}", r.getData("STARVAR").ToString());
            GameCommon.SetUIText(mGameObjUI, "win_condition_label", win_condition);
        }
        DataCenter.Self.set("NOW_RAMMBOCK_ID", null);
        DataCenter.Self.set("NOW_RAMMBOCK_STAR", null);

        mGridContainer = GameCommon.FindComponent<UIGridContainer>(mGameObjUI,"Grid");
        BattleFailManager.Self.InitFailGuideUI(mGridContainer, () => { Close(); });
    }

    protected override string _WindowKey
    {
        get
        {
            return "RAMMBOCK_BATTLE_RESULT_LOSE_WINDOW";
        }
    }
    protected override string _AnimName
    {
        get
        {
            return "lose";
        }
    }
    protected override int _IsWin
    {
        get
        {
            return 0;
        }
    }
}
