using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 单次夺宝结算数据
/// </summary>
public class GrabTreasureResultData
{
    public bool mIsWin;
    public int mFragId;
    public int mPreRoleGold;
    public int mPreRoleExp;
    public int mPreRoleLevel;
    public int mPostRoleGold;
    public int mPostRoleExp;
    public int mPostRoleLevel;
    public int mGetGold;
    public int mGetExp;
    public SC_RobMagicResult mResult;
}

/// <summary>
/// 夺宝目标列表
/// </summary>
public class GrabTreasure_GetRobAimList_Requester:
    NetRequester<CS_GetRobAimList, SC_GetRobAimList>
{
    private int mTid;       //法宝碎片tid

    public int Tid
    {
        set { mTid = value; }
        get { return mTid; }
    }

    public static IEnumerator StartRequest(int tid)
    {
        GrabTreasure_GetRobAimList_Requester tmpRequester = new GrabTreasure_GetRobAimList_Requester();
        tmpRequester.mTid = tid;
        yield return tmpRequester.Start();
    }

    protected override CS_GetRobAimList GetRequest()
    {
        CS_GetRobAimList tmpReq = new CS_GetRobAimList();
        tmpReq.tid = mTid;
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
/// 开始夺宝
/// </summary>
public class GrabTreasure_RobMagicStart_Requester:
    NetRequester<CS_RobMagicStart, SC_RobMagicStart>
{
    private string mAimUid;     //被抢玩家uid
    private int mTid;           //法宝碎片tid

    public string AimUid
    {
        set { mAimUid = value; }
        get { return mAimUid; }
    }
    public int Tid
    {
        set { mTid = value; }
        get { return mTid; }
    }

    public static IEnumerator StartRequester(string aimUid, int tid)
    {
        GrabTreasure_RobMagicStart_Requester tmpRequester = new GrabTreasure_RobMagicStart_Requester();
        tmpRequester.mAimUid = aimUid;
        tmpRequester.mTid = tid;
        yield return tmpRequester.Start();
    }

    protected override CS_RobMagicStart GetRequest()
    {
        CS_RobMagicStart tmpReq = new CS_RobMagicStart();
        tmpReq.aimUid = mAimUid;
        tmpReq.tid = mTid;
        return tmpReq;
    }

    protected override void OnSuccess()
    {
        //TODO
    }
    protected override void OnFail()
    {
        switch (respCode)
        {
            case 1101:
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_SPIRIT_NO_ENOUGH);
                } break;
        }
    }
}

/// <summary>
/// 结束夺宝
/// </summary>
public class GrabTreasure_RobMagicResult_Requester:
    NetRequester<CS_RobMagicResult, SC_RobMagicResult>
{
    private string mAimUid;     //被抢玩家
    private int mTid;           //法宝碎片tid
    private int mIsWin;         //是否胜利

    public string AimUid
    {
        set { mAimUid = value; }
        get { return mAimUid; }
    }
    public int Tid
    {
        set { mTid = value; }
        get { return mTid; }
    }
    public int IsWin
    {
        set { mIsWin = value; }
        get { return mIsWin; }
    }

    public static IEnumerator StartRequester(string aimUid, int tid, int isWin)
    {
        GrabTreasure_RobMagicResult_Requester tmpRequester = new GrabTreasure_RobMagicResult_Requester();
        tmpRequester.mAimUid = aimUid;
        tmpRequester.mTid = tid;
        tmpRequester.mIsWin = isWin;
        yield return tmpRequester.Start();
    }

    protected override CS_RobMagicResult GetRequest()
    {
        CS_RobMagicResult tmpReq = new CS_RobMagicResult();
        tmpReq.aimUid = mAimUid;
        tmpReq.tid = mTid;
        tmpReq.isWin = mIsWin;
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

// <summary>
/// 一键夺宝
/// </summary>
public class GrabTreasure_RobOneKey_Requester:
	NetRequester<CS_RobOneKey,SC_RobOneKey>
{
	private int equipTid;    	 //目标法器
	private int fragmentTid;			 //目标碎片
	public int EquipTid
	{
		set{ equipTid = value;}
		get{ return equipTid;}
	}
	public int FragmentTid
	{
		set{ fragmentTid = value;}
		get{ return fragmentTid;}
	}
	public static IEnumerator StartRequester(int aimTid, int tid)
	{
		GrabTreasure_RobOneKey_Requester tmpRequester = new GrabTreasure_RobOneKey_Requester();
		tmpRequester.equipTid = aimTid;
		tmpRequester.fragmentTid = tid;
		yield return tmpRequester.Start();
	}
	protected override CS_RobOneKey GetRequest()
	{
		CS_RobOneKey tmpReq = new CS_RobOneKey();
		tmpReq.equipTid = equipTid;
		tmpReq.fragmentTid = fragmentTid;
		return tmpReq;
	}
	protected override void OnSuccess()
	{
		//TODO
	}
	protected override void OnFail()
	{
		switch (respCode)
		{
		case 1101:
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_SPIRIT_NO_ENOUGH);
		} break;
		}
	}

}

/// <summary>
/// 夺五次
/// </summary>
public class GrabTreasure_Rob5Times_Requester:
    NetRequester<CS_Rob5Times, SC_Rob5Times>
{
    private string mAimUid;     //被抢玩家
    private int mTid;           //法强碎片tid

    public string AimUid
    {
        set { mAimUid = value; }
        get { return mAimUid; }
    }
    public int Tid
    {
        set { mTid = value; }
        get { return mTid; }
    }

    public static IEnumerator StartRequester(string aimTid, int tid)
    {
        GrabTreasure_Rob5Times_Requester tmpRequester = new GrabTreasure_Rob5Times_Requester();
        tmpRequester.mAimUid = aimTid;
        tmpRequester.mTid = tid;
        yield return tmpRequester.Start();
    }

    protected override CS_Rob5Times GetRequest()
    {
        CS_Rob5Times tmpReq = new CS_Rob5Times();
        tmpReq.aimUid = mAimUid;
        tmpReq.tid = mTid;
        return tmpReq;
    }

    protected override void OnSuccess()
    {
        //TODO
    }
    protected override void OnFail()
    {
        switch (respCode)
        {
            case 1101:
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_SPIRIT_NO_ENOUGH);
                } break;
        }
    }
}

/// <summary>
/// 被抢夺记录
/// </summary>
public class GrabTreasure_GetRobbedHistoryList_Requester:
    NetRequester<CS_GetRobbedHistoryList, SC_GetRobbedHistoryList>
{
    public static IEnumerator StartRequest()
    {
        GrabTreasure_GetRobbedHistoryList_Requester tmpRequester = new GrabTreasure_GetRobbedHistoryList_Requester();
        yield return tmpRequester.Start();
    }

    protected override CS_GetRobbedHistoryList GetRequest()
    {
        CS_GetRobbedHistoryList tmpReq = new CS_GetRobbedHistoryList();
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
/// 法器合成
/// </summary>
public class GrabTreasure_MagicCompose_Requester:
    NetRequester<CS_MagicCompose, SC_MagicCompose>
{
    private int mTid;                   //法器tid
    private List<ItemDataBase> mFrags;  //合成所需碎片

    public int Tid
    {
        set { mTid = value; }
        get { return mTid; }
    }
    public List<ItemDataBase> Frags
    {
        set { mFrags = value; }
        get { return mFrags; }
    }

    public static IEnumerator StartRequest(int tid, List<ItemDataBase> frags)
    {
        GrabTreasure_MagicCompose_Requester tmpRequester = new GrabTreasure_MagicCompose_Requester();
        tmpRequester.mTid = tid;
        tmpRequester.mFrags = frags;
        yield return tmpRequester.Start();
    }

    protected override CS_MagicCompose GetRequest()
    {
        CS_MagicCompose tmpReq = new CS_MagicCompose();
        tmpReq.tid = mTid;
        tmpReq.frags = mFrags;
        return tmpReq;
    }

    protected virtual void OnSuccess()
    {
        //TODO
    }
    protected virtual void OnFail()
    {
        switch (respCode)
        {
            case 2009:
                {
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_GRAB_COMPOSE_FRAGMENT_NO_ENOUGH);
                } break;
        }
    }
}

/// <summary>
/// 使用免战令牌
/// </summary>
public class GrabTreasure_UseTruceCard_Requester:
    NetRequester<CS_UseTruceCard, SC_UseTruceCard>
{
    private ItemDataBase mTruceCard;      //免战令

    public ItemDataBase TruceCard
    {
        set { mTruceCard = value; }
        get { return mTruceCard; }
    }

    public static IEnumerator StartRequester(ItemDataBase truceCard)
    {
        GrabTreasure_UseTruceCard_Requester tmpRequester = new GrabTreasure_UseTruceCard_Requester();
        tmpRequester.mTruceCard = truceCard;
        yield return tmpRequester.Start();
    }

    protected override CS_UseTruceCard GetRequest()
    {
        CS_UseTruceCard tmpReq = new CS_UseTruceCard();
        tmpReq.truceCard = mTruceCard;
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
/// 获取免战时间
/// </summary>
public class GrabTreasure_GetTruceTime_Requester :
    NetRequester<CS_GetTruceTime, SC_GetTruceTime>
{
    public static IEnumerator StartRequester()
    {
        GrabTreasure_GetTruceTime_Requester tmpRequester = new GrabTreasure_GetTruceTime_Requester();
        yield return tmpRequester.Start();
    }

    protected override CS_GetTruceTime GetRequest()
    {
        CS_GetTruceTime tmpReq = new CS_GetTruceTime();
        return tmpReq;
    }

    protected override void OnSuccess()
    {
        DataCenter.SetData("GRABTREASURE_WINDOW", "REFRESH_PEACE_TIME", respMsg.time);
    }
    protected override void OnFail()
    {
        //TODO
    }
}
