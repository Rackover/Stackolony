using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
#if (UNITY_EDITOR) 
public class BlockReader : EditorWindow 
{
	Vector2 scroll;
	TextAsset csvFile;
	bool preview;

	string folderName = "";

    [MenuItem("Window/BlockReader")]
    static void Init()
    {
        BlockReader br = (BlockReader)EditorWindow.GetWindow(typeof(BlockReader));
        br.Show();
    }

	void OnGUI()
    {
		GUI.color = Color.white;
		GUILayout.Label("Blocks CSV data reader", EditorStyles.boldLabel);

        csvFile = EditorGUILayout.ObjectField("Drop a CSV file here", csvFile, typeof(TextAsset), false) as TextAsset;
		if (csvFile != null)
        {
			GUILayout.BeginHorizontal();
			GUILayout.Label("Enter folder name :");

			if(folderName == "") GUI.color = Color.red;
			else GUI.color = Color.green;

			folderName = GUILayout.TextField(folderName);
			GUILayout.EndHorizontal();

			GUI.color = Color.white;

			GUILayout.BeginHorizontal();
			if(folderName != "")
			{
				if(GUILayout.Button("Read CSV and generate Blocks")) 
				{
					GenerateBlocks();
				}
			}
			else
			{
				GUI.color = Color.grey;
				GUILayout.Button("Read CSV and generate Blocks");
				GUI.color = Color.white;
			}

			if(preview)
			{
				if (GUILayout.Button("Hide datas")) 
				{
					preview = false;
				}
			}
			else
			{
				if (GUILayout.Button("Preview datas")) 
				{
					preview = true;
				}
			}
			GUILayout.EndHorizontal();

			if(preview)
			{
				GUI.color = Color.grey;
				EditorGUILayout.TextArea(csvFile.ToString());
			}				
        }
    }	

	private void GenerateBlocks()
    {
		string fullText = csvFile.text;
		
		string[] lineSeparators = new string[]{"\n", "\r", "\n\r", "\r\n"};
		char[] cellSeparators = new char[]{','};
		string[] lines = fullText.Split(lineSeparators, System.StringSplitOptions.RemoveEmptyEntries);
		
		List<string[]> completeExcelFile = new List<string[]>();

		for(int i = 0; i < lines.Length; i++)
		{
			string[] cells = lines[i].Split(cellSeparators);
			completeExcelFile.Add(cells);

			if( i > 0)
				SaveBlock(cells);
		}

	}

	void SaveBlock(string[] dataLine)
	{
		if(dataLine.Length == 12)
		{
			BlockScheme block = ScriptableObject.CreateInstance<BlockScheme>();

			// Ints
			block.ID = int.Parse(dataLine[1]);
			block.consumption = int.Parse(dataLine[2]);
			block.sensibility = int.Parse(dataLine[3]);

			// Flags
			block.flags = dataLine[4].Split(new char[]{'/'}, System.StringSplitOptions.RemoveEmptyEntries);

			// Booleans
			block.isMovable = bool.Parse(dataLine[5]);
			block.isDestroyable = bool.Parse(dataLine[6]);
			block.isBuyable = bool.Parse(dataLine[7]);
			block.canBuildAbove = bool.Parse(dataLine[8]);
			block.relyOnSpatioport = bool.Parse(dataLine[9]);
			block.fireProof = bool.Parse(dataLine[10]);
			block.riotProof = bool.Parse(dataLine[11]);

            string finalName = "Block_" + dataLine[0] + ".asset";

            // Link preservation
            UnityEngine.Object previousBlock = AssetDatabase.LoadAssetAtPath(Paths.GetBlockFolder(folderName) + "/" + finalName, typeof(BlockScheme));
            if (previousBlock != null) {
                BlockScheme previousScheme = (BlockScheme)previousBlock;
                block.model = previousScheme.model;
                block.sound = previousScheme.sound;
            }
            

            AssetDatabase.CreateAsset(block, Paths.GetBlockFolder(folderName) + "/" + finalName);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		else
		{
			Debug.LogWarning("Your file is not properly setup, there is only " + dataLine.Length + " elements in each lines. Where there should be 12");
		}
	}
}
#endif
