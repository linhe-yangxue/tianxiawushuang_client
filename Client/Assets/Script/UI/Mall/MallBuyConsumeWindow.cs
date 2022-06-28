using UnityEngine;
using System.Collections;
using Logic;
using System;
using System.Collections.Generic;
using DataTable;

public enum ShopType
{
    Mall,       //商城
    Mystery,    //黑市
    PVP,        //竞技场商城
    Tower,      //爬塔
    Boss,       //天魔
    Guild       //公会
}

public class MallBuyConsumeData
{
    private ShopType mShopType; //商城类型
    private int mIndex;         //配置表中索引
    private int mTid;
    private int mHasBuyCount;   //已经买的数量
    private int mLeftBuyCount;  //剩余可购买数量，-1为无限制
    private int mCostTid;       //消耗品Tid
    private int mPrice;         //单价
    private bool mbUseRes = false;      //> 是否是使用道具
	private Action<int> mSureBuy;   //确定购买回调
    private Action<string, int> mComplete;   //购买完成回调

    public MallBuyConsumeData(ShopType shopType)
    {
        mShopType = shopType;
    }

    public ShopType shopType
    {
        get
        {
            return mShopType;
        }
    }
    public int Index
    {
        set { mIndex = value; }
        get { return mIndex; }
    }
    public int Tid
    {
        set { mTid = value; }
        get { return mTid; }
    }
    public int HasBuyCount
    {
        set { mHasBuyCount = value; }
        get { return mHasBuyCount; }
    }
    public int LeftBuyCount
    {
        set { mLeftBuyCount = value; }
        get { return mLeftBuyCount; }
    }
    public int CostTid
    {
        set { mCostTid = value; }
        get { return mCostTid; }
    }
    public int Price
    {
        set { mPrice = value; }
        get { return mPrice; }
    }
    public bool UseRes 
    {
        set { mbUseRes = value; }
        get { return mbUseRes; }
    }
    public Action<string, int> Complete
    {
        set { mComplete = value; }
        get { return mComplete; }
    }
	public Action<int> SureBuy
	{
		set { mSureBuy = value; }
		get { return mSureBuy; }
	}
}

/// <summary>
/// 集市道具商店购买界面
/// </summary>
public class MallBuyConsumeWindow : tWindow
{
    private int mBuyCount = 0;
    private MallBuyConsumeData mConsumeData;
    private Dictionary<ShopType, Dictionary<int, DataRecord>> mDicAllShopConfig = new Dictionary<ShopType, Dictionary<int, DataRecord>>();
    private Dictionary<int, DataRecord> mDicItemsConfig;  //道具信息字典

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_mall_gift_preview_add_btn_left", new DefineFactoryLog<Button_mall_gift_preview_add_btn_left>());
        EventCenter.Self.RegisterEvent("Button_mall_gift_preview_add_btn_right", new DefineFactoryLog<Button_mall_gift_preview_add_btn_right>());
        EventCenter.Self.RegisterEvent("Button_mall_gift_preview_ten_btn_left", new DefineFactoryLog<Button_mall_gift_preview_ten_btn_left>());
        EventCenter.Self.RegisterEvent("Button_mall_gift_preview_ten_btn_right", new DefineFactoryLog<Button_mall_gift_preview_ten_btn_right>());

        EventCenter.Self.RegisterEvent("Button_mall_gift_preview_close_btn", new DefineFactoryLog<Button_Mall_Close>());
        EventCenter.Self.RegisterEvent("Button_mall_gift_preview_cancel_btn", new DefineFactoryLog<Button_Mall_Close>());
        EventCenter.Self.RegisterEvent("Button_mall_gift_preview_ok_btn", new DefineFactoryLog<Button_mall_gift_preview_ok_btn>());

        if (mDicAllShopConfig.Count <= 0)
        {
            NiceTable tmpTable = null;
            mDicAllShopConfig[ShopType.Mall] = TableCommon.GetDataFromMallConfigByPage(1);
            tmpTable = TableManager.GetTable("MysteryShopConfig") as NiceTable;
            mDicAllShopConfig[ShopType.Mystery] = (tmpTable != null) ? tmpTable.GetAllRecord() : null;
            tmpTable = TableManager.GetTable("PrestigeShopConfig") as NiceTable;
            mDicAllShopConfig[ShopType.PVP] = (tmpTable != null) ? tmpTable.GetAllRecord() : null;
            tmpTable = TableManager.GetTable("TowerShopConfig");
            mDicAllShopConfig[ShopType.Tower] = (tmpTable != null) ? tmpTable.GetAllRecord() : null;
            tmpTable = TableManager.GetTable("DeamonShopConfig");
            mDicAllShopConfig[ShopType.Boss] = (tmpTable != null) ? tmpTable.GetAllRecord() : null;
            mDicAllShopConfig[ShopType.Guild] = DataCenter.mGuildShopConfig.GetAllRecord();
        }
    }

    public override void Open(object param)
    {
        base.Open(param);

        mBuyCount = 1;      //打开界面时购买数量为1
        mConsumeData = param as MallBuyConsumeData;
        mDicItemsConfig = __GetShopConfig(mConsumeData.shopType);

        //设置购买按钮数据
        NiceData tmpBtnBuyData = GameCommon.GetButtonData(mGameObjUI, "mall_gift_preview_ok_btn");
        if (tmpBtnBuyData != null)
        {
            tmpBtnBuyData.set("ITEM_INDEX", mConsumeData.Index);
            tmpBtnBuyData.set("ITEM_TID", mConsumeData.Tid);
            tmpBtnBuyData.set("IS_USE",mConsumeData.UseRes);
//			tmpBtnBuyData.set("ITEM_TID", mConsumeData.Tid);
        }

        Refresh(null);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "ADD_COUNT": __AddCount((int)objVal); break;
        }
    }

    private void SetTitle() 
    {
        if (mConsumeData == null)
            return;
        GameCommon.SetUIText(mGameObjUI, "title_label",mConsumeData.UseRes?"使用道具":"购买道具");
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="param">参数为MallBuyConsumeData</param>
    /// <returns></returns>
    public override bool Refresh(object param)
    {
        if (mConsumeData == null)
            return true;
        SetTitle();
        //道具信息
        GameObject tmpItemInfo = GetSub("itemIcon");
        int tmpLeftCount = PackageManager.GetItemLeftCount(mConsumeData.Tid);
        GameCommon.SetOnlyItemIcon(tmpItemInfo, mConsumeData.Tid);
        GameCommon.SetUIText(tmpItemInfo, "num_label", tmpLeftCount.ToString() + "[ccb38c]个");
		GameCommon.SetUIText (tmpItemInfo,"itemName",GameCommon.GetItemName(mConsumeData.Tid));

        if (!mConsumeData.UseRes) 
        {
            //消耗品图标
            GameObject tmpPrice = GetSub("price_label");
            GameCommon.SetResIcon(tmpPrice, "Sprite", mConsumeData.CostTid, false);
        }


        //购买信息
        __RefreshBuyInfo();

        //今日可购买
        GameObject tmpTodayCount = GetSub("today_label");
        if (!mConsumeData.UseRes)
        {
            tmpTodayCount.SetActive(mConsumeData.LeftBuyCount != -1);
            if (mConsumeData.LeftBuyCount != -1)
                GameCommon.SetUIText(tmpTodayCount, "num_label", mConsumeData.LeftBuyCount.ToString() + "[ccb38c]个");
        }
        else 
        {
            tmpTodayCount.SetActive(false);
        }
        return true;
    }

    /// <summary>
    /// 获取指定商城数据配置
    /// </summary>
    /// <param name="shopType"></param>
    /// <returns></returns>
    private Dictionary<int, DataRecord> __GetShopConfig(ShopType shopType)
    {
        if (mDicAllShopConfig == null || mDicAllShopConfig.Count <= 0)
            return null;

        Dictionary<int, DataRecord> tmpDic = null;
        if (!mDicAllShopConfig.TryGetValue(shopType, out tmpDic))
            return null;

        return tmpDic;
    }

    /// <summary>
    /// 刷新购买信息
    /// </summary>

	private void __RefreshBuyInfo()
	{
		//购买数量
		GameObject tmpBuyCount = GetSub("num_bg");
		GameCommon.SetUIText(tmpBuyCount, "num", mBuyCount.ToString());
		
		//总价
		int tmpPrice = 0;
        if (!mConsumeData.UseRes) 
        {
            // By XiaoWen
            // Begin
            DataRecord tmpConfig = mDicItemsConfig[mConsumeData.Index];
            string totalPrice = tmpConfig["COST_NUM_1"].ToString();
            string[] prices = totalPrice.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < mBuyCount; i++)
            {
                int index = mConsumeData.HasBuyCount + i;
                if (index >= prices.Length - 1)
                {
                    index = prices.Length - 1;
                }
                tmpPrice += int.Parse(prices[index]);
            }
            //End
        }
//		Dictionary<int, DataRecord> tmpDicItemsConfig = TableCommon.GetDataFromMallConfigByPage(1);
//		for (int i = 0; i < mBuyCount; i++)
//		{
//			DataRecord tmpConfig = tmpDicItemsConfig[mConsumeData.Index];
//			string tmpStrTotalPrice = tmpConfig["COST_NUM_1"].ToString();
//			string[] tmpStrPrices = tmpStrTotalPrice.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
//			int tmpPriceIndex = mConsumeData.HasBuyCount + i;
//			tmpPrice += int.Parse((tmpPriceIndex >= tmpStrPrices.Length) ? tmpStrPrices[tmpStrPrices.Length - 1] : tmpStrPrices[tmpPriceIndex]);
//		}
		DataCenter.SetData("MALL_BUY_CONSUME_WINDOW", "MALL_BUY_CONSUME_WINDOW_CURRENT_PRICE", tmpPrice);
		GameObject tmpTotalPrice = GetSub("price_label");

        tmpTotalPrice.SetActive(!mConsumeData.UseRes);
        GameCommon.SetUIText(tmpTotalPrice, "num_label", tmpPrice.ToString());

		
		//设置购买按钮数据
		NiceData tmpBtnBuyData = GameCommon.GetButtonData(mGameObjUI, "mall_gift_preview_ok_btn");
		if (tmpBtnBuyData != null)
		{
			tmpBtnBuyData.set("ITEM_COUNT", mBuyCount);
			tmpBtnBuyData.set("COST_TID", mConsumeData.CostTid);
			tmpBtnBuyData.set("COST_TOTAL_PRICE", tmpPrice);
		}
	}

    /// <summary>
    /// 加物品数量，可为负
    /// </summary>
    /// <param name="count"></param>
    private void __AddCount(int count)
    {
        mBuyCount += count;
        mBuyCount = Mathf.Max(mBuyCount, 0);
        if (!mConsumeData.UseRes)
        {
            if (mConsumeData.LeftBuyCount != -1)
                mBuyCount = Mathf.Min(mBuyCount, mConsumeData.LeftBuyCount);
        }
        else 
        {
            mBuyCount = Mathf.Min(mBuyCount, PackageManager.GetItemLeftCount(mConsumeData.Tid));
        }
        __RefreshBuyInfo();
    }

    /// <summary>
    /// 购买结束时调用
    /// </summary>
    /// <param name="text"></param>
    public void OnBuyComplete(string text, int buyCount)
    {
        if (mConsumeData != null && mConsumeData.Complete != null)
            mConsumeData.Complete(text, buyCount);
    }
	/// <summary>
	/// 确定购买开始回调
	/// </summary>
	/// <param name="text"></param>
	public void OnSureBuy(int buyCount)
	{
		if (mConsumeData != null && mConsumeData.Complete != null)
			mConsumeData.SureBuy(buyCount);
	}
}

/// <summary>
/// 物品减1
/// </summary>
class Button_mall_gift_preview_add_btn_left : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("MALL_BUY_CONSUME_WINDOW", "ADD_COUNT", -1);

        return true;
    }
}
/// <summary>
/// 物品加1
/// </summary>
class Button_mall_gift_preview_add_btn_right : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("MALL_BUY_CONSUME_WINDOW", "ADD_COUNT", 1);

        return true;
    }
}
/// <summary>
/// 物品减10
/// </summary>
class Button_mall_gift_preview_ten_btn_left : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("MALL_BUY_CONSUME_WINDOW", "ADD_COUNT", -10);

        return true;
    }
}
/// <summary>
/// 物品加10
/// </summary>
class Button_mall_gift_preview_ten_btn_right : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("MALL_BUY_CONSUME_WINDOW", "ADD_COUNT", 10);

        return true;
    }
}

/// <summary>
/// 确定购买
/// </summary>
class Button_mall_gift_preview_ok_btn : CEvent
{
    public override bool _DoEvent()
    {
        int tmpIndex = (int)getObject("ITEM_INDEX");
        int tmpCount = (int)getObject("ITEM_COUNT");
        bool _isUse = (bool)getObject("IS_USE");
        if (tmpCount <= 0 && !_isUse)
        {
            //数量必须大于0
          	DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SHOP_ITEM_BUY_COUNT_LOW);
            return true;
        }

        if (!_isUse)
        {
            int tmpCostTid = (int)getObject("COST_TID");
            int tmpCostTotalPrice = (int)getObject("COST_TOTAL_PRICE");
            int tmpCostLeft = PackageManager.GetItemLeftCount(tmpCostTid);
            if (tmpCostTotalPrice > tmpCostLeft)
            {
                //string tmpCostName = GameCommon.GetItemName(tmpCostTid);
                //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_MYSTERIOUS_NO_ENGOUGH_COST, tmpCostName);
                //NewShopWindow.ToGetDiamond("MALL_BUY_CONSUME_WINDOW");

                if (tmpCostTid == (int)ITEM_TYPE.YUANBAO)
                    GameCommon.ToGetDiamond();
                else
                    DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_MYSTERIOUS_NO_ENGOUGH_COST, GameCommon.GetItemName(tmpCostTid));
                return true;
            }
            MallBuyConsumeWindow tmpWin = DataCenter.GetData("MALL_BUY_CONSUME_WINDOW") as MallBuyConsumeWindow;
            if (tmpWin != null)
                tmpWin.OnSureBuy(tmpCount);

            //        ShopNetEvent.RequestShopPurchase(tmpIndex, tmpCount, __OnRequestSuccess, __OnRequestFailed);
            Debug.Log("Button_mall_gift_preview_ok_btn");
        }
        else 
        {
            if (tmpCount <= 0) 
            {
                DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_ITEM_USE_ZERO);
                return true;
            }
            MallBuyConsumeWindow tmpWin = DataCenter.GetData("MALL_BUY_CONSUME_WINDOW") as MallBuyConsumeWindow;
            if (tmpWin != null)
                tmpWin.OnSureBuy(tmpCount);
        }
        return true;
    }
//    private void __OnRequestSuccess(string text)
//    {
//        SC_ResponseShopPurchase tmpResp = JCode.Decode<SC_ResponseShopPurchase>(text);
//
//        if (tmpResp.isBuySuccess == 1)
//        {
//            DataCenter.CloseWindow("MALL_BUY_CONSUME_WINDOW");
//
//            MallBuyConsumeWindow tmpWin = DataCenter.GetData("MALL_BUY_CONSUME_WINDOW") as MallBuyConsumeWindow;
//            if (tmpWin != null)
//                tmpWin.OnBuyComplete(text, (int)getObject("ITEM_COUNT"));
//        }
//    }
//    private void __OnRequestFailed(string text)
//    {
//        switch (text)
//        {
//            case "1113":
//                {
//                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SHOP_ITEM_MORE_THAN_LIMIT);
//                } break;
//        }
//    }
}

/// <summary>
/// 关闭购买界面
/// </summary>
class Button_Mall_Close : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("MALL_BUY_CONSUME_WINDOW");

        MallBuyConsumeWindow tmpWin = DataCenter.GetData("MALL_BUY_CONSUME_WINDOW") as MallBuyConsumeWindow;
        if (tmpWin != null)
            tmpWin.OnBuyComplete("cancel", 0);

        return true;
    }
}
