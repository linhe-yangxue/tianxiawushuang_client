using UnityEngine;
using System.Collections;

/// <summary>
/// Label的显示类型
/// </summary>
public enum LabelShowType
{
    NONE,               //> 默认值
    ONLY_NUMBER,        //> 纯数字
    HAS_SLASH,          //> 包含斜杠
    HAS_RATE,           //> 有百分号
    HAS_RATE_DOT,       //> 带小数位和百分号
    HAS_LV_DOT,         //> 有Lv.样式的
    HAS_ADD_AND_COLOR,  //> 类似突破属性有"+"号的
    HAS_ADD,            //> 突破等级
}

/// <summary>
/// 动态反馈文字Label
/// </summary>
public class ChangeLabel : MonoBehaviour 
{
    /// <summary>
    /// 动画播放时间
    /// </summary>
    public float mDuration = 0.3f; 
    /// <summary>
    /// 目标值
    /// </summary>
    public float TargetValue = 0;
    /// <summary>
    /// 对应的Label
    /// </summary>
    public UILabel mLabel;
    /// <summary>
    /// 数值的精度
    /// </summary>
    public int Precision { set; get; }

    public LabelShowType ShowType;

    private float mWaitTime = 0.05f;

    public void PlayAnim() 
    {
        mLabel = GetComponent<UILabel>();
        if (mLabel == null)
            return;
        TweenScale _tweenScale = GetComponent<TweenScale>();
        if (_tweenScale == null)
            return;
        _tweenScale.enabled = true;
        _tweenScale.ResetToBeginning();
        _tweenScale.PlayForward();
        
        //根据label显示的格式进行数字的累加
        StartCoroutine(AddNumber(GetOriginValue(mLabel.text), TargetValue));          
    }



    private LabelShowType GetLabelShowType(string kLabelStr) 
    {
        if (ShowType != LabelShowType.NONE)
        {
            return ShowType;
        }

        if (kLabelStr.IndexOf('/') >= 0) 
        {
            ShowType = LabelShowType.HAS_SLASH;
            return LabelShowType.HAS_SLASH;
        }
        if (kLabelStr.IndexOf('%') >= 0 && kLabelStr.IndexOf('.') >= 0) 
        {
            ShowType = LabelShowType.HAS_RATE_DOT;
            return LabelShowType.HAS_RATE_DOT;
        }
        if (kLabelStr.IndexOf('%') >= 0) 
        {
            ShowType = LabelShowType.HAS_RATE;
            return LabelShowType.HAS_RATE;
        }
        if (kLabelStr.IndexOf("Lv.") >= 0) 
        {
            ShowType = LabelShowType.HAS_LV_DOT;
            return LabelShowType.HAS_LV_DOT;
        }
        if (kLabelStr.IndexOf('+') >= 0 && kLabelStr.IndexOf('[') >= 0) 
        {
            ShowType = LabelShowType.HAS_ADD_AND_COLOR;
            return LabelShowType.HAS_ADD_AND_COLOR;
        }
        if (kLabelStr.IndexOf('+') >= 0) 
        {
            ShowType = LabelShowType.HAS_ADD;
            return LabelShowType.HAS_ADD;      
        }
        return LabelShowType.ONLY_NUMBER;
    }

    private string GetFormatString(string kLabelStr) 
    {
        switch (GetLabelShowType(kLabelStr)) 
        {
            case LabelShowType.ONLY_NUMBER:
                return GetNumberFormatString();
            case LabelShowType.HAS_RATE:
                return GetPercentFormatString();
            case LabelShowType.HAS_RATE_DOT:
                return GetPercentFormatString();
            case LabelShowType.HAS_LV_DOT:
                return GetLvFormatValue();           
            case LabelShowType.HAS_SLASH:
                return GetSlashFormatString(kLabelStr);           
            case LabelShowType.HAS_ADD_AND_COLOR:
                return GetAddFormatString(kLabelStr);
            case LabelShowType.HAS_ADD:
                return GetOnlyAddFormatString(kLabelStr);
        }
        return "";
    }
    private float GetOriginValue(string kLabelStr) 
    {
        switch (GetLabelShowType(kLabelStr))
        {
            case LabelShowType.ONLY_NUMBER:
                return GetNumberOriginValue(kLabelStr);
            case LabelShowType.HAS_RATE:
                return GetPercentOriginValue(kLabelStr);
            case LabelShowType.HAS_RATE_DOT:
                return GetPercentOriginValue(kLabelStr);
            case LabelShowType.HAS_LV_DOT:
                return GetLvOriginValue(kLabelStr);          
            case LabelShowType.HAS_SLASH:
                return GetSlashOriginValue(kLabelStr);
            case LabelShowType.HAS_ADD_AND_COLOR:
                return GetAddOriginValue(kLabelStr);
            case LabelShowType.HAS_ADD:
                return GetOnlyAddOriginValue(kLabelStr);
        }
        return 0f;
    }
    private int GetPrecision(string kLabelStr) 
    {
        return Precision;
    }

    private float mThreshold = 0.001f;
    #region 包含斜杠的
    private float GetSlashOriginValue(string kText) 
    {
        int _beginIndex = kText.IndexOf(']');
        int _endIndex = kText.IndexOf('/');
        float _originValue = 0f;
        float.TryParse(kText.Substring(_beginIndex+1, _endIndex - _beginIndex-1), out _originValue);
        return _originValue;
    }
    private string GetSlashFormatString(string kText) 
    {
        string _formatInfo = "";
        int _colorPrefixIndex = kText.IndexOf(']');
        int _slashIndex = kText.IndexOf('/');
        string _colorPrefixInfo = kText.Substring(0,_colorPrefixIndex+1);
        _formatInfo = _colorPrefixInfo + "{0} /" + kText.Substring(_slashIndex+1);
        return _formatInfo;
    }
  
    #endregion

    #region 纯数字的 例: "1"
    private float GetNumberOriginValue(string kText) 
    {
        float _originValue = 0;
        float.TryParse(kText, out _originValue);
        return _originValue;
    }
    private string GetNumberFormatString() 
    {
        return "{0}";
    }
    private IEnumerator AddNumber(float kOriginValue, float kTargetValue)
    {
        if (mLabel == null)
            yield break;
        float _addTotlalNum = kTargetValue - kOriginValue;
        int _step = (int)(mDuration / mWaitTime);
        float _addStepNum = _addTotlalNum / _step;

        int _precision = GetPrecision(mLabel.text);
        string _showInfo = GetFormatString(mLabel.text);
        if (Mathf.Abs(_addTotlalNum) <= 0.0001)
        {
            mLabel.text = string.Format(_showInfo, (kTargetValue + mThreshold).ToString("f" + _precision));
            yield break;
        }     
        float _showNum = kOriginValue;
        for (int i = 0; i < _step; i++)
        {
            _showNum += _addStepNum;
            mLabel.text = string.Format(_showInfo, (_showNum + mThreshold).ToString("f" + _precision));
            yield return new WaitForSeconds(mWaitTime);
        }
        mLabel.text = string.Format(_showInfo, (kTargetValue + mThreshold).ToString("f" + _precision));
    }
    #endregion

    #region 带Lv.样式的 例: "Lv.2"
    private float GetLvOriginValue(string kText) 
    {
        float _originValue = 0f;
        float.TryParse(kText.Replace("Lv.",""),out _originValue);
        return _originValue;
    }
    private string GetLvFormatValue() 
    {
        return "Lv.{0}";
    }
  
    #endregion

    #region 带%的 例: "12.22%"
    private float GetPercentOriginValue(string kText) 
    {
        float _originValue = 0f;
        float.TryParse(kText.Replace("%",""),out _originValue);
        return _originValue;
    }
    private string GetPercentFormatString() 
    {
        return "{0}%";   
    }
    #endregion

    #region 带"+"并且有颜色标记的  例: "213 [ff0000] +1"
    private float GetAddOriginValue(string kText) 
    {
        int _colorPrefixIndex = kText.IndexOf('[');
        float _originValue = 0f;
        float.TryParse(kText.Substring(0, _colorPrefixIndex), out _originValue);
        return _originValue;
    }

    private string GetAddFormatString(string kText) 
    {
        return "{0} " + kText.Substring(kText.IndexOf('['));
    }
    #endregion

    #region 带"+"号的  例: "+1"
    private float GetOnlyAddOriginValue(string kText) 
    {
        int _addIndex = kText.IndexOf('+');
        if (_addIndex < 0) 
        {
            return 0f;
        }
        float _originValue = 0f;
        float.TryParse(kText.Substring(_addIndex),out _originValue);
        return _originValue;
    }

    private string GetOnlyAddFormatString(string kText) 
    {
        return "+{0}";
    }
    #endregion
}
