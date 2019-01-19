using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Don't make any of those variables public under any circumstance
    bool inGame = false;
    bool isLoading = false;

    public string menuSceneName="Menu";

    [Header("SYSTEM")]
    public Temporality temporality;
    public FlagReader flagReader;
    public Library library;
    public SFXManager sfxManager;
    public SystemManager systemManager;
    public MissionCallbackManager missionCallbackManager;
    public CityManager cityManager;
    public MissionManager missionManager;
    public CursorManagement cursorManagement;
    public GridManagement gridManagement;
    public StorageBay storageBay;
    public PopulationManager populationManager;
    public Player player;
    public SaveManager saveManager;
    public CinematicManager cinematicManager;

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
            DestroyImmediate(this.gameObject);
            return;
        }

        SceneManager.sceneLoaded += delegate { FindAllReferences(); };
        FindAllReferences();
    }

    private void Start()
    {
        if (menuSceneName != SceneManager.GetActiveScene().name) {
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


    IEnumerator LoadGameScene(System.Action then, bool signalEnd=true)
    {
        isLoading = true;
        AsyncOperation load = SceneManager.LoadSceneAsync("Game");
        while (!load.isDone) {
            yield return null;
        }
        foreach(GameManager gm in FindObjectsOfType<GameManager>()) {
            if (gm != this) {
                DestroyImmediate(gm.gameObject);
            }
        }
        then.Invoke();
        isLoading = !signalEnd;
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
        if (missionCallbackManager == null) missionCallbackManager = GetComponentInChildren<MissionCallbackManager>();
        if (missionManager == null) missionManager = GetComponentInChildren<MissionManager>();
        if (cursorManagement == null) cursorManagement = GetComponentInChildren<CursorManagement>();
        if (gridManagement == null) gridManagement = GetComponentInChildren<GridManagement>();
        if (storageBay == null) storageBay = GetComponentInChildren<StorageBay>();
        if (populationManager == null) populationManager = GetComponentInChildren<PopulationManager>();
        if (saveManager == null) saveManager = GetComponentInChildren<SaveManager>();
        if (cinematicManager == null) cinematicManager = GetComponentInChildren<CinematicManager>();
        if (cityManager == null) cityManager = GetComponentInChildren<CityManager>();

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
            temporality.PauseTime();
        }
        
        // Pause
        if (Input.GetKeyDown(KeyCode.Escape) && IsInGame() && FindObjectsOfType<PauseWindow>().Length <= 0) {
            FindObjectOfType<Interface>().SpawnPauseWindow();
        }

        // Reset camera
        if (Input.GetKeyDown(KeyCode.V)) {
            Camera.main.GetComponent<CameraController>().ResetPosition();
        }
/*
        // Spawns and inhabits citizen
        if (Input.GetKeyDown(KeyCode.B)) {
            populationManager.AutoHouseCitizen(populationManager.SpawnCitizen(populationManager.populationTypeList[0]));
        }
*/
        // Spawns 5  cit
        if (Input.GetKeyDown(KeyCode.U)) {
            populationManager.SpawnCitizens(populationManager.populationTypeList[0], 5);
        }
        // Spawns 20 cit
        if (Input.GetKeyDown(KeyCode.L)) {
            populationManager.SpawnCitizens(populationManager.populationTypeList[0], 20);
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
                        new Vector3Int(), //storageBay.gridPosition,
                        storageBay.storedBlocks,
                        player.name,
                        cityManager.cityName,
                        temporality.cycleNumber,
                        temporality.GetCurrentCycleProgression()
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
    public void StartGame(bool newGame = true)
    {
        
        // Initialize and shut down
        storageBay.transform.Find("Visuals").GetComponent<MeshRenderer>().enabled = true;
        GameInterfaces gi = FindObjectOfType<GameInterfaces>();
        if (gi != null) {
            gi.gameObject.SetActive(true);
            gi.StartGameInterfaces();
        }
        cursorManagement.InitializeGameCursor();
        temporality.cycleNumber = 0;
        temporality.SetDate(0);
        temporality.SetTimeOfDay(20);
        temporality.SetTimeScale(1);

        // Initialize only
        gridManagement.InitializeGridManager();
        storageBay.Initialize();
        cinematicManager.GetReferences();

        // NEW GAME ONLY
        if(newGame)
        {
            // CINEMATIC
            Instantiate(library.spatioportSpawnerPrefab);
        }

        // Ingame switch
        inGame = true;
    }

    public void EndGame()
    {
        // Initialize and shut down
        storageBay.transform.Find("Visuals").GetComponent<MeshRenderer>().enabled = false;
        GameInterfaces gi = FindObjectOfType<GameInterfaces>();
        if (gi != null) {
            gi.StopGameInterfaces();
            gi.gameObject.SetActive(false);
        }
        cursorManagement.KillGameCursor();
        temporality.SetTimeOfDay(20);
        temporality.SetTimeScale(2);

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
                    StartGame(false);
                    saveManager.StartCoroutine(
                        saveManager.ReadSaveData(
                            cityManager.cityName,
                            delegate {
                                saveManager.LoadSaveData(saveManager.loadedData);
                                isLoading = false;
                            }
                        )
                    );
                },
                false
            )
        );

    }

    public void ExitToMenu()
    {
        EndGame();
        SceneManager.LoadScene(menuSceneName);
    }


    public void Exit()
    {
        Application.Quit();
    }
}
