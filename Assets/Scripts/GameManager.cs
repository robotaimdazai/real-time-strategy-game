using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    private Ray _ray;
    private RaycastHit _raycastHit;
    
    public GameParameters gameParameters;
    private void Awake()
    {
        DataHandler.LoadGameData();
        GetComponent<DayNightCycler>().enabled = gameParameters.enableDayAndNightCycle;
    }

    private void Update()
    {
        _CheckUnitsNavigation();
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
