using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Vector3 startPosition;
    public GameGlobalParameters gameGlobalParameters;
    public GamePlayersParameters gamePlayersParameters;
    public GameObject fov;
    
    [HideInInspector]
    public bool gameIsPaused;
    
    private Ray _ray;
    private RaycastHit _raycastHit;
    private void Awake()
    {
        DataHandler.LoadGameData();
        instance = this;
        GetComponent<DayNightCycler>().enabled = gameGlobalParameters.enableDayAndNightCycle;
        _GetStartPosition();
        gameIsPaused = false;
        fov.SetActive(gameGlobalParameters.enableFOV);
    }
    
    private void OnEnable()
    {
        EventManager.AddListener("PauseGame", _OnPauseGame);
        EventManager.AddListener("ResumeGame", _OnResumeGame);
        EventManager.AddListener("UpdateGameParameter:enableDayAndNightCycle", _OnUpdateDayAndNightCycle);
        EventManager.AddListener("UpdateGameParameter:enableFOV", _OnUpdateFOV);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PauseGame", _OnPauseGame);
        EventManager.RemoveListener("ResumeGame", _OnResumeGame);
        EventManager.RemoveListener("UpdateGameParameter:enableDayAndNightCycle", _OnUpdateDayAndNightCycle);
        EventManager.RemoveListener("UpdateGameParameter:enableFOV", _OnUpdateFOV);
    }

    

    private void Update()
    {
        if (gameIsPaused) return;
        _CheckUnitsNavigation();
    }
    
    private void _OnUpdateDayAndNightCycle(object data)
    {
        bool dayAndNightIsOn = (bool)data;
        GetComponent<DayNightCycler>().enabled = dayAndNightIsOn;
    }
    private void _OnUpdateFOV(object data)
    {
        bool fovIsOn = (bool)data;
        fov.SetActive(fovIsOn);
    }
    
    private void _OnPauseGame()
    {
        gameIsPaused = true;
        Time.timeScale = 0;
    }

    private void _OnResumeGame()
    {
        gameIsPaused = false;
        Time.timeScale = 1;
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
    
    private void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        DataHandler.SaveGameData();
#endif
    }

}
