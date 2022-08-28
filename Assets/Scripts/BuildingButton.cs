using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UnitData _unitData;

    public void Initialize(UnitData unitData)
    {
        _unitData = unitData;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventManager.TriggerCustomEvent("HoverBuildingButton", new CustomEventData(_unitData));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventManager.TriggerEvent("UnhoverBuildingButton");
    }
}
