using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSelection : MonoBehaviour
{
    private bool _isDraggingMouseBox = false;
    private Vector3 _dragStartPosition;
    Ray _ray;
    RaycastHit _raycastHit;
    private Dictionary<int, List<UnitManager>> _selectionGroups = new Dictionary<int, List<UnitManager>>();
    private UIManager _uiManager;

    private void Awake()
    {
        _uiManager = GetComponent<UIManager>();
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            _isDraggingMouseBox = true;
            _dragStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
            _isDraggingMouseBox = false;

        if (_isDraggingMouseBox && _dragStartPosition!=Input.mousePosition)
        {
            _SelectUnitsInDraggingBox();
        }
        
        if (Globals.SELECTED_UNITS.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                _DeselectAllUnits();
            if (Input.GetMouseButtonDown(0))
            {
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(
                        _ray,
                        out _raycastHit,
                        1000f
                    ))
                {
                    if (_raycastHit.transform.tag == "Terrain")
                        _DeselectAllUnits();
                }
            }
        }
        
        if (Input.anyKeyDown)
        {
            int alphaKey = Utils.GetAlphaKeyValue();
            if (alphaKey != -1)
            {
                if (
                    Input.GetKey(KeyCode.LeftControl) ||
                    Input.GetKey(KeyCode.RightControl) ||
                    Input.GetKey(KeyCode.LeftApple) ||
                    Input.GetKey(KeyCode.RightApple)
                )
                {
                    _CreateSelectionGroup(alphaKey);
                }
                else
                {
                    _ReselectGroup(alphaKey);
                }
                
            }
        }
    }

    private void _SelectUnitsInDraggingBox()
    {
        var bounds = Utils.GetViewportBounds(Camera.main, _dragStartPosition, Input.mousePosition);
        var units = GameObject.FindGameObjectsWithTag("Unit");
        foreach (var unit in units)
        {
            var pos = Camera.main.WorldToViewportPoint(unit.transform.position);
            var inBound =bounds.Contains(pos);
            var unitManager = unit.GetComponent<UnitManager>();
            
            if(inBound)
                unitManager.Select();
            else
                unitManager.Deselect();
        }
    }
    
    public void SelectUnitsGroup(int groupIndex)
    {
        _ReselectGroup(groupIndex);
    }
    
    private void _DeselectAllUnits()
    {
        List<UnitManager> units = new List<UnitManager>(Globals.SELECTED_UNITS);
        foreach (var unit in units)
        {
            unit.Deselect();
        }
    }

    private void _CreateSelectionGroup(int groupIndex)
    {
        // if nothing selected and keys are pressed
        if (Globals.SELECTED_UNITS.Count == 0)
        {
            if (_selectionGroups.ContainsKey(groupIndex))
            {
                _RemoveSelectionGroup(groupIndex);
                return;
            }
        }
        
        List<UnitManager> groupUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
        if (groupUnits.Count>0)
        {
            _selectionGroups[groupIndex] = groupUnits;
            _uiManager.ToggleSelectionGroupButton(groupIndex,true);
        }
    }

    private void _ReselectGroup(int groupIndex)
    {
        // check the group actually is defined
        if (!_selectionGroups.ContainsKey(groupIndex)) return;
        _DeselectAllUnits();
        foreach (UnitManager um in _selectionGroups[groupIndex])
            um.Select();
    }
    
    private void _RemoveSelectionGroup(int groupIndex)
    {
        _selectionGroups.Remove(groupIndex);
        _uiManager.ToggleSelectionGroupButton(groupIndex, false);
    }

    void OnGUI()
    {
        if (_isDraggingMouseBox)
        {
            // Create a rect from both mouse positions
            var rect = Utils.GetScreenRect(_dragStartPosition, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.5f, 1f, 0.4f, 0.2f));
            Utils.DrawScreenRectBorder(rect, 1, new Color(0.5f, 1f, 0.4f));
        }
    }
}
