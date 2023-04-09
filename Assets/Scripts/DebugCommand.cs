using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommand : DebugCommandBase
{
    private Action _action;

    public DebugCommand(string id, string description, string format, Action action)
        : base(id, description, format)
    {
        _action = action;
    }
    
    public void Invoke()
    {
        _action.Invoke();
    }
}

public class DebugCommand<T> : DebugCommandBase
{
    private Action<T> _action;

    public DebugCommand(string id, string description, string format, Action<T> action)
        : base(id, description, format)
    {
        _action = action;
    }

    public void Invoke(T value)
    {
        _action.Invoke(value);
        Debug.Log(Id);
    }
}
