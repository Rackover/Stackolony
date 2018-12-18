using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("SYSTEM")]
    public Temporality temporality;
    public FlagReader flagReader;
    public Library library;
    public SFXManager sfxManager;
    public SystemReferences systemReferences;
    public CityManagement cityManagement;
    public MissionManager missionManager;
    public GridDebugger gridDebugger;
    public CursorManagement cursorManagement;
    public GridManagement gridManagement;
    public StorageBay storageBay;

    [Space(1)][Header("INTERFACE")]
    public Interface generalInterface;
    public DeliveryManagement deliveryManagement;
    public TemporalityInterface temporalityInterface;
    public TooltipGO tooltipGO;
    public BlockInfobox blockInfobox;

    public static GameManager instance;

    void Awake()
    {
        temporality = GetComponentInChildren<Temporality>();
        flagReader = GetComponentInChildren<FlagReader>();
        library = GetComponentInChildren<Library>();
        sfxManager = GetComponentInChildren<SFXManager>();
        systemReferences = GetComponentInChildren<SystemReferences>();
        cityManagement = GetComponentInChildren<CityManagement>();
        missionManager = GetComponentInChildren<MissionManager>();
        cursorManagement = FindObjectOfType<CursorManagement>();
        gridManagement = FindObjectOfType<GridManagement>();
        gridDebugger = FindObjectOfType<GridDebugger>();
        storageBay = FindObjectOfType<StorageBay>();

        instance = this;
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
