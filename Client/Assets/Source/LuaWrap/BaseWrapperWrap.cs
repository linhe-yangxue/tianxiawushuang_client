using System;
using UnityEngine;
using LuaInterface;

public class BaseWrapperWrap
{
	public static LuaMethod[] regs = new LuaMethod[]
	{
		new LuaMethod("GetPosition", GetPosition),
		new LuaMethod("SetPosition", SetPosition),
		new LuaMethod("GetForward", GetForward),
		new LuaMethod("SetForward", SetForward),
		new LuaMethod("InSphere", InSphere),
		new LuaMethod("InRange", InRange),
		new LuaMethod("Translate", Translate),
		new LuaMethod("RotateAround", RotateAround),
		new LuaMethod("Rotate", Rotate),
		new LuaMethod("New", _CreateBaseWrapper),
		new LuaMethod("GetClassType", GetClassType),
	};

	static LuaField[] fields = new LuaField[]
	{
	};

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateBaseWrapper(IntPtr L)
	{
		LuaDLL.luaL_error(L, "BaseWrapper class does not have a constructor function");
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, typeof(BaseWrapper));
		return 1;
	}

	public static void Register(IntPtr L)
	{
		LuaScriptMgr.RegisterLib(L, "BaseWrapper", typeof(BaseWrapper), regs, fields, typeof(System.Object));
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetPosition(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseWrapper obj = LuaScriptMgr.GetNetObject<BaseWrapper>(L, 1);
		Vector3 o = obj.GetPosition();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetPosition(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseWrapper obj = LuaScriptMgr.GetNetObject<BaseWrapper>(L, 1);
		Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
		obj.SetPosition(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetForward(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseWrapper obj = LuaScriptMgr.GetNetObject<BaseWrapper>(L, 1);
		Vector3 o = obj.GetForward();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetForward(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseWrapper obj = LuaScriptMgr.GetNetObject<BaseWrapper>(L, 1);
		Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
		obj.SetForward(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int InSphere(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		BaseWrapper obj = LuaScriptMgr.GetNetObject<BaseWrapper>(L, 1);
		BaseWrapper arg0 = LuaScriptMgr.GetNetObject<BaseWrapper>(L, 2);
		double arg1 = (double)LuaScriptMgr.GetNumber(L, 3);
		bool o = obj.InSphere(arg0,arg1);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int InRange(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		BaseWrapper obj = LuaScriptMgr.GetNetObject<BaseWrapper>(L, 1);
		BaseWrapper arg0 = LuaScriptMgr.GetNetObject<BaseWrapper>(L, 2);
		double arg1 = (double)LuaScriptMgr.GetNumber(L, 3);
		bool o = obj.InRange(arg0,arg1);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Translate(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseWrapper obj = LuaScriptMgr.GetNetObject<BaseWrapper>(L, 1);
		Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
		obj.Translate(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RotateAround(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		BaseWrapper obj = LuaScriptMgr.GetNetObject<BaseWrapper>(L, 1);
		Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
		double arg1 = (double)LuaScriptMgr.GetNumber(L, 3);
		obj.RotateAround(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Rotate(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseWrapper obj = LuaScriptMgr.GetNetObject<BaseWrapper>(L, 1);
		double arg0 = (double)LuaScriptMgr.GetNumber(L, 2);
		obj.Rotate(arg0);
		return 0;
	}
}

