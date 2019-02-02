using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Don't make any of those variables public under any circumstance
    bool inGame = false;
    bool isLoading = false;
    bool isPaused = false;

    // Used for pausing
    float oldTimescale = 0f;

    public string menuSceneName="Menu";

    [Header("SYSTEM")]
    public Temporality temporality;
    public FlagReader flagReader;
    public Library library;
    public SoundManager soundManager;
    public SystemManager systemManager;
    public MissionCallbackManager missionCallbackManager;
    public CityManager cityManager;
    public MissionManager missionManager;
    public CursorManagement cursorManagement;
    public GridManagement gridManagement;
    public PopulationManager populationManager;
    public Player player;
    public SaveManager saveManager;
    public CinematicManager cinematicManager;
    public BulletinsManager bulletinsManager;
    public TimelineController timelineController;

    [Space(1)]
    [Header("INTERFACE")]
    public CursorDisplay cursorDisplay;
    public Localization localization;
    public DisplayerManager displayerManager;

    [Space(1)] [Header("INTERFACE INGAME")]
    public TemporalityInterface temporalityInterface;
    public TooltipGO tooltipGO;

    [Space(1)]
    [Header("DEBUG SETTINGS")]
    public Logger logger;
    public OverlayManager overlayManager;
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
        if (IsInGame()) {
            CheckGameInputs();
        }
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
        if (soundManager == null) soundManager = GetComponentInChildren<SoundManager>();
        if (systemManager == null) systemManager = GetComponentInChildren<SystemManager>();
        if (missionCallbackManager == null) missionCallbackManager = GetComponentInChildren<MissionCallbackManager>();
        if (missionManager == null) missionManager = GetComponentInChildren<MissionManager>();
        if (cursorManagement == null) cursorManagement = GetComponentInChildren<CursorManagement>();
        if (gridManagement == null) gridManagement = GetComponentInChildren<GridManagement>();
        if (populationManager == null) populationManager = GetComponentInChildren<PopulationManager>();
        if (saveManager == null) saveManager = GetComponentInChildren<SaveManager>();
        if (cinematicManager == null) cinematicManager = GetComponentInChildren<CinematicManager>();
        if (cityManager == null) cityManager = GetComponentInChildren<CityManager>();
        if (bulletinsManager == null) bulletinsManager = GetComponentInChildren<BulletinsManager>();
        if (overlayManager == null) overlayManager = FindObjectOfType<OverlayManager>();
        if (timelineController == null) timelineController = GetComponentInChildren<TimelineController>();
        
        // INTERFACE
        if (cursorDisplay == null) cursorDisplay = FindObjectOfType<CursorDisplay>();
        if (localization == null) localization = GetComponentInChildren<Localization>();
        if (displayerManager == null) displayerManager = GetComponentInChildren<DisplayerManager>();

        // INTERFACE - INGAME
        if (temporalityInterface == null) temporalityInterface = FindObjectOfType<TemporalityInterface>();
        if (tooltipGO == null) tooltipGO = FindObjectOfType<TooltipGO>();

        // DEBUG
        if (logger == null) logger = GetComponentInChildren<Logger>();
    }

    void CheckInputs()
    {

    }

    void CheckGameInputs()
    {
		
        if (Input.GetKeyDown(KeyCode.F1))
        {
            overlayManager.SelectOverlay(OverlayType.Default);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            overlayManager.SelectOverlay(OverlayType.Type);
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            overlayManager.SelectOverlay(OverlayType.FireRisks);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            overlayManager.SelectOverlay(OverlayType.Food);
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            overlayManager.SelectOverlay(OverlayType.Habitation);
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            overlayManager.SelectOverlay(OverlayType.Power);
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            overlayManager.SelectOverlay(OverlayType.Density);
        }


        if (Input.GetKeyDown(KeyCode.End)) {
            temporality.timeScale = 100;
        }


        if (Input.GetKeyDown(KeyCode.N)) { 
            Notifications.Notification not = new Notifications.Notification(
                new string[] { "cannotBuild", "notLinked", "newPeople" }[Mathf.FloorToInt(Random.value * 3)], 
                new Color[]{ Color.red, Color.blue, Color.yellow, Color.Lerp(Color.red, Color.yellow, 0.5f)}[Mathf.FloorToInt(Random.value*4)]
            );
            FindObjectOfType<Notifications>().Notify(not);
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            temporality.PauseTime();
        }
        
        // Pause
        if (Input.GetKeyDown(KeyCode.Escape) && IsInGame() && FindObjectsOfType<PauseWindow>().Length <= 0) {
            FindObjectOfType<Interface>().SpawnPauseWindow();
        }

        // Reset camera
        if (Input.GetKeyDown(KeyCode.V)) {
            FindObjectOfType<CameraController>().ResetPosition();
        }
/*
        // Spawns and inhabits citizen
        if (Input.GetKeyDown(KeyCode.B)) {
            populationManager.AutoHouseCitizen(populationManager.SpawnCitizen(populationManager.populationTypeList[0]));
        }
*/
        // Spawns 5  cit
        if (Input.GetKeyDown(KeyCode.U)) {
            populationManager.SpawnCitizens(populationManager.populationTypeList[Mathf.FloorToInt(populationManager.populationTypeList.Length*Random.value)], 5);
        }
        // Spawns 20 cit
        if (Input.GetKeyDown(KeyCode.L)) {
            populationManager.SpawnCitizens(populationManager.populationTypeList[0], 20);
        }

        // Affect a block under the mouse
        if(Input.GetButtonDown("Select")) // LEFT MOUSE CLICK
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)) 
            {
                Block block = hit.collider.gameObject.GetComponent<Block>();
                if (block != null) 
                {
                    if(Input.GetKey(KeyCode.F))
                    {
                        if (!block.states.Contains(BlockState.OnFire))
                            block.AddState(BlockState.OnFire);
                        else
                            block.RemoveState(BlockState.OnFire);     
                    }

                    if(Input.GetKey(KeyCode.R))
                    {
                        if (!block.states.Contains(BlockState.OnRiot))
                            block.AddState(BlockState.OnRiot);
                        else
                            block.RemoveState(BlockState.OnRiot);     
                    }

                    if(Input.GetKey(KeyCode.D))
                    {
                        if (!block.states.Contains(BlockState.Damaged))
                            block.AddState(BlockState.Damaged);
                        else
                            block.RemoveState(BlockState.Damaged);     
                    }
                }
            }
        }

        // Saves the game
        if (Input.GetKeyDown(KeyCode.M)) {
            StartCoroutine(saveManager.WriteSaveData(
                new SaveManager.SaveData(
                    new SaveManager.GameData(
                        gridManagement.grid,
                        gridManagement.bridgesList,
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

    public void Pause()
    {
        if (IsInGame()) {
            oldTimescale = temporality.timeScale;
            temporality.SetTimeScale(0);
            isPaused = true;
        }
    }
    
    public void UnPause()
    {
        if (IsInGame()) {
            temporality.SetTimeScale(Mathf.FloorToInt(oldTimescale));
            isPaused = false;
        }
    }

    public bool IsPaused()
    {
        return IsInGame() && isPaused;
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
        cinematicManager.GetReferences();
        FindObjectOfType<OverlayDisplayer>().Initialize();
        timelineController.LoadCycles();

        // NEW GAME ONLY
        if (newGame)
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
