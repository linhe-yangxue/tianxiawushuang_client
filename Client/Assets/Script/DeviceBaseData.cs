using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

public class DeviceBase:MessageBase {
    public readonly string deviceId=SystemInfo.deviceUniqueIdentifier;
    public readonly string channel=DeviceBaseData.channel;
    public readonly string clientVersion=DeviceBaseData.clientVersion;
    public readonly string systemSoftware=SystemInfo.operatingSystem;
    
    public readonly string systemHardware=DeviceBaseData.deviceName;    
}

public class LoginServerMessageDevice:DeviceBase {
    public int registtype;
}

public class CS_AppLaunch:DeviceBase {
    public readonly string ip=DeviceBaseData.ip;
    public readonly string mac=DeviceBaseData.mac;
}

public class SC_AppLaunch:RespMessage {
    
}

public class CS_AppLoadStep:DeviceBase {
    public readonly int step;
    public readonly int interval;

    public CS_AppLoadStep(int step,int interval) {
        this.step=step;
        this.interval=interval;
    }
}

public class SC_AppLoadStep:RespMessage {
    
}

public class CS_AppUpdate:DeviceBase {
    public readonly string gameVersion1;
    public readonly string gameVersion2;
    public readonly int updateTime;

    public CS_AppUpdate(string gameVersion1,string gameVersion2,int updateTime) {
        this.gameVersion1=gameVersion1;
        this.gameVersion2=gameVersion2;
        this.updateTime=updateTime;
    }
}

public class SC_AppUpdate:RespMessage{
    
}




public static class DeviceBaseData{
    static string _channel;
    static string _clientVersion;
    static string _ip;
    static string _mac;
    static string _deviceName;

    public static string deviceName {
        get {
            if(_deviceName==null) {
#if !UNITY_EDITOR && !NO_USE_SDK
                if(CommonParam.isUseSDK) {
                    string deviceInfo=U3DSharkSDK.Instance.DoPhoneInfo();
                    U3DSharkBaseData deviceData=new U3DSharkBaseData();
                    deviceData.StringToData(deviceInfo);
                    _deviceName=deviceData.GetData(U3DSharkAttName.PHONE_MODEL);
                    DEBUG.LogError(_deviceName);
                } else _deviceName="0";
#else
                _deviceName = "0";
#endif
            }
            return _deviceName;
        }
    }

    public static string channel {
        get {
            if(_channel==null)
                _channel = HotUpdateLoading.LastestClientVersion.channel;
            return _channel;
        }
    }

    public static string clientVersion {
        get {
            if(_clientVersion==null)
                _clientVersion = HotUpdateLoading.LastestClientVersion.version;
            return _clientVersion;
        }
    }

    public static string ip {
        get {
            if(_ip==null) {
#if !UNITY_EDITOR && !NO_USE_SDK
                if(CommonParam.isUseSDK) {
                    string deviceInfo=U3DSharkSDK.Instance.DoPhoneInfo();
                    U3DSharkBaseData deviceData=new U3DSharkBaseData();
                    deviceData.StringToData(deviceInfo);
                    _ip=deviceData.GetData(U3DSharkAttName.PHONE_IP);
                } else _ip="0";
#else
                _ip = "0";
#endif
            }
            return _ip;
        }
    }

    public static string mac {
        get {
            if(_mac==null) {
#if !UNITY_EDITOR && !NO_USE_SDK
                if(CommonParam.isUseSDK) {
                    string deviceInfo=U3DSharkSDK.Instance.DoPhoneInfo();
                    U3DSharkBaseData deviceData=new U3DSharkBaseData();
                    deviceData.StringToData(deviceInfo);
                    _mac=deviceData.GetData(U3DSharkAttName.MAC_ADDRESS);   
                }
                else _mac="0";
#else
                _mac = "0";
#endif
            }
            return _mac;
        }
    }
}



public enum LoadStepType { 
    TableUpdateFinished=0,
}
public static class BIHelper {
    public static void SendAppLoadStep(LoadStepType stepType) {
        if(!PlayerPrefs.HasKey(stepType.ToString())) {
            var cs=new CS_AppLoadStep((int)stepType,(int)Time.time);
            HttpModule.Instace.SendGameServerMessageT(cs,text => {
                PlayerPrefs.SetString(stepType.ToString(),stepType.ToString());
            },
            NetManager.RequestFail,false);    
        }
    }

    public static void SendUpdate(string gameVersion1,string gameVersion2,int updateTime) {
        var cs=new CS_AppUpdate(gameVersion1,gameVersion2,updateTime);
        HttpModule.Instace.SendGameServerMessageT(cs,NetManager.RequestSuccess,NetManager.RequestFail,false);
    }

    public static void SendLaunch() { 
        if(!PlayerPrefs.HasKey("Launch")){
            var cs=new CS_AppLaunch();
            HttpModule.Instace.SendGameServerMessageT(cs,text => {
                PlayerPrefs.SetString("Launch","ok");
            },
            NetManager.RequestFail,false);
        }
    }
}


public class GameServerDeviceBase : GameServerMessage
{
    public readonly string deviceId = SystemInfo.deviceUniqueIdentifier;
    public readonly string clientVersion = DeviceBaseData.clientVersion;
    public readonly string systemSoftware = SystemInfo.operatingSystem;
    public readonly string systemHardware = DeviceBaseData.deviceName;
}