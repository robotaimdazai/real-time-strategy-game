using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    enum DisplayType
    {
        None,
        Help,
        Autocomplete
    }

    private DisplayType _displayType = DisplayType.None;
    private bool _showConsole = false;
    private string _consoleInput;
    private string _consolInputName = "consoleInput";

    private void OnEnable()
    {
        EventManager.AddListener("<Input>ShowDebugConsole", _OnShowDebugConsole);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("<Input>ShowDebugConsole", _OnShowDebugConsole);
    }

    private void Awake()
    {
        new DebugCommand("toggle_fov", "Toggles the FOV parameter on/off.", "toggle_fov", () =>
        {
            bool fov = !GameManager.instance.gameGlobalParameters.enableFOV;
            GameManager.instance.gameGlobalParameters.enableFOV = fov;
            EventManager.TriggerEvent("UpdateGameParameter:enableFOV", fov);
        });
        
        new DebugCommand<int>("add_gold", "Adds a given amount of gold to the current player.", "add_gold <amount>", (x) =>
        {
            Globals.GAME_RESOURCES[GameManager.instance.gamePlayersParameters.myPlayerId][InGameResource.Gold].AddAmount(x);
            EventManager.TriggerEvent("UpdateResourceTexts");
        });
        
        new DebugCommand("?", "Lists all available debug commands.", "?", () =>
        {
            _displayType = DisplayType.Help;
        });
    }

    private void _OnShowDebugConsole()
    {
        _showConsole = true;
    }

    private void OnGUI()
    {
        if (_showConsole)
        {
            // fake boxes for opacity
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            //input 
            GUI.SetNextControlName(_consolInputName);
            string newInput = GUI.TextField(new Rect(0, 0, Screen.width, 24), _consoleInput);
            GUI.FocusControl(_consolInputName);
            //log area
            float y = 24;
            GUI.Box(new Rect(0, y, Screen.width, Screen.height - 24), "");
            if (_displayType == DisplayType.Help)
                _ShowHelp(y);
            else if (_displayType == DisplayType.Autocomplete)
                _ShowAutocomplete(y,newInput);
            
            if (_displayType != DisplayType.None && _consoleInput.Length != newInput.Length)
                _displayType = DisplayType.None;
            
            _consoleInput = newInput;
            
            Event e = Event.current;
            if (e.isKey)
            {
                if (e.keyCode == KeyCode.Return && _consoleInput.Length > 0)
                    _OnReturn();
                else if (e.keyCode == KeyCode.Escape)
                    _showConsole = false;
                else if (e.keyCode == KeyCode.Tab)
                    _displayType = DisplayType.Autocomplete;
            }
        }
    }

    private void _ShowHelp(float y)
    {
        foreach (DebugCommandBase command in DebugCommandBase.DebugCommands.Values)
        {
            GUI.Label(
                new Rect(2, y, Screen.width, 20),
                $"{command.Format} - {command.Description}"
            );
            y += 16;
        }
    }

    private void _ShowAutocomplete(float y, string newInput)
    {
        IEnumerable<string> autocompleteCommands =
            DebugCommandBase.DebugCommands.Keys
                .Where(k => k.StartsWith(newInput.ToLower()));
        foreach (string k in autocompleteCommands)
        {
            DebugCommandBase c = DebugCommandBase.DebugCommands[k];
            GUI.Label(
                new Rect(2, y, Screen.width, 20),
                $"{c.Format} - {c.Description}"
            );
            y += 16;
        }
    }
    
    private void _OnReturn()
    {
        _HandleConsoleInput();
        _consoleInput = "";
    }
    private void _HandleConsoleInput()
    {
        // parse input
        string[] inputParts = _consoleInput.Split(' ');
        string mainKeyword = inputParts[0];
        // check against available commands
        DebugCommandBase command;
        if (DebugCommandBase.DebugCommands.TryGetValue(mainKeyword.ToLower(), out command))
        {
            // try to invoke command if it exists
            if (command is DebugCommand dc)
                dc.Invoke();
            else
            {
                if (inputParts.Length < 2)
                {
                    Debug.LogError("Missing parameter!");
                    return;
                }
                if (command is DebugCommand<int> dcInt)
                {
                    int i;
                    if (int.TryParse(inputParts[1], out i))
                        dcInt.Invoke(i);
                }
            }
        }
    }
}
