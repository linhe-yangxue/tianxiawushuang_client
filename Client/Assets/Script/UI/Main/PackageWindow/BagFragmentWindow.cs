using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Logic;

public class CS_Sale:GameServerMessage {
    public readonly ItemDataBase[] itemArr;

    public CS_Sale(ItemDataBase[] itemArr) {
        this.itemArr=new ItemDataBase[itemArr.Length];
        itemArr.CopyTo(this.itemArr,0);
    }
}

public class SC_Sale:RespMessage {
    public readonly ItemDataBase reward;
}
public static class RefreshBoardHelper{

    public static Action<GameObject,ItemDataBase> RefreshFragmentAction=(board,item) => {
        int fragmentID=TableCommon.GetNumberFromFragment(item.tid,"ITEM_ID");
        GameCommon.SetItemIcon(board,"item_icon",fragmentID);
        GameCommon.SetUIText(board,"name",TableCommon.GetStringFromFragment(item.tid,"NAME"));
		GameCommon.FindObject (board,"name").GetComponent <UILabel >().width =180;
		GameCommon.FindObject (board,"name").GetComponent <UILabel >().transform .localPosition =new Vector3 (47,36,0);
        GameCommon.FindObject(board,"state_icon").SetActive(TeamManager.IsPetInTeamByTid(fragmentID));
        GameCommon.FindComponent<UILabel>(board,"fragment_num").text=item.itemNum.ToString();
    };

}

abstract class BagWindowBaseLHC:tWindow {
    //by chenliang
    //begin

//    UIGridContainer container;
//----------
    UIWrapGrid container;

    //end
    protected List<ItemDataBase> itemList;

    protected override void OpenInit() {
        base.OpenInit();
        //by chenliang
        //begin

//        container=GetUIGridContainer("container");
//--------------
        container = GetUIWrapGrid("container");

        //end
//		itemList=new List<ItemDataBase>();
    }
	public override void Init()
	{
		base.Init();
		EventCenter.Self.RegisterEvent("Button_fragment_pet_quality_btn", new DefineFactory<Button_fragment_pet_quality_btn>());
		EventCenter.Self.RegisterEvent("Button_fragment_equip_quality_btn", new DefineFactory<Button_fragment_equip_quality_btn>());
	}
    //by chenliang
    //begin

//	protected void bagInit(Action<GameObject,ItemDataBase> refreshAction,ItemDataBase[] itemArr) {
//        itemList=new List<ItemDataBase>();
//        container.MaxCount=itemArr.Length;
//		if(itemArr.Length ==0){
//			GameCommon.SetUIText (mGameObjUI ,"Label_no_fragment_label",DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_PET_FRAGMENT_TIPS,"STRING_CN"));
//			GameCommon.SetUIVisiable(mGameObjUI ,"Label_no_fragment_label",true);
//		}
//		else {
//			GameCommon.SetUIVisiable(mGameObjUI ,"Label_no_fragment_label",false);
//		}
//        for(int i=0;i<itemArr.Length;i++) {
//            var grid=container.controlList[i];
//            var item=itemArr[i];
//            refreshAction(grid,item);
//            grid.GetComponent<UIToggle>().value=false;
//            AddButtonAction(grid,() => {
//                if(!itemList.Exists(x => x.itemId==item.itemId))
//				{
//					itemList.Add(item);
//					SetGetMoney(itemList);
//				}                   
//                else 
//				{
//					itemList.Remove(itemList.Where(x => x.itemId==item.itemId).SingleOrDefault());
//					SetGetMoney(itemList);
//				}
//                GetUILabel("num_label").text=itemList.Count+"/"+itemArr.Length;
//            });
//            GetUILabel("num_label").text=itemList.Count+"/"+itemArr.Length;
//			SetGetMoney(itemList);
//        }
//    }
//------------------
    protected ItemDataBase[] mAllItems;
    protected Action<GameObject, ItemDataBase> mRefreshActoin;
    protected void bagInit(Action<GameObject, ItemDataBase> refreshAction, ItemDataBase[] itemArr)
    {
        bagInit2(refreshAction, itemArr);
    }
    protected void bagInit2(Action<GameObject, ItemDataBase> refreshAction, ItemDataBase[] itemArr)
    {
        mAllItems = itemArr;
        itemList = new List<ItemDataBase>();
        _UpdateBag(refreshAction, itemArr);
    }
    protected void _UpdateBag(Action<GameObject, ItemDataBase> refreshAction, ItemDataBase[] itemArr)
    {
        if (itemArr.Length == 0)
        {
            GameCommon.SetUIText(mGameObjUI, "Label_no_fragment_label", DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_PET_FRAGMENT_TIPS, "STRING_CN"));
            GameCommon.SetUIVisiable(mGameObjUI, "Label_no_fragment_label", true);
        }
        else
        {
            GameCommon.SetUIVisiable(mGameObjUI, "Label_no_fragment_label", false);
        }
        if (container != null)
        {
            mRefreshActoin = refreshAction;
            container.onInitializeItem = _OnGridUpdate;
            container.maxIndex = itemArr.Length - 1;
            container.ItemsCount = 16;
        }
        SetGetMoney(itemList);
    }
    protected void _OnGridUpdate(GameObject item, int wrapIndex, int index)
    {
        if (mAllItems == null)
            return;

        ItemDataBase tmpItemData = mAllItems[index];
        if (mRefreshActoin != null)
            mRefreshActoin(item, tmpItemData);
        item.GetComponent<UIToggle>().value = _IsItemSelected(tmpItemData);
        AddButtonAction(item, () =>
        {
            if (!itemList.Exists(x => x.itemId == tmpItemData.itemId))
            {
                itemList.Add(tmpItemData);
                SetGetMoney(itemList);
            }
            else
            {
                itemList.Remove(itemList.Where(x => x.itemId == tmpItemData.itemId).SingleOrDefault());
                SetGetMoney(itemList);
            }
            GetUILabel("num_label").text = itemList.Count + "/" + mAllItems.Length;
        });
        GetUILabel("num_label").text = itemList.Count + "/" + mAllItems.Length;
    }

    protected bool _IsItemSelected(ItemDataBase data)
    {
        if (itemList == null || itemList.Count <= 0)
            return false;
        return itemList.Exists((ItemDataBase tmpData) =>
        {
            return (tmpData.itemId == data.itemId);
        });
    }

    /// <summary>
    /// 添加指定品质元素
    /// </summary>
    /// <param name="quality"></param>
    /// <returns></returns>
    protected bool _AddSelectItemsByQuality(int quality)
    {
        int itemCount = 0;
        return _AddSelectItemsByQuality(quality, out itemCount);
    }
    /// <summary>
    /// 添加指定品质元素
    /// </summary>
    /// <param name="quality"></param>
    /// <param name="itemCount">指定品质元素个数</param>
    /// <returns></returns>
    protected bool _AddSelectItemsByQuality(int quality, out int itemCount)
    {
        itemCount = 0;
        if (mAllItems == null || mAllItems == null)
            return false;

        for (int i = 0, count = mAllItems.Length; i < count; i++)
        {
            ItemDataBase tmpItem = mAllItems[i];
            if (GameCommon.GetItemQuality(tmpItem.tid) != quality)
                continue;
            itemCount += 1;
            if (!itemList.Contains(tmpItem))
                itemList.Add(tmpItem);
        }
        return true;
    }
    /// <summary>
    /// 移除指定品质元素
    /// </summary>
    /// <param name="quality"></param>
    /// <param name="itemCount">指定品质元素个数</param>
    /// <returns></returns>
    protected bool _RemoveSelectItemsByQuality(int quality)
    {
        int itemCount = 0;
        return _RemoveSelectItemsByQuality(quality, out itemCount);
    }
    /// <summary>
    /// 移除指定品质元素
    /// </summary>
    /// <param name="quality"></param>
    /// <param name="itemCount">指定品质元素个数</param>
    /// <returns></returns>
    protected bool _RemoveSelectItemsByQuality(int quality, out int itemCount)
    {
        itemCount = 0;
        if (mAllItems == null || mAllItems == null)
            return false;

        for (int i = 0, count = mAllItems.Length; i < count; i++)
        {
            ItemDataBase tmpItem = mAllItems[i];
            if (GameCommon.GetItemQuality(tmpItem.tid) != quality)
                continue;
            itemCount += 1;
            if (itemList.Contains(tmpItem))
                itemList.Remove(tmpItem);
        }
        return true;
    }

    //end
	public void SetGetMoney(List<ItemDataBase> itemQualityList)
	{
		int total=0;
		int tid=(int)ITEM_TYPE.GOLD;

		itemQualityList.ForEach(item => {
			string sellPrice=DataCenter.mFragment.GetRecord(item.tid).getData("SELL_PRICE");
			List<ItemDataBase> list=GameCommon.ParseItemList(sellPrice);
			total+=list[0].itemNum*item.itemNum;
			tid=list[0].tid;
		});
		ItemDataBase itemDataBase=new ItemDataBase();
		itemDataBase.itemNum=total;
		itemDataBase.tid=tid;

		GameObject sellRewards = GameCommon.FindObject (mGameObjUI, "sell_rewards");
		GameCommon.SetResIcon (sellRewards, "sell_rewards_icon", itemDataBase.tid, false, true);
		GameCommon.SetUIText(sellRewards, "sell_rewards_num", "x" + itemDataBase.itemNum);
	}

    public override void OnOpen()
    {
        base.OnOpen();
        //added by xuke
        GameObject _upObj = GameCommon.FindObject(mGameObjUI, "up_group");
        GameCommon.FindComponent<UIScrollView>(_upObj, "ScrollView").ResetPosition();
        //end
    }
	public void CloseQualityMessage()
	{
		GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "chose_quality_info");
		choseQualityInfoObj.SetActive (false);
	}

	protected void SetFragmentQualityChoseBtnAction(Action action) 
	{		
		AddButtonAction("fragment_quality_chose_button", action);
	}
	protected void SetFragmentQualityOkBtnAction() 
	{
		AddButtonAction("fragment_chose_quality_ok_btn", () => GameCommon.FindObject (mGameObjUI, "fragment_chose_quality_info").gameObject.SetActive (false));
	}
	protected void SetFragmentQualityCloseBtnAction() 
	{
		AddButtonAction("fragment_chose_quality_close_btn", () => GameCommon.FindObject (mGameObjUI, "fragment_chose_quality_info").gameObject.SetActive (false));
        AddButtonAction("fragment_chose_quality_info_bg", () => GameCommon.FindObject(mGameObjUI, "fragment_chose_quality_info").gameObject.SetActive(false));
	    
    }

    protected void SetOkButtonAction(Action action) {
        AddButtonAction("okBtn",action);
    }

    protected void SetCloseButtonAction(Action action) {
        AddButtonAction("closeBtn",action);
        AddButtonAction("fragment_bag_group", action);
    }

    protected void SetTitleName(string name) {
        GetUILabel("title_label").text=name;
    }

    protected ItemDataBase GetRewardCount() {
        int total=0;
		int tid=(int)ITEM_TYPE.GOLD;
        itemList.ForEach(item => {
            string sellPrice=DataCenter.mFragment.GetRecord(item.tid).getData("SELL_PRICE");
            List<ItemDataBase> list=GameCommon.ParseItemList(sellPrice);
            total+=list[0].itemNum*item.itemNum;
            tid=list[0].tid;
        });
        ItemDataBase itemDataBase=new ItemDataBase();
        itemDataBase.itemNum=total;
        itemDataBase.tid=tid;

        return itemDataBase;
    }

	public int SellMoney(List<ItemDataBase> list) 
	{
		int baseMoney=0;
		list.ForEach(pet => {
			var starLevel=TableCommon.GetNumberFromActiveCongfig(pet.tid,"STAR_LEVEL");
			string basemoney = TableCommon.GetStringFromActiveCongfig(pet.tid, "SELL_PRICE");
			String[] kind_value=basemoney.Split('#');
			baseMoney += int.Parse(kind_value[1]);
		});
		return baseMoney;
	}
    
}

class PetFragmentBag:BagWindowBaseLHC {    
    public override void OnOpen() {
        base.OnOpen();
//		itemList=new List<ItemDataBase>();
		List<PetFragmentData> petFragmentList = PetFragmentLogicData.Self.mDicPetFragmentData.Values.ToList();
        __SortList(petFragmentList);
        ItemDataBase[] itemArr = petFragmentList.Select(x => x as ItemDataBase).ToArray();
        
        bagInit(RefreshBoardHelper.RefreshFragmentAction,itemArr);
        SetTitleName("选择碎片");
        Action okAction=()=>{
            if(itemList.Count==0) DataCenter.OpenMessageWindow("请选择碎片!!!");
            else {
                ItemDataBase[] arr=new ItemDataBase[1] { GetRewardCount() };
                DataCenter.OpenTipPictureWindow(new Tuple<ItemDataBase[],Action>(arr,() => {

                    CS_Sale cs=new CS_Sale(itemList.ToArray());
                    HttpModule.CallBack requestSuccess=text => {
                        var item=JCode.Decode<SC_Sale>(text);
                        PackageManager.UpdateItem(item.reward);
                        PackageManager.RemoveItem(itemList.ToArray());
                        DataCenter.OpenWindow(UIWindowString.pet_fragment_bag);
                        //DataCenter.SetData("TEAM_WINDOW","SHOW_WINDOW",3);
                        DataCenter.SetData("TEAM_PET_FRAGMENT_PACKAGE_WINDOW","REFRESH",null);
                        itemArr=PetFragmentLogicData.Self.mDicPetFragmentData.Values.Select(x => x as ItemDataBase).ToArray();
                        GetUILabel("num_label").text=itemList.Count+"/"+itemArr.Length;
                        //added by xuke 红点相关
                        TeamManager.CheckTeamPetFragTab_NewMark();
                        TeamNewMarkManager.Self.RefreshTeamNewMark();
                        //end
                    };
                    HttpModule.Instace.SendGameServerMessageT(cs,requestSuccess,NetManager.RequestFail);
                }));
            }
        };
        SetOkButtonAction(okAction);
        SetCloseButtonAction(() => DataCenter.CloseWindow(UIWindowString.pet_fragment_bag));

		Action FragmentQualitAction = () => 
		{
			GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "fragment_chose_quality_info");
			choseQualityInfoObj.SetActive (true);
			UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
			grid.MaxCount = 2;
			
			for(int i = 0; i < grid.MaxCount; i++)
			{
				GameObject obj = grid.controlList[i];
				UILabel qualityLabel = GameCommon.FindObject (obj, "name_label").GetComponent<UILabel>();
				qualityLabel.text = GameCommon.SetQualityName(i + 3);
				qualityLabel.color = GameCommon.SetQualityColor (i + 3);
				obj.transform.name = "fragment_pet_quality_btn";
				GameCommon.GetButtonData(obj).set("PET_FRAGMENT_QUALITY_INDEX", i + 3);
				obj.GetComponent<UIToggle>().value = false;
			}
		};
		SetFragmentQualityChoseBtnAction(FragmentQualitAction);
		SetFragmentQualityOkBtnAction();
		SetFragmentQualityCloseBtnAction();  		
    }

    /// <summary>
    /// 排序指定的碎片列表
    /// </summary>
    private void __SortList(List<PetFragmentData> petFragmentList)
    {
        if (petFragmentList == null)
            return;

        petFragmentList.Sort((PetFragmentData a, PetFragmentData b) =>
        {
            //按品质排序
            int iQualityA = TableCommon.GetNumberFromActiveCongfig(a.mComposeItemTid, "STAR_LEVEL");
            int iQualityB = TableCommon.GetNumberFromActiveCongfig(b.mComposeItemTid, "STAR_LEVEL");
            if (iQualityA > iQualityB)
                return 1;
            if (iQualityA < iQualityB)
                return -1;
            else
            {
                //如果品质相同，按tid排序
                if (a.tid < b.tid)
                    return -1;
                else if (a.tid == b.tid)
                    return 0;
                else
                    return 1;
            }
        });
    }
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
            case "ADD_SELECT_QUALITY":
                //by chenliang
                //begin

//                AddSelectQuality((int)objVal);
//-----------------------
                {
                    int tmpQuality = (int)objVal;
                    int tmpItemsCount = 0;
                    _AddSelectItemsByQuality(tmpQuality, out tmpItemsCount);
                    if (tmpItemsCount == 0)
                    {
                        DataCenter.ErrorTipsLabelMessage("您没有任何相应品质的宠物碎片");
                        GameObject choseQualityInfoObj = GameCommon.FindObject(mGameObjUI, "fragment_chose_quality_info");
                        UIGridContainer _grid = GameCommon.FindObject(choseQualityInfoObj, "grid").gameObject.GetComponent<UIGridContainer>();
                        _grid.MaxCount = 2;

                        for (int i = 0; i < _grid.MaxCount; i++)
                        {
                            GameObject obj = _grid.controlList[i];
                            UIToggle objToggle = obj.GetComponent<UIToggle>();
                            if (i + 3 == tmpQuality)
                                objToggle.value = false;
                        }
                    }
                    else
                    {
//                         UpdateItemInfosUI("sell_group", UpdateSellInfo);
//                         UpdateNumLabel();
//                         SetGetMoney();
                        _UpdateBag(RefreshBoardHelper.RefreshFragmentAction, mAllItems);
                    }
                }

                //end
                break;
            case "REMOVE_SELECT_QUALITY":
                //by chenliang
                //begin
                
//                RemoveSelectQuality((int)objVal);
//------------------
                {
                    int tmpQuality = (int)objVal;
                    int tmpItemsCount = 0;
                    _RemoveSelectItemsByQuality(tmpQuality, out tmpItemsCount);
                    if (tmpItemsCount == 0)
                    {
                        DataCenter.ErrorTipsLabelMessage("您没有任何相应品质的宠物");
                        GameObject choseQualityInfoObj = GameCommon.FindObject(mGameObjUI, "chose_quality_info");
                        UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
                        _grid.MaxCount = 3;

                        for (int i = 0; i < _grid.MaxCount; i++)
                        {
                            GameObject obj = _grid.controlList[i];
                            UIToggle objToggle = GameCommon.FindObject(obj, "pet_quality_btn").GetComponent<UIToggle>();
                            if (i + 1 == tmpQuality)
                                objToggle.value = false;
                        }
                    }
                    else
                    {
//                         UpdateItemInfosUI("sell_group", UpdateSellInfo);
//                         UpdateNumLabel();
//                         SetGetMoney();
                        _UpdateBag(RefreshBoardHelper.RefreshFragmentAction, mAllItems);
                    }
                }

                //end
                break;	
		}
	}
	void RemoveSelectQuality(int iQualityIndex) 
	{
		List<PetFragmentData> petFragmentList = PetFragmentLogicData.Self.mDicPetFragmentData.Values.ToList();
		__SortList(petFragmentList);
		ItemDataBase[] itemArr = petFragmentList.Select(x => x as ItemDataBase).ToArray();
		
		UIGridContainer container=GetUIGridContainer("container");
//		itemList=new List<ItemDataBase>();
		container.MaxCount=itemArr.Length;
		
		for(int i=0;i<itemArr.Length;i++) 
		{
			var grid=container.controlList[i];
			var item=itemArr[i];
			RefreshBoardHelper.RefreshFragmentAction(grid,item);
//			grid.GetComponent<UIToggle>().value=false;
			int iTid = (int)DataCenter.mFragment.GetRecord(item.tid).getData("ITEM_ID");
			int iStarLevel = 3;
			iStarLevel = TableCommon.GetNumberFromActiveCongfig(iTid,"STAR_LEVEL");
			if(iStarLevel == iQualityIndex)
			{
				itemList.Remove(itemList.Where(x => x.itemId==item.itemId).SingleOrDefault());
				SetGetMoney(itemList);
				grid.GetComponent<UIToggle>().value=false;
				GetUILabel("num_label").text=itemList.Count+"/"+itemArr.Length;
			}
		}
	}
	
	void AddSelectQuality(int iQualityIndex) 
	{
		List<PetFragmentData> petFragmentList = PetFragmentLogicData.Self.mDicPetFragmentData.Values.ToList();
		__SortList(petFragmentList);
		ItemDataBase[] itemArr = petFragmentList.Select(x => x as ItemDataBase).ToArray();
		
		UIGridContainer container=GetUIGridContainer("container");
		container.MaxCount=itemArr.Length;

		int iCurQualityNum = 0;
		
		for(int i=0;i<itemArr.Length;i++) 
		{
			var grid=container.controlList[i];
			var item=itemArr[i];
			RefreshBoardHelper.RefreshFragmentAction(grid,item);
//			grid.GetComponent<UIToggle>().value=false;
			int iTid = (int)DataCenter.mFragment.GetRecord(item.tid).getData("ITEM_ID");
			int iStarLevel = 3;
			iStarLevel = TableCommon.GetNumberFromActiveCongfig(iTid,"STAR_LEVEL");
			if(iStarLevel == iQualityIndex)
			{
				if(!itemList.Exists(x => x.itemId==item.itemId))
				{
					itemList.Add(item);
				}   
				SetGetMoney(itemList);
				grid.GetComponent<UIToggle>().value=true;
				
				GetUILabel("num_label").text=itemList.Count+"/"+itemArr.Length;
				iCurQualityNum++;
			}
		}
		if(iCurQualityNum == 0)
		{
			DataCenter.ErrorTipsLabelMessage ("您没有任何相应品质的宠物碎片");
			GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "fragment_chose_quality_info");
			UIGridContainer _grid = GameCommon.FindObject(choseQualityInfoObj, "grid").gameObject.GetComponent<UIGridContainer>();
			_grid.MaxCount = 2;
			
			for(int i = 0; i < _grid.MaxCount; i++)
			{
				GameObject obj = _grid.controlList[i];
				UIToggle objToggle = obj.GetComponent<UIToggle>();
				if(i + 3 == iQualityIndex)
					objToggle.value = false;
			}
		}
	}
}

class EquipFragmentBag:BagWindowBaseLHC {
    public override void OnOpen(){
        base.OnOpen();
//		itemList=new List<ItemDataBase>();
        ItemDataBase[] itemArr=RoleEquipFragmentLogicData.Self.mDicEquipFragmentData.Values.
            Select(data => data as ItemDataBase).ToArray();      
        
        bagInit(RefreshBoardHelper.RefreshFragmentAction,itemArr);
        SetTitleName("选择碎片");

        Action okAction=() => {
            if(itemList.Count==0) DataCenter.OpenMessageWindow("请选择碎片");
            else {
                ItemDataBase[] arr=new ItemDataBase[1] { GetRewardCount() };
                DataCenter.OpenTipPictureWindow(new Tuple<ItemDataBase[],Action>(arr,() => {
                    CS_Sale cs=new CS_Sale(itemList.ToArray());
                    HttpModule.CallBack requestSuccess=text => {
                        var item=JCode.Decode<SC_Sale>(text);
                        PackageManager.UpdateItem(item.reward);
                        PackageManager.RemoveItem(itemList.ToArray());
                        DataCenter.OpenWindow(UIWindowString.equip_fragment_bag);
                        DataCenter.OpenWindow("PACKAGE_EQUIP_WINDOW");
                        DataCenter.SetData("PACKAGE_EQUIP_WINDOW","SELECT_FRAGMENT_GROUP",true);
                        itemArr=RoleEquipFragmentLogicData.Self.mDicEquipFragmentData.Values.
                            Select(data => data as ItemDataBase).ToArray();
        
                        GetUILabel("num_label").text=itemList.Count+"/"+itemArr.Length;
                        //added by xuke 红点相关
                        EquipBagNewMarkManager.Self.CheckEquipFragNumLimit_NewMark();
                        EquipBagNewMarkManager.Self.CheckEquipCompose_NewMark();
                        EquipBagNewMarkManager.Self.RefreshEquipBagTabNewMark();
                        //end
                    };
                    HttpModule.Instace.SendGameServerMessageT(cs,requestSuccess,NetManager.RequestFail);
                }));
            }
        };
        SetOkButtonAction(okAction);
		SetCloseButtonAction(() => DataCenter.CloseWindow(UIWindowString.equip_fragment_bag));

		Action FragmentQualitAction = () => 
		{
			GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "fragment_chose_quality_info");
			choseQualityInfoObj.SetActive (true);
			UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
			grid.MaxCount = 2;
			
			for(int i = 0; i < grid.MaxCount; i++)
			{
				GameObject obj = grid.controlList[i];
				UILabel qualityLabel = GameCommon.FindObject (obj, "name_label").GetComponent<UILabel>();
				qualityLabel.text = GameCommon.SetQualityName(i + 3);
				qualityLabel.color = GameCommon.SetQualityColor (i + 3);
				obj.transform.name = "fragment_equip_quality_btn";
				GameCommon.GetButtonData(obj).set("EQUIP_FRAGMENT_QUALITY_INDEX", i + 3);
				obj.GetComponent<UIToggle>().value = false;
			}
		};
		SetFragmentQualityChoseBtnAction(FragmentQualitAction);
		SetFragmentQualityOkBtnAction();
		SetFragmentQualityCloseBtnAction();        
    }
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch(keyIndex)
		{
		case "ADD_SELECT_QUALITY":
                //by chenliang
                //begin

//                AddSelectQuality((int)objVal);
//-----------------------
                {
                    int tmpQuality = (int)objVal;
                    int tmpItemsCount = 0;
                    _AddSelectItemsByQuality(tmpQuality, out tmpItemsCount);
                    if (tmpItemsCount == 0)
                    {
                        DataCenter.ErrorTipsLabelMessage("您没有任何相应品质的装备碎片");
                        GameObject choseQualityInfoObj = GameCommon.FindObject(mGameObjUI, "fragment_chose_quality_info");
                        UIGridContainer _grid = GameCommon.FindObject(choseQualityInfoObj, "grid").gameObject.GetComponent<UIGridContainer>();
                        _grid.MaxCount = 2;

                        for (int i = 0; i < _grid.MaxCount; i++)
                        {
                            GameObject obj = _grid.controlList[i];
                            UIToggle objToggle = obj.GetComponent<UIToggle>();
                            if (i + 3 == tmpQuality)
                                objToggle.value = false;
                        }
                    }
                    else
                    {
//                         UpdateItemInfosUI("sell_group", UpdateSellInfo);
//                         UpdateNumLabel();
//                         SetGetMoney();
                        _UpdateBag(RefreshBoardHelper.RefreshFragmentAction, mAllItems);
                    }
                }

                //end
			break;
		case "REMOVE_SELECT_QUALITY":
            //by chenliang
            //begin

//                RemoveSelectQuality((int)objVal);
//------------------
            {
                int tmpQuality = (int)objVal;
                int tmpItemsCount = 0;
                _RemoveSelectItemsByQuality(tmpQuality, out tmpItemsCount);
                if (tmpItemsCount == 0)
                {
                    DataCenter.ErrorTipsLabelMessage("您没有任何相应品质的宠物");
                    GameObject choseQualityInfoObj = GameCommon.FindObject(mGameObjUI, "chose_quality_info");
                    UIGridContainer _grid = GameCommon.FindComponent<UIGridContainer>(choseQualityInfoObj, "grid");
                    _grid.MaxCount = 3;

                    for (int i = 0; i < _grid.MaxCount; i++)
                    {
                        GameObject obj = _grid.controlList[i];
                        UIToggle objToggle = GameCommon.FindObject(obj, "pet_quality_btn").GetComponent<UIToggle>();
                        if (i + 1 == tmpQuality)
                            objToggle.value = false;
                    }
                }
                else
                {
//                         UpdateItemInfosUI("sell_group", UpdateSellInfo);
//                         UpdateNumLabel();
//                         SetGetMoney();
                    _UpdateBag(RefreshBoardHelper.RefreshFragmentAction, mAllItems);
                }
            }

            //end
			break;	
		}
	}
	void RemoveSelectQuality(int iQualityIndex) 
	{
		ItemDataBase[] itemArr=RoleEquipFragmentLogicData.Self.mDicEquipFragmentData.Values.
			Select(data => data as ItemDataBase).ToArray();
		
		UIGridContainer container=GetUIGridContainer("container");
//		itemList=new List<ItemDataBase>();
		container.MaxCount=itemArr.Length;
		
		for(int i=0;i<itemArr.Length;i++) 
		{
			var grid=container.controlList[i];
			var item=itemArr[i];
			RefreshBoardHelper.RefreshFragmentAction(grid,item);
//			grid.GetComponent<UIToggle>().value=false;
			int iTid = (int)DataCenter.mFragment.GetRecord(item.tid).getData("ITEM_ID");
			int iStarLevel = 3;
			iStarLevel = TableCommon.GetNumberFromRoleEquipConfig(iTid,"QUALITY");
			if(iStarLevel == iQualityIndex)
			{
				itemList.Remove(itemList.Where(x => x.itemId==item.itemId).SingleOrDefault());
				SetGetMoney(itemList);
				grid.GetComponent<UIToggle>().value=false;
				GetUILabel("num_label").text=itemList.Count+"/"+itemArr.Length;
			}
		}
	}
	
	void AddSelectQuality(int iQualityIndex) 
	{
		ItemDataBase[] itemArr=RoleEquipFragmentLogicData.Self.mDicEquipFragmentData.Values.
			Select(data => data as ItemDataBase).ToArray();
		
		UIGridContainer container=GetUIGridContainer("container");
//		itemList=new List<ItemDataBase>();
		container.MaxCount=itemArr.Length;
		int iCurQualityNum = 0;
		
		for(int i=0;i<itemArr.Length;i++) 
		{
			var grid=container.controlList[i];
			var item=itemArr[i];
			RefreshBoardHelper.RefreshFragmentAction(grid,item);
//			grid.GetComponent<UIToggle>().value=false;
			int iTid = (int)DataCenter.mFragment.GetRecord(item.tid).getData("ITEM_ID");
			int iStarLevel = 3;
			iStarLevel = TableCommon.GetNumberFromRoleEquipConfig(iTid,"QUALITY");
			if(iStarLevel == iQualityIndex)
			{
				if(!itemList.Exists(x => x.itemId==item.itemId))
				{
					itemList.Add(item);
				}   
				SetGetMoney(itemList);
				grid.GetComponent<UIToggle>().value=true;
				
				GetUILabel("num_label").text=itemList.Count+"/"+itemArr.Length;
				iCurQualityNum++;
			}
		}
		if(iCurQualityNum == 0)
		{
			DataCenter.ErrorTipsLabelMessage ("您没有任何相应品质的装备碎片");
			GameObject choseQualityInfoObj = GameCommon.FindObject (mGameObjUI, "fragment_chose_quality_info");
			UIGridContainer _grid = GameCommon.FindObject(choseQualityInfoObj, "grid").gameObject.GetComponent<UIGridContainer>();
			_grid.MaxCount = 2;
			
			for(int i = 0; i < _grid.MaxCount; i++)
			{
				GameObject obj = _grid.controlList[i];
				UIToggle objToggle = obj.GetComponent<UIToggle>();
				if(i + 3 == iQualityIndex)
					objToggle.value = false;
			}
		}
	}
}

public class Button_fragment_chose_quality_close_btn : CEvent
{
	public override bool _DoEvent()
	{
		BagWindowBaseLHC _BagWindowBaseLHC = DataCenter.GetData("ROLE_SEL_TOP_RIGHT_GROUP") as BagWindowBaseLHC;
		_BagWindowBaseLHC.CloseQualityMessage();
		return true;
	}
}
public class Button_fragment_equip_quality_btn : CEvent
{
	public override bool DoEvent ()
	{
		int equipQualityIndex = get ("EQUIP_FRAGMENT_QUALITY_INDEX");
		object val;
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		if (toggle.value)
		{
			DataCenter.SetData("EQUIP_FRAGMENT_BAG", "ADD_SELECT_QUALITY", equipQualityIndex);
		}else
		{
			DataCenter.SetData("EQUIP_FRAGMENT_BAG", "REMOVE_SELECT_QUALITY", equipQualityIndex);		
		}
		return true;
	}
}
public class Button_fragment_pet_quality_btn : CEvent
{
	public override bool DoEvent ()
	{
		int equipQualityIndex = get ("PET_FRAGMENT_QUALITY_INDEX");
		object val;
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		if (toggle.value)
		{
			DataCenter.SetData("PET_FRAGMENT_BAG", "ADD_SELECT_QUALITY", equipQualityIndex);
		}else
		{
			DataCenter.SetData("PET_FRAGMENT_BAG", "REMOVE_SELECT_QUALITY", equipQualityIndex);		
		}
		return true;
	}
}
