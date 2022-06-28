//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sends a message to the remote object when something happens.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Message (Legacy)")]
public class UIMastButton : MonoBehaviour
{
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
		OnDoubleClick,
	}
	
	public GameObject target;
	public string functionName;
	public Trigger trigger = Trigger.OnClick;
	public bool includeChildren = false;
	
	bool mStarted = false;
	
	void Start () { mStarted = true; }
	
	void OnEnable () { if (mStarted) OnHover(UICamera.IsHighlighted(gameObject)); }
	
	void OnHover (bool isOver)
	{

	}

    void OnDragStart()
    {
        MainProcess.Self.OnDragStart();
    }

    void OnDrag(Vector2 delta)
    {
        MainProcess.Self.OnDrag(delta);
    }

    void OnDragEnd() 
    {
        MainProcess.Self.OnDragEnd();
    }
	
	void OnPress (bool isPressed)
	{
		if (enabled)
		{
			if (((isPressed && trigger == Trigger.OnPress) ||
			     (!isPressed && trigger == Trigger.OnRelease))) 
                MainProcess.Self.OnPress(isPressed);
            
            MainProcess.Self.OnPressing(isPressed);
		}
	}


	
	void OnSelect (bool isSelected)
	{
		if (enabled && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
			MainProcess.Self.OnSelect();
	}
	
	void OnClick () { if (enabled && trigger == Trigger.OnClick) MainProcess.Self.OnClick(); }
	
	void OnDoubleClick () { if (enabled && trigger == Trigger.OnDoubleClick) Send(); }
	
	void Send ()
	{

	}
}
