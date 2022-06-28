using UnityEngine;
using System.Collections;

public class Notify_Shark_Common :MonoBehaviour
{

//	public static const string MSG_LOGIN="NotifyLogin";
//	public static const string MSG_LOGOUT="NotifyLogout";
//	public static const string MSG_PAYRESULT="NotifyPayResult";
	//登录响应
	public void NotifyLogin(string _in_data)
	{
		DEBUG.Log("u3d part notify login "+ _in_data);
		U3DSharkSDK.Instance.GetUserData().StringToData(_in_data);
		U3DSharkBaseData resultDat = U3DSharkSDK.Instance.GetUserData();
		U3DSharkSDK.Instance.SendEvent(SharkEventType.EVENT_LOGIN_SUCCESS,resultDat);
	}
	//登出响应
	public void NotifyLogout(string _in_data)
	{
		U3DSharkBaseData resultDat = new U3DSharkBaseData();
		resultDat.StringToData(_in_data);
		U3DSharkSDK.Instance.SendEvent(SharkEventType.EVENT_LOGOUT,resultDat);
	}
	//支付结果响应
	public void NotifyPayResult(string _in_data)
	{
		U3DSharkBaseData resultDat = new U3DSharkBaseData();
		resultDat.StringToData(_in_data);
		U3DSharkSDK.Instance.SendEvent(SharkEventType.EVENT_PAY_RESULT,resultDat);
	}
	//更新完毕响应
	public void NotifyUpdateFinish(string _in_data)
	{
		U3DSharkBaseData resultDat = new U3DSharkBaseData();
		resultDat.StringToData(_in_data);
		U3DSharkSDK.Instance.SendEvent(SharkEventType.EVENT_UPDATE_FINISH,resultDat);
	}
	//初始化完毕响应
	public void NotifyInitFinish(string _in_data)
	{
		U3DSharkBaseData resultDat = new U3DSharkBaseData();

		resultDat.StringToData(_in_data);
		U3DSharkSDK.Instance.SendEvent(SharkEventType.EVENT_INIT_FINISH,resultDat);
	}
	//重新登录响应
	public void NotifyRelogin(string _in_data)
	{
		U3DSharkBaseData resultDat = new U3DSharkBaseData();
		resultDat.StringToData(_in_data);
		U3DSharkSDK.Instance.SendEvent(SharkEventType.EVENT_RELOGIN,resultDat);
	}
	public void NotifyCancelExitGame(string _in_data)
	{
		U3DSharkBaseData resultDat = new U3DSharkBaseData();
		resultDat.StringToData(_in_data);
		U3DSharkSDK.Instance.SendEvent(SharkEventType.EVENT_CANCEL_EXIT_GAME,resultDat);
	}

	/**收到本地推送相应（非必接）*/
	public void NotifyReceiveLocalPush(string _in_data)
	{
		U3DSharkBaseData resultDat = new U3DSharkBaseData();
		resultDat.StringToData(_in_data);
		U3DSharkSDK.Instance.SendEvent(SharkEventType.EVENT_RECEIVE_LOCAL_PUSH,resultDat);
	
	}

	public void NotifyUserFriends(string _json_string)
	{
		U3DSharkBaseData resultDat = new U3DSharkBaseData();
		resultDat.StringToData(_json_string);
		U3DSharkSDK.Instance.SendEvent(SharkEventType.EVENT_GET_FRIEND_RESULT,resultDat);
	}
	public void NotifyShareResult(string _json_string)
	{
		U3DSharkBaseData resultDat = new U3DSharkBaseData();
		resultDat.StringToData(_json_string);
		U3DSharkSDK.Instance.SendEvent(SharkEventType.EVENT_SHARE_RESULT,resultDat);
	}
    void NotifyExtraFunction(string _json_string)
    {
        U3DSharkBaseData resultDat = new U3DSharkBaseData();
        resultDat.StringToData(_json_string);
        U3DSharkSDK.Instance.SendEvent(SharkEventType.EVENT_EXTRA_FUNCTION,resultDat);
    }
}
