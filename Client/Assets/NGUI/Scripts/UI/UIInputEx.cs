//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY)
#define MOBILE
#endif

using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Input field makes it possible to enter custom information within the UI.
/// </summary>

[AddComponentMenu("NGUI/UI/Input Field")]
public class UIInputEx : UIInput
{
	/// <summary>
	/// Selection event, sent by the EventSystem.
	/// </summary>

	protected override void OnSelect (bool isSelected)
	{
		base.OnSelect(isSelected);
		if (isSelected)
			value = "";
	}
}
