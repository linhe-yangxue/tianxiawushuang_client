using System;
using UnityEngine;
using LuaInterface;

public class BaseObjectWrapperWrap
{
	public static LuaMethod[] regs = new LuaMethod[]
	{
		new LuaMethod("GetConfig", GetConfig),
		new LuaMethod("ChangeHP", ChangeHP),
		new LuaMethod("ChangeMP", ChangeMP),
		new LuaMethod("GetHP", GetHP),
		new LuaMethod("GetMP", GetMP),
		new LuaMethod("PlayAnim", PlayAnim),
		new LuaMethod("GetBase", GetBase),
		new LuaMethod("GetStatic", GetStatic),
		new LuaMethod("GetFinal", GetFinal),
		new LuaMethod("StartAffect", StartAffect),
		new LuaMethod("StopAffect", StopAffect),
		new LuaMethod("CreateBuff", CreateBuff),
		new LuaMethod("IsDead", IsDead),
		new LuaMethod("IsEnemy", IsEnemy),
		new LuaMethod("IsPet", IsPet),
		new LuaMethod("IsCharactor", IsCharactor),
		new LuaMethod("IsMonster", IsMonster),
		new LuaMethod("IsMonsterBoss", IsMonsterBoss),
		new LuaMethod("IsBigBoss", IsBigBoss),
		new LuaMethod("GetOwner", GetOwner),
		new LuaMethod("PlayEffect", PlayEffect),
		new LuaMethod("GetPosition", GetPosition),
		new LuaMethod("SetPosition", SetPosition),
		new LuaMethod("GetForward", GetForward),
		new LuaMethod("SetForward", SetForward),
		new LuaMethod("SetTarget", SetTarget),
		new LuaMethod("GetTarget", GetTarget),
		new LuaMethod("DoSkill", DoSkill),
		new LuaMethod("RemoveSkillCD", RemoveSkillCD),
		new LuaMethod("RemoveSummonCD", RemoveSummonCD),
		new LuaMethod("GetScriptBuff", GetScriptBuff),
		new LuaMethod("GetAllScriptBuffs", GetAllScriptBuffs),
		new LuaMethod("GetAllBuffIndices", GetAllBuffIndices),
		new LuaMethod("New", _CreateBaseObjectWrapper),
		new LuaMethod("GetClassType", GetClassType),
	};

	static LuaField[] fields = new LuaField[]
	{
	};

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateBaseObjectWrapper(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 1)
		{
			BaseObject arg0 = LuaScriptMgr.GetNetObject<BaseObject>(L, 1);
			BaseObjectWrapper obj = new BaseObjectWrapper(arg0);
			LuaScriptMgr.PushObject(L, obj);
			return 1;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: BaseObjectWrapper.New");
		}

		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, typeof(BaseObjectWrapper));
		return 1;
	}

	public static void Register(IntPtr L)
	{
		LuaScriptMgr.RegisterLib(L, "BaseObjectWrapper", typeof(BaseObjectWrapper), regs, fields, typeof(BaseWrapper));
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetConfig(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		Data o = obj.GetConfig(arg0);
		LuaScriptMgr.PushValue(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ChangeHP(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		double arg0 = (double)LuaScriptMgr.GetNumber(L, 2);
		obj.ChangeHP(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ChangeMP(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		double arg0 = (double)LuaScriptMgr.GetNumber(L, 2);
		obj.ChangeMP(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetHP(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		double o = obj.GetHP();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetMP(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		double o = obj.GetMP();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PlayAnim(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.PlayAnim(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetBase(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		double o = obj.GetBase(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetStatic(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		double o = obj.GetStatic(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetFinal(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		double o = obj.GetFinal(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int StartAffect(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		BaseObjectWrapper arg0 = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 2);
		int arg1 = (int)LuaScriptMgr.GetNumber(L, 3);
		LuaBehaviourWrapper o = obj.StartAffect(arg0,arg1);
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int StopAffect(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.StopAffect(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreateBuff(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		LuaBehaviourWrapper o = obj.CreateBuff();
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsDead(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		bool o = obj.IsDead();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsEnemy(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		BaseObjectWrapper arg0 = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 2);
		bool o = obj.IsEnemy(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsPet(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		bool o = obj.IsPet();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsCharactor(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		bool o = obj.IsCharactor();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsMonster(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		bool o = obj.IsMonster();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsMonsterBoss(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		bool o = obj.IsMonsterBoss();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsBigBoss(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		bool o = obj.IsBigBoss();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetOwner(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		BaseObjectWrapper o = obj.GetOwner();
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int PlayEffect(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		BaseEffectWrapper o = obj.PlayEffect(arg0);
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetPosition(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		Vector3 o = obj.GetPosition();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetPosition(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
		obj.SetPosition(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetForward(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		Vector3 o = obj.GetForward();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetForward(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
		obj.SetForward(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetTarget(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		BaseObjectWrapper arg0 = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 2);
		obj.SetTarget(arg0);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetTarget(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		BaseObjectWrapper o = obj.GetTarget();
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DoSkill(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		BaseObjectWrapper arg1 = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 3);
		obj.DoSkill(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveSkillCD(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		obj.RemoveSkillCD();
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveSummonCD(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		obj.RemoveSummonCD();
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetScriptBuff(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		LuaBehaviourWrapper o = obj.GetScriptBuff(arg0);
		LuaScriptMgr.PushObject(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetAllScriptBuffs(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		LuaInterface.LuaTable o = obj.GetAllScriptBuffs();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetAllBuffIndices(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BaseObjectWrapper obj = LuaScriptMgr.GetNetObject<BaseObjectWrapper>(L, 1);
		LuaInterface.LuaTable o = obj.GetAllBuffIndices();
		LuaScriptMgr.Push(L, o);
		return 1;
	}
}

