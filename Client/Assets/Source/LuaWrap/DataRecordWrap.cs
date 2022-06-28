using System;
using DataTable;
using LuaInterface;

public class DataRecordWrap
{
	public static LuaMethod[] regs = new LuaMethod[]
	{
		new LuaMethod("get_Item", get_Item),
		new LuaMethod("set_Item", set_Item),
		new LuaMethod("GetFieldIndex", GetFieldIndex),
		new LuaMethod("SetFieldIndex", SetFieldIndex),
		new LuaMethod("GetDataList", GetDataList),
		new LuaMethod("restore", restore),
		new LuaMethod("getData", getData),
		new LuaMethod("_set", _set),
		new LuaMethod("set", set),
		new LuaMethod("get", get),
		new LuaMethod("getObject", getObject),
		new LuaMethod("getIndex", getIndex),
		new LuaMethod("New", _CreateDataRecord),
		new LuaMethod("GetClassType", GetClassType),
	};

	static LuaField[] fields = new LuaField[]
	{
	};

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateDataRecord(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 0)
		{
			DataRecord obj = new DataRecord();
			LuaScriptMgr.PushObject(L, obj);
			return 1;
		}
		else if (count == 1)
		{
			DataTable.FieldIndex arg0 = LuaScriptMgr.GetNetObject<DataTable.FieldIndex>(L, 1);
			DataRecord obj = new DataRecord(arg0);
			LuaScriptMgr.PushObject(L, obj);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: DataRecord.New");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, typeof(DataRecord));
		return 1;
	}

	public static void Register(IntPtr L)
	{
		LuaScriptMgr.RegisterLib(L, "DataTable.DataRecord", typeof(DataRecord), regs, fields, typeof(System.Object));
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_Item(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		Data o = obj[arg0];
		LuaScriptMgr.PushValue(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_Item(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		Data arg1 = LuaScriptMgr.GetNetObject<Data>(L, 3);
		obj[arg0] = arg1;
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetFieldIndex(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
		DataTable.FieldIndex o = obj.GetFieldIndex();
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetFieldIndex(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
		DataTable.FieldIndex arg0 = LuaScriptMgr.GetNetObject<DataTable.FieldIndex>(L, 2);
		bool o = obj.SetFieldIndex(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetDataList(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
		object[] o = obj.GetDataList();
		LuaScriptMgr.PushArray(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int restore(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
		DataBuffer arg0 = LuaScriptMgr.GetNetObject<DataBuffer>(L, 2);
		bool o = obj.restore(ref arg0);
		LuaScriptMgr.Push(L, o);
		LuaScriptMgr.PushObject(L, arg0);
		return 2;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int getData(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			Data o = obj.getData(arg0);
			LuaScriptMgr.PushValue(L, o);
			return 1;
		}
		else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(string)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			Data o = obj.getData(arg0);
			LuaScriptMgr.PushValue(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: DataRecord.getData");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _set(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), typeof(string)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			string arg1 = LuaScriptMgr.GetString(L, 3);
			bool o = obj._set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), typeof(float)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
			bool o = obj._set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), typeof(int)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
			bool o = obj._set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), typeof(object)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			object arg1 = LuaScriptMgr.GetVarObject(L, 3);
			bool o = obj._set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: DataRecord._set");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), typeof(ulong)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			ulong arg1 = (ulong)LuaScriptMgr.GetNumber(L, 3);
			bool o = obj.set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(string), typeof(string)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			string arg1 = LuaScriptMgr.GetString(L, 3);
			bool o = obj.set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(string), typeof(ulong)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			ulong arg1 = (ulong)LuaScriptMgr.GetNumber(L, 3);
			bool o = obj.set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), typeof(float)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
			bool o = obj.set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), typeof(int)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
			bool o = obj.set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), typeof(string)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			string arg1 = LuaScriptMgr.GetString(L, 3);
			bool o = obj.set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(string), typeof(object)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			object arg1 = LuaScriptMgr.GetVarObject(L, 3);
			bool o = obj.set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), typeof(object)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			object arg1 = LuaScriptMgr.GetVarObject(L, 3);
			bool o = obj.set(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: DataRecord.set");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			object o = obj.get(arg0);
			LuaScriptMgr.PushVarObject(L, o);
			return 1;
		}
		else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(string)))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			Data o = obj.get(arg0);
			LuaScriptMgr.PushValue(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(string), null))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			string arg1 = null;
			bool o = obj.get(arg0,out arg1);
			LuaScriptMgr.Push(L, o);
			LuaScriptMgr.Push(L, arg1);
			return 2;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(string), null))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			float arg1;
			bool o = obj.get(arg0,out arg1);
			LuaScriptMgr.Push(L, o);
			LuaScriptMgr.Push(L, arg1);
			return 2;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), null))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			int arg1;
			bool o = obj.get(arg0,out arg1);
			LuaScriptMgr.Push(L, o);
			LuaScriptMgr.Push(L, arg1);
			return 2;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(string), null))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			ulong arg1;
			bool o = obj.get(arg0,out arg1);
			LuaScriptMgr.Push(L, o);
			LuaScriptMgr.Push(L, arg1);
			return 2;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), null))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			ulong arg1;
			bool o = obj.get(arg0,out arg1);
			LuaScriptMgr.Push(L, o);
			LuaScriptMgr.Push(L, arg1);
			return 2;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), null))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			string arg1 = null;
			bool o = obj.get(arg0,out arg1);
			LuaScriptMgr.Push(L, o);
			LuaScriptMgr.Push(L, arg1);
			return 2;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(string), null))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			int arg1;
			bool o = obj.get(arg0,out arg1);
			LuaScriptMgr.Push(L, o);
			LuaScriptMgr.Push(L, arg1);
			return 2;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.DataRecord), typeof(int), null))
		{
			DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			float arg1;
			bool o = obj.get(arg0,out arg1);
			LuaScriptMgr.Push(L, o);
			LuaScriptMgr.Push(L, arg1);
			return 2;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: DataRecord.get");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int getObject(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		object o = obj.getObject(arg0);
		LuaScriptMgr.PushVarObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int getIndex(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		DataRecord obj = LuaScriptMgr.GetNetObject<DataRecord>(L, 1);
		int o = obj.getIndex();
		LuaScriptMgr.Push(L, o);
		return 1;
	}
}

