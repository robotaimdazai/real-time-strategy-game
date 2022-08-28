using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Transform buildingMenu;
    public GameObject buildingButtonPrefab;
    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;
    public GameObject infoPanel;
    public GameObject gameResourceCostPrefab;
    public Color invalidTextColor;
    public Transform selectedUnitsListParent;
    public GameObject selectedUnitDisplayPrefab;

    private BuildingPlacer _buildingPlacer;
    private Dictionary<string, TextMeshProUGUI> _resourceTexts;
    private Dictionary<string, Button> _buildingButtons;
    private TextMeshProUGUI _infoPanelTitleText;
    private TextMeshProUGUI _infoPanelDescriptionText;
    private Transform _infoPanelResourcesCostParent;

    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts",UpdateResourceTexts);
        EventManager.AddListener("CheckBuildingButtons",CheckBuildingButtons);
        EventManager.AddCustomListener("HoverBuildingButton",OnHoverBuildingButton);
        EventManager.AddListener("UnhoverBuildingButton",OnUnHoverBuildingButton);
        EventManager.AddCustomListener("SelectUnit", _OnSelectUnit);
        EventManager.AddCustomListener("DeselectUnit", _OnDeselectUnit);
    }
    
    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts",UpdateResourceTexts);
        EventManager.RemoveListener("CheckBuildingButtons",CheckBuildingButtons);
        EventManager.RemoveCustomListener("HoverBuildingButton",OnHoverBuildingButton);
        EventManager.RemoveListener("UnhoverBuildingButton",OnUnHoverBuildingButton);
    }

    private void Awake()
    {
        _buildingPlacer = GetComponent<BuildingPlacer>();
        _resourceTexts = new Dictionary<string, TextMeshProUGUI>();
        foreach (var pair in Globals.GAME_RESOURCES)
        {
            GameObject g = Instantiate(gameResourceDisplayPrefab, resourcesUIParent);
            g.transform.Find("Icon").GetComponent<Image>().sprite = 
                Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");
            g.name = pair.Key;
            _resourceTexts.Add(pair.Key,g.transform.Find("Text").GetComponent<TextMeshProUGUI>());
            _SetResourceText(pair.Key,pair.Value.Amount);
        }

        // create buttons for each building type
        _buildingButtons = new Dictionary<string, Button>();
        for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
        {
            GameObject button = GameObject.Instantiate(
                buildingButtonPrefab,
                buildingMenu);
            string code = Globals.BUILDING_DATA[i].code;
            button.name = code;
            button.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = code;
            Button b = button.GetComponent<Button>();
            _AddBuildingButtonListener(b, i);
            
            _buildingButtons[code] = b;
            if (!Globals.BUILDING_DATA[i].CanBuy())
            {
                b.interactable = false;
            }
            button.GetComponent<BuildingButton>().Initialize(Globals.BUILDING_DATA[i]);
        }

        _infoPanelTitleText = infoPanel.transform.Find("content/title").GetComponent<TextMeshProUGUI>();
        _infoPanelDescriptionText = infoPanel.transform.Find("content/description").GetComponent<TextMeshProUGUI>();
        _infoPanelResourcesCostParent = infoPanel.transform.Find("content/cost");
        ShowInfoPanel(false);
    }

    
    private void _SetResourceText(string resource, int value)
    {
        _resourceTexts[resource].text = value.ToString();
    }
    private void _AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
    }

    private void OnHoverBuildingButton(CustomEventData customEventData)
    {
        SetInfoPanel(customEventData.unitData);
        ShowInfoPanel(true);
    }
    private void OnUnHoverBuildingButton()
    {
        ShowInfoPanel(false);
    }
    
    private void _OnSelectUnit(CustomEventData data)
    {
        _AddSelectedUnitToUIList(data.unit);
    }

    private void _OnDeselectUnit(CustomEventData data)
    {
        _RemoveSelectedUnitFromUIList(data.unit.Code);
    }

    public void CheckBuildingButtons()
    {
        foreach (UnitData data in Globals.BUILDING_DATA)
        {
            _buildingButtons[data.code].interactable = data.CanBuy();
        }
    }
    
    public void _AddSelectedUnitToUIList(Unit unit)
    {
        // if there is another unit of the same type already selected,
        // increase the counter
        Transform alreadyInstantiatedChild = selectedUnitsListParent.Find(unit.Code);
        if (alreadyInstantiatedChild != null)
        {
            Text t = alreadyInstantiatedChild.Find("Count").GetComponent<Text>();
            int count = int.Parse(t.text);
            t.text = (count + 1).ToString();
        }
        // else create a brand new counter initialized with a count of 1
        else
        {
            GameObject g = GameObject.Instantiate(
                selectedUnitDisplayPrefab, selectedUnitsListParent);
            g.name = unit.Code;
            Transform t = g.transform;
            t.Find("Count").GetComponent<Text>().text = "1";
            t.Find("Name").GetComponent<Text>().text = unit.Data.unitName;
        }
    }
    public void _RemoveSelectedUnitFromUIList(string code)
    {
        Transform listItem = selectedUnitsListParent.Find(code);
        if (listItem == null) return;
        Text t = listItem.Find("Count").GetComponent<Text>();
        int count = int.Parse(t.text);
        count -= 1;
        if (count == 0)
            DestroyImmediate(listItem.gameObject);
        else
            t.text = count.ToString();
    }
    
    
    public void UpdateResourceTexts()
    {
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
        {
            _SetResourceText(pair.Key, pair.Value.Amount);
        }
    }
    
    public void ShowInfoPanel(bool show)
    {
        infoPanel.SetActive(show);
    }
   
    public void SetInfoPanel(UnitData data)
    {
        // update texts
        if (data.code != "")
            _infoPanelTitleText.text = data.unitName;
        if (data.description != "")
            _infoPanelDescriptionText.text = data.description;

        // clear resource costs and reinstantiate new ones
        foreach (Transform child in _infoPanelResourcesCostParent)
            Destroy(child.gameObject);

        if (data.cost.Count > 0)
        {
            GameObject g;
            Transform t;
            foreach (ResourceValue resource in data.cost)
            {
                g = GameObject.Instantiate(gameResourceCostPrefab, _infoPanelResourcesCostParent);
                t = g.transform;
                var title = t.Find("text").GetComponent<TextMeshProUGUI>();
                title.text = resource.amount.ToString();
                t.Find("icon").GetComponent<Image>().sprite = Resources.Load<Sprite>
                    ($"Textures/GameResources/{resource.code}");

                if (Globals.GAME_RESOURCES[resource.code].Amount<resource.amount)
                {
                    title.color = invalidTextColor;
                }

            }
        }
    }
}