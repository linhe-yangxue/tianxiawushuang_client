using UnityEngine;
using System.Collections;

public class ItemPackageBaseWindow : tWindow {

	public UIGridContainer mGrid;
	
	public int mCurSelGridIndex = 0;
	
	public override void Init ()
	{
		base.Init ();
	}
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);

	}
	
	public override void OnOpen ()
	{
		base.OnOpen ();
		
		mGrid = mGameObjUI.transform.Find("group/scroll_view/grid").GetComponent<UIGridContainer>();
		mGrid.MaxCount = 0;

		InitVariable();
	}

	public virtual void InitVariable()
	{
	}
	
	public override bool Refresh (object param)
	{
		base.Refresh (param);
		mCurSelGridIndex = (int)param;
		
		return true;
	}
	
	
	public virtual void InitPackageIcons(){}
	
	public virtual void RefreshBagNum(){}
	
	public virtual void RefreshInfoWindow(){}
}