using UnityEngine;
using System.Collections.Generic;


public interface IStatus
{
    void Apply();
}


public abstract class StateSetting<T> where T : IStatus
{
    private List<T> _states = new List<T>();

    public abstract T defaultState { get; }

    public T currentState
    {
        get { return _states.Count > 0 ? _states[_states.Count - 1] : defaultState; }
    }

    public void Apply(T state)
    {
        _states.Add(state);
        state.Apply();
    }

    public void Revert()
    {
        if (_states.Count > 0)
        {
            _states.RemoveAt(_states.Count - 1);
            defaultState.Apply();

            for (int i = 0; i < _states.Count; ++i)
                _states[i].Apply();
        }
    }

    public void Reset()
    {
        _states.Clear();
        defaultState.Apply();
    }
}