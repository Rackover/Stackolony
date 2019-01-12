using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class BlockReader : MonoBehaviour {
/*
	Vector2 scroll;
	TextAsset csvFile;
	bool preview;

    [MenuItem("Window/BlockReader")]
    public static void Init()
    {
        BlockReader br = EditorWindow.GetWindow<BlockReader>();
        br.Show();
    }

	private void OnGUI()
    {
		GUILayout.Label("Blocks CSV data reader", EditorStyles.boldLabel);

        csvFile = EditorGUILayout.ObjectField("Drop a CSV file here", csvFile, typeof(TextAsset), false) as TextAsset;
		if (csvFile != null)
        {

			if(preview)
			{
				//scroll = EditorGUILayout.BeginScrollView(scroll);
				EditorGUILayout.TextArea(csvFile.ToString());
				//EditorGUILayout.EndScrollView();
			}

			GUILayout.BeginHorizontal();
            if (GUILayout.Button("Read CSV and generate Blocks")) 
			{
				GenerateQuests();
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
        }
    }	

	private void GenerateQuests()
    {
		string fullText = csvFile.text;
		
		string[] lineSeparators = new string[]{"\n", "\r", "\n\r", "\r\n"};
		char[] cellSeparators = new char[]{';'};
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
		BlockScheme block = ScriptableObject.CreateInstance<BlockScheme>();
		
		block.title = dataLine[0];
		block.level = int.Parse(dataLine[1]);
		block.consumption = int.Parse(dataLine[2]);
		block.complexity = int.Parse(dataLine[3]);
		block.description = dataLine[4];
		block.flags = dataLine[5].Split(new char[]{'/'}, System.StringSplitOptions.RemoveEmptyEntries);

		if(!Directory.Exists("Assets/Databank/Blocks")) Directory.CreateDirectory("Assets/Databank/Blocks");

		AssetDatabase.CreateAsset(block, "Assets/Databank/Blocks/Block_" + block.title + ".asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
    */
}
