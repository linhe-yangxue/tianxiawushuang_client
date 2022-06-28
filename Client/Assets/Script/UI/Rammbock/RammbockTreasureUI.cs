using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

/// <summary>
/// 群魔宝藏
/// </summary>


/// <summary>
/// 群魔乱舞失败后购买打折商品界面
/// </summary>
public class RammbockTreasureData
{
    public ItemDataBase itemData;       //打折商品数据
    public int currTier;                //当前所在关卡
    public int totalStar;               //本次获得总星数
    public int saleItemPrice;  //打折商品价格
    public int originalItemPrice;  //打折商品原价
}

/// <summary>
/// 群魔乱舞失败后购买打折商品界面
/// </summary>
public class RammbockTreasureWindow : tWindow
{
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_rammbock_buy_button", new DefineFactoryLog<Button_Rammbock_Treasure_buy_button>());
        EventCenter.Self.RegisterEvent("Button_rammbock_shut_button", new DefineFactoryLog<Button_Rammbock_Treasure_close_button>());
        EventCenter.Self.RegisterEvent("Button_rammbock_close_button", new DefineFactoryLog<Button_Rammbock_Treasure_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override bool Refresh(object param)
    {
        RammbockTreasureData treasureData = param as RammbockTreasureData;

        int tmpTid = treasureData.itemData.tid;

        //总星数
        SetText("total_star_label", treasureData.totalStar.ToString());

        //物品图标
        GameCommon.SetOnlyItemIcon(mGameObjUI, "item_icon", tmpTid);
        GameCommon.SetOnlyItemCount(mGameObjUI, "count_label", treasureData.itemData.itemNum);

        //物品名
        //TODO
        string treasureName = "";
        SetText("treasure_label", treasureName);

        //价格
        DataRecord climbConfig = DataCenter.mClimbingTowerConfig.GetRecord(treasureData.currTier);
        int price = treasureData.saleItemPrice;
        int basePrice = treasureData.originalItemPrice;
        //int price = (int)climbConfig.getObject("PRICE");
        //int basePrice = (int)climbConfig.getObject("BASE_PRICE");
        GameCommon.SetUIText(GetSub("base_price_label"), "number_label", basePrice.ToString());
        GameCommon.SetUIText(GetSub("price_label"), "number_label", price.ToString());

        //设置打折角标
        if (price < basePrice)
        {
            int discountNum = price * 10 / basePrice;
            string ret = "a_ui_smsddazhe_0" + discountNum;
            GameCommon.SetUISprite(mGameObjUI, "discount_sprite", ret);
        }
        GameCommon.SetUIVisiable(mGameObjUI, "discount_sprite", price < basePrice);

        NiceData btnData = GameCommon.GetButtonData(mGameObjUI, "rammbock_buy_button");
        if (btnData != null)
        {
            btnData.set("TREASURE_DATA", treasureData);
            btnData.set("TREASURE_PRICE", price);
        }

        return true;
    }
}

/// <summary>
/// 购买按钮
/// </summary>
class Button_Rammbock_Treasure_buy_button : CEvent
{
    public override bool _DoEvent()
    {
        //检查元宝是否足够
        RammbockTreasureData treasureData = getObject("TREASURE_DATA") as RammbockTreasureData;
        int price = (int)getObject("TREASURE_PRICE");
        if (price > RoleLogicData.Self.diamond)
        {
            //元宝不足
            //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NO_ENOUGH_DIAMOND);
			GameCommon.ToGetDiamond();
            return true;
        }

        DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_RAMMBOCK_CONFIRM_BUY_AWARD, "", () => { __OnBuyOk(price, treasureData.itemData); }, __OnBuyCancel);

        return true;
    }

    private void __OnBuyOk(int price, ItemDataBase itemData)
    {
        RammbockNetManager.RequestRammbockClimbTowerBuyCommodity(price, itemData);
    }
    private void __OnBuyCancel()
    {
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_Rammbock_Treasure_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("RAMMBOCK_TREASURE_WINDOW");

        return true;
    }
}
