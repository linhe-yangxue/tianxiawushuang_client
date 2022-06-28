using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DataTable;
using LuaInterface;


public enum BehaviourState
{
    Inactive,
    Active,
    Dead
}


public class LuaBehaviour : LuaScript
{
    public static LuaTable ObjectsTable;
    private static bool Active = false;
    private static LuaFunction EmptyFunc;

    static LuaBehaviour()
    {
        EmptyFunc = Lua["EmptyFunc"] as LuaFunction;
        ObjectsTable = Lua["Objects"] as LuaTable;
        DoFile("Global");
    }

    public static void ClearObjects(bool active)
    {
        if (Active && ObjectsTable.Count > 0)
        {
            foreach (var key in ObjectsTable.Keys)
            {
                ObjectsTable[key] = null;
            }
        }

        Lua["character"] = null;
        Active = active;
    }

    public static void AddObject(BaseObject obj)
    {
        if (Active)
        {
            int id = obj.GetID();

            if (id >= 0)
            {
                ObjectsTable[id + 1] = obj.mWrapper;

                if (obj is Character)
                {
                    Lua["character"] = obj.mWrapper;
                }
            }
        }
    }

    public static void RemoveObject(BaseObject obj)
    {
        if (Active)
        {
            int id = obj.GetID();

            if (id >= 0)
            {
                ObjectsTable[id + 1] = null;

                if (obj is Character)
                {
                    Lua["character"] = null;
                }
            }
        }
    }

    public static void Update()
    {
        Lua["globalTime"] = (double)Time.time;
        Lua["globalDeltaTime"] = (double)Time.deltaTime;
    }

    private static int _coroutineDepth = 0;
    private static ICoroutineBlock _block = null;

    public static void PushBlock(ICoroutineBlock b)
    {
        if (_coroutineDepth > 0)
        {
            _block = b;
        }
    }

    public BehaviourState state { get; private set; }
    public LuaBehaviourWrapper wrapper { get; private set; }

    protected double _elapsedTime { get; private set; }
    protected double _deltaTime { get; private set; }

    private double _updateTime = 0;
    private bool _firstUpdate = true;
    private GameObject _helper;
    private LuaFunction _awake;
    private LuaFunction _start;
    private LuaFunction _update;
    private LuaFunction _onDestroy;
    private LuaBehaviour _parent;
    private List<LuaBehaviour> _children = new List<LuaBehaviour>();
    private List<CoroutineInstruction> coroutines = new List<CoroutineInstruction>();
    

    public LuaBehaviour()
        : base()
    {
        state = BehaviourState.Inactive;
        wrapper = new LuaBehaviourWrapper(this);
        Set("deltaTime", 1d);
        Set("elapsedTime", 0d);
        Set("this", wrapper);
    }

    public LuaBehaviour(LuaFunction func)
        : base(func)
    {
        state = BehaviourState.Inactive;
        wrapper = new LuaBehaviourWrapper(this);
        Set("elapsedTime", 0d);
        Set("this", wrapper);
    }

    public LuaBehaviour(string script)
        : base(script)
    {
        state = BehaviourState.Inactive;
        wrapper = new LuaBehaviourWrapper(this);
        Set("elapsedTime", 0d);
        Set("this", wrapper);
    }

    public LuaBehaviour(DataRecord record)
        : base(record)
    {
        state = BehaviourState.Inactive;
        wrapper = new LuaBehaviourWrapper(this);
        Set("elapsedTime", 0d);
        Set("this", wrapper);
    }

    public void Activate()
    {
        ActivateSelf();

        foreach (var child in _children)
        {
            if (child != null && child.state == BehaviourState.Inactive)
            {
                child.Activate();
            }
        }
    }

    private void ActivateSelf()
    {
        if (state == BehaviourState.Inactive && (_parent == null || _parent.state == BehaviourState.Active))
        {
            state = BehaviourState.Active;
            OnActivate();
            _helper = new GameObject("lua_behaviour");
            _helper.transform.parent = _parent == null ? null : _parent._helper.transform;
            _helper.AddComponent<LuaBehaviourHelper>().behaviour = this;
            _OnAwake(Invoke(_awake));
        }
    }

    protected virtual void OnActivate()
    {
        _deltaTime = GetNumber("deltaTime");
        _awake = GetFunction("Awake");
        _start = GetFunction("Start");
        _update = GetFunction("Update");
        _onDestroy = GetFunction("OnDestroy");
    }

    public void Start()
    {
        _OnStart(Invoke(_start));
    }

    public virtual void Update(float deltaTime)
    {
        if (state != BehaviourState.Active)
            return;

        if (!_firstUpdate)
        {
            _updateTime -= deltaTime;
            _elapsedTime += deltaTime;
        }

        _firstUpdate = false;
        Set("elapsedTime", _elapsedTime);       

        if (_updateTime <= 0)
        {
            _updateTime += _deltaTime;            
            _OnUpdate(Invoke(_update));
        }

        UpdateCoroutines();
    }

    public void Destroy()
    {
        if (state == BehaviourState.Active && this._helper != null)
        {
            GameObject.Destroy(_helper);
        }
    }

    public bool AttachBehaviour(LuaBehaviour b)
    {
        if (state != BehaviourState.Dead && b.state == BehaviourState.Inactive && b._parent == null)
        {
            b._parent = this;
            _children.Add(b);
            b.Set("parent", this.wrapper);
            return true;
        }

        return false;
    }

    public void OnDestroy()
    {
        if (state == BehaviourState.Active)
        {
            state = BehaviourState.Dead;
            StopAllCoroutines();
            UpdateCoroutines();
            _OnDestroy(Invoke(_onDestroy));

            if (_parent != null)
            {
                _parent._children.Remove(this);
            }        
        }
   
        Dispose();
    }

    public CoroutineInstruction StartCoroutine(LuaFunction func, params object[] args)
    {
        CoroutineInstruction inst = _StartCoroutine(func, args);
        _StartCoroutine(EmptyFunc);
        return inst;
    }

    private CoroutineInstruction _StartCoroutine(LuaFunction func, params object[] args)
    {
        if (state != BehaviourState.Active)
            return null;

        LuaThread c = new LuaThread(Lua, func);
        c.Start();

        CoroutineInstruction inst = new CoroutineInstruction(c);
        coroutines.Add(inst);

        inst.block = ResumeCoroutine(c, args);
        return inst;
    }

    public void StopAllCoroutines()
    {
        foreach (var inst in coroutines)
        {
            inst.Stop();
        }
    }

    protected virtual void _OnAwake(object[] param) { }

    protected virtual void _OnStart(object[] param) { }

    protected virtual void _OnUpdate(object[] param) { }

    protected virtual void _OnDestroy(object[] param) { }

    private void UpdateCoroutines()
    {
        for (int i = 0; i < coroutines.Count; ++i)
        {         
            CoroutineInstruction inst = coroutines[i];         

            if (inst.block == null)
            {
                inst.Stop();
            }

            if (inst.IsDead())
            {
                coroutines.RemoveAt(i);
                --i;
            }
        }

        for (int i = 0; i < coroutines.Count; ++i)
        {
            CoroutineInstruction inst = coroutines[i];

            if (!inst.IsDead() && inst.block != null && !inst.block.onBlocking)
            {
                inst.block = ResumeCoroutine(inst.co);
            }
        }
    }

    private ICoroutineBlock ResumeCoroutine(LuaThread co, params object[] args)
    {
        ICoroutineBlock returnBlock = null;
        ++_coroutineDepth;

        if (co.Resume(args, null) != 1)
        {
            --_coroutineDepth;
            _block = null;
            return null;
        }
        else 
        {
            returnBlock = _block;
            --_coroutineDepth;
            _block = null;
            return returnBlock;
        }
    }
}


public class CoroutineInstruction
{ 
    private bool isDead = false;

    public LuaThread co { get; private set; } 
    public ICoroutineBlock block { get; set; }

    public CoroutineInstruction(LuaThread c)
    {
        co = c;
        block = null;
        isDead = false;
    }

    public void Stop()
    {
        isDead = true;
    }

    public bool IsDead()
    {
        return isDead;
    }
}


public class LuaTimer : LuaBehaviour
{
    protected double _totalTime { get; private set; }
    private LuaFunction _onTimeOut;

    public LuaTimer()
        : base()
    { }

    public LuaTimer(LuaFunction func)
        : base(func)
    { }

    public LuaTimer(string script)
        : base(script)
    { }

    public LuaTimer(DataRecord record)
        : base(record)
    { }

    protected override void OnActivate()
    {
        base.OnActivate();

        _totalTime = GetNumber("totalTime");
        _onTimeOut = GetFunction("OnTimeOut");
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (_totalTime > 0 && _elapsedTime >= _totalTime)
        {
            Invoke(_onTimeOut);
            Destroy();
        }
    }
}


public interface ICoroutineBlock
{
    bool onBlocking { get; }
}

public class WaitForSecondsBlock : ICoroutineBlock
{
    private float _dest = 0f;

    public WaitForSecondsBlock(float seconds)
    {
        _dest = Time.time + seconds;
    }

    public bool onBlocking
    {
        get { return Time.time <= _dest; }
    }
}

public class WaitForBehaviourBlock : ICoroutineBlock
{
    private LuaBehaviour _b = null;

    public WaitForBehaviourBlock(LuaBehaviour b)
    {
        _b = b;
    }

    public bool onBlocking
    {
        get { return _b.state == BehaviourState.Active; }
    }
}

public class WaitForCoroutineBlock : ICoroutineBlock
{
    private CoroutineInstruction _inst;

    public WaitForCoroutineBlock(CoroutineInstruction inst)
    {
        _inst = inst;
    }

    public bool onBlocking
    {
        get { return !_inst.IsDead(); }
    }
}


public class LuaBehaviourWrapper
{
    private LuaBehaviour behaviour;

    public LuaBehaviourWrapper(LuaBehaviour behaviour)
    {
        this.behaviour = behaviour;
    }

    public static LuaBehaviourWrapper CreateGlobalBehaviour()
    {
        LuaBehaviour b = new LuaBehaviour();
        b.Init();
        return b.wrapper;
    }

    public static LuaBehaviourWrapper CreateGlobalBehaviour(string script)
    {
        LuaBehaviour b = new LuaBehaviour(script);
        b.Init();
        return b.wrapper;
    }

    public static LuaBehaviourWrapper CreateGlobalBehaviour(LuaFunction func)
    {
        LuaBehaviour b = new LuaBehaviour(func);
        b.Init();
        return b.wrapper;
    }

    public static LuaBehaviourWrapper CreateGlobalTimer()
    {
        LuaBehaviour b = new LuaTimer();
        b.Init();
        return b.wrapper;
    }

    public static LuaBehaviourWrapper CreateGlobalTimer(string script)
    {
        LuaBehaviour b = new LuaTimer(script);
        b.Init();
        return b.wrapper;
    }

    public static LuaBehaviourWrapper CreateGlobalTimer(LuaFunction func)
    {
        LuaBehaviour b = new LuaTimer(func);
        b.Init();
        return b.wrapper;
    }

    public static LuaTable CreateGlobalBehaviourTable(string script)
    {
        return LuaInstance.DoFile(script)[0] as LuaTable;
    }

    public static void PushBlock(object o)
    {
        if (o is double)
        {
            LuaBehaviour.PushBlock(new WaitForSecondsBlock((float)(double)o));
        }
        else if (o is LuaBehaviourWrapper)
        {
            LuaBehaviour b = (o as LuaBehaviourWrapper).behaviour;

            if (b.state == BehaviourState.Inactive)
                b.Activate();

            LuaBehaviour.PushBlock(new WaitForBehaviourBlock(b));
        }
        else if(o is CoroutineInstruction)
        {
            LuaBehaviour.PushBlock(new WaitForCoroutineBlock((CoroutineInstruction)o));
        }
        else 
        {
            LuaBehaviour.PushBlock(new WaitForSecondsBlock(0));
        }
    }

    public void Activate()
    {
        behaviour.Activate();
    }

    public void Destroy()
    {
        behaviour.Destroy();
    }

    public void Set(string key, object val)
    {
        behaviour.Set(key, val);
    }

    public object Get(string key)
    {
        return behaviour.GetObject(key);
    }

    public bool IsActive()
    {
        return behaviour.state == BehaviourState.Active;
    }

    public bool IsDead()
    {
        return behaviour.state == BehaviourState.Dead;
    }

    public LuaBehaviourWrapper CreateBehaviour()
    {
        if (behaviour.state != BehaviourState.Dead)
        {
            LuaBehaviour b = new LuaBehaviour();
            b.Init();
            behaviour.AttachBehaviour(b);
            return b.wrapper;
        }

        return null;
    }

    public LuaBehaviourWrapper CreateBehaviour(string script)
    {
        if (behaviour.state != BehaviourState.Dead && LuaInstance.CheckScriptExist(script))
        {
            LuaBehaviour b = new LuaBehaviour(script);
            b.Init();
            behaviour.AttachBehaviour(b);
            return b.wrapper;
        }

        return null;
    }

    public LuaBehaviourWrapper CreateBehaviour(LuaFunction func)
    {
        if (behaviour.state != BehaviourState.Dead)
        {
            LuaBehaviour b = new LuaBehaviour(func);
            b.Init();
            behaviour.AttachBehaviour(b);
            return b.wrapper;
        }

        return null;
    }

    public LuaBehaviourWrapper CreateTimer()
    {
        if (behaviour.state != BehaviourState.Dead)
        {
            LuaBehaviour b = new LuaTimer();
            b.Init();
            behaviour.AttachBehaviour(b);
            return b.wrapper;
        }

        return null;
    }

    public LuaBehaviourWrapper CreateTimer(string script)
    {
        if (behaviour.state != BehaviourState.Dead && LuaInstance.CheckScriptExist(script))
        {
            LuaBehaviour b = new LuaTimer(script);
            b.Init();
            behaviour.AttachBehaviour(b);
            return b.wrapper;
        }

        return null;
    }

    public LuaBehaviourWrapper CreateTimer(LuaFunction func)
    {
        if (behaviour.state != BehaviourState.Dead)
        {
            LuaBehaviour b = new LuaTimer(func);
            b.Init();
            behaviour.AttachBehaviour(b);
            return b.wrapper;
        }

        return null;
    }

    public CoroutineInstruction StartCoroutine(LuaFunction func, params object[] args)
    {
        return behaviour.StartCoroutine(func, args);
    }

    public void StopAllCoroutines()
    {
        behaviour.StopAllCoroutines();
    }

    public LuaBehaviourWrapper CreateBuff(BaseObjectWrapper o)
    {
        LuaBehaviourWrapper b = o.CreateBuff();
        this.behaviour.AttachBehaviour(b.behaviour);
        return b;
    }
}