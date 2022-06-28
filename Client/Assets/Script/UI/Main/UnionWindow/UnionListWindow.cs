using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;

class UnionListWindow:UnionBase {
	GameObject mNotFind;
	GridsContainer container;
	GuildBaseObject[] guildTemp;
	int guildLength;
	GuildBaseObject[] guildArr;
	string[] hasAppointedArrTemp;
	string[] hasAppointedArr;
	
	protected override void OpenInit() {
		base.OpenInit();
		AddButtonAction("action_toCreateUnion_btn",() => DataCenter.OpenWindow(UIWindowString.union_create));
		AddButtonAction("action_search_btn",SearchUnion);
		mNotFind=GetCurUIGameObject("not_find_union");
		container=GetCurUIComponent<GridsContainer>("union_grid");
		container.FuncRefreshCell=RefreshUILabel;
		
		//init
        guildTemp = null;
		guildLength = 0;
	}
	
	public override void OnOpen() {
		base.OnOpen();

        //set open level
        SetOpenLevelValue();

        DataCenter.OpenBackWindow(UIWindowString.union_list, "a_ui_zongmenglb_logo", () => MainUIScript.Self.ShowMainBGUI());
		GetCurUIGameObject("not_find_union").SetActive(false);
		GetUIInput("search_input").value="";
		
		CS_GetGuildArr cs=new CS_GetGuildArr();
		HttpModule.CallBack requestSuccess=(text) => {
			DEBUG.Log("RequestSuccess:text = "+text);
			SC_GetGuildArr item=JCode.Decode<SC_GetGuildArr>(text);
			var arr=item.arr;
			if(arr.Length!=0) {
				mNotFind.SetActive(false);
				guildArr=arr;
				container.MaxCount=guildArr.Length;
				hasAppointedArr=item.applyArr;
				container.RefreshAllCell();
				
				//set data 
                if (guildArr != null && guildArr.Length > 0)
                {
                    guildTemp = guildArr;
                    guildLength = guildArr.Length;
                    hasAppointedArrTemp = item.applyArr;
                }
				//for(int i=0;i<arr.Length;i++) RefreshUILabel(list[i],arr[i],item.applyArr);
			} else mNotFind.SetActive(true);
		};
		
		HttpModule.Instace.SendGameServerMessage(cs,"CS_GetGuildArr",requestSuccess,NetManager.RequestFail);
	}
	
	public override void OnClose() {
		base.OnClose();
		DataCenter.CloseWindow(UIWindowString.common_back);
		
	}
	
	void SearchUnion() {
		var input=GetUIInput("search_input").value;
		//is null show default
		if (input == "") {
            if (guildTemp != null && guildTemp.Length > 0)
            {
                container.MaxCount = guildTemp.Length;
                guildArr = guildTemp;
                hasAppointedArr = hasAppointedArrTemp;
                container.RefreshAllCell();
                mNotFind.SetActive(false);
            }
			return;
		}
		if(!isInNameLimit(input)) input=input.Substring(0,nameLengthLimit-1);
		CS_SearchGuildName cs=new CS_SearchGuildName(input);
		
		HttpModule.CallBack requestSuccess=(text) => {
			SC_SearchGuildName item=JCode.Decode<SC_SearchGuildName>(text);
			
			var guild=item.guildObjectArr;
            if (guild !=null && guild.Length == 0)
            {
				mNotFind.SetActive(true);
				container.MaxCount=0;
			} 
            else 
            {
				mNotFind.SetActive(false);
				var list=GetUIGridContainer("union_grid",1).controlList;
                if (list.Count > 0)
                {
                    list[0].gameObject.SetActive(false);
                }
                if (guild.Length > 0)
                {
                    container.MaxCount = guild.Length;
                    guildArr = item.guildObjectArr;
                    hasAppointedArr = item.applyArr;
                    container.RefreshAllCell();
                }
			}
		};
		
		
		HttpModule.Instace.SendGameServerMessage(cs,"CS_SearchGuildName",requestSuccess,NetManager.RequestFail);
	}

    void RefreshUILabel(int index, GameObject grid)
    {
        GuildBaseObject guild = new GuildBaseObject();
        if (guildArr.Length > index)
        {
            guild = guildArr[index];
        }
        if (guild != null)
        {
            GameCommon.FindComponent<UILabel>(grid, "union_name").text = guild.name;
            GameCommon.FindComponent<UILabel>(grid, "union_outInfo").text = guild.outInfo;
            GameCommon.FindComponent<UILabel>(grid, "union_memberCount").text = guild.memberCount.ToString() + "/" + guild.memberLimit;
            GameCommon.FindComponent<UILabel>(grid, "union_full").enabled = guild.isFull;
            GameCommon.FindComponent<UILabel>(grid, "level_label").text = guild.level.ToString();

            if (!guild.isFull)
            {
                bool hasAppointed = false;
                if (hasAppointedArr != null)
                {
                    for (int i = 0; i < hasAppointedArr.Length; i++)
                    {
                        if (hasAppointedArr[i] == guild.guildId)
                        {
                            hasAppointed = true;
                            break;
                        }
                    }
                }

                GameCommon.FindComponent<UILabel>(grid, "union_joinUs").enabled = !hasAppointed;
                GameCommon.FindComponent<UILabel>(grid, "union_hasAppointed").enabled = hasAppointed;

                int reqType = hasAppointed ? 2 : 1;
                AddButtonAction(GameCommon.FindObject(grid, "join_button"), () =>
                {
                    CS_GuildApplyJoinOrCancel cs = new CS_GuildApplyJoinOrCancel(guild.guildId, reqType);

                    HttpModule.CallBack requestSuccess_apply = (text) =>
                    {
                        var sc = JCode.Decode<SC_GuildApplyJoinOrCancel>(text);
                        if (sc.appliable == 0) DataCenter.OpenMessageWindow(TableCommon.getStringFromStringList(STRING_INDEX.UNION_NOT_APPLY_TIME));
                        if (sc.isFull == 1) DataCenter.OpenMessageWindow("该宗门已满");
                        OnOpen();
                    };
                    HttpModule.Instace.SendGameServerMessage(cs, "CS_GuildApplyJoinOrCancel", requestSuccess_apply, NetManager.RequestFail);
                });
            }
            else
            {
                GameCommon.FindComponent<UILabel>(grid, "union_joinUs").enabled = false;
                GameCommon.FindComponent<UILabel>(grid, "union_hasAppointed").enabled = false;
                GameCommon.FindComponent<UIImageButton>(grid, "join_button").enabled = false;
            }
        }
    }
}