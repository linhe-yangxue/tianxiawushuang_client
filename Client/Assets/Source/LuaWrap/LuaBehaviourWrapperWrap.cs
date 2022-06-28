using System;
using LuaInterface;

public class LuaBehaviourWrapperWrap
{
	public static LuaMethod[] regs = new LuaMethod[]
	{
		new LuaMethod("CreateGlobalBehaviour", CreateGlobalBehaviour),
		new LuaMethod("CreateGlobalTimer", CreateGlobalTimer),
		new LuaMethod("CreateGlobalBehaviourTable", CreateGlobalBehaviourTable),
		new LuaMethod("PushBlock", PushBlock),
		new LuaMethod("Activate", Activate),
		new LuaMethod("Destroy", Destroy),
		new LuaMethod("Set", Set),
		new LuaMethod("Get", Get),
		new LuaMethod("IsActive", IsActive),
		new LuaMethod("IsDead", IsDead),
		new LuaMethod("CreateBehaviour", CreateBehaviour),
		new LuaMethod("CreateTimer", CreateTimer),
		new LuaMethod("StartCoroutine", StartCoroutine),
		new LuaMethod("StopAllCoroutines", StopAllCoroutines),
		new LuaMethod("CreateBuff", CreateBuff),
		new LuaMethod("New", _CreateLuaBehaviourWrapper),
		new LuaMethod("GetClassType", GetClassType),
	};

	static LuaField[] fields = new LuaField[]
	{
	};

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateLuaBehaviourWrapper(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 1)
		{
			LuaBehaviour arg0 = LuaScriptMgr.GetNetObject<LuaBehaviour>(L, 1);
			LuaBehaviourWrapper obj = new LuaBehaviourWrapper(arg0);
			LuaScriptMgr.PushObject(L, obj);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: LuaBehaviourWrapper.New");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, typeof(LuaBehaviourWrapper));
		return 1;
	}

	public static void Register(IntPtr L)
	{
		LuaScriptMgr.RegisterLib(L, "LuaBehaviourWrapper", typeof(LuaBehaviourWrapper), regs, fields, typeof(System.Object));
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateGlobalBehaviour(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 0)
		{
			LuaBehaviourWrapper o = LuaBehaviourWrapper.CreateGlobalBehaviour();
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaInterface.LuaFunction)))
		{
			LuaFunction arg0 = LuaScriptMgr.GetLuaFunction(L, 1);
			LuaBehaviourWrapper o = LuaBehaviourWrapper.CreateGlobalBehaviour(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
		{
			string arg0 = LuaScriptMgr.GetString(L, 1);
			LuaBehaviourWrapper o = LuaBehaviourWrapper.CreateGlobalBehaviour(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: LuaBehaviourWrapper.CreateGlobalBehaviour");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateGlobalTimer(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 0)
		{
			LuaBehaviourWrapper o = LuaBehaviourWrapper.CreateGlobalTimer();
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaInterface.LuaFunction)))
		{
			LuaFunction arg0 = LuaScriptMgr.GetLuaFunction(L, 1);
			LuaBehaviourWrapper o = LuaBehaviourWrapper.CreateGlobalTimer(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
		{
			string arg0 = LuaScriptMgr.GetString(L, 1);
			LuaBehaviourWrapper o = LuaBehaviourWrapper.CreateGlobalTimer(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: LuaBehaviourWrapper.CreateGlobalTimer");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateGlobalBehaviourTable(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 1);
		LuaInterface.LuaTable o = LuaBehaviourWrapper.CreateGlobalBehaviourTable(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PushBlock(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		object arg0 = LuaScriptMgr.GetVarObject(L, 1);
		LuaBehaviourWrapper.PushBlock(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Activate(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
		obj.Activate();
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Destroy(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
		obj.Destroy();
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Set(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		object arg1 = LuaScriptMgr.GetVarObject(L, 3);
		obj.Set(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Get(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		object o = obj.Get(arg0);
		LuaScriptMgr.PushVarObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsActive(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
		bool o = obj.IsActive();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsDead(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
		bool o = obj.IsDead();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateBehaviour(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 1)
		{
			LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
			LuaBehaviourWrapper o = obj.CreateBehaviour();
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaBehaviourWrapper), typeof(LuaInterface.LuaFunction)))
		{
			LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
			LuaFunction arg0 = LuaScriptMgr.GetLuaFunction(L, 2);
			LuaBehaviourWrapper o = obj.CreateBehaviour(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaBehaviourWrapper), typeof(string)))
		{
			LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			LuaBehaviourWrapper o = obj.CreateBehaviour(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: LuaBehaviourWrapper.CreateBehaviour");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateTimer(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 1)
		{
			LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
			LuaBehaviourWrapper o = obj.CreateTimer();
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaBehaviourWrapper), typeof(LuaInterface.LuaFunction)))
		{
			LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
			LuaFunction arg0 = LuaScriptMgr.GetLuaFunction(L, 2);
			LuaBehaviourWrapper o = obj.CreateTimer(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaBehaviourWrapper), typeof(string)))
		{
			LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			LuaBehaviourWrapper o = obj.CreateTimer(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: LuaBehaviourWrapper.CreateTimer");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int StartCoroutine(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);
		LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
		LuaFunction arg0 = LuaScriptMgr.GetLuaFunction(L, 2);
		object[] objs1 = LuaScriptMgr.GetParamsObject(L, 3, count - 2);
		CoroutineInstruction o = obj.StartCoroutine(arg0,objs1);
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int StopAllCoroutines(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
		obj.StopAllCoroutines();
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateBuff(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		LuaBehaviourWrapper obj = LuaScriptMgr.GetNetObject<LuaBehaviourWrapper>(L, 1);
		BaseObjectWrapper arg0 = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 2);
		LuaBehaviourWrapper o = obj.CreateBuff(arg0);
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}
}

