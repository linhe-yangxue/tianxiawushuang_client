using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using DataTable;

//夺宝协议网络请求

public class GrabTreasureNetManager
{
    /// <summary>
    /// 夺宝玩家列表
    /// </summary>
    /// <param name="tid"></param>
    public static void RequestIndianaPlayerList(int tid, Action callbackSuccess)
    {
        CS_GrabTreasure_IndianaPlayerList playerList = new CS_GrabTreasure_IndianaPlayerList();
        playerList.tid = tid;
        HttpModule.Instace.SendGameServerMessage(playerList, (x) => { __OnRequestIndianaPlayerListSuccess(callbackSuccess, x); }, __OnRequestIndianaPlayerListFailed);
    }
    private static void __OnRequestIndianaPlayerListSuccess(Action callbackSuccess, string text)
    {
        SC_GrabTreasure_IndianPlayerList retPlayerList = JCode.Decode<SC_GrabTreasure_IndianPlayerList>(text);
        if (callbackSuccess != null)
            callbackSuccess();
        if (retPlayerList != null)
            DataCenter.SetData("GRABTREASURE_FIGHT_WINDOW", "REFRESH_FIGHT_LIST", retPlayerList);
    }
    private static void __OnRequestIndianaPlayerListFailed(string text)
    {
        switch (text)
        {
            case "2001":
                {
                    //等级不足
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_ROBOT_EXCEPTION);
                }break;
        }
    }

    /// <summary>
    /// 点击夺宝按钮
    /// </summary>
    /// <param name="tid"></param>
    public static void RequestPointIndianaButton(int tid, int itemId, int grabType, int level)
    {
        CS_GrabTreasure_PointIndianaButton indianaBtn = new CS_GrabTreasure_PointIndianaButton();
        indianaBtn.tid = tid;
        indianaBtn.itemTid = itemId;
        indianaBtn.type = grabType;
        indianaBtn.robotLevel = level;
        HttpModule.Instace.SendGameServerMessage(indianaBtn, (x) => { __OnRequestPointIndianaButtonSuccess(itemId, grabType, tid, level, x); }, __OnRequestPointIndianaButtonFailed);
    }
    private static void __OnRequestPointIndianaButtonSuccess(int fragId, int grabType, int grabUid, int level, string text)
    {
        SC_GrabTreasure_PointIndianaButton retIndianaBtn = JCode.Decode<SC_GrabTreasure_PointIndianaButton>(text);

        if (retIndianaBtn.flag != 1)
        {
            //不满足夺宝条件
            return;
        }

        //减精力 （精力改在结算时扣除）
        //int tmpCostSpirit = 2;
        //RoleLogicData.Self.AddSpirit(-tmpCostSpirit);
        //去除免战
        if (grabType == 1)
            DataCenter.SetData("GRABTREASURE_WINDOW", "REFRESH_PEACE_TIME", (long)0);

        GrabTreasureWindow win = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        win.set("CURENT_GRAB_FRAG_ID", fragId);
        win.set("CURRENT_GRAB_TYPE", grabType);
        win.set("CURRENT_GRAB_UID", grabUid);
        win.set("CURRENT_CHALLENGE_PLAYER", retIndianaBtn.challengePlayer);
        win.set("CURRENT_TARGET_LEVEL", level);

        //进入战斗
        GlobalModule.ClearAllWindow();
        MainProcess.ClearBattle();
        int grabPVPBattleIndex = 20004;
        DataCenter.Set("CURRENT_STAGE", grabPVPBattleIndex);
        MainProcess.LoadBattleLoadingScene("GRABTREASURE_BATTLE_LOADING_WINDOW");
//        GrabTreasureWindow.StartGrabBattle();

        //TODO 临时结算
//        int isWin = 1;// (UnityEngine.Random.Range(0, 1) == 1) ? 1 : 0;
//        RequestResultIndiana(isWin, fragId, grabType, grabUid, level);
    }
    private static void __OnRequestPointIndianaButtonFailed(string text)
    {
        switch (text)
        {
            case "1101":
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_SPIRIT_NO_ENOUGH);
                }break;
            case "2003":
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_TARGET_FRAGMENT_NO_ENOUGH);
                }break;
            case "2010":
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_TARGET_IN_TRUCE_TIME);
                }break;
        }
    }

    /// <summary>
    /// 单次夺宝结算
    /// </summary>
    /// <param name="resultFightFlag"></param>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <param name="robedUid"></param>
    public static void RequestResultIndiana(int resultFightFlag, int tid, int type, int robedUid, int level)
    {
        CS_GrabTreasure_ResultIndiana result = new CS_GrabTreasure_ResultIndiana();
        result.resultFightFlag = resultFightFlag;
        result.tid = tid;
        result.type = type;
        result.robedUid = robedUid;
        result.robotLevel = level;
        HttpModule.Instace.SendGameServerMessage(result, (x) => { __OnRequestResultIndianaSuccess(resultFightFlag, tid, x); }, __OnRequestResultIndianaFailed);
    }
    private static void __OnRequestResultIndianaSuccess(int isWin, int tid, string text)
    {
        SC_GrabTreasure_ResultIndiana retResult = JCode.Decode<SC_GrabTreasure_ResultIndiana>(text);

        GrabTreasureResultData data = new GrabTreasureResultData();
        RoleData tmpMyRoleData = RoleLogicData.GetMainRole();
        data.mIsWin = (isWin == 1) ? true : false;
        data.mFragId = tid;
        data.mPreRoleGold = RoleLogicData.Self.gold;
        data.mPreRoleExp = tmpMyRoleData.exp;
        data.mPreRoleLevel = tmpMyRoleData.level;
//        data.mResult = retResult;
        data.mGetGold = retResult.arr[0].itemNum;
        data.mGetExp = retResult.arr[1].itemNum;

        //减精力
        int tmpCostSpirit = 2;
        RoleLogicData.Self.AddSpirit(-tmpCostSpirit);

        //加物品
        for (int i = 0, count = retResult.arr.Length; i < count; i++)
        {
            if (retResult.arr[i].itemId == -1)
                PackageManager.AddItem(retResult.arr[i]);
            else
                PackageManager.UpdateItem(retResult.arr[i]);
        }

        data.mPostRoleGold = RoleLogicData.Self.gold;
        data.mPostRoleExp = tmpMyRoleData.exp;
        data.mPostRoleLevel = tmpMyRoleData.level;

        string winName = (isWin == 1) ? "GRABTREASURE_BATTLE_RESULT_WIN_WINDOW" : "GRABTREASURE_BATTLE_RESULT_LOSE_WINDOW";
        DataCenter.OpenWindow(winName, data);
    }
    private static void __OnRequestResultIndianaFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 点击夺宝结算关闭按钮
    /// </summary>
    /// <param name="resultFightFlag"></param>
    public static void RequestResultCloseClick(int resultFightFlag)
    {
        CS_GrabTreasure_ResultCloseClick closeClick = new CS_GrabTreasure_ResultCloseClick();
        closeClick.resultFightFlag = resultFightFlag;
        HttpModule.Instace.SendGameServerMessage(closeClick, __OnRequestResultCloseClickSuccess, __OnRequestResultCloseClickFailed);
    }
    private static void __OnRequestResultCloseClickSuccess(string text)
    {
        SC_GrabTreasure_ResultCloseClick retCloseClick = JCode.Decode<SC_GrabTreasure_ResultCloseClick>(text);

        //关闭结算窗口
        DataCenter.CloseWindow("GRABTREASURE_BATTLE_RESULT_WIN_WINDOW");

        //打开选择奖励物品窗口
        DataCenter.OpenWindow("GRABTREASURE_BATTLE_WIN_AWARD_WINDOW", retCloseClick);
    }
    private static void __OnRequestResultCloseClickFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 点击选择奖励物品
    /// </summary>
    /// <param name="awardItem"></param>
    public static void RequestSelectAwardClick(ItemDataBase awardItem, int index)
    {
        CS_GrabTreasure_SelectAwardClick selectAward = new CS_GrabTreasure_SelectAwardClick();
        selectAward.awardItem = awardItem;
        HttpModule.Instace.SendGameServerMessage(selectAward, (x) => { __OnRequestSelectAwardClickSuccess(awardItem, index, x); }, __OnRequestSelectAwardClickFailed);
    }
    private static void __OnRequestSelectAwardClickSuccess(ItemDataBase awardItem, int index, string text)
    {
        SC_GrabTreasure_SelectAwardClick retSelectAward = JCode.Decode<SC_GrabTreasure_SelectAwardClick>(text);

        //加物品
        if (awardItem.itemId == -1)
            PackageManager.AddItem(awardItem);
        else
            PackageManager.UpdateItem(awardItem);

        DataCenter.SetData("GRABTREASURE_BATTLE_WIN_AWARD_WINDOW", "SELECT_AWARD", index);
    }
    private static void __OnRequestSelectAwardClickFailed(string text)
    {
        //TODO
    }
	//点击一键夺宝按钮
	public static void RequestIndianaRobOneButton(int magicTid, int tid)
	{
		CS_GrabTreasure_IndianaRobOneButton RobOne = new CS_GrabTreasure_IndianaRobOneButton();
		RobOne.MagicTid = magicTid;
		RobOne.Tid = tid;
		HttpModule.Instace.SendGameServerMessage (RobOne, (x) => {
			__OnRequestIndianaRobOneButtonSuccess(tid, x);}, __OnRequestIndianaRobOneButtonFailed);
	}
	private static void __OnRequestIndianaRobOneButtonSuccess(int fragId,string text)
	{
		SC_GrabTreasure_IndianaRobOneButton retRobOne = JCode.Decode<SC_GrabTreasure_IndianaRobOneButton>(text);
		int tmpCostSpirit = retRobOne.successTime * 2;
		RoleLogicData.Self.AddSpirit(-tmpCostSpirit);
		for (int i = 0, count = retRobOne.arr.Length; i < count; i++) {
			if (retRobOne.arr[i].itemId == -1)
				PackageManager.AddItem(retRobOne.arr[i]);
			else
				PackageManager.UpdateItem(retRobOne.arr[i]);
		}
		DataCenter.OpenWindow("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", fragId);
		DataCenter.SetData("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", "REFRESH", retRobOne);
	}
	private static void __OnRequestIndianaRobOneButtonFailed(string text)
	{
		switch (text)
		{
			case "1101":
				{
					DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_SPIRIT_NO_ENOUGH);
				} break;
		}
	}
    /// <summary>
    /// 点击夺宝5次按钮
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="robedUid"></param>
    public static void RequestIndianaFiveButton(int tid, int robedUid, int level)
    {
        CS_GrabTreasure_IndianaFiveButton fiveButton = new CS_GrabTreasure_IndianaFiveButton();
        fiveButton.tid = tid;
        fiveButton.robedUid = robedUid;
        fiveButton.robotLevel = level;
        HttpModule.Instace.SendGameServerMessage(fiveButton, (x) => { __OnRequestIndianaFiveButtonSuccess(tid, x); }, __OnRequestIndianaFiveButtonFailed);
    }
    private static void __OnRequestIndianaFiveButtonSuccess(int fragId, string text)
    {
        SC_GrabTreasure_IndianaFiveButton retFiveButton = JCode.Decode<SC_GrabTreasure_IndianaFiveButton>(text);

        //减精力
        int tmpCostSpirit = retFiveButton.successTime * 2;
        RoleLogicData.Self.AddSpirit(-tmpCostSpirit);

        //加物品
        for (int i = 0, count = retFiveButton.arr.Length; i < count; i++)
        {
            if (retFiveButton.arr[i].itemId == -1)
                PackageManager.AddItem(retFiveButton.arr[i]);
            else
                PackageManager.UpdateItem(retFiveButton.arr[i]);
        }

        DataCenter.OpenWindow("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", fragId);
        DataCenter.SetData("GRABTREASURE_FIVE_BATTLE_RESULT_WINDOW", "REFRESH", retFiveButton);
    }
    private static void __OnRequestIndianaFiveButtonFailed(string text)
    {
        switch (text)
        {
            case "1101":
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_SPIRIT_NO_ENOUGH);
                } break;
        }
    }

    /// <summary>
    /// 请求夺宝历史
    /// </summary>
    public static void RequestIndianaHistory()
    {
        CS_GrabTreasure_IndianaHistory history = new CS_GrabTreasure_IndianaHistory();
        HttpModule.Instace.SendGameServerMessage(history, __OnRequestIndianaHistorySuccess, __OnRequestIndianaHistoryFailed);
    }
    private static void __OnRequestIndianaHistorySuccess(string text)
    {
        SC_GrabTreasure_IndianaHistory retHistory = JCode.Decode<SC_GrabTreasure_IndianaHistory>(text);
        DataCenter.SetData("GRABTREASURE_RECORD_WINDOW", "REFRESH", retHistory);
    }
    private static void __OnRequestIndianaHistoryFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 请求法宝合成
    /// </summary>
    public static void RequestIndianaCompose(int tid, List<ItemDataBase> arr)
    {
        CS_GrabTreasure_IndianaCompose compose = new CS_GrabTreasure_IndianaCompose();
        compose.tid = tid;
        compose.arr = arr;
        HttpModule.Instace.SendGameServerMessage(compose, (x) => { __OnRequestIndianaComposeSuccess(tid, x); }, __OnRequestIndianaComposeFailed);
    }
    private static void __OnRequestIndianaComposeSuccess(int tid, string text)
    {
        SC_GrabTreasure_IndianaCompose retCompose = JCode.Decode<SC_GrabTreasure_IndianaCompose>(text);

        if (retCompose.composeFlag == 0)
        {
            DataCenter.OpenMessageWindow("合成失败");
            return;
        }

        //加物品
        for (int i = 0, count = retCompose.arr.Length; i < count; i++)
        {
            if (retCompose.arr[i].itemId == -1)
                PackageManager.AddItem(retCompose.arr[i]);
            else
                PackageManager.UpdateItem(retCompose.arr[i]);
        }
        //移除物品
        GrabTreasureWindow.RemoveFragmentsByCompose(tid);

        GrabTreasureWindow tmpWin = DataCenter.GetData("GRABTREASURE_WINDOW") as GrabTreasureWindow;
        ItemDataBase tmpItem = tmpWin.getObject("CURRENT_COMPOSE_TARGET_ITEM") as ItemDataBase;

        DataCenter.OpenWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW", tmpItem.tid);
    }
    private static void __OnRequestIndianaComposeFailed(string text)
    {
        switch (text)
        {
            case "1111":
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_COMPOSE_FRAGMENT_NO_ENOUGH);
                }break;
        }
    }

    /// <summary>
    /// 获取免战时间
    /// </summary>
    public static void RequestGetTruceTime()
    {
        CS_GrabTreasure_GetTruceTime getTruceTime = new CS_GrabTreasure_GetTruceTime();
        HttpModule.Instace.SendGameServerMessage(getTruceTime, __OnRequestGetTruceTimeSuccess, __OnRequestGetTruceTimeFailed);
    }
    private static void __OnRequestGetTruceTimeSuccess(string text)
    {
        SC_GrabTreasure_GetTruceTime retGetTruceTime = JCode.Decode<SC_GrabTreasure_GetTruceTime>(text);
        DataCenter.SetData("GRABTREASURE_WINDOW", "REFRESH_PEACE_TIME", (long)(retGetTruceTime.time / 1000));
    }
    private static void __OnRequestGetTruceTimeFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 用免战牌
    /// </summary>
    /// <param name="truceItemData"></param>
    /// <param name="peaceTime"></param>
    public static void RequestUseTruceToken(ItemDataBase truceItemData, long peaceTime)
    {
        CS_GrabTreasure_UseTruceToken useTruceToken = new CS_GrabTreasure_UseTruceToken();
		useTruceToken.truceCard = truceItemData;
        HttpModule.Instace.SendGameServerMessage(useTruceToken, (x) => { __OnRequestUseTruceTokenSuccess(peaceTime, truceItemData, x); }, __OnRequestUseTruceTokenFailed);
    }
    private static void __OnRequestUseTruceTokenSuccess(long peaceTime, ItemDataBase truceItemData, string text)
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
        PackageManager.RemoveItem(truceItemData.tid, truceItemData.itemId, 1);
        //刷新免战牌数量显示
        DataCenter.SetData("GRABTREASURE_USE_PEACE_WINDOW", "REFRESH_PEACE_COUNT", null);
    }
    private static void __OnRequestUseTruceTokenFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 买免战牌
    /// </summary>
    /// <param name="sIndex"></param>
    /// <param name="num"></param>
    public static void RequestBuyTruceToken(int cost, int tid, int sIndex, int num)
    {
        CS_GrabTreasure_BuyTruceToken buyTruceToken = new CS_GrabTreasure_BuyTruceToken();
        buyTruceToken.sIndex = sIndex;
        buyTruceToken.num = num;
        HttpModule.Instace.SendGameServerMessage(buyTruceToken, (x) => { __OnRequestBuyTruceTokenSuccess(cost, tid, x); }, __OnRequestBuyTruceTokenFailed);
    }
    private static void __OnRequestBuyTruceTokenSuccess(int cost, int tid, string text)
    {
        SC_GrabTreasure_BuyTruceToken retBuyTruceToken = JCode.Decode<SC_GrabTreasure_BuyTruceToken>(text);
        if(retBuyTruceToken.isBuySuccess == 1)
        {
            //减元宝
            RoleLogicData.Self.AddDiamond(-cost);
            //加物品
            for (int i = 0, count = retBuyTruceToken.buyItem.Length; i < count; i++)
            {
                if(retBuyTruceToken.buyItem[i].itemId == -1)
                    PackageManager.AddItem(retBuyTruceToken.buyItem);
                else
                    PackageManager.UpdateItem(retBuyTruceToken.buyItem);
            }
            //刷新免战牌数量显示
            DataCenter.SetData("GRABTREASURE_USE_PEACE_WINDOW", "REFRESH_PEACE_COUNT", null);
        }
        else
            DataCenter.OpenMessageWindow("免战牌购买失败");
    }
    private static void __OnRequestBuyTruceTokenFailed(string text)
    {
        //TODO
    }
}
