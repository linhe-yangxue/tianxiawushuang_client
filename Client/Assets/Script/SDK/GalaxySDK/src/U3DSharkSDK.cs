﻿// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
//  skd接口类
//
//  在使用 登录 登出 购买 等之类接口前 请务必设置平台所需参数
//  使用例子：

//  当前sdk当成功更新，登录，登出，支付后 都会发送消息
//  接受消息的例子请看demo
//  消息会发出一个 sharkeventdata的对象 其中的data 当在登录时，是sharklogindata
//					     当在支付后 是 sharkpayresultdata
//
//使用的例子：
//		U3DSharkSDK.Instance.platformData.appID = 4597;
//		U3DSharkSDK.Instance.platformData.appKey = "07c05293112bf3c4d090ebc330044ed0";
//		U3DSharkSDK.Instance.platformData.evtDelegate +=NotifySharkLogin;
//		U3DSharkSDK.Instance.platformData.IsOpenRecharge = true;
//
//
//U3DSharkSDK.Instance.platformData.evtDelegate +=NotifySharkLogin;//添加事件侦听
//void NotifySharkLogin(U3DSharkEvent evt)
//	{
//		System.Console.WriteLine ("notify event type " + evt.evtType);
//		if(evt!=null && evt.evtType == SharkEventType.EVENT_LOGIN_SUCCESS)
//		{
//			System.Console.WriteLine("success notify degele do login success");
//			U3DSharkPayData payData = new U3DSharkPayData ();
//			payData.realPrice = 2;
//			U3DSharkSDK.Instance.U3D_SharkSDKBuyItem (payData);
//			U3DSharkSDK.Instance.U3D_SharkSDKShowPersonCenter();
//		}
//	}
//
// ------------------------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Xml;

public class U3DSharkSDK :

	Notify_Shark_Common


{

	private static volatile U3DSharkSDK _instance; 
	private static object syncRoot = new object(); 
	private static  GameObject _container;

	private volatile U3DSharkBaseData _userData=null ;
//	public volatile U3DSharkPlatformData		platformData; 
	private Dictionary<SharkEventType,U3DSharkEventDelegate>	_delegateDic; 						
	private bool isInitSelf = false;
	private SharkBonjour bonjour = new SharkBonjour();
		

	private U3DSharkSDK():base()
	{

	}
	//获得sdk 实例U3DSharkSDK
	public static U3DSharkSDK Instance
	{
		get  
		{ 
			if(null == _instance)
			{
				_container = new GameObject();
				_container.name = "U3DSharkSDK";
				UnityEngine.Object.DontDestroyOnLoad(_container);
				lock(syncRoot)
				{
					if(null == _instance)
					{
						_instance = _container.AddComponent(typeof(U3DSharkSDK))as U3DSharkSDK;
                        DEBUG.LogError("is instance==null?"+(_instance==null).ToString());
//						bonjour = new SharkBonjour();
//						_instance = new U3DSharkSDK ();
						_instance._delegateDic = new Dictionary<SharkEventType, U3DSharkEventDelegate>();
					}
				}
			}
		return _instance; 
		} 
	}
	public void InitSDK()
	{
        U3DSharkEventListener.Instance.InitSelf();
		_instance.selfInit();
	}

	public U3DSharkBaseData GetUserData()
	{
		if(null == _userData)
			_userData = bonjour.GetUserData();

		return _userData;
	}
	public U3DSharkBaseData GetPlatformData()
	{
		return bonjour.GetPlatformData();
	}
	//显示登录平台的方法
	public void Login()
	{
		//if has not self init return and wait receive update event
		if(!selfInit ())
		{
			DEBUG.Log("U3DSharkSDK - Login - has not self init");
			return ;
		}
        DEBUG.Log("U3DSharkSDK - Login - do login ");
		bonjour.ShowLogin();
        DEBUG.Log("U3DSharkSDK - Login - do login finisth");

	}
	//登出平台
	public  void Logout()
	{
//		selfInit ();
		bonjour.ShowLogout();
	}

	public int  LoginState()
	{
		return bonjour.LoginState();

	}

	/**
	 * eg:
	 * 			payData.SetData(U3DSharkAttName.REAL_PRICE,"100");
			payData.SetData(U3DSharkAttName.ITEM_NAME,"sk bi");
			payData.SetData(U3DSharkAttName.ITEM_DESC,"desc");
			payData.SetData(U3DSharkAttName.ITEM_COUNT,"1");
			payData.SetData(U3DSharkAttName.ITEM_SEVER_ID,"id");
			payData.SetData(U3DSharkAttName.SEVER_ID,"1");
			payData.SetData(U3DSharkAttName.EXTRA,"extra");
			
	 * 支付函数
	 * @param _in_pay pay object 支付对象的结构体 若至少传入 一个 价格 
	 * 
	 * @return bill number
	 */
	public string PayItem(U3DSharkBaseData _in_pay)
	{
//		selfInit ();

        DEBUG.Log("U3D_shark sdk buy item");
		string  billNo =  bonjour.PayItem(_in_pay);

		return billNo;
	}
	//显示用户中心
	public void ShowPersonCenter()
	{
//		selfInit ();

		bonjour.ShowPersonCenter();
	}
	//隐藏用户中心（若平台sdk存在该方法）
	public void HidePersonCenter()
	{
//		selfInit ();
		bonjour.HidePersonCenter();
	}
	public void ShowToolBar()
	{
		bonjour.ShowToolBar();
	}
	public void HideToolbar()
	{
		bonjour.HideToolBar();
	}


	public void UpdatePlayerInfo()
	{
		U3DSharkBaseData cacheUser = GetUserData();
		DEBUG.Log("send player info : "+ cacheUser.DataToString());
		bonjour.SetPlayerInfo(cacheUser);
	}
	public void ShowShare(U3DSharkBaseData _in_data)
	{
		bonjour.ShowShare(_in_data);
	}
	public void CallCopyClipboard(U3DSharkBaseData _in_data)
	{
		bonjour.CopyClipboard(_in_data);
	}
	public bool IsHasRequest(String requestType)
	{
		return bonjour.IsHasRequest(requestType);
	}
	public void Destory()
	{
		bonjour.Destory();
	}
	public void ExitGame()
	{
        DEBUG.LogError("U3DSharkSDK.ExitGame -- Call Stack : ");
        Debug.LogError(new System.Diagnostics.StackTrace().ToString());
		bonjour.ExitGame();
	}
	public void DoAnyFunction(string _func_name,U3DSharkBaseData _in_data)
	{
		bonjour.DoAnyFunction(_func_name,_in_data);
	}
	public string DoPhoneInfo()
	{
		return bonjour.DoPhoneInfo ();
	}
	public void AddLocalPush(U3DSharkBaseData _pushData)
	{
		bonjour.AddLocalPush (_pushData);
	}
	public void RemoveLocalPush(string pushid)
	{
		bonjour.RemoveLocalPush (pushid);
	}
    public void RemoveAllLocalPush()
	{
		bonjour.RemoveAllLocalPush ();
	}
	public void GetUserFriends()
	{
		bonjour.GetUserFriends ();
	}
	/////////////////private functions ///////
	private bool selfInit()
	{
		if (isInitSelf) return true;
		isInitSelf = true;

//		AnalyXMLData();

		bonjour.initSDK();
		return false;
	}

	/**
	private void AnalyXMLData()
	{
		XmlDocument xmlDoc= SharkSDKTool.XmlTool.readXMLBelowAsster(U3DSharkDefine.SHARK_SDK_CONFIG_PATH);
		U3DSharkBaseData platformData = GetPlatformData();
		if(xmlDoc!=null)
		{
			XmlNodeList nodeList = xmlDoc.SelectSingleNode("data").ChildNodes;
			foreach(XmlElement sdk in nodeList)
			{
				foreach(XmlElement sdkEle in sdk.ChildNodes)
				{
					//					DEBUG.Log("\n"+ sdkEle.Name+" : "+sdkEle.InnerText+"<<");
					;
					if(null!=platformData.GetType().GetField(sdkEle.Name)
					   )
					{
						DEBUG.Log("change element name: "+ sdkEle.Name);
						if(null!=platformData.GetType().GetField(sdkEle.Name))
						{
						}
						else 
						{
							platformData.GetType().GetField(sdkEle.Name).SetValue(platformData,sdkEle.InnerText);
						}
//						DEBUG.Log("\n"+ sdkEle.Name + ":--:"+
//						          platformData.GetType().GetField(sdkEle.Name).GetValue(platformData).ToString()
//						          );
					}
				}
			}
		}
	}
*/

	public void AddEventDelegate(SharkEventType _in_type,U3DSharkEventDelegate _in_delegate)
	{

		if( !_delegateDic.ContainsKey(_in_type))
		{
			_delegateDic.Add(_in_type,_in_delegate);
			return;
		}
		else
		{
			U3DSharkEventDelegate cacheDelegate = _delegateDic[_in_type];
			cacheDelegate+=_in_delegate;
		}
		System.Console.WriteLine("success add degelet ");
	}
	public void RemoveEventDelegate(SharkEventType _in_type)
	{
		if( _delegateDic.ContainsKey(_in_type))
		{
			_delegateDic.Remove(_in_type);
		}
	}


	public  void SendEvent(SharkEventType _in_type,U3DSharkBaseData _in_data )
	{

		U3DSharkEvent evt = new U3DSharkEvent (_in_type,_in_data);

		if(null!=_in_data)
			DEBUG.Log(">>>>send event<<<<< type "+ _in_type + "data" + _in_data.DataToString());
		else
			DEBUG.Log(">>>>send event<<<<< type "+ _in_type + "null data" );


		DEBUG.Log("delegate dic start");
		if(_delegateDic.ContainsKey(_in_type))
		{
			if(_delegateDic[_in_type]!=null)
			{
				_delegateDic[_in_type](evt);
			}
		}
		else
		{
			DEBUG.Log("dic didnot has key");
		}
	}


	public void SendEvent(SharkEventType _in_type)
	{
		SendEvent (_in_type, new U3DSharkBaseData());
	}
}

