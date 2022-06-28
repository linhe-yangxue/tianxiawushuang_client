//using System.Collections;
//using System.Runtime.InteropServices;
//using System.Text;
//
//public class DllKeyAccessor
//{
//#if UNITY_EDITOR || UNITY_ANDROID
//    [DllImport("Utility3", EntryPoint = "GetGameStringValueBySign")]
//#elif UNITY_IOS
//    [DllImport("__Internal")]
//#endif
//    public extern static void GetGameStringValueBySign(string sz, StringBuilder sz2, int count);
//
//#if UNITY_EDITOR || UNITY_ANDROID
//    [DllImport("Utility3", EntryPoint = "GetGameBytesValueBySign")]
//#elif UNITY_IOS
//    [DllImport("__Internal")]
//#endif
//    public extern static void GetGameBytesValueBySign(string sz, byte[] bytes, int count);
//
//#if UNITY_EDITOR || UNITY_ANDROID
//    [DllImport("Utility3", EntryPoint = "GetGameBytesValueByIndex")]
//#elif UNITY_IOS
//    [DllImport("__Internal")]
//#endif
//    public extern static void GetGameBytesValueByIndex(int[] sz, byte[] bytes, int count);
//}
