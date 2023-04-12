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

public class DebugCommand<T1, T2> : DebugCommandBase
{
    private Action<T1, T2> _action;

    public DebugCommand(string id, string description, string format, Action<T1, T2> action)
        : base(id, description, format)
    {
        _action = action;
    }

    public void Invoke(T1 v1, T2 v2)
    {
        _action.Invoke(v1, v2);
    }
}
