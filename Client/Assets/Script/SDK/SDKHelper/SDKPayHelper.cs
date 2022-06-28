using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using System;

/// <summary>
/// SDK支付缺省数据接口
/// </summary>
public interface ISDKPayData
{
    /// <summary>
    /// 商品名称
    /// </summary>
    string GoodName { set; get; }
    /// <summary>
    /// 商品描述
    /// </summary>
    string GoodDesc { set; get; }
    /// <summary>
    /// 商品支付价格
    /// </summary>
    string GoodRealPrice { set; get; }
    /// <summary>
    /// 商品数量
    /// </summary>
    string GoodCount { set; get; }
    /// <summary>
    /// 内部订单号
    /// </summary>
    string BillInnerNo { set; get; }
    /// <summary>
    /// 商品在渠道商的ID
    /// </summary>
    string GoodChannelID { set; get; }
    /// <summary>
    /// 额外参数
    /// </summary>
    string ExtraData { set; get; }
}

/// <summary>
/// SDK支付完整数据接口
/// </summary>
public interface ISDKFullPayData : ISDKPayData
{
    /// <summary>
    /// 角色名称
    /// </summary>
    string RoleName { set; get; }
    /// <summary>
    /// 服务器ID
    /// </summary>
    string ServerID { set; get; }
    /// <summary>
    /// 服务器名称
    /// </summary>
    string ServerName { set; get; }
}

public class PayCompleteData
{
    private bool mPaySuccess;
    private string mPayReason;
    private string mPayData;

    public bool PaySuccess
    {
        set { mPaySuccess = value; }
        get { return mPaySuccess; }
    }
    public string PayReason
    {
        set { mPayReason = value; }
        get { return mPayReason; }
    }
    public string PayData
    {
        set { mPayData = value; }
        get { return mPayData; }
    }
}

/// <summary>
/// SDK支付辅助类
/// </summary>
public class SDKPayHelper
{
    private static SDKPayHelper msInstance;

    private ICoroutineOperation mCoroutineOperation;

//#if !UNITY_EDITOR && !NO_USE_SDK
//    private Dictionary<string, U3DSharkBaseData> mDicPayRecord = new Dictionary<string, U3DSharkBaseData>();        //支付记录，key为内部订单号，value为支付数据
//#endif
    private string mLastPayBillInnerNo = "";        //最近一次

    private SDKPayHelper()
    {
    }

    public static SDKPayHelper Instance
    {
        get
        {
            if (msInstance == null)
                msInstance = new SDKPayHelper();
            return msInstance;
        }
    }

    public ICoroutineOperation coroutineOperation
    {
        set { mCoroutineOperation = value; }
        get { return mCoroutineOperation; }
    }

    public string LastPayBillInnerNo
    {
        get { return mLastPayBillInnerNo; }
    }

    /// <summary>
    /// 根据ChargeConfig指定索引值获取相应渠道商品价格信息
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public string GetGoodShowPriceToChannelByIndex(int index)
    {
        if (DataCenter.mChargeConfig == null)
            return "";

        DataRecord tmpRecord = DataCenter.mChargeConfig.GetRecord(index);
        if (tmpRecord == null)
            return "";
        int tmpIntPrice = (int)tmpRecord.getObject("PRICE");
        float tmpPrice = (float)tmpIntPrice;
        float tmpProportion = (float)tmpRecord.getObject("PROPORTION");
        float tmpInnerChargePrice = tmpPrice * tmpProportion;
        return GetGoodShowPriceToChannel(tmpInnerChargePrice);
    }
    /// <summary>
    /// 将指定内部充值金额转换为当前渠道商品价格信息
    /// </summary>
    /// <param name="price"></param>
    /// <returns></returns>
    public string GetGoodShowPriceToChannel(float innerChargePrice)
    {
        if (DataCenter.mConduit == null)
            return "";

        string tmpStrCurrChannelID = DeviceBaseData.channel;
        int tmpCurrChannelID = 0;
        if (!int.TryParse(tmpStrCurrChannelID, out tmpCurrChannelID))
            return "";
        DataRecord tmpRecord = DataCenter.mConduit.GetRecord(tmpCurrChannelID);
        if (tmpRecord == null)
            return "";
        float tmpConduitProportion = (float)tmpRecord.getObject("CONDUIT_PROPORTION");
        float tmpShowPrice = innerChargePrice * tmpConduitProportion;
        string tmpConduitMoney = tmpRecord.getData("CONDUIT_MONEY");
        string tmpStrShowPrice = "";
        tmpStrShowPrice += tmpShowPrice.ToString("f2");
        tmpStrShowPrice += tmpConduitMoney;
        return tmpStrShowPrice;
    }

    /// <summary>
    /// 缺省支付接口
    /// </summary>
    /// <param name="payData"></param>
    /// <returns></returns>
    public bool Pay(ISDKPayData payData)
    {
        if (payData == null)
            return false;

        return Pay(
            payData.GoodName, payData.GoodDesc, payData.GoodRealPrice, payData.GoodCount,
            payData.BillInnerNo, payData.GoodChannelID,
            payData.ExtraData);
    }
    /// <summary>
    /// 缺省支付接口
    /// </summary>
    /// <param name="goodName">商品名称</param>
    /// <param name="goodDesc">商品描述</param>
    /// <param name="goodRealPrice">商品支付价格（单位：分）</param>
    /// <param name="goodCount">商品数量</param>
    /// <param name="billInnerNo">内部订单号（如果没有填“0”）</param>
    /// <param name="goodChannelID">商品在渠道上的id（如果没有填“0”）如果大于0 认为是购买商品</param>
    /// <param name="extraData">传递的额外参数（建议传入需要用来做订单标识的信息）</param>
    /// <returns>是否执行支付成功</returns>
    public bool Pay(
        string goodName, string goodDesc, string goodRealPrice, string goodCount,
        string billInnerNo, string goodChannelID,
        string extraData)
    {
        if (!LoginData.Instance.IsLoginTokenValid || !LoginData.Instance.IsGameTokenValid)
            return false;

        string tmpRoleName = RoleLogicData.Self.name;

        string tmpServerID = CommonParam.mZoneID;
        string tmpServerName = CommonParam.mZoneName;

        return Pay(
            tmpRoleName,
            goodName, goodDesc, goodRealPrice, goodCount,
            tmpServerID, tmpServerName,
            billInnerNo, goodChannelID,
            extraData);
    }
    /// <summary>
    /// 完整支付接口
    /// </summary>
    /// <param name="payData"></param>
    /// <returns></returns>
    public bool Pay(ISDKFullPayData payData)
    {
        if (payData == null)
            return false;

        return Pay(
            payData.RoleName,
            payData.GoodName, payData.GoodDesc, payData.GoodRealPrice, payData.GoodCount,
            payData.ServerID, payData.ServerName,
            payData.BillInnerNo, payData.GoodChannelID,
            payData.ExtraData);
    }
    /// <summary>
    /// 完整支付接口
    /// </summary>
    /// <param name="roleName">玩家在游戏中的角色名字</param>
    /// <param name="goodName">商品名称</param>
    /// <param name="goodDesc">商品描述</param>
    /// <param name="goodRealPrice">商品支付价格（单位：分）</param>
    /// <param name="goodCount">商品数量</param>
    /// <param name="serverID">所在服务器id（如果没有填“0”）</param>
    /// <param name="serverName">所在服务器名字（如果没有填“sever_name”）</param>
    /// <param name="billInnerNo">内部订单号（如果没有填“0”）</param>
    /// <param name="goodChannelID">商品在渠道上的id（如果没有填“0”）如果大于0 认为是购买商品</param>
    /// <param name="extraData">传递的额外参数（建议传入需要用来做订单标识的信息）</param>
    /// <returns>是否执行支付成功</returns>
    public bool Pay(
        string roleName,
        string goodName, string goodDesc, string goodRealPrice, string goodCount,
        string serverID, string serverName,
        string billInnerNo, string goodChannelID,
        string extraData)
    {
        bool tmpIsSimulateRecharge = false;
#if !UNITY_EDITOR && !NO_USE_SDK
        if (mDicPayRecord == null)
            return false;

        tmpIsSimulateRecharge = !CommonParam.isUseSDK;
#else
        tmpIsSimulateRecharge = true;
#endif
        if (tmpIsSimulateRecharge)
        {
            mLastPayBillInnerNo = billInnerNo;
#if !UNITY_EDITOR && !NO_USE_SDK
            mDicPayRecord[billInnerNo] = new U3DSharkBaseData();
            if (!CommonParam.isUseSDK)
                U3DSharkEventListener.Instance.InitSelf();
#endif
            GlobalModule.DoOnNextUpdate(() =>
            {
                DataCenter.OpenMessageOkWindow(
                    "确认充值",
                    () =>
                    {
                        //通知服务器充值成功
                        string tmpChannelBillNo = billInnerNo + UnityEngine.Random.Range(0, 10000);
                        int tmpID = RoleLogicData.Self.character.tid;
                        GlobalModule.DoCoroutine(ChargeResultSimulate_Requester.StartRequest(
                            0,
                            tmpID,
                            tmpChannelBillNo, billInnerNo, "", int.Parse(goodRealPrice),
                            () =>
                            {
                                PayComplete(1, "", "");
                            },
                            () =>
                            {
                                PayComplete(0, "Fail test.", "");
                            }));
                    },
                    () => { PayComplete(0, "", ""); });
                DataCenter.SetData("MESSAGE_WINDOW", "SET_PANEL_DEPTH", 1000);
                DataCenter.SetData("MESSAGE_WINDOW", "SET_PANEL_START_RENDER_QUEUE", 5000);
            });
            return true;
        }

#if !UNITY_EDITOR && !NO_USE_SDK
        U3DSharkBaseData tmpPayData = new U3DSharkBaseData();

        //用户ID，渠道返回，没有填空
        tmpPayData.SetData(U3DSharkAttName.USER_ID, CommonParam.SDKUserID);
        //玩家在游戏中的角色ID
        tmpPayData.SetData(U3DSharkAttName.ROLE_ID, CommonParam.mUId);
        //玩家在游戏中的角色名字
        tmpPayData.SetData(U3DSharkAttName.ROLE_NAME, roleName);

        //商品名称
        tmpPayData.SetData(U3DSharkAttName.ITEM_NAME, goodName);
        //商品描述
        tmpPayData.SetData(U3DSharkAttName.ITEM_DESC, goodDesc);
        //商品支付价格（单位：分）
        tmpPayData.SetData(U3DSharkAttName.REAL_PRICE, goodRealPrice);
        //商品数量
        tmpPayData.SetData(U3DSharkAttName.ITEM_COUNT, goodCount);

        //所在服务器id（如果没有填“0”）
        tmpPayData.SetData(U3DSharkAttName.SERVER_ID, serverID);
        //所在服务器名字（如果没有填“sever_name”）
        tmpPayData.SetData(U3DSharkAttName.SERVER_NAME, serverName);
        //内部订单号（如果没有填“0”）
        tmpPayData.SetData(U3DSharkAttName.BILL_NUMBER, billInnerNo);
        //商品在渠道上的id（如果没有填“0”）如果大于0 认为是购买商品
        tmpPayData.SetData(U3DSharkAttName.ITEM_SERVER_ID, goodChannelID);

        //传递的额外参数（建议传入需要用来做订单标识的信息）
        tmpPayData.SetData(U3DSharkAttName.EXTRA, extraData);

        mDicPayRecord[billInnerNo] = tmpPayData;

        mLastPayBillInnerNo = billInnerNo;
        U3DSharkSDK.Instance.PayItem(tmpPayData);
#endif

        return true;
    }

    /// <summary>
    /// 最后一次支付完成
    /// </summary>
    /// <param name="payCode">支付回复码</param>
    /// <param name="payReason">支付结果原因</param>
    /// <param name="payData">支付结果数据</param>
    public void PayComplete(int payCode, string payReason, string payData)
    {
        PayComplete(mLastPayBillInnerNo, payCode, payReason, payData);
    }
    /// <summary>
    /// 支付完成
    /// </summary>
    /// <param name="billInnerNo">内部订单号</param>
    /// <param name="payCode">支付回复码</param>
    /// <param name="payReason">支付结果原因</param>
    /// <param name="payData">支付结果数据</param>
    public void PayComplete(string billInnerNo, int payCode, string payReason, string payData)
    {
        if (mCoroutineOperation != null)
            mCoroutineOperation.StartCoroutine(__PayComplete(billInnerNo, payCode, payReason, payData));
    }
    /// <summary>
    /// 支付完成内部实现
    /// </summary>
    /// <param name="billInnerNo">内部订单号</param>
    /// <param name="payCode">支付回复码</param>
    /// <param name="payReason">支付结果原因</param>
    /// <param name="payData">支付结果数据</param>
    private IEnumerator __PayComplete(string billInnerNo, int payCode, string payReason, string payData)
    {
        bool tmpPaySuccess = (payCode == 1);
        if (tmpPaySuccess)
        {
            //通知服务器支付完成
            ChargePayOrder_Requester tmpRequester = new ChargePayOrder_Requester(billInnerNo);
            yield return tmpRequester.Start();
        }
        else
        {
            //通知服务器取消支付
            ChargeCancelOrder_Requester tmpRequester = new ChargeCancelOrder_Requester(billInnerNo);
            yield return tmpRequester.Start();
        }
        PayCompleteData tmpCompleteData = new PayCompleteData()
        {
            PaySuccess = tmpPaySuccess,
            PayReason = payReason,
            PayData = payData
        };
#if !UNITY_EDITOR && !NO_USE_SDK
        if (mDicPayRecord == null)
        {
            DEBUG.Log("Bill inner number " + billInnerNo + ", local pay data record is null.");
            yield break;
        }

        if (!mDicPayRecord.ContainsKey(billInnerNo))
        {
            DEBUG.Log("Bill inner number " + billInnerNo + ", local pay data record can't find bill.");
            yield break;
        }
		DEBUG.Log("支付完成内部实现2222---------");
        DataCenter.SetData("RECHARGE_WINDOW", (tmpPaySuccess ? "RECHARGE_SUCCESS" : "RECHARGE_FAILED"), tmpCompleteData);
        mDicPayRecord.Remove(billInnerNo);
#else
		DEBUG.Log("支付完成内部实现1111---------");
        DataCenter.SetData("RECHARGE_WINDOW", (tmpPaySuccess ? "RECHARGE_SUCCESS" : "RECHARGE_FAILED"), tmpCompleteData);
#endif
    }

#if !UNITY_EDITOR && !NO_USE_SDK
    /// <summary>
    /// 获取内部订单号数据
    /// </summary>
    /// <param name="billInnerNo">内部订单号</param>
    /// <returns>相应支付数据</returns>
    public U3DSharkBaseData GetPayData(string billInnerNo)
    {
        if (mDicPayRecord == null)
            return null;

        U3DSharkBaseData tmpPayData = null;
        if (!mDicPayRecord.TryGetValue(billInnerNo, out tmpPayData))
            return null;
        return tmpPayData;
    }
#endif
}
