using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using System.Linq;
using DataTable;

public class AwardsTipsWindow : tWindow
{
	public override void Open(object param)
	{
		base.Open(param);
		
		if (param is ItemDataBase)
		{
			ShowSingleItem(param as ItemDataBase);
		}else if (param is IEnumerable<ItemDataBase>)
		{
			ShowMultipleItems(param as IEnumerable<ItemDataBase>);
		}
		
		DoCoroutine(FadeOut());
	}
	
	private IEnumerator FadeOut()
	{
		float t = 0f;
		UIPanel panel = mGameObjUI.GetComponent<UIPanel>();
		panel.alpha = 1f;		
		yield return new WaitForSeconds(1f);
		while (t < 0.5f)
		{
			panel.alpha = 1f - t * 2;
			t += Time.deltaTime;
			yield return null;
		}		
		panel.alpha = 0f;
		Close();
		panel.alpha = 1f;
	}
	
	private void ShowSingleItem(ItemDataBase item)
	{
		GameObject gridObj = GetSub("Grid");
		UIGridContainer container = gridObj.GetComponent<UIGridContainer>();
		container.MaxCount = 1;
		GameObject spriteObj = GameCommon.FindObject (mGameObjUI, "bg_infos").gameObject;
		foreach (UISprite _uiSprite in spriteObj.GetComponentsInChildren<UISprite>())
		{
			if(_uiSprite != null)	
			{
				_uiSprite.height = 152;
//				spriteObj.transform.Find ("IconTexture").gameObject.SetActive (true);
			}
		}
		container.transform.localPosition = new Vector3(0, 70, 0);

		for (int i = 0; i < container.MaxCount; ++i)
		{
			GameCommon.SetUIText (container.controlList[i],"count_label",item.itemNum.ToString());
			GameCommon.SetUIText(container.controlList[i],"name_label",GameCommon.GetItemName(item.tid));
			GameCommon.SetOnlyItemIcon (GameCommon.FindObject(container.controlList[i],"item_icon"),item.tid);
		}
	}
	
	private void ShowMultipleItems(IEnumerable<ItemDataBase> items)
	{
		List<ItemDataBase> itemList = new List<ItemDataBase>(items);  
		//合并相同项
		Dictionary<int, int> tmpDic = new Dictionary<int, int>();
		for (int i = 0; i < itemList.Count; i++)
		{
			ItemDataBase tmpItem = itemList[i];
			if (tmpDic.ContainsKey(tmpItem.tid))
				tmpDic[tmpItem.tid] += tmpItem.itemNum;
			else
				tmpDic[tmpItem.tid] = tmpItem.itemNum;
		}
		List<ItemDataBase> _itemDataList = new List<ItemDataBase>();
		foreach (KeyValuePair<int, int> tmpPair in tmpDic)
		{
			_itemDataList.Add(new ItemDataBase(){tid = tmpPair.Key, itemNum = tmpPair.Value, itemId = -1});
		}
		//
		GameObject gridObj = GetSub("Grid");
		UIGridContainer container = gridObj.GetComponent<UIGridContainer>();
		container.MaxCount = _itemDataList.Count;
		GameObject spriteObj = GameCommon.FindObject (mGameObjUI, "bg_infos").gameObject;

		foreach (UISprite _uiSprite in spriteObj.GetComponentsInChildren<UISprite>())
		{
			if(_uiSprite != null)
			{
				if(container.MaxCount <= 4)
				{
					_uiSprite.height = 152;
//					spriteObj.transform.Find ("IconTexture").gameObject.SetActive (false);
				}else
				{
					_uiSprite.height = 261;
//					spriteObj.transform.Find ("IconTexture").gameObject.SetActive (true);
				}
			}					
		}

		if (container.MaxCount <= 1)
		{
			container.transform.localPosition = new Vector3(0, 70, 0);
		}else if(container.MaxCount == 2)
		{
			container.transform.localPosition = new Vector3(-50, 70, 0);
		}else if(container.MaxCount == 3)
		{
			container.transform.localPosition = new Vector3(-100, 70, 0);
		}else 
		{
			container.transform.localPosition = new Vector3(-150, 70, 0);
		}	

		for (int i = 0; i < container.MaxCount; ++i)
		{
			UILabel itemNameLabel = GameCommon.FindObject (container.controlList[i], "name_label").GetComponent<UILabel>();
			GameCommon.SetUIText (container.controlList[i],"count_label","x" + _itemDataList[i].itemNum.ToString());
			GameCommon.SetOnlyItemIcon (GameCommon.FindObject(container.controlList[i],"item_icon"),_itemDataList[i].tid);
			GameCommon.SetUIText(container.controlList[i],"name_label",GameCommon.GetItemName(_itemDataList[i].tid));
			itemNameLabel.color = GameCommon.GetNameColor(_itemDataList[i].tid);
			itemNameLabel.effectColor = GameCommon.GetNameEffectColor();
			if(GameCommon.CheckIsFragmentByTid(_itemDataList[i].tid));		//判断物品类别
			{
				DataRecord r = DataCenter.mFragment.GetRecord (_itemDataList[i].tid);
				DataRecord magic_record =DataCenter.mFragmentAdminConfig.GetRecord(_itemDataList[i].tid); 
				if(r != null)
				{
					int mItem_ID = r["ITEM_ID"];
					itemNameLabel.color = GameCommon.GetNameColor(mItem_ID);
					itemNameLabel.effectColor = GameCommon.GetNameEffectColor();
				}
				else if(magic_record !=null)
				{
					int mItem_ID = magic_record["ROLEEQUIPID"];
					itemNameLabel.color = GameCommon.GetNameColor(mItem_ID);
					itemNameLabel.effectColor = GameCommon.GetNameEffectColor();
				}
			}
		}
	}
}