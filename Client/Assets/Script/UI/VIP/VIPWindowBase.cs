using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

public class VIPWindowBase : tWindow
{
    protected RechargeContainerOpenData mOpenData;
    protected List<DataRecord> mListVIPRecords = new List<DataRecord>();
    private int mLastVipLevel = 0;

    public override void Init()
    {
        base.Init();

        foreach (KeyValuePair<int, DataRecord> tmpPair in DataCenter.mVipListConfig.GetAllRecord())
            mListVIPRecords.Add(tmpPair.Value);
    }

    public override void Open(object param)
    {
        //added by xuke 红点相关
        mLastVipLevel = RoleLogicData.Self.vipLevel;
        //end
        mOpenData = param as RechargeContainerOpenData;

        base.Open(param);

        _OnOpen(param);

        Refresh(param);
    }
    protected virtual void _OnOpen(object param)
    {
    }

    public override bool Refresh(object param)
    {
        __RefreshVIPInfo();

        return _OnRefresh(param);
    }
    protected virtual bool _OnRefresh(object param)
    {
        return true;
    }

    /// <summary>
    /// 刷新VIP信息
    /// </summary>
    protected void __RefreshVIPInfo()
    {
        tWindow _tWin = (DataCenter.GetData("RECHARGE_CONTAINER_WINDOW") as tWindow);
        if (_tWin == null || !_tWin.IsOpen())
            return;

        int tmpCurrVIPLevel = RoleLogicData.Self.vipLevel;
        if (mListVIPRecords == null || tmpCurrVIPLevel < 0 || tmpCurrVIPLevel >= mListVIPRecords.Count)
            return;

        DataRecord tmpRecord = mListVIPRecords[tmpCurrVIPLevel];

        //VIP等级
        GameObject tmpVIPNumParent  = GetSub("curr_vip_level");
		if(CommonParam.isOnLineVersion)
		{
			GameCommon.SetUIText(tmpVIPNumParent, "num", "VIP " + tmpCurrVIPLevel.ToString());
		}else
		{
			GameCommon.SetUIText(tmpVIPNumParent, "num", tmpCurrVIPLevel.ToString() + "级至尊");
		}        

        //VIP等级进度
        UISlider tmpVIPProgress = GameCommon.FindComponent<UISlider>(mGameObjUI, "progress_bar");
        //查看下一级充值额度
        int tmpMaxVIPLevel = VIPHelper.GetMaxVIPLevel();
        int tmpNextVIPLevel = (tmpCurrVIPLevel >= tmpMaxVIPLevel) ? tmpMaxVIPLevel : (tmpCurrVIPLevel + 1);
        DataRecord tmpNextVIPRecord = GameCommon.GetVIPConfig(tmpNextVIPLevel);
        int tmpNextNeedVIPExp = (int)tmpNextVIPRecord.getObject("CASHPAID");
        int tmpCurrVIPExp = RoleLogicData.Self.vipExp;
        string tmpLBVIPProgress = "";
        if (tmpCurrVIPLevel >= tmpMaxVIPLevel)
        {
            tmpVIPProgress.value = 1.0f;
            tmpLBVIPProgress = "已满级";
        }
        else
        {
            DataRecord tmpCurrVIPRecord = GameCommon.GetVIPConfig(tmpCurrVIPLevel);
            int tmpCurrNeedVIPExp = (int)tmpCurrVIPRecord.getObject("CASHPAID");
            tmpVIPProgress.value = Mathf.Min(Mathf.Max((float)(tmpCurrVIPExp - tmpCurrNeedVIPExp) / (float)(tmpNextNeedVIPExp - tmpCurrNeedVIPExp), 0.0f), 1.0f);
            tmpLBVIPProgress = tmpCurrVIPExp.ToString() + "/" + tmpNextNeedVIPExp.ToString();
        }
        GameCommon.SetUIText(tmpVIPProgress.gameObject, "num_label", tmpLBVIPProgress);

        //距离下一等级充值数
        int tmpRemainVIPExp = tmpNextNeedVIPExp - tmpCurrVIPExp;
        GameObject tmpGORemain = GameCommon.FindObject(tmpVIPProgress.gameObject, "tips_label");
        tmpGORemain.SetActive(tmpCurrVIPLevel != tmpMaxVIPLevel);
        if (tmpCurrVIPLevel != tmpMaxVIPLevel)
        {
            string tmpStrRechargeNum = string.Format("再充值 [EA3030]{0}[-] 元宝可成为", tmpRemainVIPExp);
            GameCommon.SetUIText(tmpGORemain, "num", tmpStrRechargeNum);
            string tmpStrGORemainNum = string.Format("{0}级至尊", tmpNextVIPLevel);
			if(CommonParam.isOnLineVersion)
			{
				tmpStrGORemainNum = string.Format("VIP {0}", tmpNextVIPLevel);
			}
            GameCommon.SetUIText(tmpGORemain, "vip_level_num", tmpStrGORemainNum);
        }
        //added by xuke 红点相关
        if (mLastVipLevel != tmpCurrVIPLevel) 
        {
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_SHOP_VIP_GIFT,true);
			DEBUG.Log("支付完成内部实现88888---------");
            DataCenter.SetData("RECHARGE_WINDOW", "REFRESH_RECHARGE_CHECK_MARK", null);
            mLastVipLevel = tmpCurrVIPLevel;
        }
        //end
    }
}
