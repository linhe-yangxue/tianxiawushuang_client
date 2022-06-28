//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sends a message to the remote object when something happens.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Message (Legacy)")]
public class UISelRoleMastButton : UISelModelMastButton
{
	public override void Init()
	{
		mModelMaxNum = 3;
		
		base.Init();
	}

	public override void InitModel (int iIndex)
	{
        return;
		if(iIndex == 2)
		{
			UIGridContainer cardGrid = transform.GetComponent<UIGridContainer>();

			Transform group_2_Trans = cardGrid.controlList[0].transform.Find("group/UIPoint/group_2");
			if(group_2_Trans != null)
			{
				SkinnedMeshRenderer rend = group_2_Trans.GetComponentInChildren<SkinnedMeshRenderer>();
				if(rend != null)
				{
					rend.material.color = new Color(0,0,0);
				}

				group_2_Trans.localScale = Vector3.one;
				group_2_Trans.localScale = group_2_Trans.localScale * 0.75f;
			}
		}
	}
	public override int GetBirthConfigIndex(int iIndex)
	{
		return TableManager.GetData("RoleUIConfig", iIndex, "MODEL");
	}

	public override void SetSelModelIndex()
	{
		DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "SET_INDEX", mCurModelIndex);
	}

	public override void HideModelEffect ()
	{
		DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "HIDE_ALL_EFFECT", true);
	}
}
