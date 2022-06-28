using UnityEngine;
using System.Collections;
using Logic;
using System.Diagnostics;
using DataTable;
using System.Collections.Generic;
using System;

public enum RESOURCE_HINT_TYPE 
{
    NONE,
    BEATDEMONCARD = (int)ITEM_TYPE.BEATDEMONCARD_ITEM, //> 伏魔令
    STAMINA_ITEM = (int)ITEM_TYPE.STAMINA_ITEM,        //> 体力丹
    SPIRIT_ITEM = (int)ITEM_TYPE.SPIRIT_ITEM,          //> 精力丹
}

/// <summary>
/// 资源购买和使用的弹窗数据
/// </summary>
public class ResourceHintData : ItemDataBase
{
    private RESOURCE_HINT_TYPE mResourceType;

    public RESOURCE_HINT_TYPE ResourceType
    {
        set { mResourceType = value; }
        get { return mResourceType; }
    }
}


/// <summary>
/// 资源	Tid、购买资源sIndex管理
/// </summary>
//by chenliang
//begin

// public class ResourceHintID : Singleton<ResourceHintID>
// {
//----------------
    //不继承Singleton
public class ResourceHintID
{
    private static ResourceHintID msInstance;
    public static ResourceHintID Instace
    {
        get
        {
            if (msInstance == null)
                msInstance = new ResourceHintID();
            return msInstance;
        }
    }

//end
	private Dictionary<RESOURCE_HINT_TYPE, int> mDicResID = new Dictionary<RESOURCE_HINT_TYPE, int>();
	private Dictionary<int, int> mDicSIndex = new Dictionary<int, int>();
	
	private ResourceHintID()
	{
        mDicResID.Add(RESOURCE_HINT_TYPE.BEATDEMONCARD,(int)ITEM_TYPE.BEATDEMONCARD_ITEM); //> 降魔令
        mDicResID.Add(RESOURCE_HINT_TYPE.STAMINA_ITEM, (int)ITEM_TYPE.STAMINA_ITEM);       //> 体力丹
        mDicResID.Add(RESOURCE_HINT_TYPE.SPIRIT_ITEM, (int)ITEM_TYPE.SPIRIT_ITEM);         //> 精力丹
		__InitSIndex();
	}
	
	private void __InitSIndex()
	{
		foreach (KeyValuePair<RESOURCE_HINT_TYPE, int> pair in mDicResID)
		{
			int tmpTid = pair.Value;
			DataRecord tmpConfig = null;
			foreach (KeyValuePair<int, DataRecord> tmpPair in DataCenter.mMallShopConfig.GetAllRecord())
			{
				int tmpResTid = (int)tmpPair.Value.getObject("ITEM_ID");
				if (tmpResTid == tmpTid)
				{
					tmpConfig = tmpPair.Value;
					break;
				}
			}
			if (tmpConfig == null)
				continue;
			int tmpSIndex = (int)tmpConfig.getObject("INDEX");
			mDicSIndex.Add(tmpTid, tmpSIndex);
		}
	}
	
	public int this[RESOURCE_HINT_TYPE resType]
	{
		get
		{
			if (resType == RESOURCE_HINT_TYPE.NONE || mDicResID == null || mDicResID.Count <= 0)
				return 0;
			int resID = 0;
			if (!mDicResID.TryGetValue(resType, out resID))
				return 0;
			return resID;
		}
	}
	public int this[int resTid]
	{
		get
		{
			if (mDicSIndex == null)
				return 0;
			int sIndex = 0;
			if (!mDicSIndex.TryGetValue(resTid, out sIndex))
				return 0;
			return sIndex;
		}
	}
}



public class ResourceHintWindow : tWindow
{
	private ResourceHintData mCurResourceData;
	
	public override void Init()
	{
		base.Init();
		
		EventCenter.Self.RegisterEvent("Button_add_vitality_up_close_btn",new DefineFactory<AddVitalityUpCloseBtn>());
		EventCenter.Self.RegisterEvent("Button_add_vitality_up_buy_btn", new DefineFactory<AddVitalityUpBuyBtn>());
		EventCenter.Self.RegisterEvent("Button_add_vitality_up_use_btn",new DefineFactory<AddVitalityUpUseBtn>());
    }




    public override void Open(object param)
    {
        base.Open(param);

        Refresh(param);
    }

    public override void OnOpen()
    {
        base.OnOpen();


    }
	
	private string GetResourceTipInfoByType(RESOURCE_HINT_TYPE kResourceType)
	{
		string _hintInfo = "";
		switch (kResourceType) 
		{
		    case RESOURCE_HINT_TYPE.BEATDEMONCARD:
			    _hintInfo = "少侠,目前你的降魔次数不足,使用降魔令增加降魔次数吧!";
			break;
            case RESOURCE_HINT_TYPE.STAMINA_ITEM:
                _hintInfo = "少侠，目前你的体力不足,使用体力丹增加体力值吧!";
            break;
            case RESOURCE_HINT_TYPE.SPIRIT_ITEM:
                _hintInfo = "少侠,目前你的精力不足,使用精力丹增加精力值吧!";
            break;
		    default:
			break;
		}
		return _hintInfo;
	}

	//根据资源数据设置相关按钮的数据
	private void SetResBtnData(ResourceHintData kResData)
	{
		int tmpResSIndex = ResourceHintID.Instace[kResData.tid];
		this ["ResItemIndex"] = tmpResSIndex;
		DataRecord tmpShopConfig = DataCenter.mMallShopConfig.GetRecord(tmpResSIndex);
		if (tmpShopConfig == null) 
		{
			ResourceHintWindow.LogError ("找不到商店表" + tmpResSIndex.ToString () + "数据");
		}
		else 
		{
			GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "add_vitality_up_buy_btn")).set("RES_DATA", kResData);
			GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "add_vitality_up_use_btn")).set("RES_DATA", kResData);

			ResourceHintNetManager.RequestBuyResourceNumToken(tmpResSIndex);
		}
	}

	// 设置当前资源还可以购买的次数和资源价格
	private void SetUIInfo(List<ShopPropData> kShopPropData)
	{
		GameObject _buyObj = GameCommon.FindObject (mGameObjUI,"add_vitality_up_buy_btn");
		GameObject _useObj = GameCommon.FindObject (mGameObjUI,"add_vitality_up_use_btn");
		
		DataRecord _record = DataCenter.mMallShopConfig.GetRecord (int.Parse(this["ResItemIndex"].ToString()));
		string[] tmpBuyCount = _record.getObject("BUY_NUM").ToString().Split('|');
		
		int _canBuyNum = 0;
		GameCommon.GetButtonData(GetSub("add_vitality_up_buy_btn")).set("HAVE_BUY_NUM",true);
		int tmpStrCountByVIP = int.Parse (tmpBuyCount [0]);
		// 设置还可购买数量
		// 是否没有购买上限
		if (tmpBuyCount.Length == 1 && tmpBuyCount[0].Equals ("0")) 
		{
			GameCommon.SetUIText (_buyObj, "num", "N/A");
		}
		else 
		{
			//还可购买的次数 = 根据vip等级获得总共可以购买的次数 - 已经购买的次数
			if(RoleLogicData.Self.vipLevel < tmpBuyCount.Length)
				tmpStrCountByVIP = int.Parse(tmpBuyCount[RoleLogicData.Self.vipLevel]);
			
			if(kShopPropData.Count != 0)
			{
				_canBuyNum = tmpStrCountByVIP - kShopPropData[0].buyNum;	
			}
			else
			{
				_canBuyNum = tmpStrCountByVIP;
			}
			if(_canBuyNum == 0)
				GameCommon.GetButtonData(GetSub("add_vitality_up_buy_btn")).set("HAVE_BUY_NUM",false);
			GameCommon.SetUIText (_buyObj,"num",_canBuyNum.ToString());
		}		
		// 设置购买价格
		string totalPrice = _record["COST_NUM_1"].ToString();
		string[] prices = totalPrice.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
		int tmpBuyPrice = int.Parse(prices[0]);
		// 根据已经购买次数来设置当前购买价格
		int _buyPriceIndex = 0;
		if (kShopPropData.Count != 0) 
		{
			_buyPriceIndex = kShopPropData [0].buyNum > prices.Length ? prices.Length - 1 : kShopPropData [0].buyNum;
            if (kShopPropData[0].buyNum >= prices.Length)
            {
                tmpBuyPrice = int.Parse(prices[prices.Length - 1]); 
            }
            else
            {
                tmpBuyPrice = int.Parse(prices[kShopPropData[0].buyNum]);
            }
		}		
		GameCommon.SetUIText(GetSub ("info_label"),"num_label", "x" + tmpBuyPrice.ToString());		
		// 设置当前拥有资源数量
		GameCommon.SetUIText (GameCommon.FindObject(_useObj,"tips_label"),"num",ConsumeItemLogicData.Self.GetDataByTid(mCurResourceData.tid).itemNum.ToString());
		// 设置按钮数据
		GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "add_vitality_up_buy_btn")).set("RES_COST",tmpBuyPrice);
	}
	
	public override bool Refresh(object param)
    {
		if(param != null)
        	mCurResourceData = param as ResourceHintData;
		
        //当前资源图标
		GameCommon.SetItemIcon(GetSub("item_icon"), new ItemData() { mID = mCurResourceData.tid, mType = (int)PackageManager.GetItemTypeByTableID(mCurResourceData.tid) });
        //当前资源名称
		GameCommon.SetUIText(GetSub("item_icon"), "vitality_label", GameCommon.GetItemName(mCurResourceData.tid));
		//设置标题
        if (mCurResourceData.tid == (int)ITEM_TYPE.BEATDEMONCARD_ITEM)
        {
            GameCommon.SetUIText (mGameObjUI,"title_label",GameCommon.GetItemName(mCurResourceData.tid) + " 购买");
        }
        else
        {
            if (mCurResourceData.tid == (int)ITEM_TYPE.GOLD)
            {
                DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
            }
            else
            {
                GameCommon.SetUIText(mGameObjUI, "title_label", GameCommon.GetItemName(mCurResourceData.tid) + " 不足");
            }
        }
		//提示信息
		GameCommon.SetUIText(GetSub("info_label"),"tips_label",GetResourceTipInfoByType(mCurResourceData.ResourceType));
		
		SetResBtnData (mCurResourceData);
        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if ("SHOW_WINDOW" == keyIndex)
        {
			mCurResourceData = (ResourceHintData)objVal;
        }
		switch (keyIndex) 
		{
		case "REFRESH_BEATDEMON_COUNT":
			if(GameCommon.FindObject(GameCommon.FindObject (mGameObjUI,"add_vitality_up_use_btn"),"tips_label")==null)
				break;
			GameCommon.SetUIText (GameCommon.FindObject(GameCommon.FindObject (mGameObjUI,"add_vitality_up_use_btn"),"tips_label"),"num",ConsumeItemLogicData.Self.GetDataByTid(mCurResourceData.tid).itemNum.ToString());
			break;
		case "ALREADY_BUY_NUM":
			if(objVal != null)
			{
				List<ShopPropData> _propDataList = objVal as List<ShopPropData>;
				SetUIInfo(_propDataList);
			}
			break;
		}
    }
	
    public override void Close()
    {
        base.Close();
    }

	public static void LogError(string log)
	{
		DEBUG.LogError("ResHintWindow - " + log);
	}
	public static void Log(string log)
	{
		DEBUG.Log("ResHintWindow - " + log);
	}
	
	#region 按钮事件
	/// <summary>
    /// 关闭弹窗
    /// </summary>
    public class AddVitalityUpCloseBtn : CEvent 
    {
        public override bool _DoEvent()
        {
            DataCenter.CloseWindow("ADD_VITALITY_UP_WINDOW");

            return true;
        }
    }
    /// <summary>
    /// 购买
    /// </summary>
    public class AddVitalityUpBuyBtn : CEvent 
    {
		public override bool _DoEvent()
		{

			int cost = (int)getObject("RES_COST");
			bool haveBuyNum = (bool)getObject ("HAVE_BUY_NUM");
			if (!haveBuyNum)
			{
				DataCenter.OpenMessageWindow ("超过最大购买上限!");
				return true;
			}
			if (RoleLogicData.Self.diamond < cost)
			{
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_NO_ENOUGH_DIAMOND);
				return true;
			}
			
			ResourceHintData tmpPeace = getObject("RES_DATA") as ResourceHintData;
			ResourceHintNetManager.RequestBuyResourceToken(cost, tmpPeace.tid, ResourceHintID.Instace[tmpPeace.tid], 1);
			
			return true;
		}
	}
	
	/// <summary>
	/// 使用资源
	/// </summary>
	public class AddVitalityUpUseBtn : CEvent 
    {
        private ResourceHintData mResData = null;
        public override bool _DoEvent()
        {
			ResourceHintData tmpResData = getObject("RES_DATA") as ResourceHintData;
			ConsumeItemData tmpResItemData = ConsumeItemLogicData.Self.GetDataByTid(tmpResData.tid);
			if (tmpResItemData == null || tmpResItemData.itemNum <= 0)
			{
                if (tmpResData.tid == (int)ITEM_TYPE.GOLD)
                {
                    DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
                }
                else
                {
                    DataCenter.OpenMessageWindow(GameCommon.GetItemName(tmpResData.tid) + "不足!");
                }
				return true;
			}
			else
				__OnAction();			
			return true;
        }
        
		private void __OnAction()
		{
            mResData = getObject("RES_DATA") as ResourceHintData;
            ConsumeItemData tmpResItemData = ConsumeItemLogicData.Self.GetDataByTid(mResData.tid);
			ItemDataBase tmpItemData = new ItemDataBase();
            tmpItemData.tid = mResData.tid;
			tmpItemData.itemNum = 1;
			//设置itemId
            if (mResData != null)
				tmpItemData.itemId = tmpResItemData.itemId;
			
			RequestUseResItem (tmpItemData);
		}

		private void RequestUseResItem(ItemDataBase kItemData)
		{
             ResourceHintNetManager.RequestUseItemToken(kItemData, mResData.ResourceType);	
		}

		private void OpenMessageOkWindow(STRING_INDEX stringIndex, string addInfo, string windowName, System.Action onClickOk)
		{
			int index = (int)stringIndex;
			string showText = "";
			DataRecord listInfo = DataCenter.mStringList.GetRecord(index);
			if (listInfo != null)
			{
				showText = listInfo.get("STRING_CN");
				showText = string.Format(showText, addInfo);
			}
			DataCenter.OpenWindow("MESSAGE_WINDOW", showText);
			DataCenter.SetData("MESSAGE_WINDOW", "WINDOW_SEND", windowName);
			ObserverCenter.Add("MESSAGE_OK", onClickOk);
		}
    }

#endregion
}

