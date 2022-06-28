using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logic;
using DataTable;
using UnityEngine;
using System.Collections;

public class NetManager : Singleton<NetManager>
{
    private const int needLoginTime = 12 * 3600 * 1000;// 12个小时
    static public Dictionary<PACKAGE_TYPE, HttpModule.CallBack> dicSuccessCallBack = new Dictionary<PACKAGE_TYPE, HttpModule.CallBack>();
    static public Dictionary<PACKAGE_TYPE, HttpModule.CallBack> dicFailCallBack = new Dictionary<PACKAGE_TYPE, HttpModule.CallBack>();
    void Awake()
    {
        dicSuccessCallBack.Clear();
        dicFailCallBack.Clear();

        dicSuccessCallBack.Add(PACKAGE_TYPE.PET, RequestPetSuccess);
        dicFailCallBack.Add(PACKAGE_TYPE.PET, RequestPetFail);

        dicSuccessCallBack.Add(PACKAGE_TYPE.PET_FRAGMENT, RequestPetFragmentSuccess);
        dicFailCallBack.Add(PACKAGE_TYPE.PET_FRAGMENT, RequestPetFragmentFail);

        dicSuccessCallBack.Add(PACKAGE_TYPE.EQUIP, RequestEquipSuccess);
        dicFailCallBack.Add(PACKAGE_TYPE.EQUIP, RequestEquipFail);

        dicSuccessCallBack.Add(PACKAGE_TYPE.EQUIP_FRAGMENT, RequestEquipFragmentSuccess);
        dicFailCallBack.Add(PACKAGE_TYPE.EQUIP_FRAGMENT, RequestEquipFragmentFail);

        dicSuccessCallBack.Add(PACKAGE_TYPE.MAGIC, RequestMagicSuccess);
        dicFailCallBack.Add(PACKAGE_TYPE.MAGIC, RequestMagicFail);

        dicSuccessCallBack.Add(PACKAGE_TYPE.MAGIC_FRAGMENT, RequestMagicFragmentSuccess);
        dicFailCallBack.Add(PACKAGE_TYPE.MAGIC_FRAGMENT, RequestMagicFragmentFail);

        dicSuccessCallBack.Add(PACKAGE_TYPE.CONSUME_ITEM, RequestConsumeItemSuccess);
        dicFailCallBack.Add(PACKAGE_TYPE.CONSUME_ITEM, RequestConsumeItemFail);
    }


    /// <summary>
    /// 带回调的应答协程
    /// </summary>
    /// <param name="message"> 发送数据 </param>
    /// <param name="funName"> 消息名称 </param>
    /// <param name="protectedSuccessMethod"> 受保护的成功回调，无论此协程是否被强行中断，只要收到成功回复就会执行 </param>
    /// <param name="successMethod"> 不受保护的成功回调，协程被中断就不会执行 </param>
    /// <param name="failMethod"> 不受保护的失败回调，协程被中断就不会执行 </param>
    /// <returns></returns>
    public static IEnumerator WaitResp(MessageBase message, string funName, HttpModule.CallBack protectedSuccessMethod, HttpModule.CallBack successMethod, HttpModule.CallBack failMethod)
    {
        string resp = null;
        int status = 0;

        HttpModule.CallBack onSuccess = x => { status = 1; resp = x; };
        HttpModule.CallBack onFail = x => { status = -1; resp = x; };
        onSuccess += protectedSuccessMethod;

        if (funName == null)
        {
            HttpModule.Instace.SendGameServerMessage(message, onSuccess, onFail);
        }
        else
        {
            HttpModule.Instace.SendGameServerMessage(message, funName, onSuccess, onFail);
        }

        while (status == 0)
        {
            yield return null;
        }

        if (status == 1)
        {
            if (successMethod != null)
                successMethod.Invoke(resp);
        }
        else
        {
            if (failMethod != null)
                failMethod.Invoke(resp);
        }
    }

    public static IEnumerator WaitResp(MessageBase message, string funName, HttpModule.CallBack successMethod, HttpModule.CallBack failMethod)
    {
        return WaitResp(message, funName, null, successMethod, failMethod);
    }

    public static IEnumerator WaitResp(MessageBase message, HttpModule.CallBack successMethod, HttpModule.CallBack failMethod)
    {
        return WaitResp(message, null, null, successMethod, failMethod);
    }

    public static Coroutine StartWaitResp(MessageBase message, string funName, HttpModule.CallBack successMethod, HttpModule.CallBack failMethod)
    {
        return Instace.StartCoroutine(WaitResp(message, funName, successMethod, failMethod));
    }

    public static Coroutine StartWaitResp(MessageBase message, HttpModule.CallBack successMethod, HttpModule.CallBack failMethod)
    {
        return Instace.StartCoroutine(WaitResp(message, successMethod, failMethod));
    }

    public static Coroutine StartWaitPackageData(PACKAGE_TYPE packType)
    {
        CS_GetPackageList getPackageList = new CS_GetPackageList();
        getPackageList.pkgId = (int)packType;
        return StartWaitResp(getPackageList, "CS_GetPackageList", dicSuccessCallBack[packType], dicFailCallBack[packType]);
    }
    public static void ReConnect()
    {
        //by chenliang
        //begin

        //        NetManager.Instace.StartCoroutine(NetManager.RequestGameDataThenLoad(false));
        //----------------
        NetManager.Instace.StartCoroutine(NetManager.__RequestReConnectGameData(false));

        //end
    }

    //by chenliang
    //begin

    private static bool mRequestReConnectGameData = false;      //防止RequestReConnectGameData重入
    /// <summary>
    /// 重连获取数据
    /// </summary>
    /// <param name="isRefreshUI"></param>
    /// <returns></returns>
    private static IEnumerator __RequestReConnectGameData(bool isRefreshUI = true)
    {
        if (mRequestReConnectGameData)
            yield break;
        mRequestReConnectGameData = true;

        yield return NetManager.Instace.StartCoroutine(RequestGameDataThenLoad(isRefreshUI));

        mRequestReConnectGameData = false;
    }

    //end
    /*返回登录*/
    public static IEnumerator RequestGameDataThenLoad(bool isRefreshUI = true)
    {
        // 获取包裹数据
        for (int i = (int)PACKAGE_TYPE.PET; i < (int)PACKAGE_TYPE.MAX; ++i)
        {
            yield return StartWaitPackageData((PACKAGE_TYPE)i);
        }

        // 清空副本数据
        MapLogicData.Reset();

        // 获取主线副本数据
        yield return new BattleMainMapRequester().Start();

        // 获取活动副本数据
        //yield return new BattleActiveMapRequester().Start();

        // 获得点星数据
        yield return StartWaitPointStarValue();

        //        // 获得每日签到数据
        //        yield return StartWaitDailySignQuery();

        // 获取日常任务数据
        yield return new GetDailyTaskDataRequester().Start();

        // 获取成就数据
        yield return new GetAchievementDataRequester().Start();

        //	获取次日登陆请求数据
        yield return StartWaitMorrowLandQuery();

        //		//  获得开服狂欢列表
        //		yield return RequestGetRevelryList();

        // 加载
        TeamManager.InitTeamListData();
        Relationship.Init();
        if (isRefreshUI)
        {
            //by chenlaing
            //begin

            //             DataCenter.OpenWindow("TEAM_WINDOW");
            // 			MainSceneLoadingUI.LoadMainScene();
            //-------------------
            DataCenter.OpenWindow("TEAM_WINDOW");
            //改为MainGameSceneLoadingWindow.LoadMainScene
            yield return Instace.StartCoroutine(MainGameSceneLoadingWindow.LoadMainScene(null));

            //end
        }

        //added by xuke begin
        //		DataCenter.SetData("ROLE_SEL_BOTTOM_LEFT_GROUP","UPDATE_TEAM_RED_POINT", TeamManager.HasFreeTeamPos() && PetLogicData.Self.HasFreePet());
        DataCenter.SetData("ROLE_SEL_BOTTOM_LEFT_GROUP", "UPDATE_TEAM_RED_POINT", TeamManager.HasFreeTeamPos() && PetLogicData.Self.HasFreePet() || RoleEquipLogicData.Self.HasFreeEquipAndFreeEquipPos());
        //end
    }


    //-------------------------------------------------------------------------
    // get package data
    static public void RequestPackageData(PACKAGE_TYPE packType)
    {
        CS_GetPackageList getPackageList = new CS_GetPackageList();
        getPackageList.pkgId = (int)packType;
        getPackageList.pt = "CS_GetPackageList";
        HttpModule.Instace.SendGameServerMessage(getPackageList, dicSuccessCallBack[packType], dicFailCallBack[packType]);
    }

    //-------------------------------------------------
    // pet
    //-------------------------------------------------
    static public void RequestPetFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }

    static public void RequestPetSuccess(string text)
    {
        DEBUG.Log("RequestPetSuccess:text = " + text);
        CS_RequestRoleData.ReadPetFromData(text);
    }

    //-------------------------------------------------
    // pet fragment
    //-------------------------------------------------
    static public void RequestPetFragmentFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestPetFragmentSuccess(string text)
    {
        DEBUG.Log("RequestPetFragmentSuccess:text = " + text);
        CS_RequestRoleData.ReadPetFragmentFromData(text);
    }

    //-------------------------------------------------
    // equip
    //-------------------------------------------------
    static public void RequestEquipFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestEquipSuccess(string text)
    {
        DEBUG.Log("RequestEquipSuccess:text = " + text);
        CS_RequestRoleData.ReadEquipFromData(text);
    }

    //-------------------------------------------------
    // equip fragment
    //-------------------------------------------------
    static public void RequestEquipFragmentFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestEquipFragmentSuccess(string text)
    {
        DEBUG.Log("RequestEquipFragmentSuccess:text = " + text);
        CS_RequestRoleData.ReadEquipFragmentFromData(text);
    }

    //-------------------------------------------------
    // magic
    //-------------------------------------------------
    static public void RequestMagicFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestMagicSuccess(string text)
    {
        DEBUG.Log("RequestMagicSuccess:text = " + text);
        CS_RequestRoleData.ReadMagicFromData(text);
    }

    //-------------------------------------------------
    // magic fragment
    //-------------------------------------------------
    static public void RequestMagicFragmentFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestMagicFragmentSuccess(string text)
    {
        DEBUG.Log("RequestMagicFragmentSuccess:text = " + text);
        CS_RequestRoleData.ReadMagicFragmentFromData(text);
    }

    //-------------------------------------------------
    // consume item
    //-------------------------------------------------
    static public void RequestConsumeItemFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestConsumeItemSuccess(string text)
    {
        DEBUG.Log("RequestConsumeItemSuccess:text = " + text);
        CS_RequestRoleData.ReadConsumeItemFromData(text);
    }

    static public void RequestFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        int errorId = int.Parse(text);
        if (errorId == 1310)
        {
            DataCenter.OpenMessageWindow("已取消申请");
            GlobalModule.DoLater(() => DataCenter.CloseWindow("UNION_CHECK_WINDOW"), 1.0f);
        }
        else if (errorId == 1319)
        {
            DataCenter.OpenMessageWindow("已加入一个宗门");
            GlobalModule.DoLater(() =>
            {
                DataCenter.CloseWindow("UNION_LIST_WINDOW");
                MainUIScript.Self.ShowMainBGUI();
            }, 1.0f);
        }
        //by chenliang
        //begin

        else if (errorId == -50)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SDK_LOGIN_FAILED, () =>
            {
                DataCenter.OpenWindow("FIRST_LOAD_SDK_WINDOW");
                //停止心跳包
                StaticDefine.useHeartbeat = false;
            });
        }

        //end
        return;
    }

    static public void RequestSuccess(string text)
    {
        DEBUG.Log("RequestSuccess:text = " + text);
    }

    //-------------------------------------------------
    // 获得各系统状态
    //-------------------------------------------------
    static public void RequestGetNotification()
    {
        CS_GetNotification GetNotification = new CS_GetNotification();
        GetNotification.pt = "CS_GetNotification";
        HttpModule.Instace.SendGameServerMessage(GetNotification, RequestGetNotificationSuccess, RequestGetNotificationFail);
    }

    static public void RequestGetNotificationFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestGetNotificationSuccess(string text)
    {
        DEBUG.Log("RequestGetNotificationSuccess:text = " + text);

        Dictionary<string, object> dicSystemState = JCode.Decode<Dictionary<string, object>>(text);
        Array stateList = dicSystemState["arr"] as Array;
        SystemStateManager.SetAllNotifState(stateList);

        DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "UPDATE_MARK", true);
    }

    //-------------------------------------------------
    // 添加道具
    //-------------------------------------------------
    //-------------------------------------------------------------------------
    // add item
    static public void RequestAddItem()
    {
        for (PACKAGE_TYPE i = PACKAGE_TYPE.PET; i < PACKAGE_TYPE.MAX; i++)
        {
            CS_AddItem addItem = new CS_AddItem();
            addItem.pt = "CS_AddItem";
            addItem.packageId = (int)i;
            addItem.tid = 1000 + (int)i;
            HttpModule.Instace.SendGameServerMessage(addItem, RequestAddItemSuccess, RequestConsumeItemFail);
        }
    }

    static public void RequestAddItemFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestAddItemSuccess(string text)
    {
        DEBUG.Log("RequestAddItemSuccess:text = " + text);

    }

    //-------------------------------------------------
    // 装备强化
    //-------------------------------------------------
    static public void RequestEquipStrengthen(int iItemId, int iTid, int iCount)
    {
        CS_StrengthenEquip equipStrengthen = new CS_StrengthenEquip();
        equipStrengthen.pt = "CS_StrengthenEquip";
        equipStrengthen.itemId = iItemId;
        equipStrengthen.tid = iTid;
        equipStrengthen.strenTimes = iCount;

        HttpModule.Instace.SendGameServerMessage(equipStrengthen, RequestEquipStrengthenSuccess, RequestEquipStrengthenFail);
    }

    static public void RequestEquipStrengthenFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestEquipStrengthenSuccess(string text)
    {
        DEBUG.Log("RequestEquipStrengthenSuccess:text = " + text);
        DataCenter.SetData("EQUIP_STRENGTHEN_INFO_WINDOW", "EQUIP_STRENGTHEN_SUCCESS", text);
    }
    //-------------------------------------------------
    // 天命值
    //-------------------------------------------------
    static public void RequestFateValue(int iItemId, int iTid)
    {
        CS_FateQuery roleFate = new CS_FateQuery();
        roleFate.pt = "CS_FateQuery";
        roleFate.itemId = iItemId;
        roleFate.tid = iTid;

        HttpModule.Instace.SendGameServerMessage(roleFate, RequestFateValueSuccess, RequestFateValueeFail);
    }

    static public void RequestFateValueeFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestFateValueSuccess(string text)
    {
        DEBUG.Log("RequestFateValueSuccess:text = " + text);
        SC_FateQuery item = JCode.Decode<SC_FateQuery>(text);
        int fateValue = item.fateValue;
        DataCenter.SetData("FATE_INFO_WINDOW", "SET_FATE_VALUE", fateValue);
    }
    //-------------------------------------------------
    // 天命升级
    //-------------------------------------------------
    static public void RequestFateUpgrade(int iItemId, int iTid)
    {
        ConsumeItemLogicData consumeItemLogicData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
        if (null == consumeItemLogicData)
            return;

        ConsumeItemData consumeItemData = consumeItemLogicData.GetDataByTid((int)ITEM_TYPE.FATE_STONE);
        if (consumeItemData != null)
        {
            CS_FateUpgrade fateUpgrade = new CS_FateUpgrade();
            fateUpgrade.pt = "CS_FateUpgrade";
            ItemDataBase itemData = new ItemDataBase();
            itemData.itemId = consumeItemData.itemId;
            itemData.tid = consumeItemData.tid;
            itemData.itemNum = TableCommon.GetNumberFromFateConfig(TeamManager.GetActiveDataByTeamPos((int)TeamManager.mCurTeamPos).fateLevel, "COST_NUM");
            fateUpgrade.consume = itemData;
            fateUpgrade.itemId = iItemId;
            fateUpgrade.tid = iTid;

            HttpModule.Instace.SendGameServerMessage(fateUpgrade, RequestFateUpgradeSuccess, RequestFateUpgradeFail);
        }
    }

    static public void RequestFateUpgradeFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        DataCenter.SetData("FATE_INFO_WINDOW", "FATE_STRENGTHEN_FAILE", text);
        return;
    }

    static public void RequestFateUpgradeSuccess(string text)
    {
        DEBUG.Log("RequestFateUpgradeSuccess:text = " + text);
        //        SC_FateUpgrade item = JCode.Decode<SC_FateUpgrade>(text);
        //        int isFateOk = item.isFateSuccess;
        DataCenter.SetData("FATE_INFO_WINDOW", "FATE_STRENGTHEN_SUCCESS", text);
    }

    //-------------------------------------------------
    // 突破
    //-------------------------------------------------
    static public void RequestBreakUpgrade(ActiveData curRoleData, List<ItemDataBase> costItemDataList)
    {
        ConsumeItemLogicData consumeItemLogicData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
        if (null == consumeItemLogicData)
            return;

        int breakSelfIcon = 0;
        int iItemType = curRoleData.tid / 1000;
        if (iItemType == (int)ITEM_TYPE.CHARACTER)
        {
            breakSelfIcon = 0;
        }
        else if (iItemType == (int)ITEM_TYPE.PET)
        {
            breakSelfIcon = TableCommon.GetNumberFromBreakLevelConfig(curRoleData.breakLevel, "ACTIVE_NUM");
        }

        List<PetData> petDataList;
        PetLogicData.Self.GetBreakStuffPetList(out petDataList, curRoleData);

        ConsumeItemData consumeItemData = consumeItemLogicData.GetDataByTid((int)ITEM_TYPE.BREAK_STONE);

        if (consumeItemData != null)
        {
            CS_BreakUpgrade breakUpgrade = new CS_BreakUpgrade();
            breakUpgrade.pt = "CS_BreakUpgrade";

            ItemDataBase itemData = new ItemDataBase();
            itemData.itemId = consumeItemData.itemId;
            itemData.tid = consumeItemData.tid;
            itemData.itemNum = TableCommon.GetNumberFromBreakLevelConfig(curRoleData.breakLevel, "NEED_GEM_NUM");

            breakUpgrade.consume = new ItemDataBase[breakSelfIcon + 1];
            breakUpgrade.consume[0] = itemData;

            costItemDataList.Clear();
            for (int i = 0; i < breakSelfIcon; i++)
            {
                breakUpgrade.consume[i + 1] = GameCommon.CreateItemDataBase(petDataList[i]);
                costItemDataList.Add(breakUpgrade.consume[i + 1]);
            }

            breakUpgrade.itemId = curRoleData.itemId;
            breakUpgrade.tid = curRoleData.tid;
            HttpModule.Instace.SendGameServerMessage(breakUpgrade, x => RequestbreakUpgradeSuccess(itemData, x), RequestbreakUpgradeFail);

        }
    }

    static public void RequestbreakUpgradeFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestbreakUpgradeSuccess(ItemDataBase itemData, string text)
    {
        DEBUG.Log("RequestbreakUpgradeSuccess:text = " + text);
        DataCenter.SetData("BREAK_INFO_WINDOW", "BREAK_UPGRAGE_SUCCESS", itemData);
        DataCenter.SetData("TEAM_INFO_WINDOW", "UPDATE_CUR_TEAM_POS_INFO", null);
    }

    //宠物分解
    //static public void RequestPetResolve(ItemDataBase[] nativeArr)
    //{
    //    CS_PetDisenchant cs = new CS_PetDisenchant(nativeArr);

    //    HttpModule.Instace.SendGameServerMessage(cs, "CS_PetDisenchant", RequestPetResolveSuccess, RequestPetResolveFail);
    //}

    static public void RequestPetResolveFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    //static public void RequestPetResolveSuccess(string text)
    //{
    //    DEBUG.Log("RequestPetResolveSuccess:text = " + text);
    //    SC_PetDisenchant item = JCode.Decode<SC_PetDisenchant>(text);
    //    var arr=item.resolventDataArr;
    //    for(int i=0;i<arr.Length;i++)
    //    {
    //        DEBUG.Log("itemID:" + arr[i].itemId + "___" + "tID:" + arr[i].tid + "___" + "num:" + arr[i].itemNum);
    //    }
    //}


    static public void RequestEquipDisenchantFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestEquipDisenchantSuccess(string text)
    {
        DEBUG.Log("RequestEquipDisenchantSuccess:text = " + text);
        SC_EquipDisenchant item = JCode.Decode<SC_EquipDisenchant>(text);
        var arr = item.arr;
        for (int i = 0; i < arr.Length; i++)
        {
            DEBUG.Log("itemID:" + arr[i].itemId + "___" + "tID:" + arr[i].tid + "___" + "num:" + arr[i].itemNum);
        }
    }



    static public void RequestPetRebirthFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }



    static public void RequestMagicRebirthFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestMagicRebirthSuccess(string text)
    {
        DEBUG.Log("RequestMagicRebirthSuccess:text = " + text);
        SC_PetDisenchant item = JCode.Decode<SC_PetDisenchant>(text);
        var arr = item.arr;
        for (int i = 0; i < arr.Length; i++)
        {
            DEBUG.Log("itemID:" + arr[i].itemId + "___" + "tID:" + arr[i].tid + "___" + "num:" + arr[i].itemNum);
        }
    }
    //-------------------------------------------------
    // 排名活动战力请求
    //-------------------------------------------------
    static public void RequestActivityPower()
    {
        CS_Ranklist_FightingList power_rank = new CS_Ranklist_FightingList();
        HttpModule.Instace.SendGameServerMessage(power_rank, RequestActivityPowerSuccess, RequestActivityPowerFail);
    }

    static public void RequestActivityPowerFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestActivityPowerSuccess(string text)
    {
        DEBUG.Log("RequestActivityPowerSuccess:text = " + text);
        DataCenter.SetData("RANKLIST_ACTIVITY_UI_WINDOW", "REFRESH_POWER", text);
    }
    //-------------------------------------------------
    // 排名活动pk请求
    //-------------------------------------------------
    static public void RequestActivityPeak()
    {
        CS_Ranklist_PvpList peak_rank = new CS_Ranklist_PvpList();
        HttpModule.Instace.SendGameServerMessage(peak_rank, RequestActivityPeakSuccess, RequestActivityPeakFail);
    }

    static public void RequestActivityPeakFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestActivityPeakSuccess(string text)
    {
        DEBUG.Log("RequestActivityPeakSuccess:text = " + text);
        DataCenter.SetData("RANKLIST_ACTIVITY_UI_WINDOW", "REFRESH_PEAK", text);
    }
    //-------------------------------------------------
    // 限时抢购请求
    //-------------------------------------------------
    static public void RequestLimitTimeSale()
    {
        CS_LimitTimeSale limittimesale = new CS_LimitTimeSale();
        limittimesale.pt = "CS_LimitTimeSale";
        HttpModule.Instace.SendGameServerMessage(limittimesale, RequestLimitTimeSaleSuccess, RequestLimitTimeSaleFail);
    }

    static public void RequestLimitTimeSaleFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestLimitTimeSaleSuccess(string text)
    {
        DEBUG.Log("RequestLimitTimeSaleSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_FLASH_SALE_WINDOW", "REFRESH_UI", text);
    }
    //-------------------------------------------------
    // 限时抢购抢购物品
    //-------------------------------------------------
    static public void RequestBuyCheapWares(int index)
    {
        CS_BuyCheapWares buycheapwares = new CS_BuyCheapWares();
        buycheapwares.pt = "CS_BuyCheapWares";
        buycheapwares.buyWareIndex = index;
        switch (index)
        {
            case 1: HttpModule.Instace.SendGameServerMessage(buycheapwares, RequestBuyCheapWaresSuccess1, RequestBuyCheapWaresFail);
                break;
            case 2: HttpModule.Instace.SendGameServerMessage(buycheapwares, RequestBuyCheapWaresSuccess2, RequestBuyCheapWaresFail);
                break;
            case 3: HttpModule.Instace.SendGameServerMessage(buycheapwares, RequestBuyCheapWaresSuccess3, RequestBuyCheapWaresFail);
                break;
        }
    }

    static public void RequestBuyCheapWaresFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        int errCode = 0;
        int.TryParse(text, out errCode);
        string txt = "";
        if (errCode == 1836)
        {
            txt = "/* 元宝不足 */";
        }
        DataCenter.OpenMessageWindow(txt);
        DEBUG.Log("RequestBuyCheapWaresErr:" + text);
        return;
    }
    static public void RequestBuyCheapWaresSuccess1(string text)
    {
        DEBUG.Log("RequestBuyCheapWaresSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_FLASH_SALE_WINDOW", "BUY1", text);
    }
    static public void RequestBuyCheapWaresSuccess2(string text)
    {
        DEBUG.Log("RequestBuyCheapWaresSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_FLASH_SALE_WINDOW", "BUY2", text);
    }
    static public void RequestBuyCheapWaresSuccess3(string text)
    {
        DEBUG.Log("RequestBuyCheapWaresSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_FLASH_SALE_WINDOW", "BUY3", text);
    }
    //-------------------------------------------------
    // 测试发送邮件string[]
    //-------------------------------------------------
    static public void RequestSendMail(string[] strTexts, string sTitle)
    {
        int iCount = strTexts.Length;
        ItemDataBase[] itemData = new ItemDataBase[iCount];
        for (int i = 0; i < iCount; i++)
        {
            itemData[i] = new ItemDataBase();
            itemData[i].tid = 0;
            itemData[i].itemNum = 0;
            itemData[i].itemId = -1;
        }

        for (int i = 0; i < iCount; i++)
        {
            string[] _textStr = strTexts[i].Split('*');
            itemData[i].tid = Convert.ToInt32(_textStr[0]);
            //			itemData[i].tid = Convert.ToInt32 (strTexts[i]);
            itemData[i].itemId = -1;
            if (_textStr.Length == 2)
            {
                itemData[i].itemNum = Convert.ToInt32(_textStr[1]);
            }
            else
            {
                if (itemData[i].tid >= 1000001 && itemData[i].tid < 1000011)
                {
                    itemData[i].itemNum = 10000;
                }
                else if (itemData[i].tid == 1000011)
                {
                    itemData[i].itemNum = 1000;
                }
                else if (itemData[i].tid <= 2010000 && itemData[i].tid > 1000011)
                {
                    itemData[i].itemNum = 1000;
                }
                else
                {
                    itemData[i].itemNum = 1;
                }
            }
        }

        CS_SendMail sendMail = new CS_SendMail();
        sendMail.pt = "CS_SendMail";
        sendMail.title = sTitle;
        sendMail.content = sTitle;

        sendMail.items = new ItemDataBase[iCount];
        for (int i = 0; i < iCount; i++)
        {
            sendMail.items[i] = itemData[i];
        }
        //		sendMail.items[] = itemData;
        HttpModule.Instace.SendGameServerMessage(sendMail, RequestSendMailSuccess, RequestSendMailFail);
    }

    static public void RequestSendMailFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestSendMailSuccess(string text)
    {
        DEBUG.Log("RequestSendMailSuccess:text = " + text);
        DataCenter.SetData("SEND_MAIL_WINDOW", "SET_BUTTON_STATE", true);

    }
    //-------------------------------------------------
    // 测试发送公会经验
    //-------------------------------------------------
    static public void RequestSendGuildExp(int iExp)
    {
        CS_AddGuildExp sendGuildExp = new CS_AddGuildExp();
        sendGuildExp.pt = "CS_AddGuildExp";
        sendGuildExp.exp = iExp;
        HttpModule.Instace.SendGameServerMessage(sendGuildExp, RequestSendGuildExpSuccess, RequestSendGuildExpFail);
    }

    static public void RequestSendGuildExpFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestSendGuildExpSuccess(string text)
    {
        DEBUG.Log("RequestSendGuildExpSuccess:text = " + text);
        DataCenter.SetData("SEND_MAIL_WINDOW", "SET_BUTTON_STATE", true);

    }

    //-------------------------------------------------
    // 角色上下阵
    //-------------------------------------------------
    static public void RequestTeamPosChangFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestTeamPosChangSuccess(TeamPosChangeData teamPosChangeData, string text)
    {
        DEBUG.Log("RequestBodyEquipChangSuccess:text = " + text);
        TeamPosChangeData item = JCode.Decode<TeamPosChangeData>(text);
        TeamManager.ChangeTeamPos(teamPosChangeData);
    }

    //-------------------------------------------------
    // 获取点星界面的功能 
    //-------------------------------------------------
    public static Coroutine StartWaitPointStarValue()
    {
        CS_PointStarMap pointStarMap = new CS_PointStarMap();
        pointStarMap.pt = "CS_PointStarMap";
        return StartWaitResp(pointStarMap, RequestPointStarValueSuccess, RequestPointStarValueFail);
    }

    static public void RequestPointStarValueFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestPointStarValueSuccess(string text)
    {
        DEBUG.Log("RequestPointStarValueSuccess:text = " + text);
        SC_PointStarMap item = JCode.Decode<SC_PointStarMap>(text);
        PointStarLogicData logic = new PointStarLogicData();
        logic.Init(item.currentIndex);
    }
    //-------------------------------------------------
    // 点击点亮按钮
    //-------------------------------------------------
    static public void RequestPointLightClick(int curPointStarIndex)
    {
        ConsumeItemLogicData consumeItemLogicData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
        if (null == consumeItemLogicData)
            return;

        ConsumeItemData consumeItemData = consumeItemLogicData.GetDataByTid((int)ITEM_TYPE.POINT_STAR_TONE);

        int iGetRewardsNum = 0;

        if (consumeItemData != null)
        {
            CS_PointLightenClick pointLightClick = new CS_PointLightenClick();
            pointLightClick.pt = "CS_PointLightenClick";
            ItemDataBase itemData = new ItemDataBase();
            itemData.itemId = consumeItemData.itemId;
            itemData.tid = consumeItemData.tid;
            itemData.itemNum = TableCommon.GetNumberFromPointStarConfig(curPointStarIndex, "COST");

            pointLightClick.itemArr = new ItemDataBase[iGetRewardsNum + 1];
            pointLightClick.itemArr[0] = itemData;
            //			breakUpgrade.consume = new ItemDataBase[breakSelfIcon + 1];
            HttpModule.Instace.SendGameServerMessage(pointLightClick, RequestPointLightClickSuccess, RequestPointLightClickFail);
        }
    }

    static public void RequestPointLightClickFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestPointLightClickSuccess(string text)
    {
        DEBUG.Log("RequestPointLightClickSuccess:text = " + text);

        //        DataCenter.SetData("ASTROLOGY_INFO_WINDOW", "SUCCESS_POINT_STAR", text);
        //        DataCenter.SetData("ASTROLOGY_WINDOW", "SUCCESS_POINT_STAR", text);
        DataCenter.SetData("ASTROLOGY_UI_WINDOW", "SUCCESS_POINT_STAR", text);
    }
    //-------------------------------------------------
    // 点击选择物品确定按钮
    //-------------------------------------------------
    static public void RequestPointStarRewards(ItemDataBase items)
    {
        ConsumeItemLogicData consumeItemLogicData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
        if (null == consumeItemLogicData)
            return;
        ConsumeItemData consumeItemData = consumeItemLogicData.GetDataByTid((int)ITEM_TYPE.POINT_STAR_TONE);
        int curPointStarIndex = PointStarLogicData.Self.mCurIndex;

        if (consumeItemData != null)
        {
            CS_PointLightenClick pointLightClick = new CS_PointLightenClick();
            pointLightClick.pt = "CS_PointLightenClick";
            ItemDataBase itemData = new ItemDataBase();
            itemData.itemId = consumeItemData.itemId;
            itemData.tid = consumeItemData.tid;
            itemData.itemNum = TableCommon.GetNumberFromPointStarConfig(curPointStarIndex, "COST");

            pointLightClick.itemArr = new ItemDataBase[2];
            pointLightClick.itemArr[0] = itemData;

            ItemDataBase petItemData = new ItemDataBase();
            petItemData.itemId = -1;
            petItemData.itemNum = items.itemNum;
            petItemData.tid = items.tid;
            pointLightClick.itemArr[1] = petItemData;

            HttpModule.Instace.SendGameServerMessage(pointLightClick, RequestPointStarRewardsSuccess, RequestPointStarRewardsFail);
        }
    }

    static public void RequestPointStarRewardsFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestPointStarRewardsSuccess(string text)
    {
        DEBUG.Log("RequestPointStarRewardsSuccess:text = " + text);

        //        DataCenter.SetData("ASTROLOGY_INFO_WINDOW", "SUCCESS_GET_REWARDS", text);
        //        DataCenter.SetData("ASTROLOGY_WINDOW", "SUCCESS_POINT_STAR", text);
        DataCenter.SetData("ASTROLOGY_UI_WINDOW", "SUCCESS_GET_REWARDS", text);
    }
    //-------------------------------------------------
    // 获取是否有月卡以及是否可领请求
    //-------------------------------------------------
    static public void RequestMonthCardQuery()
    {
        CS_MonthCardQuery haveMonthCard = new CS_MonthCardQuery();
        haveMonthCard.pt = "CS_MonthCardQuery";
        HttpModule.Instace.SendGameServerMessage(haveMonthCard, RequestMonthCardQuerySuccess, RequestMonthCardQueryFail);
    }

    static public void RequestMonthCardQueryFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestMonthCardQuerySuccess(string text)
    {
        DEBUG.Log("RequestMonthCardQuerySuccess:text = " + text);
        DataCenter.SetData("ACTIVE_DAILY_REWARD_FOR_MONTH_WINDOW", "HAVE_CARD", text);
    }

    //-------------------------------------------------
    // 获取月卡领取请求
    //-------------------------------------------------
    static public void RequestMonthCardReward(int kind)
    {
        CS_MonthCardReward getMonthCardReward = new CS_MonthCardReward();
        if (kind == 0)
        {
            getMonthCardReward.index = 1;
        }
        else
        {
            getMonthCardReward.index = 2;
        }
        getMonthCardReward.pt = "CS_MonthCardReward";
        HttpModule.Instace.SendGameServerMessage(getMonthCardReward, RequestGetMonthCardRewardSuccess, RequestGetMonthCardRewardFail);
    }

    static public void RequestGetMonthCardRewardFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }

    static public void RequestGetMonthCardRewardSuccess(string text)
    {
        DEBUG.Log("RequestGetMonthCardRewardSuccess:text = " + text);
        DataCenter.SetData("ACTIVE_DAILY_REWARD_FOR_MONTH_WINDOW", "GET_REWARD", text);
    }
    //-------------------------------------------------
    // 单充福利请求
    //-------------------------------------------------
    static public void RequestSingleWelfare()
    {
        CS_GetSingleRechargeInfo getSingleRechargeInfo = new CS_GetSingleRechargeInfo();
        getSingleRechargeInfo.pt = "CS_GetSingleRechargeInfo";
        HttpModule.Instace.SendGameServerMessage(getSingleRechargeInfo, RequestGetGetSingleRechargeInfoSuccess, RequestGetSingleRechargeInfoFail);
    }

    static public void RequestGetGetSingleRechargeInfoSuccess(string text)
    {
        DataCenter.SetData("ACTIVITY_SINGLE_WELFARE_WINDOW", "SINGLE_WELFARE", text);
    }

    static public void RequestGetSingleRechargeInfoFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }

    //-------------------------------------------------
    // 获取单充福利领取请求
    //-------------------------------------------------
    static public void RequestRevSingleRechargeReward(int kind)
    {
        CS_RevSingleRechargeReward RevSingleRechargeReward = new CS_RevSingleRechargeReward();

        RevSingleRechargeReward.index = kind + 1;
        
        RevSingleRechargeReward.pt = "CS_RevSingleRechargeReward";
        HttpModule.Instace.SendGameServerMessage(RevSingleRechargeReward, RevSingleRechargeRewardSuccess, RevSingleRechargeRewardFail);
    }

    static public void RevSingleRechargeRewardSuccess(string text)
    {
        DEBUG.Log("RequestGetMonthCardRewardSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_SINGLE_WELFARE_WINDOW", "GET_REWARD", text);
    }

    static public void RevSingleRechargeRewardFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }

    
    //-------------------------------------------------
    // 获取每日签到请求
    //-------------------------------------------------
    public static Coroutine StartWaitDailySignQuery()
    {
        CS_DailySignQuery dailySignQuery = new CS_DailySignQuery();
        dailySignQuery.pt = "CS_DailySignQuery";
        return StartWaitResp(dailySignQuery, RequestDailySignQuerySuccess, RequestDailySignQueryFail);
    }

    static public void RequestDailySignQueryFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestDailySignQuerySuccess(string text)
    {
        DEBUG.Log("RequestDailySignQuerySuccess:text = " + text);
        SC_DailySignQuery item = JCode.Decode<SC_DailySignQuery>(text);
        ActivitySignLogicData logic = new ActivitySignLogicData();
        logic.Init(item.signNum, item.isSign);
    }
    //-------------------------------------------------
    //  每日签到领取
    //-------------------------------------------------
    static public void RequestDailySignReward()
    {
        CS_DailySign dailySignReward = new CS_DailySign();
        dailySignReward.pt = "CS_DailySign";
        HttpModule.Instace.SendGameServerMessage(dailySignReward, RequestailySignRewardSuccess, RequestailySignRewardFail);
    }

    static public void RequestailySignRewardFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestailySignRewardSuccess(string text)
    {
        DEBUG.Log("RequestailySignRewardSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_SIGN_WINDOW", "SIGN_SUCCESS", text);
    }
    //-------------------------------------------------
    //  豪华签到请求
    //-------------------------------------------------
    static public void RequestLuxuryQueryd()
    {
        CS_LuxurySignQuery luxuryQuery = new CS_LuxurySignQuery();
        luxuryQuery.pt = "CS_LuxurySignQuery";
        HttpModule.Instace.SendGameServerMessage(luxuryQuery, RequestLuxuryQuerydSuccess, RequestLuxuryQuerydFail);
    }
    static public void RequestLuxuryQuerydFail(string text)
    {
    }
    static public void RequestLuxuryQuerydSuccess(string text)
    {
        DEBUG.Log("RequestLuxuryQuerydSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_SIGN_WINDOW", "CAN_GET", text);
    }

    //-------------------------------------------------
    //  豪华签到领取
    //-------------------------------------------------
    static public void RequestLuxuryReward()
    {
        CS_LuxurySign luxury = new CS_LuxurySign();
        luxury.pt = "CS_LuxurySign";
        HttpModule.Instace.SendGameServerMessage(luxury, RequestLuxuryRewardSuccess, RequestLuxuryRewardFail);
    }
    static public void RequestLuxuryRewardFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }
    static public void RequestLuxuryRewardSuccess(string text)
    {
        DEBUG.Log("RequestLuxuryQuerydSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_SIGN_WINDOW", "LUXURY_SUCCESS", text);
    }
    //-------------------------------------------------
    // 心跳包
    //-------------------------------------------------
    static public void HeartBeatSuccess(string text)
    {
        if (CommonParam.PingLogOnScreen)
        {
            GlobalModule.heartbeatstopwatch.Stop();
            ScreenLogger.Log("HeartBeat: " + GlobalModule.heartbeatstopwatch.ElapsedMilliseconds + " ms", LOG_LEVEL.GENERAL);
        }

        DEBUG.Log("HeartBeatSuccess:text = " + text);

        __AddLocalPush(PUSH_TYPE.LAST_LOGIN);
    }
    static public void HeartBeatFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    /// <summary>
    /// 添加推送
    /// </summary>
    /// <param name="pushiType">推送类型</param>
    /// <returns>是否添加成功</returns>
    static private bool __AddLocalPush(PUSH_TYPE pushiType)
    {
        return PushMessageManager.Self.AddLocalPush(pushiType, CommonParam.NowServerTime() + needLoginTime);
    }

    //-------------------------------------------------
    // 获取摇钱树请求
    //-------------------------------------------------
    static public void StartWaitShakeTreeQuery()
    {
        CS_TreeOfGold ShakeTreeQuery = new CS_TreeOfGold();
        ShakeTreeQuery.pt = "CS_TreeOfGold";
        HttpModule.Instace.SendGameServerMessage(ShakeTreeQuery, RequestShakeTreeQuerySuccess, RequestShakeTreeQueryFail);
    }

    static public void RequestShakeTreeQueryFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestShakeTreeQuerySuccess(string text)
    {
        DEBUG.Log("RequestShakeTreeQuerySuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_TREE_WINDOW", "SHAKE_TREE_QUERY", text);
    }
    //-------------------------------------------------
    // 摇钱树摇钱请求
    //-------------------------------------------------
    static public void RequstShakeTreeShake()
    {
        CS_ShakeTree ShakeTreeShake = new CS_ShakeTree();
        ShakeTreeShake.pt = "CS_ShakeTree";
        HttpModule.Instace.SendGameServerMessage(ShakeTreeShake, RequestShakeTreeShakeSuccess, RequestShakeTreeShakeFail);
    }

    static public void RequestShakeTreeShakeFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestShakeTreeShakeSuccess(string text)
    {
        DEBUG.Log("RequestShakeTreeShakeSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_TREE_WINDOW", "SHAKE_MONEY", text);
    }
    //-------------------------------------------------
    // 获取摇钱树已摇请求
    //-------------------------------------------------
    static public void ShakeTreeInfos()
    {
        CS_TreeOfGold shakeTreeInfos = new CS_TreeOfGold();
        shakeTreeInfos.pt = "CS_TreeOfGold";
        HttpModule.Instace.SendGameServerMessage(shakeTreeInfos, RequestShakeTreeInfosSuccess, RequestShakeTreeInfosFail);
    }

    static public void RequestShakeTreeInfosFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestShakeTreeInfosSuccess(string text)
    {
        DEBUG.Log("RequestShakeTreeInfosSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_TREE_UP_WINDOW", "SET_TREE_MESSAGE", text);
    }
    //-------------------------------------------------
    // 获取摇钱树额外奖励
    //-------------------------------------------------
    static public void RequstShakeTreeRewards()
    {
        CS_TreeExtraGold ShakeTreeRewards = new CS_TreeExtraGold();
        ShakeTreeRewards.pt = "CS_TreeExtraGold";
        HttpModule.Instace.SendGameServerMessage(ShakeTreeRewards, RequestShakeTreeRewardsySuccess, RequestShakeTreeRewardsFail);
    }

    static public void RequestShakeTreeRewardsFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestShakeTreeRewardsySuccess(string text)
    {
        DEBUG.Log("RequestShakeTreeRewardsySuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_TREE_UP_WINDOW", "GET_MONEY_MESSAGE", text);
    }

    //-------------------------------------------------
    //  获取领仙桃
    //-------------------------------------------------
    static public void RequstVitalityQuery()
    {
        CS_PowerQuery vitalityQuery = new CS_PowerQuery();
        vitalityQuery.pt = "CS_PowerQuery";
        HttpModule.Instace.SendGameServerMessage(vitalityQuery, RequestVitalityQuerySuccess, RequestVitalityQueryFail);
    }

    static public void RequestVitalityQueryFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestVitalityQuerySuccess(string text)
    {
        DEBUG.Log("RequestVitalityQuerySuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_VITALITY_WINDOW", "GET_VITALITY_MESSAGE", text);
    }
    //-------------------------------------------------
    //  获取领仙桃领取请求
    //-------------------------------------------------
    static public void RequstVitalityRewards(int strWhichCount)
    {
        CS_RewardPower vitalityRewards = new CS_RewardPower();
        vitalityRewards.pt = "CS_RewardPower";
        HttpModule.Instace.SendGameServerMessage(vitalityRewards, x => RequestVitalityRewardsSuccess(strWhichCount, x), RequestVitalityRewardsFail);
    }

    static public void RequestVitalityRewardsFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestVitalityRewardsSuccess(int strWhichCount, string text)
    {
        DEBUG.Log("RequestVitalityRewardsSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_VITALITY_WINDOW", "GET_VITALITY_REWARDS", strWhichCount);
    }
    //-------------------------------------------------
    //  获取射手乐园请求获取信息
    //-------------------------------------------------
    static public void RequstShooterParkInfo()
    {
        CS_ShootPark _shootParkRequest = new CS_ShootPark();
        HttpModule.Instace.SendGameServerMessage(_shootParkRequest, RequestShooterParkSuccess, RequestShooterParkFail);
    }

    static public void RequestShooterParkFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestShooterParkSuccess(string text)
    {
        DEBUG.Log("RequestShooterParkSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_SHOOTER_PARK_WINDOW", "REFRESH", text);
    }

    //-------------------------------------------------
    //  射手乐园射击请求
    //-------------------------------------------------
    static public void RequestShooterPark_Shoot()
    {
        CS_ShootParkShoot _request = new CS_ShootParkShoot();
        HttpModule.Instace.SendGameServerMessage(_request, RequestShooterParkShootSuccess, RequestShooterParkShootFail);
    }

    static public void RequestShooterParkShootFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }

    static public void RequestShooterParkShootSuccess(string text)
    {
        DEBUG.Log("RequestShooterParkShootSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_SHOOTER_PARK_WINDOW", "SHOOT", text);
    }

    //-------------------------------------------------
    //  幸运翻牌获取剩余次数请求
    //-------------------------------------------------
    static public void RequestLuckyCardLeftTimes()
    {
        CS_LuckyCard _request = new CS_LuckyCard();
        HttpModule.Instace.SendGameServerMessage(_request, RequestLuckyCardLeftTimesSuccess, RequestLuckyCardLeftTimesFail);
    }

    static public void RequestLuckyCardLeftTimesFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestLuckyCardLeftTimesSuccess(string text)
    {
        SC_LuckyCard _receive = JCode.Decode<SC_LuckyCard>(text);
        DataCenter.SetData("ACTIVITY_LUCKY_CARD_WINDOW", "REFRESH", _receive);
    }

    //-------------------------------------------------
    //  幸运翻牌开始翻牌请求
    //-------------------------------------------------
    static public void RequestLuckCardDraw()
    {
        CS_LuckyCard_Draw _request = new CS_LuckyCard_Draw();
        HttpModule.Instace.SendGameServerMessage(_request, RequestLuckyCardDrawSuccess, RequestLuckyCardDrawFail);
    }
    static public void RequestLuckyCardDrawFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestLuckyCardDrawSuccess(string text)
    {
        SC_LuckyCard_Draw _receive = JCode.Decode<SC_LuckyCard_Draw>(text);
        DataCenter.SetData("ACTIVITY_LUCKY_CARD_WINDOW", "START_DRAW", _receive);
    }
    //-------------------------------------------------
    //  获取充值送礼当前领取情况的请求
    //-------------------------------------------------
    static public void RequestRechargeGift()
    {
        CS_RechargeGiftQuery _request = new CS_RechargeGiftQuery();
        HttpModule.Instace.SendGameServerMessage(_request, RequestRechargeGiftSuccess, RequestRechargeGiftFail);
    }

    static public void RequestRechargeGiftFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestRechargeGiftSuccess(string text)
    {
        SC_RechargeGiftQuery _receive = JCode.Decode<SC_RechargeGiftQuery>(text);
        DataCenter.SetData("ACTIVITY_RECHARGE_GIFT_WINDOW", "SET_RECHARGE_GIFT_INFO", _receive);
    }
    //-------------------------------------------------
    //  领取充值送礼的奖励
    //-------------------------------------------------
    static public void RequestGetRechargeGift(int kRmbNum)
    {
        CS_GetRechargeGift _request = new CS_GetRechargeGift();
        _request.rmbNum = kRmbNum;
        HttpModule.Instace.SendGameServerMessage(_request, RequestGetRechargeGiftSuccess, RequestGetRechargeGiftFail);
    }
    static public void RequestGetRechargeGiftFail(string text)
    {
        //test
        //SC_GetRechargeGift _receive = new SC_GetRechargeGift() { };
        //ItemDataBase[] items = new ItemDataBase[1];
        //items[0] = new ItemDataBase();
        //items[0].itemId = -1;
        //items[0].tid = 1000011;
        //items[0].itemNum = 2000;
        //_receive.awardArr = items;
        //DataCenter.SetData("ACTIVITY_RECHARGE_GIFT_WINDOW", "GET_RECHARGE_GIFT", _receive);
        //return;
        //
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestGetRechargeGiftSuccess(string text)
    {
        SC_GetRechargeGift _receive = JCode.Decode<SC_GetRechargeGift>(text);
        DataCenter.SetData("ACTIVITY_RECHARGE_GIFT_WINDOW", "GET_RECHARGE_GIFT", _receive);
    }
    //-------------------------------------------------
    //  获取累计消费奖励领取情况的的请求
    //-------------------------------------------------
    static public void RequestCumulative()
    {
        CS_CumulativeQuery _request = new CS_CumulativeQuery();
        HttpModule.Instace.SendGameServerMessage(_request, RequestCumulativeSuccess, RequestCumulativeFail);
    }
    static public void RequestCumulativeFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestCumulativeSuccess(string text)
    {
        SC_CumulativeQuery _receive = JCode.Decode<SC_CumulativeQuery>(text);
        DataCenter.SetData("ACTIVITY_CUMULATIVE_WINDOW", "SET_CUMULATIVE_GIFT_INFO", _receive);
    }
    //-------------------------------------------------
    //  领取累计消费奖励的的请求
    //-------------------------------------------------
    static public void RequestGetCumulativeGift(int kDiamondNum)
    {
        CS_GetCumulativeGift _request = new CS_GetCumulativeGift();
        _request.clickItemMoney = kDiamondNum;
        HttpModule.Instace.SendGameServerMessage(_request, RequestGetCumulativeGiftSuccess, RequestGetCumulativeGiftFail);
    }
    static public void RequestGetCumulativeGiftFail(string text)
    {
        //test
        SC_GetCumulativeGift _receive = new SC_GetCumulativeGift() { };
        ItemDataBase[] items = new ItemDataBase[1];
        items[0] = new ItemDataBase();
        items[0].itemId = -1;
        items[0].tid = 1000011;
        items[0].itemNum = 2000;
        _receive.prizeContent = items;
        DataCenter.SetData("ACTIVITY_CUMULATIVE_WINDOW", "GET_CUMULATIVE_GIFT", _receive);
        return;
        //
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestGetCumulativeGiftSuccess(string text)
    {
        SC_GetCumulativeGift _receive = JCode.Decode<SC_GetCumulativeGift>(text);
        DataCenter.SetData("ACTIVITY_CUMULATIVE_WINDOW", "GET_CUMULATIVE_GIFT", _receive);
    }
    //-------------------------------------------------
    //  获取首充礼包状态的请求
    //-------------------------------------------------
    static public void RequestFirstCharge()
    {
        CS_FirstChargeQuery _request = new CS_FirstChargeQuery();
        HttpModule.Instace.SendGameServerMessage(_request, RequestFirstChargeSuccess, RequestFirstChargeFail);
    }

    static public void RequestFirstChargeFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestFirstChargeSuccess(string text)
    {
        SC_FirstChargeQuery _receive = JCode.Decode<SC_FirstChargeQuery>(text);
		DEBUG.Log("支付完成内部实现333---------");
        DataCenter.SetData("ACTIVITY_FIRST_RECHARGE_WINDOW", "SET_FIRST_RECHARGE_INFO", _receive);
    }
    //-------------------------------------------------
    //  领取首充礼包奖励的请求
    //-------------------------------------------------

    static public void RequestGetFirstRechargeGift()
    {
        CS_GetFirstChargeReward _request = new CS_GetFirstChargeReward();
        HttpModule.Instace.SendGameServerMessage(_request, RequestGetFirstRechargeGiftSuccess, RequestGetFirstRechargeGiftFail);
    }
    static public void RequestGetFirstRechargeGiftFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestGetFirstRechargeGiftSuccess(string text)
    {
        SC_GetFirstChargeReward _receive = JCode.Decode<SC_GetFirstChargeReward>(text);
		DEBUG.Log("支付完成内部实现4444---------");
        DataCenter.SetData("ACTIVITY_FIRST_RECHARGE_WINDOW", "GET_REWARD", _receive);
    }
    //-------------------------------------------------
    //  获取活动结束时间的请求
    //-------------------------------------------------
    static public void RequestActivitiesEndTime()
    {
        CS_GetActivitiesEndTime _request = new CS_GetActivitiesEndTime();
        HttpModule.Instace.SendGameServerMessage(_request, RequestActivitiesEndTimeSuccess, RequestActivitiesEndTimeFail);
    }
    static public void RequestActivitiesEndTimeFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }
    static public void RequestActivitiesEndTimeSuccess(string text)
    {
        SC_GetActivitiesEndTime _receive = JCode.Decode<SC_GetActivitiesEndTime>(text);
        //回调活动界面刷新
        DataCenter.Set("OPEN_TIME", _receive);
    }
    //-------------------------------------------------
    //  获取VIP礼包请求
    //-------------------------------------------------
    //每日
    static public void RequstVIPGiftQuery()
    {
        CS_VIPDailyInfoQuery vipGiftQuery = new CS_VIPDailyInfoQuery();
        vipGiftQuery.pt = "CS_VIPDailyInfo";
        HttpModule.Instace.SendGameServerMessage(vipGiftQuery, RequestVIPGiftQuerySuccess, RequestVIPGiftQueryFail);
    }

    static public void RequestVIPGiftQueryFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestVIPGiftQuerySuccess(string text)
    {
        DEBUG.Log("RequestVIPGiftQuerySuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_WELFARE_WINDOW", "VIP_GIFT_QUERY", text);
    }
    //每周
    static public void RequestVIPWeekGiftQuery(Action kAction)
    {
        CS_VIPWeeklyInfoQuery vipGiftQuery = new CS_VIPWeeklyInfoQuery();
        vipGiftQuery.pt = "CS_VIPWeeklyInfo";
        HttpModule.Instace.SendGameServerMessage(vipGiftQuery, (text) =>
        {
            RequestVIPWeekQuerySuccess(text);
            if (kAction != null) 
            {
                kAction.Invoke();              
            }
        },
        RequestVIPWeekQueryFail);
    }

    static public void RequestVIPWeekQuerySuccess(string text)
    {
        DataCenter.SetData("ACTIVITY_WELFARE_WINDOW", "VIP_WEEK_GIFT_QUERY", text);
    }

    static public void RequestVIPWeekQueryFail(string text) 
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }


    //-------------------------------------------------
    //  获取VIP每日福利
    //-------------------------------------------------

    static public void RequstVIPDailyWelfare(int viplevel)
    {
        CS_VIPDaily vipDailyWelfare = new CS_VIPDaily();
        vipDailyWelfare.pt = "CS_VIPDaily";
        vipDailyWelfare.vipLevel = viplevel;
        HttpModule.Instace.SendGameServerMessage(vipDailyWelfare, RequestVIPDailyWelfareSuccess, RequestVIPDailyWelfareFail);
    }

    static public void RequestVIPDailyWelfareFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestVIPDailyWelfareSuccess(string text)
    {
        DEBUG.Log("RequestVIPDailyWelfareSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_WELFARE_WINDOW", "VIP_DAILY_WELFARE", text);
    }
    //-------------------------------------------------
    //  获取VIP每周福利
    //-------------------------------------------------
    static public void RequstVIPWeekWelfare(int iBuyVipIndex)
    {
        CS_VIPWeek vipWeekWelfare = new CS_VIPWeek();
        vipWeekWelfare.pt = "CS_VIPWeek";
        vipWeekWelfare.index = iBuyVipIndex;
        HttpModule.Instace.SendGameServerMessage(vipWeekWelfare, RequestVIPWeekWelfareSuccess, RequestVIPWeekWelfareFail);
    }

    static public void RequestVIPWeekWelfareFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestVIPWeekWelfareSuccess(string text)
    {
        DEBUG.Log("RequestVIPWeekWelfareSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_WELFARE_WINDOW", "VIP_WEEK_WELFARE", text);
    }

    //-------------------------------------------------
    //  礼品码兑换
    //-------------------------------------------------
    static public void RequestGiftCode(string strCode)
    {
        CS_GiftCode giftCode = new CS_GiftCode();
        giftCode.pt = "CS_GiftCode";
        giftCode.giftCode = strCode;
        HttpModule.Instace.SendGameServerMessage(giftCode, RequestailyGiftCodeSuccess, RequestailyGiftCodeFail);
    }

    static public void RequestailyGiftCodeFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        //		if (!string.IsNullOrEmpty(text))
        //		{
        //			int errorIndex = Convert.ToInt32(text);
        //			STRING_INDEX error = (STRING_INDEX)errorIndex;
        //			DataCenter.OpenMessageWindow(error, true);
        //		}
        DataCenter.SetData("ACTIVITY_GIFT_WINDOW", "SET_BUTTON_STATE", true);
        return;
    }

    static public void RequestailyGiftCodeSuccess(string text)
    {
        DEBUG.Log("RequestailyGiftCodeSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_GIFT_WINDOW", "GET_GIFT_SUCCESS", text);
        DataCenter.SetData("ACTIVITY_GIFT_WINDOW", "SET_BUTTON_STATE", true);
    }

    //by chenliang
    //begin

    /// <summary>
    /// 是否可通过手机网络连接
    /// </summary>
    /// <returns></returns>
    public static bool HasCarrierDataNetConnectable()
    {
        return (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork);
    }
    /// <summary>
    /// 是否可通过Wifi网络连接
    /// </summary>
    /// <returns></returns>
    public static bool HasWifiNetConnectable()
    {
        return (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork);
    }
    /// <summary>
    /// 是否有可连接的网络
    /// </summary>
    /// <returns></returns>
    public static bool HasAnyNetConnectable()
    {
        return (
            HasCarrierDataNetConnectable() ||
            HasWifiNetConnectable());
    }
    /// <summary>
    /// 打开网络不可连接窗口
    /// </summary>
    /// <param name="reconnectCallback">重连时回调函数，如果传空，调用默认重连函数NetManager.ReConnect()</param>
    public static void OpenNoNetConnectableWindow(Action reconnectCallback)
    {
        OpenNoNetConnectableWindow("", reconnectCallback);
    }
    /// <summary>
    /// 打开网络不可连接窗口
    /// </summary>
    /// <param name="info">窗口显示内容，如果传空显示为“网络不给力，数据请求失败，请尝试重新连接”</param>
    /// <param name="reconnectCallback">重连时回调函数，如果传空，调用默认重连函数NetManager.ReConnect()</param>
    public static void OpenNoNetConnectableWindow(string info, Action reconnectCallback)
    {
        GlobalModule.NeedCheckNetConnectable = false;

        MessageBoxExOpenData tmpOpenData = new MessageBoxExOpenData()
        {
            Content = (info == "" ? "网络不给力，数据请求失败，请尝试重新连接" : info),
            ButtonType = MESSAGE_BOX_EX_BUTTON_TYPE.OK,
            BtnOkInfo = "重新连接",
            BtnOkCallback = () =>
            {
                if (reconnectCallback != null)
                    reconnectCallback();
                else
                    NetManager.ReConnect();
            }
        };
        DataCenter.OpenWindow("MESSAGE_BOX_EX_WINDOW", tmpOpenData);
    }

    //end

    //-------------------------------------------------
    // 获取次日登陆请求
    //-------------------------------------------------
    public static Coroutine StartWaitMorrowLandQuery()
    {
        CS_LoginRewardQuery morrowLandQuery = new CS_LoginRewardQuery();
        morrowLandQuery.pt = "CS_LoginRewardQuery";
        return StartWaitResp(morrowLandQuery, RequestMorrowLandQuerySuccess, RequestMorrowLandQueryFail);
    }

    static public void RequestMorrowLandQueryFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestMorrowLandQuerySuccess(string text)
    {
        DEBUG.Log("RequestMorrowLandQuerySuccess:text = " + text);
        MorrowLandLogicData.Self.Init(text);
    }

    //-------------------------------------------------
    //  获取次日登陆奖励
    //-------------------------------------------------
    static public void RequestMorrowLandRewards()
    {
        CS_LoginReward morrowLandRewards = new CS_LoginReward();
        morrowLandRewards.pt = "CS_LoginReward";
        HttpModule.Instace.SendGameServerMessage(morrowLandRewards, RequesMorrowLandRewardsSuccess, RequesMorrowLandRewardsFail);
    }

    static public void RequesMorrowLandRewardsFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }

    static public void RequesMorrowLandRewardsSuccess(string text)
    {
        DEBUG.Log("RequesMorrowLandRewardsSuccess:text = " + text);
        DataCenter.SetData("MORROW_LAND_WINDOW", "GET_REWARDS", text);
    }
    //-------------------------------------------------
    //  获得开服狂欢列表
    //-------------------------------------------------
    //	public static Coroutine RequestGetRevelryList() 
    //	{
    //		CS_GetRevelryList moGetRevelryList = new CS_GetRevelryList();
    //		moGetRevelryList.pt = "CS_GetRevelryList";
    //		return StartWaitResp(moGetRevelryList, RequesGetRevelryListSuccess, RequesGetRevelryListFail);
    //	}
    //	
    //	static public void RequesGetRevelryListFail(string text) 
    //	{
    //		if (string.IsNullOrEmpty(text))
    //			return;
    //		return;
    //	}
    //	
    //	static public void RequesGetRevelryListSuccess(string text) 
    //	{
    //		DEBUG.Log("RequesGetRevelryListSuccess:text = " + text);
    ////		RevelryLogicData.Self.Init(text);
    //	}
    static public void RequestGetRevelryList()
    {
        CS_GetRevelryList moGetRevelryList = new CS_GetRevelryList();
        moGetRevelryList.pt = "CS_GetRevelryList";
        HttpModule.Instace.SendGameServerMessage(moGetRevelryList, RequesGetRevelryListSuccess, RequesGetRevelryListFail);
    }

    static public void RequesGetRevelryListFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }

    static public void RequesGetRevelryListSuccess(string text)
    {
        DEBUG.Log("RequesGetRevelryListSuccess:text = " + text);
        SC_GetRevelryList item = JCode.Decode<SC_GetRevelryList>(text);

        CS_HalfPriceQuery mHalfPriceQuer = new CS_HalfPriceQuery();
        mHalfPriceQuer.pt = "CS_HalfPriceQuery";

        HttpModule.Instace.SendGameServerMessage(mHalfPriceQuer, (string textHalf) =>
        {
            SC_HalfPriceQuery itemHalf = JCode.Decode<SC_HalfPriceQuery>(textHalf);
            //刷新数据以后再刷新界面
            SC_GetRevelryList itemNew = SevenDaysCarnivalWindow.UpdateRevelryListByHalfQuery(itemHalf, item);
            DataCenter.SetData("SEVEN_DAYS_CARNIVAL_WINDOW", "GET_REVELRY_LIST", item);
        },
        (string textHalf) =>
        {
            if (string.IsNullOrEmpty(textHalf))
                return;
            return;
        }
        );
    }
    //-------------------------------------------------
    //  领取开服狂欢奖励
    //-------------------------------------------------
    static public void RequestTakeRevelryAward(int iRevelryId)
    {
        CS_TakeRevelryAward mTakeRevelryAward = new CS_TakeRevelryAward();
        mTakeRevelryAward.pt = "CS_TakeRevelryAward";
        mTakeRevelryAward.revelryId = iRevelryId;
        HttpModule.Instace.SendGameServerMessage(mTakeRevelryAward, RequesTakeRevelryAwardSuccess, RequesTakeRevelryAwardFail);
    }

    static public void RequesTakeRevelryAwardFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }

    static public void RequesTakeRevelryAwardSuccess(string text)
    {
        DEBUG.Log("RequesTakeRevelryAwardSuccess:text = " + text);
        DataCenter.SetData("SEVEN_DAYS_CARNIVAL_WINDOW", "GET_REVELRY_REWARDS_SUCCESS", text);
    }

    //-------------------------------------------------------------------------
    // 开服狂欢-半价抢购-初始化
    //-------------------------------------------------------------------------
    static public void RequestHalfPriceQuery()
    {
        CS_HalfPriceQuery mHalfPriceQuer = new CS_HalfPriceQuery();
        mHalfPriceQuer.pt = "CS_HalfPriceQuery";
        HttpModule.Instace.SendGameServerMessage(mHalfPriceQuer, RequestHalfPriceQuerySuccess, RequestHalfPriceQueryFail);
    }

    static public void RequestHalfPriceQueryFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }

    static public void RequestHalfPriceQuerySuccess(string text)
    {
        DEBUG.Log("RequesTakeRevelryAwardSuccess:text = " + text);
        DataCenter.SetData("SEVEN_DAYS_CARNIVAL_WINDOW", "REFRESH_HALF_PRICE_WINDOW", text);
    }

    //-------------------------------------------------------------------------
    // 开服狂欢-半价抢购-抢购
    //-------------------------------------------------------------------------
    static public void RequestHalfPrice(int whichday)
    {
        CS_HalfPrice mHalfPrice = new CS_HalfPrice();
        mHalfPrice.pt = "CS_HalfPrice";
        mHalfPrice.whichDay = whichday;
        HttpModule.Instace.SendGameServerMessage(mHalfPrice, RequestHalfPriceSuccess, RequestHalfPriceFail);
    }

    static public void RequestHalfPriceFail(string text)
    {
        if (int.Parse(text) == 154)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.HALF_PRICE_GET_BUYED);
            return;
        }
        if (int.Parse(text) == 156)
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.HALF_PRICE_NO_NUM);
            return;
        }
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }

    static public void RequestHalfPriceSuccess(string text)
    {
        DEBUG.Log("RequesHalfPriceSuccess:text = " + text);
        DataCenter.SetData("SEVEN_DAYS_CARNIVAL_WINDOW", "REFRESH_HALF_PRICE_BUY_WINDOW", text);
    }

    //-------------------------------------------------
    //  关卡重置
    //-------------------------------------------------
    public static int mCurBattleID = 0;
    static public void RequestResetBattleTimes(int kBattleID, int kNextResetTimes)
    {
        mCurBattleID = kBattleID;
        CS_ResetBattleMain _request = new CS_ResetBattleMain();
        _request.battleId = kBattleID;
        _request.resetCnt = kNextResetTimes;
        HttpModule.Instace.SendGameServerMessage(_request, RequestRequestBattleTimesSuccess, RequestResetBattleTimesFail);
    }

    static public void RequestResetBattleTimesFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        return;
    }

    static public void RequestRequestBattleTimesSuccess(string text)
    {
        MapData _mapData = MapLogicData.Instance.GetMapDataByStageIndex(mCurBattleID);
        _mapData.todayReset++;
        _mapData.todaySuccess = 0;
        int _costNum = (int)DataCenter.mResetCost.GetRecord(_mapData.todayReset).getObject("COST_NUM");
        RoleLogicData.Self.AddDiamond(-_costNum);
        DataCenter.SetData("INFO_GROUP_WINDOW", "UPDATE_DIAMOND", null);
        DataCenter.SetData("STAGE_INFO_WINDOW", "REFRESH_RESET_INFO", null);
        DataCenter.CloseWindow("ADVENTURE_RESET_WINDOW");

    }

    //-------------------------------------------------
    //  获取 开服基金请求 
    //-------------------------------------------------
    static public void RequstActivityFundQuery()
    {
        CS_FundQuery activityFundQuery = new CS_FundQuery();
        activityFundQuery.pt = "CS_FundQuery";
        HttpModule.Instace.SendGameServerMessage(activityFundQuery, RequestActivityFundQuerySuccess, RequestActivityFundQueryFail);
    }

    static public void RequestActivityFundQueryFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestActivityFundQuerySuccess(string text)
    {
        DEBUG.Log("RequestActivityFundQuerySuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_FUND_WINDOW", "ACTIVITY_FUND_QUERY", text);
    }
    //-------------------------------------------------
    //  获取 开服基金购买 
    //-------------------------------------------------
    static public void RequstActivityFundPurchase()
    {
        CS_FundPurchase activityFundPurchase = new CS_FundPurchase();
        activityFundPurchase.pt = "CS_FundPurchase";
        HttpModule.Instace.SendGameServerMessage(activityFundPurchase, RequestActivityFundPurchaseSuccess, RequestActivityFundPurchaseFail);
    }

    static public void RequestActivityFundPurchaseFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestActivityFundPurchaseSuccess(string text)
    {
        DEBUG.Log("RequestActivityFundPurchaseSuccess:text = " + text);
        SC_FundPurchase item = JCode.Decode<SC_FundPurchase>(text);
        DataCenter.SetData("ACTIVITY_FUND_WINDOW", "ACTIVITY_FUND_PURCHASE", item.isBuySuccess);
    }

    //-------------------------------------------------
    //  获取 开服基金领取  
    //-------------------------------------------------
    static public void RequstActivityFundReward(int iIndex)
    {
        CS_FundReward activityFundReward = new CS_FundReward();
        activityFundReward.pt = "CS_FundReward";
        activityFundReward.index = iIndex;
        HttpModule.Instace.SendGameServerMessage(activityFundReward, x => RequestActivityFundRewardSuccess(iIndex, x), RequestActivityFundRewardFail);
    }

    static public void RequestActivityFundRewardFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestActivityFundRewardSuccess(int iIndex, string text)
    {
        DEBUG.Log("RequestActivityFundRewardSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_FUND_WINDOW", "ACTIVITY_FUND_REWARDS", iIndex);
    }
    //-------------------------------------------------
    //  获取 全民福利领取   
    //-------------------------------------------------
    static public void RequstActivityWelfareReward(int iIndex)
    {
        CS_WelfareReward activityWelfareReward = new CS_WelfareReward();
        activityWelfareReward.pt = "CS_WelfareReward";
        activityWelfareReward.index = iIndex;
        HttpModule.Instace.SendGameServerMessage(activityWelfareReward, x => RequestActivityWelfareRewardSuccess(iIndex, x), RequestActivityWelfareRewardFail);
    }
    static public void RequestActivityWelfareRewardFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }
    static public void RequestActivityWelfareRewardSuccess(int iIndex, string text)
    {
        DEBUG.Log("RequestActivityWelfareRewardSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_FUND_WINDOW", "ACTIVITY_WELFARE_REWARDS", text);
        DataCenter.SetData("ACTIVITY_FUND_WINDOW", "ACTIVITY_WELFARE_INDEX", iIndex);
    }
    //-------------------------------------------------
    //  获取 七日登陆 请求 
    //-------------------------------------------------
    static public void RequstActivitySevenDayLoginQuery()
    {
        CS_SevenDayLoginQuery activitySevenDayLoginQuery = new CS_SevenDayLoginQuery();
        activitySevenDayLoginQuery.pt = "CS_SevenDayLoginQuery";
        HttpModule.Instace.SendGameServerMessage(activitySevenDayLoginQuery, RequestActivitySevenDayLoginQuerySuccess, RequestActivitySevenDayLoginQueryFail);
    }

    static public void RequestActivitySevenDayLoginQueryFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestActivitySevenDayLoginQuerySuccess(string text)
    {
        DEBUG.Log("RequestActivitySevenDayLoginQuerySuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_SEVEN_DAY_LOGIN_WINDOW", "ACTIVITY_SEVEN_DAY_LOGIN_QUERY", text);
    }
    //-------------------------------------------------
    //  获取 七日登陆 领取   
    //-------------------------------------------------
    static public void RequstActivitySevenDayLoginReward(int iIndex)
    {
        CS_SevenDayLoginReward activitySevenDayLoginReward = new CS_SevenDayLoginReward();
        activitySevenDayLoginReward.pt = "CS_SevenDayLoginReward";
        activitySevenDayLoginReward.index = iIndex;
        HttpModule.Instace.SendGameServerMessage(activitySevenDayLoginReward, x => RequestActivitySevenDayLoginRewardSuccess(iIndex, x), RequestActivitySevenDayLoginRewardFail);
    }
    static public void RequestActivitySevenDayLoginRewardFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }
    static public void RequestActivitySevenDayLoginRewardSuccess(int iIndex, string text)
    {
        DEBUG.Log("RequestActivitySevenDayLoginRewardSuccess:text = " + text);
        DataCenter.SetData("ACTIVITY_SEVEN_DAY_LOGIN_WINDOW", "ACTIVITY_SEVEN_DAY_LOGIN_REWARDS", text);
        DataCenter.SetData("ACTIVITY_SEVEN_DAY_LOGIN_WINDOW", "ACTIVITY_SEVEN_DAY_LOGIN_INDEX", iIndex);
    }
    //-------------------------------------------------
    //  gm超级账号工具
    //-------------------------------------------------
    static public void RequestGmSuperAccount(SuperPlayerInfo superPlayerInfo, SuperPetInfo[] superPetInfo, SuperEquipInfo[] superEquipInfo, int stageId)
    {
        CS_CreateSuperPlayer createSuperPlayer = new CS_CreateSuperPlayer();
        createSuperPlayer.superPlayerInfo = superPlayerInfo;
        createSuperPlayer.superPetInfo = superPetInfo;
        createSuperPlayer.superEquipInfo = superEquipInfo;
        createSuperPlayer.stageId = stageId;
        HttpModule.Instace.SendGameServerMessage(createSuperPlayer, RequestGmSuperAccountSuccess, RequestGmSuperAccountFail);
    }

    static public void RequestGmSuperAccountFail(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        return;
    }

    static public void RequestGmSuperAccountSuccess(string text)
    {
        DEBUG.Log("RequestEquipStrengthenSuccess:text = " + text);
        GmWindow.UsedGM = CommonParam.mUId.ToString();
        DataCenter.SetData("GM_WINDOW", "CREATE_SUCCESS", null);
        LoginNet.RequestRoleData(); ;
    }

	//-------------------------------------------------
	//  获取天魔列表
	//-------------------------------------------------
	static public void RequestGetAppearBossList()
	{
		CS_Boss_GetDemonBossList bossList = new CS_Boss_GetDemonBossList();
//		bossList.pt = "CS_Boss_GetDemonBossList";
		HttpModule.Instace.SendGameServerMessage(bossList, RequestGetAppearBossListSuccess, RequestGetAppearBossListFail);
	}	
	static public void RequestGetAppearBossListFail(string text)
	{
		if (string.IsNullOrEmpty(text))
			return;
		
		return;
	}	
	static public void RequestGetAppearBossListSuccess(string text)
	{
		DEBUG.Log("RequestGetAppearBossListSuccess:text = " + text);
		SC_Boss_GetDemonBossList retBossList = JCode.Decode<SC_Boss_GetDemonBossList>(text);
		if (retBossList != null)
		{
			DataCenter.SetData("SCROLL_WORLD_MAP_CHAPTER_NAME_WINDOW", "APPEAR_BOSS_DATA", retBossList.arr);
		}
	}
}