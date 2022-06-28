using UnityEngine;
using System.Collections;
using Logic;

public class shop_gain_pet_UI : GainPetInfoUI
{
    public override void Init()
    {
		shop_gain_pet_window petInfoWindow = new shop_gain_pet_window();
		DataCenter.Self.registerData("shop_gain_pet_window", petInfoWindow);
		
		petInfoWindow.mPetStarLevelLabel = mPetStarLevelLabel;
		petInfoWindow.mPetTitleLabel = mPetTitleLabel;
		petInfoWindow.mPetNameLabel = mPetNameLabel;
		petInfoWindow.mCard = mCard;
		petInfoWindow.mRoleMastButtonUI = mRoleMastButtonUI;
        petInfoWindow.Close();
    }

//	public override void InitCard ()
//	{
//		GameObject obj = GameCommon.LoadUIPrefabs("shop_card_group_window", mCard.name);
//		if(obj != null)
//		{
//			CardGroupUI uiScript = obj.GetComponent<CardGroupUI>();
//			uiScript.InitPetInfo(mCard.name, mfCardScale, gameObject);
//		}
//		mCard.transform.localScale = mCard.transform.localScale * mfCardScale;
//	}

	public void OnDestroy()
	{		
        //by chenliang
        //begin

//		DataCenter.Remove("shop_gain_pet_window");
//----------------
        //不能移除shop_gain_pet_window

        //end
	}
}


public class shop_gain_pet_window : GainPetInfoWindow
{
    public ShopChouKaData mChoukaData = null;
    public shop_gain_pet_window()
    {
        m_strWindowName = "shop_gain_pet_window";
    }
    //by chenliang
    //begin

    private bool mIsOpened = false;
    private GameObject mCardGroupWindow;

    public override void Init()
    {
        EventCenter.Self.RegisterEvent("but_shop_buy_again", new DefineFactoryLog<Button_Chouka_Buy_Again>());
    }

    public override void Open(object param)
    {
        base.Open(param);

        if (!mIsOpened)
        {
            mIsOpened = true;

            InitCard();
            InitUI();
        }
    }
    public override void OnClose()
    {
        mIsOpened = false;
        if (mCardGroupWindow != null)
        {
            GameObject.DestroyImmediate(mCardGroupWindow);
            mCardGroupWindow = null;
        }

        base.OnClose();
    }

    public virtual void InitCard()
    {
        mCard = GetSub("gain_pet_window_card");

        if (mCardGroupWindow != null)
        {
            GameObject.DestroyImmediate(mCardGroupWindow);
            mCardGroupWindow = null;
        }
        mCardGroupWindow = GameCommon.LoadAndIntanciateUIPrefabs("card_group_window_get_card", mCard.name);
        mCardGroupWindow.transform.parent = mCard.transform;
        mCardGroupWindow.transform.localPosition = new Vector3(0, 0, 0);
        mCardGroupWindow.transform.localScale = new Vector3(1, 1, 1);
        if (mCardGroupWindow != null)
        {
            CardGroupUI uiScript = mCardGroupWindow.GetComponent<CardGroupUI>();
            uiScript.InitPetInfo(mCard.name, 1.3f, mGameObjUI);
        }
    }

    public virtual void InitUI()
    {
        GameObject tmpInfoParent = GetSub("pet_info");
        mPetStarLevelLabel = GameCommon.FindComponent<UILabel>(tmpInfoParent, "StarLevel");
        mPetTitleLabel = GameCommon.FindComponent<UILabel>(tmpInfoParent, "Title");
        mPetNameLabel = GameCommon.FindComponent<UILabel>(tmpInfoParent, "Name");
//        mCard = mCard;
//        mRoleMastButtonUI = null;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
            case "REFRESH_BUY_AGAIN_INFO":
                { 
                    __RefreshBuyAgainInfo(objVal as ShopChouKaData);
                }break;
            case "REFRESH_BUY_AGAIN_INFO_COLOR":
                {
                    RefreshColor();
                }break;
        }
    }

    private void __RefreshBuyAgainInfo(ShopChouKaData shopData)
    {
        mChoukaData = shopData;

        GameObject tmpGOBuyAgain = GetSub("but_shop_buy_again");

        //折扣
        GameCommon.SetUIVisiable(tmpGOBuyAgain, "discount_effect", false);

        //购买图标
		GameCommon.SetResIcon (tmpGOBuyAgain,"icon_sprite",shopData.mCostTid,false, true);
        //购买数量
        int tmpLeftCount = PackageManager.GetItemLeftCount(shopData.mCostTid);
		string _colorPrefix = shopData.mCostCount > tmpLeftCount ? "[FF0000]":"[FFFFFF]";
		GameCommon.SetUIText(tmpGOBuyAgain, "buy_count_icon_label", _colorPrefix + "x"+Mathf.Abs( shopData.mCostCount).ToString());

        NiceData tmpBtnData = GameCommon.GetButtonData(tmpGOBuyAgain);
        if (tmpBtnData != null)
        {
            tmpBtnData.set("WINDOW_NAME", "shop_gain_pet_window");
            tmpBtnData.set("SHOP_DATA", shopData);
        }
    }

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

    //end
}
