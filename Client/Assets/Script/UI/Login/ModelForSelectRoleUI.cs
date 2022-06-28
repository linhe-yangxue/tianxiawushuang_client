using UnityEngine;
using System.Collections;

public class ModelForSelectRoleUI : MonoBehaviour 
{
	
	public int mModelIndexOfUIConfig = 1000;
	public int mBirthConfigIndex = 0;
	public int mOwner = 1000;

	void Awake()
	{

        mBirthConfigIndex = TableManager.GetData("RoleUIConfig", mModelIndexOfUIConfig, "MODEL");

	}

	void Start()
	{
		
		BaseObject obj = ObjectManager.Self.CreateObject(mBirthConfigIndex);
		if (obj != null)
		{
			obj.SetCamp(mOwner);
			obj.mMainObject.transform.parent = transform.parent;
			Vector3 pos = transform.position;
			obj.SetPosition(pos);
			obj.mMainObject.transform.localScale *= 0.5f;
			
			
			obj.mMainObject.transform.Rotate(transform.up,		180, Space.World);
			
			SetCollider(obj);
			
			obj.SetVisible(true);

			obj.OnIdle();

            GameCommon.SetLayer(obj.mMainObject, CommonParam.UILayer);
            
			//Destroy(gameObject);
			gameObject.SetActive(false);
		}
	}

    void OnDestroy()
    {
        ObjectManager.Self.ClearAll();
    }
	
	void SetCollider(BaseObject obj)
	{
		if(obj != null)
		{
			CapsuleCollider coll = obj.mMainObject.GetComponent<CapsuleCollider>();
			if(coll == null)
			{
				coll = obj.mMainObject.AddComponent<CapsuleCollider>();
				coll.center = new Vector3(0, 1, 0);
				coll.height = 2;
				coll.isTrigger = true;
			}

			UIRoleAction mastBtn = obj.mMainObject.AddComponent<UIRoleAction>();
            mastBtn.mModelIndexOfUIConfig = mModelIndexOfUIConfig;
            mastBtn.mModel = obj;
			
		}
	}
}

