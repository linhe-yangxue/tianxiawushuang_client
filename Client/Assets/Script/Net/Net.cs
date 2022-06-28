using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logic;
using DataTable;

public class Net
{
    static public EventCenter gNetEventCenter = null;
	static public TM_WaitNetEvent msUIWaitEffect = null;

    //static public string msServerIP = "";
    //static public int msServerPort = 0;

    static public BaseNetEvent mLastSendEvent = null;

    static public void Register(string netEventName, tEventFactory factory)
    {
        gNetEventCenter.RegisterEvent(netEventName, factory);
    }

    static public void StartWaitEffect()
    {
        if (msUIWaitEffect == null)
		{
			msUIWaitEffect = (TM_WaitNetEvent)EventCenter.Start("TM_WaitNetEvent");
            msUIWaitEffect.SetFinished(true);
		}

		msUIWaitEffect.StartWatch();
	
        // GameCommon.ShowDebugInfo(0.1f, 0.3f, "Wait net ...");
    }

    static public void StopWaitEffect()
    {
        if (msUIWaitEffect != null)
		{
            msUIWaitEffect.FinishWatch();
		}

        // GameCommon.ShowDebugInfo(0.1f, 0.3f, "");
    }
    //by chenliang
    //begin

    private static TM_WaitNetEvent msUIRechargeWaitEffect = null;
    public static void StartWaitEffectRecharge()
    {
        if (msUIRechargeWaitEffect == null)
        {
            msUIRechargeWaitEffect = (TM_WaitNetEvent)EventCenter.Start("TM_WaitNetEvent");
            msUIRechargeWaitEffect.SetFinished(true);
        }

        msUIRechargeWaitEffect.StartWatch();
    }

    public static void StopWaitEffectRecharge()
    {
        if (msUIRechargeWaitEffect != null)
            msUIRechargeWaitEffect.FinishWatch();
    }

    //end

    // Use this for initialization
	static public void Init()
	{
		gNetEventCenter = new EventCenter();
		gNetEventCenter.InitSetTimeManager(new GameTimeManager());
        gNetEventCenter.SetLog(EventCenter.Self.GetLog());

		NetEvent.RegisterNetEvent();
        ResourcesUpdate.RegisterNetEvent();
	}

    static public void StartConnect(string strIP, int port, bool bDNS, tEvent connectFinishEvt, string closeEvent, string connectFailEvent, int tryCount, float overTime)
    {
        gNetEventCenter.InitNet(strIP, port, bDNS, connectFinishEvt, closeEvent, connectFailEvent, tryCount, overTime);
    }

    static public void Stop()
    {
        if (gNetEventCenter!=null && gNetEventCenter.mNetTool!=null)
            gNetEventCenter.mNetTool.Close(false);
    }

    static public tEvent StartEvent(string netEvent)
    {
        return gNetEventCenter.StartEvent(netEvent);
    }
}
