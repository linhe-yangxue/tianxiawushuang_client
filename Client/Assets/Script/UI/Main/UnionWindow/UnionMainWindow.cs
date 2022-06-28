using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

class UnionMainWindow : UnionBase
{
    //bool isInUnion = false;  进入主界面就向服务器发消息不考虑是否是从其他界面退后到的

    protected override void OpenInit() {
        base.OpenInit();
        Func<string, Action> ToOtherWindow = (tarWindowName) =>
        {
            return () =>
            {
                DataCenter.CloseWindow(UIWindowString.union_main);
                DataCenter.OpenWindow(tarWindowName);
                if (tarWindowName == UIWindowString.union_pk_enter_window)
                {
                    DataCenter.SetData(tarWindowName, "UNION_PKENTER_FIRSTIN", null);
                }
            };
        };

        AddButtonAction("shop", ToOtherWindow(UIWindowString.union_shopContainer));
        AddButtonAction("worship", ToOtherWindow(UIWindowString.union_worship));
        AddButtonAction("infoNews", ToOtherWindow(UIWindowString.union_infoNews));
        AddButtonAction("unionPK", ToOtherWindow(UIWindowString.union_pk_enter_window));
        AddButtonAction("rank", ()=>DataCenter.OpenWindow("RANKLIST_GUILD_WINDOW"));

        UIScrollView _scrollView = GetCurUIComponent<UIScrollView>("Scroll View");
        if (_scrollView != null)
        {
            _scrollView.ResetPosition();
        }
    }

    public override void OnOpen() {
        base.OnOpen();
		
        DataCenter.OpenBackWindow(UIWindowString.union_main,"a_ui_zongmeng_logo",() => MainUIScript.Self.ShowMainBGUI(),120);
        
        CS_GetGuildMemberArr cs = new CS_GetGuildMemberArr(guildId);
        HttpModule.CallBack requestSuccess = (text) => {
            DEBUG.Log("RequestSuccess:text = " + text);
            var item = JCode.Decode<SC_GetGuildMemberArr>(text);
            Action action = () => {
                SetGuildInfo(item);
                bool flag = myTitle == UnionTitle.PRESIDENT;
                GetCurUIGameObject("dismiss", flag);
                GetCurUIGameObject("apply", flag);
                GetUILabel("inInfo").text = guildBaseObject.inInfo;              
                if (DataCenter.Get("UNION_SHOP_OPEN_FLAG") == 1) 
                {
                    DataCenter.OpenWindow(UIWindowString.union_shopContainer);
                }
                DataCenter.Set("UNION_SHOP_OPEN_FLAG", 0);
            };

            UnionBase.InGuildThenDo(item, action);
        };
        HttpModule.Instace.SendGameServerMessage(cs, "CS_GetGuildMemberArr", requestSuccess, NetManager.RequestFail);

        HttpModule.CallBack requestSuccess_dismiss = (text) => {
            DEBUG.Log("RequestSuccess:text = " + text);
            SC_GuildDissolution item = JCode.Decode<SC_GuildDissolution>(text);
            UnionBase.InGuildThenDo(item, () => {
                DataCenter.CloseWindow(UIWindowString.union_main);
				MainUIScript.Self.ShowMainBGUI();
                RoleLogicData.Self.guildId = "";
            });
            
        };

        AddButtonAction("dismiss", () => {
            if(guildBaseObject.memberCount<=10) {
                Action action=() => {
                    CS_GuildDissolution cs_dismiss=new CS_GuildDissolution(guildId);
                    HttpModule.Instace.SendGameServerMessage(cs_dismiss,"CS_GuildDissolution",requestSuccess_dismiss,NetManager.RequestFail);
                };

                DataCenter.OpenMessageOkWindow("确定解散宗门吗？",action);
            } else DataCenter.OpenMessageWindow("宗门成员大于十人禁止解散！"); 
        });

        AddButtonAction("apply", () => DataCenter.OpenWindow(UIWindowString.union_check));

        //new mark
        ShowUnionMainNewMark();
    }

    public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex,objVal);
        switch (keyIndex)
        {
            case "REFRESH_NEW_MARK_APPLY":
                {
                    ShowApplyNewMark();
                }
                break;
        }
	}
    
    public override void OnClose() {
        base.OnClose();
        DataCenter.CloseWindow(UIWindowString.common_back);
    }

    public void ShowUnionMainNewMark()
    {
        string strPath = "Scroll View/entrance_infos/";

        //增加判空处理
        Transform tmpTrans = mGameObjUI.transform.Find(strPath + "shop/title_sprite/NewMark");
        if (tmpTrans != null && tmpTrans.gameObject != null)
            tmpTrans.gameObject.SetActive(IsShowShopNewMark());
        tmpTrans = mGameObjUI.transform.Find(strPath + "worship/title_sprite/NewMark");
        if (tmpTrans != null && tmpTrans.gameObject != null)
            tmpTrans.gameObject.SetActive(IsShowWorkshipNewMark());
        tmpTrans = mGameObjUI.transform.Find(strPath + "infoNews/title_sprite/NewMark");
        if (tmpTrans != null && tmpTrans.gameObject != null)
            tmpTrans.gameObject.SetActive(IsShowInfoNewsNewMark());
        tmpTrans = mGameObjUI.transform.Find(strPath + "unionPK/title_sprite/NewMark");
        if (tmpTrans != null && tmpTrans.gameObject != null)
            tmpTrans.gameObject.SetActive(GetGuildBossRedPoint());

        //apply
        ShowApplyNewMark();
    }

    public void ShowApplyNewMark()
    {
        //by chenliang
        //begin

//        mGameObjUI.transform.Find("btn_group/apply/NewMark").gameObject.SetActive(IsShowaApplyNewMark());
//---------------
        //增加判空处理
        Transform tmpTrans = mGameObjUI.transform.Find("btn_group/apply/NewMark");
        if (tmpTrans != null && tmpTrans.gameObject != null)
            tmpTrans.gameObject.SetActive(IsShowaApplyNewMark());

        //end
    }
}
