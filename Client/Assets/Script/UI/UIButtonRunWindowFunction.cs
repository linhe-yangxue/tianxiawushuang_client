using UnityEngine;
using System.Collections;

public class UIButtonRunWindowFunction : UIButtonEvent
{
	public string WindowName;
	public string WindowFunctionName;


	public override void Send ()
	{
		NiceData butData = GameCommon.GetButtonData(gameObject);
		tLogicData win = DataCenter.GetData(WindowName);
		if (win!=null)
		{
			System.Reflection.MethodInfo method = win.GetType().GetMethod(WindowFunctionName);
			if (method!=null)
			{
				object[] p = new object[1];
				p[0] = butData;
				try{
					method.Invoke(win, p);
				}
				catch (System.Exception e)
				{
					DEBUG.LogError(e.ToString());
				}
			}
			else
				DEBUG.LogError("Window function no exist >"+WindowFunctionName);
		}
		else
			DEBUG.LogError("Window no exist >"+WindowName);
	}
}
