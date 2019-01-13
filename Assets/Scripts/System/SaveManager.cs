﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class SaveManager : MonoBehaviour {

    public int saveVersion = 2;
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

    public IEnumerator WriteSaveData(SaveData saveData)
    {
        double timeStart = Time.time;
        yield return null;

        // Step 0 - Initialize writer
        Writer writer = new Writer();

        if(!writer.OpenSave(Paths.GetSaveFile(GameManager.instance.cityManagement.cityName)))
            yield break;
        
        Logger.Info("Saving...");

        // Step 1 - Miscellaneous data
        writer.WriteUInt8(saveVersion);
        writer.WriteString(saveData.cityName);
        writer.WriteString(saveData.playerName);
        writer.WriteFloat(saveData.timeOfDay);
        writer.WriteUInt16(saveData.cyclesPassed);

        // Step 2 - Main grid
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

        // Step 3 - Storage bay grid
        writer.WriteVector3UInt8(saveData.storagePosition);
        writer.WriteVector3UInt8(saveData.storageGridSize);
        writer.WriteUInt16(saveData.storageGrid.Count);
        foreach (KeyValuePair <Vector3Int, int> data in saveData.storageGrid) {
            writer.WriteVector3UInt8(data.Key);
            writer.WriteUInt8(data.Value);
            yield return null;
        }

        // Step 4 - Bridges
        writer.WriteUInt8(saveData.bridges.Count);
        foreach (KeyValuePair<Vector3Int, Vector3Int> bridge in saveData.bridges) {
            writer.WriteVector3UInt8(bridge.Key);
            writer.WriteVector3UInt8(bridge.Value);
            yield return null;
        }

        // Step 5 - Command
        writer.WriteUInt8(saveData.command.Count);
        foreach (KeyValuePair<int, int> commandElement in saveData.command) {
            writer.WriteUInt8(commandElement.Key);
            writer.WriteUInt8(commandElement.Value);
            yield return null;
        }

        // Last step - Close handler;
        writer.Close();
        Logger.Info("Done in "+(Time.time-timeStart).ToString("n2")+" seconds");
        yield return true;
    }
    
    ///
    ///
    //////////////////////////////
    

    public IEnumerator ReadSaveData(string cityName, System.Action callback = null)
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
        Vector3Int gridSize = reader.ReadVector3UInt8();
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

        // Step 3 - Storage bay grid
        Logger.Debug("Reading storage bay...");
        Vector3Int storageGridPosition = reader.ReadVector3UInt8();
        Vector3Int storageGridSize = reader.ReadVector3UInt8();
        diskSaveData.storageGrid = new Dictionary<Vector3Int, int>();
        int storedCount = reader.ReadUInt16();
        for (int i = 0; i < storedCount; i++) {
            diskSaveData.storageGrid[reader.ReadVector3UInt8()] = reader.ReadUInt8();
            yield return null;
        }

        // Step 4 - Bridges
        Logger.Debug("Reading bridges...");
        int bridgesCount = reader.ReadUInt8();
        diskSaveData.bridges = new List<KeyValuePair<Vector3Int, Vector3Int>>();
        for (int i = 0; i < bridgesCount; i++) {
            diskSaveData.bridges.Add(new KeyValuePair<Vector3Int, Vector3Int>(reader.ReadVector3UInt8(), reader.ReadVector3UInt8()));
            yield return null;
        }

        // Step 5 - Command
        Logger.Debug("Writing command...");
        diskSaveData.command = new Dictionary<int, int>();
        int commandCount = reader.ReadUInt8();
        for(int i=0; i < commandCount; i++) {
            diskSaveData.command[reader.ReadUInt8()] = reader.ReadUInt8();
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
    public void LoadSaveData(SaveData saveData)
    {
        Logger.Debug("Loading save data");

        try {
            GridManagement gridMan = GameManager.instance.gridManagement;

            // Loading shopping List
            List<ShopDisplay> shoppingList = new List<ShopDisplay>();
            foreach (KeyValuePair<int, int> element in saveData.command) {
                ShopDisplay item = new ShopDisplay();
                item.myBlock = GameManager.instance.library.GetBlockByID(element.Key);
                if (item.myBlock == null) { Logger.Warn("BlockScheme index not found - index : " + element.Key.ToString()); return; }
                item.quantityPicked = element.Value;
                shoppingList.Add(item);
            }
            GameManager.instance.deliveryManagement.LoadShop(shoppingList);

            // Loading grid
            Logger.Debug("Loading " + saveData.blockGrid.Count + " objects into the grid");
            foreach (KeyValuePair<Vector3Int, BlockSaveData> blockData in saveData.blockGrid) {
                // Todo : Load states aswell
                Vector3Int coords = blockData.Key;
                Logger.Debug("Spawning block #" + blockData.Value.id + " at position " + blockData.Key.ToString());
                gridMan.SpawnBlock(blockData.Value.id, blockData.Key);
            }

            // Converting bridges list
            foreach (KeyValuePair<Vector3Int, Vector3Int> bridge in saveData.bridges) {
                Block origin = gridMan.grid[bridge.Key.x, bridge.Key.y, bridge.Key.z].GetComponent<Block>();
                Block destination = gridMan.grid[bridge.Value.x, bridge.Value.y, bridge.Value.z].GetComponent<Block>();
                if (gridMan.CreateBridge(origin, destination) == null) { Logger.Warn("Could not replicate bridge from " + origin + ":(" + bridge.Key + ") to " + destination + ":(" + bridge.Value + ")"); };
            }

            foreach (KeyValuePair<Vector3Int, int> stored in saveData.storageGrid) {
                Vector3Int coords = stored.Key;
                BlockScheme block = GameManager.instance.library.GetBlockByID(stored.Value);
                if (block == null) { Logger.Warn("BlockScheme index not found - index : " + stored.Value.ToString()); return; }

                GameObject storedBuilding = Instantiate(GameManager.instance.library.blockPrefab);
                Block newBlockLink = storedBuilding.GetComponent<Block>();
                newBlockLink.block = block;

                GameManager.instance.storageBay.StoreAtPosition(storedBuilding, coords);
                //GameManager.instance.storageBay.PlaceBayIfPossible(saveData.storagePosition);
            }

            GameManager.instance.player.playerName = saveData.playerName;
            GameManager.instance.cityManagement.cityName = saveData.cityName;
            GameManager.instance.temporality.cycleNumber = saveData.cyclesPassed;
            GameManager.instance.temporality.cycleProgression = saveData.timeOfDay;
        }

        catch(Exception e) {
            Logger.Error("Could not load the savegame : " + e.Message+". Going back to the main menu instead.");
            DestroySave();
            GameManager.instance.ExitToMenu();
        }

    }

    ///
    ///
    //////////////////////////////


    public class GameData
    {
        public List<ShopDisplay> shoppingList;
        public GameObject[,,] grid;
        public List<GameObject> bridgesList;
        public Vector3Int storagePosition;
        public GameObject[,,] storedBlocks;
        public string playerName;
        public string cityName;
        public int cycleNumber;
        public float cycleProgression;

        public GameData(
            List<ShopDisplay> _shoppingList,
            GameObject[,,] _grid,
            List<GameObject> _bridgesList,
            Vector3Int _storagePosition,
            GameObject[,,] _storedBlocks,
            string _playerName,
            string _cityName,
            int _cycleNumber,
            float _cycleProgression
        )
        {
            shoppingList = _shoppingList;
            grid = _grid;
            bridgesList = _bridgesList;
            storagePosition = _storagePosition;
            storedBlocks = _storedBlocks;
            playerName = _playerName;
            cityName = _cityName;
            cycleNumber = _cycleNumber;
            cycleProgression = _cycleProgression;
        }
    }

    public class SaveData
    {
        public Vector3Int gridSize;
        public Vector3Int storageGridSize;
        public Vector3Int storagePosition;
        public Dictionary<Vector3Int, int> storageGrid;
        public Dictionary<Vector3Int, BlockSaveData> blockGrid;
        public List<KeyValuePair<Vector3Int,Vector3Int>> bridges;
        public Dictionary<int, int> command;

        public float timeOfDay;
        public int cyclesPassed;

        public string playerName;
        public string cityName;

        // Empty saveData
        public SaveData() { }
            
        // New saveData from game values
        public SaveData(GameData gameData)
        {
            // Storing command
            command = ConvertCommand(gameData.shoppingList);

            // Storing grid
            gridSize = new Vector3Int(gameData.grid.GetLength(0), gameData.grid.GetLength(1), gameData.grid.GetLength(2));
            blockGrid = ConvertBlocksGrid(gameData.grid, gridSize);

            // Storing bridges
            bridges = ConvertBridges(gameData.bridgesList);

            // Storing storage zone
            storageGridSize = new Vector3Int(gameData.storedBlocks.GetLength(0), gameData.storedBlocks.GetLength(1), gameData.storedBlocks.GetLength(2));
            storageGrid = ConvertStorageGrid(gameData.storedBlocks, storageGridSize);
            storagePosition = gameData.storagePosition;

            // Other data
            playerName = gameData.playerName;
            cityName = gameData.cityName;
            cyclesPassed = gameData.cycleNumber;
            timeOfDay = gameData.cycleProgression;
        }

        // Reading savedata from files
        public SaveData(
            Vector3Int _gridSize,
            Vector3Int _storageGridSize,
            Dictionary<Vector3Int, int> _storageGrid,
            Vector3Int _storagePosition,
            Dictionary<Vector3Int, BlockSaveData> _blockGrid,
            List<KeyValuePair<Vector3Int, Vector3Int>> _bridges,
            Dictionary<int, int> _command,
            float _timeOfDay,
            int _cyclesPassed,
            string _playerName,
            string _cityName
        )
        {
            _gridSize = gridSize;
            _storageGridSize = storageGridSize;
            _storageGrid = storageGrid;
            _storagePosition = storagePosition;
            _blockGrid = blockGrid;
            _bridges = bridges;
            _command = command;
            _timeOfDay = timeOfDay;
            _cyclesPassed = cyclesPassed;
            _playerName = playerName;
            _cityName = cityName;
        }


        Dictionary<Vector3Int, int> ConvertStorageGrid(GameObject[,,] _storedBlocks, Vector3Int gridSize)
        {
            Dictionary<Vector3Int, int> storageGrid = new Dictionary<Vector3Int, int>();
            foreach (GameObject building in _storedBlocks) {
                if (building != null) {
                    Block blockLink = building.GetComponent<Block>();
                    Vector3Int coords = blockLink.gridCoordinates;
                    int id = blockLink.block.ID;
                    storageGrid[coords] = id;
                }
            }

            return storageGrid;
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
                        blockData.id = blockLink.block.ID;
                        blockData.states = new List<int>();

                        foreach (BlockState state in blockLink.states) 
                        {
                            blockData.states.Add((int)state);
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

        Dictionary<int, int> ConvertCommand(List<ShopDisplay> _shoppingList)
        {
            Dictionary<int, int> command = new Dictionary<int, int>();
            foreach (ShopDisplay element in _shoppingList) {
                command.Add(element.myBlock.ID, element.quantityPicked);
            }
            return command;
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


