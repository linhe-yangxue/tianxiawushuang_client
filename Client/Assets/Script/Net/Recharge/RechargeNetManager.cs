using UnityEngine;
using System.Collections;
using System;

public class GenerateOrder_Requester
    :NetRequester<CS_GenerateOrder, SC_GenerateOrder>
{
    private int mShelfId;

    public GenerateOrder_Requester(int shelfId)
    {
        mShelfId = shelfId;
    }

    public static IEnumerator StartRequest(int shelfId)
    {
        GenerateOrder_Requester tmpRequester = new GenerateOrder_Requester(shelfId);
        yield return tmpRequester.Start();
    }

    protected override CS_GenerateOrder GetRequest()
    {
        CS_GenerateOrder tmpReq = new CS_GenerateOrder();
        tmpReq.shelfId = mShelfId;
        return tmpReq;
    }
    protected override void OnStart() { }
    protected override void OnSuccess() { }
    protected override void OnFail() { }
}

public class ChargeFirst_Requester
    : NetRequester<CS_ChargeFirst, SC_ChargeFirst>
{
    public ChargeFirst_Requester()
    {
    }

    public static IEnumerator StartRequest()
    {
        ChargeFirst_Requester tmpRequester = new ChargeFirst_Requester();
        yield return tmpRequester.Start();
    }

    protected override CS_ChargeFirst GetRequest()
    {
        CS_ChargeFirst tmpReq = new CS_ChargeFirst();
        return tmpReq;
    }
    protected override void OnStart() { }
    protected override void OnSuccess() { }
    protected override void OnFail() { }
}

public class ChargeResultSimulate_Requester
{
    private int mCode = 0;
    private int mID = 0;
    private string mOrder = "";
    private string mCPOrder = "";
    private string mInfo = "";
    private int mAmount = 0;

    private Action mSuccessCallback;
    private Action mFailedCallback;

    public ChargeResultSimulate_Requester(
        int code,
        int id,
        string order, string cpOrder, string info, int amount,
        Action successCallback, Action failedCallback)
    {
        mCode = code;
        mID = id;
        mOrder = order;
        mCPOrder = cpOrder;
        mInfo = info;
        mAmount = amount;
        mSuccessCallback = successCallback;
        mFailedCallback = failedCallback;
    }

    public static IEnumerator StartRequest(
        int code,
        int id,
        string order, string cpOrder, string info, int amount,
        Action successCallback, Action failedCallback)
    {
        ChargeResultSimulate_Requester tmpRequester = new ChargeResultSimulate_Requester(
            code,
            id,
            order, cpOrder, info, amount,
            successCallback, failedCallback);
        yield return tmpRequester.Start();
    }

    Coroutine Start()
    {
        CS_ChargeResult tmpReq = GetRequest();
        return NetManager.StartWaitResp(tmpReq,
            (string text) =>
            {
                OnSuccess();
            },
            (string text) =>
            {
                OnFail();
            });
    }

    protected CS_ChargeResult GetRequest()
    {
        CS_ChargeResult tmpReq = new CS_ChargeResult();
        tmpReq.code = mCode;
        tmpReq.id = mID;
        tmpReq.order = mOrder;
        tmpReq.cporder = mCPOrder;
        tmpReq.info = mInfo;
        tmpReq.amount = mAmount;
        tmpReq.Sign();
        return tmpReq;
    }
    protected void OnSuccess()
    {
        if (mSuccessCallback != null)
            mSuccessCallback();
    }
    protected void OnFail()
    {
        if (mFailedCallback != null)
            mFailedCallback();
    }
}

/// <summary>
/// 充值后请求元宝，VIP等级，VIP经验的刷新信息
/// </summary>
public class ChargeFeedBack_Requester : NetRequester<CS_ChargeFeedBack, SC_ChargeFeedBack> 
{
    public ChargeFeedBack_Requester() { }
    protected override CS_ChargeFeedBack GetRequest()
    {
        CS_ChargeFeedBack _request = new CS_ChargeFeedBack();
        return _request;
    }
    public static IEnumerator StartRequest() 
    {
        ChargeFeedBack_Requester _request = new ChargeFeedBack_Requester();
        yield return _request.Start();
    }
    protected override void OnStart(){ }
    protected override void OnSuccess() { }
    protected override void OnFail() { }
}

/// <summary>
/// 用户支付订单
/// </summary>
public class ChargePayOrder_Requester
    : NetRequester<CS_ChargePayOrder, SC_ChargePayOrder>
{
    private string mCPOrder = "";       //内部订单号

    public ChargePayOrder_Requester(string oporder)
    {
        mCPOrder = oporder;
    }

    protected override CS_ChargePayOrder GetRequest()
    {
        CS_ChargePayOrder tmpReq = new CS_ChargePayOrder();
        tmpReq.cporder = mCPOrder;
        return tmpReq;
    }
}

/// <summary>
/// 用户取消订单
/// </summary>
public class ChargeCancelOrder_Requester
    : NetRequester<CS_ChargeCancelOrder, SC_ChargeCancelOrder>
{
    private string mCPOrder = "";       //内部订单号

    public ChargeCancelOrder_Requester(string cporder)
    {
        mCPOrder = cporder;
    }

    protected override CS_ChargeCancelOrder GetRequest()
    {
        CS_ChargeCancelOrder tmpReq = new CS_ChargeCancelOrder();
        tmpReq.cporder = mCPOrder;
        return tmpReq;
    }
}
