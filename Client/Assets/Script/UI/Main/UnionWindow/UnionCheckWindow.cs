using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Logic;
public class Button_union_check_window : CEvent
{
    public override bool _DoEvent()
    {
        DataCenter.CloseWindow(UIWindowString.union_check);
        return true;
    }
}
class UnionCheckWindow : UnionBase
{
    UILabel mAppointCount;
    UILabel mGuildMemberLimit;
    UIGridContainer mBoardContainer;
    Dictionary<string, GuildMemberBaseObject> mMemberDict;

    protected override void OpenInit() {
        base.OpenInit();
        mAppointCount = GetUILabel("appointCount");
        mGuildMemberLimit = GetUILabel("guildMemberLimit");
        mBoardContainer = GetUIGridContainer("boardList");
        AddButtonAction("back", () => DataCenter.CloseWindow(UIWindowString.union_check));
        EventCenter.Self.RegisterEvent("Button_union_check_window", new DefineFactory<Button_union_check_window>());
    }

    public override void OnOpen() {
        base.OnOpen();
        CS_GetApplyMemberArr cs = new CS_GetApplyMemberArr(guildId);
        HttpModule.CallBack requestSuccess = (text) => {
            var sc = JCode.Decode<SC_GetApplyMemberArr>(text);
            mMemberDict = new Dictionary<string, GuildMemberBaseObject>();
            for (int i = 0; i < sc.arr.Length; i++) mMemberDict.Add(sc.arr[i].zuid, sc.arr[i]);
            
            RefreshBoardContainer();
            mAppointCount.text = sc.arr.Length.ToString();
            mGuildMemberLimit.text = guildBaseObject.memberLimit.ToString();

        };
        HttpModule.Instace.SendGameServerMessage(cs, "CS_GetApplyMemberArr", requestSuccess, NetManager.RequestFail);

    }

    void RefreshBoardContainer() {
        mBoardContainer.MaxCount = mMemberDict.Count;
        int j = 0;
        foreach (var member in mMemberDict.Values) {
            SetMemberBoard(mBoardContainer.controlList[j], member);
            j++;
        } 
    }

    void SetMemberBoard(GameObject board, GuildMemberBaseObject member) {
        Action<string, string> setLabel = (labelName, text) => 
            GameCommon.FindComponent<UILabel>(board, labelName).text = text;
        setLabel("memberName", member.nickname);
        setLabel("power", member.power.ToString());
        setLabel("level", member.level.ToString());

		//set icon
		GameObject obj = GameCommon.FindObject (board, "item_icon");
		UISprite sprite = GameCommon.FindComponent<UISprite> (obj, "background_sprite");
		if (sprite != null) {
			GameCommon.SetPalyerIcon (sprite, member.iconIndex);
		}

        AddButtonAction(GameCommon.FindObject(board, "agree"), () => {
            HttpModule.CallBack requestSuccess = text => {
                var item = JCode.Decode<SC_GuildAddMember>(text);
                UnionBase.InGuildThenDo(item, () => {
                    if(item.memberAddGuild==0) {
                        mMemberDict.Remove(member.zuid);
                    } else DataCenter.OpenMessageWindow("已加入宗门");
                    if(item.isFull==1) DataCenter.OpenMessageWindow("宗门人数已满");
                    OnOpen();
                    SetApplyFalse();
                });
            };

            CS_GuildAddMember cs = new CS_GuildAddMember(guildId, member.zuid);
            HttpModule.Instace.SendGameServerMessage(cs, "CS_GuildAddMember", requestSuccess, NetManager.RequestFail);
        });

        AddButtonAction(GameCommon.FindObject(board, "refuse"), () => {
            HttpModule.CallBack requestSuccess = text => {
                var item = JCode.Decode<SC_GuildRefuseMember>(text);
                UnionBase.InGuildThenDo(item, () => {
                    mMemberDict.Remove(member.zuid);
                    OnOpen();
                });
            };

            CS_GuildRefuseMember cs = new CS_GuildRefuseMember(guildId, member.zuid);
            HttpModule.Instace.SendGameServerMessage(cs, "CS_GuildRefuseMember", requestSuccess, NetManager.RequestFail);
        });
    }

}
