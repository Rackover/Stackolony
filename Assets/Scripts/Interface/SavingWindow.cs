using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SavingWindow : MonoBehaviour {

    private void Start()
    {
        GameManager.instance.Save(delegate { Destroy(); });
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
