using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BuildingManager : UnitManager
{
    private BoxCollider _collider;

    private Building _building = null;
    private int _nCollisions = 0;

    protected override Unit Unit {
        get
        {
            return _building;
        }
        set
        {
            _building = value is Building ? (Building)value : null;
        }
    }

    public void Initialize(Building building)
    {
        _collider = GetComponent<BoxCollider>();
        _building = building;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Terrain") return;
        _nCollisions++;
        CheckPlacement();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Terrain") return;
        _nCollisions--;
        CheckPlacement();
    }

    protected override bool IsActive()
    {
        return _building.IsFixed;
    }

    public bool CheckPlacement()
    {
        if (_building == null) return false;
        if (_building.IsFixed) return false;
        bool validPlacement = HasValidPlacement();
        if (!validPlacement)
        {
            _building.SetMaterials(BuildingPlacement.INVALID);
        }
        else
        {
            _building.SetMaterials(BuildingPlacement.VALID);
        }
        return validPlacement;
    }

    public bool HasValidPlacement()
    {
        
        if (_nCollisions > 0) return false;
        
        var p = transform.position;
        var c = _collider.center;
        var r = _collider.size / 2;

        var bottomHeight = c.y - r.y + 0.5f;

        Vector3[] bottomCorners = new Vector3[]
        {
            new Vector3(c.x - r.x,bottomHeight,c.z-r.z),
            new Vector3(c.x - r.x,bottomHeight,c.z +r.z),
            new Vector3(c.x + r.x,bottomHeight,c.z -r.z),
            new Vector3(c.x + r.x,bottomHeight,c.z +r.z),
        };

        int invalidCornersCount = 0;
        foreach (var corner in bottomCorners)
        {
            if (!Physics.Raycast(p+corner,Vector3.up*-1f,2f,Globals.TERRAIN_LAYER_MASK))
            {
                invalidCornersCount++;
            }
        }

        return invalidCornersCount < 3;
    }
}
