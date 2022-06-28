using UnityEngine;  
using System.Collections;  

/// <summary>  
/// This is a container to deal with the particles render by control the render queue.  
/// </summary>  
[ExecuteInEditMode]  
public class UIParticles : MonoBehaviour  
{  
	private const float UPDATE_RENDER_TIME = 0.2f;  
	private float lastTime = 0f;  
	private Renderer[] rendererArray = null;  
	private bool isWidgetOK = false;  
	private bool isRendererArrayOK = false;  
	private Renderer tempMeshRenderer = null;  
	
	public bool isExplicit = false;  
	public int RenderQueue = 3000;  
	public UIWidget parentWidget = null;  
	public bool IsForward = true;  
	
	void OnDestroy()  
	{  
		rendererArray = null;  
		parentWidget = null;  
	}  
	
	void LateUpdate()  
	{  
		lastTime += Time.deltaTime;  
		if (lastTime < UPDATE_RENDER_TIME)  
			return;  
		lastTime = -Random.Range(0, UPDATE_RENDER_TIME);  
		
		if (parentWidget == null)  
		{  
			parentWidget = NGUITools.FindInParents<UIWidget>(this.gameObject);  
		}  
		
		if (rendererArray == null || rendererArray.Length == 0)  
		{  
			rendererArray = this.GetComponentsInChildren<Renderer>(true);  
		}  
		
		isWidgetOK = parentWidget != null && parentWidget.drawCall != null;  
		isRendererArrayOK = rendererArray != null && rendererArray.Length > 0;  
		
		if ((isWidgetOK || isExplicit) && (isRendererArrayOK))  
		{  
			OnChangeRenderQueue();  
		}  
	}  
	
	void OnChangeRenderQueue()  
	{  
		int curRenderQueue = (isExplicit || !isWidgetOK) ? RenderQueue : parentWidget.drawCall.finalRenderQueue;  
		
		if (IsForward)  
			curRenderQueue += 1;  
		else  
			curRenderQueue -= 1;  
		
		if (curRenderQueue != RenderQueue)  
		{  
			RenderQueue = curRenderQueue;  
			for (int i = 0; i != rendererArray.Length; ++i)  
			{  
				tempMeshRenderer = rendererArray[i];  
				if (tempMeshRenderer != null)  
				{  
					#if UNITY_EDITOR  
					tempMeshRenderer.sharedMaterial.renderQueue = RenderQueue;  
					#else  
					tempMeshRenderer.material.renderQueue = RenderQueue;  
					#endif  
				}  
			}  
		}  
	}  
}  