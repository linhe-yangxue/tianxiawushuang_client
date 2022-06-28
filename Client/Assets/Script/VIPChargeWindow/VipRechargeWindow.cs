using UnityEngine;
using System.Collections;
using Logic;

/// <summary>
/// 充值可获得物品类型
/// </summary>
public enum RechargeForType
{
	NONE,
	PET_BAG_LIMIT,	//> 增加符灵背包上限
    RESET_ADVENTURE,//> 重置关卡
}

/// <summary>
/// 打开去充值弹窗界面时需要传入的数据
/// </summary>
public class VipRechargeWindowOpenData
{
	// 充值后可获得的物品类型
	private RechargeForType mRechargeForType;
	public RechargeForType RechargeForType
	{
		get{return mRechargeForType;}
		set{mRechargeForType = value;}
	}


}
/// <summary>
/// 去充值窗口
/// </summary>
public class VipRechargeWindow : tWindow
{
	private VipRechargeWindowOpenData mVipRechargeWindowOpenData;

	public override void Init()
	{
		EventCenter.Register ("Button_vip_recharge_conside_btn",new DefineFactory<Button_vip_recharge_conside_btn>());
		EventCenter.Register ("Button_vip_recharge_up_btn", new DefineFactory<Button_vip_recharge_up_btn> ());
	}

	public override void Open(object param)
	{
		base.Open (param);
		if (param != null && param is VipRechargeWindowOpenData) 
		{
			mVipRechargeWindowOpenData = param as VipRechargeWindowOpenData;
		}

		Refresh (null);
		ShowUIInfo ();
	}

	private void ShowUIInfo()
	{
		// 是否达到了最高VIP等级
		bool _isMaxVipLvl = false;
		_isMaxVipLvl = RoleLogicData.Self.vipLevel ==  VIPHelper.GetMaxVIPLevel() ? true : false;
		int _nextVipLvl = RoleLogicData.Self.vipLevel + 1;

		switch (mVipRechargeWindowOpenData.RechargeForType) 
		{
			case RechargeForType.PET_BAG_LIMIT:	//> 增加符灵背包上限   //新增装备区背包上限
				if(_isMaxVipLvl)
                    GameCommon.SetUIText(mGameObjUI, "tips_label", TableCommon.getStringFromStringList(STRING_INDEX.RECHAGE_EXTEND_BAG_MAX));
				else
                    GameCommon.SetUIText(mGameObjUI, "tips_label", TableCommon.getStringFromStringList(STRING_INDEX.RECHAGE_EXTEND_BAG_TIPS));
				break;
            case RechargeForType.RESET_ADVENTURE:
                int _nextVipLv = GameCommon.GetNextVipLevel();
                int _resetTimes = TableCommon.GetNumberFromVipList(_nextVipLv, "COPYRESET_NUM");
                GameCommon.SetUIText(mGameObjUI, "tips_label", "将军,今日可重置次数已达上限。升级到[99ff66]VIP" + _nextVipLv.ToString() + "[-]可重置关卡[99ff66]" + _resetTimes.ToString() + "[-]次");
                    break;
			default:
				GameCommon.SetUIText(mGameObjUI,"tips_label","TEST");
				break;
		}
	}

	public override bool Refresh(object param)
	{
		return true;
	}


	public override void onChange(string keyIndex,object objVal)
	{
		base.onChange (keyIndex,objVal);
		switch(keyIndex)
		{
			default:
				break;
		}

	}
}

// 再想想按钮
public class Button_vip_recharge_conside_btn : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.SetData ("VIP_RECHARGE_UP_WINDOW","CLOSE","");
		return true;
	}

}

public class Button_vip_recharge_up_btn :CEvent 
{
	public override bool _DoEvent()
	{
		GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE ,CommonParam.rechageDepth);
		return true;
	}
}






