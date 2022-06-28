using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;

public class shop_gain_item_UI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
		Init();
	}

	public virtual void Init()
	{
		shop_gain_item_window itemInfoWindow = new shop_gain_item_window();
		DataCenter.Self.registerData("shop_gain_item_window", itemInfoWindow);

	}
	
	public void OnDestroy()
	{		
        //by chenliang
        //begin

//		DataCenter.Remove("shop_gain_item_window");
//---------------
        //不能移除shop_gain_item_window

        //end
	}
}


public class shop_gain_item_window : tWindow
{
	public PetData mPetData = null;
	
	public string mStrWindowName = "shop_gain_item_window";

	GameObject mItemInfoObj;
	UIGridContainer  mGrid;

    public ShopChouKaData mChoukaData = null;

	public override void Init ()
	{
        //by chenliang
        //begin

// 		mGameObjUI = GameCommon.FindUI(mStrWindowName);
// 
// 		mItemInfoObj = GameCommon.FindObject (mGameObjUI, "single_item_info");
// 		mGrid = GameCommon.FindObject (mGameObjUI, "grid").GetComponent<UIGridContainer>();
// 
// 		Close();
//--------------------
        //之前代码已废弃

        EventCenter.Self.RegisterEvent("but_shop_buy_again", new DefineFactoryLog<Button_Chouka_Buy_Again>());

        //end
	}
    //by chenliang
    //begin

    public override void Open(object param)
    {
        base.Open(param);

        mGameObjUI = GameCommon.FindUI(mStrWindowName);

        mItemInfoObj = GameCommon.FindObject(mGameObjUI, "single_item_info");
        mGrid = GameCommon.FindObject(mGameObjUI, "grid").GetComponent<UIGridContainer>();
		GameCommon.FindObject (mGameObjUI,"Scroll View").GetComponent<UIScrollView>().ResetPosition();
    }

    //end

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "SET_SELECT_ITEM_BY_MODEL_INDEX")
		{
			mItemInfoObj.SetActive (true);
			mGrid.MaxCount = 0;
            //by chenliang
            //begin

//			SetItem((DataRecord)objVal, mItemInfoObj);
//--------------------
            SetItem((ItemDataBase)objVal, mItemInfoObj);

            //end
		}
		if(keyIndex == "SET_SELECT_ITEMS_BY_MODEL_INDEX")
		{
			mItemInfoObj.SetActive (false);
            //by chenliang
            //begin

//			SetItemsByModelIndex((NiceTable)objVal);
//------------------
            mChoukaData = (ShopChouKaData)objVal;
            SetItemsByModelIndex((ShopChouKaData)objVal);

            //end
		}
        //by chenliang
        //begin

        if (keyIndex == "REFRESH_BUY_AGAIN_INFO")
        {
            if (objVal is ShopChouKaData)
            {
                __RefreshBuyAgainInfo((ShopChouKaData)objVal);
            }
        }
        if(keyIndex == "REFRESH_BUY_AGAIN_INFO_COLOR")
        {
            RefreshColor();
        }

        //end
	}


    //购买数量
    public void RefreshColor()
    {
        GameObject tmpGOBuyAgain = GetSub("but_shop_buy_again");
        //购买图标
        GameCommon.SetResIcon(tmpGOBuyAgain, "icon_sprite", mChoukaData.mCostTid, false, true);
        //购买数量
        int tmpLeftCount = PackageManager.GetItemLeftCount(mChoukaData.mCostTid);
        string _colorPrefix = mChoukaData.mCostCount > tmpLeftCount ? "[FF0000]" : "[FFFFFF]";
        GameCommon.SetUIText(tmpGOBuyAgain, "buy_count_icon_label", _colorPrefix + "x" + Mathf.Abs(mChoukaData.mCostCount).ToString());
        
    }
    //by chenliang
    //begin

// 	public void SetItemsByModelIndex(NiceTable itemData)
// 	{
// 		int num = 0;
// 		mGrid.MaxCount = itemData.GetAllRecord ().Count;
// 		foreach (KeyValuePair<int, DataRecord> r in itemData.GetAllRecord())
// 		{
// 			DataRecord re = r.Value;
// 			SetItem (re, mGrid.controlList[num]);
// 			num++;
// 		}
// 	}
// 
// 	public void SetItem(DataRecord dataRecord, GameObject obj)
// 	{
// 		int iItemType = dataRecord.getData("ITEM_TYPE");
// 		int iItemID = dataRecord.getData("ITEM_ID");
// 		int iItemCount = dataRecord.getData("ITEM_COUNT");
// 		int iRoleEquipElement = dataRecord.getData ("ELEMENT");
// 
// 		GameCommon.SetUIVisiable (obj, "num", true);
// 		if(iItemCount > 1) GameCommon.SetUIText (obj, "num", "x" + iItemCount.ToString ());
// 		else GameCommon.SetUIText (obj, "num", "");
// 		UISprite itemSprite = GameCommon.FindObject (obj , "item_icon").GetComponent<UISprite>();
// 		GameCommon.SetUIVisiable (obj, "role_equip_element", false);
// 		GameCommon.SetUIVisiable (obj, "star_level_label", false);
// 		GameCommon.SetUIVisiable (obj, "fragment_sprite", false);
//         GameCommon.SetStarLevelLabel(obj, 0);
// 
// 		GameObject effectFiveObj = GameCommon.FindObject (obj, "effect_five");
// 		if(effectFiveObj != null) effectFiveObj.SetActive (false);
// 		GameObject elementObj = GameCommon.FindObject (obj, "element");
// 		if(effectFiveObj != null) elementObj.SetActive (false);
// 
// //		MonoBehaviour.print ("ITEM_TYPE ==" + ((ITEM_TYPE)iItemType).ToString () + ";;ITEM_ID ==" + iItemID.ToString ());
// 
// 		switch(iItemType)
// 		{
// 		case (int)ITEM_TYPE.PET:
// 			//GameCommon.SetUIVisiable (obj, "star_level_label", true);
// 			GameCommon.SetPetIconWithElementAndStar (obj, "item_icon", "element", "star_level_label", iItemID);
// 			int iStarLevel = TableCommon.GetNumberFromActiveCongfig (iItemID, "STAR_LEVEL");
// 			if(iStarLevel == 5 && effectFiveObj != null)
// 			{
// 				effectFiveObj.SetActive (true);
// 			}
// 			break;
// 		case (int)ITEM_TYPE.PET_FRAGMENT:
// 			//GameCommon.SetUIVisiable (obj, "star_level_label", true);
// 			int iFragment = TableCommon.GetNumberFromFragment(iItemID,"ITEM_ID");
// 			GameCommon.SetPetIconWithElementAndStar (obj, "item_icon", "element", "star_level_label", iFragment);
// 			int iFragmentStarLevel = TableCommon.GetNumberFromActiveCongfig (iFragment, "STAR_LEVEL");
// 			if(iFragmentStarLevel == 5 && effectFiveObj != null)
// 			{
// 				effectFiveObj.SetActive (true);
// 			}
// 
// 			GameCommon.SetUIVisiable (obj, "fragment_sprite", iItemType == (int)ITEM_TYPE.PET_FRAGMENT ? true : false);
// 
// 			int elementIndex = TableCommon.GetNumberFromActiveCongfig(iFragment, "ELEMENT_INDEX");
// 			GameCommon.SetElementFragmentIcon (obj, "fragment_sprite", elementIndex);
// 			break;
// 		case (int)ITEM_TYPE.EQUIP:
// 			//GameCommon.SetUIVisiable (obj, "star_level_label", true);
// 			GameCommon.SetUIVisiable (obj, "num", false);
// 			GameCommon.SetUIVisiable (obj, "role_equip_element", true);
// //			GameCommon.SetElementIcon (obj, "small_element", iRoleEquipElement);
// //			GameCommon.SetEquipElementBgIcon (obj, "role_equip_element", iRoleEquipElement);
// 			GameCommon.SetEquipElementBgIcons(obj, "element", "role_equip_element", iItemID, iRoleEquipElement);
// 			//GameCommon.SetUIVisiable (obj, "star_level_label", true);
// 			GameCommon.SetEquipIcon (itemSprite, iItemID);
// 			iItemCount = TableCommon.GetNumberFromRoleEquipConfig (iItemID, "STAR_LEVEL");
// 			//GameCommon.SetUIText (obj, "star_level_label", iItemCount.ToString ());
//             GameCommon.SetStarLevelLabel(obj, iItemCount, "star_level_label");
// 			break;
// 		default :
// 			GameCommon.SetItemIcon (itemSprite, iItemType, iItemID);
// 			break;
// 		}
// 	}
//---------------------


    //------------------------
    //抽卡动画
    //------------------------
    //1.播放展示物品动画
    //2.检测是否需要展示
    //3.如果需要则暂停播放
    //4.播放完后继续播放
    //5.如果整个动画结束了则执行相应操作

    private int mCurItemIndex = 0;  //> 当前播放到的物品的索引
    private TweenPosition mTweenPos = null;
    private TweenRotation mTweenRot = null;
    private Vector3 mOriginPos = new Vector3(360,-100,0);

    /// <summary>
    /// 抽到的物品列表
    /// </summary>
    private List<ItemDataBase> mCardList = new List<ItemDataBase>();
    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="shopData"></param>
    private void InitInfo(ShopChouKaData shopData) 
    {
        mCurItemIndex = 0;
        mCardList.Clear();
        for (int i = 0, count = shopData.items.Length; i < count; ++i) 
        {
            mCardList.Add(shopData.items[i]);
            mGrid.controlList[i].SetActive(false);
        }
        mTweenPos = mGrid.GetComponent<TweenPosition>();
        mTweenRot = mGrid.GetComponent<TweenRotation>();

        GameCommon.SetUIVisiable(mGameObjUI, "btn_root",false);
    }
    /// <summary>
    /// 继续播放动画
    /// </summary>
    private void PlayAnimForward() 
    {
        if(mGrid == null)
            return;
        if(mCurItemIndex >= mGrid.MaxCount)
        {
            //整个动画结束
            GameCommon.SetUIVisiable(mGameObjUI, "btn_root", true);
        }
        else
        {
            GameObject _gridItem = mGrid.controlList[mCurItemIndex];
            Transform _targetTrans = _gridItem.transform;
            _gridItem.SetActive(true);
            // 动画配套声音
            DataRecord record = DataCenter.mEffectSound.GetRecord("ten_times_get_card");
            if(record != null) 
            {
                GameCommon.PlaySound(record.get("SOUND_FILE"), GameCommon.GetMainCamera().transform.position, record.get("SOUND_TYPE"));
            }
            if (mTweenPos != null) 
            {                
                mTweenPos.cachedTransform = _targetTrans;
                mTweenPos.from = mOriginPos;
                mTweenPos.to = GetToPos(mGrid,mCurItemIndex);
                mTweenPos.ResetToBeginning();
                mTweenPos.enabled = false;
                mTweenPos.onFinished.Add(new EventDelegate(() => { mPosAnimEnd = true; TweenAnimEndCallback(); }) { oneShot = true });

                GlobalModule.DoOnNextUpdate(2,()=>{mTweenPos.PlayForward();});
            }
            if (mTweenRot != null) 
            {
                mTweenRot.cachedTransform = _targetTrans;
                mTweenRot.ResetToBeginning();
                mTweenRot.enabled = false;
                mTweenRot.onFinished.Add(new EventDelegate(() => { mRotAnimEnd = true; TweenAnimEndCallback(); }) { oneShot = true });

                GlobalModule.DoOnNextUpdate(2,() => { mTweenRot.PlayForward(); });
            }  
        }    
    }

    private Vector3 GetToPos(UIGridContainer kGridContainer,int kCurIndex) 
    {
        if (kGridContainer == null)
            return Vector3.zero;
        int _maxPerLine = kGridContainer.MaxPerLine;
        if (UIGridContainer.Arrangement.Horizontal == kGridContainer.arrangement)
        {            
            float _posY = -1 * (kCurIndex / _maxPerLine) * kGridContainer.CellHeight;
            float _posX = (kCurIndex % _maxPerLine) * kGridContainer.CellWidth;
            return new Vector3(_posX, _posY, 0f);
        }
        else 
        {
            float _posX = -1 * (kCurIndex / _maxPerLine) * kGridContainer.CellHeight;
            float _posY = (kCurIndex % _maxPerLine) * kGridContainer.CellWidth;
            return new Vector3(_posX, _posY, 0f);
        }
    }
    private bool mPosAnimEnd = false;
    private bool mRotAnimEnd = false;
    /// <summary> 
    /// 抽到的物品的Tween动画结束后的回调
    /// </summary>
    private void TweenAnimEndCallback() 
    {
        if (!mPosAnimEnd || !mRotAnimEnd)       
            return;
        mPosAnimEnd = false;
        mRotAnimEnd = false;
        if (CheckCondition())
        {
            ItemDataBase _itemData = mCardList[mCurItemIndex];
            PetGainDeliverData _petGainData;
            _petGainData.mPetTid = _itemData.tid;
            _petGainData.mCallback = PlayAnimForward;
            DataCenter.OpenWindow("PET_GAIN_WINDOW", _petGainData);

            mCurItemIndex++;
        }
        else 
        {
            mCurItemIndex++;
            PlayAnimForward();
        }
    }
    /// <summary>
    /// 检测当前物品是否需要打开获得高品质符灵的窗口
    /// </summary>
    /// <returns></returns>
    private bool CheckCondition() 
    {
        ItemDataBase _itemData = mCardList[mCurItemIndex];
        return PetGainWindow.CheckPetGainQuality(_itemData.tid);
    }
    //----------------------

    public void SetItemsByModelIndex(ShopChouKaData shopData)
    {
        int tmpMaxCount = shopData.items.Length;
        mGrid.MaxCount = tmpMaxCount;
        for (int i = 0; i < tmpMaxCount; i++)
            SetItem(shopData.items[i], mGrid.controlList[i]);

        InitInfo(shopData);
        PlayAnimForward();
    }

    /// <summary>
    /// 判断是否显示图标上的闪光特效
    /// </summary>
    /// <param name="kTid"></param>
    /// <returns></returns>
    private bool IsShowEffect(int kTid) 
    {
        if (ITEM_TYPE.PET == PackageManager.GetItemTypeByTableID(kTid))
            return PetGainWindow.CheckPetGainQuality(kTid);
        else
            return true;
    }
    public void SetItem(ItemDataBase itemData, GameObject obj)
    {
        int iItemID = itemData.tid;
        int iItemType = (int)PackageManager.GetItemTypeByTableID(iItemID);
        int iItemCount = itemData.itemNum;
        int iRoleEquipElement = 0;//dataRecord.getData("ELEMENT");

        GameCommon.SetUIVisiable(obj, "num", true);
        if (iItemCount > 1) GameCommon.SetUIText(obj, "num", "x" + iItemCount.ToString());
        else GameCommon.SetUIText(obj, "num", "");
        UISprite itemSprite = GameCommon.FindObject(obj, "item_icon").GetComponent<UISprite>();
        GameCommon.SetUIVisiable(obj, "role_equip_element", false);
        GameCommon.SetUIVisiable(obj, "star_level_label", false);
        GameCommon.SetUIVisiable(obj, "fragment_sprite", false);
        GameCommon.SetStarLevelLabel(obj, 0);

        GameCommon.SetUIVisiable(obj, "ec_ui_tencard", IsShowEffect(itemData.tid));

        GameObject effectFiveObj = GameCommon.FindObject(obj, "effect_five");
        if (effectFiveObj != null) effectFiveObj.SetActive(false);
        GameObject elementObj = GameCommon.FindObject(obj, "element");
        if (effectFiveObj != null) elementObj.SetActive(false);

        //		MonoBehaviour.print ("ITEM_TYPE ==" + ((ITEM_TYPE)iItemType).ToString () + ";;ITEM_ID ==" + iItemID.ToString ());

        switch (iItemType)
        {
            case (int)ITEM_TYPE.PET:
                //GameCommon.SetUIVisiable (obj, "star_level_label", true);
                GameCommon.SetPetIconWithElementAndStar(obj, "item_icon", "element", "star_level_label", iItemID);
                int iStarLevel = TableCommon.GetNumberFromActiveCongfig(iItemID, "STAR_LEVEL");
                if (iStarLevel == 5 && effectFiveObj != null)
                {
                    effectFiveObj.SetActive(true);
                }
                break;
            case (int)ITEM_TYPE.PET_FRAGMENT:
                //GameCommon.SetUIVisiable (obj, "star_level_label", true);
                int iFragment = TableCommon.GetNumberFromFragment(iItemID, "ITEM_ID");
                GameCommon.SetPetIconWithElementAndStar(obj, "item_icon", "element", "star_level_label", iFragment);
                int iFragmentStarLevel = TableCommon.GetNumberFromActiveCongfig(iFragment, "STAR_LEVEL");
                if (iFragmentStarLevel == 5 && effectFiveObj != null)
                {
                    effectFiveObj.SetActive(true);
                }

                GameCommon.SetUIVisiable(obj, "fragment_sprite", iItemType == (int)ITEM_TYPE.PET_FRAGMENT ? true : false);

                int elementIndex = TableCommon.GetNumberFromActiveCongfig(iFragment, "ELEMENT_INDEX");
                GameCommon.SetElementFragmentIcon(obj, "fragment_sprite", elementIndex);
                break;
            case (int)ITEM_TYPE.EQUIP:
                //GameCommon.SetUIVisiable (obj, "star_level_label", true);
                GameCommon.SetUIVisiable(obj, "num", false);
                GameCommon.SetUIVisiable(obj, "role_equip_element", true);
                //			GameCommon.SetElementIcon (obj, "small_element", iRoleEquipElement);
                //			GameCommon.SetEquipElementBgIcon (obj, "role_equip_element", iRoleEquipElement);
                GameCommon.SetEquipElementBgIcons(obj, "element", "role_equip_element", iItemID, iRoleEquipElement);
                //GameCommon.SetUIVisiable (obj, "star_level_label", true);
                GameCommon.SetEquipIcon(itemSprite, iItemID);
                iItemCount = TableCommon.GetNumberFromRoleEquipConfig(iItemID, "STAR_LEVEL");
                //GameCommon.SetUIText (obj, "star_level_label", iItemCount.ToString ());
                GameCommon.SetStarLevelLabel(obj, iItemCount, "star_level_label");
                break;
            default:
                GameCommon.SetItemIcon(itemSprite, iItemType, iItemID);
                break;
        }
    }

    private void __RefreshBuyAgainInfo(ShopChouKaData shopData)
    {
        GameObject tmpGOBuyAgain = GetSub("but_shop_buy_again");

        //折扣
        GameCommon.SetUIVisiable(tmpGOBuyAgain, "discount_effect", false);

        //购买图标
        GameCommon.SetResIcon(tmpGOBuyAgain, "icon_sprite", shopData.mCostTid, false, true);
        //购买数量
        int tmpLeftCount = PackageManager.GetItemLeftCount(shopData.mCostTid);
        string _colorPrefix = shopData.mCostCount > tmpLeftCount ? "[FF0000]" : "[FFFFFF]";
        GameCommon.SetUIText(tmpGOBuyAgain, "buy_count_icon_label", _colorPrefix + "x" + Mathf.Abs(shopData.mCostCount).ToString());

        NiceData tmpBtnData = GameCommon.GetButtonData(tmpGOBuyAgain);
        if (tmpBtnData != null)
        {
            tmpBtnData.set("WINDOW_NAME", "shop_gain_item_window");
            tmpBtnData.set("SHOP_DATA", shopData);
        }
    }

    //end
}
	