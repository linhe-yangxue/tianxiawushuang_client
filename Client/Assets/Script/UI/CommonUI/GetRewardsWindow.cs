using UnityEngine;
using Logic;
using System.Collections.Generic;

public class DeliverParams {
	public int index;
	public object param;
}

public class GetRewardsWindow : tWindow
{
    public System.Action mOkBtnCallback = null;
    public override void Init()
    {
        EventCenter.Register("Button_get_rewards_window_button", new DefineFactory<Button_get_rewards_window_button>());
    }

    public override void Open(object param)
    {
        base.Open(param);
        Refresh(param);
    }

    public override bool Refresh(object param)
    {
		GameCommon.GetButtonData (GetSub ("get_rewards_window_button")).set ("WINDOW_NAME", mWinName);

		if(param is DeliverParams) {

			DeliverParams dp = param as DeliverParams;
			// 该信息为抽奖来源的信息
			if(dp.index == 1) {

				object content = dp.param;
				List<ItemDataBase> idb = new List<ItemDataBase>();
				//ItemDataBase[] idb = new ItemDataBase[]{};
				if(content is ItemDataBase) {
					idb.Add((ItemDataBase)content);
				}else if (content is ItemDataBase[]) {
					foreach(ItemDataBase i in (ItemDataBase[])content) {
						idb.Add(i);
					}
				}

				int count = idb.Count;
				UIScrollView scrollview = GetComponent<UIScrollView>("Scroll View");
				UIGridContainer container = GetComponent<UIGridContainer>("Grid");
				GameObject scrollbar = GetSub("scroll_bar");
				
				container.MaxCount = 0;
				container.MaxCount = count;
				//scrollview.ResetPosition();
				
				scrollview.enabled = count > 3;
				scrollbar.SetActive(count > 3);
				
				if (count == 1)
				{
					container.transform.localPosition = new Vector3(0, 0, 0);
				}
				else if (count == 2)
				{
					container.transform.localPosition = new Vector3(-74, 0, 0);
				}
				else
				{
					container.transform.localPosition = new Vector3(-148, 0, 0);
				}

				for(int m = 0; m < idb.Count; m++ ) {
					GameObject item = container.controlList[m];
					// icon setting
					UISprite icon = GameCommon.FindComponent<UISprite>(item, "item_icon");
                    //by chenliang
                    //begin

// 					string altsName = TableCommon.GetStringFromActiveCongfig(idb[m].tid, "HEAD_ATLAS_NAME");
// 					string spriteName = TableCommon.GetStringFromActiveCongfig(idb[m].tid, "HEAD_SPRITE_NAME");
// 					string itemName = TableCommon.GetStringFromActiveCongfig(idb[m].tid, "NAME");
// 					GameCommon.FindObject(item, "fragment_sprite").SetActive(false);
// 					// remove icon left top tag
// 					GameCommon.SetIcon(icon, altsName, spriteName);
//----------------------
                    //修改图标设置方式
                    GameCommon.SetOnlyItemIcon(icon.gameObject, idb[m].tid);

                    //end
					// icon end
					GameCommon.SetUIText(item, "count_label", "x" + idb[m].itemNum.ToString());
				}

			}

			return true;
		}

		if(param is ItemDataBase[]) {
			ItemDataBase[] idb = (ItemDataBase[])param;
			int count = idb.Length;
			UIScrollView scrollview = GetComponent<UIScrollView>("Scroll View");
			UIGridContainer container = GetComponent<UIGridContainer>("Grid");
			GameObject scrollbar = GetSub("scroll_bar");
			
			container.MaxCount = 0;
			container.MaxCount = count;
			//scrollview.ResetPosition();

			scrollview.enabled = count > 3;
			scrollbar.SetActive(count > 3);
			
			if (count == 1)
			{
				container.transform.localPosition = new Vector3(0, 0, 0);
			}
			else if (count == 2)
			{
				container.transform.localPosition = new Vector3(-74, 0, 0);
			}
			else
			{
				container.transform.localPosition = new Vector3(-148, 0, 0);
			}

            //for(int i=0;i<)
            for(int i=0;i<idb.Length;i++) {
                GameObject item=container.controlList[i];
                // icon setting
                UISprite icon=GameCommon.FindComponent<UISprite>(item,"item_icon");
                //by chenliang
                //begin

                // 			string altsName = TableCommon.GetStringFromRoleEquipConfig(idb[0].tid, "ICON_ATLAS_NAME");
                // 			string spriteName = TableCommon.GetStringFromRoleEquipConfig(idb[0].tid, "ICON_SPRITE_NAME");
                // 			string itemName = TableCommon.GetStringFromRoleEquipConfig(idb[0].tid, "NAME");
                // 			// remove icon left top tag
                // 			GameCommon.SetIcon(icon, altsName, spriteName);
                //----------------------
                //修改图标设置方式
                GameCommon.SetOnlyItemIcon(icon.gameObject,idb[i].tid);
                //设置奖励名称
                GameCommon.SetUIText(item,"name_label",GameCommon.GetItemName(idb[i].tid));

                //end
                // icon end
                GameCommon.SetUIText(item,"count_label","x"+idb[i].itemNum.ToString());

            }
			
			return true;
		}

        if (param is IItemDataProvider)
        {
            IItemDataProvider provider = param as IItemDataProvider;
            int count = provider.GetCount();

            UIScrollView scrollview = GetComponent<UIScrollView>("Scroll View");
            UIGridContainer container = GetComponent<UIGridContainer>("Grid");
            GameObject scrollbar = GetSub("scroll_bar");

            container.MaxCount = 0;
            container.MaxCount = count;
            //scrollview.ResetPosition();
            scrollview.enabled = count > 3;
            scrollbar.SetActive(count > 3);

            if (count == 1)
            {
                container.transform.localPosition = new Vector3(0, 0, 0);
            }
            else if (count == 2)
            {
                container.transform.localPosition = new Vector3(-74, 0, 0);
            }
            else
            {
                container.transform.localPosition = new Vector3(-148, 0, 0);
            }

            for (int i = 0; i < count; ++i)
            {
                ItemIcon icon = new ItemIcon(container.controlList[i]);
                icon.Reset();
                //added by xuke begin
                //icon.Set(provider.GetItem(i));
                GameCommon.SetUIText(container.controlList[i], "count_label", "x" + provider.GetItem(i).mNumber.ToString());
                GameCommon.SetOnlyItemIcon(GameCommon.FindObject(container.controlList[i], "item_icon"), provider.GetItem(i).mID);
                //end
				GameCommon.SetUIText(container.controlList[i],"name_label",GameCommon.GetItemName(provider.GetItem(i).mID));
                GameCommon.SetUIText(mGameObjUI, "title", "奖励信息");                
			}

        }

        return true;
    }
}


public class Button_get_rewards_window_button : CEvent
{
    public override bool _DoEvent()
    {
        GetRewardsWindow _rewardWin = DataCenter.GetData("GET_REWARDS_WINDOW") as GetRewardsWindow;
		if (GameObject .Find ("active_list_window_back_group") != null) {
			GameObject .Find ("active_list_window_back_group").GetComponent <UIPanel > ().depth = 2;
		}

        if (_rewardWin.mOkBtnCallback != null) 
        {
            _rewardWin.mOkBtnCallback();
            _rewardWin.mOkBtnCallback = null;   
        }
		string windowName = getObject ("WINDOW_NAME").ToString ();
		DataCenter.CloseWindow(windowName);
        return true;
    }
}