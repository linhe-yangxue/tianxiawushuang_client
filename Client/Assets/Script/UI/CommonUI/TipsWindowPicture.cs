using UnityEngine;
using Logic;
using System.Collections.Generic;
using System;


public class TipsPictureWindow:tWindow {
    
    public override void Open(object param) {
        base.Open(param);
        Refresh(param);
    }

    protected override void OpenInit() {
        base.OpenInit();
        AddButtonAction("Cancel",() => DataCenter.CloseWindow("TIPS_PICTURE_WINDOW"));
        AddButtonAction("get_rewards_window_background", () => DataCenter.CloseWindow("TIPS_PICTURE_WINDOW"));
    }

    public override bool Refresh(object param) {
        var tuple=(Tuple<ItemDataBase[],Action>)param;
        ItemDataBase[] idb=tuple.field1;
        int count=idb.Length;
        UIScrollView scrollview=GetComponent<UIScrollView>("Scroll View");
        UIGridContainer container=GetComponent<UIGridContainer>("Grid");
        GameObject scrollbar=GetSub("scroll_bar");

        container.MaxCount=0;
        container.MaxCount=count;
        scrollview.ResetPosition();

        scrollview.enabled=count>3;
        scrollbar.SetActive(count>3);

        if(count==1) {
            container.transform.localPosition=new Vector3(0,0,0);
        } else if(count==2) {
            container.transform.localPosition=new Vector3(-74,0,0);
        } else {
            container.transform.localPosition=new Vector3(-148,0,0);
        }

		//setcontainer
		for(int i = 0; i < count; i++ )
		{
			GameObject board=container.controlList[i];
		    setContainer (board, idb[i]);
		}

        AddButtonAction("OK",() => {
            if(tuple.field1.Length==0) DataCenter.OpenWindow("没有碎片");
            else {
                tuple.field2();
                DataCenter.CloseWindow("TIPS_PICTURE_WINDOW");    
            }
        });
        return true;
    }

	public void setContainer(GameObject board, ItemDataBase database)
	{
		GameCommon.SetItemIconNew(board, "item_icon", database.tid);
		GameCommon.SetUIText(board,"count_label","x"+database.itemNum.ToString());
	}
    
}
