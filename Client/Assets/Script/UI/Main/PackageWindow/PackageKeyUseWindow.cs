using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;
using Utilities;

public class PackageKeyUseWindow : tWindow
{
	public UIGridContainer grid;
	int iLeftNum = 0;
	ConsumeItemData data;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_package_key_use_close_button", new DefineFactory<Button_package_key_use_close_button>());
		EventCenter.Self.RegisterEvent("Button_package_key_use_finish_btn", new DefineFactory<Button_package_key_use_close_button>());
		EventCenter.Self.RegisterEvent("Button_package_key_use_stop_btn", new DefineFactory<Button_package_key_use_stop_btn>());
	}

	public override void Open(object param)
	{
		base.Open(param);
		grid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "package_key_use_grid");
		UIScrollView _resultScrollView = GameCommon.FindObject(mGameObjUI, "package_key_use_scrollview").GetComponent<UIScrollView>();
		_resultScrollView.ResetPosition();
		if (grid != null)
		{
			grid.MaxCount = 0;
		}
		data = (ConsumeItemData)param;
		iLeftNum = data.itemNum + 1;
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		switch (keyIndex)
		{
		case "PACKAGE_KEY_USE":
			SetUseList((List<ItemDataBase>)objVal);
			break;
		case "PACKAGE_KEY_USE_STOP":
			SetButtonsInfos((int)objVal);	
			break;
		}
	}
	void SetButtonsInfos(int iKeys)
	{
		GameObject packageKeyUseStopBtnObj = GameCommon.FindObject (mGameObjUI, "package_key_use_stop_btn");
		GameObject packageKeyUseFinishBtnObj = GameCommon.FindObject (mGameObjUI, "package_key_use_finish_btn");
		if(iKeys == 1)
		{
			packageKeyUseStopBtnObj.SetActive (true);
			packageKeyUseFinishBtnObj.SetActive (false);
		}else 
		{
			packageKeyUseStopBtnObj.SetActive (false);
			packageKeyUseFinishBtnObj.SetActive (true);
		}
	}

	void SetUseList(List<ItemDataBase> itemDataList)
	{
		GameObject packageKeyUseStopBtnObj = GameCommon.FindObject (mGameObjUI, "package_key_use_stop_btn");
		GameObject packageKeyUseFinishBtnObj = GameCommon.FindObject (mGameObjUI, "package_key_use_finish_btn");
		GameObject currSpritNumObj = GameCommon.FindObject (mGameObjUI, "curr_sprit_num");
		
		int tmpMaxCount = iLeftNum;
		grid.MaxCount += 1;
		GameObject tmpGOItem = grid.controlList[grid.MaxCount - 1];			
		//第几次
		GameCommon.SetUIVisiable(tmpGOItem, "time_label", true);
		GameCommon.SetUIText(tmpGOItem, "time_label", "第" + (grid.MaxCount).ToString() + "次");		
		currSpritNumObj.GetComponent<UILabel>().text = (tmpMaxCount - grid.MaxCount).ToString ();
		GameObject useOverTipLabelObj = GameCommon.FindObject (tmpGOItem, "use_over_tip_label").gameObject;
		
		//图标
		UIGridContainer itemIconGrid = GameCommon.FindObject (tmpGOItem, "item_icon_grid").GetComponent<UIGridContainer>();	
		//ItemDataBase[] _items = tmpResp.items;

        if (itemDataList != null && itemDataList.Count > 0)
            itemIconGrid.MaxCount = itemDataList.Count;
		else 
			itemIconGrid.MaxCount = 0;		
		for(int j = 0; j < itemIconGrid.MaxCount; j++)
		{
			GameObject tmpObj = itemIconGrid.controlList[j];
			GameObject tmpGOIcon = GameCommon.FindObject(tmpObj, "item_icon");
            int _tid = itemDataList[j].tid;
            GameCommon.SetOnlyItemIcon(tmpGOIcon, _tid);
            GameCommon.SetUIText(tmpGOIcon, "num_label", "x" + itemDataList[j].itemNum.ToString());
            GameCommon.BindItemDescriptionEvent(tmpObj, _tid);
		}
		PackageConsumeWindow tmpWin = DataCenter.GetData("PACKAGE_CONSUME_WINDOW") as PackageConsumeWindow;
		if(grid.MaxCount >= tmpMaxCount)
		{
			packageKeyUseStopBtnObj.SetActive (false);
			packageKeyUseFinishBtnObj.SetActive (true);
			useOverTipLabelObj.SetActive (true);
			useOverTipLabelObj.transform.localPosition = new Vector3(0, -95.0f, 0f);
			if(tmpWin != null)
			{
				tmpWin.isCanKeyUse = 0;
			}
		}else
		{
			packageKeyUseStopBtnObj.SetActive (true);
			packageKeyUseFinishBtnObj.SetActive (false);
			useOverTipLabelObj.SetActive (false);
			if(tmpWin.isCanKeyUse == 0)
			{
				packageKeyUseStopBtnObj.SetActive (false);
				packageKeyUseFinishBtnObj.SetActive (true);
			}
		}
		GlobalModule.DoCoroutine(UpdataGrid());
		if (grid.MaxCount - 2 > 3)
		{
			this.DoCoroutine(ChangeScrollValue());
		}

		GameCommon.GetButtonData(GameCommon.FindObject (mGameObjUI, "package_key_use_close_button")).set("PACKAGE_KEY_DATA", data);
		GameCommon.GetButtonData(packageKeyUseFinishBtnObj).set("PACKAGE_KEY_DATA", data);
	}	

	IEnumerator UpdataGrid()
	{
		yield return null;
		grid.Reposition();
		grid.GetComponent<DynamicGridContainer>().Adjust();
	}

	private IEnumerator ChangeScrollValue()
	{
		UIScrollView tmpResultScrollView = GameCommon.FindObject(mGameObjUI, "package_key_use_scrollview").GetComponent<UIScrollView>();
		tmpResultScrollView.ResetPosition();
		
		yield return null;
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(0.2f);
		tmpResultScrollView.SetDragAmount(0.0f, 1.0f, false);
	}
}
public class Button_package_key_use_close_button : CEvent
{
	public override bool _DoEvent()
	{
		ItemDataBase data = (ItemDataBase)getObject("PACKAGE_KEY_DATA");

		PackageConsumeWindow tmpWin = DataCenter.GetData("PACKAGE_CONSUME_WINDOW") as PackageConsumeWindow;
		if (tmpWin != null)
			tmpWin.RefreshGroup(data.itemId, data.itemNum == 0);
		DataCenter.CloseWindow("PACKAGE_KEY_USE_WINDOW");
		return true;
	}
}
public class Button_package_key_use_stop_btn : CEvent
{
	public override bool _DoEvent()
	{
		PackageConsumeWindow tmpWin = DataCenter.GetData("PACKAGE_CONSUME_WINDOW") as PackageConsumeWindow; 
		if (tmpWin != null)
			tmpWin.isCanKeyUse = 0;
		return true;
	}
}