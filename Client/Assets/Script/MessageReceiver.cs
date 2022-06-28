using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MessageReceiver : MonoBehaviour
{
    private static MessageReceiver instance = null;

    public static MessageReceiver Instance
    {
        get 
        {
            if (instance == null)
            {
                Init();
            }

            return instance;
        }
    }

    public static void Init()
    {
        if (instance == null)
        {
            GameObject receiver = new GameObject("MessageReceiver");
            DontDestroyOnLoad(receiver);
            instance = receiver.AddComponent<MessageReceiver>();
        }
    }

    public void Log(string msg)
    {
        DEBUG.Log(msg);
    }

    public void OnGetAPNSDeviceTokenSuccess(string msg)
    {
        MyPlaySDK.Variables.registerAPNSResult = 1;
        MyPlaySDK.Variables.DeviceToken = msg;
    }

    public void OnGetAPNSDeviceTokenError(string msg)
    {
        MyPlaySDK.Variables.registerAPNSResult = -1;
    }

    public void OnLoginSucceed(string msg)
    {
        Dictionary<string, string> info = ParseMessage(msg);
        string gid = "";
        string token = "";
        info.TryGetValue("GID", out gid);
        info.TryGetValue("TOKEN", out token);
        LoginNet.OnLoginSDKResult(true, gid, token);
    }

    public void OnLoginFailed(string msg)
    {
        LoginNet.OnLoginSDKResult(false, "", "");
    }

    private static Dictionary<string, string> ParseMessage(string msg)
    {
        string[] array = msg.Split(',');
        Dictionary<string, string> dict = new Dictionary<string, string>();

        foreach (var member in array)
        {
            int index = member.IndexOf('=');

            if (index < 0)
                continue;

            string key = member.Substring(0, index);
            string val = member.Substring(index + 1, member.Length - index - 1);
            dict[key] = val;
        }

        return dict;
    }
}