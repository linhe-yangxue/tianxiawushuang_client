using UnityEngine;
using System.Collections;

public class MailNetRequester {

	public static void RequestMailList(HttpModule.CallBack successCallback, HttpModule.CallBack failCallback) {
		CS_ReqMails mail = new CS_ReqMails();

		HttpModule.Instace.SendGameServerMessage(mail, "CS_ReqMails", successCallback, failCallback);
	}


	public static void RequestMailItem(long mailId, HttpModule.CallBack successCallback, HttpModule.CallBack failCallback) {
		CS_RequestMailItem mail = new CS_RequestMailItem();
		mail.mailId = mailId;

		HttpModule.Instace.SendGameServerMessage(mail, "CS_GetItem", successCallback, failCallback);
	}


	public static void RequestMailAllItem(HttpModule.CallBack success, HttpModule.CallBack fail) {
		CS_RequestMailAllItem mails = new CS_RequestMailAllItem();

		HttpModule.Instace.SendGameServerMessage(mails, "CS_GetAllItem", success, fail);
	}
}
