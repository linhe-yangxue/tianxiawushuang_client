using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using System;

public class PetRelation
{
	public int tid; // table id
}
public class TeamPetPackageWindow : tWindow
{
    private GameObject mCard;
    private float mfCardScale = 1.0f;
    private GameObject mMastButtonUI;
    private int mItemId = 0;
    //by chenliang
    //begin

    private List<PetData> mItemDataList;
    private UIWrapGrid mGridsContainer;

    private ItemDataBase mCurrClickItem = null;     //当前点击的Tid

    //end

    public override void Init()
    {
        base.Init();

        EventCenter.Self.RegisterEvent("Button_team_pet_bag_icon_btn", new DefineFactory<TeamPetBagIconBtnBtn>());
        EventCenter.Self.RegisterEvent("Button_pet_bag_add_button",new DefineFactory<TeamPetBagAddButtonBtn>());
    }

    protected override void OpenInit() {
        base.OpenInit();
        AddButtonAction("sale",() => {
            OpenBagObject<PetData> openObj=new OpenBagObject<PetData>();
            openObj.mBagShowType=BAG_SHOW_TYPE.SELL;
            openObj.isNeedClose=false;
            openObj.mSortCondition = (tempList) =>
            {
                return GameCommon.SortList<PetData>(tempList, GameCommon.SortPetDataByStarLevel);
            };
            openObj.mFilterCondition = (petData) =>
                {
                    // 屏蔽已上阵、已助战、寻仙中符灵，仅显示等级=1突破等级=0的符灵
                    return petData.inFairyland == 0 && petData.teamPos < 0 && petData.level == 1 && petData.breakLevel == 0;
                };
            openObj.mMultipleSelectAction=list => {
				if(list.Count == 0)
				{
                    DataCenter.OpenMessageWindow("请选择灵将!");
					return;
				}

                int money=CalculateMoney(list);
                int tid=(int)ITEM_TYPE.GOLD;

                ItemDataBase _item=new ItemDataBase();
                _item.itemNum=money;
                _item.tid=tid;
                ItemDataBase[] arr=new ItemDataBase[1] { _item };
                DataCenter.OpenTipPictureWindow(new Tuple<ItemDataBase[],Action>(arr,() => {
                    CS_Sale cs=new CS_Sale(list.Select(pet => {
                        ItemDataBase item=new ItemDataBase();
                        item.itemId=pet.itemId;
                        item.tid=pet.tid;
                        item.itemNum=pet.itemNum;
                        return item;
                    }).ToArray());
                    HttpModule.CallBack requestSuccess=text => {
                        var item=JCode.Decode<SC_Sale>(text);
                        PackageManager.UpdateItem(item.reward);
                        PackageManager.RemoveItem(list.ToArray());
                        //by chenliang
                        //begin

//                        DataCenter.SetData("TEAM_WINDOW", "SHOW_WINDOW", 2);
//-----------------------------------
                        this.set("REFRESH", 0);
                        //设置当前选择的符灵
                        GlobalModule.DoOnNextUpdate(() =>
                        {
                            GlobalModule.DoOnNextLateUpdate(() =>
                            {
                                this.set("SET_TOGGLE_BY_CURRENT_ITEM", null);
                            });
                        });

                        //end
                        //added by xuke 红点相关
                        TeamNewMarkManager.Self.CheckSalePet();
                        TeamNewMarkManager.Self.RefreshTeamNewMark();
                        //end
                        DataCenter.SetData("BAG_PET_WINDOW","OPEN",openObj);
                    };
                    HttpModule.Instace.SendGameServerMessageT(cs,requestSuccess,NetManager.RequestFail);
                }));

            };
            DataCenter.SetData("BAG_PET_WINDOW","OPEN",openObj);
        });

    }

    int CalculateMoney(List<PetData> list) {
        int totalExp=0;
        int breakStoneMoney=0;
        int skillMoney=0;
        int baseMoney=0;
        list.ForEach(pet => {
            var starLevel=TableCommon.GetNumberFromActiveCongfig(pet.tid,"STAR_LEVEL");
            string basemoney = TableCommon.GetStringFromActiveCongfig(pet.tid, "SELL_PRICE");
            String[] kind_value=basemoney.Split('#');
            baseMoney += int.Parse(kind_value[1]);
            totalExp+=(int)DataCenter.mPetLevelExpTable.GetRecord(pet.level).get("TOTAL_EXP_"+starLevel)+pet.exp;

            breakStoneMoney+=(TableCommon.GetNumberFromBreakLevelConfig(pet.breakLevel,"TOTAL_ACTIVE_NUM")*(int)DataCenter.mPetRecoverConfig.GetRecord(starLevel).get("MONEY"));

            for(int i=0;i<pet.skillLevel.Length;i++) {
                var skillLevel=pet.skillLevel[i];
                skillMoney+=TableCommon.GetNumberFromSkillCost(skillLevel,"TOTAL_MONEY_COST");
            }
        });
        return totalExp + breakStoneMoney + skillMoney + baseMoney;
    }



    public override void Open(object param)
    {
        base.Open(param);

        //by chenliang
        //begin

        if (mGridsContainer == null)
        {
            mGridsContainer = GameCommon.FindComponent<UIWrapGrid>(mGameObjUI, "grid");
            mGridsContainer.onInitializeItem += __OnRefreshItem;
        }

        //end
        if ((bool)param)
        {
            Refresh(param);
        }
        else
        {
            InitWindow();
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();
        //added by xuke 反馈文字
        ChangeTipManager.Self.ChangeGridPos(GRID_POS.TEAM_PET_PACKAGE);
        //end
    }

    public virtual void InitWindow()
    {
        mCard = GetSub("team_pet_info_card");
        InitCard();
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        if ("REFRESH_CARD" == keyIndex)
        {
            UpdateCard(objVal as PetData);
        }
        //by chenliang
        //begin

        else if(keyIndex == "SET_TOGGLE")
        {
//            //改变Disable状态下的UIToggle
//            int tmpIndex = (int)objVal;
//            for (int i = 0, count = mGridsContainer.controlList.Count; i < count; i++)
//            {
//                GridCell tmpGridCell = mGridsContainer.controlList[i];
//                UIToggle tmpToggle = GameCommon.FindComponent<UIToggle>(tmpGridCell.go, "team_pet_bag_icon_btn");
//                bool tmpCanBeNone = tmpToggle.optionCanBeNone;
//                tmpToggle.optionCanBeNone = true;
//                tmpToggle.value = (tmpIndex == i);
//                tmpToggle.optionCanBeNone = tmpCanBeNone;
//            }
            if (mGridsContainer != null)
                mGridsContainer.UpdateAllItems();
        }
        else if(keyIndex == "SET_TOGGLE_BY_ITEM")
        {
//            //改变Disable状态下的UIToggle
//            ItemDataBase tmpItem = (ItemDataBase)objVal;
//            bool tmpHas = false;
//            for (int i = 0, count = mGridsContainer.controlList.Count; i < count; i++)
//            {
//                GridCell tmpGridCell = mGridsContainer.controlList[i];
//                PetData tmpPetData = mItemDataList[i];
//                UIToggle tmpToggle = GameCommon.FindComponent<UIToggle>(tmpGridCell.go, "team_pet_bag_icon_btn");
//                bool tmpCanBeNone = tmpToggle.optionCanBeNone;
//                tmpToggle.optionCanBeNone = true;
//                //需要判断tid、itemId
//                if (tmpItem.tid == tmpPetData.tid && tmpItem.itemId == tmpPetData.itemId)
//                {
//                    tmpToggle.value = true;
//                    tmpHas = true;
//                }
//                else
//                    tmpToggle.value = false;
//                tmpToggle.optionCanBeNone = tmpCanBeNone;
//            }
            if (mGridsContainer != null)
                mGridsContainer.UpdateAllItems();
        }
        else if (keyIndex == "SET_TOGGLE_BY_CURRENT_ITEM")
        {
            set("SET_TOGGLE_BY_ITEM", mCurrClickItem);
        }
        else if (keyIndex == "SET_CURRENT_ITEM")
        {
            mCurrClickItem = (ItemDataBase)objVal;
        }
        if (mGameObjUI != null)
            GameCommon.FindObject(mGameObjUI, "fate_level").SetActive(false);
        //end
    }
    public override bool Refresh(object param)
    {
        base.Refresh(param);
        //by chenliang
        //begin

//        UpdateUI();
//-----------------
        bool tmpActiveFirst = true;
        if (param is int)
            tmpActiveFirst = (((int)param) == 1);
        UpdateUI(tmpActiveFirst);

        //end

        return true;
    }

	public override void Close ()
	{
		base.Close ();
		mItemId = 0;
	}

    //by chenliang
    //begin

//    public void UpdateUI()
//    {
//----------------
    public void UpdateUIOld()
    {
    //end
        UIGridContainer grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "grid");

        if (grid != null)
        {
            grid.MaxCount = PetLogicData.Self.mDicPetData.Count;
            mGameObjUI.SetActive(grid.MaxCount > 0);
            List<PetData> itemDataList = PetLogicData.Self.mDicPetData.Values.ToList();
            DataCenter.Set("DESCENDING_ORDER_QUALITY", true);
            itemDataList = GameCommon.SortList<PetData>(itemDataList, GameCommon.SortPetDataByTeamPosType);

			//added by xuke
			// 默认选中第一个符灵
			//if(itemDataList != null && itemDataList.Count > 0)
				//DataCenter.SetData("TEAM_PET_PACKAGE_WINDOW", "REFRESH_CARD", itemDataList[0]);
			//end

            for (int i = 0; i < grid.MaxCount; i++)
            {
                PetData itemData = itemDataList[i];
                GameObject obj = grid.controlList[i];

                if (obj != null && itemData != null)
                {
                    tNiceData data = GameCommon.GetButtonData(obj, "team_pet_bag_icon_btn");
                    if (data != null)
                    {
                        data.set("ITEM_DATA", itemData);
                    }

                    if (0 == i)
                    {
                        UpdateCard(itemData);
						GameObject _cardObj = GameCommon.FindObject(grid.controlList[0],"team_pet_bag_icon_btn");
						_cardObj.GetComponent<UIToggle>().value = true;
                    }

                    // 设置宠物头像
                    GameCommon.SetItemIcon(obj, "item_icon", itemData.tid);

                    // set element icon
                    int iElementIndex = TableCommon.GetNumberFromActiveCongfig(itemData.tid, "ELEMENT_INDEX");
                    GameCommon.SetElementIcon(obj, iElementIndex);

                    // set level
                    GameCommon.SetLevelLabel(obj, itemData.level);

                    // set star level
                    //GameCommon.SetStarLevelLabel(obj, TableCommon.GetNumberFromActiveCongfig(itemData.tid, "STAR_LEVEL"));

                    // set fate level text
                    GameCommon.SetUIText(obj, "fate_level", itemData.fateLevel.ToString());

                    // set break level text
                    GameCommon.SetUIText(obj, "break_level", GameCommon.ShowAddNumUI(itemData.breakLevel));

                    // 设置名称
                    GameCommon.SetUIText(obj, "name", TableCommon.GetStringFromActiveCongfig(itemData.tid, "NAME"));

                    // 设置宠物类型名称
                    string strPetTypeName = GameCommon.GetAttackType(itemData.tid);
                    GameCommon.SetUIText(obj, "defence_type", strPetTypeName);

                    // 是否在阵上
                    GameObject stateObj = GameCommon.FindObject(obj, "state_icon");
                    if (stateObj != null)
                    {
                        stateObj.SetActive(itemData.IsInCommonTeam());
                    }

                    // 是否在助战
                    GameObject stateRelateObj = GameCommon.FindObject(obj, "relate_icon");
                    if (stateRelateObj != null)
                    {
                        stateRelateObj.SetActive(itemData.IsInRelateTeam());
                    }

                    UILabel petNumLabel = GameCommon.FindComponent<UILabel>(mGameObjUI, "pet_num_label");
                    //by chenliang
                    //begin

//                    petNumLabel.text = grid.MaxCount + "/" + RoleLogicData.Self.mMaxPetNum;
//------------------------
                    petNumLabel.text = grid.MaxCount + "/" + VIPHelper.GetCurrVIPValueByField(VIP_CONFIG_FIELD.BAG_MAX_PET);

                    //end
                }
            }
        }
    }
    //by chenliang
    //begin

    /// <summary>
    /// 刷新元素
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    private void __OnRefreshItem(GameObject item, int wrapIndex, int index)
    {
        if (index < 0 || index >= mItemDataList.Count)
            return;
        PetData itemData = mItemDataList[index];
        GameObject obj = item;

        if (obj != null && itemData != null)
        {
            tNiceData data = GameCommon.GetButtonData(obj, "team_pet_bag_icon_btn");
            if (data != null)
            {
                data.set("INDEX", index);
                data.set("ITEM_DATA", itemData);
            }

            // 设置宠物头像
            GameCommon.SetItemIcon(obj, "item_icon", itemData.tid);

            // set element icon
            int iElementIndex = TableCommon.GetNumberFromActiveCongfig(itemData.tid, "ELEMENT_INDEX");
            GameCommon.SetElementIcon(obj, iElementIndex);

            // set level
            GameCommon.SetLevelLabel(obj, itemData.level);

            // set star level
            //GameCommon.SetStarLevelLabel(obj, TableCommon.GetNumberFromActiveCongfig(itemData.tid, "STAR_LEVEL"));

            // set fate level text
            GameCommon.SetUIText(obj, "fate_level", itemData.fateLevel.ToString());

            // set break level text
            GameCommon.SetUIText(obj, "break_level", GameCommon.ShowAddNumUI(itemData.breakLevel));

            // 设置名称
            GameCommon.SetUIText(obj, "name", TableCommon.GetStringFromActiveCongfig(itemData.tid, "NAME"));

            // 设置宠物类型名称
            string strPetTypeName = GameCommon.GetAttackType(itemData.tid);
            GameCommon.SetUIText(obj, "defence_type", strPetTypeName);

            // 是否在阵上
            GameObject stateObj = GameCommon.FindObject(obj, "state_icon");
            if (stateObj != null)
            {
                stateObj.SetActive(itemData.IsInCommonTeam());
            }

            // 是否在助战
            GameObject stateRelateObj = GameCommon.FindObject(obj, "relate_icon");
            if (stateRelateObj != null)
            {
                stateRelateObj.SetActive(itemData.IsInRelateTeam());
            }

            UILabel petNumLabel = GameCommon.FindComponent<UILabel>(mGameObjUI, "pet_num_label");
            petNumLabel.text = mItemDataList.Count + "/" + VIPHelper.GetCurrVIPValueByField(VIP_CONFIG_FIELD.BAG_MAX_PET);

            if (mCurrClickItem != null)
            {
                //加上Toggle状态
                UIToggle tmpToggle = GameCommon.FindComponent<UIToggle>(item, "team_pet_bag_icon_btn");
                bool tmpCanBeNone = tmpToggle.optionCanBeNone;
                tmpToggle.optionCanBeNone = true;
                tmpToggle.value = (mCurrClickItem.tid == itemData.tid && mCurrClickItem.itemId == itemData.itemId);
                tmpToggle.optionCanBeNone = tmpCanBeNone;
            }
        }
    }

    public void UpdateUI(bool activeFirst)
    {
        if (mGridsContainer != null)
        {
//            mGameObjUI.SetActive(PetLogicData.Self.mDicPetData.Count > 0);
            mItemDataList = PetLogicData.Self.mDicPetData.Values.ToList();
            DataCenter.Set("DESCENDING_ORDER_QUALITY", true);
            mItemDataList = GameCommon.SortList<PetData>(mItemDataList, GameCommon.SortPetDataByTeamPosType);
            mGridsContainer.maxIndex = mItemDataList.Count - 1;
            mGridsContainer.UpdateAllItems();
            int tmpCount = mItemDataList.Count;

            mGameObjUI.SetActive(tmpCount > 0);
            GameObject tipsObj = GameCommon.FindUI("Label_no_pet_fragment_tips");
			GameObject labObj = GameCommon.FindUI ("Label_no_pet_fragment_label");
            if (tipsObj == null)
                return;
			if(labObj == null )
				return;
            tipsObj.SetActive(tmpCount == 0);
            labObj.SetActive(tmpCount == 0);
            if (tmpCount == 0)
            {
                string str = DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_PET_TIPS, "STRING_CN");
                tipsObj.GetComponent<UILabel>().text = str + "哦！";
                labObj.GetComponent<UILabel>().text = "未选中灵将！";
                return;
			}

            //选中第一个符灵
            if (activeFirst && tmpCount > 0 && mItemDataList.Count > 0)
            {
                mCurrClickItem = mItemDataList[0];
                DataCenter.SetData("TEAM_PET_PACKAGE_WINDOW", "SET_TOGGLE", 0);
                UpdateCard(mItemDataList[0]);
                //added by xuke 反馈文字相关
                DataCenter.SetData("TEAM_INFO_WINDOW", "SET_SAVE_ATTRIBUTE_VALUE", mItemDataList[0]);
                //end
            }
        }

        this.DoCoroutine(__ScrollToBottom());
    }
    private IEnumerator __ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        UIScrollView tmpSV = GameCommon.FindComponent<UIScrollView>(mGameObjUI, "pet_scroll_view");
        if (tmpSV != null)
            tmpSV.RestrictWithinBounds(false);
    }

    //end

    private void UpdateCard(PetData itemData)
    {
        if (itemData != null)
        {
            if (mItemId == itemData.itemId)
                return;
            mItemId = itemData.itemId;
            GameCommon.SetCardInfo(mCard.name, itemData.tid, itemData.level, itemData.breakLevel, mMastButtonUI);

            GameObject obj = GetSub("pet_introduce");
            if (null == obj)
                return;

            GameCommon.FindObject(obj,"fate").SetActive(false);
            // 是否在阵上
            GameObject stateObj = GameCommon.FindObject(obj, "card_state_icon");
            if (stateObj != null)
            {
                stateObj.SetActive(itemData.IsInCommonTeam());
            }

            // 是否在助战
            GameObject stateRelateObj = GameCommon.FindObject(obj, "card_relate_icon");
            if (stateRelateObj != null)
            {
                stateRelateObj.SetActive(itemData.IsInRelateTeam());
            }

            // set level
            GameCommon.SetUIText(obj, "level_num", itemData.level.ToString());

            // set fate level text
            GameCommon.SetUIText(obj, "fate_num", itemData.fateLevel.ToString());

            // set break level text
            GameCommon.SetUIText(obj, "break_level", GameCommon.ShowAddNumUI(itemData.breakLevel));

            // 设置名称
            GameCommon.SetUIText(obj, "item_name", TableCommon.GetStringFromActiveCongfig(itemData.tid, "NAME"));
        }
        //added by xuke
        GlobalModule.DoCoroutine(GameCommon.IE_ChangeRenderQueue(itemData.tid,mCard, CommonParam.AureoleRenderQueue));
        //end

        TeamInfoWindow.CloseAllWindow();
        DataCenter.OpenWindow("TEAM_POS_INFO_WINDOW", itemData);
        //added by xuke 刷新符灵背包红点逻辑
        DataCenter.SetData("TEAM_POS_INFO_WINDOW", "REFRESH_BAGPET_POSINFO_NEWMARK", itemData);
        //end
    }

    private void InitCard()
    {
        GameCommon.InitCard(mCard, mfCardScale, mGameObjUI, "_pet");
    }
}


public class TeamPetBagIconBtnBtn : CEvent
{
    public override bool _DoEvent()
    {
        var iItemData = getObject("ITEM_DATA");

        DataCenter.SetData("TEAM_PET_PACKAGE_WINDOW", "REFRESH_CARD", iItemData);
        //by chenliang
        //begin

        PetData tmpPetData = iItemData as PetData;
        DataCenter.SetData("TEAM_PET_PACKAGE_WINDOW", "SET_CURRENT_ITEM", tmpPetData);
        //改变Disable状态下的UIToggle
        int tmpIndex = (int)getObject("INDEX");
        DataCenter.SetData("TEAM_PET_PACKAGE_WINDOW", "SET_TOGGLE", tmpIndex);

        DataCenter.SetData("TEAM_INFO_WINDOW", "SET_SAVE_ATTRIBUTE_VALUE_ONLY", tmpPetData);
        //end
        return true;
    }
}

public class TeamPetBagAddButtonBtn : CEvent 
{
    public override bool _DoEvent()
    {
        bool _isMaxVipLvl = false;
        _isMaxVipLvl = RoleLogicData.Self.vipLevel == VIPHelper.GetMaxVIPLevel() ? true : false;
        if(_isMaxVipLvl)
        {
            DataCenter.OpenMessageWindow(TableCommon.getStringFromStringList(STRING_INDEX.RECHAGE_EXTEND_BAG_MAX));
        }
        else
        {
            VipRechargeWindowOpenData _rechargeData = new VipRechargeWindowOpenData();
            _rechargeData.RechargeForType = RechargeForType.PET_BAG_LIMIT;
            DataCenter.OpenWindow("VIP_RECHARGE_UP_WINDOW", _rechargeData);
        }
        return base._DoEvent();
    }
}