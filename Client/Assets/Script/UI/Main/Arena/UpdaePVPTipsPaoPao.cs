using UnityEngine;
using System.Collections;

public class UpdaePVPTipsPaoPao : MonoBehaviour 
{
	ArenaMainWindow _ArenaMainWindow = DataCenter.GetData("ARENA_MAIN_WINDOW") as ArenaMainWindow;
	float _iIntervalTime = 0f;
	float iTime = 0f;
	// Use this for initialization
	void Start () 
	{
		_iIntervalTime = _ArenaMainWindow.iIntervalTime;
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		iTime = iTime+ Time.deltaTime;
		if(iTime >= _iIntervalTime)
		{
			_ArenaMainWindow.ShowPVPPaoPaoTips();
			iTime = 0f;
		}
	}
}
