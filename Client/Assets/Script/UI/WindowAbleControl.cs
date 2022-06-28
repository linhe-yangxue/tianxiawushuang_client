using UnityEngine;
using System.Collections;
using Utilities;
using DataTable;
using System.Collections.Generic;
using System;

public class WindowAbleControl : MonoBehaviour
{
    private static List<string> mGuideWinName = new List<string>
    {
        "GUIDE_DIALOG_WINDOW",
        "GUIDE_MASK_WINDOW"
    };

    //by qiufeng
    //begin
    //一个对象
    public class mWindowColor
    {
        public string arrWindowName;
        public Color[] arrWindowColor;
    }

    class PointListManager
    {
        List<mWindowColor> mWindowPointList = new List<mWindowColor>();

        public void Add(mWindowColor winColor)
        {
            mWindowColor tmpWinColor = winColor;
            int tmpColorIdx = mWindowPointList.FindIndex((mWindowColor tmpColor) =>
            {
                return (tmpColor.arrWindowName == tmpWinColor.arrWindowName);
            });
            if (tmpColorIdx != -1)
            {
                tmpWinColor = mWindowPointList[tmpColorIdx];
                mWindowPointList.RemoveAt(tmpColorIdx);
            }
            mWindowPointList.Add(tmpWinColor);
        }
        public void Remove(string winName)
        {
            int tmpColorIndex = mWindowPointList.FindIndex((mWindowColor tmpColor) =>
            {
                return (tmpColor.arrWindowName == winName);
            });
            if (tmpColorIndex != -1)
                mWindowPointList.RemoveAt(tmpColorIndex);
        }
        public mWindowColor GetAt(int idx)
        {
            if (mWindowPointList == null)
                return null;
            if (idx < 0 || idx >= mWindowPointList.Count)
                return null;

            return mWindowPointList[idx];
        }
        public mWindowColor GetLast()
        {
            if (mWindowPointList == null || mWindowPointList.Count <= 0)
                return null;

            return GetAt(mWindowPointList.Count - 1);
        }
        public int Count
        {
            get
            {
                if (mWindowPointList == null)
                    return 0;
                return mWindowPointList.Count;
            }
        }
    }

    //一个list
    private static PointListManager mNormalWin = new PointListManager();
    private static PointListManager mGuideWin = new PointListManager();
    //end

    protected string mStrWinName = "";
    protected string mWinColor = "";
    DataRecord mWinColorRecord;
    void OnEnable()
    {
        if (mStrWinName == "" || mWinColorRecord == null)
            return;

        string mWinColor = mWinColorRecord.get("WIN_POINT_COLOR");
        string[] mWinColorArray = mWinColor.Split('#');

        Color[] colorArray = new Color[mWinColorArray.Length];

        for (int i = 0; i < mWinColorArray.Length; i++)
        {
            colorArray[i] = sixteenToTen(mWinColorArray[i]);
        }
        string tmpStr = "WindowMask - " + mStrWinName;
        for (int i = 0, count = mWinColorArray.Length; i < count; i++)
        {
              if (i != 0)
                tmpStr += ",";
              tmpStr += mWinColorArray[i];
        }
        DEBUG.Log(tmpStr);
        mWindowColor mWc = new mWindowColor()
        {
            arrWindowName = mStrWinName,
             arrWindowColor = colorArray
        };
        if (IsGuildWindow(mStrWinName))
        {
            mGuideWin.Add(mWc);
            GlobalModule.Instance.SetWindowGuildColor(mWc.arrWindowColor);
        }
        else
        {
            mNormalWin.Add(mWc);
            GlobalModule.Instance.getWindowColorFun(mWc.arrWindowColor);
        }
        //Debug.LogWarning("enable++++++" + mStrWinName);
    }
    void OnDisable()
    {
        if (mStrWinName == "" || mWinColorRecord == null)
            return;
        //add by qiufeng
        //移除颜色块
        //         int tmpColorIdx = mWindowPointList.FindIndex((mWindowColor tmpColor) =>
        //                 {
        //                     return (tmpColor.arrWindowName == mWinPrefabName);
        //                 }  
        if (IsGuildWindow(mStrWinName))
        {
            mGuideWin.Remove(mStrWinName);
            mWindowColor tmpLastWinColor = mGuideWin.GetLast();
            GlobalModule.Instance.SetWindowGuildColor(tmpLastWinColor != null ? tmpLastWinColor.arrWindowColor : null);
        }
        else
        {
            mNormalWin.Remove(mStrWinName);
            mWindowColor tmpLastWinColor = mNormalWin.GetLast();
            GlobalModule.Instance.getWindowColorFun(tmpLastWinColor != null ? tmpLastWinColor.arrWindowColor : null);
        }
        //Debug.LogError("close window------" + mStrWinName);
        
        //Debug.LogError("disable++++++" + mStrWinName);
        //end
    }
    public string WinName
    {
        set { mStrWinName = value; }
        get { return mStrWinName; }
    }
    public string WinColor
    {
        set { mWinColor = value; }
        get { return mWinColor; }
    }
    public DataRecord winColorRecord
    {
        set { mWinColorRecord = value; }
        get { return mWinColorRecord; }
    }
    private Color setNullWindowColor()
    {
        float mMR = 80 / 255.0f;
        float mMG = 80 / 255.0f;
        float mMB = 80 / 255.0f;

        return (new Color(mMR, mMG, mMB));//一个
    }
    //by qiufeng  windowColor
    //begin
    private Color sixteenToTen(string mSixteen)
    {
        //单个[ffffff]->[RGB]
        string mR = mSixteen.Substring(0, 2);
        string mG = mSixteen.Substring(2, 2);
        string mB = mSixteen.Substring(4, 2);

        int mCR = Convert.ToInt32(mR, 16);
        int mCG =Convert.ToInt32(mG, 16);
        int mCB =Convert.ToInt32(mB, 16);

        float mMR = mCR / 255.0f;
        float mMG = mCG / 255.0f;
        float mMB = mCB / 255.0f;

        return (new Color(mMR, mMG, mMB));//一个
    }

    public static bool IsGuildWindow(string winName)
    {
        if (winName == null || winName == "")
            return false;
        if (mGuideWinName == null)
            return false;

        int tmpIdx = mGuideWinName.IndexOf(winName);
        return (tmpIdx != -1);
    }
}
