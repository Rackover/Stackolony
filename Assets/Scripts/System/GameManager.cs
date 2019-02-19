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
    bool isNewGame = true;

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
    public EventManager eventManager;
    public AnimationManager animationManager;
    public AchievementManager achievementManager;

    [Space(1)]
    [Header("INTERFACE")]
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
    public bool DISABLE_EVENTS = false;
    public bool DISABLE_GAME_OVER = false;

    public static GameManager instance;

    void Awake()
    {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(this.gameObject);
            return;
        }
        
        SceneManager.sceneLoaded += delegate { FindAllReferences(); };
        SceneManager.sceneLoaded += delegate {
            if (menuSceneName != SceneManager.GetActiveScene().name) {
                StartGame();
            }
            else {
                EndGame();
            }
        };
    }

    void Update()
    {
        if (DEBUG_MODE) {
            foreach(UnityEngine.UI.InputField i in FindObjectsOfType<UnityEngine.UI.InputField>()) {
                if (i.isFocused) {
                    return;
                }
            }
            if (IsInGame()) {
                CheckDebugGameInputs();
            }
            CheckDebugInputs();
        }
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
        if (temporality == null) temporality = FindObjectOfType<Temporality>();
        if (flagReader == null) flagReader = FindObjectOfType<FlagReader>();
        if (library == null) library = FindObjectOfType<Library>();
        if (soundManager == null) soundManager = FindObjectOfType<SoundManager>();
        if (systemManager == null) systemManager = FindObjectOfType<SystemManager>();
        if (missionCallbackManager == null) missionCallbackManager = FindObjectOfType<MissionCallbackManager>();
        if (missionManager == null) missionManager = FindObjectOfType<MissionManager>();
        if (cursorManagement == null) cursorManagement = FindObjectOfType<CursorManagement>();
        if (gridManagement == null) gridManagement = FindObjectOfType<GridManagement>();
        if (populationManager == null) populationManager = FindObjectOfType<PopulationManager>();
        if (saveManager == null) saveManager = FindObjectOfType<SaveManager>();
        if (cinematicManager == null) cinematicManager = FindObjectOfType<CinematicManager>();
        if (cityManager == null) cityManager = FindObjectOfType<CityManager>();
        if (bulletinsManager == null) bulletinsManager = FindObjectOfType<BulletinsManager>();
        if (overlayManager == null) overlayManager = FindObjectOfType<OverlayManager>();
        if (timelineController == null) timelineController = FindObjectOfType<TimelineController>();
        if (player == null) player = FindObjectOfType<Player>();
        if (eventManager == null) eventManager = FindObjectOfType<EventManager>();
        if (animationManager == null) animationManager = FindObjectOfType<AnimationManager>();
        if (achievementManager == null) achievementManager = FindObjectOfType<AchievementManager>();
        
        // INTERFACE
        if (localization == null) localization = FindObjectOfType<Localization>();
        if (displayerManager == null) displayerManager = FindObjectOfType<DisplayerManager>();

        // INTERFACE - INGAME
        if (temporalityInterface == null) temporalityInterface = FindObjectOfType<TemporalityInterface>();
        if (tooltipGO == null) tooltipGO = FindObjectOfType<TooltipGO>();

        // DEBUG
        if (logger == null) logger = FindObjectOfType<Logger>();
    }

    void CheckDebugInputs()
    {

    }

    void CheckDebugGameInputs()
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

        if (Input.GetKeyDown(KeyCode.F12)) {
            FindObjectOfType<DebugInterface>().SpawnEventDebugWindow();
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

        if (Input.GetKeyDown(KeyCode.Delete)) {
            eventManager.TriggerEvent(1000);
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
            eventManager.TriggerEvent(15);
        }

        // Pause
        if (Input.GetKeyDown(KeyCode.Escape) && IsInGame() && FindObjectsOfType<PauseWindow>().Length <= 0) {
            FindObjectOfType<Interface>().SpawnPauseWindow();
        }

        // Reset camera
        if (Input.GetKeyDown(KeyCode.V)) {
            FindObjectOfType<CameraController>().ResetPosition();
        }

        // Unlock all
        if (Input.GetKeyDown(KeyCode.T)) {
            foreach(BlockScheme s in library.blocks) {
                cityManager.UnlockBuilding(s.ID);
            }
        }

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
                        if (!block.states.ContainsKey(State.OnFire))
                            FireManager.Ignite(block);
                        else
                            block.RemoveState(State.OnFire);     
                    }

                    if(Input.GetKey(KeyCode.R))
                    {
                        if (!block.states.ContainsKey(State.OnRiot))
                            block.AddState(State.OnRiot);
                        else
                            block.RemoveState(State.OnRiot);     
                    }

                    if(Input.GetKey(KeyCode.D))
                    {
                        if (!block.states.ContainsKey(State.Damaged))
                            block.AddState(State.Damaged);
                        else
                            block.RemoveState(State.Damaged);     
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

        
        if (Input.GetKeyDown(KeyCode.G)) {
            animationManager.EndElevateTower(new Vector2Int(cursorManagement.posInGrid.x, cursorManagement.posInGrid.z));
        }

        // Goes forward in time by 1 cycle
        if (Input.GetKeyDown(KeyCode.C)) 
        {
            temporality.AddCycle();
        }

        if (Input.GetKeyDown(KeyCode.K)) {
            eventManager.LoadEvents();
        }
        
        if (Input.GetKeyDown(KeyCode.B)) 
        {
            temporality.AddMicroCycle();
        }

        if (Input.GetKeyDown(KeyCode.W)) {
            populationManager.GenerateMoodModifier(populationManager.populationTypeList[0], 100, 10, 1000);
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
    void StartGame()
    {
        // Initialize and shut down
        GameInterfaces gi = FindObjectOfType<GameInterfaces>();
        if (gi != null) {
            gi.gameObject.SetActive(true);
            gi.StartGameInterfaces();
        }
        cursorManagement.InitializeGameCursor();

        temporality.SetDate(0);
        temporality.SetTimeOfDay(20);
        temporality.SetTimeScale(1);        

        // Initialize only
        gridManagement.InitializeGridManager();
        cinematicManager.GetReferences();
        timelineController.LoadCycles();
        DifferStart(delegate { eventManager.LoadEvents(); });

        cityManager.GenerateEnvironmentBlocks();

        // TUTORIAL RUN ONLY
        if (cityManager.isTutorialRun) {

            // Lock every building
            foreach (BlockScheme scheme in library.blocks) {
                cityManager.LockBuilding(scheme.ID);
            }
        }

        // NEW GAME ONLY
        if (isNewGame) {

            // First citizen arrival and cycle loading
            timelineController.UpdateCycle(0);

            // CINEMATIC
            Instantiate(library.spatioportSpawnerPrefab);
        }

        // Ingame switch
        inGame = true;
    }

    void EndGame()
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

        // Clear events
        eventManager.ClearListeners();
        cursorManagement.ClearListeners();
        achievementManager.ClearListeners();

        // Clear system
        systemManager.ClearSystem();

        // Remove gameevents and citizens
        populationManager.Clear();
        eventManager.ResetChances();

        // Shut down only
        displayerManager.UnstageAll();

        // Ingame switch
        inGame = false;
    }

    void DifferStart(System.Action action)
    {
        StartCoroutine(ExecuteAfterFrame(action));
    }

    IEnumerator ExecuteAfterFrame(System.Action action)
    {
        yield return new WaitForEndOfFrame();
        action.Invoke();
        yield return true;
    }

    public void NewGame()
    {
        isNewGame = true;
        StartCoroutine(LoadGameScene(delegate { }));
    }

    public void Load()
    {
        isNewGame = false;
        StartCoroutine(
            LoadGameScene(
                delegate {
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
        SceneManager.LoadScene(menuSceneName);
    }


    public void Exit()
    {
        Application.Quit();
    }
}
