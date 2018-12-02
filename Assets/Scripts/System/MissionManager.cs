using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour {

    public class Mission
    {
        public List<Coroutine> activeExplorers; //Liste de toutes les listes d'explorers actifs, il y a une liste d'explorers pour chaque mission.
        public bool[,,] exploredPositions;      //Tableau tridimensionel booléen pour répértorier les positions déjà visitées par un explorer, pour chaque mission
        public List<BlockLink> blocksFound;      //Tableau de chaque block trouvé par les explorers, il y en a un pour chaque mission.
        public List<int> blockDistanceToCenter; //Tableau paralléle au tableau blocksFound, indiquant la distance de chaque bloc par rapport au point de départ de la mission
        public int power;
        public int range;
    }

    [Header("Lists")]
    public List<Mission> missionList;

    [Header("References")]
    private GridManagement gridManager;
    private CursorManagement cursorManagement;
    private CityManagement cityManager;

    [Header("Debug")]
    public GameObject explorerPrefab;


    private void Awake()
    {
        //Initialisation des listes
        missionList = new List<Mission>();

        //Recuperation du gridManager
        gridManager = FindObjectOfType<GridManagement>();
        cursorManagement = FindObjectOfType<CursorManagement>();
        cityManager = FindObjectOfType<CityManagement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            StartMission(new Vector3Int(cursorManagement.posInTerrain.x, cursorManagement.posInTerrain.y, cursorManagement.posInTerrain.z), "EmitEnergy", 3, 3);
        }
    }

    public void PrepareNewMission()
    {
        Mission newMission = new Mission();
        newMission.activeExplorers = new List<Coroutine>();
        newMission.blocksFound = new List<BlockLink>();
        newMission.blockDistanceToCenter = new List<int>();
        missionList.Add(newMission);
    }


    //Lance une nouvelle mission d'exploration, cela renverra une liste de blocks explorés et lancera la fonction callBack qui se trouve dans le CityManager.
    public void StartMission(Vector3Int position, string callBack, int range = 1, int power = 0)
    {
        PrepareNewMission();

        //Genere une nouvelle ID de mission
        int missionID = missionList.Count-1;
        Mission activeMission = missionList[missionID];
        activeMission.power = power;
        activeMission.range = range;


        //Genere le tableau tridimensionel booléen indiquant les coordonnées déjà explorées
        activeMission.exploredPositions = (new bool[gridManager.gridSize.x, gridManager.gridSize.y, gridManager.gridSize.z]);
        for (int x = 0; x < gridManager.gridSize.x; x++)
        {
            for (int y = 0; y < gridManager.gridSize.y; y++)
            {
                for (int z = 0; z < gridManager.gridSize.z; z++)
                {
                    activeMission.exploredPositions[x, y, z] = false;
                }
            }
        }

        //Genere le premier explorer de la mission
        activeMission.activeExplorers.Add(StartCoroutine(SpawnExplorer(position, callBack, missionID, range, power,0)));
    }


    //Termine la mission et supprime les listes qui y étaient associées
    public void EndMission(int missionID)
    {
        missionList.RemoveAt(missionID);
    }

    //Genere un explorer qui va vérifier les 6 directions possible, et envoyer un autre explorer pour explorer chaque chemin trouvé.
    IEnumerator SpawnExplorer(Vector3Int position, string callback, int missionID, int range, int power, int explorerID)
    {
        //Delai (à augmenter pour réduire le lag mais augmenter le temps de calcul)
        yield return new WaitForEndOfFrame();

        //L'explorer recupère sa mission
        Mission myMission = missionList[missionID];

        GameObject explorerVisuals = null;

        //L'explorer récupère les informations
        if (gridManager.grid[position.x, position.y, position.z] != null)
        {

            explorerVisuals = Instantiate(explorerPrefab);
            explorerVisuals.transform.position = gridManager.grid[position.x, position.y, position.z].transform.position;

            if (gridManager.grid[position.x, position.y, position.z].GetComponent<BlockLink>() != null)
            {
                myMission.blocksFound.Add(gridManager.grid[position.x, position.y, position.z].GetComponent<BlockLink>());
                myMission.blockDistanceToCenter.Add(range);
                myMission.exploredPositions[position.x, position.y, position.z] = true;
            }
            else
            {
                Debug.LogWarning("Explorer found a block with no informations");
                yield return null;
            }
        }

        //L'explorer analyse les 6 trajectoires possible pour essayer de créer un chemin
        List<BlockLink> AdjacentBlocks = CheckAdjacentBlocks(position, missionID);

        //L'explorer prend sa retraite
        myMission.activeExplorers.RemoveAt(0);
        if (explorerVisuals != null)
        {
            Destroy(explorerVisuals,1);
        }

        //Si l'explorer a trouvé quelque chose, il forme de nouveaux explorers pour continuer d'explorer
        if (AdjacentBlocks.Count > 0) {

            //Si je cherche les  blocs dans une certaine range, alors je cherche le bloc suivant et je diminue la range
            if (range > 0)
            {
                for (int i = 0; i < AdjacentBlocks.Count; i++)
                {
                    //Forme l'explorer relai qui transportera ses informations
                    int newExplorerID = myMission.activeExplorers.Count;
                    myMission.activeExplorers.Add(StartCoroutine(SpawnExplorer(AdjacentBlocks[i].gridCoordinates, callback, missionID, range - 1, power, newExplorerID)));
                }
            }
            //Sinon, si je cherche des blocs dans une portée infinie jusqu'à ce que je n'ai plus de power à donner, alors je continue ma recherche
            else if (range == -1 && power > 0)
            {
                foreach (BlockLink foundBlock in AdjacentBlocks)
                {
                    power -= (foundBlock.myBlock.consumption - foundBlock.currentPower);
                }

                for (int i = 0; i < AdjacentBlocks.Count; i++)
                {
                    //Forme l'explorer relai qui transportera ses informations
                    int newExplorerID = myMission.activeExplorers.Count;
                    myMission.activeExplorers.Add(StartCoroutine(SpawnExplorer(AdjacentBlocks[i].gridCoordinates, callback, missionID, range, power, newExplorerID)));
                }
            }
        }


        //S'il n'y a plus aucun explorer actif, on termine la mission
        if (myMission.activeExplorers.Count == 0)
        {
            cityManager.missionID = missionID;
            cityManager.StartCoroutine(callback);
        }



        yield return null;
    }

    //Renvoit une liste des blockLink adjacents à une position
    List<BlockLink> CheckAdjacentBlocks(Vector3Int position, int missionID)
    {
        List<BlockLink> blocksFound = new List<BlockLink>();

        Vector3Int posToCheck = Vector3Int.zero;
        BlockLink blockFound = null;

        //Check up
        posToCheck = new Vector3Int(position.x, position.y + 1, position.z);
        blockFound = CheckBlock(position, posToCheck, missionID, false);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        //Check down
        posToCheck = new Vector3Int(position.x, position.y - 1, position.z);
        blockFound = CheckBlock(position, posToCheck, missionID, false);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        //Check sides
        posToCheck = new Vector3Int(position.x+1, position.y, position.z);
        blockFound = CheckBlock(position, posToCheck, missionID, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        posToCheck = new Vector3Int(position.x-1, position.y, position.z);
        blockFound = CheckBlock(position, posToCheck, missionID, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        posToCheck = new Vector3Int(position.x, position.y, position.z+1);
        blockFound = CheckBlock(position, posToCheck, missionID, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        posToCheck = new Vector3Int(position.x, position.y, position.z-1);
        blockFound = CheckBlock(position, posToCheck, missionID, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        return blocksFound;
    }


    //Renvoit un blockLink à une position donnée, si "onlyBridges" est à true, alors le systeme ne renverra que les blocs aux extrémités des ponts
    BlockLink CheckBlock(Vector3Int initialPos, Vector3Int position, int missionID, bool onlyBridges)
    {
        BlockLink output = null;

        Mission myMission = missionList[missionID];

        //Verifie que la position n'a jamais été explorée auparavant
        if (myMission.exploredPositions[position.x, position.y, position.z] != true)
        {
            GameObject blockFound = gridManager.grid[position.x, position.y, position.z];
            if (blockFound != null)
            {
                //Si l'objet trouvé est un pont, récupère le bloc à l'extrémité de ce pont
                if (blockFound.tag == "Bridge")
                {
                    Vector3Int bridgeDestinationPosition = Vector3Int.zero;
                    if (blockFound.GetComponent<BridgeInfo>().destination == initialPos)
                    {
                        bridgeDestinationPosition = blockFound.GetComponent<BridgeInfo>().origin;
                    }
                    else
                    {
                        bridgeDestinationPosition = blockFound.GetComponent<BridgeInfo>().destination;
                    }
                    output = gridManager.grid[bridgeDestinationPosition.x, bridgeDestinationPosition.y, bridgeDestinationPosition.z].GetComponent<BlockLink>();
                    myMission.exploredPositions[bridgeDestinationPosition.x, bridgeDestinationPosition.y, bridgeDestinationPosition.z] = true;
                }
                //Sinon, récupère simplement le bloc trouvé
                else
                {
                    if (onlyBridges == false)
                    output = blockFound.GetComponent<BlockLink>();
                }
            }
            //Ajout de la position explorée à la liste des positions explorées
            myMission.exploredPositions[position.x, position.y, position.z] = true;
        }

        return output;
    }
}
