using UnityEngine;
using System.Collections;
using Logic;
using DataTable;

public class GrabTreasureUsePeaceWindow : tWindow
{
    private GrabTreasurePeaceData mCurrPeaceData;

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_grab_peace_buy_btn", new DefineFactoryLog<Button_grab_peace_buy_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_peace_use_btn", new DefineFactoryLog<Button_grab_peace_use_btn>());
        EventCenter.Self.RegisterEvent("Button_grab_buy_close_button", new DefineFactoryLog<Button_grab_buy_close_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
        __RefreshPeaceCount();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REFRESH_PEACE_COUNT":
                {
                    __RefreshPeaceCount();
                }break;
        }
    }

    public override bool Refresh(object param)
    {
        mCurrPeaceData = param as GrabTreasurePeaceData;

        //免战令牌图标
        GameCommon.SetItemIcon(GetSub("grab_peace_info"), new ItemData() { mID = mCurrPeaceData.tid, mType = (int)PackageManager.GetItemTypeByTableID(mCurrPeaceData.tid) });
        //当前免战令图标
        GameCommon.SetItemIcon(GetSub("grab_curr_peace_count"), new ItemData() { mID = mCurrPeaceData.tid, mType = (int)PackageManager.GetItemTypeByTableID(mCurrPeaceData.tid) });

        //免战令牌类型名称
        GameCommon.SetUIText(GetSub("grab_peace_info"), "peace_name_label", GrabTreasureSelectPeaceWindow.GetPeaceNameByType(mCurrPeaceData.PeaceType));

        //消耗元宝数
        int tmpPeaceSIndex = GrabTreasurePeaceID.Instace[mCurrPeaceData.tid];
        DataRecord tmpShopConfig = DataCenter.mMallShopConfig.GetRecord(tmpPeaceSIndex);
        int tmpCost = 10;
        if (tmpShopConfig == null)
            GrabTreasureWindow.LogError("找不到商店表" + tmpPeaceSIndex.ToString() + "数据");
        else
            tmpCost = int.Parse((string)tmpShopConfig.getObject("COST_NUM_1"));
        GameCommon.SetUIText(GetSub("grab_peace_buy_btn"), "num", tmpCost.ToString());

        long peaceTime = GrabTreasureSelectPeaceWindow.GetPeaceTimeByType(mCurrPeaceData.PeaceType);
        NiceData tmpBtnBuyData = GameCommon.GetButtonData(mGameObjUI, "grab_peace_buy_btn");
        if (tmpBtnBuyData != null)
        {
            tmpBtnBuyData.set("PEACE_COST", tmpCost);
            tmpBtnBuyData.set("PEACE_DATA", mCurrPeaceData);
            tmpBtnBuyData.set("PEACE_TIME", peaceTime);
        }
        NiceData tmpBtnUseData = GameCommon.GetButtonData(mGameObjUI, "grab_peace_use_btn");
        if (tmpBtnUseData != null)
        {
            tmpBtnUseData.set("PEACE_DATA", mCurrPeaceData);
            tmpBtnUseData.set("PEACE_TIME", peaceTime);
        }

        return true;
    }

    private void __RefreshPeaceCount()
    {
        if(mCurrPeaceData == null)
            return;

        //当前剩余免战牌
        ConsumeItemData tmpTruceData = ConsumeItemLogicData.Self.GetDataByTid(mCurrPeaceData.tid);
        int leftPeaceCount = 0;
        if (tmpTruceData != null)
            leftPeaceCount = tmpTruceData.itemNum;
        GameCommon.SetUIText(GetSub("cur_token"), "num", leftPeaceCount.ToString());
    }
}

/// <summary>
/// 购买免战牌
/// </summary>
class Button_grab_peace_buy_btn : CEvent
{
    public override bool _DoEvent()
    {
        int cost = (int)getObject("PEACE_COST");
        if (RoleLogicData.Self.diamond < cost)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NO_ENOUGH_DIAMOND);
            return true;
        }

        GrabTreasurePeaceData tmpPeace = getObject("PEACE_DATA") as GrabTreasurePeaceData;
        GrabTreasureNetManager.RequestBuyTruceToken(cost, tmpPeace.tid, GrabTreasurePeaceID.Instace[tmpPeace.tid], 1);

        return true;
    }
}

/// <summary>
/// 用免战牌
/// </summary>
class Button_grab_peace_use_btn : CEvent
{
    public override bool _DoEvent()
    {
        GrabTreasurePeaceData tmpPeaceData = getObject("PEACE_DATA") as GrabTreasurePeaceData;
        ConsumeItemData tmpPeaceItemData = ConsumeItemLogicData.Self.GetDataByTid(tmpPeaceData.tid);
        if (tmpPeaceItemData == null || tmpPeaceItemData.itemNum <= 0)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_TRUCE_NO_ENOUGH);
            return true;
        }
        if (GrabTreasureWindow.IsPeace)
        {
            string tmpStr = GrabTreasureWindow.LeftPeaceTime();
            OpenMessageOkWindow(STRING_INDEX.ERROR_GRAB_MULTI_PEASE, tmpStr, "", __OnUseAction);
            return true;
        }
        else
            __OnUseAction();

        return true;
    }

    private void __OnUseAction()
    {
        GrabTreasurePeaceData tmpPeaceData = getObject("PEACE_DATA") as GrabTreasurePeaceData;
        ConsumeItemData tmpPeaceItemData = ConsumeItemLogicData.Self.GetDataByTid(tmpPeaceData.tid);
        long tmpPeaceTime = (long)getObject("PEACE_TIME");
        ItemDataBase tmpItemData = new ItemDataBase();
        tmpItemData.tid = tmpPeaceData.tid;
        tmpItemData.itemNum = 1;
        //设置itemId
        if (tmpPeaceData != null)
            tmpItemData.itemId = tmpPeaceItemData.itemId;

        GlobalModule.DoCoroutine(__DoAction(tmpItemData, tmpPeaceTime));
    }
    private IEnumerator __DoAction(ItemDataBase itemData, long peaceTime)
    {
        GrabTreasure_UseTruceCard_Requester tmpRequester = new GrabTreasure_UseTruceCard_Requester();
        tmpRequester.TruceCard = itemData;
        yield return tmpRequester.Start();

        if (tmpRequester.respCode == 1)
        {
            //检查是否为第一次用免战牌
            if (!GrabTreasureWindow.IsPeace)
            {
                DataRecord tmpPeaceRecord = DataCenter.mStringList.GetRecord((int)STRING_INDEX.TIPS_GRAB_FIRST_USE_PEASE);
                DataCenter.OpenMessageWindow(string.Format(tmpPeaceRecord.getData("STRING_CN"), (int)(peaceTime / 3600)), true);
            }

            //用免战牌成功，刷新免战时间
            DataCenter.SetData("GRABTREASURE_WINDOW", "ADD_PEACE_TIME", peaceTime);
            //免战牌数量减一
            PackageManager.RemoveItem(itemData.tid, itemData.itemId, 1);
            //刷新免战牌数量显示
            DataCenter.SetData("GRABTREASURE_USE_PEACE_WINDOW", "REFRESH_PEACE_COUNT", null);
        }
    }

    private void OpenMessageOkWindow(STRING_INDEX stringIndex, string addInfo, string windowName, System.Action onClickOk)
    {
        int index = (int)stringIndex;
        string showText = "";
        DataRecord listInfo = DataCenter.mStringList.GetRecord(index);
        if (listInfo != null)
        {
            showText = listInfo.get("STRING_CN");
            showText = string.Format(showText, addInfo);
        }
        DataCenter.OpenWindow("MESSAGE_WINDOW", showText);
        DataCenter.SetData("MESSAGE_WINDOW", "WINDOW_SEND", windowName);
        ObserverCenter.Add("MESSAGE_OK", onClickOk);
    }
}

/// <summary>
/// 关闭按钮
/// </summary>
class Button_grab_buy_close_button : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("GRABTREASURE_USE_PEACE_WINDOW");

        return true;
    }
}
