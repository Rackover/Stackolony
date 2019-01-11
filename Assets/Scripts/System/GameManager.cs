using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour 
{
    [Header("SYSTEM")]
    public Temporality temporality;
    public FlagReader flagReader;
    public Library library;
    public SFXManager sfxManager;
    public SystemManager systemManager;
    public CityManagement cityManagement;
    public MissionManager missionManager;
    public GridDebugger gridDebugger;
    public CursorManagement cursorManagement;
    public GridManagement gridManagement;
    public StorageBay storageBay;
    public PopulationManager populationManager;

    public Player player;
    public Logger logger;

    [Space(1)][Header("INTERFACE")]
    public DeliveryManagement deliveryManagement;
    public TemporalityInterface temporalityInterface;
    public TooltipGO tooltipGO;
    public BlockInfobox blockInfobox;

    public ErrorDisplay errorDisplay;
    public OptionsDisplay  optionsDisplay;
    public CursorDisplay cursorDisplay;
    [Space(1)]
    [Header("SYSTEM SETTINGS")]
    public bool DEBUG_MODE = false;
    public bool ENABLE_LOGS = true;


    public static GameManager instance;

    void Awake()
    {
        instance = this;
        // SYSTEM
        if(temporality == null) temporality = GetComponentInChildren<Temporality>();
        if(flagReader == null) flagReader = GetComponentInChildren<FlagReader>();
        if(library == null) library = GetComponentInChildren<Library>();
        if(sfxManager == null) sfxManager = GetComponentInChildren<SFXManager>();
        if(systemManager == null) systemManager = GetComponentInChildren<SystemManager>();
        if(cityManagement == null) cityManagement = GetComponentInChildren<CityManagement>();
        if(missionManager == null) missionManager = GetComponentInChildren<MissionManager>();
        if(cursorManagement == null) cursorManagement = FindObjectOfType<CursorManagement>();
        if(gridManagement == null) gridManagement = FindObjectOfType<GridManagement>();
        if(gridDebugger == null) gridDebugger = FindObjectOfType<GridDebugger>();
        if(storageBay == null) storageBay = FindObjectOfType<StorageBay>();
        if (populationManager == null) populationManager = FindObjectOfType<PopulationManager>();
        if (logger == null) logger = GetComponentInChildren<Logger>();

        // INTERFACE
        if (deliveryManagement == null) deliveryManagement = FindObjectOfType<DeliveryManagement>();
        if(temporalityInterface == null) temporalityInterface = FindObjectOfType<TemporalityInterface>();
        if(tooltipGO == null) tooltipGO = FindObjectOfType<TooltipGO>();
        if(blockInfobox == null) blockInfobox = FindObjectOfType<BlockInfobox>();
        if(errorDisplay == null) errorDisplay = FindObjectOfType<ErrorDisplay>();
        if(optionsDisplay == null) optionsDisplay = FindObjectOfType<OptionsDisplay>();
        if(cursorDisplay == null) cursorDisplay = FindObjectOfType<CursorDisplay>();
    }

    void Update()
    {
        CheckInputs();
    }

    void CheckInputs()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            temporality.PauseGame();
        }
    }
}
