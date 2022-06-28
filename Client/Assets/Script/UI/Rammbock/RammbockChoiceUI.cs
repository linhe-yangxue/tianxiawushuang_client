using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

//群魔乱舞关卡选择界面

enum RammbockModelState
{
    Win = 0,
    Fight,
    NotFight
}

class RammbockModelColor
{
    static public Color[] mColor = new Color[] {
        Color.white,
        Color.white,
        Color.gray
    };
}

/// <summary>
/// 群魔乱舞关卡选择界面，位置点挂在RammbockWindow
/// </summary>
public class RammbockChoiceWindow : tWindow
{
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_rammbock_challenge_choice_bg", new DefineFactoryLog<Button_challenge_btn>());
        EventCenter.Self.RegisterEvent("Button_challenge_btn", new DefineFactoryLog<Button_challenge_btn>());
        EventCenter.Self.RegisterEvent("Button_reward_box_btn", new DefineFactoryLog<Button_reward_box_btn>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        SC_Rammbock_GetTowerClimbingInfo retClimbingInfo = win.m_climbingInfo;

        //关卡信息
        int chapter = (retClimbingInfo.nextTier - 1) / 3;
        bool isPassAll = RammbockWindow.IsPassAllMission();

        //检查领取章节奖品标志位
        RammbockWindow rammbockWin = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        RammbockBattleResultData resultData = rammbockWin.getObject("RAMMBOCK_AWARD") as RammbockBattleResultData;
        if (resultData != null)
        {
            rammbockWin.set("RAMMBOCK_AWARD", null);
            DataCenter.OpenWindow("RAMMBOCK_CHAPTER_AWARD_WINDOW", null);
            //显示奖励窗口，只有当没通过所有关卡时，在这里将关卡数减1
            if (!isPassAll)
                chapter -= 1;
        }
        else
        {
            if (rammbockWin.m_climbingInfo.chooseBuff != null && rammbockWin.m_climbingInfo.chooseBuff.Length > 0)
            {
                //直接打开加属性界面
                DataCenter.OpenWindow("RAMMBOCK_ATTRI_ADD_WINDOW", rammbockWin.m_climbingInfo.chooseBuff);
            }
        }

        //如果所有关卡已通过，将当前章节数减1
        if(isPassAll)
            chapter -= 1;
        int firstTier = chapter * 3 + 1;
        for (int i = 0; i < 3; i++)
            __RefreshMissionInfo(i, firstTier + i, retClimbingInfo.nextTier);

        NiceData awardButtonData = GameCommon.GetButtonData(mGameObjUI, "reward_box_btn");
        if (awardButtonData != null)
            awardButtonData.set("CURRENT_CHAPTER", chapter);

        return true;
    }

    private void __RefreshMissionInfo(int mission, int currTier, int nextTier)
    {
        GameObject item = GetSub("choice_boss(Clone)_" + mission.ToString());

        DataRecord climbingConfig = DataCenter.mClimbingTowerConfig.GetRecord(currTier);

        int stageIndex = 0;
        climbingConfig.get("STAR_1", out stageIndex);

        //BOSS模型
        DataRecord stageConfig = DataCenter.mStageTable.GetRecord(stageIndex);
        RammbockModelState tmpState = __GetModelState(currTier, nextTier);
        string bossID = stageConfig.getData("HEADICON");
        ActiveBirthForUI birthUI = GameCommon.FindComponent<ActiveBirthForUI>(item, "UIPoint");
        if(birthUI != null)
            __ShowModel(birthUI, System.Convert.ToInt32(bossID), __GetModelState(currTier, nextTier), currTier >= nextTier);

        //击杀图片
        GameCommon.SetUIVisiable(item, "kill_sprite", currTier < nextTier);

        //挑战按钮
        GameCommon.SetUIVisiable(item, "challenge_btn", currTier == nextTier);
        UIButton btnChallenge = GameCommon.FindComponent<UIButton>(item, "challenge_btn");
        NiceData btnData = GameCommon.GetButtonData(item, "challenge_btn");
        if (btnData != null)
            btnData.set("CURRENT_TIER", currTier);

        //挑战背景按钮事件
        BoxCollider boxBtnBG = GameCommon.FindComponent<BoxCollider>(item, "rammbock_challenge_choice_bg");
        if (boxBtnBG != null)
            boxBtnBG.enabled = (currTier == nextTier);
        NiceData btnBGData = GameCommon.GetButtonData(item, "rammbock_challenge_choice_bg");
        if (btnBGData != null)
            btnBGData.set("CURRENT_TIER", currTier);

        //关数
        string strColor = __GetMissionStateColor(tmpState);
        GameCommon.SetUIText(GameCommon.FindObject(item, "label_info"), "number_one", strColor + "第" + currTier.ToString() + "关");
    }

    private RammbockModelState __GetModelState(int currTier, int nextTier)
    {
        if (currTier < nextTier)
            return RammbockModelState.Win;
        else if (currTier == nextTier)
            return RammbockModelState.Fight;
        return RammbockModelState.NotFight;
    }
    /// <summary>
    /// 获取关卡名称颜色
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private string __GetMissionStateColor(RammbockModelState state)
    {
        string strColor = "";
        if (state == RammbockModelState.Win || state == RammbockModelState.Fight)
            strColor = "[FFFF00]";
        else
            strColor = "[808080]";
        return strColor;
    }
    private void __ShowModel(ActiveBirthForUI birthUI, int bossID, RammbockModelState state, bool visible)
    {
        if (birthUI == null)
            return;

        GameObject obj = GetSub("boss_model");
        if (!visible)
        {
            if (obj != null)
                obj.SetActive(false);
            return;
        }
        if (obj != null)
            obj.SetActive(true);

        if (birthUI.mActiveObject != null)
            birthUI.mActiveObject.OnDestroy();

        birthUI.mBirthConfigIndex = bossID;
//            birthUI.mObjectType = (int)OBJECT_TYPE.MONSTER_BOSS;
        birthUI.Init();
        if (birthUI.mActiveObject != null)
        {
            birthUI.mActiveObject.SetScale(80f);
            if (state == RammbockModelState.Win)
                birthUI.mActiveObject.PlayAnim("stun");
            else
                birthUI.mActiveObject.PlayAnim("idle");
            __ChangeModelColor(birthUI.mActiveObject.mBodyObject, RammbockModelColor.mColor[(int)state]);

            BossBirthOnApearUI modelScript = birthUI.mActiveObject.mMainObject.AddComponent<BossBirthOnApearUI>();
            if (modelScript != null)
                modelScript.mActiveObject = birthUI.mActiveObject;
        }
    }
    private void __ChangeModelColor(GameObject go, Color color)
    {
        if (go == null)
            return;

        foreach (Transform trans in go.transform)
        {
            Renderer ren = trans.gameObject.GetComponent<Renderer>();
            if (ren != null)
                ren.material.color = color;
        }
    }
}

/// <summary>
/// 挑战
/// </summary>
public class Button_challenge_btn : CEvent
{
    public override bool _DoEvent()
    {
        int currTier = 0;
        get("CURRENT_TIER", out currTier);

        DataCenter.OpenWindow("RAMMBOCK_SELECT_DIFFICULTY_WINDOW", currTier);

        return true;
    }
}

/// <summary>
/// 奖励箱
/// </summary>
public class Button_reward_box_btn : CEvent
{
    public override bool _DoEvent()
    {
        int chapter = (int)getObject("CURRENT_CHAPTER");
        DataCenter.OpenWindow("RAMMBOCK_AWARD_WINDOW", chapter);

        return true;
    }
}
