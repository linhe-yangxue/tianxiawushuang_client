using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

class UnionInfoNewsWindow : UnionBase
{
    const string QUIT = "退出";
    const string LOOK = "查看";
    const string KICK = "踢出";
    const string IMPEACH = "弹劾";
    const string APPOINT = "任命";

    Vector2 _oneButtonPos = new Vector2(240, -15);
    Vector2 _buttonPos_1 = new Vector2(240, 30);
    Vector2 _buttonPos_2 = new Vector2(240, -30);

    GameObject mInfoBoard;
    GameObject mNewsBoard;
    UILabel mCurExp;
    UILabel mNextExp;
    UISlider mExpSlider;
    UILabel mGuildName;
    UILabel mGuildLevel;
    UILabel mGuildPresident;
    UILabel mCurMember;
    UILabel mMemberLimit;
    UILabel mInInfoLabel;
    UILabel mOutInfoLabel;
    UILabel mNewsLabel;

    protected override void OpenInit() {
        base.OpenInit();
        AddButtonAction("info", RefreshInfo);
        AddButtonAction("news", RefreshNews);
        

        mInfoBoard = GetCurUIGameObject("infoBoard");
        mNewsBoard = GetCurUIGameObject("newsBoard");
        mCurExp = GetUILabel("curExp");
        mNextExp = GetUILabel("nextExp");
        mExpSlider = GetUISlider("expSlider");
        mGuildName = GetUILabel("guildName");
        mGuildLevel=GetUILabel("guildLevel");
        mGuildPresident = GetUILabel("GuildPresident");
        mCurMember = GetUILabel("curMember");
        mMemberLimit = GetUILabel("memberLimit");
        mInInfoLabel = GetUILabel("inInfoLabel");
        mOutInfoLabel = GetUILabel("outInfoLabel");
        mNewsLabel = GetUILabel("newsLabel");  
    }

    public override void OnOpen() {
        base.OnOpen();
        GetUIToggle("info", true);
        GetUIToggle("news", false);
        RefreshInfo();
        DataCenter.OpenBackWindow(UIWindowString.union_infoNews, "a_ui_zongmengdt_logo", () =>
        {
            HttpModule.CallBack requestSuccess_outInfo = text =>
            {
                var item = JCode.Decode<SC_ChangeGuildOutInfo>(text);
                UnionBase.InGuildThenDo(item);
            };
            if (myMember.title == UnionTitle.PRESIDENT || myMember.title == UnionTitle.VICE_PRESIDENT)
            {
                HttpModule.Instace.SendGameServerMessageT<CS_ChangeGuildInInfo>(new CS_ChangeGuildInInfo(guildId, mOutInfoLabel.text, mInInfoLabel.text), requestSuccess_outInfo, NetManager.RequestFail);
            }

            DataCenter.OpenWindow(UIWindowString.union_main);
        });

        //new mark
        SetInfoFalse();
    }

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange (keyIndex,objVal);
		if (keyIndex == "REFRESH_INFO") {
			RefreshInfoData();
		}
	}

    public override void OnClose() {
        base.OnClose();
        DataCenter.CloseWindow(UIWindowString.common_back);
    }

	void RefreshInfoData() {
		CS_GetGuildMemberArr cs = new CS_GetGuildMemberArr(guildId);
		HttpModule.CallBack requestSuccess = (text) => {
			var item = JCode.Decode<SC_GetGuildMemberArr>(text);
			Action action = () => {
				SetGuildInfo(item);
				DataCenter.SetData(UIWindowString.union_worship, "UPDATE_GONGXIAN", null);
			};
			
			UnionBase.InGuildThenDo(item, action);           
		};
		HttpModule.Instace.SendGameServerMessage(cs, "CS_GetGuildMemberArr", requestSuccess, NetManager.RequestFail);
	}

    void RefreshInfo() {
		if(mInfoBoard != null)
        	mInfoBoard.SetActive(true);
		if(mNewsBoard != null)
        	mNewsBoard.SetActive(false);
        
        CS_GetGuildMemberArr cs = new CS_GetGuildMemberArr(guildId);
        HttpModule.CallBack requestSuccess = (text) => {
            var item = JCode.Decode<SC_GetGuildMemberArr>(text);
            Action action = () => {
                SetGuildInfo(item);
                mCurExp.text = guildBaseObject.exp.ToString();
                mNextExp.text = guildBaseObject.nextExp.ToString();
                mExpSlider.value = guildBaseObject.expPercent;
                mGuildName.text = guildBaseObject.name;
                mGuildLevel.text=guildBaseObject.level.ToString();
                mCurMember.text = guildBaseObject.memberCount.ToString();
                mMemberLimit.text = guildBaseObject.memberLimit.ToString();
                mInInfoLabel.text = guildBaseObject.inInfo;
                mOutInfoLabel.text = guildBaseObject.outInfo;
                mGuildPresident.text = memberArr.Where(member => member.title == UnionTitle.PRESIDENT).SingleOrDefault().nickName;
                GameCommon.FindComponent<UIScrollView>(mGameObjUI, "member_scrollview").ResetPosition();
                var list = GetUIGridContainer("member_list", memberArr.Length).controlList;
                for (int i = 0; i < memberArr.Length; i++) RefreshMemberLabel(list[i], memberArr[i]);
            };

            UnionBase.InGuildThenDo(item, action);           
        };
        HttpModule.Instace.SendGameServerMessage(cs, "CS_GetGuildMemberArr", requestSuccess, NetManager.RequestFail);
    }

    void RefreshNews() {
        mInfoBoard.SetActive(false);
        mNewsBoard.SetActive(true);
        GetNews(mNewsLabel);
    }

    void RefreshMemberLabel(GameObject grid, GuildMember member) {
        GameCommon.FindComponent<UILabel>(grid, "name").text = member.nickName;
        GameCommon.FindComponent<UILabel>(grid, "level").text = member.level.ToString();
        GameCommon.FindComponent<UILabel>(grid, "power").text = member.power.ToString();
        GameCommon.FindComponent<UILabel>(grid, "todayWorship").text = member.todayWorship.ToString();
        GameCommon.FindComponent<UILabel>(grid, "unionContr").text = member.unionContr.ToString();
        GameCommon.FindComponent<UILabel>(grid,"outTime").text=(member.outTime>0)?string.Format("离线{0}小时",((float)member.outTime/3600).ToString("f1")):"在线";
        GameCommon.SetOnlyItemIcon(grid,"playerIcon",member.iconIndex);


        UILabel _unionTitleLbl = GameCommon.FindComponent<UILabel>(grid, "title");
        UISprite _unionTitleSprite = GameCommon.FindComponent<UISprite>(grid, "union_title_sprite");
        _unionTitleLbl.gameObject.SetActive(false);
        _unionTitleSprite.gameObject.SetActive(false);

        switch (member.title) 
        { 
            case UnionTitle.PRESIDENT:
                _unionTitleSprite.gameObject.SetActive(true);
                _unionTitleSprite.spriteName = "a_ui_zhangmeng";
                break;
            case UnionTitle.VICE_PRESIDENT: 
                _unionTitleSprite.gameObject.SetActive(true);
                _unionTitleSprite.spriteName = "a_ui_fuzhangmeng";
                break;
            case UnionTitle.MEMBER: 
                _unionTitleLbl.gameObject.SetActive(true);
                _unionTitleLbl.text = "成员";
                break;
        }
    
        List<string> list = new List<string>();
        if (IsMySelf(member)) list.Add(QUIT);
        else {
            list.Add(LOOK);
            if (myTitle == UnionTitle.PRESIDENT)
                list.Add(APPOINT);
            else if (myTitle == UnionTitle.VICE_PRESIDENT) {
                if (member.title == UnionTitle.PRESIDENT) list.Add(IMPEACH);
                else if (member.title == UnionTitle.MEMBER) list.Add(KICK);
            }
            else if (myTitle == UnionTitle.MEMBER) {
                if (member.title == UnionTitle.PRESIDENT) list.Add(IMPEACH);
            }
        }
        SetButton(grid, member, list.ToArray());
    }


    void SetButton(GameObject grid, GuildMember tarMember, params string[] buttonTypeArr) {
        var button_1 = GameCommon.FindObject(grid, "button_1");
        var button_2 = GameCommon.FindObject(grid, "button_2");
        if (buttonTypeArr.Length == 1) {
            button_1.SetActive(true);
            button_2.SetActive(false);
            button_1.transform.localPosition = _oneButtonPos;
        }
        else {
            button_1.SetActive(true);
            button_2.SetActive(true);
            button_1.transform.localPosition = _buttonPos_1;
            button_2.transform.localPosition = _buttonPos_2;
        }

        GameObject[] buttonGOArr = new GameObject[2] { button_1, button_2 };

      
        for (int i = 0; i < buttonTypeArr.Length; i++) {

            switch (buttonTypeArr[i]) {
                case QUIT: AddButtonAction(buttonGOArr[i], ()=>{
                    if (myMember.title != UnionTitle.PRESIDENT) 
                        FunctionHelper.GetButtonAction("退出宗门24小时之内，无法加入宗门，确定要退出吗？", GetQuitAction(myMember.uid))();
                    else {
                        if (guildBaseObject.memberCount <= 10)
                        {
                            Action action=()=>
                            {
                                HttpModule.CallBack requestSuccess_dismiss = (text) => 
                                {
                                    DEBUG.Log("RequestSuccess:text = " + text);
                                    SC_GuildDissolution item = JCode.Decode<SC_GuildDissolution>(text);
                                    UnionBase.InGuildThenDo(item, () => {
                                        DataCenter.CloseWindow(UIWindowString.union_infoNews);
										MainUIScript.Self.ShowMainBGUI();
                                        RoleLogicData.Self.guildId = "";
                                    });
                               };
                                CS_GuildDissolution cs_dismiss = new CS_GuildDissolution(guildId);
                                HttpModule.Instace.SendGameServerMessage(cs_dismiss, "CS_GuildDissolution", requestSuccess_dismiss, NetManager.RequestFail);
                            };
                           DataCenter.OpenMessageOkWindow("确定解散宗门吗？",action);
                        }
                           
                        else DataCenter.OpenMessageWindow("宗门成员大于十人禁止解散！");
                    }
                }); 
                    break;
                case LOOK:
                    ChatWindowOpenData chatData=new ChatWindowOpenData(ChatType.Private,tarMember.uid,tarMember.nickName,() => {
                        mGameObjUI.SetActive(true);
                    });

                    Button_visit_friend_button btn=new Button_visit_friend_button();
                    btn.set("FRIEND_ID",tarMember.uid);
                    btn.set("FRIEND_NAME",tarMember.nickName);
                    btn.set("WINDOW_NAME",UIWindowString.union_infoNews);

                    MsgBoxBtnInfo[] btnInfoArr = new MsgBoxBtnInfo[4] {
                        new MsgBoxBtnInfo("私聊",()=>{
                            mGameObjUI.SetActive(false);
                            DataCenter.OpenWindow("CHAT_WINDOW",chatData);
						    //延迟一帧 跳转到私聊栏位
						    GlobalModule.DoCoroutine(__GoToChatWindow()); 
                        }),
                        new MsgBoxBtnInfo("切磋",()=>DataCenter.OpenMessageWindow("此功能暂不开放")),
                        new MsgBoxBtnInfo("查看阵容",()=>btn._DoEvent()),
                        new MsgBoxBtnInfo("添加好友",()=>{
                            HttpModule.CallBack requestSuccess=text => {
                                var item=JCode.Decode<SC_SendFriendRequest>(text);
                                if(item.friendReqFull==1) DataCenter.OpenMessageWindow("该玩家好友已满");
                                else {
                                    if(item.friendsAlready==1) DataCenter.OpenMessageWindow("该玩家已经是好友");
                                    else DataCenter.OpenMessageWindow("好友申请已发送");
                                }
                            };
                            CS_SendFriendRequest cs=new CS_SendFriendRequest(tarMember.uid.ToString());
                            HttpModule.Instace.SendGameServerMessageT(cs,requestSuccess,NetManager.RequestFail);
                        }),
                    };

				MsgBoxInfo lookInfo = new MsgBoxInfo(btnInfoArr, tarMember.nickName, "玩家信息", "[d8c29f]等 级: [-] " + tarMember.level + "\r\n" + "[d8c29f]战斗力:[-]  " + tarMember.power, tarMember.iconIndex);
                    AddButtonAction(buttonGOArr[i], () => DataCenter.OpenWindow(UIWindowString.dynamic_MsgBox_4,lookInfo));
                    break;

                case KICK: AddButtonAction(buttonGOArr[i], FunctionHelper.GetButtonAction("确定将此成员移出宗门？", GetKickAction(tarMember.uid))); break;
               
                case IMPEACH:
                    AddButtonAction(buttonGOArr[i], FunctionHelper.GetButtonAction("确定弹劾掌门？", ImpeachAction));
                    break;
                case APPOINT:
                    MsgBoxBtnInfo btnInfo_1 = new MsgBoxBtnInfo("任命掌门", FunctionHelper.GetButtonAction("是否将此宗门移交给此玩家？", GetAppointAction(tarMember.uid, UnionTitle.PRESIDENT)));
                    MsgBoxBtnInfo btnInfo_2;
                    if (tarMember.title == UnionTitle.VICE_PRESIDENT)
                        btnInfo_2 = new MsgBoxBtnInfo("任命堂主", ()=>DataCenter.OpenMessageWindow("该玩家已经是堂主"));
                    else btnInfo_2 = new MsgBoxBtnInfo("任命堂主",FunctionHelper.GetButtonAction("是否将此玩家升为堂主？", GetAppointAction(tarMember.uid, UnionTitle.VICE_PRESIDENT)));


                    MsgBoxBtnInfo btnInfo_3;
                    if (tarMember.title != UnionTitle.VICE_PRESIDENT)
                        btnInfo_3 = new MsgBoxBtnInfo("罢免堂主", () => DataCenter.OpenMessageWindow("此成员不可罢职"));
                    else btnInfo_3=new MsgBoxBtnInfo("罢免堂主",FunctionHelper.GetButtonAction("是否将此玩家罢职？", GetRecallAction(tarMember.uid, UnionTitle.VICE_PRESIDENT)));

                    MsgBoxBtnInfo btnInfo_4 = new MsgBoxBtnInfo("踢出成员",GetKickAction(tarMember.uid));
                    MsgBoxBtnInfo[] arr = new MsgBoxBtnInfo[4] { btnInfo_1, btnInfo_2, btnInfo_3, btnInfo_4 };

				MsgBoxInfo info = new MsgBoxInfo(arr, tarMember.nickName, "任命书", "[d8c29f]等 级:  " + tarMember.level + "\r\n" + "[d8c29f]战斗力:  " + tarMember.power, tarMember.iconIndex); 
                    AddButtonAction(buttonGOArr[i], () =>  DataCenter.OpenWindow(UIWindowString.dynamic_MsgBox_4,info));
                    break;
            }
            GameCommon.FindComponent<UILabel>(buttonGOArr[i], "button_label").text = buttonTypeArr[i];
        }
    }

	private IEnumerator __GoToChatWindow()
	{
		yield return new WaitForEndOfFrame();
		DataCenter.SetData ("CHAT_WINDOW", "SHOW_WINDOW", "CHAT_PRIVATE_WINDOW");
	}

}


