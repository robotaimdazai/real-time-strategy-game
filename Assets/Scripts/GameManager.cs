using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Vector3 startPosition;
    public GameParameters gameParameters;
    
    private Ray _ray;
    private RaycastHit _raycastHit;
    private void Awake()
    {
        DataHandler.LoadGameData();
        instance = this;
        GetComponent<DayNightCycler>().enabled = gameParameters.enableDayAndNightCycle;
        _GetStartPosition();
    }
    
    private void Update()
    {
        _CheckUnitsNavigation();
    }
    
    private void _GetStartPosition()
    {
        startPosition = Utils.MiddleOfScreenPointToWorld();
    }

    private void _CheckUnitsNavigation()
    {
        if (Globals.SELECTED_UNITS.Count>0 && Input.GetMouseButtonUp(1))
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(
                    _ray,
                    out _raycastHit,
                    1000f,
                    Globals.TERRAIN_LAYER_MASK))
            {
                foreach (UnitManager unitManager  in Globals.SELECTED_UNITS)
                {
                    if (unitManager.GetType() == typeof(CharacterManager))
                    {
                        var characterManager = (CharacterManager)unitManager;
                        characterManager.MoveTo(_raycastHit.point);
                    }
                }
            }
        }

        
    }


}
