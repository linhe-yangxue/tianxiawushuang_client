using UnityEngine;
using System.Collections;
using Logic;

/// <summary>
/// VIP商店
/// </summary>
public class VIP_VIPShop_Requester:
    NetRequester<CS_VIPShop, SC_VIPShop>
{
    private int mIndex;

    public static IEnumerator StartRequest(int index)
    {
        VIP_VIPShop_Requester tmpRequester = new VIP_VIPShop_Requester();
        tmpRequester.mIndex = index;
        yield return tmpRequester.Start();
    }

    public int Index
    {
        set { mIndex = value; }
        get { return mIndex; }
    }

    protected override CS_VIPShop GetRequest()
    {
        CS_VIPShop tmpReq = new CS_VIPShop();
        tmpReq.index = mIndex;
        return tmpReq;
    }

    protected override void OnSuccess()
    {
        //TODO
    }
    protected override void OnFail()
    {
        //TODO
    }
}

/// <summary>
/// VIP商店请求
/// </summary>
public class VIP_VIPShopQuery_Requester:
    NetRequester<CS_VIPShopQuery, SC_VIPShopQuery>
{
    public static IEnumerator StartRequester()
    {
        VIP_VIPShopQuery_Requester tmpRequester = new VIP_VIPShopQuery_Requester();
        yield return tmpRequester.Start();
    }

    protected override CS_VIPShopQuery GetRequest()
    {
        CS_VIPShopQuery tmpReq = new CS_VIPShopQuery();
        return tmpReq;
    }

    protected override void OnSuccess()
    {
        //TODO
    }
    protected override void OnFail()
    {
        //TODO
    }
}

/// <summary>
/// 改变VIP等级
/// </summary>
public class VIP_ChangeVipLevel_Requester:
    NetRequester<CS_ChangeVipLevel, SC_ChangeVipLevel>
{
    private int mVIPLevel;

    public static IEnumerator StartRequest(int vipLevel)
    {
        VIP_ChangeVipLevel_Requester tmpRequester = new VIP_ChangeVipLevel_Requester();
        tmpRequester.mVIPLevel = vipLevel;
        yield return tmpRequester.Start();
    }

    protected override CS_ChangeVipLevel GetRequest()
    {
        CS_ChangeVipLevel tmpReq = new CS_ChangeVipLevel();
        tmpReq.vipLevel = mVIPLevel;
        return tmpReq;
    }

    protected override void OnSuccess()
    {
        RoleLogicData.Self.vipLevel = mVIPLevel;

        //刷新主界面左上角数据
        DataCenter.SetData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_ROLE_NAME", null);
    }
    protected override void OnFail()
    {
        //TODO
    }
}
