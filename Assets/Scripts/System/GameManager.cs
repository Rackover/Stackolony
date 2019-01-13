using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Don't make any of those variables public under any circumstance
    bool inGame = false;
    bool isLoading = false;

    public Object menuScene;

    [Header("SYSTEM")]
    public Temporality temporality;
    public FlagReader flagReader;
    public Library library;
    public SFXManager sfxManager;
    public SystemManager systemManager;
    public CityManagement cityManagement;
    public MissionManager missionManager;
    public CursorManagement cursorManagement;
    public GridManagement gridManagement;
    public StorageBay storageBay;
    public PopulationManager populationManager;
    public Player player;
    public SaveManager saveManager;

    [Space(1)]
    [Header("INTERFACE")]
    public CursorDisplay cursorDisplay;
    public Localization localization;

    [Space(1)] [Header("INTERFACE INGAME")]
    public DeliveryManagement deliveryManagement;
    public TemporalityInterface temporalityInterface;
    public TooltipGO tooltipGO;
    public BlockInfobox blockInfobox;
    public ErrorDisplay errorDisplay;

    [Space(1)]
    [Header("DEBUG SETTINGS")]
    public Logger logger;
    public GridDebugger gridDebugger;
    public bool DEBUG_MODE = false;
    public bool ENABLE_LOGS = true;

    public static GameManager instance;

    void Awake()
    {
        if (instance == null) {
            DontDestroyOnLoad(this);
            instance = this;
        }
        else {
            Destroy(this.gameObject);
            return;
        }

        SceneManager.sceneLoaded += delegate { FindAllReferences(); };
        FindAllReferences();
    }

    private void Start()
    {
        if (menuScene.name != SceneManager.GetActiveScene().name) {
            StartGame();
        }
        else {
            EndGame();
        }
    }

    void Update()
    {
        CheckInputs();
    }


    IEnumerator LoadGameScene(System.Action then, bool keepLoading=false)
    {
        isLoading = true;
        AsyncOperation load = SceneManager.LoadSceneAsync("Game");
        while (!load.isDone) {
            yield return null;
        }
        then.Invoke();
        isLoading = !keepLoading;
        yield return true;
    }


    void FindAllReferences()
    {
        // SYSTEM
        if (temporality == null) temporality = GetComponentInChildren<Temporality>();
        if (flagReader == null) flagReader = GetComponentInChildren<FlagReader>();
        if (library == null) library = GetComponentInChildren<Library>();
        if (sfxManager == null) sfxManager = GetComponentInChildren<SFXManager>();
        if (systemManager == null) systemManager = GetComponentInChildren<SystemManager>();
        if (cityManagement == null) cityManagement = GetComponentInChildren<CityManagement>();
        if (missionManager == null) missionManager = GetComponentInChildren<MissionManager>();
        if (cursorManagement == null) cursorManagement = GetComponentInChildren<CursorManagement>();
        if (gridManagement == null) gridManagement = GetComponentInChildren<GridManagement>();
        if (storageBay == null) storageBay = GetComponentInChildren<StorageBay>();
        if (populationManager == null) populationManager = GetComponentInChildren<PopulationManager>();
        if (saveManager == null) saveManager = GetComponentInChildren<SaveManager>();

        // INTERFACE
        if (cursorDisplay == null) cursorDisplay = FindObjectOfType<CursorDisplay>();
        if (localization == null) localization = FindObjectOfType<Localization>();

        // INTERFACE - INGAME
        if (deliveryManagement == null) deliveryManagement = FindObjectOfType<DeliveryManagement>();
        if (temporalityInterface == null) temporalityInterface = FindObjectOfType<TemporalityInterface>();
        if (tooltipGO == null) tooltipGO = FindObjectOfType<TooltipGO>();
        if (blockInfobox == null) blockInfobox = FindObjectOfType<BlockInfobox>();
        if (errorDisplay == null) errorDisplay = FindObjectOfType<ErrorDisplay>();

        // DEBUG
        if (logger == null) logger = GetComponentInChildren<Logger>();
        if (gridDebugger == null) gridDebugger = FindObjectOfType<GridDebugger>();
    }

    void CheckInputs()
    {

        if (Input.GetKeyDown(KeyCode.P)) {
            temporality.PauseGame();
        }
        
        // Reset cursor mode
        if (Input.GetKeyDown(KeyCode.Escape)) {
            cursorManagement.SwitchMode(CursorManagement.cursorMode.Default);
        }

        // Reset camera
        if (Input.GetKeyDown(KeyCode.V)) {
            Camera.main.GetComponent<CameraController>().ResetPosition();
        }

        // Spawns and inhabits citizen
        if (Input.GetKeyDown(KeyCode.B)) {
            populationManager.AutoHouseCitizen(populationManager.SpawnCitizen(populationManager.populationTypeList[0]));
        }

        if (Input.GetButtonDown("Select") && Input.GetKey(KeyCode.F)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)) {
                Block block = hit.collider.gameObject.GetComponent<Block>();
                if (block != null) {
                    if (!block.states.Contains(BlockState.OnFire))
                        block.AddState(BlockState.OnFire);
                    else
                        block.RemoveState(BlockState.OnFire);
                }
            }
        }

        // Saves the game
        if (Input.GetKeyDown(KeyCode.M)) {
            StartCoroutine(saveManager.WriteSaveData(
                new SaveManager.SaveData(
                    new SaveManager.GameData(
                        deliveryManagement.shopDisplays,
                        gridManagement.grid,
                        gridManagement.bridgesList,
                        storageBay.storedBlocks,
                        player.name,
                        cityManagement.cityName,
                        temporality.cycleNumber,
                        temporality.cycleProgression
                    )
                )
            ));
        }
        
        // Goes forward in time by 1 cycle
        if (Input.GetKeyDown(KeyCode.C)) {
            temporality.AddCycle();
        }


    }


    public bool IsInGame()
    {
        return inGame;
    }

    public bool IsLoading()
    {
        return isLoading;
    }

    // Interface functions
    public void StartGame()
    {
        // Initialize and shut down
        storageBay.gameObject.GetComponent<MeshRenderer>().enabled = true;
        GameInterfaces gi = FindObjectOfType<GameInterfaces>();
        if (gi != null) {
            gi.gameObject.SetActive(true);
            gi.StartGameInterfaces();
        }
        cursorManagement.InitializeGameCursor();
        temporality.timeScale = 0;

        // Initialize only
        gridManagement.InitializeGridManager();

        // Ingame switch
        inGame = true;
    }

    public void EndGame()
    {
        // Initialize and shut down
        storageBay.gameObject.GetComponent<MeshRenderer>().enabled = false;
        GameInterfaces gi = FindObjectOfType<GameInterfaces>();
        if (gi != null) {
            gi.StopGameInterfaces();
            gi.gameObject.SetActive(false);
        }
        cursorManagement.KillGameCursor();
        temporality.timeScale = 2;

        // Ingame switch
        inGame = false;
    }


    public void NewGame()
    {
        StartCoroutine(LoadGameScene(delegate { StartGame(); }));
    }

    public void Load()
    {
        StartCoroutine(
            LoadGameScene(
                delegate {
                    StartGame();
                    saveManager.StartCoroutine(
                        saveManager.ReadSaveData(
                            cityManagement.cityName,
                            delegate {
                                saveManager.LoadSaveData(saveManager.loadedData);
                                isLoading = false;
                            }
                        )
                    );
                }
            )
        );
        
    }
    public void Exit()
    {
        Application.Quit();
    }
}
