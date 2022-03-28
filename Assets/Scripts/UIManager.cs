
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;
    public Transform buildingMenu;
    public GameObject buildingButtonPrefab;
    public GameObject infoPanel;
    public GameObject gameResourceCostPrefab;
    public Color invalidTextColor;

    private BuildingPlacer _buildingPlacer;
    private Dictionary<string, Text> _resourceTexts;
    private Dictionary<string, Button> _buildingButtons;
    private Text _infoPanelTitleText;
    private Text _infoPanelDescriptionText;
    private Transform _infoPanelResourcesCostParent;

    private void Awake()
    {
        _buildingPlacer = GetComponent<BuildingPlacer>();

        // create texts for each in-game resource (gold, wood, stone...)
        _resourceTexts = new Dictionary<string, Text>();
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
        {
            GameObject display = Instantiate(gameResourceDisplayPrefab, resourcesUIParent);
            display.name = pair.Key;
            _resourceTexts[pair.Key] = display.transform.Find("Text").GetComponent<Text>();
            display.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");
            _SetResourceText(pair.Key, pair.Value.Amount);
        }

        // create buttons for each building type
        _buildingButtons = new Dictionary<string, Button>();
        for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
        {
            BuildingData data = Globals.BUILDING_DATA[i];

            GameObject button = GameObject.Instantiate(buildingButtonPrefab, buildingMenu);
            button.name = data.unitName;
            button.transform.Find("Text").GetComponent<Text>().text = data.unitName;

            Button b = button.GetComponent<Button>();
            _AddBuildingButtonListener(b, i);
            if (!Globals.BUILDING_DATA[i].CanBuy())
                b.interactable = false;
            _buildingButtons[data.code] = b;

            button.GetComponent<BuildingButton>().Initialize(data);
        }

        // info panel
        Transform infoPanelTransform = infoPanel.transform;

        _infoPanelTitleText = infoPanelTransform
            .Find("Content/Title").GetComponent<Text>();
        _infoPanelDescriptionText = infoPanelTransform
            .Find("Content/Description").GetComponent<Text>();
        _infoPanelResourcesCostParent = infoPanelTransform
            .Find("Content/ResourcesCost");

        ShowInfoPanel(false);
    }

    public void ShowInfoPanel(bool show)
    {
        infoPanel.SetActive(show);
    }

    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.AddListener("CheckBuildingButtons", _OnCheckBuildingButtons);

        EventManager.AddTypedListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.AddListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.RemoveListener("CheckBuildingButtons", _OnCheckBuildingButtons);

        EventManager.RemoveTypedListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.RemoveListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);
    }

    // Create another function with its own scope. This way i has the value it had during 
    // the iteration, not the changing value of the reference of i defined in the loop.
    private void _AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
    }

    private void _OnUpdateResourceTexts()
    {
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
            _SetResourceText(pair.Key, pair.Value.Amount);
    }
    
    private void _OnCheckBuildingButtons()
    {
        foreach (BuildingData data in Globals.BUILDING_DATA)
            _buildingButtons[data.code].interactable = data.CanBuy();
    }

    private void _SetResourceText(string resource, int value)
    {
        _resourceTexts[resource].text = value.ToString();
    }

    private void _OnHoverBuildingButton(CustomEventData data)
    {
        SetInfoPanel(data.unitData);
        ShowInfoPanel(true);
    }

    private void _OnUnhoverBuildingButton()
    {
        ShowInfoPanel(false);
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
            GameObject g; Transform t;
            foreach (ResourceValue resource in data.cost)
            {
                g = GameObject.Instantiate(gameResourceCostPrefab, _infoPanelResourcesCostParent);
                t = g.transform;
                t.Find("Text").GetComponent<Text>().text = resource.amount.ToString();
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(
                    $"Textures/GameResources/{resource.code}");
                if (Globals.GAME_RESOURCES[resource.code].Amount < resource.amount)
                     t.Find("Text").GetComponent<Text>().color = invalidTextColor;
            }
        }
    }
}
