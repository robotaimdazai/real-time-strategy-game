using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelection : MonoBehaviour
{
    private bool _isDraggingMouseBox = false;
    private Vector3 _dragStartPosition;
    Ray _ray;
    RaycastHit _raycastHit;
    
    private void Update()
    {
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
    
    private void _DeselectAllUnits()
    {
        List<UnitManager> units = new List<UnitManager>(Globals.SELECTED_UNITS);
        foreach (var unit in units)
        {
            unit.Deselect();
        }
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
