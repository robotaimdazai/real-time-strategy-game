using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    private Building _placedBuilding = null;
    private Ray _ray;
    private RaycastHit _raycastHit;
    private Vector3 _lastPlacementPosition;

    private void Start()
    {
        _placedBuilding = new Building(GameManager.instance.gameGlobalParameters.initialBuilding);
        _placedBuilding.SetPosition(GameManager.instance.startPosition);
        // link the data into the manager
        _placedBuilding.Transform.GetComponent<BuildingManager>().Initialize(_placedBuilding);
        _PlaceBuilding();
        // make sure we have no building selected when the player starts
        // to play
        _CancelPlacedBuilding();
    }

    void Update()
    {
        if (GameManager.instance.gameIsPaused) return;
        
        if (_placedBuilding != null)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                _CancelPlacedBuilding();
                return;
            }

            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(_ray,out _raycastHit,1000f,Globals.TERRAIN_LAYER_MASK))
            {
                _placedBuilding.SetPosition(_raycastHit.point);
                if (_lastPlacementPosition != _raycastHit.point)
                {
                    _placedBuilding.CheckValidPlacement();
                }
                _lastPlacementPosition = _raycastHit.point;
            }
            if (_placedBuilding.HasValidPlacement && Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                _PlaceBuilding();
            }

        }
    }
    public void SelectPlacedBuilding(int buildingDataIndex)
    {
        _PreparePlacedBuilding(buildingDataIndex);
    }
    void _PreparePlacedBuilding(int buildingDataIndex)
    {
        if(_placedBuilding!=null && !_placedBuilding.IsFixed)
            Destroy(_placedBuilding.Transform.gameObject);

        var building = new Building(Globals.BUILDING_DATA[buildingDataIndex]);
        building.Transform.GetComponent<BuildingManager>().Initialize(building);
        
        _placedBuilding = building;
        _lastPlacementPosition = Vector3.zero;
    }
    
    void _CancelPlacedBuilding()
    {
        Destroy(_placedBuilding.Transform.gameObject);
        _placedBuilding = null;
    }
    
    void _PlaceBuilding()
    {
        _placedBuilding.Place();
        if(_placedBuilding.CanBuy())
            _PreparePlacedBuilding(_placedBuilding.DataIndex);
        else
            _placedBuilding = null;
        
        EventManager.TriggerEvent("UpdateResourceTexts");
        EventManager.TriggerEvent("CheckBuildingButtons");
        EventManager.TriggerEvent("PlaySoundByName", "onBuildingPlacedSound");
    }
}
