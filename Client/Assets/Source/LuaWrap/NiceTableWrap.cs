using System;
using DataTable;
using System.Text;
using System.Collections.Generic;
using System.IO;
using LuaInterface;

public class NiceTableWrap
{
	public static LuaMethod[] regs = new LuaMethod[]
	{
		new LuaMethod("SetFileEncoding", SetFileEncoding),
		new LuaMethod("SetUnicodeEncoding", SetUnicodeEncoding),
		new LuaMethod("SetField", SetField),
		new LuaMethod("ResetField", ResetField),
		new LuaMethod("GetField", GetField),
		new LuaMethod("GetAllRecord", GetAllRecord),
		new LuaMethod("GetRecordCount", GetRecordCount),
		new LuaMethod("GetIndexCol", GetIndexCol),
		new LuaMethod("Records", Records),
		new LuaMethod("GetData", GetData),
		new LuaMethod("ToString", ToString),
		new LuaMethod("OnChanger", OnChanger),
		new LuaMethod("CreateRecord", CreateRecord),
		new LuaMethod("DeleteRecord", DeleteRecord),
		new LuaMethod("GetRecord", GetRecord),
		new LuaMethod("GetFirstRecord", GetFirstRecord),
		new LuaMethod("SaveTable", SaveTable),
		new LuaMethod("LoadTable", LoadTable),
		new LuaMethod("LoadTableOld", LoadTableOld),
		new LuaMethod("SaveBinary", SaveBinary),
		new LuaMethod("LoadBinary", LoadBinary),
		new LuaMethod("serialize", serialize),
		new LuaMethod("restore", restore),
		new LuaMethod("New", _CreateNiceTable),
		new LuaMethod("GetClassType", GetClassType),
		new LuaMethod("__tostring", Lua_ToString),
	};

	static LuaField[] fields = new LuaField[]
	{
		new LuaField("fileEncoding", get_fileEncoding, set_fileEncoding),
	};

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateNiceTable(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 0)
		{
			NiceTable obj = new NiceTable();
			LuaScriptMgr.PushObject(L, obj);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: NiceTable.New");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, typeof(NiceTable));
		return 1;
	}

	public static void Register(IntPtr L)
	{
		LuaScriptMgr.RegisterLib(L, "DataTable.NiceTable", typeof(NiceTable), regs, fields, typeof(System.Object));
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_fileEncoding(IntPtr L)
	{
		LuaScriptMgr.PushObject(L, NiceTable.fileEncoding);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_fileEncoding(IntPtr L)
	{
		NiceTable.fileEncoding = LuaScriptMgr.GetNetObject<Encoding>(L, 3);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Lua_ToString(IntPtr L)
	{
		object obj = LuaScriptMgr.GetLuaObject(L, 1);
		if (obj != null)
		{
			LuaScriptMgr.Push(L, obj.ToString());
		}
		else
		{
			LuaScriptMgr.Push(L, "Table: DataTable.NiceTable");
		}

		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetFileEncoding(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 1);
		bool o = NiceTable.SetFileEncoding(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetUnicodeEncoding(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 0);
		NiceTable.SetUnicodeEncoding();
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetField(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(string), typeof(string), typeof(int)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			string arg1 = LuaScriptMgr.GetString(L, 3);
			int arg2 = (int)LuaScriptMgr.GetNumber(L, 4);
			obj.SetField(arg0,arg1,arg2);
			return 0;
		}
		else if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(string), typeof(FIELD_TYPE), typeof(int)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			FIELD_TYPE arg1 = LuaScriptMgr.GetNetObject<FIELD_TYPE>(L, 3);
			int arg2 = (int)LuaScriptMgr.GetNumber(L, 4);
			obj.SetField(arg0,arg1,arg2);
			return 0;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: NiceTable.SetField");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ResetField(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		obj.ResetField();
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetField(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		DataTable.FieldIndex o = obj.GetField();
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetAllRecord(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		Dictionary<int,DataTable.DataRecord> o = obj.GetAllRecord();
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetRecordCount(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		int o = obj.GetRecordCount();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetIndexCol(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		int o = obj.GetIndexCol();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Records(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		IEnumerable<DataTable.DataRecord> o = obj.Records();
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetData(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(int), typeof(string)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			string arg1 = LuaScriptMgr.GetString(L, 3);
			Data o = obj.GetData(arg0,arg1);
			LuaScriptMgr.PushValue(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(string), typeof(string)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			string arg1 = LuaScriptMgr.GetString(L, 3);
			Data o = obj.GetData(arg0,arg1);
			LuaScriptMgr.PushValue(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: NiceTable.GetData");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ToString(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		string o = obj.ToString();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnChanger(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(DataTable.DataRecord), typeof(int), typeof(string)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			DataTable.DataRecord arg0 = LuaScriptMgr.GetNetObject<DataTable.DataRecord>(L, 2);
			int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
			string arg2 = LuaScriptMgr.GetString(L, 4);
			obj.OnChanger(arg0,arg1,arg2);
			return 0;
		}
		else if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(DataTable.DataRecord), typeof(int), typeof(int)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			DataTable.DataRecord arg0 = LuaScriptMgr.GetNetObject<DataTable.DataRecord>(L, 2);
			int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
			int arg2 = (int)LuaScriptMgr.GetNumber(L, 4);
			obj.OnChanger(arg0,arg1,arg2);
			return 0;
		}
		else if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(DataTable.DataRecord), typeof(int), typeof(object)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			DataTable.DataRecord arg0 = LuaScriptMgr.GetNetObject<DataTable.DataRecord>(L, 2);
			int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
			object arg2 = LuaScriptMgr.GetVarObject(L, 4);
			obj.OnChanger(arg0,arg1,arg2);
			return 0;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: NiceTable.OnChanger");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateRecord(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(string)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			DataTable.DataRecord o = obj.CreateRecord(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(int)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			DataTable.DataRecord o = obj.CreateRecord(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: NiceTable.CreateRecord");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DeleteRecord(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(string)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			bool o = obj.DeleteRecord(arg0);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(int)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			bool o = obj.DeleteRecord(arg0);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: NiceTable.DeleteRecord");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetRecord(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(string)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			DataTable.DataRecord o = obj.GetRecord(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(int)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
			DataTable.DataRecord o = obj.GetRecord(arg0);
			LuaScriptMgr.PushObject(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: NiceTable.GetRecord");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetFirstRecord(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		DataTable.DataRecord o = obj.GetFirstRecord();
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SaveTable(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 2)
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			bool o = obj.SaveTable(arg0);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(Stream), typeof(Encoding)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			Stream arg0 = LuaScriptMgr.GetNetObject<Stream>(L, 2);
			Encoding arg1 = LuaScriptMgr.GetNetObject<Encoding>(L, 3);
			bool o = obj.SaveTable(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(DataBuffer), typeof(Encoding)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			DataBuffer arg0 = LuaScriptMgr.GetNetObject<DataBuffer>(L, 2);
			Encoding arg1 = LuaScriptMgr.GetNetObject<Encoding>(L, 3);
			bool o = obj.SaveTable(ref arg0,arg1);
			LuaScriptMgr.Push(L, o);
			LuaScriptMgr.PushObject(L, arg0);
			return 2;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(string), typeof(Encoding)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			Encoding arg1 = LuaScriptMgr.GetNetObject<Encoding>(L, 3);
			bool o = obj.SaveTable(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: NiceTable.SaveTable");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadTable(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 2)
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			bool o = obj.LoadTable(arg0);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(DataBuffer), typeof(Encoding)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			DataBuffer arg0 = LuaScriptMgr.GetNetObject<DataBuffer>(L, 2);
			Encoding arg1 = LuaScriptMgr.GetNetObject<Encoding>(L, 3);
			bool o = obj.LoadTable(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(Stream), typeof(Encoding)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			Stream arg0 = LuaScriptMgr.GetNetObject<Stream>(L, 2);
			Encoding arg1 = LuaScriptMgr.GetNetObject<Encoding>(L, 3);
			bool o = obj.LoadTable(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(DataTable.NiceTable), typeof(string), typeof(Encoding)))
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			string arg0 = LuaScriptMgr.GetString(L, 2);
			Encoding arg1 = LuaScriptMgr.GetNetObject<Encoding>(L, 3);
			bool o = obj.LoadTable(arg0,arg1);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else if (count == 4)
		{
			NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
			string arg0 = LuaScriptMgr.GetLuaString(L, 2);
			Encoding arg1 = LuaScriptMgr.GetNetObject<Encoding>(L, 3);
			LOAD_MODE arg2 = LuaScriptMgr.GetNetObject<LOAD_MODE>(L, 4);
			bool o = obj.LoadTable(arg0,arg1,arg2);
			LuaScriptMgr.Push(L, o);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: NiceTable.LoadTable");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadTableOld(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		Stream arg0 = LuaScriptMgr.GetNetObject<Stream>(L, 2);
		Encoding arg1 = LuaScriptMgr.GetNetObject<Encoding>(L, 3);
		bool o = obj.LoadTableOld(arg0,arg1);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SaveBinary(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		bool o = obj.SaveBinary(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadBinary(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		bool o = obj.LoadBinary(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int serialize(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		DataBuffer arg0 = LuaScriptMgr.GetNetObject<DataBuffer>(L, 2);
		int o = obj.serialize(ref arg0);
		LuaScriptMgr.Push(L, o);
		LuaScriptMgr.PushObject(L, arg0);
		return 2;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int restore(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		NiceTable obj = LuaScriptMgr.GetNetObject<NiceTable>(L, 1);
		DataBuffer arg0 = LuaScriptMgr.GetNetObject<DataBuffer>(L, 2);
		bool o = obj.restore(ref arg0);
		LuaScriptMgr.Push(L, o);
		LuaScriptMgr.PushObject(L, arg0);
		return 2;
	}
}

