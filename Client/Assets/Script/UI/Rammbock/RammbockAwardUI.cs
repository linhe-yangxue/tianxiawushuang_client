using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;

/// <summary>
/// 奖励预览
/// </summary>

/// <summary>
/// 群魔乱舞每章节通关后奖励预览界面
/// </summary>
public class RammbockAwardWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_reward_close_button", new DefineFactoryLog<Button_Rambock_Award_reward_close_button>());
        EventCenter.Self.RegisterEvent("Button_rammbock_reward_window", new DefineFactoryLog<Button_Rambock_Award_reward_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        int chapter = (int)param;

        //当前星数
        int currStarsNumber = RammbockWindow.GetChapterTotalStarsNumber(chapter);
        SetText("star_number", currStarsNumber.ToString());

        //关卡数提示
        int firstTier = chapter * 3 + 1;
        SetText("relate_label", "第" + firstTier.ToString() + "到" + (firstTier + 2).ToString() + "关获得星数达到以下数量时，可以获得");

        //物品
        DataRecord climbingConfig = DataCenter.mClimbingTowerConfig.GetRecord(firstTier + 2);
        for (int i = 0; i < 3; i++)
        {
            int starNum = (i + 1) * 3;
            int groupID = (int)climbingConfig.getObject("STAR_REWARD_" + starNum.ToString());
            __RefreshGroupItems(i, groupID);
        }

        return true;
    }

    private void __RefreshGroupItems(int index, int groupID)
    {
        GameObject itemGroup = GetSub("reward_group(Clone)_" + index.ToString());

        List<ItemData> items = RammbockWindow.GetGroupItems(groupID);
        int count = items.Count;
        for (int i = 0; i < 3; i++)
        {
            GameObject itemIcon = GameCommon.FindObject(itemGroup, "item_icon(Clone)_" + i.ToString());
            if (itemIcon != null)
                itemIcon.gameObject.SetActive(i < count);
            if (i < count)
            {
                GameCommon.SetOnlyItemIcon(itemIcon, items[i].mID);
                GameCommon.SetUIText(itemIcon, "relate_number_label", "x" + items[i].mNumber.ToString());
				int iTid = items[i].mID;
				AddButtonAction (itemIcon, () => GameCommon.SetItemDetailsWindow (iTid));
            }
        }
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_Rambock_Award_reward_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RAMMBOCK_AWARD_WINDOW");

        return true;
    }
}
