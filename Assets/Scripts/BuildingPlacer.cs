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
        SpawnBuilding(
            GameManager.instance.gameGlobalParameters.initialBuilding,
            GameManager.instance.gamePlayersParameters.myPlayerId,
            GameManager.instance.startPosition
        );
        SpawnBuilding(
            GameManager.instance.gameGlobalParameters.initialBuilding,
            0,
            Vector3.zero
        );
    }
    public void SpawnBuilding(BuildingData data, int owner, Vector3 position)
    {
        SpawnBuilding(data, owner, position, new List<ResourceValue>() { });
    }
    public void SpawnBuilding(BuildingData data, int owner, Vector3 position, List<ResourceValue> production)
    {
        Building prevPlacedBuilding = _placedBuilding;
        
        _placedBuilding = new Building(data, owner, production);
        _placedBuilding.SetPosition(position);
       
        _PlaceBuilding(false);
        _placedBuilding = prevPlacedBuilding;
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

        var building = new Building(Globals.BUILDING_DATA[buildingDataIndex],GameManager.instance.gamePlayersParameters.myPlayerId);

        _placedBuilding = building;
        _lastPlacementPosition = Vector3.zero;
    }
    
    void _CancelPlacedBuilding()
    {
        Destroy(_placedBuilding.Transform.gameObject);
        _placedBuilding = null;
    }
    
    void _PlaceBuilding(bool canChain = true)
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
