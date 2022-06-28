//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sends a message to the remote object when something happens.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Message (Legacy)")]
public class UISelPetMastButton : UISelModelMastButton
{
	public override void Init()
	{
		mModelMaxNum = (int)ELEMENT_TYPE.MAX;

		base.Init();
	}

	public override int GetBirthConfigIndex(int iIndex)
	{
		return TableCommon.GetNumberFromSelPetUIConfig(iIndex, "MODEL");
	}

	public override void SetSelModelIndex()
	{		
		DataCenter.SetData("SELECT_PET_WINDOW", "SET_INDEX", mCurModelIndex);
//		DataCenter.SetData("SELECT_PET_WINDOW", "SHOW_ELEMENT_EFFECT", true);
	}

	public override void HideModelEffect ()
	{
		DataCenter.SetData("SELECT_PET_WINDOW", "HIDE_ALL_EFFECT", true);
	}
}
