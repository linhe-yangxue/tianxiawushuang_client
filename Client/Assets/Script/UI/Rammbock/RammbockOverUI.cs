using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

//群魔乱舞结束界面

/// <summary>
/// 群魔乱舞战斗失败后界面，位置点挂在RammbockWindow
/// </summary>
public class RammbockOverWindow : tWindow
{
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_rammbock_open_treasure_button", new DefineFactoryLog<Button_rammbock_open_treasure_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REFRESH_TREASURE":
                {
                    __RefreshTreasure(null);
                }break;
        }
    }

    public override bool Refresh(object param)
    {
        RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        SC_Rammbock_GetTowerClimbingInfo retClimbingInfo = win.m_climbingInfo;

        //已击败信息
        int lastMission = retClimbingInfo.nextTier - 1;
        GameCommon.SetUIText(GetSub("tips_sprite"), "Label", "大师好厉害，你已击败" + lastMission.ToString() + "关，重置后可以继续");

        __RefreshTreasure(retClimbingInfo);

        //检查打折商品标志位
        RammbockWindow rammbockWin = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        RammbockTreasureData treasureData = rammbockWin.getObject("RAMMBOCK_TREASURE") as RammbockTreasureData;
        if (treasureData != null)
        {
            rammbockWin.set("RAMMBOCK_TREASURE", null);
            DataCenter.OpenWindow("RAMMBOCK_TREASURE_WINDOW", treasureData);
        }

        return true;
    }

    private void __RefreshTreasure(SC_Rammbock_GetTowerClimbingInfo retClimbingInfo)
    {
        if (retClimbingInfo == null)
        {
            RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
            retClimbingInfo = win.m_climbingInfo;
        }

        //宝藏
        GameCommon.SetUIVisiable(mGameObjUI, "boss_reward_bg", retClimbingInfo.saleItemState == 0);
        if (retClimbingInfo.saleItemState == 0)
        {
            ItemData itemData = new ItemData() { mID = retClimbingInfo.itemOnSale.tid, mType = (int)PackageManager.GetItemTypeByTableID(retClimbingInfo.itemOnSale.tid), mNumber = retClimbingInfo.itemOnSale.itemNum };
            GameCommon.SetItemIcon(GetSub("rammbock_open_treasure_button"), itemData);
        }
    }
}

/// <summary>
/// 打开打折商品界面
/// </summary>
class Button_rammbock_open_treasure_button : CEvent
{
    public override bool _DoEvent()
    {
        RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        RammbockTreasureData treasureData = new RammbockTreasureData();

        treasureData.currTier = win.m_climbingInfo.nextTier;
        treasureData.totalStar = win.m_climbingInfo.currentStars;
        treasureData.itemData = new ItemDataBase() { tid = win.m_climbingInfo.itemOnSale.tid, itemNum = win.m_climbingInfo.itemOnSale.itemNum };
        treasureData.saleItemPrice = win.m_climbingInfo.saleItemPrice;
        treasureData.originalItemPrice = win.m_climbingInfo.originalItemPrice;
        DataCenter.OpenWindow("RAMMBOCK_TREASURE_WINDOW", treasureData);

        return true;
    }
}
