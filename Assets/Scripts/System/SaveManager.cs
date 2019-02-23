using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;

public class SaveManager : MonoBehaviour {

    int saveVersion = 4;

    public SaveData loadedData;


    public bool SaveExists(string cityName = null)
    {
        // Returns true if any save exists
        if (cityName == null) {
            return Directory.GetFiles(Paths.GetSaveFolder()).Length > 0;
        }

        // returns true if the specific save exists
        return File.Exists(Paths.GetSaveFile(cityName));
    }

    public void DestroySave(string cityName = null)
    {
        // Deletes all the saves
        if (cityName == null) {
            foreach (string file in Directory.GetFiles(Paths.GetSaveFolder())) {
                File.Delete(file);
            }
            return;
        }

        // Deletes specified save
        File.Delete(Paths.GetSaveFile(cityName));
    }

    ///////////////////////////////
    /// 
    ///     Main function to write savegame
    ///     This is the one you call publicly
    ///

    public IEnumerator WriteSaveData(SaveData saveData, Action callback)
    {
        double timeStart = Time.time;
        yield return null;

        // Step 0 - Initialize writer
        Writer writer = new Writer();

        if(!writer.OpenSave(Paths.GetSaveFile(GameManager.instance.cityManager.cityName)))
            yield break;
        
        Logger.Info("Saving...");

        // Step 1 - Miscellaneous data
        Logger.Debug("Saving misc data...");
        writer.WriteUInt8(saveVersion);
        writer.WriteString(saveData.cityName);
        writer.WriteString(saveData.playerName);
        writer.WriteFloat(saveData.timeOfDay);
        writer.WriteUInt16(saveData.cyclesPassed);

        // Step 2 - Main grid
        Logger.Debug("Saving main game grid..");
        writer.WriteVector3UInt8(saveData.gridSize);
        writer.WriteUInt16(saveData.blockGrid.Count);
        foreach (KeyValuePair<Vector3Int, BlockSaveData> data in saveData.blockGrid) {
            BlockSaveData blockData = data.Value;
            writer.WriteVector3UInt8(data.Key);
            writer.WriteUInt8(blockData.id);
            writer.WriteUInt8(blockData.states.Count);
            Logger.Debug("Saved blockData ID#" + blockData.id + " at position " + data.Key.ToString());
            foreach (int state in blockData.states) {
                writer.WriteUInt8(state);
            }
            yield return null;
        }

        // Step 3 - Bridges
        Logger.Debug("Saving "+ saveData.bridges.Count+ " bridges...");
        writer.WriteUInt8(saveData.bridges.Count);
        foreach (KeyValuePair<Vector3Int, Vector3Int> bridge in saveData.bridges) {
            writer.WriteVector3UInt8(bridge.Key);
            writer.WriteVector3UInt8(bridge.Value);
            yield return null;
        }

        // Step 4 - Populations
        Logger.Debug("Saving " + saveData.popSaveData.Count + " populations...");
        writer.WriteUInt8(saveData.popSaveData.Count);
        foreach(PopulationSaveData popSD in saveData.popSaveData) {
            writer.WriteUInt8(popSD.popId);
            writer.WriteFloat(popSD.riotRisk);
            writer.WriteFloat(popSD.averageMood);
            writer.WriteUInt8(popSD.moodModifiers.Count);
            foreach (MoodModifier moodMod in popSD.moodModifiers) {
                writer.WriteFloat(moodMod.amount);
                writer.WriteUInt8(moodMod.cyclesRemaining);
                writer.WriteUInt16(moodMod.eventId);
            }

            writer.WriteUInt8(popSD.foodModifiers.Count);
            foreach (FoodModifier foodMod in popSD.foodModifiers) {
                writer.WriteFloat(foodMod.amount);
                writer.WriteUInt8(foodMod.cyclesRemaining);
            }

            writer.WriteUInt16(popSD.citizens.Count);
            foreach (CitizenSaveData cit in popSD.citizens) {
                writer.WriteString(cit.name);
            }
        }

        // Step 5 - Locks
        Logger.Debug("Saving " + saveData.lockedBuildings.Count + " locks...");
        writer.WriteUInt8(saveData.lockedBuildings.Count);
        foreach (int buildingId in saveData.lockedBuildings) {
            writer.WriteUInt8(buildingId);
            yield return null;
        }

        // Last step - Close handler;
        writer.Close();
        Logger.Info("Done in "+(Time.time-timeStart).ToString("n2")+" seconds");

        if (callback != null) {
            callback.Invoke();
        }
        yield return true;
    }
    
    ///
    ///
    //////////////////////////////
    

    public IEnumerator ReadSaveData(string cityName, Action callback = null)
    {
        double timeStart = Time.time;
        yield return null;

        // Step 0 - Initialize reader
        Reader reader = new Reader();
        
        if(!reader.OpenSave(Paths.GetSaveFile(cityName))) {yield break;}
        
        Logger.Info("Reading...");
        SaveData diskSaveData = new SaveData();

        // Step 1 - Miscellaneous data
        Logger.Debug("Reading headers...");
        int version = reader.ReadUInt8();
        if (version != saveVersion) {
            Logger.Warn("Expected version "+saveVersion.ToString()+", got version "+version.ToString());
        }

        diskSaveData.cityName = reader.ReadString();
        diskSaveData.playerName = reader.ReadString();
        diskSaveData.timeOfDay = reader.ReadFloat();
        diskSaveData.cyclesPassed = reader.ReadUInt16();

        // Step 2 - Main grid
        Logger.Debug("Reading grid...");
        diskSaveData.gridSize = reader.ReadVector3UInt8();
        diskSaveData.blockGrid = new Dictionary<Vector3Int, BlockSaveData>();
        int count = reader.ReadUInt16();
        for (int i = 0; i < count; i++) {
            BlockSaveData blockData = new BlockSaveData();
            Vector3Int position = reader.ReadVector3UInt8();
            blockData.id = reader.ReadUInt8();
            int statesCount = reader.ReadUInt8();
            blockData.states = new List<int>();
            for (int j = 0; j < statesCount; j++) {
                blockData.states.Add(reader.ReadUInt8());
            }
            Logger.Debug("Reading block id : " + blockData.id + ":"+blockData.states.Count.ToString()+" at position " + position.ToString());
            diskSaveData.blockGrid[position] = blockData;
            yield return null;
        }

        // Step 3 - Bridges
        int bridgesCount = reader.ReadUInt8();
        Logger.Debug("Reading "+ bridgesCount + " bridges...");
        diskSaveData.bridges = new List<KeyValuePair<Vector3Int, Vector3Int>>();
        for (int i = 0; i < bridgesCount; i++) {
            diskSaveData.bridges.Add(new KeyValuePair<Vector3Int, Vector3Int>(reader.ReadVector3UInt8(), reader.ReadVector3UInt8()));
            yield return null;
        }

        // Step 4 - Population
        int popCount = reader.ReadUInt8();
        Logger.Debug("Reading "+ popCount + " populations...");
        diskSaveData.popSaveData = new List<PopulationSaveData> ();
        for (int i = 0; i < popCount; i++) {
            int popId = reader.ReadUInt8();
            float riotRisk = reader.ReadFloat();
            float mood = reader.ReadFloat();
            int moodModCount = reader.ReadUInt8();
            
            List<MoodModifier> mods = new List<MoodModifier>();
            for (int j = 0; j < moodModCount; j++) {
                mods.Add(new MoodModifier() {
                    amount = reader.ReadFloat(),
                    cyclesRemaining = reader.ReadUInt8(),
                    eventId = reader.ReadUInt16(),
                });
            }

            int foodModCount = reader.ReadUInt8();
            List<FoodModifier> foods = new List<FoodModifier>();
            for (int j = 0; j < foodModCount; j++) {
                foods.Add(new FoodModifier() {
                    amount = reader.ReadFloat(),
                    cyclesRemaining = reader.ReadUInt8(),
                });
            }

            int citizenCount = reader.ReadUInt16();
            List<CitizenSaveData> cits = new List<CitizenSaveData>();
            for (int j = 0; j < citizenCount; j++) {
                cits.Add(new CitizenSaveData() { name = reader.ReadString() });
            }

            diskSaveData.popSaveData.Add(new PopulationSaveData() {
                popId = popId,
                riotRisk = riotRisk,
                averageMood = mood,
                moodModifiers = mods,
                foodModifiers = foods,
                citizens = cits
            });
        }

        // Step 5 - Locks
        int locksCount = reader.ReadUInt8();
        Logger.Debug("Reading " + locksCount + " locks...");
        diskSaveData.lockedBuildings = new List<int>();
        for (int i = 0; i < locksCount; i++) {
            diskSaveData.lockedBuildings.Add(reader.ReadUInt8());
            yield return null;
        }

        // Last step - Close handler;
        Logger.Debug("Closing handler...");
        reader.Close();
        Logger.Info("Done in "+(Time.time-timeStart).ToString("n2")+" seconds");

        loadedData = diskSaveData;
        if(callback != null) callback();

        yield return true;
    }


    ///////////////////////////////
    /// 
    ///     Main function to load savegame
    ///     This is the one you call publicly
    ///
    public void LoadSaveData(SaveData saveData, Action callback=null)
    {
        Logger.Debug("Loading save data");

        try {
            // Loading misc
            GameManager.instance.player.playerName = saveData.playerName;
            GameManager.instance.cityManager.cityName = saveData.cityName;
            GameManager.instance.temporality.SetDate(saveData.cyclesPassed);
            GameManager.instance.temporality.SetTimeOfDay(saveData.timeOfDay);

            GridManagement gridMan = GameManager.instance.gridManagement;

            // Loading grid
            Logger.Debug("Loading " + saveData.blockGrid.Count + " objects into the grid");
            foreach (KeyValuePair<Vector3Int, BlockSaveData> blockData in saveData.blockGrid) {
                // Todo : Load states aswell
                Vector3Int coords = blockData.Key;
                Logger.Debug("Spawning block #" + blockData.Value.id + " at position " + blockData.Key.ToString());
                GameObject spawnedBlock = gridMan.SpawnBlock(blockData.Value.id, blockData.Key);
                spawnedBlock.GetComponent<Block>().container.closed = false;
            }

            // Converting bridges list
            foreach (KeyValuePair<Vector3Int, Vector3Int> bridge in saveData.bridges) {
                Block origin = gridMan.grid[bridge.Key.x, bridge.Key.y, bridge.Key.z].GetComponent<Block>();
                Block destination = gridMan.grid[bridge.Value.x, bridge.Value.y, bridge.Value.z].GetComponent<Block>();
                if (gridMan.CreateBridge(origin, destination) == null) { Logger.Warn("Could not replicate bridge from " + origin + ":(" + bridge.Key + ") to " + destination + ":(" + bridge.Value + ")"); };
            }

            // Loading population
            PopulationManager popMan = GameManager.instance.populationManager;
            popMan.populations.Clear();
            foreach (PopulationSaveData data in saveData.popSaveData) {
                Population pop = popMan.GetPopulationByID(data.popId);
                if (pop == null) {
                    Logger.Error("Error while loading population from savegame : [" + data.popId + "] is not a valid POP id");
                }
                List<PopulationManager.Citizen> citizens = new List<PopulationManager.Citizen>();
                foreach(CitizenSaveData cSD in data.citizens) {
                    citizens.Add(new PopulationManager.Citizen() {
                        name = name,
                        type = pop
                    });
                }

                popMan.populations[pop] = new PopulationManager.PopulationInformation() {
                    moodModifiers = data.moodModifiers,
                    foodModifiers = data.foodModifiers,
                    riotRisk = data.riotRisk,
                    averageMood = data.averageMood,
                    citizens=citizens
                };
            }

            // Loading unlocks
            CityManager city = GameManager.instance.cityManager;
            city.ClearLocks();
            foreach(int id in saveData.lockedBuildings) {
                city.LockBuilding(id);
            }

            // End of loading
            if (callback != null) {
                callback.Invoke();
            }
        }

        catch(Exception e) {
            Logger.Error("Could not load the savegame : \n" + e.ToString()+"\n Going back to the main menu instead.");
            DestroySave();
            GameManager.instance.ExitToMenu();
        }

    }

    ///
    ///
    //////////////////////////////


    public class GameData
    {
        public GameObject[,,] grid;
        public List<GameObject> bridgesList;
        public string playerName;
        public string cityName;
        public int cycleNumber;
        public float cycleProgression;
        public Dictionary<Population, PopulationManager.PopulationInformation> populations;
        public List<int> lockedBuildings;

        public GameData(
            GameObject[,,] _grid,
            List<GameObject> _bridgesList,
            string _playerName,
            string _cityName,
            int _cycleNumber,
            float _cycleProgression,
            Dictionary<Population, PopulationManager.PopulationInformation> _populations,
            List<int> _lockedBuildings
        )
        {
            grid = _grid;
            bridgesList = _bridgesList;
            playerName = _playerName;
            cityName = _cityName;
            cycleNumber = _cycleNumber;
            cycleProgression = _cycleProgression;
            populations = _populations;
            lockedBuildings = _lockedBuildings;
        }
    }

    public class SaveData
    {
        public Vector3Int gridSize;
        public Dictionary<Vector3Int, BlockSaveData> blockGrid;
        public List<KeyValuePair<Vector3Int,Vector3Int>> bridges;
        public List<PopulationSaveData> popSaveData;
        public List<int> lockedBuildings;

        public float timeOfDay;
        public int cyclesPassed;

        public string playerName;
        public string cityName;

        // Empty saveData
        public SaveData() { }
            
        // New saveData from game values
        public SaveData(GameData gameData)
        {
            // Storing grid
            gridSize = new Vector3Int(gameData.grid.GetLength(0), gameData.grid.GetLength(1), gameData.grid.GetLength(2));
            blockGrid = ConvertBlocksGrid(gameData.grid, gridSize);

            // Storing bridges
            bridges = ConvertBridges(gameData.bridgesList);

            // Population informations
            popSaveData = ConvertPopulationInformations(gameData.populations);

            // Unlocks
            lockedBuildings = gameData.lockedBuildings;

            // Other data
            playerName = gameData.playerName;
            cityName = gameData.cityName;
            cyclesPassed = gameData.cycleNumber;
            timeOfDay = gameData.cycleProgression;
        }

        // Reading savedata from files
        public SaveData(
            Vector3Int _gridSize,
            Dictionary<Vector3Int, BlockSaveData> _blockGrid,
            List<KeyValuePair<Vector3Int, Vector3Int>> _bridges,
            float _timeOfDay,
            int _cyclesPassed,
            string _playerName,
            string _cityName,
            List<PopulationSaveData> _popSaveData,
            List<int> _lockedBuildings
        )
        {
            gridSize = _gridSize;
            blockGrid = _blockGrid;
            bridges = _bridges;
            timeOfDay = _timeOfDay;
            cyclesPassed = _cyclesPassed;
            playerName = _playerName;
            cityName = _cityName;
            popSaveData = _popSaveData;
            lockedBuildings = _lockedBuildings;
        }
        
        List<PopulationSaveData> ConvertPopulationInformations(Dictionary<Population, PopulationManager.PopulationInformation> populations)
        {
            List<PopulationSaveData> datas = new List<PopulationSaveData>();
            foreach(Population pop in populations.Keys) {
                PopulationSaveData data = new PopulationSaveData(pop, populations[pop]);
                datas.Add(data);
            }
            return datas;
        }

        Dictionary<Vector3Int, BlockSaveData> ConvertBlocksGrid(GameObject[,,] _grid, Vector3Int gridSize)
        {
            Dictionary<Vector3Int, BlockSaveData> blockGrid = new Dictionary<Vector3Int, BlockSaveData>();

            foreach (GameObject building in _grid) {
                BlockSaveData blockData = new BlockSaveData();
                if (building != null)
                {
                    Block blockLink = building.GetComponent<Block>();
                    if(blockLink != null)
                    {
                        Vector3Int coords = blockLink.gridCoordinates;
                        blockData.id = blockLink.scheme.ID;
                        blockData.states = new List<int>();

                        
                        foreach (KeyValuePair<State, StateBehavior> state in blockLink.states) 
                        {
                            blockData.states.Add((int)state.Key);
                        }
    
                        blockGrid[coords] = blockData;
                    }
                }
            }
            return blockGrid;
        }

        List<KeyValuePair<Vector3Int, Vector3Int>> ConvertBridges(List<GameObject> _bridgesList)
        {
            List<KeyValuePair<Vector3Int, Vector3Int>> list = new List<KeyValuePair<Vector3Int, Vector3Int>>();
            foreach (GameObject bridge in _bridgesList) {
                BridgeInfo info = bridge.GetComponent<BridgeInfo>();
                KeyValuePair<Vector3Int, Vector3Int> pair = new KeyValuePair<Vector3Int, Vector3Int>(info.origin, info.destination);
                list.Add(pair);
            }
            return list;
        }
    }

    public class BlockSaveData
    {
        public int id;
        public List<int> states;

        public BlockSaveData()
        {
            id = 0;
            states = new List<int>();
        }
    }

    public class CitizenSaveData
    {
        public string name;
    }

    public class PopulationSaveData
    {
        public List<CitizenSaveData> citizens;
        public int popId;
        public float riotRisk;
        public float averageMood;
        public List<MoodModifier> moodModifiers;
        public List<FoodModifier> foodModifiers;

        public PopulationSaveData(Population pop, PopulationManager.PopulationInformation information)
        {
            citizens = new List<CitizenSaveData>();

            popId = pop.ID;
            riotRisk = information.riotRisk;
            averageMood = information.averageMood;
            foreach(var cit in information.citizens) {
                citizens.Add(new CitizenSaveData() { name = cit.name });
            }
            moodModifiers = information.moodModifiers;
            foodModifiers = information.foodModifiers;
        }

        public PopulationSaveData() { }
    }

    class Reader
    {
        private BinaryReader br;

        public bool OpenSave(string path)
        {
            try {
                if(File.Exists(path))
                {
                    br = new BinaryReader(new FileStream(path, FileMode.Open));
                    return true;
                }
               return false;
                
            }
            catch (IOException e) {
                Logger.Error(e.Message + "\n Cannot read file "+ path);
                return false;
            }
        }

        public int ReadUInt16() { try { return br.ReadUInt16(); } catch (IOException e) { Logger.Error(e.Message + "\n Cannot read integer from file."); return 0;  } }
        public int ReadUInt8() { try { return br.ReadByte(); } catch (IOException e) { Logger.Error(e.Message + "\n Cannot read uint8 from file."); return 0; } }
        public string ReadString() { try { return br.ReadString(); } catch (IOException e) { Logger.Error(e.Message + "\n Cannot read string from file."); return null; } }
        public bool ReadBool() { try { return br.ReadBoolean(); } catch (IOException e) { Logger.Error(e.Message + "\n Cannot read bool from file."); return false; } }
        public float ReadFloat() { try { return (float)br.ReadDouble(); } catch (IOException e) { Logger.Error(e.Message + "\n Cannot read float from file."); return 0f; } }
        public Vector3Int ReadVector3UInt8() { try { return new Vector3Int(br.ReadByte(), br.ReadByte(), br.ReadByte()); } catch (IOException e) { Logger.Error(e.Message + "\n Cannot read v3uint8 from file."); return new Vector3Int(); } }

        public void Close()
        {
            br.Close();
        }
    }

    class Writer
    {
        BinaryWriter bw;

        public bool OpenSave(string path)
        {
            try 
            {
                
                if(File.Exists(path)){
                    if(File.Exists(path + ".backup")){
                        File.Delete(path + ".backup");
                    }
                    File.Copy(path, path + ".backup");
                }

                bw = new BinaryWriter(new FileStream(path, FileMode.Create));
                return true;
            }
            catch (IOException e) 
            {
                Logger.Error(e.Message + "\n Cannot create file "+ path);
                return false;
            }
        }
        
        public bool WriteBool(bool val)
        {
            try {
                bw.Write(val);
                return true;
            }
            catch (IOException e) {
                Logger.Error(e.Message + "\n Cannot write " + val.ToString() + " to file.");
                return false;
            }
        }

        public bool WriteString(string val)
        {
            try {
                bw.Write(val);
                return true;
            }
            catch (IOException e) {
                Logger.Error(e.Message + "\n Cannot write " + val.ToString() + " to file.");
                return false;
            }
        }

        public bool WriteFloat(float val)
        {
            try {
                bw.Write((double)val);
                return true;
            }
            catch (IOException e) {
                Logger.Error(e.Message + "\n Cannot write " + val.ToString() + " to file.");
                return false;
            }
        }

        public bool WriteUInt16(int val)
        {
            try {
                bw.Write(Convert.ToUInt16(val));
                return true;
            }
            catch (IOException e) {
                Logger.Error(e.Message + "\n Cannot write " + val.ToString() + " to file.");
                return false;
            }
        }

        public bool WriteUInt8(int val)
        {
            try {
                bw.Write(Convert.ToByte(val));
                return true;
            }
            catch (IOException e) {
                Logger.Error(e.Message + "\n Cannot write " + val.ToString() + " to file.");
                return false;
            }
        }

        public bool WriteVector3UInt8(Vector3Int val)
        {
            return WriteUInt8(val.x) && WriteUInt8(val.y) && WriteUInt8(val.z);
        }

        public void Close()
        {
            bw.Close();
        }
    }
    
}   


