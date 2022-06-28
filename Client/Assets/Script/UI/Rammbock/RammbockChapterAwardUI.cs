using UnityEngine;
using System.Collections;
using DataTable;
using Logic;
using System.Collections.Generic;

/// <summary>
/// 群魔乱舞每章节通关后获得对应星级奖励展示界面
/// </summary>
public class RammbockChapterAwardWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_get_button", new DefineFactoryLog<Button_Rammbock_ChapterAward_get_button>());
        EventCenter.Self.RegisterEvent("Button_close_button", new DefineFactoryLog<Button_Rammbock_ChapterAward_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);
        RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        int currTier = win.m_climbingInfo.nextTier - 1;     //需要查上一关
        int chapter = (currTier - 1) / 3;
        int firstTier = chapter * 3 + 1;

        //物品
        DataRecord climbingConfig = DataCenter.mClimbingTowerConfig.GetRecord(firstTier + 2);
        int chapterStars = RammbockWindow.GetChapterTotalStarsNumber(chapter);
        //int awardStarNum = (chapterStars - 1) / 3 * 3 + 3;
		int awardStarNum = win.m_climbingInfo.previousTierStarNum/3 * 3;
        int groupID = (int)climbingConfig.getObject("STAR_REWARD_" + awardStarNum.ToString());

        UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "reward_info_grid");
        if (grid == null)
            return;

        List<ItemData> items = RammbockWindow.GetGroupItems(groupID);
        int count = items.Count;
        grid.MaxCount = count;
        for (int i = 0; i < count; i++)
        {
            GameObject itemIcon = grid.controlList[i];
            //名称
            //TODO
            string awardName = "";
            GameCommon.SetUIText(itemIcon, "name_label", awardName);
            //Icon
            GameCommon.SetItemIcon(itemIcon, items[i]);
        }
    }
}

/// <summary>
/// 领奖
/// </summary>
class Button_Rammbock_ChapterAward_get_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RAMMBOCK_CHAPTER_AWARD_WINDOW");
        RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        if (win.m_climbingInfo.chooseBuff != null && win.m_climbingInfo.chooseBuff.Length > 0)
            DataCenter.OpenWindow("RAMMBOCK_ATTRI_ADD_WINDOW", win.m_climbingInfo.chooseBuff);

        return true;
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_Rammbock_ChapterAward_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RAMMBOCK_CHAPTER_AWARD_WINDOW");

        RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        if (win.m_climbingInfo.chooseBuff != null && win.m_climbingInfo.chooseBuff.Length > 0)
            DataCenter.OpenWindow("RAMMBOCK_ATTRI_ADD_WINDOW", win.m_climbingInfo.chooseBuff);

        return true;
    }
}
