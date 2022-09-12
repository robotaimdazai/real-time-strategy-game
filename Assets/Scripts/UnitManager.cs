using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public GameObject selectionCircle;
    public GameObject fov;
    
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
    public void EnableFOV(float size)
    {
        fov.SetActive(true);
        MeshRenderer mr = fov.GetComponent<MeshRenderer>();
        mr.material = new Material(mr.material);
        StartCoroutine(_ScalingFOV(size));
    }
    
    public void EnableFOV()
    {
        fov.SetActive(true);
    }
    private IEnumerator _ScalingFOV(float size)
    {
        float r = 0f, t = 0f, step = 0.05f;
        float scaleUpTime = 0.35f;
        Vector3 _startScale = fov.transform.localScale;
        Vector3 _endScale = size * Vector3.one;
        _endScale.z = 1f;
        do
        {
            fov.transform.localScale = Vector3.Lerp(_startScale, _endScale, r);
            t += step;
            r = t / scaleUpTime;
            yield return new WaitForSecondsRealtime(step);
        } while (r < 1f);
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
        EventManager.TriggerEvent("SelectUnit", Unit);
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
        EventManager.TriggerEvent("DeselectUnit", Unit);
    }
}
