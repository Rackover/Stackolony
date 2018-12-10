using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class SaveManager : MonoBehaviour {

    public int saveVersion = 1;

    ///////////////////////////////
    /// 
    ///     Main function to write savegame
    /// 
    ///

    public void WriteSaveData(SaveData saveData, string saveName = "city.bin")
    {
        // Step 0 - Initialize writer
        Writer writer = new Writer();
        writer.OpenSave(saveName);

        // Step 1 - Miscellaneous data
        writer.WriteUInt8(saveVersion);
        writer.WriteString(saveData.cityName);
        writer.WriteString(saveData.playerName);
        writer.WriteFloat(saveData.timeOfDay);
        writer.WriteUInt16(saveData.cyclesPassed);

        // Step 2 - Main grid
        writer.WriteVector3UInt8(saveData.gridSize);
        foreach (BlockSaveData blockData in saveData.blockGrid) {
            BlockSaveData data = blockData ?? new BlockSaveData();
            writer.WriteUInt8(data.id);
            writer.WriteUInt8(data.states.Count);
            foreach (int state in data.states) {
                writer.WriteUInt8(state);
            }
        }

        // Step 3 - Storage bay grid
        writer.WriteVector3UInt8(saveData.storageGridSize);
        foreach (int blockId in saveData.storageGrid) {
            writer.WriteUInt8(blockId);
        }

        // Step 4 - Bridges
        writer.WriteUInt8(saveData.bridges.Count);
        foreach (KeyValuePair<Vector3Int, Vector3Int> bridge in saveData.bridges) {
            writer.WriteVector3UInt8(bridge.Key);
            writer.WriteVector3UInt8(bridge.Value);
        }

        // Step 5 - Command
        writer.WriteUInt8(saveData.command.Count);
        foreach (KeyValuePair<int, int> commandElement in saveData.command) {
            writer.WriteUInt8(commandElement.Key);
            writer.WriteUInt8(commandElement.Value);
        }

        // Last step - Close handler;
        writer.Close();
    }

    ///
    ///
    //////////////////////////////


    public class SaveData
    {
        public Vector3Int gridSize;
        public Vector3Int storageGridSize;
        public int[,,] storageGrid;
        public BlockSaveData[,,] blockGrid;
        public List<KeyValuePair<Vector3Int,Vector3Int>> bridges;
        public Dictionary<int, int> command;

        public float timeOfDay;
        public int cyclesPassed;

        public string playerName;
        public string cityName;

        // New saveData instantiation
        public SaveData(
            List<ShopDisplay> _shoppingList,
            GameObject[,,] _grid,
            List<GameObject> _bridgesList,
            GameObject[,,] _storedBlocks,
            string _playerName,
            string _cityName,
            int _cycleNumber,
            float _cycleProgression
        )
        {
            // Storing command
            command = ConvertCommand(_shoppingList);

            // Storing grid
            gridSize = new Vector3Int(_grid.GetLength(0), _grid.GetLength(1), _grid.GetLength(2));
            blockGrid = ConvertBlocksGrid(_grid, gridSize);

            // Storing bridges
            bridges = ConvertBridges(_bridgesList);

            // Storing storage zone
            storageGridSize = new Vector3Int(_storedBlocks.GetLength(0), _storedBlocks.GetLength(1), _storedBlocks.GetLength(2));
            storageGrid = ConvertStorageGrid(_storedBlocks, storageGridSize);

            // Other data
            playerName = _playerName;
            cityName = _cityName;
            cyclesPassed = _cycleNumber;
            timeOfDay = _cycleProgression;

        }
    }

    public class BlockSaveData
    {
        public int id = 0;
        public List<int> states = new List<int>();
    }

    class Reader
    {
        private BinaryReader br;

        bool OpenSave(string saveName = "city.bin")
        {
            string fullPath = Application.persistentDataPath + "/" + saveName;
            try {
                br = new BinaryReader(new FileStream(fullPath, FileMode.Open));
                return true;
            }
            catch (IOException e) {
                Debug.LogWarning(e.Message + "\n Cannot read file "+ fullPath);
                return false;
            }
        }

        int ReadUInt16() { try { return br.ReadUInt16(); } catch (IOException e) { Debug.LogWarning(e.Message + "\n Cannot read integer from file."); return 0;  } }
        int ReadUInt8() { try { return br.ReadByte(); } catch (IOException e) { Debug.LogWarning(e.Message + "\n Cannot read uint8 from file."); return 0; } }
        string ReadString() { try { return br.ReadString(); } catch (IOException e) { Debug.LogWarning(e.Message + "\n Cannot read string from file."); return null; } }
        bool ReadBool() { try { return br.ReadBoolean(); } catch (IOException e) { Debug.LogWarning(e.Message + "\n Cannot read bool from file."); return false; } }
        float ReadFloat() { try { return (float)br.ReadDouble(); } catch (IOException e) { Debug.LogWarning(e.Message + "\n Cannot read float from file."); return 0f; } }

        void Close()
        {
            br.Close();
        }
    }

    class Writer
    {
        BinaryWriter bw;

        public bool OpenSave(string saveName = "city.bin")
        {
            string fullPath = Application.persistentDataPath + "/" + saveName;
            Debug.Log(fullPath);
            try {
                bw = new BinaryWriter(new FileStream(fullPath, FileMode.Create));
                return true;
            }
            catch (IOException e) {
                Debug.LogWarning(e.Message + "\n Cannot create file "+ fullPath);
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
                Debug.LogWarning(e.Message + "\n Cannot write " + val.ToString() + " to file.");
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
                Debug.LogWarning(e.Message + "\n Cannot write " + val.ToString() + " to file.");
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
                Debug.LogWarning(e.Message + "\n Cannot write " + val.ToString() + " to file.");
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
                Debug.LogWarning(e.Message + "\n Cannot write " + val.ToString() + " to file.");
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
                Debug.LogWarning(e.Message + "\n Cannot write " + val.ToString() + " to file.");
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


    ///////////////////////////////
    /// 
    ///     Utility functions
    /// 
    ///

    static int[,,] ConvertStorageGrid(GameObject[,,] _storedBlocks, Vector3Int gridSize)
    {
        int[,,] storageGrid = new int[gridSize.x, gridSize.y, gridSize.z];
        foreach (GameObject building in _storedBlocks) {
            if (building != null) {
                BlockLink blockLink = building.GetComponent<BlockLink>();
                Vector3Int coords = blockLink.gridCoordinates;
                int id = 1;
                /*  WAITING FOR RENAME // int id = blockLink.block.id; */
                storageGrid[coords.x, coords.y, coords.z] = id;
            }
        }

        return storageGrid;
    }


    static BlockSaveData[,,] ConvertBlocksGrid(GameObject[,,] _grid, Vector3Int gridSize)
    {
        BlockSaveData[,,] blockGrid = new BlockSaveData[gridSize.x, gridSize.y, gridSize.z];

        foreach (GameObject building in _grid) {
            BlockSaveData blockData = new BlockSaveData();
            if (building != null) {
                BlockLink blockLink = building.GetComponent<BlockLink>();
                Vector3Int coords = blockLink.gridCoordinates;
                blockData.id = 1;
                /*  WAITING FOR RENAME // blockData.id = blockLink.block.id; */
                blockData.states = new List<int>();
                foreach (BlockState state in blockLink.states) {
                    blockData.states.Add((int)state);
                }
                blockGrid[coords.x, coords.y, coords.z] = blockData;
            }
        }

        return blockGrid;
    }

    static List<KeyValuePair<Vector3Int, Vector3Int>> ConvertBridges(List<GameObject> _bridgesList)
    {
        List<KeyValuePair<Vector3Int, Vector3Int>> list = new List<KeyValuePair<Vector3Int, Vector3Int>>();
        foreach (GameObject bridge in _bridgesList) {
            BridgeInfo info = bridge.GetComponent<BridgeInfo>();
            KeyValuePair<Vector3Int, Vector3Int> pair = new KeyValuePair<Vector3Int, Vector3Int>(info.origin, info.destination);
            list.Add(pair);
        }
        return list;
    }

    static Dictionary<int, int> ConvertCommand(List<ShopDisplay> _shoppingList)
    {
        Dictionary<int, int> command = new Dictionary<int, int>();
        foreach (ShopDisplay element in _shoppingList) {
            /* WAITING FOR RENAME // command.Add(element.block.id, element.quantityPicked); */
            command.Add(1, element.quantityPicked);
        }
        return command;
    }







    // Debug
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)){
            WriteSaveData(
                new SaveData(
                    // (Fake savegame for now)
                    new List<ShopDisplay>(), new GameObject[4, 4, 4], new List<GameObject>(), new GameObject[4, 4, 4], "Salut", "SalutVille", 3, 0.2f
                )
            );
        }
    }
}   
