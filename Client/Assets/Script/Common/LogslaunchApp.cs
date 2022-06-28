using UnityEngine;
using System.Collections;

public class LogsLaunchApp
{
	static public string GetStringLogsLaunchApp(LOGS_LAUNCH_APP type)
	{
		string strResult = "";
		switch(type)
		{
		case LOGS_LAUNCH_APP.deviceId:
			strResult = SystemInfo.deviceUniqueIdentifier;
			break;
		case LOGS_LAUNCH_APP.systemSoftware:
			strResult = SystemInfo.operatingSystem;
			break;
		case LOGS_LAUNCH_APP.systemHardware:
			strResult = SystemInfo.deviceModel;
			break;
		case LOGS_LAUNCH_APP.telecomOper:
			break;
		case LOGS_LAUNCH_APP.network:
			if(Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
				strResult = "WIFI";
			else if(Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
				strResult = "2G/3G/4G";
			break;
		case LOGS_LAUNCH_APP.cpuHardware:
			strResult = SystemInfo.processorType;
			break;
		}

		return strResult;
	}

	static public int GetIntLogsLaunchApp(LOGS_LAUNCH_APP type)
	{
		int iResult = 0;
		switch(type)
		{
		case LOGS_LAUNCH_APP.channel:
			break;
		case LOGS_LAUNCH_APP.screenHight:
			iResult = Screen.height;
			break;
		case LOGS_LAUNCH_APP.screenWidth:
			iResult = Screen.width;
			break;
		case LOGS_LAUNCH_APP.memory:
			iResult = SystemInfo.systemMemorySize;
			break;	
		}

		return iResult;
	}

	static public float GetFloatLogsLaunchApp(LOGS_LAUNCH_APP type)
	{
		float fResult = 0f;
		switch(type)
		{
		case LOGS_LAUNCH_APP.density:
			fResult = Screen.dpi;
			break;
		}

		return fResult;
	}
}

public enum LOGS_LAUNCH_APP
{
	eventTime,
	deviceId,
	channel,
	ip,
	clientVersion,
	systemSoftware,
	systemHardware,
	telecomOper,
	network,
	screenWidth,
	screenHight,
	density,
	cpuHardware,
	memory,
}