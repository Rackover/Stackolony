using UnityEngine;
public class Spawn : MonoBehaviour {
    private void Start()
    {
        transform.position =
                GameManager.instance.gridManagement.IndexToWorldPosition(
                    GameManager.instance.gridManagement.WorldPositionToIndex(
                        transform.position
                   )
                );
           
    }



}