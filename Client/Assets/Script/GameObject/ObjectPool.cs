using UnityEngine;
using System.Collections.Generic;


public interface IObjectPool<T>
{
    int MaxBufferCount { get; set; }
    int ValidCount { get; }
    int EnabledCount { get; }
    void Preload(int count);
    void Clear();
    T New(object initData);
    bool Disable(T obj);
    void DisableAll();
}


// Object has 3 status: enabled, disabled, released
public interface IObjectHandler<T>
{
    // New Object must be disabled
    T New();

    // disabled or enabled => enabled
    void Enable(T obj, object initData);

    // disabled or enabled => released
    void Destroy(T obj);

    // disabled or enabled => disabled
    void Disable(T obj);

    // if disabled or enabled then return true, else return false
    bool CheckValid(T obj);

    // if enabled then return true, else return false
    bool CheckEnabled(T obj);
}


public class SimpleObjectPool<T> : IObjectPool<T>
{
    private IObjectHandler<T> _handler;
    private int _maxBufferCount = 0;
    private List<T> _buffer = new List<T>();
    private List<T> _objs = new List<T>();

    public SimpleObjectPool(IObjectHandler<T> handler)
    {
        _handler = handler;
    }

    public int MaxBufferCount
    {
        get
        {
            return _maxBufferCount;
        }

        set
        {
            _maxBufferCount = value;

            if (_maxBufferCount > 0 && _buffer.Count > _maxBufferCount)
            {
                for (int i = _maxBufferCount; i < _buffer.Count; ++i)
                {
                    if (_handler.CheckValid(_buffer[i]))
                    {
                        _handler.Destroy(_buffer[i]);
                    }
                }

                _buffer.RemoveRange(_maxBufferCount, _buffer.Count - _maxBufferCount);
            }
        }
    }

    public int ValidCount
    {
        get { return _buffer.Count + _objs.Count; }
    }

    public int EnabledCount
    {
        get { return _objs.Count; }
    }

    public void Preload(int count)
    {
        if (_maxBufferCount > 0 && _buffer.Count + count > _maxBufferCount)
        {
            count = _maxBufferCount - _buffer.Count;
        }

        for (int i = 0; i < count; ++i)
        {
            T obj = _handler.New();
            _buffer.Add(obj);
        }
    }

    public void Clear()
    {
        foreach (var obj in _objs)
        {
            if (_handler.CheckValid(obj))
            {
                _handler.Destroy(obj);
            }
        }

        _objs.Clear();

        foreach (var obj in _buffer)
        {
            if (_handler.CheckValid(obj))
            {
                _handler.Destroy(obj);
            }
        }

        _buffer.Clear();
    }

    public T New(object initData)
    {
        T obj;

        if (_buffer.Count > 0)
        {
            obj = _buffer[_buffer.Count - 1];
            _buffer.RemoveAt(_buffer.Count - 1);

            if (!_handler.CheckValid(obj))
            {
                obj = _handler.New();
            }
        }
        else
        {
            obj = _handler.New();
        }

        _handler.Enable(obj, initData);
        _objs.Add(obj);
        return obj;
    }

    public bool Disable(T obj)
    {
        if (_objs.Remove(obj) && _handler.CheckValid(obj))
        {
            if (_maxBufferCount == 0 || _buffer.Count < _maxBufferCount)
            {
                _handler.Disable(obj);
                _buffer.Add(obj);
            }
            else
            {
                _handler.Destroy(obj);
            }

            return true;
        }

        return false;
    }

    public void DisableAll()
    {
        foreach (var obj in _objs)
        {
            if (_handler.CheckValid(obj))
            {
                if (_maxBufferCount == 0 || _buffer.Count < _maxBufferCount)
                {
                    _handler.Disable(obj);
                    _buffer.Add(obj);
                }
                else
                {
                    _handler.Destroy(obj);
                }
            }
        }

        _objs.Clear();
    }
}