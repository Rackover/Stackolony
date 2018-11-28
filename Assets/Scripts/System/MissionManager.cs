using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour {


    // TO DO : LE SCRIPT DE PROPAGATION NE FONCTIONNE QUE DANS UN SEUL SENS POUR LES PONTS

    [Header("Lists")]
    public List<List<Coroutine>> activeExplorers; //Liste de toutes les listes d'explorers actifs, il y a une liste d'explorers pour chaque mission.
    public List<bool[,,]> exploredPositions; //Tableau tridimensionel booléen pour répértorier les positions déjà visitées par un explorer, pour chaque mission
    public List<List<BlockLink>> blocksFound; //Tableau de chaque block trouvé par les explorers, il y en a un pour chaque mission.
    public List<List<int>> blocksDistanceToCenter; //Tableau paralléle au tableau blocksFound, indiquant la distance de chaque bloc par rapport au point de départ de la mission

    [Header("References")]
    private GridManagement gridManager;
    private CursorManagement cursorManagement;


    private void Awake()
    {
        //Initialisation des listes
        activeExplorers = new List<List<Coroutine>>();
        exploredPositions = new List<bool[,,]>();
        blocksFound = new List<List<BlockLink>>();
        blocksDistanceToCenter = new List<List<int>>();


        //Recuperation du gridManager
        gridManager = FindObjectOfType<GridManagement>();
        cursorManagement = FindObjectOfType<CursorManagement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            StartMission(new Vector3Int(cursorManagement.posInTerrain.x, cursorManagement.posInTerrain.y, cursorManagement.posInTerrain.z), "test", 3);
        }
    }


    //Lance une nouvelle mission d'exploration, cela renverra une liste de blocks explorés et lancera la fonction callBack qui se trouve dans le CityManager.
    public void StartMission(Vector3Int position, string callBack, int range = 1, int power = 1)
    {
        //Genere une nouvelle ID de mission
        int missionID = activeExplorers.Count;

        //Genere les listes utilisées pour la mission
        activeExplorers.Add(new List<Coroutine>());
        blocksFound.Add(new List<BlockLink>());
        blocksDistanceToCenter.Add(new List<int>());

        //Genere le tableau tridimensionel booléen indiquant les coordonnées déjà explorées
        Debug.Log(gridManager.gridSize);
        exploredPositions.Add(new bool[gridManager.gridSize.x, gridManager.gridSize.y, gridManager.gridSize.z]);
        for (int x = 0; x < gridManager.gridSize.x; x++)
        {
            for (int y = 0; y < gridManager.gridSize.y; y++)
            {
                for (int z = 0; z < gridManager.gridSize.z; z++)
                {
                    exploredPositions[missionID][x, y, z] = false;
                }
            }
        }

        //Genere le premier explorer de la mission
        Debug.Log("STARTING MISSION " + missionID);
        activeExplorers[missionID].Add(StartCoroutine(SpawnExplorer(position, callBack, missionID, range, power,0)));
    }


    //Termine la mission et supprime les listes qui y étaient associées
    public void EndMission(int missionID)
    {
        activeExplorers.RemoveAt(missionID);
        exploredPositions.RemoveAt(missionID);
        blocksFound.RemoveAt(missionID);
        blocksDistanceToCenter.RemoveAt(missionID);
    }

    //Genere un explorer qui va vérifier les 6 directions possible, et envoyer un autre explorer pour explorer chaque chemin trouvé.
    IEnumerator SpawnExplorer(Vector3Int position, string callback, int missionID, int range, int power, int explorerID)
    {
        //Delai (à augmenter pour réduire le lag mais augmenter le temps de calcul)
        yield return new WaitForEndOfFrame();

        //L'explorer récupère les informations
        if (gridManager.grid[position.x, position.y, position.z].GetComponent<BlockLink>() != null)
        {
            blocksFound[missionID].Add(gridManager.grid[position.x, position.y, position.z].GetComponent<BlockLink>());
            blocksDistanceToCenter[missionID].Add(range);
            exploredPositions[missionID][position.x, position.y, position.z] = true;
        } else
        {
            Debug.LogWarning("Explorer found a block with no informations");
            yield return null;
        }

        //L'explorer analyse les 6 trajectoires possible pour essayer de créer un chemin
        List<BlockLink> AdjacentBlocks = CheckAdjacentBlocks(position, missionID);

        //L'explorer prend sa retraite
        activeExplorers[missionID].RemoveAt(0);

        //Si l'explorer a trouvé quelque chose, il forme de nouveaux explorers pour continuer d'explorer
        if (range > 0 && AdjacentBlocks.Count > 0) {
            for (int i = 0; i < AdjacentBlocks.Count; i++) {
                //Forme l'explorer relai qui transportera ses informations
                int newExplorerID = activeExplorers[missionID].Count;
                activeExplorers[missionID].Add(StartCoroutine(SpawnExplorer(AdjacentBlocks[i].gridCoordinates, callback, missionID, range - 1, power, newExplorerID)));
            }
        }


        //S'il n'y a plus aucun explorer actif, on termine la mission
        if (activeExplorers[missionID].Count == 0)
        {
            Debug.Log("MISSION FINISHED");
            foreach (BlockLink b in blocksFound[missionID])
            {
                Debug.Log(b);
            }
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
        blockFound = CheckBlock(posToCheck, missionID, false);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        //Check down
        posToCheck = new Vector3Int(position.x, position.y - 1, position.z);
        blockFound = CheckBlock(posToCheck, missionID, false);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        //Check sides
        posToCheck = new Vector3Int(position.x+1, position.y, position.z);
        blockFound = CheckBlock(posToCheck, missionID, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        posToCheck = new Vector3Int(position.x-1, position.y, position.z);
        blockFound = CheckBlock(posToCheck, missionID, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        posToCheck = new Vector3Int(position.x, position.y, position.z+1);
        blockFound = CheckBlock(posToCheck, missionID, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        posToCheck = new Vector3Int(position.x, position.y, position.z-1);
        blockFound = CheckBlock(posToCheck, missionID, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        return blocksFound;
    }


    //Renvoit un blockLink à une position donnée, si "onlyBridges" est à true, alors le systeme ne renverra que les blocs aux extrémités des ponts
    BlockLink CheckBlock(Vector3Int position, int missionID, bool onlyBridges)
    {
        BlockLink output = null;

        //Verifie que la position n'a jamais été explorée auparavant
        if (exploredPositions[missionID][position.x, position.y, position.z] != true)
        {
            GameObject blockFound = gridManager.grid[position.x, position.y, position.z];
            if (blockFound != null)
            {
                //Si l'objet trouvé est un pont, récupère le bloc à l'extrémité de ce pont
                if (blockFound.tag == "Bridge")
                {
                    Vector3Int bridgeDestinationPosition = blockFound.GetComponent<BridgeInfo>().destination;
                    output = gridManager.grid[bridgeDestinationPosition.x, bridgeDestinationPosition.y, bridgeDestinationPosition.z].GetComponent<BlockLink>();
                    exploredPositions[missionID][bridgeDestinationPosition.x, bridgeDestinationPosition.y, bridgeDestinationPosition.z] = true;
                }
                //Sinon, récupère simplement le bloc trouvé
                else
                {
                    if (onlyBridges == false)
                    output = blockFound.GetComponent<BlockLink>();
                }
            }
            //Ajout de la position explorée à la liste des positions explorées
            exploredPositions[missionID][position.x, position.y, position.z] = true;
        }

        return output;
    }
}
