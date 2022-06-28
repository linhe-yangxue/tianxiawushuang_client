using UnityEngine;
using System;
using System.Collections.Generic;
using Utilities.Events;


public class WindowInfo : MonoBehaviour
{
    private static HashSet<string> mOpenedWindows = new HashSet<string>();

    public static HashSet<string> GetAllOpenedWindows()
    {
        return new HashSet<string>(mOpenedWindows);
    }

    private tWindow mWin = null;
    private string mWinName = "";

    public tWindow window
    {
        get 
        { 
            return mWin; 
        }
        set 
        {
            mWin = value;

            if (!string.IsNullOrEmpty(mWinName))
            {
                mOpenedWindows.Remove(mWinName);
            }

            if (mWin == null)
            {
                mWinName = "";
            }
            else 
            {
                mWinName = mWin.mWinName;

                if (!string.IsNullOrEmpty(mWinName) && mWin.mGameObjUI != null && mWin.mGameObjUI.activeInHierarchy)
                {
                    mOpenedWindows.Add(mWinName);
                }
            }
        }
    }

    public string winName
    {
        get { return mWinName; }
    }

    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(mWinName))
        {
            mOpenedWindows.Add(mWinName);
        }
    }

    private void OnDisable()
    {
        if (!string.IsNullOrEmpty(mWinName))
        {
            mOpenedWindows.Remove(mWinName);
        }
    }
}