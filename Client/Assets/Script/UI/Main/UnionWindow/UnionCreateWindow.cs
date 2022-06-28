using UnityEngine;
using System.Collections;
using Logic;
using System;

public class Button_union_create_window : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.union_create);
        return true;
    }
}
class UnionCreateWindow : UnionBase
{

    protected override void OpenInit()
    {
        base.OpenInit();
        AddButtonAction("action_createUnion_btn", CreateUnion);
        AddButtonAction("action_back_btn", () => 
        { 
            DataCenter.CloseWindow(UIWindowString.union_create);
            //DataCenter.CloseWindow(UIWindowString.union_list);

			//MainUIScript.Self.ShowMainBGUI();
        });
        EventCenter.Self.RegisterEvent("Button_union_create_window", new DefineFactory<Button_union_create_window>());
    }

    public override void OnOpen()
    {
        base.OnOpen();
//        GetUIInput("nameInput").value="";
//        GetUIInput("infoInput").value="";
    }

    void CreateUnion() 
    {
        if (RoleLogicData.Self.diamond < 500) {
            //DataCenter.OpenMessageWindow("元宝不足");
			GameCommon.ToGetDiamond();
            return;
        }
        var name = GetUIInput("nameInput").value;
        var info = GetUIInput("infoInput").value;
        if(!isInOutInfoLimit(info)) DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_UNIONINFO_TOO_LONG);
        ShowIllegalTips(name);
        if(isInNameLimit(name)&&isInOutInfoLimit(info)&&!isAllNumber(name)&&!isEmpty(name)&&!isHasSpecialChar(name))
        {
            CS_CreateGuild cs=new CS_CreateGuild(name,info);
            HttpModule.CallBack requestSuccess = text => {
                var item = JCode.Decode<SC_CreateGuild>(text);
                if(item.guildExist==0) {
                    SetGuildId(item.guildObject.guildId);
                    DataCenter.CloseWindow(UIWindowString.union_create);
                    DataCenter.CloseWindow(UIWindowString.union_list);
                    DataCenter.OpenWindow(UIWindowString.union_main);
                    PackageManager.RemoveItem((int)ITEM_TYPE.YUANBAO,-1,500);
                } else DataCenter.OpenMessageWindow("宗门已存在");
            };
            
            HttpModule.Instace.SendGameServerMessage(cs, "CS_CreateGuild", requestSuccess, NetManager.RequestFail);
        }
    }

    
    
}
