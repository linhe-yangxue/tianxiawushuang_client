using UnityEngine;
using System.Collections;

public class ChangeParticleRenderQueue : MonoBehaviour {

	public int renderQueue = 2994;   
    public bool includeParent = true;
    public bool oneShot = true;
    public bool IsChangeSharedMat = true;
	void Start()  
	{
        updateAllObjRenderQueue(includeParent ? gameObject.transform.parent.gameObject : gameObject);
	}  
	
	void Update()  
	{
        if (!oneShot)
        {
            updateAllObjRenderQueue(includeParent ? gameObject.transform.parent.gameObject : gameObject);
        }
	}

	void updateAllObjRenderQueue(GameObject parentObj)
	{
		if(parentObj != null)
		{
			updateObjRenderQueue(parentObj);

			foreach(Transform trans in parentObj.transform)
			{
				updateAllObjRenderQueue(trans.gameObject);
			}
		}
	}

	void updateObjRenderQueue(GameObject parentObj)
	{
		if(parentObj != null)
		{
			if (parentObj != null && parentObj.renderer != null && parentObj.renderer.sharedMaterial != null && parentObj.renderer.sharedMaterial.renderQueue != renderQueue)  
			{
                if (IsChangeSharedMat)
                {
                    parentObj.renderer.sharedMaterial.renderQueue = renderQueue;
                }
                else 
                {
                    parentObj.renderer.material.renderQueue = renderQueue;                    
                }
            }  
		}
	}
}

