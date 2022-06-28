using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;

public class MessageTipsWindow : tWindow 
{
	public override void Open(object param)
	{
		base.Open(param);
		GameCommon.SetUIText(mGameObjUI, "tips_label", param.ToString());
		DoCoroutine(FadeOut());
	}
	
	private IEnumerator FadeOut()
	{
		float t = 0f;
		UIPanel panel = mGameObjUI.GetComponent<UIPanel>();
		panel.alpha = 1f;
		yield return new WaitForSeconds(1f);
		
		while (t < 0.5f)
		{
			panel.alpha = 1f - t * 2;
			t += Time.deltaTime;
			yield return null;
		}
		panel.alpha = 0f;
		Close();
		panel.alpha = 1f;
	}
	
}
