using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FONCTIONNEMENT DU SYSTEME :
/// A chaque interaction avec un bloc lié à l'energie, le jeu recalcule entierement les transmissions de courant :
/// Cela signifie que les blocs ayant besoin de courant se désalimentent tous et que les blocs generateurs vont lancer leurs missions pour
/// emettre leurs explorateurs les uns après les autres.
/// Deux missions ne peuvent donc jamais avoir lieu en même temps.
/// Une fois qu'une mission est terminée, elle lance une fonction callback. Si cette fonction callback est liée à des coroutines, alors elle s'effectue, et seulement après
/// la mission sera marquée comme terminée.
/// 
/// PROBLEMES ACTUELS :
/// Lors de la chute d'un generateur, il generere 2 fois du courant
/// 
/// </summary>


public class MissionManager : MonoBehaviour {

    public bool ShowExplorers; //Affiche les explorers en leur créant un gameObject pour les visualiser plus facilement
    [System.Serializable]
    public class Mission
    {
        public List<Coroutine> activeExplorers; //Liste de toutes les listes d'explorers actifs, il y a une liste d'explorers pour chaque mission.
        public List<Vector3Int> exploredPositions;
        public List<Block> blocksFound;      //Tableau de chaque block trouvé par les explorers, il y en a un pour chaque mission.
        public List<int> blockDistanceToCenter; //Tableau paralléle au tableau blocksFound, indiquant la distance de chaque bloc par rapport au point de départ de la mission
        public int power;
        public int range;
        public string callBack;
        public Vector3Int position;
        public int explorerCount;
        public System.Type flagToFind;
    }
    [Header("Lists")]
    public List<Mission> missionList = new List<Mission>();
    public List<int> IDList;
    [Header("Debug")]
    public GameObject explorerPrefab;
    
    public Mission PrepareNewMission()
    {
        Mission newMission = new Mission();
        newMission.activeExplorers = new List<Coroutine>();
        newMission.blocksFound = new List<Block>();
        newMission.blockDistanceToCenter = new List<int>();
        newMission.exploredPositions = new List<Vector3Int>();
        missionList.Add(newMission);
        return newMission;
    }


    //Lance une nouvelle mission d'exploration, cela renverra une liste de blocks explorés et lancera la fonction callBack qui se trouve dans le CityManager.
    public void StartMission(Vector3Int position, string callBack, int range = 1, int power = 0, System.Type flagToFind = null)
    {
        Mission newMission = PrepareNewMission();
        newMission.position = position;
        newMission.callBack = callBack;
        newMission.range = range;
        newMission.power = power;
        newMission.flagToFind = flagToFind;

        //Si la mission est la premiere dans la liste, elle se lance, sinon elle attend son tour
        if (missionList.Count == 1)
        {
            StartCoroutine(StartMissionCoroutine(newMission));
        }
    }

    public IEnumerator StartMissionCoroutine(Mission mission)
    {
        //Genere le premier explorer de la mission
        mission.activeExplorers.Add(StartCoroutine(SpawnExplorer(mission.position, mission.callBack, mission, mission.range, mission.power, 0)));
        yield return null;
    }


    //Termine la mission et supprime les listes qui y étaient associées
    public void EndMission(Mission myMission)
    {
        missionList.Remove(myMission);
        if (missionList.Count > 0)
        {
            StartCoroutine(StartMissionCoroutine(missionList[0]));
        }
    }

    //Genere un explorer qui va vérifier les 6 directions possible, et envoyer un autre explorer pour explorer chaque chemin trouvé.
    IEnumerator SpawnExplorer(Vector3Int position, string callback, Mission myMission, int range, int power, int explorerID)
    {
        //Delai (à augmenter pour réduire le lag mais augmenter le temps de calcul)
        yield return new WaitForEndOfFrame();

        GameObject explorerVisuals = null;

        //L'explorer récupère les informations
        if (GameManager.instance.gridManagement.grid[position.x, position.y, position.z] != null)
        {
            if (ShowExplorers)
            {
                explorerVisuals = Instantiate(explorerPrefab);
                explorerVisuals.transform.position = GameManager.instance.gridManagement.grid[position.x, position.y, position.z].transform.position;
            }

            if (myMission.flagToFind == null)
            {
                if (GameManager.instance.gridManagement.grid[position.x, position.y, position.z].GetComponent<Block>() != null)
                {
                    myMission.blocksFound.Add(GameManager.instance.gridManagement.grid[position.x, position.y, position.z].GetComponent<Block>());
                    myMission.blockDistanceToCenter.Add(myMission.range - range);
                    myMission.exploredPositions.Add(position);
                }
                else
                {
                    Debug.LogWarning("Explorer found a block with no informations");
                    yield return null;
                }
            } else
            {
                Block foundBlock = GameManager.instance.gridManagement.grid[position.x, position.y, position.z].GetComponent<Block>();
                if (foundBlock != null) {
                    myMission.exploredPositions.Add(position);
                    foreach (Flag activeFlag in foundBlock.activeFlags) {
                        if (activeFlag.GetType() == myMission.flagToFind) {
                                myMission.blocksFound.Add(GameManager.instance.gridManagement.grid[position.x, position.y, position.z].GetComponent<Block>());
                                myMission.blockDistanceToCenter.Add(myMission.range - range);
                        }
                    }
                }
            }
        }

        //L'explorer analyse les 6 trajectoires possible pour essayer de créer un chemin
        List<Block> AdjacentBlocks = CheckAdjacentBlocks(position, myMission);

        //L'explorer prend sa retraite
        myMission.activeExplorers.RemoveAt(0);
        if (explorerVisuals != null)
        {
            Destroy(explorerVisuals,1);
        }

        //Si l'explorer a trouvé quelque chose, il forme de nouveaux explorers pour continuer d'explorer
        if (AdjacentBlocks.Count > 0) {
            //Si je cherche les  blocs dans une certaine range, alors je cherche le bloc suivant et je diminue la range
            if (myMission.range > 0)
            {
                for (int i = 0; i < AdjacentBlocks.Count; i++)
                {
                    //Forme l'explorer relai qui transportera ses informations
                    int newExplorerID = myMission.activeExplorers.Count;
                    
                    if (range > 0)
                    {
                        myMission.activeExplorers.Add(StartCoroutine(SpawnExplorer(AdjacentBlocks[i].gridCoordinates, callback, myMission, range-1, power, newExplorerID)));
                    }
                }
            }
            //Sinon, si je cherche des blocs dans une portée infinie jusqu'à ce que je n'ai plus de power à donner, alors je continue ma recherche
            else if (myMission.range <= -1)
            {
                if (power > 0)
                {
                    foreach (Block foundBlock in AdjacentBlocks)
                    {
                        power -= (foundBlock.GetConsumption() - foundBlock.currentPower);
                    }

                    for (int i = 0; i < AdjacentBlocks.Count; i++)
                    {
                        //Forme l'explorer relai qui transportera ses informations
                        int newExplorerID = myMission.activeExplorers.Count;
                        myMission.activeExplorers.Add(StartCoroutine(SpawnExplorer(AdjacentBlocks[i].gridCoordinates, callback, myMission, range, power, newExplorerID)));
                    }
                } else if (myMission.flagToFind != null)
                {
                    foreach (Block foundBlock in AdjacentBlocks)
                    {
                        foreach (Flag flag in foundBlock.activeFlags)
                        {
                            if (flag.GetType() == myMission.flagToFind)
                            {
                                //Forme l'explorer relai qui transportera ses informations
                                int newExplorerID = myMission.activeExplorers.Count;
                                if (range > 0)
                                myMission.activeExplorers.Add(StartCoroutine(SpawnExplorer(foundBlock.gridCoordinates, callback, myMission, range-1, power, newExplorerID)));
                            }
                        }
                    }
                } else if (power <= -1)
                {
                    for (int i = 0; i < AdjacentBlocks.Count; i++)
                    {
                        //Forme l'explorer relai qui transportera ses informations
                        int newExplorerID = myMission.activeExplorers.Count;
                        myMission.activeExplorers.Add(StartCoroutine(SpawnExplorer(AdjacentBlocks[i].gridCoordinates, callback, myMission, range, power, newExplorerID)));
                    }
                }
            }
        }

        //S'il n'y a plus aucun explorer actif, on termine la mission
        myMission.explorerCount = myMission.activeExplorers.Count;
        if (myMission.activeExplorers.Count == 0)
        {
            foreach (Coroutine c in myMission.activeExplorers)
            {
                StopCoroutine(c);
            }
            if (myMission.blocksFound.Count > 0)
            {
                GameManager.instance.missionCallbackManager.mission = myMission;
                if(callback != "") GameManager.instance.missionCallbackManager.StartCoroutine(callback);
            }
            else
            {
                EndMission(myMission);
            }
        }
        yield return null;
    }

    //Renvoit une liste des blockLink adjacents à une position
    public List<Block> CheckAdjacentBlocks(Vector3Int position, Mission myMission)
    {
        List<Block> blocksFound = new List<Block>();

        Vector3Int posToCheck = Vector3Int.zero;
        Block blockFound = null;

        //Check up
        posToCheck = new Vector3Int(position.x, position.y + 1, position.z);
        blockFound = CheckBlock(position, posToCheck, myMission, false);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        //Check down
        posToCheck = new Vector3Int(position.x, position.y - 1, position.z);
        blockFound = CheckBlock(position, posToCheck, myMission, false);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        //Check sides
        posToCheck = new Vector3Int(position.x+1, position.y, position.z);
        blockFound = CheckBlock(position, posToCheck, myMission, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        posToCheck = new Vector3Int(position.x-1, position.y, position.z);
        blockFound = CheckBlock(position, posToCheck, myMission, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        posToCheck = new Vector3Int(position.x, position.y, position.z+1);
        blockFound = CheckBlock(position, posToCheck, myMission, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        posToCheck = new Vector3Int(position.x, position.y, position.z-1);
        blockFound = CheckBlock(position, posToCheck, myMission, true);
        if (blockFound != null)
        {
            blocksFound.Add(blockFound);
        }

        return blocksFound;
    }

    //Renvoit un blockLink à une position donnée, si "onlyBridges" est à true, alors le systeme ne renverra que les blocs aux extrémités des ponts
    Block CheckBlock(Vector3Int initialPos, Vector3Int position, Mission myMission, bool onlyBridges)
    {
        Block output = null;

        //Verifie que la position n'a jamais été explorée auparavant
        if (!myMission.exploredPositions.Contains(position))
        {
            GameObject blockFound;
            try {
                blockFound = GameManager.instance.gridManagement.grid[position.x, position.y, position.z];
            }
            catch(System.IndexOutOfRangeException e) {
                Debug.LogWarning("ERROR in Checkblock => mission exploration" + e.Message);
                Debug.LogWarning(e);
                return null;
            }

            if (blockFound != null)
            {
                //Si l'objet trouvé est un pont, récupère le bloc à l'extrémité de ce pont
                if (blockFound.tag == "Bridge")
                {
                    foreach (Vector3Int pos in blockFound.GetComponent<BridgeInfo>().allBridgePositions)
                    {
                        myMission.exploredPositions.Add(pos);
                    }
                    Vector3Int bridgeDestinationPosition = Vector3Int.zero;
                    if (blockFound.GetComponent<BridgeInfo>().destination == initialPos)
                    {
                        bridgeDestinationPosition = blockFound.GetComponent<BridgeInfo>().origin;
                    }
                    else
                    {
                        bridgeDestinationPosition = blockFound.GetComponent<BridgeInfo>().destination;
                    }
                    if (GameManager.instance.gridManagement.grid[bridgeDestinationPosition.x, bridgeDestinationPosition.y, bridgeDestinationPosition.z] != null)
                    {
                        output = GameManager.instance.gridManagement.grid[bridgeDestinationPosition.x, bridgeDestinationPosition.y, bridgeDestinationPosition.z].GetComponent<Block>();
                        myMission.exploredPositions.Add(bridgeDestinationPosition);
                    }
                }
                //Sinon, récupère simplement le bloc trouvé
                else
                {
                    if (onlyBridges == false)
                    {
                        output = blockFound.GetComponent<Block>();
                    } else
                    {
                        Block foundBlockLink = blockFound.GetComponent<Block>();
                        if (foundBlockLink != null)
                        {
                            if (foundBlockLink.bridge != null && foundBlockLink.bridge.GetComponent<BridgeInfo>().destination == initialPos) 
                            {
                                output = blockFound.GetComponent<Block>();
                                myMission.exploredPositions.Add(output.gridCoordinates);
                                foreach (Vector3Int pos in foundBlockLink.bridge.GetComponent<BridgeInfo>().allBridgePositions)
                                {
                                    myMission.exploredPositions.Add(pos);
                                }
                            } else
                            {
                                GameObject initialBlock = GameManager.instance.gridManagement.grid[initialPos.x, initialPos.y, initialPos.z];
                                if (initialBlock != null)
                                {
                                    Block initialBlockLink = initialBlock.GetComponent<Block>();
                                    if (initialBlock.GetComponent<Block>() != null)
                                    {
                                        if (initialBlockLink.bridge != null)
                                        {
                                            Vector3Int foundBlockPosition = initialBlockLink.bridge.GetComponent<BridgeInfo>().destination;
                                            output = GameManager.instance.gridManagement.grid[foundBlockPosition.x, foundBlockPosition.y, foundBlockPosition.z].GetComponent<Block>();
                                            foreach (Vector3Int pos in initialBlockLink.bridge.GetComponent<BridgeInfo>().allBridgePositions)
                                            {
                                                myMission.exploredPositions.Add(pos);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Ajout de la position explorée à la liste des positions explorées
            if (onlyBridges == false)
            {
                myMission.exploredPositions.Add(position);
            }
        }
        return output;
    }
}
