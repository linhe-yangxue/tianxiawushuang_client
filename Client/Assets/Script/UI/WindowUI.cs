using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utilities;
using DataTable;

public class tWindow : tLogicData
{
    public GameObject mGameObjUI;
    public string mWinName = "";
    public string mWinPrefabName = "";
    public string mAnchor = "";
    public string mWinColor = "";

    //by qiufeng
    //begin
    //一个对象
    public class mWindowColor
    {
        public string arrWindowName;
        public Color[] arrWindowColor;
    }
    //一个list
    List<mWindowColor> mWindowPointList = new List<mWindowColor>();
    //end

    private GameObject mTweenRoot = null;
    protected GameObject mGridTweenRoot = null;
    private GameObject mAlphaRoot = null;

    public override void Init() { }

    /// <summary>
    /// 只有面板初次被打开时才执行
    /// </summary>
    protected virtual void OpenInit()
    {
        if (!CommonParam.isOnLineVersion)
        {
            foreach (DataTable.DataRecord pair in DataCenter.allIosCheck)
            {
                string windowName = pair.get("WINDOW_NAME");
                if (windowName.Equals(mWinName))
                {
                    string tPath = pair.get("RESOURCE_PATH");
                    string tWidgetType = pair.get("TYPE");
                    string resourceStr = pair.get("IOS_CHECK");
                    string atlasName = pair.get("IOS_ATLAS_NAME");

                    Transform tWidget = mGameObjUI.transform.Find(tPath);
                    if (tWidgetType == "Label")
                    {
                        tWidget.GetComponent<UILabel>().text = resourceStr;
                    }
                    else if (tWidgetType == "Sprite")
                    {
                        GameCommon.SetIcon(tWidget.GetComponent<UISprite>(), atlasName, resourceStr);
                    }
                    else if (tWidgetType == "Texture")
                    {
                        tWidget.GetComponent<UITexture>().mainTexture = Resources.Load<Texture2D>("textures/UItextures/Background/" + resourceStr);
                    }
                }
            }
        }
    }
    public virtual void Open(object param)
    {
        if (mGameObjUI == null)
        {
            if (mWinPrefabName != "")
            {
                if (mAnchor == "")
                    mAnchor = "CenterAnchor";
                mGameObjUI = GameCommon.LoadAndIntanciateUIPrefabs(mWinPrefabName, mAnchor);
                if (mGameObjUI != null)
                {
                    mGameObjUI.name = mWinPrefabName;
                    OpenInit();
                }




                //try {
                //    mGameObjUI = GameCommon.LoadAndIntanciateUIPrefabs(mWinPrefabName, mAnchor);
                //    if (mGameObjUI != null) {
                //        mGameObjUI.name = mWinPrefabName;
                //        OpenInit();
                //    }
                //}
                //catch {
                //    DEBUG.LogError("Load window fail >" + mWinPrefabName);
                //}
            }
        }

        if (mGameObjUI != null)
        {
            mGameObjUI.GetOrAddComponent<WindowInfo>().window = this;
            mGameObjUI.SetActive(true);
            //added by xuke
            if (CommonParam.mOpenDynamicUI)
            {
                PlayTweenAnim();
            }
            //end
        }

        //if (!isOpened)
        //{
        //    OpenInit();
        //    isOpened = true;
        //}


        //by qiufeng
        //begin
        //
        //Debug.Log(mWinPrefabName);
        //try
        //{
        if (CommonParam.UsePointMark)
        {
            DataRecord mWinColorRecord = DataCenter.mWindowPoint.GetRecord(mWinName);

            if (mWinColorRecord != null)
            {
                WindowAbleControl tmpControl = mGameObjUI.GetComponent<WindowAbleControl>();

                if (tmpControl == null)
                    tmpControl = mGameObjUI.AddComponent<WindowAbleControl>();
                if (tmpControl != null)
                {
                    tmpControl.WinName = mWinName;
                    tmpControl.winColorRecord = mWinColorRecord;
                    tmpControl.enabled = false;
                    tmpControl.enabled = true;
                }

            }
        }
    }
    
    //end
#region 动态UI
     
    protected virtual void PlayTweenAnim() 
    {
        if (!CommonParam.mOpenDynamicUI)
            return;

        CheckWindow_PlayScaleTween();
        CheckPlayGridTween();
        CheckPlayTransTween();
        CheckPlayAlphaTween();
        CheckPlayScaleTween();
    
    }
    protected virtual void CheckPlayScaleTween(string kScaleRootName = "scale_tween_root") 
    {
        GameObject _scaleRoot = GetSub(kScaleRootName);
        if (_scaleRoot == null)
            return;
        ScaleTweenAnim _scaleTween = _scaleRoot.GetComponent<ScaleTweenAnim>();
        if (_scaleRoot == null)
            return;
        _scaleTween.PlayScaleTweenAnim();
    }

    protected virtual void CheckPlayAlphaTween() 
    {
        mAlphaRoot = GetSub("alpha_tween_root");
        if (mAlphaRoot == null)
            return;
        AlphaTweenAnim _alphaTween = mAlphaRoot.GetComponent<AlphaTweenAnim>();
        if (_alphaTween == null)
            return;
        _alphaTween.PlayAlphaTweenAnim();
    }
    protected virtual void CheckPlayTransTween() 
    {
        PlayTransTween("transform_tween_root");
        PlayTransTween("transform_tween_root_2");
    }
    protected virtual void PlayTransTween(string kObjName) 
    {
        if (mGameObjUI == null)
            return;
        GameObject _transRoot = GetSub(kObjName);
        UIPanel _panel = mGameObjUI.GetComponent<UIPanel>();

        if (_transRoot == null) 
        {
            __SetDirty(_panel);
            return;            
        }
        TransformTweenAnim _transTween = _transRoot.GetComponent<TransformTweenAnim>();
        if (_transTween != null)
            _transTween.PlayTweenAnim();

        __SetDirty(_panel);
    }

    private void __SetDirty(UIPanel kPanel) 
    {
        if (kPanel == null)
            return;
        GlobalModule.DoCoroutine(DelaySetDirty(kPanel));
    }
    private IEnumerator DelaySetDirty(UIPanel kPanel) 
    {
        yield return new WaitForEndOfFrame();
        if (kPanel != null) 
        {
            kPanel.SetDirty();
        }
    }

    public virtual void CheckPlayGridTween(string kGridRootName = "grid_tween_root") 
    {
        if (!CommonParam.mOpenDynamicUI)
            return;
        mGridTweenRoot = GetSub(kGridRootName);
        if (mGridTweenRoot == null)
            return;
        GridTransformTweenAnim _transTween = mGridTweenRoot.GetComponent<GridTransformTweenAnim>();
        if (_transTween == null)
            return;
        ResetGridTweenFlag();
        //_transTween.ResetPlayFlag();
        //有些UIGridContainer的MaxCount是在预制体中改变的,代码中没有改变
        PlayGridTween(); 
    }
    public virtual void ResetGridTweenFlag() 
    {
        if (!CommonParam.mOpenDynamicUI)
            return;
        if (mGridTweenRoot == null) 
        {
            Debug.Log("grid_tween_root为空,重置标记失败");
            return;
        }
        GridTransformTweenAnim _gridTween = mGridTweenRoot.GetComponent<GridTransformTweenAnim>();
        if (_gridTween != null)
            _gridTween.ResetPlayFlag();
    }
    protected virtual void PlayGridTween() { }
    protected void CheckWindow_PlayScaleTween() 
    {
        if (mTweenRoot == null)
            mTweenRoot = GetSub("tween_root");
        if (mTweenRoot == null)
            return;

        GameCommon.PlayOpenTweenScale(mTweenRoot.transform);

        UIPanel _panel = mGameObjUI.GetComponent<UIPanel>();
        //// NGUI的坑
        __SetDirty(_panel);
    }

    protected UITweener PlayTween(UITweener kTween)
    {
        if (kTween == null)
            return null;
        kTween.enabled = false;
        kTween.ResetToBeginning();
        kTween.PlayForward();
        return kTween;
    }
    protected void ResetTween(UITweener kTween)
    {
        if (kTween == null)
            return;
        kTween.enabled = false;
        kTween.ResetToBeginning();   
    }

#endregion
    public virtual void Close()
    {
        if (mTweenRoot == null || !GameCommon.IsPlayCloseTween)
        {
            __Close();
        }
        else
        {
            GameCommon.PlayCloseTweenScale(mTweenRoot.transform, __Close);
        }
    }
       
    private void __Close() 
    {
        CloseAllSubWindows();

        if (mGameObjUI != null)
        {
            //added by xuke
            OnClose_HideObj();
            //end
            mGameObjUI.SetActive(false);
        }

        OnClose();
    }

    private void OnClose_HideObj()
    {
        if (!CommonParam.mOpenDynamicUI)
            return;
        if (mGridTweenRoot != null)
            mGridTweenRoot.transform.localPosition = new Vector3(10000, 0, 0);
        if (mAlphaRoot != null)
            mAlphaRoot.transform.localPosition = new Vector3(10000, 0, 0);
        GameObject _transform_root_1 = GetSub("transform_tween_root");
        if (_transform_root_1 != null)
            _transform_root_1.transform.localPosition = new Vector3(10000, 0, 0);
        GameObject _transform_root_2 = GetSub("transform_tween_root_2");
        if (_transform_root_2 != null)
            _transform_root_2.transform.localPosition = new Vector3(10000, 0, 0);
    }

    public virtual void OnOpen(){}
    public virtual void OnClose() { }
    public override void onChange(string keyIndex, object objVal)
    {
        if (keyIndex == "OPEN")
        {           
            Open(objVal);
            OnOpen();

            if(!string.IsNullOrEmpty(mWinName))
                OnWindowOpenObserver.Instance.Notify(mWinName, this);
        }
        else if (keyIndex == "CLOSE")
        {
            Close();

            if (!string.IsNullOrEmpty(mWinName))
                OnWindowCloseObserver.Instance.Notify(mWinName, this);
        }
        else if (keyIndex == "REFRESH")
        {
            Refresh(objVal);

            if (!string.IsNullOrEmpty(mWinName))
                OnWindowRefreshObserver.Instance.Notify(mWinName, this);
        }
    }

    public virtual bool IsOpen() { if (mGameObjUI != null) return mGameObjUI.activeSelf; return false;  }

    public virtual bool IsVisible() { if (mGameObjUI != null) return mGameObjUI.activeInHierarchy; return false; }

    public virtual bool Refresh(object param) { return true; }

    public bool SetText(string lableName, string text)
    {
        return  GameCommon.SetUIText(mGameObjUI, lableName, text);
    }

    public bool SetCountdownTime(string lableName, Int64 serverOverTime)
    {
		return SetCountdown(mGameObjUI, lableName, serverOverTime, null) != null;
    }

    public bool SetCountdownTime(GameObject subObject, string lableName, Int64 serverOverTime)
    {
		return SetCountdown(subObject, lableName, serverOverTime, null) != null;
    }

    public bool SetCountdownTime(string lableName, Int64 serverOverTime, CallBack callBack)
    {
        return SetCountdown(mGameObjUI, lableName, serverOverTime, callBack) != null;
    }

    public bool SetCountdownTime(GameObject subObject, string lableName, Int64 serverOverTime, CallBack callBack)
    {
        return SetCountdown(subObject, lableName, serverOverTime, callBack) != null;
    }

    static public CountdownUI SetCountdown(GameObject subObject, string lableName, Int64 serverOverTime, CallBack callBack)
    {
        GameObject lableObj = GameCommon.FindObject(subObject, lableName);
        if (lableObj != null)
        {
            UILabel label = lableObj.GetComponent<UILabel>();
            if (label != null)
            {
                CountdownUI countDown = lableObj.AddComponent<CountdownUI>();
                countDown.mServerOverTime = serverOverTime;
                countDown.mFinishCallBack = callBack;
                return countDown;
            }
        }
        return null;
    }

	static public CountdownUI SetCountdown(GameObject subObject, string lableName, Int64 serverOverTime, CallBack callBack, bool newCountdown,string addedInfo = "")
	{
		GameObject lableObj = GameCommon.FindObject(subObject, lableName);
		if (lableObj != null)
		{
			UILabel label = lableObj.GetComponent<UILabel>();
			if (label != null)
			{
                //CountdownUI countDown = lableObj.AddComponent<CountdownUI>();
                CountdownUI countDown = lableObj.GetComponent<CountdownUI>() == null ? lableObj.AddComponent<CountdownUI>() : lableObj.GetComponent<CountdownUI>();
                if (countDown.isActiveAndEnabled == false)
                {
                    countDown.enabled = true;
                }

				countDown.svrRelativelyTime = serverOverTime;
				countDown.newCountdown = newCountdown;
				countDown.mFinishCallBack = callBack;
				countDown.mAddedStr = addedInfo;
				return countDown;
			}
		}
		return null;
	}

    //added by xuke
    /// <summary>
    /// 当倒计时为0的时候显示提示文字
    /// </summary>
    public void SetCountdownTimeWithEndInfo(bool kIsActivityOn,string kLblName, long kEndTime, CallBack kCallback, string kHintInfo)
    {
        if (kIsActivityOn)
        {
            SetCountdownTime(kLblName, kEndTime, kCallback);
            GameCommon.FindComponent<UILabel>(mGameObjUI, kLblName).SetLabelColor(CommonColorType.WHITE);
        }
        else
        {
            GameObject lableObj = GameCommon.FindObject(mGameObjUI, kLblName);
            if (lableObj != null)
            {
                CountdownUI _countDownUI = lableObj.GetComponent<CountdownUI>();
                if (_countDownUI != null)
                {
                    MonoBehaviour.Destroy(_countDownUI);
                }
                UILabel _timeLbl = lableObj.GetComponent<UILabel>();
                _timeLbl.text = kHintInfo;
                _timeLbl.SetLabelColor(CommonColorType.HINT_RED);
            }
        }
    }
    //end

    static public void ChangeNumber(GameObject parentObject, string lableName, float changeSpaceSecond, int beginNumber, int targetNumber)
    {
		GameObject lableObj = GameCommon.FindObject(parentObject, lableName);
        if (lableObj != null)
        {
            UILabel label = lableObj.GetComponent<UILabel>();
            if (label != null)
            {
                UpdateChangeNumber numObj = new UpdateChangeNumber();
                numObj.mNowNum = beginNumber;
                numObj.mTargetNum = targetNumber;
                numObj.mTextLable = label;
				label.text = beginNumber.ToString();
                Logic.EventCenter.WaitUpdate(numObj, "Update", null, changeSpaceSecond);
            }
        }
    }

    public GameObject GetSub(string objectName)
    {
        return GameCommon.FindObject(mGameObjUI, objectName);
    }

    public T GetComponent<T>(string objectName) where T : Component
    {
        GameObject obj = GetSub(objectName);
        if (obj != null)
            return obj.GetComponent<T>();

        return null;
    }

    public UISprite GetSprite(string spriteObjectName)
    {
        GameObject obj = GameCommon.FindObject(mGameObjUI, spriteObjectName);
        if (obj != null)
            return obj.GetComponent<UISprite>();

        return null;
    }

    public UILabel GetLable(string lableObjectName)
    {
        GameObject obj = GameCommon.FindObject(mGameObjUI, lableObjectName);
        if (obj != null)
            return obj.GetComponent<UILabel>();

        return null;
    }

    public void SetVisible(string subObject, bool bVisible)
    {
        GameCommon.SetUIVisiable(mGameObjUI, subObject, bVisible);
    }

	public void SetVisible(bool bVisible, params string[] uiNames)
	{
		GameCommon.SetUIVisiable (mGameObjUI, bVisible, uiNames);
	}

    public bool SetUISprite(string spriteObject, string atlas, string atlasSpriteName)
    {
        return GameCommon.SetUISprite(mGameObjUI, spriteObject, atlas, atlasSpriteName);
    }

    /// <summary>
    /// 追加委托，所追加的委托当 mGameObjUI 被隐藏或销毁时会被自动移除，不需要手工移除委托
    /// </summary>
    /// <param name="source"> 被追加的委托链 </param>
    /// <param name="value"> 追加的委托 </param>
    public void AppendDelegate(DelegateList source, Action value)
    {
        if (mGameObjUI != null)
        {
            mGameObjUI.AppendDelegate(source, value);
        }
    }

    public void AppendDelegate(DelegateList source, Action value, bool oneshot, bool overlap)
    {
        if (mGameObjUI != null)
        {
            mGameObjUI.AppendDelegate(source, value, oneshot, overlap);
        }
    }

    public void AppendDelegate<T>(DelegateList<T> source, Action<T> value)
    {
        if (mGameObjUI != null)
        {
            mGameObjUI.AppendDelegate<T>(source, value);
        }
    }

    public void AppendDelegate<T>(DelegateList<T> source, Action<T> value, bool oneshot, bool overlap)
    {
        if (mGameObjUI != null)
        {
            mGameObjUI.AppendDelegate<T>(source, value, oneshot, overlap);
        }
    }

    public void AppendDelegate<T1, T2>(DelegateList<T1, T2> source, Action<T1, T2> value)
    {
        if (mGameObjUI != null)
        {
            mGameObjUI.AppendDelegate<T1, T2>(source, value);
        }
    }

    public void AppendDelegate<T1, T2>(DelegateList<T1, T2> source, Action<T1, T2> value, bool oneshot, bool overlap)
    {
        if (mGameObjUI != null)
        {
            mGameObjUI.AppendDelegate<T1, T2>(source, value, oneshot, overlap);
        }
    }

    public void AppendDelegate<T1, T2, T3>(DelegateList<T1, T2, T3> source, Action<T1, T2, T3> value)
    {
        if (mGameObjUI != null)
        {
            mGameObjUI.AppendDelegate<T1, T2, T3>(source, value);
        }
    }

    public void AppendDelegate<T1, T2, T3>(DelegateList<T1, T2, T3> source, Action<T1, T2, T3> value, bool oneshot, bool overlap)
    {
        if (mGameObjUI != null)
        {
            mGameObjUI.AppendDelegate<T1, T2, T3>(source, value, oneshot, overlap);
        }
    }

    /// <summary>
    /// 开启窗口协程，协程的生命周期被限定在窗口生命周期内，当窗口被关闭时，未执行完的协程也会被终止
    /// </summary>
    /// <param name="co"> 要开启的协程 </param>
    /// <returns></returns>
    public Coroutine DoCoroutine(IEnumerator co)
    {
        if (mGameObjUI != null)
        {
            return mGameObjUI.StartCoroutine(co);
        }

        return null;
    }

    /// <summary>
    /// 延迟执行一个行为，若当窗口被关闭时该行为还未执行则会被取消
    /// </summary>
    /// <param name="action"> 被延迟执行的行为 </param>
    /// <param name="delay"> 延迟时间 </param>
    public void DoLater(Action action, float delay)
    {
        if (mGameObjUI != null)
        {
            mGameObjUI.ExecuteDelayed(action, delay);
        }
    }

    protected T GetCurUIComponent<T>(string gameObjectName) where T : Component
    {

        return GameCommon.FindComponent<T>(mGameObjUI, gameObjectName);
    }

    protected GameObject GetCurUIGameObject(string gameObjectName)
    {
        return GameCommon.FindObject(mGameObjUI, gameObjectName);
    }

    protected GameObject GetCurUIGameObject(string gameObjectName,bool isActive)
    {
        var go=GameCommon.FindObject(mGameObjUI, gameObjectName);
        if(go!=null) go.SetActive(isActive);
        return go;
    }

    protected UILabel GetUILabel(string name)
    {
        return GetCurUIComponent<UILabel>(name);
    }

    protected UIInput GetUIInput(string name)
    {
        return GetCurUIComponent<UIInput>(name);
    }

    protected UIGridContainer GetUIGridContainer(string name)
    {
        return GetCurUIComponent<UIGridContainer>(name);
    }

    protected UIGridContainer GetUIGridContainer(string name,int maxCount)
    {
        var grid = GetCurUIComponent<UIGridContainer>(name);
        grid.MaxCount = maxCount;
        return grid;
    }

    protected UIToggle GetUIToggle(string name)
    {
        return GetCurUIComponent<UIToggle>(name);
    }
    protected UIToggle GetUIToggle(string name,bool isOpen)
    {
        var toggle = GetCurUIComponent<UIToggle>(name);
        toggle.value = isOpen;
        return toggle;
    }

    protected UISlider GetUISlider(string name)
    {
        
        return GetCurUIComponent<UISlider>(name);
    }


    protected void AddButtonAction(GameObject button, Action action)
    {
        var evt = button.GetComponent<UIButtonEvent>();
        if (evt == null) DEBUG.LogError("No exist button event > " + button.name);
        else evt.AddAction(action);
    }
    protected void AddButtonAction(string name,Action action)
    {
        var evt = GetCurUIGameObject(name).GetComponent<UIButtonEvent>();
        if (evt == null) DEBUG.LogError("No exist button event > " + name);
        else evt.AddAction(action);
    }

    private List<string> mWindowList = null;

    /// <summary>
    /// 打开子窗口
    /// 被打开的窗口当本窗口关闭时会被自动关闭
    /// </summary>
    /// <param name="winName"> 子窗口的名称 </param>
    /// <param name="param"> 子窗口的参数 </param>
    public void OpenSubWindow(string winName, object param)
    {
        if (mWindowList == null)
        {
            mWindowList = new List<string>();
        }

        if (!mWindowList.Contains(winName))
        {
            mWindowList.Add(winName);
        }

        DataCenter.OpenWindow(winName, param);
    }

    /// <summary>
    /// 打开子窗口
    /// 被打开的窗口当本窗口关闭时会被自动关闭 
    /// </summary>
    /// <param name="winName"> 子窗口的名称 </param>
    public void OpenSubWindow(string winName)
    {
        OpenSubWindow(winName, true);
    }


    /// <summary>
    /// 关闭所有的子窗口
    /// 子窗口是指通过方法 OpenSubWindow() 打开的窗口
    /// </summary>
    public void CloseAllSubWindows()
    {
        if (mWindowList != null)
        {
            foreach (string win in mWindowList)
            {
                DataCenter.CloseWindow(win);
            }

            mWindowList.Clear();
        }
    }
    //by chenliang
    //begin

    protected UIWrapGrid GetUIWrapGrid(string name)
    {
        return GetCurUIComponent<UIWrapGrid>(name);
    }

    //end
}

public abstract class tWindowUI : MonoBehaviour
{
    public string mWinName;

    public abstract void InitUI(string winName);

    void CreateUI(string winName)
    {
        mWinName = winName;      
		InitUI(mWinName);   
    }

    void OnDestroy()
    {
        DataCenter.Remove(mWinName);
    }
}

// update total time
public class CountdownUI : MonoBehaviour
{
    public System.Int64 mServerOverTime = 0; //secord
    public CallBack mFinishCallBack;
	public bool newCountdown;
	public float svrRelativelyTime;
	public string mAddedStr;
    void Start()
    {
        UILabel label = gameObject.GetComponent<UILabel>();
        if (label == null)
        {
            Logic.EventCenter.Log(LOG_LEVEL.ERROR, "CountdownUI must append to UILable");
            enabled = false;
        }
    }

    void Update()
    {
        double useTime = mServerOverTime - CommonParam.NowServerTime();

		if(newCountdown) {
			svrRelativelyTime -= Time.deltaTime;
			useTime = svrRelativelyTime;
		}
        //by chenliang
        //begin

//         if (useTime >= 0)
//         {
//             UILabel label = gameObject.GetComponent<UILabel>();
//             if (label != null)
//                 label.text = GameCommon.FormatTime(useTime);
//         }
//         else
//         {
//             if (mFinishCallBack != null)
//                 mFinishCallBack.Run();
//             enabled = false;
//         }
//----------------------
        UILabel label = gameObject.GetComponent<UILabel>();
        if (label != null)
        {
            long tmpUseTime = (long)useTime;
            label.text = _GetTimeFormatTime(tmpUseTime > 0 ? tmpUseTime : 0) + mAddedStr;
        }

        if(useTime < 0.0)
        {
            enabled = false;
            if (mFinishCallBack != null)
                mFinishCallBack.Run();
        }

        //end
    }

    //by chenlaing
    //begin

    /// <summary>
    /// 获取时间格式化后的字符串
    /// </summary>
    /// <param name="useTime">剩余可使用时间，以秒为单位</param>
    /// <returns></returns>
    protected virtual string _GetTimeFormatTime(long leftUseTime)
    {
        return GameCommon.FormatTime(leftUseTime);
    }

    //end
}

//by chenliang
//begin

public enum MAX_SHOWTIME_COUNTDOWN_TIME_TYPE
{
    DAYS,
    HOURS,
    MINUTES,
    SECONDS
}
/// <summary>
/// 可设置最大显示时间（天、时、分、秒），最大时间如果为-1，表示无最大限制
/// </summary>
public class MaxShowTimeCountdownUI : CountdownUI
{
    public bool IsUseDaysTime { set; get; }         //是否显示天数
    public int Days { set; get; }                   //最大天数
    public int Hours { set; get; }                  //最大小时
    public int Minutes { set; get; }                //最大分
    public int Seconds { set; get; }                //最大秒

    public MaxShowTimeCountdownUI()
    {
        Days = 0;
        Hours = 0;
        Minutes = 0;
        Seconds = 0;
    }

    public int this[MAX_SHOWTIME_COUNTDOWN_TIME_TYPE timeType]
    {
        set
        {
            switch ((int)timeType)
            {
                case 0: Days = value; break;
                case 1: Hours = value; break;
                case 2: Minutes = value; break;
                case 3: Seconds = value; break;
            }
        }
        get
        {
            int v = 0;
            switch ((int)timeType)
            {
                case 0: v = Days; break;
                case 1: v = Hours; break;
                case 2: v = Minutes; break;
                case 3: v = Seconds; break;
            }
            return v;
        }
    }

    protected override string _GetTimeFormatTime(long leftUseTime)
    {
        TimeSpan tmpSpan = new TimeSpan(leftUseTime * TimeSpan.TicksPerSecond);
        int[] tmpRetTime = new int[] {
            tmpSpan.Days,
            tmpSpan.Hours,
            tmpSpan.Minutes,
            tmpSpan.Seconds
        };
        //如果不显示天数，将天数加到小时里
        if (!IsUseDaysTime)
        {
            tmpRetTime[(int)MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.HOURS] += (tmpRetTime[(int)MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.DAYS] * 24);
            tmpRetTime[(int)MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.DAYS] = 0;
        }
        for (int i = IsUseDaysTime ? (int)MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.DAYS : (int)MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.HOURS; i <= (int)MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.SECONDS; i++)
        {
            int tmpSrcTime = tmpRetTime[i];
            int tmpDstTime = this[(MAX_SHOWTIME_COUNTDOWN_TIME_TYPE)i];
            if(tmpSrcTime == tmpDstTime)
                continue;
            if (tmpDstTime != -1 && tmpSrcTime > tmpDstTime)
            {
                tmpRetTime[i] = tmpDstTime;
                for (int j = i + 1; j < 4; j++)
                    tmpRetTime[j] = 0;
            }
            break;
        }
        return _FormatTime(tmpRetTime);
    }
    /// <summary>
    /// 格式化时间
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    protected virtual string _FormatTime(int[] time)
    {
        string tmpRet = "";
        for (int i = (int)MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.DAYS; i <= (int)MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.SECONDS; i++)
        {
            if (time[i] == 0 && i == (int)MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.DAYS)
                continue;
            tmpRet += time[i].ToString();
            switch ((MAX_SHOWTIME_COUNTDOWN_TIME_TYPE)i)
            {
                case MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.DAYS: tmpRet += "天"; break;
                case MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.HOURS:
                case MAX_SHOWTIME_COUNTDOWN_TIME_TYPE.MINUTES: tmpRet += ":"; break;
            }
        }
        return tmpRet;
    }
}

/// <summary>
/// 无界面倒计时
/// </summary>
public class CountdownNoUI : MonoBehaviour
{
    public System.Int64 mServerOverTime = 0; //secord
    public CallBack mFinishCallBack;
    public bool newCountdown;
    public float svrRelativelyTime;

    void Update()
    {
        double useTime = mServerOverTime - CommonParam.NowServerTime();

        if (newCountdown)
        {
            svrRelativelyTime -= Time.deltaTime;
            useTime = svrRelativelyTime;
        }

        if (useTime < 0.0)
        {
            enabled = false;
            if (mFinishCallBack != null)
                mFinishCallBack.Run();
        }
    }
}

//end

public class UpdateChangeNumber
{
    public UILabel mTextLable;
    public int mTargetNum;
    public int mNowNum;

    public bool Update(object p)
    {
        if (mTextLable!=null)
        {
            if (mTargetNum>mNowNum)
                ++mNowNum;
            else if (mTargetNum < mNowNum)
                --mNowNum;
            else
            {
                mTextLable.text = mNowNum.ToString();
                return false;
            }

            mTextLable.text = mNowNum.ToString();
        }
        return true;
    }

   
}