using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public GameObject selectionCircle;
    public GameObject fov;
    public AudioSource contextualSource;
    public int ownerMaterialSlotIndex = 0;
    
    protected BoxCollider _collider;
    public virtual Unit Unit { get; set; }
    
    private Transform _canvas;
    private GameObject _healthbar;
    private bool _selected = false;
    private GameObject _levelUpVFX;
    private Material _levelUpVFXMaterial;
    private Coroutine _levelUpVFXCoroutine = null;
    public bool IsSelected { get => _selected; }
    

    private void Awake()
    {
        _canvas = GameObject.Find("Canvas").transform;
    }

    private void OnMouseDown()
    {
        if (IsActive() && _IsMyUnit())
        {
            bool isHoldingShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            Select(true, isHoldingShift);
        }
    }

    private void Update()
    {
        if (_selected && Input.GetKeyDown(KeyCode.L))
        {
            if (Globals.CanBuy(Unit.GetLevelUpCost()))
            {
                Unit.LevelUp();
            }
            else
            {
                Debug.LogError("Can't buy the upgrade!");
            }
        }
    }
    
    public void SetOwnerMaterial(int owner)
    {
        Color playerColor = GameManager.instance.gamePlayersParameters.players[owner].color;
        Material[] materials = transform.Find("Mesh").GetComponent<Renderer>().materials;
        materials[ownerMaterialSlotIndex].color = playerColor;
        transform.Find("Mesh").GetComponent<Renderer>().materials = materials;
    }
    
    public void LevelUp()
    {
        if (_levelUpVFX)
        {
            if (_levelUpVFXCoroutine != null)
                StopCoroutine(_levelUpVFXCoroutine);
            _levelUpVFXCoroutine = null;
            Destroy(_levelUpVFX);
        }
        // play sound
        // create visual effect (+ discard it after a few seconds)
        
        
        GameObject vfx = Instantiate(Resources.Load("Prefabs/Units/LevelUpVFX")) as GameObject;
        Vector3 meshScale = transform.Find("Mesh").localScale;
        float s = Mathf.Max(meshScale.x, meshScale.z);
        Transform t = vfx.transform;
        t.localScale = new Vector3(s, meshScale.y, s);
        t.position = transform.position;

        _levelUpVFX = vfx;
        _levelUpVFXMaterial = t.GetComponent<Renderer>().material;
        _levelUpVFXCoroutine = StartCoroutine(_UpdatingLevelUpVFX());
    }
    
    private IEnumerator _UpdatingLevelUpVFX()
    {
        float lifetime = 1f, t = 0f, step = 0.05f;
        while (t < lifetime) {
            _levelUpVFXMaterial.SetFloat("_CurrentTime", t);
            t += step;
            yield return new WaitForSeconds(step);
        }
        Destroy(_levelUpVFX);
    }
    
    public void Attack(Transform target)
    {
        UnitManager um = target.GetComponent<UnitManager>();
        if (um == null) return;
        um.TakeHit(Unit.AttackDamage);
    }

    public void TakeHit(int attackPoints)
    {
        Unit.HP -= attackPoints;
        _UpdateHealthbar();
        if (Unit.HP <= 0) _Die();
    }

    private void _Die()
    {
        if (_selected)
            Deselect();            
        Destroy(gameObject);
    }
    private void _UpdateHealthbar()
    {
        if (!_healthbar) return;
        Transform fill = _healthbar.transform.Find("Fill");
        fill.GetComponent<UnityEngine.UI.Image>().fillAmount = Unit.HP / (float)Unit.MaxHP;
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
    private bool _IsMyUnit()
    {
        return Unit.Owner == GameManager.instance.gamePlayersParameters.myPlayerId;
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
            _UpdateHealthbar();
        }
        EventManager.TriggerEvent("SelectUnit", Unit);
        contextualSource.PlayOneShot(Unit.Data.onSelectSound);
        _selected = true;
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
        _selected = false;
    }
}
