using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PackageEquipWindow : tWindow
{
    private enum PACKAGE_PAGE
    {
        NONE,
        EQUIP,
        FRAMENT,
        MAGIC
    }

    private bool mIsEquipGroupDirty = true;
    private bool mIsFragmentGroupDirty = true;
    private bool mIsMagicGroupDirty = true;
    private int mEquipCount = 0;
    private int mFragmentCount = 0;
    private int mMagicCount = 0;
    private PACKAGE_PAGE mCurPage = PACKAGE_PAGE.NONE;
    private int mSelectedItemId=-1;
 
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_select_equip_btn", new DefineFactory<Button_select_equip_btn>());
        EventCenter.Self.RegisterEvent("Button_select_fragment_btn", new DefineFactory<Button_select_fragment_btn>());
        EventCenter.Self.RegisterEvent("Button_select_magic_btn", new DefineFactory<Button_select_magic_btn>());
        EventCenter.Self.RegisterEvent("Button_package_equip_add_button", new DefineFactory<TeamPetBagAddButtonBtn>());  //和增加符灵背包上限操作一致
    }

    public override void Open(object param)
    {
        base.Open(param);
        DataCenter.OpenBackWindow("PACKAGE_EQUIP_WINDOW", "a_ui_zhuangbei_logo", () => MainUIScript.Self.ShowMainBGUI(), 120);
        mIsEquipGroupDirty = true;
        mIsFragmentGroupDirty = true;
        mIsMagicGroupDirty = true;
        mEquipCount = 0;
        mFragmentCount = 0;
        mMagicCount = 0;
        mCurPage = PACKAGE_PAGE.NONE;
        //set "from" mark
        GameCommon.SetDataByZoneUid("IS_FROM_TEAM_INFO", "0");

        if (param != null && param is bool)
        {
            GameCommon.ToggleButton(mGameObjUI, "select_equip_btn");
        }
        else if (param != null && param is int)
        {
            switch ((int)param)
            {
                case 1:
                    GameCommon.ToggleButton(mGameObjUI, "select_equip_btn");
                    RefreshEquipGroup();
                    break;
                case 2:
                    GameCommon.ToggleButton(mGameObjUI, "select_fragment_btn");
                    RefreshFragmentGroup();
                    break;
                case 3:
                    GameCommon.ToggleButton(mGameObjUI, "select_magic_btn");
                    RefreshMagicGroup();
                    break;
                default:
                    DEBUG.LogError("No Such Tab");
                    break;
            }
        }

        //added by xuke 红点相关
        EquipBagNewMarkManager.Self.CheckEquipBagInfoAll_NewMark();
        EquipBagNewMarkManager.Self.RefreshEquipBagTabNewMark();
        //end
    }


    public override void OnClose()
    {
        TeamInfoWindow.CloseAllWindow();
        DataCenter.CloseWindow(UIWindowString.common_back);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);

        GameObject equip_window = GameCommon.FindUI("package_equip_window");
        switch (keyIndex)
        {
            case "SELECT_EQUIP_GROUP":
                GameCommon.SetUIText(equip_window, "title_label", "装备详情");
                GetUIToggle("select_equip_btn").value = true;
                GameCommon.FindComponent<UIScrollView>(GetSub("equip_group"), "ScrollView").ResetPosition();
                AddButtonAction("sale", () =>
                {
                    if (RoleEquipLogicData.Self.mDicEquip.Count == 0) DataCenter.OpenMessageWindow("没有装备");
                    else
                    {
                        OpenBagObject<EquipData> openObj = new OpenBagObject<EquipData>();
                        openObj.mBagShowType = BAG_SHOW_TYPE.SELL;
                        openObj.isNeedClose = false;
                        openObj.mFilterCondition = equip => equip.teamPos == -1 && equip.strengthenLevel == 1 && equip.refineLevel == 0;
                        openObj.mSortCondition = tempList =>
                        {
                            DataCenter.Set("DESCENDING_ORDER", false);
                            return GameCommon.SortList<EquipData>(tempList, GameCommon.SortEquipDataByStarLevel);
                        };
                        openObj.mMultipleSelectAction = list =>
                        {
                            if (list.Count == 0)
                            {
                                DataCenter.ErrorTipsLabelMessage("请选择要出售的装备");
                                return;
                            }

                            int total = 0;
                            int tid = 0;
                            list.ForEach(equip =>
                            {
                                string sellPrice = (string)DataCenter.mRoleEquipConfig.GetRecord(equip.tid).getData("SELL_PRICE");
                                var itemData = GameCommon.ParseItemList(sellPrice)[0];
                                tid = itemData.tid;
                                total += equip.strengCostGold + itemData.itemNum;
                            });

                            ItemDataBase _item = new ItemDataBase();
                            _item.itemNum = total;
                            _item.tid = tid;
                            ItemDataBase[] arr = new ItemDataBase[1] { _item };
                            DataCenter.OpenTipPictureWindow(new Tuple<ItemDataBase[], Action>(arr, () =>
                            {
                                CS_Sale cs = new CS_Sale(list.Select(equip =>
                                {
                                    ItemDataBase item = new ItemDataBase();
                                    item.itemId = equip.itemId;
                                    item.tid = equip.tid;
                                    item.itemNum = equip.itemNum;
                                    return item;
                                }).ToArray());
                                HttpModule.CallBack requestSuccess = text =>
                                {
                                    var item = JCode.Decode<SC_Sale>(text);
                                    PackageManager.UpdateItem(item.reward);
                                    PackageManager.RemoveItem(list.ToArray());
                                    DataCenter.OpenWindow("PACKAGE_EQUIP_WINDOW");
                                    DataCenter.SetData("PACKAGE_EQUIP_WINDOW", "SELECT_EQUIP_GROUP", true);
                                    DataCenter.SetData("BAG_EQUIP_WINDOW", "OPEN", openObj);

                                    //added by xuke 红点相关
                                    EquipBagNewMarkManager.Self.CheckEquipNumLimit_NewMark();
                                    EquipBagNewMarkManager.Self.RefreshEquipBagTabNewMark();
                                    //end
                                };
                                HttpModule.Instace.SendGameServerMessageT(cs, requestSuccess, NetManager.RequestFail);
                            }));


                        };
                        DataCenter.SetData("BAG_EQUIP_WINDOW", "OPEN", openObj);
                    }
                });
                OnSelectEquipGroup();
                //tips
                showTips((int)STRING_INDEX.ERROR_ROLE_EQUIP_NO_EQUIP, (int)STRING_INDEX.ERROR_ROLE_EQUIP_NO_EQUIP_DATA, mEquipCount > 0 ? false : true);
                break;

            case "SELECT_FRAGMENT_GROUP":
                GameCommon.SetUIText(equip_window, "title_label", "装备详情");
                GetUIToggle("select_fragment_btn").value = true;
                GameCommon.FindComponent<UIScrollView>(GetSub("fragment_group"), "ScrollView").ResetPosition();
                AddButtonAction("sale", () =>
                {
                    if (RoleEquipFragmentLogicData.Self.mDicEquipFragmentData.Count == 0) DataCenter.OpenMessageWindow("没有装备碎片");
                    else DataCenter.OpenWindow(UIWindowString.equip_fragment_bag);
                });
                OnSelectFragmentGroup();
                //tips
                showTips((int)STRING_INDEX.ERROR_ROLE_EQUIP_NO_FRAGMENT, (int)STRING_INDEX.ERROR_ROLE_EQUIP_NO_FRAGMENT_DATA, mFragmentCount > 0 ? false : true);
                break;
            case "COMPOSE_FRAGMENT_BTN":
                GetUIToggle("select_fragment_btn").value = true;
                AddButtonAction("sale", () =>
                {
                    if (RoleEquipFragmentLogicData.Self.mDicEquipFragmentData.Count == 0) DataCenter.OpenMessageWindow("没有装备碎片");
                    else DataCenter.OpenWindow(UIWindowString.equip_fragment_bag);
                });
                RefreshFragmentGroupWithComposeBtn();
                //tips
                showTips((int)STRING_INDEX.ERROR_ROLE_EQUIP_NO_FRAGMENT, (int)STRING_INDEX.ERROR_ROLE_EQUIP_NO_FRAGMENT_DATA, mFragmentCount > 0 ? false : true);
                break;
            case "SELECT_MAGIC_GROUP":
                GameCommon.SetUIText(equip_window, "title_label", "神器详情");

                GetUIToggle("select_magic_btn").value = true;

                AddButtonAction("sale", () =>
                {
                    if (MagicLogicData.Self.mDicEquip.Count == 0) DataCenter.OpenMessageWindow("没有神器");
                    else
                    {
                        OpenBagObject<EquipData> openObj = new OpenBagObject<EquipData>();
                        openObj.mBagShowType = BAG_SHOW_TYPE.SELL;
                        openObj.isNeedClose = false;
                        // 屏蔽已穿戴的装备，仅显示强化等级=1；精炼等级=0的装备
                        openObj.mFilterCondition = equip => equip.teamPos == -1 && equip.strengthenLevel == 1 && equip.refineLevel == 0;
                        openObj.mSortCondition = tempList =>
                        {
                            DataCenter.Set("DESCENDING_ORDER", false);
                            return GameCommon.SortList<EquipData>(tempList, GameCommon.SortEquipDataByStarLevel);
                        };
                        openObj.mMultipleSelectAction = list =>
                        {
                            if (list.Count == 0)
                            {
                                DataCenter.OpenMessageWindow("请选择神器!");
                                return;
                            }
                            int total = 0;
                            int tid = 0;
                            list.ForEach(equip =>
                            {
                                string sellPrice = (string)DataCenter.mRoleEquipConfig.GetRecord(equip.tid).getData("SELL_PRICE");
                                var itemData = GameCommon.ParseItemList(sellPrice)[0];
                                tid = itemData.tid;
                                total += DataCenter.mMagicEquipRefineConfig.GetRecord(equip.refineLevel).get("TOTAL_REFINE_EQUIP_MONEY");
                                int qualityLevel = TableCommon.GetNumberFromRoleEquipConfig(equip.tid, "QUALITY");
                                total += DataCenter.mMagicEquipLvConfig.GetRecord(equip.strengthenLevel).get("TOTAL_EXP_" + qualityLevel) + equip.strengthenExp + itemData.itemNum;
                            });

                            ItemDataBase _item = new ItemDataBase();
                            _item.itemNum = total;
                            _item.tid = tid;
                            ItemDataBase[] arr = new ItemDataBase[1] { _item };

                            DataCenter.OpenTipPictureWindow(new Tuple<ItemDataBase[], Action>(arr, () =>
                            {
                                CS_Sale cs = new CS_Sale(list.Select(equip =>
                                {
                                    ItemDataBase item = new ItemDataBase();
                                    item.itemId = equip.itemId;
                                    item.tid = equip.tid;
                                    item.itemNum = equip.itemNum;
                                    return item;
                                }).ToArray());
                                HttpModule.CallBack requestSuccess = text =>
                                {
                                    var item = JCode.Decode<SC_Sale>(text);
                                    PackageManager.UpdateItem(item.reward);
                                    PackageManager.RemoveItem(list.ToArray());
                                    //DataCenter.OpenWindow("PACKAGE_EQUIP_WINDOW");
                                    //DataCenter.SetData("PACKAGE_EQUIP_WINDOW","SELECT_EQUIP_GROUP",true);
                                    DataCenter.SetData("BAG_MAGIC_WINDOW", "OPEN", openObj);
                                    DataCenter.SetData("PACKAGE_EQUIP_WINDOW", "REFRESH_MAGIC_BAG_GROUP", true);

                                    //added by xuke 红点相关
                                    EquipBagNewMarkManager.Self.CheckMagicNumLimit_NewMark();
                                    EquipBagNewMarkManager.Self.RefreshEquipBagTabNewMark();
                                    //end
                                };
                                HttpModule.Instace.SendGameServerMessageT(cs, requestSuccess, NetManager.RequestFail);
                            }));
                        };
                        DataCenter.SetData("BAG_MAGIC_WINDOW", "OPEN", openObj);
                    }
                });
                OnSelectMagicGroup();
                //tips
                showTips((int)STRING_INDEX.ERROR_MAGIC_NO_MAGIC, (int)STRING_INDEX.ERROR_MAGIC_NO_MGAIC_DATA, mMagicCount > 0 ? false : true);
                break;
            case "REFRESH_MAGIC_BAG_GROUP":
                RefreshMagicGroup();
                mIsMagicGroupDirty = false;
                SetCount(mMagicCount, PackageManager.GetMaxMagicPackageNum());
                break;
            case "REFRESH_EQUIP_BAG_GROUP":
                if (!(DataCenter.GetData("PACKAGE_EQUIP_WINDOW") as tWindow).IsOpen())
                    return;
                RefreshEquipGroup();
                mIsEquipGroupDirty = false;
                SetCount(mEquipCount, PackageManager.GetMaxEquipPackageNum());
                break;
            case "REFRESH_EQUIP_BAG_NEWMARK":
                RefreshEquipBagNewMark();
                break;
        }
    }

    #region 红点相关
    private void RefreshEquipBagNewMark() 
    {
        GameObject _equipTabObj = GameCommon.FindObject(mGameObjUI, "select_equip_btn");
        GameObject _equipFragTabObj = GameCommon.FindObject(mGameObjUI, "select_fragment_btn");
        GameObject _magicTabObj = GameCommon.FindObject(mGameObjUI, "select_magic_btn");

        GameCommon.SetNewMarkVisible(_equipTabObj, EquipBagNewMarkManager.Self.EquipTabVisible);
        GameCommon.SetNewMarkVisible(_equipFragTabObj,EquipBagNewMarkManager.Self.EquipFragTabVisible);
        GameCommon.SetNewMarkVisible(_magicTabObj, EquipBagNewMarkManager.Self.MagicTabVisible);
    }
    #endregion
    public void showTips(int leftIndex, int rightIndex, bool visible)
    {
        SetText("Label_left_tips", DataCenter.mStringList.GetData(leftIndex, "STRING_CN"));
        SetVisible("Label_left_tips", visible);

        SetText("Label_right_tips", DataCenter.mStringList.GetData(rightIndex, "STRING_CN"));
        SetVisible("Label_right_tips", visible);
    }

    private void OnSelectEquipGroup()
    {
        if (mCurPage == PACKAGE_PAGE.EQUIP)
            return;

        //if (mIsEquipGroupDirty)
        ///{
        RefreshEquipGroup();
        mIsEquipGroupDirty = false;
        //}

        //by chenliang
        //begin

        //        SetCount(mEquipCount, 200);
//        TogglePage("select_equip_btn");
        //--------------
        SetCount(mEquipCount, PackageManager.GetMaxEquipPackageNum());
        GlobalModule.DoOnNextUpdate(2, () =>
        {
            TogglePage("select_equip_btn");
            mCurPage = PACKAGE_PAGE.EQUIP;
        });

        //end
        //added by xuke
        tWindow _tWin = DataCenter.GetData("TEAM_DATA_WINDOW") as tWindow;
        if (_tWin == null || _tWin.mGameObjUI == null)
            return;
        _tWin.mGameObjUI.transform.parent = GetSub("transform_root_equip_package").transform;
        _tWin.mGameObjUI.transform.localPosition = Vector3.zero;
        //end
    }

    private void OnSelectFragmentGroup()
    {
        if (mCurPage == PACKAGE_PAGE.FRAMENT)
            return;

        //if (mIsFragmentGroupDirty)
        //{
        RefreshFragmentGroup();
        mIsFragmentGroupDirty = false;
        //}

        SetCount(mFragmentCount, 200);
        TogglePage("select_fragment_btn");
        mCurPage = PACKAGE_PAGE.FRAMENT;

    }

    //点击合成按钮刷新装备碎片背包
    private void RefreshFragmentGroupWithComposeBtn()
    {
        RefreshFragmentGroup();
        mIsFragmentGroupDirty = false;
        SetCount(mFragmentCount, 200);
        TogglePage("select_fragment_btn");
        mCurPage = PACKAGE_PAGE.FRAMENT;
    }

    private void OnSelectMagicGroup()
    {
        if (mCurPage == PACKAGE_PAGE.MAGIC)
            return;

        //if (mIsMagicGroupDirty)
        //{
        RefreshMagicGroup();
        mIsMagicGroupDirty = false;
        //}

        //by chenliang
        //begin

//        SetCount(mMagicCount, 200);
//        TogglePage("select_magic_btn");
        //-----------------
        SetCount(mMagicCount, PackageManager.GetMaxMagicPackageNum());
        GlobalModule.DoOnNextUpdate(2, () =>
        {
            TogglePage("select_magic_btn");
            mCurPage = PACKAGE_PAGE.MAGIC;
        });

        //end
    }

    private void RefreshEquipGroup()
    {
        //by chenliang
        //begin

        __RefreshEquipGroup();
        return;

        //end
        List<EquipData> equipList = new List<EquipData>(RoleEquipLogicData.Self.mDicEquip.Values);
        equipList.RemoveAll(x => x.itemNum <= 0);
        equipList.Sort(EquipDataComparison);

        mEquipCount = equipList.Count;

        UIToggledObjects uiToggled = GetComponent<UIToggledObjects>("select_equip_btn");
        GameObject toggledView = uiToggled.activate[0];
        UIGridContainer container = GameCommon.FindComponent<UIGridContainer>(toggledView, "grid");
        container.MaxCount = equipList.Count;

        for (int i = 0; i < container.MaxCount; ++i)
        {
            RefreshEquipCell(container.controlList[i], equipList[i]);
        }


    }

    private void RefreshFragmentGroup()
    {
        //by chenliang
        //begin

        __RefreshFragmentGroup();
        return;

        //end
        List<EquipFragmentData> fragmentList = new List<EquipFragmentData>(RoleEquipFragmentLogicData.Self.mDicEquipFragmentData.Values);
        fragmentList.RemoveAll(x => x.itemNum <= 0);
        fragmentList.Sort(EquipFragmentDataComparison);
        mFragmentCount = fragmentList.Count;

        UIToggledObjects uiToggled = GetComponent<UIToggledObjects>("select_fragment_btn");
        GameObject toggledView = uiToggled.activate[0];
        UIGridContainer container = GameCommon.FindComponent<UIGridContainer>(toggledView, "grid");
        container.MaxCount = fragmentList.Count;

        for (int i = 0; i < container.MaxCount; ++i)
        {
            RefreshFramentCell(container.controlList[i], fragmentList[i]);
        }

    }

    private void RefreshMagicGroup()
    {
        //by chenliang
        //begin

        __RefreshMagicGroup();
        return;

        //end
        List<EquipData> magicList = new List<EquipData>(MagicLogicData.Self.mDicEquip.Values);
        magicList.RemoveAll(x => x.itemNum <= 0);
        magicList.Sort(EquipDataComparison);

        mMagicCount = magicList.Count;

        UIToggledObjects uiToggled = GetComponent<UIToggledObjects>("select_magic_btn");
        GameObject toggledView = uiToggled.activate[0];
        UIGridContainer container = GameCommon.FindComponent<UIGridContainer>(toggledView, "grid");
        container.MaxCount = magicList.Count;

        for (int i = 0; i < container.MaxCount; ++i)
        {
            RefreshMagicCell(container.controlList[i], magicList[i]);
        }
        GameObject _magicGroupObj = GameCommon.FindObject(mGameObjUI, "magic_group");
        UIScrollView _scrollView = GameCommon.FindComponent<UIScrollView>(_magicGroupObj, "ScrollView");
        _scrollView.ResetPosition();
    }
    //by chenliang
    //begin

    List<EquipData> mEquipList;
    private void __RefreshEquipGroup()
    {
        mEquipList = new List<EquipData>(RoleEquipLogicData.Self.mDicEquip.Values);
        mEquipList.RemoveAll(x => x.itemNum <= 0);
        mEquipList.Sort(EquipDataComparison);

        mEquipCount = mEquipList.Count;

        UIToggledObjects uiToggled = GetComponent<UIToggledObjects>("select_equip_btn");
        GameObject toggledView = uiToggled.activate[0];

        UIWrapGrid container = GameCommon.FindComponent<UIWrapGrid>(toggledView, "grid");
        container.onInitializeItem = __OnEquipGridUpdate;
        container.maxIndex = mEquipCount - 1;
        container.ItemsCount = 16;
    }
    private void __OnEquipGridUpdate(GameObject item, int wrapIndex, int index)
    {
        RefreshEquipCell(item, mEquipList[index]);
    }

    List<EquipFragmentData> mFragmentList;
    private void __RefreshFragmentGroup()
    {
        mFragmentList = new List<EquipFragmentData>(RoleEquipFragmentLogicData.Self.mDicEquipFragmentData.Values);
        mFragmentList.RemoveAll(x => x.itemNum <= 0);
        mFragmentList.Sort(EquipFragmentDataComparison);
        mFragmentCount = mFragmentList.Count;

        UIToggledObjects uiToggled = GetComponent<UIToggledObjects>("select_fragment_btn");
        GameObject toggledView = uiToggled.activate[0];

        UIWrapGrid container = GameCommon.FindComponent<UIWrapGrid>(toggledView, "grid");
        container.onInitializeItem = __OnFragmentGridUpdate;
        container.maxIndex = mFragmentCount - 1;
        container.ItemsCount = 16;
    }
    private void __OnFragmentGridUpdate(GameObject item, int wrapIndex, int index)
    {
        RefreshFramentCell(item, mFragmentList[index]);
    }

    List<EquipData> mMagicList;
    private void __RefreshMagicGroup()
    {
        mMagicList = new List<EquipData>(MagicLogicData.Self.mDicEquip.Values);
        mMagicList.RemoveAll(x => x.itemNum <= 0);
        mMagicList.Sort(EquipDataComparison);

        mMagicCount = mMagicList.Count;

        UIToggledObjects uiToggled = GetComponent<UIToggledObjects>("select_magic_btn");
        GameObject toggledView = uiToggled.activate[0];

        UIWrapGrid container = GameCommon.FindComponent<UIWrapGrid>(toggledView, "grid");
        container.onInitializeItem = __OnMagicGridUpdate;
        container.maxIndex = mMagicCount - 1;
        container.ItemsCount = 16;

//         GlobalModule.DoOnNextLateUpdate(2, () =>
//         {
//             GameObject _magicGroupObj = GameCommon.FindObject(mGameObjUI, "magic_group");
//             UIScrollView _scrollView = GameCommon.FindComponent<UIScrollView>(_magicGroupObj, "ScrollView");
//             _scrollView.ResetPosition();
//         });
    }
    private void __OnMagicGridUpdate(GameObject item, int wrapIndex, int index)
    {
        RefreshMagicCell(item, mMagicList[index]);
    }

    //end

    private void RefreshEquipCell(GameObject cell, EquipData data)
    {
        UIButtonEvent evt = GameCommon.FindComponent<UIButtonEvent>(cell, "package_equip_item_btn");
        evt.AddAction(() => { OnSelectEquipCell(cell, data); });
        
        GameCommon.SetItemIcon(cell, data);
        GameObject target = GameCommon.FindObject(cell, "package_equip_item_btn");
        UIToggle uiToggle = target.GetComponent<UIToggle>();
        if (uiToggle != null)
        {
            bool tmpCanBeNone = uiToggle.optionCanBeNone;
            uiToggle.optionCanBeNone = true;                //为了主动设置value有效
            uiToggle.value = (data.itemId == mSelectedItemId);
            uiToggle.optionCanBeNone = tmpCanBeNone;
        }
        string name = GameCommon.GetItemStringField(data.tid, GET_ITEM_FIELD_TYPE.NAME);
        GameCommon.SetUIText(cell, "name_label", name);
        GameCommon.FindObject(cell, "num_label", "num").GetComponent<UILabel>().text = data.strengthenLevel.ToString();
        GameCommon.FindObject(cell, "refine_label", "num").GetComponent<UILabel>().text = data.refineLevel.ToString();
        GameCommon.SetUIVisiable(cell, "name", data.teamPos >= 0);
        GameCommon.FindObject(cell, "name_label").GetComponent<UILabel>().color = GameCommon.GetNameColor(data.tid);
        GameCommon.FindObject(cell, "name_label").GetComponent<UILabel>().effectColor = GameCommon.GetNameEffectColor();

        if (data.teamPos >= 0)
        {
            ActiveData activeData = TeamManager.GetActiveDataByTeamPos(data.teamPos);
            string owner = GameCommon.GetItemStringField(activeData.tid, GET_ITEM_FIELD_TYPE.NAME);
            GameCommon.FindObject(cell, "name").GetComponent<UILabel>().text = owner;
        }
    }

    private void RefreshFramentCell(GameObject cell, EquipFragmentData data)
    {
        UIButtonEvent evt = GameCommon.FindComponent<UIButtonEvent>(cell, "package_equip_item_btn");
        evt.AddAction(() => OnSelectFragmentCell(cell, data));
        GameCommon.SetItemIcon(cell, data);
        string name = GameCommon.GetItemStringField(data.tid, GET_ITEM_FIELD_TYPE.NAME);
        GameCommon.SetUIText(cell, "name_label", name);
        GameCommon.FindObject(cell, "name_label").GetComponent<UILabel>().width = 160;
        int needNum = TableCommon.GetNumberFromFragment(data.tid, "COST_NUM");
        GameCommon.SetUIVisiable(cell, "tips_label", data.itemNum < needNum);
        GameObject lack = GameCommon.FindObject(cell, "lack");
        GameObject full = GameCommon.FindObject(cell, "full");

        if (data.itemNum < needNum)
        {
            lack.SetActive(true);
            full.SetActive(false);
            GameCommon.SetUIText(lack, "num01", data.itemNum.ToString());
            GameCommon.SetUIText(lack, "num03", needNum.ToString());
        }
        else
        {
            lack.SetActive(false);
            full.SetActive(true);
            GameCommon.SetUIText(full, "num01", data.itemNum.ToString());
            GameCommon.SetUIText(full, "num03", needNum.ToString());
        }
    }

    private void RefreshMagicCell(GameObject cell, EquipData data)
    {
        UIButtonEvent evt = GameCommon.FindComponent<UIButtonEvent>(cell, "package_equip_item_btn");
        evt.AddAction(() => OnSelectMagicCell(cell, data));
        GameCommon.SetItemIcon(cell, data);
        GameObject target = GameCommon.FindObject(cell, "package_equip_item_btn");
        UIToggle uiToggle = target.GetComponent<UIToggle>();
        if (uiToggle != null)
        {
            bool tmpCanBeNone = uiToggle.optionCanBeNone;
            uiToggle.optionCanBeNone = true;                //为了主动设置value有效
            uiToggle.value = (data.itemId == mSelectedItemId);
            uiToggle.optionCanBeNone = tmpCanBeNone;
        }
        string name = GameCommon.GetItemStringField(data.tid, GET_ITEM_FIELD_TYPE.NAME);
        GameCommon.SetUIText(cell, "name_label", name);
        GameCommon.FindObject(cell, "num_label", "num").GetComponent<UILabel>().text = data.strengthenLevel.ToString();
        GameCommon.FindObject(cell, "refine_label", "num").GetComponent<UILabel>().text = data.refineLevel.ToString();
        GameCommon.SetUIVisiable(cell, "name", data.teamPos >= 0);

        if (data.teamPos >= 0)
        {
            ActiveData activeData = TeamManager.GetActiveDataByTeamPos(data.teamPos);
            string owner = GameCommon.GetItemStringField(activeData.tid, GET_ITEM_FIELD_TYPE.NAME);
            GameCommon.FindObject(cell, "name").GetComponent<UILabel>().text = owner;
        }
    }

    private void TogglePage(string btnName)
    {
        GameObject toggledView = GameCommon.GetToggledObject(mGameObjUI, btnName);
        //by chenliang
        //begin

//         UIGridContainer container = GameCommon.FindComponent<UIGridContainer>(toggledView, "grid");
// 
//         if (container.MaxCount > 0)
//         {
//             GameCommon.ToggleButton(container.controlList[0], "package_equip_item_btn");
//         }
//         else
//         {
//             TeamInfoWindow.CloseAllWindow();
//         }
//---------------------
        UIWrapGrid container = GameCommon.FindComponent<UIWrapGrid>(toggledView, "grid");

        if (container.TotalCount > 0)
        {
            GameObject tmpItem = container.GetItemAt(0);
            int tmpIndex = container.GetItemRealIndex(tmpItem);
            if (tmpIndex == 0)
                GameCommon.ToggleButton(tmpItem, "package_equip_item_btn");
            else
                OnSelectMagicCell(null, mEquipList[0]);
        }
        else
            TeamInfoWindow.CloseAllWindow();

        //end
    }

    private void OnSelectEquipCell(GameObject cell, EquipData data)
    {
        mSelectedItemId = data.itemId;
        TeamInfoWindow.CloseAllWindow();
        DataCenter.OpenWindow("TEAM_DATA_WINDOW", data);
        DataCenter.SetData("TEAM_DATA_WINDOW", "HIDE_BUTTON", true);
    }

    private void OnSelectFragmentCell(GameObject cell, EquipFragmentData data)
    {
        TeamInfoWindow.CloseAllWindow();
        DataCenter.OpenWindow("EQUIP_FRAGMENT_INFO_WINDOW", data);
    }

    private void OnSelectMagicCell(GameObject cell, EquipData data)
    {
        mSelectedItemId = data.itemId;
        TeamInfoWindow.CloseAllWindow();
        DataCenter.OpenWindow("TEAM_DATA_WINDOW", data);
        DataCenter.SetData("TEAM_DATA_WINDOW", "HIDE_BUTTON", true);
    }


    /// <summary>
    /// 装备法器排序比较器
    /// 第一优先级：已上阵优先
    /// 第二优先级：品质高者优先
    /// 第三优先级：精炼等级高者优先
    /// 第四优先级：强化等级高者优先
    /// 第五优先级：tid低者优先
    /// </summary>
    /// <param name="lhs"> 装备数据 </param>
    /// <param name="rhs"> 装备数据 </param>
    /// <returns></returns>
    private int EquipDataComparison(EquipData lhs, EquipData rhs)
    {
        if (lhs.teamPos >= 0 && rhs.teamPos < 0)
        {
            return -1;
        }
        else if (lhs.teamPos < 0 && rhs.teamPos >= 0)
        {
            return 1;
        }
        else
        {
            if (lhs.mQualityType != rhs.mQualityType)
            {
                return (int)rhs.mQualityType - (int)lhs.mQualityType;
            }
            else
            {
                if (lhs.refineLevel != rhs.refineLevel)
                {
                    return rhs.refineLevel - lhs.refineLevel;
                }
                else
                {
                    if (lhs.strengthenLevel != rhs.strengthenLevel)
                    {
                        return rhs.strengthenLevel - lhs.strengthenLevel;
                    }
                    else
                    {
                        return lhs.tid - rhs.tid;
                    }
                }

            }
        }
    }

    /// <summary>
    /// 装备碎片排序比较器
    /// 第一优先级：可合成的优先
    /// 第二优先级：数量多者优先
    /// 第三优先级：品质高者优先
    /// 第四优先级：tid低者优先
    /// </summary>
    /// <param name="lhs"> 碎片数据 </param>
    /// <param name="rhs"> 碎片数据 </param>
    /// <returns></returns>
    private int EquipFragmentDataComparison(EquipFragmentData lhs, EquipFragmentData rhs)
    {
        bool lhsCanCompose = CanFragmentCompose(lhs);
        bool rhsCanCompose = CanFragmentCompose(rhs);

        if (lhsCanCompose && !rhsCanCompose)
        {
            return -1;
        }
        else if (!lhsCanCompose && rhsCanCompose)
        {
            return 1;
        }
        else
        {
            if (lhs.itemNum != rhs.itemNum)
            {
                return rhs.itemNum - lhs.itemNum;
            }
            else
            {
                int lhsQuality = GetFragmentQuality(lhs);
                int rhsQuality = GetFragmentQuality(rhs);

                if (lhsQuality != rhsQuality)
                {
                    return rhsQuality - lhsQuality;
                }
                else
                {
                    return lhs.tid - rhs.tid;
                }
            }
        }
    }

    private bool CanFragmentCompose(EquipFragmentData data)
    {
        return data.itemNum >= TableCommon.GetNumberFromFragment(data.tid, "COST_NUM");
    }

    private int GetFragmentQuality(EquipFragmentData data)
    {
        return TableCommon.GetNumberFromRoleEquipConfig(data.mComposeItemTid, "QUALITY");
    }

    private void SetCount(int current, int max)
    {
        GameObject go = GetSub("right_sprite");
        GameCommon.SetUIText(go, "Label01", current.ToString());
        GameCommon.SetUIText(go, "Label03", max.ToString());
    }
}


public class Button_select_equip_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PACKAGE_EQUIP_WINDOW", "SELECT_EQUIP_GROUP", true);
        return true;
    }
}


public class Button_select_fragment_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PACKAGE_EQUIP_WINDOW", "SELECT_FRAGMENT_GROUP", true);
        return true;
    }
}


public class Button_select_magic_btn : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.SetData("PACKAGE_EQUIP_WINDOW", "SELECT_MAGIC_GROUP", true);
        return true;
    }
}



