using UnityEngine;
using System.Collections;

public class ChatGrid : DynamicGrid
{
	public GameObject controlTemplate;
	public UIScrollBar uiScrollBar;

	[HideInInspector]
	[SerializeField]
	private int maxCount = 0;
	[ExposeProperty]
	public int MaxCount
	{
		get { return maxCount; }
		set
		{
			if (maxCount == value)
			{
				return;
			}
			maxCount = value;
			RebuildCells();
		}
	}
	
	private void RebuildCells()
	{
		if (controlTemplate == null)
			return;
		
		foreach (GameObject c in itemList)
		{
			if (c == null)
				continue;

			DestroyImmediate(c);
		}
		
		itemList.Clear();
		for (int i = 0; i < maxCount; i++)
		{
			if (itemList.Count >= maxCount)
				break;
			
			GameObject c = UnityEngine.Object.Instantiate(controlTemplate) as GameObject;
			c.name += "_" + i.ToString();
			c.transform.parent = this.transform;
			c.transform.localScale = Vector3.one;
			c.transform.localPosition = Vector3.zero;
			
			itemList.Add(c);
		}
		
		if (uiScrollBar != null)
			uiScrollBar.scrollValue = 1;
		
		ResetPosition ();
	}

	public void AddCell()
	{
		maxCount ++;
		GameObject c = UnityEngine.Object.Instantiate(controlTemplate) as GameObject;
		c.name += "_" + maxCount.ToString();
		c.transform.parent = this.transform;
		c.transform.localScale = Vector3.one;
		c.transform.localPosition = Vector3.zero;

		itemList.Add(c);

		if (uiScrollBar != null)
			uiScrollBar.scrollValue = 1;
		
		ResetPosition ();
	}

}
