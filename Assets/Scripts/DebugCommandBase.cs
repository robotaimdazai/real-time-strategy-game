using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommandBase
{
    private string _id;
    private string _description;
    private string _format;
    
    public static Dictionary<string, DebugCommandBase> DebugCommands;

    public DebugCommandBase(string id, string description, string format)
    {
        _id = id;
        _description = description;
        _format = format;
        
        if (DebugCommands == null)
            DebugCommands = new Dictionary<string, DebugCommandBase>();
        string mainKeyword = format.Split(' ')[0];
        DebugCommands[mainKeyword] = this;
    }

    public string Id => _id;
    public string Description => _description;
    public string Format => _format;

}
