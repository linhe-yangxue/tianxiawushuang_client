using System;
using LuaInterface;

public class CoroutineInstructionWrap
{
	public static LuaMethod[] regs = new LuaMethod[]
	{
		new LuaMethod("Stop", Stop),
		new LuaMethod("IsDead", IsDead),
		new LuaMethod("New", _CreateCoroutineInstruction),
		new LuaMethod("GetClassType", GetClassType),
	};

	static LuaField[] fields = new LuaField[]
	{
		new LuaField("co", get_co, null),
		new LuaField("block", get_block, set_block),
	};

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateCoroutineInstruction(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 1)
		{
			LuaInterface.LuaThread arg0 = LuaScriptMgr.GetNetObject<LuaInterface.LuaThread>(L, 1);
			CoroutineInstruction obj = new CoroutineInstruction(arg0);
			LuaScriptMgr.PushObject(L, obj);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: CoroutineInstruction.New");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, typeof(CoroutineInstruction));
		return 1;
	}

	public static void Register(IntPtr L)
	{
		LuaScriptMgr.RegisterLib(L, "CoroutineInstruction", typeof(CoroutineInstruction), regs, fields, typeof(System.Object));
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_co(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CoroutineInstruction obj = (CoroutineInstruction)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name co");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index co on a nil value");
			}
		}

		LuaScriptMgr.PushObject(L, obj.co);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_block(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CoroutineInstruction obj = (CoroutineInstruction)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name block");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index block on a nil value");
			}
		}

		LuaScriptMgr.PushObject(L, obj.block);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_block(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CoroutineInstruction obj = (CoroutineInstruction)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name block");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index block on a nil value");
			}
		}

		obj.block = LuaScriptMgr.GetNetObject<ICoroutineBlock>(L, 3);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Stop(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		CoroutineInstruction obj = LuaScriptMgr.GetNetObject<CoroutineInstruction>(L, 1);
		obj.Stop();
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsDead(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		CoroutineInstruction obj = LuaScriptMgr.GetNetObject<CoroutineInstruction>(L, 1);
		bool o = obj.IsDead();
		LuaScriptMgr.Push(L, o);
		return 1;
	}
}

