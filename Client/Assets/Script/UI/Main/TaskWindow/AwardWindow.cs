using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;


public class AwardWindow : tWindow
{
    public override void Open(object param)
    {
        base.Open(param);

        if (param is ItemDataBase)
        {
            ShowSingleItem(param as ItemDataBase);
        }
        else if (param is IEnumerable<ItemDataBase>)
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
//		while (t < 5f)
//		{
//			panel.alpha = 1f - t * 0.2f;
//			t += Time.deltaTime;
//			yield return null;
//		}
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
        SetVisible("Grid", false);
        GameObject single = GetSub("single_item");
        single.SetActive(true);
        //GameCommon.SetItemIcon(single, item);
		GameCommon.SetUIText (single,"count_label",item.itemNum.ToString());
		GameCommon.SetOnlyItemIcon (GameCommon.FindObject(single,"item_icon"),item.tid);
    }

    private void ShowMultipleItems(IEnumerable<ItemDataBase> items)
    {
        List<ItemDataBase> itemList = new List<ItemDataBase>(items);       

        SetVisible("single_item", false);
        GameObject gridObj = GetSub("Grid");
        gridObj.SetActive(true);
        UIGridContainer container = gridObj.GetComponent<UIGridContainer>();
        container.MaxCount = itemList.Count;
		if (itemList.Count <= 1)
		{
			ShowSingleItem(itemList[0]);
			return;
		}else if(itemList.Count == 2)
		{
			container.transform.localPosition = new Vector3(-50, 0, 0);
		}else if(itemList.Count == 3)
		{
			container.transform.localPosition = new Vector3(-100, 0, 0);
		}else if(itemList.Count == 4)
		{
			container.transform.localPosition = new Vector3(-150, 0, 0);
		}else 
		{
			container.transform.localPosition = new Vector3(-150, 60, 0);
		}


        for (int i = 0; i < container.MaxCount; ++i)
        {
            //GameCommon.SetItemIcon(container.controlList[i], itemList[i]);
			GameCommon.SetUIText (container.controlList[i],"count_label",itemList[i].itemNum.ToString());
			GameCommon.SetOnlyItemIcon (GameCommon.FindObject(container.controlList[i],"item_icon"),itemList[i].tid);
        }
    }
}