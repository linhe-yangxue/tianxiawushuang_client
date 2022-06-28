using UnityEngine;
using System.Collections;

public class FriendNetEvent {

	static public void RequestFriendRequestList(HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestFriendRequestList rList = new CS_RequestFriendRequestList();
		HttpModule.Instace.SendGameServerMessage(rList, "CS_GetFriendRequestList", success, fail);
	}

	// 获得已经添加好的好有列表
	static public void RequestFriendList(HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestFriendListHttp fList = new CS_RequestFriendListHttp();
		HttpModule.Instace.SendGameServerMessage(fList, "CS_GetFriendList", success, fail);
	}

    // 搜索好友
	static public void RequestSearchFriend(string name, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_SearchFriendInfo searchList = new CS_SearchFriendInfo();
		searchList.name = name;
		HttpModule.Instace.SendGameServerMessage(searchList, "CS_SearchFriendInfo", success, fail);
	}

    // 添加好友申请
	static public void RequestAddFriendRequest(string fId, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_SendFriendRequest addF = new CS_SendFriendRequest();
		addF.friendId = fId;
		HttpModule.Instace.SendGameServerMessage(addF, "CS_SendFriendRequest", success, fail);
	} 

     // 同意添加好友
	static public void RequestAcceptFriend(string fId, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestAcceptFriend acceptF = new CS_RequestAcceptFriend();
		acceptF.friendId = fId;
		HttpModule.Instace.SendGameServerMessage(acceptF, "CS_AcceptFriendRequest", success, fail);
	}

    // 拒绝添加好友
	static public void RequestRejectFriendRequest(string fId, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestRejectFriend rejectF = new CS_RequestRejectFriend();
		rejectF.friendId = fId;
		HttpModule.Instace.SendGameServerMessage(rejectF, "CS_RejectFriendRequest", success, fail);
	}

    // 删除好友
	static public void RequestDeleteFriend(string fId, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestDeleteFriend deleteF = new CS_RequestDeleteFriend();
		deleteF.friendId = fId;
		HttpModule.Instace.SendGameServerMessage(deleteF, "CS_DeleteFriend", success, fail);
	}

	static public void RequestRecommendFriendList(HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestFriendRecommendList rList = new CS_RequestFriendRecommendList();
		HttpModule.Instace.SendGameServerMessage(rList, "CS_GetFriendRcmdList", success, fail);
	}

    // 获取已经我赠送过精力的好友的列表
	static public void RequestGetSpiritSendList(HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestGetSpiritSendList sendList = new CS_RequestGetSpiritSendList();

		HttpModule.Instace.SendGameServerMessage(sendList, "CS_GetSpiritSendList", success, fail);
	}

    // 赠送精力请求
	static public void RequestSendSpirit(string friendId, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestSendSpirit sendSpirit = new CS_RequestSendSpirit();
		sendSpirit.friendId = friendId;
		HttpModule.Instace.SendGameServerMessage(sendSpirit, "CS_SendSpirit", success, fail);
	}

    // 获取赠送给我精力的好友列表
    static public void RequestSendMeSpiritList(HttpModule.CallBack success, HttpModule.CallBack fail)
    {
        CS_RequestSendMeSpiritList send = new CS_RequestSendMeSpiritList();
        HttpModule.Instace.SendGameServerMessage(send, "CS_GetSpiritRcvList", success, fail);
    }

    // 领取精力
    static public void RequestAquireSpirit(string[] friendID, HttpModule.CallBack success, HttpModule.CallBack fail)
    {
        CS_RequestAquireSpirit cs = new CS_RequestAquireSpirit();
        cs.friendIds = friendID;
        HttpModule.Instace.SendGameServerMessage(cs, "CS_RcvSpirit", success, fail);
    }

    // 访问好友请求
	static public void RequestVisitPlayer(string targetId, HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestVisitPlayer visitPalyer = new CS_RequestVisitPlayer();
		visitPalyer.targetId = targetId;
		HttpModule.Instace.SendGameServerMessage(visitPalyer, "CS_VisitPlayer", success, fail);
	}
}
