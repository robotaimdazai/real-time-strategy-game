using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
    public Transform selectionGroupsParent;
    public GameObject selectedUnitMenu;
    public GameObject unitSkillButtonPrefab;
    public GameObject gameSettingsPanel;
    public Transform gameSettingsMenusParent;
    public TextMeshProUGUI gameSettingsContentName;
    public Transform gameSettingsContentParent;
    public GameObject gameSettingsMenuButtonPrefab;
    public GameObject gameSettingsParameterPrefab;
    public GameObject sliderPrefab;
    public GameObject togglePrefab;
    public GameObject overlayTint;
    public GameObject selectedUnitMenuUpgradeButton;
    public GameObject selectedUnitMenuDestroyButton;
    public GameObject inputMappingPrefab;
    public GameObject inputBindingPrefab;
    
    
    [Header("Placed Building Production")]
    public RectTransform placedBuildingProductionRectTransform;

    private Unit _selectedUnit;
    private RectTransform _selectedUnitContentRectTransform;
    private TextMeshProUGUI _selectedUnitTitleText;
    private TextMeshProUGUI _selectedUnitLevelText;
    private Transform _selectedUnitResourcesProductionParent;
    private Transform _selectedUnitActionButtonsParent;
    private BuildingPlacer _buildingPlacer;
    private Dictionary<InGameResource, TextMeshProUGUI> _resourceTexts;
    private Dictionary<string, Button> _buildingButtons;
    private TextMeshProUGUI _infoPanelTitleText;
    private TextMeshProUGUI _infoPanelDescriptionText;
    private Transform _infoPanelResourcesCostParent;
    private Dictionary<string, GameParameters> _gameParameters;
    private List<ResourceValue> _selectedUnitNextLevelCost;
    private TextMeshProUGUI _selectedUnitDamageText;
    private TextMeshProUGUI _selectedUnitRangeText;
    private int _myPlayerId;

    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts",_OnUpdateResourceTexts);
        EventManager.AddListener("CheckBuildingButtons",CheckBuildingButtons);
        EventManager.AddListener((string)"HoverBuildingButton",(UnityAction<object>)OnHoverBuildingButton);
        EventManager.AddListener("UnhoverBuildingButton",OnUnHoverBuildingButton);
        EventManager.AddListener((string)"SelectUnit", (UnityAction<object>)_OnSelectUnit);
        EventManager.AddListener((string)"DeselectUnit", (UnityAction<object>)_OnDeselectUnit);
        EventManager.AddListener("UpdatePlacedBuildingProduction", _OnUpdatePlacedBuildingProduction);
        EventManager.AddListener("PlaceBuildingOn", _OnPlaceBuildingOn);
        EventManager.AddListener("PlaceBuildingOff", _OnPlaceBuildingOff);
    }
    
    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts",_OnUpdateResourceTexts);
        EventManager.RemoveListener("CheckBuildingButtons",CheckBuildingButtons);
        EventManager.RemoveListener((string)"HoverBuildingButton",(UnityAction<object>)OnHoverBuildingButton);
        EventManager.RemoveListener("UnhoverBuildingButton",OnUnHoverBuildingButton);
        EventManager.RemoveListener("UpdatePlacedBuildingProduction", _OnUpdatePlacedBuildingProduction);
        EventManager.RemoveListener("PlaceBuildingOn", _OnPlaceBuildingOn);
        EventManager.RemoveListener("PlaceBuildingOff", _OnPlaceBuildingOff);
    }

   

    private void Awake()
    {
        _buildingPlacer = GetComponent<BuildingPlacer>();
        // create texts for each in-game resource (gold, wood, stone...)
        _resourceTexts = new Dictionary<InGameResource, TextMeshProUGUI>();
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES[_myPlayerId])
        {
            GameObject display = GameObject.Instantiate(
                gameResourceDisplayPrefab, resourcesUIParent);
            display.name = pair.Key.ToString();
            _resourceTexts[pair.Key] = display.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            display.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");
            _SetResourceText(pair.Key, pair.Value.Amount);
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
            button.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = Globals.BUILDING_DATA[i].unitName;
            Button b = button.GetComponent<Button>();
            _AddBuildingButtonListener(b, i);
            
            _buildingButtons[code] = b;
            /*
            if (!Globals.BUILDING_DATA[i].CanBuy())
            {
                b.interactable = false;
            }
            */
            button.GetComponent<BuildingButton>().Initialize(Globals.BUILDING_DATA[i]);
        }

        _infoPanelTitleText = infoPanel.transform.Find("content/title").GetComponent<TextMeshProUGUI>();
        _infoPanelDescriptionText = infoPanel.transform.Find("content/description").GetComponent<TextMeshProUGUI>();
        _infoPanelResourcesCostParent = infoPanel.transform.Find("content/cost");
        ShowInfoPanel(false);

        for (int i = 1; i < 10; i++)
        {
            ToggleSelectionGroupButton(i, false);
        }
        
        Transform selectedUnitMenuTransform = selectedUnitMenu.transform;
        _selectedUnitContentRectTransform = selectedUnitMenuTransform
            .Find("Content").GetComponent<RectTransform>();
        _selectedUnitTitleText = selectedUnitMenuTransform
            .Find("Content/Title").GetComponent<TextMeshProUGUI>();
        _selectedUnitLevelText = selectedUnitMenuTransform
            .Find("Content/Level").GetComponent<TextMeshProUGUI>();
        _selectedUnitResourcesProductionParent = selectedUnitMenuTransform
            .Find("Content/ResourcesProduction");
        _selectedUnitActionButtonsParent = selectedUnitMenuTransform
            .Find("SpecificActions");
        _selectedUnitDamageText = selectedUnitMenuTransform.Find("Content/Damage").GetComponent<TextMeshProUGUI>();
        _selectedUnitRangeText = selectedUnitMenuTransform.Find("Content/Range").GetComponent<TextMeshProUGUI>();
        
        _ShowSelectedUnitMenu(false);
        gameSettingsPanel.SetActive(false);
        
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>(
            "ScriptableObjects/Parameters");
        _gameParameters = new Dictionary<string, GameParameters>();
        foreach (GameParameters p in gameParametersList)
            _gameParameters[p.GetParametersName()] = p;
        _SetupGameSettingsPanel();
    }

    private void Start()
    {
        _myPlayerId = GameManager.instance.gamePlayersParameters.myPlayerId;

        // set player indicator color to match my player color
        Color c = GameManager.instance.gamePlayersParameters.players[_myPlayerId].color;

        

        _CheckBuyLimits();
    }

    private void _OnPlaceBuildingOn()
    {
        placedBuildingProductionRectTransform.gameObject.SetActive(true);
    }

    private void _OnPlaceBuildingOff()
    {
        placedBuildingProductionRectTransform.gameObject.SetActive(false);
    }

    private void _OnUpdatePlacedBuildingProduction(object data)
    {
        object[] values = (object[])data;
        Dictionary<InGameResource, int> production = (Dictionary<InGameResource, int>) values[0];
        Vector3 pos = (Vector3) values[1];
        foreach (Transform child in placedBuildingProductionRectTransform.gameObject.transform)
            Destroy(child.gameObject);
        GameObject g;
        Transform t;
        foreach (KeyValuePair<InGameResource, int> pair in production)
        {
            g = GameObject.Instantiate(
                gameResourceCostPrefab,
                placedBuildingProductionRectTransform.transform);
            t = g.transform;
            t.Find("Text").GetComponent<TextMeshProUGUI>().text = $"+{pair.Value}";
            t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");
            placedBuildingProductionRectTransform.sizeDelta = new Vector2(80, 24 * production.Count);
            placedBuildingProductionRectTransform.anchoredPosition =
                (Vector2) Camera.main.WorldToScreenPoint(pos)
                + Vector2.right * 40f
                + Vector2.up * 10f;
        }
    }

    private void _SetupGameSettingsPanel()
    {
        List<String> availableMenus = new List<string>();
        foreach (var parameter in _gameParameters.Values)
        {
            if (parameter.FieldsToShowInGame.Count == 0)
                continue;

            var spawnedMenu =Instantiate(gameSettingsMenuButtonPrefab, gameSettingsMenusParent);
            var label =spawnedMenu.GetComponentInChildren<TextMeshProUGUI>();
            var parameterName = parameter.GetParametersName();
            var button = spawnedMenu.GetComponent<Button>();
            if (label)
                label.text = parameterName;
            availableMenus.Add(parameterName);
            _AddGameSettingsPanelMenuListener(button, parameterName);
        }
    }
    
    public void ClickLevelUpButton()
    {
        _selectedUnit.LevelUp();
        _SetSelectedUnitMenu(_selectedUnit,!_selectedUnit.LevelMaxedOut);
        if (_selectedUnit.LevelMaxedOut)
        {
            selectedUnitMenuUpgradeButton.transform.Find("Text").GetComponent<Text>().text = "Maxed out";
            selectedUnitMenuUpgradeButton.GetComponent<Button>().interactable = false;
            ShowInfoPanel(false);
        }
        else
        {
            _UpdateSelectedUnitLevelUpInfoPanel();
            _CheckBuyLimits();
        }
    }
    
    private void _CheckBuyLimits()
    {
        // check if level up button is disabled or not
        if (
            _selectedUnit != null &&
            _selectedUnit.Owner == GameManager.instance.gamePlayersParameters.myPlayerId &&
            !_selectedUnit.LevelMaxedOut &&
            Globals.CanBuy(_selectedUnit.LevelUpData.cost)
        )
            selectedUnitMenuUpgradeButton.GetComponent<Button>().interactable = true;
            
        // check if building buttons are disabled or not
        _OnCheckBuildingButtons();
        
        // check if buy/upgrade is affordable: update text colors
        if (infoPanel.activeSelf)
        {
            foreach (Transform resourceDisplay in _infoPanelResourcesCostParent)
            {
                InGameResource resourceCode = (InGameResource) System.Enum.Parse(
                    typeof(InGameResource),
                    resourceDisplay.Find("Icon").GetComponent<Image>().sprite.name
                );
                TextMeshProUGUI txt = resourceDisplay.Find("Text").GetComponent<TextMeshProUGUI>();
                int resourceAmount = int.Parse(txt.text);
                if (Globals.GAME_RESOURCES[_myPlayerId][resourceCode].Amount < resourceAmount)
                    txt.color = invalidTextColor;
                else
                    txt.color = Color.white;
            }
        }
    }
    
    private void _OnCheckBuildingButtons()
    {
        foreach (BuildingData data in Globals.BUILDING_DATA)
            _buildingButtons[data.code].interactable = data.CanBuy(_myPlayerId);
    }
    
    private void _AddGameSettingsPanelMenuListener(Button b, string menu)
    {
        b.onClick.AddListener(() => _SetGameSettingsContent(menu));
    }

    private void _SetGameSettingsContent(string menuName)
    {
        gameSettingsContentName.text = menuName;

        //destroy all previous menus
        foreach (Transform child in gameSettingsContentParent)
        {
            Destroy(child.gameObject);
        }

        var parameters = _gameParameters[menuName];
        var parameterType = parameters.GetType();
        int index = 0;
        GameObject gWrapper, gEditor;
        RectTransform rtWrapper, rtEditor;
        int i = 0;
        float contentWidth = 400f;
        float parameterNameWidth = 200f;
        float fieldHeight = 32f;
        bool isBindMenu = false;
        foreach (var fieldName in parameters.FieldsToShowInGame)
        {
            gWrapper = Instantiate(gameSettingsParameterPrefab, gameSettingsContentParent);
            var gameSettingsLabel = gWrapper.GetComponent<TextMeshProUGUI>();
            gameSettingsLabel.text = Utils.CapitalizeText(fieldName);
            var field = parameterType.GetField(fieldName);
            var fieldType = field.FieldType;
            
            gEditor = null;
            if (fieldType == typeof(bool))
            {
                gEditor = Instantiate(togglePrefab);
                var toggle = gEditor.GetComponent<Toggle>();
                toggle.isOn = (bool) field.GetValue(parameters);
                toggle.onValueChanged.AddListener(delegate
                {
                    _OnGameSettingsToggleValueChanged(parameters, field, fieldName, toggle);
                });
            }
            else if (fieldType == typeof(int) || fieldType == typeof(float))
            {
                bool isRange = Attribute.IsDefined(field, typeof(RangeAttribute), false);
                if (isRange)
                {
                    var rangeAttribute = (RangeAttribute)Attribute.GetCustomAttribute(field, typeof(RangeAttribute), false);
                    if (rangeAttribute != null)
                    {
                        gEditor = Instantiate(sliderPrefab);
                        var slider = gEditor.GetComponent<Slider>();
                        slider.maxValue = rangeAttribute.max;
                        slider.minValue = rangeAttribute.min;
                        slider.value = fieldType == typeof(int)
                            ? (int)field.GetValue(parameters)
                            : (float)field.GetValue(parameters);
                        slider.onValueChanged.AddListener(delegate
                        {
                            _OnGameSettingsSliderValueChanged(parameters, field, fieldName, slider);
                        });
                    }
                }
            }
            else if (field.FieldType.IsArray && field.FieldType.GetElementType() == typeof(InputBinding))
            {
                gEditor = Instantiate(inputMappingPrefab);
                InputBinding[] bindings = (InputBinding[])field.GetValue(parameters);
                for (int b = 0; b < bindings.Length; b++)
                {
                    GameObject g = Instantiate(
                        inputBindingPrefab, gEditor.transform);
                    g.transform.GetComponent<TextMeshProUGUI>().text = bindings[b].displayName;
                    g.transform.Find("Key/Text").GetComponent<TextMeshProUGUI>().text = bindings[b].key;
                    _AddInputBindingButtonListener(
                        g.transform.Find("Key").GetComponent<Button>(),gEditor.transform ,(GameInputParameters) parameters,b);
                }
                
                isBindMenu = true;
            }
            
            rtWrapper = gWrapper.GetComponent<RectTransform>();
            rtWrapper.anchoredPosition = new Vector2(0f, -i * fieldHeight);
            rtWrapper.sizeDelta = new Vector2(contentWidth, fieldHeight);

            if (gEditor != null)
            {
                gEditor.transform.SetParent(gWrapper.transform);
                
                rtEditor = gEditor.GetComponent<RectTransform>();
                if(isBindMenu)
                    rtEditor.anchoredPosition = new Vector2(0f, 0f);
                else
                    rtEditor.anchoredPosition = new Vector2((parameterNameWidth + 16f), 0f);
                
                rtEditor.sizeDelta = new Vector2(rtWrapper.sizeDelta.x - (parameterNameWidth + 16f), fieldHeight);
            }

            i++;
        }
        
        RectTransform rt = gameSettingsContentParent.GetComponent<RectTransform>();
        Vector2 size = rt.sizeDelta;
        size.y = i * fieldHeight;
        rt.sizeDelta = size;
    }
    
    private void _AddInputBindingButtonListener(Button b, Transform inputBindingsParent, GameInputParameters inputParams, int bindingIndex)
    {
        b.onClick.AddListener(() =>
        {
            var keyText = b.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            StartCoroutine(_WaitingForInputBinding(inputParams,inputBindingsParent, bindingIndex, keyText));
        });
    }
    
    private IEnumerator _WaitingForInputBinding(GameInputParameters inputParams,  Transform inputBindingsParent,int bindingIndex, TextMeshProUGUI keyText)
    {
        keyText.text = "<?>";

        GameManager.instance.waitingForInput = true;
        GameManager.instance.pressedKey = string.Empty;
        
        yield return new WaitUntil(() => !GameManager.instance.waitingForInput);

        string key = GameManager.instance.pressedKey;
        (int prevBindingIndex, InputBinding prevBinding) =
            GameManager.instance.gameInputParameters.GetBindingForKey(key);
        if (prevBinding != null)
        {
            prevBinding.key = string.Empty;
            inputBindingsParent.GetChild(prevBindingIndex).Find("Key/Text").GetComponent<TextMeshProUGUI>().text = string.Empty;
        }

        inputParams.bindings[bindingIndex].key = key;
        keyText.text = key;
    }

    private void _OnGameSettingsToggleValueChanged(GameParameters parameters, FieldInfo field, string gameParameter, Toggle change)
    {
        field.SetValue(parameters,change.isOn);
        EventManager.TriggerEvent($"UpdateGameParameter:{gameParameter}", change.isOn);
    }
    
    private void _OnGameSettingsSliderValueChanged(
        GameParameters parameters,
        FieldInfo field,
        string gameParameter,
        Slider change
    )
    {
        if (field.FieldType == typeof(int))
            field.SetValue(parameters, (int) change.value);
        else
            field.SetValue(parameters, change.value);
        EventManager.TriggerEvent($"UpdateGameParameter:{gameParameter}", change.value);
    }
    public void ToggleSelectionGroupButton(int groupIndex, bool on)
    {
        selectionGroupsParent.Find(groupIndex.ToString()).gameObject.SetActive(on);
    }
    
    public void ToggleGameSettingsPanel()
    {
        bool showGameSettingsPanel = !gameSettingsPanel.activeSelf;
        gameSettingsPanel.SetActive(showGameSettingsPanel);
        EventManager.TriggerEvent(showGameSettingsPanel ? "PauseGame" : "ResumeGame");
        overlayTint.gameObject.SetActive(showGameSettingsPanel);
    }
    
    private void _SetResourceText(InGameResource resource, int value)
    {
        _resourceTexts[resource].text = value.ToString();
    }
    private void _AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
    }
    
    private void _UpdateSelectedUnitLevelUpInfoPanel()
    {
        int nextLevel = _selectedUnit.Level + 1;
        SetInfoPanel("Level up", $"Upgrade unit to level {nextLevel}", _selectedUnit.LevelUpData.cost);
    }

    
    public void HoverLevelUpButton()
    {
        if (_selectedUnit.LevelMaxedOut) return;
        _UpdateSelectedUnitLevelUpInfoPanel();
        ShowInfoPanel(true);
        _SetSelectedUnitMenu(_selectedUnit, true);
    }

    public void UnhoverLevelUpButton()
    {
        if (_selectedUnit.LevelMaxedOut) return;
        ShowInfoPanel(false);
        _SetSelectedUnitMenu(_selectedUnit);
    }
    private void OnHoverBuildingButton(object data)
    {
        
        SetInfoPanel((UnitData)data);
        ShowInfoPanel(true);
    }
    private void OnUnHoverBuildingButton()
    {
        ShowInfoPanel(false);
    }
    
    private void _OnSelectUnit(object data)
    {
        _AddSelectedUnitToUIList((Unit)data);
        _SetSelectedUnitMenu((Unit)data);
        _ShowSelectedUnitMenu(true);
    }

    private void _OnDeselectUnit(object data)
    {
        var unitData = data as Unit;
        _RemoveSelectedUnitFromUIList(unitData.Code);
        if (Globals.SELECTED_UNITS.Count == 0)
            _ShowSelectedUnitMenu(false);
        else
            _SetSelectedUnitMenu(Globals.SELECTED_UNITS[Globals.SELECTED_UNITS.Count - 1].Unit);
    }

    public void CheckBuildingButtons()
    {
        foreach (UnitData data in Globals.BUILDING_DATA)
        {
            _buildingButtons[data.code].interactable = data.CanBuy(_myPlayerId);
        }
    }
    
    public void _AddSelectedUnitToUIList(Unit unit)
    {
        // if there is another unit of the same type already selected,
        // increase the counter
        Transform alreadyInstantiatedChild = selectedUnitsListParent.Find(unit.Code);
        if (alreadyInstantiatedChild != null)
        {
            TextMeshProUGUI t = alreadyInstantiatedChild.Find("Count").GetComponent<TextMeshProUGUI>();
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
            t.Find("Count").GetComponent<TextMeshProUGUI>().text = "1";
            t.Find("Name").GetComponent<TextMeshProUGUI>().text = unit.Data.unitName;
        }
    }
    
    public void _RemoveSelectedUnitFromUIList(string code)
    {
        Transform listItem = selectedUnitsListParent.Find(code);
        if (listItem == null) return;
        TextMeshProUGUI t = listItem.Find("Count").GetComponent<TextMeshProUGUI>();
        int count = int.Parse(t.text);
        count -= 1;
        if (count == 0)
            DestroyImmediate(listItem.gameObject);
        else
            t.text = count.ToString();
    }
    
    
    public void _OnUpdateResourceTexts()
    {
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES[_myPlayerId])
            _SetResourceText(pair.Key, pair.Value.Amount);

        _CheckBuyLimits();
    }

    public void SetInfoPanel(string title, string description, List<ResourceValue> resourceCosts)
    {
        // update texts
        _infoPanelTitleText.text = title;
        _infoPanelDescriptionText.text = description;
        // clear resource costs and reinstantiate new ones
        foreach (Transform child in _infoPanelResourcesCostParent)
            Destroy(child.gameObject);
        if (resourceCosts.Count > 0)
        {
            GameObject g;
            Transform t;
            foreach (ResourceValue resource in resourceCosts)
            {
                g = GameObject.Instantiate(
                    gameResourceCostPrefab, _infoPanelResourcesCostParent);
                t = g.transform;
                t.Find("Text").GetComponent<TextMeshProUGUI>().text = resource.amount.ToString();
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(
                    $"Textures/GameResources/{resource.code}");
                // check to see if resource requirement is not
                // currently met - in that case, turn the text into the "invalid"
                // color
                if (Globals.GAME_RESOURCES[_myPlayerId][resource.code].Amount < resource.amount)
                    t.Find("Text").GetComponent<TextMeshProUGUI>().color = invalidTextColor;
            }
        }
    }

    public void ShowInfoPanel(bool show)
    {
        infoPanel.SetActive(show);
    }
   
    public void SetInfoPanel(UnitData data)
    {
        SetInfoPanel(data.unitName, data.description, data.cost);
    }

    private void _SetSelectedUnitMenu(Unit unit, bool showUpgrade = false)
    {
        _selectedUnit = unit;
        _selectedUnitNextLevelCost = _selectedUnit.GetLevelUpCost();

        bool unitIsMine = unit.Owner == GameManager.instance.gamePlayersParameters.myPlayerId;

        // adapt content panel heights to match info to display
        int contentHeight = unitIsMine ? 60 + unit.Production.Count * 16 : 60;
        _selectedUnitContentRectTransform.sizeDelta = new Vector2(64, contentHeight);
        //_selectedUnitButtonsRectTransform.anchoredPosition = new Vector2(0, -contentHeight - 20);
        //_selectedUnitButtonsRectTransform.sizeDelta = new Vector2(70, Screen.height - contentHeight - 20);
        // update texts
        _selectedUnitTitleText.text = unit.Data.unitName;
        _selectedUnitLevelText.text = $"Level {unit.Level}";
        // clear resource production and reinstantiate new one
        foreach (Transform child in _selectedUnitResourcesProductionParent)
            Destroy(child.gameObject);
        if (unitIsMine && unit.Production.Count > 0)
        {
            GameObject g; Transform t;
            foreach (var resource in unit.Production)
            {
                g = Instantiate(
                    gameResourceCostPrefab, _selectedUnitResourcesProductionParent);
                t = g.transform;
                t.Find("Text").GetComponent<TextMeshProUGUI>().text = showUpgrade ?
                    $"<color=#00ff00>+{_selectedUnit.LevelUpData.newProduction[resource.Key]}</color>"
                    : $"+{resource.Value}";
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{resource.Key}");
            }
        }

        if (unitIsMine)
        {
            _selectedUnitDamageText.text = showUpgrade?
                $"Damage: <color=#00ff00>{_selectedUnit.LevelUpData.newAttackDamage}</color>"
                : $"Damage: {unit.AttackDamage}";
            
            
            _selectedUnitRangeText.text = showUpgrade?
                $"Range: <color=#00ff00>{(int) _selectedUnit.LevelUpData.newAttackRange}</color>"
                : $"Range: {(int) unit.AttackRange}";
        }

        _selectedUnit = unit;
        // ...
        // clear skills and reinstantiate new ones
        foreach (Transform child in _selectedUnitActionButtonsParent)
            Destroy(child.gameObject);
        if (unit.SkillManagers.Count > 0)
        {
            GameObject g; Transform t; Button b;
            for (int i = 0; i < unit.SkillManagers.Count; i++)
            {
                g = GameObject.Instantiate(
                    unitSkillButtonPrefab, _selectedUnitActionButtonsParent);
                t = g.transform;
                b = g.GetComponent<Button>();
                unit.SkillManagers[i].SetButton(b);
                t.Find("Text").GetComponent<TextMeshProUGUI>().text =
                    unit.SkillManagers[i].skill.skillName;
                _AddUnitSkillButtonListener(b, i);
            }
        }
        if (unitIsMine)
            selectedUnitMenuUpgradeButton.GetComponent<Button>().interactable = Globals.CanBuy(_selectedUnit.LevelUpData.cost);
    }
    
    private void _ShowSelectedUnitMenu(bool show)
    {
        selectedUnitMenu.SetActive(show);
    }
    private void _AddUnitSkillButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _selectedUnit.TriggerSkill(i));
    }
}
