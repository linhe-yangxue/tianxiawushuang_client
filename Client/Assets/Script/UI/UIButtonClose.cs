//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using Logic;

/// <summary>
/// Sends a message to the remote object when something happens.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Message (Legacy)")]
public class UIButtonClose : MonoBehaviour
{
    public string mCloseWindowName;

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
	
	void Start () 
    { 
        mStarted = true;
		        
    }
	
	void OnEnable () { if (mStarted) OnHover(UICamera.IsHighlighted(gameObject)); }
	
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
	
	void OnClick () { if (enabled && trigger == Trigger.OnClick) Send(); }
	
	void OnDoubleClick () { if (enabled && trigger == Trigger.OnDoubleClick) Send(); }
	
	void Send ()
	{
        DataCenter.CloseWindow(mCloseWindowName);
	}

}
