using System;
using UnityEngine;
using LuaInterface;

public class BaseEffectWrapperWrap
{
	public static LuaMethod[] regs = new LuaMethod[]
	{
		new LuaMethod("Finish", Finish),
		new LuaMethod("GetPosition", GetPosition),
		new LuaMethod("SetPosition", SetPosition),
		new LuaMethod("GetForward", GetForward),
		new LuaMethod("SetForward", SetForward),
		new LuaMethod("New", _CreateBaseEffectWrapper),
		new LuaMethod("GetClassType", GetClassType),
	};

	static LuaField[] fields = new LuaField[]
	{
	};

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateBaseEffectWrapper(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 1)
		{
			BaseEffect arg0 = LuaScriptMgr.GetNetObject<BaseEffect>(L, 1);
			BaseEffectWrapper obj = new BaseEffectWrapper(arg0);
			LuaScriptMgr.PushObject(L, obj);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: BaseEffectWrapper.New");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, typeof(BaseEffectWrapper));
		return 1;
	}

	public static void Register(IntPtr L)
	{
		LuaScriptMgr.RegisterLib(L, "BaseEffectWrapper", typeof(BaseEffectWrapper), regs, fields, typeof(BaseWrapper));
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Finish(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseEffectWrapper obj = LuaScriptMgr.GetNetObject<BaseEffectWrapper>(L, 1);
		obj.Finish();
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetPosition(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseEffectWrapper obj = LuaScriptMgr.GetNetObject<BaseEffectWrapper>(L, 1);
		Vector3 o = obj.GetPosition();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetPosition(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseEffectWrapper obj = LuaScriptMgr.GetNetObject<BaseEffectWrapper>(L, 1);
		Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
		obj.SetPosition(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetForward(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseEffectWrapper obj = LuaScriptMgr.GetNetObject<BaseEffectWrapper>(L, 1);
		Vector3 o = obj.GetForward();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetForward(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseEffectWrapper obj = LuaScriptMgr.GetNetObject<BaseEffectWrapper>(L, 1);
		Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
		obj.SetForward(arg0);
		return 0;
	}
}

