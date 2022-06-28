using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

/// <summary>
/// 群魔乱舞每关选择难度界面
/// </summary>
public class RammbockSelectDifficultyWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_challenge_button", new DefineFactoryLog<Button_Rammbock_SelectDifficulty_challenge_button>());
        EventCenter.Self.RegisterEvent("Button_rammbock_difficulty_close_button", new DefineFactoryLog<Button_Rammbock_SelectDifficulty_close_button>());
        EventCenter.Self.RegisterEvent("Button_rammbock_difficulty_window", new DefineFactoryLog<Button_Rammbock_SelectDifficulty_close_button>());
   
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        int nextTier = (int)param;

        DataRecord nextClimbConfig = DataCenter.mClimbingTowerConfig.GetRecord(nextTier);

        string stageIndex = nextClimbConfig.getData("STAR_1");
        DataRecord stageConfig = DataCenter.mStageTable.GetRecord(stageIndex);

        //关卡名
        SetText("title_label", stageConfig.getData("NAME"));

        //通关条件
        string missionCondition = RammbockWindow.GetMissionCondition(nextClimbConfig, stageIndex);
        GameCommon.SetUIText(GetSub("condition_sprite"), "label03", missionCondition);

        //战斗力
        float tmpFightPower = GameCommon.GetPower();
        GameCommon.SetUIText(GetSub("fight_strength"), "fight_strength_number", tmpFightPower.ToString());

        //挑战信息
        for (int i = 0; i < 3; i++)
            __RefreshChallengeInfo(nextClimbConfig, nextTier, i);

        return true;
    }

    private void __RefreshChallengeInfo(DataRecord climbConfig, int nextTier, int difficultyIndex)
    {
        GameObject item = GameCommon.FindObject(GetSub("change_group"), "group(Clone)_" + difficultyIndex.ToString());

        //金币
        int tmpBaseMoney = int.Parse(GetSubString(climbConfig.getData("BASE_MONEY"), 0)) * (difficultyIndex + 1);
        GameCommon.SetUIText(item, "gold_num", tmpBaseMoney.ToString());

        //威名
        int tmpBaseToken = int.Parse(GetSubString(climbConfig.getData("BASE_EQUIP_TOKEN"), 0)) * (difficultyIndex + 1);
        GameCommon.SetUIText(item, "renown_num", tmpBaseToken.ToString());

        //推荐战力
        GameCommon.SetUIText(GameCommon.FindObject(item, "label_info"), "label05", climbConfig.getData("FIGHT_POINT_" + (difficultyIndex + 1).ToString()));

        NiceData btnData = GameCommon.GetButtonData(item, "challenge_button");
        if (btnData != null)
        {
            btnData.set("NEXT_TIER", nextTier);
            btnData.set("TIER_STARS", difficultyIndex + 1);
        }
    }
    public static string GetSubString(string str, int idx)
    {
        string[] subStrs = str.Split(new char[]{'|'});
        if (idx > subStrs.Length)
            return "0";
        return subStrs[idx];
    }
}

/// <summary>
/// 挑战按钮
/// </summary>
class Button_Rammbock_SelectDifficulty_challenge_button : CEvent
{
    public override bool _DoEvent()
    {
        int nextTier = (int)getObject("NEXT_TIER");
        int tierStars = (int)getObject("TIER_STARS");
        DataCenter.Set("CURRETN_DIFFICULTY",tierStars);
        RammbockNetManager.RequestClimbTowerStart(nextTier, tierStars);
		DataCenter.Set ("QUIT_BACK_SCENE",QUIT_BACK_SCENE_TYPE.RAMMBOCK);
        
        return true;
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
public class Button_Rammbock_SelectDifficulty_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RAMMBOCK_SELECT_DIFFICULTY_WINDOW");

        return true;
    }
}
