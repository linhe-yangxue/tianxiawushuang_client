using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum LabelColor
{
    Green,
    Red,
    Black,
    White,
    Grey,
}
public static class Extension_LHC
{
    public static void Foreach<T>(this IEnumerable<T> source, Action<T> action) {
        foreach (var item in source) action(item);
    }

    
    public static string SetTextColor(this string text, LabelColor color) {
        string colorStr = "";
        switch (color) {
            case LabelColor.Black: colorStr = "000000"; break;
            case LabelColor.Green: colorStr = "33FF33"; break;
            case LabelColor.Red: colorStr = "FF0000"; break;
            case LabelColor.White: colorStr = "FFFFFF"; break;
            case LabelColor.Grey: colorStr = "666666"; break;
        }
        return "[" + colorStr + "]" + text + "[-]";
    }

    public static string InsertToString(this int num,string text){
        return string.Format(text, num);
    }

  
	
}
