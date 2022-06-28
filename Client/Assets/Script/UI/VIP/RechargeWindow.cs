using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

/// <summary>
/// 充值支付数据
/// </summary>
public class RechargePayData : ISDKPayData
{
    private string mGoodChannelID = "0";
    private string mExtraData = "";

    /// <summary>
    /// 商品名称
    /// </summary>
    public string GoodName { set; get; }
    /// <summary>
    /// 商品描述
    /// </summary>
    public string GoodDesc { set; get; }
    /// <summary>
    /// 商品支付价格
    /// </summary>
    public string GoodRealPrice { set; get; }
    /// <summary>
    /// 商品数量
    /// </summary>
    public string GoodCount { set; get; }
    /// <summary>
    /// 内部订单号
    /// </summary>
    public string BillInnerNo { set; get; }
    /// <summary>
    /// 商品在渠道商的ID
    /// </summary>
    public string GoodChannelID
    {
        set { mGoodChannelID = value; }
        get { return mGoodChannelID; }
    }
    /// <summary>
    /// 额外参数
    /// </summary>
    public string ExtraData
    {
        set { mExtraData = value; }
        get { return mExtraData; }
    }
}

class RechargeSimpleShowData
{
    private int mIdx = -1;                  //充值记录索引
    private bool mIsFirstRecharge = false;  //是否为首充
    private long mOverTime = -1;            //月卡结束时间

    public int Index
    {
        set { mIdx = value; }
        get { return mIdx; }
    }
    public bool IsFirstRecharge
    {
        set { mIsFirstRecharge = value; }
        get { return mIsFirstRecharge; }
    }
    public long OverTime
    {
        set { mOverTime = value; }
        get { return mOverTime; }
    }
}
/// <summary>
/// 充值显示数据
/// </summary>
class RechargeShowData
{
    private bool mIsMonthCard = false;      //是否为月卡
    private int mIdx = -1;                  //充值记录索引
    private int mPrice = 0;                 //价格
    private List<ItemDataBase> mListFirstRechargeAward = new List<ItemDataBase>();  //首充奖励
    private List<ItemDataBase> mListBuyAward = new List<ItemDataBase>();            //购买奖励
    private List<ItemDataBase> mListSendAward = new List<ItemDataBase>();           //赠送奖励
    private DataRecord mRecord;             //数据记录
    private bool mIsFirstRecharge = false;  //是否为首充
    private long mOverTime = -1;            //月卡结束时间

    public bool IsMonthCard
    {
        set { mIsMonthCard = value; }
        get { return mIsMonthCard; }
    }
    public int Index
    {
        set { mIdx = value; }
        get { return mIdx; }
    }
    public int Price
    {
        set { mPrice = value; }
        get { return mPrice; }
    }
    public List<ItemDataBase> ListFirstRechargeAward
    {
        set { mListFirstRechargeAward = value; }
        get { return mListFirstRechargeAward; }
    }
    public List<ItemDataBase> ListBuyAward
    {
        set { mListBuyAward = value; }
        get { return mListBuyAward; }
    }
    public List<ItemDataBase> ListSendAward
    {
        set { mListSendAward = value; }
        get { return mListSendAward; }
    }
    public DataRecord Record
    {
        set { mRecord = value; }
        get { return mRecord; }
    }
    public bool IsFirstRecharge
    {
        set { mIsFirstRecharge = value; }
        get { return mIsFirstRecharge; }
    }
    public long OverTime
    {
        set { mOverTime = value; }
        get { return mOverTime; }
    }

    public bool IsEqualsSimpleData(RechargeSimpleShowData simpleShowData)
    {
        if (simpleShowData == null)
            return false;

        return (
            mIdx == simpleShowData.Index    ||
            mIsFirstRecharge == simpleShowData.IsFirstRecharge  ||
            mOverTime == simpleShowData.OverTime);
    }
    public RechargeSimpleShowData CloneSimpleData()
    {
        RechargeSimpleShowData tmpShowData = new RechargeSimpleShowData()
        {
            Index = mIdx,
            IsFirstRecharge = mIsFirstRecharge,
            OverTime = mOverTime
        };
        return tmpShowData;
    }

    public static List<RechargeSimpleShowData> ToSimpleShowData(List<RechargeShowData> rechargeShowData)
    {
        if (rechargeShowData == null)
            return null;

        List<RechargeSimpleShowData> tmpSimpleShowData = new List<RechargeSimpleShowData>();
        for (int i = 0, count = rechargeShowData.Count; i < count; i++)
            tmpSimpleShowData.Add(rechargeShowData[i].CloneSimpleData());
        return tmpSimpleShowData;
    }
    public static bool IsEquals(List<RechargeShowData> showData, List<RechargeSimpleShowData> simpleShowData)
    {
        if (showData == null || simpleShowData == null)
            return false;
        if (showData.Count != simpleShowData.Count)
            return false;

        bool tmpEquals = true;
        for (int i = 0, count = showData.Count; i < count; i++)
        {
            if (!showData[i].IsEqualsSimpleData(simpleShowData[i]))
            {
                tmpEquals = false;
                break;
            }
        }
        return tmpEquals;
    }
}

/// <summary>
/// 充值界面
/// </summary>
public class RechargeWindow : VIPWindowBase
{
    private List<RechargeShowData> mListShowData = new List<RechargeShowData>();            //显示数据
    private List<RechargeShowData> mListImmediateShowData = new List<RechargeShowData>();   //直充显示数据
    private int mMonthCardCount = 0;
    private UIGridContainer mRechargeGrids;
    private Dictionary<int, string> mDicImmediateTitle = new Dictionary<int, string>();     //直充标题
    private int mCurrRechargeIdx = -1;      //当前充值数据索引

    private const float mRechargeTimeOut = 4.0f * 60.0f;      //充值超时
    private IEnumerator mIEnumRechargeTimeOut = null;

    private List<RechargeSimpleShowData> mListShowDataSimpleBackup = new List<RechargeSimpleShowData>();

    public static List<ItemDataBase> ParseItemList(string text)
    {
        List<ItemDataBase> result = new List<ItemDataBase>();
        string[] strs = text.Split('|');

        foreach (string s in strs)
        {
            int n = s.IndexOf('#');

            if (n < 0)
                continue;

            int id = int.Parse(s.Substring(0, n).Trim());
            int num = int.Parse(s.Substring(n + 1, s.Length - n - 1).Trim());
            result.Add(new ItemDataBase() { tid = id, itemNum = num });
        }

        return result;
    }

    public override void Init()
    {
        base.Init();

        foreach (KeyValuePair<int, DataRecord> tmpPair in DataCenter.mChargeConfig.GetAllRecord())
        {
            int tmpIsMonthCard = (int)tmpPair.Value.getObject("IS_CARD");
            int tmpIndex = (int)tmpPair.Value.getObject("INDEX");
            DataRecord tmpRecord = tmpPair.Value;
            RechargeShowData tmpShowData = new RechargeShowData()
            {
                IsMonthCard = (tmpIsMonthCard == 1),
                Index = tmpIndex,
                Price = (int)tmpPair.Value.getObject("PRICE"),
                Record = tmpRecord
            };
			DEBUG.Log("支付界面内容1111-----");
            string tmpFirstRechargeAward = tmpRecord.get("FIRST_CHARGE_REWARD1");
            tmpShowData.ListFirstRechargeAward = ParseItemList(tmpFirstRechargeAward);
            string tmpBuyAward = tmpRecord.get("REWARD_BUY");
            tmpShowData.ListBuyAward = ParseItemList(tmpBuyAward);
            string tmpSendReward = tmpRecord.get("REWARD_SEND");
            tmpShowData.ListSendAward = ParseItemList(tmpSendReward);

            mListShowData.Add(tmpShowData);
            if(tmpIsMonthCard == 0)
                mListImmediateShowData.Add(tmpShowData);
            if (tmpIsMonthCard == 0)
                mDicImmediateTitle[tmpShowData.ListBuyAward[0].itemNum] = "a_ui_chongzhi_0" + (tmpIndex - 2);
        }
        mMonthCardCount = mListShowData.Count - mListImmediateShowData.Count;
        for (int i = 0, count = mListShowData.Count; i < count;i++)
        {
            RechargeShowData tmpShowData = mListShowData[i];
            if (!tmpShowData.IsMonthCard)
                mDicImmediateTitle[tmpShowData.ListBuyAward[0].itemNum] = "a_ui_chongzhi_0" + (tmpShowData.Index - 2);
        }

        EventCenter.Register("Button_recharge_check_btn", new DefineFactoryLog<Button_recharge_check_btn>());
        EventCenter.Register("Button_recharge_month_buy_0", new DefineFactoryLog<Button_recharge_buy_month_card>());
        EventCenter.Register("Button_recharge_month_buy_1", new DefineFactoryLog<Button_recharge_buy_month_card>());
        EventCenter.Register("Button_recharge_immediate_buy_btn", new DefineFactoryLog<Button_recharge_buy>());
        EventCenter.Register("Button_vip_exclusive_btn",new DefineFactory<Button_vip_exclusive_btn>());
    }

    protected override void _OnOpen(object param)
    {
		DEBUG.Log("支付界面内容222-----" + mOpenData.PanelDepth);
        __SetPanelDepth(mOpenData.PanelDepth + 2);
        __SetLocalPosZ(CommonParam.rechagePosZ);

        mRechargeGrids = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "recharge_grid");
        mRechargeGrids.MaxCount = mListImmediateShowData.Count;

        __RefreshMonthCard();
        __RefreshImmediateCard();
        
        GlobalModule.DoCoroutine(__StartRequest());

        NiceData tmpBtnRecharge = GameCommon.GetButtonData(mGameObjUI, "recharge_check_btn");
        if (tmpBtnRecharge != null)
            tmpBtnRecharge.set("OPEN_DATA", mOpenData);

		//专属客服不显示
        //GameCommon.SetUIVisiable(mGameObjUI, "vip_exclusive_btn",GameCommon.IsFuncCanShowByFuncID(2501));
		GameCommon.SetUIVisiable(mGameObjUI, "vip_exclusive_btn",false);
        //added by xuke 红点相关
        RefreshRechargeCheckNewMark();
        //end
    }


    public override void OnClose()
    {
        mIsStopMonthCard = true;
        mIsStopFirstRecharge = true;
        mIsStopChargeFeedBack = true;

        base.OnClose();
    }

    private void RefreshRechargeCheckNewMark() 
    {
        GameObject _rechargeCheckBtnObj = GameCommon.FindObject(mGameObjUI, "recharge_check_btn");
        GameCommon.SetNewMarkVisible(_rechargeCheckBtnObj, SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_SHOP_VIP_GIFT));
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "START_RECHARGE": __StartRecharge((int)objVal); break;
            case "RECHARGE_SUCCESS": __RechargeSuccess(objVal); break;
            case "RECHARGE_FAILED": __RechargeFailed(objVal); break;
            case "REFRESH_RECHARGE_CHECK_MARK": RefreshRechargeCheckNewMark(); break;
        }
    }

    /// <summary>
    /// 设置Panel深度
    /// </summary>
    /// <param name="depth"></param>
    private void __SetPanelDepth(int depth)
    {
        UIPanel tmpPanel = mGameObjUI.GetComponent<UIPanel>();
        if (tmpPanel != null)
            tmpPanel.depth = depth;

		DEBUG.Log("支付界面内容333-----" + tmpPanel.depth);
        UIPanel tmpPanelScrolLView = GetComponent<UIPanel>("recharge_scrollview");
        if (tmpPanelScrolLView != null)
            tmpPanelScrolLView.depth = depth + 1;

		DEBUG.Log("支付界面内容4444-----" + tmpPanelScrolLView.depth);
    }

    private void __SetLocalPosZ(int posZ)
    {
        if (mGameObjUI != null)
        {
            mGameObjUI.transform.localPosition = new Vector3(0, 0, posZ);
        }
    }

    /// <summary>
    /// 开始充值
    /// </summary>
    /// <param name="idx"></param>
    private void __StartRecharge(int idx)
    {
        mCurrRechargeIdx = idx;
        Net.StartWaitEffectRecharge();
        __StartTimeOut();

        //TODO
    }
    /// <summary>
    /// 充值成功
    /// </summary>
    /// <param name="idx"></param>
    private void __RechargeSuccess(object param)
    {
        //TODO
        __StopTimeOut();
        int tmpRechargeIdx = mCurrRechargeIdx - 1;
        //added by xuke
        mCurItemIndex = tmpRechargeIdx;
        //end
        //if (tmpRechargeIdx >= 0 && tmpRechargeIdx < mListShowData.Count)
        //{
        //    RechargeShowData tmpShowData = mListShowData[tmpRechargeIdx];
        //    tmpShowData.IsFirstRecharge = false;
        //    //加充值数据
        //    RoleLogicData.Self.RechargeNum += tmpShowData.Price;
        //    RoleLogicData.Self.VIPExp += (tmpShowData.Price * 10);
        //    __RefreshVIPInfo();
        //    __RefreshRechargeCard(tmpRechargeIdx);
        //}
        PayCompleteData tmpCompleteData = param as PayCompleteData;
        DEBUG.Log("__RechargeSuccess paySuccess = " + tmpCompleteData.PaySuccess + ", payReason = " + tmpCompleteData.PayReason + ", payData = " + tmpCompleteData.PayData);
        //added by xuke
        __RequestChargeInfo();
        //end
        Net.StopWaitEffectRecharge();
        mCurrRechargeIdx = -1;
    }
    private void __RechargeFailed(object param)
    {
        //TODO
        __StopTimeOut();
        PayCompleteData tmpCompleteData = param as PayCompleteData;
        DEBUG.Log("__RechargeFailed paySuccess = " + tmpCompleteData.PaySuccess + ", payReason = " + tmpCompleteData.PayReason + ", payData = " + tmpCompleteData.PayData);

        Net.StopWaitEffectRecharge();
        mCurrRechargeIdx = -1;
    }

#region 充值成功后刷新界面
    private int mCurItemIndex = -1;                 //> 当前充值的项的下标
    public static long mLeftDay = -1;               //> 月卡剩余天数
    public static bool mIsFirstRecharge = false;    //> 是否第一次充值
    private const int REQUEST_TIMES = 48;           //> 最大请求次数
    private const int WAIT_SECONDS = 10;            //> 请求等待时间
    //by chenliang
    //begin

    //是否停止月卡请求
    private bool mIsStopMonthCard = true;
    //月卡请求索引
    private int mMonthCardIndex = 0;
    //是否停止首充请求
    private bool mIsStopFirstRecharge = true;
    //首充请求索引
    private int mFirstRechargeIndex = 0;
    //是否停止充值反馈请求
    private bool mIsStopChargeFeedBack = true;
    //充值反馈请求索引
    private int mChargeFeedBackIndex = 0;
    //用于充值反馈请求判断
    private int mOnlyForChargeDiamond = 0;

    //end
    private void __RequestChargeInfo() 
    {
        DEBUG.Log("__RequestChargeInfo");
        if (!(mCurItemIndex >= 0 && mCurItemIndex < mListShowData.Count))
            return;
        //by chenliang
        //begin

//         //1.月卡信息
//         GlobalModule.DoCoroutine(__CardTimerCoroutine());
//         //2.首充信息
//         if (mIsFirstRecharge)
//         {
//             GlobalModule.DoCoroutine(__FirstRechargeCoroutine());
//         }
//         //3.元宝和VIP经验信息
//         GlobalModule.DoCoroutine(__RequestChargeFeedBackCoroutine());
//----------------
        if (mListShowData == null)
            return;
//        mListShowDataSimpleBackup = RechargeShowData.ToSimpleShowData(mListShowData);
        //设置相应请求开关
        if (mCurItemIndex >= 0 && mCurItemIndex <= 1)
        {
            mMonthCardIndex = 0;
            if (mIsStopMonthCard)
            {
                mIsStopMonthCard = false;
                //1.月卡信息
                GlobalModule.DoCoroutine(__CardTimerCoroutine());
            }
        }
        mFirstRechargeIndex = 0;
        if (mIsStopFirstRecharge)
        {
            mIsStopFirstRecharge = false;
            //2.首充信息
            GlobalModule.DoCoroutine(__FirstRechargeCoroutine());
        }
        mChargeFeedBackIndex = 0;
        mOnlyForChargeDiamond = RoleLogicData.Self.diamond;
        if (mIsStopChargeFeedBack)
        {
            mIsStopChargeFeedBack = false;
            //3.元宝和VIP经验信息
            GlobalModule.DoCoroutine(__RequestChargeFeedBackCoroutine());
        }

        //end
    }
    
    private IEnumerator __CardTimerCoroutine()
    {
        //by chenliang
        //begin

//         for (int i = 0; i < REQUEST_TIMES; i++) 
//         {
//             //请求刷新月卡信息
//             GlobalModule.DoCoroutine(__RequestMonthCardInfo());
//             yield return new WaitForSeconds(WAIT_SECONDS);
//            
//             if (mLeftDay != -1 && mLeftDay != mListShowData[mCurItemIndex].OverTime)
//                 break;
//         }
//------------------
        while (true)
        {
            if (mIsStopMonthCard)
                break;

            //请求刷新月卡信息
            yield return GlobalModule.DoCoroutine(__RequestMonthCardInfo());
            yield return new WaitForSeconds(WAIT_SECONDS);

            if (mIsStopMonthCard)
                break;

            //一直请求
//             if (!RechargeShowData.IsEquals(mListShowData, mListShowDataSimpleBackup))
//             {
//                 mIsStopMonthCard = true;
//                 mIsStopFirstRecharge = true;
//                 break;
//             }

            if (REQUEST_TIMES > 0)
            {
                mMonthCardIndex += 1;
                if (mMonthCardIndex >= REQUEST_TIMES)
                    break;
            }
        }

        //end
    }
    private IEnumerator __FirstRechargeCoroutine() 
    {
        //by chenliang
        //begin

//         for (int i = 0; i < REQUEST_TIMES; i++)
//         {
//             //请求首充信息
//             GlobalModule.DoCoroutine(__RequestFirstChargeInfo());
//             yield return new WaitForSeconds(WAIT_SECONDS);
// 
//             if (mIsFirstRecharge != mListShowData[mCurItemIndex].IsFirstRecharge)
//                 break;
//         }
//----------------
        while (true)
        {
            if (mIsStopFirstRecharge)
                break;

            //请求首充信息
            yield return GlobalModule.DoCoroutine(__RequestFirstChargeInfo());
            yield return new WaitForSeconds(WAIT_SECONDS);

            if (mIsStopFirstRecharge)
                break;

            //一直请求
//             if (!RechargeShowData.IsEquals(mListShowData, mListShowDataSimpleBackup))
//             {
//                 mIsStopMonthCard = true;
//                 mIsStopFirstRecharge = true;
//                 break;
//             }

            if (REQUEST_TIMES > 0)
            {
                mFirstRechargeIndex += 1;
                if (mFirstRechargeIndex >= REQUEST_TIMES)
                    break;
            }
        }

        //end
    }
    private IEnumerator __RequestChargeFeedBackCoroutine() 
    {
        //by chenliang
        //begin
//     private IEnumerator __RequestChargeFeedBackCoroutine() 
//     {
//         int _tmpDiamond = RoleLogicData.Self.diamond;
//         for (int i = 0; i < REQUEST_TIMES; i++)
//         {
//             //请求VIP充值信息
//             GlobalModule.DoCoroutine(__RequestChargeFeedBack());
//             yield return new WaitForSeconds(WAIT_SECONDS);
// 
//             if (_tmpDiamond != RoleLogicData.Self.diamond)
//                 break;
//         }
//--------------------
        while(true)
        {
            if (mIsStopChargeFeedBack)
                break;

            //请求VIP充值信息
            yield return GlobalModule.DoCoroutine(__RequestChargeFeedBack());
            yield return new WaitForSeconds(WAIT_SECONDS);

            if (mIsStopChargeFeedBack)
                break;

            if (mOnlyForChargeDiamond != RoleLogicData.Self.diamond)
            {
                mIsStopChargeFeedBack = true;
                break;
            }

            if (REQUEST_TIMES > 0)
            {
                mChargeFeedBackIndex += 1;
                if (mChargeFeedBackIndex >= REQUEST_TIMES)
                    break;
            }
        }

        //end
    }

    private IEnumerator __RequestChargeFeedBack()
    {
        ChargeFeedBack_Requester _requester = new ChargeFeedBack_Requester();
        yield return _requester.Start();
        if (_requester.respCode == 1)
        {
            SC_ChargeFeedBack _receive = _requester.respMsg;
            RoleLogicData.Self.money += mListShowData[mCurItemIndex].Price;
            RoleLogicData.Self.vipExp = _receive.vipExp;
            RoleLogicData.Self.vipLevel = _receive.vipLevel;
            RoleLogicData.Self.diamond = _receive.diamond;
            GameCommon.SetWindowData("INFO_GROUP_WINDOW", "UPDATE_DIAMOND", true);
            __RefreshVIPInfo();
            __RefreshRechargeCard(mCurItemIndex);
        }
    }
#endregion
    /// <summary>
    /// 判断指定索引值是否为月卡
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    private bool __IsMonthCard(int idx)
    {
        if (mListShowData == null || mListShowData.Count <= 0)
            return false;
        if (idx < 0 || idx >= mListShowData.Count)
            return false;

        int tmpIdx = mListShowData.FindIndex((RechargeShowData showData) =>
        {
            return (showData.IsMonthCard && (showData.Index - 1 == idx));
        });
        return (tmpIdx != -1);
    }

    /// <summary>
    // 刷新指定充值卡
    /// </summary>
    /// <param name="idx"></param>
    private void __RefreshRechargeCard(int idx)
    {
        tWindow _tWin = (DataCenter.GetData("RECHARGE_CONTAINER_WINDOW") as tWindow);
        if (_tWin == null || !_tWin.IsOpen())
            return;
        if (__IsMonthCard(idx))
            __RefreshMonthCard(idx);
        else
            __RefreshImmediateCard(idx);
    }

    /// <summary>
    /// 刷新月卡
    /// </summary>
    private void __RefreshMonthCard()
    {
        __RefreshMonthCard(0);
        __RefreshMonthCard(1);
    }
    /// <summary>
    /// 刷新月卡
    /// </summary>
    /// <param name="idx">指定月卡索引</param>
    private void __RefreshMonthCard(int idx)
    {
        if (!__IsMonthCard(idx))
            return;

        RechargeShowData tmpShowData = mListShowData[idx];
        GameObject tmpGOBtn = GameCommon.FindObject(mGameObjUI, "recharge_month_buy_" + idx);
        GameCommon.SetUIVisiable(tmpGOBtn, "tips_label", tmpShowData.OverTime == -1);
        GameCommon.SetUIVisiable(tmpGOBtn, "time_label", tmpShowData.OverTime > -1);
//        SetCountdown(tmpGOBtn, "num_time", tmpShowData.OverTime, null);
        if (tmpShowData.OverTime > -1)
            GameCommon.SetUIText(tmpGOBtn, "num_time", tmpShowData.OverTime.ToString() + "天");

        NiceData tmpBtnData = tmpGOBtn.GetComponent<UIButtonEvent>().mData;
        if (tmpBtnData != null)
        {
            tmpBtnData.set("INDEX", tmpShowData.Index);
            tmpBtnData.set("SHOW_DATA", tmpShowData);
            tmpBtnData.set("RECORD", tmpShowData.Record);
        }
    }
    /// <summary>
    /// 刷新直充
    /// </summary>
    private void __RefreshImmediateCard()
    {
        for (int i = 2, count = mListShowData.Count; i < count; i++)
            __RefreshImmediateCard(i);
    }
    /// <summary>
    /// 刷新直充
    /// </summary>
    /// <param name="idx">直充索引</param>
    private void __RefreshImmediateCard(int idx)
    {
        if (__IsMonthCard(idx))
            return;
        if (mRechargeGrids == null)
            return;
		DEBUG.Log ("充值内容显示111----" + idx);
        RechargeShowData tmpShowData = mListShowData[idx];
        GameObject tmpGOCell = mRechargeGrids.controlList[idx - mMonthCardCount];

        //标题
        UISprite tmpSpriteTitle = GameCommon.FindComponent<UISprite>(tmpGOCell, "price_sprite");
        if (tmpSpriteTitle != null)
        {
            string tmpSpriteName = "";
            if (mDicImmediateTitle.TryGetValue(tmpShowData.ListBuyAward[0].itemNum, out tmpSpriteName))
                tmpSpriteTitle.spriteName = tmpSpriteName;
        }
		DEBUG.Log ("充值内容显示222----");
        //首充标签
        //GameCommon.SetUIVisiable(tmpGOCell, "recharge_first_mark", tmpShowData.IsFirstRecharge);
		GameCommon.SetUIVisiable(tmpGOCell, "recharge_first_mark", true);

        //显示的物品
        List<ItemDataBase> tmpListItems = new List<ItemDataBase>();
        if (tmpShowData.IsFirstRecharge)
        {
            for (int i = 0, count = tmpShowData.ListFirstRechargeAward.Count; i < count; i++)
                tmpListItems.Add(tmpShowData.ListFirstRechargeAward[i]);
        }
        else
        {
            //策划要求，非首充，不显示购买的物品
             for (int i = 0, count = tmpShowData.ListBuyAward.Count; i < count; i++)
                 tmpListItems.Add(tmpShowData.ListBuyAward[i]);
//            for (int i = 0, count = tmpShowData.ListSendAward.Count; i < count; i++)
//                tmpListItems.Add(tmpShowData.ListSendAward[i]);
        }
        for (int i = 0; i < 2; i++)
        {
            GameObject tmpGOItem = GameCommon.FindObject(tmpGOCell, "item_icon" + i);
            if (tmpGOItem == null)
                continue;
            tmpGOItem.SetActive(i < tmpListItems.Count);
            if (i >= tmpListItems.Count)
                continue;
            GameCommon.SetOnlyItemIcon(tmpGOItem, tmpListItems[i].tid);
            GameCommon.SetOnlyItemCount(tmpGOItem, "num_label", tmpListItems[i].itemNum);
			int iTid = tmpListItems[i].tid;
			AddButtonAction (tmpGOItem, () => GameCommon.SetItemDetailsWindow (iTid));
        }

        //消耗钱数
        GameObject tmpGOBuy = GameCommon.FindObject(tmpGOCell, "recharge_immediate_buy_btn");
        string tmpShowPrice = SDKPayHelper.Instance.GetGoodShowPriceToChannelByIndex(tmpShowData.Index);
        GameCommon.SetUIText(tmpGOBuy, "num_label", tmpShowPrice);

        NiceData tmpBtnData = tmpGOBuy.GetComponent<UIButtonEvent>().mData;
        if (tmpBtnData != null)
        {
            tmpBtnData.set("INDEX", tmpShowData.Index);
            tmpBtnData.set("SHOW_DATA", tmpShowData);
            tmpBtnData.set("RECORD", tmpShowData.Record);
        }
    }

    private IEnumerator __StartRequest()
    {
        yield return GlobalModule.DoCoroutine(__RequestMonthCardInfo());
        yield return GlobalModule.DoCoroutine(__RequestFirstChargeInfo());
    }

    /// <summary>
    /// 请求月卡信息
    /// </summary>
    private IEnumerator __RequestMonthCardInfo()
    {
        CS_MonthCardQuery tmpQuery = new CS_MonthCardQuery();
        yield return NetManager.StartWaitResp(tmpQuery, "CS_MonthCardQuery", __OnRequestMonthCardSuccess, (string text) =>
        {
            //TODO 
        });
    }
    private void __OnRequestMonthCardSuccess(string text)
    {
        SC_MonthCardQuery tmpQueryRet = JCode.Decode<SC_MonthCardQuery>(text);
        //首充剩余时间
        if (tmpQueryRet.haveCheapCard)
            mListShowData[0].OverTime = tmpQueryRet.cheapLeft;
        else
            mListShowData[0].OverTime = -1;
        if (tmpQueryRet.haveExpensiveCard)
            mListShowData[1].OverTime = tmpQueryRet.expensiveLeft;
        else
            mListShowData[1].OverTime = -1;

        __RefreshMonthCard();
    }

    /// <summary>
    /// 请求首充信息
    /// </summary>
    private IEnumerator __RequestFirstChargeInfo()
    {
        ChargeFirst_Requester tmpRequester = new ChargeFirst_Requester();
        yield return tmpRequester.Start();

        if (tmpRequester.respCode == 1)
        {
            SC_ChargeFirst tmpChargeRet = tmpRequester.respMsg;
            for (int i = 0, count = mListShowData.Count; i < count; i++)
                mListShowData[i].IsFirstRecharge = true;
            for (int i = 0, count = tmpChargeRet.infoArr.Length; i < count; i++)
                mListShowData[tmpChargeRet.infoArr[i] - 1].IsFirstRecharge = false;

            __RefreshMonthCard();
            __RefreshImmediateCard();
        }
        else
        {
            //TODO
        }
    }

    private void __StartTimeOut()
    {
        __StopTimeOut();
        mIEnumRechargeTimeOut = __OnRequestTimeOut();
        GlobalModule.DoCoroutine(mIEnumRechargeTimeOut);
    }
    private void __StopTimeOut()
    {
        if (mIEnumRechargeTimeOut == null)
            return;
        GlobalModule.Instance.StopCoroutine(mIEnumRechargeTimeOut);
    }
    /// <summary>
    /// 请求超时
    /// </summary>
    /// <returns></returns>
    private IEnumerator __OnRequestTimeOut()
    {
        yield return new WaitForSeconds(mRechargeTimeOut);
        Net.StopWaitEffectRecharge();
        mIEnumRechargeTimeOut = null;
    }
}

/// <summary>
/// 充值
/// </summary>
class Button_recharge_buy : CEvent
{
    public override bool _DoEvent()
    {
        GlobalModule.DoCoroutine(__DoAction());

        return true;
    }

    private IEnumerator __DoAction()
    {
        RechargeShowData tmpShowData = getObject("SHOW_DATA") as RechargeShowData;
        int tmpIdx = tmpShowData.Index;
        //end
        string tmpOrderNum = "test";
        string tmpGoodChannelID = "0";
        //请求内部订单号
        GenerateOrder_Requester tmpRequester = new GenerateOrder_Requester(tmpIdx);
        yield return tmpRequester.Start();
        if (tmpRequester.respCode != 1)
        {
            //TODO出错
            yield break;
        }
        tmpOrderNum = tmpRequester.respMsg.orderNum;
        tmpGoodChannelID = tmpRequester.respMsg.productId;

        //请求SDK充值
        int tmpPrice = tmpShowData.Price;
        int tmpAwardTid = tmpShowData.ListBuyAward[0].tid;
        int tmpAwardNum = tmpShowData.ListBuyAward[0].itemNum;
        string tmpName = tmpAwardNum.ToString() + GameCommon.GetItemName(tmpAwardTid);
        RechargePayData tmpPayData = new RechargePayData()
        {
            GoodName = tmpName,
            GoodDesc = tmpName,
            GoodRealPrice = tmpPrice.ToString(),
            GoodCount = "1",
            BillInnerNo = tmpOrderNum,
            GoodChannelID = tmpGoodChannelID
        };
		DEBUG.Log("支付完成内部实现55555---------");
        DataCenter.SetData("RECHARGE_WINDOW", "START_RECHARGE", tmpIdx);
        SDKPayHelper.Instance.Pay(tmpPayData);
    }
}
/// <summary>
/// 月卡购买
/// </summary>
class Button_recharge_buy_month_card : CEvent
{
    public override bool _DoEvent()
    {
        GlobalModule.DoCoroutine(__DoAction());

        return true;
    }

    private IEnumerator __DoAction()
    {
        RechargeShowData tmpShowData = getObject("SHOW_DATA") as RechargeShowData;
        int tmpIdx = tmpShowData.Index;
        // 记录充值前月卡剩余天数
        RechargeWindow.mLeftDay = tmpShowData.OverTime;
        RechargeWindow.mIsFirstRecharge = tmpShowData.IsFirstRecharge;
        string tmpOrderNum = "test";
        string tmpGoodChannelID = "0";
        //请求内部订单号
        GenerateOrder_Requester tmpRequester = new GenerateOrder_Requester(tmpIdx);
        yield return tmpRequester.Start();
        if (tmpRequester.respCode != 1)
        {
            //TODO出错
            yield break;
        }
        tmpOrderNum = tmpRequester.respMsg.orderNum;
        tmpGoodChannelID = tmpRequester.respMsg.productId;

        //请求SDK充值
        int tmpPrice = tmpShowData.Price;
        int tmpAwardTid = tmpShowData.ListBuyAward[0].tid;
        int tmpAwardNum = tmpShowData.ListBuyAward[0].itemNum;
        string tmpName = tmpShowData.Record.getData("NAME");
        RechargePayData tmpPayData = new RechargePayData()
        {
            GoodName = tmpName,
            GoodDesc = tmpName,
            GoodRealPrice = tmpPrice.ToString(),
            GoodCount = "1",
            BillInnerNo = tmpOrderNum,
            GoodChannelID = tmpGoodChannelID
        };
		DEBUG.Log("支付完成内部实现66666---------");
        DataCenter.SetData("RECHARGE_WINDOW", "START_RECHARGE", tmpIdx);
        SDKPayHelper.Instance.Pay(tmpPayData);
    }
}
/// <summary>
/// 打开VIP特权
/// </summary>
class Button_recharge_check_btn : CEvent
{
    public override bool _DoEvent()
    {
        RechargeContainerOpenData tmpCurrData = getObject("OPEN_DATA") as RechargeContainerOpenData;
        if (tmpCurrData != null)
        {
            tmpCurrData.Page = RECHARGE_PAGE.VIP_RIGHT;
            RechargeContainerWindow.OpenMyself(tmpCurrData);
        }
        else
        {
            GameCommon.OpenRecharge(RECHARGE_PAGE.VIP_RIGHT, CommonParam.rechageDepth);
        }
        return true;
    }
}
