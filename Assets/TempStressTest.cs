using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempStressTest : MonoBehaviour {

public GameObject generatorPrefab;
public GameObject blockPrefab;
public GridManagement gridManagement;
public int range;
public int height;
public KeyCode launchTestKey;

	public void Update() {
		if (Input.GetKeyDown(launchTestKey)) {
			StartCoroutine(GenerateAll());
			//Invoke("MakeBridges",5);
		}
	}
	// Use this for initialization
	IEnumerator GenerateAll () {
		int count = 0;
		for (int z = 20; z <20+range; z++) {
			for (int x = 20; x <20+range ; x++) {
				for (int y = 0; y <height ; y++) {
					yield return new WaitForEndOfFrame();
					count++;
					if (count >= 10) {
						gridManagement.SpawnBlock(generatorPrefab, new Vector2Int(x,z));
						count = 0;
					} else {
						gridManagement.SpawnBlock(blockPrefab, new Vector2Int(x,z));
					}
				}
			}
		}
		yield return null;
	}

	void MakeBridges() {
		int count = 0;
		for (int x = 10; x < 20; x++) {
			for (int y = 0; y < 20; y++) {
				for (int z = 10; z < 20; z++) {
					count++;
					if (count >= 10) {
						gridManagement.CreateBridge(gridManagement.grid[x,y,z].GetComponent<BlockLink>(),gridManagement.grid[x,y,z+1].GetComponent<BlockLink>());
						count = 0;
					}
				}
			}
		}
	}
}
