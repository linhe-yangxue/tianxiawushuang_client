using System.Runtime.InteropServices;

public class MyPlaySDK
{
    public class Variables
    {
        public static string DeviceToken = "";
        public static string GID = "";
        public static string Token = "";
        public static string ServerID = "";
        public static string RoleID = "";
        public static int registerAPNSResult = 0; // 0 - not resp yet, 1 - succeed, -1 - falied
        public static bool initIAPYet = true;
    }

    //[DllImport("__Internal")]
    public static void CustomerService() { }

    //[DllImport("__Internal")]
    public static void Login() { }

    //[DllImport("__Internal")]
    public static void Purchase() { }

    //[DllImport("__Internal")]
    public static void InitIAP(string serverID, string roleID) { }
}