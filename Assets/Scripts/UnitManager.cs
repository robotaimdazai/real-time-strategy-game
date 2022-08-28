using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public GameObject selectionCircle;
    
    protected BoxCollider _collider;
    public virtual Unit Unit { get; set; }
    
    private Transform _canvas;
    private GameObject _healthbar;
   
    

    private void Awake()
    {
        _canvas = GameObject.Find("Canvas").transform;
    }

    private void OnMouseDown()
    {
        if (IsActive())
        {
            bool isHoldingShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            Select(true, isHoldingShift);
        }
    }

    public void Initialize(Unit unit)
    {
        _collider = GetComponent<BoxCollider>();
        Unit = unit;
    }
    protected virtual bool IsActive()
    {
        return true;
    }

    public void Select(bool singleClick, bool holdingShift)
    {
        if (!singleClick)
        {
            _SelectUtil();
            return;
        }

        if (!holdingShift)
        {
            List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
            foreach (var unit in selectedUnits)
            {
                unit.Deselect();
            }
            _SelectUtil();
        }
        else
        {
            if (!Globals.SELECTED_UNITS.Contains(this))
            {
                _SelectUtil();
            }
            else
            {
                Deselect();
            }
        }

    }

    private void _SelectUtil()
    {
        if (Globals.SELECTED_UNITS.Contains(this)) return;
        Globals.SELECTED_UNITS.Add(this);
        selectionCircle.SetActive(true);
        if (_healthbar == null)
        {
            _healthbar = GameObject.Instantiate(Resources.Load("Prefabs/UI/Healthbar")) as GameObject;
            _healthbar.transform.SetParent(_canvas);
            Healthbar h = _healthbar.GetComponent<Healthbar>();
            var bounds = transform.Find("Mesh").GetComponent<MeshRenderer>().bounds;
            var rect = Utils.GetBoundingBoxOnScreen(bounds,Camera.main);
            h.Initialize(transform,rect.height);
            h.SetPosition();
        }
        EventManager.TriggerCustomEvent("SelectUnit", new CustomEventData(Unit));
    }
    
    public void Select()
    {
        Select(false, false);
    }

    public void Deselect()
    {
        if (!Globals.SELECTED_UNITS.Contains(this)) return;
        Globals.SELECTED_UNITS.Remove(this);
        selectionCircle.SetActive(false);
        Destroy(_healthbar);
        _healthbar = null;
        EventManager.TriggerCustomEvent("DeselectUnit", new CustomEventData(Unit));
    }
}
