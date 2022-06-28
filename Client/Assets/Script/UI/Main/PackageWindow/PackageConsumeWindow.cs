using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataTable;
using Logic;

public class PackageConsumeWindow : tWindow
{
	public int isCanKeyUse = 1;
    public override void Init()
    {
        EventCenter.Self.RegisterEvent("Button_package_consume_close", new DefineFactory<Button_package_consume_close>());
		EventCenter.Self.RegisterEvent("Button_package_shop_button", new DefineFactory<Button_package_shop_button>());
    }

    public override void OnOpen()
    {
        RefreshGroup();
    }

	public override void Close ()
	{
		base.Close ();
		DataCenter.CloseBackWindow ();
	}

	/// <summary>
	/// 刷新背包
	/// </summary>
	/// <param name="isChooseFirstOne">如果为true <c>true</c>选择第一个道具.</param>
    public void RefreshGroup(int itemId = -1,bool isChooseFirstOne = true)
    {
        List<ConsumeItemData> consumeList = new List<ConsumeItemData>(ConsumeItemLogicData.Self.mDicConsumeItemData.Values);
        consumeList.RemoveAll(x => x.itemNum <= 0);
        consumeList.Sort(ConsumeDataComparison);

        UIGridContainer container = GetComponent<UIGridContainer>("grid");
        container.MaxCount = consumeList.Count;
		DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_GOODS_TIPS,"STRING_CN");
		if (container.MaxCount == 0){
			GameObject .Find ("Label_left_no_equip_tips").GetComponent <UILabel >().text =DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_GOODS_TIPS,"STRING_CN");
			GameObject .Find ("Label_right_no_information_tips").GetComponent <UILabel >().text =DataCenter.mStringList.GetData((int)STRING_INDEX.ERROR_NO_INFORMATION_TIPS,"STRING_CN");
			GameObject.Find("package_shop_button").GetComponent<UIImageButton>().isEnabled = true;
		}
		else{
			GameObject .Find ("Label_left_no_equip_tips").GetComponent <UILabel >().text ="";
			GameObject .Find ("Label_right_no_information_tips").GetComponent <UILabel >().text ="";
			GameObject.Find("package_shop_button").GetComponent<UIImageButton>().isEnabled = false;
		}
        for (int i = 0; i < container.MaxCount; ++i)
        {
            // 添加使用道具需要的主角等级值
            DataRecord r = DataCenter.mConsumeConfig.GetRecord(consumeList[i].tid);
            consumeList[i].needLevel = r["USELEVEL"];

            RefreshCell(container.controlList[i], consumeList[i]);
        }

		// 1.当前选中道具用完了，并且还有道具的话就默认选择第一个道具
        if (container.MaxCount > 0 && isChooseFirstOne)
		{
			GameCommon.ToggleButton (container.controlList [0], "package_consume_item_bth");
		} 
		//2.当前道具没有用完，并且还有道具的话则还是选择当前道具
		else if (container.MaxCount > 0 && !isChooseFirstOne) 
		{
			if(itemId != -1)
			{
				// 重新选择该道具刷新信息
				for(int i = 0;i < container.MaxCount;i++)
				{
					if(itemId == consumeList[i].itemId)
						GameCommon.ToggleButton (container.controlList [i], "package_consume_item_bth");
				}
			}
		}
		// 3.当前没有道具
		else
        {
            SetVisible("consume_info", false);
        }
    }

    private void RefreshCell(GameObject cell, ConsumeItemData data)
    {
        UIButtonEvent evt = GameCommon.FindComponent<UIButtonEvent>(cell, "package_consume_item_bth");
        evt.AddAction(() => OnSelectCell(cell, data));
        GameCommon.SetItemIcon(cell, data);
        string name = GameCommon.GetItemStringField(data.tid, GET_ITEM_FIELD_TYPE.NAME);
        GameCommon.SetUIText(cell, "name_label", name);
        GameCommon.FindObject(cell, "num_label", "num").GetComponent<UILabel>().text = data.itemNum.ToString();

        // 物品有等级要求，则显示需要等级字样，同时根据主角是否满足等级要求设置字体颜色
        if (data.needLevel > 0)
        {
            UILabel needLevelLabel = GameCommon.FindComponent<UILabel>(cell, "need_level_label");
            if (needLevelLabel.gameObject == null)
            {
                DEBUG.LogError("Can't find 'package_consume_item_bth/.../need_level_label'");
                return;
            }
            needLevelLabel.text = "需要等级：" + data.needLevel.ToString();
            if (RoleLogicData.Self.character.level < data.needLevel)
            {
                needLevelLabel.color = Color.red;
            }
            else
            {
                needLevelLabel.color = Color.white;
                // 如果满足等级要求，添加提示红点
                //GameObject newMark = GameCommon.FindObject(cell, "NewMark");
                //if (newMark == null)
                //{
                //    DEBUG.LogError("Can't find the NewMark");
                //}
                //else
                //{
                //    newMark.SetActive(true);
                //}
            }
            needLevelLabel.gameObject.SetActive(true); 
        }
    }

    private void OnSelectCell(GameObject cell, ConsumeItemData data)
    {
        SetVisible("consume_info", true);
        SetInfo(data);
    }

    private int ConsumeDataComparison(ConsumeItemData lhs, ConsumeItemData rhs)
    {
        return lhs.tid - rhs.tid;
    }

    private void SetInfo(ConsumeItemData data)
    {
        GameCommon.SetItemIcon(GetSub("consume_icon"), data);
        GameObject icon = GetSub("consume_icon");

        string name = GameCommon.GetItemStringField(data.tid, GET_ITEM_FIELD_TYPE.NAME);
        GameCommon.SetUIText(icon, "name_label", name);
        GameCommon.SetUIText(icon, "num", data.itemNum.ToString());

        string desc = GameCommon.GetItemStringField(data.tid, GET_ITEM_FIELD_TYPE.DESC);
        desc = desc.Replace("\\n", "\n");
        GameCommon.SetUIText(icon, "Label", desc);

        DataRecord record = DataCenter.mConsumeConfig.GetRecord(data.tid);
        bool useable = (int)record["USEABLE"] == 1;
        int _itemGroupType = (int)record["ITEM_GROUP_TYPE"];
        int funcition = (int)record["FUNCTION"];
        string funcTip = (string)record["FUNCTION_TIP"];
        GameObject useBtn = GetSub("package_consume_use_btn");
        GameObject goBtn = GetSub("package_consume_go_btn");
		GameObject keyUseBtn = GetSub("package_consume_key_use_btn");

        if (useable)
        {
            bool _showKeyBtn = _itemGroupType == 1 || _itemGroupType == 2;
            useBtn.SetActive(true);
            goBtn.SetActive(false);
            keyUseBtn.SetActive(_showKeyBtn);
            useBtn.transform.localPosition = _showKeyBtn ? new Vector3(-100, -265, 0) : new Vector3(0, -265,0);

            if (_itemGroupType == 0)
            {
                useBtn.GetComponent<UIButtonEvent>().AddAction(() => OnClickResBtn(data));
            }
            else
            {
                useBtn.GetComponent<UIButtonEvent>().AddAction(() => OnClickUseBtn(data));
            }
			keyUseBtn.GetComponent<UIButtonEvent>().AddAction (() => OnClickKeyUseBtn (data));
        }
        else 
        {
            useBtn.SetActive(false);
			keyUseBtn.SetActive (false);
            if (funcition > 0)
            {
                goBtn.SetActive(true);
                GameCommon.SetUIText(goBtn, "Label", funcTip);

                goBtn.GetComponent<UIButtonEvent>().AddAction(() => { OnClickGoBtn(funcition); });
            }
            else 
            {
                goBtn.SetActive(false);
            }
        }
    }

    private void OnClickResBtn(ConsumeItemData data)
    {
        //当UseRes = true,这里的Type没有实际用处
        MallBuyConsumeData _mallComsumeData = new MallBuyConsumeData(ShopType.Tower);
        _mallComsumeData.UseRes = true;
        _mallComsumeData.Tid = data.tid;
        _mallComsumeData.SureBuy = (int buyCount) => 
        {
            data.mSpecifyUseNum = buyCount;
            OnClickUseBtn(data);
            DataCenter.CloseWindow("MALL_BUY_CONSUME_WINDOW");
        };
        _mallComsumeData.Complete = (string str,int value) => 
        {
           
        };
        DataCenter.OpenWindow("MALL_BUY_CONSUME_WINDOW", _mallComsumeData);
    }

    private void OnClickUseBtn(ConsumeItemData data)
    {
        //增加等级限制的判断
        if (RoleLogicData.Self.character.level < data.needLevel)//主角等级不足
        {
            DataCenter.ErrorTipsLabelMessage("等级不足");
            return;
        }

        DoCoroutine(DoUse(data));
    }
	private void OnClickKeyUseBtn(ConsumeItemData data)
	{
		//增加等级限制的判断
		if (RoleLogicData.Self.character.level < data.needLevel)//主角等级不足
		{
			DataCenter.ErrorTipsLabelMessage("等级不足");
			return;
		}
		
		DoCoroutine(DoKeyUse(data));
	}

    private void OnClickGoBtn(int kGoToIndex)
    {
        //判断对应功能是否开放
        if (!CheckFuncOpen(kGoToIndex)) 
        {
            return;
        }

        Close();

        bool _isSuccess = GetPathHandlerDic.ExecuteDelegate(kGoToIndex);
        if (!_isSuccess) 
        {
            DEBUG.LogError("没有成功执行相应的前往逻辑,Index = " + kGoToIndex);
        }
    }

    private bool CheckFuncOpen(int kGotoIndex) 
    {
        DataRecord _record = DataCenter.mGainFunctionConfig.GetRecord(kGotoIndex);
        if (_record == null)
        {
            DEBUG.LogError("mGainFunctionConfig 中没有相应的记录 Index = " + kGotoIndex);
            return false;
        }
        int _funcID = (int)_record["Function_ID"];
        if (GameCommon.IsFuncCanUse(_funcID))
        {
            return true;
        }
        else 
        {
            DataRecord _funcRecord = DataCenter.mFunctionConfig.GetRecord(_funcID);
            if(_funcRecord == null)
            {
                DEBUG.LogError("mFunctionConfig 中没有相应的记录 Index = " + _funcID);
                return false;
            }
            int _openLevel = GameCommon.GetFuncCanUseLevelByFuncID(_funcID);
            DataCenter.ErrorTipsLabelMessage(_funcRecord["FUNC_CONDITION_DESCRIBE"]);
            return false;
        }
    }

	//-------------------------------------------------------------------------------------------
	private IEnumerator DoKeyUse(ConsumeItemData data)
	{
		int iCountNum = data.itemNum;
		DataRecord r = DataCenter.mConsumeConfig.GetRecord(data.tid);    
		ItemDataBase item = new ItemDataBase { itemId = data.itemId, tid = data.tid, itemNum = 1 };
		int useType = r["ITEM_GROUP_TYPE"];
		int peaceHour = r["GUIDE_TIME"];
		string notice = r["AWARD_NOTICE"];
		int keyUseable = r["USEABLE"];

		if (peaceHour > 0)
		{
			UseTruceTokenRequester req = new UseTruceTokenRequester(data, peaceHour * 3600);
			yield return req.Start();
			
			if (req.success)
			{
				if (string.IsNullOrEmpty(notice) || notice == "0")
				{
					DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", req.item);    
				}
				else 
				{
					DataCenter.OpenMessageWindow(notice);                   
				}
				
				RefreshGroup(data.itemId,data.itemNum == 0);
			}
		}
		else if (useType == 3) // 多选一宝箱
		{
			item.itemNum = data.itemNum;
			List<ItemDataBase> list = GameCommon.GetItemGroup(r["ITEM_GROUP_ID"], false);
			//by chenliang
			//begin			
			//检查相应背包是否已满
			List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(list);
			if (!CheckPackage.Instance.CanAddItems(tmpPackageTypes))
				yield break;			
			//end
			DataCenter.OpenWindow("KEY_USE_CHOOSE_REWARDS_WINDOW", data);			
			// 设置key_use_choose_reward_ok_button的标志位
			PackageKeyUseChooseWindow _tWin = DataCenter.GetData("KEY_USE_CHOOSE_REWARDS_WINDOW") as PackageKeyUseChooseWindow;
			string str = "PackageConsumeWindow";
			if (_tWin != null)
				GameCommon.GetButtonData(GameCommon.FindObject(_tWin.mGameObjUI, "key_use_choose_reward_ok_button")).set("CALL_BY", str);			
//			SelectItemWindow.onCommit = x => GlobalModule.DoCoroutine(DoSendSelectResult(item, x, notice));      
		}
		else
		{
			isCanKeyUse = 1;
			for(int i = 0; i < iCountNum; i++)
			{
				//by chenliang
				//begin			
				//检查相应背包是否已满
				List<PACKAGE_TYPE> tmpPackageTypes = new List<PACKAGE_TYPE>()
				{
					PackageManager.GetPackageTypeByItemTid(item.tid)
				};
				if (!CheckPackage.Instance.CanAddItems(tmpPackageTypes))
					yield break;

				//end
				if(isCanKeyUse == 1)
				{
					UsePropRequester req = new UsePropRequester(item);
					yield return req.Start();
					if (req.success)
					{
						SC_UseProp tmpResp = req.respMsg;
						if(i == 0)
							DataCenter.OpenWindow("PACKAGE_KEY_USE_WINDOW", data);
						yield return new WaitForSeconds(0f);
                        List<ItemDataBase> _awardList = (List<ItemDataBase>)DataCenter.Self.getObject("USE_PROP_AWARD");
                        DataCenter.SetData("PACKAGE_KEY_USE_WINDOW", "PACKAGE_KEY_USE", _awardList);

					}else if(!req.success)
					{
						isCanKeyUse = 0;
						DataCenter.SetData("PACKAGE_KEY_USE_WINDOW", "PACKAGE_KEY_USE_STOP", isCanKeyUse);
						break;
					}
				}
			}
		}
	}
//--------------------------------------------------------------------------------------------------

    private IEnumerator DoUse(ConsumeItemData data)
    {
        DataRecord r = DataCenter.mConsumeConfig.GetRecord(data.tid);
        int _useNum = data.mSpecifyUseNum == -1 ? 1 : data.mSpecifyUseNum;
        ItemDataBase item = new ItemDataBase { itemId = data.itemId, tid = data.tid, itemNum = _useNum };
        int useType = r["ITEM_GROUP_TYPE"];
        int peaceHour = r["GUIDE_TIME"];
        string notice = r["AWARD_NOTICE"];

        if (peaceHour > 0)
        {
            UseTruceTokenRequester req = new UseTruceTokenRequester(data, peaceHour * 3600);
            yield return req.Start();

            if (req.success)
            {
                if (string.IsNullOrEmpty(notice) || notice == "0")
                {
//                    DataCenter.OpenWindow("AWARD_WINDOW", req.item);      
					DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", req.item);    
                }
                else 
                {
                    DataCenter.OpenMessageWindow(notice);                   
                }

                RefreshGroup(data.itemId,data.itemNum == 0);
            }
        }
        else if (useType == 3) // 多选一宝箱
        {
            List<ItemDataBase> list = GameCommon.GetItemGroup(r["ITEM_GROUP_ID"], false);
            //by chenliang
            //begin

            //检查相应背包是否已满
            List<PACKAGE_TYPE> tmpPackageTypes = PackageManager.GetPackageTypes(list);
            if (!CheckPackage.Instance.CanAddItems(tmpPackageTypes))
                yield break;

            //end

            DataCenter.OpenWindow("ASTROLOGY_REWARD_WINDOW", int.Parse(r["ITEM_GROUP_ID"]));

            // 设置astrology_reward_ok_button的标志位，表明由谁调用（AstrologyUIWindow处也有调用）
            tWindow _tWin = DataCenter.GetData("ASTROLOGY_REWARD_WINDOW") as tWindow;
            string str = "PackageConsumeWindow";
            if (_tWin != null)
                GameCommon.GetButtonData(GameCommon.FindObject(_tWin.mGameObjUI, "astrology_reward_ok_button")).set("CALL_BY", str);

            SelectItemWindow.onCommit = x => GlobalModule.DoCoroutine(DoSendSelectResult(item, x, notice));

            //DataCenter.OpenWindow("SELECT_ITEM_WINDOW", list);
            //DataCenter.SetData("SELECT_ITEM_WINDOW", "TITLE", (string)r["ITEM_NAME"]);
            //DataCenter.SetData("SELECT_ITEM_WINDOW", "DESC", "请选择领取其中一件奖励");        
        }
        else
        {
            //by chenliang
            //begin

            //检查相应背包是否已满
            List<PACKAGE_TYPE> tmpPackageTypes = new List<PACKAGE_TYPE>()
            {
                PackageManager.GetPackageTypeByItemTid(item.tid)
            };
            if (!CheckPackage.Instance.CanAddItems(tmpPackageTypes))
                yield break;

            //end
            UsePropRequester req = new UsePropRequester(item);
            yield return req.Start();

            if (req.success)
            {
                if (string.IsNullOrEmpty(notice) || notice == "0") 
                {
                    List<ItemDataBase> _awardList = (List<ItemDataBase>)DataCenter.Self.getObject("USE_PROP_AWARD");
                    // By XiaoWen
                    // Begin
                    bool isHasQualityPet = false;
                    List<int> _tidList = GameCommon.GetTidByItemData(_awardList);
                    for (int i = 0, count = _tidList.Count; i < count; ++i)
                    {
                        if (PetGainWindow.CheckPetGainQuality(_tidList[i]))
                        {
                            isHasQualityPet = true;
                            break;
                        }
                    }

                    if (_awardList != null) 
                    {
//                        DataCenter.OpenWindow("AWARD_WINDOW", _awardList);
                        if (!isHasQualityPet)
                        { 
						    DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", _awardList);   
                        }

                        DataCenter.Set("USE_PROP_AWARD",null);
                    }

                    for (int i = 0, count = _tidList.Count; i < count; ++i) 
                    {
                        if (PetGainWindow.CheckPetGainQuality(_tidList[i])) 
                        {
                            DataCenter.OpenWindow("PET_GAIN_WINDOW", _tidList.GetEnumerator());
                            break;
                        }
                    }
                    // End
                }
                else
                {
                    DataCenter.OpenMessageWindow(notice);
                }             
				RefreshGroup(data.itemId,data.itemNum == 0);
            }
        }
    }

    public IEnumerator DoSendSelectResult(ItemDataBase useItem, ItemDataBase selectedItem, string notice)
    {
        OpenBoxSelectRequester req = new OpenBoxSelectRequester(useItem, selectedItem);
        yield return req.Start();

     

        if (req.success)
        {
            if (string.IsNullOrEmpty(notice) || notice == "0")
            {
//              DataCenter.OpenWindow("AWARD_WINDOW", req.respMsg.item);
				DataCenter.OpenWindow("AWARDS_TIPS_WINDOW", req.respMsg.items[0]);
                int _petTid = req.respMsg.items[0].tid;

                if (PetGainWindow.CheckPetGainQuality(_petTid))
                {
                    DataCenter.OpenWindow("PET_GAIN_WINDOW", _petTid);
                }
            }
            else
            {
                DataCenter.OpenMessageWindow(notice);
            }

            RefreshGroup();
        }
    }
}

public class Button_package_shop_button : CEvent
{
	public override bool _DoEvent()
	{
		if (DataCenter.GetData ("SHOP_WINDOW") != null) 
		{
			GlobalModule.ClearAllWindow();
			DataCenter.OpenWindow("SHOP_WINDOW", null);
			GlobalModule.DoCoroutine(__GoToShop());
		}
		
		else
		{
			GlobalModule.ClearAllWindow();
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
		}
		return true;
	}	
	private IEnumerator __GoToShop()
	{
		yield return new WaitForEndOfFrame();
		DataCenter.SetData("SHOP_WINDOW", "OPEN_SHOP_WINDOW", SHOP_PAGE_TYPE.TOOL);
	}
}
public class Button_package_consume_close : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow("PACKAGE_CONSUME_WINDOW");
        MainUIScript.Self.ShowMainBGUI();
        return true;
    }
}