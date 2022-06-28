using UnityEngine;
using System.Collections;

/// <summary>
/// 战斗力属性变化反馈
/// </summary>
public class ChangeTip
{
    private string mContent;
    /// <summary>
    /// 文字内容
    /// </summary>
    public string Content
    {
        set
        {
            mContent = value;
            if (mContent.IndexOf("[-]") > 0)
                return;
            if (mContent.IndexOf('+') > 0) 
            {
                string[] _showInfoArr = mContent.Split('+');
                mContent = "[ff9900]" + _showInfoArr[0] + "[-]  [99ff66]+" + _showInfoArr[1];
            }
            else if (mContent.IndexOf('-') > 0) 
            {
                string[] _showInfoArr = mContent.Split('-');
                mContent = "[ff9900]" + _showInfoArr[0] + "[-]  [ff3333]-" + _showInfoArr[1];
            }
        }
        get{return mContent;}
    }
    /// <summary>
    /// 物体自身
    /// </summary>
    public GameObject mGameObject { set; get; }
    /// <summary>
    /// 目标物体
    /// </summary>
    public GameObject TargetObj { set; get; }
    /// <summary>
    /// 目标值
    /// </summary>
    public float TargetValue { set; get; }
    /// <summary>
    /// 目标值类型
    /// </summary>
    public ChangeTipValueType TargetValueType { set; get; }
    public LabelShowType ShowType { set; get; }

    /// <summary>
    /// 数值的精度
    /// </summary>
    public int Precision { set; get; }
    public void SetTargetObj(ChangeTipPanelType kPanelType,ChangeTipValueType kValueType) 
    {
        SetCurPanelType(kPanelType);
        //1.当前面板+属性类型决定最终位置
        TargetObj = null;
        int _panelTypeIndex = (int)kPanelType - 1;
        if (_panelTypeIndex < 0) 
            return;

        tWindow _tWin = DataCenter.GetData(kPanelType.ToString()) as tWindow;
        if (_tWin == null) 
            return;
        TargetValueType = kValueType;
        TargetObj = ChangeTipManager.Self.FindTargetObj(_tWin.mGameObjUI, ChangeTipManager.Self.TargetDic[kPanelType][kValueType]);
    }

    private void SetCurPanelType(ChangeTipPanelType kPanelType) 
    {
        if(kPanelType != ChangeTipPanelType.COMMON)
            ChangeTipManager.Self.CurShowPanelType = kPanelType;      
    }
}
