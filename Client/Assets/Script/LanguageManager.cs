//using UnityEngine;
//using System.Collections;
//using System;

//public enum BaseLan
//{
//    //1000
//    OK=1,
//    CANCEL=2,
//    CanBuy=3,
//    CanNotBuy=4,
//    HaveBuyLimitYet=5,
//    CanBuyCount = 6,
//}

//public enum UnionLan
//{
//    //2000
//    APPOINT=1,
//    KICK=2
//}

//public static class LanCenter 
//{
//    public static string GetLan(Enum lan)
//    {
//        Type enumType = lan.GetType();
//        int standardValue=0;

//        if (enumType == typeof(BaseLan)) standardValue = 1000;
//        else if(enumType == typeof(UnionLan)) standardValue = 2000;
//        else DEBUG.LogWarning("IS NOT LANGUAGE TYPE!!!");
//        return DataCenter.mBaseLanguageConfig.GetRecord(standardValue+lan.GetHashCode()).getData(0);
//    }
//}
