//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sends a message to the remote object when something happens.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Message (Legacy)")]
public class UISelModelMastButton : MonoBehaviour
{
	public GameObject mModelRoot;
	public GameObject[] mUIPetVec;
	public float mCardScale = 1.0f;
	public float mScaleRatio = 1.0f;
	public float mDuration = 0.5f;
	
	public int miIndex = 0;
	public int mCurModelIndex = 0;

	[HideInInspector]
	public int mModelMaxNum = (int)ELEMENT_TYPE.MAX;
	
	bool mDragging = false;
	
	void Start ()
	{

	}

	public virtual void Init()
	{
        miIndex = 0;
	    mCurModelIndex = 0;

		UIGridContainer grid = gameObject.GetComponent<UIGridContainer>();
		grid.MaxCount = 1;
		mModelRoot = grid.controlList[0].transform.Find("group/UIPoint").gameObject;
		
		for(int i = 0; i < mModelMaxNum; i++)
		{
			mUIPetVec[i] = mModelRoot.transform.Find("group_" + i.ToString() + "/model/UIPoint").gameObject;
			InitActiveBirthForUI(i);

			InitModel(i);
		}
	}
	
	public virtual void InitActiveBirthForUI(int iIndex)
	{
		GameObject petObj = mUIPetVec[iIndex];
		ActiveBirthSelModelForUI activeBirthForUI = petObj.GetComponent<ActiveBirthSelModelForUI>();
		if (activeBirthForUI != null)
		{
			int iChildCount = petObj.transform.parent.childCount;
			if (iChildCount > 1)
			{
				for (int i = 0; i < iChildCount; i++)
				{
					GameObject obj = petObj.transform.parent.GetChild(i).gameObject;
					if (obj == petObj)
					{
						continue;
					}
					else
					{
						activeBirthForUI.OnDestroy();
					}
				}
			}
			activeBirthForUI.mBirthConfigIndex = GetBirthConfigIndex(iIndex);
			
			activeBirthForUI.Init(false, mScaleRatio * mCardScale, true);
		}
	}

	public virtual void InitModel(int iIndex){}

	public virtual int GetBirthConfigIndex(int iIndex)
	{
		return 0;
	}
	
	public virtual void OnDrag(Vector2 delta)
	{
		if(!mDragging)
		{
			bool bIsCW = false;
			
			if(delta.x > 10)
			{
                //DEBUG.LogError("delta.x1 = " + delta.x.ToString());
				bIsCW = false;
				miIndex--;
				mCurModelIndex = (mCurModelIndex + mModelMaxNum - 1)% mModelMaxNum;
				mDragging = true;
			}
			else if(delta.x < -10)
			{
                //DEBUG.LogError("delta.x2 = " + delta.x.ToString());                
				bIsCW = true;
				miIndex++;
				mCurModelIndex = (mCurModelIndex + 1)% mModelMaxNum;
				mDragging = true;

                //DEBUG.LogError("miIndex = " + miIndex.ToString());
                //DEBUG.LogError("mCurModelIndex = " + mCurModelIndex.ToString());
			}
			
			if(mDragging)
			{
                //DEBUG.LogError("miIndex * 360 / mModelMaxNum = " + (miIndex * 360 / mModelMaxNum).ToString());
				TweenRotation.Begin(mModelRoot, mDuration, Quaternion.Euler(new Vector3(0, miIndex * 360 / mModelMaxNum, 0)));
				
				for(int i = 0; i < mUIPetVec.Length; i++)
				{
					TweenRotation.Begin(mUIPetVec[i].transform.parent.parent.gameObject, mDuration, Quaternion.Euler(new Vector3(0, miIndex * - 360 / mModelMaxNum, 0)));
				}

				HideModelEffect();
			}
		}
		
		
		//		mModelRoot.transform.Rotate(new Vector3(0, -delta.x, 0));
		//
		//
		//		for(int i = 0; i < mUIPetVec.Length; i++)
		//		{
		//			mUIPetVec[i].transform.parent.Rotate(new Vector3(0, delta.x, 0));
		//		}
	}

	public virtual void HideModelEffect()
	{

	}

	public void Update()
	{
		if(mDragging)
		{
			int fD = (int)(mModelRoot.transform.localEulerAngles.y - mCurModelIndex * 360 / mModelMaxNum);
			if(fD == 0)
			{
				MoveFisnish();
			}
		}
	}
	public void MoveFisnish()
	{
		mDragging = false;

		SetSelModelIndex();
		
		PlayAnimition();
	}

	public virtual void SetSelModelIndex()
	{
	}
	
	public void PlayAnimition()
	{
		for(int i = 0; i < mModelMaxNum; i++)
		{
			GameObject petObj = mUIPetVec[i];
			ActiveBirthSelModelForUI activeBirthForUI = petObj.GetComponent<ActiveBirthSelModelForUI>();
			if (activeBirthForUI != null && activeBirthForUI.mActiveObject != null)
			{
				if(mCurModelIndex == i)
				{
					Logic.tEvent idleEvent = Logic.EventCenter.Start("PetSelUI_PlayAttackEvent");
					idleEvent.set("INDEX", 1);
					activeBirthForUI.mActiveObject.PlayMotion("attack", idleEvent);
					//					activeBirthForUI.mActiveObject.PlayMotion("cute", idleEvent);
				}
				else
				{
					activeBirthForUI.mActiveObject.PlayAnim("idle");
				}
			}
		}
	}
	
	void OnClick()
	{
		//		if(mObject != null)
		//		{
		//	        Logic.tEvent idleEvent = Logic.EventCenter.Self.StartEvent("RoleSelUI_PlayIdleEvent");
		//			mObject.PlayMotion("cute", idleEvent);
		//		}
	}
	
	void OnDragEnd()
	{
		//		if(!mDragging)
		//			MoveFisnish();
		//		mDragging = false;
	}
	
	
}
