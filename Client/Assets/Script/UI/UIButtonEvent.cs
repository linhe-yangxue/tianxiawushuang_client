//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------


using UnityEngine;
using Logic;
using Utilities;
using System;
using DataTable;

/// <summary>
/// Sends a message to the remote object when something happens.
/// </summary>
[AddComponentMenu("NGUI/Interaction/Button Message (Legacy)")]
public class UIButtonEvent : MonoBehaviour
{
	public bool mbIsNeedJudge = true;
    public NiceData mData = new NiceData();
    public Action mAction = null;
    private Action mOnPressing;
    
    
    public int btnID=0;
    DataRecord _record;
    public DataRecord record{
        get { 
            if(_record==null) _record=TableManager.GetTable("FunctionConfig").GetRecord(btnID);
            return _record;
        }
    }
    
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
		OnDoubleClick,
	}

	public Trigger trigger = Trigger.OnClick;
	public bool includeChildren = false;
	
	bool mStarted = false;
    bool mIsPressed = false;
    public static bool can_press=true;

    
	void Start () 
    {

        mStarted = true;

        mData.set("NAME", gameObject.name);

		UIToggle uiToggle = gameObject.GetComponent<UIToggle>();
		if(uiToggle != null)
			mbIsNeedJudge = false;

        WindowInfo info = this.gameObject.FindComponentUpwards<WindowInfo>();
        mData.set("OWNER_WINDOW", info == null ? "" : info.winName);
    }
	
	void OnEnable () {
        if(mStarted) {
            OnHover(UICamera.IsHighlighted(gameObject));
        }

        if(btnID==0) return;

        //DEBUG.l
        int conditonType=record.getData("FUNC_CONDITION");
        if(conditonType==1) {
            // add by LC
            bool isOpen = GameCommon.IsFuncCanUse(btnID);
            // end
            string componentName=record.getData("FUNC_RESOURCE");
            bool isNeedChangeIcon=componentName!="0";
            if(isOpen&&isNeedChangeIcon) {
                var sprite=(gameObject.name==componentName)?gameObject.GetComponent<UISprite>():GameCommon.FindObject(gameObject,componentName).GetComponent<UISprite>();
                GameCommon.SetIcon(sprite,record.getData("ICON_ATLAS_NAME_OPEN"),record.getData("SPRITE_NAME_OPEN"));
            }
            if(!isOpen) {
                if(isNeedChangeIcon) {
                    var sprite=(gameObject.name==componentName)?gameObject.GetComponent<UISprite>():GameCommon.FindObject(gameObject,componentName).GetComponent<UISprite>();
                    GameCommon.SetIcon(sprite,record.getData("ICON_ATLAS_NAME_CLOSE"),record.getData("SPRITE_NAME_CLOSE"));
                }
            }
        }    

        
    }
	
	void OnHover (bool isOver)
	{
		if (enabled)
		{
			if (((isOver && trigger == Trigger.OnMouseOver) ||
			     (!isOver && trigger == Trigger.OnMouseOut))) Send();
		}
	}
	
	void OnPress (bool isPressed)
	{
        if (can_press)
        {
            mIsPressed = isPressed;
        }
       

		if (enabled)
		{
			if (((isPressed && trigger == Trigger.OnPress) ||
			     (!isPressed && trigger == Trigger.OnRelease))) Send();
		}
	}
	
	void OnSelect (bool isSelected)
	{
		if (enabled && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
			OnHover(isSelected);
	}
	
	void OnClick () {
		if (enabled && trigger == Trigger.OnClick) Send(); 
    }
	
	void OnDoubleClick () { 
        if (enabled && trigger == Trigger.OnDoubleClick) Send(); 
    }
	


	public virtual void Send ()
	{
        if (btnID != 0)
        {
            if (null == record)
            {
                DEBUG.LogError("btnID = " + btnID + " is not exist!");
            }
            else if (record.getData("FUNC_OPEN") == 0)
            {
                DataCenter.ErrorTipsLabelMessage(record.getData("FUNC_CLOSE_DESCRIBE"));
                return;
            }
            else
            {
                int conditonType=record.getData("FUNC_CONDITION");
                if(conditonType==1) {
                    // add by LC
                    bool isOpen = GameCommon.IsFuncCanUse(btnID);
                    // end
                    string componentName=record.getData("FUNC_RESOURCE");
                    bool isNeedChangeIcon=componentName!="0";
                    if(isOpen&&isNeedChangeIcon){
                        var sprite=(gameObject.name==componentName)?  gameObject.GetComponent<UISprite>():GameCommon.FindObject(gameObject,componentName).GetComponent<UISprite>();
                        GameCommon.SetIcon(sprite,record.getData("ICON_ATLAS_NAME_OPEN"),record.getData("SPRITE_NAME_OPEN"));
                    }
                    if(!isOpen){
                        DataCenter.ErrorTipsLabelMessage(record.getData("FUNC_CONDITION_DESCRIBE"));
                        if(isNeedChangeIcon) {
                            var sprite=(gameObject.name==componentName)?gameObject.GetComponent<UISprite>():GameCommon.FindObject(gameObject,componentName).GetComponent<UISprite>();
                            GameCommon.SetIcon(sprite,record.getData("ICON_ATLAS_NAME_CLOSE"),record.getData("SPRITE_NAME_CLOSE"));
                        }
                        return;
                    }
                }
            }
        }

        string evtName = "Button_";
        evtName += gameObject.name;
//        DEBUG.LogError("------------> evtName=" + evtName);
        Logic.tEvent evt = Logic.EventCenter.Start(evtName);
        if (mAction != null) mAction();
        else if (evt != null)
        {
			if(!GlobalModule.bIsCanClick(mbIsNeedJudge))
				return;
            evt.setData(mData);
            mData.set("BUTTON", gameObject);
            //if (mData.getObject("ACTION")!=null) ((Action)mData.getObject("ACTION"))();
            //evt.set("ACTION", mAction);
            evt.DoEvent();

            OnButtonEventObserver.Instance.Notify(evtName, evt);
        }
        else if(mOnPressing == null) DEBUG.LogError("No exist button event > " + evtName);
        
	}

    public void AddAction(Action action)
    {
        mAction=action;
    }

    public void BanPress(bool canPress)
    {
        mIsPressed = canPress;
    }
    /// <summary>
    /// 设置按住按钮时的回调
    /// 该回调在Update时序中执行
    /// 从开始按住按钮，到松开按钮期间该回调会每帧执行
    /// 在此期间即使插入遮罩，也不会打断该回调的执行
    /// </summary>
    /// <param name="action"> 欲设置的回调 </param>
    public void SetOnPressingAction(Action action)
    {
        mOnPressing = action;
    }

    private void Update()
    {
        if (enabled && mIsPressed && mOnPressing != null)
        {
            mOnPressing();
        }
    }

    public int GetbtnID()
    {
        return btnID;
    }
}