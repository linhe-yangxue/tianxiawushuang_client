using UnityEngine;
using System.Collections;
using DataTable;
using System.Collections.Generic;
using System;

public class RammbockNetManager
{
    /// <summary>
    /// 获取爬塔信息
    /// </summary>
    /// <param name="isJustRequestData">是否仅请求数据</param>
    public static void RequestGetTowerClimbingInfo(bool isJustRequestData = false, Action successCallback = null)
    {
        CS_Rammbock_GetTowerClimbingInfo climbingInfo = new CS_Rammbock_GetTowerClimbingInfo();
        HttpModule.Instace.SendGameServerMessage(climbingInfo, (string text) =>
        {
            __OnRequestRammbockClimbingInfoSuccess(text, isJustRequestData);
            if (successCallback != null)
                successCallback();
        }, __OnRequestRammbockClimbingInfoFailed);
    }
    private static void __OnRequestRammbockClimbingInfoSuccess(string text, bool isJustRequestData)
    {
        SC_Rammbock_GetTowerClimbingInfo retClimbingInfo = JCode.Decode<SC_Rammbock_GetTowerClimbingInfo>(text);
        if (retClimbingInfo.ret == 1)
        {
            if (isJustRequestData)
                DataCenter.SetData("RAMMBOCK_WINDOW", "JUST_RESTORE_REQUEST_DATA", retClimbingInfo);
            else
                DataCenter.SetData("RAMMBOCK_WINDOW", "REFRESH", retClimbingInfo);
        }
    }
    private static void __OnRequestRammbockClimbingInfoFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 开始爬塔
    /// </summary>
    /// <param name="crtTierStars"></param>
    public static void RequestClimbTowerStart(int currTier, int crtTierStars)
    {
        DataCenter.SetData("RAMMBOCK_WINDOW", "CURRENT_SELECT_STARS", crtTierStars);

        CS_Rammbock_ClimbTowerStart climbStart = new CS_Rammbock_ClimbTowerStart();
        climbStart.crtTierStars = crtTierStars;
        HttpModule.Instace.SendGameServerMessage(climbStart, x => { __OnRequestRammbockClimbTowerStartSuccess(currTier, crtTierStars, x); }, __OnRequestRammbockClimbTowerStartFailed);
    }
    private static void __OnRequestRammbockClimbTowerStartSuccess(int currTier, int tierStars, string text)
    {
        //开始爬塔
        DataCenter.CloseWindow("RAMMBOCK_SELECT_DIFFICULTY_WINDOW");
        DataCenter.CloseWindow("RAMMBOCK_WINDOW");
        DataCenter.Self.set("RAMMBOCK_START_CLIMB", true);
        //by chenliang
        //beign

//        DataCenter.Self.set("I_AM_IN_RAMMBOCK", true);
//----------------
        //防止Update里查找I_AM_IN_RAMMBOCK
        RammbockBattle.IsInRammbockBattle = true;

        //end
        int win_condition= DataCenter.mStageTable.GetData(int.Parse(DataCenter.mClimbingTowerConfig.GetData(currTier, "STAR_"+tierStars.ToString())),"ADDSTAR_0");
        DataCenter.Self.set("NOW_STAGE_WIN_CONDITION", win_condition);
        DataCenter.Self.set("NOW_RAMMBOCK_ID", currTier);
        DataCenter.Self.set("NOW_RAMMBOCK_STAR", tierStars);

        //设置关卡信息
        GlobalModule.ClearAllWindow();
        DataRecord climbConfig = DataCenter.mClimbingTowerConfig.GetRecord(currTier);
        int stageIndex = (int)climbConfig.getObject("STAR_" + tierStars.ToString());
        DataCenter.Set("CURRENT_STAGE", stageIndex);
        //设置群魔乱入战斗开始标志
        MainProcess.LoadBattleLoadingScene();

        /*
        //TODO 测试，随机一个战斗结果
        int isWin = 0;// (Random.Range(1, 2) == 1) ? 1 : 0;
        RammbockNetManager.RequestClimbTowerResult(isWin);
         * */
    }
    private static void __OnRequestRammbockClimbTowerStartFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 结束爬塔
    /// </summary>
    /// <param name="awardIndex"></param>
    public static void RequestClimbTowerResult(int isWin, ClimbTowerBuffInfo[] buffEffect)
    {
        CS_Rammbock_ClimbTowerResult climbResult = new CS_Rammbock_ClimbTowerResult();
        climbResult.isWin = isWin;
        climbResult.buffEffect = buffEffect;
        HttpModule.Instace.SendGameServerMessage(climbResult, x => { __OnRequestRammbockClimbTowerResultSuccess(isWin, x); }, __OnRequestRammbockClimbTowerResultFailed);
    }
    private static void __OnRequestRammbockClimbTowerResultSuccess(int isWin, string text)
    {
        SC_Rammbock_ClimbTowerResult retClimbResult = JCode.Decode<SC_Rammbock_ClimbTowerResult>(text);

        if (retClimbResult != null && retClimbResult.arr != null && retClimbResult.arr.Length > 0)
        {
            //加所有传来的Item
            foreach (ItemDataBase tmpItem in retClimbResult.arr)
            {
                if (tmpItem.itemId == -1)
                    PackageManager.AddItem(tmpItem);
                else
                    PackageManager.UpdateItem(tmpItem);
            }
        }
        //去除开始爬塔状态
        DataCenter.Self.set("RAMMBOCK_START_CLIMB", false);
        //by chenliang
        //beign

//        DataCenter.Self.set("I_AM_IN_RAMMBOCK", false);
//--------------
        //防止Update里查找I_AM_IN_RAMMBOCK
        RammbockBattle.IsInRammbockBattle = false;

        //end
        DataCenter.Self.set("NOW_STAGE_WIN_CONDITION", null);
        if (isWin == 1)
        {
            //赢了
            RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
            int selStarsNum = (int)win.getObject("CURRENT_SELECT_STARS");
            int currTier = win.m_climbingInfo.nextTier;
            RammbockBattleResultData resultData = new RammbockBattleResultData();
            resultData.currTier = currTier;
            resultData.selStarsNum = selStarsNum;
            resultData.retClimb = retClimbResult;
            DataCenter.OpenWindow("RAMMBOCK_BATTLE_RESULT_WIN_WINDOW", resultData);
        }
        else
        {
            //输了
            RammbockWindow win = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
            //设置标志，需要显示打折商品
            RammbockTreasureData data = new RammbockTreasureData();
            data.currTier = win.m_climbingInfo.nextTier;
            data.totalStar = win.m_climbingInfo.currentStars;
            data.itemData = retClimbResult.itemOnSale;
            data.saleItemPrice = retClimbResult.saleItemPrice;
            data.originalItemPrice = retClimbResult.originalItemPrice;
            DataCenter.SetData("RAMMBOCK_WINDOW", "RAMMBOCK_TREASURE", data);
            DataCenter.OpenWindow("RAMMBOCK_BATTLE_RESULT_LOSE_WINDOW");
        }
    }
    private static void __OnRequestRammbockClimbTowerResultFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 买商品
    /// </summary>
    public static void RequestRammbockClimbTowerBuyCommodity(int cost, ItemDataBase itemData)
    {
        CS_Rammbock_ClimbTowerBuyCommodity buy = new CS_Rammbock_ClimbTowerBuyCommodity();
        HttpModule.Instace.SendGameServerMessage(buy, x => { __OnRequestRammbockClimbTowerBuyCommodiySuccess(cost, itemData, x); }, __OnRequestRammbockClimbTowerBuyCommodiyFailed);
    }
    private static void __OnRequestRammbockClimbTowerBuyCommodiySuccess(int cost, ItemDataBase itemData, string text)
    {
        SC_Rammbock_ClimbTowerBuyCommodiy retBuy = JCode.Decode<SC_Rammbock_ClimbTowerBuyCommodiy>(text);

        //购买成功
        DataCenter.CloseWindow("RAMMBOCK_TREASURE_WINDOW");
        RammbockWindow winRamm = DataCenter.GetData("RAMMBOCK_WINDOW") as RammbockWindow;
        winRamm.m_climbingInfo.saleItemState = 1;
        DataCenter.SetData("RAMMBOCK_OVER_WINDOW", "REFRESH_TREASURE", null);
        //减元宝
        GameCommon.RoleChangeDiamond(-cost);

        //更新商品数据
        PackageManager.UpdateItem(retBuy.arr);
    }
    private static void __OnRequestRammbockClimbTowerBuyCommodiyFailed(string text)
    {
        switch (text)
        {
            case "1101":
                {
                    //元宝不足
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NO_ENOUGH_DIAMOND);
                }break;
        }
    }

    /// <summary>
    /// 选择Buff
    /// </summary>
    /// <param name="buffIndex"></param>
    public static void RequestClimbTowerChooseBuff(int buffIndex)
    {
        CS_Rammbock_ClimbTowerChooseBuff chooseBuff = new CS_Rammbock_ClimbTowerChooseBuff();
        chooseBuff.buffIndex = buffIndex;
        HttpModule.Instace.SendGameServerMessage(chooseBuff, __OnRequestClimbTowerChooseBuffSuccess, __OnRequestClimbTowerChooseBuffFailed);
    }
    private static void __OnRequestClimbTowerChooseBuffSuccess(string text)
    {
        SC_Rammbock_ClimbTowerChooseBuff retChooseBuff = JCode.Decode<SC_Rammbock_ClimbTowerChooseBuff>(text);

        DataCenter.CloseWindow("RAMMBOCK_ATTRI_ADD_WINDOW");
        RammbockNetManager.RequestGetTowerClimbingInfo();
    }
    private static void __OnRequestClimbTowerChooseBuffFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 请求重置
    /// </summary>
    public static void RequestRammbockResetClimbTower()
    {
        CS_Rammbock_ResetClimbTower climbTower = new CS_Rammbock_ResetClimbTower();
        HttpModule.Instace.SendGameServerMessage(climbTower, __OnRequestRammbockResetClimbTowerSuccess, __OnRequestRammbockResetClimbTowerFailed);
    }
    private static void __OnRequestRammbockResetClimbTowerSuccess(string text)
    {
        SC_Rammbock_ResetClimbTower retClimbTower = JCode.Decode<SC_Rammbock_ResetClimbTower>(text);

        RammbockNetManager.RequestGetTowerClimbingInfo();
    }
    private static void __OnRequestRammbockResetClimbTowerFailed(string text)
    {
        //TODO
    }

    /// <summary>
    /// 一键三星
    /// </summary>
    public static void RequestClimbTowerSweep(ClimbTowerBuffInfo[] buffEffect)
    {
        CS_ClimbTowerSweep climbTower = new CS_ClimbTowerSweep();
        climbTower.buffEffect = buffEffect;
        HttpModule.Instace.SendGameServerMessage(climbTower, OnRequestClimbTowerSweepSuccess, OnRequestFailed);
    }
    private static void OnRequestClimbTowerSweepSuccess(string text)
    {
        SC_ClimbTowerSweep ret = JCode.Decode<SC_ClimbTowerSweep>(text);
        if (ret != null)
        {
            //加星
            RammbockBase.AddStar();

            //更新获得消耗品-威名和银币
            RammbockBase.AddConsumables(ret.awardIndexes);

            //更新获得的包厢奖励
			List<ItemDataBase> itemList = PackageManager.UpdateItem(ret.tierPassRewards, true);
			ret.tierPassRewards = itemList.ToArray();
            //setdata
            RammbockBase.SetClimbData(ret);

            //刷新ui
            DataCenter.SetData(UIWindowString.rammbock_list_window, "INIT_UI", null);
        }

    }
    private static void OnRequestFailed(string text)
    {
        //TODO
    }
}
