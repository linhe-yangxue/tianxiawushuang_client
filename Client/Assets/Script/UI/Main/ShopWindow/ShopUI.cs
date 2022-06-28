using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;
using Utilities;

public class ShopUI : MonoBehaviour {
	
	void Start () {
		DataCenter.RegisterData("SHOP_WINDOW", new NewShopWindow(gameObject));

		EventCenter.Self.RegisterEvent("Button_but_shop_buy_common_one", new DefineFactory<Button_shop_buy_common_one>());
		EventCenter.Self.RegisterEvent("Button_but_shop_buy_common_ten", new DefineFactory<Button_but_shop_buy_common_ten>());
		EventCenter.Self.RegisterEvent("Button_but_shop_buy_advance_one", new DefineFactory<Button_but_shop_buy_advance_one>());
        EventCenter.Self.RegisterEvent("Button_but_shop_buy_advance_ten", new DefineFactory<Button_but_shop_buy_advacne_ten>());
		// page 1
		EventCenter.Self.RegisterEvent("Button_buy_button", new DefineFactory<Button_buy_button>());
		// page 2
		EventCenter.Self.RegisterEvent("Button_but_shop_buy_vip", new DefineFactory<Button_but_shop_buy_vip>());

		// toggle
		EventCenter.Self.RegisterEvent("Button_pet_shop_btn", new DefineFactory<Button_pet_shop_btn>());
		EventCenter.Self.RegisterEvent("Button_tool_shop_btn", new DefineFactory<Button_tool_shop_btn>());
		EventCenter.Self.RegisterEvent("Button_character_skin_shop_btn", new DefineFactory<Button_character_skin_shop_btn>());
		// end

		DataCenter.OpenWindow ("BACK_GROUP_SHOP_WINDOW");
		DataCenter.OpenWindow("INFO_GROUP_WINDOW");

		DataCenter.OpenWindow ("SHOP_WINDOW", true);
	}

    void OnDestroy()
    {
        //by chenliang
        //begin

        NewShopWindow tmpWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
        if (tmpWin != null)
            tmpWin.StopLoadChouKaScene();

        //end
		DataCenter.CloseWindow ("BACK_GROUP_SHOP_WINDOW");
		DataCenter.CloseWindow ("MYSTERIOUS_SHOP_WINDOW");
		DataCenter.Remove("SHOP_WINDOW");

		GameObject obj = GameObject.Find ("create_scene");
		if(obj != null)
			GameObject.Destroy (obj);
    }
}
//10.0.0.76
public class NewShopWindow:tWindow {

	public class NewShopWindowParams {
		public int pageIndex; //0, lottery 1, shop item 2, vip bag
		public object obj;
        public bool mIsPlayWinAnim = true;
	}

    public NewShopWindow() : base() { }
	public NewShopWindow(GameObject obj) {
		mGameObjUI = obj;
	}

	public override void Open (object param) {

        bool tmpGameObjUI = (mGameObjUI==null);

		base.Open (param);
		// Refresh(null);

        //by chenliang
        //begin

//        //初始化
//        ShowMainScene(true);
//        UiHidden(true);

//		ShopNetEvent.RequestLotteryQuery(RequestLotteryQuerySuccess, RequestLotteryQueryFail);
//-----------------
        if (!mOpened || tmpGameObjUI)
        {
            mOpened = true;

            //初始化
            ShowMainScene(true);
            UiHidden(true);

            DataCenter.OpenWindow("BACK_GROUP_SHOP_WINDOW");
            DataCenter.OpenWindow("INFO_GROUP_WINDOW");

            TweenPosition tmpOpenAnim = GetComponent<TweenPosition>("npc");
            if (tmpOpenAnim != null)
            {
                if (mOpenAnimDel == null || !tmpOpenAnim.onFinished.Contains(mOpenAnimDel))
                {
                    mOpenAnimDel = new EventDelegate(__OnOpenAnimComplete);
                    tmpOpenAnim.onFinished.Add(mOpenAnimDel);
                }

                tmpOpenAnim.ResetToBeginning();
                tmpOpenAnim.enabled = true;
            }
        }

        if (tmpGameObjUI)
        {
            mAsyncOP = null;                        //异步加载对象设空
            mLoadChouKaSceneComplete = null;        //回调设空
        }

        int tmpParam = -1;
        if (param == null || param is bool)
            tmpParam = 1;
        else
            tmpParam = (int)param;
        switch (tmpParam)
        {
            case 1:
                {
                    this.StartCoroutine(__OpenShopSubPage(SHOP_PAGE_TYPE.PET, () =>
                    {
                        mCurrShopPageType = SHOP_PAGE_TYPE.PET;
                        ShopNetEvent.RequestLotteryQuery(
                            (text) =>
                            {
                                DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", 1);
                                RequestLotteryQuerySuccess(text);
                            }, RequestLotteryQueryFail);
                    }));
                }break;

            case 2:
                {
                    this.StartCoroutine(__OpenShopSubPage(SHOP_PAGE_TYPE.TOOL, () =>
                    {
                        mCurrShopPageType = SHOP_PAGE_TYPE.TOOL;
                        ShopNetEvent.RequestPropShopQuery((text) =>
                        {
                            SC_ResponsePropShopQuery result = JCode.Decode<SC_ResponsePropShopQuery>(text);
                            if (result.ret == (int)STRING_INDEX.ERROR_NONE) { }
                            DataCenter.SetData("SHOP_WINDOW", "REFRESH", new NewShopWindow.NewShopWindowParams() { pageIndex = 1, obj = result });
                            DEBUG.Log("RequestPropShopQuerySuccess:" + text);
                            DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", 2);
                        },
                        (text) =>
                        {
                            DEBUG.Log("RequestPropShopQueryErr:" + text);
                        });
                    }));
                } break;
            case 3:
                {
                    this.StartCoroutine(__OpenShopSubPage(SHOP_PAGE_TYPE.CHARACTER, () =>
                    {
                        mCurrShopPageType = SHOP_PAGE_TYPE.CHARACTER;
                        Button_character_skin_shop_btn.OpenShopVIPWindow();
                    }));
                } break;
            default:
                DEBUG.LogError("No Such Tab");
                break;
        }

        //by chenliang
        //begin

//		GetSub ("character_skin_shop_btn").SetActive (CommonParam.isOnLineVersion);
//-----------------
        //加空判断
//        GameObject tmpGO = GetSub("character_skin_shop_btn");
//        if (tmpGO != null)
//            tmpGO.SetActive(CommonParam.isOnLineVersion);

        //end
	}

    //by chenliang
    //begin

// 	public override void Init() {
// 
// 	}
//---------------------
    private Dictionary<int, DataRecord> mDicItemsConfig;
    private Dictionary<int, DataRecord> mDicVIPConfig;

    private EventDelegate mOpenAnimDel;
    private SHOP_PAGE_TYPE mCurrShopPageType = SHOP_PAGE_TYPE.NONE;         //当前打开页签
    public SHOP_PAGE_TYPE CurrShopPageType
    {
        get
        {
            if (!IsOpen())
                return SHOP_PAGE_TYPE.NONE;
            return mCurrShopPageType;
        }
    }

    private Action mLoadChouKaSceneComplete;                    //抽卡场景加载完成回调
    public void SetLoadChouKaSceneComplete(Action callback)
    {
        if (mLoadChouKaSceneComplete != null)
            return;

        mLoadChouKaSceneComplete = callback;
    }
    public Action GetLoadChouKaSceneComplete()
    {
        return mLoadChouKaSceneComplete;
    }

    private AsyncOperation mAsyncOP;        //用于异步加载抽卡场景
    private bool mOpened = false;
    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_but_shop_preview", new DefineFactoryLog<Button_but_shop_preview>());
        EventCenter.Self.RegisterEvent("Button_fangdajing_cheap", new DefineFactoryLog<Button_fangdajing_cheap>());
        EventCenter.Self.RegisterEvent("Button_fangdajing_expensive", new DefineFactoryLog<Button_fangdajing_expensive>());
        EventCenter.Self.RegisterEvent("Button_but_shop_buy_common_one", new DefineFactory<Button_shop_buy_common_one>());
        EventCenter.Self.RegisterEvent("Button_but_shop_buy_common_ten", new DefineFactory<Button_but_shop_buy_common_ten>());
        EventCenter.Self.RegisterEvent("Button_but_shop_buy_common_free", new DefineFactory<Button_shop_buy_common_one>());
        EventCenter.Self.RegisterEvent("Button_but_shop_buy_advance_one", new DefineFactory<Button_but_shop_buy_advance_one>());
        EventCenter.Self.RegisterEvent("Button_but_shop_buy_advance_ten", new DefineFactory<Button_but_shop_buy_advacne_ten>());
        EventCenter.Self.RegisterEvent("Button_but_shop_buy_advance_free", new DefineFactory<Button_but_shop_buy_advance_one>());
        // page 1
        EventCenter.Self.RegisterEvent("Button_buy_button", new DefineFactory<Button_buy_button>());
        // page 2
        EventCenter.Self.RegisterEvent("Button_but_shop_buy_vip", new DefineFactory<Button_but_shop_buy_vip>());

        // toggle
        EventCenter.Self.RegisterEvent("Button_pet_shop_btn", new DefineFactory<Button_pet_shop_btn>());
        EventCenter.Self.RegisterEvent("Button_tool_shop_btn", new DefineFactory<Button_tool_shop_btn>());
        EventCenter.Self.RegisterEvent("Button_character_skin_shop_btn", new DefineFactory<Button_character_skin_shop_btn>());
        // end

        mDicItemsConfig = TableCommon.GetDataFromMallConfigByPage(1);
        mDicVIPConfig = TableCommon.GetDataFromMallConfigByPage(2);

        __InitGetCardRecords();
    }
    private enum GetCardType
    {
        PreciousOne = 1,
        PreciousTen,
        NormalOne,
        NormalTen
    }
    private void __InitGetCardRecords()
    {
        mNormalCardOneRecords.Clear();
        mNormalCardTenRecords.Clear();
        mPreciousCardOneRecords.Clear();
        mPreciousCardTenRecords.Clear();
        mDicGroupCardData.Clear();

        foreach (KeyValuePair<int, DataRecord> tmpPair in DataCenter.mPumpingConfig.GetAllRecord())
        {
            int tmpIntType = (int)tmpPair.Value.getObject("TYPE");
            GetCardType tmpType = (GetCardType)tmpIntType;
            switch (tmpType)
            {
                case GetCardType.PreciousOne: mPreciousCardOneRecords.Add(tmpPair.Value); break;
                case GetCardType.PreciousTen: mPreciousCardTenRecords.Add(tmpPair.Value); break;
                case GetCardType.NormalOne: mNormalCardOneRecords.Add(tmpPair.Value); break;
                case GetCardType.NormalTen: mNormalCardTenRecords.Add(tmpPair.Value); break;
            }

            if (tmpType == GetCardType.PreciousOne)
            {
                string tmpKey = "PreciousOne_" + tmpPair.Key;
                if (!mDicGroupCardData.ContainsKey(tmpKey))
                    mDicGroupCardData[tmpKey] = new GroupCardDataList();
                GroupCardDataList tmpListGroupID = mDicGroupCardData[tmpKey];
                tmpListGroupID.MinLevel = (int)tmpPair.Value.getObject("LEVEL_MIN");
                tmpListGroupID.MaxLevel = (int)tmpPair.Value.getObject("LEVEL_MAX");
                string tmpGroups = tmpPair.Value.get("GROUP_ID");
                if (tmpGroups != null && tmpGroups != "")
                {
                    string[] tmpStrGroups = tmpGroups.Split(new char[] { '|' });
                    for (int i = 0, count = tmpStrGroups.Length; i < count; i++)
                    {
                        string[] tmpStrGroup = tmpStrGroups[i].Split(new char[] { '#' });
                        int tmpGroupDataType = int.Parse(tmpStrGroup[1]);
                        tmpListGroupID.ListData.Add(new GroupCardData()
                        {
                            GroupID = int.Parse(tmpStrGroup[0]),
                            Position = i + 1,
                            type = tmpGroupDataType
                        });
                        if (tmpGroupDataType == 1 || tmpGroupDataType == 2)
                            tmpListGroupID.ImportantCount += 1;
                    }
                }
            }
        }
    }
    private DataRecord __GetCardRecord(GetCardType getCardType)
    {
        int tmpLevel = RoleLogicData.Self.character.level;
        List<DataRecord> tmpList = null;
        switch (getCardType)
        {
            case GetCardType.PreciousOne: tmpList = mPreciousCardOneRecords; break;
            case GetCardType.PreciousTen: tmpList = mPreciousCardTenRecords; break;
            case GetCardType.NormalOne: tmpList = mNormalCardOneRecords; break;
            case GetCardType.NormalTen: tmpList = mNormalCardTenRecords; break;
        }
        DataRecord tmpRet = null;
        for (int i = tmpList.Count - 1; i >= 0; i--)
        {
            DataRecord tmpRecord = tmpList[i];
            int tmpLevelMin = (int)tmpRecord.getObject("LEVEL_MIN");
            int tmpLevelMax = (int)tmpRecord.getObject("LEVEL_MAX");
            if (tmpLevel >= tmpLevelMin && tmpLevel <= tmpLevelMax)
            {
                tmpRet = tmpRecord;
                break;
            }
        }
        return tmpRet;
    }
    private string __GetImportantLeftCountInfo(int buyCount)
    {
        if (buyCount <= 0)
            return "";

        int tmpCurrLevel = RoleLogicData.Self.character.level;
        GroupCardDataList tmpList = null;
        List<GroupCardData> tmpListData = null;
        foreach (KeyValuePair<string, GroupCardDataList> tmpPair in mDicGroupCardData)
        {
            if (tmpCurrLevel >= tmpPair.Value.MinLevel && tmpCurrLevel <= tmpPair.Value.MaxLevel)
            {
                tmpList = tmpPair.Value;
                tmpListData = tmpPair.Value.ListData;
                break;
            }
        }
        if (tmpListData == null)
            return "";
        if (tmpList.ImportantCount <= 0)
            return "";
        int tmpBuyCount = buyCount - 1;
        int tmpBuyIndex = tmpBuyCount % tmpListData.Count;
        int tmpLeftCount = 0;
        string tmpDataColor = "";
        while (true)
        {
            GroupCardData tmpCardData = tmpListData[(tmpBuyIndex + tmpLeftCount) % tmpListData.Count];
            if (tmpCardData.type == 1)
            {
                tmpDataColor = ExtensionString.GetColorPrefixByColor(GameCommon.SetQualityColor(5)) + "橙色[-]";
                break;
            }
            else if(tmpCardData.type == 2)
            {
                tmpDataColor =
                    ExtensionString.GetColorPrefixByColor(GameCommon.SetQualityColor(5)) + "橙色[-]" +
                    "或" + 
                    ExtensionString.GetColorPrefixByColor(GameCommon.SetQualityColor(4)) + "紫色[-]";
                break;
            }
            tmpLeftCount += 1;
        }
        string tmpDesc = "";
        if (tmpLeftCount == 0)
            tmpDesc = string.Format("本次购买必送{0}灵将[-]", tmpDataColor);
        else if (tmpLeftCount == 1)
            tmpDesc = string.Format("下次购买必送{0}灵将[-]", tmpDataColor);
        else
            tmpDesc = string.Format("再购买" + ExtensionString.GetColorPrefixByColor(GameCommon.SetQualityColor(5)) + "{0}[-]次必送{1}灵将", (tmpLeftCount + 1), tmpDataColor);
        return tmpDesc;
    }
    public override void OnClose()
    {
        //窗口关闭时需要停止场景加载，等待下次窗口打开时再继续加载
        StopLoadChouKaScene();

        DataCenter.CloseWindow("BACK_GROUP_SHOP_WINDOW");
        DataCenter.CloseWindow("MYSTERIOUS_SHOP_WINDOW");

        GameObject obj = GameObject.Find("create_scene");
        if (obj != null)
            GameObject.Destroy(obj);

        base.OnClose();
        mOpened = false;
        mLoadChouKaSceneComplete = null;
        //MainUIScript.Self.mShopBackAction = null;
    }

    /// <summary>
    /// 打开子页面
    /// </summary>
    /// <param name="pageType"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator __OpenShopSubPage(SHOP_PAGE_TYPE pageType, Action callback)
    {
        string tmpPrefabName = "shop_window_scrollview_context_" + ((int)pageType).ToString();
        string tmpGOName = "scrollview_context_" + ((int)pageType).ToString();
        GameObject tmpGO = GetSub(tmpGOName);
        if (tmpGO != null)
        {
            if (callback != null)
                callback();
            yield break;
        }
        yield return null;
        GameObject tmpGOParent = GetSub("tab_context_panel");
        if (tmpGOParent == null)
            yield break;
        tmpGO = GameCommon.LoadAndIntanciatePrefabs(UICommonDefine.strUIPrefabsPath + tmpPrefabName);
        //为了和之前的名称统一
        tmpGO.name = tmpGOName;
        tmpGO.transform.parent = tmpGOParent.transform;
        tmpGO.transform.localScale = Vector3.one;
        if (callback != null)
            callback();
    }

    private IEnumerator __WaitForFrames(int frames, Action callback)
    {
        for (int i = 0; i < frames; i++)
            yield return null;
        if (callback != null)
            callback();
    }
    private void __OnOpenAnimComplete()
    {
        mOpenAnimDel = null;
        int tmpFrames = 1;
        if(CommonParam.mOpenDynamicUI)
            tmpFrames = 30;
        this.StartCoroutine(__WaitForFrames(tmpFrames, () =>
        {
            LoadChouKaScene();
        }));
    }
    /// <summary>
    /// 抽卡场景是否已经加载完成
    /// </summary>
    /// <returns></returns>
    public bool IsLoadChouKaSuccess()
    {
        if (!IsOpen())
            return false;
        if (mAsyncOP == null && mOpenAnimDel == null)
            return true;
        return mAsyncOP.isDone;
    }
    public void LoadChouKaScene()
    {
        if (GameObject.Find("create_scene") != null)
            return;
        if (mAsyncOP != null)
        {
            //如果之前已经进行了场景加载，这里只需要继续进行加载
            mAsyncOP.allowSceneActivation = true;
            return;
        }

//        GlobalModule.Instance.LoadScene("chouka", true);
        GlobalModule.DoCoroutine(StartLoadChouKaScene());
    }
    /// <summary>
    /// 开始抽卡场景加载
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartLoadChouKaScene()
    {
        if(mAsyncOP != null)
            yield break;

//        yield return new WaitForSeconds(0.6f);
        mAsyncOP = GlobalModule.Instance.LoadSceneAsync("chouka", true);
        yield return mAsyncOP;
        mAsyncOP = null;
        if (mLoadChouKaSceneComplete != null)
        {
            if (IsOpen() && (CurrShopPageType == SHOP_PAGE_TYPE.PET))
                mLoadChouKaSceneComplete();
            mLoadChouKaSceneComplete = null;
        }
    }
    /// <summary>
    /// 停止抽卡场景加载
    /// </summary>
    public void StopLoadChouKaScene()
    {
        if (mAsyncOP == null)
            return;
        mAsyncOP.allowSceneActivation = false;
    }

    private void __EnableToggle(SHOP_PAGE_TYPE shopType)
    {
        switch (shopType)
        {
            case SHOP_PAGE_TYPE.PET:
                {
                    GameCommon.ToggleTrue(GetSub("pet_shop_btn"));
                } break;
		    case SHOP_PAGE_TYPE.TOOL:
                {
                    GameCommon.ToggleTrue(GetSub("tool_shop_btn"));
                } break;
		    case SHOP_PAGE_TYPE.CHARACTER:
                {
                    GameCommon.ToggleTrue(GetSub("character_skin_shop_btn"));
                } break;
        }
    }
    //end

    //by chenliang
    //begin

// 	int normalLotteryLeftNum;
// 	int advanceLotteryLeftNum;
// 
// 	public void NormalLotteryCoolDown(GameObject o) {
// 		UILabel usrLeftNum = GameCommon.FindObject(o, "lab_count_tip").GetComponent<UILabel>();
// 		usrLeftNum.text = "normal cool down";
// 	}
// 
// 	public void AdvanceLotteryCoolDown(GameObject o) {
// 		UILabel usrLeftNum = GameCommon.FindObject(o, "lab_count_tip").GetComponent<UILabel>();
// 		usrLeftNum.text = "advance cool down";
// 	}
//---------------------
    //功能重写

    public void NormalLotteryCoolDown(object param)
    {
//         NewShopWindowParams tmpWinParam = param as NewShopWindowParams;
//         SC_ResponseLotteryQuery tmpResp = tmpWinParam.obj as SC_ResponseLotteryQuery;
// 
//         tmpResp.normalLottery.isFree = (tmpResp.normalLottery.leftNum <= 0) ? 0 : 1;
//         tmpResp.normalLottery.leftTime = 0;
// 
//         Refresh(tmpWinParam);
        ShopNetEvent.RequestLotteryQuery(
            (string text) =>
            {
                RequestLotteryQuerySuccess(text, false);
            }, RequestLotteryQueryFail);
    }

    public void AdvanceLotteryCoolDown(object param)
    {
//         NewShopWindowParams tmpWinParam = param as NewShopWindowParams;
//         SC_ResponseLotteryQuery tmpResp = tmpWinParam.obj as SC_ResponseLotteryQuery;
// 
//         tmpResp.preciousLottery.leftNum += 1;
//         tmpResp.preciousLottery.isFree = 1;
//         DataRecord tmpPumpingConfig = DataCenter.mPumpingConfig.GetRecord((int)SHOP_PUMPING_TYPE.PRECIOUS_ONE);
//         int tmpFreeCount = (int)tmpPumpingConfig.getObject("FREE_NUMBER");
//         if (tmpResp.preciousLottery.leftNum < tmpFreeCount)
//             tmpResp.preciousLottery.leftTime = (int)tmpPumpingConfig.getObject("RESTORE_TIME") * 60;
//         else
//             tmpResp.preciousLottery.leftTime = 0;
// 
//         Refresh(tmpWinParam);
        ShopNetEvent.RequestLotteryQuery(
            (string text) =>
            {
                RequestLotteryQuerySuccess(text, false);
            }, RequestLotteryQueryFail);
    }

    //end

	public override bool Refresh(object param) {
		base.Refresh(param);
        //added by xuke 刷新标签页红点
        RefreshTabNewMark();
        //end
        //by chenliang
        //begin

// 		UILabel yflInfolbl = GetLable("label_yfl_desc");
// 		UILabel jflInfolbl = GetLable("label_jfl_desc");
// 
// 		ConsumeItemData ycid = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.SILVER_FL_COMMAND);
// 		if(ycid != null) 
// 			yflInfolbl.text = "银符令x" + ycid.itemNum;
// 		else
// 			yflInfolbl.text = "银符令x" + 0;
// 
// 		ycid = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.GOLD_FL_COMMAND);
// 
// 		if(ycid != null)
// 			jflInfolbl.text = "金符令x" + ycid.itemNum;
// 		else
// 			jflInfolbl.text = "金符令x" + 0;
//-----------------------
        //无效代码

        //end

		if(param != null) {
			NewShopWindowParams rparam = (NewShopWindowParams)param;

			switch(rparam.pageIndex) {
				//lottery shop
			case 0:
			{
                //by chenliang
                //begin

// 				SC_ResponseLotteryQuery result = (SC_ResponseLotteryQuery)rparam.obj;
// 
// 				GameObject subItem1 = GetSub("item_shop_Info(Clone)_0");
// 				UILabel usrLeftNum = GameCommon.FindObject(subItem1, "lab_count_tip").GetComponent<UILabel>();
// 				bool freeLottery = result.normalLottery.isFree == 0 ? false : true;
// 				long leftScoend = result.normalLottery.leftTime;
// 				normalLotteryLeftNum = result.normalLottery.leftNum;
// 				if(!freeLottery && leftScoend > 0) {
// 					usrLeftNum.text = "23LLL" + leftScoend;
// 					SetCountdown(subItem1, "lab_count_tip", leftScoend, new CallBack(this, "NormalLotteryCoolDown", subItem1), true);
// 				} else if(result.normalLottery.leftNum == 0)
// 					usrLeftNum.text = "免费次数已经用完"; 
// 				else
// 					usrLeftNum.text = "剩余免费次数x" + result.normalLottery.leftNum;
// 				// bind button shop data
// 				UIButtonEvent ube = GameCommon.FindComponent<UIButtonEvent>(subItem1, "but_shop_buy_common_one");
// 				ube.mData.set("NET_DATA", result);
// 				
// 				subItem1 = GetSub("item_shop_Info(Clone)_1");
// 				usrLeftNum = GameCommon.FindObject(subItem1, "lab_count_tip").GetComponent<UILabel>();
// 				freeLottery = result.preciousLottery.isFree == 0 ? false : true;
// 				leftScoend = result.preciousLottery.leftTime;
// 				advanceLotteryLeftNum = result.preciousLottery.leftNum;
// 				if(!freeLottery && leftScoend > 0) {
// 					usrLeftNum.text = "23LLL" + leftScoend;
// 					SetCountdown(subItem1, "lab_count_tip", leftScoend, new CallBack(this, "NormalLotteryCoolDown", subItem1), true);
// 				} else if(result.normalLottery.leftNum == 0)
// 					usrLeftNum.text = "免费次数已经用完"; 
// 				else
// 					usrLeftNum.text = "剩余免费次数x" + result.preciousLottery.leftNum;
// 				// bind button data
// 				ube = GameCommon.FindComponent<UIButtonEvent>(subItem1, "but_shop_buy_advance_one");
// 				ube.mData.set("NET_DATA", result);
// 				
// 				if(ycid == null || ycid.itemNum <= 0) {
// 					subItem1 = GetSub("but_shop_buy_advance_one");
// 					subItem1 = GameCommon.FindObject(subItem1, "label_cost_info");
// 					UILabel advaceOneShot = subItem1.GetComponent<UILabel>();
// 					advaceOneShot.text = "元宝300";
// 					
// 					subItem1 = GetSub("but_shop_buy_advacne_ten");
// 					subItem1 = GameCommon.FindObject(subItem1, "label_cost_info");
// 					UILabel advaceTenShot = subItem1.GetComponent<UILabel>();
// 					advaceTenShot.text = "元宝2800";
// 					
// 					ycid = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.YUANBAO);
// 					if(ycid == null) return true;
// 					else if (ycid.itemNum < 300) {
// 						advaceOneShot.color = Color.red;
// 						advaceTenShot.color = Color.red;
// 					}else if (2800 > ycid.itemNum && ycid.itemNum > 300) {
// 						advaceTenShot.color = Color.red;
// 					}
// 				}
//-----------------------------
                __RefreshCardShop(rparam);            
                //end
                //added by xuke 刷新红点
                RrfreshPetShopNewMark();
                //end
				break;
			}
			case 1:
			{
				// item shop
                //by chenliang
                //begin

// 				Dictionary<int, DataRecord> shopItemConfigData = (Dictionary<int, DataRecord>)rparam.obj;;
// 				UIGridContainer grid = null;
// 				if (shopItemConfigData != null) {
// 					grid = GetComponent<UIGridContainer>("shop_goods_grid");
// 
// 					if (grid != null) {
// 						grid.MaxCount = shopItemConfigData.Count;
// 						int i = 0;
// 						foreach (KeyValuePair<int, DataRecord> vRe in shopItemConfigData)
// 						{
// 							DataRecord r = vRe.Value;
// 							GameObject item = grid.controlList[i++];
// 							item.name = "shop_item_buy_event";
// 
// 							string itemName = TableCommon.GetStringFromRoleEquipConfig((int)r["ITEM_ID"], "NAME");
// 							string itemInfo = TableCommon.GetStringFromRoleEquipConfig((int)r["ITEM_ID"], "DESCRIPTION");
// 							// icon setting
// 							GameCommon.SetItemIcon(item, "icon_goods", (int)r["ITEM_ID"]);
// 
// 							/*
// 							UISprite icon = GameCommon.FindComponent<UISprite>(item, "icon_goods");
// 							string altsName = TableCommon.GetStringFromRoleEquipConfig((int)r["ITEM_ID"], "ICON_ATLAS_NAME");
// 							string spriteName = TableCommon.GetStringFromRoleEquipConfig((int)r["ITEM_ID"], "ICON_SPRITE_NAME");
// 							GameCommon.SetIcon(icon, altsName, spriteName);
// 							*/
// 							// icon end
// 							string totalPrice = r["COST_NUM_1"].ToString();
// 							string[] prices = totalPrice.Split(new char[]{'|'});
// 							GameCommon.SetUIText(item, "goods_name_label", itemName);
// 							GameCommon.SetUIText(item, "goods_info_label", itemInfo);
// 							GameCommon.SetUIText(item, "need_buy_num_label", prices[0]);
// 							GameCommon.SetUIText(item, "goods_limit_label02", "可以购买" + ((int)r["BUY_NUM"]).ToString() + "次");
// 
// 							NiceData butData = GameCommon.GetButtonData(item, "buy_button");
// 							if (butData != null) {
// 								butData.set("ITEM_ID_TEST", (int)r["INDEX"]);
// 								butData.set("NEED_COST_YB", prices[0]);
// 								//butData.set("SERVER_NAME", (string)r["NAME"]);
// 								//butData.set("STATE", (int)r["STATE"]);
// 								//butData.set("SHOP_WINDOW", this);
// 							} else
// 								DEBUG.LogError("No add button event");
// 
// 						}
// 
// 					}
// 				}
//-----------------------
                //将道具商店刷新分离
                __RefreshItemShop(rparam);

                //end

				break;
			}
			case 2:
			{
				// vip shop
                //by chenliang
                //begin

// 				Dictionary<int, DataRecord> shopItemConfigData = (Dictionary<int, DataRecord>)rparam.obj;;
// 				UIGridContainer grid = null;
// 				if (shopItemConfigData != null) {
// 					grid = GetComponent<UIGridContainer>("vip_list_grid");
// 
// 					if (grid != null) {
// 						grid.MaxCount = shopItemConfigData.Count;
// 						int i = 0;
// 						foreach (KeyValuePair<int, DataRecord> vRe in shopItemConfigData)
// 						{
// 							DataRecord r = vRe.Value;
// 							GameObject item = grid.controlList[i++];
// 							item.name = "vip_item_buy_event";
// 
// 
// 							GameCommon.SetUIText(item, "lab_title_label", "vip" + ((int)r["VIP_LEVEL_DISPLAY"]).ToString() + "礼包");
// 							GameCommon.SetUIText(item, "icon_label",((int)r["COST_NUM_1"]).ToString());
// 
// 							//GameCommon.SetUIText(item, "need_buy_num_label", ((int)r["STAGE_PRICE_1"]).ToString());
// 							//GameCommon.SetUIText(item, "goods_limit_label02", "可以购买" + ((int)r["NUMBER_STAGE_1"]).ToString() + "次");
// 							
// 							NiceData butData = GameCommon.GetButtonData(item, "but_shop_buy_vip");
// 							if (butData != null) {
// 								butData.set("ITEM_ID_TEST", (int)r["INDEX"]);
// 								butData.set("GIFT_OPEN_LEVEL", (int)r["VIP_LEVEL_DISPLAY"]);
// 								butData.set("COST_NUM", (int)r["COST_NUM_1"]);
// 							} else
// 								DEBUG.LogError("No add button event");
// 
// 						}
// 						
// 					}
// 				}
//----------------------
                //将VIP商店刷新分离
                __RefreshVIPShop(rparam);

                //end
                //added by xuke 刷新红点
                RefreshCharacterSkinShopNewMark();
                //end
				break;
			}
			}


		}

		return true;
	}

    #region 商店红点刷新

    private void RefreshTabNewMark() 
    {
        RefreshPetTabNewMark();
        RefreshCharacterSkinShopNewMark();
    }
    private void RrfreshPetShopNewMark() 
    {
        GameObject _context1 = GameCommon.FindObject(mGameObjUI, "scrollview_context_1");
        UIGridContainer _gridContainer = GameCommon.FindComponent<UIGridContainer>(_context1, "grid");
        if (_gridContainer == null)
            return;
        GameObject _commmonOneBtnObj = GameCommon.FindObject(_gridContainer.controlList[0], "but_shop_buy_common_one");
        GameCommon.SetNewMarkVisible(_commmonOneBtnObj, ShopNewMarkManager.Self.NormalDrawVisible);
        GameObject _commonTenBtnObj = GameCommon.FindObject(_gridContainer.controlList[0], "but_shop_buy_common_ten");
        GameCommon.SetNewMarkVisible(_commonTenBtnObj, ShopNewMarkManager.Self.NormalTenDrawVisible);
        GameObject _advanceOneBtnObj = GameCommon.FindObject(_gridContainer.controlList[1], "but_shop_buy_advance_one");
        GameCommon.SetNewMarkVisible(_advanceOneBtnObj,ShopNewMarkManager.Self.AdvanceDrawVisible);

        RefreshPetTabNewMark();
    }

    private void RefreshPetTabNewMark() 
    {
        GameObject _petTabShopBtnObj = GameCommon.FindObject(mGameObjUI, "pet_shop_btn");
        GameCommon.SetNewMarkVisible(_petTabShopBtnObj, ShopNewMarkManager.Self.PetTabVisible);
    }
    
    private void RefreshCharacterSkinShopNewMark() 
    {
        GameObject _characterSkinTabObj = GameCommon.FindObject(mGameObjUI, "character_skin_shop_btn");
        GameCommon.SetNewMarkVisible(_characterSkinTabObj, ShopNewMarkManager.Self.CharacterSkinTabVisible);
    }
    #endregion 
    //by chenliang
    //begin

    public enum SHOP_PUMPING_TYPE
    {
        PRECIOUS_ONE = 1,
        PRECIOUS_TEN,
        NORMAL_ONE,
        NORMAL_TEN
    }

    //获取高级抽卡需要的钻石数量
    public static int GetPreciousPrice(SHOP_PUMPING_TYPE type)
    {
        int price = 0;
        Dictionary<SHOP_PUMPING_TYPE, ItemDataBase> tmpDicItems = new Dictionary<SHOP_PUMPING_TYPE, ItemDataBase>();
        foreach (KeyValuePair<int, DataRecord> tmpPair in DataCenter.mPumpingConfig.GetAllRecord())
        {
            if ((int)type == (int)tmpPair.Value.getObject("INDEX") )
            {
                int tmpMultiple = (int)tmpPair.Value.getObject("PUMPING_MULTIPLE");
                price = (int)tmpPair.Value.getObject("PUMPING_PRICE_3") * tmpMultiple;
                break;
            }
        }
        return price;
    }

    /// <summary>
    /// 获取当前消耗品信息
    /// </summary>
    /// <returns></returns>
    public Dictionary<SHOP_PUMPING_TYPE, ItemDataBase> __GetFinalUseItems()
    {
        Dictionary<SHOP_PUMPING_TYPE, ItemDataBase> tmpDicItems = new Dictionary<SHOP_PUMPING_TYPE, ItemDataBase>();
		int iCount = 0;
        foreach (KeyValuePair<int, DataRecord> tmpPair in DataCenter.mPumpingConfig.GetAllRecord())
        {
			if(++iCount > 4) break;
            ItemDataBase tmpItem = new ItemDataBase() { tid = -1, itemNum = 0, itemId = -1 };
            int tmpMultiple = (int)tmpPair.Value.getObject("PUMPING_MULTIPLE");
            int tmpPrice1 = (int)tmpPair.Value.getObject("PUMPING_PRICE_1") * tmpMultiple;
            int tmpPrice2 = (int)tmpPair.Value.getObject("PUMPING_PRICE_2") * tmpMultiple;
            int tmpPrice3 = (int)tmpPair.Value.getObject("PUMPING_PRICE_3") * tmpMultiple;
            int tmpPrice1Own = PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND);
            int tmpPrice2Own = PackageManager.GetItemLeftCount((int)ITEM_TYPE.GOLD_FL_COMMAND);
            int tmpPrice3Own = RoleLogicData.Self.diamond;
            if (tmpPrice1 > 0 && tmpPrice1Own >= tmpPrice1)
            {
                tmpItem.tid = (int)ITEM_TYPE.SILVER_FL_COMMAND;
                tmpItem.itemId = GameCommon.GetItemId((int)ITEM_TYPE.SILVER_FL_COMMAND);
                tmpItem.itemNum = tmpPrice1;
            }
            else if (tmpPrice2 > 0 && tmpPrice2Own >= tmpPrice2)
            {
                tmpItem.tid = (int)ITEM_TYPE.GOLD_FL_COMMAND;
                tmpItem.itemId = GameCommon.GetItemId((int)ITEM_TYPE.GOLD_FL_COMMAND);
                tmpItem.itemNum = tmpPrice2;
            }
            else if (tmpPrice3 > 0 && tmpPrice3Own >= tmpPrice3)
            {
                tmpItem.tid = (int)ITEM_TYPE.YUANBAO;
                tmpItem.itemNum = tmpPrice3;
            }
            else
            {
                if (tmpPrice1 > 0)
                {
                    tmpItem.tid = (int)ITEM_TYPE.SILVER_FL_COMMAND;
                    tmpItem.itemId = GameCommon.GetItemId((int)ITEM_TYPE.SILVER_FL_COMMAND);
                    tmpItem.itemNum = -tmpPrice1;
                }
                else if (tmpPrice2 > 0)
                {
                    tmpItem.tid = (int)ITEM_TYPE.YUANBAO;
                    tmpItem.itemId = GameCommon.GetItemId((int)ITEM_TYPE.YUANBAO);
                    tmpItem.itemNum = -tmpPrice3;
                }
                else if (tmpPrice3 > 0)
                {
                    tmpItem.tid = (int)ITEM_TYPE.YUANBAO;
                    tmpItem.itemNum = -tmpPrice3;
                }
            }
            tmpDicItems[(SHOP_PUMPING_TYPE)tmpPair.Key] = tmpItem;
        }
        return tmpDicItems;
    }
    /// <summary>
    /// 获取消耗品描述
    /// </summary>
    /// <param name="itemData"></param>
    /// <returns></returns>
    private string __GetUseItemDesc(ItemDataBase itemData)
    {
        string tmpDesc = "";
//         switch (itemData.tid)
//         {
//             case (int)ITEM_TYPE.SILVER_FL_COMMAND: tmpDesc += "银符灵"; break;
//             case (int)ITEM_TYPE.GOLD_FL_COMMAND: tmpDesc += "金符灵"; break;
//             case (int)ITEM_TYPE.YUANBAO: tmpDesc += "元宝"; break;
//         }
        tmpDesc += ("x" + Mathf.Abs(itemData.itemNum).ToString());
        return tmpDesc;
    }
    private void __RefreshCardShop(NewShopWindowParams rparam)
    {
        __RefreshCardShop2(rparam);
        return;

        SC_ResponseLotteryQuery result = (SC_ResponseLotteryQuery)rparam.obj;
        set("LAST_LOTTERY_QUERY", result);

        GameObject tmpParent = GetSub("scrollview_context_1");
        tmpParent.SetActive(true);
        GameObject tmpContext2 = GetSub("scrollview_context_2");
        if (tmpContext2 != null)
            tmpContext2.SetActive(false);
        GameObject tmpContext3 = GetSub("scrollview_context_3");
        if (tmpContext3 != null)
            tmpContext3.SetActive(false);

        Dictionary<SHOP_PUMPING_TYPE, ItemDataBase> tmpDicUseItems = __GetFinalUseItems();

        GameObject tmpFLCountParent = GameCommon.FindObject(tmpParent, "usr_item_package");

        //现有银符灵
        GameCommon.SetUIText(tmpFLCountParent, "label_yfl_desc", PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND).ToString());

        //现有金符灵
        GameCommon.SetUIText(tmpFLCountParent, "label_jfl_desc", PackageManager.GetItemLeftCount((int)ITEM_TYPE.GOLD_FL_COMMAND).ToString());

        //普通抽卡
        GameObject subItem1 = GameCommon.FindObject(tmpParent, "item_shop_Info(Clone)_0");
        UILabel usrLeftNum = GameCommon.FindObject(subItem1, "lab_count_tip").GetComponent<UILabel>();
        bool freeLottery = result.normalLottery.isFree == 0 ? false : true;
        long leftSecond = result.normalLottery.leftTime;
        ItemDataBase tmpItemData = tmpDicUseItems[SHOP_PUMPING_TYPE.NORMAL_ONE];
        string tmpUseItemDesc = __GetUseItemDesc(tmpItemData);
        int tmpLeftNum = 0;
        //设置倒计时或消耗信息
        if (!freeLottery && leftSecond > 0)
        {
            usrLeftNum.text = "23LLL" + leftSecond;
            SetCountdown(subItem1, "lab_count_tip", leftSecond, new CallBack(this, "NormalLotteryCoolDown", rparam), true,"[ff0000] 后免费[-]");
        }
        else if (result.normalLottery.leftNum == 0)
            //usrLeftNum.text = "免费次数已经用完\n消耗" + tmpUseItemDesc;
			usrLeftNum.text = "[ff0000]免费次数已经用完[-]";
        else
        {
            tmpLeftNum = result.normalLottery.leftNum;
            usrLeftNum.text = "剩余免费次数x" + result.normalLottery.leftNum;
        }

        //普通单抽
        GameObject tmpNormalOne = GameCommon.FindObject(subItem1, "but_shop_buy_common_one");
//        GameCommon.SetOnlyItemIcon(GameCommon.FindObject(tmpNormalOne, "Sprite"), tmpItemData.tid);
		GameCommon.SetResIcon (tmpNormalOne,"Sprite",tmpItemData.tid,false, true);
        if (result.normalLottery.leftNum > 0 && leftSecond <= 0)
        {
            GameCommon.SetUIText(tmpNormalOne, "label_cost_info", "[FFFFFF]免费");
        }
        else
        {
            GameCommon.SetUIText(tmpNormalOne, "label_cost_info", (tmpItemData.itemNum > 0 ? "[FFFFFF]" : "[FF0000]") + tmpUseItemDesc);
        }
        //GameCommon.SetUIText(tmpNormalOne, "label_cost_info", (tmpItemData.itemNum > 0 ? "[FFFFFF]" : "[FF0000]") + tmpUseItemDesc);
        UIButtonEvent tmpBtnEvt = tmpNormalOne.GetComponent<UIButtonEvent>();
        tmpBtnEvt.mData.set("USE_ITEM_DATA", tmpItemData);
        tmpBtnEvt.mData.set("FREE_COUNT", tmpLeftNum);

        //普通十连抽
        tmpItemData = tmpDicUseItems[SHOP_PUMPING_TYPE.NORMAL_TEN];
        tmpUseItemDesc = __GetUseItemDesc(tmpItemData);
        GameObject tmpNormalTen = GameCommon.FindObject(subItem1, "but_shop_buy_common_ten");
//        GameCommon.SetOnlyItemIcon(GameCommon.FindObject(tmpNormalTen, "Sprite"), tmpItemData.tid);
		GameCommon.SetResIcon (tmpNormalTen,"Sprite",tmpItemData.tid,false, true);
        GameCommon.SetUIText(tmpNormalTen, "label_cost_info", (tmpItemData.itemNum > 0 ? "[FFFFFF]" : "[FF0000]") + tmpUseItemDesc);
        tmpBtnEvt = tmpNormalTen.GetComponent<UIButtonEvent>();
        tmpBtnEvt.mData.set("USE_ITEM_DATA", tmpItemData);

        //高级抽卡
        subItem1 = GetSub("item_shop_Info(Clone)_1");
        usrLeftNum = GameCommon.FindObject(subItem1, "lab_count_tip").GetComponent<UILabel>();
        freeLottery = result.preciousLottery.isFree == 0 ? false : true;
        leftSecond = result.preciousLottery.leftTime;
        tmpItemData = tmpDicUseItems[SHOP_PUMPING_TYPE.PRECIOUS_ONE];
        tmpUseItemDesc = __GetUseItemDesc(tmpItemData);
        tmpLeftNum = 0;
        //设置倒计时或消耗信息
        if (!freeLottery && leftSecond > 0)
        {
            usrLeftNum.text = "23LLL" + leftSecond;
            SetCountdown(subItem1, "lab_count_tip", leftSecond, new CallBack(this, "AdvanceLotteryCoolDown", rparam), true,"[ff0000] 后免费[-]");
        }
        else if (result.preciousLottery.leftNum == 0)
//            usrLeftNum.text = "免费次数已经用完\n消耗" + tmpUseItemDesc;
			usrLeftNum.text = "免费次数已经用完";
        else
        {
            tmpLeftNum = result.preciousLottery.leftNum;
            usrLeftNum.text = "剩余免费次数x" + result.preciousLottery.leftNum;
        }

        //高级单抽
        GameObject tmpPreciousOne = GameCommon.FindObject(subItem1, "but_shop_buy_advance_one");
//        GameCommon.SetOnlyItemIcon(GameCommon.FindObject(tmpPreciousOne, "Sprite"), tmpItemData.tid);
		GameCommon.SetResIcon (tmpPreciousOne,"Sprite",tmpItemData.tid,false, true);
        if (result.preciousLottery.leftNum > 0)
        {
            GameCommon.SetResIcon(tmpPreciousOne, "Sprite", (int)ITEM_TYPE.YUANBAO, false, true);
            GameCommon.SetUIText(tmpPreciousOne, "label_cost_info", "[FFFFFF]免费");
        }
        else
        {
            GameCommon.SetUIText(tmpPreciousOne, "label_cost_info", (tmpItemData.itemNum > 0 ? "[FFFFFF]" : "[FF0000]") + tmpUseItemDesc);
        }
        tmpBtnEvt = tmpPreciousOne.GetComponent<UIButtonEvent>();
        tmpBtnEvt.mData.set("USE_ITEM_DATA", tmpItemData);
        tmpBtnEvt.mData.set("FREE_COUNT", tmpLeftNum);

        //高级十连抽
        tmpItemData = tmpDicUseItems[SHOP_PUMPING_TYPE.PRECIOUS_TEN];
        tmpUseItemDesc = __GetUseItemDesc(tmpItemData);
        GameObject tmpPreciousTen = GameCommon.FindObject(subItem1, "but_shop_buy_advance_ten");
//        GameCommon.SetOnlyItemIcon(GameCommon.FindObject(tmpPreciousTen, "Sprite"), tmpItemData.tid);
		GameCommon.SetResIcon (tmpPreciousTen,"Sprite",tmpItemData.tid,false, true);
        GameCommon.SetUIText(tmpPreciousTen, "label_cost_info", (tmpItemData.itemNum > 0 ? "[FFFFFF]" : "[FF0000]") + tmpUseItemDesc);
        tmpBtnEvt = tmpPreciousTen.GetComponent<UIButtonEvent>();
        tmpBtnEvt.mData.set("USE_ITEM_DATA", tmpItemData);

        //added by xuke
        CheckPlayGridTween("grid_tween_root_1");
        //end
    }

    private List<DataRecord> mNormalCardOneRecords = new List<DataRecord>();
    private List<DataRecord> mNormalCardTenRecords = new List<DataRecord>();
    private List<DataRecord> mPreciousCardOneRecords = new List<DataRecord>();
    private List<DataRecord> mPreciousCardTenRecords = new List<DataRecord>();
    private class GroupCardData
    {
        private int mGroupID = -1;
        private int mPosition = 0;
        private int mType = 0;

        public int GroupID
        {
            set { mGroupID = value; }
            get { return mGroupID; }
        }
        public int Position
        {
            set { mPosition = value; }
            get { return mPosition; }
        }
        public int type
        {
            set { mType = value; }
            get { return mType; }
        }
    }
    private class GroupCardDataList
    {
        private int mMinLevel = 0;
        private int mMaxLevel = 0;
        private int mImportantCount = 0;
        private List<GroupCardData> mListData = new List<GroupCardData>();

        public int MinLevel
        {
            set { mMinLevel = value; }
            get { return mMinLevel; }
        }
        public int MaxLevel
        {
            set { mMaxLevel = value; }
            get { return mMaxLevel; }
        }
        public int ImportantCount
        {
            set { mImportantCount = value; }
            get { return mImportantCount; }
        }
        public List<GroupCardData> ListData
        {
            set { mListData = value; }
            get { return mListData; }
        }
    }
    private Dictionary<string, GroupCardDataList> mDicGroupCardData = new Dictionary<string, GroupCardDataList>();
    private int mTotalGroupCardDataCount = 0;
    private enum GetCardBtnPosType
    {
        Left,
        Right,
        Middle
    }
    private Vector3[] mGetCardBtnPos =
    {
        new Vector3(-155.0f, -225.0f, 0.0f),
        new Vector3(-5.0f, -225.0f, 0.0f),
        new Vector3(-85.0f, -225.0f, 0.0f)
    };
    private void __RefreshCardShop2(NewShopWindowParams param)
    {
        SC_ResponseLotteryQuery tmpResult = (SC_ResponseLotteryQuery)param.obj;
        set("LAST_LOTTERY_QUERY", tmpResult);

        GameObject tmpParent = GetSub("scrollview_context_1");
        tmpParent.SetActive(true);
        GameObject tmpContext2 = GetSub("scrollview_context_2");
        if (tmpContext2 != null)
            tmpContext2.SetActive(false);
        GameObject tmpContext3 = GetSub("scrollview_context_3");
        if (tmpContext3 != null)
            tmpContext3.SetActive(false);

        int tmpLeftSilverCount = PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND);
        int tmpLeftGoldCount = PackageManager.GetItemLeftCount((int)ITEM_TYPE.GOLD_FL_COMMAND);

        GameObject tmpFLCountParent = GameCommon.FindObject(tmpParent, "usr_item_package");

        //现有银符灵
        GameCommon.SetUIText(tmpFLCountParent, "label_yfl_desc", tmpLeftSilverCount.ToString());

        //现有金符灵
        GameCommon.SetUIText(tmpFLCountParent, "label_jfl_desc", tmpLeftGoldCount.ToString());

        __RefreshNormalGetCard(param);
        __RefreshPreciousGetCard(param);

        if (param.mIsPlayWinAnim)
        {
            //added by xuke
            CheckPlayGridTween("grid_tween_root_1");
            //end
        }
    }
    public ItemDataBase GetCurentCostData(SHOP_PUMPING_TYPE type)
    {
        ItemDataBase tmpCostData = new ItemDataBase();
        int tmpLeftSilverCount = PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND);
        int tmpLeftGoldCount = PackageManager.GetItemLeftCount((int)ITEM_TYPE.GOLD_FL_COMMAND);

        switch (type)
        {
            case SHOP_PUMPING_TYPE.NORMAL_ONE:
                {
                    tmpCostData.tid = (int)ITEM_TYPE.SILVER_FL_COMMAND;
                    tmpCostData.itemId = GameCommon.GetItemId((int)ITEM_TYPE.SILVER_FL_COMMAND);
                    DataRecord tmpNormalOneRecord = __GetCardRecord(GetCardType.NormalOne);
                    int tmpNormalOneCost = (int)tmpNormalOneRecord.getObject("PUMPING_PRICE_1");
                    int tmpNormalOneMulti = (int)tmpNormalOneRecord.getObject("PUMPING_MULTIPLE");
                    tmpNormalOneCost *= tmpNormalOneMulti;
                    tmpCostData.itemNum = tmpNormalOneCost;
                } break;
            case SHOP_PUMPING_TYPE.NORMAL_TEN:
                {
                    tmpCostData.tid = (int)ITEM_TYPE.SILVER_FL_COMMAND;
                    tmpCostData.itemId = GameCommon.GetItemId((int)ITEM_TYPE.SILVER_FL_COMMAND);
                    DataRecord tmpNormalTenRecord = __GetCardRecord(GetCardType.NormalTen);
                    int tmpNormalTenCost = (int)tmpNormalTenRecord.getObject("PUMPING_PRICE_1");
                    int tmpNormalTenMulti = (int)tmpNormalTenRecord.getObject("PUMPING_MULTIPLE");
                    tmpNormalTenCost *= tmpNormalTenMulti;
                    tmpCostData.itemNum = tmpNormalTenCost;
                } break;
            case SHOP_PUMPING_TYPE.PRECIOUS_ONE:
                {
                    DataRecord tmpPreciousOneRecord = __GetCardRecord(GetCardType.PreciousOne);
                    int tmpPreciousOneCost = 0;
                    int tmpPreciousOneMulti = (int)tmpPreciousOneRecord.getObject("PUMPING_MULTIPLE");
                    if (tmpLeftGoldCount >= 1)
                    {
                        tmpPreciousOneCost = (int)tmpPreciousOneRecord.getObject("PUMPING_PRICE_2");
                        tmpCostData.tid = (int)ITEM_TYPE.GOLD_FL_COMMAND;
                        tmpCostData.itemId = GameCommon.GetItemId((int)ITEM_TYPE.GOLD_FL_COMMAND);
                    }
                    else
                    {
                        tmpPreciousOneCost = (int)tmpPreciousOneRecord.getObject("PUMPING_PRICE_3");
                        tmpCostData.tid = (int)ITEM_TYPE.YUANBAO;
                        tmpCostData.itemId = GameCommon.GetItemId((int)ITEM_TYPE.YUANBAO);
                    }
                    tmpPreciousOneCost *= tmpPreciousOneMulti;
                    tmpCostData.itemNum = tmpPreciousOneCost;
                } break;
            case SHOP_PUMPING_TYPE.PRECIOUS_TEN:
                {
                    tmpCostData.tid = (int)ITEM_TYPE.YUANBAO;
                    tmpCostData.itemId = GameCommon.GetItemId((int)ITEM_TYPE.YUANBAO);
                    DataRecord tmpPreciousTenRecord = __GetCardRecord(GetCardType.PreciousTen);
                    int tmpPreciousTenCost = (int)tmpPreciousTenRecord.getObject("PUMPING_PRICE_3");
                    int tmpPreciousTenMulti = (int)tmpPreciousTenRecord.getObject("PUMPING_MULTIPLE");
                    tmpPreciousTenCost *= tmpPreciousTenMulti;
                    tmpCostData.itemNum = tmpPreciousTenCost;
                } break;
        }

        return tmpCostData;
    }
    /// <summary>
    /// 刷新普通抽卡
    /// </summary>
    private void __RefreshNormalGetCard(NewShopWindowParams param)
    {
        SC_ResponseLotteryQuery tmpResult = (SC_ResponseLotteryQuery)param.obj;

        GameObject tmpParent = GetSub("scrollview_context_1");

        int tmpLeftSilverCount = PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND);

        GameObject tmpNormalItem = GameCommon.FindObject(tmpParent, "item_shop_Info(Clone)_0");
        GameCommon.SetUIVisiable(tmpNormalItem, false, "but_shop_buy_common_one", "but_shop_buy_common_ten", "but_shop_buy_common_free");
        GameCommon.SetUIVisiable(tmpNormalItem, false, "lab_count_tip");

		//让CoountdownUI无效
		CountdownUI tmpCountdown = GameCommon.FindComponent<CountdownUI> (tmpNormalItem, "lab_count_tip");
		if (tmpCountdown != null) {
			tmpCountdown.enabled=false;
		}

        if (tmpResult.normalLottery.leftNum > 0 && tmpResult.normalLottery.isFree == 1)
        {
            UILabel tmpLBUsrLeftNum = GameCommon.FindObject(tmpNormalItem, "lab_count_tip").GetComponent<UILabel>();
            tmpLBUsrLeftNum.gameObject.SetActive(true);
            GameCommon.SetUIPosition(tmpNormalItem, "but_shop_buy_common_free", mGetCardBtnPos[(int)GetCardBtnPosType.Middle]);
            GameCommon.SetUIVisiable(tmpNormalItem, true, "but_shop_buy_common_free");

            DataRecord tmpRecord = __GetCardRecord(GetCardType.NormalOne);
            int tmpFreeCount = (int)tmpRecord.getObject("FREE_NUMBER");
            tmpLBUsrLeftNum.text = "本次免费（" + tmpResult.normalLottery.leftNum + "/" + tmpFreeCount + "）";

            UIButtonEvent tmpBtnEvt = GameCommon.FindComponent<UIButtonEvent>(tmpNormalItem, "but_shop_buy_common_free");
            tmpBtnEvt.mData.set("USE_ITEM_DATA", new ItemDataBase()
            {
                tid = (int)ITEM_TYPE.SILVER_FL_COMMAND,
                itemId = (int)GameCommon.GetItemId((int)ITEM_TYPE.SILVER_FL_COMMAND),
                itemNum = -1
            });
            tmpBtnEvt.mData.set("FREE_COUNT", tmpResult.normalLottery.leftNum);
        }
        else if (
            tmpLeftSilverCount >= 1 && tmpLeftSilverCount < 10 &&
            (tmpResult.normalLottery.leftNum <= 0 ||
            tmpResult.normalLottery.isFree == 0))
        {
            GameCommon.SetUIPosition(tmpNormalItem, "but_shop_buy_common_one", mGetCardBtnPos[(int)GetCardBtnPosType.Middle]);
            GameCommon.SetUIVisiable(tmpNormalItem, true, "but_shop_buy_common_one");

            //设置抽卡按钮信息
            //普通单抽
            DataRecord tmpNormalOneRecord = __GetCardRecord(GetCardType.NormalOne);
            GameObject tmpNormalOne = GameCommon.FindObject(tmpNormalItem, "but_shop_buy_common_one");
            GameCommon.SetResIcon(tmpNormalOne, "Sprite", (int)ITEM_TYPE.SILVER_FL_COMMAND, false, true);
            int tmpNormalOneCost = (int)tmpNormalOneRecord.getObject("PUMPING_PRICE_1");
            int tmpNormalOneMulti = (int)tmpNormalOneRecord.getObject("PUMPING_MULTIPLE");
            tmpNormalOneCost *= tmpNormalOneMulti;
            GameCommon.SetUIText(tmpNormalOne, "label_cost_info", "[FFFFFF]x" + tmpNormalOneCost);
            GameCommon.SetUIText(tmpNormalOne, "card_button_label", "购买一次");
            UIButtonEvent tmpBtnEvt = tmpNormalOne.GetComponent<UIButtonEvent>();
            tmpBtnEvt.mData.set("USE_ITEM_DATA", new ItemDataBase()
            {
                tid = (int)ITEM_TYPE.SILVER_FL_COMMAND,
                itemId = GameCommon.GetItemId((int)ITEM_TYPE.SILVER_FL_COMMAND),
                itemNum = tmpNormalOneCost
            });
            tmpBtnEvt.mData.set("FREE_COUNT", tmpResult.normalLottery.leftNum);
        }
        else if (
            (tmpLeftSilverCount >= 10 || tmpLeftSilverCount <= 0) &&
            (tmpResult.normalLottery.leftNum <= 0 ||
            tmpResult.normalLottery.isFree == 0))
        {
            GameCommon.SetUIPosition(tmpNormalItem, "but_shop_buy_common_one", mGetCardBtnPos[(int)GetCardBtnPosType.Left]);
            GameCommon.SetUIPosition(tmpNormalItem, "but_shop_buy_common_ten", mGetCardBtnPos[(int)GetCardBtnPosType.Right]);
            GameCommon.SetUIVisiable(tmpNormalItem, true, "but_shop_buy_common_one", "but_shop_buy_common_ten");

            //设置抽卡按钮信息
            //普通单抽
            DataRecord tmpNormalOneRecord = __GetCardRecord(GetCardType.NormalOne);
            GameObject tmpNormalOne = GameCommon.FindObject(tmpNormalItem, "but_shop_buy_common_one");
            GameCommon.SetResIcon(tmpNormalOne, "Sprite", (int)ITEM_TYPE.SILVER_FL_COMMAND, false, true);
            int tmpNormalOneCost = (int)tmpNormalOneRecord.getObject("PUMPING_PRICE_1");
            int tmpNormalOneMulti = (int)tmpNormalOneRecord.getObject("PUMPING_MULTIPLE");
            tmpNormalOneCost *= tmpNormalOneMulti;
            GameCommon.SetUIText(tmpNormalOne, "label_cost_info", (tmpLeftSilverCount >= tmpNormalOneCost ? "[FFFFFF]x" : "[FF0000]x") + tmpNormalOneCost);
            GameCommon.SetUIText(tmpNormalOne, "card_button_label", "购买一次");
            UIButtonEvent tmpBtnEvt = tmpNormalOne.GetComponent<UIButtonEvent>();
            tmpBtnEvt.mData.set("USE_ITEM_DATA", new ItemDataBase()
            {
                tid = (int)ITEM_TYPE.SILVER_FL_COMMAND,
                itemId = GameCommon.GetItemId((int)ITEM_TYPE.SILVER_FL_COMMAND),
                itemNum = tmpNormalOneCost
            });
            tmpBtnEvt.mData.set("FREE_COUNT", tmpResult.normalLottery.leftNum);

            //普通十抽
            DataRecord tmpNormalTenRecord = __GetCardRecord(GetCardType.NormalTen);
            GameObject tmpNormalTen = GameCommon.FindObject(tmpNormalItem, "but_shop_buy_common_ten");
            GameCommon.SetResIcon(tmpNormalTen, "Sprite", (int)ITEM_TYPE.SILVER_FL_COMMAND, false, true);
            int tmpNormalTenCost = (int)tmpNormalTenRecord.getObject("PUMPING_PRICE_1");
            int tmpNormalTenMulti = (int)tmpNormalTenRecord.getObject("PUMPING_MULTIPLE");
            tmpNormalTenCost *= tmpNormalTenMulti;
            GameCommon.SetUIText(tmpNormalTen, "label_cost_info", (tmpLeftSilverCount >= tmpNormalTenCost ? "[FFFFFF]x" : "[FF0000]x") + tmpNormalTenCost);
            GameCommon.SetUIText(tmpNormalTen, "card_button_label", "购买十次");
            tmpBtnEvt = tmpNormalTen.GetComponent<UIButtonEvent>();
            tmpBtnEvt.mData.set("USE_ITEM_DATA", new ItemDataBase()
            {
                tid = (int)ITEM_TYPE.SILVER_FL_COMMAND,
                itemId = GameCommon.GetItemId((int)ITEM_TYPE.SILVER_FL_COMMAND),
                itemNum = tmpNormalTenCost
            });
        }
        if (tmpResult.normalLottery.isFree == 0)
        {
            UILabel tmpLBUsrLeftNum = GameCommon.FindObject(tmpNormalItem, "lab_count_tip").GetComponent<UILabel>();
            if (tmpResult.normalLottery.leftNum > 0 || tmpResult.normalLottery.leftTime > 0)
            {
                tmpLBUsrLeftNum.gameObject.SetActive(true);
                tmpLBUsrLeftNum.text = "23LLL" + tmpResult.normalLottery.leftTime;
                SetCountdown(tmpNormalItem, "lab_count_tip", tmpResult.normalLottery.leftTime, new CallBack(this, "NormalLotteryCoolDown", param), true, "[ff0000] 后免费[-]");
            }
            else if(tmpResult.normalLottery.leftNum <= 0)
            {
                tmpLBUsrLeftNum.gameObject.SetActive(true);
                tmpLBUsrLeftNum.text = "[FF0000]免费次数已用完[-]";
            }
        }
    }
    /// <summary>
    /// 刷新高级抽卡
    /// </summary>
    private void __RefreshPreciousGetCard(NewShopWindowParams param)
    {
        SC_ResponseLotteryQuery tmpResult = (SC_ResponseLotteryQuery)param.obj;

        GameObject tmpParent = GetSub("scrollview_context_1");

        int tmpLeftGoldCount = PackageManager.GetItemLeftCount((int)ITEM_TYPE.GOLD_FL_COMMAND);

        GameObject tmpPreciousItem = GameCommon.FindObject(tmpParent, "item_shop_Info(Clone)_1");
        GameCommon.SetUIVisiable(tmpPreciousItem, false, "but_shop_buy_advance_one", "but_shop_buy_advance_ten", "but_shop_buy_advance_free");
        GameCommon.SetUIVisiable(tmpPreciousItem, false, "lab_count_tip");

        if (tmpResult.preciousLottery.leftNum > 0 && tmpResult.preciousLottery.isFree == 1)
        {
            UILabel tmpLBUsrLeftNum = GameCommon.FindObject(tmpPreciousItem, "lab_count_tip").GetComponent<UILabel>();
            tmpLBUsrLeftNum.gameObject.SetActive(true);
            GameCommon.SetUIPosition(tmpPreciousItem, "but_shop_buy_advance_free", mGetCardBtnPos[(int)GetCardBtnPosType.Middle]);
            GameCommon.SetUIVisiable(tmpPreciousItem, true, "but_shop_buy_advance_free");

            DataRecord tmpRecord = __GetCardRecord(GetCardType.PreciousOne);
            int tmpFreeCount = (int)tmpRecord.getObject("FREE_NUMBER");
            tmpLBUsrLeftNum.text = "本次免费（" + tmpResult.preciousLottery.leftNum + "/" + tmpFreeCount + "）";

            UIButtonEvent tmpBtnEvt = GameCommon.FindComponent<UIButtonEvent>(tmpPreciousItem, "but_shop_buy_advance_free");
            tmpBtnEvt.mData.set("USE_ITEM_DATA", new ItemDataBase()
            {
                tid = (int)ITEM_TYPE.YUANBAO,
                itemId = GameCommon.GetItemId((int)ITEM_TYPE.YUANBAO),
                itemNum = -1
            });
            tmpBtnEvt.mData.set("FREE_COUNT", tmpResult.preciousLottery.leftNum);
        }
        else if (
            tmpLeftGoldCount >= 1 &&
            (tmpResult.preciousLottery.leftNum <= 0 ||
            tmpResult.preciousLottery.isFree == 0))
        {
            GameCommon.SetUIPosition(tmpPreciousItem, "but_shop_buy_advance_one", mGetCardBtnPos[(int)GetCardBtnPosType.Middle]);
            GameCommon.SetUIVisiable(tmpPreciousItem, true, "but_shop_buy_advance_one");

            //高级单抽
            DataRecord tmpPreciousOneRecord = __GetCardRecord(GetCardType.PreciousOne);
            GameObject tmpPreciousOne = GameCommon.FindObject(tmpPreciousItem, "but_shop_buy_advance_one");
            GameCommon.SetResIcon(tmpPreciousOne, "Sprite", (int)ITEM_TYPE.GOLD_FL_COMMAND, false, true);
            int tmpPreciousOneCost = (int)tmpPreciousOneRecord.getObject("PUMPING_PRICE_2");
            int tmpPreciousOneMulti = (int)tmpPreciousOneRecord.getObject("PUMPING_MULTIPLE");
            tmpPreciousOneCost *= tmpPreciousOneMulti;
            GameCommon.SetUIText(tmpPreciousOne, "label_cost_info", "[FFFFFF]x" + tmpPreciousOneCost);
            GameCommon.SetUIText(tmpPreciousOne, "card_button_label", "购买一次");
            UIButtonEvent tmpBtnEvt = tmpPreciousOne.GetComponent<UIButtonEvent>();
            tmpBtnEvt.mData.set("USE_ITEM_DATA", new ItemDataBase()
            {
                tid = (int)ITEM_TYPE.GOLD_FL_COMMAND,
                itemId = GameCommon.GetItemId((int)ITEM_TYPE.GOLD_FL_COMMAND),
                itemNum = tmpPreciousOneCost
            });
            tmpBtnEvt.mData.set("FREE_COUNT", tmpResult.preciousLottery.leftNum);
        }
        else if (tmpLeftGoldCount == 0 &&
            (tmpResult.preciousLottery.leftNum <= 0 ||
            tmpResult.preciousLottery.isFree == 0))
        {
            GameCommon.SetUIPosition(tmpPreciousItem, "but_shop_buy_advance_one", mGetCardBtnPos[(int)GetCardBtnPosType.Left]);
            GameCommon.SetUIPosition(tmpPreciousItem, "but_shop_buy_advance_ten", mGetCardBtnPos[(int)GetCardBtnPosType.Right]);
            GameCommon.SetUIVisiable(tmpPreciousItem, true, "but_shop_buy_advance_one", "but_shop_buy_advance_ten");

            //高级单抽
            DataRecord tmpPreciousOneRecord = __GetCardRecord(GetCardType.PreciousOne);
            GameObject tmpPreciousOne = GameCommon.FindObject(tmpPreciousItem, "but_shop_buy_advance_one");
            GameCommon.SetResIcon(tmpPreciousOne, "Sprite", (int)ITEM_TYPE.YUANBAO, false, true);
            int tmpPreciousOneCost = (int)tmpPreciousOneRecord.getObject("PUMPING_PRICE_3");
            int tmpPreciousOneMulti = (int)tmpPreciousOneRecord.getObject("PUMPING_MULTIPLE");
            tmpPreciousOneCost *= tmpPreciousOneMulti;
            GameCommon.SetUIText(tmpPreciousOne, "label_cost_info", ((RoleLogicData.Self.diamond >= tmpPreciousOneCost) ? "[FFFFFF]x" : "[FF0000]x") + tmpPreciousOneCost);
            GameCommon.SetUIText(tmpPreciousOne, "card_button_label", "购买一次");
            UIButtonEvent tmpBtnEvt = tmpPreciousOne.GetComponent<UIButtonEvent>();
            tmpBtnEvt.mData.set("USE_ITEM_DATA", new ItemDataBase()
            {
                tid = (int)ITEM_TYPE.YUANBAO,
                itemId = GameCommon.GetItemId((int)ITEM_TYPE.YUANBAO),
                itemNum = tmpPreciousOneCost
            });
            tmpBtnEvt.mData.set("FREE_COUNT", tmpResult.preciousLottery.leftNum);

            //高级十连抽
            DataRecord tmpPreciousTenRecord = __GetCardRecord(GetCardType.PreciousTen);
            GameObject tmpPreciousTen = GameCommon.FindObject(tmpPreciousItem, "but_shop_buy_advance_ten");
            GameCommon.SetResIcon(tmpPreciousTen, "Sprite", (int)ITEM_TYPE.YUANBAO, false, true);
            int tmpPreciousTenCost = (int)tmpPreciousTenRecord.getObject("PUMPING_PRICE_3");
            int tmpPreciousTenMulti = (int)tmpPreciousTenRecord.getObject("PUMPING_MULTIPLE");
            tmpPreciousTenCost *= tmpPreciousTenMulti;
            GameCommon.SetUIText(tmpPreciousTen, "label_cost_info", ((RoleLogicData.Self.diamond >= tmpPreciousTenCost) ? "[FFFFFF]x" : "[FF0000]x") + tmpPreciousTenCost);
            GameCommon.SetUIText(tmpPreciousTen, "card_button_label", "购买十次");
            tmpBtnEvt = tmpPreciousTen.GetComponent<UIButtonEvent>();
            tmpBtnEvt.mData.set("USE_ITEM_DATA", new ItemDataBase()
            {
                tid = (int)ITEM_TYPE.YUANBAO,
                itemId = GameCommon.GetItemId((int)ITEM_TYPE.YUANBAO),
                itemNum = tmpPreciousTenCost
            });
        }
        if (tmpResult.preciousLottery.isFree == 0)
        {
            UILabel tmpLBUsrLeftNum = GameCommon.FindObject(tmpPreciousItem, "lab_count_tip").GetComponent<UILabel>();
            if (tmpResult.preciousLottery.leftNum > 0 || tmpResult.preciousLottery.leftTime > 0)
            {
                tmpLBUsrLeftNum.gameObject.SetActive(true);
                tmpLBUsrLeftNum.text = "23LLL" + tmpResult.preciousLottery.leftTime;
                SetCountdown(tmpPreciousItem, "lab_count_tip", tmpResult.preciousLottery.leftTime, new CallBack(this, "AdvanceLotteryCoolDown", param), true, "[ff0000] 后免费[-]");
            }
            else if (tmpResult.preciousLottery.leftNum <= 0)
            {
                tmpLBUsrLeftNum.gameObject.SetActive(true);
                tmpLBUsrLeftNum.text = "[FF0000]免费次数已用完[-]";
            }
        }

        //剩余购买次数送符灵提示
        UILabel tmpLBGoalsBuyTimes = GameCommon.FindComponent<UILabel>(tmpParent, "goals_buy_times_label");
        if (tmpLBGoalsBuyTimes != null)
            tmpLBGoalsBuyTimes.text = __GetImportantLeftCountInfo(tmpResult.times);
    }

    /// <summary>
    /// 强制普通抽卡
    /// </summary>
    public void ForceClickNormalOne()
    {
        GameObject tmpParent = GetSub("scrollview_context_1");
        GameObject subItem1 = GameCommon.FindObject(tmpParent, "item_shop_Info(Clone)_0");
        GameObject tmpNormalOne = GameCommon.FindObject(subItem1, "but_shop_buy_common_one");
        UIButtonEvent tmpBtnEvt = tmpNormalOne.GetComponent<UIButtonEvent>();
        tmpBtnEvt.mData.set("FORCE_CLICK", true);
        tmpBtnEvt.Send();
    }
    /// <summary>
    /// 强制普通十连抽
    /// </summary>
    public void ForceClickNormalTen()
    {
        GameObject tmpParent = GetSub("scrollview_context_1");
        GameObject subItem1 = GameCommon.FindObject(tmpParent, "item_shop_Info(Clone)_0");
        GameObject tmpNormalTen = GameCommon.FindObject(subItem1, "but_shop_buy_common_ten");
        UIButtonEvent tmpBtnEvt = tmpNormalTen.GetComponent<UIButtonEvent>();
        tmpBtnEvt.mData.set("FORCE_CLICK", true);
        tmpBtnEvt.Send();
    }
    /// <summary>
    /// 强制高级抽卡
    /// </summary>
    public void ForceClickPreciousOne()
    {
        GameObject tmpParent = GetSub("scrollview_context_1");
        GameObject subItem1 = GameCommon.FindObject(tmpParent, "item_shop_Info(Clone)_1");
        GameObject tmpPreciousOne = GameCommon.FindObject(subItem1, "but_shop_buy_advance_one");
        UIButtonEvent tmpBtnEvt = tmpPreciousOne.GetComponent<UIButtonEvent>();
        tmpBtnEvt.mData.set("FORCE_CLICK", true);
        tmpBtnEvt.Send();
    }
    /// <summary>
    /// 强制高级十连抽
    /// </summary>
    public void ForceClickPreciousTen()
    {
        GameObject tmpParent = GetSub("scrollview_context_1");
        GameObject subItem1 = GameCommon.FindObject(tmpParent, "item_shop_Info(Clone)_1");
        GameObject tmpPreciousOne = GameCommon.FindObject(subItem1, "but_shop_buy_advance_ten");
        UIButtonEvent tmpBtnEvt = tmpPreciousOne.GetComponent<UIButtonEvent>();
        tmpBtnEvt.mData.set("FORCE_CLICK", true);
        tmpBtnEvt.Send();
    }

    /// <summary>
    /// 根据服务器数据，计算每个物品最终可购买个数
    /// </summary>
    /// <param name="resp"></param>
    /// <returns></returns>
    private Dictionary<int, ShopPropData> __GetFinalItemData(SC_ResponsePropShopQuery resp, out Dictionary<int, int> dicResp)
    {
        dicResp = new Dictionary<int,int>();
        Dictionary<int, ShopPropData> tmpDicItems = new Dictionary<int, ShopPropData>();
        foreach (KeyValuePair<int, DataRecord> tmpPair in mDicItemsConfig)
        {
            string[] tmpBuyCount = tmpPair.Value.getObject("BUY_NUM").ToString().Split('|');
            string tmpStrCountByVIP = tmpBuyCount[0];
            if(RoleLogicData.Self.vipLevel > 0 && RoleLogicData.Self.vipLevel < tmpBuyCount.Length)
                tmpStrCountByVIP = tmpBuyCount[RoleLogicData.Self.vipLevel];
            int tmpCountByVIP = int.Parse(tmpStrCountByVIP);
            if (tmpCountByVIP == 0)
                tmpCountByVIP = -1;
            tmpDicItems[tmpPair.Key] = new ShopPropData() { index = tmpPair.Key, buyNum = tmpCountByVIP };
        }
        for (int i = 0, count = resp.prop.Count; i < count; i++)
        {
            ShopPropData tmpShopData;
            if (!tmpDicItems.TryGetValue(resp.prop[i].index, out tmpShopData))
                continue;
            if (tmpShopData.buyNum == -1)
                continue;
            dicResp[resp.prop[i].index] = resp.prop[i].buyNum;
            tmpShopData.buyNum = Mathf.Max(0, tmpShopData.buyNum - resp.prop[i].buyNum);
        }
        return tmpDicItems;
    }

    public void sortTmpDicItems(Dictionary<int, ShopPropData> tmpDicItemsOld, out Dictionary<int, ShopPropData> tmpDicItemsNew)
    {
        Dictionary<int, ShopPropData> tmpDicItemsTemp = new Dictionary<int, ShopPropData>();
        foreach (KeyValuePair<int, ShopPropData> tmpPair in tmpDicItemsOld)
        {
            ShopPropData item = tmpPair.Value;
            if (item != null && item.buyNum != 0)
            {
                tmpDicItemsTemp.Add(tmpPair.Key, item);
            }
        }
        foreach (KeyValuePair<int, ShopPropData> tmpPair in tmpDicItemsOld)
        {
            ShopPropData item = tmpPair.Value;
            if (item != null && item.buyNum == 0 )
            {
                tmpDicItemsTemp.Add(tmpPair.Key, item);
            }
        }
        tmpDicItemsNew = tmpDicItemsTemp;
    }

    private void __RefreshItemShop(NewShopWindowParams rparam)
    {
        if (rparam == null)
            return;

        set("LAST_ITEMSHOP_DATA", rparam);
        GameObject tmpContext2 = GetSub("scrollview_context_2");
        tmpContext2.SetActive(true);
        GameObject tmpContext1 = GetSub("scrollview_context_1");
        if (tmpContext1 != null)
            tmpContext1.SetActive(false);
        GameObject tmpContext3 = GetSub("scrollview_context_3");
        if (tmpContext3 != null)
            tmpContext3.SetActive(false);

        //get data
        Dictionary<int, int> tmpDicResp = null;
        Dictionary<int, ShopPropData> tmpDicItemsOld = __GetFinalItemData(rparam.obj as SC_ResponsePropShopQuery, out tmpDicResp);
        Dictionary<int, ShopPropData> tmpDicItems;
        //sortTmpDicItems(tmpDicItemsOld, out tmpDicItems);
        tmpDicItems = tmpDicItemsOld;

        if (tmpDicItems == null)
            return;
        UIGridContainer grid = null;
        grid = GetComponent<UIGridContainer>("shop_goods_grid");
        if (grid != null)
        {
            grid.MaxCount = tmpDicItems.Count;
            int i = 0;
            foreach (KeyValuePair<int, ShopPropData> tmpPair in tmpDicItems)
            {
                ShopPropData tmpShopData = tmpPair.Value;
                GameObject item = grid.controlList[i++];
                item.name = "shop_item_buy_event";

                DataRecord tmpConfig = mDicItemsConfig[tmpShopData.index];

                int tmpItemTid = (int)tmpConfig["ITEM_ID"];

                string itemName = GameCommon.GetItemName(tmpItemTid);
                string itemInfo = GameCommon.GetItemDesc(tmpItemTid);
				string itemNum = "x" + tmpConfig.getObject("ITEM_NUM").ToString();
                // icon setting
//                GameCommon.SetItemIcon(item, "icon_goods", tmpItemTid);
				GameCommon.SetOnlyItemIcon(item, "icon_goods", tmpItemTid);
				AddButtonAction (GameCommon.FindObject (item, "icon_goods").gameObject, () =>GameCommon.SetItemDetailsWindow(tmpItemTid));


                // icon end
                string totalPrice = tmpConfig["COST_NUM_1"].ToString();
                int tmpCostTid = (int)tmpConfig.getObject("COST_TYPE_1");
                string[] prices = totalPrice.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                GameCommon.SetUIText(item, "goods_name_label", itemName);
                GameCommon.SetUIText(item, "goods_info_label", itemInfo);
				GameCommon.SetUIText(item,"item_num",itemNum);

                int tmpBuyNum = 0;
                if (!tmpDicResp.TryGetValue(tmpPair.Key, out tmpBuyNum))
                    tmpBuyNum = 0;
                int tmpPrice = int.Parse((tmpBuyNum >= prices.Length) ? prices[prices.Length - 1] : prices[tmpBuyNum]);
                GameCommon.SetUIText(item, "need_buy_num_label", "x" + tmpPrice.ToString());
//                GameCommon.SetOnlyItemIcon(item, "shop_cost_icon", tmpCostTid);
				GameCommon.SetResIcon(item, "shop_cost_icon", tmpCostTid, false, true);
                UILabel tmpLBLimit = GameCommon.FindComponent<UILabel>(item, "goods_limit_label02");
                tmpLBLimit.color = Color.green;
                tmpLBLimit.gameObject.SetActive(tmpShopData.buyNum != -1);
                if (tmpShopData.buyNum == -1)
                {
                    GameCommon.FindObject(item, "goods_limit_label02").SetActive(false);
                    GameCommon.FindObject(item, "goods_limit_label03").SetActive(false);
                    GameCommon.FindObject(item, "buy_button").GetComponent<UIImageButton>().isEnabled = true;
                }
                else if (tmpShopData.buyNum == 0)
                {
                    GameCommon.FindObject(item, "goods_limit_label02").SetActive(false);
                    GameCommon.FindObject(item, "goods_limit_label03").SetActive(true);
                    GameCommon.FindObject(item, "buy_button").GetComponent<UIImageButton>().isEnabled = true;
                }
                else
                {
                    tmpLBLimit.text = "可以购买" + tmpShopData.buyNum.ToString() + "次";
                    GameCommon.FindObject(item, "goods_limit_label02").SetActive(true);
                    GameCommon.FindObject(item, "goods_limit_label03").SetActive(false);
                    GameCommon.FindObject(item, "buy_button").GetComponent<UIImageButton>().isEnabled = true;
                }
                //tmpLBLimit.text = tmpLBLimit.text.ToString().SetTextColor(LabelColor.Green);

                NiceData butData = GameCommon.GetButtonData(item, "buy_button");
                if (butData != null)
                {
                    butData.set("HAS_BUY_NUM", tmpBuyNum);
                    butData.set("LEFT_BUY_NUM", tmpShopData.buyNum);
                    butData.set("ITEM_ID_TEST", tmpShopData.index);
                    butData.set("NEED_COST_YB", tmpPrice);
                    butData.set("SHOP_TYPE", 1);
                    butData.set("COST_TID", tmpCostTid);
                    butData.set("ITEM_TID", tmpItemTid);
                }
                else
                    DEBUG.LogError("No add button event");
            }
            //added by xuke
            CheckPlayGridTween("grid_tween_root_2");
            //end
        }
    }
    protected override void PlayGridTween()
    {
        GridTransformTweenAnim _transTween = mGridTweenRoot.GetComponent<GridTransformTweenAnim>();
        if (_transTween != null) 
        {
            Transform _trans = mGridTweenRoot.transform.GetChild(0);
            _transTween.PlayGridTween(_trans.GetComponent<UIGridContainer>());
        }
    }

    /// <summary>
    /// 根据服务器数据计算可以显示的VIP礼包
    /// </summary>
    /// <param name="resp"></param>
    /// <returns></returns>
    private List<DataRecord> __GetFinalVIPItemData(SC_VIPShopQuery resp)
    {
        int tmpCurrVIPLv = RoleLogicData.Self.vipLevel;
        List<DataRecord> tmpListItems = new List<DataRecord>();
        foreach (KeyValuePair<int, DataRecord> tmpPair in mDicVIPConfig)
            tmpListItems.Add(tmpPair.Value);
        tmpListItems.RemoveAll((DataRecord tmpConfig) =>
        {
            int tmpDisLvLimit = (int)tmpConfig.getObject("VIP_LEVEL_DISPLAY");
            if (tmpCurrVIPLv < tmpDisLvLimit)
                return true;

            ShopPropData tmpRespPropData = resp.buyInfo.Find((ShopPropData tmp) => { return (tmp.index == (int)tmpConfig.getObject("INDEX")); });
            if (tmpRespPropData == null)
                return false;
            return (tmpRespPropData.buyNum == 1);
        });
        return tmpListItems;
    }
    private void __RefreshVIPShop(NewShopWindowParams rparam)
    {
        if (rparam == null)
            return;

        set("LAST_VIPSHOP_DATA", rparam);
        GameObject tmpContext3 = GetSub("scrollview_context_3");
        tmpContext3.SetActive(true);
        GameObject tmpContext1 = GetSub("scrollview_context_1");
        if (tmpContext1 != null)
            tmpContext1.SetActive(false);
        GameObject tmpContext2 = GetSub("scrollview_context_2");
        if (tmpContext2 != null)
            tmpContext2.SetActive(false);

        List<DataRecord> tmpListItems = __GetFinalVIPItemData(rparam.obj as SC_VIPShopQuery);
        if (tmpListItems == null)
            return;
        UIGridContainer grid = null;
        grid = GetComponent<UIGridContainer>("vip_list_grid");
        if (grid != null)
        {
            grid.MaxCount = tmpListItems.Count;
            for (int i = 0, count = tmpListItems.Count; i < count; i++)
            {
                DataRecord tmpConfig = tmpListItems[i];
                GameObject item = grid.controlList[i];

                int tmpIndex = (int)tmpConfig.getObject("INDEX");
                string tmpStrLv = tmpIndex.ToString().Substring(3);
                int tmpLv = int.Parse(tmpStrLv) - 1;
                bool tmpIsStandard = VIPHelper.IsReachStandard(tmpLv);
                int tmpVIPItemTid = (int)tmpConfig.getObject("ITEM_ID");

                GameCommon.SetUIText(item, "lab_title_label", "VIP" + tmpLv.ToString() + "超值礼包");
                GameCommon.SetResIcon(item, "icon_sprite", (int)tmpConfig.getObject("COST_TYPE_1"), false, true);
                GameCommon.SetUIText(item, "icon_label", ((int)tmpConfig["COST_NUM_1"]).ToString());
                string tmpDesc = GameCommon.GetItemDesc(tmpVIPItemTid);
                tmpDesc = "[ffc579]" + tmpDesc.Replace("\\n", "\n");
                GameCommon.SetUIText(item, "lab_description_label", tmpDesc);
                //if (VIPHelper.IsReachStandard(tmpLv))
                //    GameCommon.SetUIVisiable(item, "limit", false);
                //else
                //{
                //    GameCommon.SetUIText(item, "limit", "至尊" + tmpLv + "级可购买");
                //    GameCommon.SetUIVisiable(item, "limit", true);
                //}

                GameCommon.SetUIText(item, "limit", "VIP" + tmpLv + " 可购买");
                GameCommon.SetUIVisiable(item, "limit", true);
                //added by xuke VIP礼包红点
                GameCommon.SetNewMarkVisible(item,RoleLogicData.Self.vipLevel >= tmpLv);
                //end
                NiceData tmpPreviewBtnData = GameCommon.GetButtonData(item, "but_shop_preview");
                if (tmpPreviewBtnData != null)
                    tmpPreviewBtnData.set("VIP_LEVEL", tmpLv);

                NiceData tmpBuyBtnData = GameCommon.GetButtonData(item, "but_shop_buy_vip");
                if (tmpBuyBtnData != null)
                {
                    tmpBuyBtnData.set("VIP_LEVEL_INDEX", tmpIndex);
                    tmpBuyBtnData.set("VIP_LEVEL", tmpLv);
                    tmpBuyBtnData.set("LEFT_BUY_NUM", 1);
                    tmpBuyBtnData.set("ITEM_ID_TEST", (int)tmpConfig["INDEX"]);
                    tmpBuyBtnData.set("GIFT_OPEN_LEVEL", (int)tmpConfig["VIP_LEVEL_DISPLAY"]);
                    tmpBuyBtnData.set("COST_TYPE", (int)tmpConfig.getObject("COST_TYPE_1"));
                    tmpBuyBtnData.set("COST_NUM", (int)tmpConfig["COST_NUM_1"]);
                    tmpBuyBtnData.set("SHOP_TYPE", 2);
                }
                else
                    DEBUG.LogError("No add button event");
            }
            //added by xuke
            if (CommonParam.mOpenDynamicUI)
            {
                CheckPlayGridTween("grid_tween_root_3");
            }
            else 
            {
                GlobalModule.DoOnNextUpdate(() =>
                {
                    GlobalModule.DoOnNextLateUpdate(() =>
                    {
                        GlobalModule.DoLater(() =>
                        {
                            GameObject tmpGO = GameCommon.FindObject(mGameObjUI, "scrollview_context_3");
                            if (tmpGO != null)
                            {
                                UIScrollView tmpSV = GameCommon.FindComponent<UIScrollView>(tmpGO, "scrollview");
                                if (tmpSV != null)
                                    tmpSV.RestrictWithinBounds(false);
                            }
                        }, 0.1f);
                    });
                });
            }
            //end          
        }
    }

    private void __ItemCardDataChanged(ShopChouKaData itemData)
    {
        SC_ResponseLotteryQuery tmpWinParams = getObject("LAST_LOTTERY_QUERY") as SC_ResponseLotteryQuery;
        
    }

    /// <summary>
    /// 道具商店数据改变
    /// </summary>
    /// <param name="items"></param>
    private void __ItemShopDataChanged(ShopItemData itemData)
    {
        NewShopWindowParams tmpWinParams = getObject("LAST_ITEMSHOP_DATA") as NewShopWindowParams;
        SC_ResponsePropShopQuery tmpResp = tmpWinParams.obj as SC_ResponsePropShopQuery;

        bool hasChanged = false;
        for (int i = 0, count = tmpResp.prop.Count; i < count; i++)
        {
            if (tmpResp.prop[i].index == itemData.index)
            {
                hasChanged = true;
                tmpResp.prop[i].buyNum += itemData.buyCount;
                break;
            }
        }
        if (!hasChanged)
        {
            hasChanged = true;
            ShopPropData tmpProp = new ShopPropData() { index = itemData.index, buyNum = itemData.buyCount };
            tmpResp.prop.Add(tmpProp);
        }
        if (hasChanged)
            Refresh(tmpWinParams);
    }
    /// <summary>
    /// VIP商店数据改变
    /// </summary>
    /// <param name="items"></param>
    private void __VIPShopDataChanged(ShopItemData itemData)
    {
        NewShopWindowParams tmpWinParams = getObject("LAST_VIPSHOP_DATA") as NewShopWindowParams;
        SC_VIPShopQuery tmpResp = tmpWinParams.obj as SC_VIPShopQuery;

        bool hasChanged = false;
        for (int i = 0, count = tmpResp.buyInfo.Count; i < count; i++)
        {
            if (tmpResp.buyInfo[i].index == itemData.index)
            {
                hasChanged = true;
                tmpResp.buyInfo[i].buyNum += itemData.buyCount;
                break;
            }
        }
        if (!hasChanged)
        {
            hasChanged = true;
            ShopPropData tmpProp = new ShopPropData() { index = itemData.index, buyNum = itemData.buyCount };
            tmpResp.buyInfo.Add(tmpProp);
        }
        if (hasChanged)
            Refresh(tmpWinParams);
        
        //added by xuke 如果礼包全部买光了则去取消红点显示
        if (tmpResp.buyInfo.Count == RoleLogicData.Self.vipLevel + 1) 
        {
            ShopNewMarkManager.Self.CheckSalePackage = false;
        }
        //end
    }

    //抽卡动画模块开始
    public virtual void ShowGainItemsUI(ShopChouKaData shopData)
    {
        GameObject obj = GameObject.Find("create_scene");
        if (obj == null)
            return;
        bool bIsPet = false;
        for (int i = 0, count = shopData.items.Length; i < count; i++)
        {
            if (PackageManager.GetItemTypeByTableID(shopData.items[i].tid) == ITEM_TYPE.PET)
                bIsPet = true;
        }

        UiHidden(false);
        ShowMainScene(!bIsPet);
        SetVisible("black_background", !bIsPet);
        if (shopData.items.Length == 1)
        {
            ShowGainSingleItemUI(shopData);
            if (bIsPet)
            {
                ShowGainItemAnimator(() => RoleAnimatorEnd(shopData), shopData, 90012, 1.5f,mIsJumpFuAnim);
                ShowShopGainPetWindow(false);
            }
        }
        else
        {
            if (bIsPet)
            {
                ShowGainItemAnimator(() => RoleAnimatorEnd(shopData), shopData, 90012, 1.5f, mIsJumpFuAnim);
                ShowGainGroupAllItemsUI(shopData, false);
            }
            else
                ShowGainGroupAllItemsUI(shopData, true);
        }
    }

    public void UiHidden(bool isHidden)
    {
        SetVisible(isHidden, "center_background", "tab_button_panel", "tab_context_panel");
        SetVisible(!isHidden, "black_background");

        if (isHidden)
        {
            DataCenter.OpenWindow("INFO_GROUP_WINDOW");
            DataCenter.OpenWindow("BACK_GROUP_SHOP_WINDOW");
        }
        else
        {
            DataCenter.CloseWindow("INFO_GROUP_WINDOW");
            DataCenter.CloseWindow("BACK_GROUP_SHOP_WINDOW");
        }
    }

    AudioSource maudiosource = null;
    public void ShowGainItemAnimator(Action callBack, ShopChouKaData shopData, int modelIndex, float fScale,bool kIsJumpFuAnim = false)
    {

        bool bIsRole = modelIndex == 90012 ? true : false;
        GameObject obj = GameObject.Find("create_scene");
        if (obj == null)
            return;

        //if (maudiosource.isPlaying)
        //    maudiosource.Stop();

    //    maudiosource.Play();
        if (kIsJumpFuAnim && bIsRole) 
        {
            GameCommon.SetUIVisiable(obj, "item", true);
            GameCommon.SetUIVisiable(obj, "effect_background", false);
            if (callBack != null) 
            {
                callBack.Invoke();
            }
            return;
        }
        float fAniSpeed = 1.0f;
        fAniSpeed = bIsRole ? 1.0f : 1.5f;
        string strPoint = "";
        strPoint = bIsRole ? "role_point" : "fu_point";

        GameCommon.SetUIVisiable(obj, "item", !bIsRole);
        GameCommon.SetUIVisiable(obj, "effect_background", bIsRole);

        ActiveObject mChouKaAnimatorObj = new ActiveObject();
        mChouKaAnimatorObj.mMainObject = new GameObject("_Role_");
        mChouKaAnimatorObj.CreateMainObject(modelIndex);
        GameCommon.SetLayer(mChouKaAnimatorObj.mMainObject, CommonParam.PlayerLayer);
        mChouKaAnimatorObj.mMainObject.transform.parent = GameCommon.FindObject(obj, strPoint).transform;
        mChouKaAnimatorObj.mMainObject.transform.localScale *= fScale;
        mChouKaAnimatorObj.SetVisible(true);
        mChouKaAnimatorObj.mMainObject.transform.localPosition = Vector3.zero;
        mChouKaAnimatorObj.mMainObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

        GameObject tmpGOFuEffect = null;
        if (bIsRole)
        {
            GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UI/ec_ui_futuowei", GameCommon.FindObject(mChouKaAnimatorObj.mMainObject, "Bone056"));
        }
        else
        {
            GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UI/ec_ui_fufly2", GameCommon.FindObject(mChouKaAnimatorObj.mMainObject, "Bone056"));
            if (shopData.items.Length > 0)
            {
                int petID = shopData.items[0].tid;
                tmpGOFuEffect = GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UI/ec_ui_fubagua", GameCommon.FindObject(obj, "effect_bagua"));
            }
        }

        mChouKaAnimatorObj.PlayMotion("chouka", null);
        mChouKaAnimatorObj.SetAnimSpeed(fAniSpeed);
        ChouKaAnimitorEnd mAniEvt = EventCenter.Start("ChouKaAnimitorEnd") as ChouKaAnimitorEnd;
        mAniEvt.set("CHOU_KA_OBJ", mChouKaAnimatorObj);
        mAniEvt.set("FU_EFFECT_OBJ", tmpGOFuEffect);
        mAniEvt.set("GAIN_ITEM_DATA", shopData);
        mAniEvt.set("CHOU_KA_AUDIO", maudiosource);
        mAniEvt.mAction = callBack;
        //		mAniEvt.set ("WINDOW_NAME", "SHOP_WINDOW");
        mAniEvt.DoEvent();

        SetVisible("jump_animator_button", true);
        GameCommon.GetButtonData(mGameObjUI, "jump_animator_button").set("ANI_EVT", mAniEvt);
        GameCommon.GetButtonData(mGameObjUI, "jump_animator_button").set("WINDOW_NAME", "SHOP_WINDOW");
    }

    private bool mIsJumpFuAnim = true;  //> 是否跳过角色画符动画
    void RoleAnimatorEnd(ShopChouKaData shopData)
    {
        ShowGainItemAnimator(() => FuAnimatorEnd(shopData), shopData, 90013, 1f);
    }

    void FuAnimatorEnd(ShopChouKaData shopData)
    {
        SetVisible("jump_animator_button", false);

        if (shopData.items.Length == 1)
        {
            int _petTid = shopData.items[0].tid;
            if (PetGainWindow.CheckPetGainQuality(_petTid))
            {
                ShowShopGainPetWindow(true);
                DataCenter.OpenWindow("PET_GAIN_WINDOW", _petTid);
            }
            else 
            {
                GameObject _createScene = GameObject.Find ("create_scene");
                GameObject _lowEffect = GameCommon.FindObject(_createScene, "new_ui_choukalow");
                if (_lowEffect != null) 
                {
                    if (!_lowEffect.activeSelf)
                        _lowEffect.SetActive(true);
                    ParticleSystem _ps = GameCommon.FindComponent<ParticleSystem>(_lowEffect, "mofazheng");
                    if (_ps != null) 
                    {
                        _ps.Play(true);
                        if (Settings.IsSoundEffectEnabled())
                        {
                            DataRecord record = DataCenter.mEffectSound.GetRecord("new_ui_choukalow");
                            if (record != null)
                            {
                                string filePath = record.get("SOUND_FILE");
                                int fileLevel = record.get("SOUND_TYPE");
                                GameCommon.PlaySound(filePath, GameCommon.GetMainCamera().transform.position, fileLevel);
                            }
                        }
                    }
                }
                GlobalModule.DoLater(() => { ShowShopGainPetWindow(true); },0.2f);
            }
        }
        else
            ShowGainGroupAllItemsUI(shopData, true);
    }

    void ShowMainScene(bool bVisible)
    {
        GameObject obj = GameObject.Find("Mainmenu_bg");
        if (obj != null)
        {
            RenderSettings.fogColor = bVisible ? new Color(65f / 255f, 251f / 255f, 213f / 255f) : new Color(76f / 255f, 141f / 255f, 195f / 255f);

            foreach (Transform t in obj.transform)
                t.gameObject.SetActive(bVisible);
        }
    }

    int mPetStar = 0;
    void SetPetStar(int petIndex)
    {
        mPetStar = TableCommon.GetNumberFromActiveCongfig(petIndex, "STAR_LEVEL");
    }

//     public void SetShopPetBuyAgainButton(ShopChouKaData shopData)
//     {
//         shop_gain_pet_window shopGainPetWindow = DataCenter.GetData("shop_gain_pet_window") as shop_gain_pet_window;
//         if (shopGainPetWindow != null)
//             SetShopBuyAgainButton(shopGainPetWindow.mGameObjUI, shopData);
//     }
// 
//     public void SetShopItemBuyAgainButton(ShopChouKaData shopData)
//     {
//         shop_gain_item_window shopGainItemWindow = DataCenter.GetData("shop_gain_item_window") as shop_gain_item_window;
//         if (shopGainItemWindow != null)
//             SetShopBuyAgainButton(shopGainItemWindow.mGameObjUI, shopData);
//     }
// 
//     public void SetShopBuyAgainButton(GameObject obj, ShopChouKaData shopData)
//     {
//         if (obj != null)
//         {
//             int costCount = shopData.mCostCount;
//             bool isDiscount = false;
//             GameCommon.SetUIVisiable(obj, "discount_effect", isDiscount);
// 
//             UIButtonEvent buttonData = obj.transform.Find("but_shop_buy_again").gameObject.GetComponent<UIButtonEvent>();
// //             buttonData.mData.set("GRID_INDEX", gridIndex);
//             buttonData.mData.set("SHOP_SLOT_INDEX", shopSlotIndex);
//            buttonData.mData.set("IS_DISCOUNT", isDiscount);
// 
//             // number
//             UILabel iconLabel = obj.transform.Find("but_shop_buy_again/icon_label").gameObject.GetComponent<UILabel>();
//             iconLabel.text = costCount.ToString();
// 
//             // icon
//             int iItemType = (int)PackageManager.GetItemTypeByTableID(shopData.mCostTid);
//             DataRecord itemDataRecord = DataCenter.mItemIcon.GetRecord(iItemType);
//             if (itemDataRecord != null)
//             {
//                 UISprite iconSprite = obj.transform.Find("but_shop_buy_again/icon_sprite").gameObject.GetComponent<UISprite>();
//                 string strAtlasName = itemDataRecord.get("ITEM_ATLAS_NAME").ToString();
//                 string strSpriteName = itemDataRecord.get("ITEM_SPRITE_NAME").ToString();
//                 GameCommon.SetIcon(iconSprite, strAtlasName, strSpriteName);
//             }
//             else
//             {
//                 DEBUG.LogError("dataRecord is null");
//             }
//         }
//     }

    public virtual void ShowGainSingleItemUI(ShopChouKaData shopData)
    {
        for (int i = 0, count = shopData.items.Length; i < count; i++)
        {
            int iItemType = (int)PackageManager.GetItemTypeByTableID(shopData.items[i].tid);
            int iItemID = shopData.items[i].tid;
            if (iItemType == (int)ITEM_TYPE.PET)
            {
//                SetShopPetBuyAgainButton(shopData);

                SetPetStar(iItemID);
                DataCenter.SetData("shop_gain_pet_window", "OPEN", true);
                DataCenter.SetData("shop_gain_pet_window", "SET_SELECT_PET_BY_MODEL_INDEX", iItemID);
                DataCenter.SetData("shop_gain_pet_window", "REFRESH_BUY_AGAIN_INFO", shopData);

                return;
            }

//            SetShopItemBuyAgainButton(shopData);

            DataCenter.SetData("shop_gain_item_window", "OPEN", true);
            DataCenter.SetData("shop_gain_item_window", "SET_SELECT_ITEM_BY_MODEL_INDEX", shopData.items[0]);
            DataCenter.SetData("shop_gain_item_window", "REFRESH_BUY_AGAIN_INFO", shopData);
        }
    }

    void ShowShopGainPetWindow(bool bVisible)
    {
        shop_gain_pet_window tmpWin = DataCenter.GetData("shop_gain_pet_window") as shop_gain_pet_window;
        GameObject shopGainPetWindowObj = tmpWin.mGameObjUI;
        shopGainPetWindowObj.SetActive(bVisible);

        if (!bVisible)
            return;

		//杨志恒让改为 四星及以上才显示“background_effect” 其余特效不要
//        GameCommon.SetUIVisiable(shopGainPetWindowObj, "moreThanThreeStarLevelEffect", mPetStar > 3);
        //GameCommon.SetUIVisiable(shopGainPetWindowObj, "background_effect", mPetStar > 3);
//        ShowStar();

        //PlayTweenScale(GameCommon.FindObject(shopGainPetWindowObj, "gain_pet_window_card"));

        PlayTweenScale(GameCommon.FindObject(shopGainPetWindowObj, "but_shop_buy_again"));
        PlayTweenScale(GameCommon.FindObject(shopGainPetWindowObj, "shop_gain_pet_window_close_btn"));
        PlayTweenScale(GameCommon.FindObject(shopGainPetWindowObj, "shop_gain_pet_window_details_btn"));

//        GuideManager.Notify(GuideIndex.GetFirstFreePetEnd);
    }

    void ShowStar()
    {
        if (mPetStar == 0)
            return;

        shop_gain_pet_window tmpGainPetWin = DataCenter.GetData("shop_gain_pet_window") as shop_gain_pet_window;
        GameObject tmpParent = tmpGainPetWin.mGameObjUI;

        for (int i = 1; i < 6; i++)
        {
            GameCommon.SetUIVisiable(tmpParent, "star_" + i.ToString(), false);

            if (i > 2)
                GameCommon.SetUIVisiable(tmpParent, "effect_font_star_" + i.ToString(), false);
        }

        GameCommon.SetUIVisiable(tmpParent, "star_" + mPetStar.ToString(), true);
        GameObject starInfo = GameCommon.FindObject(tmpParent, "star_" + mPetStar.ToString());
        for (int i = 1; i < mPetStar + 1; i++)
        {
            GameCommon.SetUIVisiable(starInfo, "star" + i.ToString(), false);
        }

        this.StartCoroutine(ShowStar(starInfo));
    }

    IEnumerator ShowStar(GameObject parentObj)
    {
        for (int i = 1; i < mPetStar + 1; i++)
        {
            yield return new WaitForSeconds(0.3f);
            GameCommon.SetUIVisiable(parentObj, "star" + i.ToString(), true);
        }

        if (mPetStar > 2)
        {
            shop_gain_pet_window tmpGainPetWin = DataCenter.GetData("shop_gain_pet_window") as shop_gain_pet_window;
            GameObject tmpParent = tmpGainPetWin.mGameObjUI;
            GameCommon.SetUIVisiable(tmpParent, "effect_font_star_" + mPetStar.ToString(), true);
        }
    }

    public void PlayTweenScale(GameObject obj)
    {
        TweenScale[] tweens;
        tweens = obj.GetComponents<TweenScale>();
        foreach (TweenScale t in tweens)
        {
            t.ResetToBeginning();
            t.PlayForward();
        }
    }

    public virtual void ShowGainGroupAllItemsUI(ShopChouKaData shopData, bool bVisible)
    {
//        SetShopItemBuyAgainButton(shopData);
        DataCenter.SetData("shop_gain_item_window", "OPEN", true);
        shop_gain_item_window tmpWin = DataCenter.GetData("shop_gain_item_window") as shop_gain_item_window;
        if (tmpWin == null)
            return;
        GameObject shopGainPetsWindowObj = tmpWin.mGameObjUI;
        if (shopGainPetsWindowObj == null)
            return;
        shopGainPetsWindowObj.SetActive(bVisible);

        if (!bVisible)
            return;
        DataCenter.SetData("shop_gain_item_window", "SET_SELECT_ITEMS_BY_MODEL_INDEX", shopData);
        DataCenter.SetData("shop_gain_item_window", "REFRESH_BUY_AGAIN_INFO", shopData);
    }
    //抽卡模块结束

    //end

	public override void onChange(string keyIndex, object objVal) {
		base.onChange(keyIndex, objVal);

		switch(keyIndex) {
			// 刷新用户数据
		case "UPDATA_HEAD_INFO":
               Open((int)SHOP_PAGE_TYPE.PET);
			break;
        //by chenliang
        //begin

		case "OPEN_SHOP_WINDOW":
            Open(objVal != null ? ((SHOP_PAGE_TYPE)objVal) : SHOP_PAGE_TYPE.PET);
			__EnableToggle(objVal != null ? ((SHOP_PAGE_TYPE)objVal) : SHOP_PAGE_TYPE.PET);
			break;
        case "GO_TO_VIP":
            {
                int tmpVIPLevel = (int)objVal;
            } break;
        case "ITEM_SHOP_ITEM_CHANGED": __ItemShopDataChanged(objVal as ShopItemData); break;
        case "VIP_SHOP_ITEM_CHANGED": __VIPShopDataChanged(objVal as ShopItemData); break;

        case "FU_ANIMATOR_END":
            FuAnimatorEnd((ShopChouKaData)objVal);
            break;
        case "UI_HIDDEN":
			if(mGameObjUI == null)
				return;
            UiHidden((bool)objVal);
            ShowMainScene(true);
            break;
		case "SET_SEL_PAGE":
			__EnableToggle(objVal != null ? ((SHOP_PAGE_TYPE)objVal) : SHOP_PAGE_TYPE.PET); 
			break;

        //end
		}

	}

	void RequestLotteryQuerySuccess(string text, bool playWinAnim = true) {
		SC_ResponseLotteryQuery reslq = JCode.Decode<SC_ResponseLotteryQuery>(text);

		NewShopWindowParams param = new NewShopWindowParams();
		param.pageIndex = 0;
		param.obj = reslq;
        param.mIsPlayWinAnim = playWinAnim;

		DataCenter.SetData("SHOP_WINDOW", "REFRESH", param);

        //by chenliang
        //begin

//		GetComponent<TweenPosition>("npc").enabled = true;
//---------------
        //改在Open处调用

        //end
		DEBUG.Log("LotteryQuerySuccess:" + text);
	}
	
	void RequestLotteryQueryFail(string text) {

		DEBUG.Log("LotteryQueryErr:" + text);
	}
}
//by chenliang
//begin

/// <summary>
/// 购买VIP礼包
/// </summary>
class Button_but_shop_buy_vip : CEvent
{
    public override bool _DoEvent()
    {
        int tmpLv = (int)getObject("VIP_LEVEL");
        if (!VIPHelper.IsReachStandard(tmpLv))
        {
            //DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_SHOP_VIP_LEVEL_LOW);
            DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_SHOP_VIP_LEVEL_LOW, () =>
            {
                GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, () =>
                {
                });
            });
            return true;
        }

        int tmpCostId = (int)getObject("COST_TYPE");
        int tmpCostNum = (int)getObject("COST_NUM");

        int tmpCurrNum = PackageManager.GetItemLeftCount(tmpCostId);
        if (tmpCurrNum < tmpCostNum)
        {
			GameCommon.ToGetDiamond();
            return true;
        }
        DataCenter.OnlyTipsLabelMessage(STRING_INDEX.ERROR_SHOP_BUY_SUCCESS);
        GlobalModule.DoCoroutine(__DoAction());

        return true;
    }

    private IEnumerator __DoAction()
    {
        int tmpIndex = (int)getObject("VIP_LEVEL_INDEX");
        VIP_VIPShop_Requester tmpRequester = new VIP_VIPShop_Requester();
        tmpRequester.Index = tmpIndex;
        yield return tmpRequester.Start();

        SC_VIPShop tmpResp = tmpRequester.respMsg;
        if (tmpResp.ret == 1)
        {
            //减去消耗
            int tmpCostId = (int)getObject("COST_TYPE");
            int tmpCostNum = (int)getObject("COST_NUM");
            PackageManager.RemoveItem(new ItemDataBase() { tid = tmpCostId, itemId = -1, itemNum = tmpCostNum });

            //增加物品
            if (tmpResp.isBuySuccess == 1)
            {
                for (int i = 0, count = tmpResp.buyItem.Length; i < count; i++)
                    PackageManager.UpdateItem(tmpResp.buyItem[i]);
            }

            //隐藏相应VIP选项
            ItemDataBase[] tmpItems = new ItemDataBase[tmpResp.buyItem.Length];
            for (int i = 0, count = tmpResp.buyItem.Length; i < count; i++)
                tmpItems[i] = new ItemDataBase() { tid = tmpResp.buyItem[i].tid, itemId = tmpResp.buyItem[i].itemNum, itemNum = 1 };
            DataCenter.SetData("SHOP_WINDOW", "VIP_SHOP_ITEM_CHANGED", new ShopItemData() { index = (int)getObject("ITEM_ID_TEST"), buyCount = 1, items = tmpItems });
        }
    }
}

/// <summary>
/// 抽卡界面再次购买
/// </summary>
public class Button_Chouka_Buy_Again : CEvent
{
    public override bool _DoEvent()
    {
//         string tmpWinName = getObject("WINDOW_NAME").ToString();
// 
//         DataCenter.SetData(tmpWinName, "CLOSE", true);
// 
//         DataCenter.SetData("SHOP_WINDOW", "UI_HIDDEN", true);

        ShopChouKaData tmpShopData = (ShopChouKaData)getObject("SHOP_DATA");
        NewShopWindow tmpWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
        Action tmpAction = null;
        switch (tmpShopData.mBuyType)
        {
            case 0: tmpAction = tmpWin.ForceClickNormalOne; break;
            case 1: tmpAction = tmpWin.ForceClickNormalTen; break;
            case 2: tmpAction = tmpWin.ForceClickPreciousOne; break;
            case 3: tmpAction = tmpWin.ForceClickPreciousTen; break;
        }
        if (tmpAction != null)
            tmpAction();

        return true;
    }
}

/// <summary>
/// 抽卡需要传入的数据
/// </summary>
public class ShopChouKaData
{
    public int mBuyType;    //抽卡类型，0：普通抽卡，1：普通十连抽，2：高级抽卡，3：高级十连抽
    public int mCostTid;    //花费Tid
    public int mCostCount;  //花费个数
    public ItemDataBase[] items;
}

class ShopItemData
{
    public int index;
    public int buyCount;            //购买个数
    public ItemDataBase[] items;
}

//end

public class Button_shop_buy_common_one : CEvent
{
	int useWhat = 0;
	SC_ResponseLotteryQuery srlq;
	public override bool _DoEvent()
	{
        //by chenliang
        //begin

// 		srlq = get("NET_DATA").mObj as SC_ResponseLotteryQuery;
// 		ConsumeItemData ycid = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.SILVER_FL_COMMAND);
// 		if(srlq.normalLottery.isFree == (int)STRING_INDEX.ERROR_NONE && srlq.normalLottery.leftNum > 0) {
// 			useWhat = -1;
// 		} else if (ycid != null && ycid.itemNum > 0) {
// 			useWhat = (int)ITEM_TYPE.SILVER_FL_COMMAND;
// 		} else {
// 			DataCenter.OpenMessageWindow("用户银符令不足。");
// 			return true;
// 		}
// 
// 		ItemDataBase idb = new ItemDataBase();
// 		idb.tid = useWhat;
// 		idb.itemId = ConsumeItemLogicData.Self.GetItemIdByTid(useWhat);
// 		idb.itemNum = useWhat == -1 ? -1 : 1;
//        ShopNetEvent.RequestNormalLottery(idb, RequestNormalLotterySuccess, RequestNormalLotteryFail);
//-------------------
        bool tmpIsForceClick;
        object tmpObjIsForceClick = getObject("FORCE_CLICK");
        if (tmpObjIsForceClick == null)
            tmpIsForceClick = false;
        else
            tmpIsForceClick = (bool)tmpObjIsForceClick;
        Action tmpAction = () =>
        {
            //判断背包是否已满
            if (!CheckPackage.Instance.CanAddItems(new PACKAGE_TYPE[] { PACKAGE_TYPE.PET }))
            {
                return;
            }

            int tmpFreeCount = (int)getObject("FREE_COUNT");
            ItemDataBase idb = getObject("USE_ITEM_DATA") as ItemDataBase;
            int tmpLeftCount = PackageManager.GetItemLeftCount(idb.tid);
            if (tmpFreeCount <= 0 && idb.itemNum > tmpLeftCount)
            {
                string tmpDesc = GameCommon.GetItemName(idb.tid);
                DataCenter.ErrorTipsLabelMessage(tmpDesc + "不足");
                return;
            }

            ShopNetEvent.RequestNormalLottery(idb, RequestNormalLotterySuccess, RequestNormalLotteryFail);
        };
        if (!tmpIsForceClick)
        {
            //检测抽卡场景是否加载完成
            NewShopWindow tmpShopWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
            if (tmpShopWin == null || !tmpShopWin.IsLoadChouKaSuccess() || tmpShopWin.GetLoadChouKaSceneComplete() != null)
            {
                if (tmpShopWin != null)
                    tmpShopWin.SetLoadChouKaSceneComplete(tmpAction);
                return false;
            }
        }
        tmpAction();

        //end

		return true;
	}

	void RequestNormalLotterySuccess(string text) {
		SC_ResponseNormalLottery nl = JCode.Decode<SC_ResponseNormalLottery>(text);

		if(nl.ret == (int)STRING_INDEX.ERROR_NONE) {
            //by chenliang
            //begin

// 			DeliverParams dp = new DeliverParams();
// 			ItemDataBase getFL = new ItemDataBase();
// 			getFL = nl.reward;
// 			dp.index = 1;
// 			dp.param = getFL;
// 			// 弹出奖励物品
// 			DataCenter.OpenWindow("GET_REWARDS_WINDOW", dp);
// 			// 减少银符令物品
// 			ItemDataBase idb = new ItemDataBase();
// 			idb.itemNum = 1;
// 			idb.tid = (int)ITEM_TYPE.SILVER_FL_COMMAND;
// 			ConsumeItemData ycid = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.SILVER_FL_COMMAND);
// 			if(ycid.itemNum > 0 && useWhat != -1) 
// 				PackageManager.RemoveItem(idb);
// 			// 将符令添加到背包
// 			PackageManager.AddItem(getFL);
//
//          //更新ui数据
//          DataCenter.SetData("SHOP_WINDOW", "UPDATA_HEAD_INFO", "");
//-------------------------
            //减去消耗
            ItemDataBase tmpItem = getObject("USE_ITEM_DATA") as ItemDataBase;
            if (tmpItem.itemNum > 0)
                PackageManager.RemoveItem(tmpItem);

            DeliverParams dp = new DeliverParams();
			ItemDataBase getFL = new ItemDataBase();
			getFL = nl.reward;
			dp.index = 1;
			dp.param = getFL;
			// 弹出奖励物品
//			DataCenter.OpenWindow("GET_REWARDS_WINDOW", dp);
            //增加物品
            PackageManager.UpdateItem(getFL);
            //增加银币
            RoleLogicData.Self.AddGold(nl.freeGold);
            
            //更新ui数据
            DataCenter.SetData("SHOP_WINDOW", "UPDATA_HEAD_INFO", "");

            //抽卡动画
            NewShopWindow tmpWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
            tmpWin.set("ANIMATION_CALLBACK", new CallBack(this, "OnCallback", dp));
            ItemDataBase tmpCurrCostData = tmpWin.GetCurentCostData(NewShopWindow.SHOP_PUMPING_TYPE.NORMAL_ONE);
            ShopChouKaData tmpShopData = new ShopChouKaData();
            tmpShopData.mBuyType = 0;
            tmpShopData.mCostTid = tmpCurrCostData.tid;
            tmpShopData.mCostCount = tmpCurrCostData.itemNum;
            tmpShopData.items = new ItemDataBase[] { getFL };
            tmpWin.ShowGainItemsUI(tmpShopData);

            //end

			//Test
			//DataCenter.SetData("shop_gain_item_window", "REFRESH_BUY_AGAIN_INFO",tmpShopData);
            //added by xuke 刷新红点逻辑
            ShopNewMarkManager.Self.CheckSilverFL = PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND) > 0;
            ShopNewMarkManager.Self.CheckTenSilverFL = PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND) >= 10;
            ShopNewMarkManager.Self.CheckNormalFreeDraw = false;
            //end
		}

		DEBUG.Log("NormalLotterySuccess: " + text);
	}
    //by chenliang
    //begin

    public void OnCallback(object param)
    {
        DataCenter.OpenWindow("GET_REWARDS_WINDOW", param);
    }

    //end

	void RequestNormalLotteryFail(string text) {

		DEBUG.Log("NormalLotteryErr: " + text);
	}
}

public class Button_but_shop_buy_common_ten : CEvent {

	public override bool _DoEvent ()
	{
        //by chenliang
        //begin

// 		// 低十连没免费
// 		ConsumeItemData ycid = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.SILVER_FL_COMMAND);
// 		if(ycid == null || ycid.itemNum < 10) {
// 			DataCenter.OpenMessageWindow("用户银符令不足。");
// 			return true;
// 		}
// 
// 		ItemDataBase idb = new ItemDataBase();
// 		idb.tid = (int)ITEM_TYPE.SILVER_FL_COMMAND;
// 		idb.itemId = ConsumeItemLogicData.Self.GetItemIdByTid((int)ITEM_TYPE.SILVER_FL_COMMAND);
// 		idb.itemNum = 10;
//        ShopNetEvent.RequestTenNormalLottery(idb, RequestTenNormalLotterySuccess, RequestTenNormalLotteryFail);
//-------------------
        bool tmpIsForceClick;
        object tmpObjIsForceClick = getObject("FORCE_CLICK");
        if (tmpObjIsForceClick == null)
            tmpIsForceClick = false;
        else
            tmpIsForceClick = (bool)tmpObjIsForceClick;
        Action tmpAction = () =>
        {
            //判断背包是否已满
            if (!CheckPackage.Instance.CanAddItems(new PACKAGE_TYPE[] { PACKAGE_TYPE.PET }))
            {
                return;
            }

            ItemDataBase idb = getObject("USE_ITEM_DATA") as ItemDataBase;
            int tmpLeftCount = PackageManager.GetItemLeftCount(idb.tid);
            if (idb.itemNum > tmpLeftCount)
            {
                string tmpDesc = GameCommon.GetItemName(idb.tid);
                if (idb.tid == (int)ITEM_TYPE.GOLD)
                {
                    DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
                }
                else
                {
                    DataCenter.ErrorTipsLabelMessage(tmpDesc + "不足");
                }
                return;
            }
            ShopNetEvent.RequestTenNormalLottery(idb, RequestTenNormalLotterySuccess, RequestTenNormalLotteryFail);
        };
        if (!tmpIsForceClick)
        {
            //检测抽卡场景是否加载完成
            NewShopWindow tmpShopWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
            if (tmpShopWin == null || !tmpShopWin.IsLoadChouKaSuccess() || tmpShopWin.GetLoadChouKaSceneComplete() != null)
            {
                if (tmpShopWin != null)
                    tmpShopWin.SetLoadChouKaSceneComplete(tmpAction);
                return false;
            }
        }
        tmpAction();

        //end
		
		return true;
	}

	void RequestTenNormalLotterySuccess(string text) {
		SC_ResponseTenNormalLottery nl = JCode.Decode<SC_ResponseTenNormalLottery>(text);

		if(nl.ret == (int)STRING_INDEX.ERROR_NONE) {
            //by chenliang
            //begin

// 			ItemDataBase [] idb = nl.reward;
// 			DeliverParams dp = new DeliverParams();
// 			dp.index = 1;
// 			dp.param = idb;
// 			// 弹出奖励物品
// 			DataCenter.OpenWindow("GET_REWARDS_WINDOW", dp);
// 			// 减少符令数x10
// 			ItemDataBase idbf = new ItemDataBase();
// 			idbf.itemNum = 10;
// 			idbf.tid = (int)ITEM_TYPE.SILVER_FL_COMMAND;
// 			// 减少符令
// 			PackageManager.RemoveItem(idbf);
// 			// 添加符灵x10
// 			PackageManager.AddItem(idb);
//----------------------
            //减去消耗
            ItemDataBase tmpItem = getObject("USE_ITEM_DATA") as ItemDataBase;
            if (tmpItem.itemNum > 0)
                PackageManager.RemoveItem(tmpItem);

			ItemDataBase [] idb = nl.reward;
			DeliverParams dp = new DeliverParams();
			dp.index = 1;
			dp.param = idb;
			// 弹出奖励物品
//			DataCenter.OpenWindow("GET_REWARDS_WINDOW", dp);
            //增加物品
            PackageManager.UpdateItem(idb);
            //增加银币
            RoleLogicData.Self.AddGold(nl.freeGold);

            DataCenter.SetData("SHOP_WINDOW", "UPDATA_HEAD_INFO", "");

            //抽卡动画
            NewShopWindow tmpWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
            tmpWin.set("ANIMATION_CALLBACK", new CallBack(this, "OnCallback", dp));
            ItemDataBase tmpCurrCostData = tmpWin.GetCurentCostData(NewShopWindow.SHOP_PUMPING_TYPE.NORMAL_TEN);
            ShopChouKaData tmpShopData = new ShopChouKaData();
            tmpShopData.mBuyType = 1;
            tmpShopData.mCostTid = tmpCurrCostData.tid;
            tmpShopData.mCostCount = tmpCurrCostData.itemNum;
            tmpShopData.items = idb;
            tmpWin.ShowGainItemsUI(tmpShopData);

            //end
            //added by xuke
            ShopNewMarkManager.Self.CheckSilverFL = PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND) > 0;
            ShopNewMarkManager.Self.CheckTenSilverFL = PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND) >= 10;
            //end
		}

		//DataCenter.SetData("SHOP_WINDOW", "UPDATA_HEAD_INFO", "");
	}
    //by chenliang
    //begin

    public void OnCallback(object param)
    {
        DataCenter.OpenWindow("GET_REWARDS_WINDOW", param);
    }

    //end

	void RequestTenNormalLotteryFail(string text) {

		DEBUG.Log("RequestTenNormalLotteryErr: " + text);
	}

}

public class Button_but_shop_buy_advance_one : CEvent {
	SC_ResponseLotteryQuery srlq;
	int useWhat = 0;
	public override bool _DoEvent() {
        //by chenliang
        //begin

// 		srlq = get("NET_DATA").mObj as SC_ResponseLotteryQuery;
// 		ConsumeItemData ycid = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.GOLD_FL_COMMAND);
// 		if(srlq.preciousLottery.isFree == (int)STRING_INDEX.ERROR_NONE && srlq.preciousLottery.leftNum > 0) {
// 			useWhat = -1;
// 		} else if (ycid != null && ycid.itemNum > 0) {
// 			useWhat = (int)ITEM_TYPE.GOLD_FL_COMMAND;
// 		} else if (RoleLogicData.Self.diamond >= 300){
// 			useWhat = (int)ITEM_TYPE.YUANBAO;
// 		} else {
// 			DataCenter.OpenMessageWindow("用户元宝不足。");
// 			return true;
// 		}
// 
// 		ItemDataBase idb = new ItemDataBase();
// 		idb.tid = useWhat;
// 		idb.itemId = ConsumeItemLogicData.Self.GetItemIdByTid(useWhat);
// 		idb.itemNum = useWhat == -1 ? -1 : 1;
//        ShopNetEvent.RequestPreciousLottery(idb, RequestPreciousLotterySuccess, RequestPreciousLotteryFail);
//-------------------
        bool tmpIsForceClick;
        object tmpObjIsForceClick = getObject("FORCE_CLICK");
        if (tmpObjIsForceClick == null)
            tmpIsForceClick = false;
        else
            tmpIsForceClick = (bool)tmpObjIsForceClick;
        Action tmpAction = () =>
        {
            //判断背包是否已满
            if (!CheckPackage.Instance.CanAddItems(new PACKAGE_TYPE[] { PACKAGE_TYPE.PET }))
            {
                return;
            }

            int tmpFreeCount = (int)getObject("FREE_COUNT");
            ItemDataBase idb = getObject("USE_ITEM_DATA") as ItemDataBase;
            int tmpLeftCount = PackageManager.GetItemLeftCount(idb.tid);

            if (tmpFreeCount <= 0 && idb.itemNum > tmpLeftCount)
            {
                string tmpDesc = GameCommon.GetItemName(idb.tid);
                GameCommon.ToGetDiamond("",
                    () =>
                    {
                        if (GameCommon.bIsWindowOpen("SHOP_WINDOW") && !GameCommon.bIsWindowOpen("shop_gain_pet_window"))
                        {
                            DataCenter.CloseWindow("SHOP_WINDOW");
                        }
                    },
                    () =>
                    {
                        if (!GameCommon.bIsWindowOpen("shop_gain_pet_window"))
                        {
                            DataCenter.OpenWindow("SHOP_WINDOW");
                        }
                        else
                        {
                            DataCenter.SetData("shop_gain_pet_window", "REFRESH_BUY_AGAIN_INFO_COLOR", null);
                        }
                    }
                );
                return;
            }
            ShopNetEvent.RequestPreciousLottery(idb, RequestPreciousLotterySuccess, RequestPreciousLotteryFail);
        };
        if (!tmpIsForceClick)
        {
            //检测抽卡场景是否加载完成
            NewShopWindow tmpShopWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
            if (tmpShopWin == null || !tmpShopWin.IsLoadChouKaSuccess() || tmpShopWin.GetLoadChouKaSceneComplete() != null)
            {
                if (tmpShopWin != null)
                    tmpShopWin.SetLoadChouKaSceneComplete(tmpAction);
                return false;
            }
        }
        tmpAction();
		return true;
	}
	void RequestPreciousLotterySuccess(string text) {
		SC_ResponsePreciousLottery pl = JCode.Decode<SC_ResponsePreciousLottery>(text);

		if(pl.ret == (int)STRING_INDEX.ERROR_NONE) {

            //by chenliang
            //begin

// 			// 减少银符令物品
// 			DeliverParams dp = new DeliverParams();
// 			ItemDataBase getFL = new ItemDataBase();
// 			getFL = pl.reward;
// 			dp.index = 1;
// 			dp.param = getFL;
// 			// 弹出奖励物品
// 			DataCenter.OpenWindow("GET_REWARDS_WINDOW", dp);
// 
// 			// 减少银符令物品
// 			ItemDataBase idb = new ItemDataBase();
// 
// 			// 如果用户有金涪陵，则删除道具
// 			if(useWhat == (int)ITEM_TYPE.GOLD_FL_COMMAND) {
// 				ConsumeItemData ycid = ConsumeItemLogicData.Self.GetDataByTid((int)ITEM_TYPE.SILVER_FL_COMMAND);
// 				if(ycid.itemNum > 0) {
// 					idb.tid = (int)ITEM_TYPE.GOLD_FL_COMMAND;
// 					idb.itemNum = 1;
// 				}
// 			} else if (useWhat == (int)ITEM_TYPE.YUANBAO) {
// 				idb.tid = (int)ITEM_TYPE.YUANBAO;
// 				idb.itemNum = 300;
// 			}
// 
// 			// 如果不是在免费次数内，则减少对应的消耗品
// 			if(useWhat != -1)
// 				PackageManager.RemoveItem(idb);
// 			// 将符令添加到背包
// 			PackageManager.AddItem(getFL);
//
//
//          DataCenter.SetData("SHOP_WINDOW", "UPDATA_HEAD_INFO", "");
//-------------------------
            //减去消耗
            ItemDataBase tmpItem = getObject("USE_ITEM_DATA") as ItemDataBase;
            if (tmpItem.itemNum > 0)
                PackageManager.RemoveItem(tmpItem);

            DeliverParams dp = new DeliverParams();
			ItemDataBase getFL = new ItemDataBase();
			getFL = pl.reward;
			dp.index = 1;
			dp.param = getFL;
			// 弹出奖励物品
//			DataCenter.OpenWindow("GET_REWARDS_WINDOW", dp);
            //增加物品
            PackageManager.UpdateItem(getFL);
            //增加银币
            RoleLogicData.Self.AddGold(pl.freeGold);


            DataCenter.SetData("SHOP_WINDOW", "UPDATA_HEAD_INFO", "");

            //抽卡动画
            NewShopWindow tmpWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
            tmpWin.set("ANIMATION_CALLBACK", new CallBack(this, "OnCallback", dp));
            ItemDataBase tmpCurrCostData = tmpWin.GetCurentCostData(NewShopWindow.SHOP_PUMPING_TYPE.PRECIOUS_ONE);
            ShopChouKaData tmpShopData = new ShopChouKaData();
            tmpShopData.mBuyType = 2;
            tmpShopData.mCostTid = tmpCurrCostData.tid;
            tmpShopData.mCostCount = tmpCurrCostData.itemNum;
            tmpShopData.items = new ItemDataBase[] { getFL };
            tmpWin.ShowGainItemsUI(tmpShopData);

            //added by xuke 刷新红点逻辑
            ShopNewMarkManager.Self.CheckGoldFL = PackageManager.GetItemLeftCount((int)ITEM_TYPE.GOLD_FL_COMMAND) > 0;
            ShopNewMarkManager.Self.CheckAdvanceFreeDraw = false;
            //end

			//Test
			DataCenter.SetData("shop_gain_item_window", "REFRESH_BUY_AGAIN_INFO",tmpShopData);
		}
	}
    //by chenliang
    //begin

    public void OnCallback(object param)
    {
        DataCenter.OpenWindow("GET_REWARDS_WINDOW", param);
    }

    //end

	void RequestPreciousLotteryFail(string text) {

		DEBUG.Log("RequestPreciousLotteryErr:"+text);
	}
}

public class Button_but_shop_buy_advacne_ten : CEvent {
	public override bool _DoEvent() {

        //by chenliang
        //begin

// 		int usrYb = RoleLogicData.Self.diamond;
// 		if(usrYb < 2800) {
// 			DataCenter.OpenMessageWindow("用户元宝不够");
// 			return true;
// 		}
// 
// 
// 		ItemDataBase idb = new ItemDataBase();
// 		idb.tid = (int)ITEM_TYPE.GOLD_FL_COMMAND;
// 		//idb.itemId = ConsumeItemLogicData.Self.GetItemIdByTid((int)ITEM_TYPE.GOLD_FL_COMMAND);
// 		idb.itemNum = 10;
//        ShopNetEvent.RequestTenPreciousLottery(idb, RequestTenPreciousLotterySuccess, RequestTenPreciousLotteryFail);
//------------------------
        bool tmpIsForceClick;
        object tmpObjIsForceClick = getObject("FORCE_CLICK");
        if (tmpObjIsForceClick == null)
            tmpIsForceClick = false;
        else
            tmpIsForceClick = (bool)tmpObjIsForceClick;
        Action tmpAction = () =>
        {
            //判断背包是否已满
            if (!CheckPackage.Instance.CanAddItems(new PACKAGE_TYPE[] { PACKAGE_TYPE.PET }))
            {
                return;
            }

            ItemDataBase idb = getObject("USE_ITEM_DATA") as ItemDataBase;
            Dictionary<NewShopWindow.SHOP_PUMPING_TYPE, ItemDataBase> tmpDicUseItems = GameCommon.GetFinalUseItemsForNewShopWindow();
            ItemDataBase tmpItemData = tmpDicUseItems[NewShopWindow.SHOP_PUMPING_TYPE.PRECIOUS_TEN];
            idb = tmpItemData;

            if (idb.itemNum < 0)
            {
                string tmpDesc = GameCommon.GetItemName(idb.tid);
                GameCommon.ToGetDiamond("",
                    () =>
                    {
                        if (GameCommon.bIsWindowOpen("SHOP_WINDOW") && !GameCommon.bIsWindowOpen("shop_gain_item_window"))
                        {
                            DataCenter.CloseWindow("SHOP_WINDOW");
                        }
                    },
                    () =>
                    {
                        if (!GameCommon.bIsWindowOpen("shop_gain_item_window"))
                        {
                            DataCenter.OpenWindow("SHOP_WINDOW");
                        }
                        else
                        {
                            DataCenter.SetData("shop_gain_item_window", "REFRESH_BUY_AGAIN_INFO_COLOR", null);
                        }
                    }
                );
                return;
            }
            ShopNetEvent.RequestTenPreciousLottery(idb, RequestTenPreciousLotterySuccess, RequestTenPreciousLotteryFail);
        };
        if (!tmpIsForceClick)
        {
            //检测抽卡场景是否加载完成
            NewShopWindow tmpShopWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
            if (tmpShopWin == null || !tmpShopWin.IsLoadChouKaSuccess() || tmpShopWin.GetLoadChouKaSceneComplete() != null)
            {
                if (tmpShopWin != null)
                    tmpShopWin.SetLoadChouKaSceneComplete(tmpAction);
                return false;
            }
        }
        tmpAction();

		return true;
	}
	void RequestTenPreciousLotterySuccess(string text) {
		SC_ResponseTenPreciousLottery pl = JCode.Decode<SC_ResponseTenPreciousLottery>(text);
		
		if(pl.ret == (int)STRING_INDEX.ERROR_NONE) {
			ItemDataBase [] idb = pl.reward;
			DeliverParams dp = new DeliverParams();
			dp.index = 1;
			dp.param = idb;
            //by chenliang
            //begin

//            // 弹出奖励物品
//            DataCenter.OpenWindow("GET_REWARDS_WINDOW", dp);
// 			// 减少2800元宝
// 			ItemDataBase idbc = new ItemDataBase();
// 			idbc.tid = (int)ITEM_TYPE.YUANBAO;
// 			idbc.itemNum = 2800;
// 			PackageManager.RemoveItem(idbc);
// 			// 添加符灵x10
// 			PackageManager.AddItem(idb);
//
//          DataCenter.SetData("SHOP_WINDOW", "UPDATA_HEAD_INFO", "");
//---------------------
            //减去消耗
            ItemDataBase tmpItem = getObject("USE_ITEM_DATA") as ItemDataBase;
            if (tmpItem.itemNum > 0)
                PackageManager.RemoveItem(tmpItem);

            //增加物品
            PackageManager.UpdateItem(idb);
            //增加银币
            RoleLogicData.Self.AddGold(pl.freeGold);

            // 弹出奖励物品
//            DataCenter.OpenWindow("GET_REWARDS_WINDOW", dp);

            DataCenter.SetData("SHOP_WINDOW", "UPDATA_HEAD_INFO", "");

            //抽卡动画
            NewShopWindow tmpWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
            tmpWin.set("ANIMATION_CALLBACK", new CallBack(this, "OnCallback", dp));
            ItemDataBase tmpCurrCostData = tmpWin.GetCurentCostData(NewShopWindow.SHOP_PUMPING_TYPE.PRECIOUS_TEN);
            ShopChouKaData tmpShopData = new ShopChouKaData();
            tmpShopData.mBuyType = 3;
            tmpShopData.mCostTid = tmpCurrCostData.tid;
            tmpShopData.mCostCount = tmpCurrCostData.itemNum;
            tmpShopData.items = idb;
            tmpWin.ShowGainItemsUI(tmpShopData);

            //end
		}

		DEBUG.Log("RequestTenPreciousLotterySuccess:" + text);
	}
    //by chenliang
    //begin

    public void OnCallback(object param)
    {
        DataCenter.OpenWindow("GET_REWARDS_WINDOW", param);
    }

    //end

	void RequestTenPreciousLotteryFail(string text) {

		DEBUG.Log("RequestTenPreciousLotteryErr:" + text);
	}
}

// buy item
public class Button_buy_button : CEvent {
    //by chenliang
    //begin

    private int mNeedYbVal;

    //end
	public override bool _DoEvent() {
		int ii = Convert.ToInt32( getObject("ITEM_ID_TEST"));
		string needYb = get("NEED_COST_YB");
		int needYbVal = 0;
		int.TryParse(needYb, out needYbVal);

        //by chenliang

//         int usrYB = RoleLogicData.Self.diamond;
//         if (usrYB < needYbVal)
//         {
//             DataCenter.OpenMessageWindow("用户元宝不够");
//             return true;
//         }
//         ShopNetEvent.RequestShopPurchase(ii, 1, RequestShopPurchaseSuccess, RequestShopPurchaseFail);
//-------------------
        //判断购买物品所在背包是否有足够空间
        int tmpItemTid = (int)getObject("ITEM_TID");
        PACKAGE_TYPE[] tmpPackageType = new PACKAGE_TYPE[] { PackageManager.GetPackageTypeByItemTid(tmpItemTid) };
        if (!CheckPackage.Instance.CanAddItems(tmpPackageType))
            return true;

        int tmpLeftBuyNum = (int)getObject("LEFT_BUY_NUM");
        if (tmpLeftBuyNum == 0)
        {
            Action action = ()=> {
                ShopNetEvent.RequestPropShopQuery(Button_tool_shop_btn.RequestPropShopQuerySuccess, Button_tool_shop_btn.RequestPropShopQueryFail);
            };
            GameCommon.ToGetDiamond(TableCommon.getStringFromStringList(STRING_INDEX.RECHAGE_NAM_TIPS), null, action);
            return true;
        }
        mNeedYbVal = needYbVal;

        //by chenliang
        //begin

//        MallBuyConsumeData tmpConsumeData = new MallBuyConsumeData();
//---------------
        MallBuyConsumeData tmpConsumeData = new MallBuyConsumeData(ShopType.Mall);

        //end
        tmpConsumeData.Index = ii;
        tmpConsumeData.Tid = (int)getObject("ITEM_TID");
        tmpConsumeData.HasBuyCount = (int)getObject("HAS_BUY_NUM");
        tmpConsumeData.LeftBuyCount = tmpLeftBuyNum;
        tmpConsumeData.CostTid = (int)getObject("COST_TID");
        tmpConsumeData.Price = (int)getObject("NEED_COST_YB");
		tmpConsumeData.SureBuy = (int buyCount) =>
		{
			ShopNetEvent.RequestShopPurchase(tmpConsumeData.Index, buyCount, x => _OnRequestSuccess(buyCount, x), _OnRequestFailed);

		};
		tmpConsumeData.Complete = (string tmpRespText, int buyCount) =>
        {
            if (tmpRespText != "" && tmpRespText != "cancel")
            {
                set("BUY_COUNT", buyCount);
                RequestShopPurchaseSuccess(tmpRespText);
            }
        };
        DataCenter.OpenWindow("MALL_BUY_CONSUME_WINDOW", tmpConsumeData);

        //end

        //by chenliang
        //begin

//		DataCenter.OpenMessageWindow("购买道具，index->" + ii);
//-----------------------

        //end
		
		return true;
	}
	private void _OnRequestSuccess(int buyCount, string text)
	{
		SC_ResponseShopPurchase tmpResp = JCode.Decode<SC_ResponseShopPurchase>(text);
		
		if (tmpResp.isBuySuccess == 1)
		{
			DataCenter.CloseWindow("MALL_BUY_CONSUME_WINDOW");
			
			MallBuyConsumeWindow tmpWin = DataCenter.GetData("MALL_BUY_CONSUME_WINDOW") as MallBuyConsumeWindow;
			if (tmpWin != null)
				tmpWin.OnBuyComplete(text, buyCount);
		}
	}
	private void _OnRequestFailed(string text)
	{
		switch (text)
		{
		case "1113":
		{
			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SHOP_ITEM_MORE_THAN_LIMIT);
		} break;
		}
	}
	
	void RequestShopPurchaseSuccess(string text) {
		
		DataCenter.OnlyTipsLabelMessage("购买道具成功！");
		//by chenliang
		
		MallBuyConsumeWindow tmpWin = DataCenter.GetData("MALL_BUY_CONSUME_WINDOW") as MallBuyConsumeWindow;
        mNeedYbVal = (int)tmpWin.getObject("MALL_BUY_CONSUME_WINDOW_CURRENT_PRICE");
        RoleLogicData.Self.AddDiamond(-mNeedYbVal);

        SC_ResponseShopPurchase tmpResp = JCode.Decode<SC_ResponseShopPurchase>(text);
        if (tmpResp.isBuySuccess == 1)
        {
            //更新物品
            for (int i = 0, tmpLength = tmpResp.buyItem.Length; i < tmpLength; i++)
                PackageManager.UpdateItem(tmpResp.buyItem[i]);

            //减去相应物品数量
            ItemDataBase[] tmpItems = new ItemDataBase[tmpResp.buyItem.Length];
            for (int i = 0, count = tmpResp.buyItem.Length; i < count; i++)
                tmpItems[i] = new ItemDataBase() { tid = tmpResp.buyItem[i].tid, itemNum = tmpResp.buyItem[i].itemNum, itemId = tmpResp.buyItem[i].itemId };
            DataCenter.SetData("SHOP_WINDOW", "ITEM_SHOP_ITEM_CHANGED", new ShopItemData() { index = (int)getObject("ITEM_ID_TEST"), buyCount = (int)getObject("BUY_COUNT"), items = tmpItems });
        }

        //end
		DEBUG.Log("RequestShopPurchaseSuccess:" + text);
	}

	void RequestShopPurchaseFail(string text) {
		DataCenter.ErrorTipsLabelMessage("购买道具失败！");
		DEBUG.Log("RequestShopPurchaseErr:" + text);
	}
}

//by chenliang
//begin

// public class Button_but_shop_buy_vip : CEvent {
// 	public override bool _DoEvent() {
// 		int ii = Convert.ToInt32( getObject("ITEM_ID_TEST"));
// 		int openVipLevel = get("GIFT_OPEN_LEVEL");
// 		int costNum = get("COST_NUM");
// 
// 		int usrVipLevel = RoleLogicData.Self.vipLevel;
// 		int usrYb = RoleLogicData.Self.diamond;
// 		if(usrVipLevel < openVipLevel) {
// 			DataCenter.OpenMessageWindow("用户vip等级不够");
// 			return true;
// 		} else if(usrYb < costNum) {
// 			DataCenter.OpenMessageWindow("用户元宝不够");
// 			return true;
// 		}
// 		DataCenter.OpenMessageWindow("购买vip礼包，index->" + ii);
// 
// 		ShopNetEvent.RequestVipShopPurchase(ii, RequestVipShopPurchaseSuccess, RequestVipShopPurchaseFail);
// 
// 		return true;
// 	}
// 
//     void RequestVipShopPurchaseSuccess(string text) {
// 		SC_ResponseVipShop rvs = JCode.Decode<SC_ResponseVipShop>(text);
// 		if(rvs.isBuySuccess == 1) {
// 			PackageManager.AddItem(rvs.buyItem);
// 			DataCenter.OpenMessageWindow("购买vip礼包成功！");
// 		}
// 		DEBUG.Log("RequestVipShopPurchaseSuccess:" + text);
// 	}
// 
// 	void RequestVipShopPurchaseFail(string text) {
// 
// 		DataCenter.OpenMessageWindow("购买vip礼包失败！");
// 		DEBUG.Log("RequestVipShopPurchaseErr:" + text);			
// 	}
// }
//---------------------
    //已废弃
		
//end
		
		// toggle controller addon
public class Button_pet_shop_btn : CEvent {

	public override bool _DoEvent() {

        //by chenliang
        //begin

//        DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", SHOP_PAGE_TYPE.PET);
//--------------------
//        ShopNetEvent.RequestLotteryQuery(RequestLotteryQuerySuccess, RequestLotteryQueryFail);
        DataCenter.OpenWindow("SHOP_WINDOW", (int)SHOP_PAGE_TYPE.PET);

        //end

		return true;
	}
    //by chenliang
    //begin

    void RequestLotteryQuerySuccess(string text)
    {
        SC_ResponseLotteryQuery reslq = JCode.Decode<SC_ResponseLotteryQuery>(text);

        NewShopWindow.NewShopWindowParams param = new NewShopWindow.NewShopWindowParams();
        param.pageIndex = 0;
        param.obj = reslq;

        DataCenter.SetData("SHOP_WINDOW", "REFRESH", param);

        DEBUG.Log("LotteryQuerySuccess:" + text);
    }

    void RequestLotteryQueryFail(string text)
    {

        DEBUG.Log("LotteryQueryErr:" + text);
    }

    //end
}

public class Button_tool_shop_btn : CEvent {

	public override bool _DoEvent() {

		//int i = TableCommon.GetNumberFromMallConfig(1, "TAB_ID");

        //by chenliang
        //begin

//  		Dictionary<int, DataRecord> vRe = TableCommon.GetDataFromMallConfigByPage(1);
//  		NewShopWindow.NewShopWindowParams para = new NewShopWindow.NewShopWindowParams();
//  		para.pageIndex = 1;  
//  		para.obj = vRe;  
//  
//  		DataCenter.SetData("SHOP_WINDOW", "REFRESH", para);
//----------------
        //之前操作多余

//        ShopNetEvent.RequestPropShopQuery(RequestPropShopQuerySuccess, RequestPropShopQueryFail);
        DataCenter.OpenWindow("SHOP_WINDOW", (int)SHOP_PAGE_TYPE.TOOL);

        //end

		return true;
	}

	 public static  void RequestPropShopQuerySuccess(string text) {
		SC_ResponsePropShopQuery result = JCode.Decode<SC_ResponsePropShopQuery>(text);

		if(result.ret == (int)STRING_INDEX.ERROR_NONE) {

		}
        //by chenliang
        //begin

        DataCenter.SetData("SHOP_WINDOW", "REFRESH", new NewShopWindow.NewShopWindowParams() { pageIndex = 1, obj = result });

        //end

		DEBUG.Log("RequestPropShopQuerySuccess:" + text);
	}

	public static  void RequestPropShopQueryFail(string text) {

		DEBUG.Log("RequestPropShopQueryErr:" + text);
	}

}

public class Button_character_skin_shop_btn : CEvent {

	public override bool _DoEvent() {

        //by chenliang
        //begin

// 		Dictionary<int, DataRecord> vRe = TableCommon.GetDataFromMallConfigByPage(2);
// 		NewShopWindow.NewShopWindowParams para = new NewShopWindow.NewShopWindowParams();
// 		para.pageIndex = 2;  
// 		para.obj = vRe;  
// 		
// 		DataCenter.SetData("SHOP_WINDOW", "REFRESH", para);
//-----------------
        //之前操作多余

//        NewShopWindow tmpWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
//        object tmpObj = tmpWin.getObject("ALREADY_OPEN_VIP_SHOP");
//        if (tmpObj == null)
//        {
//            tmpWin.set("ALREADY_OPEN_VIP_SHOP", true);
//            GlobalModule.DoCoroutine(_DoAction());
//        }

//        OpenShopVIPWindow();
        DataCenter.OpenWindow("SHOP_WINDOW", (int)SHOP_PAGE_TYPE.CHARACTER);

        //end


		return true;
	}
    //by chenliang
    //begin

    public static void OpenShopVIPWindow()
    {
//        NewShopWindow tmpWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
//        object tmpObj = tmpWin.getObject("ALREADY_OPEN_VIP_SHOP");
//        if (tmpObj == null)
//        {
//            tmpWin.set("ALREADY_OPEN_VIP_SHOP", true);
//            GlobalModule.DoCoroutine(_DoAction());
//        }
//        else
//        {
//            tmpWin.CheckPlayGridTween("grid_tween_root_3");
//        }
        GlobalModule.DoCoroutine(_DoAction());
    }

    private static IEnumerator _DoAction()
    {
        VIP_VIPShopQuery_Requester tmpRequester = new VIP_VIPShopQuery_Requester();
        yield return tmpRequester.Start();

        if (tmpRequester.respCode == 1)
        {
            NewShopWindow.NewShopWindowParams tmpWinParam = new NewShopWindow.NewShopWindowParams();
            tmpWinParam.pageIndex = 2;
            tmpWinParam.obj = tmpRequester.respMsg;
            DataCenter.SetData("SHOP_WINDOW", "SET_SEL_PAGE", 3);

            DataCenter.SetData("SHOP_WINDOW", "REFRESH", tmpWinParam);
        }
    }

    //end
}
// end toggle controller addon


/// <summary>
/// 废弃的shop
/// </summary>
public class ShopWindow : tWindow
{
    Dictionary<SHOP_PAGE_TYPE, GameObject> mDicShopTabBtn = new Dictionary<SHOP_PAGE_TYPE, GameObject>();
    Dictionary<SHOP_PAGE_TYPE, BaseShopSlotGroup> mDicAllShopSlotGroup = new Dictionary<SHOP_PAGE_TYPE, BaseShopSlotGroup>();
    PetShopSlotGroup mPetShopSlotGroup = new PetShopSlotGroup();
    ToolShopSlotGroup mToolShopSlotGroup = new ToolShopSlotGroup();
    CharaterShopSlotGroup mCharacterShopSlotGroup = new CharaterShopSlotGroup();
    GoldShopSlotGroup mGoldShopSlotGroup = new GoldShopSlotGroup();
    DiamondShopSlotGroup mDiamondShopSlotGroup = new DiamondShopSlotGroup();
//	HonorShopSlotGroup mHonorShopSlotGroup = new HonorShopSlotGroup();

    SHOP_PAGE_TYPE mCurrentPageType = SHOP_PAGE_TYPE.PET;

//	ActiveObject mChouKaAnimatorObj;
	int mPetStar = 0;

//	tEvent mAniEvt;
	// itemid
	//consumeItemLogicData = DataCenter.GetData ("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
	//curConsumeItemData = consumeItemLogicData.GetDataByTid ((int)ITEM_TYPE.FATE_STONE);

	public ShopWindow(GameObject obj)
	{
		mGameObjUI = obj;
	}

	public override void Init()
	{
		if(GameObject.Find ("create_scene") == null)
		{
			this.StartCoroutine (LoadChouKaScene ());
		}

		PerloaderModel();

		mDicShopTabBtn.Add(SHOP_PAGE_TYPE.PET, mGameObjUI.transform.Find("tab_button_panel/tab_button/pet_shop_btn").gameObject);
		mDicShopTabBtn.Add(SHOP_PAGE_TYPE.TOOL, mGameObjUI.transform.Find("tab_button_panel/tab_button/tool_shop_btn").gameObject);
		mDicShopTabBtn.Add(SHOP_PAGE_TYPE.CHARACTER, mGameObjUI.transform.Find("tab_button_panel/tab_button/character_skin_shop_btn").gameObject);
		mDicShopTabBtn.Add(SHOP_PAGE_TYPE.GOLD, mGameObjUI.transform.Find("tab_button_panel/tab_button/gold_shop_btn").gameObject);
		mDicShopTabBtn.Add(SHOP_PAGE_TYPE.DIAMOND, mGameObjUI.transform.Find("tab_button_panel/tab_button/diamond_shop_btn").gameObject);
		mDicShopTabBtn.Add(SHOP_PAGE_TYPE.MYSTERIOUS, mGameObjUI.transform.Find("tab_button_panel/tab_button/mysterious_shop_btn").gameObject);

        mDicAllShopSlotGroup.Add(mPetShopSlotGroup.GetPageType(), mPetShopSlotGroup);
        mDicAllShopSlotGroup.Add(mToolShopSlotGroup.GetPageType(), mToolShopSlotGroup);
        mDicAllShopSlotGroup.Add(mCharacterShopSlotGroup.GetPageType(), mCharacterShopSlotGroup);
        mDicAllShopSlotGroup.Add(mGoldShopSlotGroup.GetPageType(), mGoldShopSlotGroup);
        mDicAllShopSlotGroup.Add(mDiamondShopSlotGroup.GetPageType(), mDiamondShopSlotGroup);
//		mDicAllShopSlotGroup.Add(mHonorShopSlotGroup.GetPageType(), mHonorShopSlotGroup);

        SetSelectShopBtn(SHOP_PAGE_TYPE.PET);
	}

	public IEnumerator LoadChouKaScene()
	{
		yield return new WaitForSeconds(0.3f);
		GlobalModule.Instance.LoadScene("chouka", true);
	}

	public void PerloaderModel()
	{
		List<DataRecord> records = TableCommon.FindAllRecords (DataCenter.mShopSlotBase, lhs => {return lhs["PAGE_TYPE"] == (int)SHOP_PAGE_TYPE.CHARACTER;});
		foreach(var v in records)
		{
			Preloader.PreloadModel (TableCommon.GetNumberFromActiveCongfig (v["BUY_ITEM_COUNT"], "MODEL"));
		}
	}

    public override bool Refresh(object param)
    {
		SetVisible ("jump_animator_button", false);
		SetVisibalNewMark ();
        SetUIInfo();
        return true;
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        switch (keyIndex)
        {
        case "SET_SEL_PAGE":
	        SetSelectShopBtn((SHOP_PAGE_TYPE)objVal);
	        break;
		case "BUY_ITEM_BY_GRID_INDEX":
			BuyItemByGridIndex((int)objVal);
			break;
		case "BUY_ITEM_RESULT":
			BuyItemResult((tEvent)objVal);
			break;
		case "UI_HIDDEN":
			UiHidden((bool)objVal);
			ShowMainScene (true);
			break;
		case "SHOW_GAIN_PET_WINDOW_CARD":
			SetVisible ("gain_pet_window_card", (bool)objVal);
			break;
//		case "ROLE_ANIMATOR_END":
//			ShowGainItemAnimator ((tEvent)objVal, "FuChouKaAnimitorEnd", 90013, 1f);
//			break;
		case "FU_ANIMATOR_END":
			FuAnimatorEnd ((tEvent)objVal);
			break;
		}
	}

	private void SetVisibalNewMark()
	{
		ShopLogicData shopLogicData = DataCenter.GetData ("SHOP_DATA") as ShopLogicData;
		foreach(SHOP_PAGE_TYPE type in Enum.GetValues (typeof(SHOP_PAGE_TYPE)))
		{
			GameObject newMarkObj = GameCommon.FindObject (mDicShopTabBtn[type], "NewMark");
			if(newMarkObj != null)
			{
				if(shopLogicData == null) 
					newMarkObj.SetActive (false);
				else
					newMarkObj.SetActive (shopLogicData.GetFreeByPageType (type));
			}
		}
	}

	public void UiHidden(bool isHidden)
	{
		SetVisible (isHidden, "center_background", "tab_button_panel", "tab_context_panel");
		SetVisible (!isHidden, "black_background");

		if(isHidden)
		{
			DataCenter.OpenWindow ("INFO_GROUP_WINDOW");
			DataCenter.OpenWindow ("BACK_GROUP_SHOP_WINDOW");
		}
		else 
		{
			DataCenter.CloseWindow ("INFO_GROUP_WINDOW");
			DataCenter.CloseWindow ("BACK_GROUP_SHOP_WINDOW");
		}
	}

	public void SetSelectShopBtn(SHOP_PAGE_TYPE shopPageType)
    {
		if(DataCenter.Get ("WHICH_SHOP_PAGE"))
		{
			int iWhichShopPage = DataCenter.Get ("WHICH_SHOP_PAGE");
			shopPageType = (SHOP_PAGE_TYPE)iWhichShopPage;
			SetToggleTrue(shopPageType);
			int iShopPagePet = Convert.ToInt32 (SHOP_PAGE_TYPE.PET);
			DataCenter.Set ("WHICH_SHOP_PAGE", iShopPagePet);
		}
        else GameCommon.ToggleTrue(mDicShopTabBtn[shopPageType]);

        Refresh(null);
    }

	public void SetToggleTrue(SHOP_PAGE_TYPE shopPageType)
	{
		foreach (KeyValuePair<SHOP_PAGE_TYPE, GameObject> iter in mDicShopTabBtn)
		{
			if(shopPageType == iter.Key) GameCommon.ToggleTrue (mDicShopTabBtn[shopPageType]);
			else GameCommon.ToggleFalse (iter.Value);
		}
	}

	public void BuyItemByGridIndex(int iGridIndex)
	{
		if (CommonParam.bIsNetworkGame)
		{
			SendBuyItem(iGridIndex);
		}
		else
		{
			CS_BuyItemResult respEvt = new CS_BuyItemResult();
			respEvt.set("GRID_INDEX", 0);
			respEvt.set("IS_BAG_FULL", false);
			respEvt.set("GOLD", 1000);
			respEvt.set("TYPE", 0);
			respEvt.set("ID", 191005);
			respEvt.set("COUNT", 1);
			respEvt.set ("IS_FREE", false);
			respEvt.set ("IS_DISCOUNT", false);
			BuyItemResult(respEvt);
		}
	}

	public BaseShopSlot GetSelectBaseShopSlot(int iGridIndex)
	{
		if(mDicAllShopSlotGroup[mCurrentPageType].mDicShopSlot.ContainsKey(iGridIndex))
		{
			BaseShopSlot shopSlot = mDicAllShopSlotGroup[mCurrentPageType].mDicShopSlot[iGridIndex];
			return shopSlot;
		}
		return null;
	}
	
	public SHOP_PAGE_TYPE GetCurPageType()
    {
		foreach (KeyValuePair<SHOP_PAGE_TYPE, GameObject> iter in mDicShopTabBtn)
        {
            UIToggle toggle = iter.Value.GetComponent<UIToggle>();
            if (toggle.value)
                return iter.Key;
        }

		return SHOP_PAGE_TYPE.PET;
    }

    public void SetUIInfo()
    {
        mCurrentPageType = GetCurPageType();

		if(mCurrentPageType == SHOP_PAGE_TYPE.MYSTERIOUS)
		{
			DataCenter.OpenWindow ("MYSTERIOUS_SHOP_WINDOW");
			return ;
		}
		else
			DataCenter.CloseWindow ("MYSTERIOUS_SHOP_WINDOW");

		GameObject currentPageContext = mGameObjUI.transform.Find("tab_context_panel/scrollview_context_" + ((int)mCurrentPageType).ToString()).gameObject;
		UIGridContainer currentPageGrid = currentPageContext.transform.Find("scrollview_context/scrollview/grid").GetComponent<UIGridContainer>();
		UIScrollView currentPageScrollView = currentPageContext.transform.Find("scrollview_context/scrollview").GetComponent<UIScrollView>();

        SetDicShopSlot();

        currentPageGrid.MaxCount = mDicAllShopSlotGroup[mCurrentPageType].mDicShopSlot.Count;

        mDicAllShopSlotGroup[mCurrentPageType].mCurrentPageGrid = currentPageGrid;
        mDicAllShopSlotGroup[mCurrentPageType].InitContextUI();
    }    

    public void SetDicShopSlot()
    {
        if (mDicAllShopSlotGroup[mCurrentPageType].mDicShopSlot.Count == 0)
        {
            Dictionary<int, DataRecord> dicRecord = DataCenter.mShopSlotBase.GetAllRecord();

            foreach (KeyValuePair<int, DataRecord> iter in dicRecord)
            {
                if ((int)iter.Value.get("PAGE_TYPE") == (int)mCurrentPageType)
                {
                    mDicAllShopSlotGroup[mCurrentPageType].AttachShopSlot(iter.Value);
                }
            }
        }
    }

	public virtual void SendBuyItem(int iGridIndex)
	{
		int iShopSlotIndex = (int)get ("SHOP_SLOT_INDEX");
		bool isDiscount = (bool)get ("IS_DISCOUNT");
		bool isFree = Convert.ToBoolean(TableCommon.GetNumberFromShopSlotBase (iShopSlotIndex, "IS_FREE"));

		if(isFree)
		{
			ShopLogicData shopLogicData = DataCenter.GetData ("SHOP_DATA") as ShopLogicData;
			if(shopLogicData != null)
				isFree = shopLogicData.IsFreeByIndex (iShopSlotIndex);
			else 
				DEBUG.LogError ("ShopLogicData is null");
		}

		tEvent quest = Net.StartEvent("CS_BuyItemResult");
		quest.set ("IS_FREE", isFree);
		quest.set ("IS_DISCOUNT", isDiscount);
		quest.set("SHOP_SLOT_INDEX", iShopSlotIndex);
		quest.set("GRID_INDEX", iGridIndex);
		quest.DoEvent();
	}

	public void BuyItemResult(tEvent respEvt)
	{
		bool isFree = respEvt.get ("IS_FREE");
        if (isFree)
        {
            SetFreeShopData(respEvt);
            RefreshFreeInfo(respEvt);

            GuideManager.Notify(GuideIndex.GetFirstFreePet);
            GuideManager.Notify(GuideIndex.GetSecondFreePet);
        }
        else
        {
            ShopLogicData shopLogicData = DataCenter.GetData("SHOP_DATA") as ShopLogicData;
            int iShopSlotindex = respEvt.get("SHOP_SLOT_INDEX");
            ShopData shopData = shopLogicData.GetShopDataByIndex(iShopSlotindex);
            shopData.mUsedCount++;

            CostItem(respEvt);

            if (mCurrentPageType == SHOP_PAGE_TYPE.CHARACTER)
            {
                mDicAllShopSlotGroup[mCurrentPageType].InitContextUI();
				DataCenter.OnlyTipsLabelMessage (STRING_INDEX.ERROR_SHOP_BUY_SUCCESS);
            }
        }

		GainItems(respEvt);
		ShowGainItemsUI(respEvt);

        //GuideManager.Notify(GuideIndex.GetFirstFreePet);    
	}

	public void SetFreeShopData(tEvent respEvt)
	{
		int iShopSlotIndex = respEvt.get ("SHOP_SLOT_INDEX");
		ShopLogicData shopLogicData = DataCenter.GetData ("SHOP_DATA") as ShopLogicData;
		shopLogicData.SetShopDataByIndex (iShopSlotIndex, 1, CommonParam.NowServerTime ());
	}

	public void RefreshFreeInfo(tEvent respEvt)
	{
		int iGridIndex = respEvt.get ("GRID_INDEX");
		int iShopSlotIndex = respEvt.get ("SHOP_SLOT_INDEX");
		int iFreeIntervalTime = TableCommon.GetNumberFromShopSlotBase(iShopSlotIndex, "FREE_INTERVAL");
		int iMaxFreeCount = TableCommon.GetNumberFromShopSlotBase(iShopSlotIndex, "FREE_COUNT");
		bool isDailyFreeCard = Convert.ToBoolean(TableCommon.GetNumberFromShopSlotBase(iShopSlotIndex, "IS_DAILY_REFRESH"));

		GameObject currentPageContext = mGameObjUI.transform.Find("tab_context_panel/scrollview_context_" + ((int)mCurrentPageType).ToString()).gameObject;
		UIGridContainer currentPageGrid = currentPageContext.transform.Find("scrollview_context/scrollview/grid").GetComponent<UIGridContainer>();
		GameObject currentObj = currentPageGrid.controlList[iGridIndex];

		ShopLogicData shopLogicData = DataCenter.GetData ("SHOP_DATA") as ShopLogicData;
		ShopData shopData = shopLogicData.GetShopDataByIndex (iShopSlotIndex);
		if(shopData != null)
		{
			GameObject labelCountdownObj = GameCommon.FindObject (currentObj, "label_countdown");
			CountdownUI countdownUI = labelCountdownObj.GetComponent<CountdownUI>();
			if(countdownUI != null) 
				MonoBehaviour.Destroy (countdownUI);
			
			GameCommon.SetUIVisiable (currentObj, true, "icon_label", "icon_sprite");
			GameCommon.SetUIVisiable (currentObj, "free_label", false);

			if(isDailyFreeCard)
			{
				int freeCount = iMaxFreeCount - shopData.mUsedCount;
				if(freeCount < 0)
				{
					DEBUG.LogError ("usedCount  more than maxFreeCount >> freeCount = " + freeCount.ToString ());
					freeCount =  0;
				}

				GameCommon.SetUIText (currentObj, "label_daily_chance", (freeCount).ToString ());
				if(freeCount > 0)
				{
					GameCommon.SetUIVisiable (currentObj, "label_daily_chance", false);
					GameCommon.SetUIVisiable (currentObj, "label_countdown", true);
					tWindow.SetCountdown (currentObj, "label_countdown", (Int64)(iFreeIntervalTime + shopData.mGetFreeCardLastTime), 
					                      new CallBack(this, "DailyCountdownOverEvent", currentObj));
				}
			}
			else
			{
				GameCommon.SetUIVisiable (currentObj, "label_get_one_chance", false);
				GameCommon.SetUIVisiable (currentObj, "label_countdown", true);
				tWindow.SetCountdown (currentObj, "label_countdown", (Int64)(iFreeIntervalTime + shopData.mGetFreeCardLastTime), 
				                      new CallBack(this, "OnceCountdownOverEvent", currentObj));
			}
		}
		else 
			DEBUG.LogWarning ("not find ShopData in ShopLogicData by >>>>" + iShopSlotIndex.ToString ());
	}

	public void DailyCountdownOverEvent(GameObject obj)
	{
		GameCommon.SetUIVisiable (obj, "label_countdown", false);
		GameObject labelDailyChanceObj = GameCommon.FindObject (obj, "label_daily_chance");
		labelDailyChanceObj.SetActive (true);
		UILabel labelDailyChance = labelDailyChanceObj.GetComponent<UILabel>();
		int dailyChance = Convert.ToInt32(labelDailyChance.text);

		GameCommon.SetUIVisiable (obj, !Convert.ToBoolean (dailyChance), "icon_label", "icon_sprite");
		GameCommon.SetUIVisiable (obj, Convert.ToBoolean (dailyChance), "free_label");
	}

	public void OnceCountdownOverEvent(GameObject obj)
	{
		GameCommon.SetUIVisiable (obj, false, "label_countdown", "icon_label", "icon_sprite");
		GameCommon.SetUIVisiable (obj, true, "label_get_one_chance", "free_label");
	}

	public virtual void CostItem(tEvent respEvt)
	{		
		int iShopSlotIndex = respEvt.get ("SHOP_SLOT_INDEX");
		int iCostItemType = TableCommon.GetNumberFromShopSlotBase(iShopSlotIndex, "COST_ITEM_TYPE");
		int iCostItemCount = TableCommon.GetNumberFromShopSlotBase(iShopSlotIndex, "COST_ITEM_COUNT");
		
		if(get ("IS_DISCOUNT")) iCostItemCount  = (int)(iCostItemCount * 0.9);
		
		GameCommon.RoleChangeNumericalAboutRole (iCostItemType, (-1) * iCostItemCount);
	}

	public virtual void GainItems(tEvent respEvt)
	{
		SetCheckTaskConditionsNeedData(respEvt);

		int iShopSlotIndex = respEvt.get ("SHOP_SLOT_INDEX");
		bool bIsExtract = TableCommon.GetNumberFromShopSlotBase(iShopSlotIndex, "GROUP_ID") != 0 ? true : false;
		int iBuyItemType = TableCommon.GetNumberFromShopSlotBase(iShopSlotIndex, "BUY_ITEM_TYPE");
		int iBuyItemCount = TableCommon.GetNumberFromShopSlotBase(iShopSlotIndex, "BUY_ITEM_COUNT");

		if(!bIsExtract)
		{
			if (iBuyItemType == (int)ITEM_TYPE.CHARACTER)
			{
				RoleLogicData roleLogicData = RoleLogicData.Self;
                if (GameCommon.GetCharacterTypeByModelIndex(iBuyItemCount) == GameCommon.GetCharacterTypeByModelIndex(RoleLogicData.GetMainRole().tid))
                    return;
				DataRecord roleDataRecord = DataCenter.mActiveConfigTable.GetRecord (iBuyItemCount);
				if(roleDataRecord != null)
				{
					RoleData role = new RoleData();
                    role.mIndex = roleLogicData.GetRoleCount() + 1;
					role.level = 1;
					role.exp = 0;
					role.tid = iBuyItemCount;
					role.starLevel = roleDataRecord["STAR_LEVEL"];
                    roleLogicData.mRoleList[role.mIndex - 1] = role;
				}
				else DEBUG.Log ("role data is null");
			}
			else
				GainNumericalItem (iShopSlotIndex, iBuyItemType, iBuyItemCount);
		}
		else
		{
            object resultData;
            if (!respEvt.getData("RESULT_TABLE", out resultData))
			{
				return;
			}
			NiceTable itemData = resultData as NiceTable;
            foreach (KeyValuePair<int, DataRecord> r in itemData.GetAllRecord())
            {
                DataRecord re = r.Value;
				int iType = re.getData("ITEM_TYPE");
				int iCount = re.getData("ITEM_COUNT");
				int element = re.getData("ELEMENT");
				GainNumericalItem (iShopSlotIndex, iType, iCount);
            }
		}
	}

	public virtual void GainGroupItem(DataRecord dataRecord, int iShopSlotIndex)
	{

	}
	
	void GainNumericalItem(int iShopSlotIndex, int iItemType, int iItemCount)
	{
		if (iItemType == (int)ITEM_TYPE.YUANBAO)
		{
			int luckyIndex = RoleLogicData.Self.mLuckyGuyMultipleIndex;
			float luckyGuyMultiple = 1.0f;
			if(luckyIndex != 0)
				luckyGuyMultiple = DataCenter.mActiveLuckyGuyConfig.GetRecord (luckyIndex)["COEFFICIENT"];
			
			float minLuckyGuyMultiple = 1.0f;
			float maxLuckyGuyMultiple = 2.1f;
			foreach(KeyValuePair <int, DataRecord> r in DataCenter.mActiveLuckyGuyConfig.GetAllRecord ())
			{
				if(minLuckyGuyMultiple > r.Value["COEFFICIENT"] && r.Key != 0)
					minLuckyGuyMultiple = r.Value["COEFFICIENT"];
				
				if(maxLuckyGuyMultiple < r.Value["COEFFICIENT"] && r.Key != 0)
					maxLuckyGuyMultiple = r.Value["COEFFICIENT"];
			}

			if(luckyGuyMultiple < minLuckyGuyMultiple) luckyGuyMultiple = minLuckyGuyMultiple;
			if(luckyGuyMultiple > maxLuckyGuyMultiple) luckyGuyMultiple = maxLuckyGuyMultiple;
			
			iItemCount = (int)(iItemCount * luckyGuyMultiple);
			RoleLogicData.Self.mLuckyGuyMultipleIndex = 1001;
			
			GameCommon.RoleChangeDiamond(iItemCount);
			
			int iCost = TableCommon.GetNumberFromShopSlotBase (iShopSlotIndex, "COST_ITEM_COUNT");
			RoleLogicData roleLogicData = RoleLogicData.Self;
			roleLogicData.mVIPExp += iCost;
			AddVipLevel ();
		}
		else
			GameCommon.RoleChangeNumericalAboutRole (iItemType, iItemCount);

		if(iItemType == (int)ITEM_TYPE.GOLD)
			DataCenter.OnlyTipsLabelMessage (STRING_INDEX.ERROR_SHOP_BUY_SUCCESS);
	}

	public void AddVipLevel()
	{
		RoleLogicData roleLogicData = RoleLogicData.Self;
		if(roleLogicData.vipLevel == GetMaxVipLevel()) 
			return;

		int iIndex = roleLogicData.vipLevel + 1001;
		int iNeedCost = TableCommon.GetNumberFromVipList (iIndex, "CASHPAID");

		if(roleLogicData.mVIPExp >= iNeedCost) 
		{
			roleLogicData.mVIPExp -= iNeedCost;
			roleLogicData.vipLevel++;
	
			DataCenter.SetData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_ROLE_NAME", true);

			AddVipLevel ();
		}
	}

	int GetMaxVipLevel()
	{
		for(int i = 0; i < DataCenter.mVipListConfig.GetRecordCount (); i++)
		{
			if(TableCommon.GetStringFromVipList (i + 1000, "name") == "")
			{
				return (i - 1);
			}
		}

		return 10;
	}
	
	public virtual void ShowGainItemsUI(tEvent respEvt)
	{
		object resultData;
		if (!respEvt.getData("RESULT_TABLE", out resultData))
		{
			return;
		}
		NiceTable itemData = resultData as NiceTable;

		bool bIsPet = false;
		foreach (KeyValuePair<int, DataRecord> r in itemData.GetAllRecord())
		{
			if(r.Value.getData ("ITEM_TYPE") == (int)ITEM_TYPE.PET)
				bIsPet = true;
		}

		UiHidden (false);
		ShowMainScene (!bIsPet);
		SetVisible ("black_background", !bIsPet);
		if(itemData.GetAllRecord().Count == 1)
		{
			ShowGainSingleItemUI(respEvt);
			if(bIsPet)
			{
				ShowGainItemAnimator(() => RoleAnimatorEnd (respEvt), respEvt, 90012, 1.5f);
				ShowShopGainPetWindow (false);
			}
		}
		else
		{
			if(bIsPet)
				ShowGainItemAnimator(() => RoleAnimatorEnd (respEvt), respEvt, 90012, 1.5f);
			else
				ShowGainGroupAllItemsUI(respEvt);
		}
	}


	public void ShowGainItemAnimator(Action callBack, tEvent respEvt, int modelIndex, float fScale)
	{
		bool bIsRole = modelIndex == 90012 ? true : false;
		GameObject obj = GameObject.Find ("create_scene");
		if(obj == null)
			return;

		float fAniSpeed = 1.0f;
		fAniSpeed = bIsRole ? 1.0f : 1.5f;
		string strPoint = "";
		strPoint = bIsRole ? "role_point" : "fu_point";

		GameCommon.SetUIVisiable (obj, "item", !bIsRole);
		GameCommon.SetUIVisiable (obj, "effect_background", bIsRole);

		ActiveObject mChouKaAnimatorObj = new ActiveObject();
		mChouKaAnimatorObj.mMainObject = new GameObject("_Role_");
		mChouKaAnimatorObj.CreateMainObject (modelIndex);
		GameCommon.SetLayer (mChouKaAnimatorObj.mMainObject, CommonParam.PlayerLayer);
		mChouKaAnimatorObj.mMainObject.transform.parent = GameCommon.FindObject (obj, strPoint).transform;
		mChouKaAnimatorObj.mMainObject.transform.localScale *= fScale; 
		mChouKaAnimatorObj.SetVisible (true);
		mChouKaAnimatorObj.mMainObject.transform.localPosition = Vector3.zero;
		mChouKaAnimatorObj.mMainObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

		if(bIsRole)
		{
            GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UI/ec_ui_futuowei", GameCommon.FindObject(mChouKaAnimatorObj.mMainObject, "Bone056"));
		}
		else
		{
            GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UI/ec_ui_fufly2", GameCommon.FindObject(mChouKaAnimatorObj.mMainObject, "Bone056"));
            GameCommon.LoadAndIntanciateEffectPrefabs("Effect/UI/ec_ui_fubagua", GameCommon.FindObject(obj, "effect_bagua"));
		}

		mChouKaAnimatorObj.PlayMotion ("chouka", null);
		mChouKaAnimatorObj.SetAnimSpeed (fAniSpeed);
		ChouKaAnimitorEnd mAniEvt = EventCenter.Start ("ChouKaAnimitorEnd") as ChouKaAnimitorEnd;
		mAniEvt.set ("CHOU_KA_OBJ", mChouKaAnimatorObj);
		mAniEvt.set ("GAIN_ITEM_DATA", respEvt);
		mAniEvt.mAction = callBack;
//		mAniEvt.set ("WINDOW_NAME", "SHOP_WINDOW");
		mAniEvt.DoEvent ();

		SetVisible ("jump_animator_button", true);
		GameCommon.GetButtonData (mGameObjUI, "jump_animator_button").set ("ANI_EVT", mAniEvt);
		GameCommon.GetButtonData (mGameObjUI, "jump_animator_button").set ("WINDOW_NAME", "SHOP_WINDOW");
	}

	void RoleAnimatorEnd(tEvent respEvt)
	{
		ShowGainItemAnimator (() => FuAnimatorEnd (respEvt), respEvt, 90013, 1f);
	}

	void FuAnimatorEnd(tEvent respEvt)
	{
		SetVisible ("jump_animator_button", false);
		object resultData;
		if (!respEvt.getData("RESULT_TABLE", out resultData))
			return;
		
		NiceTable itemData = resultData as NiceTable;
		if(itemData.GetRecordCount() == 1)
			ShowShopGainPetWindow (true);
		else
			ShowGainGroupAllItemsUI(respEvt);
	}

	void ShowMainScene(bool bVisible)
	{
		GameObject obj = GameObject.Find ("Mainmenu_bg");
		if(obj != null)
		{
			RenderSettings.fogColor = bVisible ? new Color(65f/255f, 251f/255f, 213f/255f) : new Color(76f/255f, 141f/255f, 195f/255f);

			foreach(Transform t in obj.transform)
			t.gameObject.SetActive (bVisible);
		}
	}

	void SetPetStar(int petIndex)
	{
		mPetStar = TableCommon.GetNumberFromActiveCongfig (petIndex, "STAR_LEVEL");
	}

	public void SetShopPetBuyAgainButton(tEvent respEvt)
	{
		int iGridIndex = respEvt.get("GRID_INDEX");
		int iShopSlotIndex = respEvt.get ("SHOP_SLOT_INDEX");
		
		shop_gain_pet_window shopGainPetWindow = DataCenter.GetData("shop_gain_pet_window") as shop_gain_pet_window;
		if(shopGainPetWindow != null)
		{
			SetShopBuyAgainButton(shopGainPetWindow.mGameObjUI, iGridIndex, iShopSlotIndex);
		}
	}

	public void SetShopItemBuyAgainButton(tEvent respEvt)
	{
		int iGridIndex = respEvt.get("GRID_INDEX");
		int iShopSlotIndex = respEvt.get ("SHOP_SLOT_INDEX");
		
		shop_gain_item_window shopGainItemWindow = DataCenter.GetData("shop_gain_item_window") as shop_gain_item_window;
		if(shopGainItemWindow != null)
		{
			SetShopBuyAgainButton(shopGainItemWindow.mGameObjUI, iGridIndex, iShopSlotIndex);
		}
	}

	public void SetShopBuyAgainButton(GameObject obj, int gridIndex, int shopSlotIndex)
	{
		if(obj != null)
		{
			int costCount = TableCommon.GetNumberFromShopSlotBase(shopSlotIndex, "COST_ITEM_COUNT");
			bool isDiscount = Convert.ToBoolean (TableCommon.GetNumberFromShopSlotBase (shopSlotIndex, "IS_DISCOUNT"));
			if(isDiscount) costCount = (int)(costCount * 0.9);
			GameCommon.SetUIVisiable (obj, "discount_effect", isDiscount);

			UIButtonEvent buttonData = obj.transform.Find("but_shop_buy_again").gameObject.GetComponent<UIButtonEvent>();
			buttonData.mData.set("GRID_INDEX", gridIndex);
			buttonData.mData.set("SHOP_SLOT_INDEX", shopSlotIndex);
			buttonData.mData.set ("IS_DISCOUNT", isDiscount);

			// number
			UILabel iconLabel = obj.transform.Find("but_shop_buy_again/icon_label").gameObject.GetComponent<UILabel>();
			iconLabel.text = costCount.ToString();
			
			// icon
			int iItemType = TableCommon.GetNumberFromShopSlotBase(shopSlotIndex, "COST_ITEM_TYPE");
			DataRecord itemDataRecord = DataCenter.mItemIcon.GetRecord(iItemType);
			if (itemDataRecord != null)
			{
				UISprite iconSprite = obj.transform.Find("but_shop_buy_again/icon_sprite").gameObject.GetComponent<UISprite>();
				string strAtlasName = itemDataRecord.get("ITEM_ATLAS_NAME").ToString ();
				string strSpriteName = itemDataRecord.get("ITEM_SPRITE_NAME").ToString ();
				GameCommon.SetIcon (iconSprite, strAtlasName, strSpriteName);
			}
			else
			{
				DEBUG.LogError("dataRecord is null");
			}
		}
	}

	public virtual void ShowGainSingleItemUI(tEvent respEvt)
	{
		object resultData;
		if (!respEvt.getData("RESULT_TABLE", out resultData))
		{
			return;
		}
		NiceTable itemData = resultData as NiceTable;

		foreach (KeyValuePair<int, DataRecord> r in itemData.GetAllRecord())
		{
			DataRecord re = r.Value;
			int iItemType = re.getData("ITEM_TYPE");
			int iItemID = re.getData("ITEM_ID");
			if (iItemType == (int)ITEM_TYPE.PET)
			{
				SetShopPetBuyAgainButton(respEvt);

				SetPetStar (iItemID);
                DataCenter.SetData("shop_gain_pet_window", "OPEN", true);
                DataCenter.SetData("shop_gain_pet_window", "SET_SELECT_PET_BY_MODEL_INDEX", iItemID);

				return;
			}

			SetShopItemBuyAgainButton(respEvt);

			DataCenter.SetData("shop_gain_item_window", "SET_SELECT_ITEM_BY_MODEL_INDEX", re);
			DataCenter.SetData("shop_gain_item_window", "OPEN", true);
		}
	}

	void ShowShopGainPetWindow(bool bVisible)
	{
		GameObject shopGainPetWindowObj = GameCommon.FindObject (mGameObjUI, "shop_gain_pet_window");
		shopGainPetWindowObj.SetActive (bVisible);

		if(!bVisible) 
			return;
			
//		GameCommon.SetUIVisiable (shopGainPetWindowObj, "moreThanThreeStarLevelEffect", mPetStar > 3);
		//GameCommon.SetUIVisiable (shopGainPetWindowObj, "background_effect", mPetStar > 3);
//		ShowStar ();

		PlayTweenScale(GameCommon.FindObject (shopGainPetWindowObj, "gain_pet_window_card"));

		PlayTweenScale(GameCommon.FindObject (shopGainPetWindowObj, "but_shop_buy_again"));
		PlayTweenScale(GameCommon.FindObject (shopGainPetWindowObj, "shop_gain_pet_window_close_btn"));
		PlayTweenScale(GameCommon.FindObject (shopGainPetWindowObj, "shop_gain_pet_window_details_btn"));
		
		GuideManager.Notify(GuideIndex.GetFirstFreePetEnd);
	}

	void ShowStar()
	{
		if(mPetStar == 0) 
			return;
	
		for(int i = 1; i < 6; i++)
		{
			SetVisible ("star_" + i.ToString (), false);
			
			if(i > 2) SetVisible ("effect_font_star_" + i.ToString (), false);
		}
		
		SetVisible ("star_" + mPetStar.ToString (), true);
		GameObject starInfo = GetSub ("star_" + mPetStar.ToString ());
		for(int i = 1; i < mPetStar + 1; i ++)
		{
			GameCommon.SetUIVisiable (starInfo, "star" + i.ToString (), false);
		}
		
		this.StartCoroutine (ShowStar (starInfo));
	}

	IEnumerator ShowStar(GameObject parentObj)
	{
		for(int i = 1; i < mPetStar + 1; i ++)
		{
			yield return new WaitForSeconds(0.3f);
			GameCommon.SetUIVisiable (parentObj, "star" + i.ToString (), true);
		}

		if(mPetStar > 2) SetVisible ("effect_font_star_" + mPetStar.ToString (), true);
	}

	public void PlayTweenScale(GameObject obj)
	{
		TweenScale [] tweens;
		tweens = obj.GetComponents<TweenScale>();
		foreach(TweenScale t in tweens)
		{
			t.ResetToBeginning ();
			t.PlayForward ();
		}
	}
	
	public virtual void ShowGainGroupAllItemsUI(tEvent respEvt)
	{
		object resultData;
		if (!respEvt.getData("RESULT_TABLE", out resultData))
		{
			return;
		}
		NiceTable itemData = resultData as NiceTable;

		SetShopItemBuyAgainButton(respEvt);

		DataCenter.SetData("shop_gain_item_window", "SET_SELECT_ITEMS_BY_MODEL_INDEX", itemData);
		DataCenter.SetData("shop_gain_item_window", "OPEN", true);
	}

	void SetCheckTaskConditionsNeedData(tEvent respEvt)
	{
		int result = respEvt.get("RESULT");
		if ((STRING_INDEX)result == STRING_INDEX.ERROR_NONE)
		{
			int iStageIndex = DataCenter.Get("CURRENT_STAGE");
			int iGold = respEvt.get("GOLD");
			int iExp = respEvt.get("EXP");
			int iType = respEvt.get("TYPE");
			int index = respEvt.get("ID");
			
			tLogicData taskNeedData = DataCenter.GetData("TASK_NEED_DATA");
			taskNeedData.set("GOLD", iGold);
			taskNeedData.set("EXP", iExp);
			taskNeedData.set("ITEM_TYPE", iType);
			taskNeedData.set("ITEM_ID", index);
			taskNeedData.set("NUM", 1);
		}
	}
}

//--------------------------------------------------------------------------------------------
// slot group
public class BaseShopSlotGroup : tLogicData
{
    public UIGridContainer mCurrentPageGrid;
    public GameObject mCurrentGridItemObj;

    public Dictionary<int, BaseShopSlot> mDicShopSlot = new Dictionary<int, BaseShopSlot>();

    public virtual SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.PET; }

    public virtual BaseShopSlot NewShopSlot(int miGridIndex, DataRecord dataRecord)
    {
        BaseShopSlot shopSlot = NewShopSlot();

        if(shopSlot != null)
        {
            shopSlot.mGridIndex = miGridIndex;
            shopSlot.mDataRecord = dataRecord;
            shopSlot.mShopSlotGroup = this;
            shopSlot.mSlotTableIndex = (int)dataRecord.get("INDEX");
        }
        return shopSlot; 
    }

    public virtual BaseShopSlot NewShopSlot() { return null; }

    public virtual void AttachShopSlot(DataRecord dataRecord)
    {
        int iGridIndex = mDicShopSlot.Count;
        mDicShopSlot.Add(iGridIndex, NewShopSlot(iGridIndex, dataRecord));
    }

    public virtual void InitContextUI()
    {
        foreach (KeyValuePair<int, BaseShopSlot> iter in mDicShopSlot)
        {
            iter.Value.InitContextUI();
        }
    }
}

public class PetShopSlotGroup : BaseShopSlotGroup
{
    public override BaseShopSlot NewShopSlot() { return new PetShopSlot(); }
	public override SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.PET; }
}

public class ToolShopSlotGroup : BaseShopSlotGroup
{
    public override BaseShopSlot NewShopSlot() { return new ToolShopSlot(); }
	public override SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.TOOL; }
}

public class CharaterShopSlotGroup : BaseShopSlotGroup
{
    public override BaseShopSlot NewShopSlot() { return new CharaterShopSlot(); }
	public override SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.CHARACTER; }
}

public class GoldShopSlotGroup : BaseShopSlotGroup
{
    public override BaseShopSlot NewShopSlot() { return new GoldShopSlot(); }
	public override SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.GOLD; }
}

public class DiamondShopSlotGroup : BaseShopSlotGroup
{
    public override BaseShopSlot NewShopSlot() { return new DiamondShopSlot(); }
	public override SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.DIAMOND; }
}
//public class HonorShopSlotGroup : BaseShopSlotGroup
//{
//	public override BaseShopSlot NewShopSlot() { return new HonorShopSlot(); }
//	public override SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.HONOR; }
//}

//--------------------------------------------------------------------------------------------
public class BaseShopSlot : tLogicData
{
	public BaseShopSlotGroup mShopSlotGroup;
    public GameObject mCurrentGridItemObj;
    public int mGridIndex = 0;
    public int mSlotTableIndex = 0;

    public DataRecord mDataRecord;

    public virtual SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.PET; }
    
	public int mFreeCount = 0;
	public Int64 mGetFreeCardLastTime = 0;

    public virtual void InitContextUI()
    {
        if (mGridIndex < mShopSlotGroup.mCurrentPageGrid.MaxCount)
        {
            mCurrentGridItemObj = mShopSlotGroup.mCurrentPageGrid.controlList[mGridIndex];

			// slot item info
			SetSlotItemInfo();

            // name
            SetSlotTitleName();

            // description
            SetSlotDescription();

            // flag
            SetSlotFlag();

            // slot button
            SetSlotBuyButtonIcon();

			GetFreeCard();
        }
    }

	public virtual void GetFreeCard()
	{
		bool bIsFree = Convert.ToBoolean(TableCommon.GetNumberFromShopSlotBase(mSlotTableIndex, "IS_FREE"));
		GameCommon.SetUIVisiable (mCurrentGridItemObj, "free_info", bIsFree);
		if(!bIsFree) return;

		int iFreeIntervalTime = TableCommon.GetNumberFromShopSlotBase(mSlotTableIndex, "FREE_INTERVAL");
		int iMaxFreeCount = TableCommon.GetNumberFromShopSlotBase(mSlotTableIndex, "FREE_COUNT");
		bool isDailyFreeCard = Convert.ToBoolean(TableCommon.GetNumberFromShopSlotBase(mSlotTableIndex, "IS_DAILY_REFRESH"));

		ShopLogicData shopLogicData = DataCenter.GetData ("SHOP_DATA") as ShopLogicData;
		if(shopLogicData != null)
		{
			ShopData shopData = shopLogicData.GetShopDataByIndex (mSlotTableIndex);
			bool isFree = shopLogicData.IsFreeByIndex (mSlotTableIndex);
			if(shopData != null)
			{
				mGetFreeCardLastTime = shopData.mGetFreeCardLastTime;
			
				GameObject labelCountdownObj = GameCommon.FindObject (mCurrentGridItemObj, "label_countdown");
				CountdownUI countdownUI = labelCountdownObj.GetComponent<CountdownUI>();
				if(countdownUI != null) MonoBehaviour.Destroy (countdownUI);

				GameCommon.SetUIVisiable (mCurrentGridItemObj, "label_daily_chance", isDailyFreeCard);
				GameCommon.SetUIVisiable (mCurrentGridItemObj, "label_get_one_chance", !isDailyFreeCard);

				if(isDailyFreeCard) 
				{
					mFreeCount = iMaxFreeCount - shopData.mUsedCount;
					if(mFreeCount < 0)
					{
						DEBUG.Log ("usedCount more than maxFreeCount >>> freeCount = " + mFreeCount.ToString ());
						mFreeCount = 0;
					}
					GameCommon.SetUIText (mCurrentGridItemObj, "label_daily_chance", mFreeCount.ToString ());

					if(isFree || mFreeCount == 0)
					{
						GameCommon.SetUIVisiable (mCurrentGridItemObj, "label_daily_chance", true);
						GameCommon.SetUIVisiable (mCurrentGridItemObj, "label_countdown", false);
						GameCommon.SetUIVisiable (mCurrentGridItemObj,  !Convert.ToBoolean (mFreeCount), "icon_label","icon_sprite");
						GameCommon.SetUIVisiable (mCurrentGridItemObj, "free_label", Convert.ToBoolean (mFreeCount));
					}
					else
					{
						GameCommon.SetUIVisiable (mCurrentGridItemObj, true, "label_countdown", "icon_label","icon_sprite");
						GameCommon.SetUIVisiable (mCurrentGridItemObj, false, "label_daily_chance", "free_label");
						tWindow.SetCountdown (mCurrentGridItemObj, "label_countdown", (Int64)(iFreeIntervalTime + mGetFreeCardLastTime), 
						                      new CallBack(this, "DailyCountdownOverEvent", null));
					}
				}
				else 
				{
					if(isFree)
					{
						GameCommon.SetUIVisiable (mCurrentGridItemObj, false, "label_countdown", "icon_label","icon_sprite");
						GameCommon.SetUIVisiable (mCurrentGridItemObj, true, "label_get_one_chance", "free_label");
					}
					else
					{
						GameCommon.SetUIVisiable (mCurrentGridItemObj, true, "label_countdown", "icon_label","icon_sprite");
						GameCommon.SetUIVisiable (mCurrentGridItemObj, false, "label_get_one_chance", "free_label");
						
						tWindow.SetCountdown (mCurrentGridItemObj, "label_countdown", (Int64)(iFreeIntervalTime + mGetFreeCardLastTime), 
						                      new CallBack(this, "OnceCountdownOverEvent",  null));
					}
				}
			}
			else DEBUG.LogWarning ("not find ShopData in ShopLogicData by >>>>" + mSlotTableIndex.ToString ());
		}
		else DEBUG.LogError ("ShopLogicData is null");
	}

	public void DailyCountdownOverEvent(int count)
	{
		GameCommon.SetUIVisiable (mCurrentGridItemObj, "label_countdown", false);
		GameCommon.SetUIVisiable (mCurrentGridItemObj, "label_daily_chance", true);
		GameCommon.SetUIText (mCurrentGridItemObj, "label_daily_chance", mFreeCount.ToString ());
		GameCommon.SetUIVisiable (mCurrentGridItemObj, !Convert.ToBoolean (mFreeCount), "icon_label", "icon_sprite");
		GameCommon.SetUIVisiable (mCurrentGridItemObj, "free_label", Convert.ToBoolean (mFreeCount));
	}

	public void OnceCountdownOverEvent(int count)
	{
		GameCommon.SetUIVisiable (mCurrentGridItemObj, false, "label_countdown", "icon_label","icon_sprite");
		GameCommon.SetUIVisiable (mCurrentGridItemObj, true, "label_get_one_chance", "free_label");
	}
	
    public virtual void SetSlotTitleName()
    {
        UILabel labelName = mCurrentGridItemObj.transform.Find("group/lab_title_group/lab_title_label").gameObject.GetComponent<UILabel>();
        labelName.text = TableCommon.GetStringFromShopSlotBase(mSlotTableIndex, "NAME");
    }

    public virtual void SetSlotDescription()
    {
        UILabel labelDescription = GameCommon.FindObject(mCurrentGridItemObj, "lab_description_label").GetComponent<UILabel>();
		string str = TableCommon.GetStringFromShopSlotBase(mSlotTableIndex, "ITEM_DESCRIPTION");
		labelDescription.text = str;
	}
	
    public virtual void SetSlotFlag()
    {
        GameObject isHotSpriteObj = mCurrentGridItemObj.transform.Find("group/Info/flag_group/is_hot_sprite").gameObject;
        GameObject isNewSpriteObj = mCurrentGridItemObj.transform.Find("group/Info/flag_group/is_new_sprite").gameObject;
        GameObject isSaleSpriteObj = mCurrentGridItemObj.transform.Find("group/Info/flag_group/is_sale_sprite").gameObject;

        bool bIsHot = Convert.ToBoolean(TableCommon.GetNumberFromShopSlotBase(mSlotTableIndex, "IS_HOT"));
        bool bIsNew = Convert.ToBoolean(TableCommon.GetNumberFromShopSlotBase(mSlotTableIndex, "IS_NEW"));
        bool bIsSale = Convert.ToBoolean(TableCommon.GetNumberFromShopSlotBase(mSlotTableIndex, "IS_SALE"));

		int iNew = 0;
		int iSale = 0;
		float fSpace = 30f;
		
		if(bIsHot)
		{
			if(bIsNew)
			{
				iNew = 1;
				if(bIsSale) iSale = 2;
			}
			else
			{
				if(bIsSale) iSale = 1;
			}
		}
		else
		{
			if(bIsNew)
			{
				if(bIsSale) iSale = 1;
			}
		}

		Vector3 v = isHotSpriteObj.transform.localPosition;
		isNewSpriteObj.transform.localPosition = new Vector3(v.x, v.y + iNew * (-fSpace), v.z);
		isSaleSpriteObj.transform.localPosition = new Vector3(v.x, v.y + iSale * (-fSpace), v.z);

        isHotSpriteObj.SetActive(bIsHot);
        isNewSpriteObj.SetActive(bIsNew);
        isSaleSpriteObj.SetActive(bIsSale);
    }

    public virtual void SetSlotBuyButtonIcon()
    {
		// slot index
		UIButtonEvent buttonEvent = mCurrentGridItemObj.transform.Find("group/but_shop_buy").gameObject.GetComponent<UIButtonEvent>();
		buttonEvent.mData.set("GRID_INDEX", mGridIndex);
		buttonEvent.mData.set("SHOP_SLOT_INDEX", mSlotTableIndex);

		// number
        UILabel iconLabel = mCurrentGridItemObj.transform.Find("group/but_shop_buy/icon_label").gameObject.GetComponent<UILabel>();
        iconLabel.text = (TableCommon.GetNumberFromShopSlotBase(mSlotTableIndex, "COST_ITEM_COUNT")).ToString();

        // icon
        int iItemType = TableCommon.GetNumberFromShopSlotBase(mSlotTableIndex, "COST_ITEM_TYPE");
        DataRecord itemDataRecord = DataCenter.mItemIcon.GetRecord(iItemType);
        if (itemDataRecord != null)
        {
            UISprite iconSprite = mCurrentGridItemObj.transform.Find("group/but_shop_buy/icon_sprite").gameObject.GetComponent<UISprite>();
            string strAtlasName = itemDataRecord.get("ITEM_ATLAS_NAME").ToString ();
            string strSpriteName = itemDataRecord.get("ITEM_SPRITE_NAME").ToString ();
			GameCommon.SetIcon (iconSprite, strAtlasName, strSpriteName);
        }
        else
        {
            DEBUG.LogError("dataRecord is null");
        }
    }

	public virtual void SetSlotItemInfo(){	}

}

public class ModelShopSlot : BaseShopSlot
{
	public float mCardScale = 1.0f;
	public float mScaleRatio = 1.0f;

	public override void SetSlotItemInfo()
	{
		InitActiveBirthForUI();
	}

    public void InitActiveBirthForUI()
    {
        GameObject roleObj = mCurrentGridItemObj.transform.Find("group/Info/InfoCard/UIPoint").gameObject;
        if (roleObj != null)
        {
            ActiveBirthForUI activeBirthForUI = roleObj.GetComponent<ActiveBirthForUI>();
            if (activeBirthForUI != null)
            {
                int iChildCount = roleObj.transform.parent.childCount;
                if (iChildCount > 1)
                {
                    for (int i = 0; i < iChildCount; i++)
                    {
                        GameObject obj = roleObj.transform.parent.GetChild(i).gameObject;
                        if (obj == roleObj)
                        {
                            continue;
                        }
                        else
                        {
                            activeBirthForUI.OnDestroy();
                        }
                    }
                }
				activeBirthForUI.mBirthConfigIndex = TableCommon.GetNumberFromShopSlotBase(mSlotTableIndex, "ROLE_MODEL_INDEX");

                bool bIsCanOperate = (bool)get("SET_SELECT_PET_OPERATE_STATE");

                object val;
                bool b = getData("SET_SELECT_PET_COLLIDER_UI", out val);
                GameObject colliderObj = val as GameObject;

                activeBirthForUI.Init(bIsCanOperate, mScaleRatio * mCardScale, colliderObj);
            }
        }
    }
}

public class ItemShopSlot : BaseShopSlot
{
	public override void SetSlotItemInfo()
	{
		UISprite iconSprite = mCurrentGridItemObj.transform.Find("group/Info/item_icon").gameObject.GetComponent<UISprite>();
		string strAtlasName = TableCommon.GetStringFromShopSlotBase(mSlotTableIndex, "ITEM_ATLAS_NAME");
		string strSpriteName = TableCommon.GetStringFromShopSlotBase(mSlotTableIndex, "ITEM_SPRITE_NAME");
		GameCommon.SetIcon (iconSprite, strAtlasName, strSpriteName);
	}
}

public class PetShopSlot : ItemShopSlot
{
    public override SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.PET; }  
}

public class ToolShopSlot : ItemShopSlot
{
    public override SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.TOOL; }
}

 public class CharaterShopSlot : ModelShopSlot
{
    public float mCardScale = 1.0f;
    public float mScaleRatio = 1.0f;

    public override SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.CHARACTER; }

	public override void SetSlotDescription ()
	{
		base.SetSlotDescription ();
		GameObject uipoint = GameCommon.FindObject (mCurrentGridItemObj, "UIPoint");
		ActiveBirthForUI active = uipoint.GetComponent<ActiveBirthForUI>();
		int iShopModelIndex = active.mBirthConfigIndex;

		int iSkillID = TableCommon.GetNumberFromActiveCongfig (iShopModelIndex, "PET_SKILL_1");
		string strIconName = TableCommon.GetStringFromSkillConfig (iSkillID, "SKILL_SPRITE_NAME");
		string strSkillName = TableCommon.GetStringFromSkillConfig (iSkillID, "NAME");
		GameCommon.SetUISprite (mCurrentGridItemObj, "skill_icon", strIconName);

		GameCommon.GetButtonData (mCurrentGridItemObj, "role_skin_details_button").set ("ROLE_SKIN_INDEX", iShopModelIndex);
	}

	public override void SetSlotBuyButtonIcon()
	{
		base.SetSlotBuyButtonIcon ();
		GameObject uipoint = GameCommon.FindObject (mCurrentGridItemObj, "UIPoint");
		ActiveBirthForUI active = uipoint.GetComponent<ActiveBirthForUI>();
		int iShopModelIndex = active.mBirthConfigIndex;

		string strRoleAttack = TableCommon.GetStringFromActiveCongfig (iShopModelIndex, "BASE_ATTACK");
		string strRoleHP = TableCommon.GetStringFromActiveCongfig (iShopModelIndex, "BASE_HP");
		string strRoleMP = TableCommon.GetStringFromActiveCongfig (iShopModelIndex, "BASE_MP");
		GameCommon.SetUIText (mCurrentGridItemObj, "AttackLabel", strRoleAttack);
		GameCommon.SetUIText (mCurrentGridItemObj, "HPLabel", strRoleHP);
		GameCommon.SetUIText (mCurrentGridItemObj, "MPLabel", strRoleMP);

		int iStar = TableCommon.GetNumberFromActiveCongfig (iShopModelIndex, "STAR_LEVEL");
		GameCommon.SetUIText (mCurrentGridItemObj, "role_title_name", TableCommon.GetStringFromRoleStarLevel (iStar, "TITLE"));

        //for(int i = 1; i < 4; i++)
        //{
        //    if(i == iStar) GameCommon.SetUIVisiable (mCurrentGridItemObj, "ec_ui_ghostball_" + i.ToString () , true);
        //    else GameCommon.SetUIVisiable (mCurrentGridItemObj, "ec_ui_ghostball_" + i.ToString () , false);
        //}

		GameCommon.SetUIText (mCurrentGridItemObj, "name", TableCommon.GetStringFromActiveCongfig (iShopModelIndex, "NAME"));

        int iFightStrength = TableCommon.GetNumberFromRoleSkinConfig(iShopModelIndex, "FIGHT_STRENGTH");
        GameCommon.SetUIText (mCurrentGridItemObj, "fight_strength_Label", iFightStrength.ToString());

        ShopLogicData shopLogicData = DataCenter.GetData("SHOP_DATA") as ShopLogicData;
        ShopData shopData = shopLogicData.GetShopDataByIndex(mSlotTableIndex);
        if (shopData != null)
        {
            GameCommon.SetUIVisiable(mCurrentGridItemObj, "but_shop_buy", shopData.mUsedCount <= 0);
            GameCommon.SetUIVisiable(mCurrentGridItemObj, "but_shop_role_battle", shopData.mUsedCount > 0);

            bool bIsEnable = false;
            if (shopData.mUsedCount <= 0)
            {
                if (iShopModelIndex - iShopModelIndex / 1000 * 1000 == 0)
                {
                    bIsEnable = true;
                }
                else
                {
                    CREATE_ROLE_TYPE shopSlotCharType = GameCommon.GetCharacterTypeByModelIndex(iShopModelIndex);
                    CREATE_ROLE_TYPE curCharType = GameCommon.GetCharacterTypeByModelIndex(RoleLogicData.GetMainRole().tid);
                    if (shopSlotCharType == curCharType)
                    {
                        bIsEnable = true;
                    }
                }
            }
            else
            {
                bIsEnable = true;
            }

            GameCommon.SetUIVisiable(mCurrentGridItemObj, "but_shop_buy_disable", !bIsEnable);
            UIImageButton imageBtn = GameCommon.FindObject(mCurrentGridItemObj, "but_shop_buy").GetComponent<UIImageButton>();
            if (imageBtn != null)
            {
                imageBtn.isEnabled = bIsEnable;
            }
        }
      
	}
}


public class GoldShopSlot : ItemShopSlot
{
    public override SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.GOLD; }
}

public class DiamondShopSlot : ItemShopSlot
{
    public override SHOP_PAGE_TYPE GetPageType() { return SHOP_PAGE_TYPE.DIAMOND; }
}


public class ChouKaAnimitorEnd : CEvent
{
	ActiveObject mObject;
    GameObject mFuEffectObj;
    AudioSource mChouKaAudio;
//	tEvent mRespEvt;
//	string mWindowName;

	public Action mAction;
	public override bool DoEvent ()
	{
		mObject = getObject ("CHOU_KA_OBJ") as ActiveObject;
        mFuEffectObj = getObject("FU_EFFECT_OBJ") as GameObject;
        mChouKaAudio = getObject("CHOU_KA_AUDIO") as AudioSource;
//		mRespEvt = getObject ("GAIN_ITEM_DATA") as tEvent;
//		mWindowName = getObject ("WINDOW_NAME").ToString ();
		StartUpdate ();
		return true;
	}

	public override bool Update (float secondTime)
	{
		if(mObject.IsAnimStoped ())
		{
			mAction();
//			mObject.mMainObject.SetActive (false);
            __DestroyObj();
//			DataCenter.SetData (mWindowName, "ROLE_ANIMATOR_END", mRespEvt);
			return false;
		}
		return true;
	}

	public override bool _OnFinish ()
	{
//		mObject.mMainObject.SetActive (false);
        __DestroyObj();
		return base._OnFinish ();
	}
    private void __DestroyObj()
    {
        if (mChouKaAudio != null)
        {
            mChouKaAudio.Stop();
            mChouKaAudio.clip = null;
        }
        if (mObject != null)
        {
            mObject.Destroy();
            mObject.OnDestroy();
        }
        if (mFuEffectObj != null)
            GameObject.DestroyObject(mFuEffectObj);
    }
}

//public class FuChouKaAnimitorEnd : CEvent
//{
//	ActiveObject mFuChaouKa;
//	tEvent respEvt;
//	string mWindowName;
//	public override bool DoEvent ()
//	{
//		mFuChaouKa = getObject ("CHOU_KA") as ActiveObject;
//		respEvt = getObject ("GAIN_ITEM_DATA") as tEvent;
//		mWindowName = getObject ("WINDOW_NAME").ToString ();
//		StartUpdate ();
//		return true;
//	}
//	
//	public override bool Update (float secondTime)
//	{
//		if(mFuChaouKa.IsAnimStoped ())
//		{
//			mFuChaouKa.mMainObject.SetActive (false);
//			DataCenter.SetData (mWindowName, "FU_ANIMATOR_END", respEvt);
//			return false;
//		}
//		return true;
//	}
//
//	public override bool _OnFinish ()
//	{
//		mFuChaouKa.mMainObject.SetActive (false);
//		return base._OnFinish ();
//	}
//}


public class Button_jump_animator_button : CEvent
{
	public override bool _DoEvent()
	{
		GameObject obj = GameObject.Find ("create_scene");
		if(obj != null)
			GameCommon.SetUIVisiable (obj, "effect_background", false);

		tEvent evt = getObject ("ANI_EVT") as tEvent;
        //by chenliang
        //begin

// 		tEvent respEvt = evt.getObject ("GAIN_ITEM_DATA") as tEvent;
// 		DataCenter.SetData (getObject ("WINDOW_NAME").ToString (), "FU_ANIMATOR_END", respEvt);
//-------------------
        ShopChouKaData tmpShopData = evt.getObject("GAIN_ITEM_DATA") as ShopChouKaData;
        DataCenter.SetData(getObject("WINDOW_NAME").ToString(), "FU_ANIMATOR_END", tmpShopData);

        //end
		evt.Finish ();
		return true;
	}
}


public class GetShopFreeInfo : MonoBehaviour
{
	public GameObject obj;
	ShopLogicData data;
	float rate;
	void Start()
	{
		data = DataCenter.GetData ("SHOP_DATA") as ShopLogicData;
	}

	void Update()
	{
		if(Time.time > rate)
		{
			rate = Time.time + 1.0f;
			if(data != null)
			{
				obj.SetActive (data.GetFree ());
			}
		}
	}
}
//by chenliang
//begin

/// <summary>
/// VIP礼包预览
/// </summary>
public class Button_but_shop_preview : CEvent
{
    public override bool _DoEvent()
    {
        //TODO

        return true;
    }
}

//end

public class Button_fangdajing_cheap : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("SHOP_ALBUM_WINDOW","CHEAP");
        return true;
    }
}

public class Button_fangdajing_expensive : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.OpenWindow("SHOP_ALBUM_WINDOW","EXPENSIVE");
        return true;
    }
}